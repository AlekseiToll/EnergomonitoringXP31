using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EmWorkNetLicences;

namespace EMWorkNetLicenceGenerator
{
	public partial class frmMain : Form
	{
		private Licences licences = new Licences();

		public frmMain()
		{
			InitializeComponent();
		}

		private void btnGenerateLicence_Click(object sender, EventArgs e)
		{
			if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
			if (listboxDeviceNumbers.Items.Count == 0) return;

			for (int i = 0; i < listboxDeviceNumbers.Items.Count; i++)
			{
				licences.AddLiñence(Convert.ToInt64(listboxDeviceNumbers.Items[i].ToString()));
			}
			licences.SaveLicences(saveFileDialog.FileName);
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			listboxDeviceNumbers.Items.Add(mtxtDeviceNumber.Text);
		}
	}
}