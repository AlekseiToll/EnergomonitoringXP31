using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Resources;

using NativeWifi;

using DeviceIO;
using DeviceIO.Memory;
using DeviceIO.Constraints;
using EmServiceLib;
using EmDataSaver;

namespace EnergomonitoringXP
{
	public partial class frmConstraints : Form
	{
		private Settings settings_;
		private List<int[]>[] errorCells_;
		private Form mainForm_;

		private DataTable tableType_ = new DataTable();
		private DataTable tableConstraints_ = new DataTable();

		private Color errCellColor_ = Color.Red;

		private const double dblConstHighLimitConstraints_ = 50.0;
	
		/// <summary>Constructor</summary>
		public frmConstraints(Settings settings, Form formMain)
		{
			InitializeComponent();
			this.settings_ = settings;
			this.mainForm_ = formMain;

			errorCells_ = new List<int[]>[2];

			for (int i = 0; i < errorCells_.Length; i++)
				errorCells_[i] = new List<int[]>();
		}

		private void frmСonstraints_Load(object sender, EventArgs e)
		{
			try
			{
				#region Create Columns

				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConstraints));
				DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
				DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
				DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
				DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();

				//DataSet dsConstraints;
				DataColumn dcNorm3ph4w = new DataColumn();
				DataColumn dcMax3ph4w = new DataColumn();
				DataColumn dcNorm3ph3w = new DataColumn();
				DataColumn dcMax3ph3w = new DataColumn();
				DataColumn dcVal_95 = new DataColumn();
				DataColumn dcVal_100 = new DataColumn();
				DataColumn dcParamName = new DataColumn();
				DataColumn dcTypeId = new DataColumn();
				DataColumn dcConstTypeId = new DataColumn();
				DataColumn dcTypeName = new DataColumn();

				#region Columns

				dcParamName.AllowDBNull = false;
				dcParamName.Caption = "Parameter";
				dcParamName.ColumnName = "param_name";
				// 
				// dcNorm3ph4w
				// 
				dcNorm3ph4w.Caption = "Normal";
				dcNorm3ph4w.ColumnName = "normal_3ph4w";
				dcNorm3ph4w.DataType = typeof(float);
				dcNorm3ph4w.DefaultValue = 0F;
				dcNorm3ph4w.Prefix = "ttt";
				// 
				// dcMax3ph4w
				// 
				dcMax3ph4w.Caption = "Max";
				dcMax3ph4w.ColumnName = "max_3ph4w";
				dcMax3ph4w.DataType = typeof(float);
				dcMax3ph4w.DefaultValue = 0F;
				// 
				// dcNorm3ph3w
				// 
				dcNorm3ph3w.Caption = "Normal";
				dcNorm3ph3w.ColumnName = "normal_3ph3w";
				dcNorm3ph3w.DataType = typeof(float);
				dcNorm3ph3w.DefaultValue = 0F;
				// 
				// dcMax3ph3w
				// 
				dcMax3ph3w.Caption = "Max";
				dcMax3ph3w.ColumnName = "max_3ph3w";
				dcMax3ph3w.DataType = typeof(float);
				dcMax3ph3w.DefaultValue = 0F;
				// 
				// dcVal_95
				// 
				dcVal_95.Caption = "95";
				dcVal_95.ColumnName = "val_95";
				dcVal_95.DataType = typeof(float);
				dcVal_95.DefaultValue = 0F;
				// 
				// dcVal_100
				// 
				dcVal_100.Caption = "100";
				dcVal_100.ColumnName = "val_100";
				dcVal_100.DataType = typeof(float);
				dcVal_100.DefaultValue = 0F;
				// 
				// dcTypeId
				// 
				dcTypeId.AllowDBNull = false;
				dcTypeId.Caption = "Constraint";
				dcTypeId.ColumnName = "constraint_type_id";
				dcTypeId.DataType = typeof(int);
				// 
				// dcConstTypeId
				// 
				dcConstTypeId.AllowDBNull = false;
				dcConstTypeId.Caption = "id";
				dcConstTypeId.ColumnName = "type_id";
				dcConstTypeId.DataType = typeof(int);
				// 
				// dcTypeName
				// 
				dcTypeName.AllowDBNull = false;
				dcTypeName.Caption = "name";
				dcTypeName.ColumnName = "type_name";

				#endregion

				((System.ComponentModel.ISupportInitialize)(this.dsConstraints)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(tableConstraints_)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(tableType_)).BeginInit();

				tableConstraints_.TableName = "table_constraints";
				tableType_.Columns.AddRange(new System.Data.DataColumn[] {
						dcConstTypeId,
						dcTypeName});
				tableType_.Constraints.AddRange(new System.Data.Constraint[] {
				                    new UniqueConstraint("Constraint1", new string[] {
				                        "type_id"}, false)});


				tableType_.TableName = "table_types";
				this.dsConstraints.Tables.AddRange(new System.Data.DataTable[] {
							tableConstraints_,
							tableType_});

				this.dsConstraints.DataSetName = "ds_constraints_names";
				this.dsConstraints.EnforceConstraints = false;

				DataRelation[] ttmp = new DataRelation[] {
					new DataRelation("RelationMain", "table_types", "table_constraints", new string[] {
								"type_id"}, new string[] {
										"constraint_type_id"}, false)};
				this.dsConstraints.Relations.AddRange(new DataRelation[] {
				    new DataRelation("RelationMain", "table_types", "table_constraints", new string[] {
				                "type_id"}, new string[] {
				                        "constraint_type_id"}, false)});

				this.dgvConstraints.DataMember = "table_types.RelationMain";

				this.dctbParamName = new System.Windows.Forms.DataGridViewTextBoxColumn();
				this.dctbContraintTypeId = new System.Windows.Forms.DataGridViewTextBoxColumn();

				// dctbParamName
				this.dctbParamName.DataPropertyName = "param_name";
				this.dctbParamName.Frozen = true;
				resources.ApplyResources(this.dctbParamName, "dctbParamName");
				this.dctbParamName.Name = "dctbParamName";
				this.dctbParamName.ReadOnly = true;
				this.dctbParamName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
				this.dctbParamName.Width = 89;

				// dctbContraintTypeId
				this.dctbContraintTypeId.DataPropertyName = "constraint_type_id";
				resources.ApplyResources(this.dctbContraintTypeId, "dctbContraintTypeId");
				this.dctbContraintTypeId.Name = "dctbContraintTypeId";
				this.dctbContraintTypeId.SortMode = 
					System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
				this.dctbContraintTypeId.Visible = false;

				if (settings_.CurDeviceType != EmDeviceType.ETPQP_A)
				{
					lblConnectScheme.Visible = true;
					lblConnectScheme2.Visible = true;

					this.dctbNPL_3ph4w = new System.Windows.Forms.DataGridViewTextBoxColumn();
					this.dctbUPL_3ph4w = new System.Windows.Forms.DataGridViewTextBoxColumn();
					this.dctbNPL_3ph3w = new System.Windows.Forms.DataGridViewTextBoxColumn();
					this.dctbUPL_3ph3w = new System.Windows.Forms.DataGridViewTextBoxColumn();

					// dctbNPL_3ph4w
					this.dctbNPL_3ph4w.DataPropertyName = "normal_3ph4w";
					dataGridViewCellStyle1.Format = "0.0#";
					this.dctbNPL_3ph4w.DefaultCellStyle = dataGridViewCellStyle1;
					resources.ApplyResources(this.dctbNPL_3ph4w, "dctbNPL_3ph4w");
					this.dctbNPL_3ph4w.Name = "dctbNPL_3ph4w";
					this.dctbNPL_3ph4w.Width = 70;

					// dctbUPL_3ph4w
					this.dctbUPL_3ph4w.DataPropertyName = "max_3ph4w";
					dataGridViewCellStyle2.Format = "0.0#";
					this.dctbUPL_3ph4w.DefaultCellStyle = dataGridViewCellStyle2;
					resources.ApplyResources(this.dctbUPL_3ph4w, "dctbUPL_3ph4w");
					this.dctbUPL_3ph4w.Name = "dctbUPL_3ph4w";
					this.dctbUPL_3ph4w.Width = 70;

					// dctbNPL_3ph3w
					this.dctbNPL_3ph3w.DataPropertyName = "normal_3ph3w";
					dataGridViewCellStyle3.Format = "0.0#";
					this.dctbNPL_3ph3w.DefaultCellStyle = dataGridViewCellStyle3;
					resources.ApplyResources(this.dctbNPL_3ph3w, "dctbNPL_3ph3w");
					this.dctbNPL_3ph3w.Name = "dctbNPL_3ph3w";
					this.dctbNPL_3ph3w.Width = 70;

					// dctbUPL_3ph3w
					this.dctbUPL_3ph3w.DataPropertyName = "max_3ph3w";
					dataGridViewCellStyle4.Format = "0.0#";
					this.dctbUPL_3ph3w.DefaultCellStyle = dataGridViewCellStyle4;
					resources.ApplyResources(this.dctbUPL_3ph3w, "dctbUPL_3ph3w");
					this.dctbUPL_3ph3w.Name = "dctbUPL_3ph3w";
					this.dctbUPL_3ph3w.Width = 70;

					this.dgvConstraints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
								this.dctbParamName,
								this.dctbNPL_3ph4w,
								this.dctbUPL_3ph4w,
								this.dctbNPL_3ph3w,
								this.dctbUPL_3ph3w,
								this.dctbContraintTypeId});

