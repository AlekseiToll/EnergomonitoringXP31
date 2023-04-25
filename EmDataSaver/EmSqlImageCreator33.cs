using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using EmDataSaver.XmlImage;
using EmDataSaver.SavingInterface;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.ComponentModel;

using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.SqlImage;
using DeviceIO;
using EmServiceLib;
using EmServiceLib.SavingInterface;
using DbServiceLib;

namespace EmDataSaver
{
	public class EmSqlImageCreator33 : EmSqlImageCreatorBase
	{
		#region Events

		public delegate void InvalidDurationHandler(TimeSpan time);
		/// <summary>
		/// Событие OnInvalidDuration происходит если при считывании ПКЭ длительность  
		/// получилась больше суток
		/// </summary>
		public event InvalidDurationHandler OnInvalidDuration;

		#endregion

		#region Fields

		private EmXmlDeviceImage xmlImage_;
		private EmSqlDeviceImage sqlImage_;

		private BackgroundWorker bw_ = null;
		
		// коэффициент для расчета времени при несовпадении значения таймера с разницей между 
		// началом и концом периода
		private double coeffTimer = 0;

		private const double dblMaxDnoPeriodInMs = 10000000;

		#endregion

		#region Properties

		public EmSqlDeviceImage SqlImage
		{
			get { return sqlImage_; }
		}

		/// <summary>Gets image file extention</summary>
        public static string ImageFileExtention
        {
            get { return "em33t.xml"; }
        }

		public static string ImageFileExtention33T1
		{
			get { return "em33t1.xml"; }
		}

		public static string ImageFileExtention31K
		{
			get { return "em31k.xml"; }
		}
		
		/// <summary>Gets image files filter string for open/save dialogs</summary>
		public static string ImageFilter
		{
			get
			{
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					return String.Format(
						"Файлы Образа EM-3.3Т (*.{0})|*.{0}", ImageFileExtention);
				else
					return String.Format(
						"EM-3.3T Image files (*.{0})|*.{0}", ImageFileExtention);
			}
		}

		public static string ImageFilter33T1
		{
			get
			{
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					return String.Format(
						"Файлы Образа EM-3.3Т1 (*.{0})|*.{0}", ImageFileExtention33T1);
				else
					return String.Format(
						"EM-3.3T1 Image files (*.{0})|*.{0}", ImageFileExtention33T1);
			}
		}

		public static string ImageFilter31K
		{
			get
			{
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					return String.Format(
						"Файлы Образа EM-3.1K (*.{0})|*.{0}", ImageFileExtention31K);
				else
					return String.Format(
						"EM-3.1K Image files (*.{0})|*.{0}", ImageFileExtention31K);
			}
		}

		#endregion

		#region Constructor

		public EmSqlImageCreator33(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			ref EmXmlDeviceImage xmlImage)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.bw_ = bw;
			this.xmlImage_ = xmlImage;
		}

		#endregion

		#region Main methods

		public override bool CreateSqlImage()
		{
			try
			{
				if (xmlImage_ == null)
					throw new EmException("EmSqlImageCreator33::CreateSqlImage: xmlImage == null");

				#region progressbar
				// считаем число шагов (для progressbar)
				cnt_steps_progress_ = 0;
				for (int i = 0; i < xmlImage_.ArchiveList.Length; i++)
				{
					// pqp
					if (xmlImage_.ArchiveList[i].ArchivePQP != null)
						cnt_steps_progress_ += (uint)xmlImage_.ArchiveList[i].ArchivePQP.Length;
					//dns
					if (xmlImage_.ArchiveList[i].ArchiveDNS != null)
						cnt_steps_progress_ += xmlImage_.ArchiveList[i].ArchiveDNS.DnsNum;
					//avg
					if (xmlImage_.ArchiveList[i].ArchiveAVG != null)
						cnt_steps_progress_ += (uint)xmlImage_.ArchiveList[i].ArchiveAVG.DataPages.Length / 
							(uint)Em33TDevice.bytes_per_page;
				}
				// делаем ProgressBar с запасом, иначе на последних шагах он 
				// долго висит заполненный
				cnt_steps_progress_ += (uint)cnt_steps_progress_ / 50;
				//////////////////////////////
				#endregion

				// формируем SQL/XML образ
				sqlImage_ = new EmSqlDeviceImage();
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
				sqlImage_.SerialNumber = xmlImage_.SerialNumber;
				sqlImage_.Name = xmlImage_.Name;
				sqlImage_.InternalType = xmlImage_.InternalType;
				sqlImage_.DeviceType = xmlImage_.DeviceType;
				sqlImage_.Version = xmlImage_.Version;

				string folder_name = string.Format("{0} #{1} {2}", sqlImage_.Name, sqlImage_.SerialNumber, 
					DateTime.Now.ToString());

				// На выходе получаем готовый к выполнению SQL запрос, который проверит 
				// существование записи о Приборе в таблице devices и в случае необходимости
				// добавит её, затем вставит в таблицу folders запись о корневой папке всего
				// сохранения и вернет folder_id этой папки
				sqlImage_.Sql = string.Format("SELECT check_dev_serial({0}, CAST({1} as smallint));\nINSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{2}', false, 0, 0, null);\nSELECT currval('folders_folder_id_seq');",
					sqlImage_.SerialNumber,
					sqlImage_.InternalType,
					folder_name);

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				sqlImage_.Archives = new EmSqlArchive[xmlImage_.ArchiveList.Length];
				for (int i = 0; i < xmlImage_.ArchiveList.Length; i++)
				{
					sqlImage_.Archives[i] = new EmSqlArchive();

					bool res = xml2sql_archive(
						xmlImage_.ArchiveList[i],
						xmlImage_.SerialNumber,
						xmlImage_.Name,
						xmlImage_.Version,
						xmlImage_.DeviceType,
						xmlImage_.ArchiveList[i].AvgType,
						ref sqlImage_.Archives[i]);
					if (!res) return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmSqlImageCreator33::xml2sql(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				throw;
			}
		}

		private bool xml2sql_archive(EmXmlArchive xmlArhive, 
			long dev_id, string dev_name, string dev_ver, EmDeviceType dev_type, AvgTypes avgType,
			ref EmSqlArchive sqlArchive)
		{
			try
			{
				sqlArchive.ObjectName = xmlArhive.ObjectName;
				sqlArchive.CommonBegin = xmlArhive.CommonBegin;
				sqlArchive.CommonEnd = xmlArhive.CommonEnd;
				sqlArchive.ConnectionScheme = xmlArhive.ConnectionScheme;
				sqlArchive.CurrentTransducerIndex = xmlArhive.CurrentTransducerIndex;
				sqlArchive.F_Nominal = xmlArhive.F_Nominal;
				sqlArchive.U_NominalLinear = xmlArhive.U_NominalLinear;
				sqlArchive.U_NominalPhase = xmlArhive.U_NominalPhase;
				sqlArchive.U_Limit = xmlArhive.U_Limit;
				sqlArchive.I_Limit = xmlArhive.I_Limit;
				//	sqlArchive.DnsTimer = xmlArhive.DnsTimer;

				// На выходе будем иметь sql-запрос состоящий из добавления записи папки,
				// а потом записи архива в эту папку, в котором (запросе) будет недоставать
				// нескольких параметров:
				// {0} - folder_id из запроса, добавляющего корневую папку всего сохранения

				sqlArchive.Sql = string.Format("INSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{0}', true, {1}, 0, null);\n",
						sqlArchive.ObjectName, "{0}");
				sqlArchive.Sql += string.Format("INSERT INTO databases (db_id, start_datetime, end_datetime, con_scheme, f_nom, u_nom_lin, u_nom_ph, device_id, parent_id, db_name, db_info, u_limit, i_limit, current_transducer_index, device_name, device_version, t_fliker) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, currval('folders_folder_id_seq'), null, null, {7}, {8}, {9}, '{10}', '{11}', {12});\nSELECT currval('databases_db_id_seq');",
						sqlArchive.CommonBegin.ToString("MM.dd.yyyy HH:mm:ss"), // start_datetime
						sqlArchive.CommonEnd.ToString("MM.dd.yyyy HH:mm:ss"),// end_datetime
						(short)sqlArchive.ConnectionScheme,
						sqlArchive.F_Nominal.ToString(new CultureInfo("en-US")),		// f_nom
						sqlArchive.U_NominalLinear.ToString(new CultureInfo("en-US")),	// u_nom_lin
						sqlArchive.U_NominalPhase.ToString(new CultureInfo("en-US")),	// u_nom_ph
						dev_id,													// device_id
						sqlArchive.U_Limit.ToString(new CultureInfo("en-US")),	// u_limit
						sqlArchive.I_Limit.ToString(new CultureInfo("en-US")),	// i_limit
						sqlArchive.CurrentTransducerIndex,	// current_transduser_index
						dev_name,		// device_name
						dev_ver,		// device_version
						xmlArhive.T_fliker);

				List<EmSqlDataNode> sqlDataTopList = new List<EmSqlDataNode>();
				bool res = true;

				if (xmlArhive != null)
				{
					MakeTimerCoefficient(ref xmlArhive);
				}

				if (xmlArhive.ArchivePQP != null)
				{
					if (xmlArhive.ArchivePQP.Length != 0)
					{
						for (int i = 0; i < xmlArhive.ArchivePQP.Length; i++)
						{
							if (bw_.CancellationPending)
							{
								e_.Cancel = true;
								return false;
							}

							EmSqlDataNode pqpNode = new EmSqlDataNode();
							res = xml2sql_pqp_period(xmlArhive.ArchivePQP[i], xmlArhive.ConnectionScheme,
										sqlArchive.F_Nominal, sqlArchive.U_NominalLinear,
										sqlArchive.U_NominalPhase, ref pqpNode, dev_ver, avgType, dev_type);
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
				}
				if (!res)
					return false;

				if (xmlArhive.ArchiveDNS != null)
				{
					EmSqlDataNode pqpNode = new EmSqlDataNode();
					res = xml2sql_dns_period(xmlArhive.ArchiveDNS, ref pqpNode);
					sqlDataTopList.Add(pqpNode);
				}
				if (!res) return false;

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				if (xmlArhive.ArchiveAVG != null)
				{
					EmSqlDataNode pqpNode = new EmSqlDataNode();
					res = xml2sql_avg_period(xmlArhive.ArchiveAVG, xmlArhive.ConnectionScheme,
						xmlArhive.F_Nominal, xmlArhive.U_NominalLinear, xmlArhive.U_NominalPhase,
						ref pqpNode);
					sqlDataTopList.Add(pqpNode);
				}
				if (!res) return false;

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				sqlArchive.Data = new EmSqlDataNode[sqlDataTopList.Count];
				sqlDataTopList.CopyTo(sqlArchive.Data);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmSqlImageCreator33::xml2sql_archive(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		#region Building SQL query for inserting PQP data from Device

		private bool xml2sql_pqp_period(EmXmlPQP xmlPQP, ConnectScheme connection_scheme, float fNom, 
									float uLinNom, float uPhNom, ref EmSqlDataNode pqpNode, 
									string device_version, AvgTypes avgType, EmDeviceType devType)
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
				int constraint_type = xmlPQP.StandardSettingsType;

				bool flikkerExists = false;
				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					if (devType == EmDeviceType.EM33T1)
						flikkerExists = true;
					else if (devType == EmDeviceType.EM33T)
						if (Constants.isNewDeviceVersion_EM33T(device_version))
							flikkerExists = true;
				}

				// времена начала и конца максимальной нагрузки
				int iHHs1 = 0, iMMs1 = 0, iHHe1 = 0, iMMe1 = 0,
					iHHs2 = 0, iMMs2 = 0, iHHe2 = 0, iMMe2 = 0;

				DateTime ml_start_time_1 = DateTime.MinValue;
				DateTime ml_end_time_1 = DateTime.MinValue;
				DateTime ml_start_time_2 = DateTime.MinValue;
				DateTime ml_end_time_2 = DateTime.MinValue;

				iHHs1 = Conversions.byte_2_DAA(buffer[1]);
				iMMs1 = Conversions.byte_2_DAA(buffer[0]);
				iHHe1 = Conversions.byte_2_DAA(buffer[3]);
				iMMe1 = Conversions.byte_2_DAA(buffer[2]);

				iHHs2 = Conversions.byte_2_DAA(buffer[5]);
				iMMs2 = Conversions.byte_2_DAA(buffer[4]);
				iHHe2 = Conversions.byte_2_DAA(buffer[7]);
				iMMe2 = Conversions.byte_2_DAA(buffer[6]);

				iHHs1 %= 24;
				iHHe1 %= 24;
				iHHs2 %= 24;
				iHHe2 %= 24;

				if (!(iHHs1 == iHHe1 && iMMs1 == iMMe1))
				{
					ml_start_time_1 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, 
						xmlPQP.Start.Day, iHHs1, iMMs1, 0);
					ml_end_time_1 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, 
						xmlPQP.Start.Day, iHHe1, iMMe1, 0);
				}
				if (!(iHHs2 == iHHe2 && iMMs2 == iMMe2))
				{
					ml_start_time_2 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, 
						xmlPQP.Start.Day, iHHs2, iMMs2, 0);
					ml_end_time_2 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, 
						xmlPQP.Start.Day, iHHe2, iMMe2, 0);
				}

				// на выходе имеем SQL запрос добавления записи 
				// периода ПКЭ а также всех дочерних данных по ПКЭ,
				// имеющий единственный недостающий параметр - database_id
				pqpNode.Sql = string.Format("INSERT INTO day_avg_parameter_times (datetime_id, start_datetime, end_time, constraint_type, ml_start_time_1, ml_end_time_1, ml_start_time_2, ml_end_time_2, database_id) VALUES (DEFAULT, '{0}', '{1}', {2}, '{3}', '{4}', '{5}', '{6}', {7});\n",
					xmlPQP.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlPQP.End.ToString("MM.dd.yyyy HH:mm:ss"),
					constraint_type,
					ml_start_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_start_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					"{0}");

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

				StringBuilder sb = new StringBuilder();
				// t1:  K2_U, K0_U 
				// 6.2.done
				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					xml2sql_pqp_t1(ref buffer, ref sb, 448, 36, 1101); // K_2U
				}
				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					xml2sql_pqp_t1(ref buffer, ref sb, 464, 40, 1102); // K_0U
				}

