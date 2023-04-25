using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Text;

using DeviceIO;
using EmWorkNetLicences;
using EmServiceLib;

namespace EmDataSaver
{
    /// <summary>
    /// PostgreSQL server record class
    /// </summary>
    [Serializable]    
    public class PgServerItem
    {
        [NonSerialized]
        private int _port = 0;
        [NonSerialized]
        private string _host = string.Empty;
        [NonSerialized]
        private string _server_name = string.Empty;

        /// <summary>
        /// Gets or sets connection port to PostgreSQL server
        /// </summary>
        public int PgPort
        {
            get { return this._port; }
            set { if (value > 0 && value < 65536 && value != this._port) { this._port = value; } }
        }

        /// <summary>
        /// Gets or sets PostgreSQL server host
        /// </summary>
        public string PgHost
        {
            get { return this._host; }
            set { if (value != this._host) { this._host = value; } }
        }

        /// <summary>
        /// Gets or sets PostgreSQL server name
        /// </summary>
        public string PgServerName
        {
            get { return _server_name; }
            set { if (value != this._server_name) _server_name = value; }
        }

        /// <summary>
        /// Gets PostgreSQL server connection string
        /// </summary>
		public string PgConnectionStringSystem
        {
			get { return String.Format("SERVER={0};Port={1};DATABASE=postgres;USER ID=energomonitor;PASSWORD=4emworknet4;Encoding=UNICODE", _host, _port); }
        }

        public string PgConnectionStringEm33
        {
            get { return String.Format("SERVER={0};Port={1};DATABASE=em_db;USER ID=energomonitor;PASSWORD=4emworknet4;Encoding=UNICODE", _host, _port); }
        }

		public string PgConnectionStringEm32
		{
			get { return String.Format("SERVER={0};Port={1};DATABASE=em32_db;USER ID=energomonitor;PASSWORD=4emworknet4;Encoding=UNICODE", _host, _port); }
		}

		public string PgConnectionStringEtPQP
		{
			get { return String.Format("SERVER={0};Port={1};DATABASE=et33_db;USER ID=energomonitor;PASSWORD=4emworknet4;Encoding=UNICODE", _host, _port); }
		}

		public string PgConnectionStringEtPQP_A
		{
			get { return String.Format("SERVER={0};Port={1};DATABASE=etpqp_a_db;USER ID=energomonitor;PASSWORD=4emworknet4;Encoding=UNICODE", _host, _port); }
		}
    }

	/// <summary>Contains settings of the application</summary>
    public class Settings
	{
		#region Fields

		/// <summary>Synchronization state</summary>
		[NonSerialized]
		private bool settingsChanged_ = false;

		/// <summary>Settings file name</summary>
		[NonSerialized]
		private string settingsFileName_ = EmService.AppDirectory + "EnergomonitoringXP.config";

		/// <summary>Current language</summary>
		[XmlIgnore]
		public string CurrentLanguage;

		/// <summary>Application interface language</summary>
		[NonSerialized]
		private string language_ = String.Empty;

		/// <summary>Float signs in all tables</summary>
		[NonSerialized]
		private int floatSigns_ = 0;

        /// <summary>Float signs in all tables</summary>
        [NonSerialized]
        private bool bShowAvgTooltip_ = false;

		/// <summary>Float signs in all tables</summary>
		[NonSerialized]
		private string floatFormat_ = string.Empty;

		/// <summary>Serial port name</summary>
		[NonSerialized]
		private string serialPortName_ = String.Empty;

		/// <summary>Serial port name for modem</summary>
		[NonSerialized]
		private string serialPortNameModem_ = String.Empty;

		/// <summary>Serial port name for modem</summary>
		[NonSerialized]
		private string serialPortName485_ = String.Empty;

		/// <summary>Serial port speed</summary>
		[NonSerialized]
		private uint serialPortSpeed_ = 0;

		/// <summary>Serial port speed modem</summary>
		[NonSerialized]
		private uint serialSpeedModem_ = 0;

		/// <summary>Serial port speed rs-485</summary>
		[NonSerialized]
		private uint serialSpeed485_ = 0;

		/// <summary>IO Interface: 0 - COM; 1 - USB; 2 - Modem</summary>
		[NonSerialized]
		private EmPortType IOInterface_ = EmPortType.COM;

