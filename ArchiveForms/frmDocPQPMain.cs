using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Resources;
using System.IO;
using System.Text;
using System.Drawing.Drawing2D;
using System.Xml;
using System.Globalization;

using WeifenLuo.WinFormsUI;
using EnergomonitoringXP;
using DbServiceLib;
using DataGridColumnStyles;
using ZedGraph;
using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmDoc1.
	/// </summary>
	public class frmDocPQPMain : DockContentGraphMethods
	{
		#region Fields

		private EmGraphLib.PqpGraph.Bar barFreqNplSt;
		private EmGraphLib.PqpGraph.Bar barFreqUplSt;
		private EmGraphLib.PqpGraph.Bar barFreqNplRes;
		private EmGraphLib.PqpGraph.Bar barFreqUplRes;
		private EmGraphLib.PqpGraph.Bar barVoltNplSt;
		private EmGraphLib.PqpGraph.Bar barVoltUplSt;
		private EmGraphLib.PqpGraph.Bar barVoltNplRes;
		private EmGraphLib.PqpGraph.Bar barVoltUplRes;
		private EmGraphLib.PqpGraph.Bar barVoltUnbNplSt;
		private EmGraphLib.PqpGraph.Bar barVoltUnbUplSt;
		private EmGraphLib.PqpGraph.Bar barVoltUnbFake;
		private EmGraphLib.PqpGraph.Bar barVoltUnbUplRes;
		private EmGraphLib.PqpGraph.Bar barVoltNsNplSt;
		private EmGraphLib.PqpGraph.Bar barVoltNsUplSt;
		private EmGraphLib.PqpGraph.Bar barVoltNsPh1UplRes;
		private EmGraphLib.PqpGraph.Bar barVoltNsPh2UplRes;
		private EmGraphLib.PqpGraph.Bar barVoltNsPh3UplRes;
		private IContainer components;

		private EmDeviceType curDeviceType_;
		private DateTime curStartDateTime_ = DateTime.MinValue;
		private DateTime curEndDateTime_ = DateTime.MinValue;
		public ConnectScheme CurConnectScheme = ConnectScheme.Unknown;
		private int curPgServerIndex_ = 0;
		private Int64 curDatetimeId_ = 0;
		private string curDeviceName_;
		private short curConstrType_;
		private DateTime sdtToPL1_;
		private DateTime edtToPL1_;
		private DateTime sdtToPL2_;
		private DateTime edtToPL2_;

		private short t_fliker_ = -1;

		private EmDataSaver.Settings settings_;

		/// <summary> Pointer to the main application window </summary>
		private frmMain MainWindow_;

		/// <summary>Context menu</summary>
		private ContextMenuStrip cmsDoc;
		private ToolStripMenuItem percentToolStripMenuItem;
		private ToolStripMenuItem percentGlToolStripMenuItem;
		private ToolStripMenuItem timeToolStripMenuItem;
		private ToolStripMenuItem numberToolStripMenuItem;

		// Brush to mark error cells
		private Brush ErrorBrush = new SolidBrush(Color.Red);
		private TabControl tabControl1;
		private TabPage tabFrequencyDeparture;
		private SplitContainer splitContainerFreq;
		public DataGrid dgFrequencyDeparture;
		private EmGraphLib.PqpGraph.PqpGraphControl pqpGraph_dF;
		private TabPage tabDips;
		private TableLayoutPanel tableLayoutPanel1;
		private Label lblDips;
		private Label lblSwells;
		public DataGrid dgOvers;
		private Label lblSwells2;
		public DataGrid dgDips2;
		public DataGrid dgOvers2;
		public DataGrid dgDips;
		private Label lblDips2;
		private TabPage tabFValues;
		public DataGrid dgFValues;
		private TabPage tabU_Deviation;
		private SplitContainer splitContainerVolt;
		public DataGrid dgU_Deviation;
		private EmGraphLib.PqpGraph.PqpGraphControl pqpGraph_dU;
		private TabPage tabVolValues;
		public DataGrid dgVolValues;
		private TabPage tabVoltageNnbalance;
		private SplitContainer splitContainerVoltHarm0;
		public DataGrid dgNonSymmetry;
		private EmGraphLib.PqpGraph.PqpGraphControl pqpGraphVoltUnb;
		private TabPage tabVoltageNonsinusoidality;
		private SplitContainer scVolHarmonics;
		public SplitContainer splitContainerVoltHarm1;
		public DataGrid dgUNonsinusoidality;
		private Label lblPhase;
		public DataGrid dgUNonsinusoidality2;
		private Label lblInterphase;
		private EmGraphLib.PqpGraph.PqpGraphControl pqpGraphVoltNS;
		private TabPage tabInterharm;
		public DataGrid dgInterharm;
		private TabPage tabFlickerNum;
		public DataGrid dgFlickerNum;
		private TabPage tabFliker;
		private SplitContainer scFliker;
		public DataGrid dgFlicker;
		public DataGrid dgFlickerLong;
		private Label lbl_EPI_Caption;
		private Button btnMaxMode;
		private Brush grayBrush_ = new SolidBrush(Color.Gray);

		///////////////////// for EtPQP-A ////////////////////
		// эти поля используются только для ЭтПКЭ-А. для него эти периоды задаются вручную
		// и не хранятся в БД, поэтому сохраняем их тут
		private DateTime dtMaxModeStart1_ = DateTime.MinValue;
		private DateTime dtMaxModeEnd1_ = DateTime.MinValue;
		private DateTime dtMaxModeStart2_ = DateTime.MinValue;
		private DateTime dtMaxModeEnd2_ = DateTime.MinValue;
		// уставки для той же цели
		private float constrNPLtopMax_ = Single.NaN;		// Max - для наибольших нагрузок
		private float constrNPLtopMin_ = Single.NaN;		// Min - для наименьших
		private float constrNPLbottomMax_ = Single.NaN;
		private float constrNPLbottomMin_ = Single.NaN;
		private float constrUPLtopMax_ = Single.NaN;
		private float constrUPLtopMin_ = Single.NaN;
		private float constrUPLbottomMax_ = Single.NaN;
		private float constrUPLbottomMin_ = Single.NaN;
		// переменная указывает надо ли считать эти режимы
		private bool bNeedMaxModeForEtPQP_A_ = false;
		//////////////////////////////////////////////////////

		#endregion

		/// <summary>Constructor</summary>
		/// <param name="MainWindow">Pointer to the main application window</param>
		/// <param name="settings">settings object</param>
		public frmDocPQPMain(frmMain MainWindow, EmDataSaver.Settings settings)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// My region
			//
			this.settings_ = settings;
			MainWindow_ = MainWindow;

			//dgNonSymmetryInitTableStyle();
			//dgFrequencyDepartureInitTableStyle();
			//dgU_DeviationInitTableStyle();
			//dgUNonsinusInitTableStyle();
			//dgDipsInitTableStyle();
			//dgOversInitTableStyle();
			//dgDipsInitTableStyle2();
			//dgOversInitTableStyle2();
			//dgFlickerShortInitTableStyle();
			//dgFlickerLongInitTableStyle();
			//dgUValuesInitTableStyle();
			//dgFreqValuesInitTableStyle();

			//miMeasureNumberAnyVariant_Click(percentToolStripMenuItem, EventArgs.Empty);
		}

		/// <summary>Clean up any resources being used.</summary>
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

		/// <summary>Open an archive</summary>
		public void Open(int pgServerIndex, EmDeviceType devType, ConnectScheme conScheme, Int64 datetimeID,
			DateTime dtStart, DateTime dtEnd, string devVersion, 
			short t_fliker, short constrType) 
		{
			try
			{
				// actual for EtPQP-A only!
				DEVICE_VERSIONS newDipSwellMode = DEVICE_VERSIONS.ETPQP_A_OLD;
				if (devType == EmDeviceType.ETPQP_A)
				{
					newDipSwellMode = Constants.isNewDeviceVersion_ETPQP_A(devVersion);
				}

				if (devType != EmDeviceType.ETPQP_A) btnMaxMode.Visible = false;
				else btnMaxMode.Visible = true;

				curPgServerIndex_ = pgServerIndex;
				CurConnectScheme = conScheme;
				curDatetimeId_ = datetimeID;
				curStartDateTime_ = dtStart;
				curEndDateTime_ = dtEnd;
				t_fliker_ = t_fliker;
				curDeviceType_ = devType;
				curConstrType_ = constrType;

				dgNonSymmetryInitTableStyle();
				dgFrequencyDepartureInitTableStyle();
				dgU_DeviationInitTableStyle();
				dgUNonsinusInitTableStyle();
				if (curDeviceType_ == EmDeviceType.ETPQP_A) dgInterharmInitTableStyle();
				dgDipsInitTableStyle(newDipSwellMode);
				dgOversInitTableStyle(newDipSwellMode);
				if (curDeviceType_ != EmDeviceType.ETPQP_A || 
					Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073) 
					dgDipsInitTableStyle2();
				if (curDeviceType_ != EmDeviceType.ETPQP_A) dgOversInitTableStyle2();
				if (curDeviceType_ == EmDeviceType.ETPQP_A) dgFlickerNumInitTableStyle();
				if ((curDeviceType_ == EmDeviceType.ETPQP_A) ||
					(conScheme != ConnectScheme.Ph3W3 && conScheme != ConnectScheme.Ph3W3_B_calc))
					dgFlickerShortInitTableStyle();
				if ((curDeviceType_ == EmDeviceType.ETPQP_A) ||
				    (conScheme != ConnectScheme.Ph3W3 && conScheme != ConnectScheme.Ph3W3_B_calc))
					dgFlickerLongInitTableStyle();
				//if (curDeviceType_ != EmDeviceType.ETPQP_A) 
				dgUValuesInitTableStyle();
				//if (curDeviceType_ != EmDeviceType.ETPQP_A) 
				dgFreqValuesInitTableStyle();
				miMeasureNumberAnyVariant_Click(percentToolStripMenuItem, EventArgs.Empty);

				#region show or hide dgDips2 and dgOvers2

				bool dip_swell2_visible = true;
				if (curDeviceType_ == EmDeviceType.ETPQP_A || CurConnectScheme == ConnectScheme.Ph1W2)
					dip_swell2_visible = false;

				if (dip_swell2_visible)
				{
					if (tableLayoutPanel1.RowStyles.Count == 4)
					{
						this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
							System.Windows.Forms.SizeType.Absolute, 20.0F));
						this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
							System.Windows.Forms.SizeType.Percent, 25.0F));
						this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
							System.Windows.Forms.SizeType.Absolute, 20.0F));
						this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
							System.Windows.Forms.SizeType.Percent, 25.0F));

						tableLayoutPanel1.RowStyles[1].Height = 25.0F;
						tableLayoutPanel1.RowStyles[3].Height = 25.0F;
					}

					if (tableLayoutPanel1.RowStyles.Count == 6)
					{
						this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
							System.Windows.Forms.SizeType.Absolute, 20.0F));
						this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
							System.Windows.Forms.SizeType.Percent, 25.0F));

						tableLayoutPanel1.RowStyles[1].Height = 25.0F;
						tableLayoutPanel1.RowStyles[3].Height = 25.0F;
						tableLayoutPanel1.RowStyles[5].Height = 25.0F;
					}
				}
				else
				{
					// скрываем обе таблицы Dip2 and Over2
					if (Constants.isNewDeviceVersion_ETPQP_A(devVersion) != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					{
						if (tableLayoutPanel1.RowStyles.Count >= 8)
						{
							tableLayoutPanel1.RowStyles.RemoveAt(7);
							tableLayoutPanel1.RowStyles.RemoveAt(6);
							tableLayoutPanel1.RowStyles.RemoveAt(5);
							tableLayoutPanel1.RowStyles.RemoveAt(4);

							tableLayoutPanel1.RowStyles[1].Height = 50.0F;
							tableLayoutPanel1.RowStyles[3].Height = 50.0F;
						}

						if (tableLayoutPanel1.RowStyles.Count == 6)
						{
							tableLayoutPanel1.RowStyles.RemoveAt(5);
							tableLayoutPanel1.RowStyles.RemoveAt(4);

							tableLayoutPanel1.RowStyles[1].Height = 50.0F;
							tableLayoutPanel1.RowStyles[3].Height = 50.0F;
						}
					}
					else  // Over2 скрываем, а вместо Dip2 отображам прерывания
					{
						if (tableLayoutPanel1.RowStyles.Count >= 8)
						{
							tableLayoutPanel1.RowStyles.RemoveAt(7);
							tableLayoutPanel1.RowStyles.RemoveAt(6);
							//tableLayoutPanel1.RowStyles.RemoveAt(5);
							//tableLayoutPanel1.RowStyles.RemoveAt(4);

							tableLayoutPanel1.RowStyles[1].Height = 40.0F;
							tableLayoutPanel1.RowStyles[3].Height = 40.0F;
							tableLayoutPanel1.RowStyles[5].Height = 20.0F;
						}

						if (tableLayoutPanel1.RowStyles.Count == 4)
						{
							this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
								System.Windows.Forms.SizeType.Absolute, 20.0F));
							this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(
								System.Windows.Forms.SizeType.Percent, 20.0F));

							tableLayoutPanel1.RowStyles[1].Height = 40.0F;
							tableLayoutPanel1.RowStyles[3].Height = 40.0F;
						}
					}
				}

				dgOvers2.Visible = dip_swell2_visible;
				dgDips2.Visible = dip_swell2_visible || 
					Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073;
				lblDips2.Visible = dip_swell2_visible ||
					Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073;
				lblSwells2.Visible = dip_swell2_visible;

				#endregion

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					MainWindow_.wndDocPQP.wndDocVolValuesGraphBottom.ClearGraphs();
				else MainWindow_.wndDocPQP.wndDocVolValuesGraphBottom_PQP_A.ClearGraphs();
				MainWindow_.wndDocPQP.wndDocFValuesGraphBottom.ClearGraphs();

				ChangePercentFormat();

				bool flikkerExists = false;
				if (conScheme != ConnectScheme.Ph3W3 &&
					conScheme != ConnectScheme.Ph3W3_B_calc)
				{
					if (curDeviceType_ == EmDeviceType.EM32 ||
					curDeviceType_ == EmDeviceType.ETPQP ||
					curDeviceType_ == EmDeviceType.EM33T1)
						flikkerExists = true;
					else if (curDeviceType_ == EmDeviceType.EM33T)
						if (Constants.isNewDeviceVersion_EM33T(devVersion))
							flikkerExists = true;
				}
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
					flikkerExists = true;

				IninializeGrids(flikkerExists);

				drawdgFrequencyDeparture();
				drawdgU_Deviation();
				if (curDeviceType_ != EmDeviceType.EM32)
					drawdgFreqValues();
				if (curDeviceType_ != EmDeviceType.EM32)
					drawdgUValues();
				drawdgNonSymmetry();
				drawdgUNonsinusoidality();
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
					drawdgInterharm();

				drawdgDips(devVersion);
				if (curDeviceType_ != EmDeviceType.ETPQP_A || 
					Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					drawdgDips2();
				drawdgOvers(devVersion);
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					drawdgOvers2();

				tabControl1.SelectedIndex = 0;

				if (flikkerExists)
				{
					drawdgFlicker();
					drawdgFlickerLong();
				}
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					drawdgFlickerNum();
					MainWindow_.wndDocPQP.wndDocVolValuesGraphBottom_PQP_A.EnableGraphs(conScheme);
				}
				else MainWindow_.wndDocPQP.wndDocVolValuesGraphBottom.EnableGraphs(conScheme);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDocPQPMain::Open()");
				throw;
			}
		}

		private void IninializeGrids(bool flikkerExists)
		{
			try
			{
				try
				{
					tabControl1.TabPages.Clear();
				}
				catch { }

				tabControl1.TabPages.Add(tabFrequencyDeparture);

				if(curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.ETPQP_A ||
					curDeviceType_ == EmDeviceType.EM33T || curDeviceType_ == EmDeviceType.EM33T1)
					tabControl1.TabPages.Add(tabFValues);

				tabControl1.TabPages.Add(tabU_Deviation);

				if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.ETPQP_A ||
					curDeviceType_ == EmDeviceType.EM33T || curDeviceType_ == EmDeviceType.EM33T1)
					tabControl1.TabPages.Add(tabVolValues);

				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					tabControl1.TabPages.Add(tabVoltageNnbalance);
				}

				tabControl1.TabPages.Add(tabVoltageNonsinusoidality);
				tabControl1.TabPages.Add(tabDips);

				if (flikkerExists)
				{
					tabControl1.TabPages.Add(tabFliker);
				}

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					if(flikkerExists)
						tabControl1.TabPages.Add(tabFlickerNum);
					tabControl1.TabPages.Add(tabInterharm);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in IninializeGrids()");
				throw;
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocPQPMain));
            this.splitContainerFreq = new System.Windows.Forms.SplitContainer();
            this.dgFrequencyDeparture = new System.Windows.Forms.DataGrid();
            this.pqpGraph_dF = new EmGraphLib.PqpGraph.PqpGraphControl();
            this.barFreqNplSt = new EmGraphLib.PqpGraph.Bar();
            this.barFreqUplSt = new EmGraphLib.PqpGraph.Bar();
            this.barFreqNplRes = new EmGraphLib.PqpGraph.Bar();
            this.barFreqUplRes = new EmGraphLib.PqpGraph.Bar();
            this.splitContainerVolt = new System.Windows.Forms.SplitContainer();
            this.dgU_Deviation = new System.Windows.Forms.DataGrid();
            this.pqpGraph_dU = new EmGraphLib.PqpGraph.PqpGraphControl();
            this.barVoltNplSt = new EmGraphLib.PqpGraph.Bar();
            this.barVoltUplSt = new EmGraphLib.PqpGraph.Bar();
            this.barVoltNplRes = new EmGraphLib.PqpGraph.Bar();
            this.barVoltUplRes = new EmGraphLib.PqpGraph.Bar();
            this.splitContainerVoltHarm0 = new System.Windows.Forms.SplitContainer();
            this.dgNonSymmetry = new System.Windows.Forms.DataGrid();
            this.pqpGraphVoltUnb = new EmGraphLib.PqpGraph.PqpGraphControl();
            this.barVoltUnbNplSt = new EmGraphLib.PqpGraph.Bar();
            this.barVoltUnbUplSt = new EmGraphLib.PqpGraph.Bar();
            this.barVoltUnbFake = new EmGraphLib.PqpGraph.Bar();
            this.barVoltUnbUplRes = new EmGraphLib.PqpGraph.Bar();
            this.scVolHarmonics = new System.Windows.Forms.SplitContainer();
            this.splitContainerVoltHarm1 = new System.Windows.Forms.SplitContainer();
            this.dgUNonsinusoidality = new System.Windows.Forms.DataGrid();
            this.lblPhase = new System.Windows.Forms.Label();
            this.dgUNonsinusoidality2 = new System.Windows.Forms.DataGrid();
            this.lblInterphase = new System.Windows.Forms.Label();
            this.pqpGraphVoltNS = new EmGraphLib.PqpGraph.PqpGraphControl();
            this.barVoltNsNplSt = new EmGraphLib.PqpGraph.Bar();
            this.barVoltNsUplSt = new EmGraphLib.PqpGraph.Bar();
            this.barVoltNsPh1UplRes = new EmGraphLib.PqpGraph.Bar();
            this.barVoltNsPh2UplRes = new EmGraphLib.PqpGraph.Bar();
            this.barVoltNsPh3UplRes = new EmGraphLib.PqpGraph.Bar();
            this.scFliker = new System.Windows.Forms.SplitContainer();
            this.dgFlicker = new System.Windows.Forms.DataGrid();
            this.dgFlickerLong = new System.Windows.Forms.DataGrid();
            this.cmsDoc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.percentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.percentGlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.numberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabFrequencyDeparture = new System.Windows.Forms.TabPage();
            this.tabDips = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDips = new System.Windows.Forms.Label();
            this.lblSwells = new System.Windows.Forms.Label();
            this.dgOvers = new System.Windows.Forms.DataGrid();
            this.lblSwells2 = new System.Windows.Forms.Label();
            this.dgDips2 = new System.Windows.Forms.DataGrid();
            this.dgOvers2 = new System.Windows.Forms.DataGrid();
            this.dgDips = new System.Windows.Forms.DataGrid();
            this.lblDips2 = new System.Windows.Forms.Label();
            this.tabFValues = new System.Windows.Forms.TabPage();
            this.dgFValues = new System.Windows.Forms.DataGrid();
            this.tabU_Deviation = new System.Windows.Forms.TabPage();
            this.tabVolValues = new System.Windows.Forms.TabPage();
            this.dgVolValues = new System.Windows.Forms.DataGrid();
            this.tabVoltageNnbalance = new System.Windows.Forms.TabPage();
            this.tabVoltageNonsinusoidality = new System.Windows.Forms.TabPage();
            this.tabInterharm = new System.Windows.Forms.TabPage();
            this.dgInterharm = new System.Windows.Forms.DataGrid();
            this.tabFlickerNum = new System.Windows.Forms.TabPage();
            this.dgFlickerNum = new System.Windows.Forms.DataGrid();
            this.tabFliker = new System.Windows.Forms.TabPage();
            this.lbl_EPI_Caption = new System.Windows.Forms.Label();
            this.btnMaxMode = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerFreq)).BeginInit();
            this.splitContainerFreq.Panel1.SuspendLayout();
            this.splitContainerFreq.Panel2.SuspendLayout();
            this.splitContainerFreq.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgFrequencyDeparture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerVolt)).BeginInit();
            this.splitContainerVolt.Panel1.SuspendLayout();
            this.splitContainerVolt.Panel2.SuspendLayout();
            this.splitContainerVolt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgU_Deviation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerVoltHarm0)).BeginInit();
            this.splitContainerVoltHarm0.Panel1.SuspendLayout();
            this.splitContainerVoltHarm0.Panel2.SuspendLayout();
            this.splitContainerVoltHarm0.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgNonSymmetry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scVolHarmonics)).BeginInit();
            this.scVolHarmonics.Panel1.SuspendLayout();
            this.scVolHarmonics.Panel2.SuspendLayout();
            this.scVolHarmonics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerVoltHarm1)).BeginInit();
            this.splitContainerVoltHarm1.Panel1.SuspendLayout();
            this.splitContainerVoltHarm1.Panel2.SuspendLayout();
            this.splitContainerVoltHarm1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgUNonsinusoidality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgUNonsinusoidality2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scFliker)).BeginInit();
            this.scFliker.Panel1.SuspendLayout();
            this.scFliker.Panel2.SuspendLayout();
            this.scFliker.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgFlicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgFlickerLong)).BeginInit();
            this.cmsDoc.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabFrequencyDeparture.SuspendLayout();
            this.tabDips.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgOvers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDips2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgOvers2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDips)).BeginInit();
            this.tabFValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgFValues)).BeginInit();
            this.tabU_Deviation.SuspendLayout();
            this.tabVolValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgVolValues)).BeginInit();
            this.tabVoltageNnbalance.SuspendLayout();
            this.tabVoltageNonsinusoidality.SuspendLayout();
            this.tabInterharm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgInterharm)).BeginInit();
            this.tabFlickerNum.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgFlickerNum)).BeginInit();
            this.tabFliker.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerFreq
            // 
            resources.ApplyResources(this.splitContainerFreq, "splitContainerFreq");
            this.splitContainerFreq.Name = "splitContainerFreq";
            // 
            // splitContainerFreq.Panel1
            // 
            resources.ApplyResources(this.splitContainerFreq.Panel1, "splitContainerFreq.Panel1");
            this.splitContainerFreq.Panel1.Controls.Add(this.dgFrequencyDeparture);
            // 
            // splitContainerFreq.Panel2
            // 
            resources.ApplyResources(this.splitContainerFreq.Panel2, "splitContainerFreq.Panel2");
            this.splitContainerFreq.Panel2.Controls.Add(this.pqpGraph_dF);
            // 
            // dgFrequencyDeparture
            // 
            resources.ApplyResources(this.dgFrequencyDeparture, "dgFrequencyDeparture");
            this.dgFrequencyDeparture.AllowSorting = false;
            this.dgFrequencyDeparture.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgFrequencyDeparture.DataMember = "";
            this.dgFrequencyDeparture.HeaderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgFrequencyDeparture.Name = "dgFrequencyDeparture";
            this.dgFrequencyDeparture.ReadOnly = true;
            this.dgFrequencyDeparture.Tag = "Отклонение частоты";
            this.dgFrequencyDeparture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridContextMenu_MouseUp);
            // 
            // pqpGraph_dF
            // 
            resources.ApplyResources(this.pqpGraph_dF, "pqpGraph_dF");
            this.pqpGraph_dF.BackColor = System.Drawing.Color.White;
            this.pqpGraph_dF.Bars.Add(this.barFreqNplSt);
            this.pqpGraph_dF.Bars.Add(this.barFreqUplSt);
            this.pqpGraph_dF.Bars.Add(this.barFreqNplRes);
            this.pqpGraph_dF.Bars.Add(this.barFreqUplRes);
            this.pqpGraph_dF.BorderColor = System.Drawing.Color.SteelBlue;
            this.pqpGraph_dF.Name = "pqpGraph_dF";
            // 
            // barFreqNplSt
            // 
            this.barFreqNplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barFreqNplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barFreqNplSt, "barFreqNplSt");
            this.barFreqNplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barFreqNplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barFreqNplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barFreqNplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barFreqNplSt.PercentText = "";
            this.barFreqNplSt.WorldBottom = 0F;
            this.barFreqNplSt.WorldTop = 1F;
            this.barFreqNplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barFreqNplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barFreqUplSt
            // 
            this.barFreqUplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barFreqUplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barFreqUplSt, "barFreqUplSt");
            this.barFreqUplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barFreqUplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barFreqUplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barFreqUplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barFreqUplSt.PercentText = "";
            this.barFreqUplSt.WorldBottom = 0F;
            this.barFreqUplSt.WorldTop = 1F;
            this.barFreqUplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barFreqUplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barFreqNplRes
            // 
            this.barFreqNplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barFreqNplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barFreqNplRes, "barFreqNplRes");
            this.barFreqNplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barFreqNplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barFreqNplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barFreqNplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barFreqNplRes.PercentText = "";
            this.barFreqNplRes.WorldBottom = 0F;
            this.barFreqNplRes.WorldTop = 1F;
            this.barFreqNplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barFreqNplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barFreqUplRes
            // 
            this.barFreqUplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barFreqUplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barFreqUplRes, "barFreqUplRes");
            this.barFreqUplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barFreqUplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barFreqUplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barFreqUplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barFreqUplRes.PercentText = "";
            this.barFreqUplRes.WorldBottom = 0F;
            this.barFreqUplRes.WorldTop = 1F;
            this.barFreqUplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barFreqUplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // splitContainerVolt
            // 
            resources.ApplyResources(this.splitContainerVolt, "splitContainerVolt");
            this.splitContainerVolt.Name = "splitContainerVolt";
            // 
            // splitContainerVolt.Panel1
            // 
            resources.ApplyResources(this.splitContainerVolt.Panel1, "splitContainerVolt.Panel1");
            this.splitContainerVolt.Panel1.Controls.Add(this.dgU_Deviation);
            // 
            // splitContainerVolt.Panel2
            // 
            resources.ApplyResources(this.splitContainerVolt.Panel2, "splitContainerVolt.Panel2");
            this.splitContainerVolt.Panel2.Controls.Add(this.pqpGraph_dU);
            // 
            // dgU_Deviation
            // 
            resources.ApplyResources(this.dgU_Deviation, "dgU_Deviation");
            this.dgU_Deviation.AllowSorting = false;
            this.dgU_Deviation.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgU_Deviation.DataMember = "";
            this.dgU_Deviation.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgU_Deviation.Name = "dgU_Deviation";
            this.dgU_Deviation.ReadOnly = true;
            this.dgU_Deviation.Tag = "Отклонение напряжения";
            this.dgU_Deviation.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridContextMenu_MouseUp);
            // 
            // pqpGraph_dU
            // 
            resources.ApplyResources(this.pqpGraph_dU, "pqpGraph_dU");
            this.pqpGraph_dU.BackColor = System.Drawing.Color.White;
            this.pqpGraph_dU.Bars.Add(this.barVoltNplSt);
            this.pqpGraph_dU.Bars.Add(this.barVoltUplSt);
            this.pqpGraph_dU.Bars.Add(this.barVoltNplRes);
            this.pqpGraph_dU.Bars.Add(this.barVoltUplRes);
            this.pqpGraph_dU.BorderColor = System.Drawing.Color.SteelBlue;
            this.pqpGraph_dU.Name = "pqpGraph_dU";
            // 
            // barVoltNplSt
            // 
            this.barVoltNplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barVoltNplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barVoltNplSt, "barVoltNplSt");
            this.barVoltNplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barVoltNplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barVoltNplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNplSt.PercentText = "";
            this.barVoltNplSt.WorldBottom = 0F;
            this.barVoltNplSt.WorldTop = 1F;
            this.barVoltNplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barVoltNplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltUplSt
            // 
            this.barVoltUplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barVoltUplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barVoltUplSt, "barVoltUplSt");
            this.barVoltUplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barVoltUplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltUplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barVoltUplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUplSt.PercentText = "";
            this.barVoltUplSt.WorldBottom = 0F;
            this.barVoltUplSt.WorldTop = 1F;
            this.barVoltUplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barVoltUplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltNplRes
            // 
            this.barVoltNplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barVoltNplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barVoltNplRes, "barVoltNplRes");
            this.barVoltNplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barVoltNplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barVoltNplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNplRes.PercentText = "";
            this.barVoltNplRes.WorldBottom = 0F;
            this.barVoltNplRes.WorldTop = 1F;
            this.barVoltNplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barVoltNplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltUplRes
            // 
            this.barVoltUplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barVoltUplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barVoltUplRes, "barVoltUplRes");
            this.barVoltUplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barVoltUplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltUplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barVoltUplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUplRes.PercentText = "";
            this.barVoltUplRes.WorldBottom = 0F;
            this.barVoltUplRes.WorldTop = 1F;
            this.barVoltUplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barVoltUplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // splitContainerVoltHarm0
            // 
            resources.ApplyResources(this.splitContainerVoltHarm0, "splitContainerVoltHarm0");
            this.splitContainerVoltHarm0.Name = "splitContainerVoltHarm0";
            // 
            // splitContainerVoltHarm0.Panel1
            // 
            resources.ApplyResources(this.splitContainerVoltHarm0.Panel1, "splitContainerVoltHarm0.Panel1");
            this.splitContainerVoltHarm0.Panel1.Controls.Add(this.dgNonSymmetry);
            // 
            // splitContainerVoltHarm0.Panel2
            // 
            resources.ApplyResources(this.splitContainerVoltHarm0.Panel2, "splitContainerVoltHarm0.Panel2");
            this.splitContainerVoltHarm0.Panel2.Controls.Add(this.pqpGraphVoltUnb);
            // 
            // dgNonSymmetry
            // 
            resources.ApplyResources(this.dgNonSymmetry, "dgNonSymmetry");
            this.dgNonSymmetry.AllowSorting = false;
            this.dgNonSymmetry.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgNonSymmetry.DataMember = "";
            this.dgNonSymmetry.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgNonSymmetry.Name = "dgNonSymmetry";
            this.dgNonSymmetry.ReadOnly = true;
            this.dgNonSymmetry.Tag = "Несимметрия напряжений";
            this.dgNonSymmetry.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridContextMenu_MouseUp);
            // 
            // pqpGraphVoltUnb
            // 
            resources.ApplyResources(this.pqpGraphVoltUnb, "pqpGraphVoltUnb");
            this.pqpGraphVoltUnb.BackColor = System.Drawing.Color.White;
            this.pqpGraphVoltUnb.Bars.Add(this.barVoltUnbNplSt);
            this.pqpGraphVoltUnb.Bars.Add(this.barVoltUnbUplSt);
            this.pqpGraphVoltUnb.Bars.Add(this.barVoltUnbFake);
            this.pqpGraphVoltUnb.Bars.Add(this.barVoltUnbUplRes);
            this.pqpGraphVoltUnb.BorderColor = System.Drawing.Color.SteelBlue;
            this.pqpGraphVoltUnb.Name = "pqpGraphVoltUnb";
            // 
            // barVoltUnbNplSt
            // 
            this.barVoltUnbNplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbNplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barVoltUnbNplSt, "barVoltUnbNplSt");
            this.barVoltUnbNplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbNplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltUnbNplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbNplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUnbNplSt.PercentText = "";
            this.barVoltUnbNplSt.WorldBottom = 0F;
            this.barVoltUnbNplSt.WorldTop = 1F;
            this.barVoltUnbNplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbNplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltUnbUplSt
            // 
            this.barVoltUnbUplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbUplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barVoltUnbUplSt, "barVoltUnbUplSt");
            this.barVoltUnbUplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbUplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltUnbUplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbUplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUnbUplSt.PercentText = "";
            this.barVoltUnbUplSt.WorldBottom = 0F;
            this.barVoltUnbUplSt.WorldTop = 1F;
            this.barVoltUnbUplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barVoltUnbUplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltUnbFake
            // 
            this.barVoltUnbFake.BarBorderColor = System.Drawing.Color.White;
            this.barVoltUnbFake.BarColor = System.Drawing.Color.White;
            resources.ApplyResources(this.barVoltUnbFake, "barVoltUnbFake");
            this.barVoltUnbFake.CaptionColor = System.Drawing.Color.White;
            this.barVoltUnbFake.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUnbFake.PercentColor = System.Drawing.Color.White;
            this.barVoltUnbFake.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUnbFake.PercentText = "";
            this.barVoltUnbFake.WorldBottom = 0F;
            this.barVoltUnbFake.WorldTop = 1F;
            this.barVoltUnbFake.WorldValColor = System.Drawing.Color.White;
            this.barVoltUnbFake.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            // 
            // barVoltUnbUplRes
            // 
            this.barVoltUnbUplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barVoltUnbUplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barVoltUnbUplRes, "barVoltUnbUplRes");
            this.barVoltUnbUplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barVoltUnbUplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltUnbUplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barVoltUnbUplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltUnbUplRes.PercentText = "";
            this.barVoltUnbUplRes.WorldBottom = 0F;
            this.barVoltUnbUplRes.WorldTop = 1F;
            this.barVoltUnbUplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barVoltUnbUplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // scVolHarmonics
            // 
            resources.ApplyResources(this.scVolHarmonics, "scVolHarmonics");
            this.scVolHarmonics.Name = "scVolHarmonics";
            // 
            // scVolHarmonics.Panel1
            // 
            resources.ApplyResources(this.scVolHarmonics.Panel1, "scVolHarmonics.Panel1");
            this.scVolHarmonics.Panel1.Controls.Add(this.splitContainerVoltHarm1);
            // 
            // scVolHarmonics.Panel2
            // 
            resources.ApplyResources(this.scVolHarmonics.Panel2, "scVolHarmonics.Panel2");
            this.scVolHarmonics.Panel2.Controls.Add(this.pqpGraphVoltNS);
            // 
            // splitContainerVoltHarm1
            // 
            resources.ApplyResources(this.splitContainerVoltHarm1, "splitContainerVoltHarm1");
            this.splitContainerVoltHarm1.Name = "splitContainerVoltHarm1";
            // 
            // splitContainerVoltHarm1.Panel1
            // 
            resources.ApplyResources(this.splitContainerVoltHarm1.Panel1, "splitContainerVoltHarm1.Panel1");
            this.splitContainerVoltHarm1.Panel1.Controls.Add(this.dgUNonsinusoidality);
            this.splitContainerVoltHarm1.Panel1.Controls.Add(this.lblPhase);
            // 
            // splitContainerVoltHarm1.Panel2
            // 
            resources.ApplyResources(this.splitContainerVoltHarm1.Panel2, "splitContainerVoltHarm1.Panel2");
            this.splitContainerVoltHarm1.Panel2.Controls.Add(this.dgUNonsinusoidality2);
            this.splitContainerVoltHarm1.Panel2.Controls.Add(this.lblInterphase);
            // 
            // dgUNonsinusoidality
            // 
            resources.ApplyResources(this.dgUNonsinusoidality, "dgUNonsinusoidality");
            this.dgUNonsinusoidality.AllowSorting = false;
            this.dgUNonsinusoidality.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgUNonsinusoidality.DataMember = "";
            this.dgUNonsinusoidality.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgUNonsinusoidality.Name = "dgUNonsinusoidality";
            this.dgUNonsinusoidality.ReadOnly = true;
            this.dgUNonsinusoidality.Tag = "Несинусоидальность напряжения";
            this.dgUNonsinusoidality.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridContextMenu_MouseUp);
            // 
            // lblPhase
            // 
            resources.ApplyResources(this.lblPhase, "lblPhase");
            this.lblPhase.BackColor = System.Drawing.SystemColors.Menu;
            this.lblPhase.Name = "lblPhase";
            // 
            // dgUNonsinusoidality2
            // 
            resources.ApplyResources(this.dgUNonsinusoidality2, "dgUNonsinusoidality2");
            this.dgUNonsinusoidality2.AllowSorting = false;
            this.dgUNonsinusoidality2.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgUNonsinusoidality2.DataMember = "";
            this.dgUNonsinusoidality2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgUNonsinusoidality2.Name = "dgUNonsinusoidality2";
            this.dgUNonsinusoidality2.ReadOnly = true;
            this.dgUNonsinusoidality2.Tag = "Несинусоидальность напряжения 2";
            this.dgUNonsinusoidality2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridContextMenu_MouseUp);
            // 
            // lblInterphase
            // 
            resources.ApplyResources(this.lblInterphase, "lblInterphase");
            this.lblInterphase.BackColor = System.Drawing.SystemColors.Menu;
            this.lblInterphase.Name = "lblInterphase";
            // 
            // pqpGraphVoltNS
            // 
            resources.ApplyResources(this.pqpGraphVoltNS, "pqpGraphVoltNS");
            this.pqpGraphVoltNS.BackColor = System.Drawing.Color.White;
            this.pqpGraphVoltNS.Bars.Add(this.barVoltNsNplSt);
            this.pqpGraphVoltNS.Bars.Add(this.barVoltNsUplSt);
            this.pqpGraphVoltNS.Bars.Add(this.barVoltNsPh1UplRes);
            this.pqpGraphVoltNS.Bars.Add(this.barVoltNsPh2UplRes);
            this.pqpGraphVoltNS.Bars.Add(this.barVoltNsPh3UplRes);
            this.pqpGraphVoltNS.BorderColor = System.Drawing.Color.SteelBlue;
            this.pqpGraphVoltNS.Name = "pqpGraphVoltNS";
            // 
            // barVoltNsNplSt
            // 
            this.barVoltNsNplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsNplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barVoltNsNplSt, "barVoltNsNplSt");
            this.barVoltNsNplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsNplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNsNplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsNplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNsNplSt.PercentText = "";
            this.barVoltNsNplSt.WorldBottom = 0F;
            this.barVoltNsNplSt.WorldTop = 1F;
            this.barVoltNsNplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsNplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltNsUplSt
            // 
            this.barVoltNsUplSt.BarBorderColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsUplSt.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(230)))), ((int)(((byte)(189)))));
            resources.ApplyResources(this.barVoltNsUplSt, "barVoltNsUplSt");
            this.barVoltNsUplSt.CaptionColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsUplSt.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNsUplSt.PercentColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsUplSt.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNsUplSt.PercentText = "";
            this.barVoltNsUplSt.WorldBottom = 0F;
            this.barVoltNsUplSt.WorldTop = 1F;
            this.barVoltNsUplSt.WorldValColor = System.Drawing.Color.ForestGreen;
            this.barVoltNsUplSt.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltNsPh1UplRes
            // 
            this.barVoltNsPh1UplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh1UplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barVoltNsPh1UplRes, "barVoltNsPh1UplRes");
            this.barVoltNsPh1UplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh1UplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNsPh1UplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh1UplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNsPh1UplRes.PercentText = "";
            this.barVoltNsPh1UplRes.WorldBottom = 0F;
            this.barVoltNsPh1UplRes.WorldTop = 1F;
            this.barVoltNsPh1UplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh1UplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltNsPh2UplRes
            // 
            this.barVoltNsPh2UplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh2UplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barVoltNsPh2UplRes, "barVoltNsPh2UplRes");
            this.barVoltNsPh2UplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh2UplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNsPh2UplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh2UplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNsPh2UplRes.PercentText = "";
            this.barVoltNsPh2UplRes.WorldBottom = 0F;
            this.barVoltNsPh2UplRes.WorldTop = 1F;
            this.barVoltNsPh2UplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh2UplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // barVoltNsPh3UplRes
            // 
            this.barVoltNsPh3UplRes.BarBorderColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh3UplRes.BarColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.barVoltNsPh3UplRes, "barVoltNsPh3UplRes");
            this.barVoltNsPh3UplRes.CaptionColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh3UplRes.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.barVoltNsPh3UplRes.PercentColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh3UplRes.PercentFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.barVoltNsPh3UplRes.PercentText = "";
            this.barVoltNsPh3UplRes.WorldBottom = 0F;
            this.barVoltNsPh3UplRes.WorldTop = 1F;
            this.barVoltNsPh3UplRes.WorldValColor = System.Drawing.Color.SteelBlue;
            this.barVoltNsPh3UplRes.WorldValFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            // 
            // scFliker
            // 
            resources.ApplyResources(this.scFliker, "scFliker");
            this.scFliker.Name = "scFliker";
            // 
            // scFliker.Panel1
            // 
            resources.ApplyResources(this.scFliker.Panel1, "scFliker.Panel1");
            this.scFliker.Panel1.Controls.Add(this.dgFlicker);
            // 
            // scFliker.Panel2
            // 
            resources.ApplyResources(this.scFliker.Panel2, "scFliker.Panel2");
            this.scFliker.Panel2.Controls.Add(this.dgFlickerLong);
            // 
            // dgFlicker
            // 
            resources.ApplyResources(this.dgFlicker, "dgFlicker");
            this.dgFlicker.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgFlicker.DataMember = "";
            this.dgFlicker.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgFlicker.Name = "dgFlicker";
            this.dgFlicker.ReadOnly = true;
            // 
            // dgFlickerLong
            // 
            resources.ApplyResources(this.dgFlickerLong, "dgFlickerLong");
            this.dgFlickerLong.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgFlickerLong.DataMember = "";
            this.dgFlickerLong.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgFlickerLong.Name = "dgFlickerLong";
            this.dgFlickerLong.ReadOnly = true;
            // 
            // cmsDoc
            // 
            resources.ApplyResources(this.cmsDoc, "cmsDoc");
            this.cmsDoc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.percentToolStripMenuItem,
            this.percentGlToolStripMenuItem,
            this.timeToolStripMenuItem,
            this.numberToolStripMenuItem});
            this.cmsDoc.Name = "cmsDoc";
            // 
            // percentToolStripMenuItem
            // 
            resources.ApplyResources(this.percentToolStripMenuItem, "percentToolStripMenuItem");
            this.percentToolStripMenuItem.Name = "percentToolStripMenuItem";
            this.percentToolStripMenuItem.Click += new System.EventHandler(this.miMeasureNumberAnyVariant_Click);
            // 
            // percentGlToolStripMenuItem
            // 
            resources.ApplyResources(this.percentGlToolStripMenuItem, "percentGlToolStripMenuItem");
            this.percentGlToolStripMenuItem.Name = "percentGlToolStripMenuItem";
            this.percentGlToolStripMenuItem.Click += new System.EventHandler(this.miMeasureNumberAnyVariant_Click);
            // 
            // timeToolStripMenuItem
            // 
            resources.ApplyResources(this.timeToolStripMenuItem, "timeToolStripMenuItem");
            this.timeToolStripMenuItem.Name = "timeToolStripMenuItem";
            this.timeToolStripMenuItem.Click += new System.EventHandler(this.miMeasureNumberAnyVariant_Click);
            // 
            // numberToolStripMenuItem
            // 
            resources.ApplyResources(this.numberToolStripMenuItem, "numberToolStripMenuItem");
            this.numberToolStripMenuItem.Name = "numberToolStripMenuItem";
            this.numberToolStripMenuItem.Click += new System.EventHandler(this.miMeasureNumberAnyVariant_Click);
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabFrequencyDeparture);
            this.tabControl1.Controls.Add(this.tabDips);
            this.tabControl1.Controls.Add(this.tabFValues);
            this.tabControl1.Controls.Add(this.tabU_Deviation);
            this.tabControl1.Controls.Add(this.tabVolValues);
            this.tabControl1.Controls.Add(this.tabVoltageNnbalance);
            this.tabControl1.Controls.Add(this.tabVoltageNonsinusoidality);
            this.tabControl1.Controls.Add(this.tabInterharm);
            this.tabControl1.Controls.Add(this.tabFlickerNum);
            this.tabControl1.Controls.Add(this.tabFliker);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Tag = "";
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabFrequencyDeparture
            // 
            resources.ApplyResources(this.tabFrequencyDeparture, "tabFrequencyDeparture");
            this.tabFrequencyDeparture.Controls.Add(this.splitContainerFreq);
            this.tabFrequencyDeparture.Name = "tabFrequencyDeparture";
            this.tabFrequencyDeparture.UseVisualStyleBackColor = true;
            // 
            // tabDips
            // 
            resources.ApplyResources(this.tabDips, "tabDips");
            this.tabDips.Controls.Add(this.tableLayoutPanel1);
            this.tabDips.Name = "tabDips";
            this.tabDips.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.lblDips, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSwells, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.dgOvers, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblSwells2, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.dgDips2, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.dgOvers2, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.dgDips, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblDips2, 0, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // lblDips
            // 
            resources.ApplyResources(this.lblDips, "lblDips");
            this.lblDips.BackColor = System.Drawing.SystemColors.Menu;
            this.lblDips.Name = "lblDips";
            // 
            // lblSwells
            // 
            resources.ApplyResources(this.lblSwells, "lblSwells");
            this.lblSwells.BackColor = System.Drawing.SystemColors.Menu;
            this.lblSwells.Name = "lblSwells";
            // 
            // dgOvers
            // 
            resources.ApplyResources(this.dgOvers, "dgOvers");
            this.dgOvers.AllowSorting = false;
            this.dgOvers.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgOvers.DataMember = "";
            this.dgOvers.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgOvers.Name = "dgOvers";
            this.dgOvers.ReadOnly = true;
            this.dgOvers.Tag = "Перенапряжения";
            // 
            // lblSwells2
            // 
            resources.ApplyResources(this.lblSwells2, "lblSwells2");
            this.lblSwells2.BackColor = System.Drawing.SystemColors.Menu;
            this.lblSwells2.Name = "lblSwells2";
            // 
            // dgDips2
            // 
            resources.ApplyResources(this.dgDips2, "dgDips2");
            this.dgDips2.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgDips2.DataMember = "";
            this.dgDips2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgDips2.Name = "dgDips2";
            this.dgDips2.ReadOnly = true;
            // 
            // dgOvers2
            // 
            resources.ApplyResources(this.dgOvers2, "dgOvers2");
            this.dgOvers2.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgOvers2.DataMember = "";
            this.dgOvers2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgOvers2.Name = "dgOvers2";
            this.dgOvers2.ReadOnly = true;
            // 
            // dgDips
            // 
            resources.ApplyResources(this.dgDips, "dgDips");
            this.dgDips.AllowSorting = false;
            this.dgDips.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgDips.DataMember = "";
            this.dgDips.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgDips.Name = "dgDips";
            this.dgDips.ReadOnly = true;
            this.dgDips.Tag = "Провалы";
            // 
            // lblDips2
            // 
            resources.ApplyResources(this.lblDips2, "lblDips2");
            this.lblDips2.BackColor = System.Drawing.SystemColors.Menu;
            this.lblDips2.Name = "lblDips2";
            // 
            // tabFValues
            // 
            resources.ApplyResources(this.tabFValues, "tabFValues");
            this.tabFValues.Controls.Add(this.dgFValues);
            this.tabFValues.Name = "tabFValues";
            this.tabFValues.UseVisualStyleBackColor = true;
            // 
            // dgFValues
            // 
            resources.ApplyResources(this.dgFValues, "dgFValues");
            this.dgFValues.AllowSorting = false;
            this.dgFValues.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgFValues.DataMember = "";
            this.dgFValues.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgFValues.Name = "dgFValues";
            this.dgFValues.ReadOnly = true;
            // 
            // tabU_Deviation
            // 
            resources.ApplyResources(this.tabU_Deviation, "tabU_Deviation");
            this.tabU_Deviation.Controls.Add(this.splitContainerVolt);
            this.tabU_Deviation.Name = "tabU_Deviation";
            this.tabU_Deviation.UseVisualStyleBackColor = true;
            // 
            // tabVolValues
            // 
            resources.ApplyResources(this.tabVolValues, "tabVolValues");
            this.tabVolValues.Controls.Add(this.dgVolValues);
            this.tabVolValues.Name = "tabVolValues";
            this.tabVolValues.UseVisualStyleBackColor = true;
            // 
            // dgVolValues
            // 
            resources.ApplyResources(this.dgVolValues, "dgVolValues");
            this.dgVolValues.AllowSorting = false;
            this.dgVolValues.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgVolValues.DataMember = "";
            this.dgVolValues.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgVolValues.Name = "dgVolValues";
            this.dgVolValues.ReadOnly = true;
            // 
            // tabVoltageNnbalance
            // 
            resources.ApplyResources(this.tabVoltageNnbalance, "tabVoltageNnbalance");
            this.tabVoltageNnbalance.Controls.Add(this.splitContainerVoltHarm0);
            this.tabVoltageNnbalance.Name = "tabVoltageNnbalance";
            this.tabVoltageNnbalance.UseVisualStyleBackColor = true;
            // 
            // tabVoltageNonsinusoidality
            // 
            resources.ApplyResources(this.tabVoltageNonsinusoidality, "tabVoltageNonsinusoidality");
            this.tabVoltageNonsinusoidality.Controls.Add(this.scVolHarmonics);
            this.tabVoltageNonsinusoidality.Name = "tabVoltageNonsinusoidality";
            this.tabVoltageNonsinusoidality.UseVisualStyleBackColor = true;
            // 
            // tabInterharm
            // 
            resources.ApplyResources(this.tabInterharm, "tabInterharm");
            this.tabInterharm.Controls.Add(this.dgInterharm);
            this.tabInterharm.Name = "tabInterharm";
            this.tabInterharm.UseVisualStyleBackColor = true;
            // 
            // dgInterharm
            // 
            resources.ApplyResources(this.dgInterharm, "dgInterharm");
            this.dgInterharm.AllowSorting = false;
            this.dgInterharm.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgInterharm.DataMember = "";
            this.dgInterharm.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgInterharm.Name = "dgInterharm";
            this.dgInterharm.ReadOnly = true;
            this.dgInterharm.Tag = "Интергармоники";
            // 
            // tabFlickerNum
            // 
            resources.ApplyResources(this.tabFlickerNum, "tabFlickerNum");
            this.tabFlickerNum.Controls.Add(this.dgFlickerNum);
            this.tabFlickerNum.Name = "tabFlickerNum";
            this.tabFlickerNum.UseVisualStyleBackColor = true;
            // 
            // dgFlickerNum
            // 
            resources.ApplyResources(this.dgFlickerNum, "dgFlickerNum");
            this.dgFlickerNum.AllowSorting = false;
            this.dgFlickerNum.CaptionBackColor = System.Drawing.Color.Silver;
            this.dgFlickerNum.DataMember = "";
            this.dgFlickerNum.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgFlickerNum.Name = "dgFlickerNum";
            this.dgFlickerNum.ReadOnly = true;
            this.dgFlickerNum.Tag = "Несимметрия напряжений";
            this.dgFlickerNum.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridContextMenu_MouseUp);
            // 
            // tabFliker
            // 
            resources.ApplyResources(this.tabFliker, "tabFliker");
            this.tabFliker.Controls.Add(this.scFliker);
            this.tabFliker.Name = "tabFliker";
            this.tabFliker.UseVisualStyleBackColor = true;
            // 
            // lbl_EPI_Caption
            // 
            resources.ApplyResources(this.lbl_EPI_Caption, "lbl_EPI_Caption");
            this.lbl_EPI_Caption.Name = "lbl_EPI_Caption";
            // 
            // btnMaxMode
            // 
            resources.ApplyResources(this.btnMaxMode, "btnMaxMode");
            this.btnMaxMode.Name = "btnMaxMode";
            this.btnMaxMode.UseVisualStyleBackColor = true;
            this.btnMaxMode.Click += new System.EventHandler(this.btnMaxMode_Click);
            // 
            // frmDocPQPMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CloseButton = false;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.lbl_EPI_Caption);
            this.Controls.Add(this.btnMaxMode);
            this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.Document)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.HideOnClose = true;
            this.Name = "frmDocPQPMain";
            this.ShowHint = WeifenLuo.WinFormsUI.DockState.Document;
            this.TransparencyKey = System.Drawing.Color.White;
            this.splitContainerFreq.Panel1.ResumeLayout(false);
            this.splitContainerFreq.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerFreq)).EndInit();
            this.splitContainerFreq.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgFrequencyDeparture)).EndInit();
            this.splitContainerVolt.Panel1.ResumeLayout(false);
            this.splitContainerVolt.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerVolt)).EndInit();
            this.splitContainerVolt.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgU_Deviation)).EndInit();
            this.splitContainerVoltHarm0.Panel1.ResumeLayout(false);
            this.splitContainerVoltHarm0.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerVoltHarm0)).EndInit();
            this.splitContainerVoltHarm0.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgNonSymmetry)).EndInit();
            this.scVolHarmonics.Panel1.ResumeLayout(false);
            this.scVolHarmonics.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scVolHarmonics)).EndInit();
            this.scVolHarmonics.ResumeLayout(false);
            this.splitContainerVoltHarm1.Panel1.ResumeLayout(false);
            this.splitContainerVoltHarm1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerVoltHarm1)).EndInit();
            this.splitContainerVoltHarm1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgUNonsinusoidality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgUNonsinusoidality2)).EndInit();
            this.scFliker.Panel1.ResumeLayout(false);
            this.scFliker.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scFliker)).EndInit();
            this.scFliker.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgFlicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgFlickerLong)).EndInit();
            this.cmsDoc.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabFrequencyDeparture.ResumeLayout(false);
            this.tabDips.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgOvers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDips2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgOvers2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDips)).EndInit();
            this.tabFValues.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgFValues)).EndInit();
            this.tabU_Deviation.ResumeLayout(false);
            this.tabVolValues.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgVolValues)).EndInit();
            this.tabVoltageNnbalance.ResumeLayout(false);
            this.tabVoltageNonsinusoidality.ResumeLayout(false);
            this.tabInterharm.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgInterharm)).EndInit();
            this.tabFlickerNum.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgFlickerNum)).EndInit();
            this.tabFliker.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		#region Frequency Departure Grid

		/// <summary>
		/// Filling Frequency Departure Grid with data
		/// </summary>
		public void drawdgFrequencyDeparture()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					string query_text = "SELECT p.name, dapt2.num_nrm_rng, dapt2.num_max_rng, dapt2.num_out_max_rng, dapt2.real_nrm_rng_top, dapt2.real_max_rng_top, dapt2.real_nrm_rng_bottom, dapt2.real_max_rng_bottom, dapt2.calc_nrm_rng_top, dapt2.calc_max_rng_top, dapt2.calc_nrm_rng_bottom, dapt2.calc_max_rng_bottom FROM parameters p, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt2.param_id AND dapt2.param_id = 1001 AND dapt2.datetime_id = " + curDatetimeId_ + ";";

					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query_text, "day_avg_parameters_t2", ref ds);

					try
					{
						Int64 coef = 20 * TimeSpan.TicksPerSecond;

						// adding calc-fields to the dataset
						DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng", System.Type.GetType("System.Single"));
						c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_nrm_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
						c = ds.Tables[0].Columns.Add("prcnt_max_rng", System.Type.GetType("System.Single"));
						c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_max_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
						c = ds.Tables[0].Columns.Add("prcnt_out_max_rng", System.Type.GetType("System.Single"));
						c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_out_max_rng * 100/ (num_nrm_rng + num_max_rng + num_out_max_rng))";

						Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
						c = ds.Tables[0].Columns.Add("time_nrm_rng", System.Type.GetType("System.Int64"));
						c.Expression =
							"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_nrm_rng / (1 /" +
							coef.ToString() +
							"))";

						//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						//timeDuration.ToString() + 
						//	" * (num_nrm_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";

						c = ds.Tables[0].Columns.Add("time_max_rng", System.Type.GetType("System.Int64"));
						c.Expression =
							"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_max_rng / (1 /" +
							coef.ToString() +
							"))";

						//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_max_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";

						c = ds.Tables[0].Columns.Add("time_out_max_rng", System.Type.GetType("System.Int64"));
						c.Expression =
							"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_out_max_rng / (1 /" +
							coef.ToString() +
							"))";

						//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_out_max_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";
					}
					catch (Exception inner_e)
					{
						EmService.DumpException(inner_e, "drawdgFrequencyDeparture() 1");
					}

					// binding dataset with datagrid
					dgFrequencyDeparture.SetDataBinding(ds, "day_avg_parameters_t2");
				}
				else   // Et-PQP-A
				{
					//NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT p.name, f.num_all, f.num_synchro, f.num_not_synchro, f.num_nrm_rng, f.num_max_rng, f.num_out_max_rng, f.real_nrm_rng_top_syn, f.real_max_rng_top_syn, f.real_nrm_rng_bottom_syn, f.real_max_rng_bottom_syn, f.real_nrm_rng_top_iso, f.real_max_rng_top_iso, f.real_nrm_rng_bottom_iso, f.real_max_rng_bottom_iso, f.calc_nrm_rng_top, f.calc_max_rng_top, f.calc_nrm_rng_bottom, f.calc_max_rng_bottom FROM parameters p, pqp_f f WHERE p.param_id = f.param_id AND f.param_id = 1001 AND f.datetime_id = " + curDatetimeId_ + ";", conEmDb);

					string query_text = string.Format(@"SELECT p.name, f.num_all, f.num_synchro, f.num_not_synchro, f.num_nrm_rng, f.num_max_rng, f.num_out_max_rng, f.real_nrm_rng_top_syn, f.real_max_rng_top_syn, f.real_nrm_rng_bottom_syn, f.real_max_rng_bottom_syn, f.real_nrm_rng_top_iso, f.real_max_rng_top_iso, f.real_nrm_rng_bottom_iso, f.real_max_rng_bottom_iso,  
CASE WHEN f.valid_f != 0 then cast(round(cast(f.calc_nrm_rng_top as numeric), {0}) as text) else '-' end as calc_nrm_rng_top,
CASE WHEN f.valid_f != 0 then cast(round(cast(f.calc_max_rng_top as numeric), {0}) as text) else '-' end as calc_max_rng_top,
CASE WHEN f.valid_f != 0 then cast(round(cast(f.calc_nrm_rng_bottom as numeric), {0}) as text) else '-' end as calc_nrm_rng_bottom,
CASE WHEN f.valid_f != 0 then cast(round(cast(f.calc_max_rng_bottom as numeric), {0}) as text) else '-' end as calc_max_rng_bottom  
FROM parameters p, pqp_f f WHERE p.param_id = f.param_id AND f.param_id = 1001 AND f.datetime_id = {1};", settings_.FloatSigns, curDatetimeId_);

					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query_text, "pqp_f", ref ds);

					try
					{
						Int64 coef = 10 * TimeSpan.TicksPerSecond;

						// adding calc-fields to the dataset
						DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng", System.Type.GetType("System.Single"));
						c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_nrm_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
						c = ds.Tables[0].Columns.Add("prcnt_max_rng", System.Type.GetType("System.Single"));
						c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_max_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
						c = ds.Tables[0].Columns.Add("prcnt_out_max_rng", System.Type.GetType("System.Single"));
						c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_out_max_rng * 100/ (num_nrm_rng + num_max_rng + num_out_max_rng))";

						Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
						c = ds.Tables[0].Columns.Add("time_nrm_rng", System.Type.GetType("System.Int64"));
						c.Expression =
							"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_nrm_rng / (1 /" +
							coef.ToString() +
							"))";

						//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						//timeDuration.ToString() + 
						//	" * (num_nrm_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";

						c = ds.Tables[0].Columns.Add("time_max_rng", System.Type.GetType("System.Int64"));
						c.Expression =
							"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_max_rng / (1 /" +
							coef.ToString() +
							"))";

						c = ds.Tables[0].Columns.Add("time_out_max_rng", System.Type.GetType("System.Int64"));
						c.Expression =
							"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
							"num_out_max_rng / (1 /" +
							coef.ToString() +
							"))";
					}
					catch (Exception inner_e)
					{
						EmService.DumpException(inner_e, "drawdgFrequencyDeparture() 1-A");
					}

					// приводим верхние и нижние df в такой же вид как остальные поля
					// (их формат изменился после превращения их в текст)
					for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
					{
						DataRow curRow = ds.Tables[0].Rows[iRow];
						string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")].ToString();
						float f_val;
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_bottom")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_bottom")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_bottom")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_top")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_top")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_top")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_bottom")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_bottom")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_bottom")] = "-";
					}

					// binding dataset with datagrid
					dgFrequencyDeparture.SetDataBinding(ds, "pqp_f");
				}

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgFrequencyDeparture.DataSource, dgFrequencyDeparture.DataMember];

				currencyManager.CurrentChanged += new EventHandler(currencyManager_FreqChanged);

				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgFrequencyDeparture() 2");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		void currencyManager_FreqChanged(object sender, EventArgs e)
		{
			try
			{
				int ipos = dgFrequencyDeparture.CurrentRowIndex;
				if (ipos == -1)
				{
					barFreqNplSt.WorldTop = float.NaN;
					barFreqNplSt.WorldBottom = float.NaN;
					barFreqUplSt.WorldTop = float.NaN;
					barFreqUplSt.WorldBottom = float.NaN;
					barFreqNplRes.WorldTop = float.NaN;
					barFreqNplRes.WorldBottom = float.NaN;
					barFreqUplRes.WorldTop = float.NaN;
					barFreqUplRes.WorldBottom = float.NaN;
					pqpGraph_dF.Invalidate();
					return;
				}
				string pctf = "{0}%";
				GridColumnStylesCollection st = dgFrequencyDeparture.TableStyles[0].GridColumnStyles;

				float NPL_pct_real, NPL_top_real, NPL_bottom_real;
				Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["prcnt_nrm_rng"])/*4*/], out NPL_pct_real);
				if(curDeviceType_ != EmDeviceType.ETPQP_A)
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_nrm_rng_top"])/*14*/], out NPL_top_real);
				else
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_nrm_rng_top_syn"])], out NPL_top_real);
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					Conversions.object_2_float_en_ru(
						dgFrequencyDeparture[ipos, st.IndexOf(st["real_nrm_rng_bottom"])/*16*/], 
						out NPL_bottom_real);
				else
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_nrm_rng_bottom_syn"])],
					out NPL_bottom_real);

				barFreqNplSt.WorldTop = NPL_top_real;
				barFreqNplSt.WorldBottom = NPL_bottom_real;
				barFreqNplSt.PercentText = string.Format(pctf, NPL_pct_real);

				float UPL_pct_real, UPL_top_real, UPL_bottom_real;
				Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["prcnt_max_rng"])/*5*/], out UPL_pct_real);
				UPL_pct_real += NPL_pct_real;
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_max_rng_top"])/*15*/], out UPL_top_real);
				else
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_max_rng_top_syn"])], out UPL_top_real);
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_max_rng_bottom"])/*17*/], 
					out UPL_bottom_real);
				else
					Conversions.object_2_float_en_ru(
					dgFrequencyDeparture[ipos, st.IndexOf(st["real_max_rng_bottom_syn"])],
					out UPL_bottom_real);

				barFreqUplSt.WorldTop = UPL_top_real;
				barFreqUplSt.WorldBottom = UPL_bottom_real;
				barFreqUplSt.PercentText = string.Format(pctf, UPL_pct_real);

				//float NPL_pct_calc = 95F;			
				//float NPL_top_calc = (float)dgFrequencyDeparture[ipos, 10];
				//float NPL_bottom_calc = (float)dgFrequencyDeparture[ipos, 12];

				//barFreqNplRes.WorldTop = NPL_top_calc;
				//barFreqNplRes.WorldBottom = NPL_bottom_calc;
				//barFreqNplRes.PercentText = string.Format(pctf, NPL_pct_calc);

				//float UPL_pct_calc = 100F;
				//float UPL_top_calc = (float)dgFrequencyDeparture[ipos, 11];
				//float UPL_bottom_calc = (float)dgFrequencyDeparture[ipos, 13];

				//barFreqUplRes.WorldTop = UPL_top_calc;
				//barFreqUplRes.WorldBottom = UPL_bottom_calc;
				//barFreqUplRes.PercentText = string.Format(pctf, UPL_pct_calc);

				float NPL_pct_calc = 95F;
				float NPL_top_calc = float.NaN;
				if (!(dgFrequencyDeparture[ipos, st.IndexOf(st["calc_nrm_rng_top"])/*10*/] is DBNull))
					Conversions.object_2_float_en_ru(
						dgFrequencyDeparture[ipos, st.IndexOf(st["calc_nrm_rng_top"])/*10*/], 
						out NPL_top_calc);
				float NPL_bottom_calc = float.NaN;
				if (!(dgFrequencyDeparture[ipos, st.IndexOf(st["calc_nrm_rng_bottom"])/*12*/] is DBNull))
					Conversions.object_2_float_en_ru(
						dgFrequencyDeparture[ipos, st.IndexOf(st["calc_nrm_rng_bottom"])/*12*/], 
						out NPL_bottom_calc);

				barFreqNplRes.WorldTop = NPL_top_calc;
				barFreqNplRes.WorldBottom = NPL_bottom_calc;
				barFreqNplRes.PercentText = string.Format(pctf, Math.Round(NPL_pct_calc, 2));

				float UPL_pct_calc = 100F;
				float UPL_top_calc = float.NaN;
				if (!(dgFrequencyDeparture[ipos, st.IndexOf(st["calc_max_rng_top"])/*11*/] is DBNull))
					Conversions.object_2_float_en_ru(dgFrequencyDeparture[ipos, 
						st.IndexOf(st["calc_max_rng_top"])/*11*/], out UPL_top_calc);
				float UPL_bottom_calc = float.NaN;
				if (!(dgFrequencyDeparture[ipos, st.IndexOf(st["calc_max_rng_bottom"])/*13*/] is DBNull))
					Conversions.object_2_float_en_ru(dgFrequencyDeparture[ipos, 
						st.IndexOf(st["calc_max_rng_bottom"])/*13*/], out UPL_bottom_calc);

				barFreqUplRes.WorldTop = UPL_top_calc;
				barFreqUplRes.WorldBottom = UPL_bottom_calc;
				barFreqUplRes.PercentText = string.Format(pctf, Math.Round(UPL_pct_calc, 2));

				pqpGraph_dF.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in currencyManager_FreqChanged()");
				throw;
			}
		}

		/// <summary>
		/// Repainting PQP Graphs
		/// </summary>
		public void UpdatePQPGraphs()
		{
			// страница Frequency deviation
			currencyManager_FreqChanged(this, EventArgs.Empty);

			// страница Voltage deviation
			currencyManager_VoltChanged(this, EventArgs.Empty);

			// страница Voltage unbalance
			currencyManager_VoltUnbChanged(this, EventArgs.Empty);

			// страница Voltage harmonics
			currencyManager_VoltNSChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgFrequencyDepartureInitTableStyle()
		{
			try
			{
				dgFrequencyDeparture.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				DataGridGroupCaption caption0 =
					new DataGridGroupCaption(rm.GetString("name_columns_parameter"), 1);
				//int num_col_count = (curDeviceType_ == EmDeviceType.ETPQP_A) ? 6 : 3;
				DataGridGroupCaption caption6 =
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number"),
					3/*, Color.Beige*/);
				DataGridGroupCaption caption1 =
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number"),
					3/*, Color.Beige*/);
				DataGridGroupCaption caption2 =
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number") +
					", %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption3 =
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number") +
					", " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);

				DataGridGroupCaption caption4 =
					new DataGridGroupCaption(rm.GetString("name_column_measures_result") +
					", " + rm.GetString("name.hertz.short"), 4 /*, Color.Honeydew*/);
				DataGridGroupCaption caption5 =
					new DataGridGroupCaption(rm.GetString("name_columns_standard_value") +
					", " + rm.GetString("name.hertz.short"), 4/*, Color.Honeydew*/);
				DataGridGroupCaption caption7 =
					new DataGridGroupCaption(rm.GetString("name_columns_standard_value") +
					", " + rm.GetString("name.hertz.short"), 4/*, Color.Honeydew*/);

				// p.name		
				DataGridColumnCellFormula cs_fd_name = new DataGridColumnCellFormula();
				cs_fd_name.GroupCaption = caption0;
				cs_fd_name.GroupIndex = 0;
				cs_fd_name.HeaderText = "";
				cs_fd_name.MappingName = "name";
				cs_fd_name.BackgroungColor = DataGridColors.ColorPqpParam;

				//short curColumnNumber = 0;
				DataGridColumnGroupCaption cs_fd_num_all = null;
				DataGridColumnGroupCaption cs_fd_num_all_syn = null;
				DataGridColumnGroupCaption cs_fd_num_all_nsyn = null;
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					// num_all
					cs_fd_num_all = new DataGridColumnGroupCaption(caption6, 0);
					cs_fd_num_all.HeaderText = rm.GetString("name_columns_num_all");
					cs_fd_num_all.MappingName = "num_all";
					cs_fd_num_all.Format = DataColumnsFormat.FloatShortFormat;
					cs_fd_num_all.BackgroungColor = DataGridColors.ColorCommon;

					// num_all synchro
					cs_fd_num_all_syn = new DataGridColumnGroupCaption(caption6, 1);
					cs_fd_num_all_syn.HeaderText = rm.GetString("name_columns_num_synchro");
					cs_fd_num_all_syn.MappingName = "num_synchro";
					cs_fd_num_all_syn.Format = DataColumnsFormat.FloatShortFormat;
					cs_fd_num_all_syn.BackgroungColor = DataGridColors.ColorCommon;

					// num_all not synchro
					cs_fd_num_all_nsyn = new DataGridColumnGroupCaption(caption6, 2);
					cs_fd_num_all_nsyn.HeaderText = rm.GetString("name_columns_num_nsynchro");
					cs_fd_num_all_nsyn.MappingName = "num_not_synchro";
					cs_fd_num_all_nsyn.Format = DataColumnsFormat.FloatShortFormat;
					cs_fd_num_all_nsyn.BackgroungColor = DataGridColors.ColorCommon;
				}

				// dapt2.num_nrm_rng
				DataGridColumnGroupCaption cs_fd_num_nrm_rng = new DataGridColumnGroupCaption(caption1, 0);
				cs_fd_num_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_fd_num_nrm_rng.MappingName = "num_nrm_rng";
				cs_fd_num_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_fd_num_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;

				// dapt2.num_max_rng
				DataGridColumnGroupCaption cs_fd_num_max_rng = new DataGridColumnGroupCaption(caption1, 1);
				cs_fd_num_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_fd_num_max_rng.MappingName = "num_max_rng";
				cs_fd_num_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_fd_num_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_fd_num_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.num_out_max_rng
				DataGridColumnGroupCaption cs_fd_num_out_max_rng = new DataGridColumnGroupCaption(caption1, 2);
				cs_fd_num_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_fd_num_out_max_rng.MappingName = "num_out_max_rng";
				cs_fd_num_out_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_fd_num_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_fd_num_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.prcnt_nrm_rng			
				DataGridColumnGroupCaption cs_fd_prcnt_nrm_rng = new DataGridColumnGroupCaption(caption2, 0);
				cs_fd_prcnt_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_fd_prcnt_nrm_rng.MappingName = "prcnt_nrm_rng";
				cs_fd_prcnt_nrm_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_fd_prcnt_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;

				// dapt2.prcnt_max_rng
				DataGridColumnGroupCaption cs_fd_prcnt_max_rng = new DataGridColumnGroupCaption(caption2, 1);
				cs_fd_prcnt_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_fd_prcnt_max_rng.MappingName = "prcnt_max_rng";
				cs_fd_prcnt_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_fd_prcnt_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_fd_prcnt_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.prcnt_out_max_rng
				DataGridColumnGroupCaption cs_fd_prcnt_out_max_rng = new DataGridColumnGroupCaption(caption2, 2);
				cs_fd_prcnt_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_fd_prcnt_out_max_rng.MappingName = "prcnt_out_max_rng";
				cs_fd_prcnt_out_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_fd_prcnt_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_fd_prcnt_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.time_nrm_rng
				DataGridColumnTimespan cs_fd_time_nrm_rng = new DataGridColumnTimespan(caption3, 0);
				cs_fd_time_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_fd_time_nrm_rng.MappingName = "time_nrm_rng";
				cs_fd_time_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;
				//cs_fd_time_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;

				// dapt2.time_max_rng
				DataGridColumnTimespan cs_fd_time_max_rng = new DataGridColumnTimespan(caption3, 1);
				cs_fd_time_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_fd_time_max_rng.MappingName = "time_max_rng";
				cs_fd_time_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				//cs_fd_time_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_fd_time_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.time_out_max_rng
				DataGridColumnTimespan cs_fd_time_out_max_rng = new DataGridColumnTimespan(caption3, 2);
				cs_fd_time_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_fd_time_out_max_rng.MappingName = "time_out_max_rng";
				cs_fd_time_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				//cs_fd_time_out_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_fd_time_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.calc_nrm_rng_top
				DataGridColumnGroupCaption cs_fd_calc_nrm_rng_top = new DataGridColumnGroupCaption(caption4, 0);
				cs_fd_calc_nrm_rng_top.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_fd_calc_nrm_rng_top.MappingName = "calc_nrm_rng_top";
				cs_fd_calc_nrm_rng_top.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_fd_calc_nrm_rng_top.Format = DataColumnsFormat.FloatShortFormat;

				// dapt2.calc_max_rng_top
				DataGridColumnGroupCaption cs_fd_calc_max_rng_top = new DataGridColumnGroupCaption(caption4, 1);
				cs_fd_calc_max_rng_top.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_fd_calc_max_rng_top.MappingName = "calc_max_rng_top";
				cs_fd_calc_max_rng_top.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_fd_calc_max_rng_top.Format = DataColumnsFormat.FloatShortFormat;

				// dapt2.calc_nrm_rng_bottom
				DataGridColumnGroupCaption cs_fd_calc_nrm_rng_bottom = new DataGridColumnGroupCaption(caption4, 2);
				cs_fd_calc_nrm_rng_bottom.HeaderText = rm.GetString("name.columns.result.calc.npl.minus");
				cs_fd_calc_nrm_rng_bottom.MappingName = "calc_nrm_rng_bottom";
				cs_fd_calc_nrm_rng_bottom.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_fd_calc_nrm_rng_bottom.Format = DataColumnsFormat.FloatShortFormat;

				// dapt2.calc_max_rng_bottom
				DataGridColumnGroupCaption cs_fd_calc_max_rng_bottom = new DataGridColumnGroupCaption(caption4, 3);
				cs_fd_calc_max_rng_bottom.HeaderText = rm.GetString("name.columns.result.calc.upl.minus");
				cs_fd_calc_max_rng_bottom.MappingName = "calc_max_rng_bottom";
				cs_fd_calc_max_rng_bottom.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_fd_calc_max_rng_bottom.Format = DataColumnsFormat.FloatShortFormat;

				DataGridColumnGroupCaption cs_fd_real_nrm_rng_top = null;
				DataGridColumnGroupCaption cs_fd_real_max_rng_top = null;
				DataGridColumnGroupCaption cs_fd_real_nrm_rng_bottom = null;
				DataGridColumnGroupCaption cs_fd_real_max_rng_bottom = null;
				DataGridColumnGroupCaption cs_fd_real_nrm_rng_top_syn = null;
				DataGridColumnGroupCaption cs_fd_real_max_rng_top_syn = null;
				DataGridColumnGroupCaption cs_fd_real_nrm_rng_bottom_syn = null;
				DataGridColumnGroupCaption cs_fd_real_max_rng_bottom_syn = null;
				DataGridColumnGroupCaption cs_fd_real_nrm_rng_top_iso = null;
				DataGridColumnGroupCaption cs_fd_real_max_rng_top_iso = null;
				DataGridColumnGroupCaption cs_fd_real_nrm_rng_bottom_iso = null;
				DataGridColumnGroupCaption cs_fd_real_max_rng_bottom_iso = null;
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					// dapt2.real_nrm_rng_top
					cs_fd_real_nrm_rng_top = new DataGridColumnGroupCaption(caption5, 0);
					cs_fd_real_nrm_rng_top.HeaderText = rm.GetString("name.columns.result.real.npl.plus");
					cs_fd_real_nrm_rng_top.MappingName = "real_nrm_rng_top";
					cs_fd_real_nrm_rng_top.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_nrm_rng_top.Format = DataColumnsFormat.FloatShortFormat;

					// dapt1.real_max_rng_top
					cs_fd_real_max_rng_top = new DataGridColumnGroupCaption(caption5, 1);
					cs_fd_real_max_rng_top.HeaderText = rm.GetString("name.columns.result.real.upl.plus");
					cs_fd_real_max_rng_top.MappingName = "real_max_rng_top";
					cs_fd_real_max_rng_top.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_max_rng_top.Format = DataColumnsFormat.FloatShortFormat;

					// dapt2.real_nrm_rng_top_bottom
					cs_fd_real_nrm_rng_bottom = new DataGridColumnGroupCaption(caption5, 2);
					cs_fd_real_nrm_rng_bottom.HeaderText = rm.GetString("name.columns.result.real.npl.minus");
					cs_fd_real_nrm_rng_bottom.MappingName = "real_nrm_rng_bottom";
					cs_fd_real_nrm_rng_bottom.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_nrm_rng_bottom.Format = DataColumnsFormat.FloatShortFormat;

					// dapt1.real_max_rng_bottom
					cs_fd_real_max_rng_bottom = new DataGridColumnGroupCaption(caption5, 3);
					cs_fd_real_max_rng_bottom.HeaderText = rm.GetString("name.columns.result.real.upl.minus");
					cs_fd_real_max_rng_bottom.MappingName = "real_max_rng_bottom";
					cs_fd_real_max_rng_bottom.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_max_rng_bottom.Format = DataColumnsFormat.FloatShortFormat;
				}
				else
				{
					// dapt2.real_nrm_rng_top
					cs_fd_real_nrm_rng_top_syn = new DataGridColumnGroupCaption(caption5, 0);
					cs_fd_real_nrm_rng_top_syn.HeaderText = rm.GetString("name_columns_res_real_npl_plus_syn");
					cs_fd_real_nrm_rng_top_syn.MappingName = "real_nrm_rng_top_syn";
					cs_fd_real_nrm_rng_top_syn.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_nrm_rng_top_syn.Format = DataColumnsFormat.FloatShortFormat;

					// dapt1.real_max_rng_top
					cs_fd_real_max_rng_top_syn = new DataGridColumnGroupCaption(caption5, 1);
					cs_fd_real_max_rng_top_syn.HeaderText = rm.GetString("name_columns_res_real_upl_plus_syn");
					cs_fd_real_max_rng_top_syn.MappingName = "real_max_rng_top_syn";
					cs_fd_real_max_rng_top_syn.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_max_rng_top_syn.Format = DataColumnsFormat.FloatShortFormat;

					// dapt2.real_nrm_rng_top_bottom
					cs_fd_real_nrm_rng_bottom_syn = new DataGridColumnGroupCaption(caption5, 2);
					cs_fd_real_nrm_rng_bottom_syn.HeaderText = rm.GetString("name_columns_res_real_npl_minus_syn");
					cs_fd_real_nrm_rng_bottom_syn.MappingName = "real_nrm_rng_bottom_syn";
					cs_fd_real_nrm_rng_bottom_syn.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_nrm_rng_bottom_syn.Format = DataColumnsFormat.FloatShortFormat;

					// dapt1.real_max_rng_bottom
					cs_fd_real_max_rng_bottom_syn = new DataGridColumnGroupCaption(caption5, 3);
					cs_fd_real_max_rng_bottom_syn.HeaderText = rm.GetString("name_columns_res_real_upl_minus_syn");
					cs_fd_real_max_rng_bottom_syn.MappingName = "real_max_rng_bottom_syn";
					cs_fd_real_max_rng_bottom_syn.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_max_rng_bottom_syn.Format = DataColumnsFormat.FloatShortFormat;

					// dapt2.real_nrm_rng_top
					cs_fd_real_nrm_rng_top_iso = new DataGridColumnGroupCaption(caption7, 0);
					cs_fd_real_nrm_rng_top_iso.HeaderText = rm.GetString("name_columns_res_real_npl_plus_iso");
					cs_fd_real_nrm_rng_top_iso.MappingName = "real_nrm_rng_top_iso";
					cs_fd_real_nrm_rng_top_iso.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_nrm_rng_top_iso.Format = DataColumnsFormat.FloatShortFormat;

					// dapt1.real_max_rng_top
					cs_fd_real_max_rng_top_iso = new DataGridColumnGroupCaption(caption7, 1);
					cs_fd_real_max_rng_top_iso.HeaderText = rm.GetString("name_columns_res_real_upl_plus_iso");
					cs_fd_real_max_rng_top_iso.MappingName = "real_max_rng_top_iso";
					cs_fd_real_max_rng_top_iso.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_max_rng_top_iso.Format = DataColumnsFormat.FloatShortFormat;

					// dapt2.real_nrm_rng_top_bottom
					cs_fd_real_nrm_rng_bottom_iso = new DataGridColumnGroupCaption(caption7, 2);
					cs_fd_real_nrm_rng_bottom_iso.HeaderText = rm.GetString("name_columns_res_real_npl_minus_iso");
					cs_fd_real_nrm_rng_bottom_iso.MappingName = "real_nrm_rng_bottom_iso";
					cs_fd_real_nrm_rng_bottom_iso.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_nrm_rng_bottom_iso.Format = DataColumnsFormat.FloatShortFormat;

					// dapt1.real_max_rng_bottom
					cs_fd_real_max_rng_bottom_iso = new DataGridColumnGroupCaption(caption7, 3);
					cs_fd_real_max_rng_bottom_iso.HeaderText = rm.GetString("name_columns_res_real_upl_minus_iso");
					cs_fd_real_max_rng_bottom_iso.MappingName = "real_max_rng_bottom_iso";
					cs_fd_real_max_rng_bottom_iso.BackgroungColor = DataGridColors.ColorPkeStandard;
					cs_fd_real_max_rng_bottom_iso.Format = DataColumnsFormat.FloatShortFormat;
				}

				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t2";
				else ts.MappingName = "pqp_f";

				ts.GridColumnStyles.Add(cs_fd_name);

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_fd_num_all);
					ts.GridColumnStyles.Add(cs_fd_num_all_syn);
					ts.GridColumnStyles.Add(cs_fd_num_all_nsyn);
				}

				ts.GridColumnStyles.Add(cs_fd_num_nrm_rng);
				ts.GridColumnStyles.Add(cs_fd_num_max_rng);
				ts.GridColumnStyles.Add(cs_fd_num_out_max_rng);

				ts.GridColumnStyles.Add(cs_fd_prcnt_nrm_rng);
				ts.GridColumnStyles.Add(cs_fd_prcnt_max_rng);
				ts.GridColumnStyles.Add(cs_fd_prcnt_out_max_rng);

				ts.GridColumnStyles.Add(cs_fd_time_nrm_rng);
				ts.GridColumnStyles.Add(cs_fd_time_max_rng);
				ts.GridColumnStyles.Add(cs_fd_time_out_max_rng);					//9

				ts.GridColumnStyles.Add(cs_fd_calc_nrm_rng_top);					// - /10
				ts.GridColumnStyles.Add(cs_fd_calc_max_rng_top);					// 10/11
				ts.GridColumnStyles.Add(cs_fd_calc_nrm_rng_bottom);					// - /12
				ts.GridColumnStyles.Add(cs_fd_calc_max_rng_bottom);					// 11/13

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_fd_real_nrm_rng_top);					// 12/14
					ts.GridColumnStyles.Add(cs_fd_real_max_rng_top);					// 13/15
					ts.GridColumnStyles.Add(cs_fd_real_nrm_rng_bottom);					// 14/16
					ts.GridColumnStyles.Add(cs_fd_real_max_rng_bottom);					// 15/17
				}
				else
				{
					ts.GridColumnStyles.Add(cs_fd_real_nrm_rng_top_syn);
					ts.GridColumnStyles.Add(cs_fd_real_max_rng_top_syn);
					ts.GridColumnStyles.Add(cs_fd_real_nrm_rng_bottom_syn);	
					ts.GridColumnStyles.Add(cs_fd_real_max_rng_bottom_syn);

					ts.GridColumnStyles.Add(cs_fd_real_nrm_rng_top_iso);
					ts.GridColumnStyles.Add(cs_fd_real_max_rng_top_iso);
					ts.GridColumnStyles.Add(cs_fd_real_nrm_rng_bottom_iso);	
					ts.GridColumnStyles.Add(cs_fd_real_max_rng_bottom_iso);	
				}

				ts.AllowSorting = false;

				// чтобы отображать не "(null)" а прочерк: " - "
				// значения NULL возможны только для верхних и 
				// нижних значений, которые не могут быть расчитаны
				// при слишком коротком измерительном периоде
				// (менее 40 минут).
				for (int i = 0; i < ts.GridColumnStyles.Count; i++)
				{
					ts.GridColumnStyles[i].NullText = " - ";
				}

				dgFrequencyDeparture.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgFrequencyDepartureInitTableStyle()");
				throw;
			}
		}

		#endregion

		#region Voltage Deviation and Frequency Grid

		public void drawdgU_Deviation()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
            {
                #region ET-PQP

                if (curDeviceType_ == EmDeviceType.ETPQP)
				{
					string query = string.Empty;
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = string.Format(@"SELECT p.name, dapt2.num_nrm_rng, dapt2.num_max_rng, dapt2.num_out_max_rng, dapt2.real_nrm_rng_top, dapt2.real_max_rng_top, dapt2.real_nrm_rng_bottom, dapt2.real_max_rng_bottom, 
CASE WHEN dapt2.valid_duy = 0 AND (dapt2.param_id BETWEEN 1002 AND 1005) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text) 
     WHEN dapt2.valid_duy_1 = 0 AND (dapt2.param_id BETWEEN 1006 AND 1009) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text)
     WHEN dapt2.valid_duy_2 = 0 AND (dapt2.param_id BETWEEN 1010 AND 1013) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text)
     WHEN dapt2.valid_duy = 0 AND (dapt2.param_id BETWEEN 1014 AND 1016) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text) 
     WHEN dapt2.valid_duy_1 = 0 AND (dapt2.param_id BETWEEN 1017 AND 1019) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text)
     WHEN dapt2.valid_duy_2 = 0 AND (dapt2.param_id BETWEEN 1020 AND 1022) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text)
else '-' end as calc_nrm_rng_top,  
dapt2.calc_max_rng_top, 
CASE WHEN dapt2.valid_duy = 0 AND (dapt2.param_id BETWEEN 1002 AND 1005) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text) 
     WHEN dapt2.valid_duy_1 = 0 AND (dapt2.param_id BETWEEN 1006 AND 1009) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text)
     WHEN dapt2.valid_duy_2 = 0 AND (dapt2.param_id BETWEEN 1010 AND 1013) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text)
     WHEN dapt2.valid_duy = 0 AND (dapt2.param_id BETWEEN 1014 AND 1016) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text) 
     WHEN dapt2.valid_duy_1 = 0 AND (dapt2.param_id BETWEEN 1017 AND 1019) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text)
     WHEN dapt2.valid_duy_2 = 0 AND (dapt2.param_id BETWEEN 1020 AND 1022) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text)
else '-' end as calc_nrm_rng_bottom, 
dapt2.calc_max_rng_bottom FROM parameters p, day_avg_parameters_t2 dapt2 
WHERE p.param_id = dapt2.param_id AND (dapt2.param_id BETWEEN 1002 AND 1022) AND 
dapt2.datetime_id = {1} ORDER BY dapt2.param_id;", settings_.FloatSigns, curDatetimeId_);
					}
					else
					{
						query = string.Format(@"SELECT p.name, dapt2.num_nrm_rng, dapt2.num_max_rng, dapt2.num_out_max_rng, dapt2.real_nrm_rng_top, dapt2.real_max_rng_top, dapt2.real_nrm_rng_bottom, dapt2.real_max_rng_bottom, 
CASE WHEN dapt2.valid_duy = 0 AND dapt2.param_id IN(1002, 1003) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text) 
     WHEN dapt2.valid_duy_1 = 0 AND dapt2.param_id IN(1006, 1007) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text)
     WHEN dapt2.valid_duy_2 = 0 AND dapt2.param_id IN(1010, 1011) 
then cast(round(cast(dapt2.calc_nrm_rng_top as numeric), {0}) as text)
else '-' end as calc_nrm_rng_top,  
dapt2.calc_max_rng_top, 
CASE WHEN dapt2.valid_duy = 0 AND dapt2.param_id IN(1002, 1003) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text) 
     WHEN dapt2.valid_duy_1 = 0 AND dapt2.param_id IN(1006, 1007) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text)
     WHEN dapt2.valid_duy_2 = 0 AND dapt2.param_id IN(1010, 1011) 
then cast(round(cast(dapt2.calc_nrm_rng_bottom as numeric), {0}) as text)
else '-' end as calc_nrm_rng_bottom, 
dapt2.calc_max_rng_bottom FROM parameters p, day_avg_parameters_t2 dapt2 
WHERE p.param_id = dapt2.param_id AND (dapt2.param_id BETWEEN 1002 AND 1022) AND 
dapt2.datetime_id = {1} ORDER BY dapt2.param_id;", settings_.FloatSigns, curDatetimeId_);
					}

					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query, "day_avg_parameters_t2", ref ds);

					DataRow[] rows = ds.Tables[0].Select("TRIM(name) = \'δU_y\'");
					int numGlobal = 0;
					if (rows.Length > 0)
					{
						numGlobal = (int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_nrm_rng")] +
							(int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_max_rng")] +
							(int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_out_max_rng")];
					}

					// adding calc-fields to the dataset
					DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_nrm_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_max_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_out_max_rng * 100/ (num_nrm_rng + num_max_rng + num_out_max_rng))";

					Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
					TimeSpan timeMaxMode;	// режим наибольших нагрузок
					TimeSpan timeMinMode;	// режим наименьших нагрузок
					Int64 curDuration = 0;
					Constants.diffMaxMinMode(curStartDateTime_, curEndDateTime_,
												out timeMaxMode, out timeMinMode);

					ds.Tables[0].Columns.Add("time_nrm_rng", System.Type.GetType("System.Int64"));
					ds.Tables[0].Columns.Add("time_max_rng", System.Type.GetType("System.Int64"));
					ds.Tables[0].Columns.Add("time_out_max_rng", System.Type.GetType("System.Int64"));
					for (int i = 0; i < ds.Tables[0].Rows.Count; ++i)
					{
						DataRow row = ds.Tables[0].Rows[i];
						int num_nrm_rng = Int32.Parse(row["num_nrm_rng"].ToString());
						int num_max_rng = Int32.Parse(row["num_max_rng"].ToString());
						int num_out_max_rng = Int32.Parse(row["num_out_max_rng"].ToString());
						string name = row["name"].ToString();
						if (num_nrm_rng == 0 && num_max_rng == 0 && num_out_max_rng == 0)
						{
							row["time_nrm_rng"] = 0;
							row["time_max_rng"] = 0;
							row["time_out_max_rng"] = 0;
						}
						else
						{
							if (name.Contains("'"))
							{
								curDuration = timeMaxMode.Ticks;
							}
							else
							{
								if (name.Contains("\""))
								{
									curDuration = timeMinMode.Ticks;
								}
								else
								{
									curDuration = timeDuration;
								}
							}

							Int64 coef = 60 * TimeSpan.TicksPerSecond; // 60 с
							row["time_nrm_rng"] = num_nrm_rng * coef;
							row["time_max_rng"] = num_max_rng * coef;
							row["time_out_max_rng"] = num_out_max_rng * coef;
						}
					}

					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_global",
							System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_nrm_rng * 100 / (" + numGlobal.ToString() + "))";

					c = ds.Tables[0].Columns.Add("prcnt_max_rng_global",
						System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_max_rng * 100 / (" + numGlobal.ToString() + "))";

					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_global",
						System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_out_max_rng * 100 / (" + numGlobal.ToString() + "))";

					// приводим верхние и нижние dU в такой же вид как остальные поля
					// (их формат изменился после превращения их в текст)
					for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
					{
						DataRow curRow = ds.Tables[0].Rows[iRow];
						string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")].ToString();
						float f_val;
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_bottom")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_bottom")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_bottom")] = "-";
					}

					// binding dataset with datagrid
					dgU_Deviation.SetDataBinding(ds, "day_avg_parameters_t2");

					//disallow add, edit and delete operations				
					CurrencyManager currencyManager =
						(CurrencyManager)BindingContext[dgU_Deviation.DataSource,
															dgU_Deviation.DataMember];
					DataView dataView = (DataView)currencyManager.List;

					currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltChanged);

					dataView.AllowNew = false;
					dataView.AllowDelete = false;
					dataView.AllowEdit = false;
                }

                #endregion

                #region ET-PQP-A

                else if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					string query = string.Empty;
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = string.Format(@"SELECT du.param_id, p.name, du.num_marked, du.num_not_marked, du.num_nrm_rng, du.num_max_rng, du.num_out_max_rng, du.real_nrm_rng_top, du.real_max_rng_top, du.real_nrm_rng_bottom, du.real_max_rng_bottom,  
CASE WHEN du.valid_du != 0 then cast(round(cast(du.calc_nrm_rng_top as numeric), {0}) as text) else '-' end as calc_nrm_rng_top, 
CASE WHEN du.num_not_marked > 0 then cast(round(cast(du.calc_max_rng_top as numeric), {0}) as text) else '-' end as calc_max_rng_top 
FROM parameters p, pqp_du du WHERE p.param_id = du.param_id AND (du.param_id BETWEEN 1003 AND 1019) AND du.datetime_id = {1} ORDER BY du.param_id;", settings_.FloatSigns, curDatetimeId_);
					}
					else
					{
						query = string.Format(@"SELECT du.param_id, p.name, du.num_marked, du.num_not_marked, du.num_nrm_rng, du.num_max_rng, du.num_out_max_rng, du.real_nrm_rng_top, du.real_max_rng_top, du.real_nrm_rng_bottom, du.real_max_rng_bottom,
CASE WHEN du.valid_du != 0 then cast(round(cast(du.calc_nrm_rng_top as numeric), {0}) as text) else '-' end as calc_nrm_rng_top, 
CASE WHEN du.num_not_marked > 0 then cast(round(cast(du.calc_max_rng_top as numeric), {0}) as text) else '-' end as calc_max_rng_top 
FROM parameters p, pqp_du du WHERE p.param_id = du.param_id AND (du.param_id IN (1003, 1007)) AND du.datetime_id = {1} ORDER BY du.param_id;", settings_.FloatSigns, curDatetimeId_);
					}

					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query, "pqp_du", ref ds);

					DataRow[] rows = ds.Tables[0].Select("TRIM(name) = \'δU_y\'");
					int numGlobal = 0;
					if (rows.Length > 0)
					{
						numGlobal = (int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_nrm_rng")] +
							(int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_max_rng")] +
							(int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_out_max_rng")];
					}

					// adding calc-fields to the dataset
					DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_nrm_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_max_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_out_max_rng * 100/ (num_nrm_rng + num_max_rng + num_out_max_rng))";

					Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
					TimeSpan timeMaxMode;	// режим наибольших нагрузок
					TimeSpan timeMinMode;	// режим наименьших нагрузок
					Int64 curDuration = 0;
					Constants.diffMaxMinMode(curStartDateTime_, curEndDateTime_,
												out timeMaxMode, out timeMinMode);

					ds.Tables[0].Columns.Add("time_nrm_rng", System.Type.GetType("System.Int64"));
					ds.Tables[0].Columns.Add("time_max_rng", System.Type.GetType("System.Int64"));
					ds.Tables[0].Columns.Add("time_out_max_rng", System.Type.GetType("System.Int64"));
					for (int i = 0; i < ds.Tables[0].Rows.Count; ++i)
					{
						DataRow row = ds.Tables[0].Rows[i];
						int num_nrm_rng = Int32.Parse(row["num_nrm_rng"].ToString());
						int num_max_rng = Int32.Parse(row["num_max_rng"].ToString());
						int num_out_max_rng = Int32.Parse(row["num_out_max_rng"].ToString());
						string name = row["name"].ToString();
						if (num_nrm_rng == 0 && num_max_rng == 0 && num_out_max_rng == 0)
						{
							row["time_nrm_rng"] = 0;
							row["time_max_rng"] = 0;
							row["time_out_max_rng"] = 0;
						}
						else
						{
							if (name.Contains("'"))
							{
								curDuration = timeMaxMode.Ticks;
							}
							else
							{
								if (name.Contains("\""))
								{
									curDuration = timeMinMode.Ticks;
								}
								else
								{
									curDuration = timeDuration;
								}
							}

							Int64 coef = 60 * TimeSpan.TicksPerSecond; // 60 с
							row["time_nrm_rng"] = num_nrm_rng * coef;
							row["time_max_rng"] = num_max_rng * coef;
							row["time_out_max_rng"] = num_out_max_rng * coef;
						}
					}

					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_global",
							System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_nrm_rng * 100 / (" + numGlobal.ToString() + "))";

					c = ds.Tables[0].Columns.Add("prcnt_max_rng_global",
						System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_max_rng * 100 / (" + numGlobal.ToString() + "))";

					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_global",
						System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_out_max_rng * 100 / (" + numGlobal.ToString() + "))";

					// приводим верхние и нижние dU в такой же вид как остальные поля
					// (их формат изменился после превращения их в текст)
					for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
					{
						DataRow curRow = ds.Tables[0].Rows[iRow];
						string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")].ToString();
						float f_val;
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_top")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_top")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_top")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_top")] = "-";
					}

					// binding dataset with datagrid
					dgU_Deviation.SetDataBinding(ds, "pqp_du");

					//disallow add, edit and delete operations				
					CurrencyManager currencyManager =
						(CurrencyManager)BindingContext[dgU_Deviation.DataSource,
															dgU_Deviation.DataMember];
					DataView dataView = (DataView)currencyManager.List;

					currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltChanged);

					dataView.AllowNew = false;
					dataView.AllowDelete = false;
					dataView.AllowEdit = false;

					try
					{
						// если нужно, добавляем режимы макс. и мин. нагрузок
						if (bNeedMaxModeForEtPQP_A_)
						{
							if (dgVolValues != null && dgVolValues.DataSource != null &&
								(dgVolValues.DataSource as DataSet).Tables[0].Rows.Count > 0)
							{
								AddMinMaxModeString("A+", ref ds);
								if (CurConnectScheme != ConnectScheme.Ph1W2)
								{
									AddMinMaxModeString("B+", ref ds);
									AddMinMaxModeString("C+", ref ds);
								}
								AddMinMaxModeString("A-", ref ds);
								if (CurConnectScheme != ConnectScheme.Ph1W2)
								{
									AddMinMaxModeString("B-", ref ds);
									AddMinMaxModeString("C-", ref ds);
								}
							}
							else
							{
								MessageBoxes.MsgErrorGetVolValues(this);
								return;
							}
						}
					}
					catch (Exception exc)
					{
						EmService.DumpException(exc, "Error in drawdgU_Deviation while calculating max mode");
						if (EmService.ShowWndFeedback)
						{
							EmServiceLib.SavingInterface.frmSentLogs frmLogs = new EmServiceLib.SavingInterface.frmSentLogs();
							frmLogs.ShowDialog();
							EmService.ShowWndFeedback = false;
						}
					}
                }

                #endregion

                #region EM32, EM33T

                else      //(devType != EmDeviceType.ETPQP && devType != EmDeviceType.ETPQP_A)
				{
					string query = string.Empty;
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = "SELECT p.name, dapt2.num_nrm_rng, dapt2.num_max_rng, dapt2.num_out_max_rng, dapt2.real_nrm_rng_top, dapt2.real_max_rng_top, dapt2.real_nrm_rng_bottom, dapt2.real_max_rng_bottom, dapt2.calc_nrm_rng_top, dapt2.calc_max_rng_top, dapt2.calc_nrm_rng_bottom, dapt2.calc_max_rng_bottom FROM parameters p, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt2.param_id AND (dapt2.param_id BETWEEN 1002 AND 1022) AND dapt2.datetime_id = " + curDatetimeId_ + " ORDER BY dapt2.param_id;";
					}
					else
					{
						query = "SELECT p.name, dapt2.num_nrm_rng, dapt2.num_max_rng, dapt2.num_out_max_rng, dapt2.real_nrm_rng_top, dapt2.real_max_rng_top, dapt2.real_nrm_rng_bottom, dapt2.real_max_rng_bottom, dapt2.calc_nrm_rng_top, dapt2.calc_max_rng_top, dapt2.calc_nrm_rng_bottom, dapt2.calc_max_rng_bottom FROM parameters p, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt2.param_id AND (dapt2.param_id BETWEEN 1002 AND 1013) AND dapt2.datetime_id = " + curDatetimeId_ + " ORDER BY dapt2.param_id;";
					}

					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query, "day_avg_parameters_t2", ref ds);

					DataRow[] rows = ds.Tables[0].Select("TRIM(name) = \'δU_y\'");
					int numGlobal = 0;
					if (rows.Length > 0)
					{
						numGlobal = (int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_nrm_rng")] +
							(int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_max_rng")] +
							(int)rows[0].ItemArray[ds.Tables[0].Columns.IndexOf("num_out_max_rng")];
					}

					// adding calc-fields to the dataset
					DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_nrm_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_max_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
						"num_out_max_rng * 100/ (num_nrm_rng + num_max_rng + num_out_max_rng))";

					Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
					TimeSpan timeMaxMode;	// режим наибольших нагрузок
					TimeSpan timeMinMode;	// режим наименьших нагрузок
					Int64 curDuration = 0;
					Constants.diffMaxMinMode(curStartDateTime_, curEndDateTime_,
												out timeMaxMode, out timeMinMode);

					ds.Tables[0].Columns.Add("time_nrm_rng", System.Type.GetType("System.Int64"));
					ds.Tables[0].Columns.Add("time_max_rng", System.Type.GetType("System.Int64"));
					ds.Tables[0].Columns.Add("time_out_max_rng", System.Type.GetType("System.Int64"));
					for (int i = 0; i < ds.Tables[0].Rows.Count; ++i)
					{
						DataRow row = ds.Tables[0].Rows[i];
						int num_nrm_rng = Int32.Parse(row["num_nrm_rng"].ToString());
						int num_max_rng = Int32.Parse(row["num_max_rng"].ToString());
						int num_out_max_rng = Int32.Parse(row["num_out_max_rng"].ToString());
						string name = row["name"].ToString();
						if (num_nrm_rng == 0 && num_max_rng == 0 && num_out_max_rng == 0)
						{
							row["time_nrm_rng"] = 0;
							row["time_max_rng"] = 0;
							row["time_out_max_rng"] = 0;
						}
						else
						{
							if (name.Contains("'"))
							{
								curDuration = timeMaxMode.Ticks;
							}
							else
							{
								if (name.Contains("\""))
								{
									curDuration = timeMinMode.Ticks;
								}
								else
								{
									curDuration = timeDuration;
								}
							}

							Int64 coef = 60 * TimeSpan.TicksPerSecond; // 60 с
							row["time_nrm_rng"] = num_nrm_rng * coef;
							row["time_max_rng"] = num_max_rng * coef;
							row["time_out_max_rng"] = num_out_max_rng * coef;
						}
					}

					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_global",
							System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_nrm_rng * 100 / (" + numGlobal.ToString() + "))";

					c = ds.Tables[0].Columns.Add("prcnt_max_rng_global",
						System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_max_rng * 100 / (" + numGlobal.ToString() + "))";

					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_global",
						System.Type.GetType("System.Single"));
					c.Expression = "IIF((" + numGlobal.ToString() + ")=0,0," +
						"num_out_max_rng * 100 / (" + numGlobal.ToString() + "))";

					// binding dataset with datagrid
					dgU_Deviation.SetDataBinding(ds, "day_avg_parameters_t2");

					//disallow add, edit and delete operations				
					CurrencyManager currencyManager =
						(CurrencyManager)BindingContext[dgU_Deviation.DataSource,
															dgU_Deviation.DataMember];
					DataView dataView = (DataView)currencyManager.List;

					currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltChanged);

					dataView.AllowNew = false;
					dataView.AllowDelete = false;
					dataView.AllowEdit = false;
                }

                #endregion
            }
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in drawdgU_Deviation()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		private bool AddMinMaxModeString(string phase, ref DataSet ds)
		{
			DbService dbService = null;
			bool positivePhase = true;		// positive or negative phase
			try
			{
				// максимальное и верхнее (для наиб. и наим. нагрузок)
				float maxMaxMode = -1, maxMinMode = -1, upMaxMode = -1, upMinMode = -1;	
				string paramName = string.Empty;
				int markedMax = 0, notMarkedMax = 0, markedMin = 0, notMarkedMin = 0;
				bool minModeValid = false, maxModeValid = false, upValueValidMax = false, upValueValidMin = false;

				// сначала нужно получить все значения
				List<Trio<DateTime, float, short>> listUVal = new List<Trio<DateTime, float, short>>();
				dbService = new DbService(GetPgConnectionString());
				string curColumn = string.Empty;
				switch (phase)
				{
					case "A+": curColumn = "u_a_ab_pos"; break;
					case "A-": curColumn = "u_a_ab_neg"; positivePhase = false; break;
					case "B+": curColumn = "u_b_bc_pos"; break;
					case "B-": curColumn = "u_b_bc_neg"; positivePhase = false; break;
					case "C+": curColumn = "u_c_ca_pos"; break;
					case "C-": curColumn = "u_c_ca_neg"; positivePhase = false; break;
				}

				if (!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				string query = string.Format("SELECT event_datetime, case when {0} = -1 then ' ' else cast({1} as text) end, record_marked FROM pqp_du_val WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;", curColumn, curColumn);
				dbService.ExecuteReader(query);
				float cur_val;
				while (dbService.DataReaderRead())
				{
					DateTime curDt;
					string curDtStr = dbService.DataReaderData("event_datetime").ToString();
					if (!DateTime.TryParse(curDtStr, out curDt))
					{
						EmService.WriteToLogFailed("AddMinMaxModeString date error: " + curDtStr);
						continue;
					}
					short cur_record_marked = Int16.Parse(dbService.DataReaderData("record_marked").ToString());
					if (cur_record_marked == 0)
					{
						object oCurVal = dbService.DataReaderData(curColumn);
						if (oCurVal is DBNull) continue;
						if (!Conversions.object_2_float_en_ru(dbService.DataReaderData(curColumn), out cur_val))
							continue;
						listUVal.Add(new Trio<DateTime, float, short>(curDt, cur_val, cur_record_marked));
					}
					else listUVal.Add(new Trio<DateTime, float, short>(curDt, -1, 1));
				}

				if (listUVal.Count == 0)
				{
					EmService.WriteToLogFailed("AddMinMaxModeString: no dU values for phase " + phase);
					return false;
				}
				
				// затем надо разделить по режимам наибольших и наименьших:
				// создаем новые списки и туда отложим отсчеты, которые относятся к режиму наибольших,
				// а в исходных списках останутся значения режима наименьших
				List<Trio<DateTime, float, short>> listUValMax = new List<Trio<DateTime, float, short>>();
				SeparateMinMaxMode(ref listUVal, ref listUValMax);
				// теперь считаем сколько маркированных и немаркированных
				for (int iItem = 0; iItem < listUVal.Count; ++iItem)
				{
					if (listUVal[iItem].Third != 0)
					{
						markedMin++;
						listUVal.RemoveAt(iItem);
						iItem--;
					}
				}
				notMarkedMin = listUVal.Count;
				for (int iItem = 0; iItem < listUValMax.Count; ++iItem)
				{
					if (listUValMax[iItem].Third != 0)
					{
						markedMax++;
						listUValMax.RemoveAt(iItem);
						iItem--;
					}
				}
				notMarkedMax = listUValMax.Count;

				// если что-то еще осталось, значит режим можно обработать :)
				if (listUVal.Count > 0) minModeValid = true;
				if (listUValMax.Count > 0) maxModeValid = true;

				int curParamId1 = -1, curParamId2 = -1;
				switch (phase)
				{
					case "A+": curParamId1 = 1003; curParamId2 = 1014; break;
					case "B+": curParamId1 = 1004; curParamId2 = 1015; break;
					case "C+": curParamId1 = 1005; curParamId2 = 1016; break;
					case "A-": curParamId1 = 1007; curParamId2 = 1017; break;
					case "B-": curParamId1 = 1008; curParamId2 = 1018; break;
					case "C-": curParamId1 = 1009; curParamId2 = 1019; break;
				}
				for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
				{
					int paramId = Int32.Parse((ds.Tables[0].Rows[iRow]["param_id"]).ToString());
					if (paramId == curParamId1 || paramId == curParamId2)
					{
						Conversions.object_2_float_en_ru(ds.Tables[0].Rows[iRow]["real_nrm_rng_top"], out cur_val);
						if (float.IsNaN(constrNPLtopMax_)) constrNPLtopMax_ = cur_val;
						if (float.IsNaN(constrNPLtopMin_)) constrNPLtopMin_ = cur_val;
						Conversions.object_2_float_en_ru(ds.Tables[0].Rows[iRow]["real_max_rng_top"], out cur_val);
						if (float.IsNaN(constrUPLtopMax_)) constrUPLtopMax_ = cur_val;
						if (float.IsNaN(constrUPLtopMin_)) constrUPLtopMin_ = cur_val;
						Conversions.object_2_float_en_ru(ds.Tables[0].Rows[iRow]["real_nrm_rng_bottom"], out cur_val);
						if (float.IsNaN(constrNPLbottomMax_)) constrNPLbottomMax_ = cur_val;
						if (float.IsNaN(constrNPLbottomMin_)) constrNPLbottomMin_ = cur_val;
						Conversions.object_2_float_en_ru(ds.Tables[0].Rows[iRow]["real_max_rng_bottom"], out cur_val);
						if (float.IsNaN(constrUPLbottomMax_)) constrUPLbottomMax_ = cur_val;
						if (float.IsNaN(constrUPLbottomMin_)) constrUPLbottomMin_ = cur_val;

						paramName = (string)ds.Tables[0].Rows[iRow]["name"];
						//marked = Int32.Parse((ds.Tables[0].Rows[iRow]["num_marked"]).ToString());
						//not_marked = Int32.Parse((ds.Tables[0].Rows[iRow]["num_not_marked"]).ToString());
						break;
					}
				}

				// теперь считаем сколько в НДП и ПДП (наиб.нагрузки)
				int betweenNPLandUPLmax = 0, overUPLmax = 0, inNPLmax = 0;
				// для положит.фаз используем уставки top, для отриц.фаз используем bottom
				float curConstrNPLMax = constrNPLtopMax_;
				float curConstrUPLMax = constrUPLtopMax_;
				if (!positivePhase)
				{
					curConstrNPLMax = constrNPLbottomMax_;
					curConstrUPLMax = constrUPLbottomMax_;
				}
				if (maxModeValid)
				{
					for (int iItem = 0; iItem < listUValMax.Count; ++iItem)
					{
						// in NPL
						if (listUValMax[iItem].Second <= curConstrNPLMax)
						{
							inNPLmax++;
						}
						// если уставка НДП = 0, то ее как бы нет
						else if (curConstrNPLMax == 0 && listUValMax[iItem].Second < curConstrUPLMax)
						{
							inNPLmax++;
						}
						// over UPL
						else if (listUValMax[iItem].Second > curConstrUPLMax)
						{
							overUPLmax++;
						}
						else // between NPL and UPL
						{
							betweenNPLandUPLmax++;
						}
					}
				}

				// теперь считаем сколько в НДП и ПДП (наим.нагрузки)
				int betweenNPLandUPLmin = 0, overUPLmin = 0, inNPLmin = 0;
				// для положит.фаз используем уставки top, для отриц.фаз используем bottom
				float curConstrNPLMin = constrNPLtopMin_;
				float curConstrUPLMin = constrUPLtopMin_;
				if (!positivePhase)
				{
					curConstrNPLMin = constrNPLbottomMin_;
					curConstrUPLMin = constrUPLbottomMin_;
				}
				if (minModeValid)
				{
					for (int iItem = 0; iItem < listUVal.Count; ++iItem)
					{
						// in NPL
						if (listUVal[iItem].Second <= curConstrNPLMin)
						{
							inNPLmin++;
						}
						// если уставка НДП = 0, то ее как бы нет
						else if (curConstrNPLMin == 0 && listUVal[iItem].Second < curConstrUPLMin)
						{
							inNPLmin++;
						}
						// over UPL
						else if (listUVal[iItem].Second > curConstrUPLMin)
						{
							overUPLmin++;
						}
						else // between NPL and UPL
						{
							betweenNPLandUPLmin++;
						}
					}
				}

				// sort values
				if (minModeValid) listUVal.Sort(new Comparison<Trio<DateTime, float, short>>(CompareUValues));
				if (maxModeValid) listUValMax.Sort(new Comparison<Trio<DateTime, float, short>>(CompareUValues));

				// максимальные
				if (minModeValid) maxMinMode = listUVal[listUVal.Count - 1].Second;
				if (maxModeValid) maxMaxMode = listUValMax[listUValMax.Count - 1].Second;

				// верхние
				// удаляем 5% наибольших значений
				if (minModeValid)
				{
					int perc5 = listUVal.Count * 5 / 100;
					if (perc5 > 0)
					{
						upValueValidMin = true;
						listUVal.RemoveRange(listUVal.Count - perc5, perc5);
						upMinMode = listUVal[listUVal.Count - 1].Second;
					}
				}
				if (maxModeValid)
				{
					int perc5 = listUValMax.Count * 5 / 100;
					if (perc5 > 0)
					{
						upValueValidMax = true;
						listUValMax.RemoveRange(listUValMax.Count - perc5, perc5);
						upMaxMode = listUValMax[listUValMax.Count - 1].Second;
					}
				}

				// добавляем строку в таблицу (наиб. нагрузки)
				if (maxModeValid)
				{
					DataRow newRow = ds.Tables[0].NewRow();
					newRow["name"] = paramName.Trim() + " '";
					newRow["num_marked"] = markedMax;
					newRow["num_not_marked"] = notMarkedMax;
					newRow["num_nrm_rng"] = inNPLmax;
					newRow["num_max_rng"] = betweenNPLandUPLmax;
					newRow["num_out_max_rng"] = overUPLmax;
					if (upValueValidMax) newRow["calc_nrm_rng_top"] = Math.Round(upMaxMode, settings_.FloatSigns);
					newRow["calc_max_rng_top"] = Math.Round(maxMaxMode, settings_.FloatSigns);
					newRow["real_nrm_rng_top"] = constrNPLtopMax_;
					newRow["real_max_rng_top"] = constrUPLtopMax_;
					newRow["real_nrm_rng_bottom"] = constrNPLbottomMax_;
					newRow["real_max_rng_bottom"] = constrUPLbottomMax_;
					ds.Tables[0].Rows.Add(newRow);
				}

				// добавляем строку в таблицу (наим. нагрузки)
				if (minModeValid)
				{
					DataRow newRow = ds.Tables[0].NewRow();
					newRow["name"] = paramName.Trim() + " \"";
					newRow["num_marked"] = markedMin;
					newRow["num_not_marked"] = notMarkedMin;
					newRow["num_nrm_rng"] = inNPLmin;
					newRow["num_max_rng"] = betweenNPLandUPLmin;
					newRow["num_out_max_rng"] = overUPLmin;
					if (upValueValidMin) newRow["calc_nrm_rng_top"] = Math.Round(upMinMode, settings_.FloatSigns);
					newRow["calc_max_rng_top"] = Math.Round(maxMinMode, settings_.FloatSigns);
					newRow["real_nrm_rng_top"] = constrNPLtopMin_;
					newRow["real_max_rng_top"] = constrUPLtopMin_;
					newRow["real_nrm_rng_bottom"] = constrNPLbottomMin_;
					newRow["real_max_rng_bottom"] = constrUPLbottomMin_;
					ds.Tables[0].Rows.Add(newRow);
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddMinMaxModeString()");
				throw;
			}
			finally { if (dbService != null) dbService.CloseConnect(); }
		}

		private static int CompareUValues(Trio<DateTime, float, short> v1, Trio<DateTime, float, short> v2)
		{
			if (v1.Second > v2.Second) return 1;
			else if(v1.Second < v2.Second) return -1;
			return 0;
		}

		private void SeparateMinMaxMode(ref List<Trio<DateTime, float, short>> listUValpos,
										ref List<Trio<DateTime, float, short>> listUValposMax)
		{
			try
			{
				// проверяем заданы ли вообще режимы (если времена равны, то считается, что не задан)
				bool validPeriod1 = true, validPeriod2 = true;
				if (dtMaxModeStart1_.TimeOfDay == dtMaxModeEnd1_.TimeOfDay) validPeriod1 = false;
				if (dtMaxModeStart2_.TimeOfDay == dtMaxModeEnd2_.TimeOfDay) validPeriod2 = false;

				// эти переменные нужны, чтобы обработать ситуацию когда например режим с 23:50 до 00:10
				bool timeIsConsistent1 = true, timeIsConsistent2 = true;
				if (dtMaxModeStart1_.TimeOfDay > dtMaxModeEnd1_.TimeOfDay) timeIsConsistent1 = false;
				if (dtMaxModeStart2_.TimeOfDay > dtMaxModeEnd2_.TimeOfDay) timeIsConsistent2 = false;

				for (int iItem = 0; iItem < listUValpos.Count; ++iItem)
				{
					bool isMax = false;
					if (validPeriod1)
					{
						if (timeIsConsistent1)
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart1_.TimeOfDay &&
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd1_.TimeOfDay)
								isMax = true;
						}
						else
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart1_.TimeOfDay ||
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd1_.TimeOfDay)
								isMax = true;
						}
					}

					if (validPeriod2)
					{
						if (timeIsConsistent2)
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart2_.TimeOfDay &&
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd2_.TimeOfDay)
								isMax = true;
						}
						else
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart2_.TimeOfDay ||
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd2_.TimeOfDay)
								isMax = true;
						}
					}

					if (isMax)
					{
						listUValposMax.Add(listUValpos[iItem]);
						listUValpos.RemoveAt(iItem);
						iItem--;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SeparateMinMaxMode()");
				throw;
			}
		}

		/// <summary>
		/// Filling Voltage Values Grid with data
		/// </summary>
		public void drawdgUValues()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
				string tableName = "day_avg_parameters_t6";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) tableName = "pqp_du_val";

				string query = string.Empty;
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					if (CurConnectScheme == ConnectScheme.Ph3W4 ||
						CurConnectScheme == ConnectScheme.Ph3W4_B_calc)
					{
						query = "SELECT event_datetime, d_u_y, d_u_a, d_u_b, d_u_c, d_u_ab, d_u_bc, d_u_ca FROM day_avg_parameters_t6 WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;";
					}
					else if (CurConnectScheme == ConnectScheme.Ph3W3 ||
							CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
					{
						query = "SELECT event_datetime, d_u_y, d_u_ab, d_u_bc, d_u_ca FROM day_avg_parameters_t6 WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;";
					}
					else if (CurConnectScheme == ConnectScheme.Ph1W2)
					{
						query = "SELECT event_datetime, d_u_a FROM day_avg_parameters_t6 WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;";
					}
				}
				else
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						//query = "SELECT event_datetime, case when u_a_ab_pos = -1 then ' ' else cast(u_a_ab_pos as text) end, case when u_b_bc_pos = -1 then ' ' else cast(u_b_bc_pos as text) end, case when u_c_ca_pos = -1 then ' ' else cast(u_c_ca_pos as text) end, case when u_a_ab_neg = -1 then ' ' else cast(u_a_ab_neg as text) end, case when u_b_bc_neg = -1 then ' ' else cast(u_b_bc_neg as text) end, case when u_c_ca_neg = -1 then ' ' else cast(u_c_ca_neg as text) end FROM pqp_du_val WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;";
						query = "SELECT event_datetime, case when u_a_ab_pos = -1 then ' ' else cast(u_a_ab_pos as text) end, case when u_a_ab_neg = -1 then ' ' else cast(u_a_ab_neg as text) end, case when u_b_bc_pos = -1 then ' ' else cast(u_b_bc_pos as text) end, case when u_b_bc_neg = -1 then ' ' else cast(u_b_bc_neg as text) end, case when u_c_ca_pos = -1 then ' ' else cast(u_c_ca_pos as text) end, case when u_c_ca_neg = -1 then ' ' else cast(u_c_ca_neg as text) end, record_marked FROM pqp_du_val WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;";
					}
					else
					{
						query = "SELECT event_datetime, case when u_a_ab_pos = -1 then ' ' else cast(u_a_ab_pos as text) end, case when u_a_ab_neg = -1 then ' ' else cast(u_a_ab_neg as text) end, record_marked FROM pqp_du_val WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;";
					}
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, tableName, ref ds);

				// binding dataset with datagrid
				dgVolValues.SetDataBinding(ds, tableName);

				//disallow add, edit and delete operations				
				CurrencyManager currencyManager =
					(CurrencyManager)BindingContext[dgVolValues.DataSource, dgVolValues.DataMember];
				DataView dataView = (DataView)currencyManager.List;

				//currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltChanged);

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in drawdgUValues()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		public void drawdgInterharm()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if (!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
				string query = string.Empty;

				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					query = @"SELECT (param_num - 0.5) as param_num, 
case when valid_interharm = 0 then '-' else cast(val_ph1 as text) end,
case when valid_interharm = 0 then '-' else cast(val_ph2 as text) end,
case when valid_interharm = 0 then '-' else cast(val_ph3 as text) end
FROM pqp_interharm_u WHERE datetime_id = " + curDatetimeId_ + " ORDER BY param_num;";
				}
				else
				{
					query = @"SELECT (param_num - 0.5) as param_num, 
case when valid_interharm = 0 then '-' else cast(val_ph1 as text) end
FROM pqp_interharm_u WHERE datetime_id = " + curDatetimeId_ + " ORDER BY param_num;";
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "pqp_interharm_u", ref ds);

				// binding dataset with datagrid
				dgInterharm.SetDataBinding(ds, "pqp_interharm_u");

				//disallow add, edit and delete operations				
				CurrencyManager currencyManager =
					(CurrencyManager)BindingContext[dgInterharm.DataSource, dgInterharm.DataMember];
				DataView dataView = (DataView)currencyManager.List;

				//currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltChanged);

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in drawdgInterharm()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		public void drawdgFreqValues()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if (!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
				string tableName = "day_avg_parameters_t7";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) tableName = "pqp_df_val";

				string query = string.Format("SELECT event_datetime, case when d_f = -1 then ' ' else cast(d_f as text) end FROM {0} WHERE datetime_id = {1} ORDER BY event_datetime;", tableName, curDatetimeId_);
				
				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, tableName, ref ds);

				// binding dataset with datagrid
				dgFValues.SetDataBinding(ds, tableName);

				// округляем числа и делаем формат F, чтобы не было научной записи числа
				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("d_f"))
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
							{
								try
								{
									double val = Conversions.object_2_double(ds.Tables[0].Rows[r][i]);
									val = Math.Round(val, 6);
									ds.Tables[0].Rows[r][i] = val.ToString("F6", new CultureInfo("en-US"));
								}
								catch (Exception ex_val)
								{
									EmService.WriteToLogFailed("Error while processing d_f: " + ex_val.Message);
								}
							}
						}
					}
				}

				//disallow add, edit and delete operations				
				CurrencyManager currencyManager =
					(CurrencyManager)BindingContext[dgFValues.DataSource,
														dgFValues.DataMember];
				DataView dataView = (DataView)currencyManager.List;

				//currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltChanged);

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;

				// и сразу рисуем график в нижнем окне
				if (((DataSet)dgFValues.DataSource).Tables[0].Rows.Count > 0)
				{
					//AddCurver((DataGrid)((cmsDoc2.Tag as object[])[0]),
					//		Convert.ToInt32((cmsDoc2.Tag as object[])[1]), false);

					frmDocFPQPGraphBottom wndDocFValuesGraph = 
						this.MainWindow_.wndDocPQP.wndDocFValuesGraphBottom;
					int column = (dgFValues.DataSource as DataSet).Tables[0].Columns.IndexOf("d_f");
					Color curveColor = Color.Blue;
					MainWindow_.wndDocPQP.wndDocFValuesGraphBottom.AddCurver(column, curveColor);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in drawdgFreqValues()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		private void currencyManager_VoltChanged(object sender, EventArgs e)
		{
			try
			{
				// для Эт-ПКЭ-А нет значений bottom, поэтому пока непонятно как строить графики
				// (no bottom for graphs)
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					pqpGraph_dU.Visible = false;
					return;
				}
				else pqpGraph_dU.Visible = true;

				int ipos = dgU_Deviation.CurrentRowIndex;
				if (ipos == -1)
				{
					barVoltNplSt.WorldTop = float.NaN;
					barVoltNplSt.WorldBottom = float.NaN;
					barVoltUplSt.WorldTop = float.NaN;
					barVoltUplSt.WorldBottom = float.NaN;
					barVoltNplRes.WorldTop = float.NaN;
					barVoltNplRes.WorldBottom = float.NaN;
					barVoltUplRes.WorldTop = float.NaN;
					barVoltUplRes.WorldBottom = float.NaN;
					pqpGraph_dU.Invalidate();
					return;
				}
				string pctf = "{0}%";

				DataSet tmpDataSet = (DataSet)dgU_Deviation.DataSource;
				DataTable tmpTable = tmpDataSet.Tables[0];
				int iPrcntNrmRng = tmpTable.Columns.IndexOf("prcnt_nrm_rng");
				int iPrcntMaxRng = tmpTable.Columns.IndexOf("prcnt_max_rng");
				int iPrcntOutMaxRng = tmpTable.Columns.IndexOf("prcnt_out_max_rng");
				int iCalcNrmRngTop = tmpTable.Columns.IndexOf("calc_nrm_rng_top");
				int iCalcMaxRngTop = tmpTable.Columns.IndexOf("calc_max_rng_top");
				int iCalcNrmRngBottom = tmpTable.Columns.IndexOf("calc_nrm_rng_bottom");
				int iCalcMaxRngBottom = tmpTable.Columns.IndexOf("calc_max_rng_bottom");
				int iRealNrmRngTop = tmpTable.Columns.IndexOf("real_nrm_rng_top");
				int iRealMaxRngTop = tmpTable.Columns.IndexOf("real_max_rng_top");
				int iRealNrmRngBottom = tmpTable.Columns.IndexOf("real_nrm_rng_bottom");
				int iRealMaxRngBottom = tmpTable.Columns.IndexOf("real_max_rng_bottom");

				float NPL_pct_real;
				Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iPrcntNrmRng /*4*/],
					out NPL_pct_real);
				float NPL_top_real;
				Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iRealNrmRngTop/*17*/],
					out NPL_top_real);
				float NPL_bottom_real;
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iRealNrmRngBottom /*19*/],
					out NPL_bottom_real);

				barVoltNplSt.WorldTop = NPL_top_real;
				barVoltNplSt.WorldBottom = NPL_bottom_real;
				barVoltNplSt.PercentText = string.Format(pctf, Math.Round(NPL_pct_real, 2));

				float UPL_pct_real;
				Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iPrcntMaxRng /*5*/],
					out UPL_pct_real);
				UPL_pct_real += NPL_pct_real;
				float UPL_top_real;
				Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iRealMaxRngTop /*18*/],
					out UPL_top_real);
				float UPL_bottom_real;
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iRealMaxRngBottom /*20*/],
					out UPL_bottom_real);

				barVoltUplSt.WorldTop = UPL_top_real;
				barVoltUplSt.WorldBottom = UPL_bottom_real;
				barVoltUplSt.PercentText = string.Format(pctf, Math.Round(UPL_pct_real, 2));

				float NPL_pct_calc = 95F;
				float NPL_top_calc = float.NaN;
				if (!(dgU_Deviation[ipos, iCalcNrmRngTop /*13*/] is DBNull))
					if (!dgU_Deviation[ipos, 13].ToString().Equals("-"))
						Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iCalcNrmRngTop], 
							out NPL_top_calc);
				float NPL_bottom_calc = float.NaN;
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				//{
					if (!(dgU_Deviation[ipos, iCalcNrmRngBottom /*15*/] is DBNull))
						if (!dgU_Deviation[ipos, 15].ToString().Equals("-"))
							Conversions.object_2_float_en_ru(dgU_Deviation[ipos,
								iCalcNrmRngBottom], out NPL_bottom_calc);
				//}

				barVoltNplRes.WorldTop = NPL_top_calc;
				barVoltNplRes.WorldBottom = NPL_bottom_calc;
				barVoltNplRes.PercentText = string.Format(pctf, Math.Round(NPL_pct_calc, 2));

				float UPL_pct_calc = 100F;
				float UPL_top_calc = float.NaN;
				if (!(dgU_Deviation[ipos, iCalcMaxRngTop /*14*/] is DBNull))
					Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iCalcMaxRngTop],
						out UPL_top_calc);
				float UPL_bottom_calc = float.NaN;
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				//{
					if (!(dgU_Deviation[ipos, iCalcMaxRngBottom /*16*/] is DBNull))
						Conversions.object_2_float_en_ru(dgU_Deviation[ipos, iCalcMaxRngBottom],
							out UPL_bottom_calc);
				//}

				barVoltUplRes.WorldTop = UPL_top_calc;
				barVoltUplRes.WorldBottom = UPL_bottom_calc;
				barVoltUplRes.PercentText = string.Format(pctf, Math.Round(UPL_pct_calc, 2));

				pqpGraph_dU.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in currencyManager_VoltChanged(): ");
				throw;
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgU_DeviationInitTableStyle()
		{
			try
			{
				dgU_Deviation.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				DataGridGroupCaption caption0 = 
					new DataGridGroupCaption(rm.GetString("name_columns_parameter"), 1);
				DataGridGroupCaption caption6 =
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number"), 2/*, Color.Beige*/);
				DataGridGroupCaption caption1 = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number"), 3/*, Color.Beige*/);
				DataGridGroupCaption caption2 = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + ", %", 
					3/*, Color.Beige*/);
				DataGridGroupCaption caption3 = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + ", " + 
					rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);

				DataGridGroupCaption caption_percent_global = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number_global") + ", %", 
					3/*, Color.Beige*/);

				DataGridGroupCaption caption4 = 
					new DataGridGroupCaption(rm.GetString("name_column_measures_result") + ", %",
 						curDeviceType_ != EmDeviceType.ETPQP_A ? 4 : 2
					/*, Color.Honeydew*/);
				int num_col_sets = /*(curDeviceType_ == EmDeviceType.ETPQP_A) ? 2 :*/ 4;
				DataGridGroupCaption caption5 = new DataGridGroupCaption(
					rm.GetString("name_columns_standard_value") + ", %", num_col_sets
					/*, Color.Honeydew*/);

				// p.name
				DataGridColumnCellFormula cs_vd_name = new DataGridColumnCellFormula(caption0, 0);
				cs_vd_name.HeaderText = "";
				cs_vd_name.MappingName = "name";
				cs_vd_name.BackgroungColor = DataGridColors.ColorPqpParam;

				//short curColumnNumber = 0;
				DataGridColumnGroupCaption cs_vd_marked = null;
				DataGridColumnGroupCaption cs_vd_not_marked = null;
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					// dapt2.marked
					cs_vd_marked = new DataGridColumnGroupCaption(caption6, 0);
					cs_vd_marked.HeaderText = rm.GetString("name_columns_marked");
					cs_vd_marked.MappingName = "num_marked";
					cs_vd_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vd_marked.Format = DataColumnsFormat.FloatShortFormat;
					// dapt2.nnot_marked
					cs_vd_not_marked = new DataGridColumnGroupCaption(caption6, 1);
					cs_vd_not_marked.HeaderText = rm.GetString("name_columns_not_marked");
					cs_vd_not_marked.MappingName = "num_not_marked";
					cs_vd_not_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vd_not_marked.Format = DataColumnsFormat.FloatShortFormat;
				}

				// dapt2.num_nrm_rng
				DataGridColumnGroupCaption cs_vd_num_nrm_rng = 
					new DataGridColumnGroupCaption(caption1, 0);
				cs_vd_num_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vd_num_nrm_rng.MappingName = "num_nrm_rng";
				cs_vd_num_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_num_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;
				// dapt2.num_max_rng
				DataGridColumnGroupCaption cs_vd_num_max_rng = 
					new DataGridColumnGroupCaption(caption1, 1);
				cs_vd_num_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vd_num_max_rng.MappingName = "num_max_rng";
				cs_vd_num_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_num_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_vd_num_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.num_out_max_rng
				DataGridColumnGroupCaption cs_vd_num_out_max_rng = 
					new DataGridColumnGroupCaption(caption1, 2);
				cs_vd_num_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vd_num_out_max_rng.MappingName = "num_out_max_rng";
				cs_vd_num_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_num_out_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_vd_num_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.prcnt_nrm_rng
				DataGridColumnGroupCaption cs_vd_prcnt_nrm_rng = new DataGridColumnGroupCaption(caption2, 0);
				cs_vd_prcnt_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vd_prcnt_nrm_rng.MappingName = "prcnt_nrm_rng";
				cs_vd_prcnt_nrm_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_prcnt_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;

				// dapt2.prcnt_max_rng
				DataGridColumnGroupCaption cs_vd_prcnt_max_rng = new DataGridColumnGroupCaption(caption2, 1);
				cs_vd_prcnt_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vd_prcnt_max_rng.MappingName = "prcnt_max_rng";
				cs_vd_prcnt_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_prcnt_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_prcnt_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.prcnt_out_max_rng
				DataGridColumnGroupCaption cs_vd_prcnt_out_max_rng = new DataGridColumnGroupCaption(caption2, 2);
				cs_vd_prcnt_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vd_prcnt_out_max_rng.MappingName = "prcnt_out_max_rng";
				cs_vd_prcnt_out_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_prcnt_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_prcnt_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.time_nrm_rng
				DataGridColumnTimespan cs_vd_time_nrm_rng = new DataGridColumnTimespan(caption3, 0);
				cs_vd_time_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vd_time_nrm_rng.MappingName = "time_nrm_rng";
				cs_vd_time_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;

				// dapt2.time_max_rng
				DataGridColumnTimespan cs_vd_time_max_rng = new DataGridColumnTimespan(caption3, 1);
				cs_vd_time_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vd_time_max_rng.MappingName = "time_max_rng";
				cs_vd_time_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_time_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.time_out_max_rng
				DataGridColumnTimespan cs_vd_time_out_max_rng = new DataGridColumnTimespan(caption3, 2);
				cs_vd_time_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vd_time_out_max_rng.MappingName = "time_out_max_rng";
				cs_vd_time_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_time_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.prcnt_nrm_rng_global
				DataGridColumnGroupCaption cs_vd_prcnt_nrm_rng_global =
					new DataGridColumnGroupCaption(caption_percent_global, 0);
				cs_vd_prcnt_nrm_rng_global.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vd_prcnt_nrm_rng_global.MappingName = "prcnt_nrm_rng_global";
				cs_vd_prcnt_nrm_rng_global.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_prcnt_nrm_rng_global.BackgroungColor = DataGridColors.ColorCommon;

				// dapt2.prcnt_max_rng_global
				DataGridColumnGroupCaption cs_vd_prcnt_max_rng_global =
					new DataGridColumnGroupCaption(caption_percent_global, 1);
				cs_vd_prcnt_max_rng_global.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vd_prcnt_max_rng_global.MappingName = "prcnt_max_rng_global";
				cs_vd_prcnt_nrm_rng_global.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_prcnt_max_rng_global.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_prcnt_max_rng_global.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt2.prcnt_out_max_rng_global
				DataGridColumnGroupCaption cs_vd_prcnt_out_max_rng_global =
					new DataGridColumnGroupCaption(caption_percent_global, 2);
				cs_vd_prcnt_out_max_rng_global.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vd_prcnt_out_max_rng_global.MappingName = "prcnt_out_max_rng_global";
				cs_vd_prcnt_out_max_rng_global.Format = 
					DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_prcnt_out_max_rng_global.BackgroungColor = DataGridColors.ColorCommon;
				cs_vd_prcnt_out_max_rng_global.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt2.calc_nrm_rng_top
				DataGridColumnGroupCaption cs_vd_calc_nrm_rng_top = new DataGridColumnGroupCaption(caption4, 0);
				cs_vd_calc_nrm_rng_top.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vd_calc_nrm_rng_top.MappingName = "calc_nrm_rng_top";
				cs_vd_calc_nrm_rng_top.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_calc_nrm_rng_top.BackgroungColor = DataGridColors.ColorPkeResult;

				// dapt2.calc_max_rng_top
				DataGridColumnGroupCaption cs_vd_calc_max_rng_top = new DataGridColumnGroupCaption(caption4, 1);
				cs_vd_calc_max_rng_top.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vd_calc_max_rng_top.MappingName = "calc_max_rng_top";
				cs_vd_calc_max_rng_top.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_calc_max_rng_top.BackgroungColor = DataGridColors.ColorPkeResult;

				// dapt2.calc_nrm_rng_bottom
				DataGridColumnGroupCaption cs_vd_calc_nrm_rng_bottom = new DataGridColumnGroupCaption(caption4, 2);
				cs_vd_calc_nrm_rng_bottom.HeaderText = rm.GetString("name.columns.result.calc.npl.minus");
				cs_vd_calc_nrm_rng_bottom.MappingName = "calc_nrm_rng_bottom";
				cs_vd_calc_nrm_rng_bottom.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_calc_nrm_rng_bottom.BackgroungColor = DataGridColors.ColorPkeResult;

				// dapt2.calc_max_rng_bottom
				DataGridColumnGroupCaption cs_vd_calc_max_rng_bottom = new DataGridColumnGroupCaption(caption4, 3);
				cs_vd_calc_max_rng_bottom.HeaderText = rm.GetString("name.columns.result.calc.upl.minus");
				cs_vd_calc_max_rng_bottom.MappingName = "calc_max_rng_bottom";
				cs_vd_calc_max_rng_bottom.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vd_calc_max_rng_bottom.BackgroungColor = DataGridColors.ColorPkeResult;

				// dapt2.real_nrm_rng_top
				DataGridColumnGroupCaption cs_vd_real_nrm_rng_top = new DataGridColumnGroupCaption(caption5, 0);
				cs_vd_real_nrm_rng_top.HeaderText = rm.GetString("name.columns.result.real.npl.plus");
				cs_vd_real_nrm_rng_top.MappingName = "real_nrm_rng_top";
				cs_vd_real_nrm_rng_top.Format = DataColumnsFormat.FloatShortFormat;
				cs_vd_real_nrm_rng_top.BackgroungColor = DataGridColors.ColorPkeStandard;

				// dapt1.real_max_rng_top
				DataGridColumnGroupCaption cs_vd_real_max_rng_top = new DataGridColumnGroupCaption(caption5, 1);
				cs_vd_real_max_rng_top.HeaderText = rm.GetString("name.columns.result.real.upl.plus");
				cs_vd_real_max_rng_top.MappingName = "real_max_rng_top";
				cs_vd_real_max_rng_top.Format = DataColumnsFormat.FloatShortFormat;
				cs_vd_real_max_rng_top.BackgroungColor = DataGridColors.ColorPkeStandard;

				DataGridColumnGroupCaption cs_vd_real_nrm_rng_bottom = null;
				DataGridColumnGroupCaption cs_vd_real_max_rng_bottom = null;
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				//{
					// dapt2.real_nrm_rng_bottom
					cs_vd_real_nrm_rng_bottom = new DataGridColumnGroupCaption(caption5, 2);
					cs_vd_real_nrm_rng_bottom.HeaderText = rm.GetString("name.columns.result.real.npl.minus");
					cs_vd_real_nrm_rng_bottom.MappingName = "real_nrm_rng_bottom";
					cs_vd_real_nrm_rng_bottom.Format = DataColumnsFormat.FloatShortFormat;
					cs_vd_real_nrm_rng_bottom.BackgroungColor = DataGridColors.ColorPkeStandard;

					// dapt1.real_max_rng_bottom
					cs_vd_real_max_rng_bottom = new DataGridColumnGroupCaption(caption5, 3);
					cs_vd_real_max_rng_bottom.HeaderText = rm.GetString("name.columns.result.real.upl.minus");
					cs_vd_real_max_rng_bottom.MappingName = "real_max_rng_bottom";
					cs_vd_real_max_rng_bottom.Format = DataColumnsFormat.FloatShortFormat;
					cs_vd_real_max_rng_bottom.BackgroungColor = DataGridColors.ColorPkeStandard;
				//}

				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t2";
				else ts.MappingName = "pqp_du";
				#region было бы правильнее использовать этот код, но с ним почему-то нарушается порядок столбцов

				//ts.GridColumnStyles.Add(cs_vd_name);					//0

				//ts.GridColumnStyles.Add(cs_vd_num_nrm_rng);				//1
				//ts.GridColumnStyles.Add(cs_vd_num_max_rng);				//2
				//ts.GridColumnStyles.Add(cs_vd_num_out_max_rng);			//3

				//ts.GridColumnStyles.Add(cs_vd_real_nrm_rng_top);		// 4
				//ts.GridColumnStyles.Add(cs_vd_real_max_rng_top);		// 5
				//ts.GridColumnStyles.Add(cs_vd_real_nrm_rng_bottom);		// 6
				//ts.GridColumnStyles.Add(cs_vd_real_max_rng_bottom);		// 7

				//ts.GridColumnStyles.Add(cs_vd_calc_nrm_rng_top);		// 8
				//ts.GridColumnStyles.Add(cs_vd_calc_max_rng_top);		// 9
				//ts.GridColumnStyles.Add(cs_vd_calc_nrm_rng_bottom);		// 10
				//ts.GridColumnStyles.Add(cs_vd_calc_max_rng_bottom);		// 11

				//ts.GridColumnStyles.Add(cs_vd_prcnt_nrm_rng);			// 12
				//ts.GridColumnStyles.Add(cs_vd_prcnt_max_rng);			// 13
				//ts.GridColumnStyles.Add(cs_vd_prcnt_out_max_rng);		// 14

				//ts.GridColumnStyles.Add(cs_vd_time_nrm_rng);			// 15
				//ts.GridColumnStyles.Add(cs_vd_time_max_rng);			// 16
				//ts.GridColumnStyles.Add(cs_vd_time_out_max_rng);		// 17

				#endregion

				ts.GridColumnStyles.Add(cs_vd_name);				//0

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_vd_marked);
					ts.GridColumnStyles.Add(cs_vd_not_marked);
				}
				ts.GridColumnStyles.Add(cs_vd_num_nrm_rng);			//1
				ts.GridColumnStyles.Add(cs_vd_num_max_rng);			//2
				ts.GridColumnStyles.Add(cs_vd_num_out_max_rng);		//3

				ts.GridColumnStyles.Add(cs_vd_prcnt_nrm_rng);		//4
				ts.GridColumnStyles.Add(cs_vd_prcnt_max_rng);		//5
				ts.GridColumnStyles.Add(cs_vd_prcnt_out_max_rng);	//6

				ts.GridColumnStyles.Add(cs_vd_time_nrm_rng);		//7
				ts.GridColumnStyles.Add(cs_vd_time_max_rng);		//8
				ts.GridColumnStyles.Add(cs_vd_time_out_max_rng);	//9

				ts.GridColumnStyles.Add(cs_vd_prcnt_nrm_rng_global);			//1
				ts.GridColumnStyles.Add(cs_vd_prcnt_max_rng_global);			//2
				ts.GridColumnStyles.Add(cs_vd_prcnt_out_max_rng_global);		//3

				ts.GridColumnStyles.Add(cs_vd_calc_nrm_rng_top);
				ts.GridColumnStyles.Add(cs_vd_calc_max_rng_top);				// 10/11
				ts.GridColumnStyles.Add(cs_vd_calc_nrm_rng_bottom);
				ts.GridColumnStyles.Add(cs_vd_calc_max_rng_bottom);				// 11/13

				ts.GridColumnStyles.Add(cs_vd_real_nrm_rng_top);				// 12/14
				ts.GridColumnStyles.Add(cs_vd_real_max_rng_top);				// 13/15
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				//{
					ts.GridColumnStyles.Add(cs_vd_real_nrm_rng_bottom);	
					ts.GridColumnStyles.Add(cs_vd_real_max_rng_bottom);	
				//}

				ts.AllowSorting = false;

				// чтобы отображать не "(null)" а прочерк: " - "
				// значения NULL возможны только для верхних и 
				// нижних значений, которые не могут быть расчитаны
				// при слишком коротком измерительном периоде
				// (менее 40 минут).
				for (int i = 0; i < ts.GridColumnStyles.Count; i++)
				{
					ts.GridColumnStyles[i].NullText = " - ";
				}

				dgU_Deviation.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in dgU_DeviationInitTableStyle()");
				throw;
			}
		}

		#endregion

		#region Voltage Unbalance Grid

		/// <summary>
		/// Filling Voltage Unbalance Grid with data
		/// </summary>
		public void drawdgNonSymmetry()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}
			try
			{
                //NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT p.name, dapt1.num_nrm_rng, dapt1.num_max_rng, dapt1.num_out_max_rng, dapt1.real_nrm_rng, dapt1.real_max_rng, dapt1.calc_nrm_rng, dapt1.calc_max_rng FROM parameters p, day_avg_parameters_t1 dapt1 WHERE p.param_id = dapt1.param_id AND dapt1.param_id in (1101, 1102) AND dapt1.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;", conEmDb);

				string query_text, tableName;
                if (curDeviceType_ == EmDeviceType.ETPQP)
                {
					query_text = string.Format(@"SELECT p.name, dapt1.num_nrm_rng, dapt1.num_max_rng, dapt1.num_out_max_rng, dapt1.real_nrm_rng, dapt1.real_max_rng, 
case when dapt2.valid_ku = 0 then 
cast(round(cast(dapt1.calc_nrm_rng as numeric), {0}) as text) else '-' end as calc_nrm_rng, 
dapt1.calc_max_rng 
FROM parameters p, day_avg_parameters_t1 dapt1, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt1.param_id AND dapt1.param_id in (1101, 1102) AND dapt1.datetime_id = {1} AND dapt1.datetime_id = dapt2.datetime_id AND dapt2.param_id = 1001 ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
                }
				else if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					query_text = string.Format(@"SELECT p.name, non.num_marked, non.num_not_marked, non.num_nrm_rng, non.num_max_rng, non.num_out_max_rng, non.real_nrm_rng, non.real_max_rng, 
case when non.valid_nonsymm != 0 
then cast(round(cast(non.calc_nrm_rng as numeric), {0}) as text) else '-' end as calc_nrm_rng, 
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng as numeric), {0}) as text) else '-' end as calc_max_rng
FROM parameters p, pqp_nonsymmetry non WHERE p.param_id = non.param_id AND non.param_id in (1101, 1102) AND non.datetime_id = {1} ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
				}
				else if (curDeviceType_ == EmDeviceType.EM32)
				{
					query_text = "SELECT p.name, dapt1.num_nrm_rng, dapt1.num_max_rng, dapt1.num_out_max_rng, dapt1.real_nrm_rng, dapt1.real_max_rng, dapt1.calc_nrm_rng, dapt1.calc_max_rng FROM parameters p, day_avg_parameters_t1 dapt1 WHERE p.param_id = dapt1.param_id AND dapt1.param_id in (1101, 1102) AND dapt1.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
				}
                else
                {
                    query_text = "SELECT p.name, dapt1.num_nrm_rng, dapt1.num_max_rng, dapt1.num_out_max_rng, dapt1.real_nrm_rng, dapt1.real_max_rng, dapt1.calc_max_rng FROM parameters p, day_avg_parameters_t1 dapt1 WHERE p.param_id = dapt1.param_id AND dapt1.param_id in (1101, 1102) AND dapt1.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
                }
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					tableName = "day_avg_parameters_t1";
				else tableName = "pqp_nonsymmetry";

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query_text, tableName, ref ds);

				Int64 coef = 3 * TimeSpan.TicksPerSecond;

				// adding calc-fields to the dataset				
				DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," + 
					"num_nrm_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
				c = ds.Tables[0].Columns.Add("prcnt_max_rng", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					"num_max_rng * 100 / (num_nrm_rng + num_max_rng + num_out_max_rng))";
				c = ds.Tables[0].Columns.Add("prcnt_out_max_rng", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," + 
					"num_out_max_rng * 100/ (num_nrm_rng + num_max_rng + num_out_max_rng))";

				Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
				c = ds.Tables[0].Columns.Add("time_nrm_rng", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					"num_nrm_rng / (1 /" +
					coef.ToString() +
					"))";
					//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_nrm_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";
				c = ds.Tables[0].Columns.Add("time_max_rng", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					"num_max_rng / (1 /" +
					coef.ToString() +
					"))";
					//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_max_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";
				c = ds.Tables[0].Columns.Add("time_out_max_rng", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					"num_out_max_rng / (1 /" +
					coef.ToString() +
					"))";
					//"IIF((num_nrm_rng + num_max_rng + num_out_max_rng)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_out_max_rng / (num_nrm_rng + num_max_rng + num_out_max_rng)))";

				if (curDeviceType_ == EmDeviceType.ETPQP ||
				   curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					// приводим верхние и нижние Ku в такой же вид как остальные поля
					// (их формат изменился после превращения их в текст)
					for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
					{
						DataRow curRow = ds.Tables[0].Rows[iRow];
						string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng")].ToString();
						float f_val;
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng")] = "-";

						if (curDeviceType_ == EmDeviceType.ETPQP_A)
						{
							if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng")], out f_val))
								curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng")] = f_val.ToString();
							else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng")] = "-";
						}
					}
				}

				// binding dataset with datagrid
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dgNonSymmetry.SetDataBinding(ds, "day_avg_parameters_t1");
				else dgNonSymmetry.SetDataBinding(ds, "pqp_nonsymmetry");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgNonSymmetry.DataSource, dgNonSymmetry.DataMember];

				currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltUnbChanged);

				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgNonSymmetry()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		private void currencyManager_VoltUnbChanged(object sender, EventArgs e)
		{
			try
			{
				int ipos = dgNonSymmetry.CurrentRowIndex;
				if (ipos == -1)
				{
					barVoltUnbNplSt.WorldTop = float.NaN;
					barVoltUnbNplSt.WorldBottom = float.NaN;
					barVoltUnbUplSt.WorldTop = float.NaN;
					barVoltUnbUplSt.WorldBottom = float.NaN;
					barVoltUnbFake.WorldTop = float.NaN;
					barVoltUnbFake.WorldBottom = float.NaN;
					barVoltUnbUplRes.WorldTop = float.NaN;
					barVoltUnbUplRes.WorldBottom = float.NaN;
					pqpGraphVoltUnb.Invalidate();
					return;
				}
				string pctf = "{0}%";

				GridColumnStylesCollection tmpStyle = dgNonSymmetry.TableStyles[0].GridColumnStyles;
				float NPL_pct_real = Conversions.object_2_float(dgNonSymmetry[ipos,
										tmpStyle.IndexOf(tmpStyle["prcnt_nrm_rng"])]);  // 4
				float NPL_top_real = Conversions.object_2_float(dgNonSymmetry[ipos,
										tmpStyle.IndexOf(tmpStyle["prcnt_nrm_rng"])]);  // 11
				float NPL_bottom_real = 0F;

				barVoltUnbNplSt.WorldTop = NPL_top_real;
				barVoltUnbNplSt.WorldBottom = NPL_bottom_real;
				barVoltUnbNplSt.PercentText = string.Format(pctf, Math.Round(NPL_pct_real, settings_.FloatSigns));

				float UPL_pct_real = Conversions.object_2_float(dgNonSymmetry[ipos,
										tmpStyle.IndexOf(tmpStyle["prcnt_max_rng"])]) + NPL_pct_real;   // 5
				float UPL_top_real = Conversions.object_2_float(dgNonSymmetry[ipos,
										tmpStyle.IndexOf(tmpStyle["real_max_rng"])]);   // 12
				float UPL_bottom_real = 0F;

				barVoltUnbUplSt.WorldTop = UPL_top_real;
				barVoltUnbUplSt.WorldBottom = UPL_bottom_real;
				barVoltUnbUplSt.PercentText = string.Format(pctf, Math.Round(UPL_pct_real, settings_.FloatSigns));

				float UPL_pct_res = 100F;
				float UPL_top_res = Conversions.object_2_float(dgNonSymmetry[ipos,
										tmpStyle.IndexOf(tmpStyle["calc_max_rng"])]);   // 10
				float UPL_bottom_res = 0F;

				barVoltUnbUplRes.WorldTop = UPL_top_res;
				barVoltUnbUplRes.WorldBottom = UPL_bottom_res;
				barVoltUnbUplRes.PercentText = string.Format(pctf, Math.Round(UPL_pct_res, settings_.FloatSigns));

				pqpGraphVoltUnb.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in currencyManager_VoltUnbChanged()");
				throw;
			}
		}

		/// <summary>Initialization of table column styles for Frequency Values grid</summary>       
		private void dgFreqValuesInitTableStyle()
		{
			try
			{
				dgFValues.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
					this.GetType().Assembly);
				string unit_hz = rm.GetString("column_header_units_hz");

				dgFValues.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t7";
				else ts.MappingName = "pqp_df_val";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// ∆f
				DataGridColumnHeaderFormula cs_delta_f =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "∆F" + unit_hz);
				cs_delta_f.MappingName = "d_f";
				cs_delta_f.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_delta_f);

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dgFValues.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in dgFreqValuesInitTableStyle(): ");
				throw;
			}
		}

        /// <summary>Initialization of table column styles for Currents And Voltages grid</summary>       
		private void dgUValuesInitTableStyle()
		{
			try
			{
				dgVolValues.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				string unit_perc = rm.GetString("column_header_units_percent");

				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A) ts.MappingName = "day_avg_parameters_t6";
				else ts.MappingName = "pqp_du_val";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						// δU_y
						DataGridColumnHeaderFormula cs_delta_U_y =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage == "ru" ? "δU_y_" : "δU_s_") + unit_perc);
						cs_delta_U_y.MappingName = "d_u_y";
						cs_delta_U_y.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_y);
					}

					// δU_A
					DataGridColumnHeaderFormula cs_delta_U_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "δU_A_" + unit_perc);
					cs_delta_U_a.MappingName = "d_u_a";
					cs_delta_U_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_delta_U_a);

					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						// δU_B
						DataGridColumnHeaderFormula cs_delta_U_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "δU_B_" + unit_perc);
						cs_delta_U_b.MappingName = "d_u_b";
						cs_delta_U_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_b);

						// δU_C
						DataGridColumnHeaderFormula cs_delta_U_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "δU_C_" + unit_perc);
						cs_delta_U_c.MappingName = "d_u_c";
						cs_delta_U_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_c);

						// δU_AB
						DataGridColumnHeaderFormula cs_delta_U_ab =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "δU_AB_" + unit_perc);
						cs_delta_U_ab.MappingName = "d_u_ab";
						cs_delta_U_ab.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_ab);

						// δU_BC
						DataGridColumnHeaderFormula cs_delta_U_bc =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "δU_BC_" + unit_perc);
						cs_delta_U_bc.MappingName = "d_u_bc";
						cs_delta_U_bc.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_bc);

						// δU_CA
						DataGridColumnHeaderFormula cs_delta_U_ca =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "δU_CA_" + unit_perc);
						cs_delta_U_ca.MappingName = "d_u_ca";
						cs_delta_U_ca.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_ca);
					}
				}
				else  //Et-PQP-A
				{
					// δU_A+
					string param;
					if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
						param = "δU+_AB_";
					else param = "δU+_A_";
					DataGridColumnHeaderFormula cs_delta_U_ap =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, param + unit_perc);
					cs_delta_U_ap.MappingName = "u_a_ab_pos";
					cs_delta_U_ap.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_delta_U_ap);

					// δU_A-
					if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
						param = "δU-_AB_";
					else param = "δU-_A_";
					DataGridColumnHeaderFormula cs_delta_U_an =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, param + unit_perc);
					cs_delta_U_an.MappingName = "u_a_ab_neg";
					cs_delta_U_an.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_delta_U_an);

					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						// δU_B+
						if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
							param = "δU+_BC_";
						else param = "δU+_B_";
						DataGridColumnHeaderFormula cs_delta_U_bp =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, param + unit_perc);
						cs_delta_U_bp.MappingName = "u_b_bc_pos";
						cs_delta_U_bp.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_bp);

						// δU_B-
						if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
							param = "δU-_BC_";
						else param = "δU-_B_";
						DataGridColumnHeaderFormula cs_delta_U_bn =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, param + unit_perc);
						cs_delta_U_bn.MappingName = "u_b_bc_neg";
						cs_delta_U_bn.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_bn);

						// δU_C+
						if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
							param = "δU+_CA_";
						else param = "δU+_C_";
						DataGridColumnHeaderFormula cs_delta_U_cp =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, param + unit_perc);
						cs_delta_U_cp.MappingName = "u_c_ca_pos";
						cs_delta_U_cp.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_cp);

						// δU_C-
						if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
							param = "δU-_CA_";
						else param = "δU-_C_";
						DataGridColumnHeaderFormula cs_delta_U_cn =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, param + unit_perc);
						cs_delta_U_cn.MappingName = "u_c_ca_neg";
						cs_delta_U_cn.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_delta_U_cn);
					}

					// делаем колонку с признаком маркированности невидимой. в таблице она не нужна, нужна для других вычислений
					DataGridColumnHeaderFormula cs_marked =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "marked");
					cs_marked.MappingName = "record_marked";
					cs_marked.Width = 0;
					ts.GridColumnStyles.Add(cs_marked);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count - 1; i++) // кроме первой и последней колонки
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dgVolValues.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in dgUValuesInitTableStyle(): ");
				throw;
			}
		}

		private void dgInterharmInitTableStyle()
		{
			try
			{
				if (curDeviceType_ != EmDeviceType.ETPQP_A) return;

				dgInterharm.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "pqp_interharm_u";
				ts.AllowSorting = false;

				// param_num
				DataGridColumnHeaderFormula cs_param =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_param.HeaderText = rm.GetString("name_columns_parameter");
				cs_param.MappingName = "param_num";
				cs_param.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_param);

				// phase A
				DataGridColumnHeaderFormula cs_ph_a =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "K_Uisg_A_" + unit_v);
				cs_ph_a.MappingName = "val_ph1";
				cs_ph_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_ph_a);

				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					// phase B
					DataGridColumnHeaderFormula cs_ph_b =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "K_Uisg_B_" + unit_v);
					cs_ph_b.MappingName = "val_ph2";
					cs_ph_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_ph_b);

					// phase C
					DataGridColumnHeaderFormula cs_ph_c =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "K_Uisg_C_" + unit_v);
					cs_ph_c.MappingName = "val_ph3";
					cs_ph_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_ph_c);
				}

				dgInterharm.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgInterharmInitTableStyle()");
				throw;
			}
		}

		/// <summary>Initialization of table column's style</summary>
		private void dgNonSymmetryInitTableStyle()
		{
			try
			{
				dgNonSymmetry.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				DataGridGroupCaption caption0 = new DataGridGroupCaption(rm.GetString("name_columns_parameter"), 1);
				DataGridGroupCaption caption6 =
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number"),
					2/*, Color.Beige*/);
				DataGridGroupCaption caption1 = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number"),
					3/*, Color.Beige*/);
				DataGridGroupCaption caption2 = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number")
					+ ", %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption3 = 
					new DataGridGroupCaption(rm.GetString("name_columns_measures_number")
					+ ", " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);

				int num_col_ku = 1;
				if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.ETPQP_A)
					num_col_ku = 2;
				DataGridGroupCaption caption4 = new DataGridGroupCaption(rm.GetString("name_column_measures_result")
					+ ", %", /*-=pqp 95=- 2*/ num_col_ku/*, Color.Honeydew*/);
				DataGridGroupCaption caption5 = new DataGridGroupCaption(rm.GetString("name_columns_standard_value")
					+ ", %", 2/*, Color.Honeydew*/);

				// p.name
				DataGridColumnCellFormula cs_vu_name = new DataGridColumnCellFormula(caption0, 0);
				cs_vu_name.HeaderText = "";
				cs_vu_name.MappingName = "name";
				cs_vu_name.BackgroungColor = DataGridColors.ColorPqpParam;

				//short curColumnNumber = 0;
				DataGridColumnGroupCaption cs_vu_marked = null;
				DataGridColumnGroupCaption cs_vu_not_marked = null;
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					// marked
					cs_vu_marked = new DataGridColumnGroupCaption(caption6, 0);
					cs_vu_marked.HeaderText = rm.GetString("name_columns_marked");
					cs_vu_marked.MappingName = "num_marked";
					cs_vu_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vu_marked.Format = DataColumnsFormat.FloatShortFormat;
					// not_marked
					cs_vu_not_marked = new DataGridColumnGroupCaption(caption6, 1);
					cs_vu_not_marked.HeaderText = rm.GetString("name_columns_not_marked");
					cs_vu_not_marked.MappingName = "num_not_marked";
					cs_vu_not_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vu_not_marked.Format = DataColumnsFormat.FloatShortFormat;
				}

				// dapt1.num_nrm_rng
				DataGridColumnGroupCaption cs_vu_num_nrm_rng = 
					new DataGridColumnGroupCaption(caption1, 0);
				cs_vu_num_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vu_num_nrm_rng.MappingName = "num_nrm_rng";
				cs_vu_num_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_num_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;
				// dapt1.num_max_rng
				DataGridColumnGroupCaption cs_vu_num_max_rng = 
					new DataGridColumnGroupCaption(caption1, 1);
				cs_vu_num_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vu_num_max_rng.MappingName = "num_max_rng";
				cs_vu_num_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_num_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_vu_num_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt1.num_out_max_rng
				DataGridColumnGroupCaption cs_vu_num_out_max_rng = 
					new DataGridColumnGroupCaption(caption1, 2);
				cs_vu_num_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vu_num_out_max_rng.MappingName = "num_out_max_rng";
				cs_vu_num_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_num_out_max_rng.Format = DataColumnsFormat.FloatShortFormat;
				cs_vu_num_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt1.prcnt_nrm_rng
				DataGridColumnGroupCaption cs_vu_prcnt_nrm_rng = new DataGridColumnGroupCaption(caption2, 0);
				cs_vu_prcnt_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vu_prcnt_nrm_rng.MappingName = "prcnt_nrm_rng";
				cs_vu_prcnt_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_prcnt_nrm_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt1.prcnt_max_rng
				DataGridColumnGroupCaption cs_vu_prcnt_max_rng = new DataGridColumnGroupCaption(caption2, 1);
				cs_vu_prcnt_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vu_prcnt_max_rng.MappingName = "prcnt_max_rng";
				cs_vu_prcnt_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_prcnt_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vu_prcnt_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt1.prcnt_out_max_rng
				DataGridColumnGroupCaption cs_vu_prcnt_out_max_rng = new DataGridColumnGroupCaption(caption2, 2);
				cs_vu_prcnt_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vu_prcnt_out_max_rng.MappingName = "prcnt_out_max_rng";
				cs_vu_prcnt_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_prcnt_out_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vu_prcnt_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt1.time_nrm_rng
				DataGridColumnTimespan cs_vu_time_nrm_rng = new DataGridColumnTimespan(caption3, 0);
				cs_vu_time_nrm_rng.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vu_time_nrm_rng.MappingName = "time_nrm_rng";
				cs_vu_time_nrm_rng.BackgroungColor = DataGridColors.ColorCommon;

				// dapt1.time_max_rng
				DataGridColumnTimespan cs_vu_time_max_rng = new DataGridColumnTimespan(caption3, 1);
				cs_vu_time_max_rng.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vu_time_max_rng.MappingName = "time_max_rng";
				cs_vu_time_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_time_max_rng.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt1.time_out_max_rng
				DataGridColumnTimespan cs_vu_time_out_max_rng = new DataGridColumnTimespan(caption3, 2);
				cs_vu_time_out_max_rng.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vu_time_out_max_rng.MappingName = "time_out_max_rng";
				cs_vu_time_out_max_rng.BackgroungColor = DataGridColors.ColorCommon;
				cs_vu_time_out_max_rng.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt1.calc_nrm_rng
				DataGridColumnGroupCaption cs_vu_calc_nrm_rng = null;
				if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					cs_vu_calc_nrm_rng = new DataGridColumnGroupCaption(caption4, 0);
					cs_vu_calc_nrm_rng.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
					cs_vu_calc_nrm_rng.MappingName = "calc_nrm_rng";
					cs_vu_calc_nrm_rng.BackgroungColor = DataGridColors.ColorPkeResult;
					cs_vu_calc_nrm_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				}

				// dapt1.calc_max_rng
				DataGridColumnGroupCaption cs_vu_calc_max_rng = new DataGridColumnGroupCaption(caption4,
					/*-=pqp 95=- 1*/ num_col_ku - 1);
				cs_vu_calc_max_rng.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vu_calc_max_rng.MappingName = "calc_max_rng";
				cs_vu_calc_max_rng.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vu_calc_max_rng.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt1.real_nrm_rng
				DataGridColumnGroupCaption cs_vu_real_nrm_rng = new DataGridColumnGroupCaption(caption5, 0);
				cs_vu_real_nrm_rng.HeaderText = rm.GetString("name.params.npl.short");
				cs_vu_real_nrm_rng.MappingName = "real_nrm_rng";
				cs_vu_real_nrm_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vu_real_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;

				// dapt1.real_max_rng
				DataGridColumnGroupCaption cs_vu_real_max_rng = new DataGridColumnGroupCaption(caption5, 1);
				cs_vu_real_max_rng.HeaderText = rm.GetString("name.params.upl.short");
				cs_vu_real_max_rng.MappingName = "real_max_rng";
				cs_vu_real_max_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vu_real_max_rng.Format = DataColumnsFormat.FloatShortFormat;

				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t1";
				else ts.MappingName = "pqp_nonsymmetry";

				ts.GridColumnStyles.Add(cs_vu_name);

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_vu_marked);
					ts.GridColumnStyles.Add(cs_vu_not_marked);
				}

				ts.GridColumnStyles.Add(cs_vu_num_nrm_rng);
				ts.GridColumnStyles.Add(cs_vu_num_max_rng);
				ts.GridColumnStyles.Add(cs_vu_num_out_max_rng);

				ts.GridColumnStyles.Add(cs_vu_prcnt_nrm_rng);
				ts.GridColumnStyles.Add(cs_vu_prcnt_max_rng);
				ts.GridColumnStyles.Add(cs_vu_prcnt_out_max_rng);

				ts.GridColumnStyles.Add(cs_vu_time_nrm_rng);
				ts.GridColumnStyles.Add(cs_vu_time_max_rng);
				ts.GridColumnStyles.Add(cs_vu_time_out_max_rng);			// 9

				if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_vu_calc_nrm_rng);	    // uncomment for Ku
				}
				ts.GridColumnStyles.Add(cs_vu_calc_max_rng);				// 10/11

				ts.GridColumnStyles.Add(cs_vu_real_nrm_rng);				// 11/12
				ts.GridColumnStyles.Add(cs_vu_real_max_rng);				// 12/13

				dgNonSymmetry.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgNonSymmetryInitTableStyle()");
				throw;
			}
		}

		#endregion

		#region Voltage Nonsinusoidality Grid

		/// <summary>Filling Voltage Nonsinusoidality Grid with data</summary>
		public void drawdgUNonsinusoidality()
		{
			try
			{
				if(curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					_drawdgUNonsinusoidality();
					_drawdgUNonsinusoidality2();
				}
				else
				{
					if(CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
						_drawdgUNonsinusoidality2();
					else _drawdgUNonsinusoidality();
				}

				if (CurConnectScheme == ConnectScheme.Ph1W2)
				{
					barVoltNsPh2UplRes.CaptionColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh2UplRes.BarBorderColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh2UplRes.BarColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh2UplRes.PercentColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh2UplRes.WorldValColor = pqpGraphVoltUnb.BackColor;

					barVoltNsPh3UplRes.CaptionColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh3UplRes.BarBorderColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh3UplRes.BarColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh3UplRes.PercentColor = pqpGraphVoltUnb.BackColor;
					barVoltNsPh3UplRes.WorldValColor = pqpGraphVoltUnb.BackColor;

					splitContainerVoltHarm1.Panel1Collapsed = false;
					splitContainerVoltHarm1.Panel2Collapsed = true;
					NSEnterFocus(dgUNonsinusoidality, EventArgs.Empty);
				}
				else
				{
					barVoltNsPh2UplRes.CaptionColor = barFreqUplRes.CaptionColor;
					barVoltNsPh2UplRes.BarBorderColor = barFreqUplRes.BarBorderColor;
					barVoltNsPh2UplRes.BarColor = barFreqUplRes.BarColor;
					barVoltNsPh2UplRes.PercentColor = barFreqUplRes.PercentColor;
					barVoltNsPh2UplRes.WorldValColor = barFreqUplRes.WorldValColor;

					barVoltNsPh3UplRes.CaptionColor = barFreqUplRes.CaptionColor;
					barVoltNsPh3UplRes.BarBorderColor = barFreqUplRes.BarBorderColor;
					barVoltNsPh3UplRes.BarColor = barFreqUplRes.BarColor;
					barVoltNsPh3UplRes.PercentColor = barFreqUplRes.PercentColor;
					barVoltNsPh3UplRes.WorldValColor = barFreqUplRes.WorldValColor;

					if (CurConnectScheme == ConnectScheme.Unknown)
					{
						CurConnectScheme = ConnectScheme.Ph3W4;
						EmService.WriteToLogFailed("CurConnectScheme was set to 1!");
					}

					//if (curDeviceType_ == EmDeviceType.ETPQP_A)
					//{
					//    splitContainerVoltHarm1.Panel1Collapsed = false;
					//    splitContainerVoltHarm1.Panel2Collapsed = true;
					//    NSEnterFocus(dgUNonsinusoidality, EventArgs.Empty);
					//}
					//else
					//{
					if (CurConnectScheme == ConnectScheme.Ph3W4 ||
						CurConnectScheme == ConnectScheme.Ph3W4_B_calc)
					{
						splitContainerVoltHarm1.Panel1Collapsed = false;
						splitContainerVoltHarm1.Panel2Collapsed = curDeviceType_ == EmDeviceType.ETPQP_A;
						NSEnterFocus(dgUNonsinusoidality, EventArgs.Empty);
					}
					else if (CurConnectScheme == ConnectScheme.Ph3W3 ||
							CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
					{
						splitContainerVoltHarm1.Panel1Collapsed = true;
						splitContainerVoltHarm1.Panel2Collapsed = false;
						NSEnterFocus(dgUNonsinusoidality2, EventArgs.Empty);
					}
					else
					{
						throw (new EmException(
							"Invalid shortConnectionScheme value in drawdgUNonsinusoidality()"));
					}
					//}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgUNonsinusoidality()");
				throw;
			}
		}

		private void _drawdgUNonsinusoidality()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
                string query = String.Empty;

                if (curDeviceType_ == EmDeviceType.ETPQP_A)
                {
                    if (CurConnectScheme != ConnectScheme.Ph1W2)
                    {
						query = string.Format(@"SELECT p.name, non.num_marked, non.num_not_marked, non.num_nrm_rng_ph1, non.num_max_rng_ph1, non.num_out_max_rng_ph1, 
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph1 as numeric), {0}) as text) else '-' end as calc_max_rng_ph1, 
non.num_nrm_rng_ph2, non.num_max_rng_ph2, non.num_out_max_rng_ph2, 
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph2 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph2,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph2 as numeric), {0}) as text) else '-' end as calc_max_rng_ph2,  
non.num_nrm_rng_ph3, non.num_max_rng_ph3, non.num_out_max_rng_ph3, 
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph3 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph3,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph3 as numeric), {0}) as text) else '-' end as calc_max_rng_ph3,
non.real_nrm_rng, non.real_max_rng 
FROM parameters p, pqp_nonsinus non WHERE p.param_id = non.param_id AND (non.param_id between 1201 AND 1240) AND non.datetime_id = {1} ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
                    }
                    else
                    {
						query = string.Format(@"SELECT p.name, non.num_marked, non.num_not_marked, non.num_nrm_rng_ph1, non.num_max_rng_ph1, non.num_out_max_rng_ph1,
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph1 as numeric), {0}) as text) else '-' end as calc_max_rng_ph1,
non.real_nrm_rng, non.real_max_rng 
FROM parameters p, pqp_nonsinus non 
WHERE p.param_id = non.param_id AND (non.param_id between 1201 AND 1240) AND non.datetime_id = {1} ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
                    }
                }
				else if (curDeviceType_ == EmDeviceType.ETPQP)
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = string.Format(@"SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, 
case when dapt2.valid_ku = 0 
then cast(round(cast(dapt4.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1, 
dapt4.calc_max_rng_ph1, dapt4.num_nrm_rng_ph2, dapt4.num_max_rng_ph2, dapt4.num_out_max_rng_ph2, 
case when dapt2.valid_ku = 0 
then cast(round(cast(dapt4.calc_nrm_rng_ph2 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph2, 
dapt4.calc_max_rng_ph2, dapt4.num_nrm_rng_ph3, dapt4.num_max_rng_ph3, dapt4.num_out_max_rng_ph3, 
case when dapt2.valid_ku = 0 
then cast(round(cast(dapt4.calc_nrm_rng_ph3 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph3, 
dapt4.calc_max_rng_ph3, dapt4.real_nrm_rng, dapt4.real_max_rng 
FROM parameters p, day_avg_parameters_t4 dapt4, day_avg_parameters_t2 dapt2 
WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1201 AND 1240) AND dapt4.datetime_id = {1} AND dapt4.datetime_id = dapt2.datetime_id AND dapt2.param_id = 1001 ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
                    }
					else
					{
						query = string.Format("SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, case when dapt2.valid_ku = 0 then cast(round(cast(dapt4.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1201 AND 1240) AND dapt4.datetime_id = {1} AND dapt4.datetime_id = dapt2.datetime_id AND dapt2.param_id = 1001 ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
					}
				}
				else if (curDeviceType_ == EmDeviceType.EM32)
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.num_nrm_rng_ph2, dapt4.num_max_rng_ph2, dapt4.num_out_max_rng_ph2, dapt4.calc_nrm_rng_ph2, dapt4.calc_max_rng_ph2, dapt4.num_nrm_rng_ph3, dapt4.num_max_rng_ph3, dapt4.num_out_max_rng_ph3, dapt4.calc_nrm_rng_ph3, dapt4.calc_max_rng_ph3, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1201 AND 1240) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1201 AND 1240) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
				}
                else
                {
                    if (CurConnectScheme != ConnectScheme.Ph1W2)
                    {
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.num_nrm_rng_ph2, dapt4.num_max_rng_ph2, dapt4.num_out_max_rng_ph2, dapt4.calc_max_rng_ph2, dapt4.num_nrm_rng_ph3, dapt4.num_max_rng_ph3, dapt4.num_out_max_rng_ph3, dapt4.calc_max_rng_ph3, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1201 AND 1240) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
                    }
					else
                    {
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1201 AND 1240) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
                    }
                }

				DataSet ds = new DataSet();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dbService.CreateAndFillDataAdapter(query, "day_avg_parameters_t4", ref ds);
				else dbService.CreateAndFillDataAdapter(query, "pqp_nonsinus", ref ds);

				if (ds.Tables.Count == 0) return;

				Int64 coef = 3 * TimeSpan.TicksPerSecond;

				// TODO: тут добавляются необходимые В ЛЮБОМ СЛУЧАЕ столбцы (для первой фазы)
				// adding calc-fields to the dataset
				DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_nrm_rng_ph1 * 100 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";
				c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," + 
					"num_max_rng_ph1 * 100 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";
				c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," + 
					"num_out_max_rng_ph1 * 100/ (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";

				// original ver. 1.0.4 fixed
				Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
				c = ds.Tables[0].Columns.Add("time_nrm_rng_ph1", System.Type.GetType("System.Int64")); 
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_nrm_rng_ph1 / (1 /" +
					coef.ToString() +
					"))";
					//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_nrm_rng_ph1 /  (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";
				c = ds.Tables[0].Columns.Add("time_max_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_max_rng_ph1 / (1 /" +
					coef.ToString() +
					"))";
					
					//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_max_rng_ph1 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";
				c = ds.Tables[0].Columns.Add("time_out_max_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_out_max_rng_ph1 / (1 /" +
					coef.ToString() +
					"))";
					
					//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_out_max_rng_ph1 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";

				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					// TODO: тут устанавливаем расчетные столбцы для 2ой и 3ей фазы
					// adding calc-fields to the dataset				
					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_nrm_rng_ph2 * 100 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_max_rng_ph2 * 100 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_out_max_rng_ph2 * 100/ (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";

					c = ds.Tables[0].Columns.Add("time_nrm_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_nrm_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";
						
						//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_nrm_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";
					c = ds.Tables[0].Columns.Add("time_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_max_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";
						
						//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_max_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";
					c = ds.Tables[0].Columns.Add("time_out_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_out_max_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";
						
						//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_out_max_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";

					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_nrm_rng_ph3 * 100 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_max_rng_ph3 * 100 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_out_max_rng_ph3 * 100/ (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";

					c = ds.Tables[0].Columns.Add("time_nrm_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_nrm_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";
						
						//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_nrm_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
					c = ds.Tables[0].Columns.Add("time_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_max_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";
						
						//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_max_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
					c = ds.Tables[0].Columns.Add("time_out_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_out_max_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";
						
						//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_out_max_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
				}

				if (curDeviceType_ == EmDeviceType.ETPQP ||
					curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
					{
						DataRow curRow = ds.Tables[0].Rows[iRow];
						string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")].ToString();
						float f_val;
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")] = "-";

						if (CurConnectScheme != ConnectScheme.Ph1W2)
						{
							if (Conversions.object_2_float_en_ru(
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")], out f_val))
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")] = f_val.ToString();
							else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")] = "-";

							if (Conversions.object_2_float_en_ru(
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")], out f_val))
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")] = f_val.ToString();
							else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")] = "-";
						}

						if (curDeviceType_ == EmDeviceType.ETPQP_A)
						{
							if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph1")], out f_val))
								curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph1")] = f_val.ToString();
							else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph1")] = "-";

							if (CurConnectScheme != ConnectScheme.Ph1W2)
							{
								if (Conversions.object_2_float_en_ru(
									curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph2")], out f_val))
									curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph2")] = f_val.ToString();
								else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph2")] = "-";

								if (Conversions.object_2_float_en_ru(
									curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph3")], out f_val))
									curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph3")] = f_val.ToString();
								else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph3")] = "-";
							}
						}
					}
				}

				// binding dataset with datagrid
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dgUNonsinusoidality.SetDataBinding(ds, "day_avg_parameters_t4");
				else dgUNonsinusoidality.SetDataBinding(ds, "pqp_nonsinus");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgUNonsinusoidality.DataSource, dgUNonsinusoidality.DataMember];
				currencyManager.CurrentChanged += 
					new EventHandler(currencyManager_VoltNSChanged);
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in _drawdgUNonsinusoidality()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		void currencyManager_VoltNSChanged(object sender, EventArgs e)
		{
			try
			{
				int numberOfPhases = 0;

				if (dgUNonsinusoidality.DataSource == null) return;

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					if ((dgUNonsinusoidality.DataSource as System.Data.DataSet).Tables[0].Columns.Count > 14)
						numberOfPhases = 3;
					else
						numberOfPhases = 1;
				}
				else
				{
					if ((dgUNonsinusoidality.DataSource as System.Data.DataSet).Tables[0].Columns.Count > 16)
						numberOfPhases = 3;
					else
						numberOfPhases = 1;
				}

				int ipos = dgUNonsinusoidality.CurrentRowIndex;
				if (ipos == -1)
				{
					// для 3ф3пр существует только "междуфазные гармоники", поэтому прежде чем
					// сбросить графики проверяем пустая ли она
					ipos = dgUNonsinusoidality2.CurrentRowIndex;
					if (ipos == -1)
					{
						barVoltNsNplSt.WorldTop = float.NaN;
						barVoltNsNplSt.WorldBottom = float.NaN;
						barVoltNsUplSt.WorldTop = float.NaN;
						barVoltNsUplSt.WorldBottom = float.NaN;
						barVoltNsPh1UplRes.WorldTop = float.NaN;
						barVoltNsPh1UplRes.WorldBottom = float.NaN;
						barVoltNsPh2UplRes.WorldTop = float.NaN;
						barVoltNsPh2UplRes.WorldBottom = float.NaN;
						barVoltNsPh3UplRes.WorldTop = float.NaN;
						barVoltNsPh3UplRes.WorldBottom = float.NaN;
						pqpGraphVoltNS.Invalidate();
						return;
					}
					else  // если не пустая, то рисуем графики по ней
					{
						currencyManager_VoltNS2Changed(sender, e);
						return;
					}
				}

				string pctf = "{0}%";

				GridColumnStylesCollection tmpStyle =
					dgUNonsinusoidality.TableStyles[0].GridColumnStyles;
				float NPL_pct_real = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["prcnt_nrm_rng_ph1"])]);  // 4
				float NPL_top_real = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["real_nrm_rng"])]);       // 31
				float NPL_bottom_real = 0F;

				barVoltNsNplSt.WorldTop = NPL_top_real;
				barVoltNsNplSt.WorldBottom = NPL_bottom_real;
				barVoltNsNplSt.PercentText = string.Format(pctf, Math.Round(NPL_pct_real, 2));

				float UPL_pct_real = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["prcnt_max_rng_ph1"])])  // 5
											 + NPL_pct_real;
				float UPL_top_real = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["real_max_rng"])]);       // 32
				float UPL_bottom_real = 0F;

				barVoltNsUplSt.WorldTop = UPL_top_real;
				barVoltNsUplSt.WorldBottom = UPL_bottom_real;
				barVoltNsUplSt.PercentText = string.Format(pctf, Math.Round(UPL_pct_real, 2));

				ResourceManager rm = new
					ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string ph1 = rm.GetString("name_phases_phaseA");
				string ph2 = rm.GetString("name_phases_phaseB");
				string ph3 = rm.GetString("name_phases_phaseC");
				string tpl = rm.GetString("name_bar_caption_in_upl_res");
				ph1 = string.Format(tpl, ph1);
				ph2 = string.Format(tpl, ph2);
				ph3 = string.Format(tpl, ph3);

				float UPL_pct_res = 100F;
				float UPL_top_res = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["calc_max_rng_ph1"])]); // 10
				float UPL_bottom_res = 0F;

				barVoltNsPh1UplRes.Caption = ph1;
				barVoltNsPh1UplRes.WorldTop = UPL_top_res;
				barVoltNsPh1UplRes.WorldBottom = UPL_bottom_res;
				barVoltNsPh1UplRes.PercentText = string.Format(pctf, Math.Round(UPL_pct_res, 2));

				if (numberOfPhases == 3)
				{
					UPL_pct_res = 100F;
					UPL_top_res = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["calc_max_rng_ph2"])]);  // 20
					UPL_bottom_res = 0F;

					barVoltNsPh2UplRes.Caption = ph2;
					barVoltNsPh2UplRes.WorldTop = UPL_top_res;
					barVoltNsPh2UplRes.WorldBottom = UPL_bottom_res;
					barVoltNsPh2UplRes.PercentText = 
						string.Format(pctf, Math.Round(UPL_pct_res, 2));

					UPL_pct_res = 100F;
					UPL_top_res = Conversions.object_2_float(dgUNonsinusoidality[ipos,
										tmpStyle.IndexOf(tmpStyle["calc_max_rng_ph3"])]); // 30
					UPL_bottom_res = 0F;

					barVoltNsPh3UplRes.Caption = ph3;
					barVoltNsPh3UplRes.WorldTop = UPL_top_res;
					barVoltNsPh3UplRes.WorldBottom = UPL_bottom_res;
					barVoltNsPh3UplRes.PercentText = 
						string.Format(pctf, Math.Round(UPL_pct_res, 2));
				}
				pqpGraphVoltNS.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in currencyManager_VoltNSChanged()");
				throw;
			}
		}

		private void _drawdgUNonsinusoidality2()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
				string query = String.Empty;

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					query = string.Format(@"SELECT p.name, non.num_marked, non.num_not_marked, non.num_nrm_rng_ph1, non.num_max_rng_ph1, non.num_out_max_rng_ph1, 
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph1 as numeric), {0}) as text) else '-' end as calc_max_rng_ph1, 
non.num_nrm_rng_ph2, non.num_max_rng_ph2, non.num_out_max_rng_ph2, 
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph2 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph2,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph2 as numeric), {0}) as text) else '-' end as calc_max_rng_ph2,  
non.num_nrm_rng_ph3, non.num_max_rng_ph3, non.num_out_max_rng_ph3, 
case when non.valid_harm != 0 
then cast(round(cast(non.calc_nrm_rng_ph3 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph3,
case when non.num_not_marked > 0 
then cast(round(cast(non.calc_max_rng_ph3 as numeric), {0}) as text) else '-' end as calc_max_rng_ph3,
non.real_nrm_rng, non.real_max_rng 
FROM parameters p, pqp_nonsinus non WHERE p.param_id = non.param_id AND (non.param_id between 1201 AND 1240) AND non.datetime_id = {1} ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
				}
				else if (curDeviceType_ == EmDeviceType.ETPQP)
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, case when dapt2.valid_ku = 0 then cast(dapt4.calc_nrm_rng_ph1 as text) else '-' end as calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.num_nrm_rng_ph2, dapt4.num_max_rng_ph2, dapt4.num_out_max_rng_ph2, case when dapt2.valid_ku = 0 then cast(dapt4.calc_nrm_rng_ph2 as text) else '-' end as calc_nrm_rng_ph2, dapt4.calc_max_rng_ph2, dapt4.num_nrm_rng_ph3, dapt4.num_max_rng_ph3, dapt4.num_out_max_rng_ph3, case when dapt2.valid_ku = 0 then cast(dapt4.calc_nrm_rng_ph3 as text) else '-' end as calc_nrm_rng_ph3, dapt4.calc_max_rng_ph3, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1301 AND 1340) AND dapt4.datetime_id = " + curDatetimeId_ + " AND dapt4.datetime_id = dapt2.datetime_id AND dapt2.param_id = 1001 ORDER BY p.param_id;";
					}
					else
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, case when dapt2.valid_ku = 0 then cast(dapt4.calc_nrm_rng_ph1 as text) else '-' end as calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4, day_avg_parameters_t2 dapt2 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1301 AND 1340) AND dapt4.datetime_id = " + curDatetimeId_ + " AND dapt4.datetime_id = dapt2.datetime_id AND dapt2.param_id = 1001 ORDER BY p.param_id;";
					}
				}
				else if (curDeviceType_ == EmDeviceType.EM32)
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.num_nrm_rng_ph2, dapt4.num_max_rng_ph2, dapt4.num_out_max_rng_ph2, dapt4.calc_nrm_rng_ph2, dapt4.calc_max_rng_ph2, dapt4.num_nrm_rng_ph3, dapt4.num_max_rng_ph3, dapt4.num_out_max_rng_ph3, dapt4.calc_nrm_rng_ph3, dapt4.calc_max_rng_ph3, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1301 AND 1340) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_nrm_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1301 AND 1340) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
				}
				else
				{
					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.num_nrm_rng_ph2, dapt4.num_max_rng_ph2, dapt4.num_out_max_rng_ph2, dapt4.calc_max_rng_ph2, dapt4.num_nrm_rng_ph3, dapt4.num_max_rng_ph3, dapt4.num_out_max_rng_ph3, dapt4.calc_max_rng_ph3, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1301 AND 1340) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else
					{
						query = "SELECT p.name, dapt4.num_nrm_rng_ph1, dapt4.num_max_rng_ph1, dapt4.num_out_max_rng_ph1, dapt4.calc_max_rng_ph1, dapt4.real_nrm_rng, dapt4.real_max_rng FROM parameters p, day_avg_parameters_t4 dapt4 WHERE p.param_id = dapt4.param_id AND (dapt4.param_id between 1301 AND 1340) AND dapt4.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
				}

				DataSet ds = new DataSet();
				if(curDeviceType_ != EmDeviceType.ETPQP_A)
					dbService.CreateAndFillDataAdapter(query, "day_avg_parameters_t4", ref ds);
				else dbService.CreateAndFillDataAdapter(query, "pqp_nonsinus", ref ds);

				if (ds.Tables.Count == 0) return;

				Int64 coef = 3 * TimeSpan.TicksPerSecond;

				// TODO: тут добавляются необходимые В ЛЮБОМ СЛУЧАЕ столбцы (для первой фазы)
				// adding calc-fields to the dataset
				DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_nrm_rng_ph1 * 100 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";
				c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_max_rng_ph1 * 100 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";
				c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_out_max_rng_ph1 * 100/ (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";

				// original ver. 1.0.4 fixed
				Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
				c = ds.Tables[0].Columns.Add("time_nrm_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_nrm_rng_ph1 / (1 /" +
						coef.ToString() +
						"))";
					//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_nrm_rng_ph1 /  (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";
				c = ds.Tables[0].Columns.Add("time_max_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_max_rng_ph1 / (1 /" +
						coef.ToString() +
						"))";
					//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_max_rng_ph1 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";
				c = ds.Tables[0].Columns.Add("time_out_max_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_out_max_rng_ph1 / (1 /" +
						coef.ToString() +
						"))";
					//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_out_max_rng_ph1 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";


				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					// TODO: тут устанавливаем расчетные столбцы для 2ой и 3ей фазы
					// adding calc-fields to the dataset				
					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_nrm_rng_ph2 * 100 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_max_rng_ph2 * 100 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_out_max_rng_ph2 * 100/ (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";

					c = ds.Tables[0].Columns.Add("time_nrm_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_nrm_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";
						//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_nrm_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";
					c = ds.Tables[0].Columns.Add("time_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_max_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";
						//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_max_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";
					c = ds.Tables[0].Columns.Add("time_out_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_out_max_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";
						//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_out_max_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";

					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_nrm_rng_ph3 * 100 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_max_rng_ph3 * 100 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_out_max_rng_ph3 * 100/ (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";

					c = ds.Tables[0].Columns.Add("time_nrm_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_nrm_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";
						//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_nrm_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
					c = ds.Tables[0].Columns.Add("time_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_max_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";
						//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_max_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
					c = ds.Tables[0].Columns.Add("time_out_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
						"num_out_max_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";
						//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						//timeDuration.ToString() + 
						//" * (num_out_max_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
				}

				if (curDeviceType_ == EmDeviceType.ETPQP ||
					curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
					{
						DataRow curRow = ds.Tables[0].Rows[iRow];
						string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")].ToString();
						float f_val;
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")] = "-";

						if (CurConnectScheme != ConnectScheme.Ph1W2)
						{
							if (Conversions.object_2_float_en_ru(
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")], out f_val))
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")] = f_val.ToString();
							else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")] = "-";

							if (Conversions.object_2_float_en_ru(
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")], out f_val))
								curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")] = f_val.ToString();
							else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")] = "-";
						}
					}
				}

				// binding dataset with datagrid
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				dgUNonsinusoidality2.SetDataBinding(ds, "day_avg_parameters_t4");
				else dgUNonsinusoidality2.SetDataBinding(ds, "pqp_nonsinus");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgUNonsinusoidality2.
DataSource, dgUNonsinusoidality2.DataMember];

				currencyManager.CurrentChanged += new EventHandler(currencyManager_VoltNS2Changed);
				DataView dataView = (DataView)currencyManager.List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in _drawdgUNonsinusoidality2()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

        void currencyManager_VoltNS2Changed(object sender, EventArgs e)
        {
            try
            {
                int ipos = dgUNonsinusoidality2.CurrentRowIndex;
                if (ipos == -1)
                {
                    barVoltNsNplSt.WorldTop = float.NaN;
                    barVoltNsNplSt.WorldBottom = float.NaN;
                    barVoltNsUplSt.WorldTop = float.NaN;
                    barVoltNsUplSt.WorldBottom = float.NaN;
                    barVoltNsPh1UplRes.WorldTop = float.NaN;
                    barVoltNsPh1UplRes.WorldBottom = float.NaN;
                    barVoltNsPh2UplRes.WorldTop = float.NaN;
                    barVoltNsPh2UplRes.WorldBottom = float.NaN;
                    barVoltNsPh3UplRes.WorldTop = float.NaN;
                    barVoltNsPh3UplRes.WorldBottom = float.NaN;
                    pqpGraphVoltNS.Invalidate();
                    return;
                }

                string pctf = "{0}%";

                GridColumnStylesCollection tmpStyle =
                dgUNonsinusoidality2.TableStyles[0].GridColumnStyles;

                float NPL_pct_real = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                        tmpStyle.IndexOf(tmpStyle["prcnt_nrm_rng_ph1"])]);  // 4
                float NPL_top_real = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                        tmpStyle.IndexOf(tmpStyle["real_nrm_rng"])]);     // 31
                float NPL_bottom_real = 0F;

                barVoltNsNplSt.WorldTop = NPL_top_real;
                barVoltNsNplSt.WorldBottom = NPL_bottom_real;
                barVoltNsNplSt.PercentText = string.Format(pctf, Math.Round(NPL_pct_real, 2));

                float UPL_pct_real = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                        tmpStyle.IndexOf(tmpStyle["prcnt_max_rng_ph1"])])  // 5
                                             + NPL_pct_real;
                float UPL_top_real = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                        tmpStyle.IndexOf(tmpStyle["real_max_rng"])]);   // 32
                float UPL_bottom_real = 0F;

                barVoltNsUplSt.WorldTop = UPL_top_real;
                barVoltNsUplSt.WorldBottom = UPL_bottom_real;
                barVoltNsUplSt.PercentText = string.Format(pctf, Math.Round(UPL_pct_real, 2));

                ResourceManager rm = new
                    ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
                string ph1 = rm.GetString("name_phases_phaseAB");
                string ph2 = rm.GetString("name_phases_phaseBC");
                string ph3 = rm.GetString("name_phases_phaseCA");
                string tpl = rm.GetString("name_bar_caption_in_upl_res");
                ph1 = string.Format(tpl, ph1);
                ph2 = string.Format(tpl, ph2);
                ph3 = string.Format(tpl, ph3);

                float UPL_pct_res = 100F;
                float UPL_top_res = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                    tmpStyle.IndexOf(tmpStyle["calc_max_rng_ph1"])]);  // 10
                float UPL_bottom_res = 0F;

                barVoltNsPh1UplRes.Caption = ph1;
                barVoltNsPh1UplRes.WorldTop = UPL_top_res;
                barVoltNsPh1UplRes.WorldBottom = UPL_bottom_res;
                barVoltNsPh1UplRes.PercentText = string.Format(pctf, Math.Round(UPL_pct_res, 2));

                if (CurConnectScheme != ConnectScheme.Ph1W2)
                {
                    UPL_pct_res = 100F;
                    UPL_top_res = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                        tmpStyle.IndexOf(tmpStyle["calc_max_rng_ph2"])]); // 20
                    UPL_bottom_res = 0F;

                    barVoltNsPh2UplRes.Caption = ph2;
                    barVoltNsPh2UplRes.WorldTop = UPL_top_res;
                    barVoltNsPh2UplRes.WorldBottom = UPL_bottom_res;
                    barVoltNsPh2UplRes.PercentText = 
						string.Format(pctf, Math.Round(UPL_pct_res, 2));

                    UPL_pct_res = 100F;
                    UPL_top_res = Conversions.object_2_float(dgUNonsinusoidality2[ipos,
                                        tmpStyle.IndexOf(tmpStyle["calc_max_rng_ph3"])]); // 30
                    UPL_bottom_res = 0F;

                    barVoltNsPh3UplRes.Caption = ph3;
                    barVoltNsPh3UplRes.WorldTop = UPL_top_res;
                    barVoltNsPh3UplRes.WorldBottom = UPL_bottom_res;
                    barVoltNsPh3UplRes.PercentText = 
						string.Format(pctf, Math.Round(UPL_pct_res, 2));
                }

                pqpGraphVoltNS.Invalidate();
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in currencyManager_VoltNS2Changed(): ");
                throw;
            }
        }

		/// <summary>Initialization of table column's style</summary>
		private void dgUNonsinusInitTableStyle()
		{
			if (curDeviceType_ != EmDeviceType.ETPQP_A)
			{
				dgUNonsinusInitTableStyle1();
				dgUNonsinusInitTableStyle2();
			}
			else
			{
				if(CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
					dgUNonsinusInitTableStyle2();
				else dgUNonsinusInitTableStyle1();
			}
		}

		private void dgUNonsinusInitTableStyle1()
		{
			try
			{
				dgUNonsinusoidality.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				DataGridGroupCaption caption0 = new DataGridGroupCaption(rm.GetString("name_columns_parameter"), 1);
				DataGridGroupCaption caption14 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number"), 2/*, Color.Beige*/);
				DataGridGroupCaption caption1 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseA") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption2 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseA") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption3 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseA") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				int num_col_ku = 1;
				if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.ETPQP_A)
					num_col_ku = 2;
				DataGridGroupCaption caption4 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseA") + "), %", num_col_ku);

				DataGridGroupCaption caption5 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseB") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption6 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseB") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption7 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseB") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption8 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseB") + "), %", num_col_ku);

				DataGridGroupCaption caption9 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseC") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption10 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseC") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption11 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseC") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption12 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseC") + "), %", num_col_ku);
				DataGridGroupCaption caption13 = new DataGridGroupCaption(rm.GetString("name_columns_standard_value") + ", %", 2/*, Color.Honeydew*/);

				// Common
				// p.name
				DataGridColumnCellFormula cs_vn_name = new DataGridColumnCellFormula(caption0, 0);
				cs_vn_name.HeaderText = "";
				cs_vn_name.MappingName = "name";
				cs_vn_name.BackgroungColor = DataGridColors.ColorPqpParam;

				//short curColumnNumber = 0;
				DataGridColumnGroupCaption cs_vn_marked = null;
				DataGridColumnGroupCaption cs_vn_not_marked = null;
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					// marked
					cs_vn_marked = new DataGridColumnGroupCaption(caption14, 0);
					cs_vn_marked.HeaderText = rm.GetString("name_columns_marked");
					cs_vn_marked.MappingName = "num_marked";
					cs_vn_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vn_marked.Format = DataColumnsFormat.FloatShortFormat;
					// not_marked
					cs_vn_not_marked = new DataGridColumnGroupCaption(caption14, 1);
					cs_vn_not_marked.HeaderText = rm.GetString("name_columns_not_marked");
					cs_vn_not_marked.MappingName = "num_not_marked";
					cs_vn_not_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vn_not_marked.Format = DataColumnsFormat.FloatShortFormat;
				}

				// Phase A
				// dapt4.num_nrm_rng
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph1 = 
					new DataGridColumnGroupCaption(caption1, 0);
				cs_vn_num_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph1.MappingName = "num_nrm_rng_ph1";
				cs_vn_num_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph1 = 
					new DataGridColumnGroupCaption(caption1, 1);
				cs_vn_num_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph1.MappingName = "num_max_rng_ph1";
				cs_vn_num_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph1 = 
					new DataGridColumnGroupCaption(caption1, 2);
				cs_vn_num_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph1.MappingName = "num_out_max_rng_ph1";
				cs_vn_num_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption2, 0);
				cs_vn_prcnt_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph1.MappingName = "prcnt_nrm_rng_ph1";
				cs_vn_prcnt_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph1 = new DataGridColumnGroupCaption(caption2, 1);
				cs_vn_prcnt_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph1.MappingName = "prcnt_max_rng_ph1";
				cs_vn_prcnt_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph1 = new DataGridColumnGroupCaption(caption2, 2);
				cs_vn_prcnt_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph1.MappingName = "prcnt_out_max_rng_ph1";
				cs_vn_prcnt_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph1
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph1 = new DataGridColumnTimespan(caption3, 0);
				cs_vn_time_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph1.MappingName = "time_nrm_rng_ph1";
				cs_vn_time_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph1
				DataGridColumnTimespan cs_vn_time_max_rng_ph1 = new DataGridColumnTimespan(caption3, 1);
				cs_vn_time_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph1.MappingName = "time_max_rng_ph1";
				cs_vn_time_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph1
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph1 = new DataGridColumnTimespan(caption3, 2);
				cs_vn_time_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph1.MappingName = "time_out_max_rng_ph1";
				cs_vn_time_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph1
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption4, 0);
				cs_vn_calc_nrm_rng_ph1.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph1.MappingName = "calc_nrm_rng_ph1";
				cs_vn_calc_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph1 = 
					new DataGridColumnGroupCaption(caption4, /*-=pqp 95=- 1*/ num_col_ku - 1);
				cs_vn_calc_max_rng_ph1.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph1.MappingName = "calc_max_rng_ph1";
				cs_vn_calc_max_rng_ph1.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Phases B
				// dapt4.num_nrm_rng
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption5, 0);
				cs_vn_num_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph2.MappingName = "num_nrm_rng_ph2";
				cs_vn_num_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph2 = new DataGridColumnGroupCaption(caption5, 1);
				cs_vn_num_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph2.MappingName = "num_max_rng_ph2";
				cs_vn_num_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph2 = new DataGridColumnGroupCaption(caption5, 2);
				cs_vn_num_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph2.MappingName = "num_out_max_rng_ph2";
				cs_vn_num_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption6, 0);
				cs_vn_prcnt_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph2.MappingName = "prcnt_nrm_rng_ph2";
				cs_vn_prcnt_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph2 = new DataGridColumnGroupCaption(caption6, 1);
				cs_vn_prcnt_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph2.MappingName = "prcnt_max_rng_ph2";
				cs_vn_prcnt_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph2 = new DataGridColumnGroupCaption(caption6, 2);
				cs_vn_prcnt_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph2.MappingName = "prcnt_out_max_rng_ph2";
				cs_vn_prcnt_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph2
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph2 = new DataGridColumnTimespan(caption7, 0);
				cs_vn_time_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph2.MappingName = "time_nrm_rng_ph2";
				cs_vn_time_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph2
				DataGridColumnTimespan cs_vn_time_max_rng_ph2 = new DataGridColumnTimespan(caption7, 1);
				cs_vn_time_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph2.MappingName = "time_max_rng_ph2";
				cs_vn_time_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph2
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph2 = new DataGridColumnTimespan(caption7, 2);
				cs_vn_time_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph2.MappingName = "time_out_max_rng_ph2";
				cs_vn_time_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph2
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption8, 0);
				cs_vn_calc_nrm_rng_ph2.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph2.MappingName = "calc_nrm_rng_ph2";
				cs_vn_calc_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph2 =
					new DataGridColumnGroupCaption(caption8, /*-=pqp 95=- 1*/ num_col_ku - 1);
				cs_vn_calc_max_rng_ph2.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph2.MappingName = "calc_max_rng_ph2";
				cs_vn_calc_max_rng_ph2.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Phase C
				// dapt4.num_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption9, 0);
				cs_vn_num_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph3.MappingName = "num_nrm_rng_ph3";
				cs_vn_num_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph3 = new DataGridColumnGroupCaption(caption9, 1);
				cs_vn_num_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph3.MappingName = "num_max_rng_ph3";
				cs_vn_num_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph3 = new DataGridColumnGroupCaption(caption9, 2);
				cs_vn_num_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph3.MappingName = "num_out_max_rng_ph3";
				cs_vn_num_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption10, 0);
				cs_vn_prcnt_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph3.MappingName = "prcnt_nrm_rng_ph3";
				cs_vn_prcnt_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph3 = new DataGridColumnGroupCaption(caption10, 1);
				cs_vn_prcnt_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph3.MappingName = "prcnt_max_rng_ph3";
				cs_vn_prcnt_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph3 = new DataGridColumnGroupCaption(caption10, 2);
				cs_vn_prcnt_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph3.MappingName = "prcnt_out_max_rng_ph3";
				cs_vn_prcnt_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph3

				DataGridColumnTimespan cs_vn_time_nrm_rng_ph3 = new DataGridColumnTimespan(caption11, 0);
				cs_vn_time_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph3.MappingName = "time_nrm_rng_ph3";
				cs_vn_time_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph3
				DataGridColumnTimespan cs_vn_time_max_rng_ph3 = new DataGridColumnTimespan(caption11, 1);
				cs_vn_time_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph3.MappingName = "time_max_rng_ph3";
				cs_vn_time_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph3
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph3 = new DataGridColumnTimespan(caption11, 2);
				cs_vn_time_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph3.MappingName = "time_out_max_rng_ph3";
				cs_vn_time_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption12, 0);
				cs_vn_calc_nrm_rng_ph3.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph3.MappingName = "calc_nrm_rng_ph3";
				cs_vn_calc_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph3 =
					new DataGridColumnGroupCaption(caption12, /*-=pqp 95=- 1*/num_col_ku - 1);
				cs_vn_calc_max_rng_ph3.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph3.MappingName = "calc_max_rng_ph3";
				cs_vn_calc_max_rng_ph3.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Common
				// dapt4.real_nrm_rng
				DataGridColumnGroupCaption cs_vn_real_nrm_rng = new DataGridColumnGroupCaption(caption13, 0);
				cs_vn_real_nrm_rng.HeaderText = rm.GetString("name.params.npl.short");
				cs_vn_real_nrm_rng.MappingName = "real_nrm_rng";
				cs_vn_real_nrm_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vn_real_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.real_max_rng
				DataGridColumnGroupCaption cs_vn_real_max_rng = new DataGridColumnGroupCaption(caption13, 1);
				cs_vn_real_max_rng.HeaderText = rm.GetString("name.params.upl.short");
				cs_vn_real_max_rng.MappingName = "real_max_rng";
				cs_vn_real_max_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vn_real_max_rng.Format = DataColumnsFormat.FloatShortFormat;

				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t4";
				else ts.MappingName = "pqp_nonsinus";

				ts.GridColumnStyles.Add(cs_vn_name);								// 0

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_vn_marked);
					ts.GridColumnStyles.Add(cs_vn_not_marked);
				}

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph1);						// 1
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph1);						// 2
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph1);					// 3

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph1);					// 4
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph1);					// 5
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph1);				// 6

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph1);					// 7
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph1);					// 8
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph1);				// 9

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph1);		// uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph1);					// 10/11

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph2);						// 11/12
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph2);						// 12/13
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph2);					// 13/14

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph2);					// 14/15
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph2);					// 15/16
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph2);				// 16/17

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph2);					// 17/18
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph2);					// 18/19
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph2);				// 19/20

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph2);		//  uncomment for Ku            
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph2);					// 20/22

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph3);						// 21/23
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph3);						// 22/24
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph3);					// 23/25

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph3);	 				// 24/26				
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph3);	 				// 25/27
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph3);				// 26/28

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph3);					// 27/29
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph3);					// 28/30
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph3);				// 29/31

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph3);		// uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph3);					// 30/33

				ts.GridColumnStyles.Add(cs_vn_real_nrm_rng);						// 31/34
				ts.GridColumnStyles.Add(cs_vn_real_max_rng);						// 32/35

				dgUNonsinusoidality.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgUNonsinusInitTableStyle1()");
				throw;
			}
		}

		private void dgUNonsinusInitTableStyle2()
		{
			try
			{
				dgUNonsinusoidality2.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				DataGridGroupCaption caption0 = new DataGridGroupCaption(rm.GetString("name_columns_parameter"), 1);
				DataGridGroupCaption caption14 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number"), 2/*, Color.Beige*/);
				DataGridGroupCaption caption1 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseAB") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption2 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseAB") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption3 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseAB") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption4 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseAB") + "), %", 1/*, Color.Honeydew*/);

				DataGridGroupCaption caption5 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseBC") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption6 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseBC") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption7 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseBC") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption8 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseBC") + "), %", 1/*, Color.Honeydew*/);

				DataGridGroupCaption caption9 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseCA") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption10 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseCA") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption11 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseCA") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption12 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseCA") + "), %", 1/*, Color.Honeydew*/);
				DataGridGroupCaption caption13 = new DataGridGroupCaption(rm.GetString("name_columns_standard_value") + ", %", 2/*, Color.Honeydew*/);

				// Common
				// p.name
				DataGridColumnCellFormula cs_vn_name = new DataGridColumnCellFormula(caption0, 0);
				cs_vn_name.HeaderText = "";
				cs_vn_name.MappingName = "name";
				cs_vn_name.BackgroungColor = DataGridColors.ColorPqpParam;

				DataGridColumnGroupCaption cs_vn_marked = null;
				DataGridColumnGroupCaption cs_vn_not_marked = null;
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					// marked
					cs_vn_marked = new DataGridColumnGroupCaption(caption14, 0);
					cs_vn_marked.HeaderText = rm.GetString("name_columns_marked");
					cs_vn_marked.MappingName = "num_marked";
					cs_vn_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vn_marked.Format = DataColumnsFormat.FloatShortFormat;
					// not_marked
					cs_vn_not_marked = new DataGridColumnGroupCaption(caption14, 1);
					cs_vn_not_marked.HeaderText = rm.GetString("name_columns_not_marked");
					cs_vn_not_marked.MappingName = "num_not_marked";
					cs_vn_not_marked.BackgroungColor = DataGridColors.ColorCommon;
					cs_vn_not_marked.Format = DataColumnsFormat.FloatShortFormat;
				}

				// Phase A
				// dapt4.num_nrm_rng
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption1, 0);
				cs_vn_num_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph1.MappingName = "num_nrm_rng_ph1";
				cs_vn_num_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph1 = new DataGridColumnGroupCaption(caption1, 1);
				cs_vn_num_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph1.MappingName = "num_max_rng_ph1";
				cs_vn_num_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph1 = new DataGridColumnGroupCaption(caption1, 2);
				cs_vn_num_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph1.MappingName = "num_out_max_rng_ph1";
				cs_vn_num_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption2, 0);
				cs_vn_prcnt_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph1.MappingName = "prcnt_nrm_rng_ph1";
				cs_vn_prcnt_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph1 = new DataGridColumnGroupCaption(caption2, 1);
				cs_vn_prcnt_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph1.MappingName = "prcnt_max_rng_ph1";
				cs_vn_prcnt_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph1 = new DataGridColumnGroupCaption(caption2, 2);
				cs_vn_prcnt_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph1.MappingName = "prcnt_out_max_rng_ph1";
				cs_vn_prcnt_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph1
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph1 = new DataGridColumnTimespan(caption3, 0);
				cs_vn_time_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph1.MappingName = "time_nrm_rng_ph1";
				cs_vn_time_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph1
				DataGridColumnTimespan cs_vn_time_max_rng_ph1 = new DataGridColumnTimespan(caption3, 1);
				cs_vn_time_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph1.MappingName = "time_max_rng_ph1";
				cs_vn_time_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph1
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph1 = new DataGridColumnTimespan(caption3, 2);
				cs_vn_time_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph1.MappingName = "time_out_max_rng_ph1";
				cs_vn_time_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph1
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption4, 0);
				cs_vn_calc_nrm_rng_ph1.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph1.MappingName = "calc_nrm_rng_ph1";
				cs_vn_calc_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph1 = new DataGridColumnGroupCaption(caption4, /*-=pqp 95=- 1*/ 0);
				cs_vn_calc_max_rng_ph1.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph1.MappingName = "calc_max_rng_ph1";
				cs_vn_calc_max_rng_ph1.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Phases B
				// dapt4.num_nrm_rng
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption5, 0);
				cs_vn_num_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph2.MappingName = "num_nrm_rng_ph2";
				cs_vn_num_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph2 = new DataGridColumnGroupCaption(caption5, 1);
				cs_vn_num_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph2.MappingName = "num_max_rng_ph2";
				cs_vn_num_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph2 = new DataGridColumnGroupCaption(caption5, 2);
				cs_vn_num_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph2.MappingName = "num_out_max_rng_ph2";
				cs_vn_num_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption6, 0);
				cs_vn_prcnt_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph2.MappingName = "prcnt_nrm_rng_ph2";
				cs_vn_prcnt_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph2 = new DataGridColumnGroupCaption(caption6, 1);
				cs_vn_prcnt_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph2.MappingName = "prcnt_max_rng_ph2";
				cs_vn_prcnt_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph2 = new DataGridColumnGroupCaption(caption6, 2);
				cs_vn_prcnt_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph2.MappingName = "prcnt_out_max_rng_ph2";
				cs_vn_prcnt_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph2
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph2 = new DataGridColumnTimespan(caption7, 0);
				cs_vn_time_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph2.MappingName = "time_nrm_rng_ph2";
				cs_vn_time_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph2
				DataGridColumnTimespan cs_vn_time_max_rng_ph2 = new DataGridColumnTimespan(caption7, 1);
				cs_vn_time_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph2.MappingName = "time_max_rng_ph2";
				cs_vn_time_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph2
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph2 = new DataGridColumnTimespan(caption7, 2);
				cs_vn_time_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph2.MappingName = "time_out_max_rng_ph2";
				cs_vn_time_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph2
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption8, 0);
				cs_vn_calc_nrm_rng_ph2.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph2.MappingName = "calc_nrm_rng_ph2";
				cs_vn_calc_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph2 = new DataGridColumnGroupCaption(caption8, /*-=pqp 95=- 1*/ 0);
				cs_vn_calc_max_rng_ph2.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph2.MappingName = "calc_max_rng_ph2";
				cs_vn_calc_max_rng_ph2.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Phase C
				// dapt4.num_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption9, 0);
				cs_vn_num_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph3.MappingName = "num_nrm_rng_ph3";
				cs_vn_num_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph3 = new DataGridColumnGroupCaption(caption9, 1);
				cs_vn_num_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph3.MappingName = "num_max_rng_ph3";
				cs_vn_num_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph3 = new DataGridColumnGroupCaption(caption9, 2);
				cs_vn_num_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph3.MappingName = "num_out_max_rng_ph3";
				cs_vn_num_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption10, 0);
				cs_vn_prcnt_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph3.MappingName = "prcnt_nrm_rng_ph3";
				cs_vn_prcnt_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph3 = new DataGridColumnGroupCaption(caption10, 1);
				cs_vn_prcnt_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph3.MappingName = "prcnt_max_rng_ph3";
				cs_vn_prcnt_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph3 = new DataGridColumnGroupCaption(caption10, 2);
				cs_vn_prcnt_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph3.MappingName = "prcnt_out_max_rng_ph3";
				cs_vn_prcnt_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph3

				DataGridColumnTimespan cs_vn_time_nrm_rng_ph3 = new DataGridColumnTimespan(caption11, 0);
				cs_vn_time_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph3.MappingName = "time_nrm_rng_ph3";
				cs_vn_time_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph3
				DataGridColumnTimespan cs_vn_time_max_rng_ph3 = new DataGridColumnTimespan(caption11, 1);
				cs_vn_time_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph3.MappingName = "time_max_rng_ph3";
				cs_vn_time_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph3
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph3 = new DataGridColumnTimespan(caption11, 2);
				cs_vn_time_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph3.MappingName = "time_out_max_rng_ph3";
				cs_vn_time_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption12, 0);
				cs_vn_calc_nrm_rng_ph3.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph3.MappingName = "calc_nrm_rng_ph3";
				cs_vn_calc_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph3 =
					new DataGridColumnGroupCaption(caption12, /*-=pqp 95=- 1*/0);
				cs_vn_calc_max_rng_ph3.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph3.MappingName = "calc_max_rng_ph3";
				cs_vn_calc_max_rng_ph3.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Common
				// dapt4.real_nrm_rng
				DataGridColumnGroupCaption cs_vn_real_nrm_rng = new DataGridColumnGroupCaption(caption13, 0);
				cs_vn_real_nrm_rng.HeaderText = rm.GetString("name.params.npl.short");
				cs_vn_real_nrm_rng.MappingName = "real_nrm_rng";
				cs_vn_real_nrm_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vn_real_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.real_max_rng
				DataGridColumnGroupCaption cs_vn_real_max_rng = new DataGridColumnGroupCaption(caption13, 1);
				cs_vn_real_max_rng.HeaderText = rm.GetString("name.params.upl.short");
				cs_vn_real_max_rng.MappingName = "real_max_rng";
				cs_vn_real_max_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vn_real_max_rng.Format = DataColumnsFormat.FloatShortFormat;

				DataGridTableStyle ts = new DataGridTableStyle();
				if(curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t4";
				else ts.MappingName = "pqp_nonsinus";

				ts.GridColumnStyles.Add(cs_vn_name);								// 0

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_vn_marked);								// 1
					ts.GridColumnStyles.Add(cs_vn_not_marked);							// 2
				}

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph1);						// 3
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph1);						// 4
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph1);					// 5

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph1);					// 6
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph1);					// 7
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph1);				// 8

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph1);					// 9
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph1);					// 10
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph1);				// 11

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph1);		// uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph1);					// 12/13

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph2);	
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph2);	
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph2);	

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph2);
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph2);
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph2);

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph2);
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph2);
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph2);

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph2);		// uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph2);

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph3);	
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph3);
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph3);	

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph3);			
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph3);	
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph3);

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph3);	
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph3);
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph3);

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph3);		//  uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph3);

				ts.GridColumnStyles.Add(cs_vn_real_nrm_rng);
				ts.GridColumnStyles.Add(cs_vn_real_max_rng);

				dgUNonsinusoidality2.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgUNonsinusInitTableStyle2()");
				throw;
			}
		}

		private void NSEnterFocus(object sender, EventArgs e)
		{
			try
			{
				if ((sender as DataGrid).Name == "dgUNonsinusoidality")
				{
					currencyManager_VoltNSChanged(this, e);
				}
				if ((sender as DataGrid).Name == "dgUNonsinusoidality2")
				{
					currencyManager_VoltNS2Changed(this, e);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in NSEnterFocus()");
				throw;
			}
		}

		#endregion

		#region Dips Grid

		/// <summary>
		/// Filling Dips Grid with data
		/// </summary>
		public void drawdgDips(string devVersion)
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
                return;
			}
			try
			{
				string query_text = String.Empty;
				if (curDeviceType_ == EmDeviceType.EM32)
				{
					string phaseText;
					Int64 res = dbService.ExecuteScalarInt64("SELECT COUNT(*) FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('*4')");
					if (res > 0) phaseText = "*4";
					else phaseText = "*";

					if (settings_.CurrentLanguage == "ru")
					{
						//query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
						query_text = "SELECT substring(phase from 1 for 1) AS phase, common_duration, common_number, max_period_period, (max_period_value / 100) as max_period_value, max_value_period, (max_value_value / 100) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '" + phaseText + "') ORDER BY phase";
					}
					else if (settings_.CurrentLanguage == "en")
					{
						//query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
						query_text = "SELECT substring(phase from 1 for 1) AS phase, common_duration, common_number, max_period_period, (max_period_value / 100) as max_period_value, max_value_period, (max_value_value / 100) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '" + phaseText + "') ORDER BY phase";
					}
				}
				else if(curDeviceType_ == EmDeviceType.EM33T ||
						curDeviceType_ == EmDeviceType.EM31K ||
						curDeviceType_ == EmDeviceType.EM33T1)
				{
					if (settings_.CurrentLanguage == "ru")
					{
						//query_text = "SELECT phase AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1";
						query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
					}
					else if (settings_.CurrentLanguage == "en")
					{
						//query_text = "SELECT phase AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, (1 - max_period_value) as max_period_value, max_value_period, (1 - max_value_value) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1";
						query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
					}
				}
				else if (curDeviceType_ == EmDeviceType.ETPQP)
				{
					string phaseText;
					Int64 res = dbService.ExecuteScalarInt64("SELECT COUNT(*) FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('*4')");
					if (res > 0) phaseText = "*4";
					else phaseText = "*";

					if (settings_.CurrentLanguage == "ru")
					{
						//query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
						query_text = "SELECT substring(phase from 1 for 1) AS phase, common_duration, common_number, max_period_period, (max_period_value / 100) as max_period_value, max_value_period, (max_value_value / 100) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '" + phaseText + "') ORDER BY phase";
					}
					else if (settings_.CurrentLanguage == "en")
					{
						//query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
						query_text = "SELECT substring(phase from 1 for 1) AS phase, common_duration, common_number, max_period_period, (max_period_value / 100) as max_period_value, max_value_period, (max_value_value / 100) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('A', 'B', 'C', '" + phaseText + "') ORDER BY phase";
					}
				}
				else if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					if (Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_ADDED_DIP_OVER180)
					{
						query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20, dns.num_20_till_60, dns.num_over_60, dns.num_over_180 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND (dns.param_id BETWEEN 3047 AND 3052) AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else if (Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					{
						query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND (dns.param_id BETWEEN 3057 AND 3061) AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else // the oldest version
					{
						query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20, dns.num_20_till_60, dns.num_over_60,  '-' as num_over_180 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND (dns.param_id BETWEEN 3047 AND 3052) AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
				}

				DataSet ds = new DataSet();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dbService.CreateAndFillDataAdapter(query_text, "day_avg_parameters_t3", ref ds);
				else dbService.CreateAndFillDataAdapter(query_text, "pqp_dip_swell", ref ds);

				if ((CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
					&& curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					if (ds.Tables.Count > 0)
					{
						for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
						{
							// currents
							for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
							{
								if (ds.Tables[0].Columns[i].Caption.Contains("phase"))
								{
									if (!(ds.Tables[0].Rows[r][i] is DBNull))
									{
										string phase = (ds.Tables[0].Rows[r][i]).ToString();
										switch (phase)
										{
											case "A": phase = "AB"; break;
											case "B": phase = "BC"; break;
											case "C": phase = "CA"; break;
										}
										ds.Tables[0].Rows[r][i] = phase;
									}
								}
							}
						}
					}
					ds.AcceptChanges();
				}

				// binding dataset with datagrid
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dgDips.SetDataBinding(ds, "day_avg_parameters_t3");
				else dgDips.SetDataBinding(ds, "pqp_dip_swell");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgDips.DataSource, dgDips.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgDips()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Filling Dips2 Grid with data
		/// </summary>
		public void drawdgDips2()
		{
			if (curDeviceType_ == EmDeviceType.EM33T ||
				curDeviceType_ == EmDeviceType.EM31K ||
				curDeviceType_ == EmDeviceType.EM33T1)
				return;
			//if (curDeviceType_ == EmDeviceType.ETPQP_A)
			//    return;

			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}
			try
			{
				string query_text = String.Empty;

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					if (settings_.CurrentLanguage == "ru")
					{
						//query_text = "SELECT phase AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, max_period_value, max_value_period, max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1";
						query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, (max_period_value / 100) as max_period_value, max_value_period, (max_value_value / 100) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('AB', 'BC', 'CA', '*') ORDER BY phase";
					}
					else if (settings_.CurrentLanguage == "en")
					{
						//query_text = "SELECT phase AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, (1 - max_period_value) as max_period_value, max_value_period, (1 - max_value_value) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1";
						query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, (max_period_value / 100) as max_period_value, max_value_period, (max_value_value / 100) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 1 AND phase IN ('AB', 'BC', 'CA', '*') ORDER BY phase";
					}

					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query_text, "day_avg_parameters_t3", ref ds);

					if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
					{
						if (ds.Tables.Count > 0)
						{
							for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
							{
								// currents
								for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
								{
									if (ds.Tables[0].Columns[i].Caption.Contains("phase"))
									{
										if (!(ds.Tables[0].Rows[r][i] is DBNull))
										{
											string phase = (ds.Tables[0].Rows[r][i]).ToString();
											switch (phase)
											{
												case "A": phase = "AB"; break;
												case "B": phase = "BC"; break;
												case "C": phase = "CA"; break;
											}
											ds.Tables[0].Rows[r][i] = phase;
										}
									}
								}
							}
						}
					}

					// binding dataset with datagrid
					dgDips2.SetDataBinding(ds, "day_avg_parameters_t3");
				}
				else
				{
					query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20, dns.num_20_till_60, dns.num_over_60 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND dns.param_id = 3062 AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					DataSet ds = new DataSet();
					dbService.CreateAndFillDataAdapter(query_text, "pqp_dip_swell", ref ds);

					// binding dataset with datagrid
					dgDips2.SetDataBinding(ds, "pqp_dip_swell");
				}

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgDips2.DataSource, dgDips2.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgDips2()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgDipsInitTableStyle(DEVICE_VERSIONS newDipSwellMode)
		{
			try
			{
				dgDips.TableStyles.Clear();

				ResourceManager rm =
						new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					string phase = rm.GetString("name_phase");
					string summ_duration = rm.GetString("name_summ_duration");
					string summ_number = rm.GetString("name_summ_number");
					string max_duration = rm.GetString("name_max_duration_dip");
					string max_deviation = rm.GetString("name_max_deviation_dip");

					DataGridGroupCaption caption0 = new DataGridGroupCaption(phase, 1);
					DataGridGroupCaption caption1 = new DataGridGroupCaption(summ_duration, 1/*, Color.Beige*/);
					DataGridGroupCaption caption2 = new DataGridGroupCaption(summ_number, 1/*, Color.Beige*/);
					DataGridGroupCaption caption3 = new DataGridGroupCaption(max_duration, 2/*, Color.Honeydew*/);
					DataGridGroupCaption caption4 = new DataGridGroupCaption(max_deviation, 2/*, Color.Honeydew*/);

					// phase
					DataGridColumnGroupCaption cs_dips_phase = new DataGridColumnGroupCaption(caption0, 0);
					cs_dips_phase.HeaderText = " ";
					cs_dips_phase.MappingName = "phase";

					// common_duration
					DataGridColumnGroupCaption cs_summ_duration = new DataGridColumnGroupCaption(caption1, 0);
					cs_summ_duration.HeaderText = " ";
					cs_summ_duration.MappingName = "common_duration";
					cs_summ_duration.Format = "HH:mm:ss.fff";
					cs_summ_duration.Width = 120;

					// common_number
					DataGridColumnGroupCaption cs_common_number = new DataGridColumnGroupCaption(caption2, 0);
					cs_common_number.HeaderText = " ";
					cs_common_number.MappingName = "common_number";
					cs_common_number.Width = 120;

					// max_period_period
					DataGridColumnGroupCaption cs_max_period_period =
						new DataGridColumnGroupCaption(caption3, 0, "T_max");
					cs_max_period_period.HeaderText = String.Empty;
					cs_max_period_period.MappingName = "max_period_period";
					cs_max_period_period.Format = "HH:mm:ss.fff";

					// max_period_value
					string ratio_name = rm.GetString("columnheaders_pqp_dip");

					//DataGridColumnGroupCaption cs_max_period_value = new DataGridColumnGroupCaption(caption3, 1, ratio_name);
					DataGridColumnGroupCaption cs_max_period_value = new DataGridColumnGroupCaption(caption3, 1);
					if (ratio_name.Contains("_"))
					{
						cs_max_period_value.HeaderFormula = ratio_name;
					}
					else
					{
						cs_max_period_value.HeaderText = ratio_name;
					}
					cs_max_period_value.MappingName = "max_period_value";
					cs_max_period_value.Width = 70;
					cs_max_period_value.Format = "p";

					// max_value_period
					DataGridColumnGroupCaption cs_max_value_period = new DataGridColumnGroupCaption(caption4, 0);
					cs_max_value_period.HeaderText = "T";
					cs_max_value_period.MappingName = "max_value_period";
					cs_max_value_period.Format = "HH:mm:ss.fff";

					// max_value_value
					//DataGridColumnGroupCaption cs_max_value_value = new DataGridColumnGroupCaption(caption4, 1, ratio_name + " max");
					DataGridColumnGroupCaption cs_max_value_value = new DataGridColumnGroupCaption(caption4, 1);

					if (ratio_name.Contains("_"))
					{
						cs_max_value_value.HeaderFormula = ratio_name + " max";
					}
					else
					{
						cs_max_value_value.HeaderText = ratio_name + " max";
					}
					cs_max_value_value.MappingName = "max_value_value";
					cs_max_value_value.Width = 70;
					cs_max_value_value.Format = "p";

					DataGridTableStyle ts = new DataGridTableStyle();
					ts.MappingName = "day_avg_parameters_t3";

					ts.GridColumnStyles.Add(cs_dips_phase);

					ts.GridColumnStyles.Add(cs_summ_duration);
					ts.GridColumnStyles.Add(cs_common_number);
					ts.GridColumnStyles.Add(cs_max_period_period);

					ts.GridColumnStyles.Add(cs_max_period_value);
					ts.GridColumnStyles.Add(cs_max_value_period);
					ts.GridColumnStyles.Add(cs_max_value_value);

					ts.AllowSorting = false;
					dgDips.TableStyles.Add(ts);
				}
				else       // EtPQP-A
				{
					string deviation_caption = rm.GetString("name_deviation");
					string general_caption = rm.GetString("name_event_duration");

					DataGridGroupCaption caption0 = new DataGridGroupCaption(deviation_caption, 1);
					int cntDipCols = 9;
					if (newDipSwellMode == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073) cntDipCols = 6;
					DataGridGroupCaption caption1 = new DataGridGroupCaption(general_caption, cntDipCols);

					// deviation
					DataGridColumnGroupCaption cs_dips_dev = new DataGridColumnGroupCaption(caption0, 0);
					cs_dips_dev.HeaderText = " ";
					cs_dips_dev.MappingName = "name";

					// num_0_01_till_0_05
					DataGridColumnGroupCaption cs_0_01_till_0_05 = new DataGridColumnGroupCaption(caption1, 0);
					if(newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_01_till_0_05.HeaderText = rm.GetString("name_0_01_till_0_05");
					else cs_0_01_till_0_05.HeaderText = rm.GetString("name_0_01_till_0_2");
					cs_0_01_till_0_05.MappingName = "num_0_01_till_0_05";
					//cs_summ_duration.Format = "HH:mm:ss.fff";
					cs_0_01_till_0_05.Width = 120;

					// num_0_05_till_0_1
					DataGridColumnGroupCaption cs_0_05_till_0_1 = new DataGridColumnGroupCaption(caption1, 1);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_05_till_0_1.HeaderText = rm.GetString("name_0_05_till_0_1");
					else cs_0_05_till_0_1.HeaderText = rm.GetString("name_0_2_till_0_5");
					cs_0_05_till_0_1.MappingName = "num_0_05_till_0_1";
					cs_0_05_till_0_1.Width = 120;

					// num_0_1_till_0_5
					DataGridColumnGroupCaption cs_0_1_till_0_5 = new DataGridColumnGroupCaption(caption1, 2);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_1_till_0_5.HeaderText = rm.GetString("name_0_1_till_0_5");
					else cs_0_1_till_0_5.HeaderText = rm.GetString("name_0_5_till_1");
					cs_0_1_till_0_5.MappingName = "num_0_1_till_0_5";
					cs_0_1_till_0_5.Width = 120;

					// num_0_5_till_1
					DataGridColumnGroupCaption cs_0_5_till_1 = new DataGridColumnGroupCaption(caption1, 3);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_5_till_1.HeaderText = rm.GetString("name_0_5_till_1");
					else cs_0_5_till_1.HeaderText = rm.GetString("name_1_till_5");
					cs_0_5_till_1.MappingName = "num_0_5_till_1";
					cs_0_5_till_1.Width = 120;

					// num_1_till_3
					DataGridColumnGroupCaption cs_1_till_3 = new DataGridColumnGroupCaption(caption1, 4);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_1_till_3.HeaderText = rm.GetString("name_1_till_3");
					else cs_1_till_3.HeaderText = rm.GetString("name_5_till_20");
					cs_1_till_3.MappingName = "num_1_till_3";
					cs_1_till_3.Width = 120;

					// num_3_till_20
					DataGridColumnGroupCaption cs_3_till_20 = new DataGridColumnGroupCaption(caption1, 5);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_3_till_20.HeaderText = rm.GetString("name_3_till_20");
					else cs_3_till_20.HeaderText = rm.GetString("name_20_till_60");
					cs_3_till_20.MappingName = "num_3_till_20";
					cs_3_till_20.Width = 120;

					// num_20_till_60
					DataGridColumnGroupCaption cs_20_till_60 = new DataGridColumnGroupCaption(caption1, 6);
					cs_20_till_60.HeaderText = rm.GetString("name_20_till_60");
					cs_20_till_60.MappingName = "num_20_till_60";
					cs_20_till_60.Width = 120;

					// num_over_60
					DataGridColumnGroupCaption cs_over_60 = new DataGridColumnGroupCaption(caption1, 7);
					cs_over_60.HeaderText = rm.GetString("name_over_60");
					cs_over_60.MappingName = "num_over_60";
					cs_over_60.Width = 120;

					// num_over_180
					DataGridColumnGroupCaption cs_over_180 = new DataGridColumnGroupCaption(caption1, 8);
					cs_over_180.HeaderText = rm.GetString("name_over_180");
					cs_over_180.MappingName = "num_over_180";
					cs_over_180.Width = 120;

					DataGridTableStyle ts = new DataGridTableStyle();
					ts.MappingName = "pqp_dip_swell";

					ts.GridColumnStyles.Add(cs_dips_dev);

					ts.GridColumnStyles.Add(cs_0_01_till_0_05);
					ts.GridColumnStyles.Add(cs_0_05_till_0_1);
					ts.GridColumnStyles.Add(cs_0_1_till_0_5);
					ts.GridColumnStyles.Add(cs_0_5_till_1);
					ts.GridColumnStyles.Add(cs_1_till_3);
					ts.GridColumnStyles.Add(cs_3_till_20);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					{
						ts.GridColumnStyles.Add(cs_20_till_60);
						ts.GridColumnStyles.Add(cs_over_60);
						ts.GridColumnStyles.Add(cs_over_180);
					}

					ts.AllowSorting = false;
					dgDips.TableStyles.Add(ts);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgDipsInitTableStyle()");
				throw;
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgDipsInitTableStyle2()
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					lblDips2.Text = rm.GetString("str_interrupt") + ":";
				}
				else lblDips2.Text = rm.GetString("str_dip") + ":";

				dgDips2.TableStyles.Clear();

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					string phase = rm.GetString("name_phase");
					string summ_duration = rm.GetString("name_summ_duration");
					string summ_number = rm.GetString("name_summ_number");
					string max_duration = rm.GetString("name_max_duration_dip");
					string max_deviation = rm.GetString("name_max_deviation_dip");

					DataGridGroupCaption caption0 = new DataGridGroupCaption(phase, 1);
					DataGridGroupCaption caption1 = new DataGridGroupCaption(summ_duration, 1/*, Color.Beige*/);
					DataGridGroupCaption caption2 = new DataGridGroupCaption(summ_number, 1/*, Color.Beige*/);
					DataGridGroupCaption caption3 = new DataGridGroupCaption(max_duration, 2/*, Color.Honeydew*/);
					DataGridGroupCaption caption4 = new DataGridGroupCaption(max_deviation, 2/*, Color.Honeydew*/);

					// phase
					DataGridColumnGroupCaption cs_dips_phase = new DataGridColumnGroupCaption(caption0, 0);
					cs_dips_phase.HeaderText = " ";
					cs_dips_phase.MappingName = "phase";

					// common_duration
					DataGridColumnGroupCaption cs_summ_duration = new DataGridColumnGroupCaption(caption1, 0);
					cs_summ_duration.HeaderText = " ";
					cs_summ_duration.MappingName = "common_duration";
					cs_summ_duration.Format = "HH:mm:ss.fff";
					cs_summ_duration.Width = 120;

					// common_number
					DataGridColumnGroupCaption cs_common_number = new DataGridColumnGroupCaption(caption2, 0);
					cs_common_number.HeaderText = " ";
					cs_common_number.MappingName = "common_number";
					cs_common_number.Width = 120;

					// max_period_period
					DataGridColumnGroupCaption cs_max_period_period = new DataGridColumnGroupCaption(caption3, 0, "T_max");
					cs_max_period_period.HeaderText = String.Empty;
					cs_max_period_period.MappingName = "max_period_period";
					cs_max_period_period.Format = "HH:mm:ss.fff";

					// max_period_value
					string ratio_name = rm.GetString("columnheaders_pqp_dip");

					//DataGridColumnGroupCaption cs_max_period_value = new DataGridColumnGroupCaption(caption3, 1, ratio_name);
					DataGridColumnGroupCaption cs_max_period_value = new DataGridColumnGroupCaption(caption3, 1);
					if (ratio_name.Contains("_"))
					{
						cs_max_period_value.HeaderFormula = ratio_name;
					}
					else
					{
						cs_max_period_value.HeaderText = ratio_name;
					}
					cs_max_period_value.MappingName = "max_period_value";
					cs_max_period_value.Width = 70;
					cs_max_period_value.Format = "p";

					// max_value_period
					DataGridColumnGroupCaption cs_max_value_period = new DataGridColumnGroupCaption(caption4, 0);
					cs_max_value_period.HeaderText = "T";
					cs_max_value_period.MappingName = "max_value_period";
					cs_max_value_period.Format = "HH:mm:ss.fff";

					// max_value_value
					//DataGridColumnGroupCaption cs_max_value_value = new DataGridColumnGroupCaption(caption4, 1, ratio_name + " max");
					DataGridColumnGroupCaption cs_max_value_value = new DataGridColumnGroupCaption(caption4, 1);

					if (ratio_name.Contains("_"))
					{
						cs_max_value_value.HeaderFormula = ratio_name + " max";
					}
					else
					{
						cs_max_value_value.HeaderText = ratio_name + " max";
					}
					cs_max_value_value.MappingName = "max_value_value";
					cs_max_value_value.Width = 70;
					cs_max_value_value.Format = "p";

					DataGridTableStyle ts = new DataGridTableStyle();
					ts.MappingName = "day_avg_parameters_t3";

					ts.GridColumnStyles.Add(cs_dips_phase);

					ts.GridColumnStyles.Add(cs_summ_duration);
					ts.GridColumnStyles.Add(cs_common_number);
					ts.GridColumnStyles.Add(cs_max_period_period);

					ts.GridColumnStyles.Add(cs_max_period_value);
					ts.GridColumnStyles.Add(cs_max_value_period);
					ts.GridColumnStyles.Add(cs_max_value_value);

					ts.AllowSorting = false;
					dgDips2.TableStyles.Add(ts);
				}
				else       // EtPQP-A
				{
					string deviation_caption = rm.GetString("name_deviation");
					string general_caption = rm.GetString("name_event_duration");

					DataGridGroupCaption caption0 = new DataGridGroupCaption(deviation_caption, 1);
					DataGridGroupCaption caption1 = new DataGridGroupCaption(general_caption, 8);

					// deviation
					DataGridColumnGroupCaption cs_dips_dev = new DataGridColumnGroupCaption(caption0, 0);
					cs_dips_dev.HeaderText = " ";
					cs_dips_dev.MappingName = "name";

					// name_0_till_0_5
					DataGridColumnGroupCaption cs_1 = new DataGridColumnGroupCaption(caption1, 0);
					cs_1.HeaderText = rm.GetString("name_0_till_0_5");
					cs_1.MappingName = "num_0_01_till_0_05";
					//cs_summ_duration.Format = "HH:mm:ss.fff";
					cs_1.Width = 100;

					// name_0_5_till_1
					DataGridColumnGroupCaption cs_2 = new DataGridColumnGroupCaption(caption1, 1);
					cs_2.HeaderText = rm.GetString("name_0_5_till_1");
					cs_2.MappingName = "num_0_05_till_0_1";
					cs_2.Width = 100;

					// name_1_till_5
					DataGridColumnGroupCaption cs_3 = new DataGridColumnGroupCaption(caption1, 2);
					cs_3.HeaderText = rm.GetString("name_1_till_5");
					cs_3.MappingName = "num_0_1_till_0_5";
					cs_3.Width = 100;

					// name_5_till_20
					DataGridColumnGroupCaption cs_4 = new DataGridColumnGroupCaption(caption1, 3);
					cs_4.HeaderText = rm.GetString("name_5_till_20");
					cs_4.MappingName = "num_0_5_till_1";
					cs_4.Width = 100;

					// name_20_till_60
					DataGridColumnGroupCaption cs_5 = new DataGridColumnGroupCaption(caption1, 4);
					cs_5.HeaderText = rm.GetString("name_20_till_60");
					cs_5.MappingName = "num_1_till_3";
					cs_5.Width = 100;

					// name_over_60
					DataGridColumnGroupCaption cs_6 = new DataGridColumnGroupCaption(caption1, 5);
					cs_6.HeaderText = rm.GetString("name_over_60");
					cs_6.MappingName = "num_3_till_20";
					cs_6.Width = 100;

					// name_over_180
					DataGridColumnGroupCaption cs_7 = new DataGridColumnGroupCaption(caption1, 6);
					cs_7.HeaderText = rm.GetString("name_over_180");
					cs_7.MappingName = "num_20_till_60";
					cs_7.Width = 100;

					// num_max_len
					DataGridColumnGroupCaption cs_8 = new DataGridColumnGroupCaption(caption1, 7);
					cs_8.HeaderText = rm.GetString("name_max_length");
					cs_8.MappingName = "num_over_60";
					cs_8.Width = 160;

					DataGridTableStyle ts = new DataGridTableStyle();
					ts.MappingName = "pqp_dip_swell";

					ts.GridColumnStyles.Add(cs_dips_dev);

					ts.GridColumnStyles.Add(cs_1);
					ts.GridColumnStyles.Add(cs_2);
					ts.GridColumnStyles.Add(cs_3);
					ts.GridColumnStyles.Add(cs_4);
					ts.GridColumnStyles.Add(cs_5);
					ts.GridColumnStyles.Add(cs_6);
					ts.GridColumnStyles.Add(cs_7);
					ts.GridColumnStyles.Add(cs_8);

					ts.AllowSorting = false;
					dgDips2.TableStyles.Add(ts);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgDipsInitTableStyle2()");
				throw;
			}
		}
		
		#endregion

		#region Swells Grid

		/// <summary>
		/// Filling Over Voltages Grid with data
		/// </summary>
		public void drawdgOvers(string devVersion)
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}
			try
			{
				string query_text = "";
				if (curDeviceType_ == EmDeviceType.EM32)
				{
					string phaseText;
					Int64 res = dbService.ExecuteScalarInt64("SELECT COUNT(*) FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0 AND phase IN ('*4')");
					if (res > 0) phaseText = "*4";
					else phaseText = "*";

					//string query_text = "SELECT substring(phase from 1 for 1) AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0";
					query_text = "SELECT substring(phase from 1 for 1) AS phase, common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0 AND phase IN ('A', 'B', 'C', '" + phaseText + "') ORDER BY phase";
				}
				else if (curDeviceType_ == EmDeviceType.EM33T ||
							curDeviceType_ == EmDeviceType.EM31K ||
							curDeviceType_ == EmDeviceType.EM33T1)
				{
					query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0 AND phase IN ('A', 'B', 'C', '*') ORDER BY phase";
				}
				else if (curDeviceType_ == EmDeviceType.ETPQP)
				{
					string phaseText;
					Int64 res = dbService.ExecuteScalarInt64("SELECT COUNT(*) FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0 AND phase IN ('*4')");
					if (res > 0) phaseText = "*4";
					else phaseText = "*";

					//string query_text = "SELECT substring(phase from 1 for 1) AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0";
					query_text = "SELECT substring(phase from 1 for 1) AS phase, common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0 AND phase IN ('A', 'B', 'C', '" + phaseText + "') ORDER BY phase";
				}
				else if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					if (Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_ADDED_DIP_OVER180)
					{
						query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20, dns.num_20_till_60, dns.num_over_60, dns.num_over_180 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND (dns.param_id BETWEEN 3043 AND 3046) AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else if (Constants.isNewDeviceVersion_ETPQP_A(devVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					{
						query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND (dns.param_id BETWEEN 3053 AND 3056) AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
					else  // the oldest version
					{
						query_text = "SELECT p.name, dns.num_0_01_till_0_05, dns.num_0_05_till_0_1, dns.num_0_1_till_0_5, dns.num_0_5_till_1, dns.num_1_till_3, dns.num_3_till_20, dns.num_20_till_60, dns.num_over_60,  '-' as num_over_180 FROM parameters p, pqp_dip_swell dns WHERE p.param_id = dns.param_id AND (dns.param_id BETWEEN 3043 AND 3046) AND dns.datetime_id = " + curDatetimeId_ + " ORDER BY p.param_id;";
					}
				}

				DataSet ds = new DataSet();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dbService.CreateAndFillDataAdapter(query_text, "day_avg_parameters_t3", ref ds);
				else dbService.CreateAndFillDataAdapter(query_text, "pqp_dip_swell", ref ds);

				if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
				{
					if (ds.Tables.Count > 0)
					{
						for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
						{
							// currents
							for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
							{
								if (ds.Tables[0].Columns[i].Caption.Contains("phase"))
								{
									if (!(ds.Tables[0].Rows[r][i] is DBNull))
									{
										string phase = (ds.Tables[0].Rows[r][i]).ToString();
										switch (phase)
										{
											case "A": phase = "AB"; break;
											case "B": phase = "BC"; break;
											case "C": phase = "CA"; break;
										}
										ds.Tables[0].Rows[r][i] = phase;
									}
								}
							}
						}
					}
				}

				// binding dataset with datagrid
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					dgOvers.SetDataBinding(ds, "day_avg_parameters_t3");
				else dgOvers.SetDataBinding(ds, "pqp_dip_swell");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgOvers.DataSource, dgOvers.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgOvers()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Filling Over Voltages Grid with data
		/// </summary>
		public void drawdgOvers2()
		{
			if (curDeviceType_ == EmDeviceType.EM33T ||
				curDeviceType_ == EmDeviceType.EM31K ||
				curDeviceType_ == EmDeviceType.EM33T1)
				return;
			if (curDeviceType_ == EmDeviceType.ETPQP_A)
				return;

			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}
			try
			{
				//string query_text = "SELECT phase AS phase, date_trunc('milliseconds', common_duration) as common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0";
				string query_text = "SELECT phase AS phase, common_duration, common_number, max_period_period, (max_period_value + 1) as max_period_value, max_value_period, (max_value_value + 1) as max_value_value FROM day_avg_parameters_t3 WHERE datetime_id = " + curDatetimeId_ + " AND event_type = 0 AND phase IN ('AB', 'BC', 'CA', '*') ORDER BY phase";

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query_text, "day_avg_parameters_t3", ref ds);

				if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
				{
					if (ds.Tables.Count > 0)
					{
						for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
						{
							// currents
							for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
							{
								if (ds.Tables[0].Columns[i].Caption.Contains("phase"))
								{
									if (!(ds.Tables[0].Rows[r][i] is DBNull))
									{
										string phase = (ds.Tables[0].Rows[r][i]).ToString();
										switch (phase)
										{
											case "A": phase = "AB"; break;
											case "B": phase = "BC"; break;
											case "C": phase = "CA"; break;
										}
										ds.Tables[0].Rows[r][i] = phase;
									}
								}
							}
						}
					}
				}

				// binding dataset with datagrid
				dgOvers2.SetDataBinding(ds, "day_avg_parameters_t3");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgOvers2.DataSource, dgOvers2.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgOvers2()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgOversInitTableStyle(DEVICE_VERSIONS newDipSwellMode)
		{
			try
			{
				dgOvers.TableStyles.Clear();

				ResourceManager rm =
						new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				DataGridTableStyle ts = new DataGridTableStyle();

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					string phase = rm.GetString("name_phase");
					string summ_duration = rm.GetString("name_summ_duration");
					string summ_number = rm.GetString("name_summ_number");
					string max_duration = rm.GetString("name.max_duration.swell");
					string max_deviation = rm.GetString("name.max_deviation.swell");

					DataGridGroupCaption caption0 = new DataGridGroupCaption(phase, 1);
					DataGridGroupCaption caption1 = new DataGridGroupCaption(summ_duration, 1/*, Color.Beige*/);
					DataGridGroupCaption caption2 = new DataGridGroupCaption(summ_number, 1/*, Color.Beige*/);
					DataGridGroupCaption caption3 = new DataGridGroupCaption(max_duration, 2/*, Color.Honeydew*/);
					DataGridGroupCaption caption4 = new DataGridGroupCaption(max_deviation, 2/*, Color.Honeydew*/);

					// phase
					DataGridColumnGroupCaption cs_dips_phase = new DataGridColumnGroupCaption(caption0, 0);
					cs_dips_phase.HeaderText = " ";
					cs_dips_phase.MappingName = "phase";

					// common_duration
					DataGridColumnGroupCaption cs_summ_duration = new DataGridColumnGroupCaption(caption1, 0);
					cs_summ_duration.HeaderText = " ";
					cs_summ_duration.MappingName = "common_duration";
					cs_summ_duration.Format = "HH:mm:ss.fff";
					cs_summ_duration.Width = 120;

					// common_number
					DataGridColumnGroupCaption cs_common_number = new DataGridColumnGroupCaption(caption2, 0);
					cs_common_number.HeaderText = " ";
					cs_common_number.MappingName = "common_number";
					cs_common_number.Width = 120;

					// max_period_period
					DataGridColumnGroupCaption cs_max_period_period =
						new DataGridColumnGroupCaption(caption3, 0, "T_max");
					cs_max_period_period.HeaderText = String.Empty;
					cs_max_period_period.MappingName = "max_period_period";
					cs_max_period_period.Format = "HH:mm:ss.fff";

					// max_period_value
					string ratio_name = rm.GetString("columnheaders_pqp_swell");

					DataGridColumnGroupCaption cs_max_period_value = new DataGridColumnGroupCaption(caption3, 1);
					if (ratio_name.Contains("_"))
					{
						cs_max_period_value.HeaderFormula = ratio_name;
					}
					else
					{
						cs_max_period_value.HeaderText = ratio_name;
					}

					cs_max_period_value.MappingName = "max_period_value";
					cs_max_period_value.Width = 70;
					if (settings_.CurrentLanguage == "ru")
					{
						cs_max_period_value.Format = "";
					}
					else if (settings_.CurrentLanguage == "en")
					{
						cs_max_period_value.Format = "p";
					}

					// max_value_period
					DataGridColumnGroupCaption cs_max_value_period = new DataGridColumnGroupCaption(caption4, 0);
					cs_max_value_period.HeaderText = "T";
					cs_max_value_period.MappingName = "max_value_period";
					cs_max_value_period.Format = "HH:mm:ss.fff";

					DataGridColumnGroupCaption cs_max_value_value = new DataGridColumnGroupCaption(caption4, 1);
					// max_value_value
					if (ratio_name.Contains("_"))
					{
						cs_max_value_value.HeaderFormula = ratio_name + " max";
					}
					else
					{
						cs_max_value_value.HeaderText = ratio_name + " max";
					}

					cs_max_value_value.MappingName = "max_value_value";
					cs_max_value_value.Width = 70;
					if (settings_.CurrentLanguage == "ru")
					{
						cs_max_value_value.Format = "";
					}
					else if (settings_.CurrentLanguage == "en")
					{
						cs_max_value_value.Format = "p";
					}

					ts.MappingName = "day_avg_parameters_t3";

					ts.GridColumnStyles.Add(cs_dips_phase);

					ts.GridColumnStyles.Add(cs_summ_duration);
					ts.GridColumnStyles.Add(cs_common_number);
					ts.GridColumnStyles.Add(cs_max_period_period);

					ts.GridColumnStyles.Add(cs_max_period_value);
					ts.GridColumnStyles.Add(cs_max_value_period);
					ts.GridColumnStyles.Add(cs_max_value_value);
				}
				else
				{
					string deviation_caption = rm.GetString("name_deviation");
					string general_caption = rm.GetString("name_event_duration");

					DataGridGroupCaption caption0 = new DataGridGroupCaption(deviation_caption, 1);
					int cntDipCols = 9;
					if (newDipSwellMode == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073) cntDipCols = 6;
					DataGridGroupCaption caption1 = new DataGridGroupCaption(general_caption, cntDipCols);

					// deviation
					DataGridColumnGroupCaption cs_dips_dev = new DataGridColumnGroupCaption(caption0, 0);
					cs_dips_dev.HeaderText = " ";
					cs_dips_dev.MappingName = "name";

					// num_0_01_till_0_05
					DataGridColumnGroupCaption cs_0_01_till_0_05 = new DataGridColumnGroupCaption(caption1, 0);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_01_till_0_05.HeaderText = rm.GetString("name_0_01_till_0_05");
					else cs_0_01_till_0_05.HeaderText = rm.GetString("name_0_01_till_0_2");
					cs_0_01_till_0_05.MappingName = "num_0_01_till_0_05";
					//cs_summ_duration.Format = "HH:mm:ss.fff";
					cs_0_01_till_0_05.Width = 120;

					// num_0_05_till_0_1
					DataGridColumnGroupCaption cs_0_05_till_0_1 = new DataGridColumnGroupCaption(caption1, 1);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_05_till_0_1.HeaderText = rm.GetString("name_0_05_till_0_1");
					else cs_0_05_till_0_1.HeaderText = rm.GetString("name_0_2_till_0_5");
					cs_0_05_till_0_1.MappingName = "num_0_05_till_0_1";
					cs_0_05_till_0_1.Width = 120;

					// num_0_1_till_0_5
					DataGridColumnGroupCaption cs_0_1_till_0_5 = new DataGridColumnGroupCaption(caption1, 2);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_1_till_0_5.HeaderText = rm.GetString("name_0_1_till_0_5");
					else cs_0_1_till_0_5.HeaderText = rm.GetString("name_0_5_till_1");
					cs_0_1_till_0_5.MappingName = "num_0_1_till_0_5";
					cs_0_1_till_0_5.Width = 120;

					// num_0_5_till_1
					DataGridColumnGroupCaption cs_0_5_till_1 = new DataGridColumnGroupCaption(caption1, 3);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_0_5_till_1.HeaderText = rm.GetString("name_0_5_till_1");
					else cs_0_5_till_1.HeaderText = rm.GetString("name_1_till_5");
					cs_0_5_till_1.MappingName = "num_0_5_till_1";
					cs_0_5_till_1.Width = 120;

					// num_1_till_3
					DataGridColumnGroupCaption cs_1_till_3 = new DataGridColumnGroupCaption(caption1, 4);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_1_till_3.HeaderText = rm.GetString("name_1_till_3");
					else cs_1_till_3.HeaderText = rm.GetString("name_5_till_20");
					cs_1_till_3.MappingName = "num_1_till_3";
					cs_1_till_3.Width = 120;

					// num_3_till_20
					DataGridColumnGroupCaption cs_3_till_20 = new DataGridColumnGroupCaption(caption1, 5);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
						cs_3_till_20.HeaderText = rm.GetString("name_3_till_20");
					else cs_3_till_20.HeaderText = rm.GetString("name_20_till_60");
					cs_3_till_20.MappingName = "num_3_till_20";
					cs_3_till_20.Width = 120;

					// num_20_till_60
					DataGridColumnGroupCaption cs_20_till_60 = new DataGridColumnGroupCaption(caption1, 6);
					cs_20_till_60.HeaderText = rm.GetString("name_20_till_60");
					cs_20_till_60.MappingName = "num_20_till_60";
					cs_20_till_60.Width = 120;

					// num_over_60
					DataGridColumnGroupCaption cs_over_60 = new DataGridColumnGroupCaption(caption1, 7);
					cs_over_60.HeaderText = rm.GetString("name_over_60");
					cs_over_60.MappingName = "num_over_60";
					cs_over_60.Width = 120;

					// num_over_180
					DataGridColumnGroupCaption cs_over_180 = new DataGridColumnGroupCaption(caption1, 8);
					cs_over_180.HeaderText = rm.GetString("name_over_180");
					cs_over_180.MappingName = "num_over_180";
					cs_over_180.Width = 120;

					ts.MappingName = "pqp_dip_swell";

					ts.GridColumnStyles.Add(cs_dips_dev);

					ts.GridColumnStyles.Add(cs_0_01_till_0_05);
					ts.GridColumnStyles.Add(cs_0_05_till_0_1);
					ts.GridColumnStyles.Add(cs_0_1_till_0_5);
					ts.GridColumnStyles.Add(cs_0_5_till_1);
					ts.GridColumnStyles.Add(cs_1_till_3);
					ts.GridColumnStyles.Add(cs_3_till_20);
					if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					{
						ts.GridColumnStyles.Add(cs_20_till_60);
						ts.GridColumnStyles.Add(cs_over_60);
						ts.GridColumnStyles.Add(cs_over_180);
					}
				}

				ts.AllowSorting = false;
				dgOvers.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgOversInitTableStyle()");
				throw;
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgOversInitTableStyle2()
		{
			try
			{
				if (curDeviceType_ == EmDeviceType.ETPQP_A) return;

				dgOvers2.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string phase = rm.GetString("name_phase");
				string summ_duration = rm.GetString("name_summ_duration");
				string summ_number = rm.GetString("name_summ_number");
				string max_duration = rm.GetString("name.max_duration.swell");
				string max_deviation = rm.GetString("name.max_deviation.swell");

				DataGridGroupCaption caption0 = new DataGridGroupCaption(phase, 1);
				DataGridGroupCaption caption1 = new DataGridGroupCaption(summ_duration, 1/*, Color.Beige*/);
				DataGridGroupCaption caption2 = new DataGridGroupCaption(summ_number, 1/*, Color.Beige*/);
				DataGridGroupCaption caption3 = new DataGridGroupCaption(max_duration, 2/*, Color.Honeydew*/);
				DataGridGroupCaption caption4 = new DataGridGroupCaption(max_deviation, 2/*, Color.Honeydew*/);

				// phase
				DataGridColumnGroupCaption cs_dips_phase = new DataGridColumnGroupCaption(caption0, 0);
				cs_dips_phase.HeaderText = " ";
				cs_dips_phase.MappingName = "phase";

				// common_duration
				DataGridColumnGroupCaption cs_summ_duration = new DataGridColumnGroupCaption(caption1, 0);
				cs_summ_duration.HeaderText = " ";
				cs_summ_duration.MappingName = "common_duration";
				cs_summ_duration.Format = "HH:mm:ss.fff";
				cs_summ_duration.Width = 120;

				// common_number
				DataGridColumnGroupCaption cs_common_number = new DataGridColumnGroupCaption(caption2, 0);
				cs_common_number.HeaderText = " ";
				cs_common_number.MappingName = "common_number";
				cs_common_number.Width = 120;

				// max_period_period
				DataGridColumnGroupCaption cs_max_period_period = new DataGridColumnGroupCaption(caption3, 0, "T_max");
				cs_max_period_period.HeaderText = String.Empty;
				cs_max_period_period.MappingName = "max_period_period";
				cs_max_period_period.Format = "HH:mm:ss.fff";

				// max_period_value
				string ratio_name = rm.GetString("columnheaders_pqp_swell");

				DataGridColumnGroupCaption cs_max_period_value = new DataGridColumnGroupCaption(caption3, 1);
				if (ratio_name.Contains("_"))
				{
					cs_max_period_value.HeaderFormula = ratio_name;
				}
				else
				{
					cs_max_period_value.HeaderText = ratio_name;
				}

				cs_max_period_value.MappingName = "max_period_value";
				cs_max_period_value.Width = 70;
				if (settings_.CurrentLanguage == "ru")
				{
					cs_max_period_value.Format = "";
				}
				else if (settings_.CurrentLanguage == "en")
				{
					cs_max_period_value.Format = "p";
				}

				// max_value_period
				DataGridColumnGroupCaption cs_max_value_period = new DataGridColumnGroupCaption(caption4, 0);
				cs_max_value_period.HeaderText = "T";
				cs_max_value_period.MappingName = "max_value_period";
				cs_max_value_period.Format = "HH:mm:ss.fff";

				DataGridColumnGroupCaption cs_max_value_value = new DataGridColumnGroupCaption(caption4, 1);
				// max_value_value
				if (ratio_name.Contains("_"))
				{
					cs_max_value_value.HeaderFormula = ratio_name + " max";
				}
				else
				{
					cs_max_value_value.HeaderText = ratio_name + " max";
				}

				cs_max_value_value.MappingName = "max_value_value";
				cs_max_value_value.Width = 70;
				if (settings_.CurrentLanguage == "ru")
				{
					cs_max_value_value.Format = "";
				}
				else if (settings_.CurrentLanguage == "en")
				{
					cs_max_value_value.Format = "p";
				}

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "day_avg_parameters_t3";

				ts.GridColumnStyles.Add(cs_dips_phase);

				ts.GridColumnStyles.Add(cs_summ_duration);
				ts.GridColumnStyles.Add(cs_common_number);
				ts.GridColumnStyles.Add(cs_max_period_period);

				ts.GridColumnStyles.Add(cs_max_period_value);
				ts.GridColumnStyles.Add(cs_max_value_period);
				ts.GridColumnStyles.Add(cs_max_value_value);

				ts.AllowSorting = false;
				dgOvers2.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgOversInitTableStyle2()");
				throw;
			}
		}

		#endregion

		#region Fliker Grids

		private void dgFlickerNumInitTableStyle()
		{
			try
			{
				if (curDeviceType_ != EmDeviceType.ETPQP_A) return;

				dgFlickerNum.TableStyles.Clear();

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				DataGridGroupCaption caption0 = new DataGridGroupCaption(rm.GetString("name_columns_parameter"), 1);
				DataGridGroupCaption caption14 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number"), 2/*, Color.Beige*/);
				DataGridGroupCaption caption1 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseA") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption2 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseA") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption3 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseA") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption4 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseA") + "), %", 2/*, Color.Honeydew*/);

				DataGridGroupCaption caption5 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseB") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption6 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseB") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption7 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseB") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption8 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseB") + "), %", 2/*, Color.Honeydew*/);

				DataGridGroupCaption caption9 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseC") + ")", 3/*, Color.Beige*/);
				DataGridGroupCaption caption10 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseC") + "), %", 3/*, Color.Beige*/);
				DataGridGroupCaption caption11 = new DataGridGroupCaption(rm.GetString("name_columns_measures_number") + " (" + rm.GetString("name_phases_phaseC") + "), " + rm.GetString("name_columns_time"), 3/*, Color.Honeydew*/);
				DataGridGroupCaption caption12 = new DataGridGroupCaption(rm.GetString("name_column_measures_result") + " (" + rm.GetString("name_phases_phaseC") + "), %", 2/*, Color.Honeydew*/);
				DataGridGroupCaption caption13 = new DataGridGroupCaption(rm.GetString("name_columns_standard_value") + ", %", 2/*, Color.Honeydew*/);

				// Common
				// p.name
				DataGridColumnCellFormula cs_vn_name = new DataGridColumnCellFormula(caption0, 0);
				cs_vn_name.HeaderText = "";
				cs_vn_name.MappingName = "name";
				cs_vn_name.BackgroungColor = DataGridColors.ColorPqpParam;

				// marked
				DataGridColumnGroupCaption cs_vn_marked = new DataGridColumnGroupCaption(caption14, 0);
				cs_vn_marked.HeaderText = rm.GetString("name_columns_marked");
				cs_vn_marked.MappingName = "num_marked";
				cs_vn_marked.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_marked.Format = DataColumnsFormat.FloatShortFormat;
				// not_marked
				DataGridColumnGroupCaption cs_vn_not_marked = new DataGridColumnGroupCaption(caption14, 1);
				cs_vn_not_marked.HeaderText = rm.GetString("name_columns_not_marked");
				cs_vn_not_marked.MappingName = "num_not_marked";
				cs_vn_not_marked.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_not_marked.Format = DataColumnsFormat.FloatShortFormat;

				// Phase A
				// dapt4.num_nrm_rng
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph1 =
					new DataGridColumnGroupCaption(caption1, 0);
				cs_vn_num_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph1.MappingName = "num_nrm_rng_ph1";
				cs_vn_num_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph1 =
					new DataGridColumnGroupCaption(caption1, 1);
				cs_vn_num_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph1.MappingName = "num_max_rng_ph1";
				cs_vn_num_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph1 =
					new DataGridColumnGroupCaption(caption1, 2);
				cs_vn_num_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph1.MappingName = "num_out_max_rng_ph1";
				cs_vn_num_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph1.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption2, 0);
				cs_vn_prcnt_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph1.MappingName = "prcnt_nrm_rng_ph1";
				cs_vn_prcnt_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph1 = new DataGridColumnGroupCaption(caption2, 1);
				cs_vn_prcnt_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph1.MappingName = "prcnt_max_rng_ph1";
				cs_vn_prcnt_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph1 = 
					new DataGridColumnGroupCaption(caption2, 2);
				cs_vn_prcnt_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph1.MappingName = "prcnt_out_max_rng_ph1";
				cs_vn_prcnt_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph1
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph1 = new DataGridColumnTimespan(caption3, 0);
				cs_vn_time_nrm_rng_ph1.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph1.MappingName = "time_nrm_rng_ph1";
				cs_vn_time_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph1
				DataGridColumnTimespan cs_vn_time_max_rng_ph1 = new DataGridColumnTimespan(caption3, 1);
				cs_vn_time_max_rng_ph1.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph1.MappingName = "time_max_rng_ph1";
				cs_vn_time_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph1.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph1
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph1 = new DataGridColumnTimespan(caption3, 2);
				cs_vn_time_out_max_rng_ph1.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph1.MappingName = "time_out_max_rng_ph1";
				cs_vn_time_out_max_rng_ph1.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph1.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph1
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph1 = new DataGridColumnGroupCaption(caption4, 0);
				cs_vn_calc_nrm_rng_ph1.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph1.MappingName = "calc_nrm_rng_ph1";
				cs_vn_calc_nrm_rng_ph1.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph1
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph1 =
					new DataGridColumnGroupCaption(caption4, 1);
				cs_vn_calc_max_rng_ph1.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph1.MappingName = "calc_max_rng_ph1";
				cs_vn_calc_max_rng_ph1.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph1.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Phases B
				// dapt4.num_nrm_rng
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption5, 0);
				cs_vn_num_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph2.MappingName = "num_nrm_rng_ph2";
				cs_vn_num_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph2 = new DataGridColumnGroupCaption(caption5, 1);
				cs_vn_num_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph2.MappingName = "num_max_rng_ph2";
				cs_vn_num_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph2 = new DataGridColumnGroupCaption(caption5, 2);
				cs_vn_num_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph2.MappingName = "num_out_max_rng_ph2";
				cs_vn_num_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph2.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption6, 0);
				cs_vn_prcnt_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph2.MappingName = "prcnt_nrm_rng_ph2";
				cs_vn_prcnt_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph2 = new DataGridColumnGroupCaption(caption6, 1);
				cs_vn_prcnt_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph2.MappingName = "prcnt_max_rng_ph2";
				cs_vn_prcnt_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph2 = new DataGridColumnGroupCaption(caption6, 2);
				cs_vn_prcnt_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph2.MappingName = "prcnt_out_max_rng_ph2";
				cs_vn_prcnt_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph2
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph2 = new DataGridColumnTimespan(caption7, 0);
				cs_vn_time_nrm_rng_ph2.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph2.MappingName = "time_nrm_rng_ph2";
				cs_vn_time_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph2
				DataGridColumnTimespan cs_vn_time_max_rng_ph2 = new DataGridColumnTimespan(caption7, 1);
				cs_vn_time_max_rng_ph2.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph2.MappingName = "time_max_rng_ph2";
				cs_vn_time_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph2.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph2
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph2 = new DataGridColumnTimespan(caption7, 2);
				cs_vn_time_out_max_rng_ph2.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph2.MappingName = "time_out_max_rng_ph2";
				cs_vn_time_out_max_rng_ph2.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph2.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph2
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph2 = new DataGridColumnGroupCaption(caption8, 0);
				cs_vn_calc_nrm_rng_ph2.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph2.MappingName = "calc_nrm_rng_ph2";
				cs_vn_calc_nrm_rng_ph2.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph2
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph2 =
					new DataGridColumnGroupCaption(caption8, /*-=pqp 95=- 1*/ 1);
				cs_vn_calc_max_rng_ph2.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph2.MappingName = "calc_max_rng_ph2";
				cs_vn_calc_max_rng_ph2.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph2.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Phase C
				// dapt4.num_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption9, 0);
				cs_vn_num_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_num_nrm_rng_ph3.MappingName = "num_nrm_rng_ph3";
				cs_vn_num_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_nrm_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.num_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_max_rng_ph3 = new DataGridColumnGroupCaption(caption9, 1);
				cs_vn_num_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_num_max_rng_ph3.MappingName = "num_max_rng_ph3";
				cs_vn_num_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_max_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.num_out_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_num_out_max_rng_ph3 = new DataGridColumnGroupCaption(caption9, 2);
				cs_vn_num_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_num_out_max_rng_ph3.MappingName = "num_out_max_rng_ph3";
				cs_vn_num_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_num_out_max_rng_ph3.Format = DataColumnsFormat.FloatShortFormat;
				cs_vn_num_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.prcnt_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption10, 0);
				cs_vn_prcnt_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_prcnt_nrm_rng_ph3.MappingName = "prcnt_nrm_rng_ph3";
				cs_vn_prcnt_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_nrm_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.prcnt_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_max_rng_ph3 = new DataGridColumnGroupCaption(caption10, 1);
				cs_vn_prcnt_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_prcnt_max_rng_ph3.MappingName = "prcnt_max_rng_ph3";
				cs_vn_prcnt_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.prcnt_out_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_prcnt_out_max_rng_ph3 = new DataGridColumnGroupCaption(caption10, 2);
				cs_vn_prcnt_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_prcnt_out_max_rng_ph3.MappingName = "prcnt_out_max_rng_ph3";
				cs_vn_prcnt_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_prcnt_out_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				cs_vn_prcnt_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.time_nrm_rng_ph3
				DataGridColumnTimespan cs_vn_time_nrm_rng_ph3 = new DataGridColumnTimespan(caption11, 0);
				cs_vn_time_nrm_rng_ph3.HeaderText = rm.GetString("name_columns_in_npl");
				cs_vn_time_nrm_rng_ph3.MappingName = "time_nrm_rng_ph3";
				cs_vn_time_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;

				// dapt4.time_max_rng_ph3
				DataGridColumnTimespan cs_vn_time_max_rng_ph3 = new DataGridColumnTimespan(caption11, 1);
				cs_vn_time_max_rng_ph3.HeaderText = rm.GetString("name.columns.between_npl_and_upl");
				cs_vn_time_max_rng_ph3.MappingName = "time_max_rng_ph3";
				cs_vn_time_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_max_rng_ph3.CellPaint += new DataGridEventHandler(CellInside_CellPaint);

				// dapt4.time_out_max_rng_ph3
				DataGridColumnTimespan cs_vn_time_out_max_rng_ph3 = new DataGridColumnTimespan(caption11, 2);
				cs_vn_time_out_max_rng_ph3.HeaderText = rm.GetString("name_columns_out_of_upl");
				cs_vn_time_out_max_rng_ph3.MappingName = "time_out_max_rng_ph3";
				cs_vn_time_out_max_rng_ph3.BackgroungColor = DataGridColors.ColorCommon;
				cs_vn_time_out_max_rng_ph3.CellPaint += new DataGridEventHandler(CellOutMax_CellPaint);

				// dapt4.calc_nrm_rng_ph3
				DataGridColumnGroupCaption cs_vn_calc_nrm_rng_ph3 = new DataGridColumnGroupCaption(caption12, 0);
				cs_vn_calc_nrm_rng_ph3.HeaderText = rm.GetString("name.columns.result.calc.npl.plus");
				cs_vn_calc_nrm_rng_ph3.MappingName = "calc_nrm_rng_ph3";
				cs_vn_calc_nrm_rng_ph3.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_nrm_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dapt4.calc_max_rng_ph3
				DataGridColumnGroupCaption cs_vn_calc_max_rng_ph3 =
					new DataGridColumnGroupCaption(caption12, /*-=pqp 95=- 1*/1);
				cs_vn_calc_max_rng_ph3.HeaderText = rm.GetString("name.columns.result.calc.upl.plus");
				cs_vn_calc_max_rng_ph3.MappingName = "calc_max_rng_ph3";
				cs_vn_calc_max_rng_ph3.BackgroungColor = DataGridColors.ColorPkeResult;
				cs_vn_calc_max_rng_ph3.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				//Common
				// dapt4.real_nrm_rng
				DataGridColumnGroupCaption cs_vn_real_nrm_rng = new DataGridColumnGroupCaption(caption13, 0);
				cs_vn_real_nrm_rng.HeaderText = rm.GetString("name.params.npl.short");
				cs_vn_real_nrm_rng.MappingName = "real_nrm_rng";
				cs_vn_real_nrm_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vn_real_nrm_rng.Format = DataColumnsFormat.FloatShortFormat;

				// dapt4.real_max_rng
				DataGridColumnGroupCaption cs_vn_real_max_rng = new DataGridColumnGroupCaption(caption13, 1);
				cs_vn_real_max_rng.HeaderText = rm.GetString("name.params.upl.short");
				cs_vn_real_max_rng.MappingName = "real_max_rng";
				cs_vn_real_max_rng.BackgroungColor = DataGridColors.ColorPkeStandard;
				cs_vn_real_max_rng.Format = DataColumnsFormat.FloatShortFormat;

				DataGridTableStyle ts = new DataGridTableStyle();
				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				//    ts.MappingName = "day_avg_parameters_t4";
				ts.MappingName = "pqp_flicker";

				ts.GridColumnStyles.Add(cs_vn_name);								// 0

				if (curDeviceType_ == EmDeviceType.ETPQP_A)
				{
					ts.GridColumnStyles.Add(cs_vn_marked);
					ts.GridColumnStyles.Add(cs_vn_not_marked);
				}

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph1);						// 1
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph1);						// 2
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph1);					// 3

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph1);					// 4
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph1);					// 5
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph1);				// 6

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph1);					// 7
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph1);					// 8
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph1);				// 9

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph1);		// uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph1);					// 10/11

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph2);						// 11/12
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph2);						// 12/13
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph2);					// 13/14

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph2);					// 14/15
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph2);					// 15/16
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph2);				// 16/17

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph2);					// 17/18
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph2);					// 18/19
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph2);				// 19/20

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph2);		//  uncomment for Ku            
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph2);					// 20/22

				ts.GridColumnStyles.Add(cs_vn_num_nrm_rng_ph3);						// 21/23
				ts.GridColumnStyles.Add(cs_vn_num_max_rng_ph3);						// 22/24
				ts.GridColumnStyles.Add(cs_vn_num_out_max_rng_ph3);					// 23/25

				ts.GridColumnStyles.Add(cs_vn_prcnt_nrm_rng_ph3);	 				// 24/26				
				ts.GridColumnStyles.Add(cs_vn_prcnt_max_rng_ph3);	 				// 25/27
				ts.GridColumnStyles.Add(cs_vn_prcnt_out_max_rng_ph3);				// 26/28

				ts.GridColumnStyles.Add(cs_vn_time_nrm_rng_ph3);					// 27/29
				ts.GridColumnStyles.Add(cs_vn_time_max_rng_ph3);					// 28/30
				ts.GridColumnStyles.Add(cs_vn_time_out_max_rng_ph3);				// 29/31

				ts.GridColumnStyles.Add(cs_vn_calc_nrm_rng_ph3);		// uncomment for Ku
				ts.GridColumnStyles.Add(cs_vn_calc_max_rng_ph3);					// 30/33

				ts.GridColumnStyles.Add(cs_vn_real_nrm_rng);						// 31/34
				ts.GridColumnStyles.Add(cs_vn_real_max_rng);						// 32/35

				dgFlickerNum.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgFlickerNumInitTableStyle()");
				throw;
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgFlickerShortInitTableStyle()
		{
			int newSplitterDistance = 0;
			try
			{
				dgFlicker.TableStyles.Clear();

				if (scFliker.Width > 0)
				{
					if ((scFliker.Width / 2) > 0)
						newSplitterDistance = scFliker.Width / 2;
					if (newSplitterDistance < scFliker.Panel1MinSize) newSplitterDistance = scFliker.Panel1MinSize;
					if (newSplitterDistance > (scFliker.Width - scFliker.Panel2MinSize))
						newSplitterDistance = scFliker.Width - scFliker.Panel2MinSize;
					if (newSplitterDistance > 0)
						scFliker.SplitterDistance = newSplitterDistance;
					else return;
				}

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t5";
				else ts.MappingName = "pqp_flicker_val";

				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("time");
				cs_event.MappingName = "flik_time";
				cs_event.Width = 120;
				ts.GridColumnStyles.Add(cs_event);

				DataGridColumnHeaderFormula cs_flik_A =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA);
				cs_flik_A.HeaderText = rm.GetString("name_phase") + " A (P st)";
				cs_flik_A.MappingName = "flik_a";
				cs_flik_A.Width = 120;
				ts.GridColumnStyles.Add(cs_flik_A);

				DataGridColumnHeaderFormula cs_flik_B =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB);
				cs_flik_B.HeaderText = rm.GetString("name_phase") + " B (P st)";
				cs_flik_B.MappingName = "flik_b";
				cs_flik_B.Width = 120;
				ts.GridColumnStyles.Add(cs_flik_B);

				DataGridColumnHeaderFormula cs_flik_C =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC);
				cs_flik_C.HeaderText = rm.GetString("name_phase") + " C (P st)";
				cs_flik_C.MappingName = "flik_c";
				cs_flik_C.Width = 120;
				ts.GridColumnStyles.Add(cs_flik_C);

				ts.AllowSorting = false;
				dgFlicker.TableStyles.Add(ts);

				#region Old code
				/*ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
					this.GetType().Assembly);
				string time = rm.GetString("time");
				string phase_A = rm.GetString("name_phase") + " A";
				string phase_B = rm.GetString("name_phase") + " B";
				string phase_C = rm.GetString("name_phase") + " C";

				DataGridGroupCaption caption0 = new DataGridGroupCaption(time, 1);
				DataGridGroupCaption caption1 = new DataGridGroupCaption(phase_A, 2);
				DataGridGroupCaption caption2 = new DataGridGroupCaption(phase_B, 2);
				DataGridGroupCaption caption3 = new DataGridGroupCaption(phase_C, 2);

				// time
				DataGridColumnGroupCaption cs_time = new DataGridColumnGroupCaption(caption0, 0);
				cs_time.HeaderText = " ";
				cs_time.MappingName = "flik_time";
				cs_time.Width = 120;
	
				// fliker A
				DataGridColumnHeaderFormula cs_flik_A = 
					new DataGridColumnGroupCaption(caption1, 0, "P_st_");
				cs_flik_A.MappingName = "flik_a";
				cs_flik_A.Width = 120;
				cs_flik_A.BackgroungColor = DataGridColors.ColorAvgPhaseA;

				// fliker B
				DataGridColumnGroupCaption cs_flik_B = 
					new DataGridColumnGroupCaption(caption2, 0, "P_st_");
				cs_flik_B.MappingName = "flik_b";
				cs_flik_B.Width = 120;
				cs_flik_B.BackgroungColor = DataGridColors.ColorAvgPhaseB;

				// fliker C
				DataGridColumnGroupCaption cs_flik_C =
					new DataGridColumnGroupCaption(caption3, 0, "P_st_");
				cs_flik_C.MappingName = "flik_c";
				cs_flik_C.Width = 120;
				cs_flik_C.BackgroungColor = DataGridColors.ColorAvgPhaseC;

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "day_avg_parameters_t5";

				ts.GridColumnStyles.Add(cs_time);
				ts.GridColumnStyles.Add(cs_flik_A);
				ts.GridColumnStyles.Add(cs_flik_B);
				ts.GridColumnStyles.Add(cs_flik_C);

				ts.AllowSorting = false;
				dgFlicker.TableStyles.Add(ts);*/
				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgFlickerShortInitTableStyle()  " + 
					newSplitterDistance.ToString());
				throw;
			}
		}

		/// <summary>
		/// Initialization of table column's style
		/// </summary>
		private void dgFlickerLongInitTableStyle()
		{
			try
			{
				dgFlickerLong.TableStyles.Clear();

				//dgFlickerLong.PreferredRowHeight = 192;

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);
				DataGridTableStyle ts = new DataGridTableStyle();
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					ts.MappingName = "day_avg_parameters_t5";
				else ts.MappingName = "pqp_flicker_val";
				//string time = rm.GetString("time");
				//string phase_A = rm.GetString("name_phase") + " A";
				//string phase_B = rm.GetString("name_phase") + " B";
				//string phase_C = rm.GetString("name_phase") + " C";

				//DataGridGroupCaption caption0 = new DataGridGroupCaption(time, 1);
				//DataGridGroupCaption caption1 = new DataGridGroupCaption(phase_A, 2);
				//DataGridGroupCaption caption2 = new DataGridGroupCaption(phase_B, 2);
				//DataGridGroupCaption caption3 = new DataGridGroupCaption(phase_C, 2);

				////////////////////////////
				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("time");
				cs_event.MappingName = "flik_time";
				cs_event.Width = 120;
				ts.GridColumnStyles.Add(cs_event);

				DataGridColumnHeaderFormula cs_flik_A_long =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA);
				cs_flik_A_long.HeaderText = rm.GetString("name_phase") + " A (P lt)";
				cs_flik_A_long.MappingName = "flik_a_long";
				cs_flik_A_long.Width = 120;
				ts.GridColumnStyles.Add(cs_flik_A_long);

				DataGridColumnHeaderFormula cs_flik_B_long =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB);
				cs_flik_B_long.HeaderText = rm.GetString("name_phase") + " B (P lt)";
				cs_flik_B_long.MappingName = "flik_b_long";
				cs_flik_B_long.Width = 120;
				ts.GridColumnStyles.Add(cs_flik_B_long);

				DataGridColumnHeaderFormula cs_flik_C_long =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC);
				cs_flik_C_long.HeaderText = rm.GetString("name_phase") + " C (P lt)";
				cs_flik_C_long.MappingName = "flik_c_long";
				cs_flik_C_long.Width = 120;
				ts.GridColumnStyles.Add(cs_flik_C_long);
				/////////////////////////////////

				// time
				//DataGridColumnGroupCaption cs_time = new DataGridColumnGroupCaption(caption0, 0);
				//cs_time.HeaderText = " ";
				//cs_time.MappingName = "flik_time";
				//cs_time.Width = 120;

				// fliker A
				//DataGridColumnGroupCaption cs_flik_A_long =
				//    new DataGridColumnGroupCaption(caption1, 1, "P_lt_");
				//cs_flik_A_long.MappingName = "flik_a_long";
				//cs_flik_A_long.Width = 120;
				//cs_flik_A_long.BackgroungColor = DataGridColors.ColorAvgPhaseA;
				//cs_flik_A_long.CellPaint += new DataGridEventHandler(CellFliker_Lt_CellPaint);

				//// fliker B
				//DataGridColumnGroupCaption cs_flik_B_long =
				//    new DataGridColumnGroupCaption(caption2, 1, "P_lt_");
				//cs_flik_B_long.MappingName = "flik_b_long";
				//cs_flik_B_long.Width = 120;
				//cs_flik_B_long.BackgroungColor = DataGridColors.ColorAvgPhaseB;
				//cs_flik_B_long.CellPaint += new DataGridEventHandler(CellFliker_Lt_CellPaint);

				//// fliker C
				//DataGridColumnGroupCaption cs_flik_C_long =
				//    new DataGridColumnGroupCaption(caption3, 1, "P_lt_");
				//cs_flik_C_long.MappingName = "flik_c_long";
				//cs_flik_C_long.Width = 120;
				//cs_flik_C_long.BackgroungColor = DataGridColors.ColorAvgPhaseC;
				//cs_flik_C_long.CellPaint += new DataGridEventHandler(CellFliker_Lt_CellPaint);

				//ts.GridColumnStyles.Add(cs_time);
				//ts.GridColumnStyles.Add(cs_flik_A_long);
				//ts.GridColumnStyles.Add(cs_flik_B_long);
				//ts.GridColumnStyles.Add(cs_flik_C_long);

				ts.AllowSorting = false;
				//ts.PreferredRowHeight = 192;
				dgFlickerLong.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgFlickerLongInitTableStyle()");
				throw;
			}
		}

		private void drawdgFlickerNum()
		{
			if (curDeviceType_ != EmDeviceType.ETPQP_A) return;

			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
                string query = String.Empty;

				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					query = string.Format(@"SELECT p.name, f.num_marked, f.num_not_marked, f.num_nrm_rng_ph1, f.num_max_rng_ph1, f.num_out_max_rng_ph1, 
case when f.valid_flick != 0 
then cast(round(cast(f.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1, 
case when f.num_not_marked > 0 
then cast(round(cast(f.calc_max_rng_ph1 as numeric), {0}) as text) else '-' end as calc_max_rng_ph1,
f.num_nrm_rng_ph2, f.num_max_rng_ph2, f.num_out_max_rng_ph2, 
case when f.valid_flick != 0 
then cast(round(cast(f.calc_nrm_rng_ph2 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph2,
case when f.num_not_marked > 0 
then cast(round(cast(f.calc_max_rng_ph2 as numeric), {0}) as text) else '-' end as calc_max_rng_ph2, 
f.num_nrm_rng_ph3, f.num_max_rng_ph3, f.num_out_max_rng_ph3, 
case when f.valid_flick != 0 
then cast(round(cast(f.calc_nrm_rng_ph3 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph3,
case when f.num_not_marked > 0 
then cast(round(cast(f.calc_max_rng_ph3 as numeric), {0}) as text) else '-' end as calc_max_rng_ph3, 
f.real_nrm_rng, f.real_max_rng 
FROM parameters p, pqp_flicker f 
WHERE p.param_id = f.param_id AND f.datetime_id = {1} ORDER BY p.param_id;", settings_.FloatSigns,
                                                                           curDatetimeId_);
				}
				else
				{
					query = string.Format(@"SELECT p.name, f.num_nrm_rng_ph1, f.num_max_rng_ph1, f.num_out_max_rng_ph1, 
case when f.valid_flick != 0 
then cast(round(cast(f.calc_nrm_rng_ph1 as numeric), {0}) as text) else '-' end as calc_nrm_rng_ph1,
case when f.num_not_marked > 0 
then cast(round(cast(f.calc_max_rng_ph1 as numeric), {0}) as text) else '-' end as calc_max_rng_ph1,
f.real_nrm_rng, f.real_max_rng FROM parameters p, pqp_flicker f WHERE p.param_id = f.param_id AND f.datetime_id = {1} ORDER BY p.param_id;", settings_.FloatSigns, curDatetimeId_);
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "pqp_flicker", ref ds);

				if (ds.Tables.Count == 0) return;

				Int64 coef = 600 * TimeSpan.TicksPerSecond;

				// TODO: тут добавляются необходимые В ЛЮБОМ СЛУЧАЕ столбцы (для первой фазы)
				// adding calc-fields to the dataset
				DataColumn c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_nrm_rng_ph1 * 100 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";
				c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_max_rng_ph1 * 100 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";
				c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph1", System.Type.GetType("System.Single"));
				c.Expression = "IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_out_max_rng_ph1 * 100/ (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1))";

				// original ver. 1.0.4 fixed
				Int64 timeDuration = ((TimeSpan)(curEndDateTime_ - curStartDateTime_)).Ticks;
				c = ds.Tables[0].Columns.Add("time_nrm_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_nrm_rng_ph1 / (1 /" +
					coef.ToString() +
					"))";
				//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
				//timeDuration.ToString() + 
				//" * (num_nrm_rng_ph1 /  (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";
				c = ds.Tables[0].Columns.Add("time_max_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_max_rng_ph1 / (1 /" +
					coef.ToString() +
					"))";

				//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
				//timeDuration.ToString() + 
				//" * (num_max_rng_ph1 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";
				c = ds.Tables[0].Columns.Add("time_out_max_rng_ph1", System.Type.GetType("System.Int64"));
				c.Expression =
					"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
					"num_out_max_rng_ph1 / (1 /" +
					coef.ToString() +
					"))";

				//"IIF((num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)=0,0," +
				//timeDuration.ToString() + 
				//" * (num_out_max_rng_ph1 / (num_nrm_rng_ph1 + num_max_rng_ph1 + num_out_max_rng_ph1)))";

				if (CurConnectScheme != ConnectScheme.Ph1W2)
				{
					// TODO: тут устанавливаем расчетные столбцы для 2ой и 3ей фазы
					// adding calc-fields to the dataset				
					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_nrm_rng_ph2 * 100 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_max_rng_ph2 * 100 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph2", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_out_max_rng_ph2 * 100/ (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2))";

					c = ds.Tables[0].Columns.Add("time_nrm_rng_ph2", System.Type.GetType("System.Int64"));
					c.Expression =
						"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_nrm_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";

					//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_nrm_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";
					c = ds.Tables[0].Columns.Add("time_max_rng_ph2", System.Type.GetType("System.Int64"));
					c.Expression =
						"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_max_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";

					//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_max_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";
					c = ds.Tables[0].Columns.Add("time_out_max_rng_ph2", System.Type.GetType("System.Int64"));
					c.Expression =
						"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
						"num_out_max_rng_ph2 / (1 /" +
						coef.ToString() +
						"))";

					//"IIF((num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_out_max_rng_ph2 / (num_nrm_rng_ph2 + num_max_rng_ph2 + num_out_max_rng_ph2)))";

					c = ds.Tables[0].Columns.Add("prcnt_nrm_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_nrm_rng_ph3 * 100 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";
					c = ds.Tables[0].Columns.Add("prcnt_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_max_rng_ph3 * 100 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";
					c = ds.Tables[0].Columns.Add("prcnt_out_max_rng_ph3", System.Type.GetType("System.Single"));
					c.Expression = "IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_out_max_rng_ph3 * 100/ (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3))";

					c = ds.Tables[0].Columns.Add("time_nrm_rng_ph3", System.Type.GetType("System.Int64"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_nrm_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";

					//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_nrm_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
					c = ds.Tables[0].Columns.Add("time_max_rng_ph3", System.Type.GetType("System.Int64"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_max_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";

					//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_max_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
					c = ds.Tables[0].Columns.Add("time_out_max_rng_ph3", System.Type.GetType("System.Int64"));
					c.Expression =
						"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
						"num_out_max_rng_ph3 / (1 /" +
						coef.ToString() +
						"))";

					//"IIF((num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)=0,0," +
					//timeDuration.ToString() + 
					//" * (num_out_max_rng_ph3 / (num_nrm_rng_ph3 + num_max_rng_ph3 + num_out_max_rng_ph3)))";
				}

				for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
				{
					DataRow curRow = ds.Tables[0].Rows[iRow];
					string str_val = curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")].ToString();
					float f_val;
					if (Conversions.object_2_float_en_ru(
						curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")], out f_val))
						curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")] = f_val.ToString();
					else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph1")] = "-";

					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph2")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_nrm_rng_ph3")] = "-";
					}

					if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph1")], out f_val))
						curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph1")] = f_val.ToString();
					else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph1")] = "-";

					if (CurConnectScheme != ConnectScheme.Ph1W2)
					{
						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph2")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph2")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph2")] = "-";

						if (Conversions.object_2_float_en_ru(
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph3")], out f_val))
							curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph3")] = f_val.ToString();
						else curRow[ds.Tables[0].Columns.IndexOf("calc_max_rng_ph3")] = "-";
					}
				}

				// binding dataset with datagrid
				dgFlickerNum.SetDataBinding(ds, "pqp_flicker");

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgFlickerNum.DataSource, dgFlickerNum.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgFlickerNum()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Filling the first Fliker Grid with data
		/// </summary>
		public void drawdgFlicker()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, 
					dbService.Database);
				return;
			}
			try
			{
				string tableName = "day_avg_parameters_t5";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) tableName = "pqp_flicker_val";

				string query_text = String.Empty;

				if (CurConnectScheme == ConnectScheme.Ph1W2)
				{
					if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.EM32)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time, 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, case when flik_a = -1 then ' ' else cast(flik_a as text) end FROM {0} WHERE datetime_id = {1} AND flik_a <> -1 order by flik_sign, record_id", tableName, curDatetimeId_);
					}
					else if (curDeviceType_ == EmDeviceType.ETPQP_A)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time, 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, case when flik_a = -1 then ' ' else cast(flik_a as text) end FROM {0} WHERE datetime_id = {1} AND flik_a <> -1 order by record_id", tableName, curDatetimeId_);
					}
					else
					{
						query_text = string.Format(
							//"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, flik_a, flik_a_long FROM day_avg_parameters_t5 WHERE datetime_id = " + curDatetimeId_;
							"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, case when flik_a = -1 then ' ' else cast(flik_a as text) end FROM {0} WHERE datetime_id = {1} order by record_id", tableName, curDatetimeId_);
					}
				}
				else
				{
					if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.EM32)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time, 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, case when flik_a = -1 then ' ' else cast(flik_a as text) end, case when flik_b = -1 then ' ' else cast(flik_b as text) end, case when flik_c = -1 then ' ' else cast(flik_c as text) end FROM {0} WHERE datetime_id = {1} AND flik_a <> -1 order by flik_sign, record_id", tableName, curDatetimeId_);
					}
					else if (curDeviceType_ == EmDeviceType.ETPQP_A)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time, 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, case when flik_a = -1 then ' ' else cast(flik_a as text) end, case when flik_b = -1 then ' ' else cast(flik_b as text) end, case when flik_c = -1 then ' ' else cast(flik_c as text) end FROM {0} WHERE datetime_id = {1} AND flik_a <> -1 order by record_id", tableName, curDatetimeId_);
					}
					else
					{
						query_text = string.Format(
							//"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long FROM day_avg_parameters_t5 WHERE datetime_id = " + curDatetimeId_;
							"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, case when flik_a = -1 then ' ' else cast(flik_a as text) end, case when flik_b = -1 then ' ' else cast(flik_b as text) end, case when flik_c = -1 then ' ' else cast(flik_c as text) end FROM {0} WHERE datetime_id = {1} order by record_id", tableName, curDatetimeId_);
					}
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query_text, tableName, ref ds);

				// binding dataset with datagrid
				dgFlicker.SetDataBinding(ds, tableName);

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgFlicker.DataSource, dgFlicker.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;

				//извлекаем из базы название прибора, это понадобится для графика фликера
				if (curDeviceType_ == EmDeviceType.EM33T || 
					curDeviceType_ == EmDeviceType.EM31K ||
					curDeviceType_ == EmDeviceType.EM33T1)
				{
					string commandText = "SELECT database_id FROM day_avg_parameter_times WHERE datetime_id = " + curDatetimeId_;
					string database_id = dbService.ExecuteScalarString(commandText);
					commandText = "SELECT device_name FROM databases WHERE db_id = " + database_id;
					curDeviceName_ = dbService.ExecuteScalarString(commandText);
				}
				else if (curDeviceType_ == EmDeviceType.EM32)
					curDeviceName_ = "EM32";
				else if (curDeviceType_ == EmDeviceType.ETPQP)
					curDeviceName_ = "ETPQP";
				else if (curDeviceType_ == EmDeviceType.ETPQP_A)
					curDeviceName_ = "ETPQP-A";
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgFlicker()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		public void drawdgFlickerLong()
		{
			DbService dbService = new DbService(GetPgConnectionString());
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port,
					dbService.Database);
				return;
			}
			try
			{
				string tableName = "day_avg_parameters_t5";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) tableName = "pqp_flicker_val";

				string query_text = String.Empty;

				if (CurConnectScheme == ConnectScheme.Ph1W2)
				{
					if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.EM32)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time - interval '110 minutes', 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, cast(flik_a_long as text) FROM {0} WHERE datetime_id = {1} AND flik_a_long <> -1 order by flik_sign, record_id", tableName, curDatetimeId_);
					}
					else if (curDeviceType_ == EmDeviceType.ETPQP_A)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time - interval '110 minutes', 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, cast(flik_a_long as text) FROM {0} WHERE datetime_id = {1} AND flik_a_long <> -1 order by record_id", tableName, curDatetimeId_);
					}
					else
					{
						query_text = string.Format(
							//"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, flik_a, flik_a_long FROM day_avg_parameters_t5 WHERE datetime_id = " + curDatetimeId_;
							"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, cast(flik_a_long as text) FROM {0} WHERE datetime_id = {1} order by record_id", tableName, curDatetimeId_);
					}
				}
				else
				{
					if (curDeviceType_ == EmDeviceType.ETPQP || curDeviceType_ == EmDeviceType.EM32)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time - interval '110 minutes', 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, case when flik_a_long = -1 then ' ' else cast(flik_a_long as text) end as flik_a_long, case when flik_b_long = -1 then ' ' else cast(flik_b_long as text) end as flik_b_long, case when flik_c_long = -1 then ' ' else cast(flik_c_long as text) end as flik_c_long FROM {0} WHERE datetime_id = {1} AND flik_a_long <> -1 order by flik_sign, record_id", tableName, curDatetimeId_);
					}
					else if (curDeviceType_ == EmDeviceType.ETPQP_A)
					{
						query_text = string.Format(
							"SELECT to_char(flik_time - interval '110 minutes', 'HH24:MI:SS') || ' - ' || to_char(flik_time + interval '10 minutes', 'HH24:MI:SS') as flik_time, case when flik_a_long = -1 then ' ' else cast(flik_a_long as text) end as flik_a_long, case when flik_b_long = -1 then ' ' else cast(flik_b_long as text) end as flik_b_long, case when flik_c_long = -1 then ' ' else cast(flik_c_long as text) end as flik_c_long FROM {0} WHERE datetime_id = {1} AND flik_a_long <> -1 order by record_id", tableName, curDatetimeId_);
					}
					else
					{
						query_text = string.Format(
							//"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long FROM day_avg_parameters_t5 WHERE datetime_id = " + curDatetimeId_;
							"SELECT to_char(flik_time, 'HH24:MI:SS') as flik_time, case when flik_a_long = -1 then ' ' else cast(flik_a_long as text) end as flik_a_long, case when flik_b_long = -1 then ' ' else cast(flik_b_long as text) end as flik_b_long, case when flik_c_long = -1 then ' ' else cast(flik_c_long as text) end as flik_c_long FROM {0} WHERE datetime_id = {1} order by record_id", tableName, curDatetimeId_);
					}
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query_text, tableName, ref ds);

				// binding dataset with datagrid
				dgFlickerLong.SetDataBinding(ds, tableName);

				//disallow add, edit and delete operations
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dgFlickerLong.DataSource, dgFlickerLong.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;

				//извлекаем из базы название прибора, это понадобится для графика фликера
				if (curDeviceType_ == EmDeviceType.EM33T ||
					curDeviceType_ == EmDeviceType.EM31K ||
					curDeviceType_ == EmDeviceType.EM33T1)
				{
					string commandText = "SELECT database_id FROM day_avg_parameter_times WHERE datetime_id = " + curDatetimeId_;
					string database_id = dbService.ExecuteScalarString(commandText);
					commandText = "SELECT device_name FROM databases WHERE db_id = " + database_id;
					curDeviceName_ = dbService.ExecuteScalarString(commandText);
				}
				else if (curDeviceType_ == EmDeviceType.EM32)
					curDeviceName_ = "EM32";
				else if (curDeviceType_ == EmDeviceType.ETPQP)
					curDeviceName_ = "ETPQP";
				else if (curDeviceType_ == EmDeviceType.ETPQP_A)
					curDeviceName_ = "ETPQP-A";
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in drawdgFlickerLong()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		void CellFliker_Lt_CellPaint(object sender, DataGridEventArgs e)
		{
			try
			{
				DataGrid grid = (sender as DataGridColumnGroupCaption).DataGridTableStyle.DataGrid;

				TimeSpan timeBegin, curTime;
				TimeSpan diff = new TimeSpan(2, 0, 0);

				if (!TimeSpan.TryParse(grid[0, 0].ToString(), out timeBegin))
					return;
				if (e.Row < 1)
				{
					e.ForeBrush = grayBrush_;
					return;
				}

				curTime = timeBegin;
				for (int curRow = 1; curRow < e.Row; ++curRow)
				{
					curTime = curTime.Add(TimeSpan.FromMinutes(t_fliker_));
				}

				if (grid.VisibleRowCount > 0)
				{
					if ((curTime - timeBegin) < diff)
						e.ForeBrush = grayBrush_;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in CellFliker_Lt_CellPaint()");
				throw;
			}
		}

		#endregion

		#region Header labels

		/// <summary>
		/// Setting up common information header
		/// </summary>
		public void SetCommonCaption(DateTime startPeakLoad1, DateTime endPeakLoad1,
									DateTime startPeakLoad2, DateTime endPeakLoad2)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string strConstraintType = rm.GetString("name.constraint_type.header_prefix");

				string constrTypeName = string.Empty;
				if (curDeviceType_ != EmDeviceType.ETPQP_A)
					constrTypeName = rm.GetString("name.constraint_type." +
					(curConstrType_ + 1).ToString() + ".full");
				else constrTypeName = rm.GetString("name_constraint_type_pqpa_" +
					(curConstrType_ + 1).ToString() + "_full");

				strConstraintType += constrTypeName;

				string plStr = GetPeakLoadString(startPeakLoad1, endPeakLoad1, startPeakLoad2, endPeakLoad2);

				// fliker
				string str_t_fliker = "";
				if (t_fliker_ > 0)
				{
					str_t_fliker = String.Format(rm.GetString("name.fliker_period"), t_fliker_.ToString());
				}

				if (curDeviceType_ != EmDeviceType.ETPQP_A)
				{
					lbl_EPI_Caption.Text = String.Format(rm.GetString("window_prefix_common_pqp"),
						curStartDateTime_, curEndDateTime_,
						plStr, strConstraintType, str_t_fliker);
				}
				else
				{
					if (startPeakLoad1 == DateTime.MinValue && endPeakLoad1 == DateTime.MinValue &&
						startPeakLoad2 == DateTime.MinValue && endPeakLoad2 == DateTime.MinValue)
					{
						lbl_EPI_Caption.Text = String.Format(
							rm.GetString("window_prefix_common_pqp_etpqp_a"), 
							curStartDateTime_, curEndDateTime_, strConstraintType);
					}
					else
					{
						lbl_EPI_Caption.Text = String.Format(
						rm.GetString("window_prefix_common_pqp_etpqp_a_ext"),
						curStartDateTime_, curEndDateTime_,
						plStr,
						strConstraintType);
					}
				}

				sdtToPL1_ = startPeakLoad1;
				edtToPL1_ = endPeakLoad1;
				sdtToPL2_ = startPeakLoad2;
				edtToPL2_ = endPeakLoad2;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in SetCommonCaption()");
				throw;
			}
		}

		#endregion

		#region Other stuff

		/// <summary>Show context menu</summary>
		private void dataGridContextMenu_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				DataGrid.HitTestInfo info = (sender as DataGrid).HitTest(e.X, e.Y);
				if (info.Type == DataGrid.HitTestType.Caption)
				{
					cmsDoc.Show((sender as DataGrid), new Point(e.X, e.Y));
				}
			}
		}

		/// <summary>Measure number view type change</summary>
		private void miMeasureNumberAnyVariant_Click(object sender, System.EventArgs e)
		{
			try
			{
				bool dgNonsinus1Visible = true;
				bool dgNonsinus2Visible = true;
				if (curDeviceType_ == EmDeviceType.ETPQP_A &&
					(CurConnectScheme != ConnectScheme.Ph3W3 && CurConnectScheme != ConnectScheme.Ph3W3_B_calc))
					dgNonsinus2Visible = false;
				if (curDeviceType_ == EmDeviceType.ETPQP_A &&
					(CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc))
					dgNonsinus1Visible = false;
				if (CurConnectScheme == ConnectScheme.Ph1W2) dgNonsinus2Visible = false;

				percentToolStripMenuItem.Checked = false;
				percentGlToolStripMenuItem.Checked = false;
				timeToolStripMenuItem.Checked = false;
				numberToolStripMenuItem.Checked = false;

				(sender as ToolStripMenuItem).Checked = true;

				if (numberToolStripMenuItem.Checked)
				{
					#region dgNonSymmetry

					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width =
						dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_max_rng"].Width =
						dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width =
						dgNonSymmetry.PreferredColumnWidth;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0;

					#endregion

					#region dgFrequencyDeparture

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth; //1
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_max_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth; // 2
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth; // 

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0; //4
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0; //5
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0; //6

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0; //7
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0; //8
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0; //9

					#endregion

					#region dgU_Deviation

					dgU_Deviation.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_max_rng"].Width = dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = dgU_Deviation.PreferredColumnWidth;

					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_global"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_max_rng_global"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_global"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0;

					#endregion

					#region dgUNonsinusoidality

					if (dgNonsinus1Visible)
					{
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["num_nrm_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["num_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["num_out_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["prcnt_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["time_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph3"].Width = 0;
					}

					#endregion

					#region dgUNonsinusoidality2

					//if (curDeviceType_ != EmDeviceType.ETPQP_A)
					//{
					if(dgNonsinus2Visible)
					{
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width = 0;
					}

					#endregion

					#region Flicker

                    if (curDeviceType_ == EmDeviceType.ETPQP_A)
                    {
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_nrm_rng_ph1"].Width =
                            dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_max_rng_ph1"].Width =
                            dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_out_max_rng_ph1"].Width =
                            dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["time_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph3"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph3"].Width = 0;
                    }

					#endregion
				}

				if (percentToolStripMenuItem.Checked)
				{
					#region dgNonSymmetry

					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 
						dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 
						dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 
						dgNonSymmetry.PreferredColumnWidth;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0;
					
					#endregion

					#region dgFrequencyDeparture

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0; //1
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0; //2
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0; //3

					dgFrequencyDeparture.TableStyles[0].
						GridColumnStyles["prcnt_nrm_rng"].Width =
						dgFrequencyDeparture.PreferredColumnWidth;
					dgFrequencyDeparture.TableStyles[0].
						GridColumnStyles["prcnt_max_rng"].Width =
						dgFrequencyDeparture.PreferredColumnWidth;
					dgFrequencyDeparture.TableStyles[0].
						GridColumnStyles["prcnt_out_max_rng"].Width =
						dgFrequencyDeparture.PreferredColumnWidth;

					dgFrequencyDeparture.TableStyles[0].
						GridColumnStyles["time_nrm_rng"].Width = 0;
					dgFrequencyDeparture.TableStyles[0].
						GridColumnStyles["time_max_rng"].Width = 0;
					dgFrequencyDeparture.TableStyles[0].
						GridColumnStyles["time_out_max_rng"].Width = 0;

					#endregion

					#region dgU_Deviation
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0;

					dgU_Deviation.TableStyles[0].
						GridColumnStyles["prcnt_nrm_rng"].Width =
						dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].
						GridColumnStyles["prcnt_max_rng"].Width =
						dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].
						GridColumnStyles["prcnt_out_max_rng"].Width =
						dgU_Deviation.PreferredColumnWidth;

					dgU_Deviation.TableStyles[0].
						GridColumnStyles["prcnt_nrm_rng_global"].Width = 0;
					dgU_Deviation.TableStyles[0].
						GridColumnStyles["prcnt_max_rng_global"].Width = 0;
					dgU_Deviation.TableStyles[0].
						GridColumnStyles["prcnt_out_max_rng_global"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0;
					#endregion

					#region dgUNonsinusoidality

					if (dgNonsinus1Visible)
					{
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width = 0;
					}

					#endregion

					#region dgUNonsinusoidality2

					//if (curDeviceType_ != EmDeviceType.ETPQP_A)
					//{
					if(dgNonsinus2Visible)
					{
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width = 0;
					}

					#endregion

					#region Flicker

                    if (curDeviceType_ == EmDeviceType.ETPQP_A)
                    {
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph3"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph3"].Width = 0;
                    }

					#endregion
				}

				if (percentGlToolStripMenuItem.Checked)
				{
					#region dgNonSymmetry
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = dgNonSymmetry.PreferredColumnWidth;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0;
					#endregion

					#region dgFrequencyDeparture
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0; //1
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0; //2
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0; //3

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth;
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth;
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth;

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0; //7
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0; //8
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0; //9
					#endregion

					#region dgU_Deviation
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_global"].Width = dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_max_rng_global"].Width = dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_global"].Width = dgU_Deviation.PreferredColumnWidth;

					dgU_Deviation.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = 0;
					#endregion

					#region dgUNonsinusoidality

					if (dgNonsinus1Visible)
					{
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width = 0;
					}

					#endregion

					#region dgUNonsinusoidality2

					//if (curDeviceType_ != EmDeviceType.ETPQP_A)
					//{
					if(dgNonsinus2Visible)
					{
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width = 0;
					}

					#endregion

					#region Flicker

                    if (curDeviceType_ == EmDeviceType.ETPQP_A)
                    {
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["num_out_max_rng_ph3"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_nrm_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["prcnt_out_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph3"].Width = 0;
                    }

					#endregion
				}

				if (timeToolStripMenuItem.Checked)
				{
					#region dgNonSymmetry
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0;

					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_max_rng"].Width = dgNonSymmetry.PreferredColumnWidth;
					dgNonSymmetry.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = dgNonSymmetry.PreferredColumnWidth;
					#endregion

					#region dgFrequencyDeparture
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0; // 1
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0; // 2
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0; // 3

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0; // 4
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0; // 5
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0; // 6

					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth; // 7 
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_max_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth; // 8
					dgFrequencyDeparture.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = dgFrequencyDeparture.PreferredColumnWidth; // 9

					#endregion

					#region dgU_Deviation
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["num_out_max_rng"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_nrm_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_max_rng"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_out_max_rng"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_global"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_max_rng_global"].Width = 0;
					dgU_Deviation.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_global"].Width = 0;

					dgU_Deviation.TableStyles[0].GridColumnStyles["time_nrm_rng"].Width = dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_max_rng"].Width = dgU_Deviation.PreferredColumnWidth;
					dgU_Deviation.TableStyles[0].GridColumnStyles["time_out_max_rng"].Width = dgU_Deviation.PreferredColumnWidth;
					#endregion

					#region dgUNonsinusoidality

					if (dgNonsinus1Visible)
					{
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = dgUNonsinusoidality.PreferredColumnWidth;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = dgUNonsinusoidality.PreferredColumnWidth;
						dgUNonsinusoidality.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width =
							dgUNonsinusoidality.PreferredColumnWidth;
					}

					#endregion

					#region dgUNonsinusoidality2

					//if (curDeviceType_ != EmDeviceType.ETPQP_A)
					//{
					if (dgNonsinus2Visible)
					{
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_nrm_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_max_rng_ph1"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["prcnt_out_max_rng_ph1"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_nrm_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles["time_out_max_rng_ph1"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 12*/"num_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 13*/"num_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 14*/"num_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 15*/"prcnt_nrm_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 16*/"prcnt_max_rng_ph2"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 17*/"prcnt_out_max_rng_ph2"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 18*/"time_nrm_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 19*/"time_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 20*/"time_out_max_rng_ph2"].Width = dgUNonsinusoidality2.PreferredColumnWidth;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 23*/"num_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 24*/"num_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 25*/"num_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 26*/"prcnt_nrm_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 27*/"prcnt_max_rng_ph3"].Width = 0;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 28*/"prcnt_out_max_rng_ph3"].Width = 0;

						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 29*/"time_nrm_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 30*/"time_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
						dgUNonsinusoidality2.TableStyles[0].
							GridColumnStyles[/* -=pqp 95=- 31*/"time_out_max_rng_ph3"].Width = dgUNonsinusoidality2.PreferredColumnWidth;
					}

					#endregion

					#region Flicker

                    if (curDeviceType_ == EmDeviceType.ETPQP_A)
                    {
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_max_rng_ph1"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_ph1"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph1"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_max_rng_ph2"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_ph2"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph2"].Width = dgFlickerNum.PreferredColumnWidth;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["num_out_max_rng_ph3"].Width = 0;

                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_nrm_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_max_rng_ph3"].Width = 0;
                        dgFlickerNum.TableStyles[0].GridColumnStyles["prcnt_out_max_rng_ph3"].Width = 0;

                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_nrm_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_max_rng_ph3"].Width = dgFlickerNum.PreferredColumnWidth;
                        dgFlickerNum.TableStyles[0].
                            GridColumnStyles["time_out_max_rng_ph3"].Width =
                            dgFlickerNum.PreferredColumnWidth;
                    }

					#endregion
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in miMeasureNumberAnyVariant_Click()");
				throw;
			}
		}

        void CellOutMax_CellPaint(object sender, DataGridEventArgs e)
        {
			try
			{
				int index = 0;
				GridColumnStylesCollection tmpStyle;

				DataGrid grid = (sender as DataGridColumnGroupCaption).DataGridTableStyle.DataGrid;
				//if (grid.Name.Equals("dgFrequencyDeparture")) index = 6;
				if (grid.Name.Equals("dgFrequencyDeparture"))
				{
					tmpStyle = dgFrequencyDeparture.TableStyles[0].GridColumnStyles;
					index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng"]);
				}
				else if (grid.Name.Equals("dgU_Deviation"))
				{
					tmpStyle = dgU_Deviation.TableStyles[0].GridColumnStyles;
					index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng"]); 
				}
				else if (grid.Name.Equals("dgFlickerNum"))
				{
					tmpStyle = dgFlickerNum.TableStyles[0].GridColumnStyles;
					
					if (e.Column < tmpStyle.IndexOf(tmpStyle["num_nrm_rng_ph2"]))
						index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng_ph1"]); 
					else if (e.Column < tmpStyle.IndexOf(tmpStyle["num_nrm_rng_ph3"]))
						index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng_ph2"]); 
					else index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng_ph3"]); 
				}
				else if (grid.Name.Equals("dgNonSymmetry"))
				{
					tmpStyle = dgNonSymmetry.TableStyles[0].GridColumnStyles;
					index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng"]);  // index = 6
				}
				else if (grid.Name.Equals("dgUNonsinusoidality") ||
					grid.Name.Equals("dgUNonsinusoidality2"))
				{
					if (curDeviceType_ != EmDeviceType.ETPQP_A)
						tmpStyle = dgUNonsinusoidality.TableStyles[0].GridColumnStyles;
					else
					{
						if (CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
							tmpStyle = dgUNonsinusoidality2.TableStyles[0].GridColumnStyles;
						else tmpStyle = dgUNonsinusoidality.TableStyles[0].GridColumnStyles;
					}

					if (e.Column < tmpStyle.IndexOf(tmpStyle["num_nrm_rng_ph2"])/*11*/)
						index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng_ph1"]);     // index = 6
					else if (e.Column < tmpStyle.IndexOf(tmpStyle["num_nrm_rng_ph3"])/*21*/)
						index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng_ph2"]);     // index = 16
					else index = tmpStyle.IndexOf(tmpStyle["prcnt_out_max_rng_ph3"]);     // index = 26
				}

				if (index < 0) return;

				if (grid[e.Row, index] is float)
					if ((float)grid[e.Row, index] > 0)
						e.ForeBrush = ErrorBrush;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in CellOutMax_CellPaint()");
				throw;
			}
        }

        void CellInside_CellPaint(object sender, DataGridEventArgs e)
        {
			try
			{
				int index = 0;
				GridColumnStylesCollection tmpStyle;

				DataGrid grid = (sender as DataGridColumnGroupCaption).DataGridTableStyle.DataGrid;
				if (grid.Name.Equals("dgFrequencyDeparture")) index = 5;
				else if (grid.Name.Equals("dgU_Deviation")) index = 5;
				else if (grid.Name.Equals("dgNonSymmetry"))
				{
					tmpStyle = dgNonSymmetry.TableStyles[0].GridColumnStyles;
					index = tmpStyle.IndexOf(tmpStyle["prcnt_max_rng"]);  // index = 5
				}
				else if (grid.Name.Equals("dgUNonsinusoidality") ||
						grid.Name.Equals("dgUNonsinusoidality2"))
				{
					if (curDeviceType_ != EmDeviceType.ETPQP_A)
						tmpStyle = dgUNonsinusoidality.TableStyles[0].GridColumnStyles;
					else
					{
						if(CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc)
							tmpStyle = dgUNonsinusoidality2.TableStyles[0].GridColumnStyles;
						else tmpStyle = dgUNonsinusoidality.TableStyles[0].GridColumnStyles;
					}

					if (e.Column < tmpStyle.IndexOf(tmpStyle["num_nrm_rng_ph2"])/*11*/)
						index = tmpStyle.IndexOf(tmpStyle["prcnt_max_rng_ph1"]);     // index = 5
					else if (e.Column < tmpStyle.IndexOf(tmpStyle["num_nrm_rng_ph3"])/*21*/)
						index = tmpStyle.IndexOf(tmpStyle["prcnt_max_rng_ph2"]);     // index = 15
					else index = tmpStyle.IndexOf(tmpStyle["prcnt_max_rng_ph3"]);     // index = 25
				}

				//try
				//{
				if (grid[e.Row, index] is float)
					if ((float)grid[e.Row, index] > 5)
						e.ForeBrush = ErrorBrush;
				//}
				//catch { }
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in CellInside_CellPaint()");
				throw;
			}
        }

		// если выбрана страница с фликером, то нужно показать окно с графиком фликера,
		// если выбрана другая страница, то окно убираем
		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (tabControl1.SelectedTab == tabFliker)
					MainWindow_.wndDocPQP.ShowFlikerGraph(true);
				else
					MainWindow_.wndDocPQP.ShowFlikerGraph(false);

				if (tabControl1.SelectedTab == tabVolValues)
					MainWindow_.wndDocPQP.ShowVolValuesGraph(true, curDeviceType_);
				else
					MainWindow_.wndDocPQP.ShowVolValuesGraph(false, curDeviceType_);

				if (tabControl1.SelectedTab == tabFValues)
					MainWindow_.wndDocPQP.ShowFValuesGraph(true);
				else
					MainWindow_.wndDocPQP.ShowFValuesGraph(false);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in tabControl1_SelectedIndexChanged()");
				throw;
			}
		}

		private void btnMaxMode_Click(object sender, EventArgs e)
		{
			try
			{
				frmPeakLoadTime wnd = new frmPeakLoadTime();
				if (wnd.ShowDialog(MainWindow_) != DialogResult.OK)
					return;
				dtMaxModeStart1_ = wnd.TimeMaxStart1;
				dtMaxModeEnd1_ = wnd.TimeMaxEnd1;
				if (dtMaxModeStart1_ == dtMaxModeEnd1_)
				{
					dtMaxModeStart1_ = DateTime.MinValue;
					dtMaxModeEnd1_ = DateTime.MinValue;
				}

				dtMaxModeStart2_ = wnd.TimeMaxStart2;
				dtMaxModeEnd2_ = wnd.TimeMaxEnd2;
				if (dtMaxModeStart2_ == dtMaxModeEnd2_)
				{
					dtMaxModeStart2_ = DateTime.MinValue;
					dtMaxModeEnd2_ = DateTime.MinValue;
				}

				if (!wnd.GetConstrNPLtopMaxMode(out constrNPLtopMax_)) constrNPLtopMax_ = float.NaN;
				if (!wnd.GetConstrUPLtopMaxMode(out constrUPLtopMax_)) constrUPLtopMax_ = float.NaN;
				if (!wnd.GetConstrNPLbottomMaxMode(out constrNPLbottomMax_)) constrNPLbottomMax_ = float.NaN;
				if (!wnd.GetConstrUPLbottomMaxMode(out constrUPLbottomMax_)) constrUPLbottomMax_ = float.NaN;
				if (!wnd.GetConstrNPLtopMinMode(out constrNPLtopMin_)) constrNPLtopMin_ = float.NaN;
				if (!wnd.GetConstrUPLtopMinMode(out constrUPLtopMin_)) constrUPLtopMin_ = float.NaN;
				if (!wnd.GetConstrNPLbottomMinMode(out constrNPLbottomMin_)) constrNPLbottomMin_ = float.NaN;
				if (!wnd.GetConstrUPLbottomMinMode(out constrUPLbottomMin_)) constrUPLbottomMin_ = float.NaN;

				bNeedMaxModeForEtPQP_A_ = true;
				if (dtMaxModeStart1_ == DateTime.MinValue && dtMaxModeEnd1_ == DateTime.MinValue &&
					dtMaxModeStart2_ == DateTime.MinValue && dtMaxModeEnd2_ == DateTime.MinValue)
					bNeedMaxModeForEtPQP_A_ = false;

				drawdgU_Deviation();

				SetCommonCaption(dtMaxModeStart1_, dtMaxModeEnd1_, dtMaxModeStart2_, dtMaxModeEnd2_);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in btnMaxMode_Click()");
				throw;
			}
		}

        #endregion

		private string GetPeakLoadString(DateTime dtStart1, DateTime dtEnd1, DateTime dtStart2, DateTime dtEnd2)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string plFrmt = string.Empty;
				if (dtStart1.TimeOfDay != dtEnd1.TimeOfDay)
				{
					if (dtStart2.TimeOfDay != dtEnd2.TimeOfDay)
						plFrmt = "{0} - {1}; {2} - {3}";
					else
						plFrmt = "{0} - {1}";
				}
				else
				{
					if (dtStart2.TimeOfDay != dtEnd2.TimeOfDay)
						plFrmt = "{2} - {3}";
					else
						plFrmt = rm.GetString("name_peakload_none");
				}
				string plTimeFrmt = "HH:mm";
				return string.Format(plFrmt,
					dtStart1.ToString(plTimeFrmt),
					dtEnd1.ToString(plTimeFrmt),
					dtStart2.ToString(plTimeFrmt),
					dtEnd2.ToString(plTimeFrmt));
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetPeakLoadString()");
				throw;
			}
		}

        private void ChangePercentFormat()
		{
			try
			{
				// dgFrequencyDeparture

				string table_name = "day_avg_parameters_t2";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) table_name = "pqp_f";

				DataGridColumnGroupCaption curCap = (dgFrequencyDeparture.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgFrequencyDeparture.TableStyles[table_name].GridColumnStyles["prcnt_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgFrequencyDeparture.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dgU_Deviation

				if (curDeviceType_ == EmDeviceType.ETPQP_A) table_name = "pqp_du";

				curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["prcnt_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_global"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_global"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_global"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

                if (curDeviceType_ != EmDeviceType.ETPQP_A)
                {
                    curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_top"] as DataGridColumnGroupCaption);
                    if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

                    curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["calc_max_rng_top"] as DataGridColumnGroupCaption);
                    if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

                    curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_bottom"] as DataGridColumnGroupCaption);
                    if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

                    curCap = (dgU_Deviation.TableStyles[table_name].GridColumnStyles["calc_max_rng_bottom"] as DataGridColumnGroupCaption);
                    if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
                }

				// dgNonSymmetry

				table_name = "day_avg_parameters_t1";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) table_name = "pqp_nonsymmetry";

				curCap = (dgNonSymmetry.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgNonSymmetry.TableStyles[table_name].GridColumnStyles["prcnt_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgNonSymmetry.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgNonSymmetry.TableStyles[table_name].GridColumnStyles["calc_nrm_rng"] as DataGridColumnGroupCaption);
				if(curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				curCap = (dgNonSymmetry.TableStyles[table_name].GridColumnStyles["calc_max_rng"] as DataGridColumnGroupCaption);
				if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

				// dgUNonsinusoidality

				bool dgNonsinus1Visible = true;
				bool dgNonsinus2Visible = true;
				if (curDeviceType_ == EmDeviceType.ETPQP_A &&
					(CurConnectScheme != ConnectScheme.Ph3W3 && CurConnectScheme != ConnectScheme.Ph3W3_B_calc))
					dgNonsinus2Visible = false;
				if (curDeviceType_ == EmDeviceType.ETPQP_A &&
					(CurConnectScheme == ConnectScheme.Ph3W3 || CurConnectScheme == ConnectScheme.Ph3W3_B_calc))
					dgNonsinus1Visible = false;
				if (CurConnectScheme == ConnectScheme.Ph1W2) dgNonsinus2Visible = false;

				table_name = "day_avg_parameters_t4";
				if (curDeviceType_ == EmDeviceType.ETPQP_A) table_name = "pqp_nonsinus";

				if (dgNonsinus1Visible)
				{
					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["calc_max_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["calc_max_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality.TableStyles[table_name].GridColumnStyles["calc_max_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				}

				// dgUNonsinusoidality2

				//if (curDeviceType_ != EmDeviceType.ETPQP_A)
				//{
				if(dgNonsinus2Visible)
				{
					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["calc_max_rng_ph1"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["calc_max_rng_ph2"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_nrm_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_max_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["prcnt_out_max_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["calc_nrm_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);

					curCap = (dgUNonsinusoidality2.TableStyles[table_name].GridColumnStyles["calc_max_rng_ph3"] as DataGridColumnGroupCaption);
					if (curCap != null) curCap.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ChangePercentFormat():");
				//throw;
			}
		}

		private string GetPgConnectionString()
		{
			string ConnectString = "";
			if (curDeviceType_ == EmDeviceType.EM32)
				ConnectString = settings_.PgServers[curPgServerIndex_].PgConnectionStringEm32;
			else if (curDeviceType_ == EmDeviceType.EM33T ||
				curDeviceType_ == EmDeviceType.EM31K ||
				curDeviceType_ == EmDeviceType.EM33T1)
				ConnectString = settings_.PgServers[curPgServerIndex_].PgConnectionStringEm33;
			else if (curDeviceType_ == EmDeviceType.ETPQP)
				ConnectString = settings_.PgServers[curPgServerIndex_].PgConnectionStringEtPQP;
			else if (curDeviceType_ == EmDeviceType.ETPQP_A)
				ConnectString = settings_.PgServers[curPgServerIndex_].PgConnectionStringEtPQP_A;
			return ConnectString;
        }

        #region ExportPQPReport

        public void ExportPQPReport_RD(ConnectScheme cs, bool flikkerExists)
        {
            try
            {
                MainWindow_.Cursor = Cursors.WaitCursor;
                ExportToExcel.PQPReport_RD exporter = new ExportToExcel.PQPReport_RD(settings_,
					ref dgU_Deviation, ref dgVolValues, ref dgFrequencyDeparture, ref dgUNonsinusoidality,
                    ref dgUNonsinusoidality2, ref dgDips, ref dgOvers, ref dgFlicker, ref dgFlickerLong,
                    ref dgNonSymmetry, sdtToPL1_, edtToPL1_, sdtToPL2_, edtToPL2_,
					curStartDateTime_, curEndDateTime_, curDeviceType_, cs);
                exporter.ExportReport(flikkerExists);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in ExportPQPReport_RD():  ");
                throw;
            }
            finally
            {
                MainWindow_.Cursor = Cursors.Default;
            }
        }

        public void ExportReportPQP_GOST(ConnectScheme cs, bool flikkerExists)
        {
            try
            {
                MainWindow_.Cursor = Cursors.WaitCursor;
                ExportToExcel.PQPReport_GOST exporter = new ExportToExcel.PQPReport_GOST(settings_,
                    ref dgU_Deviation, ref dgVolValues, ref dgFrequencyDeparture, ref dgUNonsinusoidality,
                    ref dgUNonsinusoidality2, ref dgDips, ref dgOvers, ref dgFlicker, ref dgFlickerLong,
                    ref dgNonSymmetry, sdtToPL1_, edtToPL1_, sdtToPL2_, edtToPL2_,
					curStartDateTime_, curEndDateTime_, curDeviceType_, cs);
                exporter.ExportReport(flikkerExists);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in ExportPQPReport_GOST():  ");
                throw;
            }
            finally
            {
                MainWindow_.Cursor = Cursors.Default;
            }
        }

        public void ExportPQPReport_PQP_A(ConnectScheme cs, long ser_num, PQPProtocolType protType, 
										double gpsLatitude, double gpsLongitude,
										DEVICE_VERSIONS newDipSwellMode)
        {
            try
            {
                MainWindow_.Cursor = Cursors.WaitCursor;

				ExportToExcel.PQPReportEtPQP_A exporter;
				if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
				{
					exporter = new ExportToExcel.PQPReportEtPQP_A(settings_,
						ref dgU_Deviation, ref dgVolValues, ref dgFrequencyDeparture, ref dgUNonsinusoidality,
						ref dgUNonsinusoidality2, ref dgDips, ref dgOvers, ref dgFlickerNum,
						ref dgNonSymmetry, ref dgInterharm,
						curStartDateTime_, curEndDateTime_, curDeviceType_, cs, ser_num, protType,
						gpsLatitude, gpsLongitude,
						newDipSwellMode);
				}
				else
				{
					exporter = new ExportToExcel.PQPReportEtPQP_A(settings_,
						ref dgU_Deviation, ref dgVolValues, ref dgFrequencyDeparture, ref dgUNonsinusoidality,
						ref dgUNonsinusoidality2, ref dgDips, ref dgDips2, ref dgOvers, ref dgFlickerNum,
						ref dgNonSymmetry, ref dgInterharm,
						curStartDateTime_, curEndDateTime_, curDeviceType_, cs, ser_num, protType,
						gpsLatitude, gpsLongitude,
						newDipSwellMode);
				}
                exporter.ExportReport();
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in ExportPQPReport_PQP_A():  ");
                throw;
            }
            finally
            {
                MainWindow_.Cursor = Cursors.Default;
            }
        }

		// для протокола с добавленной таблицей температур
		// (здесь не надо выделять старую версию прошивки)
		public void ExportPQPReport_PQP_A(ConnectScheme cs, long ser_num, PQPProtocolType protType,
										double gpsLatitude, double gpsLongitude, //bool autocorrect_time_gps_enable,
										short temperatureMin, short temperatureMax)
		{
			try
			{
				MainWindow_.Cursor = Cursors.WaitCursor;

				ExportToExcel.PQPReportEtPQP_A exporter = new ExportToExcel.PQPReportEtPQP_A(settings_,
						ref dgU_Deviation, ref dgVolValues, ref dgFrequencyDeparture, ref dgUNonsinusoidality,
						ref dgUNonsinusoidality2, ref dgDips, ref dgDips2, ref dgOvers, ref dgFlickerNum,
						ref dgNonSymmetry, ref dgInterharm,
						curStartDateTime_, curEndDateTime_, curDeviceType_, cs, ser_num, protType,
						gpsLatitude, gpsLongitude, //autocorrect_time_gps_enable,
						DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073,
						temperatureMin, temperatureMax);
				
				exporter.ExportReport();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportPQPReport_PQP_A():  ");
				throw;
			}
			finally
			{
				MainWindow_.Cursor = Cursors.Default;
			}
		}

        #endregion
    }
}
