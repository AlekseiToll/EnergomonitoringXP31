#ifndef EMPORT_H
#define EMPORT_H

#include <iterator>
#include "EmServiceClasses.h"

//namespace DeviceIO
//{

#define SLIP_END             0xC0    /* indicates end of packet */
#define SLIP_ESC             0xDB    /* indicates byte stuffing */
#define SLIP_ESC_END         0xDC    /* ESC ESC_END means END data byte */
#define SLIP_ESC_ESC         0xDD    /* ESC ESC_ESC means ESC data byte */

#define		TYPE_PC								0x00
#define		TYPE_ENERGOMONITOR32				0x01
#define		TYPE_ENERGOTESTER					0x02
#define		SENDERTYPE_ENERGOMONITOR32			0x10
#define		SENDERTYPE_ENERGOTESTER				0x20

#define		BROADCAST							0xFFFF

#define		COMMAND_OK							0x1000
#define		COMMAND_UNKNOWN_COMMAND				0x1001	
#define		COMMAND_CRC_ERROR					0x1002	
#define		COMMAND_BAD_DATA					0x1003	
#define		COMMAND_BAD_PASSWORD				0x1004	
#define		COMMAND_ACCESS_ERROR				0x1005	
#define		COMMAND_CHECK_FAILED				0x1006	
#define		COMMAND_NO_DATA						0x1007
#define		COMMAND_ECHO						0x0000
#define		COMMAND_ReadTime					0x0001
#define		COMMAND_ReadCalibration				0x0002
#define		COMMAND_WriteCalibration			0x0003
#define		COMMAND_UpdateArchives				0x0004
#define		COMMAND_Reset						0x0005
#define		COMMAND_SQL							0x0006
#define		COMMAND_ReadSets					0x0007
#define		COMMAND_WriteSets					0x0008
#define		COMMAND_ReadQualityDates			0x0009
#define		COMMAND_ReadQualityEntry			0x000A
#define		COMMAND_ReadSystemData				0x000B
#define		COMMAND_WriteSystemData				0x000C
#define		COMMAND_Read3secValues				0x000D
#define		COMMAND_Read1minValues				0x000E
#define		COMMAND_Read30minValues				0x000F
#define		COMMAND_ReadDipSwellStatus			0x0010
#define		COMMAND_ReadDebugBlock				0x0011
#define		COMMAND_ReadEventLogger				0x0012
#define		COMMAND_Read3secArchiveByTimestamp								0x0013
#define		COMMAND_Read1minArchiveByTimestamp								0x0014
#define		COMMAND_Read30minArchiveByTimestamp								0x0015
#define		COMMAND_ReadAverageArchive3SecIndices							0x4013
#define		COMMAND_ReadAverageArchive10MinIndices							0x4014
#define		COMMAND_ReadAverageArchive2HourIndices							0x4015
#define		COMMAND_ReadQualityDatesByObject								0x001F
//#define		COMMAND_ReadQualityEntryObjectDemand							0x0020
#define		COMMAND_ReadQualityEntryByTimestampByObject						0x0033
#define		COMMAND_ReadDipSwellArchive										0x0019
#define		COMMAND_ReadDipSwellArchiveByObject								0x0021
#define		COMMAND_ReadDipSwellIndexByStartTimestamp						0x001A
#define		COMMAND_ReadDipSwellIndexByEndTimestamp							0x001B

#define		COMMAND_ReadDSIArchivesByRegistration							0x400E
//#define		COMMAND_ReadDipSwellIndexByStartTimestampByObject				0x0022
//#define		COMMAND_ReadDipSwellIndexByEndTimestampByObject					0x0023
#define		COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject		0x0035
#define		COMMAND_Read3secArchiveByTimestampObjectDemand					0x0026
#define		COMMAND_Read1minArchiveByTimestampObjectDemand					0x0027
#define		COMMAND_Read30minArchiveByTimestampObjectDemand					0x0028
#define		COMMAND_ReadEarliestAndLatestAverageTimestamp					0x001E
#define		COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand		0x0025
#define		COMMAND_AverageArchiveQuery										0x0006

