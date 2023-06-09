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
using DeviceIO.Constraints;

namespace EmDataSaver
{
	public class EtDataExportPQP_A : EmDataExportBase
	{
		#region Fields

        EtPQP_A_SqlDeviceImage sqlImage_ = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor with a parameters to export data to the IMAGE
		/// </summary>
		/// <param name="sender">Sender object (must not be null)</param>
        /// <param name="fileName">File name to extract image from</param>
		/// <param name="pgSrvConnectStr">PostgreSQL connection string</param>
		public EtDataExportPQP_A(
			object sender,
			EmSqlDataNodeType[] parts,
			long device_id,
			long reg_id,
			string pgSrvConnectStr,
			string fileName)
		{
			this.sender_ = sender;
			this.parts_ = parts;
			this.pgSrvConnectStr_ = pgSrvConnectStr;
			this.device_id_to_export_ = device_id;
            this.object_reg_id_to_export_ = reg_id;
			this.sqlImageFileName_ = fileName;
		}

		#endregion

		#region Properties

        public EtPQP_A_SqlDeviceImage SqlImage
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
				frmExport wndExport = new frmExport(EmDeviceType.ETPQP_A, sqlImageFileName_);
				wndExport.InputArchiveParts = parts_;

				if (wndExport.ShowDialog(sender_ as Form) != DialogResult.OK)
				{
					e.Cancel = true;
					return;
				}
				this.sqlImageFileName_ = wndExport.FileName;
				parts_ = wndExport.OutputArchiveParts;

				sqlImage_ = new EtPQP_A_SqlDeviceImage();

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
				sqlImage_.DeviceInfo = EmService.GetStringDBValue(dbService.DataReaderData("dev_info"));
				sqlImage_.Version = EmService.GetStringDBValue(dbService.DataReaderData("dev_version"));

				sqlImage_.Sql = string.Format("select insert_new_device({0}, {1}, '{2}');\n",
					sqlImage_.SerialNumber,
					(sqlImage_.DeviceInfo == string.Empty) ? "null" : "'" + sqlImage_.DeviceInfo + "'",
					sqlImage_.Version
				);

				string folder_name = string.Format("ETPQP-A # {0}", sqlImage_.SerialNumber);

				// SQL запрос, который вставит в таблицу folders 
				// запись об устройстве (если ее еще нет) и вернет folder_id этой папки
				sqlImage_.Sql += String.Format("select insert_dev_folder({0}, '{1}');",
												sqlImage_.SerialNumber, folder_name);

				commandText =
                    string.Format("SELECT * FROM registrations WHERE device_id = {0} AND reg_id = {1};",
                    device_id_to_export_, object_reg_id_to_export_);
				dbService.ExecuteReader(commandText);
				if (!dbService.DataReaderRead()) return false;

                EmSqlRegistration sqlReg = new EmSqlRegistration();
				sqlReg.SysInfo = new SystemInfoEtPQP_A();
                sqlReg.CommonBegin = (DateTime)dbService.DataReaderData("start_datetime"); // start_datetime
                sqlReg.CommonEnd = (DateTime)dbService.DataReaderData("end_datetime");	// end_datetime
				sqlReg.ConnectionScheme = (ConnectScheme)EmService.GetShortDBValue(dbService.DataReaderData("con_scheme"));
				//if (oDevInfo is System.DBNull) sqlImage_.DeviceInfo = string.Empty;
				//else sqlImage_.DeviceInfo = (string)oDevInfo;
				sqlReg.SysInfo.F_Nominal = EmService.GetFloatDBValue(dbService.DataReaderData("f_nom"));
				sqlReg.SysInfo.U_NominalLinear = EmService.GetFloatDBValue(dbService.DataReaderData("u_nom_lin"));
				sqlReg.SysInfo.U_NominalPhase = EmService.GetFloatDBValue(dbService.DataReaderData("u_nom_ph"));
				sqlReg.SysInfo.I_NominalPhase = EmService.GetFloatDBValue(dbService.DataReaderData("i_nom_ph"));
				sqlReg.SysInfo.U_Limit = EmService.GetFloatDBValue(dbService.DataReaderData("u_limit"));
				sqlReg.SysInfo.I_Limit = EmService.GetFloatDBValue(dbService.DataReaderData("i_limit"));
				sqlReg.DeviceVersion = EmService.GetStringDBValue(dbService.DataReaderData("device_version"));
				sqlReg.SysInfo.T_fliker = EmService.GetShortDBValue(dbService.DataReaderData("t_fliker"));
				sqlReg.ObjectName = EmService.GetStringDBValue(dbService.DataReaderData("obj_name"));
				sqlReg.ObjectInfo = EmService.GetStringDBValue(dbService.DataReaderData("obj_info"));
				sqlReg.ConstraintType = EmService.GetShortDBValue(dbService.DataReaderData("constraint_type"));
				sqlReg.SysInfo.U_transformer_enable = EmService.GetBoolDBValue(dbService.DataReaderData("u_transformer_enable"));
				sqlReg.SysInfo.U_transformer_type = EmService.GetShortDBValue(dbService.DataReaderData("u_transformer_type"));
				sqlReg.SysInfo.U_consistent = EmService.GetIntDBValue(dbService.DataReaderData("u_consistent"));
				sqlReg.SysInfo.I_sensor_type = EmService.GetShortDBValue(dbService.DataReaderData("i_sensor_type"));
				sqlReg.SysInfo.I_transformer_usage = EmService.GetShortDBValue(dbService.DataReaderData("i_transformer_usage"));
				sqlReg.SysInfo.I_transformer_primary = EmService.GetShortDBValue(dbService.DataReaderData("i_transformer_primary"));
				sqlReg.SysInfo.Synchro_zero_enable = EmService.GetBoolDBValue(dbService.DataReaderData("synchro_zero_enable"));
				sqlReg.SysInfo.Autocorrect_time_gps_enable = EmService.GetBoolDBValue(dbService.DataReaderData("autocorrect_time_gps_enable"));
				sqlReg.SysInfo.Electro_system = EmService.GetShortDBValue(dbService.DataReaderData("electro_system"));
				sqlReg.SysInfo.Pqp_length = EmService.GetShortDBValue(dbService.DataReaderData("pqp_length"));
				sqlReg.SysInfo.F_Limit = EmService.GetIntDBValue(dbService.DataReaderData("f_limit"));
				sqlReg.SysInfo.Start_mode = EmService.GetShortDBValue(dbService.DataReaderData("start_mode"));
				sqlReg.SysInfo.Registration_stop_cnt = EmService.GetIntDBValue(dbService.DataReaderData("registration_stop_cnt"));
				sqlReg.SysInfo.Pqp_cnt = EmService.GetIntDBValue(dbService.DataReaderData("pqp_cnt"));
				sqlReg.SysInfo.Cnt_dip = EmService.GetIntDBValue(dbService.DataReaderData("cnt_dip"));
				sqlReg.SysInfo.Cnt_swell = EmService.GetIntDBValue(dbService.DataReaderData("cnt_swell"));
				sqlReg.SysInfo.Cnt_interrupt = EmService.GetIntDBValue(dbService.DataReaderData("cnt_interrupt"));
				sqlReg.SysInfo.Gps_Latitude = EmService.GetDoubleDBValue(dbService.DataReaderData("gps_latitude"), false);
				sqlReg.SysInfo.Gps_Longitude = EmService.GetDoubleDBValue(dbService.DataReaderData("gps_longitude"), false);
				sqlReg.SysInfo.Marked_on_off = EmService.GetBoolDBValue(dbService.DataReaderData("marked_on_off"));

