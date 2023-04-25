#ifndef EMPORTCOM_H
#define EMPORTCOM_H

#include "EmPort.h"
#include "DeviceIOEmPortCpp.h"

//namespace DeviceIO
//{

class EmPortComRs232_SLIP : public EmPortComSLIP
{
public:
	EmPortComRs232_SLIP(int devType, std::string name, int speed, WORD devAddress, int hMainWnd) 
		: EmPortComSLIP(devType, devAddress, COM, hMainWnd)
	{
		EmService::WriteToLogGeneral("EmPortComRs232_SLIP constructor");
		portSpeed_ = speed;
		portName_ = name;
	}

	~EmPortComRs232_SLIP() 
	{
		EmService::WriteToLogGeneral("EmPortComRs232_SLIP destructor");
	}

	bool Open();
	bool Close();

	bool WriteModemInfo(const char* str) { /* dummy */ return true; }

protected:
	static DWORD RxThreadStart(LPVOID param)
	{
		((EmPortComRs232_SLIP*)param)->RxThread();
		return 0;
	}
};

class EmPortComRs232_33 : public EmPort33
{
protected:
	static std::string portName_;
	static int portSpeed_;

	static HANDLE handle_;

public: 
	EmPortComRs232_33(int devType, std::string name, int speed, int hMainWnd)
		: EmPort33(devType, COM, hMainWnd)//, portName_(name), portSpeed_(speed)
	{
		EmService::WriteToLogGeneral("EmPortComRs232_33 constructor");
		portName_ = name;
		portSpeed_ = speed;
	}

	~EmPortComRs232_33() 
	{
		EmService::WriteToLogGeneral("EmPortComRs232_33 destructor");
	}

	bool Open();
	bool Close();

	int Read(DWORD size, BYTE* buffer);
	int Write(DWORD size, BYTE* buffer);
};

//}

#endif