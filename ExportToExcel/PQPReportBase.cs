using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

using EmServiceLib;
using DataGridColumnStyles;

namespace ExportToExcel
{
    public abstract class PQPReportBase
    {
        #region Fields

        protected string StyleName_;

        protected string ExcelStyleTemplate_ =
            "<Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\" ss:WrapText=\"1\"/>"
            + Environment.NewLine +
            "<Borders>" + Environment.NewLine +
            "<Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>" +
            Environment.NewLine +
            "<Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>" +
            Environment.NewLine +
            "<Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>" +
            Environment.NewLine +
            "<Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>" +
            Environment.NewLine +
            "</Borders>" + Environment.NewLine +
            "<Font ss:FontName=\"Arial Cyr\" x:CharSet=\"204\" ss:Color=\"#FF0000\"/>" +
            Environment.NewLine;

        protected EmDataSaver.Settings settings_;
        protected EmDeviceType curDeviceType_;
        protected ConnectScheme connectScheme_;

        protected DataGrid dgU_Deviation_ = null;
		protected DataGrid dgU_Values_ = null;
        protected DataGrid dgFrequencyDeparture_ = null;
        protected DataGrid dgUNonsinusoidality_ = null;
        protected DataGrid dgUNonsinusoidality2_ = null;
        protected DataGrid dgDips_ = null;
		protected DataGrid dgDips2_ = null;
        protected DataGrid dgOvers_ = null;
        protected DataGrid dgNonSymmetry_ = null;
        protected DataGrid dgFlickerNum_ = null;
        protected DataGrid dgFlicker_ = null;
        protected DataGrid dgFlickerLong_ = null;
		protected DataGrid dgInterharm_ = null;

        protected DateTime sdt_;
        protected DateTime edt_;
        protected DateTime sdtToPL1_;
        protected DateTime edtToPL1_;
        protected DateTime sdtToPL2_;
        protected DateTime edtToPL2_;

		protected long ser_num_;

        #endregion

        public PQPReportBase(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
            ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgOver,
            ref DataGrid dgFlick, ref DataGrid dgNonS, ref DataGrid dgInterharm,
            DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme, long ser_num)
        {
            settings_ = settings;
            dgU_Deviation_ = dgU;
			dgU_Values_ = dgUval;
            dgFrequencyDeparture_ = dgF;
            dgUNonsinusoidality_ = dgNon;
            dgUNonsinusoidality2_ = dgNon2;
            dgDips_ = dgDip;
            dgOvers_ = dgOver;
            dgFlickerNum_ = dgFlick;
            dgNonSymmetry_ = dgNonS;
			dgInterharm_ = dgInterharm;

            sdt_ = sdt;
            edt_ = edt;
            curDeviceType_ = devType;
            connectScheme_ = conScheme;
			ser_num_ = ser_num;
        }

		public PQPReportBase(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
			ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgDip2, ref DataGrid dgOver,
			ref DataGrid dgFlick, ref DataGrid dgNonS, ref DataGrid dgInterharm,
			DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme, long ser_num)
		{
			settings_ = settings;
			dgU_Deviation_ = dgU;
			dgU_Values_ = dgUval;
			dgFrequencyDeparture_ = dgF;
			dgUNonsinusoidality_ = dgNon;
			dgUNonsinusoidality2_ = dgNon2;
			dgDips_ = dgDip;
			dgDips2_ = dgDip2;
			dgOvers_ = dgOver;
			dgFlickerNum_ = dgFlick;
			dgNonSymmetry_ = dgNonS;
			dgInterharm_ = dgInterharm;

			sdt_ = sdt;
			edt_ = edt;
			curDeviceType_ = devType;
			connectScheme_ = conScheme;
			ser_num_ = ser_num;
		}

