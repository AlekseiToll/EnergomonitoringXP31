#pragma once
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DEVICEIOEMPORTWRAP_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DEVICEIOEMPORTWRAP_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DEVICEIOEMPORTWRAP_EXPORTS
#define DEVICEIOEMPORTWRAP_API __declspec(dllexport)
#else
#define DEVICEIOEMPORTWRAP_API __declspec(dllimport)
#endif

#define WORD unsigned short
#define DWORD unsigned long
#define BYTE unsigned char
//#include <windows.h>
#include <string>
#include <vector>
#include "../DeviceIOEmPortCpp/DeviceIOEmPortCpp.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics;
using namespace System::Collections::Generic;


public ref class /*DEVICEIOEMPORTWRAP_API*/ EmPortWrapperManaged
{
private:
	EmPortWrapper* portWrapper_;

public:
	EmPortWrapperManaged(int devType, int portType, List<unsigned int>^ iparams, List<String^>^ cparams, int hMainWnd);

	~EmPortWrapperManaged()
	{
		if(portWrapper_ != 0) delete portWrapper_;
	}

	bool Close();

	bool Open();

	int Write(DWORD size, List<BYTE>^ listBuffer);

	int WriteData(WORD command, List<BYTE>^ listBuffer);

	int ReadData(WORD command, List<BYTE>^ listBuffer, List<System::UInt32>^ listParams);

	int Read(DWORD size, List<BYTE>^ listBuffer);


	static void WriteToLogGeneral(String^ s)
	{
		System::IO::StreamWriter^ sw;

		try
		{
			sw = gcnew System::IO::StreamWriter("LogGeneralCppWrap.txt", true);
			sw->WriteLine(s);
		}
		catch (...) 
		{ 
			//Debug::WriteLine("Error in EmPortWrap::WriteToLogGeneral()");
			//throw;
		}
		finally
		{
			sw->Close();
		}
	}

	static void WriteToLogFailed(String^ s)
	{
		System::IO::StreamWriter^ sw;

		try
		{
			sw = gcnew System::IO::StreamWriter("LogFailedCppWrap.txt", true);
			sw->WriteLine(s);
		}
		catch (...) 
		{ 
			//Debug::WriteLine("Error in EmPortWrap::WriteToLogGeneral()");
			//throw;
		}
		finally
		{
			sw->Close();
		}
	}

	static void DumpException(Exception^ ex, String^ info)
	{
		System::IO::StreamWriter^ sw;

		try
		{
			sw = gcnew System::IO::StreamWriter("LogFailedCppWrap.txt", true);
			sw->WriteLine(info);

			//Debug::WriteLine("--------- Outer Exception Data ---------");
			sw->WriteLine("========= Exception Dump ===============");
			sw->WriteLine("--------- Outer Exception Data ---------");
			WriteExceptionInfo(ex, sw);

			ex = ex->InnerException;
			try {
				while (ex)
				{
					//Debug::WriteLine("--------- Inner Exception Data ---------");
					sw->WriteLine("--------- Inner Exception Data ---------");
					WriteExceptionInfo(ex, sw);
					ex = ex->InnerException;
				}
			}
			catch(...) {}
			sw->WriteLine("========= end of Exception Dump ========");
		}
		catch (System::Exception^)
		{
			//Debug::WriteLine("Error in DumpException() " + nex->Message);
			//throw nex;
		}
		finally
		{
			sw->Close();
		}
	}

	static void WriteExceptionInfo(Exception^ ex, System::IO::StreamWriter^ sw)
	{
		//Debug::WriteLine("\nMessage: {0}", ex->Message);
		sw->WriteLine("\nMessage: {0}", ex->Message);
		//Debug::WriteLine("\nException Type: {0}", ex->GetType()->FullName);
		sw->WriteLine("\nException Type: {0}", ex->GetType()->FullName);
		//Debug::WriteLine("\nSource: {0}", ex->Source);
		sw->WriteLine("\nSource: {0}", ex->Source);
		//Debug::WriteLine("\nStrackTrace: {0}", ex->StackTrace);
		sw->WriteLine("\nStrackTrace: {0}", ex->StackTrace);
		//Debug::WriteLine("\nTargetSite: {0}", ex->TargetSite->ToString());
		sw->WriteLine("\nTargetSite: {0}", ex->TargetSite);
	}
};
