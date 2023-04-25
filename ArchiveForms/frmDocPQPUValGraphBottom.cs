using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml.Serialization;

using WeifenLuo.WinFormsUI;
using ZedGraph;
using EmServiceLib;
using DataGridColumnStyles;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmGraphBox.
	/// </summary>
	public class frmDocPQPUValGraphBottom : DockContentGraphMethods
	{
		private ConnectScheme conScheme_;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ToolStrip zedToolStrip;
		private ToolStripButton tsYGridLine;
		private ToolStripButton tsYMinorGridLine;
		private ToolStripButton tsXGridLine;
		private ToolStripButton tsXMinorGridLine;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripButton tsY2GridLine;
		private ToolStripButton tsY2MinorGridLine;

		// <summary>
		// Settings object
		// </summary>
		//private EmDataSaver.Settings settings_;

		/// <summary>
		/// Pointer to the main application window
		/// </summary>
		private frmMain MainWindow_;
		private ToolStripSeparator toolStripSeparator5;
        private ColorDialog dlgColor;
		private SplitContainer splitContainer1;

		private EnergomonitoringXP.Graph.GraphColors graphColors_ = 
			new EnergomonitoringXP.Graph.GraphColors();
		private SplitContainer splitContainer2;
        public ZedGraphControl zedGraph;

		// флаг показывает, что линии НДП и ПДП уже отрисованы
		private bool bNdpPdpPainted_ = false;
        private Panel panel1;
        private RadioButton rbOff;
        private RadioButton rbMin;
        private RadioButton rbMax;
        private RadioButton rbDay;
        private Panel panel2;
        private Button btnColorUca;
        private Button btnColorUbc;
        private Button btnColorUab;
        private Button btnColorUc;
        private Button btnColorUb;
        private Button btnColorUa;
        private Button btnColorUy;
        private CheckBox chbUca;
        private CheckBox chbUbc;
        private CheckBox chbUab;
        private CheckBox chbUc;
        private CheckBox chbUb;
        private CheckBox chbUa;
        private CheckBox chbUy;

		private PQPGraphBottomSettings graphSettings_ = null;

		public frmDocPQPUValGraphBottom(frmMain MainWindow)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			//this.settings_ = settings;
			this.MainWindow_ = MainWindow;
		}

		public void EnableGraphs(ConnectScheme conScheme)
		{
			try
			{
				conScheme_ = conScheme;

				switch (conScheme)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						chbUy.Enabled = true;
						chbUa.Enabled = true;
						chbUb.Enabled = true;
						chbUc.Enabled = true;
						chbUab.Enabled = true;
						chbUbc.Enabled = true;
						chbUca.Enabled = true;
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						chbUy.Enabled = true;
						chbUa.Enabled = false;
						chbUb.Enabled = false;
						chbUc.Enabled = false;
						chbUab.Enabled = true;
						chbUbc.Enabled = true;
						chbUca.Enabled = true;
						break;
					case ConnectScheme.Ph1W2:
						chbUy.Enabled = false;
						chbUa.Enabled = true;
						chbUb.Enabled = false;
						chbUc.Enabled = false;
						chbUab.Enabled = false;
						chbUbc.Enabled = false;
						chbUca.Enabled = false;
						break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EnableGraphs(): ");
				throw;
			}
		}

		public void ClearGraphs()
		{
			try
			{
				this.zedGraph.GraphPane.CurveList.Clear();
				this.zedGraph.Refresh();

				chbUy.Checked = false;
				chbUa.Checked = false;
				chbUb.Checked = false;
				chbUc.Checked = false;
				chbUab.Checked = false;
				chbUbc.Checked = false;
				chbUca.Checked = false;

				bNdpPdpPainted_ = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ClearGraphs(): ");
				throw;
			}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocPQPUValGraphBottom));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.zedGraph = new ZedGraph.ZedGraphControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.rbOff = new System.Windows.Forms.RadioButton();
			this.rbMin = new System.Windows.Forms.RadioButton();
			this.rbMax = new System.Windows.Forms.RadioButton();
			this.rbDay = new System.Windows.Forms.RadioButton();
			this.panel2 = new System.Windows.Forms.Panel();
			this.btnColorUca = new System.Windows.Forms.Button();
			this.btnColorUbc = new System.Windows.Forms.Button();
			this.btnColorUab = new System.Windows.Forms.Button();
			this.btnColorUc = new System.Windows.Forms.Button();
			this.btnColorUb = new System.Windows.Forms.Button();
			this.btnColorUa = new System.Windows.Forms.Button();
			this.btnColorUy = new System.Windows.Forms.Button();
			this.chbUca = new System.Windows.Forms.CheckBox();
			this.chbUbc = new System.Windows.Forms.CheckBox();
			this.chbUab = new System.Windows.Forms.CheckBox();
			this.chbUc = new System.Windows.Forms.CheckBox();
			this.chbUb = new System.Windows.Forms.CheckBox();
			this.chbUa = new System.Windows.Forms.CheckBox();
			this.chbUy = new System.Windows.Forms.CheckBox();
			this.zedToolStrip = new System.Windows.Forms.ToolStrip();
			this.tsYGridLine = new System.Windows.Forms.ToolStripButton();
			this.tsYMinorGridLine = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsXGridLine = new System.Windows.Forms.ToolStripButton();
			this.tsXMinorGridLine = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsY2GridLine = new System.Windows.Forms.ToolStripButton();
			this.tsY2MinorGridLine = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.dlgColor = new System.Windows.Forms.ColorDialog();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.zedToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
			this.splitContainer1.Panel2.Controls.Add(this.panel2);
			// 
			// splitContainer2
			// 
			resources.ApplyResources(this.splitContainer2, "splitContainer2");
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
			this.splitContainer2.Panel1.Controls.Add(this.zedGraph);
			// 
			// splitContainer2.Panel2
			// 
			resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
			this.splitContainer2.Panel2.Controls.Add(this.panel1);
			// 
			// zedGraph
			// 
			resources.ApplyResources(this.zedGraph, "zedGraph");
			this.zedGraph.CultureInfo = new System.Globalization.CultureInfo("");
			this.zedGraph.IsAutoScrollRange = false;
			this.zedGraph.IsEnableHPan = true;
			this.zedGraph.IsEnableVPan = true;
			this.zedGraph.IsEnableZoom = true;
			this.zedGraph.IsScrollY2 = false;
			this.zedGraph.IsShowContextMenu = true;
			this.zedGraph.IsShowHScrollBar = false;
			this.zedGraph.IsShowPointValues = false;
			this.zedGraph.IsShowVScrollBar = false;
			this.zedGraph.IsZoomOnMouseCenter = false;
			this.zedGraph.Name = "zedGraph";
			this.zedGraph.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraph.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraph.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraph.PointDateFormat = "g";
			this.zedGraph.PointValueFormat = "G";
			this.zedGraph.ScrollMaxX = 0D;
			this.zedGraph.ScrollMaxY = 0D;
			this.zedGraph.ScrollMaxY2 = 0D;
			this.zedGraph.ScrollMinX = 0D;
			this.zedGraph.ScrollMinY = 0D;
			this.zedGraph.ScrollMinY2 = 0D;
			this.zedGraph.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraph.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraph.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraph.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraph.ZoomStepFraction = 0.1D;
			// 
			// panel1
			// 
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Controls.Add(this.rbOff);
			this.panel1.Controls.Add(this.rbMin);
			this.panel1.Controls.Add(this.rbMax);
			this.panel1.Controls.Add(this.rbDay);
			this.panel1.Name = "panel1";
			// 
			// rbOff
			// 
			resources.ApplyResources(this.rbOff, "rbOff");
			this.rbOff.Checked = true;
			this.rbOff.Name = "rbOff";
			this.rbOff.TabStop = true;
			this.rbOff.UseVisualStyleBackColor = true;
			this.rbOff.CheckedChanged += new System.EventHandler(this.rbDayMaxMin_CheckedChanged);
			// 
			// rbMin
			// 
			resources.ApplyResources(this.rbMin, "rbMin");
			this.rbMin.Name = "rbMin";
			this.rbMin.UseVisualStyleBackColor = true;
			this.rbMin.CheckedChanged += new System.EventHandler(this.rbDayMaxMin_CheckedChanged);
			// 
			// rbMax
			// 
			resources.ApplyResources(this.rbMax, "rbMax");
			this.rbMax.Name = "rbMax";
			this.rbMax.UseVisualStyleBackColor = true;
			this.rbMax.CheckedChanged += new System.EventHandler(this.rbDayMaxMin_CheckedChanged);
			// 
			// rbDay
			// 
			resources.ApplyResources(this.rbDay, "rbDay");
			this.rbDay.Name = "rbDay";
			this.rbDay.UseVisualStyleBackColor = true;
			this.rbDay.CheckedChanged += new System.EventHandler(this.rbDayMaxMin_CheckedChanged);
			// 
			// panel2
			// 
			resources.ApplyResources(this.panel2, "panel2");
			this.panel2.Controls.Add(this.btnColorUca);
			this.panel2.Controls.Add(this.btnColorUbc);
			this.panel2.Controls.Add(this.btnColorUab);
			this.panel2.Controls.Add(this.btnColorUc);
			this.panel2.Controls.Add(this.btnColorUb);
			this.panel2.Controls.Add(this.btnColorUa);
			this.panel2.Controls.Add(this.btnColorUy);
			this.panel2.Controls.Add(this.chbUca);
			this.panel2.Controls.Add(this.chbUbc);
			this.panel2.Controls.Add(this.chbUab);
			this.panel2.Controls.Add(this.chbUc);
			this.panel2.Controls.Add(this.chbUb);
			this.panel2.Controls.Add(this.chbUa);
			this.panel2.Controls.Add(this.chbUy);
			this.panel2.Name = "panel2";
			// 
			// btnColorUca
			// 
			resources.ApplyResources(this.btnColorUca, "btnColorUca");
			this.btnColorUca.BackColor = System.Drawing.Color.Brown;
			this.btnColorUca.Name = "btnColorUca";
			this.btnColorUca.UseVisualStyleBackColor = false;
			this.btnColorUca.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUbc
			// 
			resources.ApplyResources(this.btnColorUbc, "btnColorUbc");
			this.btnColorUbc.BackColor = System.Drawing.Color.LimeGreen;
			this.btnColorUbc.Name = "btnColorUbc";
			this.btnColorUbc.UseVisualStyleBackColor = false;
			this.btnColorUbc.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUab
			// 
			resources.ApplyResources(this.btnColorUab, "btnColorUab");
			this.btnColorUab.BackColor = System.Drawing.Color.BurlyWood;
			this.btnColorUab.Name = "btnColorUab";
			this.btnColorUab.UseVisualStyleBackColor = false;
			this.btnColorUab.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUc
			// 
			resources.ApplyResources(this.btnColorUc, "btnColorUc");
			this.btnColorUc.BackColor = System.Drawing.Color.Red;
			this.btnColorUc.Name = "btnColorUc";
			this.btnColorUc.UseVisualStyleBackColor = false;
			this.btnColorUc.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUb
			// 
			resources.ApplyResources(this.btnColorUb, "btnColorUb");
			this.btnColorUb.BackColor = System.Drawing.Color.Green;
			this.btnColorUb.Name = "btnColorUb";
			this.btnColorUb.UseVisualStyleBackColor = false;
			this.btnColorUb.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUa
			// 
			resources.ApplyResources(this.btnColorUa, "btnColorUa");
			this.btnColorUa.BackColor = System.Drawing.Color.DarkOrange;
			this.btnColorUa.Name = "btnColorUa";
			this.btnColorUa.UseVisualStyleBackColor = false;
			this.btnColorUa.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUy
			// 
			resources.ApplyResources(this.btnColorUy, "btnColorUy");
			this.btnColorUy.BackColor = System.Drawing.Color.Blue;
			this.btnColorUy.ForeColor = System.Drawing.Color.Black;
			this.btnColorUy.Name = "btnColorUy";
			this.btnColorUy.UseVisualStyleBackColor = false;
			this.btnColorUy.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// chbUca
			// 
			resources.ApplyResources(this.chbUca, "chbUca");
			this.chbUca.Name = "chbUca";
			this.chbUca.UseVisualStyleBackColor = true;
			this.chbUca.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUbc
			// 
			resources.ApplyResources(this.chbUbc, "chbUbc");
			this.chbUbc.Name = "chbUbc";
			this.chbUbc.UseVisualStyleBackColor = true;
			this.chbUbc.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUab
			// 
			resources.ApplyResources(this.chbUab, "chbUab");
			this.chbUab.Name = "chbUab";
			this.chbUab.UseVisualStyleBackColor = true;
			this.chbUab.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUc
			// 
			resources.ApplyResources(this.chbUc, "chbUc");
			this.chbUc.Name = "chbUc";
			this.chbUc.UseVisualStyleBackColor = true;
			this.chbUc.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUb
			// 
			resources.ApplyResources(this.chbUb, "chbUb");
			this.chbUb.Name = "chbUb";
			this.chbUb.UseVisualStyleBackColor = true;
			this.chbUb.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUa
			// 
			resources.ApplyResources(this.chbUa, "chbUa");
			this.chbUa.Name = "chbUa";
			this.chbUa.UseVisualStyleBackColor = true;
			this.chbUa.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUy
			// 
			resources.ApplyResources(this.chbUy, "chbUy");
			this.chbUy.Name = "chbUy";
			this.chbUy.UseVisualStyleBackColor = true;
			this.chbUy.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// zedToolStrip
			// 
			resources.ApplyResources(this.zedToolStrip, "zedToolStrip");
			this.zedToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsYGridLine,
            this.tsYMinorGridLine,
            this.toolStripSeparator1,
            this.tsXGridLine,
            this.tsXMinorGridLine,
            this.toolStripSeparator2,
            this.tsY2GridLine,
            this.tsY2MinorGridLine,
            this.toolStripSeparator5});
			this.zedToolStrip.Name = "zedToolStrip";
			// 
			// tsYGridLine
			// 
			resources.ApplyResources(this.tsYGridLine, "tsYGridLine");
			this.tsYGridLine.Checked = true;
			this.tsYGridLine.CheckOnClick = true;
			this.tsYGridLine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsYGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsYGridLine.Name = "tsYGridLine";
			this.tsYGridLine.Click += new System.EventHandler(this.tsYGridLine_Click);
			// 
			// tsYMinorGridLine
			// 
			resources.ApplyResources(this.tsYMinorGridLine, "tsYMinorGridLine");
			this.tsYMinorGridLine.CheckOnClick = true;
			this.tsYMinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsYMinorGridLine.Name = "tsYMinorGridLine";
			this.tsYMinorGridLine.Click += new System.EventHandler(this.tsYMinorGridLine_Click);
			// 
			// toolStripSeparator1
			// 
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			// 
			// tsXGridLine
			// 
			resources.ApplyResources(this.tsXGridLine, "tsXGridLine");
			this.tsXGridLine.Checked = true;
			this.tsXGridLine.CheckOnClick = true;
			this.tsXGridLine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsXGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsXGridLine.Name = "tsXGridLine";
			this.tsXGridLine.Click += new System.EventHandler(this.tsXGridLine_Click);
			// 
			// tsXMinorGridLine
			// 
			resources.ApplyResources(this.tsXMinorGridLine, "tsXMinorGridLine");
			this.tsXMinorGridLine.CheckOnClick = true;
			this.tsXMinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsXMinorGridLine.Name = "tsXMinorGridLine";
			this.tsXMinorGridLine.Click += new System.EventHandler(this.tsXMinorGridLine_Click);
			// 
			// toolStripSeparator2
			// 
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			// 
			// tsY2GridLine
			// 
			resources.ApplyResources(this.tsY2GridLine, "tsY2GridLine");
			this.tsY2GridLine.CheckOnClick = true;
			this.tsY2GridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsY2GridLine.Name = "tsY2GridLine";
			this.tsY2GridLine.Click += new System.EventHandler(this.tsY2GridLine_Click);
			// 
			// tsY2MinorGridLine
			// 
			resources.ApplyResources(this.tsY2MinorGridLine, "tsY2MinorGridLine");
			this.tsY2MinorGridLine.CheckOnClick = true;
			this.tsY2MinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsY2MinorGridLine.Name = "tsY2MinorGridLine";
			this.tsY2MinorGridLine.Click += new System.EventHandler(this.tsY2MinorGridLine_Click);
			// 
			// toolStripSeparator5
			// 
			resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			// 
			// frmDocPQPUValGraphBottom
			// 
			resources.ApplyResources(this, "$this");
			this.AutoHidePortion = 0.2D;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CloseButton = false;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.zedToolStrip);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "frmDocPQPUValGraphBottom";
			this.Load += new System.EventHandler(this.frmGraphBox_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.zedToolStrip.ResumeLayout(false);
			this.zedToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Grid lines

		private void tsYGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.YAxis.IsShowGrid = tsYGridLine.Checked;
			this.zedGraph.GraphPane.YAxis.IsVisible = tsYGridLine.Checked;
			this.tsYMinorGridLine.Enabled = tsYGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsYMinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.YAxis.IsShowMinorGrid = tsYMinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsXGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.XAxis.IsShowGrid = tsXGridLine.Checked;
			this.zedGraph.GraphPane.XAxis.IsVisible = tsXGridLine.Checked;
			this.tsXMinorGridLine.Enabled = tsXGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsXMinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.XAxis.IsShowMinorGrid = tsXMinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsY2GridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.Y2Axis.IsShowGrid = tsY2GridLine.Checked;
			this.zedGraph.GraphPane.Y2Axis.IsVisible = tsY2GridLine.Checked;
			this.tsY2MinorGridLine.Enabled = tsY2GridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsY2MinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.Y2Axis.IsShowMinorGrid = tsY2MinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		#endregion

		private void frmGraphBox_Load(object sender, EventArgs e)
		{
			try
			{
				graphSettings_ = new PQPGraphBottomSettings();
				graphSettings_.LoadSettings();
				btnColorUy.BackColor = Color.FromArgb(graphSettings_.ColorUy);
				btnColorUa.BackColor = Color.FromArgb(graphSettings_.ColorUa);
				btnColorUb.BackColor = Color.FromArgb(graphSettings_.ColorUb);
				btnColorUc.BackColor = Color.FromArgb(graphSettings_.ColorUc);
				btnColorUab.BackColor = Color.FromArgb(graphSettings_.ColorUab);
				btnColorUbc.BackColor = Color.FromArgb(graphSettings_.ColorUbc);
				btnColorUca.BackColor = Color.FromArgb(graphSettings_.ColorUca);

				#region Initializing zedGraph object

				this.zedGraph.BackColor = Color.Black;
				GraphPane gPane = this.zedGraph.GraphPane;
				gPane.Title = string.Empty;
				gPane.IsShowTitle = false;

				gPane.XAxis.Type = AxisType.Date;
				gPane.XAxis.Title = String.Empty;
				gPane.XAxis.IsShowGrid = false;
				gPane.XAxis.GridColor = Color.DarkGray;
				gPane.XAxis.GridDashOn = 5;
				gPane.XAxis.GridDashOff = 5;
				gPane.XAxis.Step = (double)new XDate(0, 0, 0, 0, 0, 30);
				gPane.XAxis.IsVisible = true;

				gPane.XAxis.IsShowMinorGrid = false;
				gPane.XAxis.MinorGridColor = Color.Gray;
				gPane.XAxis.MinorGridDashOn = 1;
				gPane.XAxis.MinorGridDashOff = 5;
				gPane.XAxis.MinorStep = (double)new XDate(0, 0, 0, 0, 0, 3);

				gPane.YAxis.Title = String.Empty;
				gPane.YAxis.IsShowGrid = false;
				gPane.YAxis.GridColor = Color.DarkGray;
				gPane.YAxis.GridDashOn = 1;
				gPane.YAxis.GridDashOff = 0;
				gPane.YAxis.Step = 0.005;
				gPane.YAxis.IsVisible = true;

				gPane.YAxis.IsShowMinorGrid = false;
				gPane.YAxis.MinorGridColor = Color.LightGray;
				gPane.YAxis.MinorGridDashOn = 1;
				gPane.YAxis.MinorGridDashOff = 1;
				gPane.YAxis.MinorStep = 0.001;

				gPane.Y2Axis.Title = String.Empty;
				gPane.Y2Axis.IsShowGrid = false;
				gPane.Y2Axis.GridColor = Color.DarkGray;
				gPane.Y2Axis.GridDashOn = 1;
				gPane.Y2Axis.GridDashOff = 0;
				gPane.Y2Axis.Step = 0.005;
				gPane.Y2Axis.IsVisible = false;

				gPane.Y2Axis.IsShowMinorGrid = false;
				gPane.Y2Axis.MinorGridColor = Color.LightGray;
				gPane.Y2Axis.MinorGridDashOn = 1;
				gPane.Y2Axis.MinorGridDashOff = 1;
				gPane.Y2Axis.MinorStep = 0.001;

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmGraphBox_Load(): ");
				throw;
			}
		}

		private void btnColor_Click(object sender, EventArgs e)
		{
			try
			{
				if (dlgColor.ShowDialog() == DialogResult.OK)
				{
					(sender as Button).BackColor = dlgColor.Color;
				}
				graphSettings_.ColorUy = btnColorUy.BackColor.ToArgb();
				graphSettings_.ColorUa = btnColorUa.BackColor.ToArgb();
				graphSettings_.ColorUb = btnColorUb.BackColor.ToArgb();
				graphSettings_.ColorUc = btnColorUc.BackColor.ToArgb();
				graphSettings_.ColorUab = btnColorUab.BackColor.ToArgb();
				graphSettings_.ColorUbc = btnColorUbc.BackColor.ToArgb();
				graphSettings_.ColorUca = btnColorUca.BackColor.ToArgb();
				graphSettings_.SaveSettings();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in btnColor_Click(): ");
				throw;
			}
		}

		private void chbUx_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				string colName = "d_u_y";
				Button curBtn = btnColorUy;
				if (sender == chbUy)
				{
					colName = "d_u_y";
					curBtn = btnColorUy;
				}
				else if (sender == chbUa)
				{
					colName = "d_u_a";
					curBtn = btnColorUa;
				}
				else if (sender == chbUb)
				{
					colName = "d_u_b";
					curBtn = btnColorUb;
				}
				else if (sender == chbUc)
				{
					colName = "d_u_c";
					curBtn = btnColorUc;
				}
				else if (sender == chbUab)
				{
					colName = "d_u_ab";
					curBtn = btnColorUab;
				}
				else if (sender == chbUbc)
				{
					colName = "d_u_bc";
					curBtn = btnColorUbc;
				}
				else if (sender == chbUca)
				{
					colName = "d_u_ca";
					curBtn = btnColorUca;
				}

				// add curve
				if ((sender as CheckBox).Checked)
				{
					DataGrid curDg = MainWindow_.wndDocPQP.wndDocPQPMain.dgVolValues;
					DataSet tmpDataSet = (DataSet)curDg.DataSource;
					if (tmpDataSet.Tables[0].Rows.Count <= 0)
						return;

					int columnIndex = tmpDataSet.Tables[0].Columns.IndexOf(colName);
					Color curveColor = curBtn.BackColor;

					string legend = AddCurver(ref curDg, columnIndex, false, curveColor);
					(sender as CheckBox).Tag = legend;

					curBtn.Enabled = false;
				}
				//remove curve
				else
				{
					// releasing color
					graphColors_.ReleaseColor(curBtn.BackColor);

					// deleting curve
					if ((sender as CheckBox).Tag != null)
					{
						GraphPane gPane = zedGraph.GraphPane;
						if (gPane.CurveList.IndexOf((sender as CheckBox).Tag.ToString()) >= 0)
						{
							gPane.CurveList.Remove(gPane.CurveList[(sender as CheckBox).Tag.ToString()]);
						}

						// Zoom set defalult
						Graphics g = zedGraph.CreateGraphics();
						gPane.XAxis.ResetAutoScale(gPane, g);
						gPane.YAxis.ResetAutoScale(gPane, g);
						gPane.Y2Axis.ResetAutoScale(gPane, g);
						g.Dispose();
						zedGraph.Refresh();
					}

					curBtn.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in chbUx_CheckedChanged(): ");
				throw;
			}
		}

		/// <summary>
		/// Adding curver to the graph window
		/// </summary>
		/// <param name="dg">Datagrid object</param>
		/// <param name="column">column number</param>
		/// <param name="Y2Axis">is it must be added to the Y2 axis</param>
		/// <param name="curveColor">Curve color</param>
		private string AddCurver(ref DataGrid dg, int column, bool Y2Axis, Color curveColor)
		{
			try
			{
				if (column < 0) return "";
				if ((dg.DataSource as DataSet).Tables[0].Rows.Count <= 0) return "";

				// saving sort state and resorting to build graph correctly
				string SortRule = GetCurrentSortRule(dg);
				SortByColumn(dg, 0, false);

				// starting build point list
				GraphPane gPane = zedGraph.GraphPane;
				PointPairList list = new PointPairList();

				//int period_in_secs = 60;

				double _y = Conversions.object_2_double((dg.DataSource as DataSet).Tables[0].Rows[0][column]);
				double _x1 = (double)new XDate(Convert.ToDateTime(dg[0, 0]));
				list.Add(_x1, _y);
				for (int i = 0; i < (dg.DataSource as DataSet).Tables[0].Rows.Count - 1; i++)
				{
					double y = Conversions.object_2_double((dg.DataSource as DataSet).Tables[0].Rows[i + 1][column]);
					double x0 = (double)new XDate(Convert.ToDateTime(dg[i, 0]));
					list.Add(x0, y);
					double x1 = (double)new XDate(Convert.ToDateTime(dg[i + 1, 0]));
					list.Add(x1, y);
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

				CurveItem myCurve = gPane.AddCurve(legend, list, curveColor, SymbolType.None, 2.0F);

				if(!bNdpPdpPainted_)
					AddPdpNdpLine(list[0].X, list[list.Count - 1].X);

				//gPane.AxisChange(this.CreateGraphics());

				// Axis X, Y and Y2
				gPane.XAxis.IsVisible = tsXGridLine.Checked;
				gPane.XAxis.IsShowGrid = tsXGridLine.Checked;
				gPane.XAxis.IsShowMinorGrid = tsXMinorGridLine.Checked;

				gPane.YAxis.IsVisible = tsYGridLine.Checked;
				gPane.YAxis.IsShowGrid = tsYGridLine.Checked;
				gPane.YAxis.IsShowMinorGrid = tsYMinorGridLine.Checked;

				gPane.Y2Axis.IsVisible = tsY2GridLine.Checked;
				gPane.Y2Axis.IsShowGrid = tsY2GridLine.Checked;
				gPane.Y2Axis.IsShowMinorGrid = tsY2MinorGridLine.Checked;

				// Zoom set defalult
				Graphics g = zedGraph.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				zedGraph.Refresh();

				return legend;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddCurver(): ");
				throw;
			}
		}

		private void AddPdpNdpLine(double start, double end)
		{
			try
			{
				if (rbOff.Checked)
					return;

				DataGrid dg = MainWindow_.wndDocPQP.wndDocPQPMain.dgU_Deviation;
				int iRes = -1;
				for (int iRow = 0; iRow < (dg.DataSource as DataSet).Tables[0].Rows.Count; ++iRow)
				{
					string strTmp = (dg[iRow, 0]).ToString();
					if (rbDay.Checked)
					{
						if (!strTmp.Contains("'") && !strTmp.Contains("\""))
						{
							iRes = iRow;
							break;
						}
					}
					else if (rbMax.Checked)
					{
						if (strTmp.Contains("'") && !strTmp.Contains("\""))
						{
							iRes = iRow;
							break;
						}
					}
					else if (rbMin.Checked)
					{
						if (!strTmp.Contains("'") && strTmp.Contains("\""))
						{
							iRes = iRow;
							break;
						}
					}
				}
				if (iRes == -1)	// значения в таблице не найдены
					return;

				GridColumnStylesCollection st3 = dg.TableStyles[0].GridColumnStyles;
				double realNrmRngTop = 
					Conversions.object_2_double(dg[iRes, st3.IndexOf(st3["real_nrm_rng_top"])]);
				double realMaxRngTop = 
					Conversions.object_2_double(dg[iRes, st3.IndexOf(st3["real_max_rng_top"])]);
				double realNrmRngBottom =
					Conversions.object_2_double(dg[iRes, st3.IndexOf(st3["real_nrm_rng_bottom"])]);
				double realMaxRngBottom =
					Conversions.object_2_double(dg[iRes, st3.IndexOf(st3["real_max_rng_bottom"])]);

				string[] names = new string[] { "NDP+", "PDP+", "NDP-", "PDP-" };
				if (rbMax.Checked)
				{
					for (int iName = 0; iName < names.Length; ++iName)
						names[iName] += " '";
				}
				else if (rbMin.Checked)
				{
					for (int iName = 0; iName < names.Length; ++iName)
						names[iName] += " ''";
				}

				GraphPane gPane = zedGraph.GraphPane;
				PointPairList list = new PointPairList();
				list.Add(start, realNrmRngTop);
				list.Add(end, realNrmRngTop);
				gPane.AddCurve(names[0], list, Color.Green, SymbolType.None, 1.0F);

				list = new PointPairList();
				list.Add(start, realMaxRngTop);
				list.Add(end, realMaxRngTop);
				gPane.AddCurve(names[1], list, Color.Red, SymbolType.None, 1.0F);

				list = new PointPairList();
				list.Add(start, realNrmRngBottom);
				list.Add(end, realNrmRngBottom);
				gPane.AddCurve(names[2], list, Color.Green, SymbolType.None, 1.0F);

				list = new PointPairList();
				list.Add(start, realMaxRngBottom);
				list.Add(end, realMaxRngBottom);
				gPane.AddCurve(names[3], list, Color.Red, SymbolType.None, 1.0F);

				bNdpPdpPainted_ = true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddPdpNdpLine(): ");
				throw;
			}
		}

		private void DeletePdpNdpLines()
		{
			try
			{
				string[] names = new string[] { "NDP+", "NDP-", "PDP+", "PDP-",
					"NDP+ '", "NDP- '", "PDP+ '", "PDP- '", "NDP+ ''", "NDP- ''", "PDP+ ''", "PDP- ''" };
				GraphPane gPane = zedGraph.GraphPane;

				for (int i = 0; i < names.Length; ++i)
				{
					if (zedGraph.GraphPane.CurveList[names[i]] != null)
					{
						// releasing color
						graphColors_.ReleaseColor(zedGraph.GraphPane.CurveList[names[i]].Color);

						// deleting curve
						if (gPane.CurveList.IndexOf(names[i]) >= 0)
							gPane.CurveList.Remove(gPane.CurveList[names[i]]);
					}
				}
				// Zoom set defalult
				Graphics g = zedGraph.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				zedGraph.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeletePdpNdpLines(): ");
				throw;
			}
		}

		private void rbDayMaxMin_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (!(sender as RadioButton).Checked) return;

				DeletePdpNdpLines();

				DataGrid dg = MainWindow_.wndDocPQP.wndDocPQPMain.dgVolValues;
				if ((dg.DataSource as DataSet).Tables[0].Rows.Count < 1)
					return;

				// получаем начальную и конечную дату
				string SortRule = GetCurrentSortRule(dg);
				SortByColumn(dg, 0, false);
				double x1 = (double)new XDate(Convert.ToDateTime(dg[0, 0]));
				double x2 = (double)new XDate(Convert.ToDateTime(dg[
					(dg.DataSource as DataSet).Tables[0].Rows.Count - 1, 
					0]));
				// restoring sorting rule
				SetSortRule(dg, SortRule);

				AddPdpNdpLine(x1, x2);

				// Zoom set defalult
				Graphics g = zedGraph.CreateGraphics();
				zedGraph.GraphPane.XAxis.ResetAutoScale(zedGraph.GraphPane, g);
				zedGraph.GraphPane.YAxis.ResetAutoScale(zedGraph.GraphPane, g);
				zedGraph.GraphPane.Y2Axis.ResetAutoScale(zedGraph.GraphPane, g);
				g.Dispose();
				zedGraph.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in rbDayMaxMin_CheckedChanged(): ");
				throw;
			}
		}
	}

	public class PQPGraphBottomSettings
	{
		#region Fields

		/// <summary>EmSettings file name</summary>
		[NonSerialized]
		private string settingsFileName_ = "PQPGraphSettings.config";

		[NonSerialized]
		private Color clrUy_ = Color.Blue;

		[NonSerialized]
		private Color clrUa_ = Color.DarkOrange;

		[NonSerialized]
		private Color clrUb_ = Color.Green;

		[NonSerialized]
		private Color clrUc_ = Color.Red;

		[NonSerialized]
		private Color clrUab_ = Color.BurlyWood;

		[NonSerialized]
		private Color clrUbc_ = Color.LimeGreen;

		[NonSerialized]
		private Color clrUca_ = Color.Brown;

		#endregion

		#region Properties

		/// <summary>Gets settings file name</summary>
		public string SettingsFileName
		{
			get { return settingsFileName_; }
		}

		public int ColorUy
		{
			get { return clrUy_.ToArgb(); }
			set { clrUy_ = Color.FromArgb(value); }
		}

		public int ColorUa
		{
			get { return clrUa_.ToArgb(); }
			set { clrUa_ = Color.FromArgb(value); }
		}

		public int ColorUb
		{
			get { return clrUb_.ToArgb(); }
			set { clrUb_ = Color.FromArgb(value); }
		}

		public int ColorUc
		{
			get { return clrUc_.ToArgb(); }
			set { clrUc_ = Color.FromArgb(value); }
		}

		public int ColorUab
		{
			get { return clrUab_.ToArgb(); }
			set { clrUab_ = Color.FromArgb(value); }
		}

		public int ColorUbc
		{
			get { return clrUbc_.ToArgb(); }
			set { clrUbc_ = Color.FromArgb(value); }
		}

		public int ColorUca
		{
			get { return clrUca_.ToArgb(); }
			set { clrUca_ = Color.FromArgb(value); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor with defaut settings
		/// </summary>
		public PQPGraphBottomSettings()
		{
			settingsFileName_ = EmService.AppDirectory + "PQPGraphSettings.config";
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Serializes the class to the config file if any of the settings have changed.
		/// </summary>
		/// <returns>true if all ok or false</returns>
		public void SaveSettings()
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				// Create an XmlSerializer for the 
				// ApplicationSettings type.
				mySerializer = new XmlSerializer(typeof(PQPGraphBottomSettings));
				myWriter = new StreamWriter(settingsFileName_, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, this);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in PQPGraphBottomSettings::SaveSettings(): ");
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
		/// <returns>true if all ok or false</returns>
		public void LoadSettings()
		{
			XmlSerializer xmlSer = null;
			FileStream fs = null;
			try
			{
				// Create an XmlSerializer for the ApplicationSettings type.
				xmlSer = new XmlSerializer(typeof(PQPGraphBottomSettings));
				FileInfo fi = new FileInfo(settingsFileName_);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					fs = fi.OpenRead();
					// Create a new instance of the ApplicationSettings by
					// deserializing the config file.
					PQPGraphBottomSettings myAppSettings = (PQPGraphBottomSettings)xmlSer.Deserialize(fs);
					// Assign the property values to this instance of 
					// the ApplicationSettings class.
					this.clrUy_ = myAppSettings.clrUy_;
					this.clrUa_ = myAppSettings.clrUa_;
					this.clrUb_ = myAppSettings.clrUb_;
					this.clrUc_ = myAppSettings.clrUc_;
					this.clrUab_ = myAppSettings.clrUab_;
					this.clrUbc_ = myAppSettings.clrUbc_;
					this.clrUca_ = myAppSettings.clrUca_;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in PQPGraphBottomSettings::LoadSettings(): ");
				throw;
			}
			finally
			{
				if (fs != null) fs.Close();
			}
		}

		/// <summary>
		/// Clone method
		/// </summary>
		/// <returns>Copy of this object</returns>
		public PQPGraphBottomSettings Clone()
		{
			try
			{
				PQPGraphBottomSettings obj = new PQPGraphBottomSettings();
				obj.clrUy_ = this.clrUy_;
				obj.clrUa_ = this.clrUa_;
				obj.clrUb_ = this.clrUb_;
				obj.clrUc_ = this.clrUc_;
				obj.clrUab_ = this.clrUab_;
				obj.clrUbc_ = this.clrUbc_;
				obj.clrUca_ = this.clrUca_;
				return obj;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in PQPGraphBottomSettings::Clone ");
				throw;
			}
		}

		#endregion
	}
}