		public PQPReportBase(EmDataSaver.Settings settings, ref DataGrid dgU, ref DataGrid dgUval, ref DataGrid dgF,
			ref DataGrid dgNon, ref DataGrid dgNon2, ref DataGrid dgDip, ref DataGrid dgOver,
			ref DataGrid dgFlick, ref DataGrid dgFlickLong, ref DataGrid dgNonS,
			DateTime sdtToPL1, DateTime edtToPL1, DateTime sdtToPL2, DateTime edtToPL2,
			DateTime sdt, DateTime edt, EmDeviceType devType, ConnectScheme conScheme)
		{
			settings_ = settings;
			dgU_Deviation_ = dgU;
			dgU_Values_ = dgUval;
			dgFrequencyDeparture_ = dgF;
			dgUNonsinusoidality_ = dgNon;
			dgUNonsinusoidality2_ = dgNon2;
			dgDips_ = dgDip;
			dgOvers_ = dgOver;
			dgFlicker_ = dgFlick;
			dgFlickerLong_ = dgFlickLong;
			dgNonSymmetry_ = dgNonS;

			sdtToPL1_ = sdtToPL1;
			edtToPL1_ = edtToPL1;
			sdtToPL2_ = sdtToPL2;
			edtToPL2_ = edtToPL2;
			sdt_ = sdt;
			edt_ = edt;
			curDeviceType_ = devType;
			connectScheme_ = conScheme;
		}

        //protected abstract void ExprortReportPQP_U_Deviation(ref string rt, int iRow, string phase,
        //                string minmax, string fl);

        protected void ExprortReportPQP_head(ref string rt, string app_num, string rep_num)
        {
			rt = rt.Replace("{ser_num}", ser_num_.ToString());

            // numbers of protocol and appendix
            rt = rt.Replace("{num_app}", app_num);
            rt = rt.Replace("{num_prot}", rep_num);

            // dates and times of measurements
            rt = rt.Replace("{дата_нач_изм}", sdt_.ToString("dd.MM.yyyy"));
            rt = rt.Replace("{вр_нач_изм}", sdt_.ToString("HH:mm"));
            rt = rt.Replace("{дата_кон_изм}", edt_.ToString("dd.MM.yyyy"));
            rt = rt.Replace("{вр_кон_изм}", edt_.ToString("HH:mm"));

            // times of peak load
            string str_pl_tpl = "{0} - {1}";
            string str_pl1 = string.Empty;
            string str_pl2 = string.Empty;
            string str_empty_tpl = " - ";

            // флаги наличия времен максимальной нагрузки
            bool bPL1 = sdtToPL1_.TimeOfDay != edtToPL1_.TimeOfDay;
            bool bPL2 = sdtToPL2_.TimeOfDay != edtToPL2_.TimeOfDay;

            // если первое время установлено
            if (bPL1 && bPL2)
            {
                str_pl1 = string.Format(str_pl_tpl, sdtToPL1_.ToString("HH:mm"), edtToPL1_.ToString("HH:mm"));
                str_pl2 = string.Format(str_pl_tpl, sdtToPL2_.ToString("HH:mm"), edtToPL2_.ToString("HH:mm"));
                rt = rt.Replace("{вр_рнн1}", str_pl1);
                rt = rt.Replace("{вр_рнн2}", str_pl2);
            }
            else if (bPL1 && !bPL2)
            {
                str_pl1 = string.Format(str_pl_tpl, sdtToPL1_.ToString("HH:mm"), edtToPL1_.ToString("HH:mm"));
                rt = rt.Replace("{вр_рнн1}", str_pl1);
                rt = rt.Replace("{вр_рнн2}", str_empty_tpl);
            }
            else if (!bPL1 && bPL2)
            {
                str_pl2 = string.Format(str_pl_tpl, sdtToPL2_.ToString("HH:mm"), edtToPL2_.ToString("HH:mm"));
                rt = rt.Replace("{вр_рнн1}", str_pl2);
                rt = rt.Replace("{вр_рнн2}", str_empty_tpl);
            }
            else
            {
                rt = rt.Replace("{вр_рнн1}", str_empty_tpl);
                rt = rt.Replace("{вр_рнн2}", str_empty_tpl);
            }
        }

