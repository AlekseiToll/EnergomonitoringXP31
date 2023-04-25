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
	public partial class frmPeakLoadTime : Form
	{
		public frmPeakLoadTime()
		{
			InitializeComponent();
		}

		public DateTime TimeMaxStart1
		{
			get { return dtpMaxStart1.Value; }
		}

		public DateTime TimeMaxEnd1
		{
			get { return dtpMaxEnd1.Value; }
		}

		public DateTime TimeMaxStart2
		{
			get { return dtpMaxStart2.Value; }
		}

		public DateTime TimeMaxEnd2
		{
			get { return dtpMaxEnd2.Value; }
		}

		public bool GetConstrNPLtopMaxMode(out float value)
		{
			value = -1;
			if (tbMaxNPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxNPLtop.Text, out value);
		}

		public bool GetConstrUPLtopMaxMode(out float value)
		{
			value = -1;
			if (tbMaxUPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxUPLtop.Text, out value);
		}

		public bool GetConstrNPLbottomMaxMode(out float value)
		{
			value = -1;
			if (tbMaxNPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxNPLbottom.Text, out value);
		}

		public bool GetConstrUPLbottomMaxMode(out float value)
		{
			value = -1;
			if (tbMaxUPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxUPLbottom.Text, out value);
		}

		public bool GetConstrNPLtopMinMode(out float value)
		{
			value = -1;
			if (tbMinNPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinNPLtop.Text, out value);
		}

		public bool GetConstrUPLtopMinMode(out float value)
		{
			value = -1;
			if (tbMinUPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinUPLtop.Text, out value);
		}

		public bool GetConstrNPLbottomMinMode(out float value)
		{
			value = -1;
			if (tbMinNPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinNPLbottom.Text, out value);
		}

		public bool GetConstrUPLbottomMinMode(out float value)
		{
			value = -1;
			if (tbMinUPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinUPLbottom.Text, out value);
		}

		private void tb_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',' && e.KeyChar != '\b')
				e.Handled = true;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{		
		}

		private void frmPeakLoadTime_FormClosing(object sender, FormClosingEventArgs e)
		{
			// проверяем чтобы НДП не было больше ПДП
			// (если больше, то делаем НДП = ПДП)
			float npl, upl;
			if (tbMaxNPLtop.Text.Length > 0 && tbMaxUPLtop.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMaxNPLtop.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMaxUPLtop.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMaxNPLtop.Text = tbMaxUPLtop.Text;
				}
			}
			if (tbMaxNPLbottom.Text.Length > 0 && tbMaxUPLbottom.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMaxNPLbottom.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMaxUPLbottom.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMaxNPLbottom.Text = tbMaxUPLbottom.Text;
				}
			}

			// теперь тоже самое для наименьших нагрузок
			if (tbMinNPLtop.Text.Length > 0 && tbMinUPLtop.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMinNPLtop.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMinUPLtop.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMinNPLtop.Text = tbMinUPLtop.Text;
				}
			}
			if (tbMinNPLbottom.Text.Length > 0 && tbMinUPLbottom.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMinNPLbottom.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMinUPLbottom.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMinNPLbottom.Text = tbMinUPLbottom.Text;
				}
			}
		}
	}
}
