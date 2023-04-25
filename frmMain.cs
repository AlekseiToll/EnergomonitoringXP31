using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Resources;
using System.IO;
using System.Data;
using System.IO.Ports;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;

using WeifenLuo.WinFormsUI;
using EnergomonitoringXP.ArchiveTreeView;
using ZedGraph;
using EmDataSaver;
using EmDataSaver.SavingInterface;
using EmDataSaver.SqlImage;
using EmServiceLib;
using EmServiceLib.SavingInterface;
using DataGridColumnStyles;
using DeviceIO;
using EmDataSaver.XmlImage;
using EmArchiveTree;
using Microsoft.Win32;

namespace EnergomonitoringXP
{
	public enum EmAction
	{
		SaveFromDevice,
		SaveFromFile
	}

	/// <summary>
	/// frmMain - the main form's class.
	/// </summary>
	public class frmMain : Form
	{
		#region Fields

		private ArchiveInfo curArchive_;

		// константы для ловли события дисконнекта с прибором
		//public const int WM_DEVICECHANGE = 0x0219;
		//public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
		//public const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;

		// this flag is true when some reading, image creating or saving is in process
		// and in this case confirmation is needed to close the program
		private bool needConfirmClose_ = false;

		/// <summary>
		/// Object to store application settings
		/// </summary>
		private Settings settings_ = new Settings();

		// текущий тип устройства. при считывании он равен указанноу в нстройках, при 
		// открытии файла он зависит от типа файла
		private EmDeviceType currentDevType_ = EmDeviceType.NONE;
		private EmAction currentAction_;

		// это поле нужно для изменения прогрессбара при чтении средних пакетами, т.к.
		// в этом случае сообщение посылается из с++ dll прямо главному окну и мы не можем
		// рассчитать процент в классе EmDevice
		private double reader_percent_for_one_step_ = 0;
		private double reader_cur_percent_ = 0;
		private int reader_prev_percent_ = 0;
		
		private EmDataSaver33 saverEm33_ = null;  // for Em33T and Em31K
		private EmDataSaver32 saverEm32_ = null;  // for Em32
		private EtDataSaverPQP saverEtPQP_ = null;  // for EtPQP
		private EtDataSaverPQP_A saverEtPQP_A_ = null;  // for EtPQP-A
		private BackgroundWorker bwSaver_ = new BackgroundWorker();

		private EmSqlImageCreator33 imCreateEm33_ = null;  // for Em33T and Em31K
		private EmSqlImageCreator32 imCreateEm32_ = null;  // for Em32
		private EtSqlImageCreatorPQP imCreateEtPQP_ = null;  // for EtPQP
		private EtSqlImageCreatorPQP_A imCreateEtPQP_A_ = null;  // for EtPQP-A
		private BackgroundWorker bwImCreator_ = new BackgroundWorker();
		private EmSqlDeviceImage sqlImageEm33_;
		private EmSqlEm32Device sqlImageEm32_;
		private EtPQPSqlDeviceImage sqlImageEtPQP_;
		private EtPQP_A_SqlDeviceImage sqlImageEtPQP_A_;

		private EmDataReader33 readerEm33_ = null;  // for Em33T and Em31K
		private EmDataReader32 readerEm32_ = null;  // for Em32
		private EtDataReaderPQP readerEtPQP_ = null;  // for EtPQP
		private EtDataReaderPQP_A readerEtPQP_A_ = null;  // for EtPQP-A
		private BackgroundWorker bwReader_ = new BackgroundWorker();
		private Em32XmlDeviceImage xmlImageEm32_;
		private EmXmlDeviceImage xmlImageEm33_;
		private EtPQPXmlDeviceImage xmlImageEtPQP_;
		private EtPQP_A_XmlDeviceImage xmlImageEtPQP_A_;

		private EmDataExportBase exporter_ = null; 
		private BackgroundWorker bwExporter_ = new BackgroundWorker();

		private bool bCreateImageOnly_ = false;

        // поток, отвечающий за автоматический опрос приборов
        private AutoDialThread autoDial_;
        // если этот флаг установлен, программа готова к работе в автоматич.режиме
        private static bool bAutoMode_ = true;
        // объект класса, содержащего очереди автоматич.опроса
        private AutoConnect autoConnectQueues_ = new AutoConnect();
		private Thread AutoDialModemThread_ = null;

		/// <summary>Database tree window</summary>
		internal frmToolbox wndToolbox;
		/// <summary>PQP window</summary>
		internal frmDocPQP wndDocPQP;
		/// <summary>Average window</summary>
		internal frmDocAVG wndDocAVG;
		/// <summary>Dips and overs window</summary>
		internal frmDocDNS wndDocDNS;

		// форма сообщения о том, что при считывании ПКЭ длительность получилась 
		// больше суток
		private frmMessage frmInvDurMess;

		private StatusStrip MyStatusStrip;
		private ToolStripButton tbsSettings;
		private ToolStripButton tbsConstraints;
		private ToolStripButton tbsHelp;

        private ToolStripMenuItem msFile;
		private ToolStripMenuItem msFileLoadFromDevice;
        private ToolStripSeparator msFileSeparator1;
        private ToolStripMenuItem msFileExit;
        private ToolStripMenuItem msTools;
		private ToolStripMenuItem msService;
        private ToolStripMenuItem msHelp;
        private ToolStripMenuItem msHelpAbout;
        private ToolStripMenuItem msServiceLanguage;
        private ToolStripMenuItem msServiceLangEnglish;
        private ToolStripMenuItem msServiceLangRussian;
		private MenuStrip MyMainMenuStrip;
		private ToolStrip MainToolStrip;
        private ToolStripButton tsbExchangeWithDevice;
        private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem msToolsConstraints;
		private ToolStripMenuItem msToolsObjectNames;
		private ToolStripMenuItem msToolsNominalValues;
		private ToolStripMenuItem miExportToExcel;
		private ToolStripProgressBar tsProgressExportExcel;
		/// <summary>Menu item "Export AVG to Excel"</summary>
		internal ToolStripMenuItem msToolsExportToExcelAVG;
		/// <summary>Menu item "Export PQP to Excel"</summary>
		internal ToolStripMenuItem msToolsExportToExcelPQP;
		internal ToolStripMenuItem msToolsExportToExcelDNS;

		private ToolStripProgressBar tsProgressSaving;
		private ToolStripStatusLabel tsLabelSavingMode;
		private ToolStripStatusLabel tsLabelExportExcel;
		private ToolStripStatusLabel tsLabelCurArchSaving;
		private ToolStripMenuItem msHelpHelp;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem miReportPQP;
		private ToolTip MyToolTip;
		private ToolStripDropDownButton tsAbortSaving;
		private ToolStripMenuItem tsMiCancelSaving;
		private ToolStripMenuItem msFileOpenImageFile;
		private ToolStripSeparator msFileSeparator2;
		private ToolStripSeparator miHelpSeparator;
		private ToolStripStatusLabel tsConnectOptions;
		private ToolStripButton tsbMomentData;
		private ToolStripMenuItem msServiceSettings;
		private ToolStripMenuItem msToolsDevName;
		private ToolStripMenuItem miReportGOST;
		private ToolStripMenuItem miReportRD;
		private ToolStripMenuItem miReportFSK;
		private ToolStripSeparator miToolsSeparator;
		private ToolStripMenuItem miReportEtPqpA;
		private Panel panelConnect;
		private ToolTip toolTipHost;
		private ToolStripMenuItem miEditReportTemplates;
		private ToolStripMenuItem miFeedback;
		private SplitContainer splitContainerMain;
		internal DockPanel dockPanel;
		private SplitContainer splitContainerArchiveInfo;
		private Label labelArchiveInfo;
		private Button btnArchiveClose;
		private ToolStripMenuItem msToolsDeclaredU;
		private ToolStripMenuItem miReportEtPqpA_v2;
		private ToolStripMenuItem miReportEtPqpA_v3;
		private ToolStripMenuItem msToolsExportToExcelEvents;

		private System.ComponentModel.IContainer components;

		// закрытие окошка с надписью "идет закрытие порта"
		public delegate void HideWndInfoHandler();
		public event HideWndInfoHandler OnHideWndInfo;

		// послать потоку обмена с устройством сигнал о прекращении обмена (чтобы убить поток)
		//public delegate void StopIOHandler();
		//public event StopIOHandler OnStopIO;

		// переменные показывают было ли обновление БД (чтобы не делать его несколько раз
		// за один запуск программы)
		//private bool bDbUpdated = false;

		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		public frmMain(Settings settings)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.settings_ = settings;

			if (settings.Language.Equals("English"))
			{
				msServiceLangEnglish.Checked = true;
			}
			if (settings.Language.Equals("Русский"))
			{				
				msServiceLangRussian.Checked = true;
			}
			
			wndToolbox = new frmToolbox(this, settings);
			//wndDocPQP = new frmDocPQP(this, settings);
			//wndDocAVG = new frmDocAVG(this, settings, EmDeviceType.NONE);
			//wndDocDNS = new frmDocDNS(this, settings);

			CheckForIllegalCrossThreadCalls = false;

			string cap = this.ProductVersion;//.Remove(this.ProductVersion.LastIndexOf('.'));
			this.Text += cap;

