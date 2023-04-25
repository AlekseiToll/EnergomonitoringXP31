// DeviceIOEmPortCpp.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "DeviceIOEmPortCpp.h"
#include "EmServiceClasses.h"

#include "EmPortCOM.h"
#include "EmPortModemCOM.h"
#include "EmPortUSB.h"
#include "EtPqpAUSB.h"
#include "EmEthernet.h"
#include "EmPortRs485COM.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH: /*DeviceIO::EmService::WriteToLogGeneral("DllMain DLL_PROCESS_ATTACH");*/ 
		break;
	case DLL_THREAD_ATTACH: /*DeviceIO::EmService::WriteToLogGeneral("DllMain DLL_THREAD_ATTACH");*/ 
		break;
	case DLL_THREAD_DETACH: /*DeviceIO::EmService::WriteToLogGeneral("DllMain DLL_THREAD_DETACH");*/ 
		break;
	case DLL_PROCESS_DETACH: /*DeviceIO::EmService::WriteToLogGeneral("DllMain DLL_PROCESS_DETACH");*/ 
		break;
	}
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif


EmPortWrapper::EmPortWrapper(int iDevType, int iPortType, vector<string> vec_cparams, 
							 vector<unsigned int> vec_iparams, int hMainWnd)
{
	EmService::WriteToLogGeneral("EmPortWrapper constructor");
	portType_ = (EmPortType)iPortType;
	devType_ = (EmDeviceType)iDevType;
	hMainWnd_ = hMainWnd;

	bool autoMode = false;
	WORD ipAddr[4];

	switch(devType_)
	{
	case EM31K:
	case EM33T:
	case EM33T1:
		switch(portType_)
		{
		case COM:
			port_ = new EmPortComRs232_33((int)devType_, vec_cparams[0], vec_iparams[0], hMainWnd_);
			break;
		case USB:
			port_ = new EmPortUSB((int)devType_, hMainWnd_);
			break;
		default:
			throw EmException("EmPortWrapper(): invalid port type!");
		}
		break;
	case EM32:
		switch(portType_)
		{
		case COM:
			port_ = new EmPortComRs232_SLIP((int)devType_, vec_cparams[0], vec_iparams[0], 0xFFFF, hMainWnd_);
			break;
		case Modem:
			autoMode = vec_iparams[2] != 0;
			port_ = new EmPortModemCOM((int)devType_, vec_cparams[0], vec_iparams[0], 
						vec_cparams[1], vec_iparams[1], vec_iparams[3] /*devAddr*/, autoMode, hMainWnd_);
			break;
		case Ethernet:
			//autoMode = vec_iparams[5] != 0;
			for(int i = 0; i < 4; ++i)
				ipAddr[i] = vec_iparams[i];
			port_ = new EmEthernet((int)devType_, ipAddr, vec_iparams[4], 0xFFFF, hMainWnd_); 
			break;
		case Rs485:
			//autoMode = vec_iparams[2] != 0;
			port_ = new EmPortComRs485_SLIP((int)devType_, vec_cparams[0], vec_iparams[0], vec_iparams[1], hMainWnd_); 
			break;
		case GPRS:
			//autoMode = vec_iparams[5] != 0;
			for(int i = 0; i < 4; ++i)
				ipAddr[i] = vec_iparams[i];
			port_ = new EmEthernet((int)devType_, ipAddr, vec_iparams[4], vec_iparams[5] /*devAddr*/, hMainWnd_);
			break;
		default:
			throw EmException("EmPortWrapper(): invalid port type!");
		}
		break;
	case ETPQP:
		switch(portType_)
		{
		case COM:
			port_ = new EmPortComRs232_SLIP((int)devType_, vec_cparams[0], vec_iparams[0], 0xFFFF, hMainWnd_);
			break;
		default:
			throw EmException("EmPortWrapper(): invalid port type!");
		}
		break;
	case ETPQP_A:
		EmService::WriteToLogGeneral(EmService::NumberToString((int)portType_));
		switch(portType_)
		{
		case USB:
			port_ = new EtPqpAUSB((int)devType_, 0xFFFF, hMainWnd_);
			break;
		case Ethernet:
			for(int i = 0; i < 4; ++i)
				ipAddr[i] = vec_iparams[i];
			port_ = new EmEthernet((int)devType_, ipAddr, vec_iparams[4], 0xFFFF, hMainWnd_); 
			break;
		case WI_FI:
			for(int i = 0; i < 4; ++i)
				ipAddr[i] = vec_iparams[i];
			port_ = new EmEthernet((int)devType_, ipAddr, vec_iparams[4], 0xFFFF, hMainWnd_); 
			break;
		default:
			throw EmException("EmPortWrapper(): invalid port type!");
		}
		break;
	default:
		throw EmException("EmPortWrapper(): invalid device type!");
	}
}

EmPortWrapper::~EmPortWrapper()
{
	if(port_ != 0)
		delete port_;
	EmService::WriteToLogGeneral("EmPortWrapper destructor");
}

int EmPortWrapper::ReadData(WORD command, BYTE** buffer, long* arr_params, long params_count,
		long* pAnswerLen)
{
	switch(devType_)
	{
	case EM31K:
	case EM33T:
	case EM33T1:
		return -1;

	case EM32:
	case ETPQP:
	case ETPQP_A:
		return ((EmPortSLIP*)port_)->ReadData(command, buffer, arr_params, params_count, pAnswerLen);

	default:
		throw EmException("EmPortWrapper(): invalid device type!");
	}
}

int EmPortWrapper::WriteData(WORD command, BYTE* buffer, short bufLength)
{
	switch(devType_)
	{
	case EM31K:
	case EM33T:
	case EM33T1:
		return -1;

	case EM32:
	case ETPQP:
	case ETPQP_A:
		return ((EmPortSLIP*)port_)->WriteData(command, buffer, bufLength);

	default:
		throw EmException("EmPortWrapper(): invalid device type!");
	}
}

bool EmPortWrapper::Open()
{
	bool res = port_->Open();
	if(res) isOpened_ = true;
	return res;
}

bool EmPortWrapper::Close()
{
	try
	{
		if(!isOpened_) return true;

		if(devType_ == EM32 || devType_ == ETPQP || devType_ == ETPQP_A)
			((EmPortSLIP*)port_)->Disconnect();

		bool res = port_->Close();
		if(res) isOpened_ = false;
		return res;
	}
	catch(...)
	{
		EmService::WriteToLogFailed("Exception in EmPortWrapper::Close()");
	}
}

int EmPortWrapper::Write(DWORD size, BYTE* buffer)
{
	return port_->Write(size, buffer);
}

int EmPortWrapper::Read(DWORD size, BYTE* buffer)
{
	switch(devType_)
	{
	case EM31K:
	case EM33T:
	case EM33T1:
		switch(portType_)
		{
		case COM: return ((EmPortComRs232_33*)port_)->Read(size, buffer);
		case USB: return ((EmPortUSB*)port_)->Read(size, buffer);
		}
		return -1;

	case EM32:
	case ETPQP:
	case ETPQP_A:
		return -1;

	default:
		throw EmException("EmPortWrapper() Read: invalid device type!");
	}
}