				sqlReg.Constraints = new EtPQP_A_ConstraintsDetailed(ref dbService);

                // SQL запрос, который вставит в таблицу registrations 
				// запись о регистрации (если ее еще нет) и вернет reg_id этой регистрации
                sqlReg.Sql += string.Format("select insert_new_registration('{0}', '{1}', cast({2} as int2), {3}, {4}, {5}, cast({6} as int8), '{7}', null, {8}, {9}, '{10}', cast({11} as int2), cast({12} as int8), cast({13} as int2), {14});\n",
                    sqlReg.CommonBegin.ToString("MM.dd.yyyy HH:mm:ss"),
                    sqlReg.CommonEnd.ToString("MM.dd.yyyy HH:mm:ss"),
                    (short)sqlReg.ConnectionScheme,
					sqlReg.SysInfo.F_Nominal.ToString(new CultureInfo("en-US")),
					sqlReg.SysInfo.U_NominalLinear.ToString(new CultureInfo("en-US")),
					sqlReg.SysInfo.U_NominalPhase.ToString(new CultureInfo("en-US")),
                    "{0}",
                    sqlReg.ObjectName,
					sqlReg.SysInfo.U_Limit.ToString(new CultureInfo("en-US")),
					sqlReg.SysInfo.I_Limit.ToString(new CultureInfo("en-US")),
                    sqlReg.DeviceVersion,
					sqlReg.SysInfo.T_fliker,
                    sqlReg.RegistrationId,
                    sqlReg.ConstraintType,
					sqlReg.SysInfo.I_NominalPhase.ToString(new CultureInfo("en-US"))
                    );

				sqlReg.Sql += string.Format("UPDATE registrations SET u_transformer_enable = {0}, u_transformer_type = {1}, u_consistent = {2}, i_sensor_type = {3}, i_transformer_usage = {4}, i_transformer_primary = {5}, synchro_zero_enable = {6}, autocorrect_time_gps_enable = {7}, electro_system = {8}, pqp_length = {9}, f_limit = {10}, start_mode = {11}, registration_stop_cnt = {12}, pqp_cnt = {13}, cnt_dip = {14}, cnt_swell = {15}, cnt_interrupt = {16}, gps_latitude = {17}, gps_longitude = {18}, marked_on_off = {19} WHERE reg_id = currval('registrations_reg_id_seq');",
					sqlReg.SysInfo.U_transformer_enable,
					sqlReg.SysInfo.U_transformer_type,
					sqlReg.SysInfo.U_consistent,
					sqlReg.SysInfo.I_sensor_type,
					sqlReg.SysInfo.I_transformer_usage,
					sqlReg.SysInfo.I_transformer_primary,
					sqlReg.SysInfo.Synchro_zero_enable,
					sqlReg.SysInfo.Autocorrect_time_gps_enable,
					sqlReg.SysInfo.Electro_system,
					sqlReg.SysInfo.Pqp_length,
					sqlReg.SysInfo.F_Limit,
					sqlReg.SysInfo.Start_mode,
					sqlReg.SysInfo.Registration_stop_cnt,
					sqlReg.SysInfo.Pqp_cnt,
					sqlReg.SysInfo.Cnt_dip,
					sqlReg.SysInfo.Cnt_swell,
					sqlReg.SysInfo.Cnt_interrupt,
					sqlReg.SysInfo.Gps_Latitude.ToString(new CultureInfo("en-US")),
					sqlReg.SysInfo.Gps_Longitude.ToString(new CultureInfo("en-US")),
					sqlReg.SysInfo.Marked_on_off
					);

                #region Insert constraints

