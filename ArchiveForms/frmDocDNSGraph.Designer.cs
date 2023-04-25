namespace EnergomonitoringXP
{
	/// <summary>class frmDocDNSGraph</summary>
	partial class frmDocDNSGraph
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocDNSGraph));
			this.cmGraphs = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiSaveImageAs = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiSetZoomDefault = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiPrint = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tslVisiblePhases = new System.Windows.Forms.ToolStripLabel();
			this.tsbPhaseA = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseB = new System.Windows.Forms.ToolStripButton();
			this.tsbPhaseC = new System.Windows.Forms.ToolStripButton();
			this.printDocument = new System.Drawing.Printing.PrintDocument();
			this.printDlg = new System.Windows.Forms.PrintDialog();
			this.dnsGraphMain = new EmGraphLib.DNS.DNOGraph();
			this.cmGraphs.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmGraphs
			// 
			this.cmGraphs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopy,
            this.tsmiSaveImageAs,
            this.tsmiSetZoomDefault,
            this.tsmiPrint});
			this.cmGraphs.Name = "cmGraphs";
			resources.ApplyResources(this.cmGraphs, "cmGraphs");
			// 
			// tsmiCopy
			// 
			this.tsmiCopy.Name = "tsmiCopy";
			resources.ApplyResources(this.tsmiCopy, "tsmiCopy");
			this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
			// 
			// tsmiSaveImageAs
			// 
			this.tsmiSaveImageAs.Name = "tsmiSaveImageAs";
			resources.ApplyResources(this.tsmiSaveImageAs, "tsmiSaveImageAs");
			this.tsmiSaveImageAs.Click += new System.EventHandler(this.tsmiSaveImageAs_Click);
			// 
			// tsmiSetZoomDefault
			// 
			this.tsmiSetZoomDefault.Name = "tsmiSetZoomDefault";
			resources.ApplyResources(this.tsmiSetZoomDefault, "tsmiSetZoomDefault");
			this.tsmiSetZoomDefault.Click += new System.EventHandler(this.setZoomDefaultToolStripMenuItem_Click);
			// 
			// tsmiPrint
			// 
			this.tsmiPrint.Name = "tsmiPrint";
			resources.ApplyResources(this.tsmiPrint, "tsmiPrint");
			this.tsmiPrint.Click += new System.EventHandler(this.tsmiPrint_Click);
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslVisiblePhases,
            this.tsbPhaseA,
            this.tsbPhaseB,
            this.tsbPhaseC});
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.Name = "toolStrip1";
			// 
			// tslVisiblePhases
			// 
			this.tslVisiblePhases.Name = "tslVisiblePhases";
			resources.ApplyResources(this.tslVisiblePhases, "tslVisiblePhases");
			// 
			// tsbPhaseA
			// 
			this.tsbPhaseA.Checked = true;
			this.tsbPhaseA.CheckOnClick = true;
			this.tsbPhaseA.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			resources.ApplyResources(this.tsbPhaseA, "tsbPhaseA");
			this.tsbPhaseA.Name = "tsbPhaseA";
			this.tsbPhaseA.Click += new System.EventHandler(this.tsbPhaseAny_Click);
			// 
			// tsbPhaseB
			// 
			this.tsbPhaseB.Checked = true;
			this.tsbPhaseB.CheckOnClick = true;
			this.tsbPhaseB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			resources.ApplyResources(this.tsbPhaseB, "tsbPhaseB");
			this.tsbPhaseB.Name = "tsbPhaseB";
			this.tsbPhaseB.Click += new System.EventHandler(this.tsbPhaseAny_Click);
			// 
			// tsbPhaseC
			// 
			this.tsbPhaseC.Checked = true;
			this.tsbPhaseC.CheckOnClick = true;
			this.tsbPhaseC.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPhaseC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			resources.ApplyResources(this.tsbPhaseC, "tsbPhaseC");
			this.tsbPhaseC.Name = "tsbPhaseC";
			this.tsbPhaseC.Click += new System.EventHandler(this.tsbPhaseAny_Click);
			// 
			// printDocument
			// 
			this.printDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument_PrintPage);
			// 
			// printDlg
			// 
			this.printDlg.UseEXDialog = true;
			// 
			// dnsGraphMain
			// 
			this.dnsGraphMain.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.dnsGraphMain, "dnsGraphMain");
			this.dnsGraphMain.Name = "dnsGraphMain";
			this.dnsGraphMain.PhasesToDraw = null;
			this.dnsGraphMain.Spacing = 5;
			this.dnsGraphMain.TickMax = 0D;
			this.dnsGraphMain.TickMin = 0D;
			this.dnsGraphMain.VisibleTickEnd = 0D;
			this.dnsGraphMain.VisibleTickStart = 0D;
			this.dnsGraphMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dnsGraph1_MouseClick);
			this.dnsGraphMain.Resize += new System.EventHandler(this.dnsGraph1_Resize);
			// 
			// frmDocDNSGraph
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.dnsGraphMain);
			this.Controls.Add(this.toolStrip1);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "frmDocDNSGraph";
			this.Load += new System.EventHandler(this.frmDocDNSGraph_Load);
			this.cmGraphs.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		/// <summary>Dip and swell graph</summary>
		public EmGraphLib.DNS.DNOGraph dnsGraphMain;
		/// <summary>ToolStripButton tsbPhaseA</summary>
		public System.Windows.Forms.ToolStripButton tsbPhaseA;
		/// <summary>ToolStripButton tsbPhaseB</summary>
		public System.Windows.Forms.ToolStripButton tsbPhaseB;
		/// <summary>ToolStripButton tsbPhaseC</summary>
		public System.Windows.Forms.ToolStripButton tsbPhaseC;
		private System.Windows.Forms.ContextMenuStrip cmGraphs;
		private System.Windows.Forms.ToolStripMenuItem tsmiSetZoomDefault;
		public System.Windows.Forms.ToolStripLabel tslVisiblePhases;
		public System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
		private System.Windows.Forms.ToolStripMenuItem tsmiSaveImageAs;
		private System.Windows.Forms.ToolStripMenuItem tsmiPrint;
		private System.Drawing.Printing.PrintDocument printDocument;
		private System.Windows.Forms.PrintDialog printDlg;



    }
}