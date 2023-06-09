using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;
using System.ComponentModel;
using System.Threading;

using EmDataSaver.SqlImage;
using EmDataSaver.SavingInterface;
using DbServiceLib;
using EmServiceLib;
using DeviceIO;

namespace EmDataSaver
{
	public class EtDataExportPQP : EmDataExportBase
	{
		#region Fields

		EtPQPSqlDeviceImage sqlImage_ = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor with a parameters to export data to the IMAGE
		/// </summary>
		/// <param name="sender">Sender object (must not be null)</param>
		/// <param name="fname">File name to extract image from</param>
		/// <param name="pgSrvConnectStr">PostgreSQL connection string</param>
		public EtDataExportPQP(
			object sender,
			EmSqlDataNodeType[] parts,
			long device_id,
			long object_id,
			string pgSrvConnectStr,
			string fileName)
		{
			this.sender_ = sender;
			this.parts_ = parts;
			this.pgSrvConnectStr_ = pgSrvConnectStr;
			this.device_id_to_export_ = device_id;
			this.object_reg_id_to_export_ = object_id;
			this.sqlImageFileName_ = fileName;
		}

		#endregion

		#region Properties

		public EtPQPSqlDeviceImage SqlImage
		{
			get { return sqlImage_; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Read all needed data from the database
		/// form SQL Deivece Image file
		/// </summary>
		public void ExportToImage(ref DoWorkEventArgs e)
		{
			try
			{
				frmExport wndExport = new frmExport(EmDeviceType.ETPQP, sqlImageFileName_);
				wndExport.InputArchiveParts = parts_;

				if (wndExport.ShowDialog(sender_ as Form) != DialogResult.OK)
				{
					e.Cancel = true;
					return;
				}
				this.sqlImageFileName_ = wndExport.FileName;
				parts_ = wndExport.OutputArchiveParts;

				sqlImage_ = new EtPQPSqlDeviceImage();

				if (!pg2sql())
				{
					e.Result = false;
					return;
				}

				e.Result = true;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in ExportToImage(): " + emx.Message);
				e.Result = false;
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportToImage():");
				e.Result = false;
				throw;
			}
		}

		#endregion

		#region Private Methods

		private bool pg2sql()
		{
			DbService dbService = new DbService(pgSrvConnectStr_);
			string commandText = string.Empty;
			try
			{
				dbService.Open();

				commandText = string.Format("SELECT * FROM devices WHERE dev_id = {0};",
												device_id_to_export_);
				dbService.ExecuteReader(commandText);
				if (!dbService.DataReaderRead()) return false;

				sqlImage_.SerialNumber = (Int64)dbService.DataReaderData("ser_number");
				sqlImage_.DeviceId = device_id_to_export_;
				object oDevInfo = dbService.DataReaderData("dev_info");
				if (oDevInfo is System.DBNull) sqlImage_.DeviceInfo = string.Empty;
				else sqlImage_.DeviceInfo = (string)oDevInfo;
				sqlImage_.Version = dbService.DataReaderData("dev_version") as string;

				sqlImage_.Sql = string.Format("select insert_new_device({0}, {1}, '{2}');\n",
					sqlImage_.SerialNumber,
					(sqlImage_.DeviceInfo == string.Empty) ? "null" : "'" + sqlImage_.DeviceInfo + "'",
					sqlImage_.Version
				);

				string folder_name = string.Format("ETPQP # {0}", sqlImage_.SerialNumber);

				// SQL запрос, который вставит в таблицу folders 
				// запись об устройстве (если ее еще нет) и вернет folder_id этой папки
				sqlImage_.Sql += String.Format("select insert_dev_folder({0}, '{1}');",
												sqlImage_.SerialNumber, folder_name);

				commandText =
					string.Format("SELECT * FROM objects WHERE device_id = {0} AND obj_id = {1};",
                    device_id_to_export_, object_reg_id_to_export_);
				dbService.ExecuteReader(commandText);
				if (!dbService.DataReaderRead()) return false;

				EmSqlObject sqlObject = new EmSqlObject();

				sqlObject.GlobalObjectId = (Int64)dbService.DataReaderData("global_obj_id");
				sqlObject.CommonBegin = (DateTime)dbService.DataReaderData("start_datetime"); // start_datetime
				sqlObject.CommonEnd = (DateTime)dbService.DataReaderData("end_datetime");	// end_datetime
				sqlObject.ConnectionScheme = (ConnectScheme)(short)dbService.DataReaderData("con_scheme");
				sqlObject.F_Nominal = (float)dbService.DataReaderData("f_nom");
				sqlObject.U_NominalLinear = (float)dbService.DataReaderData("u_nom_lin");
				sqlObject.U_NominalPhase = (float)dbService.DataReaderData("u_nom_ph");
				sqlObject.I_NominalPhase = (float)dbService.DataReaderData("i_nom_ph");
				sqlObject.U_Limit = (float)dbService.DataReaderData("u_limit");
				sqlObject.I_Limit = (float)dbService.DataReaderData("i_limit");
				sqlObject.CurrentTransducerIndex = (ushort)(short)dbService.DataReaderData("current_transducer_index");
				sqlObject.DeviceVersion = dbService.DataReaderData("device_version") as string;
				sqlObject.T_fliker = (ushort)(short)dbService.DataReaderData("t_fliker");
				sqlObject.ObjectName = dbService.DataReaderData("obj_name") as string;
				object oObjInfo = dbService.DataReaderData("obj_info");
				if (oObjInfo is System.DBNull) sqlObject.ObjectInfo = string.Empty;
				else sqlObject.ObjectInfo = (string)oObjInfo;
				sqlObject.ConstraintType = (short)dbService.DataReaderData("constraint_type");
				DateTime ml_start_time_1;
				DateTime ml_end_time_1;
				DateTime ml_start_time_2;
				DateTime ml_end_time_2;
				object o_start_time_1 = dbService.DataReaderData("ml_start_time_1");
				object o_end_time_1 = dbService.DataReaderData("ml_end_time_1");
				object o_start_time_2 = dbService.DataReaderData("ml_start_time_2");
				object o_end_time_2 = dbService.DataReaderData("ml_end_time_2");
				if (o_start_time_1 is DBNull) ml_start_time_1 = DateTime.MinValue;
				else ml_start_time_1 = (DateTime)dbService.DataReaderData("ml_start_time_1");
				if (o_end_time_1 is DBNull) ml_end_time_1 = DateTime.MinValue;
				else ml_end_time_1 = (DateTime)dbService.DataReaderData("ml_end_time_1");
				if (o_start_time_2 is DBNull) ml_start_time_2 = DateTime.MinValue;
				else ml_start_time_2 = (DateTime)dbService.DataReaderData("ml_start_time_2");
				if (o_end_time_2 is DBNull) ml_end_time_2 = DateTime.MinValue;
				else ml_end_time_2 = (DateTime)dbService.DataReaderData("ml_end_time_2");

				// SQL запрос, который вставит в таблицу objects 
				// запись об объекте (если ее еще нет) и вернет obj_id этого объекта
				sqlObject.Sql = string.Format("select insert_new_object('{0}', '{1}', cast({2} as int2), {3}, {4}, {5}, cast({6} as int8), '{7}', {8}, {9}, {10}, cast({11} as int2), '{12}', cast({13} as int2), {14}, '{15}', '{16}', '{17}', '{18}', cast({19} as int2), {20});\n",
					sqlObject.CommonBegin.ToString("MM.dd.yyyy HH:mm:ss"),
					sqlObject.CommonEnd.ToString("MM.dd.yyyy HH:mm:ss"),
					(short)sqlObject.ConnectionScheme,  // 2
					sqlObject.F_Nominal.ToString(new CultureInfo("en-US")),
					sqlObject.U_NominalLinear.ToString(new CultureInfo("en-US")),
					sqlObject.U_NominalPhase.ToString(new CultureInfo("en-US")),	// u_nom_ph
					"{0}",  // 6  dev_id
					sqlObject.ObjectName,
					(sqlObject.ObjectInfo == string.Empty) ? "null" : "'" + sqlObject.ObjectInfo + "'",
					sqlObject.U_Limit.ToString(new CultureInfo("en-US")),	// u_limit
					sqlObject.I_Limit.ToString(new CultureInfo("en-US")),	// i_limit  10
					sqlObject.CurrentTransducerIndex,	// current_transduser_index
					sqlObject.DeviceVersion,
					sqlObject.T_fliker,
					sqlObject.GlobalObjectId,  // 14
					ml_start_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_1.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_start_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					ml_end_time_2.ToString("MM.dd.yyyy HH:mm:ss"),
					sqlObject.ConstraintType,  // 19
					sqlObject.I_NominalPhase.ToString(new CultureInfo("en-US"))	// i_nom_ph
					);

				folder_name = string.Format("ETPQP # {0} # {1}",
												sqlImage_.SerialNumber,
												sqlObject.ObjectName);

				// SQL запрос, который вставит в таблицу folders 
				// запись об объекте (если ее еще нет) и вернет folder_id этой папки
				sqlObject.Sql += String.Format("select insert_obj_folder({0}, {1}, '{2}');",
					"{0}",   // device_id
					"{1}",   // object_id
					folder_name);

				List<EmSqlDataNode> partNodes_listPQP = new List<EmSqlDataNode>();
				List<EmSqlDataNode> partNodes_listAVG = new List<EmSqlDataNode>();
				List<EmSqlDataNode> partNodes_listDNS = new List<EmSqlDataNode>();

				for (int i = 0; i < this.parts_.Length; i++)
				{
					switch (this.parts_[i])
					{
						case EmSqlDataNodeType.PQP:
							SqlImage.EmSqlDataNode[] partNodesPQP = null;
							if (!pg2sql_pqp_period(ref dbService, ref partNodesPQP,
                                    object_reg_id_to_export_)) return false;
							for (int p = 0; p < partNodesPQP.Length; p++)
								partNodes_listPQP.Add(partNodesPQP[p]);
							break;
						case EmSqlDataNodeType.AVG:
							SqlImage.EmSqlDataNode[] partNodesAVG = null;
							if (!pg2sql_avg_period(ref dbService, ref partNodesAVG,
                                object_reg_id_to_export_)) return false;
							for (int p = 0; p < partNodesAVG.Length; p++)
								partNodes_listAVG.Add(partNodesAVG[p]);
							break;
						case EmSqlDataNodeType.Events:
							SqlImage.EmSqlDataNode[] partNodesEvents = null;
							if (!pg2sql_dns_period(ref dbService, ref partNodesEvents,
                                object_reg_id_to_export_)) return false;
							for (int p = 0; p < partNodesEvents.Length; p++)
								partNodes_listDNS.Add(partNodesEvents[p]);
							break;
					}
				}

				sqlObject.DataPQP = new EmSqlDataNode[partNodes_listPQP.Count];
				partNodes_listPQP.CopyTo(sqlObject.DataPQP);
				sqlObject.DataAVG = new EmSqlDataNode[partNodes_listAVG.Count];
				partNodes_listAVG.CopyTo(sqlObject.DataAVG);
				sqlObject.DataDNS = new EmSqlDataNode[partNodes_listDNS.Count];
				partNodes_listDNS.CopyTo(sqlObject.DataDNS);

				sqlImage_.Objects = new EmSqlObject[1];
				sqlImage_.Objects[0] = sqlObject;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in pg2sql(): " + ex.Message);
				return false;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		#region Building SQL query for inserting PQP data from Database

		private bool pg2sql_pqp_period(ref DbService dbService, ref EmSqlDataNode[] partNodes, Int64 objectId)
		{
			string commandText = string.Empty;
			DbService dbService2 = new DbService(string.Empty);
			try
			{
				commandText = string.Format(
					"SELECT count(*) FROM day_avg_parameter_times WHERE object_id = {0};",
					objectId);
				int num = (int)((long)dbService.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM day_avg_parameter_times WHERE object_id = {0};",
					objectId);
				dbService.ExecuteReader(commandText);

				int index = 0;
				dbService2.CopyConnection(ref dbService);
				while (dbService.DataReaderRead())
				{
					partNodes[index] = new EmSqlDataNode();
					Int64 datetime_id = (Int64)dbService.DataReaderData("datetime_id");
					short constraint_type = (short)dbService.DataReaderData("constraint_type");
					short con_scheme = (short)dbService.DataReaderData("con_scheme");
					int f_nom = (int)dbService.DataReaderData("f_nom");
					float u_nom_lin = (float)dbService.DataReaderData("u_nom_lin");
					float u_nom_ph = (float)dbService.DataReaderData("u_nom_ph");
					object ml_start_time_1 = dbService.DataReaderData("ml_start_time_1");
					object ml_end_time_1 = dbService.DataReaderData("ml_end_time_1");
					object ml_start_time_2 = dbService.DataReaderData("ml_start_time_2");
					object ml_end_time_2 = dbService.DataReaderData("ml_end_time_2");
					if (ml_start_time_1 is DBNull) ml_start_time_1 = DateTime.MinValue;
					if (ml_end_time_1 is DBNull) ml_end_time_1 = DateTime.MinValue;
					if (ml_start_time_2 is DBNull) ml_start_time_2 = DateTime.MinValue;
					if (ml_end_time_2 is DBNull) ml_end_time_2 = DateTime.MinValue;

					partNodes[index].SqlType = EmSqlDataNodeType.PQP;
					partNodes[index].Begin = (DateTime)dbService.DataReaderData("start_datetime");
					partNodes[index].End = (DateTime)dbService.DataReaderData("end_time");

					// SQL запрос, который вставит в таблицу folders 
					// папку для года и вернет folder_id этой папки
					partNodes[index].Sql = string.Format("select insert_year_folder({0}, {1}, '{2}');\n",
											"{0}", "{1}", partNodes[index].Begin.Year.ToString());

					// SQL запрос, который вставит в таблицу folders 
					// папку для месяца и вернет folder_id этой папки
					partNodes[index].Sql += string.Format("select insert_month_folder({0}, {1}, '{2}', '{3}');\n",
											"{0}", "{1}", partNodes[index].Begin.Year.ToString(),
											partNodes[index].Begin.Month.ToString());

					// на выходе имеем SQL запрос добавления записи 
					// периода ПКЭ а также всех дочерних данных по ПКЭ,
					// имеющий 4 недостающих параметра - object_id, device_id, folder_year_id, folder_month_id
					partNodes[index].Sql += string.Format("INSERT INTO day_avg_parameter_times (datetime_id, start_datetime, end_time, object_id, device_id, ml_start_time_1, ml_end_time_1, ml_start_time_2, ml_end_time_2, con_scheme, constraint_type, f_nom, u_nom_lin, u_nom_ph, folder_year_id, folder_month_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, '{4}', '{5}', '{6}', '{7}', {8}, {9}, {10}, {11}, {12}, {13}, {14});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}",
						"{1}",  // 3
						((DateTime)ml_start_time_1).ToString("MM.dd.yyyy HH:mm:ss"),
						((DateTime)ml_end_time_1).ToString("MM.dd.yyyy HH:mm:ss"),
						((DateTime)ml_start_time_2).ToString("MM.dd.yyyy HH:mm:ss"),
						((DateTime)ml_end_time_2).ToString("MM.dd.yyyy HH:mm:ss"),
						con_scheme,  // 8
						constraint_type,
						f_nom,
						u_nom_lin.ToString(new CultureInfo("en-US")),
						u_nom_ph.ToString(new CultureInfo("en-US")),
						"{2}", "{3}");

					string sql = string.Empty;
					// t1:  K2_U, K0_U
					if (!pg2sql_pqp_t1(datetime_id, ref dbService2, ref sql)) return false;

					// t2:  ∆f, δU_y, δU_A(AB), δU_B(BC), δU_C(CA), 
					//      δU_y_', δU_A(AB)_', δU_B(BC)_', δU_C(CA)_',
					//      δU_y_", δU_A(AB)_", δU_B(BC)_", δU_C(CA)_"
					if (!pg2sql_pqp_t2(datetime_id, ref dbService2, ref sql)) return false;

					// t3:  events
					if (!pg2sql_pqp_t3(datetime_id, ref dbService2, ref sql)) return false;

					// t4:	K_UA(2..40), K_UB(2..40), K_UC(2..40) а также K_UA, K_UB и K_UC
					if (!pg2sql_pqp_t4(datetime_id, ref dbService2, ref sql)) return false;

					// t5:	fliker
					if (!pg2sql_pqp_t5(datetime_id, ref dbService2, ref sql))
						return false;

					if (!pg2sql_pqp_t6(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_t7(datetime_id, ref dbService2, ref sql)) return false;

					partNodes[index].Sql += sql;

					++index;
				}
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_period():");
				return false;
			}
		}

		private bool pg2sql_pqp_t1(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t1 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					//sql +=
					sbMain.Append(
						string.Format("INSERT INTO day_avg_parameters_t1 (datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});\n",
						dbService.DataReaderData(1),
						dbService.DataReaderData(2),
						dbService.DataReaderData(3),
						dbService.DataReaderData(4),
						((float)dbService.DataReaderData(5)).ToString(ci_enUS),
						((float)dbService.DataReaderData(6)).ToString(ci_enUS),
						((float)dbService.DataReaderData(7)).ToString(ci_enUS),
						((float)dbService.DataReaderData(8)).ToString(ci_enUS))
						);

				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t1():");
				return false;
			}
		}

		private bool pg2sql_pqp_t2(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t2 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					string str_up = string.Empty, str_down = string.Empty;
					str_up = (dbService.DataReaderData(9, false) is DBNull) ? "NULL" : 
						((float)dbService.DataReaderData(9)).ToString(ci_enUS);
					str_down = (dbService.DataReaderData(11, false) is DBNull) ? "NULL" : 
						((float)dbService.DataReaderData(11)).ToString(ci_enUS);

					sbMain.Append(
						string.Format("INSERT INTO day_avg_parameters_t2 (datetime_id, param_id, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_top, real_max_rng_top, real_nrm_rng_bottom, real_max_rng_bottom, calc_nrm_rng_top, calc_max_rng_top, calc_nrm_rng_bottom, calc_max_rng_bottom) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});\n",
					dbService.DataReaderData(1),
					dbService.DataReaderData(2),
					dbService.DataReaderData(3),
					dbService.DataReaderData(4),
					((float)dbService.DataReaderData(5)).ToString(ci_enUS),
					((float)dbService.DataReaderData(6)).ToString(ci_enUS),
					((float)dbService.DataReaderData(7)).ToString(ci_enUS),
					((float)dbService.DataReaderData(8)).ToString(ci_enUS),
					str_up,
					((float)dbService.DataReaderData(10)).ToString(ci_enUS),
					str_down,
					((float)dbService.DataReaderData(12)).ToString(ci_enUS))
					);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t2():");
				return false;
			}
		}

		private bool pg2sql_pqp_t3(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t3 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					sbMain.Append(string.Format("INSERT INTO day_avg_parameters_t3 (datetime_id, event_type, phase, common_number, common_duration, max_period_period, max_value_period, max_period_value, max_value_value) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', '{5}', {6}, {7});\n",
						dbService.DataReaderData(1),
						dbService.DataReaderData(2),
						dbService.DataReaderData(3),
						dbService.DataReaderData(4),
						dbService.DataReaderData(5),
						dbService.DataReaderData(7),
						((float)dbService.DataReaderData(6)).ToString(ci_enUS),
						((float)dbService.DataReaderData(8)).ToString(ci_enUS))
						);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t3():");
				return false;
			}
		}

		private bool pg2sql_pqp_t4(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t4 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					sbMain.Append(String.Format("INSERT INTO day_avg_parameters_t4 (datetime_id, param_id, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17});\n",
					dbService.DataReaderData(1),
					dbService.DataReaderData(2),
					dbService.DataReaderData(3),
					dbService.DataReaderData(4),
					((float)dbService.DataReaderData(5)).ToString(ci_enUS),
					((float)dbService.DataReaderData(6)).ToString(ci_enUS),
					dbService.DataReaderData(7),
					dbService.DataReaderData(8),
					dbService.DataReaderData(9),
					((float)dbService.DataReaderData(10)).ToString(ci_enUS),
					((float)dbService.DataReaderData(11)).ToString(ci_enUS),
					dbService.DataReaderData(12),
					dbService.DataReaderData(13),
					dbService.DataReaderData(14),
					((float)dbService.DataReaderData(15)).ToString(ci_enUS),
					((float)dbService.DataReaderData(16)).ToString(ci_enUS),
					((float)dbService.DataReaderData(17)).ToString(ci_enUS),
					((float)dbService.DataReaderData(18)).ToString(ci_enUS))
					);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t4():");
				return false;
			}
		}

		private bool pg2sql_pqp_t5(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t5 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					try
					{
						string col3 = "null";
						string col4 = "null";
						string col5 = "null";
						string col6 = "null";
						string col7 = "null";
						string col8 = "null";
						if (!(dbService.DataReaderData(3, false) is DBNull))
							col3 = (Double.Parse(dbService.DataReaderData(3).ToString())).ToString(ci_enUS);
						if (!(dbService.DataReaderData(4, false) is DBNull))
							col4 = (Double.Parse(dbService.DataReaderData(4).ToString())).ToString(ci_enUS);
						if (!(dbService.DataReaderData(5, false) is DBNull))
							col5 = (Double.Parse(dbService.DataReaderData(5).ToString())).ToString(ci_enUS);
						if (!(dbService.DataReaderData(6, false) is DBNull))
							col6 = (Double.Parse(dbService.DataReaderData(6).ToString())).ToString(ci_enUS);
						if (!(dbService.DataReaderData(7, false) is DBNull))
							col7 = (Double.Parse(dbService.DataReaderData(7).ToString())).ToString(ci_enUS);
						if (!(dbService.DataReaderData(8, false) is DBNull))
							col8 = (Double.Parse(dbService.DataReaderData(8).ToString())).ToString(ci_enUS);

						sbMain.Append(String.Format("INSERT INTO day_avg_parameters_t5 (datetime_id, flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6});\n",
						(DateTime.Parse(dbService.DataReaderData(2).ToString())).ToString("HH:mm:ss"),
						col3, col4, col5, col6, col7, col8));
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Error in EmDataSaver::pg2sql_pqp_t5() 1:");
						continue;
					}
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t5():  ");
				return false;
			}
		}

