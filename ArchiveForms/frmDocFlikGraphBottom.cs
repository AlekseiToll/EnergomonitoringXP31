// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Graphs float-window form

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
	public class frmDocFlikGraphBottom : DockContentGraphMethods
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Settings object
		/// </summary>
		private EmDataSaver.Settings settings_;
		
		/// <summary>
		/// Pointer to the main application window
		/// </summary>
		private frmMain MainWindow_;
		private ToolStrip zedToolStrip;
		private ToolStripButton tsbPhaseA;
		private ToolStripButton tsbPhaseALong;
		private ToolStripButton tsbPhaseB;
		private ToolStripButton tsbPhaseBLong;
		private ToolStripButton tsbPhaseC;
		private ToolStripButton tsbPhaseCLong;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton tsYGridLine;
		private ToolStripButton tsYMinorGridLine;
		private ToolStripButton tsXGridLine;
		private ToolStripButton tsXMinorGridLine;
		private ToolStripButton tsY2GridLine;
		private ToolStripButton tsY2MinorGridLine;
		public ZedGraphControl zedGraph;

		// период фликера
		short t_fliker_;
		EmDeviceType devType_;

		public EnergomonitoringXP.Graph.GraphColors GraphColors = new EnergomonitoringXP.Graph.GraphColors();

		/// <summary>Synchronize settings</summary>
		/// <param name="newSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings newSettings)
		{
			settings_ = newSettings.Clone();
		}

		public frmDocFlikGraphBottom(frmMain MainWindow, EmDataSaver.Settings settings)
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

			if (settings_.CurrentLanguage.Equals("ru"))
			{
				zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
			}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocFlikGraphBottom));
			this.zedToolStrip = new System.Windows.Forms.ToolStrip();
			this.tsbPhaseA = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseALong = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseB = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseBLong = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseC = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseCLong = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
			resources.ApplyResources(this.zedToolStrip, "zedToolStrip");
			this.zedToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbPhaseA,
            this.tsbPhaseALong,
            this.tsbPhaseB,
            this.tsbPhaseBLong,
            this.tsbPhaseC,
            this.tsbPhaseCLong,
            this.toolStripSeparator1,
            this.tsYGridLine,
            this.tsYMinorGridLine,
            this.tsXGridLine,
            this.tsXMinorGridLine,
            this.tsY2GridLine,
            this.tsY2MinorGridLine});
			this.zedToolStrip.Name = "zedToolStrip";
			this.zedToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// tsbPhaseA
			// 
			resources.ApplyResources(this.tsbPhaseA, "tsbPhaseA");
			this.tsbPhaseA.Checked = true;
			this.tsbPhaseA.CheckOnClick = true;
			this.tsbPhaseA.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbPhaseA.Name = "tsbPhaseA";
			this.tsbPhaseA.Click += new System.EventHandler(this.tsbPhaseButton_Click);
			this.tsbPhaseA.Paint += new System.Windows.Forms.PaintEventHandler(this.tsbP_st_Paint);
			// 
			// tsbPhaseALong
			// 
			resources.ApplyResources(this.tsbPhaseALong, "tsbPhaseALong");
			this.tsbPhaseALong.Checked = true;
			this.tsbPhaseALong.CheckOnClick = true;
			this.tsbPhaseALong.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseALong.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbPhaseALong.Name = "tsbPhaseALong";
			this.tsbPhaseALong.Click += new System.EventHandler(this.tsbPhaseButton_Click);
			this.tsbPhaseALong.Paint += new System.Windows.Forms.PaintEventHandler(this.tsbP_lt_Paint);
			// 
			// tsbPhaseB
			// 
			resources.ApplyResources(this.tsbPhaseB, "tsbPhaseB");
			this.tsbPhaseB.Checked = true;
			this.tsbPhaseB.CheckOnClick = true;
			this.tsbPhaseB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbPhaseB.Name = "tsbPhaseB";
			this.tsbPhaseB.Click += new System.EventHandler(this.tsbPhaseButton_Click);
			this.tsbPhaseB.Paint += new System.Windows.Forms.PaintEventHandler(this.tsbP_st_Paint);
			// 
			// tsbPhaseBLong
			// 
			resources.ApplyResources(this.tsbPhaseBLong, "tsbPhaseBLong");
			this.tsbPhaseBLong.Checked = true;
			this.tsbPhaseBLong.CheckOnClick = true;
			this.tsbPhaseBLong.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseBLong.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbPhaseBLong.Name = "tsbPhaseBLong";
			this.tsbPhaseBLong.Click += new System.EventHandler(this.tsbPhaseButton_Click);
			this.tsbPhaseBLong.Paint += new System.Windows.Forms.PaintEventHandler(this.tsbP_lt_Paint);
			// 
			// tsbPhaseC
			// 
			resources.ApplyResources(this.tsbPhaseC, "tsbPhaseC");
			this.tsbPhaseC.Checked = true;
			this.tsbPhaseC.CheckOnClick = true;
			this.tsbPhaseC.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbPhaseC.Name = "tsbPhaseC";
			this.tsbPhaseC.Click += new System.EventHandler(this.tsbPhaseButton_Click);
			this.tsbPhaseC.Paint += new System.Windows.Forms.PaintEventHandler(this.tsbP_st_Paint);
			// 
			// tsbPhaseCLong
			// 
			resources.ApplyResources(this.tsbPhaseCLong, "tsbPhaseCLong");
			this.tsbPhaseCLong.Checked = true;
			this.tsbPhaseCLong.CheckOnClick = true;
			this.tsbPhaseCLong.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseCLong.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbPhaseCLong.Name = "tsbPhaseCLong";
			this.tsbPhaseCLong.Click += new System.EventHandler(this.tsbPhaseButton_Click);
			this.tsbPhaseCLong.Paint += new System.Windows.Forms.PaintEventHandler(this.tsbP_lt_Paint);
			// 
			// toolStripSeparator1
			// 
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			this.toolStripSeparator1.Name = "toolStripSeparator1";
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
			// tsY2GridLine
			// 
			resources.ApplyResources(this.tsY2GridLine, "tsY2GridLine");
			this.tsY2GridLine.Checked = true;
			this.tsY2GridLine.CheckOnClick = true;
			this.tsY2GridLine.CheckState = System.Windows.Forms.CheckState.Checked;
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
			// frmDocFlikGraphBottom
			// 
			resources.ApplyResources(this, "$this");
			this.AutoHidePortion = 0.2D;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.zedGraph);
			this.Controls.Add(this.zedToolStrip);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "frmDocFlikGraphBottom";
			this.zedToolStrip.ResumeLayout(false);
			this.zedToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void paintCurves(bool phaseA, bool phaseALong,
			bool phaseB, bool phaseBLong,
			bool phaseC, bool phaseCLong)
		{
			try
			{
				zedGraph.GraphPane.CurveList.Clear();
				zedGraph.Refresh();
				DataGrid dgFlicker = MainWindow_.wndDocPQP.wndDocPQPMain.dgFlicker;
				DataGrid dgFlickerLong = MainWindow_.wndDocPQP.wndDocPQPMain.dgFlickerLong;

				// вычисляем строку в таблице до которой график должен рисоваться
				// серым цветом
				short grayEnd = -1;
				TimeSpan timeBegin, curTime;
				TimeSpan diff = new TimeSpan(2, 0, 0);
				if (devType_ != EmDeviceType.EM32)
				{
					if (dgFlickerLong.CurrentRowIndex >= 0)
					{
						for (short i = 0;
							i < (dgFlickerLong.DataSource as DataSet).Tables[0].Rows.Count;
							++i)
						{
							if (TimeSpan.TryParse(dgFlickerLong[0, 0].ToString(), out timeBegin))
							{
								if (TimeSpan.TryParse(dgFlickerLong[i, 0].ToString(), out curTime))
								{
									if ((curTime - timeBegin) < diff)
									{
										grayEnd = i;
									}
									else
										break;
								}
							}
						}
					}
				}
				// для EM32 серый график не нужен
				else
					grayEnd = -1;

				// рисуем графики
				if (phaseA)
					AddCurver(dgFlicker, 1, -1, -1, Color.Orange, 1.0F, 0);
				if (phaseALong)
				{
					if (grayEnd == -1)
						AddCurver(dgFlickerLong, 1, -1, -1, Color.Orange, 2.0F, 1);
					else
					{
						AddCurver(dgFlickerLong, 1, 0, grayEnd + 1, Color.Gray, 2.0F, 1);
						AddCurver(dgFlickerLong, 1, grayEnd, -1, Color.Orange, 2.0F, 1);
					}
				}

				if (MainWindow_.wndDocPQP.wndDocPQPMain.CurConnectScheme != ConnectScheme.Ph1W2)
				{
					if (phaseB)
						AddCurver(dgFlicker, 2, -1, -1, Color.Green, 1.0F, 0);
					if (phaseBLong)
					{
						if (grayEnd == -1)
							AddCurver(dgFlickerLong, 2, -1, -1, Color.Green, 2.0F, 1);
						else
						{
							AddCurver(dgFlickerLong, 2, 0, grayEnd + 1, Color.Gray, 2.0F, 1);
							AddCurver(dgFlickerLong, 2, grayEnd, -1, Color.Green, 2.0F, 1);
						}
					}

					if (phaseC)
						AddCurver(dgFlicker, 3, -1, -1, Color.Red, 1.0F, 0);
					if (phaseCLong)
					{
						if (grayEnd == -1)
							AddCurver(dgFlickerLong, 3, -1, -1, Color.Red, 2.0F, 1);
						else
						{
							AddCurver(dgFlickerLong, 3, 0, grayEnd + 1, Color.Gray, 2.0F, 1);
							AddCurver(dgFlickerLong, 3, grayEnd, -1, Color.Red, 2.0F, 1);
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in flikker paintCurves(): " + ex.Message);
				throw;
			}
		}

		// если startRow и endRow == -1, то рисуем полный график
		// type = 0 - 10-минутные измерения фликера, type = 1 - 2-часовые измерения
		private void AddCurver(DataGrid dg, int column, int startRow, int endRow, 
								Color curveColor, float width, byte type)
		{
			try
			{
				if (dg.CurrentRowIndex < 0)
				{
					return;
				}
				//if (startRow < 0) return;

				if (startRow < 0) startRow = 0;
				if (endRow < 0) endRow = (dg.DataSource as DataSet).Tables[0].Rows.Count - 1;
				if (endRow >= (dg.DataSource as DataSet).Tables[0].Rows.Count)
					endRow = (dg.DataSource as DataSet).Tables[0].Rows.Count - 1;

				// saving sort state and resorting to build graph correctly
				string SortRule = GetCurrentSortRule(dg);
				SortByColumn(dg, 0, false);

				// starting build point list
				GraphPane gPane = zedGraph.GraphPane;
				PointPairList list = new PointPairList();

				DateTime dt1, dt2;
				double y, x0, x1;

				short cur_t_fliker = t_fliker_;
				if ((type == 1) && (devType_ == EmDeviceType.EM32 || devType_ == EmDeviceType.ETPQP || 
					devType_ == EmDeviceType.ETPQP_A)) 
					cur_t_fliker = 120;

				dt1 = ExtractTimeFromString(dg[startRow, 0].ToString(), 0);
				dt2 = ExtractTimeFromString(dg[startRow, 0].ToString(), 1);
				dt1 = dt1.AddMinutes(-cur_t_fliker);
				//dt2 = dt2.AddMinutes(-cur_t_fliker);
				short rule = -1;
				if (dg[startRow, column].ToString() != " " && dg[startRow, column].ToString() != "-")
				{
					try
					{
						if (rule != -1)
							y = GetFlikkerValue(dg[startRow, column], rule);
						else
						{
							y = Convert.ToSingle(dg[startRow, column]);
							rule = 0;
						}
					}
					catch (FormatException)
					{
						y = Single.Parse(dg[startRow, column].ToString(), new CultureInfo("en-US"));
						rule = 1;
					}

					//x0 = (double)new XDate(Convert.ToDateTime(dg[startRow, 0]).AddMinutes(-t_fliker_));
					//x1 = (double)new XDate(Convert.ToDateTime(dg[startRow, 0]));
					x0 = (double)new XDate(dt1);
					x1 = (double)new XDate(dt2);
					//list.Add(x0, y);
					list.Add(x1, y);
				}
				double tmp_prev_value = Double.NaN;
				for (int i = startRow; i < endRow; i++)
				{
					//x0 = (double)new XDate(Convert.ToDateTime(dg[i, 0]));
					dt1 = dt2;
					dt2 = dt2.AddMinutes(cur_t_fliker);
					x0 = (double)new XDate(dt1);
					//x1 = (double)new XDate(Convert.ToDateTime(dg[i + 1, 0]));
					x1 = (double)new XDate(dt2);

					if (dg[i + 1, column].ToString() != " " && dg[i + 1, column].ToString() != "-")
					{
						try
						{
							if (rule != -1)
								y = GetFlikkerValue(dg[i + 1, column], rule);
							else
							{
								y = Convert.ToSingle(dg[i + 1, column]);
								rule = 0;
							}
						}
						catch (FormatException)
						{
							y = Single.Parse(dg[i + 1, column].ToString(), new CultureInfo("en-US"));
							rule = 1;
						}

						//list.Add(x0, y);
						list.Add(x1, y);
						tmp_prev_value = y;
					}
					else
					{
						if (!Double.IsNaN(tmp_prev_value))
						{
							//list.Add(x0, tmp_prev_value);
							list.Add(x1, tmp_prev_value);
						}
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

				CurveItem myCurve = gPane.AddCurve(legend, list, curveColor,
					SymbolType.None, width);

				myCurve.IsY2Axis = false;

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

				//ToolStripItem newItem = zedToolStrip.Items.Add(img);
				//newItem.ToolTipText = legend;
				//newItem.Click += new EventHandler(newItem_Click);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in flikker AddCurver(): frmDocFlik");
				throw;
			}
		}

		/// <summary>
		/// Extract time from fliker string
		/// </summary>
		/// <param name="str"></param>
		/// <param name="type">0 - start time, 1 - end time</param>
		/// <returns></returns>
		private DateTime ExtractTimeFromString(string str, int type)
		{
			try
			{
				int i = 0;
				if (type == 1)
				{
					for (i = str.Length - 1; i >= 0; --i)
					{
						if ((!Char.IsDigit(str[i])) && (str[i] != ':')) break;
					}
					return Convert.ToDateTime(str.Substring(i + 1));
				}
				else
				{
					for (i = 0; i < str.Length; ++i)
					{
						if ((!Char.IsDigit(str[i])) && (str[i] != ':')) break;
					}
					return Convert.ToDateTime(str.Substring(0, i));
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in flikker ExtractTimeFromString(): ");
				throw;
			}
		}

		private double GetFlikkerValue(object s, short rule)
		{
			try
			{
				if (rule == 0)
				{
					try
					{
						return Convert.ToSingle(s, new CultureInfo("en-US"));
					}
					catch { return Convert.ToSingle(s, new CultureInfo("ru-RU")); }
				}
				else
					return Single.Parse(s.ToString(), new CultureInfo("en-US"));
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetFlikkerValue(): ");
				throw;
			}
		}

		private void tsbPhaseButton_Click(object sender, EventArgs e)
		{
			paintCurves(tsbPhaseA.Checked, tsbPhaseALong.Checked,
				tsbPhaseB.Checked, tsbPhaseBLong.Checked,
				tsbPhaseC.Checked, tsbPhaseCLong.Checked);
		}

		private void tsY2GridLine_Click(object sender, EventArgs e)
		{
			zedGraph.GraphPane.Y2Axis.IsShowGrid = tsY2GridLine.Checked;
			zedGraph.GraphPane.Y2Axis.IsVisible = tsY2GridLine.Checked;
			tsY2MinorGridLine.Enabled = tsY2GridLine.Checked;
			zedGraph.Invalidate();
		}

		private void tsY2MinorGridLine_Click(object sender, EventArgs e)
		{
			this.zedGraph.GraphPane.Y2Axis.IsShowMinorGrid = tsY2MinorGridLine.Checked;
			this.zedGraph.Invalidate();
		}

		private void tsXGridLine_Click(object sender, EventArgs e)
		{
			zedGraph.GraphPane.XAxis.Type = AxisType.Date;
			zedGraph.GraphPane.XAxis.IsShowGrid = tsXGridLine.Checked;
			zedGraph.GraphPane.XAxis.IsVisible = tsXGridLine.Checked;
			tsXMinorGridLine.Enabled = tsXGridLine.Checked;
			zedGraph.Invalidate();
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

		private void paintToolBtn(Graphics g, ToolStripButton tsb, Brush foreBrush, string caption)
		{
			try
			{
				string[] strArrColumnHeaderFormula = caption.Split(new char[1] { '_' });
				// drawing column header
				if (strArrColumnHeaderFormula == null)
					return;

				Font headerRegularFont = new Font(FontFamily.GenericSansSerif, (float)7.75);
				Font headerSubscriptFont = new Font(FontFamily.GenericSansSerif, 7);
				Point pointDrawingZero = new Point(1, 1);

				for (int i = 0; i < strArrColumnHeaderFormula.Length; i++)
				{
					SizeF size = new SizeF(g.MeasureString(strArrColumnHeaderFormula[i],
						headerRegularFont));
					// check for the string width
					if (pointDrawingZero.X + size.Width > tsb.Bounds.Right)
						break;

					g.DrawString(strArrColumnHeaderFormula[i], headerRegularFont, foreBrush, pointDrawingZero);
					pointDrawingZero.X += (int)size.Width - 2;

					if (strArrColumnHeaderFormula.Length > ++i)
					{
						size = new SizeF(g.MeasureString(strArrColumnHeaderFormula[i], headerSubscriptFont));
						// check for the string width
						if (pointDrawingZero.X + size.Width > tsb.Bounds.Right)
							break;
						g.DrawString(strArrColumnHeaderFormula[i], headerSubscriptFont, foreBrush, pointDrawingZero.X, pointDrawingZero.Y + size.Height - 8);
						pointDrawingZero.X += (int)size.Width - 2;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in flikker paintToolBtn(): " + ex.Message);
				throw;
			}
		}

		private void tsbP_st_Paint(object sender, PaintEventArgs e)
		{
			paintToolBtn(e.Graphics, (ToolStripButton)sender, new SolidBrush(Color.Black), "P_st_");
		}

		private void tsbP_lt_Paint(object sender, PaintEventArgs e)
		{
			paintToolBtn(e.Graphics, (ToolStripButton)sender, new SolidBrush(Color.Black), "P_lt_");
		}

		/// <summary>Initialization</summary>
		public void init(short t_fliker, EmDeviceType devType)
		{
			try
			{
				t_fliker_ = t_fliker;
				devType_ = devType;

				tsbPhaseA.Checked = true;
				tsbPhaseALong.Checked = true;
				tsbPhaseB.Checked = true;
				tsbPhaseBLong.Checked = true;
				tsbPhaseC.Checked = true;
				tsbPhaseCLong.Checked = true;

				// paint graph for fliker (all phases)
				paintCurves(true, true, true, true, true, true);
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
				EmService.WriteToLogFailed("Error in flikker init(): " + ex.Message);
				throw;
			}
		}
	}
}
