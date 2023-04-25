using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Resources;
using System.Drawing.Drawing2D;

using DataGridColumnStyles;
using ZedGraph;
using EnergomonitoringXP.Graph;
using WeifenLuo.WinFormsUI;
using EmServiceLib;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmDoc2.
	/// </summary>
	public class frmDocAVG : DockContent
	{
		//private IContainer components;

		/// <summary>Dock panel object</summary>
		private DockPanel dockPanel;

		/// <summary>Main dock window</summary>
		public frmDocAVGMain wndDocAVGMain;
		/// <summary>Bottom avg graph</summary>
		public frmDocAVGGraphBottom wndDocAVGGraphBottom;
		/// <summary>Right avg graph</summary>
		public frmDocAVGGraphRight wndDocAVGGraphRight;
		
		
		/// <summary>Settings object</summary>
		private EmDataSaver.Settings settings_;		
		/// <summary>Pointer to the main application window</summary>
		private frmMain MainWindow_;

		private EmDeviceType curDevType_;
		

		/// <summary>
		/// Synchronize settings
		/// </summary>
		/// <param name="NewSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			settings_ = NewSettings.Clone();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public frmDocAVG(frmMain MainWindow, EmDataSaver.Settings settings, EmDeviceType devType)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.settings_ = settings;
			this.MainWindow_ = MainWindow;
			this.curDevType_ = devType;

			wndDocAVGMain = new frmDocAVGMain(this.MainWindow_, settings);
			wndDocAVGGraphBottom = new frmDocAVGGraphBottom(this.MainWindow_, settings);
			wndDocAVGGraphRight = new frmDocAVGGraphRight(this.MainWindow_, settings, curDevType_);
			if (settings.CurrentLanguage.Equals("ru"))
			{
				wndDocAVGGraphBottom.zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				wndDocAVGGraphRight.zedGraphI.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				wndDocAVGGraphRight.zedGraphU.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				wndDocAVGGraphRight.zedGraphAngles.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				wndDocAVGGraphRight.zedGraphW.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
                //if(components != null)
                //{
                //    components.Dispose();
                //}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocAVG));
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
			// frmDocAVG
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.dockPanel);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.Document)));
			this.HideOnClose = true;
			this.Name = "frmDocAVG";
			this.ShowHint = WeifenLuo.WinFormsUI.DockState.Document;
			this.Load += new System.EventHandler(this.frmDocAVG_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void frmDocAVG_Load(object sender, EventArgs e)
		{
			try
			{
				wndDocAVGMain.Show(dockPanel, DockState.Document);
				wndDocAVGGraphBottom.Show(dockPanel, DockState.DockBottomAutoHide);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDocAVG_Load(): ");
				throw;
			}
		}

		public void CloseGraphForms()
		{
			if (wndDocAVGGraphBottom != null) wndDocAVGGraphBottom.CloseGraphsForms();
		}

		public void SetDeviceType(EmDeviceType devType)
		{
			try
			{
				this.curDevType_ = devType;
				wndDocAVGGraphRight.SetDeviceType(devType);

				if (wndDocAVGGraphRight != null)
				{
					wndDocAVGGraphRight.Show(dockPanel, DockState.DockRightAutoHide);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SetDeviceType(): ");
				throw;
			}
		}
	}
}
