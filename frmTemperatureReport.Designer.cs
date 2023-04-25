namespace EnergomonitoringXP
{
	partial class frmTemperatureReport
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTemperatureReport));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.gbMain = new System.Windows.Forms.GroupBox();
			this.labelCmax = new System.Windows.Forms.Label();
			this.labelCmin = new System.Windows.Forms.Label();
			this.tbTmax = new System.Windows.Forms.TextBox();
			this.tbTmin = new System.Windows.Forms.TextBox();
			this.labelTmax = new System.Windows.Forms.Label();
			this.labelTmin = new System.Windows.Forms.Label();
			this.btnIgnore = new System.Windows.Forms.Button();
			this.gbMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// gbMain
			// 
			resources.ApplyResources(this.gbMain, "gbMain");
			this.gbMain.Controls.Add(this.labelCmax);
			this.gbMain.Controls.Add(this.labelCmin);
			this.gbMain.Controls.Add(this.tbTmax);
			this.gbMain.Controls.Add(this.tbTmin);
			this.gbMain.Controls.Add(this.labelTmax);
			this.gbMain.Controls.Add(this.labelTmin);
			this.gbMain.Name = "gbMain";
			this.gbMain.TabStop = false;
			// 
			// labelCmax
			// 
			resources.ApplyResources(this.labelCmax, "labelCmax");
			this.labelCmax.Name = "labelCmax";
			// 
			// labelCmin
			// 
			resources.ApplyResources(this.labelCmin, "labelCmin");
			this.labelCmin.Name = "labelCmin";
			// 
			// tbTmax
			// 
			resources.ApplyResources(this.tbTmax, "tbTmax");
			this.tbTmax.Name = "tbTmax";
			this.tbTmax.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbT_KeyPress);
			// 
			// tbTmin
			// 
			resources.ApplyResources(this.tbTmin, "tbTmin");
			this.tbTmin.Name = "tbTmin";
			this.tbTmin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbT_KeyPress);
			// 
			// labelTmax
			// 
			resources.ApplyResources(this.labelTmax, "labelTmax");
			this.labelTmax.Name = "labelTmax";
			// 
			// labelTmin
			// 
			resources.ApplyResources(this.labelTmin, "labelTmin");
			this.labelTmin.Name = "labelTmin";
			// 
			// btnIgnore
			// 
			resources.ApplyResources(this.btnIgnore, "btnIgnore");
			this.btnIgnore.DialogResult = System.Windows.Forms.DialogResult.Ignore;
			this.btnIgnore.Name = "btnIgnore";
			this.btnIgnore.UseVisualStyleBackColor = true;
			this.btnIgnore.Click += new System.EventHandler(this.btnIgnore_Click);
			// 
			// frmTemperatureReport
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.btnIgnore);
			this.Controls.Add(this.gbMain);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "frmTemperatureReport";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmTemperatureReport_FormClosing);
			this.gbMain.ResumeLayout(false);
			this.gbMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.GroupBox gbMain;
		private System.Windows.Forms.Label labelCmax;
		private System.Windows.Forms.Label labelCmin;
		private System.Windows.Forms.TextBox tbTmax;
		private System.Windows.Forms.TextBox tbTmin;
		private System.Windows.Forms.Label labelTmax;
		private System.Windows.Forms.Label labelTmin;
		private System.Windows.Forms.Button btnIgnore;
	}
}