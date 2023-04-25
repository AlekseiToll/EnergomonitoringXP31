using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EmServiceLib;
using DeviceIO;
using EmDataSaver;
using DbServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmDeviceName : Form
	{
		private Settings settings_;
		private frmMain mainForm_;

		public frmDeviceName(Settings settings, frmMain main)
		{
			InitializeComponent();

			settings_ = settings;
			mainForm_ = main;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
				// the function works ONLY for Em32
				if (settings_.CurDeviceType != EmDeviceType.EM32)
				{
					MessageBoxes.InvalidDeviceFunction(this, settings_.CurDeviceType.ToString());
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				object[] port_params = null;
				if (settings_.IOInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				if (settings_.IOInterface == EmPortType.Modem)
				{
					port_params = new object[5];
					port_params[0] = settings_.SerialPortNameModem;
					port_params[1] = settings_.SerialSpeedModem;
					port_params[2] = settings_.CurPhoneNumber;
					port_params[3] = settings_.AttemptNumber;
					port_params[4] = settings_.CurDeviceAddress;
				}
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				if (settings_.IOInterface == EmPortType.GPRS)
				{
					port_params = new object[3];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
					port_params[2] = settings_.CurDeviceAddress;
				}
				if (settings_.IOInterface == EmPortType.Rs485)
				{
					port_params = new object[3];
					port_params[0] = settings_.SerialPortName485;
					port_params[1] = settings_.SerialSpeed485;
					port_params[2] = settings_.CurDeviceAddress;
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				Em32Device device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);

				ExchangeResult errCode = ExchangeResult.Other_Error;
				Int64 serial = 0;
				try
				{
					serial = device.OpenDevice();
					if (serial == -1)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					if (!settings_.Licences.IsLicenced(serial))
					{
						MessageBoxes.DeviceIsNotLicenced(this);
						return;
					}

					int oneNameLen = 16;
					byte[] buffer = new byte[oneNameLen];
					System.Text.Encoding.Default.GetBytes(tbDevName.Text, 0, tbDevName.Text.Length, buffer, 0);

					// reversing
					//for (int i = 0; i < oneNameLen - 2; i += 2)
					//{
					//    byte Char = buffer[i];
					//    buffer[i] = buffer[i + 1];
					//    buffer[i + 1] = Char;
					//}
					// fixing ASCII bug
					for (int i = 0; i < oneNameLen; i++)
					{
						if (buffer[i] >= 0xE0) buffer[i] -= 0x40;
						else if (buffer[i] >= 0xC0) buffer[i] -= 0x20;
						if (buffer[i] == 0x00) buffer[i] = 0x20;
					}

					errCode = device.WriteDeviceName(ref buffer);
				}
				finally
				{
					if (device != null) device.Close();
				}

				if (errCode == ExchangeResult.OK)						// no errors
				{
					MessageBoxes.DeviceObjectNamesSaved(this, settings_.CurDeviceType);

					// update device name in Database
					DbService dbService = new DbService(settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEm32);
					if (!dbService.Open())
					{
						EmService.WriteToLogFailed("Error while open DB to update device name");
						MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
						return;
					}
					try
					{
						string commandText = String.Format("UPDATE devices SET object_name = '{0}' WHERE ser_number = {1};", tbDevName.Text, serial);
						dbService.ExecuteNonQuery(commandText, true);

						string folder_name = string.Format("{0}  #{1}",
											serial,
											tbDevName.Text);
						commandText = String.Format("UPDATE folders SET name = '{0}' WHERE device_id = (SELECT dev_id FROM devices WHERE ser_number = {1}) AND folder_type = 3;", folder_name, serial);
						dbService.ExecuteNonQuery(commandText, true);
					}
					catch (Exception nex)
					{
						EmService.WriteToLogFailed("Error while updating Device info: " + nex.Message);
					}
					finally
					{
						if (dbService != null) dbService.CloseConnect();
					}

					return;
				}
				else if (errCode == ExchangeResult.Other_Error)					// serial port connection error
				{
					MessageBoxes.DeviceReadError(this);
					return;
				}
				else if (errCode == ExchangeResult.Write_Error)					// write error
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
					return;
				}
			}
			catch (EmDisconnectException dex)
			{
				//MessageBox.Show("Unable to connect to device!");
				MessageBoxes.ErrorConnectDevice(this);
				EmService.DumpException(dex, "Error in frmDeviceName::btnOk_Click:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (EmException emx)
			{
				MessageBoxes.ErrorSaveDeviceName(this, emx.Message);
				EmService.DumpException(emx, "Error in frmDeviceName::btnOk_Click:");
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDeviceName::btnOk_Click:");
				throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void btnRead_Click(object sender, EventArgs e)
		{
			try
			{
				// the function works ONLY for Em32
				if (settings_.CurDeviceType != EmDeviceType.EM32)
				{
					MessageBoxes.InvalidDeviceFunction(this, settings_.CurDeviceType.ToString());
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				object[] port_params = null;
				if (settings_.IOInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				if (settings_.IOInterface == EmPortType.Modem)
				{
					port_params = new object[5];
					port_params[0] = settings_.SerialPortNameModem;
					port_params[1] = settings_.SerialSpeedModem;
					port_params[2] = settings_.CurPhoneNumber;
					port_params[3] = settings_.AttemptNumber;
					port_params[4] = settings_.CurDeviceAddress;
				}
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				if (settings_.IOInterface == EmPortType.GPRS)
				{
					port_params = new object[3];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
					port_params[2] = settings_.CurDeviceAddress;
				}
				if (settings_.IOInterface == EmPortType.Rs485)
				{
					port_params = new object[3];
					port_params[0] = settings_.SerialPortName485;
					port_params[1] = settings_.SerialSpeed485;
					port_params[2] = settings_.CurDeviceAddress;
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				Em32Device device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
				ExchangeResult errCode = ExchangeResult.Other_Error;
				try
				{
					Int64 serial = device.OpenDevice();
					if (serial == -1)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					if (!settings_.Licences.IsLicenced(serial))
					{
						MessageBoxes.DeviceIsNotLicenced(this);
						return;
					}

					string name;
					errCode = device.ReadDeviceName(out name);
					if (errCode == ExchangeResult.OK)
					{
						tbDevName.Text = name;
					}
				}
				finally
				{
					if (device != null) device.Close();
				}

				if (errCode == ExchangeResult.Parse_Error || errCode == ExchangeResult.Read_Error)
				{
					MessageBoxes.DeviceReadError(this);
					return;
				}
				else if (errCode != ExchangeResult.OK)		// device connection error
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
					return;
				}
			}
			catch (EmDisconnectException dex)
			{
				//MessageBox.Show("Unable to connect to device!");
				MessageBoxes.ErrorConnectDevice(this);
				EmService.DumpException(dex, "Error in btnLoadFromDevice:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (EmException emx)
			{
				MessageBoxes.ErrorLoadDeviceName(this, emx.Message);
				EmService.DumpException(emx, "Error in btnLoadFromDevice:");
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in btnLoadFromDevice:");
				throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}
	}
}