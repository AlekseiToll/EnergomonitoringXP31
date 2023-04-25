#include "stdafx.h"
#include "EmPort.h"
#include "Conversions.h"

//namespace DeviceIO
//{
const int EmPortSLIP::avgArchiveLength_ = 4096;

HANDLE HeartbeatTimerThread::hHeartbeatTimer_ = 0;
EmPortSLIP* HeartbeatTimerThread::owner_ = 0;
DWORD HeartbeatTimerThread::dwCommunicationTimeout_ = 0xFFFFFFFF;
DWORD HeartbeatTimerThread::dwHeartbeatCounter_ = 0;
bool HeartbeatTimerThread::bStopTimer_ = false;

EmPortType EmPort::portType_ = COM;
EmDeviceType EmPort::devType_ = NONE;
HWND EmPort::hMainWnd_ = 0;

std::string EmService::logFailedName = "LogFailedCpp.txt";
std::string EmService::logGeneralName = "LogGeneralCpp.txt";
std::string EmService::appDirectory = "";
bool EmService::appDirWasSet = false;

HANDLE EmPortSLIP::hEventCommunicationTimeout_ = INVALID_HANDLE_VALUE;
HANDLE EmPortSLIP::hEventDisconnected_ = INVALID_HANDLE_VALUE;
HANDLE EmPortSLIP::hEventPacketReceived_ = INVALID_HANDLE_VALUE;
HANDLE* EmPortSLIP::hEventsArray_ = 0;
WORD EmPortSLIP::devAddress_ = 0;
//HANDLE EmPortSLIP::mainThread_;
HANDLE EmPortSLIP::rxThread_ = 0;
Protocol EmPortSLIP::protocolState_ = (Protocol)0;
Stuff EmPortSLIP::stuffState_ = (Stuff)0;
StructReply* EmPortSLIP::actualReply_ = 0;
bool EmPortSLIP::innerError_ = false;
bool EmPortSLIP::modemConnectInProcess_ = false;
bool EmPortSLIP::terminateRxThread_ = false;
std::string EmPortSLIP::messBuffer_ = "";
HeartbeatTimerThread* EmPortSLIP::pTimerThread_;
HANDLE EmPortSLIP::hTimerThread_;

WORD EmPortSLIP::timeoutIO_ = 20;

HANDLE EmPortComSLIP::handle_ = INVALID_HANDLE_VALUE;
std::string EmPortComSLIP::portName_ = "";
int EmPortComSLIP::portSpeed_ = 0;
BYTE EmPortComSLIP::cts_state_ = 1;
BYTE EmPortComSLIP::dsr_state_ = 1;
BYTE EmPortComSLIP::dcd_state_ = 1;
BYTE EmPortComSLIP::ring_state_ = 1;

// timer function
VOID CALLBACK HeartbeatTimerAPCProcGlobal(LPVOID lpArg,             // Data value
									DWORD dwTimerLowValue,      // Timer low value
									DWORD dwTimerHighValue)		// Timer high value

{
	HeartbeatTimerThread *pTimerThread = (HeartbeatTimerThread*)lpArg;
	pTimerThread->HeartbeatTimerAPCProc();
}

void HeartbeatTimerThread::HeartbeatTimerAPCProc()
{
	InterlockedIncrement((LONG*)&dwHeartbeatCounter_);
	if (dwCommunicationTimeout_ != 0xFFFFFFFF)
	{
		if (dwHeartbeatCounter_ == dwCommunicationTimeout_)
		{
			dwCommunicationTimeout_ = 0xFFFFFFFF;
			std::stringstream ss;
			ss << "HeartbeatTimerCallBack() dwHeartbeatCounter_ = " << dwHeartbeatCounter_;
			ss << ",  " << EmService::GetCurrentDateTime();
			EmService::WriteToLogFailed(ss.str());
			owner_->Disconnect();
		}
	}
}

EmPortSLIP::EmPortSLIP(int devType, WORD devAddress, int portType, int hMainWnd) 
			: EmPort(devType, portType, hMainWnd)//,
				//innerError_(false), modemConnectInProcess_(false), terminateRxThread_(false)
{
	EmService::WriteToLogGeneral("EmPortSLIP constructor");

	terminateRxThread_ = false;

	devAddress_ = devAddress;
	InputFuncPointer = &EmPortSLIP::RxFunction; //InputFuncPointer = &RxFunction;

	// create heartbeat timer
	pTimerThread_ = new HeartbeatTimerThread(this);
	hTimerThread_ = CreateThread(NULL, 0, 
			(LPTHREAD_START_ROUTINE)(HeartbeatTimerThread::ThreadEntryStatic), pTimerThread_, 0, NULL);

	hEventCommunicationTimeout_ = CreateEvent(NULL, true, false, NULL);
	hEventPacketReceived_ = CreateEvent(NULL, true, false, NULL);
	hEventDisconnected_ = CreateEvent(NULL, true, false, NULL);
	hEventsArray_ = new HANDLE[3]; 
	hEventsArray_[0] = hEventCommunicationTimeout_; 
	hEventsArray_[1] = hEventPacketReceived_;
	hEventsArray_[2] = hEventDisconnected_ ;

	actualReply_ = new StructReply();
}

EmPortSLIP::~EmPortSLIP() 
{
	EmService::WriteToLogGeneral("EmPortSLIP destructor");

	CloseHandle(hEventCommunicationTimeout_);
	CloseHandle(hEventPacketReceived_);
	CloseHandle(hEventDisconnected_);
	delete[] hEventsArray_;

	pTimerThread_->bStopTimer_ = true;
	Sleep(1000);
	delete pTimerThread_;
	pTimerThread_ = 0;

	delete actualReply_;
}

