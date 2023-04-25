// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Folder popup-window form

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Resources;
using System.Data;

using DbServiceLib;
using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmOptionsFld.
	/// </summary>
	public class frmOptionsFld : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox gbFldOptions;
		private System.Windows.Forms.PictureBox pbFolder;
		private System.Windows.Forms.TextBox txtFldName;
		private System.Windows.Forms.Label lblFolderName;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.TextBox txtFldInfo;

		/// <summary>
		/// Info of the folder to be displayed
		/// </summary>
		public string FolderInfo;

		/// <summary>
		/// Constructor with two parameters: Folder name and Folder type index
		/// </summary>
		public frmOptionsFld(string strFolderName, string strFolderInfo, EmDeviceType devType)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			txtFldName.Text = strFolderName;

			FolderInfo = strFolderInfo;
			txtFldInfo.Text = strFolderInfo;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOptionsFld));
			this.gbFldOptions = new System.Windows.Forms.GroupBox();
			this.txtFldName = new System.Windows.Forms.TextBox();
			this.lblInfo = new System.Windows.Forms.Label();
			this.lblFolderName = new System.Windows.Forms.Label();
			this.txtFldInfo = new System.Windows.Forms.TextBox();
			this.pbFolder = new System.Windows.Forms.PictureBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.gbFldOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbFolder)).BeginInit();
			this.SuspendLayout();
			// 
			// gbFldOptions
			// 
			resources.ApplyResources(this.gbFldOptions, "gbFldOptions");
			this.gbFldOptions.Controls.Add(this.txtFldName);
			this.gbFldOptions.Controls.Add(this.lblInfo);
			this.gbFldOptions.Controls.Add(this.lblFolderName);
			this.gbFldOptions.Controls.Add(this.txtFldInfo);
			this.gbFldOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.gbFldOptions.Name = "gbFldOptions";
			this.gbFldOptions.TabStop = false;
			// 
			// txtFldName
			// 
			resources.ApplyResources(this.txtFldName, "txtFldName");
			this.txtFldName.Name = "txtFldName";
			this.txtFldName.ReadOnly = true;
			// 
			// lblInfo
			// 
			resources.ApplyResources(this.lblInfo, "lblInfo");
			this.lblInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblInfo.Name = "lblInfo";
			// 
			// lblFolderName
			// 
			resources.ApplyResources(this.lblFolderName, "lblFolderName");
			this.lblFolderName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFolderName.Name = "lblFolderName";
			// 
			// txtFldInfo
			// 
			resources.ApplyResources(this.txtFldInfo, "txtFldInfo");
			this.txtFldInfo.Name = "txtFldInfo";
			// 
			// pbFolder
			// 
			resources.ApplyResources(this.pbFolder, "pbFolder");
			this.pbFolder.BackColor = System.Drawing.Color.White;
			this.pbFolder.Name = "pbFolder";
			this.pbFolder.TabStop = false;
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// frmOptionsFld
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.gbFldOptions);
			this.Controls.Add(this.pbFolder);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmOptionsFld";
			this.VisibleChanged += new System.EventHandler(this.frmOptionsFld_VisibleChanged);
			this.gbFldOptions.ResumeLayout(false);
			this.gbFldOptions.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbFolder)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Before closing dialog window we registring changes in inner variables
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.EventArgs</param>
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			FolderInfo = txtFldInfo.Text;
		}

		/// <summary>
		/// Setting focus when open
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.EventArgs</param>
		private void frmOptionsFld_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible == true)
			{
				txtFldName.Focus();
			}
		}
	}
}
