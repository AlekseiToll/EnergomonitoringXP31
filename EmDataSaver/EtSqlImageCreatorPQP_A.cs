using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using EmDataSaver.XmlImage;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.SqlImage;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.ComponentModel;

using DbServiceLib;
using DeviceIO;
using EmServiceLib;
using EmServiceLib.SavingInterface;

namespace EmDataSaver
{
	public class EtSqlImageCreatorPQP_A : EmSqlImageCreatorBase
	{
		enum HARM_POWER
		{
			P = 0,
			Q = 1,
			ANGLE = 2
		}

		#region Fields

		// текущий предел по току (точнее множитель на который надо умножать ток и его производные.
		// если предел <= 10, то множитель равен 1, если предел 100, то множитель 10 и т.д.)
		//private float cur_current_limit_ = 1.0F;

		private BackgroundWorker bw_ = null;

		private EtPQP_A_XmlDeviceImage xmlImage_;
		private EtPQP_A_SqlDeviceImage sqlImage_;

		#endregion

		#region Properties

		public EtPQP_A_SqlDeviceImage SqlImage
		{
			get { return sqlImage_; }
		}

		/// <summary>Gets image file extention</summary>
        public static string ImageFileExtention
        {
            get { return "etPQP_A.xml"; }
        }
		
		/// <summary>Gets image files filter string for open/save dialogs</summary>
		public static string ImageFilter
		{
			get
			{
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					return String.Format(
						"Файлы Образа ET-PQP-A (*.{0})|*.{0}", ImageFileExtention);
				else
					return String.Format(
						"ET-PQP-A Image files (*.{0})|*.{0}", ImageFileExtention);
			}
		}

		#endregion

		#region Constructors

		public EtSqlImageCreatorPQP_A(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			ref EtPQP_A_XmlDeviceImage xmlImage)
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
					throw new EmException("EtDataSaverPQP::CreateSqlImage: xmlImage == null");

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
						cnt_steps_progress_ += (uint)xmlImage_.ArchiveList[i].ArchiveDNS.Length;
					//avg
					if (xmlImage_.ArchiveList[i].ArchiveAVG != null)
					{
						for (int iAvg = 0; iAvg < xmlImage_.ArchiveList[i].ArchiveAVG.Length; ++iAvg)
						{
							cnt_steps_progress_ +=
								(uint)xmlImage_.ArchiveList[i].ArchiveAVG[iAvg].DataPages.Length /
										EtPQP_A_Device.AvgRecordLength_PQP_A;
						}
					}
				}
				// делаем ProgressBar с запасом, иначе на последних шагах он 
				// долго висит заполненный
				cnt_steps_progress_ += 2;
				//////////////////////////////
				#endregion

