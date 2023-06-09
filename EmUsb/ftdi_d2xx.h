
#ifndef FTDID2XX_H
#define FTDID2XX_H

#include "ftd2xx.h"
//#include "emonitor.h"

#include <math.h>


// *************************************************************************************
// *************************************************************************************

 // ������������ ����� �������� �� ���� USB, �� ������� ����� ������� ���� ��� ������
#define		MAX_NUMBER_DEVICES		4

// *************************************************************************************

// FTDI BAUD RATES
#define	FTDI_115200bps		0x001A
#define	FTDI_3Mbps			0x0000
#define	FTDI_2Mbps			0x0001

#define FTDI_BAUDRATE		FTDI_3Mbps

// *************************************************************************************

// ������������ ������� �� ������
#define MAX_TIMEOUT			5000 // in milliseconds

// ����������� ������� �� ������
#define MIN_TIMEOUT			100 // in milliseconds

// �������� �� ���������
#define DEFAULT_WRITE_TIMEOUT	0
#define DEFAULT_READ_TIMEOUT	1000

// *************************************************************************************

// ������������ ������ �������� ������
#define MAX_INPUT_BUFFER	65536

// *************************************************************************************

// USB full speed ��������� (!!!�� ������!!!)

#define USB_TRANSFER_SIZE 	65536
#define	USB_LATENCY_TIMER	16



// *************************************************************************************

// ERRORS


#define		EM33_ERROR_NOTENOUGHSPACE	1
#define		EM33_ERROR_USBEXCHANGE		2
#define		EM33_ERROR_CRCFAILED		3
#define		EM33_ERROR_NORMAFAILED		4
#define		EM33_ERROR_WRONGLENGTH		5

// *************************************************************************************

// CRC16

#define MASK16  0x1021



// *************************************************************************************
// *************************************************************************************
/*
CLASS: FTDI_device		 
PURPOSE: Contains all the functions and variables, necessary to communicate with FT232BM device
HISTORY:		01.11.2005	BorisP	Copied from FTDI example and modified
*/
// *************************************************************************************
// *************************************************************************************


class FTDI_device
{
//===================================================================================

	public:
		FTDI_device(void);	// constructor
//		~FTDI_device(void);	// destructor

		BYTE SerialNumber[128];
		BYTE Description[128];

	
		DWORD dwBytesWritten;
		DWORD dwBytesReceived;


	
		int LoadDLL(void);

		int GetNumberDevices(void);
		int GetSerialDescription(int devIndex, BYTE SerialBuffer[128], BYTE DescrBuffer[128]);



		int ReadNAND(
				USHORT usDeviceIndex,	
				USHORT usFirstPage,
				USHORT usNumberPages,
				LPVOID Buffer );


		int ReadFRAM(
				USHORT usDeviceIndex,	
				USHORT usStartAddress,	
				USHORT usNumberWords,
				LPVOID Buffer );


		int WriteFRAM(
				USHORT usDeviceIndex,	
				USHORT usStartAddress,	
				USHORT usNumberWords,
				LPVOID Buffer );

		int WriteRAM(
				USHORT usDeviceIndex,	
				USHORT usPageNumber,
				USHORT usOffset,	
				USHORT usNumberWords,
				LPVOID Buffer );

		int ExchangeViaUSB(int iDeviceIndex , UCHAR *sSendBuffer , DWORD dwSendLength , UCHAR *sReceiveBuffer , DWORD dwReceiveLength , DWORD dwReceiveTimeout , LPDWORD dwReceied , LPBOOL bCrcOK, LPBOOL bNormaOK );
	
		

		int USBRead(BYTE *buffer, int BytesToRead, int *BytesOfRead);
		int USBWrite(BYTE *buffer, int BytesToWrite, int *BytesOfWrite);

		int OpenDeviceBySerial(void);
		int CloseDevice(void);




//===================================================================================

	protected:

//===================================================================================

		// FTD2XX DRIVER FUNCTIONS


		typedef FT_STATUS (WINAPI *PtrToOpen)(int, FT_HANDLE *); 
		PtrToOpen m_pOpen; 
//		FT_STATUS Open(int, FT_HANDLE * );

		typedef FT_STATUS (WINAPI *PtrToOpenEx)(PVOID, DWORD, FT_HANDLE *); 
		PtrToOpenEx m_pOpenEx; 
//		FT_STATUS OpenEx(PVOID, DWORD);

		typedef FT_STATUS (WINAPI *PtrToListDevices)(PVOID, PVOID, DWORD);
		PtrToListDevices m_pListDevices; 
//		FT_STATUS ListDevices(PVOID, PVOID, DWORD);

		typedef FT_STATUS (WINAPI *PtrToClose)(FT_HANDLE);
		PtrToClose m_pClose;
//		FT_STATUS Close();

		typedef FT_STATUS (WINAPI *PtrToRead)(FT_HANDLE, LPVOID, DWORD, LPDWORD);
		PtrToRead m_pRead;
//		FT_STATUS Read(LPVOID, DWORD, LPDWORD);