		private bool pg2sql_pqp_t6(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM day_avg_parameters_t6 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					string col2 = "null";
					string col3 = "null";
					string col4 = "null";
					string col5 = "null";
					string col6 = "null";
					string col7 = "null";
					string col8 = "null";
					if (!(dbService.DataReaderData(2, false) is DBNull))
						col2 = (Double.Parse(dbService.DataReaderData(2).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(3, false) is DBNull))
						col3 = (Double.Parse(dbService.DataReaderData(3).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(4, false) is DBNull))
						col4 = (Double.Parse(dbService.DataReaderData(4).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(5, false) is DBNull))
						col5 = (Double.Parse(dbService.DataReaderData(5).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(6, false) is DBNull))
						col6 = (Double.Parse(dbService.DataReaderData(6).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(7, false) is DBNull))
						col7 = (Double.Parse(dbService.DataReaderData(7).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(8, false) is DBNull))
						col8 = (Double.Parse(dbService.DataReaderData(8).ToString())).ToString(ci_enUS);

					sbMain.Append(String.Format("INSERT INTO day_avg_parameters_t6 (datetime_id, event_datetime, d_u_y, d_u_a, d_u_b, d_u_c, d_u_ab, d_u_bc, d_u_ca) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7});\n",
					((DateTime)dbService.DataReaderData(1)).ToString("MM.dd.yyyy HH:mm:ss"),
					col2, col3, col4, col5, col6, col7, col8));
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t6():");
				return false;
			}
		}

