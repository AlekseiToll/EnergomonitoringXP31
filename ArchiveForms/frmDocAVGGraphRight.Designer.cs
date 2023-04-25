namespace EnergomonitoringXP
{
	/// <summary>
	/// Averaged parameters graphs
	/// </summary>
	partial class frmDocAVGGraphRight
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocAVGGraphRight));
			this.splitContMain = new System.Windows.Forms.SplitContainer();
			this.splitTopVert = new System.Windows.Forms.SplitContainer();
			this.splitLeft = new System.Windows.Forms.SplitContainer();
			this.zedGraphU = new ZedGraph.ZedGraphControl();
			this.zedGraphI = new ZedGraph.ZedGraphControl();
			this.splitRight = new System.Windows.Forms.SplitContainer();
			this.zedGraphW = new ZedGraph.ZedGraphControl();
			this.zedGraphAngles = new ZedGraph.ZedGraphControl();
			this.radialGraph = new EmGraphLib.Radial.RadialGraph();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tsScaleMode = new System.Windows.Forms.ToolStripButton();
			this.tsSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.tslblNominalVoltage = new System.Windows.Forms.ToolStripLabel();
			this.tsNominalVoltage = new System.Windows.Forms.ToolStripTextBox();
			this.tsSep4 = new System.Windows.Forms.ToolStripSeparator();
			this.tslblNominalCurrent = new System.Windows.Forms.ToolStripLabel();
			this.tsNominalCurrent = new System.Windows.Forms.ToolStripTextBox();
			this.grToolStrip = new System.Windows.Forms.ToolStrip();
			this.tsLg = new System.Windows.Forms.ToolStripButton();
			this.tsSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsFirstHarmonic = new System.Windows.Forms.ToolStripButton();
			this.tsSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsPhaseA = new System.Windows.Forms.ToolStripButton();
			this.tsPhaseB = new System.Windows.Forms.ToolStripButton();
			this.tsPhaseC = new System.Windows.Forms.ToolStripButton();
			this.tsSumm = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsLeft = new System.Windows.Forms.ToolStripButton();
			this.tsRight = new System.Windows.Forms.ToolStripButton();
			this.tsBottom = new System.Windows.Forms.ToolStripButton();
			this.cmsForImage = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiPrintImage = new System.Windows.Forms.ToolStripMenuItem();
			this.printDocument = new System.Drawing.Printing.PrintDocument();
			this.printDlg = new System.Windows.Forms.PrintDialog();
			this.printDocumentCurve = new System.Drawing.Printing.PrintDocument();
			((System.ComponentModel.ISupportInitialize)(this.splitContMain)).BeginInit();
			this.splitContMain.Panel1.SuspendLayout();
			this.splitContMain.Panel2.SuspendLayout();
			this.splitContMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitTopVert)).BeginInit();
			this.splitTopVert.Panel1.SuspendLayout();
			this.splitTopVert.Panel2.SuspendLayout();
			this.splitTopVert.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
			this.splitLeft.Panel1.SuspendLayout();
			this.splitLeft.Panel2.SuspendLayout();
			this.splitLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
			this.splitRight.Panel1.SuspendLayout();
			this.splitRight.Panel2.SuspendLayout();
			this.splitRight.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.grToolStrip.SuspendLayout();
			this.cmsForImage.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContMain
			// 
			resources.ApplyResources(this.splitContMain, "splitContMain");
			this.splitContMain.Name = "splitContMain";
			// 
			// splitContMain.Panel1
			// 
			resources.ApplyResources(this.splitContMain.Panel1, "splitContMain.Panel1");
			this.splitContMain.Panel1.Controls.Add(this.splitTopVert);
			// 
			// splitContMain.Panel2
			// 
			resources.ApplyResources(this.splitContMain.Panel2, "splitContMain.Panel2");
			this.splitContMain.Panel2.Controls.Add(this.radialGraph);
			this.splitContMain.Panel2.Controls.Add(this.toolStrip1);
			// 
			// splitTopVert
			// 
			resources.ApplyResources(this.splitTopVert, "splitTopVert");
			this.splitTopVert.Name = "splitTopVert";
			// 
			// splitTopVert.Panel1
			// 
			resources.ApplyResources(this.splitTopVert.Panel1, "splitTopVert.Panel1");
			this.splitTopVert.Panel1.Controls.Add(this.splitLeft);
			// 
			// splitTopVert.Panel2
			// 
			resources.ApplyResources(this.splitTopVert.Panel2, "splitTopVert.Panel2");
			this.splitTopVert.Panel2.Controls.Add(this.splitRight);
			// 
			// splitLeft
			// 
			resources.ApplyResources(this.splitLeft, "splitLeft");
			this.splitLeft.Name = "splitLeft";
			// 
			// splitLeft.Panel1
			// 
			resources.ApplyResources(this.splitLeft.Panel1, "splitLeft.Panel1");
			this.splitLeft.Panel1.Controls.Add(this.zedGraphU);
			// 
			// splitLeft.Panel2
			// 
			resources.ApplyResources(this.splitLeft.Panel2, "splitLeft.Panel2");
			this.splitLeft.Panel2.Controls.Add(this.zedGraphI);
			// 
			// zedGraphU
			// 
			resources.ApplyResources(this.zedGraphU, "zedGraphU");
			this.zedGraphU.CultureInfo = new System.Globalization.CultureInfo("");
			this.zedGraphU.IsAutoScrollRange = false;
			this.zedGraphU.IsEnableHPan = true;
			this.zedGraphU.IsEnableVPan = true;
			this.zedGraphU.IsEnableZoom = true;
			this.zedGraphU.IsScrollY2 = false;
			this.zedGraphU.IsShowContextMenu = true;
			this.zedGraphU.IsShowHScrollBar = false;
			this.zedGraphU.IsShowPointValues = false;
			this.zedGraphU.IsShowVScrollBar = false;
			this.zedGraphU.IsZoomOnMouseCenter = false;
			this.zedGraphU.Name = "zedGraphU";
			this.zedGraphU.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphU.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphU.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphU.PointDateFormat = "g";
			this.zedGraphU.PointValueFormat = "G";
			this.zedGraphU.ScrollMaxX = 0D;
			this.zedGraphU.ScrollMaxY = 0D;
			this.zedGraphU.ScrollMaxY2 = 0D;
			this.zedGraphU.ScrollMinX = 0D;
			this.zedGraphU.ScrollMinY = 0D;
			this.zedGraphU.ScrollMinY2 = 0D;
			this.zedGraphU.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphU.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphU.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphU.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphU.ZoomStepFraction = 0.1D;
			// 
			// zedGraphI
			// 
			resources.ApplyResources(this.zedGraphI, "zedGraphI");
			this.zedGraphI.CultureInfo = new System.Globalization.CultureInfo("");
			this.zedGraphI.IsAutoScrollRange = false;
			this.zedGraphI.IsEnableHPan = true;
			this.zedGraphI.IsEnableVPan = true;
			this.zedGraphI.IsEnableZoom = true;
			this.zedGraphI.IsScrollY2 = false;
			this.zedGraphI.IsShowContextMenu = true;
			this.zedGraphI.IsShowHScrollBar = false;
			this.zedGraphI.IsShowPointValues = false;
			this.zedGraphI.IsShowVScrollBar = false;
			this.zedGraphI.IsZoomOnMouseCenter = false;
			this.zedGraphI.Name = "zedGraphI";
			this.zedGraphI.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphI.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphI.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphI.PointDateFormat = "g";
			this.zedGraphI.PointValueFormat = "G";
			this.zedGraphI.ScrollMaxX = 0D;
			this.zedGraphI.ScrollMaxY = 0D;
			this.zedGraphI.ScrollMaxY2 = 0D;
			this.zedGraphI.ScrollMinX = 0D;
			this.zedGraphI.ScrollMinY = 0D;
			this.zedGraphI.ScrollMinY2 = 0D;
			this.zedGraphI.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphI.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphI.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphI.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphI.ZoomStepFraction = 0.1D;
			// 
			// splitRight
			// 
			resources.ApplyResources(this.splitRight, "splitRight");
			this.splitRight.Name = "splitRight";
			// 
			// splitRight.Panel1
			// 
			resources.ApplyResources(this.splitRight.Panel1, "splitRight.Panel1");
			this.splitRight.Panel1.Controls.Add(this.zedGraphW);
			// 
			// splitRight.Panel2
			// 
			resources.ApplyResources(this.splitRight.Panel2, "splitRight.Panel2");
			this.splitRight.Panel2.Controls.Add(this.zedGraphAngles);
			// 
			// zedGraphW
			// 
			resources.ApplyResources(this.zedGraphW, "zedGraphW");
			this.zedGraphW.CultureInfo = new System.Globalization.CultureInfo("");
			this.zedGraphW.IsAutoScrollRange = false;
			this.zedGraphW.IsEnableHPan = true;
			this.zedGraphW.IsEnableVPan = true;
			this.zedGraphW.IsEnableZoom = true;
			this.zedGraphW.IsScrollY2 = false;
			this.zedGraphW.IsShowContextMenu = true;
			this.zedGraphW.IsShowHScrollBar = false;
			this.zedGraphW.IsShowPointValues = false;
			this.zedGraphW.IsShowVScrollBar = false;
			this.zedGraphW.IsZoomOnMouseCenter = false;
			this.zedGraphW.Name = "zedGraphW";
			this.zedGraphW.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphW.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphW.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphW.PointDateFormat = "g";
			this.zedGraphW.PointValueFormat = "G";
			this.zedGraphW.ScrollMaxX = 0D;
			this.zedGraphW.ScrollMaxY = 0D;
			this.zedGraphW.ScrollMaxY2 = 0D;
			this.zedGraphW.ScrollMinX = 0D;
			this.zedGraphW.ScrollMinY = 0D;
			this.zedGraphW.ScrollMinY2 = 0D;
			this.zedGraphW.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphW.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphW.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphW.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphW.ZoomStepFraction = 0.1D;
			// 
			// zedGraphAngles
			// 
			resources.ApplyResources(this.zedGraphAngles, "zedGraphAngles");
			this.zedGraphAngles.CultureInfo = new System.Globalization.CultureInfo("");
			this.zedGraphAngles.IsAutoScrollRange = false;
			this.zedGraphAngles.IsEnableHPan = true;
			this.zedGraphAngles.IsEnableVPan = true;
			this.zedGraphAngles.IsEnableZoom = true;
			this.zedGraphAngles.IsScrollY2 = false;
			this.zedGraphAngles.IsShowContextMenu = true;
			this.zedGraphAngles.IsShowHScrollBar = false;
			this.zedGraphAngles.IsShowPointValues = false;
			this.zedGraphAngles.IsShowVScrollBar = false;
			this.zedGraphAngles.IsZoomOnMouseCenter = false;
			this.zedGraphAngles.Name = "zedGraphAngles";
			this.zedGraphAngles.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphAngles.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphAngles.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphAngles.PointDateFormat = "g";
			this.zedGraphAngles.PointValueFormat = "G";
			this.zedGraphAngles.ScrollMaxX = 0D;
			this.zedGraphAngles.ScrollMaxY = 0D;
			this.zedGraphAngles.ScrollMaxY2 = 0D;
			this.zedGraphAngles.ScrollMinX = 0D;
			this.zedGraphAngles.ScrollMinY = 0D;
			this.zedGraphAngles.ScrollMinY2 = 0D;
			this.zedGraphAngles.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphAngles.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphAngles.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphAngles.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphAngles.ZoomStepFraction = 0.1D;
			// 
			// radialGraph
			// 
			resources.ApplyResources(this.radialGraph, "radialGraph");
			this.radialGraph.BackColor = System.Drawing.Color.White;
			this.radialGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.radialGraph.Name = "radialGraph";
			this.radialGraph.RealValues = false;
			this.radialGraph.Title = "";
			this.radialGraph.MouseClick += new System.Windows.Forms.MouseEventHandler(this.radialGraph_MouseClick);
			// 
			// toolStrip1
			// 
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsScaleMode,
            this.tsSep3,
            this.tslblNominalVoltage,
            this.tsNominalVoltage,
            this.tsSep4,
            this.tslblNominalCurrent,
            this.tsNominalCurrent});
			this.toolStrip1.Name = "toolStrip1";
			// 
			// tsScaleMode
			// 
			resources.ApplyResources(this.tsScaleMode, "tsScaleMode");
			this.tsScaleMode.CheckOnClick = true;
			this.tsScaleMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsScaleMode.Name = "tsScaleMode";
			this.tsScaleMode.Click += new System.EventHandler(this.tsScaleMode_Click);
			// 
			// tsSep3
			// 
			resources.ApplyResources(this.tsSep3, "tsSep3");
			this.tsSep3.Name = "tsSep3";
			// 
			// tslblNominalVoltage
			// 
			resources.ApplyResources(this.tslblNominalVoltage, "tslblNominalVoltage");
			this.tslblNominalVoltage.Name = "tslblNominalVoltage";
			// 
			// tsNominalVoltage
			// 
			resources.ApplyResources(this.tsNominalVoltage, "tsNominalVoltage");
			this.tsNominalVoltage.Name = "tsNominalVoltage";
			this.tsNominalVoltage.ReadOnly = true;
			this.tsNominalVoltage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tsNominalVoltage_KeyPress);
			// 
			// tsSep4
			// 
			resources.ApplyResources(this.tsSep4, "tsSep4");
			this.tsSep4.Name = "tsSep4";
			// 
			// tslblNominalCurrent
			// 
			resources.ApplyResources(this.tslblNominalCurrent, "tslblNominalCurrent");
			this.tslblNominalCurrent.Name = "tslblNominalCurrent";
			// 
			// tsNominalCurrent
			// 
			resources.ApplyResources(this.tsNominalCurrent, "tsNominalCurrent");
			this.tsNominalCurrent.Name = "tsNominalCurrent";
			this.tsNominalCurrent.Leave += new System.EventHandler(this.tsNominalCurrent_Leave);
			this.tsNominalCurrent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tsNominalCurrent_KeyPress);
			this.tsNominalCurrent.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tsNominalCurrent_KeyUp);
			// 
			// grToolStrip
			// 
			resources.ApplyResources(this.grToolStrip, "grToolStrip");
			this.grToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLg,
            this.tsSep1,
            this.tsFirstHarmonic,
            this.tsSep2,
            this.tsPhaseA,
            this.tsPhaseB,
            this.tsPhaseC,
            this.tsSumm,
            this.toolStripSeparator1,
            this.tsLeft,
            this.tsRight,
            this.tsBottom});
			this.grToolStrip.Name = "grToolStrip";
			// 
			// tsLg
			// 
			resources.ApplyResources(this.tsLg, "tsLg");
			this.tsLg.CheckOnClick = true;
			this.tsLg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsLg.Name = "tsLg";
			this.tsLg.Click += new System.EventHandler(this.GraphPanelSmthWasChanged);
			// 
			// tsSep1
			// 
			resources.ApplyResources(this.tsSep1, "tsSep1");
			this.tsSep1.Name = "tsSep1";
			// 
			// tsFirstHarmonic
			// 
			resources.ApplyResources(this.tsFirstHarmonic, "tsFirstHarmonic");
			this.tsFirstHarmonic.CheckOnClick = true;
			this.tsFirstHarmonic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsFirstHarmonic.Name = "tsFirstHarmonic";
			this.tsFirstHarmonic.Click += new System.EventHandler(this.GraphPanelSmthWasChanged);
			// 
			// tsSep2
			// 
			resources.ApplyResources(this.tsSep2, "tsSep2");
			this.tsSep2.Name = "tsSep2";
			// 
			// tsPhaseA
			// 
			resources.ApplyResources(this.tsPhaseA, "tsPhaseA");
			this.tsPhaseA.Checked = true;
			this.tsPhaseA.CheckOnClick = true;
			this.tsPhaseA.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsPhaseA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsPhaseA.Name = "tsPhaseA";
			this.tsPhaseA.Click += new System.EventHandler(this.GraphPanelSmthWasChanged);
			// 
			// tsPhaseB
			// 
			resources.ApplyResources(this.tsPhaseB, "tsPhaseB");
			this.tsPhaseB.Checked = true;
			this.tsPhaseB.CheckOnClick = true;
			this.tsPhaseB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsPhaseB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsPhaseB.Name = "tsPhaseB";
			this.tsPhaseB.Click += new System.EventHandler(this.GraphPanelSmthWasChanged);
			// 
			// tsPhaseC
			// 
			resources.ApplyResources(this.tsPhaseC, "tsPhaseC");
			this.tsPhaseC.Checked = true;
			this.tsPhaseC.CheckOnClick = true;
			this.tsPhaseC.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsPhaseC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsPhaseC.Name = "tsPhaseC";
			this.tsPhaseC.Click += new System.EventHandler(this.GraphPanelSmthWasChanged);
			// 
			// tsSumm
			// 
			resources.ApplyResources(this.tsSumm, "tsSumm");
			this.tsSumm.Checked = true;
			this.tsSumm.CheckOnClick = true;
			this.tsSumm.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsSumm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsSumm.Name = "tsSumm";
			this.tsSumm.Click += new System.EventHandler(this.GraphPanelSmthWasChanged);
			// 
			// toolStripSeparator1
			// 
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			// 
			// tsLeft
			// 
			resources.ApplyResources(this.tsLeft, "tsLeft");
			this.tsLeft.Checked = true;
			this.tsLeft.CheckOnClick = true;
			this.tsLeft.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsLeft.Name = "tsLeft";
			this.tsLeft.Click += new System.EventHandler(this.tsLeft_Click);
			// 
			// tsRight
			// 
			resources.ApplyResources(this.tsRight, "tsRight");
			this.tsRight.Checked = true;
			this.tsRight.CheckOnClick = true;
			this.tsRight.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsRight.Name = "tsRight";
			this.tsRight.Click += new System.EventHandler(this.tsRight_Click);
			// 
			// tsBottom
			// 
			resources.ApplyResources(this.tsBottom, "tsBottom");
			this.tsBottom.Checked = true;
			this.tsBottom.CheckOnClick = true;
			this.tsBottom.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsBottom.Name = "tsBottom";
			this.tsBottom.Click += new System.EventHandler(this.tsBottom_Click);
			// 
			// cmsForImage
			// 
			resources.ApplyResources(this.cmsForImage, "cmsForImage");
			this.cmsForImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopy,
            this.tsmiSaveAs,
            this.tsmiPrintImage});
			this.cmsForImage.Name = "cmsForImage";
			// 
			// tsmiCopy
			// 
			resources.ApplyResources(this.tsmiCopy, "tsmiCopy");
			this.tsmiCopy.Name = "tsmiCopy";
			this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
			// 
			// tsmiSaveAs
			// 
			resources.ApplyResources(this.tsmiSaveAs, "tsmiSaveAs");
			this.tsmiSaveAs.Name = "tsmiSaveAs";
			this.tsmiSaveAs.Click += new System.EventHandler(this.tsmiSaveAs_Click);
			// 
			// tsmiPrintImage
			// 
			resources.ApplyResources(this.tsmiPrintImage, "tsmiPrintImage");
			this.tsmiPrintImage.Name = "tsmiPrintImage";
			this.tsmiPrintImage.Click += new System.EventHandler(this.tsmiPrintImage_Click);
			// 
			// printDocument
			// 
			this.printDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument_PrintPage);
			// 
			// printDlg
			// 
			this.printDlg.UseEXDialog = true;
			// 
			// frmDocAVGGraphRight
			// 
			resources.ApplyResources(this, "$this");
			this.AutoHidePortion = 0.2D;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.splitContMain);
			this.Controls.Add(this.grToolStrip);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)(((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockRight)
						| WeifenLuo.WinFormsUI.DockAreas.Document)));
			this.HideOnClose = true;
			this.Name = "frmDocAVGGraphRight";
			this.Load += new System.EventHandler(this.frmGraphRightPanel_Load);
			this.splitContMain.Panel1.ResumeLayout(false);
			this.splitContMain.Panel2.ResumeLayout(false);
			this.splitContMain.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContMain)).EndInit();
			this.splitContMain.ResumeLayout(false);
			this.splitTopVert.Panel1.ResumeLayout(false);
			this.splitTopVert.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitTopVert)).EndInit();
			this.splitTopVert.ResumeLayout(false);
			this.splitLeft.Panel1.ResumeLayout(false);
			this.splitLeft.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
			this.splitLeft.ResumeLayout(false);
			this.splitRight.Panel1.ResumeLayout(false);
			this.splitRight.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
			this.splitRight.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.grToolStrip.ResumeLayout(false);
			this.grToolStrip.PerformLayout();
			this.cmsForImage.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip grToolStrip;
		private System.Windows.Forms.ToolStripSeparator tsSep1;
		private System.Windows.Forms.ToolStripSeparator tsSep2;
		public System.Windows.Forms.ToolStripButton tsLg;
		public System.Windows.Forms.ToolStripButton tsFirstHarmonic;
		public System.Windows.Forms.ToolStripButton tsPhaseA;
		public System.Windows.Forms.ToolStripButton tsPhaseB;
		public System.Windows.Forms.ToolStripButton tsPhaseC;
		private System.Windows.Forms.SplitContainer splitLeft;
		private EmGraphLib.Radial.RadialGraph radialGraph;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripSeparator tsSep3;
		private System.Windows.Forms.ToolStripTextBox tsNominalCurrent;
		private System.Windows.Forms.ToolStripLabel tslblNominalCurrent;
		private System.Windows.Forms.ToolStripLabel tslblNominalVoltage;
		private System.Windows.Forms.ToolStripTextBox tsNominalVoltage;
		private System.Windows.Forms.ToolStripSeparator tsSep4;
		public ZedGraph.ZedGraphControl zedGraphU;
		public ZedGraph.ZedGraphControl zedGraphI;
		public System.Windows.Forms.ToolStripButton tsScaleMode;
		private System.Windows.Forms.SplitContainer splitContMain;
		private System.Windows.Forms.SplitContainer splitRight;
		public ZedGraph.ZedGraphControl zedGraphW;
		public ZedGraph.ZedGraphControl zedGraphAngles;
		private System.Windows.Forms.SplitContainer splitTopVert;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsLeft;
		private System.Windows.Forms.ToolStripButton tsRight;
		private System.Windows.Forms.ToolStripButton tsBottom;
		private System.Windows.Forms.ToolStripButton tsSumm;
		private System.Windows.Forms.ContextMenuStrip cmsForImage;
		private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
		private System.Windows.Forms.ToolStripMenuItem tsmiSaveAs;
		private System.Windows.Forms.ToolStripMenuItem tsmiPrintImage;
		private System.Drawing.Printing.PrintDocument printDocument;
		private System.Windows.Forms.PrintDialog printDlg;
		private System.Drawing.Printing.PrintDocument printDocumentCurve;
	}
}