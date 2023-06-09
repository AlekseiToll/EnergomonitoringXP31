using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;
using System.ComponentModel;

using EmDataSaver.SqlImage;
using EmDataSaver.SavingInterface;
using DbServiceLib;
using EmServiceLib;

namespace EmDataSaver
{
	public class EmDataExport33 : EmDataExportBase
	{
		#region Fields

		EmSqlDeviceImage sqlImage_ = null;

		#endregion

		#region Constructors

		public EmDataExport33(
			object sender,
			EmSqlDataNodeType[] parts,
			long archive_id,
			string pgSrvConnectStr,
			string fileName)
		{
			this.sender_ = sender;
			this.parts_ = parts;
			this.pgSrvConnectStr_ = pgSrvConnectStr;
			this.archive_id_ = archive_id;
			this.sqlImageFileName_ = fileName;
		}

		#endregion

		#region Properties

		public EmSqlDeviceImage SqlImage
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
				frmExport wndExport = new frmExport((EmDeviceType)e.Argument, sqlImageFileName_);
				wndExport.InputArchiveParts = parts_;

				if (wndExport.ShowDialog(sender_ as Form) != DialogResult.OK)
				{
					e.Cancel = true;
					return;
				}
				this.sqlImageFileName_ = wndExport.FileName;
				parts_ = wndExport.OutputArchiveParts;

				sqlImage_ = new EmSqlDeviceImage();

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

				//commandText = string.Format("SELECT * FROM databases db INNER JOIN devices dev ON db.db_id = {0} AND db.device_id = dev.device_id;", archive_id_);

				commandText = string.Format("SELECT * FROM folders f INNER JOIN (SELECT * FROM databases db INNER JOIN devices dev ON db.db_id = {0} AND db.device_id = dev.device_id) as t ON t.parent_id = f.folder_id", archive_id_);

				dbService.ExecuteReader(commandText);
				if (!dbService.DataReaderRead()) return false;

				sqlImage_.Name = dbService.DataReaderData("device_name") as string;
				if (sqlImage_.Name.Contains("T1"))
					sqlImage_.DeviceType = EmDeviceType.EM33T1;
				else if (sqlImage_.Name.Contains("3.1"))
					sqlImage_.DeviceType = EmDeviceType.EM31K;
				else sqlImage_.DeviceType = EmDeviceType.EM33T;

				sqlImage_.SerialNumber = (long)dbService.DataReaderData("device_id");
				sqlImage_.InternalType = (ushort)((short)dbService.DataReaderData("type_id"));
				sqlImage_.Version = dbService.DataReaderData("device_version") as string;

				// fliker
				bool flikkerExists = false;
				if (sqlImage_.DeviceType == EmDeviceType.EM33T1)
					flikkerExists = true;
				else if (sqlImage_.DeviceType == EmDeviceType.EM33T)
					if (Constants.isNewDeviceVersion_EM33T(sqlImage_.Version))
						flikkerExists = true;
				if (flikkerExists)
					sqlImage_.T_fliker = (short)dbService.DataReaderData("t_fliker");

				string folder_name =
					string.Format("{0} #{1} {2}", sqlImage_.Name, sqlImage_.SerialNumber, 
					DateTime.Now.ToString());

				sqlImage_.Sql = string.Format("SELECT check_dev_serial({0}, CAST({1} as smallint));\nINSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{2}', false, 0, 0, null);\nSELECT currval('folders_folder_id_seq');",
					sqlImage_.SerialNumber,
					sqlImage_.InternalType,
					folder_name);

				EmSqlArchive sqlArchive = new EmSqlArchive();
				sqlArchive.ObjectName = "Exported " + (string)dbService.DataReaderData("name");
				sqlArchive.CommonBegin = (DateTime)dbService.DataReaderData("start_datetime");
				sqlArchive.CommonEnd = (DateTime)dbService.DataReaderData("end_datetime");
				sqlArchive.ConnectionScheme = (ConnectScheme)((short)dbService.DataReaderData("con_scheme"));
				sqlArchive.U_NominalLinear = (float)dbService.DataReaderData("u_nom_lin");
				sqlArchive.U_NominalPhase = (float)dbService.DataReaderData("u_nom_ph");
				sqlArchive.F_Nominal = (float)dbService.DataReaderData("f_nom");
				sqlArchive.U_Limit = (float)dbService.DataReaderData("u_limit");
				sqlArchive.I_Limit = (float)dbService.DataReaderData("i_limit");
				sqlArchive.CurrentTransducerIndex = (ushort)((short)dbService.DataReaderData("current_transducer_index"));

