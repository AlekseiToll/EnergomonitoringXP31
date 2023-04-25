using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using EmServiceLib;
using WeifenLuo.WinFormsUI;
using ZedGraph;
using EmGraphLib;

namespace EnergomonitoringXP
{
	public partial class frmDocAVGGraphRight : DockContent
	{
		/// <summary>
		/// Settings object
		/// </summary>
		private EmDataSaver.Settings settings_;

		/// <summary>
		/// Pointer to the main application window
		/// </summary>
		private frmMain MainWindow_;

		private EmDeviceType curDevType_;

		private bool bHarmPowersPercentShown_ = false;
		private bool bHarmCurrentPercentShown_ = true;

		private ConnectScheme connectScheme_ = ConnectScheme.Unknown;

		private ConnectScheme connectSchemeOld_ = ConnectScheme.Unknown;
		// используется только для схемы 3ф3пр, показывает есть ли в базе значения
		// "фазное напряжение 1-ая гармоника"
		private bool existsU1_ = false;

		public frmDocAVGGraphRight(frmMain MainWindow, EmDataSaver.Settings settings, 
									EmDeviceType devType)
		{
			InitializeComponent();

			this.settings_ = settings;
			this.MainWindow_ = MainWindow;
			this.curDevType_ = devType;
		}

		public void Customize()
		{
			try
			{
				if (curDevType_ != EmDeviceType.ETPQP_A)	// для ЭтПКЭ-А углы заменяем на интергармоники
				{
					if (connectScheme_ == ConnectScheme.Ph3W3 ||
						connectScheme_ == ConnectScheme.Ph3W3_B_calc)
					{
						if (zedGraphAngles.Visible)
							zedGraphAngles.Visible = false;
					}
					else
					{
						if (!zedGraphAngles.Visible)
							zedGraphAngles.Visible = true;
					}
				}

				if (tsScaleMode.Checked) tsScaleMode.PerformClick();
				tsNominalCurrent.Text = string.Empty;
				tsScaleMode.Enabled = false;

				if (connectScheme_ == ConnectScheme.Ph1W2)
				{
					tsLg.Checked = false;
					tsFirstHarmonic.Checked = false;
					tsPhaseA.Checked = true;
					tsPhaseB.Checked = false;
					tsPhaseB.Enabled = false;
					tsPhaseC.Checked = false;
					tsPhaseC.Enabled = false;
					tsSumm.Checked = false;
					tsSumm.Enabled = false;
				}
				else
				{
					tsLg.Checked = false;
					tsFirstHarmonic.Checked = false;
					tsPhaseA.Checked = true;
					tsPhaseB.Checked = true;
					tsPhaseB.Enabled = true;
					tsPhaseC.Checked = true;
					tsPhaseC.Enabled = true;
					tsSumm.Checked = true;
					tsSumm.Enabled = true;
				}

				if (curDevType_ == EmDeviceType.ETPQP /*|| curDevType_ == EmDeviceType.ETPQP_A*/)
					zedGraphW.Visible = false;
				else zedGraphW.Visible = true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Customize(): ");
				throw;
			}
		}

		/// <summary>Synchronize settings</summary>
		/// <param name="NewSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			settings_ = NewSettings.Clone();
		}

		public void SetDeviceType(EmDeviceType devType)
		{
			this.curDevType_ = devType;
		}

