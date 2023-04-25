namespace EnergomonitoringXP
{
	partial class frmPgServers
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPgServers));
			this.lvServers = new System.Windows.Forms.ListView();
			this.srvName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.srvHost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.srvPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ilImages = new System.Windows.Forms.ImageList(this.components);
			this.gbServers = new System.Windows.Forms.GroupBox();
			this.gbButtons = new System.Windows.Forms.GroupBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.gbServers.SuspendLayout();
			this.gbButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// lvServers
			// 
			resources.ApplyResources(this.lvServers, "lvServers");
			this.lvServers.CheckBoxes = true;
			this.lvServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.srvName,
            this.srvHost,
            this.srvPort});
			this.lvServers.FullRowSelect = true;
			this.lvServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lvServers.MultiSelect = false;
			this.lvServers.Name = "lvServers";
			this.lvServers.ShowGroups = false;
			this.lvServers.StateImageList = this.ilImages;
			this.lvServers.UseCompatibleStateImageBehavior = false;
			this.lvServers.View = System.Windows.Forms.View.Details;
			this.lvServers.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvServers_ItemChecked);
			// 
			// srvName
			// 
			resources.ApplyResources(this.srvName, "srvName");
			// 
			// srvHost
			// 
			resources.ApplyResources(this.srvHost, "srvHost");
			// 
			// srvPort
			// 
			resources.ApplyResources(this.srvPort, "srvPort");
			// 
			// ilImages
			// 
			this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
			this.ilImages.TransparentColor = System.Drawing.Color.Transparent;
			this.ilImages.Images.SetKeyName(0, "unchecked.bmp");
			this.ilImages.Images.SetKeyName(1, "cheked.bmp");
			// 
			// gbServers
			// 
			resources.ApplyResources(this.gbServers, "gbServers");
			this.gbServers.Controls.Add(this.lvServers);
			this.gbServers.Name = "gbServers";
			this.gbServers.TabStop = false;
			// 
			// gbButtons
			// 
			resources.ApplyResources(this.gbButtons, "gbButtons");
			this.gbButtons.Controls.Add(this.btnCancel);
			this.gbButtons.Controls.Add(this.btnOK);
			this.gbButtons.Name = "gbButtons";
			this.gbButtons.TabStop = false;
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			// 
			// btnOK
			// 
			resources.ApplyResources(this.btnOK, "btnOK");
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Name = "btnOK";
			// 
			// frmPgServers
			// 
			this.AcceptButton = this.btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.gbServers);
			this.Controls.Add(this.gbButtons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmPgServers";
			this.Load += new System.EventHandler(this.frmPgServers_Load);
			this.gbServers.ResumeLayout(false);
			this.gbButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ColumnHeader srvName;
		private System.Windows.Forms.ColumnHeader srvHost;
		private System.Windows.Forms.ColumnHeader srvPort;
		private System.Windows.Forms.ImageList ilImages;
		public System.Windows.Forms.ListView lvServers;
		private System.Windows.Forms.GroupBox gbServers;
		private System.Windows.Forms.GroupBox gbButtons;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
	}
}