using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NativeWifi;

using EmDataSaver;
using EmServiceLib;
using DeviceIO;

namespace EnergomonitoringXP
{
	public partial class frmDeclaredU : Form
	{
		private Settings settings_;
		private Form mainForm_;

		public frmDeclaredU(ref Settings settings, Form main)
		{
			InitializeComponent();

			this.settings_ = settings;
			this.mainForm_ = main;
		}

		private void frmDeclaredU_Load(object sender, EventArgs e)
		{

		}

		private void btnRead_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				ExchangeResult errCode = ExchangeResult.Other_Error;

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				object[] port_params = null;
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.WI_FI)
				{
					try
					{
						if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
						{
							WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
																						settings_.WifiPassword);

							if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
							{
								EmService.WriteToLogFailed("frmObjectNames: Wi-fi not connected!");
								MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
								return;
							}
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "frmObjectNames: Exception in ConnectToWifi() WI-FI:");
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					port_params = new object[2];
					port_params[0] = EmService.GetCurrentDhcpIpAddress();
					port_params[1] = settings_.CurrentPort;
				}

				EtPQP_A_Device device = new EtPQP_A_Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, settings_.CurWifiProfileName, settings_.WifiPassword, 
													 (mainForm_ as Form).Handle);
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

					byte[] buffer = null;

					errCode = device.Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
						new object[] { (ushort)160 });
					if (errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceReadError(this);
						return;
					}
					tbCurValue.Text = Conversions.bytes_2_uint_new(ref buffer, 0).ToString();

					errCode = device.Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
						new object[] { (ushort)161 });
					if (errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceReadError(this);
						return;
					}
					tbMin.Text = Conversions.bytes_2_uint_new(ref buffer, 0).ToString();

					errCode = device.Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
						new object[] { (ushort)162 });
					if (errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceReadError(this);
						return;
					}
					tbMax.Text = Conversions.bytes_2_uint_new(ref buffer, 0).ToString();
				}
				finally
				{
					if (device != null) device.Close();
				}

				tbNewValue.Enabled = true;
			}
			catch (EmDisconnectException dex)
			{
				MessageBoxes.ErrorConnectDevice(this);
				EmService.DumpException(dex, "Error in frmDeclaredU::btnLoadFromDevice:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (Exception ex)
			{
				if (System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
					MessageBox.Show("Ошибка при чтении из прибора!");
				else MessageBox.Show("Error while reading from the device!");
				EmService.DumpException(ex, "Error in frmDeclaredU::btnLoadFromDevice:");
				//throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void btnWrite_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				Int32 newVal;
				if (!Int32.TryParse(tbNewValue.Text, out newVal))
				{
					if (System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
						MessageBox.Show("Неверный формат числа!");
					else MessageBox.Show("Number format is invalid!");
					return;
				}

				try
				{
					if (newVal > Single.Parse(tbMax.Text) || newVal < Single.Parse(tbMin.Text))
					{
						if (System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
							MessageBox.Show("Число должно быть не меньше минимального и не больше максимального значения!");
						else MessageBox.Show("Number must be between minimum and maximum values!");
						return;
					}
				}
				catch
				{
					if (System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
						MessageBox.Show("Ошибка при интерпретации минимального или максимального значения");
					else MessageBox.Show("Minimum or maximum value is invalid");
					throw;
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				object[] port_params = null;
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.WI_FI)
				{
					try
					{
						if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
						{
							WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
																						settings_.WifiPassword);

							if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
							{
								EmService.WriteToLogFailed("frmObjectNames: Wi-fi not connected!");
								MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
								return;
							}
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "frmObjectNames: Exception in ConnectToWifi() WI-FI:");
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					port_params = new object[2];
					port_params[0] = EmService.GetCurrentDhcpIpAddress();
					port_params[1] = settings_.CurrentPort;
				}

				EtPQP_A_Device device = new EtPQP_A_Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, settings_.CurWifiProfileName, settings_.WifiPassword,
													 (mainForm_ as Form).Handle);

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

					byte[] buffer = new byte[sizeof(Int32)];
					Conversions.int_2_bytes(Int32.Parse(tbNewValue.Text), ref buffer, 0);
					errCode = ((EtPQP_A_Device)device).WriteSystemData(160, ref buffer);
				}
				finally
				{
					if (device != null) device.Close();
				}

				if (errCode == ExchangeResult.OK)						// no errors
				{
					if (System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
						MessageBox.Show("Значение успешно записано!");
					else MessageBox.Show("The value was written successfully!");
					return;
				}
				else if (errCode == ExchangeResult.Write_Error)			// write error
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
					return;
				}
				else if (errCode != ExchangeResult.OK)					// serial port connection error
				{
					MessageBoxes.DeviceReadError(this);
					return;
				}
			}
			catch (EmDisconnectException dex)
			{
				//MessageBox.Show("Unable to connect to device!");
				MessageBoxes.ErrorConnectDevice(this);
				EmService.DumpException(dex, "Error in frmDeclaredU::btnSaveToDevice:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (Exception ex)
			{
				if (System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
					MessageBox.Show("Ошибка при записи в прибор!");
				else MessageBox.Show("Error while writing into the device!");
				EmService.DumpException(ex, "Error in frmDeclaredU::btnSaveToDevice:");
				return;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void tbNewValue_TextChanged(object sender, EventArgs e)
		{
			btnWrite.Enabled = tbNewValue.Text.Length > 0;
		}

		private void tbNewValue_KeyPress(object sender, KeyPressEventArgs e)
		{
			string vlCell = ((TextBox)sender).Text;

			if (!Char.IsDigit(e.KeyChar) /*&& e.KeyChar != '.' && e.KeyChar != ',' */&& e.KeyChar != '\b')
				e.Handled = true;
		}
	}
}
