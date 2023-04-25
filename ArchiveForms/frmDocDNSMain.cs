// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Average values float-window form

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

using EmServiceLib;
using WeifenLuo.WinFormsUI;
using DbServiceLib;
using DataGridColumnStyles;

namespace EnergomonitoringXP
{
	public enum DNSColumns
	{
		START = 0,
		END = 1,
		DURATION = 2,
		EVENT = 3,
		PHASE = 4,
		U = 5,
		DEVIATION = 6,
		U_DECLARED = 7,
		FINISHED = 8,
		EARLIER = 9
	}

	/// <summary>
	/// Summary description for frmDoc2.
	/// </summary>
	public class frmDocDNSMain : DockContent
	{
		private EmDeviceType curDevType_;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Settings object
		/// </summary>
		private EmDataSaver.Settings settings_;

		/// <summary>
		/// Pointer to the main application window
		/// </summary>
		protected frmMain MainWindow_;

		/// <summary>
		/// Stuff
		/// </summary>
		ConnectScheme curConnectionScheme_ = 0;
		int curPgServerIndex_ = 0;
		private DataSet dataSetDNS;
		private DataTable dataTableDNS;
		private DataColumn dcolStart;
		public DataGridView dgvDNS;
		private DataColumn dcolEnd;
		private DataColumn dcolDuration;
		private DataColumn dcolEvent;
		private DataColumn dcolPhase;
		private DataColumn dcolU;
		private DataColumn dcolDeviation;
		private DataColumn dcolUDeclared;
		private DataColumn dcolFinished;
		private DataColumn dcolEarlier;
		private DataGridViewTextBoxColumn dgvcolStart;
		private DataGridViewTextBoxColumn dgvcolEnd;
		private DataGridViewTextBoxColumn dgvcolDuration;
		private DataGridViewTextBoxColumn dgvcolEvent;
		private DataGridViewTextBoxColumn dgvcolPhase;
		private DataGridViewTextBoxColumn dgvcolU;
		private DataGridViewTextBoxColumn dgvcolDeviation;
		private DataGridViewTextBoxColumn dgvcolUDeclared;
		private DataGridViewTextBoxColumn dgvcolFinished;
		private DataGridViewTextBoxColumn dgvcolEarlier;
		Int64 curDatetimeId_ = 0;

		/// <summary>
		/// Synchronize settings
		/// </summary>
		/// <param name="newSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings newSettings)
		{
			settings_ = newSettings.Clone();
		}

