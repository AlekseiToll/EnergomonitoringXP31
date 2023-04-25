using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.ComponentModel;

using EmServiceLib;
using EmServiceLib.SavingInterface;
using DeviceIO;
using DbServiceLib;
using EmDataSaver.XmlImage;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.SqlImage;

namespace EmDataSaver
{
	public class EmDataSaver32 : EmDataSaverBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		#endregion

		#region Fields

        private bool bAutoMode_;

		private BackgroundWorker bw_ = null;

		private EmSqlEm32Device sqlImage_;

		#endregion

		#region Properties

		public EmSqlEm32Device SqlImage
		{
			get { return sqlImage_; }
			set { sqlImage_ = value; }
		}

		#endregion

		#region Constructor

		public EmDataSaver32(
			object sender,
			Settings settings,
			//string sqlImageFileName,
			BackgroundWorker bw,
			ref EmSqlEm32Device sqlImage,
			bool autoMode)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.pgSrvConnectStr_ = settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEm32;
			this.pgHost_ = settings_.PgServers[settings_.CurServerIndex].PgHost;
			this.bw_ = bw;
			this.sqlImage_ = sqlImage;
			this.bAutoMode_ = autoMode;
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
					throw new EmException("EmDataSaver32::Run: sqlImage == null");

				e_ = e;
				e.Result = pgInsert();
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in EmDataSaver32::Run()");
				Thread.ResetAbort();
				e.Result = false;
				return;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in EmDataSaver32::Run(): " + emx.Message);
				e.Result = false;
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver32::Run():");
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
			string commandText = string.Empty;
			try
			{
				dbService.Open();

				string[] sqls = sqlImage_.Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

				if (sqls.Length < 2)
				{
					EmService.WriteToLogFailed("pgInsert(): Invalid sqlImage!");
					return false;
				}

				commandText = sqls[0];		//insert_new_device
				object o_dev_id = dbService.ExecuteScalar(commandText);
				long dev_id = (long)o_dev_id;

				commandText = sqls[1];		//insert_dev_folder
				object o_folder_id = dbService.ExecuteScalar(commandText);
				long folder_id = (long)o_folder_id;

				if (sqlImage_.DataPQP != null && sqlImage_.DataPQP.Length > 0)
				{
					for (int iArch = 0; iArch < sqlImage_.DataPQP.Length; ++iArch)
					{
						string[] sqlsData = sqlImage_.DataPQP[iArch].Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

						if (sqlsData.Length < 3)
							throw new EmException("pgInsert(): PQP sqlsData.Length < 3");

						// добавление папки года
						commandText = sqlsData[0];
						object o_folder_year_id = dbService.ExecuteScalar(commandText);
						long folder_year_id = (long)o_folder_year_id;
						// добавление папки месяца
						commandText = sqlsData[1];
						object o_folder_mo_id = dbService.ExecuteScalar(commandText);
						long folder_mo_id = (long)o_folder_mo_id;
						// insert into day_avg_parameter_times
						commandText = string.Format(sqlsData[2], dev_id, folder_year_id, folder_mo_id);
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
						}

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}
					}
				}

				if (sqlImage_.DataDNO != null && sqlImage_.DataDNO.Length > 0)
				{
					for (int iArch = 0; iArch < sqlImage_.DataDNO.Length; ++iArch)
					{
						string[] sqlsData = sqlImage_.DataDNO[iArch].Sql.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

						if (sqlsData.Length < 3)
							throw new EmException("pgInsert(): DNS sqlsData.Length < 3");

						// добавление папки года
						commandText = sqlsData[0];
						object o_folder_year_id = dbService.ExecuteScalar(commandText);
						long folder_year_id = (long)o_folder_year_id;
						// добавление папки месяца
						commandText = sqlsData[1];
						object o_folder_mo_id = dbService.ExecuteScalar(commandText);
						long folder_mo_id = (long)o_folder_mo_id;
						// insert into day_avg_parameter_times
						commandText = string.Format(sqlsData[2], dev_id, folder_year_id, folder_mo_id);
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
					}
				}

				if (sqlImage_.DataAVG != null && sqlImage_.DataAVG.Length > 0)
				{
					for (int iArch = 0; iArch < sqlImage_.DataAVG.Length; ++iArch)
					{
						string[] sqlsData = sqlImage_.DataAVG[iArch].Sql.Split(
							new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

						if (sqlsData.Length < 3)
							throw new EmException("pgInsert(): AVG sqlsData.Length < 3");

						// добавление папки года
						commandText = sqlsData[0];
						object o_folder_year_id = dbService.ExecuteScalar(commandText);
						long folder_year_id = (long)o_folder_year_id;
						// добавление папки месяца
						commandText = sqlsData[1];
						object o_folder_mo_id = dbService.ExecuteScalar(commandText);
						long folder_mo_id = (long)o_folder_mo_id;
						// insert into day_avg_parameter_times
						commandText = string.Format(sqlsData[2], dev_id, folder_year_id, 
																folder_mo_id);
						dbService.ExecuteNonQuery(commandText, true);

						string avgFileName = EmService.TEMP_IMAGE_DIR +
								sqlImage_.DataAVG[iArch].AvgFileName;
						if (!File.Exists(avgFileName))
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
