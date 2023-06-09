using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Resources;
using System.IO;
using System.Globalization;

using DataGridColumnStyles;
using EmServiceLib;

namespace XML.Excel
{
	public class Exporter
	{
		#region Fields

        private String fileName_ = string.Empty;
        private int maxCacheSize_ = 10240;
        private StreamWriter sw_;

		private DataGridList dataGridsList_ = new DataGridList();

		private String title_ = String.Empty;
		private String subject_ = String.Empty;
		private String author_ = String.Empty;
		private String company_ = String.Empty;

		private Font captionFont_ = new Font("Verdana", 9, FontStyle.Bold);
		private Color captionForeColor_ = Color.Black;
		private Color captionBgColor_ = Color.White;

		private Font cellFont_ = new Font("Arial", 9, FontStyle.Regular);

		private int floatSigns_ = 2;

		#endregion

		#region Constructor

		public Exporter(int floatSigns)
		{
			floatSigns_ = floatSigns;
		}

		#endregion

		#region Properties

		/// <summary>
        /// Gets report file name
        /// </summary>
        public String FileName
        {
            get { return fileName_; }
        }

        /// <summary>
        /// Gets or sets maximum size of the cache
        /// </summary>
        public int MaxCacheSize
        {
            get { return maxCacheSize_; }
            set { maxCacheSize_ = value; }
        }

		/// <summary>
		/// Gets access to the list of DataGrid objects
		/// </summary>
		public DataGridList Grids
		{
			get { return dataGridsList_; }
		}

		/// <summary>Gets or sets document title_</summary>
		public String Title
		{
			get { return title_; }
			set { title_ = value; }
		}

		/// <summary>Gets or sets document subject_</summary>
		public String Subject
		{
			get { return subject_; }
			set { subject_ = value; }
		}

		/// <summary>Gets or sets document author_</summary>
		public String Author
		{
			get { return author_; }
			set { author_ = value; }
		}

		/// <summary>Gets or sets document company_</summary>
		public String Company
		{
			get { return company_; }
			set { company_ = value; }
		}

		/// <summary>Gets or sets font of the table caption</summary>
		public Font CaptionFont
		{
			get { return captionFont_; }
			set { captionFont_ = value; }
		}

		/// <summary>Gets or sets caption fore color</summary>
		public Color CaptionForeColor
		{
			get { return captionForeColor_; }
			set { captionForeColor_ = value; }
		}

		/// <summary>Gets or sets caption background color</summary>
		public Color CaptionBgColor
		{
			get { return captionBgColor_; }
			set { captionBgColor_ = value; }
		}

		/// <summary>Gets or sets font of the table data rows</summary>
		public Font CellFont
		{
			get { return cellFont_; }
			set { cellFont_ = value; }
		}

		#endregion

		#region Private methods BuildXXX (with resources)

		/// <summary>Load workbook header from resources</summary>
		/// <returns>XML data</returns>
		private void BuildWorkbookHeader(ref StringBuilder sb)
		{
			ResourceManager rm = new ResourceManager("XML.Excel.pattetns", this.GetType().Assembly);
			sb.Append(rm.GetString("WorkbookHeader"));
		}

		/// <summary>Load workbook footer from resources</summary>
		/// <returns>XML data</returns>
        private void BuildWorkbookFooter(ref StringBuilder sb)
		{
			ResourceManager rm = new ResourceManager("XML.Excel.pattetns", this.GetType().Assembly);
			sb.Append(rm.GetString("WorkbookFooter"));
		}

		/// <summary>Load document properties footer from resources and inserts values</summary>
		/// <param name="Title">Title</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Author">Author</param>
		/// <param name="LastAuthor">LastAuthor</param>
		/// <param name="Created">Created datetime</param>
		/// <param name="Company">Company</param>
		/// <param name="Version">Version</param>
		/// <returns>XML data</returns>
		private void BuildDocumentProperties(ref StringBuilder sb,
			String Title, String Subject, String Author, String LastAuthor, 
			DateTime Created, String Company, String Version)
		{
			ResourceManager rm = new ResourceManager("XML.Excel.pattetns", this.GetType().Assembly);
			sb.Append(String.Format(rm.GetString("DocumentProperties"), Title, Subject, Author,
				LastAuthor, Created, Company, Version));
		}