#define		COMMAND_ReadRegistrationIndices									0x4009
#define		COMMAND_ReadRegistrationByIndex									0x400A
#define		COMMAND_ReadRegistrationArchiveByIndex							0x400D
#define		COMMAND_ReadAverageArchive3SecByIndex							0x4010
#define		COMMAND_ReadAverageArchive10MinByIndex							0x4011
#define		COMMAND_ReadAverageArchive2HourByIndex							0x4012
#define		COMMAND_ReadAverageArchive3SecIndexByDateTime					0x4016
#define		COMMAND_ReadAverageArchive10MinIndexByDateTime					0x4017
#define		COMMAND_ReadAverageArchive2HourIndexByDateTime					0x4018
#define 	COMMAND_ReadAverageArchive3SecMinMaxIndices						0x4019
#define 	COMMAND_ReadAverageArchive10MinMinMaxIndices					0x401A
#define 	COMMAND_ReadAverageArchive2HourMinMaxIndices					0x401B


#define 	CRC16_SEED														0xFFFF

#define		RXSOCKET_BUF_LEN												(2048)

//#define			HEARTBEAT_TIMER												1

#define USB_EPOUT_ADDRESS 0x02
#define USB_EPIN_ADDRESS 0x86
static GUID CYUSBDRV_GUID = {0xAE18A550,0x7F6A,0x11d4,0x97,0xDD,0x00,0x01,0x02,0x29,0xB9,0x5B}; 

const WORD CRC16Table[] = {
		0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
		0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
		0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
		0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
		0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
		0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
		0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
		0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
		0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
		0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
		0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
		0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
		0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
		0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
		0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
		0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
		0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
		0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
		0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
		0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
		0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
		0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
		0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
		0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
		0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
		0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
		0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
		0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
		0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
		0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
		0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
		0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040
	};

// функция таймера
VOID CALLBACK HeartbeatTimerAPCProcGlobal(LPVOID lpArg,             // Data value
									DWORD dwTimerLowValue,      // Timer low value
									DWORD dwTimerHighValue);

enum ConnectStatus
{
	CONNECTSTATUS_IDLE,
	CONNECTSTATUS_CONNECTING,
	CONNECTSTATUS_FAILED,
	CONNECTSTATUS_CONNECTED,
	CONNECTSTATUS_DISCONNECTED,
	CONNECTSTATUS_FORCEDISCONNECT
};

class EmPortSLIP;

class HeartbeatTimerThread
{ 
	static HANDLE hHeartbeatTimer_;
	static EmPortSLIP* owner_;

public: 
	HeartbeatTimerThread(EmPortSLIP* port)
	{
		owner_ = port;
	}

	static DWORD dwCommunicationTimeout_;
	static DWORD dwHeartbeatCounter_;
	static bool bStopTimer_;

	void HeartbeatTimerAPCProc();

	static void ThreadEntryStatic(HeartbeatTimerThread* pObj)
	{
		pObj->ThreadEntry();
	}

	void ThreadEntry()
	{
		if (hHeartbeatTimer_ = CreateWaitableTimer(NULL,         // Default security attributes
					FALSE,                  // Create auto-reset timer
					"HeartbeatTimer"))      // Name of waitable timer
		{
			BOOL            bSuccess;
			__int64         qwDueTime;
			LARGE_INTEGER   liDueTime;
			try 
			{
				// Create an integer that will be used to signal the timer 
				// 5 seconds from now.
				qwDueTime = -1 * 10000000;

				// Copy the relative time into a LARGE_INTEGER.
				liDueTime.LowPart  = (DWORD) ( qwDueTime & 0xFFFFFFFF );
				liDueTime.HighPart = (LONG)  ( qwDueTime >> 32 );

				bSuccess = SetWaitableTimer(hHeartbeatTimer_, &liDueTime, 1000, 
					HeartbeatTimerAPCProcGlobal, this, FALSE);

				if (bSuccess) 
				{
					while(1)
					{
					   SleepEx(INFINITE, TRUE); 
					   if(bStopTimer_) break;
					}
				} 
				else 
				{
					DWORD errCode = GetLastError();
					std::stringstream ss;
					ss << "SetWaitableTimer failed with Error " << errCode;
					EmService::WriteToLogFailed(ss.str());
				}
			}
			catch(...) 
			{
				EmService::WriteToLogFailed("Error in HeartbeatTimerThread::ThreadEntry");
				throw;
			}

			CancelWaitableTimer(hHeartbeatTimer_);
			CloseHandle(hHeartbeatTimer_);
		} 
		else 
		{
			std::stringstream ss;
			ss << "CreateWaitableTimer failed with Error " << GetLastError();
			EmService::WriteToLogFailed(ss.str());
		}
	}
};

// этот класс дублирован в файлах EmService.cs и DeviceIOEmPortWrap, 
// при каких-либо изменениях надо изменять и там
enum EmDeviceType
{
	NONE = 0,
	EM31K = 1,
    EM32 = 2,
	EM33T = 3,
	EM33T1 = 4,
	ETPQP = 5,
	ETPQP_A = 6
};