/// <summary>
/// send query and receive answer into the buffer
/// </summary>
int EmPortSLIP::ReadData(WORD command, BYTE** buffer,
						 long* arr_params, long params_count, long* pAnswerLen)
{
	try
	{
		terminateRxThread_ = false;

		(*pAnswerLen) = 0;
		///////////////////////////////////////////////////////////////////
		// sending query
		std::vector<BYTE> query_list;
		BYTE* query_buffer = 0;

		std::vector<long> other_params;
		for(int iParam = 0; iParam < params_count; ++iParam)
			other_params.push_back(arr_params[iParam]);

		// идентификатор запроса AVG. нужен чтобы распознавать нужные пакеты от прибора
		WORD id_avg_query = 0;
		// тип запроса для команды COMMAND_AverageArchiveQuery
		WORD queryType = 0;

		WORD currentDevType = TYPE_ENERGOMONITOR32;
		if (devType_ == ETPQP) currentDevType = TYPE_ENERGOTESTER;

		// формируем пакет
		BYTE btemp0;
		query_list.push_back(SLIP_END);
		//uint dwtemp0 = (0xFFFF) | ((TYPE_ENERGOMONITOR32 & 0x000F) << (16 + 8));
		DWORD dwtemp0 = (DWORD)(devAddress_) | ((DWORD)(currentDevType & 0x000F) << (16 + 8));
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);

		query_list.push_back((BYTE)(command & 0xFF));        // команда
		query_list.push_back((BYTE)((command >> 8) & 0xFF));

		// заполняем длину данных и сами данные
		int datalen;
		switch (command)
		{
			case COMMAND_ReadQualityDatesByObject:
				if (other_params.size() >= 1)
				{
					// длина данных
					datalen = 2;
					query_list.push_back((BYTE)((datalen >> 0) & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// object id
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
				}
				break;

			case COMMAND_ReadRegistrationByIndex:
			case COMMAND_ReadAverageArchive3SecByIndex:
			case COMMAND_ReadAverageArchive10MinByIndex:
			case COMMAND_ReadAverageArchive2HourByIndex:
			case COMMAND_ReadDSIArchivesByRegistration:
			case COMMAND_ReadAverageArchive3SecMinMaxIndices:
			case COMMAND_ReadAverageArchive10MinMinMaxIndices:
			case COMMAND_ReadAverageArchive2HourMinMaxIndices:
				if (other_params.size() >= 1)
				{
					// длина данных
					datalen = 4;
					query_list.push_back((BYTE)((datalen >> 0) & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// record id or registration id
					query_list.push_back((BYTE)((other_params[0]) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] / 0x100) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] / 0x10000) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] / 0x1000000) & 0xFF));
				}
				break;

			case COMMAND_ReadRegistrationArchiveByIndex:
				if (other_params.size() >= 2)
				{
					// the length of data
					datalen = 6;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// data:
					// absolute index of the pqp archive
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 16) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 24) & 0xFF));
					// the number of the segment to read
					query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
				}
				break;

			case COMMAND_ReadQualityEntry:
				if (other_params.size() >= 3)
				{
					// длина данных
					datalen = 4;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					query_list.push_back((BYTE)(other_params[2] & 0xFF));
					query_list.push_back((BYTE)(other_params[1] & 0xFF));
				}
				else
				{
					// длина данных
					query_list.push_back(0);
					query_list.push_back(0);
				}
				break;

			//case COMMAND_ReadQualityEntryObjectDemand:
			//	if (other_params.size() >= 4)
			//	{
			//		// длина данных
			//		datalen = 6;
			//		query_list.push_back((BYTE)(datalen & 0xFF));
			//		query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
			//		// данные
			//		query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
			//		query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
			//		query_list.push_back((BYTE)(other_params[2] & 0xFF));
			//		query_list.push_back((BYTE)(other_params[1] & 0xFF));
			//		// номер объекта
			//		query_list.push_back((BYTE)((other_params[3] >> 0) & 0xFF));
			//		query_list.push_back((BYTE)((other_params[3] >> 8) & 0xFF));
			//	}
			//	else
			//	{
			//		// длина данных
			//		query_list.push_back(0);
			//		query_list.push_back(0);
			//	}
			//	break;

			case COMMAND_ReadQualityEntryByTimestampByObject:
				if (other_params.size() >= 7)
				{
					// длина данных
					datalen = 10;	// 5 слов
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					query_list.push_back((BYTE)(other_params[2] & 0xFF));
					query_list.push_back((BYTE)(other_params[1] & 0xFF));
					query_list.push_back((BYTE)(other_params[4] & 0xFF));
					query_list.push_back((BYTE)(other_params[3] & 0xFF));
					query_list.push_back((BYTE)(other_params[5] & 0xFF));
					query_list.push_back(0);
					// номер объекта
					query_list.push_back((BYTE)((other_params[6] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[6] >> 8) & 0xFF));
				}
				else
				{
					// длина данных
					query_list.push_back(0);
					query_list.push_back(0);
				}
				break;

			case COMMAND_ReadSystemData:
				if (other_params.size() > 0)
				{
					// длина данных
					datalen = 2 + 64;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					for (int i = 0; i < 64; i++)
						query_list.push_back((BYTE)0xAA);
				}
				else
				{
					// длина данных
					query_list.push_back(0);
					query_list.push_back(0);
				}
				break;

			case COMMAND_ReadEventLogger:
				if (other_params.size() >= 2)
				{
					// длина данных
					datalen = 6;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// номер записи
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 16) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 24) & 0xFF));
					// кол-во записей
					query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
				}
				break;

			case COMMAND_ReadDipSwellArchive:
				if (other_params.size() >= 3)
				{
					// длина данных
					datalen = 8;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// фаза
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					// номер записи
					query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 16) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 24) & 0xFF));
					// кол-во записей
					query_list.push_back((BYTE)((other_params[2] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[2] >> 8) & 0xFF));
				}
				break;

			case COMMAND_ReadDipSwellArchiveByObject:
				if (other_params.size() >= 4)
				{
					// длина данных
					datalen = 10;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// фаза
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					// номер записи
					query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 16) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 24) & 0xFF));
					// кол-во записей
					query_list.push_back((BYTE)((other_params[2] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[2] >> 8) & 0xFF));
					// номер объекта
					query_list.push_back((BYTE)((other_params[3] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[3] >> 8) & 0xFF));
				}
				break;

			case COMMAND_ReadDipSwellIndexByStartTimestamp:
			case COMMAND_ReadDipSwellIndexByEndTimestamp:
				if (other_params.size() >= 7)
				{
					// длина данных
					datalen = 10;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// фаза
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					// год
					query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
					// день
					query_list.push_back((BYTE)other_params[3]);
					// месяц
					query_list.push_back((BYTE)other_params[2]);
					// минуты
					query_list.push_back((BYTE)other_params[5]);
					// часы
					query_list.push_back((BYTE)other_params[4]);
					// секунды
					query_list.push_back((BYTE)other_params[6]);
					query_list.push_back(0);
				}
				break;

			case COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject:
				if (other_params.size() >= 13)
				{
					// длина данных
					datalen = 18;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные:
					// дата начала:
					// год
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					// день
					query_list.push_back((BYTE)other_params[2]);
					// месяц
					query_list.push_back((BYTE)other_params[1]);
					// минуты
					query_list.push_back((BYTE)other_params[4]);
					// часы
					query_list.push_back((BYTE)other_params[3]);
					// секунды
					query_list.push_back((BYTE)other_params[5]);
					query_list.push_back(0);
					// дата окончания:
					// год
					query_list.push_back((BYTE)((other_params[6] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[6] >> 8) & 0xFF));
					// день
					query_list.push_back((BYTE)other_params[8]);
					// месяц
					query_list.push_back((BYTE)other_params[7]);
					// минуты
					query_list.push_back((BYTE)other_params[10]);
					// часы
					query_list.push_back((BYTE)other_params[9]);
					// секунды
					query_list.push_back((BYTE)other_params[11]);
					query_list.push_back(0);
					// object id
					query_list.push_back((BYTE)((other_params[12] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[12] >> 8) & 0xFF));
				}
				break;

			//case COMMAND_ReadDipSwellIndexByStartTimestampByObject:
			//case COMMAND_ReadDipSwellIndexByEndTimestampByObject:
			//	if (other_params.size() >= 8)
			//	{
			//		// длина данных
			//		datalen = 12;
			//		query_list.push_back((BYTE)(datalen & 0xFF));
			//		query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
			//		// данные
			//		// фаза
			//		query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
			//		query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
			//		// год
			//		query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
			//		query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
			//		// день
			//		query_list.push_back((BYTE)other_params[3]);
			//		// месяц
			//		query_list.push_back((BYTE)other_params[2]);
			//		// минуты
			//		query_list.push_back((BYTE)other_params[5]);
			//		// часы
			//		query_list.push_back((BYTE)other_params[4]);
			//		// секунды
			//		query_list.push_back((BYTE)other_params[6]);
			//		query_list.push_back(0);
			//		// object id
			//		query_list.push_back((BYTE)((other_params[7] >> 0) & 0xFF));
			//		query_list.push_back((BYTE)((other_params[7] >> 8) & 0xFF));
			//	}
			//	break;

			case COMMAND_ReadAverageArchive3SecIndices:
			case COMMAND_ReadAverageArchive10MinIndices:
			case COMMAND_ReadAverageArchive2HourIndices:
				if (other_params.size() >= 1)
				{
					// длина данных
					datalen = 4;
					query_list.push_back((BYTE)((datalen >> 0) & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// registration id
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 16) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 24) & 0xFF));
				}
				break;

			case COMMAND_ReadAverageArchive3SecIndexByDateTime:
			case COMMAND_ReadAverageArchive10MinIndexByDateTime:
			case COMMAND_ReadAverageArchive2HourIndexByDateTime:
				if (other_params.size() >= 8)
				{
					// длина данных
					datalen = 32;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// registration id
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 16) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 24) & 0xFF));
					// the device uses only local time so we can put zero to uts fields
					// day
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// month
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// year
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// hour
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// min
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// sec
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// day
					query_list.push_back((BYTE)((other_params[3] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[3] >> 8) & 0xFF));
					// month
					query_list.push_back((BYTE)((other_params[2] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[2] >> 8) & 0xFF));
					// year
					query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
					// hour
					query_list.push_back((BYTE)((other_params[4] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[4] >> 8) & 0xFF));
					// min
					query_list.push_back((BYTE)((other_params[5] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[5] >> 8) & 0xFF));
					// sec
					query_list.push_back((BYTE)((other_params[6] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[6] >> 8) & 0xFF));
					// it won't be used
					query_list.push_back((BYTE)0);
					query_list.push_back((BYTE)0);
					// TimeZone
					query_list.push_back((BYTE)((other_params[7] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[7] >> 8) & 0xFF));
				}
				break;

			case COMMAND_Read3secArchiveByTimestamp:
			case COMMAND_Read1minArchiveByTimestamp:
			case COMMAND_Read30minArchiveByTimestamp:
				if (other_params.size() >= 6)
				{
					// длина данных
					datalen = 8;
					query_list.push_back((BYTE)(datalen & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// год
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					// день
					query_list.push_back((BYTE)other_params[2]);
					// месяц
					query_list.push_back((BYTE)other_params[1]);
					// минуты
					query_list.push_back((BYTE)other_params[4]);
					// часы
					query_list.push_back((BYTE)other_params[3]);
					// секунды
					query_list.push_back((BYTE)other_params[5]);
					query_list.push_back(0);
				}
				break;

			case COMMAND_Read3secArchiveByTimestampObjectDemand:
			case COMMAND_Read1minArchiveByTimestampObjectDemand:
			case COMMAND_Read30minArchiveByTimestampObjectDemand:
				if (other_params.size() >= 7)
				{
					// длина данных
					datalen = 10;
					query_list.push_back((BYTE)((datalen >> 0) & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// год
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
					// день
					query_list.push_back((BYTE)other_params[2]);
					// месяц
					query_list.push_back((BYTE)other_params[1]);
					// минуты
					query_list.push_back((BYTE)other_params[4]);
					// часы
					query_list.push_back((BYTE)other_params[3]);
					// секунды
					query_list.push_back((BYTE)other_params[5]);
					query_list.push_back(0);
					// object id
					query_list.push_back((BYTE)((other_params[6] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[6] >> 8) & 0xFF));
				}
				break;

			case COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand:
				if (other_params.size() >= 1)
				{
					// длина данных
					datalen = 2;
					query_list.push_back((BYTE)((datalen >> 0) & 0xFF));
					query_list.push_back((BYTE)((datalen >> 8) & 0xFF));
					// данные
					// object id
					query_list.push_back((BYTE)((other_params[0] >> 0) & 0xFF));
					query_list.push_back((BYTE)((other_params[0] >> 8) & 0xFF));
				}
				break;

			case COMMAND_AverageArchiveQuery:
				if (other_params.size() >= 2)
				{
					queryType = (WORD)other_params[0];
					switch (queryType)
					{
						case AAQ_TYPE_ResetAll:
							// длина данных
							datalen = 4;
							query_list.push_back((BYTE)(datalen & 0xFF));
							query_list.push_back((BYTE)((datalen >> 8) & 0xFF));

							// Тип запроса
							query_list.push_back((BYTE)((queryType >> 0) & 0xFF));
							query_list.push_back((BYTE)((queryType >> 8) & 0xFF));

							// Идентификатор запроса
							query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
							query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
							id_avg_query = (WORD)other_params[1];

							break;
						case AAQ_TYPE_Query:
							// длина данных
							datalen = 22 + (other_params.size() - 15) * 2;
							// здесь 15 - число элементов массива ДО списка параметров
							// 22 - длина в байтах этих 15-ти элементов массива

							query_list.push_back((BYTE)(datalen & 0xFF));
							query_list.push_back((BYTE)((datalen >> 8) & 0xFF));

							// данные
							// Тип запроса
							query_list.push_back((BYTE)((queryType >> 0) & 0xFF));
							query_list.push_back((BYTE)((queryType >> 8) & 0xFF));

							// Идентификатор запроса
							query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
							query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
							id_avg_query = (WORD)other_params[1];

							// тип записи (3 сек, 1 мин, 30 мин)
							query_list.push_back((BYTE)(((WORD)other_params[2] >> 0) & 0xFF));
							query_list.push_back((BYTE)(((WORD)other_params[2] >> 8) & 0xFF));

							// дата начала периода
							query_list.push_back((BYTE)(((WORD)other_params[3] >> 0) & 0xFF));
							query_list.push_back((BYTE)(((WORD)other_params[3] >> 8) & 0xFF));
							// день
							query_list.push_back((BYTE)other_params[5]);
							// месяц
							query_list.push_back((BYTE)other_params[4]);
							// минуты
							query_list.push_back((BYTE)other_params[7]);
							// часы
							query_list.push_back((BYTE)other_params[6]);
							// секунды
							query_list.push_back((BYTE)other_params[8]);
							query_list.push_back(0);

							// дата окончания периода
							query_list.push_back((BYTE)((other_params[9] >> 0) & 0xFF));
							query_list.push_back((BYTE)((other_params[9] >> 8) & 0xFF));
							// день
							query_list.push_back((BYTE)other_params[11]);
							// месяц
							query_list.push_back((BYTE)other_params[10]);
							// минуты
							query_list.push_back((BYTE)other_params[13]);
							// часы
							query_list.push_back((BYTE)other_params[12]);
							// секунды
							query_list.push_back((BYTE)other_params[14]);
							query_list.push_back(0);

							for (unsigned int iParam = 15; iParam < other_params.size(); ++iParam)
							{
								query_list.push_back((BYTE)((other_params[iParam] >> 0) & 0xFF));
								query_list.push_back((BYTE)((other_params[iParam] >> 8) & 0xFF));
							}
							break;
						case AAQ_TYPE_ReadStatus:
						default:
							// длина данных
							datalen = 4;
							query_list.push_back((BYTE)(datalen & 0xFF));
							query_list.push_back((BYTE)((datalen >> 8) & 0xFF));

							// Тип запроса
							query_list.push_back((BYTE)((queryType >> 0) & 0xFF));
							query_list.push_back((BYTE)((queryType >> 8) & 0xFF));

							// Идентификатор запроса
							query_list.push_back((BYTE)((other_params[1] >> 0) & 0xFF));
							query_list.push_back((BYTE)((other_params[1] >> 8) & 0xFF));
							id_avg_query = (WORD)other_params[1];

							break;
					}
				}
				break;

			/*case COMMAND_Read3secValues:
				{
				}
			case COMMAND_Read1minValues:
				{
				}
			case COMMAND_Read30minValues:
				{
				}*/
			default:
				// длина данных
				query_list.push_back(0);
				query_list.push_back(0);
				break;
		}

		// резервируем два байта под crc
		query_list.push_back(0);
		query_list.push_back(0);

		WORD crc = CalcCrc16(query_list);	// calculating crc
		query_list[query_list.size() - 2] = (BYTE)(crc & 0xFF);
		query_list[query_list.size() - 1] = (BYTE)((crc >> 8) & 0xFF);

		std::stringstream ss;

		// создаем буфер
		long bufLen = 0;
		if (!CreateBufferToSendRs232(query_list, &query_buffer, &bufLen))
			return -1;

		try
		{
			// отправляем запрос
			Sleep(100);
			ss << "command " << EmService::GetEm32CommandText(command) << ": was sent ";
			ss << EmService::GetCurrentDateTime();
			std::string debugStr = ss.str();
			if (portType_ == Modem)
			{
				WriteModemInfo(debugStr.c_str());
			}
			else EmService::WriteToLogGeneral(debugStr);

			if (portType_ == Modem || portType_ == GPRS)
			{
				timeoutIO_ = 60; //таймер срабатывает раз в секунду => таймаут = 1 min
				if (command == COMMAND_AverageArchiveQuery && queryType == AAQ_TYPE_Query)
				{
					timeoutIO_ = 180;  // 3 min
				}
			}
			else if (portType_ == Ethernet)
			{
				timeoutIO_ = 20;  // 30 sec
			}
			else
			{
				if (command == COMMAND_AverageArchiveQuery && queryType == AAQ_TYPE_Query)
				{
					timeoutIO_ = 300;  // 5 min
				}
				else
				{
					timeoutIO_ = 20; //таймаут = 20 сек

					if (devType_ == ETPQP)
						timeoutIO_ = 40; // 40 sec
				}
			}
			if (devType_ == ETPQP_A)
			{
				timeoutIO_ = 10; // 10 sec

				if(command == COMMAND_ReadAverageArchive3SecMinMaxIndices ||
					command == COMMAND_ReadAverageArchive10MinMinMaxIndices ||
					command == COMMAND_ReadAverageArchive2HourMinMaxIndices ||
					command == COMMAND_ReadRegistrationArchiveByIndex ||
					command == COMMAND_ReadRegistrationArchiveByIndex)
				{
					timeoutIO_ = 40; // 40 sec
				}
			}

			SetTimeoutCounter(timeoutIO_);

			stuffState_ = STUFF_IDLE;
			protocolState_ = PROTOCOL_LISTENING;

			// for debug
			/*EmService::WriteToLogGeneral("Buffer to write:\n");
			for(int iDebug = 0; iDebug < bufLen; ++iDebug)
			{
				EmService::WriteToLogGeneral2(EmService::NumberToString(query_buffer[iDebug]));
				if((iDebug % 20) == 0) EmService::WriteToLogGeneral2("\n");
			}
			EmService::WriteToLogGeneral("End of buffer to write:\n");*/
			// end of for debug

			int resW = Write(bufLen, query_buffer);
			if (resW != 0)
			{
				EmService::WriteToLogFailed("Error EmPortSLIP::Write()");
				return -3;					// error exit
			}
			delete[] query_buffer;
		}
		catch(...)
		{
			EmService::WriteToLogFailed("Exeption in ReadData() before receiving");
			delete[] query_buffer;
			throw;
		}

		//#if DEBUG
		// если большой пакет - ждем чтобы прибор успел его получить
		//if (bufLen > 3000)
		//{
		//	if (portType_ == Modem ||
		//			((portType_ == COM || portType_ == Rs485) && portSpeed_ < 115200))
		//	{
		//		EmService::WriteToLogGeneral("query_buffer.Length > 3000: wait 15 sec");
		//		Sleep(15000);
		//	}
		//	else
		//	{
		//		EmService::WriteToLogGeneral("query_buffer.Length > 3000: wait 5 sec");
		//		Sleep(5000);
		//	}
		//}
		//#endif

		WORD currentSenderType = SENDERTYPE_ENERGOMONITOR32;
		if (devType_ == ETPQP) currentSenderType = SENDERTYPE_ENERGOTESTER;

		// получаем ответ
		bool bAllPacketsReceived = false, bTypeSenderIsCorrect = false;
		//BYTE* tempBuffer;

		/////////////////////////////////////////////////////////
		std::list<BYTE> listTempBuffer;

		while (!bAllPacketsReceived)
		{
			protocolState_ = PROTOCOL_LISTENING;
			stuffState_ = STUFF_IDLE;

			bTypeSenderIsCorrect = false;
			while (!bTypeSenderIsCorrect) // the cycle eliminates echo packets
			{
				switch (WaitForMultipleObjects(3, hEventsArray_, false, INFINITE))
				{
					case (WAIT_OBJECT_0 + 0):
						//сбрасываем таймаут
						ResetTimeoutCounter();
						ss.str("");
						ss << "ReadData(): hEventCommunicationTimeout_ " << EmService::GetCurrentDateTime();
						EmService::WriteToLogFailed(ss.str());
						return -1;

					case (WAIT_OBJECT_0 + 1):
						ResetEvent(hEventPacketReceived_);

						//сбрасываем таймаут
						ResetTimeoutCounter();

						// проверяем прибором ли был отправлен пакет (только для em32)
						if (devType_ == EM32)
						{
							BYTE typeSender = (BYTE)(actualReply_->dwAddress >> 24);
							if ((typeSender & (BYTE)currentSenderType) == 0)
							{
								EmService::WriteToLogFailed("typeSender wrong!");
							}
							else
							{
								bTypeSenderIsCorrect = true;
							}
						}
						else
						{
							bTypeSenderIsCorrect = true;
						}
						break;
					case (WAIT_OBJECT_0 + 2):
						ResetEvent(hEventDisconnected_);
						//сбрасываем таймаут
						ResetTimeoutCounter();
						// закрываем порт
						Close();

						EmService::WriteToLogFailed("ReadData(): hEventDisconnected_");
						return -2;
				}
				//если будем продолжать прием пакетов, ставим таймаут заново
				if (!bTypeSenderIsCorrect)
				{
					SetTimeoutCounter(timeoutIO_);
				}
			}

			EmService::WriteToLogGeneral(EmService::GetEm32CommandText(command));
			EmService::WriteToLogGeneral(EmService::GetEm32CommandText(actualReply_->wCommand));
			EmService::WriteToLogGeneral(EmService::NumberToString(queryType));
			EmService::WriteToLogGeneral(EmService::NumberToString(devType_));

			////////////////////////////////
			// анализируем полученный пакет:
			////////////////////////////////
			// если получили ответ "Не найдены данные, удовлетворяющие условию", то выходим без
			// копирования буфера
			if (/*command != COMMAND_AverageArchiveQuery*/
				actualReply_->wCommand == COMMAND_OK)
			{
				EmService::WriteToLogGeneral("actualReply_.wCommand = COMMAND_OK");
				//break;
			}
			if (actualReply_->wCommand == COMMAND_NO_DATA)
			{
				EmService::WriteToLogGeneral("actualReply_.wCommand = COMMAND_NO_DATA");
				break;
			}
			if (actualReply_->wCommand == COMMAND_UNKNOWN_COMMAND ||
				actualReply_->wCommand == COMMAND_CRC_ERROR ||
				actualReply_->wCommand == COMMAND_BAD_DATA ||
				actualReply_->wCommand == COMMAND_BAD_PASSWORD ||
				actualReply_->wCommand == COMMAND_ACCESS_ERROR ||
				actualReply_->wCommand == COMMAND_CHECK_FAILED)
			{
				std::string answer = "actualReply_.wCommand = ";
				answer += EmService::GetEm32CommandText(actualReply_->wCommand);
				EmService::WriteToLogGeneral(answer);
				//break;
				return -1;	//?????????????????
			}

			//если пришел ответ не на ту команду, которая была запрошена, то игнорируем пакет
			if (devType_ == ETPQP)
			{
				if ((command != actualReply_->wCommand) &&
					(actualReply_->wCommand != COMMAND_OK) &&
					!(command == COMMAND_ReadQualityDatesByObject && 
						actualReply_->wCommand == COMMAND_ReadQualityDates) &&
					!(command == COMMAND_ReadQualityEntryByTimestampByObject &&
						actualReply_->wCommand == COMMAND_ReadQualityEntry) &&
					!(command == COMMAND_ReadDipSwellArchiveByObject &&
						actualReply_->wCommand == COMMAND_ReadDipSwellArchive) &&
					//!(command == COMMAND_ReadDipSwellIndexByStartTimestampByObject &&
					//	actualReply_->wCommand == COMMAND_ReadDipSwellIndexByStartTimestamp) &&
					!(command == COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject &&
						actualReply_->wCommand == 
								COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject) &&
					//!(command == COMMAND_ReadDipSwellIndexByEndTimestampByObject &&
						//actualReply_->wCommand == COMMAND_ReadDipSwellIndexByEndTimestamp) &&
					!(command == COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand &&
						actualReply_->wCommand == COMMAND_ReadEarliestAndLatestAverageTimestamp) &&
					!(command == COMMAND_Read3secArchiveByTimestampObjectDemand &&
						actualReply_->wCommand == COMMAND_Read3secArchiveByTimestamp) &&
					!(command == COMMAND_Read1minArchiveByTimestampObjectDemand &&
						actualReply_->wCommand == COMMAND_Read1minArchiveByTimestamp) &&
					!(command == COMMAND_Read30minArchiveByTimestampObjectDemand &&
						actualReply_->wCommand == COMMAND_Read30minArchiveByTimestamp))
				{
					std::stringstream ss;
					ss << "ReadData(): unexpected command answer " << 
						EmService::GetEm32CommandText(actualReply_->wCommand);
					EmService::WriteToLogGeneral(ss.str());
					SetTimeoutCounter(timeoutIO_);
					continue;
				}
			}

			// усредненные, запрашиваемые через COMMAND_AverageArchiveQuery, могут приходить
			// несколькими пакетами, поэтому надо остаться в цикле для получения оставшихся пакетов.
			// то же самое относится к командам COMMAND_ReadAverageArchiveXXXIndices, COMMAND_ReadDSIArchivesByRegistration.
			// а в этот if мы попадаем во всех остальных случаях, чтобы выйти из цикла
			if (command != COMMAND_AverageArchiveQuery &&
				actualReply_->wCommand != COMMAND_AverageArchiveQuery &&
				command != COMMAND_ReadAverageArchive3SecIndices &&
				actualReply_->wCommand != COMMAND_ReadAverageArchive3SecIndices &&
				command != COMMAND_ReadAverageArchive10MinIndices &&
				actualReply_->wCommand != COMMAND_ReadAverageArchive10MinIndices &&
				command != COMMAND_ReadAverageArchive2HourIndices &&
				actualReply_->wCommand != COMMAND_ReadAverageArchive2HourIndices &&
				command != COMMAND_ReadDSIArchivesByRegistration &&
				actualReply_->wCommand != COMMAND_ReadDSIArchivesByRegistration)
			{
				if(actualReply_->wLength > 0x8000)
					EmService::WriteToLogFailed(
					"!!!!!!!!!!ReadData(): actualReply_->wLength > 0x8000!");
				std::copy(actualReply_->bData, actualReply_->bData + actualReply_->wLength,
					std::back_inserter(listTempBuffer));
				break;
			}

			// если запрашивались не усредненные, а получили усредненные, то игнорируем пакет
			if (command != COMMAND_AverageArchiveQuery &&
				actualReply_->wCommand == COMMAND_AverageArchiveQuery)
			{
				EmService::WriteToLogFailed("ReadData(): unexpected AVG packet!");
				SetTimeoutCounter(timeoutIO_);
				continue;
			}

			// если запрашивались не индексы, а получили индексы, то игнорируем пакет
			if ((command != COMMAND_ReadAverageArchive3SecIndices &&
				actualReply_->wCommand == COMMAND_ReadAverageArchive3SecIndices) ||
				(command != COMMAND_ReadAverageArchive10MinIndices &&
				actualReply_->wCommand == COMMAND_ReadAverageArchive10MinIndices) ||
				(command != COMMAND_ReadAverageArchive2HourIndices &&
				actualReply_->wCommand == COMMAND_ReadAverageArchive2HourIndices))
			{
				EmService::WriteToLogFailed("ReadData(): unexpected AVG indexes!");
				SetTimeoutCounter(timeoutIO_);
				continue;
			}

			// если запрашивались не многопакетные провалы, а получили их, то игнорируем пакет
			if (command != COMMAND_ReadDSIArchivesByRegistration &&
				actualReply_->wCommand == COMMAND_ReadDSIArchivesByRegistration)
			{
				EmService::WriteToLogFailed("ReadData(): unexpected DNS packet!");
				SetTimeoutCounter(timeoutIO_);
				continue;
			}

			// если запрашивались усредненные, а получили что-то другое - выходим
			if (command == COMMAND_AverageArchiveQuery &&
				actualReply_->wCommand != COMMAND_AverageArchiveQuery)
			{
				EmService::WriteToLogFailed("ReadData(): needed AVG packet!");
				break;
			}

			if (devType_ != ETPQP_A)
			{
				// если запрашивались усредненные, но id запроса не совпадает с id ответа, 
				// то игнорируем пакет
				WORD curId = Conversions::bytes_2_ushort(actualReply_->bData, 2);
				if (id_avg_query != curId)
				{
					std::stringstream ss;
					ss << "ReadData(): invalid id AVG query! curId = " << curId << "  id_avg_query = ";
					ss << id_avg_query;
					EmService::WriteToLogFailed(ss.str());
					SetTimeoutCounter(timeoutIO_);
					continue;
				}

				// если тип запроса и тип ответа не совпадают, то игнорируем пакет
				WORD curType = Conversions::bytes_2_ushort(actualReply_->bData, 0);
				if (curType != queryType)
				{
					std::stringstream ss;
					ss << "ReadData(): invalid AVG query type!  curType = " << curType << 
						"  queryType = ";
					ss << queryType;
					EmService::WriteToLogFailed(ss.str());
					SetTimeoutCounter(timeoutIO_);
					continue;
				}

				// если это была команда Reset для средних, то ждем немного, 
				// чтобы прибор успел среагировать
				if (queryType == 1 && curType == 1 /*ResetAll*/)
				{
					EmService::WriteToLogGeneral("queryType == 1 && curType == 1");
					Sleep(1500);
				}

				// если запрашивались усредненные, то проверяем последний ли это сегмент
				if (queryType == AAQ_TYPE_Query)
				{
					WORD numSegment = Conversions::bytes_2_ushort(actualReply_->bData, 4);
					std::stringstream ss;
					ss << "AVG number of segment: " << numSegment;
					EmService::WriteToLogGeneral(ss.str());

					if (numSegment != 0xFFFF)
						bAllPacketsReceived = false;
					else
						bAllPacketsReceived = true;

					Sleep(0);

					// получен очередной сегмент, поэтому меняем прогрессбар
					PostMessage(hMainWnd_, WM_USER + 1, 1, 0);
				}
				else
					bAllPacketsReceived = true;

				// сохраняем пакет усредненных:
				if(actualReply_->wLength > 0x8000)
					EmService::WriteToLogFailed(
					"!!!!!!!!!!ReadData(): actualReply_->wLength > 0x8000!");
				std::copy(actualReply_->bData, actualReply_->bData + actualReply_->wLength,
					std::back_inserter(listTempBuffer));
			}
			
			if (devType_ == ETPQP_A)
			{
				// если запрашивались индексы усредненных или провалы, то проверяем последний ли это пакет
				if (command == COMMAND_ReadAverageArchive3SecIndices ||
					command == COMMAND_ReadAverageArchive10MinIndices ||
					command == COMMAND_ReadAverageArchive2HourIndices ||
					command == COMMAND_ReadDSIArchivesByRegistration)
				{
					std::stringstream ss;
					ss << "AVG indexes or DNS packet length: " << actualReply_->wLength;
					EmService::WriteToLogGeneral(ss.str());

					if (actualReply_->wLength > 0)
						bAllPacketsReceived = false;
					else
						bAllPacketsReceived = true;

					Sleep(0);
				}
				else
					bAllPacketsReceived = true;

				// сохраняем пакет индексов усредненных или провалов:
				if (actualReply_->wLength > 0)
				{
					if (command == COMMAND_ReadAverageArchive3SecIndices ||
						command == COMMAND_ReadAverageArchive10MinIndices ||
						command == COMMAND_ReadAverageArchive2HourIndices ||
						command == COMMAND_ReadDSIArchivesByRegistration)
					{
						std::copy(actualReply_->bData, actualReply_->bData + actualReply_->wLength,
							std::back_inserter(listTempBuffer));
					}
				}
			}

			//если будем продолжать прием пакетов, ставим таймаут заново
			if (!bAllPacketsReceived)
			{
				SetTimeoutCounter(timeoutIO_);
			}
		}
		
		// если удалось что-то считать, сохраняем данные
		if (listTempBuffer.size() > 0)
		{
			(*pAnswerLen) = listTempBuffer.size();
			(*buffer) = new BYTE[listTempBuffer.size()];
			std::copy(listTempBuffer.begin(), listTempBuffer.end(), (*buffer));
		}
		else
			buffer = 0;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortSLIP::ReadData()");
		return -1;
	}
	return 0;
}

/// <summary>
/// send query and receive answer into the buffer
/// </summary>
int EmPortSLIP::WriteData(WORD command, BYTE* buffer, short bufLength)
{
	try
	{
		terminateRxThread_ = false;
		///////////////////////////////////////////////////////////////////
		// sending query
		std::vector<BYTE> query_list;
		BYTE* query_buffer = 0;

		WORD currentDevType = TYPE_ENERGOMONITOR32;
		if (devType_ == ETPQP) currentDevType = TYPE_ENERGOTESTER;

		// формируем пакет
		BYTE btemp0;
		query_list.push_back(SLIP_END);
		//uint dwtemp0 = (0xFFFF) | ((TYPE_ENERGOMONITOR32 & 0x000F) << (16 + 8));
		DWORD dwtemp0 = (DWORD)(devAddress_) | ((DWORD)(currentDevType & 0x000F) << (16 + 8));
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);
		btemp0 = (BYTE)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.push_back(btemp0);

		query_list.push_back((BYTE)(command & 0xFF));        // команда
		query_list.push_back((BYTE)((command >> 8) & 0xFF));

		// заполняем длину данных и сами данные
		switch (command)
		{
			case COMMAND_WriteSets:
				if (bufLength > 0)
				{
					// длина данных
					query_list.push_back((BYTE)((bufLength >> 0) & 0xFF));
					query_list.push_back((BYTE)((bufLength >> 8) & 0xFF));
					// данные
					for (int iByte = 0; iByte < bufLength; ++iByte)
					{
						query_list.push_back(buffer[iByte]);
					}
				}
				break;

			case COMMAND_WriteSystemData:
				if (bufLength > 0)
				{
					// длина данных
					query_list.push_back((BYTE)((bufLength >> 0) & 0xFF));
					query_list.push_back((BYTE)((bufLength >> 8) & 0xFF));
					// данные
					for (int iByte = 0; iByte < bufLength; ++iByte)
					{
						query_list.push_back(buffer[iByte]);
					}
				}
				else
				{
					// длина данных
					query_list.push_back(0);
					query_list.push_back(0);
				}
				break;

			default:
				// длина данных
				query_list.push_back(0);
				query_list.push_back(0);
				break;
		}

		// резервируем два байта под crc
		query_list.push_back(0);
		query_list.push_back(0);

		WORD crc = CalcCrc16(query_list);	// calculating crc
		query_list[query_list.size() - 2] = (BYTE)(crc & 0xFF);
		query_list[query_list.size() - 1] = (BYTE)((crc >> 8) & 0xFF);

		// создаем буфер
		long bufLen = 0;
		if (!CreateBufferToSendRs232(query_list, &query_buffer, &bufLen))
			return -1;

		std::stringstream ss;
		WORD timeOut;

		try
		{
			// отправляем запрос
			Sleep(100);
			ss << "command " << EmService::GetEm32CommandText(command) << ": was sent ";
			ss << EmService::GetCurrentDateTime();
			std::string debugStr = ss.str();
			if (portType_ == Modem)
			{
				WriteModemInfo(debugStr.c_str());
			}
			else EmService::WriteToLogGeneral(debugStr);

			if (portType_ == Modem || portType_ == GPRS)
			{
				timeOut = 60; //таймер срабатывает раз в секунду => таймаут = 1 min
			}
			else if (portType_ == Ethernet)
			{
				timeOut = 30;  // 30 sec
			}
			else
			{
				timeOut = 20; //таймаут = 20 сек

				if (devType_ == ETPQP)
					timeOut = 40; // 40 sec
			}
			SetTimeoutCounter(timeOut);

			stuffState_ = STUFF_IDLE;
			protocolState_ = PROTOCOL_LISTENING;

			std::list<BYTE> listTempBuffer;

			int resW = Write(bufLen, query_buffer);
			if (resW != 0)
			{
				EmService::WriteToLogFailed("Error EmPortSLIP::Write()");
				return -3;					// error exit
			}
			delete[] query_buffer;
		}
		catch(...)
		{
			delete[] query_buffer;
			throw;
		}

		WORD currentSenderType = SENDERTYPE_ENERGOMONITOR32;
		if (devType_ == ETPQP) currentSenderType = SENDERTYPE_ENERGOTESTER;

		// получаем ответ
		bool bAllPacketsReceived = false, bTypeSenderIsCorrect = false;
		//BYTE* tempBuffer;

		/////////////////////////////////////////////////////////
		bTypeSenderIsCorrect = false;
		while (!bTypeSenderIsCorrect) // the cycle eliminates echo packets
		{
			switch (WaitForMultipleObjects(3, hEventsArray_, false, INFINITE))
			{
				case (WAIT_OBJECT_0 + 0):
					//сбрасываем таймаут
					ResetTimeoutCounter();
					ss.str("");
					ss << "ReadData(): hEventCommunicationTimeout_ " << EmService::GetCurrentDateTime();
					EmService::WriteToLogFailed(ss.str());
					return -1;

				case (WAIT_OBJECT_0 + 1):
					ResetEvent(hEventPacketReceived_);

					//сбрасываем таймаут
					ResetTimeoutCounter();

					// проверяем прибором ли был отправлен пакет (только для em32)
					if (devType_ == EM32)
					{
						BYTE typeSender = (BYTE)(actualReply_->dwAddress >> 24);
						if ((typeSender & (BYTE)currentSenderType) == 0)
						{
							EmService::WriteToLogFailed("typeSender wrong!");
						}
						else
						{
							bTypeSenderIsCorrect = true;
						}
					}
					else
					{
						bTypeSenderIsCorrect = true;
					}
					break;
				case (WAIT_OBJECT_0 + 2):
					ResetEvent(hEventDisconnected_);
					//сбрасываем таймаут
					ResetTimeoutCounter();
					// закрываем порт
					Close();

					EmService::WriteToLogFailed("ReadData(): hEventDisconnected_");
					return -2;
			}
			//если будем продолжать прием пакетов, ставим таймаут заново
			if (!bTypeSenderIsCorrect)
			{
				SetTimeoutCounter(timeOut);
			}
		}

		// анализируем полученный пакет:
		if (actualReply_->wCommand == COMMAND_OK)
		{
			EmService::WriteToLogGeneral("actualReply_.wCommand = COMMAND_OK");
		}
		else
		{
			std::string answer = "actualReply_.wCommand = ";
			answer += EmService::GetEm32CommandText(actualReply_->wCommand);
			EmService::WriteToLogGeneral(answer);
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortSLIP::ReadData()");
		return -1;
	}
	return 0;
}

void EmPortSLIP::RxFunction(BYTE new_byte)
{
	try
	{
		if (protocolState_ == PROTOCOL_PACKET_BEING_PROCESSED)
		{
			while (true)
			{
				if (innerError_) break;
				if (protocolState_ != PROTOCOL_PACKET_BEING_PROCESSED) break;
				Sleep(0);
			}
		}
		//__________________________________________________________________
		if (new_byte == SLIP_END)
		{
			stuffState_ = STUFF_END;
		}
		//__________________________________________________________________
		switch (stuffState_)
		{
			//-------------------------------------------------------------	
			case STUFF_IDLE:

				switch (new_byte)
				{
					//-------------------------------------------------------------	
					case SLIP_ESC:
						stuffState_ = STUFF_BYTE1;
						break;
					//-------------------------------------------------------------	
					default:
						stuffState_ = STUFF_NEWBYTE;
						break;
				}
				break;
			//-------------------------------------------------------------	
			case STUFF_BYTE1:
				stuffState_ = STUFF_IDLE;
				switch (new_byte)
				{
					//-------------------------------------------------------------	
					case SLIP_ESC_END:
						new_byte = SLIP_END;
						stuffState_ = STUFF_NEWBYTE;
						break;
					//-------------------------------------------------------------	
					case SLIP_ESC_ESC:
						new_byte = SLIP_ESC;
						stuffState_ = STUFF_NEWBYTE;
						break;
					//-------------------------------------------------------------	
					default:
						protocolState_ = PROTOCOL_LISTENING;
						break;
				}
				break;
			//-------------------------------------------------------------	
			case STUFF_END:
				stuffState_ = STUFF_NEWBYTE;
				protocolState_ = PROTOCOL_LISTENING;
				break;
			//-------------------------------------------------------------	
			default:
				stuffState_ = STUFF_IDLE;
				protocolState_ = PROTOCOL_LISTENING;
				break;
			//-------------------------------------------------------------	
		}
		//__________________________________________________________________
		if (stuffState_ == STUFF_NEWBYTE)
		{
			// пришел байт, поэтому сбрасываем таймаут
			if(timeoutIO_ > 0 && timeoutIO_ <= 300)
				SetTimeoutCounter(timeoutIO_);

			stuffState_ = STUFF_IDLE;
			switch (protocolState_)
			{
				//-------------------------------------------------------------	
				case PROTOCOL_LISTENING:
					if (new_byte == SLIP_END)
						protocolState_ = PROTOCOL_ADDRESS0_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_ADDRESS0_EXPECTED:
					/*if (new_byte == SLIP_END)
					{
						protocolState_ = PROTOCOL_ADDRESS0_EXPECTED;
						break;
					}*/
					actualReply_->dwAddress = 0x000000FF & (DWORD)new_byte;
					protocolState_ = PROTOCOL_ADDRESS1_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_ADDRESS1_EXPECTED:
					actualReply_->dwAddress |= (DWORD)(new_byte << 8);
					protocolState_ = PROTOCOL_ADDRESS2_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_ADDRESS2_EXPECTED:
					actualReply_->dwAddress |= (DWORD)(new_byte << 16);
					protocolState_ = PROTOCOL_ADDRESS3_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_ADDRESS3_EXPECTED:
					actualReply_->dwAddress |= (DWORD)(new_byte << 24);
					protocolState_ = PROTOCOL_COMMAND0_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_COMMAND0_EXPECTED:
					actualReply_->wCommand = (WORD)new_byte;
					protocolState_ = PROTOCOL_COMMAND1_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_COMMAND1_EXPECTED:
					actualReply_->wCommand |= ((WORD)(new_byte << 8));
					protocolState_ = PROTOCOL_LENGTH0_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_LENGTH0_EXPECTED:
					actualReply_->wLength = (WORD)(0x00FF & (WORD)new_byte);
					protocolState_ = PROTOCOL_LENGTH1_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_LENGTH1_EXPECTED:
					actualReply_->wLength |= ((WORD)(new_byte << 8));
					if (actualReply_->wLength == 0)
					{
						protocolState_ = PROTOCOL_CRC0_EXPECTED;
						EmService::WriteToLogGeneral("actualReply_->wLength = 0");
					}
					else
					{
						protocolState_ = PROTOCOL_DATA_EXPECTED;
						actualReply_->wCounter = 0;
					}
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_DATA_EXPECTED:
					if(actualReply_->wCounter > 0x8000) // 0x8000 - это размер буфера
						EmService::WriteToLogFailed("!!!!!!!!!!RxFunction(): actualReply_->wCounter > 0x8000!");
					actualReply_->bData[actualReply_->wCounter] = new_byte;
					actualReply_->wCounter++;
					if (actualReply_->wCounter == actualReply_->wLength)
					{
						protocolState_ = PROTOCOL_CRC0_EXPECTED;
					}
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_CRC0_EXPECTED:
					actualReply_->wCRC = (WORD)(0x00FF & (WORD)new_byte);
					protocolState_ = PROTOCOL_CRC1_EXPECTED;
					break;
				//-------------------------------------------------------------	
				case PROTOCOL_CRC1_EXPECTED:
					actualReply_->wCRC |= (WORD)(new_byte << 8);
					WORD wTemp;
					WORD i;
					wTemp = CRC16_SEED;
					crc16((BYTE)(((actualReply_->dwAddress) >> (8 * 0)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->dwAddress) >> (8 * 1)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->dwAddress) >> (8 * 2)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->dwAddress) >> (8 * 3)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->wCommand) >> (8 * 0)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->wCommand) >> (8 * 1)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->wLength) >> (8 * 0)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->wLength) >> (8 * 1)) & 0xFF), &wTemp);
					if(actualReply_->wLength > 0x8000)
						EmService::WriteToLogFailed("!!!!!!!!!!RxFunction(): actualReply_->wLength > 0x8000!");
					for (i = 0; i < (actualReply_->wLength); i++)
						crc16(actualReply_->bData[i], &wTemp);
					crc16((BYTE)(((actualReply_->wCRC) >> (8 * 0)) & 0xFF), &wTemp);
					crc16((BYTE)(((actualReply_->wCRC) >> (8 * 1)) & 0xFF), &wTemp);
					if (wTemp == 0)
					{
						std::string debugStr = std::string("command:  ");
						debugStr += EmService::GetEm32CommandText(actualReply_->wCommand);
						debugStr += "    crc ok!  ";
						debugStr += EmService::GetCurrentDateTime();
						EmService::WriteToLogGeneral(debugStr);

						protocolState_ = PROTOCOL_PACKET_BEING_PROCESSED;
						SetEvent(hEventPacketReceived_);
					}
					else
					{
						std::string debugStr = std::string("command:  ");
						debugStr += EmService::GetEm32CommandText(actualReply_->wCommand);
						debugStr += "    crc failed!  ";
						debugStr += EmService::GetCurrentDateTime();
						EmService::WriteToLogFailed(debugStr);

						protocolState_ = PROTOCOL_ERROR;
						SetEvent(hEventCommunicationTimeout_);//??????????????? почему таймаут?
					}
					break;
			}
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in RxFunction()");
		throw;
	}
}

