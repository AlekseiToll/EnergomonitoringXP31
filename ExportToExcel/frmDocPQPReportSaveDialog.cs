using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ExportToExcel
{
	public partial class frmDocPQPReportSaveDialog : Form
	{
		private bool enableNumber_;

		public frmDocPQPReportSaveDialog(System.Globalization.CultureInfo culture, bool enableNumber)
		{
			Thread.CurrentThread.CurrentUICulture = culture;
			InitializeComponent();

			enableNumber_ = enableNumber;
		}

		private void frmDocPQPReportSaveDialog_Shown(object sender, EventArgs e)
		{
			txtFileName.Focus();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (fd.ShowDialog(this) != DialogResult.OK) return;
			txtFileName.Text = fd.FileName;
		}

		private void txtFileName_TextChanged(object sender, EventArgs e)
		{
			btnSave.Enabled = txtFileName.Text.Length > 0;
		}

		private void frmDocPQPReportSaveDialog_Load(object sender, EventArgs e)
		{
			txtReportNumber.Enabled = enableNumber_;
			txtAppendixNumber.Enabled = enableNumber_;
		}
	}
}