// этот класс дублирован в EmService.cs, 
// при каких-либо изменениях надо изменять и там
enum EmPortType
{
	/// <summary>COM port</summary>
	COM = 0,
	/// <summary>USB port</summary>
	USB = 1,
	/// <summary>GSM modem</summary>
	Modem = 2,
	/// <summary>Ethernet</summary>
	Ethernet = 3,
	/// <summary>RS-485</summary>
	Rs485 = 4,
	/// <summary>GPRS modem</summary>
	GPRS = 5,
	/// <summary>Wi-Fi</summary>
	WI_FI = 6
};

enum Protocol
{
	PROTOCOL_LISTENING,
	PROTOCOL_ADDRESS0_EXPECTED,
	PROTOCOL_ADDRESS1_EXPECTED,
	PROTOCOL_ADDRESS2_EXPECTED,
	PROTOCOL_ADDRESS3_EXPECTED,
	PROTOCOL_COMMAND0_EXPECTED,
	PROTOCOL_COMMAND1_EXPECTED,
	PROTOCOL_LENGTH0_EXPECTED,
	PROTOCOL_LENGTH1_EXPECTED,
	PROTOCOL_DATA_EXPECTED,
	PROTOCOL_CRC0_EXPECTED,
	PROTOCOL_CRC1_EXPECTED,
	PROTOCOL_PACKET_BEING_PROCESSED,
	PROTOCOL_ERROR
};

enum Stuff
{
	STUFF_IDLE,
	STUFF_BYTE1,
	STUFF_NEWBYTE,
	STUFF_END
};

class StructReply
{
public: 
	DWORD dwAddress;
	WORD wCommand;
	WORD wLength;
	WORD wCounter;
	BYTE* bData;
	WORD wCRC;

	StructReply(): bData(new BYTE[0x8000]) {}
	~StructReply() { delete[] bData; }
};

class EmPort
{
protected: 
	static EmDeviceType devType_;
	static EmPortType portType_;
	static HWND hMainWnd_;

public:
	virtual bool Open() = 0;
	virtual bool Close() = 0;
	virtual int Write(DWORD size, BYTE* buffer) = 0;

	EmPort(int devType, int portType, int hMainWnd)
	{
		devType_ = (EmDeviceType)devType;
		portType_ = (EmPortType)portType;
		hMainWnd_ = (HWND)hMainWnd;

		//InitializeCriticalSection(&(EmService::scLogFailed));
		//InitializeCriticalSection(&(EmService::scLogGeneral));

		if(!EmService::appDirWasSet)
		{
			TCHAR szPath[MAX_PATH];
			if(!GetModuleFileName(NULL, szPath, MAX_PATH))
			{
				EmService::WriteToLogFailed("GetModuleFileName failed " + EmService::NumberToString(GetLastError())); 
			}
			std::string appDir = std::string(szPath);

			static const std::basic_string<char>::size_type npos = -1;
			std::basic_string<char>::size_type indexSlash = appDir.find_last_of("\\", appDir.length());
			if(indexSlash == npos)
				EmService::WriteToLogFailed("indexSlash == npos\n");
			else appDir = appDir.substr(0, indexSlash + 1);
			EmService::appDirectory = appDir;
			EmService::logFailedName = EmService::appDirectory + EmService::logFailedName;
			EmService::logGeneralName = EmService::appDirectory + EmService::logGeneralName;
			EmService::WriteToLogGeneral("appDir = " + appDir);
			EmService::appDirWasSet = true;
		}

		EmService::WriteToLogGeneral("EmPort constructor");
	}

	virtual ~EmPort() 
	{
		EmService::WriteToLogGeneral("EmPort destructor");

		//DeleteCriticalSection(&(EmService::scLogFailed));
		//DeleteCriticalSection(&(EmService::scLogGeneral));
	}
};

class EmPort33 : public EmPort
{
public: 
	EmPort33(int devType, int portType, int hMainWnd) : EmPort(devType, portType, hMainWnd)
	{
		EmService::WriteToLogGeneral("EmPort33 constructor");
	}

	~EmPort33() 
	{
		EmService::WriteToLogGeneral("EmPort33 destructor");
	}

	virtual int Read(DWORD size, BYTE* buffer) = 0;
};

class EmPortSLIP : public EmPort
{
	enum QueryAvgType
	{
		AAQ_TYPE_ReadStatus = 0,
		AAQ_TYPE_ResetAll = 1,
		AAQ_TYPE_Query = 2
	};

	enum QueryAvgCurStatus
	{
		AAQ_STATE_Idle = 0,
		AAQ_STATE_Busy = 1
	};

protected:
	static const int avgArchiveLength_;

