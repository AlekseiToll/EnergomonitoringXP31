using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Threading;

using EmServiceLib;

namespace ExportToExcel
{
    public class PQPReport_RD : PQPReportBase
    {
		public PQPReport_RD(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
            ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgOver,
            ref DataGrid dgFlick, ref DataGrid dgFlickLong, ref DataGrid dgNonS,
            DateTime sdtToPL1, DateTime edtToPL1, DateTime sdtToPL2, DateTime edtToPL2,
            DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme)
            : base(settings, ref dgU, ref dgUval, ref dgF, ref dgNon, ref dgNon2, ref dgDip, ref dgOver,
                ref dgFlick, ref dgFlickLong, ref dgNonS, sdtToPL1, edtToPL1, sdtToPL2, edtToPL2, sdt, edt,
                devType, conScheme)
        { }

        /// <summary>
        /// Exports all PQP data into Word HTML format
        /// </summary>
        public void ExportReport(bool isNewDevVersion)
        {
            string out_fn = "";
            try
            {
                StyleName_ = "";

                string rt = string.Empty;	// text to process

                // reading template file
                string template_fn = EmService.AppDirectory +
                                        @"templates\PQPReportTpl{0}{1}.xml";
                switch (connectScheme_)
                {
                    case ConnectScheme.Ph3W4:
                    case ConnectScheme.Ph3W4_B_calc:
                        template_fn = string.Format(template_fn, "3ph4w", "{0}");
                        break;
                    case ConnectScheme.Ph3W3:
                    case ConnectScheme.Ph3W3_B_calc:
                        template_fn = string.Format(template_fn, "3ph3w", "{0}");
                        break;
                    case ConnectScheme.Ph1W2:
                        template_fn = string.Format(template_fn, "1ph2w", "{0}");
                        break;
                }

                // Проверка на времена начала и окончания максимальной нагрузки
                if (sdtToPL1_.TimeOfDay != edtToPL1_.TimeOfDay || sdtToPL2_.TimeOfDay != edtToPL2_.TimeOfDay)
                {
                    template_fn = string.Format(template_fn, "Y");
                }
                else
                {
                    template_fn = string.Format(template_fn, "N");
                }

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

                // parsing template file and inserting values

                // float format
                string fl = "0.00";
                if (settings_.FloatSigns > 1)
                {
                    fl = "0.";
                    for (int iSign = 0; iSign < settings_.FloatSigns; ++iSign) fl += "0";
                }

                #region Отклонение напряжения

                // number of rows in the Voltage Deviation table
				int rows = (dgU_Deviation_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

                // перебираем все номера
                for (int i = 0; i < rows; i++)
                {
					string row_name = (dgU_Deviation_[i, 0] as string).TrimEnd();

                    // суфикс для Фазы. A, B, C, AB, BC, CA и 
                    // string.Empty для прямой последовательности
                    string phase = string.Empty;

                    // суфикс для типа принадлежности к 
                    // наибольшим (1) /наименьшим (2) нагрузкам
                    // либо суточному (string.Empty)
                    string minmax = string.Empty;
                    // разделяем строку массив по символу "_"
                    string[] parts = row_name.Split(new string[] { "_" },
                        StringSplitOptions.RemoveEmptyEntries);

                    // определяем признак фазы
                    if (parts[1] != "y")
                    {
                        phase = parts[1];
                    }

                    if (parts.Length > 2)
                    {
                        // режим наибольших нагрузок
                        if (parts[2].Equals("\'")) minmax = "1";
                        // режим наименьших нагрузок
                        if (parts[2].Equals("\"")) minmax = "2";
                    }
                    ExprortReportPQP_U_Deviation(ref rt, i, phase, minmax, fl);
                }

                // Погрешности dUy
                rt = rt.Replace("{ddUр}", "±0,2% (абс.)");
                rt = rt.Replace("{ddUн}", "±0,5% (абс.)");
                // Погрешности dUy' и dUy"
                rt = rt.Replace("{ddU1р}", "±0,2% (абс.)");
                rt = rt.Replace("{ddU1н}", "±0,5% (абс.)");

                #endregion

                #region Коэффициэнты искажения синусоидальности

                GridColumnStylesCollection st = dgUNonsinusoidality_.TableStyles[0].GridColumnStyles;
                GridColumnStylesCollection st_2 =
                    dgUNonsinusoidality2_.TableStyles[0].GridColumnStyles;

                // проверяем есть ли строки в таблице
                int rowCountVoltageNonsinusoidality = 1;
                try
                {
                    DataSet tmpDataSet = (DataSet)dgUNonsinusoidality_.DataSource;
                    rowCountVoltageNonsinusoidality = tmpDataSet.Tables[0].Rows.Count;
                    if (connectScheme_ != ConnectScheme.Ph1W2)
                    {
                        tmpDataSet = (DataSet)dgUNonsinusoidality2_.DataSource;
                        rowCountVoltageNonsinusoidality += tmpDataSet.Tables[0].Rows.Count;
                    }
                }
                catch (Exception excount)
                {
                    EmService.DumpException(excount,
                        "Error in dgUNonsinusoidality Row Count");
                }

                if (rowCountVoltageNonsinusoidality > 0)
                {
                    //st.IndexOf(st[""]);
                    // kU (A, B, C)
                    if (connectScheme_ != ConnectScheme.Ph3W3 &&
                        connectScheme_ != ConnectScheme.Ph3W3_B_calc)
                    {
                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{KUAвр}",
                            //    (Conversions.object_2_double(dgUNonsinusoidality[0,
                            //    st.IndexOf(st["calc_nrm_rng_ph1"])])).ToString(fl));
                            if (!dgUNonsinusoidality_[0,
                                st.IndexOf(st["calc_nrm_rng_ph1"])].ToString().Equals("-"))
                            {
                                double val = Conversions.object_2_double(dgUNonsinusoidality_[0,
                                    st.IndexOf(st["calc_nrm_rng_ph1"])]);
                                rt = rt.Replace("{KUAвр}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{KUAвр}", "-");
                            }
                        }
                        rt = rt.Replace("{KUAнбр}",
                            (Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["calc_max_rng_ph1"])])).ToString(fl));

						ReplaceExcelName(ref rt, dgUNonsinusoidality_,
                            "KUAвT",
                        "prcnt_max_rng_ph1", "prcnt_out_max_rng_ph1",
                            0, fl,
                            st);

						ReplaceExcelName(ref rt, dgUNonsinusoidality_,
                            "KUAнбT",
                        "prcnt_out_max_rng_ph1",
                            0, fl,
                            st);

                        rt = rt.Replace("{KUвн}",
							(Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["real_nrm_rng"])])).ToString(fl));
                        rt = rt.Replace("{KUнбн}",
							(Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["real_max_rng"])])).ToString(fl));
                    }
                    if (connectScheme_ == ConnectScheme.Ph3W4 ||
                        connectScheme_ == ConnectScheme.Ph3W4_B_calc)
                    {
                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{KUBвр}",
                            //    (Conversions.object_2_double(dgUNonsinusoidality[0,
                            //    st.IndexOf(st["calc_nrm_rng_ph2"])])).ToString(fl));
							if (!dgUNonsinusoidality_[0,
                                st.IndexOf(st["calc_nrm_rng_ph2"])].ToString().Equals("-"))
                            {
								double val = Conversions.object_2_double(dgUNonsinusoidality_[0,
                                    st.IndexOf(st["calc_nrm_rng_ph2"])]);
                                rt = rt.Replace("{KUBвр}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{KUBвр}", "-");
                            }
                        }
                        rt = rt.Replace("{KUBнбр}",
							(Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["calc_max_rng_ph2"])])).ToString(fl));

						ReplaceExcelName(ref rt, dgUNonsinusoidality_,
                            "KUBвT",
                        "prcnt_max_rng_ph2", "prcnt_out_max_rng_ph2",
                            0, fl,
                            st);

						ReplaceExcelName(ref rt, dgUNonsinusoidality_,
                            "KUBнбT",
                        "prcnt_out_max_rng_ph2",
                            0, fl,
                            st);

                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{KUCвр}",
                            //    (Conversions.object_2_double(dgUNonsinusoidality[0,
                            //    st.IndexOf(st["calc_nrm_rng_ph3"])])).ToString(fl));
							if (!dgUNonsinusoidality_[0,
                                st.IndexOf(st["calc_nrm_rng_ph3"])].ToString().Equals("-"))
                            {
								double val = Conversions.object_2_double(dgUNonsinusoidality_[0,
                                    st.IndexOf(st["calc_nrm_rng_ph3"])]);
                                rt = rt.Replace("{KUCвр}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{KUCвр}", "-");
                            }
                        }
                        rt = rt.Replace("{KUCнбр}",
							(Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["calc_max_rng_ph3"])])).ToString(fl));

						ReplaceExcelName(ref rt, dgUNonsinusoidality_,
                            "KUCвT",
                        "prcnt_max_rng_ph3", "prcnt_out_max_rng_ph3",
                            0, fl,
                            st);

						ReplaceExcelName(ref rt, dgUNonsinusoidality_,
                            "KUCнбT",
                        "prcnt_out_max_rng_ph3",
                            0, fl,
                            st);

                        rt = rt.Replace("{KUвн}",
							(Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["real_nrm_rng"])])).ToString(fl));
                        rt = rt.Replace("{KUнбн}",
							(Conversions.object_2_double(dgUNonsinusoidality_[0,
                            st.IndexOf(st["real_max_rng"])])).ToString(fl));
                    }

                    if (connectScheme_ != ConnectScheme.Ph1W2)
                    {
                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{KUABвр}",
                            //    (Conversions.object_2_double(dgUNonsinusoidality2[0,
                            //    st_2.IndexOf(st_2["calc_nrm_rng_ph1"])])).ToString(fl));
                            if (!dgUNonsinusoidality2_[0,
                                st_2.IndexOf(st_2["calc_nrm_rng_ph1"])].ToString().Equals("-"))
                            {
								double val = Conversions.object_2_double(dgUNonsinusoidality2_[0,
                                    st_2.IndexOf(st_2["calc_nrm_rng_ph1"])]);
                                rt = rt.Replace("{KUABвр}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{KUABвр}", "-");
                            }
                        }
                        rt = rt.Replace("{KUABнбр}",
							(Conversions.object_2_double(dgUNonsinusoidality2_[0,
                            st_2.IndexOf(st_2["calc_max_rng_ph1"])])).ToString(fl));

						ReplaceExcelName(ref rt, dgUNonsinusoidality2_,
                            "KUABвT",
                        "prcnt_max_rng_ph1", "prcnt_out_max_rng_ph1",
                            0, fl,
                            st_2);

						ReplaceExcelName(ref rt, dgUNonsinusoidality2_,
                            "KUABнбT",
                        "prcnt_out_max_rng_ph1",
                            0, fl,
                            st_2);

                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{KUBCвр}",
                            //    (Conversions.object_2_double(dgUNonsinusoidality2[0,
                            //    st_2.IndexOf(st_2["calc_nrm_rng_ph2"])])).ToString(fl));
							if (!dgUNonsinusoidality2_[0,
                                st_2.IndexOf(st_2["calc_nrm_rng_ph2"])].ToString().Equals("-"))
                            {
								double val = Conversions.object_2_double(dgUNonsinusoidality2_[0,
                                    st_2.IndexOf(st_2["calc_nrm_rng_ph2"])]);
                                rt = rt.Replace("{KUBCвр}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{KUBCвр}", "-");
                            }
                        }
                        rt = rt.Replace("{KUBCнбр}",
							(Conversions.object_2_double(dgUNonsinusoidality2_[0,
                            st_2.IndexOf(st_2["calc_max_rng_ph2"])])).ToString(fl));

						ReplaceExcelName(ref rt, dgUNonsinusoidality2_,
                            "KUBCвT",
                        "prcnt_max_rng_ph2", "prcnt_out_max_rng_ph2",
                            0, fl,
                            st_2);

						ReplaceExcelName(ref rt, dgUNonsinusoidality2_,
                            "KUBCнбT",
                        "prcnt_out_max_rng_ph2",
                            0, fl,
                            st_2);

                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{KUCAвр}",
                            //    (Conversions.object_2_double(dgUNonsinusoidality2[0,
                            //    st_2.IndexOf(st_2["calc_nrm_rng_ph3"])])).ToString(fl));
							if (!dgUNonsinusoidality2_[0,
                                st_2.IndexOf(st_2["calc_nrm_rng_ph3"])].ToString().Equals("-"))
                            {
								double val = Conversions.object_2_double(dgUNonsinusoidality2_[0,
                                    st_2.IndexOf(st_2["calc_nrm_rng_ph3"])]);
                                rt = rt.Replace("{KUCAвр}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{KUCAвр}", "-");
                            }
                        }
                        rt = rt.Replace("{KUCAнбр}",
							(Conversions.object_2_double(dgUNonsinusoidality2_[0,
                            st_2.IndexOf(st_2["calc_max_rng_ph3"])])).ToString(fl));

						ReplaceExcelName(ref rt, dgUNonsinusoidality2_,
                        "KUCAвT",
                    "prcnt_max_rng_ph3", "prcnt_out_max_rng_ph3",
                        0, fl,
                        st_2);

						ReplaceExcelName(ref rt, dgUNonsinusoidality2_,
                            "KUCAнбT",
                        "prcnt_out_max_rng_ph3",
                            0, fl,
                            st_2);



                        rt = rt.Replace("{KUвн}",
							(Conversions.object_2_double(dgUNonsinusoidality2_[0,
                            /*-=pqp 95=- 34*/ st_2.IndexOf(st_2["real_nrm_rng"])])).ToString(fl)); //31
                        rt = rt.Replace("{KUнбн}",
							(Conversions.object_2_double(dgUNonsinusoidality2_[0,
                            /*-=pqp 95=- 35*/ st_2.IndexOf(st_2["real_max_rng"])])).ToString(fl)); //32
                    }
                }

                // Погрешности kU (A, B, C)
                rt = rt.Replace("{dKUр}", "при Ku &lt; 1,0 ±0,05% (абс.);");
                rt = rt.Replace("{dKUн}", "±10% (отн.)");

                #endregion

                #region Коэффициент нессимитрии прямой последовательности

                // k2u
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
                    GridColumnStylesCollection st10 = dgNonSymmetry_.TableStyles[0].GridColumnStyles;
                    //st.IndexOf(st["calc_nrm_rng"])
                    if (curDeviceType_ != EmDeviceType.ETPQP &&
                        curDeviceType_ != EmDeviceType.ETPQP_A)
                    {
                        rt = rt.Replace("{K2Uвр}", "-");
                    }
                    else
                    {
                        //rt = rt.Replace("{K2Uвр}",
                        //    (Conversions.object_2_double(dgNonSymmetry[0, 
                        //    st10.IndexOf(st10["calc_nrm_rng"])])).ToString(fl)); //10

						if (!dgNonSymmetry_[0,
                                st10.IndexOf(st10["calc_nrm_rng"])].ToString().Equals("-"))
                        {
                            double val = Conversions.object_2_double(
									dgNonSymmetry_[0,
                                    st10.IndexOf(st10["calc_nrm_rng"])]);
                            rt = rt.Replace("{K2Uвр}", val.ToString(fl));
                        }
                        else
                        {
                            rt = rt.Replace("{K2Uвр}", "-");
                        }
                    }
                    rt = rt.Replace("{K2Uнбр}",
						(Conversions.object_2_double(dgNonSymmetry_[0, /*-=pqp 95=- 11*/st10.IndexOf(st10["calc_max_rng"])])).ToString(fl)); //10
                    rt = rt.Replace("{K2Uвн}",
						(Conversions.object_2_double(dgNonSymmetry_[0, /*-=pqp 95=- 12*/st10.IndexOf(st10["real_nrm_rng"])])).ToString(fl)); //11
                    rt = rt.Replace("{K2Uнбн}",
						(Conversions.object_2_double(dgNonSymmetry_[0, /*-=pqp 95=- 13*/st10.IndexOf(st10["real_max_rng"])])).ToString(fl)); //12

					ReplaceExcelName(ref rt, dgNonSymmetry_,
                    "K2UвT",
                    "prcnt_max_rng", "prcnt_out_max_rng",
                    0, fl,
                    st10);

					ReplaceExcelName(ref rt, dgNonSymmetry_,
                        "K2UнбT",
                    "prcnt_out_max_rng",
                        0, fl,
                        st10);


                    // Погрешности k2u
                    rt = rt.Replace("{dK2Uр}", "±0,2% (абс.)");
                    rt = rt.Replace("{dK2Uн}", "±0,3% (абс.)");
                }

                #endregion

                #region Коэффициент нессиметрии обратной последовательности

                // k0u
                if (connectScheme_ == ConnectScheme.Ph3W4 ||
                    connectScheme_ == ConnectScheme.Ph3W4_B_calc)
                {
					GridColumnStylesCollection st2 = dgNonSymmetry_.TableStyles[0].GridColumnStyles;
                    //st.IndexOf(st[""]);
                    if (curDeviceType_ != EmDeviceType.ETPQP &&
                        curDeviceType_ != EmDeviceType.ETPQP_A)
                    {
                        rt = rt.Replace("{K0Uвр}", "-");
                    }
                    else
                    {
                        //rt = rt.Replace("{K0Uвр}",
                        //	(Conversions.object_2_double(dgNonSymmetry[1, 
                        //	st2.IndexOf(st2["calc_nrm_rng"])])).ToString(fl));
						if (!dgNonSymmetry_[1,
                                st2.IndexOf(st2["calc_nrm_rng"])].ToString().Equals("-"))
                        {
                            double val = Conversions.object_2_double(
									dgNonSymmetry_[1,
                                    st2.IndexOf(st2["calc_nrm_rng"])]);
                            rt = rt.Replace("{K0Uвр}", val.ToString(fl));
                        }
                        else
                        {
                            rt = rt.Replace("{K0Uвр}", "-");
                        }
                    }
                    rt = rt.Replace("{K0Uнбр}",
						(Conversions.object_2_double(dgNonSymmetry_[1, /*-=pqp 95=- 11*/st2.IndexOf(st2["calc_max_rng"])])).ToString(fl)); //10
                    rt = rt.Replace("{K0Uвн}",
						(Conversions.object_2_double(dgNonSymmetry_[1, /*-=pqp 95=- 12*/st2.IndexOf(st2["real_nrm_rng"])])).ToString(fl)); //11
                    rt = rt.Replace("{K0Uнбн}",
						(Conversions.object_2_double(dgNonSymmetry_[1, /*-=pqp 95=- 13*/st2.IndexOf(st2["real_max_rng"])])).ToString(fl)); //12

					ReplaceExcelName(ref rt, dgNonSymmetry_,
                    "K0UвT",
                    "prcnt_max_rng", "prcnt_out_max_rng",
                    1, fl,
                    st2);

					ReplaceExcelName(ref rt, dgNonSymmetry_,
                    "K0UнбT",
                    "prcnt_out_max_rng",
                    1, fl,
                    st2);


                    // Погрешности k0u
                    rt = rt.Replace("{dK0Uр}", "±0,2% (абс.)");
                    rt = rt.Replace("{dK0Uн}", "±0,5% (отн.)");
                }

                #endregion

                #region Отклонение частоты

                GridColumnStylesCollection st3 = dgFrequencyDeparture_.TableStyles[0].GridColumnStyles;

                // df
				if (dgFrequencyDeparture_[0, st3.IndexOf(st3["calc_nrm_rng_bottom"])] is float) //..12
					rt = rt.Replace("{dfнр}", ((float)dgFrequencyDeparture_[0, st3.IndexOf(st3["calc_nrm_rng_bottom"])]).ToString(fl)); //..12
				if (dgFrequencyDeparture_[0, st3.IndexOf(st3["calc_nrm_rng_top"])] is float) //..10
					rt = rt.Replace("{dfвр}", ((float)dgFrequencyDeparture_[0, st3.IndexOf(st3["calc_nrm_rng_top"])]).ToString(fl)); //..10

				rt = rt.Replace("{dfнмр}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
                    st3.IndexOf(st3["calc_max_rng_bottom"])])).ToString(fl)); //..13
				rt = rt.Replace("{dfнбр}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
                    st3.IndexOf(st3["calc_max_rng_top"])])).ToString(fl)); //..11
				rt = rt.Replace("{dfнн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
                    st3.IndexOf(st3["real_nrm_rng_bottom"])])).ToString(fl)); //..16
				rt = rt.Replace("{dfвн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
                    st3.IndexOf(st3["real_nrm_rng_top"])])).ToString(fl)); //..14
				rt = rt.Replace("{dfнмн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
                    st3.IndexOf(st3["real_max_rng_bottom"])])).ToString(fl)); //..17
				rt = rt.Replace("{dfнбн}", (Conversions.object_2_double(dgFrequencyDeparture_[0,
                    st3.IndexOf(st3["real_max_rng_top"])])).ToString(fl)); //..15

				ReplaceExcelName(ref rt, dgFrequencyDeparture_,
                    "dfT1",
                    "prcnt_max_rng", "prcnt_out_max_rng",
                    0, fl,
                    st3);

				ReplaceExcelName(ref rt, dgFrequencyDeparture_,
                    "dfT2",
                    "prcnt_out_max_rng",
                    0, fl,
                    st3);


                // Погрешности df
                rt = rt.Replace("{ddfр}", "±0,01 Гц. (абс.)");
                rt = rt.Replace("{ddfн}", "±0,03 Гц. (абс.)");

                #endregion

                #region Коэффициэнты гармонических составляющих (фазные)

                if (rowCountVoltageNonsinusoidality > 0)
                {

                    // KuN ( N = 2..40 )
                    for (int i = 2; i < 41; i++)
                    {
                        if (connectScheme_ != ConnectScheme.Ph3W3 &&
                            connectScheme_ != ConnectScheme.Ph3W3_B_calc)
                        {
                            if (curDeviceType_ != EmDeviceType.ETPQP &&
                                curDeviceType_ != EmDeviceType.ETPQP_A)
                            {
                                rt = rt.Replace("{kua" + i.ToString() + "в}", "-");
                            }
                            else
                            {
                                //rt = rt.Replace("{kua" + i.ToString() + "в}",
                                //    (Conversions.object_2_double(dgUNonsinusoidality[i - 1,
                                //    st.IndexOf(st["calc_nrm_rng_ph1"])
                                //    ])).ToString(fl));
                                if (!dgUNonsinusoidality_[i - 1,
                                st.IndexOf(st["calc_nrm_rng_ph1"])].ToString().Equals("-"))
                                {
                                    double val = Conversions.object_2_double(
										dgUNonsinusoidality_[i - 1,
                                        st.IndexOf(st["calc_nrm_rng_ph1"])]);
                                    rt = rt.Replace("{kua" + i.ToString() + "в}", val.ToString(fl));
                                }
                                else
                                {
                                    rt = rt.Replace("{kua" + i.ToString() + "в}", "-");
                                }
                            }

                            rt = rt.Replace("{kua" + i.ToString() + "нб}",
								(Conversions.object_2_double(dgUNonsinusoidality_[i - 1,
                                /*-=pqp 95=- 11 */
                                //10
                                st.IndexOf(st["calc_max_rng_ph1"])
                                ])).ToString(fl)); //10

							ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality_,
                                "kua", i.ToString(), "T1",
                                "prcnt_max_rng_ph1",
                                "prcnt_out_max_rng_ph1",
                                i - 1, fl,
                                st);
							ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality_,
                                "kua", i.ToString(), "T2",
                                "prcnt_out_max_rng_ph1",
                                i - 1, fl,
                                st);
                        }

                        if (connectScheme_ == ConnectScheme.Ph3W4 ||
                            connectScheme_ == ConnectScheme.Ph3W4_B_calc)
                        {
                            if (curDeviceType_ != EmDeviceType.ETPQP &&
                                curDeviceType_ != EmDeviceType.ETPQP_A)
                            {
                                rt = rt.Replace("{kub" + i.ToString() + "в}", "-");
                            }
                            else
                            {
                                //rt = rt.Replace("{kub" + i.ToString() + "в}", 
                                //    (Conversions.object_2_double(dgUNonsinusoidality[i - 1,
                                //    st.IndexOf(st["calc_nrm_rng_ph2"])
                                //    ])).ToString(fl));
								if (!dgUNonsinusoidality_[i - 1,
                                st.IndexOf(st["calc_nrm_rng_ph2"])].ToString().Equals("-"))
                                {
                                    double val = Conversions.object_2_double(
										dgUNonsinusoidality_[i - 1,
                                        st.IndexOf(st["calc_nrm_rng_ph2"])]);
                                    rt = rt.Replace("{kub" + i.ToString() + "в}", val.ToString(fl));
                                }
                                else
                                {
                                    rt = rt.Replace("{kub" + i.ToString() + "в}", "-");
                                }
                            }

                            rt = rt.Replace("{kub" + i.ToString() + "нб}",
								(Conversions.object_2_double(dgUNonsinusoidality_[i - 1,
                                /*-=pqp 95=- 22 */
                                //20
                                st.IndexOf(st["calc_max_rng_ph2"])
                                ])).ToString(fl)); //20

							ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality_,
                                "kub", i.ToString(), "T1",
                                "prcnt_max_rng_ph2",
                                "prcnt_out_max_rng_ph2",
                                i - 1, fl,
                                st);
							ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality_,
                                "kub", i.ToString(), "T2",
                                "prcnt_out_max_rng_ph2",
                                i - 1, fl,
                                st);

                            if (curDeviceType_ != EmDeviceType.ETPQP &&
                                curDeviceType_ != EmDeviceType.ETPQP_A)
                            {
                                rt = rt.Replace("{kuc" + i.ToString() + "в}", "-");
                            }
                            else
                            {
                                //rt = rt.Replace("{kuc" + i.ToString() + "в}", 
                                //    (Conversions.object_2_double(dgUNonsinusoidality[i - 1,
                                //    st.IndexOf(st["calc_nrm_rng_ph3"])
                                //    ])).ToString(fl)); //30
								if (!dgUNonsinusoidality_[i - 1,
                                st.IndexOf(st["calc_nrm_rng_ph3"])].ToString().Equals("-"))
                                {
                                    double val = Conversions.object_2_double(
										dgUNonsinusoidality_[i - 1,
                                        st.IndexOf(st["calc_nrm_rng_ph3"])]);
                                    rt = rt.Replace("{kuc" + i.ToString() + "в}", val.ToString(fl));
                                }
                                else
                                {
                                    rt = rt.Replace("{kuc" + i.ToString() + "в}", "-");
                                }
                            }
                            rt = rt.Replace("{kuc" + i.ToString() + "нб}",
								(Conversions.object_2_double(dgUNonsinusoidality_[i - 1,
                                st.IndexOf(st["calc_max_rng_ph3"])
                                ])).ToString(fl)); //30

							ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality_,
                                "kuc", i.ToString(), "T1",
                                "prcnt_max_rng_ph3",
                                "prcnt_out_max_rng_ph3",
                                i - 1, fl,
                                st);
							ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality_,
                                "kuc", i.ToString(), "T2",
                                "prcnt_out_max_rng_ph3",
                                i - 1, fl,
                                st);

                        }

                        if (connectScheme_ != ConnectScheme.Ph3W3 &&
                            connectScheme_ != ConnectScheme.Ph3W3_B_calc)
                        {
							rt = rt.Replace("{ku" + i.ToString() + "вн}", (Conversions.object_2_double(dgUNonsinusoidality_[i - 1, /*-=pqp 95=- 34*/
                                //31
                                st.IndexOf(st["real_nrm_rng"])
                                ])).ToString(fl)); //31
							rt = rt.Replace("{ku" + i.ToString() + "нбн}", (Conversions.object_2_double(dgUNonsinusoidality_[i - 1, /*-=pqp 95=- 35*/
                                //32
                                st.IndexOf(st["real_max_rng"])
                                ])).ToString(fl)); //32
                        }
                    }
                }
                // Погрешности KuN ( N = 2..40 )
                rt = rt.Replace("{dKUNр}", "при Ku(n) &lt; 1,0 ±0,05% (абс.);");
                rt = rt.Replace("{dKUNн}", "при Ku(n) &lt; 1,0 ±0,05% (абс.);");

                #endregion

                #region Коэффициэнты гармонических составляющих (линейные)

                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
                    // KuN ( N = 2..40 )
                    for (int i = 2; i < 41; i++)
                    {
                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{kuab" + i.ToString() + "в}", 
                            //    (Conversions.object_2_double(dgUNonsinusoidality2[i - 1, 
                            //    st_2.IndexOf(st_2["calc_nrm_rng_ph1"])])).ToString(fl));
                            if (!dgUNonsinusoidality2_[i - 1,
                                st_2.IndexOf(st_2["calc_nrm_rng_ph1"])].ToString().Equals("-"))
                            {
                                double val = Conversions.object_2_double(
									dgUNonsinusoidality2_[i - 1,
                                    st_2.IndexOf(st_2["calc_nrm_rng_ph1"])]);
                                rt = rt.Replace("{kuab" + i.ToString() + "в}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{kuab" + i.ToString() + "в}", "-");
                            }
                        }

						rt = rt.Replace("{kuab" + i.ToString() + "нб}", (Conversions.object_2_double(dgUNonsinusoidality2_[i - 1, st_2.IndexOf(st_2["calc_max_rng_ph1"])])).ToString(fl));

						ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality2_,
                            "kuab", i.ToString(), "T1",
                            "prcnt_max_rng_ph1",
                            "prcnt_out_max_rng_ph1",
                            i - 1, fl,
                            st_2);
						ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality2_,
                            "kuab", i.ToString(), "T2",
                            "prcnt_out_max_rng_ph1",
                            i - 1, fl,
                            st_2);

                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{kubc" + i.ToString() + "в}", 
                            //    (Conversions.object_2_double(dgUNonsinusoidality2[i - 1, 
                            //    st_2.IndexOf(st_2["calc_nrm_rng_ph2"])])).ToString(fl));
							if (!dgUNonsinusoidality2_[i - 1,
                                st_2.IndexOf(st_2["calc_nrm_rng_ph2"])].ToString().Equals("-"))
                            {
                                double val = Conversions.object_2_double(
									dgUNonsinusoidality2_[i - 1,
                                    st_2.IndexOf(st_2["calc_nrm_rng_ph2"])]);
                                rt = rt.Replace("{kubc" + i.ToString() + "в}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{kubc" + i.ToString() + "в}", "-");
                            }
                        }

						rt = rt.Replace("{kubc" + i.ToString() + "нб}", (Conversions.object_2_double(dgUNonsinusoidality2_[i - 1, st_2.IndexOf(st_2["calc_max_rng_ph2"])])).ToString(fl));

						ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality2_,
                            "kubc", i.ToString(), "T1",
                            "prcnt_max_rng_ph2",
                            "prcnt_out_max_rng_ph2",
                            i - 1, fl,
                            st_2);
						ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality2_,
                            "kubc", i.ToString(), "T2",
                            "prcnt_out_max_rng_ph2",
                            i - 1, fl,
                            st_2);

                        if (curDeviceType_ == EmDeviceType.ETPQP ||
                            curDeviceType_ == EmDeviceType.ETPQP_A)
                        {
                            //rt = rt.Replace("{kuca" + i.ToString() + "в}", 
                            //    (Conversions.object_2_double(dgUNonsinusoidality2[i - 1, 
                            //    st_2.IndexOf(st_2["calc_nrm_rng_ph3"])])).ToString(fl));
							if (!dgUNonsinusoidality2_[i - 1,
                                st_2.IndexOf(st_2["calc_nrm_rng_ph3"])].ToString().Equals("-"))
                            {
                                double val = Conversions.object_2_double(
									dgUNonsinusoidality2_[i - 1,
                                    st_2.IndexOf(st_2["calc_nrm_rng_ph3"])]);
                                rt = rt.Replace("{kuca" + i.ToString() + "в}", val.ToString(fl));
                            }
                            else
                            {
                                rt = rt.Replace("{kuca" + i.ToString() + "в}", "-");
                            }
                        }

						rt = rt.Replace("{kuca" + i.ToString() + "нб}", (Conversions.object_2_double(dgUNonsinusoidality2_[i - 1, st_2.IndexOf(st_2["calc_max_rng_ph3"])])).ToString(fl));

						ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality2_,
                            "kuca", i.ToString(), "T1",
                            "prcnt_max_rng_ph3",
                            "prcnt_out_max_rng_ph3",
                            i - 1, fl,
                            st_2);
						ReplaceExcelNameHarm(ref rt, dgUNonsinusoidality2_,
                            "kuca", i.ToString(), "T2",
                            "prcnt_out_max_rng_ph3",
                            i - 1, fl,
                            st_2);


						rt = rt.Replace("{_ku" + i.ToString() + "вн}", (Conversions.object_2_double(dgUNonsinusoidality2_[i - 1, st_2.IndexOf(st_2["real_nrm_rng"])])).ToString(fl));
						rt = rt.Replace("{_ku" + i.ToString() + "нбн}", (Conversions.object_2_double(dgUNonsinusoidality2_[i - 1, st_2.IndexOf(st_2["real_max_rng"])])).ToString(fl));
                    }
                }
                // Погрешности KuN ( N = 2..40 )
                rt = rt.Replace("{dKUNр}", "при Ku(n) &lt; 1,0 ±0,05% (абс.);");
                rt = rt.Replace("{dKUNн}", "при Ku(n) &lt; 1,0 ±0,05% (абс.);");

                #endregion

                #region Провалы и Перенапряжения в ПКЭ

                // провалы

                string t_frmt = "HH:mm:ss.fff";

                //int phArowIndex = (connectScheme == ConnectScheme.Ph1W2) ? 0 : 1;
                int phArowIndex = 1;
				DataSet tempDataSet = (DataSet)dgDips_.DataSource;
                int rowCountDips = tempDataSet.Tables[0].Rows.Count;
                if (rowCountDips < 2)
                    phArowIndex = 0;

				rt = rt.Replace("{пр.колUA}", ((Int64)dgDips_[phArowIndex, 2]).ToString());		// количество
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
					rt = rt.Replace("{пр.колUB}", ((Int64)dgDips_[2, 2]).ToString());
					rt = rt.Replace("{пр.колUC}", ((Int64)dgDips_[3, 2]).ToString());
                }

				rt = rt.Replace("{пр.прUA}", ((DateTime)dgDips_[phArowIndex, 1]).ToString(t_frmt));	// суммарная продолжительность
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
					rt = rt.Replace("{пр.прUB}", ((DateTime)dgDips_[2, 1]).ToString(t_frmt));
					rt = rt.Replace("{пр.прUC}", ((DateTime)dgDips_[3, 1]).ToString(t_frmt));
                }
				rt = rt.Replace("{пр.глUA}", (Conversions.object_2_double(dgDips_[phArowIndex, 6]) * 100.0).ToString(fl));		// глубина провала
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
					rt = rt.Replace("{пр.глUB}", (Conversions.object_2_double(dgDips_[2, 6]) * 100.0).ToString(fl));
					rt = rt.Replace("{пр.глUC}", (Conversions.object_2_double(dgDips_[3, 6]) * 100.0).ToString(fl));
                }

                // перенапряжения
				rt = rt.Replace("{пер.колUA}", ((Int64)dgOvers_[phArowIndex, 2]).ToString());
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
					rt = rt.Replace("{пер.колUB}", ((Int64)dgOvers_[2, 2]).ToString());
					rt = rt.Replace("{пер.колUC}", ((Int64)dgOvers_[3, 2]).ToString());
                }
				rt = rt.Replace("{пер.прUA}", ((DateTime)dgOvers_[phArowIndex, 1]).ToString(t_frmt));
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
					rt = rt.Replace("{пер.прUB}", ((DateTime)dgOvers_[2, 1]).ToString(t_frmt));
					rt = rt.Replace("{пер.прUC}", ((DateTime)dgOvers_[3, 1]).ToString(t_frmt));
                }
				rt = rt.Replace("{пер.глUA}", (Conversions.object_2_double(dgOvers_[phArowIndex, 6])).ToString(fl));
                if (connectScheme_ != ConnectScheme.Ph1W2)
                {
					rt = rt.Replace("{пер.глUB}", (Conversions.object_2_double(dgOvers_[2, 6])).ToString(fl));
					rt = rt.Replace("{пер.глUC}", (Conversions.object_2_double(dgOvers_[3, 6])).ToString(fl));
                }
				Int64 summ_deviation_number = (Int64)dgDips_[0, 2] + (Int64)dgOvers_[0, 2];

                rt = rt.Replace("{кол.сб.пит.}", summ_deviation_number.ToString());

                #endregion

                #region MaxFlicker

                if (isNewDevVersion)
                {
                    DataTable dtFliker = (dgFlicker_.DataSource as DataSet).Tables["day_avg_parameters_t5"];
                    DataTable dtFlikerLong =
                        (dgFlickerLong_.DataSource as DataSet).Tables["day_avg_parameters_t5"];

                    double Ps_A_max = 0, Pl_A_max = 0,
                        Ps_B_max = 0, Pl_B_max = 0,
                        Ps_C_max = 0, Pl_C_max = 0;

                    if (dtFliker.Rows.Count > 0)
                    {
                        Ps_A_max = Conversions.object_2_double(dtFliker.Rows[0]["flik_a"]);

                        if (connectScheme_ != ConnectScheme.Ph1W2)
                        {
                            Ps_B_max = Conversions.object_2_double(dtFliker.Rows[0]["flik_b"]);
                            Ps_C_max = Conversions.object_2_double(dtFliker.Rows[0]["flik_c"]);
                        }

                        for (int i = 1; i < dtFliker.Rows.Count; i++)
                        {
                            Ps_A_max = Math.Max(Ps_A_max, Conversions.object_2_double(dtFliker.Rows[i]["flik_a"]));

                            if (connectScheme_ != ConnectScheme.Ph1W2)
                            {
                                Ps_B_max = Math.Max(Ps_B_max, Conversions.object_2_double(
                                    dtFliker.Rows[i]["flik_b"]));
                                Ps_C_max = Math.Max(Ps_C_max, Conversions.object_2_double(
                                    dtFliker.Rows[i]["flik_c"]));
                            }
                        }

                        rt = rt.Replace("{PstAMax}", Math.Round(Ps_A_max, 2).ToString());

                        if (connectScheme_ != ConnectScheme.Ph1W2)
                        {
                            rt = rt.Replace("{PstBMax}", Math.Round(Ps_B_max, 2).ToString());
                            rt = rt.Replace("{PstCMax}", Math.Round(Ps_C_max, 2).ToString());
                        }
                    }

                    if (dtFlikerLong.Rows.Count > 0)
                    {
                        Pl_A_max = Conversions.object_2_double(dtFlikerLong.Rows[0]["flik_a_long"]);

                        if (connectScheme_ != ConnectScheme.Ph1W2)
                        {
                            Pl_B_max = Conversions.object_2_double(dtFlikerLong.Rows[0]["flik_b_long"]);
                            Pl_C_max = Conversions.object_2_double(dtFlikerLong.Rows[0]["flik_c_long"]);
                        }

                        for (int i = 1; i < dtFlikerLong.Rows.Count; i++)
                        {
                            Pl_A_max = Math.Max(Pl_A_max, Conversions.object_2_double(
                                dtFlikerLong.Rows[i]["flik_a_long"]));

                            if (connectScheme_ != ConnectScheme.Ph1W2)
                            {
                                Pl_B_max = Math.Max(Pl_B_max, Conversions.object_2_double(
                                    dtFlikerLong.Rows[i]["flik_b_long"]));
                                Pl_C_max = Math.Max(Pl_C_max, Conversions.object_2_double(
                                    dtFlikerLong.Rows[i]["flik_c_long"]));
                            }
                        }

                        rt = rt.Replace("{PltAMax}", Math.Round(Pl_A_max, 2).ToString());

                        if (connectScheme_ != ConnectScheme.Ph1W2)
                        {
                            rt = rt.Replace("{PltBMax}", Math.Round(Pl_B_max, 2).ToString());
                            rt = rt.Replace("{PltCMax}", Math.Round(Pl_C_max, 2).ToString());
                        }
                    }

                    rt = rt.Replace("{PstNorm533}", "1,38");
                    rt = rt.Replace("{PltNorm533}", "1,00");
                    rt = rt.Replace("{PstNorm534}", "1,00");
                    rt = rt.Replace("{PltNorm534}", "0,74");
                }

                #endregion

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
                EmService.DumpException(ex, "Error in ExportReportPQP():");
                MessageBoxes.PqpReportWriteError(this, out_fn);
                throw;
            }
        }

        protected void ExprortReportPQP_U_Deviation(ref string rt, int iRow, string phase,
                        string minmax, string fl)
        {
            GridColumnStylesCollection st = dgU_Deviation_.TableStyles[0].GridColumnStyles;

			if (dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_bottom"])] is string)
            {
				if (!dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_bottom"])].ToString().Equals("-"))
                {
                    float val;
                    Conversions.object_2_float_en_ru(
						dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_bottom"])],
                        out val);
                    string str_val = val.ToString(fl);

                    rt = rt.Replace("{" + string.Format("dU{0}н{1}р", phase, minmax) + "}", str_val); //12
                }
            }
			else if (dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_bottom"])] is float)
            {
                rt = rt.Replace("{" + string.Format("dU{0}н{1}р", phase, minmax) + "}",
					((float)dgU_Deviation_[iRow,
                    st.IndexOf(st["calc_nrm_rng_bottom"])]).ToString(fl)); //12
            }

			if (dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_top"])] is string)
            {
				if (!dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_top"])].ToString().Equals("-"))
                {
                    float val;
                    Conversions.object_2_float_en_ru(
						dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_top"])],
                        out val);
                    string str_val = val.ToString(fl);

                    rt = rt.Replace("{" + string.Format("dU{0}в{1}р", phase, minmax) + "}", str_val); //10
                }
            }
			else if (dgU_Deviation_[iRow, st.IndexOf(st["calc_nrm_rng_top"])] is float)
            {
                rt = rt.Replace("{" + string.Format("dU{0}в{1}р", phase, minmax) + "}",
					((float)dgU_Deviation_[iRow,
                    st.IndexOf(st["calc_nrm_rng_top"])]).ToString(fl)); //10
            }

            //if (dgU_Deviation[iRow, st.IndexOf(st["calc_nrm_rng_bottom"])] is float)
            //{
            //    rt = rt.Replace("{" + string.Format("dU{0}н{1}р", phase, minmax) + "}",
            //        ((float)dgU_Deviation[iRow,
            //        st.IndexOf(st["calc_nrm_rng_bottom"])]).ToString(fl)); //12
            //}

            //if (dgU_Deviation[iRow, st.IndexOf(st["calc_nrm_rng_top"])] is float)
            //{
            //    rt = rt.Replace("{" + string.Format("dU{0}в{1}р", phase, minmax) + "}",
            //        ((float)dgU_Deviation[iRow,
            //        st.IndexOf(st["calc_nrm_rng_top"])]).ToString(fl)); //10
            //}

            rt = rt.Replace("{" + string.Format("dU{0}нм{1}р", phase, minmax) + "}",
				((float)dgU_Deviation_[iRow,
                st.IndexOf(st["calc_max_rng_bottom"])]).ToString(fl)); //13

            rt = rt.Replace("{" + string.Format("dU{0}нб{1}р", phase, minmax) + "}",
				((float)dgU_Deviation_[iRow,
                st.IndexOf(st["calc_max_rng_top"])]).ToString(fl)); //11

            rt = rt.Replace("{" + string.Format("dU{0}н{1}н", phase, minmax) + "}",
				((float)dgU_Deviation_[iRow,
                st.IndexOf(st["real_nrm_rng_bottom"])]).ToString(fl)); //16

            rt = rt.Replace("{" + string.Format("dU{0}в{1}н", phase, minmax) + "}",
				((float)dgU_Deviation_[iRow,
                st.IndexOf(st["real_nrm_rng_top"])]).ToString(fl)); //14

            rt = rt.Replace("{" + string.Format("dU{0}нм{1}н", phase, minmax) + "}",
				((float)dgU_Deviation_[iRow,
                st.IndexOf(st["real_max_rng_bottom"])]).ToString(fl)); //17

            rt = rt.Replace("{" + string.Format("dU{0}нб{1}н", phase, minmax) + "}",
				((float)dgU_Deviation_[iRow,
                st.IndexOf(st["real_max_rng_top"])]).ToString(fl)); //15

            //rt = rt.Replace("{" + string.Format("dU{0}{1}T1", phase, minmax) + "}",
            //    ((float)dgU_Deviation[i,
            //    st.IndexOf(st["prcnt_max_rng_global"])] +
            //    (float)dgU_Deviation[i,
            //    st.IndexOf(st["prcnt_out_max_rng_global"])]).ToString(fl)); //5 + 6

			ReplaceExcelName(ref rt, dgU_Deviation_,
                "dU{0}{1}T1",
                //"prcnt_max_rng_global"
                "prcnt_max_rng",
                //"prcnt_out_max_rng_global"
                "prcnt_out_max_rng",
                    iRow, phase, minmax, fl, st);

            //rt = rt.Replace("{" + string.Format("dU{0}{1}T2", phase, minmax) + "}",
            //    ((float)dgU_Deviation[i,
            //    st.IndexOf(st["prcnt_out_max_rng_global"])]).ToString(fl)); //6 

			ReplaceExcelName(ref rt, dgU_Deviation_,
                "dU{0}{1}T2",
                //"prcnt_out_max_rng_global"
                "prcnt_out_max_rng", iRow, phase, minmax, fl, st);
        }
    }
}
