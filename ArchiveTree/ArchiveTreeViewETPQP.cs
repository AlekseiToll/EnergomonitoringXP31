using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Resources;

using EmDataSaver.SavingInterface;
using EmDataSaver.SqlImage;
using EmServiceLib;
using EmArchiveTree;
using DbServiceLib;

namespace EnergomonitoringXP.ArchiveTreeView
{
	class ArchiveTreeViewETPQP : ArchiveTreeViewBase
	{
		#region Constructors

		internal ArchiveTreeViewETPQP(int pgServerIndex, EmTreeNodeBase pgServerNode, string connectString)
			: base(pgServerIndex, pgServerNode, connectString)
		{
		}

		#endregion

		#region Internal Methods

		internal override bool ConnectServerAndLoadData(bool DBwasUpdated)
		{
			string commandText = string.Empty;
			DbService dbService = null;
			try
			{
				dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				// temporary node (reference)
				TreeNode treenode = pgServerNode_;

				// проверяем есть ли папка устройства (ETPQP)
				TreeNode tnDevNode = null;
				foreach (EmTreeNodeDeviceFolder tn in treenode.Nodes)
				{
					if (tn.Text == "ETPQP")
					{
						tnDevNode = tn;
						break;
					}
				}
				if (tnDevNode == null)	// если папки нет, создаем ее
				{
					tnDevNode = new EmTreeNodeDeviceFolder(EmDeviceType.ETPQP);
					treenode.Nodes.Add(tnDevNode);
				}

				if (!DBwasUpdated)
				{
					EmService.WriteToLogGeneral("Update DB ETPQP");

					#region Update DB

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
					ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_sum"" TO q_sum;
					ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_A"" TO q_a;
					ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_B"" TO q_b;
					ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Q_C"" TO q_c;
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
					ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""K_IC"" TO k_ic;
					ALTER TABLE period_avg_params_1_4 RENAME COLUMN ""Qtype"" TO q_type;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP: Error while renaming AVG columns 1");
					}
					try
					{
						commandText =
							@"
							ALTER TABLE period_avg_params_1_4 RENAME COLUMN q_sum TO q_sum_geom;
							ALTER TABLE period_avg_params_1_4 RENAME COLUMN q_a TO q_a_geom;
							ALTER TABLE period_avg_params_1_4 RENAME COLUMN q_b TO q_b_geom;
							ALTER TABLE period_avg_params_1_4 RENAME COLUMN q_c TO q_c_geom;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_sum_shift real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_a_shift real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_b_shift real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_c_shift real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_sum_cross real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_a_cross real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_b_cross real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_c_cross real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_sum_1harm real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_a_1harm real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_b_1harm real;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN q_c_1harm real;
							";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP: Error while renaming AVG columns 2");
					}
					try
					{
						commandText =
							@"
							ALTER TABLE period_avg_params_1_4 RENAME COLUMN tangent_p TO tangent_p_geom;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN tangent_p_shift real DEFAULT 0;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN tangent_p_cross real DEFAULT 0;
							ALTER TABLE period_avg_params_1_4 ADD COLUMN tangent_p_1harm real DEFAULT 0;
							";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP: Error while renaming AVG columns 25");
					}
					try
					{
						commandText =
							"ALTER TABLE period_avg_params_1_4 ADD COLUMN i_n real DEFAULT 0;";
						commandText +=
							"ALTER TABLE period_avg_params_1_4 ADD COLUMN i1_n real DEFAULT 0;";
						commandText +=
							"ALTER TABLE period_avg_params_5 ADD COLUMN i1_n real DEFAULT 0;";
						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 ADD COLUMN i_n_" + i.ToString() + "  real DEFAULT 0;";
						}
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP: Error while renaming AVG columns 26");
					}

					try
					{
						commandText =
							"ALTER TABLE day_avg_parameters_t2 ADD COLUMN valid_duy smallint DEFAULT 0;";
						commandText +=
							"ALTER TABLE day_avg_parameters_t2 ADD COLUMN valid_duy_1 smallint DEFAULT 0;";
						commandText +=
							"ALTER TABLE day_avg_parameters_t2 ADD COLUMN valid_duy_2 smallint DEFAULT 0;";
						commandText +=
							"ALTER TABLE day_avg_parameters_t2 ADD COLUMN valid_ku smallint DEFAULT 0;";
						dbService.ExecuteNonQuery(commandText, false);
					}
					catch (Exception)
					{
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP: Error while renaming AVG columns 26");
					}