		private void frmGraphRightPanel_Load(object sender, EventArgs e)
		{
			try
			{
				#region Customizing ZedGraph component

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				// voltages
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					zedGraphU.GraphPane.Title = rm.GetString("name_params_voltage_harmonics_ph");
				else zedGraphU.GraphPane.Title = rm.GetString("name_params_voltage_harmonics_lin");

				zedGraphU.GraphPane.XAxis.Title = string.Empty;
				zedGraphU.GraphPane.YAxis.Title = string.Empty;

				if (settings_.AvgBrushColor1 == settings_.AvgBrushColor2)
					zedGraphU.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1));
				else
					zedGraphU.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1),
						Color.FromArgb(settings_.AvgBrushColor2), 90);
				zedGraphU.GraphPane.YAxis.GridColor = Color.LightGray;
				zedGraphU.GraphPane.YAxis.GridDashOn = 1;
				zedGraphU.GraphPane.YAxis.GridDashOff = 0;
				zedGraphU.GraphPane.YAxis.IsShowGrid = true;

				zedGraphU.GraphPane.YAxis.MinorGridColor = Color.LightGray;
				zedGraphU.GraphPane.YAxis.MinorGridDashOn = 1;
				zedGraphU.GraphPane.YAxis.MinorGridDashOff = 1;
				zedGraphU.GraphPane.YAxis.IsShowMinorGrid = true;
				zedGraphU.GraphPane.XAxis.Type = AxisType.Ordinal;
				zedGraphU.GraphPane.XAxis.ScaleFormat = "0";
				zedGraphU.GraphPane.XAxis.Step = 1;

				// currents
				zedGraphI.GraphPane.Title = rm.GetString("name_params_current_harmonics");
				zedGraphI.GraphPane.XAxis.Title = string.Empty;
				zedGraphI.GraphPane.YAxis.Title = string.Empty;
				if (settings_.AvgBrushColor1 == settings_.AvgBrushColor2)
					zedGraphI.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1));
				else
					zedGraphI.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1),
						Color.FromArgb(settings_.AvgBrushColor2), 90);
				zedGraphI.GraphPane.YAxis.GridColor = Color.LightGray;
				zedGraphI.GraphPane.YAxis.GridDashOn = 1;
				zedGraphI.GraphPane.YAxis.GridDashOff = 0;
				zedGraphI.GraphPane.YAxis.IsShowGrid = true;

				zedGraphI.GraphPane.YAxis.MinorGridColor = Color.LightGray;
				zedGraphI.GraphPane.YAxis.MinorGridDashOn = 1;
				zedGraphI.GraphPane.YAxis.MinorGridDashOff = 1;
				zedGraphI.GraphPane.YAxis.IsShowMinorGrid = true;
				zedGraphI.GraphPane.XAxis.Type = AxisType.Ordinal;
				zedGraphI.GraphPane.XAxis.ScaleFormat = "0";
				zedGraphI.GraphPane.XAxis.Step = 1;

				// angles
				if (curDevType_ != EmDeviceType.ETPQP_A)
					zedGraphAngles.GraphPane.Title = rm.GetString("name_params_angles");
				else
				{
					if(connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
						zedGraphAngles.GraphPane.Title = rm.GetString("name_params_u_lin_interharm");
					else zedGraphAngles.GraphPane.Title = rm.GetString("name_params_u_ph_interharm");
				}
				zedGraphAngles.GraphPane.XAxis.Title = string.Empty;
				zedGraphAngles.GraphPane.YAxis.Title = string.Empty;

				if (settings_.AvgBrushColor1 == settings_.AvgBrushColor2)
					zedGraphAngles.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1));
				else
					zedGraphAngles.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1),
						Color.FromArgb(settings_.AvgBrushColor2), 90);

				zedGraphAngles.GraphPane.YAxis.GridColor = Color.LightGray;
				zedGraphAngles.GraphPane.YAxis.GridDashOn = 1;
				zedGraphAngles.GraphPane.YAxis.GridDashOff = 0;
				zedGraphAngles.GraphPane.YAxis.IsShowGrid = true;

				zedGraphAngles.GraphPane.YAxis.MinorGridColor = Color.LightGray;
				zedGraphAngles.GraphPane.YAxis.MinorGridDashOn = 1;
				zedGraphAngles.GraphPane.YAxis.MinorGridDashOff = 1;
				zedGraphAngles.GraphPane.YAxis.IsShowMinorGrid = true;
				zedGraphAngles.GraphPane.XAxis.Type = AxisType.Ordinal;
				zedGraphAngles.GraphPane.XAxis.ScaleFormat = "0";
				zedGraphAngles.GraphPane.XAxis.Step = 1;

				// powers
				if (curDevType_ != EmDeviceType.ETPQP_A)
					zedGraphW.GraphPane.Title = rm.GetString("name_params_power_harmonics_watt");
				else
					zedGraphAngles.GraphPane.Title = rm.GetString("name_params_i_interharm");
				zedGraphW.GraphPane.XAxis.Title = string.Empty;
				zedGraphW.GraphPane.YAxis.Title = string.Empty;
				if (settings_.AvgBrushColor1 == settings_.AvgBrushColor2)
					zedGraphW.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1));
				else
					zedGraphW.GraphPane.AxisFill = new Fill(
						Color.FromArgb(settings_.AvgBrushColor1),
						Color.FromArgb(settings_.AvgBrushColor2), 90);
				zedGraphW.GraphPane.YAxis.GridColor = Color.LightGray;
				zedGraphW.GraphPane.YAxis.GridDashOn = 1;
				zedGraphW.GraphPane.YAxis.GridDashOff = 0;
				zedGraphW.GraphPane.YAxis.IsShowGrid = true;

				zedGraphW.GraphPane.YAxis.MinorGridColor = Color.LightGray;
				zedGraphW.GraphPane.YAxis.MinorGridDashOn = 1;
				zedGraphW.GraphPane.YAxis.MinorGridDashOff = 1;
				zedGraphW.GraphPane.YAxis.IsShowMinorGrid = true;
				zedGraphW.GraphPane.XAxis.Type = AxisType.Ordinal;
				zedGraphW.GraphPane.XAxis.ScaleFormat = "0";
				zedGraphW.GraphPane.XAxis.Step = 1;

				#endregion

				#region Customizing EmRadialGraph component
				radialGraph.RadialGridList.ZeroAngle = 120;

				radialGraph.RadialGridList.Add(100.0, 1.0, 2,
							System.Drawing.Drawing2D.DashStyle.Solid);
				radialGraph.RadialGridList.Add(1, 0.75, 1,
							System.Drawing.Drawing2D.DashStyle.Solid);
				#endregion

				UpdateGraphPanel(false, true);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmGraphRightPanel_Load():");
				throw;
			}
		}

		/// <summary>Redraw graph panel</summary>
		public void UpdateGraphPanel(bool bHarmPowersPercentShown,
									bool bHarmCurrentPercentShown)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					zedGraphU.GraphPane.Title = rm.GetString("name_params_voltage_harmonics_ph");
				else zedGraphU.GraphPane.Title = rm.GetString("name_params_voltage_harmonics_lin");

				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					if (zedGraphAngles.Visible)
						zedGraphAngles.Visible = false;
				}
				else
				{
					if (!zedGraphAngles.Visible)
						zedGraphAngles.Visible = true;
				}

				string phase1, phase2, phase3;
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					phase1 = emstrings.name_phases_phaseA;
					phase2 = emstrings.name_phases_phaseB;
					phase3 = emstrings.name_phases_phaseC;
				}
				else
				{
					phase1 = emstrings.name_phases_phaseAB;
					phase2 = emstrings.name_phases_phaseBC;
					phase3 = emstrings.name_phases_phaseCA;
				}

				// buttons
				tsPhaseA.Text = phase1;
				tsPhaseB.Text = phase2;
				tsPhaseC.Text = phase3;

				tsSumm.Visible = connectScheme_ != ConnectScheme.Ph1W2;
				tsPhaseB.Visible = connectScheme_ != ConnectScheme.Ph1W2;
				tsPhaseC.Visible = connectScheme_ != ConnectScheme.Ph1W2;

				CurrencyManager[] CMs_ = MainWindow_.wndDocAVG.wndDocAVGMain.CMs_;

				#region Voltage

				GraphPane gPane_U = this.zedGraphU.GraphPane;
				gPane_U.CurveList.Clear();

				CurrencyManager curCMs = CMs_[(int)AvgPages.U_PH_HARMONICS];
				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
					curCMs = CMs_[(int)AvgPages.U_LIN_HARMONICS];

				if (curCMs != null)
				{
					if (tsLg.Checked)
					{
						gPane_U.YAxis.Type = AxisType.Log;
						////gPane_U.YAxis.Min = 0.01;
						//gPane_U.YAxis.CrossAuto = false;
						//gPane_U.YAxis.IsCrossTic = true;
						//gPane_U.YAxis.Cross = 10;

					}
					else
					{
						gPane_U.YAxis.Type = AxisType.Linear;
					}

					try
					{
						if (tsPhaseA.Checked)
						{
							double[] yUA = new double[40];
							if (tsFirstHarmonic.Checked)
							{
								// Phase A, first harmonic
								yUA[0] = (double)100;
							}
							// Phase A, harmonics from 2 to 40
							for (int i = 2; i <= 40; i++)
							{
								string rowName = "k_ua_ab_" + i.ToString();
								if (curDevType_ == EmDeviceType.EM32 || curDevType_ == EmDeviceType.ETPQP)
								{
									if (connectScheme_ == ConnectScheme.Ph3W3 ||
										connectScheme_ == ConnectScheme.Ph3W3_B_calc)
										rowName = "k_uab_" + i.ToString();
									else
										rowName = "k_ua_" + i.ToString();
								}
								else if (curDevType_ == EmDeviceType.ETPQP_A)
								{
									if (connectScheme_ == ConnectScheme.Ph3W3 ||
										connectScheme_ == ConnectScheme.Ph3W3_B_calc)
										rowName = string.Format("order_value_{0}_ab", i);
									else
										rowName = string.Format("order_value_{0}_a", i);
								}
								try
								{
									if ((curCMs.Current as DataRowView).Row[rowName] is double)
										yUA[i - 1] = (double)((curCMs.Current as DataRowView).Row[rowName]);
									else
										yUA[i - 1] = (float)((curCMs.Current as DataRowView).Row[rowName]);
								}
								catch (InvalidCastException) { yUA[i - 1] = 0; }
							}
							BarItem barA = gPane_U.AddBar(phase1, null, yUA, Color.Gold);
							barA.Bar.Border.IsVisible = false;
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (tsPhaseB.Checked)
							{
								double[] yUB = new double[40];
								if (tsFirstHarmonic.Checked)
								{
									// Phase B, first harmonic	
									yUB[0] = (double)100;
								}
								// Phase B, harmonics from 2 to 40
								for (int i = 2; i <= 40; i++)
								{
									string rowName = "k_ub_bc_" + i.ToString();
									if (curDevType_ == EmDeviceType.EM32 ||
										curDevType_ == EmDeviceType.ETPQP)
									{
										if (connectScheme_ == ConnectScheme.Ph3W3 ||
											connectScheme_ == ConnectScheme.Ph3W3_B_calc)
											rowName = "k_ubc_" + i.ToString();
										else
											rowName = "k_ub_" + i.ToString();
									}
									else if (curDevType_ == EmDeviceType.ETPQP_A)
									{
										if (connectScheme_ == ConnectScheme.Ph3W3 ||
											connectScheme_ == ConnectScheme.Ph3W3_B_calc)
											rowName = string.Format("order_value_{0}_bc", i);
										else
											rowName = string.Format("order_value_{0}_b", i);
									}
									try
									{
										if ((curCMs.Current as DataRowView).Row[rowName] is double)
											yUB[i - 1] = (double)((curCMs.Current as DataRowView).Row[rowName]);
										else
											yUB[i - 1] = (float)((curCMs.Current as DataRowView).Row[rowName]);
									}
									catch (InvalidCastException) { yUB[i - 1] = 0; }
								}
								BarItem barB = gPane_U.AddBar(phase2, null, yUB, Color.Green);
								barB.Bar.Border.IsVisible = false;
							}

							if (tsPhaseC.Checked)
							{
								double[] yUC = new double[40];
								if (tsFirstHarmonic.Checked)
								{
									// Phase C, first harmonic
									yUC[0] = (double)100;
								}
								// Phase C, harmonics from 2 to 40
								for (int i = 2; i <= 40; i++)
								{
									string rowName = "k_uc_ca_" + i.ToString();
									if (curDevType_ == EmDeviceType.EM32 ||
										curDevType_ == EmDeviceType.ETPQP)
									{
										if (connectScheme_ == ConnectScheme.Ph3W3 ||
											connectScheme_ == ConnectScheme.Ph3W3_B_calc)
											rowName = "k_uca_" + i.ToString();
										else
											rowName = "k_uc_" + i.ToString();
									}
									else if (curDevType_ == EmDeviceType.ETPQP_A)
									{
										if (connectScheme_ == ConnectScheme.Ph3W3 ||
											connectScheme_ == ConnectScheme.Ph3W3_B_calc)
											rowName = string.Format("order_value_{0}_ca", i);
										else
											rowName = string.Format("order_value_{0}_c", i);
									}
									try
									{
										if ((curCMs.Current as DataRowView).Row[rowName] is double)
											yUC[i - 1] = (double)((curCMs.Current as DataRowView).Row[rowName]);
										else
											yUC[i - 1] = (float)((curCMs.Current as DataRowView).Row[rowName]);
									}
									catch (InvalidCastException) { yUC[i - 1] = 0; }
								}
								BarItem barC = gPane_U.AddBar(phase3, null, yUC, Color.Red);
								barC.Bar.Border.IsVisible = false;
							}
						}
					}
					catch (ArgumentException aex)
					{
						EmService.DumpException(aex, "ArgumentException in UpdateGraphPanel() 2");
					}

					// Zoom set defalult
					Graphics gU = CreateGraphics();
					gPane_U.XAxis.ResetAutoScale(gPane_U, gU);
					gPane_U.YAxis.ResetAutoScale(gPane_U, gU);
					gPane_U.Y2Axis.ResetAutoScale(gPane_U, gU);
					gU.Dispose();
				}
				zedGraphU.Refresh();

				#endregion

				#region Current Harmonics

				GraphPane gPane_I = this.zedGraphI.GraphPane;
				gPane_I.CurveList.Clear();

				bHarmCurrentPercentShown_ = bHarmCurrentPercentShown;
				if (!bHarmCurrentPercentShown_ || curDevType_ == EmDeviceType.ETPQP_A)
				{
					zedGraphI.GraphPane.Title = rm.GetString("name_params_current_harmonics_a");
				}
				else
				{
					zedGraphI.GraphPane.Title = rm.GetString("name_params_current_harmonics_perc");
				}

				if (CMs_[(int)AvgPages.I_HARMONICS] != null)
				{
					if (tsLg.Checked)
					{
						gPane_I.YAxis.Type = AxisType.Log;
					}
					else
					{
						gPane_I.YAxis.Type = AxisType.Linear;
					}

					if (tsPhaseA.Checked)
					{
						double[] yIA = new double[40];
						if (tsFirstHarmonic.Checked)
						{
							// Phase A, first harmonic
							yIA[0] = (double)100;
						}
						// Phase A, harmonics from 2 to 40
						for (int i = 2; i <= 40; i++)
						{
							string rowName = "k_ia_" + i.ToString();
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								rowName = string.Format("order_value_{0}_a", i);
							}
							try
							{
								if ((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName] is double)
									yIA[i - 1] = (double)((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName]);
								else
									yIA[i - 1] = (float)((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName]);
							}
							catch { yIA[i - 1] = 0; }
						}
						BarItem barA = gPane_I.AddBar(emstrings.name_phases_phaseA, null, yIA, Color.Gold);
						barA.Bar.Border.IsVisible = false;
					}

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						if (tsPhaseB.Checked)
						{
							double[] yIB = new double[40];
							if (tsFirstHarmonic.Checked)
							{
								// Phase B, first harmonic	
								yIB[0] = (double)100;
							}
							// Phase B, harmonics from 2 to 40
							for (int i = 2; i <= 40; i++)
							{
								string rowName = "k_ib_" + i.ToString();
								if (curDevType_ == EmDeviceType.ETPQP_A)
								{
									rowName = string.Format("order_value_{0}_b", i);
								}
								try
								{
									if ((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName] is double)
										yIB[i - 1] = (double)((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName]);
									else
										yIB[i - 1] = (float)((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName]);
								}
								catch { yIB[i - 1] = 0; }
							}
							BarItem barB = gPane_I.AddBar(emstrings.name_phases_phaseB, null, yIB, Color.Green);
							barB.Bar.Border.IsVisible = false;
						}

						if (tsPhaseC.Checked)
						{
							double[] yIC = new double[40];
							if (tsFirstHarmonic.Checked)
							{
								// Phase C, first harmonic
								yIC[0] = (double)100;
							}
							// Phase C, harmonics from 2 to 40
							for (int i = 2; i <= 40; i++)
							{
								string rowName = "k_ic_" + i.ToString();
								if (curDevType_ == EmDeviceType.ETPQP_A)
								{
									rowName = string.Format("order_value_{0}_c", i);
								}
								try
								{
									if ((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName] is double)
										yIC[i - 1] = (double)((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName]);
									else
										yIC[i - 1] = (float)((CMs_[(int)AvgPages.I_HARMONICS].Current as DataRowView).Row[rowName]);
								}
								catch { yIC[i - 1] = 0; }
							}
							BarItem barC = gPane_I.AddBar(emstrings.name_phases_phaseC, null, yIC, Color.Red);
							barC.Bar.Border.IsVisible = false;
						}
					}

					Graphics gI = CreateGraphics();
					gPane_I.XAxis.ResetAutoScale(gPane_I, gI);
					gPane_I.YAxis.ResetAutoScale(gPane_I, gI);
					gPane_I.Y2Axis.ResetAutoScale(gPane_I, gI);
					gI.Dispose();
				}
				zedGraphI.Refresh();

				#endregion

				#region Powers

				bHarmPowersPercentShown_ = bHarmPowersPercentShown;
				if (!bHarmPowersPercentShown_)
				{
					zedGraphW.GraphPane.Title = rm.GetString("name_params_power_harmonics_watt");
				}
				else
				{
					zedGraphW.GraphPane.Title = rm.GetString("name_params_power_harmonics_percent");
				}
				if (curDevType_ == EmDeviceType.ETPQP_A)
				{
					if (connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
						zedGraphW.GraphPane.Title = rm.GetString("name_params_u_lin_interharm");
					else zedGraphW.GraphPane.Title = rm.GetString("name_params_u_ph_interharm");
				}

				GraphPane gPane_W = this.zedGraphW.GraphPane;
				gPane_W.CurveList.Clear();

				if (curDevType_ != EmDeviceType.ETPQP && curDevType_ != EmDeviceType.ETPQP_A)
				{
					#region Not Et-PQP-A

					if (CMs_[(int)AvgPages.HARMONIC_POWERS] != null)
					{
						if (tsSumm.Checked)
						{
							string rowName = "p_sum_";
							//if (ConnectionScheme == 2) rowName = "P_A(1) ";

							double[] yWS = new double[40];
							if (tsFirstHarmonic.Checked)
							{
								try
								{
									if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"] is double)
										yWS[0] = (double)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"]);
									else
										yWS[0] = (float)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"]);
								}
								catch { yWS[0] = 0; }
							}
							for (int i = 2; i <= 40; i++)
							{
								try
								{
									if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()] is double)
										yWS[i - 1] = (double)(
											(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]);
									else
										yWS[i - 1] = (float)(
											(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]);
								}
								catch { yWS[i - 1] = 0; }
							}
							BarItem barSumm = gPane_W.AddBar("∑", null, yWS, Color.LightBlue);
							barSumm.Bar.Border.IsVisible = false;
						}

						if (tsPhaseA.Checked)
						{
							double[] yWA = new double[40];

							string rowName = "p_a_1_";
							//if (ConnectionScheme == 2) rowName = "P_B(2) ";

							if (tsFirstHarmonic.Checked)
							{
								try
								{
									if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"] is double)
										yWA[0] = (double)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"]);
									else
										yWA[0] = (float)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"]);
								}
								catch { yWA[0] = 0; }
							}
							for (int i = 2; i <= 40; i++)
							{
								try
								{
									if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()] is double)
										yWA[i - 1] = (double)(
											(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]);
									else
										yWA[i - 1] = (float)(
											(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]);
								}
								catch { yWA[i - 1] = 0; }
							}
							string hint = phase1;
							if (connectScheme_ == ConnectScheme.Ph3W3 ||
								connectScheme_ == ConnectScheme.Ph3W3_B_calc)
								hint = "P1";
							BarItem barA = gPane_W.AddBar(hint, null, yWA, Color.Gold);
							barA.Bar.Border.IsVisible = false;
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (tsPhaseB.Checked)
							{
								string rowName = "p_b_2_";

								double[] yWB = new double[40];
								if (tsFirstHarmonic.Checked)
								{
									try
									{
										if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"] is double)
											yWB[0] = (double)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[
												rowName + "1"]);
										else
											yWB[0] = (float)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[
												rowName + "1"]);
									}
									catch { yWB[0] = 0; }
								}
								for (int i = 2; i <= 40; i++)
								{
									try
									{
										if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]
											is double)
											yWB[i - 1] = (double)(
												(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]);
										else
											yWB[i - 1] = (float)(
												(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]);
									}
									catch { yWB[i - 1] = 0; }
								}
								string hint = phase2;
								if (connectScheme_ == ConnectScheme.Ph3W3 ||
									connectScheme_ == ConnectScheme.Ph3W3_B_calc)
									hint = "P2";
								BarItem barB = gPane_W.AddBar(hint, null, yWB, Color.Green);
								barB.Bar.Border.IsVisible = false;
							}

							if (connectScheme_ == ConnectScheme.Ph3W4 ||
								connectScheme_ == ConnectScheme.Ph3W4_B_calc)
							{
								if (tsPhaseC.Checked)
								{
									double[] yWC = new double[40];

									string rowName = "p_c_";

									if (tsFirstHarmonic.Checked)
									{
										try
										{
											if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + "1"] is double)
												yWC[0] = (double)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[
													rowName + "1"]);
											else
												yWC[0] = (float)((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[
													rowName + "1"]);
										}
										catch { yWC[0] = 0; }
									}
									for (int i = 2; i <= 40; i++)
									{
										try
										{
											if ((CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName + i.ToString()]
												is double)
												yWC[i - 1] = (double)(
													(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName +
													i.ToString()]);
											else
												yWC[i - 1] = (float)(
													(CMs_[(int)AvgPages.HARMONIC_POWERS].Current as DataRowView).Row[rowName +
													i.ToString()]);
										}
										catch { yWC[i - 1] = 0; }
									}
									BarItem barC = gPane_W.AddBar(phase3, null, yWC, Color.Red);
									barC.Bar.Border.IsVisible = false;
								}
							}
						}

						// Zoom set defalult
						Graphics gW = CreateGraphics();
						gPane_W.XAxis.ResetAutoScale(gPane_W, gW);
						gPane_W.YAxis.ResetAutoScale(gPane_W, gW);
						gPane_W.Y2Axis.ResetAutoScale(gPane_W, gW);
						gW.Dispose();
					}

					#endregion
				}
				else
				{
					int curInterHarmType = (int)AvgPages.U_PH_INTERHARM;
					if (connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
						curInterHarmType = (int)AvgPages.U_LIN_INTERHARM;
					if (CMs_[curInterHarmType] != null)
					{
						if (tsPhaseA.Checked)
						{
							double[] yWA = new double[40];

							if (tsFirstHarmonic.Checked)
							{
								try
								{
									string rowName = "avg_square_order_1_a";
									if (curInterHarmType == (int)AvgPages.U_LIN_INTERHARM) rowName += "b";

									if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
										yWA[0] = (double)((CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									else
										yWA[0] = (float)((CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
								}
								catch { yWA[0] = 0; }
							}
							for (int i = 2; i <= 40; i++)
							{
								try
								{
									string rowName = string.Format("avg_square_order_{0}_a", i);
									if (curInterHarmType == (int)AvgPages.U_LIN_INTERHARM) rowName += "b";

									if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
										yWA[i - 1] = (double)(
											(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									else
										yWA[i - 1] = (float)(
											(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
								}
								catch { yWA[i - 1] = 0; }
							}
							string hint = phase1;
							BarItem barA = gPane_W.AddBar(hint, null, yWA, Color.Gold);
							barA.Bar.Border.IsVisible = false;
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (tsPhaseB.Checked)
							{
								double[] yWB = new double[40];

								if (tsFirstHarmonic.Checked)
								{
									try
									{
										string rowName = "avg_square_order_1_b";
										if (curInterHarmType == (int)AvgPages.U_LIN_INTERHARM) rowName += "c";

										if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
											yWB[0] = (double)((CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
										else
											yWB[0] = (float)((CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									}
									catch { yWB[0] = 0; }
								}
								for (int i = 2; i <= 40; i++)
								{
									try
									{
										string rowName = string.Format("avg_square_order_{0}_b", i);
										if (curInterHarmType == (int)AvgPages.U_LIN_INTERHARM) rowName += "c";

										if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
											yWB[i - 1] = (double)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
										else
											yWB[i - 1] = (float)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									}
									catch { yWB[i - 1] = 0; }
								}
								string hint = phase1;
								BarItem barB = gPane_W.AddBar(hint, null, yWB, Color.Green);
								barB.Bar.Border.IsVisible = false;
							}

							if (tsPhaseC.Checked)
							{
								double[] yWC = new double[40];

								if (tsFirstHarmonic.Checked)
								{
									try
									{
										string rowName = "avg_square_order_1_c";
										if (curInterHarmType == (int)AvgPages.U_LIN_INTERHARM) rowName += "a";

										if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
											yWC[0] = (double)((CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
										else
											yWC[0] = (float)((CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									}
									catch { yWC[0] = 0; }
								}
								for (int i = 2; i <= 40; i++)
								{
									try
									{
										string rowName = string.Format("avg_square_order_{0}_c", i);
										if (curInterHarmType == (int)AvgPages.U_LIN_INTERHARM) rowName += "a";

										if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
											yWC[i - 1] = (double)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
										else
											yWC[i - 1] = (float)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									}
									catch { yWC[i - 1] = 0; }
								}
								string hint = phase1;
								BarItem barC = gPane_W.AddBar(hint, null, yWC, Color.Red);
								barC.Bar.Border.IsVisible = false;
							}
						}

						// Zoom set defalult
						Graphics gW = CreateGraphics();
						gPane_W.XAxis.ResetAutoScale(gPane_W, gW);
						gPane_W.YAxis.ResetAutoScale(gPane_W, gW);
						gPane_W.Y2Axis.ResetAutoScale(gPane_W, gW);
						gW.Dispose();
					}
				}
				zedGraphW.Refresh();

				#endregion

				#region UI angles

				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					zedGraphAngles.GraphPane.Title = rm.GetString("name_params_angles");
				}
				else
				{
					zedGraphAngles.GraphPane.Title = rm.GetString("name_params_i_interharm");
				}

				GraphPane gPane_A = this.zedGraphAngles.GraphPane;
				gPane_A.CurveList.Clear();

				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					#region NOT EtPQP-A

					if (CMs_[(int)AvgPages.HARMONIC_ANGLES] != null)
					{
						if (tsPhaseA.Checked)
						{
							double[] yUA = new double[40];
							// Phase A, harmonics from 2 to 40
							for (int i = 1; i <= 40; i++)
							{
								try
								{
									string rowName = "an_u_a_" + i.ToString() + "_i_a_" + i.ToString();
									if (curDevType_ == EmDeviceType.ETPQP_A)
									{
										rowName = string.Format("order_value_{0}_a", i);
									}

									if ((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName] is double)
										yUA[i - 1] = (double)((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName]);
									else
										yUA[i - 1] = (float)((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName]);
								}
								catch { yUA[i - 1] = 0; }
							}
							BarItem barA = gPane_A.AddBar(phase1, null, yUA, Color.Gold);
							barA.Bar.Border.IsVisible = false;
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (tsPhaseB.Checked)
							{
								double[] yUB = new double[40];
								// Phase B, harmonics from 2 to 40
								for (int i = 1; i <= 40; i++)
								{
									try
									{
										string rowName = "an_u_b_" + i.ToString() + "_i_b_" + i.ToString();
										if ((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName] is double)
											yUB[i - 1] = (double)((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName]);
										else
											yUB[i - 1] = (float)((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName]);
									}
									catch { yUB[i - 1] = 0; }
								}
								BarItem barB = gPane_A.AddBar(phase2, null, yUB, Color.Green);
								barB.Bar.Border.IsVisible = false;
							}

							if (tsPhaseC.Checked)
							{
								double[] yUC = new double[40];
								for (int i = 1; i <= 40; i++)
								{
									try
									{
										string rowName = "an_u_c_" + i.ToString() + "_i_c_" + i.ToString();
										if ((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName] is double)
											yUC[i - 1] = (double)((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName]);
										else
											yUC[i - 1] = (float)((CMs_[(int)AvgPages.HARMONIC_ANGLES].Current as DataRowView).Row[rowName]);
									}
									catch { yUC[i - 1] = 0; }
								}
								BarItem barC = gPane_A.AddBar(phase3, null, yUC, Color.Red);
								barC.Bar.Border.IsVisible = false;
							}
						}

						// Zoom set defalult
						Graphics gA = CreateGraphics();
						gPane_A.XAxis.ResetAutoScale(gPane_A, gA);
						gPane_A.YAxis.ResetAutoScale(gPane_A, gA);
						gPane_A.Y2Axis.ResetAutoScale(gPane_A, gA);
						gA.Dispose();
					}

					#endregion
				}
				else
				{
					#region EtPQP-A

					int curInterHarmType = (int)AvgPages.I_INTERHARM;
					if (CMs_[curInterHarmType] != null)
					{
						if (tsPhaseA.Checked)
						{
							double[] yIIA = new double[40];

							if (tsFirstHarmonic.Checked)
							{
								try
								{
									if ((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_a"] is double)
										yIIA[0] = (double)((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_a"]);
									else
										yIIA[0] = (float)((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_a"]);
								}
								catch { yIIA[0] = 0; }
							}
							for (int i = 2; i <= 40; i++)
							{
								try
								{
									string rowName = string.Format("avg_square_order_{0}_a", i);
									if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
										yIIA[i - 1] = (double)(
											(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									else
										yIIA[i - 1] = (float)(
											(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
								}
								catch { yIIA[i - 1] = 0; }
							}
							string hint = phase1;
							BarItem barA = gPane_A.AddBar(hint, null, yIIA, Color.Gold);
							barA.Bar.Border.IsVisible = false;
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (tsPhaseB.Checked)
							{
								double[] yIIB = new double[40];

								if (tsFirstHarmonic.Checked)
								{
									try
									{
										if ((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_b"] is double)
											yIIB[0] = (double)((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_b"]);
										else
											yIIB[0] = (float)((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_b"]);
									}
									catch { yIIB[0] = 0; }
								}
								for (int i = 2; i <= 40; i++)
								{
									try
									{
										string rowName = string.Format("avg_square_order_{0}_b", i);
										if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
											yIIB[i - 1] = (double)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
										else
											yIIB[i - 1] = (float)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									}
									catch { yIIB[i - 1] = 0; }
								}
								string hint = phase1;
								BarItem barB = gPane_A.AddBar(hint, null, yIIB, Color.Green);
								barB.Bar.Border.IsVisible = false;
							}

							if (tsPhaseC.Checked)
							{
								double[] yIIC = new double[40];

								if (tsFirstHarmonic.Checked)
								{
									try
									{
										if ((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_c"] is double)
											yIIC[0] = (double)((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_c"]);
										else
											yIIC[0] = (float)((CMs_[curInterHarmType].Current as DataRowView).Row["avg_square_order_1_c"]);
									}
									catch { yIIC[0] = 0; }
								}
								for (int i = 2; i <= 40; i++)
								{
									try
									{
										string rowName = string.Format("avg_square_order_{0}_c", i);
										if ((CMs_[curInterHarmType].Current as DataRowView).Row[rowName] is double)
											yIIC[i - 1] = (double)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
										else
											yIIC[i - 1] = (float)(
												(CMs_[curInterHarmType].Current as DataRowView).Row[rowName]);
									}
									catch { yIIC[i - 1] = 0; }
								}
								string hint = phase1;
								BarItem barC = gPane_A.AddBar(hint, null, yIIC, Color.Red);
								barC.Bar.Border.IsVisible = false;
							}
						}

						// Zoom set defalult
						Graphics gA = CreateGraphics();
						gPane_A.XAxis.ResetAutoScale(gPane_A, gA);
						gPane_A.YAxis.ResetAutoScale(gPane_A, gA);
						gPane_A.Y2Axis.ResetAutoScale(gPane_A, gA);
						gA.Dispose();
					}

					#endregion
				}

				zedGraphAngles.Refresh();

				#endregion

				#region Bottom Radial Angles

				if (CMs_[(int)AvgPages.F_U_I] != null && CMs_[(int)AvgPages.ANGLES] != null)
				{
					existsU1_ = true;
					radialGraph.RadialGridList.ZeroAngle = 0;
					//radialGraph.CurveList.Clear();

					double[] modulesA = new double[2];
					double[] anglesA = new double[2];
					double[] modulesB = new double[2];
					double[] anglesB = new double[2];
					double[] modulesC = new double[2];
					double[] anglesC = new double[2];

					string col_name;
					// U1_A
					try
					{
						if (curDevType_ != EmDeviceType.ETPQP_A)
							col_name = "u1_a";
						else col_name = "u_a_1harm";

						if(!Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name], out modulesA[0]))
						{
							existsU1_ = false;

							if (curDevType_ != EmDeviceType.ETPQP_A)
								col_name = "u1_ab";
							else col_name = "u_ab_1harm";

							if(!Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name], out modulesA[0]))
							{
								modulesA[0] = 0;
							}
						}
					}
					catch { modulesA[0] = 0; }

					// I1_A
					try
					{
						col_name = "i1_a";
						if (curDevType_ == EmDeviceType.ETPQP_A)
						{
							col_name = "i_a_1harm";
						}
						modulesA[1] = Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name]);
					}
					catch { modulesA[1] = 0; }

					// <U1aU1b
					try
					{
						col_name = "an_u1_a_u1_b";
						if (curDevType_ == EmDeviceType.ETPQP_A)
						{
							col_name = "angle_ua_ub";
						}
						anglesA[0] = Conversions.object_2_double((CMs_[(int)AvgPages.ANGLES].Current as DataRowView).Row[col_name]);
					}
					catch { anglesA[0] = 0; }

					// <U1aI1a
					try
					{
						col_name = "an_u1_a_i1_a";
						if (curDevType_ == EmDeviceType.ETPQP_A)
						{
							col_name = "angle_ua_ia";
						}
						anglesA[1] = Conversions.object_2_double((CMs_[(int)AvgPages.ANGLES].Current as DataRowView).Row[col_name]);
					}
					catch { anglesA[1] = 0; }

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// U1_B
						try
						{
							if (curDevType_ != EmDeviceType.ETPQP_A)
								col_name = "u1_b";
							else col_name = "u_b_1harm";

							if (!Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name], out modulesB[0]))
							{
								existsU1_ = false;

								if (curDevType_ != EmDeviceType.ETPQP_A)
									col_name = "u1_bc";
								else col_name = "u_bc_1harm";

								if (!Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name], out modulesB[0]))
								{
									modulesB[0] = 0;
								}
							}
						}
						catch { modulesB[0] = 0; }

						// I1_B
						try
						{
							col_name = "i1_b";
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								col_name = "i_b_1harm";
							}
							modulesB[1] = Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name]);
						}
						catch { modulesB[1] = 0; }

						// <U1bU1c
						try
						{
							col_name = "an_u1_b_u1_c";
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								col_name = "angle_ub_uc";
							}
							anglesB[0] = Conversions.object_2_double((CMs_[(int)AvgPages.ANGLES].Current as DataRowView).Row[col_name]);
						}
						catch { anglesB[0] = 0; }

						// <U1bI1b
						try
						{
							col_name = "an_u1_b_i1_b";
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								col_name = "angle_ub_ib";
							}
							anglesB[1] = Conversions.object_2_double((CMs_[(int)AvgPages.ANGLES].Current as DataRowView).Row[col_name]);
						}
						catch { anglesB[1] = 0; }

						// U1_C
						try
						{
							if (curDevType_ != EmDeviceType.ETPQP_A)
								col_name = "u1_c";
							else col_name = "u_c_1harm";

							if (!Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name], out modulesC[0]))
							{
								existsU1_ = false;

								if (curDevType_ != EmDeviceType.ETPQP_A)
									col_name = "u1_ca";
								else col_name = "u_ca_1harm";

								if (!Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name], out modulesC[0]))
								{
									modulesC[0] = 0;
								}
							}
						}
						catch { modulesC[0] = 0; }

						// I1_C	
						try
						{
							col_name = "i1_c";
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								col_name = "i_c_1harm";
							}
							modulesC[1] = Conversions.object_2_double((CMs_[(int)AvgPages.F_U_I].Current as DataRowView).Row[col_name]);
						}
						catch { modulesC[1] = 0; }

						// <U1cU1a
						try
						{
							col_name = "an_u1_c_u1_a";
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								col_name = "angle_uc_ua";
							}
							anglesC[0] = Conversions.object_2_double((CMs_[(int)AvgPages.ANGLES].Current as DataRowView).Row[col_name]);
						}
						catch { anglesC[0] = 0; }

						// <U1cI1c
						try
						{
							col_name = "an_u1_c_i1_c";
							if (curDevType_ == EmDeviceType.ETPQP_A)
							{
								col_name = "angle_uc_ic";
							}
							anglesC[1] = Conversions.object_2_double((CMs_[(int)AvgPages.ANGLES].Current as DataRowView).Row[col_name]);
						}
						catch { anglesC[1] = 0; }
					}

					if (connectScheme_ != connectSchemeOld_)
					{
						radialGraph.CurveList.Clear();

						radialGraph.CurveList.Add(modulesA, anglesA, Color.Gold);
						radialGraph.CurveList[0].AddLegend(String.Empty);
						radialGraph.CurveList[0].AddLegend(String.Empty);

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							radialGraph.CurveList.Add(modulesB, anglesB, Color.Green);
							radialGraph.CurveList[1].AddLegend(String.Empty);
							radialGraph.CurveList[1].AddLegend(String.Empty);

							radialGraph.CurveList.Add(modulesC, anglesC, Color.Red);
							radialGraph.CurveList[2].AddLegend(String.Empty);
							radialGraph.CurveList[2].AddLegend(String.Empty);
						}
					}

					///////////////////////////////////////////

					modulesA[0] /= settings_.VoltageRatio;
					modulesB[0] /= settings_.VoltageRatio;
					modulesC[0] /= settings_.VoltageRatio;

					modulesA[1] /= settings_.CurrentRatio;
					modulesB[1] /= settings_.CurrentRatio;
					modulesC[1] /= settings_.CurrentRatio;

					///////////////////////////////////////////

					// setting values
					radialGraph.CurveList[0].VectorPairList[0].Module = modulesA[0];
					radialGraph.CurveList[0].VectorPairList[1].Module = modulesA[1];
					radialGraph.CurveList[0].VectorPairList[0].Angle = 0;
					radialGraph.CurveList[0].VectorPairList[1].Angle = anglesA[1];

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						radialGraph.CurveList[1].VectorPairList[0].Module = modulesB[0];
						radialGraph.CurveList[1].VectorPairList[1].Module = modulesB[1];
						radialGraph.CurveList[1].VectorPairList[0].Angle = anglesA[0];
						radialGraph.CurveList[1].VectorPairList[1].Angle = anglesA[0] +
																		anglesB[1];

						radialGraph.CurveList[2].VectorPairList[0].Module = modulesC[0];
						radialGraph.CurveList[2].VectorPairList[1].Module = modulesC[1];
						radialGraph.CurveList[2].VectorPairList[0].Angle = anglesA[0] +
																		anglesB[0];
						radialGraph.CurveList[2].VectorPairList[1].Angle = anglesA[0] +
																anglesB[0] + anglesC[1];

						//Service.SDebug.Out("modulesB[0]__________________" + modulesB[0].ToString());
					}

					// getting ratios suffixes
					string v_suffix = string.Empty;
					//	if (settings.VoltageRatio == 1)
					//	{
					v_suffix = emstrings.column_header_units_v.Remove(0, 2);
					//	}
					//	else if (settings.VoltageRatio == 0.001F)
					//	{
					//		v_suffix = emstrings.column_header_units_kv.Remove(0, 2);
					//	}

					string c_suffix = string.Empty;
					//	if (settings.CurrentRatio == 1)
					//	{
					c_suffix = emstrings.column_header_units_a.Remove(0, 2);
					//	}
					//	else if (settings.VoltageRatio == 0.001F)
					//	{
					//		c_suffix = emstrings.column_header_units_ka.Remove(0, 2);
					//	}


					// appaying format to the chart
					switch (connectScheme_)
					{
						case ConnectScheme.Ph1W2:
							radialGraph.CurveList[0].GetLegend(0).TextPattern = "Ua(1) {0}(" +
										v_suffix + ") ";
							radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
								anglesA[1].ToString("0.000") + "В°; Ia(1) {0}(" + c_suffix + ")";
							break;
						case ConnectScheme.Ph3W3:
						case ConnectScheme.Ph3W3_B_calc:
							radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " +
								anglesA[0].ToString("0.000") + "В°; Uab(1) {0}(" + v_suffix + ") ";
							radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " +
								anglesB[0].ToString("0.000") + "В°; Ubc(1) {0}(" + v_suffix + ") ";
							radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " +
								anglesC[0].ToString("0.000") + "В°; Uca(1) {0}(" + v_suffix + ") ";

							radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
								anglesA[1].ToString("0.000") + "В°; Ia(1) {0}(" + c_suffix + ")";
							radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " +
								anglesB[1].ToString("0.000") + "В°; Ib(1) {0}(" + c_suffix + ")";
							radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " +
								anglesC[1].ToString("0.000") + "В°; Ic(1) {0}(" + c_suffix + ")";
							break;
						case ConnectScheme.Ph3W4:
						case ConnectScheme.Ph3W4_B_calc:
							radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " +
								anglesA[0].ToString("0.000") + "В°; Ua(1) {0}(" + v_suffix + ") ";
							radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " +
								anglesB[0].ToString("0.000") + "В°; Ub(1) {0}(" + v_suffix + ") ";
							radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " +
								anglesC[0].ToString("0.000") + "В°; Uc(1) {0}(" + v_suffix + ") ";

							radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
								anglesA[1].ToString("0.000") + "В°; Ia(1) {0}(" + c_suffix + ")";
							radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " +
								anglesB[1].ToString("0.000") + "В°; Ib(1) {0}(" + c_suffix + ")";
							radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " +
								anglesC[1].ToString("0.000") + "В°; Ic(1) {0}(" + c_suffix + ")";
							break;
					}

					// Getting Nominal voltage
					radialGraph.RadialGridList[0].NominalValue = MainWindow_.CurrentArchive.UNomPhase;
					tsNominalVoltage.Text = radialGraph.RadialGridList[0].NominalValue.ToString();

					connectSchemeOld_ = connectScheme_;

					//radialGraph.Invalidate();
				}
				else
				{
					radialGraph.CurveList.Clear();
					radialGraph.Refresh();
				}

				#region old code
				//if (CMs_[0] != null)
				//{
				//    existsU1_ = true;
				//    radialGraph.RadialGridList.ZeroAngle = 0;
				//    // вот тут она сжимается и как раз в этот момент вызывается инвалидэйт
				//    //radialGraph.CurveList.Clear();

				//    double[] modulesA = new double[2];
				//    double[] anglesA = new double[2];
				//    double[] modulesB = new double[2];
				//    double[] anglesB = new double[2];
				//    double[] modulesC = new double[2];
				//    double[] anglesC = new double[2];

				//    // Раньше для 3ф3пр не было U1_A, поэтому при открытии старого архива 
				//    // из базы проверяем есть ли оно. Если нет, тогда берем U1_AB, 
				//    // как было раньше
				//    float val;
				//    string str_val = (CMs_[0].Current as DataRowView).Row.ItemArray[6].ToString();
				//    if (Single.TryParse(str_val, out val)) 
				//    {
				//        modulesA[0] = val;	// U1_A
				//    }
				//    else	// U1_AB
				//    {
				//        modulesA[0] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[12]);
				//        existsU1_ = false;
				//    }

				//    // I1_A
				//    modulesA[1] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[24]); //27
				//    // <U1aU1b
				//    anglesA[0] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[57]);
				//    // <U1aI1a
				//    anglesA[1] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[60]);	

				//    if (ConnectionScheme != ConnectScheme.Ph1W2)
				//    {
				//        str_val = (CMs_[0].Current as DataRowView).Row.ItemArray[7].ToString();
				//        if (Single.TryParse(str_val, out val))
				//        {
				//            modulesA[0] = val;	// U1_B
				//        }
				//        else	// U1_BC
				//        {
				//            modulesA[0] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[13]);
				//            existsU1_ = false;
				//        }

				//        // I1_C
				//        modulesB[1] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[25]); //28
				//        // <U1bU1c
				//        anglesB[0] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[58]);
				//        // <U1bI1b
				//        anglesB[1] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[61]);

				//        str_val = (CMs_[0].Current as DataRowView).Row.ItemArray[8].ToString();
				//        if (Single.TryParse(str_val, out val))
				//        {
				//            modulesA[0] = val;	// U1_C
				//        }
				//        else	// U1_CA
				//        {
				//            modulesA[0] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[14]);
				//            existsU1_ = false;
				//        }

				//        // I1_C	
				//        modulesC[1] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[26]); //29
				//        // <U1cU1a
				//        anglesC[0] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[59]);
				//        // <U1cI1c
				//        anglesC[1] = (float)((CMs_[0].Current as DataRowView).Row.ItemArray[62]);
				//    }

				//    if (ConnectionScheme != ConnectionSchemeOld)
				//    {
				//        radialGraph.CurveList.Clear();

				//        radialGraph.CurveList.Add(modulesA, anglesA, Color.Gold);
				//        radialGraph.CurveList[0].AddLegend(String.Empty);
				//        radialGraph.CurveList[0].AddLegend(String.Empty);

				//        if (ConnectionScheme != ConnectScheme.Ph1W2)
				//        {
				//            radialGraph.CurveList.Add(modulesB, anglesB, Color.Green);
				//            radialGraph.CurveList[1].AddLegend(String.Empty);
				//            radialGraph.CurveList[1].AddLegend(String.Empty);

				//            radialGraph.CurveList.Add(modulesC, anglesC, Color.Red);
				//            radialGraph.CurveList[2].AddLegend(String.Empty);
				//            radialGraph.CurveList[2].AddLegend(String.Empty);
				//        }
				//    }

				//    ///////////////////////////////////////////

				//    modulesA[0] /= settings.VoltageRatio;
				//    modulesB[0] /= settings.VoltageRatio;
				//    modulesC[0] /= settings.VoltageRatio;

				//    modulesA[1] /= settings.CurrentRatio;
				//    modulesB[1] /= settings.CurrentRatio;
				//    modulesC[1] /= settings.CurrentRatio;

				//    ///////////////////////////////////////////

				//    // setting values
				//    radialGraph.CurveList[0].VectorPairList[0].Module = modulesA[0];
				//    radialGraph.CurveList[0].VectorPairList[1].Module = modulesA[1];
				//    radialGraph.CurveList[0].VectorPairList[0].Angle = 0;
				//    radialGraph.CurveList[0].VectorPairList[1].Angle = anglesA[1];

				//    if (ConnectionScheme != ConnectScheme.Ph1W2)
				//    {
				//        radialGraph.CurveList[1].VectorPairList[0].Module = modulesB[0];
				//        radialGraph.CurveList[1].VectorPairList[1].Module = modulesB[1];
				//        radialGraph.CurveList[1].VectorPairList[0].Angle = anglesA[0];
				//        radialGraph.CurveList[1].VectorPairList[1].Angle = anglesA[0] + 
				//                                                        anglesB[1];

				//        radialGraph.CurveList[2].VectorPairList[0].Module = modulesC[0];
				//        radialGraph.CurveList[2].VectorPairList[1].Module = modulesC[1];
				//        radialGraph.CurveList[2].VectorPairList[0].Angle = anglesA[0] + 
				//                                                        anglesB[0];
				//        radialGraph.CurveList[2].VectorPairList[1].Angle = anglesA[0] + 
				//                                                anglesB[0] + anglesC[1];

				//    }

				//    // getting ratios suffixes
				//    string v_suffix = string.Empty;
				//    if (settings.VoltageRatio == 1)
				//    {
				//        v_suffix = emstrings.column_header_units_v.Remove(0, 2);
				//    }
				//    else if (settings.VoltageRatio == 0.001F)
				//    {
				//        v_suffix = emstrings.column_header_units_kv.Remove(0, 2);
				//    }

				//    string c_suffix = string.Empty;
				//    if (settings.CurrentRatio == 1)
				//    {
				//        c_suffix = emstrings.column_header_units_a.Remove(0, 2);
				//    }
				//    else if (settings.VoltageRatio == 0.001F)
				//    {
				//        c_suffix = emstrings.column_header_units_ka.Remove(0, 2);
				//    }


				//    // appaying format to the chart
				//    switch (ConnectionScheme)
				//    {
				//        case 3:
				//            radialGraph.CurveList[0].GetLegend(0).TextPattern = "Ua(1) {0}(" + 
				//                        v_suffix + ") ";
				//            radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " + 
				//                anglesA[1].ToString("0.000") + "°; Ia(1) {0}(" + c_suffix + ")";
				//            break;
				//        case 2:
				//            radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " + 
				//                anglesA[0].ToString("0.000") + "°; Uab(1) {0}(" + v_suffix + ") ";
				//            radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " + 
				//                anglesB[0].ToString("0.000") + "°; Ubc(1) {0}(" + v_suffix + ") ";
				//            radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " + 
				//                anglesC[0].ToString("0.000") + "°; Uca(1) {0}(" + v_suffix + ") ";

				//            radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " + 
				//                anglesA[1].ToString("0.000") + "°; Ia(1) {0}(" + c_suffix + ")";
				//            radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " + 
				//                anglesB[1].ToString("0.000") + "°; Ib(1) {0}(" + c_suffix + ")";
				//            radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " + 
				//                anglesC[1].ToString("0.000") + "°; Ic(1) {0}(" + c_suffix + ")";
				//            break;
				//        case 1:
				//            radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " + 
				//                anglesA[0].ToString("0.000") + "°; Ua(1) {0}(" + v_suffix + ") ";
				//            radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " + 
				//                anglesB[0].ToString("0.000") + "°; Ub(1) {0}(" + v_suffix + ") ";
				//            radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " + 
				//                anglesC[0].ToString("0.000") + "°; Uc {0}(" + v_suffix + ") ";

				//            radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " + 
				//                anglesA[1].ToString("0.000") + "°; Ia(1) {0}(" + c_suffix + ")";
				//            radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " + 
				//                anglesB[1].ToString("0.000") + "°; Ib(1) {0}(" + c_suffix + ")";
				//            radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " + 
				//                anglesC[1].ToString("0.000") + "°; Ic(1) {0}(" + c_suffix + ")";
				//            break;
				//    }

				//    // Getting Nominal voltage
				//    radialGraph.RadialGridList[0].NominalValue = (MainWindow.wndToolbox.tvArchives.ActiveNodeAVG.Parent.Parent as EnergomonitoringXP.ArchiveTreeView.ArchiveTreeNode).NominalPhaseVoltage;
				//    tsNominalVoltage.Text = radialGraph.RadialGridList[0].NominalValue.ToString();

				//    ConnectionSchemeOld = ConnectionScheme;

				//    //radialGraph.Invalidate();
				//}
				//else
				//{
				//    radialGraph.CurveList.Clear();
				//    radialGraph.Refresh();
				//}
				#endregion

				#endregion
			}
			catch (ArgumentException aex)
			{
				EmService.DumpException(aex, "ArgumentException in UpdateGraphPanel() 1");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in UpdateGraphPanel(): ");
				throw;
			}
		}
		
		private void GraphPanelSmthWasChanged(object sender, EventArgs e)
		{
			UpdateGraphPanel(bHarmPowersPercentShown_, bHarmCurrentPercentShown_);
		}

		private void tsScaleMode_Click(object sender, EventArgs e)
		{
			try
			{
				// проверяем, есть ли открытые средние?
				if (MainWindow_.wndToolbox.ActiveNodeAVG == null)
					return;										// если нет - выходим

				// в случае необходимости задаем новое значение номинала тока
				if (this.radialGraph.RadialGridList[1].NominalValue != (double)tsNominalCurrent.Tag)
				{
					this.radialGraph.RadialGridList[1].NominalValue = (double)tsNominalCurrent.Tag;
				}
				// вкл/выкл масштабирования
				if (this.radialGraph.RealValues != tsScaleMode.Checked)
				{
					this.radialGraph.RealValues = tsScaleMode.Checked;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsScaleMode_Click():");
				throw;
			}
		}

		private void tsNominalCurrent_Leave(object sender, EventArgs e)
		{
			try
			{
				if (tsNominalCurrent.Tag != null)		// если прежнее значение было,
					tsNominalCurrent.Text = tsNominalCurrent.Tag.ToString();  // восстанавливаем
				else
					tsNominalCurrent.Text = string.Empty;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsNominalCurrent_Leave():");
				throw;
			}
		}

		private void tsNominalCurrent_KeyUp(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == Keys.Enter)		// если подтверждаем ввод числа
				{
					double tag;
					if (Double.TryParse(tsNominalCurrent.Text, out tag))
					{
						tsNominalCurrent.Tag = tag;		// сохраняем это число на будующее
						// и в случае необходимости делаем доступным кнопку Масштабирования
						if (!tsScaleMode.Enabled)
						{
							if ((connectScheme_ != ConnectScheme.Ph3W3 &&
								connectScheme_ != ConnectScheme.Ph3W4_B_calc)
								|| existsU1_)
								tsScaleMode.Enabled = true;
						}
						if (tsScaleMode.Checked)
						{
							tsScaleMode_Click(tsScaleMode, EventArgs.Empty);
						}
						toolStrip1.Focus();
					}
					else   // если число введено неверно
					{
						// восстанавливаем прежнее значение
						tsNominalCurrent.Text = tsNominalCurrent.Tag.ToString();
					}
				}
				else if (e.KeyCode == Keys.Escape)		// по "escape"
				{							// также восстанавливаем прежнее значение
					tsNominalCurrent.Text = tsNominalCurrent.Tag.ToString();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsNominalCurrent_KeyUp():");
				throw;
			}
		}

		private void tsLeft_Click(object sender, EventArgs e)
		{
			try
			{
				splitTopVert.Panel1Collapsed = !tsLeft.Checked;

				if (!tsLeft.Checked && !tsRight.Checked)
				{
					tsBottom.Checked = true;
					splitContMain.Panel1Collapsed = true;
				}

				if (tsLeft.Checked && splitContMain.Panel1Collapsed)
				{
					splitContMain.Panel1Collapsed = false;
					splitTopVert.Panel2Collapsed = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsLeft_Click():");
				throw;
			}
		}

		private void tsRight_Click(object sender, EventArgs e)
		{
			try
			{
				splitTopVert.Panel2Collapsed = !tsRight.Checked;

				if (!tsLeft.Checked && !tsRight.Checked)
				{
					tsBottom.Checked = true;
					splitContMain.Panel1Collapsed = true;
				}

				if (tsRight.Checked && splitContMain.Panel1Collapsed)
				{
					splitContMain.Panel1Collapsed = false;
					splitTopVert.Panel1Collapsed = true;
				}

				//tsLeft.Checked = !splitTopVert.Panel1Collapsed;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsRight_Click():");
				throw;
			}
		}

		private void tsBottom_Click(object sender, EventArgs e)
		{
			try
			{
				splitContMain.Panel2Collapsed = !tsBottom.Checked;

				tsLeft.Checked = !splitTopVert.Panel1Collapsed;
				tsRight.Checked = !splitTopVert.Panel2Collapsed;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsBottom_Click():");
				throw;
			}
		}

		private void radialGraph_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Right)
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
												this.GetType().Assembly);
					tsmiCopy.Text = rm.GetString("cm_copy");
					tsmiSaveAs.Text = rm.GetString("save_image_as");
					tsmiPrintImage.Text = rm.GetString("print_image");
					cmsForImage.Show(radialGraph.PointToScreen(new Point(e.X, e.Y)));
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in radialGraph_MouseClick():");
				throw;
			}
		}

		private void tsmiCopy_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetDataObject(this.radialGraph.Image, true);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsmiCopy_Click():");
				throw;
			}
		}

		private void tsmiSaveAs_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialog saveDlg = new SaveFileDialog();
				saveDlg.Filter = "Jpeg Format (*.jpg)|*.jpg|" +
								"PNG Format (*.png)|*.png|" +
								"Gif Format (*.gif)|*.gif|" +
								"Tiff Format (*.tif)|*.tif|" +
								"Bmp Format (*.bmp)|*.bmp";

				if (saveDlg.ShowDialog() != DialogResult.OK)
					return;

				ImageFormat format = ImageFormat.Jpeg;
				if (saveDlg.FilterIndex == 2)
					format = ImageFormat.Gif;
				else if (saveDlg.FilterIndex == 3)
					format = ImageFormat.Png;
				else if (saveDlg.FilterIndex == 4)
					format = ImageFormat.Tiff;
				else if (saveDlg.FilterIndex == 5)
					format = ImageFormat.Bmp;

				Stream myStream = saveDlg.OpenFile();
				if (myStream != null)
				{
					radialGraph.Image.Save(myStream, format);
					myStream.Close();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsmiSaveAs_Click():");
				throw;
			}
		}

		private void tsmiPrintImage_Click(object sender, EventArgs e)
		{
			try
			{
				printDlg.Document = printDocument;
				if (printDlg.ShowDialog() == DialogResult.OK)
				{
					printDocument.Print();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tsmiPrintImage_Click():");
				throw;
			}
		}

		private void printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			try
			{
				radialGraph.fillGraphics(e.Graphics, radialGraph.Width, radialGraph.Height);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in printDocument_PrintPage():");
				throw;
			}
		}

        private void tsNominalCurrent_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                char c = e.KeyChar;
                bool bDelimiterUsed = (tsNominalCurrent.Text.IndexOf('.') >= 0) ||
                                        (tsNominalCurrent.Text.IndexOf(',') >= 0);
                if (!bDelimiterUsed)
                    e.Handled = !(char.IsDigit(c) || c == '.' || c == ',' || c == '\b');
                else
                    e.Handled = !(char.IsDigit(c) || c == '\b');
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in tsNominalCurrent_KeyPress():");
                throw;
            }
        }

        private void tsNominalVoltage_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                char c = e.KeyChar;
                bool bDelimiterUsed = (tsNominalVoltage.Text.IndexOf('.') >= 0) ||
                                        (tsNominalVoltage.Text.IndexOf(',') >= 0);
                if (!bDelimiterUsed)
                    e.Handled = !(char.IsDigit(c) || c == '.' || c == ',' || c == '\b');
                else
                    e.Handled = !(char.IsDigit(c) || c == '\b');
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in tsNominalVoltage_KeyPress():");
                throw;
            }
        }

		public ConnectScheme CurConnectScheme
		{
			get { return connectScheme_; }
			set { connectScheme_ = value; }
		}
	}
}