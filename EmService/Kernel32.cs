using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EmServiceLib
{
	public class Kernel32
	{
		public static Guid CYUSBDRV_GUID = new Guid(0xAE18A550, 0x7F6A, 0x11d4, 0x97, 0xDD, 0x00, 0x01, 0x02, 0x29, 0xB9, 0x5B);

		//[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		//public unsafe static extern int memcmp(void* b1, void* b2, UIntPtr count);
		//public static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DEV_BROADCAST_HDR
	{
		public UInt32 dbch_size;
		public UInt32 dbch_devicetype;
		public UInt32 dbch_reserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DEV_BROADCAST_DEVICEINTERFACE
	{
		public UInt32 dbcc_size;
		public UInt32 dbcc_devicetype;
		public UInt32 dbcc_reserved;
		public Guid dbcc_classguid;
		public char dbcc_name;		//char dbcc_name[1];
		public static readonly int Size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE));
	}

	//struct GUID {
	//    unsigned long  Data1;
	//    unsigned short Data2;
	//    unsigned short Data3;
	//    unsigned char  Data4[ 8 ];
	//}
}
