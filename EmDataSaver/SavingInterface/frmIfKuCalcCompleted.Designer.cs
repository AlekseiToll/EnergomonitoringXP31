namespace EmDataSaver.SavingInterface
{
	partial class frmIfKuCalcCompleted
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmIfKuCalcCompleted));
			this.btnNotwait = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.labelMain = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnNotwait
			// 
			resources.ApplyResources(this.btnNotwait, "btnNotwait");
			this.btnNotwait.Name = "btnNotwait";
			this.btnNotwait.UseVisualStyleBackColor = true;
			this.btnNotwait.Click += new System.EventHandler(this.btnNotwait_Click);
			// 
			// progressBar
			// 
			resources.ApplyResources(this.progressBar, "progressBar");
			this.progressBar.Maximum = 242;
			this.progressBar.Name = "progressBar";
			this.progressBar.Step = 1;
			// 
			// labelMain
			// 
			resources.ApplyResources(this.labelMain, "labelMain");
			this.labelMain.Name = "labelMain";
			// 
			// frmIfKuCalcCompleted
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.labelMain);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.btnNotwait);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmIfKuCalcCompleted";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnNotwait;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label labelMain;
	}
}