		typedef FT_STATUS (WINAPI *PtrToWrite)(FT_HANDLE, LPVOID, DWORD, LPDWORD);
		PtrToWrite m_pWrite;
//		FT_STATUS Write(LPVOID, DWORD, LPDWORD);

		typedef FT_STATUS (WINAPI *PtrToResetDevice)(FT_HANDLE);
		PtrToResetDevice m_pResetDevice;
//		FT_STATUS ResetDevice();
	
		typedef FT_STATUS (WINAPI *PtrToPurge)(FT_HANDLE, ULONG);
		PtrToPurge m_pPurge;
//		FT_STATUS Purge(ULONG);
	
		typedef FT_STATUS (WINAPI *PtrToSetTimeouts)(FT_HANDLE, ULONG, ULONG);
		PtrToSetTimeouts m_pSetTimeouts;
//		FT_STATUS SetTimeouts(ULONG, ULONG);

		typedef FT_STATUS (WINAPI *PtrToGetQueueStatus)(FT_HANDLE, LPDWORD);
		PtrToGetQueueStatus m_pGetQueueStatus;
		FT_STATUS GetQueueStatus(LPDWORD);


		typedef FT_STATUS (WINAPI *PtrToSetDivisor)(FT_HANDLE, USHORT);
		PtrToSetDivisor m_pSetDivisor;
//		FT_STATUS FT_SetDivisor(FT_HANDLE,USHORT); 


		typedef FT_STATUS (WINAPI *PtrToGetStatus)(FT_HANDLE, LPDWORD, LPDWORD, LPDWORD );
		PtrToGetStatus m_pGetStatus;
//		FT_STATUS FT_GetStatus(FT_HANDLE,LPDWORD,LPDWORD,LPDWORD); 

		typedef FT_STATUS (WINAPI *PtrToSetDataCharacteristics)(FT_HANDLE,UCHAR,UCHAR,UCHAR);
		PtrToSetDataCharacteristics m_pSetDataCharacteristics;

		typedef FT_STATUS (WINAPI *PtrToSetUSBParameters)(FT_HANDLE, DWORD, DWORD );
		PtrToSetUSBParameters m_pSetUSBParameters;

		typedef FT_STATUS (WINAPI *PtrToSetLatencyTimer)(FT_HANDLE, UCHAR );
		PtrToSetLatencyTimer m_pSetLatencyTimer;

		typedef FT_STATUS (WINAPI *PtrToEE_Read)(FT_HANDLE, PFT_PROGRAM_DATA );
		PtrToEE_Read m_pEE_Read;
		
		typedef FT_STATUS (WINAPI *PtrToEE_Program)(FT_HANDLE, PFT_PROGRAM_DATA );
		PtrToEE_Program m_pEE_Program;


//===================================================================================

	protected:

//===================================================================================

		HMODULE m_hmodule;	// MODULE - this statement defines a group of functions, typically a set of DLL entry points.

		FT_HANDLE m_ftHandle;
		FT_STATUS ftStatus;


		DWORD dwReadTimeoutMs;
		DWORD dwWriteTimeoutMs;



		int OpenDevice(int devIndex);
//		int OpenDeviceBySerial(void);
//		int CloseDevice(void);
		int ResetDevice(void);
		int SetDivisor(USHORT usDivisor);
		int PurgeBuffers(void);

		int SetUSBParameters(DWORD dwInTransferSize);
		int SetLatencyTimer(UCHAR ucTimer );



//		DWORD GetReceiveQueueStatus(void);
//		DWORD GetTransmitQueueStatus(void);

//		int WriteByte(BYTE byte);
		int WriteString(BYTE *bytes, DWORD len);
		int ReadString(BYTE *bytes, DWORD len);
		int ReadStringWithTimeout(BYTE *bytes, DWORD len, DWORD timeout );

//		int ExchangeViaUSB(int iDeviceIndex , UCHAR *sSendBuffer , DWORD dwSendLength , UCHAR *sReceiveBuffer , DWORD dwReceiveLength , DWORD dwReceiveTimeout , LPDWORD dwReceived , LPBOOL bCrcOK, LPBOOL bNormaOK );
		int WriteRead(BYTE *sSendBuffer, DWORD dwSendLength, BYTE *sReceiveBuffer, DWORD dwReceiveLength, DWORD dwReceiveTimeout, LPDWORD dwReceived);



		int SetTimeouts(DWORD dwReadTimeout, DWORD dwWriteTimeout );
		int SetDataCharacteristics(UCHAR uWordLength, UCHAR uStopBits,UCHAR uParity);

//		bool bDeviceOpened;
//		int ListDevices(PVOID, PVOID, DWORD);
//
//		int ExchangeViaUSB(int iDeviceIndex , UCHAR *sSendBuffer , DWORD dwSendLength , UCHAR *sReceiveBuffer , DWORD dwReceiveLength , DWORD dwReceiveTimeout , LPDWORD dwReceied , LPBOOL bCrcOK, LPBOOL bNormaOK );

		USHORT crc16a(void *txt, USHORT lll);
		USHORT  updcrc(USHORT c,USHORT crc);



//===================================================================================

};




#endif