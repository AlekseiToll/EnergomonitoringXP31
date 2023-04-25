using System;
using System.Drawing;
using System.Collections;
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
	public class frmDocPQPUValGraphBottom_PQP_A : DockContentGraphMethods
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

		// флаг показывает, что линии НДП и ПДП уже отрисованы
		//private bool bNdpPdpPainted_ = false;
        private Panel panel2;
        private Button btnColorUcneg;
        private Button btnColorUbneg;
        private Button btnColorUaneg;
        private Button btnColorUcpos;
        private Button btnColorUbpos;
		private Button btnColorUapos;
        private CheckBox chbUCneg;
        private CheckBox chbUBneg;
        private CheckBox chbUAneg;
        private CheckBox chbUCpos;
        private CheckBox chbUBpos;
		private CheckBox chbUApos;
		public ZedGraphControl zedGraph;
		private CheckBox chbMakeNegativeGraphs;

		private PQPGraphBottomSettings_PQP_A graphSettings_ = null;

		public frmDocPQPUValGraphBottom_PQP_A(frmMain MainWindow)
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

				if (conScheme_ != ConnectScheme.Ph3W3 && conScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					chbUApos.Text = "U a+";
					chbUAneg.Text = "U a-";
					chbUBpos.Text = "U b+";
					chbUBneg.Text = "U b-";
					chbUCpos.Text = "U c+";
					chbUCneg.Text = "U c-";
				}
				else
				{
					chbUApos.Text = "U ab+";
					chbUAneg.Text = "U ab-";
					chbUBpos.Text = "U bc+";
					chbUBneg.Text = "U bc-";
					chbUCpos.Text = "U ca+";
					chbUCneg.Text = "U ca-";
				}

				switch (conScheme)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						chbUApos.Enabled = true;
						chbUBpos.Enabled = true;
						chbUCpos.Enabled = true;
						chbUAneg.Enabled = true;
						chbUBneg.Enabled = true;
						chbUCneg.Enabled = true;
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						chbUApos.Enabled = true;
						chbUBpos.Enabled = true;
						chbUCpos.Enabled = true;
						chbUAneg.Enabled = true;
						chbUBneg.Enabled = true;
						chbUCneg.Enabled = true;
						break;
					case ConnectScheme.Ph1W2:
						chbUApos.Enabled = true;
						chbUBpos.Enabled = false;
						chbUCpos.Enabled = false;
						chbUAneg.Enabled = true;
						chbUBneg.Enabled = false;
						chbUCneg.Enabled = false;
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

				chbUApos.Checked = false;
				chbUBpos.Checked = false;
				chbUCpos.Checked = false;
				chbUAneg.Checked = false;
				chbUBneg.Checked = false;
				chbUCneg.Checked = false;

				//bNdpPdpPainted_ = false;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocPQPUValGraphBottom_PQP_A));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.zedGraph = new ZedGraph.ZedGraphControl();
			this.panel2 = new System.Windows.Forms.Panel();
			this.btnColorUcneg = new System.Windows.Forms.Button();
			this.btnColorUbneg = new System.Windows.Forms.Button();
			this.btnColorUaneg = new System.Windows.Forms.Button();
			this.btnColorUcpos = new System.Windows.Forms.Button();
			this.btnColorUbpos = new System.Windows.Forms.Button();
			this.btnColorUapos = new System.Windows.Forms.Button();
			this.chbUCneg = new System.Windows.Forms.CheckBox();
			this.chbUBneg = new System.Windows.Forms.CheckBox();
			this.chbUAneg = new System.Windows.Forms.CheckBox();
			this.chbUCpos = new System.Windows.Forms.CheckBox();
			this.chbUBpos = new System.Windows.Forms.CheckBox();
			this.chbUApos = new System.Windows.Forms.CheckBox();
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
			this.chbMakeNegativeGraphs = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
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
			this.splitContainer1.Panel1.Controls.Add(this.zedGraph);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.panel2);
			// 
			// zedGraph
			// 
			this.zedGraph.CultureInfo = new System.Globalization.CultureInfo("");
			resources.ApplyResources(this.zedGraph, "zedGraph");
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
			// panel2
			// 
			resources.ApplyResources(this.panel2, "panel2");
			this.panel2.Controls.Add(this.btnColorUcneg);
			this.panel2.Controls.Add(this.btnColorUbneg);
			this.panel2.Controls.Add(this.btnColorUaneg);
			this.panel2.Controls.Add(this.btnColorUcpos);
			this.panel2.Controls.Add(this.btnColorUbpos);
			this.panel2.Controls.Add(this.btnColorUapos);
			this.panel2.Controls.Add(this.chbUCneg);
			this.panel2.Controls.Add(this.chbUBneg);
			this.panel2.Controls.Add(this.chbUAneg);
			this.panel2.Controls.Add(this.chbUCpos);
			this.panel2.Controls.Add(this.chbUBpos);
			this.panel2.Controls.Add(this.chbUApos);
			this.panel2.Name = "panel2";
			// 
			// btnColorUcneg
			// 
			this.btnColorUcneg.BackColor = System.Drawing.Color.Brown;
			resources.ApplyResources(this.btnColorUcneg, "btnColorUcneg");
			this.btnColorUcneg.Name = "btnColorUcneg";
			this.btnColorUcneg.UseVisualStyleBackColor = false;
			this.btnColorUcneg.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUbneg
			// 
			this.btnColorUbneg.BackColor = System.Drawing.Color.LimeGreen;
			resources.ApplyResources(this.btnColorUbneg, "btnColorUbneg");
			this.btnColorUbneg.Name = "btnColorUbneg";
			this.btnColorUbneg.UseVisualStyleBackColor = false;
			this.btnColorUbneg.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUaneg
			// 
			this.btnColorUaneg.BackColor = System.Drawing.Color.BlueViolet;
			resources.ApplyResources(this.btnColorUaneg, "btnColorUaneg");
			this.btnColorUaneg.Name = "btnColorUaneg";
			this.btnColorUaneg.UseVisualStyleBackColor = false;
			this.btnColorUaneg.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUcpos
			// 
			this.btnColorUcpos.BackColor = System.Drawing.Color.Red;
			resources.ApplyResources(this.btnColorUcpos, "btnColorUcpos");
			this.btnColorUcpos.Name = "btnColorUcpos";
			this.btnColorUcpos.UseVisualStyleBackColor = false;
			this.btnColorUcpos.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUbpos
			// 
			this.btnColorUbpos.BackColor = System.Drawing.Color.Green;
			resources.ApplyResources(this.btnColorUbpos, "btnColorUbpos");
			this.btnColorUbpos.Name = "btnColorUbpos";
			this.btnColorUbpos.UseVisualStyleBackColor = false;
			this.btnColorUbpos.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnColorUapos
			// 
			this.btnColorUapos.BackColor = System.Drawing.Color.DarkOrange;
			resources.ApplyResources(this.btnColorUapos, "btnColorUapos");
			this.btnColorUapos.Name = "btnColorUapos";
			this.btnColorUapos.UseVisualStyleBackColor = false;
			this.btnColorUapos.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// chbUCneg
			// 
			resources.ApplyResources(this.chbUCneg, "chbUCneg");
			this.chbUCneg.Name = "chbUCneg";
			this.chbUCneg.UseVisualStyleBackColor = true;
			this.chbUCneg.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUBneg
			// 
			resources.ApplyResources(this.chbUBneg, "chbUBneg");
			this.chbUBneg.Name = "chbUBneg";
			this.chbUBneg.UseVisualStyleBackColor = true;
			this.chbUBneg.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUAneg
			// 
			resources.ApplyResources(this.chbUAneg, "chbUAneg");
			this.chbUAneg.Name = "chbUAneg";
			this.chbUAneg.UseVisualStyleBackColor = true;
			this.chbUAneg.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUCpos
			// 
			resources.ApplyResources(this.chbUCpos, "chbUCpos");
			this.chbUCpos.Name = "chbUCpos";
			this.chbUCpos.UseVisualStyleBackColor = true;
			this.chbUCpos.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUBpos
			// 
			resources.ApplyResources(this.chbUBpos, "chbUBpos");
			this.chbUBpos.Name = "chbUBpos";
			this.chbUBpos.UseVisualStyleBackColor = true;
			this.chbUBpos.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// chbUApos
			// 
			resources.ApplyResources(this.chbUApos, "chbUApos");
			this.chbUApos.Name = "chbUApos";
			this.chbUApos.UseVisualStyleBackColor = true;
			this.chbUApos.CheckedChanged += new System.EventHandler(this.chbUx_CheckedChanged);
			// 
			// zedToolStrip
			// 
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
			resources.ApplyResources(this.zedToolStrip, "zedToolStrip");
			this.zedToolStrip.Name = "zedToolStrip";
			// 
			// tsYGridLine
			// 
			this.tsYGridLine.Checked = true;
			this.tsYGridLine.CheckOnClick = true;
			this.tsYGridLine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsYGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsYGridLine, "tsYGridLine");
			this.tsYGridLine.Name = "tsYGridLine";
			this.tsYGridLine.Click += new System.EventHandler(this.tsYGridLine_Click);
			// 
			// tsYMinorGridLine
			// 
			this.tsYMinorGridLine.CheckOnClick = true;
			this.tsYMinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsYMinorGridLine, "tsYMinorGridLine");
			this.tsYMinorGridLine.Name = "tsYMinorGridLine";
			this.tsYMinorGridLine.Click += new System.EventHandler(this.tsYMinorGridLine_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// tsXGridLine
			// 
			this.tsXGridLine.Checked = true;
			this.tsXGridLine.CheckOnClick = true;
			this.tsXGridLine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsXGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsXGridLine, "tsXGridLine");
			this.tsXGridLine.Name = "tsXGridLine";
			this.tsXGridLine.Click += new System.EventHandler(this.tsXGridLine_Click);
			// 
			// tsXMinorGridLine
			// 
			this.tsXMinorGridLine.CheckOnClick = true;
			this.tsXMinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsXMinorGridLine, "tsXMinorGridLine");
			this.tsXMinorGridLine.Name = "tsXMinorGridLine";
			this.tsXMinorGridLine.Click += new System.EventHandler(this.tsXMinorGridLine_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// tsY2GridLine
			// 
			this.tsY2GridLine.CheckOnClick = true;
			this.tsY2GridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsY2GridLine, "tsY2GridLine");
			this.tsY2GridLine.Name = "tsY2GridLine";
			this.tsY2GridLine.Click += new System.EventHandler(this.tsY2GridLine_Click);
			// 
			// tsY2MinorGridLine
			// 
			this.tsY2MinorGridLine.CheckOnClick = true;
			this.tsY2MinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsY2MinorGridLine, "tsY2MinorGridLine");
			this.tsY2MinorGridLine.Name = "tsY2MinorGridLine";
			this.tsY2MinorGridLine.Click += new System.EventHandler(this.tsY2MinorGridLine_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
			// 
			// chbMakeNegativeGraphs
			// 
			resources.ApplyResources(this.chbMakeNegativeGraphs, "chbMakeNegativeGraphs");
			this.chbMakeNegativeGraphs.Checked = true;
			this.chbMakeNegativeGraphs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbMakeNegativeGraphs.Name = "chbMakeNegativeGraphs";
			this.chbMakeNegativeGraphs.UseVisualStyleBackColor = true;
			// 
			// frmDocPQPUValGraphBottom_PQP_A
			// 
			this.AutoHidePortion = 0.2D;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.chbMakeNegativeGraphs);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.zedToolStrip);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "frmDocPQPUValGraphBottom_PQP_A";
			this.Load += new System.EventHandler(this.frmGraphBox_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
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
				graphSettings_ = new PQPGraphBottomSettings_PQP_A();
				graphSettings_.LoadSettings();
				btnColorUapos.BackColor = Color.FromArgb(graphSettings_.ColorUApos);
				btnColorUbpos.BackColor = Color.FromArgb(graphSettings_.ColorUBpos);
				btnColorUcpos.BackColor = Color.FromArgb(graphSettings_.ColorUCpos);
				btnColorUaneg.BackColor = Color.FromArgb(graphSettings_.ColorUAneg);
				btnColorUbneg.BackColor = Color.FromArgb(graphSettings_.ColorUBneg);
				btnColorUcneg.BackColor = Color.FromArgb(graphSettings_.ColorUCneg);

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
				graphSettings_.ColorUApos = btnColorUapos.BackColor.ToArgb();
				graphSettings_.ColorUBpos = btnColorUbpos.BackColor.ToArgb();
				graphSettings_.ColorUCpos = btnColorUcpos.BackColor.ToArgb();
				graphSettings_.ColorUAneg = btnColorUaneg.BackColor.ToArgb();
				graphSettings_.ColorUBneg = btnColorUbneg.BackColor.ToArgb();
				graphSettings_.ColorUCneg = btnColorUcneg.BackColor.ToArgb();
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
				bool bMakeNegativeGraphs = chbMakeNegativeGraphs.Checked;

				string colName = string.Empty;
				Button curBtn = btnColorUapos;
				if (sender == chbUApos)
				{
					colName = "u_a_ab_pos";
					curBtn = btnColorUapos;
					bMakeNegativeGraphs = false;  // для положительных графиков не надо
				}
				else if (sender == chbUBpos)
				{
					colName = "u_b_bc_pos";
					curBtn = btnColorUbpos;
					bMakeNegativeGraphs = false;
				}
				else if (sender == chbUCpos)
				{
					colName = "u_c_ca_pos";
					curBtn = btnColorUcpos;
					bMakeNegativeGraphs = false;
				}
				else if (sender == chbUAneg)
				{
					colName = "u_a_ab_neg";
					curBtn = btnColorUaneg;
				}
				else if (sender == chbUBneg)
				{
					colName = "u_b_bc_neg";
					curBtn = btnColorUbneg;
				}
				else if (sender == chbUCneg)
				{
					colName = "u_c_ca_neg";
					curBtn = btnColorUcneg;
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

					string legend = AddCurver(ref curDg, columnIndex, false, curveColor, bMakeNegativeGraphs);
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
		private string AddCurver(ref DataGrid dg, int column, bool Y2Axis, Color curveColor,
									bool makeNegativeGraphs)
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
				if (makeNegativeGraphs) _y *= -1;
				double _x1 = (double)new XDate(Convert.ToDateTime(dg[0, 0]));
				list.Add(_x1, _y);
				for (int i = 0; i < (dg.DataSource as DataSet).Tables[0].Rows.Count - 1; i++)
				{
					double y = Conversions.object_2_double((dg.DataSource as DataSet).Tables[0].Rows[i + 1][column]);
					if (makeNegativeGraphs) y *= -1;
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
	}

	public class PQPGraphBottomSettings_PQP_A
	{
		#region Fields

		/// <summary>EmSettings file name</summary>
		[NonSerialized]
		private string settingsFileName_ = "PQPGraphSettingsPQPA.config";

		[NonSerialized]
		private Color clrUApos_ = Color.DarkOrange;

		[NonSerialized]
		private Color clrUBpos_ = Color.Green;

		[NonSerialized]
		private Color clrUCpos_ = Color.Red;

		[NonSerialized]
		private Color clrUAneg_ = Color.BurlyWood;

		[NonSerialized]
		private Color clrUBneg_ = Color.LimeGreen;

		[NonSerialized]
		private Color clrUCneg_ = Color.Brown;

		#endregion

		#region Properties

		/// <summary>Gets settings file name</summary>
		public string SettingsFileName
		{
			get { return settingsFileName_; }
		}

		public int ColorUApos
		{
			get { return clrUApos_.ToArgb(); }
			set { clrUApos_ = Color.FromArgb(value); }
		}

		public int ColorUBpos
		{
			get { return clrUBpos_.ToArgb(); }
			set { clrUBpos_ = Color.FromArgb(value); }
		}

		public int ColorUCpos
		{
			get { return clrUCpos_.ToArgb(); }
			set { clrUCpos_ = Color.FromArgb(value); }
		}

		public int ColorUAneg
		{
			get { return clrUAneg_.ToArgb(); }
			set { clrUAneg_ = Color.FromArgb(value); }
		}

		public int ColorUBneg
		{
			get { return clrUBneg_.ToArgb(); }
			set { clrUBneg_ = Color.FromArgb(value); }
		}

		public int ColorUCneg
		{
			get { return clrUCneg_.ToArgb(); }
			set { clrUCneg_ = Color.FromArgb(value); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor with defaut settings
		/// </summary>
		public PQPGraphBottomSettings_PQP_A()
		{
			settingsFileName_ = EmService.AppDirectory + "PQPGraphSettingsPQPA.config";
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
				mySerializer = new XmlSerializer(typeof(PQPGraphBottomSettings_PQP_A));
				myWriter = new StreamWriter(settingsFileName_, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, this);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in PQPGraphBottomSettingsPQPA::SaveSettings(): ");
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
				xmlSer = new XmlSerializer(typeof(PQPGraphBottomSettings_PQP_A));
				FileInfo fi = new FileInfo(settingsFileName_);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					fs = fi.OpenRead();
					// Create a new instance of the ApplicationSettings by
					// deserializing the config file.
					PQPGraphBottomSettings_PQP_A myAppSettings = (PQPGraphBottomSettings_PQP_A)xmlSer.Deserialize(fs);
					// Assign the property values to this instance of 
					// the ApplicationSettings class.
					this.clrUApos_ = myAppSettings.clrUApos_;
					this.clrUBpos_ = myAppSettings.clrUBpos_;
					this.clrUCpos_ = myAppSettings.clrUCpos_;
					this.clrUAneg_ = myAppSettings.clrUAneg_;
					this.clrUBneg_ = myAppSettings.clrUBneg_;
					this.clrUCneg_ = myAppSettings.clrUCneg_;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in PQPGraphBottomSettingsPQPA::LoadSettings(): ");
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
		public PQPGraphBottomSettings_PQP_A Clone()
		{
			try
			{
				PQPGraphBottomSettings_PQP_A obj = new PQPGraphBottomSettings_PQP_A();
				obj.clrUApos_ = this.clrUApos_;
				obj.clrUBpos_ = this.clrUBpos_;
				obj.clrUCpos_ = this.clrUCpos_;
				obj.clrUAneg_ = this.clrUAneg_;
				obj.clrUBneg_ = this.clrUBneg_;
				obj.clrUCneg_ = this.clrUCneg_;
				return obj;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in PQPGraphBottomSettingsPQPA::Clone ");
				throw;
			}
		}

		#endregion
	}
}
