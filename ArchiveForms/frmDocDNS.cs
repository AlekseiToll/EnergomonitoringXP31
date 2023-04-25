// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Average values float-window form

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Resources;
using System.Globalization;

using WeifenLuo;
using WeifenLuo.WinFormsUI;
using DbServiceLib;
using DataGridColumnStyles;
using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmDoc2.
	/// </summary>
	public class frmDocDNS : DockContent
	{

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		/// <summary>Pointer to the main application window</summary>
		protected frmMain mainWindow_;
		/// <summary>Settings object</summary>
		private EmDataSaver.Settings settings_;
		private EmDeviceType devType_;

		/// <summary>Dock panel object</summary>
		private DockPanel dockPanel;
		/// <summary>Dips and Swells window</summary>
		public frmDocDNSMain wndDocDNSMain;
		/// <summary>Dips and Swells window</summary>
		public frmDocDNSGraph wndDocDNSGraph;

		/// <summary>Synchronize settings</summary>
		/// <param name="NewSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			settings_ = NewSettings.Clone();
		}

        /// <summary>
        /// Default construcror
        /// </summary>
		public frmDocDNS(frmMain mainWindow, EmDataSaver.Settings settings, EmDeviceType devType)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			settings_ = settings;
			mainWindow_ = mainWindow;
			devType_ = devType;
			this.wndDocDNSMain = new frmDocDNSMain(this.mainWindow_, settings);
			this.wndDocDNSGraph = new frmDocDNSGraph(this.mainWindow_, settings);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocDNS));
            this.dockPanel = new WeifenLuo.WinFormsUI.DockPanel();
            this.SuspendLayout();
            // 
            // dockPanel
            // 
            this.dockPanel.ActiveAutoHideContent = null;
            resources.ApplyResources(this.dockPanel, "dockPanel");
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.SdiDocument = true;
            // 
            // frmDocDNS
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CloseButton = false;
            this.Controls.Add(this.dockPanel);
            this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.Document)));
            this.HideOnClose = true;
            this.Name = "frmDocDNS";
            this.ShowHint = WeifenLuo.WinFormsUI.DockState.Document;
            this.Load += new System.EventHandler(this.frmDocDNS_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void frmDocDNS_Load(object sender, EventArgs e)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string wndTitle;
				if(devType_ != EmDeviceType.ETPQP_A) wndTitle = rm.GetString("name_measure_type_dns_full");
				else wndTitle = rm.GetString("name_measure_type_events_full");
				this.Text = wndTitle;

				wndDocDNSMain.Show(dockPanel, WeifenLuo.WinFormsUI.DockState.Document);
				wndDocDNSGraph.Show(dockPanel, WeifenLuo.WinFormsUI.DockState.DockBottom);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDocDNS_Load():");
				throw;
			}
		}
	}
}
