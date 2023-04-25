// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Database popup-window form

using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;

using DbServiceLib;
using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmOptionsDb.
	/// </summary>
	public class frmOptionsDb : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pbDatabase;
		private System.Windows.Forms.GroupBox gbDbOptions;
		private System.Windows.Forms.TextBox txtDbStartTime;
		private System.Windows.Forms.Label lblDbStartTime;
		private System.Windows.Forms.Label lblConnectionScheme;
		private System.Windows.Forms.TextBox txtConnectionScheme;
		private System.Windows.Forms.Label lblNominalFrequency;
		private System.Windows.Forms.TextBox txtNominalFrequency;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.GroupBox grbCurrentTransformer;
		private System.Windows.Forms.CheckBox chkCurrentTrPresent;
		private System.Windows.Forms.GroupBox grbVoltageTransformer;
		private System.Windows.Forms.TextBox txtCPrimaryWinding;
		private System.Windows.Forms.Label lblCPrimaryWinding;
		private System.Windows.Forms.TextBox txtCSecondaryWinding;
		private System.Windows.Forms.Label lblCSecondaryWinding;
		private System.Windows.Forms.TextBox txtCGradeOfOccuracy;
		private System.Windows.Forms.Label lblCGradeOfOccuracy;
		private System.Windows.Forms.TextBox txtCNumbers;
		private System.Windows.Forms.Label lblCNumbers;
		private System.Windows.Forms.TextBox txtVNumbers;
		private System.Windows.Forms.Label lblVNumbers;
		private System.Windows.Forms.TextBox txtVGradeOfOccuracy;
		private System.Windows.Forms.TextBox txtVSecondaryWinding;
		private System.Windows.Forms.Label lblVSecondaryWinding;
		private System.Windows.Forms.TextBox txtVPrimaryWinding;
		private System.Windows.Forms.Label lblVPrimaryWinding;
		private System.Windows.Forms.CheckBox chkVoltageTrPresent;
		private System.Windows.Forms.Label lblVGradeOfOccuracy;
		private System.Windows.Forms.TextBox txtCTurnRatio;
		private System.Windows.Forms.Label lblCTurnRatio;
		private System.Windows.Forms.TextBox txtVTurnRatio;
		private System.Windows.Forms.Label lblVTurnRatio;
		private System.Windows.Forms.Label lblEnergyQualityRations;
		private System.Windows.Forms.Label lblAverageValues;
		private System.Windows.Forms.Label lblDipsAndOvervoltages;
		private System.Windows.Forms.Label lblDipsAndOvervoltagesTime;
		private System.Windows.Forms.Label lblAverageValuesTime;
		private System.Windows.Forms.Label lblEnergyQualityRationsTime;
		private System.Windows.Forms.Label lblNominalLinVoltage;
		private System.Windows.Forms.Label lblDeviceInfo;
		private System.Windows.Forms.TextBox txtNominalLinVoltage;
		private IContainer components;
		private Label lblNominalPhVoltage;
		private TextBox txtNominalPhVoltage;
		private TextBox txtDeviceInfo;
		private ToolTip myToolTip;

		private Int64 iDatabaseId_;
		private int iPgServerIndex_;
		private string connectString_;
		private EmDeviceType devType_;
		private GroupBox gbGPS;
		private TextBox tbLongitude;
		private TextBox tbLatitude;
		private Label labelLongitude;
		private Label labelLatitude;
		private LinkLabel linkLabelMap;

		private bool ERROR_FLAG = false;

		public frmOptionsDb(
			int PgServerIndex,
			string connectString,
			DateTime dtStart, DateTime dtEnd,
			string strConSch,
			float fULinNom, float fUPhNom, float fFNom,
			string strDbName, string strDbInfo,
			Int64 iDeviceId,
			EmDeviceType devType,
			string strDeviceName, string strDeviceVersion,
			Int64 ser_num,
			Int64 iDbId)
		{
			iDatabaseId_ = iDbId;
			iPgServerIndex_ = PgServerIndex;
			connectString_ = connectString;
			devType_ = devType;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			#region data from node

			txtDbStartTime.Text = dtStart.ToString() + " - " + dtEnd.ToString();
			txtConnectionScheme.Text = strConSch;
			txtNominalLinVoltage.Text = fULinNom.ToString();
			txtNominalPhVoltage.Text = fUPhNom.ToString();
            txtNominalFrequency.Text = fFNom.ToString();

			gbGPS.Enabled = false;

			#endregion

			#region device info

			if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
				devType == EmDeviceType.EM33T1)
			{
				txtDeviceInfo.Text = String.Format("{0} # {1} (v.{2})",
								strDeviceName, iDeviceId, strDeviceVersion);
			}
			else if (devType == EmDeviceType.ETPQP)
			{
				txtDeviceInfo.Text = String.Format("{0} #{1} (v.{2})", strDeviceName, ser_num, strDeviceVersion);
			}

			#endregion

			DbService dbService = new DbService(connectString_);
			try
			{
				if (!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}

				object dts, dte;
				string commandText = string.Empty;

				if (devType == EmDeviceType.EM33T || devType == EmDeviceType.EM31K ||
					devType == EmDeviceType.EM33T1)
				{
					#region EM33T, EM31K

					#region pqp

					commandText = "SELECT min(start_datetime) FROM day_avg_parameter_times WHERE database_id = " + iDbId.ToString() + ";";
					dts = dbService.ExecuteScalar(commandText);
					if (!(dts is DBNull))
					{
						commandText = "SELECT max(end_time) FROM day_avg_parameter_times WHERE database_id = " + iDbId.ToString() + ";";
						dte = dbService.ExecuteScalar(commandText);
						if (!(dte is DBNull))
						{
							lblEnergyQualityRationsTime.Text = dts.ToString() + " - " + dte.ToString();
						}
					}

					#endregion

					#region avg

					commandText = "SELECT min(start_datetime) FROM period_avg_params_times WHERE database_id = " + iDbId.ToString() + ";";
					dts = dbService.ExecuteScalar(commandText);
					if (!(dts is DBNull))
					{
						commandText = "SELECT max(end_datetime) FROM period_avg_params_times WHERE database_id = " + iDbId.ToString() + ";";
						dte = dbService.ExecuteScalar(commandText);
						if (!(dte is DBNull))
						{
							lblAverageValuesTime.Text = dts.ToString() + " - " + dte.ToString();
						}
					}

					#endregion

					#region dno

					commandText = "SELECT min(start_datetime) FROM dips_and_overs_times WHERE database_id = " + iDbId.ToString() + ";";
					dts = dbService.ExecuteScalar(commandText);
					if (!(dts is DBNull))
					{
						commandText = "SELECT max(end_datetime) FROM dips_and_overs_times WHERE database_id = " + iDbId.ToString() + ";";
						dte = dbService.ExecuteScalar(commandText);
						if (!(dte is DBNull))
						{
							lblDipsAndOvervoltagesTime.Text = dts.ToString() + " - " + dte.ToString();
						}
					}

					#endregion

					#region turn rations

					commandText = "SELECT * FROM turn_ratios WHERE database_id = " + iDbId.ToString() + ";";
					dbService.ExecuteReader(commandText);
					if (dbService != null) dbService.CloseConnect();

					while (dbService.DataReaderRead())
					{
						short iType = (short)dbService.DataReaderData("type");
						string strSerialNum = dbService.DataReaderData("serial_num") as string;
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");
						float fGradeOfAccuracy = (float)dbService.DataReaderData("grade_of_accuracy");

						switch (iType)
						{
							case 1:
								chkVoltageTrPresent.CheckState = CheckState.Checked;
								txtVPrimaryWinding.Text = fValue1.ToString();
								txtVSecondaryWinding.Text = fValue2.ToString();
								txtVGradeOfOccuracy.Text = fGradeOfAccuracy.ToString();
								txtVNumbers.Text = strSerialNum.Trim();
								txtVTurnRatio.Text = (fValue1 / fValue2).ToString();
								break;

							case 2:
								chkCurrentTrPresent.CheckState = CheckState.Checked;
								txtCPrimaryWinding.Text = fValue1.ToString();
								txtCSecondaryWinding.Text = fValue2.ToString();
								txtCGradeOfOccuracy.Text = fGradeOfAccuracy.ToString();
								txtCNumbers.Text = strSerialNum.Trim();
								txtCTurnRatio.Text = (fValue1 / fValue2).ToString();


								break;
						}
					}

					#endregion

					#endregion
				}

				#region ETPQP

				if (devType == EmDeviceType.ETPQP)
				{

					#region pqp

					commandText = "SELECT min(start_datetime) FROM day_avg_parameter_times WHERE object_id = " + iDbId.ToString() + ";";
					dts = dbService.ExecuteScalar(commandText);
					if (!(dts is DBNull))
					{
						commandText = "SELECT max(end_time) FROM day_avg_parameter_times WHERE object_id = " + iDbId.ToString() + ";";
						dte = dbService.ExecuteScalar(commandText);
						if (!(dte is DBNull))
						{
							lblEnergyQualityRationsTime.Text = dts.ToString() + " - " + dte.ToString();
						}
					}

					#endregion

					#region avg

					commandText = "SELECT min(start_datetime) FROM period_avg_params_times WHERE object_id = " + iDbId.ToString() + ";";
					dts = dbService.ExecuteScalar(commandText);
					if (!(dts is DBNull))
					{
						commandText = "SELECT max(end_datetime) FROM period_avg_params_times WHERE object_id = " + iDbId.ToString() + ";";
						dte = dbService.ExecuteScalar(commandText);
						if (!(dte is DBNull))
						{
							lblAverageValuesTime.Text = dts.ToString() + " - " + dte.ToString();
						}
					}

					#endregion

					#region dno

					commandText = "SELECT min(start_datetime) FROM dips_and_overs_times WHERE object_id = " + iDbId.ToString() + ";";
					dts = dbService.ExecuteScalar(commandText);
					if (!(dts is DBNull))
					{
						commandText = "SELECT max(end_datetime) FROM dips_and_overs_times WHERE object_id = " + iDbId.ToString() + ";";
						dte = dbService.ExecuteScalar(commandText);
						if (!(dte is DBNull))
						{
							lblDipsAndOvervoltagesTime.Text = dts.ToString() + " - " + dte.ToString();
						}
					}

					#endregion

					#region turn rations

					commandText = "SELECT * FROM turn_ratios WHERE object_id = " + iDbId.ToString() + ";";
					dbService.ExecuteReader(commandText);

					while (dbService.DataReaderRead())
					{
						short iType = (short)dbService.DataReaderData("turn_type");
						string strSerialNum = dbService.DataReaderData("serial_num") as string;
						float fValue1 = (float)dbService.DataReaderData("value1");
						float fValue2 = (float)dbService.DataReaderData("value2");
						float fGradeOfAccuracy = (float)dbService.DataReaderData("grade_of_accuracy");

						switch (iType)
						{
							case 1:
								chkVoltageTrPresent.CheckState = CheckState.Checked;
								txtVPrimaryWinding.Text = fValue1.ToString();
								txtVSecondaryWinding.Text = fValue2.ToString();
								txtVGradeOfOccuracy.Text = fGradeOfAccuracy.ToString();
								txtVNumbers.Text = strSerialNum.Trim();
								txtVTurnRatio.Text = (fValue1 / fValue2).ToString();
								break;

							case 2:
								chkCurrentTrPresent.CheckState = CheckState.Checked;
								txtCPrimaryWinding.Text = fValue1.ToString();
								txtCSecondaryWinding.Text = fValue2.ToString();
								txtCGradeOfOccuracy.Text = fGradeOfAccuracy.ToString();
								txtCNumbers.Text = strSerialNum.Trim();
								txtCTurnRatio.Text = (fValue1 / fValue2).ToString();


								break;
						}
					}

					#endregion

				}

				#endregion
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		public frmOptionsDb(
			int PgServerIndex,
			string connectString,
			DateTime dtStart, DateTime dtEnd,
			string strConSch,
			float fULinNom, float fUPhNom, float fFNom,
			string strDbName, string strDbInfo,
			Int64 iDeviceId,
			EmDeviceType devType,
			string strDeviceName, string strDeviceVersion,
			Int64 ser_num,
			Int64 iDbId,
			bool U_transformer_enable, short U_transformer_type, short I_transformer_usage,
			int I_transformer_primary, short I_transformer_secondary,
			double latitude, double longitude)
		{
			this.iDatabaseId_ = iDbId;
			this.iPgServerIndex_ = PgServerIndex;
			this.connectString_ = connectString;
			this.devType_ = devType;

			InitializeComponent();

			#region data from node

			txtDbStartTime.Text = dtStart.ToString() + " - " + dtEnd.ToString();
			txtConnectionScheme.Text = strConSch;
			txtNominalLinVoltage.Text = fULinNom.ToString();
			txtNominalPhVoltage.Text = fUPhNom.ToString();
			txtNominalFrequency.Text = fFNom.ToString();

			// GPS
			tbLatitude.Text = latitude.ToString();
			tbLongitude.Text = longitude.ToString();
			linkLabelMap.Links.Clear();
			linkLabelMap.Links.Add(0, linkLabelMap.Text.Length, 
				string.Format("https://www.google.ru/maps?q={0},{1}",
				latitude.ToString(new CultureInfo("en-US")),
				longitude.ToString(new CultureInfo("en-US")) ));

			if (latitude == 0 || longitude == 0 || tbLatitude.Text.Length == 0 || tbLongitude.Text.Length == 0)
				gbGPS.Enabled = false;
			else gbGPS.Enabled = true;

			#endregion

			txtDeviceInfo.Text = String.Format("{0} #{1} (v.{2})", strDeviceName, ser_num, strDeviceVersion);

			DbService dbService = new DbService(connectString_);
			try
			{
				if (!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}

				object dts, dte;
				string commandText = string.Empty;

				#region pqp

				commandText = "SELECT min(start_datetime) FROM pqp_times WHERE registration_id = " + iDbId.ToString() + ";";
				dts = dbService.ExecuteScalar(commandText);
				if (!(dts is DBNull))
				{
					commandText = "SELECT max(end_time) FROM pqp_times WHERE registration_id = " + iDbId.ToString() + ";";
					dte = dbService.ExecuteScalar(commandText);
					if (!(dte is DBNull))
					{
						lblEnergyQualityRationsTime.Text = dts.ToString() + " - " + dte.ToString();
					}
				}

				#endregion

				#region avg

				commandText = "SELECT min(start_datetime) FROM avg_times WHERE registration_id = " + iDbId.ToString() + ";";
				dts = dbService.ExecuteScalar(commandText);
				if (!(dts is DBNull))
				{
					commandText = "SELECT max(end_datetime) FROM avg_times WHERE registration_id = " + iDbId.ToString() + ";";
					dte = dbService.ExecuteScalar(commandText);
					if (!(dte is DBNull))
					{
						lblAverageValuesTime.Text = dts.ToString() + " - " + dte.ToString();
					}
				}

				#endregion

				#region dns

				commandText = "SELECT min(start_datetime) FROM dns_times WHERE registration_id = " + iDbId.ToString() + ";";
				dts = dbService.ExecuteScalar(commandText);
				if (!(dts is DBNull))
				{
					commandText = "SELECT max(end_datetime) FROM dns_times WHERE registration_id = " + iDbId.ToString() + ";";
					dte = dbService.ExecuteScalar(commandText);
					if (!(dte is DBNull))
					{
						lblDipsAndOvervoltagesTime.Text = dts.ToString() + " - " + dte.ToString();
					}
				}

				#endregion

				#region turn rations

				if (I_transformer_usage == 1 || I_transformer_usage == 2)
				{
					txtCPrimaryWinding.Text = I_transformer_primary.ToString();
					txtCSecondaryWinding.Text = I_transformer_secondary.ToString();
				}

				if (U_transformer_enable)
				{
					txtVPrimaryWinding.Text = "1:" +
						DeviceIO.EtPQP_A_Device.GetUTransformerMultiplier(U_transformer_type).ToString();
				}

				#endregion
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOptionsDb));
			this.gbDbOptions = new System.Windows.Forms.GroupBox();
			this.gbGPS = new System.Windows.Forms.GroupBox();
			this.tbLongitude = new System.Windows.Forms.TextBox();
			this.tbLatitude = new System.Windows.Forms.TextBox();
			this.labelLongitude = new System.Windows.Forms.Label();
			this.labelLatitude = new System.Windows.Forms.Label();
			this.linkLabelMap = new System.Windows.Forms.LinkLabel();
			this.txtDeviceInfo = new System.Windows.Forms.TextBox();
			this.lblDeviceInfo = new System.Windows.Forms.Label();
			this.txtNominalLinVoltage = new System.Windows.Forms.TextBox();
			this.lblNominalPhVoltage = new System.Windows.Forms.Label();
			this.txtNominalPhVoltage = new System.Windows.Forms.TextBox();
			this.lblNominalFrequency = new System.Windows.Forms.Label();
			this.txtNominalFrequency = new System.Windows.Forms.TextBox();
			this.lblNominalLinVoltage = new System.Windows.Forms.Label();
			this.lblConnectionScheme = new System.Windows.Forms.Label();
			this.txtConnectionScheme = new System.Windows.Forms.TextBox();
			this.txtDbStartTime = new System.Windows.Forms.TextBox();
			this.lblDbStartTime = new System.Windows.Forms.Label();
			this.pbDatabase = new System.Windows.Forms.PictureBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.grbCurrentTransformer = new System.Windows.Forms.GroupBox();
			this.txtCTurnRatio = new System.Windows.Forms.TextBox();
			this.lblCTurnRatio = new System.Windows.Forms.Label();
			this.txtCNumbers = new System.Windows.Forms.TextBox();
			this.lblCNumbers = new System.Windows.Forms.Label();
			this.txtCGradeOfOccuracy = new System.Windows.Forms.TextBox();
			this.lblCGradeOfOccuracy = new System.Windows.Forms.Label();
			this.txtCSecondaryWinding = new System.Windows.Forms.TextBox();
			this.lblCSecondaryWinding = new System.Windows.Forms.Label();
			this.txtCPrimaryWinding = new System.Windows.Forms.TextBox();
			this.lblCPrimaryWinding = new System.Windows.Forms.Label();
			this.chkCurrentTrPresent = new System.Windows.Forms.CheckBox();
			this.grbVoltageTransformer = new System.Windows.Forms.GroupBox();
			this.txtVNumbers = new System.Windows.Forms.TextBox();
			this.lblVNumbers = new System.Windows.Forms.Label();
			this.txtVGradeOfOccuracy = new System.Windows.Forms.TextBox();
			this.lblVGradeOfOccuracy = new System.Windows.Forms.Label();
			this.txtVSecondaryWinding = new System.Windows.Forms.TextBox();
			this.lblVSecondaryWinding = new System.Windows.Forms.Label();
			this.txtVPrimaryWinding = new System.Windows.Forms.TextBox();
			this.lblVPrimaryWinding = new System.Windows.Forms.Label();
			this.txtVTurnRatio = new System.Windows.Forms.TextBox();
			this.lblVTurnRatio = new System.Windows.Forms.Label();
			this.chkVoltageTrPresent = new System.Windows.Forms.CheckBox();
			this.lblEnergyQualityRations = new System.Windows.Forms.Label();
			this.lblAverageValues = new System.Windows.Forms.Label();
			this.lblDipsAndOvervoltages = new System.Windows.Forms.Label();
			this.lblDipsAndOvervoltagesTime = new System.Windows.Forms.Label();
			this.lblAverageValuesTime = new System.Windows.Forms.Label();
			this.lblEnergyQualityRationsTime = new System.Windows.Forms.Label();
			this.myToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.gbDbOptions.SuspendLayout();
			this.gbGPS.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbDatabase)).BeginInit();
			this.grbCurrentTransformer.SuspendLayout();
			this.grbVoltageTransformer.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbDbOptions
			// 
			resources.ApplyResources(this.gbDbOptions, "gbDbOptions");
			this.gbDbOptions.Controls.Add(this.gbGPS);
			this.gbDbOptions.Controls.Add(this.txtDeviceInfo);
			this.gbDbOptions.Controls.Add(this.lblDeviceInfo);
			this.gbDbOptions.Controls.Add(this.txtNominalLinVoltage);
			this.gbDbOptions.Controls.Add(this.lblNominalPhVoltage);
			this.gbDbOptions.Controls.Add(this.txtNominalPhVoltage);
			this.gbDbOptions.Controls.Add(this.lblNominalFrequency);
			this.gbDbOptions.Controls.Add(this.txtNominalFrequency);
			this.gbDbOptions.Controls.Add(this.lblNominalLinVoltage);
			this.gbDbOptions.Controls.Add(this.lblConnectionScheme);
			this.gbDbOptions.Controls.Add(this.txtConnectionScheme);
			this.gbDbOptions.Controls.Add(this.txtDbStartTime);
			this.gbDbOptions.Controls.Add(this.lblDbStartTime);
			this.gbDbOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.gbDbOptions.Name = "gbDbOptions";
			this.gbDbOptions.TabStop = false;
			this.myToolTip.SetToolTip(this.gbDbOptions, resources.GetString("gbDbOptions.ToolTip"));
			// 
			// gbGPS
			// 
			resources.ApplyResources(this.gbGPS, "gbGPS");
			this.gbGPS.Controls.Add(this.tbLongitude);
			this.gbGPS.Controls.Add(this.tbLatitude);
			this.gbGPS.Controls.Add(this.labelLongitude);
			this.gbGPS.Controls.Add(this.labelLatitude);
			this.gbGPS.Controls.Add(this.linkLabelMap);
			this.gbGPS.Name = "gbGPS";
			this.gbGPS.TabStop = false;
			this.myToolTip.SetToolTip(this.gbGPS, resources.GetString("gbGPS.ToolTip"));
			// 
			// tbLongitude
			// 
			resources.ApplyResources(this.tbLongitude, "tbLongitude");
			this.tbLongitude.Name = "tbLongitude";
			this.tbLongitude.ReadOnly = true;
			this.myToolTip.SetToolTip(this.tbLongitude, resources.GetString("tbLongitude.ToolTip"));
			// 
			// tbLatitude
			// 
			resources.ApplyResources(this.tbLatitude, "tbLatitude");
			this.tbLatitude.Name = "tbLatitude";
			this.tbLatitude.ReadOnly = true;
			this.myToolTip.SetToolTip(this.tbLatitude, resources.GetString("tbLatitude.ToolTip"));
			// 
			// labelLongitude
			// 
			resources.ApplyResources(this.labelLongitude, "labelLongitude");
			this.labelLongitude.Name = "labelLongitude";
			this.myToolTip.SetToolTip(this.labelLongitude, resources.GetString("labelLongitude.ToolTip"));
			// 
			// labelLatitude
			// 
			resources.ApplyResources(this.labelLatitude, "labelLatitude");
			this.labelLatitude.Name = "labelLatitude";
			this.myToolTip.SetToolTip(this.labelLatitude, resources.GetString("labelLatitude.ToolTip"));
			// 
			// linkLabelMap
			// 
			resources.ApplyResources(this.linkLabelMap, "linkLabelMap");
			this.linkLabelMap.Name = "linkLabelMap";
			this.linkLabelMap.TabStop = true;
			this.myToolTip.SetToolTip(this.linkLabelMap, resources.GetString("linkLabelMap.ToolTip"));
			this.linkLabelMap.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelMap_LinkClicked);
			// 
			// txtDeviceInfo
			// 
			resources.ApplyResources(this.txtDeviceInfo, "txtDeviceInfo");
			this.txtDeviceInfo.Name = "txtDeviceInfo";
			this.txtDeviceInfo.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtDeviceInfo, resources.GetString("txtDeviceInfo.ToolTip"));
			// 
			// lblDeviceInfo
			// 
			resources.ApplyResources(this.lblDeviceInfo, "lblDeviceInfo");
			this.lblDeviceInfo.Name = "lblDeviceInfo";
			this.myToolTip.SetToolTip(this.lblDeviceInfo, resources.GetString("lblDeviceInfo.ToolTip"));
			// 
			// txtNominalLinVoltage
			// 
			resources.ApplyResources(this.txtNominalLinVoltage, "txtNominalLinVoltage");
			this.txtNominalLinVoltage.Name = "txtNominalLinVoltage";
			this.txtNominalLinVoltage.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtNominalLinVoltage, resources.GetString("txtNominalLinVoltage.ToolTip"));
			// 
			// lblNominalPhVoltage
			// 
			resources.ApplyResources(this.lblNominalPhVoltage, "lblNominalPhVoltage");
			this.lblNominalPhVoltage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNominalPhVoltage.Name = "lblNominalPhVoltage";
			this.myToolTip.SetToolTip(this.lblNominalPhVoltage, resources.GetString("lblNominalPhVoltage.ToolTip"));
			// 
			// txtNominalPhVoltage
			// 
			resources.ApplyResources(this.txtNominalPhVoltage, "txtNominalPhVoltage");
			this.txtNominalPhVoltage.Name = "txtNominalPhVoltage";
			this.txtNominalPhVoltage.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtNominalPhVoltage, resources.GetString("txtNominalPhVoltage.ToolTip"));
			// 
			// lblNominalFrequency
			// 
			resources.ApplyResources(this.lblNominalFrequency, "lblNominalFrequency");
			this.lblNominalFrequency.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNominalFrequency.Name = "lblNominalFrequency";
			this.myToolTip.SetToolTip(this.lblNominalFrequency, resources.GetString("lblNominalFrequency.ToolTip"));
			// 
			// txtNominalFrequency
			// 
			resources.ApplyResources(this.txtNominalFrequency, "txtNominalFrequency");
			this.txtNominalFrequency.Name = "txtNominalFrequency";
			this.txtNominalFrequency.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtNominalFrequency, resources.GetString("txtNominalFrequency.ToolTip"));
			// 
			// lblNominalLinVoltage
			// 
			resources.ApplyResources(this.lblNominalLinVoltage, "lblNominalLinVoltage");
			this.lblNominalLinVoltage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNominalLinVoltage.Name = "lblNominalLinVoltage";
			this.myToolTip.SetToolTip(this.lblNominalLinVoltage, resources.GetString("lblNominalLinVoltage.ToolTip"));
			// 
			// lblConnectionScheme
			// 
			resources.ApplyResources(this.lblConnectionScheme, "lblConnectionScheme");
			this.lblConnectionScheme.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblConnectionScheme.Name = "lblConnectionScheme";
			this.myToolTip.SetToolTip(this.lblConnectionScheme, resources.GetString("lblConnectionScheme.ToolTip"));
			// 
			// txtConnectionScheme
			// 
			resources.ApplyResources(this.txtConnectionScheme, "txtConnectionScheme");
			this.txtConnectionScheme.Name = "txtConnectionScheme";
			this.txtConnectionScheme.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtConnectionScheme, resources.GetString("txtConnectionScheme.ToolTip"));
			// 
			// txtDbStartTime
			// 
			resources.ApplyResources(this.txtDbStartTime, "txtDbStartTime");
			this.txtDbStartTime.Name = "txtDbStartTime";
			this.txtDbStartTime.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtDbStartTime, resources.GetString("txtDbStartTime.ToolTip"));
			// 
			// lblDbStartTime
			// 
			resources.ApplyResources(this.lblDbStartTime, "lblDbStartTime");
			this.lblDbStartTime.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDbStartTime.Name = "lblDbStartTime";
			this.myToolTip.SetToolTip(this.lblDbStartTime, resources.GetString("lblDbStartTime.ToolTip"));
			// 
			// pbDatabase
			// 
			resources.ApplyResources(this.pbDatabase, "pbDatabase");
			this.pbDatabase.BackColor = System.Drawing.Color.White;
			this.pbDatabase.Name = "pbDatabase";
			this.pbDatabase.TabStop = false;
			this.myToolTip.SetToolTip(this.pbDatabase, resources.GetString("pbDatabase.ToolTip"));
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.myToolTip.SetToolTip(this.btnCancel, resources.GetString("btnCancel.ToolTip"));
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.myToolTip.SetToolTip(this.btnOk, resources.GetString("btnOk.ToolTip"));
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// grbCurrentTransformer
			// 
			resources.ApplyResources(this.grbCurrentTransformer, "grbCurrentTransformer");
			this.grbCurrentTransformer.Controls.Add(this.txtCTurnRatio);
			this.grbCurrentTransformer.Controls.Add(this.lblCTurnRatio);
			this.grbCurrentTransformer.Controls.Add(this.txtCNumbers);
			this.grbCurrentTransformer.Controls.Add(this.lblCNumbers);
			this.grbCurrentTransformer.Controls.Add(this.txtCGradeOfOccuracy);
			this.grbCurrentTransformer.Controls.Add(this.lblCGradeOfOccuracy);
			this.grbCurrentTransformer.Controls.Add(this.txtCSecondaryWinding);
			this.grbCurrentTransformer.Controls.Add(this.lblCSecondaryWinding);
			this.grbCurrentTransformer.Controls.Add(this.txtCPrimaryWinding);
			this.grbCurrentTransformer.Controls.Add(this.lblCPrimaryWinding);
			this.grbCurrentTransformer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.grbCurrentTransformer.Name = "grbCurrentTransformer";
			this.grbCurrentTransformer.TabStop = false;
			this.myToolTip.SetToolTip(this.grbCurrentTransformer, resources.GetString("grbCurrentTransformer.ToolTip"));
			// 
			// txtCTurnRatio
			// 
			resources.ApplyResources(this.txtCTurnRatio, "txtCTurnRatio");
			this.txtCTurnRatio.Name = "txtCTurnRatio";
			this.txtCTurnRatio.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtCTurnRatio, resources.GetString("txtCTurnRatio.ToolTip"));
			// 
			// lblCTurnRatio
			// 
			resources.ApplyResources(this.lblCTurnRatio, "lblCTurnRatio");
			this.lblCTurnRatio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCTurnRatio.Name = "lblCTurnRatio";
			this.myToolTip.SetToolTip(this.lblCTurnRatio, resources.GetString("lblCTurnRatio.ToolTip"));
			// 
			// txtCNumbers
			// 
			resources.ApplyResources(this.txtCNumbers, "txtCNumbers");
			this.txtCNumbers.Name = "txtCNumbers";
			this.myToolTip.SetToolTip(this.txtCNumbers, resources.GetString("txtCNumbers.ToolTip"));
			// 
			// lblCNumbers
			// 
			resources.ApplyResources(this.lblCNumbers, "lblCNumbers");
			this.lblCNumbers.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCNumbers.Name = "lblCNumbers";
			this.myToolTip.SetToolTip(this.lblCNumbers, resources.GetString("lblCNumbers.ToolTip"));
			// 
			// txtCGradeOfOccuracy
			// 
			resources.ApplyResources(this.txtCGradeOfOccuracy, "txtCGradeOfOccuracy");
			this.txtCGradeOfOccuracy.Name = "txtCGradeOfOccuracy";
			this.myToolTip.SetToolTip(this.txtCGradeOfOccuracy, resources.GetString("txtCGradeOfOccuracy.ToolTip"));
			this.txtCGradeOfOccuracy.Leave += new System.EventHandler(this.txt_ANY_Winding_Leave);
			// 
			// lblCGradeOfOccuracy
			// 
			resources.ApplyResources(this.lblCGradeOfOccuracy, "lblCGradeOfOccuracy");
			this.lblCGradeOfOccuracy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCGradeOfOccuracy.Name = "lblCGradeOfOccuracy";
			this.myToolTip.SetToolTip(this.lblCGradeOfOccuracy, resources.GetString("lblCGradeOfOccuracy.ToolTip"));
			// 
			// txtCSecondaryWinding
			// 
			resources.ApplyResources(this.txtCSecondaryWinding, "txtCSecondaryWinding");
			this.txtCSecondaryWinding.Name = "txtCSecondaryWinding";
			this.myToolTip.SetToolTip(this.txtCSecondaryWinding, resources.GetString("txtCSecondaryWinding.ToolTip"));
			this.txtCSecondaryWinding.TextChanged += new System.EventHandler(this.txtWinding_TextChanged);
			this.txtCSecondaryWinding.Leave += new System.EventHandler(this.txt_ANY_Winding_Leave);
			// 
			// lblCSecondaryWinding
			// 
			resources.ApplyResources(this.lblCSecondaryWinding, "lblCSecondaryWinding");
			this.lblCSecondaryWinding.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCSecondaryWinding.Name = "lblCSecondaryWinding";
			this.myToolTip.SetToolTip(this.lblCSecondaryWinding, resources.GetString("lblCSecondaryWinding.ToolTip"));
			// 
			// txtCPrimaryWinding
			// 
			resources.ApplyResources(this.txtCPrimaryWinding, "txtCPrimaryWinding");
			this.txtCPrimaryWinding.Name = "txtCPrimaryWinding";
			this.myToolTip.SetToolTip(this.txtCPrimaryWinding, resources.GetString("txtCPrimaryWinding.ToolTip"));
			this.txtCPrimaryWinding.TextChanged += new System.EventHandler(this.txtWinding_TextChanged);
			this.txtCPrimaryWinding.Leave += new System.EventHandler(this.txt_ANY_Winding_Leave);
			// 
			// lblCPrimaryWinding
			// 
			resources.ApplyResources(this.lblCPrimaryWinding, "lblCPrimaryWinding");
			this.lblCPrimaryWinding.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCPrimaryWinding.Name = "lblCPrimaryWinding";
			this.myToolTip.SetToolTip(this.lblCPrimaryWinding, resources.GetString("lblCPrimaryWinding.ToolTip"));
			// 
			// chkCurrentTrPresent
			// 
			resources.ApplyResources(this.chkCurrentTrPresent, "chkCurrentTrPresent");
			this.chkCurrentTrPresent.Name = "chkCurrentTrPresent";
			this.myToolTip.SetToolTip(this.chkCurrentTrPresent, resources.GetString("chkCurrentTrPresent.ToolTip"));
			this.chkCurrentTrPresent.CheckedChanged += new System.EventHandler(this.chkCurrentTrPresent_CheckedChanged);
			// 
			// grbVoltageTransformer
			// 
			resources.ApplyResources(this.grbVoltageTransformer, "grbVoltageTransformer");
			this.grbVoltageTransformer.Controls.Add(this.txtVNumbers);
			this.grbVoltageTransformer.Controls.Add(this.lblVNumbers);
			this.grbVoltageTransformer.Controls.Add(this.txtVGradeOfOccuracy);
			this.grbVoltageTransformer.Controls.Add(this.lblVGradeOfOccuracy);
			this.grbVoltageTransformer.Controls.Add(this.txtVSecondaryWinding);
			this.grbVoltageTransformer.Controls.Add(this.lblVSecondaryWinding);
			this.grbVoltageTransformer.Controls.Add(this.txtVPrimaryWinding);
			this.grbVoltageTransformer.Controls.Add(this.lblVPrimaryWinding);
			this.grbVoltageTransformer.Controls.Add(this.txtVTurnRatio);
			this.grbVoltageTransformer.Controls.Add(this.lblVTurnRatio);
			this.grbVoltageTransformer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.grbVoltageTransformer.Name = "grbVoltageTransformer";
			this.grbVoltageTransformer.TabStop = false;
			this.myToolTip.SetToolTip(this.grbVoltageTransformer, resources.GetString("grbVoltageTransformer.ToolTip"));
			// 
			// txtVNumbers
			// 
			resources.ApplyResources(this.txtVNumbers, "txtVNumbers");
			this.txtVNumbers.Name = "txtVNumbers";
			this.myToolTip.SetToolTip(this.txtVNumbers, resources.GetString("txtVNumbers.ToolTip"));
			// 
			// lblVNumbers
			// 
			resources.ApplyResources(this.lblVNumbers, "lblVNumbers");
			this.lblVNumbers.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblVNumbers.Name = "lblVNumbers";
			this.myToolTip.SetToolTip(this.lblVNumbers, resources.GetString("lblVNumbers.ToolTip"));
			// 
			// txtVGradeOfOccuracy
			// 
			resources.ApplyResources(this.txtVGradeOfOccuracy, "txtVGradeOfOccuracy");
			this.txtVGradeOfOccuracy.Name = "txtVGradeOfOccuracy";
			this.myToolTip.SetToolTip(this.txtVGradeOfOccuracy, resources.GetString("txtVGradeOfOccuracy.ToolTip"));
			this.txtVGradeOfOccuracy.Leave += new System.EventHandler(this.txt_ANY_Winding_Leave);
			// 
			// lblVGradeOfOccuracy
			// 
			resources.ApplyResources(this.lblVGradeOfOccuracy, "lblVGradeOfOccuracy");
			this.lblVGradeOfOccuracy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblVGradeOfOccuracy.Name = "lblVGradeOfOccuracy";
			this.myToolTip.SetToolTip(this.lblVGradeOfOccuracy, resources.GetString("lblVGradeOfOccuracy.ToolTip"));
			// 
			// txtVSecondaryWinding
			// 
			resources.ApplyResources(this.txtVSecondaryWinding, "txtVSecondaryWinding");
			this.txtVSecondaryWinding.Name = "txtVSecondaryWinding";
			this.myToolTip.SetToolTip(this.txtVSecondaryWinding, resources.GetString("txtVSecondaryWinding.ToolTip"));
			this.txtVSecondaryWinding.TextChanged += new System.EventHandler(this.txtWinding_TextChanged);
			this.txtVSecondaryWinding.Leave += new System.EventHandler(this.txt_ANY_Winding_Leave);
			// 
			// lblVSecondaryWinding
			// 
			resources.ApplyResources(this.lblVSecondaryWinding, "lblVSecondaryWinding");
			this.lblVSecondaryWinding.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblVSecondaryWinding.Name = "lblVSecondaryWinding";
			this.myToolTip.SetToolTip(this.lblVSecondaryWinding, resources.GetString("lblVSecondaryWinding.ToolTip"));
			// 
			// txtVPrimaryWinding
			// 
			resources.ApplyResources(this.txtVPrimaryWinding, "txtVPrimaryWinding");
			this.txtVPrimaryWinding.Name = "txtVPrimaryWinding";
			this.myToolTip.SetToolTip(this.txtVPrimaryWinding, resources.GetString("txtVPrimaryWinding.ToolTip"));
			this.txtVPrimaryWinding.TextChanged += new System.EventHandler(this.txtWinding_TextChanged);
			this.txtVPrimaryWinding.Leave += new System.EventHandler(this.txt_ANY_Winding_Leave);
			// 
			// lblVPrimaryWinding
			// 
			resources.ApplyResources(this.lblVPrimaryWinding, "lblVPrimaryWinding");
			this.lblVPrimaryWinding.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblVPrimaryWinding.Name = "lblVPrimaryWinding";
			this.myToolTip.SetToolTip(this.lblVPrimaryWinding, resources.GetString("lblVPrimaryWinding.ToolTip"));
			// 
			// txtVTurnRatio
			// 
			resources.ApplyResources(this.txtVTurnRatio, "txtVTurnRatio");
			this.txtVTurnRatio.Name = "txtVTurnRatio";
			this.txtVTurnRatio.ReadOnly = true;
			this.myToolTip.SetToolTip(this.txtVTurnRatio, resources.GetString("txtVTurnRatio.ToolTip"));
			// 
			// lblVTurnRatio
			// 
			resources.ApplyResources(this.lblVTurnRatio, "lblVTurnRatio");
			this.lblVTurnRatio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblVTurnRatio.Name = "lblVTurnRatio";
			this.myToolTip.SetToolTip(this.lblVTurnRatio, resources.GetString("lblVTurnRatio.ToolTip"));
			// 
			// chkVoltageTrPresent
			// 
			resources.ApplyResources(this.chkVoltageTrPresent, "chkVoltageTrPresent");
			this.chkVoltageTrPresent.Name = "chkVoltageTrPresent";
			this.myToolTip.SetToolTip(this.chkVoltageTrPresent, resources.GetString("chkVoltageTrPresent.ToolTip"));
			this.chkVoltageTrPresent.CheckedChanged += new System.EventHandler(this.chkVoltageTrPresent_CheckedChanged);
			// 
			// lblEnergyQualityRations
			// 
			resources.ApplyResources(this.lblEnergyQualityRations, "lblEnergyQualityRations");
			this.lblEnergyQualityRations.BackColor = System.Drawing.Color.White;
			this.lblEnergyQualityRations.Name = "lblEnergyQualityRations";
			this.myToolTip.SetToolTip(this.lblEnergyQualityRations, resources.GetString("lblEnergyQualityRations.ToolTip"));
			// 
			// lblAverageValues
			// 
			resources.ApplyResources(this.lblAverageValues, "lblAverageValues");
			this.lblAverageValues.BackColor = System.Drawing.Color.White;
			this.lblAverageValues.Name = "lblAverageValues";
			this.myToolTip.SetToolTip(this.lblAverageValues, resources.GetString("lblAverageValues.ToolTip"));
			// 
			// lblDipsAndOvervoltages
			// 
			resources.ApplyResources(this.lblDipsAndOvervoltages, "lblDipsAndOvervoltages");
			this.lblDipsAndOvervoltages.BackColor = System.Drawing.Color.White;
			this.lblDipsAndOvervoltages.Name = "lblDipsAndOvervoltages";
			this.myToolTip.SetToolTip(this.lblDipsAndOvervoltages, resources.GetString("lblDipsAndOvervoltages.ToolTip"));
			// 
			// lblDipsAndOvervoltagesTime
			// 
			resources.ApplyResources(this.lblDipsAndOvervoltagesTime, "lblDipsAndOvervoltagesTime");
			this.lblDipsAndOvervoltagesTime.BackColor = System.Drawing.Color.White;
			this.lblDipsAndOvervoltagesTime.Name = "lblDipsAndOvervoltagesTime";
			this.myToolTip.SetToolTip(this.lblDipsAndOvervoltagesTime, resources.GetString("lblDipsAndOvervoltagesTime.ToolTip"));
			// 
			// lblAverageValuesTime
			// 
			resources.ApplyResources(this.lblAverageValuesTime, "lblAverageValuesTime");
			this.lblAverageValuesTime.BackColor = System.Drawing.Color.White;
			this.lblAverageValuesTime.Name = "lblAverageValuesTime";
			this.myToolTip.SetToolTip(this.lblAverageValuesTime, resources.GetString("lblAverageValuesTime.ToolTip"));
			// 
			// lblEnergyQualityRationsTime
			// 
			resources.ApplyResources(this.lblEnergyQualityRationsTime, "lblEnergyQualityRationsTime");
			this.lblEnergyQualityRationsTime.BackColor = System.Drawing.Color.White;
			this.lblEnergyQualityRationsTime.Name = "lblEnergyQualityRationsTime";
			this.myToolTip.SetToolTip(this.lblEnergyQualityRationsTime, resources.GetString("lblEnergyQualityRationsTime.ToolTip"));
			// 
			// myToolTip
			// 
			this.myToolTip.IsBalloon = true;
			this.myToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
			// 
			// frmOptionsDb
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.lblDipsAndOvervoltages);
			this.Controls.Add(this.lblAverageValues);
			this.Controls.Add(this.lblEnergyQualityRations);
			this.Controls.Add(this.lblDipsAndOvervoltagesTime);
			this.Controls.Add(this.lblAverageValuesTime);
			this.Controls.Add(this.lblEnergyQualityRationsTime);
			this.Controls.Add(this.chkVoltageTrPresent);
			this.Controls.Add(this.chkCurrentTrPresent);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.gbDbOptions);
			this.Controls.Add(this.pbDatabase);
			this.Controls.Add(this.grbCurrentTransformer);
			this.Controls.Add(this.grbVoltageTransformer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmOptionsDb";
			this.myToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmOptionsDb_FormClosing);
			this.Load += new System.EventHandler(this.frmOptionsDb_Load);
			this.gbDbOptions.ResumeLayout(false);
			this.gbDbOptions.PerformLayout();
			this.gbGPS.ResumeLayout(false);
			this.gbGPS.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbDatabase)).EndInit();
			this.grbCurrentTransformer.ResumeLayout(false);
			this.grbCurrentTransformer.PerformLayout();
			this.grbVoltageTransformer.ResumeLayout(false);
			this.grbVoltageTransformer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void chkCurrentTrPresent_CheckedChanged(object sender, System.EventArgs e)
		{
			grbCurrentTransformer.Enabled = chkCurrentTrPresent.Checked;
		}

		private void chkVoltageTrPresent_CheckedChanged(object sender, System.EventArgs e)
		{
			grbVoltageTransformer.Enabled = chkVoltageTrPresent.Checked;
		}

		// Изменение текста полей Обмоток
		private void txtWinding_TextChanged(object sender, EventArgs e)
		{

		}

		private void txt_ANY_Winding_Leave(object sender, EventArgs e)
		{
			TextBox tb = (sender as TextBox);
			float f = 0;
			if (!Single.TryParse(tb.Text, out f))
			{
				ResourceManager rm = 
					new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string msg = rm.GetString("tooltip_invalid_float_value");
				myToolTip.Show(msg, tb);

				chkVoltageTrPresent.Enabled = false;
				chkCurrentTrPresent.Enabled = false;

				tb.Focus();
				return;
			}
			myToolTip.Hide(tb);

			chkVoltageTrPresent.Enabled = true;
			chkCurrentTrPresent.Enabled = true;

			if (tb.Name.StartsWith("txtV"))
			{
				float f1, f2;
				bool b = true;

				b = Single.TryParse(txtVPrimaryWinding.Text, out f1);
				if (!b) { txtVTurnRatio.Text = " - "; return; }
				b = Single.TryParse(txtVSecondaryWinding.Text, out f2);
				if (!b) { txtVTurnRatio.Text = " - "; return; }
				txtVTurnRatio.Text = (f1 / f2).ToString();

			}
			else if (tb.Name.StartsWith("txtC"))
			{
				float f1, f2;
				bool b = true;

				b = Single.TryParse(txtCPrimaryWinding.Text, out f1);
				if (!b) { txtCTurnRatio.Text = " - "; return; }
				b = Single.TryParse(txtCSecondaryWinding.Text, out f2);
				if (!b) { txtCTurnRatio.Text = " - "; return; }
				txtCTurnRatio.Text = (f1 / f2).ToString();
			}
		}

		private bool ApplyChanges(ref DbService dbService, bool exists, bool need, int type)
		{
			CultureInfo ci_enUS = new CultureInfo("en-US");
			string commandText = string.Empty;

			// если был но не нужен... delete
			if (exists && !need)
			{
				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1 || devType_ == EmDeviceType.EM32)
					commandText =
						string.Format("DELETE FROM turn_ratios WHERE database_id = {0} AND type = {1};",
						this.iDatabaseId_, type);
				else if (devType_ == EmDeviceType.ETPQP)
					commandText =
						string.Format("DELETE FROM turn_ratios WHERE object_id = {0} AND turn_type = {1};",
						this.iDatabaseId_, type);
				else if (devType_ == EmDeviceType.ETPQP_A)
					commandText =
						string.Format("DELETE FROM turn_ratios WHERE reg_id = {0} AND turn_type = {1};",
						this.iDatabaseId_, type);
				return dbService.ExecuteNonQuery(commandText, true) == 1;
			}

			// если небыл но нужен... insert
			else if (!exists && need)
			{
				float f1, f2, grade_of_accuracy;
				bool b = true;

				if (type == 1)
				{
					b &= Single.TryParse(txtVPrimaryWinding.Text, out f1);
					b &= Single.TryParse(txtVSecondaryWinding.Text, out f2);
					b &= Single.TryParse(txtVGradeOfOccuracy.Text, out grade_of_accuracy);
				}
				else
				{
					b &= Single.TryParse(txtCPrimaryWinding.Text, out f1);
					b &= Single.TryParse(txtCSecondaryWinding.Text, out f2);
					b &= Single.TryParse(txtCGradeOfOccuracy.Text, out grade_of_accuracy);
				}
				if (b != true) return false;

				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
					commandText =
						string.Format("INSERT INTO turn_ratios (database_id, \"type\", serial_num, value1, value2, grade_of_accuracy) VALUES ({0}, {1}, '{2}', {3}, {4}, {5});",
						this.iDatabaseId_,
						type,
						txtVNumbers.Text,
						f1.ToString(ci_enUS),
						f2.ToString(ci_enUS),
						grade_of_accuracy.ToString(ci_enUS));
				else if (devType_ == EmDeviceType.ETPQP)
					commandText =
						string.Format("INSERT INTO turn_ratios (object_id, turn_type, serial_num, value1, value2, grade_of_accuracy) VALUES ({0}, {1}, '{2}', {3}, {4}, {5});",
						this.iDatabaseId_,
						type,
						txtVNumbers.Text,
						f1.ToString(ci_enUS),
						f2.ToString(ci_enUS),
						grade_of_accuracy.ToString(ci_enUS));
				else if (devType_ == EmDeviceType.ETPQP_A)
					commandText =
						string.Format("INSERT INTO turn_ratios (reg_id, turn_type, serial_num, value1, value2, grade_of_accuracy) VALUES ({0}, {1}, '{2}', {3}, {4}, {5});",
						this.iDatabaseId_,
						type,
						txtVNumbers.Text,
						f1.ToString(ci_enUS),
						f2.ToString(ci_enUS),
						grade_of_accuracy.ToString(ci_enUS));

				return dbService.ExecuteNonQuery(commandText, true) == 1;
			}
			else if (need && exists)
			{
				float f1, f2, grade_of_accuracy;
				string serial = string.Empty;
				bool b = true;

				if (type == 1)
				{
					b &= Single.TryParse(txtVPrimaryWinding.Text, out f1);
					b &= Single.TryParse(txtVSecondaryWinding.Text, out f2);
					b &= Single.TryParse(txtVGradeOfOccuracy.Text, out grade_of_accuracy);
					serial = txtVNumbers.Text;
				}
				else
				{
					b &= Single.TryParse(txtCPrimaryWinding.Text, out f1);
					b &= Single.TryParse(txtCSecondaryWinding.Text, out f2);
					b &= Single.TryParse(txtCGradeOfOccuracy.Text, out grade_of_accuracy);
					serial = txtCNumbers.Text;
				}
				if (b != true) return false;

				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1)
					commandText =
						string.Format("UPDATE turn_ratios SET serial_num = '{2}', value1 = {3}, value2 = {4}, grade_of_accuracy = {5} WHERE database_id = {0} AND type = {1};",
						this.iDatabaseId_,
						type,
						serial,
						f1.ToString(ci_enUS),
						f2.ToString(ci_enUS),
						grade_of_accuracy.ToString(ci_enUS));
				else if (devType_ == EmDeviceType.ETPQP)
					commandText =
						string.Format("UPDATE turn_ratios SET serial_num = '{2}', value1 = {3}, value2 = {4}, grade_of_accuracy = {5} WHERE object_id = {0} AND turn_type = {1};",
						this.iDatabaseId_,
						type,
						serial,
						f1.ToString(ci_enUS),
						f2.ToString(ci_enUS),
						grade_of_accuracy.ToString(ci_enUS));
				else if (devType_ == EmDeviceType.ETPQP_A)
					commandText =
						string.Format("UPDATE turn_ratios SET serial_num = '{2}', value1 = {3}, value2 = {4}, grade_of_accuracy = {5} WHERE reg_id = {0} AND turn_type = {1};",
						this.iDatabaseId_,
						type,
						serial,
						f1.ToString(ci_enUS),
						f2.ToString(ci_enUS),
						grade_of_accuracy.ToString(ci_enUS));

				return dbService.ExecuteNonQuery(commandText, true) == 1;
			}

			return true;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DbService dbService = new DbService(connectString_);
			string commandText = string.Empty;
			try
			{
				dbService.Open();

				if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
					devType_ == EmDeviceType.EM33T1 || devType_ == EmDeviceType.EM32)
					commandText = string.Format(
						"SELECT type FROM turn_ratios WHERE database_id = {0};",
					this.iDatabaseId_);
				else if (devType_ == EmDeviceType.ETPQP)
					commandText = string.Format(
						"SELECT turn_type FROM turn_ratios WHERE object_id = {0};",
					this.iDatabaseId_);
				else if (devType_ == EmDeviceType.ETPQP_A)
					commandText = string.Format(
						"SELECT turn_type FROM turn_ratios WHERE reg_id = {0};",
					this.iDatabaseId_);
				dbService.ExecuteReader(commandText);

				bool bCT_Need = chkCurrentTrPresent.Checked;
				bool bVT_Need = chkVoltageTrPresent.Checked;

				bool bCT_Exists = false;
				bool bVT_Exists = false;

				while (dbService.DataReaderRead())
				{
					short iType = 1;
					if (devType_ == EmDeviceType.EM33T || devType_ == EmDeviceType.EM31K ||
						devType_ == EmDeviceType.EM33T1 || devType_ == EmDeviceType.EM32)
						iType = (short)dbService.DataReaderData("type");
					else if (devType_ == EmDeviceType.ETPQP || devType_ == EmDeviceType.ETPQP_A)
						iType = (short)dbService.DataReaderData("turn_type");

					switch (iType)
					{
						case 1:
							bVT_Exists = true;
							break;
						case 2:
							bCT_Exists = true;
							break;
					}
				}
				bool b = true;
				b &= ApplyChanges(ref dbService, bVT_Exists, bVT_Need, 1);
				if (b) b &= ApplyChanges(ref dbService, bCT_Exists, bCT_Need, 2);
				if (b)
				{
					ERROR_FLAG = false;
					return;
				}

				// here place the error message

				ERROR_FLAG = true;
				MessageBoxes.DbOptionsApplyError(this);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in btnOk_Click() 33:  " + ex.Message);
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
				ERROR_FLAG = true;
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		private void frmOptionsDb_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (ERROR_FLAG == true && this.DialogResult == DialogResult.OK)
				e.Cancel = true;
		}

		private void frmOptionsDb_Load(object sender, EventArgs e)
		{
			chkCurrentTrPresent.Enabled = devType_ != EmDeviceType.ETPQP_A;
			chkVoltageTrPresent.Enabled = devType_ != EmDeviceType.ETPQP_A;
		}

		private void linkLabelMap_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(e.Link.LinkData.ToString());
			System.Diagnostics.Process.Start(sInfo);
		}
	}
}
