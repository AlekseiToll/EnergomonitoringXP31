namespace ExportToExcel
{
	partial class frmDocPQPReportSaveDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocPQPReportSaveDialog));
			this.txtFileName = new System.Windows.Forms.TextBox();
			this.lblFileName = new System.Windows.Forms.Label();
			this.lblReportNumber = new System.Windows.Forms.Label();
			this.txtReportNumber = new System.Windows.Forms.TextBox();
			this.lblAppendixNumber = new System.Windows.Forms.Label();
			this.txtAppendixNumber = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.chkOpenAfterSaving = new System.Windows.Forms.CheckBox();
			this.fd = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// txtFileName
			// 
			resources.ApplyResources(this.txtFileName, "txtFileName");
			this.txtFileName.Name = "txtFileName";
			this.txtFileName.TextChanged += new System.EventHandler(this.txtFileName_TextChanged);
			// 
			// lblFileName
			// 
			resources.ApplyResources(this.lblFileName, "lblFileName");
			this.lblFileName.Name = "lblFileName";
			// 
			// lblReportNumber
			// 
			resources.ApplyResources(this.lblReportNumber, "lblReportNumber");
			this.lblReportNumber.Name = "lblReportNumber";
			// 
			// txtReportNumber
			// 
			resources.ApplyResources(this.txtReportNumber, "txtReportNumber");
			this.txtReportNumber.Name = "txtReportNumber";
			// 
			// lblAppendixNumber
			// 
			resources.ApplyResources(this.lblAppendixNumber, "lblAppendixNumber");
			this.lblAppendixNumber.Name = "lblAppendixNumber";
			// 
			// txtAppendixNumber
			// 
			resources.ApplyResources(this.txtAppendixNumber, "txtAppendixNumber");
			this.txtAppendixNumber.Name = "txtAppendixNumber";
			// 
			// btnBrowse
			// 
			resources.ApplyResources(this.btnBrowse, "btnBrowse");
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnSave
			// 
			resources.ApplyResources(this.btnSave, "btnSave");
			this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSave.Name = "btnSave";
			this.btnSave.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// chkOpenAfterSaving
			// 
			resources.ApplyResources(this.chkOpenAfterSaving, "chkOpenAfterSaving");
			this.chkOpenAfterSaving.Checked = true;
			this.chkOpenAfterSaving.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkOpenAfterSaving.Name = "chkOpenAfterSaving";
			this.chkOpenAfterSaving.UseVisualStyleBackColor = true;
			// 
			// fd
			// 
			this.fd.DefaultExt = "xls";
			this.fd.FileName = "Îò÷åò ÏÊÝ.xls";
			resources.ApplyResources(this.fd, "fd");
			this.fd.RestoreDirectory = true;
			this.fd.SupportMultiDottedExtensions = true;
			// 
			// frmDocPQPReportSaveDialog
			// 
			this.AcceptButton = this.btnSave;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.txtFileName);
			this.Controls.Add(this.txtReportNumber);
			this.Controls.Add(this.txtAppendixNumber);
			this.Controls.Add(this.chkOpenAfterSaving);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.lblAppendixNumber);
			this.Controls.Add(this.lblReportNumber);
			this.Controls.Add(this.lblFileName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmDocPQPReportSaveDialog";
			this.Load += new System.EventHandler(this.frmDocPQPReportSaveDialog_Load);
			this.Shown += new System.EventHandler(this.frmDocPQPReportSaveDialog_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblFileName;
		private System.Windows.Forms.Label lblReportNumber;
		private System.Windows.Forms.Label lblAppendixNumber;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnCancel;
		public System.Windows.Forms.TextBox txtFileName;
		public System.Windows.Forms.TextBox txtReportNumber;
		public System.Windows.Forms.TextBox txtAppendixNumber;
		public System.Windows.Forms.CheckBox chkOpenAfterSaving;
		private System.Windows.Forms.SaveFileDialog fd;
	}
}