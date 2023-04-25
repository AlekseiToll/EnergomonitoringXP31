#include "stdafx.h"
#include "EmEthernet.h"

#pragma comment (lib,"ws2_32")

//namespace DeviceIO
//{

WORD EmEthernet::port_ = 0;
SOCKET EmEthernet::tcpClientSocket_ = 0;
DWORD EmEthernet::connectStatus_ = CONNECTSTATUS_DISCONNECTED;
HANDLE EmEthernet::hTcpEvent_ = 0;
WORD EmEthernet::DeviceIpAddress[4] = {0};
bool EmEthernet::needWsaCleanUp_ = false;

bool EmEthernet::Open()
{
	try
	{
		EmService::WriteToLogGeneral("EmEthernet::Open");

		ResetEvent(hEventDisconnected_);
		pTimerThread_->bStopTimer_ = false;

		WSAData wsaData;
		SOCKADDR_IN tcpServerInfo;
		WSAStartup(MAKEWORD(2, 2), &wsaData);
		needWsaCleanUp_ = true;
		tcpClientSocket_ = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
		if (tcpClientSocket_ == INVALID_SOCKET)
		{
			//SetEvent(hEventDisconnected);
			if (needWsaCleanUp_)
			{
				WSACleanup();
				needWsaCleanUp_ = false;
			}
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FAILED);
			Sleep(1000);
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
			//OnDisconnect();
			return false;
		}
		tcpServerInfo.sin_family = AF_INET;
		tcpServerInfo.sin_port = htons(port_);
		DWORD dwServerIpAddress = (DWORD)(DeviceIpAddress[0]) << (3 * 8);
		dwServerIpAddress += (DWORD)(DeviceIpAddress[1]) << (2 * 8);
		dwServerIpAddress += (DWORD)(DeviceIpAddress[2]) << (1 * 8);
		dwServerIpAddress += (DWORD)(DeviceIpAddress[3]) << (0 * 8);
		tcpServerInfo.sin_addr.s_addr = htonl(dwServerIpAddress);

		BOOL bOption = TRUE;
		setsockopt(tcpClientSocket_, IPPROTO_TCP, TCP_NODELAY, (char FAR*)(&bOption), sizeof(BOOL));

		if (connect(tcpClientSocket_, (LPSOCKADDR)&tcpServerInfo, sizeof(tcpServerInfo)) != 0)
		{
			//SetEvent(hEventDisconnected);
			if (needWsaCleanUp_)
			{
				WSACleanup();
				needWsaCleanUp_ = false;
			}
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FAILED);
			Sleep(1000);
			InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
			//OnDisconnect();
			return false;
		}
		InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_CONNECTED);

		//RecvOverlapped.hEvent = WSACreateEvent();
		rxThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(RxThreadStart), this, 0, NULL);
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmEthernet.Open()!");
		return false;
	}
}

