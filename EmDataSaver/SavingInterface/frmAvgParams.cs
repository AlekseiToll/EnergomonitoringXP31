using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

using Microsoft.Win32;

using EmServiceLib;

namespace EmDataSaver.SavingInterface
{
	public partial class frmAvgParams : Form
	{
		// максимальное число параметров при запросе архива усреденных
		const int maxNumAvgFieldsQuery_ = 2048;

		// схема подключения
		ConnectScheme conScheme_;

		EmDeviceType devType_;

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="conScheme">Connection Scheme</param>
		/// <param name="devType">Device Type</param>
		/// <param name="restoreDefault">If it's necessary to restore default values </param>
		public frmAvgParams(ConnectScheme conScheme, EmDeviceType devType, bool restoreDefault)
		{
			devType_ = devType;
			conScheme_ = conScheme;

			InitializeComponent();

			btnSetAsDefault.Visible = (conScheme == ConnectScheme.Unknown);

			switch (conScheme)
			{
				case ConnectScheme.Unknown:  // форма вызвана из главного окна 
											// для выбора параметров по умолчанию
					tcMain.SelectedTab = tab3phase4wire;
					tab3phase3wire.Enabled = true;
					tab1phase2wire.Enabled = true;
					break;
				case ConnectScheme.Ph3W4:
				case ConnectScheme.Ph3W4_B_calc:
					tcMain.SelectedTab = tab3phase4wire;
					tab3phase3wire.Enabled = false;
					tab1phase2wire.Enabled = false;
					break;
				case ConnectScheme.Ph3W3:
				case ConnectScheme.Ph3W3_B_calc:
					tcMain.SelectedTab = tab3phase3wire;
					tab3phase4wire.Enabled = false;
					tab1phase2wire.Enabled = false;
					break;
				case ConnectScheme.Ph1W2: 
					tcMain.SelectedTab = tab1phase2wire;
					tab3phase4wire.Enabled = false;
					tab3phase3wire.Enabled = false;
					break;
				default: 
					tcMain.SelectedTab = tab3phase4wire;
					tab3phase3wire.Enabled = false;
					tab1phase2wire.Enabled = false;
					break;
			}

			if(restoreDefault) RestoreDefaultFromRegistry();
		}

		#endregion

		#region Private Methods

		// метрологи решили, что мощности гармоник надо убрать
		private void DisableHarmPower()
		{
			chbVC12Pn.Checked = false;
			chbVC33Pan.Checked = false;
			chbVC33Pbn.Checked = false;
			chbVC33Pcn.Checked = false;
			chbVC34Pan.Checked = false;
			chbVC34Pbn.Checked = false;
			chbVC34Pcn.Checked = false;
		}

		private void frmAvgParams_Load(object sender, EventArgs e)
		{
			DisableHarmPower();
		}

		private void btnSelectAll_Click(object sender, EventArgs e)
		{
			ChangeSelection(true, 0);
		}

		private void btnUnselectAll_Click(object sender, EventArgs e)
		{
			ChangeSelection(false, 0);
		}

		private void ChangeSelection(bool select, ConnectScheme conScheme)
		{
			try
			{
				switch (conScheme)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						for (int i = 0; i < tab3phase4wire.Controls.Count; ++i)
						{
							if (tab3phase4wire.Controls[i] is CheckBox)
							{
								(tab3phase4wire.Controls[i] as CheckBox).Checked = select;
							}
						}
						chbVC34Time.Checked = true;
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						for (int i = 0; i < tab3phase3wire.Controls.Count; ++i)
						{
							if (tab3phase3wire.Controls[i] is CheckBox)
							{
								(tab3phase3wire.Controls[i] as CheckBox).Checked = select;
							}
						}
						chbVC33Time.Checked = true;
						break;
					case ConnectScheme.Ph1W2:
						for (int i = 0; i < tab1phase2wire.Controls.Count; ++i)
						{
							if (tab1phase2wire.Controls[i] is CheckBox)
							{
								(tab1phase2wire.Controls[i] as CheckBox).Checked = select;
							}
						}
						chbVC12Time.Checked = true;
						break;
					case ConnectScheme.Unknown: // окно открыто из меню главного окна
						for (int i = 0; i < tab3phase4wire.Controls.Count; ++i)
						{
							if (tab3phase4wire.Controls[i] is CheckBox)
							{
								(tab3phase4wire.Controls[i] as CheckBox).Checked = select;
							}
						}
						chbVC34Time.Checked = true;
						for (int i = 0; i < tab3phase3wire.Controls.Count; ++i)
						{
							if (tab3phase3wire.Controls[i] is CheckBox)
							{
								(tab3phase3wire.Controls[i] as CheckBox).Checked = select;
							}
						}
						chbVC33Time.Checked = true;
						for (int i = 0; i < tab1phase2wire.Controls.Count; ++i)
						{
							if (tab1phase2wire.Controls[i] is CheckBox)
							{
								(tab1phase2wire.Controls[i] as CheckBox).Checked = select;
							}
						}
						chbVC12Time.Checked = true;
						break;
				}
				chbVC34TanP.Checked = true;
				chbVC34Qsum.Checked = true;
				chbVC34Psum.Checked = true;
				chbVC34Kps.Checked = true;
				chbVC33TanP.Checked = true;
				chbVC33Qsum.Checked = true;
				chbVC33Psum.Checked = true;
				chbVC33Kps.Checked = true;
				chbVC12TanP.Checked = true;
				chbVC12Q.Checked = true;
				chbVC12P.Checked = true;
				chbVC12Kps.Checked = true;

				DisableHarmPower();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ChangeSelection():");
				throw;
			}
		}

