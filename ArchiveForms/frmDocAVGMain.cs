using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Resources;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using DataGridColumnStyles;
using ZedGraph;
using EnergomonitoringXP.Graph;
using EmServiceLib;
using WeifenLuo.WinFormsUI;
using EmArchiveTree;
using DbServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmDoc2.
	/// </summary>
	public class frmDocAVGMain : DockContentGraphMethods
	{
		private AVGDataGridWrapperUIF dgWrapperUIF_;
		private AVGDataGridWrapperPower dgWrapperPower_;
		private AVGDataGridWrapperPQP dgWrapperPQP_;
		private AVGDataGridWrapperAngles dgWrapperAngles_;
		private AVGDataGridWrapperHarmUph dgWrapperHarmUph_;
		private AVGDataGridWrapperInterHarmUph dgWrapperInterHarmUph_;
		private AVGDataGridWrapperHarmUlin dgWrapperHarmUlin_;
		private AVGDataGridWrapperInterHarmUlin dgWrapperInterHarmUlin_;
		private AVGDataGridWrapperHarmI dgWrapperHarmI_;
		private AVGDataGridWrapperInterHarmI dgWrapperInterHarmI_;
		private AVGDataGridWrapperHarmPower dgWrapperHarmPower_;
		private AVGDataGridWrapperHarmAngles dgWrapperHarmAngles_;

		private IContainer components;
		private ContextMenuStrip cmsDoc2;
		private ToolStripMenuItem miAddColumnToGraphLeft;
		private ToolStripMenuItem miAddColumnToGraphRight;
		private ToolStripSeparator miSeparator;
		private ToolStripMenuItem miSort;
		private ToolStripMenuItem miSortDesc;

		//public frmDocAVGGraphBottom wndDocAVGGraph;

		/// <summary>
		/// Settings object
		/// </summary>
		private EmDataSaver.Settings settings_;

		private EmDeviceType curDevType_;

		// здесь инфа о добавленных графиках, чтобы не добавить один и тот же 2 раза
		List<Pair<string, int>> listGraphBottomAdded_ = new List<Pair<string, int>>();

		/// <summary>
		/// Pointer to the main application window
		/// </summary>
		internal frmMain MainWindow_;

		private ConnectScheme curConnectionScheme_ = ConnectScheme.Unknown;
		private int curPgServerIndex_ = 0;
		private Int64 curDatetimeId_ = 0;
		private bool curWithTR_;
		private ToolStripSeparator miSeparator2;
		private ToolStripMenuItem miHarmPowersPercent;
		private ToolStripMenuItem miHarmPowersWatt;

		private bool bHarmPowersShownInPercent_ = false;
		private bool bHarm_I_ShownInPercent_ = true;
		private ToolStripMenuItem miHarmCurPercent;
		private ToolStripMenuItem miHarmCurAmpere;
		private ToolTip ttAVG;

		private float uLimit_;
		private float iLimit_;

		// интервал между отсчетами в секундах
		private int period_in_secs_ = 0;
		private TabControl tabControl1;
		private TabPage tabCurrentsAndVoltages;
		private DataGrid dgCurrentsVoltages;
		private Label lblCurrentsAndVoltages;
		private TabPage tabPowers;
		private DataGrid dgPowers;
		private Label lblPowers;
		private TabPage tabAngles;
		private DataGrid dgPhAngles;
		private Label lblAngles;
		private TabPage tabPQP;
		private DataGrid dgPQP;
		private Label lblPQP;
		private TabPage tabHarmonicI;
		private DataGrid dgCurrentHarmonics;
		private Label lblHarmonicsCur;
		private TabPage tabHarmonicVoltagePh;
		private DataGrid dgVolPhHarmonics;
		private Label lblHarmonicsVolPh;
		private TabPage tabHarmonicVoltageLin;
		private DataGrid dgVolLinHarmonics;
		private Label lblHarmonicsVolLin;
		private TabPage tabHarmonicPowers;
		private DataGrid dgHarmonicPowers;
		private Label lblHarmonicPowers;
		private TabPage tabHarmonicAngles;
		private DataGrid dgHarmonicAngles;
		private Label lblHarmonicAngles;
		private TabPage tabInterHarmI;
		private DataGrid dgI_Interharm;
		private Label lbl_I_Interharm;
		private TabPage tabInterHarmUph;
		private DataGrid dgUph_Interharm;
		private Label lbl_U_Interharm;
		private TabPage tabInterHarmUlin;
		private DataGrid dgUlin_Interharm;
		private Label lbl_Ulin_Interharm;
		private Label lbl_AVG_Caption;

		/// <summary>
		/// All posible data objects
		/// </summary>
		internal CurrencyManager[]CMs_ = new CurrencyManager[13] { null, null, null, null, null, null,
										null, null, null, null, null, null, null };

		/// <summary>
		/// Synchronize settings
		/// </summary>
		/// <param name="NewSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			settings_ = NewSettings.Clone();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="MainWindow">Pointer to the main application window</param>
		/// <param name="settings">settings object</param>
		public frmDocAVGMain(frmMain MainWindow, EmDataSaver.Settings settings)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.settings_ = settings;
			this.MainWindow_ = MainWindow;
			//wndDocAVGGraph = new frmDocAVGGraphBottom(this.MainWindow, settings);
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocAVGMain));
            this.cmsDoc2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miSort = new System.Windows.Forms.ToolStripMenuItem();
            this.miSortDesc = new System.Windows.Forms.ToolStripMenuItem();
            this.miSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.miAddColumnToGraphLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.miAddColumnToGraphRight = new System.Windows.Forms.ToolStripMenuItem();
            this.miSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.miHarmPowersPercent = new System.Windows.Forms.ToolStripMenuItem();
            this.miHarmPowersWatt = new System.Windows.Forms.ToolStripMenuItem();
            this.miHarmCurPercent = new System.Windows.Forms.ToolStripMenuItem();
            this.miHarmCurAmpere = new System.Windows.Forms.ToolStripMenuItem();
            this.ttAVG = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabCurrentsAndVoltages = new System.Windows.Forms.TabPage();
            this.dgCurrentsVoltages = new System.Windows.Forms.DataGrid();
            this.lblCurrentsAndVoltages = new System.Windows.Forms.Label();
            this.tabPowers = new System.Windows.Forms.TabPage();
            this.dgPowers = new System.Windows.Forms.DataGrid();
            this.lblPowers = new System.Windows.Forms.Label();
            this.tabAngles = new System.Windows.Forms.TabPage();
            this.dgPhAngles = new System.Windows.Forms.DataGrid();
            this.lblAngles = new System.Windows.Forms.Label();
            this.tabPQP = new System.Windows.Forms.TabPage();
            this.dgPQP = new System.Windows.Forms.DataGrid();
            this.lblPQP = new System.Windows.Forms.Label();
            this.tabHarmonicI = new System.Windows.Forms.TabPage();
            this.dgCurrentHarmonics = new System.Windows.Forms.DataGrid();
            this.lblHarmonicsCur = new System.Windows.Forms.Label();
            this.tabHarmonicVoltagePh = new System.Windows.Forms.TabPage();
            this.dgVolPhHarmonics = new System.Windows.Forms.DataGrid();
            this.lblHarmonicsVolPh = new System.Windows.Forms.Label();
            this.tabHarmonicVoltageLin = new System.Windows.Forms.TabPage();
            this.dgVolLinHarmonics = new System.Windows.Forms.DataGrid();
            this.lblHarmonicsVolLin = new System.Windows.Forms.Label();
            this.tabHarmonicPowers = new System.Windows.Forms.TabPage();
            this.dgHarmonicPowers = new System.Windows.Forms.DataGrid();
            this.lblHarmonicPowers = new System.Windows.Forms.Label();
            this.tabHarmonicAngles = new System.Windows.Forms.TabPage();
            this.dgHarmonicAngles = new System.Windows.Forms.DataGrid();
            this.lblHarmonicAngles = new System.Windows.Forms.Label();
            this.tabInterHarmI = new System.Windows.Forms.TabPage();
            this.dgI_Interharm = new System.Windows.Forms.DataGrid();
            this.lbl_I_Interharm = new System.Windows.Forms.Label();
            this.tabInterHarmUph = new System.Windows.Forms.TabPage();
            this.dgUph_Interharm = new System.Windows.Forms.DataGrid();
            this.lbl_U_Interharm = new System.Windows.Forms.Label();
            this.tabInterHarmUlin = new System.Windows.Forms.TabPage();
            this.dgUlin_Interharm = new System.Windows.Forms.DataGrid();
            this.lbl_Ulin_Interharm = new System.Windows.Forms.Label();
            this.lbl_AVG_Caption = new System.Windows.Forms.Label();
            this.cmsDoc2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabCurrentsAndVoltages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCurrentsVoltages)).BeginInit();
            this.tabPowers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgPowers)).BeginInit();
            this.tabAngles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgPhAngles)).BeginInit();
            this.tabPQP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgPQP)).BeginInit();
            this.tabHarmonicI.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCurrentHarmonics)).BeginInit();
            this.tabHarmonicVoltagePh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgVolPhHarmonics)).BeginInit();
            this.tabHarmonicVoltageLin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgVolLinHarmonics)).BeginInit();
            this.tabHarmonicPowers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgHarmonicPowers)).BeginInit();
            this.tabHarmonicAngles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgHarmonicAngles)).BeginInit();
            this.tabInterHarmI.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgI_Interharm)).BeginInit();
            this.tabInterHarmUph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgUph_Interharm)).BeginInit();
            this.tabInterHarmUlin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgUlin_Interharm)).BeginInit();
            this.SuspendLayout();
            // 
            // cmsDoc2
            // 
            resources.ApplyResources(this.cmsDoc2, "cmsDoc2");
            this.cmsDoc2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSort,
            this.miSortDesc,
            this.miSeparator,
            this.miAddColumnToGraphLeft,
            this.miAddColumnToGraphRight,
            this.miSeparator2,
            this.miHarmPowersPercent,
            this.miHarmPowersWatt,
            this.miHarmCurPercent,
            this.miHarmCurAmpere});
            this.cmsDoc2.Name = "cmsDoc2";
            this.ttAVG.SetToolTip(this.cmsDoc2, resources.GetString("cmsDoc2.ToolTip"));
            // 
            // miSort
            // 
            resources.ApplyResources(this.miSort, "miSort");
            this.miSort.Name = "miSort";
            this.miSort.Click += new System.EventHandler(this.miSort_Click);
            // 
            // miSortDesc
            // 
            resources.ApplyResources(this.miSortDesc, "miSortDesc");
            this.miSortDesc.Name = "miSortDesc";
            this.miSortDesc.Click += new System.EventHandler(this.miSortDesc_Click);
            // 
            // miSeparator
            // 
            resources.ApplyResources(this.miSeparator, "miSeparator");
            this.miSeparator.Name = "miSeparator";
            // 
            // miAddColumnToGraphLeft
            // 
            resources.ApplyResources(this.miAddColumnToGraphLeft, "miAddColumnToGraphLeft");
            this.miAddColumnToGraphLeft.Name = "miAddColumnToGraphLeft";
            this.miAddColumnToGraphLeft.Click += new System.EventHandler(this.cmsAddToGraphLeft_Click);
            // 
            // miAddColumnToGraphRight
            // 
            resources.ApplyResources(this.miAddColumnToGraphRight, "miAddColumnToGraphRight");
            this.miAddColumnToGraphRight.Name = "miAddColumnToGraphRight";
            this.miAddColumnToGraphRight.Click += new System.EventHandler(this.cmsAddToGraphRight_Click);
            // 
            // miSeparator2
            // 
            resources.ApplyResources(this.miSeparator2, "miSeparator2");
            this.miSeparator2.Name = "miSeparator2";
            // 
            // miHarmPowersPercent
            // 
            resources.ApplyResources(this.miHarmPowersPercent, "miHarmPowersPercent");
            this.miHarmPowersPercent.Name = "miHarmPowersPercent";
            this.miHarmPowersPercent.Click += new System.EventHandler(this.miHarmPowersPercent_Click);
            // 
            // miHarmPowersWatt
            // 
            resources.ApplyResources(this.miHarmPowersWatt, "miHarmPowersWatt");
            this.miHarmPowersWatt.Name = "miHarmPowersWatt";
            this.miHarmPowersWatt.Click += new System.EventHandler(this.miHarmPowersWatt_Click);
            // 
            // miHarmCurPercent
            // 
            resources.ApplyResources(this.miHarmCurPercent, "miHarmCurPercent");
            this.miHarmCurPercent.Name = "miHarmCurPercent";
            this.miHarmCurPercent.Click += new System.EventHandler(this.miHarmCurPercent_Click);
            // 
            // miHarmCurAmpere
            // 
            resources.ApplyResources(this.miHarmCurAmpere, "miHarmCurAmpere");
            this.miHarmCurAmpere.Name = "miHarmCurAmpere";
            this.miHarmCurAmpere.Click += new System.EventHandler(this.miHarmCurAmpere_Click);
            // 
            // ttAVG
            // 
            this.ttAVG.AutoPopDelay = 3000;
            this.ttAVG.InitialDelay = 500;
            this.ttAVG.ReshowDelay = 100;
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabCurrentsAndVoltages);
            this.tabControl1.Controls.Add(this.tabPowers);
            this.tabControl1.Controls.Add(this.tabAngles);
            this.tabControl1.Controls.Add(this.tabPQP);
            this.tabControl1.Controls.Add(this.tabHarmonicI);
            this.tabControl1.Controls.Add(this.tabHarmonicVoltagePh);
            this.tabControl1.Controls.Add(this.tabHarmonicVoltageLin);
            this.tabControl1.Controls.Add(this.tabHarmonicPowers);
            this.tabControl1.Controls.Add(this.tabHarmonicAngles);
            this.tabControl1.Controls.Add(this.tabInterHarmI);
            this.tabControl1.Controls.Add(this.tabInterHarmUph);
            this.tabControl1.Controls.Add(this.tabInterHarmUlin);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.ttAVG.SetToolTip(this.tabControl1, resources.GetString("tabControl1.ToolTip"));
            // 
            // tabCurrentsAndVoltages
            // 
            resources.ApplyResources(this.tabCurrentsAndVoltages, "tabCurrentsAndVoltages");
            this.tabCurrentsAndVoltages.Controls.Add(this.dgCurrentsVoltages);
            this.tabCurrentsAndVoltages.Controls.Add(this.lblCurrentsAndVoltages);
            this.tabCurrentsAndVoltages.Name = "tabCurrentsAndVoltages";
            this.ttAVG.SetToolTip(this.tabCurrentsAndVoltages, resources.GetString("tabCurrentsAndVoltages.ToolTip"));
            this.tabCurrentsAndVoltages.UseVisualStyleBackColor = true;
            // 
            // dgCurrentsVoltages
            // 
            resources.ApplyResources(this.dgCurrentsVoltages, "dgCurrentsVoltages");
            this.dgCurrentsVoltages.AllowSorting = false;
            this.dgCurrentsVoltages.DataMember = "";
            this.dgCurrentsVoltages.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgCurrentsVoltages.Name = "dgCurrentsVoltages";
            this.ttAVG.SetToolTip(this.dgCurrentsVoltages, resources.GetString("dgCurrentsVoltages.ToolTip"));
            this.dgCurrentsVoltages.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgCurrentsVoltages.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgCurrentsVoltages.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblCurrentsAndVoltages
            // 
            resources.ApplyResources(this.lblCurrentsAndVoltages, "lblCurrentsAndVoltages");
            this.lblCurrentsAndVoltages.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblCurrentsAndVoltages.Name = "lblCurrentsAndVoltages";
            this.ttAVG.SetToolTip(this.lblCurrentsAndVoltages, resources.GetString("lblCurrentsAndVoltages.ToolTip"));
            // 
            // tabPowers
            // 
            resources.ApplyResources(this.tabPowers, "tabPowers");
            this.tabPowers.Controls.Add(this.dgPowers);
            this.tabPowers.Controls.Add(this.lblPowers);
            this.tabPowers.Name = "tabPowers";
            this.ttAVG.SetToolTip(this.tabPowers, resources.GetString("tabPowers.ToolTip"));
            this.tabPowers.UseVisualStyleBackColor = true;
            // 
            // dgPowers
            // 
            resources.ApplyResources(this.dgPowers, "dgPowers");
            this.dgPowers.AllowSorting = false;
            this.dgPowers.DataMember = "";
            this.dgPowers.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgPowers.Name = "dgPowers";
            this.ttAVG.SetToolTip(this.dgPowers, resources.GetString("dgPowers.ToolTip"));
            this.dgPowers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgPowers.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgPowers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblPowers
            // 
            resources.ApplyResources(this.lblPowers, "lblPowers");
            this.lblPowers.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblPowers.Name = "lblPowers";
            this.ttAVG.SetToolTip(this.lblPowers, resources.GetString("lblPowers.ToolTip"));
            // 
            // tabAngles
            // 
            resources.ApplyResources(this.tabAngles, "tabAngles");
            this.tabAngles.Controls.Add(this.dgPhAngles);
            this.tabAngles.Controls.Add(this.lblAngles);
            this.tabAngles.Name = "tabAngles";
            this.ttAVG.SetToolTip(this.tabAngles, resources.GetString("tabAngles.ToolTip"));
            this.tabAngles.UseVisualStyleBackColor = true;
            // 
            // dgPhAngles
            // 
            resources.ApplyResources(this.dgPhAngles, "dgPhAngles");
            this.dgPhAngles.AllowSorting = false;
            this.dgPhAngles.DataMember = "";
            this.dgPhAngles.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgPhAngles.Name = "dgPhAngles";
            this.dgPhAngles.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgPhAngles, resources.GetString("dgPhAngles.ToolTip"));
            this.dgPhAngles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgPhAngles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgPhAngles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblAngles
            // 
            resources.ApplyResources(this.lblAngles, "lblAngles");
            this.lblAngles.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblAngles.Name = "lblAngles";
            this.ttAVG.SetToolTip(this.lblAngles, resources.GetString("lblAngles.ToolTip"));
            // 
            // tabPQP
            // 
            resources.ApplyResources(this.tabPQP, "tabPQP");
            this.tabPQP.Controls.Add(this.dgPQP);
            this.tabPQP.Controls.Add(this.lblPQP);
            this.tabPQP.Name = "tabPQP";
            this.ttAVG.SetToolTip(this.tabPQP, resources.GetString("tabPQP.ToolTip"));
            this.tabPQP.UseVisualStyleBackColor = true;
            // 
            // dgPQP
            // 
            resources.ApplyResources(this.dgPQP, "dgPQP");
            this.dgPQP.AllowSorting = false;
            this.dgPQP.DataMember = "";
            this.dgPQP.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgPQP.Name = "dgPQP";
            this.dgPQP.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgPQP, resources.GetString("dgPQP.ToolTip"));
            this.dgPQP.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgPQP.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgPQP.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblPQP
            // 
            resources.ApplyResources(this.lblPQP, "lblPQP");
            this.lblPQP.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblPQP.Name = "lblPQP";
            this.ttAVG.SetToolTip(this.lblPQP, resources.GetString("lblPQP.ToolTip"));
            // 
            // tabHarmonicI
            // 
            resources.ApplyResources(this.tabHarmonicI, "tabHarmonicI");
            this.tabHarmonicI.Controls.Add(this.dgCurrentHarmonics);
            this.tabHarmonicI.Controls.Add(this.lblHarmonicsCur);
            this.tabHarmonicI.Name = "tabHarmonicI";
            this.ttAVG.SetToolTip(this.tabHarmonicI, resources.GetString("tabHarmonicI.ToolTip"));
            this.tabHarmonicI.UseVisualStyleBackColor = true;
            // 
            // dgCurrentHarmonics
            // 
            resources.ApplyResources(this.dgCurrentHarmonics, "dgCurrentHarmonics");
            this.dgCurrentHarmonics.AllowSorting = false;
            this.dgCurrentHarmonics.DataMember = "";
            this.dgCurrentHarmonics.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgCurrentHarmonics.Name = "dgCurrentHarmonics";
            this.dgCurrentHarmonics.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgCurrentHarmonics, resources.GetString("dgCurrentHarmonics.ToolTip"));
            this.dgCurrentHarmonics.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgCurrentHarmonics.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgCurrentHarmonics.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblHarmonicsCur
            // 
            resources.ApplyResources(this.lblHarmonicsCur, "lblHarmonicsCur");
            this.lblHarmonicsCur.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHarmonicsCur.Name = "lblHarmonicsCur";
            this.ttAVG.SetToolTip(this.lblHarmonicsCur, resources.GetString("lblHarmonicsCur.ToolTip"));
            // 
            // tabHarmonicVoltagePh
            // 
            resources.ApplyResources(this.tabHarmonicVoltagePh, "tabHarmonicVoltagePh");
            this.tabHarmonicVoltagePh.Controls.Add(this.dgVolPhHarmonics);
            this.tabHarmonicVoltagePh.Controls.Add(this.lblHarmonicsVolPh);
            this.tabHarmonicVoltagePh.Name = "tabHarmonicVoltagePh";
            this.ttAVG.SetToolTip(this.tabHarmonicVoltagePh, resources.GetString("tabHarmonicVoltagePh.ToolTip"));
            this.tabHarmonicVoltagePh.UseVisualStyleBackColor = true;
            // 
            // dgVolPhHarmonics
            // 
            resources.ApplyResources(this.dgVolPhHarmonics, "dgVolPhHarmonics");
            this.dgVolPhHarmonics.AllowSorting = false;
            this.dgVolPhHarmonics.DataMember = "";
            this.dgVolPhHarmonics.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgVolPhHarmonics.Name = "dgVolPhHarmonics";
            this.dgVolPhHarmonics.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgVolPhHarmonics, resources.GetString("dgVolPhHarmonics.ToolTip"));
            this.dgVolPhHarmonics.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgVolPhHarmonics.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgVolPhHarmonics.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblHarmonicsVolPh
            // 
            resources.ApplyResources(this.lblHarmonicsVolPh, "lblHarmonicsVolPh");
            this.lblHarmonicsVolPh.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHarmonicsVolPh.Name = "lblHarmonicsVolPh";
            this.ttAVG.SetToolTip(this.lblHarmonicsVolPh, resources.GetString("lblHarmonicsVolPh.ToolTip"));
            // 
            // tabHarmonicVoltageLin
            // 
            resources.ApplyResources(this.tabHarmonicVoltageLin, "tabHarmonicVoltageLin");
            this.tabHarmonicVoltageLin.Controls.Add(this.dgVolLinHarmonics);
            this.tabHarmonicVoltageLin.Controls.Add(this.lblHarmonicsVolLin);
            this.tabHarmonicVoltageLin.Name = "tabHarmonicVoltageLin";
            this.ttAVG.SetToolTip(this.tabHarmonicVoltageLin, resources.GetString("tabHarmonicVoltageLin.ToolTip"));
            this.tabHarmonicVoltageLin.UseVisualStyleBackColor = true;
            // 
            // dgVolLinHarmonics
            // 
            resources.ApplyResources(this.dgVolLinHarmonics, "dgVolLinHarmonics");
            this.dgVolLinHarmonics.AllowSorting = false;
            this.dgVolLinHarmonics.DataMember = "";
            this.dgVolLinHarmonics.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgVolLinHarmonics.Name = "dgVolLinHarmonics";
            this.dgVolLinHarmonics.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgVolLinHarmonics, resources.GetString("dgVolLinHarmonics.ToolTip"));
            this.dgVolLinHarmonics.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgVolLinHarmonics.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgVolLinHarmonics.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblHarmonicsVolLin
            // 
            resources.ApplyResources(this.lblHarmonicsVolLin, "lblHarmonicsVolLin");
            this.lblHarmonicsVolLin.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHarmonicsVolLin.Name = "lblHarmonicsVolLin";
            this.ttAVG.SetToolTip(this.lblHarmonicsVolLin, resources.GetString("lblHarmonicsVolLin.ToolTip"));
            // 
            // tabHarmonicPowers
            // 
            resources.ApplyResources(this.tabHarmonicPowers, "tabHarmonicPowers");
            this.tabHarmonicPowers.Controls.Add(this.dgHarmonicPowers);
            this.tabHarmonicPowers.Controls.Add(this.lblHarmonicPowers);
            this.tabHarmonicPowers.Name = "tabHarmonicPowers";
            this.ttAVG.SetToolTip(this.tabHarmonicPowers, resources.GetString("tabHarmonicPowers.ToolTip"));
            this.tabHarmonicPowers.UseVisualStyleBackColor = true;
            // 
            // dgHarmonicPowers
            // 
            resources.ApplyResources(this.dgHarmonicPowers, "dgHarmonicPowers");
            this.dgHarmonicPowers.AllowSorting = false;
            this.dgHarmonicPowers.DataMember = "";
            this.dgHarmonicPowers.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgHarmonicPowers.Name = "dgHarmonicPowers";
            this.dgHarmonicPowers.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgHarmonicPowers, resources.GetString("dgHarmonicPowers.ToolTip"));
            this.dgHarmonicPowers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgHarmonicPowers.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgHarmonicPowers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblHarmonicPowers
            // 
            resources.ApplyResources(this.lblHarmonicPowers, "lblHarmonicPowers");
            this.lblHarmonicPowers.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHarmonicPowers.Name = "lblHarmonicPowers";
            this.ttAVG.SetToolTip(this.lblHarmonicPowers, resources.GetString("lblHarmonicPowers.ToolTip"));
            // 
            // tabHarmonicAngles
            // 
            resources.ApplyResources(this.tabHarmonicAngles, "tabHarmonicAngles");
            this.tabHarmonicAngles.Controls.Add(this.dgHarmonicAngles);
            this.tabHarmonicAngles.Controls.Add(this.lblHarmonicAngles);
            this.tabHarmonicAngles.Name = "tabHarmonicAngles";
            this.ttAVG.SetToolTip(this.tabHarmonicAngles, resources.GetString("tabHarmonicAngles.ToolTip"));
            this.tabHarmonicAngles.UseVisualStyleBackColor = true;
            // 
            // dgHarmonicAngles
            // 
            resources.ApplyResources(this.dgHarmonicAngles, "dgHarmonicAngles");
            this.dgHarmonicAngles.AllowSorting = false;
            this.dgHarmonicAngles.DataMember = "";
            this.dgHarmonicAngles.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgHarmonicAngles.Name = "dgHarmonicAngles";
            this.dgHarmonicAngles.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgHarmonicAngles, resources.GetString("dgHarmonicAngles.ToolTip"));
            this.dgHarmonicAngles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgHarmonicAngles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgHarmonicAngles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lblHarmonicAngles
            // 
            resources.ApplyResources(this.lblHarmonicAngles, "lblHarmonicAngles");
            this.lblHarmonicAngles.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHarmonicAngles.Name = "lblHarmonicAngles";
            this.ttAVG.SetToolTip(this.lblHarmonicAngles, resources.GetString("lblHarmonicAngles.ToolTip"));
            // 
            // tabInterHarmI
            // 
            resources.ApplyResources(this.tabInterHarmI, "tabInterHarmI");
            this.tabInterHarmI.Controls.Add(this.dgI_Interharm);
            this.tabInterHarmI.Controls.Add(this.lbl_I_Interharm);
            this.tabInterHarmI.Name = "tabInterHarmI";
            this.ttAVG.SetToolTip(this.tabInterHarmI, resources.GetString("tabInterHarmI.ToolTip"));
            this.tabInterHarmI.UseVisualStyleBackColor = true;
            // 
            // dgI_Interharm
            // 
            resources.ApplyResources(this.dgI_Interharm, "dgI_Interharm");
            this.dgI_Interharm.AllowSorting = false;
            this.dgI_Interharm.DataMember = "";
            this.dgI_Interharm.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgI_Interharm.Name = "dgI_Interharm";
            this.dgI_Interharm.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgI_Interharm, resources.GetString("dgI_Interharm.ToolTip"));
            this.dgI_Interharm.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgI_Interharm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgI_Interharm.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lbl_I_Interharm
            // 
            resources.ApplyResources(this.lbl_I_Interharm, "lbl_I_Interharm");
            this.lbl_I_Interharm.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_I_Interharm.Name = "lbl_I_Interharm";
            this.ttAVG.SetToolTip(this.lbl_I_Interharm, resources.GetString("lbl_I_Interharm.ToolTip"));
            // 
            // tabInterHarmUph
            // 
            resources.ApplyResources(this.tabInterHarmUph, "tabInterHarmUph");
            this.tabInterHarmUph.Controls.Add(this.dgUph_Interharm);
            this.tabInterHarmUph.Controls.Add(this.lbl_U_Interharm);
            this.tabInterHarmUph.Name = "tabInterHarmUph";
            this.ttAVG.SetToolTip(this.tabInterHarmUph, resources.GetString("tabInterHarmUph.ToolTip"));
            this.tabInterHarmUph.UseVisualStyleBackColor = true;
            // 
            // dgUph_Interharm
            // 
            resources.ApplyResources(this.dgUph_Interharm, "dgUph_Interharm");
            this.dgUph_Interharm.AllowSorting = false;
            this.dgUph_Interharm.DataMember = "";
            this.dgUph_Interharm.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgUph_Interharm.Name = "dgUph_Interharm";
            this.dgUph_Interharm.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgUph_Interharm, resources.GetString("dgUph_Interharm.ToolTip"));
            this.dgUph_Interharm.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgUph_Interharm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgUph_Interharm.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lbl_U_Interharm
            // 
            resources.ApplyResources(this.lbl_U_Interharm, "lbl_U_Interharm");
            this.lbl_U_Interharm.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_U_Interharm.Name = "lbl_U_Interharm";
            this.ttAVG.SetToolTip(this.lbl_U_Interharm, resources.GetString("lbl_U_Interharm.ToolTip"));
            // 
            // tabInterHarmUlin
            // 
            resources.ApplyResources(this.tabInterHarmUlin, "tabInterHarmUlin");
            this.tabInterHarmUlin.Controls.Add(this.dgUlin_Interharm);
            this.tabInterHarmUlin.Controls.Add(this.lbl_Ulin_Interharm);
            this.tabInterHarmUlin.Name = "tabInterHarmUlin";
            this.ttAVG.SetToolTip(this.tabInterHarmUlin, resources.GetString("tabInterHarmUlin.ToolTip"));
            this.tabInterHarmUlin.UseVisualStyleBackColor = true;
            // 
            // dgUlin_Interharm
            // 
            resources.ApplyResources(this.dgUlin_Interharm, "dgUlin_Interharm");
            this.dgUlin_Interharm.AllowSorting = false;
            this.dgUlin_Interharm.DataMember = "";
            this.dgUlin_Interharm.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgUlin_Interharm.Name = "dgUlin_Interharm";
            this.dgUlin_Interharm.ReadOnly = true;
            this.ttAVG.SetToolTip(this.dgUlin_Interharm, resources.GetString("dgUlin_Interharm.ToolTip"));
            this.dgUlin_Interharm.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
            this.dgUlin_Interharm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseMove);
            this.dgUlin_Interharm.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            // 
            // lbl_Ulin_Interharm
            // 
            resources.ApplyResources(this.lbl_Ulin_Interharm, "lbl_Ulin_Interharm");
            this.lbl_Ulin_Interharm.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_Ulin_Interharm.Name = "lbl_Ulin_Interharm";
            this.ttAVG.SetToolTip(this.lbl_Ulin_Interharm, resources.GetString("lbl_Ulin_Interharm.ToolTip"));
            // 
            // lbl_AVG_Caption
            // 
            resources.ApplyResources(this.lbl_AVG_Caption, "lbl_AVG_Caption");
            this.lbl_AVG_Caption.Name = "lbl_AVG_Caption";
            this.ttAVG.SetToolTip(this.lbl_AVG_Caption, resources.GetString("lbl_AVG_Caption.ToolTip"));
            // 
            // frmDocAVGMain
            // 
            resources.ApplyResources(this, "$this");
            this.AllowRedocking = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CloseButton = false;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.lbl_AVG_Caption);
            this.DockableAreas = WeifenLuo.WinFormsUI.DockAreas.Document;
            this.HideOnClose = true;
            this.Name = "frmDocAVGMain";
            this.ShowHint = WeifenLuo.WinFormsUI.DockState.Document;
            this.ttAVG.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.frmDocAVGMain_Load);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseUp);
            this.cmsDoc2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabCurrentsAndVoltages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgCurrentsVoltages)).EndInit();
            this.tabPowers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgPowers)).EndInit();
            this.tabAngles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgPhAngles)).EndInit();
            this.tabPQP.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgPQP)).EndInit();
            this.tabHarmonicI.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgCurrentHarmonics)).EndInit();
            this.tabHarmonicVoltagePh.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgVolPhHarmonics)).EndInit();
            this.tabHarmonicVoltageLin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgVolLinHarmonics)).EndInit();
            this.tabHarmonicPowers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgHarmonicPowers)).EndInit();
            this.tabHarmonicAngles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgHarmonicAngles)).EndInit();
            this.tabInterHarmI.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgI_Interharm)).EndInit();
            this.tabInterHarmUph.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgUph_Interharm)).EndInit();
            this.tabInterHarmUlin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgUlin_Interharm)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		#region Common operations

		/// <summary>
		/// Setting up common information header (NOT for Et-PQP-A)
		/// </summary>
		public void SetCommonCaption(
			DateTime start, DateTime end,
			AvgTypes avgType,
			Int64 cur_id,
			bool CurrentWithTR,
			EmDeviceType devType)
		{
			try
			{
				listGraphBottomAdded_.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string strAvgTime = rm.GetString(EmService.GetAvgTypeStringForRM(avgType));

				DbService dbService = new DbService(
					AVGDataGridWrapperBase.GetPgConnectionString(curDevType_, curPgServerIndex_, ref settings_));
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port,
						dbService.Database);
					return;
				}

				string CtrValue = string.Empty;
				string VtrValue = string.Empty;
				string commandText;
				try
				{
					switch (devType)
					{
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
						case EmDeviceType.EM31K:
							commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", cur_id);
							break;
						case EmDeviceType.EM32:
							commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where object_id = {0});", cur_id);
							break;
						case EmDeviceType.ETPQP:
							commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where object_id = {0});", cur_id);
							break;
						default:
							EmService.WriteToLogFailed("Invalid devType in SetCommonCaption1");
							EmService.WriteToLogFailed(devType.ToString());
							return;
					}
					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (curDevType_ != EmDeviceType.ETPQP)
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						switch (iType)
						{
							case 1: // voltage
								VtrValue = (fValue1 / fValue2).ToString();
								break;

							case 2: // current
								CtrValue = (fValue1 / fValue2).ToString();
								break;
						} // switch
					} // while
				} // try
				catch
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}
				finally
				{
					dbService.CloseConnect();
				}

				string strCtrValue = string.Empty;
				string strVtrValue = string.Empty;

				bool vtr_or_ctr_exists = CtrValue != string.Empty || VtrValue != string.Empty;
				strCtrValue = CtrValue != string.Empty ?
					CtrValue : rm.GetString("name_peakload_none");

				strVtrValue = (VtrValue == string.Empty) ? rm.GetString("name_peakload_none")
					: VtrValue;

				if (!CurrentWithTR && vtr_or_ctr_exists) strVtrValue += rm.GetString("name_peakload_notapplied");

				lbl_AVG_Caption.Text = String.Format(
					rm.GetString("window_prefix_common_avg"),
					start, end, strAvgTime,
					strCtrValue, strVtrValue);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SetCommonCaptionAVG():");
				throw;
			}
		}

		/// <summary>
		/// Setting up common information header (for Et-PQP-A)
		/// </summary>
		public void SetCommonCaption(DateTime start, DateTime end,
			AvgTypes_PQP_A avgType, Int64 cur_id)
		{
			try
			{
				listGraphBottomAdded_.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string strAvgTime = rm.GetString(EmService.GetAvgTypeStringForRM(avgType));

				DbService dbService = new DbService(AVGDataGridWrapperBase.GetPgConnectionString(curDevType_, curPgServerIndex_, ref settings_));
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}

				string commandText = "SELECT u_transformer_enable, u_transformer_type, i_transformer_enable, i_transformer_primary FROM avg_times WHERE datetime_id = " + cur_id.ToString() + ";";

				bool u_transformer_enable = false;
				short i_transformer_usage = 0;
				short u_transformer_type = 0;
				int i_transformer_primary = 1;
				int i_transformer_secondary = 1;
				try
				{
					dbService.ExecuteReader(commandText);
					while (dbService.DataReaderRead())
					{
						object tmpObj = dbService.DataReaderData("u_transformer_enable");
						if (tmpObj is System.DBNull) u_transformer_enable = false;
						else u_transformer_enable = (bool)tmpObj;

						tmpObj = dbService.DataReaderData("i_transformer_enable");
						if (tmpObj is System.DBNull) i_transformer_usage = 0;
						else i_transformer_usage = (short)tmpObj;

						tmpObj = dbService.DataReaderData("u_transformer_type");
						if (tmpObj is System.DBNull) u_transformer_type = 0;
						else u_transformer_type = (short)tmpObj;

						tmpObj = dbService.DataReaderData("i_transformer_primary");
						if (tmpObj is System.DBNull) i_transformer_primary = 1;
						else i_transformer_primary = (int)tmpObj;
					}
				}
				catch (Exception tmpEx)
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					EmService.DumpException(tmpEx, "Error in SetCommonCaption() EtPQP-A Read:");
					return;
				}
				finally
				{
					dbService.CloseConnect();
				}

				string strCtrValue = string.Empty;
				string strVtrValue = string.Empty;

				if (u_transformer_enable)
				{
					strVtrValue = "1:" +
						(DeviceIO.EtPQP_A_Device.GetUTransformerMultiplier(u_transformer_type)).ToString();
				}
				else
					strVtrValue = rm.GetString("name_peakload_none");

				if (i_transformer_usage == 1 || i_transformer_usage == 2)
				{
					if (i_transformer_usage == 1)
						i_transformer_secondary = 1;
					else if (i_transformer_usage == 2)
						i_transformer_secondary = 5;

					strCtrValue = string.Format("{0}, {1}", i_transformer_primary, i_transformer_secondary);
				}
				else
					strCtrValue = rm.GetString("name_peakload_none");

				lbl_AVG_Caption.Text = String.Format(
					rm.GetString("window_prefix_common_avg"),
					start, end, strAvgTime,
					strCtrValue, strVtrValue);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SetCommonCaptionAVG() EtPQP-A:");
				throw;
			}
		}

		/// <summary>
		/// External function to prepare <c>wndDoc2</c> data grids (for Et-PQP-A!)
		/// </summary>
		// <param name="PgServerIndex">PostgreSQL server index</param>
		// <param name="WithTR">Open With Transform Ratio or Not</param>
		public void IninializeDoc2Grids(int pgServerIndex, Int64 datetimeID, ConnectScheme connectionScheme,
										float iLimit, float uLimit, EmDeviceType devType)
		{
			try
			{
				curPgServerIndex_ = pgServerIndex;
				curDatetimeId_ = datetimeID;
				curConnectionScheme_ = connectionScheme;
				curWithTR_ = false;
				curDevType_ = devType;
				iLimit_ = iLimit;
				uLimit_ = uLimit;

				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					AvgTypes avgType = ((this.MainWindow_.wndToolbox.ActiveNodeAVG as
										EmTreeNodeDBMeasureClassic).AvgType);
					if (avgType == AvgTypes.ThreeSec) period_in_secs_ = 3;
					else if (avgType == AvgTypes.OneMin) period_in_secs_ = 60;
					else if (avgType == AvgTypes.ThirtyMin) period_in_secs_ = 1800;
				}
				else
				{
					AvgTypes_PQP_A avgType = ((this.MainWindow_.wndToolbox.ActiveNodeAVG as
										EmTreeNodeDBMeasureEtPQP_A).AvgType_PQP_A);
					if (avgType == AvgTypes_PQP_A.ThreeSec) period_in_secs_ = 3;
					else if (avgType == AvgTypes_PQP_A.TenMin) period_in_secs_ = 600;
					else if (avgType == AvgTypes_PQP_A.TwoHours) period_in_secs_ = 7200;
				}

				MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurDevType = devType;
				MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurPeriodInSecs = period_in_secs_;

				bHarmPowersShownInPercent_ = false;
				bHarm_I_ShownInPercent_ = true;

				dgWrapperUIF_ = new AVGDataGridWrapperUIF(ref settings_, curDevType_, ref dgCurrentsVoltages,
					curDatetimeId_, curConnectionScheme_, iLimit_, this);
				dgWrapperPower_ = new AVGDataGridWrapperPower(ref settings_, curDevType_, ref dgPowers,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperPQP_ = new AVGDataGridWrapperPQP(ref settings_, curDevType_, ref dgPQP,
					curDatetimeId_, curConnectionScheme_, iLimit_, this);
				dgWrapperAngles_ = new AVGDataGridWrapperAngles(ref settings_, curDevType_, ref dgPhAngles,
					curDatetimeId_, curConnectionScheme_, this);

				dgWrapperHarmI_ = new AVGDataGridWrapperHarmI(ref settings_, curDevType_,
					ref dgCurrentHarmonics,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmUph_ = new AVGDataGridWrapperHarmUph(ref settings_, curDevType_, ref dgVolPhHarmonics,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmUlin_ = new AVGDataGridWrapperHarmUlin(ref settings_, curDevType_, ref dgVolLinHarmonics,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmPower_ = new AVGDataGridWrapperHarmPower(ref settings_, curDevType_, ref dgHarmonicPowers,
					curDatetimeId_, curConnectionScheme_, this);

				dgWrapperInterHarmI_ = new AVGDataGridWrapperInterHarmI(ref settings_, curDevType_, ref dgI_Interharm,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperInterHarmUph_ = new AVGDataGridWrapperInterHarmUph(ref settings_, curDevType_,
					ref dgUph_Interharm, curDatetimeId_, curConnectionScheme_, this);
				dgWrapperInterHarmUlin_ = new AVGDataGridWrapperInterHarmUlin(ref settings_, curDevType_,
					ref dgUlin_Interharm, curDatetimeId_, curConnectionScheme_, this);

				bool[] Measures = initializeExistingMeasures();
				if (Measures == null) return;

				try
				{
					tabControl1.TabPages.Clear();
				}
				catch { }

				// метрологи решили, что для этих двух приборов мощности гармоник надо убрать
				if (Measures[(int)AvgPages.HARMONIC_POWERS] && curDevType_ != EmDeviceType.ETPQP &&
					curDevType_ != EmDeviceType.ETPQP_A)
					//&& (curDevType_ != EmDeviceType.ETPQP_A || iLimit_ != 0))
				{
					// preparing data grids...
					dgWrapperHarmPower_.load(curWithTR_, bHarmPowersShownInPercent_);
					dgWrapperHarmPower_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicPowers))
						this.tabControl1.TabPages.Insert(0, this.tabHarmonicPowers);
				}
				else
				{
					dgWrapperHarmPower_.unload();
				}

				if (Measures[(int)AvgPages.U_LIN_INTERHARM] &&
					(curConnectionScheme_ == ConnectScheme.Ph3W3 ||
					curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc))
				{
					// preparing data grids...
					dgWrapperInterHarmUlin_.load(curWithTR_, true /*not used*/);
					dgWrapperInterHarmUlin_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabInterHarmUlin))
						this.tabControl1.TabPages.Insert(0, this.tabInterHarmUlin);
				}
				else
				{
					dgWrapperInterHarmUlin_.unload();
				}

				if (Measures[(int)AvgPages.U_LIN_HARMONICS] &&
					(curConnectionScheme_ == ConnectScheme.Ph3W3 ||
					curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc))
				{
					bool show = false;
					if ((curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K
						|| curDevType_ == EmDeviceType.EM33T1 || curDevType_ == EmDeviceType.ETPQP_A))
					{
						if (curConnectionScheme_ == ConnectScheme.Ph3W3 ||
						curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
						{
							show = true;
						}
					}
					else
					{
						if (curConnectionScheme_ != ConnectScheme.Ph1W2)
							show = true;
					}
					if (show)
					{
						// preparing data grids...
						dgWrapperHarmUlin_.load(curWithTR_, true /*not used*/);
						dgWrapperHarmUlin_.init();
						// and showing it
						if (!this.tabControl1.TabPages.Contains(tabHarmonicVoltageLin))
							this.tabControl1.TabPages.Insert(0, this.tabHarmonicVoltageLin);
					}
					else
					{
						dgWrapperHarmUlin_.unload();
					}
				}
				else
				{
					dgWrapperHarmUlin_.unload();
				}

				if (Measures[(int)AvgPages.U_PH_INTERHARM] &&
					(curConnectionScheme_ != ConnectScheme.Ph3W3 &&
					curConnectionScheme_ != ConnectScheme.Ph3W3_B_calc))
				{
					// preparing data grids...
					dgWrapperInterHarmUph_.load(curWithTR_, true /*not used*/);
					dgWrapperInterHarmUph_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabInterHarmUph))
						this.tabControl1.TabPages.Insert(0, this.tabInterHarmUph);
				}
				else
				{
					dgWrapperInterHarmUph_.unload();
				}

				if (Measures[(int)AvgPages.U_PH_HARMONICS] &&
					(curConnectionScheme_ != ConnectScheme.Ph3W3 &&
					curConnectionScheme_ != ConnectScheme.Ph3W3_B_calc))
				{
					// preparing data grids...
					dgWrapperHarmUph_.load(curWithTR_, true /*not used*/);
					dgWrapperHarmUph_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicVoltagePh))
						this.tabControl1.TabPages.Insert(0, this.tabHarmonicVoltagePh);
				}
				else
				{
					dgWrapperHarmUph_.unload();
				}

				if (Measures[(int)AvgPages.I_INTERHARM] && iLimit_ != 0)
				{
					// preparing data grids...
					dgWrapperInterHarmI_.load(curWithTR_, true /*not used*/);
					dgWrapperInterHarmI_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabInterHarmI))
						this.tabControl1.TabPages.Insert(0, this.tabInterHarmI);
				}
				else
				{
					dgWrapperInterHarmI_.unload();
				}

				if (Measures[(int)AvgPages.I_HARMONICS] &&
					(curDevType_ != EmDeviceType.ETPQP_A || iLimit_ != 0))
				{
					// preparing data grids...
					dgWrapperHarmI_.load(curWithTR_, bHarm_I_ShownInPercent_);
					dgWrapperHarmI_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicI))
						this.tabControl1.TabPages.Insert(0, this.tabHarmonicI);
				}
				else
				{
					dgWrapperHarmI_.unload();
				}

				if (Measures[(int)AvgPages.ANGLES])
				{
					// preparing data grids...
					dgWrapperAngles_.load(curWithTR_, true /*not used*/);
					dgWrapperAngles_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabAngles))
						this.tabControl1.TabPages.Insert(0, tabAngles);
				}

				if (Measures[(int)AvgPages.PQP])
				{
					// preparing data grids...
					dgWrapperPQP_.load(curWithTR_, true /*not used*/);
					dgWrapperPQP_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabPQP))
						this.tabControl1.TabPages.Insert(0, tabPQP);
				}

				if (Measures[(int)AvgPages.POWER] &&
					(curDevType_ != EmDeviceType.ETPQP_A || iLimit_ != 0))
				{
					// preparing data grids...
					dgWrapperPower_.load(curWithTR_, true /*not used*/);
					dgWrapperPower_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabPowers))
						this.tabControl1.TabPages.Insert(0, tabPowers);
				}

				if (Measures[(int)AvgPages.F_U_I])
				{
					// preparing data grids...
					dgWrapperUIF_.load(curWithTR_, true /*not used*/);
					dgWrapperUIF_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabCurrentsAndVoltages))
						this.tabControl1.TabPages.Insert(0, this.tabCurrentsAndVoltages);
				}

				this.tabControl1.SelectedIndex = 0;
				this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.Customize();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in IninializeDoc2Grids(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// External function to prepare <c>wndDoc2</c> data grids
		/// </summary>
		// <param name="PgServerIndex">PostgreSQL server index</param>
		// <param name="WithTR">Open With Transform Ratio or Not</param>
		public void IninializeDoc2Grids(int pgServerIndex, Int64 datetimeID, ConnectScheme connectionScheme,
										bool withTR, EmDeviceType devType)
		{
			try
			{
				bHarmPowersShownInPercent_ = false;
				bHarm_I_ShownInPercent_ = true;

				curPgServerIndex_ = pgServerIndex;
				curDatetimeId_ = datetimeID;
				curConnectionScheme_ = connectionScheme;
				curWithTR_ = withTR;
				curDevType_ = devType;

				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					AvgTypes avgType = ((this.MainWindow_.wndToolbox.ActiveNodeAVG as
										EmTreeNodeDBMeasureClassic).AvgType);
					if (avgType == AvgTypes.ThreeSec) period_in_secs_ = 3;
					else if (avgType == AvgTypes.OneMin) period_in_secs_ = 60;
					else if (avgType == AvgTypes.ThirtyMin) period_in_secs_ = 1800;
				}
				else
				{
					AvgTypes_PQP_A avgType = ((this.MainWindow_.wndToolbox.ActiveNodeAVG as
										EmTreeNodeDBMeasureEtPQP_A).AvgType_PQP_A);
					if (avgType == AvgTypes_PQP_A.ThreeSec) period_in_secs_ = 3;
					else if (avgType == AvgTypes_PQP_A.TenMin) period_in_secs_ = 600;
					else if (avgType == AvgTypes_PQP_A.TwoHours) period_in_secs_ = 7200;
				}

				MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurDevType = devType;
				MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurPeriodInSecs = period_in_secs_;

				dgWrapperUIF_ = new AVGDataGridWrapperUIF(ref settings_, curDevType_, ref dgCurrentsVoltages,
					curDatetimeId_, curConnectionScheme_, iLimit_, this);
				dgWrapperPower_ = new AVGDataGridWrapperPower(ref settings_, curDevType_, ref dgPowers,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperPQP_ = new AVGDataGridWrapperPQP(ref settings_, curDevType_, ref dgPQP,
					curDatetimeId_, curConnectionScheme_, iLimit_, this);
				dgWrapperAngles_ = new AVGDataGridWrapperAngles(ref settings_, curDevType_, ref dgPhAngles,
					curDatetimeId_, curConnectionScheme_, this);

				dgWrapperHarmI_ = new AVGDataGridWrapperHarmI(ref settings_, curDevType_,
					ref dgCurrentHarmonics,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmUph_ = new AVGDataGridWrapperHarmUph(ref settings_, curDevType_, ref dgVolPhHarmonics,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmUlin_ = new AVGDataGridWrapperHarmUlin(ref settings_, curDevType_, ref dgVolLinHarmonics,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmPower_ = new AVGDataGridWrapperHarmPower(ref settings_, curDevType_, ref dgHarmonicPowers,
					curDatetimeId_, curConnectionScheme_, this);
				dgWrapperHarmAngles_ = new AVGDataGridWrapperHarmAngles(ref settings_, curDevType_, ref dgHarmonicAngles,
					curDatetimeId_, curConnectionScheme_, this);

				bool[] Measures = initializeExistingMeasures();
				if (Measures == null) return;

				try
				{
					tabControl1.TabPages.Clear();
				}
				catch { }

				// if data for the seventh data grids exists 
				if (Measures[(int)AvgPages.HARMONIC_ANGLES] &&
					(connectionScheme != ConnectScheme.Ph3W3 &&
					connectionScheme != ConnectScheme.Ph3W3_B_calc))
				{
					// preparing data grids...
					dgWrapperHarmAngles_.load(curWithTR_, true /*not used*/);
					dgWrapperHarmAngles_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicAngles))
						this.tabControl1.TabPages.Insert(0, tabHarmonicAngles);
				}
				else
				{
					dgWrapperHarmAngles_.unload();
				}

				// метрологи решили, что для этих двух приборов мощности гармоник надо убрать
				if (Measures[(int)AvgPages.HARMONIC_POWERS] && curDevType_ != EmDeviceType.ETPQP &&
					curDevType_ != EmDeviceType.ETPQP_A)
				{
					// preparing data grids...
					dgWrapperHarmPower_.load(curWithTR_, bHarmPowersShownInPercent_);
					dgWrapperHarmPower_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicPowers))
						this.tabControl1.TabPages.Insert(0, this.tabHarmonicPowers);
				}
				else
				{
					dgWrapperHarmPower_.unload();
				}

				if (Measures[(int)AvgPages.U_LIN_HARMONICS] &&
					(connectionScheme == ConnectScheme.Ph3W3 ||
					connectionScheme == ConnectScheme.Ph3W3_B_calc))
				{
					bool show = false;
					if ((curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K
						|| curDevType_ == EmDeviceType.EM33T1 || curDevType_ == EmDeviceType.ETPQP_A))
					{
						if (curConnectionScheme_ == ConnectScheme.Ph3W3 ||
						curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
						{
							show = true;
						}
					}
					else
					{
						if (curConnectionScheme_ != ConnectScheme.Ph1W2)
							show = true;
					}
					if (show)
					{
						// preparing data grids...
						dgWrapperHarmUlin_.load(curWithTR_, true /*not used*/);
						dgWrapperHarmUlin_.init();
						// and showing it
						if (!this.tabControl1.TabPages.Contains(tabHarmonicVoltageLin))
							this.tabControl1.TabPages.Insert(0, this.tabHarmonicVoltageLin);
					}
					else
					{
						dgWrapperHarmUlin_.unload();
					}
				}
				else
				{
					dgWrapperHarmUlin_.unload();
				}

				if (Measures[(int)AvgPages.U_PH_HARMONICS] &&
					(connectionScheme != ConnectScheme.Ph3W3 &&
					connectionScheme != ConnectScheme.Ph3W3_B_calc))
				{
					// preparing data grids...
					dgWrapperHarmUph_.load(curWithTR_, true /*not used*/);
					dgWrapperHarmUph_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicVoltagePh))
						this.tabControl1.TabPages.Insert(0, this.tabHarmonicVoltagePh);
				}
				else
				{
					dgWrapperHarmUph_.unload();
				}

				if (Measures[(int)AvgPages.I_HARMONICS])
				{
					// preparing data grids...
					dgWrapperHarmI_.load(curWithTR_, bHarm_I_ShownInPercent_);
					dgWrapperHarmI_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabHarmonicI))
						this.tabControl1.TabPages.Insert(0, this.tabHarmonicI);
				}
				else
				{
					dgWrapperHarmI_.unload();
				}

				if (Measures[(int)AvgPages.ANGLES])
				{
					// preparing data grids...
					dgWrapperAngles_.load(curWithTR_, true /*not used*/);
					dgWrapperAngles_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabAngles))
						this.tabControl1.TabPages.Insert(0, tabAngles);
				}

				if (Measures[(int)AvgPages.PQP])
				{
					// preparing data grids...
					dgWrapperPQP_.load(curWithTR_, true /*not used*/);
					dgWrapperPQP_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabPQP))
						this.tabControl1.TabPages.Insert(0, tabPQP);
				}

				if (Measures[(int)AvgPages.POWER])
				{
					// preparing data grids...
					dgWrapperPower_.load(curWithTR_, true /*not used*/);
					dgWrapperPower_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabPowers))
						this.tabControl1.TabPages.Insert(0, tabPowers);
				}

				if (Measures[(int)AvgPages.F_U_I])
				{
					// preparing data grids...
					dgWrapperUIF_.load(curWithTR_, true /*not used*/);
					dgWrapperUIF_.init();
					// and showing it
					if (!this.tabControl1.TabPages.Contains(tabCurrentsAndVoltages))
						this.tabControl1.TabPages.Insert(0, this.tabCurrentsAndVoltages);
				}

				this.tabControl1.SelectedIndex = 0;
				this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.Customize();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in IninializeDoc2Grids(): ");
				throw;
			}
		}

		public void ClearGraphParamList()
		{
			listGraphBottomAdded_.Clear();
		}

		#endregion

		#region Currents and Voltages, Powers, Angles, PQP data grids

        private void dataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!settings_.ShowAvgTooltip) return;

                DataGrid dg = (DataGrid)sender;
                //DataTable dt = ((DataSet)dg.DataSource).Tables[0];
                DataGrid.HitTestInfo hitInfo = dg.HitTest(e.X, e.Y);
                if (hitInfo.Type == DataGrid.HitTestType.ColumnHeader)
                {
                    if (!ttAVG.Active)
                    {
                        int columnNum = hitInfo.Column;
                        DataGridTableStyle curDgStyle = dg.TableStyles[0];
                        string strToolTip = GetAVGParamName(
                            curDgStyle.GridColumnStyles[columnNum].MappingName);

                        if (strToolTip != null && strToolTip.Length > 0)
                        {
                            ttAVG.SetToolTip(dg, strToolTip);
                            ttAVG.Active = true;
                        }
                    }
                }
                else
                {
                    ttAVG.Active = false;
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in dataGrid_MouseMove() ");
                //throw;
            }
        }

		private void dataGrid_MouseDown(object sender, MouseEventArgs e)
		{
			try
			{
				DataGrid dg = (DataGrid)sender;
				DataTable dt = ((DataSet)dg.DataSource).Tables[0];

				GetCurrentDataGridWrapper(ref dg).AdjustGridForData(dt);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in dataGrid_MouseDown() ");
				throw;
			}
		}

		#endregion

		#region Working with GraphPanel (right)

		internal void currencyManager_PositionChanged(object sender, EventArgs e)
		{
			try
			{
				foreach (CurrencyManager cm in CMs_)
				{
					if (cm != null)
					{
						if (!cm.Equals(sender))
						{
							// disabling event handler
							cm.PositionChanged -= new EventHandler(currencyManager_PositionChanged);
							// changing position
							cm.Position = (sender as CurrencyManager).Position;
							// enabling event handler
							cm.PositionChanged += new EventHandler(currencyManager_PositionChanged);
						}
						else
						{
							//MessageBox.Show((cm.Current as DataRowView).Row.Table.ToString());
							this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.UpdateGraphPanel(
									bHarmPowersShownInPercent_,
									bHarm_I_ShownInPercent_);
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in currencyManager_PositionChanged(): " + ex.Message);
				throw;
			}
		}


		#endregion

		#region Working with GraphBox (bottom)

		private void frmDocAVGMain_Load(object sender, EventArgs e)
		{
			tabControl1_SelectedIndexChanged(sender, new EventArgs());
		}

		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			// это поле используется для разделения графиков, а это делается только на двух страницах,
			// поэтому остальные гриды не передаем
			if (tabControl1.TabPages[tabControl1.SelectedIndex].Name.Equals("tabCurrentsAndVoltages"))
				MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurDataGrid = DgUIF;
			else if (tabControl1.TabPages[tabControl1.SelectedIndex].Name.Equals("tabPQP"))
				MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurDataGrid = DgPQP;
			else MainWindow_.wndDocAVG.wndDocAVGGraphBottom.CurDataGrid = null;
		}

		private void cmsAddToGraphLeft_Click(object sender, EventArgs e)
		{
			try
			{
				AddCurver((DataGrid)((cmsDoc2.Tag as object[])[0]),
							Convert.ToInt32((cmsDoc2.Tag as object[])[1]), false);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in cmsAddToGraphLeft_Click(): ");
				throw;
			}
		}

		private void cmsAddToGraphRight_Click(object sender, EventArgs e)
		{
			try
			{
				AddCurver((DataGrid)((cmsDoc2.Tag as object[])[0]),
							Convert.ToInt32((cmsDoc2.Tag as object[])[1]), true);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in cmsAddToGraphRight_Click(): ");
				throw;
			}
		}

		/// <summary>Adding curver to the graph window</summary>
		/// <param name="dg">Datagrid object</param>
		/// <param name="column">column number</param>
		/// <param name="Y2Axis">is it must be added to the Y2 axis</param>
		private void AddCurver(DataGrid dg, int column, bool Y2Axis)
		{
			try
			{
				if (dg == null) return;
				if (dg.DataSource == null) return;

				// проверяем не был ли этот график уже добавлен. если был, то выдаем сообщение
				int indexGraph = listGraphBottomAdded_.FindIndex(x => x == new Pair<string, int>(dg.Name, column));
				if (indexGraph == -1)
				{
					listGraphBottomAdded_.Add(new Pair<string, int>(dg.Name, column));
				}
				else
				{
					MessageBoxes.MsgParamWasAlreadyAdded(this);
					return;
				}

				frmDocAVGGraphBottom wndDocAVGGraph = this.MainWindow_.wndDocAVG.wndDocAVGGraphBottom;

				Color curveColor = Color.Empty;
				// if random colors switch is on
				if (this.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.tsUseRandomColors.Checked)
				{
					// if auto-colors not all bineded
					if (wndDocAVGGraph.GraphColors.AllColorsCount > wndDocAVGGraph.GraphColors.BindedColorsCount)
					{
						// getting color from inner array
						curveColor = wndDocAVGGraph.GraphColors.BindColor();
					}
					else
					{
						// showing color dialog window
						ColorDialog wndChooseColor = new ColorDialog();
						wndChooseColor.FullOpen = true;
						wndChooseColor.AnyColor = true;
						// if user didn't click "ok" button returning
						if (wndChooseColor.ShowDialog() != DialogResult.OK) return;
						curveColor = wndChooseColor.Color;
					}
				}
				else // if we are not trying to get auto-color
				{
					// see before...
					ColorDialog wndChooseColor = new ColorDialog();
					wndChooseColor.FullOpen = true;
					wndChooseColor.AnyColor = true;
					if (wndChooseColor.ShowDialog() != DialogResult.OK) return;
					curveColor = wndChooseColor.Color;
				}

				// saving sort state and resorting to build graph correctly
				string SortRule = GetCurrentSortRule(dg);
				SortByColumn(dg, 0, false);

				// starting build point list
				GraphPane gPane = wndDocAVGGraph.zedGraph.GraphPane;
				List<PointPairList> lists = new List<PointPairList>();
				lists.Add(new PointPairList());
				ushort listIndex = 0;

				//DateTime lastDate = DateTime.MinValue;
				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					try
					{
						double y_start = GetNumberFromDataGrid(dg, 0, column);
						double x0_start = (double)new XDate(Convert.ToDateTime(dg[0, 0]).AddSeconds(-period_in_secs_));
						double x1_start = (double)new XDate(Convert.ToDateTime(dg[0, 0]));
						lists[listIndex].Add(x0_start, y_start);
						lists[listIndex].Add(x1_start, y_start);
						//lastDate = Convert.ToDateTime(dg[0, 0]);
					}
					catch { }
					for (int iRow = 0; iRow < (dg.DataSource as DataSet).Tables[0].Rows.Count - 1; iRow++)
					{
						try
						{
							//bool permanent = false;
							//if (lastDate != DateTime.MinValue)
							//    permanent = (Convert.ToDateTime(dg[iRow + 1, 0]) - lastDate).TotalSeconds <= (period_in_secs + 6);

							double y = GetNumberFromDataGrid(dg, iRow + 1, column);
							double x0 = (double)new XDate(Convert.ToDateTime(dg[iRow, 0]));
							double x1 = (double)new XDate(Convert.ToDateTime(dg[iRow + 1, 0]));
							bool permanent = (Convert.ToDateTime(dg[iRow + 1, 0]) - 
								Convert.ToDateTime(dg[iRow, 0])).TotalSeconds <= (period_in_secs_ + 9);

							lists[listIndex].Add(x0, y);
							if (!permanent)
							{
								lists.Add(new PointPairList());
								listIndex++;
							}
							else lists[listIndex].Add(x1, y);
							
							//lastDate = Convert.ToDateTime(dg[iRow + 1, 0]);
						}
						catch (FormatException) { }
					}
				}
				else
				{
					for (int iRow = 0; iRow < (dg.DataSource as DataSet).Tables[0].Rows.Count - 1; iRow++)
					{
						try
						{
							// проверяем расстояние между соседними датами. если больше заданного, то обрываем старый
							// график и начинаем новый
							//bool permanent = false;
							//if(lastDate != DateTime.MinValue)
							//    permanent = (Convert.ToDateTime(dg[iRow + 1, 0]) - lastDate).TotalSeconds <= (period_in_secs + 6);

							double y0 = GetNumberFromDataGrid(dg, iRow, column);					
							double x0 = (double)new XDate(Convert.ToDateTime(dg[iRow, 0]));
							double x1 = (double)new XDate(Convert.ToDateTime(dg[iRow + 1, 0]));

							bool permanent = (Convert.ToDateTime(dg[iRow + 1, 0]) - 
								Convert.ToDateTime(dg[iRow, 0])).TotalSeconds <= (period_in_secs_ + 9);

							lists[listIndex].Add(x0, y0);
							// если между датами большой разрыв, то обрываем старый
							// график и начинаем новый.
							if (!permanent)
							{
								lists.Add(new PointPairList());
								listIndex++;
							}
							else
								lists[listIndex].Add(x1, y0);
							
							//lastDate = Convert.ToDateTime(dg[iRow + 1, 0]);
						}
						catch (FormatException) { }
					}

				}
				// restoring sorting rule
				SetSortRule(dg, SortRule);

				DataGridColumnStyle cs = dg.TableStyles[0].GridColumnStyles[column];
				string legend = cs.HeaderText;
				if (cs is DataGridColumnHeaderFormula)
				{
					if ((cs as DataGridColumnHeaderFormula).HeaderIsFormula)
						legend = (cs as DataGridColumnHeaderFormula).HeaderFormula;
				}
				legend += Y2Axis ? " (R)" : " (L)";

				for (int iCurList = 0; iCurList < lists.Count; ++iCurList)
				{
					CurveItem myCurve = gPane.AddCurve(legend, lists[iCurList], curveColor, SymbolType.None);
					if (Y2Axis) myCurve.IsY2Axis = true;
				}

				//gPane.AxisChange(this.CreateGraphics());

				// Axis X, Y and Y2
				gPane.XAxis.IsVisible = wndDocAVGGraph.tsXGridLine.Checked;
				gPane.XAxis.IsShowGrid = wndDocAVGGraph.tsXGridLine.Checked;
				gPane.XAxis.IsShowMinorGrid = wndDocAVGGraph.tsXMinorGridLine.Checked;

				gPane.YAxis.IsVisible = wndDocAVGGraph.tsYGridLine.Checked;
				gPane.YAxis.IsShowGrid = wndDocAVGGraph.tsYGridLine.Checked;
				gPane.YAxis.IsShowMinorGrid = wndDocAVGGraph.tsYMinorGridLine.Checked;

				gPane.Y2Axis.IsVisible = wndDocAVGGraph.tsY2GridLine.Checked;
				gPane.Y2Axis.IsShowGrid = wndDocAVGGraph.tsY2GridLine.Checked;
				gPane.Y2Axis.IsShowMinorGrid = wndDocAVGGraph.tsY2MinorGridLine.Checked;

				// Zoom set defalult
				Graphics g = wndDocAVGGraph.zedGraph.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				wndDocAVGGraph.zedGraph.Refresh();

				// drawing delete button
				Bitmap img = new Bitmap(16, 16);
				Color tlColor = Color.FromArgb(0x40, curveColor);
				GraphicsUnit gu = GraphicsUnit.Display;
				RectangleF rectFull = img.GetBounds(ref gu);
				RectangleF rectSmall = new RectangleF(rectFull.Location, rectFull.Size);
				int canvas = 3;
				rectSmall.X += canvas;
				rectSmall.Y += canvas;
				rectSmall.Height -= 2 * canvas;
				rectSmall.Width -= 2 * canvas;
				Brush brush = new LinearGradientBrush(rectSmall, tlColor, curveColor, 45);
				Pen pen = new Pen(curveColor);
				g = Graphics.FromImage(img);
				g.FillRectangle(brush, rectSmall);
				g.DrawRectangle(pen, rectFull.X, rectFull.Y, rectFull.Width - 1, rectFull.Height - 1);

				ToolStripItem newItem = wndDocAVGGraph.zedToolStrip.Items.Add(img);
				newItem.ToolTipText = legend;
				newItem.Tag = dg.Name + '|' + column.ToString();
				newItem.Click += new EventHandler(newItem_Click);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddCurver():");
				throw;
			}
		}

		// эта функция нужна из-за того, что для Kp используется не числовое поле, а текстовое
		// и в значении на конце L или C, поэтому напрямую конвертировать в число нельзя
		private double GetNumberFromDataGrid(DataGrid dg, int row, int col)
		{
			try
			{
				return Convert.ToSingle(dg[row, col]);
			}
			catch
			{
				string num = dg[row, col].ToString();
				if (!Char.IsDigit(num[num.Length - 1]))
					num = num.Substring(0, num.Length - 1);
				try
				{
					System.Globalization.CultureInfo cltr =
						new System.Globalization.CultureInfo("en-US");
					return Convert.ToSingle(num, cltr);
				}
				catch
				{
					System.Globalization.CultureInfo cltr =
						new System.Globalization.CultureInfo("ru-RU");
					return Convert.ToSingle(num, cltr);
				}
			}
		}

		/// <summary>
		/// Deliting curve from the graph
		/// </summary>
		/// <param name="sender"><c>ToolBoxItem</c> object</param>
		/// <param name="e">Events arguments</param>
		void newItem_Click(object sender, EventArgs e)
		{
			try
			{
				frmDocAVGGraphBottom wndDocAVGGraph = this.MainWindow_.wndDocAVG.wndDocAVGGraphBottom;

				// releasing color
				this.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.GraphColors.ReleaseColor(this.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.zedGraph.GraphPane.CurveList[(sender as ToolStripItem).ToolTipText].Color);

				// deleting curve
				GraphPane gPane = wndDocAVGGraph.zedGraph.GraphPane;

				while (gPane.CurveList.IndexOf(gPane.CurveList[(sender as ToolStripItem).ToolTipText]) != -1)
				{
					gPane.CurveList.Remove(gPane.CurveList[(sender as ToolStripItem).ToolTipText]);
				}

				// Zoom set defalult
				Graphics g = wndDocAVGGraph.zedGraph.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				wndDocAVGGraph.zedGraph.Refresh();

				// disposing button
				wndDocAVGGraph.zedToolStrip.Items.Remove((sender as ToolStripItem));

				// убираем из списка графиков (который является защитой от дублирования)
				string dgName = string.Empty;
				int column = -1;
				try
				{
					dgName = (sender as ToolStripItem).Tag.ToString();
					column = Int32.Parse(dgName.Substring(dgName.IndexOf('|') + 1));
					dgName = dgName.Substring(0, dgName.IndexOf('|'));
					int indexGraph = listGraphBottomAdded_.FindIndex(x => x == new Pair<string, int>(dgName, column));
					if(indexGraph >= 0) listGraphBottomAdded_.RemoveAt(indexGraph);
				}
				catch (Exception exx)
				{
					EmService.WriteToLogFailed("newItem_Click: " + exx.Message);
					EmService.WriteToLogFailed(dgName + "   " + column);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in newItem_Click(): ");
				throw;
			}
		}

		#endregion

		#region Context menu

		private void miHarmPowersPercent_Click(object sender, EventArgs e)
		{
			try
			{
				bHarmPowersShownInPercent_ = true;
				dgWrapperHarmPower_.load(curWithTR_, bHarmPowersShownInPercent_);
				dgWrapperHarmPower_.RenameHarmonicPowersColumns(bHarmPowersShownInPercent_);

				if (curDevType_ != EmDeviceType.ETPQP_A)
					this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.UpdateGraphPanel(
									bHarmPowersShownInPercent_,
									bHarm_I_ShownInPercent_);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in miHarmPowersPercent_Click():");
				throw;
			}
		}

		private void miHarmPowersWatt_Click(object sender, EventArgs e)
		{
			try
			{
				bHarmPowersShownInPercent_ = false;
				dgWrapperHarmPower_.load(curWithTR_, bHarmPowersShownInPercent_);
				dgWrapperHarmPower_.RenameHarmonicPowersColumns(bHarmPowersShownInPercent_);

				if(curDevType_ != EmDeviceType.ETPQP_A)
					this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.UpdateGraphPanel(
									bHarmPowersShownInPercent_,
									bHarm_I_ShownInPercent_);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in miHarmPowersWatt_Click():");
				throw;
			}
		}

		private void miHarmCurPercent_Click(object sender, EventArgs e)
		{
			try
			{
				bHarm_I_ShownInPercent_ = true;
				dgWrapperHarmI_.load(curWithTR_, bHarm_I_ShownInPercent_);
				dgWrapperHarmI_.RenameHarmonicCurrentColumns(bHarm_I_ShownInPercent_);

				if (curDevType_ != EmDeviceType.ETPQP_A)
					this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.UpdateGraphPanel(
									bHarmPowersShownInPercent_,
									bHarm_I_ShownInPercent_);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in miHarmCurPercent_Click():");
				throw;
			}
		}

		private void miHarmCurAmpere_Click(object sender, EventArgs e)
		{
			try
			{
				bHarm_I_ShownInPercent_ = false;
				dgWrapperHarmI_.load(curWithTR_, bHarm_I_ShownInPercent_);
				dgWrapperHarmI_.RenameHarmonicCurrentColumns(bHarm_I_ShownInPercent_);

				if (curDevType_ != EmDeviceType.ETPQP_A)
					this.MainWindow_.wndDocAVG.wndDocAVGGraphRight.UpdateGraphPanel(
									bHarmPowersShownInPercent_,
									bHarm_I_ShownInPercent_);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in miHarmCurAmpere_Click():");
				throw;
			}
		}

		/// <summary>
		/// Showing context menu
		/// </summary>
		/// <param name="sender">Datagrid sending the message</param>
		/// <param name="e">Mouse event arguments</param>
		private void dataGrid_MouseUp(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Right)
				{
					DataGrid.HitTestInfo info = (sender as DataGrid).HitTest(e.X, e.Y);
					if (info.Type == DataGrid.HitTestType.ColumnHeader)
					{
						if (info.Column == 0)	// для времени графики не строим
						{
							miSeparator.Visible = false;
							miAddColumnToGraphLeft.Visible = false;
							miAddColumnToGraphRight.Visible = false;
						}
						else if (info.Column == 1 && curDevType_ == EmDeviceType.ETPQP_A)	// для маркированности тоже не строим
						{
							miSeparator.Visible = false;
							miAddColumnToGraphLeft.Visible = false;
							miAddColumnToGraphRight.Visible = false;
						}
						else
						{
							miSeparator.Visible = true;
							miAddColumnToGraphLeft.Visible = true;
							miAddColumnToGraphRight.Visible = true;
						}

						if ((sender as DataGrid).Name == "dgHarmonicPowers")
						{
							miSeparator2.Visible = true;
							if (bHarmPowersShownInPercent_)
							{
								miHarmPowersWatt.Visible = true;
								miHarmPowersPercent.Visible = false;
							}
							else
							{
								miHarmPowersPercent.Visible = true;
								miHarmPowersWatt.Visible = false;
							}
						}
						else
						{
							miSeparator2.Visible = false;
							miHarmPowersPercent.Visible = false;
							miHarmPowersWatt.Visible = false;
						}

						if ((sender as DataGrid).Name == "dgCurrentHarmonics" && curDevType_ != EmDeviceType.ETPQP_A)
						{
							miSeparator2.Visible = true;
							if (bHarm_I_ShownInPercent_)
							{
								miHarmCurAmpere.Visible = true;
								miHarmCurPercent.Visible = false;
							}
							else
							{
								miHarmCurPercent.Visible = true;
								miHarmCurAmpere.Visible = false;
							}
						}
						else
						{
							miSeparator2.Visible = false;
							miHarmCurPercent.Visible = false;
							miHarmCurAmpere.Visible = false;
						}

						cmsDoc2.Show((sender as DataGrid), new Point(e.X, e.Y));
						cmsDoc2.Tag = new object[] { sender, info.Column };
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in dataGrid_MouseUp():");
				throw;
			}
		}

		#endregion

		#region Sorting

		private void miSort_Click(object sender, EventArgs e)
		{
			try
			{
				SortByColumn((DataGrid)((cmsDoc2.Tag as object[])[0]),
							Convert.ToInt32((cmsDoc2.Tag as object[])[1]), false);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in miSort_Click(): " + ex.Message);
				throw;
			}
		}

		private void miSortDesc_Click(object sender, EventArgs e)
		{
			try
			{
				SortByColumn((DataGrid)((cmsDoc2.Tag as object[])[0]),
							Convert.ToInt32((cmsDoc2.Tag as object[])[1]), true);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in miSortDesc_Click(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#region Properties

		public ConnectScheme CurConnectionScheme
		{
			get { return curConnectionScheme_; }
		}

		/// <summary>U, I, F</summary>
		public DataGrid DgUIF
		{
			get { return dgWrapperUIF_.DGrid; }
		}

		public DataGrid DgPower
		{
			get { return dgWrapperPower_.DGrid; }
		}

		public DataGrid DgPQP
		{
			get { return dgWrapperPQP_.DGrid; }
		}

		public DataGrid DgAngles
		{
			get { return dgWrapperAngles_.DGrid; }
		}

		public DataGrid DgHarmI
		{
			get { return dgWrapperHarmI_.DGrid; }
		}

		public DataGrid DgHarmUph
		{
			get { return dgWrapperHarmUph_.DGrid; }
		}

		public DataGrid DgHarmUlin
		{
			get { return dgWrapperHarmUlin_.DGrid; }
		}

		public DataGrid DgHarmPower
		{
			get { return dgWrapperHarmPower_.DGrid; }
		}

		public DataGrid DgHarmAngles
		{
			get { return dgWrapperHarmAngles_.DGrid; }
		}

		public DataGrid DgInterHarmI
		{
			get { return dgWrapperInterHarmI_.DGrid; }
		}

		public DataGrid DgInterHarmUlin
		{
			get { return dgWrapperInterHarmUlin_.DGrid; }
		}

		public DataGrid DgInterHarmUph
		{
			get { return dgWrapperInterHarmUph_.DGrid; }
		}

		#endregion

		#region Service methods

		/// <summary>
		/// Test database for existing records in period_avg_params_XXX with known primary key
		/// </summary>
		/// <returns>array of bool values if all ok or null</returns>
		private bool[] initializeExistingMeasures()
		{
			bool[] Measures = new bool[12];

			DbService dbService = new DbService(
				AVGDataGridWrapperBase.GetPgConnectionString(curDevType_, curPgServerIndex_, ref settings_));
			if (!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return null;
			}
			try
			{
				if (curDevType_ == EmDeviceType.ETPQP_A)
				{
					#region EtPQP-A

					Int64 res = dbService.ExecuteScalarInt64(string.Format(
						 "SELECT count(*) FROM avg_u_i_f WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.F_U_I] = true;
					}
					else
					{
						Measures[(int)AvgPages.F_U_I] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						 "SELECT count(*) FROM avg_power WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.POWER] = true;
					}
					else
					{
						Measures[(int)AvgPages.POWER] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						 "SELECT count(*) FROM avg_pqp WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.PQP] = true;
					}
					else
					{
						Measures[(int)AvgPages.PQP] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_angles WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.ANGLES] = true;
					}
					else
					{
						Measures[(int)AvgPages.ANGLES] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_harm_power WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.HARMONIC_POWERS] = true;
					}
					else
					{
						Measures[(int)AvgPages.HARMONIC_POWERS] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_i_harmonics WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.I_HARMONICS] = true;
					}
					else
					{
						Measures[(int)AvgPages.I_HARMONICS] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_i_interharmonics WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.I_INTERHARM] = true;
					}
					else
					{
						Measures[(int)AvgPages.I_INTERHARM] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_u_lin_harmonics WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.U_LIN_HARMONICS] = true;
					}
					else
					{
						Measures[(int)AvgPages.U_LIN_HARMONICS] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_u_lin_interharmonics WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.U_LIN_INTERHARM] = true;
					}
					else
					{
						Measures[(int)AvgPages.U_LIN_INTERHARM] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_u_ph_interharmonics WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.U_PH_INTERHARM] = true;
					}
					else
					{
						Measures[(int)AvgPages.U_PH_INTERHARM] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM avg_u_phase_harmonics WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.U_PH_HARMONICS] = true;
					}
					else
					{
						Measures[(int)AvgPages.U_PH_HARMONICS] = false;
					}

					#endregion
				}
				else
				{
					#region NOT EtPQP-A

					Int64 res = dbService.ExecuteScalarInt64(string.Format(
						 "SELECT count(*) FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.F_U_I] = true;
						Measures[(int)AvgPages.POWER] = true;
						Measures[(int)AvgPages.PQP] = true;
						Measures[(int)AvgPages.ANGLES] = true;
					}
					else
					{
						Measures[(int)AvgPages.F_U_I] = false;
						Measures[(int)AvgPages.POWER] = false;
						Measures[(int)AvgPages.PQP] = false;
						Measures[(int)AvgPages.ANGLES] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						 "SELECT count(*) FROM period_avg_params_5 WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.I_HARMONICS] = true;
						Measures[(int)AvgPages.U_LIN_HARMONICS] = true;
						Measures[(int)AvgPages.U_PH_HARMONICS] = true;
					}
					else
					{
						Measures[(int)AvgPages.I_HARMONICS] = false;
						Measures[(int)AvgPages.U_LIN_HARMONICS] = false;
						Measures[(int)AvgPages.U_PH_HARMONICS] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						 "SELECT count(*) FROM period_avg_params_6a WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.HARMONIC_POWERS] = true;
					}
					else
					{
						Measures[(int)AvgPages.HARMONIC_POWERS] = false;
					}

					res = dbService.ExecuteScalarInt64(string.Format(
						"SELECT count(*) FROM period_avg_params_6b WHERE datetime_id = {0};", curDatetimeId_));
					if (res > 0)
					{
						Measures[(int)AvgPages.HARMONIC_ANGLES] = true;
					}
					else
					{
						Measures[(int)AvgPages.HARMONIC_ANGLES] = false;
					}

					#endregion
				}

				return Measures;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in initializeExistingMeasures(): ");
				return Measures;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		private AVGDataGridWrapperBase GetCurrentDataGridWrapper(ref DataGrid dg)
		{
			if (dg.Equals(dgCurrentsVoltages)) return dgWrapperUIF_;
			else if (dg.Equals(dgPowers)) return dgWrapperPower_;
			else if (dg.Equals(dgPhAngles)) return dgWrapperAngles_;
			else if (dg.Equals(dgPQP)) return dgWrapperPQP_;
			else if (dg.Equals(dgCurrentHarmonics)) return dgWrapperHarmI_;
			else if (dg.Equals(dgVolPhHarmonics)) return dgWrapperHarmUph_;
			else if (dg.Equals(dgVolLinHarmonics)) return dgWrapperHarmUlin_;
			else if (dg.Equals(dgHarmonicPowers)) return dgWrapperHarmPower_;
			else if (dg.Equals(dgHarmonicAngles)) return dgWrapperHarmAngles_;
			else if (dg.Equals(dgI_Interharm)) return dgWrapperInterHarmI_;
			else if (dg.Equals(dgUph_Interharm)) return dgWrapperInterHarmUph_;
			else if (dg.Equals(dgUlin_Interharm)) return dgWrapperInterHarmUlin_;
			return null;
		}

        public string GetAVGParamName(string param)
        {
            try
            {
                ResourceManager rm = new ResourceManager("EnergomonitoringXP.param_strings", 
                    this.GetType().Assembly);

				/////////////////////////////////////////////////////////////////
				// сначала разбираем гармоники и прочие многочисленные параметры
				/////////////////////////////////////////////////////////////////
				string num = string.Empty;
				//????????????????????? попробовать тут регулярные выражения
				if (param.Length > 2)
				{
					// номер гармоники либо последняя цифра, либо две последних
					if (Char.IsDigit(param[param.Length - 2]) && Char.IsDigit(param[param.Length - 1]))
						num = param.Substring(param.Length - 2, 2);
					else if (Char.IsDigit(param[param.Length - 1]))
						num = param.Substring(param.Length - 1, 1);
					// иногда и раньше
					if (!Char.IsDigit(param[param.Length - 1]))
					{
						if (param[param.Length - 2] == '_' && param.Length >= 4)
						{
							if (Char.IsDigit(param[param.Length - 4]) && Char.IsDigit(param[param.Length - 3]))
								num = param.Substring(param.Length - 4, 2);
							else if (Char.IsDigit(param[param.Length - 3]))
								num = param.Substring(param.Length - 3, 1);
						}
						else if (param[param.Length - 3] == '_' && param.Length >= 5)
						{
							if (Char.IsDigit(param[param.Length - 5]) && Char.IsDigit(param[param.Length - 4]))
								num = param.Substring(param.Length - 5, 2);
							else if (Char.IsDigit(param[param.Length - 4]))
								num = param.Substring(param.Length - 4, 1);
						}
					}

					// многочисленные значения
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("p_sum_"))
					{
						return string.Format(rm.GetString("str_p_sum_x"), num);
					}

					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("i_n_"))
					{
						return string.Format(rm.GetString("str_i_n_x"), num);
					}

					if (Char.IsDigit(param[param.Length - 1]) &&
						(param.Contains("k_ua_") || param.Contains("k_ub_") || param.Contains("k_uc_")))
					{
						// фаза
						string phase = param.Substring(3, 1).ToUpper();
						return string.Format(rm.GetString("str_k_ux_x"), num, phase);
					}
					if (Char.IsDigit(param[param.Length - 1]) &&
						(param.Contains("k_uab_") || param.Contains("k_ubc_") || param.Contains("k_uca_")))
					{
						// фаза
						string phase = param.Substring(3, 2).ToUpper();
						return string.Format(rm.GetString("str_k_uxx_x"), num, phase);
					}

					if (Char.IsDigit(param[param.Length - 1]) &&
						(param.Contains("k_ia_") || param.Contains("k_ib_") || param.Contains("k_ic_")))
					{
						// фаза
						string phase = param.Substring(3, 1).ToUpper();
						string tmp = string.Format(rm.GetString("str_k_ix_x"), num, phase);
						return string.Format(rm.GetString("str_k_ix_x"), num, phase);
					}

					if (Char.IsDigit(param[param.Length - 1]) &&
						(param.Contains("p_a_1_") || param.Contains("p_b_2_") || param.Contains("p_c_")))
					{
						// фаза
						string phase = param.Substring(2, 1).ToUpper();
						return string.Format(rm.GetString("str_p_x_x"), num, phase);
					}
					if (Char.IsDigit(param[param.Length - 1]) &&
						(param.Contains("p_ab_") || param.Contains("p_bc_")))
					{
						// фаза
						string phase = param.Substring(2, 2).ToUpper();
						if (phase == "BC") phase = "CB";
						return string.Format(rm.GetString("str_p_xx_x"), num, phase);
					}

					if (Char.IsDigit(param[param.Length - 1]) &&
						(param.Contains("an_u_a_") || param.Contains("an_u_b_") ||
						param.Contains("an_u_c_")))
					{
						return string.Format(rm.GetString("str_an_u_x_i_x"), num);
					}

					#region Harmonics

					if (param.Length >= 4)
					{
						if (Char.IsDigit(param[param.Length - 3]) && param.Contains("order_value_"))
						{
							string phase = param.Substring(param.Length - 1, 1).ToUpper();
							if(phase != "N")
								return string.Format(rm.GetString("str_order_value_x_x"), num, phase);
							else return string.Format(rm.GetString("str_order_value_x_n"), num);
						}

						if (Char.IsDigit(param[param.Length - 3]) && param.Contains("order_coeff_"))
						{
							string phase = param.Substring(param.Length - 1, 1).ToUpper();
							if (phase != "N")
								return string.Format(rm.GetString("str_order_coeff_x_x"), num, phase);
							else return string.Format(rm.GetString("str_order_coeff_x_n"), num);
						}

						if (Char.IsDigit(param[param.Length - 4]) && param.Contains("order_value_"))
						{
							string phase = param.Substring(param.Length - 2, 2).ToUpper();
							return string.Format(rm.GetString("str_order_value_x_xx"), num, phase);
						}

						if (Char.IsDigit(param[param.Length - 4]) && param.Contains("order_coeff_"))
						{
							string phase = param.Substring(param.Length - 2, 2).ToUpper();
							return string.Format(rm.GetString("str_order_coeff_x_xx"), num, phase);
						}
					}

					#endregion

					#region Interharmonics

					if (param.Length >= 4)
					{
						if (Char.IsDigit(param[param.Length - 3]) && param.Contains("avg_square_order_"))
						{
							string phase = param.Substring(param.Length - 1, 1).ToUpper();
							//if (phase != "N")
							return string.Format(rm.GetString("avg_square_order_x_x"), num, phase);
							//else return string.Format(rm.GetString("avg_square_order_x_n"), num, phase);
						}

						if (Char.IsDigit(param[param.Length - 4]) && param.Contains("avg_square_order_"))
						{
							string phase = param.Substring(param.Length - 2, 2).ToUpper();
							return string.Format(rm.GetString("avg_square_order_x_x"), num, phase);
						}
					}

					#endregion

					#region Harmonics power

					// суммарная
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_p_sum_"))
					{
						return string.Format(rm.GetString("str_pharm_p_sum_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_q_sum_"))
					{
						return string.Format(rm.GetString("str_pharm_q_sum_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_angle_sum_"))
					{
						return string.Format(rm.GetString("str_pharm_angle_sum_x"), num);
					}
					// для 3ф3пр - вместо фаз компонент 1 или 2
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_p_1_"))
					{
						return string.Format(rm.GetString("str_pharm_p_1_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_q_1_"))
					{
						return string.Format(rm.GetString("str_pharm_q_1_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_angle_1_"))
					{
						return string.Format(rm.GetString("str_pharm_angle_1_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_p_2_"))
					{
						return string.Format(rm.GetString("str_pharm_p_2_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_q_2_"))
					{
						return string.Format(rm.GetString("str_pharm_q_2_x"), num);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_angle_2_"))
					{
						return string.Format(rm.GetString("str_pharm_angle_2_x"), num);
					}
					// фазы
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_p_"))
					{
						string phase = param.Substring(8, 1).ToUpper();
						return string.Format(rm.GetString("str_pharm_p_x_x"), num, phase);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_q_"))
					{
						string phase = param.Substring(8, 1).ToUpper();
						return string.Format(rm.GetString("str_pharm_q_x_x"), num, phase);
					}
					if (Char.IsDigit(param[param.Length - 1]) && param.Contains("pharm_angle_"))
					{
						string phase = param.Substring(12, 1).ToUpper();
						return string.Format(rm.GetString("str_pharm_angle_x_x"), num, phase);
					}

					#endregion
				}

				/////////////////////////////////////////////////////////////////
				// дальше одиночные параметры
				/////////////////////////////////////////////////////////////////
                if (param.Equals("k_ua_ab"))
                {
                    if (curConnectionScheme_ == ConnectScheme.Ph3W3 || 
                        curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
                        param = "k_uab";
                    else param = "k_ua";
                }
                if (param.Equals("k_ub_bc"))
                {
                    if (curConnectionScheme_ == ConnectScheme.Ph3W3 || 
                        curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
                        param = "k_ubc";
                    else param = "k_ub";
                }
                if (param.Equals("k_uc_ca"))
                {
                    if (curConnectionScheme_ == ConnectScheme.Ph3W3 || 
                        curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
                        param = "k_uca";
                    else param = "k_uc";
                }

				if (param.Equals("p_a_1"))
				{
					if (curConnectionScheme_ == ConnectScheme.Ph3W3 ||
						curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
						param = "p_ab";
					else param = "p_a";
				}
				if (param.Equals("p_b_2"))
				{
					if (curConnectionScheme_ == ConnectScheme.Ph3W3 ||
						curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
						param = "p_cb";
					else param = "p_b";
				}
               
                return rm.GetString("str_" + param);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in GetAVGParamName():");
                return "";
            }
		}

		#endregion
	}
}
