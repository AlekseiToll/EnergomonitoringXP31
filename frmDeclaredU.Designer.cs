namespace EnergomonitoringXP
{
	partial class frmDeclaredU
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDeclaredU));
			this.panelButtons = new System.Windows.Forms.Panel();
			this.btnClose = new System.Windows.Forms.Button();
			this.btnWrite = new System.Windows.Forms.Button();
			this.btnRead = new System.Windows.Forms.Button();
			this.labelCurValue = new System.Windows.Forms.Label();
			this.tbCurValue = new System.Windows.Forms.TextBox();
			this.tbMin = new System.Windows.Forms.TextBox();
			this.labelMin = new System.Windows.Forms.Label();
			this.tbMax = new System.Windows.Forms.TextBox();
			this.labelMax = new System.Windows.Forms.Label();
			this.tbNewValue = new System.Windows.Forms.TextBox();
			this.labelNewValue = new System.Windows.Forms.Label();
			this.labelInfo = new System.Windows.Forms.Label();
			this.labelCurValUnit = new System.Windows.Forms.Label();
			this.labelMinUnit = new System.Windows.Forms.Label();
			this.labelMaxUnit = new System.Windows.Forms.Label();
			this.labelNewUnit = new System.Windows.Forms.Label();
			this.panelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelButtons
			// 
			resources.ApplyResources(this.panelButtons, "panelButtons");
			this.panelButtons.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelButtons.Controls.Add(this.btnClose);
			this.panelButtons.Controls.Add(this.btnWrite);
			this.panelButtons.Controls.Add(this.btnRead);
			this.panelButtons.Name = "panelButtons";
			// 
			// btnClose
			// 
			resources.ApplyResources(this.btnClose, "btnClose");
			this.btnClose.Name = "btnClose";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// btnWrite
			// 
			resources.ApplyResources(this.btnWrite, "btnWrite");
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.UseVisualStyleBackColor = true;
			this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
			// 
			// btnRead
			// 
			resources.ApplyResources(this.btnRead, "btnRead");
			this.btnRead.Name = "btnRead";
			this.btnRead.UseVisualStyleBackColor = true;
			this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
			// 
			// labelCurValue
			// 
			resources.ApplyResources(this.labelCurValue, "labelCurValue");
			this.labelCurValue.Name = "labelCurValue";
			// 
			// tbCurValue
			// 
			resources.ApplyResources(this.tbCurValue, "tbCurValue");
			this.tbCurValue.Name = "tbCurValue";
			this.tbCurValue.ReadOnly = true;
			// 
			// tbMin
			// 
			resources.ApplyResources(this.tbMin, "tbMin");
			this.tbMin.Name = "tbMin";
			this.tbMin.ReadOnly = true;
			// 
			// labelMin
			// 
			resources.ApplyResources(this.labelMin, "labelMin");
			this.labelMin.Name = "labelMin";
			// 
			// tbMax
			// 
			resources.ApplyResources(this.tbMax, "tbMax");
			this.tbMax.Name = "tbMax";
			this.tbMax.ReadOnly = true;
			// 
			// labelMax
			// 
			resources.ApplyResources(this.labelMax, "labelMax");
			this.labelMax.Name = "labelMax";
			// 
			// tbNewValue
			// 
			resources.ApplyResources(this.tbNewValue, "tbNewValue");
			this.tbNewValue.Name = "tbNewValue";
			this.tbNewValue.TextChanged += new System.EventHandler(this.tbNewValue_TextChanged);
			this.tbNewValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbNewValue_KeyPress);
			// 
			// labelNewValue
			// 
			resources.ApplyResources(this.labelNewValue, "labelNewValue");
			this.labelNewValue.Name = "labelNewValue";
			// 
			// labelInfo
			// 
			resources.ApplyResources(this.labelInfo, "labelInfo");
			this.labelInfo.Name = "labelInfo";
			// 
			// labelCurValUnit
			// 
			resources.ApplyResources(this.labelCurValUnit, "labelCurValUnit");
			this.labelCurValUnit.Name = "labelCurValUnit";
			// 
			// labelMinUnit
			// 
			resources.ApplyResources(this.labelMinUnit, "labelMinUnit");
			this.labelMinUnit.Name = "labelMinUnit";
			// 
			// labelMaxUnit
			// 
			resources.ApplyResources(this.labelMaxUnit, "labelMaxUnit");
			this.labelMaxUnit.Name = "labelMaxUnit";
			// 
			// labelNewUnit
			// 
			resources.ApplyResources(this.labelNewUnit, "labelNewUnit");
			this.labelNewUnit.Name = "labelNewUnit";
			// 
			// frmDeclaredU
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.labelNewUnit);
			this.Controls.Add(this.labelMaxUnit);
			this.Controls.Add(this.labelMinUnit);
			this.Controls.Add(this.labelCurValUnit);
			this.Controls.Add(this.labelInfo);
			this.Controls.Add(this.tbNewValue);
			this.Controls.Add(this.labelNewValue);
			this.Controls.Add(this.tbMax);
			this.Controls.Add(this.labelMax);
			this.Controls.Add(this.tbMin);
			this.Controls.Add(this.labelMin);
			this.Controls.Add(this.tbCurValue);
			this.Controls.Add(this.labelCurValue);
			this.Controls.Add(this.panelButtons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmDeclaredU";
			this.Load += new System.EventHandler(this.frmDeclaredU_Load);
			this.panelButtons.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelButtons;
		private System.Windows.Forms.Button btnRead;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.Label labelCurValue;
		private System.Windows.Forms.TextBox tbCurValue;
		private System.Windows.Forms.TextBox tbMin;
		private System.Windows.Forms.Label labelMin;
		private System.Windows.Forms.TextBox tbMax;
		private System.Windows.Forms.Label labelMax;
		private System.Windows.Forms.TextBox tbNewValue;
		private System.Windows.Forms.Label labelNewValue;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.Label labelCurValUnit;
		private System.Windows.Forms.Label labelMinUnit;
		private System.Windows.Forms.Label labelMaxUnit;
		private System.Windows.Forms.Label labelNewUnit;
	}
}