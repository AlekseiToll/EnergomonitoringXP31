using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Threading;

using EmServiceLib;
using DbServiceLib;

namespace ExportToExcel
{
    public class PQPReportEtPQP_A : PQPReportBase
    {
	    private const short TemperatureMinLimit = 18;	// нижняя граница диапазона норм.температур для ЭтПКЭ-А
		private const short TemperatureMaxLimit = 28;	// верхняя граница диапазона норм.температур для ЭтПКЭ-А

		private PQPProtocolType protocolType_;
		private double gpsLatitude_ = 0;
		private double gpsLongitude_ = 0;
		//private bool autocorrectTimeGpsEnable_;
	    private DEVICE_VERSIONS newDipSwellMode_;
	    private short temperatureMin_ = Int16.MaxValue;
	    private short temperatureMax_ = Int16.MaxValue;

		// устаревший конструктор, тут прервывания вместе с провалами
        public PQPReportEtPQP_A(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
            ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgOver,
            ref DataGrid dgFlick, ref DataGrid dgNonS, ref DataGrid dgInterharm,
            DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme, long ser_num,
			PQPProtocolType type, double gpsLatitude, double gpsLongitude,
			DEVICE_VERSIONS newDipSwellMode)
            : base(settings, ref dgU, ref dgUval, ref dgF, ref dgNon, ref dgNon2, ref dgDip, ref dgOver,
                    ref dgFlick, ref dgNonS, ref dgInterharm, sdt, edt, devType, conScheme, ser_num)
        {
			protocolType_ = type;
			gpsLatitude_ = gpsLatitude;
			gpsLongitude_ = gpsLongitude;
	        newDipSwellMode_ = newDipSwellMode;
        }

		// в этом конструкторе в таблице dgDip2 находятся прерывания, которые вынесены
		// отдельно (т.к. для них изменились интервалы)
		public PQPReportEtPQP_A(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
			ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgDip2, ref DataGrid dgOver,
			ref DataGrid dgFlick, ref DataGrid dgNonS, ref DataGrid dgInterharm,
			DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme, long ser_num,
			PQPProtocolType type, double gpsLatitude, double gpsLongitude,
			DEVICE_VERSIONS newDipSwellMode)
			: base(settings, ref dgU, ref dgUval, ref dgF, ref dgNon, ref dgNon2, ref dgDip, ref dgDip2, ref dgOver,
					ref dgFlick, ref dgNonS, ref dgInterharm, sdt, edt, devType, conScheme, ser_num)
		{
			protocolType_ = type;
			gpsLatitude_ = gpsLatitude;
			gpsLongitude_ = gpsLongitude;
			newDipSwellMode_ = newDipSwellMode;
		}

		// добавлена температура
		public PQPReportEtPQP_A(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
			ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgDip2, ref DataGrid dgOver,
			ref DataGrid dgFlick, ref DataGrid dgNonS, ref DataGrid dgInterharm,
			DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme, long ser_num,
			PQPProtocolType type,
			double gpsLatitude, double gpsLongitude, //bool autocorrectTimeGpsEnable,
			DEVICE_VERSIONS newDipSwellMode,
			short temperatureMin, short temperatureMax)
			: base(settings, ref dgU, ref dgUval, ref dgF, ref dgNon, ref dgNon2, ref dgDip, ref dgDip2, ref dgOver,
					ref dgFlick, ref dgNonS, ref dgInterharm, sdt, edt, devType, conScheme, ser_num)
		{
			protocolType_ = type;
			gpsLatitude_ = gpsLatitude;
			gpsLongitude_ = gpsLongitude;
			//autocorrectTimeGpsEnable_ = autocorrectTimeGpsEnable;
			newDipSwellMode_ = newDipSwellMode;
			temperatureMin_ = temperatureMin;
			temperatureMax_ = temperatureMax;
		}

        /// <summary>
        /// Exports all PQP data into Excel format
        /// </summary>
        public void ExportReport()
        {
            string out_fn = string.Empty;
            try
            {
                StyleName_ = string.Empty;

                //bool bEXIT = false;			// exit flag
                string rt = string.Empty;	// text to process

                // reading template file
                string template_fn = string.Empty;
				switch (connectScheme_)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						switch (protocolType_)
						{
							case PQPProtocolType.VERSION_1:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph4w"; break;
							case PQPProtocolType.VERSION_2:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph4w_v2"; break;
							case PQPProtocolType.VERSION_3:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph4w_v3"; break;
						}
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						switch (protocolType_)
						{
							case PQPProtocolType.VERSION_1:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph3w"; break;
							case PQPProtocolType.VERSION_2:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph3w_v2"; break;
							case PQPProtocolType.VERSION_3:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph3w_v3"; break;
						}
						break;
					case ConnectScheme.Ph1W2:
						switch (protocolType_)
						{
							case PQPProtocolType.VERSION_1:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_1ph2w"; break;
							case PQPProtocolType.VERSION_2:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_1ph2w_v2"; break;
							case PQPProtocolType.VERSION_3:
								template_fn = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_1ph2w_v3"; break;
						}
						break;
				}
	            if (newDipSwellMode_ != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073) template_fn += "_old";
	            template_fn += ".xml";

                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(template_fn, System.Text.Encoding.UTF8);
                    rt = sr.ReadToEnd();
                }
                catch
                {
                    MessageBoxes.PqpReportTmplateError(this, template_fn);
                    return;
                }
                finally { if (sr != null) sr.Close(); }

                frmDocPQPReportSaveDialog wndPqpRepSettings = new frmDocPQPReportSaveDialog(
					Thread.CurrentThread.CurrentUICulture, true);
                if (wndPqpRepSettings.ShowDialog() != DialogResult.OK) return;

                out_fn = wndPqpRepSettings.txtFileName.Text;
                string rep_num = wndPqpRepSettings.txtReportNumber.Text;
                string app_num = wndPqpRepSettings.txtAppendixNumber.Text;
                bool bOPEN_REPORT_AFTER_SAVING = wndPqpRepSettings.chkOpenAfterSaving.Checked;

                // Header
                ExprortReportPQP_head(ref rt, app_num, rep_num);

				// GPS
				//if (gpsLatitude_ > 0 || gpsLongitude_ > 0)
				//{
				//    rt = rt.Replace("{gps_latitude}", gpsLatitude_.ToString());
				//    rt = rt.Replace("{gps_longitude}", gpsLongitude_.ToString());
				//}

                // parsing template file and inserting values

                // float format
                string fl = "0.00";
                if (settings_.FloatSigns > 1)
                {
                    fl = "0.";
                    for (int iSign = 0; iSign < settings_.FloatSigns; ++iSign) fl += "0";
                }

				// it's important to fill this table at first! lapses arrays are filled here
				ExportTemperatureTable(ref rt, fl);

				ExportFDeviation(ref rt, fl);
				ExportUDeviation(ref rt, fl);
				ExportNonsymmetry(ref rt, fl);
				ExportNonsinus(ref rt, fl);
				if(newDipSwellMode_ != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					ExportDipAndSwell_OLD(ref rt, fl);
				else ExportDipAndSwell_GOST2014(ref rt, fl);
				ExportFlicker(ref rt, fl);
				ExportInterharm(ref rt, fl);
				ExportMarkedTable(ref rt, sdt_, edt_, fl);

                // напоследок убиваем все шаблоны, что не смогли заменить значениями
                System.Text.RegularExpressions.Regex regex =
                    new System.Text.RegularExpressions.Regex("{.*?}");
                rt = regex.Replace(rt, " - ");

                // saving file
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(out_fn, false, System.Text.Encoding.UTF8);
                    sw.Write(rt);
                }
                catch
                {
                    MessageBoxes.PqpReportWriteError(this, out_fn);
                    return;
                }
                finally { if (sw != null) sw.Close(); }

