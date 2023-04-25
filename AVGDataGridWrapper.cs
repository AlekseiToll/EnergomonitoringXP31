using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using System.Resources;

using EmServiceLib;
using DbServiceLib;
using EmDataSaver;
using DataGridColumnStyles;

namespace EnergomonitoringXP
{
	internal abstract class AVGDataGridWrapperBase
	{
		protected DataGrid dataGrid_;
		protected EmDeviceType devType_;
		//protected int pgServerIndex_;
		protected Settings settings_;
		protected Int64 curDatetimeId_;
		protected ConnectScheme connectScheme_;
		protected frmDocAVGMain mainWnd_;

		// это поле нужно для исключения ошибки при двойном клике на разделитель строк датагрида.
		// чтобы исключить ошибку, нужно убрать лишние стили колонок, но на всякий случай они сохраняются
		// в этих списках
		protected List<DataGridColumnStyle> missingColumnStyles_ = new List<DataGridColumnStyle>();

		public DataGrid DGrid { get { return dataGrid_; } }

		public AVGDataGridWrapperBase(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
		{
			settings_ = settings;
			devType_ = devType;
			dataGrid_ = dg;
			curDatetimeId_ = curDatetimeId;
			connectScheme_ = conScheme;
			mainWnd_ = mainWnd;
		}

		public void init()
		{
			DbService dbService = null;
			try
			{
				dbService = new DbService(GetPgConnectionString(devType_, settings_.CurServerIndex));
				dbService.Open();

				if (devType_ == EmDeviceType.ETPQP_A) init_etPQP_A(ref dbService);
				else init_not_etPQP_A(ref dbService);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperBase::init() ");
				return;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		protected abstract void init_not_etPQP_A(ref DbService dbService);
		protected abstract void init_etPQP_A(ref DbService dbService);

		public void load(bool curWithTR, bool showInPercent)
		{
			DbService dbService = null;
			try
			{
				dbService = new DbService(GetPgConnectionString(devType_, settings_.CurServerIndex));
				dbService.Open();

				if (devType_ == EmDeviceType.ETPQP_A)
					load_etPQP_A(ref dbService, curWithTR, showInPercent);
				else load_not_etPQP_A(ref dbService, curWithTR, showInPercent);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperBase::load() ");
				return;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}
		protected abstract void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent);
		protected abstract void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent);

		protected void GetMask(out Int64 iMask, out Int64 iMask2, ref DbService dbService, string table)
		{
			iMask = -1; iMask2 = -1;
			string commandText;
			try
			{
				// у Em32 и EtPQP возможны неполные архивы, поэтому считываем маску, чтобы
				// определить какие колонки таблицы заполнены
				if (devType_ == EmDeviceType.EM32 || devType_ == EmDeviceType.ETPQP)
				{
					commandText = String.Format(
						"SELECT max(mask1) from {0} WHERE datetime_id = {1};",
						table, curDatetimeId_);
					object oMask = dbService.ExecuteScalar(commandText);
					try
					{
						iMask = (Int64)oMask;
					}
					catch
					{
						EmService.WriteToLogFailed(
							"AVGDataGridWrapperBase::GetMask(): Invalid Mask1 format!");
						iMask = -1;
					}

					commandText = String.Format(
						"SELECT max(mask2) from {0} WHERE datetime_id = {1};",
						table, curDatetimeId_);
					oMask = dbService.ExecuteScalar(commandText);
					try
					{
						iMask2 = (Int64)oMask;
					}
					catch
					{
						EmService.WriteToLogFailed("AVGDataGridWrapperBase::GetMask(): Invalid Mask2 format!");
						iMask2 = -1;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperBase::GetMask() ");
				throw;
			}
		}

		protected string GetPgConnectionString(EmDeviceType devType, int PgServerIndex)
		{
			try
			{
				switch (devType)
				{
					case EmDeviceType.EM32:
						return settings_.PgServers[PgServerIndex].PgConnectionStringEm32;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
						return settings_.PgServers[PgServerIndex].PgConnectionStringEm33;
					case EmDeviceType.ETPQP:
						return settings_.PgServers[PgServerIndex].PgConnectionStringEtPQP;
					case EmDeviceType.ETPQP_A:
						return settings_.PgServers[PgServerIndex].PgConnectionStringEtPQP_A;
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in GetPgConnectionString(): " + ex.Message);
				return string.Empty;
			}
		}

		public static string GetPgConnectionString(EmDeviceType devType, int PgServerIndex, ref Settings settings)
		{
			try
			{
				switch (devType)
				{
					case EmDeviceType.EM32:
						return settings.PgServers[PgServerIndex].PgConnectionStringEm32;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
						return settings.PgServers[PgServerIndex].PgConnectionStringEm33;
					case EmDeviceType.ETPQP:
						return settings.PgServers[PgServerIndex].PgConnectionStringEtPQP;
					case EmDeviceType.ETPQP_A:
						return settings.PgServers[PgServerIndex].PgConnectionStringEtPQP_A;
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in GetPgConnectionString(): " + ex.Message);
				return string.Empty;
			}
		}

		/// <summary>
		/// Adjust the grid's definition to accept the given DataTable.
		/// я нашел этот метод здесь http://stackoverflow.com/questions/1174820/how-do-i-handle-an-exception-caused-by-double-clicking-between-rows-in-a-datagri
		/// он нужен, чтобы избежать исключения при двойном клике на разделитель строк таблицы
		/// (исключение связано с тем, что в таблице нет некоторых столбцов, которые есть в стилях таблицы)
		/// </summary>
		public void AdjustGridForData(DataTable table)
		{
			string name = "";
			try
			{
				// Remove column styles whose mapped columns are missing.
				// This fixes the notorious, uncatchable exception caused when 
				// double-clicking row borders.
				GridColumnStylesCollection columnStyles =
					dataGrid_.TableStyles[table.TableName].GridColumnStyles;

				List<DataGridColumnStyle> curMissingColumnStyles = missingColumnStyles_;

				if (curMissingColumnStyles == null) return;

				// Add previously removed column styles back to the grid, in case the new bound table
				// has something we removed last time this method was executed.
				foreach (DataGridColumnStyle missingColumnStyle in curMissingColumnStyles)
				{
					name = missingColumnStyle.MappingName;

					bool contains = false;
					foreach (DataGridColumnStyle curStyle in columnStyles)
					{
						if (curStyle.MappingName == name)
						{
							contains = true;
							break;
						}
					}

					if (!columnStyles.Contains(missingColumnStyle) && !contains)
						columnStyles.Add(missingColumnStyle);
				}
				curMissingColumnStyles.Clear();

				// Move the offending column styles into a separate list.
				List<string> missingColumns = new List<string>();
				foreach (DataGridColumnStyle style in columnStyles)
				{
					if (!table.Columns.Contains(style.MappingName))
						missingColumns.Add(style.MappingName);
				}
				foreach (string column in missingColumns)
				{
					curMissingColumnStyles.Add(columnStyles[column]);
					columnStyles.Remove(columnStyles[column]);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AdjustGridForData() " + name);
				throw;
			}
		}
	}

	/// <summary> U, I, F</summary>
	internal class AVGDataGridWrapperUIF : AVGDataGridWrapperBase
	{
		float iLimit_;

		public AVGDataGridWrapperUIF(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme, float iLimit,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ iLimit_ = iLimit; }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_hz = rm.GetString("column_header_units_hz");
				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");
				string unit_a = (settings_.CurrentRatio == 1) ?
					rm.GetString("column_header_units_a") :
					rm.GetString("column_header_units_ka");

				dataGrid_.TableStyles.Clear();
				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "avg_u_i_f";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				//cs_marked.Format = "D";
				ts.GridColumnStyles.Add(cs_marked);

				if (connectScheme_ != ConnectScheme.Ph3W3 && connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// f_a
					//DataGridColumnHeaderFormula cs_f_a =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "F_A_" + unit_hz);
					//cs_f_a.MappingName = "f_a";
					//cs_f_a.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_f_a);

					//if (connectScheme_ == ConnectScheme.Ph3W4 ||
					//    connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					//{
					//    // f_b
					//    DataGridColumnHeaderFormula cs_f_b =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "F_B_" + unit_hz);
					//    cs_f_b.MappingName = "f_b";
					//    cs_f_b.Width = DataColumnsWidth.CommonWidth;
					//    ts.GridColumnStyles.Add(cs_f_b);

					//    // f_c
					//    DataGridColumnHeaderFormula cs_f_c =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "F_C_" + unit_hz);
					//    cs_f_c.MappingName = "f_c";
					//    cs_f_c.Width = DataColumnsWidth.CommonWidth;
					//    ts.GridColumnStyles.Add(cs_f_c);
					//}
				}
				else
				{
					// f_ab
					//DataGridColumnHeaderFormula cs_f_ab =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "F_AB_" + unit_hz);
					//cs_f_ab.MappingName = "f_ab";
					//cs_f_ab.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_f_ab);

					//// f_bc
					//DataGridColumnHeaderFormula cs_f_bc =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "F_BC_" + unit_hz);
					//cs_f_bc.MappingName = "f_bc";
					//cs_f_bc.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_f_bc);

					//// f_ca
					//DataGridColumnHeaderFormula cs_f_ca =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "F_CA_" + unit_hz);
					//cs_f_ca.MappingName = "f_ca";
					//cs_f_ca.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_f_ca);
				}

				// Напряжение – действующие значения
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U_A
					DataGridColumnHeaderFormula cs_U_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_A RMS_" + unit_v);
					cs_U_A.MappingName = "u_a";
					cs_U_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_A);

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// U_B
						DataGridColumnHeaderFormula cs_U_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_B RMS_" + unit_v);
						cs_U_B.MappingName = "u_b";
						cs_U_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U_B);

						// U_C
						DataGridColumnHeaderFormula cs_U_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_C RMS_" + unit_v);
						cs_U_C.MappingName = "u_c";
						cs_U_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U_C);
					}
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U_AB
					DataGridColumnHeaderFormula cs_U_AB =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_AB RMS_" + unit_v);
					cs_U_AB.MappingName = "u_ab";
					cs_U_AB.Width = DataColumnsWidth.CommonWidth; ;
					ts.GridColumnStyles.Add(cs_U_AB);

					// U_BC
					DataGridColumnHeaderFormula cs_U_BC =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_BC RMS_" + unit_v);
					cs_U_BC.MappingName = "u_bc";
					cs_U_BC.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_BC);

					// U_CA
					DataGridColumnHeaderFormula cs_U_CA =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_CA RMS_" + unit_v);
					cs_U_CA.MappingName = "u_ca";
					cs_U_CA.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_CA);
				}

