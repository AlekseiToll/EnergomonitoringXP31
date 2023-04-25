#ifndef EMSERVICECLASSES_H
#define EMSERVICECLASSES_H

#include <vector>
#include <sstream>
#include <list>
#include <sys/timeb.h>
#include <time.h>
#include <fstream>
#include <iostream>
#include <algorithm>

namespace DeviceIO
{

class DateTime
{
	WORD year_;
	WORD month_;
	WORD day_;
	WORD hour_;
	WORD minutes_;
	WORD seconds_;
	WORD milliseconds_;

	friend bool operator ==(const DateTime& d1, const DateTime& d2);

public:
	DateTime(WORD y, BYTE mo, BYTE d, BYTE h, BYTE min, BYTE sec, WORD millisec) :
	  year_(y), month_(mo), day_(d), hour_(h), minutes_(min), seconds_(sec), milliseconds_(millisec)
	  {}

	DateTime(WORD y, BYTE mo, BYTE d, BYTE h, BYTE min, BYTE sec) :
	  year_(y), month_(mo), day_(d), hour_(h), minutes_(min), seconds_(sec), milliseconds_(0)
	  {}

	DateTime(BYTE h, BYTE min, BYTE sec) :
	  hour_(h), minutes_(min), seconds_(sec), milliseconds_(0)
	{
		time_t curTimer;
		time(&curTimer);
		tm* curTime = localtime(&curTimer);
		year_ = curTime->tm_year + 1900;
		month_ = curTime->tm_mon;
		day_ = curTime->tm_mday;
	}

	static DateTime Now()
	{
		time_t curTimer;
		time(&curTimer);
		tm* curTime = localtime(&curTimer);
		return DateTime(curTime->tm_year + 1900, curTime->tm_mon, curTime->tm_mday, curTime->tm_hour,
			curTime->tm_min, curTime->tm_sec);
	}

	std::string ToString()
	{
		std::stringstream ss;
		ss << day_ << "." << month_ << "." << year_ << " " << hour_ << ":" << minutes_ << ":" << seconds_;
		return ss.str();
	}
};

bool operator ==(const DateTime& d1, const DateTime& d2);

template<class T> class List
{
	std::list<T> list_;

public:
	bool Contains(const T& item)
	{
		std::list<T>::iterator res = std::find(list_.begin(), list_.end(), item);
		if(res == list_.end()) return false;
		return true;
	}

	void push_back(const T& value)
	{
		list_.push_back(value);
	}
};

class EmService 
{
	//static std::string logFailedName;
	//static std::string logGeneralName;

public:
	//static CRITICAL_SECTION scLogFailed;
	//static CRITICAL_SECTION scLogGeneral;

	static std::string GetLogFailedName()
	{
		TCHAR szPath[MAX_PATH];
		DWORD len = GetModuleFileName(NULL, szPath, MAX_PATH);
		if(len == 0)
		{
			return "LogFailedCpp.txt";
		}
		std::string name = szPath;
		name = name.substr(0, name.find_last_of("\\"));
		name += "\\LogFailedCpp.txt";
	}

	static std::string GetLogGeneralName()
	{
		TCHAR szPath[MAX_PATH];
		DWORD len = GetModuleFileName(NULL, szPath, MAX_PATH);
		if(len == 0)
		{
			return "LogGeneralCpp.txt";
		}
		std::string name = szPath;
		name = name.substr(0, name.find_last_of("\\"));
		name += "\\LogGeneralCpp.txt";
	}

	static void WriteToLogFailed(const std::string& s)
	{
		//EnterCriticalSection(&scLogFailed);
		std::ofstream ofs(GetLogFailedName().c_str(), std::ios_base::out | std::ios_base::app);
		ofs << s << '\n';
		ofs.close();
		//LeaveCriticalSection(&scLogFailed);
	}

	static void WriteToLogGeneral(const std::string& s)
	{
		//EnterCriticalSection(&scLogGeneral);
		std::ofstream ofs(GetLogGeneralName().c_str(), std::ios_base::out | std::ios_base::app);
		ofs << s << '\n';
		ofs.close();
		//LeaveCriticalSection(&scLogGeneral);
	}