        protected void ReplaceTextForReport(ref string rt, string text, double val, string fl)
        {
            try
            {
                if (val == 0)
                {
                    rt = rt.Replace(text, "0");
                }
                else
                {
                    rt = rt.Replace(text, val.ToString(fl));
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in ReplaceTextForReport():");
                throw;
            }
        }

        protected void InsertPaintExcelStyle(ref string rt)
        {
            try
            {
                XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(rt);

                XmlNode StylesListNode = null;

                #region Search StylesListNode

                foreach (XmlNode DocChildNode in doc.ChildNodes)
                {
                    if (DocChildNode.Name == "Workbook")
                    {
                        bool ok = false;

                        foreach (XmlNode WBChildNode in DocChildNode.ChildNodes)
                        {
                            if (WBChildNode.Name == "Styles")
                            {
                                StylesListNode = WBChildNode;
                                ok = true;
                                break;
                            }
                        }

                        if (ok)
                        {
                            break;
                        }
                    }
                }

                #endregion

                #region Search free style name

                string StyleNamePrefix = "s";
                int index = -1;

                while (true)
                {
                    index++;
                    StyleName_ = StyleNamePrefix + index.ToString();

                    bool match = false;

                    foreach (XmlNode style in StylesListNode.ChildNodes)
                    {
                        if (style.Attributes["ss:ID"].ToString() == StyleName_)
                        {
                            match = true;
                            break;
                        }
                    }

                    if (!match)
                    {
                        break;
                    }
                }

                #endregion

                XmlNode NewStyleNode = doc.CreateNode("element", "Style", String.Empty);
                XmlAttribute attribute = doc.CreateAttribute("ss:ID");
                attribute.Value = StyleName_;
                NewStyleNode.Attributes.Append(attribute);
                NewStyleNode.InnerText = ExcelStyleTemplate_;
                StylesListNode.AppendChild(NewStyleNode);

                StringWriter sw = new StringWriter();
                doc.Save(sw);
                rt = sw.ToString();

                rt = rt.Replace("xmlns=\"\"", String.Empty);
                rt = rt.Replace("&lt;", "<");
                rt = rt.Replace("&gt;", ">");

                string s_ID = "ID=" + "\"" + StyleName_ + "\"";
                int i_ID = rt.IndexOf(s_ID);
                rt = rt.Insert(i_ID, "ss:");

                //	StyleInsertTest(ref doc, ref rt);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in InsertPaintExcelStyle():");
                throw;
            }
        }

        protected void PaintExcelValue(ref string rt, string ExcelName)
        {
            try
            {
                int k = rt.IndexOf(ExcelName);
                if (k == -1)
                {
                    return;
                }
                k = rt.LastIndexOf("ss:StyleID=", k, k + 1);
                k = rt.IndexOf('\"', k);
                int k1 = rt.IndexOf('\"', k + 1);

                rt = rt.Remove(k + 1, k1 - k - 1);
                rt = rt.Insert(k + 1, StyleName_);
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in PaintExcelValue():");
                throw;
            }
        }

        protected virtual void CheckForPaintExcelValue(ref string rt, string ExcelName,
             string phase, string minmax, float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelName)
                {
                    case "dU{0}{1}T1": limit = 5; break;
                    case "dU{0}{1}T2": limit = 0; break;
                }

                if ((limit != -1) && (value > limit))
                {
                    if (StyleName_.Length == 0)
                    {
                        InsertPaintExcelStyle(ref rt);
                    }

                    PaintExcelValue(ref rt, "{" + string.Format(ExcelName, phase, minmax) + "}");
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in CheckForPaintExcelValue():");
                throw;
            }
        }

        protected virtual void CheckForPaintExcelValue(ref string rt, string ExcelName,
             float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelName)
                {
                    case "KUAвT":
                    case "KUBвT":
                    case "KUCвT":

                    case "KUABвT":
                    case "KUBCвT":
                    case "KUCAвT":

                    case "K2UвT":
                    case "K0UвT":

                    case "dfT1":
                        limit = 5; break;

                    case "KUAнбT":
                    case "KUBнбT":
                    case "KUCнбT":

                    case "KUABнбT":
                    case "KUBCнбT":
                    case "KUCAнбT":

                    case "K2UнбT":
                    case "K0UнбT":

                    case "dfT2":
                        limit = 0; break;
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
                EmService.DumpException(ex, "Error in CheckForPaintExcelValue():");
                throw;
            }
        }

