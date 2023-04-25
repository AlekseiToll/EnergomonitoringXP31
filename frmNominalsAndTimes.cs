using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DeviceIO;
using EmServiceLib;
using EmDataSaver;

namespace EnergomonitoringXP
{
	public partial class frmNominalsAndTimes : Form
	{
		Settings settings_;
		private Form mainForm_;

		public frmNominalsAndTimes(Settings settings, Form main)
		{
			InitializeComponent();

			settings_ = settings;
			mainForm_ = main;
		}

		private void loadDefaults(DataTable table)
		{
			table.Clear();

			DataRow newRow;

			newRow = table.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Nominal frequency (Hz):" : "Номинальная частота (Гц):";
			newRow[1] = String.Empty;
			table.Rows.Add(newRow);

			newRow = table.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Nominal line voltage (V):" : "Номинальное линейное напряжение (В):";
			newRow[1] = String.Empty;
			table.Rows.Add(newRow);

			newRow = table.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Nominal phase voltage (V):" : "Номинальное фазное напряжение (В):";
			newRow[1] = String.Empty;
			table.Rows.Add(newRow);

			newRow = myTable.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Peak loading start time 1 (hh:mm):" : "Время начала макс. нагрузок 1(чч:мм):";
			newRow[1] = String.Empty;
			myTable.Rows.Add(newRow);

			newRow = table.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Peak loading end time 1 (hh:mm):" : "Время окончания макс. нагрузок 1 (чч:мм):";
			newRow[1] = String.Empty;
			table.Rows.Add(newRow);

			newRow = myTable.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Peak loading start time 1 (hh:mm):" : "Время начала макс. нагрузок 2(чч:мм):";
			newRow[1] = String.Empty;
			myTable.Rows.Add(newRow);

			newRow = table.NewRow();
			newRow[0] = settings_.CurrentLanguage.Equals("en") ? "Peak loading end time 1 (hh:mm):" : "Время окончания макс. нагрузок 2 (чч:мм):";
			newRow[1] = String.Empty;
			table.Rows.Add(newRow);

			table.AcceptChanges();
		}

		private void frmNominalsAndTimes_Load(object sender, EventArgs e)
		{			
			loadDefaults(myTable);
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		#region I/O File

		private void btnSaveToFile_Click(object sender, EventArgs e)
		{
			SaveFileDialog fd = new SaveFileDialog();
			fd.DefaultExt = "nominals";
			fd.AddExtension = true;
			fd.FileName = "unnamed.nominals";
			fd.Filter = "Nominals and times files (*.nominals)|*.nominals|All files (*.*)|*.*";
			if (fd.ShowDialog(this) != DialogResult.OK) return;

			myTable.WriteXml(fd.FileName);
		}

		private void btnLoadFromFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog();
			fd.DefaultExt = "nominals";
			fd.AddExtension = true;
			fd.Filter = "Nominals and times files (*.nominals)|*.nominals|All files (*.*)|*.*";

			if (fd.ShowDialog(this) != DialogResult.OK) return;

			try
			{
				DataSet temp = new DataSet();
				
				temp.ReadXml(fd.FileName);

				for (int i = 0; i < temp.Tables[0].Rows.Count; i++)
				{
					myTable.Rows[i].ItemArray = new object[] { 
							myTable.Rows[i][0], 
							temp.Tables[0].Rows[i][1] 
					};
				}
				myTable.AcceptChanges();
			}
			catch
			{
				loadDefaults(myTable);
				MessageBoxes.FileReadError(this, fd.FileName);
			}
		}

		#endregion

		#region I/O Device

