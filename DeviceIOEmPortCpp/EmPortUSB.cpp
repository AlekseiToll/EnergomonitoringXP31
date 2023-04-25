#include "stdafx.h"
#include "EmPortUSB.h"

//namespace DeviceIO
//{

std::string EmPortUSB::serial_ = "";
std::string EmPortUSB::descr_ = "";
std::string EmPortUSB::DEVICE_PREFIX = "E33";

bool EmPortUSB::Open()
{
	try
	{
		EmService::WriteToLogGeneral("EmPortUSB::Open() entry");

		if (!USBCreateInstance()) return false;
		if (!USBLoadDriver())
		{
			Close();
			return false;
		}

		EmService::WriteToLogGeneral("EmPortUSB::Open() USBLoadDriver successful");

		int dev_num = USBGetNumberDevices();
		if (dev_num == 0)
		{
			Close();
			return false;
		}

		// buffers for reading
		BYTE SerialBuffer[128];
		BYTE DescriptionBuffer[128];

		// human strings
		serial_ = "";
		descr_ = "";

		bool bDeviceFound = false;

		for (int i = 0; i < dev_num; i++)
		{
			if (!USBGetSerialDescription(i, SerialBuffer, DescriptionBuffer))
			{
				Close();
				return false;
			}

			serial_ = (char*)SerialBuffer;			//"E3300132"
			descr_ = (char*)DescriptionBuffer;		//"Energomonitor 3.3-T"
			std::stringstream ss;
			ss << "serial_ = " << serial_ << ", descr_ = " << descr_;
			EmService::WriteToLogGeneral(ss.str());

			if (serial_.find(DEVICE_PREFIX, 0) == std::string::npos) 
			{
				EmService::WriteToLogGeneral("device was NOT found!");
				continue;
			}
			else
			{
				EmService::WriteToLogGeneral("device was found!");
				bDeviceFound = true;
				break;
			}
		}
		if (!bDeviceFound)
		{
			Close();
			return false;
		}
		if (!USBSetSerial(SerialBuffer))
		{
			USBFreeInstance();
			return false;
		}
		if (!USBOpenDeviceBySerial())
		{
			USBFreeInstance();
			return false;
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortUSB::Open()");
		Close();
		return false;
	}	

	return true;
}

//}