		/// <summary>List of PostgreSQL database servers</summary>
		[NonSerialized]
		private PgServerItem[] pgServers_ = null;

		/// <summary>All registred devices hashs</summary>
		[NonSerialized]
		private Licences licences_ = new EmWorkNetLicences.Licences();

		/// <summary>1 or 0,001 (for A or kA)</summary>
		[NonSerialized]
		private float currentRatio_;

		/// <summary>1 or 0,001 (for V or kV)</summary>
		[NonSerialized]
		private float voltageRatio_;

		/// <summary>1 or 0,001 (for W or kW)</summary>
		[NonSerialized]
		private float powerRatio_;

		/// <summary>Averaged values graph's background color 1</summary>
		[NonSerialized]
		private Color avgBrushColor1_;

		/// <summary>Averaged values graph's background color 2</summary>
		[NonSerialized]
		private Color avgBrushColor2_;

		/// <summary>Current device address (only for Em32)</summary>
		[NonSerialized]
		private ushort curDeviceAddress_;

		//// <summary>IP address for wi-fi</summary>
		//[NonSerialized]
		//private string curWifiIPaddress_ = string.Empty;

		/// <summary>IP address for wi-fi</summary>
		[NonSerialized]
		private string curWifiProfileName_ = string.Empty;

		/// <summary>Password for wi-fi</summary>
		[NonSerialized]
		private string wifiPassword_ = string.Empty;

		/// <summary>Only for modem</summary>
		[NonSerialized]
		private string curPhoneNumber_;

		/// <summary>Only for ethernet and GPRS</summary>
		[NonSerialized] 
		private int curPort_ = 2200;

		/// <summary>Only for ethernet and GPRS</summary>
		[NonSerialized]
		private string curIPAddress_;

        /// <summary>Only for modem</summary>
        [NonSerialized]
		private int numAttempts_;

		/// <summary>Current DB Server</summary>
		[NonSerialized]
		private int curServerIndex_;

		/// <summary>Current Device Type</summary>
		[NonSerialized]
		private EmDeviceType curDeviceType_ = EmDeviceType.NONE;

		/// <summary>Method to insert to DB</summary>
		[NonSerialized]
		private bool optimisedInsert_;

		/// <summary>If need to check new firmware for EtPQP-A</summary>
		[NonSerialized]
		private bool checkFirmwareEtPQP_A_;

		/// <summary>If need to check new software version</summary>
		[NonSerialized]
		private bool checkNewSoftwareVersion_;

		/// <summary>If need to warn that auto time synchronization is disabled</summary>
		[NonSerialized]
		private bool dontWarnAutoSynchroTimeDisabled_;

		#endregion

		#region Properties

		/// <summary>If need to warn that auto time synchronization is disabled</summary>
		public bool DontWarnAutoSynchroTimeDisabled
		{
			get { return dontWarnAutoSynchroTimeDisabled_; }
			set { dontWarnAutoSynchroTimeDisabled_ = value; }
		}

		/// <summary>If need to check new firmware for EtPQP-A</summary>
		public bool CheckFirmwareEtPQP_A
		{
			get { return checkFirmwareEtPQP_A_; }
			set { checkFirmwareEtPQP_A_ = value; }
		}

		/// <summary>If need to check new software version</summary>
		public bool CheckNewSoftwareVersion
		{
			get { return checkNewSoftwareVersion_; }
			set { checkNewSoftwareVersion_ = value; }
		}

		/// <summary>Gets settings state</summary>
		public bool SettingsChanged
		{
			get { return settingsChanged_; }
		}

		/// <summary>Gets settings file name</summary>
		public string SettingsFileName
		{
			get { return settingsFileName_; }
		}
		