				// Напряжение – 1-я гармоника
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U1_A
					DataGridColumnHeaderFormula cs_U1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_A (1)_" + unit_v);
					cs_U1_A.MappingName = "u_a_1harm";
					cs_U1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_A);

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// U1_B
						DataGridColumnHeaderFormula cs_U1_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_B (1)_" + unit_v);
						cs_U1_B.MappingName = "u_b_1harm";
						cs_U1_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U1_B);

						// U1_C
						DataGridColumnHeaderFormula cs_U1_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_C (1)_" + unit_v);
						cs_U1_C.MappingName = "u_c_1harm";
						cs_U1_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U1_C);
					}
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U1_AB
					DataGridColumnHeaderFormula cs_U1_AB =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_AB (1)_" + unit_v);
					cs_U1_AB.MappingName = "u_ab_1harm";
					cs_U1_AB.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_AB);

					// U1_BC
					DataGridColumnHeaderFormula cs_U1_BC =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_BC (1)_" + unit_v);
					cs_U1_BC.MappingName = "u_bc_1harm";
					cs_U1_BC.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_BC);

					// U1_CA
					DataGridColumnHeaderFormula cs_U1_CA =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_CA (1)_" + unit_v);
					cs_U1_CA.MappingName = "u_ca_1harm";
					cs_U1_CA.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_CA);
				}

				// Напряжение – постоянная составляющая
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U_A
					DataGridColumnHeaderFormula cs_U_A_const =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_A 0_" + unit_v);
					cs_U_A_const.MappingName = "u_a_const";
					cs_U_A_const.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_A_const);

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// U_B
						DataGridColumnHeaderFormula cs_U_B_const =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_B 0_" + unit_v);
						cs_U_B_const.MappingName = "u_b_const";
						cs_U_B_const.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U_B_const);

						// U_C
						DataGridColumnHeaderFormula cs_U_C_const =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_C 0_" + unit_v);
						cs_U_C_const.MappingName = "u_c_const";
						cs_U_C_const.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U_C_const);
					}
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U_AB
					DataGridColumnHeaderFormula cs_U_AB_const =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_AB 0_" + unit_v);
					cs_U_AB_const.MappingName = "u_ab_const";
					cs_U_AB_const.Width = DataColumnsWidth.CommonWidth; ;
					ts.GridColumnStyles.Add(cs_U_AB_const);

					// U_BC
					DataGridColumnHeaderFormula cs_U_BC_const =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_BC 0_" + unit_v);
					cs_U_BC_const.MappingName = "u_bc_const";
					cs_U_BC_const.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_BC_const);

					// U_CA
					DataGridColumnHeaderFormula cs_U_CA_const =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_CA 0_" + unit_v);
					cs_U_CA_const.MappingName = "u_ca_const";
					cs_U_CA_const.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_CA_const);
				}

				// Напряжение – средневыпрямленное значение
				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U_A
					DataGridColumnHeaderFormula cs_U_A_avdirect =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "U_A ср.в._" : "U_A av.-r._") + unit_v);
					cs_U_A_avdirect.MappingName = "u_a_avdirect";
					cs_U_A_avdirect.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_A_avdirect);

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// U_B
						DataGridColumnHeaderFormula cs_U_B_avdirect =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "U_B ср.в._" : "U_B av.-r._") + unit_v);
						cs_U_B_avdirect.MappingName = "u_b_avdirect";
						cs_U_B_avdirect.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U_B_avdirect);

						// U_C
						DataGridColumnHeaderFormula cs_U_C_avdirect =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "U_C ср.в._" : "U_C av.-r._") + unit_v);
						cs_U_C_avdirect.MappingName = "u_c_avdirect";
						cs_U_C_avdirect.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_U_C_avdirect);
					}
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U_AB
					DataGridColumnHeaderFormula cs_U_AB_avdirect =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "U_AB ср.в._" : "U_AB av.-r._") + unit_v);
					cs_U_AB_avdirect.MappingName = "u_ab_avdirect";
					cs_U_AB_avdirect.Width = DataColumnsWidth.CommonWidth; ;
					ts.GridColumnStyles.Add(cs_U_AB_avdirect);

					// U_BC
					DataGridColumnHeaderFormula cs_U_BC_avdirect =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "U_BC ср.в._" : "U_BC av.-r._") + unit_v);
					cs_U_BC_avdirect.MappingName = "u_bc_avdirect";
					cs_U_BC_avdirect.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_BC_avdirect);

					// U_CA
					DataGridColumnHeaderFormula cs_U_CA_avdirect =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "U_CA ср.в._" : "U_CA av.-r._") + unit_v);
					cs_U_CA_avdirect.MappingName = "u_ca_avdirect";
					cs_U_CA_avdirect.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_CA_avdirect);
				}

				if (iLimit_ != 0)
				{
					// ток – действующие значения
					// I_A
					DataGridColumnHeaderFormula cs_I_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "I_A RMS_" + unit_a);
					cs_I_A.MappingName = "i_a";
					cs_I_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_A);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// I_B
						DataGridColumnHeaderFormula cs_I_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "I_B RMS_" + unit_a);
						cs_I_B.MappingName = "i_b";
						cs_I_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_B);

						// I_C
						DataGridColumnHeaderFormula cs_I_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "I_C RMS_" + unit_a);
						cs_I_C.MappingName = "i_c";
						cs_I_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_C);
					}
					if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc ||
						connectScheme_ == ConnectScheme.Ph3W3)
					{
						// I_N
						DataGridColumnHeaderFormula cs_I_N =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_N RMS_" + unit_a);
						cs_I_N.MappingName = "i_n";
						cs_I_N.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_N);
					}

					// ток – постоянная составляющая
					// I_A
					DataGridColumnHeaderFormula cs_I_A_const =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "I_A 0_" + unit_a);
					cs_I_A_const.MappingName = "i_a_const";
					cs_I_A_const.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_A_const);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// I_B
						DataGridColumnHeaderFormula cs_I_B_const =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "I_B 0_" + unit_a);
						cs_I_B_const.MappingName = "i_b_const";
						cs_I_B_const.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_B_const);

						// I_C
						DataGridColumnHeaderFormula cs_I_C_const =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "I_C 0_" + unit_a);
						cs_I_C_const.MappingName = "i_c_const";
						cs_I_C_const.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_C_const);
					}
					if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc ||
						connectScheme_ == ConnectScheme.Ph3W3)
					{
						// I_N
						DataGridColumnHeaderFormula cs_I_N_const =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_N 0_" + unit_a);
						cs_I_N_const.MappingName = "i_n_const";
						cs_I_N_const.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_N_const);
					}

					// ток – средневыпрямленное значение
					// I_A
					DataGridColumnHeaderFormula cs_I_A_avdirect =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								(settings_.CurrentLanguage.Equals("ru") ? "I_A ср.в._" : "I_A av.-r._") + unit_a);
					cs_I_A_avdirect.MappingName = "i_a_avdirect";
					cs_I_A_avdirect.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_A_avdirect);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// I_B
						DataGridColumnHeaderFormula cs_I_B_avdirect =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								(settings_.CurrentLanguage.Equals("ru") ? "I_B ср.в._" : "I_B av.-r._") + unit_a);
						cs_I_B_avdirect.MappingName = "i_b_avdirect";
						cs_I_B_avdirect.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_B_avdirect);

						// I_C
						DataGridColumnHeaderFormula cs_I_C_avdirect =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								(settings_.CurrentLanguage.Equals("ru") ? "I_C ср.в._" : "I_C av.-r._") + unit_a);
						cs_I_C_avdirect.MappingName = "i_c_avdirect";
						cs_I_C_avdirect.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_C_avdirect);
					}
					if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc ||
						connectScheme_ == ConnectScheme.Ph3W3)
					{
						// I_N
						DataGridColumnHeaderFormula cs_I_N_avdirect =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
								(settings_.CurrentLanguage.Equals("ru") ? "I_N ср.в._" : "I_N av.-r._") + unit_a);
						cs_I_N_avdirect.MappingName = "i_n_avdirect";
						cs_I_N_avdirect.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_N_avdirect);
					}

					// ток – 1-я гармоника
					// I1_A
					DataGridColumnHeaderFormula cs_I1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "I_A (1)_" + unit_a);
					cs_I1_A.MappingName = "i_a_1harm";
					cs_I1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I1_A);

					if (connectScheme_ < ConnectScheme.Ph1W2)
					{
						// I1_B
						DataGridColumnHeaderFormula cs_I1_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "I_B (1)_" + unit_a);
						cs_I1_B.MappingName = "i_b_1harm";
						cs_I1_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I1_B);

						// I1_C
						DataGridColumnHeaderFormula cs_I1_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "I_C (1)_" + unit_a);
						cs_I1_C.MappingName = "i_c_1harm";
						cs_I1_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I1_C);
					}
					if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc ||
						connectScheme_ == ConnectScheme.Ph3W3)
					{
						// I1_N
						DataGridColumnHeaderFormula cs_I1_N =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_N (1)_" + unit_a);
						cs_I1_N.MappingName = "i_n_1harm";
						cs_I1_N.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I1_N);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);

				//DataTable dt = ((DataSet)dgCurrentsVoltages.DataSource).Tables[0];
				//DateTime dtBegin = (DateTime)(dt.Rows[0].ItemArray.GetValue(1));
				//DateTime dtEnd   = (DateTime)(dt.Rows[dt.Rows.Count - 1].ItemArray.GetValue(1));
				DateTime dtBegin = DateTime.MinValue;
				DateTime dtEnd = DateTime.MaxValue;

				string commandText = String.Format("SELECT min(dt_start) FROM avg_service_info WHERE datetime_id = {0};", curDatetimeId_);
				object oDtStart = dbService.ExecuteScalar(commandText);
				if (!(oDtStart is DBNull))
				{
					dtBegin = (DateTime)oDtStart;
				}
				commandText = String.Format("SELECT max(dt_start) FROM avg_service_info WHERE datetime_id = {0};", curDatetimeId_);
				object oDtEnd = dbService.ExecuteScalar(commandText);
				if (!(oDtEnd is DBNull))
				{
					dtEnd = (DateTime)oDtEnd;
				}

				//................................................................
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MinDate =
					new DateTime(1753, 1, 1, 0, 0, 0); // min DateTimePicker date
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MaxDate =
					new DateTime(9998, 12, 31, 0, 0, 0); // max DateTimePicker date

				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MinDate =
					new DateTime(1753, 1, 1, 0, 0, 0); // min DateTimePicker date
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MaxDate =
					new DateTime(9998, 12, 31, 0, 0, 0); // max DateTimePicker date
				//................................................................
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MinDate = dtBegin;
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MaxDate = dtEnd;

				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MinDate = dtBegin;
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MaxDate = dtEnd;

				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.Value = dtBegin;
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.Value = dtEnd;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperUIF::init_etPQP_A()");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_hz = rm.GetString("column_header_units_hz");
				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");
				string unit_a = (settings_.CurrentRatio == 1) ?
					rm.GetString("column_header_units_a") :
					rm.GetString("column_header_units_ka");

				dataGrid_.TableStyles.Clear();
				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "period_avg_params_1_4";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// f
				DataGridColumnHeaderFormula cs_f = new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_f.HeaderText = "F" + unit_hz;
				cs_f.MappingName = "f";
				cs_f.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_f);

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U_A
					DataGridColumnHeaderFormula cs_U_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_A_" + unit_v);
					cs_U_A.MappingName = "u_a";
					cs_U_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_A);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// U_B
					DataGridColumnHeaderFormula cs_U_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_B_" + unit_v);
					cs_U_B.MappingName = "u_b";
					cs_U_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_B);

					// U_C
					DataGridColumnHeaderFormula cs_U_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_C_" + unit_v);
					cs_U_C.MappingName = "u_c";
					cs_U_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_C);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U1_A
					DataGridColumnHeaderFormula cs_U1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_A (1)_" + unit_v);
					cs_U1_A.MappingName = "u1_a";
					cs_U1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_A);
				}
				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// U1_B
					DataGridColumnHeaderFormula cs_U1_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_B (1)_" + unit_v);
					cs_U1_B.MappingName = "u1_b";
					cs_U1_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_B);

					// U1_C
					DataGridColumnHeaderFormula cs_U1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_C (1)_" + unit_v);
					cs_U1_C.MappingName = "u1_c";
					cs_U1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_C);
				}
				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U_AB
					DataGridColumnHeaderFormula cs_U_AB =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_AB_" + unit_v);
					cs_U_AB.MappingName = "u_ab";
					cs_U_AB.Width = DataColumnsWidth.CommonWidth; ;
					ts.GridColumnStyles.Add(cs_U_AB);

					// U_BC
					DataGridColumnHeaderFormula cs_U_BC =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_BC_" + unit_v);
					cs_U_BC.MappingName = "u_bc";
					cs_U_BC.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_BC);

					// U_CA
					DataGridColumnHeaderFormula cs_U_CA =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_CA_" + unit_v);
					cs_U_CA.MappingName = "u_ca";
					cs_U_CA.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_CA);

					// U1_AB
					DataGridColumnHeaderFormula cs_U1_AB =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_AB (1)_" + unit_v);
					cs_U1_AB.MappingName = "u1_ab";
					cs_U1_AB.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_AB);

					// U1_BC
					DataGridColumnHeaderFormula cs_U1_BC =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_BC (1)_" + unit_v);
					cs_U1_BC.MappingName = "u1_bc";
					cs_U1_BC.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_BC);

					// U1_CA
					DataGridColumnHeaderFormula cs_U1_CA =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_CA (1)_" + unit_v);
					cs_U1_CA.MappingName = "u1_ca";
					cs_U1_CA.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1_CA);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U0_A
					DataGridColumnHeaderFormula cs_U0_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "U_A (0)_" + unit_v);
					cs_U0_A.MappingName = "u0_a";
					cs_U0_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U0_A);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// U0_B
					DataGridColumnHeaderFormula cs_U0_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "U_B (0)_" + unit_v);
					cs_U0_B.MappingName = "u0_b";
					cs_U0_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U0_B);

					// U0_C
					DataGridColumnHeaderFormula cs_U0_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "U_C (0)_" + unit_v);
					cs_U0_C.MappingName = "u0_c";
					cs_U0_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U0_C);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// U_hp A
					DataGridColumnHeaderFormula cs_U_hp_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "U_A ср.в._" : "U_A av.-r._") + unit_v);
					cs_U_hp_A.MappingName = "u_hp_a";
					cs_U_hp_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_hp_A);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// U_hp B
					DataGridColumnHeaderFormula cs_U_hp_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "U_B ср.в._" : "U_B av.-r._") + unit_v);
					cs_U_hp_B.MappingName = "u_hp_b";
					cs_U_hp_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_hp_B);

					// U_hp C
					DataGridColumnHeaderFormula cs_U_hp_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						(settings_.CurrentLanguage.Equals("ru") ? "U_C ср.в._" : "U_C av.-r._") + unit_v);
					cs_U_hp_C.MappingName = "u_hp_c";
					cs_U_hp_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_hp_C);
				}

				// U_1
				DataGridColumnHeaderFormula cs_U_1 =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
					(settings_.CurrentLanguage.Equals("ru") ? "U_y_" : "U_s_") + unit_v);
				cs_U_1.MappingName = "u_1";
				cs_U_1.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_U_1);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U_2
					DataGridColumnHeaderFormula cs_U_2 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "U_2 (1)_" + unit_v);
					cs_U_2.MappingName = "u_2";
					cs_U_2.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_2);
				}
				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// U_0
					DataGridColumnHeaderFormula cs_U_0 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "U_0 (1)_" + unit_v);
					cs_U_0.MappingName = "u_0";
					cs_U_0.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U_0);
				}
				// I_A
				DataGridColumnHeaderFormula cs_I_A =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "I_A_" + unit_a);
				cs_I_A.MappingName = "i_a";
				cs_I_A.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_I_A);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// I_B
					DataGridColumnHeaderFormula cs_I_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "I_B_" + unit_a);
					cs_I_B.MappingName = "i_b";
					cs_I_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_B);

					// I_C
					DataGridColumnHeaderFormula cs_I_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "I_C_" + unit_a);
					cs_I_C.MappingName = "i_c";
					cs_I_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_C);
				}

				// I1_A
				DataGridColumnHeaderFormula cs_I1_A =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "I_A (1)_" + unit_a);
				cs_I1_A.MappingName = "i1_a";
				cs_I1_A.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_I1_A);

				if (connectScheme_ < ConnectScheme.Ph1W2)
				{
					// I1_B
					DataGridColumnHeaderFormula cs_I1_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "I_B (1)_" + unit_a);
					cs_I1_B.MappingName = "i1_b";
					cs_I1_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I1_B);

					// I1_C
					DataGridColumnHeaderFormula cs_I1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "I_C (1)_" + unit_a);
					cs_I1_C.MappingName = "i1_c";
					cs_I1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I1_C);
				}
				// I_1
				DataGridColumnHeaderFormula cs_I_1 =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_1 (1)_" + unit_a);
				cs_I_1.MappingName = "i_1";
				cs_I_1.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_I_1);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// I_2
					DataGridColumnHeaderFormula cs_I_2 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_2 (1)_" + unit_a);
					cs_I_2.MappingName = "i_2";
					cs_I_2.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_2);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// I_0
					DataGridColumnHeaderFormula cs_I_0 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_0 (1)_" + unit_a);
					cs_I_0.MappingName = "i_0";
					cs_I_0.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_I_0);

					if (devType_ == EmDeviceType.ETPQP)
					{
						// I_N
						DataGridColumnHeaderFormula cs_I_n =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_N_" + unit_a);
						cs_I_n.MappingName = "i_n";
						cs_I_n.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_n);

						// I_N_1
						DataGridColumnHeaderFormula cs_I_n1 =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_N (1)_" + unit_a);
						cs_I_n1.MappingName = "i1_n";
						cs_I_n1.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I_n1);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);

				//DataTable dt = ((DataSet)dgCurrentsVoltages.DataSource).Tables[0];
				//DateTime dtBegin = (DateTime)(dt.Rows[0].ItemArray.GetValue(1));
				//DateTime dtEnd   = (DateTime)(dt.Rows[dt.Rows.Count - 1].ItemArray.GetValue(1));
				DateTime dtBegin = DateTime.MinValue;
				DateTime dtEnd = DateTime.MaxValue;

				string commandText = String.Format("SELECT min(event_datetime) FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_);
				object oDtStart = dbService.ExecuteScalar(commandText);
				if (!(oDtStart is DBNull))
				{
					dtBegin = (DateTime)oDtStart;
				}
				commandText = String.Format("SELECT max(event_datetime) FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_);
				object oDtEnd = dbService.ExecuteScalar(commandText);
				if (!(oDtEnd is DBNull))
				{
					dtEnd = (DateTime)oDtEnd;
				}

				//................................................................
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MinDate =
					new DateTime(1753, 1, 1, 0, 0, 0); // min DateTimePicker date
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MaxDate =
					new DateTime(9998, 12, 31, 0, 0, 0); // max DateTimePicker date

				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MinDate =
					new DateTime(1753, 1, 1, 0, 0, 0); // min DateTimePicker date
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MaxDate =
					new DateTime(9998, 12, 31, 0, 0, 0); // max DateTimePicker date
				//................................................................
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MinDate = dtBegin;
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.MaxDate = dtEnd;

				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MinDate = dtBegin;
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.MaxDate = dtEnd;

				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpFrom.Value = dtBegin;
				mainWnd_.MainWindow_.wndDocAVG.wndDocAVGGraphBottom.dtpTo.Value = dtEnd;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperUIF::init_not_etPQP_A()");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				string query = String.Format("SELECT uif.datetime_id, uif.record_id, dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, f_a, f_b, f_c, f_ab, f_bc, f_ca, u_a, u_b, u_c, u_ab, u_bc, u_ca, u_a_const, u_b_const, u_c_const, u_ab_const, u_bc_const, u_ca_const, u_a_avdirect, u_b_avdirect, u_c_avdirect, u_ab_avdirect, u_bc_avdirect, u_ca_avdirect, u_a_1harm, u_b_1harm, u_c_1harm, u_ab_1harm, u_bc_1harm, u_ca_1harm, i_a, i_b, i_c, i_n, i_a_const, i_b_const, i_c_const, i_n_const, i_a_avdirect, i_b_avdirect, i_c_avdirect, i_n_avdirect, i_a_1harm, i_b_1harm, i_c_1harm, i_n_1harm FROM avg_u_i_f uif INNER JOIN avg_service_info info ON uif.datetime_id = info.datetime_id AND uif.record_id = info.record_id AND uif.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_u_i_f", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rU = settings_.VoltageRatio;
				float rI = settings_.CurrentRatio;
				//float rW = settings_.PowerRatio;

				//float ctr = 1, vtr = 1;
				//if (curWithTR)
				//{
				//    sqlCommand = new NpgsqlCommand();
				//    sqlCommand.Connection = conEmDb;
				//    commandText = string.Format("SELECT * FROM turn_ratios WHERE reg_id = (SELECT registration_id FROM avg_times where datetime_id = {0});", curDatetimeId_);

				//    dbService.ExecuteReader(commandText);

				//    while (dbService.DataReaderRead())
				//    {
				//        short iType = (short)dbService.DataReaderData("turn_type"];
				//        float fValue1 = (float)dbService.DataReaderData("value1"];
				//        float fValue2 = (float)dbService.DataReaderData("value2"];

				//        if (iType == 1) vtr = fValue1 / fValue2;
				//        if (iType == 2) ctr = fValue1 / fValue2;
				//    }
				//}
				//rU *= vtr;
				//rI *= ctr;
				//EmService.WriteToLogDebug(string.Format("КТ применялись: {0} ; КТТ = {1} ; КТН = {2}",
				//	curWithTR, ctr, vtr));

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// voltages
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("u"))
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rU;
						}
					}

					// currents	
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("i_"))
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rI;
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_u_i_f");

				mainWnd_.CMs_[(int)AvgPages.F_U_I] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.F_U_I].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.F_U_I].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperUIF::load_etPQP_A() ");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string commandText = string.Empty;
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_1_4");

				string query = "";
				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
				{
					query += String.Format(" SELECT datetime_id, event_datetime, f, u_a, u_b, u_c, u1_a, u1_b, u1_c, u_ab, u_bc, u_ca, u1_ab, u1_bc, u1_ca, u0_a, u0_b, u0_c, u_hp_a, u_hp_b, u_hp_c, u_1, u_2, u_0, i_a, i_b, i_c, i1_a, i1_b, i1_c, i_1, i_2, i_0 FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_);
				}
				else if (devType_ == EmDeviceType.ETPQP)
				{
					#region ETPQP

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask & 0x000000000001) != 0) { query += " f,"; }		// 1 bit
								if ((iMask & 0x000000000002) != 0) { query += " u_a,"; }	// 2 bit
								if ((iMask & 0x000000000004) != 0) { query += " u_b,"; }	// 3 bit
								if ((iMask & 0x000000000008) != 0) { query += " u_c,"; }	// 4 bit
								if ((iMask & 0x000000000010) != 0) { query += " u1_a,"; }	// 5 bit
								if ((iMask & 0x000000000020) != 0) { query += " u1_b,"; }	// 6 bit
								if ((iMask & 0x000000000040) != 0) { query += " u1_c,"; }	// 7 bit
								if ((iMask & 0x000000000080) != 0) { query += " u_ab,"; }	// 8 bit
								if ((iMask & 0x000000000100) != 0) { query += " u_bc,"; }	// 9 bit
								if ((iMask & 0x000000000200) != 0) { query += " u_ca,"; }	// 10 bit
								if ((iMask & 0x000000000400) != 0) { query += " u1_ab,"; }	// 11 bit
								if ((iMask & 0x000000000800) != 0) { query += " u1_bc,"; }	// 12 bit
								if ((iMask & 0x000000001000) != 0) { query += " u1_ca,"; }	// 13 bit
								if ((iMask & 0x000000002000) != 0) { query += " u0_a,"; }	// 14 bit
								if ((iMask & 0x000000004000) != 0) { query += " u0_b,"; }	// 15 bit
								if ((iMask & 0x000000008000) != 0) { query += " u0_c,"; }	// 16 bit

								if ((iMask & 0x000000080000) != 0) { query += " u_1,"; }	// 20 bit
								if ((iMask & 0x000000100000) != 0) { query += " u_2,"; }	// 21 bit
								if ((iMask & 0x000000200000) != 0) { query += " u_0,"; }	// 22 bit
								if ((iMask & 0x000000400000) != 0) { query += " i_a,"; }	// 23 bit
								if ((iMask & 0x000000800000) != 0) { query += " i_b,"; }	// 24 bit
								if ((iMask & 0x000001000000) != 0) { query += " i_c,"; }	// 25 bit
								if ((iMask & 0x000002000000) != 0) { query += " i1_a,"; }	// 26 bit
								if ((iMask & 0x000004000000) != 0) { query += " i1_b,"; }	// 27 bit
								if ((iMask & 0x000008000000) != 0) { query += " i1_c,"; }	// 28 bit
								if ((iMask & 0x000010000000) != 0) { query += " i_1,"; }	// 29 bit
								if ((iMask & 0x000020000000) != 0) { query += " i_2,"; }	// 30 bit
								if ((iMask & 0x000040000000) != 0) { query += " i_0,"; }	// 31 bit
								if ((iMask & 0x080000000000) != 0) { query += " i_n, i1_n,"; }	// 44 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask & 0x000000001) != 0) { query += " f,"; }		// 1 bit

								if ((iMask & 0x000000080) != 0) { query += " u_ab,"; }		// 8 bit
								if ((iMask & 0x000000100) != 0) { query += " u_bc,"; }		// 9 bit
								if ((iMask & 0x000000200) != 0) { query += " u_ca,"; }		// 10 bit
								if ((iMask & 0x000000400) != 0) { query += " u1_ab,"; }		// 11 bit
								if ((iMask & 0x000000800) != 0) { query += " u1_bc,"; }		// 12 bit
								if ((iMask & 0x000001000) != 0) { query += " u1_ca,"; }		// 13 bit

								if ((iMask & 0x000080000) != 0) { query += " u_1,"; }		// 20 bit
								if ((iMask & 0x000100000) != 0) { query += " u_2,"; }		// 21 bit

								if ((iMask & 0x000400000) != 0) { query += " i_a,"; }		// 23 bit
								if ((iMask & 0x000800000) != 0) { query += " i_b,"; }		// 24 bit
								if ((iMask & 0x001000000) != 0) { query += " i_c,"; }		// 25 bit
								if ((iMask & 0x002000000) != 0) { query += " i1_a,"; }		// 26 bit
								if ((iMask & 0x004000000) != 0) { query += " i1_b,"; }		// 27 bit
								if ((iMask & 0x008000000) != 0) { query += " i1_c,"; }		// 28 bit
								if ((iMask & 0x010000000) != 0) { query += " i_1,"; }		// 29 bit
								if ((iMask & 0x020000000) != 0) { query += " i_2,"; }		// 30 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask & 0x000000001) != 0) { query += " f,"; }				// 1 bit
								if ((iMask & 0x000000002) != 0) { query += " u_a,"; }			// 2 bit

								if ((iMask & 0x000000010) != 0) { query += " u1_a,"; }			// 5 bit

								if ((iMask & 0x000002000) != 0) { query += " u0_a,"; }			// 14 bit

								if ((iMask & 0x000080000) != 0) { query += " u_1,"; }			// 20 bit

								if ((iMask & 0x000400000) != 0) { query += " i_a,"; }			// 23 bit

								if ((iMask & 0x010000000) != 0) { query += " i1_a,"; }			// 26 bit

								if ((iMask & 0x010000000) != 0) { query += " i_1,"; }			// 29 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для EtPQP если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, f, u_a, u_b, u_c, u1_a, u1_b, u1_c, u_ab, u_bc, u_ca, u1_ab, u1_bc, u1_ca, u0_a, u0_b, u0_c, u_hp_a, u_hp_b, u_hp_c, u_1, u_2, u_0, i_a, i_b, i_c, i1_a, i1_b, i1_c, i_1, i_2, i_0, i_n, i1_n FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}
				else if (devType_ == EmDeviceType.EM32)
				{
					#region EM32

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask & 0x000000001) != 0) { query += " f,"; }				// 1 bit
								if ((iMask & 0x000000002) != 0) { query += " u_a,"; }			// 2 bit
								if ((iMask & 0x000000004) != 0) { query += " u_b,"; }			// 3 bit
								if ((iMask & 0x000000008) != 0) { query += " u_c,"; }			// 4 bit
								if ((iMask & 0x000000010) != 0) { query += " u1_a,"; }			// 5 bit
								if ((iMask & 0x000000020) != 0) { query += " u1_b,"; }			// 6 bit
								if ((iMask & 0x000000040) != 0) { query += " u1_c,"; }			// 7 bit
								if ((iMask & 0x000000080) != 0) { query += " u_ab,"; }			// 8 bit
								if ((iMask & 0x000000100) != 0) { query += " u_bc,"; }			// 9 bit
								if ((iMask & 0x000000200) != 0) { query += " u_ca,"; }			// 10 bit
								if ((iMask & 0x000000400) != 0) { query += " u1_ab,"; }			// 11 bit
								if ((iMask & 0x000000800) != 0) { query += " u1_bc,"; }			// 12 bit
								if ((iMask & 0x000001000) != 0) { query += " u1_ca,"; }			// 13 bit
								if ((iMask & 0x000002000) != 0) { query += " u0_a,"; }			// 14 bit
								if ((iMask & 0x000004000) != 0) { query += " u0_b,"; }			// 15 bit
								if ((iMask & 0x000008000) != 0) { query += " u0_c,"; }			// 16 bit

								if ((iMask & 0x000080000) != 0) { query += " u_1,"; }			// 20 bit
								if ((iMask & 0x000100000) != 0) { query += " u_2,"; }			// 21 bit
								if ((iMask & 0x000200000) != 0) { query += " u_0,"; }			// 22 bit
								if ((iMask & 0x000400000) != 0) { query += " i_a,"; }			// 23 bit
								if ((iMask & 0x000800000) != 0) { query += " i_b,"; }			// 24 bit
								if ((iMask & 0x001000000) != 0) { query += " i_c,"; }			// 25 bit
								if ((iMask & 0x002000000) != 0) { query += " i1_a,"; }			// 26 bit
								if ((iMask & 0x004000000) != 0) { query += " i1_b,"; }			// 27 bit
								if ((iMask & 0x008000000) != 0) { query += " i1_c,"; }			// 28 bit
								if ((iMask & 0x010000000) != 0) { query += " i_1,"; }			// 29 bit
								if ((iMask & 0x020000000) != 0) { query += " i_2,"; }			// 30 bit
								if ((iMask & 0x040000000) != 0) { query += " i_0,"; }			// 31 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask & 0x000000001) != 0) { query += " f,"; }					// 1 bit

								if ((iMask & 0x000000080) != 0) { query += " u_ab,"; }			// 8 bit
								if ((iMask & 0x000000100) != 0) { query += " u_bc,"; }			// 9 bit
								if ((iMask & 0x000000200) != 0) { query += " u_ca,"; }			// 10 bit
								if ((iMask & 0x000000400) != 0) { query += " u1_ab,"; }			// 11 bit
								if ((iMask & 0x000000800) != 0) { query += " u1_bc,"; }			// 12 bit
								if ((iMask & 0x000001000) != 0) { query += " u1_ca,"; }			// 13 bit

								if ((iMask & 0x000080000) != 0) { query += " u_1,"; }			// 20 bit
								if ((iMask & 0x000100000) != 0) { query += " u_2,"; }			// 21 bit

								if ((iMask & 0x000400000) != 0) { query += " i_a,"; }			// 23 bit
								if ((iMask & 0x000800000) != 0) { query += " i_b,"; }			// 24 bit
								if ((iMask & 0x001000000) != 0) { query += " i_c,"; }			// 25 bit
								if ((iMask & 0x002000000) != 0) { query += " i1_a,"; }			// 26 bit
								if ((iMask & 0x004000000) != 0) { query += " i1_b,"; }			// 27 bit
								if ((iMask & 0x008000000) != 0) { query += " i1_c,"; }			// 28 bit
								if ((iMask & 0x010000000) != 0) { query += " i_1,"; }			// 29 bit
								if ((iMask & 0x020000000) != 0) { query += " i_2,"; }			// 30 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask & 0x000000001) != 0) { query += " f,"; }				// 1 bit
								if ((iMask & 0x000000002) != 0) { query += " u_a,"; }			// 2 bit
								if ((iMask & 0x000000010) != 0) { query += " u1_a,"; }			// 5 bit
								if ((iMask & 0x000002000) != 0) { query += " u0_a,"; }			// 14 bit
								if ((iMask & 0x000080000) != 0) { query += " u_1,"; }			// 20 bit
								if ((iMask & 0x000400000) != 0) { query += " i_a,"; }			// 23 bit
								if ((iMask & 0x010000000) != 0) { query += " i1_a,"; }			// 26 bit
								if ((iMask & 0x010000000) != 0) { query += " i_1,"; }			// 29 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для Em32 если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, f, u_a, u_b, u_c, u1_a, u1_b, u1_c, u_ab, u_bc, u_ca, u1_ab, u1_bc, u1_ca, u0_a, u0_b, u0_c, u_hp_a, u_hp_b, u_hp_c, u_1, u_2, u_0, i_a, i_b, i_c, i1_a, i1_b, i1_c, i_1, i_2, i_0 FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_1_4", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rU = settings_.VoltageRatio;
				float rI = settings_.CurrentRatio;

				float ctr = 1, vtr = 1;

				if (curWithTR)
				{
					if (devType_ == EmDeviceType.EM33T ||
						devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.EM32)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.ETPQP)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);

					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (devType_ != EmDeviceType.ETPQP)
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						if (iType == 1) vtr = fValue1 / fValue2;
						if (iType == 2) ctr = fValue1 / fValue2;
					}
				}
				rU *= vtr;
				rI *= ctr;

				EmService.WriteToLogDebug(string.Format("КТ применялись: {0} ; КТТ = {1} ; КТН = {2}",
					curWithTR, ctr, vtr));

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// voltages
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("u") &&
							(!ds.Tables[0].Columns[i].Caption.Contains("an_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("d_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("k") &&
							!ds.Tables[0].Columns[i].Caption.Contains("sum") &&
							!ds.Tables[0].Columns[i].Caption.Contains("tangent"))
							)
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rU;
						}
					}

					// currents	
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if ((ds.Tables[0].Columns[i].Caption.Contains("i_") ||
							ds.Tables[0].Columns[i].Caption.Contains("i1_")) &&
							(!ds.Tables[0].Columns[i].Caption.Contains("an_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("k") &&
							!ds.Tables[0].Columns[i].Caption.Contains("tangent"))
							)
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rI;
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_1_4");

				mainWnd_.CMs_[(int)AvgPages.F_U_I] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.F_U_I].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.F_U_I].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperUIF::load_not_etPQP_A() ");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}
	}

	internal class AVGDataGridWrapperPower : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperPower(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm =new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_w = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_w") :
					rm.GetString("column_header_units_kw");
				string unit_va = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_va") :
					rm.GetString("column_header_units_kva");
				string unit_var = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_var") :
					rm.GetString("column_header_units_kvar");

				dataGrid_.TableStyles.Clear();
				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "avg_power";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// P_A
					DataGridColumnHeaderFormula cs_P_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "P_A_" + unit_w);
					cs_P_A.MappingName = "p_a";
					cs_P_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_A);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// P_B
						DataGridColumnHeaderFormula cs_P_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "P_B_" + unit_w);
						cs_P_B.MappingName = "p_b";
						cs_P_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P_B);

						// P_C
						DataGridColumnHeaderFormula cs_P_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "P_C_" + unit_w);
						cs_P_C.MappingName = "p_c";
						cs_P_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P_C);

						// P_Σ
						DataGridColumnHeaderFormula cs_P_Summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_Σ_" + unit_w);
						cs_P_Summ.MappingName = "p_sum";
						cs_P_Summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P_Summ);
					}
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3)
				{
					// P_Σ
					DataGridColumnHeaderFormula cs_P_Summ =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_Σ_" + unit_w);
					cs_P_Summ.MappingName = "p_12sum";//"p_sum";
					cs_P_Summ.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_Summ);
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// P_1
					DataGridColumnHeaderFormula cs_P_1 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_1_" + unit_w);
					cs_P_1.MappingName = "p_1";
					cs_P_1.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_1);

					// P_2
					DataGridColumnHeaderFormula cs_P_2 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_2_" + unit_w);
					cs_P_2.MappingName = "p_2";
					cs_P_2.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_2);

					// P_Σ
					DataGridColumnHeaderFormula cs_P_Summ12 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_Σ_" + unit_w);
					cs_P_Summ12.MappingName = "p_12sum";
					cs_P_Summ12.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_Summ12);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// S_A
					DataGridColumnHeaderFormula cs_S_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "S_A_" + unit_va);
					cs_S_A.MappingName = "s_a";
					cs_S_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_A);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// S_B
						DataGridColumnHeaderFormula cs_S_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "S_B_" + unit_va);
						cs_S_B.MappingName = "s_b";
						cs_S_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_S_B);

						// S_C
						DataGridColumnHeaderFormula cs_S_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "S_C_" + unit_va);
						cs_S_C.MappingName = "s_c";
						cs_S_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_S_C);

						// S_Σ
						DataGridColumnHeaderFormula cs_S_Summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "S_Σ_" + unit_va);
						cs_S_Summ.MappingName = "s_sum";
						cs_S_Summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_S_Summ);
					}
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					//// S_1
					//DataGridColumnHeaderFormula cs_S_1 =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "S_1_" + unit_va);
					//cs_S_1.MappingName = "s_1";
					//cs_S_1.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_S_1);

					//// S_2
					//DataGridColumnHeaderFormula cs_S_2 =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "S_2_" + unit_va);
					//cs_S_2.MappingName = "s_2";
					//cs_S_2.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_S_2);

					// S_Σ
					DataGridColumnHeaderFormula cs_S_Summ12 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "S_Σ_" + unit_va);
					cs_S_Summ12.MappingName = "s_12sum";
					cs_S_Summ12.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_Summ12);
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3)
				{
					// S_Σ
					DataGridColumnHeaderFormula cs_S_Summ12 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "S_Σ_" + unit_va);
					cs_S_Summ12.MappingName = "s_12sum";
					cs_S_Summ12.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_Summ12);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// Q_A
					DataGridColumnHeaderFormula cs_Q_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Q_A_" + unit_var);
					cs_Q_A.MappingName = "q_a";
					cs_Q_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Q_A);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// Q_B
						DataGridColumnHeaderFormula cs_Q_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Q_B_" + unit_var);
						cs_Q_B.MappingName = "q_b";
						cs_Q_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B);

						// Q_C
						DataGridColumnHeaderFormula cs_Q_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Q_C_" + unit_var);
						cs_Q_C.MappingName = "q_c";
						cs_Q_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C);

						// Q_Σ
						DataGridColumnHeaderFormula cs_Q_Summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "Q_Σ_" + unit_var);
						cs_Q_Summ.MappingName = "q_sum";
						cs_Q_Summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ);
					}
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					//// Q_1
					//DataGridColumnHeaderFormula cs_Q_1 =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "Q_1_" + unit_var);
					//cs_Q_1.MappingName = "q_1";
					//cs_Q_1.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_Q_1);

					//// Q_2
					//DataGridColumnHeaderFormula cs_Q_2 =
					//    new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "Q_2_" + unit_var);
					//cs_Q_2.MappingName = "q_2";
					//cs_Q_2.Width = DataColumnsWidth.CommonWidth;
					//ts.GridColumnStyles.Add(cs_Q_2);

					// Q_Σ
					DataGridColumnHeaderFormula cs_Q_Summ12 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "Q_Σ_" + unit_var);
					cs_Q_Summ12.MappingName = "q_12sum";
					cs_Q_Summ12.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Q_Summ12);
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3)
				{
					// Q_Σ
					DataGridColumnHeaderFormula cs_Q_Summ12 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "Q_Σ_" + unit_var);
					cs_Q_Summ12.MappingName = "q_12sum";
					cs_Q_Summ12.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Q_Summ12);
				}

				// tangent p
				DataGridColumnHeaderFormula cs_tanP =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
					(settings_.CurrentLanguage.Equals("ru") ? "tg φ" : "tan φ"));
				cs_tanP.MappingName = "tangent_p";
				cs_tanP.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_tanP);

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// Kp_A
					DataGridColumnHeaderFormula cs_Kp_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						settings_.CurrentLanguage.Equals("ru") ? "Kp_A" : "PF_A");
					cs_Kp_A.MappingName = "kp_a";
					cs_Kp_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Kp_A);

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// Kp_B
						DataGridColumnHeaderFormula cs_Kp_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							settings_.CurrentLanguage.Equals("ru") ? "Kp_B" : "PF_B");
						cs_Kp_B.MappingName = "kp_b";
						cs_Kp_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Kp_B);

						// Kp_C
						DataGridColumnHeaderFormula cs_Kp_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							settings_.CurrentLanguage.Equals("ru") ? "Kp_C" : "PF_C");
						cs_Kp_C.MappingName = "kp_c";
						cs_Kp_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Kp_C);

						// Kp_Σ
						DataGridColumnHeaderFormula cs_Kp_Summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							settings_.CurrentLanguage.Equals("ru") ? "Kp_Σ" : "PF_Σ");
						cs_Kp_Summ.MappingName = "kp_abc";
						cs_Kp_Summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Kp_Summ);
					}
				}

				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// Kp_Σ
					DataGridColumnHeaderFormula cs_Kp_Summ12 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						settings_.CurrentLanguage.Equals("ru") ? "Kp_Σ" : "PF_Σ");
					cs_Kp_Summ12.MappingName = "kp_12";
					cs_Kp_Summ12.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Kp_Summ12);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPower::init_etPQP_A()");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_w = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_w") :
					rm.GetString("column_header_units_kw");
				string unit_va = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_va") :
					rm.GetString("column_header_units_kva");
				string unit_var = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_var") :
					rm.GetString("column_header_units_kvar");

				dataGrid_.TableStyles.Clear();
				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "period_avg_params_1_4";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// P_Σ
					DataGridColumnHeaderFormula cs_P_Summ =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_Σ_" + unit_w);
					cs_P_Summ.MappingName = "p_sum";
					cs_P_Summ.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_Summ);
				}

				// Р 1-го элемента 
				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// P_1
					DataGridColumnHeaderFormula cs_P_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_AB_" + unit_w);
					cs_P_A.MappingName = "p_a_1";
					cs_P_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_A);
				}
				else
				{
					// P_A
					DataGridColumnHeaderFormula cs_P_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "P_A_" + unit_w);
					cs_P_A.MappingName = "p_a_1";
					cs_P_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_A);
				}

				// P 2-го элемента
				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// P_B
					DataGridColumnHeaderFormula cs_P_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_CB_" + unit_w);
					cs_P_B.MappingName = "p_b_2";
					cs_P_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_B);
				}
				else
					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// P_B
						DataGridColumnHeaderFormula cs_P_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "P_B_" + unit_w);
						cs_P_B.MappingName = "p_b_2";
						cs_P_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P_B);
					}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// P_C
					DataGridColumnHeaderFormula cs_P_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "P_C_" + unit_w);
					cs_P_C.MappingName = "p_c";
					cs_P_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_C);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// S_Σ
					DataGridColumnHeaderFormula cs_S_Summ =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "S_Σ_" + unit_va);
					cs_S_Summ.MappingName = "s_sum";
					cs_S_Summ.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_Summ);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// S_A
					DataGridColumnHeaderFormula cs_S_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "S_A_" + unit_va);
					cs_S_A.MappingName = "s_a";
					cs_S_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_A);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// S_B
					DataGridColumnHeaderFormula cs_S_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "S_B_" + unit_va);
					cs_S_B.MappingName = "s_b";
					cs_S_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_B);

					// S_C
					DataGridColumnHeaderFormula cs_S_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "S_C_" + unit_va);
					cs_S_C.MappingName = "s_c";
					cs_S_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_S_C);
				}

				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
				{
					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// Q_Σ geom
						DataGridColumnHeaderFormula cs_Q_Summ_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ геом._" : "Q_Σ geom._") + unit_var);
						cs_Q_Summ_geom.MappingName = "q_sum_geom";
						cs_Q_Summ_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_geom);

						// tangent p geom
						DataGridColumnHeaderFormula cs_tanP_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ геом._" : "tan φ geom._"));
						cs_tanP_geom.MappingName = "tangent_p_geom";
						cs_tanP_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_geom);
					}

					if (connectScheme_ != ConnectScheme.Ph3W3 &&
						connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						// Q_A geom 
						DataGridColumnHeaderFormula cs_Q_A_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A геом._" : "Q_A geom._") + unit_var);
						cs_Q_A_geom.MappingName = "q_a_geom";
						cs_Q_A_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_geom);
					}

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// Q_B geom
						DataGridColumnHeaderFormula cs_Q_B_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B геом._" : "Q_B geom._") + unit_var);
						cs_Q_B_geom.MappingName = "q_b_geom";
						cs_Q_B_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_geom);

						// Q_C geom
						DataGridColumnHeaderFormula cs_Q_C_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C геом._" : "Q_C geom._") + unit_var);
						cs_Q_C_geom.MappingName = "q_c_geom";
						cs_Q_C_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_geom);
					}

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// Q_Σ shift
						DataGridColumnHeaderFormula cs_Q_Summ_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ сдвиг._" : "Q_Σ ph.sh._") + unit_var);
						cs_Q_Summ_shift.MappingName = "q_sum_shift";
						cs_Q_Summ_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_shift);

						// tangent p shift
						DataGridColumnHeaderFormula cs_tanP_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ сдвиг._" : "tan φ sh._"));
						cs_tanP_shift.MappingName = "tangent_p_shift";
						cs_tanP_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_shift);
					}

					if (connectScheme_ != ConnectScheme.Ph3W3 &&
						connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						// Q_A shift 
						DataGridColumnHeaderFormula cs_Q_A_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A сдвиг._" : "Q_A ph.sh._") + unit_var);
						cs_Q_A_shift.MappingName = "q_a_shift";
						cs_Q_A_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_shift);
					}

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						// Q_B shift
						DataGridColumnHeaderFormula cs_Q_B_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B сдвиг._" : "Q_B ph.sh._") +
							unit_var);
						cs_Q_B_shift.MappingName = "q_b_shift";
						cs_Q_B_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_shift);

						// Q_C shift
						DataGridColumnHeaderFormula cs_Q_C_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C сдвиг._" : "Q_C ph.sh._") +
							unit_var);
						cs_Q_C_shift.MappingName = "q_c_shift";
						cs_Q_C_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_shift);
					}

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// Q_Σ cross
						DataGridColumnHeaderFormula cs_Q_Summ_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ перекр._" : "Q_Σ cr.conn._") +
							unit_var);
						cs_Q_Summ_cross.MappingName = "q_sum_cross";
						cs_Q_Summ_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_cross);

						// tangent p cross
						DataGridColumnHeaderFormula cs_tanP_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ перекр._" : "tan φ cr._"));
						cs_tanP_cross.MappingName = "tangent_p_cross";
						cs_tanP_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_cross);

						// Q_A cross 
						DataGridColumnHeaderFormula cs_Q_A_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A перекр._" : "Q_A cr.conn._") +
							unit_var);
						cs_Q_A_cross.MappingName = "q_a_cross";
						cs_Q_A_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_cross);

						// Q_B cross
						DataGridColumnHeaderFormula cs_Q_B_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B перекр._" : "Q_B cr.conn._") +
							unit_var);
						cs_Q_B_cross.MappingName = "q_b_cross";
						cs_Q_B_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_cross);

						// Q_C cross
						DataGridColumnHeaderFormula cs_Q_C_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C перекр._" : "Q_C cr.conn._") +
							unit_var);
						cs_Q_C_cross.MappingName = "q_c_cross";
						cs_Q_C_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_cross);
					}
				}
				else if (devType_ == EmDeviceType.EM32)
				{
					#region Em32

					if (connectScheme_ == ConnectScheme.Ph1W2)
					{
						// Q_A
						DataGridColumnHeaderFormula cs_Q_A =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_" : "Q_") + unit_var);
						cs_Q_A.MappingName = "q_a";
						cs_Q_A.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A);

						// tangent p
						DataGridColumnHeaderFormula cs_tanP =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ" : "tan φ"));
						cs_tanP.MappingName = "tangent_p";
						cs_tanP.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP);
					}
					else
					{
						// Q_Σ
						DataGridColumnHeaderFormula cs_Q_Summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ_" : "Q_Σ_") + unit_var);
						cs_Q_Summ.MappingName = "q_sum";
						cs_Q_Summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ);

						// tangent p
						DataGridColumnHeaderFormula cs_tanP =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ" : "tan φ"));
						cs_tanP.MappingName = "tangent_p";
						cs_tanP.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP);

						// Q_A
						DataGridColumnHeaderFormula cs_Q_A =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							"Q_A_" + unit_var);
						cs_Q_A.MappingName = "q_a";
						cs_Q_A.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A);

						// Q_B
						DataGridColumnHeaderFormula cs_Q_B =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							"Q_B_" + unit_var);
						cs_Q_B.MappingName = "q_b";
						cs_Q_B.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B);

						// Q_C
						DataGridColumnHeaderFormula cs_Q_C =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							"Q_C_" + unit_var);
						cs_Q_C.MappingName = "q_c";
						cs_Q_C.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C);
					}

					#endregion
				}
				else if (devType_ == EmDeviceType.ETPQP)
				{
					if (connectScheme_ == ConnectScheme.Ph1W2)
					{
						// Q_A geom
						DataGridColumnHeaderFormula cs_Q_A_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A геом._" : "Q_A geom._") + unit_var);
						cs_Q_A_geom.MappingName = "q_a_geom";
						cs_Q_A_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_geom);

						// Q_A shift
						DataGridColumnHeaderFormula cs_Q_A_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A сдвиг._" : "Q_A ph.sh._") + unit_var);
						cs_Q_A_shift.MappingName = "q_a_shift";
						cs_Q_A_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_shift);

						// Q_A cross
						DataGridColumnHeaderFormula cs_Q_A_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A перекр._" : "Q_A cr.conn._") + unit_var);
						cs_Q_A_cross.MappingName = "q_a_cross";
						cs_Q_A_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_cross);

						// Q_A 1harm
						DataGridColumnHeaderFormula cs_Q_A_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A 1 гарм._" : "Q_A 1 harm._") + unit_var);
						cs_Q_A_1harm.MappingName = "q_a_1harm";
						cs_Q_A_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_1harm);

						// tangent p geom
						DataGridColumnHeaderFormula cs_tanP_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ геом._" : "tan φ geom._"));
						cs_tanP_geom.MappingName = "tangent_p_geom";
						cs_tanP_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_geom);

						// tangent p shift
						DataGridColumnHeaderFormula cs_tanP_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ сдвиг._" : "tan φ ph.sh._"));
						cs_tanP_shift.MappingName = "tangent_p_shift";
						cs_tanP_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_shift);

						// tangent p cross
						DataGridColumnHeaderFormula cs_tanP_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ перекр._" : "tan φ cr.conn._"));
						cs_tanP_cross.MappingName = "tangent_p_cross";
						cs_tanP_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_cross);

						// tangent p 1harm
						DataGridColumnHeaderFormula cs_tanP_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ 1 гарм._" : "tan φ 1 harm._"));
						cs_tanP_1harm.MappingName = "tangent_p_1harm";
						cs_tanP_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_1harm);
					}
					else  // not 1ph2w
					{
						// Q_Σ geom
						DataGridColumnHeaderFormula cs_Q_Summ_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ_ геом._" : "Q_Σ_ geom._") + unit_var);
						cs_Q_Summ_geom.MappingName = "q_sum_geom";
						cs_Q_Summ_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_geom);

						// Q_Σ shift
						DataGridColumnHeaderFormula cs_Q_Summ_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ_ сдвиг._" : "Q_Σ_ ph.sh._") + unit_var);
						cs_Q_Summ_shift.MappingName = "q_sum_shift";
						cs_Q_Summ_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_shift);

						// Q_Σ cross
						DataGridColumnHeaderFormula cs_Q_Summ_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ_ перекр._" : "Q_Σ_ cr.conn._") +
							unit_var);
						cs_Q_Summ_cross.MappingName = "q_sum_cross";
						cs_Q_Summ_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_cross);

						// Q_Σ 1harm
						DataGridColumnHeaderFormula cs_Q_Summ_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_Σ_ 1 гарм._" : "Q_Σ_ 1 harm._") +
							unit_var);
						cs_Q_Summ_1harm.MappingName = "q_sum_1harm";
						cs_Q_Summ_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_Summ_1harm);

						// tangent p geom
						DataGridColumnHeaderFormula cs_tanP_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ геом._" : "tan φ geom._"));
						cs_tanP_geom.MappingName = "tangent_p_geom";
						cs_tanP_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_geom);

						// tangent p shift
						DataGridColumnHeaderFormula cs_tanP_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ сдвиг._" : "tan φ ph.sh._"));
						cs_tanP_shift.MappingName = "tangent_p_shift";
						cs_tanP_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_shift);

						// tangent p cross
						DataGridColumnHeaderFormula cs_tanP_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ перекр._" : "tan φ cr.conn._"));
						cs_tanP_cross.MappingName = "tangent_p_cross";
						cs_tanP_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_cross);

						// tangent p 1harm
						DataGridColumnHeaderFormula cs_tanP_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
							(settings_.CurrentLanguage.Equals("ru") ? "tg φ 1 гарм._" : "tan φ 1 harm._"));
						cs_tanP_1harm.MappingName = "tangent_p_1harm";
						cs_tanP_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_tanP_1harm);

						// Q_A geom
						DataGridColumnHeaderFormula cs_Q_A_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A геом._" : "Q_A geom._") + unit_var);
						cs_Q_A_geom.MappingName = "q_a_geom";
						cs_Q_A_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_geom);

						// Q_A shift
						DataGridColumnHeaderFormula cs_Q_A_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A сдвиг._" : "Q_A ph.sh._") +
							unit_var);
						cs_Q_A_shift.MappingName = "q_a_shift";
						cs_Q_A_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_shift);

						// Q_A cross
						DataGridColumnHeaderFormula cs_Q_A_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A перекр._" : "Q_A cr.conn._") +
							unit_var);
						cs_Q_A_cross.MappingName = "q_a_cross";
						cs_Q_A_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_cross);

						// Q_A 1harm
						DataGridColumnHeaderFormula cs_Q_A_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_A 1 гарм._" : "Q_A 1 harm._") +
							unit_var);
						cs_Q_A_1harm.MappingName = "q_a_1harm";
						cs_Q_A_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_A_1harm);

						// Q_B geom
						DataGridColumnHeaderFormula cs_Q_B_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B геом._" : "Q_B geom._") + unit_var);
						cs_Q_B_geom.MappingName = "q_b_geom";
						cs_Q_B_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_geom);

						// Q_B shift
						DataGridColumnHeaderFormula cs_Q_B_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B сдвиг._" : "Q_B ph.sh._") +
							unit_var);
						cs_Q_B_shift.MappingName = "q_b_shift";
						cs_Q_B_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_shift);

						// Q_B cross
						DataGridColumnHeaderFormula cs_Q_B_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B перекр._" : "Q_B cr.conn._") +
							unit_var);
						cs_Q_B_cross.MappingName = "q_b_cross";
						cs_Q_B_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_cross);

						// Q_B 1harm
						DataGridColumnHeaderFormula cs_Q_B_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_B 1 гарм._" : "Q_B 1 harm._") +
							unit_var);
						cs_Q_B_1harm.MappingName = "q_b_1harm";
						cs_Q_B_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_B_1harm);

						// Q_C geom
						DataGridColumnHeaderFormula cs_Q_C_geom =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C геом._" : "Q_C geom._") + unit_var);
						cs_Q_C_geom.MappingName = "q_c_geom";
						cs_Q_C_geom.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_geom);

						// Q_C shift
						DataGridColumnHeaderFormula cs_Q_C_shift =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C сдвиг._" : "Q_C ph.sh._") +
							unit_var);
						cs_Q_C_shift.MappingName = "q_c_shift";
						cs_Q_C_shift.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_shift);

						// Q_C cross
						DataGridColumnHeaderFormula cs_Q_C_cross =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C перекр._" : "Q_C cr.conn._") +
							unit_var);
						cs_Q_C_cross.MappingName = "q_c_cross";
						cs_Q_C_cross.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_cross);

						// Q_C 1harm
						DataGridColumnHeaderFormula cs_Q_C_1harm =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							(settings_.CurrentLanguage.Equals("ru") ? "Q_C 1 гарм._" : "Q_C 1 harm._") +
							unit_var);
						cs_Q_C_1harm.MappingName = "q_c_1harm";
						cs_Q_C_1harm.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_Q_C_1harm);
					}
				} // end of Et-PQP

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// Kp_Σ
					DataGridColumnHeaderFormula cs_Kp_Summ =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						settings_.CurrentLanguage.Equals("ru") ? "Kp_Σ" : "PF_Σ");
					cs_Kp_Summ.MappingName = "kp_sum";
					cs_Kp_Summ.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Kp_Summ);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// Kp_A
					DataGridColumnHeaderFormula cs_Kp_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						settings_.CurrentLanguage.Equals("ru") ? "Kp_A" : "PF_A");
					cs_Kp_A.MappingName = "kp_a";
					cs_Kp_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Kp_A);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// Kp_B
					DataGridColumnHeaderFormula cs_Kp_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						settings_.CurrentLanguage.Equals("ru") ? "Kp_B" : "PF_B");
					cs_Kp_B.MappingName = "kp_b";
					cs_Kp_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Kp_B);

					// Kp_C
					DataGridColumnHeaderFormula cs_Kp_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						settings_.CurrentLanguage.Equals("ru") ? "Kp_C" : "PF_C");
					cs_Kp_C.MappingName = "kp_c";
					cs_Kp_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_Kp_C);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// P_0
					DataGridColumnHeaderFormula cs_P_0 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_0(1)_" + unit_w);
					cs_P_0.MappingName = "p_0";
					cs_P_0.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_0);
				}

				// P_1
				DataGridColumnHeaderFormula cs_P_1 =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_1(1)_" + unit_w);
				cs_P_1.MappingName = "p_1";
				cs_P_1.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_P_1);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// P_2
					DataGridColumnHeaderFormula cs_P_2 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_2(1)_" + unit_w);
					cs_P_2.MappingName = "p_2";
					cs_P_2.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_P_2);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPower::init_not_etPQP_A()");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				string query = String.Format("SELECT p.datetime_id, p.record_id, dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, p_a, p_b, p_c, p_sum, p_1, p_2, p_12sum, s_a, s_b, s_c, s_sum, s_1, s_2, s_12sum, q_a, q_b, q_c, q_sum, q_1, q_2, q_12sum, kp_a, kp_b, kp_c, kp_abc, kp_12, q1, q2, q0, CASE WHEN tangent_p IS NULL THEN '-' ELSE CAST(tangent_p AS text) END AS tangent_p FROM avg_power p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_power", ref ds);

				#region Selecting turn_ratios and changing rW

				float rW = settings_.PowerRatio;

				//float ctr = 1, vtr = 1;
				//if (curWithTR)
				//{
				//    sqlCommand = new NpgsqlCommand();
				//    sqlCommand.Connection = conEmDb;
				//    commandText = string.Format("SELECT * FROM turn_ratios WHERE reg_id = (SELECT registration_id FROM avg_times where datetime_id = {0});", curDatetimeId_);

				//    dbService.ExecuteReader(commandText);

				//    while (dbService.DataReaderRead())
				//    {
				//        short iType = (short)dbService.DataReaderData("turn_type"];
				//        float fValue1 = (float)dbService.DataReaderData("value1"];
				//        float fValue2 = (float)dbService.DataReaderData("value2"];

				//        if (iType == 1) vtr = fValue1 / fValue2;
				//        if (iType == 2) ctr = fValue1 / fValue2;
				//    }
				//}
				//rW *= vtr * ctr;
				//EmService.WriteToLogDebug(string.Format("КТ применялись: {0} ; КТТ = {1} ; КТН = {2}",
				//    curWithTR, ctr, vtr));

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// powers
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if ((ds.Tables[0].Columns[i].Caption.Contains("p_") ||
							ds.Tables[0].Columns[i].Caption.Contains("s_") ||
							ds.Tables[0].Columns[i].Caption.Contains("q_")) &&
							(!ds.Tables[0].Columns[i].Caption.Contains("q_type") &&
							!ds.Tables[0].Columns[i].Caption.Contains("kp_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("hp_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("tangent"))
							)
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rW;
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_power");

				mainWnd_.CMs_[(int)AvgPages.POWER] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.POWER].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.POWER].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPower::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string commandText = string.Empty;
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_1_4");

				string query = "";
				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
				{
					query += String.Format(" SELECT datetime_id, event_datetime, p_sum, p_a_1, p_b_2, p_c, s_sum, s_a, s_b, s_c, q_sum_geom, q_a_geom, q_b_geom, q_c_geom, q_sum_shift, q_a_shift, q_b_shift, q_c_shift, q_sum_cross, q_a_cross, q_b_cross, q_c_cross, text(kp_sum) as kp_sum, text(kp_a) as kp_a, text(kp_b) as kp_b, text(kp_c) as kp_c, p_0, p_1, p_2,  tangent_p_geom, tangent_p_shift, tangent_p_cross FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_);
				}
				else if (devType_ == EmDeviceType.ETPQP)
				{
					#region ETPQP

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask & 0x000080000000) != 0) { query += " q_sum_shift,"; }// 32 bit
								if ((iMask & 0x000100000000) != 0) { query += " q_a_shift,"; }	// 33 bit
								if ((iMask & 0x000200000000) != 0) { query += " q_b_shift,"; }	// 34 bit
								if ((iMask & 0x000400000000) != 0) { query += " q_c_shift,"; }	// 35 bit
								if ((iMask & 0x000800000000) != 0) { query += " q_sum_cross,"; }// 36 bit
								if ((iMask & 0x001000000000) != 0) { query += " q_a_cross,"; }	// 37 bit
								if ((iMask & 0x002000000000) != 0) { query += " q_b_cross,"; }	// 38 bit
								if ((iMask & 0x004000000000) != 0) { query += " q_c_cross,"; }	// 39 bit
								if ((iMask & 0x008000000000) != 0) { query += " q_sum_1harm,"; }// 40 bit
								if ((iMask & 0x010000000000) != 0) { query += " q_a_1harm,"; }	// 41 bit
								if ((iMask & 0x020000000000) != 0) { query += " q_b_1harm,"; }	// 42 bit
								if ((iMask & 0x040000000000) != 0) { query += " q_c_1harm,"; }	// 43 bit
								////////////////////////////////////////////////////////////////
								if ((iMask2 & 0x000000000001) != 0) { query += " p_sum,"; }	// 1 bit
								if ((iMask2 & 0x000000000002) != 0) { query += " p_a_1,"; }	// 2 bit
								if ((iMask2 & 0x000000000004) != 0) { query += " p_b_2,"; }	// 3 bit
								if ((iMask2 & 0x000000000008) != 0) { query += " p_c,"; }	// 4 bit
								if ((iMask2 & 0x000000000010) != 0) { query += " s_sum,"; }	// 5 bit
								if ((iMask2 & 0x000000000020) != 0) { query += " s_a,"; }	// 6 bit
								if ((iMask2 & 0x000000000040) != 0) { query += " s_b,"; }	// 7 bit
								if ((iMask2 & 0x000000000080) != 0) { query += " s_c,"; }	// 8 bit
								if ((iMask2 & 0x000000000100) != 0) { query += " q_sum_geom,"; }// 9 bit
								if ((iMask2 & 0x000000000200) != 0) { query += " q_a_geom,"; }	// 10 bit
								if ((iMask2 & 0x000000000400) != 0) { query += " q_b_geom,"; }	// 11 bit
								if ((iMask2 & 0x000000000800) != 0) { query += " q_c_geom,"; }	// 12 bit
								if ((iMask2 & 0x000000001000) != 0)
								{
									query += " text(kp_sum) as kp_sum,";
								}// 13 bit
								if ((iMask2 & 0x000000002000) != 0)
								{
									query += " text(kp_a) as kp_a,";
								}// 14 bit
								if ((iMask2 & 0x000000004000) != 0)
								{
									query += " text(kp_b) as kp_b,";
								}// 15 bit
								if ((iMask2 & 0x000000008000) != 0)
								{
									query += " text(kp_c) as kp_c,";
								}// 16 bit
								
								if ((iMask2 & 0x000002000000) != 0) { query += " p_0,"; }	// 26 bit
								if ((iMask2 & 0x000004000000) != 0) { query += " p_1,"; }	// 27 bit
								if ((iMask2 & 0x000008000000) != 0) { query += " p_2,"; }	// 28 bit
								if ((iMask2 & 0x100000000000) != 0) { query += " q_type,"; }// 45 bit

								// если есть Р и Q значит есть тангенс Р
								if ((iMask2 & 0x000000000001) != 0 && (iMask2 & 0x000000000100) != 0)
								{ query += " tangent_p_geom,"; }	// 1, 9 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x000080000000) != 0)
								{ query += " tangent_p_shift,"; }	// 1, 32 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x000800000000) != 0)
								{ query += " tangent_p_cross,"; }	// 1, 36 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x008000000000) != 0)
								{ query += " tangent_p_1harm,"; }	// 1, 40 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask & 0x000080000000) != 0) { query += " q_sum_shift,"; }// 32 bit
								if ((iMask & 0x000100000000) != 0) { query += " q_a_shift,"; }	// 33 bit
								if ((iMask & 0x000200000000) != 0) { query += " q_b_shift,"; }	// 34 bit
								if ((iMask & 0x000400000000) != 0) { query += " q_c_shift,"; }	// 35 bit
								if ((iMask & 0x000800000000) != 0) { query += " q_sum_cross,"; }// 36 bit
								if ((iMask & 0x001000000000) != 0) { query += " q_a_cross,"; }	// 37 bit
								if ((iMask & 0x002000000000) != 0) { query += " q_b_cross,"; }	// 38 bit
								if ((iMask & 0x004000000000) != 0) { query += " q_c_cross,"; }	// 39 bit
								if ((iMask & 0x008000000000) != 0) { query += " q_sum_1harm,"; }// 40 bit
								if ((iMask & 0x010000000000) != 0) { query += " q_a_1harm,"; }	// 41 bit
								if ((iMask & 0x020000000000) != 0) { query += " q_b_1harm,"; }	// 42 bit
								if ((iMask & 0x040000000000) != 0) { query += " q_c_1harm,"; }	// 43 bit
								////////////////////////////////////////////////////////////////////
								if ((iMask2 & 0x000000000001) != 0) { query += " p_sum,"; }		// 1 bit
								if ((iMask2 & 0x000000000002) != 0) { query += " p_a_1,"; }		// 2 bit
								if ((iMask2 & 0x000000000004) != 0) { query += " p_b_2,"; }		// 3 bit

								if ((iMask2 & 0x000000000010) != 0) { query += " s_sum,"; }		// 5 bit

								if ((iMask2 & 0x000000000100) != 0) { query += " q_sum_geom,"; }// 9 bit
								if ((iMask2 & 0x000000000200) != 0) { query += " q_a_geom,"; }	// 10 bit
								if ((iMask2 & 0x000000000400) != 0) { query += " q_b_geom,"; }	// 11 bit
								if ((iMask2 & 0x000000000800) != 0) { query += " q_c_geom,"; }	// 12 bit
								if ((iMask2 & 0x000000001000) != 0)
								{
									query += " text(kp_sum) as kp_sum,";
								}// 13 bit

								if ((iMask2 & 0x000004000000) != 0) { query += " p_1,"; }		// 27 bit
								if ((iMask2 & 0x000008000000) != 0) { query += " p_2,"; }		// 28 bit
								if ((iMask2 & 0x100000000000) != 0) { query += " q_type,"; }	// 45 bit

								// если есть Р и Q значит есть тангенс Р
								if ((iMask2 & 0x000000000001) != 0 && (iMask2 & 0x000000000100) != 0)
								{ query += " tangent_p_geom,"; }	// 1, 9 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x000080000000) != 0)
								{ query += " tangent_p_shift,"; }	// 1, 32 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x000800000000) != 0)
								{ query += " tangent_p_cross,"; }	// 1, 36 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x008000000000) != 0)
								{ query += " tangent_p_1harm,"; }	// 1, 40 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask & 0x000080000000) != 0) { query += " q_a_shift,"; }	// 32 bit
								if ((iMask & 0x000800000000) != 0) { query += " q_a_cross,"; }	// 36 bit
								if ((iMask & 0x008000000000) != 0) { query += " q_a_1harm,"; }	// 40 bit
								/////////////////////////////////////////////////////////////////////
								if ((iMask2 & 0x000000000001) != 0) { query += " p_a_1,"; }		// 1 bit
								if ((iMask2 & 0x000000000010) != 0) { query += " s_a,"; }		// 5 bit
								if ((iMask2 & 0x000000000100) != 0) { query += " q_a_geom,"; }	// 9 bit

								if ((iMask2 & 0x000000001000) != 0)
								{
									query += " text(kp_a) as kp_a,";
								}	// 13 bit

								if ((iMask2 & 0x000004000000) != 0) { query += " p_1,"; }		// 27 bit
								if ((iMask2 & 0x100000000000) != 0) { query += " q_type,"; }	// 45 bit

								// если есть Р и Q значит есть тангенс Р
								if ((iMask2 & 0x000000000001) != 0 && (iMask2 & 0x000000000100) != 0)
								{ query += " tangent_p_geom,"; }	// 1, 9 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x000080000000) != 0)
								{ query += " tangent_p_shift,"; }	// 1, 32 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x000800000000) != 0)
								{ query += " tangent_p_cross,"; }	// 1, 36 bit
								if ((iMask2 & 0x000000000001) != 0 && (iMask & 0x008000000000) != 0)
								{ query += " tangent_p_1harm,"; }	// 1, 40 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для EtPQP если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, p_sum, p_a_1, p_b_2, p_c, s_sum, s_a, s_b, s_c, q_sum_geom, q_a_geom, q_b_geom, q_c_geom, text(kp_sum) as kp_sum, text(kp_a) as kp_a, text(kp_b) as kp_b, text(kp_c) as kp_c, p_0, p_1, p_2, q_type, tangent_p_geom, q_sum_shift, q_a_shift, q_b_shift, q_c_shift, q_sum_cross, q_a_cross, q_b_cross, q_c_cross, tangent_p_shift, tangent_p_cross, tangent_p_1harm, q_sum_1harm, q_a_1harm, q_b_1harm, q_c_1harm FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}
				else if (devType_ == EmDeviceType.EM32)
				{
					#region EM32

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask2 & 0x000000000001) != 0) { query += " p_sum,"; }		// 1 bit
								if ((iMask2 & 0x000000000002) != 0) { query += " p_a_1,"; }		// 2 bit
								if ((iMask2 & 0x000000000004) != 0) { query += " p_b_2,"; }		// 3 bit
								if ((iMask2 & 0x000000000008) != 0) { query += " p_c,"; }		// 4 bit
								if ((iMask2 & 0x000000000010) != 0) { query += " s_sum,"; }			// 5 bit
								if ((iMask2 & 0x000000000020) != 0) { query += " s_a,"; }			// 6 bit
								if ((iMask2 & 0x000000000040) != 0) { query += " s_b,"; }			// 7 bit
								if ((iMask2 & 0x000000000080) != 0) { query += " s_c,"; }			// 8 bit
								if ((iMask2 & 0x000000000100) != 0) { query += " q_sum,"; }			// 9 bit
								if ((iMask2 & 0x000000000200) != 0) { query += " q_a,"; }			// 10 bit
								if ((iMask2 & 0x000000000400) != 0) { query += " q_b,"; }			// 11 bit
								if ((iMask2 & 0x000000000800) != 0) { query += " q_c,"; }			// 12 bit
								if ((iMask2 & 0x000000001000) != 0) { query += " text(kp_sum) as kp_sum,"; }// 13 bit
								if ((iMask2 & 0x000000002000) != 0) { query += " text(kp_a) as kp_a,"; }// 14 bit
								if ((iMask2 & 0x000000004000) != 0) { query += " text(kp_b) as kp_b,"; }// 15 bit
								if ((iMask2 & 0x000000008000) != 0) { query += " text(kp_c) as kp_c,"; }// 16 bit
								if ((iMask2 & 0x000002000000) != 0) { query += " p_0,"; }			// 26 bit
								if ((iMask2 & 0x000004000000) != 0) { query += " p_1,"; }			// 27 bit
								if ((iMask2 & 0x000008000000) != 0) { query += " p_2,"; }			// 28 bit
								if ((iMask2 & 0x100000000000) != 0) { query += " q_type,"; }			// 45 bi
								// если есть Р и Q значит есть тангенс Р
								if ((iMask2 & 0x000000000001) != 0 && (iMask2 & 0x000000000100) != 0) 	// 1, 9 bit
								{ query += " tangent_p,"; }
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask2 & 0x000000000001) != 0) { query += " p_sum,"; }		// 1 bit
								if ((iMask2 & 0x000000000002) != 0) { query += " p_a_1,"; }		// 2 bit
								if ((iMask2 & 0x000000000004) != 0) { query += " p_b_2,"; }		// 3 bit
								if ((iMask2 & 0x000000000010) != 0) { query += " s_sum,"; }			// 5 bit
								if ((iMask2 & 0x000000000100) != 0) { query += " q_sum,"; }			// 9 bit
								if ((iMask2 & 0x000000000200) != 0) { query += " q_a,"; }			// 10 bit
								if ((iMask2 & 0x000000000400) != 0) { query += " q_b,"; }			// 11 bit
								if ((iMask2 & 0x000000000800) != 0) { query += " q_c,"; }			// 12 bit
								if ((iMask2 & 0x000000001000) != 0) { query += " text(kp_sum) as kp_sum,"; }// 13 bit
								if ((iMask2 & 0x000004000000) != 0) { query += " p_1,"; }			// 27 bit
								if ((iMask2 & 0x000008000000) != 0) { query += " p_2,"; }			// 28 bit
								if ((iMask2 & 0x100000000000) != 0) { query += " q_type,"; }		// 45 bit
								// если есть Р и Q значит есть тангенс Р
								if ((iMask2 & 0x000000000001) != 0 && (iMask2 & 0x000000000100) != 0) 	// 1, 9 bit
								{ query += " tangent_p,"; }
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask2 & 0x000000000001) != 0) { query += " p_a_1,"; }		// 1 bit
								if ((iMask2 & 0x000000000010) != 0) { query += " s_a,"; }		// 5 bit
								if ((iMask2 & 0x000000000100) != 0) { query += " q_a,"; }		// 9 bit
								if ((iMask2 & 0x000000001000) != 0) { query += " text(kp_a) as kp_a,"; }// 13 bit
								if ((iMask2 & 0x000004000000) != 0) { query += " p_1,"; }		// 27 bit
								if ((iMask2 & 0x100000000000) != 0) { query += " q_type,"; }	// 45 bit
								// если есть Р и Q значит есть тангенс Р
								if ((iMask2 & 0x000000000001) != 0 && (iMask2 & 0x000000000100) != 0) 	// 1, 9 bit
								{ query += " tangent_p,"; }
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для Em32 если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, p_sum, p_a_1, p_b_2, p_c, s_sum, s_a, s_b, s_c, q_sum, q_a, q_b, q_c, text(kp_sum) as kp_sum, text(kp_a) as kp_a, text(kp_b) as kp_b, text(kp_c) as kp_c, p_0, p_1, p_2, q_type, tangent_p FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_1_4", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rW = settings_.PowerRatio;

				float ctr = 1, vtr = 1;

				if (curWithTR)
				{
					if (devType_ == EmDeviceType.EM33T ||
						devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.EM32)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.ETPQP)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);

					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (devType_ != EmDeviceType.ETPQP)
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						if (iType == 1) vtr = fValue1 / fValue2;
						if (iType == 2) ctr = fValue1 / fValue2;
					}
				}
				rW *= vtr * ctr;

				EmService.WriteToLogDebug(string.Format("КТ применялись: {0} ; КТТ = {1} ; КТН = {2}",
					curWithTR, ctr, vtr));

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// powers
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if ((ds.Tables[0].Columns[i].Caption.Contains("p_") ||
							ds.Tables[0].Columns[i].Caption.Contains("s_") ||
							ds.Tables[0].Columns[i].Caption.Contains("q_")) &&
							(!ds.Tables[0].Columns[i].Caption.Contains("q_type") &&
							!ds.Tables[0].Columns[i].Caption.Contains("kp_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("hp_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("tangent"))
							)
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rW;
						}
					}
				}

				// добавляем коэффициенту мощности букву L или C
				int ncolPsum = ds.Tables[0].Columns.IndexOf("p_sum");
				int ncolQsumGeom = ds.Tables[0].Columns.IndexOf("q_sum_geom");
				int ncolQsumShift = ds.Tables[0].Columns.IndexOf("q_sum_shift");
				int ncolQsumCross = ds.Tables[0].Columns.IndexOf("q_sum_cross");
				int ncolQsum1harm = -1;
				if (devType_ == EmDeviceType.ETPQP)
					ncolQsum1harm = ds.Tables[0].Columns.IndexOf("q_sum_1harm");
				//if (curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K ||
				//	curDevType_ == EmDeviceType.EM33T1)
				//{
				//	ncolQsumGeom = ds.Tables[0].Columns.IndexOf("q_sum_geom");
				//}
				int ncolKpsum = ds.Tables[0].Columns.IndexOf("kp_sum");
				int ncolPa = ds.Tables[0].Columns.IndexOf("p_a_1");
				int ncolQaGeom = ds.Tables[0].Columns.IndexOf("q_a_geom");
				int ncolQaShift = ds.Tables[0].Columns.IndexOf("q_a_shift");
				int ncolQaCross = ds.Tables[0].Columns.IndexOf("q_a_cross");
				int ncolQa1harm = -1;
				if (devType_ == EmDeviceType.ETPQP)
					ncolQa1harm = ds.Tables[0].Columns.IndexOf("q_a_1harm");
				//if(curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K ||
				//    curDevType_ == EmDeviceType.EM33T1)
				//    ncolQa = ds.Tables[0].Columns.IndexOf("q_a_shift");
				int ncolKpa = ds.Tables[0].Columns.IndexOf("kp_a");
				int ncolPb = ds.Tables[0].Columns.IndexOf("p_b_2");
				int ncolQbGeom = ds.Tables[0].Columns.IndexOf("q_b_geom");
				int ncolQbShift = ds.Tables[0].Columns.IndexOf("q_b_shift");
				int ncolQbCross = ds.Tables[0].Columns.IndexOf("q_b_cross");
				int ncolQb1harm = -1;
				if (devType_ == EmDeviceType.ETPQP)
					ncolQb1harm = ds.Tables[0].Columns.IndexOf("q_b_1harm");
				//if (curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K ||
				//    curDevType_ == EmDeviceType.EM33T1)
				//    ncolQb = ds.Tables[0].Columns.IndexOf("q_b_shift");
				int ncolKpb = ds.Tables[0].Columns.IndexOf("kp_b");
				int ncolPc = ds.Tables[0].Columns.IndexOf("p_c");
				int ncolQcGeom = ds.Tables[0].Columns.IndexOf("q_c_geom");
				int ncolQcShift = ds.Tables[0].Columns.IndexOf("q_c_shift");
				int ncolQcCross = ds.Tables[0].Columns.IndexOf("q_c_cross");
				int ncolQc1harm = -1;
				if (devType_ == EmDeviceType.ETPQP)
					ncolQc1harm = ds.Tables[0].Columns.IndexOf("q_c_1harm");
				//if (curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K ||
				//    curDevType_ == EmDeviceType.EM33T1)
				//    ncolQc = ds.Tables[0].Columns.IndexOf("q_c_shift");
				int ncolKpc = ds.Tables[0].Columns.IndexOf("kp_c");

				bool curPsign, curQsign;
				float curPvalue, curQvalue;
				char curLetter;

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					if (ncolPsum >= 0 && ncolQsumGeom >= 0 && ncolKpsum >= 0)
					{
						if (Single.TryParse(ds.Tables[0].Rows[r][ncolPsum].ToString(), out curPvalue) &&
							Single.TryParse(ds.Tables[0].Rows[r][ncolQsumGeom].ToString(), out curQvalue))
						{
							curPsign = curPvalue > 0;
							curQsign = curQvalue > 0;

							curLetter = 'C';
							if ((curPsign && curQsign) || (!curPsign && !curQsign))
								curLetter = 'L';
							ds.Tables[0].Rows[r][ncolKpsum] =
								(ds.Tables[0].Rows[r][ncolKpsum]).ToString() + curLetter;
						}
					}

					if (ncolPa >= 0 && ncolQaGeom >= 0 && ncolKpa >= 0)
					{
						if (Single.TryParse(ds.Tables[0].Rows[r][ncolPa].ToString(), out curPvalue) &&
							Single.TryParse(ds.Tables[0].Rows[r][ncolQaGeom].ToString(), out curQvalue))
						{
							curPsign = curPvalue > 0;
							curQsign = curQvalue > 0;

							curLetter = 'C';
							if ((curPsign && curQsign) || (!curPsign && !curQsign))
								curLetter = 'L';
							ds.Tables[0].Rows[r][ncolKpa] =
								(ds.Tables[0].Rows[r][ncolKpa]).ToString() + curLetter;
						}
					}

					if (ncolPb >= 0 && ncolQbGeom >= 0 && ncolKpb >= 0)
					{
						if (Single.TryParse(ds.Tables[0].Rows[r][ncolPb].ToString(), out curPvalue) &&
							Single.TryParse(ds.Tables[0].Rows[r][ncolQbGeom].ToString(), out curQvalue))
						{
							curPsign = curPvalue > 0;
							curQsign = curQvalue > 0;

							curLetter = 'C';
							if ((curPsign && curQsign) || (!curPsign && !curQsign))
								curLetter = 'L';
							ds.Tables[0].Rows[r][ncolKpb] =
								(ds.Tables[0].Rows[r][ncolKpb]).ToString() + curLetter;
						}
					}

					if (ncolPc >= 0 && ncolQcGeom >= 0 && ncolKpc >= 0)
					{
						if (Single.TryParse(ds.Tables[0].Rows[r][ncolPc].ToString(), out curPvalue) &&
							Single.TryParse(ds.Tables[0].Rows[r][ncolQcGeom].ToString(), out curQvalue))
						{
							curPsign = curPvalue > 0;
							curQsign = curQvalue > 0;

							curLetter = 'C';
							if ((curPsign && curQsign) || (!curPsign && !curQsign))
								curLetter = 'L';
							ds.Tables[0].Rows[r][ncolKpc] =
								(ds.Tables[0].Rows[r][ncolKpc]).ToString() + curLetter;
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_1_4");

				mainWnd_.CMs_[(int)AvgPages.POWER] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.POWER].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.POWER].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPower::load_not_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}
	}

	internal class AVGDataGridWrapperPQP : AVGDataGridWrapperBase
	{
		float iLimit_;

		public AVGDataGridWrapperPQP(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme, float iLimit,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ iLimit_ = iLimit; }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_perc = rm.GetString("column_header_units_percent");
				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");
				string unit_grad = rm.GetString("column_header_units_grad");
				string unit_a = (settings_.CurrentRatio == 1) ?
					rm.GetString("column_header_units_a") :
					rm.GetString("column_header_units_ka");
				string unit_w = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_w") :
					rm.GetString("column_header_units_kw");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "avg_pqp";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// U1
					DataGridColumnHeaderFormula cs_U1 =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "U_1_" + unit_v);
					cs_U1.MappingName = "u_1";
					cs_U1.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U1);

					// U2
					DataGridColumnHeaderFormula cs_U2 =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "U_2_" + unit_v);
					cs_U2.MappingName = "u_2";
					cs_U2.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U2);

					// U0
					DataGridColumnHeaderFormula cs_U0 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "U_0_" + unit_v);
					cs_U0.MappingName = "u_0";
					cs_U0.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_U0);

					// K_2U
					DataGridColumnHeaderFormula cs_K_2U =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "K_2U_" + unit_perc);
					cs_K_2U.MappingName = "k_2u";
					cs_K_2U.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_2U);

					// K_0U
					DataGridColumnHeaderFormula cs_K_0U =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "K_0U_" + unit_perc);
					cs_K_0U.MappingName = "k_0u";
					cs_K_0U.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_0U);

					if (iLimit_ != 0)
					{
						// I1
						DataGridColumnHeaderFormula cs_I1 =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_1_" + unit_a);
						cs_I1.MappingName = "i_1";
						cs_I1.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I1);

						// I2
						DataGridColumnHeaderFormula cs_I2 =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_2_" + unit_a);
						cs_I2.MappingName = "i_2";
						cs_I2.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I2);

						// I0
						DataGridColumnHeaderFormula cs_I0 =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_0_" + unit_a);
						cs_I0.MappingName = "i_0";
						cs_I0.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_I0);

						// P1
						DataGridColumnHeaderFormula cs_P1 =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_1_" + unit_w);
						cs_P1.MappingName = "p_1";
						cs_P1.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P1);

						// P2
						DataGridColumnHeaderFormula cs_P2 =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_2_" + unit_w);
						cs_P2.MappingName = "p_2";
						cs_P2.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P2);

						// P0
						DataGridColumnHeaderFormula cs_P0 =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_0_" + unit_w);
						cs_P0.MappingName = "p_0";
						cs_P0.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_P0);

						// angle_p_1
						DataGridColumnHeaderFormula cs_angle_1 =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "<P_1" + unit_grad);
						cs_angle_1.MappingName = "angle_p_1";
						cs_angle_1.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_angle_1);

						// angle_p_2
						DataGridColumnHeaderFormula cs_angle_2 =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "<P_2" + unit_grad);
						cs_angle_2.MappingName = "angle_p_2";
						cs_angle_2.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_angle_2);

						// angle_p_0
						DataGridColumnHeaderFormula cs_angle_0 =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "<P_0" + unit_grad);
						cs_angle_0.MappingName = "angle_p_0";
						cs_angle_0.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_angle_0);
					}

					// Отклонение установившегося напряжения [относительное]
					// δU_y
					DataGridColumnHeaderFormula cs_dU_y =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						(settings_.CurrentLanguage == "ru" ? "δUrel_y_" : "δUrel_s_") + unit_perc);
					cs_dU_y.MappingName = "rd_u";
					cs_dU_y.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_dU_y);
				}

				// Отклонение 1 гармоники от номинала [относительное]
				for (int iPhase = 0; iPhase < 6; ++iPhase)
				{
					if (connectScheme_ == ConnectScheme.Ph1W2 && iPhase > 0) continue;
					if ((connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc) &&
						iPhase < 3) continue;

					DataGridColumnHeaderFormula cs_dU_1harm =
						new DataGridColumnHeaderFormula(
							PhasesInfo.GetPhase6Color(iPhase),
							string.Format("δUrel_(1) {0}_", PhasesInfo.GetPhase6Name(iPhase)) + unit_perc);
					cs_dU_1harm.MappingName = string.Format("rd_u_1harm_{0}", PhasesInfo.GetPhase6Name(iPhase).ToLower());
					cs_dU_1harm.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_dU_1harm);
				}

				// Положительное отклонение напряжения [абсолютное]
				for (int iPhase = 0; iPhase < 6; ++iPhase)
				{
					if (connectScheme_ == ConnectScheme.Ph1W2 && iPhase > 0) continue;
					if ((connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc) &&
						iPhase < 3) continue;

					DataGridColumnHeaderFormula cs_dU =
						new DataGridColumnHeaderFormula(
							PhasesInfo.GetPhase6Color(iPhase),
							string.Format("δU_+ {0}_", PhasesInfo.GetPhase6Name(iPhase)) + unit_v);
					cs_dU.MappingName = string.Format("d_u_pos_{0}", PhasesInfo.GetPhase6Name(iPhase).ToLower());
					cs_dU.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_dU);
				}

				// Отрицательное отклонение напряжения [абсолютное]
				for (int iPhase = 0; iPhase < 6; ++iPhase)
				{
					if (connectScheme_ == ConnectScheme.Ph1W2 && iPhase > 0) continue;
					if ((connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc) &&
						iPhase < 3) continue;

					DataGridColumnHeaderFormula cs_dU =
						new DataGridColumnHeaderFormula(
							PhasesInfo.GetPhase6Color(iPhase),
							string.Format("δU_- {0}_", PhasesInfo.GetPhase6Name(iPhase)) + unit_v);
					cs_dU.MappingName = string.Format("d_u_neg_{0}", PhasesInfo.GetPhase6Name(iPhase).ToLower());
					cs_dU.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_dU);
				}

				// Положительное отклонение напряжения [относительное]
				for (int iPhase = 0; iPhase < 6; ++iPhase)
				{
					if (connectScheme_ == ConnectScheme.Ph1W2 && iPhase > 0) continue;
					if ((connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc) &&
						iPhase < 3) continue;

					DataGridColumnHeaderFormula cs_rdU =
						new DataGridColumnHeaderFormula(
							PhasesInfo.GetPhase6Color(iPhase),
							string.Format("δUrel_+ {0}_", PhasesInfo.GetPhase6Name(iPhase)) + unit_perc);
					cs_rdU.MappingName = string.Format("rd_u_pos_{0}", PhasesInfo.GetPhase6Name(iPhase).ToLower());
					cs_rdU.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_rdU);
				}

				// Отрицательное отклонение напряжения [относительное]
				for (int iPhase = 0; iPhase < 6; ++iPhase)
				{
					if (connectScheme_ == ConnectScheme.Ph1W2 && iPhase > 0) continue;
					if ((connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc) &&
						iPhase < 3) continue;

					DataGridColumnHeaderFormula cs_rdU =
						new DataGridColumnHeaderFormula(
							PhasesInfo.GetPhase6Color(iPhase),
							string.Format("δUrel_- {0}_", PhasesInfo.GetPhase6Name(iPhase)) + unit_perc);
					cs_rdU.MappingName = string.Format("rd_u_neg_{0}", PhasesInfo.GetPhase6Name(iPhase).ToLower());
					cs_rdU.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_rdU);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPQP::init_etPQP_A()");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_perc = rm.GetString("column_header_units_percent");
				string unit_hz = rm.GetString("column_header_units_hz");
				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");
				string unit_grad = rm.GetString("column_header_units_grad");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "period_avg_params_1_4";
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

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// δU_y
					DataGridColumnHeaderFormula cs_delta_U_y =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						(settings_.CurrentLanguage == "ru" ? "δU_y_" : "δU_s_") + unit_perc);
					cs_delta_U_y.MappingName = "d_u_y";
					cs_delta_U_y.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_delta_U_y);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// δU_A
					DataGridColumnHeaderFormula cs_delta_U_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "δU_A_" + unit_perc);
					cs_delta_U_a.MappingName = "d_u_a";
					cs_delta_U_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_delta_U_a);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
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
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
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

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// K_2U
					DataGridColumnHeaderFormula cs_K_2U =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "K_2U_" + unit_perc);
					cs_K_2U.MappingName = "k_2u";
					cs_K_2U.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_2U);
				}
				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// K_0U
					DataGridColumnHeaderFormula cs_K_0U =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "K_0U_" + unit_perc);
					cs_K_0U.MappingName = "k_0u";
					cs_K_0U.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_0U);
				}

				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// K_UAB
					DataGridColumnHeaderFormula cs_K_U_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						(settings_.CurrentLanguage == "ru" ? "K_U AB_" : "THD_U AB_") + unit_perc);
					cs_K_U_a.MappingName = "k_ua_ab";
					cs_K_U_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_U_a);
				}
				else
				{
					// K_UA
					DataGridColumnHeaderFormula cs_K_U_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						(settings_.CurrentLanguage == "ru" ? "K_U A_" : "THD_U A_") + unit_perc);
					cs_K_U_a.MappingName = "k_ua_ab";
					cs_K_U_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_U_a);
				}

				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// K_UBC
					DataGridColumnHeaderFormula cs_K_U_b =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						(settings_.CurrentLanguage == "ru" ? "K_U BC_" : "THD_U BC_") + unit_perc);
					cs_K_U_b.MappingName = "k_ub_bc";
					cs_K_U_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_U_b);
				}
				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// K_UB
					DataGridColumnHeaderFormula cs_K_U_b =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						(settings_.CurrentLanguage == "ru" ? "K_U B_" : "THD_U B_") + unit_perc);
					cs_K_U_b.MappingName = "k_ub_bc";
					cs_K_U_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_U_b);
				}

				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// K_UCA
					DataGridColumnHeaderFormula cs_K_U_c =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						(settings_.CurrentLanguage == "ru" ? "K_U CA_" : "THD_U CA_") + unit_perc);
					cs_K_U_c.MappingName = "k_uc_ca";
					cs_K_U_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_U_c);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// K_UC
					DataGridColumnHeaderFormula cs_K_U_c =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						(settings_.CurrentLanguage == "ru" ? "K_U C" : "THD_U C") + unit_perc);
					cs_K_U_c.MappingName = "k_uc_ca";
					cs_K_U_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_U_c);
				}

				// K_IA
				DataGridColumnHeaderFormula cs_K_I_a =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
					(settings_.CurrentLanguage.Equals("ru") ? "K_I A_" : "THD_I A_") + unit_perc);
				cs_K_I_a.MappingName = "k_ia";
				cs_K_I_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_K_I_a);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// K_IB
					DataGridColumnHeaderFormula cs_K_I_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						(settings_.CurrentLanguage.Equals("ru") ? "K_I B_" : "THD_I B_") + unit_perc);
					cs_K_I_B.MappingName = "k_ib";
					cs_K_I_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_I_B);

					// K_IC
					DataGridColumnHeaderFormula cs_K_I_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						(settings_.CurrentLanguage.Equals("ru") ? "K_I C_" : "THD_I C_") + unit_perc);
					cs_K_I_C.MappingName = "k_ic";
					cs_K_I_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_K_I_C);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPQP::init_not_etPQP_A()");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				string query = String.Format("SELECT p.datetime_id, p.record_id, dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, u_1, u_2, u_0, k_2u, k_0u, i_1, i_2, i_0, p_1, p_2, p_0, angle_p_1, angle_p_2, angle_p_0, rd_u, rd_u_1harm_a, rd_u_1harm_b, rd_u_1harm_c, rd_u_1harm_ab, rd_u_1harm_bc, rd_u_1harm_ca, d_u_pos_a, d_u_pos_b, d_u_pos_c, d_u_pos_ab, d_u_pos_bc, d_u_pos_ca, d_u_neg_a, d_u_neg_b, d_u_neg_c, d_u_neg_ab, d_u_neg_bc, d_u_neg_ca, rd_u_pos_a, rd_u_pos_b, rd_u_pos_c, rd_u_pos_ab, rd_u_pos_bc, rd_u_pos_ca, rd_u_neg_a, rd_u_neg_b, rd_u_neg_c, rd_u_neg_ab, rd_u_neg_bc, rd_u_neg_ca FROM avg_pqp p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_pqp", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rU = settings_.VoltageRatio;
				float rI = settings_.CurrentRatio;
				float rW = settings_.PowerRatio;

				//float ctr = 1, vtr = 1;

				//if (curWithTR)
				//{
				//    sqlCommand = new NpgsqlCommand();
				//    sqlCommand.Connection = conEmDb;
				//    commandText = string.Format("SELECT * FROM turn_ratios WHERE reg_id = (SELECT registration_id FROM avg_times where datetime_id = {0});", curDatetimeId_);

				//    dbService.ExecuteReader(commandText);

				//    while (dbService.DataReaderRead())
				//    {
				//        short iType = (short)dbService.DataReaderData("turn_type"];
				//        float fValue1 = (float)dbService.DataReaderData("value1"];
				//        float fValue2 = (float)dbService.DataReaderData("value2"];

				//        if (iType == 1) vtr = fValue1 / fValue2;
				//        if (iType == 2) ctr = fValue1 / fValue2;
				//    }
				//}
				//rU *= vtr;
				//rI *= ctr;
				//rW *= vtr * ctr;

				//EmService.WriteToLogDebug(string.Format("КТ применялись: {0} ; КТТ = {1} ; КТН = {2}",
				//    curWithTR, ctr, vtr));

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// voltages
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("u"))
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rU;
						}
					}

					// currents	
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("i_"))
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rI;
						}
					}
					// powers
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if ((ds.Tables[0].Columns[i].Caption.Contains("p_") ||
							ds.Tables[0].Columns[i].Caption.Contains("s_") ||
							ds.Tables[0].Columns[i].Caption.Contains("q_")) &&
							(!ds.Tables[0].Columns[i].Caption.Contains("q_type") &&
							!ds.Tables[0].Columns[i].Caption.Contains("kp_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("hp_") &&
							!ds.Tables[0].Columns[i].Caption.Contains("tangent"))
							)
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rW;
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_pqp");

				mainWnd_.CMs_[(int)AvgPages.PQP] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.PQP].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.PQP].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPQP::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_1_4");

				string query = "";
				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
				{
					query += String.Format(" SELECT datetime_id, event_datetime, d_f, d_u_y, d_u_a, d_u_b, d_u_c, d_u_ab, d_u_bc, d_u_ca, k_2u, k_0u, k_ua_ab, k_ub_bc, k_uc_ca, k_ia, k_ib, k_ic FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_);
				}
				else if (devType_ == EmDeviceType.ETPQP)
				{
					#region ETPQP

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask2 & 0x000010000000) != 0) { query += " d_f,"; }	// 29 bit
								if ((iMask2 & 0x000020000000) != 0) { query += " d_u_y,"; }	// 30 bit
								if ((iMask2 & 0x000040000000) != 0) { query += " d_u_a,"; }	// 31 bit
								if ((iMask2 & 0x000080000000) != 0) { query += " d_u_b,"; }	// 32 bit
								if ((iMask2 & 0x000100000000) != 0) { query += " d_u_c,"; }	// 33 bit
								if ((iMask2 & 0x000200000000) != 0) { query += " d_u_ab,"; }// 34 bit
								if ((iMask2 & 0x000400000000) != 0) { query += " d_u_bc,"; }// 35 bit
								if ((iMask2 & 0x000800000000) != 0) { query += " d_u_ca,"; }// 36 bit
								if ((iMask2 & 0x001000000000) != 0) { query += " k_2u,"; }	// 37 bit
								if ((iMask2 & 0x002000000000) != 0) { query += " k_0u,"; }	// 38 bit
								if ((iMask2 & 0x004000000000) != 0) { query += " k_ua_ab,"; }// 39 bit
								if ((iMask2 & 0x008000000000) != 0) { query += " k_ub_bc,"; }// 40 bit
								if ((iMask2 & 0x010000000000) != 0) { query += " k_uc_ca,"; }// 41 bit
								if ((iMask2 & 0x020000000000) != 0) { query += " k_ia,"; }	// 42 bit
								if ((iMask2 & 0x040000000000) != 0) { query += " k_ib,"; }	// 43 bit
								if ((iMask2 & 0x080000000000) != 0) { query += " k_ic,"; }	// 44 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask2 & 0x000010000000) != 0) { query += " d_f,"; }		// 29 bit
								if ((iMask2 & 0x000020000000) != 0) { query += " d_u_y,"; }		// 30 bit

								if ((iMask2 & 0x000200000000) != 0) { query += " d_u_ab,"; }	// 34 bit
								if ((iMask2 & 0x000400000000) != 0) { query += " d_u_bc,"; }	// 35 bit
								if ((iMask2 & 0x000800000000) != 0) { query += " d_u_ca,"; }	// 36 bit
								if ((iMask2 & 0x001000000000) != 0) { query += " k_2u,"; }		// 37 bit

								if ((iMask2 & 0x004000000000) != 0) { query += " k_ua_ab,"; }	// 39 bit
								if ((iMask2 & 0x008000000000) != 0) { query += " k_ub_bc,"; }	// 40 bit
								if ((iMask2 & 0x010000000000) != 0) { query += " k_uc_ca,"; }	// 41 bit
								if ((iMask2 & 0x020000000000) != 0) { query += " k_ia,"; }		// 42 bit
								if ((iMask2 & 0x040000000000) != 0) { query += " k_ib,"; }		// 43 bit
								if ((iMask2 & 0x080000000000) != 0) { query += " k_ic,"; }		// 44 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask2 & 0x000010000000) != 0) { query += " d_f,"; }		// 29 bit
								if ((iMask2 & 0x000020000000) != 0) { query += " d_u_a,"; }		// 30 bit
								if ((iMask2 & 0x004000000000) != 0) { query += " k_ua_ab,"; }	// 39 bit
								if ((iMask2 & 0x020000000000) != 0) { query += " k_ia,"; }		// 42 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для EtPQP если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, d_f, d_u_y, d_u_a, d_u_b, d_u_c, d_u_ab, d_u_bc, d_u_ca, k_2u, k_0u, k_ua_ab, k_ub_bc, k_uc_ca, k_ia, k_ib, k_ic FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}
				else if (devType_ == EmDeviceType.EM32)
				{
					#region EM32

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask2 & 0x000010000000) != 0) { query += " d_f,"; }			// 29 bit
								if ((iMask2 & 0x000020000000) != 0) { query += " d_u_y,"; }			// 30 bit
								if ((iMask2 & 0x000040000000) != 0) { query += " d_u_a,"; }			// 31 bit
								if ((iMask2 & 0x000080000000) != 0) { query += " d_u_b,"; }			// 32 bit
								if ((iMask2 & 0x000100000000) != 0) { query += " d_u_c,"; }			// 33 bit
								if ((iMask2 & 0x000200000000) != 0) { query += " d_u_ab,"; }		// 34 bit
								if ((iMask2 & 0x000400000000) != 0) { query += " d_u_bc,"; }		// 35 bit
								if ((iMask2 & 0x000800000000) != 0) { query += " d_u_ca,"; }		// 36 bit
								if ((iMask2 & 0x001000000000) != 0) { query += " k_2u,"; }			// 37 bit
								if ((iMask2 & 0x002000000000) != 0) { query += " k_0u,"; }			// 38 bit
								if ((iMask2 & 0x004000000000) != 0) { query += " k_ua_ab,"; }		// 39 bit
								if ((iMask2 & 0x008000000000) != 0) { query += " k_ub_bc,"; }		// 40 bit
								if ((iMask2 & 0x010000000000) != 0) { query += " k_uc_ca,"; }		// 41 bit
								if ((iMask2 & 0x020000000000) != 0) { query += " k_ia,"; }			// 42 bit
								if ((iMask2 & 0x040000000000) != 0) { query += " k_ib,"; }			// 43 bit
								if ((iMask2 & 0x080000000000) != 0) { query += " k_ic,"; }			// 44 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask2 & 0x000010000000) != 0) { query += " d_f,"; }			// 29 bit
								if ((iMask2 & 0x000020000000) != 0) { query += " d_u_y,"; }			// 30 bit
								if ((iMask2 & 0x000200000000) != 0) { query += " d_u_ab,"; }		// 34 bit
								if ((iMask2 & 0x000400000000) != 0) { query += " d_u_bc,"; }		// 35 bit
								if ((iMask2 & 0x000800000000) != 0) { query += " d_u_ca,"; }		// 36 bit
								if ((iMask2 & 0x001000000000) != 0) { query += " k_2u,"; }			// 37 bit
								if ((iMask2 & 0x004000000000) != 0) { query += " k_ua_ab,"; }		// 39 bit
								if ((iMask2 & 0x008000000000) != 0) { query += " k_ub_bc,"; }		// 40 bit
								if ((iMask2 & 0x010000000000) != 0) { query += " k_uc_ca,"; }		// 41 bit
								if ((iMask2 & 0x020000000000) != 0) { query += " k_ia,"; }			// 42 bit
								if ((iMask2 & 0x040000000000) != 0) { query += " k_ib,"; }			// 43 bit
								if ((iMask2 & 0x080000000000) != 0) { query += " k_ic,"; }			// 44 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask2 & 0x000010000000) != 0) { query += " d_f,"; }		// 29 bit
								if ((iMask2 & 0x000020000000) != 0) { query += " d_u_a,"; }		// 30 bit
								if ((iMask2 & 0x004000000000) != 0) { query += " k_ua_ab,"; }	// 39 bit
								if ((iMask2 & 0x020000000000) != 0) { query += " k_ia,"; }		// 42 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для Em32 если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, d_f, d_u_y, d_u_a, d_u_b, d_u_c, d_u_ab, d_u_bc, d_u_ca, k_2u, k_0u, k_ua_ab, k_ub_bc, k_uc_ca, k_ia, k_ib, k_ic FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_1_4", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_1_4");

				mainWnd_.CMs_[(int)AvgPages.PQP] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.PQP].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.PQP].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperPQP::load_not_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}
	}

	internal class AVGDataGridWrapperAngles : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperAngles(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_grad = rm.GetString("column_header_units_grad");

				dataGrid_.TableStyles.Clear();
				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "avg_angles";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// ∟U1_A_ U1_B
					DataGridColumnHeaderFormula cs_aU1_A_U1_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						"<U_A(1)_ U_B(1)_" + unit_grad);
					cs_aU1_A_U1_B.MappingName = "angle_ua_ub";
					cs_aU1_A_U1_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_A_U1_B);

					// ∟U1_B_ U1_C
					DataGridColumnHeaderFormula cs_aU1_B_U1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						"<U_B(1)_ U_C(1)_" + unit_grad);
					cs_aU1_B_U1_C.MappingName = "angle_ub_uc";
					cs_aU1_B_U1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_B_U1_C);

					// ∟U1_C_ U1_A
					DataGridColumnHeaderFormula cs_aU1_C_U1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						"<U_C(1)_ U_A(1)_" + unit_grad);
					cs_aU1_C_U1_A.MappingName = "angle_uc_ua";
					cs_aU1_C_U1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_C_U1_A);
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 && connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					// ∟U1_A_ I1_A
					DataGridColumnHeaderFormula cs_aU1_A_I1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						"<U_A(1)_ I_A(1)_" + unit_grad);
					cs_aU1_A_I1_A.MappingName = "angle_ua_ia";
					cs_aU1_A_I1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_A_I1_A);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// ∟U1_B_ I1_B
					DataGridColumnHeaderFormula cs_BU1_B_I1_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						"<U_B(1)_ I_B(1)_" + unit_grad);
					cs_BU1_B_I1_B.MappingName = "angle_ub_ib";
					cs_BU1_B_I1_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_BU1_B_I1_B);

					// ∟U1_C_ I1_C
					DataGridColumnHeaderFormula cs_CU1_C_I1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						"<U_C(1)_ I_C(1)_" + unit_grad);
					cs_CU1_C_I1_C.MappingName = "angle_uc_ic";
					cs_CU1_C_I1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_CU1_C_I1_C);
				}

				if (connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					// ∟U1_AB_ U1_CB
					DataGridColumnHeaderFormula cs_aU1_AB_U1_CB =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						"<U_AB(1)_ U_CB(1)_" + unit_grad);
					cs_aU1_AB_U1_CB.MappingName = "angle_uab_ucb";
					cs_aU1_AB_U1_CB.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_AB_U1_CB);

					// ∟U1_AB_ I1_A
					DataGridColumnHeaderFormula cs_aU1_AB_I1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						"<U_AB(1)_ I_A(1)_" + unit_grad);
					cs_aU1_AB_I1_A.MappingName = "angle_uab_ia";
					cs_aU1_AB_I1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_AB_I1_A);

					// ∟U1_CB_ I1_C
					DataGridColumnHeaderFormula cs_aU1_CB_I1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						"<U_CB(1)_ I_C(1)_" + unit_grad);
					cs_aU1_CB_I1_C.MappingName = "angle_ucb_ic";
					cs_aU1_CB_I1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_CB_I1_C);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperAngles::init_etPQP_A()");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string unit_grad = rm.GetString("column_header_units_grad");

				dataGrid_.TableStyles.Clear();
				DataGridTableStyle ts = new DataGridTableStyle();

				ts.MappingName = "period_avg_params_1_4";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time"); cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// ∟U1_A_ U1_B
					DataGridColumnHeaderFormula cs_aU1_A_U1_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						"<U_A(1)_ U_B(1)_" + unit_grad);
					cs_aU1_A_U1_B.MappingName = "an_u1_a_u1_b";
					cs_aU1_A_U1_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_A_U1_B);

					// ∟U1_B_ U1_C
					DataGridColumnHeaderFormula cs_aU1_B_U1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						"<U_B(1)_ U_C(1)_" + unit_grad);
					cs_aU1_B_U1_C.MappingName = "an_u1_b_u1_c";
					cs_aU1_B_U1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_B_U1_C);

					// ∟U1_C_ U1_A
					DataGridColumnHeaderFormula cs_aU1_C_U1_A =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						"<U_C(1)_ U_A(1)_" + unit_grad);
					cs_aU1_C_U1_A.MappingName = "an_u1_c_u1_a";
					cs_aU1_C_U1_A.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU1_C_U1_A);
				}

				// ∟U1_A_ I1_A
				DataGridColumnHeaderFormula cs_aU1_A_I1_A =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
					"<U_A(1)_ I_A(1)_" + unit_grad);
				cs_aU1_A_I1_A.MappingName = "an_u1_a_i1_a";
				cs_aU1_A_I1_A.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_aU1_A_I1_A);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// ∟U1_B_ I1_B
					DataGridColumnHeaderFormula cs_BU1_B_I1_B =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
						"<U_B(1)_ I_B(1)_" + unit_grad);
					cs_BU1_B_I1_B.MappingName = "an_u1_b_i1_b";
					cs_BU1_B_I1_B.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_BU1_B_I1_B);

					// ∟U1_C_ I1_C
					DataGridColumnHeaderFormula cs_CU1_C_I1_C =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
						"<U_C(1)_ I_C(1)_" + unit_grad);
					cs_CU1_C_I1_C.MappingName = "an_u1_c_i1_c";
					cs_CU1_C_I1_C.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_CU1_C_I1_C);
				}

				// ∟U_1_ I_1
				DataGridColumnHeaderFormula cs_aU_1_I_1 =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "<U_y_ I_1(1)_" + unit_grad);
				cs_aU_1_I_1.MappingName = "an_u_1_i_1";
				cs_aU_1_I_1.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_aU_1_I_1);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// ∟U_2_ I_2
					DataGridColumnHeaderFormula cs_aU_2_I_2 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						"<U_2(1)_ I_2(1)_" + unit_grad);
					cs_aU_2_I_2.MappingName = "an_u_2_i_2";
					cs_aU_2_I_2.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU_2_I_2);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					// ∟U_0_ I_0
					DataGridColumnHeaderFormula cs_aU_0_I_0 =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
						"<U_0(1)_ I_0(1)_" + unit_grad);
					cs_aU_0_I_0.MappingName = "an_u_0_i_0";
					cs_aU_0_I_0.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_aU_0_I_0);
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperAngles::init_not_etPQP_A()");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				string query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
case when angle_ua_ub > 360 OR angle_ua_ub < -360 then '-' else cast(angle_ua_ub as text) end as angle_ua_ub,
case when angle_ub_uc > 360 OR angle_ub_uc < -360 then '-' else cast(angle_ub_uc as text) end as angle_ub_uc,
case when angle_uc_ua > 360 OR angle_uc_ua < -360 then '-' else cast(angle_uc_ua as text) end as angle_uc_ua, 
case when angle_ua_ia > 360 OR angle_ua_ia < -360 then '-' else cast(angle_ua_ia as text) end as angle_ua_ia,
case when angle_ub_ib > 360 OR angle_ub_ib < -360 then '-' else cast(angle_ub_ib as text) end as angle_ub_ib,
case when angle_uc_ic > 360 OR angle_uc_ic < -360 then '-' else cast(angle_uc_ic as text) end as angle_uc_ic,
case when angle_uab_ucb > 360 OR angle_uab_ucb < -360 then '-' else cast(angle_uab_ucb as text) end as angle_uab_ucb,
case when angle_uab_ia > 360 OR angle_uab_ia < -360 then '-' else cast(angle_uab_ia as text) end as angle_uab_ia,
case when angle_ucb_ic > 360 OR angle_ucb_ic < -360 then '-' else cast(angle_ucb_ic as text) end as angle_ucb_ic FROM avg_angles p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_angles", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_angles");

				mainWnd_.CMs_[(int)AvgPages.ANGLES] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.ANGLES].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.ANGLES].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperAngles::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_1_4");

				string query = "";
				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
				{
					query += String.Format(" SELECT datetime_id, event_datetime, an_u1_a_u1_b, an_u1_b_u1_c, an_u1_c_u1_a, an_u1_a_i1_a, an_u1_b_i1_b, an_u1_c_i1_c, an_u_1_i_1, an_u_2_i_2, an_u_0_i_0 FROM period_avg_params_1_4 WHERE datetime_id = {0};", curDatetimeId_);
				}
				else if (devType_ == EmDeviceType.ETPQP)
				{
					#region ETPQP

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask2 & 0x000000010000) != 0)
								{
									query += " an_u1_a_u1_b,";
								}	// 17 bit
								if ((iMask2 & 0x000000020000) != 0)
								{
									query += " an_u1_b_u1_c,";
								}	// 18 bit
								if ((iMask2 & 0x000000040000) != 0)
								{
									query += " an_u1_c_u1_a,";
								}	// 19 bit
								if ((iMask2 & 0x000000080000) != 0)
								{
									query += " an_u1_a_i1_a,";
								}	// 20 bit
								if ((iMask2 & 0x000000100000) != 0)
								{
									query += " an_u1_b_i1_b,";
								}	// 21 bit
								if ((iMask2 & 0x000000200000) != 0)
								{
									query += " an_u1_c_i1_c,";
								}	// 22 bit
								if ((iMask2 & 0x000000400000) != 0)
								{
									query += " an_u_1_i_1,";
								}	// 23 bit
								if ((iMask2 & 0x000000800000) != 0)
								{
									query += " an_u_2_i_2,";
								}	// 24 bit
								if ((iMask2 & 0x000001000000) != 0)
								{
									query += " an_u_0_i_0,";
								}	// 25 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask2 & 0x000000010000) != 0)
								{
									query += " an_u1_a_u1_b,";
								}		// 17 bit
								if ((iMask2 & 0x000000020000) != 0)
								{
									query += " an_u1_b_u1_c,";
								}		// 18 bit
								if ((iMask2 & 0x000000040000) != 0)
								{
									query += " an_u1_c_u1_a,";
								}		// 19 bit
								if ((iMask2 & 0x000000080000) != 0)
								{
									query += " an_u1_a_i1_a,";
								}		// 20 bit
								if ((iMask2 & 0x000000100000) != 0)
								{
									query += " an_u1_b_i1_b,";
								}		// 21 bit
								if ((iMask2 & 0x000000200000) != 0)
								{
									query += " an_u1_c_i1_c,";
								}		// 22 bit
								if ((iMask2 & 0x000000400000) != 0) { query += " an_u_1_i_1,"; }// 23 bit
								if ((iMask2 & 0x000000800000) != 0) { query += " an_u_2_i_2,"; }// 24 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask2 & 0x000000080000) != 0)
								{
									query += " an_u1_a_i1_a,";
								}		// 20 bit
								if ((iMask2 & 0x000000400000) != 0) { query += " an_u_1_i_1,"; }// 23 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для EtPQP если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, an_u1_a_u1_b, an_u1_b_u1_c, an_u1_c_u1_a, an_u1_a_i1_a, an_u1_b_i1_b, an_u1_c_i1_c, an_u_1_i_1, an_u_2_i_2, an_u_0_i_0 FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}
				else if (devType_ == EmDeviceType.EM32)
				{
					#region EM32

					if (iMask != -1 || iMask2 != -1)
					{
						query = " SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask2 & 0x000000010000) != 0) { query += " an_u1_a_u1_b,"; }	// 17 bit
								if ((iMask2 & 0x000000020000) != 0) { query += " an_u1_b_u1_c,"; }	// 18 bit
								if ((iMask2 & 0x000000040000) != 0) { query += " an_u1_c_u1_a,"; }	// 19 bit
								if ((iMask2 & 0x000000080000) != 0) { query += " an_u1_a_i1_a,"; }	// 20 bit
								if ((iMask2 & 0x000000100000) != 0) { query += " an_u1_b_i1_b,"; }	// 21 bit
								if ((iMask2 & 0x000000200000) != 0) { query += " an_u1_c_i1_c,"; }	// 22 bit
								if ((iMask2 & 0x000000400000) != 0) { query += " an_u_1_i_1,"; }	// 23 bit
								if ((iMask2 & 0x000000800000) != 0) { query += " an_u_2_i_2,"; }	// 24 bit
								if ((iMask2 & 0x000001000000) != 0) { query += " an_u_0_i_0,"; }	// 25 bit
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask2 & 0x000000010000) != 0) { query += " an_u1_a_u1_b,"; }		// 17 bit
								if ((iMask2 & 0x000000020000) != 0) { query += " an_u1_b_u1_c,"; }		// 18 bit
								if ((iMask2 & 0x000000040000) != 0) { query += " an_u1_c_u1_a,"; }		// 19 bit
								if ((iMask2 & 0x000000080000) != 0) { query += " an_u1_a_i1_a,"; }		// 20 bit
								if ((iMask2 & 0x000000100000) != 0) { query += " an_u1_b_i1_b,"; }		// 21 bit
								if ((iMask2 & 0x000000200000) != 0) { query += " an_u1_c_i1_c,"; }		// 22 bit
								if ((iMask2 & 0x000000400000) != 0) { query += " an_u_1_i_1,"; }		// 23 bit
								if ((iMask2 & 0x000000800000) != 0) { query += " an_u_2_i_2,"; }		// 24 bit
								break;

							case ConnectScheme.Ph1W2:
								if ((iMask2 & 0x000000080000) != 0) { query += " an_u1_a_i1_a,"; }		// 20 bit
								if ((iMask2 & 0x000000400000) != 0) { query += " an_u_1_i_1,"; }		// 23 bit
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код всегда выполняется для Em32 если архив полный
					{
						// выбираем все
						query += String.Format(" SELECT datetime_id, event_datetime, an_u1_a_u1_b, an_u1_b_u1_c, an_u1_c_u1_a, an_u1_a_i1_a, an_u1_b_i1_b, an_u1_c_i1_c, an_u_1_i_1, an_u_2_i_2, an_u_0_i_0 FROM period_avg_params_1_4 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}

					#endregion
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_1_4", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_1_4");

				mainWnd_.CMs_[(int)AvgPages.ANGLES] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.ANGLES].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.ANGLES].List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperAngles::load_not_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}
	}

	internal class AVGDataGridWrapperHarmUph : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperHarmUph(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_perc = "_" + rm.GetString("column_header_units_percent");

				string unit_v = "_";
				unit_v += (settings_.VoltageRatio == 1) ?
					 rm.GetString("column_header_units_v") :
					 rm.GetString("column_header_units_kv");

				string value_for_order =
					settings_.CurrentLanguage.Equals("ru") ? "Кп_{0}({1})" : "C ord_{0}({1})";

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_u_phase_harmonics";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				DataGridColumnHeaderFormula cs_s_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Σgpgr>1_A" + unit_v);
				cs_s_a.HeaderText = "";
				cs_s_a.MappingName = "summ_for_order_more_1_a";
				cs_s_a.Format = DataColumnsFormat.FloatFormat;
				cs_s_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_s_a);

				for (int iCol = 1; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format("Gsg{0}_A{1}", iCol, unit_v));
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("order_value_{0}_a", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				DataGridColumnHeaderFormula cs_sc_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "THDS_A_, %");
				cs_sc_a.HeaderText = "";
				cs_sc_a.MappingName = "summ_coeff_a";
				cs_sc_a.Format = DataColumnsFormat.FloatFormat;
				cs_sc_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_sc_a);

				for (int iCol = 2; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format(value_for_order, "A", iCol) + unit_perc);
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("order_coeff_{0}_a", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_s_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Σgpgr>1_B" + unit_v);
					cs_s_b.HeaderText = "";
					cs_s_b.MappingName = "summ_for_order_more_1_b";
					cs_s_b.Format = DataColumnsFormat.FloatFormat;
					cs_s_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_b);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Gsg{0}_B{1}", iCol, unit_v));
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("order_value_{0}_b", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_sc_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "THDS_B_, %");
					cs_sc_b.HeaderText = "";
					cs_sc_b.MappingName = "summ_coeff_b";
					cs_sc_b.Format = DataColumnsFormat.FloatFormat;
					cs_sc_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_b);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format(value_for_order, "B", iCol) + unit_perc);
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("order_coeff_{0}_b", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_s_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Σgpgr>1_C" + unit_v);
					cs_s_c.HeaderText = "";
					cs_s_c.MappingName = "summ_for_order_more_1_c";
					cs_s_c.Format = DataColumnsFormat.FloatFormat;
					cs_s_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_c);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Gsg{0}_C{1}", iCol, unit_v));
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("order_value_{0}_c", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}

					DataGridColumnHeaderFormula cs_sc_c =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "THDS_C_, %");
					cs_sc_c.HeaderText = "";
					cs_sc_c.MappingName = "summ_coeff_c";
					cs_sc_c.Format = DataColumnsFormat.FloatFormat;
					cs_sc_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_c);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format(value_for_order, "C", iCol) + unit_perc);
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("order_coeff_{0}_c", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				//for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				//{
				//    (ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_v);
				//}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUph::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				// если Эм33Т или Эм31К и схема подключения 1ф2пр или 3ф4пр, то фазных нет
				if ((devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T ||
					devType_ == EmDeviceType.EM33T1) &&
					(connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc))
				{
					return;
				}

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_perc = rm.GetString("column_header_units_percent");

				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "period_avg_params_5";
				ts.AllowSorting = false;


				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				if (devType_ == EmDeviceType.EM31K || devType_ == EmDeviceType.EM33T ||
					devType_ == EmDeviceType.EM33T1)
				{
					// U1_A
					string cap_u1_a_ab = "U_A(1)_";
					DataGridColumnHeaderFormula cs_u1_a_ab =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap_u1_a_ab + unit_v);
					cs_u1_a_ab.HeaderText = "";
					cs_u1_a_ab.MappingName = "u1_a_ab";
					cs_u1_a_ab.Format = DataColumnsFormat.FloatFormat;
					cs_u1_a_ab.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_u1_a_ab);

					// K_UA
					for (int i = 2; i < 41; i++)
					{
						string cap = "K_U A(" + i.ToString() + ")_";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_ua_ab_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}
					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// U1_B
						string cap_u1_b_bc = "U_B(1)_";
						DataGridColumnHeaderFormula cs_u1_b_bc =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
															cap_u1_b_bc + unit_v);
						cs_u1_b_bc.HeaderText = "";
						cs_u1_b_bc.MappingName = "u1_b_bc";
						cs_u1_b_bc.Format = DataColumnsFormat.FloatFormat;
						cs_u1_b_bc.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_u1_b_bc);

						// K_UB
						for (int i = 2; i < 41; i++)
						{
							string cap = "K_U B(" + i.ToString() + ")_";
							DataGridColumnHeaderFormula cs_k =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
																cap + unit_perc);
							cs_k.HeaderText = "";
							cs_k.MappingName = "k_ub_bc_" + i.ToString();
							cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
							cs_k.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_k);
						}

						// U1_C
						string cap_u1_c_ca = "U_C(1)_";
						DataGridColumnHeaderFormula cs_u1_c_ca =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
															cap_u1_c_ca + unit_v);
						cs_u1_c_ca.HeaderText = "";
						cs_u1_c_ca.MappingName = "u1_c_ca";
						cs_u1_c_ca.Format = DataColumnsFormat.FloatFormat;
						cs_u1_c_ca.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_u1_c_ca);

						// K_UC
						for (int i = 2; i < 41; i++)
						{
							string cap = "K_U C(" + i.ToString() + ")_";
							DataGridColumnHeaderFormula cs_k =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
																cap + unit_perc);
							cs_k.HeaderText = "";
							cs_k.MappingName = "k_uc_ca_" + i.ToString();
							cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
							cs_k.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_k);
						}
					}
				}
				else if (devType_ == EmDeviceType.EM32 || devType_ == EmDeviceType.ETPQP)
				{
					// U1_A
					string cap_u1_a_ab = "U_A(1)_";
					DataGridColumnHeaderFormula cs_u1_a_ab =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap_u1_a_ab + unit_v);
					cs_u1_a_ab.HeaderText = "";
					cs_u1_a_ab.MappingName = "u1_a";
					cs_u1_a_ab.Format = DataColumnsFormat.FloatFormat;
					cs_u1_a_ab.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_u1_a_ab);

					// K_UA
					for (int i = 2; i < 41; i++)
					{
						string cap = "K_U A(" + i.ToString() + ")_";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_ua_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}
					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// U1_B
						string cap_u1_b_bc = "U_B(1)_";
						DataGridColumnHeaderFormula cs_u1_b_bc =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
															cap_u1_b_bc + unit_v);
						cs_u1_b_bc.HeaderText = "";
						cs_u1_b_bc.MappingName = "u1_b";
						cs_u1_b_bc.Format = DataColumnsFormat.FloatFormat;
						cs_u1_b_bc.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_u1_b_bc);

						// K_UB
						for (int i = 2; i < 41; i++)
						{
							string cap = "K_U B(" + i.ToString() + ")_";
							DataGridColumnHeaderFormula cs_k =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
																cap + unit_perc);
							cs_k.HeaderText = "";
							cs_k.MappingName = "k_ub_" + i.ToString();
							cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
							cs_k.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_k);
						}

						// U1_C
						string cap_u1_c_ca = "U_C(1)_";
						DataGridColumnHeaderFormula cs_u1_c_ca =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
															cap_u1_c_ca + unit_v);
						cs_u1_c_ca.HeaderText = "";
						cs_u1_c_ca.MappingName = "u1_c";
						cs_u1_c_ca.Format = DataColumnsFormat.FloatFormat;
						cs_u1_c_ca.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_u1_c_ca);

						// K_UC
						for (int i = 2; i < 41; i++)
						{
							string cap = "K_U C(" + i.ToString() + ")_";
							DataGridColumnHeaderFormula cs_k =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
																cap + unit_perc);
							cs_k.HeaderText = "";
							cs_k.MappingName = "k_uc_" + i.ToString();
							cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
							cs_k.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_k);
						}
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUph::init(): ");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string query = string.Empty;

				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				#region Query

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					query = String.Format(@"SELECT dt_start, cnt_windows_not_locked,
case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked,
p.datetime_id, p.record_id, summ_for_order_more_1_a, 
order_value_1_a, order_value_2_a, order_value_3_a, order_value_4_a, order_value_5_a, 
order_value_6_a, order_value_7_a, order_value_8_a, order_value_9_a, 
order_value_10_a, order_value_11_a, order_value_12_a, order_value_13_a, 
order_value_14_a, order_value_15_a, order_value_16_a, order_value_17_a, 
order_value_18_a, order_value_19_a, order_value_20_a, order_value_21_a, 
order_value_22_a, order_value_23_a, order_value_24_a, order_value_25_a, 
order_value_26_a, order_value_27_a, order_value_28_a, order_value_29_a, 
order_value_30_a, order_value_31_a, order_value_32_a, order_value_33_a, 
order_value_34_a, order_value_35_a, order_value_36_a, order_value_37_a, 
order_value_38_a, order_value_39_a, order_value_40_a, order_value_41_a, 
order_value_42_a, order_value_43_a, order_value_44_a, order_value_45_a, 
order_value_46_a, order_value_47_a, order_value_48_a, order_value_49_a, 
order_value_50_a, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_a as text) end as summ_coeff_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_a as text) end as order_coeff_2_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_a as text) end as order_coeff_3_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_a as text) end as order_coeff_4_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_a as text) end as order_coeff_5_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_a as text) end as order_coeff_6_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_a as text) end as order_coeff_7_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_a as text) end as order_coeff_8_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_a as text) end as order_coeff_9_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_a as text) end as order_coeff_10_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_a as text) end as order_coeff_11_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_a as text) end as order_coeff_12_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_a as text) end as order_coeff_13_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_a as text) end as order_coeff_14_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_a as text) end as order_coeff_15_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_a as text) end as order_coeff_16_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_a as text) end as order_coeff_17_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_a as text) end as order_coeff_18_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_a as text) end as order_coeff_19_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_a as text) end as order_coeff_20_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_a as text) end as order_coeff_21_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_a as text) end as order_coeff_22_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_a as text) end as order_coeff_23_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_a as text) end as order_coeff_24_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_a as text) end as order_coeff_25_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_a as text) end as order_coeff_26_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_a as text) end as order_coeff_27_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_a as text) end as order_coeff_28_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_a as text) end as order_coeff_29_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_a as text) end as order_coeff_30_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_a as text) end as order_coeff_31_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_a as text) end as order_coeff_32_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_a as text) end as order_coeff_33_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_a as text) end as order_coeff_34_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_a as text) end as order_coeff_35_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_a as text) end as order_coeff_36_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_a as text) end as order_coeff_37_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_a as text) end as order_coeff_38_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_a as text) end as order_coeff_39_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_a as text) end as order_coeff_40_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_a as text) end as order_coeff_41_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_a as text) end as order_coeff_42_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_a as text) end as order_coeff_43_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_a as text) end as order_coeff_44_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_a as text) end as order_coeff_45_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_a as text) end as order_coeff_46_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_a as text) end as order_coeff_47_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_a as text) end as order_coeff_48_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_a as text) end as order_coeff_49_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_a as text) end as order_coeff_50_a,
summ_for_order_more_1_b, 
       order_value_1_b, order_value_2_b, order_value_3_b, order_value_4_b, 
       order_value_5_b, order_value_6_b, order_value_7_b, order_value_8_b, 
       order_value_9_b, order_value_10_b, order_value_11_b, order_value_12_b, 
       order_value_13_b, order_value_14_b, order_value_15_b, order_value_16_b, 
       order_value_17_b, order_value_18_b, order_value_19_b, order_value_20_b, 
       order_value_21_b, order_value_22_b, order_value_23_b, order_value_24_b, 
       order_value_25_b, order_value_26_b, order_value_27_b, order_value_28_b, 
       order_value_29_b, order_value_30_b, order_value_31_b, order_value_32_b, 
       order_value_33_b, order_value_34_b, order_value_35_b, order_value_36_b, 
       order_value_37_b, order_value_38_b, order_value_39_b, order_value_40_b, 
       order_value_41_b, order_value_42_b, order_value_43_b, order_value_44_b, 
       order_value_45_b, order_value_46_b, order_value_47_b, order_value_48_b, 
       order_value_49_b, order_value_50_b, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_b as text) end as summ_coeff_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_b as text) end as order_coeff_2_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_b as text) end as order_coeff_3_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_b as text) end as order_coeff_4_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_b as text) end as order_coeff_5_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_b as text) end as order_coeff_6_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_b as text) end as order_coeff_7_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_b as text) end as order_coeff_8_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_b as text) end as order_coeff_9_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_b as text) end as order_coeff_10_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_b as text) end as order_coeff_11_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_b as text) end as order_coeff_12_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_b as text) end as order_coeff_13_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_b as text) end as order_coeff_14_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_b as text) end as order_coeff_15_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_b as text) end as order_coeff_16_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_b as text) end as order_coeff_17_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_b as text) end as order_coeff_18_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_b as text) end as order_coeff_19_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_b as text) end as order_coeff_20_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_b as text) end as order_coeff_21_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_b as text) end as order_coeff_22_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_b as text) end as order_coeff_23_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_b as text) end as order_coeff_24_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_b as text) end as order_coeff_25_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_b as text) end as order_coeff_26_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_b as text) end as order_coeff_27_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_b as text) end as order_coeff_28_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_b as text) end as order_coeff_29_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_b as text) end as order_coeff_30_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_b as text) end as order_coeff_31_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_b as text) end as order_coeff_32_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_b as text) end as order_coeff_33_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_b as text) end as order_coeff_34_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_b as text) end as order_coeff_35_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_b as text) end as order_coeff_36_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_b as text) end as order_coeff_37_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_b as text) end as order_coeff_38_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_b as text) end as order_coeff_39_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_b as text) end as order_coeff_40_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_b as text) end as order_coeff_41_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_b as text) end as order_coeff_42_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_b as text) end as order_coeff_43_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_b as text) end as order_coeff_44_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_b as text) end as order_coeff_45_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_b as text) end as order_coeff_46_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_b as text) end as order_coeff_47_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_b as text) end as order_coeff_48_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_b as text) end as order_coeff_49_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_b as text) end as order_coeff_50_b,
       summ_for_order_more_1_c, order_value_1_c, order_value_2_c, order_value_3_c, 
       order_value_4_c, order_value_5_c, order_value_6_c, order_value_7_c, 
       order_value_8_c, order_value_9_c, order_value_10_c, order_value_11_c, 
       order_value_12_c, order_value_13_c, order_value_14_c, order_value_15_c, 
       order_value_16_c, order_value_17_c, order_value_18_c, order_value_19_c, 
       order_value_20_c, order_value_21_c, order_value_22_c, order_value_23_c, 
       order_value_24_c, order_value_25_c, order_value_26_c, order_value_27_c, 
       order_value_28_c, order_value_29_c, order_value_30_c, order_value_31_c, 
       order_value_32_c, order_value_33_c, order_value_34_c, order_value_35_c, 
       order_value_36_c, order_value_37_c, order_value_38_c, order_value_39_c, 
       order_value_40_c, order_value_41_c, order_value_42_c, order_value_43_c, 
       order_value_44_c, order_value_45_c, order_value_46_c, order_value_47_c, 
       order_value_48_c, order_value_49_c, order_value_50_c, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_c as text) end as summ_coeff_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_c as text) end as order_coeff_2_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_c as text) end as order_coeff_3_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_c as text) end as order_coeff_4_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_c as text) end as order_coeff_5_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_c as text) end as order_coeff_6_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_c as text) end as order_coeff_7_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_c as text) end as order_coeff_8_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_c as text) end as order_coeff_9_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_c as text) end as order_coeff_10_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_c as text) end as order_coeff_11_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_c as text) end as order_coeff_12_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_c as text) end as order_coeff_13_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_c as text) end as order_coeff_14_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_c as text) end as order_coeff_15_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_c as text) end as order_coeff_16_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_c as text) end as order_coeff_17_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_c as text) end as order_coeff_18_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_c as text) end as order_coeff_19_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_c as text) end as order_coeff_20_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_c as text) end as order_coeff_21_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_c as text) end as order_coeff_22_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_c as text) end as order_coeff_23_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_c as text) end as order_coeff_24_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_c as text) end as order_coeff_25_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_c as text) end as order_coeff_26_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_c as text) end as order_coeff_27_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_c as text) end as order_coeff_28_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_c as text) end as order_coeff_29_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_c as text) end as order_coeff_30_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_c as text) end as order_coeff_31_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_c as text) end as order_coeff_32_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_c as text) end as order_coeff_33_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_c as text) end as order_coeff_34_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_c as text) end as order_coeff_35_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_c as text) end as order_coeff_36_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_c as text) end as order_coeff_37_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_c as text) end as order_coeff_38_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_c as text) end as order_coeff_39_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_c as text) end as order_coeff_40_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_c as text) end as order_coeff_41_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_c as text) end as order_coeff_42_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_c as text) end as order_coeff_43_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_c as text) end as order_coeff_44_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_c as text) end as order_coeff_45_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_c as text) end as order_coeff_46_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_c as text) end as order_coeff_47_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_c as text) end as order_coeff_48_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_c as text) end as order_coeff_49_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_c as text) end as order_coeff_50_c
					FROM avg_u_phase_harmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}
				else if (connectScheme_ == ConnectScheme.Ph1W2)
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, cnt_windows_not_locked,
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked,
       summ_for_order_more_1_a, order_value_1_a, order_value_2_a, order_value_3_a, order_value_4_a, order_value_5_a, 
       order_value_6_a, order_value_7_a, order_value_8_a, order_value_9_a, 
       order_value_10_a, order_value_11_a, order_value_12_a, order_value_13_a, 
       order_value_14_a, order_value_15_a, order_value_16_a, order_value_17_a, 
       order_value_18_a, order_value_19_a, order_value_20_a, order_value_21_a, 
       order_value_22_a, order_value_23_a, order_value_24_a, order_value_25_a, 
       order_value_26_a, order_value_27_a, order_value_28_a, order_value_29_a, 
       order_value_30_a, order_value_31_a, order_value_32_a, order_value_33_a, 
       order_value_34_a, order_value_35_a, order_value_36_a, order_value_37_a, 
       order_value_38_a, order_value_39_a, order_value_40_a, order_value_41_a, 
       order_value_42_a, order_value_43_a, order_value_44_a, order_value_45_a, 
       order_value_46_a, order_value_47_a, order_value_48_a, order_value_49_a, 
       order_value_50_a, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_a as text) end as summ_coeff_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_a as text) end as order_coeff_2_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_a as text) end as order_coeff_3_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_a as text) end as order_coeff_4_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_a as text) end as order_coeff_5_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_a as text) end as order_coeff_6_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_a as text) end as order_coeff_7_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_a as text) end as order_coeff_8_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_a as text) end as order_coeff_9_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_a as text) end as order_coeff_10_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_a as text) end as order_coeff_11_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_a as text) end as order_coeff_12_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_a as text) end as order_coeff_13_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_a as text) end as order_coeff_14_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_a as text) end as order_coeff_15_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_a as text) end as order_coeff_16_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_a as text) end as order_coeff_17_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_a as text) end as order_coeff_18_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_a as text) end as order_coeff_19_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_a as text) end as order_coeff_20_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_a as text) end as order_coeff_21_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_a as text) end as order_coeff_22_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_a as text) end as order_coeff_23_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_a as text) end as order_coeff_24_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_a as text) end as order_coeff_25_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_a as text) end as order_coeff_26_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_a as text) end as order_coeff_27_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_a as text) end as order_coeff_28_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_a as text) end as order_coeff_29_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_a as text) end as order_coeff_30_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_a as text) end as order_coeff_31_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_a as text) end as order_coeff_32_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_a as text) end as order_coeff_33_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_a as text) end as order_coeff_34_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_a as text) end as order_coeff_35_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_a as text) end as order_coeff_36_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_a as text) end as order_coeff_37_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_a as text) end as order_coeff_38_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_a as text) end as order_coeff_39_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_a as text) end as order_coeff_40_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_a as text) end as order_coeff_41_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_a as text) end as order_coeff_42_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_a as text) end as order_coeff_43_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_a as text) end as order_coeff_44_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_a as text) end as order_coeff_45_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_a as text) end as order_coeff_46_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_a as text) end as order_coeff_47_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_a as text) end as order_coeff_48_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_a as text) end as order_coeff_49_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_a as text) end as order_coeff_50_a
  FROM avg_u_phase_harmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}

				#endregion

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_u_phase_harmonics", ref ds);

				// это была попытка более правильной обработки суммарного коэфф-та, но этот цикл слишком
				// медленно работает, поэтому пока ограничимся выставлением прочерка в SQL-запросе
				//try
				//{
				//    for (int iRow = 0; iRow < ds.Tables[0].Rows.Count; ++iRow)
				//    {
				//        object o_cnt_windows_not_locked = ds.Tables[0].Rows[iRow]["cnt_windows_not_locked"];
				//        if (o_cnt_windows_not_locked is DBNull) continue;
				//        int cnt_windows_not_locked = (int)o_cnt_windows_not_locked;
				//        if (cnt_windows_not_locked > 0) ds.Tables[0].Rows[iRow]["summ_coeff_a"] = "-";

				//        object tmpObj = ds.Tables[0].Rows[iRow]["summ_coeff_a"];
				//        if (tmpObj is DBNull) continue;
				//        float tmpFloat = Conversions.object_2_float(tmpObj);
				//        if (tmpFloat > 100) ds.Tables[0].Rows[iRow]["summ_coeff_a"] = ">= " + tmpObj.ToString();
				//    }
				//}
				//catch
				//{
				//    EmService.WriteToLogFailed("Excetion while handling order_coeff_x_y in ph harmonics!!!");
				//}

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_u_phase_harmonics");

				mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS].List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUph::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_5");

				// в базе Эм33Т для фазных и линейных гармоник используются одни и те же поля:
				// для 3ф4пр и 1ф2пр туда кладутся фазные, а для 3ф3пр туда кладутся линейные.
				// в базе ЭтПКЭ и Эм32 фазные и линейные кладутся каждые в свои поля, поэтому устройства
				// надо обрабатывать по-разному

				string query = "";
				if (devType_ == EmDeviceType.EM31K || devType_ == EmDeviceType.EM33T ||
					devType_ == EmDeviceType.EM33T1)
				{
					if (connectScheme_ != ConnectScheme.Ph3W3 &&
						connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						query = String.Format("SELECT * FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else query = "";	// для 3ф3пр нет фазных гармоник
				}
				else
				{
					query = "";
					if (iMask != -1)
					{
						query = "SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								if ((iMask & 0x000000001) != 0)							// 1 bit
								{
									query += "u1_a,";
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_ua_" + i.ToString() + ",";
									}
								}
								if ((iMask & 0x000000002) != 0)							// 2 bit
								{
									query += "u1_b,";
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_ub_" + i.ToString() + ",";
									}
								}
								if ((iMask & 0x000000004) != 0)							// 3 bit
								{
									query += "u1_c,";
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_uc_" + i.ToString() + ",";
									}
								}
								break;
							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								break;
							case ConnectScheme.Ph1W2:
								if ((iMask & 0x000000001) != 0)							// 1 bit
								{
									query += "u1_a,";
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_ua_" + i.ToString() + ",";
									}
								}
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код выполняется если архив полный
					{
						query = String.Format("SELECT * FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_5", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rU = settings_.VoltageRatio;
				float ctr = 1, vtr = 1;
				string commandText = string.Empty;

				if (curWithTR)
				{
					if (devType_ == EmDeviceType.EM33T ||
						devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.EM32)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.ETPQP)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);

					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (devType_ != EmDeviceType.ETPQP) 
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						if (iType == 1) vtr = fValue1 / fValue2;
						else if (iType == 2) ctr = fValue1 / fValue2;
					}
				}
				rU *= vtr;

				#endregion

				#region Changing table values

				if (ds.Tables.Count > 0)
				{
					for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
					{
						// voltages
						for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
						{
							if (ds.Tables[0].Columns[i].Caption.Contains("u1_"))
							{
								if (!(ds.Tables[0].Rows[r][i] is DBNull))
									ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rU;
							}
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_5");

				if (ds.Tables.Count > 0)
				{
					mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS] = 
						(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
					mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS].PositionChanged += 
						new EventHandler(mainWnd_.currencyManager_PositionChanged);
					DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS].List;
					dataView.AllowNew = false;
					dataView.AllowDelete = false;
					dataView.AllowEdit = false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUph::load_not_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.U_PH_HARMONICS] = null;
		}
	}

	internal class AVGDataGridWrapperHarmUlin : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperHarmUlin(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_perc = "_" + rm.GetString("column_header_units_percent");

				string unit_v = "_";
				unit_v += (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");

				string value_for_order =
					settings_.CurrentLanguage.Equals("ru") ? "Кп_{0}({1})" : "C ord_{0}({1})";

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_u_lin_harmonics";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				DataGridColumnHeaderFormula cs_s_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Σgpgr>1_AB" + unit_v);
				cs_s_a.HeaderText = "";
				cs_s_a.MappingName = "summ_for_order_more_1_ab";
				cs_s_a.Format = DataColumnsFormat.FloatFormat;
				cs_s_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_s_a);

				for (int iCol = 1; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format("Gsg{0}_AB{1}", iCol, unit_v));
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("order_value_{0}_ab", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				DataGridColumnHeaderFormula cs_sc_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "THDS_AB_, %");
				cs_sc_a.HeaderText = "";
				cs_sc_a.MappingName = "summ_coeff_ab";
				cs_sc_a.Format = DataColumnsFormat.FloatFormat;
				cs_sc_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_sc_a);

				for (int iCol = 2; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format(value_for_order, "AB", iCol) + unit_perc);
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("order_coeff_{0}_ab", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_s_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Σgpgr>1_BC" + unit_v);
					cs_s_b.HeaderText = "";
					cs_s_b.MappingName = "summ_for_order_more_1_bc";
					cs_s_b.Format = DataColumnsFormat.FloatFormat;
					cs_s_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_b);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Gsg{0}_BC{1}", iCol, unit_v));
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("order_value_{0}_bc", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_sc_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "THDS_BC_, %");
					cs_sc_b.HeaderText = "";
					cs_sc_b.MappingName = "summ_coeff_bc";
					cs_sc_b.Format = DataColumnsFormat.FloatFormat;
					cs_sc_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_b);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format(value_for_order, "BC", iCol) + unit_perc);
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("order_coeff_{0}_bc", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_s_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Σgpgr>1_CA" + unit_v);
					cs_s_c.HeaderText = "";
					cs_s_c.MappingName = "summ_for_order_more_1_ca";
					cs_s_c.Format = DataColumnsFormat.FloatFormat;
					cs_s_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_c);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Gsg{0}_CA{1}", iCol, unit_v));
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("order_value_{0}_ca", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}

					DataGridColumnHeaderFormula cs_sc_c =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "THDS_CA_, %");
					cs_sc_c.HeaderText = "";
					cs_sc_c.MappingName = "summ_coeff_ca";
					cs_sc_c.Format = DataColumnsFormat.FloatFormat;
					cs_sc_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_c);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format(value_for_order, "CA", iCol) + unit_perc);
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("order_coeff_{0}_ca", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				//for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				//{
				//    (ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_v);
				//}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUlin::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				// если Эм33Т или Эм31К и схема подключения 1ф2пр или 3ф4пр, то линейных нет
				if ((devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T ||
					devType_ == EmDeviceType.EM33T1) &&
					(connectScheme_ != ConnectScheme.Ph3W3 &&
					connectScheme_ != ConnectScheme.Ph3W3_B_calc))
				{
					return;
				}

				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
															this.GetType().Assembly);

				string unit_perc = rm.GetString("column_header_units_percent");

				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "period_avg_params_5";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// если Эм33Т или Эм31К, то в БД нет отдельных полей для линейных
				if ((devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T ||
					devType_ == EmDeviceType.EM33T1) &&
					(connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc))
				{
					// U1_A
					string cap_u1_a_ab = "U_AB(1)_";
					DataGridColumnHeaderFormula cs_u1_a_ab =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap_u1_a_ab + unit_v);
					cs_u1_a_ab.HeaderText = "";
					cs_u1_a_ab.MappingName = "u1_a_ab";
					cs_u1_a_ab.Format = DataColumnsFormat.FloatFormat;
					cs_u1_a_ab.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_u1_a_ab);

					// K_UA
					for (int i = 2; i < 41; i++)
					{
						string cap = "K_U AB(" + i.ToString() + ")_";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_ua_ab_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}
					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						// U1_B
						string cap_u1_b_bc = "U_BC(1)_";
						DataGridColumnHeaderFormula cs_u1_b_bc =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
															cap_u1_b_bc + unit_v);
						cs_u1_b_bc.HeaderText = "";
						cs_u1_b_bc.MappingName = "u1_b_bc";
						cs_u1_b_bc.Format = DataColumnsFormat.FloatFormat;
						cs_u1_b_bc.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_u1_b_bc);

						// K_UB
						for (int i = 2; i < 41; i++)
						{
							string cap = "K_U BC(" + i.ToString() + ")_";
							DataGridColumnHeaderFormula cs_k =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
																cap + unit_perc);
							cs_k.HeaderText = "";
							cs_k.MappingName = "k_ub_bc_" + i.ToString();
							cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
							cs_k.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_k);
						}

						// U1_C
						string cap_u1_c_ca = "U_CA(1)_";
						DataGridColumnHeaderFormula cs_u1_c_ca =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
															cap_u1_c_ca + unit_v);
						cs_u1_c_ca.HeaderText = "";
						cs_u1_c_ca.MappingName = "u1_c_ca";
						cs_u1_c_ca.Format = DataColumnsFormat.FloatFormat;
						cs_u1_c_ca.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_u1_c_ca);

						// K_UC
						for (int i = 2; i < 41; i++)
						{
							string cap = "K_U CA(" + i.ToString() + ")_";
							DataGridColumnHeaderFormula cs_k =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
																cap + unit_perc);
							cs_k.HeaderText = "";
							cs_k.MappingName = "k_uc_ca_" + i.ToString();
							cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
							cs_k.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_k);
						}
					}
				}
				else if (devType_ == EmDeviceType.EM32 || devType_ == EmDeviceType.ETPQP)
				{
					// U1_AB
					string cap_u1_a_ab = "U_AB(1)_";
					DataGridColumnHeaderFormula cs_u1_a_ab =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap_u1_a_ab + unit_v);
					cs_u1_a_ab.HeaderText = "";
					cs_u1_a_ab.MappingName = "u1_ab";
					cs_u1_a_ab.Format = DataColumnsFormat.FloatFormat;
					cs_u1_a_ab.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_u1_a_ab);

					// K_UAB
					for (int i = 2; i < 41; i++)
					{
						string cap = "K_U AB(" + i.ToString() + ")_";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_uab_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}

					// U1_BC
					string cap_u1_b_bc = "U_BC(1)_";
					DataGridColumnHeaderFormula cs_u1_b_bc =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, cap_u1_b_bc + unit_v);
					cs_u1_b_bc.HeaderText = "";
					cs_u1_b_bc.MappingName = "u1_bc";
					cs_u1_b_bc.Format = DataColumnsFormat.FloatFormat;
					cs_u1_b_bc.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_u1_b_bc);

					// K_UBC
					for (int i = 2; i < 41; i++)
					{
						string cap = "K_U BC(" + i.ToString() + ")_";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, cap + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_ubc_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}

					// U1_CA
					string cap_u1_c_ca = "U_CA(1)_";
					DataGridColumnHeaderFormula cs_u1_c_ca =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, cap_u1_c_ca + unit_v);
					cs_u1_c_ca.HeaderText = "";
					cs_u1_c_ca.MappingName = "u1_ca";
					cs_u1_c_ca.Format = DataColumnsFormat.FloatFormat;
					cs_u1_c_ca.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_u1_c_ca);

					// K_UCA
					for (int i = 2; i < 41; i++)
					{
						string cap = "K_U CA(" + i.ToString() + ")_";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, cap + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_uca_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUlin::init(): ");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string query = string.Empty;

				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				#region Query

				query = String.Format(@"SELECT dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
				p.datetime_id, p.record_id, summ_for_order_more_1_ab, order_value_1_ab, 
       order_value_2_ab, order_value_3_ab, order_value_4_ab, order_value_5_ab, 
       order_value_6_ab, order_value_7_ab, order_value_8_ab, order_value_9_ab, 
       order_value_10_ab, order_value_11_ab, order_value_12_ab, order_value_13_ab, 
       order_value_14_ab, order_value_15_ab, order_value_16_ab, order_value_17_ab, 
       order_value_18_ab, order_value_19_ab, order_value_20_ab, order_value_21_ab, 
       order_value_22_ab, order_value_23_ab, order_value_24_ab, order_value_25_ab, 
       order_value_26_ab, order_value_27_ab, order_value_28_ab, order_value_29_ab, 
       order_value_30_ab, order_value_31_ab, order_value_32_ab, order_value_33_ab, 
       order_value_34_ab, order_value_35_ab, order_value_36_ab, order_value_37_ab, 
       order_value_38_ab, order_value_39_ab, order_value_40_ab, order_value_41_ab, 
       order_value_42_ab, order_value_43_ab, order_value_44_ab, order_value_45_ab, 
       order_value_46_ab, order_value_47_ab, order_value_48_ab, order_value_49_ab, 
       order_value_50_ab, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_ab as text) end as summ_coeff_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_ab as text) end as order_coeff_2_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_ab as text) end as order_coeff_3_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_ab as text) end as order_coeff_4_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_ab as text) end as order_coeff_5_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_ab as text) end as order_coeff_6_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_ab as text) end as order_coeff_7_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_ab as text) end as order_coeff_8_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_ab as text) end as order_coeff_9_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_ab as text) end as order_coeff_10_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_ab as text) end as order_coeff_11_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_ab as text) end as order_coeff_12_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_ab as text) end as order_coeff_13_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_ab as text) end as order_coeff_14_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_ab as text) end as order_coeff_15_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_ab as text) end as order_coeff_16_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_ab as text) end as order_coeff_17_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_ab as text) end as order_coeff_18_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_ab as text) end as order_coeff_19_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_ab as text) end as order_coeff_20_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_ab as text) end as order_coeff_21_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_ab as text) end as order_coeff_22_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_ab as text) end as order_coeff_23_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_ab as text) end as order_coeff_24_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_ab as text) end as order_coeff_25_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_ab as text) end as order_coeff_26_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_ab as text) end as order_coeff_27_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_ab as text) end as order_coeff_28_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_ab as text) end as order_coeff_29_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_ab as text) end as order_coeff_30_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_ab as text) end as order_coeff_31_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_ab as text) end as order_coeff_32_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_ab as text) end as order_coeff_33_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_ab as text) end as order_coeff_34_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_ab as text) end as order_coeff_35_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_ab as text) end as order_coeff_36_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_ab as text) end as order_coeff_37_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_ab as text) end as order_coeff_38_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_ab as text) end as order_coeff_39_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_ab as text) end as order_coeff_40_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_ab as text) end as order_coeff_41_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_ab as text) end as order_coeff_42_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_ab as text) end as order_coeff_43_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_ab as text) end as order_coeff_44_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_ab as text) end as order_coeff_45_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_ab as text) end as order_coeff_46_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_ab as text) end as order_coeff_47_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_ab as text) end as order_coeff_48_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_ab as text) end as order_coeff_49_ab,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_ab as text) end as order_coeff_50_ab, 
