using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using EmServiceLib;
using ZedGraph;

namespace EnergomonitoringXP
{
	public partial class frmSeparatedGraphs : DockContentGraphMethods
	{
		// значения, которые можно выбрать для отображения на графике
		private List<Pair<string, string>> u_params_valid_ = new List<Pair<string, string>>();
		private List<Pair<string, string>> cur_u_params_ = new List<Pair<string, string>>();
		private List<CheckBox> listChbParams_ = new List<CheckBox>();
		private DataGrid curDataGrid_;
		private int period_in_secs_;
		private EmDeviceType curDevType_ = EmDeviceType.NONE;
		private string curLanguage_;
		private List<ZedGraphControl> listZedGraphControls_ = new List<ZedGraphControl>();

		public frmSeparatedGraphs(List<string> u_params, ref DataGrid dg, int period_in_secs, EmDeviceType devType, string curLanguage, DateTime dtMin, DateTime dtMax)
		{
			try
			{
				InitializeComponent();

				listZedGraphControls_.Add(zedGraphControl1);
				listZedGraphControls_.Add(zedGraphControl2);
				listZedGraphControls_.Add(zedGraphControl3);
				listZedGraphControls_.Add(zedGraphControl4);

				curDataGrid_ = dg;
				period_in_secs_ = period_in_secs;
				curDevType_ = devType;
				curLanguage_ = curLanguage;

				#region params list

				u_params_valid_.Add(new Pair<string, string>("u_1", "U_1"));
				u_params_valid_.Add(new Pair<string, string>("u_2", "U_2"));
				u_params_valid_.Add(new Pair<string, string>("u_0", "U_0"));
				u_params_valid_.Add(new Pair<string, string>("i_1", "I_1"));
				u_params_valid_.Add(new Pair<string, string>("i_2", "I_2"));
				u_params_valid_.Add(new Pair<string, string>("i_0", "I_0"));
				u_params_valid_.Add(new Pair<string, string>("d_u_pos_a", "dU_A+"));
				u_params_valid_.Add(new Pair<string, string>("d_u_pos_b", "dU_B+"));
				u_params_valid_.Add(new Pair<string, string>("d_u_pos_c", "dU_C+"));
				u_params_valid_.Add(new Pair<string, string>("d_u_pos_ab", "dU_AB+"));
				u_params_valid_.Add(new Pair<string, string>("d_u_pos_bc", "dU_BC+"));
				u_params_valid_.Add(new Pair<string, string>("d_u_pos_ca", "dU_CA+"));
				u_params_valid_.Add(new Pair<string, string>("d_u_neg_a", "dU_A-"));
				u_params_valid_.Add(new Pair<string, string>("d_u_neg_b", "dU_B-"));
				u_params_valid_.Add(new Pair<string, string>("d_u_neg_c", "dU_C-"));
				u_params_valid_.Add(new Pair<string, string>("d_u_neg_ab", "dU_AB-"));
				u_params_valid_.Add(new Pair<string, string>("d_u_neg_bc", "dU_BC-"));
				u_params_valid_.Add(new Pair<string, string>("d_u_neg_ca", "dU_CA-"));
				u_params_valid_.Add(new Pair<string, string>("u_a", "U_A"));
				u_params_valid_.Add(new Pair<string, string>("u_b", "U_B"));
				u_params_valid_.Add(new Pair<string, string>("u_c", "U_C"));
				u_params_valid_.Add(new Pair<string, string>("u_ab", "U_AB"));
				u_params_valid_.Add(new Pair<string, string>("u_bc", "U_BC"));
				u_params_valid_.Add(new Pair<string, string>("u_ca", "U_CA"));
				u_params_valid_.Add(new Pair<string, string>("i_a", "I_A"));
				u_params_valid_.Add(new Pair<string, string>("i_b", "I_B"));
				u_params_valid_.Add(new Pair<string, string>("i_c", "I_C"));
				u_params_valid_.Add(new Pair<string, string>("i_n", "I_N"));

				u_params_valid_.Add(new Pair<string, string>("d_u_y", "dUy"));
				u_params_valid_.Add(new Pair<string, string>("d_u_a", "dU_A"));
				u_params_valid_.Add(new Pair<string, string>("d_u_b", "dU_B"));
				u_params_valid_.Add(new Pair<string, string>("d_u_c", "dU_C"));
				u_params_valid_.Add(new Pair<string, string>("d_u_ab", "dU_AB"));
				u_params_valid_.Add(new Pair<string, string>("d_u_bc", "dU_BC"));
				u_params_valid_.Add(new Pair<string, string>("d_u_ca", "dU_CA"));

				#endregion

				// из полученных параметров выбираем валидные (т.е. графики будем строить не для всех, а только для самых нужных)
				for (int iParam = 0; iParam < u_params.Count; ++iParam)
				{
					Pair<string, string> curPair = u_params_valid_.Find(x => x.First.Equals(u_params[iParam]));
					if (curPair != null) cur_u_params_.Add(curPair);
				}

				dtpFrom.Format = DateTimePickerFormat.Custom;
				dtpFrom.CustomFormat = "dd.MM.yyyy - HH:mm:ss";
				dtpFrom.MinDate = dtMin;
				dtpFrom.MaxDate = dtMax;
				dtpFrom.Value = dtMin;

				dtpTo.Format = DateTimePickerFormat.Custom;
				dtpTo.CustomFormat = "dd.MM.yyyy - HH:mm:ss";
				dtpTo.MinDate = dtMin;
				dtpTo.MaxDate = dtMax;
				dtpTo.Value = dtMax;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmSeparatedGraphs::frmSeparatedGraphs(): ");
				throw;
			}
		}

