using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Resources;

using EmDataSaver.SqlImage;
using EmDataSaver.SavingInterface;
using EmServiceLib;
using EmArchiveTree;
using DbServiceLib;

namespace EnergomonitoringXP.ArchiveTreeView
{
	class ArchiveTreeViewEM33T : ArchiveTreeViewBase
	{
		#region Constructors

		internal ArchiveTreeViewEM33T(int pgServerIndex, EmTreeNodeBase pgServerNode, string connectString)
			: base(pgServerIndex, pgServerNode, connectString)
		{
		}

		#endregion

		#region Internal Methods

		internal override bool ConnectServerAndLoadData(bool DBwasUpdated)
		{
			DbService dbService = null;
			string commandText = string.Empty;
			try
			{
				// temporary node (reference)
				TreeNode treenode = pgServerNode_;

				// проверяем есть ли папка устройства (EM31 или EM33)
				TreeNode tnDevNode = null;
				foreach (EmTreeNodeDeviceFolder tn in treenode.Nodes)
				{
					if (tn.Text == "EM33T")
					{
						tnDevNode = tn;
						break;
					}
				}
				if (tnDevNode == null)	// если папки нет, создаем ее
				{
					tnDevNode = new EmTreeNodeDeviceFolder(EmDeviceType.EM33T);
					treenode.Nodes.Add(tnDevNode);
				}

				dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				if (!DBwasUpdated)
				{
					EmService.WriteToLogGeneral("Update DB EM-33T");
					#region Update DB

					// изменения в БД для Em33T
					commandText =
	@"CREATE OR REPLACE FUNCTION db_set_parent_empty()
   RETURNS trigger AS
   $BODY$
   DECLARE
   BEGIN  
	 DELETE FROM folders WHERE folder_id = OLD.parent_id;
	 RETURN NULL;
   END;
   $BODY$
   LANGUAGE 'plpgsql' VOLATILE
   COST 100;
   ALTER FUNCTION db_set_parent_empty() OWNER TO energomonitor;";
					dbService.ExecuteNonQuery(commandText, false);

					// add column "tangent_p_geom"
					commandText =
	@"SELECT count(*) 
FROM pg_attribute, pg_class
WHERE pg_class.relname = 'period_avg_params_1_4'
AND pg_class.relfilenode = pg_attribute.attrelid
AND pg_attribute.attnum > 0 AND pg_attribute.attname = 'tangent_p_geom';";
					object oCount = dbService.ExecuteScalar(commandText);
					Int64 iCount = 0;
					if (!(oCount is DBNull)) { iCount = (Int64)oCount; }
					if (iCount <= 0)
					{
						commandText =
							"ALTER TABLE period_avg_params_1_4 ADD COLUMN tangent_p_geom real DEFAULT 0";
						try
						{
							dbService.ExecuteNonQuery(commandText, false);
						}
						catch (Exception nex)
						{
							EmService.WriteToLogFailed("Error while updating Em33T DB: " + nex.Message);
							EmService.WriteToLogFailed("iCount = " + iCount.ToString());
							if (oCount is DBNull) EmService.WriteToLogFailed("oCount is DBNull!");
						}
					}

					// add column "tangent_p_shift"
					commandText =
	@"SELECT count(*) 
FROM pg_attribute, pg_class
WHERE pg_class.relname = 'period_avg_params_1_4'
AND pg_class.relfilenode = pg_attribute.attrelid
AND pg_attribute.attnum > 0 AND pg_attribute.attname = 'tangent_p_shift';";
					oCount = dbService.ExecuteScalar(commandText);
					iCount = 0;
					if (!(oCount is DBNull)) { iCount = (Int64)oCount; }
					if (iCount <= 0)
					{
						commandText =
							"ALTER TABLE period_avg_params_1_4 ADD COLUMN tangent_p_shift real DEFAULT 0";
						try
						{
							dbService.ExecuteNonQuery(commandText, false);
						}
						catch (Exception nex)
						{
							EmService.WriteToLogFailed("Error while updating Em33T DB: " + nex.Message);
							EmService.WriteToLogFailed("iCount = " + iCount.ToString());
							if (oCount is DBNull) EmService.WriteToLogFailed("oCount is DBNull!");
						}
					}

					// add column "tangent_p_cross"
					commandText =
	@"SELECT count(*) 
FROM pg_attribute, pg_class
WHERE pg_class.relname = 'period_avg_params_1_4'
AND pg_class.relfilenode = pg_attribute.attrelid
AND pg_attribute.attnum > 0 AND pg_attribute.attname = 'tangent_p_cross';";
					oCount = dbService.ExecuteScalar(commandText);
					iCount = 0;
					if (!(oCount is DBNull)) { iCount = (Int64)oCount; }
					if (iCount <= 0)
					{
						commandText =
							"ALTER TABLE period_avg_params_1_4 ADD COLUMN tangent_p_cross real DEFAULT 0";
						try
						{
							dbService.ExecuteNonQuery(commandText, false);
						}
						catch (Exception nex)
						{
							EmService.WriteToLogFailed("Error while updating Em33T DB: " + nex.Message);
							EmService.WriteToLogFailed("iCount = " + iCount.ToString());
							if (oCount is DBNull) EmService.WriteToLogFailed("oCount is DBNull!");
						}
					}

					#region rename columns in AVG tables

					try
					{
						commandText =
						@"ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_A"" TO u_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_B"" TO u_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_C"" TO u_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U1_A"" TO u1_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U1_B"" TO u1_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U1_C"" TO u1_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_AB"" TO u_ab;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_BC"" TO u_bc;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_CA"" TO u_ca;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U1_AB"" TO u1_ab;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U1_BC"" TO u1_bc;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U1_CA"" TO u1_ca;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U0_A"" TO u0_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U0_B"" TO u0_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U0_C"" TO u0_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_hp A"" TO u_hp_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_hp B"" TO u_hp_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_hp C"" TO u_hp_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_1"" TO u_1;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_2"" TO u_2;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""U_0"" TO u_0;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I_A"" TO i_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I_B"" TO i_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I_C"" TO i_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I1_A"" TO i1_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I1_B"" TO i1_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I1_C"" TO i1_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I_1"" TO i_1;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I_2"" TO i_2;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""I_0"" TO i_0;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_Σ"" TO p_sum;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_A(1)"" TO p_a_1;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_B(2)"" TO p_b_2;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_C"" TO p_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""S_Σ"" TO s_sum;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""S_A"" TO s_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""S_B"" TO s_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""S_C"" TO s_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_Σ geom"" TO q_sum_geom;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_A geom"" TO q_a_geom;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_B geom"" TO q_b_geom;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_C geom"" TO q_c_geom;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_Σ shift"" TO q_sum_shift;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_A shift"" TO q_a_shift;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_B shift"" TO q_b_shift;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_C shift"" TO q_c_shift;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_Σ cross"" TO q_sum_cross;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_A cross"" TO q_a_cross;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_B cross"" TO q_b_cross;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_C cross"" TO q_c_cross;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Kp_Σ"" TO kp_sum;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Kp_A"" TO kp_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Kp_B"" TO kp_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Kp_C"" TO kp_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U1_A_ U1_B"" TO an_u1_a_u1_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U1_B_ U1_C"" TO an_u1_b_u1_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U1_C_ U1_A"" TO an_u1_c_u1_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U1_A_ I1_A"" TO an_u1_a_i1_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U1_B_ I1_B"" TO an_u1_b_i1_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U1_C_ I1_C"" TO an_u1_c_i1_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U_1_ I_1"" TO an_u_1_i_1;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U_2_ I_2"" TO an_u_2_i_2;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∟U_0_ I_0"" TO an_u_0_i_0;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_1"" TO p_1;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_2"" TO p_2;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""P_0"" TO p_0;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""∆f"" TO d_f;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_y"" TO d_u_y;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_A"" TO d_u_a;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_B"" TO d_u_b;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_C"" TO d_u_c;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_AB"" TO d_u_ab;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_BC"" TO d_u_bc;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""δU_CA"" TO d_u_ca;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_2U"" TO k_2u;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_0U"" TO k_0u;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_UA(AB)"" TO k_ua_ab;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_UB(BC)"" TO k_ub_bc;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_UC(CA)"" TO k_uc_ca;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_IA"" TO k_ia;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_IB"" TO k_ib;
						ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_IC"" TO k_ic;";
						dbService.ExecuteNonQuery(commandText, false);

						commandText =
						@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_A(AB)"" TO u1_a_ab;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UA(AB) " + i.ToString() +
								@""" TO k_ua_ab_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_B(BC)"" TO u1_b_bc;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UB(BC) " + i.ToString() +
								@""" TO k_ub_bc_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_C(CA)"" TO u1_c_ca;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UC(CA) " + i.ToString() +
								@""" TO k_uc_ca_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""I1_A"" TO i1_a;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_IA " + i.ToString() +
								@""" TO k_ia_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""I1_B"" TO i1_b;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_IB " + i.ToString() +
								@""" TO k_ib_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""I1_C"" TO i1_c;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_IC " + i.ToString() +
								@""" TO k_ic_" + i.ToString() + ";";
						}
						dbService.ExecuteNonQuery(commandText, false);

						//////////////////////////////////////////////////////////////////////////////////////////////

						commandText = @"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_Σ"" TO p_sum;";
						commandText +=
							@"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_A(1)"" TO p_a_1;";
						commandText +=
							@"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_B(2)"" TO p_b_2;";
						commandText += @"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_C"" TO p_c;";

						for (int i = 1; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_A(1) " + i.ToString() +
								@""" TO p_a_1_" + i.ToString() + ";";
						}

						for (int i = 1; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_B(2) " + i.ToString() +
								@""" TO p_b_2_" + i.ToString() + ";";
						}

						for (int i = 1; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_C " + i.ToString() +
								@""" TO p_c_" + i.ToString() + ";";
						}
						dbService.ExecuteNonQuery(commandText, false);

						////////////////////////////////////////////////////////////////////////
						commandText = "";

						for (int i = 1; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_6b RENAME COLUMN ""∟U_A " + i.ToString() +
								@"_ I_A " + i.ToString() +
								@""" TO an_u_a_" + i.ToString() + "_i_a_" + i.ToString() + ";";
						}

						for (int i = 1; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_6b RENAME COLUMN ""∟U_B " + i.ToString() +
								@"_ I_B " + i.ToString() +
								@""" TO an_u_b_" + i.ToString() + "_i_b_" + i.ToString() + ";";
						}

						for (int i = 1; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_6b RENAME COLUMN ""∟U_C " + i.ToString() +
								@"_ I_C " + i.ToString() +
								@""" TO an_u_c_" + i.ToString() + "_i_c_" + i.ToString() + ";";
						}
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewEm33T: Error while renaming AVG columns");
					}

					#endregion

					// add day_avg_parameters_t6 table for dU
					commandText =
	@"CREATE TABLE day_avg_parameters_t6
(
  datetime_id bigint NOT NULL, -- Идентификатор времени
  event_datetime timestamp without time zone NOT NULL, -- Время измерения значения параметра
  d_u_y real,
  d_u_a real,
  d_u_b real,
  d_u_c real,
  d_u_ab real,
  d_u_bc real,
  d_u_ca real,
  CONSTRAINT day_avg_parameters_t6_pkey PRIMARY KEY (datetime_id, event_datetime),
  CONSTRAINT day_avg_parameters_t6_dt_fkey FOREIGN KEY (datetime_id)
      REFERENCES day_avg_parameter_times (datetime_id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE CASCADE
)
WITH (OIDS=TRUE);
ALTER TABLE day_avg_parameters_t6 OWNER TO energomonitor;
COMMENT ON TABLE day_avg_parameters_t6 IS 'Архив ПКЭ. Отклонения напряжения';
COMMENT ON COLUMN day_avg_parameters_t6.datetime_id IS 'Идентификатор времени';
COMMENT ON COLUMN day_avg_parameters_t6.event_datetime IS 'Время измерения значения параметра';";
					try
					{
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception nex)
					{
						EmService.WriteToLogFailed("Error while updating Em33T DB: " + nex.Message);
					}

					// add day_avg_parameters_t7 table for dF
					commandText =
	@"CREATE TABLE day_avg_parameters_t7
(
  datetime_id bigint NOT NULL, -- Идентификатор времени
  event_datetime timestamp without time zone NOT NULL, -- Время измерения значения параметра
  d_f real,
  CONSTRAINT day_avg_parameters_t7_pkey PRIMARY KEY (datetime_id, event_datetime),
  CONSTRAINT day_avg_parameters_t7_dt_fkey FOREIGN KEY (datetime_id)
      REFERENCES day_avg_parameter_times (datetime_id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE CASCADE
)
WITH (OIDS=TRUE);
ALTER TABLE day_avg_parameters_t7 OWNER TO energomonitor;
COMMENT ON TABLE day_avg_parameters_t7 IS 'Архив ПКЭ. Отклонения частоты';
COMMENT ON COLUMN day_avg_parameters_t7.datetime_id IS 'Идентификатор времени';
COMMENT ON COLUMN day_avg_parameters_t7.event_datetime IS 'Время измерения значения параметра';";
					try
					{
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception nex)
					{
						EmService.WriteToLogFailed("Error while updating Em33T DB: " + nex.Message);
					}

					#endregion
				}

				AddFoldersLevelToTree(0, (tnDevNode as EmArchNodeBase), connectString_);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "ConnectServerAndLoadDataEm33 failed 2");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
			return true;
		}

		/// <summary>Adding new subfolder</summary>
		internal override bool CreateNewFolder(ref EmTreeNodeBase contextNode, ref TreeNode selectedNode)
		{
			try
			{
				if (!(contextNode is EmArchNodeBase)) return false;

				if (contextNode.NodeType != EmTreeNodeType.Folder &&
					contextNode.NodeType != EmTreeNodeType.DeviceFolder) return false;

				string commandText = string.Empty;
				DbService dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}
				try
				{
					if (contextNode.NodeType == EmTreeNodeType.Folder)
					{
						frmAddNewFolder wndAddNewFolder = new frmAddNewFolder();
						if (wndAddNewFolder.ShowDialog() != DialogResult.OK) return true;

						commandText = String.Format("INSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{0}', true, {1}, 0, {2});",
							wndAddNewFolder.FolderName,
							(contextNode as EmTreeNodeFolder).FolderId,
							wndAddNewFolder.FolderInfo == string.Empty ? "null" : "'" + 
										wndAddNewFolder.FolderInfo + "'");
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.Folder,
											(contextNode as EmArchNodeBase).DeviceType,
											null, null);
							contextNode.Nodes.Add(folderNode);
							
							selectedNode = folderNode;
							contextNode = folderNode;
						}
						return true;
					}
					else if (contextNode.NodeType == EmTreeNodeType.DeviceFolder)
					{
						frmAddNewFolder wndAddNewFolder = new frmAddNewFolder();
						if (wndAddNewFolder.ShowDialog() != DialogResult.OK) return true;

						commandText = String.Format("INSERT INTO folders (folder_id, name, is_subfolder, parent_id, folder_type, folder_info) VALUES (DEFAULT, '{0}', false, 0, 0, {1});",
							wndAddNewFolder.FolderName,
							wndAddNewFolder.FolderInfo == string.Empty ? "null" : "'" + 
										wndAddNewFolder.FolderInfo + "'");
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.Folder,
											(contextNode as EmArchNodeBase).DeviceType,
											null, null);
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
				EmService.DumpException(ex, "Error in CreateNewFolderEm33Em32(): ");
				throw;
			}
		}

