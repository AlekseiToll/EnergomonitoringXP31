using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using WeifenLuo.WinFormsUI;
using ZedGraph;
using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmGraphBox.
	/// </summary>
	public class frmDocAVGGraphBottom : DockContent
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public ToolStrip zedToolStrip;
		public ToolStripButton tsYGridLine;
		public ToolStripButton tsYMinorGridLine;
		public ToolStripButton tsXGridLine;
		public ToolStripButton tsXMinorGridLine;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		public ToolStripButton tsY2GridLine;
		public ToolStripButton tsY2MinorGridLine;
		private ToolStripSeparator toolStripSeparator3;
		public ToolStripButton tsUseRandomColors;
		private ToolStripSeparator toolStripSeparator4;
		public ToolStripButton tsClearAll;

		// таблица, которая открыта в данный момент в окне архива
		private DataGrid curDataGrid_ = null;
		private frmMain wndMain_;

		private EmDeviceType curDevType_;
		private int curPeriodInSecs_;
		private string curLanguage_;

		public ZedGraph.ZedGraphControl zedGraph;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripLabel tslFrom;
		private ToolStripLabel tslTo;
		private ToolStripLabel tslDTTo;
		private ToolStripLabel tslDTSpace;
		public DateTimePicker dtpTo;
		private ToolStripButton tsbShow;
		private ToolStripButton tsSeparateGraphs;

		// здесь храним ссылки на формы с графиками, чтобы можно было их закрыть при закрытии архива
		private List<frmSeparatedGraphs> listGraphs_ = new List<frmSeparatedGraphs>();
		private ToolStripLabel tslDTFrom;
		public DateTimePicker dtpFrom;

		public EnergomonitoringXP.Graph.GraphColors GraphColors = new EnergomonitoringXP.Graph.GraphColors();

		public frmDocAVGGraphBottom(frmMain wndMain, EmDataSaver.Settings settings)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			wndMain_ = wndMain;

			dtpFrom.Format = DateTimePickerFormat.Custom;
			dtpFrom.CustomFormat = "dd.MM.yyyy - HH:mm:ss";

			dtpTo.Format   = DateTimePickerFormat.Custom;
			dtpTo.CustomFormat   = "dd.MM.yyyy - HH:mm:ss";

			curLanguage_ = settings.CurrentLanguage;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocAVGGraphBottom));
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
			this.tslFrom = new System.Windows.Forms.ToolStripLabel();
			this.tslDTFrom = new System.Windows.Forms.ToolStripLabel();
			this.tslDTSpace = new System.Windows.Forms.ToolStripLabel();
			this.tslTo = new System.Windows.Forms.ToolStripLabel();
			this.tslDTTo = new System.Windows.Forms.ToolStripLabel();
			this.tsbShow = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.tsUseRandomColors = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.tsClearAll = new System.Windows.Forms.ToolStripButton();
			this.tsSeparateGraphs = new System.Windows.Forms.ToolStripButton();
			this.zedGraph = new ZedGraph.ZedGraphControl();
			this.dtpTo = new System.Windows.Forms.DateTimePicker();
			this.dtpFrom = new System.Windows.Forms.DateTimePicker();
			this.zedToolStrip.SuspendLayout();
			this.SuspendLayout();
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
            this.toolStripSeparator5,
            this.tslFrom,
            this.tslDTSpace,
            this.tslDTFrom,
            this.tslTo,
            this.tslDTTo,
            this.tsbShow,
            this.toolStripSeparator3,
            this.tsUseRandomColors,
            this.toolStripSeparator4,
            this.tsClearAll,
            this.tsSeparateGraphs});
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
			// tslFrom
			// 
			resources.ApplyResources(this.tslFrom, "tslFrom");
			this.tslFrom.Name = "tslFrom";
			// 
			// tslDTFrom
			// 
			resources.ApplyResources(this.tslDTFrom, "tslDTFrom");
			this.tslDTFrom.Name = "tslDTFrom";
			// 
			// tslDTSpace
			// 
			resources.ApplyResources(this.tslDTSpace, "tslDTSpace");
			this.tslDTSpace.Name = "tslDTSpace";
			// 
			// tslTo
			// 
			resources.ApplyResources(this.tslTo, "tslTo");
			this.tslTo.Name = "tslTo";
			// 
			// tslDTTo
			// 
			resources.ApplyResources(this.tslDTTo, "tslDTTo");
			this.tslDTTo.Name = "tslDTTo";
			// 
			// tsbShow
			// 
			resources.ApplyResources(this.tsbShow, "tsbShow");
			this.tsbShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbShow.Name = "tsbShow";
			this.tsbShow.Click += new System.EventHandler(this.tsbShow_Click);
			// 
			// toolStripSeparator3
			// 
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			// 
			// tsUseRandomColors
			// 
			resources.ApplyResources(this.tsUseRandomColors, "tsUseRandomColors");
			this.tsUseRandomColors.Checked = true;
			this.tsUseRandomColors.CheckOnClick = true;
			this.tsUseRandomColors.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsUseRandomColors.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsUseRandomColors.Name = "tsUseRandomColors";
			// 
			// toolStripSeparator4
			// 
			resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			// 
			// tsClearAll
			// 
			resources.ApplyResources(this.tsClearAll, "tsClearAll");
			this.tsClearAll.Name = "tsClearAll";
			this.tsClearAll.Click += new System.EventHandler(this.tsClearAll_Click);
			// 
			// tsSeparateGraphs
			// 
			resources.ApplyResources(this.tsSeparateGraphs, "tsSeparateGraphs");
			this.tsSeparateGraphs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsSeparateGraphs.Name = "tsSeparateGraphs";
			this.tsSeparateGraphs.Click += new System.EventHandler(this.tsSeparateGraphs_Click);
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
			// dtpTo
			// 
			resources.ApplyResources(this.dtpTo, "dtpTo");
			this.dtpTo.Name = "dtpTo";
			this.dtpTo.ShowUpDown = true;
			// 
			// dtpFrom
			// 
			resources.ApplyResources(this.dtpFrom, "dtpFrom");
			this.dtpFrom.Name = "dtpFrom";
			this.dtpFrom.ShowUpDown = true;
			// 
			// frmDocAVGGraphBottom
			// 
			resources.ApplyResources(this, "$this");
			this.AutoHidePortion = 0.2D;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CloseButton = false;
			this.Controls.Add(this.dtpFrom);
			this.Controls.Add(this.dtpTo);
			this.Controls.Add(this.zedGraph);
			this.Controls.Add(this.zedToolStrip);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "frmDocAVGGraphBottom";
			this.Load += new System.EventHandler(this.frmGraphBox_Load);
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

		private void tsClearAll_Click(object sender, EventArgs e)
		{
			try
			{
				wndMain_.wndDocAVG.wndDocAVGMain.ClearGraphParamList();

				this.zedGraph.GraphPane.CurveList.Clear();
				this.zedGraph.Refresh();

				// disposing buttons
				for (int i = zedToolStrip.Items.Count - 1; zedToolStrip.Items[i].Name != "tsSeparateGraphs"; i--)
				//for (int i = zedToolStrip.Items.Count - 1; zedToolStrip.Items[i].Name != "tsClearAll"; i--)
				{
					zedToolStrip.Items.RemoveAt(i);
				}
				// releasing colors
				GraphColors.ReleaseColors();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsClearAll_Click(): ");
				throw;
			}
		}

		private void frmGraphBox_Load(object sender, EventArgs e)
		{
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

		private void tsbShow_Click(object sender, EventArgs e)
		{
			try
			{
				if (dtpFrom.Value > dtpTo.Value)
				{
					EmService.WriteToLogFailed(
						"Error tsbShow_Click(): Start Date is larger than End Date");
					MessageBoxes.ErrorStartDateMoreThanEndDate(this);
					return;
				}

				double x1 = (double)new XDate(dtpFrom.Value);
				double x2 = (double)new XDate(dtpTo.Value);

				GraphPane pane = zedGraph.MasterPane.PaneList[0];

				ZoomState oldState = pane.GetZoomStack().Push(pane, ZoomState.StateType.Zoom);

				pane.XAxis.Min = x1;
				pane.XAxis.Max = x2;

				zedGraph.SetScroll(zedGraph.hScrollBar1, pane.XAxis, 0, 0);

				// Provide Callback to notify the user of zoom events
				//if (zedGraph.ZoomEvent != null)
				//	zedGraph.ZoomEvent(zedGraph, oldState, new ZoomState(pane, ZoomState.StateType.Zoom));

				Graphics g = zedGraph.CreateGraphics();
				pane.AxisChange(g);
				g.Dispose();
				zedGraph.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsbShow_Click(): ");
				throw;
			}
		}

		private void tsSeparateGraphs_Click(object sender, EventArgs e)
		{
			try
			{
				if (curDataGrid_ == null)
				{
					EmService.WriteToLogFailed("tsSeparateGraphs_Click: curDataGrid_ = null");
					return;
				}

				List<string> listParams = new List<string>();
				for (int iCol = 0; iCol < curDataGrid_.TableStyles[0].GridColumnStyles.Count; ++iCol)
				{
					listParams.Add(curDataGrid_.TableStyles[0].GridColumnStyles[iCol].MappingName);
				}

				//List<string> listParams = new List<string>();
				//if (curDataGrid_.Name.Equals("dgCurrentsVoltages"))
				//{
				//    listParams.Add("Ua");
				//    listParams.Add("Ub");
				//    listParams.Add("Uc");
				//}
				//else if (curDataGrid_.Name.Equals("dgPQP"))
				//{
				//    listParams.Add("U1");
				//    listParams.Add("U2");
				//    listParams.Add("U0");
				//}
				//else
				//{
				//    EmService.WriteToLogFailed("tsSeparateGraphs_Click: curDataGrid_ = " + curDataGrid_.Name);
				//    return;
				//}
				if (!curDataGrid_.Name.Equals("dgCurrentsVoltages") && !curDataGrid_.Name.Equals("dgPQP"))
				{
					EmService.WriteToLogFailed("tsSeparateGraphs_Click: curDataGrid_ = " + curDataGrid_.Name);
					return;
				}

				listGraphs_.Add(new frmSeparatedGraphs(listParams, ref curDataGrid_, curPeriodInSecs_, curDevType_, curLanguage_, dtpFrom.MinDate, dtpFrom.MaxDate));
				listGraphs_[listGraphs_.Count - 1].Show();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsSeparateGraphs_Click(): ");
				//throw;
			}
		}

		public void CloseGraphsForms()
		{
			try
			{
				if (listGraphs_ == null || listGraphs_.Count == 0) return;

				for (int iGraph = 0; iGraph < listGraphs_.Count; ++iGraph)
				{
					listGraphs_[iGraph].Hide();
					listGraphs_[iGraph].Dispose();
				}
				listGraphs_.Clear();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CloseGraphsForms(): ");
				//throw;
			}
		}

		public DataGrid CurDataGrid
		{
			get { return curDataGrid_; }
			set 
			{ 
				curDataGrid_ = value;

				if (curDataGrid_ != null)
				{
					if (curDataGrid_.Name.Equals("dgCurrentsVoltages") ||
						curDataGrid_.Name.Equals("dgPQP"))
					{
						tsSeparateGraphs.Enabled = true;
					}
					else
					{
						tsSeparateGraphs.Enabled = false;
					}
				}
				else tsSeparateGraphs.Enabled = false;
			}
		}

		public EmDeviceType CurDevType
		{
			//get { return curDevType_; }
			set { curDevType_ = value; }
		}

		public int CurPeriodInSecs
		{
			//get { return curPeriodInSecs_; }
			set { curPeriodInSecs_ = value; }
		}
	}
}
