using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;
using ZedGraph;
using System.Resources;
using System.Drawing.Imaging;
using System.IO;

using EmServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmDocDNSGraph : DockContent
	{
		/// <summary>Pointer to the main application window</summary>
		protected frmMain MainWindow;
		/// <summary>Settings object</summary>
		private EmDataSaver.Settings settings;

		internal ConnectScheme ConnectionScheme;


		/// <summary>Default constructor</summary>
		public frmDocDNSGraph(frmMain MainWindow, EmDataSaver.Settings settings)
		{
			InitializeComponent();

			this.settings = settings;
			this.MainWindow = MainWindow;
		}
		
		/// <summary>Synchronize settings</summary>
		/// <param name="NewSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			settings = NewSettings.Clone();
		}

		private void frmDocDNSGraph_Load(object sender, EventArgs e)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", 
											sender.GetType().Assembly);

				//dnsGraphMain.Grid.NominalValue = 50;
				dnsGraphMain.Grid.LimitValue = 0.10F;

				dnsGraphMain.Grid.NominalLine.Label.Color = Color.SlateGray;
				dnsGraphMain.Grid.NominalLine.Label.Format = rm.GetString("name.dno.labels.nominal_line");

				dnsGraphMain.Grid.LimitLine.Label.Color = Color.SlateGray;
				dnsGraphMain.Grid.LimitLine.Label.Format = rm.GetString("name.dno.labels.limit_line");

				dnsGraphMain.Grid.NominalLine.Color = Color.FromArgb(0x80, Color.SteelBlue);
				dnsGraphMain.Grid.LimitLine.Color = Color.FromArgb(0x80, Color.SlateGray);

				dnsGraphMain.Items.OutOfNormalRegion.BackColor = Color.FromArgb(0x80, Color.Salmon);
				dnsGraphMain.Items.OutOfNormalRegion.ForeColor = Color.FromArgb(0x80, Color.Brown);

				dnsGraphMain.Grid.LimitLine.Style = System.Drawing.Drawing2D.DashStyle.Dot;

				// customizing highliht
				dnsGraphMain.Items.Highlight.ForeColor = Color.SteelBlue;
				dnsGraphMain.Items.Highlight.BackColor = Color.FromArgb(0x80, Color.SteelBlue);

				// customizing timeline
				dnsGraphMain.Grid.TimeLine.Color = Color.SlateGray;
				dnsGraphMain.Grid.TimeLine.Label.Color = Color.SlateGray;

				//dnsGraphMain.PhasesToDraw = new EmGraphLib.DNS.Phase[] { 
				//	EmGraphLib.DNS.Phase.A, EmGraphLib.DNS.Phase.B, EmGraphLib.DNS.Phase.C };
				selectPhasesToDraw();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in frmDocDNSGraph_Load(): " + ex.Message);
				throw;
			}
		}

		private void dnsGraph1_Resize(object sender, EventArgs e)
		{
			try
			{
				dnsGraphMain.Invalidate();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dnsGraph1_Resize(): " + ex.Message);
				throw;
			}
		}

		private void tsbPhaseAny_Click(object sender, EventArgs e)
		{
			try
			{
				selectPhasesToDraw();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsbPhaseAny_Click(): " + ex.Message);
				throw;
			}
		}

		internal void selectPhasesToDraw()
		{
			try
			{
				List<EmGraphLib.DNS.Phase> phases = new List<EmGraphLib.DNS.Phase>();
				if (tsbPhaseA.Checked) phases.Add(EmGraphLib.DNS.Phase.A);
				if (tsbPhaseB.Checked) phases.Add(EmGraphLib.DNS.Phase.B);
				if (tsbPhaseC.Checked) phases.Add(EmGraphLib.DNS.Phase.C);

				EmGraphLib.DNS.Phase[] phases_arr = new EmGraphLib.DNS.Phase[phases.Count];
				for (int i = 0; i < phases.Count; i++)
				{
					phases_arr[i] = phases[i];
				}

				dnsGraphMain.PhasesToDraw = phases_arr;
				dnsGraphMain.Invalidate();

				if ((this.MainWindow.wndDocDNS.wndDocDNSGraph.tsbPhaseA.Checked &&
						this.MainWindow.wndDocDNS.wndDocDNSGraph.tsbPhaseB.Checked &&
						this.MainWindow.wndDocDNS.wndDocDNSGraph.tsbPhaseC.Checked) || 
						ConnectionScheme == ConnectScheme.Ph1W2)
				{
					this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Items.Highlight.Visible = true;
					this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.RedrawHighlight();
				}
				else
				{
					this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Invalidate();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in selectPhasesToDraw(): " + ex.Message);
				throw;
			}
		}

		private void setZoomDefaultToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.ResetZoom();
				this.MainWindow.wndDocDNS.wndDocDNSGraph.dnsGraphMain.Refresh();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in setZoomDefaultToolStripMenuItem_Click(): " 
							+ ex.Message);
				throw;
			}
		}

		private void tsmiSaveImageAs_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialog saveDlg = new SaveFileDialog();
				saveDlg.Filter = "Jpeg Format (*.jpg)|*.jpg|" +
								"PNG Format (*.png)|*.png|" +
								"Gif Format (*.gif)|*.gif|" +
								"Tiff Format (*.tif)|*.tif|" +
								"Bmp Format (*.bmp)|*.bmp";

				if (saveDlg.ShowDialog() != DialogResult.OK)
					return;

				ImageFormat format = ImageFormat.Jpeg;
				if (saveDlg.FilterIndex == 2)
					format = ImageFormat.Gif;
				else if (saveDlg.FilterIndex == 3)
					format = ImageFormat.Png;
				else if (saveDlg.FilterIndex == 4)
					format = ImageFormat.Tiff;
				else if (saveDlg.FilterIndex == 5)
					format = ImageFormat.Bmp;

				Stream myStream = saveDlg.OpenFile();
				if (myStream != null)
				{
					dnsGraphMain.Image.Save(myStream, format);
					myStream.Close();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsmiSaveImageAs_Click(): " + ex.Message);
				throw;
			}
		}

		private void tsmiPrint_Click(object sender, EventArgs e)
		{
			try
			{
				printDlg.Document = printDocument;
				if (printDlg.ShowDialog() == DialogResult.OK)
				{
					printDocument.Print();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsmiPrint_Click(): " + ex.Message);
				throw;
			}
		}

		private void tsmiCopy_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetDataObject(this.dnsGraphMain.Image, true);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tsmiCopy_Click(): " + ex.Message);
				throw;
			}
		}

		private void printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			try
			{
				dnsGraphMain.fillGraphics(e.Graphics, dnsGraphMain.Width, dnsGraphMain.Height);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in printDocument_PrintPage(): " + ex.Message);
				throw;
			}
		}

		private void dnsGraph1_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Right)
				{
					ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings",
												this.GetType().Assembly);
					tsmiCopy.Text = rm.GetString("cm_copy");
					tsmiSaveImageAs.Text = rm.GetString("save_image_as");
					tsmiPrint.Text = rm.GetString("print_image");
					tsmiSetZoomDefault.Text = rm.GetString("default_zoom");
					cmGraphs.Show(dnsGraphMain.PointToScreen(new Point(e.X, e.Y)));
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in dnsGraph1_MouseClick(): " + ex.Message);
				throw;
			}
		}
	}
}