		/// <summary>
		/// Load styles from resources
		/// </summary>
		/// <returns>XML data</returns>
        private void BuildStyles(ref StringBuilder sb)
		{
			ResourceManager rm = new ResourceManager("XML.Excel.pattetns", this.GetType().Assembly);
			
			String strWebColor = String.Empty;
			if (captionBgColor_.R < 0x10) strWebColor += "0";
			strWebColor += captionBgColor_.R.ToString("X");
			if (captionBgColor_.G < 0x10) strWebColor += "0";
			strWebColor += captionBgColor_.G.ToString("X");
			if (captionBgColor_.B < 0x10) strWebColor += "0";
			strWebColor += captionBgColor_.B.ToString("X");
			
			sb.Append(String.Format(rm.GetString("Styles"),
				captionFont_.Name,
				captionFont_.Size.ToString("0.00", new CultureInfo("en-US")),
				captionFont_.Bold ? "1" : "0",
				captionFont_.Italic ? "1" : "0",
				strWebColor, 
				cellFont_.Name,
				cellFont_.Size.ToString("0.00", new CultureInfo("en-US")),
				cellFont_.Bold ? "1" : "0",
				cellFont_.Italic ? "1" : "0"));
		}

		/// <summary>
		/// Loads worksheet options from resources
		/// </summary>
		/// <returns></returns>
		private void BuildWorksheetOptions(ref StringBuilder sb)
		{
			ResourceManager rm = new ResourceManager("XML.Excel.pattetns", this.GetType().Assembly);
			sb.Append(rm.GetString("WorksheetOptions"));
		}

		#endregion

		#region Private methods BuildXXX (XML generation)