		private bool pg2sql_pqp_t7(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM day_avg_parameters_t7 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					string col1 = "null";
					if (!(dbService.DataReaderData(2, false) is DBNull))
						col1 = (Double.Parse(dbService.DataReaderData(2).ToString())).ToString(ci_enUS);

					sbMain.Append(String.Format("INSERT INTO day_avg_parameters_t7 (datetime_id, event_datetime, d_f) VALUES (currval('day_avg_parameter_times_datetime_id_seq'), '{0}', {1});\n",
					((DateTime)dbService.DataReaderData(1)).ToString("MM.dd.yyyy HH:mm:ss"), col1));
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_t7():");
				return false;
			}
		}

		#endregion

		#region Building SQL query for inserting AVG data from Database

		private bool pg2sql_avg_period(ref DbService dbServiceMain, ref EmSqlDataNode[] partNodes, Int64 objectId)
		{
			try
			{
				string commandText = string.Format(
					"SELECT count(*) FROM period_avg_params_times WHERE object_id = {0};",
					objectId);
				int num = (int)((long)dbServiceMain.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM period_avg_params_times WHERE object_id = {0};",
					objectId);
				dbServiceMain.ExecuteReader(commandText);

				int index = 0;
				FileStream fsTmp = null;
				StreamWriter swTmp = null;

				DbService dbServiceTmp = new DbService(string.Empty);
				dbServiceTmp.CopyConnection(ref dbServiceMain);
				while (dbServiceMain.DataReaderRead())
				{
					try
					{
						partNodes[index] = new EmSqlDataNode();
						long datetime_id = (long)dbServiceMain.DataReaderData("datetime_id");
						short period_id = (short)dbServiceMain.DataReaderData("period_id");
						int f_nom = (int)dbServiceMain.DataReaderData("f_nom");
						float u_nom_lin = (float)dbServiceMain.DataReaderData("u_nom_lin");
						float u_nom_ph = (float)dbServiceMain.DataReaderData("u_nom_ph");
						float i_nom_ph = (float)dbServiceMain.DataReaderData("i_nom_ph");

						AvgTypes curAvgType = AvgTypes.Bad;
						switch (period_id)
						{
							case 1: curAvgType = AvgTypes.ThreeSec; break;
							case 2: curAvgType = AvgTypes.OneMin; break;
							case 3: curAvgType = AvgTypes.ThirtyMin; break;
						}
						partNodes[index].SqlType = EmSqlDataNodeType.AVG;
						partNodes[index].Begin = (DateTime)dbServiceMain.DataReaderData("start_datetime");
						partNodes[index].End = (DateTime)dbServiceMain.DataReaderData("end_datetime");

						partNodes[index].AvgFileName = EmService.GetAvgTmpFileName(
								curAvgType.ToString(), partNodes[index].Begin);
						string avgFileFullPath = EmService.TEMP_IMAGE_DIR + 
							partNodes[index].AvgFileName;
						if (File.Exists(avgFileFullPath)) File.Delete(avgFileFullPath);

						fsTmp = new FileStream(avgFileFullPath, FileMode.Append);
						swTmp = new StreamWriter(fsTmp);

						// SQL запрос, который вставит в таблицу folders 
						// папку для года и вернет folder_id этой папки
						partNodes[index].Sql = string.Format("select insert_year_folder({0}, {1}, '{2}');\n",
												"{0}", "{1}", partNodes[index].Begin.Year.ToString());

						// SQL запрос, который вставит в таблицу folders 
						// папку для месяца и вернет folder_id этой папки
						partNodes[index].Sql += string.Format(
							"select insert_month_folder({0}, {1}, '{2}', '{3}');\n",
							"{0}", "{1}", partNodes[index].Begin.Year.ToString(),
							partNodes[index].Begin.Month.ToString());

						partNodes[index].Sql += string.Format("INSERT INTO period_avg_params_times (datetime_id, start_datetime, end_datetime, object_id, device_id, period_id, folder_year_id, folder_month_id, f_nom, u_nom_lin, u_nom_ph, i_nom_ph) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}", "{1}",
						period_id,
						"{2}", "{3}",
						f_nom.ToString(),
						u_nom_lin.ToString(new CultureInfo("en-US")),
						u_nom_ph.ToString(new CultureInfo("en-US")),
						i_nom_ph.ToString(new CultureInfo("en-US"))
						);

						string sql = string.Empty;
						if (!pg2sql_avg_14(datetime_id, ref dbServiceTmp, ref sql)) return false;
						if (!pg2sql_avg_5(datetime_id, ref dbServiceTmp, ref sql)) return false;
						// метрологи решили убрать мощности гармоник для ЭтПКЭ и ЭтПКЭ-А
						//if (!pg2sql_avg_6a(datetime_id, ref dbServiceTmp, ref sql)) return false;
						if (!pg2sql_avg_6b(datetime_id, ref dbServiceTmp, ref sql)) return false;

						swTmp.Write(sql);
						Thread.Sleep(3000);
						//partNodes[index].Sql += sql;

						++index;
					}
					finally
					{
						if (swTmp != null) swTmp.Close(); swTmp = null;
						if (fsTmp != null) fsTmp.Close(); fsTmp = null;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_avg_period():");
				return false;
			}
		}

		private bool pg2sql_avg_14(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format(
					"SELECT * FROM period_avg_params_1_4 WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return true;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_1_4 VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i <= 76; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
							sb.Append(((float)dbService.DataReaderData(i)).ToString(ci_enUS)).Append(",");
					}
					// Qtype
					if (dbService.DataReaderData(77, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((short)dbService.DataReaderData(77)).ToString()).Append(",");
					// mask1
					if (dbService.DataReaderData(78, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(78)).ToString()).Append(",");
					// mask2
					if (dbService.DataReaderData(79, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(79)).ToString()).Append(",");
					// from 'tangent_p_geom' to 'i1_n'
					for (int i = 80; i <= 97; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
							sb.Append(((float)dbService.DataReaderData(i)).ToString(ci_enUS)).Append(",");
					}

					sb.Remove(sb.Length - 1, 1).Append(");\n");
					sbMain.Append(sb);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_avg_14():");
				return false;
			}
		}

		private bool pg2sql_avg_5(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format(
					"SELECT * FROM period_avg_params_5 WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return true;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_5 VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i <= 241; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
							sb.Append(((float)dbService.DataReaderData(i)).ToString(ci_enUS)).Append(",");
					}
					// mask1
					if (dbService.DataReaderData(242, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(242)).ToString()).Append(",");
					// mask2
					if (dbService.DataReaderData(243, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(243)).ToString()).Append(",");

					// после маски были еще добавлены поля
					for (int i = 244; i < 404; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
							sb.Append(((float)dbService.DataReaderData(i)).ToString(ci_enUS)).Append(",");
					}

					sb.Remove(sb.Length - 1, 1).Append(");\n");
					sbMain.Append(sb);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in pg2sql_avg_5():  " + e.Message);
				return false;
			}
		}

		private bool pg2sql_avg_6a(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format(
					"SELECT * FROM period_avg_params_6a WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return true;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_6a VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i <= 125; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
							sb.Append(((float)dbService.DataReaderData(i)).ToString(ci_enUS)).Append(",");
					}
					// mask1
					if (dbService.DataReaderData(126, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(126)).ToString()).Append(",");
					// mask2
					if (dbService.DataReaderData(127, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(127)).ToString()).Append(",");

					sb.Remove(sb.Length - 1, 1).Append(");\n");
					sbMain.Append(sb);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in pg2sql_avg_6a():  " + e.Message);
				return false;
			}
		}

		private bool pg2sql_avg_6b(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format(
					"SELECT * FROM period_avg_params_6b WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return true;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_6b VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i <= 121; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
							sb.Append(((float)dbService.DataReaderData(i)).ToString(ci_enUS)).Append(",");
					}
					// mask1
					if (dbService.DataReaderData(122, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(122)).ToString()).Append(",");
					// mask2
					if (dbService.DataReaderData(123, false) is DBNull)
						sb.Append("null,");
					else
						sb.Append(((Int64)dbService.DataReaderData(123)).ToString()).Append(",");

					sb.Remove(sb.Length - 1, 1).Append(");\n");
					sbMain.Append(sb);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in pg2sql_avg_6b():  " + e.Message);
				return false;
			}
		}

		#endregion

		#region Building SQL query for inserting DNS data from Database

		private bool pg2sql_dns_period(ref DbService dbService, ref EmSqlDataNode[] partNodes, Int64 objectId)
		{
			DbService dbService2 = new DbService(string.Empty);
			try
			{
				dbService2.CopyConnection(ref dbService);

				string commandText = string.Format(
					"SELECT count(*) FROM dips_and_overs_times WHERE object_id = {0};",
					objectId);
				int num = (int)((long)dbService.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM dips_and_overs_times WHERE object_id = {0};",
					objectId);

				dbService.ExecuteReader(commandText);

				int index = 0;
				while (dbService.DataReaderRead())
				{
					partNodes[index] = new EmSqlDataNode();
					long datetime_id = (long)dbService.DataReaderData("datetime_id");
					partNodes[index].SqlType = EmSqlDataNodeType.Events;
					partNodes[index].Begin = (DateTime)dbService.DataReaderData("start_datetime");
					partNodes[index].End = (DateTime)dbService.DataReaderData("end_datetime");

					// SQL запрос, который вставит в таблицу folders 
					// папку для года и вернет folder_id этой папки
					partNodes[index].Sql = string.Format("select insert_year_folder({0}, {1}, '{2}');\n",
											"{0}", "{1}", partNodes[index].Begin.Year.ToString());

					// SQL запрос, который вставит в таблицу folders 
					// папку для месяца и вернет folder_id этой папки
					partNodes[index].Sql += string.Format("select insert_month_folder({0}, {1}, '{2}', '{3}');\n",
											"{0}", "{1}", partNodes[index].Begin.Year.ToString(),
											partNodes[index].Begin.Month.ToString());

					partNodes[index].Sql += string.Format("INSERT INTO dips_and_overs_times (datetime_id, start_datetime, end_datetime, object_id, device_id, folder_year_id, folder_month_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}", "{1}", "{2}", "{3}");

					string commandText2 = string.Format(
							"SELECT * FROM dips_and_overs WHERE datetime_id = {0};", datetime_id);
					dbService2.ExecuteReader(commandText2);

					StringBuilder sbMain = new StringBuilder();
					while (dbService2.DataReaderRead())
					{
						sbMain.Append(string.Format("INSERT INTO dips_and_overs (datetime_id, event_type, phase, deviation, start_datetime, end_datetime, pointer, is_finished, phase_num, is_earlier) VALUES (currval('dips_and_overs_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}', {5}, {6}, {7}, {8});\n",
							(short)dbService2.DataReaderData("event_type"),
							(string)dbService2.DataReaderData("phase"),
							((float)dbService2.DataReaderData("deviation")).ToString(new CultureInfo("en-US")),
							((DateTime)dbService2.DataReaderData("start_datetime")).ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
							((DateTime)dbService2.DataReaderData("end_datetime")).ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
							(long)dbService2.DataReaderData("pointer"),
							((bool)dbService2.DataReaderData("is_finished")).ToString(),
							(short)dbService2.DataReaderData("phase_num"),
							((bool)dbService2.DataReaderData("is_earlier")).ToString())
							);
					}
					partNodes[index].Sql += sbMain.ToString();

					++index;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_dns_period():");
				return false;
			}
			finally { dbService2.CloseReader(); }
			return true;
		}

		#endregion

		#endregion
	}
}