				// формируем SQL/XML образ
				sqlImage_ = new EtPQP_A_SqlDeviceImage();
				return xml2sql();
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in SaveFromDevice()");
				Thread.ResetAbort();
				return false;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in SaveFromDevice(): " + emx.Message);
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SaveFromDevice() 2:");
				throw;
			}
		}

		#endregion

		#region Private SQL Image methods

		private bool xml2sql()
		{
			try
			{
				EmService.WriteToLogGeneral("xml2sql(): " + DateTime.Now.ToString());

				sqlImage_.SerialNumber = xmlImage_.SerialNumber;
				sqlImage_.Version = xmlImage_.Version;

				// SQL запрос, который вставит в таблицу devices 
				// запись об устройстве (если ее еще нет) и вернет dev_id этого устройства
				sqlImage_.Sql = string.Format("select insert_new_device({0}, null, '{1}');\n",
					sqlImage_.SerialNumber,
					sqlImage_.Version);

				string folder_name = string.Format("ETPQP-A # {0}", sqlImage_.SerialNumber);

				// SQL запрос, который вставит в таблицу folders 
				// запись об устройстве (если ее еще нет) и вернет folder_id этой папки
				sqlImage_.Sql += String.Format("select insert_dev_folder({0}, '{1}');",
											sqlImage_.SerialNumber, folder_name);

				sqlImage_.Registrations = new EmSqlRegistration[xmlImage_.ArchiveList.Length];
				for (int i = 0; i < xmlImage_.ArchiveList.Length; i++)
				{
					sqlImage_.Registrations[i] = new EmSqlRegistration();

					bool res = xml2sql_archive(
						ref xmlImage_.ArchiveList[i],
						xmlImage_.SerialNumber,
						//sqlImage_.Version,
						sqlImage_.T_fliker,
						sqlImage_.DeviceId,
						ref sqlImage_.Registrations[i]);
					if (!res) return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtSqlImageCreatorPQP_A::xml2sql(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				throw;
			}
		}

		private bool xml2sql_archive(ref EtPQP_A_XmlArchive xmlArhive, long ser_number, 
				short t_fliker, Int64 devId, ref EmSqlRegistration sqlArchive)
		{
			try
			{
				sqlArchive.RegistrationId = xmlArhive.RegistrationId;
				sqlArchive.ObjectName = xmlArhive.ObjectName;
				sqlArchive.CommonBegin = xmlArhive.CommonBegin;
				sqlArchive.CommonEnd = xmlArhive.CommonEnd;
				sqlArchive.ConnectionScheme = xmlArhive.ConnectionScheme;
				sqlArchive.ConstraintType = xmlArhive.ConstraintType;
				sqlArchive.DeviceVersion = xmlArhive.DevVersion;
				sqlArchive.DevVersionDate = xmlArhive.DevVersionDate;

				sqlArchive.SysInfo = xmlArhive.SysInfo.Clone();

				// SQL запрос, который вставит в таблицу registrations 
				// запись об регистрации (если ее еще нет) и вернет reg_id этой регистрации
				sqlArchive.Sql = string.Format("select insert_new_registration('{0}', '{1}', cast({2} as int2), {3}, {4}, {5}, cast({6} as int8), '{7}', null, {8}, {9}, '{10}', cast({11} as int2), cast({12} as int8), cast({13} as int2), {14});\n",
					sqlArchive.CommonBegin.ToString("MM.dd.yyyy HH:mm:ss"),
					sqlArchive.CommonEnd.ToString("MM.dd.yyyy HH:mm:ss"),
					(short)sqlArchive.ConnectionScheme,  // 2
					sqlArchive.SysInfo.F_Nominal.ToString(new CultureInfo("en-US")),
					sqlArchive.SysInfo.U_NominalLinear.ToString(new CultureInfo("en-US")),
					sqlArchive.SysInfo.U_NominalPhase.ToString(new CultureInfo("en-US")),	// u_nom_ph
					"{0}",  // 6  dev_id
					sqlArchive.ObjectName,
					sqlArchive.SysInfo.U_Limit.ToString(new CultureInfo("en-US")),	// u_limit
					sqlArchive.SysInfo.I_Limit.ToString(new CultureInfo("en-US")),	// i_limit  9
					sqlArchive.DeviceVersion,
					t_fliker,
					sqlArchive.RegistrationId,  // 13
					sqlArchive.ConstraintType,  // 18
					sqlArchive.SysInfo.I_NominalPhase.ToString(new CultureInfo("en-US"))	// i_nom_ph
					);

				sqlArchive.Sql += string.Format("UPDATE registrations SET u_transformer_enable = {0}, u_transformer_type = {1}, u_consistent = {2}, i_sensor_type = {3}, i_transformer_usage = {4}, i_transformer_primary = {5}, synchro_zero_enable = {6}, autocorrect_time_gps_enable = {7}, electro_system = {8}, pqp_length = {9}, f_limit = {10}, start_mode = {11}, registration_stop_cnt = {12}, pqp_cnt = {13}, cnt_dip = {14}, cnt_swell = {15}, cnt_interrupt = {16}, gps_latitude = {17}, gps_longitude = {18}, marked_on_off = {19} WHERE reg_id = currval('registrations_reg_id_seq');",
					sqlArchive.SysInfo.U_transformer_enable,
					sqlArchive.SysInfo.U_transformer_type,
					sqlArchive.SysInfo.U_consistent,
					sqlArchive.SysInfo.I_sensor_type,
					sqlArchive.SysInfo.I_transformer_usage,
					sqlArchive.SysInfo.I_transformer_primary,
					sqlArchive.SysInfo.Synchro_zero_enable,
					sqlArchive.SysInfo.Autocorrect_time_gps_enable,
					sqlArchive.SysInfo.Electro_system,
					sqlArchive.SysInfo.Pqp_length,
					sqlArchive.SysInfo.F_Limit,
					sqlArchive.SysInfo.Start_mode,
					sqlArchive.SysInfo.Registration_stop_cnt,
					sqlArchive.SysInfo.Pqp_cnt,
					sqlArchive.SysInfo.Cnt_dip,
					sqlArchive.SysInfo.Cnt_swell,
					sqlArchive.SysInfo.Cnt_interrupt,
					Math.Round(sqlArchive.SysInfo.Gps_Latitude, 6).ToString(new CultureInfo("en-US")),
					Math.Round(sqlArchive.SysInfo.Gps_Longitude, 6).ToString(new CultureInfo("en-US")),
					sqlArchive.SysInfo.Marked_on_off
					);

				#region Insert constraints

				sqlArchive.Constraints = new DeviceIO.Constraints.EtPQP_A_ConstraintsDetailed(xmlArhive.Constraints);

				sqlArchive.Sql += string.Format("UPDATE registrations SET sets_f_synchro_down95 = {0}, sets_f_synchro_down100 = {1}, sets_f_synchro_up95 = {2}, sets_f_synchro_up100 = {3}, sets_f_isolate_down95 = {4}, sets_f_isolate_down100 = {5}, sets_f_isolate_up95 = {6}, sets_f_isolate_up100 = {7}, sets_u_deviation_up95 = {8}, sets_u_deviation_up100 = {9}, sets_u_deviation_down95 = {8}, sets_u_deviation_down100 = {9}, sets_flick_short_down95 = {10}, sets_flick_short_down100 = {11}, sets_flick_long_up95 = {12}, sets_flick_long_up100 = {13} WHERE reg_id = currval('registrations_reg_id_seq');",
					xmlArhive.Constraints[0].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[1].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[2].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[3].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[4].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[5].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[6].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[7].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[8].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[9].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[10].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[11].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[12].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[13].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[14].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[15].ToString(new CultureInfo("en-US")));
				string tmpConstr = "UPDATE registrations SET ";
				for (int iConstr = 2; iConstr <= 40; ++iConstr)
				{
					tmpConstr += "sets_k_harm_95_" + iConstr.ToString();
					tmpConstr += string.Format(" = {0}, ", 
						xmlArhive.Constraints[iConstr + 14].ToString(new CultureInfo("en-US")));
				}
				for (int iConstr = 2; iConstr <= 40; ++iConstr)
				{
					tmpConstr += "sets_k_harm_100_" + iConstr.ToString();
					tmpConstr += string.Format(" = {0}, ",
						xmlArhive.Constraints[iConstr + 14 + 39].ToString(new CultureInfo("en-US")));
				}
				tmpConstr += string.Format("sets_k_harm_total_95 = {0}, sets_k_harm_total_100 = {1}, sets_k2u_95 = {2}, sets_k2u_100 = {3}, sets_k0u_95 = {4}, sets_k0u_100 = {5} WHERE reg_id = currval('registrations_reg_id_seq');\n",
					xmlArhive.Constraints[94].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[95].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[96].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[97].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[98].ToString(new CultureInfo("en-US")),
					xmlArhive.Constraints[99].ToString(new CultureInfo("en-US")));

				sqlArchive.Sql += tmpConstr;

				#endregion

				string folder_name = string.Format("ETPQP-A # {0} # {1}", ser_number, sqlArchive.ObjectName);

				// SQL запрос, который вставит в таблицу folders 
				// запись об объекте (если ее еще нет) и вернет folder_id этой папки
				sqlArchive.Sql += String.Format(
					"select insert_registration_folder({0}, {1}, '{2}');",
					"{0}",   // device_id
					"{1}",   // object_id
					folder_name);

				List<EmSqlDataNode> sqlDataTopList = new List<EmSqlDataNode>();
				bool res = true;

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
							res = xml2sql_pqp_period(ref xmlArhive.ArchivePQP[i], ref pqpNode,
								ref xmlArhive);
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
				sqlArchive.DataPQP = new EmSqlDataNode[sqlDataTopList.Count];
				sqlDataTopList.CopyTo(sqlArchive.DataPQP);
				sqlDataTopList.Clear();

				if (xmlArhive.ArchiveDNS != null && xmlArhive.ArchiveDNS.Length > 0)
				{
					for (int iDns = 0; iDns < xmlArhive.ArchiveDNS.Length; iDns++)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}

						EmSqlDataNode dnsNode = new EmSqlDataNode();
						res = xml2sql_dns_period(xmlArhive.ArchiveDNS[iDns], ref dnsNode, 
													sqlArchive.ConnectionScheme);

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
				sqlArchive.DataDNS = new EmSqlDataNode[sqlDataTopList.Count];
				sqlDataTopList.CopyTo(sqlArchive.DataDNS);
				sqlDataTopList.Clear();

				if (xmlArhive.ArchiveAVG != null && xmlArhive.ArchiveAVG.Length > 0)
				{
					for (int iAvg = 0; iAvg < xmlArhive.ArchiveAVG.Length; iAvg++)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}

						EmSqlDataNode avgNode = new EmSqlDataNode();
						res = xml2sql_avg_period(xmlArhive.ArchiveAVG[iAvg], 
							ref xmlArhive, ref avgNode);

						if (res) sqlDataTopList.Add(avgNode);
						else
						{
							EmService.WriteToLogFailed(
								"xml2sql_avg_period() didn't create any archive!");
						}
					}
				}
				sqlArchive.DataAVG = new EmSqlDataNode[sqlDataTopList.Count];
				sqlDataTopList.CopyTo(sqlArchive.DataAVG);
				sqlDataTopList.Clear();

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_archive():  ");
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

		private bool xml2sql_pqp_period(ref EmXmlPQP_PQP_A xmlPQP, ref EmSqlDataNode pqpNode,
											ref EtPQP_A_XmlArchive xmlArchive)
		{
			try
			{
				pqpNode.SqlType = EmSqlDataNodeType.PQP;
				pqpNode.Begin = xmlPQP.Start;
				pqpNode.End = xmlPQP.End;

				ConnectScheme con_scheme = xmlArchive.ConnectionScheme;
				float[] constraints = xmlArchive.Constraints;

				///////////////////////////////////////
				// парсим по необходимости страницы ПКЭ
				byte[] buffer = xmlPQP.DataPages;

				if (xmlPQP.StandardSettingsType < 0 || xmlPQP.StandardSettingsType > 5)
				{
					EmService.WriteToLogFailed("EtPQP-A: xml2sql_pqp_period: sets type set to 0! "
						+ xmlPQP.StandardSettingsType.ToString());
					xmlPQP.StandardSettingsType = 0;
				}

				uint arcive_id = Conversions.bytes_2_uint_new(ref buffer, 0);
				// на выходе имеем SQL запрос добавления записи 
				// периода ПКЭ а также всех дочерних данных по ПКЭ,
				// имеющий 4 недостающих параметра - object_id, device_id, folder_year_id, folder_month_id
				pqpNode.Sql += string.Format("INSERT INTO pqp_times (datetime_id, start_datetime, end_time, constraint_type, registration_id, device_id, parent_folder_id, con_scheme, f_nom, u_nom_lin, u_nom_ph, archive_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});\n",
					xmlPQP.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlPQP.End.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlPQP.StandardSettingsType,
					"{0}",
					"{1}",
					"{2}",
					(short)con_scheme,
					xmlArchive.SysInfo.F_Nominal.ToString(new CultureInfo("en-US")),
					xmlArchive.SysInfo.U_NominalLinear.ToString(new CultureInfo("en-US")),
                    xmlArchive.SysInfo.U_NominalPhase.ToString(new CultureInfo("en-US")),
					arcive_id);

				StringBuilder sql = new StringBuilder();

				xml2sql_pqp_f(ref buffer, ref sql, arcive_id, con_scheme, ref constraints);	// ∆f

				xml2sql_pqp_nonsymmetry(ref buffer, ref sql, arcive_id, con_scheme, ref constraints);

				if (con_scheme == ConnectScheme.Ph1W2)
				{
					xml2sql_pqp_du(ref buffer, ref sql, 1003, arcive_id, con_scheme, ref constraints); // dU A+
					xml2sql_pqp_du(ref buffer, ref sql, 1007, arcive_id, con_scheme, ref constraints); // dU A-
				}
				else if (con_scheme == ConnectScheme.Ph3W4 ||
					con_scheme == ConnectScheme.Ph3W4_B_calc)
				{
					xml2sql_pqp_du(ref buffer, ref sql, 1003, arcive_id, con_scheme, ref constraints); // dU A+
					xml2sql_pqp_du(ref buffer, ref sql, 1007, arcive_id, con_scheme, ref constraints); // dU A-
					xml2sql_pqp_du(ref buffer, ref sql, 1004, arcive_id, con_scheme, ref constraints); // dU B+
					xml2sql_pqp_du(ref buffer, ref sql, 1008, arcive_id, con_scheme, ref constraints); // dU B-
					xml2sql_pqp_du(ref buffer, ref sql, 1005, arcive_id, con_scheme, ref constraints); // dU C+
					xml2sql_pqp_du(ref buffer, ref sql, 1009, arcive_id, con_scheme, ref constraints); // dU C-
				}
				else if (con_scheme == ConnectScheme.Ph3W3 ||
					con_scheme == ConnectScheme.Ph3W3_B_calc)
				{
					xml2sql_pqp_du(ref buffer, ref sql, 1014, arcive_id, con_scheme, ref constraints); // dU AB+
					xml2sql_pqp_du(ref buffer, ref sql, 1017, arcive_id, con_scheme, ref constraints); // dU AB-
					xml2sql_pqp_du(ref buffer, ref sql, 1015, arcive_id, con_scheme, ref constraints); // dU BC+
					xml2sql_pqp_du(ref buffer, ref sql, 1018, arcive_id, con_scheme, ref constraints); // dU BC-
					xml2sql_pqp_du(ref buffer, ref sql, 1016, arcive_id, con_scheme, ref constraints); // dU CA+
					xml2sql_pqp_du(ref buffer, ref sql, 1019, arcive_id, con_scheme, ref constraints); // dU CA-
				}

				// dip and swell
				DEVICE_VERSIONS newDipSwellMode = Constants.isNewDeviceVersion_ETPQP_A(xmlArchive.DevVersion);
				xml2sql_pqp_dip_swell(ref buffer, ref sql, arcive_id, newDipSwellMode);

				// K_UA(2..40), K_UB(2..40), K_UC(2..40) а также K_UA, K_UB и K_UC
				xml2sql_pqp_nonsinus(ref buffer, ref sql, arcive_id, con_scheme, ref constraints);

				xml2sql_pqp_interharm_u(ref buffer, ref sql, arcive_id, con_scheme, ref xmlArchive);

				// fliker
				xml2sql_pqp_flicker(ref buffer, ref sql, arcive_id, con_scheme, ref constraints);
				xml2sql_pqp_flick_val(ref buffer, ref sql, con_scheme, xmlPQP.Start, xmlPQP.End);

				xml2sql_pqp_df_du_val(ref buffer, ref sql, con_scheme, xmlPQP.Start, xmlPQP.End);

				pqpNode.Sql += sql.ToString();

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_pqp_period():  ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		private bool xml2sql_pqp_nonsymmetry(ref byte[] buffer, ref StringBuilder SqlQuery,
												uint archive_id, ConnectScheme con_scheme, 
												ref float[] constraints)
		{
			try
			{
				//if (con_scheme != ConnectScheme.Ph1W2)
				//{
				//    xml2sql_pqp_t1(ref buffer, ref sql, 1101); // K_2U
				//}
				//if (con_scheme == ConnectScheme.Ph3W4 ||
				//    con_scheme == ConnectScheme.Ph3W4_B_calc)
				//{
				//    xml2sql_pqp_t1(ref buffer, ref sql, 1102); // K_0U
				//}

				CultureInfo ci_enUS = new CultureInfo("en-US");

				ushort statisticValid = Conversions.bytes_2_ushort(ref buffer, 484002);
				if (statisticValid != 0) statisticValid = 1;

				// ГОСТы из уставок
				float f95_u_k2u = constraints[96];	
				float f100_u_k2u = constraints[97];
				float f95_u_k0u = constraints[98];
				float f100_u_k0u = constraints[99];

				// общее кол-во отсчетов
				ushort num_all = Conversions.bytes_2_ushort(ref buffer, 483996);
				// кол-во не маркированных отсчетов
				ushort num_not_marked = Conversions.bytes_2_ushort(ref buffer, 483998);
				// кол-во маркированных отсчетов
				ushort num_marked = Conversions.bytes_2_ushort(ref buffer, 484000);

				// отсчетов между ПДП и НДП
				ushort num_max_rng_k2u = Conversions.bytes_2_ushort(ref buffer, 514748);
				ushort num_out_max_rng_k2u = Conversions.bytes_2_ushort(ref buffer, 514750);// отсчетов за ПДП
				ushort num_nrm_rng_k2u =
					(ushort)(num_not_marked - num_max_rng_k2u - num_out_max_rng_k2u);	// отсчетов в НДП
				// отсчетов между ПДП и НДП
				ushort num_max_rng_k0u = Conversions.bytes_2_ushort(ref buffer, 514760);
				ushort num_out_max_rng_k0u = Conversions.bytes_2_ushort(ref buffer, 514762);// отсчетов за ПДП
				ushort num_nrm_rng_k0u =
					(ushort)(num_not_marked - num_max_rng_k0u - num_out_max_rng_k0u); ;	// отсчетов в НДП

				float f_up_k2u = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514752);
				float f_max_k2u = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514756);
				float f_up_k0u = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514764);
				float f_max_k0u = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514768);

				// округляем, а то не вставится в БД
				f_up_k2u = (float)Math.Round(f_up_k2u, 15);
				f_up_k0u = (float)Math.Round(f_up_k0u, 15);
				f_max_k2u = (float)Math.Round(f_max_k2u, 15);
				f_max_k0u = (float)Math.Round(f_max_k0u, 15);

				// если данные по отсчетам по какой-то причине отсутствуют
				// делаем вид, что все прошло успешно
				//if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return true;

				SqlQuery.Append(string.Format("INSERT INTO pqp_nonsymmetry (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng, valid_nonsymm) VALUES (currval('pqp_times_datetime_id_seq'), 1101, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});\n",
					archive_id,
					num_all, num_marked, num_not_marked,
					num_nrm_rng_k2u.ToString(ci_enUS),
					num_max_rng_k2u.ToString(ci_enUS),
					num_out_max_rng_k2u.ToString(ci_enUS),
					f95_u_k2u.ToString(ci_enUS),		// ГОСТы из уставок
					f100_u_k2u.ToString(ci_enUS),		// ГОСТы из уставок
					f_up_k2u.ToString(ci_enUS),		// 95%
					f_max_k2u.ToString(ci_enUS),	// max
					statisticValid
					));

				SqlQuery.Append(string.Format("INSERT INTO pqp_nonsymmetry (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng, valid_nonsymm) VALUES (currval('pqp_times_datetime_id_seq'), 1102, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});\n",
					archive_id,
					num_all, num_marked, num_not_marked,
					num_nrm_rng_k0u.ToString(ci_enUS),
					num_max_rng_k0u.ToString(ci_enUS),
					num_out_max_rng_k0u.ToString(ci_enUS),
					f95_u_k0u.ToString(ci_enUS),		// ГОСТы из уставок
					f100_u_k0u.ToString(ci_enUS),		// ГОСТы из уставок
					f_up_k0u.ToString(ci_enUS),		// 95%
					f_max_k0u.ToString(ci_enUS),	// max
					statisticValid
					));	

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_nonsymmetry():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_f(ref byte[] buffer, ref StringBuilder SqlQuery, uint archive_id,
									ConnectScheme con_scheme, ref float[] constraints)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				ushort statisticValid = Conversions.bytes_2_ushort(ref buffer, 138);
				if (statisticValid != 0) statisticValid = 1;

				// ГОСТы из уставок
				float f95_u_syn = constraints[2];	// Верхнее откл-е частоты для синхр. электросети (95%)
				float f95_d_syn = constraints[0];	// Нижнее откл-е частоты для синхр. электросети (95%)
				float f100_u_syn = constraints[3];	// Верхнее откл-е частоты для синхр. электросети (100%)
				float f100_d_syn = constraints[1];	// Нижнее откл-е частоты для синхр. электросети (100%)
				float f95_u_iso = constraints[6];	// Верхнее откл-е частоты для изолир. электросети (95%)
				float f95_d_iso = constraints[4];	// Нижнее откл-е частоты для изолир. электросети (95%)
				float f100_u_iso = constraints[7];	// Верхнее откл-е частоты для изолир. электросети (100%)
				float f100_d_iso = constraints[5];	// Нижнее откл-е частоты для изолир. электросети (100%)

				ushort num_all = Conversions.bytes_2_ushort(ref buffer, 128);	// общее кол-во отсчетов
				// кол-во отсчетов, синхронизированное с сетью
				ushort num_synchro = Conversions.bytes_2_ushort(ref buffer, 130);
				// кол-во отсчетов, не синхронизированное с сетью
				ushort num_not_synchro = Conversions.bytes_2_ushort(ref buffer, 136);
				// отсчетов между ПДП и НДП
				ushort num_max_rng = Conversions.bytes_2_ushort(ref buffer, 132);
				//отсчетов за ПДП
				ushort num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 134);
				// отсчетов в НДП
				ushort num_nrm_rng = (ushort)(num_all - num_max_rng - num_out_max_rng);	
				
				// наибольшее и наименьшее значения
				float f_max = (float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 152), 3);
				float f_min = (float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 144), 3);

				// 95 %
				float f_95_low = (float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 140), 3);
				float f_95_up = (float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 148), 3);

				// если данные по отсчетам по какой-то причине отсутствуют, делаем вид, 
				// что все прошло успешно :)
				//if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return true;

				SqlQuery.Append(string.Format("INSERT INTO pqp_f (datetime_id, param_id, archive_id, num_all, num_synchro, num_not_synchro, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_top_syn, real_max_rng_top_syn, real_nrm_rng_bottom_syn, real_max_rng_bottom_syn, calc_nrm_rng_top, calc_max_rng_top, calc_nrm_rng_bottom, calc_max_rng_bottom, valid_f, real_nrm_rng_top_iso, real_max_rng_top_iso, real_nrm_rng_bottom_iso, real_max_rng_bottom_iso) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20});\n",
					1001,	// param_id
					archive_id.ToString(),
					num_all.ToString(), num_synchro.ToString(), num_not_synchro.ToString(),
					num_nrm_rng.ToString(),		// отсчетов в НДП
					num_max_rng.ToString(),		// отсчетов между ПДП и НДП
					num_out_max_rng.ToString(),	// отсчетов за ПДП

					f95_u_syn.ToString(ci_enUS),	// НДП н
					f100_u_syn.ToString(ci_enUS),	// НДП в
					f95_d_syn.ToString(ci_enUS),	// ПДП н
					f100_d_syn.ToString(ci_enUS),	// ПДП в

                    f_95_up.ToString(ci_enUS),	// 95% в
                    f_max.ToString(ci_enUS),	// max в
					f_95_low.ToString(ci_enUS),	// 95% н
					f_min.ToString(ci_enUS),	// min н			

					statisticValid.ToString(),		// являются ли верхние значения df корректными

					f95_u_iso.ToString(ci_enUS),	// НДП н
					f100_u_iso.ToString(ci_enUS),	// НДП в
					f95_d_iso.ToString(ci_enUS),	// ПДП н
					f100_d_iso.ToString(ci_enUS)	// ПДП в
					));

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_f():  ");
				return false;
			}
		}

		//private bool xml2sql_pqp_fvalue(ref byte[] buffer, ref StringBuilder sql, ConnectScheme connection_scheme,
		//            DateTime startTime, DateTime endTime)
		//{
		#region fvalue and uvalue
		//    try
		//    {
		//        CultureInfo ci = new CultureInfo("en-US");
		//        ushort timeInterval = 20;
		//        DateTime event_datetime;
		//        // если архив начат не с ноля часов, то сверху будут значения с ноля часов второго дня
		//        //if (startTime.Day != endTime.Day)
		//        //    event_datetime = new DateTime(endTime.Year, endTime.Month, endTime.Day, 0, 0, 0);
		//        //else
		//        //    event_datetime = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0);

		//        float fVal = 0;

		//        UInt16 curBit = 1;
		//        int curShitfInBitArray = 24;
		//        UInt16 curWord = Conversions.bytes_2_ushort(ref buffer, curShitfInBitArray);
		//        // перебираем все страницы с данными о частоте и напряжениях
		//        for (int iItem = 0; iItem < 4320; iItem++)
		//        {
		//            // увеличиваем дату
		//            event_datetime = event_datetime.AddSeconds(timeInterval);
		//            // если дата больше конечной, значит закончились вторые сутки и начались первые
		//            if ((event_datetime > endTime) && (startTime.Day != endTime.Day))
		//                event_datetime = event_datetime.AddDays(-1);

		//            // проверяем в битовом массиве актуально ли поле
		//            bool bValidData = (curBit & curWord) != 0;
		//            curBit <<= 1;
		//            if (curBit == 0)	// проверили 4 байта, начинаем заново
		//            {
		//                curBit = 1;
		//                curShitfInBitArray += 2; // берем следующие 2 байта
		//                curWord = Conversions.bytes_2_ushort(ref buffer, curShitfInBitArray);
		//            }
		//            if (!bValidData)
		//                continue;

		//            fVal = Conversions.bytes_2_signed_float_Q_6_9(ref buffer, 744 + iItem * 2);

		//            sql.Append(String.Format
		//                ("INSERT INTO day_avg_parameters_t7 VALUES ({0}, '{1}', {2});\n",
		//                "currval('pqp_times_datetime_id_seq')",
		//                event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
		//                fVal.ToString(ci)));
		//        }

		//        return true;
		//    }
		//    catch (Exception e)
		//    {
		//        EmService.DumpException(e, "Error in xml2sql_pqp_t7():  ");
		//        return false;
		//    }
		//}

		//private bool xml2sql_pqp_uvalue(ref byte[] buffer, ref StringBuilder sql, ConnectScheme connectScheme,
		//            DateTime startTime, DateTime endTime)
		//{
		//    try
		//    {
		//        CultureInfo ci = new CultureInfo("en-US");
		//        ushort timeInterval = 60;
		//        DateTime event_datetime;
		//        // если архив начат не с ноля часов, то сверху будут значения с ноля часов второго дня
		//        //if (startTime.Day != endTime.Day)
		//        //    event_datetime = new DateTime(endTime.Year, endTime.Month, endTime.Day, 0, 0, 0);
		//        //else
		//        //    event_datetime = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0);

		//        float Uy = 0, U_A = 0, U_B = 0, U_C = 0, U_AB = 0, U_BC = 0, U_CA = 0;
		//        UInt16 curBit = 1;
		//        int curShitfInBitArray = 564;
		//        UInt16 curWord = Conversions.bytes_2_ushort(ref buffer, curShitfInBitArray);
		//        // перебираем все страницы с данными о частоте и напряжениях
		//        for (int iItem = 0; iItem < 1440; iItem++)
		//        {
		//            // увеличиваем дату
		//            event_datetime = event_datetime.AddSeconds(timeInterval);
		//            // если дата больше конечной, значит закончились вторые сутки и начались первые
		//            if ((event_datetime > endTime) && (startTime.Day != endTime.Day))
		//                event_datetime = event_datetime.AddDays(-1);

		//            // проверяем в битовом массиве актуально ли поле
		//            bool bValidData = (curBit & curWord) != 0;
		//            curBit <<= 1;
		//            if (curBit == 0)	// проверили 4 байта, начинаем заново
		//            {
		//                curBit = 1;
		//                curShitfInBitArray += 2; // берем следующие 2 байта
		//                curWord = Conversions.bytes_2_ushort(ref buffer, curShitfInBitArray);
		//            }
		//            if (!bValidData)
		//                continue;

		//            Uy = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 9384 + iItem * 2) * 100;
		//            Uy = (float)Math.Round((double)Uy, 8);
		//            U_A = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 12264 + iItem * 2) * 100;
		//            U_A = (float)Math.Round((double)U_A, 8);
		//            U_B = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 18024 + iItem * 2) * 100;
		//            U_B = (float)Math.Round((double)U_B, 8);
		//            U_C = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 23784 + iItem * 2) * 100;
		//            U_C = (float)Math.Round((double)U_C, 8);
		//            U_AB = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 15144 + iItem * 2) * 100;
		//            U_AB = (float)Math.Round((double)U_AB, 8);
		//            U_BC = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 20904 + iItem * 2) * 100;
		//            U_BC = (float)Math.Round((double)U_BC, 8);
		//            U_CA = Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 26664 + iItem * 2) * 100;
		//            U_CA = (float)Math.Round((double)U_CA, 8);

		//            switch (connectScheme)
		//            {
		//                case ConnectScheme.Ph3W4:
		//                case ConnectScheme.Ph3W4_B_calc:
		//                    sql.Append(String.Format
		//                        ("INSERT INTO day_avg_parameters_t6 VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8});\n",
		//                        "currval('pqp_times_datetime_id_seq')",
		//                        event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
		//                        Uy.ToString(ci),
		//                        U_A.ToString(ci),
		//                        U_B.ToString(ci),
		//                        U_C.ToString(ci),
		//                        U_AB.ToString(ci),
		//                        U_BC.ToString(ci),
		//                        U_CA.ToString(ci)
		//                        ));
		//                    break;

		//                case ConnectScheme.Ph3W3:
		//                case ConnectScheme.Ph3W3_B_calc:
		//                    sql.Append(String.Format("INSERT INTO day_avg_parameters_t6 (datetime_id, event_datetime, d_u_y, d_u_ab, d_u_bc, d_u_ca) VALUES ({0}, '{1}', {2}, {3}, {4}, {5});\n",
		//                        "currval('pqp_times_datetime_id_seq')",
		//                        event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
		//                        Uy.ToString(ci),
		//                        U_AB.ToString(ci),
		//                        U_BC.ToString(ci),
		//                        U_CA.ToString(ci)
		//                        ));
		//                    break;

		//                case ConnectScheme.Ph1W2:
		//                    sql.Append(String.Format
		//                        ("INSERT INTO day_avg_parameters_t6 (datetime_id, event_datetime, d_u_a) VALUES ({0}, '{1}', {2});\n",
		//                        "currval('pqp_times_datetime_id_seq')",
		//                        event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
		//                        U_A.ToString(ci)
		//                        ));
		//                    break;
		//            }
		//        }
		//        return true;
		//    }
		//    catch (Exception e)
		//    {
		//        EmService.DumpException(e, "Error in xml2sql_pqp_t6():  ");
		//        return false;
		//    }
		//}
		#endregion

		private bool xml2sql_pqp_du(ref byte[] buffer, ref StringBuilder SqlQuery, int param_id, 
                                    uint archive_id, ConnectScheme con_scheme, 
									ref float[] constraints)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				ushort statisticValid = Conversions.bytes_2_ushort(ref buffer, 484002);
				if (statisticValid != 0) statisticValid = 1;
				
				// ГОСТы из уставок
				float f95_u = constraints[8];	// положительное отклонение напряжения (95%)
				float f100_u = constraints[9];	// положительное отклонение напряжения (100%)
				float f95_d = constraints[10];	// отрицательное отклонение напряжения (95%)
				float f100_d = constraints[11];	// отрицательное отклонение напряжения (100%)

				// общее кол-во отсчетов
				ushort num_all = Conversions.bytes_2_ushort(ref buffer, 483996);	
				// кол-во маркированных отсчетов
				ushort num_not_marked = Conversions.bytes_2_ushort(ref buffer, 483998);
				// кол-во не маркированных отсчетов
                ushort num_marked = Conversions.bytes_2_ushort(ref buffer, 484000);

				ushort num_max_rng = 0;			// отсчетов между ПДП и НДП
				ushort num_out_max_rng = 0;		// отсчетов за ПДП
				ushort num_nrm_rng = 0;			// отсчетов в НДП
				float f_max = 0;				// наибольшее отклонение
				float f_95_up = 0;				// верхнее отклонение

				switch (param_id)
				{
					case 1003:		// dU_A
					case 1014:		// dU_AB
						num_max_rng = Conversions.bytes_2_ushort(ref buffer, 489044);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 489046);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489052), 4);
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489048), 4);
						break;

					case 1004:		// dU_B
					case 1015:		// dU_BC
						num_max_rng = Conversions.bytes_2_ushort(ref buffer, 489056);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 489058);	// за ПДП

						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489064), 4);
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489060), 4);
						break;

					case 1005:		// dU_C
					case 1016:		// dU_CA
						num_max_rng = Conversions.bytes_2_ushort(ref buffer, 489068);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 489070);	// за ПДП

						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489076), 4);
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489072), 4);
						break;

					case 1007:		// dU_A-
					case 1017:		// dU_AB-
						num_max_rng = Conversions.bytes_2_ushort(ref buffer, 489080);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 489082);

						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489088), 4);
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489084), 4);
						break;

					case 1008:		// dU_B-
					case 1018:		// dU_BC-
						num_max_rng = Conversions.bytes_2_ushort(ref buffer, 489092);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 489094);	// за ПДП

						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489100), 4);
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489096), 4);
						break;

					case 1009:		// dU_C-
					case 1019:		// dU_CA-
						num_max_rng = Conversions.bytes_2_ushort(ref buffer, 489104);
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 489106);	// за ПДП

						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489112), 4);
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489108), 4);
						break;
				}
				num_nrm_rng = (ushort)(num_not_marked - num_max_rng - num_out_max_rng);

				// если данные по отсчетам по какой-то причине отсутствуют, делаем вид, 
				// что все прошло более чем успешно :)
				//if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return true;

				SqlQuery.Append(string.Format("INSERT INTO pqp_du(datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_top, real_max_rng_top, real_nrm_rng_bottom, real_max_rng_bottom, calc_nrm_rng_top, calc_max_rng_top, valid_du) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14});\n",
					param_id.ToString(),
					archive_id.ToString(),
					num_all.ToString(), 
					num_marked.ToString(), 
					num_not_marked.ToString(),

					num_nrm_rng.ToString(),		// отсчетов в НДП
					num_max_rng.ToString(),		// отсчетов между ПДП и НДП
					num_out_max_rng.ToString(),	// отсчетов за ПДП

					f95_u.ToString(ci_enUS),	// НДП в
					f100_u.ToString(ci_enUS),	// ПДП в
					f95_d.ToString(ci_enUS),	// НДП н
					f100_d.ToString(ci_enUS),	// ПДП н

					f_95_up.ToString(ci_enUS),	// 100%
					f_max.ToString(ci_enUS),	// 95%

					statisticValid.ToString()		// являются ли верхние значения du корректными
					));

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_du():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_df_du_val(ref byte[] buffer, ref StringBuilder SqlQuery,
						ConnectScheme connectScheme, DateTime dtStart, DateTime dtEnd)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// общее кол-во отсчетов
				ushort num_f = Conversions.bytes_2_ushort(ref buffer, 128);
				int shiftF = 242076;
				int shiftFSeconds = 156;

				float[] df = new float[num_f];
				int[] f_seconds = new int[num_f];	// Массив временных меток измерений отклонения частоты
				for (int iArr = 0; iArr < num_f; ++iArr) df[iArr] = -1;

				// считываем значение из буфера
				for (int iF = 0; iF < num_f; ++iF)
				{
					f_seconds[iF] = Conversions.bytes_2_int(ref buffer, shiftFSeconds);
					df[iF] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftF);
					if (df[iF] == 0x7FFFFFFF) df[iF] = -1;
					else df[iF] = (float)Math.Round(df[iF], 6);
					shiftF += 4;	// 4 is length of one record
					shiftFSeconds += 4;
				}

				DateTime dtTemp = dtStart;
				for (int iRecord = 0; iRecord < num_f; ++iRecord)
				{
					if (Single.IsInfinity(df[iRecord])) df[iRecord] = -1;

					SqlQuery.Append(string.Format("INSERT INTO pqp_df_val (datetime_id, event_datetime, d_f, f_seconds) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2});\n",
							dtTemp.ToString("MM.dd.yyyy HH:mm:ss"),
							df[iRecord].ToString("F6", ci_enUS),
							f_seconds[iRecord].ToString()
							));
					
					dtTemp = dtTemp.AddSeconds(10);
				}
				
				/////////////////////////////
				// voltage
				/////////////////////////////

				// общее кол-во отсчетов
				ushort num_u = Conversions.bytes_2_ushort(ref buffer, 483996);
				int shiftMarked = 488036;
				int shiftU = 489116;
				int shiftRecSeconds = 484004;

				float[] u_a_pos = new float[num_u];
				float[] u_b_pos = new float[num_u];
				float[] u_c_pos = new float[num_u];
				float[] u_a_neg = new float[num_u];
				float[] u_b_neg = new float[num_u];
				float[] u_c_neg = new float[num_u];
				short[] rec_marked = new short[num_u];
				int[] rec_seconds = new int[num_u];			// Массив временных меток измерений
				for (int iArr = 0; iArr < num_u; ++iArr)
				{
					u_a_pos[iArr] = u_b_pos[iArr] = u_c_pos[iArr] = -1;
					u_a_neg[iArr] = u_b_neg[iArr] = u_c_neg[iArr] = -1;
					rec_marked[iArr] = 1;
					rec_seconds[iArr] = 0;
				}

				// считываем значение из буфера
				for (int iU = 0; iU < num_u; ++iU)
				{
					rec_marked[iU] = buffer[shiftMarked];
					rec_seconds[iU] = Conversions.bytes_2_int(ref buffer, shiftRecSeconds);

					if (rec_marked[iU] == 0)
					{
						u_a_pos[iU] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU);
						u_a_pos[iU] = (float)Math.Round(u_a_pos[iU], 4);
						u_a_neg[iU] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 12096);
						u_a_neg[iU] = (float)Math.Round(u_a_neg[iU], 4);
					}

					if (connectScheme != ConnectScheme.Ph1W2)
					{
						if (rec_marked[iU] == 0)
						{
							u_b_pos[iU] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 4032);
							u_b_pos[iU] = (float)Math.Round(u_b_pos[iU], 4);
							u_c_pos[iU] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 8064);
							u_c_pos[iU] = (float)Math.Round(u_c_pos[iU], 4);
							u_b_neg[iU] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 16128);
							u_b_neg[iU] = (float)Math.Round(u_b_neg[iU], 4);
							u_c_neg[iU] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 20160);
							u_c_neg[iU] = (float)Math.Round(u_c_neg[iU], 4);
						}
					}

					shiftU += 4;	// 4 is length of one record
					shiftMarked += 1;
					shiftRecSeconds += 4;
				}

				dtTemp = dtStart;
				for (int iRecord = 0; iRecord < num_u; ++iRecord)
				{
					// отрицат. значений тут не бывает, поэтому можно использовать -1 как признак невалидности
					if (Single.IsInfinity(u_a_pos[iRecord])) u_a_pos[iRecord] = -1;
					if (Single.IsInfinity(u_a_neg[iRecord])) u_a_neg[iRecord] = -1;
					if (Single.IsInfinity(u_b_pos[iRecord])) u_b_pos[iRecord] = -1;
					if (Single.IsInfinity(u_b_neg[iRecord])) u_b_neg[iRecord] = -1;
					if (Single.IsInfinity(u_c_pos[iRecord])) u_c_pos[iRecord] = -1;
					if (Single.IsInfinity(u_c_neg[iRecord])) u_c_neg[iRecord] = -1;

					if (connectScheme == ConnectScheme.Ph1W2)
					{
						SqlQuery.Append(string.Format("INSERT INTO pqp_du_val(datetime_id, event_datetime, u_a_ab_pos, u_a_ab_neg, record_marked, record_seconds) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4});\n",
							dtTemp.ToString("MM.dd.yyyy HH:mm:ss"),
							u_a_pos[iRecord].ToString("F6", ci_enUS),
							u_a_neg[iRecord].ToString("F6", ci_enUS),
							rec_marked[iRecord].ToString(),
							rec_seconds[iRecord].ToString()
							));
					}
					else
					{
						SqlQuery.Append(string.Format("INSERT INTO pqp_du_val(datetime_id, event_datetime, u_a_ab_pos, u_b_bc_pos, u_c_ca_pos, u_a_ab_neg, u_b_bc_neg, u_c_ca_neg, record_marked, record_seconds) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});\n",
							dtTemp.ToString("MM.dd.yyyy HH:mm:ss"),
							u_a_pos[iRecord].ToString("F6", ci_enUS),
							u_b_pos[iRecord].ToString("F6", ci_enUS),
							u_c_pos[iRecord].ToString("F6", ci_enUS),
							u_a_neg[iRecord].ToString("F6", ci_enUS),
							u_b_neg[iRecord].ToString("F6", ci_enUS),
							u_c_neg[iRecord].ToString("F6", ci_enUS),
							rec_marked[iRecord].ToString(),
							rec_seconds[iRecord].ToString()
							));
					}

					dtTemp = dtTemp.AddMinutes(10);
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_pqp_df_du_val():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_dip_swell(ref byte[] buffer, ref StringBuilder SqlQuery, uint archive_id,
											DEVICE_VERSIONS newDipSwellMode)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				int param_id = 3043;

				if (newDipSwellMode != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
				{
					int shift = 260900*2;
					int shift_over_180 = 261226*2; // для событий длительностью >180 (они в архиве отдельно)
					for (int iEvent = 0; iEvent < 10; ++iEvent)
					{
						ushort num_0_01_till_0_05 = Conversions.bytes_2_ushort(ref buffer, shift);
						ushort num_0_05_till_0_1 = Conversions.bytes_2_ushort(ref buffer, shift + 2);
						ushort num_0_1_till_0_5 = Conversions.bytes_2_ushort(ref buffer, shift + 4);
						ushort num_0_5_till_1 = Conversions.bytes_2_ushort(ref buffer, shift + 6);
						ushort num_1_till_3 = Conversions.bytes_2_ushort(ref buffer, shift + 8);
						ushort num_3_till_20 = Conversions.bytes_2_ushort(ref buffer, shift + 10);
						ushort num_20_till_60 = Conversions.bytes_2_ushort(ref buffer, shift + 12);
						ushort num_over_60 = Conversions.bytes_2_ushort(ref buffer, shift + 14);
						ushort num_over_180 = Conversions.bytes_2_ushort(ref buffer, shift_over_180);

						SqlQuery.Append(
							string.Format(
								"INSERT INTO pqp_dip_swell(datetime_id, param_id, archive_id, num_0_01_till_0_05, num_0_05_till_0_1, num_0_1_till_0_5, num_0_5_till_1, num_1_till_3, num_3_till_20, num_20_till_60, num_over_60, num_over_180) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});\n",
								param_id,
								archive_id,
								num_0_01_till_0_05,
								num_0_05_till_0_1,
								num_0_1_till_0_5,
								num_0_5_till_1,
								num_1_till_3,
								num_3_till_20,
								num_20_till_60,
								num_over_60,
								num_over_180
								));

						++param_id;
						shift += 8 * 2;
						shift_over_180 += 2;
					}
				}
				else
				{
					param_id = 3053;
					// для нового режима кладем в те же столбцы (получается, что название столбца в БД
					// не соответствует содержимому)
					int shift = 261236 * 2;
					for (int iEvent = 0; iEvent < 9; ++iEvent)
					{
						ushort num_0_01_till_0_2 = Conversions.bytes_2_ushort(ref buffer, shift);
						ushort num_0_2_till_0_5 = Conversions.bytes_2_ushort(ref buffer, shift + 2);
						ushort num_0_5_till_1 = Conversions.bytes_2_ushort(ref buffer, shift + 4);
						ushort num_1_till_5 = Conversions.bytes_2_ushort(ref buffer, shift + 6);
						ushort num_5_till_20 = Conversions.bytes_2_ushort(ref buffer, shift + 8);
						ushort num_20_till_60 = Conversions.bytes_2_ushort(ref buffer, shift + 10);

						SqlQuery.Append(
							string.Format(
								"INSERT INTO pqp_dip_swell(datetime_id, param_id, archive_id, num_0_01_till_0_05, num_0_05_till_0_1, num_0_1_till_0_5, num_0_5_till_1, num_1_till_3, num_3_till_20) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});\n",
								param_id,
								archive_id,
								num_0_01_till_0_2,
								num_0_2_till_0_5,
								num_0_5_till_1,
								num_1_till_5,
								num_5_till_20,
								num_20_till_60
								));

						++param_id;
						shift += 6 * 2;
					}

					// прерывания теперь отдельно
					shift = 261290 * 2;
					ushort num_0_till_0_5 = Conversions.bytes_2_ushort(ref buffer, shift);
					ushort num_0_5_till_1_new = Conversions.bytes_2_ushort(ref buffer, shift + 2);
					ushort num_1_till_5_new = Conversions.bytes_2_ushort(ref buffer, shift + 4);
					ushort num_5_till_20_new = Conversions.bytes_2_ushort(ref buffer, shift + 6);
					ushort num_20_till_60_new = Conversions.bytes_2_ushort(ref buffer, shift + 8);
					ushort num_60_till_180 = Conversions.bytes_2_ushort(ref buffer, shift + 10);
					ushort num_over_180 = Conversions.bytes_2_ushort(ref buffer, shift + 12);

					int num_max_len = Conversions.bytes_2_int(ref buffer, 261298 * 2);

					param_id = 3062;

					SqlQuery.Append(
						string.Format(
							"INSERT INTO pqp_dip_swell(datetime_id, param_id, archive_id, num_0_01_till_0_05, num_0_05_till_0_1, num_0_1_till_0_5, num_0_5_till_1, num_1_till_3, num_3_till_20, num_20_till_60, num_over_60) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9});\n",
							param_id,
							archive_id,
							num_0_till_0_5,
							num_0_5_till_1_new,
							num_1_till_5_new,
							num_5_till_20_new,
							num_20_till_60_new,
							num_60_till_180,
							num_over_180,
							num_max_len
							));
				}
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_dip_swell():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_nonsinus(ref byte[] buffer, ref StringBuilder SqlQuery, 
								uint archive_id, ConnectScheme ConnectionScheme, ref float[] constraints)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				ushort statisticValid = Conversions.bytes_2_ushort(ref buffer, 484002);
				if (statisticValid != 0) statisticValid = 1;

				int shift_int = 0;
				int shift_float = 0;
				int shift_constraints = 16;
				int param_id = 1201;

				// общее кол-во отсчетов
				ushort num_all = Conversions.bytes_2_ushort(ref buffer, 483996);
				// кол-во маркированных отсчетов
				ushort num_not_marked = Conversions.bytes_2_ushort(ref buffer, 483998);
				// кол-во не маркированных отсчетов
				ushort num_marked = Conversions.bytes_2_ushort(ref buffer, 484000);

                float f_95_a_total = 0.0F, f_max_a_total = 0.0F;
                float f_95_b_total = 0.0F, f_max_b_total = 0.0F;
                float f_95_c_total = 0.0F, f_max_c_total = 0.0F;

				for (int iKu = 0; iKu < 40; ++iKu)
				{
					// ГОСТы из уставок
					float fNDP = 0;
					float fPDP = 0;
					if (iKu == 0)
					{
						fNDP = constraints[94];
						fPDP = constraints[95];
					}
					else
					{
						fNDP = constraints[shift_constraints];
						fPDP = constraints[shift_constraints + 39];
                        ++shift_constraints;
					}

					// отсчеты прибора
					// отсчетов между ПДП и НДП
					ushort num_max_rng_a = Conversions.bytes_2_ushort(ref buffer, shift_int + 513308);
					ushort num_out_max_rng_a = Conversions.bytes_2_ushort(ref buffer, shift_int + 513388);//за ПДП
					ushort num_nrm_rng_a = (ushort)(num_not_marked - num_max_rng_a - num_out_max_rng_a);// в НДП

					// отсчетов между ПДП и НДП
					ushort num_max_rng_b = Conversions.bytes_2_ushort(ref buffer, shift_int + 513788);
					ushort num_out_max_rng_b = Conversions.bytes_2_ushort(ref buffer, shift_int + 513868);// за ПДП
					ushort num_nrm_rng_b = (ushort)(num_not_marked - num_max_rng_b - num_out_max_rng_b);// в НДП

					// отсчетов между ПДП и НДП
					ushort num_max_rng_c = Conversions.bytes_2_ushort(ref buffer, shift_int + 514268);
					ushort num_out_max_rng_c = Conversions.bytes_2_ushort(ref buffer, shift_int + 514348);// за ПДП
					ushort num_nrm_rng_c = (ushort)(num_not_marked - num_max_rng_c - num_out_max_rng_c);// в НДП

					// верхнее значение (95%) и наибольшее значение (max)
					float f_95_a = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 513468);
					float f_max_a = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 513628);

					float f_95_b = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 513948);
					float f_max_b = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 514108);

					float f_95_c = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 514428);
					float f_max_c = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 514588);

                    if (iKu == 0)       // сохраняем суммарные значения
                    {
                        f_95_a_total = f_95_a;
                        f_max_a_total = f_max_a;
                        f_95_b_total = f_95_b;
                        f_max_b_total = f_max_b;
                        f_95_c_total = f_95_c;
                        f_max_c_total = f_max_c;
                    }
                    else   // исправляем ошибку в прошивке прибора - если значение зашкаливает, ставим 0
                    {
                        if (f_95_a > f_95_a_total) f_95_a = 0.0F;
                        if (f_max_a > f_max_a_total) f_max_a = 0.0F;
                        if (f_95_b > f_95_b_total) f_95_b = 0.0F;
                        if (f_max_b > f_max_b_total) f_max_b = 0.0F;
                        if (f_95_c > f_95_c_total) f_95_c = 0.0F;
                        if (f_max_c > f_max_c_total) f_max_c = 0.0F;
                    }

                    //if (num_nrm_rng_a != 0 || num_max_rng_a != 0 || num_out_max_rng_a != 0 ||
                    //    num_nrm_rng_b != 0 || num_max_rng_b != 0 || num_out_max_rng_b != 0 ||
                    //    num_nrm_rng_c != 0 || num_max_rng_c != 0 || num_out_max_rng_c != 0)
                    //{
					SqlQuery.Append(String.Format("INSERT INTO pqp_nonsinus (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng, valid_harm) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22});\n",
						param_id,
						archive_id,
						num_all, num_marked, num_not_marked,
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
						fPDP.ToString(ci_enUS),			// ПДП
						statisticValid
						));		
					//}

					++param_id;
					shift_int += 2;
					shift_float += 4;
				}

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_nonsinus():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_interharm_u(ref byte[] buffer, ref StringBuilder SqlQuery,
								uint archive_id, ConnectScheme ConnectionScheme, ref EtPQP_A_XmlArchive xmlArchive)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				int shift = 521960;
				//int param_id = 1201;

				// общее кол-во отсчетов
				ushort num_all = Conversions.bytes_2_ushort(ref buffer, 483996);
				// кол-во маркированных отсчетов
				ushort num_not_marked = Conversions.bytes_2_ushort(ref buffer, 483998);
				// кол-во не маркированных отсчетов
				ushort num_marked = Conversions.bytes_2_ushort(ref buffer, 484000);

				float u_multiplier = 1;
				if (xmlArchive.SysInfo.U_transformer_enable)
					u_multiplier = EtPQP_A_Device.GetUTransformerMultiplier(xmlArchive.SysInfo.U_transformer_type);
				u_multiplier /= 1000000f;

				for (int iInter = 1; iInter < 42; ++iInter)
				{
					double valA = Conversions.bytes_2_int(ref buffer, shift) * u_multiplier;
					double valB = Conversions.bytes_2_int(ref buffer, shift + 164) * u_multiplier;
					double valC = Conversions.bytes_2_int(ref buffer, shift + 164 + 164) * u_multiplier;
					valA = Math.Round(valA, 4);
					valB = Math.Round(valB, 4);
					valC = Math.Round(valC, 4);

					SqlQuery.Append(String.Format("INSERT INTO pqp_interharm_u(datetime_id, param_id, param_num, archive_id, num_all, num_marked, num_not_marked, val_ph1, val_ph2, val_ph3, valid_interharm) VALUES (currval('pqp_times_datetime_id_seq'), null, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});\n",
						//param_id,
						iInter,
						archive_id,
						num_all, num_marked, num_not_marked,
						valA.ToString("F6", ci_enUS),
						valB.ToString("F6", ci_enUS),
						valC.ToString("F6", ci_enUS),
						num_not_marked > 0 ? 1 : 0				// if values are valid
						));

					//++param_id;
					shift += 4;
				}

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_interharm_u():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_flicker(ref byte[] buffer, ref StringBuilder SqlQuery,
								uint archive_id, ConnectScheme ConnectionScheme, ref float[] constraints)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				ushort statisticValid_short = Conversions.bytes_2_ushort(ref buffer, 484002);
                ushort statisticValid_long = Conversions.bytes_2_ushort(ref buffer, 521796);

				// общее кол-во отсчетов
				ushort num_all_short = Conversions.bytes_2_ushort(ref buffer, 483996);	
				// кол-во маркированных отсчетов
                ushort num_not_marked_short = Conversions.bytes_2_ushort(ref buffer, 483998);
				// кол-во не маркированных отсчетов
                ushort num_marked_short = Conversions.bytes_2_ushort(ref buffer, 484000);

                ushort num_all_long = Conversions.bytes_2_ushort(ref buffer, 521792);
                ushort num_not_marked_long = Conversions.bytes_2_ushort(ref buffer, 521794);
				ushort num_marked_long = (ushort)(num_all_long - num_not_marked_long);

				// ГОСТы из уставок
				float fNDP_short = constraints[12];
				float fPDP_short = constraints[13];
				float fNDP_long = constraints[14];
				float fPDP_long = constraints[15];

				// отсчеты прибора
				// отсчетов между ПДП и НДП
				ushort num_max_rng_a_short = Conversions.bytes_2_ushort(ref buffer, 514772);
				ushort num_out_max_rng_a_short = Conversions.bytes_2_ushort(ref buffer, 514774);// за ПДП
				ushort num_nrm_rng_a_short =
					(ushort)(num_not_marked_short - num_max_rng_a_short - num_out_max_rng_a_short);	// в НДП

                ushort num_max_rng_b_short = Conversions.bytes_2_ushort(ref buffer, 514788);
                ushort num_out_max_rng_b_short = Conversions.bytes_2_ushort(ref buffer, 514790);
				ushort num_nrm_rng_b_short =
					(ushort)(num_not_marked_short - num_max_rng_b_short - num_out_max_rng_b_short);

                ushort num_max_rng_c_short = Conversions.bytes_2_ushort(ref buffer, 514804);
                ushort num_out_max_rng_c_short = Conversions.bytes_2_ushort(ref buffer, 514806);
				ushort num_nrm_rng_c_short =
					(ushort)(num_not_marked_short - num_max_rng_c_short - num_out_max_rng_c_short);

				ushort num_max_rng_a_long = Conversions.bytes_2_ushort(ref buffer, 514776);
				ushort num_out_max_rng_a_long = Conversions.bytes_2_ushort(ref buffer, 514778);
				ushort num_nrm_rng_a_long =
					(ushort)(num_not_marked_long - num_max_rng_a_long - num_out_max_rng_a_long);

                ushort num_max_rng_b_long = Conversions.bytes_2_ushort(ref buffer, 514792);
                ushort num_out_max_rng_b_long = Conversions.bytes_2_ushort(ref buffer, 514794);
				ushort num_nrm_rng_b_long =
					(ushort)(num_not_marked_long - num_max_rng_b_long - num_out_max_rng_b_long);

                ushort num_max_rng_c_long = Conversions.bytes_2_ushort(ref buffer, 514808);
                ushort num_out_max_rng_c_long = Conversions.bytes_2_ushort(ref buffer, 514810);
				ushort num_nrm_rng_c_long =
					(ushort)(num_not_marked_long - num_max_rng_c_long - num_out_max_rng_c_long);

				// верхнее значение (95%) и наибольшее значение (max)
				float f_95_a_short = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514780);
				float f_max_a_short = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514782);

                float f_95_b_short = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514796);
                float f_max_b_short = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514798);

                float f_95_c_short = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514812);
                float f_max_c_short = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514814);

				float f_95_a_long = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514784);
				float f_max_a_long = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514786);

                float f_95_b_long = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514800);
                float f_max_b_long = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514802);

                float f_95_c_long = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514816);
                float f_max_c_long = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514818);

				SqlQuery.Append(String.Format("INSERT INTO pqp_flicker (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng, valid_flick) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22});\n",
					3041,
					archive_id,
					num_all_short, num_marked_short, num_not_marked_short,
					// фаза A
					num_nrm_rng_a_short.ToString(),		// отсчетов в НДП
					num_max_rng_a_short.ToString(),		// отсчетов в ПДП
					num_out_max_rng_a_short.ToString(),	// отсчетов за ПДП
					f_95_a_short.ToString(ci_enUS),		// 95%
					f_max_a_short.ToString(ci_enUS),		// max
					// фаза B
					num_nrm_rng_b_short.ToString(),		// отсчетов в НДП
					num_max_rng_b_short.ToString(),		// отсчетов в ПДП
					num_out_max_rng_b_short.ToString(),	// отсчетов за ПДП
					f_95_b_short.ToString(ci_enUS),		// 95%
					f_max_b_short.ToString(ci_enUS),		// max
					// фаза C
					num_nrm_rng_c_short.ToString(),		// отсчетов в НДП
					num_max_rng_c_short.ToString(),		// отсчетов в ПДП
					num_out_max_rng_c_short.ToString(),	// отсчетов за ПДП
					f_95_c_short.ToString(ci_enUS),		// 95%
					f_max_c_short.ToString(ci_enUS),		// max
					// ГОСТы
					fNDP_short.ToString(ci_enUS),			// НДП
					fPDP_short.ToString(ci_enUS),			// ПДП
					statisticValid_short != 0 ? 1 : 0
					));

				SqlQuery.Append(String.Format("INSERT INTO pqp_flicker (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng, valid_flick) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22});\n",
					3042,
					archive_id,
					num_all_long, num_marked_long, num_not_marked_long,
					// фаза A
					num_nrm_rng_a_long.ToString(),		// отсчетов в НДП
					num_max_rng_a_long.ToString(),		// отсчетов в ПДП
					num_out_max_rng_a_long.ToString(),	// отсчетов за ПДП
					f_95_a_long.ToString(ci_enUS),		// 95%
					f_max_a_long.ToString(ci_enUS),		// max
					// фаза B
					num_nrm_rng_b_long.ToString(),		// отсчетов в НДП
					num_max_rng_b_long.ToString(),		// отсчетов в ПДП
					num_out_max_rng_b_long.ToString(),	// отсчетов за ПДП
					f_95_b_long.ToString(ci_enUS),		// 95%
					f_max_b_long.ToString(ci_enUS),		// max
					// фаза C
					num_nrm_rng_c_long.ToString(),		// отсчетов в НДП
					num_max_rng_c_long.ToString(),		// отсчетов в ПДП
					num_out_max_rng_c_long.ToString(),	// отсчетов за ПДП
					f_95_c_long.ToString(ci_enUS),		// 95%
					f_max_c_long.ToString(ci_enUS),		// max
					// ГОСТы
					fNDP_long.ToString(ci_enUS),		// НДП
					fPDP_long.ToString(ci_enUS),		// ПДП
					statisticValid_long != 0 ? 1 : 0
					));

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in xml2sql_pqp_flicker():  ");
				return false;
			}
		}

		private bool xml2sql_pqp_flick_val(ref byte[] buffer, ref StringBuilder SqlQuery,
						ConnectScheme connectScheme, DateTime dtStart, DateTime dtEnd)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// общее кол-во отсчетов
				ushort num_all_short = Conversions.bytes_2_ushort(ref buffer, 483996);
				ushort num_all_long = Conversions.bytes_2_ushort(ref buffer, 521792);
				int shiftMarkedShort = 488036;
				int shiftMarkedLong = 521708;
				int shiftFlickerShort = 514820;
				int shiftFlickerLong = 520868;
				int shiftSecondsShort = 484004;
				int shiftSecondsLong = 521372;

				float[] flik_A = new float[num_all_short];
				float[] flik_B = new float[num_all_short];
				float[] flik_C = new float[num_all_short];
				float[] flik_A_long = new float[num_all_short];
				float[] flik_B_long = new float[num_all_short];
				float[] flik_C_long = new float[num_all_short];
				short[] flik_marked_short = new short[num_all_short];
				short[] flik_marked_long = new short[num_all_short];
				int[] seconds_short = new int[num_all_short];		// Массив временных меток измерений
				int[] seconds_long = new int[num_all_short];		// Массив временных меток измерений
				for (int iArr = 0; iArr < num_all_short; ++iArr)
				{
					flik_A[iArr] = flik_B[iArr] = flik_C[iArr] = -1;
					flik_A_long[iArr] = flik_B_long[iArr] = flik_C_long[iArr] = -1;
					flik_marked_short[iArr] = flik_marked_long[iArr] = 1;
					seconds_long[iArr] = seconds_short[iArr] = 0;
				}

				// считываем значение из буфера
				for (int iShort = 0; iShort < num_all_short; ++iShort)
				{
					flik_marked_short[iShort] = buffer[shiftMarkedShort];
					seconds_short[iShort] = Conversions.bytes_2_int(ref buffer, shiftSecondsShort);

					// st
					if (flik_marked_short[iShort] == 0)
						flik_A[iShort] = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerShort);

					//lt
					if (num_all_long > 0)
					{
						// в двух часах 12 десятиминуток
						if (((iShort + 1) % 12) == 0 && iShort != 0)
						//if (((iShort + 1) % num_all_long) == 0 && iShort != 0)
						{
							flik_marked_long[iShort] = buffer[shiftMarkedLong];
							seconds_long[iShort] = Conversions.bytes_2_int(ref buffer, shiftSecondsLong);

							if (flik_marked_long[iShort] == 0)
							{
								flik_A_long[iShort] = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerLong);
								if (connectScheme != ConnectScheme.Ph1W2)
								{
									flik_B_long[iShort] =
										Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerLong + 168);
									flik_C_long[iShort] =
										Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerLong + 336);
								}
							}

							shiftFlickerLong += 2;
							shiftMarkedLong += 1;
							shiftSecondsLong += 4;
						}
					}
					// end of lt

					// st
					if (connectScheme != ConnectScheme.Ph1W2)
					{
						if (flik_marked_short[iShort] == 0)
							flik_B[iShort] = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerShort + 2016);
						if (flik_marked_short[iShort] == 0)
							flik_C[iShort] = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerShort + 4032);
					}

					shiftFlickerShort += 2;	// 2 is length of one record
					shiftMarkedShort += 1;
					shiftSecondsShort += 4;
				}

				DateTime dtTemp = dtStart;
				for (int iRecord = 0; iRecord < num_all_short; ++iRecord)
				{
					if (Single.IsInfinity(flik_A[iRecord])) flik_A[iRecord] = -1;
					if (Single.IsInfinity(flik_A_long[iRecord])) flik_A_long[iRecord] = -1;
					if (Single.IsInfinity(flik_B[iRecord])) flik_B[iRecord] = -1;
					if (Single.IsInfinity(flik_B_long[iRecord])) flik_B_long[iRecord] = -1;
					if (Single.IsInfinity(flik_C[iRecord])) flik_C[iRecord] = -1;
					if (Single.IsInfinity(flik_C_long[iRecord])) flik_C_long[iRecord] = -1;

					if (connectScheme == ConnectScheme.Ph1W2)
					{
						SqlQuery.Append(string.Format("INSERT INTO pqp_flicker_val (datetime_id, flik_time, flik_a, flik_a_long, flik_short_seconds, flik_lond_seconds, flik_short_marked, flik_long_marked) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6});\n",
							dtTemp.ToString("MM.dd.yyyy HH:mm:ss"),
							flik_A[iRecord].ToString(ci_enUS),
							flik_A_long[iRecord].ToString(ci_enUS),
							seconds_short[iRecord].ToString(),
							seconds_long[iRecord].ToString(),
							flik_marked_short[iRecord].ToString(),
							flik_marked_long[iRecord].ToString()
							));
					}
					else
					{
						SqlQuery.Append(string.Format("INSERT INTO pqp_flicker_val (datetime_id, flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long, flik_short_seconds, flik_lond_seconds, flik_short_marked, flik_long_marked) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});\n",
							dtTemp.ToString("MM.dd.yyyy HH:mm:ss"),
							flik_A[iRecord].ToString(ci_enUS),
							flik_A_long[iRecord].ToString(ci_enUS),
							flik_B[iRecord].ToString(ci_enUS),
							flik_B_long[iRecord].ToString(ci_enUS),
							flik_C[iRecord].ToString(ci_enUS),
							flik_C_long[iRecord].ToString(ci_enUS),
							seconds_short[iRecord].ToString(),
							seconds_long[iRecord].ToString(),
							flik_marked_short[iRecord].ToString(),
							flik_marked_long[iRecord].ToString()
							));
					}

					dtTemp = dtTemp.AddMinutes(10);
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_pqp_flick_val():  ");
				return false;
			}
		}

		#endregion

		#region Building SQL query for inserting Events data from Device

		private bool xml2sql_dns_period(EmXmlDNS xmlDNS, ref EmSqlDataNode dnsNode, ConnectScheme conScheme)
		{
			try
			{
				dnsNode.SqlType = EmSqlDataNodeType.Events;

				dnsNode.Begin = xmlDNS.Start;
				dnsNode.End = xmlDNS.End;

				bool res = false;
				string tempSql = "";
				if (xmlDNS.DataPages != null)
					res = xml2sql_dns_archive(ref xmlDNS, ref tempSql);
				//if (xmlDNS.CurrentDNSBuffer != null)
				//	res = xml2sql_dns_current_period(ref xmlDNS, ref tempSql, conScheme);

				if (tempSql != "")
				{
					dnsNode.Sql += string.Format("INSERT INTO dns_times (datetime_id, start_datetime, end_datetime, registration_id, device_id, parent_folder_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4});\n",
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

		private bool xml2sql_dns_archive(ref EmXmlDNS xmlDNS, ref string tempSql)
		{
			byte[] buffer = xmlDNS.DataPages;
			int recLen = 128;	// length of one record is 128 bytes
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
					long eventIndex = Conversions.bytes_2_uint_new(ref buffer, 0 + i * recLen);

					try
					{
						dtStart = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 24 + i * recLen, "DNS start");
						dtEnd = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 52 + i * recLen, "DNS end");
						if (dtStart == DateTime.MinValue || dtEnd == DateTime.MinValue)
						{
							EmService.WriteToLogFailed("xml2sql_dns_period: datetime error!  " + i.ToString());
							continue;
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "xml2sql_dns_period: invalid data!  " + i.ToString());
						continue;
					}

					if (dtStart < xmlDNS.Start)
					{
						dtStart = xmlDNS.Start;
						wasEarlier = true;
					}

					if (dtStart > dtEnd)
					{
						EmService.WriteToLogFailed("xml2sql_dns_period(): dtStart > dtEnd  " +
											dtStart.ToString() + "   " + dtEnd.ToString());
						continue;
					}

					wasEvent = true;

					isFinished = (Conversions.bytes_2_ushort(ref buffer, 94) > 0);

					// Младший байт – тип события, Старший байт – фаза
					ushort eventType = Conversions.bytes_2_ushort(ref buffer, 8 + i * recLen);
					byte phaseNum = (byte)((eventType >> 8) & 0xFF);
					eventType &= 0xFF;
					// get phase and type
					string phase = string.Empty;
					if (phaseNum == 0x00) { phase = "ABCN"; }
					else if (phaseNum == 0x01) { phase = "ABC"; }
					else if (phaseNum == 0x02) { phase = "A"; }
					else if (phaseNum == 0x03) { phase = "B"; }
					else if (phaseNum == 0x04) { phase = "C"; }
					else if (phaseNum == 0x05) { phase = "AB"; }
					else if (phaseNum == 0x06) { phase = "BC"; }
					else if (phaseNum == 0x07) { phase = "CA"; }

					float u_value = Conversions.bytes_2_uint_new(ref buffer, 12 + i * recLen) / 1000000f;
					float d_u = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, 16 + i * recLen) * 100;
					float u_declared = Conversions.bytes_2_uint_new(ref buffer, 20 + i * recLen) / 1000000f;

					UInt32 total_seconds = Conversions.bytes_2_uint_new(ref buffer, 80 + i * recLen);
					ushort millisec = Conversions.bytes_2_ushort(ref buffer, 84 + i * recLen);
					ushort days = Conversions.bytes_2_ushort(ref buffer, 86 + i * recLen);
					ushort hours = Conversions.bytes_2_ushort(ref buffer, 88 + i * recLen);
					ushort min = Conversions.bytes_2_ushort(ref buffer, 90 + i * recLen);
					ushort sec = Conversions.bytes_2_ushort(ref buffer, 92 + i * recLen);

					sbMain.Append(string.Format("INSERT INTO dns_events(datetime_id, event_id, event_type, phase, u_value, d_u, u_declared, dt_start, dt_end, total_seconds, duration_millisec, duration_days, duration_hours, duration_min, duration_sec, is_finished, phase_num, is_earlier) VALUES (currval('dns_times_datetime_id_seq'), {0}, {1}, '{2}', {3}, {4}, {5}, '{6}', '{7}', {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16});\n",
					eventIndex,
					eventType,
					phase,
					u_value.ToString(new CultureInfo("en-US")),
					d_u.ToString(new CultureInfo("en-US")),
					u_declared.ToString(new CultureInfo("en-US")),
					dtStart.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
					dtEnd.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
					total_seconds,
					millisec, days, hours, min, sec,
					isFinished.ToString(),
					EmService.GetPhaseAsNumber(phase),
					wasEarlier.ToString()
					));

					// set ProgressBar position
					//cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
					//bw_.ReportProgress((int)cur_percent_progress_);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_dns_period(): i = " + i.ToString());
				return false;
			}
			if (wasEvent)
				tempSql = sbMain.ToString();
			else
				tempSql = "";

			return true;
		}

		#endregion

		#region Building SQL query for inserting AVG data from Device

		private bool xml2sql_avg_period(EmXmlAVG_PQP_A xmlAVG, ref EtPQP_A_XmlArchive xmlArchive, 
										ref EmSqlDataNode avgNode)
		{
			try
			{
				if (xmlAVG.DataPages.Length == 0)
				{
					EmService.WriteToLogFailed("AVG buffer length = 0");
					return false;
				}
				avgNode.SqlType = EmSqlDataNodeType.AVG;
				avgNode.Begin = xmlAVG.Start;
				avgNode.End = xmlAVG.End;
				byte[] buffer = xmlAVG.DataPages;

				if (buffer == null)
				{
					EmService.WriteToLogFailed("xml2sql_avg_period: NO AVG DATA!");
					return false;
				}

				bool U_transformer_enable = (Conversions.bytes_2_ushort(ref buffer, 80) == 0);
				if (xmlArchive.SysInfo.U_transformer_enable != U_transformer_enable)
					xmlArchive.SysInfo.U_transformer_enable = U_transformer_enable;
				short U_transformer_type = Conversions.bytes_2_short(ref buffer, 82);
				if (xmlArchive.SysInfo.U_transformer_type != U_transformer_type)
					xmlArchive.SysInfo.U_transformer_type = U_transformer_type;

				avgNode.Sql += string.Format("INSERT INTO avg_times (datetime_id, start_datetime, end_datetime, registration_id, device_id, period_id, parent_folder_id, f_nom, u_nom_lin, u_nom_ph, i_nom_ph, con_scheme, i_limit, f_limit, u_limit, current_sensor_type, u_transformer_enable, u_transformer_type, i_transformer_primary, i_transformer_enable) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18});\n",
							xmlAVG.Start.ToString("MM.dd.yyyy HH:mm:ss"),
							xmlAVG.End.ToString("MM.dd.yyyy HH:mm:ss"),
							"{0}", "{1}",   // reg_id, device_id
							(int)xmlAVG.AvgType,
							"{2}",   // parent_folder_id
							xmlArchive.SysInfo.F_Nominal.ToString(),
                            xmlArchive.SysInfo.U_NominalLinear.ToString(new CultureInfo("en-US")),
                            xmlArchive.SysInfo.U_NominalPhase.ToString(new CultureInfo("en-US")),
                            xmlArchive.SysInfo.I_NominalPhase.ToString(new CultureInfo("en-US")),
							(short)xmlArchive.ConnectionScheme,
                            xmlArchive.SysInfo.I_Limit.ToString(),
							xmlArchive.SysInfo.F_Limit.ToString(),
							xmlArchive.SysInfo.U_Limit.ToString(),
							xmlArchive.SysInfo.I_sensor_type.ToString(),
							xmlArchive.SysInfo.U_transformer_enable,
							xmlArchive.SysInfo.U_transformer_type,
							xmlArchive.SysInfo.I_transformer_primary, 
							xmlArchive.SysInfo.I_transformer_usage);

				EmService.WriteToLogGeneral("AVG type = " + (int)xmlAVG.AvgType);

				int recordsCount = buffer.Length / EtPQP_A_Device.AvgRecordLength_PQP_A;
				int curShift = 0;
				StringBuilder sbSql = new StringBuilder();

				avgNode.AvgFileName = EmService.GetAvgTmpFileName(
					xmlAVG.AvgType.ToString(), xmlAVG.Start);
				string avgFileFullPath = EmService.TEMP_IMAGE_DIR + avgNode.AvgFileName;
				if (File.Exists(avgFileFullPath)) File.Delete(avgFileFullPath);

				List<UInt32> listRecordId = new List<UInt32>(recordsCount);
                for (int iArch = 0; iArch < recordsCount; ++iArch)
				{
                    UInt32 recordId;

                    if (!xml2sql_avg_service_info(ref buffer, curShift, out recordId, ref listRecordId, 
						ref sbSql))
                    {
                        // set ProgressBar position
                        cur_percent_progress_ += 100.0 * 1.0 / cnt_steps_progress_;
                        bw_.ReportProgress((int)cur_percent_progress_);

						curShift += EtPQP_A_Device.AvgRecordLength_PQP_A;
                        continue;
                    }
					listRecordId.Add(recordId);

					xml2sql_avg_u_i_f(ref buffer, curShift, recordId, ref sbSql, ref xmlArchive);
					xml2sql_avg_power(ref buffer, curShift, recordId, ref sbSql, ref xmlArchive);
					xml2sql_avg_pqp(ref buffer, curShift, recordId, ref sbSql, ref xmlArchive);
					xml2sql_avg_angles(ref buffer, curShift, recordId, ref sbSql);
					xml2sql_avg_harmonic_power(ref buffer, curShift, recordId, xmlArchive.ConnectionScheme, ref sbSql,
						ref xmlArchive);
					xml2sql_avg_u_harmonics(ref buffer, curShift, recordId, ref sbSql, 
						xmlArchive.SysInfo.U_transformer_enable,
						xmlArchive.SysInfo.U_transformer_type);
					xml2sql_avg_u_interharmonics(ref buffer, curShift, recordId, ref sbSql,
						xmlArchive.SysInfo.U_transformer_enable,
						xmlArchive.SysInfo.U_transformer_type);
					xml2sql_avg_i_harmonics(ref buffer, curShift, recordId, ref sbSql, ref xmlArchive);
					xml2sql_avg_i_interharmonics(ref buffer, curShift, recordId, ref sbSql, ref xmlArchive);

					// если набралось слишком много замеров или если это последний цикл
					// то сбрасываем в файл
					if (((iArch % 500) == 0 && iArch != 0) || 
						(iArch == recordsCount - 1))
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

					curShift += EtPQP_A_Device.AvgRecordLength_PQP_A;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_period():  ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

        /// <summary>Service info</summary>
        private bool xml2sql_avg_service_info(ref byte[] buffer, int shift, out UInt32 recordId, 
										ref List<UInt32> listRecordId, ref StringBuilder sql)
        {
            try
            {
                CultureInfo ci = new CultureInfo("en-US");

                recordId = Conversions.bytes_2_uint_new(ref buffer, shift + 0);
				if (listRecordId.Contains(recordId))
				{
					EmService.WriteToLogFailed("Duplicate recordId!  " + recordId.ToString());
					return false;
				}

                DateTime start_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, shift + 12,
                                    "AVG record start date");
                DateTime end_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, shift + 40,
                                    "AVG record end date");

                if (start_datetime == DateTime.MinValue || end_datetime == DateTime.MinValue)
                {
                    EmService.WriteToLogFailed("Error in xml2sql_avg_period() dates:");
                    return false;
                }

                // Длительность интервала в милисекундах
                UInt32 length_millisec = Conversions.bytes_2_uint_new(ref buffer, shift + 204);

                // Суммарное количество сэмплов(выборок) в интервале усреднения
                UInt32 all_samples = Conversions.bytes_2_uint_new(ref buffer, shift + 208);

                // Количество сэмплов(выборок) в измерительных окнах, 15 значений
                ushort cnt_samples_in_win = 15;
                ushort[] samples_in_window = new ushort[cnt_samples_in_win];
                for (ushort iSW = 0; iSW < cnt_samples_in_win; ++iSW)
                {
                    samples_in_window[iSW] = Conversions.bytes_2_ushort(ref buffer, shift + 212 + iSW * 2);
                }

                // Количество измерительных окон в интервале усреднения
                // Синхронизированных с фазой A
                ushort cnt_windows_locked_A = Conversions.bytes_2_ushort(ref buffer, shift + 242);
                ushort cnt_windows_locked_B = Conversions.bytes_2_ushort(ref buffer, shift + 244);
                ushort cnt_windows_locked_C = Conversions.bytes_2_ushort(ref buffer, shift + 246);
                ushort cnt_windows_locked_AB = Conversions.bytes_2_ushort(ref buffer, shift + 248);
                ushort cnt_windows_locked_BC = Conversions.bytes_2_ushort(ref buffer, shift + 250);
                ushort cnt_windows_locked_CA = Conversions.bytes_2_ushort(ref buffer, shift + 252);
                // Не синхронизированных с сетью
                ushort cnt_not_locked = Conversions.bytes_2_ushort(ref buffer, shift + 254);
				// 0-запись Немаркирована, 1-запись маркирована
				ushort if_record_marked = Conversions.bytes_2_ushort(ref buffer, shift + 92);

				sql.Append("INSERT INTO avg_service_info(datetime_id, record_id, dt_start, dt_end, length_millisec, all_samples, samples_in_window, cnt_windows_locked_a, cnt_windows_locked_b, cnt_windows_locked_c, cnt_windows_locked_ab, cnt_windows_locked_bc, cnt_windows_locked_ca, cnt_windows_not_locked, if_record_marked) VALUES (");

                // adding pkey
                sql.Append("currval('avg_times_datetime_id_seq'), ");
                sql.Append(recordId.ToString() + ", '");
                sql.Append(start_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', '");
                sql.Append(end_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");
                sql.Append(length_millisec.ToString() + ", ");
                sql.Append(all_samples.ToString() + ", ");

                sql.Append("ARRAY[");
                for (int i = 0; i < 14; i++)
                    sql.Append(samples_in_window[i].ToString() + ", ");
                sql.Append(samples_in_window[14].ToString() + "],");

                sql.Append(cnt_windows_locked_A.ToString() + ", ");
                sql.Append(cnt_windows_locked_B.ToString() + ", ");
                sql.Append(cnt_windows_locked_C.ToString() + ", ");
                sql.Append(cnt_windows_locked_AB.ToString() + ", ");
                sql.Append(cnt_windows_locked_BC.ToString() + ", ");
				sql.Append(cnt_windows_locked_CA.ToString() + ", ");
				sql.Append(cnt_not_locked.ToString() + ", ");
				sql.Append(if_record_marked.ToString());

                sql.Append(");\n");

                return true;
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in xml2sql_avg_service_info():");
                throw;
            }
        }

        /// <summary>Частота, напряжение, ток</summary>
        private void xml2sql_avg_u_i_f(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			ref EtPQP_A_XmlArchive xmlArchive)
        {
            try
            {
                CultureInfo ci = new CultureInfo("en-US");

				float u_multiplier = 1;
				if (xmlArchive.SysInfo.U_transformer_enable)
					u_multiplier = EtPQP_A_Device.GetUTransformerMultiplier(xmlArchive.SysInfo.U_transformer_type);
				u_multiplier /= 1000000f;

				float i_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					i_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					i_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				i_multiplier /= 1000000f;

                // частота
                float f_a = Conversions.bytes_2_uint_new(ref buffer, shift + 256) / 1000f;
				float f_b = Conversions.bytes_2_uint_new(ref buffer, shift + 260) / 1000f;
				float f_c = Conversions.bytes_2_uint_new(ref buffer, shift + 264) / 1000f;
				float f_ab = Conversions.bytes_2_uint_new(ref buffer, shift + 268) / 1000f;
				float f_bc = Conversions.bytes_2_uint_new(ref buffer, shift + 272) / 1000f;
				float f_ca = Conversions.bytes_2_uint_new(ref buffer, shift + 276) / 1000f;

                // напряжение - действующие значения
				float u_a = Conversions.bytes_2_uint_new(ref buffer, shift + 280) * u_multiplier;
				float u_b = Conversions.bytes_2_uint_new(ref buffer, shift + 284) * u_multiplier;
				float u_c = Conversions.bytes_2_uint_new(ref buffer, shift + 288) * u_multiplier;
				float u_ab = Conversions.bytes_2_uint_new(ref buffer, shift + 292) * u_multiplier;
				float u_bc = Conversions.bytes_2_uint_new(ref buffer, shift + 296) * u_multiplier;
				float u_ca = Conversions.bytes_2_uint_new(ref buffer, shift + 300) * u_multiplier;

                // ток - действующие значения
				float i_a = Conversions.bytes_2_uint_new(ref buffer, shift + 304) * i_multiplier;
				float i_b = Conversions.bytes_2_uint_new(ref buffer, shift + 308) * i_multiplier;
				float i_c = Conversions.bytes_2_uint_new(ref buffer, shift + 312) * i_multiplier;
				float i_n = Conversions.bytes_2_uint_new(ref buffer, shift + 316) * i_multiplier;

                // напряжение - постоянная составляющая
				float u_a_const = Conversions.bytes_2_int(ref buffer, shift + 320) * u_multiplier;
				float u_b_const = Conversions.bytes_2_int(ref buffer, shift + 324) * u_multiplier;
				float u_c_const = Conversions.bytes_2_int(ref buffer, shift + 328) * u_multiplier;
				float u_ab_const = Conversions.bytes_2_int(ref buffer, shift + 332) * u_multiplier;
				float u_bc_const = Conversions.bytes_2_int(ref buffer, shift + 336) * u_multiplier;
				float u_ca_const = Conversions.bytes_2_int(ref buffer, shift + 340) * u_multiplier;

                // ток - постоянная составляющая
				float i_a_const = Conversions.bytes_2_int(ref buffer, shift + 344) * i_multiplier;
				float i_b_const = Conversions.bytes_2_int(ref buffer, shift + 348) * i_multiplier;
				float i_c_const = Conversions.bytes_2_int(ref buffer, shift + 352) * i_multiplier;
				float i_n_const = Conversions.bytes_2_int(ref buffer, shift + 356) * i_multiplier;

                // напряжение - средневыпрямленное значение
				float u_a_avdirect = Conversions.bytes_2_int(ref buffer, shift + 360) * u_multiplier;
				float u_b_avdirect = Conversions.bytes_2_int(ref buffer, shift + 364) * u_multiplier;
				float u_c_avdirect = Conversions.bytes_2_int(ref buffer, shift + 368) * u_multiplier;
				float u_ab_avdirect = Conversions.bytes_2_int(ref buffer, shift + 372) * u_multiplier;
				float u_bc_avdirect = Conversions.bytes_2_int(ref buffer, shift + 376) * u_multiplier;
				float u_ca_avdirect = Conversions.bytes_2_int(ref buffer, shift + 380) * u_multiplier;

                // ток - средневыпрямленное значение
				float i_a_avdirect = Conversions.bytes_2_int(ref buffer, shift + 384) * i_multiplier;
				float i_b_avdirect = Conversions.bytes_2_int(ref buffer, shift + 388) * i_multiplier;
				float i_c_avdirect = Conversions.bytes_2_int(ref buffer, shift + 392) * i_multiplier;
				float i_n_avdirect = Conversions.bytes_2_int(ref buffer, shift + 396) * i_multiplier;

                // напряжение - 1-ая гармоника
				float u_a_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 400) * u_multiplier;
				float u_b_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 404) * u_multiplier;
				float u_c_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 408) * u_multiplier;
				float u_ab_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 412) * u_multiplier;
				float u_bc_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 416) * u_multiplier;
				float u_ca_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 420) * u_multiplier;

                // ток - 1-ая гармоника
				float i_a_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 424) * i_multiplier;
				float i_b_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 428) * i_multiplier;
				float i_c_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 432) * i_multiplier;
				float i_n_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 436) * i_multiplier;

                sql.Append(String.Format("INSERT INTO avg_u_i_f(datetime_id, record_id, f_a, f_b, f_c, f_ab, f_bc, f_ca, u_a, u_b, u_c, u_ab, u_bc, u_ca, u_a_const, u_b_const, u_c_const, u_ab_const, u_bc_const, u_ca_const, u_a_avdirect, u_b_avdirect, u_c_avdirect, u_ab_avdirect, u_bc_avdirect, u_ca_avdirect, u_a_1harm, u_b_1harm, u_c_1harm, u_ab_1harm, u_bc_1harm, u_ca_1harm, i_a, i_b, i_c, i_n, i_a_const, i_b_const, i_c_const, i_n_const, i_a_avdirect, i_b_avdirect, i_c_avdirect, i_n_avdirect, i_a_1harm, i_b_1harm, i_c_1harm, i_n_1harm) VALUES (currval('avg_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46});\n",
                    recordId,
                    f_a.ToString(ci), f_b.ToString(ci), f_c.ToString(ci), f_ab.ToString(ci), 
					f_bc.ToString(ci), f_ca.ToString(ci),
                    u_a.ToString(ci), u_b.ToString(ci), u_c.ToString(ci), u_ab.ToString(ci), 
					u_bc.ToString(ci), u_ca.ToString(ci),
                    u_a_const.ToString(ci), u_b_const.ToString(ci), u_c_const.ToString(ci),
                    u_ab_const.ToString(ci), u_bc_const.ToString(ci), u_ca_const.ToString(ci),
                    u_a_avdirect.ToString(ci), u_b_avdirect.ToString(ci), u_c_avdirect.ToString(ci),
                    u_ab_avdirect.ToString(ci), u_bc_avdirect.ToString(ci), u_ca_avdirect.ToString(ci),
                    u_a_1harm.ToString(ci), u_b_1harm.ToString(ci), u_c_1harm.ToString(ci),
                    u_ab_1harm.ToString(ci), u_bc_1harm.ToString(ci), u_ca_1harm.ToString(ci),
                    i_a.ToString(ci), i_b.ToString(ci), i_c.ToString(ci), i_n.ToString(ci),
                    i_a_const.ToString(ci), i_b_const.ToString(ci), i_c_const.ToString(ci), i_n_const.ToString(ci),
                    i_a_avdirect.ToString(ci), i_b_avdirect.ToString(ci), i_c_avdirect.ToString(ci), 
					i_n_avdirect.ToString(ci),
                    i_a_1harm.ToString(ci), i_b_1harm.ToString(ci), i_c_1harm.ToString(ci), i_n_1harm.ToString(ci)
                    ));
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in xml2sql_avg_u_i_f():");
                throw;
            }
        }

        /// <summary>Power</summary>
        private void xml2sql_avg_power(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			ref EtPQP_A_XmlArchive xmlArchive)
        {
            try
            {
                CultureInfo ci = new CultureInfo("en-US");

				float p_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.U_transformer_enable)
					p_multiplier *= EtPQP_A_Device.GetUTransformerMultiplier(xmlArchive.SysInfo.U_transformer_type);

				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					p_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					p_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				p_multiplier /= 1000000f;

                // Мощность активная
                float p_a = Conversions.bytes_2_int(ref buffer, shift + 440) * p_multiplier;
				float p_b = Conversions.bytes_2_int(ref buffer, shift + 444) * p_multiplier;
				float p_c = Conversions.bytes_2_int(ref buffer, shift + 448) * p_multiplier;
				float p_sum = Conversions.bytes_2_int(ref buffer, shift + 452) * p_multiplier;
				float p_1 = Conversions.bytes_2_int(ref buffer, shift + 456) * p_multiplier;
				float p_2 = Conversions.bytes_2_int(ref buffer, shift + 460) * p_multiplier;
				float p_12sum = Conversions.bytes_2_int(ref buffer, shift + 464) * p_multiplier;

                // Мощность полная
				float s_a = Conversions.bytes_2_int(ref buffer, shift + 468) * p_multiplier;
				float s_b = Conversions.bytes_2_int(ref buffer, shift + 472) * p_multiplier;
				float s_c = Conversions.bytes_2_int(ref buffer, shift + 476) * p_multiplier;
				float s_sum = Conversions.bytes_2_int(ref buffer, shift + 480) * p_multiplier;
				float s_1 = Conversions.bytes_2_int(ref buffer, shift + 484) * p_multiplier;
				float s_2 = Conversions.bytes_2_int(ref buffer, shift + 488) * p_multiplier;
				float s_12sum = Conversions.bytes_2_int(ref buffer, shift + 492) * p_multiplier;

                // Мощность реактивная (по первой гармонике)
				float q_a = Conversions.bytes_2_int(ref buffer, shift + 496) * p_multiplier;
				float q_b = Conversions.bytes_2_int(ref buffer, shift + 500) * p_multiplier;
				float q_c = Conversions.bytes_2_int(ref buffer, shift + 504) * p_multiplier;
				float q_sum = Conversions.bytes_2_int(ref buffer, shift + 508) * p_multiplier;
				float q_1 = Conversions.bytes_2_int(ref buffer, shift + 512) * p_multiplier;
				float q_2 = Conversions.bytes_2_int(ref buffer, shift + 516) * p_multiplier;
				float q_12sum = Conversions.bytes_2_int(ref buffer, shift + 520) * p_multiplier;

				float tanP = 0;
	            if (xmlArchive.ConnectionScheme == ConnectScheme.Ph3W3 ||
	                xmlArchive.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
	            {
		            if (Math.Abs(p_12sum) >= 0.05 && Math.Abs(q_12sum) >= 0.05)
		            {
			            tanP = (p_12sum == 0 ? 0 : q_12sum/p_12sum);
			            tanP = (float) Math.Round((double) tanP, 8);
		            }
	            }
	            else if(xmlArchive.ConnectionScheme == ConnectScheme.Ph1W2)
	            {
					if (Math.Abs(p_a) >= 0.05 && Math.Abs(q_a) >= 0.05)
					{
						tanP = (p_a == 0 ? 0 : q_a / p_a);
						tanP = (float)Math.Round((double)tanP, 8);
					}
	            }
				else  // 3ph 4w
				{
					if (Math.Abs(p_sum) >= 0.05 && Math.Abs(q_sum) >= 0.05)
					{
						tanP = (p_sum == 0 ? 0 : q_sum / p_sum);
						tanP = (float)Math.Round((double)tanP, 8);
					}
				}

	            // Коэффициент мощности Kp
                float kp_a = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 524);
                float kp_b = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 528);
                float kp_c = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 532);
                float kp_abc = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 536);
                float kp_12 = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 540);

				sql.Append(String.Format("INSERT INTO avg_power(datetime_id, record_id, p_a, p_b, p_c, p_sum, p_1, p_2, p_12sum, s_a, s_b, s_c, s_sum, s_1, s_2, s_12sum, q_a, q_b, q_c, q_sum, q_1, q_2, q_12sum, kp_a, kp_b, kp_c, kp_abc, kp_12, tangent_p) VALUES (currval('avg_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27});\n",
                    recordId,
                    p_a.ToString(ci), p_b.ToString(ci), p_c.ToString(ci), p_sum.ToString(ci), 
                    p_1.ToString(ci), p_2.ToString(ci), p_12sum.ToString(ci),
                    s_a.ToString(ci), s_b.ToString(ci), s_c.ToString(ci), s_sum.ToString(ci),
                    s_1.ToString(ci), s_2.ToString(ci), s_12sum.ToString(ci),
                    q_a.ToString(ci), q_b.ToString(ci), q_c.ToString(ci), q_sum.ToString(ci),
                    q_1.ToString(ci), q_2.ToString(ci), q_12sum.ToString(ci),
                    kp_a.ToString(ci), kp_b.ToString(ci), kp_c.ToString(ci), kp_abc.ToString(ci), kp_12.ToString(ci),
					tanP.ToString(ci)
                    ));
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in xml2sql_avg_p():");
                throw;
            }
        }

        /// <summary>PQP</summary>
        private void xml2sql_avg_pqp(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			ref EtPQP_A_XmlArchive xmlArchive)
        {
            try
            {
                CultureInfo ci = new CultureInfo("en-US");

				float u_multiplier = 1;
				if (xmlArchive.SysInfo.U_transformer_enable)
					u_multiplier = EtPQP_A_Device.GetUTransformerMultiplier(xmlArchive.SysInfo.U_transformer_type);
				u_multiplier /= 1000000f;

				float i_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					i_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					i_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				i_multiplier /= 1000000f;

				float p_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.U_transformer_enable)
					p_multiplier *= EtPQP_A_Device.GetUTransformerMultiplier(xmlArchive.SysInfo.U_transformer_type);

				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					p_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					p_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				p_multiplier /= 1000000f;

                // Напряжение прямой последовательности 
                float u_1 = Conversions.bytes_2_int(ref buffer, shift + 544) * u_multiplier;
                // Напряжение обратной последовательности
				float u_2 = Conversions.bytes_2_int(ref buffer, shift + 548) * u_multiplier;
                // Напряжение нулевой последовательности
				float u_0 = Conversions.bytes_2_int(ref buffer, shift + 552) * u_multiplier;
                // Коэффициент обратной последовательности
				float k2u = Conversions.bytes_2_int(ref buffer, shift + 556) / 1342177.28f;
                // Коэффициент нулевой последовательности
				float k0u = Conversions.bytes_2_int(ref buffer, shift + 560) / 1342177.28f;
                // Ток прямой последовательности
				float i_1 = Conversions.bytes_2_int(ref buffer, shift + 564) * i_multiplier;
                // Ток обратной последовательности
				float i_2 = Conversions.bytes_2_int(ref buffer, shift + 568) * i_multiplier;
                // Ток нулевой последовательности
				float i_0 = Conversions.bytes_2_int(ref buffer, shift + 572) * i_multiplier;
                // Мощность прямой последовательности
                float p_1 = Conversions.bytes_2_int(ref buffer, shift + 576) * p_multiplier;
                // Мощность обратной последовательности
				float p_2 = Conversions.bytes_2_int(ref buffer, shift + 580) * p_multiplier;
                // Мощность нулевой последовательности
				float p_0 = Conversions.bytes_2_int(ref buffer, shift + 584) * p_multiplier;
                // Угол мощности прямой последовательности
                float angle_p_1 = Conversions.bytes_2_int(ref buffer, shift + 588) / 1000f;
                // Угол мощности обратной последовательности
				float angle_p_2 = Conversions.bytes_2_int(ref buffer, shift + 592) / 1000f;
                // Угол мощности нулевой последовательности
				float angle_p_0 = Conversions.bytes_2_int(ref buffer, shift + 596) / 1000f;

                // Отклонение установившегося напряжения [относительное]
				float rd_u = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 600) * 100;
                // Отклонение 1 гармоники от номинала – фаза A [относительное]
                float rd_u_1harm_A = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 604) * 100;
				float rd_u_1harm_B = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 608) * 100;
				float rd_u_1harm_C = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 612) * 100;
				float rd_u_1harm_AB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 616) * 100;
				float rd_u_1harm_BC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 620) * 100;
				float rd_u_1harm_CA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 624) * 100;

                // Положительное отклонение напряжения - фаза A [абсолютное]
				float d_u_pos_A = Conversions.bytes_2_int(ref buffer, shift + 628) * u_multiplier;
				float d_u_pos_B = Conversions.bytes_2_int(ref buffer, shift + 632) * u_multiplier;
				float d_u_pos_C = Conversions.bytes_2_int(ref buffer, shift + 636) * u_multiplier;
				float d_u_pos_AB = Conversions.bytes_2_int(ref buffer, shift + 640) * u_multiplier;
				float d_u_pos_BC = Conversions.bytes_2_int(ref buffer, shift + 644) * u_multiplier;
				float d_u_pos_CA = Conversions.bytes_2_int(ref buffer, shift + 648) * u_multiplier;

                // Отрицательное отклонение напряжения - фаза A [абсолютное]
				float d_u_neg_A = Conversions.bytes_2_int(ref buffer, shift + 652) * u_multiplier;
				float d_u_neg_B = Conversions.bytes_2_int(ref buffer, shift + 656) * u_multiplier;
				float d_u_neg_C = Conversions.bytes_2_int(ref buffer, shift + 660) * u_multiplier;
				float d_u_neg_AB = Conversions.bytes_2_int(ref buffer, shift + 664) * u_multiplier;
				float d_u_neg_BC = Conversions.bytes_2_int(ref buffer, shift + 668) * u_multiplier;
				float d_u_neg_CA = Conversions.bytes_2_int(ref buffer, shift + 672) * u_multiplier;

                // Положительное отклонение напряжения - фаза A [относительное]
				float rd_u_pos_A = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 676) * 100;
				float rd_u_pos_B = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 680) * 100;
				float rd_u_pos_C = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 684) * 100;
				float rd_u_pos_AB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 688) * 100;
				float rd_u_pos_BC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 692) * 100;
				float rd_u_pos_CA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 696) * 100;

                // Отрицательное отклонение напряжения - фаза A [относительное]
				float rd_u_neg_A = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 700) * 100;
				float rd_u_neg_B = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 704) * 100;
				float rd_u_neg_C = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 708) * 100;
				float rd_u_neg_AB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 712) * 100;
				float rd_u_neg_BC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 716) * 100;
				float rd_u_neg_CA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 720) * 100;

				sql.Append(String.Format("INSERT INTO avg_pqp(datetime_id, record_id, u_1, u_2, u_0, k_2u, k_0u, i_1, i_2, i_0, p_1, p_2, p_0, angle_p_1, angle_p_2, angle_p_0, rd_u, rd_u_1harm_a, rd_u_1harm_b, rd_u_1harm_c, rd_u_1harm_ab, rd_u_1harm_bc, rd_u_1harm_ca, d_u_pos_a, d_u_pos_b, d_u_pos_c, d_u_pos_ab, d_u_pos_bc, d_u_pos_ca, d_u_neg_a, d_u_neg_b, d_u_neg_c, d_u_neg_ab, d_u_neg_bc, d_u_neg_ca, rd_u_pos_a, rd_u_pos_b, rd_u_pos_c, rd_u_pos_ab, rd_u_pos_bc, rd_u_pos_ca, rd_u_neg_a, rd_u_neg_b, rd_u_neg_c, rd_u_neg_ab, rd_u_neg_bc, rd_u_neg_ca) VALUES (currval('avg_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45});\n",
                    recordId,
					u_1.ToString(ci), u_2.ToString(ci), u_0.ToString(ci),
					k2u.ToString(ci), k0u.ToString(ci), i_1.ToString(ci), i_2.ToString(ci), i_0.ToString(ci),
					p_1.ToString(ci), p_2.ToString(ci), p_0.ToString(ci), angle_p_1.ToString(ci), angle_p_2.ToString(ci), angle_p_0.ToString(ci),
					rd_u.ToString(ci), rd_u_1harm_A.ToString(ci), rd_u_1harm_B.ToString(ci), rd_u_1harm_C.ToString(ci),
					rd_u_1harm_AB.ToString(ci), rd_u_1harm_BC.ToString(ci), rd_u_1harm_CA.ToString(ci),
					d_u_pos_A.ToString(ci), d_u_pos_B.ToString(ci), d_u_pos_C.ToString(ci),
					d_u_pos_AB.ToString(ci), d_u_pos_BC.ToString(ci), d_u_pos_CA.ToString(ci),
					d_u_neg_A.ToString(ci), d_u_neg_B.ToString(ci), d_u_neg_C.ToString(ci),
					d_u_neg_AB.ToString(ci), d_u_neg_BC.ToString(ci), d_u_neg_CA.ToString(ci),
					rd_u_pos_A.ToString(ci), rd_u_pos_B.ToString(ci), rd_u_pos_C.ToString(ci), 
					rd_u_pos_AB.ToString(ci), rd_u_pos_BC.ToString(ci),
					rd_u_pos_CA.ToString(ci), rd_u_neg_A.ToString(ci), rd_u_neg_B.ToString(ci), rd_u_neg_C.ToString(ci), 
					rd_u_neg_AB.ToString(ci), rd_u_neg_BC.ToString(ci), rd_u_neg_CA.ToString(ci)
                    ));
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in xml2sql_avg_pqp():");
                throw;
            }
        }

		/// <summary>Angles</summary>
		private void xml2sql_avg_angles(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				float a_multiplier = 1f / 1000f;

				// Углы между напряжениями и токами 1-ой гармоники
				// Между фазными напряжениями UA UB
				float angle_ua_ub = Conversions.bytes_2_int(ref buffer, shift + 724) * a_multiplier;
				float angle_ub_uc = Conversions.bytes_2_int(ref buffer, shift + 728) * a_multiplier;
				float angle_uc_ua = Conversions.bytes_2_int(ref buffer, shift + 732) * a_multiplier;

				// Между фазным напряжением UA и током IA
				float angle_ua_ia = Conversions.bytes_2_int(ref buffer, shift + 736) * a_multiplier;
				float angle_ub_ib = Conversions.bytes_2_int(ref buffer, shift + 740) * a_multiplier;
				float angle_uc_ic = Conversions.bytes_2_int(ref buffer, shift + 744) * a_multiplier;

				// Между междуфазными напряжениями AB и CB
				float angle_uab_ucb = Conversions.bytes_2_int(ref buffer, shift + 748) * a_multiplier;
				float angle_uab_ia = Conversions.bytes_2_int(ref buffer, shift + 752) * a_multiplier;
				float angle_ucb_ic = Conversions.bytes_2_int(ref buffer, shift + 756) * a_multiplier;

				// Углы между напряжениями и токами 1-ой гармоники (ВСПОМОГАТЕЛЬНЫЕ)
				// Между междуфазными напряжениями AB и BC
				float angle_uab_ubc = Conversions.bytes_2_int(ref buffer, shift + 10600) * a_multiplier;
				float angle_ubc_uca = Conversions.bytes_2_int(ref buffer, shift + 10604) * a_multiplier;
				float angle_uca_uab = Conversions.bytes_2_int(ref buffer, shift + 10608) * a_multiplier;

				// Между междуфазным напряжением BC и током IA (вспомогательный угол для графики)
				float angle_ubc_ia = Conversions.bytes_2_int(ref buffer, shift + 10612) * a_multiplier;
				float angle_ubc_ib = Conversions.bytes_2_int(ref buffer, shift + 10616) * a_multiplier;
				float angle_ubc_ic = Conversions.bytes_2_int(ref buffer, shift + 10620) * a_multiplier;
				float angle_ubc_in = Conversions.bytes_2_int(ref buffer, shift + 10624) * a_multiplier;

				sql.Append(String.Format("INSERT INTO avg_angles(datetime_id, record_id, angle_ua_ub, angle_ub_uc, angle_uc_ua, angle_ua_ia, angle_ub_ib, angle_uc_ic, angle_uab_ucb, angle_uab_ia, angle_ucb_ic, angle_uab_ubc, angle_ubc_uca, angle_uca_uab, angle_ubc_ia, angle_ubc_ib, angle_ubc_ic, angle_ubc_in) VALUES (currval('avg_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16});\n",
					recordId,
					angle_ua_ub.ToString(ci), angle_ub_uc.ToString(ci), angle_uc_ua.ToString(ci),
					angle_ua_ia.ToString(ci), angle_ub_ib.ToString(ci), angle_uc_ic.ToString(ci),
					angle_uab_ucb.ToString(ci), angle_uab_ia.ToString(ci), angle_ucb_ic.ToString(ci),
					angle_uab_ubc.ToString(ci), angle_ubc_uca.ToString(ci), angle_uca_uab.ToString(ci),
					angle_ubc_ia.ToString(ci), angle_ubc_ib.ToString(ci), angle_ubc_ic.ToString(ci), 
					angle_ubc_in.ToString(ci)
					));
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_angles():");
				throw;
			}
		}

		/// <summary>Voltage(U) Harmonics</summary>
		private void xml2sql_avg_u_harmonics(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			bool use_u_transformer, short u_transformer_type)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				float u_multiplier = 1;
				if (use_u_transformer) u_multiplier = EtPQP_A_Device.GetUTransformerMultiplier(u_transformer_type);
				u_multiplier /= 1000000f;

				// Количество измерительных окон в интервале усреднения
				// Не синхронизированных с сетью
				ushort cnt_not_locked = Conversions.bytes_2_ushort(ref buffer, shift + 254);

				// Ua
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_a = Conversions.bytes_2_int(ref buffer, shift + 880) * u_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_a[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 884 + iOrder * 4) * u_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_a[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1084 + iOrder * 4) * 100;
				}
				// Ub
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_b = Conversions.bytes_2_int(ref buffer, shift + 1284) * u_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_b[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 1288 + iOrder * 4) * u_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_b[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1488 + iOrder * 4) * 100;
				}
				// Uc
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_c = Conversions.bytes_2_int(ref buffer, shift + 1688) * u_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_c[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 1692 + iOrder * 4) * u_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_c[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1892 + iOrder * 4) * 100;
				}

				sql.Append(String.Format("INSERT INTO avg_u_phase_harmonics VALUES (currval('avg_times_datetime_id_seq'), {0}, {1},", recordId, summ_for_order_more_1_a.ToString(ci)));

				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_a[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_a[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_b.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_b[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_b[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_c.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_c[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_c[iParam].ToString(ci)));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");

				// Uab
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_ab = Conversions.bytes_2_int(ref buffer, shift + 2092) * u_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_ab = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_ab[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 2096 + iOrder * 4) * u_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_ab = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_ab[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 2296 + iOrder * 4) * 100;
				}
				// Ubc
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_bc = Conversions.bytes_2_int(ref buffer, shift + 2496) * u_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_bc = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_bc[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 2500 + iOrder * 4) * u_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_bc = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_bc[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 2700 + iOrder * 4) * 100;
				}
				// Uca
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_ca = Conversions.bytes_2_int(ref buffer, shift + 2900) * u_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_ca = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_ca[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 2904 + iOrder * 4) * u_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_ca = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_ca[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3104 + iOrder * 4) * 100;
				}

				sql.Append(String.Format("INSERT INTO avg_u_lin_harmonics VALUES (currval('avg_times_datetime_id_seq'), {0}, {1},", recordId, summ_for_order_more_1_ab.ToString(ci)));

				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_ab[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_ab[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_bc.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_bc[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_bc[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_ca.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_ca[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_ca[iParam].ToString(ci)));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_u_harmonics():");
				throw;
			}
		}

		/// <summary>Voltage(U) InterHarmonics</summary>
		private void xml2sql_avg_u_interharmonics(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			bool use_u_transformer, short u_transformer_type)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				float u_multiplier = 1;
				if (use_u_transformer) u_multiplier = EtPQP_A_Device.GetUTransformerMultiplier(u_transformer_type);
				u_multiplier /= 1000000f;

				// Ua
				// Среднеквадратическое значение субгармонической группы
				float avg_square_a = Conversions.bytes_2_int(ref buffer, shift + 4920) * u_multiplier;
				// Среднеквадратическое значение интергармонических групп порядков 1…50
				float[] avg_square_order_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_a[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 4924 + iOrder * 4) * u_multiplier;
				}
				// Ub
				float avg_square_b = Conversions.bytes_2_int(ref buffer, shift + 5124) * u_multiplier;
				float[] avg_square_order_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_b[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 5128 + iOrder * 4) * u_multiplier;
				}
				// Uc
				float avg_square_c = Conversions.bytes_2_int(ref buffer, shift + 5328) * u_multiplier;
				float[] avg_square_order_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_c[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 5332 + iOrder * 4) * u_multiplier;
				}

				sql.Append(String.Format("INSERT INTO avg_u_ph_interharmonics VALUES (currval('avg_times_datetime_id_seq'), {0}, {1},", recordId, avg_square_a.ToString(ci)));

				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_a[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_b.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_b[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_c.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_c[iParam].ToString(ci)));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");

				// Uab
				// Среднеквадратическое значение субгармонической группы
				avg_square_a = Conversions.bytes_2_int(ref buffer, shift + 5532) * u_multiplier;
				// Среднеквадратическое значение интергармонических групп порядков 1…50
				avg_square_order_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_a[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 5536 + iOrder * 4) * u_multiplier;
				}
				// Ubc
				avg_square_b = Conversions.bytes_2_int(ref buffer, shift + 5736) * u_multiplier;
				avg_square_order_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_b[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 5740 + iOrder * 4) * u_multiplier;
				}
				// Uca
				avg_square_c = Conversions.bytes_2_int(ref buffer, shift + 5940) * u_multiplier;
				avg_square_order_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_c[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 5944 + iOrder * 4) * u_multiplier;
				}

				sql.Append(String.Format("INSERT INTO avg_u_lin_interharmonics VALUES (currval('avg_times_datetime_id_seq'), {0}, {1},", recordId, avg_square_a.ToString(ci)));

				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_a[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_b.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_b[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_c.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_c[iParam].ToString(ci)));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_u_interharmonics():");
				throw;
			}
		}

		/// <summary>Current(I) Harmonics</summary>
		private void xml2sql_avg_i_harmonics(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			ref EtPQP_A_XmlArchive xmlArchive)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				float i_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					i_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					i_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				i_multiplier /= 1000000f;

				// Ia
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_a = Conversions.bytes_2_int(ref buffer, shift + 3304) * i_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_a[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 3308 + iOrder * 4) * i_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_a[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3508 + iOrder * 4) * 100;
				}
				// Ib
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_b = Conversions.bytes_2_int(ref buffer, shift + 3708) * i_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_b[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 3712 + iOrder * 4) * i_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_b[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3912 + iOrder * 4) * 100;
				}
				// Ic
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_c = Conversions.bytes_2_int(ref buffer, shift + 4112) * i_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_c[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 4116 + iOrder * 4) * i_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_c[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 4316 + iOrder * 4) * 100;
				}
				// IN
				// Суммарное значение для гармонических подгрупп порядка > 1
				float summ_for_order_more_1_n = Conversions.bytes_2_int(ref buffer, shift + 4516) * i_multiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				float[] order_value_n = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_value_n[iOrder] = 
						Conversions.bytes_2_int(ref buffer, shift + 4520 + iOrder * 4) * i_multiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				float[] order_coeff_n = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					order_coeff_n[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 4720 + iOrder * 4) * 100;
				}

				sql.Append(String.Format("INSERT INTO avg_i_harmonics VALUES (currval('avg_times_datetime_id_seq'), {0}, {1},", recordId, summ_for_order_more_1_a.ToString(ci)));

				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_a[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_a[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_b.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_b[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_b[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_c.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_c[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_c[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", summ_for_order_more_1_n.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_value_n[iParam].ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", order_coeff_n[iParam].ToString(ci)));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_i_harmonics():");
				throw;
			}
		}

		/// <summary>Current(I) InterHarmonics</summary>
		private void xml2sql_avg_i_interharmonics(ref byte[] buffer, int shift, UInt32 recordId, ref StringBuilder sql,
			ref EtPQP_A_XmlArchive xmlArchive)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				float i_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					i_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					i_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				i_multiplier /= 1000000f;

				// Ia
				// Среднеквадратическое значение субгармонической группы
				float avg_square_a = Conversions.bytes_2_int(ref buffer, shift + 6144) * i_multiplier;
				// Среднеквадратическое значение интергармонических групп порядков 1…50
				float[] avg_square_order_a = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_a[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6148 + iOrder * 4) * i_multiplier;
				}
				// Ib
				float avg_square_b = Conversions.bytes_2_int(ref buffer, shift + 6348) * i_multiplier;
				float[] avg_square_order_b = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_b[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6352 + iOrder * 4) * i_multiplier;
				}
				// Ic
				float avg_square_c = Conversions.bytes_2_int(ref buffer, shift + 6552) * i_multiplier;
				float[] avg_square_order_c = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_c[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6556 + iOrder * 4) * i_multiplier;
				}
				// In
				float avg_square_n = Conversions.bytes_2_int(ref buffer, shift + 6756) * i_multiplier;
				float[] avg_square_order_n = new float[50];
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					avg_square_order_n[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6760 + iOrder * 4) * i_multiplier;
				}

				sql.Append(String.Format("INSERT INTO avg_i_interharmonics VALUES (currval('avg_times_datetime_id_seq'), {0}, {1},", recordId, avg_square_a.ToString(ci)));

				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_a[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_b.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_b[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_c.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_c[iParam].ToString(ci)));
				sql.Append(string.Format(" {0},", avg_square_n.ToString(ci)));
				for (int iParam = 0; iParam < 50; ++iParam)
					sql.Append(string.Format(" {0},", avg_square_order_n[iParam].ToString(ci)));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_i_interharmonics():");
				throw;
			}
		}

		/// <summary>Harmonic Power</summary>
		private void xml2sql_avg_harmonic_power(ref byte[] buffer, int shift, UInt32 recordId, 
										ConnectScheme connScheme, ref StringBuilder sql,
										ref EtPQP_A_XmlArchive xmlArchive)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");

				float p_multiplier = xmlArchive.SysInfo.I_Limit;
				if (xmlArchive.SysInfo.U_transformer_enable)
					p_multiplier *= EtPQP_A_Device.GetUTransformerMultiplier(xmlArchive.SysInfo.U_transformer_type);

				if (xmlArchive.SysInfo.I_transformer_usage == 1 || xmlArchive.SysInfo.I_transformer_usage == 2)
				{
					p_multiplier *= xmlArchive.SysInfo.I_transformer_primary;
					p_multiplier /= xmlArchive.SysInfo.I_TransformerSecondary;
				}
				p_multiplier /= 1000000f;

				float[,] harm_power_A = null;
				float[,] harm_power_B = null;
				float[,] harm_power_C = null;
				float[,] harm_power_1 = null;
				float[,] harm_power_2 = null;
				float[,] harm_power_SUM = null;
				int harm_shift;

				if (connScheme != ConnectScheme.Ph3W3 && connScheme != ConnectScheme.Ph3W3_B_calc)
				{
					harm_power_A = new float[3, 50];
					harm_shift = shift + 6960;
					// 3 типа - это активная, реактивная и угол
					for (int iType = 0; iType < 3; ++iType)
					{
						for (int iHarm = 0; iHarm < 50; ++iHarm)
						{
							harm_power_A[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) harm_power_A[iType, iHarm] *= p_multiplier;
							harm_shift += 4;
						}
					}

					if (connScheme != ConnectScheme.Ph1W2)
					{
						harm_power_B = new float[3, 50];
						harm_shift = shift + 7560;
						for (int iType = 0; iType < 3; ++iType)
						{
							for (int iHarm = 0; iHarm < 50; ++iHarm)
							{
								harm_power_B[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
								if (iType < 2) harm_power_B[iType, iHarm] *= p_multiplier;
								harm_shift += 4;
							}
						}

						harm_power_C = new float[3, 50];
						harm_shift = shift + 8160;
						for (int iType = 0; iType < 3; ++iType)
						{
							for (int iHarm = 0; iHarm < 50; ++iHarm)
							{
								harm_power_C[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
								if (iType < 2) harm_power_C[iType, iHarm] *= p_multiplier;
								harm_shift += 4;
							}
						}
					}
				}
				else
				{
					harm_power_1 = new float[3, 50];
					harm_shift = shift + 8760;
					for (int iType = 0; iType < 3; ++iType)
					{
						for (int iHarm = 0; iHarm < 50; ++iHarm)
						{
							harm_power_1[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) harm_power_1[iType, iHarm] *= p_multiplier;
							harm_shift += 4;
						}
					}

					harm_power_2 = new float[3, 50];
					harm_shift = shift + 9360;
					for (int iType = 0; iType < 3; ++iType)
					{
						for (int iHarm = 0; iHarm < 50; ++iHarm)
						{
							harm_power_2[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) harm_power_2[iType, iHarm] *= p_multiplier;
							harm_shift += 4;
						}
					}
				}

				if (connScheme != ConnectScheme.Ph1W2)
				{
					harm_power_SUM = new float[3, 50];
					harm_shift = shift + 9960;
					for (int iType = 0; iType < 3; ++iType)
					{
						for (int iHarm = 0; iHarm < 50; ++iHarm)
						{
							harm_power_SUM[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) harm_power_SUM[iType, iHarm] *= p_multiplier;
							harm_shift += 4;
						}
					}
				}

				sql.Append("INSERT INTO avg_harm_power(datetime_id, record_id,");

				if (connScheme != ConnectScheme.Ph3W3 && connScheme != ConnectScheme.Ph3W3_B_calc)
				{
					for(int iParam = 1; iParam <= 50; ++iParam)
					{
						sql.Append(string.Format(" pharm_p_a_{0}, pharm_q_a_{0}, pharm_angle_a_{0},", iParam));
					
						if (connScheme != ConnectScheme.Ph1W2)
						{
							sql.Append(string.Format(" pharm_p_b_{0}, pharm_q_b_{0}, pharm_angle_b_{0},", iParam));
							sql.Append(string.Format(" pharm_p_c_{0}, pharm_q_c_{0}, pharm_angle_c_{0},", iParam));
						}
					}
				}
				else
				{
					for(int iParam = 1; iParam <= 50; ++iParam)
					{
						sql.Append(string.Format(" pharm_p_1_{0}, pharm_q_1_{0}, pharm_angle_1_{0},", iParam));
						sql.Append(string.Format(" pharm_p_2_{0}, pharm_q_2_{0}, pharm_angle_2_{0},", iParam));
					}
				}
				if (connScheme != ConnectScheme.Ph1W2)
					for(int iParam = 1; iParam <= 50; ++iParam)
						sql.Append(string.Format(" pharm_p_sum_{0}, pharm_q_sum_{0}, pharm_angle_sum_{0},", iParam));

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(string.Format(") VALUES(currval('avg_times_datetime_id_seq'), {0},", recordId));

				if (connScheme != ConnectScheme.Ph3W3 && connScheme != ConnectScheme.Ph3W3_B_calc)
				{
					for(int iParam = 0; iParam < 50; ++iParam)
					{
						sql.Append(string.Format(" {0}, {1}, {2},",
							harm_power_A[(int)HARM_POWER.P, iParam].ToString(ci),
							harm_power_A[(int)HARM_POWER.Q, iParam].ToString(ci),
							harm_power_A[(int)HARM_POWER.ANGLE, iParam].ToString(ci)));
					
						if (connScheme != ConnectScheme.Ph1W2)
						{
							sql.Append(string.Format(" {0}, {1}, {2},",
								harm_power_B[(int)HARM_POWER.P, iParam].ToString(ci),
								harm_power_B[(int)HARM_POWER.Q, iParam].ToString(ci),
								harm_power_B[(int)HARM_POWER.ANGLE, iParam].ToString(ci)));
							sql.Append(string.Format(" {0}, {1}, {2},",
								harm_power_C[(int)HARM_POWER.P, iParam].ToString(ci),
								harm_power_C[(int)HARM_POWER.Q, iParam].ToString(ci),
								harm_power_C[(int)HARM_POWER.ANGLE, iParam].ToString(ci)));
						}
					}
				}
				else
				{
					for(int iParam = 0; iParam < 50; ++iParam)
					{
						sql.Append(string.Format(" {0}, {1}, {2},",
							harm_power_1[(int)HARM_POWER.P, iParam].ToString(ci),
							harm_power_1[(int)HARM_POWER.Q, iParam].ToString(ci),
							harm_power_1[(int)HARM_POWER.ANGLE, iParam].ToString(ci)));
						sql.Append(string.Format(" {0}, {1}, {2},",
							harm_power_2[(int)HARM_POWER.P, iParam].ToString(ci),
							harm_power_2[(int)HARM_POWER.Q, iParam].ToString(ci),
							harm_power_2[(int)HARM_POWER.ANGLE, iParam].ToString(ci)));
					}
				}
				if (connScheme != ConnectScheme.Ph1W2)
					for(int iParam = 0; iParam < 50; ++iParam)
					{
						sql.Append(string.Format(" {0}, {1}, {2},",
							harm_power_SUM[(int)HARM_POWER.P, iParam].ToString(ci),
							harm_power_SUM[(int)HARM_POWER.Q, iParam].ToString(ci),
							harm_power_SUM[(int)HARM_POWER.ANGLE, iParam].ToString(ci)));
					}

				// delete the last comma
				sql.Remove(sql.Length - 1, 1);

				sql.Append(");\n");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in xml2sql_avg_harmonic_power():");
				throw;
			}
		}

        #endregion

		#endregion
	}
}