				// t2:  ∆f, δU_y, δU_A(AB), δU_B(BC), δU_C(CA),
				//      δU_y_', δU_A(AB)_', δU_B(BC)_', δU_C(CA)_',
				//      δU_y_", δU_A(AB)_", δU_B(BC)_", δU_C(CA)_"
				//
				// вот тут и начинается волшебство ;)

				#region It's a kind of magic

				List<float> f = new List<float>();
				List<float> Uy = new List<float>();
				List<float> U_A = new List<float>();
				List<float> U_B = new List<float>();
				List<float> U_C = new List<float>();
				List<float> U_AB = new List<float>();
				List<float> U_BC = new List<float>();
				List<float> U_CA = new List<float>();
				List<float> Uy1 = new List<float>();
				List<float> U_A1 = new List<float>();
				List<float> U_B1 = new List<float>();
				List<float> U_C1 = new List<float>();
				List<float> U_AB1 = new List<float>();
				List<float> U_BC1 = new List<float>();
				List<float> U_CA1 = new List<float>();
				List<float> Uy2 = new List<float>();
				List<float> U_A2 = new List<float>();
				List<float> U_B2 = new List<float>();
				List<float> U_C2 = new List<float>();
				List<float> U_AB2 = new List<float>();
				List<float> U_BC2 = new List<float>();
				List<float> U_CA2 = new List<float>();
				// fliker
				List<float> flik_A = new List<float>();
				List<float> flik_B = new List<float>();
				List<float> flik_C = new List<float>();

				int _g_shift = 4096;

				int record_size = 44;
				if (devType == EmDeviceType.EM33T1)
					record_size = 48;
				else if (devType == EmDeviceType.EM33T)
					if (Constants.isNewDeviceVersion_EM33T(device_version))
						record_size = 48;

				byte[] DataPages = xmlPQP.DataPages;

