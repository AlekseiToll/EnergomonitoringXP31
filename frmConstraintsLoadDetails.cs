
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EnergomonitoringXP
{
	public partial class frmConstraintsLoadDetails : Form
	{
		EmDataSaver.Settings settings;

		public frmConstraintsLoadDetails(EmDataSaver.Settings settings, bool[] okImages)
		{
			InitializeComponent();

			for (int i = 0; i < 4; i++)
			{
				lvDetails.Items[i].ImageIndex = okImages[i] ? 0 : 1;
			}
			this.settings = settings;

			if (settings.CurDeviceType == EmServiceLib.EmDeviceType.ETPQP_A)
			{
				for (int i = 0; i < 4; i++)
				{
					lvDetails.Items[i].Text =
						lvDetails.Items[i].Text.Replace("13109-97", "32144-2013");
				}
			}
		}

		private void frmConstraintsLoadDetails_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				this.Close();
			}
		}
	}
}