			msToolsDevName.Visible = settings_.CurDeviceType == EmDeviceType.EM32;
            msToolsObjectNames.Visible =
                    !(settings_.CurDeviceType == EmDeviceType.EM32);
            msToolsNominalValues.Visible = !(settings_.CurDeviceType == EmDeviceType.ETPQP_A ||
                settings_.CurDeviceType == EmDeviceType.ETPQP);
			msToolsDeclaredU.Visible = (settings_.CurDeviceType == EmDeviceType.ETPQP_A);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.splitContainerArchiveInfo = new System.Windows.Forms.SplitContainer();
            this.labelArchiveInfo = new System.Windows.Forms.Label();
            this.btnArchiveClose = new System.Windows.Forms.Button();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.dockPanel = new WeifenLuo.WinFormsUI.DockPanel();
            this.MyStatusStrip = new System.Windows.Forms.StatusStrip();
            this.tsLabelExportExcel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgressExportExcel = new System.Windows.Forms.ToolStripProgressBar();
            this.tsLabelSavingMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsLabelCurArchSaving = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgressSaving = new System.Windows.Forms.ToolStripProgressBar();
            this.tsAbortSaving = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsMiCancelSaving = new System.Windows.Forms.ToolStripMenuItem();
            this.tsConnectOptions = new System.Windows.Forms.ToolStripStatusLabel();
            this.MyMainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.msFile = new System.Windows.Forms.ToolStripMenuItem();
            this.msFileLoadFromDevice = new System.Windows.Forms.ToolStripMenuItem();
            this.msFileSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.msFileOpenImageFile = new System.Windows.Forms.ToolStripMenuItem();
            this.msFileSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.msFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.msTools = new System.Windows.Forms.ToolStripMenuItem();
            this.miExportToExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsExportToExcelPQP = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsExportToExcelAVG = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsExportToExcelDNS = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsExportToExcelEvents = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportPQP = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportGOST = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportRD = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportFSK = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportEtPqpA = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportEtPqpA_v2 = new System.Windows.Forms.ToolStripMenuItem();
            this.miReportEtPqpA_v3 = new System.Windows.Forms.ToolStripMenuItem();
            this.miEditReportTemplates = new System.Windows.Forms.ToolStripMenuItem();
            this.miToolsSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.msToolsConstraints = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsObjectNames = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsDevName = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsNominalValues = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolsDeclaredU = new System.Windows.Forms.ToolStripMenuItem();
            this.msService = new System.Windows.Forms.ToolStripMenuItem();
            this.msServiceLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.msServiceLangEnglish = new System.Windows.Forms.ToolStripMenuItem();
            this.msServiceLangRussian = new System.Windows.Forms.ToolStripMenuItem();
            this.msServiceSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.msHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.msHelpHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miFeedback = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.msHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.MainToolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbExchangeWithDevice = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbsSettings = new System.Windows.Forms.ToolStripButton();
            this.tbsConstraints = new System.Windows.Forms.ToolStripButton();
            this.tbsHelp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbMomentData = new System.Windows.Forms.ToolStripButton();
            this.MyToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelConnect = new System.Windows.Forms.Panel();
            this.toolTipHost = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerArchiveInfo)).BeginInit();
            this.splitContainerArchiveInfo.Panel1.SuspendLayout();
            this.splitContainerArchiveInfo.Panel2.SuspendLayout();
            this.splitContainerArchiveInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.MyStatusStrip.SuspendLayout();
            this.MyMainMenuStrip.SuspendLayout();
            this.MainToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerArchiveInfo
            // 
            resources.ApplyResources(this.splitContainerArchiveInfo, "splitContainerArchiveInfo");
            this.splitContainerArchiveInfo.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerArchiveInfo.Name = "splitContainerArchiveInfo";
            // 
            // splitContainerArchiveInfo.Panel1
            // 
            resources.ApplyResources(this.splitContainerArchiveInfo.Panel1, "splitContainerArchiveInfo.Panel1");
            this.splitContainerArchiveInfo.Panel1.Controls.Add(this.labelArchiveInfo);
            // 
            // splitContainerArchiveInfo.Panel2
            // 
            this.splitContainerArchiveInfo.Panel2.Controls.Add(this.btnArchiveClose);
            // 
            // labelArchiveInfo
            // 
            resources.ApplyResources(this.labelArchiveInfo, "labelArchiveInfo");
            this.labelArchiveInfo.BackColor = System.Drawing.SystemColors.Control;
            this.labelArchiveInfo.Name = "labelArchiveInfo";
            // 
            // btnArchiveClose
            // 
            resources.ApplyResources(this.btnArchiveClose, "btnArchiveClose");
            this.btnArchiveClose.Name = "btnArchiveClose";
            this.btnArchiveClose.UseVisualStyleBackColor = true;
            this.btnArchiveClose.Click += new System.EventHandler(this.btnArchiveClose_Click);
            // 
            // splitContainerMain
            // 
            resources.ApplyResources(this.splitContainerMain, "splitContainerMain");
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerArchiveInfo);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.dockPanel);
            // 
            // dockPanel
            // 
            this.dockPanel.ActiveAutoHideContent = null;
            resources.ApplyResources(this.dockPanel, "dockPanel");
            this.dockPanel.Name = "dockPanel";
            // 
            // MyStatusStrip
            // 
            this.MyStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLabelExportExcel,
            this.tsProgressExportExcel,
            this.tsLabelSavingMode,
            this.tsLabelCurArchSaving,
            this.tsProgressSaving,
            this.tsAbortSaving,
            this.tsConnectOptions});
            resources.ApplyResources(this.MyStatusStrip, "MyStatusStrip");
            this.MyStatusStrip.Name = "MyStatusStrip";
            // 
            // tsLabelExportExcel
            // 
            this.tsLabelExportExcel.Name = "tsLabelExportExcel";
            resources.ApplyResources(this.tsLabelExportExcel, "tsLabelExportExcel");
            // 
            // tsProgressExportExcel
            // 
            this.tsProgressExportExcel.Name = "tsProgressExportExcel";
            resources.ApplyResources(this.tsProgressExportExcel, "tsProgressExportExcel");
            this.tsProgressExportExcel.Step = 1;
            this.tsProgressExportExcel.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // tsLabelSavingMode
            // 
            this.tsLabelSavingMode.Name = "tsLabelSavingMode";
            resources.ApplyResources(this.tsLabelSavingMode, "tsLabelSavingMode");
            // 
            // tsLabelCurArchSaving
            // 
            this.tsLabelCurArchSaving.Name = "tsLabelCurArchSaving";
            resources.ApplyResources(this.tsLabelCurArchSaving, "tsLabelCurArchSaving");
            // 
            // tsProgressSaving
            // 
            this.tsProgressSaving.Name = "tsProgressSaving";
            resources.ApplyResources(this.tsProgressSaving, "tsProgressSaving");
            this.tsProgressSaving.Step = 1;
            this.tsProgressSaving.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // tsAbortSaving
            // 
            this.tsAbortSaving.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMiCancelSaving});
            resources.ApplyResources(this.tsAbortSaving, "tsAbortSaving");
            this.tsAbortSaving.Name = "tsAbortSaving";
            // 
            // tsMiCancelSaving
            // 
            resources.ApplyResources(this.tsMiCancelSaving, "tsMiCancelSaving");
            this.tsMiCancelSaving.Name = "tsMiCancelSaving";
            this.tsMiCancelSaving.Click += new System.EventHandler(this.tsMiCancelSaving_Click);
            // 
            // tsConnectOptions
            // 
            this.tsConnectOptions.Name = "tsConnectOptions";
            resources.ApplyResources(this.tsConnectOptions, "tsConnectOptions");
            // 
            // MyMainMenuStrip
            // 
            this.MyMainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msFile,
            this.msTools,
            this.msService,
            this.msHelp});
            resources.ApplyResources(this.MyMainMenuStrip, "MyMainMenuStrip");
            this.MyMainMenuStrip.Name = "MyMainMenuStrip";
            // 
            // msFile
            // 
            this.msFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msFileLoadFromDevice,
            this.msFileSeparator1,
            this.msFileOpenImageFile,
            this.msFileSeparator2,
            this.msFileExit});
            resources.ApplyResources(this.msFile, "msFile");
            this.msFile.Name = "msFile";
            // 
            // msFileLoadFromDevice
            // 
            resources.ApplyResources(this.msFileLoadFromDevice, "msFileLoadFromDevice");
            this.msFileLoadFromDevice.Name = "msFileLoadFromDevice";
            this.msFileLoadFromDevice.Click += new System.EventHandler(this.mmFileLoadFromDevice_Click);
            // 
            // msFileSeparator1
            // 
            this.msFileSeparator1.Name = "msFileSeparator1";
            resources.ApplyResources(this.msFileSeparator1, "msFileSeparator1");
            // 
            // msFileOpenImageFile
            // 
            resources.ApplyResources(this.msFileOpenImageFile, "msFileOpenImageFile");
            this.msFileOpenImageFile.Name = "msFileOpenImageFile";
            this.msFileOpenImageFile.Click += new System.EventHandler(this.msFileOpenImageFile_Click);
            // 
            // msFileSeparator2
            // 
            this.msFileSeparator2.Name = "msFileSeparator2";
            resources.ApplyResources(this.msFileSeparator2, "msFileSeparator2");
            // 
            // msFileExit
            // 
            resources.ApplyResources(this.msFileExit, "msFileExit");
            this.msFileExit.Name = "msFileExit";
            this.msFileExit.Click += new System.EventHandler(this.msFileExit_Click);
            // 
            // msTools
            // 
            this.msTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miExportToExcel,
            this.miReportPQP,
            this.miToolsSeparator,
            this.msToolsConstraints,
            this.msToolsObjectNames,
            this.msToolsDevName,
            this.msToolsNominalValues,
            this.msToolsDeclaredU});
            this.msTools.Name = "msTools";
            resources.ApplyResources(this.msTools, "msTools");
            // 
            // miExportToExcel
            // 
            this.miExportToExcel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msToolsExportToExcelPQP,
            this.msToolsExportToExcelAVG,
            this.msToolsExportToExcelDNS,
            this.msToolsExportToExcelEvents});
            resources.ApplyResources(this.miExportToExcel, "miExportToExcel");
            this.miExportToExcel.Name = "miExportToExcel";
            // 
            // msToolsExportToExcelPQP
            // 
            resources.ApplyResources(this.msToolsExportToExcelPQP, "msToolsExportToExcelPQP");
            this.msToolsExportToExcelPQP.Name = "msToolsExportToExcelPQP";
            this.msToolsExportToExcelPQP.Click += new System.EventHandler(this.msToolsExportToExcelPQP_Click);
            // 
            // msToolsExportToExcelAVG
            // 
            resources.ApplyResources(this.msToolsExportToExcelAVG, "msToolsExportToExcelAVG");
            this.msToolsExportToExcelAVG.Name = "msToolsExportToExcelAVG";
            this.msToolsExportToExcelAVG.Click += new System.EventHandler(this.msToolsExportToExcelAVG_Click);
            // 
            // msToolsExportToExcelDNS
            // 
            resources.ApplyResources(this.msToolsExportToExcelDNS, "msToolsExportToExcelDNS");
            this.msToolsExportToExcelDNS.Name = "msToolsExportToExcelDNS";
            this.msToolsExportToExcelDNS.Click += new System.EventHandler(this.msToolsExportToExcelDNS_Click);
            // 
            // msToolsExportToExcelEvents
            // 
            resources.ApplyResources(this.msToolsExportToExcelEvents, "msToolsExportToExcelEvents");
            this.msToolsExportToExcelEvents.Name = "msToolsExportToExcelEvents";
            this.msToolsExportToExcelEvents.Click += new System.EventHandler(this.msToolsExportToExcelDNS_Click);
            // 
            // miReportPQP
            // 
            this.miReportPQP.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miReportGOST,
            this.miReportRD,
            this.miReportFSK,
            this.miReportEtPqpA,
            this.miReportEtPqpA_v2,
            this.miReportEtPqpA_v3,
            this.miEditReportTemplates});
            resources.ApplyResources(this.miReportPQP, "miReportPQP");
            this.miReportPQP.Name = "miReportPQP";
            // 
            // miReportGOST
            // 
            resources.ApplyResources(this.miReportGOST, "miReportGOST");
            this.miReportGOST.Name = "miReportGOST";
            this.miReportGOST.Click += new System.EventHandler(this.miReportGOST_Click);
            // 
            // miReportRD
            // 
            resources.ApplyResources(this.miReportRD, "miReportRD");
            this.miReportRD.Name = "miReportRD";
            this.miReportRD.Click += new System.EventHandler(this.miReportRD_Click);
            // 
            // miReportFSK
            // 
            resources.ApplyResources(this.miReportFSK, "miReportFSK");
            this.miReportFSK.Name = "miReportFSK";
            this.miReportFSK.Click += new System.EventHandler(this.miReportFSK_Click);
            // 
            // miReportEtPqpA
            // 
            resources.ApplyResources(this.miReportEtPqpA, "miReportEtPqpA");
            this.miReportEtPqpA.Name = "miReportEtPqpA";
            this.miReportEtPqpA.Click += new System.EventHandler(this.miReportEtPqpA_Click);
            // 
            // miReportEtPqpA_v2
            // 
            resources.ApplyResources(this.miReportEtPqpA_v2, "miReportEtPqpA_v2");
            this.miReportEtPqpA_v2.Name = "miReportEtPqpA_v2";
            this.miReportEtPqpA_v2.Click += new System.EventHandler(this.miReportEtPqpA_v2_Click);
            // 
            // miReportEtPqpA_v3
            // 
            resources.ApplyResources(this.miReportEtPqpA_v3, "miReportEtPqpA_v3");
            this.miReportEtPqpA_v3.Name = "miReportEtPqpA_v3";
            this.miReportEtPqpA_v3.Click += new System.EventHandler(this.miReportEtPqpA_v3_Click);
            // 
            // miEditReportTemplates
            // 
            this.miEditReportTemplates.Name = "miEditReportTemplates";
            resources.ApplyResources(this.miEditReportTemplates, "miEditReportTemplates");
            this.miEditReportTemplates.Click += new System.EventHandler(this.miEditReportTemplates_Click);
            // 
            // miToolsSeparator
            // 
            this.miToolsSeparator.Name = "miToolsSeparator";
            resources.ApplyResources(this.miToolsSeparator, "miToolsSeparator");
            // 
            // msToolsConstraints
            // 
            resources.ApplyResources(this.msToolsConstraints, "msToolsConstraints");
            this.msToolsConstraints.Name = "msToolsConstraints";
            this.msToolsConstraints.Click += new System.EventHandler(this.msToolsConstraints_Click);
            // 
            // msToolsObjectNames
            // 
            resources.ApplyResources(this.msToolsObjectNames, "msToolsObjectNames");
            this.msToolsObjectNames.Name = "msToolsObjectNames";
            this.msToolsObjectNames.Click += new System.EventHandler(this.msToolsObjectNames_Click);
            // 
            // msToolsDevName
            // 
            this.msToolsDevName.Name = "msToolsDevName";
            resources.ApplyResources(this.msToolsDevName, "msToolsDevName");
            this.msToolsDevName.Click += new System.EventHandler(this.msToolsDevName_Click);
            // 
            // msToolsNominalValues
            // 
            this.msToolsNominalValues.Name = "msToolsNominalValues";
            resources.ApplyResources(this.msToolsNominalValues, "msToolsNominalValues");
            this.msToolsNominalValues.Click += new System.EventHandler(this.msToolsNominalValues_Click);
            // 
            // msToolsDeclaredU
            // 
            this.msToolsDeclaredU.Name = "msToolsDeclaredU";
            resources.ApplyResources(this.msToolsDeclaredU, "msToolsDeclaredU");
            this.msToolsDeclaredU.Click += new System.EventHandler(this.msToolsDeclaredU_Click);
            // 
            // msService
            // 
            this.msService.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msServiceLanguage,
            this.msServiceSettings});
            this.msService.Name = "msService";
            resources.ApplyResources(this.msService, "msService");
            // 
            // msServiceLanguage
            // 
            this.msServiceLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msServiceLangEnglish,
            this.msServiceLangRussian});
            this.msServiceLanguage.Name = "msServiceLanguage";
            resources.ApplyResources(this.msServiceLanguage, "msServiceLanguage");
            // 
            // msServiceLangEnglish
            // 
            this.msServiceLangEnglish.Name = "msServiceLangEnglish";
            resources.ApplyResources(this.msServiceLangEnglish, "msServiceLangEnglish");
            this.msServiceLangEnglish.Click += new System.EventHandler(this.msServiceLangEnglish_Click);
            // 
            // msServiceLangRussian
            // 
            this.msServiceLangRussian.Name = "msServiceLangRussian";
            resources.ApplyResources(this.msServiceLangRussian, "msServiceLangRussian");
            this.msServiceLangRussian.Click += new System.EventHandler(this.msServiceLangRussian_Click);
            // 
            // msServiceSettings
            // 
            resources.ApplyResources(this.msServiceSettings, "msServiceSettings");
            this.msServiceSettings.Name = "msServiceSettings";
            this.msServiceSettings.Click += new System.EventHandler(this.msServiceSettings_Click);
            // 
            // msHelp
            // 
            this.msHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msHelpHelp,
            this.miFeedback,
            this.miHelpSeparator,
            this.msHelpAbout});
            this.msHelp.Name = "msHelp";
            resources.ApplyResources(this.msHelp, "msHelp");
            // 
            // msHelpHelp
            // 
            resources.ApplyResources(this.msHelpHelp, "msHelpHelp");
            this.msHelpHelp.Name = "msHelpHelp";
            this.msHelpHelp.Click += new System.EventHandler(this.msHelpHelp_Click);
            // 
            // miFeedback
            // 
            this.miFeedback.Name = "miFeedback";
            resources.ApplyResources(this.miFeedback, "miFeedback");
            this.miFeedback.Click += new System.EventHandler(this.miFeedback_Click);
            // 
            // miHelpSeparator
            // 
            this.miHelpSeparator.Name = "miHelpSeparator";
            resources.ApplyResources(this.miHelpSeparator, "miHelpSeparator");
            // 
            // msHelpAbout
            // 
            resources.ApplyResources(this.msHelpAbout, "msHelpAbout");
            this.msHelpAbout.Name = "msHelpAbout";
            this.msHelpAbout.Click += new System.EventHandler(this.msHelpAboutEnergomonitoring_Click);
            // 
            // MainToolStrip
            // 
            this.MainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbExchangeWithDevice,
            this.toolStripSeparator1,
            this.tbsSettings,
            this.tbsConstraints,
            this.tbsHelp,
            this.toolStripSeparator2,
            this.tsbMomentData});
            resources.ApplyResources(this.MainToolStrip, "MainToolStrip");
            this.MainToolStrip.Name = "MainToolStrip";
            // 
            // tsbExchangeWithDevice
            // 
            this.tsbExchangeWithDevice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tsbExchangeWithDevice, "tsbExchangeWithDevice");
            this.tsbExchangeWithDevice.Name = "tsbExchangeWithDevice";
            this.tsbExchangeWithDevice.Click += new System.EventHandler(this.mmFileLoadFromDevice_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tbsSettings
            // 
            this.tbsSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbsSettings, "tbsSettings");
            this.tbsSettings.Name = "tbsSettings";
            this.tbsSettings.Click += new System.EventHandler(this.tbsSettings_Click);
            // 
            // tbsConstraints
            // 
            this.tbsConstraints.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbsConstraints, "tbsConstraints");
            this.tbsConstraints.Name = "tbsConstraints";
            this.tbsConstraints.Click += new System.EventHandler(this.tbsConstraints_Click);
            // 
            // tbsHelp
            // 
            this.tbsHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tbsHelp, "tbsHelp");
            this.tbsHelp.Name = "tbsHelp";
            this.tbsHelp.Click += new System.EventHandler(this.tbsHelp_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // tsbMomentData
            // 
            this.tsbMomentData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.tsbMomentData, "tsbMomentData");
            this.tsbMomentData.Name = "tsbMomentData";
            this.tsbMomentData.Click += new System.EventHandler(this.tsbMomentData_Click);
            // 
            // panelConnect
            // 
            resources.ApplyResources(this.panelConnect, "panelConnect");
            this.panelConnect.BackColor = System.Drawing.Color.Red;
            this.panelConnect.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelConnect.Name = "panelConnect";
            // 
            // toolTipHost
            // 
            this.toolTipHost.AutoPopDelay = 5000;
            this.toolTipHost.InitialDelay = 5;
            this.toolTipHost.IsBalloon = true;
            this.toolTipHost.ReshowDelay = 100;
            // 
            // frmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.panelConnect);
            this.Controls.Add(this.MainToolStrip);
            this.Controls.Add(this.MyMainMenuStrip);
            this.Controls.Add(this.MyStatusStrip);
            this.Name = "frmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.splitContainerArchiveInfo.Panel1.ResumeLayout(false);
            this.splitContainerArchiveInfo.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerArchiveInfo)).EndInit();
            this.splitContainerArchiveInfo.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.MyStatusStrip.ResumeLayout(false);
            this.MyStatusStrip.PerformLayout();
            this.MyMainMenuStrip.ResumeLayout(false);
            this.MyMainMenuStrip.PerformLayout();
            this.MainToolStrip.ResumeLayout(false);
            this.MainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			try
			{
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.UnhandledException += 
					new UnhandledExceptionEventHandler(LastExceptionHandler);

				EmService.Init();
				if (!Directory.Exists(EmService.TEMP_IMAGE_DIR))
					Directory.CreateDirectory(EmService.TEMP_IMAGE_DIR);

				// trying to load settings			
				Settings settings = new Settings();
				settings.LoadSettings();

				string locale;
				switch (settings.Language)
				{
					case "Русский":
						locale = "ru-RU";
						settings.CurrentLanguage = "ru";
						break;
					case "English":
						locale = "en-US";
						settings.CurrentLanguage = "en";
						break;
					default:
						//locale = string.Empty;
						locale = "ru-RU";
						EmService.WriteToLogFailed("no language detected! russian was set");
						settings.CurrentLanguage = "ru";
						settings.Language = "Русский";
						break;
				}

				// setting up current culture
				Thread.CurrentThread.CurrentCulture =
					Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale, false);
				Application.CurrentCulture = Thread.CurrentThread.CurrentCulture;

				// enabling visual styles
				Application.EnableVisualStyles();
				// enabling imagelist			
				Application.DoEvents();

				Application.Run(new frmMain(settings));
			}
            catch (OutOfMemoryException mex)
            {
                ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
                                    System.Reflection.Assembly.GetExecutingAssembly());
                string msg = rm.GetString("msg_outofmemory");
                string cap = rm.GetString("unfortunately_caption");

                MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                EmService.DumpException(mex, "Exception in Main():");
                //throw;
            }
			catch (Exception ex)
			{
                MessageBox.Show("Exception in Main():  " + ex.Message);
				EmService.DumpException(ex, "Exception in Main():");

				//if (EmService.ShowWndFeedback)
				//{
				frmSentLogs frmLogs = new frmSentLogs();
				frmLogs.ShowDialog();
				EmService.ShowWndFeedback = false;
				//}
				//throw;
			}
		}

		// переопределенная оконная процедура
		protected override void WndProc(ref Message m)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
				
				switch (m.Msg)
				{
					case EmService.WM_USER + 1:
						reader_cur_percent_ += reader_percent_for_one_step_ * (double)m.WParam;
						int pos = tsProgressSaving.Value + ((int)reader_cur_percent_ - reader_prev_percent_);
						if (pos <= tsProgressSaving.Maximum)
							tsProgressSaving.Value = pos;

						reader_prev_percent_ = (int)reader_cur_percent_;
						break;

					case EmService.WM_USER + 2:	// connection with the device was established
						panelConnect.BackColor = Color.LimeGreen;
						toolTipHost.SetToolTip(this.panelConnect, rm.GetString("str_dev_connected"));
						break;

					case EmService.WM_USER + 3:   // the device was disconnected
						panelConnect.BackColor = Color.Red;
						toolTipHost.SetToolTip(this.panelConnect, rm.GetString("str_dev_disconnected"));
						break;

					//case WM_DEVICECHANGE:
					//    unsafe
					//    {
					//        DEV_BROADCAST_HDR* pDeviceInfo;
					//        DEV_BROADCAST_DEVICEINTERFACE* pDeviceMoreInfo;
					//        Guid CYUSBDRV_GUID = new Guid(0xAE18A550, 0x7F6A, 0x11d4, 0x97, 0xDD, 0x00, 
					//0x01, 0x02, 0x29, 0xB9, 0x5B);
					//        if (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
					//        {
					//            pDeviceInfo = (DEV_BROADCAST_HDR*)m.LParam;
					//            if (pDeviceInfo->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
					//            {
					//                pDeviceMoreInfo = (DEV_BROADCAST_DEVICEINTERFACE*)m.LParam;
					//                if (Kernel32.memcmp(&(CYUSBDRV_GUID), &(pDeviceMoreInfo->dbcc_classguid),
					//                            new UIntPtr((uint)sizeof(Guid))) == 0)
					//                {
					//                    //switch (ConnectStatus)
					//                    //{
					//                    //    case CONNECTSTATUS_IDLE:
					//                    //    case CONNECTSTATUS_FAILED:
					//                    //    case CONNECTSTATUS_DISCONNECTED:
					//                    //    case CONNECTSTATUS_FORCEDISCONNECT:
					//                    //        break;
					//                    //    case CONNECTSTATUS_CONNECTING:
					//                    //    case CONNECTSTATUS_CONNECTED:
					//                    //        InterlockedExchange((LONG*)(&ConnectStatus),
											//CONNECTSTATUS_FORCEDISCONNECT);
					//                    //        break;
					//                    //}
											//завести переменную bool, отвечающую за коннект
					//                    if (panelConnect.BackColor == Color.LimeGreen)
					//                    {
					//                        panelConnect.BackColor = Color.Red;
					//                        DisableConnectInterface();
					//                    }
					//                }
					//            }
					//        }
					//    }
					//    break;
				}
				base.WndProc(ref m);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in WndProc():");
				throw;
			}
		}

		static void LastExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception ex = (Exception)args.ExceptionObject;
			EmService.DumpException(ex, "Information from LastExceptionHandler:");
		}

		#region Public Methods

		/// <summary>For EtPQP-A archives</summary>
		public void SetCurrentArchive(string parentName,
			DateTime start, DateTime end,
			ConnectScheme connectScheme,
			float uNomLinear, float uNomPhase, float fNom,
			AvgTypes_PQP_A avgType,
			Int64 cur_id,		// registration id
			short constraintType,
			short t_fliker,
			string deviceVersion,
			int pgServerIndex,
			float iLimit, float uLimit)
		{
			try
			{
				curArchive_ = new ArchiveInfo(ref settings_);
				curArchive_.SetCommonInfo(pgServerIndex, parentName, start, end, connectScheme,
											uNomLinear, uNomPhase, fNom, avgType, cur_id, deviceVersion,
											t_fliker, constraintType,
											iLimit, uLimit);
				labelArchiveInfo.Text = curArchive_.GetArchiveInfo();

				btnArchiveClose.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SetCurrentArchive(): ");
				throw;
			}
		}

		/// <summary>For NOT EtPQP-A archives</summary>
		public void SetCurrentArchive(string parentName,
			DateTime start, DateTime end,
			ConnectScheme connectScheme,
			float uNomLinear, float uNomPhase, float fNom,
			AvgTypes avgType,
			Int64 cur_id,			// object id
			bool currentWithTR,
			short constraintType,
			short t_fliker,
			DateTime mlStartDateTime1, DateTime mlEndDateTime1,
			DateTime mlStartDateTime2, DateTime mlEndDateTime2,
			string deviceVersion,
			EmDeviceType devType,
			int pgServerIndex)
		{
			try
			{
				curArchive_ = new ArchiveInfo(ref settings_);
				curArchive_.SetCommonInfo(pgServerIndex, parentName, start, end, connectScheme,
											uNomLinear, uNomPhase, fNom, avgType, cur_id,
											currentWithTR, devType, deviceVersion, t_fliker, constraintType,
											mlStartDateTime1, mlEndDateTime1, mlStartDateTime2, mlEndDateTime2);
				labelArchiveInfo.Text = curArchive_.GetArchiveInfo();

				btnArchiveClose.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SetCurrentArchive(): ");
				throw;
			}
		}

		/// <summary>SUB-archive</summary>
		public void SetCurrentSubArchive(ArchiveType archiveType, long subArchiveId, DateTime start, DateTime end)
		{
			try
			{
				if (curArchive_.DevType != EmDeviceType.ETPQP_A)
				{
					msToolsExportToExcelEvents.Visible = false;
					msToolsExportToExcelDNS.Visible = true;
				}
				else
				{
					msToolsExportToExcelEvents.Visible = true;
					msToolsExportToExcelDNS.Visible = false;
				}

				if (archiveType == ArchiveType.AVG)
				{
					if (wndDocAVG == null) wndDocAVG = new frmDocAVG(this, settings_, EmDeviceType.NONE);
					wndDocAVG.wndDocAVGGraphRight.CurConnectScheme = curArchive_.ConnectScheme;
					wndDocAVG.SetDeviceType(curArchive_.DevType);

					if (curArchive_.DevType != EmDeviceType.ETPQP_A)
					{
						wndDocAVG.wndDocAVGMain.IninializeDoc2Grids(curArchive_.CurPgServerIndex,
								subArchiveId,
								curArchive_.ConnectScheme,
								curArchive_.CurrentWithTR, curArchive_.DevType);
					}
					else
					{
						wndDocAVG.wndDocAVGMain.IninializeDoc2Grids(curArchive_.CurPgServerIndex,
								subArchiveId,
								curArchive_.ConnectScheme,
								curArchive_.ILimit, curArchive_.ULimit,
								EmDeviceType.ETPQP_A);
					}

					if (curArchive_.DevType != EmDeviceType.ETPQP_A)
					{
						AvgTypes avgType = ((wndToolbox.ActiveNodeAVG as
											EmTreeNodeDBMeasureClassic).AvgType);
						wndDocAVG.wndDocAVGMain.SetCommonCaption(start, end, avgType, subArchiveId, 
							curArchive_.CurrentWithTR, curArchive_.DevType);
					}
					else
					{
						AvgTypes_PQP_A avgType = ((wndToolbox.ActiveNodeAVG as
											EmTreeNodeDBMeasureEtPQP_A).AvgType_PQP_A);
						wndDocAVG.wndDocAVGMain.SetCommonCaption(start, end, avgType, subArchiveId);
					} 

					wndDocAVG.Show(dockPanel);
					wndDocAVG.wndDocAVGGraphRight.UpdateGraphPanel(false, true);
					msToolsExportToExcelAVG.Enabled = true;
				}
				else if (archiveType == ArchiveType.DNS)
				{
					if (wndDocDNS == null) wndDocDNS = new frmDocDNS(this, settings_, curArchive_.DevType);

					float nominal = 0;
					if (curArchive_.ConnectScheme == ConnectScheme.Ph3W3 ||
						curArchive_.ConnectScheme == ConnectScheme.Ph3W3_B_calc)
						nominal = curArchive_.UNomLinear;
					else
						nominal = curArchive_.UNomPhase;

					wndDocDNS.wndDocDNSMain.Open(curArchive_.CurPgServerIndex,
						subArchiveId, curArchive_.ConnectScheme, nominal, curArchive_.DevType);
					wndDocDNS.Show(dockPanel);

					msToolsExportToExcelDNS.Enabled = true;
					msToolsExportToExcelEvents.Enabled = true;
				}
				else if (archiveType == ArchiveType.PQP)
				{
					if (wndDocPQP == null) wndDocPQP = new frmDocPQP(this, settings_);

					wndDocPQP.wndDocPQPMain.Open(curArchive_.CurPgServerIndex, curArchive_.DevType,
						curArchive_.ConnectScheme,
						subArchiveId, start, end,
						curArchive_.DevVersion,
						curArchive_.T_fliker,
						curArchive_.ConstraintType);

					wndDocPQP.CloseGraphsOfPrevArchives(curArchive_.DevType);
					wndDocPQP.wndDocPQPMain.UpdatePQPGraphs();

					bool flikkerExists = false;
					if (curArchive_.ConnectScheme != ConnectScheme.Ph3W3 &&
						curArchive_.ConnectScheme != ConnectScheme.Ph3W3_B_calc)
					{
						if (curArchive_.DevType == EmDeviceType.EM32 || curArchive_.DevType == EmDeviceType.ETPQP ||
							curArchive_.DevType == EmDeviceType.ETPQP_A || curArchive_.DevType == EmDeviceType.EM33T1)
							flikkerExists = true;
						else if (curArchive_.DevType == EmDeviceType.EM33T)
							if (Constants.isNewDeviceVersion_EM33T(curArchive_.DevVersion))
								flikkerExists = true;
					}
					if (flikkerExists)
						wndDocPQP.wndDocFlikGraphBottom.init(curArchive_.T_fliker, curArchive_.DevType);
					wndDocPQP.wndDocFValuesGraphBottom.init();

					if (curArchive_.DevType != EmDeviceType.ETPQP_A)
					{
						wndDocPQP.wndDocPQPMain.SetCommonCaption(
							curArchive_.MlStartDateTime1, curArchive_.MlEndDateTime1,
							curArchive_.MlStartDateTime2, curArchive_.MlEndDateTime2);
					}
					else
					{
						wndDocPQP.wndDocPQPMain.SetCommonCaption(DateTime.MinValue, DateTime.MinValue,
																DateTime.MinValue, DateTime.MinValue);
					} 

					wndDocPQP.Show(dockPanel);
					msToolsExportToExcelPQP.Enabled = true;

					EnabledMenuPQPReport(true, curArchive_.DevType);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SetCurrentArchive(): ");
				throw;
			}
		}

		public void CloseArchive()
		{
			try
			{
				if (wndDocAVG != null) 
				{
					wndDocAVG.CloseGraphForms();
					wndDocAVG.Hide(); wndDocAVG.Dispose(); wndDocAVG = null; 
				}
				if (wndDocDNS != null) { wndDocDNS.Hide(); wndDocDNS.Dispose(); wndDocDNS = null; }
				if (wndDocPQP != null) { wndDocPQP.Hide(); wndDocPQP.Dispose(); wndDocPQP = null; }
				curArchive_ = null;

				if (!settings_.CurrentLanguage.Equals("ru"))
					labelArchiveInfo.Text = "No archive is selected";
				else labelArchiveInfo.Text = "Архив не выбран";

				wndToolbox.DeactivateAllActiveNodes();

				btnArchiveClose.Enabled = false;
				EnabledMenuPQPReport(false, EmDeviceType.NONE);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CloseArchive():");
				throw;
			}
		}

		public void SyncronizeSettings()
		{
			if (wndDocPQP != null) wndDocPQP.SyncronizeSettings(settings_);
			if (wndDocAVG != null) wndDocAVG.SyncronizeSettings(settings_);
			if (wndDocDNS != null) wndDocDNS.SyncronizeSettings(settings_);
		}

		public void StartExportArchive(EmDeviceType devType, int PgServerIndex, Int64 archive_id,
										Int64 object_id, EmSqlDataNodeType[] parts,
										string fileName)
		{
			try
			{
				currentDevType_ = devType;

				bwExporter_ = new BackgroundWorker();
				bwExporter_.WorkerReportsProgress = true;
				bwExporter_.WorkerSupportsCancellation = true;
				bwExporter_.DoWork += bwExporter_DoWork;
				bwExporter_.ProgressChanged += bwExporter_ProgressChanged;
				bwExporter_.RunWorkerCompleted += bwExporter_RunWorkerCompleted;

				////// меняем статусбар на экспорт ///////////////////
				ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				tsLabelSavingMode.Text = rm.GetString("msg_toolbar_exporting");
				tsLabelCurArchSaving.Text = string.Empty;

				tsConnectOptions.Visible = false;
				tsLabelSavingMode.Visible = true;
				tsLabelCurArchSaving.Visible = true;
				tsProgressSaving.Visible = true;
				tsProgressSaving.Style = ProgressBarStyle.Marquee;
				tsAbortSaving.Visible = true;
				tsAbortSaving.Text = rm.GetString("cancel_text");
				//////////////////////////////////////////////////////

				switch (devType)
				{
					case EmDeviceType.EM32:
						exporter_ = new EmDataExport32(
							this, parts, archive_id,
							settings_.PgServers[PgServerIndex].PgConnectionStringEm32, fileName);
						break;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
					case EmDeviceType.NONE:
						exporter_ = new EmDataExport33(
							this, parts, archive_id,
							settings_.PgServers[PgServerIndex].PgConnectionStringEm33, fileName);
						break;
					case EmDeviceType.ETPQP:
						exporter_ = new EtDataExportPQP(
							this, parts, archive_id, object_id,
							settings_.PgServers[PgServerIndex].PgConnectionStringEtPQP, fileName);
						break;
					case EmDeviceType.ETPQP_A:
						exporter_ = new EtDataExportPQP_A(
							this, parts, archive_id, object_id,
							settings_.PgServers[PgServerIndex].PgConnectionStringEtPQP_A, fileName);
						break;
				}
				bwExporter_.RunWorkerAsync(devType);
				EnableBtnLoad(false);
			}
			catch (EmException emx)
			{
				saverOnSavingEnds();
				EmService.WriteToLogFailed("Error StartExportArchive(): " + emx.Message);
			}
			catch (Exception ex)
			{
				saverOnSavingEnds();
				EmService.DumpException(ex, "Error StartExportArchive()");
				throw;
			}
		}

		#endregion

		#region Form Events Handlers

		private void btnArchiveClose_Click(object sender, EventArgs e)
		{
			CloseArchive();
		}

		private void miEditReportTemplates_Click(object sender, EventArgs e)
		{
			frmReportTemplates frmReport = new frmReportTemplates();
			frmReport.ShowDialog();
		}

		private void miFeedback_Click(object sender, EventArgs e)
		{
			frmSentLogs frmLogs = new frmSentLogs();
			frmLogs.ShowDialog();
		}

		private void frmMain_Load(object sender, System.EventArgs e)
		{
			try
			{
				try
				{
					if (File.Exists(EmService.logGeneralName))
						File.Delete(EmService.logGeneralName);
					if (File.Exists(EmService.logFailedName))
						File.Delete(EmService.logFailedName);
					if (File.Exists(EmService.logDebugName))
						File.Delete(EmService.logDebugName);

					if (File.Exists("LogFailedCpp.txt")) File.Delete("LogFailedCpp.txt");
					if (File.Exists("LogGeneralCpp.txt")) File.Delete("LogGeneralCpp.txt");
					if (File.Exists("LogGeneralCppWrap.txt")) File.Delete("LogGeneralCppWrap.txt");
					if (File.Exists("LogFailedCppWrap.txt")) File.Delete("LogFailedCppWrap.txt");
					if (File.Exists("LogGeneralCppUnman.txt")) File.Delete("LogGeneralCppUnman.txt");
				}
				catch (Exception)
				{
				}

				settings_.LoadSettings();

				msToolsExportToExcelDNS.Visible = settings_.CurDeviceType != EmDeviceType.ETPQP_A;
				msToolsExportToExcelEvents.Visible = settings_.CurDeviceType == EmDeviceType.ETPQP_A;

				// проверяем на сайте есть ли новая версия программы
				try
				{
					if (settings_.CheckNewSoftwareVersion)
					{
						// определяем версию программы на компьютере
						string assemblyName = this.GetType().Assembly.FullName;
						EmService.WriteToLogDebug("AssemblyName = " + assemblyName);
						int posStart = assemblyName.IndexOf("Version=") + 8;
						int posEnd = assemblyName.IndexOf(',', posStart);
						int len = assemblyName.Length - (assemblyName.Length - posEnd) - posStart;
						assemblyName = assemblyName.Substring(posStart, len);
						EmService.WriteToLogDebug("VersionName = " + assemblyName);
						posStart = assemblyName.LastIndexOf('.');
						short curVersion = Int16.Parse(assemblyName.Substring(posStart + 1));

						// теперь смотрим, что на фтп
						FTPClient.FTPMarsClient ftp = new FTPClient.FTPMarsClient();
						short verFtp = ftp.FtpGetSoftwareVersion();
						EmService.WriteToLogDebug("Program version FTP: " + verFtp.ToString());
						EmService.WriteToLogDebug("Program version current: " + curVersion.ToString());
						if (verFtp > curVersion)
						{
							frmNewSoftware frm = new frmNewSoftware();
							frm.ShowDialog();

							//settings_.LoadSettings();
							if (settings_.CheckNewSoftwareVersion != frm.ShowThisMessage)
							{
								settings_.CheckNewSoftwareVersion = frm.ShowThisMessage;
								//settings_.SaveSettings();
							}
						}
					}
				}
				catch (Exception ftpEx)
				{
					EmService.DumpException(ftpEx, "Error in FTP code:");
				}

				// пока для всех Em32 выставляем широковещательный адрес
				// потом для 485 надо будет использовать конкретные адреса
				settings_.CurDeviceAddress = 0xFFFF;

				if (settings_.IOInterface == EmPortType.Rs485 || settings_.IOInterface == EmPortType.Modem ||
					settings_.IOInterface == EmPortType.GPRS)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						if (settings_.IOInterface == EmPortType.Rs485) settings_.CurDeviceAddress = wnd.ActiveDevAddress485;
						if (settings_.IOInterface == EmPortType.Modem) settings_.CurDeviceAddress = wnd.ActiveDevAddressGSM;
						if (settings_.IOInterface == EmPortType.GPRS) settings_.CurDeviceAddress = wnd.ActiveDevAddressGPRS;
					}
				}

				if (settings_.CurrentLanguage == "ru")
					miReportPQP.Visible = true;
				wndToolbox.Show(dockPanel);

				// запускаем таймер и поток автоматического считывания
				bAutoMode_ = true;
				autoDial_ = new AutoDialThread(settings_, autoConnectQueues_, this);
				autoDial_.OnTimerDial += new AutoDialThread.TimerDialHandler(frmMain_OnTimerDial);
				autoDial_.OnTimerEthernet +=
									new AutoDialThread.TimerEthernetHandler(frmMain_OnTimerEthernet);
				autoDial_.OnTimerRs485 += new AutoDialThread.TimerRs485Handler(frmMain_OnTimerRs485);
				autoDial_.OnTimerGPRS += new AutoDialThread.TimerGPRSHandler(frmMain_OnTimerGPRS);
				AutoDialModemThread_ = new Thread(new ThreadStart(autoDial_.Run));
				string locale = settings_.CurrentLanguage.Equals("ru") ? "ru-RU" : "en-US";
				AutoDialModemThread_.CurrentCulture =
					AutoDialModemThread_.CurrentUICulture = new System.Globalization.CultureInfo(locale, false);
				AutoDialModemThread_.Start();

				// проверяем не осталось ли каких-нибудь временных файлов, кот-е создавались для
				// архива усредненных (такое например могло произойти при возникновении исключения)
				string[] tmpFiles = Directory.GetFiles(EmService.TEMP_IMAGE_DIR);
				for (int iFile = 0; iFile < tmpFiles.Length; ++iFile)
				{
					if (tmpFiles[iFile].ToLower().Contains(AvgTypes.ThreeSec.ToString().ToLower()) ||
						tmpFiles[iFile].ToLower().Contains(AvgTypes.OneMin.ToString().ToLower()) ||
						tmpFiles[iFile].ToLower().Contains(AvgTypes.ThirtyMin.ToString().ToLower()))
						File.Delete(tmpFiles[iFile]);
				}

				SetStatusBarText();
				tsConnectOptions.Visible = true;

				settings_.SaveSettings();

				try
				{
					// править реестр, чтобы избежать зависаний при переключении раскладки
					string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\IMM";
					Registry.SetValue(keyName, "LoadIMM", 0, RegistryValueKind.DWord);
				}
				catch (System.Security.SecurityException)
				{
					EmService.WriteToLogFailed("The user does not have the permissions required to create or modify registry keys. Unable to set LoadIMM");
				}
				catch (System.UnauthorizedAccessException)
				{
					EmService.WriteToLogFailed("The user does not have the permissions required to create or modify registry keys. Unable to set LoadIMM");
				}
				catch (Exception regex)
				{
					EmService.DumpException(regex, "Unable to set LoadIMM:  ");
				}

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
				panelConnect.BackColor = Color.Red;
				toolTipHost.SetToolTip(this.panelConnect, rm.GetString("str_dev_disconnected"));

				// Connecting to the database server
				if (settings_.CurServerIndex >= 0)
				{
					if (!ConnectToServer(settings_.CurServerIndex))
						MessageBoxes.DbConnectError(this, settings_.PgServers[settings_.CurServerIndex].PgHost,
							settings_.PgServers[settings_.CurServerIndex].PgPort, string.Empty);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in frmMain_Load():  ");
				throw;
			}
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			ClosingInfoThread threadInfo = null;
			Thread thread = null;

			try
			{
				// if reading is in process demand the confirmation
				if (needConfirmClose_)
				{
					DialogResult res = MessageBoxes.ConfirmExit(this);
					if (res != DialogResult.Yes)
					{
						e.Cancel = true;
						return;
					}
				}

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
													this.GetType().Assembly);
				string mess = rm.GetString("before_closing_text");

				if (bwReader_ != null)
				{
					if (bwReader_.IsBusy)
					{
						bwReader_.CancelAsync();
						switch (currentDevType_)
						{
							case EmDeviceType.EM32:
								if (readerEm32_ != null) readerEm32_.DisconnectDevice(); break;
							case EmDeviceType.ETPQP:
								if (readerEtPQP_ != null) readerEtPQP_.DisconnectDevice(); break;
							case EmDeviceType.ETPQP_A:
								if (readerEtPQP_A_ != null) readerEtPQP_A_.DisconnectDevice(); break;
						}

						if (threadInfo == null)
						{
							threadInfo = new ClosingInfoThread(this, mess);
							thread = new Thread(new ThreadStart(threadInfo.ThreadEntry));
							thread.Start();
						}

						int cnt = 0;
						while (bwReader_.IsBusy)
						{
							Thread.Sleep(1000);
							cnt++;
							Application.DoEvents();
							if (cnt > 20) break;
						}
					}
				}

				if (bwExporter_ != null)
				{
					if (bwExporter_.IsBusy)
					{
						bwExporter_.CancelAsync();

						while (bwExporter_.IsBusy)
						{
							Thread.Sleep(1000);
							Application.DoEvents();
						}
					}
				}

				if (AutoDialModemThread_ != null)
				{
					if (AutoDialModemThread_.IsAlive)
					{
						//?????????? переделать на BackgroundWorker
						AutoDialModemThread_.Abort(); Thread.Sleep(1000);
					}
					if (AutoDialModemThread_.IsAlive &&
						(AutoDialModemThread_.ThreadState & ThreadState.AbortRequested) != 0)
					{
						if (threadInfo == null)
						{
							threadInfo = new ClosingInfoThread(this, mess);
							thread = new Thread(new ThreadStart(threadInfo.ThreadEntry));
							thread.Start();
						}
						AutoDialModemThread_.Join(3000);
					}
				}
			}
			finally
			{
				if (threadInfo != null)
				{
					if (OnHideWndInfo != null) OnHideWndInfo();
					thread.Abort(); threadInfo = null;
				}
			}
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}

		private void tsMiCancelSaving_Click(object sender, EventArgs e)
		{
			ClosingInfoThread threadInfo = null;
			Thread thread = null;

			try
			{
				// if reading is in process demand the confirmation
				if (needConfirmClose_)
				{
					DialogResult res = MessageBoxes.ConfirmCancel(this);
					if (res != DialogResult.Yes)
					{
						return;
					}
				}

				saverOnSavingEnds();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
													this.GetType().Assembly);
				string mess = rm.GetString("before_closing_text");

				if (bwReader_ != null)
				{
					if (bwReader_.IsBusy)
					{
						bwReader_.CancelAsync();
						switch (currentDevType_)
						{
							case EmDeviceType.EM31K:
							case EmDeviceType.EM33T:
							case EmDeviceType.EM33T1:
								if (readerEm33_ != null)
									readerEm33_.SetCancelReading();
								break;

							case EmDeviceType.EM32:
								if (readerEm32_ != null)
								{
									readerEm32_.SetCancelReading();
									readerEm32_.DisconnectDevice();
								}
								break;
							case EmDeviceType.ETPQP:
								if (readerEtPQP_ != null)
								{
									readerEtPQP_.SetCancelReading();
									readerEtPQP_.DisconnectDevice();
								}
								break;
							case EmDeviceType.ETPQP_A:
								if (readerEtPQP_A_ != null)
								{
									readerEtPQP_A_.SetCancelReading();
									readerEtPQP_A_.DisconnectDevice();
								}
								break;
						}

						if (threadInfo == null)
						{
							threadInfo = new ClosingInfoThread(this, mess);
							thread = new Thread(new ThreadStart(threadInfo.ThreadEntry));
							thread.Start();
						}

						while (bwReader_.IsBusy)
						{
							Thread.Sleep(1000);
							Application.DoEvents();
						}
					}
				}

				if (bwImCreator_ != null)
				{
					if (bwImCreator_.IsBusy)
					{
						bwImCreator_.CancelAsync();

						while (bwImCreator_.IsBusy)
						{
							Thread.Sleep(1000);
							Application.DoEvents();
						}
					}
				}

				if (bwSaver_ != null)
				{
					if (bwSaver_.IsBusy)
					{
						bwSaver_.CancelAsync();

						while (bwSaver_.IsBusy)
						{
							Thread.Sleep(1000);
							Application.DoEvents();
						}
					}
				}

				if (bwExporter_ != null)
				{
					if (bwExporter_.IsBusy)
					{
						bwExporter_.CancelAsync();

						while (bwExporter_.IsBusy)
						{
							Thread.Sleep(1000);
							Application.DoEvents();
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsMiCancelSaving_Click():");
			}
			finally
			{
				bAutoMode_ = true;

				if (threadInfo != null)
				{
					if (OnHideWndInfo != null) OnHideWndInfo();
					thread.Abort(); threadInfo = null;
				}

				xmlImageEtPQP_ = null;
				xmlImageEtPQP_A_ = null;
				xmlImageEm33_ = null;
				xmlImageEm32_ = null;
				sqlImageEm32_ = null;
				sqlImageEm33_ = null; 
				sqlImageEtPQP_ = null;
				sqlImageEtPQP_A_ = null;
			}
		}

		private void tsbMomentData_Click(object sender, EventArgs e)
		{
			try
			{
				ResourceManager rm;
				settings_.LoadSettings();

				if(settings_.CurDeviceType != EmDeviceType.EM32 &&
					settings_.CurDeviceType != EmDeviceType.ETPQP_A)
				{
					MessageBoxes.InvalidDeviceFunction(this, settings_.CurDeviceType.ToString());
					return;
				}

				if (settings_.IOInterface == EmPortType.Modem)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						settings_.CurPhoneNumber = wnd.ActiveNumber;
						settings_.AttemptNumber = wnd.Attempts;
					}

					if (settings_.CurPhoneNumber == "")
					{
						rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
						string msg = rm.GetString("msg_device_no_active_number");
						string cap = rm.GetString("msg_device_connect_error_caption");

						MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
				}

				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						settings_.CurrentIPAddress = wnd.ActiveIPAddress;
						settings_.CurrentPort = Int32.Parse(wnd.ActivePort);
					}

					if (settings_.CurrentIPAddress == "" || settings_.CurrentPort == 0)
					{
						rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
						string msg = rm.GetString("msg_device_no_active_ipaddress");
						string cap = rm.GetString("msg_device_connect_error_caption");

						MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK, 
							MessageBoxIcon.Error);
						return;
					}
				}

				if (settings_.IOInterface == EmPortType.GPRS)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						settings_.CurrentIPAddress = wnd.ActiveIPAddress;
						settings_.CurrentPort = Int32.Parse(wnd.ActivePort);
					}

					if (settings_.CurrentIPAddress == "" || settings_.CurrentPort == 0)
					{
						rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
						string msg = rm.GetString("msg_device_no_active_ipaddress");
						string cap = rm.GetString("msg_device_connect_error_caption");

						MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK, 
							MessageBoxIcon.Error);
						return;
					}
				}

				// determine device address
				if (settings_.IOInterface == EmPortType.Rs485)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						settings_.CurDeviceAddress = wnd.ActiveDevAddress485;
					}

					//if (settings.CurDeviceAddress <= 0)
					//{
					//    rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					//    string msg = rm.GetString("msg_device_no_active_ipaddress");
					//    string cap = rm.GetString("msg_device_connect_error_caption");

					//    MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK, 
					//MessageBoxIcon.Error);
					//    return;
					//}
				}
				else if (settings_.IOInterface == EmPortType.Modem)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						settings_.CurDeviceAddress = wnd.ActiveDevAddressGSM;
					}
				}
				else if (settings_.IOInterface == EmPortType.GPRS)
				{
					using (frmSettings wnd = new frmSettings(settings_, this))
					{
						wnd.OpenDevicesInfo();
						settings_.CurDeviceAddress = wnd.ActiveDevAddressGPRS;
					}
				}
				else   // in other cases we have to set the broadcasting address
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				// create a form
				bAutoMode_ = false;
				if (settings_.CurDeviceType == EmDeviceType.EM32)
				{
					frmMomentData wndMoment = new frmMomentData(settings_, this);
					wndMoment.ShowDialog();
				}
				else
				{
					frmMomentDataEtPQP_A wndMoment = new frmMomentDataEtPQP_A(settings_, this);
					wndMoment.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsbMomentData_Click(): " + ex.Message);
			}
			finally
			{
				bAutoMode_ = true;
			}
		}

		#region MainToolStrip buttons

		private void tbsSettings_Click(object sender, EventArgs e)
		{
			msServiceSettings_Click(sender, e);
		}

		private void tbsConstraints_Click(object sender, EventArgs e)
		{
			msToolsConstraints_Click(sender, e);
		}

		private void tbsHelp_Click(object sender, EventArgs e)
		{
			msHelpHelp_Click(sender, e);
		}

		#endregion

		#region Menu File

		private void mmFileLoadFromDevice_Click(object sender, System.EventArgs e)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				xmlImageEtPQP_ = null;
				xmlImageEtPQP_A_ = null;
				xmlImageEm33_ = null; 
				xmlImageEm32_ = null;
				sqlImageEm32_ = null;
				sqlImageEm33_ = null; 
				sqlImageEtPQP_ = null;
				sqlImageEtPQP_A_ = null;
				//GC.Collect();

				ResourceManager rm;
				settings_.LoadSettings();

				currentDevType_ = settings_.CurDeviceType;
				currentAction_ = EmAction.SaveFromDevice;
				bCreateImageOnly_ = false;

				if (settings_.CurServerIndex == -1)
				{
					rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					string msg = rm.GetString("msg_device_no_server_selected");
					string cap = "Error";
					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (settings_.IOInterface == EmPortType.WI_FI && string.IsNullOrEmpty(settings_.CurWifiProfileName))
				{
					if (settings_.CurrentLanguage == "ru") MessageBox.Show("Необходимо выбрать имя подключения Wi-Fi в окне настроек!");
					else MessageBox.Show("You must select Wi-Fi connection name in Settings window!");
					return;
				}

				using (frmSettings wnd = new frmSettings(settings_, this))
				{
					wnd.OpenDevicesInfo();

					if (settings_.IOInterface == EmPortType.Modem)
					{
						settings_.CurPhoneNumber = wnd.ActiveNumber;
						settings_.AttemptNumber = wnd.Attempts;

						if (settings_.CurPhoneNumber == "")
						{
							rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_active_number");
							string cap = rm.GetString("msg_device_connect_error_caption");

							MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK, 
								MessageBoxIcon.Error);
							return;
						}
					}
					else if (settings_.IOInterface == EmPortType.Ethernet)
					{
						settings_.CurrentIPAddress = wnd.ActiveIPAddress;
						settings_.CurrentPort = Int32.Parse(wnd.ActivePort);

						if (settings_.CurrentIPAddress == "" || settings_.CurrentPort == 0 ||
							settings_.CurrentIPAddress == "0.0.0.0")
						{
							rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_active_ipaddress");
							string cap = rm.GetString("msg_device_connect_error_caption");

							MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK,
								MessageBoxIcon.Error);
							return;
						}
					}
					else if (settings_.IOInterface == EmPortType.GPRS)
					{
						settings_.CurrentIPAddress = wnd.ActiveIPAddress;
						settings_.CurrentPort = Int32.Parse(wnd.ActivePort);

						if (settings_.CurrentIPAddress == "" || settings_.CurrentPort == 0 ||
							settings_.CurrentIPAddress == "0.0.0.0")
						{
							rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_active_ipaddress");
							string cap = rm.GetString("msg_device_connect_error_caption");

							MessageBox.Show(sender as Form, msg, cap, MessageBoxButtons.OK,
								MessageBoxIcon.Error);
							return;
						}
					}
					////////////////////////////////
					if (settings_.IOInterface == EmPortType.Rs485)
					{
						settings_.CurDeviceAddress = wnd.ActiveDevAddress485;

						if (settings_.CurDeviceAddress <= 0)
						{
							rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_active_485address");
							string cap = rm.GetString("msg_device_connect_error_caption");

							MessageBox.Show(sender as Form, msg, cap,
											MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}
					}
					else if (settings_.IOInterface == EmPortType.Modem)
					{
						settings_.CurDeviceAddress = wnd.ActiveDevAddressGSM;

						if (settings_.CurDeviceAddress <= 0)
						{
							rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_active_485address");
							string cap = rm.GetString("msg_device_connect_error_caption");

							MessageBox.Show(sender as Form, msg, cap,
											MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}
					}
					else if (settings_.IOInterface == EmPortType.GPRS)
					{
						settings_.CurDeviceAddress = wnd.ActiveDevAddressGPRS;

						if (settings_.CurDeviceAddress <= 0)
						{
							rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_active_485address");
							string cap = rm.GetString("msg_device_connect_error_caption");

							MessageBox.Show(sender as Form, msg, cap,
											MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}
					}
					else   // if not RS-485 then we have to set the broadcasting address
					{
						settings_.CurDeviceAddress = 0xFFFF;
					}
				}

				bAutoMode_ = false;

				StartConnectionToDevice();

				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in mmFileLoadFromDevice_Click(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
			}
		}

		private void msFileOpenImageFile_Click(object sender, EventArgs e)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				//GC.Collect();
				// open file dialog
				OpenFileDialog fd = new OpenFileDialog();
				fd.RestoreDirectory = true;

				string allFilter = "";
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					allFilter = String.Format(
						"Все Файлы Образа (*.{0})|*.{0}", "xml");
				else
					allFilter = String.Format(
						"All Image files (*.{0})|*.{0}", "xml");

				fd.DefaultExt = "xml";//EmDataSaver.EmSqlImageCreator32.ImageFileExtention;
				fd.AddExtension = true;
				fd.Filter = allFilter;
				fd.Filter += "|" + EmDataSaver.EmSqlImageCreator33.ImageFilter;
				fd.Filter += "|" + EmDataSaver.EmSqlImageCreator32.ImageFilter;
				fd.Filter += "|" + EmDataSaver.EtSqlImageCreatorPQP.ImageFilter;
				fd.Filter += "|" + EmDataSaver.EmSqlImageCreator33.ImageFilter33T1;
				fd.Filter += "|" + EmDataSaver.EmSqlImageCreator33.ImageFilter31K;
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					fd.Filter += "|Все файлы (*.*)|*.*";
				else
					fd.Filter += "|All files (*.*)|*.*";
				if (fd.ShowDialog(this) != DialogResult.OK) return;

				// определяем с какого устройства был сформирован образ
				string contens = "";
				int iLine = 0;
				using (FileStream fs = new FileStream(fd.FileName, FileMode.Open))
				{
					using (StreamReader sr = new StreamReader(fs))
					{
						try
						{
							while (sr.Peek() >= 0 && iLine < 100)
							{
								contens += sr.ReadLine();
								++iLine;
							}
						}
						finally
						{
							if (sr != null) sr.Close(); 
							if (fs != null) fs.Close(); 
						}
					}
				}

				//определение типа устройства по расширению файла
				currentDevType_ = EmDeviceType.EM33T;
				if (fd.FileName.ToLower().Contains("em32"))
					currentDevType_ = EmDeviceType.EM32;
				else if (fd.FileName.ToLower().Contains("em33t1"))
					currentDevType_ = EmDeviceType.EM33T1;
				else if (fd.FileName.ToLower().Contains("etpqp_a"))
					currentDevType_ = EmDeviceType.ETPQP_A;
				else if (fd.FileName.ToLower().Contains("etpqp"))
					currentDevType_ = EmDeviceType.ETPQP;

				currentAction_ = EmAction.SaveFromFile;
				bCreateImageOnly_ = false;

				// Connecting to the database server
				if (!ConnectToServer(settings_.CurServerIndex))
					throw new EmException(
						"msFileOpenImageFile_Click: Unable to connect to the server!");

				bAutoMode_ = false;

				// если это xml-образ
				if (fd.FileName.EndsWith(".debug.xml") ||
					fd.FileName.EndsWith(".devinfo.xml"))
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
					string msgDebugText = string.Format(rm.GetString("msg_image_tryopendebugfile_text"),
						fd.FileName);
					string msgDebugCap = rm.GetString("warning_caption");
					if (MessageBox.Show(this,
										msgDebugText,
										msgDebugCap,
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Warning) != DialogResult.Yes)
						return;

					bool res = false;
					switch (currentDevType_)
					{
						case EmDeviceType.EM31K:
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
							xmlImageEm33_ = new EmXmlDeviceImage();
							res = loadXmlImage(fd.FileName, ref xmlImageEm33_);
							break;
						case EmDeviceType.EM32:
							xmlImageEm32_ = new Em32XmlDeviceImage();
							res = loadXmlImage(fd.FileName, ref xmlImageEm32_);
							break;
						case EmDeviceType.ETPQP:
							xmlImageEtPQP_ = new EtPQPXmlDeviceImage();
							res = loadXmlImage(fd.FileName, ref xmlImageEtPQP_);
							break;
						case EmDeviceType.ETPQP_A:
							xmlImageEtPQP_A_ = new EtPQP_A_XmlDeviceImage();
							res = loadXmlImage(fd.FileName, ref xmlImageEtPQP_A_);
							break;
						default: throw new EmException("Unknown device type!");
					}

					if (!res)
					{
						MessageBoxes.ErrorLoadXmlImage(this);
						bAutoMode_ = true;
						return;
					}

					StartImageCreating();
				}
				else
				{
					bool res = false;
					switch (currentDevType_)
					{
						case EmDeviceType.EM31K:
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
							sqlImageEm33_ = new EmSqlDeviceImage();
							res = loadSqlImage(fd.FileName, ref sqlImageEm33_);
							break;
						case EmDeviceType.EM32:
							sqlImageEm32_ = new EmSqlEm32Device();
							res = loadSqlImage(fd.FileName, ref sqlImageEm32_);
							break;
						case EmDeviceType.ETPQP:
							sqlImageEtPQP_ = new EtPQPSqlDeviceImage();
							res = loadSqlImage(fd.FileName, ref sqlImageEtPQP_);
							break;
						case EmDeviceType.ETPQP_A:
							sqlImageEtPQP_A_ = new EtPQP_A_SqlDeviceImage();
							res = loadSqlImage(fd.FileName, ref sqlImageEtPQP_A_);
							break;
						default: throw new EmException("Unknown device type!");
					}

					if (!res)
					{
						MessageBoxes.ErrorLoadSqlImage(this);
						bAutoMode_ = true;
						return;
					}

					StartInsertToDB();
				}
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in msFileOpenImageFile_Click(): " + emx.Message);
				bAutoMode_ = true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in msFileOpenImageFile_Click():");
				bAutoMode_ = true;
				throw;
			}
			Environment.CurrentDirectory = EmService.AppDirectory;
		}

		private void msFileExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		#endregion

		#region Menu Tools

		private void msToolsObjectNames_Click(object sender, EventArgs e)
		{
			frmObjectNames wndObjectNames = new frmObjectNames(settings_, this);
			wndObjectNames.ShowDialog(this);
		}

		private void msToolsDevName_Click(object sender, EventArgs e)
		{
			if (settings_.CurDeviceType != EmDeviceType.EM32)
				return;
			frmDeviceName wnd = new frmDeviceName(settings_, this);
			wnd.ShowDialog(this);
		}

		private void msToolsConstraints_Click(object sender, EventArgs e)
		{
			frmConstraints wndConstraints = new frmConstraints(settings_, this);
			wndConstraints.ShowDialog(this);
		}

		private void msToolsNominalValues_Click(object sender, EventArgs e)
		{
			frmNominalsAndTimes wndNominals = new frmNominalsAndTimes(settings_, this);
			wndNominals.ShowDialog(this);
		}

		private void msToolsDeclaredU_Click(object sender, EventArgs e)
		{
			frmDeclaredU wndDeclaredU = new frmDeclaredU(ref settings_, this);
			wndDeclaredU.ShowDialog(this);
		}

		private void msToolsExportToExcelPQP_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialog fd = new SaveFileDialog();
				fd.DefaultExt = "xls";
				fd.AddExtension = true;
				fd.FileName = String.Format("Показатели качества объекта {0}.xls",//?????????????? for english version
					curArchive_.ObjectName);
				fd.Filter = "Файлы Microsoft Excel (*.xls)|*.xls|Все файлы (*.*)|*.*";
				if (fd.ShowDialog(this) != DialogResult.OK) return;

				// for EtPQP_A only
				DEVICE_VERSIONS newDipSwellMode = DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073;

				EmDeviceType devType = wndToolbox.ActiveNodePQP.DeviceType;
				ConnectScheme conScheme = ConnectScheme.Ph3W4;
				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1)
				{
					EmTreeNodeObject objNode = (EmTreeNodeObject)this.wndToolbox.ActiveNodePQP.ParentObject;
					conScheme = objNode.ConnectionScheme;
				}
				else if (devType == EmDeviceType.EM32)
				{
					EmTreeNodeEm32Device devNode =
							(EmTreeNodeEm32Device)this.wndToolbox.ActiveNodePQP.ParentDevice;
					conScheme = devNode.ConnectionScheme;
				}
				else if (devType == EmDeviceType.ETPQP)
				{
					EmTreeNodeObject objNode = (EmTreeNodeObject)this.wndToolbox.ActiveNodePQP.ParentObject;
					conScheme = objNode.ConnectionScheme;
				}
				else if (devType == EmDeviceType.ETPQP_A)
				{
					EmTreeNodeRegistration regNode = (EmTreeNodeRegistration)this.wndToolbox.ActiveNodePQP.ParentObject;
					conScheme = regNode.ConnectionScheme;
					newDipSwellMode = Constants.isNewDeviceVersion_ETPQP_A(regNode.DeviceVersion);
				}

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				XML.Excel.Exporter xlExporter = new XML.Excel.Exporter(settings_.FloatSigns);

				xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFrequencyDeparture,
									rm.GetString("name.measure_type.pqp.frequency_departure"));
				if (devType != EmDeviceType.EM32)
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFValues,
									rm.GetString("name.measure_type.pqp.frequency_val_departure"));
				xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgU_Deviation,
									rm.GetString("name.measure_type.pqp.voltage_deviation"));
				if(devType != EmDeviceType.EM32)
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgVolValues,
									rm.GetString("name.measure_type.pqp.voltage_val_deviation"));

				if (devType == EmDeviceType.EM32 || conScheme != ConnectScheme.Ph1W2)
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgNonSymmetry,
									rm.GetString("name_measure_type_pqp_voltage_unbalance"));

				// если есть фазные
				if (!this.wndDocPQP.wndDocPQPMain.splitContainerVoltHarm1.Panel1Collapsed)
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgUNonsinusoidality,
									rm.GetString("name.measure_type.pqp.voltage_nonsinusoidality"));
				// если есть линейные
				if (!this.wndDocPQP.wndDocPQPMain.splitContainerVoltHarm1.Panel2Collapsed)
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgUNonsinusoidality2,
									rm.GetString("name_measure_type_pqp_nonsinusoidality2"));

				xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgDips,
									rm.GetString("name_measure_type_pqp_dips"));
				if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
				{
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgDips2,
										rm.GetString("name_measure_type_pqp_dips_lin"));
				}
				if (devType == EmDeviceType.ETPQP_A && newDipSwellMode == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
				{
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgDips2,
										rm.GetString("str_interrupt"));
				}
				xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgOvers,
									rm.GetString("name_measure_type_pqp_overs"));
				if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
				{
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgOvers2,
										rm.GetString("name_measure_type_pqp_overs_lin"));
				}

				if (conScheme != ConnectScheme.Ph3W3 &&
					conScheme != ConnectScheme.Ph3W3_B_calc)
				{
					if (devType == EmDeviceType.ETPQP_A)
					{
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerNum,
										rm.GetString("name.measure_type.pqp.fliker"));
					}

					if (devType == EmDeviceType.EM33T)
					{
						// check device version for this archive
						string devVersion =
							(wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).DeviceVersion;

						// add fliker dataGrids
						if (Constants.isNewDeviceVersion_EM33T(devVersion))
						{
							xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlicker,
										rm.GetString("name.measure_type.pqp.fliker_short"));

							xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerLong,
										rm.GetString("name.measure_type.pqp.fliker_long"));
						}
					}
					else if (devType == EmDeviceType.EM33T1)
					{
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlicker,
										rm.GetString("name.measure_type.pqp.fliker_short"));
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerLong,
										rm.GetString("name.measure_type.pqp.fliker_long"));
					}
					else if (devType != EmDeviceType.EM31K)	// см. внимательно, что тут !=
					{
						// здесь кажется была проблема
						// из-за того, что в таблице не все строки заполнены,
						// но теперь почему-то все нормально
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlicker,
										rm.GetString("name.measure_type.pqp.fliker_short"));
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerLong,
										rm.GetString("name.measure_type.pqp.fliker_long"));
					}
				}

				if (devType == EmDeviceType.ETPQP_A)
				{
					xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgInterharm,
										rm.GetString("name_measure_type_pqp_interharm"));
				}

				xlExporter.Title = "Показатели Качества Энергии объекта " + curArchive_.ObjectName;
				xlExporter.Company = "Mars-Energo Ltd.";

				try
				{
					xlExporter.Open(fd.FileName);
					xlExporter.Export();
				}
				catch (IOException)
				{
					MessageBoxes.ErrorCantAccessFile(this);
					return;
				}
				finally
				{
					xlExporter.Close();
				}
			}
			catch (NullReferenceException nex)
			{
				EmService.DumpException(nex, 
					"NullReferenceException in msToolsExportToExcelPQP_Click()");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in msToolsExportToExcelPQP_Click()");
				throw;
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}

		private void msToolsExportToExcelDNS_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialog fd = new SaveFileDialog();
				fd.DefaultExt = "xls";
				fd.AddExtension = true;
				fd.FileName = String.Format("Провалы и перенапряжения объекта {0}.xls",
					curArchive_.ObjectName);
				fd.Filter = "Файлы Microsoft Excel (*.xls)|*.xls|Все файлы (*.*)|*.*";
				if (fd.ShowDialog(this) != DialogResult.OK) return;

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
					this.GetType().Assembly);
				XML.Excel.Exporter xlExporter = new XML.Excel.Exporter(settings_.FloatSigns);

				DataGridView dgSource = wndDocDNS.wndDocDNSMain.dgvDNS;
				DataGrid dgDest = new DataGrid();

				DataGridTableStyle ts = new DataGridTableStyle();
				DataGridColumnHeaderFormula cs_f;

				System.ComponentModel.ComponentResourceManager resources =
					new System.ComponentModel.ComponentResourceManager(typeof(frmDocDNSMain));

				EmDeviceType devType = wndToolbox.ActiveNodeDNS.DeviceType;

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colStart";
				resources.ApplyResources(cs_f, "colStart");
				ts.GridColumnStyles.Add(cs_f);

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colEnd";
				resources.ApplyResources(cs_f, "colEnd");
				ts.GridColumnStyles.Add(cs_f);

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colDuration";
				resources.ApplyResources(cs_f, "colDuration");
				ts.GridColumnStyles.Add(cs_f);

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colEvent";
				resources.ApplyResources(cs_f, "colEvent");
				cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
				ts.GridColumnStyles.Add(cs_f);

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colPhase";
				resources.ApplyResources(cs_f, "colPhase");
				cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
				ts.GridColumnStyles.Add(cs_f);

				if (devType == EmDeviceType.ETPQP_A)
				{
					cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
					cs_f.MappingName = "colU";
					resources.ApplyResources(cs_f, "colU");
					cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
					ts.GridColumnStyles.Add(cs_f);
				}

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colDeviation";
				resources.ApplyResources(cs_f, "colDeviation");
				cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
				ts.GridColumnStyles.Add(cs_f);

				if (devType == EmDeviceType.ETPQP_A)
				{
					cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
					cs_f.MappingName = "colUDeclared";
					resources.ApplyResources(cs_f, "colUDeclared");
					cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
					ts.GridColumnStyles.Add(cs_f);
				}

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colFinished";
				resources.ApplyResources(cs_f, "colFinished");
				cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
				ts.GridColumnStyles.Add(cs_f);

				cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.MappingName = "colEarlier";
				resources.ApplyResources(cs_f, "colEarlier");
				cs_f.BackgroungColor = DataGridColors.ColorPqpParam;
				ts.GridColumnStyles.Add(cs_f);

				dgDest.SetDataBinding(dgSource.DataSource, "dataTableDNS");
				dgDest.TableStyles.Add(ts);

				xlExporter.Grids.Add(dgDest, rm.GetString("name.measure_type.avg.dips_and_swells"));

				xlExporter.Company = "Mars-Energo Ltd.";
				xlExporter.Title = "Провалы и перенапряжения объекта " + curArchive_.ObjectName;

				try
				{
					xlExporter.Open(fd.FileName);
					xlExporter.Export();
				}
				catch (IOException)
				{
					MessageBoxes.ErrorCantAccessFile(this);
					return;
				}
				finally
				{
					xlExporter.Close();
				}
			}
			catch (NullReferenceException nex)
			{
				EmService.DumpException(nex,
					"NullReferenceException in msToolsExportToExcelDNS_Click()");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in msToolsExportToExcelDNS_Click()");
				throw;
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}

		private void msToolsExportToExcelAVG_Click(object sender, EventArgs e)
		{
			try
			{
				EmDeviceType devType = this.wndToolbox.ActiveNodeAVG.DeviceType;
				ConnectScheme conSheme = ConnectScheme.Unknown;

				int cntPages = 12;
				bool[] pages = new bool[cntPages];
				pages[(int)AvgPages.F_U_I] = true;
				pages[(int)AvgPages.POWER] = true;
				pages[(int)AvgPages.ANGLES] = true;
				pages[(int)AvgPages.PQP] = true;
				pages[(int)AvgPages.I_HARMONICS] = true;
				pages[(int)AvgPages.HARMONIC_POWERS] = true;

				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1)
				{
					EmTreeNodeObject objNode = (EmTreeNodeObject)this.wndToolbox.ActiveNodeAVG.ParentObject;
					conSheme = objNode.ConnectionScheme;

					if (conSheme != ConnectScheme.Ph3W3 &&
						conSheme != ConnectScheme.Ph3W3_B_calc)
					{
						pages[(int)AvgPages.U_PH_HARMONICS] = true;
						pages[(int)AvgPages.U_LIN_HARMONICS] = false;
					}
					else
					{
						pages[(int)AvgPages.U_LIN_HARMONICS] = true;
						pages[(int)AvgPages.U_PH_HARMONICS] = false;
					}
					if (conSheme != ConnectScheme.Ph3W3 && conSheme != ConnectScheme.Ph3W3_B_calc)
						pages[(int)AvgPages.HARMONIC_ANGLES] = true;
					else pages[(int)AvgPages.HARMONIC_ANGLES] = false;
				}
				else if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
				{
					if (devType == EmDeviceType.EM32)
					{
						EmTreeNodeEm32Device devNode =
							(EmTreeNodeEm32Device)this.wndToolbox.ActiveNodeAVG.ParentDevice;
						conSheme = devNode.ConnectionScheme;
					}
					else
					{
						EmTreeNodeObject objNode = (EmTreeNodeObject)this.wndToolbox.ActiveNodeAVG.ParentObject;
						conSheme = objNode.ConnectionScheme;
					}

					if (conSheme != ConnectScheme.Ph3W3 &&
						conSheme != ConnectScheme.Ph3W3_B_calc)
					{
						pages[(int)AvgPages.U_PH_HARMONICS] = true;	// "avg_harmonics_ph_voltage"
					}
					else pages[(int)AvgPages.U_PH_HARMONICS] = false;
					if (conSheme != ConnectScheme.Ph1W2)
					{
						pages[(int)AvgPages.U_LIN_HARMONICS] = true;	// "avg_harmonics_lin_voltage"
					}
					else pages[(int)AvgPages.U_LIN_HARMONICS] = false;
					if (conSheme != ConnectScheme.Ph3W3 && conSheme != ConnectScheme.Ph3W3_B_calc)
						pages[(int)AvgPages.HARMONIC_ANGLES] = true;
					else pages[(int)AvgPages.HARMONIC_ANGLES] = false;
				}
				if (devType != EmDeviceType.ETPQP_A)
				{
					pages[(int)AvgPages.I_INTERHARM] = false;
					pages[(int)AvgPages.U_LIN_INTERHARM] = false;
					pages[(int)AvgPages.U_PH_INTERHARM] = false;
				}
				else
				{
					pages[(int)AvgPages.HARMONIC_ANGLES] = false;

					EmTreeNodeRegistration regNode = (EmTreeNodeRegistration)this.wndToolbox.ActiveNodeAVG.ParentObject;
					conSheme = regNode.ConnectionScheme;

					pages[(int)AvgPages.I_INTERHARM] = true;

					if (conSheme != ConnectScheme.Ph3W3 &&
						conSheme != ConnectScheme.Ph3W3_B_calc)
					{
						pages[(int)AvgPages.U_PH_HARMONICS] = true;
						pages[(int)AvgPages.U_PH_INTERHARM] = true;
						pages[(int)AvgPages.U_LIN_HARMONICS] = false;
						pages[(int)AvgPages.U_LIN_INTERHARM] = false;
					}
					else
					{
						pages[(int)AvgPages.U_PH_HARMONICS] = false;
						pages[(int)AvgPages.U_PH_INTERHARM] = false;
						pages[(int)AvgPages.U_LIN_HARMONICS] = true;
						pages[(int)AvgPages.U_LIN_INTERHARM] = true;
					}
				}

				frmAvgPages wnd = new frmAvgPages(pages);
				if (wnd.ShowDialog() != DialogResult.OK)
					return;
				pages = wnd.PagesSelected;

				SaveFileDialog fd = new SaveFileDialog();
				fd.DefaultExt = "xls";
				fd.AddExtension = true;
				fd.FileName = String.Format("Усредненные значения объекта {0}.xls",
					curArchive_.ObjectName);
				fd.Filter = "Файлы Microsoft Excel (*.xls)|*.xls|Все файлы (*.*)|*.*";
				if (fd.ShowDialog(this) != DialogResult.OK) return;

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				XML.Excel.Exporter xlExporter = new XML.Excel.Exporter(settings_.FloatSigns);

				if (wndDocAVG.wndDocAVGMain.DgUIF.TableStyles.Count > 0 &&
						pages[(int)AvgPages.F_U_I])
					xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgUIF,
						rm.GetString("avg_currents_and_voltages"));
				if (wndDocAVG.wndDocAVGMain.DgPower.TableStyles.Count > 0 &&
					pages[(int)AvgPages.POWER])
					xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgPower,
						rm.GetString("avg_powers"));
				if (wndDocAVG.wndDocAVGMain.DgAngles.TableStyles.Count > 0 &&
					pages[(int)AvgPages.ANGLES])
					xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgAngles,
						rm.GetString("avg_angles"));
				if (wndDocAVG.wndDocAVGMain.DgPQP.TableStyles.Count > 0 &&
					pages[(int)AvgPages.PQP])
					xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgPQP,
						rm.GetString("avg_pqp"));
				if (wndDocAVG.wndDocAVGMain.DgHarmI.TableStyles.Count > 0 &&
					pages[(int)AvgPages.I_HARMONICS])
					xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmI,
						rm.GetString("avg_harmonics_current"));
				if (wndDocAVG.wndDocAVGMain.DgHarmPower.TableStyles.Count > 0 &&
						pages[(int)AvgPages.HARMONIC_POWERS])
					xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmPower,
						rm.GetString("avg_harmonic_powers"));

				if(devType != EmDeviceType.ETPQP_A)
					if (wndDocAVG.wndDocAVGMain.DgHarmAngles.TableStyles.Count > 0 &&
						(conSheme != ConnectScheme.Ph3W3 && conSheme != ConnectScheme.Ph3W3_B_calc) &&
						pages[(int)AvgPages.HARMONIC_ANGLES])
						xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmAngles,
							rm.GetString("avg_harmonic_angles"));

				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1 || devType == EmDeviceType.ETPQP_A)
				{
					if (conSheme != ConnectScheme.Ph3W3 &&
						conSheme != ConnectScheme.Ph3W3_B_calc)
					{
						if (wndDocAVG.wndDocAVGMain.DgHarmUph.TableStyles.Count > 0 &&
							pages[(int)AvgPages.U_PH_HARMONICS])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmUph,
								rm.GetString("avg_harmonics_ph_voltage"));
					}
					else
					{
						if (wndDocAVG.wndDocAVGMain.DgHarmUlin.TableStyles.Count > 0 &&
							pages[(int)AvgPages.U_LIN_HARMONICS])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmUlin,
								rm.GetString("avg_harmonics_lin_voltage"));
					}

					if (devType == EmDeviceType.ETPQP_A)
					{
						if (wndDocAVG.wndDocAVGMain.DgInterHarmI.TableStyles.Count > 0 &&
								pages[(int)AvgPages.I_INTERHARM])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgInterHarmI,
								rm.GetString("avg_interharmonics_current"));

						if (wndDocAVG.wndDocAVGMain.DgInterHarmUph.TableStyles.Count > 0 &&
								pages[(int)AvgPages.U_PH_INTERHARM])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgInterHarmUph,
								rm.GetString("avg_interharmonics_ph_voltage"));

						if (wndDocAVG.wndDocAVGMain.DgInterHarmUlin.TableStyles.Count > 0 &&
								pages[(int)AvgPages.U_LIN_INTERHARM])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgInterHarmUlin,
								rm.GetString("avg_interharmonics_lin_voltage"));
					}
				}
				else if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
				{
					if (conSheme != ConnectScheme.Ph3W3 &&
						conSheme != ConnectScheme.Ph3W3_B_calc)
					{
						if (wndDocAVG.wndDocAVGMain.DgHarmUph.TableStyles.Count > 0 &&
							pages[(int)AvgPages.U_PH_HARMONICS])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmUph,
								rm.GetString("avg_harmonics_ph_voltage"));
					}
					if (conSheme != ConnectScheme.Ph1W2)
					{
						if (wndDocAVG.wndDocAVGMain.DgHarmUlin.TableStyles.Count > 0 &&
							pages[(int)AvgPages.U_LIN_HARMONICS])
							xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmUlin,
								rm.GetString("avg_harmonics_lin_voltage"));
					}
				}

				xlExporter.Company = "Mars-Energo Ltd.";
				xlExporter.Title = "Усредненные значения объекта " + curArchive_.ObjectName;

				try
				{
					xlExporter.Open(fd.FileName);
					xlExporter.Export();
				}
				catch (IOException)
				{
					MessageBoxes.ErrorCantAccessFile(this);
					return;
				}
				finally
				{
					xlExporter.Close();
				}
			}
			catch (NullReferenceException nex)
			{
				EmService.DumpException(nex,
					"NullReferenceException in msToolsExportToExcelAVG_Click()");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in msToolsExportToExcelAVG_Click() 2:");
				throw;
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}

		private void miReportEtPqpA_Click(object sender, EventArgs e)
		{
			CreateReportEtPqpA(PQPProtocolType.VERSION_1);
		}

		private void miReportEtPqpA_v2_Click(object sender, EventArgs e)
		{
			CreateReportEtPqpA(PQPProtocolType.VERSION_2);
		}

		private void miReportEtPqpA_v3_Click(object sender, EventArgs e)
		{
			CreateReportEtPqpA(PQPProtocolType.VERSION_3);
		}

		private void CreateReportEtPqpA(PQPProtocolType version)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				if (this.wndToolbox.ActiveNodePQP == null) return;

				EmDeviceType devType = this.wndToolbox.ActiveNodePQP.DeviceType;
				if (devType != EmDeviceType.ETPQP_A) return;

				EmTreeNodeRegistration reg = (this.wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeRegistration);

				if (Constants.isNewDeviceVersion_ETPQP_A(reg.DeviceVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
				{
					frmTemperatureReport frm = new frmTemperatureReport();
					if (frm.ShowDialog() == DialogResult.Cancel)
						return;
					this.wndDocPQP.wndDocPQPMain.ExportPQPReport_PQP_A(reg.ConnectionScheme, reg.DeviceSerNumber,
							  version, reg.GPS_Latitude, reg.GPS_Longitude, //reg.Autocorrect_time_gps_enable,
							  frm.TemperatureMin, frm.TemperatureMax);
				}
				else
				{
					this.wndDocPQP.wndDocPQPMain.ExportPQPReport_PQP_A(reg.ConnectionScheme, reg.DeviceSerNumber,
							  version, reg.GPS_Latitude, reg.GPS_Longitude,
							  Constants.isNewDeviceVersion_ETPQP_A(reg.DeviceVersion));
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in CreateReportEtPqpA()");
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}
		
		private void miReportGOST_Click(object sender, EventArgs e)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				if (this.wndToolbox.ActiveNodePQP == null) return;

				EmDeviceType devType = this.wndToolbox.ActiveNodePQP.DeviceType;

				ConnectScheme cs = ConnectScheme.Ph3W4;
				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1)
					cs = (this.wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).ConnectionScheme;
				else if (devType == EmDeviceType.EM32)
				{
					EmTreeNodeEm32Device parentDev =
						this.wndToolbox.ActiveNodePQP.ParentDevice as EmTreeNodeEm32Device;
					cs = parentDev.ConnectionScheme;
				}
				else if (devType == EmDeviceType.ETPQP ||
						devType == EmDeviceType.ETPQP_A)
					cs = (this.wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).ConnectionScheme;

				bool flikkerExists = false;
				if (cs != ConnectScheme.Ph3W3 && cs != ConnectScheme.Ph3W3_B_calc)
				{
					if (devType == EmDeviceType.EM33T)
					{
						// check device version for this archive
						string devVersion =
							(wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).DeviceVersion;
						flikkerExists = Constants.isNewDeviceVersion_EM33T(devVersion);
					}
					else if (devType == EmDeviceType.EM31K)
						flikkerExists = false;
					else flikkerExists = true;
				}

				this.wndDocPQP.wndDocPQPMain.ExportReportPQP_GOST(cs, flikkerExists);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in miReportGOST_Click()");
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}

		private void miReportRD_Click(object sender, EventArgs e)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				if (this.wndToolbox.ActiveNodePQP == null) return;

				EmDeviceType devType = this.wndToolbox.ActiveNodePQP.DeviceType;

				ConnectScheme cs = ConnectScheme.Ph3W4;
				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1)
					cs = (this.wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).ConnectionScheme;
				else if (devType == EmDeviceType.EM32)
				{
					EmTreeNodeEm32Device parentDev =
						this.wndToolbox.ActiveNodePQP.ParentDevice as EmTreeNodeEm32Device;
					cs = parentDev.ConnectionScheme;
				}
				else if (devType == EmDeviceType.ETPQP ||
						devType == EmDeviceType.ETPQP_A)
					cs = (this.wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).ConnectionScheme;

				bool flikkerExists = false;
				if (cs != ConnectScheme.Ph3W3 && cs != ConnectScheme.Ph3W3_B_calc)
				{
					if (devType == EmDeviceType.EM33T)
					{
						// check device version for this archive
						string devVersion =
							(wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).DeviceVersion;
						flikkerExists = Constants.isNewDeviceVersion_EM33T(devVersion);
					}
					else if (devType == EmDeviceType.EM31K)
						flikkerExists = false;
					else flikkerExists = true;
				}

				this.wndDocPQP.wndDocPQPMain.ExportPQPReport_RD(cs, flikkerExists);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in miReportRD_Click()");
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}

		private void miReportFSK_Click(object sender, EventArgs e)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				if (this.wndToolbox.ActiveNodePQP == null ||
					this.wndToolbox.ActiveNodeAVG == null)
				{
					MessageBoxes.MsgOpenArchiveForFSK(this);
					return;
				}
				//if (this.wndToolbox.ActiveNodeDNS == null) return;

				EmDeviceType devType = wndToolbox.ActiveNodePQP.DeviceType;
				ConnectScheme conScheme = ConnectScheme.Ph3W4;
				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1)
				{
					EmTreeNodeObject objNode = (EmTreeNodeObject)this.wndToolbox.ActiveNodePQP.ParentObject;
					EmTreeNodeObject objNodeAVG = (EmTreeNodeObject)this.wndToolbox.ActiveNodeAVG.ParentObject;
					EmTreeNodeObject objNodeDNS = null;
					if (this.wndToolbox.ActiveNodeDNS != null)
						objNodeDNS = (EmTreeNodeObject)this.wndToolbox.ActiveNodeDNS.ParentObject;
					if (!objNode.Equals(objNodeAVG))
					{
						MessageBoxes.MsgOpenArchiveForFSK_TheSameObject(this);
						return;
					}
					if (objNodeDNS != null && (!objNodeDNS.Equals(objNode) || !objNodeDNS.Equals(objNodeAVG)))
					{
						MessageBoxes.MsgOpenArchiveForFSK_TheSameObject(this);
						return;
					}

					conScheme = objNode.ConnectionScheme;
				}
				else if (devType == EmDeviceType.EM32)
				{
					EmTreeNodeEm32Device devNode =
							(EmTreeNodeEm32Device)this.wndToolbox.ActiveNodePQP.ParentDevice;
					EmTreeNodeObject devNodeAVG = (EmTreeNodeObject)this.wndToolbox.ActiveNodeAVG.ParentDevice;
					EmTreeNodeObject devNodeDNS = null;
					if (this.wndToolbox.ActiveNodeDNS != null)
						devNodeDNS = (EmTreeNodeObject)this.wndToolbox.ActiveNodeDNS.ParentDevice;
					if (!devNode.Equals(devNodeAVG))
					{
						MessageBoxes.MsgOpenArchiveForFSK_TheSameObject(this);
						return;
					}
					if (devNodeDNS != null && (!devNodeDNS.Equals(devNode) || !devNodeDNS.Equals(devNodeAVG)))
					{
						MessageBoxes.MsgOpenArchiveForFSK_TheSameObject(this);
						return;
					}

					conScheme = devNode.ConnectionScheme;
				}
				else if (devType == EmDeviceType.ETPQP)
				{
					EmTreeNodeObject objNode = (EmTreeNodeObject)this.wndToolbox.ActiveNodePQP.ParentObject;
					EmTreeNodeObject objNodeAVG = (EmTreeNodeObject)this.wndToolbox.ActiveNodeAVG.ParentObject;
					EmTreeNodeObject objNodeDNS = null;
					if (this.wndToolbox.ActiveNodeDNS != null)
						objNodeDNS = (EmTreeNodeObject)this.wndToolbox.ActiveNodeDNS.ParentObject;
					if (!objNode.Equals(objNodeAVG))
					{
						MessageBoxes.MsgOpenArchiveForFSK_TheSameObject(this);
						return;
					}
					if (objNodeDNS != null && (!objNodeDNS.Equals(objNode) || !objNodeDNS.Equals(objNodeAVG)))
					{
						MessageBoxes.MsgOpenArchiveForFSK_TheSameObject(this);
						return;
					}

					conScheme = objNode.ConnectionScheme;
				}

				SaveFileDialog fd = new SaveFileDialog();
				fd.DefaultExt = "xls";
				fd.AddExtension = true;
				fd.FileName = String.Format("Отчет по ПКЭ и Усредненным значениям {0}.xls",
					curArchive_.ObjectName);
				fd.Filter = "Файлы Microsoft Excel (*.xls)|*.xls|Все файлы (*.*)|*.*";
				if (fd.ShowDialog(this) != DialogResult.OK) return;

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				DataGridView dgvDNS = null;
				if (wndDocDNS != null && wndDocDNS.wndDocDNSMain != null)
					dgvDNS = wndDocDNS.wndDocDNSMain.dgvDNS;
				XML.Excel.ExporterFSK xlExporter = new XML.Excel.ExporterFSK(settings_.FloatSigns, 
													conScheme, devType,
													dgvDNS,
													settings_.CurrentRatio,
													settings_.VoltageRatio,
													settings_.PowerRatio);

				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgPQP,
									rm.GetString("avg_pqp"));
				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmUph,
										rm.GetString("name_params_voltage_harmonics_ph"));
				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmUlin,
										rm.GetString("name_params_voltage_harmonics_lin"));
				//xlExporter.Grids.Add(wndDocDNS.wndDocDNSMain.dgvDNS,
										//rm.GetString("name_params_voltage_harmonics_lin"));
				bool flikAdded = false;
				if (conScheme != ConnectScheme.Ph3W3 &&
					conScheme != ConnectScheme.Ph3W3_B_calc)
				{
					if (devType == EmDeviceType.EM33T)
					{
						// check device version for this archive
						string devVersion =
							(wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeObject).DeviceVersion;

						// add fliker dataGrids
						if (Constants.isNewDeviceVersion_EM33T(devVersion))
						{
							xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlicker,
										rm.GetString("name.measure_type.pqp.fliker"));

							xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerLong,
										rm.GetString("name.measure_type.pqp.fliker_long"));
							flikAdded = true;
						}
					}
					else if (devType == EmDeviceType.EM33T1)
					{
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlicker,
										rm.GetString("name.measure_type.pqp.fliker"));
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerLong,
										rm.GetString("name.measure_type.pqp.fliker_long"));
						flikAdded = true;
					}
					else if (devType != EmDeviceType.EM31K)
					{
						// здесь кажется была проблема
						// из-за того, что в таблице не все строки заполнены,
						// но теперь почему-то все нормально
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlicker,
										rm.GetString("name.measure_type.pqp.fliker"));
						xlExporter.Grids.Add(wndDocPQP.wndDocPQPMain.dgFlickerLong,
										rm.GetString("name.measure_type.pqp.fliker_long"));
						flikAdded = true;
					}
				}
				if (!flikAdded)
				{
					xlExporter.Grids.Add(null, rm.GetString("name.measure_type.pqp.fliker"));
					xlExporter.Grids.Add(null, rm.GetString("name.measure_type.pqp.fliker_long"));
				}
				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgUIF,
									rm.GetString("avg_currents_and_voltages"));
				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmI,
									rm.GetString("avg_harmonics_current"));
				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgPower,
									rm.GetString("avg_powers"));
				xlExporter.Grids.Add(wndDocAVG.wndDocAVGMain.DgHarmPower,
									rm.GetString("avg_harmonic_powers"));
				//?????????????????????? for english version
				xlExporter.Title = "Отчет по ПКЭ и Усредненным значениям " + curArchive_.ObjectName;
				xlExporter.Company = "Mars-Energo Ltd.";

				try
				{
					xlExporter.Open(fd.FileName);
					xlExporter.Export();
				}
				catch (IOException)
				{
					MessageBoxes.ErrorCantAccessFile(this);
					return;
				}
				finally
				{
					xlExporter.Close();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in miReportFSK_Click()");
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
			}
		}

		#endregion

		#region Menu Settings

		private void msServiceSettings_Click(object sender, EventArgs e)
		{
			try
			{
				// creating new window Options
				frmSettings wndSettings = new frmSettings(settings_, this);
				// showing it as dialog window
				wndSettings.ShowDialog(this);
				// loading changed settings
				settings_.LoadSettings();
				SetStatusBarText();

				// syncronization of settings in other windows
				if (wndToolbox != null) wndToolbox.SyncronizeSettings(settings_);
				if (wndDocPQP != null) wndDocPQP.SyncronizeSettings(settings_);
				if (wndDocAVG != null) wndDocAVG.SyncronizeSettings(settings_);
				//if (wndDocAVGGraph != null) wndDocAVGGraph.SyncronizeSettings(settings_);

				msToolsDevName.Visible = settings_.CurDeviceType == EmDeviceType.EM32;
				msToolsObjectNames.Visible =
                    !(settings_.CurDeviceType == EmDeviceType.EM32);
                msToolsNominalValues.Visible = !(settings_.CurDeviceType == EmDeviceType.ETPQP_A ||
                    settings_.CurDeviceType == EmDeviceType.ETPQP);
				msToolsDeclaredU.Visible = (settings_.CurDeviceType == EmDeviceType.ETPQP_A);

				msToolsExportToExcelDNS.Visible = settings_.CurDeviceType != EmDeviceType.ETPQP_A;
				msToolsExportToExcelEvents.Visible = settings_.CurDeviceType == EmDeviceType.ETPQP_A;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in msServiceSettings_Click():");
			}
		}

		private void msServiceLangRussian_Click(object sender, EventArgs e)
		{
			try
			{
				if (msServiceLangRussian.Checked == true) return;
				msServiceLangEnglish.Checked = false;
				msServiceLangRussian.Checked = true;
				settings_.Language = "Русский";
				settings_.SaveSettings();
				MessageBox.Show("Язык изменится только после перезапуска приложения", 
					"К сведению", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Exception in msServiceLangRussian_Click():  " + ex.Message);
			}
		}

		private void msServiceLangEnglish_Click(object sender, EventArgs e)
		{
			try
			{
				if (msServiceLangEnglish.Checked == true) return;
				msServiceLangEnglish.Checked = true;
				msServiceLangRussian.Checked = false;
				settings_.Language = "English";
				settings_.SaveSettings();
				MessageBox.Show("Interface language will be changed only after application restarts", 
					"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(
					"Exception in msServiceLangEnglish_Click():  " + ex.Message);
			}
		}

		#endregion

		#region Menu Help

		private void msHelpAboutEnergomonitoring_Click(object sender, EventArgs e)
		{
			frmAboutBox wndAboutBox = new frmAboutBox();
			wndAboutBox.ShowDialog(this);
		}

		private void msHelpHelp_Click(object sender, EventArgs e)
		{
			try
			{
				string fname = string.Format("\"{0}doc\\EmWorkNet4UM_{1}.pdf\"",
					EmService.AppDirectory, this.settings_.CurrentLanguage);
				System.Diagnostics.Process process = new System.Diagnostics.Process();
				process.StartInfo.FileName = fname;
				process.StartInfo.Arguments = string.Empty;
				process.StartInfo.WorkingDirectory = EmService.AppDirectory;
				process.StartInfo.UseShellExecute = true;
				process.StartInfo.CreateNoWindow = false;
				process.StartInfo.RedirectStandardOutput = false;
				process.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
				EmService.WriteToLogFailed("Error in helpToolStripMenuItem_Click(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#endregion

		#region Private Service Methods

		private void EnabledMenuPQPReport(bool enable, EmDeviceType devType)
		{
			try
			{
				miReportPQP.Enabled = enable;
				miReportGOST.Enabled = enable && (devType != EmDeviceType.ETPQP_A);
				miReportRD.Enabled = enable && (devType != EmDeviceType.ETPQP_A);
				miReportFSK.Enabled = enable && (devType != EmDeviceType.ETPQP_A);
				miReportEtPqpA.Enabled = enable && (devType == EmDeviceType.ETPQP_A);
				miReportEtPqpA_v2.Enabled = enable && (devType == EmDeviceType.ETPQP_A);
				miReportEtPqpA_v3.Enabled = enable && (devType == EmDeviceType.ETPQP_A);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EnabledMenuPQPReport():");
			}
		}

		private bool ConnectToServer(int dataSaverPgServerIndex)
		{
			try
			{
				if (dataSaverPgServerIndex > -1)
				{
					return wndToolbox.ConnectServerAndLoadData(dataSaverPgServerIndex, false);
				}
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ConnectToServer():");
				throw;
			}
		}

		private void DisableConnectInterface()
		{
			try
			{
				tsConnectOptions.Visible = true;
				tsProgressSaving.Visible = false;
				tsAbortSaving.Visible = false;
				tsLabelSavingMode.Visible = false;
				tsLabelCurArchSaving.Visible = false;

				EnableBtnLoad(true);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DisableConnectInterface():");
				throw;
			}
		}

		//private void ConnectSignColorChange(bool connected)
		//{
		//    try
		//    {
		//        if (this.InvokeRequired == false) // thread checking
		//        {
		//            if (connected) panelConnect.BackColor = Color.LimeGreen;
		//            else panelConnect.BackColor = Color.Red;
		//        }
		//        else
		//        {
		//            this.Invoke(ConnectSignColorChange, new Object[] { connected });
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Error in ConnectSignColorChange():");
		//    }
		//}

		private void EnableBtnLoad(bool enable)
		{
			try
			{
				msFileLoadFromDevice.Enabled = enable;
				tsbExchangeWithDevice.Enabled = enable;

				needConfirmClose_ = !enable;

				if (enable) Kernel32.PostMessage(this.Handle, EmService.WM_USER + 3, 0, 0);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in EnableBtnLoad():");
				throw;
			}
		}

		private void saverOnSavingEnds()
		{
			try
			{
				bAutoMode_ = true;
				bCreateImageOnly_ = false;
				DisableConnectInterface();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in saverOnSavingEnds():");
				throw;
			}
		}

		private void bwReader_DoWork(object sender, DoWorkEventArgs e)
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

				switch ((EmDeviceType)e.Argument)
				{
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM33T: readerEm33_.Run(ref e); break;
					case EmDeviceType.EM32: readerEm32_.Run(ref e); break;
					case EmDeviceType.ETPQP: readerEtPQP_.Run(ref e); break;
					case EmDeviceType.ETPQP_A: readerEtPQP_A_.Run(ref e); break;
					case EmDeviceType.NONE:
						throw new EmException("bwReader_DoWork: Unknown device type!");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwReader_DoWork():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//throw;  unhandled exception
			}
		}

		private void bwImCreator_DoWork(object sender, DoWorkEventArgs e)
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

				switch ((EmDeviceType)e.Argument)
				{
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM33T: imCreateEm33_.Run(ref e); break;
					case EmDeviceType.EM32: imCreateEm32_.Run(ref e); break;
					case EmDeviceType.ETPQP: imCreateEtPQP_.Run(ref e); break;
					case EmDeviceType.ETPQP_A: imCreateEtPQP_A_.Run(ref e); break;
					case EmDeviceType.NONE:
						throw new EmException("bwImCreator_DoWork: Unknown device type!");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwImCreator_DoWork():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				//throw;  unhandled exception
			}
		}

		private void bwSaver_DoWork(object sender, DoWorkEventArgs e)
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

				switch ((EmDeviceType)e.Argument)
				{
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM33T: saverEm33_.Run(ref e); break;
					case EmDeviceType.EM32: saverEm32_.Run(ref e); break;
					case EmDeviceType.ETPQP: saverEtPQP_.Run(ref e); break;
					case EmDeviceType.ETPQP_A: saverEtPQP_A_.Run(ref e); break;
					case EmDeviceType.NONE:
						throw new EmException("bwSaver_DoWork: Unkown device type!");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwSaver_DoWork():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				//throw;  unhandled exception
			}
		}

		private void bwExporter_DoWork(object sender, DoWorkEventArgs e)
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

				switch ((EmDeviceType)e.Argument)
				{
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM33T: (exporter_ as EmDataExport33).ExportToImage(ref e); break;
					case EmDeviceType.EM32: (exporter_ as EmDataExport32).ExportToImage(ref e); break;
					case EmDeviceType.ETPQP: (exporter_ as EtDataExportPQP).ExportToImage(ref e); break;
					case EmDeviceType.ETPQP_A: (exporter_ as EtDataExportPQP_A).ExportToImage(ref e); break;
					case EmDeviceType.NONE:
						throw new EmException("bwExport_DoWork: Unkown device type!");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwExport_DoWork():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				//throw;  unhandled exception
			}
		}

		private void SetStatusBarText()
		{
			try
			{
				string strIO = "";
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				string strDev = rm.GetString("current_device_text");
				switch (settings_.CurDeviceType)
				{
					case EmDeviceType.EM31K: strDev += " EM31K,  "; break;
					case EmDeviceType.EM32: strDev += " EM32,  "; break;
					case EmDeviceType.EM33T: strDev += " EM33T,  "; break;
					case EmDeviceType.ETPQP: strDev += " ETPQP,  "; break;
					case EmDeviceType.ETPQP_A: strDev += " ETPQP-A,  "; break;
					case EmDeviceType.NONE: strDev += " NONE,  "; break;
				}

				switch (settings_.IOInterface)
				{
					case EmPortType.Modem:
						strIO = rm.GetString("interface_modem_text");
						using (frmSettings wnd = new frmSettings(settings_, this))
						{
							wnd.OpenDevicesInfo();
							settings_.CurDeviceAddress = wnd.ActiveDevAddressGSM;
							settings_.CurPhoneNumber = wnd.ActiveNumber;
							settings_.AttemptNumber = wnd.Attempts;
							if (settings_.CurPhoneNumber != "")
								strIO += (",  " + settings_.CurPhoneNumber);
							strIO += (",  " + settings_.CurDeviceAddress);
						}
						break;
					case EmPortType.COM:
						strIO = String.Format(rm.GetString("interface_serial_text"),
									settings_.SerialPortName, settings_.SerialPortSpeed);
						break;
					case EmPortType.USB:
						strIO = rm.GetString("interface_USB_text");
						break;
					case EmPortType.Ethernet:
						strIO = rm.GetString("interface_ethernet_text");
						using (frmSettings wnd = new frmSettings(settings_, this))
						{
							wnd.OpenDevicesInfo();
							settings_.CurrentIPAddress = wnd.ActiveIPAddress;
							settings_.CurrentPort = Int32.Parse(wnd.ActivePort);
							if (settings_.CurrentIPAddress != "" && settings_.CurrentIPAddress != "0.0.0.0")
								strIO += (",  " + settings_.CurrentIPAddress);
						}
						break;
					case EmPortType.GPRS:
						strIO = rm.GetString("interface_gprs_text");
						using (frmSettings wnd = new frmSettings(settings_, this))
						{
							wnd.OpenDevicesInfo();
							settings_.CurDeviceAddress = wnd.ActiveDevAddressGPRS;
							settings_.CurrentIPAddress = wnd.ActiveIPAddress;
							settings_.CurrentPort = Int32.Parse(wnd.ActivePort);
							if (settings_.CurrentIPAddress != "" && settings_.CurrentIPAddress != "0.0.0.0")
								strIO += (",  " + settings_.CurrentIPAddress);
							strIO += (",  " + settings_.CurDeviceAddress);
						}
						break;
					case EmPortType.Rs485:
						using (frmSettings wnd = new frmSettings(settings_, this))
						{
							wnd.OpenDevicesInfo();
							settings_.CurDeviceAddress = wnd.ActiveDevAddress485;
						}
						strIO = "RS-485: " + settings_.SerialPortName485 + ", " +
										settings_.CurDeviceAddress.ToString();
						break;
					case EmPortType.WI_FI:
						strIO = string.Format("Wi-Fi: {0}", settings_.CurWifiProfileName);
						break;
				}

				tsConnectOptions.Text = strDev + strIO;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in SetStatusBarText():");
			}
		}

		#endregion

		#region DataSaver Events

		private void reader_OnSetSerialNumber(Int64 serial, string info, EmPortType type)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					frmSettings wnd = new frmSettings(settings_, this);
					wnd.OpenDevicesInfo();
					
					if (type == EmPortType.Rs485)
					{
						ushort devAddress = UInt16.Parse(info);
						wnd.SetSerialNumber(String.Empty /*dummy*/, devAddress,
										serial.ToString(), EmPortType.Rs485);
					}
					else
						wnd.SetSerialNumber(info, 0 /*dummy*/,
										serial.ToString(), type);

					wnd.SaveDevicesInfo();
					wnd.Dispose();
				}
				else
				{
					EmDataSaver.EmDataReader32.SetSerialNumberHandler setSerial =
						new EmDataSaver.EmDataReader32.SetSerialNumberHandler(reader_OnSetSerialNumber);
					this.Invoke(setSerial, new Object[] { serial, info, type });
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in reader_OnSetSerialNumber(): " + ex.Message);
			}
		}

		private void reader_OnStartProgressBar(double percent_for_one_step)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					reader_percent_for_one_step_ = percent_for_one_step;
					tsProgressSaving.Style = ProgressBarStyle.Blocks;
					tsProgressSaving.Value = 0;
					tsProgressSaving.Minimum = 0;
					tsProgressSaving.Maximum = 100;
				}
				else
				{
					EmDataSaver.EmDataReader32.StartProgressBarHandler handler =
						new EmDataSaver.EmDataReader32.StartProgressBarHandler(reader_OnStartProgressBar);
					this.Invoke(handler, new object[] { percent_for_one_step });
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in reader_OnStartProgressBar(): " + ex.Message);
			}
		}

		private void reader_SetCntArchives(int totalArchives, int curArchive)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
						this.GetType().Assembly);
					tsLabelSavingMode.Text = rm.GetString("msg_toolbar_saving_mode_reading");
					tsLabelCurArchSaving.Text = string.Format(
						rm.GetString("msg_toolbar_saving_current_archive"), curArchive, totalArchives);
				}
				else
				{
					EmDataSaver.EmDataReader33.SetCntArchivesHandler handler =
						new EmDataSaver.EmDataReader33.SetCntArchivesHandler(reader_SetCntArchives);
					this.Invoke(handler, new object[] { totalArchives, curArchive });
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in reader_SetCntArchives(): " + ex.Message);
			}
		}

		private void saver_SetCntArchives(int totalArchives, int curArchive)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
					tsLabelSavingMode.Text = rm.GetString("msg.toolbar.saving_mode_db_inserting");
					tsLabelCurArchSaving.Text = string.Format(
						rm.GetString("msg_toolbar_saving_current_archive"), curArchive, totalArchives);
				}
				else
				{
					EmDataSaver.EmDataSaver33.SetCntArchivesHandler handler =
						new EmDataSaver.EmDataSaver33.SetCntArchivesHandler(saver_SetCntArchives);
					this.Invoke(handler, new object[] { totalArchives, curArchive });
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in saver_SetCntArchives(): " + ex.Message);
			}
		}

		// обработчик события, которое возникает, если при считывании ПКЭ длительность 
		// получается больше суток
		private void imCreator_InvalidDurationHandler(TimeSpan timeInv)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					if (frmInvDurMess != null && frmInvDurMess.IsDisposed)
						frmInvDurMess.DeleteInstance();

					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
								this.GetType().Assembly);
					string mess = String.Format(rm.GetString("msg_invalid_duration_text"),
									timeInv);
					frmInvDurMess = frmMessage.Instance(mess, true, 8.25f);
					frmInvDurMess.Show();
				}
				else
				{
					EmDataSaver.EmSqlImageCreator33.InvalidDurationHandler handler =
						new EmDataSaver.EmSqlImageCreator33.InvalidDurationHandler(
							imCreator_InvalidDurationHandler);
					this.Invoke(handler, new object[] { timeInv });
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in imCreator_InvalidDurationHandler(): " + ex.Message);
			}
		}

		#endregion

		#region Connection Methods

		/// <summary>
		/// In this thread we read serial number and archives list from device
		/// </summary>
		private void StartConnectionToDevice()
		{
			try
			{
				if(!bAutoMode_)
					currentDevType_ = settings_.CurDeviceType;
				currentAction_ = EmAction.SaveFromDevice;
				reader_cur_percent_ = 0;
				reader_prev_percent_ = 0;

				ResourceManager rm = null;

				bwReader_ = new BackgroundWorker();
				bwReader_.WorkerReportsProgress = true;
				bwReader_.WorkerSupportsCancellation = true;
				bwReader_.DoWork += bwReader_DoWork;
				bwReader_.ProgressChanged += bwReader_ProgressChanged;
				bwReader_.RunWorkerCompleted += bwReader_RunWorkerCompleted;

				if (settings_.CurServerIndex < 0)
				{
					rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					string msg = rm.GetString("msg_device_no_server_selected");
					string cap = "Error";
					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					// убираем диалог соединения
					saverOnSavingEnds();
					return;
				}

				EnableBtnLoad(false);

				// меняем статусбар на считывание ///////////////
				rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				tsLabelSavingMode.Text = rm.GetString("msg_toolbar_saving_mode_reading");
				tsLabelSavingMode.Visible = true;
				tsProgressSaving.Style = ProgressBarStyle.Marquee;
				tsProgressSaving.Visible = true;
				tsConnectOptions.Visible = false;
				tsAbortSaving.Visible = true;
				/////////////////////////////////////////////////

				// Connecting to the database server
				if (!ConnectToServer(settings_.CurServerIndex))
					throw new EmException(
						"StartConnectionToDevice: Unable to connect to PostgreSQL!");

				if (bAutoMode_)
				{
					xmlImageEm32_ = new Em32XmlDeviceImage();
					readerEm32_ = new EmDataSaver.EmDataReader32(this, settings_,
									settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEm32,
									bwReader_, ref xmlImageEm32_,
									this.autoConnectQueues_, bAutoMode_, this.Handle);
					readerEm32_.OnSetSerialNumber +=
									new EmDataReader32.SetSerialNumberHandler(reader_OnSetSerialNumber);
					readerEm32_.OnSetCntArchives +=
									new EmDataReader32.SetCntArchivesHandler(reader_SetCntArchives);
					readerEm32_.OnStartProgressBar +=
									new EmDataReader32.StartProgressBarHandler(reader_OnStartProgressBar);

					bwReader_.RunWorkerAsync(EmDeviceType.EM32);
				}
				else
				{
					switch (currentDevType_)
					{
						case EmDeviceType.EM32:
							if (settings_.IOInterface != EmPortType.USB)
							{
								xmlImageEm32_ = new Em32XmlDeviceImage();
								readerEm32_ = new EmDataReader32(this, settings_,
									settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEm32,
									bwReader_, ref xmlImageEm32_,
									this.autoConnectQueues_, bAutoMode_, this.Handle);

								readerEm32_.OnSetSerialNumber +=
									new EmDataReader32.SetSerialNumberHandler(reader_OnSetSerialNumber);
								readerEm32_.OnSetCntArchives +=
									new EmDataReader32.SetCntArchivesHandler(reader_SetCntArchives);
								readerEm32_.OnStartProgressBar +=
									new EmDataReader32.StartProgressBarHandler(reader_OnStartProgressBar);
							}
							else
							{
								MessageBoxes.InvalidInterface(this, settings_.IOInterface,
										currentDevType_.ToString());
								throw new EmException("Invalid interface");
							}
							break;
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
						case EmDeviceType.EM31K:
						case EmDeviceType.NONE:
							if (settings_.IOInterface == EmPortType.COM ||
								settings_.IOInterface == EmPortType.USB)
							{
								xmlImageEm33_ = new EmXmlDeviceImage();
								readerEm33_ = new EmDataReader33(this, settings_,
									bwReader_, ref xmlImageEm33_, this.Handle);
								readerEm33_.OnSetCntArchives +=
									new EmDataReader33.SetCntArchivesHandler(reader_SetCntArchives);
								readerEm33_.OnStartProgressBar +=
									new EmDataReader33.StartProgressBarHandler(reader_OnStartProgressBar);
							}
							else
							{
								MessageBoxes.InvalidInterface(this, settings_.IOInterface,
										currentDevType_.ToString());
								throw new EmException("Invalid interface");
							}
							break;
						case EmDeviceType.ETPQP:
							if (settings_.IOInterface == EmPortType.COM)
							{
								xmlImageEtPQP_ = new EtPQPXmlDeviceImage();
								readerEtPQP_ = new EtDataReaderPQP(this, settings_,
									bwReader_, ref xmlImageEtPQP_, this.Handle);
								readerEtPQP_.OnSetCntArchives += reader_SetCntArchives;
								readerEtPQP_.OnStartProgressBar += reader_OnStartProgressBar;
							}
							else
							{
								MessageBoxes.InvalidInterface(this, settings_.IOInterface,
										currentDevType_.ToString());
								throw new EmException("Invalid interface");
							}
							break;
						case EmDeviceType.ETPQP_A:
							if (settings_.IOInterface == EmPortType.USB ||
								settings_.IOInterface == EmPortType.Ethernet ||
								settings_.IOInterface == EmPortType.WI_FI)
							{
								xmlImageEtPQP_A_ = new EtPQP_A_XmlDeviceImage();
								readerEtPQP_A_ = new EtDataReaderPQP_A(this, settings_,
									bwReader_, ref xmlImageEtPQP_A_, this.Handle);
								readerEtPQP_A_.OnSetCntArchives += reader_SetCntArchives;
								readerEtPQP_A_.OnStartProgressBar += reader_OnStartProgressBar;
							}
							else
							{
								MessageBoxes.InvalidInterface(this, settings_.IOInterface,
										currentDevType_.ToString());
								throw new EmException("Invalid interface");
							}
							break;
					}
					bwReader_.RunWorkerAsync(currentDevType_);
				}
			}
			catch (EmException emx)
			{
				saverOnSavingEnds();
				EmService.WriteToLogFailed("Error StartConnectionToDevice(): " + emx.Message);
			}
			catch (Exception ex)
			{
				saverOnSavingEnds();
				EmService.DumpException(ex, "Error StartConnectionToDevice()");
				throw;
			}
		}

		private void StartImageCreating()
		{
			try
			{
				bwImCreator_ = new BackgroundWorker();
				bwImCreator_.WorkerReportsProgress = true;
				bwImCreator_.WorkerSupportsCancellation = true;
				bwImCreator_.DoWork += bwImCreator_DoWork;
				bwImCreator_.ProgressChanged += bwImCreator_ProgressChanged;
				bwImCreator_.RunWorkerCompleted += bwImCreator_RunWorkerCompleted;

				// меняем статусбар на формирование образа //////
				ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				tsLabelSavingMode.Text = rm.GetString("msg.toolbar.saving_mode_creating_img");
				tsProgressSaving.Style = ProgressBarStyle.Blocks;
				tsProgressSaving.Value = 0;
				tsLabelSavingMode.Visible = true;
				tsProgressSaving.Visible = true;
				tsConnectOptions.Visible = false;
				tsAbortSaving.Visible = true;
				/////////////////////////////////////////////////

				if (settings_.CurServerIndex < 0)
				{
					rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					string msg = rm.GetString("msg_device_no_server_selected");
					string cap = "Error";
					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					// убираем диалог соединения
					saverOnSavingEnds();
					return;
				}

				switch (currentDevType_)
				{
					case EmDeviceType.EM32:
						imCreateEm32_ = new EmSqlImageCreator32(this, settings_,
								bwImCreator_, ref xmlImageEm32_, bAutoMode_);
						break;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM31K:
					case EmDeviceType.NONE:
						imCreateEm33_ = new EmSqlImageCreator33(this, settings_,
							bwImCreator_, ref xmlImageEm33_);
						imCreateEm33_.OnInvalidDuration +=
							new EmSqlImageCreator33.InvalidDurationHandler(imCreator_InvalidDurationHandler);
						break;
					case EmDeviceType.ETPQP:
						imCreateEtPQP_ = new EtSqlImageCreatorPQP(this, settings_,
								bwImCreator_, ref xmlImageEtPQP_);
						break;
					case EmDeviceType.ETPQP_A:
						imCreateEtPQP_A_ = new EtSqlImageCreatorPQP_A(this, settings_,
								bwImCreator_, ref xmlImageEtPQP_A_);
						break;
				}
				bwImCreator_.RunWorkerAsync(currentDevType_);
			}
			catch (EmException emx)
			{
				saverOnSavingEnds();
				EmService.WriteToLogFailed("Error StartImageCreating(): " + emx.Message);
			}
			catch (Exception ex)
			{
				saverOnSavingEnds();
				EmService.DumpException(ex, "Error StartImageCreating()");
				throw;
			}
		}

		private void StartInsertToDB()
		{
			try
			{
				EmService.WriteToLogGeneral("StartInsertToDB() start");
				bwSaver_ = new BackgroundWorker();
				bwSaver_.WorkerReportsProgress = true;
				bwSaver_.WorkerSupportsCancellation = true;
				bwSaver_.DoWork += bwSaver_DoWork;
				bwSaver_.ProgressChanged += bwSaver_ProgressChanged;
				bwSaver_.RunWorkerCompleted += bwSaver_RunWorkerCompleted;

				// меняем статусбар на сохранение ///////////////////
				ResourceManager rm = 
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				tsLabelSavingMode.Text = rm.GetString("msg.toolbar.saving_mode_db_inserting");
				// здесь Progressbar'а нет
				tsProgressSaving.Style = ProgressBarStyle.Marquee;
				tsLabelSavingMode.Visible = true;
				tsProgressSaving.Visible = true;
				tsConnectOptions.Visible = false;
				tsAbortSaving.Visible = true;
				//////////////////////////////////////////////////////

				if (settings_.CurServerIndex < 0)
				{
					rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					string msg = rm.GetString("msg_device_no_server_selected");
					string cap = "Error";
					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					// убираем диалог соединения
					saverOnSavingEnds();
					return;
				}

				frmDeviceExchange wndDeviceExchange;
				switch (currentDevType_)
				{
					case EmDeviceType.EM32:
						if (currentAction_ == EmAction.SaveFromFile)
						{
							wndDeviceExchange = new frmDeviceExchange(sqlImageEm32_);
							if (wndDeviceExchange.ShowDialog(this) != DialogResult.OK)
							{
								saverOnSavingEnds(); return;
							}
						}
						saverEm32_ = new EmDataSaver32(this, settings_,
								bwSaver_, ref sqlImageEm32_, bAutoMode_);
						saverEm32_.OnSetCntArchives +=
									new EmDataSaver32.SetCntArchivesHandler(saver_SetCntArchives);
						break;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
					case EmDeviceType.NONE:
						if (currentAction_ == EmAction.SaveFromFile)
						{
							wndDeviceExchange = new frmDeviceExchange(sqlImageEm33_);
							if (wndDeviceExchange.ShowDialog(this) != DialogResult.OK)
							{
								saverOnSavingEnds(); 
								return;
							}
						}

						// если нужна оптимизированная вставка
						if ((currentDevType_ == EmDeviceType.EM31K ||
							currentDevType_ == EmDeviceType.EM33T ||
							currentDevType_ == EmDeviceType.EM33T1) &&
							settings_.OptimisedInsertion && xmlImageEm33_ != null &&
							currentAction_ != EmAction.SaveFromFile)
						{
							saverEm33_ = new EmDataSaver33(this, settings_, bwSaver_,
									null, xmlImageEm33_);
						}
						else
						{
							saverEm33_ = new EmDataSaver33(this, settings_, bwSaver_,
									sqlImageEm33_, null);
						}
						saverEm33_.OnSetCntArchives +=
									new EmDataSaver33.SetCntArchivesHandler(saver_SetCntArchives);
						saverEm33_.OnInvalidDuration +=
							new EmDataSaver33.InvalidDurationHandler(imCreator_InvalidDurationHandler);
						break;
					case EmDeviceType.ETPQP:
						if (currentAction_ == EmAction.SaveFromFile)
						{
							wndDeviceExchange = new frmDeviceExchange(sqlImageEtPQP_);
							if (wndDeviceExchange.ShowDialog(this) != DialogResult.OK)
							{
								saverOnSavingEnds(); return;
							}
						}
						saverEtPQP_ = new EtDataSaverPQP(this, settings_, bwSaver_,
								ref sqlImageEtPQP_);
						saverEtPQP_.OnSetCntArchives +=
									new EtDataSaverPQP.SetCntArchivesHandler(saver_SetCntArchives);
						break;
					case EmDeviceType.ETPQP_A:
						if (currentAction_ == EmAction.SaveFromFile)
						{
							wndDeviceExchange = new frmDeviceExchange(sqlImageEtPQP_A_);
							if (wndDeviceExchange.ShowDialog(this) != DialogResult.OK)
							{
								saverOnSavingEnds(); return;
							}
						}
						saverEtPQP_A_ = new EtDataSaverPQP_A(this, settings_, bwSaver_,
								ref sqlImageEtPQP_A_);
						saverEtPQP_A_.OnSetCntArchives +=
									new EtDataSaverPQP_A.SetCntArchivesHandler(saver_SetCntArchives);
						break;
				}
				bwSaver_.RunWorkerAsync(currentDevType_);
				EmService.WriteToLogGeneral("StartInsertToDB() end");
			}
			catch (EmException emx)
			{
				saverOnSavingEnds();
				EmService.WriteToLogFailed("Error StartInsertToDB(): " + emx.Message);
			}
			catch (Exception ex)
			{
				saverOnSavingEnds();
				EmService.DumpException(ex, "Error StartInsertToDB()");
				throw;
			}
		}

		#endregion

		#region FormMain Events

		private void bwReader_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			int pos = tsProgressSaving.Value + e.ProgressPercentage;
			if (pos <= tsProgressSaving.Maximum)
				tsProgressSaving.Value = pos;
		}

		private void bwReader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				bool res = false;
				if (e.Error != null)
				{
					EmService.WriteToLogFailed(
						"Error in bwReader_RunWorkerCompleted() 1: " + e.Error.Message);
					res = false;
				}
				else if (e.Cancelled)
				{
					saverOnSavingEnds();
					return;
				}
				else if (e.Result != null) res = (bool)e.Result;

				if (!bAutoMode_)
				{
					switch (currentDevType_)
					{
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
						case EmDeviceType.EM31K: bCreateImageOnly_ = readerEm33_.CreateImageOnly; break;
						case EmDeviceType.EM32: bCreateImageOnly_ = readerEm32_.CreateImageOnly; break;
						case EmDeviceType.ETPQP: bCreateImageOnly_ = readerEtPQP_.CreateImageOnly; break;
						case EmDeviceType.ETPQP_A: bCreateImageOnly_ = readerEtPQP_A_.CreateImageOnly; break;
						default: throw new EmException("bwReader_RunWorkerCompleted: Unknown device type! 1");
					}
				}
				else bCreateImageOnly_ = false;

				// для Эм32 - если не считано ни одного архива - устанавливаем флаг ошибки и
				// выдаем сообщение
				if (res && currentDevType_ == EmDeviceType.EM32 && xmlImageEm32_ != null)
				{
					if (xmlImageEm32_.ArchivePQP == null && xmlImageEm32_.ArchiveAVG == null
							&& xmlImageEm32_.ArchiveDNS == null /*&&!device_.DeviceInfo.DnsExists()*/)
					{
						if (!bAutoMode_)
						{
							res = false;

							ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
								this.GetType().Assembly);
							string msg = rm.GetString("msg_device_no_archive_error");
							string cap = rm.GetString("msg_device_data_reading_error_caption");

							MessageBox.Show(this, msg, cap, MessageBoxButtons.OK,
								MessageBoxIcon.Error);
						}
						EmService.WriteToLogFailed(
							"bwReader_RunWorkerCompleted(): no archive was read!");
					}
				}

				// если завершили неуспешно, проверяем была ли считана хотя бы инфа об устройстве,
				// если да, то пытаемся сохранить xml-образ; если нет, то просто выдаем сообщение об ошибке
				if (!res)
				{
					bool bDevInfoWasSaved = true;
					string xmlImageFileName = "";
					switch (currentDevType_)
					{
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
						case EmDeviceType.EM31K:
							if (readerEm33_ != null && readerEm33_.IsDevInfoFull)
							{
								xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.EM33T);
								if (xmlImageEm33_ != null)
									res = saveXmlImage(xmlImageFileName, ref xmlImageEm33_, this);
							}
							else bDevInfoWasSaved = false;
							break;
						case EmDeviceType.EM32:
							if (readerEm32_ != null && readerEm32_.IsDevInfoFull)
							{
								xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.EM32);
								if (xmlImageEm32_ != null)
									res = saveXmlImage(xmlImageFileName, ref xmlImageEm32_, this);
							}
							else bDevInfoWasSaved = false;
							break;
						case EmDeviceType.ETPQP:
							if (readerEtPQP_ != null && readerEtPQP_.IsDevInfoFull)
							{
								xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.ETPQP);
								if (xmlImageEtPQP_ != null) 
									res = saveXmlImage(xmlImageFileName, ref xmlImageEtPQP_, this);
							}
							else bDevInfoWasSaved = false;
							break;
						case EmDeviceType.ETPQP_A:
							if (readerEtPQP_A_ != null && readerEtPQP_A_.IsDevInfoFull)
							{
								xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.ETPQP_A);
								if (xmlImageEtPQP_A_ != null)
									res = saveXmlImage(xmlImageFileName, ref xmlImageEtPQP_A_, this);
							}
							else bDevInfoWasSaved = false;
							break;
						default: throw new EmException("bwReader_RunWorkerCompleted: Unknown device type! 2");
					}

					if (!bAutoMode_ && bDevInfoWasSaved)
					{
						ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
								this.GetType().Assembly);
						string msg = res ? string.Format(
							rm.GetString("msg_sql_img_error_page_img_created"),
							xmlImageFileName) :
							rm.GetString("msg_sql_img_error_page_img_not_created");
						string cap = rm.GetString("msg_sql_img_forming_error_caption");

						MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}

					if (!bAutoMode_ && !bDevInfoWasSaved)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
					}

					saverOnSavingEnds();
					return;
				}
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed(
					"Error in bwReader_RunWorkerCompleted() 2: " + emx.Message);
				saverOnSavingEnds();
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bwReader_RunWorkerCompleted() 3: ");
				saverOnSavingEnds();
				throw;
			}

			// если устройство Эм33Т, то может потребоваться оптимизированная вставка
			if (currentDevType_ == EmDeviceType.EM33T ||
				currentDevType_ == EmDeviceType.EM31K ||
				currentDevType_ == EmDeviceType.EM33T1)
			{
				if (settings_.OptimisedInsertion)
				{
					StartInsertToDB();
					return;
				}
			}

			// если считывание завершилось удачно, запускаем поток создания образа
			StartImageCreating();
		}

		private void bwImCreator_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage <= tsProgressSaving.Maximum)
				tsProgressSaving.Value = e.ProgressPercentage;
		}

		private void bwImCreator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				EmService.WriteToLogGeneral("bwImCreator_RunWorkerCompleted() start");
				bool res = false;
				if (e.Error != null)
				{
					EmService.WriteToLogFailed("Error in bwImCreator_RunWorkerCompleted(): " + e.Error.Message);
					res = false;
				}
				else if (e.Cancelled)
				{
					saverOnSavingEnds();
					return;
				}
				else if (e.Result != null) res = (bool)e.Result;

				switch (currentDevType_)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM31K: sqlImageEm33_ = imCreateEm33_.SqlImage; break;
					case EmDeviceType.EM32: sqlImageEm32_ = imCreateEm32_.SqlImage; break;
					case EmDeviceType.ETPQP: sqlImageEtPQP_ = imCreateEtPQP_.SqlImage; break;
					case EmDeviceType.ETPQP_A: sqlImageEtPQP_A_ = imCreateEtPQP_A_.SqlImage; break;
					default: throw new EmException("bwImCreator_RunWorkerCompleted::Unknown device type!");
				}

				bool debugMode = false;
				switch (currentDevType_)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM31K: if (readerEm33_ != null) debugMode = readerEm33_.DEBUG_MODE_FLAG;
						break;
					case EmDeviceType.EM32: if (readerEm32_ != null) debugMode = readerEm32_.DEBUG_MODE_FLAG;
						break;
					case EmDeviceType.ETPQP: if (readerEtPQP_ != null) debugMode = readerEtPQP_.DEBUG_MODE_FLAG;
						break;
					case EmDeviceType.ETPQP_A: 
						if (readerEtPQP_A_ != null) debugMode = readerEtPQP_A_.DEBUG_MODE_FLAG;
						break;
					default: throw new EmException("bwImCreator_RunWorkerCompleted::Unknown device type!");
				}

				// если завершили неуспешно, то пытаемся сохранить xml-образ и sql-образ
				// если юзер выбрал "сохранить только образ" - тоже сохраняем образы
				if (!res || bCreateImageOnly_ || debugMode)
				{
					EmService.WriteToLogGeneral("bwImCreator_RunWorkerCompleted() !res || bCreateImageOnly_ || debugMode");
					EmService.WriteToLogGeneral(string.Format("{0}, {1}, {2}", res, bCreateImageOnly_, debugMode));
					bool resSql = false;
					bool resXml = false;
					string xmlImageFileName, sqlImageFileName;

					switch (currentDevType_)
					{
						case EmDeviceType.EM33T:
						case EmDeviceType.EM31K:
						case EmDeviceType.EM33T1:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.EM33T);
							if (readerEm33_ != null) sqlImageFileName = readerEm33_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							resXml = saveXmlImage(xmlImageFileName, ref xmlImageEm33_, this);
							if (imCreateEm33_.SqlImage != null)
								resSql = saveSqlImage(sqlImageFileName, imCreateEm33_.SqlImage, this);
							break;
						case EmDeviceType.EM32:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.EM32);
							if (readerEm32_ != null) sqlImageFileName = readerEm32_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							resXml = saveXmlImage(xmlImageFileName, ref xmlImageEm32_, this);
							if (imCreateEm32_.SqlImage != null)
								resSql = saveSqlImage(sqlImageFileName, imCreateEm32_.SqlImage, this);
							break;
						case EmDeviceType.ETPQP:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.ETPQP);
							if (readerEtPQP_ != null) sqlImageFileName = readerEtPQP_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							resXml = saveXmlImage(xmlImageFileName, ref xmlImageEtPQP_, this);
							if (imCreateEtPQP_.SqlImage != null)
								resSql = saveSqlImage(sqlImageFileName, imCreateEtPQP_.SqlImage, this);
							break;
						case EmDeviceType.ETPQP_A:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.ETPQP_A);
							if (readerEtPQP_A_ != null) sqlImageFileName = readerEtPQP_A_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							resXml = saveXmlImage(xmlImageFileName, ref xmlImageEtPQP_A_, this);
							if (imCreateEtPQP_A_.SqlImage != null)
								resSql = saveSqlImage(sqlImageFileName, imCreateEtPQP_A_.SqlImage, this);
							break;
						default: throw new EmException("bwImCreator_RunWorkerCompleted::Unknown device type!");
					}

					if (!res)
					{
						if (!bAutoMode_)
						{
							ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
								this.GetType().Assembly);
							string msg = res ? string.Format(
								rm.GetString("msg_sql_img_error_page_img_created"),
								xmlImageFileName) :
								rm.GetString("msg_sql_img_error_page_img_not_created");
							string cap = rm.GetString("msg_sql_img_forming_error_caption");

							MessageBox.Show(this, msg, cap, MessageBoxButtons.OK,
									MessageBoxIcon.Error);
						}
						// выходим из процессов считывания/сохранения
						saverOnSavingEnds();
						return;
					}
					else if (bCreateImageOnly_)
					{
						if (!resSql)
						{
							ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
								this.GetType().Assembly);
							string msg = res ? string.Format(
								rm.GetString("msg_sql_img_error_page_img_created"),
								xmlImageFileName) :
								rm.GetString("msg_sql_img_error_page_img_not_created");
							string cap = rm.GetString("msg_sql_img_writing_error_caption");

							MessageBox.Show(this, msg, cap, MessageBoxButtons.OK,
								MessageBoxIcon.Error);
						}
						// выходим из процессов считывания/сохранения
						saverOnSavingEnds();
						return;
					}
				}
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed(
					"Error in bwImCreator_RunWorkerCompleted(): " + emx.Message);
				saverOnSavingEnds();
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bwImCreator_RunWorkerCompleted(): ");
				saverOnSavingEnds();
				throw;
			}
			finally
			{
				// xml-образы уже не нужны, поэтому обнуляем ссылки, чтобы их собрал сборщик мусора
				xmlImageEm33_ = null;
				xmlImageEm32_ = null;
				xmlImageEtPQP_ = null;
				xmlImageEtPQP_A_ = null;
				Environment.CurrentDirectory = EmService.AppDirectory;
				//GC.Collect();
			}

			// если считывание завершилось удачно, запускаем поток сохранения в БД
			EmService.WriteToLogGeneral("bwImCreator_RunWorkerCompleted() end");
			StartInsertToDB();
		}

		private void bwSaver_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage <= tsProgressSaving.Maximum)
				tsProgressSaving.Value = e.ProgressPercentage;
		}

		private void bwSaver_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				wndToolbox.ConnectServerAndLoadData(settings_.CurServerIndex, true);

				bool res = false;
				if (e.Error != null)
				{
					EmService.WriteToLogFailed(
						"Error in bwSaver_RunWorkerCompleted(): " + e.Error.Message);
					res = false;
				}
				else if (e.Cancelled)
				{
					saverOnSavingEnds();
					return;
				}
				else if (e.Result != null) res = (bool)e.Result;

				// если завершили неуспешно, то пытаемся сохранить xml-образ и sql-образ
				if (!res)
				{
					bool resSql = false;
					bool resXml = false;
					string xmlImageFileName, sqlImageFileName;

					switch (currentDevType_)
					{
						case EmDeviceType.EM33T:
						case EmDeviceType.EM31K:
						case EmDeviceType.EM33T1:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.EM33T);
							if (readerEm33_ != null) sqlImageFileName = readerEm33_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							if (xmlImageEm33_ != null)
								resXml = saveXmlImage(xmlImageFileName, ref xmlImageEm33_, this);
							resSql = saveSqlImage(sqlImageFileName, saverEm33_.SqlImage, this);
							break;
						case EmDeviceType.EM32:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.EM32);
							if (readerEm32_ != null) sqlImageFileName = readerEm32_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							if (xmlImageEm32_ != null)
								resXml = saveXmlImage(xmlImageFileName, ref xmlImageEm32_, this);
							resSql = saveSqlImage(sqlImageFileName, saverEm32_.SqlImage, this);
							break;
						case EmDeviceType.ETPQP:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.ETPQP);
							if (readerEtPQP_ != null) sqlImageFileName = readerEtPQP_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							if (xmlImageEtPQP_ != null)
								resXml = saveXmlImage(xmlImageFileName, ref xmlImageEtPQP_, this);
							resSql = saveSqlImage(sqlImageFileName, saverEtPQP_.SqlImage, this);
							break;
						case EmDeviceType.ETPQP_A:
							xmlImageFileName = EmService.GetXmlImageFilePathAndName(EmDeviceType.ETPQP_A);
							if (readerEtPQP_A_ != null) sqlImageFileName = readerEtPQP_A_.SqlImageFileName;
							else sqlImageFileName = EmService.GetSqlImageFilePathAndName(currentDevType_);
							if (xmlImageEtPQP_A_ != null)
								resXml = saveXmlImage(xmlImageFileName, ref xmlImageEtPQP_A_, this);
							resSql = saveSqlImage(sqlImageFileName, saverEtPQP_A_.SqlImage, this);
							break;
						default: throw new EmException("Unknown device type!");
					}

					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
					string cap = rm.GetString("msg_db_insert_error_caption");
					string msg = string.Empty;

					if (!resSql && !resXml)
						msg = rm.GetString("db_error_nopage_nosql_text");
					else if (!resSql && resXml)
						msg = string.Format(rm.GetString("msg_device_db_error_page_nosql"),
							xmlImageFileName);
					else if (resSql && !resXml)
						msg = string.Format(rm.GetString("db_error_nopage_sql_text"),
							sqlImageFileName);
					else if (resSql && resXml)
						msg = string.Format(rm.GetString("msg_device_db_error_page_sql"),
							sqlImageFileName, xmlImageFileName);

					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed(
					"Error in bwSaver_RunWorkerCompleted(): " + emx.Message);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bwSaver_RunWorkerCompleted():");
				throw;
			}
			finally
			{
				saverOnSavingEnds();

				switch (currentDevType_)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
						saverEm33_.SqlImage = null;
						break;
					case EmDeviceType.EM32:
						saverEm32_.SqlImage = null;
						break;
					case EmDeviceType.ETPQP:
						saverEtPQP_.SqlImage = null;
						break;
					case EmDeviceType.ETPQP_A:
						saverEtPQP_A_.SqlImage = null;
						break;
					default: throw new EmException("Unknown device type!");
				}
				//GC.Collect();
			}
		}

		private void bwExporter_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage <= tsProgressSaving.Maximum)
				tsProgressSaving.Value = e.ProgressPercentage;
		}

		private void bwExporter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				bool res = false;
				if (e.Error != null)
				{
					EmService.WriteToLogFailed(
						"Error in bwExporter_RunWorkerCompleted(): " + e.Error.Message);
					res = false;
				}
				else if (e.Cancelled)
				{
					saverOnSavingEnds();
					return;
				}
				else if (e.Result != null) res = (bool)e.Result;

				// если завершили неуспешно, то пишем в лог, выдаем сообщение и выходим
				if (!res && !e.Cancelled)
				{
					EmService.WriteToLogFailed("Unable to create sql-image to export!");
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
					string cap = rm.GetString("msg_image_export_error_caption");
					string msg = rm.GetString("msg_image_export_error_text");
					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				switch (currentDevType_)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
						res = saveSqlImage(exporter_.SqlImageFileName,
							(exporter_ as EmDataExport33).SqlImage, this); break;
					case EmDeviceType.EM32:
						res = saveSqlImage(exporter_.SqlImageFileName,
							(exporter_ as EmDataExport32).SqlImage, this); break;
					case EmDeviceType.ETPQP:
						res = saveSqlImage(exporter_.SqlImageFileName,
							(exporter_ as EtDataExportPQP).SqlImage, this); break;
					case EmDeviceType.ETPQP_A:
						res = saveSqlImage(exporter_.SqlImageFileName,
							(exporter_ as EtDataExportPQP_A).SqlImage, this); break;
					default: throw new EmException("bwExporter_RunWorkerCompleted: Unknown device type!");
				}

				if (!res)
				{
					EmService.WriteToLogFailed("Unable to save exported sql-image!");
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
						this.GetType().Assembly);
					string cap = rm.GetString("msg_image_export_error_caption");
					string msg = rm.GetString("msg_image_export_error_text");
					MessageBox.Show(this, msg, cap, MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed(
					"Error in bwExporter_RunWorkerCompleted(): " + emx.Message);
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bwExporter_RunWorkerCompleted(): ");
				throw;
			}
			finally
			{
				saverOnSavingEnds();
			}
		}

		void frmMain_OnTimerDial(string phone, int attempts)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					settings_.AutoSettings.AutoPhoneNumber = phone;
					settings_.AttemptNumber = attempts;
					settings_.AutoSettings.AutoIOInterface = EmPortType.Modem;
					currentDevType_ = EmDeviceType.EM32;

					msFileLoadFromDevice.Enabled = false;

					StartConnectionToDevice();

					return;
				}
				else
				{
					AutoDialThread.TimerDialHandler timerDial =
						new AutoDialThread.TimerDialHandler(frmMain_OnTimerDial);
					this.Invoke(timerDial, new object[] { phone, attempts });
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmMain_OnTimerDial():");
			}
		}

		void frmMain_OnTimerEthernet(string address, string port)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					settings_.AutoSettings.AutoIPAddress = address;
					settings_.AutoSettings.AutoPort = Int32.Parse(port);
					settings_.AutoSettings.AutoIOInterface = EmPortType.Ethernet;
					currentDevType_ = EmDeviceType.EM32;

					msFileLoadFromDevice.Enabled = false;

					StartConnectionToDevice();

					return;
				}
				else
				{
					AutoDialThread.TimerEthernetHandler timerDial =
						new AutoDialThread.TimerEthernetHandler(frmMain_OnTimerEthernet);
					this.Invoke(timerDial, new object[] { address, port });
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmMain_OnTimerEthernet(): ");
			}
		}

		void frmMain_OnTimerGPRS(string address, string port)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					settings_.AutoSettings.AutoIPAddress = address;
					settings_.AutoSettings.AutoPort = Int32.Parse(port);
					settings_.AutoSettings.AutoIOInterface = EmPortType.GPRS;
					currentDevType_ = EmDeviceType.EM32;

					msFileLoadFromDevice.Enabled = false;

					StartConnectionToDevice();

					return;
				}
				else
				{
					AutoDialThread.TimerGPRSHandler timerDial =
						new AutoDialThread.TimerGPRSHandler(frmMain_OnTimerGPRS);
					this.Invoke(timerDial, new object[] { address, port });
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmMain_OnTimerGPRS(): ");
			}
		}

		void frmMain_OnTimerRs485(ushort devAddress)
		{
			try
			{
				if (this.InvokeRequired == false) // thread checking
				{
					settings_.AutoSettings.AutoDeviceAddress = devAddress;
					settings_.AutoSettings.AutoIOInterface = EmPortType.Rs485;
					currentDevType_ = EmDeviceType.EM32;

					msFileLoadFromDevice.Enabled = false;

					StartConnectionToDevice();

					return;
				}
				else
				{
					AutoDialThread.TimerRs485Handler timerRs485 =
						new AutoDialThread.TimerRs485Handler(frmMain_OnTimerRs485);
					this.Invoke(timerRs485, new object[] { devAddress });
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmMain_OnTimerRs485(): ");
			}
		}

		#endregion

		#region Properties

		internal ArchiveInfo CurrentArchive
		{
			get { return curArchive_; }
			//set { curArchive_ = value; }
		}

		public bool AutoMode
        {
            get { return bAutoMode_; }
            //set { bAutoMode_ = value; }
        }

		public AutoConnect AutoConnectQueues
        {
            get { return autoConnectQueues_; }
		}

		#endregion

		#region Save / Load Xml / Sql image

		private static bool loadXmlImage(string fileName, ref EmXmlDeviceImage image)
		{
			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EmXmlDeviceImage));
				FileInfo fi = new FileInfo(fileName);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the EmXmlDeviceImage by
					// deserializing the config file.
					image = (EmXmlDeviceImage)mySerializer.Deserialize(myFileStream);
				}
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie, "InvalidOperationException in loadXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in loadXmlImage()");
				throw;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
			return true;
		}

		private static bool loadXmlImage(string fileName, ref Em32XmlDeviceImage image)
		{
			if (image == null) return false;

			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(Em32XmlDeviceImage));
				FileInfo fi = new FileInfo(fileName);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the EmXmlDeviceImage by
					// deserializing the config file.
					image = (Em32XmlDeviceImage)mySerializer.Deserialize(myFileStream);
				}
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie, "InvalidOperationException in loadXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in loadXmlImage()");
				throw;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
			return true;
		}

		public static bool loadXmlImage(string fileName, ref EtPQPXmlDeviceImage image)
		{
			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EtPQPXmlDeviceImage));
				FileInfo fi = new FileInfo(fileName);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the EmXmlDeviceImage by
					// deserializing the config file.
					image = (EtPQPXmlDeviceImage)mySerializer.Deserialize(myFileStream);
				}
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie, "InvalidOperationException in loadXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in loadXmlImage()");
				throw;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
			return true;
		}

		public static bool loadXmlImage(string fileName, ref EtPQP_A_XmlDeviceImage image)
		{
			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EtPQP_A_XmlDeviceImage));
				FileInfo fi = new FileInfo(fileName);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the EmXmlDeviceImage by
					// deserializing the config file.
					image = (EtPQP_A_XmlDeviceImage)mySerializer.Deserialize(myFileStream);
				}
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie, "InvalidOperationException in loadXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in loadXmlImage()");
				throw;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
			return true;
		}

		private static bool saveXmlImage(string fileName, ref EmXmlDeviceImage image, object sender)
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{

				if (image == null) return false;

				System.IO.FileInfo fi = new FileInfo(fileName);
				if (!fi.Directory.Exists)
				{
					try
					{
						Directory.CreateDirectory(fi.Directory.FullName);
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed(
							"Error in EtDataSaverPQP::saveXmlImage(): " + ex.Message);
						MessageBoxes.ErrorCreateDir(sender);
						return false;
					}
				}

				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EmXmlDeviceImage));
				myWriter = new StreamWriter(fileName, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, image);
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie,
					"InvalidOperationException in saveXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in saveXmlImage()");
				throw;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}

			return true;
		}

		private static bool saveXmlImage(string fileName, ref Em32XmlDeviceImage image, object sender)
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				if (image == null) return false;

				System.IO.FileInfo fi = new FileInfo(fileName);
				if (!fi.Directory.Exists)
				{
					try
					{
						Directory.CreateDirectory(fi.Directory.FullName);
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed(
							"Error in EtDataSaverPQP::saveXmlImage(): " + ex.Message);
						MessageBoxes.ErrorCreateDir(sender);
						return false;
					}
				}

				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(Em32XmlDeviceImage));
				myWriter = new StreamWriter(fileName, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, image);
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie,
					"InvalidOperationException in saveXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in saveXmlImage()");
				throw;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}
			return true;
		}

		private static bool saveXmlImage(string fileName, ref EtPQPXmlDeviceImage image, object sender)
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				if (image == null) return false;

				System.IO.FileInfo fi = new FileInfo(fileName);
				if (!fi.Directory.Exists)
				{
					try
					{
						Directory.CreateDirectory(fi.Directory.FullName);
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed(
							"Error in EtDataSaverPQP::saveXmlImage(): " + ex.Message);
						MessageBoxes.ErrorCreateDir(sender);
						return false;
					}
				}

				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EtPQPXmlDeviceImage));
				myWriter = new StreamWriter(fileName, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, image);
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie,
					"InvalidOperationException in saveXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in saveXmlImage()");
				throw;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}
			return true;
		}

		private static bool saveXmlImage(string fileName, ref EtPQP_A_XmlDeviceImage image, object sender)
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				if (image == null) return false;

				System.IO.FileInfo fi = new FileInfo(fileName);
				if (!fi.Directory.Exists)
				{
					try
					{
						Directory.CreateDirectory(fi.Directory.FullName);
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed(
							"Error in EtDataSaverPQP::saveXmlImage(): " + ex.Message);
						MessageBoxes.ErrorCreateDir(sender);
						return false;
					}
				}

				// Create an XmlSerializer for the EmXmlDeviceImage type.
                mySerializer = new XmlSerializer(typeof(EtPQP_A_XmlDeviceImage));
				myWriter = new StreamWriter(fileName, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, image);
			}
			catch (InvalidOperationException ie)
			{
				EmService.DumpException(ie,
					"InvalidOperationException in saveXmlImage()");
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in saveXmlImage()");
				throw;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}
			return true;
		}

		/// <summary>Save device SQL image in the XML format</summary>
		private static bool saveSqlImage(string fileName, EtPQPSqlDeviceImage image, object sender)
		{
			FileStream fsMain = null;
			try
			{
				// создаем список дополнительных файлов для усредненных: имя + длина
				List<Pair<string, Int64>> dicAvgFiles = new List<Pair<string, Int64>>();
				for (int iObj = 0; iObj < image.Objects.Length; ++iObj)
				{
					for (int iAvg = 0; iAvg < image.Objects[iObj].DataAVG.Length; ++iAvg)
					{
						if(image.Objects[iObj].DataAVG != null &&
							image.Objects[iObj].DataAVG.Length > 0)
						{
							string avgFullPath = EmService.TEMP_IMAGE_DIR +
								image.Objects[iObj].DataAVG[iAvg].AvgFileName;
							dicAvgFiles.Add(new Pair<string, Int64>(
								image.Objects[iObj].DataAVG[iAvg].AvgFileName,
								new FileInfo(avgFullPath).Length)
								);
						}
					}
				}

				// создаем образ во временный файл
				string fileNameImageTmp = EmService.TEMP_IMAGE_DIR + "EtPQPSqlDeviceImage.tmp";
				CheckIfDirectoryExists(fileNameImageTmp, sender);
				StreamWriter swTmp = null;
				try
				{
					swTmp = new StreamWriter(fileNameImageTmp);
					// Create an XmlSerializer for the EmXmlDeviceImage type.
					XmlSerializer xmlSer = new XmlSerializer(typeof(EtPQPSqlDeviceImage));
					// Serialize this instance of the ApplicationSettings class to the config file.
					xmlSer.Serialize(swTmp, image);
				}
				finally { if (swTmp != null) swTmp.Close(); }
				Int64 lenFileImageTmp = new FileInfo(fileNameImageTmp).Length;

				////////////////////////////////////////////////////////////////
				// формируем главный файл (который содержит все вспомогательные)
				CheckIfDirectoryExists(fileName, sender);
				if (File.Exists(fileName)) File.Delete(fileName);
				StreamWriter swMain = null;
				try
				{
					swMain = new StreamWriter(fileName, true);
					// создаем заголовок файла:
					// кол-во строк заголовка
					swMain.WriteLine((dicAvgFiles.Count * 2 + 1).ToString());
					// размер файла образа
					swMain.WriteLine(lenFileImageTmp);
					for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
					{
						// имя файла avg
						swMain.WriteLine(dicAvgFiles[iDic].First);
						// размер файла avg
						swMain.WriteLine(dicAvgFiles[iDic].Second);
					}
				}
				finally
				{
					swMain.Close();
				}
				// после создания заголовка переписываем образ в главный файл
				FileStream fsTmp = null;
				try
				{
					fsTmp = new FileStream(fileNameImageTmp, FileMode.Open);
					fsMain = new FileStream(fileName, FileMode.Append);
					byte[] bufImageTmp = new byte[lenFileImageTmp];
					int dwRead = fsTmp.Read(bufImageTmp, 0, bufImageTmp.Length);
					if (dwRead != lenFileImageTmp)
						EmService.WriteToLogFailed(
							String.Format("saveSqlImage(): dwRead = {0}, but lenFileImageTmp = {1}",
							dwRead, lenFileImageTmp));
					fsMain.Write(bufImageTmp, 0, bufImageTmp.Length);
				}
				finally 
				{
					if (fsTmp != null) fsTmp.Close();
					File.Delete(fileNameImageTmp);
				}
				// записываем дополнительные файлы для усредненных
				FileStream fsAvgTmp = null;
				try
				{
					for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
					{
						try
						{
							fsAvgTmp = new FileStream(
								EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First,
								FileMode.Open);
							byte[] bufAvgTmp = new byte[dicAvgFiles[iDic].Second];
							int dwRead = fsAvgTmp.Read(bufAvgTmp, 0, bufAvgTmp.Length);
							if (dwRead != dicAvgFiles[iDic].Second)
								EmService.WriteToLogFailed(
									String.Format("saveSqlImage(): dwRead = {0}, but lenAvgFileTmp = {1}",
									dwRead, dicAvgFiles[iDic].Second));
							fsMain.Write(bufAvgTmp, 0, bufAvgTmp.Length);
							Thread.Sleep(3000);
						}
						finally
						{
							if (fsAvgTmp != null) fsAvgTmp.Close();
							File.Delete(EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First);
						}
					}
				}
				finally
				{
					if (fsAvgTmp != null) fsAvgTmp.Close();
				}
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in saveSqlImage(): 1");
				return false;
			}
			finally
			{
				if (fsMain != null) fsMain.Close();
			}
			return true;
		}

		/// <summary>Save device SQL image in the XML format</summary>
		private static bool saveSqlImage(string fileName, EtPQP_A_SqlDeviceImage image, object sender)
		{
			FileStream fsMain = null;
			try
			{
				// создаем список дополнительных файлов для усредненных: имя + длина
				List<Pair<string, Int64>> dicAvgFiles = new List<Pair<string, Int64>>();
				for (int iReg = 0; iReg < image.Registrations.Length; ++iReg)
				{
					for (int iAvg = 0; iAvg < image.Registrations[iReg].DataAVG.Length; ++iAvg)
					{
						if (image.Registrations[iReg].DataAVG != null &&
							image.Registrations[iReg].DataAVG.Length > 0)
						{
							string avgFullPath = EmService.TEMP_IMAGE_DIR +
								image.Registrations[iReg].DataAVG[iAvg].AvgFileName;
							dicAvgFiles.Add(new Pair<string, Int64>(
								image.Registrations[iReg].DataAVG[iAvg].AvgFileName,
								new FileInfo(avgFullPath).Length)
								);
						}
					}
				}

				// создаем образ во временный файл
				string fileNameImageTmp = EmService.TEMP_IMAGE_DIR + "EtPQP_A_SqlDeviceImage.tmp";
				CheckIfDirectoryExists(fileNameImageTmp, sender);
				StreamWriter swTmp = null;
				try
				{
					swTmp = new StreamWriter(fileNameImageTmp);
					// Create an XmlSerializer for the EmXmlDeviceImage type.
					XmlSerializer xmlSer = new XmlSerializer(typeof(EtPQP_A_SqlDeviceImage));
					// Serialize this instance of the ApplicationSettings class to the config file.
					xmlSer.Serialize(swTmp, image);
				}
                catch (Exception e)
                {
                    EmService.DumpException(e, "Error in saveSqlImage():");
                    throw e;
                }
				finally { if (swTmp != null) swTmp.Close(); }
				Int64 lenFileImageTmp = new FileInfo(fileNameImageTmp).Length;

				////////////////////////////////////////////////////////////////
				// формируем главный файл (который содержит все вспомогательные)
				CheckIfDirectoryExists(fileName, sender);
				if (File.Exists(fileName)) File.Delete(fileName);
				StreamWriter swMain = null;
				try
				{
					swMain = new StreamWriter(fileName, true);
					// создаем заголовок файла:
					// кол-во строк заголовка
					swMain.WriteLine((dicAvgFiles.Count * 2 + 1).ToString());
					// размер файла образа
					swMain.WriteLine(lenFileImageTmp);
					for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
					{
						// имя файла avg
						swMain.WriteLine(dicAvgFiles[iDic].First);
						// размер файла avg
						swMain.WriteLine(dicAvgFiles[iDic].Second);
					}
				}
				finally
				{
					swMain.Close();
				}
				// после создания заголовка переписываем образ в главный файл
				FileStream fsTmp = null;
				try
				{
					fsTmp = new FileStream(fileNameImageTmp, FileMode.Open);
					fsMain = new FileStream(fileName, FileMode.Append);
					byte[] bufImageTmp = new byte[lenFileImageTmp];
					int dwRead = fsTmp.Read(bufImageTmp, 0, bufImageTmp.Length);
					if (dwRead != lenFileImageTmp)
						EmService.WriteToLogFailed(
							String.Format("saveSqlImage(): dwRead = {0}, but lenFileImageTmp = {1}",
							dwRead, lenFileImageTmp));
					fsMain.Write(bufImageTmp, 0, bufImageTmp.Length);
				}
				finally
				{
					if (fsTmp != null) fsTmp.Close();
					File.Delete(fileNameImageTmp);
				}
				// записываем дополнительные файлы для усредненных
				FileStream fsAvgTmp = null;
				try
				{
					for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
					{
						try
						{
							fsAvgTmp = new FileStream(
								EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First,
								FileMode.Open);
							byte[] bufAvgTmp = new byte[dicAvgFiles[iDic].Second];
							int dwRead = fsAvgTmp.Read(bufAvgTmp, 0, bufAvgTmp.Length);
							if (dwRead != dicAvgFiles[iDic].Second)
								EmService.WriteToLogFailed(
									String.Format("saveSqlImage(): dwRead = {0}, but lenAvgFileTmp = {1}",
									dwRead, dicAvgFiles[iDic].Second));
							fsMain.Write(bufAvgTmp, 0, bufAvgTmp.Length);
							Thread.Sleep(3000);
						}
						finally
						{
							if (fsAvgTmp != null) fsAvgTmp.Close();
							File.Delete(EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First);
						}
					}
				}
				finally
				{
					if (fsAvgTmp != null) fsAvgTmp.Close();
				}
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in saveSqlImage(): 1");
				return false;
			}
			finally
			{
				if (fsMain != null) fsMain.Close();
			}
			return true;
		}

		/// <summary>Save device SQL image in the XML format</summary>
		private static bool saveSqlImage(string fileName, EmSqlDeviceImage image, object sender)
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				System.IO.FileInfo fi = new FileInfo(fileName);
				if (!fi.Directory.Exists)
				{
					try
					{
						Directory.CreateDirectory(fi.Directory.FullName);
					}
					catch (Exception ex)
					{
						MessageBoxes.ErrorCreateDir(sender);
						EmService.WriteToLogFailed("Error in saveSqlImage() 1: " + ex.Message);
						return false;
					}
				}

				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EmSqlDeviceImage));
				myWriter = new StreamWriter(fileName, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file
				mySerializer.Serialize(myWriter, image);
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in saveSqlImage() 2: " + e.Message);
				return false;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}
			return true;
		}

		private static bool saveSqlImage(string fileName, EmSqlEm32Device image, object sender)
		{
			FileStream fsMain = null;
			try
			{
				// создаем список дополнительных файлов для усредненных: имя + длина
				List<Pair<string, Int64>> dicAvgFiles = new List<Pair<string, Int64>>();
				for (int iAvg = 0; iAvg < image.DataAVG.Length; ++iAvg)
				{
					if (image.DataAVG != null && image.DataAVG.Length > 0)
					{
						string avgFullPath = EmService.TEMP_IMAGE_DIR +
							image.DataAVG[iAvg].AvgFileName;
						dicAvgFiles.Add(new Pair<string, Int64>(
							image.DataAVG[iAvg].AvgFileName,
							new FileInfo(avgFullPath).Length)
							);
					}
				}

				// создаем образ во временный файл
				string fileNameImageTmp = EmService.TEMP_IMAGE_DIR + "Em32SqlDeviceImage.tmp";
				CheckIfDirectoryExists(fileNameImageTmp, sender);
				StreamWriter swTmp = null;
				try
				{
					swTmp = new StreamWriter(fileNameImageTmp);
					// Create an XmlSerializer for the EmXmlDeviceImage type.
					XmlSerializer xmlSer = new XmlSerializer(typeof(EmSqlEm32Device));
					// Serialize this instance of the ApplicationSettings class to the config file.
					xmlSer.Serialize(swTmp, image);
				}
				finally { if (swTmp != null) swTmp.Close(); }
				Int64 lenFileImageTmp = new FileInfo(fileNameImageTmp).Length;

				////////////////////////////////////////////////////////////////
				// формируем главный файл (который содержит все вспомогательные)
				CheckIfDirectoryExists(fileName, sender);
				if (File.Exists(fileName)) File.Delete(fileName);
				StreamWriter swMain = null;
				try
				{
					swMain = new StreamWriter(fileName, true);
					// создаем заголовок файла:
					// кол-во строк заголовка
					swMain.WriteLine((dicAvgFiles.Count * 2 + 1).ToString());
					// размер файла образа
					swMain.WriteLine(lenFileImageTmp);
					for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
					{
						// имя файла avg
						swMain.WriteLine(dicAvgFiles[iDic].First);
						// размер файла avg
						swMain.WriteLine(dicAvgFiles[iDic].Second);
					}
				}
				finally
				{
					swMain.Close();
				}
				// после создания заголовка переписываем образ в главный файл
				FileStream fsTmp = null;
				try
				{
					fsTmp = new FileStream(fileNameImageTmp, FileMode.Open);
					fsMain = new FileStream(fileName, FileMode.Append);
					byte[] bufImageTmp = new byte[lenFileImageTmp];
					int dwRead = fsTmp.Read(bufImageTmp, 0, bufImageTmp.Length);
					if (dwRead != lenFileImageTmp)
						EmService.WriteToLogFailed(
							String.Format("saveSqlImage(): dwRead = {0}, but lenFileImageTmp = {1}",
							dwRead, lenFileImageTmp));
					fsMain.Write(bufImageTmp, 0, bufImageTmp.Length);
				}
				finally
				{
					if (fsTmp != null) fsTmp.Close();
					File.Delete(fileNameImageTmp);
				}
				// записываем дополнительные файлы для усредненных
				FileStream fsAvgTmp = null;
				try
				{
					for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
					{
						try
						{
							fsAvgTmp = new FileStream(
								EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First,
								FileMode.Open);
							byte[] bufAvgTmp = new byte[dicAvgFiles[iDic].Second];
							int dwRead = fsAvgTmp.Read(bufAvgTmp, 0, bufAvgTmp.Length);
							if (dwRead != dicAvgFiles[iDic].Second)
								EmService.WriteToLogFailed(
									String.Format("saveSqlImage(): dwRead = {0}, but lenAvgFileTmp = {1}",
									dwRead, dicAvgFiles[iDic].Second));
							fsMain.Write(bufAvgTmp, 0, bufAvgTmp.Length);
							Thread.Sleep(3000);
						}
						finally
						{
							if (fsAvgTmp != null) fsAvgTmp.Close();
							File.Delete(EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First);
						}
					}
				}
				finally
				{
					if (fsAvgTmp != null) fsAvgTmp.Close();
				}
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in saveSqlImage(): 1");
				return false;
			}
			finally
			{
				if (fsMain != null) fsMain.Close();
			}
			return true;
		}

		private bool loadSqlImage(string fileName, ref EmSqlDeviceImage image)
		{
			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the EmXmlDeviceImage type.
				mySerializer = new XmlSerializer(typeof(EmSqlDeviceImage));
				FileInfo fi = new FileInfo(fileName);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the EmXmlDeviceImage by
					// deserializing the config file.
					image = (EmSqlDeviceImage)mySerializer.Deserialize(myFileStream);
				}
			}
			catch (Exception ex)
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
					this.GetType().Assembly);
				string msg = string.Format(
					rm.GetString("msg_image_bad_sql_image_file_text"), fileName);
				string cap = rm.GetString("msg_image_bad_sql_image_file_caption");

				MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
				EmService.DumpException(ex, "Error in loadSqlImage(): ");
				return false;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
			return true;
		}

		private bool loadSqlImage(string fileName, ref EmSqlEm32Device image)
		{
			return loadSqlImageCommon<EmSqlEm32Device>(fileName, ref image);
		}

		private bool loadSqlImage(string fileName, ref EtPQPSqlDeviceImage image)
		{
			return loadSqlImageCommon<EtPQPSqlDeviceImage>(fileName, ref image);
		}

		private bool loadSqlImage(string fileName, ref EtPQP_A_SqlDeviceImage image)
		{
			return loadSqlImageCommon<EtPQP_A_SqlDeviceImage>(fileName, ref image);
		}

		private bool loadSqlImageCommon<T>(string fileName, ref T image)
		{
			FileStream fs = null;
			StreamReader sr = null;
			try
			{
				if (!File.Exists(fileName)) return false;
				// сколько символов обозначает перевод строки (1 или 2)
				short brCount = 1;

				// открываем файл образа
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs);
				char c;
				string strCntHeaderStrings = string.Empty;
				while (Char.IsDigit((char)sr.Peek()))
				{
					c = (char)sr.Read();
					strCntHeaderStrings += c;
				}
				// это считано кол-во строк заголовка
				int cntHeaderStrings = Int32.Parse(strCntHeaderStrings);

				// вдруг что-то случайно окажется перед переводом строки
				while (sr.Peek() != 0x0A && sr.Peek() != 0x0D) sr.Read();
				// читаем перевод строки
				sr.Read();
				// проверяем есть ли еще
				while (sr.Peek() == 0x0A || sr.Peek() == 0x0D)
				{
					brCount++;
					sr.Read();
				}

				// длина заголовка в байтах. ее будем рассчитывать в процессе чтения заголовка
				Int64 headerLen = strCntHeaderStrings.Length + brCount;
				// считываем размер образа
				string strFileImageSize = sr.ReadLine();
				headerLen += strFileImageSize.Length + brCount;
				int fileImageSize = Int32.Parse(strFileImageSize);
				// считываем данные о вспомогательных файлах: имя + размер
				List<Pair<string, Int64>> dicAvgFiles = new List<Pair<string, Int64>>();
				for (int iHeader = 1; iHeader < cntHeaderStrings; iHeader += 2)
				{
					string name = sr.ReadLine();
					string strLen = sr.ReadLine();
					// при вычислении длины прибавляем кол-во символов перевода строки
					headerLen += (name.Length + brCount + strLen.Length + brCount);
					dicAvgFiles.Add(new Pair<string, Int64>(name, Int64.Parse(strLen)));
				}

				// формируем временный файл, в который запишем образ, чтобы из этого файла
				// сделать десериализацию
				string fileNameImageTmp = EmService.TEMP_IMAGE_DIR + "CommonSqlDeviceImage.tmp";
				FileStream fsImageTmp = null;
				try
				{
					fsImageTmp = new FileStream(fileNameImageTmp, FileMode.Create);
					fs.Position = headerLen;
					byte[] bufTmp = new byte[fileImageSize];
					int dwReadTmp = fs.Read(bufTmp, 0, bufTmp.Length);
					if (dwReadTmp != fileImageSize)
						EmService.WriteToLogFailed(String.Format(
							"loadSqlImage(): dwReadTmp = {0}, but fileImageSize = {1}!",
							dwReadTmp, fileImageSize));
					fsImageTmp.Write(bufTmp, 0, bufTmp.Length);
					fsImageTmp.Position = 0;
					//fsImageTmp.Close();
					XmlSerializer xmlSer = new XmlSerializer(typeof(T));
					image = (T)xmlSer.Deserialize(fsImageTmp);
				}
				finally
				{
					fsImageTmp.Close();
					File.Delete(fileNameImageTmp);
				}

				// создаем вспомогательные файлы для усредненных
				//fs.Position = fileImageSize + headerLen + 2;
				for (int iDic = 0; iDic < dicAvgFiles.Count; ++iDic)
				{
					FileStream fsAvg = null;
					try
					{
						fsAvg = File.Create(EmService.TEMP_IMAGE_DIR + dicAvgFiles[iDic].First);
						byte[] buffer = new byte[dicAvgFiles[iDic].Second];
						int dwRead = fs.Read(buffer, 0, (int)dicAvgFiles[iDic].Second);
						if (dwRead != dicAvgFiles[iDic].Second)
							EmService.WriteToLogFailed(
								"loadSqlImage: while reading AVG file dwRead = " + dwRead.ToString() +
								", dicAvgFiles[iDic].Second = " + dicAvgFiles[iDic].Second.ToString());
						fsAvg.Write(buffer, 0, buffer.Length);
						Thread.Sleep(3000);
					}
					finally
					{
						if (fsAvg != null) fsAvg.Close();
					}
				}
			}
			catch (Exception e)
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				string msg = string.Format(
					rm.GetString("msg_image_bad_sql_image_file_text"), fileName);
				string cap = rm.GetString("msg_image_bad_sql_image_file_caption");

				MessageBox.Show(this, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
				EmService.WriteToLogFailed("Error in loadSqlImageCommon():  " + e.Message);
				return false;
			}
			finally
			{
				if (sr != null) sr.Close();
				if (fs != null) fs.Close();
			}
			return true;
		}

		private static void CheckIfDirectoryExists(string fileName, object sender)
		{
			try
			{
				System.IO.FileInfo fi = new FileInfo(fileName);
				if (!fi.Directory.Exists)
				{
					Directory.CreateDirectory(fi.Directory.FullName);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex,
					"Error in EmDataSaverBase::saveXmlImage():");
				MessageBoxes.ErrorCreateDir(sender);
				throw;
			}
		}

		#endregion
	}

	class ClosingInfoThread
	{
		private frmMessage wndInfo_;
		private string mess_;

		public ClosingInfoThread(frmMain owner, string mess)
		{
			owner.OnHideWndInfo += new frmMain.HideWndInfoHandler(wndInfo_OnHideWndInfo);
			mess_ = mess;
		}

		public void ThreadEntry()
		{
			wndInfo_ = frmMessage.Instance(mess_, false, 12.0f);
			wndInfo_.ShowDialog();
		}

		void wndInfo_OnHideWndInfo()
		{
			try
			{
				if (wndInfo_.InvokeRequired == false) // thread checking
				{
					if (wndInfo_ != null) wndInfo_.Hide();
				}
				else
				{
					frmMain.HideWndInfoHandler hideWnd =
						new frmMain.HideWndInfoHandler(wndInfo_OnHideWndInfo);
					wndInfo_.Invoke(hideWnd);
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in wndInfo_OnHideWndInfo(): " + ex.Message);
			}
		}
	}
}