	static WORD devAddress_;

	//HANDLE mainThread_;
	static HANDLE rxThread_;

	static Protocol protocolState_;
	static Stuff stuffState_;
	static StructReply* actualReply_;

	// указатель на функцию, обрабатывающую полученный байт
	void (EmPortSLIP::*InputFuncPointer)(BYTE new_byte);

	static HeartbeatTimerThread* pTimerThread_;
	static HANDLE hTimerThread_;
	static HANDLE hEventCommunicationTimeout_;
	static HANDLE hEventPacketReceived_;
	static HANDLE hEventDisconnected_;
	static HANDLE* hEventsArray_;

	// флаг указывает на то, что при считывании из порта произошла ошибка и поток считывания
	// должен быть остановлен
	static bool innerError_;
	// флаг указывает на то, что идет соединение с модемом. в этом случае мы игнорируем
	// ошибки считывания
	static bool modemConnectInProcess_;
	// флаг указывает на то, что надо остановить RxThread()
	static bool terminateRxThread_;

	static std::string messBuffer_;

	// reading timeout (in seconds)
	static WORD timeoutIO_;

public: 
	EmPortSLIP(int devType, WORD devAddress, int portType, int hMainWnd);
	~EmPortSLIP();

	int ReadData(WORD command, BYTE** buffer, long* arr_params, long params_count,
		long* pAnswerLen);
	int EmPortSLIP::WriteData(WORD command, BYTE* buffer, short bufLength);

	static WORD CalcCrc16(std::vector<BYTE>& data);
	static WORD CalcCrc16(std::vector<BYTE>& buffer, int start, int len);

	BOOL Disconnect()
	{
		EmService::WriteToLogGeneral("EmPortSLIP::Disconnect()");
		return SetEvent(hEventDisconnected_);
	}

protected: 
	static bool CreateBufferToSendRs232(std::vector<BYTE>& buf_temp, BYTE** buf, long* bufLen);

	void RxFunction(BYTE new_byte);

	void RxFunctionConnect(BYTE new_byte)
	{
		messBuffer_ += new_byte;
	}

	static void crc16(BYTE Byte, WORD* crc)
	{
		try
		{
			WORD temp = (WORD)(((Byte & 0x00FF) ^ (*crc)) & 0xFF);
			(*crc) >>= 8;
			(*crc) ^= CRC16Table[temp];
		}
		catch (...)
		{
			EmService::WriteToLogFailed("Error in crc16()");
		}
	}

	void SetTimeoutCounter(WORD timeOut)
	{
		DWORD dwtemp = pTimerThread_->dwHeartbeatCounter_ + timeOut;
		if (dwtemp == 0xFFFFFFFF) dwtemp = 0;
		pTimerThread_->dwCommunicationTimeout_ = dwtemp;
		ResetEvent(hEventCommunicationTimeout_);
		ResetEvent(hEventPacketReceived_);
	}

	void ResetTimeoutCounter()
	{
		pTimerThread_->dwCommunicationTimeout_ = 0xFFFFFFFF;
		ResetEvent(hEventCommunicationTimeout_);
	}

	virtual bool WriteModemInfo(const char* str) = 0;
};

class EmPortComSLIP : public EmPortSLIP
{
protected:
	static HANDLE handle_;
	static std::string portName_;
	static int portSpeed_;

	static BYTE cts_state_;
	static BYTE dsr_state_;
	static BYTE dcd_state_;
	static BYTE ring_state_;

public: 
	EmPortComSLIP(int devType, WORD devAddress, int portType, int hMainWnd) 
		: EmPortSLIP(devType, devAddress, portType, hMainWnd)//,
		//cts_state_(1), dsr_state_(1), dcd_state_(1), ring_state_(1)
	{
		EmService::WriteToLogGeneral("EmPortComSLIP constructor");
	}

	~EmPortComSLIP() 
	{
		EmService::WriteToLogGeneral("EmPortComSLIP destructor");
	}

	int Write(DWORD size, BYTE* buffer);

protected: 

	static void ThreadStart0(PVOID param)
	{
		//((EmPortCom32)param)->Running();	
	}

	void Running();

	static void RxThreadStart(PVOID param)
	{
		((EmPortComSLIP*)param)->RxThread();
	}

	void RxThread();

	void ProcessEvent(UINT EventMask);

	void Purge();
	bool SetTimeOuts(DWORD iTimeOut);
	void SetDtrOff();
	void SetDtrOn();
	int UpdateModemStatus();
};

//}

#endif
