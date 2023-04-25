using System;
using System.Collections.Generic;
using System.Text;
using WeifenLuo.WinFormsUI;
using System.ComponentModel;

using EmServiceLib;

namespace EnergomonitoringXP
{
	public class frmDocPQP : DockContent
	{
		//private IContainer components;

		/// <summary>Dock panel object</summary>
		private DockPanel dockPanel;
		/// <summary>Main dock window</summary>
		public frmDocPQPMain wndDocPQPMain;
		/// <summary>Bottom fliker graph</summary>
		public frmDocFlikGraphBottom wndDocFlikGraphBottom;
        private DockState docstFlikGraphBottom = DockState.DockBottomAutoHide;
        /// <summary>Bottom voltage values graph</summary>
        public frmDocPQPUValGraphBottom wndDocVolValuesGraphBottom;
		public frmDocPQPUValGraphBottom_PQP_A wndDocVolValuesGraphBottom_PQP_A;
        private DockState docstVolValuesGraphBottom = DockState.DockBottomAutoHide;
		/// <summary>Bottom frequency values graph</summary>
		public frmDocFPQPGraphBottom wndDocFValuesGraphBottom;
		private DockState docstFValuesGraphBottom = DockState.DockBottomAutoHide;

		/// <summary>Settings object</summary>
		private EmDataSaver.Settings settings_;
		/// <summary>Pointer to the main application window</summary>
		private frmMain MainWindow_;

		// при открытии архива, нужно закрыть нижние графики от предыдущего архива, а для этого нужен тип устройства предыдущего архива
		private EmDeviceType prevDevType_ = EmDeviceType.NONE;

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
		/// <param name="MainWindow">Pointer to the main application window</param>
		/// <param name="settings">settings object</param>
		public frmDocPQP(frmMain MainWindow, EmDataSaver.Settings settings)
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
			wndDocPQPMain = new frmDocPQPMain(this.MainWindow_, settings_);
			wndDocFlikGraphBottom = new frmDocFlikGraphBottom(this.MainWindow_, settings_);
			wndDocVolValuesGraphBottom_PQP_A = new frmDocPQPUValGraphBottom_PQP_A(this.MainWindow_);
            wndDocVolValuesGraphBottom = new frmDocPQPUValGraphBottom(this.MainWindow_);
			wndDocFValuesGraphBottom = new frmDocFPQPGraphBottom(this.MainWindow_, settings_);

			if (settings_.CurrentLanguage.Equals("ru"))
			{
				wndDocFlikGraphBottom.zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
                wndDocVolValuesGraphBottom.zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				wndDocVolValuesGraphBottom_PQP_A.zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
				wndDocFValuesGraphBottom.zedGraph.CultureInfo = new System.Globalization.CultureInfo("ru-RU");
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
                //if (components != null)
                //{
                //    components.Dispose();
                //}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDocPQP));
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
			// frmDocPQP
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CloseButton = false;
			this.Controls.Add(this.dockPanel);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.Document)));
			this.HideOnClose = true;
			this.Name = "frmDocPQP";
			this.ShowHint = WeifenLuo.WinFormsUI.DockState.Document;
			this.Load += new System.EventHandler(this.frmDocPQP_Load);
			this.ResumeLayout(false);

		}
		#endregion

		public void CloseGraphsOfPrevArchives(EmDeviceType curDevType)
		{
			try
			{
				if (prevDevType_ != EmDeviceType.NONE)
				{
					ShowVolValuesGraph(false, prevDevType_);
					ShowFValuesGraph(false);
					ShowFlikerGraph(false);
				}

				prevDevType_ = curDevType;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in CloseGraphsOfPrevArchives()");
				throw;
			}
		}

		private void frmDocPQP_Load(object sender, EventArgs e)
		{
			wndDocPQPMain.Show(dockPanel, DockState.Document);
			//wndDocFlikGraphBottom.Show(dockPanel, DockState.DockBottomAutoHide);
		}

		public void ShowFlikerGraph(bool show)
		{
			try
			{
				if (show)
				{
					wndDocFlikGraphBottom.Show(dockPanel, docstFlikGraphBottom);
				}
				else
				{
					if (wndDocFlikGraphBottom.DockState != DockState.Hidden &&
						wndDocFlikGraphBottom.DockState != DockState.Unknown)
					{
						docstFlikGraphBottom = wndDocFlikGraphBottom.DockState;
					}

					wndDocFlikGraphBottom.Hide();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ShowFlikerGraph()");
				throw;
			}
		}

		// Vol = voltage
        public void ShowVolValuesGraph(bool show, EmDeviceType devType)
        {
			try
			{
				if (show)
				{
					if (devType != EmDeviceType.ETPQP_A)
						wndDocVolValuesGraphBottom.Show(dockPanel, docstVolValuesGraphBottom);
					else wndDocVolValuesGraphBottom_PQP_A.Show(dockPanel, docstVolValuesGraphBottom);
				}
				else
				{
					if (devType != EmDeviceType.ETPQP_A)
					{
						if (wndDocVolValuesGraphBottom.DockState != DockState.Hidden &&
							wndDocVolValuesGraphBottom.DockState != DockState.Unknown)
						{
							docstVolValuesGraphBottom = wndDocVolValuesGraphBottom.DockState;
						}
						wndDocVolValuesGraphBottom.Hide();
					}
					else
					{
						if (wndDocVolValuesGraphBottom_PQP_A.DockState != DockState.Hidden &&
							wndDocVolValuesGraphBottom_PQP_A.DockState != DockState.Unknown)
						{
							docstVolValuesGraphBottom = wndDocVolValuesGraphBottom_PQP_A.DockState;
						}
						wndDocVolValuesGraphBottom_PQP_A.Hide();
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ShowVolValuesGraph()");
				//throw;
			}
        }

		// F = frequency
		public void ShowFValuesGraph(bool show)
		{
			try
			{
				if (show)
				{
					wndDocFValuesGraphBottom.Show(dockPanel, docstFValuesGraphBottom);
				}
				else
				{
					if (wndDocFValuesGraphBottom.DockState != DockState.Hidden &&
						wndDocFValuesGraphBottom.DockState != DockState.Unknown)
					{
						docstFValuesGraphBottom = wndDocFValuesGraphBottom.DockState;
					}

					wndDocFValuesGraphBottom.Hide();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ShowFValuesGraph()");
				throw;
			}
		}
	}
}
