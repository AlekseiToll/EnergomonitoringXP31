namespace EmXPinitializer
{
	partial class frmMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.lblDescription = new System.Windows.Forms.Label();
			this.txtPgPath = new System.Windows.Forms.TextBox();
			this.lblPgPath = new System.Windows.Forms.Label();
			this.btnStart = new System.Windows.Forms.Button();
			this.pictureBoxSearching = new System.Windows.Forms.PictureBox();
			this.btnStopAutoSearching = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.StatusStripInit = new System.Windows.Forms.StatusStrip();
			this.tsProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.ProgressBarInit = new System.Windows.Forms.ToolStripProgressBar();
			this.btnScript = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.gbSettings = new System.Windows.Forms.GroupBox();
			this.gbDatabases = new System.Windows.Forms.GroupBox();
			this.chbDbEtPQP_a = new System.Windows.Forms.CheckBox();
			this.chbDbEtPQP = new System.Windows.Forms.CheckBox();
			this.chbDbEm32 = new System.Windows.Forms.CheckBox();
			this.chbDbEm33T = new System.Windows.Forms.CheckBox();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.txtHost = new System.Windows.Forms.TextBox();
			this.lblPort = new System.Windows.Forms.Label();
			this.lblHost = new System.Windows.Forms.Label();
			this.txtPSWD = new System.Windows.Forms.TextBox();
			this.txtSU = new System.Windows.Forms.TextBox();
			this.lblPSWD = new System.Windows.Forms.Label();
			this.lblSU = new System.Windows.Forms.Label();
			this.btnConnect = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearching)).BeginInit();
			this.StatusStripInit.SuspendLayout();
			this.gbSettings.SuspendLayout();
			this.gbDatabases.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			resources.ApplyResources(this.lblDescription, "lblDescription");
			this.lblDescription.Name = "lblDescription";
			// 
			// txtPgPath
			// 
			resources.ApplyResources(this.txtPgPath, "txtPgPath");
			this.txtPgPath.Name = "txtPgPath";
			this.txtPgPath.ReadOnly = true;
			// 
			// lblPgPath
			// 
			resources.ApplyResources(this.lblPgPath, "lblPgPath");
			this.lblPgPath.Name = "lblPgPath";
			// 
			// btnStart
			// 
			resources.ApplyResources(this.btnStart, "btnStart");
			this.btnStart.Name = "btnStart";
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// pictureBoxSearching
			// 
			resources.ApplyResources(this.pictureBoxSearching, "pictureBoxSearching");
			this.pictureBoxSearching.Name = "pictureBoxSearching";
			this.pictureBoxSearching.TabStop = false;
			// 
			// btnStopAutoSearching
			// 
			resources.ApplyResources(this.btnStopAutoSearching, "btnStopAutoSearching");
			this.btnStopAutoSearching.Name = "btnStopAutoSearching";
			this.btnStopAutoSearching.UseVisualStyleBackColor = true;
			this.btnStopAutoSearching.Click += new System.EventHandler(this.btnStopAutoSearching_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "exe";
			this.openFileDialog.FileName = "psql.exe";
			resources.ApplyResources(this.openFileDialog, "openFileDialog");
			// 
			// StatusStripInit
			// 
			resources.ApplyResources(this.StatusStripInit, "StatusStripInit");
			this.StatusStripInit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsProgressBar});
			this.StatusStripInit.Name = "StatusStripInit";
			this.StatusStripInit.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.StatusStripInit.SizingGrip = false;
			// 
			// tsProgressBar
			// 
			resources.ApplyResources(this.tsProgressBar, "tsProgressBar");
			this.tsProgressBar.Name = "tsProgressBar";
			// 
			// ProgressBarInit
			// 
			resources.ApplyResources(this.ProgressBarInit, "ProgressBarInit");
			this.ProgressBarInit.Name = "ProgressBarInit";
			this.ProgressBarInit.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			// 
			// btnScript
			// 
			resources.ApplyResources(this.btnScript, "btnScript");
			this.btnScript.Name = "btnScript";
			this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.bat";
			this.saveFileDialog.FileName = "EmWorkNet4DbInit.bat";
			resources.ApplyResources(this.saveFileDialog, "saveFileDialog");
			this.saveFileDialog.RestoreDirectory = true;
			// 
			// gbSettings
			// 
			resources.ApplyResources(this.gbSettings, "gbSettings");
			this.gbSettings.Controls.Add(this.gbDatabases);
			this.gbSettings.Controls.Add(this.txtPort);
			this.gbSettings.Controls.Add(this.txtHost);
			this.gbSettings.Controls.Add(this.lblPort);
			this.gbSettings.Controls.Add(this.lblHost);
			this.gbSettings.Controls.Add(this.btnStopAutoSearching);
			this.gbSettings.Controls.Add(this.txtPSWD);
			this.gbSettings.Controls.Add(this.txtSU);
			this.gbSettings.Controls.Add(this.lblPSWD);
			this.gbSettings.Controls.Add(this.lblSU);
			this.gbSettings.Name = "gbSettings";
			this.gbSettings.TabStop = false;
			// 
			// gbDatabases
			// 
			resources.ApplyResources(this.gbDatabases, "gbDatabases");
			this.gbDatabases.Controls.Add(this.chbDbEtPQP_a);
			this.gbDatabases.Controls.Add(this.chbDbEtPQP);
			this.gbDatabases.Controls.Add(this.chbDbEm32);
			this.gbDatabases.Controls.Add(this.chbDbEm33T);
			this.gbDatabases.Name = "gbDatabases";
			this.gbDatabases.TabStop = false;
			// 
			// chbDbEtPQP_a
			// 
			resources.ApplyResources(this.chbDbEtPQP_a, "chbDbEtPQP_a");
			this.chbDbEtPQP_a.Checked = true;
			this.chbDbEtPQP_a.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbDbEtPQP_a.Name = "chbDbEtPQP_a";
			this.chbDbEtPQP_a.UseVisualStyleBackColor = true;
			this.chbDbEtPQP_a.CheckedChanged += new System.EventHandler(this.chbDb_CheckedChanged);
			// 
			// chbDbEtPQP
			// 
			resources.ApplyResources(this.chbDbEtPQP, "chbDbEtPQP");
			this.chbDbEtPQP.Checked = true;
			this.chbDbEtPQP.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbDbEtPQP.Name = "chbDbEtPQP";
			this.chbDbEtPQP.UseVisualStyleBackColor = true;
			this.chbDbEtPQP.CheckedChanged += new System.EventHandler(this.chbDb_CheckedChanged);
			// 
			// chbDbEm32
			// 
			resources.ApplyResources(this.chbDbEm32, "chbDbEm32");
			this.chbDbEm32.Checked = true;
			this.chbDbEm32.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbDbEm32.Name = "chbDbEm32";
			this.chbDbEm32.UseVisualStyleBackColor = true;
			this.chbDbEm32.CheckedChanged += new System.EventHandler(this.chbDb_CheckedChanged);
			// 
			// chbDbEm33T
			// 
			resources.ApplyResources(this.chbDbEm33T, "chbDbEm33T");
			this.chbDbEm33T.Checked = true;
			this.chbDbEm33T.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbDbEm33T.Name = "chbDbEm33T";
			this.chbDbEm33T.UseVisualStyleBackColor = true;
			this.chbDbEm33T.CheckedChanged += new System.EventHandler(this.chbDb_CheckedChanged);
			// 
			// txtPort
			// 
			resources.ApplyResources(this.txtPort, "txtPort");
			this.txtPort.Name = "txtPort";
			// 
			// txtHost
			// 
			resources.ApplyResources(this.txtHost, "txtHost");
			this.txtHost.Name = "txtHost";
			// 
			// lblPort
			// 
			resources.ApplyResources(this.lblPort, "lblPort");
			this.lblPort.Name = "lblPort";
			// 
			// lblHost
			// 
			resources.ApplyResources(this.lblHost, "lblHost");
			this.lblHost.Name = "lblHost";
			// 
			// txtPSWD
			// 
			resources.ApplyResources(this.txtPSWD, "txtPSWD");
			this.txtPSWD.Name = "txtPSWD";
			// 
			// txtSU
			// 
			resources.ApplyResources(this.txtSU, "txtSU");
			this.txtSU.Name = "txtSU";
			// 
			// lblPSWD
			// 
			resources.ApplyResources(this.lblPSWD, "lblPSWD");
			this.lblPSWD.Name = "lblPSWD";
			// 
			// lblSU
			// 
			resources.ApplyResources(this.lblSU, "lblSU");
			this.lblSU.Name = "lblSU";
			// 
			// btnConnect
			// 
			resources.ApplyResources(this.btnConnect, "btnConnect");
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// frmMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.gbSettings);
			this.Controls.Add(this.btnScript);
			this.Controls.Add(this.StatusStripInit);
			this.Controls.Add(this.pictureBoxSearching);
			this.Controls.Add(this.txtPgPath);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.lblPgPath);
			this.Controls.Add(this.lblDescription);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			this.Load += new System.EventHandler(this.frmMain_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearching)).EndInit();
			this.StatusStripInit.ResumeLayout(false);
			this.StatusStripInit.PerformLayout();
			this.gbSettings.ResumeLayout(false);
			this.gbSettings.PerformLayout();
			this.gbDatabases.ResumeLayout(false);
			this.gbDatabases.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.TextBox txtPgPath;
		private System.Windows.Forms.Label lblPgPath;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.PictureBox pictureBoxSearching;
		private System.Windows.Forms.Button btnStopAutoSearching;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.StatusStrip StatusStripInit;
		private System.Windows.Forms.ToolStripProgressBar ProgressBarInit;
        private System.Windows.Forms.Button btnScript;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.GroupBox gbSettings;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.TextBox txtHost;
		private System.Windows.Forms.Label lblPort;
		private System.Windows.Forms.Label lblHost;
		private System.Windows.Forms.TextBox txtPSWD;
		private System.Windows.Forms.TextBox txtSU;
		private System.Windows.Forms.Label lblPSWD;
		private System.Windows.Forms.Label lblSU;
		private System.Windows.Forms.GroupBox gbDatabases;
		private System.Windows.Forms.CheckBox chbDbEtPQP;
		private System.Windows.Forms.CheckBox chbDbEm32;
		private System.Windows.Forms.CheckBox chbDbEm33T;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.ToolStripProgressBar tsProgressBar;
		private System.Windows.Forms.CheckBox chbDbEtPQP_a;
	}
}

