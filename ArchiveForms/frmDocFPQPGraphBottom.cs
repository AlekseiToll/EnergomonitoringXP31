using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;
using System.Data;
using ZedGraph;
using DataGridColumnStyles;
using System.Collections.Generic;
using System.Globalization;

using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmGraphBox.
	/// </summary>
	public class frmDocFPQPGraphBottom : DockContentGraphMethods
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// <summary>
		// Settings object
		// </summary>
		//private EmDataSaver.Settings settings_;
		
		/// <summary>
		/// Pointer to the main application window
		/// </summary>
		private frmMain MainWindow_;
		private ToolStrip zedToolStrip;
		private ToolStripButton tsYGridLine;
		private ToolStripButton tsYMinorGridLine;
		private ToolStripButton tsXGridLine;
		private ToolStripButton tsXMinorGridLine;
		private ToolStripButton tsY2GridLine;
		private ToolStripButton tsY2MinorGridLine;
		public ZedGraphControl zedGraph;

		public EnergomonitoringXP.Graph.GraphColors GraphColors = 
			new EnergomonitoringXP.Graph.GraphColors();

		// <summary>
		// Synchronize settings
		// </summary>
		// <param name="NewSettings">Object to synchronize with</param>
		//public void SyncronizeSettings(EmDataSaver.Settings newSettings)
		//{
		//    settings_ = newSettings.Clone();
		//}

		public frmDocFPQPGraphBottom(frmMain MainWindow, EmDataSaver.Settings settings)
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

			//if (settings_.CurrentLanguage.Equals("ru"))
			//{
			//    zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
			//}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocFPQPGraphBottom));
			this.zedToolStrip = new System.Windows.Forms.ToolStrip();
			this.tsYGridLine = new System.Windows.Forms.ToolStripButton();
			this.tsYMinorGridLine = new System.Windows.Forms.ToolStripButton();
			this.tsXGridLine = new System.Windows.Forms.ToolStripButton();
			this.tsXMinorGridLine = new System.Windows.Forms.ToolStripButton();
			this.tsY2GridLine = new System.Windows.Forms.ToolStripButton();
			this.tsY2MinorGridLine = new System.Windows.Forms.ToolStripButton();
			this.zedGraph = new ZedGraph.ZedGraphControl();
			this.zedToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// zedToolStrip
			// 
			this.zedToolStrip.AccessibleDescription = null;
			this.zedToolStrip.AccessibleName = null;
			resources.ApplyResources(this.zedToolStrip, "zedToolStrip");
			this.zedToolStrip.BackgroundImage = null;
			this.zedToolStrip.Font = null;
			this.zedToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsYGridLine,
            this.tsYMinorGridLine,
            this.tsXGridLine,
            this.tsXMinorGridLine,
            this.tsY2GridLine,
            this.tsY2MinorGridLine});
			this.zedToolStrip.Name = "zedToolStrip";
			this.zedToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// tsYGridLine
			// 
			this.tsYGridLine.AccessibleDescription = null;
			this.tsYGridLine.AccessibleName = null;
			resources.ApplyResources(this.tsYGridLine, "tsYGridLine");
			this.tsYGridLine.BackgroundImage = null;
			this.tsYGridLine.Checked = true;
			this.tsYGridLine.CheckOnClick = true;
			this.tsYGridLine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsYGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsYGridLine.Name = "tsYGridLine";
			this.tsYGridLine.Click += new System.EventHandler(this.tsYGridLine_Click);
			// 
			// tsYMinorGridLine
			// 
			this.tsYMinorGridLine.AccessibleDescription = null;
			this.tsYMinorGridLine.AccessibleName = null;
			resources.ApplyResources(this.tsYMinorGridLine, "tsYMinorGridLine");
			this.tsYMinorGridLine.BackgroundImage = null;
			this.tsYMinorGridLine.CheckOnClick = true;
			this.tsYMinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsYMinorGridLine.Name = "tsYMinorGridLine";
			this.tsYMinorGridLine.Click += new System.EventHandler(this.tsYMinorGridLine_Click);
			// 
			// tsXGridLine
			// 
			this.tsXGridLine.AccessibleDescription = null;
			this.tsXGridLine.AccessibleName = null;
			resources.ApplyResources(this.tsXGridLine, "tsXGridLine");
			this.tsXGridLine.BackgroundImage = null;
			this.tsXGridLine.Checked = true;
			this.tsXGridLine.CheckOnClick = true;
			this.tsXGridLine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsXGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsXGridLine.Name = "tsXGridLine";
			this.tsXGridLine.Click += new System.EventHandler(this.tsXGridLine_Click);
			// 
			// tsXMinorGridLine
			// 
			this.tsXMinorGridLine.AccessibleDescription = null;
			this.tsXMinorGridLine.AccessibleName = null;
			resources.ApplyResources(this.tsXMinorGridLine, "tsXMinorGridLine");
			this.tsXMinorGridLine.BackgroundImage = null;
			this.tsXMinorGridLine.CheckOnClick = true;
			this.tsXMinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsXMinorGridLine.Name = "tsXMinorGridLine";
			this.tsXMinorGridLine.Click += new System.EventHandler(this.tsXMinorGridLine_Click);
			// 
			// tsY2GridLine
			// 
			this.tsY2GridLine.AccessibleDescription = null;
			this.tsY2GridLine.AccessibleName = null;
			resources.ApplyResources(this.tsY2GridLine, "tsY2GridLine");
			this.tsY2GridLine.BackgroundImage = null;
			this.tsY2GridLine.CheckOnClick = true;
			this.tsY2GridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsY2GridLine.Name = "tsY2GridLine";
			this.tsY2GridLine.Click += new System.EventHandler(this.tsY2GridLine_Click);
			// 
			// tsY2MinorGridLine
			// 
			this.tsY2MinorGridLine.AccessibleDescription = null;
			this.tsY2MinorGridLine.AccessibleName = null;
			resources.ApplyResources(this.tsY2MinorGridLine, "tsY2MinorGridLine");
			this.tsY2MinorGridLine.BackgroundImage = null;
			this.tsY2MinorGridLine.CheckOnClick = true;
			this.tsY2MinorGridLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsY2MinorGridLine.Name = "tsY2MinorGridLine";
			this.tsY2MinorGridLine.Click += new System.EventHandler(this.tsY2MinorGridLine_Click);
			// 
			// zedGraph
			// 
			this.zedGraph.AccessibleDescription = null;
			this.zedGraph.AccessibleName = null;
			resources.ApplyResources(this.zedGraph, "zedGraph");
			this.zedGraph.BackgroundImage = null;
			this.zedGraph.CultureInfo = new System.Globalization.CultureInfo("");
			this.zedGraph.Font = null;
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
			this.zedGraph.ScrollMaxX = 0;
			this.zedGraph.ScrollMaxY = 0;
			this.zedGraph.ScrollMaxY2 = 0;
			this.zedGraph.ScrollMinX = 0;
			this.zedGraph.ScrollMinY = 0;
			this.zedGraph.ScrollMinY2 = 0;
			this.zedGraph.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraph.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraph.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraph.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraph.ZoomStepFraction = 0.1;
			// 
			// frmDocFPQPGraphBottom
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			this.AutoHidePortion = 0.2;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.CloseButton = false;
			this.Controls.Add(this.zedGraph);
			this.Controls.Add(this.zedToolStrip);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.Font = null;
			this.HideOnClose = true;
			this.Name = "frmDocFPQPGraphBottom";
			this.ToolTipText = null;
			this.zedToolStrip.ResumeLayout(false);
			this.zedToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		public void ClearGraphs()
		{
			try
			{
				this.zedGraph.GraphPane.CurveList.Clear();
				this.zedGraph.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ClearGraphs(): ");
				throw;
			}
		}

		public void AddCurver(int column, Color curveColor)
		{
			try
			{
				zedGraph.GraphPane.CurveList.Clear();
				zedGraph.Refresh();

				DataGrid dg = MainWindow_.wndDocPQP.wndDocPQPMain.dgFValues;
				//int timeIntervalInSeconds = 20;

				if ((dg.DataSource as DataSet).Tables[0].Rows.Count < 0)
				{
					return;
				}

				// saving sort state and resorting to build graph correctly
				string SortRule = GetCurrentSortRule(dg);
				SortByColumn(dg, 0, false);

				GraphPane gPane = zedGraph.GraphPane;
				PointPairList list = new PointPairList();

				//int period_in_secs = 20;

				double _y = Conversions.object_2_double(dg[0, column]);
				//double _x0 = (double)new XDate(Convert.ToDateTime(dg[0, 0]).AddSeconds(-period_in_secs));
				double _x1 = (double)new XDate(Convert.ToDateTime(dg[0, 0]));
				//list.Add(_x0, _y);
				list.Add(_x1, _y);
				for (int i = 0; i < (dg.DataSource as DataSet).Tables[0].Rows.Count - 1; i++)
				{
					double y = Conversions.object_2_double(dg[i + 1, column]);
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
				//legend += " (L)";

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
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in flikker AddCurver(): frmDocFPQP");
				throw;
			}
		}

		private void tsY2GridLine_Click(object sender, EventArgs e)
		{
			try
			{
				zedGraph.GraphPane.Y2Axis.IsShowGrid = tsY2GridLine.Checked;
				zedGraph.GraphPane.Y2Axis.IsVisible = tsY2GridLine.Checked;
				tsY2MinorGridLine.Enabled = tsY2GridLine.Checked;
				zedGraph.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsY2GridLine_Click(): " + ex.Message);
				throw;
			}
		}

		private void tsY2MinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.Y2Axis.IsShowMinorGrid = tsY2MinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsXGridLine_Click(object sender, EventArgs e)
		{
			try
			{
				zedGraph.GraphPane.XAxis.Type = AxisType.Date;
				zedGraph.GraphPane.XAxis.IsShowGrid = tsXGridLine.Checked;
				zedGraph.GraphPane.XAxis.IsVisible = tsXGridLine.Checked;
				tsXMinorGridLine.Enabled = tsXGridLine.Checked;
				zedGraph.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsXGridLine_Click(): " + ex.Message);
				throw;
			}
		}

		private void tsXMinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.XAxis.IsShowMinorGrid = tsXMinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsYGridLine_Click(object sender, EventArgs e)
		{
			zedGraph.GraphPane.YAxis.IsShowGrid = tsYGridLine.Checked;
			zedGraph.GraphPane.YAxis.IsVisible = tsYGridLine.Checked;
			tsYMinorGridLine.Enabled = tsYGridLine.Checked;
			zedGraph.Invalidate();
		}

		private void tsYMinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.YAxis.IsShowMinorGrid = tsYMinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		/// <summary>Initialization</summary>
		public void init()
		{
			try
			{
				zedGraph.GraphPane.XAxis.Type = AxisType.Date;
				zedGraph.GraphPane.Title = " ";
				zedGraph.restoreScale();
				zedGraph.Refresh();

				tsYGridLine_Click(null, null);
				tsY2GridLine_Click(null, null);
				tsXGridLine_Click(null, null);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in frecuency init(): " + ex.Message);
				throw;
			}
		}
	}
}
