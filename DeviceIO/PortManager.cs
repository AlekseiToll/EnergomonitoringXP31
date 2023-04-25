using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

//using NativeWifi;
using DeviceIOEmPortCSharp;
using EmServiceLib;

namespace DeviceIO
{
	public class PortManager
	{
		private EmPortWrapperManaged port_;
		private EmPort portA_;

		private IntPtr hMainWnd_;
		private EmPortType portType_;
		private EmDeviceType devType_;
		private bool bAutoMode_;

		private string wifiProfileName_;
		private string wifiPassword_;

		private object[] portParams_;

		public PortManager(IntPtr hMainWnd, EmPortType portType, EmDeviceType devType, 
			ref object[] portParams, bool bAutoMode)
		{
			hMainWnd_ = hMainWnd;
			portType_ = portType;
			devType_ = devType;
			portParams_ = portParams;
			bAutoMode_ = bAutoMode;
		}

		public PortManager(IntPtr hMainWnd, EmPortType portType, EmDeviceType devType,
			string wifiProfileName, string wifiPassword,
			ref object[] portParams, bool bAutoMode)
		{
			hMainWnd_ = hMainWnd;
			portType_ = portType;
			devType_ = devType;
			portParams_ = portParams;
			bAutoMode_ = bAutoMode;
			wifiProfileName_ = wifiProfileName;
			wifiPassword_ = wifiPassword;
		}

		/// <summary>
		/// NOT for EtPQP-A  ?????????????????? merge
		/// </summary>
		public ExchangeResult WriteData(EmCommands command, ref List<byte> buffer)
		{
			if (IsMainPortUsed)
			{
				int res = port_.WriteData((ushort)command, buffer);
				if (res == 0) return ExchangeResult.OK;
				if (res == -2) return ExchangeResult.Disconnect_Error;
				if (res == -3) return ExchangeResult.Write_Error;
				return ExchangeResult.Write_Error;
			}
			else
			{
				return ExchangeResult.Other_Error;
			}
		}

		/// <summary>
		/// for EtPQP-A ONLY  ?????????????????? merge
		/// </summary>
		public ExchangeResult WriteData(EmCommands command, ref byte[] buffer)
		{
			if (IsMainPortUsed)
			{
				List<byte> tmpBuf = new List<byte>(buffer);
				ExchangeResult res = WriteData(command, ref tmpBuf);
				return res;
			}
			else
			{
				return ((EtPqpAUSB)portA_).WriteData((ushort)command, ref buffer);
			}
		}

		/// <summary>
		/// for EtPQP-A ONLY  ?????????????????? merge
		/// </summary>
		public ExchangeResult ReadData(EmCommands command, ref byte[] buffer, List<UInt32> listParams)
		{
			if (IsMainPortUsed)
			{
				List<byte> tmpBuf = new List<byte>();
				ExchangeResult res = ReadData(command, ref tmpBuf, listParams);
				if (tmpBuf != null) buffer = tmpBuf.ToArray();
				return res;
			}
			else
			{
				return ((EtPqpAUSB)portA_).ReadData((ushort)command, ref buffer, listParams);
			}
		}

		/// <summary>
		/// NOT for EtPQP-A  ?????????????????? merge
		/// </summary>
		public ExchangeResult ReadData(EmCommands command, ref List<byte> buffer, List<UInt32> listParams)
		{
			if (IsMainPortUsed)
			{
				int res = port_.ReadData((ushort) command, buffer, listParams);
				if (res == 0) return ExchangeResult.OK;
				if (res == -2) return ExchangeResult.Disconnect_Error;
				if (res == -3) return ExchangeResult.Write_Error;
				return ExchangeResult.Read_Error;
			}
			else
			{
				return ExchangeResult.Other_Error;
			}
		}

		/// <summary>
		/// NOT for EtPQP-A
		/// </summary>
		public ExchangeResult WriteToPort(UInt32 size, ref byte[] buffer)
		{
			try
			{
				if (!IsMainPortUsed) return ExchangeResult.Other_Error;

				List<byte> listBuffer = new List<byte>(buffer.Length);
				for (int i = 0; i < buffer.Length; ++i)
					listBuffer.Add(buffer[i]);

				int res = port_.Write(size, listBuffer);
				if (res == 0) return ExchangeResult.OK;
				else return ExchangeResult.Write_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in WriteToPort:");
				return ExchangeResult.Write_Error;
			}
		}

