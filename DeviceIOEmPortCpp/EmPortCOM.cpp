#include "stdafx.h"
#include "EmPortCOM.h"

//namespace DeviceIO
//{

bool EmPortComRs232_SLIP::Open()
{
	try
	{
		ResetEvent(hEventDisconnected_);
		pTimerThread_->bStopTimer_ = false;

		if (portName_ == "" || portSpeed_ == 0) return false;

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
			int errCode = GetLastError();
			EmService::WriteToLogFailed("EmPortComRs232_SLIP::unable to open COM port! handle = -1");
			EmService::WriteToLogFailed(portName_);
			EmService::WriteToLogFailed(EmService::NumberToString(errCode));
			EmService::WriteToLogFailed("===================================");
			return false;
		}

		DCB dcb;
		if(!GetCommState(handle_, &dcb))
		{
			EmService::WriteToLogFailed("GetCommState() failed!");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}
		dcb.fBinary = 1;
		dcb.ByteSize = 8;	// 8
		dcb.BaudRate = (DWORD)portSpeed_;
		dcb.fParity = NOPARITY;
		dcb.StopBits = ONESTOPBIT;
		dcb.fRtsControl = RTS_CONTROL_HANDSHAKE;
		//dcb.fRtsControl = RTS_CONTROL_TOGGLE;
		dcb.fOutxCtsFlow = true;
		dcb.fOutxDsrFlow  = false;
		dcb.fDsrSensitivity  = false;
		dcb.fDtrControl = DTR_CONTROL_ENABLE;
		dcb.fAbortOnError = 0;
		SetupComm(handle_ , 8192 , 8192);
		if(!SetCommState(handle_, &dcb)) 
		{
			EmService::WriteToLogFailed("SetCommState() failed!");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}
	
		Purge();

		COMMTIMEOUTS cmt;
		GetCommTimeouts(handle_, &cmt);
		cmt.ReadTotalTimeoutConstant = 2000;
		cmt.WriteTotalTimeoutConstant = 20000;
		SetCommTimeouts(handle_, &cmt);

		//mainThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(threadstart0), this, 0, NULL);
		rxThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(RxThreadStart), this, 0, NULL);

		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Open COM port SLIP failed!");
		return false;
	}
}

bool EmPortComRs232_SLIP::Close()
{
	try
	{
		EmService::WriteToLogGeneral("Close COM port");

		pTimerThread_->bStopTimer_ = true;
		terminateRxThread_ = true;
		Sleep(2000);
		CloseHandle(rxThread_);
		
		//mainThread_ = null;

		if (handle_ == INVALID_HANDLE_VALUE) return false;

		if (CloseHandle(handle_) == false)
		{
			EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}
		handle_ = INVALID_HANDLE_VALUE;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("EmPortComRs232_32 Close failed!");
		return false;
	}
	return true;
}

//===================================================================
// EmPortComRs232_33
//===================================================================

std::string EmPortComRs232_33::portName_ = "";
int EmPortComRs232_33::portSpeed_ = 0;
HANDLE EmPortComRs232_33::handle_ = 0;

bool EmPortComRs232_33::Open()
{
	try
	{
		EmService::WriteToLogGeneral("Open COM port");

		if (portName_ == "" || portSpeed_ == 0) return false;

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
		catch (...) { }

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
		if(!GetCommState(handle_, &dcb))
		{
			EmService::WriteToLogFailed("GetCommState() failed!");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}
		dcb.ByteSize = 8;	// 8
		dcb.BaudRate = (DWORD)portSpeed_;
		dcb.fParity = NOPARITY;
		dcb.StopBits = ONESTOPBIT;
		dcb.fDtrControl = DTR_CONTROL_ENABLE;
		dcb.fRtsControl = RTS_CONTROL_ENABLE;
		if(!SetCommState(handle_, &dcb)) 
		{
			EmService::WriteToLogFailed("SetCommState() failed!");
			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}

		COMMTIMEOUTS cmt;
		cmt.ReadIntervalTimeout = 0;
		cmt.ReadTotalTimeoutConstant = 2000;
		cmt.ReadTotalTimeoutMultiplier = 0;
		cmt.WriteTotalTimeoutConstant = 2000;
		cmt.WriteTotalTimeoutMultiplier = 0;
		SetCommTimeouts(handle_, &cmt);

		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Open COM port 33 failed!");
		return false;
	}
}

bool EmPortComRs232_33::Close()
{
	try
	{
		EmService::WriteToLogGeneral("Close COM port");

		if (handle_ == INVALID_HANDLE_VALUE) return false;

		if (CloseHandle(handle_) == false)
		{
			EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}
		handle_ = INVALID_HANDLE_VALUE;

		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Close COM port 33 failed!");
		return false;
	}
}

int EmPortComRs232_33::Read(DWORD size, BYTE* buffer)
{
	try
	{
		if (handle_ == INVALID_HANDLE_VALUE) return -1;

		OVERLAPPED osRead = {0};
		DWORD dwRead = 0;
		DWORD dwRes;
		osRead.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

		if (ReadFile(handle_, buffer, size, &dwRead, &osRead) == FALSE)
		{
			DWORD error = GetLastError();
			if (error != ERROR_IO_PENDING)
			{
				std::string debugStr = "Error in ReadFile(): ";
				debugStr += EmService::NumberToString(error);
				EmService::WriteToLogFailed(debugStr);
				return -1;
			}
			else
				dwRes = WaitForSingleObject(osRead.hEvent, 180000);  // timeout = 3 min

			switch(dwRes)
			{
				case WAIT_OBJECT_0:
					if (!GetOverlappedResult(handle_, &osRead, &dwRead, FALSE))
						return -2;
					break;

				default:
					return -1;
			}
		}

		if (dwRead != size) 
		{
			std::string debugStr = "Error in Read(): dwRead = ";
			debugStr += EmService::NumberToString(dwRead);
			debugStr += ", size = ";  debugStr += EmService::NumberToString(size);
			EmService::WriteToLogFailed(debugStr);
			return -3;
		}

		return 0;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Read COM port 33 failed!");
		return -1;
	}
}

int EmPortComRs232_33::Write(DWORD size, BYTE* buffer)
{
	try
	{
		if (handle_ == INVALID_HANDLE_VALUE) return -1;

		OVERLAPPED osWrite = {0};
		DWORD dwWritten = 0;
		DWORD dwRes;
		osWrite.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

		BOOL success = WriteFile(handle_, buffer, size, &dwWritten, &osWrite);
		if (success == FALSE)
		{
			DWORD error = GetLastError();
			if (error != ERROR_IO_PENDING)
			{
				std::string debugStr = "Error in Write(): GetLastError() = ";
				debugStr += EmService::NumberToString(error);
				EmService::WriteToLogFailed(debugStr);
				return -2;
			}
			else
				dwRes = WaitForSingleObject(osWrite.hEvent, 180000);  // timeout = 3 min

			switch(dwRes)
			{
				case WAIT_OBJECT_0:
					if (!GetOverlappedResult(handle_, &osWrite, &dwWritten, FALSE))
						return -2;
					break;

				default:
					return -1;
			}
		}
	
		if (dwWritten != size) 
		{
			std::string debugStr = "Error in Write(): dwWritten = ";
			debugStr += EmService::NumberToString(dwWritten);
			debugStr += ", size = ";  debugStr += EmService::NumberToString(size);
			EmService::WriteToLogFailed(debugStr);
			return -3;
		}

		return 0;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Write COM port 33 failed!");
		return -1;
	}
}
	
//}
