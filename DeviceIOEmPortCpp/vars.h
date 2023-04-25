

#ifndef __VARS_H__
#define __VARS_H__

#define _CRT_SECURE_NO_WARNINGS 


#include <winsock2.h>
#include <windows.h>
#include <commctrl.h>
#include <stdio.h>
#include <tchar.h>
#include "rs232.h"
#include "resource.h"

//#include "../../firmware/arm_common/comm.h"
#include <commdlg.h>


//==================================================================


#define			WindowInitialPositionX					100
#define			WindowInitialPositionY					100
#define			WindowInitialWidth						800
#define			WindowInitialHeight						700+10+20

extern HWND *phWnd;
extern HINSTANCE *phInst;

extern DWORD WindowPositionX;
extern DWORD WindowPositionY;


//==================================================================

extern void CreateStaticControls(HWND hWnd);


extern HWND hSta_DeviceType;
extern HWND hBtn_DeviceType_EM32;
extern HWND hBtn_DeviceType_ETPKE;
extern HWND hBtn_DeviceType_EF30;
extern HWND hBtn_DeviceType_EM30;
extern HWND hBtn_DeviceType_ETPKEA;
extern HWND hBtn_Mode_BIOS;
extern HWND hBtn_Mode_WORK;
extern HWND hBtn_Interface_RS232;
extern HWND hBtn_Interface_MODEM;
extern HWND hBtn_Interface_ETHERNET;
extern HWND hBtn_Interface_USB;
extern HWND hCmb_ComPort;
extern HWND hCmb_BaudRate;
extern HWND hEdt_Telephone;
extern HWND hEdt_TcpPort;
extern HWND hEdt_IP1;
extern HWND hEdt_IP2;
extern HWND hEdt_IP3;
extern HWND hEdt_IP4;
extern HWND hBtn_ConnectDisconnect;
extern HWND hSta_ConnectStatus;
extern HWND hSta_GlobalStatus;
extern HWND hEdt_Serial;
extern HWND hBtn_Unicast;
extern HWND hSta_Rx;
extern HWND hSta_Tx;
extern HWND hSta_ErrorCounter;
extern HWND hSta_TimeoutCounter;
extern HWND hBtn_StopCommunication;
extern HWND hPBar_ProgressBar_Complete;
extern HWND hPBar_ProgressBar_Tx;
extern HWND hPBar_ProgressBar_Rx;
extern HWND hSta_Interface;
extern HWND hSta_ComPort;
extern HWND hSta_Telephone;
extern HWND hSta_IP;
extern HWND hSta_Mode;
extern HWND hSta_Serial;
extern HWND hSta_Port;
extern HWND hStatusBar;
extern HWND hSta_MAC;
extern HWND hEdt_MAC;
extern HWND hBtn_GetIP;

//==================================================================





extern void ScreenModeInterfaceUpdate(void);
extern void ScreenModeComPortUpdate(void);

//==================================================================

enum DeviceTypes
{
DEVICE_TYPE_EM32,
DEVICE_TYPE_ETPKE,
DEVICE_TYPE_EF30,
DEVICE_TYPE_EM30,
DEVICE_TYPE_ETPKEA
};

enum Modes
{
MODE_BIOS,
MODE_WORK
};
enum Interfaces
{
INTERFACE_RS232,
INTERFACE_MODEM,
INTERFACE_ETHERNET,
INTERFACE_USB
};
enum ComPorts
{
COMPORT1,
COMPORT2,
COMPORT3,
COMPORT4,
COMPORT5,
COMPORT6,
COMPORT7,
COMPORT8,
COMPORT9,
COMPORT10,
COMPORT11,
COMPORT12,
COMPORT13,
COMPORT14,
COMPORT15,
COMPORT16,
COMPORT17,
COMPORT18,
COMPORT19,
COMPORT20,
COMPORT21,
COMPORT22,
COMPORT23,
COMPORT24,
COMPORT25,
COMPORT26,
COMPORT27,
COMPORT28,
COMPORT29,
COMPORT30,
COMPORT31,
COMPORT32
};

enum BaudRates
{
BR_4800,
BR_9600,
BR_19200,
BR_38400,
BR_57600,
BR_115200,
BR_460800,
BR_3M,
};


extern DeviceTypes GlobalDeviceType;
extern Modes GlobalMode; 
extern Interfaces GlobalInterface;
extern ComPorts GlobalComPort;
extern BaudRates GlobalBaudRate;
extern WORD GlobalTcpPort;
extern BYTE GlobalTelNumber[11];
extern WORD GlobalIpAddress[4];
extern WORD GlobalUnicastSerial;
extern WORD GlobalSerial;
extern BYTE abMAC[6];
extern DWORD dwMinIP;
extern DWORD dwMaxIP;

extern HANDLE hThread_TcpUdpReceiver;
extern DWORD WINAPI ThreadFunc_TcpUdpReceiver(LPVOID lpv);
extern WSAOVERLAPPED RecvOverlapped;
extern WSAEVENT hTcpEvent;
extern WSAEVENT hUdpEvent;


//==================================================================

extern HWND hMainTab;

extern WORD wProgress_Complete;
extern WORD wProgress_Rx;
extern WORD wProgress_Tx;

//==================================================================

extern void GetNonVolatileDataFromRegistry(HWND hWnd);
extern bool SaveNonVolatileDataToRegistry(HWND hWnd);