		/// <summary>
		/// NOT for EtPQP-A
		/// </summary>
		public ExchangeResult ReadFromPort(UInt32 size, ref byte[] buffer)
		{
			try
			{
				if(!IsMainPortUsed) return ExchangeResult.Other_Error;

				int res = 0;

				for (int attempt = 0; attempt < 10; ++attempt)
				{
					List<byte> listBuffer = new List<byte>();

					res = port_.Read(size, listBuffer);
					if (res == 0)
					{
						buffer = new byte[listBuffer.Count];
						listBuffer.CopyTo(buffer);

						break;
					}
					Thread.Sleep(200);
				}

				if (res != 0) return ExchangeResult.Read_Error;
				return ExchangeResult.OK;
			}
			catch (ThreadAbortException tex)
			{
				EmService.DumpException(tex, "ThreadAbortException in ReadFromPort:");
				Thread.ResetAbort();
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadFromPort:");
				return ExchangeResult.Other_Error;
			}
		}

		//public static bool ConnectToWifi()
		//{
		//    try
		//    {
		//        if (!Wlan.IsWifiConnected(false, wifiProfileName_))
		//        {
		//            WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(wifiProfileName_,
		//                                                                        wifiPassword_);

		//            if (!Wlan.IsWifiConnected(true, wlanIface, wifiProfileName_))
		//            {
		//                EmService.WriteToLogFailed("Wi-fi not connected!");
		//                return false;
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Exception in ConnectToWifi() WI-FI:");
		//        return false;
		//    }
		//}

		public bool CreatePort()
		{
			try
			{
				ushort devAddress = 0xFFFF;
				List<string> cparams = null;

				switch (portType_)
				{
					case EmPortType.COM:
						string name = string.Empty;
						uint speed = 0;
						try
						{
							name = (string)portParams_[0];
							speed = (uint)portParams_[1];
						}
						catch
						{
							return false;
						}
						if (name == string.Empty || speed == 0) return false;

						cparams = new List<string>(1);
						cparams.Add(name);
						List<uint> iparams = new List<uint>(1);
						iparams.Add(speed);
						port_ = new EmPortWrapperManaged((int)devType_, (int)portType_,
														 iparams, cparams, (int)hMainWnd_);
						break;

					case EmPortType.USB:
						if (devType_ != EmDeviceType.ETPQP_A)
						{
							port_ = new EmPortWrapperManaged((int) devType_, (int) portType_,
							                                 new List<uint>(0), new List<String>(0),
							                                 (int) hMainWnd_);
						}
						else
						{
							portA_ = new EtPqpAUSB(devType_, 0xFFFF, hMainWnd_);
						}
						break;

					case EmPortType.Ethernet:
						string address;
						uint port;
						try
						{
							address = (string)portParams_[0];
							port = (uint)(int)portParams_[1];
						}
						catch
						{
							return false;
						}
						if (address == string.Empty || port == 0) return false;
						System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse(address);
						byte[] arrayAddr = ipAddress.GetAddressBytes();

						iparams = new List<uint>(6);
						for (int i = 0; i < 4; ++i)
							iparams.Add((uint)arrayAddr[i]);
						iparams.Add(port);
						//iparams.Add(bAutoMode_ ? 1 : 0);
						port_ = new EmPortWrapperManaged((int)devType_, (int)portType_, iparams, cparams,
														 (int)hMainWnd_);
						break;

					case EmPortType.WI_FI:
						address = string.Empty;
						port = 0;
						try
						{
							address = (string)portParams_[0];
							port = (uint)(int)portParams_[1];
						}
						catch
						{
							return false;
						}
						if (address == string.Empty || port == 0) return false;
						ipAddress = System.Net.IPAddress.Parse(address);
						arrayAddr = ipAddress.GetAddressBytes();

						iparams = new List<uint>(6);
						for (int i = 0; i < 4; ++i)
							iparams.Add((uint)arrayAddr[i]);
						// add 1 to the last digit (specially for wi-fi)
						iparams[3] += 1;

						iparams.Add(port);
						//iparams.Add(bAutoMode_ ? 1 : 0);
						port_ = new EmPortWrapperManaged((int)devType_, (int)portType_, iparams, cparams,
														 (int)hMainWnd_);
						break;

					case EmPortType.Modem:
						string nameModem = string.Empty;
						uint speedModem = 0;
						string phone = string.Empty;
						uint attempts = 0;
						try
						{
							nameModem = (string)portParams_[0];
							speedModem = (uint)portParams_[1];
							phone = (string)portParams_[2];
							attempts = (uint)(int)portParams_[3];
							devAddress = (ushort)portParams_[4];
						}
						catch
						{
							return false;
						}
						if (nameModem == string.Empty || speedModem == 0 || phone == string.Empty)
							return false;

						cparams = new List<string>(2);
						cparams.Add(nameModem);
						cparams.Add(phone);
						iparams = new List<uint>(4);
						iparams.Add(speedModem);
						iparams.Add(attempts);
						iparams.Add((uint)(bAutoMode_ ? 1 : 0));
						iparams.Add(devAddress);
						port_ = new EmPortWrapperManaged((int)devType_, (int)portType_, iparams, cparams,
														 (int)hMainWnd_);
						break;

					case EmPortType.GPRS:
						string addressGPRS = string.Empty;
						uint portGPRS = 0;
						try
						{
							addressGPRS = (string)portParams_[0];
							portGPRS = (uint)(int)portParams_[1];
							devAddress = (ushort)portParams_[2];
						}
						catch
						{
							return false;
						}
						if (addressGPRS == string.Empty || portGPRS == 0) return false;
						System.Net.IPAddress ipAddressGPRS = System.Net.IPAddress.Parse(addressGPRS);
						byte[] arrayAddrGPRS = ipAddressGPRS.GetAddressBytes();

						iparams = new List<uint>(7);
						for (int i = 0; i < 4; ++i)
							iparams.Add((uint)arrayAddrGPRS[i]);
						iparams.Add(portGPRS);
						//iparams.Add(bAutoMode_ ? 1 : 0);
						iparams.Add(devAddress);
						port_ = new EmPortWrapperManaged((int)devType_, (int)portType_, iparams, cparams,
														 (int)hMainWnd_);
						break;

					case EmPortType.Rs485:
						string name485 = string.Empty;
						uint speed485 = 0;
						try
						{
							name485 = (string)portParams_[0];
							speed485 = (uint)portParams_[1];
							devAddress = (ushort)portParams_[2];
						}
						catch
						{
							return false;
						}
						if (name485 == string.Empty || speed485 == 0) return false;

						cparams = new List<string>(1);
						cparams.Add(name485);
						iparams = new List<uint>(3);
						iparams.Add(speed485);
						iparams.Add(devAddress);
						//iparams.Add(bAutoMode_ ? 1 : 0);
						port_ = new EmPortWrapperManaged((int)devType_, (int)portType_, iparams, cparams,
														 (int)hMainWnd_);
						break;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in PortManager::CreatePort():");
				throw;
			}
		}

		public bool OpenPort()
		{
			bool res = false;
			if (IsMainPortUsed)
			{
				res = port_.Open();
			}
			else
			{
				res = portA_.Open();
			}
			if (res) Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 2, 0, 0);
			else Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			return res;
		}

