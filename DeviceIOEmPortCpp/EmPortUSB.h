#ifndef EMPORTUSB_H
#define EMPORTUSB_H

#include "EmPort.h"
#include "../EmUsb/EmUsb.h"

//namespace DeviceIO
//{

class EmPortUSB : public EmPort33
{
	static std::string serial_;
	static std::string descr_;
	static std::string DEVICE_PREFIX;

public: 
	EmPortUSB(int devType, int hMainWnd) : EmPort33(devType, USB, hMainWnd)//, DEVICE_PREFIX("E33")
	{
		EmService::WriteToLogGeneral("EmPortUSB constructor");
	}

	~EmPortUSB() 
	{
		EmService::WriteToLogGeneral("EmPortUSB destructor");
	}

	bool Open();

	bool Close()
	{
		EmService::WriteToLogGeneral("Close USB port");

		if(!USBClose())
			EmService::WriteToLogFailed("EmPortUSB::Close(): USBClose() returned false!");
		USBFreeInstance();

		return true;
	}

	int Read(DWORD size, BYTE* buffer)
	{
		int res = USBRead(buffer, (int)size);

		if (res != 0) return 0;

		return -3;
	}

	int Write(DWORD size, BYTE* buffer)
	{
		int res = USBWrite(buffer, (int)size);

		if (res != 0) return 0;

		return -3;
	}
};

//}

#endif
