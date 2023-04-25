using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using EmServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmReportTemplates : Form
	{
		public frmReportTemplates()
		{
			InitializeComponent();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void OpenTemplate(string fileName)
		{
			try
			{
				System.Diagnostics.Process process = new System.Diagnostics.Process();
				process.StartInfo.FileName = "excel.exe";
				process.StartInfo.Arguments = String.Format("\"{0}\"", fileName);
				process.StartInfo.WorkingDirectory = "";
				process.StartInfo.UseShellExecute = true;
				process.StartInfo.CreateNoWindow = false;
				process.StartInfo.RedirectStandardOutput = false;
				process.Start();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in OpenTemplate():");
				throw;
			}
		}

		private void btnMM3ph4_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl3ph4wY.xml");
		}

		private void btnMM3ph3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl3ph3wY.xml");
		}

		private void btnMM1ph2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl1ph2wY.xml");
		}

		private void btnNMM3ph4_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl3ph4wN.xml");
		}

		private void btnNMM3ph3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl3ph3wN.xml");
		}

		private void btnNMM1ph2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl1ph2wN.xml");
		}

		private void btn53333_3ph4_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl3ph4wY_5333.xml");
		}

		private void btn53333_3ph3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl3ph3wY_5333.xml");
		}

		private void btn53333_1ph2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportTpl1ph2wY_5333.xml");
		}

		private void btnPqpA3ph4_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_3ph4w.xml");
		}

		private void btnPqpA3ph3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_3ph3w.xml");
		}

		private void btnPqpA1ph2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_1ph2w.xml");
		}

		private void frmReportTemplates_Load(object sender, EventArgs e)
		{
			string path = Application.StartupPath;
			labelPathOrigin.Text = path + "\\templates_copy";
			labelPathMain.Text = path + "\\templates";
		}

		private void btnPqpA3ph4_v2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_3ph4w_v2.xml");
		}

		private void btnPqpA3ph3_v2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_3ph3w_v2.xml");
		}

		private void btnPqpA1ph2_v2_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_1ph2w_v2.xml");
		}

		private void btnPqpA3ph4_v3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_3ph4w_v3.xml");
		}

		private void btnPqpA3ph3_v3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_3ph3w_v3.xml");
		}

		private void btnPqpA1ph2_v3_Click(object sender, EventArgs e)
		{
			OpenTemplate("templates\\PQPReportEtPQP_A_1ph2w_v3.xml");
		}
	}
}
