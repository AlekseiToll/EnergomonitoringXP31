namespace EnergomonitoringXP
{
	partial class frmConstraintsLoadDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConstraintsLoadDetails));
            this.lvcCommon = new System.Windows.Forms.ColumnHeader();
            this.ilDetails = new System.Windows.Forms.ImageList(this.components);
            this.lvDetails = new System.Windows.Forms.ListView();
            this.myColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // ilDetails
            // 
            this.ilDetails.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilDetails.ImageStream")));
            this.ilDetails.TransparentColor = System.Drawing.Color.Transparent;
            this.ilDetails.Images.SetKeyName(0, "OK.bmp");
            this.ilDetails.Images.SetKeyName(1, "Warning.bmp");
            // 
            // lvDetails
            // 
            this.lvDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.myColumnHeader});
            resources.ApplyResources(this.lvDetails, "lvDetails");
            this.lvDetails.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("lvDetails.Groups"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("lvDetails.Groups1")))});
            this.lvDetails.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lvDetails.Items"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lvDetails.Items1"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lvDetails.Items2"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lvDetails.Items3"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lvDetails.Items4"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("lvDetails.Items5")))});
            this.lvDetails.Name = "lvDetails";
            this.lvDetails.SmallImageList = this.ilDetails;
            this.lvDetails.UseCompatibleStateImageBehavior = false;
            this.lvDetails.View = System.Windows.Forms.View.Details;
            // 
            // myColumnHeader
            // 
            resources.ApplyResources(this.myColumnHeader, "myColumnHeader");
            // 
            // frmConstraintsLoadDetails
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "frmConstraintsLoadDetails";
            this.ShowInTaskbar = false;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmConstraintsLoadDetails_KeyDown);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ColumnHeader lvcCommon;
		private System.Windows.Forms.ImageList ilDetails;
		private System.Windows.Forms.ListView lvDetails;
		private System.Windows.Forms.ColumnHeader myColumnHeader;

	}
}