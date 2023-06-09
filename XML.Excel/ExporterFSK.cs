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
	public class ExporterFSK
	{
		#region Fields

		private String fileName_ = string.Empty;
		private int maxCacheSize_ = 10240;
		private StreamWriter sw_;

		private DataGridList dataGridsList_ = new DataGridList();
		private DataGridView dgvDNS_;
		private List<TableColumn> listColumnsNormirPQP = new List<TableColumn>();
		private List<TableColumn> listColumnsOther = new List<TableColumn>();
		private List<TableColumn> listColumnsPower = new List<TableColumn>();

		private String title_ = String.Empty;
		private String subject_ = String.Empty;
		private String author_ = String.Empty;
		private String company_ = String.Empty;

		private Font captionFont_ = new Font("Verdana", 9, FontStyle.Bold);
		private Color captionForeColor_ = Color.Black;
		private Color captionBgColor_ = Color.White;

		private Font cellFont_ = new Font("Arial", 9, FontStyle.Regular);

		private int floatSigns_ = 2;
		private ConnectScheme conScheme_;
		private EmDeviceType devType_;
		private float currentRatio_;
		private float voltageRatio_;
		private float powerRatio_;

		private const int pagesCount_ = 7;

		#endregion

		#region Constructor

		public ExporterFSK(int floatSigns, ConnectScheme con, EmDeviceType dt, DataGridView dgvDNS,
							float curRatio, float volRatio, float powerRatio)
		{
			floatSigns_ = floatSigns;
			conScheme_ = con;
			devType_ = dt;
			dgvDNS_ = dgvDNS;
			currentRatio_ = curRatio;
			voltageRatio_ = volRatio;
			powerRatio_ = powerRatio;
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
				for (int iPage = 0; iPage < pagesCount_; iPage++)
				{
					BuildWorksheet(ref sb, (PAGES)iPage);
					BuildWorksheetOptions(ref sb);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildBody failed:");
				throw;
			}
		}

		private void BuildWorksheet(ref StringBuilder sb, PAGES iPage)
		{
			string cap = string.Empty;
			switch (iPage)
			{
				case PAGES.TITUL: cap = "Титул"; break;
				case PAGES.NORMIR_PQP: cap = "Нормир ПКЭ"; break;
				case PAGES.DIP_SWELL: cap = "Провалы и перенапряжения"; break;
				case PAGES.FLIK_PST: cap = "Колебания PSt"; break;
				case PAGES.FLIK_PLT: cap = "Колебания PLt"; break;
				//case 5: cap = "Импульсы"; break;
				case PAGES.OTHER: cap = "Прочие параметры"; break;
				case PAGES.POWER: cap = "Мощность"; break;
			}
			sb.AppendLine(" <Worksheet ss:Name=\"" + cap + "\">");
			BuildTable(ref sb, iPage);
			sb.AppendLine(" </Worksheet>");
		}

		private void BuildTable(ref StringBuilder sb, PAGES iPage)
		{
			try
			{
				sb.AppendLine("  <Table>");
				BuildColumns(iPage, ref sb);
				BuildColumnCaptions(iPage, ref sb);
				BuildDataRows(iPage, ref sb);
				sb.AppendLine("  </Table>");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTable failed:");
				throw;
			}
		}

		private void BuildColumns(PAGES iPage, ref StringBuilder sb)
		{
			try
			{
				if (iPage == PAGES.TITUL) return;		// титульный лист
				else if (iPage == PAGES.NORMIR_PQP)		// лист "Нормир ПКЭ"
				{
					int colWidth = 120;
					sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					colWidth = 80;
					for (int iCol = 0; iCol < /*127*//*126*/128; ++iCol)
					{
						sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
						// <Column ss:Index="127" ss:Width="66"/>
					}
				}
				else if (iPage == PAGES.DIP_SWELL)			// лист "Провалы"
				{
					int colWidth = 120;
					sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					colWidth = 100;
					for (int iCol = 0; iCol < 5; ++iCol)
					{
						sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					}
				}
				else if (iPage == PAGES.FLIK_PST || iPage == PAGES.FLIK_PLT)				// лист "Колебания"
				{
					int colWidth = 120;
					sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					colWidth = 100;
					for (int iCol = 0; iCol < 3; ++iCol)
					{
						sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					}
				}
				else if (iPage == PAGES.OTHER)							// лист "Прочие параметры"
				{
					int colWidth = 120;
					sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					colWidth = 80;
					for (int iCol = 0; iCol < 123; ++iCol)
					{
						sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					}
				}
				else if (iPage == PAGES.POWER)							// лист "Мощность"
				{
					int colWidth = 120;
					sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					colWidth = 80;
					for (int iCol = 0; iCol < 135; ++iCol)
					{
						sb.AppendLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + colWidth.ToString("0.00", new CultureInfo("en-US")) + "\"/>");
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildColumns failed:");
				throw;
			}
		}

		private void BuildColumnCaptions(PAGES iPage, ref StringBuilder sb)
		{
			try
			{
				if (iPage == PAGES.TITUL) return;		// титульный лист

				else if (iPage == PAGES.NORMIR_PQP)		// лист "Нормир ПКЭ"
				{
					// формируем список столбцов, чтобы знать какие из них есть реально
					listColumnsNormirPQP.Add(new TableColumn("event_datetime", "Дата", 120));
					DataGrid dgPQP = dataGridsList_[(int)DATAGRIDS.PQP];
					DataTable tablePQP = null;
					if (dgPQP != null && dgPQP.DataSource != null)
					{
						if (((DataSet)dgPQP.DataSource).Tables.Count > 0)
						{
							tablePQP = ((DataSet)dgPQP.DataSource).Tables[0];
						}
					}
					if (dgPQP == null || tablePQP == null)
					{
						EmService.WriteToLogFailed("BuildColumnCaptions: dgPQP == null || tablePQP == null");
						return;
					}

					DataGrid dgLH = dataGridsList_[(int)DATAGRIDS.VOLTAGE_HARM_LIN];
					DataGrid dgPH = dataGridsList_[(int)DATAGRIDS.VOLTAGE_HARM_PH];
					DataTable tableLH = null;
					DataGridTableStyle tableStyleLH = null;
					DataTable tablePH = null;
					DataGridTableStyle tableStylePH = null;
					if (dgLH != null && dgLH.DataSource != null)
					{
						if (((DataSet)dgLH.DataSource).Tables.Count > 0)
						{
							tableLH = ((DataSet)dgLH.DataSource).Tables[0];
							tableStyleLH = dgLH.TableStyles[0];
							if (tableLH.Rows.Count != tablePQP.Rows.Count)
								tableLH = null;
						}
					}
					if (dgPH != null && dgPH.DataSource != null)
					{
						if (((DataSet)dgPH.DataSource).Tables.Count > 0)
						{
							tablePH = ((DataSet)dgPH.DataSource).Tables[0];
							tableStylePH = dgPH.TableStyles[0];
							if (tablePH.Rows.Count != tablePQP.Rows.Count)
								tablePH = null;
						}
					}

					if (tablePQP.Columns.Contains("d_f"))
					{
						listColumnsNormirPQP.Add(new TableColumn("d_f", "Δf, Гц"));
					}
					if (conScheme_ != ConnectScheme.Ph3W3 && conScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						if (tablePQP.Columns.Contains("d_u_a"))
						{
							listColumnsNormirPQP.Add(new TableColumn("d_u_a", "δUуA, %"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePQP.Columns.Contains("d_u_b"))
							{
								listColumnsNormirPQP.Add(new TableColumn("d_u_b", "δUуB, %"));
							}
							if (tablePQP.Columns.Contains("d_u_c"))
							{
								listColumnsNormirPQP.Add(new TableColumn("d_u_c", "δUуC, %"));
							}
						}
					}
					if (conScheme_ != ConnectScheme.Ph1W2)
					{
						if (tablePQP.Columns.Contains("d_u_ab"))
						{
							listColumnsNormirPQP.Add(new TableColumn("d_u_ab", "δUуAB, %"));
						}
						if (tablePQP.Columns.Contains("d_u_bc"))
						{
							listColumnsNormirPQP.Add(new TableColumn("d_u_bc", "δUуBC, %"));
						}
						if (tablePQP.Columns.Contains("d_u_ca"))
						{
							listColumnsNormirPQP.Add(new TableColumn("d_u_ca", "δUуCA, %"));
						}
					}

					if (conScheme_ != ConnectScheme.Ph1W2)
					{
						if (tablePQP.Columns.Contains("k_2u"))
						{
							listColumnsNormirPQP.Add(new TableColumn("k_2u", "К2u, %"));
						}
					}

					if (conScheme_ != ConnectScheme.Ph3W3 && conScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						if (tablePQP.Columns.Contains("k_ua_ab"))
						{
							listColumnsNormirPQP.Add(new TableColumn("k_ua_ab", "КuA, %"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePQP.Columns.Contains("k_ub_bc"))
							{
								listColumnsNormirPQP.Add(new TableColumn("k_ub_bc", "КuB, %"));
							}
							if (tablePQP.Columns.Contains("k_uc_ca"))
							{
								listColumnsNormirPQP.Add(new TableColumn("k_uc_ca", "КuC, %"));
							}
						}
					}
					else
					{
						if (tablePQP.Columns.Contains("k_ua_ab"))
						{
							listColumnsNormirPQP.Add(new TableColumn("k_ua_ab", "КuAB, %"));
						}
						if (tablePQP.Columns.Contains("k_ub_bc"))
						{
							listColumnsNormirPQP.Add(new TableColumn("k_ub_bc", "КuBC, %"));
						}
						if (tablePQP.Columns.Contains("k_uc_ca"))
						{
							listColumnsNormirPQP.Add(new TableColumn("k_uc_ca", "КuCA, %"));
						}
					}

					if (devType_ == EmDeviceType.ETPQP || devType_ == EmDeviceType.EM32)
					{
						#region EtPQP, Em32

						if (conScheme_ != ConnectScheme.Ph3W3 &&
							conScheme_ != ConnectScheme.Ph3W3_B_calc)
						{
							if (tablePH != null)
							{
								for (int iCol = 2; iCol < 41; ++iCol)
								{
									if (tablePH.Columns.Contains(string.Format("k_ua_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_ua_{0}", iCol),
											string.Format("Кu({0})A, %", iCol)));
									}
									if (conScheme_ != ConnectScheme.Ph1W2)
									{
										if (tablePH.Columns.Contains(string.Format("k_ub_{0}", iCol)))
										{
											listColumnsNormirPQP.Add(new TableColumn(
												string.Format("k_ub_{0}", iCol),
												string.Format("Кu({0})B, %", iCol)));
										}
										if (tablePH.Columns.Contains(string.Format("k_uc_{0}", iCol)))
										{
											listColumnsNormirPQP.Add(new TableColumn(
												string.Format("k_uc_{0}", iCol),
												string.Format("Кu({0})C, %", iCol)));
										}
									}
								}
							}
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tableLH != null)
							{
								for (int iCol = 2; iCol < 41; ++iCol)
								{
									if (tableLH.Columns.Contains(string.Format("k_uab_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_uab_{0}", iCol),
											string.Format("Кu({0})AB, %", iCol)));
									}
									if (tableLH.Columns.Contains(string.Format("k_ubc_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_ubc_{0}", iCol),
											string.Format("Кu({0})BC, %", iCol)));
									}
									if (tableLH.Columns.Contains(string.Format("k_uca_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_uca_{0}", iCol),
											string.Format("Кu({0})CA, %", iCol)));
									}
								}
							}
						}

						#endregion
					}
					else     // Em33T
					{
						#region Em33T

						if (conScheme_ != ConnectScheme.Ph3W3 &&
							conScheme_ != ConnectScheme.Ph3W3_B_calc)
						{
							if (tablePH != null)
							{
								for (int iCol = 2; iCol < 41; ++iCol)
								{
									if (tablePH.Columns.Contains(string.Format("k_ua_ab_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_ua_ab_{0}", iCol),
											string.Format("Кu({0})A, %", iCol)));
									}
									if (conScheme_ != ConnectScheme.Ph1W2)
									{
										if (tablePH.Columns.Contains(string.Format("k_ub_bc_{0}", iCol)))
										{
											listColumnsNormirPQP.Add(new TableColumn(
												string.Format("k_ub_bc_{0}", iCol),
												string.Format("Кu({0})B, %", iCol)));
										}
										if (tablePH.Columns.Contains(string.Format("k_uc_ca_{0}", iCol)))
										{
											listColumnsNormirPQP.Add(new TableColumn(
												string.Format("k_uc_ca_{0}", iCol),
												string.Format("Кu({0})C, %", iCol)));
										}
									}
								}
							}
						}
						else   // 3p3w
						{
							if (tableLH != null)
							{
								for (int iCol = 2; iCol < 41; ++iCol)
								{
									if (tableLH.Columns.Contains(string.Format("k_ua_ab_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_ua_ab_{0}", iCol),
											string.Format("Кu({0})AB, %", iCol)));
									}
									if (tableLH.Columns.Contains(string.Format("k_ub_bc_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_ub_bc_{0}", iCol),
											string.Format("Кu({0})BC, %", iCol)));
									}
									if (tableLH.Columns.Contains(string.Format("k_uc_ca_{0}", iCol)))
									{
										listColumnsNormirPQP.Add(new TableColumn(
											string.Format("k_uc_ca_{0}", iCol),
											string.Format("Кu({0})CA, %", iCol)));
									}
								}
							}
						}

						#endregion
					}

					// вносим в таблицу
					sb.AppendLine("   <Row>");
					for (int iListCol = 0; iListCol < listColumnsNormirPQP.Count; ++iListCol)
					{
						string colName = listColumnsNormirPQP[iListCol].VisibleName;
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));
					}
					sb.AppendLine("   </Row>");
				}

				else if (iPage == PAGES.DIP_SWELL)		// лист "Провалы"
				{
					sb.AppendLine("   <Row>");
					string colName = "Дата начала";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					colName = "Дата окончания";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					colName = "Длительность, сек";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					colName = "Событие";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					colName = "Фаза";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					colName = "Отклонение, %";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					sb.AppendLine("   </Row>");
				}

				else if (iPage == PAGES.FLIK_PST || iPage == PAGES.FLIK_PLT)		// лист "Колебания"
				{
					sb.AppendLine("   <Row>");

					string colName = "Дата";
					sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

					if (conScheme_ != ConnectScheme.Ph3W3 && conScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						colName = "Ua";
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							colName = "Ub";
							sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

							colName = "Uc";
							sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));
						}
					}
					else
					{
						colName = "Uab";
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

						colName = "Ubc";
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));

						colName = "Uca";
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));
					}

					sb.AppendLine("   </Row>");
				}

				else if (iPage == PAGES.OTHER)		// лист "Прочие параметры"
				{
					#region Other params

					// формируем список столбцов, чтобы знать какие из них есть реально
					listColumnsOther.Add(new TableColumn("event_datetime", "Дата", 120));

					DataGrid dgI = dataGridsList_[(int)DATAGRIDS.CURRENT];
					DataGrid dgPQP = dataGridsList_[(int)DATAGRIDS.PQP];
					DataGrid dgIHarm = dataGridsList_[(int)DATAGRIDS.CURRENT_HARM];

					if (dgI == null || dgI.DataSource == null)
					{
						EmService.WriteToLogFailed("BuildColumnCaptions: dgI == null || tableI == null");
						return;
					}

					DataTable tableI = ((DataSet)dgI.DataSource).Tables[0];

					DataTable tablePQP = null;
					DataTable tableIHarm = null;
					if (dgPQP != null && dgPQP.DataSource != null)
					{
						if (((DataSet)dgPQP.DataSource).Tables.Count > 0)
						{
							tablePQP = ((DataSet)dgPQP.DataSource).Tables[0];
							if (tablePQP.Rows.Count != tableI.Rows.Count)
								tablePQP = null;
						}
					}
					if (dgIHarm != null && dgIHarm.DataSource != null)
					{
						if (((DataSet)dgIHarm.DataSource).Tables.Count > 0)
						{
							tableIHarm = ((DataSet)dgIHarm.DataSource).Tables[0];
							if (tableIHarm.Rows.Count != tableI.Rows.Count)
								tableIHarm = null;
						}
					}

					if (tableI.Columns.Contains("i_a"))
					{
						if(currentRatio_ < 1)
							listColumnsOther.Add(new TableColumn("i_a", "Ia, кA"));
						else listColumnsOther.Add(new TableColumn("i_a", "Ia, A"));
					}
					if (conScheme_ != ConnectScheme.Ph1W2)
					{
						if (tableI.Columns.Contains("i_b"))
						{
							if (currentRatio_ < 1)
								listColumnsOther.Add(new TableColumn("i_b", "Ib, кA"));
							else listColumnsOther.Add(new TableColumn("i_b", "Ib, A"));
						}
						if (tableI.Columns.Contains("i_c"))
						{
							if (currentRatio_ < 1)
								listColumnsOther.Add(new TableColumn("i_c", "Ic, кA"));
							else listColumnsOther.Add(new TableColumn("i_c", "Ic, A"));
						}
					}
					if (tablePQP.Columns.Contains("k_ia"))
					{
						listColumnsOther.Add(new TableColumn("k_ia", "KiA, %"));
					}
					if (conScheme_ != ConnectScheme.Ph1W2)
					{
						if (tablePQP.Columns.Contains("k_ib"))
						{
							listColumnsOther.Add(new TableColumn("k_ib", "KiB, %"));
						}
						if (tablePQP.Columns.Contains("k_ic"))
						{
							listColumnsOther.Add(new TableColumn("k_ic", "KiC, %"));
						}
					}

					for (int iCol = 2; iCol < 41; ++iCol)
					{
						if (tableIHarm.Columns.Contains(string.Format("k_ia_{0}", iCol)))
						{
							listColumnsOther.Add(new TableColumn(
								string.Format("k_ia_{0}", iCol),
								string.Format("КiA({0}), %", iCol)));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tableIHarm.Columns.Contains(string.Format("k_ib_{0}", iCol)))
							{
								listColumnsOther.Add(new TableColumn(
									string.Format("k_ib_{0}", iCol),
									string.Format("КiB({0}), %", iCol)));
							}
							if (tableIHarm.Columns.Contains(string.Format("k_ic_{0}", iCol)))
							{
								listColumnsOther.Add(new TableColumn(
									string.Format("k_ic_{0}", iCol),
									string.Format("КiC({0}), %", iCol)));
							}
						}
					}

					#endregion

					// вносим в таблицу
					sb.AppendLine("   <Row>");
					for (int iListCol = 0; iListCol < listColumnsOther.Count; ++iListCol)
					{
						string colName = listColumnsOther[iListCol].VisibleName;
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));
					}
					sb.AppendLine("   </Row>");
				}

				else if (iPage == PAGES.POWER)		// лист "Мощность"
				{
					#region Power

					// формируем список столбцов, чтобы знать какие из них есть реально
					listColumnsPower.Add(new TableColumn("event_datetime", "Дата", 120));

					DataGrid dgPower = dataGridsList_[(int)DATAGRIDS.POWER];
					DataGrid dgHarmPower = dataGridsList_[(int)DATAGRIDS.HARM_POWER];

					if (dgPower == null || dgPower.DataSource == null)
					{
						EmService.WriteToLogFailed(
							"BuildColumnCaptions: dgPower == null || tablePower == null");
						return;
					}

					DataTable tablePower = ((DataSet)dgPower.DataSource).Tables[0];
					DataTable tableHarmPower = null;
					if (dgHarmPower != null && dgHarmPower.DataSource != null)
					{
						if (((DataSet)dgHarmPower.DataSource).Tables.Count > 0)
						{
							tableHarmPower = ((DataSet)dgHarmPower.DataSource).Tables[0];
							if (tableHarmPower.Rows.Count != tablePower.Rows.Count)
								tableHarmPower = null;
						}
					}

					if (conScheme_ == ConnectScheme.Ph1W2)
					{
						if (tablePower.Columns.Contains("p_a_1"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("p_a_1", "Psum, кВт"));
							else listColumnsPower.Add(new TableColumn("p_a_1", "Psum, Вт"));
						}
					}
					else
					{
						if (tablePower.Columns.Contains("p_sum"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("p_sum", "Psum, кВт"));
							else listColumnsPower.Add(new TableColumn("p_sum", "Psum, Вт"));
						}
						if (conScheme_ == ConnectScheme.Ph3W4 || conScheme_ == ConnectScheme.Ph3W4_B_calc)
						{
							if (tablePower.Columns.Contains("p_a_1"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("p_a_1", "Pa, кВт"));
								else listColumnsPower.Add(new TableColumn("p_a_1", "Pa, Вт"));
							}
							if (tablePower.Columns.Contains("p_b_2"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("p_b_2", "Pb, кВт"));
								else listColumnsPower.Add(new TableColumn("p_b_2", "Pb, Вт"));
							}
							if (tablePower.Columns.Contains("p_c"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("p_c", "Pc, кВт"));
								else listColumnsPower.Add(new TableColumn("p_c", "Pc, Вт"));
							}
						}
						if (conScheme_ == ConnectScheme.Ph3W3 || conScheme_ == ConnectScheme.Ph3W3_B_calc)
						{
							if (tablePower.Columns.Contains("p_a_1"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("p_a_1", "Pab, кВт"));
								else listColumnsPower.Add(new TableColumn("p_a_1", "Pab, Вт"));
							}
							if (tablePower.Columns.Contains("p_b_2"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("p_b_2", "Pcb, кВт"));
								else listColumnsPower.Add(new TableColumn("p_b_2", "Pcb, Вт"));
							}
						}
					}

					if (devType_ == EmDeviceType.EM32)
					{
						if (tablePower.Columns.Contains("q_sum"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("q_sum", "Qsum, квар"));
							else listColumnsPower.Add(new TableColumn("q_sum", "Qsum, вар"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePower.Columns.Contains("q_a"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a", "Qa, квар"));
								else listColumnsPower.Add(new TableColumn("q_a", "Qa, вар"));
							}
							if (tablePower.Columns.Contains("q_b"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_b", "Qb, квар"));
								else listColumnsPower.Add(new TableColumn("q_b", "Qb, вар"));
							}
							if (tablePower.Columns.Contains("q_c"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_c", "Qc, квар"));
								else listColumnsPower.Add(new TableColumn("q_c", "Qc, вар"));
							}
						}
					}
					else if (devType_ == EmDeviceType.ETPQP || devType_ == EmDeviceType.ETPQP_A)
					{
						if (tablePower.Columns.Contains("q_sum_geom"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("q_sum_geom", "Qsum геом, квар"));
							else listColumnsPower.Add(new TableColumn("q_sum_geom", "Qsum геом, вар"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePower.Columns.Contains("q_a_geom"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_geom", "Qa геом, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_geom", "Qa геом, вар"));
							}
							if (tablePower.Columns.Contains("q_b_geom"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_b_geom", "Qb геом, квар"));
								else listColumnsPower.Add(new TableColumn("q_b_geom", "Qb геом, вар"));
							}
							if (tablePower.Columns.Contains("q_c_geom"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_c_geom", "Qc геом, квар"));
								else listColumnsPower.Add(new TableColumn("q_c_geom", "Qc геом, вар"));
							}
						}

						if (tablePower.Columns.Contains("q_sum_shift"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("q_sum_shift", "Qsum сдвиг, квар"));
							else listColumnsPower.Add(new TableColumn("q_sum_shift", "Qsum сдвиг, вар"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePower.Columns.Contains("q_a_shift"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_shift", "Qa сдвиг, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_shift", "Qa сдвиг, вар"));
							}
							if (tablePower.Columns.Contains("q_b_shift"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_b_shift", "Qb сдвиг, квар"));
								else listColumnsPower.Add(new TableColumn("q_b_shift", "Qb сдвиг, вар"));
							}
							if (tablePower.Columns.Contains("q_c_shift"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_c_shift", "Qc сдвиг, квар"));
								else listColumnsPower.Add(new TableColumn("q_c_shift", "Qc сдвиг, вар"));
							}
						}

						if (tablePower.Columns.Contains("q_sum_cross"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("q_sum_cross", "Qsum перекр, квар"));
							else listColumnsPower.Add(new TableColumn("q_sum_cross", "Qsum перекр, вар"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePower.Columns.Contains("q_a_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_cross", "Qa перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_cross", "Qa перекр, вар"));
							}
							if (tablePower.Columns.Contains("q_b_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_b_cross", "Qb перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_b_cross", "Qb перекр, вар"));
							}
							if (tablePower.Columns.Contains("q_c_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_c_cross", "Qc перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_c_cross", "Qc перекр, вар"));
							}
						}

						if (tablePower.Columns.Contains("q_sum_1harm"))
						{
							if (powerRatio_ < 1)
								listColumnsPower.Add(new TableColumn("q_sum_1harm", "Qsum 1гарм, квар"));
							else listColumnsPower.Add(new TableColumn("q_sum_1harm", "Qsum 1гарм, вар"));
						}
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePower.Columns.Contains("q_a_1harm"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_1harm", "Qa 1гарм, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_1harm", "Qa 1гарм, вар"));
							}
							if (tablePower.Columns.Contains("q_b_1harm"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_b_1harm", "Qb 1гарм, квар"));
								else listColumnsPower.Add(new TableColumn("q_b_1harm", "Qb 1гарм, вар"));
							}
							if (tablePower.Columns.Contains("q_c_1harm"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_c_1harm", "Qc 1гарм, квар"));
								else listColumnsPower.Add(new TableColumn("q_c_1harm", "Qc 1гарм, вар"));
							}
						}
					}
					else //Em33T
					{
						if (conScheme_ != ConnectScheme.Ph1W2)
						{
							if (tablePower.Columns.Contains("q_sum_geom"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_sum_geom", "Qsum геом, квар"));
								else listColumnsPower.Add(new TableColumn("q_sum_geom", "Qsum геом, вар"));
							}
							if (tablePower.Columns.Contains("q_sum_shift"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_sum_shift", "Qsum сдвиг, квар"));
								else listColumnsPower.Add(new TableColumn("q_sum_shift", "Qsum сдвиг, вар"));
							}
							if (tablePower.Columns.Contains("q_sum_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_sum_cross", "Qsum перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_sum_cross", "Qsum перекр, вар"));
							}

							if (conScheme_ == ConnectScheme.Ph3W4 || conScheme_ == ConnectScheme.Ph3W4_B_calc)
							{
								if (tablePower.Columns.Contains("q_a_geom"))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn("q_a_geom", "Qa геом, квар"));
									else listColumnsPower.Add(new TableColumn("q_a_geom", "Qa геом, вар"));
								}
								if (tablePower.Columns.Contains("q_b_geom"))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn("q_b_geom", "Qb геом, квар"));
									else listColumnsPower.Add(new TableColumn("q_b_geom", "Qb геом, вар"));
								}
								if (tablePower.Columns.Contains("q_c_geom"))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn("q_c_geom", "Qc геом, квар"));
									else listColumnsPower.Add(new TableColumn("q_c_geom", "Qc геом, вар"));
								}
								if (tablePower.Columns.Contains("q_a_shift"))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn("q_a_shift", "Qa сдвиг, квар"));
									else listColumnsPower.Add(new TableColumn("q_a_shift", "Qa сдвиг, вар"));
								}
								if (tablePower.Columns.Contains("q_b_shift"))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn("q_b_shift", "Qb сдвиг, квар"));
									else listColumnsPower.Add(new TableColumn("q_b_shift", "Qb сдвиг, вар"));
								}
								if (tablePower.Columns.Contains("q_c_shift"))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn("q_c_shift", "Qc сдвиг, квар"));
									else listColumnsPower.Add(new TableColumn("q_c_shift", "Qc сдвиг, вар"));
								}
							}
							if (tablePower.Columns.Contains("q_a_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_cross", "Qa перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_cross", "Qa перекр, вар"));
							}
							if (tablePower.Columns.Contains("q_b_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_b_cross", "Qb перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_b_cross", "Qb перекр, вар"));
							}
							if (tablePower.Columns.Contains("q_c_cross"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_c_cross", "Qc перекр, квар"));
								else listColumnsPower.Add(new TableColumn("q_c_cross", "Qc перекр, вар"));
							}
						}
						else
						{
							if (tablePower.Columns.Contains("q_a_geom"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_geom", "Qa геом, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_geom", "Qa геом, вар"));
							}
							if (tablePower.Columns.Contains("q_a_shift"))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn("q_a_shift", "Qa сдвиг, квар"));
								else listColumnsPower.Add(new TableColumn("q_a_shift", "Qa сдвиг, вар"));
							}
						}
					}

					if (conScheme_ == ConnectScheme.Ph3W4 || conScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						if (tableHarmPower != null)
						{
							for (int iCol = 2; iCol < 41; ++iCol)
							{
								if (tableHarmPower.Columns.Contains(string.Format("p_a_1_{0}", iCol)))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn(
											string.Format("p_a_1_{0}", iCol),
											string.Format("Pa ({0}), кВт", iCol)));
									else listColumnsPower.Add(new TableColumn(
										string.Format("p_a_1_{0}", iCol),
										string.Format("Pa ({0}), Вт", iCol)));
								}
								if (tableHarmPower.Columns.Contains(string.Format("p_b_2_{0}", iCol)))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn(
											string.Format("p_b_2_{0}", iCol),
											string.Format("Pb ({0}), кВт", iCol)));
									else listColumnsPower.Add(new TableColumn(
										string.Format("p_b_2_{0}", iCol),
										string.Format("Pb ({0}), Вт", iCol)));
								}
								if (tableHarmPower.Columns.Contains(string.Format("p_c_{0}", iCol)))
								{
									if (powerRatio_ < 1)
										listColumnsPower.Add(new TableColumn(
											string.Format("p_c_{0}", iCol),
											string.Format("Pc ({0}), кВт", iCol)));
									else listColumnsPower.Add(new TableColumn(
										string.Format("p_c_{0}", iCol),
										string.Format("Pc ({0}), Вт", iCol)));
								}
							}
						}
					}
					else if (conScheme_ == ConnectScheme.Ph3W3 || conScheme_ == ConnectScheme.Ph3W3_B_calc)
					{
						for (int iCol = 2; iCol < 41; ++iCol)
						{
							if (tableHarmPower.Columns.Contains(string.Format("p_a_1_{0}", iCol)))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn(
										string.Format("p_a_1_{0}", iCol),
										string.Format("Pab ({0}), кВт", iCol)));
								else listColumnsPower.Add(new TableColumn(
									string.Format("p_a_1_{0}", iCol),
									string.Format("Pab ({0}), Вт", iCol)));
							}
							if (tableHarmPower.Columns.Contains(string.Format("p_b_2_{0}", iCol)))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn(
										string.Format("p_b_2_{0}", iCol),
										string.Format("Pcb ({0}), кВт", iCol)));
								else listColumnsPower.Add(new TableColumn(
									string.Format("p_b_2_{0}", iCol),
									string.Format("Pcb ({0}), Вт", iCol)));
							}
						}
					}
					else if (conScheme_ == ConnectScheme.Ph1W2)
					{
						for (int iCol = 2; iCol < 41; ++iCol)
						{
							if (tableHarmPower.Columns.Contains(string.Format("p_a_1_{0}", iCol)))
							{
								if (powerRatio_ < 1)
									listColumnsPower.Add(new TableColumn(
										string.Format("p_a_1_{0}", iCol),
										string.Format("Pa ({0}), кВт", iCol)));
								else listColumnsPower.Add(new TableColumn(
									string.Format("p_a_1_{0}", iCol),
									string.Format("Pa ({0}), Вт", iCol)));
							}
						}
					}

					#endregion

					// вносим в таблицу
					sb.AppendLine("   <Row>");
					for (int iListCol = 0; iListCol < listColumnsPower.Count; ++iListCol)
					{
						string colName = listColumnsPower[iListCol].VisibleName;
						sb.AppendLine(string.Format("   <Cell ss:StyleID=\"EmParamName\"><Data ss:Type=\"String\">{0}</Data></Cell>", colName));
					}
					sb.AppendLine("   </Row>");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildColumnCaptions failed:");
				throw;
			}
		}

		private void BuildDataRows(PAGES iPage, ref StringBuilder sb)
		{
			try
			{
				if (iPage == PAGES.TITUL)			// титульный лист
				{
					BuildOneDataRowString("Название ПС", ref sb);
					BuildOneDataRowString("Шины", ref sb);
					BuildOneDataRowString("Присоединение", ref sb);
					BuildOneDataRowString("Начало измерений", ref sb);
					BuildOneDataRowString("Окончание измерений", ref sb);
					return;
				}
				else if (iPage == PAGES.NORMIR_PQP)	// лист "Нормир ПКЭ"
				{
					List<string> columnNames = new List<string>();
					for (int iCol = 2; iCol < listColumnsNormirPQP.Count; ++iCol)
					{
						columnNames.Add(listColumnsNormirPQP[iCol].PostgresName);
					}
					BuildTableForPage1(ref dataGridsList_, ref columnNames, ref sb);

					//DataGrid dg = dataGridsList_[(int)DATAGRIDS.PQP];
					//BuildTableRow(ref dg, ref columnNames, ref sb);
				}
				else if (iPage == PAGES.DIP_SWELL)	// лист "Провалы"
				{
					List<string> columnNames = new List<string>();
					columnNames.Add("colStart");
					columnNames.Add("colEnd");
					columnNames.Add("colDuration");
					columnNames.Add("colEvent");
					columnNames.Add("colPhase");
					columnNames.Add("colDeviation");

					//columnNames.Add("ColumnStartDateTime");
					//columnNames.Add("ColumnEndDateTime");
					//columnNames.Add("ColumnPeriodTime");
					//columnNames.Add("ColumnEvent");
					//columnNames.Add("ColumnPhase");
					//columnNames.Add("ColumnDeviation");

					//columnNames.Add("Start");
					//columnNames.Add("End");
					//columnNames.Add("Period");
					//columnNames.Add("Event");
					//columnNames.Add("Phase");
					//columnNames.Add("Deviation");

					if (dgvDNS_ != null)
						BuildTableRow(ref dgvDNS_, ref columnNames, ref sb);
				}
				else if (iPage == PAGES.FLIK_PST)	// лист "Колебания PSt"
				{
					List<string> columnNames = new List<string>();
					columnNames.Add("flik_time");
					columnNames.Add("flik_a");
					if (conScheme_ != ConnectScheme.Ph1W2)
					{
						columnNames.Add("flik_b");
						columnNames.Add("flik_c");
					}

					DataGrid dg = dataGridsList_[(int)DATAGRIDS.FLIK_ST];
					BuildTableRow(ref dg, ref columnNames, ref sb);
				}
				else if (iPage == PAGES.FLIK_PLT)	// лист "Колебания PLt"
				{
					List<string> columnNames = new List<string>();
					columnNames.Add("flik_time");
					columnNames.Add("flik_a_long");
					if (conScheme_ != ConnectScheme.Ph1W2)
					{
						columnNames.Add("flik_b_long");
						columnNames.Add("flik_c_long");
					}

					DataGrid dg = dataGridsList_[(int)DATAGRIDS.FLIK_LT];
					BuildTableRow(ref dg, ref columnNames, ref sb);
				}
				else if (iPage == PAGES.OTHER)	// лист "Прочие параметры"
				{
					List<string> columnNames = new List<string>();
					for (int iCol = 2; iCol < listColumnsOther.Count; ++iCol)
					{
						columnNames.Add(listColumnsOther[iCol].PostgresName);
					}
					BuildTableForPage5(ref dataGridsList_, ref columnNames, ref sb);

					//DataGrid dg = dataGridsList_[(int)DATAGRIDS.PQP];
					//BuildTableRow(ref dg, ref columnNames, ref sb);
				}

				else if (iPage == PAGES.POWER)		// лист "Мощность"
				{
					List<string> columnNames = new List<string>();
					for (int iCol = 2; iCol < listColumnsPower.Count; ++iCol)
					{
						columnNames.Add(listColumnsPower[iCol].PostgresName);
					}
					BuildTableForPage6(ref dataGridsList_, ref columnNames, ref sb);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildDataRows failed:");
				throw;
			}
		}

		// Страница НОРМИР ПКЭ
		private void BuildTableForPage1(ref DataGridList dgList, ref List<string> columnNames, 
										ref StringBuilder sb)
		{
			try
			{
				DataGrid dgPQP = dataGridsList_[(int)DATAGRIDS.PQP];
				DataGrid dgLH = dataGridsList_[(int)DATAGRIDS.VOLTAGE_HARM_LIN];
				DataGrid dgPH = dataGridsList_[(int)DATAGRIDS.VOLTAGE_HARM_PH];

				if (dgPQP != null && dgPQP.DataSource != null)
				{
					DataTable tablePQP = ((DataSet)dgPQP.DataSource).Tables[0];
					DataGridTableStyle tableStylePQP = dgPQP.TableStyles[0];

					DataTable tableLH = null;
					DataGridTableStyle tableStyleLH = null;
					DataTable tablePH = null;
					DataGridTableStyle tableStylePH = null;
					if (dgLH != null && dgLH.DataSource != null)
					{
						if (((DataSet)dgLH.DataSource).Tables.Count > 0)
						{
							tableLH = ((DataSet)dgLH.DataSource).Tables[0];
							tableStyleLH = dgLH.TableStyles[0];
							if (tableLH.Rows.Count != tablePQP.Rows.Count)
								tableLH = null;
						}
					}
					if (dgPH != null && dgPH.DataSource != null)
					{
						if (((DataSet)dgPH.DataSource).Tables.Count > 0)
						{
							tablePH = ((DataSet)dgPH.DataSource).Tables[0];
							tableStylePH = dgPH.TableStyles[0];
							if (tablePH.Rows.Count != tablePQP.Rows.Count)
								tablePH = null;
						}
					}

					for(int iRow = 0; iRow < tablePQP.Rows.Count; ++iRow)
					//foreach (DataRow row in tablePQP.Rows)
					{
						sb.AppendLine("   <Row>");

						DataRow rowPQP = tablePQP.Rows[iRow];
						DataRow rowPH = null;
						DataRow rowLH = null;
						if (tablePH != null && tablePH.Rows.Count > iRow)
							rowPH = tablePH.Rows[iRow];
						if (tableLH != null && tableLH.Rows.Count > iRow)
							rowLH = tableLH.Rows[iRow];

						for (int iCol = 0; iCol < listColumnsNormirPQP.Count; ++iCol)
						{
							if (tablePQP.Columns.Contains(listColumnsNormirPQP[iCol].PostgresName))
							{
								BuildOneDataRowParam(rowPQP, listColumnsNormirPQP[iCol].PostgresName, ref sb);
								continue;
							}
							if (rowPH != null)
							{
								if (tablePH.Columns.Contains(listColumnsNormirPQP[iCol].PostgresName) &&
									listColumnsNormirPQP[iCol].PostgresName.Contains("k_"))
								{
									BuildOneDataRowParam(rowPH, listColumnsNormirPQP[iCol].PostgresName, ref sb);
									continue;
								}
							}
							if (rowLH != null)
							{
								if (tableLH.Columns.Contains(listColumnsNormirPQP[iCol].PostgresName) &&
									listColumnsNormirPQP[iCol].PostgresName.Contains("k_"))
								{
									BuildOneDataRowParam(rowLH, listColumnsNormirPQP[iCol].PostgresName, ref sb);
									continue;
								}
							}
						}

						sb.AppendLine("   </Row>");

						CheckCache(ref sb);

						#region old code

						/*for (int iStyle = 0; iStyle < tableStylePQP.GridColumnStyles.Count; iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStylePQP.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowPQP[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowPQP[dgStyle.MappingName])).ToString();
							}
							else if (rowPQP[dgStyle.MappingName] is Single)
							{
								float num = (float)rowPQP[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPQP[dgStyle.MappingName] is Double)
							{
								double num = (double)rowPQP[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPQP[dgStyle.MappingName] is String)
							{
								strValue = rowPQP[dgStyle.MappingName].ToString();
							}
							else if (rowPQP[dgStyle.MappingName] is DateTime)
							{
								strValue = 
									((DateTime)rowPQP[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowPQP[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowPQP[dgStyle.MappingName] is String ||
									rowPQP[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}

						for (int iStyle = 0; 
							tableStylePH != null && iStyle < tableStylePH.GridColumnStyles.Count &&
							rowPH != null; 
							iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStylePH.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName) ||
								!dgStyle.MappingName.Contains("k_"))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowPH[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowPH[dgStyle.MappingName])).ToString();
							}
							else if (rowPH[dgStyle.MappingName] is Single)
							{
								float num = (float)rowPH[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPH[dgStyle.MappingName] is Double)
							{
								double num = (double)rowPH[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPH[dgStyle.MappingName] is String)
							{
								strValue = rowPH[dgStyle.MappingName].ToString();
							}
							else if (rowPH[dgStyle.MappingName] is DateTime)
							{
								strValue = 
									((DateTime)rowPH[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowPH[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowPH[dgStyle.MappingName] is String ||
									rowPH[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}

						for (int iStyle = 0;
							tableStyleLH != null && iStyle < tableStyleLH.GridColumnStyles.Count &&
							rowLH != null; 
							iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStyleLH.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName) ||
								!dgStyle.MappingName.Contains("k_"))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowLH[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowLH[dgStyle.MappingName])).ToString();
							}
							else if (rowLH[dgStyle.MappingName] is Single)
							{
								float num = (float)rowLH[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowLH[dgStyle.MappingName] is Double)
							{
								double num = (double)rowLH[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowLH[dgStyle.MappingName] is String)
							{
								strValue = rowLH[dgStyle.MappingName].ToString();
							}
							else if (rowLH[dgStyle.MappingName] is DateTime)
							{
								strValue = 
									((DateTime)rowLH[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowLH[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowLH[dgStyle.MappingName] is String ||
									rowLH[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}

						sb.AppendLine("   </Row>");

						CheckCache(ref sb);*/
						#endregion
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTableForPage1 failed:");
				throw;
			}
		}

		// Страница Прочие параметры
		private void BuildTableForPage5(ref DataGridList dgList, ref List<string> columnNames,
										ref StringBuilder sb)
		{
			try
			{
				DataGrid dgI = dataGridsList_[(int)DATAGRIDS.CURRENT];
				DataGrid dgPQP = dataGridsList_[(int)DATAGRIDS.PQP];
				DataGrid dgIHarm = dataGridsList_[(int)DATAGRIDS.CURRENT_HARM];

				if (dgI != null && dgI.DataSource != null)
				{
					DataTable tableI = ((DataSet)dgI.DataSource).Tables[0];
					DataGridTableStyle tableStyleI = dgI.TableStyles[0];

					DataTable tablePQP = null;
					DataGridTableStyle tableStylePQP = null;
					DataTable tableIHarm = null;
					DataGridTableStyle tableStyleIHarm = null;
					if (dgPQP != null && dgPQP.DataSource != null)
					{
						if (((DataSet)dgPQP.DataSource).Tables.Count > 0)
						{
							tablePQP = ((DataSet)dgPQP.DataSource).Tables[0];
							tableStylePQP = dgPQP.TableStyles[0];
							if (tablePQP.Rows.Count != tableI.Rows.Count)
								tablePQP = null;
						}
					}
					if (dgIHarm != null && dgIHarm.DataSource != null)
					{
						if (((DataSet)dgIHarm.DataSource).Tables.Count > 0)
						{
							tableIHarm = ((DataSet)dgIHarm.DataSource).Tables[0];
							tableStyleIHarm = dgIHarm.TableStyles[0];
							if (tableIHarm.Rows.Count != tableI.Rows.Count)
								tableIHarm = null;
						}
					}

					for (int iRow = 0; iRow < tableI.Rows.Count; ++iRow)
					{
						sb.AppendLine("   <Row>");

						DataRow rowI = tableI.Rows[iRow];
						DataRow rowIHarm = null;
						DataRow rowPQP = null;
						if (tableIHarm != null && tableIHarm.Rows.Count > iRow)
							rowIHarm = tableIHarm.Rows[iRow];
						if (tablePQP != null && tablePQP.Rows.Count > iRow)
							rowPQP = tablePQP.Rows[iRow];

						for (int iCol = 0; iCol < listColumnsOther.Count; ++iCol)
						{
							if (tableI.Columns.Contains(listColumnsOther[iCol].PostgresName))
							{
								BuildOneDataRowParam(rowI, listColumnsOther[iCol].PostgresName, ref sb);
								continue;
							}
							if (rowIHarm != null)
							{
								if (tableIHarm.Columns.Contains(listColumnsOther[iCol].PostgresName) &&
									listColumnsOther[iCol].PostgresName.Contains("k_"))
								{
									BuildOneDataRowParam(rowIHarm, listColumnsOther[iCol].PostgresName, ref sb);
									continue;
								}
							}
							if (rowPQP != null)
							{
								if (tablePQP.Columns.Contains(listColumnsOther[iCol].PostgresName) &&
									listColumnsOther[iCol].PostgresName.Contains("k_"))
								{
									BuildOneDataRowParam(rowPQP, listColumnsOther[iCol].PostgresName, ref sb);
									continue;
								}
							}
						}

						#region old ccode

						/*for (int iStyle = 0; iStyle < tableStyleI.GridColumnStyles.Count; iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStyleI.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowI[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowI[dgStyle.MappingName])).ToString();
							}
							else if (rowI[dgStyle.MappingName] is Single)
							{
								float num = (float)rowI[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowI[dgStyle.MappingName] is Double)
							{
								double num = (double)rowI[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowI[dgStyle.MappingName] is String)
							{
								strValue = rowI[dgStyle.MappingName].ToString();
							}
							else if (rowI[dgStyle.MappingName] is DateTime)
							{
								strValue =
									((DateTime)rowI[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowI[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowI[dgStyle.MappingName] is String ||
									rowI[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}

						for (int iStyle = 0;
							tableStylePQP != null && iStyle < tableStylePQP.GridColumnStyles.Count &&
							rowPQP != null;
							iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStylePQP.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName) ||
								!dgStyle.MappingName.Contains("k_"))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowPQP[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowPQP[dgStyle.MappingName])).ToString();
							}
							else if (rowPQP[dgStyle.MappingName] is Single)
							{
								float num = (float)rowPQP[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPQP[dgStyle.MappingName] is Double)
							{
								double num = (double)rowPQP[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPQP[dgStyle.MappingName] is String)
							{
								strValue = rowPQP[dgStyle.MappingName].ToString();
							}
							else if (rowPQP[dgStyle.MappingName] is DateTime)
							{
								strValue =
									((DateTime)rowPQP[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowPQP[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowPQP[dgStyle.MappingName] is String ||
									rowPQP[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}

						for (int iStyle = 0;
							tableStyleIHarm != null && iStyle < tableStyleIHarm.GridColumnStyles.Count &&
							rowIHarm != null;
							iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStyleIHarm.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName) ||
								!dgStyle.MappingName.Contains("k_"))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowIHarm[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowIHarm[dgStyle.MappingName])).ToString();
							}
							else if (rowIHarm[dgStyle.MappingName] is Single)
							{
								float num = (float)rowIHarm[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowIHarm[dgStyle.MappingName] is Double)
							{
								double num = (double)rowIHarm[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowIHarm[dgStyle.MappingName] is String)
							{
								strValue = rowIHarm[dgStyle.MappingName].ToString();
							}
							else if (rowIHarm[dgStyle.MappingName] is DateTime)
							{
								strValue =
									((DateTime)rowIHarm[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowIHarm[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowIHarm[dgStyle.MappingName] is String ||
									rowIHarm[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}*/
						#endregion

						sb.AppendLine("   </Row>");

						CheckCache(ref sb);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTableForPage5 failed:");
				throw;
			}
		}

		// страница Мощности
		private void BuildTableForPage6(ref DataGridList dgList, ref List<string> columnNames,
										ref StringBuilder sb)
		{
			try
			{
				DataGrid dgPower = dataGridsList_[(int)DATAGRIDS.POWER];
				DataGrid dgHarmPower = dataGridsList_[(int)DATAGRIDS.HARM_POWER];

				if (dgPower != null && dgPower.DataSource != null)
				{
					DataTable tablePower = ((DataSet)dgPower.DataSource).Tables[0];
					DataGridTableStyle tableStylePower = dgPower.TableStyles[0];
					DataTable tableHarmPower = null;
					DataGridTableStyle tableStyleHarmPower = null;
					if (dgHarmPower != null && dgHarmPower.DataSource != null)
					{
						if (((DataSet)dgHarmPower.DataSource).Tables.Count > 0)
						{
							tableHarmPower = ((DataSet)dgHarmPower.DataSource).Tables[0];
							tableStyleHarmPower = dgHarmPower.TableStyles[0];
							if (tableHarmPower.Rows.Count != tablePower.Rows.Count)
								tableHarmPower = null;
						}
					}

					for (int iRow = 0; iRow < tablePower.Rows.Count; ++iRow)
					{
						sb.AppendLine("   <Row>");

						DataRow rowPower = tablePower.Rows[iRow];
						DataRow rowHarmPower = null;
						if (tableHarmPower != null && tableHarmPower.Rows.Count > iRow)
							rowHarmPower = tableHarmPower.Rows[iRow];

						for (int iCol = 0; iCol < listColumnsPower.Count; ++iCol)
						{
							if (tablePower.Columns.Contains(listColumnsPower[iCol].PostgresName)) /*&&
								(listColumnsPower[iCol].PostgresName[0] == 'p' ||
								listColumnsPower[iCol].PostgresName[0] == 'q' ||
								listColumnsPower[iCol].PostgresName.Equals("event_datetime")))*/
							{
								BuildOneDataRowParam(rowPower, listColumnsPower[iCol].PostgresName, ref sb);
								continue;
							}
							if (rowHarmPower != null)
							{
								if (tableHarmPower.Columns.Contains(listColumnsPower[iCol].PostgresName) &&
									!listColumnsPower[iCol].PostgresName.Equals("p_sum") &&
									!listColumnsPower[iCol].PostgresName.Equals("p_a_1") &&
									!listColumnsPower[iCol].PostgresName.Equals("p_b_2") &&
									!listColumnsPower[iCol].PostgresName.Equals("p_c") &&
									!listColumnsPower[iCol].PostgresName.Equals("event_datetime") &&
									!listColumnsPower[iCol].PostgresName.Equals("datetime_id"))
								{
									BuildOneDataRowParam(rowHarmPower,
										listColumnsPower[iCol].PostgresName, ref sb);
									continue;
								}
							}
						}

						#region old code

						/*for (int iStyle = 0; iStyle < tableStylePower.GridColumnStyles.Count; iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStylePower.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowPower[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowPower[dgStyle.MappingName])).ToString();
							}
							else if (rowPower[dgStyle.MappingName] is Single)
							{
								float num = (float)rowPower[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPower[dgStyle.MappingName] is Double)
							{
								double num = (double)rowPower[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowPower[dgStyle.MappingName] is String)
							{
								strValue = rowPower[dgStyle.MappingName].ToString();
							}
							else if (rowPower[dgStyle.MappingName] is DateTime)
							{
								strValue =
									((DateTime)rowPower[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowPower[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowPower[dgStyle.MappingName] is String ||
									rowPower[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}

						for (int iStyle = 0;
							tableStyleHarmPower != null && iStyle < tableStyleHarmPower.GridColumnStyles.Count &&
							rowHarmPower != null;
							iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStyleHarmPower.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName) ||
								dgStyle.MappingName.Equals("p_sum") ||
								dgStyle.MappingName.Equals("p_a_1") ||
								dgStyle.MappingName.Equals("p_b_2") ||
								dgStyle.MappingName.Equals("p_c") ||
								dgStyle.MappingName.Equals("event_datetime") ||
								dgStyle.MappingName.Equals("datetime_id"))
								continue;

							string strStyleID = "EmNumber";

							String strValue = String.Empty;
							if (rowHarmPower[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(rowHarmPower[dgStyle.MappingName])).ToString();
							}
							else if (rowHarmPower[dgStyle.MappingName] is Single)
							{
								float num = (float)rowHarmPower[dgStyle.MappingName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowHarmPower[dgStyle.MappingName] is Double)
							{
								double num = (double)rowHarmPower[dgStyle.MappingName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (rowHarmPower[dgStyle.MappingName] is String)
							{
								strValue = rowHarmPower[dgStyle.MappingName].ToString();
							}
							else if (rowHarmPower[dgStyle.MappingName] is DateTime)
							{
								strValue =
									((DateTime)rowHarmPower[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (rowHarmPower[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (rowHarmPower[dgStyle.MappingName] is String ||
									rowHarmPower[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}*/

						#endregion

						sb.AppendLine("   </Row>");

						CheckCache(ref sb);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTableForPage6 failed:");
				throw;
			}
		}

		#region old code1
		// Страница НОРМИР ПКЭ
		//private void BuildTableForPage5(ref DataGridList dgList, ref List<string> columnNames,
		//                                ref StringBuilder sb)
		//{
		//    try
		//    {
		//        DataGrid dgI = dataGridsList_[(int)DATAGRIDS.CURRENT];
		//        DataGrid dgPQP = dataGridsList_[(int)DATAGRIDS.CURRENT];
		//        DataGrid dgIHarm = dataGridsList_[(int)DATAGRIDS.CURRENT_HARM];

		//        if (dgI != null && dgI.DataSource != null)
		//        {
		//            DataTable tableI = ((DataSet)dgI.DataSource).Tables[0];
		//            DataGridTableStyle tableStyleI = dgI.TableStyles[0];

		//            DataTable tableIHarm = null;
		//            DataGridTableStyle tableStyleIHarm = null;
		//            if (dgIHarm != null && dgIHarm.DataSource != null)
		//            {
		//                if (((DataSet)dgIHarm.DataSource).Tables.Count > 0)
		//                {
		//                    tableIHarm = ((DataSet)dgIHarm.DataSource).Tables[0];
		//                    tableStyleIHarm = dgIHarm.TableStyles[0];
		//                    if (tableIHarm.Rows.Count != tableI.Rows.Count)
		//                        tableIHarm = null;
		//                }
		//            }

		//            for (int iRow = 0; iRow < tableI.Rows.Count; ++iRow)
		//            //foreach (DataRow row in tableI.Rows)
		//            {
		//                sb.AppendLine("   <Row>");

		//                DataRow rowI = tableI.Rows[iRow];
		//                DataRow rowIHarm = null;
		//                if (tableIHarm != null && tableIHarm.Rows.Count > iRow)
		//                    rowIHarm = tableIHarm.Rows[iRow];

		//                for (int iStyle = 0; iStyle < tableStyleI.GridColumnStyles.Count; iStyle++)
		//                {
		//                    String strCell = String.Empty;

		//                    DataGridColumnStyle dgStyle = tableStyleI.GridColumnStyles[iStyle];
		//                    if (dgStyle.Width == 0) continue;

		//                    if (!columnNames.Contains(dgStyle.MappingName))
		//                        continue;

		//                    string strStyleID = "EmNumber";

		//                    String strValue = String.Empty;
		//                    if (rowI[dgStyle.MappingName] is Int32)
		//                    {
		//                        strValue = ((int)(rowI[dgStyle.MappingName])).ToString();
		//                    }
		//                    else if (rowI[dgStyle.MappingName] is Single)
		//                    {
		//                        float num = (float)rowI[dgStyle.MappingName];
		//                        num = (float)Math.Round(num, floatSigns_);
		//                        strValue = num.ToString(new CultureInfo("en-US"));
		//                    }
		//                    else if (rowI[dgStyle.MappingName] is Double)
		//                    {
		//                        double num = (double)rowI[dgStyle.MappingName];
		//                        num = Math.Round(num, floatSigns_);
		//                        strValue = num.ToString(new CultureInfo("en-US"));
		//                    }
		//                    else if (rowI[dgStyle.MappingName] is String)
		//                    {
		//                        strValue = rowI[dgStyle.MappingName].ToString();
		//                    }
		//                    else if (rowI[dgStyle.MappingName] is DateTime)
		//                    {
		//                        strValue =
		//                            ((DateTime)rowI[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
		//                    }
		//                    else if (rowI[dgStyle.MappingName] is DBNull)
		//                    {
		//                        strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
		//                    }

		//                    if (strCell.Equals(String.Empty))
		//                    {
		//                        if (rowI[dgStyle.MappingName] is String ||
		//                            rowI[dgStyle.MappingName] is DateTime)
		//                            strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
		//                        else
		//                            strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
		//                    }
		//                    sb.AppendLine(strCell);
		//                }

		//                for (int iStyle = 0;
		//                    tableStyleIHarm != null && iStyle < tableStyleIHarm.GridColumnStyles.Count &&
		//                    rowIHarm != null;
		//                    iStyle++)
		//                {
		//                    String strCell = String.Empty;

		//                    DataGridColumnStyle dgStyle = tableStyleIHarm.GridColumnStyles[iStyle];
		//                    if (dgStyle.Width == 0) continue;

		//                    if (!columnNames.Contains(dgStyle.MappingName) ||
		//                        !dgStyle.MappingName.Contains("k_"))
		//                        continue;

		//                    string strStyleID = "EmNumber";

		//                    String strValue = String.Empty;
		//                    if (rowIHarm[dgStyle.MappingName] is Int32)
		//                    {
		//                        strValue = ((int)(rowIHarm[dgStyle.MappingName])).ToString();
		//                    }
		//                    else if (rowIHarm[dgStyle.MappingName] is Single)
		//                    {
		//                        float num = (float)rowIHarm[dgStyle.MappingName];
		//                        num = (float)Math.Round(num, floatSigns_);
		//                        strValue = num.ToString(new CultureInfo("en-US"));
		//                    }
		//                    else if (rowIHarm[dgStyle.MappingName] is Double)
		//                    {
		//                        double num = (double)rowIHarm[dgStyle.MappingName];
		//                        num = Math.Round(num, floatSigns_);
		//                        strValue = num.ToString(new CultureInfo("en-US"));
		//                    }
		//                    else if (rowIHarm[dgStyle.MappingName] is String)
		//                    {
		//                        strValue = rowIHarm[dgStyle.MappingName].ToString();
		//                    }
		//                    else if (rowIHarm[dgStyle.MappingName] is DateTime)
		//                    {
		//                        strValue =
		//                            ((DateTime)rowIHarm[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
		//                    }
		//                    else if (rowIHarm[dgStyle.MappingName] is DBNull)
		//                    {
		//                        strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
		//                    }

		//                    if (strCell.Equals(String.Empty))
		//                    {
		//                        if (rowIHarm[dgStyle.MappingName] is String ||
		//                            rowIHarm[dgStyle.MappingName] is DateTime)
		//                            strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
		//                        else
		//                            strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
		//                    }
		//                    sb.AppendLine(strCell);
		//                }

		//                sb.AppendLine("   </Row>");

		//                CheckCache(ref sb);
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "BuildTableForPage5 failed:");
		//        throw;
		//    }
		//}
		#endregion

		private void BuildTableRow(ref DataGrid dg, ref List<string> columnNames, ref StringBuilder sb)
		{
			try
			{
				if (dg != null && dg.DataSource != null)
				{
					DataTable table = ((DataSet)dg.DataSource).Tables[0];
					DataGridTableStyle tableStyle = dg.TableStyles[0];
					foreach (DataRow row in table.Rows)
					{
						sb.AppendLine("   <Row>");
						for (int iStyle = 0; iStyle < tableStyle.GridColumnStyles.Count; iStyle++)
						{
							String strCell = String.Empty;

							DataGridColumnStyle dgStyle = tableStyle.GridColumnStyles[iStyle];
							if (dgStyle.Width == 0) continue;

							if (!columnNames.Contains(dgStyle.MappingName))
								continue;

							string strStyleID = /*string.Empty;
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
							else strStyleID =*/ "EmNumber";

							String strValue = String.Empty;
							if (row[dgStyle.MappingName] is Int32)
							{
								strValue = ((int)(row[dgStyle.MappingName])).ToString();
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
							else if (row[dgStyle.MappingName] is String)
							{
								strValue = row[dgStyle.MappingName].ToString();
							}
							else if (row[dgStyle.MappingName] is DateTime)
							{
								strValue = 
									((DateTime)row[dgStyle.MappingName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (row[dgStyle.MappingName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (row[dgStyle.MappingName] is String ||
									row[dgStyle.MappingName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}
						sb.AppendLine("   </Row>");

						CheckCache(ref sb);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTableRow failed:");
				throw;
			}
		}

		private void BuildTableRow(ref DataGridView dg, ref List<string> columnNames, ref StringBuilder sb)
		{
			try
			{
				if (dg != null && dg.DataSource != null)
				{
					DataTable table = ((DataSet)dg.DataSource).Tables[0];
					//DataGridTableStyle tableStyle = dg.TableStyles[0];
					foreach (DataRow row in table.Rows)
					{
						sb.AppendLine("   <Row>");
						for (int iCol = 0; iCol < table.Columns.Count; iCol++)
						{
							String strCell = String.Empty;
							DataColumn curColumn = table.Columns[iCol];

							//DataGridColumnStyle dgStyle = tableStyle.GridColumnStyles[iStyle];
							//if (curColumn..Width == 0) continue;

							if (!columnNames.Contains(curColumn.ColumnName))
								continue;

							string strStyleID = /*string.Empty;
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
							else strStyleID =*/"EmNumber";

							String strValue = String.Empty;
							if (row[curColumn.ColumnName] is Int32)
							{
								strValue = ((int)(row[curColumn.ColumnName])).ToString();
							}
							else if (row[curColumn.ColumnName] is Single)
							{
								float num = (float)row[curColumn.ColumnName];
								num = (float)Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (row[curColumn.ColumnName] is Double)
							{
								double num = (double)row[curColumn.ColumnName];
								num = Math.Round(num, floatSigns_);
								strValue = num.ToString(new CultureInfo("en-US"));
							}
							else if (row[curColumn.ColumnName] is String)
							{
								strValue = row[curColumn.ColumnName].ToString();
							}
							else if (row[curColumn.ColumnName] is DateTime)
							{
								strValue =
									((DateTime)row[curColumn.ColumnName]).ToString("dd.MM.yyyy HH:mm:ss");
							}
							else if (row[curColumn.ColumnName] is DBNull)
							{
								strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
							}

							if (strCell.Equals(String.Empty))
							{
								if (row[curColumn.ColumnName] is String ||
									row[curColumn.ColumnName] is DateTime)
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
								else
									strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
							}
							sb.AppendLine(strCell);
						}
						sb.AppendLine("   </Row>");

						CheckCache(ref sb);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildTableRow failed:");
				throw;
			}
		}

		private void BuildOneDataRowParam(DataRow row, string colName, ref StringBuilder sb)
		{
			try
			{
				string strStyleID = "EmNumber";
				String strCell = String.Empty;

				String strValue = String.Empty;
				if (row[colName] is Int32)
				{
					strValue = ((int)(row[colName])).ToString();
				}
				else if (row[colName] is Single)
				{
					float num = (float)row[colName];
					num = (float)Math.Round(num, floatSigns_);
					strValue = num.ToString(new CultureInfo("en-US"));
				}
				else if (row[colName] is Double)
				{
					double num = (double)row[colName];
					num = Math.Round(num, floatSigns_);
					strValue = num.ToString(new CultureInfo("en-US"));
				}
				else if (row[colName] is String)
				{
					strValue = row[colName].ToString();
				}
				else if (row[colName] is DateTime)
				{
					strValue =
						((DateTime)row[colName]).ToString(
						"dd.MM.yyyy HH:mm:ss");
				}
				else if (row[colName] is DBNull)
				{
					strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\"> - </Data></Cell>", strStyleID);
				}

				if (strCell.Equals(String.Empty))
				{
					if (row[colName] is String || row[colName] is DateTime)
						strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>", strStyleID, strValue);
					else
						strCell = String.Format("    <Cell ss:StyleID=\"{0}\"><Data ss:Type=\"Number\">{1}</Data></Cell>", strStyleID, strValue);
				}
				sb.AppendLine(strCell);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildOneDataRowParam failed:");
				throw;
			}
		}

		private void BuildOneDataRowString(string text, ref StringBuilder sb)
		{
			try
			{
				sb.AppendLine("   <Row>");
				//sb.AppendLine(String.Format("    <Cell ss:StyleID=\"String\"><ss:Data xmlns=\"http://www.w3.org/TR/REC-html40\" ss:Type=\"String\">{0}</ss:Data></Cell>", text));
				sb.AppendLine(String.Format("     <Cell ss:Index=\"2\"><Data ss:Type=\"String\">{0}</Data></Cell>", text));
				sb.AppendLine("   </Row>");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "BuildOneDataRowString failed:");
				throw;
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

		private void CheckCache(ref StringBuilder sb)
		{
			if (sb.Length > maxCacheSize_)
			{
				sw_.Write(sb.ToString());
				sb.Remove(0, sb.Length);
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
			fileName_ = FileName;
			File.Delete(fileName_);
			sw_ = File.CreateText(fileName_);
		}

		public void Close()
		{
			if (sw_ != null) sw_.Close();
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

	class TableColumn
	{
		private string postgresName_;
		private string visibleName_;
		private int width_ = 80;

		public int Width
		{
			get { return width_; }
			set { width_ = value; }
		}

		public string PostgresName
		{
			get { return postgresName_; }
			set { postgresName_ = value; }
		}

		public string VisibleName
		{
			get { return visibleName_; }
			set { visibleName_ = value; }
		}

		public TableColumn(string pn, string vn)
		{
			postgresName_ = pn;
			visibleName_ = vn;
		}

		public TableColumn(string pn, string vn, int w)
		{
			postgresName_ = pn;
			visibleName_ = vn;
			width_ = w;
		}
	}

	enum DATAGRIDS
	{
		PQP = 0,
		VOLTAGE_HARM_PH = 1,
		VOLTAGE_HARM_LIN = 2,
		//DIP = 3,
		//SWELL = 4,
		FLIK_ST = 3,
		FLIK_LT = 4,
		//IMPLUSE = 5,
		CURRENT = 5,
		CURRENT_HARM = 6,
		POWER = 7,
		HARM_POWER = 8
	}

	enum PAGES
	{
		TITUL = 0,
		NORMIR_PQP = 1,
		DIP_SWELL = 2,
		FLIK_PST = 3,
		FLIK_PLT = 4,
		OTHER = 5,
		POWER = 6
	}
}
