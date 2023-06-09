using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Resources;

using EmDataSaver.SavingInterface;
using DbServiceLib;
using EmDataSaver.SqlImage;
using EmServiceLib;
using EmArchiveTree;

namespace EnergomonitoringXP.ArchiveTreeView
{
	class ArchiveTreeViewETPQP_A : ArchiveTreeViewBase
	{
		#region Constructors

		internal ArchiveTreeViewETPQP_A(int pgServerIndex, EmTreeNodeBase pgServerNode, string connectString)
			: base(pgServerIndex, pgServerNode, connectString)
		{
		}

		#endregion

		#region Internal Methods

		internal override bool ConnectServerAndLoadData(bool DBwasUpdated)
		{
			DbService dbService = null;
			try
			{
				string commandText = string.Empty;
				dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				// temporary node (reference)
				TreeNode treenode = pgServerNode_;

				// проверяем есть ли папка устройства (ETPQP-A)
				TreeNode tnDevNode = null;
				foreach (EmTreeNodeDeviceFolder tn in treenode.Nodes)
				{
					if (tn.Text == "ETPQP-A")
					{
						tnDevNode = tn;
						break;
					}
				}
				if (tnDevNode == null)	// если папки нет, создаем ее
				{
					tnDevNode = new EmTreeNodeDeviceFolder(EmDeviceType.ETPQP_A);
					treenode.Nodes.Add(tnDevNode);
				}

				if (!DBwasUpdated)
				{
					EmService.WriteToLogGeneral("Update DB ETPQP-A");

					#region Update DB

					try
					{
						commandText = "ALTER TABLE registrations ADD COLUMN marked_on_off boolean DEFAULT true;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while adding marked_on_off");
					}

					try
					{
						commandText = "ALTER TABLE avg_power ADD COLUMN tangent_p real;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while adding tangent_p");
					}

					try
					{
						commandText =
							"ALTER TABLE pqp_dip_swell ADD COLUMN num_over_180 integer DEFAULT 0;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while adding num_over_180");
					}

					try
					{
						commandText =
							"ALTER TABLE registrations ADD COLUMN sets_u_deviation_down95 integer DEFAULT 0;";
						commandText +=
							"ALTER TABLE registrations ADD COLUMN sets_u_deviation_down100 integer DEFAULT 10;";
						commandText +=
							"ALTER TABLE pqp_du ADD COLUMN real_nrm_rng_bottom real DEFAULT 0;";
						commandText +=
							"ALTER TABLE pqp_du ADD COLUMN real_max_rng_bottom real DEFAULT 10;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while adding sets_u_deviation_down95");
					}

