namespace EnergomonitoringXP
{
	partial class frmEventJournal
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEventJournal));
            this.btnClose = new System.Windows.Forms.Button();
            this.lvEvents = new System.Windows.Forms.ListView();
            this.colDate = new System.Windows.Forms.ColumnHeader();
            this.colEvent = new System.Windows.Forms.ColumnHeader();
            this.gbCntEntry = new System.Windows.Forms.GroupBox();
            this.labelEntries = new System.Windows.Forms.Label();
            this.numCntEntries = new System.Windows.Forms.NumericUpDown();
            this.rbCntEntries = new System.Windows.Forms.RadioButton();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.gbCntEntry.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCntEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lvEvents
            // 
            this.lvEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDate,
            this.colEvent});
            this.lvEvents.GridLines = true;
            resources.ApplyResources(this.lvEvents, "lvEvents");
            this.lvEvents.Name = "lvEvents";
            this.lvEvents.UseCompatibleStateImageBehavior = false;
            this.lvEvents.View = System.Windows.Forms.View.Details;
            // 
            // colDate
            // 
            resources.ApplyResources(this.colDate, "colDate");
            // 
            // colEvent
            // 
            resources.ApplyResources(this.colEvent, "colEvent");
            // 
            // gbCntEntry
            // 
            this.gbCntEntry.Controls.Add(this.labelEntries);
            this.gbCntEntry.Controls.Add(this.numCntEntries);
            this.gbCntEntry.Controls.Add(this.rbCntEntries);
            this.gbCntEntry.Controls.Add(this.rbAll);
            resources.ApplyResources(this.gbCntEntry, "gbCntEntry");
            this.gbCntEntry.Name = "gbCntEntry";
            this.gbCntEntry.TabStop = false;
            // 
            // labelEntries
            // 
            resources.ApplyResources(this.labelEntries, "labelEntries");
            this.labelEntries.Name = "labelEntries";
            // 
            // numCntEntries
            // 
            resources.ApplyResources(this.numCntEntries, "numCntEntries");
            this.numCntEntries.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.numCntEntries.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCntEntries.Name = "numCntEntries";
            this.numCntEntries.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // rbCntEntries
            // 
            resources.ApplyResources(this.rbCntEntries, "rbCntEntries");
            this.rbCntEntries.Checked = true;
            this.rbCntEntries.Name = "rbCntEntries";
            this.rbCntEntries.TabStop = true;
            this.rbCntEntries.UseVisualStyleBackColor = true;
            // 
            // rbAll
            // 
            resources.ApplyResources(this.rbAll, "rbAll");
            this.rbAll.Name = "rbAll";
            this.rbAll.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // frmEventJournal
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.gbCntEntry);
            this.Controls.Add(this.lvEvents);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEventJournal";
            this.Load += new System.EventHandler(this.frmEventJournal_Load);
            this.gbCntEntry.ResumeLayout(false);
            this.gbCntEntry.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCntEntries)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.ListView lvEvents;
		private System.Windows.Forms.ColumnHeader colDate;
		private System.Windows.Forms.ColumnHeader colEvent;
		private System.Windows.Forms.GroupBox gbCntEntry;
		private System.Windows.Forms.Label labelEntries;
		private System.Windows.Forms.NumericUpDown numCntEntries;
		private System.Windows.Forms.RadioButton rbCntEntries;
		private System.Windows.Forms.RadioButton rbAll;
		private System.Windows.Forms.ProgressBar progressBar;
	}
}