	static std::string GetEm32CommandText(WORD command)
	{
		switch (command)
		{
			case 0x1000: return "COMMAND_OK";
			case 0x1003: return "COMMAND_BAD_DATA";
			case 0x1004: return "COMMAND_BAD_PASSWORD";
			case 0x1005: return "COMMAND_ACCESS_ERROR";
			case 0x1006: return "COMMAND_CHECK_FAILED";
			case 0x1007: return "COMMAND_NO_DATA";
			case 0x1001: return "COMMAND_UNKNOWN_COMMAND";
			case 0x1002: return "COMMAND_CRC_ERROR";
			case 0x0000: return "COMMAND_ECHO";
			case 0x0001: return "COMMAND_ReadActualValues";
			case 0x0002: return "COMMAND_ReadCalibration";
			case 0x0003: return "COMMAND_WriteCalibration";
			case 0x0009: return "COMMAND_ReadQualityDates";
			case 0x000A: return "COMMAND_ReadQualityEntry";
			case 0x0007: return "COMMAND_ReadSets";
			case 0x0008: return "COMMAND_WriteSets";
			case 0x000B: return "COMMAND_ReadSystemData";
			case 0x000C: return "COMMAND_WriteSystemData";
			case 0x000D: return "COMMAND_Read3secValues";
			case 0x000E: return "COMMAND_Read1minValues";
			case 0x000F: return "COMMAND_Read30minValues";
			case 0x0012: return "COMMAND_ReadEventLogger";
			case 0x0019: return "COMMAND_ReadDipSwellArchive";
			case 0x0010: return "COMMAND_ReadDipSwellStatus";
			case 0x001A: return "COMMAND_ReadDipSwellIndexByStartTimestamp";
			case 0x001B: return "COMMAND_ReadDipSwellIndexByEndTimestamp";
			case 0x001C: return "COMMAND_ReadEarliestAndLatestDipSwellTimestamp";
			case 0x0013: return "COMMAND_Read3secArchiveByTimestamp";
			case 0x0014: return "COMMAND_Read1minArchiveByTimestamp";
			case 0x0015: return "COMMAND_Read30minArchiveByTimestamp";
			case 0x001E: return "COMMAND_ReadEarliestAndLatestAverageTimestamp";
			case 0x001F: return "COMMAND_ReadQualityDatesByObject";
			case 0x0006: return "COMMAND_AverageArchiveQuery";
			case 0x0026: return "COMMAND_Read3secArchiveByTimestampObjectDemand";
			case 0x0027: return "COMMAND_Read1minArchiveByTimestampObjectDemand";
			case 0x0028: return "COMMAND_Read30minArchiveByTimestampObjectDemand";
			case 0x0024: return "COMMAND_ReadObjectsEntrys";
			case 0x0025: return "COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand";
			case 0x0021: return "COMMAND_ReadDipSwellArchiveByObject";
			case 0x0022: return "COMMAND_ReadDipSwellIndexByStartTimestampByObject";
			case 0x0023: return "COMMAND_ReadDipSwellIndexByEndTimestampByObject";
			case 0x0020: return "COMMAND_ReadQualityEntryObjectDemand";
			default: return EmService::NumberToString(command);
		}
	}

	static std::string GetCurrentDateTime()
	{
		/*char tmpbuf[128];
		std::string res;

		// Set time zone from TZ environment variable. If TZ is not set,
		// the operating system is queried to obtain the default value 
		// for the variable. 
		_tzset();

		// Display operating system-style date and time. 
		_strtime_s( tmpbuf, 128 );
		res = std::string(tmpbuf);
		_strdate_s( tmpbuf, 128 );
		res += std::string("  ");
		res += std::string(tmpbuf);
		return res;*/

		return DateTime::Now().ToString();
	}

	static std::string NumberToString(long value)
	{
		/*char buffer[1024];
		_itoa_s(value, buffer, 10); 
		std::string res = std::string(buffer);
		return res;*/
		std::stringstream ss;
		ss << value;
		return ss.str();
	}

	static long StringToNumber(const std::string& str)
	{
		return atol(str.c_str());
	}

	static std::string StdStringToLower(const std::string& str)
	{
		std::string res;
		std::string::const_iterator it = str.begin();
		for(;  it != str.end(); ++it) 
		{
			res += tolower(*it);
		}
		return res;
	}

	static std::string RemoveSymbolFromString(const std::string& str, int symbolCode)
	{
		return RemoveSymbolFromString(str, (char)symbolCode);
	}

	static std::string RemoveSymbolFromString(const std::string& str, char c)
	{
		std::string res = str;
		std::string::iterator b = res.begin(), e = res.end(), r;
		r = remove(b, e, c);
		if(r != e) res.erase(r, res.end());
		return res;
	}
};

class EmException
{
public:
	std::string Message;
	EmException(std::string mess): Message(mess) {}
};

}

#endif