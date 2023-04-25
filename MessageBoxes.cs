// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Localized message boxes

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Resources;

using EmServiceLib;
using DeviceIO;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Static classes to show language dependent message boxes
	/// </summary>
	public class MessageBoxes
	{
		public static void MsgParamWasAlreadyAdded(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				sender.GetType().Assembly);
			string msg = rm.GetString("msg_param_was_added");
			string cap = rm.GetString("warning_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static void MsgErrorGetVolValues(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				sender.GetType().Assembly);
			string msg = rm.GetString("msg_error_get_volvalues");
			string cap = rm.GetString("unfortunately_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void MsgDBnotInitialized(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				sender.GetType().Assembly);
			string msg = rm.GetString("msg_db_not_init");
			string cap = rm.GetString("unfortunately_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Confirm exit the program</summary>
		public static DialogResult ConfirmCancel(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				sender.GetType().Assembly);
			string msg = rm.GetString("confirm_cancel_msg");
			string cap = rm.GetString("warning_caption");
			return MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.YesNo,
				MessageBoxIcon.Information);
		}

		/// <summary>Confirm exit the program</summary>
		public static DialogResult ConfirmExit(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				sender.GetType().Assembly);
			string msg = rm.GetString("confirm_exit_msg");
			string cap = rm.GetString("warning_caption");
			return MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.YesNo,
				MessageBoxIcon.Information);
		}

		/// <summary>Shows file read error message box</summary>
		/// <param name="sender">Sender object</param>
		/// <param name="FileName">Name of error file</param>
		public static void FileReadError(object sender, string FileName)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = String.Format(rm.GetString("msg_file_read_error_text"), FileName);
			string cap = rm.GetString("msg_file_read_error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows file read error message box</summary>
		/// <param name="sender">Sender object</param>
		public static void ErrorFromDevice(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_error_from_device");
			string cap = rm.GetString("unfortunately_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device connection error message box</summary>
		/// <param name="sender">Sender object</param>
		/// <param name="portInterface">Connection interface</param>
		/// <param name="portSettings">Connection interface settings</param>
		public static void DeviceConnectionError(object sender, 
					EmPortType portInterface, object[] portSettings)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_connect_error_text");
			string cap = rm.GetString("msg_device_connect_error_caption");

			switch (portInterface)				
			{
				case EmPortType.COM:
					msg = string.Format(msg, portInterface, (string)portSettings[0] + ", ", 
										(uint)portSettings[1]);
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
			
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device connection error message box</summary>
		public static void InvalidInterface(object sender,
					EmPortType portInterface, string curDevice)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
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

			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device connection error message box</summary>
		/// <param name="sender">Sender object</param>
		/// <param name="portInterface">Connection interface</param>
		/// <param name="portSettings">Connection interface settings</param>
		public static void ReadDevInfoError(object sender,
					EmPortType portInterface, object[] portSettings)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
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

			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void DeviceConnectionEm33TError(object sender,
					EmPortType portInterface, object[] portSettings)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_connect_em33t_error_text");
			string cap = rm.GetString("msg_device_connect_error_caption");

			switch (portInterface)
			{
				case EmPortType.COM:
					msg = string.Format(msg, portInterface, (string)portSettings[0] + ", ", (int)portSettings[1]);
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

			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device read error message box</summary>
		/// <param name="sender">Sender object</param>
		public static void DeviceReadError(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_read_error");
			string cap = rm.GetString("msg_device_read_error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device read error message box</summary>
		public static void InvalidDeviceFunction(object sender, string device)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = String.Format(rm.GetString("msg_invalid_device_function_text"), device);
			string cap = rm.GetString("unfortunately_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows device read error message box</summary>
		/// <param name="sender">Sender object</param>
		public static void DeviceHasNoData(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_empty_text");
			string cap = rm.GetString("information_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>Shows device constraints saved message box</summary>
		public static void DeviceConstraintsSaved(object sender, EmDeviceType devType)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg;
			if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM33T1)
				msg = rm.GetString("msg_device_const_saved_em33t_text");
			else
				msg = rm.GetString("msg_device_const_saved_text");
			string cap = rm.GetString("msg_device_saved_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>Shows device object names saved message box</summary>
		public static void DeviceObjectNamesSaved(object sender, EmDeviceType devType)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg;
			if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM33T1)
				msg = rm.GetString("msg_device_objnames_em33t_saved");
			else
				msg = rm.GetString("msg_device_objnames_saved");
			string cap = rm.GetString("msg_device_saved_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>Shows device object names saved message box</summary>
		/// <param name="sender">Sender object</param>
		public static void DeviceNominalsSaved(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_nom_times_saved");
			string cap = rm.GetString("msg_device_saved_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>Shows device input data error message box</summary>
		/// <param name="sender">Sender object</param>
		public static void DeviceInputDataError(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_input_error");
			string cap = rm.GetString("msg_device_input_error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows inserting in database error message box</summary>
		/// <param name="sender">Sender object</param>
		/// <param name="errorTable">Table name inseting in which throwed an exception</param>
		public static void DatabaseInsertingError(object sender, string errorTable)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = String.Format(rm.GetString("msg_db_insert_error_text"), errorTable);
			string cap = rm.GetString("msg_db_insert_error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		#region Options

		#region Device

		/// <summary>Shows prompt to autodetect operation</summary>
		public static DialogResult OptionsAutoDetectPrompt(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_options_dev_autodetect_text");
			string cap = rm.GetString("caption_device_autodetect");
			return MessageBox.Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
		}

		/// <summary>Shows successfull result of autodetect operation</summary>
		public static void OptionsAutoDetectOk(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_autodetect_ok");
			string cap = rm.GetString("ok_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>Shows failed result of autodetect operation</summary>
		public static void OptionsAutoDetectFailed(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_autodetect_failed");
			string cap = rm.GetString("unfortunately_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		#region Database server

		/// <summary>Shows successfull result of database server connection</summary>
		public static void DbConnectionOk(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_db_connect_ok");
			string cap = rm.GetString("ok_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


		public static void DbConnectError(object sender, string host, int port, string database)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string strMsg = String.Format(
				rm.GetString("msg_db_connect_error_text"), database, host, port);
			string strCap = rm.GetString("unfortunately_caption");
			MessageBox.Show(strMsg, strCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		#endregion

		#region Licences

		public static DialogResult OptionsDropLicenceQuestion(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_licences_check_drop_text");
			string cap = rm.GetString("msg_licences_check_drop_caption");
			return MessageBox.Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
		}

		/// <summary>Shows device input data error message box</summary>
		/// <param name="sender">Sender object</param>
		public static void DeviceIsNotLicenced(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_device_licence_failed_text");
			string cap = rm.GetString("msg_device_licence_failed_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		#endregion

		public static void InvalidPhoneNumber(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_phone_number");
			string cap = rm.GetString("error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void InvalidIP(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_ip_address");
			string cap = rm.GetString("error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void InvalidPort(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_ip_port");
			string cap = rm.GetString("error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void InvalidData(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_data");
			string cap = rm.GetString("error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void InvalidSerial(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_serial");
			string cap = rm.GetString("error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		#endregion

		#region Constraints

		/// <summary>Shows input data error message box</summary>
		public static void ConstraintsInputDataError(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_constraints_input_data_failed_text");
			string cap = rm.GetString("msg_constraints_input_data_failed_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>Shows input data error message box high limit</summary>
		public static void ConstraintsInputDataErrorHighLimit(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_invalid_high_limit_text");
			string cap = rm.GetString("msg_constraints_input_data_failed_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void ConstraintsSaved(object sender, string fname)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = string.Format(rm.GetString("msg_constraints_saved"), fname);
			string cap = rm.GetString("ok_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		public static void DbOptionsApplyError(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_db_options_apply_error_text");
			string cap = rm.GetString("msg_db_options_apply_error_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static DialogResult DeleteConfirmation(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_interface_delete_confirmation");
			string cap = rm.GetString("warning_caption");
			return MessageBox.Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
		}

		public static void NoPgServers(object sender)
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
			string msg = rm.GetString("msg_image_no_pg_servers_text");
			string cap = rm.GetString("msg_image_no_pg_servers_caption");
			MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		public static void UnknownDevType(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_unknown_devtype");
				string cap = rm.GetString("warning_caption");
				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in UnknownDevType:");
				throw;
			}
		}

		public static void ErrorLoadConstraints(object sender, string mess)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_load_constraints");
				string cap = rm.GetString("unfortunately_caption");

				msg = string.Format(msg, mess);

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorLoadConstraints:");
				throw;
			}
		}

		public static void ErrorSaveConstraints(object sender, string mess)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_save_constraints");
				string cap = rm.GetString("unfortunately_caption");

				msg = string.Format(msg, mess);

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorSaveConstraints:");
				throw;
			}
		}

		public static void ErrorSaveDeviceName(object sender, string mess)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_save_devname");
				string cap = rm.GetString("unfortunately_caption");

				msg = string.Format(msg, mess);

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorSaveDeviceName:");
				throw;
			}
		}

		public static void ErrorLoadDeviceName(object sender, string mess)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_load_devname");
				string cap = rm.GetString("unfortunately_caption");

				msg = string.Format(msg, mess);

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorLoadDeviceName:");
				throw;
			}
		}

		public static void ErrorLoadObjectNames(object sender, string mess)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_load_objnames");
				string cap = rm.GetString("unfortunately_caption");

				msg = string.Format(msg, mess);

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorLoadObjectNames:");
				throw;
			}
		}

		public static void ErrorSaveObjectNames(object sender, string mess)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_save_objnames");
				string cap = rm.GetString("unfortunately_caption");

				msg = string.Format(msg, mess);

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorSaveObjectNames:");
				throw;
			}
		}

		public static void ErrorConnectDevice(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_device_connect_error_short");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorConnectDevice:");
				throw;
			}
		}

		public static void ErrorConnectDB(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_db_connect_error");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorConnectDevice:");
				throw;
			}
		}

		public static void ErrorStartDateMoreThanEndDate(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_start_date_more_than_end_date");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorStartDateMoreThanEndDate:");
				throw;
			}
		}

		public static void ErrorLoadXmlImage(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_load_xml_image");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorLoadXmlImage:");
				throw;
			}
		}

		public static void ErrorLoadSqlImage(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_load_sql_image");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorLoadSqlImage:");
				throw;
			}
		}

		public static void ErrorCantAccessFile(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_cant_access_file");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorCantAccessFile:");
				throw;
			}
		}

		public static void ErrorCreateDir(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_create_dir");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorCreateDir:");
				throw;
			}
		}

		public static void ExportSuccess(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_export_success");
				string cap = rm.GetString("ok_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportSuccess:");
				throw;
			}
		}

		public static void ErrorExport(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_export");
				string cap = rm.GetString("unfortunately_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ErrorExport:");
				throw;
			}
		}

		public static void MsgOpenArchiveForFSK(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_fsk_report_open_archive");
				string cap = rm.GetString("warning_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in MsgOpenArchiveForFSK:");
				throw;
			}
		}

		public static void MsgOpenArchiveForFSK_TheSameObject(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_fsk_report_the_same_object");
				string cap = rm.GetString("warning_caption");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in MsgOpenArchiveForFSK_TheSameObject:");
				throw;
			}
		}

        public static void MsgOutOfMemory(object sender)
        {
            try
            {
                ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", sender.GetType().Assembly);
                string msg = rm.GetString("msg_outofmemory");
                string cap = rm.GetString("unfortunately_caption");

                MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in MsgOutOfMemory:");
                throw;
            }
        }
	}
}
