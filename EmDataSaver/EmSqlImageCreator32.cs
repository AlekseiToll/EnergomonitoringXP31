using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.ComponentModel;

using EmServiceLib;
using EmServiceLib.SavingInterface;
using DeviceIO;
using DbServiceLib;
using EmDataSaver.XmlImage;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.SqlImage;

namespace EmDataSaver
{
	public class EmSqlImageCreator32 : EmSqlImageCreatorBase
	{
		#region Fields

        private bool bAutoMode_;

		private const int avgArchiveLength_ = 4096;

		private BackgroundWorker bw_ = null;

		private Em32XmlDeviceImage xmlImage_;
		private EmSqlEm32Device sqlImage_;
		//private string sqlImageFileName_;

		#endregion

		#region Properties

		public EmSqlEm32Device SqlImage
		{
			get { return sqlImage_; }
		}

		/// <summary>Gets image file extention</summary>
        public static string ImageFileExtention
        {
            get { return "em32.xml"; }
        }
		
		/// <summary>Gets image files filter string for open/save dialogs</summary>
		public static string ImageFilter
		{
			get
			{
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					return String.Format(
						"Файлы Образа EM-32 (*.{0})|*.{0}", ImageFileExtention);
				else
					return String.Format(
						"EM-32 Image files (*.{0})|*.{0}", ImageFileExtention);
			}
		}

		#endregion

		#region Constructor

		public EmSqlImageCreator32(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			ref Em32XmlDeviceImage xmlImage,
			bool autoMode)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.bw_ = bw;
			this.xmlImage_ = xmlImage;
			this.bAutoMode_ = autoMode;
		}

		#endregion

		#region Main methods

		public override bool CreateSqlImage()
		{
			try
			{
				if (xmlImage_ == null)
					throw new EmException("EmDataSaver32::CreateSqlImage: xmlImage == null");

				#region progressbar

				// считаем число шагов (для progressbar)
				cnt_steps_progress_ = 0;

				// pqp
				if (xmlImage_.ArchivePQP != null)
					cnt_steps_progress_ += (uint)xmlImage_.ArchivePQP.Length;
				//dns
				if (xmlImage_.ArchiveDNS != null)
					cnt_steps_progress_ += (uint)xmlImage_.ArchiveDNS.Length;
				//avg
				if (xmlImage_.ArchiveAVG != null)
				{
					for (int i = 0; i < xmlImage_.ArchiveAVG.Length; ++i)
					{
						cnt_steps_progress_ += (uint)xmlImage_.ArchiveAVG[i].DataPages.Length 
							/ avgArchiveLength_;
					}
				}

				// делаем ProgressBar с запасом, иначе на последних шагах он 
				// долго висит заполненный
				cnt_steps_progress_ += 2;

				#endregion

				// формируем SQL/XML образ
				sqlImage_ = new EmSqlEm32Device();
				return xml2sql();
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in CreateSqlImage()");
				Thread.ResetAbort();
				return false;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in CreateSqlImage(): " + emx.Message);
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CreateSqlImage():");
				throw;
			}
		}
		
		#endregion

		#region Private SQL Image methods