				sqlArchive.Sql = string.Format(
					"INSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{0}', true, {1}, 0, null);\n",
				sqlArchive.ObjectName, "{0}");
				sqlArchive.Sql += string.Format(
					"INSERT INTO databases (db_id, start_datetime, end_datetime, con_scheme, f_nom, u_nom_lin, u_nom_ph, device_id, parent_id, db_name, db_info, u_limit, i_limit, current_transducer_index, device_name, device_version, t_fliker) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, currval('folders_folder_id_seq'), null, null, {7}, {8}, {9}, '{10}', '{11}', {12});\nSELECT currval('databases_db_id_seq');",
					sqlArchive.CommonBegin.ToString("MM.dd.yyyy HH:mm:ss"), // start_datetime
					sqlArchive.CommonEnd.ToString("MM.dd.yyyy HH:mm:ss"),	// end_datetime
					(short)sqlArchive.ConnectionScheme,
					sqlArchive.F_Nominal.ToString(new CultureInfo("en-US")),		// f_nom
					sqlArchive.U_NominalLinear.ToString(new CultureInfo("en-US")),	// u_nom_lin
					sqlArchive.U_NominalPhase.ToString(new CultureInfo("en-US")),	// u_nom_ph
					sqlImage_.SerialNumber,									// device_id
					sqlArchive.U_Limit.ToString(new CultureInfo("en-US")),	// u_limit
					sqlArchive.I_Limit.ToString(new CultureInfo("en-US")),	// i_limit
					sqlArchive.CurrentTransducerIndex,	// current_transduser_index
					sqlImage_.Name,				// device_name
					sqlImage_.Version,			// device_version
					sqlImage_.T_fliker);

