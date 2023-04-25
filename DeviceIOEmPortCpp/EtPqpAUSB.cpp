#include "stdafx.h"
#include "EtPqpAUSB.h"

//#include "vars.h"
#include <Setupapi.h> // !!! Setupapi.lib must be linked !!!
#include "cyioctl.h"
#include <winioctl.h>

//namespace DeviceIO
//{

DWORD EtPqpAUSB::connectStatus_ = CONNECTSTATUS_DISCONNECTED;
HANDLE EtPqpAUSB::hUsbDevice_ = INVALID_HANDLE_VALUE;

bool EtPqpAUSB::Open()
{
	try
	{
		EmService::WriteToLogGeneral("EtPqpAUSB Open");

		ResetEvent(hEventDisconnected_);

		HDEVINFO hwDeviceInfo = SetupDiGetClassDevs ( (LPGUID) &CYUSBDRV_GUID, 
													  NULL, 
													  NULL, 
													  DIGCF_PRESENT|DIGCF_INTERFACEDEVICE); 

		SP_DEVINFO_DATA devInfoData; 
		SP_DEVICE_INTERFACE_DATA  devInterfaceData; 
		PSP_INTERFACE_DEVICE_DETAIL_DATA functionClassDeviceData; 
		int deviceNumber = 0;
		DWORD requiredLength = 0; 

		if (hwDeviceInfo != INVALID_HANDLE_VALUE)
		{ 
			devInterfaceData.cbSize = sizeof(devInterfaceData); 

			if (SetupDiEnumDeviceInterfaces ( hwDeviceInfo, 0, (LPGUID) &CYUSBDRV_GUID, deviceNumber, &devInterfaceData))
			{ 
				SetupDiGetInterfaceDeviceDetail ( hwDeviceInfo, &devInterfaceData, NULL, 0, &requiredLength, NULL); 

				ULONG predictedLength = requiredLength; 

				functionClassDeviceData = (PSP_INTERFACE_DEVICE_DETAIL_DATA) malloc (predictedLength); 
				functionClassDeviceData->cbSize = sizeof (SP_INTERFACE_DEVICE_DETAIL_DATA); 

				devInfoData.cbSize = sizeof(devInfoData); 

				if (SetupDiGetInterfaceDeviceDetail (hwDeviceInfo, &devInterfaceData, functionClassDeviceData, predictedLength, &requiredLength, &devInfoData))
				{
					hUsbDevice_ = CreateFile (
									functionClassDeviceData->DevicePath, 
									GENERIC_WRITE | GENERIC_READ, 
									FILE_SHARE_WRITE | FILE_SHARE_READ, 
									NULL, 
									OPEN_EXISTING, 
									FILE_FLAG_OVERLAPPED, 
									NULL); 

					free(functionClassDeviceData); 
					SetupDiDestroyDeviceInfoList(hwDeviceInfo);

					InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_CONNECTED);
					
					rxThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(RxThreadStart), this, 0, NULL);
					return true;
				}
				else
				{
					InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FAILED);
					Sleep(1000);
					InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
					//OnDisconnect();
					return false;
				}
				InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FAILED);
				Sleep(1000);
				InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
				//OnDisconnect();
				return false;

			}
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FAILED);
			Sleep(1000);
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
			//OnDisconnect();
			return false;
		}
		else
		{
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FAILED);
			Sleep(1000);
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
			//OnDisconnect();
			return false;
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EtPqpAUSB.Open()!");
		return false;
	}
}

bool EtPqpAUSB::Close()
{
	try
	{
		EmService::WriteToLogGeneral("Close EtPqpAUSB");

		pTimerThread_->bStopTimer_ = true;
		terminateRxThread_ = true;
		Sleep(2000);
		CloseHandle(rxThread_);

		DWORD dwReturnBytes;
		InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
		EndpointAddress_ = USB_EPIN_ADDRESS;
		BOOL bDeviceIoControlResult = DeviceIoControl(hUsbDevice_,
						IOCTL_ADAPT_ABORT_PIPE,
						&EndpointAddress_, sizeof(BYTE),NULL,0,
						&dwReturnBytes, NULL); 
		CloseHandle(hUsbDevice_);						
		//OnDisconnect();
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("EtPqpAUSB Close failed!");
		return false;
	}
	return true;
}