summ_for_order_more_1_bc, 
       order_value_1_bc, order_value_2_bc, order_value_3_bc, order_value_4_bc, 
       order_value_5_bc, order_value_6_bc, order_value_7_bc, order_value_8_bc, 
       order_value_9_bc, order_value_10_bc, order_value_11_bc, order_value_12_bc, 
       order_value_13_bc, order_value_14_bc, order_value_15_bc, order_value_16_bc, 
       order_value_17_bc, order_value_18_bc, order_value_19_bc, order_value_20_bc, 
       order_value_21_bc, order_value_22_bc, order_value_23_bc, order_value_24_bc, 
       order_value_25_bc, order_value_26_bc, order_value_27_bc, order_value_28_bc, 
       order_value_29_bc, order_value_30_bc, order_value_31_bc, order_value_32_bc, 
       order_value_33_bc, order_value_34_bc, order_value_35_bc, order_value_36_bc, 
       order_value_37_bc, order_value_38_bc, order_value_39_bc, order_value_40_bc, 
       order_value_41_bc, order_value_42_bc, order_value_43_bc, order_value_44_bc, 
       order_value_45_bc, order_value_46_bc, order_value_47_bc, order_value_48_bc, 
       order_value_49_bc, order_value_50_bc, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_bc as text) end as summ_coeff_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_bc as text) end as order_coeff_2_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_bc as text) end as order_coeff_3_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_bc as text) end as order_coeff_4_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_bc as text) end as order_coeff_5_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_bc as text) end as order_coeff_6_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_bc as text) end as order_coeff_7_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_bc as text) end as order_coeff_8_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_bc as text) end as order_coeff_9_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_bc as text) end as order_coeff_10_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_bc as text) end as order_coeff_11_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_bc as text) end as order_coeff_12_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_bc as text) end as order_coeff_13_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_bc as text) end as order_coeff_14_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_bc as text) end as order_coeff_15_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_bc as text) end as order_coeff_16_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_bc as text) end as order_coeff_17_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_bc as text) end as order_coeff_18_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_bc as text) end as order_coeff_19_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_bc as text) end as order_coeff_20_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_bc as text) end as order_coeff_21_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_bc as text) end as order_coeff_22_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_bc as text) end as order_coeff_23_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_bc as text) end as order_coeff_24_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_bc as text) end as order_coeff_25_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_bc as text) end as order_coeff_26_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_bc as text) end as order_coeff_27_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_bc as text) end as order_coeff_28_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_bc as text) end as order_coeff_29_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_bc as text) end as order_coeff_30_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_bc as text) end as order_coeff_31_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_bc as text) end as order_coeff_32_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_bc as text) end as order_coeff_33_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_bc as text) end as order_coeff_34_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_bc as text) end as order_coeff_35_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_bc as text) end as order_coeff_36_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_bc as text) end as order_coeff_37_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_bc as text) end as order_coeff_38_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_bc as text) end as order_coeff_39_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_bc as text) end as order_coeff_40_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_bc as text) end as order_coeff_41_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_bc as text) end as order_coeff_42_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_bc as text) end as order_coeff_43_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_bc as text) end as order_coeff_44_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_bc as text) end as order_coeff_45_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_bc as text) end as order_coeff_46_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_bc as text) end as order_coeff_47_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_bc as text) end as order_coeff_48_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_bc as text) end as order_coeff_49_bc,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_bc as text) end as order_coeff_50_bc,
