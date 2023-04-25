using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using System.IO.Ports;
using System.Xml.Serialization;
using System.Resources;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Net;
using System.Globalization;

using DeviceIO;
using NativeWifi;
using EmServiceLib;
using EmDataSaver;

namespace EnergomonitoringXP
{
	public partial class frmSettings : Form
	{
		/// <summary>
		/// <c>Settings</c> object to be serialized/deserialized
		/// </summary>
		private Settings settings_;
		private Form mainForm_;

		private string strDevicesDialUpFileName_ = "DevicesDialUp.xml";
		private string strDevicesEthernetFileName_ = "DevicesEthernet.xml";
		private string strDevicesGPRSFileName_ = "DevicesGPRS.xml";
		private string strDevices485FileName_ = "Devices485.xml";

		private object lockFileDevicesDialUp_ = new object();
		private object lockFileDevicesEthernet_ = new object();
		private object lockFileDevicesGPRS_ = new object();
		private object lockFileDevicesRs485_ = new object();

		private XmlSerializer xmlserDialUp_ = new XmlSerializer(typeof(AllDevicesAutoConnect));
		private XmlSerializer xmlserEthernet_ = new XmlSerializer(typeof(AllDevicesAutoConnect));
		private XmlSerializer xmlserGPRS_ = new XmlSerializer(typeof(AllDevicesAutoConnect));
		private XmlSerializer xmlser485_ = new XmlSerializer(typeof(AllDevicesAutoConnect));

		private BackgroundWorker bwAutoDetect_ = new BackgroundWorker();
		private AutoDetectorEm33T autoDetector_;
        private frmMessage frmAutoDetect_ = null;
        private int[] speeds_ = new int[] { 115200, 38400, 19200, 9600 };

		private bool bCanCloseWindow_ = true;
				
		public frmSettings(Settings settings, Form mainForm)
		{
			InitializeComponent();

			settings_ = settings.Clone();
			mainForm_ = mainForm;
		}

		private void frmSettings_Load(object sender, EventArgs e)
		{
			try
			{
				// DB servers
				settings_.LoadSettings();
				if (settings_.PgServers != null)
				{
					foreach (EmDataSaver.PgServerItem pgSrv in settings_.PgServers)
					{
						string server = pgSrv.PgServerName + "  " + pgSrv.PgHost + "  " +
									pgSrv.PgPort.ToString();
						cbServerDB.Items.Add(server);
					}
					if (cbServerDB.Items.Count > 0)
					{
						cbServerDB.SelectedIndex = settings_.CurServerIndex;
					}
					else settings_.CurServerIndex = -1;
				}
				else settings_.CurServerIndex = -1;

				try
				{
					OpenDevicesInfo();
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in frmSettings_Load(): OpenDevicesInfo: ");
					throw;
				}

				// common
				try
				{
					cmbLanguage.SelectedIndex = cmbLanguage.FindString(settings_.Language);
				}
				catch
				{
					cmbLanguage.SelectedIndex = -1;
				}

				try
				{
					cmbFloatSigns.SelectedIndex = cmbFloatSigns.FindString(settings_.FloatSigns.ToString());
                    float f = 12.3456F;
                    txtFloatFormatExample.Text = f.ToString(settings_.FloatFormat);
				}
				catch
				{
					cmbFloatSigns.SelectedIndex = -1;
				}

				rbA.Checked = settings_.CurrentRatio == 1;
				rbKA.Checked = settings_.CurrentRatio == 0.001F;
				rbV.Checked = settings_.VoltageRatio == 1;
				rbKV.Checked = settings_.VoltageRatio == 0.001F;
				rbW.Checked = settings_.PowerRatio == 1;
				rbKW.Checked = settings_.PowerRatio == 0.001F;

                if (settings_.ShowAvgTooltip) rbTooltipOn.Checked = true;
                else rbTooltipOff.Checked = true;

				chbNewFirmware.Checked = settings_.CheckFirmwareEtPQP_A;
				chbNewSoftware.Checked = settings_.CheckNewSoftwareVersion;

				chbAutoTimeWarn.Checked = settings_.DontWarnAutoSynchroTimeDisabled;

				// serial port
				try
				{
					cmbSerialPort.Items.Clear();
					cmbSerialPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
					cmbSerialPort.SelectedIndex = cmbSerialPort.FindStringExact(settings_.SerialPortName);
				}
				catch
				{
					cmbSerialPort.SelectedIndex = -1;
				}

				try
				{
					cmbSerialPortModem.Items.Clear();
					cmbSerialPortModem.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
					cmbSerialPortModem.SelectedIndex =
						cmbSerialPortModem.FindStringExact(settings_.SerialPortNameModem);
				}
				catch
				{
					cmbSerialPortModem.SelectedIndex = -1;
				}

				try
				{
					cmbSerialPort485.Items.Clear();
					cmbSerialPort485.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
					cmbSerialPort485.SelectedIndex =
						cmbSerialPort485.FindStringExact(settings_.SerialPortName485);
				}
				catch
				{
					cmbSerialPort485.SelectedIndex = -1;
				}

				try
				{
					cmbSerialPortSpeed.SelectedIndex = cmbSerialPortSpeed.FindStringExact(
						settings_.SerialPortSpeed.ToString());
				}
				catch
				{
					cmbSerialPortSpeed.SelectedIndex = -1;
				}

				try
				{
					cmbSerialSpeedModem.SelectedIndex = cmbSerialSpeedModem.FindStringExact(
						settings_.SerialSpeedModem.ToString());
				}
				catch
				{
					cmbSerialSpeedModem.SelectedIndex = -1;
				}

				try
				{
					cmbSerialSpeed485.SelectedIndex = cmbSerialSpeed485.FindStringExact(
						settings_.SerialSpeed485.ToString());
				}
				catch
				{
					cmbSerialSpeed485.SelectedIndex = -1;
				}

				try
				{
					switch (settings_.IOInterface)
					{
						case EmPortType.COM:
							rbtnCOM.Checked = true;
							cmbSerialPortSpeed.Enabled = true;
							EnableEthernetPage(false);
							EnableGPRSPage(false);
							EnableModemPage(false);
							EnableRs485Page(false);
							EnableWifiPage(false);
							break;
						case EmPortType.USB:
							rbtnUSB.Checked = true;
							EnableEthernetPage(false);
							EnableGPRSPage(false);
							EnableModemPage(false);
							EnableRs485Page(false);
							EnableWifiPage(false);
							break;
						case EmPortType.Modem:
							rbtnModemGSM.Checked = true;
							cmbSerialPortSpeed.Enabled = false;
							EnableEthernetPage(false);
							EnableGPRSPage(false);
							EnableModemPage(true);
							EnableRs485Page(false);
							EnableWifiPage(false);
							tcConnections.SelectedTab = tabGSM;
							break;
						case EmPortType.Ethernet:
							rbEthernet.Checked = true;
							EnableEthernetPage(true);
							EnableGPRSPage(false);
							EnableModemPage(false);
							EnableRs485Page(false);
							EnableWifiPage(false);
							tcConnections.SelectedTab = tabEthernet;
							break;
						case EmPortType.GPRS:
							rbtnModemGPRS.Checked = true;
							EnableEthernetPage(false);
							EnableGPRSPage(true);
							EnableModemPage(false);
							EnableRs485Page(false);
							EnableWifiPage(false);
							tcConnections.SelectedTab = tabGPRS;
							break;
						case EmPortType.Rs485:
							rbRs485.Checked = true;
							EnableEthernetPage(false);
							EnableGPRSPage(false);
							EnableModemPage(false);
							EnableRs485Page(true);
							EnableWifiPage(false);
							tcConnections.SelectedTab = tabRs485;
							break;
						case EmPortType.WI_FI:
							rbWifi.Checked = true;
							EnableEthernetPage(false);
							EnableGPRSPage(false);
							EnableModemPage(false);
							EnableRs485Page(false);
							EnableWifiPage(true);
							tcConnections.SelectedTab = tabWifi;
							break;
					}
				}
				catch
				{
					rbtnCOM.Checked = true;
				}

				// device type
				try
				{
					switch (settings_.CurDeviceType)
					{
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
						case EmDeviceType.NONE:
							rbEm33t.Checked = true;
							chbOptimizeInsert.Enabled = true;
							btnDetect.Enabled = true;
							break;
						case EmDeviceType.EM31K:
							rbEm31k.Checked = true;
							chbOptimizeInsert.Enabled = true;
							btnDetect.Enabled = false;
							break;
						case EmDeviceType.EM32:
							rbEm32.Checked = true;
							chbOptimizeInsert.Enabled = false;
							btnDetect.Enabled = false;
							break;
						case EmDeviceType.ETPQP:
							rbEtPQP.Checked = true;
							chbOptimizeInsert.Enabled = false;
							btnDetect.Enabled = false;
							// set 3000000 speed (do it AFTER filling cmbSerialPortSpeed!!!)
							cmbSerialPortSpeed.SelectedIndex = cmbSerialPortSpeed.FindString("3000000");
							cmbSerialPortSpeed.Enabled = false;
							break;
						case EmDeviceType.ETPQP_A:
							rbEtPQP_A.Checked = true;
							chbOptimizeInsert.Enabled = false;
							btnDetect.Enabled = false;
							break;
					}
				}
				catch
				{
					rbEm33t.Checked = true;
				}
				EnableDisableIOFieldsForDevType();

				// optimised insertion
				if (settings_.OptimisedInsertion)
					chbOptimizeInsert.Checked = true;
				else chbOptimizeInsert.Checked = false;

				// device manager
				if (settings_.Licences.LicencedDevices != null)
				{
					if (settings_.Licences.LicencedDevices.Length != 0)
						listBoxInstalled.Items.AddRange(settings_.Licences.LicencedDevices);
				}

				if (settings_.AvgBrushColor1 != 0)
				{
					pnlAvgBrushColor1.BackColor = Color.FromArgb(settings_.AvgBrushColor1);
				}

				if (settings_.AvgBrushColor2 != 0)
				{
					pnlAvgBrushColor2.BackColor = Color.FromArgb(settings_.AvgBrushColor2);
				}

				tbWifiPassword.Text = settings_.WifiPassword;
				tbCurWifiProfile.Text = settings_.CurWifiProfileName;

				btnApply.Enabled = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in frmSettings_Load():  ");
				throw;
			}
		}

		#region Common

		private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (settings_.Language == cmbLanguage.SelectedItem.ToString())
				{
					return;
				}

