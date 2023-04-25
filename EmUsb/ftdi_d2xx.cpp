


#include "stdafx.h"

#include "ftdi_d2xx.h"


// *************************************************************************************


//========================================================================
//========================================================================
unsigned __int64 GetCycleCount(void)
{
		_asm rdtsc	// �������������� ������ __rdtsc();
}
//========================================================================
//========================================================================



FTDI_device::FTDI_device(void)
{
//	bDeviceOpened = false;

	dwBytesWritten = 0;
	dwBytesReceived = 0;

	dwReadTimeoutMs = DEFAULT_READ_TIMEOUT;
	dwWriteTimeoutMs = DEFAULT_WRITE_TIMEOUT;
}


/*
FTDI_device::~FTDI_device(void)
{

}
*/


// *************************************************************************************
// *************************************************************************************
/*
FUNCTION:		 
PURPOSE:
PARAMETERS:		void
RETURN VALUE:	0 if there are errors, 1 if success
HISTORY:		01.11.2005	BorisP	Copied from FTDI example and modified
*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::LoadDLL(void)
{
	m_hmodule = LoadLibrary("Ftd2xx.dll");	

	if(m_hmodule == NULL)
	{
//		printf("ERROR (LoadDLL): Could not load FTD2XX driver\n ");
		return 0;
	}

//	printf("FTD2XX driver loaded\n");

//===========================================================




	m_pWrite = (PtrToWrite)GetProcAddress(m_hmodule, "FT_Write");
	if (m_pWrite == NULL)
	{
//		printf("Error: Can't Find FT_Write\n");
		return 0;
	}

	m_pListDevices = (PtrToListDevices)GetProcAddress(m_hmodule, "FT_ListDevices");
	if(m_pListDevices == NULL)
	{
//		printf("Error: Can't Find FT_ListDevices\n");
		return 0;
	}

	m_pOpen = (PtrToOpen)GetProcAddress(m_hmodule, "FT_Open");
	if (m_pOpen == NULL)
	{
//		printf("ERROR: Can't Find FT_Open\n");
		return 0;
	}

	m_pOpenEx = (PtrToOpenEx)GetProcAddress(m_hmodule, "FT_OpenEx");
	if (m_pOpenEx == NULL)
	{
//		printf("ERROR: Can't Find FT_Open\n");
		return 0;
	}

	m_pClose = (PtrToClose)GetProcAddress(m_hmodule, "FT_Close");
	if (m_pClose == NULL)
	{
//		printf("ERROR: Can't Find FT_Close\n");
		return 0;
	}

	m_pResetDevice = (PtrToClose)GetProcAddress(m_hmodule, "FT_ResetDevice");
	if (m_pResetDevice == NULL)
	{
//		printf("ERROR: Can't Find FT_ResetDevice\n");
		return 0;
	}

	m_pWrite = (PtrToWrite)GetProcAddress(m_hmodule, "FT_Write");
	if (m_pWrite == NULL)
	{
//		printf("ERROR: Can't Find FT_Write\n");
		return 0;
	}

	m_pRead = (PtrToRead)GetProcAddress(m_hmodule, "FT_Read");
	if (m_pRead == NULL)
	{
//		printf("ERROR: Can't Find FT_Read\n");
		return 0;
	}

	m_pSetDivisor = (PtrToSetDivisor)GetProcAddress(m_hmodule, "FT_SetDivisor");
	if (m_pSetDivisor == NULL)
	{
//		printf("ERROR: Can't Find FT_SetDivisor\n");
		return 0;
	}

//	m_pGetStatus = (PtrToGetStatus)GetProcAddress(m_hmodule, "FT_GetStatus");
//	if (m_pGetStatus == NULL)
//	{
//		printf("ERROR: Can't Find FT_GetStatus\n");
//		return 0;
//	}

//	m_pGetQueueStatus = (PtrToGetQueueStatus)GetProcAddress(m_hmodule, "FT_GetQueueStatus");
//	if (m_pGetQueueStatus == NULL)
//	{
//		printf("ERROR: Can't Find FT_GetQueueStatus\n");
//		return 0;
//	}

	m_pSetTimeouts = (PtrToSetTimeouts)GetProcAddress(m_hmodule, "FT_SetTimeouts");
	if (m_pSetTimeouts == NULL)
	{
//		printf("ERROR: Can't Find FT_SetTimeouts\n");
		return 0;
	}

	m_pSetDataCharacteristics = (PtrToSetDataCharacteristics)GetProcAddress(m_hmodule, "FT_SetDataCharacteristics");
	if (m_pSetDataCharacteristics == NULL)
	{
//		printf("ERROR: Can't Find FT_SetDataCharacteristics\n");
		return 0;
	}

	m_pPurge = (PtrToPurge)GetProcAddress(m_hmodule, "FT_Purge");
	if (m_pPurge == NULL)
	{
//		printf("ERROR: Can't Find FT_Purge\n");
		return 0;
	}
	
	m_pSetUSBParameters = (PtrToSetUSBParameters)GetProcAddress(m_hmodule, "FT_SetUSBParameters");
	if (m_pPurge == NULL)
	{
//		printf("ERROR: Can't Find FT_Purge\n");
		return 0;
	}

	m_pSetLatencyTimer  = (PtrToSetLatencyTimer)GetProcAddress(m_hmodule, "FT_SetLatencyTimer");
	if (m_pSetLatencyTimer == NULL)
	{
//		printf("ERROR: Can't Find FT_SetLatencyTimer\n");
		return 0;
	}

	m_pEE_Read = (PtrToEE_Read)GetProcAddress(m_hmodule, "FT_EE_Read");
	if (m_pEE_Read == NULL)
	{
//		printf("ERROR: Can't Find FT_EE_Read\n");
		return 0;
	}

	m_pEE_Program = (PtrToEE_Program)GetProcAddress(m_hmodule, "FT_EE_Program");
	if (m_pEE_Program == NULL)
	{
//		printf("ERROR: Can't Find FT_EE_Program\n");
		return 0;
	}

	return 1;
}	




// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: 		 
PURPOSE: Get information concerning the devices currently connected
PARAMETERS:	See manual
RETURN VALUE: FT_OK if successful, otherwise the return value is an FT error code
HISTORY:		02.11.2005	BorisP	Copied from FTDI example and modified
*/
// *************************************************************************************
// *************************************************************************************
/*
int FTDI_device::ListDevices(PVOID pArg1, PVOID pArg2, DWORD dwFlags)
{
	ftStatus = (*m_pListDevices)(pArg1, pArg2, dwFlags);

	if (ftStatus != FT_OK) { 
		printf("ERROR: FT_ListDevices failed\n");
		return 0;
	} 
	return 1;

}
*/






// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: GetNumberDevices 		 
PURPOSE: Gets number of devices connected
PARAMETERS:	void
RETURN VALUE: number of devices connected
HISTORY:		02.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::GetNumberDevices(void)
{

	DWORD numDevs = 0;

	ftStatus = (*m_pListDevices)(&numDevs,NULL,FT_LIST_NUMBER_ONLY); 

	if (ftStatus != FT_OK) { 
		//printf("ERROR (GetNumberDevices): FT_ListDevices failed\n");
		return 0;
	} 

	return (int)numDevs;



}


// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: GetSerialNumber	 
PURPOSE: Gets serial number of device number devIndex
PARAMETERS:	number of device, buffer to write the serial number to
RETURN VALUE: 1 on success, 0 on failure
HISTORY:		02.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::GetSerialDescription( int devIndex, BYTE SerialBuffer[128], BYTE DescrBuffer[128])
{

	int iNDevs = GetNumberDevices();
	DWORD numDevs = 0;

	ftStatus = (*m_pListDevices)((PVOID)devIndex,SerialBuffer,FT_LIST_BY_INDEX|FT_OPEN_BY_SERIAL_NUMBER); 

//	ftStatus = ListDevices((PVOID)devIndex,SerialBuffer,FT_LIST_BY_INDEX|FT_OPEN_BY_DESCRIPTION); 

	if (!FT_SUCCESS(ftStatus))
	{ 
//		printf("ListDevices in GetSerialNumber failed\n");
		return 0;
	} 


	ftStatus = (*m_pListDevices)((PVOID)devIndex,DescrBuffer,FT_LIST_BY_INDEX|FT_OPEN_BY_DESCRIPTION); 

	if (!FT_SUCCESS(ftStatus))
	{ 
//		printf("ListDevices in GetSerialNumber failed\n");
		return 0;
	} 



	return 1;
}



// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: OpenDevice	 
PURPOSE: Opens specified device, sets baud rate, data characteristics and initial timeouts
PARAMETERS:	index
RETURN VALUE: 1 on success, 0 on failure
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::OpenDevice(int devIndex)
{
	int iRetValue = 1;
	ftStatus = (m_pOpen)( devIndex ,&m_ftHandle); 
	if (ftStatus != FT_OK)
	{ 
//		printf("FATAL ERROR: FT_Open failed\n");
		return 0;
	} 
//	bDeviceOpened = true;
	iRetValue = 1;
// Set the USB request transfer size and latency timer value 
	iRetValue *= SetLatencyTimer(USB_LATENCY_TIMER); 
	iRetValue *= SetUSBParameters(USB_TRANSFER_SIZE);
// resetting the device
	iRetValue *= ResetDevice();
	iRetValue *= PurgeBuffers();
// setting baud rate
	iRetValue *= SetDivisor(FTDI_BAUDRATE);
// setting data characterstics
	iRetValue *= SetDataCharacteristics(FT_BITS_8, FT_STOP_BITS_2, FT_PARITY_NONE);
// setting timeouts
	iRetValue *= SetTimeouts( DEFAULT_READ_TIMEOUT, DEFAULT_WRITE_TIMEOUT );

	return iRetValue;
}


int FTDI_device::OpenDeviceBySerial(void)
{

	int iRetValue = 1;
	ftStatus = (m_pOpenEx)( SerialNumber, FT_OPEN_BY_SERIAL_NUMBER,&m_ftHandle); 
	if (ftStatus != FT_OK)
	{ 
//		printf("FATAL ERROR: FT_Open failed\n");
		return 0;
	} 




	iRetValue = 1;
// Set the USB request transfer size and latency timer value 
	iRetValue *= SetLatencyTimer(USB_LATENCY_TIMER); 
	iRetValue *= SetUSBParameters(USB_TRANSFER_SIZE);
// resetting the device
	iRetValue *= ResetDevice();
	iRetValue *= PurgeBuffers();
// setting baud rate
	iRetValue *= SetDivisor(FTDI_BAUDRATE);
// setting data characterstics
	iRetValue *= SetDataCharacteristics(FT_BITS_8, FT_STOP_BITS_2, FT_PARITY_NONE);
// setting timeouts
	iRetValue *= SetTimeouts( DEFAULT_READ_TIMEOUT, DEFAULT_WRITE_TIMEOUT );


	return iRetValue;
}





// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: CloseDevice	 
PURPOSE: Closes an opened device
PARAMETERS:	void
RETURN VALUE: 1 on success, 0 on failure
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************