					try
					{
						commandText = string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3053, "110<u≤120%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3054, "120<u≤140%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3055, "140<u≤160%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3056, "160<u≤180%");

						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3057, "90>u≥85%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3058, "85>u≥70%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3059, "70>u≥40%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3060, "40>u≥10%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3061, "10>u≥0%");
						commandText += string.Format(
							"INSERT INTO parameters(param_id, name) VALUES ({0}, '{1}');", 3062, "5>u≥0%");
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while adding new dip parameters");
					}

					try
					{
						commandText =
							"ALTER TABLE registrations ADD COLUMN gps_latitude double precision;";
						commandText +=
							"ALTER TABLE registrations ADD COLUMN gps_longitude double precision;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while adding gps columns");
					}

					try
					{
						commandText =
							@"CREATE TABLE pqp_flicker_val(
							  record_id bigserial NOT NULL,
							  datetime_id bigint NOT NULL,
							  flik_time timestamp without time zone NOT NULL,
							  flik_a real, flik_a_long real,
							  flik_b real, flik_b_long real,
							  flik_c real, flik_c_long real,
					          flik_short_seconds integer,
                              flik_lond_seconds integer,
                              flik_short_marked smallint, flik_long_marked smallint,
							  CONSTRAINT pqp_flicker_val_pkey PRIMARY KEY (record_id),
							  CONSTRAINT pqp_flicker_val_dt_fkey FOREIGN KEY (datetime_id)
								  REFERENCES pqp_times (datetime_id) MATCH SIMPLE
								  ON UPDATE RESTRICT ON DELETE CASCADE
							) WITH ( OIDS=TRUE );
							ALTER TABLE pqp_flicker_val OWNER TO energomonitor;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while creating pqp_flicker_val");
					}

					try
					{
						commandText =
							@"CREATE TABLE pqp_df_val(
							  record_id bigserial NOT NULL,
							  datetime_id bigint NOT NULL,
							  event_datetime timestamp without time zone NOT NULL,
							  d_f real,
							  f_seconds integer,
							  CONSTRAINT pqp_df_val_pkey PRIMARY KEY (record_id),
							  CONSTRAINT pqp_df_val_dt_fkey FOREIGN KEY (datetime_id)
								  REFERENCES pqp_times (datetime_id) MATCH SIMPLE
								  ON UPDATE RESTRICT ON DELETE CASCADE)
							WITH (OIDS=TRUE);
							ALTER TABLE pqp_df_val OWNER TO energomonitor;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while creating pqp_df_val");
					}

					try
					{
						commandText =
							@"CREATE TABLE pqp_du_val(
							  record_id bigserial NOT NULL,
							  datetime_id bigint NOT NULL,
							  event_datetime timestamp without time zone NOT NULL,
							  u_a_ab_pos real,
							  u_b_bc_pos real,
							  u_c_ca_pos real,
							  u_a_ab_neg real,
							  u_b_bc_neg real,
							  u_c_ca_neg real,
							  record_marked smallint,
							  record_seconds integer,
							  CONSTRAINT pqp_du_val_pkey PRIMARY KEY (record_id),
							  CONSTRAINT pqp_du_val_dt_fkey FOREIGN KEY (datetime_id)
								  REFERENCES pqp_times (datetime_id) MATCH SIMPLE
								  ON UPDATE RESTRICT ON DELETE CASCADE)
							WITH (OIDS=TRUE);
							ALTER TABLE pqp_du_val OWNER TO energomonitor;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while creating pqp_du_val");
					}

					try
					{
						commandText =
							@"CREATE TABLE pqp_interharm_u(
  datetime_id bigint NOT NULL,
  param_id smallint,
  param_num smallint NOT NULL,
  archive_id bigint,
  num_all integer,
  num_marked integer,
  num_not_marked integer,
  val_ph1 real, 
  val_ph2 real,
  val_ph3 real,
  valid_interharm smallint DEFAULT 0,
  CONSTRAINT pqp_interharm_u_pkey PRIMARY KEY (datetime_id, param_num),
  CONSTRAINT pqp_interharm_u_dt_fkey FOREIGN KEY (datetime_id)
      REFERENCES pqp_times (datetime_id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE CASCADE)
WITH (OIDS=TRUE);
ALTER TABLE pqp_interharm_u OWNER TO energomonitor;
COMMENT ON TABLE pqp_interharm_u IS 'Интергармоники напряжения';
COMMENT ON COLUMN pqp_interharm_u.archive_id IS 'Идентификатор архива (уникальный в приборе)';
COMMENT ON COLUMN pqp_interharm_u.num_all IS 'Общее кол-во отсчетов';
COMMENT ON COLUMN pqp_interharm_u.num_marked IS 'Количество маркированных отсчетов';
COMMENT ON COLUMN pqp_interharm_u.num_not_marked IS 'Количество не маркированных отсчетов';
COMMENT ON COLUMN pqp_interharm_u.val_ph1 IS 'МАКСИМАЛЬНЫЕ действующие значения интергармоник A/AB порядков 0…40';
COMMENT ON COLUMN pqp_interharm_u.valid_interharm IS '0 – статистика НЕ вычислялась, 1 - статистика вычислялась';";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP_A: Error while creating pqp_interharm_u");
					}

					#endregion
				}

				AddFoldersLevelToTree(0, (tnDevNode as EmArchNodeBase), connectString_);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("database \"etpqp_a_db\" does not exist"))
				{
					EmService.WriteToLogFailed(ex.Message);
					throw new EmNoDatabaseException();
				}
				else
				{
					EmService.DumpException(ex, "ConnectServerAndLoadData failed 2");
					throw;
				}
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		internal override bool DeleteFolder(ref EmTreeNodeBase contextNode)
		{
			try
			{
				if (!(contextNode is EmArchNodeBase)) return false;

				DbService dbService = new DbService(connectString_);
				string commandText = string.Empty;
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				try
				{
					EmTreeNodeRegistration parentReg = 
						(contextNode as EmArchNodeBase).ParentObject as EmTreeNodeRegistration;

					switch (contextNode.NodeType)
					{
						// registrations
						case EmTreeNodeType.Registration:
							commandText = String.Format(
								"DELETE FROM registrations WHERE reg_id = {0} AND device_id = {1};",
								(contextNode as EmTreeNodeRegistration).RegistrationId,
								(contextNode as EmTreeNodeRegistration).DeviceID);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;

						// measure groups
						case EmTreeNodeType.MeasureGroup:
							switch ((contextNode as EmTreeNodeDBMeasureType).MeasureType)
							{
								case MeasureType.PQP:
									commandText = String.Format(
									"DELETE FROM pqp_times WHERE registration_id = {0} AND device_id = {1};",
									parentReg.RegistrationId,
									parentReg.DeviceID);
									break;
								case MeasureType.AVG:
								    commandText = String.Format(
								    "DELETE FROM avg_times WHERE registration_id = {0} AND device_id = {1};",
								    parentReg.RegistrationId,
									parentReg.DeviceID);
								    break;
								case MeasureType.DNS:
								    commandText = String.Format(
								    "DELETE FROM dns_times WHERE registration_id = {0} AND device_id = {1};",
								    parentReg.RegistrationId,
									parentReg.DeviceID);
								    break;
							}
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;

						// measures
						case EmTreeNodeType.Measure:
							EmTreeNodeDBMeasureType parent;
							if (contextNode.Parent is EmTreeNodeDBMeasureType) // for PQP, DNS
								parent = (EmTreeNodeDBMeasureType)contextNode.Parent;
							else parent = (EmTreeNodeDBMeasureType)contextNode.Parent.Parent;  // for AVG
							switch (parent.MeasureType)
							{
								case MeasureType.PQP:
									commandText = String.Format(
									"DELETE FROM pqp_times WHERE datetime_id = {0};",
									(contextNode as EmTreeNodeDBMeasureEtPQP_A).Id);
									break;
								case MeasureType.AVG:
								    commandText = String.Format(
								    "DELETE FROM avg_times WHERE datetime_id = {0};",
									(contextNode as EmTreeNodeDBMeasureEtPQP_A).Id);
								    break;
								case MeasureType.DNS:
								    commandText = String.Format(
								    "DELETE FROM dns_times WHERE datetime_id = {0};",
									(contextNode as EmTreeNodeDBMeasureEtPQP_A).Id);
								    break;
							}
							dbService.ExecuteNonQuery(commandText, true);
							this.CascadeDeleteEmptyFolders(ref contextNode);
							break;

						// folders
						case EmTreeNodeType.Folder:
						case EmTreeNodeType.FolderInDevice:
							commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(contextNode as EmTreeNodeFolder).FolderId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;

						// EtPQP-A device
						case EmTreeNodeType.ETPQP_A_Device:
							commandText = String.Format(
								"DELETE FROM devices WHERE dev_id = {0};",
								(contextNode as EmTreeNodeEtPQP_A_Device).DeviceId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;
					}
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in DeleteFolder() 3: ");
					throw;
				}
				finally
				{
					if (dbService != null) dbService.CloseConnect();
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeleteFolder(): ");
				throw;
			}
		}

		/// <summary>Adding new subfolder</summary>
		internal override bool CreateNewFolder(ref EmTreeNodeBase contextNode, ref TreeNode selectedNode)
		{
			try
			{
				if (!(contextNode is EmArchNodeBase)) return false;

				if (contextNode.NodeType != EmTreeNodeType.DeviceFolder &&
					contextNode.NodeType != EmTreeNodeType.Folder &&
					contextNode.NodeType != EmTreeNodeType.ETPQPDevice &&
					contextNode.NodeType != EmTreeNodeType.FolderInDevice &&
					contextNode.NodeType != EmTreeNodeType.ETPQP_A_Device)
					return false;

				string commandText = string.Empty;
				DbService dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}
				try
				{
					frmAddNewFolder wndAddNewFolder = new frmAddNewFolder();
					if (wndAddNewFolder.ShowDialog() != DialogResult.OK) return true;

					// папки, внешние по отношению к устройству
					if (contextNode.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, registration_id) VALUES (DEFAULT, '{0}', true, {1}, 0, {2}, 0, {3}, 0);",
							wndAddNewFolder.FolderName,
							(contextNode as EmTreeNodeFolder).FolderId,
							wndAddNewFolder.FolderInfo == string.Empty ?
										"null" : "'" + wndAddNewFolder.FolderInfo + "'",
							(contextNode as EmTreeNodeFolder).DeviceId);
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.Folder,
											EmDeviceType.ETPQP_A,
											(contextNode as EmTreeNodeFolder).DeviceId,
											null, null);
							contextNode.Nodes.Add(folderNode);
							
							selectedNode = folderNode;
							contextNode = folderNode;
						}
						return true;
					}
					else if (contextNode.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, registration_id) VALUES (DEFAULT, '{0}', false, 0, 0, {1}, 0, 0, 0);",
							wndAddNewFolder.FolderName,
							wndAddNewFolder.FolderInfo == string.Empty ?
										"null" : "'" + wndAddNewFolder.FolderInfo + "'");
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.Folder,
											(contextNode as EmArchNodeBase).DeviceType, 0,
											null, null);
							contextNode.Nodes.Add(folderNode);
							
							selectedNode = folderNode;
							contextNode = folderNode;
						}
						return true;
					}
					else if (contextNode.NodeType == EmTreeNodeType.ETPQP_A_Device)
					{
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, registration_id) VALUES (DEFAULT, '{0}', true, {1}, 0, {2}, 5, {3}, 0);",
							wndAddNewFolder.FolderName,
							(contextNode as EmTreeNodeEtPQP_A_Device).FolderId,
							wndAddNewFolder.FolderInfo == string.Empty ?
										"null" : "'" + wndAddNewFolder.FolderInfo + "'",
							(contextNode as EmTreeNodeEtPQP_A_Device).DeviceId);
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.FolderInDevice,
											EmDeviceType.ETPQP_A,
											(contextNode as EmTreeNodeEtPQP_A_Device).DeviceId,
											contextNode as EmArchNodeBase, null);
							contextNode.Nodes.Add(folderNode);
						
							selectedNode = folderNode;
							contextNode = folderNode;
						}
						return true;
					}
					else if (contextNode.NodeType == EmTreeNodeType.FolderInDevice)
					{
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, registration_id) VALUES (DEFAULT, '{0}', true, {1}, 0, {2}, 5, {3}, 0);",
							wndAddNewFolder.FolderName,
							(contextNode as EmTreeNodeFolder).FolderId,
							wndAddNewFolder.FolderInfo == string.Empty ?
										"null" : "'" + wndAddNewFolder.FolderInfo + "'",
							(contextNode as EmTreeNodeFolder).DeviceId);
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);
							Int64 dev_id = 
								((contextNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQP_A_Device).DeviceId;

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.FolderInDevice,
											(contextNode as EmArchNodeBase).DeviceType,
											dev_id, (contextNode as EmArchNodeBase).ParentDevice, null);
							contextNode.Nodes.Add(folderNode);
							
							selectedNode = folderNode;
							contextNode = folderNode;
						}
						return true;
					}
				}
				catch (Exception exc)
				{
				    MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				    throw exc;
				}
				finally
				{
					if (dbService != null) dbService.CloseConnect();
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CreateNewFolder() 2: ");
				throw;
			}
		}

		internal override bool ExportArchive(out EmSqlDataNodeType[] parts, ref EmTreeNodeBase contextNode)
		{
			parts = null;

			try
			{
				if (!(contextNode is EmTreeNodeRegistration)) return false;

				List<EmSqlDataNodeType> parts_list = new List<EmSqlDataNodeType>();

				for (int iType = 0; iType < contextNode.Nodes.Count; iType++)
				{
					switch ((contextNode.Nodes[iType] as EmTreeNodeDBMeasureType).MeasureType)
					{
						case MeasureType.PQP:
							parts_list.Add(EmSqlDataNodeType.PQP);
							break;
						case MeasureType.AVG:
							parts_list.Add(EmSqlDataNodeType.AVG);
							break;
						case MeasureType.DNS:
							parts_list.Add(EmSqlDataNodeType.Events);
							break;
					}
				}
				parts = new EmSqlDataNodeType[parts_list.Count];
				parts_list.CopyTo(parts);

				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ExportArchive(): " + ex.Message);
				throw;
			}
		}

		internal override bool ShowArchOptions(ref EmTreeNodeBase contextNode)
		{
			try
			{
				DbService dbService = new DbService(connectString_);
				string commandText = string.Empty;
				frmOptionsFld wndOptionsFld;

				// Options from device
				if (contextNode.NodeType == EmTreeNodeType.ETPQP_A_Device)
				{
					EmTreeNodeEtPQP_A_Device devNode = contextNode as EmTreeNodeEtPQP_A_Device;

					wndOptionsFld = new frmOptionsFld(contextNode.Text, devNode.DeviceInfo,
														EmDeviceType.ETPQP_A);
					DialogResult result = wndOptionsFld.ShowDialog();
					if (result == DialogResult.OK && (devNode.DeviceInfo != wndOptionsFld.FolderInfo))
					{
						if(!dbService.Open())
						{
							MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
							return false;
						}

						try
						{
							commandText = String.Format("UPDATE folders SET folder_info = '{0}' WHERE folder_id = {1};",
								wndOptionsFld.FolderInfo,
								(contextNode as EmTreeNodeEtPQP_A_Device).FolderId);

							int iResult = dbService.ExecuteNonQuery(commandText, true);
							devNode.DeviceInfo = wndOptionsFld.FolderInfo;
						}
						finally
						{
							if (dbService != null) dbService.CloseConnect();
						}
					}
				}

				// Options from folder
				if (contextNode.NodeType == EmTreeNodeType.Folder ||
					contextNode.NodeType == EmTreeNodeType.FolderInDevice)
				{
					EmTreeNodeFolder folderNode = contextNode as EmTreeNodeFolder;

					wndOptionsFld = new frmOptionsFld(contextNode.Text, folderNode.FolderInfo, folderNode.DeviceType);
					DialogResult result = wndOptionsFld.ShowDialog();
					if (result == DialogResult.OK && (folderNode.FolderInfo != wndOptionsFld.FolderInfo))
					{
						if (!dbService.Open())
						{
							MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
							return false;
						}

						try
						{
							commandText = String.Format("UPDATE folders SET folder_info = '{0}' WHERE folder_id = {1};",
								wndOptionsFld.FolderInfo,
								(contextNode as EmTreeNodeFolder).FolderId);

							int iResult = dbService.ExecuteNonQuery(commandText, true);
							folderNode.FolderInfo = wndOptionsFld.FolderInfo;
						}
						finally
						{
							if (dbService != null) dbService.CloseConnect();
						}
					}
				}

				// Options from object
				else if (contextNode.NodeType == EmTreeNodeType.Registration)
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
						this.GetType().Assembly);
					string strConSch = rm.GetString("name_con_scheme_" +
						(contextNode as EmTreeNodeRegistration).ConnectionScheme.ToString() +
						"_full");

					EmTreeNodeEtPQP_A_Device parentDev = 
						(contextNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQP_A_Device;
					if (parentDev == null) throw new EmException("Unable to get parent device!");

					frmOptionsDb wndOptionsDb = new frmOptionsDb(
						contextNode.PgServerIndex,
						connectString_,
						(contextNode as EmTreeNodeRegistration).StartDateTime,
						(contextNode as EmTreeNodeRegistration).EndDateTime,
						strConSch,
						(contextNode as EmTreeNodeRegistration).NominalLinearVoltage,
						(contextNode as EmTreeNodeRegistration).NominalPhaseVoltage,
						(contextNode as EmTreeNodeRegistration).NominalFrequency,
						(contextNode as EmTreeNodeRegistration).ObjectName,
						(contextNode as EmTreeNodeRegistration).ObjectInfo,
						(contextNode as EmTreeNodeRegistration).DeviceID,
						(contextNode as EmTreeNodeRegistration).DeviceType,
						"ETPQP-A",
						(contextNode as EmTreeNodeRegistration).DeviceVersion,
						parentDev.SerialNumber,
						(contextNode as EmTreeNodeRegistration).RegistrationId,
						(contextNode as EmTreeNodeRegistration).U_transformer_enable,
						(contextNode as EmTreeNodeRegistration).U_transformer_type,
						(contextNode as EmTreeNodeRegistration).I_transformer_usage,
						(contextNode as EmTreeNodeRegistration).I_transformer_primary,
						(contextNode as EmTreeNodeRegistration).I_transformer_secondary,
						(contextNode as EmTreeNodeRegistration).GPS_Latitude,
						(contextNode as EmTreeNodeRegistration).GPS_Longitude);
					wndOptionsDb.ShowDialog();
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ShowArchOptions(): ");
				throw;
			}
		}

		internal override bool InsertFolder(ref EmArchNodeBase nodeToPaste, ref EmArchNodeBase nodeDestination)
		{
			if (nodeToPaste == null)
			{
				EmService.WriteToLogFailed("Paste was clicked, but buffer node is null");
				return false;
			}

			if (nodeToPaste.DeviceType != nodeDestination.DeviceType)
				return false;

			if (nodeToPaste == nodeDestination || nodeToPaste.Parent == nodeDestination)
				return true;

			// Avoid that drop node is child of drag node 
			TreeNode tmpNode = nodeDestination;
			while (tmpNode.Parent != null)
			{
				if (tmpNode.Parent == nodeToPaste) return true;
				tmpNode = tmpNode.Parent;
			}

			// applying changes to database
			DbService dbService = new DbService(connectString_);
			if (!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return false;
			}
			try
			{
				string commandText = string.Empty;

				// if buffer node is object
				if (nodeToPaste.NodeType == EmTreeNodeType.Registration)
				{
					if (nodeDestination.NodeType == EmTreeNodeType.FolderInDevice)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0} WHERE folder_type = 2 AND registration_id = {1} AND device_id = {2}",
							(nodeDestination as EmTreeNodeFolder).FolderId,
							(nodeToPaste as EmTreeNodeRegistration).RegistrationId,
							(nodeToPaste as EmTreeNodeRegistration).DeviceID);
					}
					else if (nodeDestination.NodeType == EmTreeNodeType.ETPQP_A_Device)
					{
						if((nodeDestination as EmTreeNodeEtPQP_A_Device).DeviceId == 
							(nodeToPaste as EmTreeNodeRegistration).DeviceID)
							commandText = String.Format("UPDATE folders SET parent_id = {0} WHERE folder_type = 2 AND registration_id = {1} AND device_id = {2}",
							(nodeDestination as EmTreeNodeEtPQP_A_Device).FolderId,
							(nodeToPaste as EmTreeNodeRegistration).RegistrationId,
							(nodeToPaste as EmTreeNodeRegistration).DeviceID);
					}
				}
				// if buffer node is folder
				else if (nodeToPaste.NodeType == EmTreeNodeType.Folder)
				{
					// drop node is subfolder
					if (nodeDestination.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folder_id = {1};",
							((EmTreeNodeFolder)nodeDestination).FolderId,
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}

					// drop node is root
					else if (nodeDestination.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = 0, is_subfolder = false WHERE folder_id = {0};",
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}
				}
				// if buffer node is folder
				else if (nodeToPaste.NodeType == EmTreeNodeType.FolderInDevice)
				{
					// drop node is subfolder
					if (nodeDestination.NodeType == EmTreeNodeType.FolderInDevice)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folder_id = {1};",
							((EmTreeNodeFolder)nodeDestination).FolderId,
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}

					// drop node is device
					else if (nodeDestination.NodeType == EmTreeNodeType.ETPQP_A_Device)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folder_id = {1};",
							((EmTreeNodeEtPQP_A_Device)nodeDestination).FolderId,
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}
				}
				// if buffer node is device
				else if (nodeToPaste.NodeType == EmTreeNodeType.ETPQP_A_Device)
				{
					// drop node is device folder
					if (nodeDestination.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = 0, is_subfolder = FALSE WHERE folder_id = {0};", ((EmTreeNodeEtPQP_A_Device)nodeToPaste).FolderId);
					}

					// drop node is folder
					else if (nodeDestination.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folder_id = {1};",
							((EmTreeNodeFolder)nodeDestination).FolderId,
							((EmTreeNodeEtPQP_A_Device)nodeToPaste).FolderId);
					}
				}

				if (commandText == string.Empty) return false;

				// visual updates
				if (dbService.ExecuteNonQuery(commandText, true) > 0)
				{
					EmTreeNodeBase oldParent = (EmTreeNodeBase)nodeToPaste.Parent;

					// Remove drag node from parent
					oldParent.Nodes.Remove(nodeToPaste);

					// add node to tree
					nodeDestination.Nodes.Add((TreeNode)nodeToPaste.Clone());

					nodeDestination.Expand();
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in InsertFolder(): ");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		#endregion

		#region Protected Methods

		protected bool AddFoldersLevelToTree(Int64 parentFldId, EmArchNodeBase parentNode, 
			string connectString)
		{
			DbService dbService = null;
			try
			{
				dbService = new DbService(connectString);
				dbService.Open();
				// выбираем из БД все вложенные папки (внешние по отношению к устройству)
				string commandText = String.Format("SELECT * FROM folders f WHERE folder_type = 0 AND parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					Int64 fldId = (Int64)dbService.DataReaderData("folder_id");
					string fldName = dbService.DataReaderData("folder_name") as string;
					string fldInfo = string.Empty;
					object oFldInfo = dbService.DataReaderData("folder_info");
					if (!(oFldInfo is System.DBNull)) fldInfo = (string)oFldInfo;
					EmTreeNodeFolder fldNode = new EmTreeNodeFolder(fldId, fldName, fldInfo,
																EmTreeNodeType.Folder,
																EmDeviceType.ETPQP_A,
																null, null);

					// ищем данную папку (по названию)
					EmTreeNodeFolder tnFldNodeTemp = null;
					//for (int iFolder = 0; iFolder < parentNode.Nodes.Count; ++iFolder)
					//{
					//    if (parentNode.Nodes[iFolder] is EmTreeNodeFolder)
					//    {
					//        if ((parentNode.Nodes[iFolder] as EmTreeNodeFolder).Text == fldName)
					//        {
					//            tnFldNodeTemp = (EmTreeNodeFolder)parentNode.Nodes[iFolder];
					//            break;
					//        }
					//    }
					//}
					// если не нашли, то добавляем папку в дерево
					if (tnFldNodeTemp == null)
					{
						parentNode.Nodes.Add(fldNode);
						// рекурсивный вызов для этой папки
						AddFoldersLevelToTree(fldId, fldNode, connectString);
					}
				}

				// выбираем из БД все вложенные устройства
				commandText = String.Format("SELECT * FROM folders f INNER JOIN devices d ON f.device_id = d.dev_id AND d.parent_folder_id = f.folder_id AND f.folder_type = 1 AND f.parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					EmTreeNodeEtPQP_A_Device tnDevNode = new EmTreeNodeEtPQP_A_Device();
					tnDevNode.SerialNumber = (Int64)dbService.DataReaderData("ser_number");
					tnDevNode.DeviceId = (Int64)dbService.DataReaderData("dev_id");
					tnDevNode.DeviceVersion = dbService.DataReaderData("dev_version") as string;
					object oDevInfo = dbService.DataReaderData("folder_info");
					if (oDevInfo is System.DBNull) tnDevNode.DeviceInfo = string.Empty;
					else tnDevNode.DeviceInfo = oDevInfo as string;
					tnDevNode.FolderId = (Int64)dbService.DataReaderData("folder_id");

					tnDevNode.Text = string.Format("ETPQP-A # {0}", tnDevNode.SerialNumber);

					// ищем папку для данного прибора (по серийному номеру)
					EmTreeNodeEtPQP_A_Device tnDevNodeTemp = null;

					for (int iItem = 0; iItem < parentNode.Nodes.Count; ++iItem)
					{
						if (parentNode.Nodes[iItem] is EmTreeNodeEtPQP_A_Device)
						{
							if (tnDevNode.SerialNumber ==
								(parentNode.Nodes[iItem] as EmTreeNodeEtPQP_A_Device).SerialNumber)
							{
								tnDevNodeTemp = (EmTreeNodeEtPQP_A_Device)parentNode.Nodes[iItem];
								break;
							}
						}
					}

					// добавляем папку прибора в дерево
					if (tnDevNodeTemp == null)
					{
						parentNode.Nodes.Add(tnDevNode);
						AddFoldersLevelToTree(tnDevNode.FolderId, tnDevNode, connectString);
					}
				}

				// выбираем из БД все вложенные в устройство папки
				commandText = String.Format("SELECT * FROM folders f WHERE folder_type = 5 AND parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					Int64 fldId = (Int64)dbService.DataReaderData("folder_id");
					string fldName = dbService.DataReaderData("folder_name") as string;
					string fldInfo = string.Empty;
					object oFldInfo = dbService.DataReaderData("folder_info");
					if (!(oFldInfo is System.DBNull)) fldInfo = (string)oFldInfo;

					EmTreeNodeEtPQP_A_Device parentDev = null;
					if (parentNode is EmTreeNodeEtPQP_A_Device) parentDev = parentNode as EmTreeNodeEtPQP_A_Device;
					else if (parentNode is EmTreeNodeFolder)
						parentDev = (parentNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQP_A_Device;
					if (parentDev == null)
						throw new EmException("AddFoldersLevelToTree(): Unable to get EtPQP parent device!");

					EmTreeNodeFolder fldNode = new EmTreeNodeFolder(fldId, fldName, fldInfo,
																EmTreeNodeType.FolderInDevice,
																EmDeviceType.ETPQP_A, parentDev, null);

					// ищем данную папку (по названию)
					EmTreeNodeFolder tnFldNodeTemp = null;
					//for (int iFolder = 0; iFolder < parentNode.Nodes.Count; ++iFolder)
					//{
					//    if (parentNode.Nodes[iFolder] is EmTreeNodeFolder)
					//    {
					//        if ((parentNode.Nodes[iFolder] as EmTreeNodeFolder).Text == fldName)
					//        {
					//            tnFldNodeTemp = (EmTreeNodeFolder)parentNode.Nodes[iFolder];
					//            break;
					//        }
					//    }
					//}
					// если не нашли, то добавляем папку в дерево
					if (tnFldNodeTemp == null)
					{
						parentNode.Nodes.Add(fldNode);
						// рекурсивный вызов для этой папки
						AddFoldersLevelToTree(fldId, fldNode, connectString);
					}
				}

				// выбираем из БД все вложенные объекты (для них рекурсивно не вызываем, т.к. у них
				// не может быть вложенных папок)
				commandText = String.Format("SELECT * FROM (SELECT reg.* FROM folders f INNER JOIN registrations reg ON f.folder_id = reg.parent_folder_id AND f.registration_id = reg.reg_id AND f.folder_type = 2 AND f.parent_id = {0}) ff INNER JOIN devices dev ON ff.device_id = dev.dev_id;", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					EmTreeNodeEtPQP_A_Device parentDev = null;
					if (parentNode is EmTreeNodeEtPQP_A_Device) parentDev = parentNode as EmTreeNodeEtPQP_A_Device;
					else if (parentNode is EmTreeNodeFolder)
						parentDev = (parentNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQP_A_Device;
					if (parentDev == null)
						throw new EmException("AddFoldersLevelToTree(): Unable to get EtPQP-A parent device!");

					EmTreeNodeRegistration regNode = new EmTreeNodeRegistration(parentDev, EmDeviceType.ETPQP_A);
					regNode.RegistrationId = (Int64)dbService.DataReaderData("reg_id");
					regNode.StartDateTime = (DateTime)dbService.DataReaderData("start_datetime");
					regNode.EndDateTime = (DateTime)dbService.DataReaderData("end_datetime");
					regNode.ConnectionScheme = (ConnectScheme)EmService.GetShortDBValue(dbService.DataReaderData("con_scheme"));
					regNode.NominalLinearVoltage = EmService.GetFloatDBValue(dbService.DataReaderData("u_nom_lin"));
					regNode.NominalPhaseVoltage = EmService.GetFloatDBValue(dbService.DataReaderData("u_nom_ph"));
					regNode.NominalPhaseCurrent = EmService.GetFloatDBValue(dbService.DataReaderData("i_nom_ph"));
					regNode.NominalFrequency = EmService.GetFloatDBValue(dbService.DataReaderData("f_nom"));
					regNode.DeviceID = (Int64)dbService.DataReaderData("device_id");
					regNode.DeviceSerNumber = (Int64)dbService.DataReaderData("ser_number");
					regNode.DeviceType = EmDeviceType.ETPQP_A;
					regNode.DeviceVersion = EmService.GetStringDBValue(dbService.DataReaderData("device_version"));
					regNode.ObjectName = EmService.GetStringDBValue(dbService.DataReaderData("obj_name"));
					regNode.ObjectInfo = EmService.GetStringDBValue(dbService.DataReaderData("obj_info"));
					regNode.ULimit = EmService.GetFloatDBValue(dbService.DataReaderData("u_limit"));
					regNode.ILimit = EmService.GetFloatDBValue(dbService.DataReaderData("i_limit"));
					regNode.ConstraintType = EmService.GetShortDBValue(dbService.DataReaderData("constraint_type"));
					//regNode.T_Fliker = EmService.GetShortDBValue(dbService.DataReaderData("t_fliker");   // fliker period
					regNode.FolderId = (Int64)dbService.DataReaderData("parent_folder_id");

					regNode.GPS_Latitude = EmService.GetDoubleDBValue(dbService.DataReaderData("gps_latitude"), false);
					regNode.GPS_Longitude = EmService.GetDoubleDBValue(dbService.DataReaderData("gps_longitude"), false);
					regNode.Autocorrect_time_gps_enable = EmService.GetBoolDBValue(
						dbService.DataReaderData("autocorrect_time_gps_enable"));

					regNode.U_transformer_enable = EmService.GetBoolDBValue(dbService.DataReaderData("u_transformer_enable"));
					regNode.I_transformer_usage = EmService.GetShortDBValue(dbService.DataReaderData("i_transformer_usage"));
					regNode.U_transformer_type = EmService.GetShortDBValue(dbService.DataReaderData("u_transformer_type"));
					regNode.I_transformer_primary = EmService.GetIntDBValue(dbService.DataReaderData("i_transformer_primary"));

					if (regNode.I_transformer_usage == 1)
						regNode.I_transformer_secondary = 1;
					else if (regNode.I_transformer_usage == 2)
						regNode.I_transformer_secondary = 5;

					regNode.Text = string.Format("{0} # {1} - {2}", regNode.ObjectName,
						regNode.StartDateTime.ToString(), regNode.EndDateTime.ToString());

					// ищем папку для данного объекта (по id)
					EmTreeNodeRegistration tnRegNodeTemp = null;

					for (int iItem = 0; iItem < parentNode.Nodes.Count; ++iItem)
					{
						if (parentNode.Nodes[iItem] is EmTreeNodeRegistration)
						{
							if (regNode.RegistrationId ==
								(parentNode.Nodes[iItem] as EmTreeNodeRegistration).RegistrationId)
							{
								tnRegNodeTemp = (EmTreeNodeRegistration)parentNode.Nodes[iItem];
								break;
							}
						}
					}

					// добавляем папку объекта в дерево
					if (tnRegNodeTemp == null)
					{
						parentNode.Nodes.Add(regNode);
						// добавляем архивы в объект
						AddArchivesToRegistrationFolder(regNode);
					}
				}

				pgServerNode_.Expand();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "AddFoldersLevelToTree EtPQP-A failed");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Function to add archives to the object
		/// </summary>
		protected void AddArchivesToRegistrationFolder(EmTreeNodeRegistration tnRegNode)
		{
			try
			{
				if (tnRegNode == null) return;

				DbService dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}
				try
				{
					#region PQP

					// trying to find PQP
					string commandText = String.Format("SELECT count(*) AS count FROM pqp_times WHERE registration_id = {0} AND device_id = {1};", tnRegNode.RegistrationId, tnRegNode.DeviceID);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM pqp_times WHERE registration_id = {0} AND device_id = {1};", tnRegNode.RegistrationId, tnRegNode.DeviceID);
						dbService.ExecuteReader(commandText);

						PQPDatesList pqpList = new PQPDatesList();

						while (dbService.DataReaderRead())
						{
							pqpList.Add(new EmInfoForNodePQP_EtPQP_A(ref dbService, EmDeviceType.ETPQP_A));
						}
						pqpList.SortItems();

						for (int iPqp = 0; iPqp < pqpList.Count; ++iPqp)
						{
							// ищем папку для ПКЭ
							EmTreeNodeDBMeasureType tnPQPNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.ETPQP_A);
							foreach (EmTreeNodeDBMeasureType tn in tnRegNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this,
									EmDeviceType.ETPQP_A))
								{
									tnPQPNode = tn;
									break;
								}
							}
							if (tnPQPNode == null)	// если папки нет, создаем ее
							{
								tnPQPNode = new EmTreeNodeDBMeasureType(MeasureType.PQP,
									EmDeviceType.ETPQP_A, tnRegNode.ParentDevice, tnRegNode);
								tnRegNode.Nodes.Add(tnPQPNode);
							}

							// ищем папку самого архива
							EmTreeNodeDBMeasureEtPQP_A tnPQPArchNode = null;
							foreach (EmTreeNodeDBMeasureEtPQP_A tn in tnPQPNode.Nodes)
							{
								if (tn.Id == pqpList[iPqp].DatetimeId)
								{
									tnPQPArchNode = tn;
									break;
								}
							}
							if (tnPQPArchNode == null)	// если папки арихва нет, создаем ее
							{
								tnPQPArchNode = new EmTreeNodeDBMeasureEtPQP_A(pqpList[iPqp].DatetimeId,
									pqpList[iPqp].DtStart,
									pqpList[iPqp].DtEnd,
									tnRegNode.DeviceVersion, 
									tnRegNode.T_Fliker,
									EmDeviceType.ETPQP_A,
									tnRegNode.ConstraintType,
									tnRegNode.ParentDevice, tnRegNode);
								tnPQPNode.Nodes.Add(tnPQPArchNode);
							}
						}
					}

					#endregion

					#region DNS

					// trying to find DNS
					commandText =
						String.Format("SELECT count(*) AS count FROM dns_times WHERE registration_id = {0} AND device_id = {1};", tnRegNode.RegistrationId, tnRegNode.DeviceID);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM dns_times WHERE registration_id = {0} AND device_id = {1};", tnRegNode.RegistrationId, tnRegNode.DeviceID);
						dbService.ExecuteReader(commandText);

						DNSDatesList dnsList = new DNSDatesList();

						while (dbService.DataReaderRead())
						{
							dnsList.Add(new EmInfoForNodeDNS_EtPQP_A(ref dbService, EmDeviceType.ETPQP_A));
						}
						dnsList.SortItems();

						for (int iDns = 0; iDns < dnsList.Count; ++iDns)
						{
							// добавляем в дерево архивов
							// ищем папку для DNS
							EmTreeNodeDBMeasureType tnDNSNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.ETPQP_A);
							foreach (EmTreeNodeDBMeasureType tn in tnRegNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this,
									EmDeviceType.ETPQP_A))
								{
									tnDNSNode = tn;
									break;
								}
							}
							if (tnDNSNode == null)	// если папки нет, создаем ее
							{
								tnDNSNode = new EmTreeNodeDBMeasureType(MeasureType.DNS,
												EmDeviceType.ETPQP_A, tnRegNode.ParentDevice, tnRegNode);
								tnRegNode.Nodes.Add(tnDNSNode);
							}

							// ищем папку самого архива
							EmTreeNodeDBMeasureEtPQP_A tnDNSArchNode = null;
							foreach (EmTreeNodeDBMeasureEtPQP_A tn in tnDNSNode.Nodes)
							{
								if (tn.Id == dnsList[iDns].DatetimeId)
								{
									tnDNSArchNode = tn;
									break;
								}
							}
							if (tnDNSArchNode == null)	// если папки арихва нет, создаем ее
							{
								tnDNSArchNode = new EmTreeNodeDBMeasureEtPQP_A(dnsList[iDns].DatetimeId,
									dnsList[iDns].DtStart,
									dnsList[iDns].DtEnd,
									tnRegNode.DeviceVersion, tnRegNode.T_Fliker,
									EmDeviceType.ETPQP_A,
									tnRegNode.ConstraintType, tnRegNode.ParentDevice, tnRegNode);
								tnDNSNode.Nodes.Add(tnDNSArchNode);
							}
						}
					}

					#endregion

					#region AVG

					// trying to find AVG
					commandText =
						String.Format("SELECT count(*) AS count FROM avg_times WHERE registration_id = {0} AND device_id = {1};", tnRegNode.RegistrationId, tnRegNode.DeviceID);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM avg_times WHERE registration_id = {0} AND device_id = {1};", tnRegNode.RegistrationId, tnRegNode.DeviceID);
						dbService.ExecuteReader(commandText);

						AVGDatesList avgList = new AVGDatesList();

						while (dbService.DataReaderRead())
						{
							avgList.Add(new EmInfoForNodeAVG_EtPQP_A(ref dbService, EmDeviceType.ETPQP_A));
						}
						avgList.SortItems();

						for (int iAvg = 0; iAvg < avgList.Count; ++iAvg)
						{
							// ищем папку для AVG
							EmTreeNodeDBMeasureType tnAVGNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.ETPQP_A);
							foreach (EmTreeNodeDBMeasureType tn in tnRegNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this,
									EmDeviceType.ETPQP_A))
								{
									tnAVGNode = tn;
									break;
								}
							}
							if (tnAVGNode == null)	// если папки нет, создаем ее
							{
								tnAVGNode = new EmTreeNodeDBMeasureType(MeasureType.AVG,
												EmDeviceType.ETPQP_A, tnRegNode.ParentDevice, tnRegNode);
								tnRegNode.Nodes.Add(tnAVGNode);
							}

							// ищем папку для типа усреденения
							TreeNode tnAvgTypeNode = null;
							for (int iAvgType = 0; iAvgType < tnAVGNode.Nodes.Count; ++iAvgType)
							{
								if (tnAVGNode.Nodes[iAvgType] is EmTreeNodeAvgTypeEtPQP_A)
								{
									AvgTypes_PQP_A curType = 
										(tnAVGNode.Nodes[iAvgType] as EmTreeNodeAvgTypeEtPQP_A).AvgType;
									if (curType == (avgList[iAvg] as EmInfoForNodeAVG_EtPQP_A).AvgType)
									{
										tnAvgTypeNode = tnAVGNode.Nodes[iAvgType];
										break;
									}
								}
							}
							if (tnAvgTypeNode == null)	// если папки нет, создаем ее
							{
								tnAvgTypeNode = new EmTreeNodeAvgTypeEtPQP_A(
											(avgList[iAvg] as EmInfoForNodeAVG_EtPQP_A).AvgType,
											EmDeviceType.ETPQP_A, tnRegNode.ParentDevice, tnRegNode);
								tnAVGNode.Nodes.Add(tnAvgTypeNode);
							}

							// ищем папку самого архива
							EmTreeNodeDBMeasureEtPQP_A tnAVGArchNode = null;
							foreach (EmTreeNodeDBMeasureEtPQP_A tn in tnAvgTypeNode.Nodes)
							{
								if (tn.Id == avgList[iAvg].DatetimeId)
								{
									tnAVGArchNode = tn;
									break;
								}
							}
							if (tnAVGArchNode == null)	// если папки арихва нет, создаем ее
							{
								tnAVGArchNode = new EmTreeNodeDBMeasureEtPQP_A(avgList[iAvg].DatetimeId,
									avgList[iAvg].DtStart,
									avgList[iAvg].DtEnd,
									tnRegNode.DeviceVersion,
									tnRegNode.T_Fliker,
									EmDeviceType.ETPQP_A,
									(avgList[iAvg] as EmInfoForNodeAVG_EtPQP_A).AvgType,
									tnRegNode.ConstraintType,
									tnRegNode.ParentDevice, tnRegNode);

								tnAvgTypeNode.Nodes.Add(tnAVGArchNode);
							}
						}
					}

					#endregion
				}
				finally
				{
					if (dbService != null) dbService.CloseConnect();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddArchivesToRegFolder():");
				throw;
			}
		}

		protected void CascadeDeleteEmptyFolders(ref EmTreeNodeBase contextNode)
		{
			try
			{
				EmTreeNodeBase nodeToRemove = contextNode;
				EmTreeNodeBase parentNode = (EmTreeNodeBase)contextNode.Parent;

				if (nodeToRemove is EmTreeNodeDBMeasureEtPQP_A)  // сам архив
				{
					nodeToRemove.Remove();
				}
				else if (nodeToRemove is EmTreeNodeAvgTypeEtPQP_A)
				{
					nodeToRemove.Remove();
				}
				else if (nodeToRemove is EmTreeNodeDBMeasureType)
				{
					nodeToRemove.Remove();
				}

				if (parentNode.Nodes.Count == 0 && !(parentNode is EmTreeNodeEtPQP_A_Device)
					&& !(parentNode is EmTreeNodeFolder) && !(parentNode is EmTreeNodeServer))
				{
					CascadeDeleteEmptyFolders(ref parentNode);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CascadeDeleteEmptyFolders(): ");
				throw;
			}
		}

		#endregion
	}
}
