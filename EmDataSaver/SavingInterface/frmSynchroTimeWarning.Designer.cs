namespace EmDataSaver.SavingInterface
{
	partial class frmSynchroTimeWarning
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSynchroTimeWarning));
			this.btnOk = new System.Windows.Forms.Button();
			this.chbShowMessage = new System.Windows.Forms.CheckBox();
			this.panelMain = new System.Windows.Forms.Panel();
			this.labelText = new System.Windows.Forms.Label();
			this.panelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			// 
			// chbShowMessage
			// 
			resources.ApplyResources(this.chbShowMessage, "chbShowMessage");
			this.chbShowMessage.Name = "chbShowMessage";
			this.chbShowMessage.UseVisualStyleBackColor = true;
			// 
			// panelMain
			// 
			this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelMain.Controls.Add(this.labelText);
			resources.ApplyResources(this.panelMain, "panelMain");
			this.panelMain.Name = "panelMain";
			// 
			// labelText
			// 
			resources.ApplyResources(this.labelText, "labelText");
			this.labelText.Name = "labelText";
			// 
			// frmSynchroTimeWarning
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelMain);
			this.Controls.Add(this.chbShowMessage);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSynchroTimeWarning";
			this.panelMain.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.CheckBox chbShowMessage;
		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Label labelText;
	}
}