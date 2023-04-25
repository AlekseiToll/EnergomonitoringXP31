#include "stdafx.h"
#include "EmPortRs485COM.h"

//namespace DeviceIO
//{

bool EmPortComRs485_SLIP::Open()
{
	try
	{
		ResetEvent(hEventDisconnected_);
		pTimerThread_->bStopTimer_ = false;

		if (portName_ == "" || portSpeed_ == 0)
			return false;

		try
		{
			short portNumber = EmService::StringToNumber(portName_.substr(3, portName_.length() - 3));
			if (portNumber > 9)
			{
				std::stringstream ss;
				ss << "\\\\.\\COM" << portNumber;
				portName_ = ss.str();
			}
		}
		catch(...) {}

		handle_ = CreateFile(
			(LPCTSTR)portName_.c_str(),
			GENERIC_READ | GENERIC_WRITE,
			(DWORD) 0,			// share mode
			NULL,				// pointer to security attributes
			OPEN_EXISTING,		// how to create
			FILE_FLAG_OVERLAPPED,  // file attributes
			NULL         // handle to file with attributes to copy
			);

		if (handle_ == INVALID_HANDLE_VALUE)
		{
			CloseHandle(handle_);
			EmService::WriteToLogFailed("unable to open COM port! handle = -1");
			return false;
		}

		DCB dcb;
		GetCommState(handle_, &dcb);
		dcb.fBinary = true;
		dcb.BaudRate = (DWORD)portSpeed_;
		dcb.ByteSize = 8;
		dcb.fParity = NOPARITY;
		dcb.StopBits = ONESTOPBIT;
		//dcb.DtrControl = DtrControl.Handshake; 
		dcb.fDtrControl = DTR_CONTROL_ENABLE;
		dcb.fRtsControl = RTS_CONTROL_ENABLE;
		dcb.fOutxCtsFlow = false;
		dcb.fOutxDsrFlow = false;
		dcb.fDsrSensitivity = false;
		SetupComm(handle_, 8192, 8192);
		if (!SetCommState(handle_, &dcb))
		{
			EmService::WriteToLogFailed("Open failed: SetCommState");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}

		DWORD dwStoredFlags = EV_BREAK | EV_CTS | EV_DSR | EV_ERR | EV_RING | EV_RLSD |
				EV_RXCHAR | EV_RXFLAG | EV_TXEMPTY;
		//	dwStoredFlags = EV_CTS | EV_DSR | EV_RING | EV_RLSD;
		if (!SetCommMask(handle_, dwStoredFlags))
		{
			EmService::WriteToLogFailed("Open failed: SetCommMask");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}

		Purge();

		COMMTIMEOUTS cmt;
		GetCommTimeouts(handle_, &cmt);
		cmt.ReadTotalTimeoutConstant = 2000;	//1000;
		cmt.WriteTotalTimeoutConstant = 20000;
		cmt.ReadIntervalTimeout = 0;
		cmt.ReadTotalTimeoutMultiplier = 0;
		cmt.WriteTotalTimeoutMultiplier = 0;
		if (!SetCommTimeouts(handle_, &cmt))
		{
			EmService::WriteToLogFailed("Open failed: SetCommTimeouts");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}

		cmt.ReadIntervalTimeout = 0;
		cmt.ReadTotalTimeoutConstant = 10000;
		cmt.ReadTotalTimeoutMultiplier = 0;
		cmt.WriteTotalTimeoutConstant = 10000;
		cmt.WriteTotalTimeoutMultiplier = 0;
		SetCommTimeouts(handle_, &cmt);

		rxThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(RxThreadStart), this, 0, NULL);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Open COM 485 failed!");
		return false;
	}

	return true;
}

bool EmPortComRs485_SLIP::Close()
{
	try
	{
		EmService::WriteToLogGeneral("Close COM 485");

		pTimerThread_->bStopTimer_ = true;
		terminateRxThread_ = true;
		Sleep(2000);
		CloseHandle(rxThread_);
		
		//mainThread_ = null;

		if (handle_ == INVALID_HANDLE_VALUE) return false;
		if (CloseHandle(handle_) == false)
		{
			return false;
		}
		handle_ = INVALID_HANDLE_VALUE;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("EmPortComRs485_32 Close failed!");
		return false;
	}
	return true;
}

//}