				// перебираем все страницы с данными о частоте и напряжениях
				for (int unfp = 0; unfp < xmlPQP.UnfPagesLength; unfp++)
				{
					// смещение начала текущей страницы с данными о частоте и напряжениях
					int page_sh = _g_shift + unfp * 512;
					// количество записей на данной странице
					int usnf_length = Conversions.bytes_2_ushort(ref DataPages, page_sh + 2);
					//смещение текущей записи
					int rec_sh = 4;

					// перебираем все записи на unfp-той странице
					for (int usnf = 0; usnf < usnf_length; usnf++)
					{
						float _f1 = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 0) - fNom;
						float _f2 = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 4) - fNom;
						float _f3 = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 8) - fNom;

						float _Uy = Conversions.bytes_2_signed_float2w65536(ref DataPages,
							page_sh + rec_sh + 12) - 
									((connection_scheme == ConnectScheme.Ph1W2) ? uPhNom : uLinNom);
						_Uy /= ((connection_scheme == ConnectScheme.Ph1W2) ? uPhNom : uLinNom) / 100;

						float _U_A = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 16) - uPhNom;
						_U_A /= uPhNom / 100;
						float _U_B = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 20) - uPhNom;
						_U_B /= uPhNom / 100;
						float _U_C = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 24) - uPhNom;
						_U_C /= uPhNom / 100;
						float _U_AB = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 28) - uLinNom;
						_U_AB /= uLinNom / 100;
						float _U_BC = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 32) - uLinNom;
						_U_BC /= uLinNom / 100;
						float _U_CA = Conversions.bytes_2_signed_float2w65536(ref DataPages,
										page_sh + rec_sh + 36) - uLinNom;
						_U_CA /= uLinNom / 100;
						// частота
						f.Add(_f1); f.Add(_f2); f.Add(_f3);
						// напряжения (без учета времени максимальных и минимальных нагрузок)
						Uy.Add(_Uy);
						U_A.Add(_U_A);
						U_B.Add(_U_B);
						U_C.Add(_U_C);
						U_AB.Add(_U_AB);
						U_BC.Add(_U_BC);
						U_CA.Add(_U_CA);

						int tp;
						if ( (devType == EmDeviceType.EM33T1) || 
							(devType == EmDeviceType.EM33T &&
							Constants.isNewDeviceVersion_EM33T(device_version)) )
						{
							tp = (int)Conversions.bytes_2_ushort(ref DataPages, page_sh + rec_sh + 40);
						}
						else
						{
							tp = (int)Conversions.bytes_2_uint(ref DataPages, page_sh + rec_sh + 40);
						}

						if (tp == 1)
						{
							Uy1.Add(_Uy);
							U_A1.Add(_U_A);
							U_B1.Add(_U_B);
							U_C1.Add(_U_C);
							U_AB1.Add(_U_AB);
							U_BC1.Add(_U_BC);
							U_CA1.Add(_U_CA);
						}
						else
							if (tp == 2)
							{
								Uy2.Add(_Uy);
								U_A2.Add(_U_A);
								U_B2.Add(_U_B);
								U_C2.Add(_U_C);
								U_AB2.Add(_U_AB);
								U_BC2.Add(_U_BC);
								U_CA2.Add(_U_CA);
							}

						// fliker
						if (flikkerExists)
						{
							// три значения фликера для фаз A, B, C (формат угла)
							float val;
							if (buffer[page_sh + rec_sh + 42] != 0xFF ||
								buffer[page_sh + rec_sh + 42 + 1] != 0xFF)
							{
								val = Conversions.bytes_2_unsigned_float1024(ref buffer,
									page_sh + rec_sh + 42);
								flik_A.Add(val);  // фликер для фазы А
							}
							//else
							//flik_A.Add(Single.NaN);

							if (buffer[page_sh + rec_sh + 44] != 0xFF ||
								buffer[page_sh + rec_sh + 44 + 1] != 0xFF)
							{
								val = Conversions.bytes_2_unsigned_float1024(ref buffer,
									page_sh + rec_sh + 44);
								flik_B.Add(val);  // фликер для фазы B
							}

							if (buffer[page_sh + rec_sh + 46] != 0xFF ||
								buffer[page_sh + rec_sh + 46 + 1] != 0xFF)
							{
								val = Conversions.bytes_2_unsigned_float1024(ref buffer,
									page_sh + rec_sh + 46);
								flik_C.Add(val);  // фликер для фазы C
							}
						}

						rec_sh += record_size; // переходим к следующей записи
					}
				}	// закончили форировать списки

				// читаем из уставок ГОСТы для НДП и ПДП
				// не будем читать из уставок ГОСТы для НДП и ПДП
				// будев вызывать фкнкцию с параметрами... но сначала мы их все расчитаем ;)
				//
				// оставайтесь с нами ;)
				//

				xml2sql_pqp_t2(ref buffer, ref sb, ref f, 4, 1001);	// ∆f

				xml2sql_pqp_t2(ref buffer, ref sb, ref Uy, 20, 1002); // δU_y
				xml2sql_pqp_t2(ref buffer, ref sb, ref Uy1, 28, 1006); // δU_y_'
				xml2sql_pqp_t2(ref buffer, ref sb, ref Uy2, 20, 1010); // δU_y_"

				if (connection_scheme != ConnectScheme.Ph3W3 &&
					connection_scheme != ConnectScheme.Ph3W3_B_calc)
				{
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_A, 20, 1003); // δU_A
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_A1, 28, 1007); // δU_A_'
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_A2, 20, 1011); // δU_A_"
				}

				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_B, 20, 1004); // δU_B
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_C, 20, 1005); // δU_C

					xml2sql_pqp_t2(ref buffer, ref sb, ref U_B1, 28, 1008); // δU_B_'
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_C1, 28, 1009); // δU_C_'

					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_B2, 20, 1012); // δU_B_"
					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_C2, 20, 1013); // δU_C_"
				}

				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_AB, 20, 1014); // δU_AB
					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_BC, 20, 1015); // δU_BC
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_CA, 20, 1016); // δU_CA

					xml2sql_pqp_t2(ref buffer, ref sb, ref U_AB1, 28, 1017); // δU_AB_'
					xml2sql_pqp_t2(ref buffer, ref sb, ref U_BC1, 28, 1018); // δU_BC_'
					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_CA1, 28, 1019); // δU_CA_'

					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_AB2, 20, 1020); // δU_AB_"
					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_BC2, 20, 1021); // δU_BC_"
					xml2sql_pqp_t2(ref buffer, ref sb, ref  U_CA2, 20, 1022); // δU_CA_"
				}

				#endregion

				// t3:  events
				// 6.2.done
				xml2sql_pqp_t3(ref buffer, ref sb, 3360, "A", 0);	// swell / перенапряжение 
				xml2sql_pqp_t3(ref buffer, ref sb, 3380, "A", 1);	// dip / провал
				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					xml2sql_pqp_t3(ref buffer, ref sb, 3400, "B", 0);	// swell / перенапряжение 
					xml2sql_pqp_t3(ref buffer, ref sb, 3420, "B", 1);	// dip / провал
					xml2sql_pqp_t3(ref buffer, ref sb, 3440, "C", 0);	// swell / перенапряжение 
					xml2sql_pqp_t3(ref buffer, ref sb, 3460, "C", 1);	// dip / провал
					xml2sql_pqp_t3(ref buffer, ref sb, 3480, "*", 0);	// swell / перенапряжение 
					xml2sql_pqp_t3(ref buffer, ref sb, 3500, "*", 1);	// dip / провал
				}

				// t4:	K_UA(2..40), K_UB(2..40), K_UC(2..40) а также K_UA, K_UB и K_UC
				// в таблице parameters для прошивки 6.2 добавлена новая группа параметров
				// с идентификаторами от 1301 до 1340 для линейных несинусоидальностей;
				// их имена совпадают полностью с именами параметров от 1201 до 1240 (для фазных)			
				// 6.2.done
				int j = 0;
				for (j = 0; j < 39; ++j)
				{
					xml2sql_pqp_t4(ref buffer, ref sb, j * 12, 44 + j * 4,
								1202 + j, connection_scheme);
				}
				xml2sql_pqp_t4(ref buffer, ref sb, j * 12, 44 + j * 4, 1201,
							connection_scheme);

				// fliker
				if (flikkerExists)
				{
					xml2sql_pqp_t5(flik_A, flik_B, flik_C, ref sb, connection_scheme,
						startTimeFlik, endTimeFlik, xmlPQP.T_fliker);
				}

				xml2sql_pqp_t6(ref xmlPQP, ref sb, uLinNom, uPhNom, device_version,
					connection_scheme, devType);
				xml2sql_pqp_t7(ref xmlPQP, ref sb, fNom, device_version, devType);

				pqpNode.Sql += sb.ToString();

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in EmSqlImageCreator33::xml2sql_pqp_period(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		private void xml2sql_pqp_t1(ref byte[] buffer, ref StringBuilder SqlQuery, int Shift, 
							int ShiftNominal, int param_id)
		{
			try
			{
				// после изменения карты памяти у Валеры - область копии уставок немного 
				// съехала - на 24 байта:
				ShiftNominal += 20;

				CultureInfo ci_enUS = new CultureInfo("en-US");

				// отсчеты прибора
				ushort num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, Shift + 2);		// отсчетов в НДП
				ushort num_max_rng = Conversions.bytes_2_ushort(ref buffer, Shift + 4);		// отсчетов в ПДП
				ushort num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, Shift + 6);	// отсчетов за ПДП

				// если данные по отсчетам по какой-то причине отсутствуют
				// делаем вид, что все прошло успешно
				if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return;

				SqlQuery.Append(string.Format("INSERT INTO day_avg_parameters_t1 (datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});\n",
					param_id,
					num_nrm_rng,
					num_max_rng,
					num_out_max_rng,
					(Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal) * 100).ToString(ci_enUS),	// НДП
					(Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 2) * 100).ToString(ci_enUS),// ПДП
					(Conversions.bytes_2_float2w65536(ref buffer, Shift + 8) * 100).ToString(ci_enUS),	// 95%
					(Conversions.bytes_2_float2w65536(ref buffer, Shift + 12) * 100).ToString(ci_enUS)) // max
					);
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_pqp_t1(): " + e.Message);
				throw e;
			}
		}

		protected bool IsDuring24Hours(int param_id)
		{
			switch (param_id)
			{
				case 1002: // Uy
				case 1003: // UA
				case 1004: // UB
				case 1005: // UC
				case 1014: // AB
				case 1015: // BC
				case 1016: // CA
					return true;
			}
			return false;
		}

		private void xml2sql_pqp_t2(ref byte[] buffer, ref StringBuilder SqlQuery, ref List<float> list,
			int ShiftNominal, int param_id)
		{
			try
			{
				// если нет ничего в списке - попросту выходим
				if (list.Count == 0) return;

				CultureInfo ci_enUS = new CultureInfo("en-US");

				// сдесь почему-то не 24 а 20 - непонятно - но практикой выявлено что надо 20 :(
				ShiftNominal += 20;

				// ГОСТы из уставок
				float fNDP_d, fNDP_u, fPDP_d, fPDP_u; // ГОСТ ( НДП н, НДП в, ПДП н, ПДП в)			
				if (param_id == 1001) // Гост для частоты хранится в двойном слове, остальные в одинарном
				{
					fNDP_d = Conversions.bytes_2_signed_float2w65536(ref buffer, ShiftNominal);
					fNDP_u = Conversions.bytes_2_signed_float2w65536(ref buffer, ShiftNominal + 4);
					fPDP_d = Conversions.bytes_2_signed_float2w65536(ref buffer, ShiftNominal + 8);
					fPDP_u = Conversions.bytes_2_signed_float2w65536(ref buffer, ShiftNominal + 12);
				}
				else
				{
					if (IsDuring24Hours(param_id)
						//param_id == 1002 ||	// Uy
						//param_id == 1003 ||	// UA
						//param_id == 1004 ||	// UB
						//param_id == 1005 ||	// UC
						//param_id == 1014 ||	// UAB
						//param_id == 1015 ||	// UBC
						//param_id == 1016
						)	// UCA
					{
						fNDP_d = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal) * 100;
						fNDP_u = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal + 10) * 100;
						fPDP_d = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal + 4) * 100;
						fPDP_u = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal + 14) * 100;
					}
					else
					{
						fNDP_d = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal) * 100;
						fNDP_u = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal + 2) * 100;
						fPDP_d = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal + 4) * 100;
						fPDP_u = Conversions.bytes_2_signed_float1w65536(ref buffer, ShiftNominal + 6) * 100;
					}
				}

				list.Sort();
				float N = list.Count;

				// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
				float f_max = (float)Math.Round(list[(int)N - 1], 3);
				float f_min = (float)Math.Round(list[0], 3);

				// 95 %
				string str_f_95_d = string.Empty;
				string str_f_95_u = string.Empty;

				int mn = 0, mv = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] > fNDP_u) mv++;
					if (list[i] < fNDP_d) mn++;
				}
				double Nn = 0, Nv = 0;

				// все эти алгоритмы имеют смысл только в случае
				// когда количество отсчетов хотя бы 40
				// иначе будем вставлять в базу NULL'ы
				if (list.Count >= 40)
				{
					if (mn == 0 && mv == 0)
					{
						Nn = 0.025 * N;
						Nv = Nn + 0.95 * N;
					}
					else if (mn + mv <= 0.05 * N)
					{
						if (mn == 0)
						{
							Nn = (0.05 * N - mv) / 2;
							Nv = Nn + 0.95 * N;
						}
						else if (mv == 0)
						{
							Nn = (0.05 * N + mn) / 2;
							Nv = Nn + 0.95 * N;
						}
						else
						{
							Nn = 0.05 * N * mn / (mn + mv);
							Nv = Nn + 0.95 * N;
						}
					}
					else if (mn + mv < list.Count)
					{
						if (mn != 0 && mv != 0)
						{
							Nn = 0.05 * N * mn / (mn + mv);
							Nv = Nn + 0.95 * N;
						}
						else if (mn == 0 && mv != 0)
						{
							Nn = 1;
							Nv = 0.95 * N;
						}
						else
						{
							Nn = 0.05 * N;
							Nv = N;
						}
					}
					else
					{
						Nn = 0.0025 * N;
						Nv = Nn + 0.95 * N;
					}

					Nn = Math.Floor(Nn) - 1;
					Nv = Math.Ceiling(Nv) - 1;

					if (Nn < 0) Nn = 0;
					if (Nv >= N) Nv = N - 1;

					str_f_95_d = Math.Round(list[(int)Nn], 3).ToString(ci_enUS);
					str_f_95_u = Math.Round(list[(int)Nv], 3).ToString(ci_enUS);
				}
				else
				{
					str_f_95_d = "NULL";
					str_f_95_u = "NULL";
				}

				// отсчеты прибора
				int Mn = 0, Mv = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] > fPDP_u) Mv++;
					if (list[i] < fPDP_d) Mn++;
				}

				ushort num_out_max_rng = (ushort)(Mv + Mn);		// отсчетов за ПДП
				int temp_num_nrm_rng = (int)N - mn - mv;
				if (temp_num_nrm_rng < 0)
					temp_num_nrm_rng = 0;
				ushort num_nrm_rng = (ushort)(temp_num_nrm_rng);		// отсчетов в НДП
				ushort num_max_rng = (ushort)(N - num_nrm_rng - num_out_max_rng); // отсчетов между ПДП и НДП

				// если данные по отсчетам по какой-то причине отсутствуют, 
				// делаем вид, что все прошло более чем успешно )
				if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return;

				SqlQuery.Append(string.Format("INSERT INTO day_avg_parameters_t2 (datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_bottom, real_nrm_rng_top, real_max_rng_bottom, real_max_rng_top, calc_nrm_rng_bottom, calc_nrm_rng_top, calc_max_rng_bottom, calc_max_rng_top) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});\n",
					param_id,
					num_nrm_rng.ToString(),		// отсчетов в НДП
					num_max_rng.ToString(),		// отсчетов в ПДП
					num_out_max_rng.ToString(),	// отсчетов за ПДП

					fNDP_d.ToString(ci_enUS),	// НДП н
					fNDP_u.ToString(ci_enUS),	// НДП в
					fPDP_d.ToString(ci_enUS),	// ПДП н
					fPDP_u.ToString(ci_enUS),	// ПДП в

					str_f_95_d,					// 95% н
					str_f_95_u,					// 95% в
					f_min.ToString(ci_enUS),	// min н
					f_max.ToString(ci_enUS)));	// max в
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in EmSqlImageCreator33::xml2sql_pqp_t2(): "
					+ e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t3(ref byte[] buffer, ref StringBuilder SqlQuery, int Shift, 
			string phase, short event_type)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// константа, необходимая для перевода десяток миллисекунд в наносекунды
				long time_const = 100000;

				// общая продолжительность
				long hi_word = 0, lo_word = 0;
				// 1st time
				// похоже что здесь слова поменяны местами!!!
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 2);

				// при вычислении common_duration отбрасываем последний знак (но сохраняем разрядность),
				// чтобы дробная часть миллисекунд была не больше трех знаков, иначе
				// потом возникает проблема при отображении данных (значение не конвертируется
				// в тип DateTime)
				long duration = (long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer);
				TimeSpan common_duration = new TimeSpan(TrimDuration(duration));
				//long duration = (long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer);
				//if ((duration % 100) != 0)
				//{
				//	duration -= (duration % 100);
				//}
				//TimeSpan common_duration = new TimeSpan(duration);
				//TimeSpan common_duration = new TimeSpan((long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer));

				// аналогично меняем местами слова в остальных временах
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 8);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 10);
				//TimeSpan max_period_period = new TimeSpan((lo_word + hi_word * 0x10000) * time_const);
				long lMaxPeriodPeriod = (long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer);
				TimeSpan max_period_period = new TimeSpan(TrimDuration(lMaxPeriodPeriod));

				//TimeSpan max_period_period = new TimeSpan((long)Conversions.bytes_2_uint(ref Buffer, Shift + 8) * time_const);
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 4);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 6);
				long lMaxValuePeriod = (long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer);
				TimeSpan max_value_period = new TimeSpan(TrimDuration(lMaxValuePeriod));
				//TimeSpan max_value_period = new TimeSpan((lo_word + hi_word * 0x10000) * time_const);

				// общее число перенапряжений за время регистрации
				// накладываем маску на третий бит самого старшего 
				// (после перестановки старшего и младшего байт в каждом слове)
				// байта в четверке ( AND 00011111b = 1Fh = 31d )
				buffer[Shift + 17] &= 31;

				uint common_number = Conversions.bytes_2_uint(ref buffer, Shift + 16);
				float max_period_value = (float)Conversions.bytes_2_short(ref buffer, Shift + 14) / 8192;
				float max_value_value = (float)Conversions.bytes_2_short(ref buffer, Shift + 12) / 8192;

				// если из-за погрешности вычислений длительности получилось больше суток 
				// (несмотря на использование коэффициента), то приравниваем длительность 
				// суткам, а на экран выводим сообщенние
				if (common_duration.Days > 0)
				{
					if (OnInvalidDuration != null)
					{
					    OnInvalidDuration(common_duration);
					}
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

				SqlQuery.Append(string.Format("INSERT INTO day_avg_parameters_t3 (datetime_id, event_type, phase, common_number, common_duration, max_period_period, max_value_period, max_period_value, max_value_value) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', '{5}', {6}, {7});\n",
					event_type,
					phase,
					common_number,
					common_duration.ToString(),
					max_period_period.ToString(),
					max_value_period.ToString(),
					max_period_value.ToString(ci_enUS),
					max_value_value.ToString(ci_enUS)));
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_pqp_t3(): " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t4(ref byte[] buffer, ref StringBuilder SqlQuery, int Shift, int ShiftNominal, 
			int param_id, ConnectScheme ConnectionScheme)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// в прошивке 6.2 уставки сдвинулись на 20 байта
				ShiftNominal += 20;

				// ФАЗНЫЕ ГАРМОНИКИ:
				if (ConnectionScheme != ConnectScheme.Ph3W3 && ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					// ГОСТы из уставок
					float fNDP = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal) * 100;
					float fPDP = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 2) * 100;
					// только первая фаза(!)
					// отсчеты прибора
					ushort num_nrm_rng_a = Conversions.bytes_2_ushort(ref buffer, 480 + Shift + 2);		// отсчетов в НДП
					ushort num_max_rng_a = Conversions.bytes_2_ushort(ref buffer, 480 + Shift + 4);		// отсчетов в ПДП
					ushort num_out_max_rng_a = Conversions.bytes_2_ushort(ref buffer, 480 + Shift + 6);	// отсчетов за ПДП

					// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
					float f_95_a = Conversions.bytes_2_float1w65536(ref buffer, 480 + Shift + 8) * 100;
					float f_max_a = Conversions.bytes_2_float1w65536(ref buffer, 480 + Shift + 10) * 100;

					if (ConnectionScheme == ConnectScheme.Ph3W4 ||
						ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
					{
						// отсчеты прибора
						ushort num_nrm_rng_b = Conversions.bytes_2_ushort(ref buffer, Shift + 1440 + 2);	// отсчетов в НДП ф.B
						ushort num_max_rng_b = Conversions.bytes_2_ushort(ref buffer, Shift + 1440 + 4);	// отсчетов в ПДП ф.B
						ushort num_out_max_rng_b = Conversions.bytes_2_ushort(ref buffer, Shift + 1440 + 6);// отсчетов за ПДП ф.B

						ushort num_nrm_rng_c = Conversions.bytes_2_ushort(ref buffer, Shift + 2400 + 2);	// отсчетов в НДП ф.C
						ushort num_max_rng_c = Conversions.bytes_2_ushort(ref buffer, Shift + 2400 + 4);	// отсчетов в ПДП ф.C
						ushort num_out_max_rng_c = Conversions.bytes_2_ushort(ref buffer, Shift + 2400 + 6);// отсчетов за ПДП ф.C

						// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
						float f_95_b = Conversions.bytes_2_float1w65536(ref buffer, Shift + 1440 + 8) * 100;	// ф.B
						float f_max_b = Conversions.bytes_2_float1w65536(ref buffer, Shift + 1440 + 10) * 100;	// ф.B

						float f_95_c = Conversions.bytes_2_float1w65536(ref buffer, Shift + 2400 + 8) * 100;	// ф.C
						float f_max_c = Conversions.bytes_2_float1w65536(ref buffer, Shift + 2400 + 10) * 100;	// ф.C

						if (num_nrm_rng_a != 0 || num_max_rng_a != 0 || num_out_max_rng_a != 0 ||
							num_nrm_rng_b != 0 || num_max_rng_b != 0 || num_out_max_rng_b != 0 ||
							num_nrm_rng_c != 0 || num_max_rng_c != 0 || num_out_max_rng_c != 0)
						{

							SqlQuery.Append(String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17});\n",
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
								fPDP.ToString(ci_enUS)));		// ПДП
						}
					}
					else // только одна фаза => запрос несколько упростится
					{
						if (num_nrm_rng_a != 0 || num_max_rng_a != 0 || num_out_max_rng_a != 0)
						{
							SqlQuery.Append(String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, 0,0,0,0,0,0,0,0,0,0, {6}, {7});\n",
								param_id,
								// фаза 1
								num_nrm_rng_a.ToString(),		// отсчетов в НДП
								num_max_rng_a.ToString(),		// отсчетов в ПДП
								num_out_max_rng_a.ToString(),	// отсчетов за ПДП
								f_95_a.ToString(ci_enUS),		// 95%
								f_max_a.ToString(ci_enUS),		// max
								// ГОСТы
								fNDP.ToString(ci_enUS),			// НДП
								fPDP.ToString(ci_enUS)));		// ПДП
						}
					}
				}

				// ЛИНЕЙНЫЕ ГАРМОНИКИ:
				if (ConnectionScheme != ConnectScheme.Ph1W2)
				{
					param_id += 100;

					// ГОСТы из уставок
					float fNDP = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 160) * 100;
					float fPDP = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 162) * 100;

					// отсчеты прибора
					ushort num_nrm_rng_ab = Conversions.bytes_2_ushort(ref buffer, 960 + Shift + 2);	// отсчетов в НДП ф.AB
					ushort num_max_rng_ab = Conversions.bytes_2_ushort(ref buffer, 960 + Shift + 4);	// отсчетов в ПДП ф.AB
					ushort num_out_max_rng_ab = Conversions.bytes_2_ushort(ref buffer, 960 + Shift + 6);// отсчетов за ПДП ф.AB

					ushort num_nrm_rng_bc = Conversions.bytes_2_ushort(ref buffer, Shift + 1920 + 2);	// отсчетов в НДП ф.BC
					ushort num_max_rng_bc = Conversions.bytes_2_ushort(ref buffer, Shift + 1920 + 4);	// отсчетов в ПДП ф.BC
					ushort num_out_max_rng_bc = Conversions.bytes_2_ushort(ref buffer, Shift + 1920 + 6);// отсчетов за ПДП ф.BC

					ushort num_nrm_rng_ca = Conversions.bytes_2_ushort(ref buffer, Shift + 2880 + 2);	// отсчетов в НДП ф.CA
					ushort num_max_rng_ca = Conversions.bytes_2_ushort(ref buffer, Shift + 2880 + 4);	// отсчетов в ПДП ф.CA
					ushort num_out_max_rng_ca = Conversions.bytes_2_ushort(ref buffer, Shift + 2880 + 6);// отсчетов за ПДП ф.CA

					// расчетные ГОСТы (или "какими они должны быть, чтобы все было хорошо")
					float f_95_ab = Conversions.bytes_2_float1w65536(ref buffer, 960 + Shift + 8) * 100;	// ф.AB
					float f_max_ab = Conversions.bytes_2_float1w65536(ref buffer, 960 + Shift + 10) * 100;	// ф.AC

					float f_95_bc = Conversions.bytes_2_float1w65536(ref buffer, Shift + 1920 + 8) * 100;	// ф.BC
					float f_max_bc = Conversions.bytes_2_float1w65536(ref buffer, Shift + 1920 + 10) * 100;	// ф.BC

					float f_95_ca = Conversions.bytes_2_float1w65536(ref buffer, Shift + 2880 + 8) * 100;	// ф.CA
					float f_max_ca = Conversions.bytes_2_float1w65536(ref buffer, Shift + 2880 + 10) * 100;	// ф.CA

					if (num_nrm_rng_ab != 0 || num_max_rng_ab != 0 || num_out_max_rng_ab != 0 ||
							num_nrm_rng_bc != 0 || num_max_rng_bc != 0 || num_out_max_rng_bc != 0 ||
							num_nrm_rng_ca != 0 || num_max_rng_ca != 0 || num_out_max_rng_ca != 0)
					{

						SqlQuery.Append(String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17});\n",
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
							fPDP.ToString(ci_enUS)));		// ПДП
					}
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_pqp_t4(): " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t5(List<float> flik_A, List<float> flik_B,
					List<float> flik_C, ref StringBuilder SqlQuery,
					ConnectScheme connection_scheme,
					DateTime startTime, DateTime endTime, short t_flik)
		{
			if (connection_scheme == ConnectScheme.Ph3W3 ||
				connection_scheme == ConnectScheme.Ph3W3_B_calc)
				return;

			try
			{
				DateTime curTime = startTime;
				DateTime fullTime = startTime;
				fullTime = fullTime.AddHours(2);  // время, начиная с которого фликер становится 
				// полноценным, т.е. измеренным не меньше чем за 2 часа
				short start = 0, end = 0;
				CultureInfo ci_enUS = new CultureInfo("en-US");

				for (int i = 0; i < flik_A.Count; ++i)
				{
					if (connection_scheme == ConnectScheme.Ph1W2)
					{
						SqlQuery.Append(string.Format("INSERT INTO day_avg_parameters_t5 (datetime_id, flik_time, flik_a, flik_a_long) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1}, {2});\n",
							curTime.ToString("HH:mm:ss"),//curTime.ToString("MM.dd.yyyy HH:mm:ss"),
							flik_A[i].ToString(ci_enUS),
							getLongFliker(flik_A, start, end).ToString(ci_enUS)));
					}
					else
					{
						SqlQuery.Append(string.Format("INSERT INTO day_avg_parameters_t5 (datetime_id, flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6});\n",
							curTime.ToString("HH:mm:ss"),//curTime.ToString("MM.dd.yyyy HH:mm:ss"),
							flik_A[i].ToString(ci_enUS),
							getLongFliker(flik_A, start, end).ToString(ci_enUS),
							flik_B[i].ToString(ci_enUS),
							getLongFliker(flik_B, start, end).ToString(ci_enUS),
							flik_C[i].ToString(ci_enUS),
							getLongFliker(flik_C, start, end).ToString(ci_enUS)));
					}
					//getLongFliker(flik_C, start, end).ToString("#########0.#####", ci_enUS));

					curTime = curTime.AddMinutes(t_flik);
					if (curTime > endTime) break;
					++end;
					if (end >= flik_A.Count) break;
					if (curTime > fullTime)
						++start;
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_pqp_t5(): " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t6(ref EmXmlPQP xmlPQP, ref StringBuilder sql,
			float uLinNom, float uPhNom, string device_version, ConnectScheme connectScheme,
			EmDeviceType devType)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				ushort timeInterval = 60;
				DateTime event_datetime = xmlPQP.Start.AddSeconds(timeInterval);
				byte[] buffer = xmlPQP.DataPages;

				int g_shift = 4096;

				int record_size = 44;
				if (devType == EmDeviceType.EM33T1)
					record_size = 48;
				else if (devType == EmDeviceType.EM33T)
					if (Constants.isNewDeviceVersion_EM33T(device_version))
						record_size = 48;

				float Uy = 0, U_A = 0, U_B = 0, U_C = 0, U_AB = 0, U_BC = 0, U_CA = 0;

				// перебираем все страницы с данными о частоте и напряжениях
				for (int unfp = 0; unfp < xmlPQP.UnfPagesLength; unfp++)
				{
					// смещение начала текущей страницы с данными о частоте и напряжениях
					int page_sh = g_shift + unfp * 512;
					//смещение текущей записи
					int rec_sh = 4;
					// количество записей на данной странице
					int usnf_length = Conversions.bytes_2_ushort(ref buffer, page_sh + 2);

					// перебираем все записи на unfp-той странице
					for (int usnf = 0; usnf < usnf_length; usnf++)
					{
						switch (connectScheme)
						{
							case ConnectScheme.Ph3W4:
							case ConnectScheme.Ph3W4_B_calc:
								Uy = Conversions.bytes_2_signed_float2w65536(ref buffer,
										page_sh + rec_sh + 12) - uLinNom;
								Uy /= uLinNom / 100;

								U_A = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 16) - uPhNom;
								U_A /= uPhNom / 100;
								U_B = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 20) - uPhNom;
								U_B /= uPhNom / 100;
								U_C = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 24) - uPhNom;
								U_C /= uPhNom / 100;
								U_AB = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 28) - uLinNom;
								U_AB /= uLinNom / 100;
								U_BC = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 32) - uLinNom;
								U_BC /= uLinNom / 100;
								U_CA = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 36) - uLinNom;
								U_CA /= uLinNom / 100;

								sql.Append(String.Format
									("INSERT INTO day_avg_parameters_t6 VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8});\n",
									"currval('day_avg_parameter_times_datetime_id_seq')",
									event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
									Uy.ToString(ci),
									U_A.ToString(ci),
									U_B.ToString(ci),
									U_C.ToString(ci),
									U_AB.ToString(ci),
									U_BC.ToString(ci),
									U_CA.ToString(ci)
									));
								break;

							case ConnectScheme.Ph3W3:
							case ConnectScheme.Ph3W3_B_calc:
								Uy = Conversions.bytes_2_signed_float2w65536(ref buffer,
								page_sh + rec_sh + 12) - uLinNom;
								Uy /= uLinNom / 100;
								U_AB = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 28) - uLinNom;
								U_AB /= uLinNom / 100;
								U_BC = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 32) - uLinNom;
								U_BC /= uLinNom / 100;
								U_CA = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 36) - uLinNom;
								U_CA /= uLinNom / 100;
								sql.Append(String.Format("INSERT INTO day_avg_parameters_t6 (datetime_id, event_datetime, d_u_y, d_u_ab, d_u_bc, d_u_ca) VALUES ({0}, '{1}', {2}, {3}, {4}, {5});\n",
									"currval('day_avg_parameter_times_datetime_id_seq')",
									event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
									Uy.ToString(ci),
									U_AB.ToString(ci),
									U_BC.ToString(ci),
									U_CA.ToString(ci)
									));
								break;

							case ConnectScheme.Ph1W2:
								U_A = Conversions.bytes_2_signed_float2w65536(ref buffer,
												page_sh + rec_sh + 16) - uPhNom;
								U_A /= uPhNom / 100;
								sql.Append(String.Format
									("INSERT INTO day_avg_parameters_t6 (datetime_id, event_datetime, d_u_a) VALUES ({0}, '{1}', {2});\n",
									"currval('day_avg_parameter_times_datetime_id_seq')",
									event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
									U_A.ToString(ci)
									));
								break;
						}
						event_datetime = event_datetime.AddSeconds(timeInterval);

						rec_sh += record_size; // переходим к следующей записи
					}
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_pqp_t6(): " + e.Message);
				throw e;
			}
		}

		private void xml2sql_pqp_t7(ref EmXmlPQP xmlPQP, ref StringBuilder sql,
			float fNom, string device_version, EmDeviceType devType)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				ushort timeInterval = 20;
				DateTime event_datetime = xmlPQP.Start.AddSeconds(timeInterval);
				byte[] buffer = xmlPQP.DataPages;

				int g_shift = 4096;

				int record_size = 44;
				if (devType == EmDeviceType.EM33T1)
					record_size = 48;
				else if (devType == EmDeviceType.EM33T)
					if (Constants.isNewDeviceVersion_EM33T(device_version))
						record_size = 48;

				float f1 = 0, f2 = 0, f3 = 0;

				// перебираем все страницы с данными о частоте и напряжениях
				for (int unfp = 0; unfp < xmlPQP.UnfPagesLength; unfp++)
				{
					// смещение начала текущей страницы с данными о частоте и напряжениях
					int page_sh = g_shift + unfp * 512;
					//смещение текущей записи
					int rec_sh = 4;
					// количество записей на данной странице
					int usnf_length = Conversions.bytes_2_ushort(ref buffer, page_sh + 2);

					// перебираем все записи на unfp-той странице
					for (int usnf = 0; usnf < usnf_length; usnf++)
					{
						f1 = Conversions.bytes_2_signed_float2w65536(ref buffer,
									page_sh + rec_sh + 0) - fNom;
						f2 = Conversions.bytes_2_signed_float2w65536(ref buffer,
									page_sh + rec_sh + 4) - fNom;
						f3 = Conversions.bytes_2_signed_float2w65536(ref buffer,
										page_sh + rec_sh + 8) - fNom;

						sql.Append(String.Format
							("INSERT INTO day_avg_parameters_t7 VALUES ({0}, '{1}', {2});\n",
							"currval('day_avg_parameter_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f1.ToString(ci)));
						event_datetime = event_datetime.AddSeconds(timeInterval);
						sql.Append(String.Format
							("INSERT INTO day_avg_parameters_t7 VALUES ({0}, '{1}', {2});\n",
							"currval('day_avg_parameter_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f2.ToString(ci)));
						event_datetime = event_datetime.AddSeconds(timeInterval);
						sql.Append(String.Format
							("INSERT INTO day_avg_parameters_t7 VALUES ({0}, '{1}', {2});\n",
							"currval('day_avg_parameter_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f3.ToString(ci)));
						event_datetime = event_datetime.AddSeconds(timeInterval);

						rec_sh += record_size; // переходим к следующей записи
					}
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_pqp_t6(): " + e.Message);
				throw e;
			}
		}

		private double getLongFliker(List<float> flik, short start, short end)
		{
			double sum = 0;
			for (int i = start; i < end; ++i)
				sum += Math.Pow(flik[i], 3);
			double res = Math.Pow(sum, 1.0 / 3.0);
			res /= 3.0;
			if (Double.IsNaN(res)) 
				res = 0;
			return res;
		}

		#endregion

		#region Building SQL query for inserting Events data from Device

		private bool xml2sql_dns_period(EmXmlDNS xmlDNS, ref EmSqlDataNode dnsNode)
		{
			try
			{
				dnsNode.SqlType = EmSqlDataNodeType.Events;

				dnsNode.Begin = xmlDNS.Start;
				dnsNode.End = xmlDNS.End;

				dnsNode.Sql = string.Format("INSERT INTO dips_and_overs_times (datetime_id, start_datetime, end_datetime, database_id) VALUES (DEFAULT, '{0}', '{1}', {2});\n",
					xmlDNS.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlDNS.End.ToString("MM.dd.yyyy HH:mm:ss"),
					"{0}");

				byte[] buffer = xmlDNS.DataPages;

				for (int i = 0; i < xmlDNS.DnsNum; i++)
				{
					int recAddress = 16 * i;

					// если записей оказалось меньше чем ожидалось, тем не менее 
					// делаем штатный выход
					if (recAddress + 15 > buffer.Length)
						return true;

					// выход в случае плохих данных
					// не делаю выход аварийным, так как он бывало часто срабатывал
					// а травмировать психику девственного пользователя непонятными
					// сообщениями типа "Все сохранено, но с ошибками!" 
					// задача неблагодарная.
					if (Conversions.bytes_2_ushort(ref buffer, recAddress) != 0)
						return true;

					// 2 байта признак
					byte info = Conversions.byte_2_DAA(buffer[recAddress + 2]);
					int event_type = info % 2;
					string phase = string.Empty;

					if (info < 2) phase = "A";
					else if (info < 4) phase = "B";
					else phase = "C";

					// вермя начала отклонения в отсчетах таймера
					uint startTimer = Conversions.bytes_2_uint(ref buffer, recAddress + 4);
					// длительность отклонения в отсчетах таймера
					uint lenghtTimer = Conversions.bytes_2_uint(ref buffer, recAddress + 12);
					// отклонение
					float deviation = (float)((float)Conversions.bytes_2_ushort(
						ref buffer, recAddress + 8) / 8192);
					if(event_type == 1 /*dip*/) deviation *= 100;
					// здесь надо прибавлять миллисекунды а у нас десятки миллисекнуд, 
					// поэтому умножаем на 10
					DateTime start = xmlDNS.Start.AddMilliseconds(startTimer * coeffTimer * 10);
					//long lLength = 
					DateTime end = start.AddMilliseconds(TrimDurationIn_ms(lenghtTimer * coeffTimer * 10));

					if (end > xmlDNS.End)
					{
						// DEBUG
						string dbg = string.Format(
							"Events. The end of the event exceed the end of the archive ({0} ms)!",
							((TimeSpan)(end - xmlDNS.End)).TotalMilliseconds);
						EmService.WriteToLogDebug(dbg);
						// END DEBUG
						end = xmlDNS.End;
					}

					dnsNode.Sql += string.Format("INSERT INTO dips_and_overs (datetime_id, event_type, phase, deviation, start_datetime, end_datetime) VALUES (currval('dips_and_overs_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}');\n",
						event_type,
						phase,
						deviation.ToString(new System.Globalization.CultureInfo("en-US")),
						start.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
						end.ToString("MM.dd.yyyy HH:mm:ss.FFFFF")
						);

					// set ProgressBar position
					cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
					bw_.ReportProgress((int)cur_percent_progress_);
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmSqlImageCreator33::xml2sql_dns_period():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		#endregion

		#region Building SQL query for inserting AVG data from Device

		private bool xml2sql_avg_period(EmXmlAVG xmlAVG, ConnectScheme connection_scheme, 
			float fnom, float unomln, float unomph, ref EmSqlDataNode avgNode)
		{
			try
			{
				avgNode.SqlType = EmSqlDataNodeType.AVG;

				avgNode.Begin = xmlAVG.Start;
				avgNode.End = xmlAVG.End;

				avgNode.Sql = string.Format("INSERT INTO period_avg_params_times (datetime_id, start_datetime, end_datetime, database_id, period_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3});\n",
					xmlAVG.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlAVG.End.ToString("MM.dd.yyyy HH:mm:ss"),
					"{0}",
					(int)xmlAVG.AvgType - 1);
				EmService.WriteToLogGeneral("AVG type = " + xmlAVG.AvgType);

				/////////////////////////////////////
				// парсим страницы Усредненных данных
				float fpages = (float)xmlAVG.DataPages.Length / Em33TDevice.bytes_per_page;
				int pages = (int)fpages;
				if (fpages != (float)pages) return false;

				// в гармониках для 3ф3пр нет 1-ой гармоники U лин (вместо нее лежит U ф), поэтому возьмем ее
				// из основной части архива
				float U1ab = 0, U1bc = 0, U1ca = 0;

				StringBuilder sbTemp = new StringBuilder();
				for (int iPage = 0; iPage < pages; iPage++)
				{
					byte[] buffer = new byte[Em33TDevice.bytes_per_page];
					Array.Copy(xmlAVG.DataPages, iPage * Em33TDevice.bytes_per_page,
						buffer, 0, Em33TDevice.bytes_per_page);
					int Page = (int)(buffer[0x07]);

					switch (Page)
					{
						case 0: xml2sql_avg_p1(ref buffer, connection_scheme, fnom,
										unomln, unomph, ref sbTemp, ref U1ab, ref U1bc, ref U1ca);
							break;
						// гармоники
						case 1: xml2sql_avg_p2(ref buffer, connection_scheme, ref sbTemp, U1ab, U1bc, U1ca);  
							break;
						// углы между гармониками
						case 2: xml2sql_avg_p3(ref buffer, connection_scheme, ref sbTemp);  
							break;
						// мощности гармоник
						case 3: xml2sql_avg_p4(ref buffer, connection_scheme, ref sbTemp);  
							break;
						case 4:
							break;
					}

					// set ProgressBar position
					cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
					bw_.ReportProgress((int)cur_percent_progress_);

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
				avgNode.Sql += sbTemp.ToString();

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmSqlImageCreator33::xml2sql_avg_period(): ");
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
		private void xml2sql_avg_p1(ref byte[] buffer, ConnectScheme connection_scheme,
			float fnom, float unomln, float unomph, ref StringBuilder sql, ref float U1ab, ref float U1bc, ref float U1ca)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				switch (connection_scheme)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:

						float pSum34 = 
							Conversions.bytes_2_signed_float3w65536(ref buffer, 316);
						float qSumGeom34 = 
							Conversions.bytes_2_signed_float3w65536(ref buffer, 340);
						float qSumShift34 =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 334);
						float qSumCross34 =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 328);

						// tangent P
						float tanPshift34 = 0;
						if (Math.Abs(pSum34) >= 0.05 && Math.Abs(qSumShift34) >= 0.05)
						{
							tanPshift34 = (pSum34 == 0 ? 0 : qSumShift34 / pSum34);
							tanPshift34 = (float)Math.Round((double)tanPshift34, 8);
						}
						float tanPgeom34 = 0;
						if (Math.Abs(pSum34) >= 0.05 && Math.Abs(qSumGeom34) >= 0.05)
						{
							tanPgeom34 = (pSum34 == 0 ? 0 : qSumGeom34 / pSum34);
							tanPgeom34 = (float)Math.Round((double)tanPgeom34, 8);
						}
						float tanPcross34 = 0;
						if (Math.Abs(pSum34) >= 0.05 && Math.Abs(qSumCross34) >= 0.05)
						{
							tanPcross34 = (pSum34 == 0 ? 0 : qSumCross34 / pSum34);
							tanPcross34 = (float)Math.Round((double)tanPcross34, 8);
						}

						sql.Append(String.Format
							("INSERT INTO period_avg_params_1_4 VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53}, {54}, {55}, {56}, {57}, {58}, {59}, {60}, {61}, {62}, {63}, {64}, {65}, {66}, {67}, {68}, {69}, {70}, {71}, {72}, {73}, {74}, {75}, {76}, {77}, {78}, {79}, {80}, {81}, {82}, {83}, {84}, {85}, {86}, {87});\n",
							"currval('period_avg_params_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							Conversions.bytes_2_float2w65536(ref buffer, 8).ToString(ci),	// f

							Conversions.bytes_2_float2w65536(ref buffer, 156).ToString(ci),	// U_A
							Conversions.bytes_2_float2w65536(ref buffer, 224).ToString(ci),	// U_B
							Conversions.bytes_2_float2w65536(ref buffer, 292).ToString(ci),	// U_C

							Conversions.bytes_2_float2w65536(ref buffer, 160).ToString(ci),	// U1_A
							Conversions.bytes_2_float2w65536(ref buffer, 228).ToString(ci),	// U1_B
							Conversions.bytes_2_float2w65536(ref buffer, 296).ToString(ci),	// U1_C

							Conversions.bytes_2_float2w65536(ref buffer, 164).ToString(ci),	// U_AB
							Conversions.bytes_2_float2w65536(ref buffer, 232).ToString(ci),	// U_BC
							Conversions.bytes_2_float2w65536(ref buffer, 300).ToString(ci),	// U_CA

							Conversions.bytes_2_float2w65536(ref buffer, 168).ToString(ci),	// U1_AB
							Conversions.bytes_2_float2w65536(ref buffer, 236).ToString(ci),	// U1_BC
							Conversions.bytes_2_float2w65536(ref buffer, 304).ToString(ci),	// U1_CA

							Conversions.bytes_2_signed_float2w65536(ref buffer, 176).ToString(ci),	// U0_A*
							Conversions.bytes_2_signed_float2w65536(ref buffer, 244).ToString(ci),	// U0_B
							Conversions.bytes_2_signed_float2w65536(ref buffer, 312).ToString(ci),	// U0_C

							Conversions.bytes_2_float2w65536(ref buffer, 172).ToString(ci),	// U_hp A
							Conversions.bytes_2_float2w65536(ref buffer, 240).ToString(ci),	// U_hp B
							Conversions.bytes_2_float2w65536(ref buffer, 308).ToString(ci),	// U_hp C

							Conversions.bytes_2_float2w65536(ref buffer, 40).ToString(ci),	// U_1 = U_у
							Conversions.bytes_2_float2w65536(ref buffer, 32).ToString(ci),	// U_2
							Conversions.bytes_2_float2w65536(ref buffer, 36).ToString(ci),	// U_0

							Conversions.bytes_2_float2w65536(ref buffer, 112).ToString(ci),	// I_A
							Conversions.bytes_2_float2w65536(ref buffer, 180).ToString(ci),	// I_B
							Conversions.bytes_2_float2w65536(ref buffer, 248).ToString(ci),	// I_C

							Conversions.bytes_2_float2w65536(ref buffer, 116).ToString(ci),	// I1_A
							Conversions.bytes_2_float2w65536(ref buffer, 184).ToString(ci),	// I1_B
							Conversions.bytes_2_float2w65536(ref buffer, 252).ToString(ci),	// I1_C

							Conversions.bytes_2_float2w65536(ref buffer, 20).ToString(ci),	// I_1
							Conversions.bytes_2_float2w65536(ref buffer, 24).ToString(ci),	// I_2
							Conversions.bytes_2_float2w65536(ref buffer, 28).ToString(ci),	// I_0

							pSum34.ToString(ci),	// P_Σ
							Conversions.bytes_2_signed_float3w65536(ref buffer, 124).ToString(ci),	// P_A(1)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 192).ToString(ci),	// P_B(2)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 260).ToString(ci),	// P_C

							Conversions.bytes_2_signed_float3w65536(ref buffer, 322).ToString(ci),	// S_Σ
							Conversions.bytes_2_signed_float3w65536(ref buffer, 130).ToString(ci),	// S_A
							Conversions.bytes_2_signed_float3w65536(ref buffer, 198).ToString(ci),	// S_B
							Conversions.bytes_2_signed_float3w65536(ref buffer, 266).ToString(ci),	// S_C

							qSumGeom34.ToString(ci),	// Q_Σ geom
							Conversions.bytes_2_signed_float3w65536(ref buffer, 148).ToString(ci),	// Q_A geom
							Conversions.bytes_2_signed_float3w65536(ref buffer, 216).ToString(ci),	// Q_B geom
							Conversions.bytes_2_signed_float3w65536(ref buffer, 284).ToString(ci),	// Q_C geom

							qSumShift34.ToString(ci),	// Q_Σ shift
							Conversions.bytes_2_signed_float3w65536(ref buffer, 142).ToString(ci),	// Q_A shift
							Conversions.bytes_2_signed_float3w65536(ref buffer, 210).ToString(ci),	// Q_B shift
							Conversions.bytes_2_signed_float3w65536(ref buffer, 278).ToString(ci),	// Q_C shift

							qSumCross34.ToString(ci),	// Q_Σ cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 136).ToString(ci),	// Q_A cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 204).ToString(ci),	// Q_B cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 272).ToString(ci),	// Q_C cross

							Conversions.bytes_2_signed_float4096(ref buffer, 364).ToString(ci),	// Kp_Σ
							Conversions.bytes_2_signed_float4096(ref buffer, 154).ToString(ci),	// Kp_A
							Conversions.bytes_2_signed_float4096(ref buffer, 222).ToString(ci),	// Kp_B
							Conversions.bytes_2_signed_float4096(ref buffer, 290).ToString(ci),	// Kp_C

							Conversions.bytes_2_signed_float128(ref buffer, 122).ToString(ci),	// ∟U1_A_ U1_B
							Conversions.bytes_2_signed_float128(ref buffer, 190).ToString(ci),	// ∟U1_B_ U1_C
							Conversions.bytes_2_signed_float128(ref buffer, 258).ToString(ci),	// ∟U1_C_ U1_A

							Conversions.bytes_2_signed_float128(ref buffer, 120).ToString(ci),	// ∟U1_A_ I1_A
							Conversions.bytes_2_signed_float128(ref buffer, 188).ToString(ci),	// ∟U1_B_ I1_B	
							Conversions.bytes_2_signed_float128(ref buffer, 256).ToString(ci),	// ∟U1_C_ I1_C

							Conversions.bytes_2_signed_float128(ref buffer, 366).ToString(ci),	// ∟U_y_ I_1
							Conversions.bytes_2_signed_float128(ref buffer, 368).ToString(ci),	// ∟U_2_ I_2
							Conversions.bytes_2_signed_float128(ref buffer, 370).ToString(ci),	// ∟U_0_ I_0	

							Conversions.bytes_2_signed_float3w65536(ref buffer, 358).ToString(ci),//P_0
							Conversions.bytes_2_signed_float3w65536(ref buffer, 346).ToString(ci),//P_1
							Conversions.bytes_2_signed_float3w65536(ref buffer, 352).ToString(ci),//P_2

							// ∆f = f - f_ном
							(Conversions.bytes_2_float2w65536(ref buffer, 8) - fnom).ToString(ci),	

							// δU_y = (U_y - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 40) - unomln) * 100 / 
							unomln).ToString(ci),
							// δU_A = (U1_A - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 160) - unomph) * 100 / 
							unomph).ToString(ci),
							// δU_B = (U1_B - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 228) - unomph) * 100 / 
							unomph).ToString(ci),
							// δU_C = (U1_C - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 296) - unomph) * 100 / 
							unomph).ToString(ci),
							// δU_AB = (U1_AB - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 168) - unomln) * 100 / 
							unomln).ToString(ci),
							// δU_BC = (U1_BC - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 236) - unomln) * 100 / 
							unomln).ToString(ci),
							// δU_CA = (U1_CA - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 304) - unomln) * 100 / 
							unomln).ToString(ci),

							(Conversions.bytes_2_float2w65536(ref buffer, 12) * 100).ToString(ci),	// K_2U
							(Conversions.bytes_2_float2w65536(ref buffer, 16) * 100).ToString(ci),	// K_0U

							Conversions.bytes_2_float1w65536_percent(ref buffer, 78).ToString(ci),	// K_UA(AB)
							Conversions.bytes_2_float1w65536_percent(ref buffer, 82).ToString(ci),	// K_UB(BC)
							Conversions.bytes_2_float1w65536_percent(ref buffer, 86).ToString(ci),	// K_UC(CA)

							Conversions.bytes_2_float1w65536_percent(ref buffer, 76).ToString(ci),	// K_IA
							Conversions.bytes_2_float1w65536_percent(ref buffer, 80).ToString(ci),	// K_IB
							Conversions.bytes_2_float1w65536_percent(ref buffer, 84).ToString(ci),	// K_IC
							tanPgeom34.ToString(ci),	// tangent P geom
							tanPshift34.ToString(ci),	// tangent P shift
							tanPcross34.ToString(ci)	// tangent P cross
							));
						break;

					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:

						U1ab = Conversions.bytes_2_float2w65536(ref buffer, 168);
						U1bc = Conversions.bytes_2_float2w65536(ref buffer, 236);
						U1ca = Conversions.bytes_2_float2w65536(ref buffer, 304);

						float pSum33 =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 316);
						float qSumGeom33 =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 340);
						float qSumShift33 =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 334);
						float qSumCross33 =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 328);

						// tangent P
						float tanPshift33 = 0;
						if (Math.Abs(pSum33) >= 0.05 && Math.Abs(qSumShift33) >= 0.05)
						{
							tanPshift33 = (pSum33 == 0 ? 0 : qSumShift33 / pSum33);
							tanPshift33 = (float)Math.Round((double)tanPshift33, 8);
						}
						float tanPgeom33 = 0;
						if (Math.Abs(pSum33) >= 0.05 && Math.Abs(qSumGeom33) >= 0.05)
						{
							tanPgeom33 = (pSum33 == 0 ? 0 : qSumGeom33 / pSum33);
							tanPgeom33 = (float)Math.Round((double)tanPgeom33, 8);
						}
						float tanPcross33 = 0;
						if (Math.Abs(pSum33) >= 0.05 && Math.Abs(qSumCross33) >= 0.05)
						{
							tanPcross33 = (pSum33 == 0 ? 0 : qSumCross33 / pSum33);
							tanPcross33 = (float)Math.Round((double)tanPcross33, 8);
						}

						sql.Append(String.Format("INSERT INTO period_avg_params_1_4 (datetime_id, event_datetime, f, u1_a, u1_b, u1_c, u_ab, u_bc, u_ca, u1_ab, u1_bc, u1_ca, u_1, u_2, i_a, i_b, i_c, i1_a, i1_b, i1_c, i_1, i_2, p_sum, p_a_1, p_b_2, s_sum, q_sum_geom, q_sum_shift, q_sum_cross, q_a_cross, q_b_cross, q_c_cross, kp_sum, an_u1_a_u1_b, an_u1_b_u1_c, an_u1_c_u1_a, an_u1_a_i1_a, an_u1_b_i1_b, an_u1_c_i1_c, an_u_1_i_1, an_u_2_i_2, p_1, p_2, d_f, d_u_y, d_u_ab, d_u_bc, d_u_ca, k_2u, k_ua_ab, k_ub_bc, k_uc_ca, k_ia, k_ib, k_ic, tangent_p_geom, tangent_p_shift, tangent_p_cross) VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53}, {54}, {55}, {56}, {57});\n",
							"currval('period_avg_params_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							Conversions.bytes_2_float2w65536(ref buffer, 8).ToString(ci),	// f

							Conversions.bytes_2_float2w65536(ref buffer, 160).ToString(ci),	// U1_A
							Conversions.bytes_2_float2w65536(ref buffer, 228).ToString(ci),	// U1_B
							Conversions.bytes_2_float2w65536(ref buffer, 296).ToString(ci),	// U1_C

							Conversions.bytes_2_float2w65536(ref buffer, 164).ToString(ci),	// U_AB
							Conversions.bytes_2_float2w65536(ref buffer, 232).ToString(ci),	// U_BC
							Conversions.bytes_2_float2w65536(ref buffer, 300).ToString(ci),	// U_CA

							Conversions.bytes_2_float2w65536(ref buffer, 168).ToString(ci),	// U1_AB
							Conversions.bytes_2_float2w65536(ref buffer, 236).ToString(ci),	// U1_BC
							Conversions.bytes_2_float2w65536(ref buffer, 304).ToString(ci),	// U1_CA	

							Conversions.bytes_2_float2w65536(ref buffer, 40).ToString(ci),	// U_1
							Conversions.bytes_2_float2w65536(ref buffer, 32).ToString(ci),	// U_2

							Conversions.bytes_2_float2w65536(ref buffer, 112).ToString(ci),	// I_A
							Conversions.bytes_2_float2w65536(ref buffer, 180).ToString(ci),	// I_B
							Conversions.bytes_2_float2w65536(ref buffer, 248).ToString(ci),	// I_C	  

							Conversions.bytes_2_float2w65536(ref buffer, 116).ToString(ci),	// I1_A
							Conversions.bytes_2_float2w65536(ref buffer, 184).ToString(ci),	// I1_B
							Conversions.bytes_2_float2w65536(ref buffer, 252).ToString(ci),	// I1_C

							Conversions.bytes_2_float2w65536(ref buffer, 20).ToString(ci),	// I_1
							Conversions.bytes_2_float2w65536(ref buffer, 24).ToString(ci),	// I_2  

							pSum33.ToString(ci),	//P_Σ
							Conversions.bytes_2_signed_float3w65536(ref buffer, 124).ToString(ci),	// P_A(1)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 260).ToString(ci),	// P_C(2)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 322).ToString(ci),	//S_Σ

							qSumGeom33.ToString(ci),	// Q_Σ geom
							qSumShift33.ToString(ci),	// Q_Σ shift
							qSumCross33.ToString(ci),	// Q_Σ cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 136).ToString(ci),	// Q_A cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 204).ToString(ci),	// Q_B cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 272).ToString(ci),	// Q_C cross

							Conversions.bytes_2_signed_float4096(ref buffer, 364).ToString(ci),	// Kp_Σ

							Conversions.bytes_2_signed_float128(ref buffer, 122).ToString(ci),	// ∟U1_A_ U1_B
							Conversions.bytes_2_signed_float128(ref buffer, 190).ToString(ci),	// ∟U1_B_ U1_C
							Conversions.bytes_2_signed_float128(ref buffer, 258).ToString(ci),	// ∟U1_C_ U1_A

							Conversions.bytes_2_signed_float128(ref buffer, 120).ToString(ci),	// ∟U1_A_ I1_A
							Conversions.bytes_2_signed_float128(ref buffer, 188).ToString(ci),	// ∟U1_B_ I1_B	
							Conversions.bytes_2_signed_float128(ref buffer, 256).ToString(ci),	// ∟U1_C_ I1_C

							Conversions.bytes_2_signed_float128(ref buffer, 366).ToString(ci),	// ∟U_y_ I_1
							Conversions.bytes_2_signed_float128(ref buffer, 368).ToString(ci),	// ∟U_2_ I_2

							Conversions.bytes_2_signed_float3w65536(ref buffer, 346).ToString(ci),//P_1
							Conversions.bytes_2_signed_float3w65536(ref buffer, 352).ToString(ci),//P_2

							(Conversions.bytes_2_float2w65536(ref buffer, 8) - fnom).ToString(ci),	// ∆f = f - f_ном
							// δU_y = (U_y - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 40) - unomln) * 100 / unomln).ToString(ci),
							// δU_AB = (U1_AB - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 168) - unomln) * 100 / unomln).ToString(ci),
							// δU_BC = (U1_BC - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 236) - unomln) * 100 / unomln).ToString(ci),
							// δU_CA = (U1_CA - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 304) - unomln) * 100 / unomln).ToString(ci),

							(Conversions.bytes_2_float2w65536(ref buffer, 12) * 100).ToString(ci),	// K_2U

							Conversions.bytes_2_float1w65536_percent(ref buffer, 78).ToString(ci),	// K_UA(AB)
							Conversions.bytes_2_float1w65536_percent(ref buffer, 82).ToString(ci),	// K_UB(BC)
							Conversions.bytes_2_float1w65536_percent(ref buffer, 86).ToString(ci),	// K_UC(CA)

							Conversions.bytes_2_float1w65536_percent(ref buffer, 76).ToString(ci),	// K_IA	
							Conversions.bytes_2_float1w65536_percent(ref buffer, 80).ToString(ci),	// K_IB
							Conversions.bytes_2_float1w65536_percent(ref buffer, 84).ToString(ci),	// K_IC
							tanPgeom33.ToString(ci),	// tangent P geom
							tanPshift33.ToString(ci),	// tangent P shift
							tanPcross33.ToString(ci)	// tangent P cross
							));
						break;

					case ConnectScheme.Ph1W2:

						float pA = 
							Conversions.bytes_2_signed_float3w65536(ref buffer, 124);
						float qAGeom =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 148);
						float qAShift =
							Conversions.bytes_2_signed_float3w65536(ref buffer, 142);

						// tangent P
						float tanPshift12 = 0;
						if (Math.Abs(pA) >= 0.05 && Math.Abs(qAShift) >= 0.05)
						{
							tanPshift12 = (pA == 0 ? 0 : qAShift / pA);
							tanPshift12 = (float)Math.Round((double)tanPshift12, 8);
						}
						float tanPgeom12 = 0;
						if (Math.Abs(pA) >= 0.05 && Math.Abs(qAGeom) >= 0.05)
						{
							tanPgeom12 = (pA == 0 ? 0 : qAGeom / pA);
							tanPgeom12 = (float)Math.Round((double)tanPgeom12, 8);
						}
					
						sql.Append(String.Format
							("INSERT INTO period_avg_params_1_4 (datetime_id, event_datetime, f, u_a, u1_a, u0_a, u_hp_a, u_1, i_a, i1_a, i_1, p_a_1, s_a, q_a_geom, q_a_shift, kp_a, an_u1_a_u1_b, an_u1_a_i1_a, an_u_1_i_1, p_1, d_f, d_u_a, k_ua_ab, k_ia, tangent_p_geom, tangent_p_shift, tangent_p_cross) VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26});\n",
							"currval('period_avg_params_times_datetime_id_seq')",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
							Conversions.bytes_2_float2w65536(ref buffer, 8).ToString(ci),	// f

							Conversions.bytes_2_float2w65536(ref buffer, 156).ToString(ci),	// U_A
							Conversions.bytes_2_float2w65536(ref buffer, 160).ToString(ci),	// U1_A
							Conversions.bytes_2_signed_float2w65536(ref buffer, 176).ToString(ci),	// U0_A
							Conversions.bytes_2_float2w65536(ref buffer, 172).ToString(ci),	// U_hp A

							Conversions.bytes_2_float2w65536(ref buffer, 40).ToString(ci),	// U_1

							Conversions.bytes_2_float2w65536(ref buffer, 112).ToString(ci),	// I_A
							Conversions.bytes_2_float2w65536(ref buffer, 116).ToString(ci),	// I1_A

							Conversions.bytes_2_float2w65536(ref buffer, 20).ToString(ci),	// I_1

							pA.ToString(ci),			// P_A(1)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 130).ToString(ci),	// S_A

							qAGeom.ToString(ci),		// Q_A geom
							qAShift.ToString(ci),	// Q_A shift

							Conversions.bytes_2_signed_float4096(ref buffer, 154).ToString(ci),	// Kp_A

							Conversions.bytes_2_signed_float128(ref buffer, 122).ToString(ci),	// ∟U1_A_ U1_B
							Conversions.bytes_2_signed_float128(ref buffer, 120).ToString(ci),	// ∟U1_A_ I1_A

							Conversions.bytes_2_signed_float128(ref buffer, 366).ToString(ci),	// ∟U_y_ I_1
							Conversions.bytes_2_signed_float3w65536(ref buffer, 346).ToString(ci),	// P_1
							// ∆f = f - f_ном
							(Conversions.bytes_2_float2w65536(ref buffer, 8) - fnom).ToString(ci),	
							// δU_A = (U1_A - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 160) - unomph) * 
								100 / unomph).ToString(ci),
							Conversions.bytes_2_float1w65536_percent(ref buffer, 78).ToString(ci),	// K_UA(AB)
							Conversions.bytes_2_float1w65536_percent(ref buffer, 76).ToString(ci),	//K_IA
							tanPgeom12.ToString(ci),	// tangent P geom
							tanPshift12.ToString(ci),	// tangent P shift
							0	// dummy for tangent P cross
							));
						break;
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_avg_p1(): " + e.Message);
				throw e;
			}
		}

		/// <summary>Гармоники</summary>
		private void xml2sql_avg_p2(ref byte[] buffer, ConnectScheme connection_scheme, 
									ref StringBuilder sql, float U1ab, float U1bc, float U1ca)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				if (connection_scheme != ConnectScheme.Ph1W2)
					sql.Append("INSERT INTO period_avg_params_5 VALUES (");
				else
					sql.Append("INSERT INTO period_avg_params_5 (datetime_id, event_datetime, u1_a_ab, k_ua_ab_2, k_ua_ab_3, k_ua_ab_4, k_ua_ab_5, k_ua_ab_6, k_ua_ab_7, k_ua_ab_8, k_ua_ab_9, k_ua_ab_10, k_ua_ab_11, k_ua_ab_12, k_ua_ab_13, k_ua_ab_14, k_ua_ab_15, k_ua_ab_16, k_ua_ab_17, k_ua_ab_18, k_ua_ab_19, k_ua_ab_20, k_ua_ab_21, k_ua_ab_22, k_ua_ab_23, k_ua_ab_24, k_ua_ab_25, k_ua_ab_26, k_ua_ab_27, k_ua_ab_28, k_ua_ab_29, k_ua_ab_30, k_ua_ab_31, k_ua_ab_32, k_ua_ab_33, k_ua_ab_34, k_ua_ab_35, k_ua_ab_36, k_ua_ab_37, k_ua_ab_38, k_ua_ab_39, k_ua_ab_40, i1_a, k_ia_2, k_ia_3, k_ia_4, k_ia_5, k_ia_6, k_ia_7, k_ia_8, k_ia_9, k_ia_10, k_ia_11, k_ia_12, k_ia_13, k_ia_14, k_ia_15, k_ia_16, k_ia_17, k_ia_18, k_ia_19, k_ia_20, k_ia_21, k_ia_22, k_ia_23, k_ia_24, k_ia_25, k_ia_26, k_ia_27, k_ia_28, k_ia_29, k_ia_30, k_ia_31, k_ia_32, k_ia_33, k_ia_34, k_ia_35, k_ia_36, k_ia_37, k_ia_38, k_ia_39, k_ia_40) VALUES (");

				// adding pkey
				sql.Append("currval('period_avg_params_times_datetime_id_seq'), '");
				sql.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

				// U1_A(AB)
				// для 3ф3пр берем линейную гармонику, а для остальных текущую, т.е. фазную
				if(connection_scheme != ConnectScheme.Ph3W3 && connection_scheme != ConnectScheme.Ph3W3_B_calc)
					U1ab = Conversions.bytes_2_float2w65536(ref buffer, 492);
				sql.Append(U1ab.ToString(ci) + ", ");

				for (int i = 0; i < 39; i++)	// K_UA(AB)_2..K_UA(AB)_40
					sql.Append(
						(Conversions.bytes_2_float1w65536_percent(ref buffer, 88 + i * 2)).ToString(ci) +
						", ");

				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					// U1_B(BC)
					// для 3ф3пр берем линейную гармонику, а для остальных текущую, т.е. фазную
					if (connection_scheme != ConnectScheme.Ph3W3 && connection_scheme != ConnectScheme.Ph3W3_B_calc)
						U1bc = Conversions.bytes_2_float2w65536(ref buffer, 500);
					sql.Append(U1bc.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UB(BC)_2..K_UB(BC)_40
						sql.Append(
							(Conversions.bytes_2_float1w65536_percent(ref buffer, 248 + i * 2)).ToString(ci) 
							+ ", ");
					// U1_C(CA)
					// для 3ф3пр берем линейную гармонику, а для остальных текущую, т.е. фазную
					if (connection_scheme != ConnectScheme.Ph3W3 && connection_scheme != ConnectScheme.Ph3W3_B_calc)
						U1ca = Conversions.bytes_2_float2w65536(ref buffer, 508);
					sql.Append(U1ca.ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_UC(CA)_2..K_UC(CA)_40
						sql.Append(
							(Conversions.bytes_2_float1w65536_percent(ref buffer, 408 + i * 2)).ToString(ci) 
							+ ", ");
				}

				// I1_A
				sql.Append((Conversions.bytes_2_float2w65536(ref buffer, 488)).ToString(ci) + ", ");
				for (int i = 0; i < 39; i++)	// K_IA_2..K_IA_40
					sql.Append(
						(Conversions.bytes_2_float1w65536_percent(ref buffer, 8 + i * 2)).ToString(ci) 
						+ ", ");

				if (connection_scheme != ConnectScheme.Ph1W2)
				{
					// I1_B
					sql.Append((Conversions.bytes_2_float2w65536(ref buffer, 496)).ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IB_2..K_IB_40
						sql.Append(
							(Conversions.bytes_2_float1w65536_percent(ref buffer, 168 + i * 2)).ToString(ci) 
							+ ", ");
					// I1_C
					sql.Append((Conversions.bytes_2_float2w65536(ref buffer, 504)).ToString(ci) + ", ");
					for (int i = 0; i < 39; i++)	// K_IC_2..K_IC_40
						sql.Append(
							(Conversions.bytes_2_float1w65536_percent(ref buffer, 328 + i * 2)).ToString(ci) 
							+ ", ");
				}

				// удаляем последний пробел с запятой и добавляем окончание строки)
				sql.Remove(sql.Length - 2, 2);
				sql.Append(");\n");
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in EmSqlImageCreator33::xml2sql_avg_p2(): " + 
					e.Message);
				throw e;
			}
		}

		/// <summary>Углы между гармониками</summary>
		private void xml2sql_avg_p3(ref byte[] buffer, ConnectScheme connection_scheme, 
									ref StringBuilder sql)
		{
			try
			{
				if (connection_scheme == ConnectScheme.Ph3W3 ||
					connection_scheme == ConnectScheme.Ph3W3_B_calc)  // для 3ф3пр этих данных нет
					return;

				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				if (connection_scheme != ConnectScheme.Ph1W2)
					sql.Append("INSERT INTO period_avg_params_6b VALUES (");
				else
					sql.Append("INSERT INTO period_avg_params_6b (datetime_id, event_datetime, an_u_a_1_i_a_1, an_u_a_2_i_a_2, an_u_a_3_i_a_3, an_u_a_4_i_a_4, an_u_a_5_i_a_5, an_u_a_6_i_a_6, an_u_a_7_i_a_7, an_u_a_8_i_a_8, an_u_a_9_i_a_9, an_u_a_10_i_a_10, an_u_a_11_i_a_11, an_u_a_12_i_a_12, an_u_a_13_i_a_13, an_u_a_14_i_a_14, an_u_a_15_i_a_15, an_u_a_16_i_a_16, an_u_a_17_i_a_17, an_u_a_18_i_a_18, an_u_a_19_i_a_19, an_u_a_20_i_a_20, an_u_a_21_i_a_21, an_u_a_22_i_a_22, an_u_a_23_i_a_23, an_u_a_24_i_a_24, an_u_a_25_i_a_25, an_u_a_26_i_a_26, an_u_a_27_i_a_27, an_u_a_28_i_a_28, an_u_a_29_i_a_29, an_u_a_30_i_a_30, an_u_a_31_i_a_31, an_u_a_32_i_a_32, an_u_a_33_i_a_33, an_u_a_34_i_a_34, an_u_a_35_i_a_35, an_u_a_36_i_a_36, an_u_a_37_i_a_37, an_u_a_38_i_a_38, an_u_a_39_i_a_39, an_u_a_40_i_a_40) VALUES (");

				// adding pkey
				sql.Append("currval('period_avg_params_times_datetime_id_seq'), '");
				sql.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

				for (int i = 0; i < 40; i++)	// ∟U_A 1_ I_A 1..∟U_A 1_ I_A 40
					sql.Append((Conversions.bytes_2_signed_float128(ref buffer, 32 + i * 2)).ToString(ci) + ", ");

				if (connection_scheme != ConnectScheme.Ph1W2) 
				{
					for (int i = 0; i < 40; i++)	// ∟U_B 1_ I_B 1..∟UBA 1_ I_B 40
						sql.Append((Conversions.bytes_2_signed_float128(ref buffer, 112 + i * 2)).ToString(ci) + ", ");
					for (int i = 0; i < 40; i++)	// ∟U_C 1_ I_C 1..∟U_C 1_ I_C 40
						sql.Append((Conversions.bytes_2_signed_float128(ref buffer, 192 + i * 2)).ToString(ci) + ", ");
				}

				// удаляем последний пробел с запятой и добавляем окончание строки)
				sql.Remove(sql.Length - 2, 2);
				sql.Append(");\n");
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_avg_p3(): " + e.Message);
				throw e;
			}
		}

		/// <summary>Мощности гармоник</summary>
		private void xml2sql_avg_p4(ref byte[] buffer, ConnectScheme connection_scheme, 
									ref StringBuilder sql)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				if (connection_scheme != ConnectScheme.Ph1W2)
					sql.Append("INSERT INTO period_avg_params_6a VALUES (");
				else
					sql.Append("INSERT INTO period_avg_params_6a (datetime_id, event_datetime, p_sum, p_a_1, p_a_1_1, p_a_1_2, p_a_1_3, p_a_1_4, p_a_1_5, p_a_1_6, p_a_1_7, p_a_1_8, p_a_1_9, p_a_1_10, p_a_1_11, p_a_1_12, p_a_1_13, p_a_1_14, p_a_1_15, p_a_1_16, p_a_1_17, p_a_1_18, p_a_1_19, p_a_1_20, p_a_1_21, p_a_1_22, p_a_1_23, p_a_1_24, p_a_1_25, p_a_1_26, p_a_1_27, p_a_1_28, p_a_1_29, p_a_1_30, p_a_1_31, p_a_1_32, p_a_1_33, p_a_1_34, p_a_1_35, p_a_1_36, p_a_1_37, p_a_1_38, p_a_1_39, p_a_1_40) VALUES (");

				// adding pkey
				sql.Append("currval('period_avg_params_times_datetime_id_seq'), '");
				sql.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

				// P_Σ 
				sql.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 488).ToString(ci)) + ", ");
				// P_A(1)
				sql.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 494).ToString(ci)) + ", ");

				// P_B(2)
				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					sql.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 500).ToString(ci)) + 
						", ");// P_B	
				}
				if (connection_scheme == ConnectScheme.Ph3W3 ||
					connection_scheme == ConnectScheme.Ph3W3_B_calc) 
				{
					sql.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 500).ToString(ci)) + 
						", ");// P_2
				}

				// P_C
				if (connection_scheme == ConnectScheme.Ph3W4 ||
					connection_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					sql.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 506).ToString(ci)) + 
						", ");// P_C
				}
				if (connection_scheme == ConnectScheme.Ph3W3 ||
					connection_scheme == ConnectScheme.Ph3W3_B_calc) // а для 3ф3пр забиваем NULL
				{
					sql.Append("NULL, ");
				}

				for (int i = 0; i < 40; i++)	// P_A(1) 1..P_A(1) 40
					sql.Append((Conversions.bytes_2_signed_float2w1024(ref buffer, 8 + i * 4)).ToString(ci) + 
						", ");

				if (connection_scheme != ConnectScheme.Ph1W2) 
				{
					for (int i = 0; i < 40; i++)	// P_B(2) 1..P_B(2) 40
						sql.Append((Conversions.bytes_2_signed_float2w1024(ref buffer, 168 + i * 4)).ToString(ci) 
							+ ", ");
					for (int i = 0; i < 40; i++)	// P_C 1..P_C 40
						sql.Append((Conversions.bytes_2_signed_float2w1024(ref buffer, 328 + i * 4)).ToString(ci) 
							+ ", ");
				}

				// удаляем последний пробел с запятой и добавляем окончание строки =)
				sql.Remove(sql.Length - 2, 2);
				sql.Append(");\n");
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed(
					"Error in EmSqlImageCreator33::xml2sql_avg_p4(): " + e.Message);
				throw e;
			}
		}

		#endregion

		#endregion

		#region Service functions
		
		private bool MakeTimerCoefficient(ref EmXmlArchive xmlArhive)
		{
			try
			{
				double dblMilliseconds =
					((TimeSpan)(xmlArhive.CommonEnd - xmlArhive.CommonBegin)).TotalMilliseconds;

				if (xmlArhive.DnsTimer == 0)
					coeffTimer = 1;
				else
				{
					try
					{
						if (dblMilliseconds > dblMaxDnoPeriodInMs)
						{
							coeffTimer = dblMilliseconds / (xmlArhive.DnsTimer * 10);
						}
						else
						{
							coeffTimer = 1;
						}
					}
					catch
					{
						coeffTimer = 1;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in MakeTimerCoefficient(): " + ex.Message);
				return false;
			}
			return true;
		}
		
		private long TrimDuration(long duration)
		{
			try
			{
				if ((duration % 100) != 0)
				{
					duration -= (duration % 100);
				}

				return duration;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in TrimDuration(): " + ex.Message);
				return -1;
			}
		}
		double TrimDurationIn_ms(double duration)
		{
			return Math.Truncate(duration * 100) / 100;
		}

		#endregion
	}
}
