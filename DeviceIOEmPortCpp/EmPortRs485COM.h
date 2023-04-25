#ifndef EMPORT485COM_H
#define EMPORT485COM_H

#include "EmPort.h"

//namespace DeviceIO
//{

class EmPortComRs485_SLIP : public EmPortComSLIP
{
public: 
	EmPortComRs485_SLIP(int devType, std::string name, int speed, WORD devAddr, int hMainWnd) 
		: EmPortComSLIP(devType, devAddr, Rs485, hMainWnd)
	{
		EmService::WriteToLogGeneral("EmPortComRs485_SLIP constructor");
		portName_ = name;
		portSpeed_ = speed;
	}

	~EmPortComRs485_SLIP() 
	{
		EmService::WriteToLogGeneral("EmPortComRs485_SLIP destructor");
	}

	bool Open();
	bool Close();

	bool WriteModemInfo(const char* str) { /* dummy */ return true; }
};

//}

#endif