bool EmEthernet::Close()
{
	try
	{
		EmService::WriteToLogGeneral("Close socket");

		pTimerThread_->bStopTimer_ = true;
		terminateRxThread_ = true;
		Sleep(2000);
		CloseHandle(rxThread_);
		
		//mainThread_ = null;

		WSACloseEvent(hTcpEvent_);
		closesocket(tcpClientSocket_);
		if (needWsaCleanUp_)
		{
			WSACleanup();
			needWsaCleanUp_ = false;
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Ethernet Close failed!");
		return false;
	}
	return true;
}

int EmEthernet::Write(DWORD size, BYTE* buffer)
{
	try
	{
		int bytesSent = send(tcpClientSocket_, (char*)buffer, size, NULL);
		if (size == bytesSent)
			return 0;
		else
			return -1;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Ethernet Write failed!");
		return -1;
	}
}

void EmEthernet::RxThread()
{
	WSABUF wsaDataBuf;
	wsaDataBuf.len = RXSOCKET_BUF_LEN;
	wsaDataBuf.buf = new char[RXSOCKET_BUF_LEN];
	try
	{
		int iResult;
		DWORD recvBytes = 0;
		WSANETWORKEVENTS networkEvents;
		DWORD flags = MSG_PARTIAL;

		hTcpEvent_ = WSACreateEvent();
		if (hTcpEvent_ == WSA_INVALID_EVENT)
		{
			iResult = 0;
			delete[] wsaDataBuf.buf;
			return;
		}
		iResult = WSAEventSelect(tcpClientSocket_, hTcpEvent_, FD_READ | FD_CLOSE);
		if (iResult != 0)
		{
			//iError = WSAGetLastError();
			delete[] wsaDataBuf.buf;
			return;
		}

		innerError_ = false;
		
		Sleep(100);

		while (true)
		{
			if (terminateRxThread_) break;

			if (connectStatus_ != CONNECTSTATUS_CONNECTED)
			{
				WSACloseEvent(hTcpEvent_);
				delete[] wsaDataBuf.buf;
				EmService::WriteToLogFailed("EmEthernet::RxThread() innerError");
				innerError_ = true;
				SetEvent(hEventDisconnected_);
				return;
			}
			switch (iResult = (int)WSAWaitForMultipleEvents(1, &hTcpEvent_, 0, 1000/*10*/, 0))
			{
				case WSA_WAIT_TIMEOUT:
					break;

				case WSA_WAIT_FAILED:
					WSACloseEvent(hTcpEvent_);
					delete[] wsaDataBuf.buf;
					EmService::WriteToLogFailed("innerError WSA_WAIT_FAILED");
					innerError_ = true;
					SetEvent(hEventDisconnected_);
					return;

				case WSA_WAIT_EVENT_0:
					WSAEnumNetworkEvents(tcpClientSocket_, hTcpEvent_, &networkEvents);
					if (networkEvents.lNetworkEvents == FD_CLOSE)
					{
						WSACloseEvent(hTcpEvent_);
						delete[] wsaDataBuf.buf;
						InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_FORCEDISCONNECT);
						EmService::WriteToLogFailed("innerError FD_CLOSE");
						innerError_ = true;
						SetEvent(hEventDisconnected_);
						return;
					}
					if (networkEvents.lNetworkEvents == FD_READ)
					{
						WSAResetEvent(hTcpEvent_);
						recvBytes = 0;
						flags = MSG_PARTIAL;

						iResult = WSARecv(tcpClientSocket_, &wsaDataBuf, 1, &recvBytes, &flags, 0, NULL);
						if(recvBytes > RXSOCKET_BUF_LEN)
							EmService::WriteToLogFailed("!!!!!RxThread(): recvBytes > RXSOCKET_BUF_LEN!");
						if (iResult == 0)
						{
							for (int j = 0; j < (WORD)recvBytes; j++)
							{
								(this->*InputFuncPointer)(wsaDataBuf.buf[j]);
							}
						}
						else
						{
							if (WSAGetLastError() != WSAEWOULDBLOCK)
							{
								WSACloseEvent(hTcpEvent_);
								delete[] wsaDataBuf.buf;
								EmService::WriteToLogFailed("innerError: WSAGetLastError() != WSAEWOULDBLOCK");
								innerError_ = true;
								SetEvent(hEventDisconnected_);
								return;
							}
						}
						break;
					}
					WSAResetEvent(hTcpEvent_);
					break;
			}
		}	//end of while
		delete[] wsaDataBuf.buf;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmEthernet.RxThread()!");
		innerError_ = true;
		SetEvent(hEventDisconnected_);
	}
	/*__finally
	{
		delete[] wsaDataBuf.buf;
	}*/
}

bool EmEthernet::ParseIPAddress(std::string addr, WORD* ipaddr)
{
	try 
	{
		DWORD itPoint1 = addr.find(".", 0);
		if(itPoint1 == std::string::npos) throw EmException("Invalid IP address format!");
		ipaddr[0] = (WORD)EmService::StringToNumber(addr.substr(0, itPoint1));

		DWORD itPoint2 = addr.find(".", itPoint1);
		if(itPoint2 == std::string::npos) throw EmException("Invalid IP address format!");
		ipaddr[1] = (WORD)EmService::StringToNumber(addr.substr(itPoint1, itPoint2));

		DWORD itPoint3 = addr.find(".", itPoint2);
		if(itPoint3 == std::string::npos) throw EmException("Invalid IP address format!");
		ipaddr[2] = (WORD)EmService::StringToNumber(addr.substr(itPoint2, itPoint3));
		ipaddr[3] = (WORD)EmService::StringToNumber(addr.substr(itPoint3, addr.size() - itPoint3));

		return true;
	}
	catch (EmException ex)
	{
		EmService::WriteToLogFailed("Error in ParseIPAddress(): " + ex.Message);
		return false;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in ParseIPAddress()!");
		return false;
	}
}

//}