		internal override bool DeleteFolder(ref EmTreeNodeBase contextNode)
		{
			try
			{
				if (!(contextNode is EmArchNodeBase)) return false;

				string commandText = string.Empty;
				DbService dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				try
				{
					switch (contextNode.NodeType)
					{
						// archives
						case EmTreeNodeType.Object:
							commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(contextNode as EmTreeNodeObject).FolderId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;

						// measure groups
						case EmTreeNodeType.MeasureGroup:
							switch ((contextNode as EmTreeNodeDBMeasureType).MeasureType)
							{
								case MeasureType.PQP:
									commandText = String.Format(
									"DELETE FROM day_avg_parameter_times WHERE database_id = {0};",
									(contextNode.Parent as EmTreeNodeObject).ObjectId);
									break;
								case MeasureType.AVG:
									commandText = String.Format(
									"DELETE FROM period_avg_params_times WHERE database_id = {0};",
									(contextNode.Parent as EmTreeNodeObject).ObjectId);
									break;
								case MeasureType.DNS:
									commandText = String.Format(
									"DELETE FROM dips_and_overs_times WHERE database_id = {0};",
									(contextNode.Parent as EmTreeNodeObject).ObjectId);
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
									"DELETE FROM day_avg_parameter_times WHERE datetime_id = {0};",
									(contextNode as EmTreeNodeDBMeasureClassic).Id);
									break;
								case MeasureType.AVG:
									commandText = String.Format(
									"DELETE FROM period_avg_params_times WHERE datetime_id = {0};",
									(contextNode as EmTreeNodeDBMeasureClassic).Id);
									break;
								case MeasureType.DNS:
									commandText = String.Format(
									"DELETE FROM dips_and_overs_times WHERE datetime_id = {0};",
									(contextNode as EmTreeNodeDBMeasureClassic).Id);
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
					}
				}
				catch (Exception ex)
				{
					EmService.WriteToLogFailed("Error in DBTreeView::DeleteFolder() 1: " +
						ex.Message);
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
				EmService.WriteToLogFailed("Error in DeleteFolder(): " + ex.Message);
				throw;
			}
		}

		internal override bool ExportArchive(out EmSqlDataNodeType[] parts, ref EmTreeNodeBase contextNode)
		{
			parts = null;
			try
			{
				List<EmSqlDataNodeType> parts_list = new List<EmSqlDataNodeType>();

				for (int i = 0; i < contextNode.Nodes.Count; i++)
				{
					switch ((contextNode.Nodes[i] as EmTreeNodeDBMeasureType).MeasureType)
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
				string commandText = string.Empty;
				DbService dbService = new DbService(connectString_);
				frmOptionsFld wndOptionsFld;

				// Options from folder
				if (contextNode.NodeType == EmTreeNodeType.Folder)
				{
					EmTreeNodeFolder folderNode = contextNode as EmTreeNodeFolder;

					wndOptionsFld = new frmOptionsFld(contextNode.Text,
										folderNode.FolderInfo, folderNode.DeviceType);
					DialogResult result = wndOptionsFld.ShowDialog();
					if (result == DialogResult.OK && (folderNode.FolderInfo != wndOptionsFld.FolderInfo))
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
				else if (contextNode.NodeType == EmTreeNodeType.Object)
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
					string strConSch = rm.GetString("name_con_scheme_" +
						(contextNode as EmTreeNodeObject).ConnectionScheme.ToString() +
						"_full");


					frmOptionsDb wndOptionsDb = new frmOptionsDb(
						contextNode.PgServerIndex,
						connectString_,
						(contextNode as EmTreeNodeObject).StartDateTime,
						(contextNode as EmTreeNodeObject).EndDateTime,
						strConSch,
						(contextNode as EmTreeNodeObject).NominalLinearVoltage,
						(contextNode as EmTreeNodeObject).NominalPhaseVoltage,
						(contextNode as EmTreeNodeObject).NominalFrequency,
						(contextNode as EmTreeNodeObject).ObjectName,
						(contextNode as EmTreeNodeObject).ObjectInfo,
						(contextNode as EmTreeNodeObject).DeviceID,
						(contextNode as EmTreeNodeObject).DeviceType,
						(contextNode as EmTreeNodeObject).DeviceName,
						(contextNode as EmTreeNodeObject).DeviceVersion,
						0,  // dummy
						(contextNode as EmTreeNodeObject).ObjectId);
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
			string commandText = string.Empty;
			DbService dbService = new DbService(connectString_);
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return false;
			}
			try
			{
				// if buffer node is object
				if (nodeToPaste.NodeType == EmTreeNodeType.Object)
				{
					if (nodeDestination.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0} WHERE folder_id = {1};",
							(nodeDestination as EmTreeNodeFolder).FolderId,
							(nodeToPaste as EmTreeNodeObject).FolderId);
					}
					// drop node is root
					else if (nodeDestination.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = 0, is_subfolder = false WHERE folder_id = {0};",
							(nodeToPaste as EmTreeNodeObject).FolderId);
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

		protected bool AddFoldersLevelToTree(Int64 parentFldId, EmArchNodeBase parentNode, string connectString)
		{
			DbService dbService = null;
			try
			{
				dbService = new DbService(connectString);
				dbService.Open();
				// выбираем из БД все вложенные папки
				string commandText = String.Format("SELECT * FROM folders f WHERE parent_id = {0} AND folder_type <> 2 ORDER BY folder_id;", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					Int64 fldId = (Int64)dbService.DataReaderData("folder_id");
					string fldName = dbService.DataReaderData("name") as string;
					string fldInfo = string.Empty;
					object oFldInfo = dbService.DataReaderData("folder_info");
					if (!(oFldInfo is System.DBNull)) fldInfo = (string)oFldInfo;
					EmTreeNodeFolder fldNode = new EmTreeNodeFolder(fldId, fldName, fldInfo,
																EmTreeNodeType.Folder,
																EmDeviceType.EM33T, null, null);

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
						AddFoldersLevelToTree(fldId, fldNode, connectString_);
					}
				}

				// выбираем из БД все вложенные объекты (для них рекурсивно не вызываем, т.к. у них
				// не может быть вложенных папок)
				commandText = String.Format("SELECT d.db_id, d.start_datetime, d.end_datetime, d.con_scheme, d.f_nom, d.u_nom_lin, d.u_nom_ph, d.device_id, d.parent_id as parent_folder_id, d.db_name, d.db_info, d.u_limit,  d.i_limit, d.current_transducer_index, d.device_name, d.device_version, d.t_fliker, f.folder_id, f.name as folder_name, f.is_subfolder, f.parent_id, f.folder_type, f.folder_info FROM databases d INNER JOIN folders f ON d.parent_id = f.folder_id AND f.parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					//Em parentDev = null;
					//if (parentNode is EtPqpDeviceTreeNode) parentDev = parentNode as EtPqpDeviceTreeNode;
					//else if (parentNode is EmTreeNodeFolder)
					//    parentDev = (parentNode as EmArchNodeBase).ParentDevice as EtPqpDeviceTreeNode;
					//if (parentDev == null)
					//    throw new EmException("AddFoldersLevelToTree(): Unable to get EtPQP parent device!");

					EmDeviceType curDevType = EmDeviceType.EM33T;
					if ((dbService.DataReaderData("device_name") as string).Contains("T1"))
						curDevType = EmDeviceType.EM33T1;
					if ((dbService.DataReaderData("device_name") as string).Contains("3.1"))
						curDevType = EmDeviceType.EM31K;

					EmTreeNodeObject objNode = new EmTreeNodeObject(null, curDevType);
					objNode.ObjectId = (Int64)dbService.DataReaderData("db_id");
					objNode.StartDateTime = (DateTime)dbService.DataReaderData("start_datetime");
					objNode.EndDateTime = (DateTime)dbService.DataReaderData("end_datetime");
					objNode.ConnectionScheme = (ConnectScheme)(short)dbService.DataReaderData("con_scheme");
					objNode.NominalLinearVoltage = (float)dbService.DataReaderData("u_nom_lin");
					objNode.NominalPhaseVoltage = (float)dbService.DataReaderData("u_nom_ph");
					//objNode.NominalPhaseCurrent = (float)dbService.DataReaderData("i_nom_ph"];
					objNode.NominalFrequency = (float)dbService.DataReaderData("f_nom");
					objNode.DeviceID = (Int64)dbService.DataReaderData("device_id");

					objNode.DeviceName = dbService.DataReaderData("device_name") as string;
					//if (objNode.DeviceName.Contains("T1")) objNode.DeviceType = EmDeviceType.EM33T1;
					//if (objNode.DeviceName.Contains("3.1")) objNode.DeviceType = EmDeviceType.EM31K;

					objNode.DeviceVersion = dbService.DataReaderData("device_version") as string;
					objNode.ObjectName = dbService.DataReaderData("folder_name") as string;
					object oObjInfo = dbService.DataReaderData("folder_info");
					if (oObjInfo is System.DBNull) objNode.ObjectInfo = string.Empty;
					else objNode.ObjectInfo = oObjInfo as string;
					objNode.ULimit = (float)dbService.DataReaderData("u_limit");
					objNode.ILimit = (float)dbService.DataReaderData("i_limit");
					objNode.CurrentTransducerIndex = (short)dbService.DataReaderData("current_transducer_index");
					//objNode.ConstraintType = (short)dbService.DataReaderData("constraint_type"];
					objNode.T_Fliker = (short)dbService.DataReaderData("t_fliker");   // fliker period
					objNode.FolderId = (Int64)dbService.DataReaderData("parent_folder_id");
					//try { objNode.MlStartDateTime1 = (DateTime)dbService.DataReaderData("ml_start_time_1"]; }
					//catch (InvalidCastException) { objNode.MlStartDateTime1 = DateTime.MinValue; }
					//try { objNode.MlEndDateTime1 = (DateTime)dbService.DataReaderData("ml_end_time_1"]; }
					//catch (InvalidCastException) { objNode.MlEndDateTime1 = DateTime.MinValue; }
					//try { objNode.MlStartDateTime2 = (DateTime)dbService.DataReaderData("ml_start_time_2"]; }
					//catch (InvalidCastException) { objNode.MlStartDateTime2 = DateTime.MinValue; }
					//try { objNode.MlEndDateTime2 = (DateTime)dbService.DataReaderData("ml_end_time_2"]; }
					//catch (InvalidCastException) { objNode.MlEndDateTime2 = DateTime.MinValue; }

					objNode.Text = string.Format("{0} # {1} - {2}", objNode.ObjectName,
						objNode.StartDateTime.ToString(), objNode.EndDateTime.ToString());

					// ищем папку для данного объекта (по id)
					EmTreeNodeObject tnObjNodeTemp = null;

					for (int iItem = 0; iItem < parentNode.Nodes.Count; ++iItem)
					{
						if (parentNode.Nodes[iItem] is EmTreeNodeObject)
						{
							if (objNode.ObjectId ==
								(parentNode.Nodes[iItem] as EmTreeNodeObject).ObjectId)
							{
								tnObjNodeTemp = (EmTreeNodeObject)parentNode.Nodes[iItem];
								break;
							}
						}
					}

					// добавляем папку объекта в дерево
					if (tnObjNodeTemp == null)
					{
						parentNode.Nodes.Add(objNode);
						// добавляем архивы в объект
						AddArchivesToObjectFolder(objNode, objNode.DeviceType);
					}
				}

				pgServerNode_.Expand();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "AddFoldersLevelToTree Em33T failed");
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
		protected void AddArchivesToObjectFolder(EmTreeNodeObject tnObjNode, EmDeviceType devType)
		{
			try
			{
				if (tnObjNode == null) return;

				string commandText = string.Empty;
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
					commandText = String.Format("SELECT count(*) AS count FROM day_avg_parameter_times WHERE database_id = {0};", tnObjNode.ObjectId);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM day_avg_parameter_times WHERE database_id = {0};", tnObjNode.ObjectId);
						dbService.ExecuteReader(commandText);

						PQPDatesList pqpList = new PQPDatesList();

						while (dbService.DataReaderRead())
						{
							pqpList.Add(new EmInfoForNodePQPClassic(ref dbService, devType));
						}
						pqpList.SortItems();

						for (int iPqp = 0; iPqp < pqpList.Count; ++iPqp)
						{
							// ищем папку для ПКЭ
							EmTreeNodeDBMeasureType tnPQPNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.EM33T);
							foreach (EmTreeNodeDBMeasureType tn in tnObjNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.EM33T))
								{
									tnPQPNode = tn;
									break;
								}
							}
							if (tnPQPNode == null)	// если папки нет, создаем ее
							{
								tnPQPNode = new EmTreeNodeDBMeasureType(MeasureType.PQP,
									devType, tnObjNode.ParentDevice, tnObjNode);
								tnObjNode.Nodes.Add(tnPQPNode);
							}

							// ищем папку самого архива
							EmTreeNodeDBMeasureClassic tnPQPArchNode = null;
							foreach (EmTreeNodeDBMeasureClassic tn in tnPQPNode.Nodes)
							{
								if (tn.Id == pqpList[iPqp].DatetimeId)
								{
									tnPQPArchNode = tn;
									break;
								}
							}
							if (tnPQPArchNode == null)	// если папки арихва нет, создаем ее
							{
								tnPQPArchNode = new EmTreeNodeDBMeasureClassic(pqpList[iPqp].DatetimeId,
									pqpList[iPqp].DtStart,
									pqpList[iPqp].DtEnd,
									tnObjNode.DeviceVersion, tnObjNode.T_Fliker,
									devType,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlStartTime1,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlEndTime1,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlStartTime2,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlEndTime2,
									//tnObjNode.ConstraintType,
									pqpList[iPqp].ConstraintType,
									tnObjNode.ParentDevice, tnObjNode);
								tnPQPNode.Nodes.Add(tnPQPArchNode);
							}
						}
					}

					#endregion

					#region DNS

					// trying to find DNS
					commandText =
						String.Format("SELECT count(*) AS count FROM dips_and_overs_times WHERE database_id = {0};", tnObjNode.ObjectId);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM dips_and_overs_times WHERE database_id = {0};", tnObjNode.ObjectId);
						dbService.ExecuteReader(commandText);

						DNSDatesList dnsList = new DNSDatesList();

						while (dbService.DataReaderRead())
						{
							dnsList.Add(new EmInfoForNodeDNSClassic(ref dbService, devType));
						}
						dnsList.SortItems();

						for (int iDns = 0; iDns < dnsList.Count; ++iDns)
						{
							// ищем папку для DNS
							EmTreeNodeDBMeasureType tnDNSNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.EM33T);
							foreach (EmTreeNodeDBMeasureType tn in tnObjNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.EM33T))
								{
									tnDNSNode = tn;
									break;
								}
							}
							if (tnDNSNode == null)	// если папки нет, создаем ее
							{
								tnDNSNode = new EmTreeNodeDBMeasureType(MeasureType.DNS,
												devType, tnObjNode.ParentDevice, tnObjNode);
								tnObjNode.Nodes.Add(tnDNSNode);
							}