		/// <summary>
		/// For EtPQP-A and USB only
		/// </summary>
		public bool OpenFast(bool bNeedClose)
		{
			//if (IsMainPortUsed) return false;
			try
			{
				if (bNeedClose)
				{
					ClosePort(false);
					Thread.Sleep(2000);
				}

				if (!CreatePort()) return false;

				if (IsMainPortUsed)
				{
					if (!port_.Open()) return false;
				}
				else
				{
					if (!portA_.Open()) return false;
				}

				Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 2, 0, 0);
				return true;
			}
			catch (EmDisconnectException)
			{
				ClosePort(true);
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in OpenFast EtPQP-A device:");
				throw;
			}
		}

		public bool ClosePort(bool bNeedDispose)
		{
			Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			if(IsMainPortUsed) { if (port_ == null) return false; }
			else { if (portA_ == null) return false; }

			try
			{
				if (IsMainPortUsed)
				{
					if (!port_.Close()) return false;
				}
				else
				{
					if (!portA_.Close()) return false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ClosePort() 1:");
				//throw;
			}

			if (!bNeedDispose) return true;

			try
			{
				if (IsMainPortUsed)
				{
					if (port_ != null) port_.Dispose();
					port_ = null;
				}
				else
				{
					//if (portA_ != null) portA_.Dispose();
					portA_ = null;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ClosePort() 2:");
				if (IsMainPortUsed) port_ = null;
				else portA_ = null;
				throw;
			}
		}

		private bool IsMainPortUsed
		{
			get 
			{ 
				return !(devType_ == EmDeviceType.ETPQP_A && portType_ == EmPortType.USB); 
			}
		}
	}
}
