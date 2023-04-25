// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the EMUSB_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// EMUSB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef EMUSB_EXPORTS
#define EMUSB_API __declspec(dllexport)
#else
#define EMUSB_API __declspec(dllimport)
#endif

extern "C" EMUSB_API bool USBCreateInstance(void);
extern "C" EMUSB_API void USBFreeInstance(void);

extern "C" EMUSB_API bool USBLoadDriver(void);
extern "C" EMUSB_API int USBGetNumberDevices(void);
extern "C" EMUSB_API bool USBGetSerialDescription(int devIndex, BYTE* SerialBuffer, BYTE* DescrBuffer);
extern "C" EMUSB_API bool USBSetSerial(BYTE* SerialBuffer);
extern "C" EMUSB_API bool USBOpenDeviceBySerial();
extern "C" EMUSB_API bool USBClose();

extern "C" EMUSB_API int USBRead(BYTE* buffer, int bytesToRead);
extern "C" EMUSB_API int USBWrite(BYTE* buffer, int bytesToWrite);

extern "C" EMUSB_API void dbgInfo(LPCSTR lpcFileName, BYTE* buffer, int length);