							// ищем папку самого архива
							EmTreeNodeDBMeasureClassic tnDNSArchNode = null;
							foreach (EmTreeNodeDBMeasureClassic tn in tnDNSNode.Nodes)
							{
								if (tn.Id == dnsList[iDns].DatetimeId)
								{
									tnDNSArchNode = tn;
									break;
								}
							}
							if (tnDNSArchNode == null)	// если папки арихва нет, создаем ее
							{
								tnDNSArchNode = new EmTreeNodeDBMeasureClassic(dnsList[iDns].DatetimeId,
									dnsList[iDns].DtStart,
									dnsList[iDns].DtEnd,
									tnObjNode.DeviceVersion, tnObjNode.T_Fliker,
									devType,
									tnObjNode.MlStartDateTime1, tnObjNode.MlEndDateTime1,
									tnObjNode.MlStartDateTime2, tnObjNode.MlEndDateTime2,
									tnObjNode.ConstraintType, tnObjNode.ParentDevice, tnObjNode);
								tnDNSNode.Nodes.Add(tnDNSArchNode);
							}
						}
					}

					#endregion

					#region AVG

					// trying to find AVG
					commandText =
						String.Format("SELECT count(*) AS count FROM period_avg_params_times WHERE database_id = {0};", tnObjNode.ObjectId);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM period_avg_params_times WHERE database_id = {0};", tnObjNode.ObjectId);
						dbService.ExecuteReader(commandText);

						AVGDatesList avgList = new AVGDatesList();

						while (dbService.DataReaderRead())
						{
							avgList.Add(new EmInfoForNodeAVGClassic(ref dbService, devType));
						}
						avgList.SortItems();

						for (int iAvg = 0; iAvg < avgList.Count; ++iAvg)
						{
							// ищем папку для AVG
							EmTreeNodeDBMeasureType tnAVGNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.EM33T);
							foreach (EmTreeNodeDBMeasureType tn in tnObjNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.EM33T))
								{
									tnAVGNode = tn;
									break;
								}
							}
							if (tnAVGNode == null)	// если папки нет, создаем ее
							{
								tnAVGNode = new EmTreeNodeDBMeasureType(MeasureType.AVG,
												devType, tnObjNode.ParentDevice, tnObjNode);
								tnObjNode.Nodes.Add(tnAVGNode);
							}

							// ищем папку для типа усреденения
							TreeNode tnAvgTypeNode = null;
							for (int iAvgType = 0; iAvgType < tnAVGNode.Nodes.Count; ++iAvgType)
							{
								if (tnAVGNode.Nodes[iAvgType] is EmTreeNodeAvgTypeClassic)
								{
									AvgTypes curType = (tnAVGNode.Nodes[iAvgType] as EmTreeNodeAvgTypeClassic).AvgType;
									if (curType == (avgList[iAvg] as EmInfoForNodeAVGClassic).AvgType)
									{
										tnAvgTypeNode = tnAVGNode.Nodes[iAvgType];
										break;
									}
								}
							}
							if (tnAvgTypeNode == null)	// если папки нет, создаем ее
							{
								tnAvgTypeNode = new EmTreeNodeAvgTypeClassic(
											(avgList[iAvg] as EmInfoForNodeAVGClassic).AvgType,
											devType, tnObjNode.ParentDevice, tnObjNode);
								tnAVGNode.Nodes.Add(tnAvgTypeNode);
								//(tnAvgTypeNode as EmTreeNodeAvgTypeClassic).SetParentDevice();
							}

							// ищем папку самого архива
							EmTreeNodeDBMeasureClassic tnAVGArchNode = null;
							foreach (EmTreeNodeDBMeasureClassic tn in tnAvgTypeNode.Nodes)
							{
								if (tn.Id == avgList[iAvg].DatetimeId)
								{
									tnAVGArchNode = tn;
									break;
								}
							}
							if (tnAVGArchNode == null)	// если папки арихва нет, создаем ее
							{
								tnAVGArchNode = new EmTreeNodeDBMeasureClassic(avgList[iAvg].DatetimeId,
									avgList[iAvg].DtStart,
									avgList[iAvg].DtEnd,
									tnObjNode.DeviceVersion,
									tnObjNode.T_Fliker,
									devType,
									(avgList[iAvg] as EmInfoForNodeAVGClassic).AvgType,
									tnObjNode.ConstraintType,
									tnObjNode.ParentDevice, tnObjNode);

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
				EmService.DumpException(ex, "Error in AddArchivesToObjectFolder():");
				throw;
			}
		}

		protected void CascadeDeleteEmptyFolders(ref EmTreeNodeBase contextNode)
		{
			try
			{
				EmTreeNodeBase nodeToRemove = contextNode;
				EmTreeNodeBase parentNode = (EmTreeNodeBase)contextNode.Parent;

				if (nodeToRemove is EmTreeNodeDBMeasureClassic)  // сам архив
				{
					nodeToRemove.Remove();
				}
				else if (nodeToRemove is EmTreeNodeAvgTypeClassic)
				{
					nodeToRemove.Remove();
				}
				else if (nodeToRemove is EmTreeNodeDBMeasureType)
				{
					nodeToRemove.Remove();
				}

				if (parentNode.Nodes.Count == 0 && !(parentNode is EmTreeNodeObject)
					&& !(parentNode is EmTreeNodeFolder) && !(parentNode is EmTreeNodeServer))
				{
					CascadeDeleteEmptyFolders(ref parentNode);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CascadeDeleteEmptyFolders():");
				throw;
			}
		}

		#endregion
	}
}
