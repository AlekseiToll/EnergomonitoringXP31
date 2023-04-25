namespace EnergomonitoringXP
{
	partial class frmNominalsAndTimes
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNominalsAndTimes));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.dgvNominals = new System.Windows.Forms.DataGridView();
			this.myDataSet = new System.Data.DataSet();
			this.myTable = new System.Data.DataTable();
			this.myDataColumnParam = new System.Data.DataColumn();
			this.myDataColumnValue = new System.Data.DataColumn();
			this.btnSaveToDevice = new System.Windows.Forms.Button();
			this.btnLoadFromDevice = new System.Windows.Forms.Button();
			this.btnSaveToFile = new System.Windows.Forms.Button();
			this.btnLoadFromFile = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.lblObjectNames = new System.Windows.Forms.Label();
			this.param = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dgvNominals)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.myDataSet)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.myTable)).BeginInit();
			this.SuspendLayout();
			// 
			// dgvNominals
			// 
			resources.ApplyResources(this.dgvNominals, "dgvNominals");
			this.dgvNominals.AllowUserToAddRows = false;
			this.dgvNominals.AllowUserToDeleteRows = false;
			this.dgvNominals.AllowUserToResizeColumns = false;
			this.dgvNominals.AllowUserToResizeRows = false;
			this.dgvNominals.AutoGenerateColumns = false;
			this.dgvNominals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvNominals.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.param,
            this.value});
			this.dgvNominals.DataMember = "table_nominal_and_times";
			this.dgvNominals.DataSource = this.myDataSet;
			this.dgvNominals.MultiSelect = false;
			this.dgvNominals.Name = "dgvNominals";
			this.dgvNominals.RowHeadersVisible = false;
			this.dgvNominals.RowTemplate.Height = 24;
			this.dgvNominals.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dgvNominals.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.myDataGridView_CellEndEdit);
			// 
			// myDataSet
			// 
			this.myDataSet.DataSetName = "ds_obj_names";
			this.myDataSet.Tables.AddRange(new System.Data.DataTable[] {
            this.myTable});
			// 
			// myTable
			// 
			this.myTable.Columns.AddRange(new System.Data.DataColumn[] {
            this.myDataColumnParam,
            this.myDataColumnValue});
			this.myTable.TableName = "table_nominal_and_times";
			// 
			// myDataColumnParam
			// 
			this.myDataColumnParam.ColumnName = "param";
			// 
			// myDataColumnValue
			// 
			this.myDataColumnValue.ColumnName = "value";
			// 
			// btnSaveToDevice
			// 
			resources.ApplyResources(this.btnSaveToDevice, "btnSaveToDevice");
			this.btnSaveToDevice.Name = "btnSaveToDevice";
			this.btnSaveToDevice.Click += new System.EventHandler(this.btnSaveToDevice_Click);
			// 
			// btnLoadFromDevice
			// 
			resources.ApplyResources(this.btnLoadFromDevice, "btnLoadFromDevice");
			this.btnLoadFromDevice.Name = "btnLoadFromDevice";
			this.btnLoadFromDevice.Click += new System.EventHandler(this.btnLoadFromDevice_Click);
			// 
			// btnSaveToFile
			// 
			resources.ApplyResources(this.btnSaveToFile, "btnSaveToFile");
			this.btnSaveToFile.Name = "btnSaveToFile";
			this.btnSaveToFile.Click += new System.EventHandler(this.btnSaveToFile_Click);
			// 
			// btnLoadFromFile
			// 
			resources.ApplyResources(this.btnLoadFromFile, "btnLoadFromFile");
			this.btnLoadFromFile.Name = "btnLoadFromFile";
			this.btnLoadFromFile.Click += new System.EventHandler(this.btnLoadFromFile_Click);
			// 
			// btnClose
			// 
			resources.ApplyResources(this.btnClose, "btnClose");
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Name = "btnClose";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// lblObjectNames
			// 
			resources.ApplyResources(this.lblObjectNames, "lblObjectNames");
			this.lblObjectNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(111)))));
			this.lblObjectNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblObjectNames.Name = "lblObjectNames";
			// 
			// param
			// 
			this.param.DataPropertyName = "param";
			resources.ApplyResources(this.param, "param");
			this.param.Name = "param";
			this.param.ReadOnly = true;
			this.param.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// value
			// 
			this.value.DataPropertyName = "value";
			dataGridViewCellStyle1.NullValue = null;
			this.value.DefaultCellStyle = dataGridViewCellStyle1;
			resources.ApplyResources(this.value, "value");
			this.value.Name = "value";
			this.value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// frmNominalsAndTimes
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.btnSaveToDevice);
			this.Controls.Add(this.btnLoadFromDevice);
			this.Controls.Add(this.btnSaveToFile);
			this.Controls.Add(this.btnLoadFromFile);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.lblObjectNames);
			this.Controls.Add(this.dgvNominals);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmNominalsAndTimes";
			this.Load += new System.EventHandler(this.frmNominalsAndTimes_Load);
			((System.ComponentModel.ISupportInitialize)(this.dgvNominals)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.myDataSet)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.myTable)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dgvNominals;
		private System.Data.DataSet myDataSet;
		private System.Data.DataTable myTable;
		private System.Data.DataColumn myDataColumnParam;
		private System.Windows.Forms.Button btnSaveToDevice;
		private System.Windows.Forms.Button btnLoadFromDevice;
		private System.Windows.Forms.Button btnSaveToFile;
		private System.Windows.Forms.Button btnLoadFromFile;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Label lblObjectNames;
		private System.Data.DataColumn myDataColumnValue;
		private System.Windows.Forms.DataGridViewTextBoxColumn param;
		private System.Windows.Forms.DataGridViewTextBoxColumn value;
		//private System.Windows.Forms.DataGridViewTextBoxColumn paramDataGridViewTextBoxColumn;
		//private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		//private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		//private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;


	}
}