				sqlReg.Sql += string.Format("UPDATE registrations SET sets_f_synchro_down95 = {0}, sets_f_synchro_down100 = {1}, sets_f_synchro_up95 = {2}, sets_f_synchro_up100 = {3}, sets_f_isolate_down95 = {4}, sets_f_isolate_down100 = {5}, sets_f_isolate_up95 = {6}, sets_f_isolate_up100 = {7}, sets_u_deviation_down95 = {8}, sets_u_deviation_down100 = {9}, sets_u_deviation_up95 = {10}, sets_u_deviation_up100 = {11}, sets_flick_short_down95 = {12}, sets_flick_short_down100 = {13}, sets_flick_long_up95 = {14}, sets_flick_long_up100 = {15} WHERE reg_id = currval('registrations_reg_id_seq');",
                    sqlReg.Constraints.ConstraintsArray[0].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[1].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[2].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[3].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[4].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[5].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[6].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[7].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[8].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[9].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[10].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[11].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[12].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[13].ToString(new CultureInfo("en-US")),
					sqlReg.Constraints.ConstraintsArray[14].ToString(new CultureInfo("en-US")),
					sqlReg.Constraints.ConstraintsArray[15].ToString(new CultureInfo("en-US")));
                string tmpConstr = "UPDATE registrations SET ";
                for (int iConstr = 2; iConstr <= 40; ++iConstr)
                {
                    tmpConstr += "sets_k_harm_95_" + iConstr.ToString();
                    tmpConstr += string.Format(" = {0}, ",
                        sqlReg.Constraints.ConstraintsArray[iConstr + 14].ToString(
                        new CultureInfo("en-US")));
                }
                for (int iConstr = 2; iConstr <= 40; ++iConstr)
                {
                    tmpConstr += "sets_k_harm_100_" + iConstr.ToString();
                    tmpConstr += string.Format(" = {0}, ",
                        sqlReg.Constraints.ConstraintsArray[iConstr + 14 + 39].ToString(
                        new CultureInfo("en-US")));
                }
                tmpConstr += string.Format("sets_k_harm_total_95 = {0}, sets_k_harm_total_100 = {1}, sets_k2u_95 = {2}, sets_k2u_100 = {3}, sets_k0u_95 = {4}, sets_k0u_100 = {5} WHERE reg_id = currval('registrations_reg_id_seq');\n",
                    sqlReg.Constraints.ConstraintsArray[94].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[95].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[96].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[97].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[98].ToString(new CultureInfo("en-US")),
                    sqlReg.Constraints.ConstraintsArray[99].ToString(new CultureInfo("en-US")));

                sqlReg.Sql += tmpConstr;

                #endregion

				folder_name = string.Format("ETPQP-A # {0} # {1}",
												sqlImage_.SerialNumber,
                                                sqlReg.ObjectName);

