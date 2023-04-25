using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Threading;
using Npgsql;

namespace EmXPinitializer
{
	public partial class frmMain : Form
	{
		BackgroundSearcher myBackgroundSearcher = null;
		BackgroundWorker myBackGroundInit = null;
        bool searchStop = false;
		bool doAutomatic_;

		private static string logFileName_ = @"C:\log_init.txt";
		private static object logGeneralLock_ = new object();

		#region Constructors

		/// <summary>Constructor</summary>
		public frmMain( bool doAutomatic )
		{
            doAutomatic_ = doAutomatic;
			InitializeComponent();
		}

		#endregion

        bool PreinstallInit()
        {
            bool res = false;

            if (!File.Exists("em32_pg9011.backup"))
            {
                string msg;
                string header;
                

                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
                {
                    msg = "Не найден файл \"em32_pg9011.backup\".\nПожалуйста, убедитесь, что он находится в той же директории, что и программа\nи повсторите попытку";
                    header = "К сожалению";
                }
                else
                {
                    msg = "The \"em32_pg9011.backup\" file is not found.\nPlease make sure that the file is in the same directory as the Program and try again";
                    header = "Unfortunately";
                }

                if (!doAutomatic_)
                {
                    MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Visible = false;
                    Application.Exit();
                }
                else
                {                    
                    return res;
                }
            }

            if (!File.Exists("em33_pg9011.backup"))
            {
                string msg;
                string header;

                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
                {
                    msg = "Не найден файл \"em33_pg9011.backup\".\nПожалуйста, убедитесь, что он находится в той же директории, что и программа\nи повсторите попытку";
                    header = "К сожалению";
                }
                else
                {
                    msg = "The \"em33_pg9011.backup\" file is not found.\nPlease make sure that the file is in the same directory as the Program and try again";
                    header = "Unfortunately";
                }

                if (!doAutomatic_)
                {
                    MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Visible = false;
                    Application.Exit();
                }
                else
                {
                    return res;
                }
            }

            if (!File.Exists("et33_pg9011.backup"))
            {
                string msg;
                string header;

                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
                {
                    msg = "Не найден файл \"et33_pg9011.backup\".\nПожалуйста, убедитесь, что он находится в той же директории, что и программа\nи повсторите попытку";
                    header = "К сожалению";
                }
                else
                {
                    msg = "The \"et33_pg9011.backup\" file is not found.\nPlease make sure that the file is in the same directory as the Program and try again";
                    header = "Unfortunately";
                }

                if (!doAutomatic_)
                {
                    MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Visible = false;
                    Application.Exit();
                }
                else
                {
                    return res;
                }
            }

			if (!File.Exists("etpqp_a_pg9011.backup"))
            {
                string msg;
                string header;

                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
                {
					msg = "Не найден файл \"etpqp_a_pg9011.backup\".\nПожалуйста, убедитесь, что он находится в той же директории, что и программа\nи повсторите попытку";
                    header = "К сожалению";
                }
                else
                {
					msg = "The \"etpqp_a_pg9011.backup\" file is not found.\nPlease make sure that the file is in the same directory as the Program and try again";
                    header = "Unfortunately";
                }

                if (!doAutomatic_)
                {
                    MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Visible = false;
                    Application.Exit();
                }
                else
                {
                    return res;
                }
            }

            res = true;
            return res;

        }

        string getPSQLPath()
        {
            string path = "";

            DriveInfo[] drives = DriveInfo.GetDrives();
            FileInfo[] f_info_arr = null;

            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType != DriveType.Fixed) continue;
                DirectoryInfo dir_info = new DirectoryInfo(drive.RootDirectory.FullName);
                try
                {
                    f_info_arr = searchFileR(dir_info, "psql.exe");
                }
                catch (Exception ex)
                {
                    throw;
                }

                if (f_info_arr == null) continue;
                else break;
            }
            path = f_info_arr[0].DirectoryName;