summ_for_order_more_1_ca, order_value_1_ca, order_value_2_ca, 
       order_value_3_ca, order_value_4_ca, order_value_5_ca, order_value_6_ca, 
       order_value_7_ca, order_value_8_ca, order_value_9_ca, order_value_10_ca, 
       order_value_11_ca, order_value_12_ca, order_value_13_ca, order_value_14_ca, 
       order_value_15_ca, order_value_16_ca, order_value_17_ca, order_value_18_ca, 
       order_value_19_ca, order_value_20_ca, order_value_21_ca, order_value_22_ca, 
       order_value_23_ca, order_value_24_ca, order_value_25_ca, order_value_26_ca, 
       order_value_27_ca, order_value_28_ca, order_value_29_ca, order_value_30_ca, 
       order_value_31_ca, order_value_32_ca, order_value_33_ca, order_value_34_ca, 
       order_value_35_ca, order_value_36_ca, order_value_37_ca, order_value_38_ca, 
       order_value_39_ca, order_value_40_ca, order_value_41_ca, order_value_42_ca, 
       order_value_43_ca, order_value_44_ca, order_value_45_ca, order_value_46_ca, 
       order_value_47_ca, order_value_48_ca, order_value_49_ca, order_value_50_ca, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_ca as text) end as summ_coeff_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_ca as text) end as order_coeff_2_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_ca as text) end as order_coeff_3_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_ca as text) end as order_coeff_4_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_ca as text) end as order_coeff_5_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_ca as text) end as order_coeff_6_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_ca as text) end as order_coeff_7_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_ca as text) end as order_coeff_8_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_ca as text) end as order_coeff_9_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_ca as text) end as order_coeff_10_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_ca as text) end as order_coeff_11_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_ca as text) end as order_coeff_12_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_ca as text) end as order_coeff_13_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_ca as text) end as order_coeff_14_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_ca as text) end as order_coeff_15_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_ca as text) end as order_coeff_16_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_ca as text) end as order_coeff_17_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_ca as text) end as order_coeff_18_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_ca as text) end as order_coeff_19_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_ca as text) end as order_coeff_20_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_ca as text) end as order_coeff_21_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_ca as text) end as order_coeff_22_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_ca as text) end as order_coeff_23_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_ca as text) end as order_coeff_24_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_ca as text) end as order_coeff_25_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_ca as text) end as order_coeff_26_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_ca as text) end as order_coeff_27_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_ca as text) end as order_coeff_28_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_ca as text) end as order_coeff_29_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_ca as text) end as order_coeff_30_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_ca as text) end as order_coeff_31_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_ca as text) end as order_coeff_32_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_ca as text) end as order_coeff_33_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_ca as text) end as order_coeff_34_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_ca as text) end as order_coeff_35_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_ca as text) end as order_coeff_36_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_ca as text) end as order_coeff_37_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_ca as text) end as order_coeff_38_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_ca as text) end as order_coeff_39_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_ca as text) end as order_coeff_40_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_ca as text) end as order_coeff_41_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_ca as text) end as order_coeff_42_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_ca as text) end as order_coeff_43_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_ca as text) end as order_coeff_44_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_ca as text) end as order_coeff_45_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_ca as text) end as order_coeff_46_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_ca as text) end as order_coeff_47_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_ca as text) end as order_coeff_48_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_ca as text) end as order_coeff_49_ca,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_ca as text) end as order_coeff_50_ca 