		private bool xml2sql()
		{
			try
			{
				EmService.WriteToLogGeneral("xmi2sql(): " + DateTime.Now.ToString());

				sqlImage_.SerialNumber = xmlImage_.SerialNumber;
				sqlImage_.ObjectName = xmlImage_.ObjectName;
				sqlImage_.MlStartTime1 = xmlImage_.MlStartTime1;
				sqlImage_.MlEndTime1 = xmlImage_.MlEndTime1;
				sqlImage_.MlStartTime2 = xmlImage_.MlStartTime2;
				sqlImage_.MlEndTime2 = xmlImage_.MlEndTime2;
				sqlImage_.ConnectionScheme = xmlImage_.ConnectionScheme;
				sqlImage_.CurrentTransducerIndex = xmlImage_.CurrentTransducerIndex;
				sqlImage_.F_Nominal = xmlImage_.F_Nominal;
				sqlImage_.U_NominalLinear = xmlImage_.U_NominalLinear;
				sqlImage_.U_NominalPhase = xmlImage_.U_NominalPhase;
				sqlImage_.I_NominalPhase = xmlImage_.I_NominalPhase;
				sqlImage_.U_Limit = xmlImage_.U_Limit;
				sqlImage_.I_Limit = xmlImage_.I_Limit;
				sqlImage_.ConstraintType = xmlImage_.ConstraintType;
				sqlImage_.DevVersion = xmlImage_.DevVersion;
				sqlImage_.T_fliker = xmlImage_.T_fliker;

				DateTime mlStart1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
					sqlImage_.MlStartTime1.Hours, sqlImage_.MlStartTime1.Minutes,
					sqlImage_.MlStartTime1.Seconds);
				DateTime mlEnd1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
					sqlImage_.MlEndTime1.Hours, sqlImage_.MlEndTime1.Minutes, sqlImage_.MlEndTime1.Seconds);
				DateTime mlStart2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
					sqlImage_.MlStartTime2.Hours, sqlImage_.MlStartTime2.Minutes,
					sqlImage_.MlStartTime2.Seconds);
				DateTime mlEnd2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
					sqlImage_.MlEndTime2.Hours, sqlImage_.MlEndTime2.Minutes, sqlImage_.MlEndTime2.Seconds);

				// SQL запрос, который вставит в таблицу devices 
				// запись об устройстве (если ее еще нет) и вернет dev_id этого устройства
				sqlImage_.Sql = string.Format("select insert_new_device({0}, cast({1} as int2), {2}, {3}, {4}, {5}, {6}, {7}, cast({8} as int2), null, '{9}', cast({10} as int2), '{11}', cast({12} as int2), '{13}', '{14}', '{15}', '{16}');\n",
					sqlImage_.SerialNumber,
					(short)sqlImage_.ConnectionScheme,
					sqlImage_.F_Nominal.ToString(new CultureInfo("en-US")),
					sqlImage_.U_NominalLinear.ToString(new CultureInfo("en-US")),
					sqlImage_.U_NominalPhase.ToString(new CultureInfo("en-US")),	// u_nom_ph
					sqlImage_.I_NominalPhase.ToString(new CultureInfo("en-US")),	// i_nom_ph
					sqlImage_.U_Limit.ToString(new CultureInfo("en-US")),	// u_limit
					sqlImage_.I_Limit.ToString(new CultureInfo("en-US")),	// i_limit
					sqlImage_.CurrentTransducerIndex,	// current_transduser_index
					sqlImage_.DevVersion,
					sqlImage_.T_fliker,
					sqlImage_.ObjectName,
					sqlImage_.ConstraintType,
					mlStart1.ToString("MM.dd.yyyy HH:mm:ss"),
					mlEnd1.ToString("MM.dd.yyyy HH:mm:ss"),
					mlStart2.ToString("MM.dd.yyyy HH:mm:ss"),
					mlEnd2.ToString("MM.dd.yyyy HH:mm:ss")
					);

				string folder_name = string.Format("{0}  #{1}",
						sqlImage_.SerialNumber,
						sqlImage_.ObjectName);

				// SQL запрос, который вставит в таблицу folders 
				// запись об устройстве (если ее еще нет) и вернет folder_id этой папки
				sqlImage_.Sql += String.Format("select insert_dev_folder({0}, '{1}');", 
					sqlImage_.SerialNumber, folder_name);

				List<EmSqlDataNode> sqlDataTopList = new List<EmSqlDataNode>();
				bool res = true;

				if (xmlImage_.ArchivePQP != null)
				{
					for (int i = 0; i < xmlImage_.ArchivePQP.Length; i++)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}

						EmSqlDataNode pqpNode = new EmSqlDataNode();
						res = xml2sql_pqp_period(xmlImage_.ArchivePQP[i],
									ref pqpNode, sqlImage_.SerialNumber);
						if (res) sqlDataTopList.Add(pqpNode);
						else
						{
							EmService.WriteToLogFailed(
								"xml2sql_pqp_period() didn't create any archive!");
						}

						// set ProgressBar position
						cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
						bw_.ReportProgress((int)cur_percent_progress_);
					}
				}

				if (sqlDataTopList.Count > 0)
				{
					sqlImage_.DataPQP = new EmSqlDataNode[sqlDataTopList.Count];
					sqlDataTopList.CopyTo(sqlImage_.DataPQP);
				}
				sqlDataTopList.Clear();

				if (xmlImage_.ArchiveDNS != null)
				{
					for (int i = 0; i < xmlImage_.ArchiveDNS.Length; i++)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}

						EmSqlDataNode dnsNode = new EmSqlDataNode();
						res = xml2sql_dns_period(xmlImage_.ArchiveDNS[i], ref dnsNode,
												sqlImage_.SerialNumber, sqlImage_.ConnectionScheme);

						if (res) sqlDataTopList.Add(dnsNode);
						else
						{
							EmService.WriteToLogFailed(
								"xml2sql_dns_period() didn't create any archive!");
						}

						// set ProgressBar position
						cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
						bw_.ReportProgress((int)cur_percent_progress_);
					}
				}

				if (sqlDataTopList.Count > 0)
				{
					sqlImage_.DataDNO = new EmSqlDataNode[sqlDataTopList.Count];
					sqlDataTopList.CopyTo(sqlImage_.DataDNO);
				}
				sqlDataTopList.Clear();

				if (xmlImage_.ArchiveAVG != null)
				{
					for (int i = 0; i < xmlImage_.ArchiveAVG.Length; i++)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}

						EmSqlDataNode avgNode = new EmSqlDataNode();
						res = xml2sql_avg_period(xmlImage_.ArchiveAVG[i], xmlImage_.ConnectionScheme,
							xmlImage_.F_Nominal, xmlImage_.U_NominalLinear, xmlImage_.U_NominalPhase,
							ref avgNode, sqlImage_.SerialNumber);

						if (res) sqlDataTopList.Add(avgNode);
						else
						{
							EmService.WriteToLogFailed(
								"xml2sql_avg_period() didn't create any archive!");
						}
					}
				}

				if (sqlDataTopList.Count > 0)
				{
					sqlImage_.DataAVG = new EmSqlDataNode[sqlDataTopList.Count];
					sqlDataTopList.CopyTo(sqlImage_.DataAVG);
				}
				sqlDataTopList.Clear();

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmSqlImageCreator32::xml2sql(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				throw;
			}
		}

		#region Building SQL query for inserting PQP data from Device

		private bool xml2sql_pqp_period(EmXmlPQP xmlPQP, ref EmSqlDataNode pqpNode, long ser_number)
		{
			try
			{
				pqpNode.SqlType = EmSqlDataNodeType.PQP;
				pqpNode.Begin = xmlPQP.Start;
				pqpNode.End = xmlPQP.End;

				///////////////////////////////////////
				// парсим по необходимости страницы ПКЭ
				byte[] buffer = xmlPQP.DataPages;

				// извлекаем из массива тип уставок
				short constraint_type = Conversions.bytes_2_short(ref buffer, 384);
				constraint_type -= 1;

				ConnectScheme con_scheme = (ConnectScheme)Conversions.bytes_2_ushort(ref buffer, 396);
				// нумерация схем подключения не совпадает у 33 и 32, поэтому исправляем
				if (con_scheme == ConnectScheme.Unknown || con_scheme == ConnectScheme.Ph3W4)	// 4пр
					con_scheme = ConnectScheme.Ph3W4;
				else if (con_scheme == ConnectScheme.Ph3W3 || con_scheme == ConnectScheme.Ph1W2)	// 3пр
					con_scheme = ConnectScheme.Ph3W3;//???????????

				float u_nom_ph = Conversions.bytes_2_signed_float2w65536(ref buffer, 392);
				float u_nom_lin = Conversions.bytes_2_signed_float2w65536(ref buffer, 388);
				ushort f_nom = Conversions.bytes_2_ushort(ref buffer, 386);

				// времена начала и конца максимальной нагрузки
				DateTime ml_start_time_1 = DateTime.MinValue;
				DateTime ml_end_time_1 = DateTime.MinValue;
				DateTime ml_start_time_2 = DateTime.MinValue;
				DateTime ml_end_time_2 = DateTime.MinValue;

				int iHHs1 = (byte)buffer[1];
				int iMMs1 = (byte)buffer[0];
				int iHHe1 = (byte)buffer[3];
				int iMMe1 = (byte)buffer[2];

				int iHHs2 = (byte)buffer[5];
				int iMMs2 = (byte)buffer[4];
				int iHHe2 = (byte)buffer[7];
				int iMMe2 = (byte)buffer[6];

				iHHs1 %= 24;
				iHHe1 %= 24;
				iHHs2 %= 24;
				iHHe2 %= 24;

				if (!(iHHs1 == iHHe1 && iMMs1 == iMMe1))
				{
					ml_start_time_1 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHs1, iMMs1, 0);
					ml_end_time_1 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHe1, iMMe1, 0);
				}
				if (!(iHHs2 == iHHe2 && iMMs2 == iMMe2))
				{
					ml_start_time_2 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHs2, iMMs2, 0);
					ml_end_time_2 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHe2, iMMe2, 0);
				}

				// SQL запрос, который вставит в таблицу folders 
				// папку для года и вернет folder_id этой папки
				pqpNode.Sql = string.Format("select insert_year_folder({0}, '{1}');\n", ser_number, xmlPQP.Start.Year.ToString());

				// SQL запрос, который вставит в таблицу folders 
				// папку для месяца и вернет folder_id этой папки
				pqpNode.Sql += string.Format("select insert_month_folder({0}, '{1}', '{2}');\n", ser_number, xmlPQP.Start.Year.ToString(), xmlPQP.Start.Month.ToString());

				// на выходе имеем SQL запрос добавления записи 
				// периода ПКЭ а также всех дочерних данных по ПКЭ,
				// имеющий три недостающих параметра - device_id, folder_year_id, folder_month_id
				pqpNode.Sql += string.Format("INSERT INTO day_avg_parameter_times (datetime_id, start_datetime, end_time, device_id, ml_start_time_1, ml_end_time_1, ml_start_time_2, ml_end_time_2, con_scheme, constraint_type, f_nom, u_nom_lin, u_nom_ph, folder_year_id, folder_month_id) VALUES (DEFAULT, '{0}', '{1}', {2}, '{3}', '{4}', '{5}', '{6}', {7}, {8}, {9}, {10}, {11}, {12}, {13});\n",
					xmlPQP.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlPQP.End.ToString("MM.dd.yyyy HH:mm:ss"),
					"{0}",
					ml_start_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_start_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					(short)con_scheme,
					constraint_type,
					f_nom,
					u_nom_lin.ToString(new CultureInfo("en-US")),
					u_nom_ph.ToString(new CultureInfo("en-US")),
					"{1}", "{2}");

				// вычисляем время, с которого начинается фликер. количество минут 
				// кратно периоду фликера (1, 5 или 10)
				DateTime startTimeFlik = xmlPQP.Start;
				if (startTimeFlik.Millisecond > 0)
				{
					startTimeFlik = new DateTime(startTimeFlik.Year, startTimeFlik.Month,
						startTimeFlik.Day, startTimeFlik.Hour, startTimeFlik.Minute,
						startTimeFlik.Second, 0);
					startTimeFlik = startTimeFlik.AddSeconds(1);
				}
				if (startTimeFlik.Second > 0)
				{
					startTimeFlik = new DateTime(startTimeFlik.Year, startTimeFlik.Month,
						startTimeFlik.Day, startTimeFlik.Hour, startTimeFlik.Minute,
						0, 0);
					startTimeFlik = startTimeFlik.AddMinutes(1);
				}
				if (xmlPQP.T_fliker != 0)
				{
					while ((startTimeFlik.Minute % xmlPQP.T_fliker) > 0)
						startTimeFlik = startTimeFlik.AddMinutes(1);
				}
				// вычисляем время, на котором заканчивается фликер
				DateTime endTimeFlik = xmlPQP.End;
				endTimeFlik = new DateTime(endTimeFlik.Year, endTimeFlik.Month,
						endTimeFlik.Day, endTimeFlik.Hour, endTimeFlik.Minute, 0, 0);
				if (xmlPQP.T_fliker != 0)
				{
					while ((endTimeFlik.Minute % xmlPQP.T_fliker) > 0)
						endTimeFlik = endTimeFlik.AddMinutes(-1);
				}

				string sql = string.Empty;

				if (con_scheme != ConnectScheme.Ph1W2)
				{
					xml2sql_pqp_t1(ref buffer, ref sql, 1101); // K_2U
				}
				if (con_scheme == ConnectScheme.Ph3W4 || con_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					xml2sql_pqp_t1(ref buffer, ref sql, 1102); // K_0U
				}

				// t2:  ∆f, δU_y, δU_A(AB), δU_B(BC), δU_C(CA),
				//      δU_y_', δU_A(AB)_', δU_B(BC)_', δU_C(CA)_',
				//      δU_y_", δU_A(AB)_", δU_B(BC)_", δU_C(CA)_"
				//
				xml2sql_pqp_t2(ref buffer, ref sql, 1001, con_scheme);	// ∆f

				xml2sql_pqp_t2(ref buffer, ref sql, 1002, con_scheme); // δU_y
				xml2sql_pqp_t2(ref buffer, ref sql, 1006, con_scheme); // δU_y_'
				xml2sql_pqp_t2(ref buffer, ref sql, 1010, con_scheme); // δU_y_"

				if (con_scheme == ConnectScheme.Ph3W4 || con_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					xml2sql_pqp_t2(ref buffer, ref sql, 1003, con_scheme); // δU_A
					xml2sql_pqp_t2(ref buffer, ref sql, 1007, con_scheme); // δU_A_'
					xml2sql_pqp_t2(ref buffer, ref sql, 1011, con_scheme); // δU_A_"

					xml2sql_pqp_t2(ref buffer, ref sql, 1004, con_scheme); // δU_B
					xml2sql_pqp_t2(ref buffer, ref sql, 1005, con_scheme); // δU_C

					xml2sql_pqp_t2(ref buffer, ref sql, 1008, con_scheme); // δU_B_'
					xml2sql_pqp_t2(ref buffer, ref sql, 1009, con_scheme); // δU_C_'

					xml2sql_pqp_t2(ref buffer, ref sql, 1012, con_scheme); // δU_B_"
					xml2sql_pqp_t2(ref buffer, ref sql, 1013, con_scheme); // δU_C_"
				}

				if (con_scheme != ConnectScheme.Ph1W2)
				{
					xml2sql_pqp_t2(ref buffer, ref sql, 1014, con_scheme); // δU_AB
					xml2sql_pqp_t2(ref buffer, ref sql, 1015, con_scheme); // δU_BC
					xml2sql_pqp_t2(ref buffer, ref sql, 1016, con_scheme); // δU_CA

					xml2sql_pqp_t2(ref buffer, ref sql, 1017, con_scheme); // δU_AB_'
					xml2sql_pqp_t2(ref buffer, ref sql, 1018, con_scheme); // δU_BC_'
					xml2sql_pqp_t2(ref buffer, ref sql, 1019, con_scheme); // δU_CA_'

					xml2sql_pqp_t2(ref buffer, ref sql, 1020, con_scheme); // δU_AB_"
					xml2sql_pqp_t2(ref buffer, ref sql, 1021, con_scheme); // δU_BC_"
					xml2sql_pqp_t2(ref buffer, ref sql, 1022, con_scheme); // δU_CA_"
				}

				// t3:  events
				xml2sql_pqp_t3(ref buffer, ref sql, 3712, "A", 0);	// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 3760, "A", 1);	// dip
				//if (connection_scheme < 3)
				//{
				xml2sql_pqp_t3(ref buffer, ref sql, 3904, "B", 0);	// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 3952, "B", 1);	// dip
				xml2sql_pqp_t3(ref buffer, ref sql, 4096, "C", 0);	// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 4144, "C", 1);	// dip
				//if (con_scheme == 1)
				//{
				xml2sql_pqp_t3(ref buffer, ref sql, 4288, "*4", 0);	// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 4336, "*4", 1);	// dip
				//}
				//if (con_scheme == 2)
				//{
				xml2sql_pqp_t3(ref buffer, ref sql, 4384, "*", 0);	// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 4432, "*", 1);	// dip
				//}
				//}
				xml2sql_pqp_t3(ref buffer, ref sql, 3808, "AB", 0);		// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 3856, "AB", 1);		// dip
				xml2sql_pqp_t3(ref buffer, ref sql, 4000, "BC", 0);		// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 4048, "BC", 1);		// dip
				xml2sql_pqp_t3(ref buffer, ref sql, 4192, "CA", 0);		// swell
				xml2sql_pqp_t3(ref buffer, ref sql, 4240, "CA", 1);		// dip

				// t4:	K_UA(2..40), K_UB(2..40), K_UC(2..40) а также K_UA, K_UB и K_UC
				// в таблице parameters для прошивки 6.2 добавлена новая группа параметров
				// с идентификаторами от 1301 до 1340 для линейных несинусоидальностей;
				// их имена совпадают полностью с именами параметров от 1201 до 1240 (для фазных)			
				// 6.2.done
				int j = 0;
				for (j = 0; j < 39; ++j)
				{
					xml2sql_pqp_t4(ref buffer, ref sql, j * 12, 44 + j * 4, 1202 + j, con_scheme);
				}
				xml2sql_pqp_t4(ref buffer, ref sql, j * 12, 44 + j * 4, 1201, con_scheme);

				// fliker
				xml2sql_pqp_t5(ref buffer, ref sql, con_scheme,
						startTimeFlik, endTimeFlik, xmlPQP.T_fliker);

				pqpNode.Sql += sql;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_pqp_period():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		private void xml2sql_pqp_t1(ref byte[] buffer, ref string SqlQuery, int param_id)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				ushort num_all = 0;			// общее количество отсчетов
				ushort num_nrm_rng = 0;		// отсчетов в НДП
				ushort num_max_rng = 0;		// отсчетов между ПДП и НДП
				ushort num_out_max_rng = 0;	// отсчетов за ПДП
				float fNDP = 0, fPDP = 0, f_up = 0, f_max = 0;

				switch (param_id)
				{
					case 1101:		// K_2U
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 802);
						num_all = Conversions.bytes_2_ushort(ref buffer, 800);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 806);
						//num_max_rng = (ushort)(num_nrm_rng - (num_all - num_out_max_rng));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						fNDP = Conversions.bytes_2_signed_float1024(ref buffer, 56);
						fPDP = Conversions.bytes_2_signed_float1024(ref buffer, 58);
						f_up = Conversions.bytes_2_float2wIEEE754(ref buffer, 808, true) * 100;
						f_max = Conversions.bytes_2_float2wIEEE754(ref buffer, 812, true) * 100;
						break;
					case 1102:		// K_0U
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 818);
						num_all = Conversions.bytes_2_ushort(ref buffer, 816);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 822);
						//num_max_rng = (ushort)(num_nrm_rng - (num_all - num_out_max_rng));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						fNDP = Conversions.bytes_2_signed_float1024(ref buffer, 60);
						fPDP = Conversions.bytes_2_signed_float1024(ref buffer, 62);
						f_up = Conversions.bytes_2_float2wIEEE754(ref buffer, 824, true) * 100;
						f_max = Conversions.bytes_2_float2wIEEE754(ref buffer, 828, true) * 100;
						break;
				}

				// округляем, а то не вставится в БД
				f_max = (float)Math.Round(f_max, 15);

				// если данные по отсчетам по какой-то причине отсутствуют
				// делаем вид, что все прошло успешно
				if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return;

				SqlQuery += string.Format("INSERT INTO day_avg_parameters_t1 (datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});\n",
					param_id,
					num_nrm_rng.ToString(ci_enUS),
					num_max_rng.ToString(ci_enUS),
					num_out_max_rng.ToString(ci_enUS),
					fNDP.ToString(ci_enUS),		// НДП
					fPDP.ToString(ci_enUS),		// ПДП
					f_up.ToString(ci_enUS),		// 95%
					f_max.ToString(ci_enUS));	// max
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in xml2sql_pqp_t1():  " + e.Message);
			}
		}

		private void xml2sql_pqp_t2(ref byte[] buffer, ref string SqlQuery, int param_id,
									ConnectScheme con_scheme)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// ГОСТы из уставок
				float fNDP_d = 0, fNDP_u = 0, fPDP_d = 0, fPDP_u = 0; // ГОСТ  НДП н, НДП в, ПДП н, ПДП в)
				float f_max = 0, f_min = 0, f_95_low = 0, f_95_up = 0;
				ushort num_out_max_rng = 0, num_nrm_rng = 0, num_all = 0, num_max_rng = 0;

				get_data_for_t2(ref buffer, param_id,
								ref fNDP_d, ref fNDP_u, ref fPDP_d, ref fPDP_u,
								ref f_max, ref f_min, ref f_95_low, ref f_95_up,
								ref num_out_max_rng, ref num_nrm_rng, ref num_all, ref num_max_rng,
								con_scheme);

				// если данные по отсчетам по какой-то причине отсутствуют, делаем вид, 
				// что все прошло более чем успешно :)
				if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return;

				SqlQuery += string.Format("INSERT INTO day_avg_parameters_t2 (datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_bottom, real_nrm_rng_top, real_max_rng_bottom, real_max_rng_top, calc_nrm_rng_bottom, calc_nrm_rng_top, calc_max_rng_bottom, calc_max_rng_top) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});\n",
					param_id,
					num_nrm_rng.ToString(),		// отсчетов в НДП
					num_max_rng.ToString(),		// отсчетов между ПДП и НДП
					num_out_max_rng.ToString(),	// отсчетов за ПДП

					fNDP_d.ToString(ci_enUS),	// НДП н
					fNDP_u.ToString(ci_enUS),	// НДП в
					fPDP_d.ToString(ci_enUS),	// ПДП н
					fPDP_u.ToString(ci_enUS),	// ПДП в

					f_95_low.ToString(ci_enUS),	// 95% н
					f_95_up.ToString(ci_enUS),	// 95% в
					f_min.ToString(ci_enUS),	// min н
					f_max.ToString(ci_enUS));	// max в
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in xml2sql_pqp_t2():  " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t3(ref byte[] buffer, ref string SqlQuery, int Shift, string phase, 
			short event_type)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// константа, необходимая для перевода десяток миллисекунд в отсчеты
				long time_const = 10000;

				// общая продолжительность
				long hi_word = 0, lo_word = 0;
				// 1st time
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 2);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift);

				// при вычислении common_duration отбрасываем последний знак (но сохраняем разрядность),
				// чтобы дробная часть миллисекунд была не больше трех знаков, иначе
				// потом возникает проблема при отображении данных (значение не конвертируется
				// в тип DateTime)
				long duration = (long)((lo_word + hi_word * 0x10000) * time_const);
				TimeSpan common_duration = new TimeSpan(TrimDuration(duration));

				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 10);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 8);
				long lMaxPeriodPeriod = (long)((lo_word + hi_word * 0x10000) * time_const);
				TimeSpan max_period_period = new TimeSpan(TrimDuration(lMaxPeriodPeriod));

				//TimeSpan max_period_period = new TimeSpan((long)Conversions.bytes_2_uint(ref Buffer, Shift + 8) * time_const);
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 6);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 4);
				long lMaxValuePeriod = (long)((lo_word + hi_word * 0x10000) * time_const);
				TimeSpan max_value_period = new TimeSpan(TrimDuration(lMaxValuePeriod));

				// общее число перенапряжений за время регистрации
				// накладываем маску на третий бит самого старшего 
				// (после перестановки старшего и младшего байт в каждом слове)
				// байта в четверке ( AND 00011111b = 1Fh = 31d )
				//buffer[Shift + 17] &= 31;

				uint common_number = Conversions.bytes_2_ushort(ref buffer, Shift + 16);
				//float max_period_value = (float)Conversions.bytes_2_short(ref buffer, Shift + 14) / 8192;
				//float max_value_value = (float)Conversions.bytes_2_short(ref buffer, Shift + 12) / 8192;
				float max_period_value = (float)Conversions.bytes_2_usigned_float_Q_1_15(ref buffer, Shift + 14);
				float max_value_value = (float)Conversions.bytes_2_usigned_float_Q_1_15(ref buffer, Shift + 12);
				if (event_type == 0)	//swell
				{
					//max_period_value += 1.0f;		// единица прибавляется при отображении поэтому 
					//max_value_value += 1.0f;		// здесь не надо
				}
				else
				{
					max_period_value *= 100.0f;
					max_value_value *= 100.0f;
				}

				// если из-за погрешности вычислений длительности получилось больше суток 
				// (несмотря на использование коэффициента), то приравниваем длительность 
				// суткам, а на экран выводим сообщенние
				if (common_duration.Days > 0)
				{
					//if (OnInvalidDuration != null)
					//{
					//    OnInvalidDuration(common_duration);
					//}
					common_duration = new TimeSpan(0, 23, 59, 59, 999);
				}
				if (max_period_period.Days > 0)
				{
					max_period_period = new TimeSpan(0, 23, 59, 59, 999);
				}
				if (max_value_period.Days > 0)
				{
					max_value_period = new TimeSpan(0, 23, 59, 59, 999);
				}

				SqlQuery += string.Format("INSERT INTO day_avg_parameters_t3 (datetime_id, event_type, phase, common_number, common_duration, max_period_period, max_value_period, max_period_value, max_value_value) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', '{5}', {6}, {7});\n",
					event_type,
					phase,
					common_number,
					common_duration.ToString(),
					max_period_period.ToString(),
					max_value_period.ToString(),
					max_period_value.ToString(ci_enUS),
					max_value_value.ToString(ci_enUS));
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in xml2sql_pqp_t3():  " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t4(ref byte[] buffer, ref string SqlQuery, int Shift, int ShiftNominal, 
									int param_id, ConnectScheme ConnectionScheme)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				//ShiftNominal += 24;
				ShiftNominal += 20;

				float f_95_a, f_max_a, f_95_b, f_max_b, f_95_c, f_max_c;

				// ФАЗНЫЕ ГАРМОНИКИ:
				if (ConnectionScheme != ConnectScheme.Ph3W3 && 
					ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					// ГОСТы из уставок
					float fNDP = Conversions.bytes_2_signed_float1024(ref buffer, ShiftNominal);
					float fPDP = Conversions.bytes_2_signed_float1024(ref buffer, ShiftNominal + 2);
					// только первая фаза(!)
					// отсчеты прибора
					ushort num_nrm_rng_a = Conversions.bytes_2_ushort(ref buffer, 832 + Shift + 2);	 // в НДП
					ushort num_max_rng_a = Conversions.bytes_2_ushort(ref buffer, 832 + Shift + 4);	 // в ПДП
					ushort num_out_max_rng_a = Conversions.bytes_2_ushort(ref buffer, 832 + Shift + 6);	// за ПДП

					// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
					f_95_a = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 832 + Shift + 8) * 100;
					f_max_a = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 832 + Shift + 10) * 100;

					if (ConnectionScheme == ConnectScheme.Ph3W4 ||
						ConnectionScheme == ConnectScheme.Ph3W4_B_calc) // если 3ф4пр... то дорасчитываем еще 2
					{
						// отсчеты прибора
						ushort num_nrm_rng_b = Conversions.bytes_2_ushort(ref buffer, Shift + 1792 + 2);  // в НДП ф.B
						ushort num_max_rng_b = Conversions.bytes_2_ushort(ref buffer, Shift + 1792 + 4);  // в ПДП ф.B
						ushort num_out_max_rng_b = Conversions.bytes_2_ushort(ref buffer, Shift + 1792 + 6);// за ПДП ф.B

						ushort num_nrm_rng_c = Conversions.bytes_2_ushort(ref buffer, Shift + 2752 + 2);  // в НДП ф.C
						ushort num_max_rng_c = Conversions.bytes_2_ushort(ref buffer, Shift + 2752 + 4);  // в ПДП ф.C
						ushort num_out_max_rng_c = Conversions.bytes_2_ushort(ref buffer, Shift + 2752 + 6);// за ПДП ф.C

						// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
						f_95_b = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 1792 + 8) * 100;// ф.B
						f_max_b = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 1792 + 10) * 100;	// ф.B

						f_95_c = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 2752 + 8) * 100;// ф.C
						f_max_c = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 2752 + 10) * 100;	// ф.C

						if (num_nrm_rng_a != 0 || num_max_rng_a != 0 || num_out_max_rng_a != 0 ||
							num_nrm_rng_b != 0 || num_max_rng_b != 0 || num_out_max_rng_b != 0 ||
							num_nrm_rng_c != 0 || num_max_rng_c != 0 || num_out_max_rng_c != 0)
						{

							SqlQuery += String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17});\n",
								param_id,
								// фаза A
								num_nrm_rng_a.ToString(),		// отсчетов в НДП
								num_max_rng_a.ToString(),		// отсчетов в ПДП
								num_out_max_rng_a.ToString(),	// отсчетов за ПДП
								f_95_a.ToString(ci_enUS),		// 95%
								f_max_a.ToString(ci_enUS),		// max
								// фаза B
								num_nrm_rng_b.ToString(),		// отсчетов в НДП
								num_max_rng_b.ToString(),		// отсчетов в ПДП
								num_out_max_rng_b.ToString(),	// отсчетов за ПДП
								f_95_b.ToString(ci_enUS),		// 95%
								f_max_b.ToString(ci_enUS),		// max
								// фаза C
								num_nrm_rng_c.ToString(),		// отсчетов в НДП
								num_max_rng_c.ToString(),		// отсчетов в ПДП
								num_out_max_rng_c.ToString(),	// отсчетов за ПДП
								f_95_c.ToString(ci_enUS),		// 95%
								f_max_c.ToString(ci_enUS),		// max
								// ГОСТы
								fNDP.ToString(ci_enUS),			// НДП
								fPDP.ToString(ci_enUS));		// ПДП
						}
					}
					else // только одна фаза => запрос несколько упростится
					{
						if (num_nrm_rng_a != 0 || num_max_rng_a != 0 || num_out_max_rng_a != 0)
						{
							SqlQuery += String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, 0,0,0,0,0,0,0,0,0,0, {6}, {7});\n",
								param_id,
								// фаза 1
								num_nrm_rng_a.ToString(),		// отсчетов в НДП
								num_max_rng_a.ToString(),		// отсчетов в ПДП
								num_out_max_rng_a.ToString(),	// отсчетов за ПДП
								f_95_a.ToString(ci_enUS),		// 95%
								f_max_a.ToString(ci_enUS),		// max
								// ГОСТы
								fNDP.ToString(ci_enUS),			// НДП
								fPDP.ToString(ci_enUS));		// ПДП
						}
					}
				}

				// ЛИНЕЙНЫЕ ГАРМОНИКИ:
				if (ConnectionScheme != ConnectScheme.Ph1W2)
				{
					param_id += 100;

					// ГОСТы из уставок
					float fNDP = Conversions.bytes_2_signed_float1024(ref buffer, ShiftNominal + 160);
					float fPDP = Conversions.bytes_2_signed_float1024(ref buffer, ShiftNominal + 162);

					// отсчеты прибора
					ushort num_nrm_rng_ab = Conversions.bytes_2_ushort(ref buffer, 1312 + Shift + 2);	// в НДП ф.AB
					ushort num_max_rng_ab = Conversions.bytes_2_ushort(ref buffer, 1312 + Shift + 4);	// в ПДП ф.AB
					ushort num_out_max_rng_ab = Conversions.bytes_2_ushort(ref buffer, 1312 + Shift + 6);// за ПДП ф.AB

					ushort num_nrm_rng_bc = Conversions.bytes_2_ushort(ref buffer, Shift + 2272 + 2);	// в НДП ф.BC
					ushort num_max_rng_bc = Conversions.bytes_2_ushort(ref buffer, Shift + 2272 + 4);	// в ПДП ф.BC
					ushort num_out_max_rng_bc = Conversions.bytes_2_ushort(ref buffer, Shift + 2272 + 6);// за ПДП ф.BC

					ushort num_nrm_rng_ca = Conversions.bytes_2_ushort(ref buffer, Shift + 3232 + 2);	// в НДП ф.CA
					ushort num_max_rng_ca = Conversions.bytes_2_ushort(ref buffer, Shift + 3232 + 4);	// в ПДП ф.CA
					ushort num_out_max_rng_ca = Conversions.bytes_2_ushort(ref buffer, Shift + 3232 + 6);// за ПДП ф.CA

					// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
					float f_95_ab = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 1312 + Shift + 8) * 100;//AB
					float f_max_ab = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 1312 + Shift + 10) * 100;//AC

					float f_95_bc = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 2272 + 8) * 100;//BC
					float f_max_bc = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 2272 + 10) * 100;//BC

					float f_95_ca = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 3232 + 8) * 100;//CA
					float f_max_ca = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, Shift + 3232 + 10) * 100;//CA

					if (num_nrm_rng_ab != 0 || num_max_rng_ab != 0 || num_out_max_rng_ab != 0 ||
							num_nrm_rng_bc != 0 || num_max_rng_bc != 0 || num_out_max_rng_bc != 0 ||
							num_nrm_rng_ca != 0 || num_max_rng_ca != 0 || num_out_max_rng_ca != 0)
					{

						SqlQuery += String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17});\n",
							param_id,
							// фаза AB
							num_nrm_rng_ab.ToString(),		// отсчетов в НДП
							num_max_rng_ab.ToString(),		// отсчетов в ПДП
							num_out_max_rng_ab.ToString(),	// отсчетов за ПДП
							f_95_ab.ToString(ci_enUS),		// 95%
							f_max_ab.ToString(ci_enUS),		// max
							// фаза BC
							num_nrm_rng_bc.ToString(),		// отсчетов в НДП
							num_max_rng_bc.ToString(),		// отсчетов в ПДП
							num_out_max_rng_bc.ToString(),	// отсчетов за ПДП
							f_95_bc.ToString(ci_enUS),		// 95%
							f_max_bc.ToString(ci_enUS),		// max
							// фаза CA
							num_nrm_rng_ca.ToString(),		// отсчетов в НДП
							num_max_rng_ca.ToString(),		// отсчетов в ПДП
							num_out_max_rng_ca.ToString(),	// отсчетов за ПДП
							f_95_ca.ToString(ci_enUS),		// 95%
							f_max_ca.ToString(ci_enUS),		// max
							// ГОСТы
							fNDP.ToString(ci_enUS),			// НДП
							fPDP.ToString(ci_enUS));		// ПДП
					}
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in xml2sql_pqp_t4():  " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t5(ref byte[] buffer, ref string SqlQuery, ConnectScheme connection_scheme,
					DateTime startTime, DateTime endTime, short t_flik)
		{
			try
			{
				//DateTime curTime = startTime;
				DateTime curTime = new DateTime(startTime.Year, startTime.Month, startTime.Day,
												0, 0, 0, 0);
				//DateTime fullTime = startTime;
				//fullTime = fullTime.AddHours(2);  // время, начиная с которого фликер становится 
				// полноценным, т.е. измеренным не меньше чем за 2 часа
				//short start = 0, end = 0;
				CultureInfo ci_enUS = new CultureInfo("en-US");

				List<float> flik_A = new List<float>();
				List<float> flik_B = new List<float>();
				List<float> flik_C = new List<float>();
				List<float> flik_A_long = new List<float>();
				List<float> flik_B_long = new List<float>();
				List<float> flik_C_long = new List<float>();
				int Shift = 4480;
				int Shift_long = 6208;
				for (int i = 0; i < 144; ++i)
				{
					// st
					if (BitConverter.ToInt32(buffer, Shift) != -1)
						flik_A.Add(Conversions.bytes_2_float2wIEEE754(ref buffer, Shift, true));
					else
						flik_A.Add(-1);

					//lt
					if ((((i + 1) % 12) == 0 && i != 0) && (BitConverter.ToInt32(buffer, Shift_long) != -1))
						flik_A_long.Add(Conversions.bytes_2_float2wIEEE754(ref buffer, Shift_long, true));
					else
						flik_A_long.Add(-1);

					if (connection_scheme != ConnectScheme.Ph1W2)
					{
						// st
						if (BitConverter.ToInt32(buffer, Shift + 4) != -1)
							flik_B.Add(Conversions.bytes_2_float2wIEEE754(ref buffer, Shift + 4, true));
						else
							flik_B.Add(-1);
						if (BitConverter.ToInt32(buffer, Shift + 8) != -1)
							flik_C.Add(Conversions.bytes_2_float2wIEEE754(ref buffer, Shift + 8, true));
						else
							flik_C.Add(-1);

						// lt
						if ((((i + 1) % 12) == 0 && i != 0) && (BitConverter.ToInt32(buffer, Shift_long + 4) != -1))
							flik_B_long.Add(Conversions.bytes_2_float2wIEEE754(ref buffer, Shift_long + 4, true));
						else
							flik_B_long.Add(-1);

						if ((((i + 1) % 12) == 0 && i != 0) && (BitConverter.ToInt32(buffer, Shift_long + 8) != -1))
							flik_C_long.Add(Conversions.bytes_2_float2wIEEE754(ref buffer, Shift_long + 8, true));
						else
							flik_C_long.Add(-1);
					}
					Shift += 12;
					if (((i + 1) % 12) == 0 && i != 0)
						Shift_long += 12;
				}

				for (int i = 0; i < flik_A.Count; ++i)
				{
					if (flik_A.Count > i && Single.IsInfinity(flik_A[i]))
						flik_A[i] = -1;
					if (flik_A_long.Count > i && Single.IsInfinity(flik_A_long[i]))
						flik_A_long[i] = -1;
					if (flik_B.Count > i && Single.IsInfinity(flik_B[i]))
						flik_B[i] = -1;
					if (flik_B_long.Count > i && Single.IsInfinity(flik_B_long[i]))
						flik_B_long[i] = -1;
					if (flik_C.Count > i && Single.IsInfinity(flik_C[i]))
						flik_C[i] = -1;
					if (flik_C_long.Count > i && Single.IsInfinity(flik_C_long[i]))
						flik_C_long[i] = -1;

					if (connection_scheme == ConnectScheme.Ph1W2)
					{
						SqlQuery += string.Format("INSERT INTO day_avg_parameters_t5 (datetime_id, flik_time, flik_a, flik_a_long) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1}, {2});\n",
							curTime.ToString("HH:mm:ss"),
							flik_A[i].ToString(ci_enUS),
							flik_A_long[i].ToString(ci_enUS));
						//getLongFliker(flik_A, start, end).ToString(ci_enUS));
					}
					else
					{
						SqlQuery += string.Format("INSERT INTO day_avg_parameters_t5 (datetime_id, flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6});\n",
							curTime.ToString("HH:mm:ss"),//curTime.ToString("MM.dd.yyyy HH:mm:ss"),
							flik_A[i].ToString(ci_enUS),
							flik_A_long[i].ToString(ci_enUS),
							flik_B[i].ToString(ci_enUS),
							flik_B_long[i].ToString(ci_enUS),
							flik_C[i].ToString(ci_enUS),
							flik_C_long[i].ToString(ci_enUS));
					}

					curTime = curTime.AddMinutes(10);
					//if (curTime > endTime) 
					//	break;
					//++end;
					//if (end >= flik_A.Count) 
					//break;
					//if (curTime > fullTime)
					//++start;
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in xml2sql_pqp_t5():  " + e.Message);
				throw e;
			}
		}

		#endregion

		#region Building SQL query for inserting Events data from Device

		private bool xml2sql_dns_period(EmXmlDNS xmlDNS, ref EmSqlDataNode dnsNode, 
										long ser_number, ConnectScheme conScheme)
		{
			try
			{
				dnsNode.SqlType = EmSqlDataNodeType.Events;

				dnsNode.Begin = xmlDNS.Start;
				dnsNode.End = xmlDNS.End;

				bool res = false;
				string tempSql = "";
				if (xmlDNS.DataPages != null)
					res = xml2sql_dns_archive(xmlDNS, ser_number, ref tempSql);
				if (xmlDNS.CurrentDNSBuffer != null)
					res = xml2sql_dns_current_period(ref xmlDNS, ref tempSql, conScheme);

				if (tempSql != "")
				{
					// SQL запрос, который вставит в таблицу folders 
					// папку для года и вернет folder_id этой папки
					dnsNode.Sql += string.Format("select insert_year_folder({0}, '{1}');\n", 
						ser_number, 
						xmlDNS.Start.Year.ToString());

					// SQL запрос, который вставит в таблицу folders 
					// папку для месяца и вернет folder_id этой папки
					dnsNode.Sql += string.Format("select insert_month_folder({0}, '{1}', '{2}');\n", 
						ser_number, 
						xmlDNS.Start.Year.ToString(), 
						xmlDNS.Start.Month.ToString());

					dnsNode.Sql += string.Format("INSERT INTO dips_and_overs_times (datetime_id, start_datetime, end_datetime, device_id, folder_year_id, folder_month_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4});\n",
						xmlDNS.Start.ToString("MM.dd.yyyy HH:mm:ss"),
						xmlDNS.End.ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}", "{1}", "{2}");

					dnsNode.Sql += tempSql;
				}
				else
				{
					dnsNode.Sql = "";
					EmService.WriteToLogFailed(
						"xml2sql_dns_period(): There was no DNS event during this period: "
						+ xmlDNS.Start.ToString() + " - " + xmlDNS.End.ToString());
					return false;
				}

				return res;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_dns_period():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		private bool xml2sql_dns_archive(EmXmlDNS xmlDNS, long ser_number, ref string tempSql)
		{
			byte[] buffer = xmlDNS.DataPages;
			int recLen = 64;	// length of one record is 64 bytes
			long dnsNum = buffer.Length / recLen;

			StringBuilder sbMain = new StringBuilder();
			int i = 0;
			DateTime dtStart, dtEnd;
			bool wasEarlier = false;
			bool isFinished = true;
			// флаг показывает, что нашлось хотя бы одно валидное событие
			bool wasEvent = false;
			try
			{
				for (i = 0; i < dnsNum; i++)
				{
					long numEvent = Conversions.bytes_2_uint_new(ref buffer, 0 + i * recLen);
					ushort typeEvent = Conversions.bytes_2_ushort(ref buffer, 4 + i * recLen);
					ushort millisec = Conversions.bytes_2_ushort(ref buffer, 16 + i * recLen);
					if (millisec > 999) millisec = 999;
					try 
					{
						ushort iYear = (ushort)(buffer[8 + i * recLen] + (buffer[9 + i * recLen] << 8));
						if (iYear < 2008) iYear += 2000;
						byte iMo = buffer[11 + i * recLen];
						byte iDay = buffer[10 + i * recLen];
						dtStart = new DateTime(iYear, iMo, iDay,
										buffer[13 + i * recLen],
										buffer[12 + i * recLen],
										buffer[14 + i * recLen],
										millisec);

						millisec = Conversions.bytes_2_ushort(ref buffer, 26 + i * recLen);
						if (millisec > 999) millisec = 999;
						ushort iYearEnd = (ushort)(buffer[18 + i * recLen] + (buffer[19 + i * recLen] << 8));
						if (iYearEnd < 2008) iYearEnd += 2000;
						byte iMoEnd = buffer[21 + i * recLen];
						byte iDayEnd = buffer[20 + i * recLen];
						dtEnd = new DateTime(iYearEnd, iMoEnd, iDayEnd,
										buffer[23 + i * recLen],
										buffer[22 + i * recLen],
										buffer[24 + i * recLen],
										millisec);
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed("xml2sql_dns_period: invalid data!  " + i.ToString()
														+ "   " + ex.Message);
						continue;
					}

					if (dtStart < xmlDNS.Start)
					{
						dtStart = xmlDNS.Start;
						wasEarlier = true;
					}
					else
						wasEarlier = false;

					if (dtEnd > xmlDNS.End)
					{
						dtEnd = xmlDNS.End;
						isFinished = false;
					}
					else
						isFinished = true;

					if (dtStart > dtEnd)
					{
						EmService.WriteToLogFailed("xml2sql_dns_period(): dtStart > dtEnd  " +
											typeEvent.ToString() + "   " + numEvent.ToString());
						continue;
					}

					wasEvent = true;

					// get phase and type
					string phase = string.Empty;
					int type = -1;
					if (typeEvent == 0x00) { phase = "A"; type = 1; }
					else if (typeEvent == 0x01) { phase = "B"; type = 1; }
					else if (typeEvent == 0x02) { phase = "C"; type = 1; }
					else if (typeEvent == 0x03) { phase = "AB"; type = 1; }
					else if (typeEvent == 0x04) { phase = "BC"; type = 1; }
					else if (typeEvent == 0x05) { phase = "CA"; type = 1; }
					else if (typeEvent == 0x07) { phase = "ABCN"; type = 1; }
					else if (typeEvent == 0x06) { phase = "ABC"; type = 1; }
					else if (typeEvent == 0x08) { phase = "A"; type = 0; }
					else if (typeEvent == 0x09) { phase = "B"; type = 0; }
					else if (typeEvent == 0x0A) { phase = "C"; type = 0; }
					else if (typeEvent == 0x0B) { phase = "AB"; type = 0; }
					else if (typeEvent == 0x0C) { phase = "BC"; type = 0; }
					else if (typeEvent == 0x0D) { phase = "CA"; type = 0; }
					else if (typeEvent == 0x0F) { phase = "ABCN"; type = 0; }
					else if (typeEvent == 0x0E) { phase = "ABC"; type = 0; }

					float value = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 6 + i * recLen);
					if (type == 0)	//swell
					{
						//value += 1.0f;		// единица прибавляется при отображении поэтому не надо
					}
					else   // dip
					{
						value *= 100.0f;
					}

					sbMain.Append(string.Format("INSERT INTO dips_and_overs (datetime_id, event_type, phase, deviation, start_datetime, end_datetime, pointer, is_finished, is_earlier, phase_num) VALUES (currval('dips_and_overs_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', {5}, {6}, {7}, {8});\n",
						type,
						phase,
						value.ToString(new CultureInfo("en-US")),
						dtStart.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
						dtEnd.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
						numEvent,
						isFinished.ToString(),
						wasEarlier.ToString(),
						EmService.GetPhaseAsNumber(phase))
						);

					// set ProgressBar position
					//cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
					//bw_.ReportProgress((int)cur_percent_progress_);
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in xml2sql_dns_period(): " + ex.Message +
													"  i = " + i.ToString());
				return false;
			}
			if (wasEvent)
				tempSql = sbMain.ToString();
			else
				tempSql = "";

			return true;
		}

		private bool xml2sql_dns_current_period(ref EmXmlDNS xmlDNS, ref string tempSql, 
												ConnectScheme conScheme)
		{
			if (xmlDNS.CurrentDNSBuffer == null) return true;

			byte[] buffer = xmlDNS.CurrentDNSBuffer;

			try
			{
				// какие провалы или перенапряжения существуют
				ushort whatExists = Conversions.bytes_2_ushortBackToFront(ref buffer, 8);
				// текущий бит для проверки существования событий
				ushort curByte = 0x01;

				int offset = 16;
				ushort typeEvent = 0;
				bool wasEarlier = false;
				DateTime dtStart, dtEnd;
				for (int iPhase = 0; iPhase < /*EmService.CountPhases*/6; ++iPhase)
				{
					if (((whatExists & curByte) != 0) || ((whatExists & (curByte * 2)) != 0))
					{
						long numEvent = Conversions.bytes_2_uint_new(ref buffer, 12 + iPhase * offset);
						if ((whatExists & curByte) != 0)
						{
							typeEvent = 1;
						}
						else
						{
							typeEvent = 0;
						}

						float value = Conversions.bytes_2_usigned_float_Q_1_15(ref buffer,
													26 + iPhase * offset);
						if (typeEvent == 0)	//swell
						{
							//value += 1.0f;		// единица прибавляется при отображении поэтому не надо
						}
						else   // dip
						{
							value *= 100.0f;
						}

						try
						{
							ushort iYear =
								(ushort)(buffer[16 + iPhase * offset] + (buffer[17 + iPhase * offset] << 8));
							byte iMo = buffer[19 + iPhase * offset];
							byte iDay = buffer[18 + iPhase * offset];
							ushort millisec = Conversions.bytes_2_ushort(ref buffer, 24 + iPhase * offset);
							if (millisec > 999) millisec = 999;
							dtStart = new DateTime(iYear, iMo, iDay,
											buffer[21 + iPhase * offset],
											buffer[20 + iPhase * offset],
											buffer[22 + iPhase * offset],
											millisec);
						}
						catch (Exception ex)
						{
							EmService.WriteToLogFailed("xml2sql_dns_current_period: invalid data!  "
													+ ex.Message);
							continue;
						}

						dtEnd = DateTime.Now;

						// если не попадает в заданный интервал - пропускаем
						if (dtStart >= xmlDNS.End) continue;

						if (dtStart < xmlDNS.Start)
						{
							dtStart = xmlDNS.Start;
							wasEarlier = true;
						}
						else
							wasEarlier = false;

						if (dtEnd > xmlDNS.End) dtEnd = xmlDNS.End;

						if (dtStart > dtEnd) continue;

						tempSql += string.Format("INSERT INTO dips_and_overs (datetime_id, event_type, phase, deviation, start_datetime, end_datetime, pointer, is_finished, is_earlier, phase_num) VALUES (currval('dips_and_overs_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', {5}, FALSE, {6}, {7});\n",
								typeEvent,
								EmService.GetPhaseName(iPhase),
								value.ToString(new CultureInfo("en-US")),
								dtStart.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
								dtEnd.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
								numEvent,
								wasEarlier.ToString(),
								iPhase);
					}
					curByte *= 4;
				}

				offset = -1;
				int curPhase = -1;
				if (conScheme == ConnectScheme.Ph3W4 || conScheme == ConnectScheme.Ph3W4_B_calc)
				{
					if ((whatExists & 0x1000) != 0)
					{
						offset = 108; curPhase = 6; typeEvent = 1;
					}
					if ((whatExists & 0x2000) != 0)
					{
						offset = 124; curPhase = 6; typeEvent = 0;
					}
				}
				else
				{
					if ((whatExists & 0x4000) != 0)
					{
						offset = 140; curPhase = 7; typeEvent = 1;
					}
					if ((whatExists & 0x8000) != 0)
					{
						offset = 156; curPhase = 7; typeEvent = 0;
					}
				}

				if (offset != -1)
				{
					long numEvent = Conversions.bytes_2_uint_new(ref buffer, 0 + offset);
					float value = Conversions.bytes_2_usigned_float_Q_1_15(ref buffer, 14 + offset);
					if (typeEvent == 1)
						value *= 100.0f;
					//else
					//value += 1.0f;    // единица прибавляется при отображении поэтому не надо

					try
					{
						ushort iYear =
							(ushort)(buffer[4 + offset] + (buffer[5 + offset] << 8));
						byte iMo = buffer[7 + offset];
						byte iDay = buffer[6 + offset];
						ushort millisec = Conversions.bytes_2_ushort(ref buffer, 12 + offset);
						if (millisec > 999) millisec = 999;
						dtStart = new DateTime(iYear, iMo, iDay,
										buffer[9 + offset],
										buffer[8 + offset],
										buffer[10 + offset],
										millisec);
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed("xml2sql_dns_current_period: invalid data!  "
															+ ex.Message);
						dtStart = DateTime.MinValue;
					}

					dtEnd = DateTime.Now;

					// если не попадает в заданный интервал - пропускаем
					if (dtStart < xmlDNS.End && dtStart != DateTime.MinValue)
					{
						if (dtStart < xmlDNS.Start)
						{
							dtStart = xmlDNS.Start;
							wasEarlier = true;
						}
						else
							wasEarlier = false;

						if (dtEnd > xmlDNS.End) dtEnd = xmlDNS.End;

						if (dtStart <= dtEnd)
						{
							tempSql += string.Format("INSERT INTO dips_and_overs (datetime_id, event_type, phase, deviation, start_datetime, end_datetime, pointer, is_finished, is_earlier, phase_num) VALUES (currval('dips_and_overs_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', {5}, FALSE, {6}, {7});\n",
								typeEvent,
								EmService.GetPhaseName(curPhase),
								value.ToString(new CultureInfo("en-US")),
								dtStart.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
								dtEnd.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
								numEvent,
								wasEarlier.ToString(),
								curPhase
								);
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_dns_current_period:");
				return false;
			}

			return true;
		}

		#endregion

		#region Building SQL query for inserting AVG data from Device

		private bool xml2sql_avg_period(EmXmlAVG xmlAVG, ConnectScheme connection_scheme,
			float fnom, float unomln, float unomph, ref EmSqlDataNode avgNode, long ser_number)
		{
			try
			{
				avgNode.SqlType = EmSqlDataNodeType.AVG;
				avgNode.Begin = xmlAVG.Start;
				avgNode.End = xmlAVG.End;
				byte[] buffer = xmlAVG.DataPages;

				if (buffer == null)
				{
					EmService.WriteToLogFailed("xml2sql_avg_period: NO AVG DATA!");
					return false;
				}

				float u_nom_ph = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 24, true);
				float u_nom_lin = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 28, true);
				float i_nom_ph = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 32, true);
				float f_nom = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 20, true);

				// SQL запрос, который вставит в таблицу folders 
				// папку для года и вернет folder_id этой папки
				avgNode.Sql = string.Format("select insert_year_folder({0}, '{1}');\n", ser_number, 
										xmlAVG.Start.Year.ToString());

				// SQL запрос, который вставит в таблицу folders 
				// папку для месяца и вернет folder_id этой папки
				avgNode.Sql += string.Format("select insert_month_folder({0}, '{1}', '{2}');\n", ser_number, 
											xmlAVG.Start.Year.ToString(), xmlAVG.Start.Month.ToString());

				avgNode.Sql += string.Format("INSERT INTO period_avg_params_times (datetime_id, start_datetime, end_datetime, device_id, period_id, folder_year_id, folder_month_id, f_nom, u_nom_lin, u_nom_ph, i_nom_ph) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9});\n",
							xmlAVG.Start.ToString("MM.dd.yyyy HH:mm:ss"),
							xmlAVG.End.ToString("MM.dd.yyyy HH:mm:ss"),
							"{0}", (int)xmlAVG.AvgType, "{1}", "{2}",
							f_nom.ToString(new CultureInfo("en-US")),
							u_nom_lin.ToString(new CultureInfo("en-US")),
							u_nom_ph.ToString(new CultureInfo("en-US")),
							i_nom_ph.ToString(new CultureInfo("en-US")));
				EmService.WriteToLogGeneral("AVG type = " + (int)xmlAVG.AvgType);

				int archiveCount = buffer.Length / avgArchiveLength_;
				int curShift = 0;
				StringBuilder sbSql = new StringBuilder();

				avgNode.AvgFileName = EmService.GetAvgTmpFileName(
					xmlAVG.AvgType.ToString(), xmlAVG.Start);
				string avgFileFullPath = EmService.TEMP_IMAGE_DIR + avgNode.AvgFileName;
				if (File.Exists(avgFileFullPath)) File.Delete(avgFileFullPath);

				for (int iArch = 0; iArch < archiveCount; ++iArch)
				{
					DateTime event_datetime;
					ushort iYear = (ushort)(buffer[curShift + 44 + 0] + (buffer[curShift + 44 + 1] << 8));
					byte iDay = buffer[curShift + 44 + 2];
					byte iMo = buffer[curShift + 44 + 3];
					byte iHour = buffer[curShift + 44 + 5];
					byte iMin = buffer[curShift + 44 + 4];
					byte iSec = buffer[curShift + 44 + 6];
					try
					{
						event_datetime = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
					}
					catch (ArgumentOutOfRangeException)
					{
						EmService.WriteToLogFailed("Error in xml2sql_avg_period() dates:");
						EmService.WriteToLogFailed(
							string.Format("{0}, {1}, {2}, {3}, {4}, {5}", iYear, iMo, iDay, iHour, iMin, iSec));
						event_datetime = DateTime.MinValue;

						// set ProgressBar position
						cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
						bw_.ReportProgress((int)cur_percent_progress_);

						curShift += avgArchiveLength_;

						continue;
					}

					xml2sql_avg_p1(ref buffer, curShift, connection_scheme, fnom,
											unomln, unomph, event_datetime,
											xmlAVG.MasksAvg.MaskTable_1_4, ref sbSql);
					// гармоники
					xml2sql_avg_p2(ref buffer, curShift, connection_scheme, event_datetime,
											xmlAVG.MasksAvg.MaskTable_5, ref sbSql);
					// углы между гармониками
					xml2sql_avg_p3(ref buffer, curShift, connection_scheme, event_datetime,
											xmlAVG.MasksAvg.MaskTable_6b, ref sbSql);
					// мощности гармоник
					xml2sql_avg_p4(ref buffer, curShift, connection_scheme, event_datetime,
											xmlAVG.MasksAvg.MaskTable_6a, ref sbSql);

					// если набралось слишком много замеров или если это последний цикл
					if (((iArch % 500) == 0 && iArch != 0) ||
						(iArch == archiveCount - 1))
					{
						FileStream fsTmp = null;
						StreamWriter swTmp = null;
						try
						{
							fsTmp = new FileStream(avgFileFullPath, FileMode.Append);
							swTmp = new StreamWriter(fsTmp);
							swTmp.Write(sbSql.ToString());
						}
						finally
						{
							if (swTmp != null) swTmp.Close();
							if (fsTmp != null) fsTmp.Close();
						}

						sbSql = new StringBuilder();
						//GC.Collect();
						EmService.WriteToLogDebug("GC.Collect: " + DateTime.Now.ToString());
					}

					// set ProgressBar position
					cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
					bw_.ReportProgress((int)cur_percent_progress_);

					//avgNode.Sql += sbSql.ToString();

					curShift += avgArchiveLength_;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_period(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		/// <summary>Основные показатели</summary>
		private void xml2sql_avg_p1(ref byte[] buffer, int shift, ConnectScheme connection_scheme,
			float fnom, float unomln, float unomph, DateTime event_datetime,
			Int64[] masksAvg, ref StringBuilder sql)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				// Метод измерения реактивной мощности
				ushort qType = Conversions.bytes_2_ushort(ref buffer, shift + 10); 

				// частота
				float f = Conversions.bytes_2_signed_float_Q_7_24(ref buffer, shift + 64);
				f = (float)Math.Round((double)f, 8);
				// отклонение частоты
				float df = Conversions.bytes_2_signed_float_Q_7_24(ref buffer, shift + 68);
				df = (float)Math.Round((double)df, 8);

				// напряжение прямой последовательности U1
				float u1 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 72, true);
				u1 = (float)Math.Round((double)u1, 8);
				// Напряжение обратной последовательности U2
				float u2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 76, true);
				u2 = (float)Math.Round((double)u2, 8);
				// Напряжение нулевой последовательности U0
				float u0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 80, true);
				u0 = (float)Math.Round((double)u0, 8);
				// Отклонение напряжения dU
				float dU = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 84, true);
				dU = (float)Math.Round((double)dU, 8);
				// Отклонение напряжения dU в процентах
				float UlinNom = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 28, true);
				float dUPer = 0;
				if (UlinNom != 0) dUPer = (dU / UlinNom) * 100;
				dUPer = (float)Math.Round((double)dUPer, 8);
				// Коэффициент обратной последовательности KU2
				float kU2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 88, true) * 100;
				kU2 = (float)Math.Round((double)kU2, 8);
				// Коэффициент нулевой последовательности KU0
				float kU0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 92, true) * 100;
				kU0 = (float)Math.Round((double)kU0, 8);
				// Ток прямой последовательности I1
				float i1 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 96, true);
				i1 = (float)Math.Round((double)i1, 8);
				// Ток обратной последовательности I2
				float i2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 100, true);
				i2 = (float)Math.Round((double)i2, 8);
				// Ток нулевой последовательности I0
				float i0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 104, true);
				i0 = (float)Math.Round((double)i0, 8);
				// Мощность прямой последовательности P1
				float p1 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 108, true);
				p1 = (float)Math.Round((double)p1, 8);
				// Мощность обратной последовательности P2
				float p2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 112, true);
				p2 = (float)Math.Round((double)p2, 8);
				// Мощность нулевой оследовательности P0
				float p0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 116, true);
				p0 = (float)Math.Round((double)p0, 8);
				// Угол между током и напряжением прямой последовательности UI1
				float ui1 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 120);
				ui1 = (float)Math.Round((double)ui1, 8);
				// Угол между током и напряжением обратной последовательности UI2
				float ui2 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 124);
				ui2 = (float)Math.Round((double)ui2, 8);
				// Угол между током и напряжением нулевой последовательности UI0
				float ui0 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 128);
				ui0 = (float)Math.Round((double)ui0, 8);
				// Суммарная активная мощность P
				float p = 0;
				if (connection_scheme != ConnectScheme.Ph3W3 && 
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					p = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 132, true);
				}
				else
					p = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 148, true);
				p = (float)Math.Round((double)p, 8);
				// Суммарная реактивная мощность Q
				float qSum = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 136, true);
				qSum = (float)Math.Round((double)qSum, 8);
				// Суммарная полная мощность S
				float s = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 140, true);
				s = (float)Math.Round((double)s, 8);
				// Суммарный коэффициент мощности Kp
				float Kp = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 144, true);
				Kp = (float)Math.Round((double)Kp, 8);

				// Параметры фазы A
				// Фазное напряжение
				float uA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 192);
				uA = (float)Math.Round((double)uA, 8);
				// Фазное напряжение 1 гармоники
				float u1A = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 196);
				u1A = (float)Math.Round((double)u1A, 8);
				// Фазное напряжение смещения
				float UavA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 200);
				UavA = (float)Math.Round((double)UavA, 8);
				// Отклонение фазного напряжения от номинала
				float dUA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 208) * 100;
				dUA = (float)Math.Round((double)dUA, 8);
				// Линейное напряжение
				float uLA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 212);
				uLA = (float)Math.Round((double)uLA, 8);
				// Линейное напряжение 1 гармоники
				float uL1A = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 216);
				uL1A = (float)Math.Round((double)uL1A, 8);
				// Линейное напряжение смещения
				float uLdA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 220);
				uLdA = (float)Math.Round((double)uLdA, 8);
				// Отклонение линейного напряжения от номинала
				float dULA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 228) * 100;
				dULA = (float)Math.Round((double)dULA, 8);
				// Ток
				float iA = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 232);
				iA = (float)Math.Round((double)iA, 8);
				// Ток 1 гармоники
				float i1A = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 236);
				i1A = (float)Math.Round((double)i1A, 8);
				// Ток смещения
				float IavA = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 240);
				IavA = (float)Math.Round((double)IavA, 8);
				// Отклонение тока от номинала
				float diA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 248) * 100;
				diA = (float)Math.Round((double)diA, 8);
				// Угол между током и напряжением
				float aIUA = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 252);
				aIUA = (float)Math.Round((double)aIUA, 8);
				// Угол напряжения фазный
				float aPhA = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 256);
				aPhA = (float)Math.Round((double)aPhA, 8);
				// Активная мощность
				float apA_AB = 0;
				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					// мощность фазы А
					apA_AB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 260, true);
				}
				else
				{
					// мощность 1-го элемента
					apA_AB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 3280, true);
				}
				apA_AB = (float)Math.Round((double)apA_AB, 8);
				// Реактивная мощность
				float qA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 264, true);
				qA = (float)Math.Round((double)qA, 8);
				// Полная мощность
				float pFullA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 268, true);
				pFullA = (float)Math.Round((double)pFullA, 8);
				// Коэффициент мощности
				float kpA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 272, true);
				kpA = (float)Math.Round((double)kpA, 8);

				// Параметры фазы B
				// Фазное напряжение
				float uB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1216);
				uB = (float)Math.Round((double)uB, 8);
				// Фазное напряжение 1 гармоники
				float u1B = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1220);
				u1B = (float)Math.Round((double)u1B, 8);
				// Фазное напряжение смещения
				float UavB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1224);
				UavB = (float)Math.Round((double)UavB, 8);
				// Отклонение фазного напряжения от номинала
				float dUB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1232) * 100;
				dUB = (float)Math.Round((double)dUB, 8);
				// Линейное напряжение
				float uLB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1236);
				uLB = (float)Math.Round((double)uLB, 8);
				// Линейное напряжение 1 гармоники
				float uL1B = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1240);
				uL1B = (float)Math.Round((double)uL1B, 8);
				// Линейное напряжение смещения
				float uLdB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1244);
				uLdB = (float)Math.Round((double)uLdB, 8);
				// Отклонение линейного напряжения от номинала
				float dULB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1252) * 100;
				dULB = (float)Math.Round((double)dULB, 8);
				// Ток
				float iB = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 1256);
				iB = (float)Math.Round((double)iB, 8);
				// Ток 1 гармоники
				float i1B = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 1260);
				i1B = (float)Math.Round((double)i1B, 8);
				// Ток смещения
				float IavB = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 1264);
				IavB = (float)Math.Round((double)IavB, 8);
				// Отклонение тока от номинала
				float diB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1272) * 100;
				diB = (float)Math.Round((double)diB, 8);
				// Угол между током и напряжением
				float aIUB = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 1276);
				aIUB = (float)Math.Round((double)aIUB, 8);
				// Угол напряжения фазный
				float aPhB = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 1280);
				aPhB = (float)Math.Round((double)aPhB, 8);
				// Активная мощность
				float apB_BC = 0;
				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					// мощность фазы B
					apB_BC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 1284, true);
				}
				else
				{
					// мощность 2-го элемента
					apB_BC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 3284, true);
				}
				apB_BC = (float)Math.Round((double)apB_BC, 8);
				// Реактивная мощность
				float qB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 1288, true);
				qB = (float)Math.Round((double)qB, 8);
				// Полная мощность
				float pFullB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 1292, true);
				pFullB = (float)Math.Round((double)pFullB, 8);
				// Коэффициент мощности
				float kpB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 1296, true);
				kpB = (float)Math.Round((double)kpB, 8);

				// Параметры фазы C
				// Фазное напряжение
				float uC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2240);
				uC = (float)Math.Round((double)uC, 8);
				// Фазное напряжение 1 гармоники
				float u1C = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2244);
				u1C = (float)Math.Round((double)u1C, 8);
				// Фазное напряжение смещения
				float UavC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2248);
				UavC = (float)Math.Round((double)UavC, 8);
				// Отклонение фазного напряжения от номинала
				float dUC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2256) * 100;
				dUC = (float)Math.Round((double)dUC, 8);
				// Линейное напряжение
				float uLC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2260);
				uLC = (float)Math.Round((double)uLC, 8);
				// Линейное напряжение 1 гармоники
				float uL1C = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2264);
				uL1C = (float)Math.Round((double)uL1C, 8);
				// Линейное напряжение смещения
				float uLdC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2268);
				uLdC = (float)Math.Round((double)uLdC, 8);
				// Отклонение линейного напряжения от номинала
				float dULC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2276) * 100;
				dULC = (float)Math.Round((double)dULC, 8);
				// Ток
				float iC = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 2280);
				iC = (float)Math.Round((double)iC, 8);
				// Ток 1 гармоники
				float i1C = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 2284);
				i1C = (float)Math.Round((double)i1C, 8);
				// Ток смещения
				float IavC = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 2288);
				IavC = (float)Math.Round((double)IavC, 8);
				// Отклонение тока от номинала
				float diC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2296) * 100;
				diC = (float)Math.Round((double)diC, 8);
				// Угол между током и напряжением
				float aIUC = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 2300);
				aIUC = (float)Math.Round((double)aIUC, 8);
				// Угол напряжения фазный
				float aPhC = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 2304);
				aPhC = (float)Math.Round((double)aPhC, 8);
				// Активная мощность
				float apC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 2308, true);
				apC = (float)Math.Round((double)apC, 8);
				// Реактивная мощность
				float qC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 2312, true);
				qC = (float)Math.Round((double)qC, 8);
				// Полная мощность
				float pFullC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 2316, true);
				pFullC = (float)Math.Round((double)pFullC, 8);
				// Коэффициент мощности
				float kpC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 2320, true);
				kpC = (float)Math.Round((double)kpC, 8);

				// Коэффициенты гармоник фазного напряжения
				float KuPhA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 276) * 100;
				KuPhA = (float)Math.Round((double)KuPhA, 8);
				float KuPhB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1300) * 100;
				KuPhB = (float)Math.Round((double)KuPhB, 8);
				float KuPhC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2324) * 100;
				KuPhC = (float)Math.Round((double)KuPhC, 8);

				// Коэффициенты гармоник линейного напряжения
				float KuLinA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 436) * 100;
				KuLinA = (float)Math.Round((double)KuLinA, 8);
				float KuLinB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1460) * 100;
				KuLinB = (float)Math.Round((double)KuLinB, 8);
				float KuLinC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2484) * 100;
				KuLinC = (float)Math.Round((double)KuLinC, 8);

				// Коэффициенты гармоник тока
				float KiA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 596) * 100;
				KiA = (float)Math.Round((double)KiA, 8);
				float KiB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1620) * 100;
				KiB = (float)Math.Round((double)KiB, 8);
				float KiC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2644) * 100;
				KiC = (float)Math.Round((double)KiC, 8);

				// tangent P
				float tanP = 0;
				if (Math.Abs(p) >= 0.05 && Math.Abs(qSum) >= 0.05)
				{
					tanP = (p == 0 ? 0 : qSum / p);
					tanP = (float)Math.Round((double)tanP, 8);
				}

				switch (connection_scheme)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						sql.Append(String.Format
							("INSERT INTO period_avg_params_1_4 VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53}, {54}, {55}, {56}, {57}, {58}, {59}, {60}, {61}, {62}, {63}, {64}, {65}, {66}, {67}, {68}, {69}, {70}, {71}, {72}, {73}, {74}, {75}, {76}, {77}, {78}, {79}, {80});\n",
							"currval('period_avg_params_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f.ToString(ci),	  // f

							uA.ToString(ci),  // U_A
							uB.ToString(ci),  // U_B
							uC.ToString(ci),  // U_C

							u1A.ToString(ci),	// U1_A
							u1B.ToString(ci),	// U1_B
							u1C.ToString(ci),	// U1_C

							uLA.ToString(ci),	// U_AB
							uLB.ToString(ci),	// U_BC
							uLC.ToString(ci),	// U_CA

							uL1A.ToString(ci),	// U1_AB
							uL1B.ToString(ci),	// U1_BC
							uL1C.ToString(ci),	// U1_CA

							UavA.ToString(ci),	// U0_A* old???? средневыпрямленное
							UavB.ToString(ci),	// U0_B
							UavC.ToString(ci),	// U0_C

							UavA.ToString(ci),	// U_hp A  0-ая гармоника
							UavB.ToString(ci),	// U_hp B
							UavC.ToString(ci),	// U_hp C

							u1.ToString(ci),	// U_1 = U_у
							u2.ToString(ci),	// U_2
							u0.ToString(ci),	// U_0

							iA.ToString(ci),	// I_A
							iB.ToString(ci),	// I_B
							iC.ToString(ci),	// I_C

							i1A.ToString(ci),	// I1_A
							i1B.ToString(ci),	// I1_B
							i1C.ToString(ci),	// I1_C

							i1.ToString(ci),	// I_1
							i2.ToString(ci),	// I_2
							i0.ToString(ci),	// I_0

							p.ToString(ci),		// P_Σ
							apA_AB.ToString(ci),	// P_A(1)
							apB_BC.ToString(ci),	// P_B(2)
							apC.ToString(ci),	// P_C

							s.ToString(ci),			// S_Σ
							pFullA.ToString(ci),	// S_A
							pFullB.ToString(ci),	// S_B
							pFullC.ToString(ci),	// S_C

							qSum.ToString(ci),	// Q_Sum
							qA.ToString(ci),	// Q_A
							qB.ToString(ci),	// Q_B
							qC.ToString(ci),	// Q_C

							Kp.ToString(ci),	// Kp_Σ
							kpA.ToString(ci),	// Kp_A
							kpB.ToString(ci),	// Kp_B
							kpC.ToString(ci),	// Kp_C

							aPhA.ToString(ci),		// ∟U1_A_ U1_B
							aPhB.ToString(ci),		// ∟U1_B_ U1_C
							aPhC.ToString(ci),		// ∟U1_C_ U1_A

							aIUA.ToString(ci),		// ∟U1_A_ I1_A
							aIUB.ToString(ci),		// ∟U1_B_ I1_B	
							aIUC.ToString(ci),		// ∟U1_C_ I1_C

							ui1.ToString(ci),		// ∟U_1_ I_1
							ui2.ToString(ci),		// ∟U_2_ I_2	
							ui0.ToString(ci),		// ∟U_0_ I_0

							p0.ToString(ci),		//P_0
							p1.ToString(ci),		//P_1
							p2.ToString(ci),		//P_2

							df.ToString(ci),	// ∆f = f - f_ном
							//(f - fnom).ToString(ci),

							// δU_y = (U_y - U_ном.лин.) * 100% / U_ном.лин.
							dUPer.ToString(ci),
							//dU.ToString(ci),
							// δU_A = (U1_A - U_ном.фаз.) * 100% / U_ном.фаз.
							dUA.ToString(ci),
							// δU_B = (U1_B - U_ном.фаз.) * 100% / U_ном.фаз.
							dUB.ToString(ci),
							// δU_C = (U1_C - U_ном.фаз.) * 100% / U_ном.фаз.
							dUC.ToString(ci),
							// δU_AB = (U1_AB - U_ном.лин.) * 100% / U_ном.лин.
							dULA.ToString(ci),
							// δU_BC = (U1_BC - U_ном.лин.) * 100% / U_ном.лин.
							dULB.ToString(ci),
							// δU_CA = (U1_CA - U_ном.лин.) * 100% / U_ном.лин.
							dULC.ToString(ci),

							kU2.ToString(ci),	// K_2U
							kU0.ToString(ci),	// K_0U

							KuPhA.ToString(ci),	// K_UA(AB)
							KuPhB.ToString(ci),	// K_UB(BC)
							KuPhC.ToString(ci),	// K_UC(CA)

							KiA.ToString(ci),	// K_IA
							KiB.ToString(ci),	// K_IB
							KiC.ToString(ci),	// K_IC
                            qType.ToString(),  // метод измерения реактивной мощности
							masksAvg[0], masksAvg[1],
							tanP.ToString(ci)		// tangent P
							));
						break;

					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						sql.Append(String.Format("INSERT INTO period_avg_params_1_4 (datetime_id, event_datetime, f, u_ab, u_bc, u_ca, u1_ab, u1_bc, u1_ca, u_1, u_2, i_a, i_b, i_c, i1_a, i1_b, i1_c, i_1, i_2, p_sum, p_a_1, p_b_2, s_sum, q_sum, q_a, q_b, q_c, kp_sum, an_u1_a_u1_b, an_u1_b_u1_c, an_u1_c_u1_a, an_u1_a_i1_a, an_u1_b_i1_b, an_u1_c_i1_c, an_u_1_i_1, an_u_2_i_2, p_1, p_2, d_f, d_u_y, d_u_ab, d_u_bc, d_u_ca, k_2u, k_ua_ab, k_ub_bc, k_uc_ca, k_ia, k_ib, k_ic, q_type, mask1, mask2, tangent_p) VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53});\n",
							"currval('period_avg_params_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f.ToString(ci),	// f

							uLA.ToString(ci),	// U_AB
							uLB.ToString(ci),	// U_BC
							uLC.ToString(ci),	// U_CA

							uL1A.ToString(ci),	// U1_AB
							uL1B.ToString(ci),	// U1_BC
							uL1C.ToString(ci),	// U1_CA	

							u1.ToString(ci),	// U_1 = U_у
							u2.ToString(ci),	// U_2

							iA.ToString(ci),	// I_A
							iB.ToString(ci),	// I_B
							iC.ToString(ci),	// I_C	  

							i1A.ToString(ci),	// I1_A
							i1B.ToString(ci),	// I1_B
							i1C.ToString(ci),	// I1_C

							i1.ToString(ci),	// I_1
							i2.ToString(ci),	// I_2

							p.ToString(ci),		// P_Σ
							apA_AB.ToString(ci),	// P_A(1)
							apB_BC.ToString(ci),	// P_B(2)
							s.ToString(ci),		// S_Σ
							qSum.ToString(ci),	// Q_Σ
							qA.ToString(ci),	// Q_A
							qB.ToString(ci),	// Q_B
							qC.ToString(ci),	// Q_C

							Kp.ToString(ci),	// Kp_Σ

							aPhA.ToString(ci),		// ∟U1_A_ U1_B
							aPhB.ToString(ci),		// ∟U1_B_ U1_C
							aPhC.ToString(ci),		// ∟U1_C_ U1_A

							aIUA.ToString(ci),		// ∟U1_A_ I1_A
							aIUB.ToString(ci),		// ∟U1_B_ I1_B	
							aIUC.ToString(ci),		// ∟U1_C_ I1_C

							ui1.ToString(ci),		// ∟U_1_ I_1
							ui2.ToString(ci),		// ∟U_2_ I_2

							p1.ToString(ci),		//P_1
							p2.ToString(ci),		//P_2

							df.ToString(ci),	// ∆f = f - f_ном
							//(f - fnom).ToString(ci),
							// δU_y = (U_y - U_ном.лин.) * 100% / U_ном.лин.
							dUPer.ToString(ci),
							//dU.ToString(ci),
							// δU_AB = (U1_AB - U_ном.лин.) * 100% / U_ном.лин.
							dULA.ToString(ci),
							// δU_BC = (U1_BC - U_ном.лин.) * 100% / U_ном.лин.
							dULB.ToString(ci),
							// δU_CA = (U1_CA - U_ном.лин.) * 100% / U_ном.лин.
							dULC.ToString(ci),

							kU2.ToString(ci),	// K_2U
							KuLinA.ToString(ci),	// K_UA(AB)
							KuLinB.ToString(ci),	// K_UB(BC)
							KuLinC.ToString(ci),	// K_UC(CA)

							KiA.ToString(ci),	// K_IA
							KiB.ToString(ci),	// K_IB
							KiC.ToString(ci),	// K_IC
                            qType.ToString(),  // метод измерения реактивной мощности
							masksAvg[0], masksAvg[1],
							tanP.ToString(ci)	// tangent P
							));
						break;

					case ConnectScheme.Ph1W2:
						sql.Append(String.Format
							("INSERT INTO period_avg_params_1_4 (datetime_id, event_datetime, f, u_a, u1_a, u0_a, u_hp_a, u_1, i_a, i1_a, i_1, p_a_1, s_a, q_a, kp_a, an_u1_a_i1_a, an_u_1_i_1, p_1, d_f, d_u_a, k_ua_ab, k_ia, q_type, mask1, mask2, tangent_p) VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25});\n",
							"currval('period_avg_params_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
							f.ToString(ci),	// f

							uA.ToString(ci),  // U_A
							u1A.ToString(ci),	// U1_A
							UavA.ToString(ci),	// U0_A* old???? средневыпрямленное

							UavA.ToString(ci),	// U_hp A  0-ая гармоника
							u1.ToString(ci),	// U_1 = U_у

							iA.ToString(ci),	// I_A
							i1A.ToString(ci),	// I1_A
							i1.ToString(ci),	// I_1

							apA_AB.ToString(ci),	// P_A(1)
							pFullA.ToString(ci),	// S_A

							qA.ToString(ci),	// Q_A

							kpA.ToString(ci),	// Kp_A

							aIUA.ToString(ci),		// ∟U1_A_ I1_A
							ui1.ToString(ci),		// ∟U_1_ I_1

							p1.ToString(ci),		//P_1

							df.ToString(ci),	// ∆f = f - f_ном
							//(f - fnom).ToString(ci),
							// δU_A = (U1_A - U_ном.фаз.) * 100% / U_ном.фаз.
							dUA.ToString(ci),	// здесь уже в процентах
							KuPhA.ToString(ci),	// K_UA(AB)
							KiA.ToString(ci),	// K_IA
                            qType.ToString(),  // метод измерения реактивной мощности
							masksAvg[0], masksAvg[1],
							tanP.ToString(ci)	// tangent P
							));
						break;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in xml2sql_avg_p1(): " + ex.Message);
				throw;
			}
		}

		/// <summary>Гармоники</summary>
		private void xml2sql_avg_p2(ref byte[] buffer, int shift, ConnectScheme connection_scheme,
									DateTime event_datetime, Int64[] masksAvg, ref StringBuilder sql)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				// Фазное напряжение 1 гармоники
				float u1A = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 196);
				u1A = (float)Math.Round((double)u1A, 8);
				float u1B = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1220);
				u1B = (float)Math.Round((double)u1B, 8);
				float u1C = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2244);
				u1C = (float)Math.Round((double)u1C, 8);

				// Линейное напряжение 1 гармоники
				float uL1A = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 216);
				uL1A = (float)Math.Round((double)uL1A, 8);
				float uL1B = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 1240);
				uL1B = (float)Math.Round((double)uL1B, 8);
				float uL1C = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, shift + 2264);
				uL1C = (float)Math.Round((double)uL1C, 8);

				// Ток 1 гармоники
				float i1A = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 236);
				i1A = (float)Math.Round((double)i1A, 8);
				float i1B = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 1260);
				i1B = (float)Math.Round((double)i1B, 8);
				float i1C = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, shift + 2284);
				i1C = (float)Math.Round((double)i1C, 8);

				// Коэффициенты гармоник фазного напряжения
				float[] kPhUA = new float[39];
				float[] kPhUB = new float[39];
				float[] kPhUC = new float[39];
				//float KuPhA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 276) * 100;
				//KuPhA = (float)Math.Round((double)KuPhA, 8);
				//float KuPhB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1300) * 100;
				//KuPhB = (float)Math.Round((double)KuPhB, 8);
				//float KuPhC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2324) * 100;
				//KuPhC = (float)Math.Round((double)KuPhC, 8);

				for (int i = 0; i < 39; ++i)
				{
					kPhUA[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 280 + i * 4) * 100;
					kPhUA[i] = (float)Math.Round((double)kPhUA[i], 8);
					kPhUB[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1304 + i * 4) * 100;
					kPhUB[i] = (float)Math.Round((double)kPhUB[i], 8);
					kPhUC[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2328 + i * 4) * 100;
					kPhUC[i] = (float)Math.Round((double)kPhUC[i], 8);
				}
				// Коэффициенты гармоник линейного напряжения
				float[] kPLUA = new float[39];
				float[] kPLUB = new float[39];
				float[] kPLUC = new float[39];
				//float KuLinA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 436) * 100;
				//KuLinA = (float)Math.Round((double)KuLinA, 8);
				//float KuLinB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1460) * 100;
				//KuLinB = (float)Math.Round((double)KuLinB, 8);
				//float KuLinC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2484) * 100;
				//KuLinC = (float)Math.Round((double)KuLinC, 8);

				for (int i = 0; i < 39; ++i)
				{
					kPLUA[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 440 + i * 4) * 100;
					kPLUA[i] = (float)Math.Round((double)kPLUA[i], 8);
					kPLUB[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1464 + i * 4) * 100;
					kPLUB[i] = (float)Math.Round((double)kPLUB[i], 8);
					kPLUC[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2488 + i * 4) * 100;
					kPLUC[i] = (float)Math.Round((double)kPLUC[i], 8);
				}
				// Коэффициенты гармоник тока
				float[] kIA = new float[39];
				float[] kIB = new float[39];
				float[] kIC = new float[39];
				//float KiA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 596) * 100;
				//KiA = (float)Math.Round((double)KiA, 8);
				//float KiB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1620) * 100;
				//KiB = (float)Math.Round((double)KiB, 8);
				//float KiC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2644) * 100;
				//KiC = (float)Math.Round((double)KiC, 8);

				for (int i = 0; i < 39; ++i)
				{
					kIA[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 600 + i * 4) * 100;
					kIA[i] = (float)Math.Round((double)kIA[i], 8);
					kIB[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 1624 + i * 4) * 100;
					kIB[i] = (float)Math.Round((double)kIB[i], 8);
					kIC[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, shift + 2648 + i * 4) * 100;
					kIC[i] = (float)Math.Round((double)kIC[i], 8);
				}

				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					sb.Append("INSERT INTO period_avg_params_5(datetime_id, event_datetime, u1_a, k_ua_2, k_ua_3, k_ua_4, k_ua_5, k_ua_6, k_ua_7, k_ua_8, k_ua_9, k_ua_10, k_ua_11, k_ua_12, k_ua_13, k_ua_14, k_ua_15, k_ua_16, k_ua_17, k_ua_18, k_ua_19, k_ua_20, k_ua_21, k_ua_22, k_ua_23, k_ua_24, k_ua_25, k_ua_26, k_ua_27, k_ua_28, k_ua_29, k_ua_30, k_ua_31, k_ua_32, k_ua_33, k_ua_34, k_ua_35, k_ua_36, k_ua_37, k_ua_38, k_ua_39, k_ua_40, u1_b, k_ub_2, k_ub_3, k_ub_4, k_ub_5, k_ub_6, k_ub_7, k_ub_8, k_ub_9, k_ub_10, k_ub_11, k_ub_12, k_ub_13, k_ub_14, k_ub_15, k_ub_16, k_ub_17, k_ub_18, k_ub_19, k_ub_20, k_ub_21, k_ub_22, k_ub_23, k_ub_24, k_ub_25, k_ub_26, k_ub_27, k_ub_28, k_ub_29, k_ub_30, k_ub_31, k_ub_32, k_ub_33, k_ub_34, k_ub_35, k_ub_36, k_ub_37, k_ub_38, k_ub_39, k_ub_40, u1_c, k_uc_2, k_uc_3, k_uc_4, k_uc_5, k_uc_6, k_uc_7, k_uc_8, k_uc_9, k_uc_10, k_uc_11, k_uc_12, k_uc_13, k_uc_14, k_uc_15, k_uc_16, k_uc_17, k_uc_18, k_uc_19, k_uc_20, k_uc_21, k_uc_22, k_uc_23, k_uc_24, k_uc_25, k_uc_26, k_uc_27, k_uc_28, k_uc_29, k_uc_30, k_uc_31, k_uc_32, k_uc_33, k_uc_34, k_uc_35, k_uc_36, k_uc_37, k_uc_38, k_uc_39, k_uc_40, i1_a, k_ia_2, k_ia_3, k_ia_4, k_ia_5, k_ia_6, k_ia_7, k_ia_8, k_ia_9, k_ia_10, k_ia_11, k_ia_12, k_ia_13, k_ia_14, k_ia_15, k_ia_16, k_ia_17, k_ia_18, k_ia_19, k_ia_20, k_ia_21, k_ia_22, k_ia_23, k_ia_24, k_ia_25, k_ia_26, k_ia_27, k_ia_28, k_ia_29, k_ia_30, k_ia_31, k_ia_32, k_ia_33, k_ia_34, k_ia_35, k_ia_36, k_ia_37, k_ia_38, k_ia_39, k_ia_40, i1_b, k_ib_2, k_ib_3, k_ib_4, k_ib_5, k_ib_6, k_ib_7, k_ib_8, k_ib_9, k_ib_10, k_ib_11, k_ib_12, k_ib_13, k_ib_14, k_ib_15, k_ib_16, k_ib_17, k_ib_18, k_ib_19, k_ib_20, k_ib_21, k_ib_22, k_ib_23, k_ib_24, k_ib_25, k_ib_26, k_ib_27, k_ib_28, k_ib_29, k_ib_30, k_ib_31, k_ib_32, k_ib_33, k_ib_34, k_ib_35, k_ib_36, k_ib_37, k_ib_38, k_ib_39, k_ib_40, i1_c, k_ic_2, k_ic_3, k_ic_4, k_ic_5, k_ic_6, k_ic_7, k_ic_8, k_ic_9, k_ic_10, k_ic_11, k_ic_12, k_ic_13, k_ic_14, k_ic_15, k_ic_16, k_ic_17, k_ic_18, k_ic_19, k_ic_20, k_ic_21, k_ic_22, k_ic_23, k_ic_24, k_ic_25, k_ic_26, k_ic_27, k_ic_28, k_ic_29, k_ic_30, k_ic_31, k_ic_32, k_ic_33, k_ic_34, k_ic_35, k_ic_36, k_ic_37, k_ic_38, k_ic_39, k_ic_40, u1_ab, k_uab_2, k_uab_3, k_uab_4, k_uab_5, k_uab_6, k_uab_7, k_uab_8, k_uab_9, k_uab_10, k_uab_11, k_uab_12, k_uab_13, k_uab_14, k_uab_15, k_uab_16, k_uab_17, k_uab_18, k_uab_19, k_uab_20, k_uab_21, k_uab_22, k_uab_23, k_uab_24, k_uab_25, k_uab_26, k_uab_27, k_uab_28, k_uab_29, k_uab_30, k_uab_31, k_uab_32, k_uab_33, k_uab_34, k_uab_35, k_uab_36, k_uab_37, k_uab_38, k_uab_39, k_uab_40, u1_bc, k_ubc_2, k_ubc_3, k_ubc_4, k_ubc_5, k_ubc_6, k_ubc_7, k_ubc_8, k_ubc_9, k_ubc_10, k_ubc_11, k_ubc_12, k_ubc_13, k_ubc_14, k_ubc_15, k_ubc_16, k_ubc_17, k_ubc_18, k_ubc_19, k_ubc_20, k_ubc_21, k_ubc_22, k_ubc_23, k_ubc_24, k_ubc_25, k_ubc_26, k_ubc_27, k_ubc_28, k_ubc_29, k_ubc_30, k_ubc_31, k_ubc_32, k_ubc_33, k_ubc_34, k_ubc_35, k_ubc_36, k_ubc_37, k_ubc_38, k_ubc_39, k_ubc_40, u1_ca, k_uca_2, k_uca_3, k_uca_4, k_uca_5, k_uca_6, k_uca_7, k_uca_8, k_uca_9, k_uca_10, k_uca_11, k_uca_12, k_uca_13, k_uca_14, k_uca_15, k_uca_16, k_uca_17, k_uca_18, k_uca_19, k_uca_20, k_uca_21, k_uca_22, k_uca_23, k_uca_24, k_uca_25, k_uca_26, k_uca_27, k_uca_28, k_uca_29, k_uca_30, k_uca_31, k_uca_32, k_uca_33, k_uca_34, k_uca_35, k_uca_36, k_uca_37, k_uca_38, k_uca_39, k_uca_40, mask1, mask2) VALUES (");
				}
				else if (connection_scheme == ConnectScheme.Ph3W3 ||
						connection_scheme == ConnectScheme.Ph3W3_B_calc)
				{
					sb.Append("INSERT INTO period_avg_params_5 (datetime_id, event_datetime, i1_a, k_ia_2, k_ia_3, k_ia_4, k_ia_5, k_ia_6, k_ia_7, k_ia_8, k_ia_9, k_ia_10, k_ia_11, k_ia_12, k_ia_13, k_ia_14, k_ia_15, k_ia_16, k_ia_17, k_ia_18, k_ia_19, k_ia_20, k_ia_21, k_ia_22, k_ia_23, k_ia_24, k_ia_25, k_ia_26, k_ia_27, k_ia_28, k_ia_29, k_ia_30, k_ia_31, k_ia_32, k_ia_33, k_ia_34, k_ia_35, k_ia_36, k_ia_37, k_ia_38, k_ia_39, k_ia_40, i1_b, k_ib_2, k_ib_3, k_ib_4, k_ib_5, k_ib_6, k_ib_7, k_ib_8, k_ib_9, k_ib_10, k_ib_11, k_ib_12, k_ib_13, k_ib_14, k_ib_15, k_ib_16, k_ib_17, k_ib_18, k_ib_19, k_ib_20, k_ib_21, k_ib_22, k_ib_23, k_ib_24, k_ib_25, k_ib_26, k_ib_27, k_ib_28, k_ib_29, k_ib_30, k_ib_31, k_ib_32, k_ib_33, k_ib_34, k_ib_35, k_ib_36, k_ib_37, k_ib_38, k_ib_39, k_ib_40, i1_c, k_ic_2, k_ic_3, k_ic_4, k_ic_5, k_ic_6, k_ic_7, k_ic_8, k_ic_9, k_ic_10, k_ic_11, k_ic_12, k_ic_13, k_ic_14, k_ic_15, k_ic_16, k_ic_17, k_ic_18, k_ic_19, k_ic_20, k_ic_21, k_ic_22, k_ic_23, k_ic_24, k_ic_25, k_ic_26, k_ic_27, k_ic_28, k_ic_29, k_ic_30, k_ic_31, k_ic_32, k_ic_33, k_ic_34, k_ic_35, k_ic_36, k_ic_37, k_ic_38, k_ic_39, k_ic_40, u1_ab, k_uab_2, k_uab_3, k_uab_4, k_uab_5, k_uab_6, k_uab_7, k_uab_8, k_uab_9, k_uab_10, k_uab_11, k_uab_12, k_uab_13, k_uab_14, k_uab_15, k_uab_16, k_uab_17, k_uab_18, k_uab_19, k_uab_20, k_uab_21, k_uab_22, k_uab_23, k_uab_24, k_uab_25, k_uab_26, k_uab_27, k_uab_28, k_uab_29, k_uab_30, k_uab_31, k_uab_32, k_uab_33, k_uab_34, k_uab_35, k_uab_36, k_uab_37, k_uab_38, k_uab_39, k_uab_40, u1_bc, k_ubc_2, k_ubc_3, k_ubc_4, k_ubc_5, k_ubc_6, k_ubc_7, k_ubc_8, k_ubc_9, k_ubc_10, k_ubc_11, k_ubc_12, k_ubc_13, k_ubc_14, k_ubc_15, k_ubc_16, k_ubc_17, k_ubc_18, k_ubc_19, k_ubc_20, k_ubc_21, k_ubc_22, k_ubc_23, k_ubc_24, k_ubc_25, k_ubc_26, k_ubc_27, k_ubc_28, k_ubc_29, k_ubc_30, k_ubc_31, k_ubc_32, k_ubc_33, k_ubc_34, k_ubc_35, k_ubc_36, k_ubc_37, k_ubc_38, k_ubc_39, k_ubc_40, u1_ca, k_uca_2, k_uca_3, k_uca_4, k_uca_5, k_uca_6, k_uca_7, k_uca_8, k_uca_9, k_uca_10, k_uca_11, k_uca_12, k_uca_13, k_uca_14, k_uca_15, k_uca_16, k_uca_17, k_uca_18, k_uca_19, k_uca_20, k_uca_21, k_uca_22, k_uca_23, k_uca_24, k_uca_25, k_uca_26, k_uca_27, k_uca_28, k_uca_29, k_uca_30, k_uca_31, k_uca_32, k_uca_33, k_uca_34, k_uca_35, k_uca_36, k_uca_37, k_uca_38, k_uca_39, k_uca_40, mask1, mask2) VALUES (");
				}
				else if (connection_scheme == ConnectScheme.Ph1W2)
				{
					sb.Append("INSERT INTO period_avg_params_5 (datetime_id, event_datetime, u1_a, k_ua_2, k_ua_3, k_ua_4, k_ua_5, k_ua_6, k_ua_7, k_ua_8, k_ua_9, k_ua_10, k_ua_11, k_ua_12, k_ua_13, k_ua_14, k_ua_15, k_ua_16, k_ua_17, k_ua_18, k_ua_19, k_ua_20, k_ua_21, k_ua_22, k_ua_23, k_ua_24, k_ua_25, k_ua_26, k_ua_27, k_ua_28, k_ua_29, k_ua_30, k_ua_31, k_ua_32, k_ua_33, k_ua_34, k_ua_35, k_ua_36, k_ua_37, k_ua_38, k_ua_39, k_ua_40, i1_a, k_ia_2, k_ia_3, k_ia_4, k_ia_5, k_ia_6, k_ia_7, k_ia_8, k_ia_9, k_ia_10, k_ia_11, k_ia_12, k_ia_13, k_ia_14, k_ia_15, k_ia_16, k_ia_17, k_ia_18, k_ia_19, k_ia_20, k_ia_21, k_ia_22, k_ia_23, k_ia_24, k_ia_25, k_ia_26, k_ia_27, k_ia_28, k_ia_29, k_ia_30, k_ia_31, k_ia_32, k_ia_33, k_ia_34, k_ia_35, k_ia_36, k_ia_37, k_ia_38, k_ia_39, k_ia_40, mask1, mask2) VALUES (");
				}

				// adding pkey
				sb.Append("currval('period_avg_params_times_datetime_id_seq'), '");
				sb.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					// гармоники фазного напряжения
					sb.Append(u1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UA(AB)_2..K_UA(AB)_40
						sb.Append(kPhUA[i].ToString(ci) + ", ");

					sb.Append(u1B.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UB(BC)_2..K_UB(BC)_40
						sb.Append(kPhUB[i].ToString(ci) + ", ");

					sb.Append(u1C.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UC(CA)_2..K_UC(CA)_40
						sb.Append(kPhUC[i].ToString(ci) + ", ");

					// гармоники тока
					sb.Append(i1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IA_2..K_IA_40
						sb.Append(kIA[i].ToString(ci) + ", ");

					sb.Append(i1B.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IB_2..K_IB_40
						sb.Append(kIB[i].ToString(ci) + ", ");

					sb.Append(i1C.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IC_2..K_IC_40
						sb.Append(kIC[i].ToString(ci) + ", ");

					// гармоники линейного напряжения
					sb.Append(uL1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UAB_2..K_UAB_40
						sb.Append(kPLUA[i].ToString(ci) + ", ");

					sb.Append(uL1B.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UBC_2..K_UBC_40
						sb.Append(kPLUB[i].ToString(ci) + ", ");

					sb.Append(uL1C.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UCA_2..K_UCA_40
						sb.Append(kPLUC[i].ToString(ci) + ", ");
				}

				if (connection_scheme == ConnectScheme.Ph3W3 ||
					connection_scheme == ConnectScheme.Ph3W3_B_calc)
				{
					// гармоники тока
					sb.Append(i1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IA_2..K_IA_40
						sb.Append(kIA[i].ToString(ci) + ", ");

					sb.Append(i1B.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IB_2..K_IB_40
						sb.Append(kIB[i].ToString(ci) + ", ");

					sb.Append(i1C.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IC_2..K_IC_40
						sb.Append(kIC[i].ToString(ci) + ", ");

					// гармоники линейного напряжения
					sb.Append(uL1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UAB_2..K_UAB_40
						sb.Append(kPLUA[i].ToString(ci) + ", ");

					sb.Append(uL1B.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UBC_2..K_UBC_40
						sb.Append(kPLUB[i].ToString(ci) + ", ");

					sb.Append(uL1C.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UCA_2..K_UCA_40
						sb.Append(kPLUC[i].ToString(ci) + ", ");
				}

				if (connection_scheme == ConnectScheme.Ph1W2)
				{
					// гармоники фазного напряжения
					sb.Append(u1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UA(AB)_2..K_UA(AB)_40
						sb.Append(kPhUA[i].ToString(ci) + ", ");

					// гармоники тока
					sb.Append(i1A.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IA_2..K_IA_40
						sb.Append(kIA[i].ToString(ci) + ", ");
				}

				sb.Append(masksAvg[0].ToString() + ", ");
				sb.Append(masksAvg[1].ToString());
				sb.Append(");\n");
				sql.Append(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in xml2sql_avg_p2(): " + ex.Message);
				throw;
			}
		}

		/// <summary>Углы между гармониками</summary>
		private void xml2sql_avg_p3(ref byte[] buffer, int shift, ConnectScheme connection_scheme,
									DateTime event_datetime, Int64[] masksAvg, ref StringBuilder sql)
		{
			try
			{
				//if (connection_scheme == 2)  // для 3ф3пр этих данных нет
				//	return;

				// Углы мощностей гармоник
				float[] aphA = new float[40];
				float[] aphB = new float[40];
				float[] aphC = new float[40];

				for (int i = 0; i < 40; ++i)
				{
					aphA[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 916 + i * 4);
					aphA[i] = (float)Math.Round((double)aphA[i], 8);
					aphB[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 1940 + i * 4);
					aphB[i] = (float)Math.Round((double)aphB[i], 8);
					aphC[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, shift + 2964 + i * 4);
					aphC[i] = (float)Math.Round((double)aphC[i], 8);
				}

				CultureInfo ci = new CultureInfo("en-US");

				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb = new System.Text.StringBuilder();
				if (connection_scheme != ConnectScheme.Ph1W2)
					sb.Append("INSERT INTO period_avg_params_6b VALUES (");
				else
					sb.Append("INSERT INTO period_avg_params_6b (datetime_id, event_datetime, an_u_a_1_i_a_1, an_u_a_2_i_a_2, an_u_a_3_i_a_3, an_u_a_4_i_a_4, an_u_a_5_i_a_5, an_u_a_6_i_a_6, an_u_a_7_i_a_7, an_u_a_8_i_a_8, an_u_a_9_i_a_9, an_u_a_10_i_a_10, an_u_a_11_i_a_11, an_u_a_12_i_a_12, an_u_a_13_i_a_13, an_u_a_14_i_a_14, an_u_a_15_i_a_15, an_u_a_16_i_a_16, an_u_a_17_i_a_17, an_u_a_18_i_a_18, an_u_a_19_i_a_19, an_u_a_20_i_a_20, an_u_a_21_i_a_21, an_u_a_22_i_a_22, an_u_a_23_i_a_23, an_u_a_24_i_a_24, an_u_a_25_i_a_25, an_u_a_26_i_a_26, an_u_a_27_i_a_27, an_u_a_28_i_a_28, an_u_a_29_i_a_29, an_u_a_30_i_a_30, an_u_a_31_i_a_31, an_u_a_32_i_a_32, an_u_a_33_i_a_33, an_u_a_34_i_a_34, an_u_a_35_i_a_35, an_u_a_36_i_a_36, an_u_a_37_i_a_37, an_u_a_38_i_a_38, an_u_a_39_i_a_39, an_u_a_40_i_a_40, mask1, mask2) VALUES (");

				// adding pkey
				sb.Append("currval('period_avg_params_times_datetime_id_seq'), '");
				sb.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

				for (int i = 0; i < 40; i++)	// ∟U_A 1_ I_A 1..∟U_A 1_ I_A 40
					sb.Append(aphA[i].ToString(ci) + ", ");

				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					for (int i = 0; i < 40; i++)	// ∟U_B 1_ I_B 1..∟UBA 1_ I_B 40
						sb.Append(aphB[i].ToString(ci) + ", ");
					for (int i = 0; i < 40; i++)	// ∟U_C 1_ I_C 1..∟U_C 1_ I_C 40
						sb.Append(aphC[i].ToString(ci) + ", ");
				}

				sb.Append(masksAvg[0].ToString() + ", ");
				sb.Append(masksAvg[1].ToString());
				sb.Append(");\n");
				sql.Append(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in xml2sql_avg_p3(): " + ex.Message);
				throw;
			}
		}

		/// <summary>Мощности гармоник</summary>
		private void xml2sql_avg_p4(ref byte[] buffer, int shift, ConnectScheme connection_scheme,
									DateTime event_datetime, Int64[] masksAvg, ref StringBuilder sql)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				// Суммарная активная мощность P
				float p = 0;
				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					p = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 132, true);
				}
				else
					p = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 148, true);
				p = (float)Math.Round((double)p, 8);

				float apA = 0, apB = 0, apC = 0, apAB = 0, apBC = 0;

				// Активная мощность
				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					apA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 260, true);
					apA = (float)Math.Round((double)apA, 8);
					if (connection_scheme == ConnectScheme.Ph3W4 ||
						connection_scheme == ConnectScheme.Ph3W4_B_calc)
					{
						apB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 1284, true);
						apB = (float)Math.Round((double)apB, 8);
						apC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 2308, true);
						apC = (float)Math.Round((double)apC, 8);
					}
				}
				else
				{
					apAB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 3280, true);
					apAB = (float)Math.Round((double)apAB, 8);
					apBC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, shift + 3284, true);
					apBC = (float)Math.Round((double)apBC, 8);
				}

				// Мощности гармоник
				float[] pA_AB = new float[40];
				float[] pB_BC = new float[40];
				float[] pC = new float[40];

				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					for (int i = 0; i < 40; ++i)
					{
						pA_AB[i] = Conversions.bytes_2_float2wIEEE754_old(
							ref buffer, shift + 756 + i * 4, true);
						pA_AB[i] = (float)Math.Round((double)pA_AB[i], 8);

						if (connection_scheme == ConnectScheme.Ph3W4 ||
							connection_scheme == ConnectScheme.Ph3W4_B_calc)
						{
							pB_BC[i] = Conversions.bytes_2_float2wIEEE754_old(
								ref buffer, shift + 1780 + i * 4, true);
							pB_BC[i] = (float)Math.Round((double)pB_BC[i], 8);
							pC[i] = Conversions.bytes_2_float2wIEEE754_old(
								ref buffer, shift + 2804 + i * 4, true);
							pC[i] = (float)Math.Round((double)pC[i], 8);
						}
					}
				}
				else
				{
					for (int i = 0; i < 40; ++i)
					{
						pA_AB[i] = Conversions.bytes_2_float2wIEEE754_old(
								ref buffer, shift + 3352 + i * 4, true);
						pA_AB[i] = (float)Math.Round((double)pA_AB[i], 8);

						pB_BC[i] = Conversions.bytes_2_float2wIEEE754_old(
							ref buffer, shift + 3512 + i * 4, true);
						pB_BC[i] = (float)Math.Round((double)pB_BC[i], 8);

						pC[i] = 0;
					}
				}
				
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb = new System.Text.StringBuilder();
				if (connection_scheme != ConnectScheme.Ph1W2)
					sb.Append("INSERT INTO period_avg_params_6a VALUES (");
				else if (connection_scheme == ConnectScheme.Ph1W2)
					sb.Append("INSERT INTO period_avg_params_6a (datetime_id, event_datetime, p_sum, p_a_1, p_a_1_1, p_a_1_2, p_a_1_3, p_a_1_4, p_a_1_5, p_a_1_6, p_a_1_7, p_a_1_8, p_a_1_9, p_a_1_10, p_a_1_11, p_a_1_12, p_a_1_13, p_a_1_14, p_a_1_15, p_a_1_16, p_a_1_17, p_a_1_18, p_a_1_19, p_a_1_20, p_a_1_21, p_a_1_22, p_a_1_23, p_a_1_24, p_a_1_25, p_a_1_26, p_a_1_27, p_a_1_28, p_a_1_29, p_a_1_30, p_a_1_31, p_a_1_32, p_a_1_33, p_a_1_34, p_a_1_35, p_a_1_36, p_a_1_37, p_a_1_38, p_a_1_39, p_a_1_40, mask1, mask2) VALUES (");

				// adding pkey
				sb.Append("currval('period_avg_params_times_datetime_id_seq'), '");
				sb.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

				// P_Σ 
				sb.Append(p.ToString(ci) + ", ");

				// P_A(1)
				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					sb.Append(apA.ToString("F8", ci) + ", ");				// P_A	
				}
				else
				{
					sb.Append(apAB.ToString("F8", ci) + ", ");				// P_1
				}

				// P_B(2)
				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					sb.Append(apB.ToString(ci) + ", ");				// P_B	
				}
				if (connection_scheme == ConnectScheme.Ph3W3 ||
					connection_scheme == ConnectScheme.Ph3W3_B_calc)
				{
					sb.Append(apBC.ToString(ci) + ", ");			// P_2
				}

				// P_C
				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					sb.Append(apC.ToString(ci) + ", ");				// P_C
				}
				if (connection_scheme == ConnectScheme.Ph3W3 ||
					connection_scheme == ConnectScheme.Ph3W3_B_calc) // а для 3ф3пр забиваем NULL
				{
					sb.Append("NULL, ");
				}

				for (int i = 0; i < 40; i++)	// P_A(1) 1..P_A(1) 40
					sb.Append(pA_AB[i].ToString(ci) + ", ");

				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					for (int i = 0; i < 40; i++)	// P_B(2) 1..P_B(2) 40
						sb.Append(pB_BC[i].ToString(ci) + ", ");
					for (int i = 0; i < 40; i++)	// P_C 1..P_C 40
						sb.Append(pC[i].ToString(ci) + ", ");
				}

				sb.Append(masksAvg[0].ToString() + ", ");
				sb.Append(masksAvg[1].ToString());
				sb.Append(");\n");
				sql.Append(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in xml2sql_avg_p4(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#endregion

		#region Service functions
		
		long TrimDuration(long duration)
		{
			if ((duration % 100) != 0)
			{
				duration -= (duration % 100);
			}

			return duration;
		}

		#endregion
	}
}
