namespace EnergomonitoringXP
{
	partial class frmSeparatedGraphs
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSeparatedGraphs));
			this.splitContainerMain = new System.Windows.Forms.SplitContainer();
			this.panelCheck = new System.Windows.Forms.Panel();
			this.panelGraphs = new System.Windows.Forms.Panel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.zedGraphControl1 = new ZedGraph.ZedGraphControl();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.zedGraphControl2 = new ZedGraph.ZedGraphControl();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.zedGraphControl3 = new ZedGraph.ZedGraphControl();
			this.zedGraphControl4 = new ZedGraph.ZedGraphControl();
			this.dtpTo = new System.Windows.Forms.DateTimePicker();
			this.dtpFrom = new System.Windows.Forms.DateTimePicker();
			this.labelFrom = new System.Windows.Forms.Label();
			this.labelTo = new System.Windows.Forms.Label();
			this.btnApply = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			this.panelGraphs.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainerMain
			// 
			resources.ApplyResources(this.splitContainerMain, "splitContainerMain");
			this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainerMain.Name = "splitContainerMain";
			// 
			// splitContainerMain.Panel1
			// 
			resources.ApplyResources(this.splitContainerMain.Panel1, "splitContainerMain.Panel1");
			this.splitContainerMain.Panel1.Controls.Add(this.panelCheck);
			// 
			// splitContainerMain.Panel2
			// 
			resources.ApplyResources(this.splitContainerMain.Panel2, "splitContainerMain.Panel2");
			this.splitContainerMain.Panel2.Controls.Add(this.panelGraphs);
			// 
			// panelCheck
			// 
			resources.ApplyResources(this.panelCheck, "panelCheck");
			this.panelCheck.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelCheck.Name = "panelCheck";
			// 
			// panelGraphs
			// 
			resources.ApplyResources(this.panelGraphs, "panelGraphs");
			this.panelGraphs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelGraphs.Controls.Add(this.splitContainer1);
			this.panelGraphs.Name = "panelGraphs";
			// 
			// splitContainer1
			// 
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
			this.splitContainer1.Panel1.Controls.Add(this.zedGraphControl1);
			// 
			// splitContainer1.Panel2
			// 
			resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			// 
			// zedGraphControl1
			// 
			resources.ApplyResources(this.zedGraphControl1, "zedGraphControl1");
			this.zedGraphControl1.CultureInfo = new System.Globalization.CultureInfo("en-US");
			this.zedGraphControl1.IsAutoScrollRange = false;
			this.zedGraphControl1.IsEnableHPan = true;
			this.zedGraphControl1.IsEnableVPan = true;
			this.zedGraphControl1.IsEnableZoom = true;
			this.zedGraphControl1.IsScrollY2 = false;
			this.zedGraphControl1.IsShowContextMenu = true;
			this.zedGraphControl1.IsShowHScrollBar = false;
			this.zedGraphControl1.IsShowPointValues = false;
			this.zedGraphControl1.IsShowVScrollBar = false;
			this.zedGraphControl1.IsZoomOnMouseCenter = false;
			this.zedGraphControl1.Name = "zedGraphControl1";
			this.zedGraphControl1.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl1.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphControl1.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl1.PointDateFormat = "g";
			this.zedGraphControl1.PointValueFormat = "G";
			this.zedGraphControl1.ScrollMaxX = 0D;
			this.zedGraphControl1.ScrollMaxY = 0D;
			this.zedGraphControl1.ScrollMaxY2 = 0D;
			this.zedGraphControl1.ScrollMinX = 0D;
			this.zedGraphControl1.ScrollMinY = 0D;
			this.zedGraphControl1.ScrollMinY2 = 0D;
			this.zedGraphControl1.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl1.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphControl1.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphControl1.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl1.ZoomStepFraction = 0.1D;
			// 
			// splitContainer2
			// 
			resources.ApplyResources(this.splitContainer2, "splitContainer2");
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
			this.splitContainer2.Panel1.Controls.Add(this.zedGraphControl2);
			// 
			// splitContainer2.Panel2
			// 
			resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
			this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
			// 
			// zedGraphControl2
			// 
			resources.ApplyResources(this.zedGraphControl2, "zedGraphControl2");
			this.zedGraphControl2.CultureInfo = new System.Globalization.CultureInfo("en-US");
			this.zedGraphControl2.IsAutoScrollRange = false;
			this.zedGraphControl2.IsEnableHPan = true;
			this.zedGraphControl2.IsEnableVPan = true;
			this.zedGraphControl2.IsEnableZoom = true;
			this.zedGraphControl2.IsScrollY2 = false;
			this.zedGraphControl2.IsShowContextMenu = true;
			this.zedGraphControl2.IsShowHScrollBar = false;
			this.zedGraphControl2.IsShowPointValues = false;
			this.zedGraphControl2.IsShowVScrollBar = false;
			this.zedGraphControl2.IsZoomOnMouseCenter = false;
			this.zedGraphControl2.Name = "zedGraphControl2";
			this.zedGraphControl2.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl2.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphControl2.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl2.PointDateFormat = "g";
			this.zedGraphControl2.PointValueFormat = "G";
			this.zedGraphControl2.ScrollMaxX = 0D;
			this.zedGraphControl2.ScrollMaxY = 0D;
			this.zedGraphControl2.ScrollMaxY2 = 0D;
			this.zedGraphControl2.ScrollMinX = 0D;
			this.zedGraphControl2.ScrollMinY = 0D;
			this.zedGraphControl2.ScrollMinY2 = 0D;
			this.zedGraphControl2.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl2.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphControl2.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphControl2.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl2.ZoomStepFraction = 0.1D;
			// 
			// splitContainer3
			// 
			resources.ApplyResources(this.splitContainer3, "splitContainer3");
			this.splitContainer3.Name = "splitContainer3";
			// 
			// splitContainer3.Panel1
			// 
			resources.ApplyResources(this.splitContainer3.Panel1, "splitContainer3.Panel1");
			this.splitContainer3.Panel1.Controls.Add(this.zedGraphControl3);
			// 
			// splitContainer3.Panel2
			// 
			resources.ApplyResources(this.splitContainer3.Panel2, "splitContainer3.Panel2");
			this.splitContainer3.Panel2.Controls.Add(this.zedGraphControl4);
			// 
			// zedGraphControl3
			// 
			resources.ApplyResources(this.zedGraphControl3, "zedGraphControl3");
			this.zedGraphControl3.CultureInfo = new System.Globalization.CultureInfo("en-US");
			this.zedGraphControl3.IsAutoScrollRange = false;
			this.zedGraphControl3.IsEnableHPan = true;
			this.zedGraphControl3.IsEnableVPan = true;
			this.zedGraphControl3.IsEnableZoom = true;
			this.zedGraphControl3.IsScrollY2 = false;
			this.zedGraphControl3.IsShowContextMenu = true;
			this.zedGraphControl3.IsShowHScrollBar = false;
			this.zedGraphControl3.IsShowPointValues = false;
			this.zedGraphControl3.IsShowVScrollBar = false;
			this.zedGraphControl3.IsZoomOnMouseCenter = false;
			this.zedGraphControl3.Name = "zedGraphControl3";
			this.zedGraphControl3.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl3.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphControl3.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl3.PointDateFormat = "g";
			this.zedGraphControl3.PointValueFormat = "G";
			this.zedGraphControl3.ScrollMaxX = 0D;
			this.zedGraphControl3.ScrollMaxY = 0D;
			this.zedGraphControl3.ScrollMaxY2 = 0D;
			this.zedGraphControl3.ScrollMinX = 0D;
			this.zedGraphControl3.ScrollMinY = 0D;
			this.zedGraphControl3.ScrollMinY2 = 0D;
			this.zedGraphControl3.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl3.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphControl3.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphControl3.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl3.ZoomStepFraction = 0.1D;
			// 
			// zedGraphControl4
			// 
			resources.ApplyResources(this.zedGraphControl4, "zedGraphControl4");
			this.zedGraphControl4.CultureInfo = new System.Globalization.CultureInfo("en-US");
			this.zedGraphControl4.IsAutoScrollRange = false;
			this.zedGraphControl4.IsEnableHPan = true;
			this.zedGraphControl4.IsEnableVPan = true;
			this.zedGraphControl4.IsEnableZoom = true;
			this.zedGraphControl4.IsScrollY2 = false;
			this.zedGraphControl4.IsShowContextMenu = true;
			this.zedGraphControl4.IsShowHScrollBar = false;
			this.zedGraphControl4.IsShowPointValues = false;
			this.zedGraphControl4.IsShowVScrollBar = false;
			this.zedGraphControl4.IsZoomOnMouseCenter = false;
			this.zedGraphControl4.Name = "zedGraphControl4";
			this.zedGraphControl4.PanButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl4.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
			this.zedGraphControl4.PanModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl4.PointDateFormat = "g";
			this.zedGraphControl4.PointValueFormat = "G";
			this.zedGraphControl4.ScrollMaxX = 0D;
			this.zedGraphControl4.ScrollMaxY = 0D;
			this.zedGraphControl4.ScrollMaxY2 = 0D;
			this.zedGraphControl4.ScrollMinX = 0D;
			this.zedGraphControl4.ScrollMinY = 0D;
			this.zedGraphControl4.ScrollMinY2 = 0D;
			this.zedGraphControl4.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
			this.zedGraphControl4.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
			this.zedGraphControl4.ZoomModifierKeys = System.Windows.Forms.Keys.None;
			this.zedGraphControl4.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
			this.zedGraphControl4.ZoomStepFraction = 0.1D;
			// 
			// dtpTo
			// 
			resources.ApplyResources(this.dtpTo, "dtpTo");
			this.dtpTo.Name = "dtpTo";
			this.dtpTo.ShowUpDown = true;
			// 
			// dtpFrom
			// 
			resources.ApplyResources(this.dtpFrom, "dtpFrom");
			this.dtpFrom.Name = "dtpFrom";
			this.dtpFrom.ShowUpDown = true;
			// 
			// labelFrom
			// 
			resources.ApplyResources(this.labelFrom, "labelFrom");
			this.labelFrom.Name = "labelFrom";
			// 
			// labelTo
			// 
			resources.ApplyResources(this.labelTo, "labelTo");
			this.labelTo.Name = "labelTo";
			// 
			// btnApply
			// 
			resources.ApplyResources(this.btnApply, "btnApply");
			this.btnApply.Name = "btnApply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// frmSeparatedGraphs
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.labelTo);
			this.Controls.Add(this.labelFrom);
			this.Controls.Add(this.dtpTo);
			this.Controls.Add(this.dtpFrom);
			this.Controls.Add(this.splitContainerMain);
			this.Name = "frmSeparatedGraphs";
			this.Load += new System.EventHandler(this.frmSeparatedGraphs_Load);
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			this.panelGraphs.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainerMain;
		private System.Windows.Forms.Panel panelCheck;
		private System.Windows.Forms.Panel panelGraphs;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private ZedGraph.ZedGraphControl zedGraphControl1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private ZedGraph.ZedGraphControl zedGraphControl2;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private ZedGraph.ZedGraphControl zedGraphControl3;
		private ZedGraph.ZedGraphControl zedGraphControl4;
		public System.Windows.Forms.DateTimePicker dtpTo;
		public System.Windows.Forms.DateTimePicker dtpFrom;
		private System.Windows.Forms.Label labelFrom;
		private System.Windows.Forms.Label labelTo;
		private System.Windows.Forms.Button btnApply;
	}
}