					tableConstraints_.Columns.AddRange(new System.Data.DataColumn[] {
							dcParamName,
							dcNorm3ph4w,
							dcMax3ph4w,
							dcNorm3ph3w,
							dcMax3ph3w,
							dcTypeId});
					tableConstraints_.Constraints.AddRange(new System.Data.Constraint[] {
							new System.Data.ForeignKeyConstraint("RelationMain", "table_types", new string[] {
										"type_id"}, new string[] {
										"constraint_type_id"}, System.Data.AcceptRejectRule.None, 
										System.Data.Rule.Cascade, System.Data.Rule.Cascade)});
				}
				else
				{
					lblConnectScheme.Visible = false;
					lblConnectScheme2.Visible = false;

					this.dctbVal_95 = new System.Windows.Forms.DataGridViewTextBoxColumn();
					this.dctbVal_100 = new System.Windows.Forms.DataGridViewTextBoxColumn();

					// dctbVal_95
					this.dctbVal_95.DataPropertyName = "val_95";
					dataGridViewCellStyle1.Format = "0.0#";
					this.dctbVal_95.DefaultCellStyle = dataGridViewCellStyle1;
					resources.ApplyResources(this.dctbVal_95, "dctbVal_95");
					this.dctbVal_95.Name = "dctbVal_95";
					this.dctbVal_95.HeaderText = "95";

					// dctbVal_100
					this.dctbVal_100.DataPropertyName = "val_100";
					dataGridViewCellStyle2.Format = "0.0#";
					this.dctbVal_100.DefaultCellStyle = dataGridViewCellStyle2;
					resources.ApplyResources(this.dctbVal_100, "dctbVal_100");
					this.dctbVal_100.Name = "dctbVal_100";
					this.dctbVal_100.HeaderText = "100";

					this.dgvConstraints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
								this.dctbParamName,
								this.dctbVal_95,
								this.dctbVal_100,
								this.dctbContraintTypeId});

					this.tableConstraints_.Columns.AddRange(new System.Data.DataColumn[] {
							dcParamName,
							dcVal_95,
							dcVal_100,
							dcTypeId});
					this.tableConstraints_.Constraints.AddRange(new System.Data.Constraint[] {
							new System.Data.ForeignKeyConstraint("RelationMain", "table_types", new string[] {
										"type_id"}, new string[] {
										"constraint_type_id"}, System.Data.AcceptRejectRule.None, 
										System.Data.Rule.Cascade, System.Data.Rule.Cascade)});
				}

				((System.ComponentModel.ISupportInitialize)(this.dsConstraints)).EndInit();
				((System.ComponentModel.ISupportInitialize)(tableConstraints_)).EndInit();
				((System.ComponentModel.ISupportInitialize)(tableType_)).EndInit();

				this.cmbConstraintType.DisplayMember = "table_types.type_name";
				this.cmbConstraintType.ValueMember = "table_types.type_id";

				#endregion

				if (settings_.CurDeviceType != EmDeviceType.ETPQP_A)
				{
					createContraintsTypesDataSet();
					createContraintsDataSetType1();
					createContraintsDataSetType2();
					createContraintsDataSetType3();
					createContraintsDataSetType4();
					createContraintsDataSetType5();
					createContraintsDataSetType6();
				}
				else
				{
					createContraintsTypesDataSet();
					createContraintsDataSetType1_EtPQP_A();
					createContraintsDataSetType2_EtPQP_A();
					createContraintsDataSetType3_EtPQP_A();
					createContraintsDataSetType4_EtPQP_A();
					createContraintsDataSetType5_EtPQP_A();
					createContraintsDataSetType6_EtPQP_A();
				}

				cmbConstraintType_SelectedIndexChanged(sender, e);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmConstraints_Load(): ");
				throw;
			}
		}

		private void createContraintsTypesDataSet()
		{
			try
			{
				DataRow newRow;
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
					this.GetType().Assembly);

				for (int i = 0; i < EMConstraintsBase.CntConstraintsSets; i++)
				{
					string constr_name = string.Empty;
					if (settings_.CurDeviceType != EmServiceLib.EmDeviceType.ETPQP_A)
					{
						constr_name = rm.GetString(string.Format(
							"name.constraint_type.{0}.full", i + 1));
					}
					else
					{
						constr_name = rm.GetString(string.Format(
							"name_constraint_type_pqpa_{0}_full", i + 1));
					}

					newRow = tableType_.NewRow();
					newRow["type_id"] = i + 1;
					newRow["type_name"] = constr_name;

					tableType_.Rows.Add(newRow);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in createContraintsTypesDataSet(): ");
				throw;
			}
		}

		/// NOTE: THIS FILE IS GENERATED
		/// ACCORDING TO DATAGRIDVIEW INFORMATION
		///
		
		private void createContraintsDataSetType1()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 1

			// ∆F+ , Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.4;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.4;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- , Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = -0.2;
			newRow["max_3ph4w"] = -0.4;
			newRow["normal_3ph3w"] = -0.2;
			newRow["max_3ph3w"] = -0.4;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// δUs''+ , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''+" : "δUs''+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// δUs''- , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''-" : "δUs''-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// δUs'+ , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'+" : "δUs'+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// δUs'- , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'-" : "δUs'-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(2) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(3) , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 7.5;
			newRow["normal_3ph3w"] = 2.5;
			newRow["max_3ph3w"] = 3.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(4) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(5) , %";
			newRow["normal_3ph4w"] = 6;
			newRow["max_3ph4w"] = 9;
			newRow["normal_3ph3w"] = 6;
			newRow["max_3ph3w"] = 9;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(6) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(7) , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 7.5;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 7.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(8) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(9) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 0.75;
			newRow["max_3ph3w"] = 1.13;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(10) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(11) , %";
			newRow["normal_3ph4w"] = 3.5;
			newRow["max_3ph4w"] = 5.25;
			newRow["normal_3ph3w"] = 3.5;
			newRow["max_3ph3w"] = 5.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(12) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(13) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 3;
			newRow["max_3ph3w"] = 4.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(14) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(15) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(16) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(17) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(18) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(19) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(20) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(21) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(22) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(23) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(24) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(25) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(26) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(27) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(28) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(29) , %";
			newRow["normal_3ph4w"] = 1.32;
			newRow["max_3ph4w"] = 1.98;
			newRow["normal_3ph3w"] = 1.32;
			newRow["max_3ph3w"] = 1.98;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(30) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(31) , %";
			newRow["normal_3ph4w"] = 1.25;
			newRow["max_3ph4w"] = 1.87;
			newRow["normal_3ph3w"] = 1.25;
			newRow["max_3ph3w"] = 1.87;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(32) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(33) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(34) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(35) , %";
			newRow["normal_3ph4w"] = 1.13;
			newRow["max_3ph4w"] = 1.69;
			newRow["normal_3ph3w"] = 1.13;
			newRow["max_3ph3w"] = 1.69;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(36) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(37) , %";
			newRow["normal_3ph4w"] = 1.08;
			newRow["max_3ph4w"] = 1.62;
			newRow["normal_3ph3w"] = 1.08;
			newRow["max_3ph3w"] = 1.62;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(38) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(39) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// Ku(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(40) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// THDu , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "Ku" : "THDu") + " , %";
			newRow["normal_3ph4w"] = 8;
			newRow["max_3ph4w"] = 12;
			newRow["normal_3ph3w"] = 8;
			newRow["max_3ph3w"] = 12;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType2()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 2

			// ∆F+
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.4;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.4;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// ∆F-
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = -0.2;
			newRow["max_3ph4w"] = -0.4;
			newRow["normal_3ph3w"] = -0.2;
			newRow["max_3ph3w"] = -0.4;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// δU+''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''+" : "δUs''+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// δU-''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''-" : "δUs''-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// δU+'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'+" : "δUs'+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// δU-'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'-" : "δUs'-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			//if (settings_.CurDeviceType == EmDeviceType.EM32 ||
			//    settings_.CurDeviceType == EmDeviceType.ETPQP)
			//{
			//    // δUs+ , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU+" : "δUs+") + " , %";
			//    newRow["normal_3ph4w"] = 5;
			//    newRow["max_3ph4w"] = 10;
			//    newRow["normal_3ph3w"] = 5;
			//    newRow["max_3ph3w"] = 10;
			//    newRow["constraint_type_id"] = 2;
			//    tableConstraints_.Rows.Add(newRow);

			//    // δUs- , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU-" : "δUs-") + " , %";
			//    newRow["normal_3ph4w"] = -5;
			//    newRow["max_3ph4w"] = -10;
			//    newRow["normal_3ph3w"] = -5;
			//    newRow["max_3ph3w"] = -10;
			//    newRow["constraint_type_id"] = 2;
			//    tableConstraints_.Rows.Add(newRow);
			//}

			// K2u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// K0u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku2
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(2) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku3
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(3) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku4
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(4) , %";
			newRow["normal_3ph4w"] = 0.7;
			newRow["max_3ph4w"] = 1.05;
			newRow["normal_3ph3w"] = 0.7;
			newRow["max_3ph3w"] = 1.05;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku5
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(5) , %";
			newRow["normal_3ph4w"] = 4;
			newRow["max_3ph4w"] = 6;
			newRow["normal_3ph3w"] = 4;
			newRow["max_3ph3w"] = 6;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku6
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(6) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku7
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(7) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 3;
			newRow["max_3ph3w"] = 4.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku8
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(8) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku9
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(9) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku10
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(10) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku11
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(11) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku12
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(12) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku13
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(13) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku14
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(14) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku15
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(15) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku16
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(16) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku17
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(17) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku18
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(18) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku19
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(19) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku20
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(20) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku21
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(21) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku22
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(22) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku23
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(23) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku24
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(24) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku25
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(25) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku26
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(26) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku27
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(27) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku28
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(28) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku29
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(29) , %";
			newRow["normal_3ph4w"] = 0.89;
			newRow["max_3ph4w"] = 1.33;
			newRow["normal_3ph3w"] = 0.89;
			newRow["max_3ph3w"] = 1.33;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku30
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(30) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku31
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(31) , %";
			newRow["normal_3ph4w"] = 0.85;
			newRow["max_3ph4w"] = 1.27;
			newRow["normal_3ph3w"] = 0.85;
			newRow["max_3ph3w"] = 1.27;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku32
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(32) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku33
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(33) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku34
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(34) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku35
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(35) , %";
			newRow["normal_3ph4w"] = 0.77;
			newRow["max_3ph4w"] = 1.16;
			newRow["normal_3ph3w"] = 0.77;
			newRow["max_3ph3w"] = 1.16;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku36
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(36) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku37
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(37) , %";
			newRow["normal_3ph4w"] = 0.74;
			newRow["max_3ph4w"] = 1.11;
			newRow["normal_3ph3w"] = 0.74;
			newRow["max_3ph3w"] = 1.11;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku38
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(38) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku39
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(39) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku40
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(40) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// Ku
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "Ku" : "THDu") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 8;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 8;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType3()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 3

			// ∆F+
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.4;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.4;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// ∆F-
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = -0.2;
			newRow["max_3ph4w"] = -0.4;
			newRow["normal_3ph3w"] = -0.2;
			newRow["max_3ph3w"] = -0.4;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// δU+''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''+" : "δUs''+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// δU-''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''-" : "δUs''-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// δU+'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'+" : "δUs'+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// δU-'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'-" : "δUs'-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			//if (settings_.CurDeviceType == EmDeviceType.EM32 ||
			//    settings_.CurDeviceType == EmDeviceType.ETPQP)
			//{
			//    // δUs+ , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU+" : "δUs+") + " , %";
			//    newRow["normal_3ph4w"] = 5;
			//    newRow["max_3ph4w"] = 10;
			//    newRow["normal_3ph3w"] = 5;
			//    newRow["max_3ph3w"] = 10;
			//    newRow["constraint_type_id"] = 3;
			//    tableConstraints_.Rows.Add(newRow);

			//    // δUs- , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU-" : "δUs-") + " , %";
			//    newRow["normal_3ph4w"] = -5;
			//    newRow["max_3ph4w"] = -10;
			//    newRow["normal_3ph3w"] = -5;
			//    newRow["max_3ph3w"] = -10;
			//    newRow["constraint_type_id"] = 3;
			//    tableConstraints_.Rows.Add(newRow);
			//}

			// K2u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// K0u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku2
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(2) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku3
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(3) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku4
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(4) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku5
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(5) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 3;
			newRow["max_3ph3w"] = 4.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku6
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(6) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku7
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(7) , %";
			newRow["normal_3ph4w"] = 2.5;
			newRow["max_3ph4w"] = 3.75;
			newRow["normal_3ph3w"] = 2.5;
			newRow["max_3ph3w"] = 3.75;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku8
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(8) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku9
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(9) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku10
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(10) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku11
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(11) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku12
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(12) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku13
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(13) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku14
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(14) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku15
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(15) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku16
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(16) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku17
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(17) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku18
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(18) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku19
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(19) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku20
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(20) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku21
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(21) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku22
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(22) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku23
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(23) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku24
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(24) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku25
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(25) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku26
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(26) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku27
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(27) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku28
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(28) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku29
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(29) , %";
			newRow["normal_3ph4w"] = 0.72;
			newRow["max_3ph4w"] = 1.08;
			newRow["normal_3ph3w"] = 0.72;
			newRow["max_3ph3w"] = 1.08;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku30
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(30) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku31
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(31) , %";
			newRow["normal_3ph4w"] = 0.68;
			newRow["max_3ph4w"] = 1.03;
			newRow["normal_3ph3w"] = 0.68;
			newRow["max_3ph3w"] = 1.03;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku32
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(32) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku33
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(33) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku34
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(34) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku35
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(35) , %";
			newRow["normal_3ph4w"] = 0.63;
			newRow["max_3ph4w"] = 0.94;
			newRow["normal_3ph3w"] = 0.63;
			newRow["max_3ph3w"] = 0.94;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku36
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(36) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku37
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(37) , %";
			newRow["normal_3ph4w"] = 0.61;
			newRow["max_3ph4w"] = 0.91;
			newRow["normal_3ph3w"] = 0.61;
			newRow["max_3ph3w"] = 0.91;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku38
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(38) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku39
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(39) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku40
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(40) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// Ku
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "Ku" : "THDu") + " , %";
			newRow["normal_3ph4w"] = 4;
			newRow["max_3ph4w"] = 6;
			newRow["normal_3ph3w"] = 4;
			newRow["max_3ph3w"] = 6;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType4()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 4

			// ∆F+
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.4;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.4;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// ∆F-
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = -0.2;
			newRow["max_3ph4w"] = -0.4;
			newRow["normal_3ph3w"] = -0.2;
			newRow["max_3ph3w"] = -0.4;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// δU+''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''+" : "δUs''+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// δU-''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''-" : "δUs''-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// δU+'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'+" : "δUs'+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// δU-'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'-" : "δUs'-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			//if (settings_.CurDeviceType == EmDeviceType.EM32 ||
			//    settings_.CurDeviceType == EmDeviceType.ETPQP)
			//{
			//    // δUs+ , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU+" : "δUs+") + " , %";
			//    newRow["normal_3ph4w"] = 5;
			//    newRow["max_3ph4w"] = 10;
			//    newRow["normal_3ph3w"] = 5;
			//    newRow["max_3ph3w"] = 10;
			//    newRow["constraint_type_id"] = 4;
			//    tableConstraints_.Rows.Add(newRow);

			//    // δUs- , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU-" : "δUs-") + " , %";
			//    newRow["normal_3ph4w"] = -5;
			//    newRow["max_3ph4w"] = -10;
			//    newRow["normal_3ph3w"] = -5;
			//    newRow["max_3ph3w"] = -10;
			//    newRow["constraint_type_id"] = 4;
			//    tableConstraints_.Rows.Add(newRow);
			//}

			// K2u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// K0u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku2
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(2) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku3
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(3) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 0.75;
			newRow["max_3ph3w"] = 1.13;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku4
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(4) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku5
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(5) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku6
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(6) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku7
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(7) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku8
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(8) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku9
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(9) , %";
			newRow["normal_3ph4w"] = 0.4;
			newRow["max_3ph4w"] = 0.6;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku10
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(10) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku11
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(11) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku12
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(12) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku13
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(13) , %";
			newRow["normal_3ph4w"] = 0.7;
			newRow["max_3ph4w"] = 1.05;
			newRow["normal_3ph3w"] = 0.7;
			newRow["max_3ph3w"] = 1.05;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku14
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(14) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku15
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(15) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku16
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(16) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku17
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(17) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku18
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(18) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku19
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(19) , %";
			newRow["normal_3ph4w"] = 0.4;
			newRow["max_3ph4w"] = 0.6;
			newRow["normal_3ph3w"] = 0.4;
			newRow["max_3ph3w"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku20
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(20) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku21
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(21) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku22
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(22) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku23
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(23) , %";
			newRow["normal_3ph4w"] = 0.4;
			newRow["max_3ph4w"] = 0.6;
			newRow["normal_3ph3w"] = 0.4;
			newRow["max_3ph3w"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku24
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(24) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku25
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(25) , %";
			newRow["normal_3ph4w"] = 0.4;
			newRow["max_3ph4w"] = 0.6;
			newRow["normal_3ph3w"] = 0.4;
			newRow["max_3ph3w"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku26
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(26) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku27
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(27) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku28
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(28) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku29
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(29) , %";
			newRow["normal_3ph4w"] = 0.37;
			newRow["max_3ph4w"] = 0.56;
			newRow["normal_3ph3w"] = 0.37;
			newRow["max_3ph3w"] = 0.56;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku30
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(30) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku31
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(31) , %";
			newRow["normal_3ph4w"] = 0.36;
			newRow["max_3ph4w"] = 0.54;
			newRow["normal_3ph3w"] = 0.36;
			newRow["max_3ph3w"] = 0.54;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku32
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(32) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku33
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(33) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku34
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(34) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku35
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(35) , %";
			newRow["normal_3ph4w"] = 0.34;
			newRow["max_3ph4w"] = 0.51;
			newRow["normal_3ph3w"] = 0.34;
			newRow["max_3ph3w"] = 0.51;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku36
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(36) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku37
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(37) , %";
			newRow["normal_3ph4w"] = 0.34;
			newRow["max_3ph4w"] = (settings_.CurDeviceType == EmDeviceType.ETPQP) ? 0.5 : 0.51;
			newRow["normal_3ph3w"] = 0.34;
			newRow["max_3ph3w"] = (settings_.CurDeviceType == EmDeviceType.ETPQP) ? 0.5 : 0.51;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku38
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(38) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku39
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(39) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku40
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(40) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// Ku
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "Ku" : "THDu") + " , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType5()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
			DataRow newRow;

			#region Type 5

			// ∆F+
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.4;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.4;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// ∆F-
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = -0.2;
			newRow["max_3ph4w"] = -0.4;
			newRow["normal_3ph3w"] = -0.2;
			newRow["max_3ph3w"] = -0.4;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// δU+''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''+" : "δUs''+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// δU-''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''-" : "δUs''-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// δU+'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'+" : "δUs'+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// δU-'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'-" : "δUs'-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			//if (settings_.CurDeviceType == EmDeviceType.EM32 ||
			//    settings_.CurDeviceType == EmDeviceType.ETPQP)
			//{
			//    // δUs+ , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU+" : "δUs+") + " , %";
			//    newRow["normal_3ph4w"] = 5;
			//    newRow["max_3ph4w"] = 10;
			//    newRow["normal_3ph3w"] = 5;
			//    newRow["max_3ph3w"] = 10;
			//    newRow["constraint_type_id"] = 5;
			//    tableConstraints_.Rows.Add(newRow);

			//    // δUs- , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU-" : "δUs-") + " , %";
			//    newRow["normal_3ph4w"] = -5;
			//    newRow["max_3ph4w"] = -10;
			//    newRow["normal_3ph3w"] = -5;
			//    newRow["max_3ph3w"] = -10;
			//    newRow["constraint_type_id"] = 5;
			//    tableConstraints_.Rows.Add(newRow);
			//}

			// K2u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// K0u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku2
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(2) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku3
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(3) , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 7.5;
			newRow["normal_3ph3w"] = 2.5;
			newRow["max_3ph3w"] = 3.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku4
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(4) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku5
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(5) , %";
			newRow["normal_3ph4w"] = 6;
			newRow["max_3ph4w"] = 9;
			newRow["normal_3ph3w"] = 6;
			newRow["max_3ph3w"] = 9;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku6
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(6) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku7
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(7) , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 7.5;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 7.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku8
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(8) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku9
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(9) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 0.75;
			newRow["max_3ph3w"] = 1.13;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku10
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(10) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku11
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(11) , %";
			newRow["normal_3ph4w"] = 3.5;
			newRow["max_3ph4w"] = 5.25;
			newRow["normal_3ph3w"] = 3.5;
			newRow["max_3ph3w"] = 5.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku12
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(12) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku13
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(13) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 3;
			newRow["max_3ph3w"] = 4.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku14
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(14) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku15
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(15) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku16
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(16) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku17
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(17) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku18
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(18) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku19
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(19) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku20
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(20) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku21
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(21) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku22
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(22) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku23
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(23) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku24
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(24) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku25
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(25) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku26
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(26) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku27
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(27) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku28
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(28) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku29
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(29) , %";
			newRow["normal_3ph4w"] = 1.32;
			newRow["max_3ph4w"] = 1.98;
			newRow["normal_3ph3w"] = 1.32;
			newRow["max_3ph3w"] = 1.98;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku30
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(30) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku31
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(31) , %";
			newRow["normal_3ph4w"] = 1.25;
			newRow["max_3ph4w"] = 1.87;
			newRow["normal_3ph3w"] = 1.25;
			newRow["max_3ph3w"] = 1.87;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku32
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(32) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku33
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(33) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku34
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(34) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku35
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(35) , %";
			newRow["normal_3ph4w"] = 1.13;
			newRow["max_3ph4w"] = 1.69;
			newRow["normal_3ph3w"] = 1.13;
			newRow["max_3ph3w"] = 1.69;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku36
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(36) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			//Ku37
		   newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(37) , %";
			newRow["normal_3ph4w"] = 1.08;
			newRow["max_3ph4w"] = 1.62;
			newRow["normal_3ph3w"] = 1.08;
			newRow["max_3ph3w"] = 1.62;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku38
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(38) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku39
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(39) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku40
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(40) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// Ku
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "Ku" : "THDu") + " , %";
			newRow["normal_3ph4w"] = 8;
			newRow["max_3ph4w"] = 12;
			newRow["normal_3ph3w"] = 8;
			newRow["max_3ph3w"] = 12;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType6()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 6

			// ∆F+
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.4;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.4;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// ∆F-
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- , " + rm.GetString("name.hertz.short");
			newRow["normal_3ph4w"] = -0.2;
			newRow["max_3ph4w"] = -0.4;
			newRow["normal_3ph3w"] = -0.2;
			newRow["max_3ph3w"] = -0.4;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// δU+''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''+" : "δUs''+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// δU-''
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU''-" : "δUs''-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// δU+'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'+" : "δUs'+") + " , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 10;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 10;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// δU-'
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU'-" : "δUs'-") + " , %";
			newRow["normal_3ph4w"] = -5;
			newRow["max_3ph4w"] = -10;
			newRow["normal_3ph3w"] = -5;
			newRow["max_3ph3w"] = -10;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			//if (settings_.CurDeviceType == EmDeviceType.EM32 ||
			//    settings_.CurDeviceType == EmDeviceType.ETPQP)
			//{
			//    // δUs+ , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU+" : "δUs+") + " , %";
			//    newRow["normal_3ph4w"] = 5;
			//    newRow["max_3ph4w"] = 10;
			//    newRow["normal_3ph3w"] = 5;
			//    newRow["max_3ph3w"] = 10;
			//    newRow["constraint_type_id"] = 6;
			//    tableConstraints_.Rows.Add(newRow);

			//    // δUs- , %
			//    newRow = tableConstraints_.NewRow();
			//    newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "δU-" : "δUs-") + " , %";
			//    newRow["normal_3ph4w"] = -5;
			//    newRow["max_3ph4w"] = -10;
			//    newRow["normal_3ph3w"] = -5;
			//    newRow["max_3ph3w"] = -10;
			//    newRow["constraint_type_id"] = 6;
			//    tableConstraints_.Rows.Add(newRow);
			//}

			// K2u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// K0u
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 4;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 4;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku2
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(2) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku3
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(3) , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 7.5;
			newRow["normal_3ph3w"] = 2.5;
			newRow["max_3ph3w"] = 3.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku4
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(4) , %";
			newRow["normal_3ph4w"] = 1;
			newRow["max_3ph4w"] = 1.5;
			newRow["normal_3ph3w"] = 1;
			newRow["max_3ph3w"] = 1.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku5
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(5) , %";
			newRow["normal_3ph4w"] = 6;
			newRow["max_3ph4w"] = 9;
			newRow["normal_3ph3w"] = 6;
			newRow["max_3ph3w"] = 9;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku6
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(6) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku7
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(7) , %";
			newRow["normal_3ph4w"] = 5;
			newRow["max_3ph4w"] = 7.5;
			newRow["normal_3ph3w"] = 5;
			newRow["max_3ph3w"] = 7.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku8
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(8) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku9
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(9) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 0.75;
			newRow["max_3ph3w"] = 1.13;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku10
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(10) , %";
			newRow["normal_3ph4w"] = 0.5;
			newRow["max_3ph4w"] = 0.75;
			newRow["normal_3ph3w"] = 0.5;
			newRow["max_3ph3w"] = 0.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku11
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(11) , %";
			newRow["normal_3ph4w"] = 3.5;
			newRow["max_3ph4w"] = 5.25;
			newRow["normal_3ph3w"] = 3.5;
			newRow["max_3ph3w"] = 5.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku12
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(12) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku13
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(13) , %";
			newRow["normal_3ph4w"] = 3;
			newRow["max_3ph4w"] = 4.5;
			newRow["normal_3ph3w"] = 3;
			newRow["max_3ph3w"] = 4.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku14
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(14) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku15
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(15) , %";
			newRow["normal_3ph4w"] = 0.3;
			newRow["max_3ph4w"] = 0.45;
			newRow["normal_3ph3w"] = 0.3;
			newRow["max_3ph3w"] = 0.45;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku16
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(16) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku17
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(17) , %";
			newRow["normal_3ph4w"] = 2;
			newRow["max_3ph4w"] = 3;
			newRow["normal_3ph3w"] = 2;
			newRow["max_3ph3w"] = 3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku18
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(18) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku19
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(19) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku20
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(20) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku21
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(21) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku22
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(22) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku23
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(23) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku24
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(24) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku25
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(25) , %";
			newRow["normal_3ph4w"] = 1.5;
			newRow["max_3ph4w"] = 2.25;
			newRow["normal_3ph3w"] = 1.5;
			newRow["max_3ph3w"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku26
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(26) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku27
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(27) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku28
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(28) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku29
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(29) , %";
			newRow["normal_3ph4w"] = 1.32;
			newRow["max_3ph4w"] = 1.98;
			newRow["normal_3ph3w"] = 1.32;
			newRow["max_3ph3w"] = 1.98;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku30
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(30) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku31
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(31) , %";
			newRow["normal_3ph4w"] = 1.25;
			newRow["max_3ph4w"] = 1.87;
			newRow["normal_3ph3w"] = 1.25;
			newRow["max_3ph3w"] = 1.87;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku32
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(32) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku33
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(33) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku34
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(34) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku35
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(35) , %";
			newRow["normal_3ph4w"] = 1.13;
			newRow["max_3ph4w"] = 1.69;
			newRow["normal_3ph3w"] = 1.13;
			newRow["max_3ph3w"] = 1.69;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku36
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(36) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku37
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(37) , %";
			newRow["normal_3ph4w"] = 1.08;
			newRow["max_3ph4w"] = 1.62;
			newRow["normal_3ph3w"] = 1.08;
			newRow["max_3ph3w"] = 1.62;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku38
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(38) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku39
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(39) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku40
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "Ku(40) , %";
			newRow["normal_3ph4w"] = 0.2;
			newRow["max_3ph4w"] = 0.3;
			newRow["normal_3ph3w"] = 0.2;
			newRow["max_3ph3w"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// Ku
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = ((settings_.CurrentLanguage == "ru") ? "Ku" : "THDu") + " , %";
			newRow["normal_3ph4w"] = 8;
			newRow["max_3ph4w"] = 12;
			newRow["normal_3ph3w"] = 8;
			newRow["max_3ph3w"] = 12;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType1_EtPQP_A()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 1

			// ∆F+ synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.4;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -0.2;
			newRow["val_100"] = -0.4;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// ∆F+ iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 5.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -1.0;
			newRow["val_100"] = -5.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// δU + , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU+" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// δU - , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU-" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// fliker short
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick short";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.38;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// fliker long
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick long";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(2) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(3) , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 7.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(4) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(5) , %";
			newRow["val_95"] = 6.0;
			newRow["val_100"] = 9.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(6) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(7) , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 7.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(8) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(9) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(10) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(11) , %";
			newRow["val_95"] = 3.5;
			newRow["val_100"] = 5.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(12) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(13) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(14) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(15) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(16) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(17) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(18) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(19) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(20) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(21) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(22) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(23) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(24) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(25) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(26) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(27) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(28) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(29) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(30) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(31) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(32) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(33) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(34) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(35) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(36) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(37) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(38) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(39) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(40) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// kHarm Total , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] =
				((settings_.CurrentLanguage == "ru") ? "kHarm тотал" : "kHarm total") + " , %";
			newRow["val_95"] = 8.0;
			newRow["val_100"] = 12.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 1;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType2_EtPQP_A()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 2

			// ∆F+ synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.4;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -0.2;
			newRow["val_100"] = -0.4;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// ∆F+ iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 5.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -1.0;
			newRow["val_100"] = -5.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// δU + , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU+" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// δU - , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU-" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// fliker short
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "fliker short";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.38;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// fliker long
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick long";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(2) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(3) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(4) , %";
			newRow["val_95"] = 0.7;
			newRow["val_100"] = 1.05;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(5) , %";
			newRow["val_95"] = 4.0;
			newRow["val_100"] = 6.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(6) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(7) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(8) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(9) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(10) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(11) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(12) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(13) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(14) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(15) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(16) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(17) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(18) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(19) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(20) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(21) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(22) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(23) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(24) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(25) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(26) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(27) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(28) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(29) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(30) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(31) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(32) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(33) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(34) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(35) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(36) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(37) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(38) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(39) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(40) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// kHarm Total , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] =
				((settings_.CurrentLanguage == "ru") ? "kHarm тотал" : "kHarm total") + " , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 8.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 2;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType3_EtPQP_A()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 3

			// ∆F+ synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.4;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -0.2;
			newRow["val_100"] = -0.4;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// ∆F+ iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 5.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -1.0;
			newRow["val_100"] = -5.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// δU + , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU+" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// δU - , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU-" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// fliker short
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "fliker short";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.38;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// fliker long
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick long";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(2) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(3) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(4) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(5) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(6) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(7) , %";
			newRow["val_95"] = 2.5;
			newRow["val_100"] = 3.75;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(8) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(9) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(10) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(11) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(12) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(13) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(14) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(15) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(16) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(17) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(18) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(19) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(20) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(21) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(22) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(23) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(24) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(25) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(26) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(27) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(28) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(29) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(30) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(31) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(32) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(33) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(34) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(35) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(36) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(37) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(38) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(39) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(40) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// kHarm Total , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] =
				((settings_.CurrentLanguage == "ru") ? "kHarm тотал" : "kHarm total") + " , %";
			newRow["val_95"] = 4.0;
			newRow["val_100"] = 6.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 3;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType4_EtPQP_A()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 4

			// ∆F+ synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.4;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -0.2;
			newRow["val_100"] = -0.4;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// ∆F+ iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 5.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -1.0;
			newRow["val_100"] = -5.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// δU + , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU+" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// δU - , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU-" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// fliker short
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "fliker short";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.38;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// fliker long
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick long";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(2) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(3) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(4) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(5) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(6) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(7) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(8) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(9) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(10) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(11) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(12) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(13) , %";
			newRow["val_95"] = 0.7;
			newRow["val_100"] = 1.05;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(14) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(15) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(16) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(17) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(18) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(19) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(20) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(21) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(22) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(23) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(24) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(25) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(26) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(27) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(28) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(29) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(30) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(31) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(32) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(33) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(34) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(35) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(36) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(37) , %";
			newRow["val_95"] = 0.4;
			newRow["val_100"] = 0.6;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(38) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(39) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(40) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// kHarm Total , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] =
				((settings_.CurrentLanguage == "ru") ? "kHarm тотал" : "kHarm total") + " , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 4;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType5_EtPQP_A()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 5

			// ∆F+ synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.4;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -0.2;
			newRow["val_100"] = -0.4;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// ∆F+ iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 5.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -1.0;
			newRow["val_100"] = -5.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// δU + , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU+" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// δU - , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU-" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// fliker short
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "fliker short";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.38;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// fliker long
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick long";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(2) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(3) , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 7.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(4) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(5) , %";
			newRow["val_95"] = 6.0;
			newRow["val_100"] = 9.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(6) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(7) , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 7.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(8) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(9) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(10) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(11) , %";
			newRow["val_95"] = 3.5;
			newRow["val_100"] = 5.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(12) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(13) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(14) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(15) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(16) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(17) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(18) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(19) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(20) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(21) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(22) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(23) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(24) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(25) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(26) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(27) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(28) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(29) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(30) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(31) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(32) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(33) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(34) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(35) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(36) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(37) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(38) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(39) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(40) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// kHarm Total , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] =
				((settings_.CurrentLanguage == "ru") ? "kHarm тотал" : "kHarm total") + " , %";
			newRow["val_95"] = 8.0;
			newRow["val_100"] = 12.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 5;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		private void createContraintsDataSetType6_EtPQP_A()
		{
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
				this.GetType().Assembly);
			DataRow newRow;

			#region Type 6

			// ∆F+ synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.4;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- synch, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- synch, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -0.2;
			newRow["val_100"] = -0.4;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// ∆F+ iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F+ isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 5.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// ∆F- iso, Hz
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "∆F- isolate, " + rm.GetString("name.hertz.short");
			newRow["val_95"] = -1.0;
			newRow["val_100"] = -5.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// δU + , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU+" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// δU - , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "δU-" + ", %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 10.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// fliker short
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "fliker short";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.38;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// fliker long
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "flick long";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 1.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(2) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(2) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(3) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(3) , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 7.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(4) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(4) , %";
			newRow["val_95"] = 1.0;
			newRow["val_100"] = 1.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(5) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(5) , %";
			newRow["val_95"] = 6.0;
			newRow["val_100"] = 9.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(6) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(6) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(7) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(7) , %";
			newRow["val_95"] = 5.0;
			newRow["val_100"] = 7.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(8) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(8) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(9) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(9) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(10) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(10) , %";
			newRow["val_95"] = 0.5;
			newRow["val_100"] = 0.75;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(11) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(11) , %";
			newRow["val_95"] = 3.5;
			newRow["val_100"] = 5.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(12) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(12) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(13) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(13) , %";
			newRow["val_95"] = 3.0;
			newRow["val_100"] = 4.5;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(14) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(14) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(15) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(15) , %";
			newRow["val_95"] = 0.3;
			newRow["val_100"] = 0.45;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(16) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(16) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(17) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(17) , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 3.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(18) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(18) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(19) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(19) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(20) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(20) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(21) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(21) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(22) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(22) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(23) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(23) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(24) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(24) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(25) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(25) , %";
			newRow["val_95"] = 1.5;
			newRow["val_100"] = 2.25;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(26) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(26) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(27) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(27) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(28) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(28) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(29) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(29) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(30) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(30) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(31) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(31) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(32) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(32) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(33) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(33) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(34) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(34) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(35) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(35) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(36) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(36) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(37) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(37) , %";
			newRow["val_95"] = 0.0;
			newRow["val_100"] = 0.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(38) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(38) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(39) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(39) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm(40) , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "kHarm(40) , %";
			newRow["val_95"] = 0.2;
			newRow["val_100"] = 0.3;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// kHarm Total , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] =
				((settings_.CurrentLanguage == "ru") ? "kHarm тотал" : "kHarm total") + " , %";
			newRow["val_95"] = 8.0;
			newRow["val_100"] = 12.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// K2u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K2u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			// K0u , %
			newRow = tableConstraints_.NewRow();
			newRow["param_name"] = "K0u , %";
			newRow["val_95"] = 2.0;
			newRow["val_100"] = 4.0;
			newRow["constraint_type_id"] = 6;
			tableConstraints_.Rows.Add(newRow);

			#endregion
		}

		///
		/// NOTE: THIS FILE IS GENERATED
		/// ACCORDING TO DATAGRIDVIEW INFORMATION
		
		private void btnSaveToFile_Click(object sender, EventArgs e)
		{
			SaveFileDialog fd = new SaveFileDialog();
			fd.DefaultExt = "consts";
			fd.AddExtension = true;
			fd.FileName = "unnamed.consts";
			fd.Filter = "Constraints files (*.consts)|*.consts|All files (*.*)|*.*";

			if (fd.ShowDialog(this) != DialogResult.OK) return;

			DataRow[] dr = tableConstraints_.Select("constraint_type_id = " + 
				(cmbConstraintType.SelectedIndex + 1));

			XmlDocument doc = new XmlDocument();			
			XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", null, "yes");
			doc.AppendChild(xmldecl);

			XmlNode root = doc.CreateNode(XmlNodeType.Element, "ds_constraints_names", "");
			XmlNode row, field, value;
			for (int i = 0; i < dr.Length; i++)
			{
				row = doc.CreateNode(XmlNodeType.Element, "table_constraints", "");
				for (int j = 0; j < tableConstraints_.Columns.Count-1; j++)
				{
					field = doc.CreateNode(XmlNodeType.Element, tableConstraints_.Columns[j].ColumnName, "");
					value = doc.CreateNode(XmlNodeType.Text, String.Empty, String.Empty);
					value.Value = Convert.ToString(dr[i][j], new System.Globalization.CultureInfo("en-US"));
					field.AppendChild(value);
					row.AppendChild(field);
				}
				root.AppendChild(row);
			}
			doc.AppendChild(root);
			doc.Save(fd.FileName);
		}

		private void btnLoadFromFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog();
			fd.DefaultExt = "consts";
			fd.AddExtension = true;
			fd.Filter = "Constraints files (*.consts)|*.consts|All files (*.*)|*.*";

			if (fd.ShowDialog(this) != DialogResult.OK) return;

			Int32 const_type = cmbConstraintType.SelectedIndex + 1;

			try
			{
				int colCount = tableConstraints_.Columns.Count;

				// deleting old values
				DataRow[] dr = tableConstraints_.Select("constraint_type_id = " + 
					(cmbConstraintType.SelectedIndex + 1));
				for (int i = 0; i < dr.Length; i++)
				{
					dr[i].Delete();
				}

				// trying to add new values
				XmlDocument doc = new XmlDocument();
				doc.Load(fd.FileName);
				XmlNode root = doc.ChildNodes[1];
				for (int i = 0; i < root.ChildNodes.Count; i++)
				{
					object[] arr = new object[colCount];
					arr[0] = root.ChildNodes[i].ChildNodes[0].InnerText;
					for (int k = 1; k <= colCount - 2; k++)
					{
						arr[k] = System.Convert.ToSingle(root.ChildNodes[i].ChildNodes[k].InnerText, 
							new System.Globalization.CultureInfo("en-US"));
					}
					arr[colCount - 1] = (System.Int32)const_type;
					tableConstraints_.LoadDataRow(arr, false);
				}

				tableConstraints_.AcceptChanges();

				for (int i = 0; i < dgvConstraints.Rows.Count; i++)
				{
					for (int j = 1; j < dgvConstraints.Columns.Count - 1; j += 2)
					{
						ValidateCell(i, j);
					}
				}
				MarkErrCells(cmbConstraintType.SelectedIndex - 4);
			}
			catch
			{
				MessageBoxes.FileReadError(this, fd.FileName);
			}
		}

		private void cmbConstraintType_SelectedIndexChanged(object sender, EventArgs e)
		{
			// если уставки пользовательские
			btnLoadFromFile.Enabled = cmbConstraintType.SelectedIndex >= 4;

			// если ГОСТ, то read only
			dgvConstraints.Columns[1].ReadOnly = cmbConstraintType.SelectedIndex < 4;
			dgvConstraints.Columns[2].ReadOnly = cmbConstraintType.SelectedIndex < 4;
			if (settings_.CurDeviceType != EmDeviceType.ETPQP_A)
			{
				dgvConstraints.Columns[3].ReadOnly = cmbConstraintType.SelectedIndex < 4;
				dgvConstraints.Columns[4].ReadOnly = cmbConstraintType.SelectedIndex < 4;
			}

			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
				this.GetType().Assembly);
			string header_prefix = rm.GetString("name.constraint_type.header_prefix");
			lblObjectNames.Text = header_prefix + cmbConstraintType.Text;
			if (settings_.CurDeviceType == EmDeviceType.ETPQP_A)
			{
				lblObjectNames.Text =
					lblObjectNames.Text.Replace("13109-97", "32144-2013");
			}

			// если ГОСТ
			if (cmbConstraintType.SelectedIndex < 4)
			{
				btnSaveToFile.Enabled = true;
			}
		}

		private void btnLoadFromDevice_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				object[] port_params = null;
				if (settings_.IOInterface == EmPortType.USB)
				{
					port_params = null;
				}
				else if (settings_.IOInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				else if (settings_.IOInterface == EmPortType.Modem)
				{
					port_params = new object[5];
					port_params[0] = settings_.SerialPortNameModem;
					port_params[1] = settings_.SerialSpeedModem;
					port_params[2] = settings_.CurPhoneNumber;
					port_params[3] = settings_.AttemptNumber;
					port_params[4] = settings_.CurDeviceAddress;
				}
				else if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.WI_FI)
				{
					try
					{
						if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
						{
							WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
																						settings_.WifiPassword);

							if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
							{
								EmService.WriteToLogFailed("frmObjectNames: Wi-fi not connected!");
								MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
								return;
							}
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "frmObjectNames: Exception in ConnectToWifi() WI-FI:");
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					port_params = new object[2];
					port_params[0] = EmService.GetCurrentDhcpIpAddress();
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.GPRS)
				{
					port_params = new object[3];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
					port_params[2] = settings_.CurDeviceAddress;
				}
				else if (settings_.IOInterface == EmPortType.Rs485)
				{
					port_params = new object[3];
					port_params[0] = settings_.SerialPortName485;
					port_params[1] = settings_.SerialSpeed485;
					port_params[2] = settings_.CurDeviceAddress;
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				EmDevice device = null;
				switch (settings_.CurDeviceType)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
						device = new Em33TDevice(settings_.IOInterface,
							settings_.CurDeviceAddress, false, port_params, 
							(mainForm_ as Form).Handle);
						break;

					case EmDeviceType.EM31K:
						device = new Em31KDevice(settings_.IOInterface,
							settings_.CurDeviceAddress, false, port_params, 
							(mainForm_ as Form).Handle);
						break;

					case EmDeviceType.EM32:
						device = new Em32Device(settings_.IOInterface, 
							settings_.CurDeviceAddress, false, port_params, 
							(mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP:
						device = new EtPQPDevice(settings_.IOInterface,
							settings_.CurDeviceAddress, false, port_params, 
							(mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP_A:
						device = new EtPQP_A_Device(settings_.IOInterface,
							settings_.CurDeviceAddress, false, port_params, settings_.CurWifiProfileName, settings_.WifiPassword,
							(mainForm_ as Form).Handle);
						break;

					default:
						MessageBoxes.UnknownDevType(this);
						return;
				}

				float[, ,] vals = null;
                //float[,] valsEtPqpA = null;
				try
				{
					Int64 serial = device.OpenDevice();
					if (serial == -1)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					if (!settings_.Licences.IsLicenced(serial))
					{
						MessageBoxes.DeviceIsNotLicenced(this);
						return;
					}

					byte[] buffer = null;
					ExchangeResult errCode = ExchangeResult.Other_Error;

					if (settings_.CurDeviceType == EmDeviceType.EM33T ||
						settings_.CurDeviceType == EmDeviceType.EM33T1)
					{
						EM33TConstraints pageStandSettings = new EM33TConstraints();
						errCode = (device as Em33TDevice).Read(DeviceIO.Memory.EMemory.FRAM,
							pageStandSettings.Address,
							(ushort)(pageStandSettings.Size / 2),  // здесь нужен рамер в словах
							ref buffer, false);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
						if (!pageStandSettings.Parse(ref buffer))
							errCode = ExchangeResult.Parse_Error;

						vals = pageStandSettings.ConstraintsForTable;
					}
					else if (settings_.CurDeviceType == EmDeviceType.EM31K)
					{
						EM33TConstraints pageStandSettings = new EM33TConstraints();
						errCode = (device as Em31KDevice).Read(DeviceIO.Memory.EMemory.FRAM,
							pageStandSettings.Address,
							(ushort)(pageStandSettings.Size / 2),  // здесь нужен рамер в словах
							ref buffer, false);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
						if (!pageStandSettings.Parse(ref buffer))
							errCode = ExchangeResult.Parse_Error;

						vals = pageStandSettings.ConstraintsForTable;
					}
					else if (settings_.CurDeviceType == EmDeviceType.EM32)
					{
						EMSLIPConstraints pageStandSettings = new EMSLIPConstraints();
						errCode = (device as Em32Device).Read(EmCommands.COMMAND_ReadSets_PQP,
																ref buffer, null);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
						if (!pageStandSettings.Parse(ref buffer))
							errCode = ExchangeResult.Parse_Error;

						vals = pageStandSettings.ConstraintsForTable;
					}

					else if (settings_.CurDeviceType == EmDeviceType.ETPQP)
					{
						EMSLIPConstraints pageStandSettings = new EMSLIPConstraints();
						errCode = (device as EtPQPDevice).Read(EmCommands.COMMAND_ReadSets_PQP,
																ref buffer, null);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
						if (!pageStandSettings.Parse(ref buffer))
							errCode = ExchangeResult.Parse_Error;

						vals = pageStandSettings.ConstraintsForTable;
					}

					else if (settings_.CurDeviceType == EmDeviceType.ETPQP_A)
					{
                        EtPQPAConstraints pageStandSettings = new EtPQPAConstraints();
						errCode = (device as EtPQP_A_Device).Read(EmCommands.COMMAND_ReadSets_PQP_A,
																ref buffer, null);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
						if (!pageStandSettings.Parse(ref buffer))
							errCode = ExchangeResult.Parse_Error;

                        vals = pageStandSettings.ConstraintsForTable;
					}
					else return;
				}
				finally
				{
					if (device != null) device.Close();
				}
				
				ClearErrCell(0);
				ClearErrCell(1);

				// fixing bug!
				// удаляем из таблицы гостовские уставки, чтобы перезаписать их зашитыми в программе
				// (если в приборе были зашиты неверные уставки)
				DataRow[] dr = tableConstraints_.Select("constraint_type_id = 1 OR constraint_type_id = 2 OR constraint_type_id = 3 OR constraint_type_id = 4");
				for (int i = 0; i < dr.Length; i++)
				{
					dr[i].Delete();
				}
				if (settings_.CurDeviceType != EmDeviceType.ETPQP_A)
				{
					createContraintsDataSetType1();
					createContraintsDataSetType2();
					createContraintsDataSetType3();
					createContraintsDataSetType4();
				}
				else
				{
					createContraintsDataSetType1_EtPQP_A();
					createContraintsDataSetType2_EtPQP_A();
					createContraintsDataSetType3_EtPQP_A();
					createContraintsDataSetType4_EtPQP_A();
				}
				tableConstraints_.AcceptChanges();
				// end fixing bug!

				bool[] okValues = new bool[4] { true, true, true, true };

				if (settings_.CurDeviceType == EmDeviceType.EM33T ||
					settings_.CurDeviceType == EmDeviceType.EM33T1 ||
					settings_.CurDeviceType == EmDeviceType.EM31K)
				{
					for (int iSet = 0; iSet < EM33TConstraints.CntConstraintsSets; iSet++)
					{
						dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", iSet + 1));
						for (int iRow = 0; iRow < (EM33TConstraints.CntConstraints / 2); iRow++)
						{
							for (int iCol = 0; iCol < 4; iCol++)
							{
								vals[iSet, iRow, iCol] = (float)Math.Round(vals[iSet, iRow, iCol], 2);
								if ((float)(dr[iRow][iCol + 1]) != vals[iSet, iRow, iCol])
								{
									if (iSet < 4)	// 4 - кол-во наборов ГОСТовских уставок
									{
										okValues[iSet] = false;
									}
									dr[iRow][iCol + 1] = vals[iSet, iRow, iCol];
								}
							}
						}
					}
				}
				else if(settings_.CurDeviceType == EmDeviceType.ETPQP_A)
				{
					for (int iSet = 0; iSet < EtPQPAConstraints.CntConstraintsSets; iSet++)
					{
						dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", iSet + 1));
						for (int iRow = 0; iRow < (EtPQPAConstraints.CntConstraints / 2); iRow++)
						{
							for (int iCol = 0; iCol < 2; iCol++)
							{
								vals[iSet, iRow, iCol] = (float)Math.Round(vals[iSet, iRow, iCol], 2);
								if ((float)(dr[iRow][iCol + 1]) != vals[iSet, iRow, iCol])
								{
									if (iSet < 4)	// 4 - кол-во наборов ГОСТовских уставок
									{
										//float tmp1 = (float)(dr[iRow][iCol + 1]);
										//float tmp2 = vals[iSet, iRow, iCol];
										okValues[iSet] = false;
									}
									dr[iRow][iCol + 1] = vals[iSet, iRow, iCol];
								}
							}
						}
					}
				}
				else
				{
					for (int iSet = 0; iSet < EMSLIPConstraints.CntConstraintsSets; iSet++)
					{
						dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", iSet + 1));
						for (int iRow = 0; iRow < (EMSLIPConstraints.CntConstraints / 2); iRow++)
						{
							for (int iCol = 0; iCol < 4; iCol++)
							{
								// строки 6 и 7 это суточные dU, их не показываем
								int curTableRow = iRow;
								if (iRow > 7) curTableRow = iRow - 2;

								vals[iSet, iRow, iCol] =
									(float)Math.Round(vals[iSet, iRow, iCol], 2);

								if (iRow != 6 && iRow != 7 &&
									(float)(dr[curTableRow][iCol + 1]) != vals[iSet, iRow, iCol])
								{
									if (iSet < 4)	// 4 - кол-во наборов ГОСТовских уставок
									{
										okValues[iSet] = false;
									}

									dr[curTableRow][iCol + 1] = vals[iSet, iRow, iCol];
								}
							}
						}
					}
				}
				tableConstraints_.AcceptChanges();

				frmConstraintsLoadDetails wndDetails = new frmConstraintsLoadDetails(settings_, okValues);
				wndDetails.ShowDialog(this);

				return;
			}
			catch (EmDisconnectException dex)
			{
				MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
				EmService.DumpException(dex, "Error in btnLoadFromDevice_Click:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (EmException emx)
			{
				MessageBoxes.ErrorLoadConstraints(this, emx.Message);
				EmService.DumpException(emx, "Error in btnLoadFromDevice_Click:");
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmConstraints::btnLoadFromDevice_Click():");
				throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void btnSaveToDevice_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;

				// Функция не работает для Эм 3.1К
				if (settings_.CurDeviceType == EmDeviceType.EM31K)
				{
					MessageBoxes.InvalidDeviceFunction(this, EmDeviceType.EM31K.ToString());
					return;
				}

				ExchangeResult errCode = ExchangeResult.Other_Error;
				object[] port_params = null;

				if (settings_.IOInterface == EmPortType.USB)
				{
					port_params = null;
				}
				else if (settings_.IOInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				else if (settings_.IOInterface == EmPortType.Modem)
				{
					port_params = new object[5];
					port_params[0] = settings_.SerialPortNameModem;
					port_params[1] = settings_.SerialSpeedModem;
					port_params[2] = settings_.CurPhoneNumber;
					port_params[3] = settings_.AttemptNumber;
					port_params[4] = settings_.CurDeviceAddress;
				}
				else if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.WI_FI)
				{
					try
					{
						if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
						{
							WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
																						settings_.WifiPassword);

							if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
							{
								EmService.WriteToLogFailed("frmObjectNames: Wi-fi not connected!");
								MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
								return;
							}
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "frmObjectNames: Exception in ConnectToWifi() WI-FI:");
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
						return;
					}

					port_params = new object[2];
					port_params[0] = EmService.GetCurrentDhcpIpAddress();
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.GPRS)
				{
					port_params = new object[3];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
					port_params[2] = settings_.CurDeviceAddress;
				}
				else if (settings_.IOInterface == EmPortType.Rs485)
				{
					port_params = new object[3];
					port_params[0] = settings_.SerialPortName485;
					port_params[1] = settings_.SerialSpeed485;
					port_params[2] = settings_.CurDeviceAddress;
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				EmDevice device = null;
				switch (settings_.CurDeviceType)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
						device = new Em33TDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					//case EmDeviceType.EM31K:
					//    device = new Em31KDevice(settings_.IOInterface, settings_.CurDeviceAddress,
					//                             false, port_params, (mainForm_ as Form).Handle);
					//    break;

					case EmDeviceType.EM32:
						device = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP:
						device = new EtPQPDevice(settings_.IOInterface, settings_.CurDeviceAddress,
													 false, port_params, (mainForm_ as Form).Handle);
						break;

					case EmDeviceType.ETPQP_A:
						device = new EtPQP_A_Device(settings_.IOInterface,
									settings_.CurDeviceAddress,
									false, port_params, settings_.CurWifiProfileName, settings_.WifiPassword, 
									(mainForm_ as Form).Handle);
						break;

					default:
						MessageBoxes.UnknownDevType(this);
						return;
				}

				try
				{
					Int64 serial = device.OpenDevice();
					if (serial == -1)
					{
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
						return;
					}

					if (!settings_.Licences.IsLicenced(serial))
					{
						MessageBoxes.DeviceIsNotLicenced(this);
						return;
					}

					// preparing to write
					// fixing bug!
					// удаляем из таблицы гостовские уставки, чтобы перезаписать их зашитыми в программе
					DataRow[] dr = tableConstraints_.Select("constraint_type_id = 1 OR constraint_type_id = 2 OR constraint_type_id = 3 OR constraint_type_id = 4");
					for (int i = 0; i < dr.Length; i++)
					{
						dr[i].Delete();
					}
					if (settings_.CurDeviceType != EmDeviceType.ETPQP_A)
					{
						createContraintsDataSetType1();
						createContraintsDataSetType2();
						createContraintsDataSetType3();
						createContraintsDataSetType4();
					}
					else
					{
						createContraintsDataSetType1_EtPQP_A();
						createContraintsDataSetType2_EtPQP_A();
						createContraintsDataSetType3_EtPQP_A();
						createContraintsDataSetType4_EtPQP_A();
					}
					tableConstraints_.AcceptChanges();
					// end fixing bug!

					if (settings_.CurDeviceType == EmDeviceType.EM31K ||
						settings_.CurDeviceType == EmDeviceType.EM33T ||
						settings_.CurDeviceType == EmDeviceType.EM33T1)
					{
						EM33TConstraints pageStandSettings = new EM33TConstraints();

						float[, ,] vals = pageStandSettings.ConstraintsForTable;

						for (int iSet = 0; iSet < EMSLIPConstraints.CntConstraintsSets; ++iSet)
						{
							dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", iSet + 1));
							for (int j = 0; j < EM33TConstraints.CntConstraints / 2; j++)
							{
								for (int k = 0; k < EM33TConstraints.CntSubsets * 2; k++)
								{
									vals[iSet, j, k] = (float)(dr[j][k + 1]);
								}
							}
						}
						pageStandSettings.ConstraintsForTable = vals;
						byte[] buffer = pageStandSettings.Pack();

						errCode = (device as Em33TDevice).Write(DeviceIO.Memory.EMemory.FRAM,
										pageStandSettings.Address, (ushort)(pageStandSettings.Size / 2),
										ref buffer);
					}
					else if (settings_.CurDeviceType == EmDeviceType.EM32)
					{
						EMSLIPConstraints pageStandSettings = new EMSLIPConstraints();

						float[, ,] vals = pageStandSettings.EmptyConstraintsForTable;

						for (int iSet = 0; iSet < EMSLIPConstraints.CntConstraintsSets; ++iSet)
						{
							dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", 
								iSet + 1));

							for (int j = 0; j < EMSLIPConstraints.CntConstraints / 2; j++)
							{
								// 4 - кол-во данных в одной строке таблицы: НДП (3ф4пр), ПДП (3ф4пр),
								// НДП (3ф3пр), ПДП (3ф3пр)
								for (int k = 0; k < EMSLIPConstraints.CntSubsets * 2; k++)
								{
									// для суточных значений dU берем значения по формуле:
									// НДЗн δUy = НДЗн δUy”  НДЗв δUy = НДЗв δUy’
									// ПДЗн δUy = ПДЗн δUy”  ПДЗв δUy = ПДЗн δUy’
									if (j < 6)
										vals[iSet, j, k] = (float)(dr[j][k + 1]);
									else if (j == 6)
									{
										vals[iSet, j, k] = (float)(dr[4][k + 1]);
									}
									else if (j == 7)
									{
										vals[iSet, j, k] = (float)(dr[3][k + 1]);
									}
									else if (j > 7)
										vals[iSet, j, k] = (float)(dr[j - 2][k + 1]);
								}
							}
						}

						pageStandSettings.ConstraintsForTable = vals;
						byte[] buffer = pageStandSettings.Pack();

						errCode = (device as Em32Device).WriteSets(ref buffer);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
					}
					else if (settings_.CurDeviceType == EmDeviceType.ETPQP)
					{
						EMSLIPConstraints pageStandSettings = new EMSLIPConstraints();

						float[,,] vals = pageStandSettings.EmptyConstraintsForTable;

						for (int iSet = 0; iSet < EMSLIPConstraints.CntConstraintsSets; ++iSet)
						{
							dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", iSet + 1));

							for (int j = 0; j < EMSLIPConstraints.CntConstraints / 2; j++)
							{
								// 4 - кол-во данных в одной строке таблицы: НДП (3ф4пр), ПДП (3ф4пр),
								// НДП (3ф3пр), ПДП (3ф3пр)
								for (int k = 0; k < EMSLIPConstraints.CntSubsets * 2; k++)
								{
									// для суточных значений dU берем значения по формуле:
									// НДЗн δUy = НДЗн δUy”  НДЗв δUy = НДЗв δUy’
									// ПДЗн δUy = ПДЗн δUy”  ПДЗв δUy = ПДЗн δUy’
									if (j < 6)
										vals[iSet, j, k] = (float)(dr[j][k + 1]);
									else if (j == 6)
									{
										vals[iSet, j, k] = (float)(dr[4][k + 1]);
									}
									else if (j == 7)
									{
										vals[iSet, j, k] = (float)(dr[3][k + 1]);
									}
									else if (j > 7)
										vals[iSet, j, k] = (float)(dr[j - 2][k + 1]);
								}
							}
						}
						pageStandSettings.ConstraintsForTable = vals;
						byte[] buffer = pageStandSettings.Pack();

						errCode = (device as EtPQPDevice).WriteSets(ref buffer);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
					}
					else if (settings_.CurDeviceType == EmDeviceType.ETPQP_A)
					{
						EtPQPAConstraints pageStandSettings = new EtPQPAConstraints();

						float[, ,] vals = pageStandSettings.EmptyConstraintsForTable;

						for (int iSet = 0; iSet < EtPQPAConstraints.CntConstraintsSets; ++iSet)
						{
							dr = tableConstraints_.Select(String.Format("constraint_type_id = {0}", iSet + 1));

							for (int j = 0; j < EtPQPAConstraints.CntConstraints / 2; j++)
							{
								// 2 - кол-во данных в одной строке таблицы: 95 и 100
								for (int k = 0; k < 2; k++)
								{
									vals[iSet, j, k] = (float)(dr[j][k + 1]);
								}
							}
						}
						pageStandSettings.ConstraintsForTable = vals;
						byte[] buffer = null;
						Int32 checkSum1, checkSum2;
						if (!pageStandSettings.Pack(ref buffer, out checkSum1, out checkSum2))
							throw new EmException("pageStandSettings.Pack() error! EtPQP-A");

						errCode = (device as EtPQP_A_Device).WriteSets(ref buffer, checkSum1, checkSum2);
						if (errCode != ExchangeResult.OK)
						{
							MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								settings_.IOParameters);
							return;
						}
					}
					else return;
				}
				finally
				{
					if (device != null) device.Close();
				}

				if (errCode == ExchangeResult.OK)			// no errors
				{
					MessageBoxes.DeviceConstraintsSaved(this, settings_.CurDeviceType);
					return;
				}
				else if (errCode == ExchangeResult.Parse_Error || errCode == ExchangeResult.Read_Error)
				{
					MessageBoxes.DeviceReadError(this);
					return;
				}
				else if (errCode != ExchangeResult.OK)		// device connection error
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface, settings_.IOParameters);
					return;
				}
			}
			catch (EmDisconnectException dex)
			{
				MessageBoxes.ErrorConnectDevice(this);
				EmService.DumpException(dex, "Error in btnSaveToDevice_Click:");
				Kernel32.PostMessage(mainForm_.Handle, EmService.WM_USER + 3, 0, 0);
				return;
			}
			catch (EmException emx)
			{
				MessageBoxes.ErrorSaveConstraints(this, emx.Message);
				EmService.DumpException(emx, "Error in btnSaveToDevice_Click:");
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmConstraints::btnSaveToDevice_Click():");
				throw;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void dgvConstraints_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			try
			{
				MessageBoxes.ConstraintsInputDataError((sender as DataGridView).Parent);
				e.ThrowException = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgvConstraints_DataError():");
				throw;
			}
		}

		private void dgvConstraints_CellValidating(object sender,
			DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				if (settings_.CurDeviceType == EmDeviceType.ETPQP_A) return;

				if ((sender as DataGridView).EditingControl == null)
				{
					return;
				}

				try
				{
					double value = double.Parse((sender as DataGridView).EditingControl.Text);
					value = Math.Abs(value);

					if (value > dblConstHighLimitConstraints_)
					{
						MessageBoxes.ConstraintsInputDataErrorHighLimit((sender as DataGridView).Parent);
						e.Cancel = true;
					}
				}
				catch
				{
					e.Cancel = false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgvConstraints_CellValidating():");
				throw;
			}
		}

		private void dgvConstraints_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (settings_.CurDeviceType == EmDeviceType.ETPQP_A) return;

				ModifyAndValidateCells(e.RowIndex, e.ColumnIndex);

				if (cmbConstraintType.SelectedIndex >= 4)
				{
					MarkErrCells(cmbConstraintType.SelectedIndex - 4);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in dgvConstraints_CellEndEdit():");
				throw;
			}
		}

		private int GetCellsIndex(List<int[]> list, int[] cells)
		{
			try
			{
				if (list == null || cells == null) return -1;
				if (cells.Length != 4) return -1;
				if (list.Count < 0) return -1;

				int index = -1;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i][0] == cells[0] &&
						list[i][1] == cells[1] &&
						list[i][2] == cells[2] &&
						list[i][3] == cells[3])
					{
						index = i;
						break;
					}
				}
				return index;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetCellsIndex():");
				throw;
			}
		}

		private void DuplicateValueToOtherColumn(int row, int col, out int colNew)
		{
			try
			{
				if (row > 7)
				{
					colNew = -1;
					return;
				}

				if (col <= 2)
				{
					colNew = col + 2;
				}
				else
				{
					colNew = col - 2;
				}

				dgvConstraints.Rows[row].Cells[colNew].Value =
					dgvConstraints.Rows[row].Cells[col].Value;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in DuplicateValueToOtherColumn():");
				throw;
			}
		}

		private void ModifyAndValidateCells(int row, int col)
		{
			try
			{
				if (settings_.CurDeviceType == EmDeviceType.ETPQP_A) return;

				int colNew;
				DuplicateValueToOtherColumn(row, col, out colNew);

				ValidateCell(row, col);
				if (colNew != -1)
				{
					ValidateCell(row, colNew);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ModifyAndValidateCells():");
				throw;
			}
		}

		private void ValidateCell(int row, int col)
		{
			try
			{
				if (settings_.CurDeviceType == EmDeviceType.ETPQP_A) return;

				int ColumnShift = 0;
				if (col == 1 || col == 2) ColumnShift = 1;
				if (col == 3 || col == 4) ColumnShift = 3;

				DataGridViewTextBoxCell cell_095 =
					(dgvConstraints.Rows[row].Cells[ColumnShift] as DataGridViewTextBoxCell);
				DataGridViewTextBoxCell cell_100 =
					(dgvConstraints.Rows[row].Cells[ColumnShift + 1] as DataGridViewTextBoxCell);

				float val_095 = (float)Math.Round((float)cell_095.Value, 2);
				float val_100 = (float)Math.Round((float)cell_100.Value, 2);

				bool bInvalidCellPair = false;
				//if (settings_.CurDeviceType == EmDeviceType.EM32 ||
				//	settings_.CurDeviceType == EmDeviceType.ETPQP)
				//{
				//	if (((row == 1 || row == 3 || row == 5 || row == 7) && val_100 >= val_095) ||
				//		((row != 1 && row != 3 && row != 5 && row != 7) && val_100 <= val_095))
				//		bInvalidCellPair = true;
				//}
				//else
				//{
				if (((row == 1 || row == 3 || row == 5) && val_100 >= val_095) ||
					((row != 1 && row != 3 && row != 5) && val_100 <= val_095))
					bInvalidCellPair = true;
				//}

				int[] cellsPair = new int[4] { 
					cell_095.ColumnIndex, cell_095.RowIndex, cell_100.ColumnIndex, cell_100.RowIndex };

				if (bInvalidCellPair)
				{
					cell_095.Style.ForeColor = errCellColor_;
					cell_100.Style.ForeColor = errCellColor_;

					if (GetCellsIndex(errorCells_[cmbConstraintType.SelectedIndex - 4], cellsPair) < 0)
						errorCells_[cmbConstraintType.SelectedIndex - 4].Add(cellsPair);
				}
				else
				{
					cell_095.Style.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
					cell_100.Style.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
					int index = GetCellsIndex(errorCells_[cmbConstraintType.SelectedIndex - 4], cellsPair);
					if (index > -1)
						errorCells_[cmbConstraintType.SelectedIndex - 4].RemoveAt(index);
				}

				if (bInvalidCellPair)
				{
					cell_095.Style.ForeColor = errCellColor_;
					cell_100.Style.ForeColor = errCellColor_;

					if (GetCellsIndex(errorCells_[cmbConstraintType.SelectedIndex - 4], cellsPair) < 0)
						errorCells_[cmbConstraintType.SelectedIndex - 4].Add(cellsPair);
				}
				else
				{
					cell_095.Style.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
					cell_100.Style.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
					int index = GetCellsIndex(errorCells_[cmbConstraintType.SelectedIndex - 4], cellsPair);
					if (index > -1)
						errorCells_[cmbConstraintType.SelectedIndex - 4].RemoveAt(index);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ValidateCell():");
				throw;
			}
		}

		private void ClearErrCell(int page)
		{
			errorCells_[page].Clear();
		}

		private void MarkErrCells(int page)
		{
			try
			{
				if (settings_.CurDeviceType == EmDeviceType.ETPQP_A) return;

				for (int i = 0; i < errorCells_[page].Count; i++)
				{
					DataGridViewTextBoxCell cell_095 =
						dgvConstraints.Rows[errorCells_[page][i][1]].Cells[errorCells_[page][i][0]] as
						DataGridViewTextBoxCell;
					DataGridViewTextBoxCell cell_100 =
						dgvConstraints.Rows[errorCells_[page][i][3]].Cells[errorCells_[page][i][2]] as
						DataGridViewTextBoxCell;

					cell_095.Style.ForeColor = errCellColor_;
					cell_100.Style.ForeColor = errCellColor_;
				}

				if (errorCells_[page].Count > 0)
				{
					btnSaveToDevice.Enabled = false;
					btnSaveToFile.Enabled = false;
				}
				else
				{
					btnSaveToDevice.Enabled = true;
					btnSaveToFile.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in MarkErrCells():");
				throw;
			}
		}

		private void btnGenerateCode_Click(object sender, EventArgs e)
		{
			string fname = "generated.functions";
			FileStream fs = new FileStream(fname, FileMode.Create);
			StreamWriter sr = new StreamWriter(fs, System.Text.Encoding.UTF8);

			#region CODING: saving code of default creating datarows
			try
			{
				sr.WriteLine("/// NOTE: THIS FILE IS GENERATED");
				sr.WriteLine("/// ACCORDING TO DATAGRIDVIEW INFORMATION");
				sr.WriteLine("///");
				sr.WriteLine();

				for (int i = 0; i < 6; i++)
				{
					sr.WriteLine(String.Format("private void _createContraintsDataSetType{0}()", i + 1));
					sr.WriteLine("{");
					sr.WriteLine("\tResourceManager rm = new ResourceManager(\"EnergomonitoringXP.emstrings\", this.GetType().Assembly);");
					sr.WriteLine("\tDataRow newRow;");
					sr.WriteLine();
					sr.WriteLine(String.Format("\t#region Type {0}\n", i + 1));

					DataRow[] dr = tableConstraints_.Select(
						String.Format("constraint_type_id = {0}", i + 1));
					for (int j = 0; j < dr.Length; j++)
					{
						sr.WriteLine("\t" + @"// " + dr[j][0]);
						sr.WriteLine("\tnewRow = myTable.NewRow();");
						if (j == 0) sr.WriteLine("\tnewRow[\"param_name\"] = \"∆F+ , \" + rm.GetString(\"name.hertz.short\");");
						else if (j == 1) sr.WriteLine("\tnewRow[\"param_name\"] = \"∆F- , \" + rm.GetString(\"name.hertz.short\");");
						else if (j == 2) sr.WriteLine("\tnewRow[\"param_name\"] = ((settings.CurrentLanguage == \"ru\") ? \"δU''+\" : \"δUs''+\") + \" , %\";");
						else if (j == 3) sr.WriteLine("\tnewRow[\"param_name\"] = ((settings.CurrentLanguage == \"ru\") ? \"δU''-\" : \"δUs''-\") + \" , %\";");
						else if (j == 4) sr.WriteLine("\tnewRow[\"param_name\"] = ((settings.CurrentLanguage == \"ru\") ? \"δU'+\" : \"δUs'+\") + \" , %\";");
						else if (j == 5) sr.WriteLine("\tnewRow[\"param_name\"] = ((settings.CurrentLanguage == \"ru\") ? \"δU'-\" : \"δUs'-\") + \" , %\";");
						else if (j == 6) sr.WriteLine("\tnewRow[\"param_name\"] = \"K2u , %\";");
						else if (j == 7) sr.WriteLine("\tnewRow[\"param_name\"] = \"K0u , %\";");
						else if (j == dr.Length - 1) sr.WriteLine("newRow[\"param_name\"] = ((settings.CurrentLanguage == \"ru\") ? \"Ku\" : \"THDu\") + \" , %\";");
						else
							sr.WriteLine(String.Format("\tnewRow[\"param_name\"] = \"Ku({0}) , %\";", j-6));

						sr.WriteLine(String.Format("\tnewRow[\"normal_3ph4w\"] = {0};", ((float)dr[j][1]).ToString(new System.Globalization.CultureInfo("en-US"))));
						sr.WriteLine(String.Format("\tnewRow[\"max_3ph4w\"] = {0};", ((float)dr[j][2]).ToString(new System.Globalization.CultureInfo("en-US"))));
						sr.WriteLine(String.Format("\tnewRow[\"normal_3ph3w\"] = {0};", ((float)dr[j][3]).ToString(new System.Globalization.CultureInfo("en-US"))));
						sr.WriteLine(String.Format("\tnewRow[\"max_3ph3w\"] = {0};", ((float)dr[j][4]).ToString(new System.Globalization.CultureInfo("en-US"))));
						sr.WriteLine(String.Format("\tnewRow[\"constraint_type_id\"] = {0};", i + 1));
						sr.WriteLine("\tmyTable.Rows.Add(newRow);");
						sr.WriteLine(String.Empty);
					}
					sr.WriteLine("\t#endregion");
					sr.WriteLine("}");
					sr.WriteLine();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmConstraints.btnGenerateCode_Click(): ");
				throw;
			}
			finally
			{
				if (sr != null) sr.Close();
				if (fs != null) fs.Close();
			}

			//MessageBox.Show("Код уставок успешно сохранен в файл \"" + fname + "\"",
			//    "Поздравляю ;)", MessageBoxButtons.OK, MessageBoxIcon.Information);
			MessageBoxes.ConstraintsSaved(this, fname);

			#endregion
		}
		
		private void btnFLASH_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialog fd = new OpenFileDialog();
				fd.DefaultExt = "rm";
				fd.AddExtension = true;
				fd.Filter = "FLASH RAM files (*.rm)|*.rm|All files (*.*)|*.*";

				if (fd.ShowDialog(this) != DialogResult.OK) return;

				StreamReader r = new StreamReader(fd.FileName);
				String data = r.ReadToEnd();
				r.Close();

				string[] arr = data.Split(new string[] { "\r\n", " " }, StringSplitOptions.RemoveEmptyEntries);

				ushort address;
				ushort.TryParse(arr[0], out address);
				ushort size;
				ushort.TryParse(arr[1], out size);

				ushort[] vals = new ushort[arr.Length - 2];

				for (int i = 0; i < vals.Length; i++)
				{
					ushort.TryParse(arr[i + 2], System.Globalization.NumberStyles.HexNumber,
						System.Globalization.CultureInfo.InvariantCulture, out vals[i]);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in btnFLASH_Click():");
				throw;
			}
		}

		private void dgvConstraints_DataBindingComplete(object sender, 
			DataGridViewBindingCompleteEventArgs e)
		{
			if (cmbConstraintType.SelectedIndex >= 4)
			{
				MarkErrCells(cmbConstraintType.SelectedIndex - 4);
			}
		}

		private void tb_KeyPress(object sender, KeyPressEventArgs e)
		{
			//string vlCell = ((TextBox)sender).Text;

			if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',' && e.KeyChar != '\b')
				e.Handled = true;
		}

		private void dgvConstraints_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			TextBox tb = (TextBox)e.Control;
			tb.KeyPress += new KeyPressEventHandler(tb_KeyPress);
		}
	}
}