				// SQL запрос, который вставит в таблицу folders 
				// запись об объекте (если ее еще нет) и вернет folder_id этой папки
                sqlReg.Sql += String.Format("select insert_registration_folder({0}, {1}, '{2}');",
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

                sqlReg.DataPQP = new EmSqlDataNode[partNodes_listPQP.Count];
                partNodes_listPQP.CopyTo(sqlReg.DataPQP);
                sqlReg.DataAVG = new EmSqlDataNode[partNodes_listAVG.Count];
                partNodes_listAVG.CopyTo(sqlReg.DataAVG);
                sqlReg.DataDNS = new EmSqlDataNode[partNodes_listDNS.Count];
                partNodes_listDNS.CopyTo(sqlReg.DataDNS);

				sqlImage_.Registrations = new EmSqlRegistration[1];
                sqlImage_.Registrations[0] = sqlReg;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pg2sql(): ");
				return false;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		#region Building SQL query for inserting PQP data from Database

		private bool pg2sql_pqp_period(ref DbService dbService, ref EmSqlDataNode[] partNodes, Int64 regId)
		{
			string commandText = string.Empty;
			DbService dbService2 = new DbService(string.Empty);
			try
			{
				commandText = string.Format(
					"SELECT count(*) FROM pqp_times WHERE registration_id = {0};",
					regId);
				int num = (int)((long)dbService.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM pqp_times WHERE registration_id = {0};",
					regId);
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
					Int64 archive_id = (Int64)dbService.DataReaderData("archive_id");

					partNodes[index].SqlType = EmSqlDataNodeType.PQP;
					partNodes[index].Begin = (DateTime)dbService.DataReaderData("start_datetime");
					partNodes[index].End = (DateTime)dbService.DataReaderData("end_time");

					// на выходе имеем SQL запрос добавления записи 
					// периода ПКЭ а также всех дочерних данных по ПКЭ,
					// имеющий 3 недостающих параметра - registration_id, device_id
					partNodes[index].Sql = string.Format("INSERT INTO pqp_times (datetime_id, start_datetime, end_time, constraint_type, registration_id, device_id, parent_folder_id, con_scheme, f_nom, u_nom_lin, u_nom_ph, archive_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, '{4}', '{5}', '{6}', '{7}', {8}, {9}, {10});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						constraint_type,
						"{0}",
						"{1}",
						"{2}",
						con_scheme,
						f_nom,
						u_nom_lin.ToString(new CultureInfo("en-US")),
						u_nom_ph.ToString(new CultureInfo("en-US")),
						archive_id);

					string sql = string.Empty;

					if (!pg2sql_pqp_df(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_du(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_df_val(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_du_val(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_nonsymmetry(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_nonsinus(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_dip_swell(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_flicker(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_flicker_val(datetime_id, ref dbService2, ref sql)) return false;
					if (!pg2sql_pqp_interharm_u(datetime_id, ref dbService2, ref sql)) return false;

					partNodes[index].Sql += sql;

					++index;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_pqp_period():");
				return false;
			}
			finally
			{
				dbService2.CloseReader();
			}
		}

		private bool pg2sql_pqp_df(long datetime_id, ref DbService dbService, ref string sql)
        {
			string commandText = string.Empty;
            try
            {
                CultureInfo ci_enUS = new CultureInfo("en-US");
                commandText = string.Format("SELECT * FROM pqp_f WHERE datetime_id = {0};",
                    datetime_id);
                dbService.ExecuteReader(commandText);

                StringBuilder sbMain = new StringBuilder();
                while (dbService.DataReaderRead())
                {
                    //string str_up = string.Empty, str_down = string.Empty;
                    //str_up = (dbService.DataReaderData(9] is DBNull) ? "NULL" : ((float)dbService.DataReaderData(9]).ToString(ci_enUS);
                    //str_down = (dbService.DataReaderData(11] is DBNull) ? "NULL" : ((float)dbService.DataReaderData(11]).ToString(ci_enUS);

                    sbMain.Append(
                        string.Format("INSERT INTO pqp_f (datetime_id, param_id, archive_id, num_all, num_synchro, num_not_synchro, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_top_syn, real_max_rng_top_syn, real_nrm_rng_bottom_syn, real_max_rng_bottom_syn, calc_nrm_rng_top, calc_max_rng_top, calc_nrm_rng_bottom, calc_max_rng_bottom, valid_f, real_nrm_rng_top_iso, real_max_rng_top_iso, real_nrm_rng_bottom_iso, real_max_rng_bottom_iso) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20});\n",
                    dbService.DataReaderData(1),
                    dbService.DataReaderData(2),
                    dbService.DataReaderData(3),
                    dbService.DataReaderData(4),
                    dbService.DataReaderData(5),
                    dbService.DataReaderData(6),
                    dbService.DataReaderData(7),
                    dbService.DataReaderData(8),
                    ((float)dbService.DataReaderData(9)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(10)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(11)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(12)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(13)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(14)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(15)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(16)).ToString(ci_enUS),
                    dbService.DataReaderData(17),
                    ((float)dbService.DataReaderData(18)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(19)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(20)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(21)).ToString(ci_enUS)
                    ));
                }
                sql += sbMain.ToString();
                return true;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in pg2sql_pqp_df():");
                return false;
            }
        }

		private bool pg2sql_pqp_du(long datetime_id, ref DbService dbService, ref string sql)
        {
			string commandText = string.Empty;
            try
            {
                CultureInfo ci_enUS = new CultureInfo("en-US");
                commandText = string.Format("SELECT * FROM pqp_du WHERE datetime_id = {0};", datetime_id);
                dbService.ExecuteReader(commandText);

                StringBuilder sbMain = new StringBuilder();
                while (dbService.DataReaderRead())
                {
                    //string str_up = string.Empty, str_down = string.Empty;
                    //str_up = (dbService.DataReaderData(9] is DBNull) ? "NULL" : ((float)dbService.DataReaderData(9]).ToString(ci_enUS);
                    //str_down = (dbService.DataReaderData(11] is DBNull) ? "NULL" : ((float)dbService.DataReaderData(11]).ToString(ci_enUS);

                    sbMain.Append(
						string.Format("INSERT INTO pqp_du (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng_top, real_max_rng_top, calc_nrm_rng_top, calc_max_rng_top, valid_du, real_nrm_rng_bottom, real_max_rng_bottom) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14});\n",
                    dbService.DataReaderData(1),
                    dbService.DataReaderData(2),
                    dbService.DataReaderData(3),
                    dbService.DataReaderData(4),
                    dbService.DataReaderData(5),
                    dbService.DataReaderData(6),
                    dbService.DataReaderData(7),
                    dbService.DataReaderData(8),
                    ((float)dbService.DataReaderData(9)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(10)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(11)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(12)).ToString(ci_enUS),
					dbService.DataReaderData(13),
					((float)dbService.DataReaderData(14)).ToString(ci_enUS),
					((float)dbService.DataReaderData(15)).ToString(ci_enUS)
                    ));
                }
                sql += sbMain.ToString();
                return true;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in pg2sql_pqp_du():");
                return false;
            }
        }

		private bool pg2sql_pqp_du_val(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM pqp_du_val WHERE datetime_id = {0};", datetime_id);
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
					if (!(dbService.DataReaderData(3, false) is DBNull))
						col2 = (Double.Parse(dbService.DataReaderData(3).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(4, false) is DBNull))
						col3 = (Double.Parse(dbService.DataReaderData(4).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(5, false) is DBNull))
						col4 = (Double.Parse(dbService.DataReaderData(5).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(6, false) is DBNull))
						col5 = (Double.Parse(dbService.DataReaderData(6).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(7, false) is DBNull))
						col6 = (Double.Parse(dbService.DataReaderData(7).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(8, false) is DBNull))
						col7 = (Double.Parse(dbService.DataReaderData(8).ToString())).ToString(ci_enUS);

					sbMain.Append(String.Format("INSERT INTO pqp_du_val (datetime_id, event_datetime, u_a_ab_pos, u_b_bc_pos, u_c_ca_pos, u_a_ab_neg, u_b_bc_neg, u_c_ca_neg, record_marked, record_seconds) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});\n",
					((DateTime)dbService.DataReaderData(2)).ToString("MM.dd.yyyy HH:mm:ss"),
					col2, col3, col4, col5, col6, col7,
					dbService.DataReaderData(9), dbService.DataReaderData(10)));
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_du_val():");
				return false;
			}
		}

		private bool pg2sql_pqp_df_val(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM pqp_df_val WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					string col1 = "null";
					if (!(dbService.DataReaderData(2, false) is DBNull))
						col1 = (Double.Parse(dbService.DataReaderData(3).ToString())).ToString(ci_enUS);

					sbMain.Append(String.Format("INSERT INTO pqp_df_val (datetime_id, event_datetime, d_f, f_seconds) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2});\n",
					((DateTime)dbService.DataReaderData(2)).ToString("MM.dd.yyyy HH:mm:ss"), col1, dbService.DataReaderData(4)));
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_df_val():");
				return false;
			}
		}

		private bool pg2sql_pqp_nonsymmetry(long datetime_id, ref DbService dbService, ref string sql)
        {
            try
            {
                CultureInfo ci_enUS = new CultureInfo("en-US");
                string commandText = string.Format("SELECT * FROM pqp_nonsymmetry WHERE datetime_id = {0};", datetime_id);
                dbService.ExecuteReader(commandText);

                StringBuilder sbMain = new StringBuilder();
                while (dbService.DataReaderRead())
                {
                    //string str_up = string.Empty, str_down = string.Empty;
                    //str_up = (dbService.DataReaderData(9] is DBNull) ? "NULL" : ((float)dbService.DataReaderData(9]).ToString(ci_enUS);
                    //str_down = (dbService.DataReaderData(11] is DBNull) ? "NULL" : ((float)dbService.DataReaderData(11]).ToString(ci_enUS);

                    sbMain.Append(
                        string.Format("INSERT INTO pqp_nonsymmetry (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng, num_max_rng, num_out_max_rng, real_nrm_rng, real_max_rng, calc_nrm_rng, calc_max_rng, valid_nonsymm) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12});\n",
                    dbService.DataReaderData(1),
                    dbService.DataReaderData(2),
                    dbService.DataReaderData(3),
                    dbService.DataReaderData(4),
                    dbService.DataReaderData(5),
                    dbService.DataReaderData(6),
                    dbService.DataReaderData(7),
                    dbService.DataReaderData(8),
                    ((float)dbService.DataReaderData(9)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(10)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(11)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(12)).ToString(ci_enUS),
                    dbService.DataReaderData(13)));
                }
                sql += sbMain.ToString();
                return true;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in pg2sql_pqp_nonsymmetry():");
                return false;
            }
        }

		private bool pg2sql_pqp_nonsinus(long datetime_id, ref DbService dbService, ref string sql)
        {
            try
            {
                CultureInfo ci_enUS = new CultureInfo("en-US");
                string commandText = string.Format("SELECT * FROM pqp_nonsinus WHERE datetime_id = {0};", datetime_id);
                dbService.ExecuteReader(commandText);

                StringBuilder sbMain = new StringBuilder();
                while (dbService.DataReaderRead())
                {
                    sbMain.Append(
                        string.Format("INSERT INTO pqp_nonsinus (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng, valid_harm) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22});\n",
                    dbService.DataReaderData(1),
                    dbService.DataReaderData(2),
                    dbService.DataReaderData(3),
                    dbService.DataReaderData(4),
                    dbService.DataReaderData(5),
                    dbService.DataReaderData(6),
                    dbService.DataReaderData(7),
                    dbService.DataReaderData(8),
                    ((float)dbService.DataReaderData(9)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(10)).ToString(ci_enUS),
                    dbService.DataReaderData(11),
                    dbService.DataReaderData(12),
                    dbService.DataReaderData(13),
                    ((float)dbService.DataReaderData(14)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(15)).ToString(ci_enUS),
                    dbService.DataReaderData(16),
                    dbService.DataReaderData(17),
                    dbService.DataReaderData(18),
                    ((float)dbService.DataReaderData(19)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(20)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(21)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(22)).ToString(ci_enUS),
                    dbService.DataReaderData(23)));
                }
                sql += sbMain.ToString();
                return true;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in pg2sql_pqp_nonsinus():");
                return false;
            }
        }

		private bool pg2sql_pqp_interharm_u(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM pqp_interharm_u WHERE datetime_id = {0} ORDER BY param_num;", datetime_id);
				dbService.ExecuteReader(commandText);

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					sbMain.Append(
						string.Format("INSERT INTO pqp_interharm_u (datetime_id, param_id, param_num, archive_id, num_all, num_marked, num_not_marked, val_ph1, val_ph2, val_ph3, valid_interharm) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9});\n",
					"null",//dbService.DataReaderData(1], param_id
					dbService.DataReaderData(2),
					dbService.DataReaderData(3),
					dbService.DataReaderData(4),
					dbService.DataReaderData(5),
					dbService.DataReaderData(6),
					((float)dbService.DataReaderData(7)).ToString(ci_enUS),
					((float)dbService.DataReaderData(8)).ToString(ci_enUS),
					((float)dbService.DataReaderData(9)).ToString(ci_enUS),
					dbService.DataReaderData(10)));
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_interharm_u():");
				return false;
			}
		}

		private bool pg2sql_pqp_dip_swell(long datetime_id, ref DbService dbService, ref string sql)
        {
            try
            {
                CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM pqp_dip_swell WHERE datetime_id = {0};",
                    datetime_id);
                dbService.ExecuteReader(commandText);

                StringBuilder sbMain = new StringBuilder();
                while (dbService.DataReaderRead())
                {
                    sbMain.Append(
						string.Format("INSERT INTO pqp_dip_swell (datetime_id, param_id, archive_id, num_0_01_till_0_05, num_0_05_till_0_1, num_0_1_till_0_5, num_0_5_till_1, num_1_till_3, num_3_till_20, num_20_till_60, num_over_60, num_over_180) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});\n",
                    dbService.DataReaderData(1),
                    dbService.DataReaderData(2),
                    dbService.DataReaderData(3),
                    dbService.DataReaderData(4),
                    dbService.DataReaderData(5),
                    dbService.DataReaderData(6),
                    dbService.DataReaderData(7),
                    dbService.DataReaderData(8),
                    dbService.DataReaderData(9),
                    dbService.DataReaderData(10),
					dbService.DataReaderData(11)));
                }
                sql += sbMain.ToString();
                return true;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in pg2sql_pqp_dip_swell():");
                return false;
            }
        }

		private bool pg2sql_pqp_flicker(long datetime_id, ref DbService dbService, ref string sql)
        {
            try
            {
                CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM pqp_flicker WHERE datetime_id = {0};",
                    datetime_id);
                dbService.ExecuteReader(commandText);

                StringBuilder sbMain = new StringBuilder();
                while (dbService.DataReaderRead())
                {
                    sbMain.Append(
                        string.Format("INSERT INTO pqp_flicker (datetime_id, param_id, archive_id, num_all, num_marked, num_not_marked, num_nrm_rng_ph1, num_max_rng_ph1, num_out_max_rng_ph1, calc_nrm_rng_ph1, calc_max_rng_ph1, num_nrm_rng_ph2, num_max_rng_ph2, num_out_max_rng_ph2, calc_nrm_rng_ph2, calc_max_rng_ph2, num_nrm_rng_ph3, num_max_rng_ph3, num_out_max_rng_ph3, calc_nrm_rng_ph3, calc_max_rng_ph3, real_nrm_rng, real_max_rng, valid_flick) VALUES (currval('pqp_times_datetime_id_seq'), {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22});\n",
                    dbService.DataReaderData(1),
                    dbService.DataReaderData(2),
                    dbService.DataReaderData(3),
                    dbService.DataReaderData(4),
                    dbService.DataReaderData(5),
                    dbService.DataReaderData(6),
                    dbService.DataReaderData(7),
                    dbService.DataReaderData(8),
                    ((float)dbService.DataReaderData(9)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(10)).ToString(ci_enUS),
                    dbService.DataReaderData(11),
                    dbService.DataReaderData(12),
                    dbService.DataReaderData(13),
                    ((float)dbService.DataReaderData(14)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(15)).ToString(ci_enUS),
                    dbService.DataReaderData(16),
                    dbService.DataReaderData(17),
                    dbService.DataReaderData(18),
                    ((float)dbService.DataReaderData(19)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(20)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(21)).ToString(ci_enUS),
                    ((float)dbService.DataReaderData(22)).ToString(ci_enUS),
                    dbService.DataReaderData(23) ));
                }
                sql += sbMain.ToString();
                return true;
            }
            catch (Exception e)
            {
                EmService.DumpException(e, "Error in pg2sql_pqp_flicker():");
                return false;
            }
        }

		private bool pg2sql_pqp_flicker_val(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format("SELECT * FROM pqp_flicker_val WHERE datetime_id = {0};", datetime_id);
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
					if (!(dbService.DataReaderData(3, false) is DBNull))
						col2 = (Double.Parse(dbService.DataReaderData(3).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(4, false) is DBNull))
						col3 = (Double.Parse(dbService.DataReaderData(4).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(5, false) is DBNull))
						col4 = (Double.Parse(dbService.DataReaderData(5).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(6, false) is DBNull))
						col5 = (Double.Parse(dbService.DataReaderData(6).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(7, false) is DBNull))
						col6 = (Double.Parse(dbService.DataReaderData(7).ToString())).ToString(ci_enUS);
					if (!(dbService.DataReaderData(8, false) is DBNull))
						col7 = (Double.Parse(dbService.DataReaderData(8).ToString())).ToString(ci_enUS);

					sbMain.Append(String.Format("INSERT INTO pqp_flicker_val (datetime_id, flik_time, flik_a, flik_a_long, flik_b, flik_b_long, flik_c, flik_c_long, flik_short_seconds, flik_lond_seconds, flik_short_marked, flik_long_marked) VALUES (currval('pqp_times_datetime_id_seq'), '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});\n",
					((DateTime)dbService.DataReaderData(2)).ToString("MM.dd.yyyy HH:mm:ss"),
					col2, col3, col4, col5, col6, col7,
					dbService.DataReaderData(9), 
					dbService.DataReaderData(10),
					dbService.DataReaderData(11),
					dbService.DataReaderData(12)));
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in pg2sql_pqp_flicker_val():");
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
					"SELECT count(*) FROM avg_times WHERE registration_id = {0};",
					objectId);
				int num = (int)((long)dbServiceMain.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM avg_times WHERE registration_id = {0};",
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
						short period_id = EmService.GetShortDBValue(dbServiceMain.DataReaderData("period_id"));
						int f_nom = EmService.GetIntDBValue(dbServiceMain.DataReaderData("f_nom"));
						float u_nom_lin = EmService.GetFloatDBValue(dbServiceMain.DataReaderData("u_nom_lin"));
						float u_nom_ph = EmService.GetFloatDBValue(dbServiceMain.DataReaderData("u_nom_ph"));
						float i_nom_ph = EmService.GetFloatDBValue(dbServiceMain.DataReaderData("i_nom_ph"));
						short conScheme = EmService.GetShortDBValue(dbServiceMain.DataReaderData("con_scheme"));
						int i_limit = EmService.GetIntDBValue(dbServiceMain.DataReaderData("i_limit"));
						int f_limit = EmService.GetIntDBValue(dbServiceMain.DataReaderData("f_limit"));
						int u_limit = EmService.GetIntDBValue(dbServiceMain.DataReaderData("u_limit"));
						short current_sensor_type = EmService.GetShortDBValue(dbServiceMain.DataReaderData("current_sensor_type"));
						bool u_transformer_enable = EmService.GetBoolDBValue(dbServiceMain.DataReaderData("u_transformer_enable"));
						short u_transformer_type = EmService.GetShortDBValue(dbServiceMain.DataReaderData("u_transformer_type"));
						//int u_declared = GetIntDBValue(dbService.DataReaderData("u_declared"));
						int i_transformer_primary = EmService.GetIntDBValue(dbServiceMain.DataReaderData("i_transformer_primary"));
						short i_transformer_enable = EmService.GetShortDBValue(dbServiceMain.DataReaderData("i_transformer_enable"));

						AvgTypes_PQP_A curAvgType = AvgTypes_PQP_A.Bad;
						switch (period_id)
						{
							case 1: curAvgType = AvgTypes_PQP_A.ThreeSec; break;
							case 2: curAvgType = AvgTypes_PQP_A.TenMin; break;
							case 3: curAvgType = AvgTypes_PQP_A.TwoHours; break;
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

						partNodes[index].Sql = string.Format("INSERT INTO avg_times (datetime_id, start_datetime, end_datetime, registration_id, period_id, device_id, parent_folder_id, f_nom, u_nom_lin, u_nom_ph, i_nom_ph, con_scheme, i_limit, f_limit, u_limit, current_sensor_type, u_transformer_enable, u_transformer_type, i_transformer_primary, i_transformer_enable) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}",
						period_id,
						"{1}",
						"{2}",
						f_nom.ToString(),
						u_nom_lin.ToString(new CultureInfo("en-US")),
						u_nom_ph.ToString(new CultureInfo("en-US")),
						i_nom_ph.ToString(new CultureInfo("en-US")),
						//GetNumericFieldAsString(dbService.DataReaderData("f_nom"]),
						//GetNumericFieldAsString(dbService.DataReaderData("u_nom_lin"]),
						//GetNumericFieldAsString(dbService.DataReaderData("u_nom_ph"]),
						//GetNumericFieldAsString(dbService.DataReaderData("i_nom_ph"]),
						conScheme,
						i_limit, f_limit, u_limit,
						//GetNumericFieldAsString(dbService.DataReaderData("i_limit"]),
						//GetNumericFieldAsString(dbService.DataReaderData("f_limit"]),
						//GetNumericFieldAsString(dbService.DataReaderData("u_limit"]),
						current_sensor_type,
						//GetNumericFieldAsString(dbService.DataReaderData("current_sensor_type"]),
						u_transformer_enable, u_transformer_type,
						//u_declared,
						i_transformer_primary,
						i_transformer_enable
						//GetNumericFieldAsString(dbService.DataReaderData("u_declared"]),
						//GetNumericFieldAsString(dbService.DataReaderData("i_transformer_primary"]),
						//GetNumericFieldAsString(dbService.DataReaderData("i_transformer_enable"])
						);

						string sql = string.Empty;
						if (!pg2sql_avg_service_info(datetime_id, ref dbServiceTmp, ref sql)) return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_angles", 18, ref sql)) 
							return false;
						// метрологи решили убрать мощности гармоник для ЭтПКЭ и ЭтПКЭ-А
						//if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_harm_power", 802, ref sql))
						//	return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_i_harmonics", 406, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_i_interharmonics", 206, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_power", 31, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_pqp", 47, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_u_i_f", 48, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_u_lin_harmonics", 305, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_u_lin_interharmonics", 155, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_u_ph_interharmonics", 155, ref sql))
							return false;
						if (!pg2sql_avg_real_table(datetime_id, ref dbServiceTmp, "avg_u_phase_harmonics", 305, ref sql))
							return false;

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

		private bool pg2sql_avg_service_info(long datetime_id, ref DbService dbService, ref string sql)
		{
			try
			{
				string commandText = string.Format(
					"SELECT * FROM avg_service_info WHERE datetime_id = {0};", datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return true;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder("INSERT INTO avg_service_info VALUES (currval('avg_times_datetime_id_seq'), ");
					sb.Append(((long)dbService.DataReaderData("record_id")).ToString()).Append(", ");

					DateTime start_datetime = (DateTime)dbService.DataReaderData("dt_start");
					sb.Append("'").Append(start_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");
					DateTime end_datetime = (DateTime)dbService.DataReaderData("dt_end");
					sb.Append("'").Append(end_datetime.ToString("MM.dd.yyyy HH:mm:ss")).Append("', ");

					sb.Append(((Int32)dbService.DataReaderData("length_millisec")).ToString()).Append(", ");
					sb.Append(((Int32)dbService.DataReaderData("all_samples")).ToString()).Append(", ");

					sb.Append("null, "); // dummy: Количество сэмплов(выборок) в измерительных окнах, 15 значений
					// это поле не используется, оно нужно для внутр. расчетов в приборе

					sb.Append(((Int32)dbService.DataReaderData("cnt_windows_locked_a")).ToString()).Append(", ");
					sb.Append(((Int32)dbService.DataReaderData("cnt_windows_locked_b")).ToString()).Append(", ");
					sb.Append(((Int32)dbService.DataReaderData("cnt_windows_locked_c")).ToString()).Append(", ");
					sb.Append(((Int32)dbService.DataReaderData("cnt_windows_locked_ab")).ToString()).Append(", ");
					sb.Append(((Int32)dbService.DataReaderData("cnt_windows_locked_bc")).ToString()).Append(", ");
					sb.Append(((Int32)dbService.DataReaderData("cnt_windows_locked_ca")).ToString()).Append(", ");
					sb.Append(GetNumericFieldAsString(dbService.DataReaderData("cnt_windows_not_locked"))).Append(", ");

					sb.Append(GetNumericFieldAsString(dbService.DataReaderData("if_record_marked"))).Append(");");

					sbMain.Append(sb);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_avg_service_info():");
				return false;
			}
		}

		#region Old Code
		//private bool pg2sql_avg_angles(long datetime_id, NpgsqlCommand sqlCommand, ref string sql)
		//{
		//    NpgsqlDataReader dataReader = null;
		//    try
		//    {
		//        CultureInfo ci_enUS = new CultureInfo("en-US");
		//        commandText = string.Format(
		//            "SELECT * FROM avg_angles WHERE datetime_id = {0};", datetime_id);
		//        dbService.ExecuteReader(commandText);

		//        if (!dataReader.HasRows) return true;

		//        StringBuilder sbMain = new StringBuilder();
		//        while (dbService.DataReaderRead())
		//        {
		//            StringBuilder sb = new StringBuilder("INSERT INTO avg_angles VALUES (currval('avg_times_datetime_id_seq'), ");
		//            sb.Append(((long)dbService.DataReaderData("record_id"]).ToString()).Append(", ");
		//            for (int i = 2; i <= 17; i++)
		//            {
		//                if (dbService.DataReaderData(i] is DBNull)
		//                    sb.Append("null,");
		//                else
		//                    sb.Append(((float)dbService.DataReaderData(i]).ToString(ci_enUS)).Append(",");
		//            }
		//            sb.Remove(sb.Length - 1, 1).Append(");\n");
		//            sbMain.Append(sb);
		//        }
		//        sql += sbMain.ToString();
		//        return true;
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Error in pg2sql_avg_angles():");
		//        return false;
		//    }
		//    finally
		//    {
		//        if (dataReader != null) dataReader.Close();
		//    }
		//}
		#endregion

		// Типовая функция, создающая sql-запрос для таблицы, в которой первые 2 поля - datetime_id и
		// record_id, а все остальные поля имеют тип real
		private bool pg2sql_avg_real_table(long datetime_id, ref DbService dbService, 
											string table_name, int real_columns_count, ref string sql)
		{
			try
			{
				CultureInfo ci_enUS = new CultureInfo("en-US");
				string commandText = string.Format(
					"SELECT * FROM {0} WHERE datetime_id = {1};",
					table_name, datetime_id);
				dbService.ExecuteReader(commandText);

				if (!dbService.DataReaderHasRows) return true;

				StringBuilder sbMain = new StringBuilder();
				while (dbService.DataReaderRead())
				{
					StringBuilder sb = new StringBuilder(string.Format(
						"INSERT INTO {0} VALUES (currval('avg_times_datetime_id_seq'), ", table_name));
					sb.Append(((long)dbService.DataReaderData("record_id")).ToString()).Append(", ");
					for (int i = 2; i < real_columns_count; i++)
					{
						if (dbService.DataReaderData(i, false) is DBNull)
							sb.Append("null,");
						else
						{
							float f = 0;
							try
							{
								f = (float)dbService.DataReaderData(i);
							}
							catch
							{
								object obj = dbService.DataReaderData(i);
								EmService.WriteToLogFailed(string.Format("Error in pg2sql_avg_real_table() {0}, {1}, {2}", 
									table_name, i, obj));
							}
							sb.Append(f.ToString(ci_enUS)).Append(",");
						}
					}
					sb.Remove(sb.Length - 1, 1).Append(");\n");
					sbMain.Append(sb);
				}
				sql += sbMain.ToString();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in pg2sql_avg_real_table():  " + table_name);
				return false;
			}
		}

		#endregion

		#region Building SQL query for inserting DNS data from Database

		private bool pg2sql_dns_period(ref DbService dbService, ref EmSqlDataNode[] partNodes, Int64 regId)
		{
			DbService dbService2 = new DbService(string.Empty);
			try
			{
				dbService2.CopyConnection(ref dbService);

				string commandText = string.Format(
					"SELECT count(*) FROM dns_times WHERE registration_id = {0};", regId);
				int num = (int)((long)dbService.ExecuteScalar(commandText));
				partNodes = new EmSqlDataNode[num];

				commandText = string.Format(
					"SELECT * FROM dns_times WHERE registration_id = {0};", regId);

				dbService.ExecuteReader(commandText);

				int index = 0;
				while (dbService.DataReaderRead())
				{
					partNodes[index] = new EmSqlDataNode();
					long datetime_id = (long)dbService.DataReaderData("datetime_id");
					partNodes[index].SqlType = EmSqlDataNodeType.Events;
					partNodes[index].Begin = (DateTime)dbService.DataReaderData("start_datetime");
					partNodes[index].End = (DateTime)dbService.DataReaderData("end_datetime");

					partNodes[index].Sql += string.Format("INSERT INTO dns_times (datetime_id, start_datetime, end_datetime, registration_id, device_id, parent_folder_id) VALUES (DEFAULT, '{0}', '{1}', {2}, {3}, {4});\n",
						partNodes[index].Begin.ToString("MM.dd.yyyy HH:mm:ss"),
						partNodes[index].End.ToString("MM.dd.yyyy HH:mm:ss"),
						"{0}", "{1}", "{2}");

					string commandText2 = string.Format("SELECT * FROM dns_events WHERE datetime_id = {0};", datetime_id);
					dbService2.ExecuteReader(commandText2);

					StringBuilder sbMain = new StringBuilder();
					while (dbService2.DataReaderRead())
					{
						sbMain.Append(string.Format("INSERT INTO dns_events (datetime_id, event_id, event_type, phase, u_value, d_u, u_declared, dt_start, dt_end, total_seconds, duration_millisec, duration_days, duration_hours, duration_min, duration_sec, phase_num, is_finished, is_earlier) VALUES (currval('dns_times_datetime_id_seq'), {0}, {1}, '{2}', {3}, {4}, {5}, '{6}', '{7}', {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16});\n",
							(long)dbService2.DataReaderData("event_id"),
							(short)dbService2.DataReaderData("event_type"),
							(string)dbService2.DataReaderData("phase"),
							((float)dbService2.DataReaderData("u_value")).ToString(new CultureInfo("en-US")),
							((float)dbService2.DataReaderData("d_u")).ToString(new CultureInfo("en-US")),
							((float)dbService2.DataReaderData("u_declared")).ToString(new CultureInfo("en-US")),
							((DateTime)dbService2.DataReaderData("dt_start")).ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
							((DateTime)dbService2.DataReaderData("dt_end")).ToString("MM.dd.yyyy HH:mm:ss.FFFFF"),
							(long)dbService2.DataReaderData("total_seconds"),
							(int)dbService2.DataReaderData("duration_millisec"),
							(short)dbService2.DataReaderData("duration_days"),
							(short)dbService2.DataReaderData("duration_hours"),
							(short)dbService2.DataReaderData("duration_min"),
							(short)dbService2.DataReaderData("duration_sec"),
							(short)dbService2.DataReaderData("phase_num"),
							((bool)dbService2.DataReaderData("is_finished")).ToString(),
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
			finally
			{
				dbService2.CloseReader();
			}
			return true;
		}

		#endregion

		#endregion
	}
}
