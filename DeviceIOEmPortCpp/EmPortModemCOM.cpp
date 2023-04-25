#include "stdafx.h"
#include "EmPortModemCOM.h"

//namespace DeviceIO
//{
HANDLE EmPortModemCOM::threadInfo_ = 0;
ConnectInfoThread* EmPortModemCOM::threadObj_ = 0;
std::string EmPortModemCOM::phone_ = "";
int EmPortModemCOM::attempts_ = 3;
bool EmPortModemCOM::bAutoMode_ = false;

bool EmPortModemCOM::Open()
{
	ResetEvent(hEventDisconnected_);
	pTimerThread_->bStopTimer_ = false;

	try
	{
		if (portName_ == "" || portSpeed_ == 0 || phone_.size() < 7)
			return false;

		if (!bAutoMode_)
		{
			// создаем поток окна для вывода сообщений
			threadInfo_ = CreateThread(NULL, 0, 
				(LPTHREAD_START_ROUTINE)(ConnectInfoThread::ThreadEntryStatic), threadObj_, 0, NULL);
			Sleep(1000);
		}

		try
		{
			short portNumber = (short)EmService::StringToNumber(portName_.substr(3, portName_.length() - 3));
			if (portNumber > 9)
			{
				std::stringstream ss;
				ss << "\\\\.\\COM" << portNumber;
				portName_ = ss.str();
			}
		}
		catch(...) {}

		WriteModemInfo("opening port...");

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
			EmService::WriteToLogFailed("unable to open COM port! handle = -1");
			return false;
		}

		WriteModemInfo("port was opened successfully");

		modemConnectInProcess_ = true;

		DCB dcb;
		GetCommState(handle_, &dcb);
		dcb.fBinary = 1;
		dcb.BaudRate = (DWORD)portSpeed_;
		dcb.ByteSize = 8;
		dcb.fParity = NOPARITY;
		dcb.StopBits = ONESTOPBIT;
		//dcb.DtrControl = DtrControl.Handshake; 
		dcb.fDtrControl = DTR_CONTROL_ENABLE;
		dcb.fRtsControl = RTS_CONTROL_HANDSHAKE;
		dcb.fOutxCtsFlow = true;
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
		WriteModemInfo("SetCommState successfully");

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
		WriteModemInfo("SetCommMask successfully");

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
		WriteModemInfo("SetCommTimeouts successfully");

		//------------------------------------------------------

		InputFuncPointer = &EmPortModemCOM::RxFunctionConnect;
		BYTE init_data[256];

		rxThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(RxThreadStart), this, 0, NULL);

		std::string strSetup = "at&d2\r";
		strSetup.copy((char*)init_data, strSetup.size());
		WriteModemInfo("sending at&d2...");
		messBuffer_ = "";
		Write(strSetup.size(), init_data);
		Sleep(3000);
		messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\n');
		messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\r');
		WriteModemInfo(messBuffer_.c_str());

		if (EmService::StdStringToLower(messBuffer_).find("error", 0) != std::string::npos)
		{
			EmService::WriteToLogGeneral(EmService::GetCurrentDateTime() + ":  connection failed!");
			EmService::WriteToLogGeneral("at&d2\r failed");

			if (CloseHandle(handle_) == false)
				EmService::WriteToLogFailed("Error while closing port handle");
			return false;
		}

		//-------------------------------------
		strSetup = "ati\r";
		strSetup.copy((char*)init_data, strSetup.size());
		WriteModemInfo("sending ati...");
		messBuffer_ = "";
		Write(strSetup.size(), init_data);
		Sleep(3000);
		messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\n');
		messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\r');
		WriteModemInfo(messBuffer_.c_str());

		strSetup = "at+csq\r";
		strSetup.copy((char*)init_data, strSetup.size());
		WriteModemInfo("sending at+csq...");
		messBuffer_ = "";
		Write(strSetup.size(), init_data);
		Sleep(3000);
		messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\n');
		messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\r');
		WriteModemInfo(messBuffer_.c_str());

		//---------------------------------------

		time_t timeStart, timeFinish;
		for (int cur_attempt = 0; cur_attempt < attempts_; ++cur_attempt)
		{
			std::string sendatd = "sending atd"; sendatd += phone_; sendatd += "...";
			WriteModemInfo(sendatd.c_str());

			strSetup = "atd" + phone_ + "\r";
			strSetup.copy((char*)init_data, strSetup.size());
			messBuffer_ = "";
			Write(strSetup.size(), init_data);

			bool connected = false;
			time(&timeStart);
			while (true)
			{
				if (EmService::StdStringToLower(messBuffer_).find("connect", 0) != std::string::npos)
				{
					connected = true;
					break;
				}
				if (EmService::StdStringToLower(messBuffer_).find("no carrier", 0) != std::string::npos)
				{
					break;
				}
				if (EmService::StdStringToLower(messBuffer_).find("error", 0) != std::string::npos)
				{
					break;
				}
				if (EmService::StdStringToLower(messBuffer_).find("busy", 0) != std::string::npos)
				{
					break;
				}
				DWORD modem_status = 0;
				GetCommModemStatus(handle_, &modem_status);
				if ((modem_status & MS_RLSD_ON) != 0) //connect
				{
					connected = true;
					break;
				}
				time(&timeFinish);
				double timeElapsed = difftime(timeFinish, timeStart);
				if (timeElapsed > 90) //error
				{
					break;
				}
				Sleep(1000);
			}
			messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, 0);
			messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\n');
			messBuffer_ = EmService::RemoveSymbolFromString(messBuffer_, '\r');
			WriteModemInfo(messBuffer_.c_str());

			if (connected)
			{
				EmService::WriteToLogGeneral(EmService::GetCurrentDateTime() +
						":  connection successful!");
				break;
			}
			else if (cur_attempt == (attempts_ - 1) && !connected)
			{
				if (CloseHandle(handle_) == false)
					EmService::WriteToLogFailed("Error while closing port handle");
				//mainThread_ = null;
				pTimerThread_->bStopTimer_ = true;
				terminateRxThread_ = true;
				Sleep(2000);
				CloseHandle(rxThread_);
				return false;
			}
			else
			{
				EmService::WriteToLogGeneral(EmService::GetCurrentDateTime() +
						":  connection failed!");
			}
		}
		//------------------------------------------------------
		cmt.ReadIntervalTimeout = 0;
		cmt.ReadTotalTimeoutConstant = 10000;
		cmt.ReadTotalTimeoutMultiplier = 0;
		cmt.WriteTotalTimeoutConstant = 10000;
		cmt.WriteTotalTimeoutMultiplier = 0;
		SetCommTimeouts(handle_, &cmt);

		Purge();
		SetDtrOn();
		UpdateModemStatus();

		modemConnectInProcess_ = false;

		InputFuncPointer = &EmPortModemCOM::RxFunction;

		return true;
	}
	catch(...)
	{
		EmService::WriteToLogFailed("Error in Open modem port!");

		InputFuncPointer = &EmPortModemCOM::RxFunction;
		Close();
		return false;
	}
}

bool EmPortModemCOM::Close()
{
	try
	{
		EmService::WriteToLogGeneral("Close modem port");

		if (handle_ == INVALID_HANDLE_VALUE) return false;

		threadObj_->HideWnd();
		Sleep(1000);
		CloseHandle(threadInfo_);

		pTimerThread_->bStopTimer_ = true;
		terminateRxThread_ = true;
		Sleep(2000);
		CloseHandle(rxThread_);
		
		//mainThread_ = null;

		SetDtrOff();
		Sleep(200);

		if (CloseHandle(handle_) == false)
			EmService::WriteToLogFailed("Error while closing port handle");
		handle_ = INVALID_HANDLE_VALUE;

		//try
		//{
		//	OnHideMessageWnd();
		//}
		//catch (Exception ex)
		//{
		//	EmService::WriteToLogFailed("EmPortModemCOM::Close OnHideMessageWnd(): " 
		//					+ ex.Message);
		//}
		//threadInfo_ = null;
		//thread_ = null;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("EmPortModemCOM Close failed!");
		return false;
	}
	return true;
}
	
//}
