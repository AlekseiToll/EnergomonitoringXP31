namespace EnergomonitoringXP
{
	partial class frmPeakLoadTime
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPeakLoadTime));
			this.panelMain = new System.Windows.Forms.Panel();
			this.labelHyphen2 = new System.Windows.Forms.Label();
			this.labelHyphen1 = new System.Windows.Forms.Label();
			this.labelMax2 = new System.Windows.Forms.Label();
			this.labelMax1 = new System.Windows.Forms.Label();
			this.dtpMaxEnd2 = new System.Windows.Forms.DateTimePicker();
			this.dtpMaxStart2 = new System.Windows.Forms.DateTimePicker();
			this.dtpMaxEnd1 = new System.Windows.Forms.DateTimePicker();
			this.dtpMaxStart1 = new System.Windows.Forms.DateTimePicker();
			this.labelInfo = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.labelConstr = new System.Windows.Forms.Label();
			this.panelConstr = new System.Windows.Forms.Panel();
			this.label7 = new System.Windows.Forms.Label();
			this.tbMinUPLbottom = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tbMinNPLbottom = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.tbMaxUPLbottom = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tbMaxNPLbottom = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbMinUPLtop = new System.Windows.Forms.TextBox();
			this.labelMin = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tbMinNPLtop = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbMaxUPLtop = new System.Windows.Forms.TextBox();
			this.labelMax = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.tbMaxNPLtop = new System.Windows.Forms.TextBox();
			this.panelMain.SuspendLayout();
			this.panelConstr.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelMain
			// 
			resources.ApplyResources(this.panelMain, "panelMain");
			this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelMain.Controls.Add(this.labelHyphen2);
			this.panelMain.Controls.Add(this.labelHyphen1);
			this.panelMain.Controls.Add(this.labelMax2);
			this.panelMain.Controls.Add(this.labelMax1);
			this.panelMain.Controls.Add(this.dtpMaxEnd2);
			this.panelMain.Controls.Add(this.dtpMaxStart2);
			this.panelMain.Controls.Add(this.dtpMaxEnd1);
			this.panelMain.Controls.Add(this.dtpMaxStart1);
			this.panelMain.Name = "panelMain";
			// 
			// labelHyphen2
			// 
			resources.ApplyResources(this.labelHyphen2, "labelHyphen2");
			this.labelHyphen2.Name = "labelHyphen2";
			// 
			// labelHyphen1
			// 
			resources.ApplyResources(this.labelHyphen1, "labelHyphen1");
			this.labelHyphen1.Name = "labelHyphen1";
			// 
			// labelMax2
			// 
			resources.ApplyResources(this.labelMax2, "labelMax2");
			this.labelMax2.Name = "labelMax2";
			// 
			// labelMax1
			// 
			resources.ApplyResources(this.labelMax1, "labelMax1");
			this.labelMax1.Name = "labelMax1";
			// 
			// dtpMaxEnd2
			// 
			this.dtpMaxEnd2.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			resources.ApplyResources(this.dtpMaxEnd2, "dtpMaxEnd2");
			this.dtpMaxEnd2.Name = "dtpMaxEnd2";
			this.dtpMaxEnd2.ShowUpDown = true;
			this.dtpMaxEnd2.Value = new System.DateTime(2014, 10, 15, 0, 0, 0, 0);
			// 
			// dtpMaxStart2
			// 
			this.dtpMaxStart2.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			resources.ApplyResources(this.dtpMaxStart2, "dtpMaxStart2");
			this.dtpMaxStart2.Name = "dtpMaxStart2";
			this.dtpMaxStart2.ShowUpDown = true;
			this.dtpMaxStart2.Value = new System.DateTime(2014, 10, 15, 0, 0, 0, 0);
			// 
			// dtpMaxEnd1
			// 
			this.dtpMaxEnd1.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			resources.ApplyResources(this.dtpMaxEnd1, "dtpMaxEnd1");
			this.dtpMaxEnd1.Name = "dtpMaxEnd1";
			this.dtpMaxEnd1.ShowUpDown = true;
			this.dtpMaxEnd1.Value = new System.DateTime(2014, 10, 15, 0, 0, 0, 0);
			// 
			// dtpMaxStart1
			// 
			this.dtpMaxStart1.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			resources.ApplyResources(this.dtpMaxStart1, "dtpMaxStart1");
			this.dtpMaxStart1.Name = "dtpMaxStart1";
			this.dtpMaxStart1.ShowUpDown = true;
			this.dtpMaxStart1.Value = new System.DateTime(2014, 10, 15, 0, 0, 0, 0);
			// 
			// labelInfo
			// 
			resources.ApplyResources(this.labelInfo, "labelInfo");
			this.labelInfo.Name = "labelInfo";
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
			// 
			// labelConstr
			// 
			resources.ApplyResources(this.labelConstr, "labelConstr");
			this.labelConstr.Name = "labelConstr";
			// 
			// panelConstr
			// 
			resources.ApplyResources(this.panelConstr, "panelConstr");
			this.panelConstr.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelConstr.Controls.Add(this.label7);
			this.panelConstr.Controls.Add(this.tbMinUPLbottom);
			this.panelConstr.Controls.Add(this.label8);
			this.panelConstr.Controls.Add(this.tbMinNPLbottom);
			this.panelConstr.Controls.Add(this.label5);
			this.panelConstr.Controls.Add(this.tbMaxUPLbottom);
			this.panelConstr.Controls.Add(this.label6);
			this.panelConstr.Controls.Add(this.tbMaxNPLbottom);
			this.panelConstr.Controls.Add(this.label4);
			this.panelConstr.Controls.Add(this.tbMinUPLtop);
			this.panelConstr.Controls.Add(this.labelMin);
			this.panelConstr.Controls.Add(this.label3);
			this.panelConstr.Controls.Add(this.tbMinNPLtop);
			this.panelConstr.Controls.Add(this.label2);
			this.panelConstr.Controls.Add(this.tbMaxUPLtop);
			this.panelConstr.Controls.Add(this.labelMax);
			this.panelConstr.Controls.Add(this.label1);
			this.panelConstr.Controls.Add(this.tbMaxNPLtop);
			this.panelConstr.Name = "panelConstr";
			// 
			// label7
			// 
			resources.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			// 
			// tbMinUPLbottom
			// 
			resources.ApplyResources(this.tbMinUPLbottom, "tbMinUPLbottom");
			this.tbMinUPLbottom.Name = "tbMinUPLbottom";
			this.tbMinUPLbottom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// label8
			// 
			resources.ApplyResources(this.label8, "label8");
			this.label8.Name = "label8";
			// 
			// tbMinNPLbottom
			// 
			resources.ApplyResources(this.tbMinNPLbottom, "tbMinNPLbottom");
			this.tbMinNPLbottom.Name = "tbMinNPLbottom";
			this.tbMinNPLbottom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// label5
			// 
			resources.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			// 
			// tbMaxUPLbottom
			// 
			resources.ApplyResources(this.tbMaxUPLbottom, "tbMaxUPLbottom");
			this.tbMaxUPLbottom.Name = "tbMaxUPLbottom";
			this.tbMaxUPLbottom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// label6
			// 
			resources.ApplyResources(this.label6, "label6");
			this.label6.Name = "label6";
			// 
			// tbMaxNPLbottom
			// 
			resources.ApplyResources(this.tbMaxNPLbottom, "tbMaxNPLbottom");
			this.tbMaxNPLbottom.Name = "tbMaxNPLbottom";
			this.tbMaxNPLbottom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// label4
			// 
			resources.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			// 
			// tbMinUPLtop
			// 
			resources.ApplyResources(this.tbMinUPLtop, "tbMinUPLtop");
			this.tbMinUPLtop.Name = "tbMinUPLtop";
			this.tbMinUPLtop.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// labelMin
			// 
			resources.ApplyResources(this.labelMin, "labelMin");
			this.labelMin.Name = "labelMin";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// tbMinNPLtop
			// 
			resources.ApplyResources(this.tbMinNPLtop, "tbMinNPLtop");
			this.tbMinNPLtop.Name = "tbMinNPLtop";
			this.tbMinNPLtop.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// tbMaxUPLtop
			// 
			resources.ApplyResources(this.tbMaxUPLtop, "tbMaxUPLtop");
			this.tbMaxUPLtop.Name = "tbMaxUPLtop";
			this.tbMaxUPLtop.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// labelMax
			// 
			resources.ApplyResources(this.labelMax, "labelMax");
			this.labelMax.Name = "labelMax";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// tbMaxNPLtop
			// 
			resources.ApplyResources(this.tbMaxNPLtop, "tbMaxNPLtop");
			this.tbMaxNPLtop.Name = "tbMaxNPLtop";
			this.tbMaxNPLtop.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_KeyPress);
			// 
			// frmPeakLoadTime
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelConstr);
			this.Controls.Add(this.labelConstr);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.labelInfo);
			this.Controls.Add(this.panelMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmPeakLoadTime";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPeakLoadTime_FormClosing);
			this.panelMain.ResumeLayout(false);
			this.panelMain.PerformLayout();
			this.panelConstr.ResumeLayout(false);
			this.panelConstr.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Label labelMax1;
		private System.Windows.Forms.DateTimePicker dtpMaxEnd2;
		private System.Windows.Forms.DateTimePicker dtpMaxStart2;
		private System.Windows.Forms.DateTimePicker dtpMaxEnd1;
		private System.Windows.Forms.DateTimePicker dtpMaxStart1;
		private System.Windows.Forms.Label labelMax2;
		private System.Windows.Forms.Label labelHyphen1;
		private System.Windows.Forms.Label labelHyphen2;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label labelConstr;
		private System.Windows.Forms.Panel panelConstr;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbMaxNPLtop;
		private System.Windows.Forms.Label labelMax;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbMaxUPLtop;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbMinUPLtop;
		private System.Windows.Forms.Label labelMin;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbMinNPLtop;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbMaxUPLbottom;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbMaxNPLbottom;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbMinUPLbottom;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox tbMinNPLbottom;

	}
}