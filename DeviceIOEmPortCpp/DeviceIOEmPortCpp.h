#ifndef DEVICEIOEMPORTCPP_H
#define DEVICEIOEMPORTCPP_H

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DEVICEIOEMPORTCPP_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DEVICEIOEMPORTCPP_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DEVICEIOEMPORTCPP_EXPORTS
#define DEVICEIOEMPORTCPP_API __declspec(dllexport)
#else
#define DEVICEIOEMPORTCPP_API __declspec(dllimport)
#endif

#include <string>
#include <vector>

#include "EmPort.h"

//using namespace DeviceIO;
using namespace std;

class DEVICEIOEMPORTCPP_API EmPortWrapper
{
	EmPortType portType_;
	EmDeviceType devType_;
	int hMainWnd_;

	EmPort* port_;

	bool isOpened_;

public:
	EmPortWrapper(int iDevType, int iPortType, vector<string> vec_cparams, vector<unsigned int> vec_iparams, 
		int hMainWnd);
	~EmPortWrapper();

	bool Open();
	bool Close();

	int Write(DWORD size, BYTE* buffer);
	int Read(DWORD size, BYTE* buffer);

	int ReadData(WORD command, BYTE** buffer, long* arr_params, long params_count,
		long* pAnswerLen);
	int WriteData(WORD command, BYTE* buffer, short bufLength);
};

#endif

