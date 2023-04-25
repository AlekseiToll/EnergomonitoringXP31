using System.Windows.Forms;

namespace EmDataSaver.SavingInterface
{
	partial class frmExport: System.Windows.Forms.Form
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExport));
			this.chklbParts = new System.Windows.Forms.CheckedListBox();
			this.lblParts = new System.Windows.Forms.Label();
			this.txtImagePath = new System.Windows.Forms.TextBox();
			this.lblImageFilename = new System.Windows.Forms.Label();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// chklbParts
			// 
			resources.ApplyResources(this.chklbParts, "chklbParts");
			this.chklbParts.FormattingEnabled = true;
			this.chklbParts.Name = "chklbParts";
			this.chklbParts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklbParts_ItemCheck);
			// 
			// lblParts
			// 
			resources.ApplyResources(this.lblParts, "lblParts");
			this.lblParts.Name = "lblParts";
			// 
			// txtImagePath
			// 
			resources.ApplyResources(this.txtImagePath, "txtImagePath");
			this.txtImagePath.Name = "txtImagePath";
			// 
			// lblImageFilename
			// 
			resources.ApplyResources(this.lblImageFilename, "lblImageFilename");
			this.lblImageFilename.Name = "lblImageFilename";
			// 
			// btnBrowse
			// 
			resources.ApplyResources(this.btnBrowse, "btnBrowse");
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			// 
			// frmExport
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.txtImagePath);
			this.Controls.Add(this.chklbParts);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.lblImageFilename);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.lblParts);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "frmExport";
			this.Load += new System.EventHandler(this.frmToolboxExportDetails_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckedListBox chklbParts;
		private System.Windows.Forms.Label lblParts;
		private System.Windows.Forms.TextBox txtImagePath;
		private System.Windows.Forms.Label lblImageFilename;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;

	}
}