//==================================================================

//extern void OnConnect(void);
//extern void OnDisconnect(void);


//==================================================================
#define			HEARTBEAT_TIMER				1
#define			UPDATE_TIMER				2
#define			PPS_TIMER					3

extern CRITICAL_SECTION csUpdateStatus;
extern char sGlobalStatus[128];
extern char sConnectStatus[128];



//==================================================================




//____________________________________________________

extern WORD wIndentificationStartAddress;
extern WORD wIndentificationEndAddress;

extern DWORD WINAPI ThreadFunc_BroadcastIdentification(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_SynchronizeTime(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_Identification(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_RestartWORK(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_RestartBIOS(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_RestartDSP(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_ReadSettings_EM32(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadSettings_ETPKE(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadSettings_EF30(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadSettings_EM30(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_EditParameter_EM32(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_EditParameter_ETPKE(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_EditParameter_EF30(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_EditParameter_EM30(LPVOID lpv);


extern DWORD WINAPI ThreadFunc_ReadEEPROM(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadNOR(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadSDRAM(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadNAND(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_LoadARMBODY_EM32(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadARMBODY_ETPKE(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadARMBODY_ETPKEA(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadARMBODY_EF30(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadFirmware_ETPKE(LPVOID lpv);


extern DWORD WINAPI ThreadFunc_LoadDSPBODY_EM32(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadDSPBODY_ETPKE(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadDSPBODY_EF30(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_LoadFPGA_EM32(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadFPGA_EF30(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_LoadBOOTLOADER_EM32(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadBOOTLOADER_ETPKE(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_LoadBOOTLOADER_EF30(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_LoadCONSTANTS_EF30(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_ReadScreen(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadSets(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_WriteSets(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadAdcImage(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_ReadTelemetrySettings(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_WriteTelemetrySettings(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_RequestMeasurements(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadDipSwellInfo(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadEventLogger(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadQuality(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadEmeterArchives(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadOscilloscopeArchives(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_ReadAverage(LPVOID lpv);

extern DWORD WINAPI ThreadFunc_ReadObjects(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_RegistrationStart(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_RegistrationStop(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_OscilloscopingStart(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_OscilloscopingStop(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadObjectData(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadTraces(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_ReadQTemplates(LPVOID lpv);



enum Communication
{
COMM_IDLE,
COMM_FORCESTOP,
COMM_RUNNING
};

extern DWORD dwCommunicationStatus;

extern DWORD dwRxByteCounter;
extern DWORD dwTxByteCounter;
extern DWORD dwRxPacketCounter;
extern DWORD dwTxPacketCounter;
extern DWORD dwRxErrorCounter;
extern DWORD dwRxTimeoutCounter;


/*enum Stuff
{
STUFF_IDLE,
STUFF_BYTE1, 
STUFF_NEWBYTE,
STUFF_END,
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
};

extern Stuff StuffState;
extern Protocol ProtocolState;*/

typedef struct
{
	DWORD dwAddress;
	WORD wCommand;
		DWORD wpad0;
	WORD wLength;
		DWORD wpad1;
	BYTE bData[0x8000];
		DWORD wpad2;
	WORD wCRC;
} StructRequest;

typedef struct
{
	DWORD dwAddress;
	WORD wCommand;
		DWORD wpad0;
	WORD wLength;
		DWORD wpad1;
	WORD wCounter;
		DWORD wpad2;
	BYTE bData[0x8000];
		DWORD wpad3;
	WORD wCRC;
} StructReply;
extern StructReply ActualReply;
extern void SendRequest( StructRequest *Reuest);

extern DWORD dwCommunicationTimeout;

//==================================================================

enum Connection
{
DISCONNECTED,
CONNECTING,
CONNECTED,
};

extern rs232_port *comx;
extern Connection ConnectionState;

extern void crc16(BYTE byte, WORD *crc);
extern SOCKET TcpClientSocket;
extern SOCKADDR_IN TcpServerInfo;
extern DWORD dwServerIpAddress;
extern WSADATA wsaData;
extern WSAOVERLAPPED RecvOverlapped;

#define 		CRC16_SEED		0xFFFF
const WORD CRC16Table[] =
{
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

//extern DWORD ConnectStatus;
//extern DWORD PreviousConnectStatus;

enum
{
CONNECTSTATUS_IDLE,
CONNECTSTATUS_CONNECTING,
CONNECTSTATUS_FAILED,
CONNECTSTATUS_CONNECTED,
CONNECTSTATUS_DISCONNECTED,
CONNECTSTATUS_FORCEDISCONNECT,

};

extern HANDLE hThreadConnect_Rs232;
extern HANDLE hThreadConnect_Modem;
extern HANDLE hThreadConnect_UDP;
extern HANDLE hThreadConnect_TCP;
extern HANDLE hThreadConnect_USB;

extern DWORD WINAPI ThreadConnectFunc_Rs232(LPVOID lpv);
extern DWORD WINAPI ThreadConnectFunc_Modem(LPVOID lpv);
extern DWORD WINAPI ThreadConnectFunc_UDP(LPVOID lpv);
extern DWORD WINAPI ThreadConnectFunc_TCP(LPVOID lpv);
extern DWORD WINAPI ThreadConnectFunc_USB(LPVOID lpv);

extern HANDLE hConsoleOutput;
extern void RxFuncModemConnecting(BYTE new_byte); 

extern DWORD dwHeartbeatCounter;
extern DWORD dwConnectTimeout;

extern SOCKET UdpClientSocket;
extern SOCKADDR_IN UdpServerInfo;

extern void RxFunction(BYTE new_byte);
extern void TxFunction(BYTE new_byte);
extern void TxFunctionSlip(BYTE new_byte);

//==================================================================
/*
#define SLIP_END             0xC0 
#define SLIP_ESC             0xDB 
#define SLIP_ESC_END         0xDC 
#define SLIP_ESC_ESC         0xDD 

#define		TYPE_PC								0x00
#define		TYPE_ENERGOMONITOR32				0x01
#define		TYPE_ENERGOTESTERPKE				0x02
#define		TYPE_ENERGOFORMA30					0x03
#define		TYPE_ENERGOMONITOR30				0x04
#define		TYPE_BROADCAST						0xFF

#define		BROADCAST										0xFFFF

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
#define		COMMAND_WriteTime					0x0002
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
#define		COMMAND_ReadSystemDataEx				0x0030
#define		COMMAND_WriteSystemData				0x000C
#define		COMMAND_Read3secValues				0x000D
#define		COMMAND_Read1minValues				0x000E
#define		COMMAND_Read30minValues				0x000F
#define		COMMAND_ReadDipSwellStatus			0x0010
#define		COMMAND_ReadDebugBlock				0x0011
#define		COMMAND_ReadEventLogger				0x0012
#define		COMMAND_Read3secArchiveByTimestamp	0x0013
#define		COMMAND_Read1minArchiveByTimestamp	0x0014
#define		COMMAND_Read30minArchiveByTimestamp	0x0015
#define		COMMAND_RestartWORK					0x0016
#define		COMMAND_RestartBIOS					0x0017
#define		COMMAND_RestartDSP					0x0018
#define		COMMAND_ReadDipSwellArchive							0x0019
#define		COMMAND_ReadDipSwellIndexByStartTimestamp			0x001A
#define		COMMAND_ReadDipSwellIndexByEndTimestamp				0x001B
#define		COMMAND_ReadEarliestAndLatestDipSwellTimestamp 		0x001C
#define		COMMAND_ReadEarliestAndLatestDipSwellTimestampForEveryObject 0x0031

#define		COMMAND_FreezeDSP									0x001D


#define 	COMMAND_ReadObjectsEntries									0x0024
#define		COMMAND_ReadDipSwellMinimalAndMaximalIndexes				0x0029
#define		COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand	0x0025
#define 	COMMAND_ReadEarliestAndLatestOscTimestampObjectDemand 		0x002b
#define 	COMMAND_ReadOscAddrObjectDemand 							0x002c



#define		COMMAND_ReadEEPROM					0x2000
#define		COMMAND_WriteEEPROM					0x2001

#define		COMMAND_ReadNOR						0x2010
#define		COMMAND_EraseNOR					0x2011
#define		COMMAND_WriteNOR					0x2012


#define		COMMAND_ReadSDRAM					0x2020

#define		COMMAND_ReadRawNAND					0x2030
#define		COMMAND_EraseRawNAND				0x2031
#define		COMMAND_WriteRawNAND				0x2032

#define		COMMAND_ReadLbaNAND					0x2040
#define		COMMAND_EraseLbaNAND				0x2041
#define		COMMAND_WriteLbaNAND				0x2042

#define		COMMAND_FreezeLCD					0x2043
#define		COMMAND_StartLCD					0x2044


#define		COMMAND_ReadEarliestAndLatestAverageTimestamp	0x001E
#define		COMMAND_AverageArchiveQuery						0x0006

#define 	COMMAND_ReadQualityDatesObjectDemand						0x001F
#define 	COMMAND_ReadQualityEntryObjectDemand						0x0020

#define		COMMAND_ReadAllObjects									0x0024
#define		COMMAND_ReadMinimalAndMaximalObjectIDs					0x002D
#define		COMMAND_ReadObjectsByID									0x002E
#define		COMMAND_ReadActualObject								0x002F


*/




//==================================================================

//extern void UpdateSysDataOnScreen(WORD item);
extern void UpdateSysDataOnScreen_EM32(WORD item);
extern void UpdateSysDataOnScreen_ETPKE(WORD item);
extern void EditSettings(int item);
extern void EditSettingsShowInfo(int item);

extern BOOL EditParameterMode;
extern void EditParameterUpdateScreen_EM32(void);
extern void EditParameterUpdateScreen_ETPKE(void);
extern void EditParameterUpdateScreen_EF30(void);
extern void EditParameterUpdateScreen_EM30(void);


extern WORD EditParameterNumber;
extern BOOL EditParameterCheck(void);
extern void EditParameterCheckEdit(void);


//==================================================================

extern WORD wEEPROM_FirstPage;
extern WORD wEEPROM_LastPage;
extern WORD wEEPROM_NumPages;
extern const WORD wEEPROM_MaxPage[5];

extern void EnableScreen_ReadMemory(void);
extern void DisableScreen_ReadMemory(void);

extern WORD wNAND_FirstBlock;
extern WORD wNAND_LastBlock;
extern WORD wNAND_NumBlocks;
extern BOOL NAND_RawMode;


extern BYTE *PreSendBuffer;
extern FILE *debugfile;


extern HANDLE hEventsArray[3];
#define		hEventCommunicationTimeout			hEventsArray[0]
#define		hEventPacketReceived				hEventsArray[1]
#define		hEventDisconnected					hEventsArray[2]

extern void EnableScreen_Identification(void);
extern void DisableScreen_Identification(void);
extern DWORD dwDurationCounter;



extern char CurrentDirectory[MAX_PATH];


extern SYSTEMTIME SystemTime;
extern SYSTEMTIME LocalTime;
extern TIME_ZONE_INFORMATION TimeZoneInformation;
extern DWORD SummerWinterTime;

typedef struct 
{
	char Description[64];
	SYSTEMTIME RTC;
	SYSTEMTIME UTC;
	SHORT Bias;
	BYTE SummerWinter;
} IdentificationInfo;

extern IdentificationInfo IdentificationInfoArray[100];
extern WORD IdentificationInfoLength;
extern WORD PrevIdentificationInfoLength;

extern void IncrementTime(SYSTEMTIME *SysTime);

extern HFONT hfont1;
extern HFONT hfont2;
extern HFONT hfont3;

extern WORD *RawScreen;
extern BOOL CALLBACK ScreenDialogProcETPKE(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);
extern BOOL CALLBACK ScreenDialogProcEF30(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);
extern HBITMAP hScreenBMP;

typedef struct 
{
	BYTE B;
	BYTE G;
	BYTE R;
} ScreenPixel;
extern ScreenPixel *ScreenPixels;


#define		MESSAGE_UnableWhenDisconnected		" ОШИБКА : прибор не подключен "
#define		MESSAGE_Timeout						" ОШИБКА : прибор не отвечает "


extern OPENFILENAME sSaveFileName_Screen;
extern char SaveFileName_Screen[MAX_PATH];
extern char SaveFileNameWithoutPath_Screen[MAX_PATH];
extern OPENFILENAME sSaveFileName_AdcImage;
extern char SaveFileName_AdcImage[MAX_PATH];
extern char SaveFileNameWithoutPath_AdcImage[MAX_PATH];
extern OPENFILENAME sOpenFileName_Firmware;
extern char OpenFileName_Firmware[MAX_PATH];
extern char OpenFileNameWithoutPath_Firmware[MAX_PATH];
extern OPENFILENAME sOpenFileName_Sets;
extern char OpenFileName_Sets[MAX_PATH];
extern char OpenFileNameWithoutPath_Sets[MAX_PATH];
extern OPENFILENAME sSaveFileName_Sets;
extern char SaveFileName_Sets[MAX_PATH];
extern char SaveFileNameWithoutPath_Sets[MAX_PATH];


extern WORD *AdcImage;

extern void DrawColouredStatus(void);

//----------------------------------------------
extern TOOLINFO ToolTipInfo;

extern HWND hTTip_hEdt_Telephone;
extern HWND hTTip_hEdt_Serial;
extern HWND hTTip_hBtn_BroadcastSerial;
extern HWND hTTip_hSta_RxCounter;
extern HWND hTTip_hSta_TxCounter;
extern HWND hTTip_hSta_ErrorCounter;
extern HWND hTTip_hSta_TimeoutCounter;

extern LPTSTR sToolTipText_hEdt_Telephone;
extern LPTSTR sToolTipText_hEdt_Serial;
extern LPTSTR sToolTipText_hBtn_BroadcastSerial;
extern LPTSTR sToolTipText_hSta_RxCounter;
extern LPTSTR sToolTipText_hSta_TxCounter;
extern LPTSTR sToolTipText_hSta_ErrorCounter;
extern LPTSTR sToolTipText_hSta_TimeoutCounter;

//----------------------------------------------

typedef struct 
{
	WORD length; // parameter data length (words)
	WORD access; // access type (ACCESS_READONLY=0,ACCESS_USER=1,ACCESS_MARSENERGO=2)
	CHAR name[64]; // 
	BYTE raw_value[64];
	BYTE raw_length;
	CHAR txt_value[64];
	WORD device_serial;
	WORD device_type;
	CHAR description[256];
} TSysDataItem;


extern void SysDataInit(void);
extern void SysDataScreenUpdate(void);
extern WORD SysDataScreenPointer;
extern WORD SysDataScreenLength;
extern TSysDataItem SysData_EM32[];
extern TSysDataItem SysData_ETPKE[];
extern TSysDataItem SysData_EF30[];
extern TSysDataItem SysData_EM30[];
extern TSysDataItem *pActualSysData;
extern WORD ActualSysDataSize;
extern void SysDataScreenShowInfo(void);


extern void SysDataScreenShowList_EM32(void);
extern void SysDataScreenShowList_ETPKE(void);
extern void SysDataScreenShowList_EF30(void);
extern void SysDataScreenShowList_EM30(void);
extern int SysDataScreenCurrentSelection;


#define		MAX_SIZE_OF_SYSDATA					1024
extern WORD SysDataScreenArray[MAX_SIZE_OF_SYSDATA];

extern WORD SysDataFilter_ReadOnly;
extern WORD SysDataFilter_UserEditable;
extern WORD SysDataFilter_VendorEditable;

extern void SysDataEdit(void);
extern void SysDataEdit_EM32(void);
extern void SysDataEdit_ETPKE(void);



#define 		CRC16_SEED_EM32_ARMBODY				(0xFFFF-0x0000)
#define 		CRC16_SEED_EM32_DSPBODY				(0xFFFF-0x0001)
#define 		CRC16_SEED_EM32_FPGA				(0xFFFF-0x0002)
#define 		CRC16_SEED_EM32_BOOTLOADER			(0xFFFF-0x0003)
#define 		CRC16_SEED_ETPKE_ARMBODY			(0xFFFF-0x0004)
#define 		CRC16_SEED_ETPKE_DSPBODY			(0xFFFF-0x0005)
#define 		CRC16_SEED_ETPKE_BOOTLOADER			(0xFFFF-0x0006)
#define 		CRC16_SEED_EF30_MASTER				(0xFFFF-0x0007)			
#define 		CRC16_SEED_EF30_DSP					(0xFFFF-0x0008)
#define 		CRC16_SEED_EF30_FPGA				(0xFFFF-0x0009)
#define 		CRC16_SEED_EF30_BOOTLOADER			(0xFFFF-0x000A)
#define 		CRC16_SEED_EF30_CONSTANTS			(0xFFFF-0x000B)
#define 		CRC16_SEED_ETPKE2_BOOTLOADER			(0xFFFF-0x000C)
#define 		CRC16_SEED_ETPKE2_ARMBODY			(0xFFFF-0x000D)
#define 		CRC16_SEED_ETPKE2_DSPBODY			(0xFFFF-0x000E)


//================================================================================

extern float fGlobalCurrentNominal;
extern float fGlobalVoltageNominal;

extern float fGlobalCurrentPrimary;
extern float fGlobalVoltagePrimary;

extern float fGlobalCurrentTransformationRatio;
extern float fGlobalVoltageTransformationRatio;

extern WORD GlobalTelemetryOut1Enable;
extern WORD GlobalTelemetryOut2Enable;
extern WORD GlobalTelemetryOut3Enable;

extern WORD GlobalTelemetryOut1Parameter;
extern WORD GlobalTelemetryOut2Parameter;
extern WORD GlobalTelemetryOut3Parameter;

extern WORD GlobalTelemetryOut1Condition;
extern WORD GlobalTelemetryOut2Condition;
extern WORD GlobalTelemetryOut3Condition;

extern float fGlobalTelemetryOut1Value;
extern float fGlobalTelemetryOut2Value;
extern float fGlobalTelemetryOut3Value;

extern DWORD dwGlobalTelemetryOut1Value;
extern DWORD dwGlobalTelemetryOut2Value;
extern DWORD dwGlobalTelemetryOut3Value;


extern void UpdateTelemetryScreen(void);
extern void TelemetryInit(void);
extern void CheckTelemetryValue(HWND hEdit, float *fValue, WORD Parameter);
extern void ConvertTelemetryValue_dword2float(WORD Parameter, DWORD *dwValue,float *fValue);
extern void ConvertTelemetryValue_float2dword(WORD Parameter, float *fValue,DWORD *dwValue);

extern void PhasingImage_AddPoint(int x,int y,COLORREF color,int thickness);
extern COLORREF PhasingImage[200][200];
extern BOOL CALLBACK PhasingDialogProc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);

extern BOOL CheckPhasing(float UAB,float UBC,float UCA,float UIA,float UIB,float UIC,WORD UTtype,WORD IType,WORD LineType);

#define		PHASE_ABC			0
#define		PHASE_BCA			1
#define		PHASE_CAB			2
#define		PHASE_BAC			3
#define		PHASE_CBA			4
#define		PHASE_ACB			5

#define		PHASE_aBC			6
#define		PHASE_AbC			7
#define		PHASE_ABc			8

#define		PHASE_bCA			9
#define		PHASE_BcA			10
#define		PHASE_BCa			11

#define		PHASE_cAB			12
#define		PHASE_CaB			13
#define		PHASE_CAb			14

#define		PHASE_bAC			15
#define		PHASE_BaC			16
#define		PHASE_BAc			17

#define		PHASE_cBA			18
#define		PHASE_CbA			19
#define		PHASE_CBa			20

#define		PHASE_aCB			21
#define		PHASE_AcB			22
#define		PHASE_ACb			23


//-------------------------------------------------------------------------------------

typedef	struct 
{
	WORD Type;	
	WORD DateDDMM;
	WORD Date__YY;
	WORD TimeHHMM;
	WORD Time__SS;
	WORD Time__ms;
	WORD MaxMinValue;
	WORD dummy;
} TDipSwellRawEntry;

#define		DipSwellEntryType_UA_Offset						0x1000
#define		DipSwellEntryType_UB_Offset						0x2000
#define		DipSwellEntryType_UC_Offset						0x3000
#define		DipSwellEntryType_UAB_Offset					0x4000
#define		DipSwellEntryType_UBC_Offset					0x5000
#define		DipSwellEntryType_UCA_Offset					0x6000
#define		DipSwellEntryType_UA_DipStart					DipSwellEntryType_UA_Offset + 0
#define		DipSwellEntryType_UA_SwellStart					DipSwellEntryType_UA_Offset + 1
#define		DipSwellEntryType_UA_NormalStart				DipSwellEntryType_UA_Offset + 2
#define		DipSwellEntryType_UA_DipEnd						DipSwellEntryType_UA_Offset + 3
#define		DipSwellEntryType_UA_SwellEnd					DipSwellEntryType_UA_Offset + 4
#define		DipSwellEntryType_UA_Dip						DipSwellEntryType_UA_Offset + 5
#define		DipSwellEntryType_UA_Swell						DipSwellEntryType_UA_Offset + 6
#define		DipSwellEntryType_UB_DipStart					DipSwellEntryType_UB_Offset + 0
#define		DipSwellEntryType_UB_SwellStart					DipSwellEntryType_UB_Offset + 1
#define		DipSwellEntryType_UB_NormalStart				DipSwellEntryType_UB_Offset + 2
#define		DipSwellEntryType_UB_DipEnd						DipSwellEntryType_UB_Offset + 3
#define		DipSwellEntryType_UB_SwellEnd					DipSwellEntryType_UB_Offset + 4
#define		DipSwellEntryType_UB_Dip						DipSwellEntryType_UB_Offset + 5
#define		DipSwellEntryType_UB_Swell						DipSwellEntryType_UB_Offset + 6
#define		DipSwellEntryType_UC_DipStart					DipSwellEntryType_UC_Offset + 0
#define		DipSwellEntryType_UC_SwellStart					DipSwellEntryType_UC_Offset + 1
#define		DipSwellEntryType_UC_NormalStart				DipSwellEntryType_UC_Offset + 2
#define		DipSwellEntryType_UC_DipEnd						DipSwellEntryType_UC_Offset + 3
#define		DipSwellEntryType_UC_SwellEnd					DipSwellEntryType_UC_Offset + 4
#define		DipSwellEntryType_UC_Dip						DipSwellEntryType_UC_Offset + 5
#define		DipSwellEntryType_UC_Swell						DipSwellEntryType_UC_Offset + 6
#define		DipSwellEntryType_UAB_DipStart					DipSwellEntryType_UAB_Offset + 0
#define		DipSwellEntryType_UAB_SwellStart				DipSwellEntryType_UAB_Offset + 1
#define		DipSwellEntryType_UAB_NormalStart				DipSwellEntryType_UAB_Offset + 2
#define		DipSwellEntryType_UAB_DipEnd					DipSwellEntryType_UAB_Offset + 3
#define		DipSwellEntryType_UAB_SwellEnd					DipSwellEntryType_UAB_Offset + 4
#define		DipSwellEntryType_UAB_Dip						DipSwellEntryType_UAB_Offset + 5
#define		DipSwellEntryType_UAB_Swell						DipSwellEntryType_UAB_Offset + 6
#define		DipSwellEntryType_UBC_DipStart					DipSwellEntryType_UBC_Offset + 0
#define		DipSwellEntryType_UBC_SwellStart				DipSwellEntryType_UBC_Offset + 1
#define		DipSwellEntryType_UBC_NormalStart				DipSwellEntryType_UBC_Offset + 2
#define		DipSwellEntryType_UBC_DipEnd					DipSwellEntryType_UBC_Offset + 3
#define		DipSwellEntryType_UBC_SwellEnd					DipSwellEntryType_UBC_Offset + 4
#define		DipSwellEntryType_UBC_Dip						DipSwellEntryType_UBC_Offset + 5
#define		DipSwellEntryType_UBC_Swell						DipSwellEntryType_UBC_Offset + 6
#define		DipSwellEntryType_UCA_DipStart					DipSwellEntryType_UCA_Offset + 0
#define		DipSwellEntryType_UCA_SwellStart				DipSwellEntryType_UCA_Offset + 1
#define		DipSwellEntryType_UCA_NormalStart				DipSwellEntryType_UCA_Offset + 2
#define		DipSwellEntryType_UCA_DipEnd					DipSwellEntryType_UCA_Offset + 3
#define		DipSwellEntryType_UCA_SwellEnd					DipSwellEntryType_UCA_Offset + 4
#define		DipSwellEntryType_UCA_Dip						DipSwellEntryType_UCA_Offset + 5
#define		DipSwellEntryType_UCA_Swell						DipSwellEntryType_UCA_Offset + 6

typedef struct
{
	DWORD EventIndex;
	WORD Type;
	WORD Value;
	WORD StartTime[5];
	WORD EndTime[5];
	WORD Duration[4];

	DWORD StartSecs;
	DWORD EndSecs;
	WORD StartDays;
	WORD EndDays;
	DWORD DurationSecs;
	DWORD fUphNominal;
	DWORD fUlinNominal;

	WORD ObjectID;
	WORD Crc;
} TDipSwellEntry;


extern DWORD dwConnectionTimeCounter;

typedef struct
{
	DWORD dwIndex;
	WORD wType;
	WORD UtcDateYYYY;
	BYTE UtcDateDD;
	BYTE UtcDateMM;
	BYTE UtcTimeMM;
	BYTE UtcTimeHH;
	WORD UtcTime__SS;
	WORD RtcDateYYYY;
	BYTE RtcDateDD;
	BYTE RtcDateMM;
	BYTE RtcTimeMM;
	BYTE RtcTimeHH;
	WORD RtcTime__SS;
	SHORT RtcLocalTimeBias;
	WORD RtcSummerTimeFlag;
	WORD wAuxData0;
	WORD wAuxData1;
	WORD wCrc;
} TEventLoggerEntry;



typedef struct
{
	WORD Year;
	BYTE Date;
	BYTE Month;
	BYTE StartHour;
	BYTE StartMimute;
	WORD StartSecond;
	BYTE EndHour;
	BYTE EndMimute;
	WORD EndSecond;
	char ObjectName[16];



} TQualityArchiveHeader;


typedef struct 
{
	WORD MinTimestamp[4];
	WORD MaxTimestamp[4];
	WORD ObjectID;
	WORD Reserved;
	WORD dFCounter;
	WORD dUxCounter;
	BYTE dFArrayValid[540];
	BYTE dUxArrayValid[180];
	SHORT dFArray[4320];
	SHORT dUyArray[1440];
	SHORT dUAArray[1440];
	SHORT dUABArray[1440];
	SHORT dUBArray[1440];
	SHORT dUBCArray[1440];
	SHORT dUCArray[1440];
	SHORT dUCAArray[1440];
//	BYTE Fill[3228];

} TQualityVFArray;


//-------------------------------------------------------------------------------------

extern void Average_ScreenUpdate(void);
extern void Average_TimeUpdate(void);
extern void Average_DateUpdate(void);
extern void Average_NumberOfEntriesUpdate(void);

extern BYTE AverageEarliest_Date;
extern BYTE AverageEarliest_Month;
extern BYTE AverageEarliest_Year;
extern BYTE AverageEarliest_Hour;
extern BYTE AverageEarliest_Minute;
extern BYTE AverageEarliest_Second;

extern BYTE AverageLatest_Date;
extern BYTE AverageLatest_Month;
extern BYTE AverageLatest_Year;
extern BYTE AverageLatest_Hour;
extern BYTE AverageLatest_Minute;
extern BYTE AverageLatest_Second;

extern BYTE AverageArchiveType;
extern BYTE AverageQueryType;

extern int AverageNumberOfEntries;


#define	AAQ_TYPE_ReadStatus				0x0000
#define	AAQ_TYPE_ResetAll				0x0001
#define	AAQ_TYPE_Query					0x0002

//-------------------------------------------------------------------------------------


//======================================================================

#define	PHASE_ABC		0
#define	PHASE_A			1
#define	PHASE_B			2
#define	PHASE_C			3
#define	PHASE_X			4

#define	VALIDITY_PROGRESSIVETOTAL			(0x5555+0)
#define	VALIDITY_15MIN						(0x5555+1)


struct DateTime
{
	BYTE hour;
	BYTE minute;
	BYTE second;
	BYTE date;
	BYTE month;
	BYTE year;
	BYTE weekday;
	WORD millisecond;
};

typedef struct
{
	WORD EmeterArchivePointerCopy;

	WORD ValidityFlag;
	struct DateTime LastTimestamp;

	LONGLONG TotalEnergy_P1[PHASE_X];
	LONGLONG TotalEnergy_P2[PHASE_X];
	LONGLONG TotalEnergy_P3[PHASE_X];
	LONGLONG TotalEnergy_P4[PHASE_X];
	LONGLONG TotalEnergy_Pimport[PHASE_X];
	LONGLONG TotalEnergy_Pexport[PHASE_X];
	LONGLONG TotalEnergy_Psum[PHASE_X];
	LONGLONG TotalEnergy_Pdiff[PHASE_X];
	LONGLONG TotalEnergy_Q1[PHASE_X];
	LONGLONG TotalEnergy_Q2[PHASE_X];
	LONGLONG TotalEnergy_Q3[PHASE_X];
	LONGLONG TotalEnergy_Q4[PHASE_X];
	LONGLONG TotalEnergy_Qimport[PHASE_X];
	LONGLONG TotalEnergy_Qexport[PHASE_X];
	LONGLONG TotalEnergy_Qsum[PHASE_X];
	LONGLONG TotalEnergy_Qdiff[PHASE_X];
	LONGLONG TotalEnergy_S1[PHASE_X];
	LONGLONG TotalEnergy_S2[PHASE_X];
	LONGLONG TotalEnergy_S3[PHASE_X];
	LONGLONG TotalEnergy_S4[PHASE_X];
	LONGLONG TotalEnergy_Simport[PHASE_X];
	LONGLONG TotalEnergy_Sexport[PHASE_X];
	LONGLONG TotalEnergy_Ssum[PHASE_X];
	LONGLONG TotalEnergy_Sdiff[PHASE_X];

} TEmeterArchiveEntry;

typedef struct 
{
	DWORD TimestampID;
	struct DateTime LastTimestamp;
	WORD ValidityFlag;

	DWORD ProgressiveEnergy_P1[PHASE_X];
	DWORD ProgressiveEnergy_P2[PHASE_X];
	DWORD ProgressiveEnergy_P3[PHASE_X];
	DWORD ProgressiveEnergy_P4[PHASE_X];

	DWORD ProgressiveEnergy_Q1[PHASE_X];
	DWORD ProgressiveEnergy_Q2[PHASE_X];
	DWORD ProgressiveEnergy_Q3[PHASE_X];
	DWORD ProgressiveEnergy_Q4[PHASE_X];

	DWORD ProgressiveEnergy_S1[PHASE_X];
	DWORD ProgressiveEnergy_S2[PHASE_X];
	DWORD ProgressiveEnergy_S3[PHASE_X];
	DWORD ProgressiveEnergy_S4[PHASE_X];

	DWORD Padding[12]; // till 256 bytes


} TEmeter15MinArchiveEntry;



extern char *AboutText;
extern void UpdateAboutText(void);
extern BOOL StartHelp(void);
extern char *HelpErrorText;


extern HANDLE hMutex_Indentification;



#define	SWAP_BYTES_IN_WORD(w)	(((w>>8)&0xFF)+((w&0xFF)<<8))  

extern void OpenResultsInNotepad(HWND hListBox);

extern int StatusBarParts;
extern int StatusBarWidths[];


extern void StartDeviceManager(void);


extern void StartCommunication(void);
extern void StopCommunication(char *szLegend);
extern int Communication(		
					WORD wCommand,
					WORD wLength,
					void* pvData,
					BOOL bBroadcast,
					DWORD dwTimeoutSecs,
					WORD *wReplyCommand,
					WORD *wReplyLength,
					void* pvReplyData,
					WORD wMaxReplyLength);


enum
{
COMMRESULT_TIMEOUT=0,
COMMRESULT_COMMERROR,
COMMRESULT_CRCERROR,
COMMRESULT_OK,
COMMRESULT_CANCELLED,


};



typedef struct
{
	DWORD Average3SecArchiveStartIndex;
	DWORD Average3SecArchiveIndex;
	DWORD Average3SecArchiveEndIndex;
	WORD State; 
	WORD QualityArchiveIndex;
	WORD Readiness;
} TQualityDeferredInfo;



void UpdateReadObjectDataButton(void);


extern DWORD WINAPI ThreadFunc_ReadOscilloscopeOnEventSettings(LPVOID lpv);
extern DWORD WINAPI ThreadFunc_WriteOscilloscopeOnEventSettings(LPVOID lpv);


extern void OscilloscopeOnEventSettingsShowValuesAndUnits(void);

extern DWORD dwOscilloscopeOnEventValue1;
extern DWORD dwOscilloscopeOnEventValue2;
extern DWORD dwOscilloscopeOnEventValue3;
extern float fOscilloscopeOnEventValue1;
extern float fOscilloscopeOnEventValue2;
extern float fOscilloscopeOnEventValue3;
extern WORD wOscilloscopeOnEventParameter1;
extern WORD wOscilloscopeOnEventParameter2;
extern WORD wOscilloscopeOnEventParameter3;
extern WORD wOscilloscopeOnEventCondition1;
extern WORD wOscilloscopeOnEventCondition2;
extern WORD wOscilloscopeOnEventCondition3;

extern char szOscilloscopeOnEventPassword[64];

extern DWORD dwOscilloscopeOnEventNominalCurrent;
extern DWORD dwOscilloscopeOnEventNominalVoltage;

extern WORD wOscilloscopeOnEventStartMode;
extern WORD wOscilloscopeOnEventEventType;
extern WORD wOscilloscopeOnEventPreEventTime;
extern WORD wOscilloscopeOnEventPostEventTime;
extern WORD wOscilloscopeOnEventDipSwellType;
extern WORD wOscilloscopeOnEventDipValue;
extern WORD wOscilloscopeOnEventSwellValue;

extern void SaveSettignsToFile_ETPKE(void);

extern WORD EtpkeCurrentBufferLCD;


extern void UsbSendOverlapped(BYTE *pbData,DWORD dwBytes,DWORD dwTimeout);


//"{ED8E660B-172C-435d-ADD7-31379DE3350B}"
//static GUID CYUSBDRV_GUID = {0xED8E660B,0x172C,0x435d,0xAD,0xD7,0x31,0x37,0x9D,0xE3,0x35,0x0B}; 
//"{AE18A550-7F6A-11d4-97DD-00010229B95B}"
static GUID CYUSBDRV_GUID = {0xAE18A550,0x7F6A,0x11d4,0x97,0xDD,0x00,0x01,0x02,0x29,0xB9,0x5B}; 

#define USB_EPOUT_ADDRESS 0x02
#define USB_EPIN_ADDRESS 0x86

extern FILE *FileRxBytesLogger;

#define	OSCILLOSCOPE_COMPRESSED	
//#undef	OSCILLOSCOPE_COMPRESSED	

#define	OSCILLOSCOPE_NAND_READ_MULTIPLE_PAGES	
//#undef	OSCILLOSCOPE_NAND_READ_MULTIPLE_PAGES	


extern BOOL CALLBACK ScreenDialogProcPOSTSETS(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);
extern float fPOSTSETS_DF_NPDP;
extern float fPOSTSETS_DF_NNDP;
extern float fPOSTSETS_DF_PNDP;
extern float fPOSTSETS_DF_PPDP;
extern float fPOSTSETS_DV_NPDP;
extern float fPOSTSETS_DV_NNDP;
extern float fPOSTSETS_DV_PNDP;
extern float fPOSTSETS_DV_PPDP;
extern DWORD dwPOSTSETS_MAXLOADS1_START;
extern DWORD dwPOSTSETS_MAXLOADS1_STOP;
extern DWORD dwPOSTSETS_MAXLOADS2_START;
extern DWORD dwPOSTSETS_MAXLOADS2_STOP;


extern BOOL CALLBACK ScreenDialogProcGetIP(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);

#endif

