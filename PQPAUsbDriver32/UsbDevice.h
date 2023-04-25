

#include "stdafx.h"
#include <windows.h>

static GUID CYUSBDRV_GUID = {0xAE18A550,0x7F6A,0x11d4,0x97,0xDD,0x00,0x01,0x02,0x29,0xB9,0x5B}; 

#define USB_EPOUT_ADDRESS 0x02
#define USB_EPIN_ADDRESS 0x86

BOOL UsbConnect();
void UsbDisconnect();
BOOL SendData(BYTE *pbData, DWORD dwBytes, DWORD dwTimeout);
int ReadData(unsigned char * pRx, int timeout);