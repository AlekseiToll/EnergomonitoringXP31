#ifndef USBETPQPA_H
#define USBETPQPA_H

#include "EmPort.h"

//namespace DeviceIO
//{

class EtPqpAUSB : public EmPortSLIP
{
protected:
	static HANDLE hUsbDevice_;
	static DWORD connectStatus_;
	BYTE EndpointAddress_;

public:
	EtPqpAUSB(int devType, WORD devAddress, int hMainWnd) 
		: EmPortSLIP(devType, devAddress, USB, hMainWnd)
	{
		EmService::WriteToLogGeneral("EtPqpAUSB constructor");
	}

	~EtPqpAUSB() 
	{
		EmService::WriteToLogGeneral("EtPqpAUSB destructor");
	}

	bool Open();

	bool Close();

	int Write(DWORD size, BYTE* buffer);

	bool WriteModemInfo(const char* str) { /* dummy */ return true; }

protected: 
	static void RxThreadStart(LPVOID param)
	{
		((EtPqpAUSB*)param)->RxThread();
	}

	void RxThread();
};

//}

#endif