        protected virtual void CheckForPaintExcelValueHarm(ref string rt,
            string ExcelNameStart, string ExcelNameNumber, string ExcelNameEnd,
             float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelNameStart)
                {
                    case "kua":
                    case "kub":
                    case "kuc":
                    case "kuab":
                    case "kubc":
                    case "kuca":
                        switch (ExcelNameEnd)
                        {
                            case "T1": limit = 5; break;
                            case "T2": limit = 0; break;
                        }
                        break;
                }

                if ((limit != -1) && (value > limit))
                {
                    if (StyleName_.Length == 0)
                    {
                        InsertPaintExcelStyle(ref rt);
                    }
                    PaintExcelValue(ref rt, "{" + ExcelNameStart + ExcelNameNumber +
                        ExcelNameEnd + "}");
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in CheckForPaintExcelValueHarm():");
                throw;
            }
        }

        protected virtual void ReplaceExcelName(ref string rt,
            DataGrid datagrid, string ExcelName, string GridName,
            int i, string phase, string minmax, string fl,
            GridColumnStylesCollection st)
        {
            float value;
            Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName])], out value);

            CheckForPaintExcelValue(ref rt, ExcelName, phase, minmax, value);

            rt = rt.Replace("{" +
                string.Format(ExcelName, phase, minmax) +
                "}", value.ToString(fl));
        }

        protected virtual void ReplaceExcelName(ref string rt, DataGrid datagrid, string ExcelName,
            string GridName1, string GridName2, int i, string phase, string minmax, string fl,
            GridColumnStylesCollection st)
        {
            try
            {
                float value;
                Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
                float value2;
                Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName2])], out value2);
                value += value2;

                CheckForPaintExcelValue(ref rt, ExcelName, phase, minmax, value);

                rt = rt.Replace("{" +
                    string.Format(ExcelName, phase, minmax) +
                    "}", value.ToString(fl));
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Exception in ReplaceExcelName():");
                throw;
            }
        }

        protected virtual void ReplaceExcelName(ref string rt, DataGrid datagrid,
            string ExcelName, string GridName, int i, string fl,
            GridColumnStylesCollection st)
        {
            float value = (float)datagrid[i, st.IndexOf(st[GridName])];

            CheckForPaintExcelValue(ref rt, ExcelName, value);

            rt = rt.Replace("{" +
                ExcelName +
                "}", value.ToString(fl));
        }

        protected virtual void ReplaceExcelName(ref string rt, DataGrid datagrid, string ExcelName,
            string GridName1, string GridName2, int i, string fl,
            GridColumnStylesCollection st)
        {
            float value = (float)datagrid[i, st.IndexOf(st[GridName1])] +
                (float)datagrid[i, st.IndexOf(st[GridName2])];

            CheckForPaintExcelValue(ref rt, ExcelName, value);

            rt = rt.Replace("{" + ExcelName + "}", value.ToString(fl));
        }

        protected virtual void ReplaceExcelNameHarm(ref string rt, DataGrid datagrid,
            string ExcelNameStart, string ExcelNameNumber, string ExcelNameEnd,
            string GridName, int i, string fl,
            GridColumnStylesCollection st)
        {
            float value = (float)datagrid[i, st.IndexOf(st[GridName])];

            CheckForPaintExcelValueHarm(ref rt,
                ExcelNameStart, ExcelNameNumber, ExcelNameEnd, value);

            rt = rt.Replace("{" +
                ExcelNameStart +
                ExcelNameNumber +
                ExcelNameEnd +
                "}", value.ToString(fl));
        }

        protected virtual void ReplaceExcelNameHarm(ref string rt, DataGrid datagrid,
            string ExcelNameStart, string ExcelNameNumber, string ExcelNameEnd,
            string GridName1, string GridName2, int i, string fl,
            GridColumnStylesCollection st)
        {
            float value = (float)datagrid[i, st.IndexOf(st[GridName1])] +
                (float)datagrid[i, st.IndexOf(st[GridName2])];

            CheckForPaintExcelValueHarm(ref rt,
                ExcelNameStart, ExcelNameNumber, ExcelNameEnd,
                value);

            rt = rt.Replace("{" +
                ExcelNameStart +
                ExcelNameNumber +
                ExcelNameEnd +
                "}", value.ToString(fl));
        }
    }
}
