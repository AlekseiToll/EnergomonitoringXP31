using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EmServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmAvgPages : Form
	{
		public frmAvgPages(bool[] pages)
		{
			InitializeComponent();

			if (pages.Length >= 12)
			{
				chbVolCur.Enabled = pages[(int)AvgPages.F_U_I];
				chbPower.Enabled = pages[(int)AvgPages.POWER];
				chbAngles.Enabled = pages[(int)AvgPages.ANGLES];
				chbPQP.Enabled = pages[(int)AvgPages.PQP];
				chbCurHarm.Enabled = pages[(int)AvgPages.I_HARMONICS];
				chbPhHarm.Enabled = pages[(int)AvgPages.U_PH_HARMONICS];
				chbLinHarm.Enabled = pages[(int)AvgPages.U_LIN_HARMONICS];
				chbHarmPowers.Enabled = pages[(int)AvgPages.HARMONIC_POWERS];
				chbHarmAngles.Enabled = pages[(int)AvgPages.HARMONIC_ANGLES];
				chbCurInterHarm.Enabled = pages[(int)AvgPages.I_INTERHARM];
				chbPhInterHarm.Enabled = pages[(int)AvgPages.U_PH_INTERHARM];
				chbLinInterHarm.Enabled = pages[(int)AvgPages.U_LIN_INTERHARM];
			}
			else EmService.WriteToLogFailed("pages.Length < 12!!!!");
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < gbPages.Controls.Count; ++i)
			{
				if (gbPages.Controls[i] is CheckBox)
				{
					if((gbPages.Controls[i] as CheckBox).Enabled)
						(gbPages.Controls[i] as CheckBox).Checked = true;
				}
			}
			btnOk.Enabled = true;
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < gbPages.Controls.Count; ++i)
			{
				if (gbPages.Controls[i] is CheckBox)
				{
					(gbPages.Controls[i] as CheckBox).Checked = false;
				}
			}
			btnOk.Enabled = false;
		}

		public bool[] PagesSelected
		{
			get
			{
				bool[] pages = new bool[12];
				pages[(int)AvgPages.F_U_I] = chbVolCur.Checked;
				pages[(int)AvgPages.POWER] = chbPower.Checked;
				pages[(int)AvgPages.ANGLES] = chbAngles.Checked;
				pages[(int)AvgPages.PQP] = chbPQP.Checked;
				pages[(int)AvgPages.I_HARMONICS] = chbCurHarm.Checked;
				pages[(int)AvgPages.U_PH_HARMONICS] = chbPhHarm.Checked;
				pages[(int)AvgPages.U_LIN_HARMONICS] = chbLinHarm.Checked;
				pages[(int)AvgPages.HARMONIC_POWERS] = chbHarmPowers.Checked;
				pages[(int)AvgPages.HARMONIC_ANGLES] = chbHarmAngles.Checked;
				pages[(int)AvgPages.I_INTERHARM] = chbCurInterHarm.Checked;
				pages[(int)AvgPages.U_PH_INTERHARM] = chbPhInterHarm.Checked;
				pages[(int)AvgPages.U_LIN_INTERHARM] = chbLinInterHarm.Checked;
				return pages;
			}
		}

		private void chb_CheckedChanged(object sender, EventArgs e)
		{
			bool selected = false;
			for (int i = 0; i < gbPages.Controls.Count; ++i)
			{
				if (gbPages.Controls[i] is CheckBox)
				{
					if ((gbPages.Controls[i] as CheckBox).Enabled &&
						(gbPages.Controls[i] as CheckBox).Checked)
					{
						selected = true;
						break;
					}
				}
			}
			btnOk.Enabled = selected;
		}
	}
}