bool EmPortSLIP::CreateBufferToSendRs232(std::vector<BYTE>& buf_temp, BYTE** buf, long* bufLen)
{
	(*buf) = 0;
	try
	{
		std::list<BYTE> buf_list(buf_temp.begin(), buf_temp.end());
		std::list<BYTE>::iterator it = buf_list.begin();
		// начинаем с 1-го индекса, т.к. с 0-ым идет SLIP_END, который не надо менять
		++it;
		for (; it != buf_list.end(); ++it)
		{
			switch (*it)
			{
				case SLIP_END:
					(*it) = SLIP_ESC;
					buf_list.insert((++it), SLIP_ESC_END);
					--it;
					break;
				case SLIP_ESC:
					(*it) = SLIP_ESC;
					buf_list.insert((++it), SLIP_ESC_ESC);
					--it;
					break;
				default:
					break;
			}
		}
		(*bufLen) = buf_list.size();
		(*buf) = new BYTE[buf_list.size()];
		int iByte = 0;
		for (it = buf_list.begin(); it != buf_list.end(); ++it)
		{
			(*buf)[iByte++] = *it;
		}
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in CreateBufferToSendRs232()");
		if(buf != 0) delete[] buf;
		buf = 0;
		return false;
	}
}

WORD EmPortSLIP::CalcCrc16(std::vector<BYTE>& data)
{
	WORD temp, crc = CRC16_SEED;
	try
	{
		// последние два байта не рассматриваем, т.к. они зарезервированы для crc,
		// первый байт тоже пропускаем
		for (unsigned int i = 1; i < data.size() - 2; ++i)
		{
			temp = (WORD)((data[i] ^ crc) & 0xFF);
			crc >>= 8;
			crc ^= CRC16Table[temp];
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in CalcCrc16()");
		throw;
	}
	return crc;
}

WORD EmPortSLIP::CalcCrc16(std::vector<BYTE>& buffer, int start, int len)
{
	WORD temp, crc = CRC16_SEED;
	try
	{
		// последние два байта не рассматриваем, т.к. они зарезервированы для crc,
		// первый байт тоже пропускаем
		for (unsigned int i = start; i < (len + start - 2) && buffer.size() > len; ++i)
		{
			temp = (WORD)((buffer[i] ^ crc) & 0xFF);
			crc >>= 8;
			crc ^= CRC16Table[temp];
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in CalcCrc16()");
		throw;
	}
	return crc;
}

/////////////////////////////////////////////////////////

int EmPortComSLIP::Write(DWORD size, BYTE* buffer)
{
	try
	{
		if (handle_ == INVALID_HANDLE_VALUE) return -1;

		OVERLAPPED osWrite = {0};
		DWORD dwWritten;
		DWORD dwRes;
		osWrite.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

		if (WriteFile(handle_, buffer, size, &dwWritten, &osWrite) == FALSE) 
		{
			DWORD errCode = GetLastError();
			if (errCode != ERROR_IO_PENDING) 
			{
				std::string debugStr = "Error in EmPortComSLIP::Write() 1: errCode = ";
				debugStr += EmService::NumberToString(errCode);
				EmService::WriteToLogFailed(debugStr);
				return -1;
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
		EmService::WriteToLogFailed("Error in EmPortComSLIP::Write()");
		throw;
	}
}

int EmPortComSLIP::UpdateModemStatus()
{
	try
	{
		BOOL btemp;
		DWORD modem_status = 0;
		btemp = GetCommModemStatus(handle_, &modem_status);

		if (btemp)
		{
			if ((modem_status & MS_CTS_ON) == 0) cts_state_ = 1; else cts_state_ = 0;
			if ((modem_status & MS_DSR_ON) == 0) dsr_state_ = 1; else dsr_state_ = 0;
			if ((modem_status & MS_RING_ON) == 0) ring_state_ = 1; else ring_state_ = 0;
			if ((modem_status & MS_RLSD_ON) == 0) dcd_state_ = 1; else dcd_state_ = 0;
		}
		return 0;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortComSLIP::UpdateModemStatus()");
		return -1;
	}
}

void EmPortComSLIP::Running()
{
	try
	{
		DWORD dwRes;
		DWORD dwCommEvent;
		DWORD dwStoredFlags;
		bool fWaitingOnStat = false;
		OVERLAPPED osStatus = {0};
		DWORD dwOvRes;

		dwStoredFlags = EV_BREAK | EV_CTS | EV_DSR | EV_ERR | EV_RING | EV_RLSD | EV_RXCHAR | EV_RXFLAG | EV_TXEMPTY;
		//	dwStoredFlags = EV_CTS | EV_DSR | EV_RING | EV_RLSD;
		if (!SetCommMask(handle_, dwStoredFlags))
		{
			return;
		}
		osStatus.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
		if (osStatus.hEvent == INVALID_HANDLE_VALUE)
		{
			return;
		}

		Sleep(100);
		while (true)
		{
			WaitCommEvent(handle_, &dwCommEvent, &osStatus);
			dwRes = WaitForSingleObject(osStatus.hEvent, INFINITE);
			switch (dwRes)
			{
				// Event occurred.
				case WAIT_OBJECT_0:
					if (!GetOverlappedResult(handle_, &osStatus, &dwOvRes, FALSE))
					{
						return;
					}
					else
					{
						ProcessEvent(dwCommEvent);
					}
					fWaitingOnStat = false;
					break;
				case WAIT_TIMEOUT:
					//Sleep(0);
					break;
				default:
					CloseHandle(osStatus.hEvent);
					return;
			}
		}
		CloseHandle(osStatus.hEvent);

		//while (true) ;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortComSLIP::Running()");
		throw;
	}
}

void EmPortComSLIP::RxThread()
{
	try
	{
		DWORD dwRead;
		OVERLAPPED osReader = {0};
		BOOL Success;
		BYTE data[1];
		DWORD dwRes;
		DWORD error;
		DWORD timeoutCounter = 0;
		osReader.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
		DWORD modem_status = 0;
		std::string debugStr;

		innerError_ = false;

		while (true)
		{
			if (handle_ == INVALID_HANDLE_VALUE) throw EmException("Device was disconnected!");

			if (terminateRxThread_) break;

			//Sleep(0);
			Success = ReadFile(handle_, data, 1, &dwRead, &osReader);

			if (Success == false)
			{
				error = GetLastError();
				if (error != ERROR_IO_PENDING)
				{
					if (!modemConnectInProcess_ || error == ERROR_INVALID_HANDLE)
					{
						innerError_ = true;
						SetEvent(hEventDisconnected_);
						debugStr = "innerError 7:  ";
						debugStr += EmService::NumberToString(error);
						EmService::WriteToLogFailed(debugStr);
						throw EmException("Device was disconnected!");
					}
				}
				else
				{
					if ((!modemConnectInProcess_) && (devType_ != ETPQP) &&
									(portType_ == COM || portType_ == Modem))
					{
						GetCommModemStatus(handle_, &modem_status);
						if ((modem_status & MS_RLSD_ON) == 0) //not connected
						{
							innerError_ = true;
							SetEvent(hEventDisconnected_);
							EmService::WriteToLogFailed("innerError 2: MS_RLSD_ON not set 1");
							throw EmException("Device was disconnected!");
						}
					}
				}
			}
			else
			{
				if (dwRead == 1)
				{
					timeoutCounter = 0;
					(this->*InputFuncPointer)(data[0]);  //RxFunction(data[0]);
				}
			}

			if (dwRead == 0)
			{
				Sleep(100);
				dwRes = WaitForSingleObject(osReader.hEvent, /*INFINITE*/30000);
				switch (dwRes)
				{
					case WAIT_OBJECT_0:
						if (GetOverlappedResult(handle_, &osReader, &dwRead, FALSE) == FALSE)
						{
							error = GetLastError();
							if (!modemConnectInProcess_ || error == ERROR_INVALID_HANDLE)
							{
								innerError_ = true;
								SetEvent(hEventDisconnected_);
								debugStr = "innerError 3:  ";
								debugStr += EmService::NumberToString(error);
								EmService::WriteToLogFailed(debugStr);
								throw EmException("Device was disconnected!");
							}
						}
						else
						{
							switch (dwRead)
							{
								case 0:
									error = 0;
									break;
								case 1:
									timeoutCounter = 0;
									(this->*InputFuncPointer)(data[0]); //(*input_func)(data);
									break;
								default:
									error = 0;
									break;
							}
						}
						break;
					case WAIT_TIMEOUT:
						GetCommModemStatus(handle_, &modem_status);
						if ((!modemConnectInProcess_) && (devType_ != ETPQP) &&
							(portType_ == COM || portType_ == Modem))
						{
							if ((modem_status & MS_RLSD_ON) == 0) //not connected
							{
								innerError_ = true;
								SetEvent(hEventDisconnected_);
								EmService::WriteToLogFailed("innerError 4: MS_RLSD_ON not set 2");
								throw EmException("Device was disconnected!");
							}
						}
						timeoutCounter++;
						if (timeoutCounter > 10 && !modemConnectInProcess_)
						{
							innerError_ = true;
							SetEvent(hEventDisconnected_);
							EmService::WriteToLogFailed("innerError 5: WAIT_TIMEOUT > 10");
							throw EmException("Device was disconnected!");
						}
						break;
					case WAIT_FAILED:
						error = GetLastError();
						debugStr = "WAIT_FAILED:  ";
						debugStr += EmService::NumberToString(error);
						EmService::WriteToLogFailed(debugStr);
						break;
					default:
						break;
				}
			}
		}  //end of while (true);
	}
	/*catch (ThreadAbortException)
	{
		EmService.WriteToLogFailed("ThreadAbortException in EmPortCom32.RxThread()");
		innerError_ = true;
	}*/
	catch (EmException ex)
	{
		std::string failedStr = "Error in EmPortComSLIP::RxThread() ";
		failedStr += ex.Message;
		EmService::WriteToLogFailed(failedStr);
		innerError_ = true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortComSLIP::RxThread()");
		SetEvent(hEventDisconnected_);
		Sleep(100);
		innerError_ = true;
	}
}

void EmPortComSLIP::ProcessEvent(UINT EventMask)
{
	try
	{
		DWORD modem_status = 0;
		if ((EventMask & EV_BREAK) != 0) { }
		if ((EventMask & EV_CTS) != 0)
		{
			GetCommModemStatus(handle_, &modem_status);
			if ((modem_status & MS_CTS_ON) == 0)
			{
				cts_state_ = 1;
			}
			else
			{
				cts_state_ = 0;
			}

		}
		if ((EventMask & EV_DSR) != 0)
		{
			GetCommModemStatus(handle_, &modem_status);
			if ((modem_status & MS_DSR_ON) == 0)
			{
				dsr_state_ = 1;
			}
			else
			{
				dsr_state_ = 0;
			}
		}
		if ((EventMask & EV_ERR) != 0) { }
		if ((EventMask & EV_RING) != 0) { }
		if ((EventMask & EV_RLSD) != 0)
		{
			GetCommModemStatus(handle_, &modem_status);
			if ((modem_status & MS_RLSD_ON) == 0)
			{
				dcd_state_ = 1;
			}
			else
			{
				dcd_state_ = 0;
			}
		}
		if ((EventMask & EV_RXFLAG) != 0) { }
		if ((EventMask & EV_TXEMPTY) != 0) { }
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortComSLIP::ProcessEvent()");
		throw;
	}
}

void EmPortComSLIP::SetDtrOff()
{
	try
	{
		DCB dcb;
		GetCommState(handle_, &dcb);
		dcb.fDtrControl = DTR_CONTROL_DISABLE;
		SetCommState(handle_, &dcb);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortComSLIP::SetDtrOff()");
		throw;
	}
}

void EmPortComSLIP::SetDtrOn()
{
	try
	{
		DCB dcb;
		GetCommState(handle_, &dcb);
		dcb.fDtrControl = DTR_CONTROL_ENABLE;
		SetCommState(handle_, &dcb);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortComSLIP::SetDtrOn()");
		throw;
	}
}

void EmPortComSLIP::Purge()
{
	try
	{
		PurgeComm(handle_, PURGE_TXABORT);
		PurgeComm(handle_, PURGE_RXABORT);
		PurgeComm(handle_, PURGE_TXCLEAR);
		PurgeComm(handle_, PURGE_RXCLEAR);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in EmPortCom32.Purge()");
		throw;
	}
}

bool EmPortComSLIP::SetTimeOuts(DWORD iTimeOut)
{
	if (handle_ == INVALID_HANDLE_VALUE) return false;
	try
	{
		COMMTIMEOUTS cmt;
		cmt.ReadIntervalTimeout = 0;
		cmt.ReadTotalTimeoutConstant = iTimeOut;
		cmt.ReadTotalTimeoutMultiplier = 0;
		cmt.WriteTotalTimeoutConstant = iTimeOut;
		cmt.WriteTotalTimeoutMultiplier = 0;
		SetCommTimeouts(handle_, &cmt);
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("SetTimeOuts() failed!");
		return false;
	}
}

//}