				List<EmSqlDataNode> partNodes_list = new List<EmSqlDataNode>();
				for (int i = 0; i < this.parts_.Length; i++)
				{
					switch (this.parts_[i])
					{
						case EmSqlDataNodeType.PQP:
							SqlImage.EmSqlDataNode[] partNodesPQP = null;

							flikkerExists = false;
							if(sqlArchive.ConnectionScheme != ConnectScheme.Ph3W3 &&
								sqlArchive.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
							{
								if (sqlImage_.DeviceType == EmDeviceType.EM33T1)
									flikkerExists = true;
								else if (sqlImage_.DeviceType == EmDeviceType.EM33T)
									if (Constants.isNewDeviceVersion_EM33T(sqlImage_.Version))
										flikkerExists = true;
							}

							if (!pg2sql_pqp_period(ref dbService, ref partNodesPQP,
									flikkerExists, sqlArchive.ConnectionScheme)) return false;
							for (int p = 0; p < partNodesPQP.Length; p++)
								partNodes_list.Add(partNodesPQP[p]);
							break;
						case EmSqlDataNodeType.AVG:
							SqlImage.EmSqlDataNode partNodeAVG = new EmSqlDataNode();
							if (!pg2sql_avg_period(ref dbService, ref partNodeAVG)) return false;
							partNodes_list.Add(partNodeAVG);
							break;
						case EmSqlDataNodeType.Events:
							SqlImage.EmSqlDataNode partNodeEvents = new EmSqlDataNode();
							if (!pg2sql_dns_period(ref dbService, ref partNodeEvents)) return false;
							partNodes_list.Add(partNodeEvents);
							break;
					}
				}
				sqlArchive.Data = new EmSqlDataNode[partNodes_list.Count];
				partNodes_list.CopyTo(sqlArchive.Data);
				sqlImage_.Archives = new EmSqlArchive[] { sqlArchive };
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::pg2sql():");
				return false;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		#region Building SQL query for inserting PQP data from Database

		private bool pg2sql_pqp_period(ref DbService dbService,
				ref EmSqlDataNode[] partNodes, bool flikkerExists, ConnectScheme connectionScheme)
		{
			string commandText = string.Empty;
			DbService dbService2 = new DbService(string.Empty);
			try
			{
				commandText = string.Format(
					"SELECT count(*) FROM day_avg_parameter_times WHERE database_id = {0};",
					this.archive_id_);
				int num = (int)((long)dbService.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM day_avg_parameter_times WHERE database_id = {0};",
					this.archive_id_);
				dbService.ExecuteReader(commandText);

				int index = 0;
				dbService2.CopyConnection(ref dbService);
				while (dbService.DataReaderRead())
				{
					partNodes[index] = new EmSqlDataNode();
					long datetime_id = (long)dbService.DataReaderData("datetime_id");
					object constraint_type = (short)dbService.DataReaderData("constraint_type");
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

					partNodes[index].Sql = string.Format("INSERT INTO day_avg_parameter_times (datetime_id, start_datetime, end_time, constraint_type, ml_start_time_1, ml_end_time_1, ml_start_time_2, ml_end_time_2, database_id) VALUES (DEFAULT, '{0}', '{1}', {2}, '{3}', '{4}', '{5}', '{6}', {7});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						constraint_type,
						((DateTime)ml_start_time_1).ToString("MM.dd.yyyy HH:mm:ss"),
						((DateTime)ml_end_time_1).ToString("MM.dd.yyyy HH:mm:ss"),
						((DateTime)ml_start_time_2).ToString("MM.dd.yyyy HH:mm:ss"),
						((DateTime)ml_end_time_2).ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}");

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
					if (flikkerExists && 
						(connectionScheme != ConnectScheme.Ph3W3 && 
						connectionScheme != ConnectScheme.Ph3W3_B_calc))
					{
						if (!pg2sql_pqp_t5(datetime_id, ref dbService2, ref sql))
							return false;
					}

					if (!pg2sql_pqp_t6(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_t7(datetime_id, ref dbService2, ref sql)) return false;

					partNodes[index].Sql += sql;

					++index;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::pg2sql_pqp_period():");
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t1():");
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t2():");
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t3():");
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t4():");
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
						EmService.DumpException(ex, "Error in pg2sql_pqp_t5() 1 :");
						continue;
					}
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t5():");
				return false;
			}
		}

		private bool pg2sql_pqp_t6(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t6 WHERE datetime_id = {0};", datetime_id);
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t6():");
				return false;
			}
		}

		private bool pg2sql_pqp_t7(long datetime_id, ref DbService dbService, ref string sql)
		{
			string commandText = string.Empty;
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format("SELECT * FROM day_avg_parameters_t7 WHERE datetime_id = {0};", datetime_id);
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_t7():");
				return false;
			}
		}

		#endregion

		#region Building SQL query for inserting AVG data from Database

		private bool pg2sql_avg_period(ref DbService dbService, ref EmSqlDataNode partNode)
		{
			string commandText = string.Empty;
			try
			{
				commandText = string.Format(
					"SELECT * FROM period_avg_params_times WHERE database_id = {0};",
					this.archive_id_);

				dbService.ExecuteReader(commandText);
				if (!dbService.DataReaderRead()) return false;

				long datetime_id = (long)dbService.DataReaderData("datetime_id");
				short period_id = (short)dbService.DataReaderData("period_id");

				partNode.SqlType = EmSqlDataNodeType.AVG;
				partNode.Begin = (DateTime)dbService.DataReaderData("start_datetime");
				partNode.End = (DateTime)dbService.DataReaderData("end_datetime");

				partNode.Sql = string.Format("INSERT INTO period_avg_params_times (datetime_id, start_datetime, end_datetime, database_id, period_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3});\n",
					partNode.Begin.ToString("MM.dd.yyyy HH:mm:ss"),
					partNode.End.ToString("MM.dd.yyyy HH:mm:ss"),
					"{0}",
					period_id);

				string sql = string.Empty;
				pg2sql_avg_1_4(datetime_id, ref dbService, ref sql);
				pg2sql_avg_5(datetime_id, ref dbService, ref sql);
				pg2sql_avg_6a(datetime_id, ref dbService, ref sql);
				pg2sql_avg_6b(datetime_id, ref dbService, ref sql);

				partNode.Sql += sql;
				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in EmDataSaver::pg2sql_avg_period(): " + ex.Message);
				return false;
			}
		}

