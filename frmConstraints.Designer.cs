namespace EnergomonitoringXP
{
	/// <summary>
	/// Класс описывает форму окна просмотра/изменения уставок
	/// </summary>
	partial class frmConstraints
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConstraints));
			this.dsConstraints = new System.Data.DataSet();
			this.btnSaveToDevice = new System.Windows.Forms.Button();
			this.btnLoadFromDevice = new System.Windows.Forms.Button();
			this.btnSaveToFile = new System.Windows.Forms.Button();
			this.btnLoadFromFile = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.btnGenerateCode = new System.Windows.Forms.Button();
			this.lblEmpty2 = new System.Windows.Forms.Label();
			this.lblConnectScheme2 = new System.Windows.Forms.Label();
			this.lblConnectScheme = new System.Windows.Forms.Label();
			this.lblEmpty = new System.Windows.Forms.Label();
			this.lblObjectNames = new System.Windows.Forms.Label();
			this.cmbConstraintType = new System.Windows.Forms.ComboBox();
			this.lblSelect = new System.Windows.Forms.Label();
			this.dgvConstraints = new System.Windows.Forms.DataGridView();
			this.btnFLASH = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dsConstraints)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvConstraints)).BeginInit();
			this.SuspendLayout();
			// 
			// dsConstraints
			// 
			this.dsConstraints.DataSetName = "NewDataSet";
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
			// 
			// btnGenerateCode
			// 
			resources.ApplyResources(this.btnGenerateCode, "btnGenerateCode");
			this.btnGenerateCode.Name = "btnGenerateCode";
			this.btnGenerateCode.Click += new System.EventHandler(this.btnGenerateCode_Click);
			// 
			// lblEmpty2
			// 
			resources.ApplyResources(this.lblEmpty2, "lblEmpty2");
			this.lblEmpty2.BackColor = System.Drawing.Color.Transparent;
			this.lblEmpty2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblEmpty2.Name = "lblEmpty2";
			// 
			// lblConnectScheme2
			// 
			resources.ApplyResources(this.lblConnectScheme2, "lblConnectScheme2");
			this.lblConnectScheme2.BackColor = System.Drawing.Color.Transparent;
			this.lblConnectScheme2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblConnectScheme2.Name = "lblConnectScheme2";
			// 
			// lblConnectScheme
			// 
			resources.ApplyResources(this.lblConnectScheme, "lblConnectScheme");
			this.lblConnectScheme.BackColor = System.Drawing.Color.Transparent;
			this.lblConnectScheme.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblConnectScheme.Name = "lblConnectScheme";
			// 
			// lblEmpty
			// 
			resources.ApplyResources(this.lblEmpty, "lblEmpty");
			this.lblEmpty.BackColor = System.Drawing.Color.Transparent;
			this.lblEmpty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblEmpty.Name = "lblEmpty";
			// 
			// lblObjectNames
			// 
			resources.ApplyResources(this.lblObjectNames, "lblObjectNames");
			this.lblObjectNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(111)))));
			this.lblObjectNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblObjectNames.Name = "lblObjectNames";
			// 
			// cmbConstraintType
			// 
			resources.ApplyResources(this.cmbConstraintType, "cmbConstraintType");
			this.cmbConstraintType.DataSource = this.dsConstraints;
			this.cmbConstraintType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbConstraintType.FormattingEnabled = true;
			this.cmbConstraintType.Name = "cmbConstraintType";
			this.cmbConstraintType.SelectedIndexChanged += new System.EventHandler(this.cmbConstraintType_SelectedIndexChanged);
			// 
			// lblSelect
			// 
			resources.ApplyResources(this.lblSelect, "lblSelect");
			this.lblSelect.Name = "lblSelect";
			// 
			// dgvConstraints
			// 
			resources.ApplyResources(this.dgvConstraints, "dgvConstraints");
			this.dgvConstraints.AllowUserToAddRows = false;
			this.dgvConstraints.AllowUserToDeleteRows = false;
			this.dgvConstraints.AllowUserToResizeColumns = false;
			this.dgvConstraints.AllowUserToResizeRows = false;
			this.dgvConstraints.AutoGenerateColumns = false;
			this.dgvConstraints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvConstraints.DataSource = this.dsConstraints;
			this.dgvConstraints.Name = "dgvConstraints";
			this.dgvConstraints.RowHeadersVisible = false;
			this.dgvConstraints.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dgvConstraints.RowTemplate.DefaultCellStyle.NullValue = null;
			this.dgvConstraints.RowTemplate.Height = 24;
			this.dgvConstraints.ShowCellErrors = false;
			this.dgvConstraints.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvConstraints_CellEndEdit);
			this.dgvConstraints.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvConstraints_CellValidating);
			this.dgvConstraints.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvConstraints_DataBindingComplete);
			this.dgvConstraints.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvConstraints_DataError);
			this.dgvConstraints.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvConstraints_EditingControlShowing);
			// 
			// btnFLASH
			// 
			resources.ApplyResources(this.btnFLASH, "btnFLASH");
			this.btnFLASH.Name = "btnFLASH";
			this.btnFLASH.Click += new System.EventHandler(this.btnFLASH_Click);
			// 
			// frmConstraints
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.btnFLASH);
			this.Controls.Add(this.cmbConstraintType);
			this.Controls.Add(this.dgvConstraints);
			this.Controls.Add(this.lblSelect);
			this.Controls.Add(this.lblEmpty2);
			this.Controls.Add(this.lblConnectScheme2);
			this.Controls.Add(this.lblConnectScheme);
			this.Controls.Add(this.lblEmpty);
			this.Controls.Add(this.lblObjectNames);
			this.Controls.Add(this.btnSaveToDevice);
			this.Controls.Add(this.btnLoadFromDevice);
			this.Controls.Add(this.btnSaveToFile);
			this.Controls.Add(this.btnLoadFromFile);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnGenerateCode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmConstraints";
			this.Load += new System.EventHandler(this.frmСonstraints_Load);
			((System.ComponentModel.ISupportInitialize)(this.dsConstraints)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvConstraints)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Data.DataSet dsConstraints;
		private System.Windows.Forms.Button btnSaveToDevice;
		private System.Windows.Forms.Button btnLoadFromDevice;
		private System.Windows.Forms.Button btnSaveToFile;
		private System.Windows.Forms.Button btnLoadFromFile;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Button btnGenerateCode;
		private System.Windows.Forms.Label lblEmpty2;
		private System.Windows.Forms.Label lblConnectScheme2;
		private System.Windows.Forms.Label lblConnectScheme;
		private System.Windows.Forms.Label lblEmpty;
		private System.Windows.Forms.Label lblObjectNames;
		private System.Windows.Forms.ComboBox cmbConstraintType;
		private System.Windows.Forms.Label lblSelect;
		private System.Windows.Forms.DataGridView dgvConstraints;
		private System.Windows.Forms.Button btnFLASH;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbParamName;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbNPL_3ph4w;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbUPL_3ph4w;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbNPL_3ph3w;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbUPL_3ph3w;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbContraintTypeId;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbVal_95;
		private System.Windows.Forms.DataGridViewTextBoxColumn dctbVal_100;
	}
}