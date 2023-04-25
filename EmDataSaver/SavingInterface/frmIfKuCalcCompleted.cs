using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EmDataSaver.SavingInterface
{
	public partial class frmIfKuCalcCompleted : Form
	{
		public bool bNotwait = false;

		public frmIfKuCalcCompleted()
		{
			InitializeComponent();
		}

		private void btnNotwait_Click(object sender, EventArgs e)
		{
			bNotwait = true;
		}

		public void SetProgressValue(int val)
		{
			progressBar.Value = val;
		}
	}
}