FROM avg_u_lin_harmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);

				#endregion

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_u_lin_harmonics", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_u_lin_harmonics");

				mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUlin::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_5");

				// в базе Эм33Т для фазных и линейных гармоник используются одни и те же поля:
				// для 3ф4пр и 1ф2пр туда кладутся фазные, а для 3ф3пр туда кладутся линейные.
				// в базе ЭтПКЭ и Эм32 фазные и линейные кладутся каждые в свои поля, поэтому устройства
				// надо обрабатывать по-разному

				string query = "";

				if (devType_ == EmDeviceType.EM31K || devType_ == EmDeviceType.EM33T ||
					devType_ == EmDeviceType.EM33T1)
				{
					if (connectScheme_ == ConnectScheme.Ph3W3 ||
						connectScheme_ == ConnectScheme.Ph3W3_B_calc)
					{
						query = String.Format("SELECT * FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else query = "";	// для 3ф3пр нет линейных гармоник
				}
				else
				{
					query = "";
					if (iMask != -1)
					{
						query = "SELECT datetime_id, event_datetime,";

						switch (connectScheme_)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W4_B_calc:
							case ConnectScheme.Ph3W3_B_calc:
								if ((iMask & 0x000000040) != 0) { query += "u1_ab,"; }		// 7 bit
								if ((iMask & 0x000000080) != 0)								// 8 bit
								{
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_uab_" + i.ToString() + ",";
									}
								}
								if ((iMask & 0x000000100) != 0) { query += "u1_bc,"; }		// 9 bit
								if ((iMask & 0x000000200) != 0)								// 10 bit
								{
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_ubc_" + i.ToString() + ",";
									}
								}
								if ((iMask & 0x000000400) != 0) { query += "u1_ca,"; }		// 11 bit
								if ((iMask & 0x000000800) != 0)								// 12 bit
								{
									for (int i = 2; i <= 40; ++i)
									{
										query += " k_uca_" + i.ToString() + ",";
									}
								}
								break;
							case ConnectScheme.Ph1W2:
								break;
						}

						// delete the last comma
						query = query.Remove(query.Length - 1, 1);

						query += String.Format(" FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else  // этот код выполняется если архив полный
					{
						query = String.Format("SELECT * FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_5", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rU = settings_.VoltageRatio;
				float ctr = 1;
				float vtr = 1;
				string commandText = string.Empty;

				if (curWithTR)
				{
					if (devType_ == EmDeviceType.EM33T ||
						devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.EM32)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.ETPQP)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);

					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (devType_ != EmDeviceType.ETPQP)
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						if (iType == 1) vtr = fValue1 / fValue2;
						else if (iType == 2) ctr = fValue1 / fValue2;
					}
				}
				rU *= vtr;

				#endregion

				#region Changing table values

				if (ds.Tables.Count > 0)
				{
					for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
					{
						// voltages
						for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
						{
							if (ds.Tables[0].Columns[i].Caption.Contains("u1_"))
							{
								if (!(ds.Tables[0].Rows[r][i] is DBNull))
									ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rU;
							}
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_5");

				if (ds.Tables.Count > 0)
				{
					mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS] = 
						(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
					mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS].PositionChanged += 
						new EventHandler(mainWnd_.currencyManager_PositionChanged);
					DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS].List;
					dataView.AllowNew = false;
					dataView.AllowDelete = false;
					dataView.AllowEdit = false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUlin::load_not_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.U_LIN_HARMONICS] = null;
		}
	}

	internal class AVGDataGridWrapperHarmI : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperHarmI(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_perc = "_" + rm.GetString("column_header_units_percent");

				string unit_a = "_";
				unit_a += (settings_.CurrentRatio == 1) ?
					 rm.GetString("column_header_units_a") :
					 rm.GetString("column_header_units_ka");

				string value_for_order =
					settings_.CurrentLanguage.Equals("ru") ? "Кп_{0}({1})" : "C ord_{0}({1})";

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_i_harmonics";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				DataGridColumnHeaderFormula cs_s_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Σgpgr>1_A" + unit_a);
				cs_s_a.HeaderText = "";
				cs_s_a.MappingName = "summ_for_order_more_1_a";
				cs_s_a.Format = DataColumnsFormat.FloatFormat;
				cs_s_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_s_a);

				for (int iCol = 1; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format("Gsg{0}_A{1}", iCol, unit_a));
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("order_value_{0}_a", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				DataGridColumnHeaderFormula cs_sc_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "THDS_A_, %");
				cs_sc_a.HeaderText = "";
				cs_sc_a.MappingName = "summ_coeff_a";
				cs_sc_a.Format = DataColumnsFormat.FloatFormat;
				cs_sc_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_sc_a);

				for (int iCol = 2; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format(value_for_order, "A", iCol) + unit_perc);
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("order_coeff_{0}_a", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_s_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Σgpgr>1_B" + unit_a);
					cs_s_b.HeaderText = "";
					cs_s_b.MappingName = "summ_for_order_more_1_b";
					cs_s_b.Format = DataColumnsFormat.FloatFormat;
					cs_s_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_b);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Gsg{0}_B{1}", iCol, unit_a));
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("order_value_{0}_b", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_sc_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "THDS_B_, %");
					cs_sc_b.HeaderText = "";
					cs_sc_b.MappingName = "summ_coeff_b";
					cs_sc_b.Format = DataColumnsFormat.FloatFormat;
					cs_sc_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_b);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format(value_for_order, "B", iCol) + unit_perc);
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("order_coeff_{0}_b", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_s_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Σgpgr>1_C" + unit_a);
					cs_s_c.HeaderText = "";
					cs_s_c.MappingName = "summ_for_order_more_1_c";
					cs_s_c.Format = DataColumnsFormat.FloatFormat;
					cs_s_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_c);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Gsg{0}_C{1}", iCol, unit_a));
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("order_value_{0}_c", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}

					DataGridColumnHeaderFormula cs_sc_c =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "THDS_C_, %");
					cs_sc_c.HeaderText = "";
					cs_sc_c.MappingName = "summ_coeff_c";
					cs_sc_c.Format = DataColumnsFormat.FloatFormat;
					cs_sc_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_c);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format(value_for_order, "C", iCol) + unit_perc);
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("order_coeff_{0}_c", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					DataGridColumnHeaderFormula cs_s_n =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Σgpgr>1_N" + unit_a);
					cs_s_n.HeaderText = "";
					cs_s_n.MappingName = "summ_for_order_more_1_n";
					cs_s_n.Format = DataColumnsFormat.FloatFormat;
					cs_s_n.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_n);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_n =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Gsg{0}_N{1}", iCol, unit_a));
						cs_n.HeaderText = "";
						cs_n.MappingName = string.Format("order_value_{0}_n", iCol);
						cs_n.Format = DataColumnsFormat.FloatFormat;
						cs_n.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_n);
					}

					DataGridColumnHeaderFormula cs_sc_n =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "THDS_N_, %");
					cs_sc_n.HeaderText = "";
					cs_sc_n.MappingName = "summ_coeff_n";
					cs_sc_n.Format = DataColumnsFormat.FloatFormat;
					cs_sc_n.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_sc_n);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_n =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format(value_for_order, "N", iCol) + unit_perc);
						cs_n.HeaderText = "";
						cs_n.MappingName = string.Format("order_coeff_{0}_n", iCol);
						cs_n.Format = DataColumnsFormat.FloatFormat;
						cs_n.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_n);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				//for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				//{
				//    (ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_a);
				//}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmI::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				string unit_perc = rm.GetString("column_header_units_percent");

				string unit_a = (settings_.CurrentRatio == 1) ?
					rm.GetString("column_header_units_a") :
					rm.GetString("column_header_units_ka");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "period_avg_params_5";
				ts.AllowSorting = false;


				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// I1_A
				DataGridColumnHeaderFormula cs_i1_a =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "I_A(1)_" + unit_a);
				cs_i1_a.HeaderText = "";
				cs_i1_a.MappingName = "i1_a";
				cs_i1_a.Format = DataColumnsFormat.FloatFormat;
				cs_i1_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_i1_a);

				// K_IA
				for (int i = 2; i < 41; i++)
				{
					DataGridColumnHeaderFormula cs_k =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
						"K_I A(" + i.ToString() + ")_" + unit_perc);
					cs_k.HeaderText = "";
					cs_k.MappingName = "k_ia_" + i.ToString();
					cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
					cs_k.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_k);
				}
				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// I1_B
					DataGridColumnHeaderFormula cs_i1_b =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "I_B(1)_" + unit_a);
					cs_i1_b.HeaderText = "";
					cs_i1_b.MappingName = "i1_b";
					cs_i1_b.Format = DataColumnsFormat.FloatFormat;
					cs_i1_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_i1_b);

					// K_IB
					for (int i = 2; i < 41; i++)
					{
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
							"K_I B(" + i.ToString() + ")_" + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_ib_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}

					// I1_C
					DataGridColumnHeaderFormula cs_i1_c =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "I_C(1)_" + unit_a);
					cs_i1_c.HeaderText = "";
					cs_i1_c.MappingName = "i1_c";
					cs_i1_c.Format = DataColumnsFormat.FloatFormat;
					cs_i1_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_i1_c);

					// K_IC
					for (int i = 2; i < 41; i++)
					{
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
							"K_I C(" + i.ToString() + ")_" + unit_perc);
						cs_k.HeaderText = "";
						cs_k.MappingName = "k_ic_" + i.ToString();
						cs_k.Format = DataColumnsFormat.GetPercentFormat(settings_.FloatSigns);
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}

					if (connectScheme_ == ConnectScheme.Ph3W4 ||
						connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						if (devType_ == EmDeviceType.ETPQP ||
							devType_ == EmDeviceType.ETPQP_A)
						{
							// I1_n
							DataGridColumnHeaderFormula cs_i1_n =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "I_N(1)_" + unit_a);
							cs_i1_n.HeaderText = "";
							cs_i1_n.MappingName = "i1_n";
							cs_i1_n.Format = DataColumnsFormat.FloatFormat;
							cs_i1_n.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_i1_n);

							for (int i = 2; i < 41; i++)
							{
								DataGridColumnHeaderFormula cs_in =
									new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
									"I_N (" + i.ToString() + ")_" + unit_a);
								cs_in.HeaderText = "";
								cs_in.MappingName = "i_n_" + i.ToString();
								cs_in.Format = DataColumnsFormat.FloatFormat;
								cs_in.Width = DataColumnsWidth.CommonWidth;
								ts.GridColumnStyles.Add(cs_in);
							}
						}
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmI::init(): ");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string query = string.Empty;

				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				#region Query

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					query = String.Format(@"SELECT dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
					p.datetime_id, p.record_id, summ_for_order_more_1_a, order_value_1_a, 
       order_value_2_a, order_value_3_a, order_value_4_a, order_value_5_a, 
       order_value_6_a, order_value_7_a, order_value_8_a, order_value_9_a, 
       order_value_10_a, order_value_11_a, order_value_12_a, order_value_13_a, 
       order_value_14_a, order_value_15_a, order_value_16_a, order_value_17_a, 
       order_value_18_a, order_value_19_a, order_value_20_a, order_value_21_a, 
       order_value_22_a, order_value_23_a, order_value_24_a, order_value_25_a, 
       order_value_26_a, order_value_27_a, order_value_28_a, order_value_29_a, 
       order_value_30_a, order_value_31_a, order_value_32_a, order_value_33_a, 
       order_value_34_a, order_value_35_a, order_value_36_a, order_value_37_a, 
       order_value_38_a, order_value_39_a, order_value_40_a, order_value_41_a, 
       order_value_42_a, order_value_43_a, order_value_44_a, order_value_45_a, 
       order_value_46_a, order_value_47_a, order_value_48_a, order_value_49_a, 
       order_value_50_a, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_a as text) end as summ_coeff_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_a as text) end as order_coeff_2_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_a as text) end as order_coeff_3_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_a as text) end as order_coeff_4_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_a as text) end as order_coeff_5_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_a as text) end as order_coeff_6_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_a as text) end as order_coeff_7_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_a as text) end as order_coeff_8_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_a as text) end as order_coeff_9_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_a as text) end as order_coeff_10_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_a as text) end as order_coeff_11_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_a as text) end as order_coeff_12_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_a as text) end as order_coeff_13_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_a as text) end as order_coeff_14_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_a as text) end as order_coeff_15_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_a as text) end as order_coeff_16_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_a as text) end as order_coeff_17_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_a as text) end as order_coeff_18_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_a as text) end as order_coeff_19_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_a as text) end as order_coeff_20_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_a as text) end as order_coeff_21_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_a as text) end as order_coeff_22_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_a as text) end as order_coeff_23_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_a as text) end as order_coeff_24_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_a as text) end as order_coeff_25_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_a as text) end as order_coeff_26_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_a as text) end as order_coeff_27_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_a as text) end as order_coeff_28_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_a as text) end as order_coeff_29_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_a as text) end as order_coeff_30_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_a as text) end as order_coeff_31_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_a as text) end as order_coeff_32_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_a as text) end as order_coeff_33_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_a as text) end as order_coeff_34_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_a as text) end as order_coeff_35_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_a as text) end as order_coeff_36_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_a as text) end as order_coeff_37_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_a as text) end as order_coeff_38_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_a as text) end as order_coeff_39_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_a as text) end as order_coeff_40_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_a as text) end as order_coeff_41_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_a as text) end as order_coeff_42_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_a as text) end as order_coeff_43_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_a as text) end as order_coeff_44_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_a as text) end as order_coeff_45_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_a as text) end as order_coeff_46_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_a as text) end as order_coeff_47_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_a as text) end as order_coeff_48_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_a as text) end as order_coeff_49_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_a as text) end as order_coeff_50_a, 
summ_for_order_more_1_b, 
       order_value_1_b, order_value_2_b, order_value_3_b, order_value_4_b, 
       order_value_5_b, order_value_6_b, order_value_7_b, order_value_8_b, 
       order_value_9_b, order_value_10_b, order_value_11_b, order_value_12_b, 
       order_value_13_b, order_value_14_b, order_value_15_b, order_value_16_b, 
       order_value_17_b, order_value_18_b, order_value_19_b, order_value_20_b, 
       order_value_21_b, order_value_22_b, order_value_23_b, order_value_24_b, 
       order_value_25_b, order_value_26_b, order_value_27_b, order_value_28_b, 
       order_value_29_b, order_value_30_b, order_value_31_b, order_value_32_b, 
       order_value_33_b, order_value_34_b, order_value_35_b, order_value_36_b, 
       order_value_37_b, order_value_38_b, order_value_39_b, order_value_40_b, 
       order_value_41_b, order_value_42_b, order_value_43_b, order_value_44_b, 
       order_value_45_b, order_value_46_b, order_value_47_b, order_value_48_b, 
       order_value_49_b, order_value_50_b, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_b as text) end as summ_coeff_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_b as text) end as order_coeff_2_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_b as text) end as order_coeff_3_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_b as text) end as order_coeff_4_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_b as text) end as order_coeff_5_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_b as text) end as order_coeff_6_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_b as text) end as order_coeff_7_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_b as text) end as order_coeff_8_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_b as text) end as order_coeff_9_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_b as text) end as order_coeff_10_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_b as text) end as order_coeff_11_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_b as text) end as order_coeff_12_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_b as text) end as order_coeff_13_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_b as text) end as order_coeff_14_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_b as text) end as order_coeff_15_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_b as text) end as order_coeff_16_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_b as text) end as order_coeff_17_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_b as text) end as order_coeff_18_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_b as text) end as order_coeff_19_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_b as text) end as order_coeff_20_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_b as text) end as order_coeff_21_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_b as text) end as order_coeff_22_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_b as text) end as order_coeff_23_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_b as text) end as order_coeff_24_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_b as text) end as order_coeff_25_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_b as text) end as order_coeff_26_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_b as text) end as order_coeff_27_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_b as text) end as order_coeff_28_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_b as text) end as order_coeff_29_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_b as text) end as order_coeff_30_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_b as text) end as order_coeff_31_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_b as text) end as order_coeff_32_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_b as text) end as order_coeff_33_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_b as text) end as order_coeff_34_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_b as text) end as order_coeff_35_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_b as text) end as order_coeff_36_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_b as text) end as order_coeff_37_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_b as text) end as order_coeff_38_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_b as text) end as order_coeff_39_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_b as text) end as order_coeff_40_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_b as text) end as order_coeff_41_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_b as text) end as order_coeff_42_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_b as text) end as order_coeff_43_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_b as text) end as order_coeff_44_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_b as text) end as order_coeff_45_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_b as text) end as order_coeff_46_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_b as text) end as order_coeff_47_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_b as text) end as order_coeff_48_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_b as text) end as order_coeff_49_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_b as text) end as order_coeff_50_b, 
summ_for_order_more_1_c, order_value_1_c, order_value_2_c, order_value_3_c, 
       order_value_4_c, order_value_5_c, order_value_6_c, order_value_7_c, 
       order_value_8_c, order_value_9_c, order_value_10_c, order_value_11_c, 
       order_value_12_c, order_value_13_c, order_value_14_c, order_value_15_c, 
       order_value_16_c, order_value_17_c, order_value_18_c, order_value_19_c, 
       order_value_20_c, order_value_21_c, order_value_22_c, order_value_23_c, 
       order_value_24_c, order_value_25_c, order_value_26_c, order_value_27_c, 
       order_value_28_c, order_value_29_c, order_value_30_c, order_value_31_c, 
       order_value_32_c, order_value_33_c, order_value_34_c, order_value_35_c, 
       order_value_36_c, order_value_37_c, order_value_38_c, order_value_39_c, 
       order_value_40_c, order_value_41_c, order_value_42_c, order_value_43_c, 
       order_value_44_c, order_value_45_c, order_value_46_c, order_value_47_c, 
       order_value_48_c, order_value_49_c, order_value_50_c, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_c as text) end as summ_coeff_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_c as text) end as order_coeff_2_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_c as text) end as order_coeff_3_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_c as text) end as order_coeff_4_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_c as text) end as order_coeff_5_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_c as text) end as order_coeff_6_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_c as text) end as order_coeff_7_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_c as text) end as order_coeff_8_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_c as text) end as order_coeff_9_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_c as text) end as order_coeff_10_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_c as text) end as order_coeff_11_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_c as text) end as order_coeff_12_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_c as text) end as order_coeff_13_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_c as text) end as order_coeff_14_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_c as text) end as order_coeff_15_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_c as text) end as order_coeff_16_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_c as text) end as order_coeff_17_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_c as text) end as order_coeff_18_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_c as text) end as order_coeff_19_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_c as text) end as order_coeff_20_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_c as text) end as order_coeff_21_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_c as text) end as order_coeff_22_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_c as text) end as order_coeff_23_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_c as text) end as order_coeff_24_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_c as text) end as order_coeff_25_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_c as text) end as order_coeff_26_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_c as text) end as order_coeff_27_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_c as text) end as order_coeff_28_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_c as text) end as order_coeff_29_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_c as text) end as order_coeff_30_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_c as text) end as order_coeff_31_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_c as text) end as order_coeff_32_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_c as text) end as order_coeff_33_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_c as text) end as order_coeff_34_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_c as text) end as order_coeff_35_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_c as text) end as order_coeff_36_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_c as text) end as order_coeff_37_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_c as text) end as order_coeff_38_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_c as text) end as order_coeff_39_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_c as text) end as order_coeff_40_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_c as text) end as order_coeff_41_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_c as text) end as order_coeff_42_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_c as text) end as order_coeff_43_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_c as text) end as order_coeff_44_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_c as text) end as order_coeff_45_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_c as text) end as order_coeff_46_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_c as text) end as order_coeff_47_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_c as text) end as order_coeff_48_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_c as text) end as order_coeff_49_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_c as text) end as order_coeff_50_c, 