            return path;
        }

        void DisableAllControls()
        {
            btnConnect.Enabled = false;
            btnScript.Enabled = false;
            btnStart.Enabled = false;
            btnStopAutoSearching.Enabled = false;            
            txtPort.Enabled = false;
            txtSU.Enabled = false;
            txtPSWD.Enabled = false;
            chbDbEm32.Enabled = false;
            chbDbEm33T.Enabled = false;
            chbDbEtPQP.Enabled = false;
			chbDbEtPQP_a.Enabled = false;
        }

        bool CheckAllowDBInit()
        {
            bool res = true;
            string connStr = String.Format("SERVER={0};Port={1};DATABASE=postgres;USER ID=postgres;PASSWORD={2};Encoding=SQL_ASCII", txtHost.Text, txtPort.Text, txtPSWD.Text);
            NpgsqlConnection connectPostgres = new NpgsqlConnection(connStr);
                                  
            try
            {
                connectPostgres.Open();

                NpgsqlDataReader dataReader = null;
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = connectPostgres;
                command.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = FALSE;";
                dataReader = command.ExecuteReader();
                List<string> dbNames = new List<string>();
				while (dataReader.Read())
                {
                    if (!(dataReader[0] is DBNull))
                    {
                        dbNames.Add(dataReader[0] as string);
                    }
                }
                if (dbNames.Contains("em_db") &&
                    dbNames.Contains("em32_db") &&
                    dbNames.Contains("et33_db") &&
					dbNames.Contains("etpqp_a_db"))
                {
                    res = false;
                }
            }
            catch (Exception ex)
            {
                string msg, caption;
                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
                {
                    msg = "Ошибка проверки существования баз данных. Инициализация прервана.";
                    caption = "Ошибка";
                }
                else
                {
                    msg = "Error checking databases for existance. Initialization interrupted.";
                    caption = "Error";
                }
                MessageBox.Show(this, msg, caption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                res = false;
            }
            finally
            {
                connectPostgres.Close();
            }

            return res;
        }

		private void frmMain_Load(object sender, EventArgs e)
		{
            bool pi = PreinstallInit();
            
            if (pi == true && doAutomatic_)
            {
				WriteToLogGeneral("automatic!");

                DisableAllControls();
                txtPSWD.Text = "postgres";

				this.Hide();
				
                string path = getPSQLPath();
                txtPgPath.Text = path + @"\";
                if ( path == null) // шеф, всё пропало!
                {
                    Application.Exit();
                }
                else
                {
                    if ( !CheckAllowDBInit() ) // check dbs are exist
                        Application.Exit();
                    if ( ConnectClick(true) )
                        btnStart_Click(this, null);
                }
            }
            else if ( pi && !doAutomatic_ )
            {
				WriteToLogGeneral("NOT automatic!");
                myBackgroundSearcher = new BackgroundSearcher();
                myBackgroundSearcher.WorkerSupportsCancellation = true;
                myBackgroundSearcher.DoWork += new DoWorkEventHandler(myBackgroundSearcher_DoWork);
                myBackgroundSearcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(myBackgroundSearcher_RunWorkerCompleted);

                myBackgroundSearcher.RunWorkerAsync();
            }			
		}

		#region Background Searcher

		private void myBackgroundSearcher_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (myBackgroundSearcher.FilePath.Length != 0)
			{
				txtPgPath.Text = myBackgroundSearcher.FilePath + @"\";
				pictureBoxSearching.Visible = false;
				txtSU.Enabled = txtPSWD.Enabled = btnConnect.Enabled = true;
				txtPSWD.Focus();
			}
			else
				if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					txtPgPath.Text = "Директория \"bin\" PostgreSQL не была найдена...";
				else
					txtPgPath.Text = "The PostgreSQL's \"bin\" directory is not found...";
		}

		private void myBackgroundSearcher_DoWork(object sender, DoWorkEventArgs e)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			FileInfo[] f_info_arr = null;

			foreach (DriveInfo drive in drives)
			{
                // cancel from other thread
                if (searchStop)
                {
                    myBackgroundSearcher.FilePath = string.Empty;
                    return;
                }

				if (drive.DriveType != DriveType.Fixed) continue;
				DirectoryInfo dir_info = new DirectoryInfo(drive.RootDirectory.FullName);
				try
				{
					f_info_arr = searchFileR(dir_info, "psql.exe");
				}
				catch (Exception ex)
				{
					throw;
				}

				if (f_info_arr == null) continue;
				else break;
			}

			if (f_info_arr != null) myBackgroundSearcher.FilePath = f_info_arr[0].DirectoryName;
			else myBackgroundSearcher.FilePath = string.Empty;
		}
		
		private FileInfo[] searchFileR(DirectoryInfo DirInfo, string Mask)
		{
            // cancel from other thread
            if (searchStop) return null;

			FileInfo[] f_info_arr = null;
			try
			{
				f_info_arr = DirInfo.GetFiles(Mask, SearchOption.TopDirectoryOnly);
				if (f_info_arr.Length < 1)
				{
					f_info_arr = null;
					DirectoryInfo[] dir_infos = DirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
					foreach (DirectoryInfo dir_info in dir_infos)
					{
						f_info_arr = searchFileR(dir_info, Mask);
						if (f_info_arr != null) break;
					}
				}
				return f_info_arr;
			}
			catch 
			{
				return null;
			}
		}

		private void btnStopAutoSearching_Click(object sender, EventArgs e)
		{
            searchStop = true;
			myBackgroundSearcher.CancelAsync();
			myBackgroundSearcher.Dispose();

			pictureBoxSearching.Visible = false;

			if (openFileDialog.ShowDialog() != DialogResult.OK) return;
			{
				txtPgPath.Text = openFileDialog.FileName.Remove(openFileDialog.FileName.Length - 8, 8);
				txtSU.Enabled = txtPSWD.Enabled = btnConnect.Enabled = true;
				txtPSWD.Focus();
			}
		}

		#endregion

		#region Database initialization

        private bool ConnectClick( bool doSilent )
        {
            bool res = false;
            // проверяем какие БД есть в наличии
            string connStr = String.Format("SERVER={0};Port={1};DATABASE=postgres;USER ID=postgres;PASSWORD={2};Encoding=SQL_ASCII", txtHost.Text, txtPort.Text, txtPSWD.Text);
            NpgsqlConnection connectPostgres = new NpgsqlConnection(connStr);
            try
            {
                connectPostgres.Open();
            }
            catch (Exception ex)
            {
                WriteToLogGeneral("Unable to connect to DB server! " + ex.Message);
                if (!doSilent)
                {
                    if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
                        MessageBox.Show(this, "Не удалось соединиться с сервером БД!");
                    else
                        MessageBox.Show(this, "Unable to connect to the database server!");
                }
                return res;
            }
            try
            {
                NpgsqlDataReader dataReader = null;
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = connectPostgres;
                command.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = FALSE;";
                dataReader = command.ExecuteReader();
                List<string> dbNames = new List<string>();
                while (dbService.DataReaderRead())
                {
                    if (!(dataReader[0] is DBNull))
                    {
                        dbNames.Add(dataReader[0] as string);
                    }
                }
                if (dbNames.Contains("em_db"))
                {
					if (!doSilent)
						chbDbEm33T.Enabled = true;
					else
						chbDbEm33T.Checked = false;
                }
                else
                {
                    chbDbEm33T.Checked = true;
                    if (!doSilent)
                        chbDbEm33T.Enabled = false;
                }
                if (dbNames.Contains("em32_db"))
                {
                    if (!doSilent)
                        chbDbEm32.Enabled = true;
					else
						chbDbEm32.Checked = false;
                }
                else
                {
                    chbDbEm32.Checked = true;
                    if (!doSilent)
                        chbDbEm32.Enabled = false;
                }
                if (dbNames.Contains("et33_db"))
                {
                    if (!doSilent)
                        chbDbEtPQP.Enabled = true;
					else
						chbDbEtPQP.Checked = false;
                }
                else
                {
                    chbDbEtPQP.Checked = true;
                    if (!doSilent)
                        chbDbEtPQP.Enabled = false;
                }
				if (dbNames.Contains("etpqp_a_db"))
				{
					if (!doSilent)
						chbDbEtPQP_a.Enabled = true;
					else
						chbDbEtPQP_a.Checked = false;
				}
				else
				{
					chbDbEtPQP_a.Checked = true;
					if (!doSilent)
						chbDbEtPQP_a.Enabled = false;
				}

                if (chbDbEm32.Checked || chbDbEm33T.Checked || 
					chbDbEtPQP.Checked || chbDbEtPQP_a.Checked)
                {
                    if (!doSilent)
                        btnStart.Enabled = true;

                    res = true;

                    if (!doSilent)
                    {
                        if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
                            MessageBox.Show(this, "Соединение выполнено успешно");
                        else
                            MessageBox.Show(this, "Connection is established successfully");
                    }
                    WriteToLogGeneral("Connection successful!");
                }
            }
            catch (Exception ex)
            {
                WriteToLogGeneral("Unable to get DB list!");
                if (!doSilent)
                {
                    if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
                        MessageBox.Show(this, "Не удалось получить список баз данных! " + ex.Message);
                    else
                        MessageBox.Show(this, "Unable to get to the database list!" + ex.Message);
                }
                return res;
            }
            finally
            {
                connectPostgres.Close();
            }

            return res;
        }

		private void btnConnect_Click(object sender, EventArgs e)
		{
            ConnectClick(false);
		}

		/// <summary>
		/// EmDb initialization method
		/// </summary>
		private void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				// сообщение об уничтожении БД
				//if (chbDbEm32.Enabled || chbDbEm33T.Enabled || chbDbEtPQP.Enabled)
                if (chbDbEm32.Checked || chbDbEm33T.Checked || 
					chbDbEtPQP.Checked || chbDbEtPQP_a.Checked)
				{
                    if (!doAutomatic_)
                    {
                        ResourceManager rm = new ResourceManager("EmXPinitializer.emstrings",
                                                                    sender.GetType().Assembly);
                        //string cap = rm.GetString("attention_caption");
                        //string mess = rm.GetString("warning_delete_db");
                        string cap = "Внимание";
                        string mess = "Если выбранные базы данных существуют, все данные в них будут уничтожены! Продолжить?";
                        DialogResult res = MessageBox.Show(sender as Form, mess, cap,
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (res != DialogResult.Yes)
                            return;
                    }
				}
				InitThread threadInit = new InitThread(txtHost.Text, txtPort.Text, txtPSWD.Text, 
					txtPgPath.Text,
					chbDbEm33T.Checked, chbDbEm32.Checked, chbDbEtPQP.Checked, chbDbEtPQP_a.Checked);
                threadInit.m_doSilent = doAutomatic_; // hide cmd windows
				threadInit.OnInitStarted += new InitThread.InitStartedHandler(frmMain_InitStarted);
				threadInit.OnInitFinished += new InitThread.InitFinishedHandler(frmMain_InitFinished);
				Thread thread = new Thread(threadInit.Run);
				thread.Start();
			}
			catch (Exception ex)
			{
				WriteToLogGeneral("Error btnStart_Click: " + ex.Message);
			}
		}

		private void allEnable(bool bEnable)
		{
            if (!doAutomatic_)
            {
                try
                {
                    btnStart.Enabled = bEnable;
                    btnConnect.Enabled = bEnable;
                    btnStopAutoSearching.Enabled = bEnable;
                    txtSU.Enabled = bEnable;
                    txtPSWD.Enabled = bEnable;

                    StatusStripInit.Visible = !bEnable;
                }
                catch (Exception ex)
                {
                    WriteToLogGeneral("Error allEnable: " + ex.Message);
                }
            }
		}

		private void frmMain_InitStarted()
		{
			try
			{
				if (doAutomatic_) return;

				if (this.InvokeRequired == false) // thread checking
				{
					tsProgressBar.Style = ProgressBarStyle.Marquee;
					allEnable(false);
				}
				else
				{
					// если нет, то вызываем нашу функцию из нужного потока
					InitThread.InitStartedHandler initStarted =
						new InitThread.InitStartedHandler(frmMain_InitStarted);
					this.Invoke(initStarted);
				}
			}
			catch (Exception ex)
			{
				WriteToLogGeneral("Error in frmMain_InitStarted(): " + ex.Message);
			}
		}

		private void frmMain_InitFinished()
		{
			try
			{
				if (doAutomatic_) return;

				if (this.InvokeRequired == false) // thread checking
				{
					tsProgressBar.Style = ProgressBarStyle.Blocks;
					allEnable(true);
				}
				else
				{
					// если нет, то вызываем нашу функцию из нужного потока
					InitThread.InitFinishedHandler initFinished =
						new InitThread.InitFinishedHandler(frmMain_InitFinished);
					this.Invoke(initFinished);
				}
			}
			catch (Exception ex)
			{
				WriteToLogGeneral("Error in frmMain_InitFinished(): " + ex.Message);
			}
		}

		#endregion

        private void btnScript_Click(object sender, EventArgs e)
        {
			try
			{
				if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

				List<string> cmds = new List<string>();

				cmds.Add("CREATE TRUSTED PROCEDURAL LANGUAGE 'plpgsql' HANDLER plpgsql_call_handler VALIDATOR plpgsql_validator;");
				cmds.Add(@"CREATE OR REPLACE FUNCTION add_em_user() 
			RETURNS void 
			AS $BODY$ 
			BEGIN 
			  if (select count(*) from pg_roles where rolname = 'energomonitor') > 0 
			    then if (select rolsuper from pg_roles where rolname = 'energomonitor') <> true 
			       then drop role energomonitor; 
			    end if; 
			  end if; 
			if (select count(*) from pg_roles where rolname = 'energomonitor') = 0 
			  then create role energomonitor SUPERUSER LOGIN ENCRYPTED PASSWORD '4emworknet4'; 
			end if; 
			RETURN; 
			END; $BODY$ LANGUAGE 'plpgsql' VOLATILE;");
				cmds.Add("select add_em_user();");
				cmds.Add("DROP LANGUAGE plpgsql CASCADE;");

				if (chbDbEm32.Checked)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "em32_pg9011.backup";

					cmds.Add("DROP DATABASE IF EXISTS em32_db;");
					cmds.Add("CREATE DATABASE em32_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'");
					cmds.Add(txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " + txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d em32_db -v \"" + pathToBackup + "\"");
				}
				if (chbDbEm33T.Checked)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "em33_pg9011.backup";

					cmds.Add("DROP DATABASE IF EXISTS em_db;");
					cmds.Add("CREATE DATABASE em_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'");
					cmds.Add(txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " + txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d em_db -v \"" + pathToBackup + "\"");
				}
				if (chbDbEtPQP.Checked)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "et33_pg9011.backup";

					cmds.Add("DROP DATABASE IF EXISTS et33_db;");
					cmds.Add("CREATE DATABASE et33_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'");
					cmds.Add(txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " + txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d et33_db -v \"" + pathToBackup + "\"");
				}
				if (chbDbEtPQP_a.Checked)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "etpqp_a_pg9011.backup";

					cmds.Add("DROP DATABASE IF EXISTS etpqp_a_db;");
					cmds.Add("CREATE DATABASE etpqp_a_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'");
					cmds.Add(txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " + txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d etpqp_a_db -v \"" + pathToBackup + "\"");
				}

				StreamWriter sw = new StreamWriter(saveFileDialog.FileName);
				foreach (string command in cmds)
				{
					sw.WriteLine(command);
				}
				sw.Close();
				System.Media.SystemSound beep = System.Media.SystemSounds.Beep;
				beep.Play();
			}
			catch (Exception ex)
			{
				WriteToLogGeneral("Error btnScript_Click: " + ex.Message);
			}
        }

		private void chbDb_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (!chbDbEm32.Checked && !chbDbEm33T.Checked && 
					!chbDbEtPQP.Checked && !chbDbEtPQP_a.Checked)
				{
					btnScript.Enabled = false;
					btnStart.Enabled = false;
				}
				else
				{
					btnScript.Enabled = true;
					btnStart.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				WriteToLogGeneral("Error chbDb_CheckedChanged: " + ex.Message);
			}
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}

		public static void WriteToLogGeneral(string s)
		{
			StreamWriter sw = null;
			try
			{
				System.Diagnostics.Debug.WriteLine(s);
				lock (logGeneralLock_)
				{
					try
					{
						sw = new StreamWriter(logFileName_, true);
						sw.WriteLine(s);
					}
					catch { }
					finally
					{
						try { if (sw != null) sw.Close(); }
						catch { }
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error in WriteToLogGeneral() " + ex.Message);
			}
		}
	}

	class BackgroundSearcher : BackgroundWorker
	{
		public String FilePath = String.Empty;
	}

	class InitThread
	{
		public delegate void InitStartedHandler();
		public event InitStartedHandler OnInitStarted;
		public delegate void InitFinishedHandler();
		public event InitFinishedHandler OnInitFinished;

		string host_;
		string port_;
		string pswd_;
		string pgPath_;
		bool bInitEm33T_;
		bool bInitEm32_;
		bool bInitEtPQP_;
		bool bInitEtPQP_a_;

        public bool m_doSilent;

		private static string logFileName_ = @"C:\log_init.txt";

		public InitThread(string h, string p, string pswd, string path,
			bool em33t, bool em32, bool etpqp, bool etpqp_a)
		{
			host_ = h; port_ = p; pswd_ = pswd; pgPath_ = path;
			bInitEm33T_ = em33t; bInitEm32_ = em32; bInitEtPQP_ = etpqp; bInitEtPQP_a_ = etpqp_a;
            m_doSilent = false;
		}

		public void Run()
		{
			if (OnInitStarted != null) OnInitStarted();

			//StringBuilder output = new StringBuilder();
			frmMain.WriteToLogGeneral("(c) Mars-Energo Ltd.");
			frmMain.WriteToLogGeneral("EmWorkNet database initialization");
			frmMain.WriteToLogGeneral("");

			bool res = true;

			string connStr = String.Format(
				"SERVER={0};Port={1};DATABASE=postgres;USER ID=postgres;PASSWORD={2};Encoding=UTF8",
				host_, port_, pswd_);
			NpgsqlConnection connect = new NpgsqlConnection(connStr);
			NpgsqlCommand command = new NpgsqlCommand();

			try
			{
				connect.Open();
				command.Connection = connect;

				// сначала создаем все требуемые БД
				if (bInitEm32_)
				{
					command.CommandText = "DROP DATABASE IF EXISTS em32_db;";
					command.ExecuteNonQuery();
					command.CommandText =
						"CREATE DATABASE em32_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'";
					command.ExecuteNonQuery();
					frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);
				}
				if (bInitEm33T_)
				{
					command.CommandText = "DROP DATABASE IF EXISTS em_db;";
					command.ExecuteNonQuery();
					command.CommandText =
						"CREATE DATABASE em_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'";
					command.ExecuteNonQuery();
					frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);
				}
				if (bInitEtPQP_)
				{
					command.CommandText = "DROP DATABASE IF EXISTS et33_db;";
					command.ExecuteNonQuery();
					command.CommandText =
						"CREATE DATABASE et33_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'";
					command.ExecuteNonQuery();
					frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);
				}
				if (bInitEtPQP_a_)
				{
					command.CommandText = "DROP DATABASE IF EXISTS etpqp_a_db;";
					command.ExecuteNonQuery();
					command.CommandText =
						"CREATE DATABASE etpqp_a_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8'";
					command.ExecuteNonQuery();
					frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);
				}

				try
				{
					command.CommandText = "CREATE TRUSTED PROCEDURAL LANGUAGE 'plpgsql' HANDLER plpgsql_call_handler VALIDATOR plpgsql_validator;";
					command.ExecuteNonQuery();
					frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);
				}
				catch (Exception ex)
				{
					frmMain.WriteToLogGeneral("Unable to create LANGUAGE 'plpgsql'");
				}

				command.CommandText =
