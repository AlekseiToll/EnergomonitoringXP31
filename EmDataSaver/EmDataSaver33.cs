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
	public class EmDataSaver33 : EmDataSaverBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		public delegate void InvalidDurationHandler(TimeSpan time);
		/// <summary>
		/// Событие OnInvalidDuration происходит если при считывании ПКЭ длительность  
		/// получилась больше суток
		/// </summary>
		public event InvalidDurationHandler OnInvalidDuration;

		#endregion

		#region Fields

		private EmSqlDeviceImage sqlImage_;
		private EmXmlDeviceImage xmlImage_;

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
			set { sqlImage_ = value; }
		}

		#endregion

		#region Constructor

		public EmDataSaver33(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			EmSqlDeviceImage sqlImage,
			EmXmlDeviceImage xmlImage)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.pgSrvConnectStr_ = settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEm33;
			this.pgHost_ = settings_.PgServers[settings_.CurServerIndex].PgHost;
			this.bw_ = bw;
			this.sqlImage_ = sqlImage;
			this.xmlImage_ = xmlImage;
		}

		#endregion

		#region Main methods

		/// <summary>
		/// Main saving function
		/// Start saving process
		/// </summary>
		public void Run(ref DoWorkEventArgs e)
		{
			try
			{
				e_ = e;

				// используем неоптимизированную вставку
				if (pgHost_ != "localhost" || settings_.OptimisedInsertion == false)
				{
					if (sqlImage_ == null)
						throw new EmException("EmDataSaver33::Run: sqlImage == null");
					e.Result = pgInsert();
				}
				else
				{
					if (xmlImage_ == null)
					{
						if(sqlImage_ != null)
							e.Result = pgInsert(); // пытаемся сделать хотя бы неоптимизированную вставку
						else
							throw new EmException("EmDataSaver33::Run: xmlImage == null");
					}
					else
						e.Result = pgInsert2();
				}
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in EmDataSaver33::Run()");
				Thread.ResetAbort();
				e.Result = false;
				return;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in EmDataSaver33::Run(): " + emx.Message);
				e.Result = false;
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver33::Run():");
				e.Result = false;
				throw;
			}
		}

		#endregion

		#region Private PostgreSQL Inserting methods

		private bool pgInsert()
		{
			DbService dbService = new DbService(pgSrvConnectStr_);
			try
			{
				dbService.Open();

				string commandText;
				string[] sqls = sqlImage_.Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				int i;
				for (i = 0; i < sqls.Length - 1; i++)
				{
					commandText = sqls[i];
					dbService.ExecuteNonQuery(commandText, true);
				}
				commandText = sqls[i];
				object o_folder_id = dbService.ExecuteScalar(commandText);
				long folder_id = (long)o_folder_id;

				for (int iArch = 0; iArch < sqlImage_.Archives.Length; iArch++)
				{
					// посылаем сообщение (Вставка в БД: Архив X из Y)
					if (OnSetCntArchives != null) OnSetCntArchives(sqlImage_.Archives.Length, iArch + 1);

					string _sql = string.Format(sqlImage_.Archives[iArch].Sql, folder_id);

					string[] _sqls = _sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
					int j;
					for (j = 0; j < _sqls.Length - 1; j++)
					{
						commandText = _sqls[j];
						dbService.ExecuteNonQuery(commandText, true);
					}
					commandText = _sqls[j];
					object o_archive_id = dbService.ExecuteScalar(commandText);
					long archive_id = (long)o_archive_id;

					for (int iArchPart = 0; iArchPart < sqlImage_.Archives[iArch].Data.Length; iArchPart++)
					{
						// обработка вставки данных Усредненных значений
						if (sqlImage_.Archives[iArch].Data[iArchPart].SqlType == EmSqlDataNodeType.AVG)
						{
							string[] __sqls = sqlImage_.Archives[iArch].Data[iArchPart].Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							commandText = string.Format(__sqls[0], archive_id);
							dbService.ExecuteNonQuery(commandText, true);

							int pos = 1;
							int sqls_per_block = 20;
							bool cont = true;
							string sql_block;
							while (cont)
							{
								sql_block = string.Empty;
								for (int u = pos; u < pos + sqls_per_block; u++)
								{
									if (u >= __sqls.Length)
									{
										u = __sqls.Length - 1;
										cont = false;
										break;
									}
									sql_block += __sqls[u] + "\n";
								}
								pos += sqls_per_block;

								commandText = sql_block;

								dbService.ExecuteNonQuery(commandText, true);

								if (bw_.CancellationPending)
								{
									e_.Cancel = true;
									return false;
								}
							}
						}
						// все остальные (События или ПКЭ)
						else
						{
							commandText = string.Format(sqlImage_.Archives[iArch].Data[iArchPart].Sql, archive_id);
							dbService.ExecuteNonQuery(commandText, true);

							// DEBUG =============================
							/*string[] tmp_sqls = commandText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
							int jj = 0;
							try
							{
								for (jj = 0; jj < tmp_sqls.Length - 1; jj++)
								{
									commandText = tmp_sqls[jj];
									dbService.ExecuteNonQuery(commandText);
								}
							}
							catch
							{
								int jjj = jj;
								string sss = tmp_sqls[jj];
							}*/
							// END OF DEBUG ========================
						}

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}
					}

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pgInsert(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		#endregion

		#region Methods for optimized inserting to Database

		// оптимизированная вставка в БД
		private bool pgInsert2()
		{
			string folder_name = string.Format(
				"{0} #{1} {2}", xmlImage_.Name, xmlImage_.SerialNumber, DateTime.Now.ToString());

			DbService dbService = new DbService(pgSrvConnectStr_);
			try
			{
				dbService.Open();

				string commandText = string.Empty;
				// Эти SQL запросы проверяют
				// существование записи о Приборе в таблице devices и в случае необходимости
				// добавляют её, затем вставляют в таблицу folders запись о корневой папке всего
				// сохранения и возвращают folder_id этой папки
				commandText = string.Format("SELECT check_dev_serial({0}, CAST({1} as smallint));", xmlImage_.SerialNumber, xmlImage_.InternalType);
				dbService.ExecuteNonQuery(commandText, true);
				commandText = string.Format("INSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{0}', false, 0, 0, null);", folder_name);
				dbService.ExecuteNonQuery(commandText, true);
				commandText = "SELECT currval('folders_folder_id_seq');";
				object o_folder_id = dbService.ExecuteScalar(commandText);
				long folder_id = (long)o_folder_id;

				for (int iArch = 0; iArch < xmlImage_.ArchiveList.Length; ++iArch)
				{
					EmXmlArchive xmlArhive = xmlImage_.ArchiveList[iArch];

					MakeTimerCoefficient(ref xmlArhive);

					// посылаем сообщение (Вставка в БД: Архив X из Y)
					if (OnSetCntArchives != null)
						OnSetCntArchives(xmlImage_.ArchiveList.Length, iArch + 1);

					// sql-запрос состоящий из добавления записи папки,
					// а потом записи архива в эту папку
					commandText = string.Format("INSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{0}', true, {1}, 0, null);", xmlArhive.ObjectName, folder_id);
					dbService.ExecuteNonQuery(commandText, true);

					commandText = string.Format("INSERT INTO databases (db_id, start_datetime, end_datetime, con_scheme, f_nom, u_nom_lin, u_nom_ph, device_id, parent_id, db_name, db_info, u_limit, i_limit, current_transducer_index, device_name, device_version, t_fliker) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, currval('folders_folder_id_seq'), null, null, {7}, {8}, {9}, '{10}', '{11}', {12});",
						// start_datetime
						xmlArhive.CommonBegin.ToString("MM.dd.yyyy HH:mm:ss"),
						// end_datetime
						xmlArhive.CommonEnd.ToString("MM.dd.yyyy HH:mm:ss"),
						(short)xmlArhive.ConnectionScheme,
						// f_nom
						xmlArhive.F_Nominal.ToString(new CultureInfo("en-US")),
						// u_nom_lin
						xmlArhive.U_NominalLinear.ToString(new CultureInfo("en-US")),
						// u_nom_ph
						xmlArhive.U_NominalPhase.ToString(new CultureInfo("en-US")),
						xmlImage_.SerialNumber,						// device_id
						xmlArhive.U_Limit.ToString(new CultureInfo("en-US")), // u_limit
						xmlArhive.I_Limit.ToString(new CultureInfo("en-US")), // i_limit
						xmlArhive.CurrentTransducerIndex,	// current_transduser_index
						xmlImage_.Name,			// device_name
						xmlImage_.Version,		// device_version
						xmlArhive.T_fliker);	// fliker period

					dbService.ExecuteNonQuery(commandText, true);

					commandText = "SELECT currval('databases_db_id_seq');";
					object o_archive_id = dbService.ExecuteScalar(commandText);
					long archive_id = (long)o_archive_id;

					bool res;

					if (xmlArhive.ArchivePQP != null)
					{
						if (xmlArhive.ArchivePQP.Length != 0)
						{
							for (int i = 0; i < xmlArhive.ArchivePQP.Length; i++)
							{
								res = insertPqpArchive(xmlArhive.ArchivePQP[i], ref dbService,
											archive_id, xmlArhive.ConnectionScheme, i,
											xmlArhive.F_Nominal,
											xmlArhive.U_NominalLinear,
											xmlArhive.U_NominalPhase,
											xmlImage_.Version,
											xmlImage_.DeviceType);
								if (!res) return false;

								if (bw_.CancellationPending)
								{
									e_.Cancel = true;
									return false;
								}
							}
						}
					}
					if (xmlArhive.ArchiveDNS != null)
					{
						res = insertDnsArchive(xmlArhive.ArchiveDNS, ref dbService, archive_id);
						if (!res) return false;
					}
					if (xmlArhive.ArchiveAVG != null)
					{
						res = insertAvgArchive(xmlArhive.ArchiveAVG, xmlArhive.ConnectionScheme,
										ref dbService, xmlArhive.F_Nominal,
										xmlArhive.U_NominalLinear,
										xmlArhive.U_NominalPhase,
										archive_id);
						if (!res) return false;
					}

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in pgInsert2(): " + ex.Message);
				return false;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		private bool insertDnsArchive(EmXmlDNS xmlDNS, ref DbService dbService, long archive_id)
		{
			string sqlImageFileName = EmService.GetSqlImageFilePathAndName(EmDeviceType.EM33T);
			string fileName = sqlImageFileName.Remove(
				sqlImageFileName.Length - EmSqlImageCreator33.ImageFileExtention.Length,
				EmSqlImageCreator33.ImageFileExtention.Length) + "dns.pg";
			try
			{
				FileStream f = new FileStream(fileName, FileMode.Create, FileAccess.Write);
				StreamWriter sw = new StreamWriter(f, Encoding.Default);
				try
				{
					string commandText = string.Format("INSERT INTO dips_and_overs_times (datetime_id, start_datetime, end_datetime, database_id) VALUES (DEFAULT, '{0}', '{1}', {2});\n",
						xmlDNS.Start.ToString("MM.dd.yyyy HH:mm:ss"),
						xmlDNS.End.ToString("MM.dd.yyyy HH:mm:ss"),
						archive_id);
					dbService.ExecuteNonQuery(commandText, true);

					// узнаем из базы текущий datetime_id
					string cur_datetime_id;
					commandText = "SELECT currval('dips_and_overs_times_datetime_id_seq');";
					cur_datetime_id = dbService.ExecuteScalarString(commandText);

					byte[] buffer = xmlDNS.DataPages;

					for (int i = 0; i < xmlDNS.DnsNum; i++)
					{
						int recAddress = 16 * i;

						// если записей оказалось меньше чем ожидалось, 
						// тем не менее делаем штатный выход
						if (recAddress + 15 > buffer.Length)
							return true;

						// выход в случае плохих данных
						// не делаю выход аварийным, так как он бывало часто срабатывал
						// а травмировать психику пользователя непонятными
						// сообщениями типа "Все сохранено, но с ошибками!" - 
						// задача неблагодарная.
						if (Conversions.bytes_2_ushort(ref buffer, recAddress) != 0)
						{
							return true;
						}

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
						float deviation =
							(float)((float)Conversions.bytes_2_ushort(ref buffer, recAddress + 8) / 8192);
						// здесь надо прибавлять миллисекунды а у нас десятки миллисекнуд, 
						// поэтому умножаем на 10
						DateTime start = xmlDNS.Start.AddMilliseconds(startTimer * coeffTimer * 10);
						DateTime end =
							start.AddMilliseconds(TrimDurationIn_ms(lenghtTimer * coeffTimer * 10));

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

						StringBuilder sb = new StringBuilder();
						sb.Append(cur_datetime_id);
						sb.Append(String.Format("|{0}|{1}|{2}|'{3}'|'{4}'",
							event_type,
							phase,
							deviation.ToString(new CultureInfo("en-US")),
							start.ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
							end.ToString("MM.dd.yyyy HH:mm:ss.FFFFF")
							));
						sw.WriteLine(sb.ToString());

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}
					}
					sw.WriteLine("\\.");
				}
				finally
				{
					if (sw != null) sw.Close();
					if (f != null) f.Close();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in insertDnsArchive(): " + ex.Message);
				return false;
			}
			finally
			{
				if (File.Exists(fileName))
				{
					// поскольку в случае плохих данных Андрей сделал неаварийный выход,??????????????????????????
					// добавляем в базу в любом случае 
					/////////////////////////////////////////////////////////
					// insert into Database
					fileName = fileName.Replace('\\', '/');
					string commandText = string.Format("copy dips_and_overs(datetime_id, event_type, phase, deviation, start_datetime, end_datetime) from '{0}' using delimiters '|';", fileName);
					dbService.ExecuteNonQuery(commandText, true);

					File.Delete(fileName);
				}
			}

			return true;
		}

		private bool insertPqpArchive(EmXmlPQP xmlPQP, ref DbService dbService,
					long archive_id, ConnectScheme connectionScheme, int numPqp,
					float fNom, float uLinNom, float uPhNom, string device_version,
					EmDeviceType devType)
		{
			try
			{
				if (dbService.ConnectionState != System.Data.ConnectionState.Open)
					return false;

				///////////////////////////////////////
				// парсим по необходимости страницы ПКЭ
				byte[] buffer = xmlPQP.DataPages;

				// извлекаем из массива тип уставок
				int constraint_type = xmlPQP.StandardSettingsType;

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
					ml_start_time_1 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHs1, iMMs1, 0);
					ml_end_time_1 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHe1, iMMe1, 0);
				}
				if (!(iHHs2 == iHHe2 && iMMs2 == iMMe2))
				{
					ml_start_time_2 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHs2, iMMs2, 0);
					ml_end_time_2 = new DateTime(xmlPQP.Start.Year, xmlPQP.Start.Month, xmlPQP.Start.Day, iHHe2, iMMe2, 0);
				}

				string commandText = string.Format("INSERT INTO day_avg_parameter_times (datetime_id, start_datetime, end_time, constraint_type, ml_start_time_1, ml_end_time_1, ml_start_time_2, ml_end_time_2, database_id) VALUES (DEFAULT, '{0}', '{1}', {2}, '{3}', '{4}', '{5}', '{6}', {7});\n",
					xmlPQP.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlPQP.End.ToString("MM.dd.yyyy HH:mm:ss"),
					constraint_type,
					ml_start_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_start_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					archive_id);
				dbService.ExecuteNonQuery(commandText, true);

				bool flikkerExists = false;
				if (connectionScheme != ConnectScheme.Ph3W3 && connectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					if (devType == EmDeviceType.EM33T1)
						flikkerExists = true;
					else if (devType == EmDeviceType.EM33T)
						if (Constants.isNewDeviceVersion_EM33T(device_version))
							flikkerExists = true;
				}

				// для значений ПКЭ у нас четыре таблицы, поэтому будем заполнять 
				// четыре файла
				// создаем имена файлов и пятый - для fliker
				short filesCount = 7;
				string[] fileNames = new string[filesCount];
				FileStream[] files = new FileStream[filesCount];
				StreamWriter[] streams = new StreamWriter[filesCount];
				string sqlImageFileName = EmService.GetSqlImageFilePathAndName(EmDeviceType.EM33T);
				try
				{
					try
					{
						for (int i_name = 0; i_name < filesCount; ++i_name)
						{
							fileNames[i_name] = sqlImageFileName.Remove(
								sqlImageFileName.Length - EmSqlImageCreator33.ImageFileExtention.Length,
								EmSqlImageCreator33.ImageFileExtention.Length) + "pqp" + 
								numPqp.ToString() + i_name.ToString() + ".pg";
							files[i_name] = new FileStream(fileNames[i_name], 
								FileMode.Create, FileAccess.Write);
							streams[i_name] = new StreamWriter(files[i_name], Encoding.Default);
						}

						// узнаем из базы текущий datetime_id
						commandText =
							"SELECT currval('day_avg_parameter_times_datetime_id_seq');";
						string curDatetimeId = dbService.ExecuteScalarString(commandText);

						// t1:  K2_U, K0_U 
						// 6.2.done
						if (connectionScheme != ConnectScheme.Ph1W2)
						{
							//K_2U
							formPqpFile_t1(ref buffer, streams[0], 448, 36, 1101, curDatetimeId);
						}
						if (connectionScheme == ConnectScheme.Ph3W4 || 
							connectionScheme == ConnectScheme.Ph3W4_B_calc)
						{
							//K_0U
							formPqpFile_t1(ref buffer, streams[0], 464, 40, 1102, curDatetimeId);
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

						if (flikkerExists)
							record_size = 48;		// fliker

						// перебираем все страницы с данными о частоте и напряжениях
						for (int unfp = 0; unfp < xmlPQP.UnfPagesLength; unfp++)
						{
							// смещение начала текущей страницы с данными о частоте и напряжениях
							int page_sh = _g_shift + unfp * 512;
							// 
							byte[] DataPages = xmlPQP.DataPages;
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
									((connectionScheme == ConnectScheme.Ph1W2) ? uPhNom : uLinNom);
								_Uy /= ((connectionScheme == ConnectScheme.Ph1W2) ? uPhNom : uLinNom) / 100;

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

								//int tp = (int)Conversions.bytes_2_uint(ref DataPages, page_sh + rec_sh + 40);

								int tp;
								if (flikkerExists)
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

						formPqpFile_t2(ref buffer, streams[1], ref f, 4, 1001, curDatetimeId); // ∆f
						formPqpFile_t2(ref buffer, streams[1], ref Uy, 20, 1002,
							curDatetimeId); //δU_y
						formPqpFile_t2(ref buffer, streams[1], ref Uy1, 28, 1006,
							curDatetimeId); //δU_y_'
						formPqpFile_t2(ref buffer, streams[1], ref  Uy2, 20, 1010,
							curDatetimeId); //δU_y_"

						if (connectionScheme != ConnectScheme.Ph3W3 && 
							connectionScheme != ConnectScheme.Ph3W3_B_calc)
						{
							formPqpFile_t2(ref buffer, streams[1], ref U_A, 20, 1003,
								curDatetimeId); // δU_A
							formPqpFile_t2(ref buffer, streams[1], ref U_A1, 28, 1007,
								curDatetimeId); // δU_A_'
							formPqpFile_t2(ref buffer, streams[1], ref  U_A2, 20, 1011,
								curDatetimeId); // δU_A_"
						}

						if (connectionScheme == ConnectScheme.Ph3W4 ||
							connectionScheme == ConnectScheme.Ph3W4_B_calc)
						{
							formPqpFile_t2(ref buffer, streams[1], ref U_B, 20, 1004,
								curDatetimeId); // δU_B
							formPqpFile_t2(ref buffer, streams[1], ref U_C, 20, 1005,
								curDatetimeId); // δU_C

							formPqpFile_t2(ref buffer, streams[1], ref U_B1, 28, 1008,
								curDatetimeId); // δU_B_'
							formPqpFile_t2(ref buffer, streams[1], ref U_C1, 28, 1009,
								curDatetimeId); // δU_C_'

							formPqpFile_t2(ref buffer, streams[1], ref  U_B2, 20, 1012,
								curDatetimeId); // δU_B_"
							formPqpFile_t2(ref buffer, streams[1], ref  U_C2, 20, 1013,
								curDatetimeId); // δU_C_"
						}

						if (connectionScheme != ConnectScheme.Ph1W2)
						{
							formPqpFile_t2(ref buffer, streams[1], ref  U_AB, 20, 1014,
								curDatetimeId); // δU_AB
							formPqpFile_t2(ref buffer, streams[1], ref  U_BC, 20, 1015,
								curDatetimeId); // δU_BC
							formPqpFile_t2(ref buffer, streams[1], ref U_CA, 20, 1016,
								curDatetimeId); // δU_CA

							formPqpFile_t2(ref buffer, streams[1], ref U_AB1, 28, 1017,
								curDatetimeId); // δU_AB_'
							formPqpFile_t2(ref buffer, streams[1], ref U_BC1, 28, 1018,
								curDatetimeId); // δU_BC_'
							formPqpFile_t2(ref buffer, streams[1], ref  U_CA1, 28, 1019,
								curDatetimeId); // δU_CA_'

							formPqpFile_t2(ref buffer, streams[1], ref  U_AB2, 20, 1020,
								curDatetimeId); // δU_AB_"
							formPqpFile_t2(ref buffer, streams[1], ref  U_BC2, 20, 1021,
								curDatetimeId); // δU_BC_"
							formPqpFile_t2(ref buffer, streams[1], ref  U_CA2, 20, 1022,
								curDatetimeId); // δU_CA_"
						}

						#endregion

						// t3:  events
						// 6.2.done
						formPqpFile_t3(ref buffer, streams[2], 3360, "A", 0, curDatetimeId); //swell
						formPqpFile_t3(ref buffer, streams[2], 3380, "A", 1, curDatetimeId); //dip
						if (connectionScheme != ConnectScheme.Ph1W2)
						{
							formPqpFile_t3(ref buffer, streams[2], 3400, "B", 0, 
								curDatetimeId);//swell / перенапряжение 
							formPqpFile_t3(ref buffer, streams[2], 3420, "B", 1, 
								curDatetimeId);	// dip / провал
							formPqpFile_t3(ref buffer, streams[2], 3440, "C", 0, 
								curDatetimeId);	// swell / перенапряжение 
							formPqpFile_t3(ref buffer, streams[2], 3460, "C", 1, 
								curDatetimeId);	// dip / провал
							formPqpFile_t3(ref buffer, streams[2], 3480, "*", 0, 
								curDatetimeId);	// swell / перенапряжение 
							formPqpFile_t3(ref buffer, streams[2], 3500, "*", 1, 
								curDatetimeId);	// dip / провал
						}

						// t4:	K_UA(2..40), K_UB(2..40), K_UC(2..40) а также K_UA, K_UB и K_UC
						// в таблице parameters для прошивки 6.2 добавлена новая группа 
						// параметров
						// с идентификаторами от 1301 до 1340 для линейных несинусоидальностей;
						// их имена совпадают полностью с именами параметров от 1201 до 1240 
						// (для фазных)			
						// 6.2.done
						int j = 0;
						for (j = 0; j < 39; j++)
						{
							formPqpFile_t4(ref buffer, streams[3], j * 12, 44 + j * 4, 1202 + j,
											connectionScheme, curDatetimeId);
						}
						formPqpFile_t4(ref buffer, streams[3], j * 12, 44 + j * 4, 1201,
											connectionScheme, curDatetimeId);

						// fliker
						// вычисляем время, с которого начинается фликер. количество минут 
						// кратно периоду фликера (1, 5 или 10)
						DateTime startTimeFlik = xmlPQP.Start;
						if (startTimeFlik.Millisecond > 0)
						{
							startTimeFlik = new DateTime(startTimeFlik.Year,
								startTimeFlik.Month,
								startTimeFlik.Day, startTimeFlik.Hour, startTimeFlik.Minute,
								startTimeFlik.Second, 0);
							startTimeFlik = startTimeFlik.AddSeconds(1);
						}
						if (startTimeFlik.Second > 0)
						{
							startTimeFlik = new DateTime(startTimeFlik.Year,
								startTimeFlik.Month,
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

						// fliker
						if (flikkerExists)
						{
							formPqpFile_t5(flik_A, flik_B, flik_C, streams[4],
								connectionScheme,
								startTimeFlik, endTimeFlik, xmlPQP.T_fliker, curDatetimeId);
						}

						formPqpFile_t6(ref xmlPQP, streams[5], uLinNom, uPhNom, device_version,
										connectionScheme, devType, curDatetimeId);
						formPqpFile_t7(ref xmlPQP, streams[6], fNom, device_version, devType,
										curDatetimeId);

						// вставляем признак конца файла
						for (int i_name = 0; i_name < filesCount; ++i_name)
						{
							streams[i_name].WriteLine("\\.");
						}
					}
					finally
					{
						for (int i_name = 0; i_name < filesCount; ++i_name)
						{
							if (streams[i_name] != null) streams[i_name].Close();
							if (files[i_name] != null) files[i_name].Close();
						}
					}

					////////////////////////////
					// insert into Database
					for (int i_name = 0; i_name < filesCount; ++i_name)
					{
						fileNames[i_name] =
							fileNames[i_name].Replace('\\', '/');
					}
					// t1
					if (connectionScheme != ConnectScheme.Ph1W2)
					{
						commandText = string.Format("copy day_avg_parameters_t1(datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng) from '{0}' using delimiters '|';", fileNames[0]);
						dbService.ExecuteNonQuery(commandText, true);
					}
					// t2
					commandText = string.Format("copy day_avg_parameters_t2(datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_bottom, real_nrm_rng_top, real_max_rng_bottom, real_max_rng_top, calc_nrm_rng_bottom, calc_nrm_rng_top, calc_max_rng_bottom, calc_max_rng_top) from '{0}' using delimiters '|';", fileNames[1]);
					dbService.ExecuteNonQuery(commandText, true);
					// t3
					commandText = string.Format("copy day_avg_parameters_t3(datetime_id, event_type, phase, common_number, common_duration, max_period_period, max_value_period, max_period_value, max_value_value) from '{0}' using delimiters '|';", fileNames[2]);
					dbService.ExecuteNonQuery(commandText, true);
					// t4
					commandText = string.Format("copy day_avg_parameters_t4(datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) from '{0}' using delimiters '|';", fileNames[3]);
					dbService.ExecuteNonQuery(commandText, true);
					// t5
					// fliker
					if (flikkerExists)
					{
						if (connectionScheme == ConnectScheme.Ph1W2)
						{
							commandText = string.Format("copy day_avg_parameters_t5(datetime_id, flik_time, flik_a, flik_a_long) from '{0}' using delimiters '|';", fileNames[4]);
						}
						else
						{
							commandText = string.Format("copy day_avg_parameters_t5(datetime_id, flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long) from '{0}' using delimiters '|';", fileNames[4]);
						}
						dbService.ExecuteNonQuery(commandText, true);
					}

					// t6
					switch (connectionScheme)
					{
						case ConnectScheme.Ph3W4:
						case ConnectScheme.Ph3W4_B_calc:
							commandText = string.Format("copy day_avg_parameters_t6(datetime_id, event_datetime, d_u_y, d_u_a, d_u_b, d_u_c, d_u_ab, d_u_bc, d_u_ca) from '{0}' using delimiters '|';", fileNames[5]);
							dbService.ExecuteNonQuery(commandText, true);
							break;
						case ConnectScheme.Ph3W3:
						case ConnectScheme.Ph3W3_B_calc:
							commandText = string.Format("copy day_avg_parameters_t6(datetime_id, event_datetime, d_u_y, d_u_ab, d_u_bc, d_u_ca) from '{0}' using delimiters '|';", fileNames[5]);
							dbService.ExecuteNonQuery(commandText, true);
							break;

						case ConnectScheme.Ph1W2:
							commandText = string.Format("copy day_avg_parameters_t6(datetime_id, event_datetime, d_u_a) from '{0}' using delimiters '|';", fileNames[5]);
							dbService.ExecuteNonQuery(commandText, true);
							break;
					}

					// t7
					commandText = string.Format("copy day_avg_parameters_t7(datetime_id, event_datetime, d_f) from '{0}' using delimiters '|';", fileNames[6]);
					dbService.ExecuteNonQuery(commandText, true);
				}
				finally
				{
					// удаляем файлы
					for (int i_name = 0; i_name < filesCount; ++i_name)
					{
						if (File.Exists(fileNames[i_name]))
							File.Delete(fileNames[i_name]);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in insertPqpArchive(): ");
				return false;
			}
		}

		private bool formPqpFile_t1(ref byte[] buffer, StreamWriter sw, int Shift,
			int ShiftNominal, int param_id, string curDatetimeId)
		{
			try
			{
				// после изменения карты памяти у Валеры - область копии уставок немного 
				// съехала - на 24 байта:
				ShiftNominal += 20;

				CultureInfo ci_enUS = new CultureInfo("en-US");

				// отсчеты прибора:
				// отсчетов в НДП
				ushort num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, Shift + 2);
				// отсчетов в ПДП
				ushort num_max_rng = Conversions.bytes_2_ushort(ref buffer, Shift + 4);
				// отсчетов за ПДП
				ushort num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, Shift + 6);

				// если данные по отсчетам по какой-то причине отсутствуют
				// делаем вид, что все прошло успешно
				if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return true;

				//float tmp1 = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal) * 100;
				//float tmp2 = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 2) * 100;
				//float tmp3 = Conversions.bytes_2_float2w65536(ref buffer, Shift + 8) * 100;
				//float tmp4 = Conversions.bytes_2_float2w65536(ref buffer, Shift + 12) * 100;

				StringBuilder sb = new StringBuilder();
				sb.Append(curDatetimeId);
				sb.Append(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
					param_id,
					num_nrm_rng,
					num_max_rng,
					num_out_max_rng,
					(Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal) * 100).ToString(ci_enUS),	// НДП
					(Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 2) * 100).ToString(ci_enUS),//ПДП
					(Conversions.bytes_2_float2w65536(ref buffer, Shift + 8) * 100).ToString(ci_enUS),	// 95%
					(Conversions.bytes_2_float2w65536(ref buffer, Shift + 12) * 100).ToString(ci_enUS)));
				sw.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formPqpFile_t1(): ");
				return false;
			}
			return true;
		}

		private bool formPqpFile_t2(ref byte[] buffer, StreamWriter sw, ref List<float> list,
			int ShiftNominal, int param_id, string curDatetimeId)
		{
			try
			{
				// если нет ничего в списке - попросту выходим
				if (list.Count == 0) return true;

				CultureInfo ci_enUS = new CultureInfo("en-US");

				// здесь почему-то не 24 а 20 - непонятно - но практикой выявлено что надо 20 :(
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
					// при загрузке через файл вместо NULL используется именно такой знак! - \N
					str_f_95_d = "\\N";
					str_f_95_u = "\\N";
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

				// если данные по отсчетам по какой-то причине отсутствуют, делаем вид, 
				// что все прошло более чем успешно :)
				if (num_nrm_rng + num_max_rng + num_out_max_rng == 0) return true;

				StringBuilder sb = new StringBuilder();
				sb.Append(curDatetimeId);
				sb.Append(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}",
					param_id,
					num_nrm_rng,
					num_max_rng,
					num_out_max_rng,
					fNDP_d.ToString(ci_enUS),	// НДП н
					fNDP_u.ToString(ci_enUS),	// НДП в
					fPDP_d.ToString(ci_enUS),	// ПДП н
					fPDP_u.ToString(ci_enUS),	// ПДП в
					str_f_95_d,					// 95% н
					str_f_95_u,					// 95% в
					f_min.ToString(ci_enUS),	// min н
					f_max.ToString(ci_enUS)		// max в
					));
				sw.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formPqpFile_t2(): ");
				return false;
			}
			return true;
		}

		private bool formPqpFile_t3(ref byte[] buffer, StreamWriter sw, int Shift,
			string phase, short event_type, string curDatetimeId)
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
				// debug
				long temp = TrimDuration(duration);
				TimeSpan common_duration = new TimeSpan(TrimDuration(duration));
				//TimeSpan common_duration = new TimeSpan((long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer));

				// аналогично меняем местами слова в остальных временах
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 8);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 10);

				long lMaxPeriodPeriod = (long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer);
				TimeSpan max_period_period = new TimeSpan(TrimDuration(lMaxPeriodPeriod));

				//TimeSpan max_period_period = new TimeSpan((long)Conversions.bytes_2_uint(ref Buffer, Shift + 8) * time_const);
				hi_word = Conversions.bytes_2_ushort(ref buffer, Shift + 4);
				lo_word = Conversions.bytes_2_ushort(ref buffer, Shift + 6);
				long lMaxValuePeriod = (long)((lo_word + hi_word * 0x10000) * time_const * coeffTimer);
				TimeSpan max_value_period = new TimeSpan(TrimDuration(lMaxValuePeriod));

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
					common_duration = new TimeSpan(0, 23, 59, 59, 99);
				}
				if (max_period_period.Days > 0)
				{
					max_period_period = new TimeSpan(0, 23, 59, 59, 999);
				}
				if (max_value_period.Days > 0)
				{
					max_value_period = new TimeSpan(0, 23, 59, 59, 999);
				}

				StringBuilder sb = new StringBuilder();
				sb.Append(curDatetimeId);
				sb.Append(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
					event_type,
					phase,
					common_number,
					common_duration.ToString(),
					max_period_period.ToString(),
					max_value_period.ToString(),
					max_period_value.ToString(ci_enUS),
					max_value_value.ToString(ci_enUS)
					));
				sw.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formPqpFile_t3(): ");
				return false;
			}
			return true;
		}

		private bool formPqpFile_t4(ref byte[] buffer, StreamWriter sw, int Shift,
			int ShiftNominal, int param_id, ConnectScheme connectionScheme, string curDatetimeId)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");

				// в прошивке 6.2 уставки сдвинулись на 20 байта
				ShiftNominal += 20;

				// ФАЗНЫЕ ГАРМОНИКИ:
				if (connectionScheme != ConnectScheme.Ph3W3 && 
					connectionScheme != ConnectScheme.Ph3W3_B_calc)
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

					if (connectionScheme == ConnectScheme.Ph3W4 ||
						connectionScheme == ConnectScheme.Ph3W4_B_calc) // дорасчитываем еще 2
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
							StringBuilder sb = new StringBuilder();
							sb.Append(curDatetimeId);
							sb.Append(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}",
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
								fPDP.ToString(ci_enUS)
								));
							sw.WriteLine(sb.ToString());
						}
					}
					else // только одна фаза => запрос несколько упростится
					{
						if (num_nrm_rng_a != 0 || num_max_rng_a != 0 || num_out_max_rng_a != 0)
						{
							StringBuilder sb = new StringBuilder();
							sb.Append(curDatetimeId);
							sb.Append(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|0|0|0|0|0|0|0|0|0|0|{6}|{7}",
								param_id,
								// фаза 1
								num_nrm_rng_a.ToString(),		// отсчетов в НДП
								num_max_rng_a.ToString(),		// отсчетов в ПДП
								num_out_max_rng_a.ToString(),	// отсчетов за ПДП
								f_95_a.ToString(ci_enUS),		// 95%
								f_max_a.ToString(ci_enUS),		// max
								// ГОСТы
								fNDP.ToString(ci_enUS),			// НДП
								fPDP.ToString(ci_enUS)
								));
							sw.WriteLine(sb.ToString());
						}
					}
				}

				// ЛИНЕЙНЫЕ ГАРМОНИКИ:
				if (connectionScheme != ConnectScheme.Ph1W2)
				{
					param_id += 100;

					// ГОСТы из уставок
					float fNDP = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 160) * 100;
					float fPDP = Conversions.bytes_2_float1w65536(ref buffer, ShiftNominal + 162) * 100;

					// отсчеты прибора
					ushort num_nrm_rng_ab = 
						Conversions.bytes_2_ushort(ref buffer, 960 + Shift + 2);	// отсчетов в НДП ф.AB
					ushort num_max_rng_ab = 
						Conversions.bytes_2_ushort(ref buffer, 960 + Shift + 4);	// отсчетов в ПДП ф.AB
					ushort num_out_max_rng_ab = 
						Conversions.bytes_2_ushort(ref buffer, 960 + Shift + 6);// отсчетов за ПДП ф.AB

					ushort num_nrm_rng_bc = 
						Conversions.bytes_2_ushort(ref buffer, Shift + 1920 + 2);	// отсчетов в НДП ф.BC
					ushort num_max_rng_bc = 
						Conversions.bytes_2_ushort(ref buffer, Shift + 1920 + 4);	// отсчетов в ПДП ф.BC
					ushort num_out_max_rng_bc = 
						Conversions.bytes_2_ushort(ref buffer, Shift + 1920 + 6);// отсчетов за ПДП ф.BC

					ushort num_nrm_rng_ca = 
						Conversions.bytes_2_ushort(ref buffer, Shift + 2880 + 2);	// отсчетов в НДП ф.CA
					ushort num_max_rng_ca = 
						Conversions.bytes_2_ushort(ref buffer, Shift + 2880 + 4);	// отсчетов в ПДП ф.CA
					ushort num_out_max_rng_ca = 
						Conversions.bytes_2_ushort(ref buffer, Shift + 2880 + 6);// отсчетов за ПДП ф.CA

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
						StringBuilder sb = new StringBuilder();
						sb.Append(curDatetimeId);
						sb.Append(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}",
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
							fPDP.ToString(ci_enUS)
							));
						sw.WriteLine(sb.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formPqpFile_t4(): ");
				return false;
			}
			return true;
		}

		private bool formPqpFile_t5(List<float> flik_A, List<float> flik_B,
					List<float> flik_C, StreamWriter sw,
					ConnectScheme connection_scheme,
					DateTime startTime, DateTime endTime, short t_flik,
					string curDatetimeId)
		{
			try
			{
				if (connection_scheme == ConnectScheme.Ph3W3 || 
					connection_scheme == ConnectScheme.Ph3W3_B_calc)
					return true;

				DateTime curTime = startTime;
				DateTime fullTime = startTime;
				fullTime = fullTime.AddHours(2);  // время, начиная с которого фликер становится 
				// полноценным, т.е. измеренным не меньше чем за 2 часа
				short start = 0, end = 0;
				CultureInfo ci_enUS = new CultureInfo("en-US");

				for (int i = 0; i < flik_A.Count; ++i)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(curDatetimeId);

					if (connection_scheme == ConnectScheme.Ph1W2)
					{
						sb.Append(String.Format("|'{0}'|{1}|{2}",
							curTime.ToString("HH:mm:ss"),
							flik_A[i].ToString(ci_enUS),
							getLongFliker(flik_A, start, end).ToString(ci_enUS)));
					}
					else
					{
						sb.Append(String.Format("|'{0}'|{1}|{2}|{3}|{4}|{5}|{6}",
							curTime.ToString("HH:mm:ss"),
							flik_A[i].ToString(ci_enUS),
							getLongFliker(flik_A, start, end).ToString(ci_enUS),
							flik_B[i].ToString(ci_enUS),
							getLongFliker(flik_B, start, end).ToString(ci_enUS),
							flik_C[i].ToString(ci_enUS),
							getLongFliker(flik_C, start, end).ToString(ci_enUS)));
					}
					sw.WriteLine(sb.ToString());

					curTime = curTime.AddMinutes(t_flik);
					if (curTime > endTime) break;
					++end;
					if (end >= flik_A.Count) break;
					if (curTime > fullTime)
						++start;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formPqpFile_t5(): ");
				return false;
			}
			return true;
		}

		private bool formPqpFile_t6(ref EmXmlPQP xmlPQP, StreamWriter sw,
			float uLinNom, float uPhNom, string device_version, ConnectScheme connectScheme,
			EmDeviceType devType, string curDatetimeId)
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

				StringBuilder sb = new StringBuilder();
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

								sb.Append(curDatetimeId);
								sb.AppendLine(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
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

								sb.Append(curDatetimeId);
								sb.AppendLine(String.Format("|{0}|{1}|{2}|{3}|{4}",
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

								sb.Append(curDatetimeId);
								sb.AppendLine(String.Format("|{0}|{1}",
									event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
									U_A.ToString(ci)
									));
								break;
						}
						event_datetime = event_datetime.AddSeconds(timeInterval);

						rec_sh += record_size; // переходим к следующей записи
					}
				}
				sw.Write(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formPqpFile_t6(): ");
				return false;
			}
			return true;
		}

		private bool formPqpFile_t7(ref EmXmlPQP xmlPQP, StreamWriter sw,
			float fNom, string device_version, EmDeviceType devType, string curDatetimeId)
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

				StringBuilder sb = new StringBuilder();
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

						sb.Append(curDatetimeId);
						sb.AppendLine(String.Format("|{0}|{1}",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f1.ToString(ci)
							));
						event_datetime = event_datetime.AddSeconds(timeInterval);
						sb.Append(curDatetimeId);
						sb.AppendLine(String.Format("|{0}|{1}",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f2.ToString(ci)
							));
						event_datetime = event_datetime.AddSeconds(timeInterval);
						sb.Append(curDatetimeId);
						sb.AppendLine(String.Format("|{0}|{1}",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),		// datetime of event
							f3.ToString(ci)
							));
						event_datetime = event_datetime.AddSeconds(timeInterval);

						rec_sh += record_size; // переходим к следующей записи
					}
				}
				sw.Write(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formPqpFile_t7(): ");
				return false;
			}
			return true;
		}

		private bool insertAvgArchive(EmXmlAVG xmlAVG, ConnectScheme connectionScheme,
									ref DbService dbService, float fnom, float unomln, float unomph, long archive_id)
		{
			try
			{
				string commandText = string.Format("INSERT INTO period_avg_params_times (datetime_id, start_datetime, end_datetime, database_id, period_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3});\n",
					xmlAVG.Start.ToString("MM.dd.yyyy HH:mm:ss"),
					xmlAVG.End.ToString("MM.dd.yyyy HH:mm:ss"),
					archive_id,
					(int)xmlAVG.AvgType - 1);
				EmService.WriteToLogGeneral("AVG type = " + xmlAVG.AvgType);
				dbService.ExecuteNonQuery(commandText, true);

				// узнаем из базы текущий datetime_id
				string cur_datetime_id;
				commandText = "SELECT currval('period_avg_params_times_datetime_id_seq');";
				cur_datetime_id = (dbService.ExecuteScalar(commandText)).ToString();

				// имена файлов для четырех таблиц
				string[] fileNamesAvg = new string[4];
				FileStream[] files = new FileStream[4];
				StreamWriter[] streams = new StreamWriter[4];
				string sqlImageFileName = EmService.GetSqlImageFilePathAndName(EmDeviceType.EM33T);
				try
				{
					for (int i_name = 0; i_name < 4; ++i_name)
					{
						fileNamesAvg[i_name] = sqlImageFileName.Remove(
							sqlImageFileName.Length - EmSqlImageCreator33.ImageFileExtention.Length,
							EmSqlImageCreator33.ImageFileExtention.Length) + "avg" + i_name.ToString() + ".pg";
						files[i_name] = new FileStream(fileNamesAvg[i_name], FileMode.Create, FileAccess.Write);
						streams[i_name] = new StreamWriter(files[i_name], Encoding.Default);
					}
					try
					{
						/////////////////////////////////////
						// парсим страницы Усредненных данных
						float fpages = (float)xmlAVG.DataPages.Length / Em33TDevice.bytes_per_page;
						int pages = (int)fpages;
						if (fpages != (float)pages) return false;

						string sql = string.Empty;
						for (int i = 0; i < pages; i++)
						{
							byte[] buffer = new byte[Em33TDevice.bytes_per_page];
							Array.Copy(xmlAVG.DataPages, i * Em33TDevice.bytes_per_page,
								buffer, 0, Em33TDevice.bytes_per_page);
							int Page = (int)(buffer[0x07]);

							switch (Page)
							{
								case 0: formAvgFile_p1(ref buffer, connectionScheme, cur_datetime_id,
											fnom, unomln, unomph, streams[0]);
									break;
								case 1: formAvgFile_p2(ref buffer, connectionScheme, cur_datetime_id,
											streams[1]);
									break;
								case 2: formAvgFile_p3(ref buffer, connectionScheme, cur_datetime_id,
											streams[2]);
									break;
								case 3: formAvgFile_p4(ref buffer, connectionScheme, cur_datetime_id,
											streams[3]);
									break;
								case 4:
									break;
							}

							if (bw_.CancellationPending)
							{
								e_.Cancel = true;
								return false;
							}
						}

						// вставляем признак конца файла
						for (int i_name = 0; i_name < 4; ++i_name)
						{
							streams[i_name].WriteLine("\\.");
						}
					}
					catch (Exception ex)
					{
						EmService.WriteToLogFailed("Error in insertAvgArchive() 1 " + ex.Message);
						return false;
					}
					finally
					{
						try
						{
							for (int i_name = 0; i_name < 4; ++i_name)
							{
								if (streams[i_name] != null) streams[i_name].Close();
								if (files[i_name] != null) files[i_name].Close();
							}
						}
						catch { }
					}

					// insert into Database
					for (int i_name = 0; i_name < 4; ++i_name)
					{
						fileNamesAvg[i_name] =
							fileNamesAvg[i_name].Replace('\\', '/');
					}
					// p1
					switch (connectionScheme)
					{
						case ConnectScheme.Ph3W4:
						case ConnectScheme.Ph3W4_B_calc:
							commandText = string.Format("copy period_avg_params_1_4 from '{0}' using delimiters '|';", fileNamesAvg[0]);
							break;
						case ConnectScheme.Ph3W3:
						case ConnectScheme.Ph3W3_B_calc:
							commandText = string.Format("copy period_avg_params_1_4(datetime_id, event_datetime, f, u1_a, u1_b, u1_c, u_ab, u_bc, u_ca, u1_ab, u1_bc, u1_ca, u_1, u_2, i_a, i_b, i_c, i1_a, i1_b, i1_c, i_1, i_2, p_sum, p_a_1, p_b_2, s_sum, q_sum_geom, q_sum_shift, q_sum_cross, q_a_cross, q_b_cross, q_c_cross, kp_sum, an_u1_a_u1_b, an_u1_b_u1_c, an_u1_c_u1_a, an_u1_a_i1_a, an_u1_b_i1_b, an_u1_c_i1_c, an_u_1_i_1, an_u_2_i_2, p_1, p_2, d_f, d_u_y, d_u_ab, d_u_bc, d_u_ca, k_2u, k_ua_ab, k_ub_bc, k_uc_ca, k_ia, k_ib, k_ic, tangent_p_geom, tangent_p_shift, tangent_p_cross) from '{0}' using delimiters '|';", fileNamesAvg[0]);
							break;
						case ConnectScheme.Ph1W2:
							commandText = string.Format("copy period_avg_params_1_4(datetime_id, event_datetime, f, u_a, u1_a, u0_a, u_hp_a, u_1, i_a, i1_a, i_1, p_a_1, s_a, q_a_geom, q_a_shift, kp_a, an_u1_a_u1_b, an_u1_a_i1_a, an_u_1_i_1, p_1, d_f, d_u_a, k_ua_ab, k_ia, tangent_p_geom, tangent_p_shift, tangent_p_cross) from '{0}' using delimiters '|';", fileNamesAvg[0]);
							break;
					}
					dbService.ExecuteNonQuery(commandText, true);

					// p2
					if (connectionScheme != ConnectScheme.Ph1W2)
					{
						commandText = string.Format("copy period_avg_params_5 from '{0}' using delimiters '|';", fileNamesAvg[1]);
					}
					else
					{
						commandText = string.Format("copy period_avg_params_5 (datetime_id, event_datetime, u1_a_ab, k_ua_ab_2, k_ua_ab_3, k_ua_ab_4, k_ua_ab_5, k_ua_ab_6, k_ua_ab_7, k_ua_ab_8, k_ua_ab_9, k_ua_ab_10, k_ua_ab_11, k_ua_ab_12, k_ua_ab_13, k_ua_ab_14, k_ua_ab_15, k_ua_ab_16, k_ua_ab_17, k_ua_ab_18, k_ua_ab_19, k_ua_ab_20, k_ua_ab_21, k_ua_ab_22, k_ua_ab_23, k_ua_ab_24, k_ua_ab_25, k_ua_ab_26, k_ua_ab_27, k_ua_ab_28, k_ua_ab_29, k_ua_ab_30, k_ua_ab_31, k_ua_ab_32, k_ua_ab_33, k_ua_ab_34, k_ua_ab_35, k_ua_ab_36, k_ua_ab_37, k_ua_ab_38, k_ua_ab_39, k_ua_ab_40, i1_a, k_ia_2, k_ia_3, k_ia_4, k_ia_5, k_ia_6, k_ia_7, k_ia_8, k_ia_9, k_ia_10, k_ia_11, k_ia_12, k_ia_13, k_ia_14, k_ia_15, k_ia_16, k_ia_17, k_ia_18, k_ia_19, k_ia_20, k_ia_21, k_ia_22, k_ia_23, k_ia_24, k_ia_25, k_ia_26, k_ia_27, k_ia_28, k_ia_29, k_ia_30, k_ia_31, k_ia_32, k_ia_33, k_ia_34, k_ia_35, k_ia_36, k_ia_37, k_ia_38, k_ia_39, k_ia_40) from '{0}' using delimiters '|';", fileNamesAvg[1]);
					}
					dbService.ExecuteNonQuery(commandText, true);

					// p3
					if (connectionScheme != ConnectScheme.Ph1W2)
					{
						commandText = string.Format("copy period_avg_params_6b from '{0}' using delimiters '|';", fileNamesAvg[2]);
					}
					else
					{
						commandText = string.Format("copy period_avg_params_6b (datetime_id, event_datetime, an_u_a_1_i_a_1, an_u_a_2_i_a_2, an_u_a_3_i_a_3, an_u_a_4_i_a_4, an_u_a_5_i_a_5, an_u_a_6_i_a_6, an_u_a_7_i_a_7, an_u_a_8_i_a_8, an_u_a_9_i_a_9, an_u_a_10_i_a_10, an_u_a_11_i_a_11, an_u_a_12_i_a_12, an_u_a_13_i_a_13, an_u_a_14_i_a_14, an_u_a_15_i_a_15, an_u_a_16_i_a_16, an_u_a_17_i_a_17, an_u_a_18_i_a_18, an_u_a_19_i_a_19, an_u_a_20_i_a_20, an_u_a_21_i_a_21, an_u_a_22_i_a_22, an_u_a_23_i_a_23, an_u_a_24_i_a_24, an_u_a_25_i_a_25, an_u_a_26_i_a_26, an_u_a_27_i_a_27, an_u_a_28_i_a_28, an_u_a_29_i_a_29, an_u_a_30_i_a_30, an_u_a_31_i_a_31, an_u_a_32_i_a_32, an_u_a_33_i_a_33, an_u_a_34_i_a_34, an_u_a_35_i_a_35, an_u_a_36_i_a_36, an_u_a_37_i_a_37, an_u_a_38_i_a_38, an_u_a_39_i_a_39, an_u_a_40_i_a_40) from '{0}' using delimiters '|';", fileNamesAvg[2]);
					}
					dbService.ExecuteNonQuery(commandText, true);

					// p4
					if (connectionScheme != ConnectScheme.Ph1W2)
					{
						commandText = string.Format("copy period_avg_params_6a from '{0}' using delimiters '|';", fileNamesAvg[3]);
					}
					else
					{
						commandText = string.Format("copy period_avg_params_6a (datetime_id, event_datetime, p_sum, p_a_1, p_a_1_1, p_a_1_2, p_a_1_3, p_a_1_4, p_a_1_5, p_a_1_6, p_a_1_7, p_a_1_8, p_a_1_9, p_a_1_10, p_a_1_11, p_a_1_12, p_a_1_13, p_a_1_14, p_a_1_15, p_a_1_16, p_a_1_17, p_a_1_18, p_a_1_19, p_a_1_20, p_a_1_21, p_a_1_22, p_a_1_23, p_a_1_24, p_a_1_25, p_a_1_26, p_a_1_27, p_a_1_28, p_a_1_29, p_a_1_30, p_a_1_31, p_a_1_32, p_a_1_33, p_a_1_34, p_a_1_35, p_a_1_36, p_a_1_37, p_a_1_38, p_a_1_39, p_a_1_40) from '{0}' using delimiters '|';", fileNamesAvg[3]);
					}
					dbService.ExecuteNonQuery(commandText, true);
				}
				catch (Exception ex)
				{
					EmService.WriteToLogFailed("Error in insertAvgArchive() 2 " + ex.Message);
					return false;
				}
				finally
				{
					try
					{
						// удаляем файлы
						for (int i_name = 0; i_name < 4; ++i_name)
						{
							if (File.Exists(fileNamesAvg[i_name]))
								File.Delete(fileNamesAvg[i_name]);
						}
					}
					catch { }
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in insertAvgArchive(): ");
				return false;
			}
			return true;
		}

		private bool formAvgFile_p1(ref byte[] buffer, ConnectScheme connectionScheme,
			string cur_datetime_id,
			float fnom, float unomln, float unomph, StreamWriter sw)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				StringBuilder sb;

				#region Data to insert

				switch (connectionScheme)
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

						sb = new StringBuilder();
						sb.Append(cur_datetime_id);
						sb.Append(String.Format("|'{0}'|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}|{32}|{33}|{34}|{35}|{36}|{37}|{38}|{39}|{40}|{41}|{42}|{43}|{44}|{45}|{46}|{47}|{48}|{49}|{50}|{51}|{52}|{53}|{54}|{55}|{56}|{57}|{58}|{59}|{60}|{61}|{62}|{63}|{64}|{65}|{66}|{67}|{68}|{69}|{70}|{71}|{72}|{73}|{74}|{75}|{76}|{77}|{78}|{79}|{80}|{81}|{82}|{83}|{84}|{85}|{86}|0",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
							Conversions.bytes_2_float2w65536(ref buffer, 8).ToString(ci),// f

							Conversions.bytes_2_float2w65536(ref buffer, 156).ToString(ci),	// U_A
							Conversions.bytes_2_float2w65536(ref buffer, 224).ToString(ci),	// U_B
							Conversions.bytes_2_float2w65536(ref buffer, 292).ToString(ci),	// U_C

							Conversions.bytes_2_float2w65536(ref buffer, 160).ToString(ci),// U1_A
							Conversions.bytes_2_float2w65536(ref buffer, 228).ToString(ci),//U1_B
							Conversions.bytes_2_float2w65536(ref buffer, 296).ToString(ci),	//U1_C

							Conversions.bytes_2_float2w65536(ref buffer, 164).ToString(ci),	//U_AB
							Conversions.bytes_2_float2w65536(ref buffer, 232).ToString(ci),	//U_BC
							Conversions.bytes_2_float2w65536(ref buffer, 300).ToString(ci),	//U_CA

							Conversions.bytes_2_float2w65536(ref buffer, 168).ToString(ci),//U1_AB
							Conversions.bytes_2_float2w65536(ref buffer, 236).ToString(ci),//U1_BC
							Conversions.bytes_2_float2w65536(ref buffer, 304).ToString(ci),//U1_CA

							Conversions.bytes_2_signed_float2w65536(ref buffer, 176).ToString(ci),// U0_A*
							Conversions.bytes_2_signed_float2w65536(ref buffer, 244).ToString(ci),// U0_B
							Conversions.bytes_2_signed_float2w65536(ref buffer, 312).ToString(ci),// U0_C

							Conversions.bytes_2_float2w65536(ref buffer, 172).ToString(ci),//U_hp A
							Conversions.bytes_2_float2w65536(ref buffer, 240).ToString(ci),//U_hp B
							Conversions.bytes_2_float2w65536(ref buffer, 308).ToString(ci),//U_hp C

							Conversions.bytes_2_float2w65536(ref buffer, 40).ToString(ci),			// U_1 = U_у
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

							Conversions.bytes_2_signed_float4096(ref buffer, 364).ToString(ci),		// Kp_Σ
							Conversions.bytes_2_signed_float4096(ref buffer, 154).ToString(ci),		// Kp_A
							Conversions.bytes_2_signed_float4096(ref buffer, 222).ToString(ci),		// Kp_B
							Conversions.bytes_2_signed_float4096(ref buffer, 290).ToString(ci),		// Kp_C

							Conversions.bytes_2_signed_float128(ref buffer, 122).ToString(ci),	// ∟U1_A_ U1_B
							Conversions.bytes_2_signed_float128(ref buffer, 190).ToString(ci),	// ∟U1_B_ U1_C
							Conversions.bytes_2_signed_float128(ref buffer, 258).ToString(ci),	// ∟U1_C_ U1_A

							Conversions.bytes_2_signed_float128(ref buffer, 120).ToString(ci),	// ∟U1_A_ I1_A
							Conversions.bytes_2_signed_float128(ref buffer, 188).ToString(ci),	// ∟U1_B_ I1_B	
							Conversions.bytes_2_signed_float128(ref buffer, 256).ToString(ci),	// ∟U1_C_ I1_C

							Conversions.bytes_2_signed_float128(ref buffer, 366).ToString(ci),	// ∟U_y_ I_1
							Conversions.bytes_2_signed_float128(ref buffer, 368).ToString(ci),	// ∟U_2_ I_2
							Conversions.bytes_2_signed_float128(ref buffer, 370).ToString(ci),	// ∟U_0_ I_0	

							Conversions.bytes_2_signed_float3w65536(ref buffer, 358).ToString(ci),	// P_0
							Conversions.bytes_2_signed_float3w65536(ref buffer, 346).ToString(ci),	// P_1
							Conversions.bytes_2_signed_float3w65536(ref buffer, 352).ToString(ci),	// P_2

							// ∆f = f - f_ном
							(Conversions.bytes_2_float2w65536(ref buffer, 8) - fnom).ToString(ci),

							// δU_y = (U_y - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 40) - unomln) * 
								100 / unomln).ToString(ci),
							// δU_A = (U1_A - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 160) - unomph) * 
								100 / unomph).ToString(ci),
							// δU_B = (U1_B - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 228) - unomph) * 
								100 / unomph).ToString(ci),
							// δU_C = (U1_C - U_ном.фаз.) * 100% / U_ном.фаз.
							((Conversions.bytes_2_float2w65536(ref buffer, 296) - unomph) * 
								100 / unomph).ToString(ci),
							// δU_AB = (U1_AB - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 168) - unomln) * 
								100 / unomln).ToString(ci),
							// δU_BC = (U1_BC - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 236) - unomln) * 
								100 / unomln).ToString(ci),
							// δU_CA = (U1_CA - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 304) - unomln) * 
								100 / unomln).ToString(ci),

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
						sw.WriteLine(sb.ToString());

						break;

					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:

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

						sb = new StringBuilder();
						sb.Append(cur_datetime_id);
						sb.Append(String.Format("|'{0}'|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}|{32}|{33}|{34}|{35}|{36}|{37}|{38}|{39}|{40}|{41}|{42}|{43}|{44}|{45}|{46}|{47}|{48}|{49}|{50}|{51}|{52}|{53}|{54}|{55}|{56}",
							event_datetime.ToString("MM.dd.yyyy HH:mm:ss"),	// datetime of event
							Conversions.bytes_2_float2w65536(ref buffer, 8).ToString(ci),// f

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

							pSum33.ToString(ci),	// P_Σ
							Conversions.bytes_2_signed_float3w65536(ref buffer, 124).ToString(ci),	// P_A(1)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 260).ToString(ci),	// P_C(2)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 322).ToString(ci),	// S_Σ

							qSumGeom33.ToString(ci),	// Q_Σ geom
							qSumShift33.ToString(ci),	// Q_Σ shift
							qSumCross33.ToString(ci),	// Q_Σ cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 136).ToString(ci),	// Q_A cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 204).ToString(ci),	// Q_B cross
							Conversions.bytes_2_signed_float3w65536(ref buffer, 272).ToString(ci),	// Q_C cross

							Conversions.bytes_2_signed_float4096(ref buffer, 364).ToString(ci),		// Kp_Σ

							Conversions.bytes_2_signed_float128(ref buffer, 122).ToString(ci),	// ∟U1_A_ U1_B
							Conversions.bytes_2_signed_float128(ref buffer, 190).ToString(ci),	// ∟U1_B_ U1_C
							Conversions.bytes_2_signed_float128(ref buffer, 258).ToString(ci),	// ∟U1_C_ U1_A

							Conversions.bytes_2_signed_float128(ref buffer, 120).ToString(ci),	// ∟U1_A_ I1_A
							Conversions.bytes_2_signed_float128(ref buffer, 188).ToString(ci),	// ∟U1_B_ I1_B	
							Conversions.bytes_2_signed_float128(ref buffer, 256).ToString(ci),	// ∟U1_C_ I1_C

							Conversions.bytes_2_signed_float128(ref buffer, 366).ToString(ci),	// ∟U_y_ I_1
							Conversions.bytes_2_signed_float128(ref buffer, 368).ToString(ci),	// ∟U_2_ I_2

							Conversions.bytes_2_signed_float3w65536(ref buffer, 346).ToString(ci),	// P_1
							Conversions.bytes_2_signed_float3w65536(ref buffer, 352).ToString(ci),	// P_2

							// ∆f = f - f_ном
							(Conversions.bytes_2_float2w65536(ref buffer, 8) - fnom).ToString(ci),	
							// δU_y = (U_y - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 40) - unomln) * 
								100 / unomln).ToString(ci),
							// δU_AB = (U1_AB - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 168) - unomln) * 
								100 / unomln).ToString(ci),
							// δU_BC = (U1_BC - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 236) - unomln) * 
								100 / unomln).ToString(ci),
							// δU_CA = (U1_CA - U_ном.лин.) * 100% / U_ном.лин.
							((Conversions.bytes_2_float2w65536(ref buffer, 304) - unomln) * 
								100 / unomln).ToString(ci),

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
						sw.WriteLine(sb.ToString());

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

						sb = new StringBuilder();
						sb.Append(cur_datetime_id);
						sb.Append(String.Format("|'{0}'|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}",
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

							pA.ToString(ci),	// P_A(1)
							Conversions.bytes_2_signed_float3w65536(ref buffer, 130).ToString(ci),	// S_A

							qAGeom.ToString(ci),	// Q_A geom
							qAShift.ToString(ci),	// Q_A shift

							Conversions.bytes_2_signed_float4096(ref buffer, 154).ToString(ci),		// Kp_A

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
							Conversions.bytes_2_float1w65536_percent(ref buffer, 76).ToString(ci),	// K_IA
							tanPgeom12.ToString(ci),	// tangent P geom
							tanPshift12.ToString(ci),	// tangent P shift
							0	// dummy for tangent P cross
							));
						sw.WriteLine(sb.ToString());
						break;
				}

				#endregion

			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formAvgFile_p1(): ");
				return false;
			}
			return true;
		}

		private bool formAvgFile_p2(ref byte[] buffer, ConnectScheme connectionScheme,
			string cur_datetime_id, StreamWriter sw)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				StringBuilder sb = new StringBuilder();
				sb.Append(cur_datetime_id).Append("|'");
				sb.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("'|");
				// U1_A(AB)
				sb.Append((Conversions.bytes_2_float2w65536(ref buffer, 492)).ToString(ci) + "|");

				for (int i = 0; i < 39; i++)	// K_UA(AB)_2..K_UA(AB)_40
					sb.Append((Conversions.bytes_2_float1w65536_percent(ref buffer, 88 + i * 2)).ToString(ci) + "|");

				if (connectionScheme != ConnectScheme.Ph1W2)
				{
					// U1_B(BC)
					sb.Append((Conversions.bytes_2_float2w65536(ref buffer, 500)).ToString(ci) + "|");
					for (int i = 0; i < 39; i++)	// K_UB(BC)_2..K_UB(BC)_40
						sb.Append((Conversions.bytes_2_float1w65536_percent(ref buffer, 248 + i * 2)).ToString(ci) + "|");
					// U1_C(CA)
					sb.Append((Conversions.bytes_2_float2w65536(ref buffer, 508)).ToString(ci) + "|");
					for (int i = 0; i < 39; i++)	// K_UC(CA)_2..K_UC(CA)_40
						sb.Append((Conversions.bytes_2_float1w65536_percent(ref buffer, 408 + i * 2)).ToString(ci) + "|");
				}

				// I1_A
				sb.Append((Conversions.bytes_2_float2w65536(ref buffer, 488)).ToString(ci) + "|");
				for (int i = 0; i < 39; i++)	// K_IA_2..K_IA_40
					sb.Append((Conversions.bytes_2_float1w65536_percent(ref buffer, 8 + i * 2)).ToString(ci) + "|");

				if (connectionScheme != ConnectScheme.Ph1W2)
				{
					// I1_B
					sb.Append((Conversions.bytes_2_float2w65536(ref buffer, 496)).ToString(ci) + "|");
					for (int i = 0; i < 39; i++)	// K_IB_2..K_IB_40
						sb.Append((Conversions.bytes_2_float1w65536_percent(ref buffer, 168 + i * 2)).ToString(ci) + "|");
					// I1_C
					sb.Append((Conversions.bytes_2_float2w65536(ref buffer, 504)).ToString(ci) + "|");
					for (int i = 0; i < 39; i++)	// K_IC_2..K_IC_40
						sb.Append((Conversions.bytes_2_float1w65536_percent(ref buffer, 328 + i * 2)).ToString(ci) + "|");
				}
				// удаляем последнюю вертикальную черту
				sb.Remove(sb.Length - 1, 1);
				sw.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formAvgFile_p2(): ");
				return false;
			}
			return true;
		}

		private bool formAvgFile_p3(ref byte[] buffer, ConnectScheme connectionScheme,
			string cur_datetime_id, StreamWriter sw)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				StringBuilder sb = new StringBuilder();
				sb.Append(cur_datetime_id).Append("|'");
				sb.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("'|");

				for (int i = 0; i < 40; i++)	// ∟U_A 1_ I_A 1..∟U_A 1_ I_A 40
					sb.Append((Conversions.bytes_2_signed_float128(ref buffer, 32 + i * 2)).ToString(ci) + "|");

				if (connectionScheme != ConnectScheme.Ph1W2)
				{
					for (int i = 0; i < 40; i++)	// ∟U_B 1_ I_B 1..∟UBA 1_ I_B 40
						sb.Append((Conversions.bytes_2_signed_float128(
							ref buffer, 112 + i * 2)).ToString(ci) + "|");
					for (int i = 0; i < 40; i++)	// ∟U_C 1_ I_C 1..∟U_C 1_ I_C 40
						sb.Append((Conversions.bytes_2_signed_float128(
							ref buffer, 192 + i * 2)).ToString(ci) + "|");
				}

				// удаляем последнюю вертикальную черту
				sb.Remove(sb.Length - 1, 1);
				sw.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formAvgFile_p3(): ");
				return false;
			}
			return true;
		}

		private bool formAvgFile_p4(ref byte[] buffer, ConnectScheme connectionScheme,
			string cur_datetime_id, StreamWriter sw)
		{
			try
			{
				CultureInfo ci = new CultureInfo("en-US");
				DateTime event_datetime = Conversions.bytes_2_DateTime(ref buffer, 0);

				StringBuilder sb = new StringBuilder();
				sb.Append(cur_datetime_id).Append("|'");
				sb.Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("'|");

				// P_Σ 
				sb.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 488).ToString(ci)) + "|");
				// P_A(1)
				sb.Append((Conversions.bytes_2_signed_float3w65536(ref buffer, 494).ToString(ci)) + "|");

				// P_B(2)
				if (connectionScheme == ConnectScheme.Ph3W4 || connectionScheme == ConnectScheme.Ph3W4_B_calc)
				{
					sb.Append(
						(Conversions.bytes_2_signed_float3w65536(ref buffer, 500).ToString(ci)) + "|");// P_B	
				}
				if (connectionScheme == ConnectScheme.Ph3W3 || connectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					sb.Append(
						(Conversions.bytes_2_signed_float3w65536(ref buffer, 506).ToString(ci)) + "|");// P_2
				}

				// P_C
				if (connectionScheme == ConnectScheme.Ph3W4 || connectionScheme == ConnectScheme.Ph3W4_B_calc)
				{
					sb.Append(
						(Conversions.bytes_2_signed_float3w65536(ref buffer, 506).ToString(ci)) + "|");// P_C
				}
				// а для 3ф3пр забиваем NULL
				if (connectionScheme == ConnectScheme.Ph3W3 || connectionScheme == ConnectScheme.Ph3W3_B_calc) 
				{
					sb.Append("\\N|");
				}

				for (int i = 0; i < 40; i++)	// P_A(1) 1..P_A(1) 40
					sb.Append(
						(Conversions.bytes_2_signed_float2w1024(ref buffer, 8 + i * 4)).ToString(ci) + "|");

				if (connectionScheme != ConnectScheme.Ph1W2)
				{
					for (int i = 0; i < 40; i++)	// P_B(2) 1..P_B(2) 40
						sb.Append((Conversions.bytes_2_signed_float2w1024(
										ref buffer, 168 + i * 4)).ToString(ci) + "|");
					for (int i = 0; i < 40; i++)	// P_C 1..P_C 40
						sb.Append((Conversions.bytes_2_signed_float2w1024(
										ref buffer, 328 + i * 4)).ToString(ci) + "|");
				}

				// удаляем последнюю вертикальную черту
				sb.Remove(sb.Length - 1, 1);
				sw.WriteLine(sb.ToString());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in formAvgFile_p4(): ");
				return false;
			}
			return true;
		}

		private EmDeviceType GetDeviceType(ushort internalType)
		{
			switch (internalType)
			{
				case 1: return EmDeviceType.EM31K;
				case 2: return EmDeviceType.EM32;
				case 3: return EmDeviceType.EM33T;
				case 4: return EmDeviceType.ETPQP;
				default: return EmDeviceType.EM33T;
			}
		}

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
	}
}
