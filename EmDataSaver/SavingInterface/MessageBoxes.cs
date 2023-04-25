using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Resources;

using EmServiceLib;
using EmDataSaver;
using DeviceIO;

namespace EmDataSaver.SavingInterface
{
	public class MessageBoxes
	{
		public static DialogResult MsgArchiveMoreThanLimit(object wndOwner, object classOwner, int limit)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string msg = string.Format(rm.GetString("archive_more_than_limit"), limit);
			string cap = rm.GetString("information_caption");
			return MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.YesNo, 
				MessageBoxIcon.Information);
		}

		public static void NotCorrectTime(object wndOwner, object classOwner)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string msg = rm.GetString("msg_time_not_correct");
			string cap = rm.GetString("information_caption");
			MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static void NoDnsEvents(object wndOwner, object classOwner)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string msg = rm.GetString("msg_dns_empty_text");
			string cap = rm.GetString("information_caption");
			MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>Shows device input data error message box</summary>
		public static void UnableToReadNewAvgIndex(object wndOwner, object classOwner)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
				string msg = rm.GetString("msg_not_read_avg_index");
				string cap = rm.GetString("unfortunately_caption");
				MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in UnableToReadNewAvgIndex:");
				throw;
			}
		}

		/// <summary>Shows device input data error message box</summary>
		public static void DeviceIsNotLicenced(object wndOwner, object classOwner)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
				string msg = rm.GetString("msg_device_licence_failed_text");
				string cap = rm.GetString("msg_device_licence_failed_caption");
				MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceIsNotLicenced:");
				throw;
			}
		}

		/// <summary>Shows device connection error message box</summary>
		public static void ReadDevInfoError(object wndOwner, object classOwner,
					EmPortType portInterface, object[] portSettings)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string msg = rm.GetString("msg_device_devinfo_error_text");
			string cap = rm.GetString("msg_device_connect_error_caption");

			switch (portInterface)
			{
				case EmPortType.COM:
					msg = string.Format(msg, portInterface, (string)portSettings[0] + ", ",
										(int)portSettings[1]);
					break;
				case EmPortType.USB:
					msg = string.Format(msg, portInterface, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					break;
				case EmPortType.Modem:
					msg = string.Format(msg, portInterface, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					break;
				case EmPortType.Ethernet:
					msg = string.Format(msg, portInterface, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					break;
				case EmPortType.GPRS:
					msg = string.Format(msg, portInterface, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					break;
				case EmPortType.Rs485:
					msg = string.Format(msg, portInterface, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					break;
				case EmPortType.WI_FI:
					msg = string.Format(msg, portInterface, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					break;
				default:
					throw (new EmException("Error 0x02432"));
			}

			MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device read error message box</summary>
		public static void DeviceHasNoData(object wndOwner, object classOwner)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string msg = rm.GetString("msg_device_empty_text");
			string cap = rm.GetString("information_caption");
			MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public static void DbConnectError(object wndOwner, object classOwner, 
											string host, int port, string database)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string strMsg = String.Format(
				rm.GetString("msg_db_connect_error_text"), database, host, port);
			string strCap = rm.GetString("unfortunately_caption");
			MessageBox.Show(wndOwner as Form, strMsg, strCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device connection error message box</summary>
		public static void InvalidInterface(object wndOwner, object classOwner,
					EmPortType portInterface, string curDevice)
		{
			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_interface_text");
			string cap = rm.GetString("msg_device_connect_error_caption");

			switch (portInterface)
			{
				case EmPortType.COM:
					msg = string.Format(msg, curDevice, "COM");
					break;
				case EmPortType.USB:
					msg = string.Format(msg, curDevice, "USB");
					break;
				case EmPortType.Modem:
					msg = string.Format(msg, curDevice, "GSM Modem");
					break;
				case EmPortType.Ethernet:
					msg = string.Format(msg, curDevice, "Ethernet");
					break;
				case EmPortType.GPRS:
					msg = string.Format(msg, curDevice, "GPRS");
					break;
				case EmPortType.Rs485:
					msg = string.Format(msg, curDevice, "RS-485");
					break;
				case EmPortType.WI_FI:
					msg = string.Format(msg, curDevice, "Wi-Fi");
					break;
				default:
					throw (new EmException("Error 0x02432"));
			}

			MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device connection error message box</summary>
		public static void DeviceConnectionError(object wndOwner, object classOwner,
					EmPortType portInterface, object[] portSettings)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
				string msg = rm.GetString("msg_device_connect_error_text");
				string cap = rm.GetString("msg_device_connect_error_caption");

				//??????????? тут бывает Excepion: Specified cast is not valid.

				switch (portInterface)
				{
					case EmPortType.COM:
						msg = string.Format(msg, portInterface, (string)portSettings[0] + ", ",
											(int)portSettings[1]);
						break;
					case EmPortType.USB:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.Modem:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.Ethernet:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.GPRS:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.Rs485:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.WI_FI:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					default:
						throw (new EmException("Error 0x02432"));
				}

				MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceConnectionError:");
				throw;
			}
		}

		public static void DeviceOldVersion(object wndOwner, object classOwner)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
				string msg = rm.GetString("etpqp_version_is_old");
				string cap = rm.GetString("unfortunately_caption");
				MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceOldVersion:");
				throw;
			}
		}

		public static void CRCerrorPages(object wndOwner, object classOwner, List<ushort> pages)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
				string msg = rm.GetString("msg_crc_error_pages");
				string str_pages = string.Empty;
				for (int iPage = 0; iPage < pages.Count; ++iPage)
					str_pages += pages[iPage].ToString() + ", ";
				str_pages = str_pages.Remove(str_pages.Length - 2);
				msg = string.Format(msg, str_pages);

				string cap = rm.GetString("information_caption");
				MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceOldVersion:");
				throw;
			}
		}

		public static void CRCerrorPages(object wndOwner, object classOwner, ushort page)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", classOwner.GetType().Assembly);
				string msg = rm.GetString("msg_crc_error_pages");
				msg = string.Format(msg, page);

				string cap = rm.GetString("information_caption");
				MessageBox.Show(wndOwner as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceOldVersion:");
				throw;
			}
		}
	}
}
