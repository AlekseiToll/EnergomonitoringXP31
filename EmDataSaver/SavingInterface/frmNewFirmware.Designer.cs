namespace EmDataSaver.SavingInterface
{
	partial class frmNewFirmware
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNewFirmware));
			this.btnClose = new System.Windows.Forms.Button();
			this.chbShowMessage = new System.Windows.Forms.CheckBox();
			this.panelMain = new System.Windows.Forms.Panel();
			this.linkFtp = new System.Windows.Forms.LinkLabel();
			this.labelText = new System.Windows.Forms.Label();
			this.panelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			resources.ApplyResources(this.btnClose, "btnClose");
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnClose.Name = "btnClose";
			this.btnClose.UseVisualStyleBackColor = true;
			// 
			// chbShowMessage
			// 
			resources.ApplyResources(this.chbShowMessage, "chbShowMessage");
			this.chbShowMessage.Name = "chbShowMessage";
			this.chbShowMessage.UseVisualStyleBackColor = true;
			// 
			// panelMain
			// 
			resources.ApplyResources(this.panelMain, "panelMain");
			this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelMain.Controls.Add(this.linkFtp);
			this.panelMain.Controls.Add(this.labelText);
			this.panelMain.Name = "panelMain";
			// 
			// linkFtp
			// 
			resources.ApplyResources(this.linkFtp, "linkFtp");
			this.linkFtp.Name = "linkFtp";
			this.linkFtp.TabStop = true;
			// 
			// labelText
			// 
			resources.ApplyResources(this.labelText, "labelText");
			this.labelText.Name = "labelText";
			// 
			// frmNewFirmware
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelMain);
			this.Controls.Add(this.chbShowMessage);
			this.Controls.Add(this.btnClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmNewFirmware";
			this.Load += new System.EventHandler(this.frmNewFirmware_Load);
			this.panelMain.ResumeLayout(false);
			this.panelMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.CheckBox chbShowMessage;
		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.LinkLabel linkFtp;
		private System.Windows.Forms.Label labelText;
	}
}