int EtPqpAUSB::Write(DWORD size, BYTE* buffer)
{
	BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
	try
	{
		DWORD dwTimeout = 100;
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  

		BOOL bDeviceIoControlResult;
		DWORD dwReturnBytes;  
		OVERLAPPED TxOverlapped;

		memset(pCtrlBuf,0,sizeof(SINGLE_TRANSFER));  
		pTransfer->ucEndpointAddress = USB_EPOUT_ADDRESS;  

		memset(&TxOverlapped, 0, sizeof(TxOverlapped));
		TxOverlapped.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);


		bDeviceIoControlResult = DeviceIoControl (hUsbDevice_,  
					IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
					pTransfer, sizeof(SINGLE_TRANSFER),   
					buffer, size,   
					&dwReturnBytes, &TxOverlapped);  

		if (bDeviceIoControlResult == TRUE)
		{
		}
		else
		{
			WaitForSingleObject(TxOverlapped.hEvent,dwTimeout);
			GetOverlappedResult(hUsbDevice_,&TxOverlapped,&dwReturnBytes,TRUE);

			EndpointAddress_ = USB_EPOUT_ADDRESS;
			bDeviceIoControlResult = DeviceIoControl(hUsbDevice_,
							IOCTL_ADAPT_ABORT_PIPE,
							&EndpointAddress_, sizeof(BYTE), NULL, 0,
							&dwReturnBytes, NULL); 
		}							
		CloseHandle(TxOverlapped.hEvent);
		delete [] pCtrlBuf;
		return 0;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("EtPqpAUSB Write failed!");
		delete [] pCtrlBuf;
		return -1;
	}
	/*finally
	{
		delete [] pCtrlBuf;
	}*/
}

void EtPqpAUSB::RxThread()
{
	BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
	try
	{
		BYTE EndpointAddress;
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;
		OVERLAPPED RxOverlapped;
		BYTE UsbRxBuffer[4096];
		BOOL bDeviceIoControlResult;
		BOOL bGetOverolappedResultResult;
		DWORD dwReturnBytes;
		DWORD dw; 
		BOOL bBreak;
		DWORD dwLastError;

		while(1)
		{
			if (terminateRxThread_) break;

			memset(pCtrlBuf,0,sizeof(SINGLE_TRANSFER));  
			pTransfer->ucEndpointAddress = USB_EPIN_ADDRESS;  
			memset(&RxOverlapped, 0, sizeof(RxOverlapped));
			RxOverlapped.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
			bDeviceIoControlResult = DeviceIoControl (hUsbDevice_,  
						IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
						pTransfer, sizeof(SINGLE_TRANSFER),   
						UsbRxBuffer,512,   
						&dwReturnBytes,&RxOverlapped);  
			
			if (bDeviceIoControlResult==TRUE)
			{
				//if (dwReturnBytes!=0) InterlockedIncrement((LONG*)(&dwRxErrorCounter));
				for(dw=0;dw<dwReturnBytes;dw++)
				{
					RxFunction(UsbRxBuffer[dw]);
				}
			}
			else
			{
				bBreak = FALSE;
				if ((dwLastError=GetLastError()) != ERROR_IO_PENDING)
				{
				}
				while(1)
				{
					if (terminateRxThread_) break;

					switch(WaitForSingleObject(RxOverlapped.hEvent, 100))
					{
						case WAIT_OBJECT_0:
							bGetOverolappedResultResult = GetOverlappedResult(hUsbDevice_, &RxOverlapped, &dwReturnBytes, TRUE);
							//if (dwReturnBytes!=0) InterlockedIncrement((LONG*)(&dwRxErrorCounter));
							for(dw = 0; dw < dwReturnBytes; dw++)
							{
								RxFunction(UsbRxBuffer[dw]);
							}
							bBreak = TRUE;
							break;
						case WAIT_TIMEOUT:
							bGetOverolappedResultResult = GetOverlappedResult(hUsbDevice_, &RxOverlapped, &dwReturnBytes, FALSE);
							//if (dwReturnBytes!=0) InterlockedIncrement((LONG*)(&dwRxErrorCounter));
							for(dw = 0; dw < dwReturnBytes; dw++)
							{
								RxFunction(UsbRxBuffer[dw]);
							}
							break;
						default:
							dw = 0;
							break;
					}
					if (bBreak == TRUE)
					{
						break;
					}
					if (connectStatus_ == CONNECTSTATUS_FORCEDISCONNECT)
					{
						InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
						EndpointAddress = USB_EPIN_ADDRESS;
						bDeviceIoControlResult = DeviceIoControl(hUsbDevice_,
										IOCTL_ADAPT_ABORT_PIPE,
										&EndpointAddress,sizeof(BYTE),NULL,0,
										&dwReturnBytes,NULL); 
						delete [] pCtrlBuf;
						pCtrlBuf = 0;
						CloseHandle(hUsbDevice_);						
						//OnDisconnect();
						return;
					}
				}
			}
			CloseHandle(RxOverlapped.hEvent);
		}

		if(pCtrlBuf != 0) delete [] pCtrlBuf;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EtPqpAUSB.RxThread()!");
		delete [] pCtrlBuf;
		innerError_ = true;
		SetEvent(hEventDisconnected_);
	}
	/*finally
	{
		delete [] pCtrlBuf;
	}*/
}

//}