		private void btnLoadFromDevice_Click(object sender, EventArgs e)
		{
			EmDevice device = null;

			try
			{
				if (settings_.CurDeviceType == EmDeviceType.EM31K)
				{
					MessageBoxes.InvalidDeviceFunction(this, settings_.CurDeviceType.ToString());
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				object[] port_params = null;

				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

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

					case EmDeviceType.EM32:
						device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP:
						device = new EtPQPDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					default:
						MessageBoxes.UnknownDevType(this);
						return;
				}

				object[] vals = new object[7];

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

				if (settings_.CurDeviceType == EmDeviceType.EM33T ||
					settings_.CurDeviceType == EmDeviceType.EM33T1)
				{
					// reading system page... if not successfully, return with error message
					DeviceIO.Memory.EM33TPSystem pageSystem = new DeviceIO.Memory.EM33TPSystem();

					ExchangeResult errCode = (device as Em33TDevice).Read(DeviceIO.Memory.EMemory.FRAM,
												pageSystem.Address, pageSystem.Size,
												ref buffer, false);

					if (!pageSystem.Parse(ref buffer) || errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					vals[0] = pageSystem.Data["FNominal"];
					vals[1] = pageSystem.Data["ULineNominal"];
					vals[2] = pageSystem.Data["UPhaseNominal"];
					vals[3] = ((DateTime)pageSystem.Data["StartPeakLoadInterval1"]).ToString("HH:mm");
					vals[4] = ((DateTime)pageSystem.Data["EndPeakLoadInterval1"]).ToString("HH:mm");
					vals[5] = ((DateTime)pageSystem.Data["StartPeakLoadInterval2"]).ToString("HH:mm");
					vals[6] = ((DateTime)pageSystem.Data["EndPeakLoadInterval2"]).ToString("HH:mm");
				}
				else if (settings_.CurDeviceType == EmDeviceType.EM32)
				{
					ExchangeResult errCode = (device as Em32Device).ReadNominalsAndTimes(ref vals);
					if (errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

				}
				else if (settings_.CurDeviceType == EmDeviceType.ETPQP)
				{
					ExchangeResult errCode = (device as EtPQPDevice).ReadNominalsAndTimes(ref vals);
					if (errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}
				}

				for (int i = 0; i < vals.Length; i++)
				{
					myTable.Rows[i].ItemArray = new object[] { myTable.Rows[i][0], vals[i] };
				}
				myTable.AcceptChanges();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error Load Nominals:");
				throw;
			}
			finally
			{
				if (device != null) device.Close();
				this.Cursor = Cursors.Default;
			}
		}

		private void btnSaveToDevice_Click(object sender, EventArgs e)
		{
			EmDevice device = null;

			try
			{
				if (settings_.CurDeviceType == EmDeviceType.EM31K)
				{
					MessageBoxes.InvalidDeviceFunction(this, settings_.CurDeviceType.ToString());
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				object[] port_params = null;

				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

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

					case EmDeviceType.EM32:
						device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP:
						device = new EtPQPDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					default:
						MessageBoxes.UnknownDevType(this);
						return;
				}

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

				object[] vals = new object[7];
				UInt16 my_ushort = 0;
				Single my_float = 0;
				DateTime my_datetime = DateTime.MinValue;
				DataRow[] rows = myTable.Select();

				#region Check if data is valid

				bool err = false;
				if (System.UInt16.TryParse(rows[0][1].ToString(), out my_ushort))
				{
					vals[0] = my_ushort;
					// f = 50/60 Hz
					if (my_ushort != 50 && my_ushort != 60) err = true;
				}
				else err = true;

				if (System.Single.TryParse(rows[1][1].ToString(), out my_float))
				{
					vals[1] = my_float;
					// U_l = [17;626)
					if (my_float < 17 || my_float > 626) err = true;
				}
				else err = true;

				if (System.Single.TryParse(rows[2][1].ToString(), out my_float))
				{
					vals[2] = my_float;
					// U_ph = [10;360)
					if (my_float < 10 || my_float >= 361) err = true;
				}
				else err = true;

				if (System.DateTime.TryParse(rows[3][1].ToString(), out my_datetime))
				{
					// time is only divisible by 30
					vals[3] = my_datetime;
					if (my_datetime.Minute % 30 != 0) err = true;
				}
				else err = true;

				if (System.DateTime.TryParse(rows[4][1].ToString(), out my_datetime))
				{
					// time is only divisible by 30
					vals[4] = my_datetime;
					if (my_datetime.Minute % 30 != 0) err = true;
				}
				else err = true;

				if (System.DateTime.TryParse(rows[5][1].ToString(), out my_datetime))
				{
					// time is only divisible by 30
					vals[5] = my_datetime;
					if (my_datetime.Minute % 30 != 0) err = true;
				}
				else err = true;

				if (System.DateTime.TryParse(rows[6][1].ToString(), out my_datetime))
				{
					// time is only divisible by 30
					vals[6] = my_datetime;
					if (my_datetime.Minute % 30 != 0) err = true;
				}
				else err = true;

				// parse error
				if (err == true)
				{
					MessageBoxes.DeviceInputDataError(this);
					return;
				}

				#endregion

				//object[] portIDs = null;
				//object[] outData = null;
				byte[] buffer = null;
				ExchangeResult errCode = ExchangeResult.Other_Error;

				if (device.DeviceType == EmDeviceType.EM33T ||
						device.DeviceType == EmDeviceType.EM33T1)
				{
					// reading system page... if not successfully, return with error message
					DeviceIO.Memory.EM33TPSystem pageSystem = new DeviceIO.Memory.EM33TPSystem();

					errCode = (device as Em33TDevice).Read(DeviceIO.Memory.EMemory.FRAM,
										pageSystem.Address, pageSystem.Size, ref buffer, false);
					if (errCode != ExchangeResult.OK)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					Conversions.ushort_2_bytes((ushort)vals[0], ref buffer, 262);
					Conversions.float2w65536_2_bytes((float)vals[1], ref buffer, 264);
					Conversions.float2w65536_2_bytes((float)vals[2], ref buffer, 268);
					buffer[249] = Conversions.DAA_2_byte((byte)(((DateTime)vals[3]).Hour));
					buffer[248] = Conversions.DAA_2_byte((byte)(((DateTime)vals[3]).Minute));
					buffer[251] = Conversions.DAA_2_byte((byte)(((DateTime)vals[4]).Hour));
					buffer[250] = Conversions.DAA_2_byte((byte)(((DateTime)vals[4]).Minute));

					buffer[253] = Conversions.DAA_2_byte((byte)(((DateTime)vals[5]).Hour));
					buffer[252] = Conversions.DAA_2_byte((byte)(((DateTime)vals[5]).Minute));
					buffer[255] = Conversions.DAA_2_byte((byte)(((DateTime)vals[6]).Hour));
					buffer[254] = Conversions.DAA_2_byte((byte)(((DateTime)vals[6]).Minute));

					// recalcing crc block
					//ushort crc = RS232Lib.CommPort._calcCRC(Buffer, 
					//(ushort)(FM_System.Size * 2 - 2), 0, false);
					//Conversions.ushort_2_bytes(crc, ref Buffer, FM_System.Size * 2 - 2);

					errCode = (device as Em33TDevice).Write(DeviceIO.Memory.EMemory.FRAM,
											pageSystem.Address, pageSystem.Size, ref buffer);

					if (errCode == ExchangeResult.OK)
					{
						errCode = (device as Em33TDevice).Write(DeviceIO.Memory.EMemory.RAM,
											pageSystem.Address, pageSystem.Size, ref buffer);
					}
				}
				else if (device.DeviceType == EmDeviceType.EM32)
				{
					errCode = (device as Em32Device).WriteNominalsAndTimes(ref vals);
				}
				else if (device.DeviceType == EmDeviceType.ETPQP)
				{
					errCode = (device as EtPQPDevice).WriteNominalsAndTimes(ref vals);
				}

				if (errCode == ExchangeResult.OK)						// no errors
				{
					MessageBoxes.DeviceNominalsSaved(this);
					return;
				}
				else if (errCode == ExchangeResult.Write_Error)			// write error
				{
					MessageBoxes.DeviceReadError(this);
					return;
				}
				else if (errCode == ExchangeResult.Other_Error)			// serial port connection error
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
					return;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error Save Nominals:");
				throw;
			}
			finally
			{
				if (device != null) device.Close();
				this.Cursor = Cursors.Default;
			}
		}

		#endregion

		private void myDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{

			if (e.RowIndex == 1 || e.RowIndex == 2)
			{
				DataGridViewTextBoxCell cell_Ul = (DataGridViewTextBoxCell)dgvNominals.Rows[1].Cells[e.ColumnIndex];
				DataGridViewTextBoxCell cell_Uph = (DataGridViewTextBoxCell)dgvNominals.Rows[2].Cells[e.ColumnIndex];

				float f_Ul = 0.0F, f_Uph = 0.0F;

				if (e.RowIndex == 1)
				{
					if (float.TryParse((string)(cell_Ul.Value), out f_Ul))
					{
						f_Uph = f_Ul / 1.732051F;
						cell_Uph.Value = f_Uph.ToString("0.####");
					}
				}
				if (e.RowIndex == 2)
				{
					if (float.TryParse((string)(cell_Uph.Value), out f_Uph))
					{
						f_Ul = f_Uph * 1.732051F;
						cell_Ul.Value = f_Ul.ToString("0.####");
					}

				}
			}
		}
			 
	}
}