		private void btnSetAsDefault_Click(object sender, EventArgs e)
		{
			try
			{
				MaskAvgArray masks12 = GetMask12();
				MaskAvgArray masks33 = GetMask33();
				MaskAvgArray masks34 = GetMask34();

				const string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Mars-Energo\\EnergomonitoringXP\\DefaultAVGParams";
				Registry.SetValue(keyName, "MaskTable12_1_4_0", masks12.MaskTable_1_4[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_1_4_1", masks12.MaskTable_1_4[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_5_0", masks12.MaskTable_5[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_5_1", masks12.MaskTable_5[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_6a_0", masks12.MaskTable_6a[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_6a_1", masks12.MaskTable_6a[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_6b_0", masks12.MaskTable_6b[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable12_6b_1", masks12.MaskTable_6b[1].ToString(),
					RegistryValueKind.String);

				Registry.SetValue(keyName, "MaskTable33_1_4_0", masks33.MaskTable_1_4[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_1_4_1", masks33.MaskTable_1_4[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_5_0", masks33.MaskTable_5[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_5_1", masks33.MaskTable_5[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_6a_0", masks33.MaskTable_6a[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_6a_1", masks33.MaskTable_6a[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_6b_0", masks33.MaskTable_6b[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable33_6b_1", masks33.MaskTable_6b[1].ToString(),
					RegistryValueKind.String);

				Registry.SetValue(keyName, "MaskTable34_1_4_0", masks34.MaskTable_1_4[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_1_4_1", masks34.MaskTable_1_4[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_5_0", masks34.MaskTable_5[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_5_1", masks34.MaskTable_5[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_6a_0", masks34.MaskTable_6a[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_6a_1", masks34.MaskTable_6a[1].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_6b_0", masks34.MaskTable_6b[0].ToString(),
					RegistryValueKind.String);
				Registry.SetValue(keyName, "MaskTable34_6b_1", masks34.MaskTable_6b[1].ToString(),
					RegistryValueKind.String);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in btnSetAsDefault_Click():");
				throw;
			}
		}

		private void chb_Click(object sender, EventArgs e)
		{
			//if (chbVC12Kps.Checked)
			//{
			//    chbVC12P.Checked = true;
			//    chbVC12Q.Checked = true;
			//}
			//if (chbVC33Kps.Checked)
			//{
			//    chbVC33Psum.Checked = true;
			//    chbVC33Qsum.Checked = true;
			//}
			//if (chbVC34Kps.Checked)
			//{
			//    chbVC34Psum.Checked = true;
			//    chbVC34Qsum.Checked = true;
			//}
			if (chbVC34Kpa.Checked)
			{
				chbVC34Pa.Checked = true;
				chbVC34Qa.Checked = true;
			}
			if (chbVC34Kpb.Checked)
			{
				chbVC34Pb.Checked = true;
				chbVC34Qb.Checked = true;
			}
			if (chbVC34Kpc.Checked)
			{
				chbVC34Pc.Checked = true;
				chbVC34Qc.Checked = true;
			}

			//if (chbVC33Kpa.Checked)
			//{
			//    chbVC33Pa.Checked = true;
			//    chbVC33Qa.Checked = true;
			//}
			//if (chbVC33Kpb.Checked)
			//{
			//    chbVC33Pb.Checked = true;
			//    chbVC33Qb.Checked = true;
			//}
			//if (chbVC33Kpc.Checked)
			//{
			//    chbVC33Pc.Checked = true;
			//    chbVC33Qc.Checked = true;
			//}

			// for tangent P
			//if (chbVC34TanP.Checked)
			//{
			//    chbVC34Psum.Checked = true;
			//    chbVC34Qsum.Checked = true;
			//}
			//if (chbVC33TanP.Checked)
			//{
			//    chbVC33Psum.Checked = true;
			//    chbVC33Qsum.Checked = true;
			//}
			//if (chbVC12TanP.Checked)
			//{
			//    chbVC12P.Checked = true;
			//    chbVC12Q.Checked = true;
			//}
			//if (chbVC34Psum.Checked && chbVC34Qsum.Checked)
			//{
			//    chbVC34TanP.Checked = true;
			//}
			//else chbVC34TanP.Checked = false;
			//if (chbVC33Psum.Checked && chbVC33Qsum.Checked)
			//{
			//    chbVC33TanP.Checked = true;
			//}
			//else chbVC33TanP.Checked = false;
			//if (chbVC12P.Checked && chbVC12Q.Checked)
			//{
			//    chbVC12TanP.Checked = true;
			//}
			//else chbVC12TanP.Checked = false;

			///////////////////////////////////////////////////
			if (chbVC34Kian.Checked) chbVC34I1a.Checked = true;
			if (chbVC34Kibn.Checked) chbVC34I1b.Checked = true;
			if (chbVC34Kicn.Checked) chbVC34I1c.Checked = true;
			if (chbVC33Kian.Checked) chbVC33I1a.Checked = true;
			if (chbVC33Kibn.Checked) chbVC33I1b.Checked = true;
			if (chbVC33Kicn.Checked) chbVC33I1c.Checked = true;
			if (chbVC12Kin.Checked) chbVC12I1a.Checked = true;

			if (chbVC34Kuan.Checked) chbVC34U1a.Checked = true;
			if (chbVC34Kubn.Checked) chbVC34U1b.Checked = true;
			if (chbVC34Kucn.Checked) chbVC34U1c.Checked = true;
			if (chbVC12Kun.Checked) chbVC12U1.Checked = true;

			if (chbVC34Kuabn.Checked) chbVC34U1ab.Checked = true;
			if (chbVC34Kubcn.Checked) chbVC34U1bc.Checked = true;
			if (chbVC34Kucan.Checked) chbVC34U1ca.Checked = true;
			if (chbVC33Kuabn.Checked) chbVC33U1ab.Checked = true;
			if (chbVC33Kubcn.Checked) chbVC33U1bc.Checked = true;
			if (chbVC33Kucan.Checked) chbVC33U1ca.Checked = true;

			if (chbVC34Pan.Checked) chbVC34Pa.Checked = true;
			if (chbVC34Pbn.Checked) chbVC34Pb.Checked = true;
			if (chbVC34Pcn.Checked) chbVC34Pc.Checked = true;
			if (chbVC33Pan.Checked) chbVC33P1.Checked = true;
			if (chbVC33Pbn.Checked) chbVC33P2.Checked = true;
			//if (chbVC33Pan.Checked) chbVC33Pa.Checked = true;
			//if (chbVC33Pbn.Checked) chbVC33Pb.Checked = true;
			//if (chbVC33Pcn.Checked) chbVC33Pc.Checked = true;

			if (chbVC12Pn.Checked) chbVC12P.Checked = true;

			if (chbVC34Inn.Checked) chbVC34In.Checked = true;
		}

		private void btnVC34_Click(object sender, EventArgs e)
		{
			if (tcMain.SelectedTab == tab3phase4wire)
			{
				chbVC34F.Checked = true;
				chbVC34dF.Checked = true;
				chbVC34Ua.Checked = true;
				chbVC34Ub.Checked = true;
				chbVC34Uc.Checked = true;
				chbVC34Ua0.Checked = true;
				chbVC34Ub0.Checked = true;
				chbVC34Uc0.Checked = true;
				chbVC34Uab0.Checked = true;
				chbVC34Ubc0.Checked = true;
				chbVC34Uca0.Checked = true;
				chbVC34dUa.Checked = true;
				chbVC34dUb.Checked = true;
				chbVC34dUc.Checked = true;
				chbVC34dUab.Checked = true;
				chbVC34dUbc.Checked = true;
				chbVC34dUca.Checked = true;
				chbVC34U1.Checked = true;
				chbVC34U2.Checked = true;
				chbVC34U0.Checked = true;
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
			chbVC34Kpa.Checked = true;
			chbVC34Kpb.Checked = true;
			chbVC34Kpc.Checked = true;
			//chbVC34Kps.Checked = true;
			chbVC34Sa.Checked = true;
			chbVC34Sb.Checked = true;
			chbVC34Sc.Checked = true;
			chbVC34Ssum.Checked = true;
			chbVC34Pa.Checked = true;
			chbVC34Pb.Checked = true;
			chbVC34Pc.Checked = true;
			chbVC34P1.Checked = true;
			chbVC34P2.Checked = true;
			chbVC34P0.Checked = true;
			//chbVC34Psum.Checked = true;
			chbVC34Qa.Checked = true;
			chbVC34Qb.Checked = true;
			chbVC34Qc.Checked = true;
			//chbVC34Qsum.Checked = true;
			//chbVC34TanP.Checked = true;
		}

		private void btnAngles34_Click(object sender, EventArgs e)
		{
			chbVC34orUab.Checked = true;
			chbVC34orUbc.Checked = true;
			chbVC34orUca.Checked = true;
			chbVC34orUaIa.Checked = true;
			chbVC34orUbIb.Checked = true;
			chbVC34orUcIc.Checked = true;
			chbVC34UaIan.Checked = true;
			chbVC34UbIbn.Checked = true;
			chbVC34UcIcn.Checked = true;
			chbVC34UI1.Checked = true;
			chbVC34UI2.Checked = true;
			chbVC34UI0.Checked = true;
		}

		private void btnPqp34_Click(object sender, EventArgs e)
		{
			chbVC34dF.Checked = true;
			chbVC34dU.Checked = true;
			chbVC34K2u.Checked = true;
			chbVC34K0u.Checked = true;
			chbVC34Kua.Checked = true;
			chbVC34Kub.Checked = true;
			chbVC34Kuc.Checked = true;
			chbVC34Kuab.Checked = true;
			chbVC34Kubc.Checked = true;
			chbVC34Kuca.Checked = true;
			chbVC34Kia.Checked = true;
			chbVC34Kib.Checked = true;
			chbVC34Kic.Checked = true;
			chbVC34dUa.Checked = true;
			chbVC34dUb.Checked = true;
			chbVC34dUc.Checked = true;
			chbVC34dUab.Checked = true;
			chbVC34dUbc.Checked = true;
			chbVC34dUca.Checked = true;
		}

		private void btnHarmonics34_Click(object sender, EventArgs e)
		{
			chbVC34Kia.Checked = true;
			chbVC34Kib.Checked = true;
			chbVC34Kic.Checked = true;
			chbVC34I1a.Checked = true;
			chbVC34I1b.Checked = true;
			chbVC34I1c.Checked = true;
			chbVC34U1a.Checked = true;
			chbVC34U1b.Checked = true;
			chbVC34U1c.Checked = true;
			chbVC34U1ab.Checked = true;
			chbVC34U1bc.Checked = true;
			chbVC34U1ca.Checked = true;
			chbVC34Kua.Checked = true;
			chbVC34Kub.Checked = true;
			chbVC34Kuc.Checked = true;
			chbVC34Kuan.Checked = true;
			chbVC34Kubn.Checked = true;
			chbVC34Kucn.Checked = true;
			chbVC34Kian.Checked = true;
			chbVC34Kibn.Checked = true;
			chbVC34Kicn.Checked = true;
			chbVC34Kuab.Checked = true;
			chbVC34Kubc.Checked = true;
			chbVC34Kuca.Checked = true;
			chbVC34Kuabn.Checked = true;
			chbVC34Kubcn.Checked = true;
			chbVC34Kucan.Checked = true;
			//chbVC34Pan.Checked = true;
			//chbVC34Pbn.Checked = true;
			//chbVC34Pcn.Checked = true;
			chbVC34In.Checked = true;
			chbVC34Inn.Checked = true;
		}

		private void btnVC33_Click(object sender, EventArgs e)
		{
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
			chbVC33Uab0.Checked = true;
			chbVC33Ubc0.Checked = true;
			chbVC33Uca0.Checked = true;
			chbVC33dUab.Checked = true;
			chbVC33dUbc.Checked = true;
			chbVC33dUca.Checked = true;
			chbVC33U1.Checked = true;
			chbVC33U2.Checked = true;
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
			chbVC33Kps.Checked = true;
			chbVC33Ssum.Checked = true;
			chbVC33P1.Checked = true;
			chbVC33P2.Checked = true;
			chbVC33Pab.Checked = true;
			chbVC33Pcb.Checked = true;
			chbVC33Psum.Checked = true;
			chbVC33Qsum.Checked = true;
			chbVC33Qa.Checked = true;
			chbVC33Qb.Checked = true;
			chbVC33Qc.Checked = true;
			chbVC33TanP.Checked = true;
		}

		private void btnAngles33_Click(object sender, EventArgs e)
		{
			chbVC33orUab.Checked = true;
			chbVC33orUbc.Checked = true;
			chbVC33orUca.Checked = true;
			chbVC33orUaIa.Checked = true;
			chbVC33orUbIb.Checked = true;
			chbVC33orUcIc.Checked = true;
			chbVC33UaIan.Checked = true;
			chbVC33UbIbn.Checked = true;
			chbVC33UcIcn.Checked = true;
			chbVC33UI1.Checked = true;
			chbVC33UI2.Checked = true;
		}

		private void btnPqp33_Click(object sender, EventArgs e)
		{
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
			chbVC33dUab.Checked = true;
			chbVC33dUbc.Checked = true;
			chbVC33dUca.Checked = true;
		}

		private void btnHarmonics33_Click(object sender, EventArgs e)
		{
			chbVC33I1a.Checked = true;
			chbVC33I1b.Checked = true;
			chbVC33I1c.Checked = true;
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
			//chbVC33Pan.Checked = true;
			//chbVC33Pbn.Checked = true;
			//chbVC33Pcn.Checked = true;
		}

		private void btnVC12_Click(object sender, EventArgs e)
		{
			//ChangeSelection(false, ConnectScheme.Ph1W2);

			chbVC12F.Checked = true;
			chbVC12dF.Checked = true;
			chbVC12Ua.Checked = true;
			chbVC12U1a.Checked = true;
			chbVC12Ua0.Checked = true;
			chbVC12I.Checked = true;
			chbVC12I1.Checked = true;
			chbVC12U1.Checked = true;
			chbVC12I1a.Checked = true;
			chbVC12dU.Checked = true;
		}

		private void btnPower12_Click(object sender, EventArgs e)
		{
			//ChangeSelection(false, ConnectScheme.Ph1W2);

			chbVC12Kps.Checked = true;
			chbVC12S.Checked = true;
			chbVC12P.Checked = true;
			chbVC12P1.Checked = true;
			chbVC12Q.Checked = true;
			chbVC12TanP.Checked = true;
		}

		private void btnAngles12_Click(object sender, EventArgs e)
		{
			//ChangeSelection(false, ConnectScheme.Ph1W2);

			chbVC12UaIa.Checked = true;
			chbVC12UaIan.Checked = true;
			chbVC12orUI1.Checked = true;
		}

		private void btnPqp12_Click(object sender, EventArgs e)
		{
			//ChangeSelection(false, ConnectScheme.Ph1W2);

			chbVC12dF.Checked = true;
			chbVC12dU.Checked = true;
			chbVC12Ku.Checked = true;
			chbVC12Ki.Checked = true;
		}

		private void btnHarmonics12_Click(object sender, EventArgs e)
		{
			//ChangeSelection(false, ConnectScheme.Ph1W2);

			chbVC12Ku.Checked = true;
			chbVC12Ki.Checked = true;
			chbVC12U1a.Checked = true;
			chbVC12I1a.Checked = true;
			chbVC12Kun.Checked = true;
			chbVC12Kin.Checked = true;
			//chbVC12Pn.Checked = true;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (btnSetAsDefault.Visible)
				btnSetAsDefault_Click(sender, e);
		}

		private bool IfAllParamsSelected34()
		{
			for (int i = 0; i < tab3phase4wire.Controls.Count; ++i)
			{
				if (tab3phase4wire.Controls[i] is CheckBox)
				{
					if ((tab3phase4wire.Controls[i] as CheckBox).Checked == false)
						return false;
				}
			}
			return true;
		}

		private bool IfAllParamsSelected33()
		{
			for (int i = 0; i < tab3phase3wire.Controls.Count; ++i)
			{
				if (tab3phase3wire.Controls[i] is CheckBox)
				{
					if ((tab3phase3wire.Controls[i] as CheckBox).Checked == false)
						return false;
				}
			}
			return true;
		}

		private bool IfAllParamsSelected12()
		{
			for (int i = 0; i < tab1phase2wire.Controls.Count; ++i)
			{
				if (tab1phase2wire.Controls[i] is CheckBox)
				{
					if ((tab1phase2wire.Controls[i] as CheckBox).Checked == false)
						return false;
				}
			}
			return true;
		}

		private static void GetParameters34(out List<ushort> fields, ref MaskAvgArray mask, 
											EmDeviceType devType)
		{
			try
			{
				//если выбраны все параметры, то будем делать обычный непараметризированный запрос
				if (mask.IsEmpty)
				{
					fields = null;
					return;
				}

				fields = new List<ushort>();

				// AVG type
				fields.Add(0);
				// id EtPQP
				fields.Add(3);
				// limit
				fields.Add(4);
				// номиналы
				fields.Add(10); fields.Add(11);
				fields.Add(12); fields.Add(13);
				fields.Add(14); fields.Add(15);
				fields.Add(16); fields.Add(17);

				// Ток нейтрали
				if ((mask.MaskTable_5[0] & 0x000000001000) != 0)	// In(n)
				{
					// если не равно нулю, значит 1ая гармоника уже добавлена (вместе с In)
					if ((mask.MaskTable_1_4[0] & 0x080000000000) == 0)
						fields.Add((ushort)(1922)); fields.Add((ushort)(1922 + 1));	//In(1)

					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(1924 + i)); fields.Add((ushort)(1924 + i + 1));
					}
				}

				//Коэффициенты гармоник тока
				if ((mask.MaskTable_5[0] & 0x000000000020) != 0)	// Kicn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(1324 + i)); fields.Add((ushort)(1324 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000010) != 0)	// Kibn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(812 + i)); fields.Add((ushort)(812 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000008) != 0)	// Kian
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(300 + i)); fields.Add((ushort)(300 + i + 1));
					}
				}
				// Коэффициенты гармоник линейного напряжения
				if ((mask.MaskTable_5[0] & 0x000000000800) != 0)	// Kucan
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(1244 + i)); fields.Add((ushort)(1244 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000200) != 0)	// Kubcn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(732 + i)); fields.Add((ushort)(732 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000080) != 0)	// Kuabn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(220 + i)); fields.Add((ushort)(220 + i + 1));
					}
				}
				// Коэффициенты гармоник фазного напряжения
				if ((mask.MaskTable_5[0] & 0x000000000004) != 0)	// Kucn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(1164 + i)); fields.Add((ushort)(1164 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000002) != 0)	// Kubn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(652 + i)); fields.Add((ushort)(652 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000001) != 0)	// Kuan
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(140 + i)); fields.Add((ushort)(140 + i + 1));
					}
				}
				// Мощности гармоник
				if ((mask.MaskTable_6a[0] & 0x000000000001) != 0)	// Pan
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(378 + i)); fields.Add((ushort)(378 + i + 1));
					}
				}
				if ((mask.MaskTable_6a[0] & 0x000000000002) != 0)	// Pbn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(890 + i)); fields.Add((ushort)(890 + i + 1));
					}
				}
				if ((mask.MaskTable_6a[0] & 0x000000000004) != 0)	// Pcn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(1402 + i)); fields.Add((ushort)(1402 + i + 1));
					}
				}
				// Углы мощностей гармоник
				if ((mask.MaskTable_6b[0] & 0x000000000001) != 0)	// UaIan
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(458 + i)); fields.Add((ushort)(458 + i + 1));
					}
				}
				if ((mask.MaskTable_6b[0] & 0x000000000002) != 0)	// UbIbn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(970 + i)); fields.Add((ushort)(970 + i + 1));
					}
				}
				if ((mask.MaskTable_6b[0] & 0x000000000004) != 0)	// UcIcn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(1482 + i)); fields.Add((ushort)(1482 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000400) != 0)		// Kuca
					{ fields.Add(1242); fields.Add(1243); }
				if ((mask.MaskTable_5[0] & 0x000000000100) != 0)		// Kubc
					{ fields.Add(730); fields.Add(731); }
				if ((mask.MaskTable_5[0] & 0x000000000040) != 0)		// Kuab
					{ fields.Add(218); fields.Add(219); }

				if ((mask.MaskTable_1_4[0] & 0x080000000000) != 0)		// In, In(1) 
				{ fields.Add(1916); fields.Add(1917); 
				  fields.Add(1922); fields.Add(1923); }

				if ((mask.MaskTable_1_4[1] & 0x080000000000) != 0)		// Kic 
					{ fields.Add(1322); fields.Add(1323); }
				if ((mask.MaskTable_1_4[1] & 0x040000000000) != 0)		// Kib
					{ fields.Add(810); fields.Add(811); }
				if ((mask.MaskTable_1_4[1] & 0x020000000000) != 0)		// Kia
					{ fields.Add(298); fields.Add(299); }
				if ((mask.MaskTable_1_4[1] & 0x010000000000) != 0)		// Kuc
					{ fields.Add(1162); fields.Add(1163); }
				if ((mask.MaskTable_1_4[1] & 0x008000000000) != 0)		// Kub
					{ fields.Add(650); fields.Add(651); }
				if ((mask.MaskTable_1_4[1] & 0x004000000000) != 0)		// Kua
					{ fields.Add(138); fields.Add(139); }
				if ((mask.MaskTable_1_4[1] & 0x002000000000) != 0)		// K0u 
					{ fields.Add(46); fields.Add(47); }
				if ((mask.MaskTable_1_4[1] & 0x001000000000) != 0)		// K2u 
					{ fields.Add(44); fields.Add(45); }
				if ((mask.MaskTable_1_4[1] & 0x000800000000) != 0)		// dUca 
					{ fields.Add(1138); fields.Add(1139); }
				if ((mask.MaskTable_1_4[1] & 0x000400000000) != 0)		// dUbc 
					{ fields.Add(626); fields.Add(627); }
				if ((mask.MaskTable_1_4[1] & 0x000200000000) != 0)		// dUab 
					{ fields.Add(114); fields.Add(115); }
				if ((mask.MaskTable_1_4[1] & 0x000100000000) != 0)		// dUc 
					{ fields.Add(1128); fields.Add(1129); }
				if ((mask.MaskTable_1_4[1] & 0x000080000000) != 0)		// dUb 
					{ fields.Add(616); fields.Add(617); }
				if ((mask.MaskTable_1_4[1] & 0x000040000000) != 0)		// dUa 
					{ fields.Add(104); fields.Add(105); }
				if ((mask.MaskTable_1_4[1] & 0x000020000000) != 0)		// dU 
					{ fields.Add(42); fields.Add(43); }
				if ((mask.MaskTable_1_4[1] & 0x000010000000) != 0)		// dF 
					{ fields.Add(34); fields.Add(35); }
				if ((mask.MaskTable_1_4[1] & 0x000008000000) != 0)		// P2 
					{ fields.Add(56); fields.Add(57); }
				if ((mask.MaskTable_1_4[1] & 0x000004000000) != 0)		// P1 
					{ fields.Add(54); fields.Add(55); }
				if ((mask.MaskTable_1_4[1] & 0x000002000000) != 0)		// P0 
					{ fields.Add(58); fields.Add(59); }
				if ((mask.MaskTable_1_4[1] & 0x000001000000) != 0)		// UI0 
					{ fields.Add(64); fields.Add(65); }
				if ((mask.MaskTable_1_4[1] & 0x000000800000) != 0)		// UI2 
					{ fields.Add(62); fields.Add(63); }
				if ((mask.MaskTable_1_4[1] & 0x000000400000) != 0)		// UI1 
					{ fields.Add(60); fields.Add(61); }
				if ((mask.MaskTable_1_4[1] & 0x000000200000) != 0)		// orUcIc
					{ fields.Add(1150); fields.Add(1151); }
				if ((mask.MaskTable_1_4[1] & 0x000000100000) != 0)		// orUbIb 
					{ fields.Add(638); fields.Add(639); }
				if ((mask.MaskTable_1_4[1] & 0x000000080000) != 0)		// orUaIa
					{ fields.Add(126); fields.Add(127); }
				if ((mask.MaskTable_1_4[1] & 0x000000040000) != 0)		// orUca
					{ fields.Add(1152); fields.Add(1153); }
				if ((mask.MaskTable_1_4[1] & 0x000000020000) != 0)		// orUbc 
					{ fields.Add(640); fields.Add(641); }
				if ((mask.MaskTable_1_4[1] & 0x000000010000) != 0)		// orUab 
					{ fields.Add(128); fields.Add(129); }
				if ((mask.MaskTable_1_4[1] & 0x000000001000) != 0)		// Kps 
					{ fields.Add(72); fields.Add(73); }
				if ((mask.MaskTable_1_4[1] & 0x000000008000) != 0)		// Kpc
					{ fields.Add(1160); fields.Add(1161); }
				if ((mask.MaskTable_1_4[1] & 0x000000004000) != 0)		// Kpb
					{ fields.Add(648); fields.Add(649); }
				if ((mask.MaskTable_1_4[1] & 0x000000002000) != 0)		// Kpa
					{ fields.Add(136); fields.Add(137); }

				if (devType == EmDeviceType.EM32)
				{
					if ((mask.MaskTable_1_4[1] & 0x000000000100) != 0)		// Qsum
					{ fields.Add(68); fields.Add(69); fields.Add(5); }
					if ((mask.MaskTable_1_4[1] & 0x000000000800) != 0)		// Qc
					{ fields.Add(1156); fields.Add(1157); fields.Add(5); }
					if ((mask.MaskTable_1_4[1] & 0x000000000400) != 0)		// Qb
					{ fields.Add(644); fields.Add(645); fields.Add(5); }
					if ((mask.MaskTable_1_4[1] & 0x000000000200) != 0)		// Qa
					{ fields.Add(132); fields.Add(133); fields.Add(5); }
				}
				else
				{
					if ((mask.MaskTable_1_4[1] & 0x000000000100) != 0)		// Qsum
					{ 
						fields.Add(76); fields.Add(77); 
						fields.Add(78); fields.Add(79);
						fields.Add(80); fields.Add(81);
						fields.Add(82); fields.Add(83);
					}
					if ((mask.MaskTable_1_4[1] & 0x000000000800) != 0)		// Qc
					{
						fields.Add(1562); fields.Add(1563);
						fields.Add(1564); fields.Add(1565);
						fields.Add(1566); fields.Add(1567);
						fields.Add(1568); fields.Add(1569); 
					}
					if ((mask.MaskTable_1_4[1] & 0x000000000400) != 0)		// Qb
					{
						fields.Add(1050); fields.Add(1051);
						fields.Add(1052); fields.Add(1053);
						fields.Add(1054); fields.Add(1055);
						fields.Add(1056); fields.Add(1057); 
					}
					if ((mask.MaskTable_1_4[1] & 0x000000000200) != 0)		// Qa
					{
						fields.Add(538); fields.Add(539);
						fields.Add(540); fields.Add(541);
						fields.Add(542); fields.Add(543);
						fields.Add(544); fields.Add(545);
					}
				}

				if ((mask.MaskTable_1_4[1] & 0x000000000010) != 0)		// Ssum
					{ fields.Add(70); fields.Add(71); }
				if ((mask.MaskTable_1_4[1] & 0x000000000080) != 0)		// Sc
					{ fields.Add(1158); fields.Add(1159); }
				if ((mask.MaskTable_1_4[1] & 0x000000000040) != 0)		// Sb
					{ fields.Add(646); fields.Add(647); }
				if ((mask.MaskTable_1_4[1] & 0x000000000020) != 0)		// Sa
					{ fields.Add(134); fields.Add(135); }
				if ((mask.MaskTable_1_4[1] & 0x000000000001) != 0)		// Psum
					{ fields.Add(66); fields.Add(67); }
				if ((mask.MaskTable_1_4[1] & 0x000000000008) != 0)		// Pc
					{ fields.Add(1154); fields.Add(1155); }
				if ((mask.MaskTable_1_4[1] & 0x000000000004) != 0)		// Pb
					{ fields.Add(642); fields.Add(643); }
				if ((mask.MaskTable_1_4[1] & 0x000000000002) != 0)		// Pa
					{ fields.Add(130); fields.Add(131); }
				//////////////////////////////////////////////////////////////
				if ((mask.MaskTable_1_4[0] & 0x000040000000) != 0)		// I0 
					{ fields.Add(52); fields.Add(53); }
				if ((mask.MaskTable_1_4[0] & 0x000020000000) != 0)		// I2 
					{ fields.Add(50); fields.Add(51); }
				if ((mask.MaskTable_1_4[0] & 0x000010000000) != 0)		// I1 
					{ fields.Add(48); fields.Add(49); }
				if ((mask.MaskTable_1_4[0] & 0x000008000000) != 0)		// I1c 
					{ fields.Add(1142); fields.Add(1143); }
				if ((mask.MaskTable_1_4[0] & 0x000004000000) != 0)		// I1b 
					{ fields.Add(630); fields.Add(631); }
				if ((mask.MaskTable_1_4[0] & 0x000002000000) != 0)		// I1a 
					{ fields.Add(118); fields.Add(119); }
				if ((mask.MaskTable_1_4[0] & 0x000001000000) != 0)		// Ic 
					{ fields.Add(1140); fields.Add(1141); }
				if ((mask.MaskTable_1_4[0] & 0x000000800000) != 0)		// Ib 
					{ fields.Add(628); fields.Add(629); }
				if ((mask.MaskTable_1_4[0] & 0x000000400000) != 0)		// Ia 
					{ fields.Add(116); fields.Add(117); }
				if ((mask.MaskTable_1_4[0] & 0x000000200000) != 0)		// U0 
					{ fields.Add(40); fields.Add(41); }
				if ((mask.MaskTable_1_4[0] & 0x000000100000) != 0)		// U2 
					{ fields.Add(38); fields.Add(39); }
				if ((mask.MaskTable_1_4[0] & 0x000000080000) != 0)		// U1 
					{ fields.Add(36); fields.Add(37); }
				if ((mask.MaskTable_1_4[0] & 0x000000040000) != 0)		// Uca0 
					{ fields.Add(1134); fields.Add(1135); }
				if ((mask.MaskTable_1_4[0] & 0x000000020000) != 0)		// Ubc0 
					{ fields.Add(622); fields.Add(623); }
				if ((mask.MaskTable_1_4[0] & 0x000000010000) != 0)		// Uab0 
					{ fields.Add(110); fields.Add(111); }
				if ((mask.MaskTable_1_4[0] & 0x000000008000) != 0)		// Uc0 
					{ fields.Add(1124); fields.Add(1125); }
				if ((mask.MaskTable_1_4[0] & 0x000000004000) != 0)		// Ub0 
					{ fields.Add(612); fields.Add(613); }
				if ((mask.MaskTable_1_4[0] & 0x000000002000) != 0)		// Ua0 
					{ fields.Add(100); fields.Add(101); }
				if ((mask.MaskTable_1_4[0] & 0x000000001000) != 0)		// U1ca 
					{ fields.Add(1132); fields.Add(1133); }
				if ((mask.MaskTable_1_4[0] & 0x000000000800) != 0)		// U1bc 
					{ fields.Add(620); fields.Add(621); }
				if ((mask.MaskTable_1_4[0] & 0x000000000400) != 0)		// U1ab 
					{ fields.Add(108); fields.Add(109); }
				if ((mask.MaskTable_1_4[0] & 0x000000000200) != 0)		// Uca 
					{ fields.Add(1130); fields.Add(1131); }
				if ((mask.MaskTable_1_4[0] & 0x000000000100) != 0)		// Ubc 
					{ fields.Add(618); fields.Add(619); }
				if ((mask.MaskTable_1_4[0] & 0x000000000080) != 0)		// Uab 
					{ fields.Add(106); fields.Add(107); }
				if ((mask.MaskTable_1_4[0] & 0x000000000040) != 0)		// U1c 
					{ fields.Add(1122); fields.Add(1123); }
				if ((mask.MaskTable_1_4[0] & 0x000000000020) != 0)		// U1b 
					{ fields.Add(610); fields.Add(611); }
				if ((mask.MaskTable_1_4[0] & 0x000000000010) != 0)		// U1a 
					{ fields.Add(98); fields.Add(99); }
				if ((mask.MaskTable_1_4[0] & 0x000000000008) != 0)		// Uc 
					{ fields.Add(1120); fields.Add(1121); }
				if ((mask.MaskTable_1_4[0] & 0x000000000004) != 0)		// Ub 
					{ fields.Add(608); fields.Add(609); }
				if ((mask.MaskTable_1_4[0] & 0x000000000002) != 0)		// Ua 
					{ fields.Add(96); fields.Add(97); }
				if ((mask.MaskTable_1_4[0] & 0x000000000001) != 0)		// F 
					{ fields.Add(32); fields.Add(33); }

				fields.Sort();

				if (fields.Count == 0)
					fields = null;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetParameters34():");
				throw;
			}
		}

		private static void GetParameters33(out List<ushort> fields, ref MaskAvgArray mask,
											EmDeviceType devType)
		{
			try
			{
				//если выбраны все параметры, то будем делать обычный непараметризированный запрос
				if (mask.IsEmpty)
				{
					fields = null;
					return;
				}

				fields = new List<ushort>();

				// AVG type
				fields.Add(0);
				// id EtPQP
				fields.Add(3);
				// limit
				fields.Add(4);
				// номиналы
				fields.Add(10); fields.Add(11);
				fields.Add(12); fields.Add(13);
				fields.Add(14); fields.Add(15);
				fields.Add(16); fields.Add(17);

				//Коэффициенты гармоник тока
				if ((mask.MaskTable_5[0] & 0x000000000020) != 0)	// Kicn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(1324 + i)); fields.Add((ushort)(1324 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000010) != 0)	// Kibn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(812 + i)); fields.Add((ushort)(812 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000008) != 0)	// Kian
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(300 + i)); fields.Add((ushort)(300 + i + 1));
					}
				}

				// Коэффициенты гармоник линейного напряжения
				if ((mask.MaskTable_5[0] & 0x000000000800) != 0)	// Kucan
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(1244 + i)); fields.Add((ushort)(1244 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000200) != 0)	// Kubcn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(732 + i)); fields.Add((ushort)(732 + i + 1));
					}
				}
				if ((mask.MaskTable_5[0] & 0x000000000080) != 0)	// Kuabn
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(220 + i)); fields.Add((ushort)(220 + i + 1));
					}
				}

				// Мощности гармоник
				if ((mask.MaskTable_6a[0] & 0x000000000001) != 0)	// Pan
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(1676 + i)); fields.Add((ushort)(1676 + i + 1));
					}
				}
				if ((mask.MaskTable_6a[0] & 0x000000000002) != 0)	// Pbn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(1756 + i)); fields.Add((ushort)(1756 + i + 1));
					}
				}
				//if ((mask.MaskTable_6a[0] & 0x000000000004) != 0)	// Pcn
				//{
				//    for (ushort i = 0; i < 80; i += 2)
				//    {
				//        fields.Add((ushort)(1402 + i)); fields.Add((ushort)(1402 + i + 1));
				//    }
				//}

				// Углы мощностей гармоник
				if ((mask.MaskTable_6b[0] & 0x000000000001) != 0)	// UaIan
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(458 + i)); fields.Add((ushort)(458 + i + 1));
					}
				}
				if ((mask.MaskTable_6b[0] & 0x000000000002) != 0)	// UbIbn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(970 + i)); fields.Add((ushort)(970 + i + 1));
					}
				}
				if ((mask.MaskTable_6b[0] & 0x000000000004) != 0)	// UcIcn
				{
					for (ushort i = 0; i < 80; i += 2)
					{
						fields.Add((ushort)(1482 + i)); fields.Add((ushort)(1482 + i + 1));
					}
				}
				
				if ((mask.MaskTable_1_4[1] & 0x080000000000) != 0)		// Kic 
					{ fields.Add(1322); fields.Add(1323); }
				if ((mask.MaskTable_1_4[1] & 0x040000000000) != 0)		// Kib
					{ fields.Add(810); fields.Add(811); }
				if ((mask.MaskTable_1_4[1] & 0x020000000000) != 0)		// Kia
					{ fields.Add(298); fields.Add(299); }
				if ((mask.MaskTable_5[0] & 0x000000000400) != 0)		// Kuca
					{ fields.Add(1242); fields.Add(1243); }
				if ((mask.MaskTable_5[0] & 0x000000000100) != 0)		// Kubc
					{ fields.Add(730); fields.Add(731); }
				if ((mask.MaskTable_5[0] & 0x000000000040) != 0)		// Kuab
					{ fields.Add(218); fields.Add(219); }
				if ((mask.MaskTable_1_4[1] & 0x001000000000) != 0)		// K2u 
					{ fields.Add(44); fields.Add(45); }
				if ((mask.MaskTable_1_4[1] & 0x000800000000) != 0)		// dUca 
					{ fields.Add(1138); fields.Add(1139); }
				if ((mask.MaskTable_1_4[1] & 0x000400000000) != 0)		// dUbc 
					{ fields.Add(626); fields.Add(627); }
				if ((mask.MaskTable_1_4[1] & 0x000200000000) != 0)		// dUab 
					{ fields.Add(114); fields.Add(115); }
				if ((mask.MaskTable_1_4[1] & 0x000020000000) != 0)		// dU 
					{ fields.Add(42); fields.Add(43); }
				if ((mask.MaskTable_1_4[1] & 0x000010000000) != 0)		// dF 
					{ fields.Add(34); fields.Add(35); }
				if ((mask.MaskTable_1_4[1] & 0x000008000000) != 0)		// P2 
					{ fields.Add(56); fields.Add(57); }
				if ((mask.MaskTable_1_4[1] & 0x000004000000) != 0)		// P1 
					{ fields.Add(54); fields.Add(55); }
				if ((mask.MaskTable_1_4[1] & 0x000000800000) != 0)		// UI2 
					{ fields.Add(62); fields.Add(63); }
				if ((mask.MaskTable_1_4[1] & 0x000000400000) != 0)		// UI1 
					{ fields.Add(60); fields.Add(61); }
				if ((mask.MaskTable_1_4[1] & 0x000000200000) != 0)		// orUcIc
					{ fields.Add(1150); fields.Add(1151); }
				if ((mask.MaskTable_1_4[1] & 0x000000100000) != 0)		// orUbIb 
					{ fields.Add(638); fields.Add(639); }
				if ((mask.MaskTable_1_4[1] & 0x000000080000) != 0)		// orUaIa
					{ fields.Add(126); fields.Add(127); }
				if ((mask.MaskTable_1_4[1] & 0x000000040000) != 0)		// orUca
					{ fields.Add(1152); fields.Add(1153); }
				if ((mask.MaskTable_1_4[1] & 0x000000020000) != 0)		// orUbc 
					{ fields.Add(640); fields.Add(641); }
				if ((mask.MaskTable_1_4[1] & 0x000000010000) != 0)		// orUab 
					{ fields.Add(128); fields.Add(129); }
				if ((mask.MaskTable_1_4[1] & 0x000000001000) != 0)		// Kps 
					{ fields.Add(72); fields.Add(73); }
				if (devType == EmDeviceType.EM32)
				{
					if ((mask.MaskTable_1_4[1] & 0x000000000100) != 0)		// Qsum
					{ fields.Add(68); fields.Add(69); fields.Add(5); }
					if ((mask.MaskTable_1_4[1] & 0x000000000800) != 0)		// Qc
					{ fields.Add(1156); fields.Add(1157); fields.Add(5); }
					if ((mask.MaskTable_1_4[1] & 0x000000000400) != 0)		// Qb
					{ fields.Add(644); fields.Add(645); fields.Add(5); }
					if ((mask.MaskTable_1_4[1] & 0x000000000200) != 0)		// Qa
					{ fields.Add(132); fields.Add(133); fields.Add(5); }
				}
				else
				{
					if ((mask.MaskTable_1_4[1] & 0x000000000100) != 0)		// Qsum
					{
						fields.Add(76); fields.Add(77);
						fields.Add(78); fields.Add(79);
						fields.Add(80); fields.Add(81);
						fields.Add(82); fields.Add(83);
					}
					if ((mask.MaskTable_1_4[1] & 0x000000000800) != 0)		// Qc
					{
						fields.Add(1562); fields.Add(1563);
						fields.Add(1564); fields.Add(1565);
						fields.Add(1566); fields.Add(1567);
						fields.Add(1568); fields.Add(1569);
					}
					if ((mask.MaskTable_1_4[1] & 0x000000000400) != 0)		// Qb
					{
						fields.Add(1050); fields.Add(1051);
						fields.Add(1052); fields.Add(1053);
						fields.Add(1054); fields.Add(1055);
						fields.Add(1056); fields.Add(1057);
					}
					if ((mask.MaskTable_1_4[1] & 0x000000000200) != 0)		// Qa
					{
						fields.Add(538); fields.Add(539);
						fields.Add(540); fields.Add(541);
						fields.Add(542); fields.Add(543);
						fields.Add(544); fields.Add(545);
					}
				}
				if ((mask.MaskTable_1_4[1] & 0x000000000010) != 0)		// Ssum
					{ fields.Add(70); fields.Add(71); }
				if ((mask.MaskTable_1_4[1] & 0x000000000001) != 0)		// Psum
					{ fields.Add(74); fields.Add(75); }
				if ((mask.MaskTable_1_4[1] & 0x000000000004) != 0)		// Pcb
					{ fields.Add(1642); fields.Add(1643); }
				if ((mask.MaskTable_1_4[1] & 0x000000000002) != 0)		// Pab
					{ fields.Add(1640); fields.Add(1641); }
				/////////////////////////////////////////////////////////////
				if ((mask.MaskTable_1_4[0] & 0x000020000000) != 0)		// I2 
					{ fields.Add(50); fields.Add(51); }
				if ((mask.MaskTable_1_4[0] & 0x000010000000) != 0)		// I1 
					{ fields.Add(48); fields.Add(49); }
				if ((mask.MaskTable_1_4[0] & 0x000008000000) != 0)		// I1c 
					{ fields.Add(1142); fields.Add(1143); }
				if ((mask.MaskTable_1_4[0] & 0x000004000000) != 0)		// I1b 
					{ fields.Add(630); fields.Add(631); }
				if ((mask.MaskTable_1_4[0] & 0x000002000000) != 0)		// I1a 
					{ fields.Add(118); fields.Add(119); }
				if ((mask.MaskTable_1_4[0] & 0x000001000000) != 0)		// Ic 
					{ fields.Add(1140); fields.Add(1141); }
				if ((mask.MaskTable_1_4[0] & 0x000000800000) != 0)		// Ib 
					{ fields.Add(628); fields.Add(629); }
				if ((mask.MaskTable_1_4[0] & 0x000000400000) != 0)		// Ia 
					{ fields.Add(116); fields.Add(117); }
				if ((mask.MaskTable_1_4[0] & 0x000000100000) != 0)		// U2 
					{ fields.Add(38); fields.Add(39); }
				if ((mask.MaskTable_1_4[0] & 0x000000080000) != 0)		// U1 
					{ fields.Add(36); fields.Add(37); }
				if ((mask.MaskTable_1_4[0] & 0x000000040000) != 0)		// Uca0 
					{ fields.Add(1134); fields.Add(1135); }
				if ((mask.MaskTable_1_4[0] & 0x000000020000) != 0)		// Ubc0 
					{ fields.Add(622); fields.Add(623); }
				if ((mask.MaskTable_1_4[0] & 0x000000010000) != 0)		// Uab0 
					{ fields.Add(110); fields.Add(111); }
				if ((mask.MaskTable_1_4[0] & 0x000000001000) != 0)		// U1ca 
					{ fields.Add(1132); fields.Add(1133); }
				if ((mask.MaskTable_1_4[0] & 0x000000000800) != 0)		// U1bc 
					{ fields.Add(620); fields.Add(621); }
				if ((mask.MaskTable_1_4[0] & 0x000000000400) != 0)		// U1ab 
					{ fields.Add(108); fields.Add(109); }
				if ((mask.MaskTable_1_4[0] & 0x000000000200) != 0)		// Uca 
					{ fields.Add(1130); fields.Add(1131); }
				if ((mask.MaskTable_1_4[0] & 0x000000000100) != 0)		// Ubc 
					{ fields.Add(618); fields.Add(619); }
				if ((mask.MaskTable_1_4[0] & 0x000000000080) != 0)		// Uab 
					{ fields.Add(106); fields.Add(107); }
				if ((mask.MaskTable_1_4[0] & 0x000000000001) != 0)		// F 
					{ fields.Add(32); fields.Add(33); }

				fields.Sort();

				if (fields.Count == 0)
					fields = null;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetParameters33():");
				throw;
			}
		}

		private static void GetParameters12(out List<ushort> fields, ref MaskAvgArray mask,
											EmDeviceType devType)
		{
			try
			{
				//если выбраны все параметры, то будем делать обычный непараметризированный запрос
				if (mask.IsEmpty)
				{
					fields = null;
					return;
				}

				fields = new List<ushort>();

				// AVG type
				fields.Add(0);
				// id EtPQP
				fields.Add(3);
				// limit
				fields.Add(4);
				// номиналы
				fields.Add(10); fields.Add(11);
				fields.Add(12); fields.Add(13);
				fields.Add(14); fields.Add(15);
				fields.Add(16); fields.Add(17);

				//Коэффициенты гармоник тока
				if ((mask.MaskTable_5[0] & 0x000000000008) != 0)	// Kin
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(300 + i)); fields.Add((ushort)(300 + i + 1));
					}
				}

				// Коэффициенты гармоник фазного напряжения
				if ((mask.MaskTable_5[0] & 0x000000000001) != 0)	// Kun
				{
					for (ushort i = 0; i < 78; i += 2)
					{
						fields.Add((ushort)(140 + i)); fields.Add((ushort)(140 + i + 1));
					}
				}
		
				if ((mask.MaskTable_1_4[1] & 0x020000000000) != 0)		// Ki
					{ fields.Add(298); fields.Add(299); }
				if ((mask.MaskTable_1_4[1] & 0x004000000000) != 0)		// Ku
					{ fields.Add(138); fields.Add(139); }
				if ((mask.MaskTable_1_4[1] & 0x000020000000) != 0)		// dU 
					{ fields.Add(104); fields.Add(105); }
				if ((mask.MaskTable_1_4[1] & 0x000010000000) != 0)		// dF 
					{ fields.Add(34); fields.Add(35); }
				if ((mask.MaskTable_1_4[1] & 0x000004000000) != 0)		// P1
					{ fields.Add(54); fields.Add(55); }
				if ((mask.MaskTable_1_4[1] & 0x000000400000) != 0)		// ^U1I1
					{ fields.Add(60); fields.Add(61); }
				if ((mask.MaskTable_1_4[1] & 0x000000080000) != 0)		// ^UaIa
					{ fields.Add(126); fields.Add(127); }
				if ((mask.MaskTable_1_4[1] & 0x000000001000) != 0)		// Kpa 
					{ fields.Add(136); fields.Add(137); }
				
				if (devType == EmDeviceType.EM32)
				{
					if ((mask.MaskTable_1_4[1] & 0x000000000100) != 0)		// Qa
					{ fields.Add(132); fields.Add(133); fields.Add(5); }
				}
				else
				{
					if ((mask.MaskTable_1_4[1] & 0x000000000100) != 0)		// Qsum
					{
						fields.Add(538); fields.Add(539);
						fields.Add(540); fields.Add(541);
						fields.Add(542); fields.Add(543);
						fields.Add(544); fields.Add(545);
					}
				}

				if ((mask.MaskTable_1_4[1] & 0x000000000001) != 0)		// Pa
					{ fields.Add(130); fields.Add(131); }
				if ((mask.MaskTable_1_4[1] & 0x000000000010) != 0)		// Sa
					{ fields.Add(134); fields.Add(135); }
				/////////////////////////////////////////////////////////////
				if ((mask.MaskTable_1_4[0] & 0x000010000000) != 0)		// I1 
					{ fields.Add(48); fields.Add(49); }
				if ((mask.MaskTable_1_4[0] & 0x000002000000) != 0)		// I1a 
					{ fields.Add(118); fields.Add(119); }
				if ((mask.MaskTable_1_4[0] & 0x000000400000) != 0)		// Ia 
					{ fields.Add(116); fields.Add(117); }
				if ((mask.MaskTable_1_4[0] & 0x000000080000) != 0)		// U1 
					{ fields.Add(36); fields.Add(37); }
				if ((mask.MaskTable_1_4[0] & 0x000000002000) != 0)		// Ua0 
					{ fields.Add(100); fields.Add(101); }
				if ((mask.MaskTable_1_4[0] & 0x000000000010) != 0)		// U1a 
					{ fields.Add(98); fields.Add(99); }	
				if ((mask.MaskTable_1_4[0] & 0x000000000002) != 0)		// Ua 
					{ fields.Add(96); fields.Add(97); }
				if ((mask.MaskTable_1_4[0] & 0x000000000001) != 0)		// F 
					{ fields.Add(32); fields.Add(33); }

				fields.Sort();

				if (fields.Count == 0)
					fields = null;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetParameters12():");
				throw;
			}
		}

		private MaskAvgArray GetMask34()
		{
			MaskAvgArray res = new MaskAvgArray();

			try
			{
				//if (IfAllParamsSelected34())
				//{
				//    return res;
				//}

				// table period_avg_params_1_4
				Int64 iMask = 0, iMask2 = 0;

				if (chbVC34F.Checked)		{ iMask |= 0x000000001; }		// 1 bit
				if (chbVC34Ua.Checked)		{ iMask |= 0x000000002; }		// 2 bit
				if (chbVC34Ub.Checked)		{ iMask |= 0x000000004; }		// 3 bit
				if (chbVC34Uc.Checked)		{ iMask |= 0x000000008; }		// 4 bit
				if (chbVC34U1a.Checked)		{ iMask |= 0x000000010; }		// 5 bit
				if (chbVC34U1b.Checked)		{ iMask |= 0x000000020; }		// 6 bit
				if (chbVC34U1c.Checked)		{ iMask |= 0x000000040; }		// 7 bit
				if (chbVC34Uab.Checked)		{ iMask |= 0x000000080; }		// 8 bit
				if (chbVC34Ubc.Checked)		{ iMask |= 0x000000100; }		// 9 bit
				if (chbVC34Uca.Checked)		{ iMask |= 0x000000200; }		// 10 bit
				if (chbVC34U1ab.Checked)	{ iMask |= 0x000000400; }		// 11 bit
				if (chbVC34U1bc.Checked)	{ iMask |= 0x000000800; }		// 12 bit
				if (chbVC34U1ca.Checked)	{ iMask |= 0x000001000; }		// 13 bit
				if (chbVC34Ua0.Checked)		{ iMask |= 0x000002000; }		// 14 bit		U0_A
				if (chbVC34Ub0.Checked)		{ iMask |= 0x000004000; }		// 15 bit		U0_B
				if (chbVC34Uc0.Checked)		{ iMask |= 0x000008000; }		// 16 bit		U0_C
				if (chbVC34Uab0.Checked)	{ iMask |= 0x000010000; }		// 17 bit
				if (chbVC34Ubc0.Checked)	{ iMask |= 0x000020000; }		// 18 bit
				if (chbVC34Uca0.Checked)	{ iMask |= 0x000040000; }		// 19 bit
				if (chbVC34U1.Checked)		{ iMask |= 0x000080000; }		// 20 bit		U_1
				if (chbVC34U2.Checked)		{ iMask |= 0x000100000; }		// 21 bit		U_2
				if (chbVC34U0.Checked)		{ iMask |= 0x000200000; }		// 22 bit		U_0
				if (chbVC34Ia.Checked)		{ iMask |= 0x000400000; }		// 23 bit
				if (chbVC34Ib.Checked)		{ iMask |= 0x000800000; }		// 24 bit
				if (chbVC34Ic.Checked)		{ iMask |= 0x001000000; }		// 25 bit
				if (chbVC34I1a.Checked)		{ iMask |= 0x002000000; }		// 26 bit
				if (chbVC34I1b.Checked)		{ iMask |= 0x004000000; }		// 27 bit
				if (chbVC34I1c.Checked)		{ iMask |= 0x008000000; }		// 28 bit
				if (chbVC34I1.Checked)		{ iMask |= 0x010000000; }		// 29 bit
				if (chbVC34I2.Checked)		{ iMask |= 0x020000000; }		// 30 bit
				if (chbVC34I0.Checked)		{ iMask |= 0x040000000; }		// 31 bit
				// биты 32 - 43 заняты!!! (см.ниже)
				if (chbVC34In.Checked)		{ iMask |= 0x080000000000; }	// 44 bit
				///////////////////////////////////////////////////////////////////////////
				if (chbVC34Psum.Checked)	{ iMask2 |= 0x000000000001; }		// 1 bit
				if (chbVC34Pa.Checked)		{ iMask2 |= 0x000000000002; }		// 2 bit
				if (chbVC34Pb.Checked)		{ iMask2 |= 0x000000000004; }		// 3 bit
				if (chbVC34Pc.Checked)		{ iMask2 |= 0x000000000008; }		// 4 bit
				if (chbVC34Ssum.Checked)	{ iMask2 |= 0x000000000010; }		// 5 bit
				if (chbVC34Sa.Checked)		{ iMask2 |= 0x000000000020; }		// 6 bit
				if (chbVC34Sb.Checked)		{ iMask2 |= 0x000000000040; }		// 7 bit
				if (chbVC34Sc.Checked)		{ iMask2 |= 0x000000000080; }		// 8 bit
				if (chbVC34Qsum.Checked)	
				{
					iMask |= 0x000080000000;		// 32 bit
					iMask |= 0x000800000000;		// 36 bit
					iMask |= 0x008000000000;		// 40 bit
					iMask2 |= 0x000000000100;		// 9 bit
				}
				if (chbVC34Qa.Checked)		
				{
					iMask |= 0x000100000000;		// 33 bit
					iMask |= 0x001000000000;		// 37 bit
					iMask |= 0x010000000000;		// 41 bit
					iMask2 |= 0x000000000200;		// 10 bit
				}
				if (chbVC34Qb.Checked)		
				{
					iMask |= 0x000200000000;		// 34 bit
					iMask |= 0x002000000000;		// 38 bit
					iMask |= 0x020000000000;		// 42 bit
					iMask2 |= 0x000000000400;		// 11 bit
				}
				if (chbVC34Qc.Checked)		
				{
					iMask |= 0x000400000000;		// 35 bit
					iMask |= 0x004000000000;		// 39 bit
					iMask |= 0x040000000000;		// 43 bit
					iMask2 |= 0x000000000800;		// 12 bit
				}		
				if (chbVC34Kps.Checked)		{ iMask2 |= 0x000000001000; }		// 13 bit
				if (chbVC34Kpa.Checked)		{ iMask2 |= 0x000000002000; }		// 14 bit
				if (chbVC34Kpb.Checked)		{ iMask2 |= 0x000000004000; }		// 15 bit
				if (chbVC34Kpc.Checked)		{ iMask2 |= 0x000000008000; }		// 16 bit
				if (chbVC34orUab.Checked)	{ iMask2 |= 0x000000010000; }		// 17 bit
				if (chbVC34orUbc.Checked)	{ iMask2 |= 0x000000020000; }		// 18 bit
				if (chbVC34orUca.Checked)	{ iMask2 |= 0x000000040000; }		// 19 bit
				if (chbVC34orUaIa.Checked)	{ iMask2 |= 0x000000080000; }		// 20 bit
				if (chbVC34orUbIb.Checked)	{ iMask2 |= 0x000000100000; }		// 21 bit
				if (chbVC34orUcIc.Checked)	{ iMask2 |= 0x000000200000; }		// 22 bit
				if (chbVC34UI1.Checked)		{ iMask2 |= 0x000000400000; }		// 23 bit
				if (chbVC34UI2.Checked)		{ iMask2 |= 0x000000800000; }		// 24 bit
				if (chbVC34UI0.Checked)		{ iMask2 |= 0x000001000000; }		// 25 bit
				if (chbVC34P0.Checked)		{ iMask2 |= 0x000002000000; }		// 26 bit
				if (chbVC34P1.Checked)		{ iMask2 |= 0x000004000000; }		// 27 bit
				if (chbVC34P2.Checked)		{ iMask2 |= 0x000008000000; }		// 28 bit
				if (chbVC34dF.Checked)		{ iMask2 |= 0x000010000000; }		// 29 bit
				if (chbVC34dU.Checked)		{ iMask2 |= 0x000020000000; }		// 30 bit		δU_y
				if (chbVC34dUa.Checked)		{ iMask2 |= 0x000040000000; }		// 31 bit		δU_A
				if (chbVC34dUb.Checked)		{ iMask2 |= 0x000080000000; }		// 32 bit		δU_B
				if (chbVC34dUc.Checked)		{ iMask2 |= 0x000100000000; }		// 33 bit		δU_C
				if (chbVC34dUab.Checked)	{ iMask2 |= 0x000200000000; }		// 34 bit		δU_AB
				if (chbVC34dUbc.Checked)	{ iMask2 |= 0x000400000000; }		// 35 bit		δU_BC
				if (chbVC34dUca.Checked)	{ iMask2 |= 0x000800000000; }		// 36 bit		δU_CA
				if (chbVC34K2u.Checked)		{ iMask2 |= 0x001000000000; }		// 37 bit
				if (chbVC34K0u.Checked)		{ iMask2 |= 0x002000000000; }		// 38 bit
				if (chbVC34Kua.Checked)		{ iMask2 |= 0x004000000000; }		// 39 bit
				if (chbVC34Kub.Checked)		{ iMask2 |= 0x008000000000; }		// 40 bit
				if (chbVC34Kuc.Checked)		{ iMask2 |= 0x010000000000; }		// 41 bit
				if (chbVC34Kia.Checked)		{ iMask2 |= 0x020000000000; }		// 42 bit
				if (chbVC34Kib.Checked)		{ iMask2 |= 0x040000000000; }		// 43 bit
				if (chbVC34Kic.Checked)		{ iMask2 |= 0x080000000000; }		// 44 bit
				if (chbVC34Qsum.Checked || chbVC34Qa.Checked ||
					chbVC34Qb.Checked || chbVC34Qc.Checked)
				{ iMask2 |= 0x100000000000; }		// 45 bit

				res.MaskTable_1_4[0] = iMask;
				res.MaskTable_1_4[1] = iMask2;

				// table period_avg_params_5
				iMask = iMask2 = 0;

				if (chbVC34Kuan.Checked)	{ iMask |= 0x000000001; }		// 1 bit
				if (chbVC34Kubn.Checked)	{ iMask |= 0x000000002; }		// 2 bit
				if (chbVC34Kucn.Checked)	{ iMask |= 0x000000004; }		// 3 bit
				if (chbVC34Kian.Checked)	{ iMask |= 0x000000008; }		// 4 bit
				if (chbVC34Kibn.Checked)	{ iMask |= 0x000000010; }		// 5 bit
				if (chbVC34Kicn.Checked)	{ iMask |= 0x000000020; }		// 6 bit
				if (chbVC34Kuab.Checked)	{ iMask |= 0x000000040; }		// 7 bit
				if (chbVC34Kuabn.Checked)	{ iMask |= 0x000000080; }		// 8 bit
				if (chbVC34Kubc.Checked)	{ iMask |= 0x000000100; }		// 9 bit
				if (chbVC34Kubcn.Checked)	{ iMask |= 0x000000200; }		// 10 bit
				if (chbVC34Kuca.Checked)	{ iMask |= 0x000000400; }		// 11 bit
				if (chbVC34Kucan.Checked)	{ iMask |= 0x000000800; }		// 12 bit
				if (chbVC34Inn.Checked)		{ iMask |= 0x000001000; }		// 13 bit
				
				res.MaskTable_5[0] = iMask;
				res.MaskTable_5[1] = iMask2;

				// table period_avg_params_6a
				iMask = iMask2 = 0;

				if (chbVC34Pan.Checked)		{ iMask |= 0x000000001; }		// 1 bit
				if (chbVC34Pbn.Checked)		{ iMask |= 0x000000002; }		// 2 bit
				if (chbVC34Pcn.Checked)		{ iMask |= 0x000000004; }		// 3 bit

				res.MaskTable_6a[0] = iMask;
				res.MaskTable_6a[1] = iMask2;

				// table period_avg_params_6b
				iMask = iMask2 = 0;

				if (chbVC34UaIan.Checked)	{ iMask |= 0x000000001; }		// 1 bit
				if (chbVC34UbIbn.Checked)	{ iMask |= 0x000000002; }		// 2 bit
				if (chbVC34UcIcn.Checked)	{ iMask |= 0x000000004; }		// 3 bit

				res.MaskTable_6b[0] = iMask;
				res.MaskTable_6b[1] = iMask2;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetMask34():");
				throw;
			}

			return res;
		}

		private MaskAvgArray GetMask33()
		{
			MaskAvgArray res = new MaskAvgArray();

			try
			{
				//if (IfAllParamsSelected33())
				//{
				//    return res;
				//}

				// table period_avg_params_1_4
				Int64 iMask = 0, iMask2 = 0;

				if (chbVC33F.Checked)		{ iMask |= 0x000000001; }		// 1 bit
				
				if (chbVC33Uab.Checked)		{ iMask |= 0x000000080; }		// 8 bit
				if (chbVC33Ubc.Checked)		{ iMask |= 0x000000100; }		// 9 bit
				if (chbVC33Uca.Checked)		{ iMask |= 0x000000200; }		// 10 bit
				if (chbVC33U1ab.Checked)	{ iMask |= 0x000000400; }		// 11 bit
				if (chbVC33U1bc.Checked)	{ iMask |= 0x000000800; }		// 12 bit
				if (chbVC33U1ca.Checked)	{ iMask |= 0x000001000; }		// 13 bit

				if (chbVC33Uab0.Checked)	{ iMask |= 0x000010000; }		// 17 bit
				if (chbVC33Ubc0.Checked)	{ iMask |= 0x000020000; }		// 18 bit
				if (chbVC33Uca0.Checked)	{ iMask |= 0x000040000; }		// 19 bit

				if (chbVC33U1.Checked)		{ iMask |= 0x000080000; }		// 20 bit			U_1
				if (chbVC33U2.Checked)		{ iMask |= 0x000100000; }		// 21 bit			U_2
				//if (chbVC33U0.Checked)		{ iMask |= 0x000200000; }	// 22 bit			U_0

				if (chbVC33Ia.Checked)		{ iMask |= 0x000400000; }		// 23 bit
				if (chbVC33Ib.Checked)		{ iMask |= 0x000800000; }		// 24 bit
				if (chbVC33Ic.Checked)		{ iMask |= 0x001000000; }		// 25 bit
				if (chbVC33I1a.Checked)		{ iMask |= 0x002000000; }		// 26 bit
				if (chbVC33I1b.Checked)		{ iMask |= 0x004000000; }		// 27 bit
				if (chbVC33I1c.Checked)		{ iMask |= 0x008000000; }		// 28 bit
				if (chbVC33I1.Checked)		{ iMask |= 0x010000000; }		// 29 bit
				if (chbVC33I2.Checked)		{ iMask |= 0x020000000; }		// 30 bit
				////Mask2/////////////////////////////////////////////////////////////
				if (chbVC33Psum.Checked)	{ iMask2 |= 0x000000000001; }		// 1 bit
				if (chbVC33Pab.Checked)		{ iMask2 |= 0x000000000002; }		// 2 bit
				if (chbVC33Pcb.Checked)		{ iMask2 |= 0x000000000004; }		// 3 bit
				
				if (chbVC33Ssum.Checked)	{ iMask2 |= 0x000000000010; }		// 5 bit

				if (chbVC33Qsum.Checked)
				{
					iMask |= 0x000080000000;		// 32 bit
					iMask |= 0x000800000000;		// 36 bit
					iMask |= 0x008000000000;		// 40 bit
					iMask2 |= 0x000000000100;		// 9 bit
				}
				if (chbVC33Qa.Checked)
				{
					iMask |= 0x000100000000;		// 33 bit
					iMask |= 0x001000000000;		// 37 bit
					iMask |= 0x010000000000;		// 41 bit
					iMask2 |= 0x000000000200;		// 10 bit
				}
				if (chbVC33Qb.Checked)
				{
					iMask |= 0x000200000000;		// 34 bit
					iMask |= 0x002000000000;		// 38 bit
					iMask |= 0x020000000000;		// 42 bit
					iMask2 |= 0x000000000400;		// 11 bit
				}
				if (chbVC33Qc.Checked)
				{
					iMask |= 0x000400000000;		// 35 bit
					iMask |= 0x004000000000;		// 39 bit
					iMask |= 0x040000000000;		// 43 bit
					iMask2 |= 0x000000000800;		// 12 bit
				}	
				if (chbVC33Kps.Checked)		{ iMask2 |= 0x000000001000; }		// 13 bit
				
				if (chbVC33orUab.Checked)	{ iMask2 |= 0x000000010000; }		// 17 bit
				if (chbVC33orUbc.Checked)	{ iMask2 |= 0x000000020000; }		// 18 bit
				if (chbVC33orUca.Checked)	{ iMask2 |= 0x000000040000; }		// 19 bit
				if (chbVC33orUaIa.Checked)	{ iMask2 |= 0x000000080000; }		// 20 bit
				if (chbVC33orUbIb.Checked)	{ iMask2 |= 0x000000100000; }		// 21 bit
				if (chbVC33orUcIc.Checked)	{ iMask2 |= 0x000000200000; }		// 22 bit
				if (chbVC33UI1.Checked)		{ iMask2 |= 0x000000400000; }		// 23 bit
				if (chbVC33UI2.Checked)		{ iMask2 |= 0x000000800000; }		// 24 bit
				//if (chbVC33UI0.Checked) { iMask2 |= 0x000001000000; }		// 25 bit

				if (chbVC33P1.Checked)		{ iMask2 |= 0x000004000000; }		// 27 bit
				if (chbVC33P2.Checked)		{ iMask2 |= 0x000008000000; }		// 28 bit
				if (chbVC33dF.Checked)		{ iMask2 |= 0x000010000000; }		// 29 bit
				if (chbVC33dU.Checked)		{ iMask2 |= 0x000020000000; }		// 30 bit

				if (chbVC33dUab.Checked)	{ iMask2 |= 0x000200000000; }		// 34 bit		δU_AB
				if (chbVC33dUbc.Checked)	{ iMask2 |= 0x000400000000; }		// 35 bit		δU_BC
				if (chbVC33dUca.Checked)	{ iMask2 |= 0x000800000000; }		// 36 bit		δU_CA
				
				if (chbVC33K2u.Checked)		{ iMask2 |= 0x001000000000; }		// 37 bit

				if (chbVC33Kuab.Checked)	{ iMask2 |= 0x004000000000; }		// 39 bit
				if (chbVC33Kubc.Checked)	{ iMask2 |= 0x008000000000; }		// 40 bit
				if (chbVC33Kuca.Checked)	{ iMask2 |= 0x010000000000; }		// 41 bit
				if (chbVC33Kia.Checked)		{ iMask2 |= 0x020000000000; }		// 42 bit
				if (chbVC33Kib.Checked)		{ iMask2 |= 0x040000000000; }		// 43 bit
				if (chbVC33Kic.Checked)		{ iMask2 |= 0x080000000000; }		// 44 bit
				if (chbVC33Qsum.Checked || chbVC33Qa.Checked ||
					chbVC33Qb.Checked || chbVC33Qc.Checked)
				{ iMask2 |= 0x100000000000; }		// 45 bit

				res.MaskTable_1_4[0] = iMask;
				res.MaskTable_1_4[1] = iMask2;

				// table period_avg_params_5
				iMask = iMask2 = 0;

				if (chbVC33Kian.Checked)	{ iMask |= 0x000000000008; }		// 4 bit
				if (chbVC33Kibn.Checked)	{ iMask |= 0x000000000010; }		// 5 bit
				if (chbVC33Kicn.Checked)	{ iMask |= 0x000000000020; }		// 6 bit
				if (chbVC33Kuab.Checked)	{ iMask |= 0x000000000040; }		// 7 bit
				if (chbVC33Kuabn.Checked)	{ iMask |= 0x000000000080; }		// 8 bit
				if (chbVC33Kubc.Checked)	{ iMask |= 0x000000000100; }		// 9 bit
				if (chbVC33Kubcn.Checked)	{ iMask |= 0x000000000200; }		// 10 bit
				if (chbVC33Kuca.Checked)	{ iMask |= 0x000000000400; }		// 11 bit
				if (chbVC33Kucan.Checked)	{ iMask |= 0x000000000800; }		// 12 bit

				res.MaskTable_5[0] = iMask;
				res.MaskTable_5[1] = iMask2;

				// table period_avg_params_6a
				iMask = iMask2 = 0;

				if (chbVC33Pan.Checked)		{ iMask |= 0x000000001; }		// 1 bit
				if (chbVC33Pbn.Checked)		{ iMask |= 0x000000002; }		// 2 bit
				if (chbVC33Pcn.Checked)		{ iMask |= 0x000000004; }		// 3 bit

				res.MaskTable_6a[0] = iMask;
				res.MaskTable_6a[1] = iMask2;

				// table period_avg_params_6b
				iMask = iMask2 = 0;

				if (chbVC33UaIan.Checked)	{ iMask |= 0x000000001; }		// 1 bit
				if (chbVC33UbIbn.Checked)	{ iMask |= 0x000000002; }		// 2 bit
				if (chbVC33UcIcn.Checked)	{ iMask |= 0x000000004; }		// 3 bit

				res.MaskTable_6b[0] = iMask;
				res.MaskTable_6b[1] = iMask2;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetMask33():");
				throw;
			}

			return res;
		}

		private MaskAvgArray GetMask12()
		{
			MaskAvgArray res = new MaskAvgArray();

			try
			{
				//if (IfAllParamsSelected12())
				//{
				//    return res;
				//}

				// table period_avg_params_1_4
				Int64 iMask = 0, iMask2 = 0;

				if (chbVC12F.Checked)	{ iMask |= 0x000000001; }	// 1 bit
				if (chbVC12Ua.Checked)	{ iMask |= 0x000000002; }	// 2 bit
				
				if (chbVC12U1a.Checked) { iMask |= 0x000000010; }	// 5 bit

				if (chbVC12Ua0.Checked) { iMask |= 0x000002000; }	// 14 bit

				if (chbVC12U1.Checked)	{ iMask |= 0x000080000; }	// 20 bit

				if (chbVC12I.Checked)	{ iMask |= 0x000400000; }	// 23 bit

				if (chbVC12I1a.Checked) { iMask |= 0x002000000; }	// 26 bit
				
				if (chbVC12I1.Checked)	{ iMask |= 0x010000000; }	// 29 bit
				/////Mask2/////////////////////////////////////////////////////
				if (chbVC12P.Checked)	{ iMask2 |= 0x000000000001; }		// 1 bit
				
				if (chbVC12S.Checked)	{ iMask2 |= 0x000000000010; }		// 5 bit
				
				if (chbVC12Q.Checked)
				{
					iMask |= 0x000080000000;		// 32 bit
					iMask |= 0x000800000000;		// 36 bit
					iMask |= 0x008000000000;		// 40 bit
					iMask2 |= 0x000000000100;		// 9 bit
				}
				
				if (chbVC12Kps.Checked) { iMask2 |= 0x000000001000; }		// 13 bit

				if (chbVC12UaIa.Checked) { iMask2 |= 0x000000080000; }		// 20 bit

				if (chbVC12orUI1.Checked) { iMask2 |= 0x000000400000; }		// 23 bit

				if (chbVC12P1.Checked)	{ iMask2 |= 0x000004000000; }		// 27 bit
				
				if (chbVC12dF.Checked)	{ iMask2 |= 0x000010000000; }		// 29 bit
				if (chbVC12dU.Checked)	{ iMask2 |= 0x000020000000; }		// 30 bit
				
				if (chbVC12Ku.Checked)	{ iMask2 |= 0x004000000000; }		// 39 bit
				
				if (chbVC12Ki.Checked)	{ iMask2 |= 0x020000000000; }		// 42 bit
				
				if (chbVC12Q.Checked)	{ iMask2 |= 0x100000000000; }		// 45 bit

				res.MaskTable_1_4[0] = iMask;
				res.MaskTable_1_4[1] = iMask2;

				// table period_avg_params_5
				iMask = iMask2 = 0;

				if (chbVC12Kun.Checked) { iMask |= 0x000000001; }		// 1 bit
				
				if (chbVC12Kin.Checked) { iMask |= 0x000000008; }		// 4 bit

				res.MaskTable_5[0] = iMask;
				res.MaskTable_5[1] = iMask2;

				// table period_avg_params_6a
				iMask = iMask2 = 0;
				if (chbVC12Pn.Checked) { iMask |= 0x000000001; }		// 1 bit
				res.MaskTable_6a[0] = iMask;
				res.MaskTable_6a[1] = iMask2;

				// table period_avg_params_6b
				iMask = iMask2 = 0;
				if (chbVC12UaIan.Checked) { iMask |= 0x000000001; }		// 1 bit
				res.MaskTable_6b[0] = iMask;
				res.MaskTable_6b[1] = iMask2;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetMask12():");
				throw;
			}

			return res;
		}

		#endregion

		#region Public Methods

		public static void GetParametersList(out List<ushort> fields, MaskAvgArray mask,
												ConnectScheme conScheme, EmDeviceType devType)
		{
			switch (conScheme)
			{
				case ConnectScheme.Ph3W4:
				case ConnectScheme.Ph3W4_B_calc:
					GetParameters34(out fields, ref mask, devType); break;
				case ConnectScheme.Ph3W3:
				case ConnectScheme.Ph3W3_B_calc:
					GetParameters33(out fields, ref mask, devType); break;
				case ConnectScheme.Ph1W2:
					GetParameters12(out fields, ref mask, devType); break;
				default:
					EmService.WriteToLogFailed("GetParametersList(): Unknown connection scheme!!!");
					GetParameters34(out fields, ref mask, devType); break;
			}

			// если список параметров отсутствует, то заполняем его полностью, чтобы был считан
			// весь архив
			// вернуть это если будет нужно чтение avg одним запросом
			//if (devType == EmDeviceType.EM32)
			//{
			//    if (fields == null)
			//    {
			//        fields = new List<ushort>(2048);
			//        for (ushort i = 0; i < 2048; ++i)
			//            fields.Add(i);
			//    }
			//}
		}

		public MaskAvgArray GetMask()
		{
			switch (conScheme_)
			{
				case ConnectScheme.Ph3W4:
				case ConnectScheme.Ph3W4_B_calc:
					return GetMask34();
				case ConnectScheme.Ph3W3:
				case ConnectScheme.Ph3W3_B_calc:
					return GetMask33();
				case ConnectScheme.Ph1W2:
					return GetMask12();
				default:
					return GetMask34();
			}
		}

		public void RestoreDefaultFromRegistry()
		{
			try
			{
				MaskAvgArray masks12 = new MaskAvgArray();
				MaskAvgArray masks33 = new MaskAvgArray();
				MaskAvgArray masks34 = new MaskAvgArray();

				const string keyName = 
					"HKEY_LOCAL_MACHINE\\SOFTWARE\\Mars-Energo\\EnergomonitoringXP\\DefaultAVGParams";
				Int64 curMask;

				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_1_4_0", "-1"), out curMask))
					masks12.MaskTable_1_4[0] = curMask;
				else masks12.MaskTable_1_4[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_1_4_1", "-1"), out curMask))
					masks12.MaskTable_1_4[1] = curMask;
				else masks12.MaskTable_1_4[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_5_0", "-1"), out curMask))
					masks12.MaskTable_5[0] = curMask;
				else masks12.MaskTable_5[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_5_1", "-1"), out curMask))
					masks12.MaskTable_5[1] = curMask;
				else masks12.MaskTable_5[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_6a_0", "-1"), out curMask))
					masks12.MaskTable_6a[0] = curMask;
				else masks12.MaskTable_6a[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_6a_1", "-1"), out curMask))
					masks12.MaskTable_6a[1] = curMask;
				else masks12.MaskTable_6a[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_6b_0", "-1"), out curMask))
					masks12.MaskTable_6b[0] = curMask;
				else masks12.MaskTable_6b[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable12_6b_1", "-1"), out curMask))
					masks12.MaskTable_6b[1] = curMask;
				else masks12.MaskTable_6b[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_1_4_0", "-1"), out curMask))
					masks33.MaskTable_1_4[0] = curMask;
				else masks33.MaskTable_1_4[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_1_4_1", "-1"), out curMask))
					masks33.MaskTable_1_4[1] = curMask;
				else masks33.MaskTable_1_4[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_5_0", "-1"), out curMask))
					masks33.MaskTable_5[0] = curMask;
				else masks33.MaskTable_5[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_5_1", "-1"), out curMask))
					masks33.MaskTable_5[1] = curMask;
				else masks33.MaskTable_5[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_6a_0", "-1"), out curMask))
					masks33.MaskTable_6a[0] = curMask;
				else masks33.MaskTable_6a[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_6a_1", "-1"), out curMask))
					masks33.MaskTable_6a[1] = curMask;
				else masks33.MaskTable_6a[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_6b_0", "-1"), out curMask))
					masks33.MaskTable_6b[0] = curMask;
				else masks33.MaskTable_6b[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable33_6b_1", "-1"), out curMask))
					masks33.MaskTable_6b[1] = curMask;
				else masks33.MaskTable_6b[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_1_4_0", "-1"), out curMask))
					masks34.MaskTable_1_4[0] = curMask;
				else masks34.MaskTable_1_4[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_1_4_1", "-1"), out curMask))
					masks34.MaskTable_1_4[1] = curMask;
				else masks34.MaskTable_1_4[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_5_0", "-1"), out curMask))
					masks34.MaskTable_5[0] = curMask;
				else masks34.MaskTable_5[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_5_1", "-1"), out curMask))
					masks34.MaskTable_5[1] = curMask;
				else masks34.MaskTable_5[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_6a_0", "-1"), out curMask))
					masks34.MaskTable_6a[0] = curMask;
				else masks34.MaskTable_6a[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_6a_1", "-1"), out curMask))
					masks34.MaskTable_6a[1] = curMask;
				else masks34.MaskTable_6a[1] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_6b_0", "-1"), out curMask))
					masks34.MaskTable_6b[0] = curMask;
				else masks34.MaskTable_6b[0] = -1;
				if (Int64.TryParse((string)Registry.GetValue(keyName, "MaskTable34_6b_1", "-1"), out curMask))
					masks34.MaskTable_6b[1] = curMask;
				else masks34.MaskTable_6b[1] = -1;

				LoadFormState(ref masks34, ref masks33, ref masks12);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in RestoreDefaultFromRegistry():");
				throw;
			}
		}

		public void LoadFormState(ref MaskAvgArray newState34, ref MaskAvgArray newState33,
									ref MaskAvgArray newState12)
		{
			LoadFormState(newState34, ConnectScheme.Ph3W4);
			LoadFormState(newState33, ConnectScheme.Ph3W3);
			LoadFormState(newState12, ConnectScheme.Ph1W2);

			//if (chbVC34Psum.Checked && chbVC34Qsum.Checked)
			//{
			//    chbVC34TanP.Checked = true;
			//}
			//else chbVC34TanP.Checked = false;
			//if (chbVC33Psum.Checked && chbVC33Qsum.Checked)
			//{
			//    chbVC33TanP.Checked = true;
			//}
			//else chbVC33TanP.Checked = false;
			//if (chbVC12P.Checked && chbVC12Q.Checked)
			//{
			//    chbVC12TanP.Checked = true;
			//}
			//else chbVC12TanP.Checked = false;
		}

		public void LoadFormState(MaskAvgArray newState, ConnectScheme conScheme)
		{
			try
			{
				if (newState.IsEmpty)
				{
					ChangeSelection(true, conScheme);
					return;
				}

				ChangeSelection(false, conScheme);

				switch (conScheme)
				{
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						Int64 iMask = newState.MaskTable_1_4[0];
						Int64 iMask2 = newState.MaskTable_1_4[1];

						if ((iMask & 0x000000001) != 0) { chbVC34F.Checked = true; }		// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC34Ua.Checked = true; }		// 2 bit
						if ((iMask & 0x000000004) != 0) { chbVC34Ub.Checked = true; }		// 3 bit
						if ((iMask & 0x000000008) != 0) { chbVC34Uc.Checked = true; }		// 4 bit
						if ((iMask & 0x000000010) != 0) { chbVC34U1a.Checked = true; }		// 5 bit
						if ((iMask & 0x000000020) != 0) { chbVC34U1b.Checked = true; }		// 6 bit
						if ((iMask & 0x000000040) != 0) { chbVC34U1c.Checked = true; }		// 7 bit
						if ((iMask & 0x000000080) != 0) { chbVC34Uab.Checked = true; }		// 8 bit
						if ((iMask & 0x000000100) != 0) { chbVC34Ubc.Checked = true; }		// 9 bit
						if ((iMask & 0x000000200) != 0) { chbVC34Uca.Checked = true; }		// 10 bit
						if ((iMask & 0x000000400) != 0) { chbVC34U1ab.Checked = true; }		// 11 bit
						if ((iMask & 0x000000800) != 0) { chbVC34U1bc.Checked = true; }		// 12 bit
						if ((iMask & 0x000001000) != 0) { chbVC34U1ca.Checked = true; }		// 13 bit
						if ((iMask & 0x000002000) != 0) { chbVC34Ua0.Checked = true; }		// 14 bit
						if ((iMask & 0x000004000) != 0) { chbVC34Ub0.Checked = true; }		// 15 bit
						if ((iMask & 0x000008000) != 0) { chbVC34Uc0.Checked = true; }		// 16 bit
						if ((iMask & 0x000010000) != 0) { chbVC34Uab0.Checked = true; }		// 17 bit
						if ((iMask & 0x000020000) != 0) { chbVC34Ubc0.Checked = true; }		// 18 bit
						if ((iMask & 0x000040000) != 0) { chbVC34Uca0.Checked = true; }		// 19 bit
						if ((iMask & 0x000080000) != 0) { chbVC34U1.Checked = true; }		// 20 bit
						if ((iMask & 0x000100000) != 0) { chbVC34U2.Checked = true; }		// 21 bit
						if ((iMask & 0x000200000) != 0) { chbVC34U0.Checked = true; }		// 22 bit
						if ((iMask & 0x000400000) != 0) { chbVC34Ia.Checked = true; }		// 23 bit
						if ((iMask & 0x000800000) != 0) { chbVC34Ib.Checked = true; }		// 24 bit
						if ((iMask & 0x001000000) != 0) { chbVC34Ic.Checked = true; }		// 25 bit
						if ((iMask & 0x002000000) != 0) { chbVC34I1a.Checked = true; }		// 26 bit
						if ((iMask & 0x004000000) != 0) { chbVC34I1b.Checked = true; }		// 27 bit
						if ((iMask & 0x008000000) != 0) { chbVC34I1c.Checked = true; }		// 28 bit
						if ((iMask & 0x010000000) != 0) { chbVC34I1.Checked = true; }		// 29 bit
						if ((iMask & 0x020000000) != 0) { chbVC34I2.Checked = true; }		// 30 bit
						if ((iMask & 0x040000000) != 0) { chbVC34I0.Checked = true; }		// 31 bit

						if ((iMask & 0x080000000000) != 0) { chbVC34In.Checked = true; }	// 44 bit

						/////Mask2 ////////////////////////////////////////////////////////////////

						if ((iMask2 & 0x000000001) != 0) { chbVC34Psum.Checked = true; }	// 1 bit
						if ((iMask2 & 0x000000002) != 0) { chbVC34Pa.Checked = true; }		// 2 bit
						if ((iMask2 & 0x000000004) != 0) { chbVC34Pb.Checked = true; }		// 3 bit
						if ((iMask2 & 0x000000008) != 0) { chbVC34Pc.Checked = true; }		// 4 bit
						if ((iMask2 & 0x000000010) != 0) { chbVC34Ssum.Checked = true; }	// 5 bit
						if ((iMask2 & 0x000000020) != 0) { chbVC34Sa.Checked = true; }		// 6 bit
						if ((iMask2 & 0x000000040) != 0) { chbVC34Sb.Checked = true; }		// 7 bit
						if ((iMask2 & 0x000000080) != 0) { chbVC34Sc.Checked = true; }		// 8 bit
						if ((iMask2 & 0x000000100) != 0) { chbVC34Qsum.Checked = true; }	// 9 bit
						if ((iMask2 & 0x000000200) != 0) { chbVC34Qa.Checked = true; }		// 10 bit
						if ((iMask2 & 0x000000400) != 0) { chbVC34Qb.Checked = true; }		// 11 bit
						if ((iMask2 & 0x000000800) != 0) { chbVC34Qc.Checked = true; }		// 12 bit
						if ((iMask2 & 0x000001000) != 0) { chbVC34Kps.Checked = true; }		// 13 bit
						if ((iMask2 & 0x000002000) != 0) { chbVC34Kpa.Checked = true; }		// 14 bit
						if ((iMask2 & 0x000004000) != 0) { chbVC34Kpb.Checked = true; }		// 15 bit
						if ((iMask2 & 0x000008000) != 0) { chbVC34Kpc.Checked = true; }		// 16 bit
						if ((iMask2 & 0x000010000) != 0) { chbVC34orUab.Checked = true; }		// 17 bit
						if ((iMask2 & 0x000020000) != 0) { chbVC34orUbc.Checked = true; }		// 18 bit
						if ((iMask2 & 0x000040000) != 0) { chbVC34orUca.Checked = true; }		// 19 bit
						if ((iMask2 & 0x000000080000) != 0) { chbVC34orUaIa.Checked = true; }	// 20 bit
						if ((iMask2 & 0x000000100000) != 0) { chbVC34orUbIb.Checked = true; }	// 21 bit
						if ((iMask2 & 0x000000200000) != 0) { chbVC34orUcIc.Checked = true; }	// 22 bit
						if ((iMask2 & 0x000000400000) != 0) { chbVC34UI1.Checked = true; }	// 23 bit
						if ((iMask2 & 0x000000800000) != 0) { chbVC34UI2.Checked = true; }	// 24 bit
						if ((iMask2 & 0x000001000000) != 0) { chbVC34UI0.Checked = true; }	// 25 bit
						if ((iMask2 & 0x000002000000) != 0) { chbVC34P0.Checked = true; }	// 26 bit
						if ((iMask2 & 0x000004000000) != 0) { chbVC34P1.Checked = true; }	// 27 bit
						if ((iMask2 & 0x000008000000) != 0) { chbVC34P2.Checked = true; }	// 28 bit
						if ((iMask2 & 0x000010000000) != 0) { chbVC34dF.Checked = true; }	// 29 bit
						if ((iMask2 & 0x000020000000) != 0) { chbVC34dU.Checked = true; }	// 30 bit
						if ((iMask2 & 0x000040000000) != 0) { chbVC34dUa.Checked = true; }	// 31 bit
						if ((iMask2 & 0x000080000000) != 0) { chbVC34dUb.Checked = true; }	// 32 bit
						if ((iMask2 & 0x000100000000) != 0) { chbVC34dUc.Checked = true; }	// 33 bit
						if ((iMask2 & 0x000200000000) != 0) { chbVC34dUab.Checked = true; }	// 34 bit
						if ((iMask2 & 0x000400000000) != 0) { chbVC34dUbc.Checked = true; }	// 35 bit
						if ((iMask2 & 0x000800000000) != 0) { chbVC34dUca.Checked = true; }	// 36 bit

						if ((iMask2 & 0x001000000000) != 0) { chbVC34K2u.Checked = true; }		// 37 bit
						if ((iMask2 & 0x002000000000) != 0) { chbVC34K0u.Checked = true; }		// 38 bit
						if ((iMask2 & 0x004000000000) != 0) { chbVC34Kua.Checked = true; }		// 39 bit
						if ((iMask2 & 0x008000000000) != 0) { chbVC34Kub.Checked = true; }		// 40 bit
						if ((iMask2 & 0x010000000000) != 0) { chbVC34Kuc.Checked = true; }		// 41 bit
						if ((iMask2 & 0x020000000000) != 0) { chbVC34Kia.Checked = true; }		// 42 bit
						if ((iMask2 & 0x040000000000) != 0) { chbVC34Kib.Checked = true; }		// 43 bit
						if ((iMask2 & 0x080000000000) != 0) { chbVC34Kic.Checked = true; }		// 44 bit

						//if (chbVC34Qsum.Checked || chbVC34Qa.Checked ||
						//    chbVC34Qb.Checked || chbVC34Qc.Checked)
						//{ iMask2 |= 0x100000000000; }		// 45 bit

						// table period_avg_params_5
						iMask = newState.MaskTable_5[0];

						if ((iMask & 0x000000001) != 0) { chbVC34Kuan.Checked = true; }		// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC34Kubn.Checked = true; }		// 2 bit
						if ((iMask & 0x000000004) != 0) { chbVC34Kucn.Checked = true; }		// 3 bit
						if ((iMask & 0x000000008) != 0) { chbVC34Kian.Checked = true; }		// 4 bit
						if ((iMask & 0x000000010) != 0) { chbVC34Kibn.Checked = true; }		// 5 bit
						if ((iMask & 0x000000020) != 0) { chbVC34Kicn.Checked = true; }		// 6 bit
						if ((iMask & 0x000000040) != 0) { chbVC34Kuab.Checked = true; }		// 7 bit
						if ((iMask & 0x000000080) != 0) { chbVC34Kuabn.Checked = true; }	// 8 bit
						if ((iMask & 0x000000100) != 0) { chbVC34Kubc.Checked = true; }		// 9 bit
						if ((iMask & 0x000000200) != 0) { chbVC34Kubcn.Checked = true; }	// 10 bit
						if ((iMask & 0x000000400) != 0) { chbVC34Kuca.Checked = true; }		// 11 bit
						if ((iMask & 0x000000800) != 0) { chbVC34Kucan.Checked = true; }	// 12 bit
						if ((iMask & 0x000001000) != 0) { chbVC34Inn.Checked = true; }		// 13 bit

						// table period_avg_params_6a
						iMask = newState.MaskTable_6a[0];

						if ((iMask & 0x000000001) != 0) { chbVC34Pan.Checked = true; }		// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC34Pbn.Checked = true; }		// 2 bit
						if ((iMask & 0x000000004) != 0) { chbVC34Pcn.Checked = true; }		// 3 bit

						// table period_avg_params_6b
						iMask = newState.MaskTable_6b[0];

						if ((iMask & 0x000000001) != 0) { chbVC34UaIan.Checked = true; }	// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC34UbIbn.Checked = true; }	// 2 bit
						if ((iMask & 0x000000004) != 0) { chbVC34UcIcn.Checked = true; }	// 3 bit
						break;

					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						iMask = newState.MaskTable_1_4[0];
						iMask2 = newState.MaskTable_1_4[1];

						if ((iMask & 0x000000001) != 0) { chbVC33F.Checked = true; }		// 1 bit

						if ((iMask & 0x000000080) != 0) { chbVC33Uab.Checked = true; }		// 8 bit
						if ((iMask & 0x000000100) != 0) { chbVC33Ubc.Checked = true; }		// 9 bit
						if ((iMask & 0x000000200) != 0) { chbVC33Uca.Checked = true; }		// 10 bit
						if ((iMask & 0x000000400) != 0) { chbVC33U1ab.Checked = true; }		// 11 bit
						if ((iMask & 0x000000800) != 0) { chbVC33U1bc.Checked = true; }		// 12 bit
						if ((iMask & 0x000001000) != 0) { chbVC33U1ca.Checked = true; }		// 13 bit

						if ((iMask & 0x000010000) != 0) { chbVC33Uab0.Checked = true; }		// 17 bit
						if ((iMask & 0x000020000) != 0) { chbVC33Ubc0.Checked = true; }		// 18 bit
						if ((iMask & 0x000040000) != 0) { chbVC33Uca0.Checked = true; }		// 19 bit

						if ((iMask & 0x000080000) != 0) { chbVC33U1.Checked = true; }		// 20 bit
						if ((iMask & 0x000100000) != 0) { chbVC33U2.Checked = true; }		// 21 bit
						//if ((iMask & 0x000200000) != 0) { chbVC33U0.Checked = true; }		// 22 bit

						if ((iMask & 0x000400000) != 0) { chbVC33Ia.Checked = true; }		// 23 bit
						if ((iMask & 0x000800000) != 0) { chbVC33Ib.Checked = true; }		// 24 bit
						if ((iMask & 0x001000000) != 0) { chbVC33Ic.Checked = true; }		// 25 bit
						if ((iMask & 0x002000000) != 0) { chbVC33I1a.Checked = true; }		// 26 bit
						if ((iMask & 0x004000000) != 0) { chbVC33I1b.Checked = true; }		// 27 bit
						if ((iMask & 0x008000000) != 0) { chbVC33I1c.Checked = true; }		// 28 bit
						if ((iMask & 0x010000000) != 0) { chbVC33I1.Checked = true; }		// 29 bit
						if ((iMask & 0x020000000) != 0) { chbVC33I2.Checked = true; }		// 30 bit

						/////Mask2//////////////////////////////////////////////////
						if ((iMask2 & 0x000000001) != 0) { chbVC33Psum.Checked = true; }	// 1 bit
						if ((iMask2 & 0x000000002) != 0) { chbVC33Pab.Checked = true; }		// 2 bit
						if ((iMask2 & 0x000000004) != 0) { chbVC33Pcb.Checked = true; }		// 3 bit

						if ((iMask2 & 0x000000010) != 0) { chbVC33Ssum.Checked = true; }	// 5 bit

						if ((iMask2 & 0x000000100) != 0) { chbVC33Qsum.Checked = true; }	// 9 bit
						if ((iMask2 & 0x000000200) != 0) { chbVC33Qa.Checked = true; }		// 10 bit
						if ((iMask2 & 0x000000400) != 0) { chbVC33Qb.Checked = true; }		// 11 bit
						if ((iMask2 & 0x000000800) != 0) { chbVC33Qc.Checked = true; }		// 12 bit
						if ((iMask2 & 0x000001000) != 0) { chbVC33Kps.Checked = true; }		// 13 bit

						if ((iMask2 & 0x000010000) != 0) { chbVC33orUab.Checked = true; }		// 17 bit
						if ((iMask2 & 0x000020000) != 0) { chbVC33orUbc.Checked = true; }		// 18 bit
						if ((iMask2 & 0x000040000) != 0) { chbVC33orUca.Checked = true; }		// 19 bit
						if ((iMask2 & 0x000000080000) != 0) { chbVC33orUaIa.Checked = true; }	// 20 bit
						if ((iMask2 & 0x000000100000) != 0) { chbVC33orUbIb.Checked = true; }	// 21 bit
						if ((iMask2 & 0x000000200000) != 0) { chbVC33orUcIc.Checked = true; }	// 22 bit
						if ((iMask2 & 0x000000400000) != 0) { chbVC33UI1.Checked = true; }	// 23 bit
						if ((iMask2 & 0x000000800000) != 0) { chbVC33UI2.Checked = true; }	// 24 bit
						//if ((iMask2 & 0x000001000000) != 0) { chbVC33UI0.Checked = true; }	// 25 bit

						if ((iMask2 & 0x000004000000) != 0) { chbVC33P1.Checked = true; }		// 27 bit
						if ((iMask2 & 0x000008000000) != 0) { chbVC33P2.Checked = true; }		// 28 bit
						if ((iMask2 & 0x000010000000) != 0) { chbVC33dF.Checked = true; }		// 29 bit
						if ((iMask2 & 0x000020000000) != 0) { chbVC33dU.Checked = true; }		// 30 bit

						if ((iMask2 & 0x000200000000) != 0) { chbVC33dUab.Checked = true; }		// 34 bit
						if ((iMask2 & 0x000400000000) != 0) { chbVC33dUbc.Checked = true; }		// 35 bit
						if ((iMask2 & 0x000800000000) != 0) { chbVC33dUca.Checked = true; }		// 36 bit
						if ((iMask2 & 0x001000000000) != 0) { chbVC33K2u.Checked = true; }		// 37 bit

						if ((iMask2 & 0x004000000000) != 0) { chbVC33Kuab.Checked = true; }		// 39 bit
						if ((iMask2 & 0x008000000000) != 0) { chbVC33Kubc.Checked = true; }		// 40 bit
						if ((iMask2 & 0x010000000000) != 0) { chbVC33Kuca.Checked = true; }		// 41 bit
						if ((iMask2 & 0x020000000000) != 0) { chbVC33Kia.Checked = true; }		// 42 bit
						if ((iMask2 & 0x040000000000) != 0) { chbVC33Kib.Checked = true; }		// 43 bit
						if ((iMask2 & 0x080000000000) != 0) { chbVC33Kic.Checked = true; }		// 44 bit

						iMask = newState.MaskTable_5[0];

						if ((iMask & 0x000000008) != 0) { chbVC33Kian.Checked = true; }		// 4 bit
						if ((iMask & 0x000000010) != 0) { chbVC33Kibn.Checked = true; }		// 5 bit
						if ((iMask & 0x000000020) != 0) { chbVC33Kicn.Checked = true; }		// 6 bit
						if ((iMask & 0x000000040) != 0) { chbVC33Kuab.Checked = true; }		// 7 bit
						if ((iMask & 0x000000080) != 0) { chbVC33Kuabn.Checked = true; }	// 8 bit
						if ((iMask & 0x000000100) != 0) { chbVC33Kubc.Checked = true; }		// 9 bit
						if ((iMask & 0x000000200) != 0) { chbVC33Kubcn.Checked = true; }	// 10 bit
						if ((iMask & 0x000000400) != 0) { chbVC33Kuca.Checked = true; }		// 11 bit
						if ((iMask & 0x000000800) != 0) { chbVC33Kucan.Checked = true; }	// 12 bit

						// table period_avg_params_6a
						iMask = newState.MaskTable_6a[0];

						if ((iMask & 0x000000001) != 0) { chbVC33Pan.Checked = true; }		// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC33Pbn.Checked = true; }		// 2 bit
						if ((iMask & 0x000000004) != 0) { chbVC33Pcn.Checked = true; }		// 3 bit

						// table period_avg_params_6b
						iMask = newState.MaskTable_6b[0];

						if ((iMask & 0x000000001) != 0) { chbVC33UaIan.Checked = true; }	// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC33UbIbn.Checked = true; }	// 2 bit
						if ((iMask & 0x000000004) != 0) { chbVC33UcIcn.Checked = true; }	// 3 bit
						break;

					case ConnectScheme.Ph1W2:
						iMask = newState.MaskTable_1_4[0];
						iMask2 = newState.MaskTable_1_4[1];

						if ((iMask & 0x000000001) != 0) { chbVC12F.Checked = true; }		// 1 bit
						if ((iMask & 0x000000002) != 0) { chbVC12Ua.Checked = true; }		// 2 bit
						if ((iMask & 0x000000010) != 0) { chbVC12U1a.Checked = true; }		// 5 bit
						if ((iMask & 0x000002000) != 0) { chbVC12Ua0.Checked = true; }		// 14 bit
						if ((iMask & 0x000080000) != 0) { chbVC12U1.Checked = true; }		// 20 bit
						if ((iMask & 0x000400000) != 0) { chbVC12I.Checked = true; }		// 23 bit
						if ((iMask & 0x002000000) != 0) { chbVC12I1a.Checked = true; }		// 26 bit
						if ((iMask & 0x010000000) != 0) { chbVC12I1.Checked = true; }		// 29 bit

						/////Mask2 ////////////////////////////////////////////////////////////////
						if ((iMask2 & 0x000000001) != 0) { chbVC12P.Checked = true; }			// 1 bit
						if ((iMask2 & 0x000000010) != 0) { chbVC12S.Checked = true; }			// 5 bit
						if ((iMask2 & 0x000000100) != 0) { chbVC12Q.Checked = true; }			// 9 bit
						if ((iMask2 & 0x000001000) != 0) { chbVC12Kps.Checked = true; }			// 13 bit
						if ((iMask2 & 0x000000080000) != 0) { chbVC12UaIa.Checked = true; }		// 20 bit
						if ((iMask2 & 0x000000400000) != 0) { chbVC12orUI1.Checked = true; }	// 23 bit
						if ((iMask2 & 0x000004000000) != 0) { chbVC12P1.Checked = true; }		// 27 bit
						if ((iMask2 & 0x000010000000) != 0) { chbVC12dF.Checked = true; }		// 29 bit
						if ((iMask2 & 0x000020000000) != 0) { chbVC12dU.Checked = true; }		// 30 bit
						if ((iMask2 & 0x004000000000) != 0) { chbVC12Ku.Checked = true; }		// 39 bit
						if ((iMask2 & 0x020000000000) != 0) { chbVC12Ki.Checked = true; }		// 42 bit

						iMask = newState.MaskTable_5[0];

						if ((iMask & 0x000000001) != 0) { chbVC12Kun.Checked = true; }		// 1 bit
						if ((iMask & 0x000000008) != 0) { chbVC12Kin.Checked = true; }		// 4 bit

						// table period_avg_params_6a
						iMask = newState.MaskTable_6a[0];
						if ((iMask & 0x000000001) != 0) { chbVC12Pn.Checked = true; }		// 1 bit

						// table period_avg_params_6b
						iMask = newState.MaskTable_6b[0];
						if ((iMask & 0x000000001) != 0) { chbVC12UaIan.Checked = true; }		// 1 bit

						break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadFormState():");
				throw;
			}
		}

		#endregion

		#region Inner Class

		public class MaskAvgArray
		{
			private Int64[] maskTable_1_4_ = new Int64[] { -1, -1 };
			private Int64[] maskTable_5_ = new Int64[] { -1, -1 };
			private Int64[] maskTable_6a_ = new Int64[] { -1, -1 };
			private Int64[] maskTable_6b_ = new Int64[] { -1, -1 };

			[XmlAttribute(AttributeName = "MaskTable_1_4")]
			public Int64[] MaskTable_1_4
			{
				get { return maskTable_1_4_; }
				set { maskTable_1_4_ = value; }
			}

			[XmlAttribute(AttributeName = "MaskTable_5")]
			public Int64[] MaskTable_5
			{
				get { return maskTable_5_; }
				set { maskTable_5_ = value; }
			}

			[XmlAttribute(AttributeName = "MaskTable_6a")]
			public Int64[] MaskTable_6a
			{
				get { return maskTable_6a_; }
				set { maskTable_6a_ = value; }
			}

			[XmlAttribute(AttributeName = "MaskTable_6b")]
			public Int64[] MaskTable_6b
			{
				get { return maskTable_6b_; }
				set { maskTable_6b_ = value; }
			}

			public void ResetMasks() 
			{
				MaskTable_1_4[0] = MaskTable_1_4[1] = -1;
				MaskTable_5[0] = MaskTable_5[1] = -1;
				MaskTable_6a[0] = MaskTable_6a[1] = -1;
				MaskTable_6b[0] = MaskTable_6b[1] = -1;
			}

			public bool IsEmpty
			{
				get
				{
					return MaskTable_1_4[0] == -1 && MaskTable_1_4[1] == -1 &&
					MaskTable_5[0] == -1 && MaskTable_5[1] == -1 &&
					MaskTable_6a[0] == -1 && MaskTable_6a[1] == -1 &&
					MaskTable_6b[0] == -1 && MaskTable_6b[1] == -1;
				}
			}
		}

		#endregion
	}
}