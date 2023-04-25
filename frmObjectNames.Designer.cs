namespace EnergomonitoringXP
{
	partial class frmObjectNames
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmObjectNames));
			this.myDataSet = new System.Data.DataSet();
			this.tableNames = new System.Data.DataTable();
			this.myColumnID = new System.Data.DataColumn();
			this.myColumnName = new System.Data.DataColumn();
			this.lblObjectNames = new System.Windows.Forms.Label();
			this.btnClearAll = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.btnLoadDefaults = new System.Windows.Forms.Button();
			this.btnLoadFromFile = new System.Windows.Forms.Button();
			this.btnSaveToFile = new System.Windows.Forms.Button();
			this.btnSaveToDevice = new System.Windows.Forms.Button();
			this.btnLoadFromDevice = new System.Windows.Forms.Button();
			this.myDataGridView = new System.Windows.Forms.DataGridView();
			this.obj_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.obj_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.myDataSet)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tableNames)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.myDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// myDataSet
			// 
			this.myDataSet.DataSetName = "ds_obj_names";
			this.myDataSet.Tables.AddRange(new System.Data.DataTable[] {
            this.tableNames});
			// 
			// tableNames
			// 
			this.tableNames.Columns.AddRange(new System.Data.DataColumn[] {
            this.myColumnID,
            this.myColumnName});
			this.tableNames.TableName = "table_obj_names";
			// 
			// myColumnID
			// 
			this.myColumnID.Caption = "#";
			this.myColumnID.ColumnName = "obj_id";
			this.myColumnID.DataType = typeof(int);
			// 
			// myColumnName
			// 
			this.myColumnName.Caption = "Name";
			this.myColumnName.ColumnName = "obj_name";
			// 
			// lblObjectNames
			// 
			resources.ApplyResources(this.lblObjectNames, "lblObjectNames");
			this.lblObjectNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(111)))));
			this.lblObjectNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblObjectNames.Name = "lblObjectNames";
			// 
			// btnClearAll
			// 
			resources.ApplyResources(this.btnClearAll, "btnClearAll");
			this.btnClearAll.Name = "btnClearAll";
			this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
			// 
			// btnClose
			// 
			resources.ApplyResources(this.btnClose, "btnClose");
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Name = "btnClose";
			// 
			// btnLoadDefaults
			// 
			resources.ApplyResources(this.btnLoadDefaults, "btnLoadDefaults");
			this.btnLoadDefaults.Name = "btnLoadDefaults";
			this.btnLoadDefaults.Click += new System.EventHandler(this.btnLoadDefaults_Click);
			// 
			// btnLoadFromFile
			// 
			resources.ApplyResources(this.btnLoadFromFile, "btnLoadFromFile");
			this.btnLoadFromFile.Name = "btnLoadFromFile";
			this.btnLoadFromFile.Click += new System.EventHandler(this.btnLoadFromFile_Click);
			// 
			// btnSaveToFile
			// 
			resources.ApplyResources(this.btnSaveToFile, "btnSaveToFile");
			this.btnSaveToFile.Name = "btnSaveToFile";
			this.btnSaveToFile.Click += new System.EventHandler(this.btnSaveToFile_Click);
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
			// myDataGridView
			// 
			resources.ApplyResources(this.myDataGridView, "myDataGridView");
			this.myDataGridView.AllowUserToAddRows = false;
			this.myDataGridView.AllowUserToDeleteRows = false;
			this.myDataGridView.AllowUserToResizeColumns = false;
			this.myDataGridView.AllowUserToResizeRows = false;
			this.myDataGridView.AutoGenerateColumns = false;
			this.myDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.myDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.obj_id,
            this.obj_name});
			this.myDataGridView.DataMember = "table_obj_names";
			this.myDataGridView.DataSource = this.myDataSet;
			this.myDataGridView.MultiSelect = false;
			this.myDataGridView.Name = "myDataGridView";
			this.myDataGridView.RowHeadersVisible = false;
			this.myDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			// 
			// obj_id
			// 
			this.obj_id.DataPropertyName = "obj_id";
			resources.ApplyResources(this.obj_id, "obj_id");
			this.obj_id.Name = "obj_id";
			this.obj_id.ReadOnly = true;
			this.obj_id.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// obj_name
			// 
			this.obj_name.DataPropertyName = "obj_name";
			resources.ApplyResources(this.obj_name, "obj_name");
			this.obj_name.MaxInputLength = 14;
			this.obj_name.Name = "obj_name";
			this.obj_name.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// frmObjectNames
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.myDataGridView);
			this.Controls.Add(this.btnSaveToDevice);
			this.Controls.Add(this.btnLoadFromDevice);
			this.Controls.Add(this.btnSaveToFile);
			this.Controls.Add(this.btnLoadFromFile);
			this.Controls.Add(this.btnLoadDefaults);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnClearAll);
			this.Controls.Add(this.lblObjectNames);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmObjectNames";
			this.Load += new System.EventHandler(this.frmObjectNames_Load);
			((System.ComponentModel.ISupportInitialize)(this.myDataSet)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tableNames)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.myDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Data.DataSet myDataSet;
		private System.Data.DataTable tableNames;
		private System.Data.DataColumn myColumnID;
		private System.Data.DataColumn myColumnName;
		private System.Windows.Forms.Label lblObjectNames;
		private System.Windows.Forms.Button btnClearAll;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Button btnLoadDefaults;
		private System.Windows.Forms.Button btnLoadFromFile;
		private System.Windows.Forms.Button btnSaveToFile;
		private System.Windows.Forms.Button btnSaveToDevice;
		private System.Windows.Forms.Button btnLoadFromDevice;
		private System.Windows.Forms.DataGridView myDataGridView;
		private System.Windows.Forms.DataGridViewTextBoxColumn obj_id;
		private System.Windows.Forms.DataGridViewTextBoxColumn obj_name;

	}
}