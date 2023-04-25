#include "stdafx.h"
#include "resource.h"
#include "ModemInfoWnd.h"

//namespace DeviceIO
//{

HWND CModemInfoWindow::hDlg = (HWND)-1;
HMODULE CModemInfoWindow::hModule_ = (HMODULE)NULL;

CModemInfoWindow* ConnectInfoThread::wndInfo_ = 0;

CModemInfoWindow::CModemInfoWindow(LPSTR szClassName, LPSTR szWindowTitle)
{
	hModule_ = GetModuleHandle("DeviceIOEmPortCpp.dll");
	if(hModule_ == NULL) EmService::WriteToLogFailed("CModemInfoWindow constructor: hModule = NULL");

	INT_PTR res = DialogBox(hModule_, MAKEINTRESOURCE(DLG_MODEM_INFO), 
                         NULL, (DLGPROC)DlgProc);
}

BOOL CALLBACK DlgProc(HWND hdlg, UINT msg, WPARAM wParam, LPARAM lParam) 
{
	CModemInfoWindow::hDlg = hdlg;
	switch(msg) 
	{
		case WM_INITDIALOG: 
			return TRUE;
		case DLG_ADD_TEXT:
			SendDlgItemMessage(hdlg, LB_MODEM_INFO, LB_ADDSTRING, 0, lParam);
			return TRUE;
		case WM_CLOSE:
			EndDialog(hdlg, 0);
			return TRUE;
	}
	return FALSE;
}

void CModemInfoWindow::AddText(const char* str)
{
	if(CModemInfoWindow::hDlg != (HWND)-1 && CModemInfoWindow::hDlg != NULL)
		PostMessage(CModemInfoWindow::hDlg, DLG_ADD_TEXT, 0, (LPARAM)str);
}

void CModemInfoWindow::CloseWndInfo()
{
	if(CModemInfoWindow::hDlg != (HWND)-1 && CModemInfoWindow::hDlg != NULL)
		PostMessage(CModemInfoWindow::hDlg, WM_CLOSE, 0, 0);
}

//}