				if (cmbLanguage.SelectedIndex > -1)
				{
					settings_.Language = cmbLanguage.SelectedItem.ToString();
					if (settings_.SettingsChanged)
					{
						if (cmbLanguage.Items[cmbLanguage.SelectedIndex].ToString() == "English")
							MessageBox.Show("Interface language will be changed only after application restart", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
						if (cmbLanguage.Items[cmbLanguage.SelectedIndex].ToString() == "Русский")
							MessageBox.Show("Язык изменится только после перезапуска приложения", "К сведению", MessageBoxButtons.OK, MessageBoxIcon.Information);
						if (!btnApply.Enabled) btnApply.Enabled = true;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in cmbLanguage_SelectedIndexChanged(): " + ex.Message);
				throw;
			}
		}

		private void cmbFloatSigns_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (settings_.FloatSigns == Convert.ToInt32(cmbFloatSigns.SelectedItem.ToString()))
				{
					return;
				}

				if (cmbFloatSigns.SelectedIndex > -1)
				{
					settings_.FloatSigns = Convert.ToInt32(cmbFloatSigns.SelectedItem.ToString());
					float f = 12.3456F;
					txtFloatFormatExample.Text = f.ToString(settings_.FloatFormat);

					if (settings_.SettingsChanged)
					{
						if (this.settings_.CurrentLanguage == "en")
							MessageBox.Show("Resolution of displaying fractions will be changed\nonly after restart of the application.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
						else if (this.settings_.CurrentLanguage == "ru")
							MessageBox.Show("Точность отображения дробных величин изменится\nтолько после повторного открытия архива", "К сведению", MessageBoxButtons.OK, MessageBoxIcon.Information);
						if (!btnApply.Enabled) btnApply.Enabled = true;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Error in cmbFloatSigns_SelectedIndexChanged(): " + ex.Message);
				throw;
			}
		}

		private void RatiosRadioButtons_ANY_Click(object sender, EventArgs e)
		{
			switch ((sender as RadioButton).Name)
			{
				case "rbA":
					rbA.Checked = true;
					rbKA.Checked = false;
					break;
				case "rbKA":
					rbKA.Checked = true;
					rbA.Checked = false;
					break;
				case "rbV":
					rbV.Checked = true;
					rbKV.Checked = false;
					break;
				case "rbKV":
					rbKV.Checked = true;
					rbV.Checked = false;
					break;
				case "rbW":
					rbW.Checked = true;
					rbKW.Checked = false;
					break;
				case "rbKW":
					rbKW.Checked = true;
					rbW.Checked = false;
					break;
			}
		}

		private void RatiosRadioButtons_ANY_CheckedChanged(object sender, EventArgs e)
		{
			switch ((sender as RadioButton).Name)
			{
				case "rbA":
					if ((sender as RadioButton).Checked)
						this.settings_.CurrentRatio = 1;
					else
						this.settings_.CurrentRatio = 0.001F;
					break;
				case "rbV":
					if ((sender as RadioButton).Checked)
						this.settings_.VoltageRatio = 1;
					else
						this.settings_.VoltageRatio = 0.001F;
					break;
				case "rbW":
					if ((sender as RadioButton).Checked)
						this.settings_.PowerRatio = 1;
					else
						this.settings_.PowerRatio = 0.001F;
					break;
			}
			btnApply.Enabled = true;
		}

        private void rbTooltip_Click(object sender, EventArgs e)
        {
            settings_.ShowAvgTooltip = rbTooltipOn.Checked;

            btnApply.Enabled = true;
        }

		#region Brushes

		private void chkAvgBrushGradient_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (!chkAvgBrushGradient.Checked)
				{
					pnlAvgBrushExample.BackColor =
					pnlAvgBrushColor2.BackColor =
					pnlAvgBrushColor1.BackColor;

					settings_.AvgBrushColor1 = settings_.AvgBrushColor2 =
						pnlAvgBrushColor1.BackColor.ToArgb();
				}
				pnlAvgBrushExample.Invalidate(pnlAvgBrushExample.Region);

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Error in chkAvgBrushGradient_CheckedChanged(): " + ex.Message);
				throw;
			}
		}

		private void pnlAvgBrushColor1_Click(object sender, EventArgs e)
		{
			try
			{
				// showing color dialog window
				ColorDialog wndChooseColor = new ColorDialog();
				wndChooseColor.FullOpen = true;
				wndChooseColor.AnyColor = true;
				wndChooseColor.Color = pnlAvgBrushColor1.BackColor;
				// if user didn't click "ok" button returning
				if (wndChooseColor.ShowDialog() != DialogResult.OK) return;
				pnlAvgBrushColor1.BackColor = wndChooseColor.Color;
				settings_.AvgBrushColor1 = pnlAvgBrushColor1.BackColor.ToArgb();
				if (chkAvgBrushGradient.Checked)
				{
					pnlAvgBrushExample.Invalidate(pnlAvgBrushExample.Region);
				}

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in pnlAvgBrushColor1_Click(): " + ex.Message);
				throw;
			}
		}

		private void pnlAvgBrushColor2_Click(object sender, EventArgs e)
		{
			try
			{
				if (!chkAvgBrushGradient.Checked) return;
				// showing color dialog window
				ColorDialog wndChooseColor = new ColorDialog();
				wndChooseColor.FullOpen = true;
				wndChooseColor.AnyColor = true;
				wndChooseColor.Color = pnlAvgBrushColor2.BackColor;
				// if user didn't click "ok" button returning
				if (wndChooseColor.ShowDialog() != DialogResult.OK) return;
				pnlAvgBrushColor2.BackColor = wndChooseColor.Color;
				settings_.AvgBrushColor2 = pnlAvgBrushColor2.BackColor.ToArgb();
				pnlAvgBrushExample.Invalidate(pnlAvgBrushExample.Region);

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in pnlAvgBrushColor2_Click(): " + ex.Message);
				throw;
			}
		}

		private void pnlAvgBrushExample_Paint(object sender, PaintEventArgs e)
		{
			try
			{
				Brush brush;
				if (chkAvgBrushGradient.Checked)
				{
					brush = new LinearGradientBrush(new Rectangle(
						0, 0, pnlAvgBrushExample.Width, pnlAvgBrushExample.Height),
						Color.FromArgb(settings_.AvgBrushColor1),
						Color.FromArgb(settings_.AvgBrushColor2), 90);
				}
				else
				{
					brush = new SolidBrush(Color.FromArgb(settings_.AvgBrushColor1));
				}

				e.Graphics.FillRectangle(brush, 0, 0,
					pnlAvgBrushExample.Width, pnlAvgBrushExample.Height);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in pnlAvgBrushExample_Paint(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#endregion

		#region Auto Detect

		// функция рабоает только для Em33
		private void btnDetect_Click(object sender, EventArgs e)
		{
            if (MessageBoxes.OptionsAutoDetectPrompt(this) == DialogResult.Cancel) return;

			string[] serialPortsNames = SerialPort.GetPortNames();

			progressBar.Minimum = 0;
			progressBar.Maximum = serialPortsNames.Length + 1;
            progressBar.Value = 0;
			progressBar.Show();

			if (settings_.IOInterface == EmPortType.COM)
			{
				bwAutoDetect_ = new BackgroundWorker();
				bwAutoDetect_.WorkerReportsProgress = true;
				bwAutoDetect_.WorkerSupportsCancellation = true;
				bwAutoDetect_.DoWork += bwAutoDetect_DoWork;
				bwAutoDetect_.ProgressChanged += bwAutoDetect_ProgressChanged;
				bwAutoDetect_.RunWorkerCompleted += bwAutoDetect_RunWorkerCompleted;

				autoDetector_ = new AutoDetectorEm33T(serialPortsNames,
							speeds_, bwAutoDetect_, mainForm_.Handle);

                ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
                    this.GetType().Assembly);
                string msg = rm.GetString("msg_auto_detect_time");
                frmAutoDetect_ = frmMessage.Instance(msg, true, 10);
                frmAutoDetect_.Show();

				bwAutoDetect_.RunWorkerAsync();
				btnDetect.Enabled = false;
			}

			#region Old Code For Modem

			// автоопределение модема
			//else
			//if (settings.IOInterface == DeviceIO.Ports.EPort.Modem)
			//{
			//    //bool ok = false;
			//    //int i = 0;
			//    int res1, res2;
			//    string sPnpClass;
			//    string sPnpDevice;
			//    //int err = 0;
			//    //byte[] buffer = null;

			//    int y = 15;
			//    listRbtnModem.Clear();

			//    for (i = 0; i < _serialPortsNames.Length; i++)
			//    {
			//        progressBar.Increment(1);

			//        res1 = SerialPortModem.IsPortAvailable(_serialPortsNames[i]);

			//        if (res1 == 1)
			//        {
			//            res2 = SerialPortModem.GetPortPnpData(_serialPortsNames[i], out sPnpClass, out sPnpDevice);
			//            if (res2 == 1 && sPnpClass.ToUpper() == "MODEM")
			//            {
			//                ok = true;
			//                sPnpDevice += (" (" + _serialPortsNames[i] + ")");
			//                RadioButton rbTmp = new RadioButton();
			//                rbTmp.Click += new System.EventHandler(this.rbModem_Click);
			//                rbTmp.AutoSize = true;
			//                rbTmp.Text = sPnpDevice;
			//                rbTmp.Top = y;
			//                rbTmp.Left = 10;
			//                rbTmp.Parent = gbModems;
			//                rbTmp.Show();
			//                listRbtnModem.Add(rbTmp);

			//                y += 20;
			//            }
			//        }
			//    }
			//}

			#endregion
		}

		private void cmbSerialPort_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbSerialPort.SelectedIndex > -1)
			{
				settings_.SerialPortName = cmbSerialPort.SelectedItem.ToString();
				btnApply.Enabled = true;
			}
		}