		private void frmSeparatedGraphs_Load(object sender, EventArgs e)
		{
			try
			{
				#region Initializing zedGraph objects

				if (curLanguage_.Equals("ru"))
				{
					zedGraphControl1.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
					zedGraphControl2.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
					zedGraphControl3.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
					zedGraphControl4.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				}

				for (int iGraph = 0; iGraph < listZedGraphControls_.Count; ++iGraph)
				{
					ZedGraphControl curZedGraph = listZedGraphControls_[iGraph];

					curZedGraph.BackColor = Color.Black;
					GraphPane gPane = curZedGraph.GraphPane;
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
				}

				#endregion

				if (cur_u_params_ == null) return;

				int x = 10, y = 10;
				for (int iParam = 0; iParam < cur_u_params_.Count; ++iParam)
				{
					listChbParams_.Add(new CheckBox());
					this.panelCheck.Controls.Add(listChbParams_[listChbParams_.Count - 1]);

					listChbParams_[listChbParams_.Count - 1].AutoSize = true;
					listChbParams_[listChbParams_.Count - 1].Location = new System.Drawing.Point(x, y);
					listChbParams_[listChbParams_.Count - 1].Name = "chbParam" + cur_u_params_[iParam].First;
					listChbParams_[listChbParams_.Count - 1].Tag = cur_u_params_[iParam].First;
					listChbParams_[listChbParams_.Count - 1].Size = new System.Drawing.Size(80, 17);
					listChbParams_[listChbParams_.Count - 1].TabIndex = 0;
					listChbParams_[listChbParams_.Count - 1].Text = cur_u_params_[iParam].Second;
					listChbParams_[listChbParams_.Count - 1].UseVisualStyleBackColor = true;
					listChbParams_[listChbParams_.Count - 1].CheckedChanged +=
						new System.EventHandler(this.chbParam_CheckedChanged);

					y += 20;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmSeparatedGraphs_Load(): ");
				throw;
			}
		}

		private void chbParam_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				short selected;
				checkIfNotMore4Selected(out selected);

				CheckBox curChb = (CheckBox)sender;
				if (curChb == null) return;

				if (curChb.Checked)
				{
					string param = curChb.Tag.ToString();
					int index = -1;
					for (int iCol = 0; iCol < curDataGrid_.TableStyles[0].GridColumnStyles.Count; ++iCol)
					{
						if (curDataGrid_.TableStyles[0].GridColumnStyles[iCol].MappingName.Equals(param))
						{
							index = iCol;
							break;
						}
					}
					if (index == -1)
					{
						EmService.WriteToLogFailed("chbParam_CheckedChanged: param wasn't found! - " + param);
						return;
					}

					ZedGraphControl curGraph = null;
					int iGraph;
					// ищем первый свободный график
					for (iGraph = 0; iGraph < listZedGraphControls_.Count; ++iGraph)
					{
						if (listZedGraphControls_[iGraph].GraphPane.CurveList.Count == 0)
						{
							curGraph = listZedGraphControls_[iGraph];
							break;
						}
					}
					if (curGraph == null)
					{
						EmService.WriteToLogFailed("chbParam_CheckedChanged: no free graph!");
						return;
					}

					Color curColor = Color.Red;
					if (iGraph == 1) { curColor = Color.BlueViolet; }
					else if (iGraph == 2) { curColor = Color.DarkGreen; }
					else if (iGraph == 3) { curColor = Color.Brown; }
					AddCurver(index, curColor, ref curGraph, curChb.Text);
				}
				else
				{
					for (int iGraph = 0; iGraph < listZedGraphControls_.Count; ++iGraph)
					{
						if (listZedGraphControls_[iGraph].GraphPane.CurveList.Count > 0)
						{
							string legend = listZedGraphControls_[iGraph].GraphPane.CurveList[0].Label;
							if (curChb.Text.Equals(legend))
							{
								// deleting curve
								GraphPane gPane = listZedGraphControls_[iGraph].GraphPane;

								while (gPane.CurveList.IndexOf(gPane.CurveList[legend]) != -1)
								{
									gPane.CurveList.Remove(gPane.CurveList[legend]);
								}

								// Zoom set defalult
								Graphics g = listZedGraphControls_[iGraph].CreateGraphics();
								gPane.XAxis.ResetAutoScale(gPane, g);
								gPane.YAxis.ResetAutoScale(gPane, g);
								gPane.Y2Axis.ResetAutoScale(gPane, g);
								g.Dispose();
								listZedGraphControls_[iGraph].Refresh();

								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmSeparatedGraphs::chbParam_CheckedChanged(): ");
				throw;
			}
		}

		private void AddCurver(int column, Color curColor, ref ZedGraphControl curGraph, string legend)
		{
			try
			{
				// saving sort state and resorting to build graph correctly
				string SortRule = GetCurrentSortRule(curDataGrid_);
				SortByColumn(curDataGrid_, 0, false);

				// starting build point list
				GraphPane gPane = curGraph.GraphPane;
				List<PointPairList> lists = new List<PointPairList>();
				lists.Add(new PointPairList());
				ushort listIndex = 0;

				//DateTime lastDate = DateTime.MinValue;
				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					try
					{
						double y_start = GetNumberFromDataGrid(curDataGrid_, 0, column);
						double x0_start = (double)new XDate(Convert.ToDateTime(curDataGrid_[0, 0]).AddSeconds(-period_in_secs_));
						double x1_start = (double)new XDate(Convert.ToDateTime(curDataGrid_[0, 0]));
						lists[listIndex].Add(x0_start, y_start);
						lists[listIndex].Add(x1_start, y_start);
						//lastDate = Convert.ToDateTime(dg[0, 0]);
					}
					catch { }
					for (int iRow = 0; iRow < (curDataGrid_.DataSource as DataSet).Tables[0].Rows.Count - 1; iRow++)
					{
						try
						{
							//bool permanent = false;
							//if (lastDate != DateTime.MinValue)
							//    permanent = (Convert.ToDateTime(dg[iRow + 1, 0]) - lastDate).TotalSeconds <= (period_in_secs + 6);

							double y = GetNumberFromDataGrid(curDataGrid_, iRow + 1, column);
							double x0 = (double)new XDate(Convert.ToDateTime(curDataGrid_[iRow, 0]));
							double x1 = (double)new XDate(Convert.ToDateTime(curDataGrid_[iRow + 1, 0]));
							bool permanent = (Convert.ToDateTime(curDataGrid_[iRow + 1, 0]) - 
								Convert.ToDateTime(curDataGrid_[iRow, 0])).TotalSeconds <= (period_in_secs_ + 9);

							lists[listIndex].Add(x0, y);
							if (!permanent)
							{
								lists.Add(new PointPairList());
								listIndex++;
							}
							else lists[listIndex].Add(x1, y);

							//lastDate = Convert.ToDateTime(dg[iRow + 1, 0]);
						}
						catch { }
					}
				}
				else
				{
					for (int iRow = 0; iRow < (curDataGrid_.DataSource as DataSet).Tables[0].Rows.Count - 1; iRow++)
					{
						try
						{
							// проверяем расстояние между соседними датами. если больше заданного, то обрываем старый
							// график и начинаем новый
							//bool permanent = false;
							//if(lastDate != DateTime.MinValue)
							//    permanent = (Convert.ToDateTime(dg[iRow + 1, 0]) - lastDate).TotalSeconds <= (period_in_secs + 6);

							double y0 = GetNumberFromDataGrid(curDataGrid_, iRow, column);
							double x0 = (double)new XDate(Convert.ToDateTime(curDataGrid_[iRow, 0]));
							double x1 = (double)new XDate(Convert.ToDateTime(curDataGrid_[iRow + 1, 0]));

							bool permanent = (Convert.ToDateTime(curDataGrid_[iRow + 1, 0]) - 
								Convert.ToDateTime(curDataGrid_[iRow, 0])).TotalSeconds <= (period_in_secs_ + 9);

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
						catch { }
					}

				}
				// restoring sorting rule
				SetSortRule(curDataGrid_, SortRule);

				for (int iCurList = 0; iCurList < lists.Count; ++iCurList)
				{
					CurveItem myCurve = gPane.AddCurve(legend, lists[iCurList], curColor, SymbolType.None);
					//if (Y2Axis) myCurve.IsY2Axis = true;
				}

				//gPane.AxisChange(this.CreateGraphics());

				// Axis X, Y and Y2
				gPane.XAxis.IsVisible = true;	// wndDocAVGGraph.tsXGridLine.Checked;
				gPane.XAxis.IsShowGrid = true;	// wndDocAVGGraph.tsXGridLine.Checked;
				gPane.XAxis.IsShowMinorGrid = false;	// wndDocAVGGraph.tsXMinorGridLine.Checked;

				gPane.YAxis.IsVisible = true;	// wndDocAVGGraph.tsYGridLine.Checked;
				gPane.YAxis.IsShowGrid = true;	// wndDocAVGGraph.tsYGridLine.Checked;
				gPane.YAxis.IsShowMinorGrid = false;	// wndDocAVGGraph.tsYMinorGridLine.Checked;

				gPane.Y2Axis.IsVisible = false;		// wndDocAVGGraph.tsY2GridLine.Checked;
				gPane.Y2Axis.IsShowGrid = false;	// wndDocAVGGraph.tsY2GridLine.Checked;
				gPane.Y2Axis.IsShowMinorGrid = false;	// wndDocAVGGraph.tsY2MinorGridLine.Checked;

				// Zoom set defalult
				Graphics g = curGraph.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				curGraph.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmSeparatedGraphs::AddCurver():");
				throw;
			}
		}

		// ф-я проверяет не выбрано ли больше 4-х параметров для графика
		private void checkIfNotMore4Selected(out short selected)
		{
			selected = 0;
			try
			{
				for (int iCheck = 0; iCheck < listChbParams_.Count; ++iCheck)
				{
					CheckBox curChb = (CheckBox)listChbParams_[iCheck];
					if (curChb != null)
					{
						if (curChb.Checked) selected++;
					}
				}

				// если уже отмечено 4, делаем остальные неактивными
				if (selected >= 4)
				{
					for (int iCheck = 0; iCheck < listChbParams_.Count; ++iCheck)
					{
						CheckBox curChb = (CheckBox)listChbParams_[iCheck];
						if (curChb != null)
						{
							if (!curChb.Checked) curChb.Enabled = false;
						}
					}
				}
				else
				{
					for (int iCheck = 0; iCheck < listChbParams_.Count; ++iCheck)
					{
						CheckBox curChb = (CheckBox)listChbParams_[iCheck];
						if (curChb != null)
						{
							curChb.Enabled = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in checkIfNotMore4Selected(): ");
				//throw;
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

		private void btnApply_Click(object sender, EventArgs e)
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

				for (int iGraph = 0; iGraph < listZedGraphControls_.Count; ++iGraph)
				{
					GraphPane pane = listZedGraphControls_[iGraph].MasterPane.PaneList[0];

					ZoomState oldState = pane.GetZoomStack().Push(pane, ZoomState.StateType.Zoom);

					pane.XAxis.Min = x1;
					pane.XAxis.Max = x2;

					listZedGraphControls_[iGraph].SetScroll(
						listZedGraphControls_[iGraph].hScrollBar1, pane.XAxis, 0, 0);

					// Provide Callback to notify the user of zoom events
					//if (zedGraph.ZoomEvent != null)
					//	zedGraph.ZoomEvent(zedGraph, oldState, new ZoomState(pane, ZoomState.StateType.Zoom));

					Graphics g = listZedGraphControls_[iGraph].CreateGraphics();
					pane.AxisChange(g);
					g.Dispose();
					listZedGraphControls_[iGraph].Refresh();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmSeparatedGraphs::btnApply_Click(): ");
				throw;
			}
		}
	}
}
