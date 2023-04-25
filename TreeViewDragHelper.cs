using System;
using System.Runtime.InteropServices;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Class imports several functions from comctl32.dll library to help draging nodes in TreeView object
	/// </summary>
	public class DragHelper
	{
		[DllImport("comctl32.dll")]
		public static extern bool InitCommonControls();

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern bool ImageList_BeginDrag(IntPtr himlTrack, int
			iTrack, int dxHotspot, int dyHotspot);

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern bool ImageList_DragMove(int x, int y);

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern void ImageList_EndDrag();

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern bool ImageList_DragEnter(IntPtr hwndLock, int x, int y);

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern bool ImageList_DragLeave(IntPtr hwndLock);

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern bool ImageList_DragShowNolock(bool fShow);

		static DragHelper()
		{
			InitCommonControls();
		}
	}
}