					//////////////////////////////////////////////////////////////////////////////////
					try 
					{
						commandText =
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_A(AB)"" TO u1_a;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UA(AB) " + i.ToString() +
								@""" TO k_ua_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_B(BC)"" TO u1_b;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UB(BC) " + i.ToString() +
								@""" TO k_ub_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_C(CA)"" TO u1_c;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UC(CA) " + i.ToString() +
								@""" TO k_uc_" + i.ToString() + ";";
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

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_AB"" TO u1_ab;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UAB " + i.ToString() +
								@""" TO k_uab_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_BC"" TO u1_bc;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UBC " + i.ToString() +
								@""" TO k_ubc_" + i.ToString() + ";";
						}

						commandText +=
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_CA"" TO u1_ca;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UCA " + i.ToString() +
								@""" TO k_uca_" + i.ToString() + ";";
						}
						dbService.ExecuteNonQuery(commandText, false);

						//////////////////////////////////////////////////////////////////////////////////

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
						EmService.WriteToLogFailed("ArchiveTreeViewETPQP: Error while renaming AVG columns 3");
					}

					// add column "flik_sign"
					commandText =
	@"SELECT count(*) 
FROM pg_attribute, pg_class
WHERE pg_class.relname = 'day_avg_parameters_t5'
AND pg_class.relfilenode = pg_attribute.attrelid
AND pg_attribute.attnum > 0 AND pg_attribute.attname = 'flik_sign';";
					object oCount = dbService.ExecuteScalar(commandText);
					Int64 iCount = 0;
					if (!(oCount is DBNull)) { iCount = (Int64)oCount; }
					if (iCount <= 0)
					{
						commandText =
							"ALTER TABLE day_avg_parameters_t5 ADD COLUMN flik_sign smallint DEFAULT 0";
						try
						{
							dbService.ExecuteNonQuery(commandText, false);
						}
						catch (Exception nex)
						{
						    EmService.WriteToLogFailed("Error while updating Em33T DB (flik_sign): " + nex.Message);
						    EmService.WriteToLogFailed("iCount = " + iCount.ToString());
						    if (oCount is DBNull) EmService.WriteToLogFailed("oCount is DBNull!");
						}
					}

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
				if (ex.Message.Contains("database \"et33_db\" does not exist"))
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

				string commandText = string.Empty;
				DbService dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				try
				{
					EmTreeNodeObject parentObj = (contextNode as EmArchNodeBase).ParentObject as EmTreeNodeObject;

					switch (contextNode.NodeType)
					{
						// objects
						case EmTreeNodeType.Object:
							commandText = String.Format(
								"DELETE FROM objects WHERE obj_id = {0} AND device_id = {1};",
								(contextNode as EmTreeNodeObject).ObjectId,
								(contextNode as EmTreeNodeObject).DeviceID);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;

						// measure groups
						case EmTreeNodeType.MeasureGroup:
							switch ((contextNode as EmTreeNodeDBMeasureType).MeasureType)
							{
								case MeasureType.PQP:
									commandText = String.Format(
									"DELETE FROM day_avg_parameter_times WHERE object_id = {0} AND device_id = {1};",
									parentObj.ObjectId,
									parentObj.DeviceID);
									break;
								case MeasureType.AVG:
									commandText = String.Format(
									"DELETE FROM period_avg_params_times WHERE object_id = {0} AND device_id = {1};",
									parentObj.ObjectId,
									parentObj.DeviceID);
									break;
								case MeasureType.DNS:
									commandText = String.Format(
									"DELETE FROM dips_and_overs_times WHERE object_id = {0}) AND device_id = {1};",
									parentObj.ObjectId,
									parentObj.DeviceID);
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
							this.CascadeDeleteEmptyFolders(ref dbService, ref contextNode);
							break;

						// month folders
						case EmTreeNodeType.MonthFolder:
							commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(contextNode as EmTreeNodeMonthFolder).FolderId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;

						// year folders
						case EmTreeNodeType.YearFolder:
							commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(contextNode as EmTreeNodeYearFolder).FolderId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
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

						// EtPQP device
						case EmTreeNodeType.ETPQPDevice:
							commandText = String.Format(
								"DELETE FROM devices WHERE dev_id = {0};",
								(contextNode as EmTreeNodeEtPQPDevice).DeviceId);
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
					contextNode.NodeType != EmTreeNodeType.FolderInDevice)
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
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, object_id) VALUES (DEFAULT, '{0}', true, {1}, 0, {2}, 0, {3}, 0);",
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
											EmDeviceType.ETPQP,
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
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, object_id) VALUES (DEFAULT, '{0}', false, 0, 0, {1}, 0, 0, 0);",
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
					else if (contextNode.NodeType == EmTreeNodeType.ETPQPDevice)
					{
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, object_id) VALUES (DEFAULT, '{0}', true, {1}, 0, {2}, 5, {3}, 0);",
							wndAddNewFolder.FolderName,
							(contextNode as EmTreeNodeEtPQPDevice).FolderId,
							wndAddNewFolder.FolderInfo == string.Empty ?
										"null" : "'" + wndAddNewFolder.FolderInfo + "'",
							(contextNode as EmTreeNodeEtPQPDevice).DeviceId);
						int iResult = dbService.ExecuteNonQuery(commandText, true);
						if (iResult > 0)
						{
							commandText = "SELECT currval('folders_folder_id_seq');";
							Int64 folder_id = (Int64)dbService.ExecuteScalar(commandText);

							EmTreeNodeFolder folderNode = new EmTreeNodeFolder(folder_id,
											wndAddNewFolder.FolderName,
											wndAddNewFolder.FolderInfo,
											EmTreeNodeType.FolderInDevice,
											EmDeviceType.ETPQP,
											(contextNode as EmTreeNodeEtPQPDevice).DeviceId,
											contextNode as EmArchNodeBase, null);
							contextNode.Nodes.Add(folderNode);
						
							selectedNode = folderNode;
							contextNode = folderNode;
						}
						return true;
					}
					else if (contextNode.NodeType == EmTreeNodeType.FolderInDevice)
					{
						commandText = String.Format("INSERT INTO folders (folder_id, folder_name, is_subfolder, parent_id, folder_child, folder_info, folder_type, device_id, object_id) VALUES (DEFAULT, '{0}', true, {1}, 0, {2}, 5, {3}, 0);",
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
								((contextNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQPDevice).DeviceId;

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
				if (!(contextNode is EmTreeNodeObject)) return false;

				List<EmSqlDataNodeType> parts_list = new List<EmSqlDataNodeType>();

				EmArchNodeBase curYearNode, curMoNode;
				bool pqpAdded = false, avgAdded = false, dnoAdded = false;

				for (int iYear = 0; iYear < contextNode.Nodes.Count; iYear++)
				{
					curYearNode = (EmArchNodeBase)contextNode.Nodes[iYear];
					for (int iMo = 0; iMo < curYearNode.Nodes.Count; iMo++)
					{
						curMoNode = (EmArchNodeBase)curYearNode.Nodes[iMo];
						for (int iType = 0; iType < curMoNode.Nodes.Count; iType++)
						{
							switch ((curMoNode.Nodes[iType] as EmTreeNodeDBMeasureType).MeasureType)
							{
								case MeasureType.PQP:
									if (!pqpAdded)
									{
										parts_list.Add(EmSqlDataNodeType.PQP);
										pqpAdded = true;
									}
									break;
								case MeasureType.AVG:
									if (!avgAdded)
									{
										parts_list.Add(EmSqlDataNodeType.AVG);
										avgAdded = true;
									}
									break;
								case MeasureType.DNS:
									if (!dnoAdded)
									{
										parts_list.Add(EmSqlDataNodeType.Events);
										dnoAdded = true;
									}
									break;
							}
						}
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

				// Options from device
				if (contextNode.NodeType == EmTreeNodeType.ETPQPDevice)
				{
					EmTreeNodeEtPQPDevice devNode = contextNode as EmTreeNodeEtPQPDevice;

					wndOptionsFld = new frmOptionsFld(contextNode.Text, devNode.DeviceInfo,
														EmDeviceType.ETPQP);
					DialogResult result = wndOptionsFld.ShowDialog();
					if (result == DialogResult.OK && (devNode.DeviceInfo != wndOptionsFld.FolderInfo))
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
								(contextNode as EmTreeNodeEtPQPDevice).FolderId);

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

				// Options from year folder
				else if (contextNode.NodeType == EmTreeNodeType.YearFolder)
				{
					EmTreeNodeYearFolder folderNode = (contextNode as EmTreeNodeYearFolder);

					if(!dbService.Open())
					{
						MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
						return false;
					}

					try
					{
						commandText = String.Format("SELECT folder_info from folders WHERE folder_id = {0};", folderNode.FolderId);
						object info = dbService.ExecuteScalar(commandText);
						string fldInfo;
						if (info is DBNull) fldInfo = "";
						else fldInfo = (string)info;
						folderNode.FolderInfo = fldInfo;

						wndOptionsFld = new frmOptionsFld(contextNode.Text, fldInfo, folderNode.DeviceType);
						DialogResult result = wndOptionsFld.ShowDialog();
						if (result == DialogResult.OK && (folderNode.FolderInfo != wndOptionsFld.FolderInfo))
						{
							commandText = String.Format("UPDATE folders SET folder_info = '{0}' WHERE folders.folder_id = {1};",
								wndOptionsFld.FolderInfo,
								folderNode.FolderId);

							int iResult = dbService.ExecuteNonQuery(commandText, true);
							folderNode.FolderInfo = wndOptionsFld.FolderInfo;
						}
					}
					finally
					{
						if (dbService != null) dbService.CloseConnect();
					}
				}

				// Options from month folder
				else if (contextNode.NodeType == EmTreeNodeType.MonthFolder)
				{
					EmTreeNodeMonthFolder folderNode = (contextNode as EmTreeNodeMonthFolder);

					if(!dbService.Open())
					{
						MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
						return false;
					}

					try
					{
						commandText = String.Format("SELECT folder_info from folders WHERE folder_id = {0};", folderNode.FolderId);

						object info = dbService.ExecuteScalar(commandText);
						string fldInfo;
						if (info is DBNull) fldInfo = "";
						else fldInfo = (string)info;
						folderNode.FolderInfo = fldInfo;

						wndOptionsFld = new frmOptionsFld(contextNode.Text, fldInfo, folderNode.DeviceType);
						DialogResult result = wndOptionsFld.ShowDialog();
						if (result == DialogResult.OK && (folderNode.FolderInfo != wndOptionsFld.FolderInfo))
						{
							commandText = String.Format("UPDATE folders SET folder_info = '{0}' WHERE folders.folder_id = {1};",
								wndOptionsFld.FolderInfo,
								folderNode.FolderId);

							int iResult = dbService.ExecuteNonQuery(commandText, true);
							folderNode.FolderInfo = wndOptionsFld.FolderInfo;
						}
					}
					finally
					{
						if (dbService != null) dbService.CloseConnect();
					}
				}

				// Options from object
				else if (contextNode.NodeType == EmTreeNodeType.Object)
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
						this.GetType().Assembly);
					string strConSch = rm.GetString("name_con_scheme_" +
						(contextNode as EmTreeNodeObject).ConnectionScheme.ToString() +
						"_full");

					EmTreeNodeEtPQPDevice parentDev = 
						(contextNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQPDevice;
					if (parentDev == null) throw new EmException("Unable to get parent device!");

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
						"ETPQP",
						(contextNode as EmTreeNodeObject).DeviceVersion,
						parentDev.SerialNumber,
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
			DbService dbService = new DbService(connectString_);
			if(!dbService.Open())
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				return false;
			}
			try
			{
				string commandText = string.Empty;

				// if buffer node is object
				if (nodeToPaste.NodeType == EmTreeNodeType.Object)
				{
					if (nodeDestination.NodeType == EmTreeNodeType.FolderInDevice)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0} WHERE folder_type = 2 AND object_id = {1} AND device_id = {2}",
							(nodeDestination as EmTreeNodeFolder).FolderId,
							(nodeToPaste as EmTreeNodeObject).ObjectId,
							(nodeToPaste as EmTreeNodeObject).DeviceID);
					}
					else if (nodeDestination.NodeType == EmTreeNodeType.ETPQPDevice)
					{
						if((nodeDestination as EmTreeNodeEtPQPDevice).DeviceId == 
							(nodeToPaste as EmTreeNodeObject).DeviceID)
						commandText = String.Format("UPDATE folders SET parent_id = {0} WHERE folder_type = 2 AND object_id = {1} AND device_id = {2}",
							(nodeDestination as EmTreeNodeEtPQPDevice).FolderId,
							(nodeToPaste as EmTreeNodeObject).ObjectId,
							(nodeToPaste as EmTreeNodeObject).DeviceID);
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
					else if (nodeDestination.NodeType == EmTreeNodeType.ETPQPDevice)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folder_id = {1};",
							((EmTreeNodeEtPQPDevice)nodeDestination).FolderId,
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}
				}
				// if buffer node is device
				else if (nodeToPaste.NodeType == EmTreeNodeType.ETPQPDevice)
				{
					// drop node is device folder
					if (nodeDestination.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = 0, is_subfolder = FALSE WHERE folder_id = {0};", ((EmTreeNodeEtPQPDevice)nodeToPaste).FolderId);
					}

					// drop node is folder
					else if (nodeDestination.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folder_id = {1};",
							((EmTreeNodeFolder)nodeDestination).FolderId,
							((EmTreeNodeEtPQPDevice)nodeToPaste).FolderId);
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
																EmDeviceType.ETPQP,
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
						AddFoldersLevelToTree(fldId, fldNode, connectString_);
					}
				}

				// выбираем из БД все вложенные устройства
				commandText = String.Format("SELECT * FROM folders f INNER JOIN devices d ON f.device_id = d.dev_id AND d.parent_folder_id = f.folder_id AND f.folder_type = 1 AND f.parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					EmTreeNodeEtPQPDevice tnDevNode = new EmTreeNodeEtPQPDevice();
					tnDevNode.SerialNumber = (Int64)dbService.DataReaderData("ser_number");
					tnDevNode.DeviceId = (Int64)dbService.DataReaderData("dev_id");
					tnDevNode.DeviceVersion = dbService.DataReaderData("dev_version") as string;
					object oDevInfo = dbService.DataReaderData("folder_info");
					if (oDevInfo is System.DBNull) tnDevNode.DeviceInfo = string.Empty;
					else tnDevNode.DeviceInfo = oDevInfo as string;
					tnDevNode.FolderId = (Int64)dbService.DataReaderData("folder_id");

					tnDevNode.Text = string.Format("ETPQP # {0}", tnDevNode.SerialNumber);

					// ищем папку для данного прибора (по серийному номеру)
					EmTreeNodeEtPQPDevice tnDevNodeTemp = null;

					for (int iItem = 0; iItem < parentNode.Nodes.Count; ++iItem)
					{
						if (parentNode.Nodes[iItem] is EmTreeNodeEtPQPDevice)
						{
							if (tnDevNode.SerialNumber ==
								(parentNode.Nodes[iItem] as EmTreeNodeEtPQPDevice).SerialNumber)
							{
								tnDevNodeTemp = (EmTreeNodeEtPQPDevice)parentNode.Nodes[iItem];
								break;
							}
						}
					}

					// добавляем папку прибора в дерево
					if (tnDevNodeTemp == null)
					{
						parentNode.Nodes.Add(tnDevNode);
						AddFoldersLevelToTree(tnDevNode.FolderId, tnDevNode, connectString_);
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

					EmTreeNodeEtPQPDevice parentDev = null;
					if (parentNode is EmTreeNodeEtPQPDevice) parentDev = parentNode as EmTreeNodeEtPQPDevice;
					else if (parentNode is EmTreeNodeFolder)
						parentDev = (parentNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQPDevice;
					if (parentDev == null)
						throw new EmException("AddFoldersLevelToTree(): Unable to get EtPQP parent device!");

					EmTreeNodeFolder fldNode = new EmTreeNodeFolder(fldId, fldName, fldInfo,
																EmTreeNodeType.FolderInDevice,
																EmDeviceType.ETPQP, parentDev, null);

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
				commandText = String.Format("SELECT * FROM folders f INNER JOIN objects ob ON f.folder_id = ob.parent_folder_id AND f.object_id = ob.obj_id AND f.folder_type = 2 AND f.parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					EmTreeNodeEtPQPDevice parentDev = null;
					if (parentNode is EmTreeNodeEtPQPDevice) parentDev = parentNode as EmTreeNodeEtPQPDevice;
					else if (parentNode is EmTreeNodeFolder)
						parentDev = (parentNode as EmArchNodeBase).ParentDevice as EmTreeNodeEtPQPDevice;
					if (parentDev == null)
						throw new EmException("AddFoldersLevelToTree(): Unable to get EtPQP parent device!");

					EmTreeNodeObject objNode = new EmTreeNodeObject(parentDev, EmDeviceType.ETPQP);
					objNode.ObjectId = (Int64)dbService.DataReaderData("obj_id");
					objNode.GlobalObjectId = (Int64)dbService.DataReaderData("global_obj_id");
					objNode.StartDateTime = (DateTime)dbService.DataReaderData("start_datetime");
					objNode.EndDateTime = (DateTime)dbService.DataReaderData("end_datetime");
					objNode.ConnectionScheme = (ConnectScheme)(short)dbService.DataReaderData("con_scheme");
					objNode.NominalLinearVoltage = (float)dbService.DataReaderData("u_nom_lin");
					objNode.NominalPhaseVoltage = (float)dbService.DataReaderData("u_nom_ph");
					objNode.NominalPhaseCurrent = (float)dbService.DataReaderData("i_nom_ph");
					objNode.NominalFrequency = (float)dbService.DataReaderData("f_nom");
					objNode.DeviceID = (Int64)dbService.DataReaderData("device_id");
					//objNode.DeviceType = EmDeviceType.ETPQP;
					objNode.DeviceVersion = dbService.DataReaderData("device_version") as string;
					objNode.ObjectName = dbService.DataReaderData("obj_name") as string;
					object oObjInfo = dbService.DataReaderData("obj_info");
					if (oObjInfo is System.DBNull) objNode.ObjectInfo = string.Empty;
					else objNode.ObjectInfo = oObjInfo as string;
					objNode.ULimit = (float)dbService.DataReaderData("u_limit");
					objNode.ILimit = (float)dbService.DataReaderData("i_limit");
					objNode.CurrentTransducerIndex = (short)dbService.DataReaderData("current_transducer_index");
					objNode.ConstraintType = (short)dbService.DataReaderData("constraint_type");
					objNode.T_Fliker = (short)dbService.DataReaderData("t_fliker");   // fliker period
					objNode.FolderId = (Int64)dbService.DataReaderData("parent_folder_id");
					try { objNode.MlStartDateTime1 = (DateTime)dbService.DataReaderData("ml_start_time_1"); }
					catch (InvalidCastException) { objNode.MlStartDateTime1 = DateTime.MinValue; }
					try { objNode.MlEndDateTime1 = (DateTime)dbService.DataReaderData("ml_end_time_1"); }
					catch (InvalidCastException) { objNode.MlEndDateTime1 = DateTime.MinValue; }
					try { objNode.MlStartDateTime2 = (DateTime)dbService.DataReaderData("ml_start_time_2"); }
					catch (InvalidCastException) { objNode.MlStartDateTime2 = DateTime.MinValue; }
					try { objNode.MlEndDateTime2 = (DateTime)dbService.DataReaderData("ml_end_time_2"); }
					catch (InvalidCastException) { objNode.MlEndDateTime2 = DateTime.MinValue; }

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
						AddArchivesToObjectFolder(objNode);
					}
				}

				pgServerNode_.Expand();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "AddFoldersLevelToTree EtPQP failed");
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
		protected void AddArchivesToObjectFolder(EmTreeNodeObject tnObjNode)
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
					commandText = String.Format("SELECT count(*) AS count FROM day_avg_parameter_times WHERE object_id = {0} AND device_id = {1};", tnObjNode.ObjectId, tnObjNode.DeviceID);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM day_avg_parameter_times WHERE object_id = {0} AND device_id = {1};", tnObjNode.ObjectId, tnObjNode.DeviceID);
						dbService.ExecuteReader(commandText);

						PQPDatesList pqpList = new PQPDatesList();

						while (dbService.DataReaderRead())
						{
							pqpList.Add(new EmInfoForNodePQPClassic(ref dbService, EmDeviceType.ETPQP));
						}
						pqpList.SortItems();

						for (int iPqp = 0; iPqp < pqpList.Count; ++iPqp)
						{
							// добавляем в дерево архивов
							// ищем папку для года
							TreeNode tnYearNode = null;
							foreach (EmTreeNodeYearFolder tn in tnObjNode.Nodes)
							{
								//if (tn.Text == start_datetime.Year.ToString())
								if (tn.FolderId == (pqpList[iPqp] as EmInfoForNodePQPClassic).FldYearId)
								{
									tnYearNode = tn;
									break;
								}
							}
							if (tnYearNode == null)	// если папки нет, создаем ее
							{
								tnYearNode = new EmTreeNodeYearFolder(
											(pqpList[iPqp] as EmInfoForNodePQPClassic).FldYearId,
											EmDeviceType.ETPQP,
											pqpList[iPqp].DtStart.Year.ToString(),
											tnObjNode.ParentDevice, tnObjNode);
								tnObjNode.Nodes.Add(tnYearNode);
								//(tnYearNode as EmTreeNodeYearFolder).SetParentDevice();
							}

							// ищем папку для месяца
							TreeNode tnMonthNode = null;
							foreach (EmTreeNodeMonthFolder tn in tnYearNode.Nodes)
							{
								//if (tn.Text == start_datetime.Month.ToString())
								if (tn.FolderId == (pqpList[iPqp] as EmInfoForNodePQPClassic).FldMonthId)
								{
									tnMonthNode = tn;
									break;
								}
							}
							if (tnMonthNode == null)	// если папки нет, создаем ее
							{
								tnMonthNode = new EmTreeNodeMonthFolder(
											(pqpList[iPqp] as EmInfoForNodePQPClassic).FldMonthId,
											EmDeviceType.ETPQP,
											pqpList[iPqp].DtStart.Month.ToString(),
											tnObjNode.ParentDevice, tnObjNode);
								tnYearNode.Nodes.Add(tnMonthNode);
								//(tnMonthNode as EmTreeNodeMonthFolder).SetParentDevice();
							}

							// ищем папку для ПКЭ
							EmTreeNodeDBMeasureType tnPQPNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.ETPQP);
							foreach (EmTreeNodeDBMeasureType tn in tnMonthNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.ETPQP))
								{
									tnPQPNode = tn;
									break;
								}
							}
							if (tnPQPNode == null)	// если папки нет, создаем ее
							{
								tnPQPNode = new EmTreeNodeDBMeasureType(MeasureType.PQP,
									EmDeviceType.ETPQP, tnObjNode.ParentDevice, tnObjNode);
								tnMonthNode.Nodes.Add(tnPQPNode);
								//tnPQPNode.SetParentDevice();
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
									EmDeviceType.ETPQP,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlStartTime1,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlEndTime1,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlStartTime2,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlEndTime2,
									tnObjNode.ConstraintType,
									tnObjNode.ParentDevice, tnObjNode);
								tnPQPNode.Nodes.Add(tnPQPArchNode);
								//tnPQPArchNode.SetParentDevice();
							}
						}
					}

					#endregion

					#region DNS

					// trying to find DNS
					commandText =
						String.Format("SELECT count(*) AS count FROM dips_and_overs_times WHERE object_id = {0} AND device_id = {1};", tnObjNode.ObjectId, tnObjNode.DeviceID);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM dips_and_overs_times WHERE object_id = {0} AND device_id = {1};", tnObjNode.ObjectId, tnObjNode.DeviceID);
						dbService.ExecuteReader(commandText);

						DNSDatesList dnsList = new DNSDatesList();

						while (dbService.DataReaderRead())
						{
							dnsList.Add(new EmInfoForNodeDNSClassic(ref dbService, EmDeviceType.ETPQP));
						}
						dnsList.SortItems();

						for (int iDns = 0; iDns < dnsList.Count; ++iDns)
						{
							// добавляем в дерево архивов
							// ищем папку для года
							TreeNode tnYearNode = null;
							foreach (EmTreeNodeYearFolder tn in tnObjNode.Nodes)
							{
								//if (tn.Text == start_datetime.Year.ToString())
								if (tn.FolderId == (dnsList[iDns] as EmInfoForNodeDNSClassic).FldYearId)
								{
									tnYearNode = tn;
									break;
								}
							}
							if (tnYearNode == null)	// если папки нет, создаем ее
							{
								tnYearNode = new EmTreeNodeYearFolder(
											(dnsList[iDns] as EmInfoForNodeDNSClassic).FldYearId,
											EmDeviceType.ETPQP,
											dnsList[iDns].DtStart.Year.ToString(),
											tnObjNode.ParentDevice, tnObjNode);
								tnObjNode.Nodes.Add(tnYearNode);
								//(tnYearNode as EmTreeNodeYearFolder).SetParentDevice();
							}

							// ищем папку для месяца
							TreeNode tnMonthNode = null;
							foreach (EmTreeNodeMonthFolder tn in tnYearNode.Nodes)
							{
								//if (tn.Text == start_datetime.Month.ToString())
								if (tn.FolderId == (dnsList[iDns] as EmInfoForNodeDNSClassic).FldMonthId)
								{
									tnMonthNode = tn;
									break;
								}
							}
							if (tnMonthNode == null)	// если папки нет, создаем ее
							{
								tnMonthNode = new EmTreeNodeMonthFolder(
											(dnsList[iDns] as EmInfoForNodeDNSClassic).FldMonthId,
											EmDeviceType.ETPQP,
											dnsList[iDns].DtStart.Month.ToString(),
											tnObjNode.ParentDevice, tnObjNode);
								tnYearNode.Nodes.Add(tnMonthNode);
								//(tnMonthNode as EmTreeNodeMonthFolder).SetParentDevice();
							}

							// ищем папку для DNS
							EmTreeNodeDBMeasureType tnDNSNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.ETPQP);
							foreach (EmTreeNodeDBMeasureType tn in tnMonthNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.ETPQP))
								{
									tnDNSNode = tn;
									break;
								}
							}
							if (tnDNSNode == null)	// если папки нет, создаем ее
							{
								tnDNSNode = new EmTreeNodeDBMeasureType(MeasureType.DNS,
												EmDeviceType.ETPQP, tnObjNode.ParentDevice, tnObjNode);
								tnMonthNode.Nodes.Add(tnDNSNode);
								//tnDNSNode.SetParentDevice();
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
									EmDeviceType.ETPQP,
									tnObjNode.MlStartDateTime1, tnObjNode.MlEndDateTime1,
									tnObjNode.MlStartDateTime2, tnObjNode.MlEndDateTime2,
									tnObjNode.ConstraintType, tnObjNode.ParentDevice, tnObjNode);
								tnDNSNode.Nodes.Add(tnDNSArchNode);
								//tnDNSArchNode.SetParentDevice();
							}
						}
					}

					#endregion

					#region AVG

					// trying to find AVG
					commandText =
						String.Format("SELECT count(*) AS count FROM period_avg_params_times WHERE object_id = {0} AND device_id = {1};", tnObjNode.ObjectId, tnObjNode.DeviceID);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM period_avg_params_times WHERE object_id = {0} AND device_id = {1};", tnObjNode.ObjectId, tnObjNode.DeviceID);
						dbService.ExecuteReader(commandText);

						AVGDatesList avgList = new AVGDatesList();

						while (dbService.DataReaderRead())
						{
							avgList.Add(new EmInfoForNodeAVGClassic(ref dbService, EmDeviceType.ETPQP));
						}
						avgList.SortItems();

						for (int iAvg = 0; iAvg < avgList.Count; ++iAvg)
						{
							// добавляем в дерево архивов
							// ищем папку для года
							TreeNode tnYearNode = null;
							foreach (EmTreeNodeYearFolder tn in tnObjNode.Nodes)
							{
								//if (tn.Text == start_datetime.Year.ToString())
								if (tn.FolderId == (avgList[iAvg] as EmInfoForNodeAVGClassic).FldYearId)
								{
									tnYearNode = tn;
									break;
								}
							}
							if (tnYearNode == null)	// если папки нет, создаем ее
							{
								tnYearNode = new EmTreeNodeYearFolder(
											(avgList[iAvg] as EmInfoForNodeAVGClassic).FldYearId,
											EmDeviceType.ETPQP,
											avgList[iAvg].DtStart.Year.ToString(), 
											tnObjNode.ParentDevice, tnObjNode);
								tnObjNode.Nodes.Add(tnYearNode);
							}

							// ищем папку для месяца
							TreeNode tnMonthNode = null;
							foreach (EmTreeNodeMonthFolder tn in tnYearNode.Nodes)
							{
								//if (tn.Text == start_datetime.Month.ToString())
								if (tn.FolderId == (avgList[iAvg] as EmInfoForNodeAVGClassic).FldMonthId)
								{
									tnMonthNode = tn;
									break;
								}
							}
							if (tnMonthNode == null)	// если папки нет, создаем ее
							{
								tnMonthNode = new EmTreeNodeMonthFolder(
											(avgList[iAvg] as EmInfoForNodeAVGClassic).FldMonthId,
											EmDeviceType.ETPQP,
											avgList[iAvg].DtStart.Month.ToString(), 
											tnObjNode.ParentDevice, tnObjNode);
								tnYearNode.Nodes.Add(tnMonthNode);
							}

							// ищем папку для AVG
							EmTreeNodeDBMeasureType tnAVGNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.ETPQP);
							foreach (EmTreeNodeDBMeasureType tn in tnMonthNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.ETPQP))
								{
									tnAVGNode = tn;
									break;
								}
							}
							if (tnAVGNode == null)	// если папки нет, создаем ее
							{
								tnAVGNode = new EmTreeNodeDBMeasureType(MeasureType.AVG,
												EmDeviceType.ETPQP, tnObjNode.ParentDevice, tnObjNode);
								tnMonthNode.Nodes.Add(tnAVGNode);
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
											EmDeviceType.ETPQP, tnObjNode.ParentDevice, tnObjNode);
								tnAVGNode.Nodes.Add(tnAvgTypeNode);
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
									EmDeviceType.ETPQP,
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

		protected void CascadeDeleteEmptyFolders(ref DbService dbService, ref EmTreeNodeBase contextNode)
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
				else if (nodeToRemove is EmTreeNodeMonthFolder)
				{
					string commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(nodeToRemove as EmTreeNodeMonthFolder).FolderId);
					dbService.ExecuteNonQuery(commandText, true);
					nodeToRemove.Remove();
				}
				else if (nodeToRemove is EmTreeNodeYearFolder)
				{
					string commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(nodeToRemove as EmTreeNodeYearFolder).FolderId);
					dbService.ExecuteNonQuery(commandText, true);
					nodeToRemove.Remove();
				}

				if (parentNode.Nodes.Count == 0 && !(parentNode is EmTreeNodeEtPQPDevice)
					&& !(parentNode is EmTreeNodeFolder) && !(parentNode is EmTreeNodeServer))
				{
					CascadeDeleteEmptyFolders(ref dbService, ref parentNode);
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