        private void BuildBody(ref StringBuilder sb)
		{
			try
			{
				for (int i = 0; i < dataGridsList_.Count; i++)
				{
					BuildWorksheet(ref sb, dataGridsList_[i], dataGridsList_[i, true]);
					BuildWorksheetOptions(ref sb);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildBody failed:");
				throw;
			}
		}

		private void BuildWorksheet(ref StringBuilder sb, DataGrid dg, String cap)
		{			
			sb.AppendLine(" <Worksheet ss:Name=\"" + cap + "\">");
			BuildTable(ref sb, dg);
			sb.AppendLine(" </Worksheet>");
		}

        private void BuildTable(ref StringBuilder sb, DataGrid dg)
		{
			try
			{
				sb.AppendLine("  <Table>");
				BuildColumns(dg, ref sb);
				BuildColumnGroupCaptions(dg, ref sb);
				BuildColumnCaptions(dg, ref sb);
				BuildDataRows(dg, ref sb);
				sb.AppendLine("  </Table>");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTable failed:");
				throw;
			}
		}

        private void BuildColumns(DataGrid dg, ref StringBuilder sb)
		{
			try
			{
				Graphics gr = Graphics.FromHwnd((new Panel()).Handle);
				for (int i = 0; i < dg.TableStyles[0].GridColumnStyles.Count; i++)
				{
					DataGridColumnStyle style = dg.TableStyles[0].GridColumnStyles[i];
					if (style.Width == 0) continue;
					float colWidth = 0;
					if (style is DataGridColumnGroupCaption)
					{
						// вычисляем ширину столбцов группы и задаем каждому столбцу ширину 
						// в зависимости от их количества в группе
						colWidth = (float)Math.Round((gr.MeasureString((style as DataGridColumnGroupCaption).GroupCaption.Text, this.captionFont_).ToSize().Width) / ((style as DataGridColumnGroupCaption).GroupCaption.Colspan * 1.00), 2);
					}

					// Теперь смотрим - умещается ли текст заголовка столбца
					// в только что рассчитанном на основании группового заголовка
					string strHeader;
					if (style is DataGridColumnHeaderFormula)
					{
						if ((style as DataGridColumnHeaderFormula).HeaderIsFormula)
						{
							strHeader = (style as DataGridColumnHeaderFormula).HeaderFormula;
						}
						else
						{
							strHeader = style.HeaderText;
						}
					}
					else
					{
						strHeader = style.HeaderText;
					}

					float _colWidth = (float)Math.Round(gr.MeasureString(strHeader, this.cellFont_).ToSize().Width * 1.00, 2);
					// и если нет - расширяем
					if (colWidth < _colWidth) colWidth = _colWidth;
					if (colWidth < 50) colWidth = 50;
					if (style.MappingName == "event_datetime")
					{
						colWidth = 100;
					}
					else if (style.MappingName == "colStart" ||
						style.MappingName == "colEnd" ||
						style.MappingName == "colDuration" ||
						style.MappingName == "dt_start" || style.MappingName == "dt_end")
					{
						colWidth = 110;
					}
					else if (style.MappingName == "colDeviation")
					{
						colWidth = 120;
					}
					else if (style.MappingName == "colU")
					{
						colWidth = 100;
					}
					else if (style.MappingName == "colEvent")
					{
						colWidth = 110;
					}

					// полученный результат сохраняем
					sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in BuildColumns():  ");
				throw;
			}
		}

        private void BuildColumnGroupCaptions(DataGrid dg, ref StringBuilder sb)
		{
			try
			{
				if (!(dg.TableStyles[0].GridColumnStyles[0] is DataGridColumnGroupCaption)) return;

				sb.AppendLine("   <Row>");
				for (int i = 0; i < dg.TableStyles[0].GridColumnStyles.Count; i++)
				{
					// упрощаем себе жизнь, создаем указатель "style"
					DataGridColumnGroupCaption style = 
						(DataGridColumnGroupCaption)dg.TableStyles[0].GridColumnStyles[i];

					if (dg.DataSource == null) 
						continue;

					DataTable table = ((DataSet)dg.DataSource).Tables[0];
					// если ширина равна нулю, то на это я пойтить не могу!
					if (style.Width == 0) continue;
					bool bContinue = true;
					for (int zz = 0; zz < table.Columns.Count; zz++)
					{
						if (style.MappingName == table.Columns[zz].ColumnName)
						{
							bContinue = false; break;
						}
						else if (style.MappingName.ToLower() == table.Columns[zz].ColumnName.ToLower())
						{
							bContinue = false; break;
						}
					}
					if (bContinue) continue;
					String strCell = String.Format("    <Cell ss:MergeAcross=\"{0}\" ss:StyleID=\"EmHeader\"><Data ss:Type=\"String\">{1}</Data></Cell>", style.ColumnsInGroup - 1, style.GroupCaption.Text);
					sb.AppendLine(strCell);

					i += (style.ColumnsInGroup - 1);
				}
				sb.AppendLine("   </Row>");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildColumnGroupCaptions failed:");
				throw;
			}
		}

        private void BuildColumnCaptions(DataGrid dg, ref StringBuilder sb)
		{
			try
			{
				sb.AppendLine("   <Row>");
				for (int iColStyle = 0; iColStyle < dg.TableStyles[0].GridColumnStyles.Count; iColStyle++)
				{
					// упрощаем себе жизнь, создаем указатель "style"
					DataGridColumnStyle style = dg.TableStyles[0].GridColumnStyles[iColStyle];

					if (dg.DataSource == null) continue;

					DataTable table = ((DataSet)dg.DataSource).Tables[0];
					// если ширина равна нулю, то на это я пойтить не могу!
					if (style.Width == 0) continue;
					bool bContinue = true;
					for (int iCol = 0; iCol < table.Columns.Count; iCol++)
					{
						if (style.MappingName == table.Columns[iCol].ColumnName)
						{
							bContinue = false; break;
						}
						else if (style.MappingName.ToLower() == table.Columns[iCol].ColumnName.ToLower())
						{
							bContinue = false; break;
						}
					}
					if (bContinue) continue;
					String strCell = String.Empty;
					if (style is DataGridColumnHeaderFormula)
					{
						if ((style as DataGridColumnHeaderFormula).HeaderIsFormula)
						{
							// Необходимо преобразовать каждый нечетный символ "_" в тег "<Sub>"
							// а каждый четный в "</Sub>"						
							string header = Underline2Sub((style as DataGridColumnHeaderFormula).HeaderFormula.Trim());
							strCell = String.Format("        <Cell ss:StyleID=\"EmHeader\"><ss:Data xmlns=\"http://www.w3.org/TR/REC-html40\" ss:Type=\"String\">{0}</ss:Data></Cell>", header);
						}
						else
						{
							strCell = String.Format("        <Cell ss:StyleID=\"EmHeader\"><Data ss:Type=\"String\">{0}</Data></Cell>", style.HeaderText);
						}
					}
					else
					{
						strCell = String.Format("        <Cell ss:StyleID=\"EmHeader\"><Data ss:Type=\"String\">{0}</Data></Cell>", style.HeaderText);
					}
					sb.AppendLine(strCell);
				}
				sb.AppendLine("   </Row>");
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in BuildColumnCaptions(): ");
				throw e;
			}
		}

		private bool IsSimpleText(String strColStyleName)
		{
			switch (strColStyleName)
			{
				case "colEventDatetime":

				case "colU_A":
				case "colU_B":
				case "colU_C":

				case "colU_AB":
				case "colU_BC":
				case "colU_CA":

				case "colI_A":
				case "colI_B":
				case "colI_C":

				case "colP_A":
				case "colP_B":
				case "colP_C":

				case "colP_1":
				case "colP_2":

				case "colP_summ":

				case "colQ_1":
				case "colQ_2":
				case "colQ_3":

				case "colQ_sum":

					return true;
			}

			return false;
		}

		private void BuildDataRows(DataGrid dg, ref StringBuilder sb)
		{
			int iRow = 0;
			int iColumn = 0;
			try
			{
				if (dg.DataSource == null) return;

				DataTable table = ((DataSet)dg.DataSource).Tables[0];
				DataGridTableStyle tableStyle = dg.TableStyles[0];

				// DEBUG 
				//if (table.TableName.Equals("day_avg_parameters_t4"))
				//{
				//   System.Diagnostics.Debug.WriteLine("******************** tableStyle.GridColumnStyles ***********************");
				//   for (int zz = 0; zz < tableStyle.GridColumnStyles.Count; zz++)
				//   {
				//      System.Diagnostics.Debug.WriteLine(
				//         string.Format("Column {0}::{1}('{2}') width is {3}",
				//         (tableStyle.GridColumnStyles[zz] as DataGridColumnStyles.DataGridColumnGroupCaption).GroupCaption.Text,
				//         tableStyle.GridColumnStyles[zz].HeaderText == string.Empty ? "<empty>" : tableStyle.GridColumnStyles[zz].HeaderText,
				//         tableStyle.GridColumnStyles[zz].MappingName,
				//         tableStyle.GridColumnStyles[zz].Width));
				//   }

				//   System.Diagnostics.Debug.WriteLine(string.Empty);
				//   System.Diagnostics.Debug.WriteLine("******************** table.Columns ***********************");
				//   for (int zz = 0; zz < table.Columns.Count; zz++)
				//   {
				//      System.Diagnostics.Debug.WriteLine(
				//         "Column.Caption: " + table.Columns[zz].Caption + "\tColumn.ColumnName: " + table.Columns[zz].ColumnName);
				//   }
				//}
				// END DEBUG

				//foreach (DataRow row in table.Rows)
				for(iRow = 0; iRow < table.Rows.Count; ++iRow)
				{
					DataRow row = table.Rows[iRow];

					sb.AppendLine("   <Row>");
					for (int iStyle = 0; iStyle < tableStyle.GridColumnStyles.Count; iStyle++)
					{
						String strCell = String.Empty;

						DataGridColumnStyle dgStyle = tableStyle.GridColumnStyles[iStyle];
						if (dgStyle.Width == 0) continue;
						bool bContinue = true;
						for (int zz = 0; zz < table.Columns.Count; zz++)
						{
							if (dgStyle.MappingName == table.Columns[zz].ColumnName)
							{
								bContinue = false; break;
							}
							else if (dgStyle.MappingName.ToLower() == table.Columns[zz].ColumnName.ToLower())
							{
								bContinue = false; break;
							}
						}
						if (bContinue) continue;

						#region Различная обработка столбцов различного типа

						// для имени параметра
						if (dgStyle.MappingName.Equals("name") || dgStyle.MappingName.Equals("phase"))
						{
							string param = Underline2Sub(row[dgStyle.MappingName].ToString().Trim());
							strCell = String.Format("    <Cell ss:StyleID=\"EmParamName\"><ss:Data xmlns=\"http://www.w3.org/TR/REC-html40\" ss:Type=\"String\">{0}</ss:Data></Cell>", param);
						}
						else if (dgStyle.MappingName.Equals("ColumnStartDateTime") ||
							dgStyle.MappingName.Equals("dt_start") ||
							dgStyle.MappingName.Equals("ColumnEndDateTime") ||
							dgStyle.MappingName.Equals("ColumnPeriodTime") ||
							dgStyle.MappingName.Equals("ColumnEvent") ||
							dgStyle.MappingName.Equals("ColumnPhase") ||
							dgStyle.MappingName.Equals("ColumnDeviation"))
						{
							strCell = String.Format("    <Cell ss:StyleID=\"EmParamName\"><ss:Data xmlns=\"http://www.w3.org/TR/REC-html40\" ss:Type=\"String\">{0}</ss:Data></Cell>",
								row[dgStyle.MappingName].ToString());
						}
						else if (IsSimpleText(dgStyle.MappingName))
						{
							strCell = String.Format("    <Cell ss:StyleID=\"EmParamName\"><ss:Data xmlns=\"http://www.w3.org/TR/REC-html40\" ss:Type=\"String\">{0}</ss:Data></Cell>",
								row[dgStyle.MappingName].ToString());
						}

						// для времени события
						else if (dgStyle.MappingName.Equals("event_datetime") ||
							dgStyle.MappingName.Equals("dt_start"))
						{
							DateTime datetime = ((DateTime)row[dgStyle.MappingName]);

							if (table.TableName == "ad_converter_values")
							{
								strCell = String.Format("    <Cell ss:StyleID=\"EmEventDatetime\"><Data ss:Type=\"String\">{0}</Data></Cell>", datetime.ToString("HH:mm:ss.fffffff"));
							}
							else
							{
								// fix bug
								strCell = String.Format("    <Cell ss:StyleID=\"EmEventDatetime\"><Data ss:Type=\"DateTime\">{1}-{2:00}-{3:00}T{0}.000</Data></Cell>", String.Format("{1:00}{0}{2:00}{0}{3:00}", CultureInfo.InstalledUICulture.DateTimeFormat.TimeSeparator, datetime.Hour, datetime.Minute, datetime.Second), datetime.Year, datetime.Month, datetime.Day);
							}
						}

							// для времен провалов и перенапряжений
						else if (dgStyle.MappingName.Equals("common_duration") || dgStyle.MappingName.Equals("max_period_period") || dgStyle.MappingName.Equals("max_value_period"))
						{
							DateTime datetime = ((DateTime)row[dgStyle.MappingName]);
							strCell = String.Format("    <Cell ss:StyleID=\"EmTimespan\"><Data ss:Type=\"DateTime\">1899-12-31T{0}.000</Data></Cell>", String.Format("{1:00}{0}{2:00}{0}{3:00}", CultureInfo.InstalledUICulture.DateTimeFormat.TimeSeparator, datetime.Hour, datetime.Minute, datetime.Second));
						}

						// для значений провалов и перенапряжений
						else if (dgStyle.MappingName.Equals("common_number") || dgStyle.MappingName.Equals("max_period_value") || dgStyle.MappingName.Equals("max_value_value"))
						{
							if (dgStyle.MappingName.Equals("common_number"))
							{
								strCell = String.Format("    <Cell ss:StyleID=\"EmNumber\"><Data ss:Type=\"Number\">{0}</Data></Cell>", (Int64)row[dgStyle.MappingName]);
							}
							else
							{
								string strVal = string.Empty;
								if (row[dgStyle.MappingName] is Single)
								{
									strVal = ((float)(row[dgStyle.MappingName])).ToString(new CultureInfo("en-US"));
								}
								else if (row[dgStyle.MappingName] is Double)
								{
									strVal = ((double)(row[dgStyle.MappingName])).ToString(new CultureInfo("en-US"));
								}
								strCell = String.Format("    <Cell ss:StyleID=\"EmPercent\"><Data ss:Type=\"Number\">{0}</Data></Cell>", strVal);
							}
						}

						// для процентных величин
						#region Percents

						else if ((dgStyle as DataGridTextBoxColumn).Format.Equals(
								DataColumnsFormat.GetPercentFormat(floatSigns_)))
						{
							string strStyleID = string.Empty;
							if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorCommon)
								strStyleID = "EmPercentCommon";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorAvgPhaseA)
								strStyleID = "EmPercentPhA";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorAvgPhaseB)
								strStyleID = "EmPercentPhB";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorAvgPhaseC)
								strStyleID = "EmPercentPhC";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorPkeResult)
								strStyleID = "EmPercentRes";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorPkeStandard)
								strStyleID = "EmPercentSt";
							else strStyleID = "EmPercent";
							if (row[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>, ", strStyleID);
							}
							else
							{
								float curValue;
								if (!Conversions.object_2_float_en_ru(
									row[dgStyle.MappingName], out curValue))
									EmService.WriteToLogFailed("Exporter: error conversion S108!");
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, (curValue / 100).ToString(new CultureInfo("en-US")));
							}
						}

