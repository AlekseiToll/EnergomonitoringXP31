using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EnergomonitoringXP
{
	public partial class frmAvgParams : Form
	{
		// максимальное число параметров при запросе архива усреденных
		const int maxNumAvgFieldsQuery_ = 2048; 

		public frmAvgParams()
		{
			InitializeComponent();
		}

		private void btnSelectAll_Click(object sender, EventArgs e)
		{
			ChangeSelection(true);
		}

		private void btnUnselectAll_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);
		}

		private void ChangeSelection(bool select)
		{
			if (tcMain.SelectedTab == tab3phase4wire)
			{
				for (int i = 0; i < tab3phase4wire.Controls.Count; ++i)
				{
					if (tab3phase4wire.Controls[i] is CheckBox)
					{
						(tab3phase4wire.Controls[i] as CheckBox).Checked = select;
					}
				}
				chbVC34Time.Checked = true;
			}
			if (tcMain.SelectedTab == tab3phase3wire)
			{
				for (int i = 0; i < tab3phase3wire.Controls.Count; ++i)
				{
					if (tab3phase3wire.Controls[i] is CheckBox)
					{
						(tab3phase3wire.Controls[i] as CheckBox).Checked = select;
					}
				}
				chbVC33Time.Checked = true;
			}
			if (tcMain.SelectedTab == tab1phase2wire)
			{
				for (int i = 0; i < tab1phase2wire.Controls.Count; ++i)
				{
					if (tab1phase2wire.Controls[i] is CheckBox)
					{
						(tab1phase2wire.Controls[i] as CheckBox).Checked = select;
					}
				}
				chbVC12Time.Checked = true;
			}
		}

		private void btnVC34_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			if (tcMain.SelectedTab == tab3phase4wire)
			{
				chbVC34F.Checked = true;
				chbVC34Ua.Checked = true;
				chbVC34Ub.Checked = true;
				chbVC34Uc.Checked = true;
				chbVC34Ia.Checked = true;
				chbVC34Ib.Checked = true;
				chbVC34Ic.Checked = true;
				chbVC34Uab.Checked = true;
				chbVC34Ubc.Checked = true;
				chbVC34Uca.Checked = true;
				chbVC34U1a.Checked = true;
				chbVC34U1b.Checked = true;
				chbVC34U1c.Checked = true;
				chbVC34U1ab.Checked = true;
				chbVC34U1bc.Checked = true;
				chbVC34U1ca.Checked = true;
				chbVC34I1a.Checked = true;
				chbVC34I1b.Checked = true;
				chbVC34I1c.Checked = true;
				chbVC34dU.Checked = true;
				chbVC34K2u.Checked = true;
				chbVC34K0u.Checked = true;
				chbVC34I1.Checked = true;
				chbVC34I2.Checked = true;
				chbVC34I0.Checked = true;
			}
		}

		private void btnPower34_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC34Kpa.Checked = true;
			chbVC34Kpb.Checked = true;
			chbVC34Kpc.Checked = true;
			chbVC34Kps.Checked = true;
			chbVC34Sa.Checked = true;
			chbVC34Sb.Checked = true;
			chbVC34Sc.Checked = true;
			chbVC34Ssum.Checked = true;
			chbVC34Pa.Checked = true;
			chbVC34Pb.Checked = true;
			chbVC34Pc.Checked = true;
			chbVC34Psum.Checked = true;
			chbVC34Qgeom_a.Checked = true;
			chbVC34Qgeom_b.Checked = true;
			chbVC34Qgeom_c.Checked = true;
			chbVC34Qgeom_sum.Checked = true;
			chbVC34Qcross1.Checked = true;
			chbVC34Qcross2.Checked = true;
			chbVC34Qcross3.Checked = true;
			chbVC34Qcross_s.Checked = true;
			chbVC34Qshift_a.Checked = true;
			chbVC34Qshift_b.Checked = true;
			chbVC34Qshift_c.Checked = true;
			chbVC34Qshift_s.Checked = true;
		}

		private void btnAngles34_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC34orUab.Checked = true;
			chbVC34orUbc.Checked = true;
			chbVC34orUca.Checked = true;
			chbVC34orUaIa.Checked = true;
			chbVC34orUbIb.Checked = true;
			chbVC34orUcIc.Checked = true;
		}

		private void btnPqp34_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC34dF.Checked = true;
			chbVC34dU.Checked = true;
			chbVC34K2u.Checked = true;
			chbVC34K0u.Checked = true;
			chbVC34I1.Checked = true;
			chbVC34I2.Checked = true;
			chbVC34I0.Checked = true;
			chbVC34Kua.Checked = true;
			chbVC34Kub.Checked = true;
			chbVC34Kuc.Checked = true;
			chbVC34Kia.Checked = true;
			chbVC34Kib.Checked = true;
			chbVC34Kic.Checked = true;
		}

		private void btnHarmonics34_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC34Kua.Checked = true;
			chbVC34Kub.Checked = true;
			chbVC34Kuc.Checked = true;
			chbVC34Kia.Checked = true;
			chbVC34Kib.Checked = true;
			chbVC34Kic.Checked = true;
			chbVC34U1a.Checked = true;
			chbVC34U1b.Checked = true;
			chbVC34U1c.Checked = true;
			chbVC34Kuan.Checked = true;
			chbVC34Kubn.Checked = true;
			chbVC34Kucn.Checked = true;
			chbVC34Kian.Checked = true;
			chbVC34Kibn.Checked = true;
			chbVC34Kicn.Checked = true;
		}

		private void btnVC33_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC33F.Checked = true;
			chbVC33Ia.Checked = true;
			chbVC33Ib.Checked = true;
			chbVC33Ic.Checked = true;
			chbVC33Uab.Checked = true;
			chbVC33Ubc.Checked = true;
			chbVC33Uca.Checked = true;
			chbVC33U1ab.Checked = true;
			chbVC33U1bc.Checked = true;
			chbVC33U1ca.Checked = true;
			chbVC33I1a.Checked = true;
			chbVC33I1b.Checked = true;
			chbVC33I1c.Checked = true;
			chbVC33dF.Checked = true;
			chbVC33dU.Checked = true;
			chbVC33K2u.Checked = true;
			chbVC33I1.Checked = true;
			chbVC33I2.Checked = true;
		}

		private void btnPower33_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC33Kps.Checked = true;
			chbVC33Ssum.Checked = true;
			chbVC33P1.Checked = true;
			chbVC33P2.Checked = true;
			chbVC33Psum.Checked = true;
			chbVC33Qgeom_sum.Checked = true;
			chbVC33Qcross1.Checked = true;
			chbVC33Qcross2.Checked = true;
			chbVC33Qcross3.Checked = true;
			chbVC33Qcross_s.Checked = true;
			chbVC33Qshift_s.Checked = true;
		}

		private void btnAngles33_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC33orUab.Checked = true;
			chbVC33orUbc.Checked = true;
			chbVC33orUca.Checked = true;
			chbVC33orUaIa.Checked = true;
			chbVC33orUbIb.Checked = true;
			chbVC33orUcIc.Checked = true;
		}

		private void btnPqp33_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC33dF.Checked = true;
			chbVC33dU.Checked = true;
			chbVC33K2u.Checked = true;
			chbVC33I1.Checked = true;
			chbVC33I2.Checked = true;
			chbVC33Kuab.Checked = true;
			chbVC33Kubc.Checked = true;
			chbVC33Kuca.Checked = true;
			chbVC33Kia.Checked = true;
			chbVC33Kib.Checked = true;
			chbVC33Kic.Checked = true;
		}

		private void btnHarmonics33_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC33Kuab.Checked = true;
			chbVC33Kubc.Checked = true;
			chbVC33Kuca.Checked = true;
			chbVC33Kia.Checked = true;
			chbVC33Kib.Checked = true;
			chbVC33Kic.Checked = true;
			chbVC33U1ab.Checked = true;
			chbVC33U1bc.Checked = true;
			chbVC33U1ca.Checked = true;
			chbVC33Kuabn.Checked = true;
			chbVC33Kubcn.Checked = true;
			chbVC33Kucan.Checked = true;
			chbVC33Kian.Checked = true;
			chbVC33Kibn.Checked = true;
			chbVC33Kicn.Checked = true;
		}

		private void btnVC12_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC12F.Checked = true;
			chbVC12U.Checked = true;
			chbVC12I.Checked = true;
			chbVC12U1.Checked = true;
			chbVC12I1.Checked = true;
			chbVC12dU.Checked = true;
		}

		private void btnPower12_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC12Kps.Checked = true;
			chbVC12S.Checked = true;
			chbVC12P.Checked = true;
			chbVC12Qgeom.Checked = true;
			chbVC12Qshift.Checked = true;
		}

		private void btnAngles12_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC12F.Checked = true;
			chbVC12U.Checked = true;
			chbVC12I.Checked = true;
			chbVC12U1.Checked = true;
			chbVC12I1.Checked = true;
			chbVC12orU1I1.Checked = true;
		}

		private void btnPqp12_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC12dF.Checked = true;
			chbVC12dU.Checked = true;
			chbVC12I1.Checked = true;
			chbVC12Ku.Checked = true;
			chbVC12Ki.Checked = true;
		}

		private void btnHarmonics12_Click(object sender, EventArgs e)
		{
			ChangeSelection(false);

			chbVC12Ku.Checked = true;
			chbVC12Ki.Checked = true;
			chbVC12U1.Checked = true;
			chbVC12Kun.Checked = true;
			chbVC12Kin.Checked = true;
		}

		public void GetParameters34(ref List<int> fields)
		{
			//int[] fields = new int[maxNumAvgFieldsQuery_];
			fields = new List<int>();

			if (this.chbVC34Kicn.Checked) ;
			if (this.chbVC34Kibn.Checked) ;
			if (this.chbVC34Kian.Checked) ;
			if (this.chbVC34Kucn.Checked) ;
			if (this.chbVC34Kubn.Checked) ;
			if (this.chbVC34Kuan.Checked) ;
			if (this.chbVC34Kic.Checked) ;
			if (this.chbVC34Kib.Checked) ;
			if (this.chbVC34Kia.Checked) ;
			if (this.chbVC34Kuc.Checked) ;
			if (this.chbVC34Kub.Checked) ;
			if (this.chbVC34Kua.Checked) ;
			if (this.chbVC34orUcIc.Checked) ;
			if (this.chbVC34orUbIb.Checked) ;
			if (this.chbVC34orUaIa.Checked) ;
			if (this.chbVC34orUca.Checked) ;
			if (this.chbVC34orUbc.Checked) ;
			if (this.chbVC34orUab.Checked) ;
			if (this.chbVC34Qshift_s.Checked) ;
			if (this.chbVC34Qshift_c.Checked) ;
			if (this.chbVC34Qshift_b.Checked) ;
			if (this.chbVC34Qshift_a.Checked) ;
			if (this.chbVC34Qcross_s.Checked) ;
			if (this.chbVC34Qcross3.Checked) ;
			if (this.chbVC34Qcross2.Checked) ;
			if (this.chbVC34Qcross1.Checked) ;
			if (this.chbVC34Qgeom_sum.Checked) ;
			if (this.chbVC34Qgeom_c.Checked) ;
			if (this.chbVC34Qgeom_b.Checked) ;
			if (this.chbVC34Qgeom_a.Checked) ;
			if (this.chbVC34Psum.Checked) ;
			if (this.chbVC34Pc.Checked) ;
			if (this.chbVC34Pb.Checked) ;
			if (this.chbVC34Pa.Checked) ;
			if (this.chbVC34Ssum.Checked) ;
			if (this.chbVC34Sc.Checked) ;
			if (this.chbVC34Sb.Checked) ;
			if (this.chbVC34Sa.Checked) ;
			if (this.chbVC34Kps.Checked) ;
			if (this.chbVC34Kpc.Checked) ;
			if (this.chbVC34Kpb.Checked) ;
			if (this.chbVC34Kpa.Checked) ;
			if (this.chbVC34I0.Checked) ;
			if (this.chbVC34I2.Checked) ;
			if (this.chbVC34I1.Checked) ;
			if (this.chbVC34K0u.Checked) ;
			if (this.chbVC34K2u.Checked) ;
			if (this.chbVC34dU.Checked) ;
			if (this.chbVC34dF.Checked) ;
			if (this.chbVC34I1c.Checked) ;
			if (this.chbVC34I1b.Checked) ;
			if (this.chbVC34I1a.Checked) ;
			if (this.chbVC34U1ca.Checked) ;
			if (this.chbVC34U1bc.Checked) ;
			if (this.chbVC34U1ab.Checked) ;
			if (this.chbVC34U1c.Checked) ;
			if (this.chbVC34U1b.Checked) ;
			if (this.chbVC34U1a.Checked) ;
			if (this.chbVC34Uca.Checked) ;
			if (this.chbVC34Ubc.Checked) ;
			if (this.chbVC34Uab.Checked) ;
			if (this.chbVC34Ic.Checked) ;
			if (this.chbVC34Ib.Checked) ;
			if (this.chbVC34Ia.Checked) ;
			if (this.chbVC34Uc.Checked) ;
			if (this.chbVC34Ub.Checked) ;
			if (this.chbVC34Ua.Checked) ;
			if (this.chbVC34F.Checked) ;

			// dummy
			fields.Add(32);
			fields.Add(33);

			return fields;
		}

		public int[] GetParameters33()
		{
			int[] fields = new int[maxNumAvgFieldsQuery_];

			//????????????????

			return fields;
		}

		public int[] GetParameters12()
		{
			int[] fields = new int[maxNumAvgFieldsQuery_];

			//????????????????

			return fields;
		}
	}
}