summ_for_order_more_1_n, order_value_1_n, order_value_2_n, 
       order_value_3_n, order_value_4_n, order_value_5_n, order_value_6_n, 
       order_value_7_n, order_value_8_n, order_value_9_n, order_value_10_n, 
       order_value_11_n, order_value_12_n, order_value_13_n, order_value_14_n, 
       order_value_15_n, order_value_16_n, order_value_17_n, order_value_18_n, 
       order_value_19_n, order_value_20_n, order_value_21_n, order_value_22_n, 
       order_value_23_n, order_value_24_n, order_value_25_n, order_value_26_n, 
       order_value_27_n, order_value_28_n, order_value_29_n, order_value_30_n, 
       order_value_31_n, order_value_32_n, order_value_33_n, order_value_34_n, 
       order_value_35_n, order_value_36_n, order_value_37_n, order_value_38_n, 
       order_value_39_n, order_value_40_n, order_value_41_n, order_value_42_n, 
       order_value_43_n, order_value_44_n, order_value_45_n, order_value_46_n, 
       order_value_47_n, order_value_48_n, order_value_49_n, order_value_50_n, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_n as text) end as summ_coeff_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_n as text) end as order_coeff_2_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_n as text) end as order_coeff_3_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_n as text) end as order_coeff_4_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_n as text) end as order_coeff_5_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_n as text) end as order_coeff_6_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_n as text) end as order_coeff_7_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_n as text) end as order_coeff_8_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_n as text) end as order_coeff_9_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_n as text) end as order_coeff_10_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_n as text) end as order_coeff_11_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_n as text) end as order_coeff_12_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_n as text) end as order_coeff_13_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_n as text) end as order_coeff_14_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_n as text) end as order_coeff_15_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_n as text) end as order_coeff_16_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_n as text) end as order_coeff_17_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_n as text) end as order_coeff_18_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_n as text) end as order_coeff_19_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_n as text) end as order_coeff_20_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_n as text) end as order_coeff_21_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_n as text) end as order_coeff_22_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_n as text) end as order_coeff_23_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_n as text) end as order_coeff_24_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_n as text) end as order_coeff_25_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_n as text) end as order_coeff_26_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_n as text) end as order_coeff_27_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_n as text) end as order_coeff_28_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_n as text) end as order_coeff_29_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_n as text) end as order_coeff_30_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_n as text) end as order_coeff_31_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_n as text) end as order_coeff_32_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_n as text) end as order_coeff_33_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_n as text) end as order_coeff_34_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_n as text) end as order_coeff_35_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_n as text) end as order_coeff_36_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_n as text) end as order_coeff_37_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_n as text) end as order_coeff_38_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_n as text) end as order_coeff_39_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_n as text) end as order_coeff_40_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_n as text) end as order_coeff_41_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_n as text) end as order_coeff_42_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_n as text) end as order_coeff_43_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_n as text) end as order_coeff_44_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_n as text) end as order_coeff_45_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_n as text) end as order_coeff_46_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_n as text) end as order_coeff_47_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_n as text) end as order_coeff_48_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_n as text) end as order_coeff_49_n,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_n as text) end as order_coeff_50_n
FROM avg_i_harmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
       summ_for_order_more_1_a, order_value_1_a, 
       order_value_2_a, order_value_3_a, order_value_4_a, order_value_5_a, 
       order_value_6_a, order_value_7_a, order_value_8_a, order_value_9_a, 
       order_value_10_a, order_value_11_a, order_value_12_a, order_value_13_a, 
       order_value_14_a, order_value_15_a, order_value_16_a, order_value_17_a, 
       order_value_18_a, order_value_19_a, order_value_20_a, order_value_21_a, 
       order_value_22_a, order_value_23_a, order_value_24_a, order_value_25_a, 
       order_value_26_a, order_value_27_a, order_value_28_a, order_value_29_a, 
       order_value_30_a, order_value_31_a, order_value_32_a, order_value_33_a, 
       order_value_34_a, order_value_35_a, order_value_36_a, order_value_37_a, 
       order_value_38_a, order_value_39_a, order_value_40_a, order_value_41_a, 
       order_value_42_a, order_value_43_a, order_value_44_a, order_value_45_a, 
       order_value_46_a, order_value_47_a, order_value_48_a, order_value_49_a, 
       order_value_50_a, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_a as text) end as summ_coeff_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_a as text) end as order_coeff_2_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_a as text) end as order_coeff_3_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_a as text) end as order_coeff_4_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_a as text) end as order_coeff_5_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_a as text) end as order_coeff_6_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_a as text) end as order_coeff_7_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_a as text) end as order_coeff_8_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_a as text) end as order_coeff_9_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_a as text) end as order_coeff_10_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_a as text) end as order_coeff_11_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_a as text) end as order_coeff_12_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_a as text) end as order_coeff_13_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_a as text) end as order_coeff_14_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_a as text) end as order_coeff_15_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_a as text) end as order_coeff_16_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_a as text) end as order_coeff_17_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_a as text) end as order_coeff_18_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_a as text) end as order_coeff_19_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_a as text) end as order_coeff_20_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_a as text) end as order_coeff_21_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_a as text) end as order_coeff_22_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_a as text) end as order_coeff_23_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_a as text) end as order_coeff_24_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_a as text) end as order_coeff_25_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_a as text) end as order_coeff_26_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_a as text) end as order_coeff_27_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_a as text) end as order_coeff_28_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_a as text) end as order_coeff_29_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_a as text) end as order_coeff_30_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_a as text) end as order_coeff_31_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_a as text) end as order_coeff_32_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_a as text) end as order_coeff_33_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_a as text) end as order_coeff_34_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_a as text) end as order_coeff_35_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_a as text) end as order_coeff_36_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_a as text) end as order_coeff_37_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_a as text) end as order_coeff_38_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_a as text) end as order_coeff_39_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_a as text) end as order_coeff_40_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_a as text) end as order_coeff_41_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_a as text) end as order_coeff_42_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_a as text) end as order_coeff_43_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_a as text) end as order_coeff_44_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_a as text) end as order_coeff_45_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_a as text) end as order_coeff_46_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_a as text) end as order_coeff_47_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_a as text) end as order_coeff_48_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_a as text) end as order_coeff_49_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_a as text) end as order_coeff_50_a, 
summ_for_order_more_1_b, 
       order_value_1_b, order_value_2_b, order_value_3_b, order_value_4_b, 
       order_value_5_b, order_value_6_b, order_value_7_b, order_value_8_b, 
       order_value_9_b, order_value_10_b, order_value_11_b, order_value_12_b, 
       order_value_13_b, order_value_14_b, order_value_15_b, order_value_16_b, 
       order_value_17_b, order_value_18_b, order_value_19_b, order_value_20_b, 
       order_value_21_b, order_value_22_b, order_value_23_b, order_value_24_b, 
       order_value_25_b, order_value_26_b, order_value_27_b, order_value_28_b, 
       order_value_29_b, order_value_30_b, order_value_31_b, order_value_32_b, 
       order_value_33_b, order_value_34_b, order_value_35_b, order_value_36_b, 
       order_value_37_b, order_value_38_b, order_value_39_b, order_value_40_b, 
       order_value_41_b, order_value_42_b, order_value_43_b, order_value_44_b, 
       order_value_45_b, order_value_46_b, order_value_47_b, order_value_48_b, 
       order_value_49_b, order_value_50_b, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_b as text) end as summ_coeff_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_b as text) end as order_coeff_2_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_b as text) end as order_coeff_3_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_b as text) end as order_coeff_4_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_b as text) end as order_coeff_5_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_b as text) end as order_coeff_6_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_b as text) end as order_coeff_7_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_b as text) end as order_coeff_8_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_b as text) end as order_coeff_9_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_b as text) end as order_coeff_10_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_b as text) end as order_coeff_11_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_b as text) end as order_coeff_12_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_b as text) end as order_coeff_13_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_b as text) end as order_coeff_14_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_b as text) end as order_coeff_15_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_b as text) end as order_coeff_16_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_b as text) end as order_coeff_17_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_b as text) end as order_coeff_18_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_b as text) end as order_coeff_19_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_b as text) end as order_coeff_20_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_b as text) end as order_coeff_21_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_b as text) end as order_coeff_22_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_b as text) end as order_coeff_23_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_b as text) end as order_coeff_24_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_b as text) end as order_coeff_25_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_b as text) end as order_coeff_26_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_b as text) end as order_coeff_27_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_b as text) end as order_coeff_28_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_b as text) end as order_coeff_29_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_b as text) end as order_coeff_30_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_b as text) end as order_coeff_31_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_b as text) end as order_coeff_32_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_b as text) end as order_coeff_33_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_b as text) end as order_coeff_34_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_b as text) end as order_coeff_35_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_b as text) end as order_coeff_36_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_b as text) end as order_coeff_37_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_b as text) end as order_coeff_38_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_b as text) end as order_coeff_39_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_b as text) end as order_coeff_40_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_b as text) end as order_coeff_41_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_b as text) end as order_coeff_42_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_b as text) end as order_coeff_43_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_b as text) end as order_coeff_44_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_b as text) end as order_coeff_45_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_b as text) end as order_coeff_46_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_b as text) end as order_coeff_47_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_b as text) end as order_coeff_48_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_b as text) end as order_coeff_49_b,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_b as text) end as order_coeff_50_b, 
summ_for_order_more_1_c, order_value_1_c, order_value_2_c, order_value_3_c, 
       order_value_4_c, order_value_5_c, order_value_6_c, order_value_7_c, 
       order_value_8_c, order_value_9_c, order_value_10_c, order_value_11_c, 
       order_value_12_c, order_value_13_c, order_value_14_c, order_value_15_c, 
       order_value_16_c, order_value_17_c, order_value_18_c, order_value_19_c, 
       order_value_20_c, order_value_21_c, order_value_22_c, order_value_23_c, 
       order_value_24_c, order_value_25_c, order_value_26_c, order_value_27_c, 
       order_value_28_c, order_value_29_c, order_value_30_c, order_value_31_c, 
       order_value_32_c, order_value_33_c, order_value_34_c, order_value_35_c, 
       order_value_36_c, order_value_37_c, order_value_38_c, order_value_39_c, 
       order_value_40_c, order_value_41_c, order_value_42_c, order_value_43_c, 
       order_value_44_c, order_value_45_c, order_value_46_c, order_value_47_c, 
       order_value_48_c, order_value_49_c, order_value_50_c, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_c as text) end as summ_coeff_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_c as text) end as order_coeff_2_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_c as text) end as order_coeff_3_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_c as text) end as order_coeff_4_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_c as text) end as order_coeff_5_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_c as text) end as order_coeff_6_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_c as text) end as order_coeff_7_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_c as text) end as order_coeff_8_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_c as text) end as order_coeff_9_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_c as text) end as order_coeff_10_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_c as text) end as order_coeff_11_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_c as text) end as order_coeff_12_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_c as text) end as order_coeff_13_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_c as text) end as order_coeff_14_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_c as text) end as order_coeff_15_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_c as text) end as order_coeff_16_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_c as text) end as order_coeff_17_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_c as text) end as order_coeff_18_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_c as text) end as order_coeff_19_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_c as text) end as order_coeff_20_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_c as text) end as order_coeff_21_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_c as text) end as order_coeff_22_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_c as text) end as order_coeff_23_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_c as text) end as order_coeff_24_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_c as text) end as order_coeff_25_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_c as text) end as order_coeff_26_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_c as text) end as order_coeff_27_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_c as text) end as order_coeff_28_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_c as text) end as order_coeff_29_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_c as text) end as order_coeff_30_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_c as text) end as order_coeff_31_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_c as text) end as order_coeff_32_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_c as text) end as order_coeff_33_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_c as text) end as order_coeff_34_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_c as text) end as order_coeff_35_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_c as text) end as order_coeff_36_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_c as text) end as order_coeff_37_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_c as text) end as order_coeff_38_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_c as text) end as order_coeff_39_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_c as text) end as order_coeff_40_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_c as text) end as order_coeff_41_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_c as text) end as order_coeff_42_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_c as text) end as order_coeff_43_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_c as text) end as order_coeff_44_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_c as text) end as order_coeff_45_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_c as text) end as order_coeff_46_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_c as text) end as order_coeff_47_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_c as text) end as order_coeff_48_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_c as text) end as order_coeff_49_c,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_c as text) end as order_coeff_50_c
FROM avg_i_harmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}
				else if (connectScheme_ == ConnectScheme.Ph1W2)
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
       summ_for_order_more_1_a, order_value_1_a, 
       order_value_2_a, order_value_3_a, order_value_4_a, order_value_5_a, 
       order_value_6_a, order_value_7_a, order_value_8_a, order_value_9_a, 
       order_value_10_a, order_value_11_a, order_value_12_a, order_value_13_a, 
       order_value_14_a, order_value_15_a, order_value_16_a, order_value_17_a, 
       order_value_18_a, order_value_19_a, order_value_20_a, order_value_21_a, 
       order_value_22_a, order_value_23_a, order_value_24_a, order_value_25_a, 
       order_value_26_a, order_value_27_a, order_value_28_a, order_value_29_a, 
       order_value_30_a, order_value_31_a, order_value_32_a, order_value_33_a, 
       order_value_34_a, order_value_35_a, order_value_36_a, order_value_37_a, 
       order_value_38_a, order_value_39_a, order_value_40_a, order_value_41_a, 
       order_value_42_a, order_value_43_a, order_value_44_a, order_value_45_a, 
       order_value_46_a, order_value_47_a, order_value_48_a, order_value_49_a, 
       order_value_50_a, 
