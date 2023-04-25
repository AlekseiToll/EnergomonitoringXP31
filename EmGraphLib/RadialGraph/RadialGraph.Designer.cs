	namespace EmGraphLib.Radial
{
	partial class RadialGraph
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.myFowPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.SuspendLayout();
			// 
			// myFowPanel
			// 
			this.myFowPanel.AutoSize = true;
			this.myFowPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.myFowPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.myFowPanel.Location = new System.Drawing.Point(0, 0);
			this.myFowPanel.Name = "myFowPanel";
			this.myFowPanel.Size = new System.Drawing.Size(189, 0);
			this.myFowPanel.TabIndex = 0;
			this.myFowPanel.Resize += new System.EventHandler(this.myFowPanel_Resize);
			// 
			// RadialGraph
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.myFowPanel);
			this.Name = "RadialGraph";
			this.Size = new System.Drawing.Size(189, 282);
			this.Resize += new System.EventHandler(this.RadialGraph_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel myFowPanel;

	}
}