@"CREATE OR REPLACE FUNCTION add_em_user() 
RETURNS void 
AS $BODY$ 
BEGIN 
  if (select count(*) from pg_roles where rolname = 'energomonitor') > 0 
    then if (select rolsuper from pg_roles where rolname = 'energomonitor') <> true 
       then drop role energomonitor; 
    end if; 
  end if; 
if (select count(*) from pg_roles where rolname = 'energomonitor') = 0 
  then create role energomonitor SUPERUSER LOGIN ENCRYPTED PASSWORD '4emworknet4'; 
end if; 
RETURN; 
END; $BODY$ LANGUAGE 'plpgsql' VOLATILE;";
				command.ExecuteNonQuery();
				frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);

				command.CommandText = "select add_em_user();";
				command.ExecuteNonQuery();
				frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);

				command.CommandText = "DROP LANGUAGE plpgsql CASCADE;";
				command.ExecuteNonQuery();
				frmMain.WriteToLogGeneral("Successful command: " + command.CommandText);
			}
			catch (Exception exc)
			{
				frmMain.WriteToLogGeneral("Error on command: " + command.CommandText);
				frmMain.WriteToLogGeneral(exc.Message);
				res = false;
			}
			finally
			{
				connect.Close();
			}

			try
			{

				#region EM33T

				if (bInitEm33T_)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "em33_pg9011.backup";

					//string restoreStr = txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " + 
					//    txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d em_db -v \"" + 
					//    pathToBackup + "\"";
					string restoreStr = "type pswd| \"" + pgPath_ + "pg_restore.exe\" -i -h " + host_ + " -p " + port_ + " -U postgres -d em_db -v \"" + pathToBackup + "\"";
					if (!RestoreDB(restoreStr)) res = false;
				}

				#endregion

				#region EM32

				if (bInitEm32_)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "em32_pg9011.backup";

					//string restoreStr = txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " +
					//    txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d em32_db -v \"" +
					//    pathToBackup + "\"";
					string restoreStr = "type pswd| \"" + pgPath_ + "pg_restore.exe\" -i -h " + host_ + " -p " + port_ + " -U postgres -d em32_db -v \"" + pathToBackup + "\"";
					if (!RestoreDB(restoreStr)) res = false;
				}

				#endregion

				#region ETPQP

				if (bInitEtPQP_)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "et33_pg9011.backup";

					//string restoreStr = txtPSWD.Text + " \"" + txtPgPath.Text + "pg_restore.exe\" -i -h " +
					//    txtHost.Text + " -p " + txtPort.Text + " -U postgres -W -d et33_db -v \"" +
					//    pathToBackup + "\"";
					//string restoreStr = "-i -h " + txtHost.Text + " -p " + txtPort.Text + " -U postgres -W " + txtPSWD.Text + " -d et33_db -v \"" + pathToBackup + "\"";
					string restoreStr = "type pswd| \"" + pgPath_ + "pg_restore.exe\" -i -h " + host_ + " -p " + port_ + " -U postgres -d et33_db -v \"" + pathToBackup + "\"";
					if (!RestoreDB(restoreStr)) res = false;
				}

				#endregion

				#region ETPQP-A

				frmMain.WriteToLogGeneral("region ETPQP-A");

				if (bInitEtPQP_a_)
				{
					string pathToBackup = AppDomain.CurrentDomain.BaseDirectory + "etpqp_a_pg9011.backup";

					string restoreStr = "type pswd| \"" + pgPath_ + "pg_restore.exe\" -i -h " + host_ + " -p " + port_ + " -U postgres -d etpqp_a_db -v \"" + pathToBackup + "\"";
					if (!RestoreDB(restoreStr)) res = false;
				}

				frmMain.WriteToLogGeneral("end of region ETPQP-A");

				#endregion

				//FileStream fLog = new FileStream(this.LogFileName, FileMode.Create);
				//StreamWriter swLog = new StreamWriter(fLog, System.Text.Encoding.Default);
				//swLog.Write(output.ToString());
				//swLog.Close();
				//fLog.Close();

				string msg = string.Empty;
				string header = string.Empty;

				if (res)
				{
					frmMain.WriteToLogGeneral("result true");

					if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					{
						msg = String.Format("Инициализация прошла успешно!\nФайл отчета: {0}", logFileName_);
						header = "Поздравления";
					}
					else
					{
						msg = String.Format(
							"Initialization has been successfully completed!\nLog file: {0}", logFileName_);
						header = "Congratulations";
					}
                    
					MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Information);                    
				}
				else
				{
					frmMain.WriteToLogGeneral("result false");

					if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("ru"))
					{
						msg = String.Format("Ошибка инициализации.\nФайл отчета: {0}", logFileName_);
						header = "К сожалению";
					}
					else
					{
						msg = String.Format("Initialization failure!\nLog file: {0}", logFileName_);
						header = "Unfortunately";
					}
					MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

                if (m_doSilent)
                    Application.Exit();
			}
			catch (Exception ex)
			{
				frmMain.WriteToLogGeneral("Error InitThread::Run: " + ex.Message);
			}
		}

		private bool RestoreDB(string input)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			try
			{
				//process.StartInfo.FileName = pgPath_ + "pg_restore.exe";
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Arguments = "/A /C " + input;
				process.StartInfo.WorkingDirectory = "";
				process.StartInfo.UseShellExecute = false;
				//process.StartInfo.CreateNoWindow = true;
				process.StartInfo.CreateNoWindow = m_doSilent;
				process.StartInfo.RedirectStandardOutput = true;
				frmMain.WriteToLogGeneral(
					"process.StartInfo.FileName: " + process.StartInfo.FileName.ToString());
				frmMain.WriteToLogGeneral(
					"process.StartInfo.Arguments: " + process.StartInfo.Arguments.ToString());

				frmMain.WriteToLogGeneral("Before process RestoreDB start");
				Thread.Sleep(1000);
				process.Start();
				//process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
				//string outputSrc = process.StandardOutput.ReadToEnd();
				//frmMain.WriteToLogGeneral(outputSrc);

				process.WaitForExit();
				//outputSrc = process.StandardOutput.ReadToEnd();
				//frmMain.WriteToLogGeneral(outputSrc);
				frmMain.WriteToLogGeneral("After process RestoreDB end");

				frmMain.WriteToLogGeneral("RestoreDB(): ExitCode = " + process.ExitCode);

				return (process.ExitCode == 0);
			}
			catch (Exception ex)
			{
				frmMain.WriteToLogGeneral("Error RestoreDB(): " + ex.Message);
				return false;
			}
			finally
			{
				if (OnInitFinished != null) OnInitFinished();
			}
		}
	}
}