		private void cmbSerialPortModem_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbSerialPortModem.SelectedIndex > -1)
			{
				settings_.SerialPortNameModem = cmbSerialPortModem.SelectedItem.ToString();
				btnApply.Enabled = true;
			}
		}

		private void cmbSerialPortSpeed_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbSerialPortSpeed.SelectedIndex > -1)
			{
				settings_.SerialPortSpeed = Convert.ToUInt32(cmbSerialPortSpeed.SelectedItem.ToString());
				btnApply.Enabled = true;
			}
		}

		private void rbtn_CheckedChanged(object sender, EventArgs e)
		{
			gbxSerial.Enabled = rbtnCOM.Checked;
			
			if (rbtnCOM.Checked)
			{
				settings_.IOInterface = EmPortType.COM;
				cmbSerialPortSpeed.Enabled = true;
				EnableEthernetPage(false);
				EnableGPRSPage(false);
				EnableModemPage(false);
				EnableRs485Page(false);
				EnableWifiPage(false);

				if (rbEtPQP.Checked)
				{
					// set 3000000 speed
					cmbSerialPortSpeed.SelectedIndex = cmbSerialPortSpeed.FindString("3000000");
					cmbSerialPortSpeed.Enabled = false;
				}
			}
			else if (rbtnUSB.Checked)
			{
				settings_.IOInterface = EmPortType.USB;
				EnableEthernetPage(false);
				EnableGPRSPage(false);
				EnableModemPage(false);
				EnableRs485Page(false);
				EnableWifiPage(false);
			}
			else if (rbtnModemGSM.Checked)
			{
				settings_.IOInterface = EmPortType.Modem;
				cmbSerialPortSpeed.Enabled = false;
				tcConnections.SelectedTab = tabGSM;
				EnableEthernetPage(false);
				EnableGPRSPage(false);
				EnableModemPage(true);
				EnableRs485Page(false);
				EnableWifiPage(false);
			}
			else if (rbEthernet.Checked)
			{
				settings_.IOInterface = EmPortType.Ethernet;
				tcConnections.SelectedTab = tabEthernet;
				EnableEthernetPage(true);
				EnableGPRSPage(false);
				EnableModemPage(false);
				EnableRs485Page(false);
				EnableWifiPage(false);
			}
			else if (rbtnModemGPRS.Checked)
			{
				settings_.IOInterface = EmPortType.GPRS;
				tcConnections.SelectedTab = tabGPRS;
				EnableEthernetPage(false);
				EnableGPRSPage(true);
				EnableModemPage(false);
				EnableRs485Page(false);
				EnableWifiPage(false);
			}
			else if (rbRs485.Checked)
			{
				settings_.IOInterface = EmPortType.Rs485;
				tcConnections.SelectedTab = tabRs485;
				EnableEthernetPage(false);
				EnableGPRSPage(false);
				EnableModemPage(false);
				EnableRs485Page(true);
				EnableWifiPage(false);
			}
			else if (rbWifi.Checked)
			{
				settings_.IOInterface = EmPortType.WI_FI;
				tcConnections.SelectedTab = tabWifi;
				EnableEthernetPage(false);
				EnableGPRSPage(false);
				EnableModemPage(false);
				EnableRs485Page(false);
				EnableWifiPage(true);
			}

			btnApply.Enabled = true;
		}

		private void EnableDisableIOFieldsForDevType()
		{
			if (rbEtPQP_A.Checked)
				rbEthernet.Text = "Internet/LAN";
			else rbEthernet.Text = "Ethernet";

			if (rbEm33t.Checked)
			{
				settings_.CurDeviceType = EmDeviceType.EM33T;
				chbOptimizeInsert.Enabled = true;
				btnDetect.Enabled = true;
				cmbSerialPortSpeed.Enabled = true;
                gbxInterface.Enabled = true;

				if(settings_.IOInterface != EmPortType.COM)
					rbtnUSB.Checked = true;
				else rbtnCOM.Checked = true;
				rbtnUSB.Enabled = true;
				rbtnCOM.Enabled = true;
				rbtnModemGPRS.Enabled = false;
				rbtnModemGSM.Enabled = false;
				rbRs485.Enabled = false;
				rbEthernet.Enabled = false;
				rbWifi.Enabled = false;
			}
			else if (rbEm31k.Checked)
			{
				settings_.CurDeviceType = EmDeviceType.EM31K;
				chbOptimizeInsert.Enabled = true;
				btnDetect.Enabled = false;
				cmbSerialPortSpeed.Enabled = true;
                gbxInterface.Enabled = true;

				if (settings_.IOInterface != EmPortType.COM)
					rbtnUSB.Checked = true;
				else rbtnCOM.Checked = true;
				rbtnCOM.Enabled = true;
				rbtnUSB.Enabled = true;
				rbtnModemGPRS.Enabled = false;
				rbtnModemGSM.Enabled = false;
				rbRs485.Enabled = false;
				rbEthernet.Enabled = false;
				rbWifi.Enabled = false;
			}
			else if (rbEm32.Checked)
			{
				settings_.CurDeviceType = EmDeviceType.EM32;
				chbOptimizeInsert.Enabled = false;
				btnDetect.Enabled = false;
				cmbSerialPortSpeed.Enabled = true;
                gbxInterface.Enabled = true;

				if(rbtnUSB.Checked) rbtnCOM.Checked = true;
				rbtnCOM.Enabled = true;
				rbtnUSB.Enabled = false;
				rbtnModemGPRS.Enabled = true;
				rbtnModemGSM.Enabled = true;
				rbRs485.Enabled = true;
				rbEthernet.Enabled = true;
				rbWifi.Enabled = false;

				// make visible columns for Ethernet DataGrid
				colAutoE.Visible = true;
				colSerialE.Visible = true;
				colStartE.Visible = true;
				colPort.Width = 40;
				colIPAddress.Width = 100;
			}
			else if (rbEtPQP.Checked)
			{
				settings_.CurDeviceType = EmDeviceType.ETPQP;
				chbOptimizeInsert.Enabled = false;
				btnDetect.Enabled = false;

				// set 3000000 speed
				cmbSerialPortSpeed.SelectedIndex = cmbSerialPortSpeed.FindString("3000000");
				cmbSerialPortSpeed.Enabled = false;
                rbtnCOM.Checked = true;
                gbxInterface.Enabled = false;
			}
			else if (rbEtPQP_A.Checked)
			{
				settings_.CurDeviceType = EmDeviceType.ETPQP_A;
				chbOptimizeInsert.Enabled = false;
                gbxSerial.Enabled = false;
                gbxInterface.Enabled = true;
				btnDetect.Enabled = false;
				cmbSerialPortSpeed.Enabled = false;

				if (rbtnCOM.Checked) rbtnUSB.Checked = true;
				rbtnCOM.Enabled = false;
				rbtnUSB.Enabled = true;
				rbtnModemGPRS.Enabled = false;
				rbtnModemGSM.Enabled = false;
				rbRs485.Enabled = false;
				rbEthernet.Enabled = true;
				rbWifi.Enabled = true;

				// make invisible columns for Ethernet DataGrid
				colAutoE.Visible = false;
				colSerialE.Visible = false;
				colStartE.Visible = false;
				colPort.Width = 80;
				colIPAddress.Width = 150;
			}
		}

		private void rbtnCurDev_CheckedChanged(object sender, EventArgs e)
		{
			EnableDisableIOFieldsForDevType();

			btnApply.Enabled = true;
		}

		#endregion

		#region Licences

		private void btnOpenLicenseFile_Click(object sender, EventArgs e)
		{
			try
			{
				if (dlgOpenLicence.ShowDialog() != DialogResult.OK) return;
				string[] dev = this.settings_.Licences.LoadLicences(dlgOpenLicence.FileNames);
				if (dev != null)
				{
					if (dev.Length != 0)
					{
						listBoxAvailable.Items.AddRange(dev);
						listBoxAvailable.Enabled = true;
						btnAddLicence.Enabled = true;
						listBoxAvailable.Focus();
						for (int i = 0; i < listBoxAvailable.Items.Count; i++)
							listBoxAvailable.SelectedIndices.Add(i);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnOpenLicenseFile_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnAddLicence_Click(object sender, EventArgs e)
		{
			try
			{
				if (listBoxAvailable.SelectedIndices.Count < 0) return;

				for (int i = 0; i < listBoxAvailable.SelectedIndices.Count; i++)
				{
					string str_num = listBoxAvailable.Items[listBoxAvailable.SelectedIndices[i]].ToString();
					listBoxInstalled.Items.Add(str_num);
					settings_.Licences.AddLiсence(Convert.ToInt64(str_num));
				}

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnAddLicence_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnDropLicence_Click(object sender, EventArgs e)
		{
			try
			{
				if (listBoxInstalled.SelectedIndex < 0) return;
				if (MessageBoxes.OptionsDropLicenceQuestion(this) != DialogResult.OK) return;

				settings_.Licences.DropLicence(Convert.ToInt64(listBoxInstalled.SelectedItem.ToString()));
				listBoxInstalled.Items.Remove(listBoxInstalled.SelectedItem);

                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnDropLicence_Click(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#region Buttons

		private void btnApply_Click(object sender, EventArgs e)
		{
			try
			{
				bCanCloseWindow_ = true;

				if (settings_.IOInterface == EmPortType.WI_FI &&
					string.IsNullOrEmpty(settings_.CurWifiProfileName))
				{
					if (settings_.CurrentLanguage == "ru")
						MessageBox.Show("Необходимо выбрать имя подключения Wi-Fi!");
					else MessageBox.Show("You must select Wi-Fi connection name!");
					bCanCloseWindow_ = false;
					return;
				}

				settings_.CurrentIPAddress = this.ActiveIPAddress;

				//settings_.CurWifiIPaddress = this.mtbIp.Text.Replace(" ", "");
				//settings_.CurWifiIPaddress = settings_.CurWifiIPaddress.Replace(',', '.');
				settings_.WifiPassword = tbWifiPassword.Text;

				settings_.CurrentPort = Int32.Parse(this.ActivePort);
				settings_.CurPhoneNumber = this.ActiveNumber;
				settings_.AttemptNumber = this.Attempts;
				if(settings_.IOInterface == EmPortType.Rs485) settings_.CurDeviceAddress = this.ActiveDevAddress485;
				if (settings_.IOInterface == EmPortType.Modem) settings_.CurDeviceAddress = this.ActiveDevAddressGSM;
				if (settings_.IOInterface == EmPortType.GPRS) settings_.CurDeviceAddress = this.ActiveDevAddressGPRS;

				settings_.SaveSettings();

				//dgvPhoneNumbers.Sort(dgvPhoneNumbers.Columns[2], ListSortDirection.Ascending);
				SaveDevicesInfo();

				btnApply.Enabled = false;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnApply_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			bCanCloseWindow_ = true;

			if (settings_.IOInterface == EmPortType.WI_FI &&
				string.IsNullOrEmpty(settings_.CurWifiProfileName))
			{
				if (settings_.CurrentLanguage == "ru") 
					MessageBox.Show("Необходимо выбрать имя подключения Wi-Fi!");
				else MessageBox.Show("You must select Wi-Fi connection name!");
				bCanCloseWindow_ = false;
				return;
			}

			if (btnApply.Enabled)
			{
				btnApply_Click(sender, e);
			}
		}

		#endregion	

		#region Properties

		public int Attempts
		{
			get { return (int)nudAttempts.Value; }
		}

		public List<EmDataSaver.AutoConnect.AutoQueueItemData> AllAutoDialData
		{
			get
			{
				try
				{
					OpenDevicesInfo();

					List<EmDataSaver.AutoConnect.AutoQueueItemData> list =
								new List<EmDataSaver.AutoConnect.AutoQueueItemData>();
					for (int i = 0; i < dgvPhoneNumbers.RowCount; ++i)
					{
						DateTime dt = (DateTime)dgvPhoneNumbers["colTime", i].Value;
						TimeSpan time = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
						bool auto = (bool)dgvPhoneNumbers["colAuto", i].Value;
						ushort address = UInt16.Parse(dgvPhoneNumbers["colSerial", i].Value.ToString());
						if (auto)
						{
							EmDataSaver.AutoConnect.AutoQueueItemData tmp =
								new EmDataSaver.AutoConnect.AutoQueueItemData(
								CorrectPhoneNumber((string)dgvPhoneNumbers["colPhone", i].Value),
								address,
								time,
								auto,
								Attempts,
								EmDataSaver.AutoConnect.AutoQueueItemType.GSM_MODEM);
							list.Add(tmp);
						}
					}
					return list;
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in AllAutoDialData 1");
					return null;
				}
			}
		}

		public List<EmDataSaver.AutoConnect.AutoQueueItemData> AllAutoEthernetData
		{
			get
			{
				try
				{
					OpenDevicesInfo();

					List<EmDataSaver.AutoConnect.AutoQueueItemData> list =
								new List<EmDataSaver.AutoConnect.AutoQueueItemData>();
					for (int i = 0; i < dgvEthernet.RowCount; ++i)
					{
						DateTime dt = (DateTime)dgvEthernet["colStartE", i].Value;
						TimeSpan time = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
						bool auto = (bool)dgvEthernet["colAutoE", i].Value;
						if (auto)
						{
							EmDataSaver.AutoConnect.AutoQueueItemData tmp =
								new EmDataSaver.AutoConnect.AutoQueueItemData(
								(string)dgvEthernet["colIPAddress", i].Value,
								(string)dgvEthernet["colPort", i].Value,
								time,
								auto,
								EmDataSaver.AutoConnect.AutoQueueItemType.ETHERNET);
							list.Add(tmp);
						}
					}
					return list;
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in AllAutoDialData 2");
					return null;
				}
			}
		}

		public List<EmDataSaver.AutoConnect.AutoQueueItemData> AllAutoGPRSData
		{
			get
			{
				try
				{
					OpenDevicesInfo();

					List<EmDataSaver.AutoConnect.AutoQueueItemData> list =
								new List<EmDataSaver.AutoConnect.AutoQueueItemData>();
					for (int i = 0; i < dgvGPRS.RowCount; ++i)
					{
						DateTime dt = (DateTime)dgvGPRS["colStartGPRS", i].Value;
						TimeSpan time = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
						bool auto = (bool)dgvGPRS["colAutoGPRS", i].Value;
						ushort address = UInt16.Parse(dgvPhoneNumbers["colSerial", i].Value.ToString());
						if (auto)
						{
							EmDataSaver.AutoConnect.AutoQueueItemData tmp =
								new EmDataSaver.AutoConnect.AutoQueueItemData(
								(string)dgvGPRS["colIPAddressGPRS", i].Value,
								address,
								(string)dgvGPRS["colPortGPRS", i].Value,
								time, auto,
								EmDataSaver.AutoConnect.AutoQueueItemType.GPRS);
							list.Add(tmp);
						}
					}
					return list;
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in AllAutoDialData 3");
					return null;
				}
			}
		}

		public List<EmDataSaver.AutoConnect.AutoQueueItemData> AllAuto485Data
		{
			get
			{
				try
				{
					OpenDevicesInfo();

					List<EmDataSaver.AutoConnect.AutoQueueItemData> list =
						new List<EmDataSaver.AutoConnect.AutoQueueItemData>();
					for (int i = 0; i < dgv485.RowCount; ++i)
					{
						DateTime dt = (DateTime)dgv485["colStart485", i].Value;
						TimeSpan time = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
						bool auto = (bool)dgv485["colAuto485", i].Value;
						if (auto)
						{
							EmDataSaver.AutoConnect.AutoQueueItemData tmp =
								new EmDataSaver.AutoConnect.AutoQueueItemData(
								UInt16.Parse(dgv485["colSerial485", i].Value.ToString()),
								time,
								auto,
								EmDataSaver.AutoConnect.AutoQueueItemType.RS485);
							list.Add(tmp);
						}
					}
					return list;
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in AllAutoDialData 4");
					return null;
				}
			}
		}

		public string ActiveNumber
		{
			get
			{
				string num = "";
				for (int i = 0; i < dgvPhoneNumbers.RowCount; ++i)
				{
					bool active = (bool)dgvPhoneNumbers["colActive", i].Value;
					if (active)
					{
						num = CorrectPhoneNumber((string)dgvPhoneNumbers["colPhone", i].Value);
						break;
					}
				}
				return num;
			}
		}

		public string ActiveIPAddress
		{
			get
			{
				string ip = "0.0.0.0";
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					for (int i = 0; i < dgvEthernet.RowCount; ++i)
					{
						bool active = (bool)dgvEthernet["colActiveE", i].Value;
						if (active)
						{
							ip = (string)dgvEthernet["colIPAddress", i].Value;
							break;
						}
					}
				}
				else if (settings_.IOInterface == EmPortType.GPRS)
				{
					for (int i = 0; i < dgvGPRS.RowCount; ++i)
					{
						bool active = (bool)dgvGPRS["colActiveGPRS", i].Value;
						if (active)
						{
							ip = (string)dgvGPRS["colIPAddressGPRS", i].Value;
							break;
						}
					}
				}
				return ip;
			}
		}

		public string ActivePort
		{
			get
			{
				string port = "0";
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					for (int i = 0; i < dgvEthernet.RowCount; ++i)
					{
						bool active = (bool)dgvEthernet["colActiveE", i].Value;
						if (active)
						{
							port = (string)dgvEthernet["colPort", i].Value;
							break;
						}
					}
				}
				else if (settings_.IOInterface == EmPortType.GPRS)
				{
					for (int i = 0; i < dgvGPRS.RowCount; ++i)
					{
						bool active = (bool)dgvGPRS["colActiveGPRS", i].Value;
						if (active)
						{
							port = (string)dgvGPRS["colPortGPRS", i].Value;
							break;
						}
					}
				}
				else if (settings_.IOInterface == EmPortType.WI_FI)
				{
					port = "2200";
				}
				return port;
			}
		}

		//public string ActiveIPAddressGPRS
		//{
		//    get
		//    {
		//        string ip = "0.0.0.0";
		//        for (int i = 0; i < dgvGPRS.RowCount; ++i)
		//        {
		//            bool active = (bool)dgvGPRS["colActiveGPRS", i].Value;
		//            if (active)
		//            {
		//                ip = (string)dgvGPRS["colIPAddressGPRS", i].Value;
		//                break;
		//            }
		//        }
		//        return ip;
		//    }
		//}

		//public string ActivePortGPRS
		//{
		//    get
		//    {
		//        string port = "0";
		//        for (int i = 0; i < dgvGPRS.RowCount; ++i)
		//        {
		//            bool active = (bool)dgvGPRS["colActiveGPRS", i].Value;
		//            if (active)
		//            {
		//                port = (string)dgvGPRS["colPortGPRS", i].Value;
		//                break;
		//            }
		//        }
		//        return port;
		//    }
		//}

		public ushort ActiveDevAddress485
		{
			get
			{
				ushort addr = 0;
				for (int i = 0; i < dgv485.RowCount; ++i)
				{
					bool active = (bool)dgv485["colActive485", i].Value;
					if (active)
					{
						addr = UInt16.Parse(dgv485["colSerial485", i].Value.ToString());
						break;
					}
				}
				return addr;
			}
		}

		public ushort ActiveDevAddressGSM
		{
			get
			{
				ushort addr = 0;
				for (int i = 0; i < dgvPhoneNumbers.RowCount; ++i)
				{
					bool active = (bool)dgvPhoneNumbers["colActive", i].Value;
					if (active)
					{
						addr = UInt16.Parse(dgvPhoneNumbers["colSerial", i].Value.ToString());
						break;
					}
				}
				return addr;
			}
		}

		public ushort ActiveDevAddressGPRS
		{
			get
			{
				ushort addr = 0;
				for (int i = 0; i < dgvGPRS.RowCount; ++i)
				{
					bool active = (bool)dgvGPRS["colActiveGPRS", i].Value;
					if (active)
					{
						addr = UInt16.Parse(dgvGPRS["colSerialGPRS", i].Value.ToString());
						break;
					}
				}
				return addr;
			}
		}

		#endregion

		#region Form Event Handlers

		private void chbAutoTimeWarn_CheckedChanged(object sender, EventArgs e)
		{
			settings_.DontWarnAutoSynchroTimeDisabled = chbAutoTimeWarn.Checked;
			btnApply.Enabled = true;
		}

		private void chbNewFirmware_CheckedChanged(object sender, EventArgs e)
		{
			settings_.CheckFirmwareEtPQP_A = chbNewFirmware.Checked;
			btnApply.Enabled = true;
		}

		private void chbNewSoftware_CheckedChanged(object sender, EventArgs e)
		{
			settings_.CheckNewSoftwareVersion = chbNewSoftware.Checked;
			btnApply.Enabled = true;
		}

		private void cbServerDB_SelectedIndexChanged(object sender, EventArgs e)
		{
			settings_.CurServerIndex = cbServerDB.SelectedIndex;
			btnApply.Enabled = true;
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			try
			{
				int index = dgvPhoneNumbers.Rows.Add();

				dgvPhoneNumbers["colPhone", index].Value = "8 777 777-77-77";
				dgvPhoneNumbers["colSerial", index].Value = "0";
				dgvPhoneNumbers["colTime", index].Value =
					CorrectDateTime(DateTimePicker.MinimumDateTime);
				dgvPhoneNumbers["colAuto", index].Value = false;
				dgvPhoneNumbers["colActive", index].Value = false;

                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnAdd_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (dgvPhoneNumbers.SelectedRows.Count > 0)
					dgvPhoneNumbers.Rows.RemoveAt(dgvPhoneNumbers.SelectedRows[0].Index);
                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnRemove_Click(): " + ex.Message);
				throw;
			}
		}

		private void dgvEthernet_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
                if ((sender as DataGridView).Rows.Count <= 0) return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				if (e.ColumnIndex == dr.Cells["colActiveE"].ColumnIndex)
				{
					for (int i = 0; i < dgvEthernet.Rows.Count; ++i)
					{
						DataGridViewRow row = dgvEthernet.Rows[i];
						if (!row.Selected)
						{
							DataGridViewCell cell = row.Cells["colActiveE"];
							cell.Value = false;
						}
					}
					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvEthernet_CellClick(): " + ex.Message);
				throw;
			}
		}

		private void dgvGPRS_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
                if ((sender as DataGridView).Rows.Count <= 0) return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				if (e.ColumnIndex == dr.Cells["colActiveGPRS"].ColumnIndex)
				{
					for (int i = 0; i < dgvGPRS.Rows.Count; ++i)
					{
						DataGridViewRow row = dgvGPRS.Rows[i];
						if (!row.Selected)
						{
							DataGridViewCell cell = row.Cells["colActiveGPRS"];
							cell.Value = false;
						}
					}
					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvGPRS_CellClick(): " + ex.Message);
				throw;
			}
		}

		private void dgvPhoneNumbers_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				if ((sender as DataGridView).EditingControl == null)
					return;
				if ((sender as DataGridView).Rows.Count <= 0)
					return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				int iPhoneIndex = dr.Cells["colPhone"].ColumnIndex;
				int iTimeIndex = dr.Cells["colTime"].ColumnIndex;
				int iAddressIndex = dr.Cells["colSerial"].ColumnIndex;

				if ((sender as DataGridView).CurrentCell.ColumnIndex == iAddressIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						Int64 value = Int64.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidSerial(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iPhoneIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						str = CorrectPhoneNumber(str);
						Int64 value = Int64.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidPhoneNumber(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iTimeIndex)
				{
					try
					{
						DateTime dt;
						DateTime.TryParse((sender as DataGridView).EditingControl.Text, out dt);
						dt = CorrectDateTime(dt);

						string str = String.Format("{0:D}:{1:D}", dt.Hour, dt.Minute);

						(sender as DataGridView).EditingControl.Text = str;

						//if (CheckIfTimeExists((sender as DataGridView), iTimeIndex,
						//            (sender as DataGridView).CurrentCell.RowIndex))
						//{
						//    MessageBox.Show("This time already exists!");
						//    e.Cancel = true;
						//}
					}
					catch
					{
						e.Cancel = true;
					}
				}

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvPhoneNumbers_CellValidating(): " + ex.Message);
				throw;
			}
		}

		private void dgvEthernet_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				if ((sender as DataGridView).Rows.Count <= 0)
					return;

				if ((sender as DataGridView).EditingControl == null)
					return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				int iTimeIndex = dr.Cells["colStartE"].ColumnIndex;
				int iAddressIndex = dr.Cells["colIPAddress"].ColumnIndex;
				int iPortIndex = dr.Cells["colPort"].ColumnIndex;

				if ((sender as DataGridView).CurrentCell.ColumnIndex == iAddressIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						IPAddress addr = IPAddress.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidIP(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iPortIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						Int64 value = Int64.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidPort(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iTimeIndex)
				{
					try
					{
						DateTime dt;
						DateTime.TryParse((sender as DataGridView).EditingControl.Text, out dt);
						dt = CorrectDateTime(dt);

						string str = String.Format("{0:D}:{1:D}", dt.Hour, dt.Minute);

						(sender as DataGridView).EditingControl.Text = str;
					}
					catch
					{
						e.Cancel = true;
					}
				}

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvEthernet_CellValidating(): " + ex.Message);
				throw;
			}
		}

		private void dgvGPRS_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				if ((sender as DataGridView).Rows.Count <= 0)
					return;

				if ((sender as DataGridView).EditingControl == null)
					return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				int iTimeIndex = dr.Cells["colStartGPRS"].ColumnIndex;
				int iIPAddressIndex = dr.Cells["colIPAddressGPRS"].ColumnIndex;
				int iPortIndex = dr.Cells["colPortGPRS"].ColumnIndex;
				int iAddressIndex = dr.Cells["colSerialGPRS"].ColumnIndex;

				if ((sender as DataGridView).CurrentCell.ColumnIndex == iAddressIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						Int64 value = Int64.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidSerial(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iIPAddressIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						IPAddress addr = IPAddress.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidIP(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iPortIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						Int64 value = Int64.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidPort(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iTimeIndex)
				{
					try
					{
						DateTime dt;
						DateTime.TryParse((sender as DataGridView).EditingControl.Text, out dt);
						dt = CorrectDateTime(dt);

						string str = String.Format("{0:D}:{1:D}", dt.Hour, dt.Minute);

						(sender as DataGridView).EditingControl.Text = str;
					}
					catch
					{
						e.Cancel = true;
					}
				}

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvGPRS_CellValidating(): " + ex.Message);
				throw;
			}
		}

		private void dgvPhoneNumbers_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if ((sender as DataGridView).Rows.Count <= 0) return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				if (e.ColumnIndex == dr.Cells["colActive"].ColumnIndex)
				{
					for (int i = 0; i < dgvPhoneNumbers.Rows.Count; ++i)
					{
						DataGridViewRow row = dgvPhoneNumbers.Rows[i];
						if (!row.Selected)
						{
							DataGridViewCell cell = row.Cells["colActive"];
							cell.Value = false;
						}
					}

					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvPhoneNumbers_CellClick(): " + ex.Message);
				throw;
			}
		}

		private void btnAddE_Click(object sender, EventArgs e)
		{
			try
			{
				int index = dgvEthernet.Rows.Add();

				dgvEthernet["colIPAddress", index].Value = "0.0.0.0";
				dgvEthernet["colPort", index].Value = "0";
				dgvEthernet["colSerialE", index].Value = "0";
				dgvEthernet["colStartE", index].Value = CorrectDateTime(DateTimePicker.MinimumDateTime);
				dgvEthernet["colAutoE", index].Value = false;
				dgvEthernet["colActiveE", index].Value = false;

                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnAddE_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnRemoveE_Click(object sender, EventArgs e)
		{
			try
			{
				if (dgvEthernet.SelectedRows.Count > 0)
					dgvEthernet.Rows.RemoveAt(dgvEthernet.SelectedRows[0].Index);
                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnRemoveE_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnAddGPRS_Click(object sender, EventArgs e)
		{
			try
			{
				int index = dgvGPRS.Rows.Add();

				dgvGPRS["colIPAddressGPRS", index].Value = "0.0.0.0";
				dgvGPRS["colPortGPRS", index].Value = "0";
				dgvGPRS["colSerialGPRS", index].Value = "0";
				dgvGPRS["colStartGPRS", index].Value = CorrectDateTime(DateTimePicker.MinimumDateTime);
				dgvGPRS["colAutoGPRS", index].Value = false;
				dgvGPRS["colActiveGPRS", index].Value = false;

                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnAddGPRS_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnRemoveGPRS_Click(object sender, EventArgs e)
		{
			try
			{
				if (dgvGPRS.SelectedRows.Count > 0)
					dgvGPRS.Rows.RemoveAt(dgvGPRS.SelectedRows[0].Index);
                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnRemoveGPRS_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnAdd485_Click(object sender, EventArgs e)
		{
			try
			{
				int index = dgv485.Rows.Add();

				dgv485["colSerial485", index].Value = "0";
				dgv485["colStart485", index].Value = CorrectDateTime(DateTimePicker.MinimumDateTime);
				dgv485["colAuto485", index].Value = false;
				dgv485["colActive485", index].Value = false;

                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnAdd485_Click(): " + ex.Message);
				throw;
			}
		}

		private void btnRemove485_Click(object sender, EventArgs e)
		{
			try
			{
				if (dgv485.SelectedRows.Count > 0)
					dgv485.Rows.RemoveAt(dgv485.SelectedRows[0].Index);
                btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnRemove485_Click(): " + ex.Message);
				throw;
			}
		}

		private void cmbSerialSpeedModem_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (cmbSerialSpeedModem.SelectedIndex > -1)
				{
					settings_.SerialSpeedModem = Convert.ToUInt32(cmbSerialSpeedModem.SelectedItem.ToString());
					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Error in cmbSerialSpeedModem_SelectedIndexChanged(): " + ex.Message);
				throw;
			}
		}

		private void cmbSerialSpeed485_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (cmbSerialSpeed485.SelectedIndex > -1)
				{
					settings_.SerialSpeed485 = Convert.ToUInt32(cmbSerialSpeed485.SelectedItem.ToString());
					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Error in cmbSerialSpeed485_SelectedIndexChanged(): " + ex.Message);
				throw;
			}
		}

		private void cmbSerialPort485_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (cmbSerialPort485.SelectedIndex > -1)
				{
					settings_.SerialPortName485 = cmbSerialPort485.SelectedItem.ToString();
					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Error in cmbSerialPort485_SelectedIndexChanged(): " + ex.Message);
				throw;
			}
		}

		private void dgv485_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				if ((sender as DataGridView).Rows.Count <= 0)
					return;

				if ((sender as DataGridView).EditingControl == null)
					return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				int iTimeIndex = dr.Cells["colStart485"].ColumnIndex;
				int iAddressIndex = dr.Cells["colSerial485"].ColumnIndex;

				if ((sender as DataGridView).CurrentCell.ColumnIndex == iAddressIndex)
				{
					try
					{
						string str = (sender as DataGridView).EditingControl.Text;
						Int64 value = Int64.Parse(str);
					}
					catch
					{
						MessageBoxes.InvalidSerial(this);
						e.Cancel = true;
					}
				}
				else if ((sender as DataGridView).CurrentCell.ColumnIndex == iTimeIndex)
				{
					try
					{
						DateTime dt;
						DateTime.TryParse((sender as DataGridView).EditingControl.Text, out dt);
						dt = CorrectDateTime(dt);

						string str = String.Format("{0:D}:{1:D}", dt.Hour, dt.Minute);

						(sender as DataGridView).EditingControl.Text = str;
					}
					catch
					{
						e.Cancel = true;
					}
				}

				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgv485_CellValidating(): " + ex.Message);
				throw;
			}
		}

		private void dgv485_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
                if ((sender as DataGridView).Rows.Count <= 0) return;

				DataGridViewRow dr = (sender as DataGridView).Rows[0];
				if (e.ColumnIndex == dr.Cells["colActive485"].ColumnIndex)
				{
					for (int i = 0; i < dgv485.Rows.Count; ++i)
					{
						DataGridViewRow row = dgv485.Rows[i];
						if (!row.Selected)
						{
							DataGridViewCell cell = row.Cells["colActive485"];
							cell.Value = false;
						}
					}
					btnApply.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgv485_CellClick(): " + ex.Message);
				throw;
			}
		}

		private void frmSettings_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!bCanCloseWindow_)
			{
				e.Cancel = true;
				bCanCloseWindow_ = true;
				return;
			}

            if (bwAutoDetect_ != null)
                if (bwAutoDetect_.IsBusy)
                    bwAutoDetect_.CancelAsync();

			if (CheckIfEqualTimesExist())
			{
				ResourceManager rm = 
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string msg = rm.GetString("msg_device_equal_auto_times");
				string cap = "Error";
				MessageBox.Show(mainForm_ != null ? mainForm_ as Form : null , 
					msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);

				e.Cancel = true;
			}
		}

		private void chbOptimizeInsert_CheckedChanged(object sender, EventArgs e)
		{
			if (chbOptimizeInsert.Checked) this.settings_.OptimisedInsertion = true;
			else this.settings_.OptimisedInsertion = false;
			btnApply.Enabled = true;
		}

		private void btnSetAvgParams_Click(object sender, EventArgs e)
		{
			EmDataSaver.SavingInterface.frmAvgParams frm =
				new EmDataSaver.SavingInterface.frmAvgParams(0, EmDeviceType.NONE, true);
			if (frm.ShowDialog() == DialogResult.OK)
				btnApply.Enabled = true;
		}

		#endregion

		#region Open And Save Info

		/// <summary>
		/// Save info from file
		/// </summary>
		public void SaveDevicesInfo()
		{
			try
			{
				dgvPhoneNumbers.EndEdit();

				StreamWriter sw = null;

				if (dgvPhoneNumbers.Rows.Count > 0)
				{
					DataGridViewRow dr = dgvPhoneNumbers.Rows[0];
					//dgvPhoneNumbers.Sort(dgvPhoneNumbers.Columns[dr.Cells["colTime"].ColumnIndex],
					//ListSortDirection.Ascending);

					AllDevicesAutoConnect AllDevicesM = new AllDevicesAutoConnect();

					foreach (DataGridViewRow row in dgvPhoneNumbers.Rows)
					{
						if (row.IsNewRow)
						{
							continue;
						}

						EmDataSaver.AutoConnect.AutoQueueItemData device =
							new EmDataSaver.AutoConnect.AutoQueueItemData();
						device.Active = (bool)row.Cells["colActive"].Value;
						device.PhoneNumber = (String)row.Cells["colPhone"].Value;
						device.DevAddress = UInt16.Parse(row.Cells["colSerial"].Value.ToString());
						device.dtStartTime = (DateTime)row.Cells["colTime"].Value;
						device.AutoDial = (bool)row.Cells["colAuto"].Value;
						device.DevType = (string)row.Cells["colDevType"].Value;
						device.Comment = (string)row.Cells["colComment"].Value;

						AllDevicesM.Devices.Add(device);
					}

					AllDevicesM.nAttempts = (int)nudAttempts.Value;

					string[] args = System.Environment.GetCommandLineArgs();

					lock (lockFileDevicesDialUp_)
					{
						try
						{
							sw = new StreamWriter(EmService.AppDirectory +
								strDevicesDialUpFileName_);
							xmlserDialUp_.Serialize(sw, AllDevicesM);
						}
						finally
						{
							if (sw != null) sw.Close();
						}
					}
				}
                else
                {
                    if (File.Exists(EmService.AppDirectory + strDevicesDialUpFileName_))
                        File.Delete(EmService.AppDirectory + strDevicesDialUpFileName_);
                }

				// ethernet
				dgvEthernet.EndEdit();
				if (dgvEthernet.Rows.Count > 0)
				{
					DataGridViewRow dr = dgvEthernet.Rows[0];
					//dgvEthernet.Sort(dgvEthernet.Columns[dr.Cells["colStartE"].ColumnIndex],
					//ListSortDirection.Ascending);

					AllDevicesAutoConnect AllDevicesE = new AllDevicesAutoConnect();

					foreach (DataGridViewRow row in dgvEthernet.Rows)
					{
						if (row.IsNewRow)
						{
							continue;
						}

						EmDataSaver.AutoConnect.AutoQueueItemData device =
							new EmDataSaver.AutoConnect.AutoQueueItemData();
						device.Active = (bool)row.Cells["colActiveE"].Value;
						device.IPAddress = (String)row.Cells["colIPAddress"].Value;
						device.Port = (String)row.Cells["colPort"].Value;
						try
						{
							device.DevAddress = (ushort)row.Cells["colSerialE"].Value;
						}
						catch { device.DevAddress = 0xFFFF; }
						device.dtStartTime = (DateTime)row.Cells["colStartE"].Value;
						device.AutoDial = (bool)row.Cells["colAutoE"].Value;
						device.DevType = (string)row.Cells["colDevTypeE"].Value;
						device.Comment = (string)row.Cells["colCommentE"].Value;

						AllDevicesE.Devices.Add(device);
					}

					string[] args = System.Environment.GetCommandLineArgs();

					lock (lockFileDevicesEthernet_)
					{
						try
						{
							sw = new StreamWriter(EmService.AppDirectory +
								strDevicesEthernetFileName_);
							xmlserEthernet_.Serialize(sw, AllDevicesE);
						}
						finally
						{
							if (sw != null) sw.Close();
						}
					}
				}
                else
                {
                    if (File.Exists(EmService.AppDirectory + strDevicesEthernetFileName_))
                        File.Delete(EmService.AppDirectory + strDevicesEthernetFileName_);
                }

				// GPRS
				dgvGPRS.EndEdit();
				if (dgvGPRS.Rows.Count > 0)
				{
					DataGridViewRow dr = dgvGPRS.Rows[0];

					AllDevicesAutoConnect AllDevicesGPRS = new AllDevicesAutoConnect();

					foreach (DataGridViewRow row in dgvGPRS.Rows)
					{
						if (row.IsNewRow)
						{
							continue;
						}

						EmDataSaver.AutoConnect.AutoQueueItemData device =
							new EmDataSaver.AutoConnect.AutoQueueItemData();
						device.Active = (bool)row.Cells["colActiveGPRS"].Value;
						device.IPAddress = (String)row.Cells["colIPAddressGPRS"].Value;
						device.Port = (String)row.Cells["colPortGPRS"].Value;
						device.DevAddress = UInt16.Parse(row.Cells["colSerialGPRS"].Value.ToString());
						device.dtStartTime = (DateTime)row.Cells["colStartGPRS"].Value;
						device.AutoDial = (bool)row.Cells["colAutoGPRS"].Value;
						device.DevType = (string)row.Cells["colDevTypeGPRS"].Value;
						device.Comment = (string)row.Cells["colCommentGPRS"].Value;

						AllDevicesGPRS.Devices.Add(device);
					}

					string[] args = System.Environment.GetCommandLineArgs();

					lock (lockFileDevicesGPRS_)
					{
						try
						{
							sw = new StreamWriter(EmService.AppDirectory +
								strDevicesGPRSFileName_);
							xmlserGPRS_.Serialize(sw, AllDevicesGPRS);
						}
						finally
						{
							if (sw != null) sw.Close();
						}
					}
				}
                else
                {
                    if (File.Exists(EmService.AppDirectory + strDevicesGPRSFileName_))
                        File.Delete(EmService.AppDirectory + strDevicesGPRSFileName_);
                }

				// rs-485
				dgv485.EndEdit();
				if (dgv485.Rows.Count > 0)
				{
					DataGridViewRow dr = dgv485.Rows[0];
					//dgv485.Sort(dgv485.Columns[dr.Cells["colStart485"].ColumnIndex],
					//ListSortDirection.Ascending);

					AllDevicesAutoConnect AllDevices_485 = new AllDevicesAutoConnect();

					foreach (DataGridViewRow row in dgv485.Rows)
					{
						if (row.IsNewRow)
						{
							continue;
						}

						EmDataSaver.AutoConnect.AutoQueueItemData device =
							new EmDataSaver.AutoConnect.AutoQueueItemData();
						device.Active = (bool)row.Cells["colActive485"].Value;
						device.DevAddress = UInt16.Parse(row.Cells["colSerial485"].Value.ToString());
						device.dtStartTime = (DateTime)row.Cells["colStart485"].Value;
						device.AutoDial = (bool)row.Cells["colAuto485"].Value;
						device.DevType = (string)row.Cells["colDevType485"].Value;
						device.Comment = (string)row.Cells["colComment485"].Value;

						AllDevices_485.Devices.Add(device);
					}

					string[] args = System.Environment.GetCommandLineArgs();

					lock (lockFileDevicesRs485_)
					{
						try
						{
							sw = new StreamWriter(EmService.AppDirectory +
								strDevices485FileName_);
							xmlser485_.Serialize(sw, AllDevices_485);
						}
						finally
						{
							if (sw != null) sw.Close();
						}
					}
				}
                else
                {
                    if (File.Exists(EmService.AppDirectory + strDevices485FileName_))
                        File.Delete(EmService.AppDirectory + strDevices485FileName_);
                }
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in SaveDevicesInfo(): " + ex.Message);
			}
		}

		/// <summary>
		/// Load info from file
		/// </summary>
		public void OpenDevicesInfo()
		{
			StreamReader sr = null;
			AllDevicesAutoConnect AllDevices;

			try
			{
				string path = EmService.AppDirectory + strDevicesDialUpFileName_;
				dgvPhoneNumbers.Rows.Clear();
				lock (lockFileDevicesDialUp_)
				{
					if (File.Exists(path))
					{
						try
						{
							sr = new StreamReader(path);
							AllDevices = (AllDevicesAutoConnect)xmlserDialUp_.Deserialize(sr);
						}
						finally
						{
							if (sr != null) sr.Close();
						}

						if (AllDevices != null)
						{
							foreach (EmDataSaver.AutoConnect.AutoQueueItemData device in AllDevices.Devices)
							{
								int index = dgvPhoneNumbers.Rows.Add();

								dgvPhoneNumbers["colActive", index].Value = device.Active;
								dgvPhoneNumbers["colPhone", index].Value = device.PhoneNumber;
								dgvPhoneNumbers["colSerial", index].Value = device.DevAddress;
								dgvPhoneNumbers["colTime", index].Value = device.dtStartTime;
								dgvPhoneNumbers["colAuto", index].Value = device.AutoDial;
								dgvPhoneNumbers["colDevType", index].Value = device.DevType;
								dgvPhoneNumbers["colComment", index].Value = device.Comment;
							}

							nudAttempts.Value = (Decimal)AllDevices.nAttempts;
						}
					}
				}
			}
			catch (Exception ex)
			{
				// to do: show message about failure
				EmService.WriteToLogFailed("Error in OpenDevicesInfo() 1: " + ex.Message);
			}

			// ethernet
			try
			{
				string path = EmService.AppDirectory + strDevicesEthernetFileName_;
				dgvEthernet.Rows.Clear();
				lock (lockFileDevicesEthernet_)
				{
					if (File.Exists(path))
					{
						try
						{
							sr = new StreamReader(path);
							AllDevices = (AllDevicesAutoConnect)xmlserEthernet_.Deserialize(sr);
						}
						finally
						{
							if (sr != null) sr.Close();
						}

						if (AllDevices != null)
						{
							foreach (EmDataSaver.AutoConnect.AutoQueueItemData device in AllDevices.Devices)
							{
								int index = dgvEthernet.Rows.Add();

								dgvEthernet["colActiveE", index].Value = device.Active;
								dgvEthernet["colIPAddress", index].Value = device.IPAddress;
								dgvEthernet["colPort", index].Value = device.Port;
								dgvEthernet["colSerialE", index].Value = device.DevAddress;
								dgvEthernet["colStartE", index].Value = device.dtStartTime;
								dgvEthernet["colAutoE", index].Value = device.AutoDial;
								dgvEthernet["colDevTypeE", index].Value = device.DevType;
								dgvEthernet["colCommentE", index].Value = device.Comment;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				// to do: show message about failure
				EmService.WriteToLogFailed("Error in OpenDevicesInfo() 2: " + ex.Message);
			}

			// GPRS
			try
			{
				string path = EmService.AppDirectory + strDevicesGPRSFileName_;
				dgvGPRS.Rows.Clear();
				lock (lockFileDevicesGPRS_)
				{
					if (File.Exists(path))
					{
						try
						{
							sr = new StreamReader(path);
							AllDevices = (AllDevicesAutoConnect)xmlserGPRS_.Deserialize(sr);
						}
						finally
						{
							if (sr != null) sr.Close();
						}

						if (AllDevices != null)
						{
							foreach (EmDataSaver.AutoConnect.AutoQueueItemData device in AllDevices.Devices)
							{
								int index = dgvGPRS.Rows.Add();

								dgvGPRS["colActiveGPRS", index].Value = device.Active;
								dgvGPRS["colIPAddressGPRS", index].Value = device.IPAddress;
								dgvGPRS["colPortGPRS", index].Value = device.Port;
								dgvGPRS["colSerialGPRS", index].Value = device.DevAddress;
								dgvGPRS["colStartGPRS", index].Value = device.dtStartTime;
								dgvGPRS["colAutoGPRS", index].Value = device.AutoDial;
								dgvGPRS["colDevTypeGPRS", index].Value = device.DevType;
								dgvGPRS["colCommentGPRS", index].Value = device.Comment;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				// to do: show message about failure
				EmService.WriteToLogFailed("Error in OpenDevicesInfo() 2: " + ex.Message);
			}

			// rs-485
			try
			{
				string path = EmService.AppDirectory + strDevices485FileName_;
				dgv485.Rows.Clear();
				lock (lockFileDevicesRs485_)
				{
					if (File.Exists(path))
					{
						try
						{
							sr = new StreamReader(path);
							AllDevices = (AllDevicesAutoConnect)xmlser485_.Deserialize(sr);
						}
						finally
						{
							if (sr != null) sr.Close();
						}

						if (AllDevices != null)
						{
							foreach (EmDataSaver.AutoConnect.AutoQueueItemData device in AllDevices.Devices)
							{
								int index = dgv485.Rows.Add();

								dgv485["colActive485", index].Value = device.Active;
								dgv485["colSerial485", index].Value = device.DevAddress;
								dgv485["colStart485", index].Value = device.dtStartTime;
								dgv485["colAuto485", index].Value = device.AutoDial;
								dgv485["colDevType485", index].Value = device.DevType;
								dgv485["colComment485", index].Value = device.Comment;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in OpenDevicesInfo() 3: " + ex.Message);
				// to do: show message about failure
			}
		}

		#endregion

		#region Other Public Methods

		/// <summary>
		/// Метод сохраняет серийный номер прибора в таблице
		/// </summary>
		/// <param name="sAddress">Номер телефона или IP-адрес</param>
		/// <param name="iAddress">Числовой адрес (для RS-485)</param>
		/// <param name="serNumber">Серийный номер</param>
		/// <param name="ioType">Тип интерфейса</param>
		public void SetSerialNumber(string sAddress, ushort iAddress, string serNumber, 
									EmPortType ioType)
		{
			try
			{
				switch (ioType)
				{
					//case EmPortType.Modem:
					//    for (int index = 0; index < dgvPhoneNumbers.Rows.Count; ++index)
					//    {
					//        if (CorrectPhoneNumber(
					//            (string)(dgvPhoneNumbers["colPhone", index].Value)) == sAddress)
					//        {
					//            dgvPhoneNumbers["colSerial", index].Value = serNumber;
					//            break;
					//        }
					//    }
					//    break;
					case EmPortType.Ethernet:
						for (int index = 0; index < dgvEthernet.Rows.Count; ++index)
						{
							if (((string)dgvEthernet["colIPAddress", index].Value) == sAddress)
							{
								dgvEthernet["colSerialE", index].Value = serNumber;
								break;
							}
						}
						break;
					//case EmPortType.GPRS:
					//    for (int index = 0; index < dgvGPRS.Rows.Count; ++index)
					//    {
					//        if (((string)dgvGPRS["colIPAddressGPRS", index].Value) == sAddress)
					//        {
					//            dgvGPRS["colSerialGPRS", index].Value = serNumber;
					//            break;
					//        }
					//    }
					//    break;
					//case EmPortType.Rs485:
					//    for (int index = 0; index < dgv485.Rows.Count; ++index)
					//    {
					//        if (((ushort)dgv485["colSerial485", index].Value) == iAddress)
					//        {
					//            dgv485["colSerial485", index].Value = serNumber;
					//            break;
					//        }
					//    }
					//    break;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in SetSerialNumber(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Метод сохраняет тип прибора в таблице
		/// </summary>
		/// <param name="sAddress">Номер телефона или IP-адрес</param>
		/// <param name="iAddress">Числовой адрес (для RS-485)</param>
		/// <param name="devType">Тип прибора</param>
		/// <param name="ioType">Тип интерфейса</param>
		public void SetDeviceType(string sAddress, ushort iAddress, string devType, EmPortType ioType)
		{
			try
			{
				switch (ioType)
				{
					case EmPortType.Modem:
						for (int index = 0; index < dgvPhoneNumbers.Rows.Count; ++index)
						{
							if (CorrectPhoneNumber((string)(dgvPhoneNumbers["colPhone", index].Value)) == sAddress)
							{
								dgvPhoneNumbers["colDevType", index].Value = devType;
								//break;
							}
						}
						break;
					case EmPortType.Ethernet:
						for (int index = 0; index < dgvEthernet.Rows.Count; ++index)
						{
							if (((string)dgvEthernet["colIPAddress", index].Value) == sAddress)
							{
								dgvEthernet["colDevTypeE", index].Value = devType;
								break;
							}
						}
						break;
					case EmPortType.GPRS:
						for (int index = 0; index < dgvGPRS.Rows.Count; ++index)
						{
							if (((string)dgvGPRS["colIPAddressGPRS", index].Value) == sAddress)
							{
								dgvGPRS["colDevTypeGPRS", index].Value = devType;
								//break;
							}
						}
						break;
					case EmPortType.Rs485:
						for (int index = 0; index < dgv485.Rows.Count; ++index)
						{
							if (UInt16.Parse(dgv485["colSerial485", index].Value.ToString()) == iAddress)
							{
								dgv485["colDevType485", index].Value = devType;
								//break;
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in SetDeviceType(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#region Service Methods

		private void bwAutoDetect_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (settings_.CurrentLanguage == "ru")
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
					Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
				}
				else
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
					Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
				}

				autoDetector_.Run(ref e);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwAutoDetect_DoWork():");
				throw;
			}
		}

		private void bwAutoDetect_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//if (e.ProgressPercentage <= progressBar.Maximum)
			//	progressBar.Value = e.ProgressPercentage;
			if (progressBar.Value < progressBar.Maximum)
				progressBar.Increment(1);
		}

		private void bwAutoDetect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
                if (frmAutoDetect_ != null)
                {
                    frmAutoDetect_.Close();
                    frmAutoDetect_ = null;
                }
				progressBar.Hide();
				btnDetect.Enabled = true;

				if (autoDetector_.PortNameIndex != -1
					&& autoDetector_.SpeedIndex != -1)
				{
					if (settings_.IOInterface == EmPortType.COM)
					{
						cmbSerialPort.SelectedIndex = autoDetector_.PortNameIndex;
                        cmbSerialPortSpeed.SelectedIndex =
                            cmbSerialPortSpeed.FindString(speeds_[autoDetector_.SpeedIndex].ToString());
						MessageBoxes.OptionsAutoDetectOk(this);
					}
					//if (settings.IOInterface == DeviceIO.Ports.EPort.Modem)
					//{
					//    gbModems.Visible = true;
					//}
				}
				else
				{
                    if(!e.Cancelled)
					    MessageBoxes.OptionsAutoDetectFailed(this);
                    //gbModems.Visible = false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwAutoDetect_RunWorkerCompleted():");
				throw;
			}
		}

		/// <summary>
		/// Make time 10 min interval
		/// </summary>
		private DateTime CorrectDateTime(DateTime time)
		{
			TimeSpan ts = new TimeSpan(0, time.Minute % 10, 0);
			return time.Subtract(ts);
		}

		private string CorrectPhoneNumber(string strPhone)
		{
			return strPhone.Replace(" ", "").Replace("-", "");
		}

		/// <summary>
		/// Метод проверяет нет ли одинаковых времен для автоматического считывания
		/// </summary>
		/// <returns></returns>
		private bool CheckIfEqualTimesExist()
		{
			try
			{
				DataGridViewRow drPh = null;
				DataGridViewRow drEth = null;
				DataGridViewRow drGPRS = null;
				DataGridViewRow dr485 = null;
				if (dgvPhoneNumbers.Rows.Count > 0)
					drPh = dgvPhoneNumbers.Rows[0];
				if (dgvEthernet.Rows.Count > 0) drEth = dgvEthernet.Rows[0];
				if (dgvGPRS.Rows.Count > 0) drGPRS = dgvGPRS.Rows[0];
				if (dgv485.Rows.Count > 0) dr485 = dgv485.Rows[0];

				DateTime dtTemp = DateTime.MinValue;
				bool bTemp;
				List<DateTime> listDates = new List<DateTime>();

				int iTimeIndexPh = -1;
				int iAutoIndexPh = -1;
				int iTimeIndexEth = -1;
				int iAutoIndexEth = -1;
				int iTimeIndexGPRS = -1;
				int iAutoIndexGPRS = -1;
				int iTimeIndex485 = -1;
				int iAutoIndex485 = -1;
				if (drPh != null)
				{
					iTimeIndexPh = drPh.Cells["colTime"].ColumnIndex;
					iAutoIndexPh = drPh.Cells["colAuto"].ColumnIndex;
				}
				if (drEth != null)
				{
					iTimeIndexEth = drEth.Cells["colStartE"].ColumnIndex;
					iAutoIndexEth = drEth.Cells["colAutoE"].ColumnIndex;
				}
				if (drGPRS != null)
				{
					iTimeIndexGPRS = drGPRS.Cells["colStartGPRS"].ColumnIndex;
					iAutoIndexGPRS = drGPRS.Cells["colAutoGPRS"].ColumnIndex;
				}
				if (dr485 != null)
				{
					iTimeIndex485 = dr485.Cells["colStart485"].ColumnIndex;
					iAutoIndex485 = dr485.Cells["colAuto485"].ColumnIndex;
				}

				for (int iRow = 0; iRow < dgvPhoneNumbers.Rows.Count; ++iRow)
				{
					dtTemp = (DateTime)dgvPhoneNumbers[iTimeIndexPh, iRow].Value;
					bTemp = (bool)dgvPhoneNumbers[iAutoIndexPh, iRow].Value;
					if (bTemp)
					{
						if (IfListContainsHourMinute(listDates, dtTemp))
							return true;
						listDates.Add(dtTemp);
					}
				}

				for (int iRow = 0; iRow < dgvEthernet.Rows.Count; ++iRow)
				{
					dtTemp = (DateTime)dgvEthernet[iTimeIndexEth, iRow].Value;
					bTemp = (bool)dgvEthernet[iAutoIndexEth, iRow].Value;
					if (bTemp)
					{
						if (IfListContainsHourMinute(listDates, dtTemp))
							return true;
						listDates.Add(dtTemp);
					}
				}

				for (int iRow = 0; iRow < dgvGPRS.Rows.Count; ++iRow)
				{
					dtTemp = (DateTime)dgvGPRS[iTimeIndexGPRS, iRow].Value;
					bTemp = (bool)dgvGPRS[iAutoIndexGPRS, iRow].Value;
					if (bTemp)
					{
						if (IfListContainsHourMinute(listDates, dtTemp))
							return true;
						listDates.Add(dtTemp);
					}
				}

				for (int iRow = 0; iRow < dgv485.Rows.Count; ++iRow)
				{
					dtTemp = (DateTime)dgv485[iTimeIndex485, iRow].Value;
					bTemp = (bool)dgv485[iAutoIndex485, iRow].Value;
					if (bTemp)
					{
						if (IfListContainsHourMinute(listDates, dtTemp))
							return true;
						listDates.Add(dtTemp);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CheckIfEqualTimesExist():");
			}
			return false;
		}

		private static bool IfListContainsHourMinute(List<DateTime> list, DateTime item)
		{
			foreach (DateTime d in list)
			{
				if (d.Hour == item.Hour && d.Minute == item.Minute)
					return true;
			}
			return false;
		}

		private void EnableModemPage(bool enable)
		{
			EnableDataGridView(dgvPhoneNumbers, enable);
			btnAdd.Enabled = enable;
			btnRemove.Enabled = enable;
			nudAttempts.Enabled = enable;
			cmbSerialPortModem.Enabled = enable;
			cmbSerialSpeedModem.Enabled = enable;
		}

		private void EnableEthernetPage(bool enable)
		{
			EnableDataGridView(dgvEthernet, enable);
			btnAddE.Enabled = enable;
			btnRemoveE.Enabled = enable;
		}

		private void EnableGPRSPage(bool enable)
		{
			EnableDataGridView(dgvGPRS, enable);
			btnAddGPRS.Enabled = enable;
			btnRemoveGPRS.Enabled = enable;
		}

		private void EnableRs485Page(bool enable)
		{
			EnableDataGridView(dgv485, enable);
			btnAdd485.Enabled = enable;
			btnRemove485.Enabled = enable;
			cmbSerialPort485.Enabled = enable;
			cmbSerialSpeed485.Enabled = enable;
		}

		private void EnableWifiPage(bool enable)
		{
			lvConnections.Enabled = enable;
			btnConnect.Enabled = enable;
			btnRefresh.Enabled = enable;
			//mtbIp.Enabled = enable;
			tbWifiPassword.Enabled = enable;
		}

		private void EnableDataGridView(DataGridView dgv, bool enable)
		{
			if (dgv.Rows.Count < 1) return;
			if (!enable)
			{
				//if (originCellColor_ == Color.Empty)
				//originCellColor_ = dgv[0, 0].Style.BackColor;

				for (int iRow = 0; iRow < dgv.Rows.Count; ++iRow)
				{
					for (int iCol = 0; iCol < dgv.Rows[iRow].Cells.Count; ++iCol)
					{
						dgv[iCol, iRow].Style.BackColor = Color.DarkGray;
					}
				}
				dgv.ReadOnly = true;
			}
			else
			{
				for (int iRow = 0; iRow < dgv.Rows.Count; ++iRow)
				{
					for (int iCol = 0; iCol < dgv.Rows[iRow].Cells.Count; ++iCol)
					{
						dgv[iCol, iRow].Style.BackColor = Color.Empty;
					}
				}
				dgv.ReadOnly = false;
			}
		}

		#endregion

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			try
			{
				lvConnections.Items.Clear();

				WlanClient client = new WlanClient();

				foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
				{
					Wlan.WlanAvailableNetwork[] wlanBssEntries = wlanIface.GetAvailableNetworkList(0);

					foreach (Wlan.WlanAvailableNetwork network in wlanBssEntries)
					{
						string curProfileName = network.profileName;
						if (curProfileName == string.Empty)
							curProfileName = System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).Trim((char)0);

						if (!curProfileName.Contains("etpqp"))
						{
							//EmService.WriteToLogDebug("WIFI:  " + curProfileName);
							continue;
						}

						int space = curProfileName.IndexOf(' ');
						if (space > 0)
							curProfileName = curProfileName.Substring(0, space);

						bool bItemExisis = false;
						foreach (ListViewItem curItem in lvConnections.Items)
						{
							if (curItem.Text == curProfileName) { bItemExisis = true; break; }
						}
						if (bItemExisis) continue;

						// создаём экземпляр элемента листвью
						ListViewItem listItemWiFi = new ListViewItem();
						// назначаем ему имя нашей первой найденой сети, в конце убираем нулевые символы
						//listItemWiFi.Text = System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).Trim((char)0);
						listItemWiFi.Text = curProfileName;

						// узнаеём дополнительную информацию о сети и так же добавляем в созданый итем.
						listItemWiFi.SubItems.Add(network.wlanSignalQuality.ToString() + "%"); // качество связи в процентах

						//добавляем скомпанованый элемент непосредственно в листвью
						lvConnections.Items.Add(listItemWiFi);
					}
				}

				// если подключение только одно, то его и используем
				if (lvConnections.Items.Count == 1)
				{
					if (settings_.CurWifiProfileName != lvConnections.Items[0].Text)
					{
						settings_.CurWifiProfileName = lvConnections.Items[0].Text;
						tbCurWifiProfile.Text = settings_.CurWifiProfileName;
						btnApply.Enabled = true;
					}
				}
				else if (lvConnections.Items.Count == 0)
				{
					if (settings_.CurrentLanguage == "ru") MessageBox.Show("Не найдено ни одной точки доступа!");
					else MessageBox.Show("No connection available!");
					settings_.CurWifiProfileName = string.Empty;
					tbCurWifiProfile.Text = settings_.CurWifiProfileName;
					btnApply.Enabled = true;
				}
				else if (lvConnections.Items.Count > 1)
				{
					// пытаемся найти прошлое имя подключения. если его нет среди существующих, обнуляем имя
					if (!string.IsNullOrEmpty(settings_.CurWifiProfileName))
					{
						bool found = false;
						foreach (ListViewItem lvi in lvConnections.Items)
						{
							if (lvi.Text == settings_.CurWifiProfileName)
							{
								found = true;
								break;
							}
						}

						if (!found)
						{
							settings_.CurWifiProfileName = string.Empty;
							tbCurWifiProfile.Text = settings_.CurWifiProfileName;
							btnApply.Enabled = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in btnRefresh_Click(WiFi)():");
				if (settings_.CurrentLanguage == "ru") MessageBox.Show("Ошибка при получении списка подключений!");
				else MessageBox.Show("Unable to get connection list!");
			}
		}

		private bool IsWifiConnected(bool wait)
		{
			try
			{
				WlanClient client = new WlanClient();
				foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
				{
					if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Connected)
						return true;
					else if (!wait) return false;
					else
					{
						EmService.WriteToLogDebug(wlanIface.InterfaceName);
						EmService.WriteToLogDebug(wlanIface.InterfaceState.ToString());
						Thread.Sleep(5000);
						if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Connected)
							return true;
						else
						{
							EmService.WriteToLogDebug(wlanIface.InterfaceName);
							EmService.WriteToLogDebug(wlanIface.InterfaceState.ToString());
							Thread.Sleep(15000);
							if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Connected)
								return true;
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in IsWifiConnected():");
				return false;
			}
		}

		private void mtbIp_TextChanged(object sender, EventArgs e)
		{
			btnApply.Enabled = true;
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				if (tbCurWifiProfile.Text.Length < 1)
				{
					if (settings_.CurrentLanguage == "ru") MessageBox.Show("Не выбрано устройство для подключения!");
					else MessageBox.Show("You have to select the device to connect!");
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				string profileName = lvConnections.SelectedItems[0].Text;

				WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(profileName, tbWifiPassword.Text);

				if (!Wlan.IsWifiConnected(true, wlanIface, profileName))
				{
					if (settings_.CurrentLanguage == "ru") MessageBox.Show("Ошибка подключения!");
					else MessageBox.Show("Unable to connect!");
				}
				else
				{
					if (settings_.CurrentLanguage == "ru") MessageBox.Show("Подключение выполнено успешно!");
					else MessageBox.Show("Connection successful!");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in btnConnect_Click(WiFi)():");
				if (settings_.CurrentLanguage == "ru") MessageBox.Show("Ошибка подключения!");
				else MessageBox.Show("Unable to connect!");
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void tbWifiPassword_TextChanged(object sender, EventArgs e)
		{
			btnApply.Enabled = true;
		}

		private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				settings_.CurWifiProfileName = lvConnections.SelectedItems[0].Text;
				tbCurWifiProfile.Text = settings_.CurWifiProfileName;
				btnApply.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in lvConnections_SelectedIndexChanged():");
				settings_.CurWifiProfileName = string.Empty;
				tbCurWifiProfile.Text = settings_.CurWifiProfileName;
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			bCanCloseWindow_ = true;
		}
	}

	/// <summary>
	/// Info about all devices for automatic connection
	/// </summary>
	public class AllDevicesAutoConnect
	{
		/// <summary>devices</summary>
		public List<EmDataSaver.AutoConnect.AutoQueueItemData> Devices =
			new List<EmDataSaver.AutoConnect.AutoQueueItemData>();

		/// <summary>Number of attempts</summary>
		public int nAttempts = 1;
	}

	public class AutoDetectorEm33T
	{
		private DoWorkEventArgs e_;
		private BackgroundWorker bw_ = null;
		private Em33TDevice device_ = null;
		private string[] portNames_ = null;
		private int[] speeds_ = null;

		public int PortNameIndex = -1;
		public int SpeedIndex = -1;

		private IntPtr hMainWnd_;

		public AutoDetectorEm33T(string[] portNames, int[] speeds, BackgroundWorker bw, IntPtr hMainWnd)
		{
			portNames_ = portNames;
			speeds_ = speeds;
			bw_ = bw;
			hMainWnd_ = hMainWnd;
		}

		public void Run(ref DoWorkEventArgs e)
		{
			try
			{
				e_ = e;

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					e_.Result = false;
					return;
				}

				long serial = -1;

				for (int i = 0; i < portNames_.Length; i++)
				{
					for (int j = 0; j < speeds_.Length; j++)
					{
						object[] port_params = new object[2];
						port_params[0] = portNames_[i]; ;
						port_params[1] = speeds_[j];

						device_ = new Em33TDevice(EmPortType.COM, 
													/*settings_.CurDeviceAddress*/ 0xFFFF,
													false, port_params, 
													/*(sender_ as Form).Handle*/ IntPtr.Zero);
						serial = device_.OpenDevice();

						//DeviceIO.Memory.EM33TPSystem pageSystem = new DeviceIO.Memory.EM33TPSystem();

						//object[] port_params = new object[2];
						//port_params[0] = serialPortsNames[i];
						//port_params[1] = speed[j];

						//device_ = new Em33TDevice(EmPortType.COM, 
						//                    0xFFFF,  // device address
						//                    false,
						//                    port_params, (this as Form).Handle);

						//Int64 serial = device_.Open();
						//if (serial == -1) continue;

						//err = device_.Read(DeviceIO.Memory.EMemory.FRAM,
						//                pageSystem.Address, pageSystem.Size, ref buffer, false);

						//if (device_ != null) device_.ClosePort();

						if (serial != -1)
						{
							e_.Result = true;
							PortNameIndex = i;
							SpeedIndex = j;
							return;
						}

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							e_.Result = false;
							return;
						}
					}
					bw_.ReportProgress(1);
				}

				if (serial == -1)
				{
					e_.Result = false;
					return;
				}
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in AutoDetectorEm33T::Run(): " + emx.Message);
				e_.Result = false;
				return;
			}
			catch (EmDisconnectException)
			{
                //if (!e_.Cancel && !bw_.CancellationPending)
                //{
                //    e_.Cancel = true;
                //}
				Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
                e_.Result = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AutoDetectorEm33T::Run():");
				e_.Result = false;
				throw;
			}
			finally
			{
				if (device_ != null) device_.Close();

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
				}
			}
		}
	}
}	