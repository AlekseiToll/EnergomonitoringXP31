#ifndef MODEM_WINDOW_H
#define MODEM_WINDOW_H

//#define STRICT
#include <windows.h>
#include "EmServiceClasses.h"

//namespace DeviceIO
//{

BOOL CALLBACK DlgProc(HWND hdlg, UINT msg, WPARAM wParam, LPARAM lParam);

class CModemInfoWindow  
{
	static HMODULE hModule_;

public:
	static HWND hDlg;

    CModemInfoWindow(LPSTR szClassName, LPSTR szWindowTitle);
    
	void AddText(const char* str);
	void CloseWndInfo();
};

class ConnectInfoThread
{ 
	static CModemInfoWindow* wndInfo_;

public: 
	ConnectInfoThread()/*: wndInfo_(0)*/ {}
	~ConnectInfoThread() 
	{
		if(wndInfo_ != 0) delete wndInfo_;
	}

	static void ThreadEntryStatic(ConnectInfoThread* pObj)
	{
		pObj->ThreadEntry();
	}

	void ThreadEntry()
	{
		wndInfo_ = new CModemInfoWindow("ClassName", "WindowTitle");
		EmService::WriteToLogGeneral("ConnectInfoThread::ThreadEntry exit");
	}

	void AddText(const char* str)
	{
		EmService::WriteToLogGeneral(str);
		wndInfo_->AddText(str);
	}

	void HideWnd()
	{
		wndInfo_->CloseWndInfo();
	}
};

//}

#endif