		/// <summary>Gets or sets application interface language</summary>
		public string Language
		{
			get { return language_; }
			set
			{
				if (value != language_)
				{
					language_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Float signs in all tables</summary>
		public int FloatSigns
		{
			get { return floatSigns_; }
			set 
			{
				if (floatSigns_ != value)
				{
					floatSigns_ = value;
					settingsChanged_ = true;
				}
			}
		}

        /// <summary>Show Tooltips for AVG params</summary>
        public bool ShowAvgTooltip
        {
            get { return bShowAvgTooltip_; }
            set
            {
                if (bShowAvgTooltip_ != value)
                {
                    bShowAvgTooltip_ = value;
                    settingsChanged_ = true;
                }
            }
        }

		/// <summary>Gets string float format</summary>
		public string FloatFormat
		{
			get
			{
				StringBuilder sb = new StringBuilder("0");
				if (floatSigns_ > 0)
				{
					sb.Append(".");
					for (int i = 0; i < floatSigns_; i++)
					{
						sb.Append("0");
					}
				}
				return sb.ToString();
			}
		}

		/// <summary>Gets or sets serial port name</summary>
		public string SerialPortName
		{
			get { return serialPortName_; }
			set
			{
				if (value != serialPortName_)
				{
					serialPortName_ = value;
					settingsChanged_ = true;
				}
			}	
		}

		/// <summary>Gets or sets serial port name</summary>
		public string SerialPortName485
		{
			get { return serialPortName485_; }
			set
			{
				if (value != serialPortName485_)
				{
					serialPortName485_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Gets or sets serial port name</summary>
		public string SerialPortNameModem
		{
			get { return serialPortNameModem_; }
			set
			{
				if (value != serialPortNameModem_)
				{
					serialPortNameModem_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Gets or sets serial port speed</summary>
		public uint SerialPortSpeed
		{
			get { return serialPortSpeed_; }
			set
			{
				if (value != serialPortSpeed_)
				{
					serialPortSpeed_ = value;
					settingsChanged_ = true;
				}
			}
		}

		public uint SerialSpeedModem
		{
			get { return serialSpeedModem_; }
			set
			{
				if (value != serialSpeedModem_)
				{
					serialSpeedModem_ = value;
					settingsChanged_ = true;
				}
			}
		}

		public uint SerialSpeed485
		{
			get { return serialSpeed485_; }
			set
			{
				if (value != serialSpeed485_)
				{
					serialSpeed485_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Gets or sets I/O Interface</summary>
		public EmPortType IOInterface
		{
			get { return IOInterface_; }
			set { IOInterface_ = value; }
		}

		/// <summary>Gets or sets Current device type</summary>
		public EmDeviceType CurDeviceType
		{
			get { return curDeviceType_; }
			set { curDeviceType_ = value; }
		}

		/// <summary>Gets or sets Method to insert to DB</summary>
		public bool OptimisedInsertion
		{
			get { return optimisedInsert_; }
			set { optimisedInsert_ = value; }
		}

		/// <summary>Current device address (only for Em32)</summary>		
		public ushort CurDeviceAddress
		{
			get { return curDeviceAddress_; }
			set { curDeviceAddress_ = value; }
		}

		/// <summary>Current DB Server</summary>		
		public int CurServerIndex
		{
			get { return curServerIndex_; }
			set { curServerIndex_ = value; }
		}

		/// <summary>Current phone number (only for modem)</summary>		
		public string CurPhoneNumber
		{
			get { return curPhoneNumber_; }
			set { curPhoneNumber_ = value; }
		}

		/// <summary>Current IP address (only for Ethernet and GPRS)</summary>		
		public string CurrentIPAddress
		{
			get { return curIPAddress_; }
			set 
			{
				if (value != curIPAddress_)
				{
					curIPAddress_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Current port (only for Ethernet and GPRS)</summary>		
		public int CurrentPort
		{
			get { return curPort_; }
			set
			{
				if (value != curPort_)
				{
					curPort_ = value;
					settingsChanged_ = true;
				}
			}
		}

        /// <summary>Current dial attempt number (only for modem)</summary>		
        public int AttemptNumber
        {
            get { return numAttempts_; }
            set { numAttempts_ = value; }
        }

		/// <summary>Gets or sets IP address of PostgreSQL database server</summary>
		public PgServerItem[] PgServers
		{
			get { return pgServers_; }
            set {pgServers_ = value;}
        }
        
		/// <summary>Gets licences class </summary>
		public Licences Licences
		{
			get { return licences_; }
			set { licences_ = value; }
		}

		/// <summary>Gets connection interface parameters array</summary>
		public object[] IOParameters
		{
			get
			{
				switch (IOInterface)
				{
					case EmPortType.COM:
						return new object[] { this.serialPortName_, this.serialPortSpeed_ };
					case EmPortType.USB:
						return null;
					case EmPortType.Modem:
						return new object[] { this.serialPortNameModem_, this.serialSpeedModem_,  
												this.curPhoneNumber_, this.numAttempts_};
					case EmPortType.Ethernet:
						return new object[] { this.curIPAddress_, this.curPort_ };
					case EmPortType.GPRS:
						return new object[] { this.curIPAddress_, this.curPort_ };
					case EmPortType.Rs485:
						return new object[] { this.serialPortName485_, this.serialSpeed485_,
												this.curDeviceAddress_};
					default:
						return null;
				}
			}
		}

		/// <summary>1 or 0,001 (for A or kA)</summary>
		public float CurrentRatio
		{
			get { return currentRatio_; }
			set { currentRatio_ = value; }
		}


		/// <summary>1 or 0,001 (for V or kV)</summary>
		public float VoltageRatio
		{
			get { return voltageRatio_; }
			set { voltageRatio_ = value; }
		}

		/// <summary>1 or 0,001 (for W or kW)</summary>
		public float PowerRatio
		{
			get { return powerRatio_; }
			set { powerRatio_ = value; }
		}

		/// <summary>Averaged values graph's background color 1</summary>
		public int AvgBrushColor1
		{
			get { return avgBrushColor1_.ToArgb(); }
			set { avgBrushColor1_ = Color.FromArgb(value); }
		}

		/// <summary>Averaged values graph's background color 2</summary>		
		public int AvgBrushColor2
		{
			get { return avgBrushColor2_.ToArgb(); }
			set { avgBrushColor2_ = Color.FromArgb(value); }
		}

		/// <summary>Password for wi-fi</summary>
		public string WifiPassword
		{
			get { return wifiPassword_; }
			set { wifiPassword_ = value; }
		}

		public string CurWifiProfileName
		{
			get { return curWifiProfileName_; }
			set { curWifiProfileName_ = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor with defaut settings
		/// </summary>
		public Settings()
		{
			// defaults:
			this.language_ = "English";
			this.CurrentLanguage = "en";
			this.floatSigns_ = 2;
            this.bShowAvgTooltip_ = false;
			this.IOInterface_ = EmPortType.COM;
			this.serialPortName_ = "COM1";
			this.serialPortNameModem_ = "COM1";
			this.serialPortName485_ = "COM1";
			this.serialPortSpeed_ = 115200;
			this.serialSpeedModem_ = 115200;
			this.serialSpeed485_ = 115200;
			this.settingsChanged_ = false;
			this.currentRatio_ = 0.0F;
			this.voltageRatio_ = 0.0F; 
			this.powerRatio_ = 0.0F;
			this.avgBrushColor1_ = Color.Gray;
			this.avgBrushColor2_ = Color.Black;
            this.pgServers_ = null;
			this.curDeviceType_ = EmDeviceType.EM33T;
			this.optimisedInsert_ = false;
			this.checkFirmwareEtPQP_A_ = true;
			this.checkNewSoftwareVersion_ = true;
			this.dontWarnAutoSynchroTimeDisabled_ = false;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Serializes the class to the config file if any of the settings have changed.
		/// </summary>
		public void SaveSettings()
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				// Create an XmlSerializer for the 
				// ApplicationSettings type.
				mySerializer = new XmlSerializer(typeof(Settings));
				myWriter = new StreamWriter(settingsFileName_, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, this);
				this.settingsChanged_ = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in SaveSettings(): ");
				throw;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}
		}

		/// <summary>
		/// Deserializes the class from the config file.
		/// </summary>
		public void LoadSettings()
		{
			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the ApplicationSettings type.
				mySerializer = new XmlSerializer(typeof(Settings));
				FileInfo fi = new FileInfo(settingsFileName_);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the ApplicationSettings by
					// deserializing the config file.
					Settings myAppSettings = (Settings)mySerializer.Deserialize(myFileStream);
					// Assign the property values to this instance of 
					// the ApplicationSettings class.
					this.language_ = myAppSettings.language_;
					this.serialPortName_ = myAppSettings.serialPortName_;
					this.serialPortNameModem_ = myAppSettings.serialPortNameModem_;
					this.serialPortName485_ = myAppSettings.serialPortName485_;
					this.serialSpeedModem_ = myAppSettings.serialSpeedModem_;
					this.serialSpeed485_ = myAppSettings.serialSpeed485_;
					this.serialPortSpeed_ = myAppSettings.serialPortSpeed_;
					this.IOInterface_ = myAppSettings.IOInterface_;
					this.settingsChanged_ = false;
					this.licences_ = myAppSettings.licences_;
					this.floatSigns_ = myAppSettings.floatSigns_;
                    this.bShowAvgTooltip_ = myAppSettings.bShowAvgTooltip_;
					this.currentRatio_ = myAppSettings.currentRatio_;
					this.voltageRatio_ = myAppSettings.voltageRatio_;
					this.powerRatio_ = myAppSettings.powerRatio_;
					this.avgBrushColor1_ = myAppSettings.avgBrushColor1_;
					this.avgBrushColor2_ = myAppSettings.avgBrushColor2_;
                    this.pgServers_ = myAppSettings.pgServers_;
					this.curIPAddress_ = myAppSettings.curIPAddress_;
					//this.curWifiIPaddress_ = myAppSettings.curWifiIPaddress_;
					this.wifiPassword_ = myAppSettings.wifiPassword_;
					this.curWifiProfileName_ = myAppSettings.curWifiProfileName_;
					this.curPort_ = myAppSettings.curPort_;
					this.curServerIndex_ = myAppSettings.curServerIndex_;
					this.curDeviceType_ = myAppSettings.curDeviceType_;
					this.optimisedInsert_ = myAppSettings.optimisedInsert_;
					this.checkFirmwareEtPQP_A_ = myAppSettings.checkFirmwareEtPQP_A_;
					this.checkNewSoftwareVersion_ = myAppSettings.checkNewSoftwareVersion_;
					this.dontWarnAutoSynchroTimeDisabled_ = myAppSettings.dontWarnAutoSynchroTimeDisabled_;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadSettings(): ");
				throw;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
		}

		/// <summary>
		/// Clone method
		/// </summary>
		/// <returns>Copy of this object</returns>
		public Settings Clone()
		{
			Settings obj = new Settings();
			obj.language_ = this.language_;
			obj.serialPortName_ = this.serialPortName_;
			obj.serialPortNameModem_ = this.serialPortNameModem_;
			obj.serialPortName485_ = this.serialPortName485_;
			obj.serialPortSpeed_ = this.serialPortSpeed_;
			obj.serialSpeedModem_ = this.serialSpeedModem_;
			obj.serialSpeed485_ = this.serialSpeed485_;
			obj.settingsChanged_ = this.settingsChanged_;
			obj.CurrentLanguage = this.CurrentLanguage;
			obj.floatSigns_ = this.floatSigns_;
            obj.bShowAvgTooltip_ = this.bShowAvgTooltip_;
			obj.licences_ = this.licences_;
			obj.IOInterface_ = this.IOInterface_;
			obj.currentRatio_ = this.currentRatio_;
			obj.voltageRatio_ = this.voltageRatio_;
			obj.powerRatio_ = this.powerRatio_;
			obj.avgBrushColor1_ = this.avgBrushColor1_;
			obj.avgBrushColor2_ = this.avgBrushColor2_;
            obj.pgServers_ = this.pgServers_;
			obj.curIPAddress_ = this.curIPAddress_;
			//obj.curWifiIPaddress_ = this.curWifiIPaddress_;
			obj.wifiPassword_ = this.wifiPassword_;
			obj.curWifiProfileName_ = this.curWifiProfileName_;
			obj.curPhoneNumber_ = this.curPhoneNumber_;
			obj.curPort_ = this.curPort_;
			obj.curServerIndex_ = this.curServerIndex_;
			obj.curDeviceType_ = this.curDeviceType_;
			obj.optimisedInsert_ = this.optimisedInsert_;
			obj.checkFirmwareEtPQP_A_ = this.checkFirmwareEtPQP_A_;
			obj.checkNewSoftwareVersion_ = this.checkNewSoftwareVersion_;
			obj.dontWarnAutoSynchroTimeDisabled_ = this.dontWarnAutoSynchroTimeDisabled_;
			return obj;
		}

		#endregion

		#region auto settings

		[NonSerialized]
		public AutoSettingsData AutoSettings = new AutoSettingsData(); 

		public class AutoSettingsData
		{
			public EmPortType AutoIOInterface;

			// modem and RS485
			//public string AutoSerialPortName;
			//public int AutoSerialPortSpeed;
			public string AutoPhoneNumber;

			//ethernet or GPRS
			public int AutoPort;
			public string AutoIPAddress;

			//RS485
			public ushort AutoDeviceAddress;
		}

		#endregion
	}	
}