						#endregion

						// для временных отрезков
						#region Timespans

						else if (dgStyle.MappingName.Equals("flik_time"))
						{
							strCell = String.Format("    <Cell ss:StyleID=\"EmTimespan\"><Data ss:Type=\"String\">{0}</Data></Cell>", row[dgStyle.MappingName].ToString());
						}

						else if (dgStyle is DataGridColumnTimespan)
						{
							string strStyleID = string.Empty;
							if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorCommon)
								strStyleID = "EmTimespanCommon";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorAvgPhaseA)
								strStyleID = "EmTimespanPhA";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorAvgPhaseB)
								strStyleID = "EmTimespanPhB";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorAvgPhaseC)
								strStyleID = "EmTimespanPhC";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorPkeResult)
								strStyleID = "EmTimespanRes";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor == DataGridColors.ColorPkeStandard)
								strStyleID = "EmTimespanSt";
							else
								strStyleID = "EmTimespan";

							TimeSpan timespan = new TimeSpan((long)row[dgStyle.MappingName]);

							strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"DateTime\">1899-12-31T{1}.000</Data></Cell>", strStyleID, String.Format("{1:00}{0}{2:00}{0}{3:00}", CultureInfo.InstalledUICulture.DateTimeFormat.TimeSeparator, timespan.Hours, timespan.Minutes, timespan.Seconds));
						}

						#endregion

						// для всех остальных (коие являются просто числовыми данными или строковыми ,kzlm)
						else
						{
							string strStyleID = string.Empty;
							if ((dgStyle as DataGridColumnOneColor).BackgroungColor ==
								DataGridColors.ColorCommon)
								strStyleID = "EmNumberCommon";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor ==
								DataGridColors.ColorAvgPhaseA)
								strStyleID = "EmNumberPhA";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor ==
								DataGridColors.ColorAvgPhaseB)
								strStyleID = "EmNumberPhB";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor ==
								DataGridColors.ColorAvgPhaseC)
								strStyleID = "EmNumberPhC";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor ==
								DataGridColors.ColorPkeResult)
								strStyleID = "EmNumberRes";
							else if ((dgStyle as DataGridColumnOneColor).BackgroungColor ==
								DataGridColors.ColorPkeStandard)
								strStyleID = "EmNumberSt";
							else strStyleID = "EmNumber";

							String strValue = String.Empty;
							bool bStr = false;
							if (row[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(row[dgStyle.MappingName])).ToString();
							}
							if (row[dgStyle.MappingName] is Int16)
							{
								strValue = ((short)(row[dgStyle.MappingName])).ToString();
							}
							else if (row[dgStyle.MappingName] is Single)
							{
								float num = (float)row[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (row[dgStyle.MappingName] is Double)
							{
								double num = (double)row[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (row[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}
							else //if (row[dgStyle.MappingName] is String)
							{
								strValue = row[dgStyle.MappingName].ToString();
								bStr = true;
							}

							if (strCell.Equals(String.Empty))
							{
								if (bStr)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", 
										strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", 
										strStyleID, strValue);
							}
						}

						#endregion

						sb.AppendLine(strCell);

						++iColumn;
					}
					sb.AppendLine("   </Row>");

					CheckCache(ref sb);
				}
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in BuildDataRows(): ");
				throw e;
			}
		}

		/// <summary>
		/// Converts any odd symbol "_" to the tag <!--<Sub>--> 
		/// and any even to the tab <!--</Sub>-->
		/// </summary>
		/// <param name="source">Source string with "_" symbols</param>
		/// <returns>Destination string with <!--<Sub>-->/<!--</Sub>--> tags</returns>
		
        private string Underline2Sub(String source)
		{
			try
			{
				string[] header_arr = source.Split(new string[1] { "_" }, StringSplitOptions.RemoveEmptyEntries);
				StringBuilder sb = new StringBuilder(source.Replace("_", String.Empty));

				int curPos = 0;
				for (int i = 0; i < header_arr.Length; i++)
				{
					if (i % 2 == 1)
					{
						sb.Insert(curPos, "<Sub>");
						curPos += 5;
						sb.Insert(curPos + header_arr[i].Length, "</Sub>");
						curPos += 6;
					}
					curPos += header_arr[i].Length;
				}
				return sb.ToString();
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in Underline2Sub(): ");
				throw e;
			}
		}

        private void CheckCache(ref StringBuilder sb)
        {
            if (sb.Length > maxCacheSize_)
            {
                sw_.Write(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }

		#endregion

		#region Private method WriteFile

		private void WriteFile(String FileName, String XMLData)
		{
			System.IO.File.Delete(FileName);
			System.IO.StreamWriter sw_ = new System.IO.StreamWriter(FileName, true, System.Text.Encoding.Unicode);
			sw_.Write(XMLData);
			sw_.Close();
		}

		#endregion

		#region Public methods

        public void Open(String FileName)
        {
            this.fileName_ = FileName;
            File.Delete(this.fileName_);
            sw_ = File.CreateText(this.fileName_);
        }

        public void Close()
        {
            if(sw_ != null) sw_.Close();
        }

		/// <summary>
		/// Creates XML Excel file with data from DataGird's array;
		/// </summary>
		/// <param name="FileName">File name to create</param>
		public void Export()
		{
			StringBuilder sb = new StringBuilder();
			BuildWorkbookHeader(ref sb);
			BuildDocumentProperties(ref sb, title_, subject_, author_, author_, DateTime.Now, company_, "11.5606");
			BuildStyles(ref sb);
			BuildBody(ref sb);
			BuildWorkbookFooter(ref sb);
			sw_.Write(sb.ToString());
		}

		#endregion
	}
}
