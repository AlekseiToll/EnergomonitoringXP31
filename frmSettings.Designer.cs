namespace EnergomonitoringXP
{
	/// <summary>
	/// Form of the <c>Options</c> dialog window
	/// </summary>
	partial class frmSettings
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
			this.tcOptions = new System.Windows.Forms.TabControl();
			this.tabCommon = new System.Windows.Forms.TabPage();
			this.gbTime = new System.Windows.Forms.GroupBox();
			this.chbAutoTimeWarn = new System.Windows.Forms.CheckBox();
			this.gbNewFirmware = new System.Windows.Forms.GroupBox();
			this.chbNewSoftware = new System.Windows.Forms.CheckBox();
			this.chbNewFirmware = new System.Windows.Forms.CheckBox();
			this.gbAvgTooltip = new System.Windows.Forms.GroupBox();
			this.rbTooltipOff = new System.Windows.Forms.RadioButton();
			this.rbTooltipOn = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.gbAVGParams = new System.Windows.Forms.GroupBox();
			this.btnSetAvgParams = new System.Windows.Forms.Button();
			this.gbDevice = new System.Windows.Forms.GroupBox();
			this.rbEtPQP_A = new System.Windows.Forms.RadioButton();
			this.rbEm31k = new System.Windows.Forms.RadioButton();
			this.rbEtPQP = new System.Windows.Forms.RadioButton();
			this.rbEm32 = new System.Windows.Forms.RadioButton();
			this.rbEm33t = new System.Windows.Forms.RadioButton();
			this.gbServerDB = new System.Windows.Forms.GroupBox();
			this.chbOptimizeInsert = new System.Windows.Forms.CheckBox();
			this.cbServerDB = new System.Windows.Forms.ComboBox();
			this.gbAvgColors = new System.Windows.Forms.GroupBox();
			this.lblAvgGraphsBrushExample = new System.Windows.Forms.Label();
			this.pnlAvgBrushExample = new System.Windows.Forms.Panel();
			this.pnlAvgBrushColor2 = new System.Windows.Forms.Panel();
			this.pnlAvgBrushColor1 = new System.Windows.Forms.Panel();
			this.chkAvgBrushGradient = new System.Windows.Forms.CheckBox();
			this.gbRatios = new System.Windows.Forms.GroupBox();
			this.rbKW = new System.Windows.Forms.RadioButton();
			this.rbW = new System.Windows.Forms.RadioButton();
			this.rbKV = new System.Windows.Forms.RadioButton();
			this.rbKA = new System.Windows.Forms.RadioButton();
			this.rbV = new System.Windows.Forms.RadioButton();
			this.rbA = new System.Windows.Forms.RadioButton();
			this.gbLanguage = new System.Windows.Forms.GroupBox();
			this.cmbLanguage = new System.Windows.Forms.ComboBox();
			this.gbDecimalType = new System.Windows.Forms.GroupBox();
			this.cmbFloatSigns = new System.Windows.Forms.ComboBox();
			this.txtFloatFormatExample = new System.Windows.Forms.TextBox();
			this.tabSerialPort = new System.Windows.Forms.TabPage();
			this.tcConnections = new System.Windows.Forms.TabControl();
			this.tabGSM = new System.Windows.Forms.TabPage();
			this.labelSpeedModem = new System.Windows.Forms.Label();
			this.cmbSerialSpeedModem = new System.Windows.Forms.ComboBox();
			this.labelPortModem = new System.Windows.Forms.Label();
			this.cmbSerialPortModem = new System.Windows.Forms.ComboBox();
			this.nudAttempts = new System.Windows.Forms.NumericUpDown();
			this.labelAttempts = new System.Windows.Forms.Label();
			this.dgvPhoneNumbers = new System.Windows.Forms.DataGridView();
			this.colActive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colAuto = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSerial = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTime = new EnergomonitoringXP.CalendarColumn();
			this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDevType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.btnRemove = new System.Windows.Forms.Button();
			this.btnAdd = new System.Windows.Forms.Button();
			this.tabGPRS = new System.Windows.Forms.TabPage();
			this.btnRemoveGPRS = new System.Windows.Forms.Button();
			this.btnAddGPRS = new System.Windows.Forms.Button();
			this.dgvGPRS = new System.Windows.Forms.DataGridView();
			this.colActiveGPRS = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colAutoGPRS = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colIPAddressGPRS = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colPortGPRS = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSerialGPRS = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colStartGPRS = new EnergomonitoringXP.CalendarColumn();
			this.colCommentGPRS = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDevTypeGPRS = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabEthernet = new System.Windows.Forms.TabPage();
			this.btnRemoveE = new System.Windows.Forms.Button();
			this.btnAddE = new System.Windows.Forms.Button();
			this.dgvEthernet = new System.Windows.Forms.DataGridView();
			this.colActiveE = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colAutoE = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colIPAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSerialE = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colStartE = new EnergomonitoringXP.CalendarColumn();
			this.colCommentE = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDevTypeE = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabRs485 = new System.Windows.Forms.TabPage();
			this.labelSpeed485 = new System.Windows.Forms.Label();
			this.cmbSerialSpeed485 = new System.Windows.Forms.ComboBox();
			this.labelPort485 = new System.Windows.Forms.Label();
			this.cmbSerialPort485 = new System.Windows.Forms.ComboBox();
			this.btnRemove485 = new System.Windows.Forms.Button();
			this.btnAdd485 = new System.Windows.Forms.Button();
			this.dgv485 = new System.Windows.Forms.DataGridView();
			this.colActive485 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colAuto485 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colSerial485 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colStart485 = new EnergomonitoringXP.CalendarColumn();
			this.colComment485 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDevType485 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabWifi = new System.Windows.Forms.TabPage();
			this.btnConnect = new System.Windows.Forms.Button();
			this.labelCurWiifiPofile = new System.Windows.Forms.Label();
			this.tbCurWifiProfile = new System.Windows.Forms.TextBox();
			this.labelWifiPassword = new System.Windows.Forms.Label();
			this.tbWifiPassword = new System.Windows.Forms.TextBox();
			this.gbConnections = new System.Windows.Forms.GroupBox();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.lvConnections = new System.Windows.Forms.ListView();
			this.colWifiName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colWifiSignal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.gbxSerial = new System.Windows.Forms.GroupBox();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.lblSelectSerialPort = new System.Windows.Forms.Label();
			this.lblSelectSerialPortSpeed = new System.Windows.Forms.Label();
			this.btnDetect = new System.Windows.Forms.Button();
			this.cmbSerialPort = new System.Windows.Forms.ComboBox();
			this.cmbSerialPortSpeed = new System.Windows.Forms.ComboBox();
			this.gbxInterface = new System.Windows.Forms.GroupBox();
			this.rbWifi = new System.Windows.Forms.RadioButton();
			this.rbtnModemGPRS = new System.Windows.Forms.RadioButton();
			this.rbRs485 = new System.Windows.Forms.RadioButton();
			this.rbEthernet = new System.Windows.Forms.RadioButton();
			this.rbtnModemGSM = new System.Windows.Forms.RadioButton();
			this.rbtnUSB = new System.Windows.Forms.RadioButton();
			this.rbtnCOM = new System.Windows.Forms.RadioButton();
			this.tabDeviceManager = new System.Windows.Forms.TabPage();
			this.listBoxAvailable = new System.Windows.Forms.ListBox();
			this.labelLicDevices = new System.Windows.Forms.Label();
			this.listBoxInstalled = new System.Windows.Forms.ListBox();
			this.lblInstalledDevices = new System.Windows.Forms.Label();
			this.btnDropLicence = new System.Windows.Forms.Button();
			this.btnAddLicence = new System.Windows.Forms.Button();
			this.btnOpenLicenseFile = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnApply = new System.Windows.Forms.Button();
			this.toolTipHost = new System.Windows.Forms.ToolTip(this.components);
			this.dlgOpenLicence = new System.Windows.Forms.OpenFileDialog();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.calendarColumn1 = new EnergomonitoringXP.CalendarColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.calendarColumn2 = new EnergomonitoringXP.CalendarColumn();
			this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.calendarColumn3 = new EnergomonitoringXP.CalendarColumn();
			this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.calendarColumn4 = new EnergomonitoringXP.CalendarColumn();
			this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tcOptions.SuspendLayout();
			this.tabCommon.SuspendLayout();
			this.gbTime.SuspendLayout();
			this.gbNewFirmware.SuspendLayout();
			this.gbAvgTooltip.SuspendLayout();
			this.gbAVGParams.SuspendLayout();
			this.gbDevice.SuspendLayout();
			this.gbServerDB.SuspendLayout();
			this.gbAvgColors.SuspendLayout();
			this.gbRatios.SuspendLayout();
			this.gbLanguage.SuspendLayout();
			this.gbDecimalType.SuspendLayout();
			this.tabSerialPort.SuspendLayout();
			this.tcConnections.SuspendLayout();
			this.tabGSM.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudAttempts)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvPhoneNumbers)).BeginInit();
			this.tabGPRS.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvGPRS)).BeginInit();
			this.tabEthernet.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvEthernet)).BeginInit();
			this.tabRs485.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgv485)).BeginInit();
			this.tabWifi.SuspendLayout();
			this.gbConnections.SuspendLayout();
			this.gbxSerial.SuspendLayout();
			this.gbxInterface.SuspendLayout();
			this.tabDeviceManager.SuspendLayout();
			this.SuspendLayout();
			// 
			// tcOptions
			// 
			this.tcOptions.Controls.Add(this.tabCommon);
			this.tcOptions.Controls.Add(this.tabSerialPort);
			this.tcOptions.Controls.Add(this.tabDeviceManager);
			resources.ApplyResources(this.tcOptions, "tcOptions");
			this.tcOptions.Name = "tcOptions";
			this.tcOptions.SelectedIndex = 0;
			// 
			// tabCommon
			// 
			this.tabCommon.Controls.Add(this.gbTime);
			this.tabCommon.Controls.Add(this.gbNewFirmware);
			this.tabCommon.Controls.Add(this.gbAvgTooltip);
			this.tabCommon.Controls.Add(this.gbAVGParams);
			this.tabCommon.Controls.Add(this.gbDevice);
			this.tabCommon.Controls.Add(this.gbServerDB);
			this.tabCommon.Controls.Add(this.gbAvgColors);
			this.tabCommon.Controls.Add(this.gbRatios);
			this.tabCommon.Controls.Add(this.gbLanguage);
			this.tabCommon.Controls.Add(this.gbDecimalType);
			resources.ApplyResources(this.tabCommon, "tabCommon");
			this.tabCommon.Name = "tabCommon";
			this.tabCommon.UseVisualStyleBackColor = true;
			// 
			// gbTime
			// 
			this.gbTime.Controls.Add(this.chbAutoTimeWarn);
			resources.ApplyResources(this.gbTime, "gbTime");
			this.gbTime.Name = "gbTime";
			this.gbTime.TabStop = false;
			// 
			// chbAutoTimeWarn
			// 
			resources.ApplyResources(this.chbAutoTimeWarn, "chbAutoTimeWarn");
			this.chbAutoTimeWarn.Name = "chbAutoTimeWarn";
			this.chbAutoTimeWarn.UseVisualStyleBackColor = true;
			this.chbAutoTimeWarn.CheckedChanged += new System.EventHandler(this.chbAutoTimeWarn_CheckedChanged);
			// 
			// gbNewFirmware
			// 
			this.gbNewFirmware.Controls.Add(this.chbNewSoftware);
			this.gbNewFirmware.Controls.Add(this.chbNewFirmware);
			resources.ApplyResources(this.gbNewFirmware, "gbNewFirmware");
			this.gbNewFirmware.Name = "gbNewFirmware";
			this.gbNewFirmware.TabStop = false;
			// 
			// chbNewSoftware
			// 
			resources.ApplyResources(this.chbNewSoftware, "chbNewSoftware");
			this.chbNewSoftware.Name = "chbNewSoftware";
			this.chbNewSoftware.UseVisualStyleBackColor = true;
			this.chbNewSoftware.CheckedChanged += new System.EventHandler(this.chbNewSoftware_CheckedChanged);
			// 
			// chbNewFirmware
			// 
			resources.ApplyResources(this.chbNewFirmware, "chbNewFirmware");
			this.chbNewFirmware.Name = "chbNewFirmware";
			this.chbNewFirmware.UseVisualStyleBackColor = true;
			this.chbNewFirmware.CheckedChanged += new System.EventHandler(this.chbNewFirmware_CheckedChanged);
			// 
			// gbAvgTooltip
			// 
			this.gbAvgTooltip.Controls.Add(this.rbTooltipOff);
			this.gbAvgTooltip.Controls.Add(this.rbTooltipOn);
			this.gbAvgTooltip.Controls.Add(this.label1);
			resources.ApplyResources(this.gbAvgTooltip, "gbAvgTooltip");
			this.gbAvgTooltip.Name = "gbAvgTooltip";
			this.gbAvgTooltip.TabStop = false;
			// 
			// rbTooltipOff
			// 
			resources.ApplyResources(this.rbTooltipOff, "rbTooltipOff");
			this.rbTooltipOff.Checked = true;
			this.rbTooltipOff.Name = "rbTooltipOff";
			this.rbTooltipOff.TabStop = true;
			this.rbTooltipOff.UseVisualStyleBackColor = true;
			this.rbTooltipOff.Click += new System.EventHandler(this.rbTooltip_Click);
			// 
			// rbTooltipOn
			// 
			resources.ApplyResources(this.rbTooltipOn, "rbTooltipOn");
			this.rbTooltipOn.Name = "rbTooltipOn";
			this.rbTooltipOn.UseVisualStyleBackColor = true;
			this.rbTooltipOn.Click += new System.EventHandler(this.rbTooltip_Click);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// gbAVGParams
			// 
			this.gbAVGParams.Controls.Add(this.btnSetAvgParams);
			resources.ApplyResources(this.gbAVGParams, "gbAVGParams");
			this.gbAVGParams.Name = "gbAVGParams";
			this.gbAVGParams.TabStop = false;
			// 
			// btnSetAvgParams
			// 
			resources.ApplyResources(this.btnSetAvgParams, "btnSetAvgParams");
			this.btnSetAvgParams.Name = "btnSetAvgParams";
			this.btnSetAvgParams.UseVisualStyleBackColor = true;
			this.btnSetAvgParams.Click += new System.EventHandler(this.btnSetAvgParams_Click);
			// 
			// gbDevice
			// 
			this.gbDevice.Controls.Add(this.rbEtPQP_A);
			this.gbDevice.Controls.Add(this.rbEm31k);
			this.gbDevice.Controls.Add(this.rbEtPQP);
			this.gbDevice.Controls.Add(this.rbEm32);
			this.gbDevice.Controls.Add(this.rbEm33t);
			resources.ApplyResources(this.gbDevice, "gbDevice");
			this.gbDevice.Name = "gbDevice";
			this.gbDevice.TabStop = false;
			// 
			// rbEtPQP_A
			// 
			resources.ApplyResources(this.rbEtPQP_A, "rbEtPQP_A");
			this.rbEtPQP_A.Name = "rbEtPQP_A";
			this.rbEtPQP_A.UseVisualStyleBackColor = true;
			this.rbEtPQP_A.CheckedChanged += new System.EventHandler(this.rbtnCurDev_CheckedChanged);
			// 
			// rbEm31k
			// 
			resources.ApplyResources(this.rbEm31k, "rbEm31k");
			this.rbEm31k.Name = "rbEm31k";
			this.rbEm31k.UseVisualStyleBackColor = true;
			this.rbEm31k.CheckedChanged += new System.EventHandler(this.rbtnCurDev_CheckedChanged);
			// 
			// rbEtPQP
			// 
			resources.ApplyResources(this.rbEtPQP, "rbEtPQP");
			this.rbEtPQP.Name = "rbEtPQP";
			this.rbEtPQP.UseVisualStyleBackColor = true;
			this.rbEtPQP.CheckedChanged += new System.EventHandler(this.rbtnCurDev_CheckedChanged);
			// 
			// rbEm32
			// 
			resources.ApplyResources(this.rbEm32, "rbEm32");
			this.rbEm32.Name = "rbEm32";
			this.rbEm32.UseVisualStyleBackColor = true;
			this.rbEm32.CheckedChanged += new System.EventHandler(this.rbtnCurDev_CheckedChanged);
			// 
			// rbEm33t
			// 
			resources.ApplyResources(this.rbEm33t, "rbEm33t");
			this.rbEm33t.Checked = true;
			this.rbEm33t.Name = "rbEm33t";
			this.rbEm33t.TabStop = true;
			this.rbEm33t.UseVisualStyleBackColor = true;
			this.rbEm33t.CheckedChanged += new System.EventHandler(this.rbtnCurDev_CheckedChanged);
			// 
			// gbServerDB
			// 
			this.gbServerDB.Controls.Add(this.chbOptimizeInsert);
			this.gbServerDB.Controls.Add(this.cbServerDB);
			resources.ApplyResources(this.gbServerDB, "gbServerDB");
			this.gbServerDB.Name = "gbServerDB";
			this.gbServerDB.TabStop = false;
			// 
			// chbOptimizeInsert
			// 
			resources.ApplyResources(this.chbOptimizeInsert, "chbOptimizeInsert");
			this.chbOptimizeInsert.Name = "chbOptimizeInsert";
			this.chbOptimizeInsert.UseVisualStyleBackColor = true;
			this.chbOptimizeInsert.CheckedChanged += new System.EventHandler(this.chbOptimizeInsert_CheckedChanged);
			// 
			// cbServerDB
			// 
			this.cbServerDB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbServerDB.FormattingEnabled = true;
			resources.ApplyResources(this.cbServerDB, "cbServerDB");
			this.cbServerDB.Name = "cbServerDB";
			this.cbServerDB.SelectedIndexChanged += new System.EventHandler(this.cbServerDB_SelectedIndexChanged);
			// 
			// gbAvgColors
			// 
			this.gbAvgColors.Controls.Add(this.lblAvgGraphsBrushExample);
			this.gbAvgColors.Controls.Add(this.pnlAvgBrushExample);
			this.gbAvgColors.Controls.Add(this.pnlAvgBrushColor2);
			this.gbAvgColors.Controls.Add(this.pnlAvgBrushColor1);
			this.gbAvgColors.Controls.Add(this.chkAvgBrushGradient);
			resources.ApplyResources(this.gbAvgColors, "gbAvgColors");
			this.gbAvgColors.Name = "gbAvgColors";
			this.gbAvgColors.TabStop = false;
			// 
			// lblAvgGraphsBrushExample
			// 
			resources.ApplyResources(this.lblAvgGraphsBrushExample, "lblAvgGraphsBrushExample");
			this.lblAvgGraphsBrushExample.Name = "lblAvgGraphsBrushExample";
			// 
			// pnlAvgBrushExample
			// 
			this.pnlAvgBrushExample.BackColor = System.Drawing.Color.Transparent;
			this.pnlAvgBrushExample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			resources.ApplyResources(this.pnlAvgBrushExample, "pnlAvgBrushExample");
			this.pnlAvgBrushExample.Name = "pnlAvgBrushExample";
			this.pnlAvgBrushExample.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlAvgBrushExample_Paint);
			// 
			// pnlAvgBrushColor2
			// 
			this.pnlAvgBrushColor2.BackColor = System.Drawing.Color.Black;
			this.pnlAvgBrushColor2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlAvgBrushColor2.Cursor = System.Windows.Forms.Cursors.Hand;
			resources.ApplyResources(this.pnlAvgBrushColor2, "pnlAvgBrushColor2");
			this.pnlAvgBrushColor2.Name = "pnlAvgBrushColor2";
			this.pnlAvgBrushColor2.Click += new System.EventHandler(this.pnlAvgBrushColor2_Click);
			// 
			// pnlAvgBrushColor1
			// 
			this.pnlAvgBrushColor1.BackColor = System.Drawing.Color.Gray;
			this.pnlAvgBrushColor1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlAvgBrushColor1.Cursor = System.Windows.Forms.Cursors.Hand;
			resources.ApplyResources(this.pnlAvgBrushColor1, "pnlAvgBrushColor1");
			this.pnlAvgBrushColor1.Name = "pnlAvgBrushColor1";
			this.pnlAvgBrushColor1.Click += new System.EventHandler(this.pnlAvgBrushColor1_Click);
			// 
			// chkAvgBrushGradient
			// 
			resources.ApplyResources(this.chkAvgBrushGradient, "chkAvgBrushGradient");
			this.chkAvgBrushGradient.Checked = true;
			this.chkAvgBrushGradient.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkAvgBrushGradient.Name = "chkAvgBrushGradient";
			this.chkAvgBrushGradient.UseVisualStyleBackColor = true;
			this.chkAvgBrushGradient.CheckedChanged += new System.EventHandler(this.chkAvgBrushGradient_CheckedChanged);
			// 
			// gbRatios
			// 
			this.gbRatios.Controls.Add(this.rbKW);
			this.gbRatios.Controls.Add(this.rbW);
			this.gbRatios.Controls.Add(this.rbKV);
			this.gbRatios.Controls.Add(this.rbKA);
			this.gbRatios.Controls.Add(this.rbV);
			this.gbRatios.Controls.Add(this.rbA);
			resources.ApplyResources(this.gbRatios, "gbRatios");
			this.gbRatios.Name = "gbRatios";
			this.gbRatios.TabStop = false;
			// 
			// rbKW
			// 
			this.rbKW.AutoCheck = false;
			resources.ApplyResources(this.rbKW, "rbKW");
			this.rbKW.Name = "rbKW";
			this.rbKW.UseVisualStyleBackColor = true;
			this.rbKW.CheckedChanged += new System.EventHandler(this.RatiosRadioButtons_ANY_CheckedChanged);
			this.rbKW.Click += new System.EventHandler(this.RatiosRadioButtons_ANY_Click);
			// 
			// rbW
			// 
			this.rbW.AutoCheck = false;
			resources.ApplyResources(this.rbW, "rbW");
			this.rbW.Checked = true;
			this.rbW.Name = "rbW";
			this.rbW.TabStop = true;
			this.rbW.UseVisualStyleBackColor = true;
			this.rbW.CheckedChanged += new System.EventHandler(this.RatiosRadioButtons_ANY_CheckedChanged);
			this.rbW.Click += new System.EventHandler(this.RatiosRadioButtons_ANY_Click);
			// 
			// rbKV
			// 
			this.rbKV.AutoCheck = false;
			resources.ApplyResources(this.rbKV, "rbKV");
			this.rbKV.Name = "rbKV";
			this.rbKV.UseVisualStyleBackColor = true;
			this.rbKV.CheckedChanged += new System.EventHandler(this.RatiosRadioButtons_ANY_CheckedChanged);
			this.rbKV.Click += new System.EventHandler(this.RatiosRadioButtons_ANY_Click);
			// 
			// rbKA
			// 
			this.rbKA.AutoCheck = false;
			resources.ApplyResources(this.rbKA, "rbKA");
			this.rbKA.Name = "rbKA";
			this.rbKA.UseVisualStyleBackColor = true;
			this.rbKA.CheckedChanged += new System.EventHandler(this.RatiosRadioButtons_ANY_CheckedChanged);
			this.rbKA.Click += new System.EventHandler(this.RatiosRadioButtons_ANY_Click);
			// 
			// rbV
			// 
			this.rbV.AutoCheck = false;
			resources.ApplyResources(this.rbV, "rbV");
			this.rbV.Checked = true;
			this.rbV.Name = "rbV";
			this.rbV.UseVisualStyleBackColor = true;
			this.rbV.CheckedChanged += new System.EventHandler(this.RatiosRadioButtons_ANY_CheckedChanged);
			this.rbV.Click += new System.EventHandler(this.RatiosRadioButtons_ANY_Click);
			// 
			// rbA
			// 
			this.rbA.AutoCheck = false;
			resources.ApplyResources(this.rbA, "rbA");
			this.rbA.Checked = true;
			this.rbA.Name = "rbA";
			this.rbA.UseVisualStyleBackColor = true;
			this.rbA.CheckedChanged += new System.EventHandler(this.RatiosRadioButtons_ANY_CheckedChanged);
			this.rbA.Click += new System.EventHandler(this.RatiosRadioButtons_ANY_Click);
			// 
			// gbLanguage
			// 
			this.gbLanguage.Controls.Add(this.cmbLanguage);
			resources.ApplyResources(this.gbLanguage, "gbLanguage");
			this.gbLanguage.Name = "gbLanguage";
			this.gbLanguage.TabStop = false;
			// 
			// cmbLanguage
			// 
			this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbLanguage.FormattingEnabled = true;
			this.cmbLanguage.Items.AddRange(new object[] {
            resources.GetString("cmbLanguage.Items"),
            resources.GetString("cmbLanguage.Items1")});
			resources.ApplyResources(this.cmbLanguage, "cmbLanguage");
			this.cmbLanguage.Name = "cmbLanguage";
			this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
			// 
			// gbDecimalType
			// 
			this.gbDecimalType.Controls.Add(this.cmbFloatSigns);
			this.gbDecimalType.Controls.Add(this.txtFloatFormatExample);
			resources.ApplyResources(this.gbDecimalType, "gbDecimalType");
			this.gbDecimalType.Name = "gbDecimalType";
			this.gbDecimalType.TabStop = false;
			// 
			// cmbFloatSigns
			// 
			this.cmbFloatSigns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbFloatSigns.FormattingEnabled = true;
			this.cmbFloatSigns.Items.AddRange(new object[] {
            resources.GetString("cmbFloatSigns.Items"),
            resources.GetString("cmbFloatSigns.Items1"),
            resources.GetString("cmbFloatSigns.Items2"),
            resources.GetString("cmbFloatSigns.Items3"),
            resources.GetString("cmbFloatSigns.Items4")});
			resources.ApplyResources(this.cmbFloatSigns, "cmbFloatSigns");
			this.cmbFloatSigns.Name = "cmbFloatSigns";
			this.cmbFloatSigns.SelectedIndexChanged += new System.EventHandler(this.cmbFloatSigns_SelectedIndexChanged);
			// 
			// txtFloatFormatExample
			// 
			this.txtFloatFormatExample.BackColor = System.Drawing.Color.AliceBlue;
			resources.ApplyResources(this.txtFloatFormatExample, "txtFloatFormatExample");
			this.txtFloatFormatExample.Name = "txtFloatFormatExample";
			this.txtFloatFormatExample.ReadOnly = true;
			this.toolTipHost.SetToolTip(this.txtFloatFormatExample, resources.GetString("txtFloatFormatExample.ToolTip"));
			// 
			// tabSerialPort
			// 
			this.tabSerialPort.Controls.Add(this.tcConnections);
			this.tabSerialPort.Controls.Add(this.gbxSerial);
			this.tabSerialPort.Controls.Add(this.gbxInterface);
			resources.ApplyResources(this.tabSerialPort, "tabSerialPort");
			this.tabSerialPort.Name = "tabSerialPort";
			this.tabSerialPort.UseVisualStyleBackColor = true;
			// 
			// tcConnections
			// 
			this.tcConnections.Controls.Add(this.tabGSM);
			this.tcConnections.Controls.Add(this.tabGPRS);
			this.tcConnections.Controls.Add(this.tabEthernet);
			this.tcConnections.Controls.Add(this.tabRs485);
			this.tcConnections.Controls.Add(this.tabWifi);
			resources.ApplyResources(this.tcConnections, "tcConnections");
			this.tcConnections.Name = "tcConnections";
			this.tcConnections.SelectedIndex = 0;
			// 
			// tabGSM
			// 
			this.tabGSM.Controls.Add(this.labelSpeedModem);
			this.tabGSM.Controls.Add(this.cmbSerialSpeedModem);
			this.tabGSM.Controls.Add(this.labelPortModem);
			this.tabGSM.Controls.Add(this.cmbSerialPortModem);
			this.tabGSM.Controls.Add(this.nudAttempts);
			this.tabGSM.Controls.Add(this.labelAttempts);
			this.tabGSM.Controls.Add(this.dgvPhoneNumbers);
			this.tabGSM.Controls.Add(this.btnRemove);
			this.tabGSM.Controls.Add(this.btnAdd);
			resources.ApplyResources(this.tabGSM, "tabGSM");
			this.tabGSM.Name = "tabGSM";
			this.tabGSM.UseVisualStyleBackColor = true;
			// 
			// labelSpeedModem
			// 
			resources.ApplyResources(this.labelSpeedModem, "labelSpeedModem");
			this.labelSpeedModem.Name = "labelSpeedModem";
			// 
			// cmbSerialSpeedModem
			// 
			this.cmbSerialSpeedModem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialSpeedModem.FormattingEnabled = true;
			this.cmbSerialSpeedModem.Items.AddRange(new object[] {
            resources.GetString("cmbSerialSpeedModem.Items"),
            resources.GetString("cmbSerialSpeedModem.Items1"),
            resources.GetString("cmbSerialSpeedModem.Items2"),
            resources.GetString("cmbSerialSpeedModem.Items3")});
			resources.ApplyResources(this.cmbSerialSpeedModem, "cmbSerialSpeedModem");
			this.cmbSerialSpeedModem.Name = "cmbSerialSpeedModem";
			this.cmbSerialSpeedModem.SelectedIndexChanged += new System.EventHandler(this.cmbSerialSpeedModem_SelectedIndexChanged);
			// 
			// labelPortModem
			// 
			resources.ApplyResources(this.labelPortModem, "labelPortModem");
			this.labelPortModem.Name = "labelPortModem";
			// 
			// cmbSerialPortModem
			// 
			this.cmbSerialPortModem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialPortModem.FormattingEnabled = true;
			resources.ApplyResources(this.cmbSerialPortModem, "cmbSerialPortModem");
			this.cmbSerialPortModem.Name = "cmbSerialPortModem";
			this.cmbSerialPortModem.SelectedIndexChanged += new System.EventHandler(this.cmbSerialPortModem_SelectedIndexChanged);
			// 
			// nudAttempts
			// 
			resources.ApplyResources(this.nudAttempts, "nudAttempts");
			this.nudAttempts.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudAttempts.Name = "nudAttempts";
			this.nudAttempts.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// labelAttempts
			// 
			resources.ApplyResources(this.labelAttempts, "labelAttempts");
			this.labelAttempts.Name = "labelAttempts";
			// 
			// dgvPhoneNumbers
			// 
			this.dgvPhoneNumbers.AllowUserToAddRows = false;
			this.dgvPhoneNumbers.AllowUserToDeleteRows = false;
			this.dgvPhoneNumbers.AllowUserToResizeRows = false;
			this.dgvPhoneNumbers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvPhoneNumbers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colActive,
            this.colAuto,
            this.colPhone,
            this.colSerial,
            this.colTime,
            this.colComment,
            this.colDevType});
			resources.ApplyResources(this.dgvPhoneNumbers, "dgvPhoneNumbers");
			this.dgvPhoneNumbers.MultiSelect = false;
			this.dgvPhoneNumbers.Name = "dgvPhoneNumbers";
			this.dgvPhoneNumbers.RowHeadersVisible = false;
			this.dgvPhoneNumbers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvPhoneNumbers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPhoneNumbers_CellClick);
			this.dgvPhoneNumbers.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvPhoneNumbers_CellValidating);
			// 
			// colActive
			// 
			resources.ApplyResources(this.colActive, "colActive");
			this.colActive.Name = "colActive";
			// 
			// colAuto
			// 
			resources.ApplyResources(this.colAuto, "colAuto");
			this.colAuto.Name = "colAuto";
			this.colAuto.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// colPhone
			// 
			resources.ApplyResources(this.colPhone, "colPhone");
			this.colPhone.Name = "colPhone";
			// 
			// colSerial
			// 
			resources.ApplyResources(this.colSerial, "colSerial");
			this.colSerial.Name = "colSerial";
			// 
			// colTime
			// 
			resources.ApplyResources(this.colTime, "colTime");
			this.colTime.Name = "colTime";
			this.colTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// colComment
			// 
			this.colComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.colComment, "colComment");
			this.colComment.Name = "colComment";
			// 
			// colDevType
			// 
			resources.ApplyResources(this.colDevType, "colDevType");
			this.colDevType.Name = "colDevType";
			this.colDevType.ReadOnly = true;
			// 
			// btnRemove
			// 
			resources.ApplyResources(this.btnRemove, "btnRemove");
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// btnAdd
			// 
			resources.ApplyResources(this.btnAdd, "btnAdd");
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// tabGPRS
			// 
			this.tabGPRS.Controls.Add(this.btnRemoveGPRS);
			this.tabGPRS.Controls.Add(this.btnAddGPRS);
			this.tabGPRS.Controls.Add(this.dgvGPRS);
			resources.ApplyResources(this.tabGPRS, "tabGPRS");
			this.tabGPRS.Name = "tabGPRS";
			this.tabGPRS.UseVisualStyleBackColor = true;
			// 
			// btnRemoveGPRS
			// 
			resources.ApplyResources(this.btnRemoveGPRS, "btnRemoveGPRS");
			this.btnRemoveGPRS.Name = "btnRemoveGPRS";
			this.btnRemoveGPRS.UseVisualStyleBackColor = true;
			this.btnRemoveGPRS.Click += new System.EventHandler(this.btnRemoveGPRS_Click);
			// 
			// btnAddGPRS
			// 
			resources.ApplyResources(this.btnAddGPRS, "btnAddGPRS");
			this.btnAddGPRS.Name = "btnAddGPRS";
			this.btnAddGPRS.UseVisualStyleBackColor = true;
			this.btnAddGPRS.Click += new System.EventHandler(this.btnAddGPRS_Click);
			// 
			// dgvGPRS
			// 
			this.dgvGPRS.AllowUserToAddRows = false;
			this.dgvGPRS.AllowUserToDeleteRows = false;
			this.dgvGPRS.AllowUserToResizeRows = false;
			this.dgvGPRS.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvGPRS.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colActiveGPRS,
            this.colAutoGPRS,
            this.colIPAddressGPRS,
            this.colPortGPRS,
            this.colSerialGPRS,
            this.colStartGPRS,
            this.colCommentGPRS,
            this.colDevTypeGPRS});
			resources.ApplyResources(this.dgvGPRS, "dgvGPRS");
			this.dgvGPRS.MultiSelect = false;
			this.dgvGPRS.Name = "dgvGPRS";
			this.dgvGPRS.RowHeadersVisible = false;
			this.dgvGPRS.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvGPRS.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvGPRS_CellClick);
			this.dgvGPRS.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvGPRS_CellValidating);
			// 
			// colActiveGPRS
			// 
			resources.ApplyResources(this.colActiveGPRS, "colActiveGPRS");
			this.colActiveGPRS.Name = "colActiveGPRS";
			// 
			// colAutoGPRS
			// 
			resources.ApplyResources(this.colAutoGPRS, "colAutoGPRS");
			this.colAutoGPRS.Name = "colAutoGPRS";
			this.colAutoGPRS.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// colIPAddressGPRS
			// 
			resources.ApplyResources(this.colIPAddressGPRS, "colIPAddressGPRS");
			this.colIPAddressGPRS.Name = "colIPAddressGPRS";
			// 
			// colPortGPRS
			// 
			resources.ApplyResources(this.colPortGPRS, "colPortGPRS");
			this.colPortGPRS.Name = "colPortGPRS";
			// 
			// colSerialGPRS
			// 
			resources.ApplyResources(this.colSerialGPRS, "colSerialGPRS");
			this.colSerialGPRS.Name = "colSerialGPRS";
			// 
			// colStartGPRS
			// 
			resources.ApplyResources(this.colStartGPRS, "colStartGPRS");
			this.colStartGPRS.Name = "colStartGPRS";
			this.colStartGPRS.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// colCommentGPRS
			// 
			this.colCommentGPRS.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.colCommentGPRS, "colCommentGPRS");
			this.colCommentGPRS.Name = "colCommentGPRS";
			// 
			// colDevTypeGPRS
			// 
			resources.ApplyResources(this.colDevTypeGPRS, "colDevTypeGPRS");
			this.colDevTypeGPRS.Name = "colDevTypeGPRS";
			this.colDevTypeGPRS.ReadOnly = true;
			// 
			// tabEthernet
			// 
			this.tabEthernet.Controls.Add(this.btnRemoveE);
			this.tabEthernet.Controls.Add(this.btnAddE);
			this.tabEthernet.Controls.Add(this.dgvEthernet);
			resources.ApplyResources(this.tabEthernet, "tabEthernet");
			this.tabEthernet.Name = "tabEthernet";
			this.tabEthernet.UseVisualStyleBackColor = true;
			// 
			// btnRemoveE
			// 
			resources.ApplyResources(this.btnRemoveE, "btnRemoveE");
			this.btnRemoveE.Name = "btnRemoveE";
			this.btnRemoveE.UseVisualStyleBackColor = true;
			this.btnRemoveE.Click += new System.EventHandler(this.btnRemoveE_Click);
			// 
			// btnAddE
			// 
			resources.ApplyResources(this.btnAddE, "btnAddE");
			this.btnAddE.Name = "btnAddE";
			this.btnAddE.UseVisualStyleBackColor = true;
			this.btnAddE.Click += new System.EventHandler(this.btnAddE_Click);
			// 
			// dgvEthernet
			// 
			this.dgvEthernet.AllowUserToAddRows = false;
			this.dgvEthernet.AllowUserToDeleteRows = false;
			this.dgvEthernet.AllowUserToResizeRows = false;
			this.dgvEthernet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvEthernet.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colActiveE,
            this.colAutoE,
            this.colIPAddress,
            this.colPort,
            this.colSerialE,
            this.colStartE,
            this.colCommentE,
            this.colDevTypeE});
			resources.ApplyResources(this.dgvEthernet, "dgvEthernet");
			this.dgvEthernet.MultiSelect = false;
			this.dgvEthernet.Name = "dgvEthernet";
			this.dgvEthernet.RowHeadersVisible = false;
			this.dgvEthernet.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvEthernet.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvEthernet_CellClick);
			this.dgvEthernet.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvEthernet_CellValidating);
			// 
			// colActiveE
			// 
			resources.ApplyResources(this.colActiveE, "colActiveE");
			this.colActiveE.Name = "colActiveE";
			// 
			// colAutoE
			// 
			resources.ApplyResources(this.colAutoE, "colAutoE");
			this.colAutoE.Name = "colAutoE";
			this.colAutoE.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// colIPAddress
			// 
			resources.ApplyResources(this.colIPAddress, "colIPAddress");
			this.colIPAddress.Name = "colIPAddress";
			// 
			// colPort
			// 
			resources.ApplyResources(this.colPort, "colPort");
			this.colPort.Name = "colPort";
			// 
			// colSerialE
			// 
			resources.ApplyResources(this.colSerialE, "colSerialE");
			this.colSerialE.Name = "colSerialE";
			this.colSerialE.ReadOnly = true;
			// 
			// colStartE
			// 
			resources.ApplyResources(this.colStartE, "colStartE");
			this.colStartE.Name = "colStartE";
			this.colStartE.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// colCommentE
			// 
			this.colCommentE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.colCommentE, "colCommentE");
			this.colCommentE.Name = "colCommentE";
			// 
			// colDevTypeE
			// 
			resources.ApplyResources(this.colDevTypeE, "colDevTypeE");
			this.colDevTypeE.Name = "colDevTypeE";
			this.colDevTypeE.ReadOnly = true;
			// 
			// tabRs485
			// 
			this.tabRs485.Controls.Add(this.labelSpeed485);
			this.tabRs485.Controls.Add(this.cmbSerialSpeed485);
			this.tabRs485.Controls.Add(this.labelPort485);
			this.tabRs485.Controls.Add(this.cmbSerialPort485);
			this.tabRs485.Controls.Add(this.btnRemove485);
			this.tabRs485.Controls.Add(this.btnAdd485);
			this.tabRs485.Controls.Add(this.dgv485);
			resources.ApplyResources(this.tabRs485, "tabRs485");
			this.tabRs485.Name = "tabRs485";
			this.tabRs485.UseVisualStyleBackColor = true;
			// 
			// labelSpeed485
			// 
			resources.ApplyResources(this.labelSpeed485, "labelSpeed485");
			this.labelSpeed485.Name = "labelSpeed485";
			// 
			// cmbSerialSpeed485
			// 
			this.cmbSerialSpeed485.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialSpeed485.FormattingEnabled = true;
			this.cmbSerialSpeed485.Items.AddRange(new object[] {
            resources.GetString("cmbSerialSpeed485.Items"),
            resources.GetString("cmbSerialSpeed485.Items1"),
            resources.GetString("cmbSerialSpeed485.Items2"),
            resources.GetString("cmbSerialSpeed485.Items3"),
            resources.GetString("cmbSerialSpeed485.Items4"),
            resources.GetString("cmbSerialSpeed485.Items5"),
            resources.GetString("cmbSerialSpeed485.Items6"),
            resources.GetString("cmbSerialSpeed485.Items7")});
			resources.ApplyResources(this.cmbSerialSpeed485, "cmbSerialSpeed485");
			this.cmbSerialSpeed485.Name = "cmbSerialSpeed485";
			this.cmbSerialSpeed485.SelectedIndexChanged += new System.EventHandler(this.cmbSerialSpeed485_SelectedIndexChanged);
			// 
			// labelPort485
			// 
			resources.ApplyResources(this.labelPort485, "labelPort485");
			this.labelPort485.Name = "labelPort485";
			// 
			// cmbSerialPort485
			// 
			this.cmbSerialPort485.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialPort485.FormattingEnabled = true;
			resources.ApplyResources(this.cmbSerialPort485, "cmbSerialPort485");
			this.cmbSerialPort485.Name = "cmbSerialPort485";
			this.cmbSerialPort485.SelectedIndexChanged += new System.EventHandler(this.cmbSerialPort485_SelectedIndexChanged);
			// 
			// btnRemove485
			// 
			resources.ApplyResources(this.btnRemove485, "btnRemove485");
			this.btnRemove485.Name = "btnRemove485";
			this.btnRemove485.UseVisualStyleBackColor = true;
			this.btnRemove485.Click += new System.EventHandler(this.btnRemove485_Click);
			// 
			// btnAdd485
			// 
			resources.ApplyResources(this.btnAdd485, "btnAdd485");
			this.btnAdd485.Name = "btnAdd485";
			this.btnAdd485.UseVisualStyleBackColor = true;
			this.btnAdd485.Click += new System.EventHandler(this.btnAdd485_Click);
			// 
			// dgv485
			// 
			this.dgv485.AllowUserToAddRows = false;
			this.dgv485.AllowUserToDeleteRows = false;
			this.dgv485.AllowUserToResizeRows = false;
			this.dgv485.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgv485.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colActive485,
            this.colAuto485,
            this.colSerial485,
            this.colStart485,
            this.colComment485,
            this.colDevType485});
			resources.ApplyResources(this.dgv485, "dgv485");
			this.dgv485.MultiSelect = false;
			this.dgv485.Name = "dgv485";
			this.dgv485.RowHeadersVisible = false;
			this.dgv485.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgv485.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv485_CellClick);
			this.dgv485.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgv485_CellValidating);
			// 
			// colActive485
			// 
			resources.ApplyResources(this.colActive485, "colActive485");
			this.colActive485.Name = "colActive485";
			// 
			// colAuto485
			// 
			resources.ApplyResources(this.colAuto485, "colAuto485");
			this.colAuto485.Name = "colAuto485";
			this.colAuto485.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// colSerial485
			// 
			resources.ApplyResources(this.colSerial485, "colSerial485");
			this.colSerial485.Name = "colSerial485";
			// 
			// colStart485
			// 
			resources.ApplyResources(this.colStart485, "colStart485");
			this.colStart485.Name = "colStart485";
			this.colStart485.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// colComment485
			// 
			this.colComment485.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.colComment485, "colComment485");
			this.colComment485.Name = "colComment485";
			// 
			// colDevType485
			// 
			resources.ApplyResources(this.colDevType485, "colDevType485");
			this.colDevType485.Name = "colDevType485";
			this.colDevType485.ReadOnly = true;
			// 
			// tabWifi
			// 
			this.tabWifi.Controls.Add(this.btnConnect);
			this.tabWifi.Controls.Add(this.labelCurWiifiPofile);
			this.tabWifi.Controls.Add(this.tbCurWifiProfile);
			this.tabWifi.Controls.Add(this.labelWifiPassword);
			this.tabWifi.Controls.Add(this.tbWifiPassword);
			this.tabWifi.Controls.Add(this.gbConnections);
			resources.ApplyResources(this.tabWifi, "tabWifi");
			this.tabWifi.Name = "tabWifi";
			this.tabWifi.UseVisualStyleBackColor = true;
			// 
			// btnConnect
			// 
			resources.ApplyResources(this.btnConnect, "btnConnect");
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// labelCurWiifiPofile
			// 
			resources.ApplyResources(this.labelCurWiifiPofile, "labelCurWiifiPofile");
			this.labelCurWiifiPofile.Name = "labelCurWiifiPofile";
			// 
			// tbCurWifiProfile
			// 
			resources.ApplyResources(this.tbCurWifiProfile, "tbCurWifiProfile");
			this.tbCurWifiProfile.Name = "tbCurWifiProfile";
			this.tbCurWifiProfile.ReadOnly = true;
			// 
			// labelWifiPassword
			// 
			resources.ApplyResources(this.labelWifiPassword, "labelWifiPassword");
			this.labelWifiPassword.Name = "labelWifiPassword";
			// 
			// tbWifiPassword
			// 
			resources.ApplyResources(this.tbWifiPassword, "tbWifiPassword");
			this.tbWifiPassword.Name = "tbWifiPassword";
			this.tbWifiPassword.TextChanged += new System.EventHandler(this.tbWifiPassword_TextChanged);
			// 
			// gbConnections
			// 
			this.gbConnections.Controls.Add(this.btnRefresh);
			this.gbConnections.Controls.Add(this.lvConnections);
			resources.ApplyResources(this.gbConnections, "gbConnections");
			this.gbConnections.Name = "gbConnections";
			this.gbConnections.TabStop = false;
			// 
			// btnRefresh
			// 
			resources.ApplyResources(this.btnRefresh, "btnRefresh");
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// lvConnections
			// 
			this.lvConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colWifiName,
            this.colWifiSignal});
			this.lvConnections.FullRowSelect = true;
			this.lvConnections.GridLines = true;
			this.lvConnections.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			resources.ApplyResources(this.lvConnections, "lvConnections");
			this.lvConnections.MultiSelect = false;
			this.lvConnections.Name = "lvConnections";
			this.lvConnections.UseCompatibleStateImageBehavior = false;
			this.lvConnections.View = System.Windows.Forms.View.Details;
			this.lvConnections.SelectedIndexChanged += new System.EventHandler(this.lvConnections_SelectedIndexChanged);
			// 
			// colWifiName
			// 
			resources.ApplyResources(this.colWifiName, "colWifiName");
			// 
			// colWifiSignal
			// 
			resources.ApplyResources(this.colWifiSignal, "colWifiSignal");
			// 
			// gbxSerial
			// 
			this.gbxSerial.Controls.Add(this.progressBar);
			this.gbxSerial.Controls.Add(this.lblSelectSerialPort);
			this.gbxSerial.Controls.Add(this.lblSelectSerialPortSpeed);
			this.gbxSerial.Controls.Add(this.btnDetect);
			this.gbxSerial.Controls.Add(this.cmbSerialPort);
			this.gbxSerial.Controls.Add(this.cmbSerialPortSpeed);
			resources.ApplyResources(this.gbxSerial, "gbxSerial");
			this.gbxSerial.Name = "gbxSerial";
			this.gbxSerial.TabStop = false;
			// 
			// progressBar
			// 
			resources.ApplyResources(this.progressBar, "progressBar");
			this.progressBar.Name = "progressBar";
			// 
			// lblSelectSerialPort
			// 
			resources.ApplyResources(this.lblSelectSerialPort, "lblSelectSerialPort");
			this.lblSelectSerialPort.Name = "lblSelectSerialPort";
			// 
			// lblSelectSerialPortSpeed
			// 
			resources.ApplyResources(this.lblSelectSerialPortSpeed, "lblSelectSerialPortSpeed");
			this.lblSelectSerialPortSpeed.Name = "lblSelectSerialPortSpeed";
			// 
			// btnDetect
			// 
			resources.ApplyResources(this.btnDetect, "btnDetect");
			this.btnDetect.Name = "btnDetect";
			this.toolTipHost.SetToolTip(this.btnDetect, resources.GetString("btnDetect.ToolTip"));
			this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
			// 
			// cmbSerialPort
			// 
			this.cmbSerialPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialPort.FormattingEnabled = true;
			resources.ApplyResources(this.cmbSerialPort, "cmbSerialPort");
			this.cmbSerialPort.Name = "cmbSerialPort";
			this.cmbSerialPort.SelectedIndexChanged += new System.EventHandler(this.cmbSerialPort_SelectedIndexChanged);
			// 
			// cmbSerialPortSpeed
			// 
			this.cmbSerialPortSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSerialPortSpeed.FormattingEnabled = true;
			this.cmbSerialPortSpeed.Items.AddRange(new object[] {
            resources.GetString("cmbSerialPortSpeed.Items"),
            resources.GetString("cmbSerialPortSpeed.Items1"),
            resources.GetString("cmbSerialPortSpeed.Items2"),
            resources.GetString("cmbSerialPortSpeed.Items3"),
            resources.GetString("cmbSerialPortSpeed.Items4"),
            resources.GetString("cmbSerialPortSpeed.Items5"),
            resources.GetString("cmbSerialPortSpeed.Items6"),
            resources.GetString("cmbSerialPortSpeed.Items7")});
			resources.ApplyResources(this.cmbSerialPortSpeed, "cmbSerialPortSpeed");
			this.cmbSerialPortSpeed.Name = "cmbSerialPortSpeed";
			this.cmbSerialPortSpeed.SelectedIndexChanged += new System.EventHandler(this.cmbSerialPortSpeed_SelectedIndexChanged);
			// 
			// gbxInterface
			// 
			this.gbxInterface.Controls.Add(this.rbWifi);
			this.gbxInterface.Controls.Add(this.rbtnModemGPRS);
			this.gbxInterface.Controls.Add(this.rbRs485);
			this.gbxInterface.Controls.Add(this.rbEthernet);
			this.gbxInterface.Controls.Add(this.rbtnModemGSM);
			this.gbxInterface.Controls.Add(this.rbtnUSB);
			this.gbxInterface.Controls.Add(this.rbtnCOM);
			resources.ApplyResources(this.gbxInterface, "gbxInterface");
			this.gbxInterface.Name = "gbxInterface";
			this.gbxInterface.TabStop = false;
			// 
			// rbWifi
			// 
			resources.ApplyResources(this.rbWifi, "rbWifi");
			this.rbWifi.Name = "rbWifi";
			this.rbWifi.UseVisualStyleBackColor = true;
			this.rbWifi.CheckedChanged += new System.EventHandler(this.rbtn_CheckedChanged);
			// 
			// rbtnModemGPRS
			// 
			resources.ApplyResources(this.rbtnModemGPRS, "rbtnModemGPRS");
			this.rbtnModemGPRS.Name = "rbtnModemGPRS";
			this.rbtnModemGPRS.UseVisualStyleBackColor = true;
			// 
			// rbRs485
			// 
			resources.ApplyResources(this.rbRs485, "rbRs485");
			this.rbRs485.Name = "rbRs485";
			this.rbRs485.UseVisualStyleBackColor = true;
			this.rbRs485.CheckedChanged += new System.EventHandler(this.rbtn_CheckedChanged);
			// 
			// rbEthernet
			// 
			resources.ApplyResources(this.rbEthernet, "rbEthernet");
			this.rbEthernet.Name = "rbEthernet";
			this.rbEthernet.UseVisualStyleBackColor = true;
			this.rbEthernet.CheckedChanged += new System.EventHandler(this.rbtn_CheckedChanged);
			// 
			// rbtnModemGSM
			// 
			resources.ApplyResources(this.rbtnModemGSM, "rbtnModemGSM");
			this.rbtnModemGSM.Name = "rbtnModemGSM";
			this.rbtnModemGSM.UseVisualStyleBackColor = true;
			this.rbtnModemGSM.CheckedChanged += new System.EventHandler(this.rbtn_CheckedChanged);
			// 
			// rbtnUSB
			// 
			resources.ApplyResources(this.rbtnUSB, "rbtnUSB");
			this.rbtnUSB.Name = "rbtnUSB";
			this.rbtnUSB.UseVisualStyleBackColor = true;
			this.rbtnUSB.CheckedChanged += new System.EventHandler(this.rbtn_CheckedChanged);
			// 
			// rbtnCOM
			// 
			resources.ApplyResources(this.rbtnCOM, "rbtnCOM");
			this.rbtnCOM.Checked = true;
			this.rbtnCOM.Name = "rbtnCOM";
			this.rbtnCOM.TabStop = true;
			this.rbtnCOM.UseVisualStyleBackColor = true;
			this.rbtnCOM.CheckedChanged += new System.EventHandler(this.rbtn_CheckedChanged);
			// 
			// tabDeviceManager
			// 
			this.tabDeviceManager.Controls.Add(this.listBoxAvailable);
			this.tabDeviceManager.Controls.Add(this.labelLicDevices);
			this.tabDeviceManager.Controls.Add(this.listBoxInstalled);
			this.tabDeviceManager.Controls.Add(this.lblInstalledDevices);
			this.tabDeviceManager.Controls.Add(this.btnDropLicence);
			this.tabDeviceManager.Controls.Add(this.btnAddLicence);
			this.tabDeviceManager.Controls.Add(this.btnOpenLicenseFile);
			resources.ApplyResources(this.tabDeviceManager, "tabDeviceManager");
			this.tabDeviceManager.Name = "tabDeviceManager";
			this.tabDeviceManager.UseVisualStyleBackColor = true;
			// 
			// listBoxAvailable
			// 
			this.listBoxAvailable.BackColor = System.Drawing.SystemColors.Window;
			resources.ApplyResources(this.listBoxAvailable, "listBoxAvailable");
			this.listBoxAvailable.FormattingEnabled = true;
			this.listBoxAvailable.Name = "listBoxAvailable";
			this.listBoxAvailable.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			// 
			// labelLicDevices
			// 
			resources.ApplyResources(this.labelLicDevices, "labelLicDevices");
			this.labelLicDevices.Name = "labelLicDevices";
			// 
			// listBoxInstalled
			// 
			this.listBoxInstalled.FormattingEnabled = true;
			resources.ApplyResources(this.listBoxInstalled, "listBoxInstalled");
			this.listBoxInstalled.Name = "listBoxInstalled";
			// 
			// lblInstalledDevices
			// 
			resources.ApplyResources(this.lblInstalledDevices, "lblInstalledDevices");
			this.lblInstalledDevices.Name = "lblInstalledDevices";
			// 
			// btnDropLicence
			// 
			resources.ApplyResources(this.btnDropLicence, "btnDropLicence");
			this.btnDropLicence.Name = "btnDropLicence";
			this.btnDropLicence.Click += new System.EventHandler(this.btnDropLicence_Click);
			// 
			// btnAddLicence
			// 
			resources.ApplyResources(this.btnAddLicence, "btnAddLicence");
			this.btnAddLicence.Name = "btnAddLicence";
			this.btnAddLicence.Click += new System.EventHandler(this.btnAddLicence_Click);
			// 
			// btnOpenLicenseFile
			// 
			resources.ApplyResources(this.btnOpenLicenseFile, "btnOpenLicenseFile");
			this.btnOpenLicenseFile.Name = "btnOpenLicenseFile";
			this.btnOpenLicenseFile.Click += new System.EventHandler(this.btnOpenLicenseFile_Click);
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnApply
			// 
			resources.ApplyResources(this.btnApply, "btnApply");
			this.btnApply.Name = "btnApply";
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// toolTipHost
			// 
			this.toolTipHost.IsBalloon = true;
			this.toolTipHost.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			// 
			// dlgOpenLicence
			// 
			resources.ApplyResources(this.dlgOpenLicence, "dlgOpenLicence");
			this.dlgOpenLicence.Multiselect = true;
			this.dlgOpenLicence.RestoreDirectory = true;
			this.dlgOpenLicence.SupportMultiDottedExtensions = true;
			// 
			// dataGridViewTextBoxColumn1
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			// 
			// dataGridViewTextBoxColumn2
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			// 
			// calendarColumn1
			// 
			resources.ApplyResources(this.calendarColumn1, "calendarColumn1");
			this.calendarColumn1.Name = "calendarColumn1";
			this.calendarColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.dataGridViewTextBoxColumn3, "dataGridViewTextBoxColumn3");
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			// 
			// dataGridViewTextBoxColumn4
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn4, "dataGridViewTextBoxColumn4");
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn5
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn5, "dataGridViewTextBoxColumn5");
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			// 
			// dataGridViewTextBoxColumn6
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn6, "dataGridViewTextBoxColumn6");
			this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			// 
			// dataGridViewTextBoxColumn7
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn7, "dataGridViewTextBoxColumn7");
			this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
			this.dataGridViewTextBoxColumn7.ReadOnly = true;
			// 
			// calendarColumn2
			// 
			resources.ApplyResources(this.calendarColumn2, "calendarColumn2");
			this.calendarColumn2.Name = "calendarColumn2";
			this.calendarColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewTextBoxColumn8
			// 
			this.dataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.dataGridViewTextBoxColumn8, "dataGridViewTextBoxColumn8");
			this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
			// 
			// dataGridViewTextBoxColumn9
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn9, "dataGridViewTextBoxColumn9");
			this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
			this.dataGridViewTextBoxColumn9.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn10
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn10, "dataGridViewTextBoxColumn10");
			this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
			// 
			// dataGridViewTextBoxColumn11
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn11, "dataGridViewTextBoxColumn11");
			this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
			// 
			// dataGridViewTextBoxColumn12
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn12, "dataGridViewTextBoxColumn12");
			this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
			this.dataGridViewTextBoxColumn12.ReadOnly = true;
			// 
			// calendarColumn3
			// 
			resources.ApplyResources(this.calendarColumn3, "calendarColumn3");
			this.calendarColumn3.Name = "calendarColumn3";
			this.calendarColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewTextBoxColumn13
			// 
			this.dataGridViewTextBoxColumn13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.dataGridViewTextBoxColumn13, "dataGridViewTextBoxColumn13");
			this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
			// 
			// dataGridViewTextBoxColumn14
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn14, "dataGridViewTextBoxColumn14");
			this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
			this.dataGridViewTextBoxColumn14.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn15
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn15, "dataGridViewTextBoxColumn15");
			this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
			// 
			// dataGridViewTextBoxColumn16
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn16, "dataGridViewTextBoxColumn16");
			this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
			this.dataGridViewTextBoxColumn16.ReadOnly = true;
			// 
			// calendarColumn4
			// 
			resources.ApplyResources(this.calendarColumn4, "calendarColumn4");
			this.calendarColumn4.Name = "calendarColumn4";
			this.calendarColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewTextBoxColumn17
			// 
			this.dataGridViewTextBoxColumn17.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.dataGridViewTextBoxColumn17, "dataGridViewTextBoxColumn17");
			this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
			// 
			// dataGridViewTextBoxColumn18
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn18, "dataGridViewTextBoxColumn18");
			this.dataGridViewTextBoxColumn18.Name = "dataGridViewTextBoxColumn18";
			this.dataGridViewTextBoxColumn18.ReadOnly = true;
			// 
			// frmSettings
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.tcOptions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSettings";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSettings_FormClosing);
			this.Load += new System.EventHandler(this.frmSettings_Load);
			this.tcOptions.ResumeLayout(false);
			this.tabCommon.ResumeLayout(false);
			this.gbTime.ResumeLayout(false);
			this.gbNewFirmware.ResumeLayout(false);
			this.gbNewFirmware.PerformLayout();
			this.gbAvgTooltip.ResumeLayout(false);
			this.gbAvgTooltip.PerformLayout();
			this.gbAVGParams.ResumeLayout(false);
			this.gbDevice.ResumeLayout(false);
			this.gbDevice.PerformLayout();
			this.gbServerDB.ResumeLayout(false);
			this.gbServerDB.PerformLayout();
			this.gbAvgColors.ResumeLayout(false);
			this.gbAvgColors.PerformLayout();
			this.gbRatios.ResumeLayout(false);
			this.gbRatios.PerformLayout();
			this.gbLanguage.ResumeLayout(false);
			this.gbDecimalType.ResumeLayout(false);
			this.gbDecimalType.PerformLayout();
			this.tabSerialPort.ResumeLayout(false);
			this.tcConnections.ResumeLayout(false);
			this.tabGSM.ResumeLayout(false);
			this.tabGSM.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudAttempts)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvPhoneNumbers)).EndInit();
			this.tabGPRS.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvGPRS)).EndInit();
			this.tabEthernet.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvEthernet)).EndInit();
			this.tabRs485.ResumeLayout(false);
			this.tabRs485.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgv485)).EndInit();
			this.tabWifi.ResumeLayout(false);
			this.tabWifi.PerformLayout();
			this.gbConnections.ResumeLayout(false);
			this.gbxSerial.ResumeLayout(false);
			this.gbxSerial.PerformLayout();
			this.gbxInterface.ResumeLayout(false);
			this.gbxInterface.PerformLayout();
			this.tabDeviceManager.ResumeLayout(false);
			this.tabDeviceManager.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tcOptions;
        private System.Windows.Forms.TabPage tabSerialPort;
		private System.Windows.Forms.ComboBox cmbSerialPort;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.ComboBox cmbSerialPortSpeed;
		private System.Windows.Forms.TabPage tabCommon;
		private System.Windows.Forms.ComboBox cmbLanguage;
		private System.Windows.Forms.ToolTip toolTipHost;
		private System.Windows.Forms.TabPage tabDeviceManager;
		private System.Windows.Forms.Button btnOpenLicenseFile;
		private System.Windows.Forms.OpenFileDialog dlgOpenLicence;
		private System.Windows.Forms.Button btnAddLicence;
		private System.Windows.Forms.Button btnDropLicence;
		private System.Windows.Forms.GroupBox gbxInterface;
		private System.Windows.Forms.RadioButton rbtnUSB;
		private System.Windows.Forms.RadioButton rbtnCOM;
		private System.Windows.Forms.GroupBox gbxSerial;
		private System.Windows.Forms.ComboBox cmbFloatSigns;
		private System.Windows.Forms.TextBox txtFloatFormatExample;
		private System.Windows.Forms.GroupBox gbDecimalType;
		private System.Windows.Forms.GroupBox gbLanguage;
		private System.Windows.Forms.GroupBox gbRatios;
		private System.Windows.Forms.RadioButton rbV;
		private System.Windows.Forms.RadioButton rbA;
		private System.Windows.Forms.RadioButton rbKA;
		private System.Windows.Forms.RadioButton rbKV;
		private System.Windows.Forms.RadioButton rbKW;
		private System.Windows.Forms.RadioButton rbW;
		private System.Windows.Forms.GroupBox gbAvgColors;
		private System.Windows.Forms.CheckBox chkAvgBrushGradient;
		private System.Windows.Forms.Panel pnlAvgBrushColor2;
		private System.Windows.Forms.Panel pnlAvgBrushColor1;
		private System.Windows.Forms.Label lblAvgGraphsBrushExample;
		private System.Windows.Forms.Panel pnlAvgBrushExample;
		private System.Windows.Forms.RadioButton rbtnModemGSM;
		private System.Windows.Forms.TabControl tcConnections;
		private System.Windows.Forms.TabPage tabGSM;
		private System.Windows.Forms.TabPage tabEthernet;
		private System.Windows.Forms.NumericUpDown nudAttempts;
		private System.Windows.Forms.Label labelAttempts;
		private System.Windows.Forms.DataGridView dgvPhoneNumbers;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.ListBox listBoxAvailable;
		private System.Windows.Forms.Label labelLicDevices;
		private System.Windows.Forms.ListBox listBoxInstalled;
		private System.Windows.Forms.Label lblInstalledDevices;
		private System.Windows.Forms.Button btnDetect;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label lblSelectSerialPort;
		private System.Windows.Forms.Label lblSelectSerialPortSpeed;
		private System.Windows.Forms.RadioButton rbEthernet;
		private System.Windows.Forms.GroupBox gbServerDB;
		private System.Windows.Forms.ComboBox cbServerDB;
		private System.Windows.Forms.DataGridView dgvEthernet;
		private System.Windows.Forms.Button btnRemoveE;
		private System.Windows.Forms.Button btnAddE;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colActiveE;
		private System.Windows.Forms.DataGridViewTextBoxColumn colIPAddress;
		private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSerialE;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colAutoE;
		private CalendarColumn colStartE;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCommentE;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDevTypeE;
		private System.Windows.Forms.Label labelPortModem;
		private System.Windows.Forms.ComboBox cmbSerialPortModem;
		private System.Windows.Forms.TabPage tabRs485;
		private System.Windows.Forms.RadioButton rbRs485;
		private System.Windows.Forms.DataGridView dgv485;
		private System.Windows.Forms.Button btnRemove485;
		private System.Windows.Forms.Button btnAdd485;
		private System.Windows.Forms.Label labelSpeedModem;
		private System.Windows.Forms.ComboBox cmbSerialSpeedModem;
		private System.Windows.Forms.Label labelSpeed485;
		private System.Windows.Forms.ComboBox cmbSerialSpeed485;
		private System.Windows.Forms.Label labelPort485;
		private System.Windows.Forms.ComboBox cmbSerialPort485;
		private System.Windows.Forms.GroupBox gbDevice;
		private System.Windows.Forms.RadioButton rbEm31k;
		private System.Windows.Forms.RadioButton rbEtPQP;
		private System.Windows.Forms.RadioButton rbEm32;
		private System.Windows.Forms.RadioButton rbEm33t;
		private System.Windows.Forms.CheckBox chbOptimizeInsert;
		private System.Windows.Forms.RadioButton rbtnModemGPRS;
		private System.Windows.Forms.TabPage tabGPRS;
		private System.Windows.Forms.Button btnRemoveGPRS;
		private System.Windows.Forms.Button btnAddGPRS;
		private System.Windows.Forms.DataGridView dgvGPRS;
		private System.Windows.Forms.GroupBox gbAVGParams;
		private System.Windows.Forms.Button btnSetAvgParams;
		private System.Windows.Forms.RadioButton rbEtPQP_A;
        private System.Windows.Forms.GroupBox gbAvgTooltip;
        private System.Windows.Forms.RadioButton rbTooltipOff;
        private System.Windows.Forms.RadioButton rbTooltipOn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private CalendarColumn calendarColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private CalendarColumn calendarColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private CalendarColumn calendarColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private CalendarColumn calendarColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colActive;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colAuto;
		private System.Windows.Forms.DataGridViewTextBoxColumn colPhone;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSerial;
		private CalendarColumn colTime;
		private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDevType;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colActiveGPRS;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colAutoGPRS;
		private System.Windows.Forms.DataGridViewTextBoxColumn colIPAddressGPRS;
		private System.Windows.Forms.DataGridViewTextBoxColumn colPortGPRS;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSerialGPRS;
		private CalendarColumn colStartGPRS;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCommentGPRS;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDevTypeGPRS;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colActive485;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colAuto485;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSerial485;
		private CalendarColumn colStart485;
		private System.Windows.Forms.DataGridViewTextBoxColumn colComment485;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDevType485;
		private System.Windows.Forms.GroupBox gbNewFirmware;
		private System.Windows.Forms.CheckBox chbNewFirmware;
		private System.Windows.Forms.GroupBox gbTime;
		private System.Windows.Forms.CheckBox chbAutoTimeWarn;
		private System.Windows.Forms.CheckBox chbNewSoftware;
		private System.Windows.Forms.RadioButton rbWifi;
		private System.Windows.Forms.TabPage tabWifi;
		private System.Windows.Forms.GroupBox gbConnections;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.ListView lvConnections;
		private System.Windows.Forms.ColumnHeader colWifiName;
		private System.Windows.Forms.ColumnHeader colWifiSignal;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Label labelWifiPassword;
		private System.Windows.Forms.TextBox tbWifiPassword;
		private System.Windows.Forms.Label labelCurWiifiPofile;
		private System.Windows.Forms.TextBox tbCurWifiProfile;


	}
}