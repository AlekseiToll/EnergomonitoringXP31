// EmUsb.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "EmUsb.h"
#include "ftdi_d2xx.h"

#include <stdio.h>
//#include <conio.h>
//#include <stdlib.h>
//#include <time.h>

#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

////////////////////////////////////////
// GLOBAL INNER OBJECT EMonitor33

class FTDI_device *EMonitor33 = NULL;

////////////////////////////////////////
// EXTERN FUNCTIONS

EMUSB_API bool USBCreateInstance(void)
{
	if (EMonitor33 != NULL) delete(EMonitor33);
	EMonitor33 = new FTDI_device();
	return EMonitor33 != NULL;
}

EMUSB_API void USBFreeInstance(void)
{
	if (EMonitor33 != NULL) delete(EMonitor33);
	EMonitor33 = NULL;
}

EMUSB_API bool USBLoadDriver(void)
{
	if (EMonitor33 == NULL) return false;
	return (bool) EMonitor33->LoadDLL();
}

EMUSB_API int USBGetNumberDevices(void)
{
	if (EMonitor33 == NULL) return 0;
	return  EMonitor33->GetNumberDevices();
}

EMUSB_API bool USBGetSerialDescription(int devIndex, BYTE* SerialBuffer, BYTE* DescrBuffer)
{
	//dbgInfo(SerialBuffer, 128);
	if (EMonitor33 == NULL) return false;
	return (bool)EMonitor33->GetSerialDescription(devIndex, SerialBuffer, DescrBuffer);
}

EMUSB_API bool USBSetSerial(BYTE* SerialBuffer)
{
	//dbgInfo(SerialBuffer, 128);
	if (EMonitor33 == NULL) return false;
	for (int i=0; i<128; i++)
	{
		EMonitor33->SerialNumber[i] = SerialBuffer[i];
	}
	return true;
}

EMUSB_API bool USBOpenDeviceBySerial()
{
	if (EMonitor33 == NULL) return false;
	return (bool)EMonitor33->OpenDeviceBySerial();
}

EMUSB_API bool USBClose()
{
	if (EMonitor33 == NULL) return false;
	return (bool)EMonitor33->CloseDevice();
}

EMUSB_API int USBRead(BYTE* buffer, int bytesToRead)
{
	if (EMonitor33 == NULL) return false;
	int bytesOfRead = 0;
	return EMonitor33->USBRead(buffer, bytesToRead, &bytesOfRead);
}

EMUSB_API int USBWrite(BYTE* buffer, int bytesToWrite)
{
	if (EMonitor33 == NULL) return false;
	int bytesOfWrite = 0;

//	dbgInfo(TEXT("USB_Write_buffer.dbg"), buffer, bytesToWrite);
//	dbgInfo(TEXT("USB_Write_buffer_length.dbg"), (BYTE *)&bytesToWrite, sizeof(bytesToWrite));

	return EMonitor33->USBWrite(buffer, bytesToWrite, &bytesOfWrite);
}

EMUSB_API void dbgInfo(LPCSTR lpcFileName, BYTE* buffer, int length)
{
	/*HANDLE hFile; 
	 
	hFile = CreateFile(lpcFileName,				// file to open
					   GENERIC_WRITE,			// open for reading
					   FILE_SHARE_WRITE,		// share for reading
					   NULL,					// default security
					   OPEN_ALWAYS,				// existing file only
					   FILE_ATTRIBUTE_NORMAL,	// normal file
					   NULL);					// no attr. template
	 
	if (hFile == INVALID_HANDLE_VALUE) 
	{ 
		//printf("Could not open file (error %d)\n", GetLastError());
		return;
	}
	DWORD written = 0;
	WriteFile(hFile, (void *)buffer, length, &written, NULL);
	CloseHandle(hFile);*/

}