using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using NativeWifi;

using DeviceIO;
using DeviceIO.Memory;
using EmServiceLib;
using EmDataSaver;

namespace EnergomonitoringXP
{
	public partial class frmObjectNames : Form
	{
		private Settings settings_;
		private Form mainForm_;
		
		public frmObjectNames(Settings settings, Form main)
		{
			InitializeComponent();
			
			this.settings_ = settings;
			this.mainForm_ = main;
		}

		private void frmObjectNames_Load(object sender, EventArgs e)
		{
			btnClearAll_Click(sender, e);
		}

		private void btnClearAll_Click(object sender, EventArgs e)
		{
			tableNames.Clear();

			int count = EM33TObjNames.COUNT_NAMES;
			if (settings_.CurDeviceType == EmDeviceType.ETPQP || settings_.CurDeviceType == EmDeviceType.ETPQP_A)
				count = EMSLIPObjNames.GetCountNames(settings_.CurDeviceType);
			for (int i = 0; i < count; i++)
			{
				DataRow newRow = tableNames.NewRow();
				newRow[0] = i + 1;
				newRow[1] = String.Empty;
				tableNames.Rows.Add(newRow);
			}
		}

		private void btnLoadDefaults_Click(object sender, EventArgs e)
		{
			string str = "Object";
			if (settings_.Language == "Ðóññêèé")	str = "ÎÁÚÅÊÒ";

			int count = EM33TObjNames.COUNT_NAMES;
			if (settings_.CurDeviceType == EmDeviceType.ETPQP || settings_.CurDeviceType == EmDeviceType.ETPQP_A) 
				count = EMSLIPObjNames.GetCountNames(settings_.CurDeviceType);
			for (int i = 0; i < count; i++)
			{
				tableNames.Rows[i].BeginEdit();
				tableNames.Rows[i].ItemArray = new object[2] { i + 1, String.Format("{0} {1}", str, i + 1) };
				tableNames.Rows[i].EndEdit();
			}		
		}

		private void btnSaveToFile_Click(object sender, EventArgs e)
		{
			SaveFileDialog fd = new SaveFileDialog();
			fd.DefaultExt = "names";
			fd.AddExtension = true;
			fd.FileName = "unnamed.names";
			fd.Filter = "Object names files (*.names)|*.names|All files (*.*)|*.*";

			if (fd.ShowDialog(this) != DialogResult.OK) return;

			tableNames.WriteXml(fd.FileName);
		}

		private void btnLoadFromFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog();
			fd.DefaultExt = "names";
			fd.AddExtension = true;
			fd.Filter = "Object names files (*.names)|*.names|All files (*.*)|*.*";

			if (fd.ShowDialog(this) != DialogResult.OK) return;

			try
			{
				tableNames.Clear();
				tableNames.ReadXml(fd.FileName);
			}
			catch
			{
				btnClearAll_Click(sender, e);
				MessageBoxes.FileReadError(this, fd.FileName);
			}
		}

		private void btnLoadFromDevice_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				// the function does NOT work for Em32 (it haven't got objects)
				if (settings_.CurDeviceType == EmDeviceType.EM32 ||
					settings_.CurDeviceType == EmDeviceType.EM31K)
				{
					MessageBoxes.InvalidDeviceFunction(this, settings_.CurDeviceType.ToString());
					return;
				}

				object[] port_params = null;
				ExchangeResult errCode = ExchangeResult.Other_Error;

				if (settings_.IOInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				else if (settings_.IOInterface == EmPortType.Ethernet)
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

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				EmDevice device = null;
				switch (settings_.CurDeviceType)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
						device = new Em33TDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					//case EmDeviceType.EM31K:
					//    device = new Em31KDevice(settings_.IOInterface, settings_.CurDeviceAddress,
					//                                 false, port_params, (mainForm_ as Form).Handle);
					//    break;
					//case EmDeviceType.EM32:
					//    device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
					//                                 false, port_params, (mainForm_ as Form).Handle);
					//    break;

					case EmDeviceType.ETPQP:
						device = new EtPQPDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP_A:
						device = new EtPQP_A_Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, settings_.CurWifiProfileName, 
													 settings_.WifiPassword, 
													 (mainForm_ as Form).Handle);
						break;