		private void pg2sql_avg_1_4(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				string commandText = "select relnatts from pg_class where relname = 'period_avg_params_1_4';";
				int cntColumns = dbService.ExecuteScalarInt16(commandText);

				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format(
					"SELECT * FROM period_avg_params_1_4 WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_1_4 VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i <= cntColumns - 2; i++)	//87 columns
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
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::pg2sql_avg_1_4(): ");
				throw;
			}
		}

		private void pg2sql_avg_5(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				string commandText = "select relnatts from pg_class where relname = 'period_avg_params_5';";
				int cntColumns = dbService.ExecuteScalarInt16(commandText);

				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format(
					"SELECT * FROM period_avg_params_5 WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return;

				StringBuilder sbMain = new StringBuilder();

				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_5 VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i < cntColumns; i++)	// 242 columns
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
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::pg2sql_avg_5(): ");
				throw;
			}
		}

		private void pg2sql_avg_6a(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				string commandText = "select relnatts from pg_class where relname = 'period_avg_params_6a';";
				int cntColumns = dbService.ExecuteScalarInt16(commandText);

				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format(
					"SELECT * FROM period_avg_params_6a WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_6a VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i < cntColumns; i++)	// 126 columns
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
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::pg2sql_avg_6a(): ");
				throw;
			}
		}

		private void pg2sql_avg_6b(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				string commandText = "select relnatts from pg_class where relname = 'period_avg_params_6b';";
				int cntColumns = dbService.ExecuteScalarInt16(commandText);

				CultureInfo ci_enUS = new CultureInfo("en-US");
				commandText = string.Format(
					"SELECT * FROM period_avg_params_6b WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO period_avg_params_6b VALUES (currval('period_avg_params_times_datetime_id_seq'), ");
					DateTime event_datetime = (DateTime)dbService.DataReaderData("event_datetime");
					sb.Append("'").Append(event_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("',");
					for (int i = 2; i < cntColumns; i++)	//122 columns
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
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::pg2sql_avg_6b(): ");
				throw;
			}
		}

		#endregion

		#region Building SQL query for inserting DNS data from Database

		private bool pg2sql_dns_period(ref DbService dbService, ref EmSqlDataNode partNode)
		{
			string commandText = string.Empty;
			try
			{
				commandText = string.Format(
					"SELECT * FROM dips_and_overs_times WHERE database_id = {0};",
					this.archive_id_);

				dbService.ExecuteReader(commandText);
				if (!dbService.DataReaderRead()) return false;

				long datetime_id = (long)dbService.DataReaderData("datetime_id");

				partNode.SqlType = EmSqlDataNodeType.Events;
				partNode.Begin = (DateTime)dbService.DataReaderData("start_datetime");
				partNode.End = (DateTime)dbService.DataReaderData("end_datetime");

				partNode.Sql = string.Format("INSERT INTO dips_and_overs_times (datetime_id, start_datetime, end_datetime, database_id) VALUES (DEFAULT, '{0}', '{1}', {2});\n",
					partNode.Begin.ToString("MM.dd.yyyy HH:mm:ss"),
					partNode.End.ToString("MM.dd.yyyy HH:mm:ss"),
					"{0}");

				commandText = string.Format(
					"SELECT * FROM dips_and_overs WHERE datetime_id = {0};",
					datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					sbMain.Append(string.Format("INSERT INTO dips_and_overs (datetime_id, event_type, phase, deviation, start_datetime, end_datetime) VALUES (currval('dips_and_overs_times_datetime_id_seq'), {0}, '{1}', {2}, '{3}', '{4}');\n",
						(short)dbService.DataReaderData("event_type"),
						dbService.DataReaderData("phase") as string,
						((float)dbService.DataReaderData("deviation")).ToString(new CultureInfo("en-US")),
						((DateTime)dbService.DataReaderData("start_datetime")).ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
						((DateTime)dbService.DataReaderData("end_datetime")).ToString("MM.dd.yyyy HH:mm:ss.FFFFF"))
						);
				}
				partNode.Sql += sbMain.ToString();
				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in EmDataSaver::pg2sql_dns_period(): " + ex.Message);
				return false;
			}
		}

		#endregion

		#endregion
	}
}
