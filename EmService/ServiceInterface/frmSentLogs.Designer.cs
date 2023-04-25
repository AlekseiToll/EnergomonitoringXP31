namespace EmServiceLib.SavingInterface
{
	partial class frmSentLogs
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSentLogs));
			this.btnSent = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.panelMain = new System.Windows.Forms.Panel();
			this.tbComment = new System.Windows.Forms.TextBox();
			this.labelComment = new System.Windows.Forms.Label();
			this.tbEmail = new System.Windows.Forms.TextBox();
			this.labelEmail = new System.Windows.Forms.Label();
			this.tbSender = new System.Windows.Forms.TextBox();
			this.labelSender = new System.Windows.Forms.Label();
			this.panelText = new System.Windows.Forms.Panel();
			this.labelMain = new System.Windows.Forms.Label();
			this.panelMain.SuspendLayout();
			this.panelText.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnSent
			// 
			resources.ApplyResources(this.btnSent, "btnSent");
			this.btnSent.Name = "btnSent";
			this.btnSent.UseVisualStyleBackColor = true;
			this.btnSent.Click += new System.EventHandler(this.btnSent_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// panelMain
			// 
			resources.ApplyResources(this.panelMain, "panelMain");
			this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelMain.Controls.Add(this.tbComment);
			this.panelMain.Controls.Add(this.labelComment);
			this.panelMain.Controls.Add(this.tbEmail);
			this.panelMain.Controls.Add(this.labelEmail);
			this.panelMain.Controls.Add(this.tbSender);
			this.panelMain.Controls.Add(this.labelSender);
			this.panelMain.Controls.Add(this.panelText);
			this.panelMain.Name = "panelMain";
			// 
			// tbComment
			// 
			resources.ApplyResources(this.tbComment, "tbComment");
			this.tbComment.Name = "tbComment";
			// 
			// labelComment
			// 
			resources.ApplyResources(this.labelComment, "labelComment");
			this.labelComment.Name = "labelComment";
			// 
			// tbEmail
			// 
			resources.ApplyResources(this.tbEmail, "tbEmail");
			this.tbEmail.Name = "tbEmail";
			// 
			// labelEmail
			// 
			resources.ApplyResources(this.labelEmail, "labelEmail");
			this.labelEmail.Name = "labelEmail";
			// 
			// tbSender
			// 
			resources.ApplyResources(this.tbSender, "tbSender");
			this.tbSender.Name = "tbSender";
			// 
			// labelSender
			// 
			resources.ApplyResources(this.labelSender, "labelSender");
			this.labelSender.Name = "labelSender";
			// 
			// panelText
			// 
			resources.ApplyResources(this.panelText, "panelText");
			this.panelText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelText.Controls.Add(this.labelMain);
			this.panelText.Name = "panelText";
			// 
			// labelMain
			// 
			resources.ApplyResources(this.labelMain, "labelMain");
			this.labelMain.Name = "labelMain";
			// 
			// frmSentLogs
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelMain);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSent);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSentLogs";
			this.panelMain.ResumeLayout(false);
			this.panelMain.PerformLayout();
			this.panelText.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnSent;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.TextBox tbComment;
		private System.Windows.Forms.Label labelComment;
		private System.Windows.Forms.TextBox tbEmail;
		private System.Windows.Forms.Label labelEmail;
		private System.Windows.Forms.TextBox tbSender;
		private System.Windows.Forms.Label labelSender;
		private System.Windows.Forms.Panel panelText;
		private System.Windows.Forms.Label labelMain;
	}
}