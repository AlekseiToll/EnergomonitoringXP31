// DeviceIOEmPortWrap.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "DeviceIOEmPortWrap.h"


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
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif


EmPortWrapperManaged::EmPortWrapperManaged(int devType, int portType, List<unsigned int>^ iparams, 
										   List<String^>^ cparams, int hMainWnd)
{
	std::vector<std::string> vec_cparams;
	int curCount;
	try { curCount = cparams->Count; }
	catch(...) { curCount = 0; }
	for(int iStr = 0; iStr < curCount; ++iStr)
	{
		char* ch = (char*)(void*)Marshal::StringToHGlobalAnsi(cparams[iStr]);
		std::string str = ch;
		vec_cparams.push_back(str);
	}

	std::vector<unsigned int> vec_iparams;
	try { curCount = iparams->Count; }
	catch(...) { curCount = 0; }
	for(int iInt = 0; iInt < curCount; ++iInt)
	{
		vec_iparams.push_back(iparams[iInt]);
	}

	portWrapper_ = new EmPortWrapper(devType, portType, vec_cparams, vec_iparams, hMainWnd);
}

bool EmPortWrapperManaged::Close()
{
	try
	{
		return portWrapper_->Close();
	}
	catch(...)
	{
		WriteToLogFailed("Exception in EmPortWrapperManaged::Close()");
	}
}

bool EmPortWrapperManaged::Open()
{
	return portWrapper_->Open();
}

int EmPortWrapperManaged::Write(DWORD size, List<BYTE>^ listBuffer)
{
	int res = -1;
	BYTE* pBuffer = 0;
	try
	{
		pBuffer = new BYTE[size];
		for(unsigned int i = 0; i < size; ++i)
		{
			pBuffer[i] = listBuffer[i];
		}
	
		res = portWrapper_->Write(size, pBuffer);
	}
	catch(System::Exception^ ex) 
	{
		//WriteToLogGeneral("Exception in EmPortWrapperManaged::Write()");
		//WriteToLogGeneral(ex->Message);
		DumpException(ex, "Exception in EmPortWrapperManaged::Write()");
		res = -1;
		//throw;
	}
	finally
	{
		if(pBuffer != 0) delete[] pBuffer;
	}
	return res;
}

int EmPortWrapperManaged::WriteData(WORD command, List<BYTE>^ listBuffer)
{
	int res = -1;
	BYTE* pBuffer = 0;
	try
	{
		pBuffer = new BYTE[listBuffer->Count];
		for(int iItem = 0; iItem < listBuffer->Count; ++iItem)
			pBuffer[iItem] = listBuffer[iItem];

		res = portWrapper_->WriteData(command, pBuffer, 
			(short)listBuffer->Count);
	}
	catch(System::Exception^ ex) 
	{
		//WriteToLogGeneral("Exception in EmPortWrapperManaged::WriteData()");
		//WriteToLogGeneral(ex->Message);
		DumpException(ex, "Exception in EmPortWrapperManaged::WriteData()");
		res = -1;
		//throw;
	}
	finally 
	{
		if(pBuffer != 0) delete[] pBuffer;
	}
	return res;
}

int EmPortWrapperManaged::ReadData(WORD command, List<BYTE>^ listBuffer, List<System::UInt32>^ listParams)
{
	int res = -1;
	long answerLen = 0;
	BYTE** ppBuffer;
	BYTE* p = 0;
	pin_ptr<BYTE> pinP = p;
	ppBuffer = (BYTE**)&pinP;
	
	long* pParams = 0;
	try
	{
		pParams = new long[listParams->Count];
		for(int iItem = 0; iItem < listParams->Count; ++iItem)
			pParams[iItem] = listParams[iItem];

		res = portWrapper_->ReadData(command, ppBuffer, pParams, listParams->Count, &answerLen);

		if(res == 0)
		{
			BYTE* bufTemp = new BYTE[answerLen];

			memcpy(bufTemp, (*ppBuffer), answerLen);

			listBuffer->Capacity = answerLen;
			for(int i = 0; i < answerLen; ++i)
			{
				listBuffer->Add(bufTemp[i]);
			}
			delete[] bufTemp;
			delete[] (*ppBuffer);
		}
	}
	catch(System::Exception^ ex) 
	{
		//WriteToLogGeneral("Exception in EmPortWrapperManaged::ReadData()");
		//WriteToLogGeneral(ex->Message);
		DumpException(ex, "Exception in EmPortWrapperManaged::ReadData()");
		res = -1;
		//throw;
	}
	finally 
	{
		if(pParams != 0) delete[] pParams;
	}
	return res;
}

int EmPortWrapperManaged::Read(DWORD size, List<BYTE>^ listBuffer)
{
	int res = -1;
	BYTE* pBuffer = new BYTE[size];
	
	try
	{
		res = portWrapper_->Read(size, pBuffer);

		if(res == 0)
		{
			listBuffer->Capacity = size;
			for(unsigned int i = 0; i < size; ++i)
			{
				listBuffer->Add(pBuffer[i]);
			}
		}
	}
	catch(System::Exception^ ex) 
	{
		//WriteToLogGeneral("Exception in EmPortWrapperManaged::Read()");
		//WriteToLogGeneral(ex->Message);
		DumpException(ex, "Exception in EmPortWrapperManaged::Read()");
		res = -1;
		//throw;
	}
	finally
	{
		delete[] pBuffer;
	}
	return res;
}
