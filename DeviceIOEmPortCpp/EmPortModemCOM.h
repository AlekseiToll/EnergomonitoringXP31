#ifndef EMPORTMODEMCOM_H
#define EMPORTMODEMCOM_H

#include "EmPort.h"
#include "ModemInfoWnd.h"

//namespace DeviceIO
//{

class EmPortModemCOM : public EmPortComSLIP
{
	static std::string phone_;
	static int attempts_;
	static bool bAutoMode_;

	static HANDLE threadInfo_;
	static ConnectInfoThread* threadObj_;

public:

	EmPortModemCOM(int devType, std::string name, int speed, 
						std::string phone, int attempts, WORD devAddress, bool autoMode, int hMainWnd) 
		: EmPortComSLIP(devType, devAddress, Modem, hMainWnd)
	{
		EmService::WriteToLogGeneral("EmPortModemCOM constructor");

		portName_ = name;
		portSpeed_ = speed;
		phone_ = phone;
		attempts_ = attempts;
		bAutoMode_ = autoMode;
		if (attempts_ <= 0) attempts_ = 5;

		threadObj_ = new ConnectInfoThread();
	}

	~EmPortModemCOM() 
	{
		EmService::WriteToLogGeneral("EmPortModemCOM destructor");
		//Close();
		delete threadObj_;
	}

	bool Open();
	bool Close();

	bool WriteModemInfo(const char* str)
	{
		try
		{
			//EmService::WriteToLogGeneral(str);
			threadObj_->AddText(str);
			return true;
		}
		catch (...)
		{
			EmService::WriteToLogFailed("Error in WriteModemInfo()!");
			return false;
		}
	}
};

//}

#endif
