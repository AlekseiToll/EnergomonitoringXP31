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
	public class EtDataSaverPQP : EmDataSaverBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		#endregion

		#region Fields

		private BackgroundWorker bw_ = null;

		private EtPQPSqlDeviceImage sqlImage_;

		#endregion

		#region Properties

		public EtPQPSqlDeviceImage SqlImage
		{
			get { return sqlImage_; }
			set { sqlImage_ = value; }
		}

		#endregion

		#region Constructors

		public EtDataSaverPQP(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			ref EtPQPSqlDeviceImage sqlImage)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.pgSrvConnectStr_ = settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEtPQP;
			this.pgHost_ = settings_.PgServers[settings_.CurServerIndex].PgHost;
			this.bw_ = bw;
			this.sqlImage_ = sqlImage;
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
				if (sqlImage_ == null)
					throw new EmException("EtDataSaverPQP::Run: sqlImage == null");

				e_ = e;
				e.Result = pgInsert();
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in EtDataSaverPQP::Run()");
				Thread.ResetAbort();
				e.Result = false;
				return;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in EtDataSaverPQP::Run(): " + emx.Message);
				e.Result = false;
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDataSaverPQP::Run():");
				e.Result = false;
				throw;
			}
		}

		#endregion

		#region Private PostgreSQL Inserting methods

		private bool pgInsert()
		{
			DbService dbService = new DbService(pgSrvConnectStr_);
			EmService.WriteToLogGeneral("pgInsert(): " + DateTime.Now.ToString());

			try
			{
				dbService.Open();

				string[] sqls = sqlImage_.Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

				if (sqls.Length < 2)
				{
					EmService.WriteToLogFailed("pgInsert(): Invalid sqlImage!");
					return false;
				}

				string commandText = sqls[0];		//insert_new_device
				object o_dev_id = dbService.ExecuteScalar(commandText);
				long dev_id = (long)o_dev_id;

				commandText = sqls[1];		//insert_dev_folder
				object o_folder_id = dbService.ExecuteScalar(commandText);
				long folder_id = (long)o_folder_id;

				for (int iArch = 0; iArch < sqlImage_.Objects.Length; iArch++)
				{
					// посылаем сообщение (Вставка в БД: Архив X из Y)
					if (OnSetCntArchives != null) OnSetCntArchives(sqlImage_.Objects.Length, iArch + 1);

					sqls = sqlImage_.Objects[iArch].Sql.Split(new string[] { "\n" }, 
										StringSplitOptions.RemoveEmptyEntries);

					if (sqls.Length < 2)
					{
						EmService.WriteToLogFailed("pgInsert(): Invalid sqlObject!");
						return false;
					}

					commandText = string.Format(sqls[0], dev_id);		//insert_new_object
					object o_obj_id = dbService.ExecuteScalar(commandText);
					long obj_id = (long)o_obj_id;

					commandText = string.Format(sqls[1], dev_id, obj_id);	//insert_obj_folder
					object o_objFolder_id = dbService.ExecuteScalar(commandText);
					long objFolder_id = (long)o_objFolder_id;

					// обработка вставки данных Усредненных значений
					if (sqlImage_.Objects[iArch].DataAVG != null)
					{
						for (int iArchPart = 0; iArchPart < sqlImage_.Objects[iArch].DataAVG.Length; iArchPart++)
						{
							string[] sqlsData = sqlImage_.Objects[iArch].DataAVG[iArchPart].Sql.Split(
								new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							if (sqlsData.Length < 3)
								throw new EmException("pgInsert(): AVG sqlsData.Length < 3");

							// добавление папки года
							commandText = string.Format(sqlsData[0], obj_id, dev_id);
							object o_folder_year_id = dbService.ExecuteScalar(commandText);
							long folder_year_id = (long)o_folder_year_id;
							// добавление папки месяца
							commandText = string.Format(sqlsData[1], obj_id, dev_id);
							object o_folder_mo_id = dbService.ExecuteScalar(commandText);
							long folder_mo_id = (long)o_folder_mo_id;
							// insert into period_avg_params_times
							commandText = string.Format(sqlsData[2], obj_id, dev_id, 
								folder_year_id, folder_mo_id);
							dbService.ExecuteNonQuery(commandText, true);

							string avgFileName = EmService.TEMP_IMAGE_DIR +
								sqlImage_.Objects[iArch].DataAVG[iArchPart].AvgFileName;
							if(!File.Exists(avgFileName))
							{
								EmService.WriteToLogFailed("Temporary AVG file '" +
									avgFileName + "' doesn't exist!");
								break;
							}

							FileStream fsAvg = null;
							StreamReader srAvg = null;
							try
							{
								fsAvg = new FileStream(avgFileName, FileMode.Open);
								srAvg = new StreamReader(fsAvg);

								while (srAvg.Peek() >= 0)
								{
									try
									{
										commandText = srAvg.ReadLine();
										dbService.ExecuteNonQuery(commandText, true);
									}
									catch (Exception exAvg)
									{
										EmService.WriteToLogFailed("Error in pgInsert():  " +
											commandText);
										EmService.DumpException(exAvg, "Error in pgInsert():  ");
										throw exAvg;
									}

									if (bw_.CancellationPending)
									{
										e_.Cancel = true;
										return false;
									}
								}

							}
							finally
							{
								if (srAvg != null) srAvg.Close();
								if (fsAvg != null) fsAvg.Close();
								if (File.Exists(avgFileName)) File.Delete(avgFileName);
							}		
						}
					}
					// ПКЭ
					if (sqlImage_.Objects[iArch].DataPQP != null)
					{
						for (int iArchPart = 0; iArchPart < sqlImage_.Objects[iArch].DataPQP.Length; iArchPart++)
						{
							string[] sqlsData = sqlImage_.Objects[iArch].DataPQP[iArchPart].Sql.Split(
								new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							if (sqlsData.Length < 3)
								throw new EmException("pgInsert(): PQP sqlsData.Length < 3");

							// добавление папки года
							commandText = string.Format(sqlsData[0], obj_id, dev_id);
							object o_folder_year_id = dbService.ExecuteScalar(commandText);
							long folder_year_id = (long)o_folder_year_id;
							// добавление папки месяца
							commandText = string.Format(sqlsData[1], obj_id, dev_id);
							object o_folder_mo_id = dbService.ExecuteScalar(commandText);
							long folder_mo_id = (long)o_folder_mo_id;
							// insert into day_avg_parameter_times
							commandText = string.Format(sqlsData[2], obj_id, dev_id, 
								folder_year_id, folder_mo_id);
							dbService.ExecuteNonQuery(commandText, true);

							for (int iSqlData = 3; iSqlData < sqlsData.Length; ++iSqlData)
							{
								try
								{
									commandText = sqlsData[iSqlData];
									dbService.ExecuteNonQuery(commandText, true);
								}
								catch (Exception exPqp)
								{
									EmService.WriteToLogFailed("Error in pgInsert():  " +
										commandText);
									EmService.DumpException(exPqp, "Error in pgInsert():  ");
									throw exPqp;
								}

								if (bw_.CancellationPending)
								{
									e_.Cancel = true;
									return false;
								}
							}
						}
					}
					// DNS
					if (sqlImage_.Objects[iArch].DataDNS != null)
					{
						for (int iArchPart = 0; iArchPart < sqlImage_.Objects[iArch].DataDNS.Length; iArchPart++)
						{
							string[] sqlsData = sqlImage_.Objects[iArch].DataDNS[iArchPart].Sql.Split(
										new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							if (sqlsData.Length < 3)
								throw new EmException("pgInsert(): DNS sqlsData.Length < 3");

							// добавление папки года
							commandText = string.Format(sqlsData[0], obj_id, dev_id);
							object o_folder_year_id = dbService.ExecuteScalar(commandText);
							long folder_year_id = (long)o_folder_year_id;
							// добавление папки месяца
							commandText = string.Format(sqlsData[1], obj_id, dev_id);
							object o_folder_mo_id = dbService.ExecuteScalar(commandText);
							long folder_mo_id = (long)o_folder_mo_id;
							// insert into day_avg_parameter_times
							commandText = string.Format(sqlsData[2], obj_id, dev_id, 
											folder_year_id, folder_mo_id);
							dbService.ExecuteNonQuery(commandText, true);

							for (int iSqlData = 3; iSqlData < sqlsData.Length; ++iSqlData)
							{
								try
								{
									commandText = sqlsData[iSqlData];
									dbService.ExecuteNonQuery(commandText, true);
								}
								catch (Exception exDns)
								{
									EmService.WriteToLogFailed("Error in pgInsert():  " +
										commandText);
									EmService.DumpException(exDns, "Error in pgInsert():  ");
									throw exDns;
								}

								if (bw_.CancellationPending)
								{
									e_.Cancel = true;
									return false;
								}
							}

							//commandText =
							//     string.Format(sqlImg.Objects[iArch].DataDNS[iArchPart].Sql, obj_id);
							//dbService.ExecuteNonQuery(commandText);

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
					}
					//}

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
	}
}
