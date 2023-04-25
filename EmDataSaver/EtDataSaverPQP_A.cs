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
	public class EtDataSaverPQP_A : EmDataSaverBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		#endregion

		#region Fields

		private BackgroundWorker bw_ = null;

		private EtPQP_A_SqlDeviceImage sqlImage_;

		#endregion

		#region Properties

		public EtPQP_A_SqlDeviceImage SqlImage
		{
			get { return sqlImage_; }
			set { sqlImage_ = value; }
		}

		#endregion

		#region Constructors

		public EtDataSaverPQP_A(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			ref EtPQP_A_SqlDeviceImage sqlImage)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.pgSrvConnectStr_ = settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEtPQP_A;
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

				string commandText;
				string[] sqls = sqlImage_.Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				EmService.WriteToLogGeneral("pgInsert(): sqls.Length = " + sqls.Length.ToString());
				if (sqls.Length < 2)
				{
					EmService.WriteToLogFailed("pgInsert(): Invalid sqlImage!");
					return false;
				}

				commandText = sqls[0];		//insert_new_device
				EmService.WriteToLogGeneral(commandText);
				object o_dev_id = dbService.ExecuteScalar(commandText);
				long dev_id = (long)o_dev_id;

				commandText = sqls[1];		//insert_dev_folder
				EmService.WriteToLogGeneral(commandText);
				object o_folder_id = dbService.ExecuteScalar(commandText);
				long folder_id = (long)o_folder_id;

				EmService.WriteToLogGeneral("pgInsert(): Registrations.Length = " + sqlImage_.Registrations.Length.ToString());
				for (int iArch = 0; iArch < sqlImage_.Registrations.Length; iArch++)
				{
					// посылаем сообщение (Вставка в БД: Архив X из Y)
					if (OnSetCntArchives != null) OnSetCntArchives(sqlImage_.Registrations.Length, iArch + 1);

					sqls = sqlImage_.Registrations[iArch].Sql.Split(new string[] { "\n" }, 
										StringSplitOptions.RemoveEmptyEntries);

					if (sqls.Length < 3)
					{
						EmService.WriteToLogFailed("pgInsert(): Invalid sqlObject!");
						return false;
					}

					commandText = string.Format(sqls[0], dev_id);		//insert_new_registration
					object o_reg_id = dbService.ExecuteScalar(commandText);
					long reg_id = (long)o_reg_id;

					commandText = sqls[1];		//set registration constraints
					dbService.ExecuteNonQuery(commandText, true);

					commandText = string.Format(sqls[2], dev_id, reg_id);	//insert_reg_folder
					object o_regFolder_id = dbService.ExecuteScalar(commandText);
					long regFolder_id = (long)o_regFolder_id;

					#region AVG

					if (sqlImage_.Registrations[iArch].DataAVG != null)
					{
						for (int iArchPart = 0; iArchPart < sqlImage_.Registrations[iArch].DataAVG.Length; 
							iArchPart++)
						{
							if (sqlImage_.Registrations[iArch].DataAVG[iArchPart] == null) continue;

							string[] sqlsData = sqlImage_.Registrations[iArch].DataAVG[iArchPart].Sql.Split(
								new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							// insert into avg_times
							commandText = string.Format(sqlsData[0], reg_id, dev_id,
								regFolder_id);
							dbService.ExecuteNonQuery(commandText, true);

							string avgFileName = EmService.TEMP_IMAGE_DIR +
								sqlImage_.Registrations[iArch].DataAVG[iArchPart].AvgFileName;
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

					#endregion

					#region PQP

					if (sqlImage_.Registrations[iArch].DataPQP != null)
					{
						for (int iArchPart = 0; iArchPart < sqlImage_.Registrations[iArch].DataPQP.Length; iArchPart++)
						{
							if (sqlImage_.Registrations[iArch].DataPQP[iArchPart] == null) continue;

							string[] sqlsData = sqlImage_.Registrations[iArch].DataPQP[iArchPart].Sql.Split(
								new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							// insert into pqp_times
							commandText = string.Format(sqlsData[0], reg_id, dev_id, regFolder_id);
							dbService.ExecuteNonQuery(commandText, true);

							for (int iSqlData = 1; iSqlData < sqlsData.Length; ++iSqlData)
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

					#endregion

					#region DNS

					if (sqlImage_.Registrations[iArch].DataDNS != null)
					{
						for (int iArchPart = 0; iArchPart < sqlImage_.Registrations[iArch].DataDNS.Length; iArchPart++)
						{
							if (sqlImage_.Registrations[iArch].DataDNS[iArchPart] == null) continue;

							string[] sqlsData = sqlImage_.Registrations[iArch].DataDNS[iArchPart].Sql.Split(
										new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

							// insert into dns_times
							commandText = string.Format(sqlsData[0], reg_id, dev_id,
											regFolder_id);
							dbService.ExecuteNonQuery(commandText, true);

							for (int iSqlData = 1; iSqlData < sqlsData.Length; ++iSqlData)
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

					#endregion

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