int FTDI_device::CloseDevice(void)
{

	PurgeBuffers();

	ResetDevice();

	ftStatus = (m_pClose)(m_ftHandle); 
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_close failed");
		return 0;
	} 

//	bDeviceOpened = false;
	return 1;


}



// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: ResetDevice	 
PURPOSE: Resets an opened device
PARAMETERS:	void
RETURN VALUE: 1 on success, 0 on failure
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************



int FTDI_device::ResetDevice(void)
{

	ftStatus = (m_pResetDevice)(m_ftHandle); 
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_ResetDevice failed");
		return 0;
	} 

	return 1;


}




// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: PurgeBuffers	 
PURPOSE: Purges receive and transmit buffers
PARAMETERS:	void
RETURN VALUE: 1 on success, 0 on failure
HISTORY:		10.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::PurgeBuffers(void)
{

	ftStatus = (m_pPurge)(m_ftHandle, FT_PURGE_RX); 
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_PURGE failed\n");
		return 0;
	} 

	ftStatus = (m_pPurge)(m_ftHandle, FT_PURGE_TX); 
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_PURGE failed\n");
		return 0;
	} 


	return 1;
}



// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: SetDivisor	 
PURPOSE: This function sets the baud rate for the device. It is used to set non-standard baud rates. 
PARAMETERS:	void
RETURN VALUE: 1 on success, 0 on failure
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::SetDivisor(USHORT usDivisor)
{

	ftStatus = (m_pSetDivisor)(m_ftHandle, usDivisor); 
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_SetDivisor failed\n");
		return 0;
	} 

	return 1;

}


// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: GetReceiveQueueStatus	 
PURPOSE: gets the number of characters in the receive queue 
PARAMETERS:	void
RETURN VALUE: number of bytes in receive queue, (-1) on error
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

/*
DWORD FTDI_device::GetReceiveQueueStatus(void)
{
 
	DWORD dwTemp1 = -1;
	DWORD dwTemp2 = -1;
	DWORD dwTemp3 = -1;

	ftStatus = (m_pGetStatus)(m_ftHandle, &dwTemp1, &dwTemp2, &dwTemp3); 
	if (ftStatus != FT_OK)
	{
		dwTemp1 = -1;
	}


	return dwTemp1;

}


FT_STATUS FTDI_device::GetQueueStatus(LPDWORD lpdwTemp)
{



		return (*m_pGetQueueStatus)(m_ftHandle, lpdwTemp);

}
*/

		
// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: GetTransmitQueueStatus	 
PURPOSE: gets the number of characters in the transmit queue 
PARAMETERS:	void
RETURN VALUE: number of bytes in transmit queue,  (-1) on error
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

/*		
DWORD FTDI_device::GetTransmitQueueStatus(void)
{

	DWORD dwTemp1 = -1;
	DWORD dwTemp2 = -1;
	DWORD dwTemp3 = -1;


	ftStatus = (m_pGetStatus)(m_ftHandle, &dwTemp1, &dwTemp2, &dwTemp3); 
	if (ftStatus != FT_OK)
	{
		dwTemp2 = -1;
	}

	
	return dwTemp2;

}
*/

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: WriteByte	 
PURPOSE: writes 1 byte to UART 
PARAMETERS:	byte
RETURN VALUE: 1 on success, 0 on error
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************
/*
int FTDI_device::WriteByte(BYTE byte)
{

	BYTE buffer[1];

	ftStatus = (m_pWrite)(m_ftHandle, (LPVOID)buffer, 1, &dwBytesWritten); 

	if (ftStatus != FT_OK)
	{ 
		return 0;
	}

	return 1;
}
*/
// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: WriteString	 
PURPOSE: writes array of bytes to UART 
PARAMETERS:	bytes array, its length
RETURN VALUE: 1 on success, 0 on error
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::WriteString(BYTE *bytes, DWORD len)
{

	ftStatus = (m_pWrite)(m_ftHandle, (LPVOID)bytes, len, &dwBytesWritten); 

	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_Write failed\n");
		return 0;
	}

	

	return 1;
}


// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: ReadString	 
PURPOSE: read array of received bytes
PARAMETERS:	bytes array, its length
RETURN VALUE: 1 on success, 0 on error
HISTORY:		03.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::ReadString(BYTE *bytes, DWORD len)
{

	ftStatus = (m_pRead)(m_ftHandle, (LPVOID)bytes, len, &dwBytesReceived); 
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_Read failed\n");
		return 0;
	}




	return 1;

}


// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: SetTimeouts	 
PURPOSE: This function sets the read and write timeouts for the opened device
PARAMETERS:	 Read and write timeout in milliseconds.  
RETURN VALUE: 1 on success, 0 on error
HISTORY:		07.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::SetTimeouts(DWORD dwReadTimeout, DWORD dwWriteTimeout )
{

	ftStatus = (*m_pSetTimeouts)(m_ftHandle, dwReadTimeout, dwWriteTimeout);
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_SetTimeout failed\n");
		return 0;
	}

	return 1;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: SetDataCharacteristics	 
PURPOSE: This function sets the data characteristics for the device. 
PARAMETERS:
			UWordLength    Number of bits per word - must be FT_BITS_8 or FT_BITS_7.  
			UStopBits    Number of stop bits - must be FT_STOP_BITS_1 or FT_STOP_BITS_2.  
 			Uparity    FT_PARITY_NONE, FT_PARITY_ODD, FT_PARITY_EVEN, FT_PARITY_MARK, FT_PARITY_SPACE.  