case when cnt_windows_not_locked > 0 then '-' else cast(summ_coeff_a as text) end as summ_coeff_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_2_a as text) end as order_coeff_2_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_3_a as text) end as order_coeff_3_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_4_a as text) end as order_coeff_4_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_5_a as text) end as order_coeff_5_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_6_a as text) end as order_coeff_6_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_7_a as text) end as order_coeff_7_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_8_a as text) end as order_coeff_8_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_9_a as text) end as order_coeff_9_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_10_a as text) end as order_coeff_10_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_11_a as text) end as order_coeff_11_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_12_a as text) end as order_coeff_12_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_13_a as text) end as order_coeff_13_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_14_a as text) end as order_coeff_14_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_15_a as text) end as order_coeff_15_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_16_a as text) end as order_coeff_16_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_17_a as text) end as order_coeff_17_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_18_a as text) end as order_coeff_18_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_19_a as text) end as order_coeff_19_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_20_a as text) end as order_coeff_20_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_21_a as text) end as order_coeff_21_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_22_a as text) end as order_coeff_22_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_23_a as text) end as order_coeff_23_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_24_a as text) end as order_coeff_24_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_25_a as text) end as order_coeff_25_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_26_a as text) end as order_coeff_26_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_27_a as text) end as order_coeff_27_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_28_a as text) end as order_coeff_28_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_29_a as text) end as order_coeff_29_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_30_a as text) end as order_coeff_30_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_31_a as text) end as order_coeff_31_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_32_a as text) end as order_coeff_32_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_33_a as text) end as order_coeff_33_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_34_a as text) end as order_coeff_34_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_35_a as text) end as order_coeff_35_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_36_a as text) end as order_coeff_36_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_37_a as text) end as order_coeff_37_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_38_a as text) end as order_coeff_38_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_39_a as text) end as order_coeff_39_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_40_a as text) end as order_coeff_40_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_41_a as text) end as order_coeff_41_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_42_a as text) end as order_coeff_42_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_43_a as text) end as order_coeff_43_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_44_a as text) end as order_coeff_44_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_45_a as text) end as order_coeff_45_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_46_a as text) end as order_coeff_46_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_47_a as text) end as order_coeff_47_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_48_a as text) end as order_coeff_48_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_49_a as text) end as order_coeff_49_a,
case when cnt_windows_not_locked > 0 then '-' else cast(order_coeff_50_a as text) end as order_coeff_50_a
  FROM avg_i_harmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}

				#endregion

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_i_harmonics", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_i_harmonics");

				mainWnd_.CMs_[(int)AvgPages.I_HARMONICS] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.I_HARMONICS].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.I_HARMONICS].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmI::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR,
			bool bHarmCurrentShownInPercent)
		{
			try
			{
				Int64 iMask, iMask2;
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_5");

				string query = "";
				query = "SELECT datetime_id, event_datetime,";
				if ((iMask & 0x000000008) != 0 || iMask == -1)			// 4 bit
				{
					query += "i1_a,";
					if (bHarmCurrentShownInPercent)
					{
						for (int i = 2; i <= 40; ++i)
						{
							query += " k_ia_" + i.ToString() + ",";
						}
					}
					else
					{
						for (int i = 2; i <= 40; ++i)
						{
							query += string.Format(" (k_ia_{0} * i1_a) / 100 as k_ia_{1},", i, i);
						}
					}
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					if ((iMask & 0x000000010) != 0 || iMask == -1)		// 5 bit
					{
						query += "i1_b,";
						if (bHarmCurrentShownInPercent)
						{
							for (int i = 2; i <= 40; ++i)
							{
								query += " k_ib_" + i.ToString() + ",";
							}
						}
						else
						{
							for (int i = 2; i <= 40; ++i)
							{
								query += string.Format(" (k_ib_{0} * i1_b) / 100 as k_ib_{1},", i, i);
							}
						}
					}
					if ((iMask & 0x000000020) != 0 || iMask == -1)		// 6 bit
					{
						query += "i1_c,";
						if (bHarmCurrentShownInPercent)
						{
							for (int i = 2; i <= 40; ++i)
							{
								query += " k_ic_" + i.ToString() + ",";
							}
						}
						else
						{
							for (int i = 2; i <= 40; ++i)
							{
								query += string.Format(" (k_ic_{0} * i1_c) / 100 as k_ic_{1},", i, i);
							}
						}
					}
					if (devType_ == EmDeviceType.ETPQP ||
						devType_ == EmDeviceType.ETPQP_A)
					{
						if (connectScheme_ == ConnectScheme.Ph3W4 ||
							connectScheme_ == ConnectScheme.Ph3W4_B_calc)
						{
							if ((iMask & 0x000001000) != 0)							// 13 bit	
							{
								query += "i1_n,";
								for (int i = 2; i <= 40; ++i)
								{
									query += " i_n_" + i.ToString() + ",";
								}
							}
						}
					}
				}

				// delete the last comma
				query = query.Remove(query.Length - 1, 1);

				query += String.Format(" FROM period_avg_params_5 WHERE datetime_id = {0} ORDER BY event_datetime ASC;",
					curDatetimeId_);

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_5", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rI = settings_.CurrentRatio;
				float ctr = 1, vtr = 1;
				string commandText = string.Empty;

				if (curWithTR)
				{
					if (devType_ == EmDeviceType.EM33T ||
						devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.EM32)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.ETPQP)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);

					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (devType_ != EmDeviceType.ETPQP)
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						if (iType == 1) vtr = fValue1 / fValue2;
						else if (iType == 2) ctr = fValue1 / fValue2;
					}
				}
				rI *= ctr;

				#endregion

				#region Changing table values

				if (ds.Tables.Count > 0)
				{
					for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
					{
						// currents
						for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
						{
							if (ds.Tables[0].Columns[i].Caption.Contains("i1_"))
							{
								if (!(ds.Tables[0].Rows[r][i] is DBNull))
									ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rI;
							}
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_5");

				if (ds.Tables.Count > 0)
				{
					mainWnd_.CMs_[(int)AvgPages.I_HARMONICS] = 
						(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
					mainWnd_.CMs_[(int)AvgPages.I_HARMONICS].PositionChanged += 
						new EventHandler(mainWnd_.currencyManager_PositionChanged);
					DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.I_HARMONICS].List;
					dataView.AllowNew = false;
					dataView.AllowDelete = false;
					dataView.AllowEdit = false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmI::load_not_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.I_HARMONICS] = null;
		}

		public void RenameHarmonicCurrentColumns(bool toPercent)
		{
			try
			{
				string tableName = "period_avg_params_5";
				if (devType_ == EmDeviceType.ETPQP_A) tableName = "";

				string newUnit = "";
				if (toPercent) newUnit = ", %";
				else
				{
					ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					newUnit = (settings_.PowerRatio == 1) ?
						rm.GetString("column_header_units_a") :
						rm.GetString("column_header_units_ka");
				}

				for (int i = 2; i <= 40; ++i)
				{
					DataGridColumnHeaderFormula newCap =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								"K_I A(" + i.ToString() + ")_" + newUnit);

					DataGridColumnHeaderFormula oldCap =
					(dataGrid_.TableStyles[tableName].GridColumnStyles[
							"k_ia_" + i.ToString()] as DataGridColumnHeaderFormula);
					oldCap.HeaderFormula = newCap.HeaderFormula;
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					for (int i = 2; i <= 40; ++i)
					{
						DataGridColumnHeaderFormula newCap =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
									"K_I B(" + i.ToString() + ")_" + newUnit);

						DataGridColumnHeaderFormula oldCap =
						(dataGrid_.TableStyles[tableName].GridColumnStyles[
								"k_ib_" + i.ToString()] as DataGridColumnHeaderFormula);
						oldCap.HeaderFormula = newCap.HeaderFormula;
					}

					for (int i = 2; i <= 40; ++i)
					{
						DataGridColumnHeaderFormula newCap =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
									"K_I C(" + i.ToString() + ")_" + newUnit);

						DataGridColumnHeaderFormula oldCap =
						(dataGrid_.TableStyles[tableName].GridColumnStyles[
								"k_ic_" + i.ToString()] as DataGridColumnHeaderFormula);
						oldCap.HeaderFormula = newCap.HeaderFormula;
					}
				}

				dataGrid_.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in RenameHarmonicPowersColumns():");
				throw;
			}
		}
	}

	internal class AVGDataGridWrapperInterHarmI : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperInterHarmI(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_a = (settings_.CurrentRatio == 1) ?
					rm.GetString("column_header_units_a") :
					rm.GetString("column_header_units_ka");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_i_interharmonics";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				DataGridColumnHeaderFormula cs_s_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Cig0_A");
				cs_s_a.HeaderText = "";
				cs_s_a.MappingName = "avg_square_a";
				cs_s_a.Format = DataColumnsFormat.FloatFormat;
				cs_s_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_s_a);

				for (int iCol = 1; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format("Cig{0}_A", iCol));
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("avg_square_order_{0}_a", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_s_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Cig0_B");
					cs_s_b.HeaderText = "";
					cs_s_b.MappingName = "avg_square_b";
					cs_s_b.Format = DataColumnsFormat.FloatFormat;
					cs_s_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_b);

					for (int iCol = 2; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Cig{0}_B", iCol));
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("avg_square_order_{0}_b", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_s_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Cig0_C");
					cs_s_c.HeaderText = "";
					cs_s_c.MappingName = "avg_square_c";
					cs_s_c.Format = DataColumnsFormat.FloatFormat;
					cs_s_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_c);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Cig{0}_C", iCol));
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("avg_square_order_{0}_c", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					DataGridColumnHeaderFormula cs_s_n =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Cig0_N");
					cs_s_n.HeaderText = "";
					cs_s_n.MappingName = "avg_square_n";
					cs_s_n.Format = DataColumnsFormat.FloatFormat;
					cs_s_n.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_n);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_n =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Cig{0}_N", iCol));
						cs_n.HeaderText = "";
						cs_n.MappingName = string.Format("avg_square_order_{0}_n", iCol);
						cs_n.Format = DataColumnsFormat.FloatFormat;
						cs_n.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_n);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_a);
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmI::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			// dummy
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string query = string.Empty;

				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				#region Query

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					query = String.Format("SELECT dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, p.* FROM avg_i_interharmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", 
						str_yes, str_no, curDatetimeId_);
				}
				else
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked,
       avg_square_a, avg_square_order_1_a, avg_square_order_2_a, 
       avg_square_order_3_a, avg_square_order_4_a, avg_square_order_5_a, 
       avg_square_order_6_a, avg_square_order_7_a, avg_square_order_8_a, 
       avg_square_order_9_a, avg_square_order_10_a, avg_square_order_11_a, 
       avg_square_order_12_a, avg_square_order_13_a, avg_square_order_14_a, 
       avg_square_order_15_a, avg_square_order_16_a, avg_square_order_17_a, 
       avg_square_order_18_a, avg_square_order_19_a, avg_square_order_20_a, 
       avg_square_order_21_a, avg_square_order_22_a, avg_square_order_23_a, 
       avg_square_order_24_a, avg_square_order_25_a, avg_square_order_26_a, 
       avg_square_order_27_a, avg_square_order_28_a, avg_square_order_29_a, 
       avg_square_order_30_a, avg_square_order_31_a, avg_square_order_32_a, 
       avg_square_order_33_a, avg_square_order_34_a, avg_square_order_35_a, 
       avg_square_order_36_a, avg_square_order_37_a, avg_square_order_38_a, 
       avg_square_order_39_a, avg_square_order_40_a, avg_square_order_41_a, 
       avg_square_order_42_a, avg_square_order_43_a, avg_square_order_44_a, 
       avg_square_order_45_a, avg_square_order_46_a, avg_square_order_47_a, 
       avg_square_order_48_a, avg_square_order_49_a, avg_square_order_50_a
  FROM avg_i_interharmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}

				#endregion

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_i_interharmonics", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_i_interharmonics");

				mainWnd_.CMs_[(int)AvgPages.I_INTERHARM] =
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.I_INTERHARM].PositionChanged +=
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.I_INTERHARM].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperInterHarmI::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			// dummy
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.I_INTERHARM] = null;
		}
	}

	internal class AVGDataGridWrapperInterHarmUph : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperInterHarmUph(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_u_ph_interharmonics";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				DataGridColumnHeaderFormula cs_s_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Cig0_A");
				cs_s_a.HeaderText = "";
				cs_s_a.MappingName = "avg_square_a";
				cs_s_a.Format = DataColumnsFormat.FloatFormat;
				cs_s_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_s_a);

				for (int iCol = 1; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format("Cig{0}_A", iCol));
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("avg_square_order_{0}_a", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_s_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Cig0_B");
					cs_s_b.HeaderText = "";
					cs_s_b.MappingName = "avg_square_b";
					cs_s_b.Format = DataColumnsFormat.FloatFormat;
					cs_s_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_b);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Cig{0}_B", iCol));
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("avg_square_order_{0}_b", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_s_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Cig0_C");
					cs_s_c.HeaderText = "";
					cs_s_c.MappingName = "avg_square_c";
					cs_s_c.Format = DataColumnsFormat.FloatFormat;
					cs_s_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_c);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Cig{0}_C", iCol));
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("avg_square_order_{0}_c", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_v);
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUph::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			// dummy
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string query = string.Empty;

				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					query = String.Format("SELECT dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, p.* FROM avg_u_ph_interharmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}
				else if (connectScheme_ == ConnectScheme.Ph1W2)
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked,
       avg_square_a, avg_square_order_1_a, avg_square_order_2_a, 
       avg_square_order_3_a, avg_square_order_4_a, avg_square_order_5_a, 
       avg_square_order_6_a, avg_square_order_7_a, avg_square_order_8_a, 
       avg_square_order_9_a, avg_square_order_10_a, avg_square_order_11_a, 
       avg_square_order_12_a, avg_square_order_13_a, avg_square_order_14_a, 
       avg_square_order_15_a, avg_square_order_16_a, avg_square_order_17_a, 
       avg_square_order_18_a, avg_square_order_19_a, avg_square_order_20_a, 
       avg_square_order_21_a, avg_square_order_22_a, avg_square_order_23_a, 
       avg_square_order_24_a, avg_square_order_25_a, avg_square_order_26_a, 
       avg_square_order_27_a, avg_square_order_28_a, avg_square_order_29_a, 
       avg_square_order_30_a, avg_square_order_31_a, avg_square_order_32_a, 
       avg_square_order_33_a, avg_square_order_34_a, avg_square_order_35_a, 
       avg_square_order_36_a, avg_square_order_37_a, avg_square_order_38_a, 
       avg_square_order_39_a, avg_square_order_40_a, avg_square_order_41_a, 
       avg_square_order_42_a, avg_square_order_43_a, avg_square_order_44_a, 
       avg_square_order_45_a, avg_square_order_46_a, avg_square_order_47_a, 
       avg_square_order_48_a, avg_square_order_49_a, avg_square_order_50_a
  FROM avg_u_ph_interharmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_u_ph_interharmonics", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_u_ph_interharmonics");

				mainWnd_.CMs_[(int)AvgPages.U_PH_INTERHARM] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.U_PH_INTERHARM].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.U_PH_INTERHARM].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperInterHarmUph::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			// dummy
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.U_PH_INTERHARM] = null;
		}
	}

	internal class AVGDataGridWrapperInterHarmUlin : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperInterHarmUlin(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_v = (settings_.VoltageRatio == 1) ?
					rm.GetString("column_header_units_v") :
					rm.GetString("column_header_units_kv");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_u_lin_interharmonics";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				DataGridColumnHeaderFormula cs_s_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, "Cig0_AB");
				cs_s_a.HeaderText = "";
				cs_s_a.MappingName = "avg_square_ab";
				cs_s_a.Format = DataColumnsFormat.FloatFormat;
				cs_s_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_s_a);

				for (int iCol = 1; iCol <= 50; ++iCol)
				{
					DataGridColumnHeaderFormula cs_a =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
							string.Format("Cig{0}_AB", iCol));
					cs_a.HeaderText = "";
					cs_a.MappingName = string.Format("avg_square_order_{0}_ab", iCol);
					cs_a.Format = DataColumnsFormat.FloatFormat;
					cs_a.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_a);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_s_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, "Cig0_BC");
					cs_s_b.HeaderText = "";
					cs_s_b.MappingName = "avg_square_bc";
					cs_s_b.Format = DataColumnsFormat.FloatFormat;
					cs_s_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_b);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Cig{0}_BC", iCol));
						cs_b.HeaderText = "";
						cs_b.MappingName = string.Format("avg_square_order_{0}_bc", iCol);
						cs_b.Format = DataColumnsFormat.FloatFormat;
						cs_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_b);
					}

					DataGridColumnHeaderFormula cs_s_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "Cig0_CA");
					cs_s_c.HeaderText = "";
					cs_s_c.MappingName = "avg_square_ca";
					cs_s_c.Format = DataColumnsFormat.FloatFormat;
					cs_s_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_s_c);

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
								string.Format("Cig{0}_CA", iCol));
						cs_c.HeaderText = "";
						cs_c.MappingName = string.Format("avg_square_order_{0}_ca", iCol);
						cs_c.Format = DataColumnsFormat.FloatFormat;
						cs_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_c);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_v);
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmUlin::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			// dummy
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			try
			{
				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				string query = String.Format("SELECT dt_start, case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, p.* FROM avg_u_lin_interharmonics p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_u_lin_interharmonics", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_u_lin_interharmonics");

				mainWnd_.CMs_[(int)AvgPages.U_LIN_INTERHARM] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.U_LIN_INTERHARM].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.U_LIN_INTERHARM].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperInterHarmUlin::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			// dummy
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.U_LIN_INTERHARM] = null;
		}
	}

	internal class AVGDataGridWrapperHarmPower : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperHarmPower(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_w = "_";
				unit_w += (settings_.PowerRatio == 1) ?
					 rm.GetString("column_header_units_w") :
					 rm.GetString("column_header_units_kw");
				string unit_var = "_";
				unit_var += (settings_.PowerRatio == 1) ?
					 rm.GetString("column_header_units_var") :
					 rm.GetString("column_header_units_kvar");
				string unit_grad = "_" + rm.GetString("column_header_units_grad"); 

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "avg_harm_power";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);
				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "dt_start";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				// if marked
				DataGridColumnHeaderFormula cs_marked =
					new DataGridColumnHeaderFormula(DataGridColors.ColorCommon);
				cs_marked.HeaderText = rm.GetString("name_columnheaders_avg_marked");
				cs_marked.MappingName = "if_record_marked";
				cs_marked.Width = DataColumnsWidth.SmallWidth;
				ts.GridColumnStyles.Add(cs_marked);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, 
								string.Format("P_Σ({0})", iCol) + unit_w);
						cs_p_summ.HeaderText = "";
						cs_p_summ.MappingName = string.Format("pharm_p_sum_{0}", iCol);
						cs_p_summ.Format = DataColumnsFormat.FloatFormat;
						cs_p_summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_summ);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
								string.Format("Q_Σ({0})", iCol) + unit_var);
						cs_p_summ.HeaderText = "";
						cs_p_summ.MappingName = string.Format("pharm_q_sum_{0}", iCol);
						cs_p_summ.Format = DataColumnsFormat.FloatFormat;
						cs_p_summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_summ);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_summ =
							new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
								string.Format("<_Σ({0})", iCol) + unit_grad);
						cs_p_summ.HeaderText = "";
						cs_p_summ.MappingName = string.Format("pharm_angle_sum_{0}", iCol);
						cs_p_summ.Format = DataColumnsFormat.FloatFormat;
						cs_p_summ.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_summ);
					}
				}

				if (connectScheme_ != ConnectScheme.Ph3W3 && connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								string.Format("P_A({0})", iCol) + unit_w);
						cs_p_a.HeaderText = "";
						cs_p_a.MappingName = string.Format("pharm_p_a_{0}", iCol);
						cs_p_a.Format = DataColumnsFormat.FloatFormat;
						cs_p_a.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_a);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								string.Format("Q_A({0})", iCol) + unit_var);
						cs_p_a.HeaderText = "";
						cs_p_a.MappingName = string.Format("pharm_q_a_{0}", iCol);
						cs_p_a.Format = DataColumnsFormat.FloatFormat;
						cs_p_a.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_a);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_a =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								string.Format("<_A({0})", iCol) + unit_grad);
						cs_p_a.HeaderText = "";
						cs_p_a.MappingName = string.Format("pharm_angle_a_{0}", iCol);
						cs_p_a.Format = DataColumnsFormat.FloatFormat;
						cs_p_a.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_a);
					}

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						for (int iCol = 1; iCol <= 50; ++iCol)
						{
							DataGridColumnHeaderFormula cs_p_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
									string.Format("P_B({0})", iCol) + unit_w);
							cs_p_b.HeaderText = "";
							cs_p_b.MappingName = string.Format("pharm_p_b_{0}", iCol);
							cs_p_b.Format = DataColumnsFormat.FloatFormat;
							cs_p_b.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_p_b);
						}

						for (int iCol = 1; iCol <= 50; ++iCol)
						{
							DataGridColumnHeaderFormula cs_p_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
									string.Format("Q_B({0})", iCol) + unit_var);
							cs_p_b.HeaderText = "";
							cs_p_b.MappingName = string.Format("pharm_q_b_{0}", iCol);
							cs_p_b.Format = DataColumnsFormat.FloatFormat;
							cs_p_b.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_p_b);
						}

						for (int iCol = 1; iCol <= 50; ++iCol)
						{
							DataGridColumnHeaderFormula cs_p_b =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
									string.Format("<_B({0})", iCol) + unit_grad);
							cs_p_b.HeaderText = "";
							cs_p_b.MappingName = string.Format("pharm_angle_b_{0}", iCol);
							cs_p_b.Format = DataColumnsFormat.FloatFormat;
							cs_p_b.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_p_b);
						}

						for (int iCol = 1; iCol <= 50; ++iCol)
						{
							DataGridColumnHeaderFormula cs_p_c =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
									string.Format("P_C({0})", iCol) + unit_w);
							cs_p_c.HeaderText = "";
							cs_p_c.MappingName = string.Format("pharm_p_c_{0}", iCol);
							cs_p_c.Format = DataColumnsFormat.FloatFormat;
							cs_p_c.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_p_c);
						}

						for (int iCol = 1; iCol <= 50; ++iCol)
						{
							DataGridColumnHeaderFormula cs_p_c =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
									string.Format("Q_C({0})", iCol) + unit_var);
							cs_p_c.HeaderText = "";
							cs_p_c.MappingName = string.Format("pharm_q_c_{0}", iCol);
							cs_p_c.Format = DataColumnsFormat.FloatFormat;
							cs_p_c.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_p_c);
						}

						for (int iCol = 1; iCol <= 50; ++iCol)
						{
							DataGridColumnHeaderFormula cs_p_c =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
									string.Format("<_C({0})", iCol) + unit_grad);
							cs_p_c.HeaderText = "";
							cs_p_c.MappingName = string.Format("pharm_angle_c_{0}", iCol);
							cs_p_c.Format = DataColumnsFormat.FloatFormat;
							cs_p_c.Width = DataColumnsWidth.CommonWidth;
							ts.GridColumnStyles.Add(cs_p_c);
						}
					}
				}
				else   // 3ph3w
				{
					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								string.Format("P_1({0})", iCol) + unit_w);
						cs_p_b.HeaderText = "";
						cs_p_b.MappingName = string.Format("pharm_p_1_{0}", iCol);
						cs_p_b.Format = DataColumnsFormat.FloatFormat;
						cs_p_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_b);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								string.Format("Q_1({0})", iCol) + unit_var);
						cs_p_b.HeaderText = "";
						cs_p_b.MappingName = string.Format("pharm_q_1_{0}", iCol);
						cs_p_b.Format = DataColumnsFormat.FloatFormat;
						cs_p_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_b);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_b =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								string.Format("<_1({0})", iCol) + unit_grad);
						cs_p_b.HeaderText = "";
						cs_p_b.MappingName = string.Format("pharm_angle_1_{0}", iCol);
						cs_p_b.Format = DataColumnsFormat.FloatFormat;
						cs_p_b.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_b);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("P_2({0})", iCol) + unit_w);
						cs_p_c.HeaderText = "";
						cs_p_c.MappingName = string.Format("pharm_p_2_{0}", iCol);
						cs_p_c.Format = DataColumnsFormat.FloatFormat;
						cs_p_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_c);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("Q_2({0})", iCol) + unit_var);
						cs_p_c.HeaderText = "";
						cs_p_c.MappingName = string.Format("pharm_q_2_{0}", iCol);
						cs_p_c.Format = DataColumnsFormat.FloatFormat;
						cs_p_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_c);
					}

					for (int iCol = 1; iCol <= 50; ++iCol)
					{
						DataGridColumnHeaderFormula cs_p_c =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
								string.Format("<_2({0})", iCol) + unit_grad);
						cs_p_c.HeaderText = "";
						cs_p_c.MappingName = string.Format("pharm_angle_2_{0}", iCol);
						cs_p_c.Format = DataColumnsFormat.FloatFormat;
						cs_p_c.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_p_c);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				//for (int i = 2; i < ts.GridColumnStyles.Count; i++)
				//{
				//    (ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_w);
				//}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmPower::init(): ");
				throw;
			}
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				string unit_w = (settings_.PowerRatio == 1) ?
					rm.GetString("column_header_units_w") :
					rm.GetString("column_header_units_kw");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "period_avg_params_6a";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event = new DataGridColumnHeaderFormula(
					DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				cs_event.Format = "G";
				ts.GridColumnStyles.Add(cs_event);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					DataGridColumnHeaderFormula cs_p_summ =
						new DataGridColumnHeaderFormula(DataGridColors.ColorCommon, "P_Σ");
					cs_p_summ.HeaderText = "";
					cs_p_summ.MappingName = "p_sum";
					cs_p_summ.Format = DataColumnsFormat.FloatFormat;
					cs_p_summ.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_p_summ);
				}

				string cap = "P_A";
				if (connectScheme_ == ConnectScheme.Ph3W3 ||
					connectScheme_ == ConnectScheme.Ph3W3_B_calc)
					cap = "P_AB";
				DataGridColumnHeaderFormula cs_p_a =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap);
				cs_p_a.HeaderText = "";
				cs_p_a.MappingName = "p_a_1";
				cs_p_a.Format = DataColumnsFormat.FloatFormat;
				cs_p_a.Width = DataColumnsWidth.CommonWidth;
				ts.GridColumnStyles.Add(cs_p_a);

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					cap = "P_B";
					if (connectScheme_ == ConnectScheme.Ph3W3 ||
						connectScheme_ == ConnectScheme.Ph3W3_B_calc)
						cap = "P_CB";
					DataGridColumnHeaderFormula cs_p_b =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, cap);
					cs_p_b.HeaderText = "";
					cs_p_b.MappingName = "p_b_2";
					cs_p_b.Format = DataColumnsFormat.FloatFormat;
					cs_p_b.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_p_b);
				}

				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					DataGridColumnHeaderFormula cs_p_c =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, "P_C");
					cs_p_c.HeaderText = "";
					cs_p_c.MappingName = "p_c";
					cs_p_c.Format = DataColumnsFormat.FloatFormat;
					cs_p_c.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_p_c);

					// P_Σ 1..40
					//for (int i = 1; i <= 40; i++)
					//{
					//    DataGridColumnHeaderFormula cs_k =
					//        new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
					//        "P_Σ(" + i.ToString() + ")");
					//    cs_k.HeaderText = " ";
					//    cs_k.MappingName = "p_sum_" + i.ToString();
					//    cs_k.Format = DataColumnsFormat.FloatFormat;
					//    cs_k.Width = DataColumnsWidth.CommonWidth;
					//    ts.GridColumnStyles.Add(cs_k);
					//}
				}

				// для Эм33Т для 4-проводной лежит Ра, а для 3-проводной там же лежит Рсум
				// P_A(1) 1..40
				for (int i = 1; i <= 40; i++)
				{
					if (connectScheme_ == ConnectScheme.Ph3W3 ||
						connectScheme_ == ConnectScheme.Ph3W3_B_calc)
						cap = "P_Σ" + i.ToString() + ")";
					else
						cap = "P_A(" + i.ToString() + ")";
					DataGridColumnHeaderFormula cs_k =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap);
					cs_k.HeaderText = "";
					cs_k.MappingName = "p_a_1_" + i.ToString();
					cs_k.Format = DataColumnsFormat.FloatFormat;
					cs_k.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_k);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// P_B(2) 1..40
					for (int i = 1; i <= 40; i++)
					{
						if (connectScheme_ == ConnectScheme.Ph3W3 ||
							connectScheme_ == ConnectScheme.Ph3W3_B_calc)
							cap = "P_1(" + i.ToString() + ")";
						else
							cap = "P_B(" + i.ToString() + ")";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, cap);
						cs_k.HeaderText = "";
						cs_k.MappingName = "p_b_2_" + i.ToString();
						cs_k.Format = DataColumnsFormat.FloatFormat;
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}

					// P_C 1..40
					for (int i = 1; i < 41; i++)
					{
						if (connectScheme_ == ConnectScheme.Ph3W3 ||
							connectScheme_ == ConnectScheme.Ph3W3_B_calc)
							cap = "P_2(" + i.ToString() + ")";
						else
							cap = "P_C(" + i.ToString() + ")";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, cap);
						cs_k.HeaderText = "";
						cs_k.MappingName = "p_c_" + i.ToString();
						cs_k.Format = DataColumnsFormat.FloatFormat;
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_w);
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmPower::init(): ");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR, bool bShownInPercent)
		{
			//NpgsqlDataReader dataReader = null;
			//NpgsqlCommand sqlCommand = null;

			try
			{
				string query = string.Empty;

				string str_yes = "yes", str_no = "no";
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
				{
					str_yes = "да";
					str_no = "нет";
				}

				#region Query

				if (connectScheme_ == ConnectScheme.Ph3W4 || connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
       pharm_p_a_1, pharm_p_a_2, pharm_p_a_3, 
       pharm_p_a_4, pharm_p_a_5, pharm_p_a_6, pharm_p_a_7, pharm_p_a_8, 
       pharm_p_a_9, pharm_p_a_10, pharm_p_a_11, pharm_p_a_12, pharm_p_a_13, 
       pharm_p_a_14, pharm_p_a_15, pharm_p_a_16, pharm_p_a_17, pharm_p_a_18, 
       pharm_p_a_19, pharm_p_a_20, pharm_p_a_21, pharm_p_a_22, pharm_p_a_23, 
       pharm_p_a_24, pharm_p_a_25, pharm_p_a_26, pharm_p_a_27, pharm_p_a_28, 
       pharm_p_a_29, pharm_p_a_30, pharm_p_a_31, pharm_p_a_32, pharm_p_a_33, 
       pharm_p_a_34, pharm_p_a_35, pharm_p_a_36, pharm_p_a_37, pharm_p_a_38, 
       pharm_p_a_39, pharm_p_a_40, pharm_p_a_41, pharm_p_a_42, pharm_p_a_43, 
       pharm_p_a_44, pharm_p_a_45, pharm_p_a_46, pharm_p_a_47, pharm_p_a_48, 
       pharm_p_a_49, pharm_p_a_50, pharm_q_a_1, pharm_q_a_2, pharm_q_a_3, 
       pharm_q_a_4, pharm_q_a_5, pharm_q_a_6, pharm_q_a_7, pharm_q_a_8, 
       pharm_q_a_9, pharm_q_a_10, pharm_q_a_11, pharm_q_a_12, pharm_q_a_13, 
       pharm_q_a_14, pharm_q_a_15, pharm_q_a_16, pharm_q_a_17, pharm_q_a_18, 
       pharm_q_a_19, pharm_q_a_20, pharm_q_a_21, pharm_q_a_22, pharm_q_a_23, 
       pharm_q_a_24, pharm_q_a_25, pharm_q_a_26, pharm_q_a_27, pharm_q_a_28, 
       pharm_q_a_29, pharm_q_a_30, pharm_q_a_31, pharm_q_a_32, pharm_q_a_33, 
       pharm_q_a_34, pharm_q_a_35, pharm_q_a_36, pharm_q_a_37, pharm_q_a_38, 
       pharm_q_a_39, pharm_q_a_40, pharm_q_a_41, pharm_q_a_42, pharm_q_a_43, 
       pharm_q_a_44, pharm_q_a_45, pharm_q_a_46, pharm_q_a_47, pharm_q_a_48, 
       pharm_q_a_49, pharm_q_a_50, pharm_angle_a_1, pharm_angle_a_2, 
       pharm_angle_a_3, pharm_angle_a_4, pharm_angle_a_5, pharm_angle_a_6, 
       pharm_angle_a_7, pharm_angle_a_8, pharm_angle_a_9, pharm_angle_a_10, 
       pharm_angle_a_11, pharm_angle_a_12, pharm_angle_a_13, pharm_angle_a_14, 
       pharm_angle_a_15, pharm_angle_a_16, pharm_angle_a_17, pharm_angle_a_18, 
       pharm_angle_a_19, pharm_angle_a_20, pharm_angle_a_21, pharm_angle_a_22, 
       pharm_angle_a_23, pharm_angle_a_24, pharm_angle_a_25, pharm_angle_a_26, 
       pharm_angle_a_27, pharm_angle_a_28, pharm_angle_a_29, pharm_angle_a_30, 
       pharm_angle_a_31, pharm_angle_a_32, pharm_angle_a_33, pharm_angle_a_34, 
       pharm_angle_a_35, pharm_angle_a_36, pharm_angle_a_37, pharm_angle_a_38, 
       pharm_angle_a_39, pharm_angle_a_40, pharm_angle_a_41, pharm_angle_a_42, 
       pharm_angle_a_43, pharm_angle_a_44, pharm_angle_a_45, pharm_angle_a_46, 
       pharm_angle_a_47, pharm_angle_a_48, pharm_angle_a_49, pharm_angle_a_50, 
       pharm_p_b_1, pharm_p_b_2, pharm_p_b_3, pharm_p_b_4, pharm_p_b_5, 
       pharm_p_b_6, pharm_p_b_7, pharm_p_b_8, pharm_p_b_9, pharm_p_b_10, 
       pharm_p_b_11, pharm_p_b_12, pharm_p_b_13, pharm_p_b_14, pharm_p_b_15, 
       pharm_p_b_16, pharm_p_b_17, pharm_p_b_18, pharm_p_b_19, pharm_p_b_20, 
       pharm_p_b_21, pharm_p_b_22, pharm_p_b_23, pharm_p_b_24, pharm_p_b_25, 
       pharm_p_b_26, pharm_p_b_27, pharm_p_b_28, pharm_p_b_29, pharm_p_b_30, 
       pharm_p_b_31, pharm_p_b_32, pharm_p_b_33, pharm_p_b_34, pharm_p_b_35, 
       pharm_p_b_36, pharm_p_b_37, pharm_p_b_38, pharm_p_b_39, pharm_p_b_40, 
       pharm_p_b_41, pharm_p_b_42, pharm_p_b_43, pharm_p_b_44, pharm_p_b_45, 
       pharm_p_b_46, pharm_p_b_47, pharm_p_b_48, pharm_p_b_49, pharm_p_b_50, 
       pharm_q_b_1, pharm_q_b_2, pharm_q_b_3, pharm_q_b_4, pharm_q_b_5, 
       pharm_q_b_6, pharm_q_b_7, pharm_q_b_8, pharm_q_b_9, pharm_q_b_10, 
       pharm_q_b_11, pharm_q_b_12, pharm_q_b_13, pharm_q_b_14, pharm_q_b_15, 
       pharm_q_b_16, pharm_q_b_17, pharm_q_b_18, pharm_q_b_19, pharm_q_b_20, 
       pharm_q_b_21, pharm_q_b_22, pharm_q_b_23, pharm_q_b_24, pharm_q_b_25, 
       pharm_q_b_26, pharm_q_b_27, pharm_q_b_28, pharm_q_b_29, pharm_q_b_30, 
       pharm_q_b_31, pharm_q_b_32, pharm_q_b_33, pharm_q_b_34, pharm_q_b_35, 
       pharm_q_b_36, pharm_q_b_37, pharm_q_b_38, pharm_q_b_39, pharm_q_b_40, 
       pharm_q_b_41, pharm_q_b_42, pharm_q_b_43, pharm_q_b_44, pharm_q_b_45, 
       pharm_q_b_46, pharm_q_b_47, pharm_q_b_48, pharm_q_b_49, pharm_q_b_50, 
       pharm_angle_b_1, pharm_angle_b_2, pharm_angle_b_3, pharm_angle_b_4, 
       pharm_angle_b_5, pharm_angle_b_6, pharm_angle_b_7, pharm_angle_b_8, 
       pharm_angle_b_9, pharm_angle_b_10, pharm_angle_b_11, pharm_angle_b_12, 
       pharm_angle_b_13, pharm_angle_b_14, pharm_angle_b_15, pharm_angle_b_16, 
       pharm_angle_b_17, pharm_angle_b_18, pharm_angle_b_19, pharm_angle_b_20, 
       pharm_angle_b_21, pharm_angle_b_22, pharm_angle_b_23, pharm_angle_b_24, 
       pharm_angle_b_25, pharm_angle_b_26, pharm_angle_b_27, pharm_angle_b_28, 
       pharm_angle_b_29, pharm_angle_b_30, pharm_angle_b_31, pharm_angle_b_32, 
       pharm_angle_b_33, pharm_angle_b_34, pharm_angle_b_35, pharm_angle_b_36, 
       pharm_angle_b_37, pharm_angle_b_38, pharm_angle_b_39, pharm_angle_b_40, 
       pharm_angle_b_41, pharm_angle_b_42, pharm_angle_b_43, pharm_angle_b_44, 
       pharm_angle_b_45, pharm_angle_b_46, pharm_angle_b_47, pharm_angle_b_48, 
       pharm_angle_b_49, pharm_angle_b_50, pharm_p_c_1, pharm_p_c_2, 
       pharm_p_c_3, pharm_p_c_4, pharm_p_c_5, pharm_p_c_6, pharm_p_c_7, 
       pharm_p_c_8, pharm_p_c_9, pharm_p_c_10, pharm_p_c_11, pharm_p_c_12, 
       pharm_p_c_13, pharm_p_c_14, pharm_p_c_15, pharm_p_c_16, pharm_p_c_17, 
       pharm_p_c_18, pharm_p_c_19, pharm_p_c_20, pharm_p_c_21, pharm_p_c_22, 
       pharm_p_c_23, pharm_p_c_24, pharm_p_c_25, pharm_p_c_26, pharm_p_c_27, 
       pharm_p_c_28, pharm_p_c_29, pharm_p_c_30, pharm_p_c_31, pharm_p_c_32, 
       pharm_p_c_33, pharm_p_c_34, pharm_p_c_35, pharm_p_c_36, pharm_p_c_37, 
       pharm_p_c_38, pharm_p_c_39, pharm_p_c_40, pharm_p_c_41, pharm_p_c_42, 
       pharm_p_c_43, pharm_p_c_44, pharm_p_c_45, pharm_p_c_46, pharm_p_c_47, 
       pharm_p_c_48, pharm_p_c_49, pharm_p_c_50, pharm_q_c_1, pharm_q_c_2, 
       pharm_q_c_3, pharm_q_c_4, pharm_q_c_5, pharm_q_c_6, pharm_q_c_7, 
       pharm_q_c_8, pharm_q_c_9, pharm_q_c_10, pharm_q_c_11, pharm_q_c_12, 
       pharm_q_c_13, pharm_q_c_14, pharm_q_c_15, pharm_q_c_16, pharm_q_c_17, 
       pharm_q_c_18, pharm_q_c_19, pharm_q_c_20, pharm_q_c_21, pharm_q_c_22, 
       pharm_q_c_23, pharm_q_c_24, pharm_q_c_25, pharm_q_c_26, pharm_q_c_27, 
       pharm_q_c_28, pharm_q_c_29, pharm_q_c_30, pharm_q_c_31, pharm_q_c_32, 
       pharm_q_c_33, pharm_q_c_34, pharm_q_c_35, pharm_q_c_36, pharm_q_c_37, 
       pharm_q_c_38, pharm_q_c_39, pharm_q_c_40, pharm_q_c_41, pharm_q_c_42, 
       pharm_q_c_43, pharm_q_c_44, pharm_q_c_45, pharm_q_c_46, pharm_q_c_47, 
       pharm_q_c_48, pharm_q_c_49, pharm_q_c_50, pharm_angle_c_1, pharm_angle_c_2, 
       pharm_angle_c_3, pharm_angle_c_4, pharm_angle_c_5, pharm_angle_c_6, 
       pharm_angle_c_7, pharm_angle_c_8, pharm_angle_c_9, pharm_angle_c_10, 
       pharm_angle_c_11, pharm_angle_c_12, pharm_angle_c_13, pharm_angle_c_14, 
       pharm_angle_c_15, pharm_angle_c_16, pharm_angle_c_17, pharm_angle_c_18, 
       pharm_angle_c_19, pharm_angle_c_20, pharm_angle_c_21, pharm_angle_c_22, 
       pharm_angle_c_23, pharm_angle_c_24, pharm_angle_c_25, pharm_angle_c_26, 
       pharm_angle_c_27, pharm_angle_c_28, pharm_angle_c_29, pharm_angle_c_30, 
       pharm_angle_c_31, pharm_angle_c_32, pharm_angle_c_33, pharm_angle_c_34, 
       pharm_angle_c_35, pharm_angle_c_36, pharm_angle_c_37, pharm_angle_c_38, 
       pharm_angle_c_39, pharm_angle_c_40, pharm_angle_c_41, pharm_angle_c_42, 
       pharm_angle_c_43, pharm_angle_c_44, pharm_angle_c_45, pharm_angle_c_46, 
       pharm_angle_c_47, pharm_angle_c_48, pharm_angle_c_49, pharm_angle_c_50,  
       pharm_p_sum_1, pharm_p_sum_2, pharm_p_sum_3, pharm_p_sum_4, pharm_p_sum_5, 
       pharm_p_sum_6, pharm_p_sum_7, pharm_p_sum_8, pharm_p_sum_9, pharm_p_sum_10, 
       pharm_p_sum_11, pharm_p_sum_12, pharm_p_sum_13, pharm_p_sum_14, 
       pharm_p_sum_15, pharm_p_sum_16, pharm_p_sum_17, pharm_p_sum_18, 
       pharm_p_sum_19, pharm_p_sum_20, pharm_p_sum_21, pharm_p_sum_22, 
       pharm_p_sum_23, pharm_p_sum_24, pharm_p_sum_25, pharm_p_sum_26, 
       pharm_p_sum_27, pharm_p_sum_28, pharm_p_sum_29, pharm_p_sum_30, 
       pharm_p_sum_31, pharm_p_sum_32, pharm_p_sum_33, pharm_p_sum_34, 
       pharm_p_sum_35, pharm_p_sum_36, pharm_p_sum_37, pharm_p_sum_38, 
       pharm_p_sum_39, pharm_p_sum_40, pharm_p_sum_41, pharm_p_sum_42, 
       pharm_p_sum_43, pharm_p_sum_44, pharm_p_sum_45, pharm_p_sum_46, 
       pharm_p_sum_47, pharm_p_sum_48, pharm_p_sum_49, pharm_p_sum_50, 
       pharm_q_sum_1, pharm_q_sum_2, pharm_q_sum_3, pharm_q_sum_4, pharm_q_sum_5, 
       pharm_q_sum_6, pharm_q_sum_7, pharm_q_sum_8, pharm_q_sum_9, pharm_q_sum_10, 
       pharm_q_sum_11, pharm_q_sum_12, pharm_q_sum_13, pharm_q_sum_14, 
       pharm_q_sum_15, pharm_q_sum_16, pharm_q_sum_17, pharm_q_sum_18, 
       pharm_q_sum_19, pharm_q_sum_20, pharm_q_sum_21, pharm_q_sum_22, 
       pharm_q_sum_23, pharm_q_sum_24, pharm_q_sum_25, pharm_q_sum_26, 
       pharm_q_sum_27, pharm_q_sum_28, pharm_q_sum_29, pharm_q_sum_30, 
       pharm_q_sum_31, pharm_q_sum_32, pharm_q_sum_33, pharm_q_sum_34, 
       pharm_q_sum_35, pharm_q_sum_36, pharm_q_sum_37, pharm_q_sum_38, 
       pharm_q_sum_39, pharm_q_sum_40, pharm_q_sum_41, pharm_q_sum_42, 
       pharm_q_sum_43, pharm_q_sum_44, pharm_q_sum_45, pharm_q_sum_46, 
       pharm_q_sum_47, pharm_q_sum_48, pharm_q_sum_49, pharm_q_sum_50, 
       pharm_angle_sum_1, pharm_angle_sum_2, pharm_angle_sum_3, pharm_angle_sum_4, 
       pharm_angle_sum_5, pharm_angle_sum_6, pharm_angle_sum_7, pharm_angle_sum_8, 
       pharm_angle_sum_9, pharm_angle_sum_10, pharm_angle_sum_11, pharm_angle_sum_12, 
       pharm_angle_sum_13, pharm_angle_sum_14, pharm_angle_sum_15, pharm_angle_sum_16, 
       pharm_angle_sum_17, pharm_angle_sum_18, pharm_angle_sum_19, pharm_angle_sum_20, 
       pharm_angle_sum_21, pharm_angle_sum_22, pharm_angle_sum_23, pharm_angle_sum_24, 
       pharm_angle_sum_25, pharm_angle_sum_26, pharm_angle_sum_27, pharm_angle_sum_28, 
       pharm_angle_sum_29, pharm_angle_sum_30, pharm_angle_sum_31, pharm_angle_sum_32, 
       pharm_angle_sum_33, pharm_angle_sum_34, pharm_angle_sum_35, pharm_angle_sum_36, 
       pharm_angle_sum_37, pharm_angle_sum_38, pharm_angle_sum_39, pharm_angle_sum_40, 
       pharm_angle_sum_41, pharm_angle_sum_42, pharm_angle_sum_43, pharm_angle_sum_44, 
       pharm_angle_sum_45, pharm_angle_sum_46, pharm_angle_sum_47, pharm_angle_sum_48, 
       pharm_angle_sum_49, pharm_angle_sum_50
  FROM avg_harm_power p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}
				else if (connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
				{
					query = string.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
       pharm_p_1_1, pharm_p_1_2, pharm_p_1_3, pharm_p_1_4, pharm_p_1_5, 
       pharm_p_1_6, pharm_p_1_7, pharm_p_1_8, pharm_p_1_9, pharm_p_1_10, 
       pharm_p_1_11, pharm_p_1_12, pharm_p_1_13, pharm_p_1_14, pharm_p_1_15, 
       pharm_p_1_16, pharm_p_1_17, pharm_p_1_18, pharm_p_1_19, pharm_p_1_20, 
       pharm_p_1_21, pharm_p_1_22, pharm_p_1_23, pharm_p_1_24, pharm_p_1_25, 
       pharm_p_1_26, pharm_p_1_27, pharm_p_1_28, pharm_p_1_29, pharm_p_1_30, 
       pharm_p_1_31, pharm_p_1_32, pharm_p_1_33, pharm_p_1_34, pharm_p_1_35, 
       pharm_p_1_36, pharm_p_1_37, pharm_p_1_38, pharm_p_1_39, pharm_p_1_40, 
       pharm_p_1_41, pharm_p_1_42, pharm_p_1_43, pharm_p_1_44, pharm_p_1_45, 
       pharm_p_1_46, pharm_p_1_47, pharm_p_1_48, pharm_p_1_49, pharm_p_1_50, 
       pharm_q_1_1, pharm_q_1_2, pharm_q_1_3, pharm_q_1_4, pharm_q_1_5, 
       pharm_q_1_6, pharm_q_1_7, pharm_q_1_8, pharm_q_1_9, pharm_q_1_10, 
       pharm_q_1_11, pharm_q_1_12, pharm_q_1_13, pharm_q_1_14, pharm_q_1_15, 
       pharm_q_1_16, pharm_q_1_17, pharm_q_1_18, pharm_q_1_19, pharm_q_1_20, 
       pharm_q_1_21, pharm_q_1_22, pharm_q_1_23, pharm_q_1_24, pharm_q_1_25, 
       pharm_q_1_26, pharm_q_1_27, pharm_q_1_28, pharm_q_1_29, pharm_q_1_30, 
       pharm_q_1_31, pharm_q_1_32, pharm_q_1_33, pharm_q_1_34, pharm_q_1_35, 
       pharm_q_1_36, pharm_q_1_37, pharm_q_1_38, pharm_q_1_39, pharm_q_1_40, 
       pharm_q_1_41, pharm_q_1_42, pharm_q_1_43, pharm_q_1_44, pharm_q_1_45, 
       pharm_q_1_46, pharm_q_1_47, pharm_q_1_48, pharm_q_1_49, pharm_q_1_50, 
       pharm_angle_1_1, pharm_angle_1_2, pharm_angle_1_3, pharm_angle_1_4, 
       pharm_angle_1_5, pharm_angle_1_6, pharm_angle_1_7, pharm_angle_1_8, 
       pharm_angle_1_9, pharm_angle_1_10, pharm_angle_1_11, pharm_angle_1_12, 
       pharm_angle_1_13, pharm_angle_1_14, pharm_angle_1_15, pharm_angle_1_16, 
       pharm_angle_1_17, pharm_angle_1_18, pharm_angle_1_19, pharm_angle_1_20, 
       pharm_angle_1_21, pharm_angle_1_22, pharm_angle_1_23, pharm_angle_1_24, 
       pharm_angle_1_25, pharm_angle_1_26, pharm_angle_1_27, pharm_angle_1_28, 
       pharm_angle_1_29, pharm_angle_1_30, pharm_angle_1_31, pharm_angle_1_32, 
       pharm_angle_1_33, pharm_angle_1_34, pharm_angle_1_35, pharm_angle_1_36, 
       pharm_angle_1_37, pharm_angle_1_38, pharm_angle_1_39, pharm_angle_1_40, 
       pharm_angle_1_41, pharm_angle_1_42, pharm_angle_1_43, pharm_angle_1_44, 
       pharm_angle_1_45, pharm_angle_1_46, pharm_angle_1_47, pharm_angle_1_48, 
       pharm_angle_1_49, pharm_angle_1_50, pharm_p_2_1, pharm_p_2_2, 
       pharm_p_2_3, pharm_p_2_4, pharm_p_2_5, pharm_p_2_6, pharm_p_2_7, 
       pharm_p_2_8, pharm_p_2_9, pharm_p_2_10, pharm_p_2_11, pharm_p_2_12, 
       pharm_p_2_13, pharm_p_2_14, pharm_p_2_15, pharm_p_2_16, pharm_p_2_17, 
       pharm_p_2_18, pharm_p_2_19, pharm_p_2_20, pharm_p_2_21, pharm_p_2_22, 
       pharm_p_2_23, pharm_p_2_24, pharm_p_2_25, pharm_p_2_26, pharm_p_2_27, 
       pharm_p_2_28, pharm_p_2_29, pharm_p_2_30, pharm_p_2_31, pharm_p_2_32, 
       pharm_p_2_33, pharm_p_2_34, pharm_p_2_35, pharm_p_2_36, pharm_p_2_37, 
       pharm_p_2_38, pharm_p_2_39, pharm_p_2_40, pharm_p_2_41, pharm_p_2_42, 
       pharm_p_2_43, pharm_p_2_44, pharm_p_2_45, pharm_p_2_46, pharm_p_2_47, 
       pharm_p_2_48, pharm_p_2_49, pharm_p_2_50, pharm_q_2_1, pharm_q_2_2, 
       pharm_q_2_3, pharm_q_2_4, pharm_q_2_5, pharm_q_2_6, pharm_q_2_7, 
       pharm_q_2_8, pharm_q_2_9, pharm_q_2_10, pharm_q_2_11, pharm_q_2_12, 
       pharm_q_2_13, pharm_q_2_14, pharm_q_2_15, pharm_q_2_16, pharm_q_2_17, 
       pharm_q_2_18, pharm_q_2_19, pharm_q_2_20, pharm_q_2_21, pharm_q_2_22, 
       pharm_q_2_23, pharm_q_2_24, pharm_q_2_25, pharm_q_2_26, pharm_q_2_27, 
       pharm_q_2_28, pharm_q_2_29, pharm_q_2_30, pharm_q_2_31, pharm_q_2_32, 
       pharm_q_2_33, pharm_q_2_34, pharm_q_2_35, pharm_q_2_36, pharm_q_2_37, 
       pharm_q_2_38, pharm_q_2_39, pharm_q_2_40, pharm_q_2_41, pharm_q_2_42, 
       pharm_q_2_43, pharm_q_2_44, pharm_q_2_45, pharm_q_2_46, pharm_q_2_47, 
       pharm_q_2_48, pharm_q_2_49, pharm_q_2_50, pharm_angle_2_1, pharm_angle_2_2, 
       pharm_angle_2_3, pharm_angle_2_4, pharm_angle_2_5, pharm_angle_2_6, 
       pharm_angle_2_7, pharm_angle_2_8, pharm_angle_2_9, pharm_angle_2_10, 
       pharm_angle_2_11, pharm_angle_2_12, pharm_angle_2_13, pharm_angle_2_14, 
       pharm_angle_2_15, pharm_angle_2_16, pharm_angle_2_17, pharm_angle_2_18, 
       pharm_angle_2_19, pharm_angle_2_20, pharm_angle_2_21, pharm_angle_2_22, 
       pharm_angle_2_23, pharm_angle_2_24, pharm_angle_2_25, pharm_angle_2_26, 
       pharm_angle_2_27, pharm_angle_2_28, pharm_angle_2_29, pharm_angle_2_30, 
       pharm_angle_2_31, pharm_angle_2_32, pharm_angle_2_33, pharm_angle_2_34, 
       pharm_angle_2_35, pharm_angle_2_36, pharm_angle_2_37, pharm_angle_2_38, 
       pharm_angle_2_39, pharm_angle_2_40, pharm_angle_2_41, pharm_angle_2_42, 
       pharm_angle_2_43, pharm_angle_2_44, pharm_angle_2_45, pharm_angle_2_46, 
       pharm_angle_2_47, pharm_angle_2_48, pharm_angle_2_49, pharm_angle_2_50, 
       pharm_p_sum_1, pharm_p_sum_2, pharm_p_sum_3, pharm_p_sum_4, pharm_p_sum_5, 
       pharm_p_sum_6, pharm_p_sum_7, pharm_p_sum_8, pharm_p_sum_9, pharm_p_sum_10, 
       pharm_p_sum_11, pharm_p_sum_12, pharm_p_sum_13, pharm_p_sum_14, 
       pharm_p_sum_15, pharm_p_sum_16, pharm_p_sum_17, pharm_p_sum_18, 
       pharm_p_sum_19, pharm_p_sum_20, pharm_p_sum_21, pharm_p_sum_22, 
       pharm_p_sum_23, pharm_p_sum_24, pharm_p_sum_25, pharm_p_sum_26, 
       pharm_p_sum_27, pharm_p_sum_28, pharm_p_sum_29, pharm_p_sum_30, 
       pharm_p_sum_31, pharm_p_sum_32, pharm_p_sum_33, pharm_p_sum_34, 
       pharm_p_sum_35, pharm_p_sum_36, pharm_p_sum_37, pharm_p_sum_38, 
       pharm_p_sum_39, pharm_p_sum_40, pharm_p_sum_41, pharm_p_sum_42, 
       pharm_p_sum_43, pharm_p_sum_44, pharm_p_sum_45, pharm_p_sum_46, 
       pharm_p_sum_47, pharm_p_sum_48, pharm_p_sum_49, pharm_p_sum_50, 
       pharm_q_sum_1, pharm_q_sum_2, pharm_q_sum_3, pharm_q_sum_4, pharm_q_sum_5, 
       pharm_q_sum_6, pharm_q_sum_7, pharm_q_sum_8, pharm_q_sum_9, pharm_q_sum_10, 
       pharm_q_sum_11, pharm_q_sum_12, pharm_q_sum_13, pharm_q_sum_14, 
       pharm_q_sum_15, pharm_q_sum_16, pharm_q_sum_17, pharm_q_sum_18, 
       pharm_q_sum_19, pharm_q_sum_20, pharm_q_sum_21, pharm_q_sum_22, 
       pharm_q_sum_23, pharm_q_sum_24, pharm_q_sum_25, pharm_q_sum_26, 
       pharm_q_sum_27, pharm_q_sum_28, pharm_q_sum_29, pharm_q_sum_30, 
       pharm_q_sum_31, pharm_q_sum_32, pharm_q_sum_33, pharm_q_sum_34, 
       pharm_q_sum_35, pharm_q_sum_36, pharm_q_sum_37, pharm_q_sum_38, 
       pharm_q_sum_39, pharm_q_sum_40, pharm_q_sum_41, pharm_q_sum_42, 
       pharm_q_sum_43, pharm_q_sum_44, pharm_q_sum_45, pharm_q_sum_46, 
       pharm_q_sum_47, pharm_q_sum_48, pharm_q_sum_49, pharm_q_sum_50, 
       pharm_angle_sum_1, pharm_angle_sum_2, pharm_angle_sum_3, pharm_angle_sum_4, 
       pharm_angle_sum_5, pharm_angle_sum_6, pharm_angle_sum_7, pharm_angle_sum_8, 
       pharm_angle_sum_9, pharm_angle_sum_10, pharm_angle_sum_11, pharm_angle_sum_12, 
       pharm_angle_sum_13, pharm_angle_sum_14, pharm_angle_sum_15, pharm_angle_sum_16, 
       pharm_angle_sum_17, pharm_angle_sum_18, pharm_angle_sum_19, pharm_angle_sum_20, 
       pharm_angle_sum_21, pharm_angle_sum_22, pharm_angle_sum_23, pharm_angle_sum_24, 
       pharm_angle_sum_25, pharm_angle_sum_26, pharm_angle_sum_27, pharm_angle_sum_28, 
       pharm_angle_sum_29, pharm_angle_sum_30, pharm_angle_sum_31, pharm_angle_sum_32, 
       pharm_angle_sum_33, pharm_angle_sum_34, pharm_angle_sum_35, pharm_angle_sum_36, 
       pharm_angle_sum_37, pharm_angle_sum_38, pharm_angle_sum_39, pharm_angle_sum_40, 
       pharm_angle_sum_41, pharm_angle_sum_42, pharm_angle_sum_43, pharm_angle_sum_44, 
       pharm_angle_sum_45, pharm_angle_sum_46, pharm_angle_sum_47, pharm_angle_sum_48, 
       pharm_angle_sum_49, pharm_angle_sum_50
  FROM avg_harm_power p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}
				else if (connectScheme_ == ConnectScheme.Ph1W2)
				{
					query = String.Format(@"SELECT p.datetime_id, p.record_id, dt_start, 
       case when if_record_marked > 0 then '{0}' else '{1}' end as if_record_marked, 
       pharm_p_a_1, pharm_p_a_2, pharm_p_a_3, 
       pharm_p_a_4, pharm_p_a_5, pharm_p_a_6, pharm_p_a_7, pharm_p_a_8, 
       pharm_p_a_9, pharm_p_a_10, pharm_p_a_11, pharm_p_a_12, pharm_p_a_13, 
       pharm_p_a_14, pharm_p_a_15, pharm_p_a_16, pharm_p_a_17, pharm_p_a_18, 
       pharm_p_a_19, pharm_p_a_20, pharm_p_a_21, pharm_p_a_22, pharm_p_a_23, 
       pharm_p_a_24, pharm_p_a_25, pharm_p_a_26, pharm_p_a_27, pharm_p_a_28, 
       pharm_p_a_29, pharm_p_a_30, pharm_p_a_31, pharm_p_a_32, pharm_p_a_33, 
       pharm_p_a_34, pharm_p_a_35, pharm_p_a_36, pharm_p_a_37, pharm_p_a_38, 
       pharm_p_a_39, pharm_p_a_40, pharm_p_a_41, pharm_p_a_42, pharm_p_a_43, 
       pharm_p_a_44, pharm_p_a_45, pharm_p_a_46, pharm_p_a_47, pharm_p_a_48, 
       pharm_p_a_49, pharm_p_a_50, pharm_q_a_1, pharm_q_a_2, pharm_q_a_3, 
       pharm_q_a_4, pharm_q_a_5, pharm_q_a_6, pharm_q_a_7, pharm_q_a_8, 
       pharm_q_a_9, pharm_q_a_10, pharm_q_a_11, pharm_q_a_12, pharm_q_a_13, 
       pharm_q_a_14, pharm_q_a_15, pharm_q_a_16, pharm_q_a_17, pharm_q_a_18, 
       pharm_q_a_19, pharm_q_a_20, pharm_q_a_21, pharm_q_a_22, pharm_q_a_23, 
       pharm_q_a_24, pharm_q_a_25, pharm_q_a_26, pharm_q_a_27, pharm_q_a_28, 
       pharm_q_a_29, pharm_q_a_30, pharm_q_a_31, pharm_q_a_32, pharm_q_a_33, 
       pharm_q_a_34, pharm_q_a_35, pharm_q_a_36, pharm_q_a_37, pharm_q_a_38, 
       pharm_q_a_39, pharm_q_a_40, pharm_q_a_41, pharm_q_a_42, pharm_q_a_43, 
       pharm_q_a_44, pharm_q_a_45, pharm_q_a_46, pharm_q_a_47, pharm_q_a_48, 
       pharm_q_a_49, pharm_q_a_50, pharm_angle_a_1, pharm_angle_a_2, 
       pharm_angle_a_3, pharm_angle_a_4, pharm_angle_a_5, pharm_angle_a_6, 
       pharm_angle_a_7, pharm_angle_a_8, pharm_angle_a_9, pharm_angle_a_10, 
       pharm_angle_a_11, pharm_angle_a_12, pharm_angle_a_13, pharm_angle_a_14, 
       pharm_angle_a_15, pharm_angle_a_16, pharm_angle_a_17, pharm_angle_a_18, 
       pharm_angle_a_19, pharm_angle_a_20, pharm_angle_a_21, pharm_angle_a_22, 
       pharm_angle_a_23, pharm_angle_a_24, pharm_angle_a_25, pharm_angle_a_26, 
       pharm_angle_a_27, pharm_angle_a_28, pharm_angle_a_29, pharm_angle_a_30, 
       pharm_angle_a_31, pharm_angle_a_32, pharm_angle_a_33, pharm_angle_a_34, 
       pharm_angle_a_35, pharm_angle_a_36, pharm_angle_a_37, pharm_angle_a_38, 
       pharm_angle_a_39, pharm_angle_a_40, pharm_angle_a_41, pharm_angle_a_42, 
       pharm_angle_a_43, pharm_angle_a_44, pharm_angle_a_45, pharm_angle_a_46, 
       pharm_angle_a_47, pharm_angle_a_48, pharm_angle_a_49, pharm_angle_a_50
  FROM avg_harm_power p INNER JOIN avg_service_info info ON p.datetime_id = info.datetime_id AND p.record_id = info.record_id AND p.datetime_id = {2} ORDER BY dt_start ASC;", str_yes, str_no, curDatetimeId_);
				}

				#endregion

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "avg_harm_power", ref ds);

				#region Selecting turn_ratios and changing rW

				float rW = settings_.PowerRatio;

				//float ctr = 1, vtr = 1;

				//if (curWithTR)
				//{
				//    sqlCommand = new NpgsqlCommand();
				//    sqlCommand.Connection = conEmDb;
				//    commandText = string.Format("SELECT * FROM turn_ratios WHERE reg_id = (SELECT registration_id FROM avg_times where datetime_id = {0});", curDatetimeId_);

				//    dbService.ExecuteReader(commandText);

				//    while (dbService.DataReaderRead())
				//    {
				//        short iType = (short)dbService.DataReaderData("turn_type"];
				//        float fValue1 = (float)dbService.DataReaderData("value1"];
				//        float fValue2 = (float)dbService.DataReaderData("value2"];

				//        if (iType == 1) vtr = fValue1 / fValue2;
				//        if (iType == 2) ctr = fValue1 / fValue2;
				//    }
				//}

				//rW *= vtr * ctr;

				//EmService.WriteToLogDebug(string.Format("КТ применялись: {0} ; КТТ = {1} ; КТН = {2}",
				//    curWithTR, ctr, vtr));

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// powers
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if ((ds.Tables[0].Columns[i].Caption.Contains("p_") ||
							ds.Tables[0].Columns[i].Caption.Contains("pharm_"))
							)
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
								ds.Tables[0].Rows[r][i] = (float)(ds.Tables[0].Rows[r][i]) * rW;
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "avg_harm_power");

				mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				// creating event handler of change position event
				mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);

				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS].List;

				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmPower::load_etPQP_A()");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR, 
			bool bHarmPowersShownInPercent)
		{
			try
			{
				string commandText = string.Empty;
				Int64 iMask, iMask2;
				// у Em32 и EtPQP возможны неполные архивы, поэтому считываем маску, чтобы
				// определить какие колонки таблицы заполнены
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_6a");

				#region Make query

				string query = "";
				if (iMask != -1)
				{
					query = "SELECT datetime_id, event_datetime, ";

					switch (connectScheme_)
					{
						case ConnectScheme.Ph3W4:
						case ConnectScheme.Ph3W4_B_calc:
							if (((iMask & 0x000000001) != 0) && ((iMask & 0x000000002) != 0) &&
								((iMask & 0x000000004) != 0))
							{
								query += "p_sum, ";
							}

							if ((iMask & 0x000000001) != 0) { query += "p_a_1, "; }	// 1 bit
							if ((iMask & 0x000000002) != 0) { query += "p_b_2, "; }	// 2 bit
							if ((iMask & 0x000000004) != 0) { query += "p_c, "; }		// 3 bit

							if (((iMask & 0x000000001) != 0) &&			// 1 bit
								((iMask & 0x000000002) != 0) &&			// 2 bit
								((iMask & 0x000000004) != 0))			// 3 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)		//P_Σ
									{
										query += "p_a_1_" + i.ToString() +
												" + p_b_2_" + i.ToString() +
												" + p_c_" + i.ToString() +
												" as p_sum_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_a_1_1 + p_b_2_1 + p_c_1 as p_sum_1, ";
									for (int i = 2; i <= 40; ++i)		//P_Σ
									{
										query += "CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE " +
											"(p_a_1_" + i.ToString() +
											" + p_b_2_" + i.ToString() +
											" + p_c_" + i.ToString() +
											") * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_" +
											i.ToString() + ", ";
									}
								}
							}
							if ((iMask & 0x000000001) != 0)				// 1 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)
									{
										query += "p_a_1_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_a_1_1, ";
									for (int i = 2; i <= 40; ++i)
									{
										query += "CASE WHEN p_a_1_1=0 THEN 0 ELSE " +
											"p_a_1_" + i.ToString() +
											" * 100 / p_a_1_1 END as p_a_1_" + i.ToString() + ", ";
									}
								}
							}
							if ((iMask & 0x000000002) != 0)				// 2 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)
									{
										query += "p_b_2_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_b_2_1, ";
									for (int i = 2; i <= 40; ++i)
									{
										query += "CASE WHEN p_b_2_1=0 THEN 0 ELSE " +
											"p_b_2_" + i.ToString() +
											" * 100 / p_b_2_1 END as p_b_2_" + i.ToString() + ", ";
									}
								}
							}
							if ((iMask & 0x000000004) != 0)				// 3 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)
									{
										query += "p_c_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_c_1, ";
									for (int i = 2; i <= 40; ++i)
									{
										query += "CASE WHEN p_c_1=0 THEN 0 ELSE " +
											"p_c_" + i.ToString() +
											" * 100 / p_c_1 END as p_c_" + i.ToString() + ", ";
									}
								}
							}
							break;
						case ConnectScheme.Ph3W3:
						case ConnectScheme.Ph3W3_B_calc:
							if (((iMask & 0x000000001) != 0) && ((iMask & 0x000000002) != 0))
							{
								query += "p_sum, ";
							}
							if ((iMask & 0x000000001) != 0) { query += "p_a_1, "; }	// 1 bit
							if ((iMask & 0x000000002) != 0) { query += "p_b_2, "; }	// 2 bit

							if (((iMask & 0x000000001) != 0) &&			// 1 bit
								((iMask & 0x000000002) != 0))			// 2 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)		//P_Σ
									{
										query += "p_a_1_" + i.ToString() +
												" + p_b_2_" + i.ToString() +
												" as p_sum_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_a_1_1 + p_b_2_1 + p_c_1 as p_sum_1, ";
									for (int i = 2; i <= 40; ++i)		//P_Σ
									{
										query += "CASE WHEN (p_a_1_1 + p_b_2_1)=0 THEN 0 ELSE " +
											"(p_a_1_" + i.ToString() +
											" + p_b_2_" + i.ToString() +
											") * 100 / (p_a_1_1 + p_b_2_1) END as p_sum_" +
											i.ToString() + ", ";
									}
								}
							}
							if ((iMask & 0x000000001) != 0)				// 1 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)
									{
										query += "p_a_1_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_a_1_1, ";
									for (int i = 2; i <= 40; ++i)
									{
										query += "CASE WHEN p_a_1_1=0 THEN 0 ELSE " +
											"p_a_1_" + i.ToString() +
											" * 100 / p_a_1_1 END as p_a_1_" + i.ToString() + ", ";
									}
								}
							}
							if ((iMask & 0x000000002) != 0)				// 2 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)
									{
										query += "p_b_2_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_b_2_1, ";
									for (int i = 2; i <= 40; ++i)
									{
										query += "CASE WHEN p_b_2_1=0 THEN 0 ELSE " +
											"p_b_2_" + i.ToString() +
											" * 100 / p_b_2_1 END as p_b_2_" + i.ToString() + ", ";
									}
								}
							}
							break;
						case ConnectScheme.Ph1W2:
							if ((iMask & 0x000000001) != 0) { query += "p_a_1, "; }	// 1 bit
							if ((iMask & 0x000000001) != 0)				// 1 bit
							{
								if (!bHarmPowersShownInPercent)
								{
									for (int i = 1; i <= 40; ++i)
									{
										query += "p_a_1_" + i.ToString() + ", ";
									}
								}
								else
								{
									query += "p_a_1_1, ";
									for (int i = 2; i <= 40; ++i)
									{
										query += "CASE WHEN p_a_1_1=0 THEN 0 ELSE " +
											"p_a_1_" + i.ToString() +
											" * 100 / p_a_1_1 END as p_a_1_" + i.ToString() + ", ";
									}
								}
							}
							break;
					}

					// delete the last comma
					query = query.Remove(query.Length - 2, 2);

					query += String.Format(" FROM period_avg_params_6a WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
				}
				else  // этот код всегда выполняется для Em33, а также для Em32 если архив полный
				{
					if (!bHarmPowersShownInPercent)
					{
						query = String.Format("SELECT event_datetime, p_sum, p_a_1, p_b_2, p_c, p_a_1_1 + p_b_2_1 + p_c_1 as p_sum_1, p_a_1_2 + p_b_2_2 + p_c_2 as p_sum_2, p_a_1_3 + p_b_2_3 + p_c_3 as p_sum_3, p_a_1_4 + p_b_2_4 + p_c_4 as p_sum_4, p_a_1_5 + p_b_2_5 + p_c_5 as p_sum_5, p_a_1_6 + p_b_2_6 + p_c_6 as p_sum_6, p_a_1_7 + p_b_2_7 + p_c_7 as p_sum_7, p_a_1_8 + p_b_2_8 + p_c_8 as p_sum_8, p_a_1_9 + p_b_2_9 + p_c_9 as p_sum_9, p_a_1_10 + p_b_2_10 + p_c_10 as p_sum_10, p_a_1_11 + p_b_2_11 + p_c_11 as p_sum_11, p_a_1_12 + p_b_2_12 + p_c_12 as p_sum_12, p_a_1_13 + p_b_2_13 + p_c_13 as p_sum_13, p_a_1_14 + p_b_2_14 + p_c_14 as p_sum_14, p_a_1_15 + p_b_2_15 + p_c_15 as p_sum_15, p_a_1_16 + p_b_2_16 + p_c_16 as p_sum_16, p_a_1_17 + p_b_2_17 + p_c_17 as p_sum_17, p_a_1_18 + p_b_2_18 + p_c_18 as p_sum_18, p_a_1_19 + p_b_2_19 + p_c_19 as p_sum_19, p_a_1_20 + p_b_2_20 + p_c_20 as p_sum_20, p_a_1_21 + p_b_2_21 + p_c_21 as p_sum_21, p_a_1_22 + p_b_2_22 + p_c_22 as p_sum_22, p_a_1_23 + p_b_2_23 + p_c_23 as p_sum_23, p_a_1_24 + p_b_2_24 + p_c_24 as p_sum_24, p_a_1_25 + p_b_2_25 + p_c_25 as p_sum_25, p_a_1_26 + p_b_2_26 + p_c_26 as p_sum_26, p_a_1_27 + p_b_2_27 + p_c_27 as p_sum_27, p_a_1_28 + p_b_2_28 + p_c_28 as p_sum_28, p_a_1_29 + p_b_2_29 + p_c_29 as p_sum_29, p_a_1_30 + p_b_2_30 + p_c_30 as p_sum_30, p_a_1_31 + p_b_2_31 + p_c_31 as p_sum_31, p_a_1_32 + p_b_2_32 + p_c_32 as p_sum_32, p_a_1_33 + p_b_2_33 + p_c_33 as p_sum_33, p_a_1_34 + p_b_2_34 + p_c_34 as p_sum_34, p_a_1_35 + p_b_2_35 + p_c_35 as p_sum_35, p_a_1_36 + p_b_2_36 + p_c_36 as p_sum_36, p_a_1_37 + p_b_2_37 + p_c_37 as p_sum_37, p_a_1_38 + p_b_2_38 + p_c_38 as p_sum_38, p_a_1_39 + p_b_2_39 + p_c_39 as p_sum_39, p_a_1_40 + p_b_2_40 + p_c_40 as p_sum_40, p_a_1_1, p_a_1_2, p_a_1_3, p_a_1_4, p_a_1_5, p_a_1_6, p_a_1_7, p_a_1_8, p_a_1_9, p_a_1_10, p_a_1_11, p_a_1_12, p_a_1_13, p_a_1_14, p_a_1_15, p_a_1_16, p_a_1_17, p_a_1_18, p_a_1_19, p_a_1_20, p_a_1_21, p_a_1_22, p_a_1_23, p_a_1_24, p_a_1_25, p_a_1_26, p_a_1_27, p_a_1_28, p_a_1_29, p_a_1_30, p_a_1_31, p_a_1_32, p_a_1_33, p_a_1_34, p_a_1_35, p_a_1_36, p_a_1_37, p_a_1_38, p_a_1_39, p_a_1_40, p_b_2_1, p_b_2_2, p_b_2_3, p_b_2_4, p_b_2_5, p_b_2_6, p_b_2_7, p_b_2_8, p_b_2_9, p_b_2_10, p_b_2_11, p_b_2_12, p_b_2_13, p_b_2_14, p_b_2_15, p_b_2_16, p_b_2_17, p_b_2_18, p_b_2_19, p_b_2_20, p_b_2_21, p_b_2_22, p_b_2_23, p_b_2_24, p_b_2_25, p_b_2_26, p_b_2_27, p_b_2_28, p_b_2_29, p_b_2_30, p_b_2_31, p_b_2_32, p_b_2_33, p_b_2_34, p_b_2_35, p_b_2_36, p_b_2_37, p_b_2_38, p_b_2_39, p_b_2_40, p_c_1, p_c_2, p_c_3, p_c_4, p_c_5, p_c_6, p_c_7, p_c_8, p_c_9, p_c_10, p_c_11, p_c_12, p_c_13, p_c_14, p_c_15, p_c_16, p_c_17, p_c_18, p_c_19, p_c_20, p_c_21, p_c_22, p_c_23, p_c_24, p_c_25, p_c_26, p_c_27, p_c_28, p_c_29, p_c_30, p_c_31, p_c_32, p_c_33, p_c_34, p_c_35, p_c_36, p_c_37, p_c_38, p_c_39, p_c_40 FROM period_avg_params_6a WHERE datetime_id = {0} ORDER BY event_datetime ASC;", curDatetimeId_);
					}
					else
					{
						query =
						@"SELECT event_datetime, p_sum, p_a_1, p_b_2, p_c, 
p_a_1_1 + p_b_2_1 + p_c_1 as p_sum_1,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_2 + p_b_2_2 + p_c_2) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_2,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_3 + p_b_2_3 + p_c_3) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_3,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_4 + p_b_2_4 + p_c_4) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_4,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_5 + p_b_2_5 + p_c_5) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_5,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_6 + p_b_2_6 + p_c_6) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_6,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_7 + p_b_2_7 + p_c_7) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_7,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_8 + p_b_2_8 + p_c_8) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_8,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_9 + p_b_2_9 + p_c_9) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_9,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_10 + p_b_2_10 + p_c_10) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_10,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_11 + p_b_2_11 + p_c_11) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_11,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_12 + p_b_2_12 + p_c_12) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_12,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_13 + p_b_2_13 + p_c_13) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_13,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_14 + p_b_2_14 + p_c_14) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_14,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_15 + p_b_2_15 + p_c_15) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_15,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_16 + p_b_2_16 + p_c_16) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_16,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_17 + p_b_2_17 + p_c_17) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_17,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_18 + p_b_2_18 + p_c_18) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_18,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_19 + p_b_2_19 + p_c_19) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_19,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_20 + p_b_2_20 + p_c_20) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_20,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_21 + p_b_2_21 + p_c_21) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_21,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_22 + p_b_2_22 + p_c_22) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_22,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_23 + p_b_2_23 + p_c_23) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_23,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_24 + p_b_2_24 + p_c_24) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_24,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_25 + p_b_2_25 + p_c_25) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_25,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_26 + p_b_2_26 + p_c_26) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_26,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_27 + p_b_2_27 + p_c_27) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_27,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_28 + p_b_2_28 + p_c_28) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_28,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_29 + p_b_2_29 + p_c_29) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_29,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_30 + p_b_2_30 + p_c_30) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_30,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_31 + p_b_2_31 + p_c_31) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_31,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_32 + p_b_2_32 + p_c_32) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_32,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_33 + p_b_2_33 + p_c_33) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_33,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_34 + p_b_2_34 + p_c_34) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_34,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_35 + p_b_2_35 + p_c_35) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_35,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_36 + p_b_2_36 + p_c_36) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_36,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_37 + p_b_2_37 + p_c_37) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_37,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_38 + p_b_2_38 + p_c_38) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_38,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_39 + p_b_2_39 + p_c_39) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_39,
CASE WHEN (p_a_1_1 + p_b_2_1 + p_c_1)=0 THEN 0 ELSE (p_a_1_40 + p_b_2_40 + p_c_40) * 100 / (p_a_1_1 + p_b_2_1 + p_c_1) END as p_sum_40,
p_a_1_1, 
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_2 * 100 / p_a_1_1 END as p_a_1_2,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_3 * 100 / p_a_1_1 END as p_a_1_3,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_4 * 100 / p_a_1_1 END as p_a_1_4,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_5 * 100 / p_a_1_1 END as p_a_1_5,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_6 * 100 / p_a_1_1 END as p_a_1_6,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_7 * 100 / p_a_1_1 END as p_a_1_7,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_8 * 100 / p_a_1_1 END as p_a_1_8,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_9 * 100 / p_a_1_1 END as p_a_1_9,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_10 * 100 / p_a_1_1 END as p_a_1_10,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_11 * 100 / p_a_1_1 END as p_a_1_11,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_12 * 100 / p_a_1_1 END as p_a_1_12,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_13 * 100 / p_a_1_1 END as p_a_1_13,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_14 * 100 / p_a_1_1 END as p_a_1_14,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_15 * 100 / p_a_1_1 END as p_a_1_15,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_16 * 100 / p_a_1_1 END as p_a_1_16,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_17 * 100 / p_a_1_1 END as p_a_1_17,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_18 * 100 / p_a_1_1 END as p_a_1_18,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_19 * 100 / p_a_1_1 END as p_a_1_19,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_20 * 100 / p_a_1_1 END as p_a_1_20,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_21 * 100 / p_a_1_1 END as p_a_1_21,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_22 * 100 / p_a_1_1 END as p_a_1_22,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_23 * 100 / p_a_1_1 END as p_a_1_23,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_24 * 100 / p_a_1_1 END as p_a_1_24,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_25 * 100 / p_a_1_1 END as p_a_1_25,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_26 * 100 / p_a_1_1 END as p_a_1_26,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_27 * 100 / p_a_1_1 END as p_a_1_27,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_28 * 100 / p_a_1_1 END as p_a_1_28,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_29 * 100 / p_a_1_1 END as p_a_1_29,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_30 * 100 / p_a_1_1 END as p_a_1_30,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_31 * 100 / p_a_1_1 END as p_a_1_31,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_32 * 100 / p_a_1_1 END as p_a_1_32,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_33 * 100 / p_a_1_1 END as p_a_1_33,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_34 * 100 / p_a_1_1 END as p_a_1_34,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_35 * 100 / p_a_1_1 END as p_a_1_35,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_36 * 100 / p_a_1_1 END as p_a_1_36,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_37 * 100 / p_a_1_1 END as p_a_1_37,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_38 * 100 / p_a_1_1 END as p_a_1_38,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_39 * 100 / p_a_1_1 END as p_a_1_39,
CASE WHEN p_a_1_1=0 THEN 0 ELSE p_a_1_40 * 100 / p_a_1_1 END as p_a_1_40,
p_b_2_1, 
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_2 * 100 / p_b_2_1 END as p_b_2_2,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_3 * 100 / p_b_2_1 END as p_b_2_3,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_4 * 100 / p_b_2_1 END as p_b_2_4,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_5 * 100 / p_b_2_1 END as p_b_2_5,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_6 * 100 / p_b_2_1 END as p_b_2_6,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_7 * 100 / p_b_2_1 END as p_b_2_7,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_8 * 100 / p_b_2_1 END as p_b_2_8,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_9 * 100 / p_b_2_1 END as p_b_2_9,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_10 * 100 / p_b_2_1 END as p_b_2_10,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_11 * 100 / p_b_2_1 END as p_b_2_11,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_12 * 100 / p_b_2_1 END as p_b_2_12,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_13 * 100 / p_b_2_1 END as p_b_2_13,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_14 * 100 / p_b_2_1 END as p_b_2_14,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_15 * 100 / p_b_2_1 END as p_b_2_15,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_16 * 100 / p_b_2_1 END as p_b_2_16,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_17 * 100 / p_b_2_1 END as p_b_2_17,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_18 * 100 / p_b_2_1 END as p_b_2_18,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_19 * 100 / p_b_2_1 END as p_b_2_19,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_20 * 100 / p_b_2_1 END as p_b_2_20,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_21 * 100 / p_b_2_1 END as p_b_2_21,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_22 * 100 / p_b_2_1 END as p_b_2_22,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_23 * 100 / p_b_2_1 END as p_b_2_23,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_24 * 100 / p_b_2_1 END as p_b_2_24,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_25 * 100 / p_b_2_1 END as p_b_2_25,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_26 * 100 / p_b_2_1 END as p_b_2_26,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_27 * 100 / p_b_2_1 END as p_b_2_27,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_28 * 100 / p_b_2_1 END as p_b_2_28,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_29 * 100 / p_b_2_1 END as p_b_2_29,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_30 * 100 / p_b_2_1 END as p_b_2_30,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_31 * 100 / p_b_2_1 END as p_b_2_31,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_32 * 100 / p_b_2_1 END as p_b_2_32,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_33 * 100 / p_b_2_1 END as p_b_2_33,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_34 * 100 / p_b_2_1 END as p_b_2_34,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_35 * 100 / p_b_2_1 END as p_b_2_35,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_36 * 100 / p_b_2_1 END as p_b_2_36,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_37 * 100 / p_b_2_1 END as p_b_2_37,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_38 * 100 / p_b_2_1 END as p_b_2_38,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_39 * 100 / p_b_2_1 END as p_b_2_39,
CASE WHEN p_b_2_1=0 THEN 0 ELSE p_b_2_40 * 100 / p_b_2_1 END as p_b_2_40,
p_c_1,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_2 * 100 / p_c_1 END as p_c_2,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_3 * 100 / p_c_1 END as p_c_3,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_4 * 100 / p_c_1 END as p_c_4,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_5 * 100 / p_c_1 END as p_c_5,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_6 * 100 / p_c_1 END as p_c_6,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_7 * 100 / p_c_1 END as p_c_7,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_8 * 100 / p_c_1 END as p_c_8,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_9 * 100 / p_c_1 END as p_c_9,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_10 * 100 / p_c_1 END as p_c_10,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_11 * 100 / p_c_1 END as p_c_11,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_12 * 100 / p_c_1 END as p_c_12,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_13 * 100 / p_c_1 END as p_c_13,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_14 * 100 / p_c_1 END as p_c_14,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_15 * 100 / p_c_1 END as p_c_15,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_16 * 100 / p_c_1 END as p_c_16,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_17 * 100 / p_c_1 END as p_c_17,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_18 * 100 / p_c_1 END as p_c_18,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_19 * 100 / p_c_1 END as p_c_19,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_20 * 100 / p_c_1 END as p_c_20,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_21 * 100 / p_c_1 END as p_c_21,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_22 * 100 / p_c_1 END as p_c_22,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_23 * 100 / p_c_1 END as p_c_23,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_24 * 100 / p_c_1 END as p_c_24,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_25 * 100 / p_c_1 END as p_c_25,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_26 * 100 / p_c_1 END as p_c_26,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_27 * 100 / p_c_1 END as p_c_27,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_28 * 100 / p_c_1 END as p_c_28,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_29 * 100 / p_c_1 END as p_c_29,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_30 * 100 / p_c_1 END as p_c_30,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_31 * 100 / p_c_1 END as p_c_31,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_32 * 100 / p_c_1 END as p_c_32,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_33 * 100 / p_c_1 END as p_c_33,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_34 * 100 / p_c_1 END as p_c_34,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_35 * 100 / p_c_1 END as p_c_35,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_36 * 100 / p_c_1 END as p_c_36,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_37 * 100 / p_c_1 END as p_c_37,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_38 * 100 / p_c_1 END as p_c_38,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_39 * 100 / p_c_1 END as p_c_39,
CASE WHEN p_c_1=0 THEN 0 ELSE p_c_40 * 100 / p_c_1 END as p_c_40
FROM period_avg_params_6a WHERE datetime_id = " + curDatetimeId_ +
						" ORDER BY event_datetime ASC;";
					}
				}

				#endregion

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_6a", ref ds);

				#region Selecting turn_ratios and changing rU, rI and rW

				float rW = settings_.PowerRatio;
				float ctr = 1, vtr = 1;

				if (curWithTR)
				{
					if (devType_ == EmDeviceType.EM33T ||
						devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE database_id = (SELECT database_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.EM32)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE device_id = (SELECT device_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);
					else if (devType_ == EmDeviceType.ETPQP)
						commandText = string.Format("SELECT * FROM turn_ratios WHERE object_id = (SELECT object_id FROM period_avg_params_times where datetime_id = {0});", curDatetimeId_);

					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = 0;
						if (devType_ != EmDeviceType.ETPQP)
							iType = (short)dbService.DataReaderData("type");
						else iType = (short)dbService.DataReaderData("turn_type");
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");

						if (iType == 1) vtr = fValue1 / fValue2;
						else if (iType == 2) ctr = fValue1 / fValue2;
					}
				}
				rW *= vtr * ctr;

				#endregion

				#region Changing table values

				for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
				{
					// powers
					for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
					{
						if (ds.Tables[0].Columns[i].Caption.Contains("p_"))
						{
							if (!(ds.Tables[0].Rows[r][i] is DBNull))
							{
								ds.Tables[0].Rows[r][i] =
									Conversions.object_2_float(ds.Tables[0].Rows[r][i]) * rW;
							}
						}
					}
				}

				ds.AcceptChanges();

				#endregion

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_6a");

				mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);
				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS].List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmPower::load_not_EtPQP(): ");
				MessageBoxes.DbConnectError(mainWnd_, dbService.Host, dbService.Port, dbService.Database);
				throw;
			}
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.HARMONIC_POWERS] = null;
		}

		public void RenameHarmonicPowersColumns(bool toPercent)
		{
			try
			{
				string tableName = "period_avg_params_6a";
				if (devType_ == EmDeviceType.ETPQP_A) tableName = "avg_harm_power";

				string newUnit = string.Empty, newUnitVar = string.Empty, newUnitGrad = string.Empty;
				if (toPercent) { newUnit = ", %"; newUnitVar = ", %"; newUnitGrad = ", %"; }
				else
				{
					ResourceManager rm =
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					newUnit = (settings_.PowerRatio == 1) ?
						rm.GetString("column_header_units_w") :
						rm.GetString("column_header_units_kw");
					newUnitVar = (settings_.PowerRatio == 1) ?
						 rm.GetString("column_header_units_var") :
						 rm.GetString("column_header_units_kvar");
					newUnitGrad = rm.GetString("column_header_units_grad");
				}

				if (devType_ == EmDeviceType.ETPQP_A)
				{
					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
										"P_Σ(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_p_sum_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
										"Q_Σ(" + i.ToString() + ")_" + newUnitVar);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_q_sum_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
										"<_Σ(" + i.ToString() + ")_" + newUnitGrad);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_angle_sum_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
					}
					if (connectScheme_ != ConnectScheme.Ph3W3 && connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"P_A(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_p_a_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"Q_A(" + i.ToString() + ")_" + newUnitVar);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_q_a_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"<_A(" + i.ToString() + ")_" + newUnitGrad);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_angle_a_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							for (int i = 2; i <= 40; ++i)
							{
								DataGridColumnHeaderFormula newCap =
									new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
											"P_B(" + i.ToString() + ")_" + newUnit);

								DataGridColumnHeaderFormula oldCap =
								(dataGrid_.TableStyles[tableName].GridColumnStyles[
										"pharm_p_b_" + i.ToString()] as DataGridColumnHeaderFormula);
								oldCap.HeaderFormula = newCap.HeaderFormula;
							}
							for (int i = 2; i <= 40; ++i)
							{
								DataGridColumnHeaderFormula newCap =
									new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
											"Q_B(" + i.ToString() + ")_" + newUnitVar);

								DataGridColumnHeaderFormula oldCap =
								(dataGrid_.TableStyles[tableName].GridColumnStyles[
										"pharm_q_b_" + i.ToString()] as DataGridColumnHeaderFormula);
								oldCap.HeaderFormula = newCap.HeaderFormula;
							}
							for (int i = 2; i <= 40; ++i)
							{
								DataGridColumnHeaderFormula newCap =
									new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
											"<_B(" + i.ToString() + ")_" + newUnitGrad);

								DataGridColumnHeaderFormula oldCap =
								(dataGrid_.TableStyles[tableName].GridColumnStyles[
										"pharm_angle_b_" + i.ToString()] as DataGridColumnHeaderFormula);
								oldCap.HeaderFormula = newCap.HeaderFormula;
							}
							for (int i = 2; i <= 40; ++i)
							{
								DataGridColumnHeaderFormula newCap =
									new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
											"P_C(" + i.ToString() + ")_" + newUnit);

								DataGridColumnHeaderFormula oldCap =
								(dataGrid_.TableStyles[tableName].GridColumnStyles[
										"pharm_p_c_" + i.ToString()] as DataGridColumnHeaderFormula);
								oldCap.HeaderFormula = newCap.HeaderFormula;
							}
							for (int i = 2; i <= 40; ++i)
							{
								DataGridColumnHeaderFormula newCap =
									new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
											"Q_C(" + i.ToString() + ")_" + newUnitVar);

								DataGridColumnHeaderFormula oldCap =
								(dataGrid_.TableStyles[tableName].GridColumnStyles[
										"pharm_q_c_" + i.ToString()] as DataGridColumnHeaderFormula);
								oldCap.HeaderFormula = newCap.HeaderFormula;
							}
							for (int i = 2; i <= 40; ++i)
							{
								DataGridColumnHeaderFormula newCap =
									new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
											"<_C(" + i.ToString() + ")_" + newUnitGrad);

								DataGridColumnHeaderFormula oldCap =
								(dataGrid_.TableStyles[tableName].GridColumnStyles[
										"pharm_angle_c_" + i.ToString()] as DataGridColumnHeaderFormula);
								oldCap.HeaderFormula = newCap.HeaderFormula;
							}
						}
					}
					else   //3ph3w
					{
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"P_1(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_p_1_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"Q_1(" + i.ToString() + ")_" + newUnitVar);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_q_1_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"<_1(" + i.ToString() + ")_" + newUnitGrad);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_angle_1_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}

						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"P_2(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_p_2_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"Q_2(" + i.ToString() + ")_" + newUnitVar);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_q_2_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"<_2(" + i.ToString() + ")_" + newUnitGrad);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"pharm_angle_2_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
					}
				}
				else   // not Et-PQP-A
				{
					if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorCommon,
										"P_Σ(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"p_sum_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}

						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap =
								new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC,
										"P_C(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"p_c_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
					}

					for (int i = 2; i <= 40; ++i)
					{
						DataGridColumnHeaderFormula newCap;
						if (connectScheme_ == ConnectScheme.Ph3W3 ||
							connectScheme_ == ConnectScheme.Ph3W3_B_calc)
							newCap = new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								"P_AB(" + i.ToString() + ")_" + newUnit);
						else newCap = new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA,
								"P_A(" + i.ToString() + ")_" + newUnit);

						DataGridColumnHeaderFormula oldCap =
						(dataGrid_.TableStyles[tableName].GridColumnStyles[
								"p_a_1_" + i.ToString()] as DataGridColumnHeaderFormula);
						oldCap.HeaderFormula = newCap.HeaderFormula;
					}

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						for (int i = 2; i <= 40; ++i)
						{
							DataGridColumnHeaderFormula newCap;
							if (connectScheme_ == ConnectScheme.Ph3W3 ||
								connectScheme_ == ConnectScheme.Ph3W3_B_calc)
								newCap = new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
									"P_CB(" + i.ToString() + ")_" + newUnit);
							else newCap = new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB,
									"P_B(" + i.ToString() + ")_" + newUnit);

							DataGridColumnHeaderFormula oldCap =
							(dataGrid_.TableStyles[tableName].GridColumnStyles[
									"p_b_2_" + i.ToString()] as DataGridColumnHeaderFormula);
							oldCap.HeaderFormula = newCap.HeaderFormula;
						}
					}
				}

				dataGrid_.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in RenameHarmonicPowersColumns():");
				throw;
			}
		}
	}

	internal class AVGDataGridWrapperHarmAngles : AVGDataGridWrapperBase
	{
		public AVGDataGridWrapperHarmAngles(ref Settings settings, EmDeviceType devType, ref DataGrid dg,
								Int64 curDatetimeId, ConnectScheme conScheme,
								frmDocAVGMain mainWnd)
			: base(ref settings, devType, ref dg, curDatetimeId, conScheme, mainWnd)
		{ }

		protected override void init_etPQP_A(ref DbService dbService)
		{
		}

		protected override void init_not_etPQP_A(ref DbService dbService)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				string unit_grad = rm.GetString("column_header_units_grad");

				dataGrid_.TableStyles.Clear();

				DataGridTableStyle ts = new DataGridTableStyle();
				ts.MappingName = "period_avg_params_6b";
				ts.AllowSorting = false;

				// event_datetime
				DataGridColumnHeaderFormula cs_event =
					new DataGridColumnHeaderFormula(DataGridColors.ColorAvgTime);

				cs_event.HeaderText = rm.GetString("name_columnheaders_avg_time");
				cs_event.MappingName = "event_datetime";
				cs_event.Format = "G";
				cs_event.Width = DataColumnsWidth.TimeWidth;
				ts.GridColumnStyles.Add(cs_event);

				// ∟U_A 1_ I_A 1
				for (int i = 1; i < 41; i++)
				{
					string cap = "<U_A(" + i.ToString() + ")_ I_A(" + i.ToString() + ")";
					DataGridColumnHeaderFormula cs_k =
						new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseA, cap);
					cs_k.HeaderText = "";
					cs_k.MappingName = "an_u_a_" + i.ToString() + "_i_a_" + i.ToString();
					cs_k.Format = DataColumnsFormat.FloatFormat;
					cs_k.Width = DataColumnsWidth.CommonWidth;
					ts.GridColumnStyles.Add(cs_k);
				}

				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					// ∟U_B 1_ I_B 1
					for (int i = 1; i < 41; i++)
					{
						string cap = "<U_B(" + i.ToString() + ")_ I_B(" + i.ToString() + ")";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseB, cap);
						cs_k.HeaderText = "";
						cs_k.MappingName = "an_u_b_" + i.ToString() + "_i_b_" + i.ToString();
						cs_k.Format = DataColumnsFormat.FloatFormat;
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}

					// ∟U_C 1_ I_C 1
					for (int i = 1; i < 41; i++)
					{
						string cap = "<U_C(" + i.ToString() + ")_ I_C(" + i.ToString() + ")";
						DataGridColumnHeaderFormula cs_k =
							new DataGridColumnHeaderFormula(DataGridColors.ColorAvgPhaseC, cap);
						cs_k.HeaderText = "";
						cs_k.MappingName = "an_u_c_" + i.ToString() + "_i_c_" + i.ToString();
						cs_k.Format = DataColumnsFormat.FloatFormat;
						cs_k.Width = DataColumnsWidth.CommonWidth;
						ts.GridColumnStyles.Add(cs_k);
					}
				}

				string float_format = settings_.FloatFormat;
				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridTextBoxColumn).Format = float_format;
				}

				for (int i = 1; i < ts.GridColumnStyles.Count; i++)
				{
					(ts.GridColumnStyles[i] as DataGridColumnHeaderFormula).HeaderFormula += ("_" + unit_grad);
				}

				dataGrid_.TableStyles.Add(ts);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmAngles::init(): ");
				throw;
			}
		}

		protected override void load_etPQP_A(ref DbService dbService, bool curWithTR,
			bool bHarmPowersShownInPercent)
		{
		}

		protected override void load_not_etPQP_A(ref DbService dbService, bool curWithTR,
			bool bHarmPowersShownInPercent)
		{
			try
			{
				Int64 iMask, iMask2;
				// у Em32 и EtPQP возможны неполные архивы, поэтому считываем маску, чтобы
				// определить какие колонки таблицы заполнены
				GetMask(out iMask, out iMask2, ref dbService, "period_avg_params_6b");

				string query = "";
				if (iMask != -1)
				{
					query = "SELECT datetime_id, event_datetime, ";

					switch (connectScheme_)
					{
						case ConnectScheme.Ph3W4:
						case ConnectScheme.Ph3W3:
						case ConnectScheme.Ph3W4_B_calc:
						case ConnectScheme.Ph3W3_B_calc:
							if ((iMask & 0x00000001) != 0)				// 1 bit
							{
								for (int i = 1; i <= 40; ++i)
								{
									query += "an_u_a_" + i.ToString() + "_i_a_" + i.ToString() + ", ";
								}
							}
							if ((iMask & 0x00000002) != 0)				// 2 bit
							{
								for (int i = 1; i <= 40; ++i)
								{
									query += "an_u_b_" + i.ToString() + "_i_b_" + i.ToString() + ", ";
								}
							}
							if ((iMask & 0x00000004) != 0)				// 3 bit
							{
								for (int i = 1; i <= 40; ++i)
								{
									query += "an_u_c_" + i.ToString() + "_i_c_" + i.ToString() + ", ";
								}
							}
							break;
						case ConnectScheme.Ph1W2:
							if ((iMask & 0x00000001) != 0)				// 1 bit
							{
								for (int i = 1; i <= 40; ++i)
								{
									query += "an_u_a_" + i.ToString() + "_i_a_" + i.ToString() + ", ";
								}
							}
							break;
					}

					// delete the last comma
					query = query.Remove(query.Length - 2, 2);

					query += String.Format(
						" FROM period_avg_params_6b WHERE datetime_id = {0} ORDER BY event_datetime ASC;",
						curDatetimeId_);
				}
				else  // этот код всегда выполняется для Em33, а также для Em32 если архив полный
				{
					query = String.Format(
						"SELECT * FROM period_avg_params_6b WHERE datetime_id = {0} ORDER BY event_datetime ASC;",
						curDatetimeId_);
				}

				DataSet ds = new DataSet();
				dbService.CreateAndFillDataAdapter(query, "period_avg_params_6b", ref ds);

				// binding dataset with datagrid
				dataGrid_.SetDataBinding(ds, "period_avg_params_6b");

				mainWnd_.CMs_[(int)AvgPages.HARMONIC_ANGLES] = 
					(CurrencyManager)mainWnd_.BindingContext[ds, dataGrid_.DataMember];
				mainWnd_.CMs_[(int)AvgPages.HARMONIC_ANGLES].PositionChanged += 
					new EventHandler(mainWnd_.currencyManager_PositionChanged);
				DataView dataView = (DataView)mainWnd_.CMs_[(int)AvgPages.HARMONIC_ANGLES].List;
				dataView.AllowNew = false;
				dataView.AllowDelete = false;
				dataView.AllowEdit = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AVGDataGridWrapperHarmAngles::load_not_EtPQP(): ");
				throw;
			}
		}

		public void unload()
		{
			mainWnd_.CMs_[(int)AvgPages.HARMONIC_ANGLES] = null;
		}
	}
}