                // and opening if needed
                if (bOPEN_REPORT_AFTER_SAVING)
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "excel.exe";
                    process.StartInfo.Arguments = String.Format("\"{0}\"", out_fn);
                    process.StartInfo.WorkingDirectory = "";
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.Start();
                }
                else
                {
                    MessageBoxes.PqpReportSavedSuccess(this, out_fn);
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in ExportReportPQP_A():");
                MessageBoxes.PqpReportWriteError(this, out_fn);
                throw;
            }
        }

		// структура, описывающая одни сутки
		struct DayForMarkedTable 
		{
			public DateTime dtStart;
			public DateTime dtEnd;
			public int cntMarked;
			public int cntAll;
		}

		protected void ExportMarkedTable(ref string rt, DateTime dtStart, DateTime dtEnd, string fl)
		{
			try
			{
				// number of rows in the Voltage Deviation table
				int rows = (dgU_Deviation_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;
				if (rows == 0) return;

				// если архив не больше суток, то просто берем кол-во маркированных из таблицы dU и подставляем в таблицу
				TimeSpan dtDiff = dtEnd - dtStart;
				if ((dtDiff.Days == 0) || 
					(dtDiff.Days == 1 && dtDiff.Hours == 0 && dtDiff.Minutes == 0 && dtDiff.Seconds == 0))
				{
					GridColumnStylesCollection stU = dgU_Deviation_.TableStyles[0].GridColumnStyles;

					// перебираем все номера
					int num_not_marked = 0, num_marked = 0;
					for (int iRow = 0; iRow < rows; iRow++)
					{
						string row_name = (dgU_Deviation_[iRow, 0] as string).TrimEnd();

						// отбрасываем строки, касающиеся наибольших и наименьших
						if (row_name.Contains('\'') || row_name.Contains('"')) continue;

						num_not_marked = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
						num_marked = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_marked"])];
						break;	// во всех строках эти значения одинаковые, поэтому, получив один раз, выходим
					}
					rt = rt.Replace("{start1}", dtStart.ToString());
					rt = rt.Replace("{end1}", dtEnd.ToString());

					float perc_marked = (float)num_marked * 100.0F / ((float)num_marked + (float)num_not_marked);
					rt = rt.Replace("{mark_a_1}", perc_marked.ToString(fl));
					rt = rt.Replace("{mark_b_1}", perc_marked.ToString(fl));
					rt = rt.Replace("{mark_c_1}", perc_marked.ToString(fl));
				}
				else  // если в архиве больше суток, надо рассчитать все по суткам
				{
					GridColumnStylesCollection stU = dgU_Values_.TableStyles[0].GridColumnStyles;

					List<Pair<DateTime, short>> listMarked = new List<Pair<DateTime, short>>();
					rows = (dgU_Values_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;
					for (int iRow = 0; iRow < rows; iRow++)
					{
						short record_marked = (short)dgU_Values_[iRow, stU.IndexOf(stU["record_marked"])];

						DateTime curDt;
						string curDtStr = dgU_Values_[iRow, stU.IndexOf(stU["event_datetime"])].ToString();
						if (!DateTime.TryParse(curDtStr, out curDt))
						{
							EmService.WriteToLogFailed("ExportMarkedTable date error: " + curDtStr);
							continue;
						}
						listMarked.Add(new Pair<DateTime, short>(curDt, record_marked));
					}

					bool not_finished = true;
					int day_number = 1;
					DateTime curDtStart = dtStart;
					while (not_finished)
					{
						DayForMarkedTable curData = new DayForMarkedTable();
						curData.dtStart = curDtStart;
						curData.dtEnd = curDtStart.AddDays(1);
						if (curData.dtEnd >= dtEnd)
						{
							if (curData.dtEnd > dtEnd) curData.dtEnd = dtEnd;
							not_finished = false;
						}

						// выбираем строки относящиеся к текущим суткам
						for (int iItem = 0; iItem < listMarked.Count; ++iItem)
						{
							if (listMarked[iItem].First >= curData.dtStart && listMarked[iItem].First <= curData.dtEnd)
							{
								curData.cntAll++;
								if (listMarked[iItem].Second != 0) curData.cntMarked++;
							}
						}

						rt = rt.Replace("{start" + day_number.ToString() + "}", curData.dtStart.ToString());
						rt = rt.Replace("{end" + day_number.ToString() + "}", curData.dtEnd.ToString());

						float perc_marked = 0;
						if (curData.cntAll != 0) perc_marked = curData.cntMarked * 100 / curData.cntAll;
						string strCurValue = (curData.cntAll != 0) ? perc_marked.ToString(fl) : "-";
						rt = rt.Replace("{mark_a_" + day_number.ToString() + "}", strCurValue);
						rt = rt.Replace("{mark_b_" + day_number.ToString() + "}", strCurValue);
						rt = rt.Replace("{mark_c_" + day_number.ToString() + "}", strCurValue);

						day_number++;
						curDtStart = curDtStart.AddDays(1);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportMarkedTable():");
				throw;
			}
		}

		private Pair<string, float> GetLapse(bool main, string name)
		{
			if (main)
			{
				foreach (var item in lapsesMain_)
				{
					if (item.First == name) return item;
				}
			}
			else
			{
				foreach (var item in lapsesAdditional_)
				{
					if (item.First == name) return item;
				}
			}
			return null;
		}

	    private List<Pair<string, float>> lapsesMain_ = new List<Pair<string, float>>();
		private List<Pair<string, float>> lapsesAdditional_ = new List<Pair<string, float>>(); 

		protected void ExportTemperatureTable(ref string rt, string fl)//, int Unom, 
			//float U0, float U1, float Ua, float Ub, float Uc)
		{
			fl = "0.000";  //we decided to always use 3 signs 

			// if user didn't introduce temperature values, exit the function
			if(temperatureMin_ == Int16.MaxValue || temperatureMax_ == Int16.MaxValue)
				return;

			#region Lapses

			lapsesMain_.Clear();
			lapsesAdditional_.Clear();

			lapsesMain_.Add(new Pair<string, float>("_df", 0.01f));
			lapsesMain_.Add(new Pair<string, float>("_f", 0.01f));
			//lapsesMain_.Add(new Pair<string, float>("_u_av", 0.1f));
			//lapsesMain_.Add(new Pair<string, float>("_u1", 0.1f));
			//lapsesMain_.Add(new Pair<string, float>("_ud", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_dU", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_angleU", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_k2u", 0.15f));
			lapsesMain_.Add(new Pair<string, float>("_ku", 5.0f));			// was 0.05
			lapsesMain_.Add(new Pair<string, float>("_ku_harm", 5.0f));		// was 0.05
			lapsesMain_.Add(new Pair<string, float>("_u_harm", 5.0f));
			lapsesMain_.Add(new Pair<string, float>("_u_interharm", 5.0f));
			//lapsesMain_.Add(new Pair<string, float>("_dip_u", 0.1f));
			//lapsesMain_.Add(new Pair<string, float>("_interr_u", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_dip_val", 0.2f));
			lapsesMain_.Add(new Pair<string, float>("_interr_t", 0.2f));
			lapsesMain_.Add(new Pair<string, float>("_dip_t", 0.02f));
			//lapsesMain_.Add(new Pair<string, float>("_u_over", 0.2f));
			lapsesMain_.Add(new Pair<string, float>("_over_t", 0.02f));
			lapsesMain_.Add(new Pair<string, float>("_flik_sh", 5.0f));
			lapsesMain_.Add(new Pair<string, float>("_flik_long", 5.0f));
			lapsesMain_.Add(new Pair<string, float>("_time_utc", 0.005f));
			lapsesMain_.Add(new Pair<string, float>("_time_not_utc", 0.5f));

			#endregion

			try
			{
				// calculate temperature difference
				// min
				int dMin, dMax, dAll;
				if (temperatureMin_ < TemperatureMinLimit)
					dMin = TemperatureMinLimit - temperatureMin_;
				else if (temperatureMin_ > TemperatureMaxLimit)
					dMin = temperatureMin_ - TemperatureMaxLimit;
				else dMin = 0;

				if (temperatureMax_ < TemperatureMinLimit)
					dMax = TemperatureMinLimit - temperatureMax_;
				else if (temperatureMax_ > TemperatureMaxLimit)
					dMax = temperatureMax_ - TemperatureMaxLimit;
				else dMax = 0;

				dAll = dMax > dMin ? dMax : dMin;
				rt = rt.Replace("{dTemper}", dAll.ToString());

				// calculate additional lapse
				//float normLapse_f = lapses_.Find(x => x.First.Equals("_df")).Second;
				//float additionalLapse_f = 0.02f /*const for EtPQP-A*/ * normLapse_f * dAll;
				//rt = rt.Replace("{tl_f}", additionalLapse_f.ToString(fl));
				foreach (var curPair in lapsesMain_)
				{
					float normLapse = curPair.Second;
					float additionalLapse = 0.02f /*const for EtPQP-A*/ * normLapse * dAll;
					rt = rt.Replace("{tl" + curPair.First + "}", additionalLapse.ToString(fl));
					lapsesAdditional_.Add(new Pair<string, float>(curPair.First, additionalLapse));
				}

				//////////////////////////////////////////////////////
				// fill inaccuracy in all other tables
				float lapseMain;
				float lapseAdd;

				//if (!autocorrectTimeGpsEnable_)
				//{
				//    EmService.WriteToLogGeneral(
				//        "ExportTemperatureTable: autocorrectTimeGpsEnable_ = false");
				//}

				if ((gpsLatitude_ > 0 || gpsLongitude_ > 0) /*&& autocorrectTimeGpsEnable_*/)
				{
					lapseMain = GetLapse(true, "_time_utc").Second;
					lapseAdd = GetLapse(false, "_time_utc").Second;
					rt = rt.Replace("{markir_data_lapse}", (lapseMain + lapseAdd).ToString(fl));
				}
				else
				{
					lapseMain = GetLapse(true, "_time_not_utc").Second;
					lapseAdd = GetLapse(false, "_time_not_utc").Second;
					rt = rt.Replace("{markir_data_lapse}", (lapseMain + lapseAdd).ToString(fl));
				}

				// dU
				lapseMain = GetLapse(true, "_dU").Second;
				lapseAdd = GetLapse(false, "_dU").Second;
				rt = rt.Replace("{dU_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// df
				lapseMain = GetLapse(true, "_df").Second;
				lapseAdd = GetLapse(false, "_df").Second;
				rt = rt.Replace("{df_lapse}", (lapseMain + lapseAdd).ToString(fl));
				
				// K2u, K0u
				lapseMain = GetLapse(true, "_k2u").Second;
				lapseAdd = GetLapse(false, "_k2u").Second;
				rt = rt.Replace("{dK2U_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// Ku
				lapseMain = GetLapse(true, "_ku").Second;
				lapseAdd = GetLapse(false, "_ku").Second;
				rt = rt.Replace("{dKu_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// Ku n
				lapseMain = GetLapse(true, "_ku_harm").Second;
				lapseAdd = GetLapse(false, "_ku_harm").Second;
				rt = rt.Replace("{dKun_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// dip an swell
				lapseMain = GetLapse(true, "_dip_val").Second;
				lapseAdd = GetLapse(false, "_dip_val").Second;
				rt = rt.Replace("{dipswell_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// fliker short
				lapseMain = GetLapse(true, "_flik_sh").Second;
				lapseAdd = GetLapse(false, "_flik_sh").Second;
				rt = rt.Replace("{dFst_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// fliker long
				lapseMain = GetLapse(true, "_flik_long").Second;
				lapseAdd = GetLapse(false, "_flik_long").Second;
				rt = rt.Replace("{dFlt_lapse}", (lapseMain + lapseAdd).ToString(fl));
				///////////////////////////////////////////////////////
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportTemperatureTable():");
				throw;
			}
		}

		protected void ExportFDeviation(ref string rt, string fl)
		{
			try
			{
				GridColumnStylesCollection stF = dgFrequencyDeparture_.TableStyles[0].GridColumnStyles;

				// df
				//if (dgFrequencyDeparture[0, stF.IndexOf(stF["calc_nrm_rng_bottom"])] is float) //..12
				//    rt = rt.Replace("{dfнр}", ((float)dgFrequencyDeparture[0,
				//        stF.IndexOf(stF["calc_nrm_rng_bottom"])]).ToString(fl)); //..12
				//if (dgFrequencyDeparture[0, stF.IndexOf(stF["calc_nrm_rng_top"])] is float) //..10
				//    rt = rt.Replace("{dfвр}", ((float)dgFrequencyDeparture[0,
				//        stF.IndexOf(stF["calc_nrm_rng_top"])]).ToString(fl)); //..10

				float fval = 0;
				if (dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_bottom"])] is string)
				{
					if (!dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_bottom"])].ToString().Equals("-"))
					{
						Conversions.object_2_float_en_ru(
							dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_bottom"])],
							out fval);
						string str_val = fval.ToString(fl);

						rt = rt.Replace("{dfнр}", str_val);
					}
				}
				else if (dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_bottom"])] is float)
				{
					rt = rt.Replace("{dfнр}", ((float)dgFrequencyDeparture_[0,
									stF.IndexOf(stF["calc_nrm_rng_bottom"])]).ToString(fl));
				}

				if (dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_top"])] is string)
				{
					if (!dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_top"])].ToString().Equals("-"))
					{
						Conversions.object_2_float_en_ru(
							dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_top"])],
							out fval);
						string str_val = fval.ToString(fl);

						rt = rt.Replace("{dfвр}", str_val);
					}
				}
				else if (dgFrequencyDeparture_[0, stF.IndexOf(stF["calc_nrm_rng_top"])] is float)
				{
					rt = rt.Replace("{dfвр}", ((float)dgFrequencyDeparture_[0,
									stF.IndexOf(stF["calc_nrm_rng_top"])]).ToString(fl));
				}

				rt = rt.Replace("{dfнмр}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
					stF.IndexOf(stF["calc_max_rng_bottom"])])).ToString(fl));
				rt = rt.Replace("{dfнбр}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
					stF.IndexOf(stF["calc_max_rng_top"])])).ToString(fl));
				rt = rt.Replace("{dfнн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
					stF.IndexOf(stF["real_nrm_rng_bottom_syn"])])).ToString(fl));
				rt = rt.Replace("{dfвн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
					stF.IndexOf(stF["real_nrm_rng_top_syn"])])).ToString(fl));
				rt = rt.Replace("{dfнмн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
					stF.IndexOf(stF["real_max_rng_bottom_syn"])])).ToString(fl));
				rt = rt.Replace("{dfнбн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
					stF.IndexOf(stF["real_max_rng_top_syn"])])).ToString(fl));

				//ReplaceExcelName(ref rt, dgFrequencyDeparture,
				//    "dfT1", "prcnt_max_rng", "prcnt_out_max_rng",
				//    0, fl, stF);

				ReplaceExcelName_EtPQP_A(ref rt, dgFrequencyDeparture_,
					"dfT1", "num_max_rng", "num_out_max_rng", "num_synchro",
					0, fl, stF);

				ReplaceExcelName_EtPQP_A(ref rt, dgFrequencyDeparture_,
					"dfT2", "num_out_max_rng", "num_synchro",
					0, fl, stF);

				// Погрешности df
				rt = rt.Replace("{ddfр}", "±0,01 Гц. (абс.)");
				rt = rt.Replace("{ddfн}", "±0,01 Гц.");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportFDeviation():");
				throw;
			}
		}

		protected void ExportUDeviation(ref string rt, string fl)
		{
			try
			{
				// number of rows in the Voltage Deviation table
				int rows = (dgU_Deviation_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;
				if (rows == 0) return;

				GridColumnStylesCollection stU = dgU_Deviation_.TableStyles[0].GridColumnStyles;

				if (protocolType_ == PQPProtocolType.VERSION_1)
				{
					float t2_a = 0, t2_b = 0, t2_c = 0;
					// перебираем все номера
					int num_not_marked = 0;
					for (int iRow = 0; iRow < rows; iRow++)
					{
						string row_name = (dgU_Deviation_[iRow, 0] as string).TrimEnd();

						// пока отбрасываем строки, касающиеся наибольших и наименьших (если они есть)
						if (row_name.Contains('\'') || row_name.Contains('"')) continue;

						// суфикс для Фазы. A, B, C, AB, BC, CA и 
						// string.Empty для прямой последовательности
						string phase = string.Empty;

						// разделяем строку массив по символу "_"
						string[] parts = row_name.Split(new string[] { "_" },
							StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length > 2)
							phase = parts[1] + parts[2];
						else phase = parts[1];

						if (phase == "AB+") phase = "A+";
						if (phase == "BC+") phase = "B+";
						if (phase == "CA+") phase = "C+";
						if (phase == "AB-") phase = "A-";
						if (phase == "BC-") phase = "B-";
						if (phase == "CA-") phase = "C-";

						float val = 0;
						int num_out_max = 0;
						if (dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])] is string)
						{
							if (!dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])].ToString().Equals("-"))
							{
								Conversions.object_2_float_en_ru(
									dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])],
									out val);
								string str_val = val.ToString(fl);

								rt = rt.Replace("{" + string.Format("dU{0}r", phase) + "}", str_val);
							}
						}
						else if (dgU_Deviation_[iRow, stU.IndexOf(stU["calc_nrm_rng_top"])] is float)
						{
							val = (float)dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])];
							rt = rt.Replace("{" + string.Format("dU{0}r", phase) + "}",
																		val.ToString(fl));
						}

						if (phase.Contains('+'))
							rt = rt.Replace("{" + string.Format("dU{0}n", phase) + "}",
								((float)dgU_Deviation_[iRow,
								stU.IndexOf(stU["real_max_rng_top"])]).ToString(fl));
						else
							rt = rt.Replace("{" + string.Format("dU{0}n", phase) + "}",
								((float)dgU_Deviation_[iRow,
								stU.IndexOf(stU["real_max_rng_bottom"])]).ToString(fl));

						// это поле нужно только для вычисления T2, в отчет оно не идет
						if (dgU_Deviation_[iRow, stU.IndexOf(stU["num_out_max_rng"])] is int)
						{
							num_out_max = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_out_max_rng"])];
						}

						// формула: (num_out_max_plus + num_out_max_minus)/num_not_marked
						num_not_marked = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
						switch (phase)
						{
							case "A+":
							case "A-": t2_a += num_out_max; break;
							case "B+":
							case "B-": t2_b += num_out_max; break;
							case "C+":
							case "C-": t2_c += num_out_max; break;
						}
					}
					// num_not_marked не разбивается по фазам, поэтому можно использовать число из последней строки
					if (num_not_marked != 0)
					{
						t2_a /= num_not_marked;
						t2_b /= num_not_marked;
						t2_c /= num_not_marked;

						t2_a = (float)Math.Round(t2_a * 100, settings_.FloatSigns);
						t2_b = (float)Math.Round(t2_b * 100, settings_.FloatSigns);
						t2_c = (float)Math.Round(t2_c * 100, settings_.FloatSigns);

						rt = rt.Replace("{dUAT2}", t2_a.ToString(fl));
						rt = rt.Replace("{dUBT2}", t2_b.ToString(fl));
						rt = rt.Replace("{dUCT2}", t2_c.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{dUAT2}", "-");
						rt = rt.Replace("{dUBT2}", "-");
						rt = rt.Replace("{dUCT2}", "-");
					}
				}

				#region Peak Load Mode

				// режимы наибольших и наименьших нагрузок для В.2
				if (protocolType_ == PQPProtocolType.VERSION_2 || protocolType_ == PQPProtocolType.VERSION_3)
				{
					// for version 2
					float t2_a_min = 0, t2_b_min = 0, t2_c_min = 0;
					float t2_a_max = 0, t2_b_max = 0, t2_c_max = 0;
					// for version 3
					float t2_a_min_pos = 0, t2_b_min_pos = 0, t2_c_min_pos = 0;
					float t2_a_max_pos = 0, t2_b_max_pos = 0, t2_c_max_pos = 0;
					float t2_a_min_neg = 0, t2_b_min_neg = 0, t2_c_min_neg = 0;
					float t2_a_max_neg = 0, t2_b_max_neg = 0, t2_c_max_neg = 0;

					int num_not_marked_a_min_pos = 0, num_not_marked_b_min_pos = 0, num_not_marked_c_min_pos = 0;
					int num_not_marked_a_max_pos = 0, num_not_marked_b_max_pos = 0, num_not_marked_c_max_pos = 0;
					int num_not_marked_a_min_neg = 0, num_not_marked_b_min_neg = 0, num_not_marked_c_min_neg = 0;
					int num_not_marked_a_max_neg = 0, num_not_marked_b_max_neg = 0, num_not_marked_c_max_neg = 0;

					string cur_mode_suffix;
					for (int iRow = 0; iRow < rows; iRow++)
					{
						string row_name = (dgU_Deviation_[iRow, 0] as string).TrimEnd();
						// отбрасываем строки, не касающиеся наибольших и наименьших
						if (!row_name.Contains('\'') && !row_name.Contains('"')) continue;

						if (row_name.Contains('\'')) cur_mode_suffix = "_max"; // режим наименьших нагрузок
						else cur_mode_suffix = "_min"; // режим наибольших нагрузок

						// суфикс для Фазы. A, B, C, AB, BC, CA и 
						// string.Empty для прямой последовательности
						string phase = string.Empty;

						// разделяем строку массив по символу "_"
						string[] parts = row_name.Split(new string[] { "_" },
							StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length > 2) parts[2] = (parts[2][0]).ToString();
						if (parts.Length > 2) phase = parts[1] + parts[2];
						else phase = parts[1];

						if (phase.Contains("AB+")) phase = "A+";
						else if (phase.Contains("BC+")) phase = "B+";
						else if (phase.Contains("CA+")) phase = "C+";
						else if (phase.Contains("AB-")) phase = "A-";
						else if (phase.Contains("BC-")) phase = "B-";
						else if (phase.Contains("CA-")) phase = "C-";

						float val = 0;
						int num_out_max = 0;
						if (dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])] is string)
						{
							if (!dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])].ToString().Equals("-"))
							{
								Conversions.object_2_float_en_ru(
									dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])],
									out val);
								string str_val = val.ToString(fl);

								//string tmp = string.Format("dU{0}r{1}", phase, cur_mode_suffix);
								rt = rt.Replace("{" + string.Format("dU{0}r{1}", phase, cur_mode_suffix) + "}", str_val);
							}
						}
						else if (dgU_Deviation_[iRow, stU.IndexOf(stU["calc_nrm_rng_top"])] is float)
						{
							val = (float)dgU_Deviation_[iRow, stU.IndexOf(stU["calc_max_rng_top"])];
							rt = rt.Replace("{" + string.Format("dU{0}r{1}", phase, cur_mode_suffix) + "}",
																		val.ToString(fl));
						}

						if (phase.Contains('+'))
							rt = rt.Replace("{" + string.Format("dU{0}n{1}", phase, cur_mode_suffix) + "}",
								((float)dgU_Deviation_[iRow,
								stU.IndexOf(stU["real_max_rng_top"])]).ToString(fl));
						else
							rt = rt.Replace("{" + string.Format("dU{0}n{1}", phase, cur_mode_suffix) + "}",
								((float)dgU_Deviation_[iRow,
								stU.IndexOf(stU["real_max_rng_bottom"])]).ToString(fl));

						// это поле нужно только для вычисления T2, в отчет оно не идет
						if (dgU_Deviation_[iRow, stU.IndexOf(stU["num_out_max_rng"])] is int)
						{
							num_out_max = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_out_max_rng"])];
						}

						// формула: (num_out_max_plus + num_out_max_minus)/num_not_marked
						switch (phase)
						{
							case "A+":
								if (row_name.Contains('"'))
								{
									t2_a_min_pos += num_out_max;
									num_not_marked_a_min_pos = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								else
								{
									t2_a_max_pos += num_out_max;
									num_not_marked_a_max_pos = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								break;
							case "A-":
								if (row_name.Contains('"'))
								{
									t2_a_min_neg += num_out_max;
									num_not_marked_a_min_neg = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								else
								{
									t2_a_max_neg += num_out_max;
									num_not_marked_a_max_neg = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								break;
							case "B+":
								if (row_name.Contains('"'))
								{
									t2_b_min_pos += num_out_max;
									num_not_marked_b_min_pos = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								else
								{
									t2_b_max_pos += num_out_max;
									num_not_marked_b_max_pos = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								break;
							case "B-":
								if (row_name.Contains('"'))
								{
									t2_b_min_neg += num_out_max;
									num_not_marked_b_min_neg = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								else
								{
									t2_b_max_neg += num_out_max;
									num_not_marked_b_max_neg = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								break;
							case "C+":
								if (row_name.Contains('"'))
								{
									t2_c_min_pos += num_out_max;
									num_not_marked_c_min_pos = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								else
								{
									t2_c_max_pos += num_out_max;
									num_not_marked_c_max_pos = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								break;
							case "C-":
								if (row_name.Contains('"'))
								{
									t2_c_min_neg += num_out_max;
									num_not_marked_c_min_neg = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								else
								{
									t2_c_max_neg += num_out_max;
									num_not_marked_c_max_neg = (int)dgU_Deviation_[iRow, stU.IndexOf(stU["num_not_marked"])];
								}
								break;
						}
					}

					if (protocolType_ == PQPProtocolType.VERSION_2)
					{
						t2_a_min = t2_a_min_pos + t2_a_min_neg;
						t2_b_min = t2_b_min_pos + t2_b_min_neg;
						t2_c_min = t2_c_min_pos + t2_c_min_neg;

						t2_a_max = t2_a_max_pos + t2_a_max_neg;
						t2_b_max = t2_b_max_pos + t2_b_max_neg;
						t2_c_max = t2_c_max_pos + t2_c_max_neg;

						// num_not_marked не разбивается по фазам, поэтому можно использовать число из любой фазы
						if ((num_not_marked_a_min_pos + num_not_marked_a_min_neg) != 0)
						{
							if ((num_not_marked_a_min_pos + num_not_marked_a_min_neg) != 0)
								t2_a_min /= (num_not_marked_a_min_pos + num_not_marked_a_min_neg);
							if ((num_not_marked_b_min_pos + num_not_marked_b_min_neg) != 0)
								t2_b_min /= (num_not_marked_b_min_pos + num_not_marked_b_min_neg);
							if ((num_not_marked_c_min_pos + num_not_marked_c_min_neg) != 0)
								t2_c_min /= (num_not_marked_c_min_pos + num_not_marked_c_min_neg);

							t2_a_min = (float)Math.Round(t2_a_min * 100, settings_.FloatSigns);
							t2_b_min = (float)Math.Round(t2_b_min * 100, settings_.FloatSigns);
							t2_c_min = (float)Math.Round(t2_c_min * 100, settings_.FloatSigns);

							if (!Single.IsNaN(t2_a_min)) rt = rt.Replace("{dUAT2_min}", t2_a_min.ToString(fl));
							else rt = rt.Replace("{dUAT2_min}", "-");
							if (!Single.IsNaN(t2_b_min)) rt = rt.Replace("{dUBT2_min}", t2_b_min.ToString(fl));
							else rt = rt.Replace("{dUBT2_min}", "-");
							if (!Single.IsNaN(t2_c_min)) rt = rt.Replace("{dUCT2_min}", t2_c_min.ToString(fl));
							else rt = rt.Replace("{dUCT2_min}", "-");
						}
						else
						{
							rt = rt.Replace("{dUAT2_min}", "-");
							rt = rt.Replace("{dUBT2_min}", "-");
							rt = rt.Replace("{dUCT2_min}", "-");
						}

						if ((num_not_marked_a_max_pos + num_not_marked_a_max_neg) != 0)
						{
							if ((num_not_marked_a_max_pos + num_not_marked_a_max_neg) != 0)
								t2_a_max /= (num_not_marked_a_max_pos + num_not_marked_a_max_neg);
							if ((num_not_marked_b_max_pos + num_not_marked_b_max_neg) != 0)
								t2_b_max /= (num_not_marked_b_max_pos + num_not_marked_b_max_neg);
							if ((num_not_marked_c_max_pos + num_not_marked_c_max_neg) != 0)
								t2_c_max /= (num_not_marked_c_max_pos + num_not_marked_c_max_neg);

							t2_a_max = (float)Math.Round(t2_a_max * 100, settings_.FloatSigns);
							t2_b_max = (float)Math.Round(t2_b_max * 100, settings_.FloatSigns);
							t2_c_max = (float)Math.Round(t2_c_max * 100, settings_.FloatSigns);

							if (!Single.IsNaN(t2_a_max)) rt = rt.Replace("{dUAT2_max}", t2_a_max.ToString(fl));
							else rt = rt.Replace("{dUAT2_max}", "-");
							if (!Single.IsNaN(t2_b_max)) rt = rt.Replace("{dUBT2_max}", t2_b_max.ToString(fl));
							else rt = rt.Replace("{dUBT2_max}", "-");
							if (!Single.IsNaN(t2_c_max)) rt = rt.Replace("{dUCT2_max}", t2_c_max.ToString(fl));
							else rt = rt.Replace("{dUCT2_max}", "-");
						}
						else
						{
							rt = rt.Replace("{dUAT2_max}", "-");
							rt = rt.Replace("{dUBT2_max}", "-");
							rt = rt.Replace("{dUCT2_max}", "-");
						}
					}

					if (protocolType_ == PQPProtocolType.VERSION_3)
					{
						if (num_not_marked_a_min_pos != 0)
						{
							if (num_not_marked_a_min_pos != 0) t2_a_min_pos /= num_not_marked_a_min_pos;
							if (num_not_marked_b_min_pos != 0) t2_b_min_pos /= num_not_marked_b_min_pos;
							if (num_not_marked_c_min_pos != 0) t2_c_min_pos /= num_not_marked_c_min_pos;

							t2_a_min_pos = (float)Math.Round(t2_a_min_pos * 100, settings_.FloatSigns);
							t2_b_min_pos = (float)Math.Round(t2_b_min_pos * 100, settings_.FloatSigns);
							t2_c_min_pos = (float)Math.Round(t2_c_min_pos * 100, settings_.FloatSigns);

							if (!Single.IsNaN(t2_a_min_pos)) rt = rt.Replace("{dUA+T2_min}", t2_a_min_pos.ToString(fl));
							else rt = rt.Replace("{dUA+T2_min}", "-");
							if (!Single.IsNaN(t2_b_min_pos)) rt = rt.Replace("{dUB+T2_min}", t2_b_min_pos.ToString(fl));
							else rt = rt.Replace("{dUB+T2_min}", "-");
							if (!Single.IsNaN(t2_c_min_pos)) rt = rt.Replace("{dUC+T2_min}", t2_c_min_pos.ToString(fl));
							else rt = rt.Replace("{dUC+T2_min}", "-");
						}
						else
						{
							rt = rt.Replace("{dUA+T2_min}", "-");
							rt = rt.Replace("{dUB+T2_min}", "-");
							rt = rt.Replace("{dUC+T2_min}", "-");
						}

						if (num_not_marked_a_min_neg != 0)
						{
							if (num_not_marked_a_min_neg != 0) t2_a_min_neg /= num_not_marked_a_min_neg;
							if (num_not_marked_b_min_neg != 0) t2_b_min_neg /= num_not_marked_b_min_neg;
							if (num_not_marked_c_min_neg != 0) t2_c_min_neg /= num_not_marked_c_min_neg;

							t2_a_min_neg = (float)Math.Round(t2_a_min_neg * 100, settings_.FloatSigns);
							t2_b_min_neg = (float)Math.Round(t2_b_min_neg * 100, settings_.FloatSigns);
							t2_c_min_neg = (float)Math.Round(t2_c_min_neg * 100, settings_.FloatSigns);

							if (!Single.IsNaN(t2_a_min_neg)) rt = rt.Replace("{dUA-T2_min}", t2_a_min_neg.ToString(fl));
							else rt = rt.Replace("{dUA-T2_min}", "-");
							if (!Single.IsNaN(t2_b_min_neg)) rt = rt.Replace("{dUB-T2_min}", t2_b_min_neg.ToString(fl));
							else rt = rt.Replace("{dUB-T2_min}", "-");
							if (!Single.IsNaN(t2_c_min_neg)) rt = rt.Replace("{dUC-T2_min}", t2_c_min_neg.ToString(fl));
							else rt = rt.Replace("{dUC-T2_min}", "-");
						}
						else
						{
							rt = rt.Replace("{dUA-T2_min}", "-");
							rt = rt.Replace("{dUB-T2_min}", "-");
							rt = rt.Replace("{dUC-T2_min}", "-");
						}

						if (num_not_marked_a_max_pos != 0)
						{
							if (num_not_marked_a_max_pos != 0) t2_a_max_pos /= num_not_marked_a_max_pos;
							if (num_not_marked_b_max_pos != 0) t2_b_max_pos /= num_not_marked_b_max_pos;
							if (num_not_marked_c_max_pos != 0) t2_c_max_pos /= num_not_marked_c_max_pos;

							t2_a_max_pos = (float)Math.Round(t2_a_max_pos * 100, settings_.FloatSigns);
							t2_b_max_pos = (float)Math.Round(t2_b_max_pos * 100, settings_.FloatSigns);
							t2_c_max_pos = (float)Math.Round(t2_c_max_pos * 100, settings_.FloatSigns);

							if (!Single.IsNaN(t2_a_max_pos)) rt = rt.Replace("{dUA+T2_max}", t2_a_max_pos.ToString(fl));
							else rt = rt.Replace("{dUA+T2_max}", "-");
							if (!Single.IsNaN(t2_b_max_pos)) rt = rt.Replace("{dUB+T2_max}", t2_b_max_pos.ToString(fl));
							else rt = rt.Replace("{dUB+T2_max}", "-");
							if (!Single.IsNaN(t2_c_max_pos)) rt = rt.Replace("{dUC+T2_max}", t2_c_max_pos.ToString(fl));
							else rt = rt.Replace("{dUC+T2_max}", "-");
						}
						else
						{
							rt = rt.Replace("{dUA+T2_max}", "-");
							rt = rt.Replace("{dUB+T2_max}", "-");
							rt = rt.Replace("{dUC+T2_max}", "-");
						}

						if (num_not_marked_a_max_neg != 0)
						{
							if (num_not_marked_a_max_neg != 0) t2_a_max_neg /= num_not_marked_a_max_neg;
							if (num_not_marked_b_max_neg != 0) t2_b_max_neg /= num_not_marked_b_max_neg;
							if (num_not_marked_c_max_neg != 0) t2_c_max_neg /= num_not_marked_c_max_neg;

							t2_a_max_neg = (float)Math.Round(t2_a_max_neg * 100, settings_.FloatSigns);
							t2_b_max_neg = (float)Math.Round(t2_b_max_neg * 100, settings_.FloatSigns);
							t2_c_max_neg = (float)Math.Round(t2_c_max_neg * 100, settings_.FloatSigns);

							if (!Single.IsNaN(t2_a_max_neg)) rt = rt.Replace("{dUA-T2_max}", t2_a_max_neg.ToString(fl));
							else rt = rt.Replace("{dUA-T2_max}", "-");
							if (!Single.IsNaN(t2_b_max_neg)) rt = rt.Replace("{dUB-T2_max}", t2_b_max_neg.ToString(fl));
							else rt = rt.Replace("{dUB-T2_max}", "-");
							if (!Single.IsNaN(t2_c_max_neg)) rt = rt.Replace("{dUC-T2_max}", t2_c_max_neg.ToString(fl));
							else rt = rt.Replace("{dUC-T2_max}", "-");
						}
						else
						{
							rt = rt.Replace("{dUA-T2_max}", "-");
							rt = rt.Replace("{dUB-T2_max}", "-");
							rt = rt.Replace("{dUC-T2_max}", "-");
						}
					}
				}

				#endregion

				// Погрешности dUy
				rt = rt.Replace("{ddUр}", "±0,1% (абс.)");
				rt = rt.Replace("{ddUн}", "±0,1%");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportUDeviation():");
				throw;
			}
		}

		protected void ExportNonsymmetry(ref string rt, string fl)
		{
			try
			{
				#region Коэффициент нессиметрии прямой последовательности

				// k2u
				if (connectScheme_ != ConnectScheme.Ph1W2)
				{
					GridColumnStylesCollection stK2U = dgNonSymmetry_.TableStyles[0].GridColumnStyles;
					//st.IndexOf(st["calc_nrm_rng"])

					//rt = rt.Replace("{K2Uвр}",
					//	(Conversions.object_2_double(dgNonSymmetry[0, 
					//	st10.IndexOf(st10["calc_nrm_rng"])])).ToString(fl));
					if (!dgNonSymmetry_[0,
							stK2U.IndexOf(stK2U["calc_nrm_rng"])].ToString().Equals("-"))
					{
						double val = Conversions.object_2_double(
								dgNonSymmetry_[0,
								stK2U.IndexOf(stK2U["calc_nrm_rng"])]);
						rt = rt.Replace("{K2Uвр}", val.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{K2Uвр}", "-");
					}

					if (!dgNonSymmetry_[0,
							stK2U.IndexOf(stK2U["calc_max_rng"])].ToString().Equals("-"))
					{
						double val = Conversions.object_2_double(
								dgNonSymmetry_[0,
								stK2U.IndexOf(stK2U["calc_max_rng"])]);
						rt = rt.Replace("{K2Uнбр}", val.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{K2Uнбр}", "-");
					}

					rt = rt.Replace("{K2Uвн}",
						(Conversions.object_2_double(dgNonSymmetry_[0,
						stK2U.IndexOf(stK2U["real_nrm_rng"])])).ToString(fl));
					rt = rt.Replace("{K2Uнбн}",
						(Conversions.object_2_double(dgNonSymmetry_[0,
						stK2U.IndexOf(stK2U["real_max_rng"])])).ToString(fl));

					ReplaceExcelName_EtPQP_A(ref rt, dgNonSymmetry_,
						"K2UвT", "num_max_rng", "num_out_max_rng", "num_not_marked",
						0, fl, stK2U);

					ReplaceExcelName_EtPQP_A(ref rt, dgNonSymmetry_,
						"K2UнбT", "num_out_max_rng", "num_not_marked",
						0, fl, stK2U);

					// Погрешности k2u
					rt = rt.Replace("{dK2Uр}", "±0,15% (абс.)");
					rt = rt.Replace("{dK2Uн}", "±0,15%");
				}

				#endregion

				#region Коэффициент нессиметрии обратной последовательности

				// k0u
				if (connectScheme_ == ConnectScheme.Ph3W4 ||
					connectScheme_ == ConnectScheme.Ph3W4_B_calc)
				{
					GridColumnStylesCollection stK0u = dgNonSymmetry_.TableStyles[0].GridColumnStyles;
					//st.IndexOf(st[""]);

					//rt = rt.Replace("{K0Uвр}",
					//	(Conversions.object_2_double(dgNonSymmetry[1, 
					//	st2.IndexOf(st2["calc_nrm_rng"])])).ToString(fl));
					if (!dgNonSymmetry_[1,
							stK0u.IndexOf(stK0u["calc_nrm_rng"])].ToString().Equals("-"))
					{
						double val = Conversions.object_2_double(
								dgNonSymmetry_[1,
								stK0u.IndexOf(stK0u["calc_nrm_rng"])]);
						rt = rt.Replace("{K0Uвр}", val.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{K0Uвр}", "-");
					}

					if (!dgNonSymmetry_[1,
							stK0u.IndexOf(stK0u["calc_max_rng"])].ToString().Equals("-"))
					{
						double val = Conversions.object_2_double(
								dgNonSymmetry_[1,
								stK0u.IndexOf(stK0u["calc_max_rng"])]);
						rt = rt.Replace("{K0Uнбр}", val.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{K0Uнбр}", "-");
					}

					rt = rt.Replace("{K0Uвн}",
						(Conversions.object_2_double(dgNonSymmetry_[1,
						stK0u.IndexOf(stK0u["real_nrm_rng"])])).ToString(fl));
					rt = rt.Replace("{K0Uнбн}",
						(Conversions.object_2_double(dgNonSymmetry_[1,
						stK0u.IndexOf(stK0u["real_max_rng"])])).ToString(fl));

					ReplaceExcelName_EtPQP_A(ref rt, dgNonSymmetry_,
						"K0UвT", "num_max_rng", "num_out_max_rng", "num_not_marked",
						1, fl, stK0u);

					ReplaceExcelName_EtPQP_A(ref rt, dgNonSymmetry_,
						"K0UнбT", "num_out_max_rng", "num_not_marked",
						1, fl, stK0u);

					// Погрешности k0u
					rt = rt.Replace("{dK0Uр}", "±0,15% (абс.)");
					rt = rt.Replace("{dK0Uн}", "±0,15%");
				}

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportNonsymmetry():");
				throw;
			}
		}

		protected void ExportNonsinus(ref string rt, string fl)
		{
			try
			{
				DataGrid curDgNonsinus = dgUNonsinusoidality_;
				if (connectScheme_ == ConnectScheme.Ph3W3 || connectScheme_ == ConnectScheme.Ph3W3_B_calc)
					curDgNonsinus = dgUNonsinusoidality2_;
				GridColumnStylesCollection stKu = curDgNonsinus.TableStyles[0].GridColumnStyles;
				//GridColumnStylesCollection st_2 =
				//    dgUNonsinusoidality2.TableStyles[0].GridColumnStyles;

				// проверяем есть ли строки в таблице
				int rowCountVoltageNonsinusoidality = 1;
				try
				{
					DataSet tmpDataSet = (DataSet)curDgNonsinus.DataSource;
					rowCountVoltageNonsinusoidality = tmpDataSet.Tables[0].Rows.Count;
				}
				catch (Exception excount)
				{
					EmService.DumpException(excount, "Error in dgUNonsinusoidality Row Count");
				}

				if (rowCountVoltageNonsinusoidality > 0)
				{
					//rt = rt.Replace("{KUAвр}",
					//    (Conversions.object_2_double(dgUNonsinusoidality[0,
					//    st.IndexOf(st["calc_nrm_rng_ph1"])])).ToString(fl));
					if (!curDgNonsinus[0,
						stKu.IndexOf(stKu["calc_nrm_rng_ph1"])].ToString().Equals("-"))
					{
						double val = Conversions.object_2_double(
							curDgNonsinus[0,
							stKu.IndexOf(stKu["calc_nrm_rng_ph1"])]);
						rt = rt.Replace("{KUAвр}", val.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{KUAвр}", "-");
					}

					if (!curDgNonsinus[0,
						stKu.IndexOf(stKu["calc_max_rng_ph1"])].ToString().Equals("-"))
					{
						double val = Conversions.object_2_double(
							curDgNonsinus[0,
							stKu.IndexOf(stKu["calc_max_rng_ph1"])]);
						rt = rt.Replace("{KUAнбр}", val.ToString(fl));
					}
					else
					{
						rt = rt.Replace("{KUAнбр}", "-");
					}

					ReplaceExcelName_EtPQP_A(ref rt, curDgNonsinus,
						"KUAвT", "num_max_rng_ph1", "num_out_max_rng_ph1", "num_not_marked",
						0, fl, stKu);

					ReplaceExcelName_EtPQP_A(ref rt, curDgNonsinus,
						"KUAнбT", "num_out_max_rng_ph1", "num_not_marked",
						0, fl, stKu);

					rt = rt.Replace("{KUвн}",
						(Conversions.object_2_double(curDgNonsinus[0,
						stKu.IndexOf(stKu["real_nrm_rng"])])).ToString(fl));
					rt = rt.Replace("{KUнбн}",
						(Conversions.object_2_double(curDgNonsinus[0,
						stKu.IndexOf(stKu["real_max_rng"])])).ToString(fl));

					if (connectScheme_ != ConnectScheme.Ph1W2)
					{
						//rt = rt.Replace("{KUBвр}",
						//    (Conversions.object_2_double(dgUNonsinusoidality[0,
						//    st.IndexOf(st["calc_nrm_rng_ph2"])])).ToString(fl));
						if (!curDgNonsinus[0,
							stKu.IndexOf(stKu["calc_nrm_rng_ph2"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(curDgNonsinus[0,
								stKu.IndexOf(stKu["calc_nrm_rng_ph2"])]);
							rt = rt.Replace("{KUBвр}", val.ToString(fl));
						}
						else
						{
							rt = rt.Replace("{KUBвр}", "-");
						}

						if (!curDgNonsinus[0,
							stKu.IndexOf(stKu["calc_max_rng_ph2"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(curDgNonsinus[0,
								stKu.IndexOf(stKu["calc_max_rng_ph2"])]);
							rt = rt.Replace("{KUBнбр}", val.ToString(fl));
						}
						else
						{
							rt = rt.Replace("{KUBнбр}", "-");
						}

						ReplaceExcelName_EtPQP_A(ref rt, curDgNonsinus,
							"KUBвT", "num_max_rng_ph2", "num_out_max_rng_ph2", "num_not_marked",
							0, fl, stKu);

						ReplaceExcelName_EtPQP_A(ref rt, curDgNonsinus,
							"KUBнбT", "num_out_max_rng_ph2", "num_not_marked",
							0, fl, stKu);

						//rt = rt.Replace("{KUCвр}",
						//    (Conversions.object_2_double(dgUNonsinusoidality[0,
						//    st.IndexOf(st["calc_nrm_rng_ph3"])])).ToString(fl));
						if (!curDgNonsinus[0,
							stKu.IndexOf(stKu["calc_nrm_rng_ph3"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(curDgNonsinus[0,
								stKu.IndexOf(stKu["calc_nrm_rng_ph3"])]);
							rt = rt.Replace("{KUCвр}", val.ToString(fl));
						}
						else
						{
							rt = rt.Replace("{KUCвр}", "-");
						}

						if (!curDgNonsinus[0,
							stKu.IndexOf(stKu["calc_max_rng_ph3"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(curDgNonsinus[0,
								stKu.IndexOf(stKu["calc_max_rng_ph3"])]);
							rt = rt.Replace("{KUCнбр}", val.ToString(fl));
						}
						else
						{
							rt = rt.Replace("{KUCнбр}", "-");
						}

						ReplaceExcelName_EtPQP_A(ref rt, curDgNonsinus,
							"KUCвT", "num_max_rng_ph3", "num_out_max_rng_ph3", "num_not_marked",
							0, fl, stKu);

						ReplaceExcelName_EtPQP_A(ref rt, curDgNonsinus,
							"KUCнбT", "num_out_max_rng_ph3", "num_not_marked",
							0, fl, stKu);
					}
				}

				// Погрешности kU (A, B, C)
				//rt = rt.Replace("{dKUр}", "±0,05% (абс.) при <= 1%,\n ±5% (отн.) при > 1%");
				rt = rt.Replace("{dKUн}", "±0,05% или ±5%");

				#region Коэффициэнты гармонических составляющих

				if (rowCountVoltageNonsinusoidality > 0)
				{
					// KuN ( N = 2..40 )
					for (int i = 2; i < 41; i++)
					{
						//rt = rt.Replace("{kua" + i.ToString() + "в}",
						//    (Conversions.object_2_double(dgUNonsinusoidality[i - 1,
						//    st.IndexOf(st["calc_nrm_rng_ph1"])
						//    ])).ToString(fl));
						if (!curDgNonsinus[i - 1,
							stKu.IndexOf(stKu["calc_nrm_rng_ph1"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(
								curDgNonsinus[i - 1,
								stKu.IndexOf(stKu["calc_nrm_rng_ph1"])]);
							//rt = rt.Replace("{kua" + i.ToString() + "в}", val.ToString(fl));
							ReplaceTextForReport(ref rt, "{kua" + i.ToString() + "в}", val, fl);
						}
						else
						{
							rt = rt.Replace("{kua" + i.ToString() + "в}", "-");
						}

						if (!curDgNonsinus[i - 1,
							stKu.IndexOf(stKu["calc_max_rng_ph1"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(
								curDgNonsinus[i - 1,
								stKu.IndexOf(stKu["calc_max_rng_ph1"])]);
							//rt = rt.Replace("{kua" + i.ToString() + "нб}", val.ToString(fl));
							ReplaceTextForReport(ref rt, "{kua" + i.ToString() + "нб}", val, fl);
						}
						else
						{
							rt = rt.Replace("{kua" + i.ToString() + "нб}", "-");
						}

						ReplaceExcelNameHarm_PQP_A(ref rt, curDgNonsinus,
							"kua", i.ToString(), "T1",
							"num_max_rng_ph1", "num_out_max_rng_ph1", "num_not_marked",
							i - 1, fl, stKu);
						ReplaceExcelNameHarm_PQP_A(ref rt, curDgNonsinus,
							"kua", i.ToString(), "T2",
							"num_out_max_rng_ph1", "num_not_marked",
							i - 1, fl, stKu);

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (!curDgNonsinus[i - 1,
							stKu.IndexOf(stKu["calc_nrm_rng_ph2"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(
									curDgNonsinus[i - 1,
									stKu.IndexOf(stKu["calc_nrm_rng_ph2"])]);
								rt = rt.Replace("{kub" + i.ToString() + "в}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{kub" + i.ToString() + "в}", "-");
							}

							if (!curDgNonsinus[i - 1,
							stKu.IndexOf(stKu["calc_max_rng_ph2"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(
									curDgNonsinus[i - 1,
									stKu.IndexOf(stKu["calc_max_rng_ph2"])]);
								rt = rt.Replace("{kub" + i.ToString() + "нб}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{kub" + i.ToString() + "нб}", "-");
							}

							ReplaceExcelNameHarm_PQP_A(ref rt, curDgNonsinus,
								"kub", i.ToString(), "T1",
								"num_max_rng_ph2", "num_out_max_rng_ph2", "num_not_marked",
								i - 1, fl, stKu);
							ReplaceExcelNameHarm_PQP_A(ref rt, curDgNonsinus,
								"kub", i.ToString(), "T2",
								"num_out_max_rng_ph2", "num_not_marked",
								i - 1, fl, stKu);

							if (!curDgNonsinus[i - 1,
								stKu.IndexOf(stKu["calc_nrm_rng_ph3"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(
									curDgNonsinus[i - 1,
									stKu.IndexOf(stKu["calc_nrm_rng_ph3"])]);
								rt = rt.Replace("{kuc" + i.ToString() + "в}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{kuc" + i.ToString() + "в}", "-");
							}

							if (!curDgNonsinus[i - 1,
								stKu.IndexOf(stKu["calc_max_rng_ph3"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(
									curDgNonsinus[i - 1,
									stKu.IndexOf(stKu["calc_max_rng_ph3"])]);
								rt = rt.Replace("{kuc" + i.ToString() + "нб}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{kuc" + i.ToString() + "нб}", "-");
							}

							ReplaceExcelNameHarm_PQP_A(ref rt, curDgNonsinus,
								"kuc", i.ToString(), "T1",
								"num_max_rng_ph3", "num_out_max_rng_ph3", "num_not_marked",
								i - 1, fl, stKu);
							ReplaceExcelNameHarm_PQP_A(ref rt, curDgNonsinus,
								"kuc", i.ToString(), "T2",
								"num_out_max_rng_ph3", "num_not_marked",
								i - 1, fl, stKu);
						}

						//if (connectScheme != ConnectScheme.Ph3W3 &&
						//    connectScheme != ConnectScheme.Ph3W3_B_calc)
						//{
						rt = rt.Replace("{ku" + i.ToString() + "вн}",
							(Conversions.object_2_double(curDgNonsinus[i - 1,
							stKu.IndexOf(stKu["real_nrm_rng"])])).ToString(fl));
						rt = rt.Replace("{ku" + i.ToString() + "нбн}",
							(Conversions.object_2_double(curDgNonsinus[i - 1,
							stKu.IndexOf(stKu["real_max_rng"])
							])).ToString(fl));
						//}
					}
				}
				// Погрешности KuN ( N = 2..40 )
				//rt = rt.Replace("{dKUNр}", "±0,05% (абс.) при <= 1%,\n ±5% (отн.) при > 1%");
				rt = rt.Replace("{dKUNн}", "±0,05% или ±5%");

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportNonsinus():");
				throw;
			}
		}

		protected void ExportDipAndSwell_OLD(ref string rt, string fl)
		{
			try
			{
				GridColumnStylesCollection stDip = dgDips_.TableStyles[0].GridColumnStyles;
				GridColumnStylesCollection stSwell = dgOvers_.TableStyles[0].GridColumnStyles;

				// провалы
				// number of rows in the dip table
				int rows = (dgDips_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

				// перебираем все номера
				int ival = 0;
				for (int i = 0; i < rows; i++)
				{
					string row_name = (dgDips_[i, 0] as string).TrimEnd();

					if (row_name.Contains("90") && row_name.Contains("85")) row_name = "{dip90_85_";
					else if (row_name.Contains("85") && row_name.Contains("70")) row_name = "{dip85_70_";
					else if (row_name.Contains("70") && row_name.Contains("40")) row_name = "{dip70_40_";
					else if (row_name.Contains("40") && row_name.Contains("10")) row_name = "{dip40_10_";
					else if (row_name.Contains("10") && row_name.Contains("5")) row_name = "{dip10_5_";
					else if (row_name.Contains("5") && row_name.Contains("0")) row_name = "{dip5_0_";

					int temp = stDip.IndexOf(stDip["num_0_01_till_0_05"]);
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_01_till_0_05"])]);
					rt = rt.Replace(row_name + "1}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_05_till_0_1"])]);
					rt = rt.Replace(row_name + "2}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_1_till_0_5"])]);
					rt = rt.Replace(row_name + "3}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_5_till_1"])]);
					rt = rt.Replace(row_name + "4}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_1_till_3"])]);
					rt = rt.Replace(row_name + "5}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_3_till_20"])]);
					rt = rt.Replace(row_name + "6}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_20_till_60"])]);
					rt = rt.Replace(row_name + "7}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_over_60"])]);
					rt = rt.Replace(row_name + "8}", ival.ToString());

					try
					{
						ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_over_180"])]);
						rt = rt.Replace(row_name + "9}", ival.ToString());
					}
					catch { rt = rt.Replace(row_name + "9}", "-"); }
				}

				// swells
				// number of rows in the swell table
				rows = (dgOvers_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

				// перебираем все номера
				for (int i = 0; i < rows; i++)
				{
					string row_name = (dgOvers_[i, 0] as string).TrimEnd();

					if (row_name.Contains("110") && row_name.Contains("112")) row_name = "{over110_112_";
					else if (row_name.Contains("112") && row_name.Contains("115")) row_name = "{over112_115_";
					else if (row_name.Contains("115") && row_name.Contains("120")) row_name = "{over115_120_";
					else if (row_name.Contains("120") && row_name.Contains("150")) row_name = "{over120_150_";

					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_01_till_0_05"])]);
					rt = rt.Replace(row_name + "1}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_05_till_0_1"])]);
					rt = rt.Replace(row_name + "2}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_1_till_0_5"])]);
					rt = rt.Replace(row_name + "3}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_5_till_1"])]);
					rt = rt.Replace(row_name + "4}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_1_till_3"])]);
					rt = rt.Replace(row_name + "5}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_3_till_20"])]);
					rt = rt.Replace(row_name + "6}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_20_till_60"])]);
					rt = rt.Replace(row_name + "7}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_over_60"])]);
					rt = rt.Replace(row_name + "8}", ival.ToString());

					try
					{
						ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_over_180"])]);
						rt = rt.Replace(row_name + "9}", ival.ToString());
					}
					catch { rt = rt.Replace(row_name + "9}", "-"); }
				}
			}
			catch (Exception ex)
            {
				EmService.DumpException(ex, "Error in ExportDipAndSwell():");
                throw;
            }
		}

		protected void ExportDipAndSwell_GOST2014(ref string rt, string fl)
		{
			try
			{
				GridColumnStylesCollection stDip = dgDips_.TableStyles[0].GridColumnStyles;
				GridColumnStylesCollection stDip2 = dgDips2_.TableStyles[0].GridColumnStyles; // interruptions
				GridColumnStylesCollection stSwell = dgOvers_.TableStyles[0].GridColumnStyles;

				// провалы
				// number of rows in the dip table
				int rows = (dgDips_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

				// перебираем все номера
				int ival = 0;
				for (int i = 0; i < rows; i++)
				{
					string row_name = (dgDips_[i, 0] as string).TrimEnd();

					if (row_name.Contains("90") && row_name.Contains("85")) row_name = "{dip90_85_";
					else if (row_name.Contains("85") && row_name.Contains("70")) row_name = "{dip85_70_";
					else if (row_name.Contains("70") && row_name.Contains("40")) row_name = "{dip70_40_";
					else if (row_name.Contains("40") && row_name.Contains("10")) row_name = "{dip40_10_";
					else if (row_name.Contains("10") && row_name.Contains("0")) row_name = "{dip10_5_";
					//else if (row_name.Contains("5") && row_name.Contains("0")) row_name = "{dip5_0_";

					int temp = stDip.IndexOf(stDip["num_0_01_till_0_05"]);
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_01_till_0_05"])]);
					rt = rt.Replace(row_name + "1}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_05_till_0_1"])]);
					rt = rt.Replace(row_name + "2}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_1_till_0_5"])]);
					rt = rt.Replace(row_name + "3}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_5_till_1"])]);
					rt = rt.Replace(row_name + "4}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_1_till_3"])]);
					rt = rt.Replace(row_name + "5}", ival.ToString());
					ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_3_till_20"])]);
					rt = rt.Replace(row_name + "6}", ival.ToString());
					//ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_20_till_60"])]);
					//rt = rt.Replace(row_name + "7}", ival.ToString());
					//ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_over_60"])]);
					//rt = rt.Replace(row_name + "8}", ival.ToString());

					//try
					//{
					//    ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_over_180"])]);
					//    rt = rt.Replace(row_name + "9}", ival.ToString());
					//}
					//catch { rt = rt.Replace(row_name + "9}", "-"); }
				}

				// interrupts
				if (dgDips2_ != null)
				{
					rows = (dgDips2_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;
					if (rows > 0)
					{
						string row_name = "{dip5_0_";
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_0_01_till_0_05"])]);
						rt = rt.Replace(row_name + "1}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_0_05_till_0_1"])]);
						rt = rt.Replace(row_name + "2}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_0_1_till_0_5"])]);
						rt = rt.Replace(row_name + "3}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_0_5_till_1"])]);
						rt = rt.Replace(row_name + "4}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_1_till_3"])]);
						rt = rt.Replace(row_name + "5}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_3_till_20"])]);
						rt = rt.Replace(row_name + "6}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_20_till_60"])]);
						rt = rt.Replace(row_name + "7}", ival.ToString());
						ival = Convert.ToInt32(dgDips2_[0, stDip2.IndexOf(stDip2["num_over_60"])]);
						rt = rt.Replace("{dip_max_len}", ival.ToString());
					}
				}
				else EmService.WriteToLogFailed("ExportDipSwell: dgDips2 = null");

				// swells
				// number of rows in the swell table
				rows = (dgOvers_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

				// перебираем все номера
				for (int i = 0; i < rows; i++)
				{
					string row_name = (dgOvers_[i, 0] as string).TrimEnd();

					if (row_name.Contains("110") && row_name.Contains("120")) row_name = "{over110_120_";
					else if (row_name.Contains("120") && row_name.Contains("140")) row_name = "{over120_140_";
					else if (row_name.Contains("140") && row_name.Contains("160")) row_name = "{over140_160_";
					else if (row_name.Contains("160") && row_name.Contains("180")) row_name = "{over160_180_";

					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_01_till_0_05"])]);
					rt = rt.Replace(row_name + "1}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_05_till_0_1"])]);
					rt = rt.Replace(row_name + "2}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_1_till_0_5"])]);
					rt = rt.Replace(row_name + "3}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_5_till_1"])]);
					rt = rt.Replace(row_name + "4}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_1_till_3"])]);
					rt = rt.Replace(row_name + "5}", ival.ToString());
					ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_3_till_20"])]);
					rt = rt.Replace(row_name + "6}", ival.ToString());
					//ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_20_till_60"])]);
					//rt = rt.Replace(row_name + "7}", ival.ToString());
					//ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_over_60"])]);
					//rt = rt.Replace(row_name + "8}", ival.ToString());

					//try
					//{
					//    ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_over_180"])]);
					//    rt = rt.Replace(row_name + "9}", ival.ToString());
					//}
					//catch { rt = rt.Replace(row_name + "9}", "-"); }
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportDipAndSwell():");
				throw;
			}
		}

		protected void ExportFlicker(ref string rt, string fl)
		{
			try
			{
				if (connectScheme_ != ConnectScheme.Ph3W3 && connectScheme_ != ConnectScheme.Ph3W3_B_calc)
				{
					GridColumnStylesCollection stFlick = dgFlickerNum_.TableStyles[0].GridColumnStyles;
					// number of rows in the flicker table
					int rows = (dgFlickerNum_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

					// перебираем все номера
					for (int i = 0; i < rows; i++)
					{
						string row_name = (dgFlickerNum_[i, 0] as string).TrimEnd();
						if (row_name == "ST")
							row_name = "f_st_";
						else row_name = "f_lt_";

						if (!dgFlickerNum_[i,
							stFlick.IndexOf(stFlick["calc_max_rng_ph1"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(
								dgFlickerNum_[i, stFlick.IndexOf(stFlick["calc_max_rng_ph1"])]);
							rt = rt.Replace("{" + row_name + "a}", val.ToString(fl));
						}
						else
						{
							rt = rt.Replace("{" + row_name + "a}", "-");
						}

						rt = rt.Replace("{" + row_name + "n}",
							(Conversions.object_2_double(dgFlickerNum_[i,
								stFlick.IndexOf(stFlick["real_max_rng"])])).ToString(fl));

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (!dgFlickerNum_[i,
								stFlick.IndexOf(stFlick["calc_max_rng_ph2"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(dgFlickerNum_[i,
									stFlick.IndexOf(stFlick["calc_max_rng_ph2"])]);
								rt = rt.Replace("{" + row_name + "b}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{" + row_name + "b}", "-");
							}

							if (!dgFlickerNum_[i,
								stFlick.IndexOf(stFlick["calc_max_rng_ph3"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(dgFlickerNum_[i,
									stFlick.IndexOf(stFlick["calc_max_rng_ph3"])]);
								rt = rt.Replace("{" + row_name + "c}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{" + row_name + "c}", "-");
							}
						}
					}

					// Погрешности
					//rt = rt.Replace("{dFst_r}", "±5,0 (отн.)");
					rt = rt.Replace("{dFst_n}", "±5,0");
					//rt = rt.Replace("{dFlt_r}", "±5,0 (отн.)");
					rt = rt.Replace("{dFlt_n}", "±5,0");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportFlicker():");
				throw;
			}
		}

		protected void ExportInterharm(ref string rt, string fl)
		{
			try
			{
				GridColumnStylesCollection stInter = dgInterharm_.TableStyles[0].GridColumnStyles;

				// проверяем есть ли строки в таблице
				int rowCountInterharm = 1;
				try
				{
					DataSet tmpDataSet = (DataSet)dgInterharm_.DataSource;
					rowCountInterharm = tmpDataSet.Tables[0].Rows.Count;
				}
				catch (Exception excount)
				{
					EmService.DumpException(excount, "Error in dgInterharm_ Row Count");
				}

				// if rows exist
				if (rowCountInterharm > 0)
				{
					for (int i = 1; i <= 41; i++)
					{
						if (!dgInterharm_[i - 1,
							stInter.IndexOf(stInter["val_ph1"])].ToString().Equals("-"))
						{
							double val = Conversions.object_2_double(
								dgInterharm_[i - 1,
								stInter.IndexOf(stInter["val_ph1"])]);
							ReplaceTextForReport(ref rt, "{IsgA" + i.ToString() + "}", val, fl);
						}
						else
						{
							rt = rt.Replace("{IsgA" + i.ToString() + "}", "-");
						}

						if (connectScheme_ != ConnectScheme.Ph1W2)
						{
							if (!dgInterharm_[i - 1,
							stInter.IndexOf(stInter["val_ph2"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(
									dgInterharm_[i - 1,
									stInter.IndexOf(stInter["val_ph2"])]);
								ReplaceTextForReport(ref rt, "{IsgB" + i.ToString() + "}", val, fl);
								//rt = rt.Replace("{IsgB" + i.ToString() + "}", val.ToString(fl));
							}
							else
							{
								rt = rt.Replace("{IsgB" + i.ToString() + "}", "-");
							}

							if (!dgInterharm_[i - 1,
								stInter.IndexOf(stInter["val_ph3"])].ToString().Equals("-"))
							{
								double val = Conversions.object_2_double(
									dgInterharm_[i - 1,
									stInter.IndexOf(stInter["val_ph3"])]);
								ReplaceTextForReport(ref rt, "{IsgC" + i.ToString() + "}", val, fl);
							}
							else
							{
								rt = rt.Replace("{IsgC" + i.ToString() + "}", "-");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportInterharm():");
				throw;
			}
		}

		// T1
        protected void ReplaceExcelNameHarm_PQP_A(ref string rt, DataGrid datagrid,
            string ExcelNameStart, string ExcelNameNumber, string ExcelNameEnd,
            string GridName1, string GridName2, string GridNameAll, int i, string fl,
            GridColumnStylesCollection st)
        {
			try
			{
				float value;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
				float value2;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName2])], out value2);
				float valueAll;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridNameAll])], out valueAll);
				float res = 0;
				if (valueAll != 0) 
					res = (value + value2) / valueAll * 100.0F;

				CheckForPaintExcelValueHarm(ref rt,
					ExcelNameStart, ExcelNameNumber, ExcelNameEnd,
					value);

				//rt = rt.Replace("{" + ExcelName + "}", res.ToString(fl));
				ReplaceTextForReport(ref rt, "{" + ExcelNameStart + ExcelNameNumber + ExcelNameEnd + "}",
					res, fl);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReplaceExcelNameHarm_PQP_A():");
				throw;
			}
        }

		//T2
		protected void ReplaceExcelNameHarm_PQP_A(ref string rt, DataGrid datagrid,
			string ExcelNameStart, string ExcelNameNumber, string ExcelNameEnd,
			string GridName1, string GridNameAll, int i, string fl,
			GridColumnStylesCollection st)
		{
			try
			{
				float value;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
				float valueAll;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridNameAll])], out valueAll);
				float res = 0;
				if (valueAll != 0) res = value / valueAll * 100.0F;

				CheckForPaintExcelValueHarm(ref rt,
					ExcelNameStart, ExcelNameNumber, ExcelNameEnd,
					value);

				ReplaceTextForReport(ref rt, "{" + ExcelNameStart + ExcelNameNumber + ExcelNameEnd + "}",
					res, fl);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReplaceExcelNameHarm_PQP_A():");
				throw;
			}
		}

		// T1
        protected void ReplaceExcelName_EtPQP_A(ref string rt, DataGrid datagrid, 
			string ExcelName,
            string GridName1, string GridName2, string GridNameAll,
            int i, string fl, GridColumnStylesCollection st)
        {
            try
            {
                float value;
                Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
                float value2;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName2])], out value2);
				float valueAll;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridNameAll])], out valueAll);
				float res = 0;
				if (valueAll != 0) res = (value + value2) / valueAll * 100.0F;

                CheckForPaintExcelValue(ref rt, ExcelName, res);

                //rt = rt.Replace("{" + ExcelName + "}", res.ToString(fl));
                ReplaceTextForReport(ref rt, "{" + ExcelName + "}", res, fl);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Exception in ReplaceExcelNameEtPqpA():");
                throw;
            }
        }

		// T2
		protected void ReplaceExcelName_EtPQP_A(ref string rt, DataGrid datagrid, 
			string ExcelName,
			string GridName1, string GridNameAll,
			int i, string fl, GridColumnStylesCollection st)
		{
			try
			{
				float value;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
				float value2;
				Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridNameAll])], out value2);
				float res = 0;
				if (value2 != 0) res = value / value2 * 100.0F;

				CheckForPaintExcelValue(ref rt, ExcelName, res);

				//rt = rt.Replace("{" + ExcelName + "}", res.ToString(fl));
				ReplaceTextForReport(ref rt, "{" + ExcelName + "}", res, fl);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ReplaceExcelNameEtPqpA():");
				throw;
			}
		}

		//protected void ReplaceExcelNameNonSymm(ref string rt, DataGrid datagrid, string ExcelName,
		//    string GridName1, string GridName2,
		//    int i, string fl, GridColumnStylesCollection st)
		//{
		//    try
		//    {
		//        float value;
		//        Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
		//        int value2 = (int)datagrid[i, st.IndexOf(st[GridName2])];
		//        float res = 0;
		//        if (value2 != 0) res = value / value2 * 100;

		//        CheckForPaintExcelValue(ref rt, ExcelName, res);

		//        //rt = rt.Replace("{" + ExcelName + "}", res.ToString(fl));
		//        ReplaceTextForReport(ref rt, "{" + ExcelName + "}", res, fl);
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Exception in ReplaceExcelNameNonSymm():");
		//        throw;
		//    }
		//}

        protected override void CheckForPaintExcelValue(ref string rt, string ExcelName, float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelName)
                {
                    case "dU{0}T1": limit = 5; break;
                    case "dU{0}T2": limit = 0; break;
                }

                if ((limit != -1) && (value > limit))
                {
                    if (StyleName_.Length == 0)
                    {
                        InsertPaintExcelStyle(ref rt);
                    }
                    PaintExcelValue(ref rt, "{" + ExcelName + "}");
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in CheckForPaintExcelValueEtPqpA():");
                throw;
            }
        }

        protected void CheckForPaintExcelValue(ref string rt, string ExcelName,
             string phase, float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelName)
                {
                    case "dU{0}T1": limit = 5; break;
                    case "dU{0}T2": limit = 0; break;
                }

                if ((limit != -1) && (value > limit))
                {
                    if (StyleName_.Length == 0)
                    {
                        InsertPaintExcelStyle(ref rt);
                    }

                    PaintExcelValue(ref rt, "{" + string.Format(ExcelName, phase) + "}");
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in CheckForPaintExcelValueEtPqpA():");
                throw;
            }
        }
    }
}
