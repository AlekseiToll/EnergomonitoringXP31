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
	class ArchiveTreeViewEM32 : ArchiveTreeViewBase
	{
		#region Constructors

		internal ArchiveTreeViewEM32(int pgServerIndex, EmTreeNodeBase pgServerNode, string connectString)
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
				dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return false;
				}

				// temporary node (reference)
				TreeNode treenode = pgServerNode_;

				// проверяем есть ли папка EM32
				EmTreeNodeDeviceFolder tnEM32Node = null;
				foreach (EmTreeNodeDeviceFolder tn in treenode.Nodes)
				{
					if (tn.Text == "EM32")
					{
						tnEM32Node = tn;
						break;
					}
				}
				if (tnEM32Node == null)	// если папки нет, создаем ее
				{
					tnEM32Node = new EmTreeNodeDeviceFolder(EmDeviceType.EM32);
					treenode.Nodes.Add(tnEM32Node);
				}

				if (!DBwasUpdated)
				{
					EmService.WriteToLogGeneral("Update DB EM-32");

					#region Update DB

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
							EmService.WriteToLogFailed("Error while updating Em32 DB (flik_sign): " + nex.Message);
							EmService.WriteToLogFailed("iCount = " + iCount.ToString());
							if (oCount is DBNull) EmService.WriteToLogFailed("oCount is DBNull!");
						}
					}

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

						////////////////////////////////////////////////////////////////////////////////////////////

						commandText =
							@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_A(AB)"" TO u1_a;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UA(AB) " + i.ToString() +
								@""" TO k_ua_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_B(BC)"" TO u1_b;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UB(BC) " + i.ToString() +
								@""" TO k_ub_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_C(CA)"" TO u1_c;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UC(CA) " + i.ToString() +
								@""" TO k_uc_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""I1_A"" TO i1_a;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_IA " + i.ToString() +
								@""" TO k_ia_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""I1_B"" TO i1_b;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_IB " + i.ToString() +
								@""" TO k_ib_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""I1_C"" TO i1_c;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_IC " + i.ToString() +
								@""" TO k_ic_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_AB"" TO u1_ab;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UAB " + i.ToString() +
								@""" TO k_uab_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_BC"" TO u1_bc;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UBC " + i.ToString() +
								@""" TO k_ubc_" + i.ToString() + ";";
						}

						commandText += @"ALTER TABLE period_avg_params_5 RENAME COLUMN ""U1_CA"" TO u1_ca;";

						for (int i = 2; i <= 40; ++i)
						{
							commandText +=
								@"ALTER TABLE period_avg_params_5 RENAME COLUMN ""K_UCA " + i.ToString() +
								@""" TO k_uca_" + i.ToString() + ";";
						}
						dbService.ExecuteNonQuery(commandText, false);

						//////////////////////////////////////////////////////////////////////////////////////////////

						commandText = @"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_Σ"" TO p_sum;";
						commandText += @"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_A(1)"" TO p_a_1;";
						commandText += @"ALTER TABLE period_avg_params_6a RENAME COLUMN ""P_B(2)"" TO p_b_2;";
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
						EmService.WriteToLogFailed("ArchiveTreeViewEm32: Error while renaming AVG columns");
					}

					#endregion
				}

				AddFoldersLevelToTree(0, (tnEM32Node as EmArchNodeBase), connectString_);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("database \"em32_db\" does not exist"))
				{
					EmService.WriteToLogFailed(ex.Message);
					throw new EmNoDatabaseException();
				}
				else
				{
					EmService.DumpException(ex, "ConnectServerAndLoadDataEm32 failed 1");
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
					switch (contextNode.NodeType)
					{
						// measure groups
						case EmTreeNodeType.MeasureGroup:
							EmTreeNodeEm32Device parentDevice = 
								(contextNode as EmArchNodeBase).ParentDevice as EmTreeNodeEm32Device;
							switch ((contextNode as EmTreeNodeDBMeasureType).MeasureType)
							{
								case MeasureType.PQP:
									commandText = String.Format(
									"DELETE FROM day_avg_parameter_times WHERE datetime_id IN (SELECT datetime_id FROM day_avg_parameter_times WHERE device_id = {0} and folder_month_id = {1});",
									parentDevice.DeviceId, (contextNode.Parent as EmTreeNodeMonthFolder).FolderId);
									break;
								case MeasureType.AVG:
									commandText = String.Format(
									"DELETE FROM period_avg_params_times WHERE datetime_id IN (SELECT datetime_id FROM period_avg_params_times WHERE device_id = {0} and folder_month_id = {1});",
									parentDevice.DeviceId, (contextNode.Parent as EmTreeNodeMonthFolder).FolderId);
									break;
								case MeasureType.DNS:
									commandText = String.Format(
									"DELETE FROM dips_and_overs_times WHERE datetime_id IN (SELECT datetime_id FROM dips_and_overs_times WHERE device_id = {0} and folder_month_id = {1});",
									parentDevice.DeviceId, (contextNode.Parent as EmTreeNodeMonthFolder).FolderId);
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
							CascadeDeleteEmptyFolders(ref dbService, ref contextNode);
							break;

						// folders
						case EmTreeNodeType.Folder:
							commandText = String.Format(
								"DELETE FROM folders WHERE folder_id = {0};",
								(contextNode as EmTreeNodeFolder).FolderId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
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

						// em32 device
						case EmTreeNodeType.EM32Device:
							commandText = String.Format(
								"DELETE FROM devices WHERE dev_id = {0};",
								(contextNode as EmTreeNodeEm32Device).DeviceId);
							dbService.ExecuteNonQuery(commandText, true);
							contextNode.Remove();
							break;
					}
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in DBTreeView::DeleteFolder() 2: ");
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
							wndAddNewFolder.FolderInfo == string.Empty ? "null" : "'" + wndAddNewFolder.FolderInfo + "'");
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
							wndAddNewFolder.FolderInfo == string.Empty ? "null" : "'" + wndAddNewFolder.FolderInfo + "'");
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

		internal override bool ExportArchive(out EmSqlDataNodeType[] parts, 
										ref EmTreeNodeBase contextNode)
		{
			parts = null;
			try
			{
				if (!(contextNode is EmTreeNodeEm32Device)) return false;

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

				// Options from folder
				if (contextNode.NodeType == EmTreeNodeType.Folder)
				{
					EmTreeNodeFolder folderNode = contextNode as EmTreeNodeFolder;

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

				// options from Em32 device
				else if (contextNode.NodeType == EmTreeNodeType.EM32Device)
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
															this.GetType().Assembly);
					string strConSch = rm.GetString("name_con_scheme_" + 
						(contextNode as EmTreeNodeEm32Device).ConnectionScheme.ToString() + 
						"_full");

					if(!dbService.Open())
					{
						MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
						return false;
					}

					try
					{
						commandText = String.Format("SELECT dev_info FROM devices WHERE dev_id = {0};",
							(contextNode as EmTreeNodeEm32Device).DeviceId);

						object info = dbService.ExecuteScalar(commandText);
						string devInfo;
						if (info is DBNull) devInfo = "";
						else devInfo = (string)info;
						(contextNode as EmTreeNodeEm32Device).DeviceInfo = devInfo;
					}
					finally
					{
						if (dbService != null) dbService.CloseConnect();
					}

					frmOptionsEm32 wndOptionsDb = new frmOptionsEm32(
						contextNode.PgServerIndex,
						connectString_,
						strConSch,
						(contextNode as EmTreeNodeEm32Device).NominalLinearVoltage,
						(contextNode as EmTreeNodeEm32Device).NominalPhaseVoltage,
						(contextNode as EmTreeNodeEm32Device).NominalFrequency,
						(contextNode as EmTreeNodeEm32Device).NominalPhaseCurrent,
						(contextNode as EmTreeNodeEm32Device).DeviceId,
						(contextNode as EmTreeNodeEm32Device).DeviceInfo,
						(contextNode as EmTreeNodeEm32Device).ObjectName,
						(contextNode as EmTreeNodeEm32Device).DeviceVersion,
						(contextNode as EmTreeNodeEm32Device).ConstraintType,
						(contextNode as EmTreeNodeEm32Device).SerialNumber,
						(contextNode as EmTreeNodeEm32Device).MlStartDateTime1,
						(contextNode as EmTreeNodeEm32Device).MlEndDateTime1,
						(contextNode as EmTreeNodeEm32Device).MlStartDateTime2,
						(contextNode as EmTreeNodeEm32Device).MlEndDateTime2);

					wndOptionsDb.ShowDialog();
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ShowArchOptions(): " + ex.Message);
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
				// if buffer node is folder
				if (nodeToPaste.NodeType == EmTreeNodeType.Folder)
				{
					// drop node is subfolder
					if (nodeDestination.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folders.folder_id = {1};",
							((EmTreeNodeFolder)nodeDestination).FolderId,
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}

					// drop node is root
					else if (nodeDestination.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = 0, is_subfolder = false WHERE folders.folder_id = {0};",
							((EmTreeNodeFolder)nodeToPaste).FolderId);
					}
				}
				else if (nodeToPaste.NodeType == EmTreeNodeType.EM32Device)
				{
					// drop node is subfolder
					if (nodeDestination.NodeType == EmTreeNodeType.Folder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = {0}, is_subfolder = true WHERE folders.folder_id = {1};",
							((EmTreeNodeFolder)nodeDestination).FolderId,
							((EmTreeNodeEm32Device)nodeToPaste).FolderId);
					}

					// drop node is root
					else if (nodeDestination.NodeType == EmTreeNodeType.DeviceFolder)
					{
						commandText = String.Format("UPDATE folders SET parent_id = 0, is_subfolder = false WHERE folders.folder_id = {0};",
							((EmTreeNodeEm32Device)nodeToPaste).FolderId);
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
				EmService.WriteToLogFailed("Error in InsertFolder(): " + ex.Message);
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Function to add archives to the device
		/// </summary>
		protected void AddArchivesToDeviceFolder(EmTreeNodeEm32Device tnDevNode)
		{
			try
			{
				if (tnDevNode == null) return;

				string commandText = string.Empty;
				DbService dbService = new DbService(connectString_);
				if(!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}
				try
				{
					// trying to find PQP
					commandText = String.Format("SELECT count(*) AS count FROM day_avg_parameter_times WHERE device_id = {0};", tnDevNode.DeviceId);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM day_avg_parameter_times WHERE device_id = {0};", tnDevNode.DeviceId);
						dbService.ExecuteReader(commandText);

						PQPDatesList pqpList = new PQPDatesList();

						while (dbService.DataReaderRead())
						{
							pqpList.Add(new EmInfoForNodePQPClassic(ref dbService, EmDeviceType.EM32));
						}
						pqpList.SortItems();

						for (int iPqp = 0; iPqp < pqpList.Count; ++iPqp)
						{
							// добавляем в дерево архивов
							// ищем папку для года
							TreeNode tnYearNode = null;
							foreach (EmTreeNodeYearFolder tn in tnDevNode.Nodes)
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
											EmDeviceType.EM32,
											pqpList[iPqp].DtStart.Year.ToString(),
											tnDevNode, null);
								tnDevNode.Nodes.Add(tnYearNode);
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
											EmDeviceType.EM32,
											pqpList[iPqp].DtStart.Month.ToString(),
											tnDevNode, null);
								tnYearNode.Nodes.Add(tnMonthNode);
							}

							// ищем папку для ПКЭ
							EmTreeNodeDBMeasureType tnPQPNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.EM32);
							foreach (EmTreeNodeDBMeasureType tn in tnMonthNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.PQP, this, EmDeviceType.EM32))
								{
									tnPQPNode = tn;
									break;
								}
							}
							if (tnPQPNode == null)	// если папки нет, создаем ее
							{
								tnPQPNode = new EmTreeNodeDBMeasureType(MeasureType.PQP,
									EmDeviceType.EM32, tnDevNode, null);
								tnMonthNode.Nodes.Add(tnPQPNode);
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
									//new object[] { pqpList[iPqp].ml_start_time_1_, 
									//            pqpList[iPqp].ml_end_time_1_, 
									//            pqpList[iPqp].ml_start_time_2_, 
									//            pqpList[iPqp].ml_end_time_2_ },
									tnDevNode.DeviceVersion, tnDevNode.T_fliker,
									EmDeviceType.EM32,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlStartTime1,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlEndTime1,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlStartTime2,
									(pqpList[iPqp] as EmInfoForNodePQPClassic).MlEndTime2,
									tnDevNode.ConstraintType,
									tnDevNode, null);
								tnPQPNode.Nodes.Add(tnPQPArchNode);
							}
						}
					}

					// trying to find DNS
					commandText =
						String.Format(
						"SELECT count(*) AS count FROM dips_and_overs_times WHERE device_id = {0};", 
						tnDevNode.DeviceId);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = 
							String.Format("SELECT * FROM dips_and_overs_times WHERE device_id = {0};", 
							tnDevNode.DeviceId);
						dbService.ExecuteReader(commandText);

						DNSDatesList dnsList = new DNSDatesList();

						while (dbService.DataReaderRead())
						{
							dnsList.Add(new EmInfoForNodeDNSClassic(ref dbService, EmDeviceType.EM32));
						}
						dnsList.SortItems();

						for (int iDns = 0; iDns < dnsList.Count; ++iDns)
						{
							// добавляем в дерево архивов
							// ищем папку для года
							TreeNode tnYearNode = null;
							foreach (EmTreeNodeYearFolder tn in tnDevNode.Nodes)
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
											EmDeviceType.EM32,
											dnsList[iDns].DtStart.Year.ToString(),
											tnDevNode, null);
								tnDevNode.Nodes.Add(tnYearNode);
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
											EmDeviceType.EM32,
											dnsList[iDns].DtStart.Month.ToString(),
											tnDevNode, null);
								tnYearNode.Nodes.Add(tnMonthNode);
							}

							// ищем папку для DNS
							EmTreeNodeDBMeasureType tnDNSNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.EM32);
							foreach (EmTreeNodeDBMeasureType tn in tnMonthNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.DNS, this, EmDeviceType.EM32))
								{
									tnDNSNode = tn;
									break;
								}
							}
							if (tnDNSNode == null)	// если папки нет, создаем ее
							{
								tnDNSNode = new EmTreeNodeDBMeasureType(MeasureType.DNS,
												EmDeviceType.EM32, tnDevNode, null);
								tnMonthNode.Nodes.Add(tnDNSNode);
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
									//new object[] { dtMlStartDateTime1, 
									//            dtMlEndDateTime1, 
									//            dtMlStartDateTime2, 
									//            dtMlEndDateTime2 },
									tnDevNode.DeviceVersion, tnDevNode.T_fliker,
									EmDeviceType.EM32,
									tnDevNode.MlStartDateTime1, tnDevNode.MlEndDateTime1,
									tnDevNode.MlStartDateTime2, tnDevNode.MlEndDateTime2,
									tnDevNode.ConstraintType,
									tnDevNode, null);
								tnDNSNode.Nodes.Add(tnDNSArchNode);
							}
						}
					}

					// trying to find AVG
					commandText =
						String.Format("SELECT count(*) AS count FROM period_avg_params_times WHERE device_id = {0};", tnDevNode.DeviceId);
					if ((Int64)dbService.ExecuteScalar(commandText) > 0)
					{
						commandText = String.Format("SELECT * FROM period_avg_params_times WHERE device_id = {0};", tnDevNode.DeviceId);
						dbService.ExecuteReader(commandText);

						AVGDatesList avgList = new AVGDatesList();

						while (dbService.DataReaderRead())
						{
							avgList.Add(new EmInfoForNodeAVGClassic(ref dbService, EmDeviceType.EM32));
						}
						avgList.SortItems();

						for (int iAvg = 0; iAvg < avgList.Count; ++iAvg)
						{
							// добавляем в дерево архивов
							// ищем папку для года
							TreeNode tnYearNode = null;
							foreach (EmTreeNodeYearFolder tn in tnDevNode.Nodes)
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
											EmDeviceType.EM32,
											avgList[iAvg].DtStart.Year.ToString(),
											tnDevNode, null);
								tnDevNode.Nodes.Add(tnYearNode);
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
											EmDeviceType.EM32,
											avgList[iAvg].DtStart.Month.ToString(),
											tnDevNode, null);
								tnYearNode.Nodes.Add(tnMonthNode);
							}

							// ищем папку для AVG
							EmTreeNodeDBMeasureType tnAVGNode = null;
							string tmp = EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.EM32);
							foreach (EmTreeNodeDBMeasureType tn in tnMonthNode.Nodes)
							{
								if (tn.Text == EmTreeNodeDBMeasureType.GetText(MeasureType.AVG, this, EmDeviceType.EM32))
								{
									tnAVGNode = tn;
									break;
								}
							}
							if (tnAVGNode == null)	// если папки нет, создаем ее
							{
								tnAVGNode = new EmTreeNodeDBMeasureType(MeasureType.AVG,
												EmDeviceType.EM32, tnDevNode, null);
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
											EmDeviceType.EM32, tnDevNode, null);
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
									tnDevNode.DeviceVersion,
									tnDevNode.T_fliker,
									EmDeviceType.EM32,
									(avgList[iAvg] as EmInfoForNodeAVGClassic).AvgType,
									tnDevNode.ConstraintType,
									tnDevNode, null);

								tnAvgTypeNode.Nodes.Add(tnAVGArchNode);
							}
						}
					}
				}
				finally
				{
					if (dbService != null) dbService.CloseConnect();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddArchivesToDeviceFolderEm32(): ");
				throw;
			}
		}

		protected bool AddFoldersLevelToTree(Int64 parentFldId, EmArchNodeBase parentNode,
					string connectString)
		{
			DbService dbService = null;
			try
			{
				dbService = new DbService(connectString);
				dbService.Open();
				// выбираем из БД все вложенные папки
				string commandText = String.Format("SELECT * FROM folders f WHERE folder_type <> 3 AND parent_id = {0};", parentFldId);
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
																EmDeviceType.EM32, null, null);

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

				// выбираем из БД все вложенные устройства (для них рекурсивно не вызываем, т.к. у них
				// не может быть вложенных папок)
				commandText = String.Format("SELECT * FROM folders f INNER JOIN devices d ON f.device_id = d.dev_id AND d.parent_folder_id = f.folder_id AND f.folder_type = 3 AND f.parent_id = {0};", parentFldId);
				dbService.ExecuteReader(commandText);
				while (dbService.DataReaderRead())
				{
					EmTreeNodeEm32Device tnDevNode = new EmTreeNodeEm32Device();
					tnDevNode.SerialNumber = (Int64)dbService.DataReaderData("ser_number");

					// нумерация схем подключения не совпадает у 33 и 32, поэтому исправляем
					//????????????????????con_scheme
					short conSchemeTmp = (short)dbService.DataReaderData("con_scheme");
					ConnectScheme conSchemeRes = ConnectScheme.Unknown;
					if (conSchemeTmp == 0 || conSchemeTmp == 1 ||
						conSchemeTmp == 4 || conSchemeTmp == 5)	// 4пр
						conSchemeRes = ConnectScheme.Ph3W4;
					else if (conSchemeTmp == 2 || conSchemeTmp == 3)	// 3пр
						conSchemeRes = ConnectScheme.Ph3W3;
					tnDevNode.ConnectionScheme = conSchemeRes;

					tnDevNode.NominalLinearVoltage = (float)dbService.DataReaderData("u_nom_lin");
					tnDevNode.NominalPhaseVoltage = (float)dbService.DataReaderData("u_nom_ph");
					tnDevNode.NominalPhaseCurrent = (float)dbService.DataReaderData("i_nom_ph");
					tnDevNode.NominalFrequency = (float)dbService.DataReaderData("f_nom");
					tnDevNode.DeviceId = (Int64)dbService.DataReaderData("dev_id");
					tnDevNode.DeviceVersion = dbService.DataReaderData("dev_version") as string;
					tnDevNode.DeviceInfo = dbService.DataReaderData("dev_info") as string;
					tnDevNode.ObjectName = dbService.DataReaderData("object_name") as string;
					tnDevNode.ULimit = (float)dbService.DataReaderData("u_limit");
					tnDevNode.ILimit = (float)dbService.DataReaderData("i_limit");
					tnDevNode.CurrentTransducerIndex = (ushort)((short)dbService.DataReaderData("current_transducer_index"));
					tnDevNode.T_fliker = (short)dbService.DataReaderData("t_fliker");
					tnDevNode.ConstraintType = (short)dbService.DataReaderData("constraint_type");
					tnDevNode.FolderId = (Int64)dbService.DataReaderData("parent_folder_id");
					try { tnDevNode.MlStartDateTime1 = (DateTime)dbService.DataReaderData("ml_start_time_1"); }
					catch (InvalidCastException) { tnDevNode.MlStartDateTime1 = DateTime.MinValue; }
					try { tnDevNode.MlEndDateTime1 = (DateTime)dbService.DataReaderData("ml_end_time_1"); }
					catch (InvalidCastException) { tnDevNode.MlEndDateTime1 = DateTime.MinValue; }
					try { tnDevNode.MlStartDateTime2 = (DateTime)dbService.DataReaderData("ml_start_time_2"); }
					catch (InvalidCastException) { tnDevNode.MlStartDateTime2 = DateTime.MinValue; }
					try { tnDevNode.MlEndDateTime2 = (DateTime)dbService.DataReaderData("ml_end_time_2"); }
					catch (InvalidCastException) { tnDevNode.MlEndDateTime2 = DateTime.MinValue; }

					//tnDevNode.Text = tnDevNode.SerialNumber.ToString() + "  " + tnDevNode.ObjectName;
					tnDevNode.Text = dbService.DataReaderData("name") as string;

					// ищем папку для данного прибора (по серийному номеру)
					EmTreeNodeEm32Device tnDevNodeTemp = null;

					for (int iItem = 0; iItem < parentNode.Nodes.Count; ++iItem)
					{
						if (parentNode.Nodes[iItem] is EmTreeNodeEm32Device)
						{
							if (tnDevNode.SerialNumber ==
								(parentNode.Nodes[iItem] as EmTreeNodeEm32Device).SerialNumber)
							{
								tnDevNodeTemp = (EmTreeNodeEm32Device)parentNode.Nodes[iItem];
								break;
							}
						}
					}

					// добавляем папку прибора в дерево
					if (tnDevNodeTemp == null)
					{
						parentNode.Nodes.Add(tnDevNode);
						// добавляем в дерево архивы
						AddArchivesToDeviceFolder(tnDevNode);
					}
				}

				pgServerNode_.Expand();
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "AddFoldersLevelToTreeEm32 failed");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
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

				if (parentNode.Nodes.Count == 0 && !(parentNode is EmTreeNodeEm32Device)
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
