#ifndef EMETHERNET_H
#define EMETHERNET_H

#include <winsock2.h>
#include "EmPort.h"

//namespace DeviceIO
//{

class EmEthernet : public EmPortSLIP
{
protected:
	static WORD port_;
	static SOCKET tcpClientSocket_;
	static DWORD connectStatus_;
	static HANDLE hTcpEvent_;
	static WORD DeviceIpAddress[4];
	// этот флаг нужен для контроля того, чтобы на один вызов WSAStartUp приходилось
	// не более одного вызова WSACleanUp. иначе нарушается соединение с PostgreSQL
	static bool needWsaCleanUp_;

public:
	EmEthernet(int devType, WORD* ipAddr, WORD port, WORD devAddress, int hMainWnd) 
		: EmPortSLIP(devType, devAddress, Ethernet, hMainWnd)//, needWsaCleanUp_(false)
	{
		EmService::WriteToLogGeneral("EmEthernet constructor");

		DeviceIpAddress[0] = ipAddr[0];
		DeviceIpAddress[1] = ipAddr[1];
		DeviceIpAddress[2] = ipAddr[2];
		DeviceIpAddress[3] = ipAddr[3];
		port_ = port;
	}

	~EmEthernet() 
	{
		EmService::WriteToLogGeneral("EmEthernet destructor");
	}

	bool Open();

	bool Close();

	int Write(DWORD size, BYTE* buffer);

	bool WriteModemInfo(const char* str) { /* dummy */ return true; }

protected: 
	static void RxThreadStart(LPVOID param)
	{
		((EmEthernet*)param)->RxThread();
	}

	void RxThread();

	static bool ParseIPAddress(std::string addr, WORD* ipaddr);
};

//}

#endif
