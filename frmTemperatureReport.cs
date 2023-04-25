using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using EmServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmTemperatureReport : Form
	{
		private bool bNeedToCheckValues_ = false;

		public frmTemperatureReport()
		{
			InitializeComponent();
		}

		public short TemperatureMin
		{
			get
			{
				if(tbTmin.Text.Length > 0)
					return Int16.Parse(tbTmin.Text);
				return Int16.MaxValue;
			}
		}

		public short TemperatureMax
		{
			get
			{
				if (tbTmax.Text.Length > 0)
					return Int16.Parse(tbTmax.Text);
				return Int16.MaxValue;
			}
		}

		private void frmTemperatureReport_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!bNeedToCheckValues_) return;

			try
			{
				if (tbTmin.Text.Length == 0 || tbTmax.Text.Length == 0)
				{
					if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
						MessageBox.Show("Необходимо ввести значения температуры");
					else MessageBox.Show("Temperature values must be entered");

					e.Cancel = true;
				}

				short val;
				if (!Int16.TryParse(tbTmin.Text, out val) || !Int16.TryParse(tbTmax.Text, out val))
				{
					if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
						MessageBox.Show("Необходимо ввести корректные значения температуры");
					else MessageBox.Show("Temperature values are invalid");

					e.Cancel = true;
				}

				if (Int16.Parse(tbTmin.Text) > Int16.Parse(tbTmax.Text))
				{
					if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
						MessageBox.Show("Минимальное значение не может быть больше максимального");
					else MessageBox.Show("Minimum value mustn't be greater than maximum one");

					e.Cancel = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmTemperatureReport_FormClosing():");
				//throw;
			}
		}

		private void tbT_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				e.Handled = !(char.IsDigit(e.KeyChar) || e.KeyChar == '-' || e.KeyChar == '\b');
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tbT_KeyPress():");
				throw;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			bNeedToCheckValues_ = true;
		}

		private void btnIgnore_Click(object sender, EventArgs e)
		{
			bNeedToCheckValues_ = false;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			bNeedToCheckValues_ = false;
		}
	}
}
