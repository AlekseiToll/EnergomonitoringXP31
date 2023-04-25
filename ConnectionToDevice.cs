using System;
using System.Collections.Generic;
using System.Text;

namespace EnergomonitoringXP
{
	class CConnectionToDevice
	{
		///<summary>Delegate of event OnCloseDevice</summary>
		public delegate void CloseDeviceHandler();
		/// <summary>Event OnCloseDevice occures when thread must close device</summary>
		public event CloseDeviceHandler OnCloseDevice;
	}
}