RETURN VALUE: 1 on success, 0 on error
HISTORY:		08.11.2005	BorisP	created
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::SetDataCharacteristics(UCHAR uWordLength, UCHAR uStopBits,UCHAR uParity)
{
	ftStatus = (*m_pSetDataCharacteristics)(m_ftHandle, uWordLength, uStopBits, uParity);
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_SetDataCharacteristics failed\n");
		return 0;
	}
	return 1;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: ReadStringWithTimeout	 
PURPOSE: read array of received bytes, exit if time elapsed
PARAMETERS:	bytes array, its length, timeout in milliseconds
RETURN VALUE: 1 on success, 0 on error 
HISTORY:		09.11.2005	BorisP	created
NOTES:

������� ������ ��������� �������� ���������� ���� � ������������ �����
����� �� ������� ���������� ���� �������� ���������� ���� ���������, ���� ���� ����� �������� �������

*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::ReadStringWithTimeout(BYTE *bytes, DWORD len, DWORD timeout )
{
	DWORD dwTimeout = timeout;
	USHORT usCounter = 0;

	DWORD i = 0 ;
	USHORT usTemp = 0;

	DWORD dwReceived = 0;

// ��������� �������� �� �������� ��������
	if ( dwTimeout == 0 ) dwTimeout = 100;
	if ( dwTimeout > MAX_TIMEOUT ) dwTimeout = MAX_TIMEOUT;

// ��������� ���������� ������� ������, �������������� �� ����� ��������
	USHORT usStopCounter = (USHORT)ceil((double)dwTimeout / (double)MIN_TIMEOUT );
	
	
	BYTE *buffer = new BYTE[MAX_INPUT_BUFFER];

	if ( buffer == NULL )
	{
//		printf("FATAL ERROR: ReadStringWithTimeout - not enough memory\n");
		return 0;
	}

// ������������� ����������� ������� �� ������

	SetTimeouts( MIN_TIMEOUT, dwWriteTimeoutMs);

// ������� ������

	for( usCounter = 0 ; usCounter < usStopCounter ; usCounter++ )
	{
		usTemp = ReadString(buffer, len);

	// ������ ������
		if ( usTemp == 0 )
		{
			return 0;
		}

	// appending newly received array to the previously received array
		for ( i = 0 ; i < dwBytesReceived ; i++)
		{
			bytes[ i + dwReceived ] = buffer [i] ; 
		}

//		printf("(#)");

		dwReceived += dwBytesReceived;
		
	// ���� �������� ����� ���� ��������� - �������
		if ( dwReceived >= len )
		{
//			printf("( dwReceived >= len )", usStopCounter);
			break;
		}
	}

	dwBytesReceived = dwReceived;

	//printf("( %d )", usStopCounter);

	// ���������� �������������� ���������
	SetTimeouts( MIN_TIMEOUT, dwWriteTimeoutMs);

	delete buffer;

	return 1;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: ExchangeViaUSB 
PURPOSE: 
PARAMETERS:	
RETURN VALUE:	1 if success, 0 if errors  
HISTORY:		10.11.2005	BorisP	created
NOTES:
*/
// *************************************************************************************
// *************************************************************************************
int FTDI_device::ExchangeViaUSB(
	int iDeviceIndex ,
	UCHAR *sSendBuffer ,
	DWORD dwSendLength ,
	UCHAR *sReceiveBuffer ,
	DWORD dwReceiveLength ,
	DWORD dwReceiveTimeout ,
	LPDWORD dwReceived ,
	LPBOOL bCrcOK,
	LPBOOL bNormaOK )
{

	int iRetValue = 1; 

	*bCrcOK = false;
	*bNormaOK = false;

	// �������� 
	// ��� ������� �������� �����, ��������� ��������, ��������� ������������ (�������� ����), ��������� ��������� �� ���������
		
	//	iRetValue *= OpenDevice(iDeviceIndex);
	iRetValue *= OpenDeviceBySerial();


// �������� ���������
//	iRetValue *= WriteString(sSendBuffer, dwSendLength);

// ��������� ���������
//	iRetValue *= ReadStringWithTimeout(sReceiveBuffer, dwReceiveLength /*bytes*/, dwReceiveTimeout /*milliseconds*/ );


//	Sleep(200);
	
	iRetValue *= WriteRead(sSendBuffer, dwSendLength, sReceiveBuffer, dwReceiveLength, dwReceiveTimeout, dwReceived);


	dwBytesReceived = *dwReceived;

// ��������� ���������
	if ( dwBytesReceived > 0 )
	{
		if ( ( sReceiveBuffer[0] == 0x02 ) && ( sReceiveBuffer[1] == 0x00 ) ) *bNormaOK = true;
	
		if ( crc16a( sReceiveBuffer, dwBytesReceived ) == 0 ) *bCrcOK = true;

	}

	// ���������
	iRetValue *= CloseDevice();

// �������

//	if ( *bCrcOK != true )
//	{
//		Beep(500, 200);
//	}

	return iRetValue;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: SetUSBParameters 
PURPOSE: 
PARAMETERS:	
RETURN VALUE:	1 if success, 0 if errors  
HISTORY:		10.11.2005	BorisP	created
NOTES:
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::SetUSBParameters(DWORD dwInTransferSize)
{
	ftStatus = (*m_pSetUSBParameters)(m_ftHandle, dwInTransferSize, 0 );
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_SetUSBParameters failed\n");
		return 0;
	}
	return 1;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: SetUSBParameters 
PURPOSE: 
PARAMETERS:	
RETURN VALUE:	1 if success, 0 if errors  
HISTORY:		10.11.2005	BorisP	created
NOTES:
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::SetLatencyTimer(UCHAR ucTimer )
{
//FT_STATUS FT_SetLatencyTimer ( FT_HANDLE ftHandle, UCHAR ucTimer ) 

	ftStatus = (*m_pSetLatencyTimer)(m_ftHandle, ucTimer );
	if (ftStatus != FT_OK)
	{ 
//		printf("ERROR: FT_SetLatencyTimer failed\n");
		return 0;
	}
	return 1;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: WriteRead 
PURPOSE: 
PARAMETERS:	
RETURN VALUE:	1 if success, 0 if errors  
HISTORY:		11.11.2005	BorisP	created
NOTES:
*/
// *************************************************************************************
// *************************************************************************************

int FTDI_device::WriteRead(
			BYTE *sSendBuffer,
			DWORD dwSendLength,
			BYTE *sReceiveBuffer,
			DWORD dwReceiveLength,
			DWORD dwReceiveTimeout,
			LPDWORD dwReceivedLength)
{
	// ��������� ����������

	DWORD dwTimeout = dwReceiveTimeout;
	USHORT usCounter = 0;

	DWORD i = 0 ;
	USHORT usTemp = 0;

	DWORD dwReceived = 0;

	int iRetValue = 1;

	sReceiveBuffer[1] = 0x00; // ������������� 

	// ��������� �������� �� �������� ��������

	if ( dwTimeout == 0 ) dwTimeout = 100;
	if ( dwTimeout > MAX_TIMEOUT ) dwTimeout = MAX_TIMEOUT;

	// ��������� ���������� ������� ������, �������������� �� ����� ��������
	USHORT usStopCounter = (USHORT)ceil((double)dwTimeout / (double)MIN_TIMEOUT );
	
	
	BYTE *buffer = new BYTE[MAX_INPUT_BUFFER];

	if ( buffer == NULL )
	{
//		printf("FATAL ERROR: ReadStringWithTimeout - not enough memory\n");
		return 0;
	}


// ������������� ����������� ������� �� ������


	SetTimeouts( MIN_TIMEOUT, 0);


// ������� �������� ���������

//	int iThreadPriority = GetThreadPriority(GetCurrentThread());
//	DWORD dwPriorityClass = GetPriorityClass(GetCurrentThread());
	
//	SetPriorityClass( GetCurrentThread(), REALTIME_PRIORITY_CLASS );
//	SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

// �����

	iRetValue *= WriteString(sSendBuffer, dwSendLength);

// ������� ������

	for( usCounter = 0 ; usCounter < usStopCounter ; usCounter++ )
	{
		dwBytesReceived = 0 ;

		iRetValue *= ReadString(buffer, dwReceiveLength);

	// appending newly received array to the previously received array
		for ( i = 0 ; i < dwBytesReceived ; i++)
		{
			sReceiveBuffer[ i + dwReceived ] = buffer [i] ; 
		}

		if ( sReceiveBuffer[1] == 0xFF )
		{
			break; // �� ����� - ������� ����� �� ������ �������

		}

		dwReceived += dwBytesReceived;
		
	// ���� �������� ����� ���� ��������� - �������
		if ( dwReceived >= dwReceiveLength )
		{
//			printf("\n##### (dwReceived = dwReceiveLength) OK\n");
			break;
		}
	}

//	SetPriorityClass( GetCurrentThread(), dwPriorityClass );
//	SetThreadPriority(GetCurrentThread(), iThreadPriority);


//	if ( usCounter == usStopCounter )
//	{
//		printf("\n##### (TIMEOUT) \n");
//	}
	
	*dwReceivedLength = dwReceived;

	delete buffer;

	return iRetValue;
}

int FTDI_device::USBRead(BYTE *buffer, int BytesToRead, int *BytesOfRead)
{

	int iRetValue = 1;

//	Sleep(1);

	unsigned __int64 t_start;
	unsigned __int64 dt = 1500000;
//	unsigned __int64 dt = 5000000;
	t_start = GetCycleCount();
	while(1)
	{
		if ((GetCycleCount() - t_start) > dt) break;
	}


//	SetTimeouts(3000, 3000);
	SetTimeouts(500, 100);
	iRetValue *= ReadString(buffer, BytesToRead);
	*BytesOfRead = (int)dwBytesReceived;
	if (*BytesOfRead != BytesToRead)
	{
		if(*BytesOfRead == 0)
			iRetValue = 0;
		else
			Beep(50, 20);
	}
	return iRetValue;

}


int FTDI_device::USBWrite(BYTE *buffer, int BytesToWrite, int *BytesOfWrite)
{

	int iRetValue = 1;
	SetTimeouts( 1000, 100);	
	iRetValue *= WriteString(buffer, BytesToWrite);
	*BytesOfWrite = (int)dwBytesWritten;

	return iRetValue;

}







// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: ReadNAND 
PURPOSE: Opens USB device number (usDeviceIndex), reads (usNumberPages) pages in NAND FLASH, starting from page (usFirstPage)
PARAMETERS:	
RETURN VALUE:	0 if success, -1 if timeout, -2 if reading error (CRC  failed, NORMA failed, SYNCHRO failed), -3 if USB error
HISTORY:		16.11.2005	BorisP	created
NOTES:

������ ����� ���� 64������
������������ � ���� 65536 ������� �� 512 ����
������ ��� �������� CRC - ����� 13





*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::ReadNAND(
			USHORT usDeviceIndex,	
			USHORT usFirstPage,
			USHORT usNumberPages,
			LPVOID Buffer )
{

///////////////////////////////

	if ( usNumberPages > 127 )
		return -1;

	if ( (USHORT)(usNumberPages + usFirstPage) < (USHORT)usFirstPage )
		return -1;

///////////////////////////////
// preparing query message

	BYTE message[12];
	USHORT usTemp = 0;
	USHORT usOutLength = 0;

	// ���������
	message[0] = 0x02;
	message[1] = 0x0D; // ����� 13

	message[2] = 0x06;
	message[3] = 0x00; // ����� ����������� ����

	message[4] = 0x00;
	message[5] = 0x00; // ����� ���� 0

	message[6] = (BYTE)(usFirstPage & 0xFF );
	message[7] = (BYTE)( usFirstPage >> 8 );	// ����� ��������� ��������

	message[8] = (BYTE)(usNumberPages & 0xFF );
	message[9] = (BYTE)( usNumberPages >> 8 ); // ���������� �������

	usTemp = crc16a( (void*) message, 10);
	message[10] = (BYTE)(usTemp & 0xFF );
	message[11] = (BYTE)( usTemp >> 8 );

//	printf("ReadNAND(): query message prepared\n");


///////////////////////////////
// preparing buffer

	BYTE *buffer = new BYTE[0xFFFF];
	if ( buffer == NULL)
	{
//		printf("ReadNAND(): not enough memory\n");
		return -3;
	}

//	printf("ReadNAND(): buffer prepared\n");


///////////////////////////////
// USB transaction

	int iTemp1 = 0;
	int iTemp2 = 0;
	int iTemp3 = 0;

	DWORD dwExpectedBytes = 512 * (DWORD)usNumberPages + 6;
	DWORD dwTimeout = 200.00 + 20 * usNumberPages ;

	BOOL bCRC_OK = false;
	BOOL bNORMA_OK = false;
	DWORD dwReceivedBytes = 0;


	while(1)
	{

		bCRC_OK = false;
		bNORMA_OK = false;
		dwReceivedBytes = 0;

		iTemp1 = ExchangeViaUSB(usDeviceIndex , message, 12 , (BYTE*)buffer , dwExpectedBytes , dwTimeout , &dwReceivedBytes , &bCRC_OK, &bNORMA_OK );

//		printf("ReadNAND(): exchange returned %d\n", iTemp1);

		if (	( dwReceivedBytes == dwExpectedBytes ) &&
				( bCRC_OK == (BOOL)true ) &&
				( bNORMA_OK == (BOOL)true ) &&
				( ((BYTE*)buffer)[0] == 0x02 ) ) 
		{
			iTemp2 = 0;
//			printf("ReadNAND(): exchange OK\n");

			for (iTemp1 = 0 ; iTemp1 < (usNumberPages * 512) ; iTemp1++ )
			{
				((BYTE*)Buffer)[iTemp1] = buffer[iTemp1 + 4];
			}

			break;
		}



		if ( iTemp3++ == 3 )
		{
							
			if ( dwReceivedBytes != dwExpectedBytes )
			{
				iTemp2 = -1 ; // timeout
//				printf("ReadNAND(): exit on timeout\n");
				break;
			}

			if ( ( bCRC_OK != (BOOL)true ) || ( bNORMA_OK != (BOOL)true ) || ( ((BYTE*)buffer)[0] != 0x02 ) )
			{
				iTemp2 = -2 ; // read error
//				printf("ReadNAND(): exit on read error\n");
				break;
			}
			
			iTemp2 = -3 ; // other error

//			printf("ReadNAND(): exit on other error\n");

			break;
			
		}


	}


	delete buffer;


	return iTemp2;


}





// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: ReadFRAM 
PURPOSE: Opens USB device number (usDeviceIndex), reads (usNumberWords) words in FRAM , starting from byte (usStartAddress)
PARAMETERS:	
RETURN VALUE:	0 if success, -1 if timeout, -2 if reading error (CRC  failed, NORMA failed, SYNCHRO failed), -3 if USB error
HISTORY:		17.11.2005	BorisP	created
NOTES:

������ ����� FRAM 64������
������  - ����� 15





*/
// *************************************************************************************
// *************************************************************************************




int FTDI_device::ReadFRAM(
			USHORT usDeviceIndex,	
			USHORT usStartAddress,	
			USHORT usNumberWords,
			LPVOID Buffer )
{

///////////////////////////////
// parameters check


	if ( (USHORT)(usStartAddress + usNumberWords * 2) < (USHORT)usStartAddress )
		return -1;

//	printf("ReadFRAM(): parameters check OK\n");

///////////////////////////////
// preparing query message

	BYTE message[10];
	USHORT usTemp = 0;
	USHORT usOutLength = 10;

	// ���������
	message[0] = 0x02;
	message[1] = 0x0F; // ����� 15

	message[2] = 0x04;
	message[3] = 0x00; // ����� ����������� ����

	message[4] = (BYTE)(usStartAddress & 0xFF );
	message[5] = (BYTE)( usStartAddress >> 8 );	// ��������� �����

	message[6] = (BYTE)(usNumberWords & 0xFF );
	message[7] = (BYTE)( usNumberWords >> 8 ); // ���������� ����

	usTemp = crc16a( (void*) message, 8);
	message[8] = (BYTE)(usTemp & 0xFF );
	message[9] = (BYTE)( usTemp >> 8 );

//	printf("ReadFRAM(): query message prepared\n");


///////////////////////////////
// preparing buffer

	BYTE *buffer = new BYTE[0xFFFF];
	if ( buffer == NULL)
	{
//		printf("ReadFRAM(): not enough memory\n");
		return -3;
	}

//	printf("ReadFRAM(): buffer prepared\n");


///////////////////////////////
// USB transaction

	int iTemp1 = 0;
	int iTemp2 = 0;
	int iTemp3 = 0;

	DWORD dwExpectedBytes = 2 * (DWORD)usNumberWords + 6;
	DWORD dwTimeout = 200.00 + 0.3 * usNumberWords ;

	BOOL bCRC_OK = false;
	BOOL bNORMA_OK = false;
	DWORD dwReceivedBytes = 0;

	while(1)
	{

		bCRC_OK = false;
		bNORMA_OK = false;
		dwReceivedBytes = 0;

		iTemp1 = ExchangeViaUSB(usDeviceIndex , message, usOutLength , (BYTE*)buffer , dwExpectedBytes , dwTimeout , &dwReceivedBytes , &bCRC_OK, &bNORMA_OK );

//		printf("ReadFRAM(): exchange returned %d bytes\n", dwReceivedBytes);


		if (	( dwReceivedBytes == dwExpectedBytes ) &&
				( bCRC_OK == (BOOL)true ) &&
				( bNORMA_OK == (BOOL)true ) &&
				( ((BYTE*)buffer)[0] == 0x02 ) ) 
		{
			iTemp2 = 0;
//			printf("ReadFRAM(): exchange OK\n");

			for (iTemp1 = 0 ; iTemp1 < (usNumberWords * 2) ; iTemp1++ )
			{
				((BYTE*)Buffer)[iTemp1] = buffer[iTemp1 + 4];
			}

			break;
		}



		if ( iTemp3++ == 3 )
		{
							
			if ( dwReceivedBytes < dwExpectedBytes )
			{
				iTemp2 = -1 ; // timeout
//				printf("ReadFRAM(): exit on timeout ( received %d bytes )\n", dwReceivedBytes);
				break;
			}

			if ( ( bCRC_OK != (BOOL)true ) || ( bNORMA_OK != (BOOL)true ) || ( ((BYTE*)buffer)[0] != 0x02 ) )
			{
				iTemp2 = -2 ; // read error
//				printf("ReadFRAM(): exit on read error\n");
				break;
			}
			
			iTemp2 = -3 ; // other error

//			printf("ReadFRAM(): exit on other error\n");

			break;
			
		}


	}


	delete buffer;


	return iTemp2;



}



// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: WriteFRAM 
PURPOSE: Opens USB device number (usDeviceIndex), writes (usNumberWords) words in FRAM , starting from byte (usStartAddress)
PARAMETERS:	
RETURN VALUE:	0 if success, -1 if timeout, -2 if reading error (CRC  failed, NORMA failed, SYNCHRO failed), -3 if USB or other error
HISTORY:		17.11.2005	BorisP	created
NOTES:

������ ����� FRAM 64������
������  - ����� 14





*/
// *************************************************************************************
// *************************************************************************************



int FTDI_device::WriteFRAM(
		USHORT usDeviceIndex,	
		USHORT usStartAddress,	
		USHORT usNumberWords,
		LPVOID Buffer )
{


	int i = 0;

///////////////////////////////////////
// checking the integrity of this FRAM area	

	BYTE *CheckBuffer1 = new BYTE[usNumberWords * 2];

	if ( CheckBuffer1 == NULL )
	{
//		printf("WriteFRAM(): not enough memory\n");
		return -3;
	}

	int iReadResult = ReadFRAM(
			usDeviceIndex,	
			usStartAddress,
			usNumberWords,
			CheckBuffer1 );

	if ( iReadResult != 0 )
	{
//		printf("WriteFRAM(): non integral FRAM area\n");
		return -2;
	}
	
	delete CheckBuffer1;
	
	
///////////////////////////////
// preparing message

	USHORT usOutLength = 10 + usNumberWords * 2;

	
	BYTE *message = new BYTE[usOutLength];
	USHORT usTemp = 0;

	// ���������
	message[0] = 0x02;
	message[1] = 0x0E; // ����� 14

	message[2] = (BYTE)((usOutLength - 6) & 0xFF );
	message[3] = (BYTE)((usOutLength - 6) >> 8 ); // ����� ����������� ����

	message[4] = (BYTE)(usStartAddress & 0xFF );
	message[5] = (BYTE)( usStartAddress >> 8 );	// ��������� �����

	message[6] = (BYTE)(usNumberWords & 0xFF );
	message[7] = (BYTE)( usNumberWords >> 8 ); // ���������� ����

	for ( i = 0 ; i < (2 * usNumberWords ) ; i++ )
	{
		message[8 + i] = ((BYTE*)Buffer)[i];
	}

	usTemp = crc16a( (void*) message, usOutLength - 2);
	message[usOutLength - 2] = (BYTE)(usTemp & 0xFF );
	message[usOutLength - 1] = (BYTE)( usTemp >> 8 );

//	printf("WriteFRAM(): query message prepared\n");
	



///////////////////////////////
// USB transaction

	int iTemp1 = 0;
	int iTemp2 = 0;
	int iTemp3 = 0;
	BYTE buffer[6];

	DWORD dwExpectedBytes = 6;
	DWORD dwTimeout = 200;

	BOOL bCRC_OK = false;
	BOOL bNORMA_OK = false;
	DWORD dwReceivedBytes = 0;

	iTemp1 = ExchangeViaUSB(usDeviceIndex , message, usOutLength , (BYTE*)buffer , dwExpectedBytes , dwTimeout , &dwReceivedBytes , &bCRC_OK, &bNORMA_OK );

	delete message;

	if ( iTemp1 != 1 )
	{
		return -3;
	}


//	printf("WriteFRAM(): exchange returned %d bytes\n", dwReceivedBytes);

	if (	( dwReceivedBytes == dwExpectedBytes ) &&
			( bCRC_OK == (BOOL)true ) &&
			( bNORMA_OK == (BOOL)true ) &&
			( ((BYTE*)buffer)[0] == 0x02 ) ) 
	{	
		return 0;
	}

	if (	( bCRC_OK != (BOOL)true ) ||
			( bNORMA_OK != (BOOL)true ) ||
			( ((BYTE*)buffer)[0] != 0x02 ) ) 
	{	
		return -2;
	}

	if ( dwReceivedBytes < dwExpectedBytes )
	{
		return -1;
	}

	return -3;



}




// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: WriteRAM 
PURPOSE: Opens USB device number (usDeviceIndex), writes (usNumberWords) words in RAM in page (usPageNumber), starting from byte (usOffset)
PARAMETERS:	
RETURN VALUE:	0 if success, -1 if timeout, -2 if reading error (CRC  failed, NORMA failed, SYNCHRO failed), -3 if USB or other error
HISTORY:		17.11.2005	BorisP	created
NOTES:

������ ����� FRAM 64������
������  - ����� 14





*/
// *************************************************************************************
// *************************************************************************************


int FTDI_device::WriteRAM(
	USHORT usDeviceIndex,	
	USHORT usPageNumber,
	USHORT usOffset,	
	USHORT usNumberWords,
	LPVOID Buffer )
{

	
	
	
///////////////////////////////
// checking parameters

	if (
		( usPageNumber > 15 ) ||
		( usOffset > 8191 ) ||
		( ( usNumberWords + usOffset ) > 8192 ) )
	{
		return -1;
	}



///////////////////////////////
// preparing message

	USHORT usOutLength = 12 + usNumberWords * 2;
	int i = 0;
	
	BYTE *message = new BYTE[usOutLength];
	USHORT usTemp = 0;

	// ���������
	message[0] = 0x02;
	message[1] = 0x08; // ����� 8

	message[2] = (BYTE)((usOutLength - 6) & 0xFF );
	message[3] = (BYTE)((usOutLength - 6) >> 8 ); // ����� ����������� ����

	message[4] = (BYTE)( usPageNumber & 0xFF );
	message[5] = (BYTE)( usPageNumber >> 8 );	// ����� ��������

	message[6] = (BYTE)( usOffset & 0xFF );
	message[7] = (BYTE)( usOffset >> 8 );	// �������� �� ��������

	message[8] = (BYTE)( usNumberWords & 0xFF );
	message[9] = (BYTE)( usNumberWords >> 8 ); // ���������� ����

	for ( i = 0 ; i < (2 * usNumberWords ) ; i++ )
	{
		message[10 + i] = ((BYTE*)Buffer)[i];
	}

	usTemp = crc16a( (void*) message, usOutLength - 2);
	message[usOutLength - 2] = (BYTE)(usTemp & 0xFF );
	message[usOutLength - 1] = (BYTE)( usTemp >> 8 );

//	printf("WriteRAM(): query message prepared\n");
	

///////////////////////////////
// USB transaction

	int iTemp1 = 0;
	int iTemp2 = 0;
	BYTE buffer[6];

	DWORD dwExpectedBytes = 6;
	DWORD dwTimeout = 200;

	BOOL bCRC_OK = false;
	BOOL bNORMA_OK = false;
	DWORD dwReceivedBytes = 0;

	iTemp1 = ExchangeViaUSB(usDeviceIndex , message, usOutLength , (BYTE*)buffer , dwExpectedBytes , dwTimeout , &dwReceivedBytes , &bCRC_OK, &bNORMA_OK );

	delete message;

	if ( iTemp1 != 1 )
	{
		return -3;
	}


//	printf("WriteFRAM(): exchange returned %d bytes\n", dwReceivedBytes);

	if (	( dwReceivedBytes == dwExpectedBytes ) &&
			( bCRC_OK == (BOOL)true ) &&
			( bNORMA_OK == (BOOL)true ) &&
			( ((BYTE*)buffer)[0] == 0x02 ) ) 
	{	
		return 0;
	}

	if (	( bCRC_OK != (BOOL)true ) ||
			( bNORMA_OK != (BOOL)true ) ||
			( ((BYTE*)buffer)[0] != 0x02 ) ) 
	{	
		return -2;
	}

	if ( dwReceivedBytes < dwExpectedBytes )
	{
		return -1;
	}

	return -3;








	return 0;

}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: crc16a
PURPOSE: Gets CRC16 for a string
PARAMETERS:	array of bytes and int length
RETURN VALUE: CRC16 calculated
HISTORY:		XX.XX.XXXX	Chivets	created
*/
// *************************************************************************************
// *************************************************************************************

USHORT FTDI_device::crc16a(void *txt, USHORT lll)
{
USHORT i,sum;

sum=0;
for (i=0; i<lll; i++)
   sum=updcrc(*((BYTE *)txt+i),sum);
sum=updcrc(0,sum);
sum=updcrc(0,sum);
sum = (sum >> 8) + (sum << 8);
return sum;
}

// *************************************************************************************
// *************************************************************************************
/*
FUNCTION: updcrc
PURPOSE: Gets CRC16 for a byte
PARAMETERS:	new byte and previous crc
RETURN VALUE: CRC16 calculated
HISTORY:		XX.XX.XXXX	Chivets	created
*/
// *************************************************************************************
// *************************************************************************************


USHORT  FTDI_device::updcrc(USHORT c,USHORT crc)
{
//	register count;
	int count;
for (count=8; --count>=0;)
   {
   if (crc & 0x8000)
	{
	crc <<= 1;
	crc += (((c<<=1) & 0400)  !=  0);
	crc ^= MASK16;
	}
	else
	{
	crc <<= 1;
	crc += (((c<<=1) & 0400)  !=  0);
        }
   }
return crc;
}