        /// <summary>
        /// Default construcror
        /// </summary>
		public frmDocDNSMain(frmMain mainWindow, EmDataSaver.Settings settings)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.settings_ = settings;
			this.MainWindow_ = mainWindow;			
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocDNSMain));
			this.dataSetDNS = new System.Data.DataSet();
			this.dataTableDNS = new System.Data.DataTable();
			this.dcolStart = new System.Data.DataColumn();
			this.dcolEnd = new System.Data.DataColumn();
			this.dcolDuration = new System.Data.DataColumn();
			this.dcolEvent = new System.Data.DataColumn();
			this.dcolPhase = new System.Data.DataColumn();
			this.dcolU = new System.Data.DataColumn();
			this.dcolDeviation = new System.Data.DataColumn();
			this.dcolUDeclared = new System.Data.DataColumn();
			this.dcolFinished = new System.Data.DataColumn();
			this.dcolEarlier = new System.Data.DataColumn();
			this.dgvDNS = new System.Windows.Forms.DataGridView();
			this.dgvcolStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolEnd = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolDuration = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolEvent = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolPhase = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolU = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolDeviation = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolUDeclared = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolFinished = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvcolEarlier = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dataSetDNS)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTableDNS)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvDNS)).BeginInit();
			this.SuspendLayout();
			// 
			// dataSetDNS
			// 
			this.dataSetDNS.DataSetName = "DataSetDNS";
			this.dataSetDNS.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTableDNS});
			// 
			// dataTableDNS
			// 
			this.dataTableDNS.Columns.AddRange(new System.Data.DataColumn[] {
            this.dcolStart,
            this.dcolEnd,
            this.dcolDuration,
            this.dcolEvent,
            this.dcolPhase,
            this.dcolU,
            this.dcolDeviation,
            this.dcolUDeclared,
            this.dcolFinished,
            this.dcolEarlier});
			this.dataTableDNS.TableName = "TableDNS";
			// 
			// dcolStart
			// 
			this.dcolStart.Caption = "Start";
			this.dcolStart.ColumnName = "colStart";
			// 
			// dcolEnd
			// 
			this.dcolEnd.Caption = "End";
			this.dcolEnd.ColumnName = "colEnd";
			// 
			// dcolDuration
			// 
			this.dcolDuration.Caption = "Duration";
			this.dcolDuration.ColumnName = "colDuration";
			// 
			// dcolEvent
			// 
			this.dcolEvent.Caption = "Event";
			this.dcolEvent.ColumnName = "colEvent";
			// 
			// dcolPhase
			// 
			this.dcolPhase.Caption = "Phase";
			this.dcolPhase.ColumnName = "colPhase";
			// 
			// dcolU
			// 
			this.dcolU.Caption = "U";
			this.dcolU.ColumnName = "colU";
			// 
			// dcolDeviation
			// 
			this.dcolDeviation.Caption = "Deviation";
			this.dcolDeviation.ColumnName = "colDeviation";
			// 
			// dcolUDeclared
			// 
			this.dcolUDeclared.Caption = "Unom";
			this.dcolUDeclared.ColumnName = "colUDeclared";
			// 
			// dcolFinished
			// 
			this.dcolFinished.Caption = "Finished";
			this.dcolFinished.ColumnName = "colFinished";
			// 
			// dcolEarlier
			// 
			this.dcolEarlier.Caption = "Begin Earlier";
			this.dcolEarlier.ColumnName = "colEarlier";
			// 
			// dgvDNS
			// 
			resources.ApplyResources(this.dgvDNS, "dgvDNS");
			this.dgvDNS.AllowUserToAddRows = false;
			this.dgvDNS.AllowUserToDeleteRows = false;
			this.dgvDNS.AllowUserToResizeRows = false;
			this.dgvDNS.AutoGenerateColumns = false;
			this.dgvDNS.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvDNS.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvcolStart,
            this.dgvcolEnd,
            this.dgvcolDuration,
            this.dgvcolEvent,
            this.dgvcolPhase,
            this.dgvcolU,
            this.dgvcolDeviation,
            this.dgvcolUDeclared,
            this.dgvcolFinished,
            this.dgvcolEarlier});
			this.dgvDNS.DataMember = "TableDNS";
			this.dgvDNS.DataSource = this.dataSetDNS;
			this.dgvDNS.EnableHeadersVisualStyles = false;
			this.dgvDNS.Name = "dgvDNS";
			this.dgvDNS.ReadOnly = true;
			this.dgvDNS.RowTemplate.Height = 24;
			this.dgvDNS.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			// 
			// dgvcolStart
			// 
			this.dgvcolStart.DataPropertyName = "colStart";
			resources.ApplyResources(this.dgvcolStart, "dgvcolStart");
			this.dgvcolStart.Name = "dgvcolStart";
			this.dgvcolStart.ReadOnly = true;
			// 
			// dgvcolEnd
			// 
			this.dgvcolEnd.DataPropertyName = "colEnd";
			resources.ApplyResources(this.dgvcolEnd, "dgvcolEnd");
			this.dgvcolEnd.Name = "dgvcolEnd";
			this.dgvcolEnd.ReadOnly = true;
			// 
			// dgvcolDuration
			// 
			this.dgvcolDuration.DataPropertyName = "colDuration";
			resources.ApplyResources(this.dgvcolDuration, "dgvcolDuration");
			this.dgvcolDuration.Name = "dgvcolDuration";
			this.dgvcolDuration.ReadOnly = true;
			// 
			// dgvcolEvent
			// 
			this.dgvcolEvent.DataPropertyName = "colEvent";
			resources.ApplyResources(this.dgvcolEvent, "dgvcolEvent");
			this.dgvcolEvent.Name = "dgvcolEvent";
			this.dgvcolEvent.ReadOnly = true;
			// 
			// dgvcolPhase
			// 
			this.dgvcolPhase.DataPropertyName = "colPhase";
			resources.ApplyResources(this.dgvcolPhase, "dgvcolPhase");
			this.dgvcolPhase.Name = "dgvcolPhase";
			this.dgvcolPhase.ReadOnly = true;
			// 
			// dgvcolU
			// 
			this.dgvcolU.DataPropertyName = "colU";
			resources.ApplyResources(this.dgvcolU, "dgvcolU");
			this.dgvcolU.Name = "dgvcolU";
			this.dgvcolU.ReadOnly = true;
			// 
			// dgvcolDeviation
			// 
			this.dgvcolDeviation.DataPropertyName = "colDeviation";
			resources.ApplyResources(this.dgvcolDeviation, "dgvcolDeviation");
			this.dgvcolDeviation.Name = "dgvcolDeviation";
			this.dgvcolDeviation.ReadOnly = true;
			// 
			// dgvcolUDeclared
			// 
			this.dgvcolUDeclared.DataPropertyName = "colUDeclared";
			resources.ApplyResources(this.dgvcolUDeclared, "dgvcolUDeclared");
			this.dgvcolUDeclared.Name = "dgvcolUDeclared";
			this.dgvcolUDeclared.ReadOnly = true;
			// 
			// dgvcolFinished
			// 
			this.dgvcolFinished.DataPropertyName = "colFinished";
			resources.ApplyResources(this.dgvcolFinished, "dgvcolFinished");
			this.dgvcolFinished.Name = "dgvcolFinished";
			this.dgvcolFinished.ReadOnly = true;
			// 
			// dgvcolEarlier
			// 
			this.dgvcolEarlier.DataPropertyName = "colEarlier";
			resources.ApplyResources(this.dgvcolEarlier, "dgvcolEarlier");
			this.dgvcolEarlier.Name = "dgvcolEarlier";
			this.dgvcolEarlier.ReadOnly = true;
			// 
			// frmDocDNSMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.dgvDNS);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.Document)));
			this.HideOnClose = true;
			this.Name = "frmDocDNSMain";
			this.ShowHint = WeifenLuo.WinFormsUI.DockState.Document;
			((System.ComponentModel.ISupportInitialize)(this.dataSetDNS)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTableDNS)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvDNS)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private void ClearData()
		{
			dataTableDNS.Clear();
		}

		/// <summary>
		/// Loading and showing Dips and Overvoltages data in toolbox window
		/// </summary>
		public void Open(int PgServerIndex, Int64 DateTimeID, ConnectScheme ConnectionScheme, float Nominal,
						EmDeviceType devType)
		{
			try
			{
				curConnectionScheme_ = ConnectionScheme;
				curDatetimeId_ = DateTimeID;
				curPgServerIndex_ = PgServerIndex;
				curDevType_ = devType;

				ClearData();

				this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Grid.NominalValue = Nominal;
				this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Clear();

				LoadData();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in frmDocDNSMain::Open(): " + ex.Message);
				throw;
			}
		}
		
		private void LoadData()
		{
			this.MainWindow_.wndDocDNS.wndDocDNSGraph.ConnectionScheme = curConnectionScheme_;

			string commandText = string.Empty;
			DbService dbService = null;
			if (curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K ||
				curDevType_ == EmDeviceType.EM33T1)
			{
				dbService = new DbService(
					settings_.PgServers[curPgServerIndex_].PgConnectionStringEm33);
			}
			else if(curDevType_ == EmDeviceType.EM32)
			{
				dbService = new DbService(
					settings_.PgServers[curPgServerIndex_].PgConnectionStringEm32);
			}
			else if (curDevType_ == EmDeviceType.ETPQP)
			{
				dbService = new DbService(
					settings_.PgServers[curPgServerIndex_].PgConnectionStringEtPQP);
			}
			else if (curDevType_ == EmDeviceType.ETPQP_A)
			{
				dbService = new DbService(
					settings_.PgServers[curPgServerIndex_].PgConnectionStringEtPQP_A);
			}

			try { dbService.Open(); }
			catch
			{
				MessageBoxes.DbConnectError(MainWindow_, dbService.Host, dbService.Port, dbService.Database);
				return;
			}

			try
			{
				if (curDevType_ == EmDeviceType.ETPQP_A)
				{
					if (curConnectionScheme_ == ConnectScheme.Ph3W4 ||
						curConnectionScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						commandText = String.Format("SELECT datetime_id, event_id, event_type, phase, u_value, d_u, u_declared, dt_start, dt_end, total_seconds, duration_millisec, duration_days, duration_hours, duration_min, duration_sec, is_finished, phase_num, is_earlier FROM dns_events WHERE datetime_id = {0} AND phase IN('A', 'B', 'C', 'ABCN') ORDER BY dt_start;", curDatetimeId_);
					}
					else if (curConnectionScheme_ == ConnectScheme.Ph3W3 ||
						curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
					{
						commandText = String.Format("SELECT datetime_id, event_id, event_type, phase, u_value, d_u, u_declared, dt_start, dt_end, total_seconds, duration_millisec, duration_days, duration_hours, duration_min, duration_sec, is_finished, phase_num, is_earlier FROM dns_events WHERE datetime_id = {0} AND phase IN('AB', 'BC', 'CA', 'ABC') ORDER BY dt_start;", curDatetimeId_);
					}
					else if (curConnectionScheme_ == ConnectScheme.Ph1W2)
					{
						commandText = String.Format("SELECT datetime_id, event_id, event_type, phase, u_value, d_u, u_declared, dt_start, dt_end, total_seconds, duration_millisec, duration_days, duration_hours, duration_min, duration_sec, is_finished, phase_num, is_earlier FROM dns_events WHERE datetime_id = {0} AND phase IN('A') ORDER BY dt_start;", curDatetimeId_);
					}
				}
				else if (curDevType_ == EmDeviceType.EM32 || curDevType_ == EmDeviceType.ETPQP)
				{
					if (curConnectionScheme_ == ConnectScheme.Ph3W4 ||
						curConnectionScheme_ == ConnectScheme.Ph3W4_B_calc)
					{
						commandText = String.Format("SELECT DISTINCT start_datetime, end_datetime, event_type, phase, deviation, is_finished, is_earlier FROM dips_and_overs WHERE datetime_id = {0} AND phase IN('A', 'B', 'C', 'ABCN') ORDER BY start_datetime;", curDatetimeId_);
					}
					else if (curConnectionScheme_ == ConnectScheme.Ph3W3 ||
						curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
					{
						commandText = String.Format("SELECT DISTINCT start_datetime, end_datetime, event_type, phase, deviation, is_finished, is_earlier FROM dips_and_overs WHERE datetime_id = {0} AND phase IN('AB', 'BC', 'CA', 'ABC') ORDER BY start_datetime;", curDatetimeId_);
					}
					else if (curConnectionScheme_ == ConnectScheme.Ph1W2)
					{
						commandText = String.Format("SELECT DISTINCT start_datetime, end_datetime, event_type, phase, deviation, is_finished, is_earlier FROM dips_and_overs WHERE datetime_id = {0} AND phase IN('A') ORDER BY start_datetime;", curDatetimeId_);
					}
				}
				else if (curDevType_ == EmDeviceType.EM33T || curDevType_ == EmDeviceType.EM31K ||
					curDevType_ == EmDeviceType.EM33T1)
				{
					if (curConnectionScheme_ == ConnectScheme.Ph1W2)
					{
						commandText = String.Format("SELECT DISTINCT start_datetime, end_datetime, event_type, phase, deviation FROM dips_and_overs WHERE datetime_id = {0} AND phase IN('A') ORDER BY start_datetime;", curDatetimeId_);
					}
					else
					{
						commandText = String.Format("SELECT DISTINCT start_datetime, end_datetime, event_type, phase, deviation FROM dips_and_overs WHERE datetime_id = {0} ORDER BY start_datetime;", curDatetimeId_);
					}
				}
				dbService.ExecuteReader(commandText);

				string datetime_format = string.Empty;
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
					datetime_format = "dd.MM.yyyy HH:mm:ss.FFF";
				else
					datetime_format = "MM/dd/yyyy HH:mm:ss.FFF";

				DataRow newRow;
				DateTime start_datetime, end_datetime;
				short event_type;
				string phase;
				TimeSpan duration;
				float deviation, u_value = 0, u_declared = 0;
				string dif_deviation;
				//bool isFinished = false, isEarlier = false;

				ResourceManager rm =
						new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

				string strYes = "yes";
				string strNo = "no";
				if (settings_.CurrentLanguage == "ru") { strYes = "да"; strNo = "нет"; }

				string prefix_dip = rm.GetString("columnheaders_pqp_dip");
				string prefix_swell = rm.GetString("columnheaders_pqp_swell");
				string unit_v = rm.GetString("column_header_units_v");

				while (dbService.DataReaderRead())
				{
					newRow = dataTableDNS.NewRow();
					event_type = (short)dbService.DataReaderData("event_type");
					phase = dbService.DataReaderData("phase") as String;

					if (curConnectionScheme_ == ConnectScheme.Ph3W3 || curConnectionScheme_ == ConnectScheme.Ph3W3_B_calc)
					{
						switch (phase)
						{
							case "A": phase = "AB"; break;
							case "B": phase = "BC"; break;
							case "C": phase = "CA"; break;
						}
					}

					if (curDevType_ != EmDeviceType.ETPQP_A)
					{
						start_datetime = (DateTime)dbService.DataReaderData("start_datetime");
						end_datetime = (DateTime)dbService.DataReaderData("end_datetime");
						newRow[(int)DNSColumns.START] = start_datetime.ToString(datetime_format);
						newRow[(int)DNSColumns.END] = end_datetime.ToString(datetime_format);

						duration =
							(TimeSpan)((DateTime)dbService.DataReaderData("end_datetime") - (DateTime)dbService.DataReaderData("start_datetime"));
						newRow[(int)DNSColumns.DURATION] = duration.ToString(@"d\.hh\:mm\:ss\.fff");

						newRow[(int)DNSColumns.EVENT] =
							event_type == 0 ? rm.GetString("name_params_swell") : rm.GetString("name_params_dip");
						newRow[(int)DNSColumns.PHASE] = phase;
						deviation = (float)dbService.DataReaderData("deviation");
						dif_deviation = String.Empty;
						if (event_type == 0) // перенапряжение
						{
							if (this.settings_.CurrentLanguage == "en")
								dif_deviation = ((1 + deviation) * 100).ToString() + "%";
							if (this.settings_.CurrentLanguage == "ru")
								dif_deviation = (1 + deviation).ToString();
						}
						else				// провал
						{
							if (this.settings_.CurrentLanguage == "en") dif_deviation =
										((1 - deviation) /* * 100*/).ToString() + "%";
							if (this.settings_.CurrentLanguage == "ru") dif_deviation =
										(deviation /* * 100*/).ToString() + "%";
						}

						if (event_type == 0)
							newRow[(int)DNSColumns.DEVIATION] = prefix_swell + " = " + dif_deviation;
						else newRow[(int)DNSColumns.DEVIATION] = prefix_dip + " = " + dif_deviation;

						if (curDevType_ == EmDeviceType.EM32 || curDevType_ == EmDeviceType.ETPQP)
						{
							newRow[(int)DNSColumns.FINISHED] = ((bool)dbService.DataReaderData("is_finished")) ? strYes : strNo;
							newRow[(int)DNSColumns.EARLIER] = ((bool)dbService.DataReaderData("is_earlier")) ? strYes : strNo;
						}
					}
					else   // Et-PQP-A
					{
						start_datetime = (DateTime)dbService.DataReaderData("dt_start");
						end_datetime = (DateTime)dbService.DataReaderData("dt_end");
						newRow[(int)DNSColumns.START] = start_datetime.ToString(datetime_format);
						newRow[(int)DNSColumns.END] = end_datetime.ToString(datetime_format);

						duration =
							(TimeSpan)((DateTime)dbService.DataReaderData("dt_end") - (DateTime)dbService.DataReaderData("dt_start"));
						newRow[(int)DNSColumns.DURATION] = duration.ToString(@"d\.hh\:mm\:ss\.fff");

						switch (event_type)
						{
							case 0: newRow[(int)DNSColumns.EVENT] = rm.GetString("name_params_dip"); break;
							case 1: newRow[(int)DNSColumns.EVENT] = rm.GetString("name_params_swell"); break;
							case 2: newRow[(int)DNSColumns.EVENT] = rm.GetString("name_params_interrupt"); break;
							default: EmService.WriteToLogFailed("DNS::LoadData: invalid type: " +
								event_type.ToString()); break;
						}

						newRow[4] = phase;

						deviation = (float)dbService.DataReaderData("d_u");
						//dif_deviation = String.Empty;
						//if (event_type == 1) // перенапряжение
						//{
						//    if (this.settings_.CurrentLanguage == "en") dif_deviation = 
						//        ((1 + deviation) * 100).ToString() + "%";
						//    if (this.settings_.CurrentLanguage == "ru") dif_deviation = (1 + deviation).ToString();
						//}
						//else				// провал
						//{
						//    if (this.settings_.CurrentLanguage == "en") dif_deviation =
						//                ((1 - deviation) /* * 100*/).ToString() + "%";
						//    if (this.settings_.CurrentLanguage == "ru") dif_deviation =
						//                (deviation /* * 100*/).ToString() + "%";
						//}
						if (event_type == 1)
							newRow[(int)DNSColumns.DEVIATION] =
								prefix_swell + " = " + deviation.ToString(new CultureInfo("en-US")) + '%';
						else newRow[(int)DNSColumns.DEVIATION] =
							prefix_dip + " = " + deviation.ToString(new CultureInfo("en-US")) + '%';

						u_value = (float)dbService.DataReaderData("u_value");
						newRow[(int)DNSColumns.U] = u_value.ToString(new CultureInfo("en-US")) + ' ' + unit_v;

						u_declared = (float)dbService.DataReaderData("u_declared");
						newRow[(int)DNSColumns.U_DECLARED] =
							u_declared.ToString(new CultureInfo("en-US")) + ' ' + unit_v;

						newRow[(int)DNSColumns.FINISHED] = ((bool)dbService.DataReaderData("is_finished")) ? strYes : strNo;
						newRow[(int)DNSColumns.EARLIER] = ((bool)dbService.DataReaderData("is_earlier")) ? strYes : strNo;
					}

					dataTableDNS.Rows.Add(newRow);

					// difining Phase
					EmGraphLib.DNS.Phase EmGraphPhase;
					if (phase.Equals("A"))
						EmGraphPhase = EmGraphLib.DNS.Phase.A;
					else if (phase.Equals("B"))
						EmGraphPhase = EmGraphLib.DNS.Phase.B;
					else if (phase.Equals("C"))
						EmGraphPhase = EmGraphLib.DNS.Phase.C;
					else if (phase.Equals("AB"))
						EmGraphPhase = EmGraphLib.DNS.Phase.AB;
					else if (phase.Equals("BC"))
						EmGraphPhase = EmGraphLib.DNS.Phase.BC;
					else if (phase.Equals("CA"))
						EmGraphPhase = EmGraphLib.DNS.Phase.CA;
					else if (phase.Equals("ABC"))
						EmGraphPhase = EmGraphLib.DNS.Phase.ABC;
					else if (phase.Equals("ABCN"))
						EmGraphPhase = EmGraphLib.DNS.Phase.ABCN;
					else
						continue;

					// adding items to the graph
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Add(
						start_datetime, duration, event_type == 0 ? deviation : -deviation, EmGraphPhase);
				}

				if (curDevType_ == EmDeviceType.EM32 || curDevType_ == EmDeviceType.ETPQP ||
					curDevType_ == EmDeviceType.ETPQP_A)
				{
					dgvDNS.Columns[(int)DNSColumns.FINISHED].Visible = true;
					dgvDNS.Columns[(int)DNSColumns.EARLIER].Visible = true;
				}
				else
				{
					dgvDNS.Columns[(int)DNSColumns.FINISHED].Visible = false;
					dgvDNS.Columns[(int)DNSColumns.EARLIER].Visible = false;
				}
				if (curDevType_ != EmDeviceType.ETPQP_A)
				{
					dgvDNS.Columns[(int)DNSColumns.U].Visible = false;
					dgvDNS.Columns[(int)DNSColumns.U_DECLARED].Visible = false;
				}
				else
				{
					dgvDNS.Columns[(int)DNSColumns.U].Visible = true;
					dgvDNS.Columns[(int)DNSColumns.U_DECLARED].Visible = true;
				}

				if (curDevType_ != EmDeviceType.ETPQP_A)
					commandText = String.Format("SELECT start_datetime, end_datetime FROM dips_and_overs_times WHERE datetime_id = {0};", curDatetimeId_);
				else
					commandText = String.Format("SELECT start_datetime, end_datetime FROM dns_times WHERE datetime_id = {0};", curDatetimeId_);

				dbService.ExecuteReader(commandText);
				if (dbService.DataReaderRead())
				{
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.TickMin =
						((DateTime)dbService.DataReaderData("start_datetime")).Ticks;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.TickMax =
						((DateTime)dbService.DataReaderData("end_datetime")).Ticks;
					//this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.VisibleTickStart =
					//((DateTime)dbService.DataReaderData("start_datetime"]).Ticks; 
					//this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.VisibleTickEnd =
					//((DateTime)dbService.DataReaderData("end_datetime"]).Ticks;
				}

				this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.HighlightTicks =
					((DateTime)dbService.DataReaderData(0)).Ticks;

				if (curConnectionScheme_ == ConnectScheme.Ph1W2)
				{
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseA.Checked = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseB.Checked = false;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseC.Checked = false;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseA.Enabled = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseB.Enabled = false;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseC.Enabled = false;
				}
				else
				{
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseA.Checked = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseB.Checked = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseC.Checked = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseA.Enabled = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseB.Enabled = true;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.tsbPhaseC.Enabled = true;
				}
				this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Highlight.Visible = true;
				this.MainWindow_.wndDocDNS.wndDocDNSGraph.selectPhasesToDraw();

			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDocDNSMain.LoadData()");
				throw;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		#region Grid changing state effects with DNOGraph

		private void dgvDNS_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
			try
			{
				if (this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Count != 0)
				{
					/*
					if (this.MainWindow.wndDocDNS.wndDocDNSGraph.tsbPhaseA.Checked &&
						this.MainWindow.wndDocDNS.wndDocDNSGraph.tsbPhaseB.Checked &&
						this.MainWindow.wndDocDNS.wndDocDNSGraph.tsbPhaseC.Checked)
					{
						//this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Highlight.Visible = true;
						this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.HighlightTicks = Convert.ToDateTime(myDataGrid[0, e.RowIndex].Value).Ticks;
						this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.RedrawHighlight();
					}
					else
					{
						//this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Highlight.Visible = false;
						this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Invalidate();					
					}
					*/
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.HighlightTicks =
						Convert.ToDateTime(dgvDNS[0, e.RowIndex].Value).Ticks;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.RedrawHighlight();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvDNS_RowEnter(): " + ex.Message);
				throw;
			}
        }

		private void dgvDNS_Sorted(object sender, EventArgs e)
		{
			try
			{
				if (this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Count != 0)
				{
					//this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.HighlightIndex = e.RowIndex;
					this.MainWindow_.wndDocDNS.wndDocDNSGraph.dnsGraphMain.RedrawHighlight();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dgvDNS_Sorted(): " + ex.Message);
				throw;
			}
		}

		#endregion

	}
}