					default:
						MessageBoxes.UnknownDevType(this);
						return;
				}

				string[] strNames = null;
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
					string[] names = null;
					if (settings_.CurDeviceType == EmDeviceType.EM33T ||
						settings_.CurDeviceType == EmDeviceType.EM33T1)
					{
						EM33TObjNames pageObjNames = new EM33TObjNames();
						errCode = ((Em33TDevice)device).Read(DeviceIO.Memory.EMemory.FRAM,
									pageObjNames.Address, (ushort)(pageObjNames.Size / 2), ref buffer, false);
						if (errCode == ExchangeResult.OK)
						{
							if (!pageObjNames.Parse(ref buffer)) errCode = ExchangeResult.Other_Error;
						}
						strNames = pageObjNames.ListNames.ToArray();
					}
					//else if (settings_.CurDeviceType == EmDeviceType.EM31K)
					//{	
					//}
					//else if (settings_.CurDeviceType == EmDeviceType.EM32)
					//{
					//}
					else if (settings_.CurDeviceType == EmDeviceType.ETPQP)
					{
						EMSLIPObjNames pageObjNames = new EMSLIPObjNames(settings_.CurDeviceType);
						errCode = ((EtPQPDevice)device).ReadObjectNames(ref names);
						if (errCode == ExchangeResult.OK)
						{
							if (!pageObjNames.Parse(ref names)) errCode = ExchangeResult.Parse_Error;
						}
						strNames = pageObjNames.ListNames.ToArray();
					}
					else if (settings_.CurDeviceType == EmDeviceType.ETPQP_A)
					{
						EMSLIPObjNames pageObjNames = new EMSLIPObjNames(settings_.CurDeviceType);
						errCode = ((EtPQP_A_Device)device).ReadObjectNames(ref names);
						if (errCode == ExchangeResult.OK)
						{
							if (!pageObjNames.Parse(ref names)) errCode = ExchangeResult.Parse_Error;
						}
						strNames = pageObjNames.ListNames.ToArray();
					}
					else
					{
						MessageBoxes.UnknownDevType(this);
						return;
					}
				}
				finally
				{
					if (device != null) device.Close();
				}

				if (errCode == ExchangeResult.Parse_Error || errCode == ExchangeResult.Read_Error)		// read error or parse error
				{
					MessageBoxes.DeviceReadError(this);
					return;
				}
				else if (errCode != ExchangeResult.OK)		// device connection error
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
					return;
				}

				tableNames.Clear();
				int count = EM33TObjNames.COUNT_NAMES;
				if (settings_.CurDeviceType == EmDeviceType.ETPQP || settings_.CurDeviceType == EmDeviceType.ETPQP_A) 
					count = EMSLIPObjNames.GetCountNames(settings_.CurDeviceType);

				for (int i = 0; i < count; i++)
				{
					DataRow newRow = tableNames.NewRow();
					newRow[0] = i + 1;
					newRow[1] = strNames[i];
					tableNames.Rows.Add(newRow);
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
				MessageBoxes.ErrorLoadObjectNames(this, emx.Message);
				EmService.DumpException(emx, "Error in btnLoadFromDevice:");
				return;
			}
			catch(Exception ex)
			{
				EmService.DumpException(ex, "Error in btnLoadFromDevice:");
				throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void btnSaveToDevice_Click(object sender, EventArgs e)
		{
			try
			{
				// the function does NOT work for Em32 (it haven't got objects)
				if (settings_.CurDeviceType == EmDeviceType.EM32 ||
					settings_.CurDeviceType == EmDeviceType.EM31K)
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
				else if (settings_.IOInterface == EmPortType.Ethernet)
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

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				EmDevice device = null;
				switch (settings_.CurDeviceType)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
						device = new Em33TDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					//case EmDeviceType.EM31K:
					//    device = new Em31KDevice(settings_.IOInterface, settings_.CurDeviceAddress,
					//                                 false, port_params, (mainForm_ as Form).Handle);
					//    break;
					//case EmDeviceType.EM32:
					//    device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
					//                                 false, port_params, (mainForm_ as Form).Handle);
					//    break;

					case EmDeviceType.ETPQP:
						device = new EtPQPDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;
					case EmDeviceType.ETPQP_A:
						device = new EtPQP_A_Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, settings_.CurWifiProfileName, settings_.WifiPassword,
													 (mainForm_ as Form).Handle);
						break;

					default:
						MessageBoxes.UnknownDevType(this);
						return;
				}

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

					int count = EM33TObjNames.COUNT_NAMES;
					if (settings_.CurDeviceType == EmDeviceType.ETPQP || settings_.CurDeviceType == EmDeviceType.ETPQP_A) 
						count = EMSLIPObjNames.GetCountNames(settings_.CurDeviceType);

					string[] names = new string[count];
					DataRow[] rows = tableNames.Select();
					for (int i = 0; i < count; i++)
					{
						names[i] = rows[i][1].ToString();
					}

					byte[] buffer = null;

					if (settings_.CurDeviceType == EmDeviceType.EM33T ||
						settings_.CurDeviceType == EmDeviceType.EM33T1)
					{
						EM33TObjNames pageObjNames = new EM33TObjNames();
						pageObjNames.FillStrings(ref names);
						buffer = pageObjNames.Pack();

						errCode = ((Em33TDevice)device).Write(DeviceIO.Memory.EMemory.FRAM,
							pageObjNames.Address,
							(ushort)(pageObjNames.Size / 2),
							ref buffer);
						if (errCode == ExchangeResult.OK)
						{
							errCode = ((Em33TDevice)device).Write(DeviceIO.Memory.EMemory.RAM,
												pageObjNames.Address,
												(ushort)(pageObjNames.Size / 2),
												ref buffer);
						}
					}
					//else if (settings_.CurDeviceType == EmDeviceType.EM31K)
					//{	
					//}
					//else if (settings_.CurDeviceType == EmDeviceType.EM32)
					//{
					//}
					else if (settings_.CurDeviceType == EmDeviceType.ETPQP)
					{
						EMSLIPObjNames pageObjNames = new EMSLIPObjNames(settings_.CurDeviceType);
						pageObjNames.FillStrings(ref names);
						buffer = pageObjNames.Pack();

						errCode = ((EtPQPDevice)device).WriteObjectNames(ref buffer);
					}
					else if (settings_.CurDeviceType == EmDeviceType.ETPQP_A)
					{
						EMSLIPObjNames pageObjNames = new EMSLIPObjNames(settings_.CurDeviceType);
						pageObjNames.FillStrings(ref names);
						buffer = pageObjNames.Pack();

						errCode = ((EtPQP_A_Device)device).WriteObjectNames(ref buffer);
					}
					else
					{
						MessageBoxes.UnknownDevType(this);
						return;
					}
				}
				finally
				{
					if (device != null) device.Close();
				}

				if (errCode == ExchangeResult.OK)						// no errors
				{
					MessageBoxes.DeviceObjectNamesSaved(this, settings_.CurDeviceType);
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
				EmService.DumpException(dex, "Error in btnSaveToDevice:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (EmException emx)
			{
				MessageBoxes.ErrorSaveObjectNames(this, emx.Message);
				EmService.DumpException(emx, "Error in btnSaveToDevice:");
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in btnSaveToDevice:");
				throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}
	}
}