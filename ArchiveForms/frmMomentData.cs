using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DeviceIO;
using System.Threading;
using System.Resources;
using System.IO;
using EmServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmMomentData : Form
	{
		EmDataSaver.Settings settings_;

		// ��� ���������� ��������: 1 - 3 sec, 2 - 1 min, 3 - 30 min
		private byte typeAVG_ = 1;
		private object sender_;

		protected Thread ConnectionThreadAVG = null;
		DataReceiveThread dataThread_;
		public static byte[] buffer_ = null;
		public static BaseDeviceCommonInfo devInfo_ = new BaseDeviceCommonInfo();

		public delegate void EnableProgressHandler(bool enable);
		public event EnableProgressHandler OnProgress;

		public frmMomentData(EmDataSaver.Settings s, object sender)
		{
			settings_ = s;
			sender_ = sender;
			InitializeComponent();
		}

		void frmMomentData_OnDataReceived()
		{
			if (this.InvokeRequired == false) // thread checking
			{
				lock (frmMomentData.buffer_)
				{
					ProcessData(ref buffer_);
				}
			}
			else
			{
				DataReceiveThread.DataReceivedHandler received =
					new DataReceiveThread.DataReceivedHandler(frmMomentData_OnDataReceived);
				this.Invoke(received);
			}
		}

		void frmMomentData_OnConnectEnd()
		{
			if (this.InvokeRequired == false) // thread checking
			{
				if (OnProgress != null) OnProgress(false);
			}
			else
			{
				DataReceiveThread.ConnectEndHandler end =
					new DataReceiveThread.ConnectEndHandler(frmMomentData_OnConnectEnd);
				this.Invoke(end);
			}
		}

		private void ProcessData(ref byte[] buffer)
		{
			if (buffer == null)
			{
				EmService.WriteToLogFailed("frmMomentData.ProcessData(): buffer == null");
				return;
			}

			labelSerial2.Text = devInfo_.SerialNumber.ToString();
			labelName2.Text = devInfo_.ObjectName;

			string strConSch = String.Empty;
			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
			switch (devInfo_.ConnectionScheme)
			{
				case ConnectScheme.Ph3W4:
				case ConnectScheme.Ph3W4_B_calc:
					strConSch = rm.GetString("name_con_scheme_Ph3W4_full");
					break;
				case ConnectScheme.Ph3W3:
				case ConnectScheme.Ph3W3_B_calc:
					strConSch = rm.GetString("name_con_scheme_Ph3W3_full");
					break;
				case ConnectScheme.Ph1W2:
					strConSch = rm.GetString("name_con_scheme_Ph1W2_full");
					break;
			}
			labelScheme2.Text = strConSch;

			tbFNom.Text = devInfo_.F_Nominal.ToString();
			tbUNomLin.Text = devInfo_.U_NominalLinear.ToString();
			tbUNomPh.Text = devInfo_.U_NominalPhase.ToString();
			tbINomPh.Text = devInfo_.I_NominalPhase.ToString();

			// ��� ������
			ushort type = Conversions.bytes_2_ushort(ref buffer, 0);
			//if (updateTitle)
			//{
			//    switch (type)
			//    {
			//        case 1: labelAVG.Text += "   (3 sec)"; break;
			//        case 2: labelAVG.Text += "   (1 min)"; break;
			//        case 3: labelAVG.Text += "   (30 min)"; break;
			//    }
			//}
			System.Globalization.CultureInfo cultureRu = new System.Globalization.CultureInfo("ru-RU");
			// �������
			float f = Conversions.bytes_2_signed_float_Q_7_24(ref buffer, 64);
			f = (float)Math.Round((double)f, 8);
			tbF.Text = f.ToString("F8", cultureRu);
			tbF.Text = formatNumber(tbF.Text, 8);
			// ���������� �������
			float df = Conversions.bytes_2_signed_float_Q_7_24(ref buffer, 68);
			df = (float)Math.Round((double)df, 8);
			tbdF.Text = df.ToString("F8", cultureRu);
			tbdF.Text = formatNumber(tbdF.Text, 8);

			// ���������� ������ ������������������ U1
			float u1 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 72, true);
			u1 = (float)Math.Round((double)u1, 8);
			tbU1.Text = u1.ToString("F8", cultureRu);
			tbU1.Text = formatNumber(tbU1.Text, 8);
			// ���������� �������� ������������������ U2
			float u2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 76, true);
			u2 = (float)Math.Round((double)u2, 8);
			tbU2.Text = u2.ToString("F8", cultureRu);
			tbU2.Text = formatNumber(tbU2.Text, 8);
			// ���������� ������� ������������������ U0
			float u0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 80, true);
			u0 = (float)Math.Round((double)u0, 8);
			tbU0.Text = u0.ToString("F8", cultureRu);
			tbU0.Text = formatNumber(tbU0.Text, 8);
			// ���������� ���������� dU
			float dU = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 84, true);
			dU = (float)Math.Round((double)dU, 8);
			tbdU.Text = dU.ToString("F8", cultureRu);
			tbdU.Text = formatNumber(tbdU.Text, 8);
			// ���������� ���������� dU � ���������
			float UlinNom = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 28, true);
			float dUPer = ((u1 - UlinNom) / UlinNom) * 100;
			dUPer = (float)Math.Round((double)dUPer, 8);
			tbdUPer.Text = dUPer.ToString("F8", cultureRu);
			tbdUPer.Text = formatNumber(tbdUPer.Text, 8);
			// ����������� �������� ������������������ KU2
			float kU2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 88, true) * 100;
			kU2 = (float)Math.Round((double)kU2, 8);
			tbKU2.Text = kU2.ToString("F8", cultureRu);
			tbKU2.Text = formatNumber(tbKU2.Text, 8);
			// ����������� ������� ������������������ KU0
			float kU0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 92, true) * 100;
			kU0 = (float)Math.Round((double)kU0, 8);
			tbKU0.Text = kU0.ToString("F8", cultureRu);
			tbKU0.Text = formatNumber(tbKU0.Text, 8);
			// ��� ������ ������������������ I1
			float i1 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 96, true);
			i1 = (float)Math.Round((double)i1, 8);
			tbI1.Text = i1.ToString("F8", cultureRu);
			tbI1.Text = formatNumber(tbI1.Text, 8);
			// ��� �������� ������������������ I2
			float i2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 100, true);
			i2 = (float)Math.Round((double)i2, 8);
			tbI2.Text = i2.ToString("F8", cultureRu);
			tbI2.Text = formatNumber(tbI2.Text, 8);
			// ��� ������� ������������������ I0
			float i0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 104, true);
			i0 = (float)Math.Round((double)i0, 8);
			tbI0.Text = i0.ToString("F8", cultureRu);
			tbI0.Text = formatNumber(tbI0.Text, 8);
			// �������� ������ ������������������ P1
			float p1 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 108, true);
			p1 = (float)Math.Round((double)p1, 8);
			tbP1.Text = p1.ToString("F8", cultureRu);
			tbP1.Text = formatNumber(tbP1.Text, 8);
			// �������� �������� ������������������ P2
			float p2 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 112, true);
			p2 = (float)Math.Round((double)p2, 8);
			tbP2.Text = p2.ToString("F8", cultureRu);
			tbP2.Text = formatNumber(tbP2.Text, 8);
			// �������� ������� ����������������� P0
			float p0 = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 116, true);
			p0 = (float)Math.Round((double)p0, 8);
			tbP0.Text = p0.ToString("F8", cultureRu);
			tbP0.Text = formatNumber(tbP0.Text, 8);
			// ���� ����� ����� � ����������� ������ ������������������ UI1
			float ui1 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 120);
			ui1 = (float)Math.Round((double)ui1, 8);
			tbUI1new.Text = ui1.ToString("F8", cultureRu);
			tbUI1new.Text = formatNumber(tbUI1new.Text, 8);
			// ���� ����� ����� � ����������� �������� ������������������ UI2
			float ui2 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 124);
			ui2 = (float)Math.Round((double)ui2, 8);
			tbUI2new.Text = ui2.ToString("F8", cultureRu);
			tbUI2new.Text = formatNumber(tbUI2new.Text, 8);
			// ���� ����� ����� � ����������� ������� ������������������ UI0
			float ui0 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 128);
			ui0 = (float)Math.Round((double)ui0, 8);
			tbUI0new.Text = ui0.ToString("F8", cultureRu);
			tbUI0new.Text = formatNumber(tbUI0new.Text, 8);
			// ��������� �������� �������� P
			float p = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 132, true);
			p = (float)Math.Round((double)p, 8);
			tbPaSum.Text = p.ToString("F8", cultureRu);
			tbPaSum.Text = formatNumber(tbPaSum.Text, 8);
			// ��������� ���������� �������� Q
			float q = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 136, true);
			q = (float)Math.Round((double)q, 8);
			tbPrSum.Text = q.ToString("F8", cultureRu);
			tbPrSum.Text = formatNumber(tbPrSum.Text, 8);
			// ��������� ������ �������� S
			float s = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 140, true);
			s = (float)Math.Round((double)s, 8);
			tbPFullSum.Text = s.ToString("F8", cultureRu);
			tbPFullSum.Text = formatNumber(tbPFullSum.Text, 8);
			// ��������� ����������� �������� Kp
			float Kp = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 144, true);
			Kp = (float)Math.Round((double)Kp, 8);
			tbKPSum.Text = Kp.ToString("F8", cultureRu);
			tbKPSum.Text = formatNumber(tbKPSum.Text, 8);

			// ��������� ���� A
			// ������ ����������
			float uA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 192);
			uA = (float)Math.Round((double)uA, 8);
			tbUA.Text = uA.ToString("F8", cultureRu);
			tbUA.Text = formatNumber(tbUA.Text, 8);
			// ������ ���������� 1 ���������
			float u1A = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 196);
			u1A = (float)Math.Round((double)u1A, 8);
			tbUA1.Text = u1A.ToString("F8", cultureRu);
			tbUA1.Text = formatNumber(tbUA1.Text, 8);
			tbUA1diag.Text = formatNumber(tbUA1.Text, 8);
			// ������ ���������� ��������
			float UavA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 200);
			UavA = (float)Math.Round((double)UavA, 8);
			tbUAav.Text = UavA.ToString("F8", cultureRu);
			tbUAav.Text = formatNumber(tbUAav.Text, 8);
			// ���������� ������� ���������� �� ��������
			float dUA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 208) * 100;
			dUA = (float)Math.Round((double)dUA, 8);
			tbdUA.Text = dUA.ToString("F8", cultureRu);
			tbdUA.Text = formatNumber(tbdUA.Text, 8);
			// �������� ����������
			float uLA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 212);
			uLA = (float)Math.Round((double)uLA, 8);
			tbUAl.Text = uLA.ToString("F8", cultureRu);
			tbUAl.Text = formatNumber(tbUAl.Text, 8);
			// �������� ���������� 1 ���������
			float uL1A = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 216);
			uL1A = (float)Math.Round((double)uL1A, 8);
			tbUA1l.Text = uL1A.ToString("F8", cultureRu);
			tbUA1l.Text = formatNumber(tbUA1l.Text, 8);
			// �������� ���������� ��������
			float uLdA = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 220);
			uLdA = (float)Math.Round((double)uLdA, 8);
			tbUAavl.Text = uLdA.ToString("F8", cultureRu);
			tbUAavl.Text = formatNumber(tbUAavl.Text, 8);
			// ���������� ��������� ���������� �� ��������
			float dULA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 228) * 100;
			dULA = (float)Math.Round((double)dULA, 8);
			tbdUAl.Text = dULA.ToString("F8", cultureRu);
			tbdUAl.Text = formatNumber(tbdUAl.Text, 8);
			// ���
			float iA = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 232);
			iA = (float)Math.Round((double)iA, 8);
			tbIA.Text = iA.ToString("F8", cultureRu);
			tbIA.Text = formatNumber(tbIA.Text, 8);
			// ��� 1 ���������
			float i1A = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 236);
			i1A = (float)Math.Round((double)i1A, 8);
			tbIA1.Text = i1A.ToString("F8", cultureRu);
			tbIA1.Text = formatNumber(tbIA1.Text, 8);
			tbIA1diag.Text = formatNumber(tbIA1.Text, 8);
			// ��� ��������
			float IavA = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 240);
			IavA = (float)Math.Round((double)IavA, 8);
			tbIAav.Text = IavA.ToString("F8", cultureRu);
			tbIAav.Text = formatNumber(tbIAav.Text, 8);
			// ���������� ���� �� ��������
			float diA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 248) * 100;
			diA = (float)Math.Round((double)diA, 8);
			tbdIA.Text = diA.ToString("F8", cultureRu);
			tbdIA.Text = formatNumber(tbdIA.Text, 8);
			// ���� ����� ����� � �����������
			float aIUA = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 252);
			aIUA = (float)Math.Round((double)aIUA, 8);
			tbAIUA.Text = aIUA.ToString("F8", cultureRu);
			tbAIUA.Text = formatNumber(tbAIUA.Text, 8);
			// ���� ���������� ������
			float aPhA = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 256);
			aPhA = (float)Math.Round((double)aPhA, 8);
			tbAUPhA.Text = aPhA.ToString("F8", cultureRu);
			tbAUPhA.Text = formatNumber(tbAUPhA.Text, 8);
			// �������� ��������
			float apA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 260, true);
			apA = (float)Math.Round((double)apA, 8);
			tbPaA.Text = apA.ToString("F8", cultureRu);
			tbPaA.Text = formatNumber(tbPaA.Text, 8);
			// ���������� ��������
			float rpA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 264, true);
			rpA = (float)Math.Round((double)rpA, 8);
			tbPrA.Text = rpA.ToString("F8", cultureRu);
			tbPrA.Text = formatNumber(tbPrA.Text, 8);
			// ������ ��������
			float pFullA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 268, true);
			pFullA = (float)Math.Round((double)pFullA, 8);
			tbPFullA.Text = pFullA.ToString("F8", cultureRu);
			tbPFullA.Text = formatNumber(tbPFullA.Text, 8);
			// ����������� ��������
			float kpA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 272, true);
			kpA = (float)Math.Round((double)kpA, 8);
			tbKPA.Text = kpA.ToString("F8", cultureRu);
			tbKPA.Text = formatNumber(tbKPA.Text, 8);

			// ��������� ���� B
			// ������ ����������
			float uB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 1216);
			uB = (float)Math.Round((double)uB, 8);
			tbUB.Text = uB.ToString("F8", cultureRu);
			tbUB.Text = formatNumber(tbUB.Text, 8);
			// ������ ���������� 1 ���������
			float u1B = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 1220);
			u1B = (float)Math.Round((double)u1B, 8);
			tbUB1.Text = u1B.ToString("F8", cultureRu);
			tbUB1.Text = formatNumber(tbUB1.Text, 8);
			tbUB1diag.Text = formatNumber(tbUB1.Text, 8);
			// ������ ���������� ��������
			float UavB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 1224);
			UavB = (float)Math.Round((double)UavB, 8);
			tbUBav.Text = UavB.ToString("F8", cultureRu);
			tbUBav.Text = formatNumber(tbUBav.Text, 8);
			// ���������� ������� ���������� �� ��������
			float dUB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1232) * 100;
			dUB = (float)Math.Round((double)dUB, 8);
			tbdUB.Text = dUB.ToString("F8", cultureRu);
			tbdUB.Text = formatNumber(tbdUB.Text, 8);
			// �������� ����������
			float uLB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 1236);
			uLB = (float)Math.Round((double)uLB, 8);
			tbUBl.Text = uLB.ToString("F8", cultureRu);
			tbUBl.Text = formatNumber(tbUBl.Text, 8);
			// �������� ���������� 1 ���������
			float uL1B = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 1240);
			uL1B = (float)Math.Round((double)uL1B, 8);
			tbUB1l.Text = uL1B.ToString("F8", cultureRu);
			tbUB1l.Text = formatNumber(tbUB1l.Text, 8);
			// �������� ���������� ��������
			float uLdB = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 1244);
			uLdB = (float)Math.Round((double)uLdB, 8);
			tbUBavl.Text = uLdB.ToString("F8", cultureRu);
			tbUBavl.Text = formatNumber(tbUBavl.Text, 8);
			// ���������� ��������� ���������� �� ��������
			float dULB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1252) * 100;
			dULB = (float)Math.Round((double)dULB, 8);
			tbdUBl.Text = dULB.ToString("F8", cultureRu);
			tbdUBl.Text = formatNumber(tbdUBl.Text, 8);
			// ���
			float iB = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 1256);
			iB = (float)Math.Round((double)iB, 8);
			tbIB.Text = iB.ToString("F8", cultureRu);
			tbIB.Text = formatNumber(tbIB.Text, 8);
			// ��� 1 ���������
			float i1B = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 1260);
			i1B = (float)Math.Round((double)i1B, 8);
			tbIB1.Text = i1B.ToString("F8", cultureRu);
			tbIB1.Text = formatNumber(tbIB1.Text, 8);
			tbIB1diag.Text = formatNumber(tbIB1.Text, 8);
			// ��� ��������
			float IavB = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 1264);
			IavB = (float)Math.Round((double)IavB, 8);
			tbIBav.Text = IavB.ToString("F8", cultureRu);
			tbIBav.Text = formatNumber(tbIBav.Text, 8);
			// ���������� ���� �� ��������
			float diB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1272) * 100;
			diB = (float)Math.Round((double)diB, 8);
			tbdIB.Text = diB.ToString("F8", cultureRu);
			tbdIB.Text = formatNumber(tbdIB.Text, 8);
			// ���� ����� ����� � �����������
			float aIUB = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 1276);
			aIUB = (float)Math.Round((double)aIUB, 8);
			tbAIUB.Text = aIUB.ToString("F8", cultureRu);
			tbAIUB.Text = formatNumber(tbAIUB.Text, 8);
			// ���� ���������� ������
			float aPhB = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 1280);
			aPhB = (float)Math.Round((double)aPhB, 8);
			tbAUPhB.Text = aPhB.ToString("F8", cultureRu);
			tbAUPhB.Text = formatNumber(tbAUPhB.Text, 8);
			// �������� ��������
			float apB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 1284, true);
			apB = (float)Math.Round((double)apB, 8);
			tbPaB.Text = apB.ToString("F8", cultureRu);
			tbPaB.Text = formatNumber(tbPaB.Text, 8);
			// ���������� ��������
			float rpB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 1288, true);
			rpB = (float)Math.Round((double)rpB, 8);
			tbPrB.Text = rpB.ToString("F8", cultureRu);
			tbPrB.Text = formatNumber(tbPrB.Text, 8);
			// ������ ��������
			float pFullB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 1292, true);
			pFullB = (float)Math.Round((double)pFullB, 8);
			tbPFullB.Text = pFullB.ToString("F8", cultureRu);
			tbPFullB.Text = formatNumber(tbPFullB.Text, 8);
			// ����������� ��������
			float kpB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 1296, true);
			kpB = (float)Math.Round((double)kpB, 8);
			tbKPB.Text = kpB.ToString("F8", cultureRu);
			tbKPB.Text = formatNumber(tbKPB.Text, 8);

			// ��������� ���� C
			// ������ ����������
			float uC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 2240);
			uC = (float)Math.Round((double)uC, 8);
			tbUC.Text = uC.ToString("F8", cultureRu);
			tbUC.Text = formatNumber(tbUC.Text, 8);
			// ������ ���������� 1 ���������
			float u1C = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 2244);
			u1C = (float)Math.Round((double)u1C, 8);
			tbUC1.Text = u1C.ToString("F8", cultureRu);
			tbUC1.Text = formatNumber(tbUC1.Text, 8);
			tbUC1diag.Text = formatNumber(tbUC1.Text, 8);
			// ������ ���������� ��������
			float UavC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 2248);
			UavC = (float)Math.Round((double)UavC, 8);
			tbUCav.Text = UavC.ToString("F8", cultureRu);
			tbUCav.Text = formatNumber(tbUCav.Text, 8);
			// ���������� ������� ���������� �� ��������
			float dUC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2256) * 100;
			dUC = (float)Math.Round((double)dUC, 8);
			tbdUC.Text = dUC.ToString("F8", cultureRu);
			tbdUC.Text = formatNumber(tbdUC.Text, 8);
			// �������� ����������
			float uLC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 2260);
			uLC = (float)Math.Round((double)uLC, 8);
			tbUCl.Text = uLC.ToString("F8", cultureRu);
			tbUCl.Text = formatNumber(tbUCl.Text, 8);
			// �������� ���������� 1 ���������
			float uL1C = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 2264);
			uL1C = (float)Math.Round((double)uL1C, 8);
			tbUC1l.Text = uL1C.ToString("F8", cultureRu);
			tbUC1l.Text = formatNumber(tbUC1l.Text, 8);
			// �������� ���������� ��������
			float uLdC = Conversions.bytes_2_signed_float_Q_10_21(ref buffer, 2268);
			uLdC = (float)Math.Round((double)uLdC, 8);
			tbUCavl.Text = uLdC.ToString("F8", cultureRu);
			tbUCavl.Text = formatNumber(tbUCavl.Text, 8);
			// ���������� ��������� ���������� �� ��������
			float dULC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2276) * 100;
			dULC = (float)Math.Round((double)dULC, 8);
			tbdUCl.Text = dULC.ToString("F8", cultureRu);
			tbdUCl.Text = formatNumber(tbdUCl.Text, 8);
			// ���
			float iC = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 2280);
			iC = (float)Math.Round((double)iC, 8);
			tbIC.Text = iC.ToString("F8", cultureRu);
			tbIC.Text = formatNumber(tbIC.Text, 8);
			// ��� 1 ���������
			float i1C = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 2284);
			i1C = (float)Math.Round((double)i1C, 8);
			tbIC1.Text = i1C.ToString("F8", cultureRu);
			tbIC1.Text = formatNumber(tbIC1.Text, 8);
			tbIC1diag.Text = formatNumber(tbIC1.Text, 8);
			// ��� ��������
			float IavC = Conversions.bytes_2_unsigned_float_Q_4_27(ref buffer, 2288);
			IavC = (float)Math.Round((double)IavC, 8);
			tbICav.Text = IavC.ToString("F8", cultureRu);
			tbICav.Text = formatNumber(tbICav.Text, 8);
			// ���������� ���� �� ��������
			float diC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2296) * 100;
			diC = (float)Math.Round((double)diC, 8);
			tbdIC.Text = diC.ToString("F8", cultureRu);
			tbdIC.Text = formatNumber(tbdIC.Text, 8);
			// ���� ����� ����� � �����������
			float aIUC = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 2300);
			aIUC = (float)Math.Round((double)aIUC, 8);
			tbAIUC.Text = aIUC.ToString("F8", cultureRu);
			tbAIUC.Text = formatNumber(tbAIUC.Text, 8);
			// ���� ���������� ������
			float aPhC = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 2304);
			aPhC = (float)Math.Round((double)aPhC, 8);
			tbAUPhC.Text = aPhC.ToString("F8", cultureRu);
			tbAUPhC.Text = formatNumber(tbAUPhC.Text, 8);
			// �������� ��������
			float apC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 2308, true);
			apC = (float)Math.Round((double)apC, 8);
			tbPaC.Text = apC.ToString("F8", cultureRu);
			tbPaC.Text = formatNumber(tbPaC.Text, 8);
			// ���������� ��������
			float rpC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 2312, true);
			rpC = (float)Math.Round((double)rpC, 8);
			tbPrC.Text = rpC.ToString("F8", cultureRu);
			tbPrC.Text = formatNumber(tbPrC.Text, 8);
			// ������ ��������
			float pFullC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 2316, true);
			pFullC = (float)Math.Round((double)pFullC, 8);
			tbPFullC.Text = pFullC.ToString("F8", cultureRu);
			tbPFullC.Text = formatNumber(tbPFullC.Text, 8);
			// ����������� ��������
			float kpC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 2320, true);
			kpC = (float)Math.Round((double)kpC, 8);
			tbKPC.Text = kpC.ToString("F8", cultureRu);
			tbKPC.Text = formatNumber(tbKPC.Text, 8);

			if (p > 0)
			{
				if (q > 0)
				{
					tbKPSum.Text += "L";
					tbKPA.Text += "L";
					tbKPB.Text += "L";
					tbKPC.Text += "L";
				}
				else
				{
					tbKPSum.Text += "C";
					tbKPA.Text += "C";
					tbKPB.Text += "C";
					tbKPC.Text += "C";
				}
			}
			else
			{
				if (q > 0)
				{
					tbKPSum.Text += "C";
					tbKPA.Text += "C";
					tbKPB.Text += "C";
					tbKPC.Text += "C";
				}
				else
				{
					tbKPSum.Text += "L";
					tbKPA.Text += "L";
					tbKPB.Text += "L";
					tbKPC.Text += "L";
				}
			}

			// ������������ �������� ������� ����������
			float[] kPhUA = new float[39];
			float[] kPhUB = new float[39];
			float[] kPhUC = new float[39];
			float KuPhA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 276) * 100;
			KuPhA = (float)Math.Round((double)KuPhA, 8);
			tbKuPhA.Text = KuPhA.ToString("F8", cultureRu);
			tbKuPhA.Text = formatNumber(tbKuPhA.Text, 8);
			float KuPhB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1300) * 100;
			KuPhB = (float)Math.Round((double)KuPhB, 8);
			tbKuPhB.Text = KuPhB.ToString("F8", cultureRu);
			tbKuPhB.Text = formatNumber(tbKuPhB.Text, 8);
			float KuPhC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2324) * 100;
			KuPhC = (float)Math.Round((double)KuPhC, 8);
			tbKuPhC.Text = KuPhC.ToString("F8", cultureRu);
			tbKuPhC.Text = formatNumber(tbKuPhC.Text, 8);

			lvHarmonicsPh.Items.Clear();
			for (int i = 0; i < 39; ++i)
			{
				kPhUA[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 280 + i * 4) * 100;
				kPhUA[i] = (float)Math.Round((double)kPhUA[i], 8);
				kPhUB[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1304 + i * 4) * 100;
				kPhUB[i] = (float)Math.Round((double)kPhUB[i], 8);
				kPhUC[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2328 + i * 4) * 100;
				kPhUC[i] = (float)Math.Round((double)kPhUC[i], 8);
				ListViewItem item1 = new ListViewItem((i + 2).ToString(), 0);
				item1.SubItems.Add(formatNumber(kPhUA[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(kPhUB[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(kPhUC[i].ToString("F8", cultureRu), 6));
				lvHarmonicsPh.Items.Add(item1);
			}
			// ������������ �������� ��������� ����������
			float[] kPLUA = new float[39];
			float[] kPLUB = new float[39];
			float[] kPLUC = new float[39];
			float KuLinA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 436) * 100;
			KuLinA = (float)Math.Round((double)KuLinA, 8);
			tbKuLinA.Text = KuLinA.ToString("F8", cultureRu);
			tbKuLinA.Text = formatNumber(tbKuLinA.Text, 8);
			float KuLinB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1460) * 100;
			KuLinB = (float)Math.Round((double)KuLinB, 8);
			tbKuLinB.Text = KuLinB.ToString("F8", cultureRu);
			tbKuLinB.Text = formatNumber(tbKuLinB.Text, 8);
			float KuLinC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2484) * 100;
			KuLinC = (float)Math.Round((double)KuLinC, 8);
			tbKuLinC.Text = KuLinC.ToString("F8", cultureRu);
			tbKuLinC.Text = formatNumber(tbKuLinC.Text, 8);

			lvHarmonicsL.Items.Clear();
			for (int i = 0; i < 39; ++i)
			{
				kPLUA[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 440 + i * 4) * 100;
				kPLUA[i] = (float)Math.Round((double)kPLUA[i], 8);
				kPLUB[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1464 + i * 4) * 100;
				kPLUB[i] = (float)Math.Round((double)kPLUB[i], 8);
				kPLUC[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2488 + i * 4) * 100;
				kPLUC[i] = (float)Math.Round((double)kPLUC[i], 8);
				ListViewItem item1 = new ListViewItem((i + 2).ToString(), 0);
				item1.SubItems.Add(formatNumber(kPLUA[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(kPLUB[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(kPLUC[i].ToString("F8", cultureRu), 6));
				lvHarmonicsL.Items.Add(item1);
			}
			// ������������ �������� ����
			float[] kIA = new float[39];
			float[] kIB = new float[39];
			float[] kIC = new float[39];
			float KiA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 596) * 100;
			KiA = (float)Math.Round((double)KiA, 8);
			tbKiA.Text = KiA.ToString("F8", cultureRu);
			tbKiA.Text = formatNumber(tbKiA.Text, 8);
			float KiB = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1620) * 100;
			KiB = (float)Math.Round((double)KiB, 8);
			tbKiB.Text = KiB.ToString("F8", cultureRu);
			tbKiB.Text = formatNumber(tbKiB.Text, 8);
			float KiC = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2644) * 100;
			KiC = (float)Math.Round((double)KiC, 8);
			tbKiC.Text = KiC.ToString("F8", cultureRu);
			tbKiC.Text = formatNumber(tbKiC.Text, 8);

			lvHarmI.Items.Clear();
			for (int i = 0; i < 39; ++i)
			{
				kIA[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 600 + i * 4) * 100;
				kIA[i] = (float)Math.Round((double)kIA[i], 8);
				kIB[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 1624 + i * 4) * 100;
				kIB[i] = (float)Math.Round((double)kIB[i], 8);
				kIC[i] = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 2648 + i * 4) * 100;
				kIC[i] = (float)Math.Round((double)kIC[i], 8);
				ListViewItem item1 = new ListViewItem((i + 2).ToString(), 0);
				item1.SubItems.Add(formatNumber(kIA[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(kIB[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(kIC[i].ToString("F8", cultureRu), 6));
				lvHarmI.Items.Add(item1);
			}
			// �������� ��������
			float[] pA = new float[40];
			float[] pB = new float[40];
			float[] pC = new float[40];

			lvPHarm.Items.Clear();
			for (int i = 0; i < 40; ++i)
			{
				pA[i] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 756 + i * 4, true);
				pA[i] = (float)Math.Round((double)pA[i], 8);
				pB[i] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 1780 + i * 4, true);
				pB[i] = (float)Math.Round((double)pB[i], 8);
				pC[i] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 2804 + i * 4, true);
				pC[i] = (float)Math.Round((double)pC[i], 8);
				ListViewItem item1 = new ListViewItem((i + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(pA[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(pB[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(pC[i].ToString("F8", cultureRu), 6));
				lvPHarm.Items.Add(item1);
			}
			// ���� ��������� ��������
			float[] aphA = new float[40];
			float[] aphB = new float[40];
			float[] aphC = new float[40];

			lvAPHarm.Items.Clear();
			for (int i = 0; i < 40; ++i)
			{
				aphA[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 916 + i * 4);
				aphA[i] = (float)Math.Round((double)aphA[i], 8);
				aphB[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 1940 + i * 4);
				aphB[i] = (float)Math.Round((double)aphB[i], 8);
				aphC[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 2964 + i * 4);
				aphC[i] = (float)Math.Round((double)aphC[i], 8);
				ListViewItem item1 = new ListViewItem((i + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(aphA[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(aphB[i].ToString("F8", cultureRu), 6));
				item1.SubItems.Add(formatNumber(aphC[i].ToString("F8", cultureRu), 6));
				lvAPHarm.Items.Add(item1);
			}

			// flikker
			// A
			float flikA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 3264, true);
			flikA = (float)Math.Round((double)flikA, 8);
			tbFlikA.Text = flikA.ToString("F8", cultureRu);
			tbFlikA.Text = formatNumber(tbFlikA.Text, 8);
			// B
			float flikB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 3268, true);
			flikB = (float)Math.Round((double)flikB, 8);
			tbFlikB.Text = flikB.ToString("F8", cultureRu);
			tbFlikB.Text = formatNumber(tbFlikB.Text, 8);
			// C
			float flikC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 3272, true);
			flikC = (float)Math.Round((double)flikC, 8);
			tbFlikC.Text = flikC.ToString("F8", cultureRu);
			tbFlikC.Text = formatNumber(tbFlikC.Text, 8);

			//temperature
			short iTemper = Conversions.bytes_2_short(ref buffer, 3276);
			float fTemper = iTemper / 16;
			tbTemperature.Text = fTemper.ToString("F8", cultureRu);

			// paint diagram
			radialGraph.RadialGridList.ZeroAngle = 0;
			//existsU1 = true;

			//radialGraph.RadialGridList.ZeroAngle = 120;

			double[] modulesA = new double[2];
			double[] anglesA = new double[2];
			double[] modulesB = new double[2];
			double[] anglesB = new double[2];
			double[] modulesC = new double[2];
			double[] anglesC = new double[2];

			modulesA[0] = u1A;		// U1_A
			modulesA[1] = i1A;		// I1_A					15
			anglesA[0] = aPhA;		// <U1aU1b
			anglesA[1] = aIUA;		// <U1aI1a				-90

			modulesB[0] = u1B;		// U1_A
			modulesB[1] = i1B;		// I1_A					10
			anglesB[0] = aPhB;		// <U1aU1b
			anglesB[1] = aIUB;		// <U1aI1a				-90

			modulesC[0] = u1C;		// U1_A
			modulesC[1] = i1C;		// I1_A					20
			anglesC[0] = aPhC;		// <U1aU1b
			anglesC[1] = aIUC;		// <U1aI1a				-90

			///////////////////////////////////////////

			//modulesA[0] *= settings_.VoltageRatio;
			//modulesB[0] *= settings_.VoltageRatio;
			//modulesC[0] *= settings_.VoltageRatio;

			//modulesA[1] *= settings_.CurrentRatio;
			//modulesB[1] *= settings_.CurrentRatio;
			//modulesC[1] *= settings_.CurrentRatio;

			///////////////////////////////////////////

			double maxI = Math.Max(modulesA[1], modulesB[1]);
			maxI = Math.Max(maxI, modulesC[1]);
			double maxU = Math.Max(modulesA[0], modulesB[0]);
			maxU = Math.Max(maxU, modulesC[0]);


			radialGraph.RadialGridList.Clear();
			radialGraph.RadialGridList.Add(maxU, 1.0, 2,
						System.Drawing.Drawing2D.DashStyle.Solid);
			radialGraph.RadialGridList.Add(maxI, 0.75, 1,
						System.Drawing.Drawing2D.DashStyle.Solid);

			radialGraph.CurveList.Clear();

			// Getting Nominal voltage
			//radialGraph.RadialGridList[0].NominalValue = devInfo_.U_NominalPhase;
			radialGraph.RealValues = true;

			radialGraph.CurveList.Add(modulesA, anglesA, Color.Gold);
			radialGraph.CurveList[0].AddLegend(String.Empty);
			radialGraph.CurveList[0].AddLegend(String.Empty);

			//if (ConnectionScheme < 3)
			//{
			radialGraph.CurveList.Add(modulesB, anglesB, Color.Green);
			radialGraph.CurveList[1].AddLegend(String.Empty);
			radialGraph.CurveList[1].AddLegend(String.Empty);

			radialGraph.CurveList.Add(modulesC, anglesC, Color.Red);
			radialGraph.CurveList[2].AddLegend(String.Empty);
			radialGraph.CurveList[2].AddLegend(String.Empty);
			//}

			// setting values
			radialGraph.CurveList[0].VectorPairList[0].Module = modulesA[0];
			radialGraph.CurveList[0].VectorPairList[1].Module = modulesA[1];
			radialGraph.CurveList[0].VectorPairList[0].Angle = 0;
			radialGraph.CurveList[0].VectorPairList[1].Angle = anglesA[1];

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				radialGraph.CurveList[1].VectorPairList[0].Module = modulesB[0];
				radialGraph.CurveList[1].VectorPairList[1].Module = modulesB[1];
				radialGraph.CurveList[1].VectorPairList[0].Angle = anglesA[0];
				radialGraph.CurveList[1].VectorPairList[1].Angle = anglesA[0] +
																anglesB[1];

				radialGraph.CurveList[2].VectorPairList[0].Module = modulesC[0];
				radialGraph.CurveList[2].VectorPairList[1].Module = modulesC[1];
				radialGraph.CurveList[2].VectorPairList[0].Angle = anglesA[0] +
																anglesB[0];
				radialGraph.CurveList[2].VectorPairList[1].Angle = anglesA[0] +
														anglesB[0] + anglesC[1];
			}

			// getting ratios suffixes
			string v_suffix = string.Empty;
			//if (settings.VoltageRatio == 1)
			//	{
			v_suffix = emstrings.column_header_units_v.Remove(0, 2);
			//	}
			//	else if (settings.VoltageRatio == 0.001F)
			//	{
			//		v_suffix = emstrings.column_header_units_kv.Remove(0, 2);
			//	}

			string c_suffix = string.Empty;
			//	if (settings.CurrentRatio == 1)
			//	{
			c_suffix = emstrings.column_header_units_a.Remove(0, 2);
			//	}
			//	else if (settings.VoltageRatio == 0.001F)
			//	{
			//		c_suffix = emstrings.column_header_units_ka.Remove(0, 2);
			//	}

			// appaying format to the chart
			switch (devInfo_.ConnectionScheme)
			{
				case ConnectScheme.Ph1W2:
					radialGraph.CurveList[0].GetLegend(0).TextPattern = "Ua(1) {0}(" +
								v_suffix + ") ";
					radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
						anglesA[1].ToString("0.000") + "°; Ia(1) {0}(" + c_suffix + ")";
					break;
				case ConnectScheme.Ph3W3:
				case ConnectScheme.Ph3W3_B_calc:
					radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " +
						anglesA[0].ToString("0.000") + "°; Uab(1) {0}(" + v_suffix + ") ";
					radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " +
						anglesB[0].ToString("0.000") + "°; Ubc(1) {0}(" + v_suffix + ") ";
					radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " +
						anglesC[0].ToString("0.000") + "°; Uca(1) {0}(" + v_suffix + ") ";

					radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
						anglesA[1].ToString("0.000") + "°; Ia(1) {0}(" + c_suffix + ")";
					radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " +
						anglesB[1].ToString("0.000") + "°; Ib(1) {0}(" + c_suffix + ")";
					radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " +
						anglesC[1].ToString("0.000") + "°; Ic(1) {0}(" + c_suffix + ")";
					break;
				case ConnectScheme.Ph3W4:
				case ConnectScheme.Ph3W4_B_calc:
					radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " +
						anglesA[0].ToString("0.000") + "°; Ua(1) {0}(" + v_suffix + ") ";
					radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " +
						anglesB[0].ToString("0.000") + "°; Ub(1) {0}(" + v_suffix + ") ";
					radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " +
						anglesC[0].ToString("0.000") + "°; Uc(1) {0}(" + v_suffix + ") ";

					radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
						anglesA[1].ToString("0.000") + "°; Ia(1) {0}(" + c_suffix + ")";
					radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " +
						anglesB[1].ToString("0.000") + "°; Ib(1) {0}(" + c_suffix + ")";
					radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " +
						anglesC[1].ToString("0.000") + "°; Ic(1) {0}(" + c_suffix + ")";
					break;
			}

			radialGraph.Invalidate();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			try
			{
				ConnectionThreadAVG.Abort();
			}
			catch { }
			ConnectionThreadAVG = null;
			this.Close();
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			try
			{
				dataThread_ = new DataReceiveThread(settings_, typeAVG_, this as Form);
				dataThread_.OnDataReceived += new DataReceiveThread.DataReceivedHandler(frmMomentData_OnDataReceived);
				dataThread_.OnConnectEnd += new DataReceiveThread.ConnectEndHandler(frmMomentData_OnConnectEnd);

				ConnectionThreadAVG = new Thread(new ThreadStart(dataThread_.ThreadEntry));

				string locale = settings_.CurrentLanguage.Equals("ru") ? "ru-RU" : string.Empty;
				ConnectionThreadAVG.CurrentCulture =
					ConnectionThreadAVG.CurrentUICulture = new System.Globalization.CultureInfo(locale, false);
				if (OnProgress != null) OnProgress(true);
				ConnectionThreadAVG.Start();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in btnLoad_Click():  " + ex.Message);
				throw;
			}
		}

		private void frmMomentData_Load(object sender, EventArgs e)
		{
			this.OnProgress += new EnableProgressHandler(frmMomentData_OnProgress);

			//radialGraph.RadialGridList.ZeroAngle = 120;

			//radialGraph.RadialGridList.Add(100.0, 1.0, 2,
			//            System.Drawing.Drawing2D.DashStyle.Solid);
			//radialGraph.RadialGridList.Add(1, 0.75, 1,
						//System.Drawing.Drawing2D.DashStyle.Solid);
		}

		private void frmMomentData_OnProgress(bool enable)
		{
			if (this.InvokeRequired == false) // thread checking
			{
				progressBar.Visible = enable;
			}
			else
			{
				EnableProgressHandler pr =
					new EnableProgressHandler(frmMomentData_OnProgress);
				this.Invoke(pr, new object[] { enable });
			}
		}

		private string formatNumber(string num, int lenWhole)
		{
			lenWhole++;
			int point = num.IndexOf(',');
			if (point == -1)
				point = num.IndexOf('.');

			//if (num[0] != '-')
			//	num = num.Insert(0, " ");

			// ����� �����
			if (point == -1)
			{
				while (num.Length < lenWhole)
				{
					num = num.Insert(0, " ");
				}
			}
			else   // ����� �������
			{
				while (point < lenWhole)
				{
					num = num.Insert(0, " ");
					point = num.IndexOf(',');
					if (point == -1)
						point = num.IndexOf('.');
				}
			}
			return num;
		}

		private void rb3sec_Click(object sender, EventArgs e)
		{
			typeAVG_ = 1;
		}

		private void rb1min_Click(object sender, EventArgs e)
		{
			typeAVG_ = 2;
		}

		private void rb30min_Click(object sender, EventArgs e)
		{
			typeAVG_ = 3;
		}

		private bool ExportData()
		{
			StreamWriter sw = null;
			try
			{
				SaveFileDialog fd = new SaveFileDialog();
				fd.DefaultExt = "txt";
				fd.AddExtension = true;
				fd.FileName = "unnamed.txt";
				fd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

				if (fd.ShowDialog(this) != DialogResult.OK) return true;

				//sw = new StreamWriter(fd.FileName, false, Encoding.Default);
				sw = new StreamWriter(fd.FileName, false, Encoding.UTF8);

				sw.WriteLine("�������� ����� ����������: " + labelSerial2.Text);
				sw.WriteLine("��� �������: " + labelName2.Text);
				sw.WriteLine("����� �����������: " + labelScheme2.Text);

				if (rb3sec.Checked) sw.WriteLine("��� ����������: 3 �������");
				else if (rb1min.Checked) sw.WriteLine("��� ����������: 1 ������");
				else if (rb30min.Checked) sw.WriteLine("��� ����������: 30 �����");

				sw.WriteLine("\n\n����������� �������: " + tbFNom.Text);
				sw.WriteLine("����������� �������� ����������: " + tbUNomLin.Text);
				sw.WriteLine("����������� ������ ����������: " + tbUNomPh.Text);
				sw.WriteLine("����������� ���: " + tbINomPh.Text);


				// �������
				sw.WriteLine("\n\n�������: " + tbF.Text);
				// ���������� �������
				sw.WriteLine("���������� �������: " + tbdF.Text);
				// ���������� ������ ������������������ U1
				sw.WriteLine("���������� ������ ������������������ U1: " + tbU1.Text);
				// ���������� �������� ������������������ U2
				sw.WriteLine("���������� �������� ������������������ U2: " + tbU2.Text);
				// ���������� ������� ������������������ U0
				sw.WriteLine("���������� ������� ������������������ U0: " + tbU0.Text);
				// ���������� ���������� dU
				sw.WriteLine("���������� ���������� dU: " + tbdU.Text);
				// ���������� ���������� dU � ���������
				sw.WriteLine("���������� ���������� dU � ���������: " + tbdUPer.Text);
				// ����������� �������� ������������������ KU2
				sw.WriteLine("����������� �������� ������������������ KU2: " + tbKU2.Text);
				// ����������� ������� ������������������ KU0
				sw.WriteLine("����������� ������� ������������������ KU0: " + tbKU0.Text);
				// ��� ������ ������������������ I1
				sw.WriteLine("��� ������ ������������������ I1: " + tbI1.Text);
				// ��� �������� ������������������ I2
				sw.WriteLine("��� �������� ������������������ I2: " + tbI2.Text);
				// ��� ������� ������������������ I0
				sw.WriteLine("��� ������� ������������������ I0: " + tbI0.Text);
				// �������� ������ ������������������ P1
				sw.WriteLine("�������� ������ ������������������ P1: " + tbP1.Text);
				// �������� �������� ������������������ P2
				sw.WriteLine("�������� �������� ������������������ P2: " + tbP2.Text);
				// �������� ������� ����������������� P0
				sw.WriteLine("�������� ������� ����������������� P0: " + tbP0.Text);
				// ���� ����� ����� � ����������� ������ ������������������ UI1
				sw.WriteLine("���� ����� ����� � ����������� ������ ������������������ UI1: " + tbUI1new.Text);
				// ���� ����� ����� � ����������� �������� ������������������ UI2
				sw.WriteLine("���� ����� ����� � ����������� �������� ������������������ UI2: "
					+ tbUI2new.Text);
				// ���� ����� ����� � ����������� ������� ������������������ UI0
				sw.WriteLine("���� ����� ����� � ����������� ������� ������������������ UI0: " + tbUI0new.Text);
				// ��������� �������� �������� P
				sw.WriteLine("��������� �������� �������� P: " + tbPaSum.Text);
				// ��������� ���������� �������� Q
				sw.WriteLine("��������� ���������� �������� Q: " + tbPrSum.Text);
				// ��������� ������ �������� S
				sw.WriteLine("��������� ������ �������� S: " + tbPFullSum.Text);
				// ��������� ����������� �������� Kp
				sw.WriteLine("��������� ����������� �������� Kp: " + tbKPSum.Text);

				// ��������� ���� A
				sw.WriteLine("\n\n\n\n���� �\n");
				// ������ ����������
				sw.WriteLine("������ ����������: " + tbUA.Text);
				// ������ ���������� 1 ���������
				sw.WriteLine("������ ���������� 1 ���������: " + tbUA1.Text);
				// ������ ���������� ��������
				sw.WriteLine("������ ���������� ��������: " + tbUAav.Text);
				// ���������� ������� ���������� �� ��������
				sw.WriteLine("���������� ������� ���������� �� ��������: " + tbdUA.Text);
				// �������� ����������
				sw.WriteLine("�������� ����������: " + tbUAl.Text);
				// �������� ���������� 1 ���������
				sw.WriteLine("�������� ���������� 1 ���������: " + tbUA1l.Text);
				// �������� ���������� ��������
				sw.WriteLine("�������� ���������� ��������: " + tbUAavl.Text);
				// ���������� ��������� ���������� �� ��������
				sw.WriteLine("���������� ��������� ���������� �� ��������: " + tbdUAl.Text);
				// ���
				sw.WriteLine("���: " + tbIA.Text);
				// ��� 1 ���������
				sw.WriteLine("��� 1 ���������: " + tbIA1.Text);
				// ��� ��������
				sw.WriteLine("��� ��������: " + tbIAav.Text);
				// ���������� ���� �� ��������
				sw.WriteLine("���������� ���� �� ��������: " + tbdIA.Text);
				// ���� ����� ����� � �����������
				sw.WriteLine("���� ����� ����� � �����������: " + tbAIUA.Text);
				// ���� ���������� ������
				sw.WriteLine("���� ���������� ������: " + tbAUPhA.Text);
				// �������� ��������
				sw.WriteLine("�������� �������� P: " + tbPaA.Text);
				// ���������� ��������
				sw.WriteLine("���������� �������� Q: " + tbPrA.Text);
				// ������ ��������
				sw.WriteLine("������ �������� S: " + tbPFullA.Text);
				// ����������� ��������
				sw.WriteLine("����������� �������� Kp: " + tbKPA.Text);

				// ��������� ���� B
				sw.WriteLine("\n\n\n\n���� B\n");
				// ������ ����������
				sw.WriteLine("������ ����������: " + tbUB.Text);
				// ������ ���������� 1 ���������
				sw.WriteLine("������ ���������� 1 ���������: " + tbUB1.Text);
				// ������ ���������� ��������
				sw.WriteLine("������ ���������� ��������: " + tbUBav.Text);
				// ���������� ������� ���������� �� ��������
				sw.WriteLine("���������� ������� ���������� �� ��������: " + tbdUB.Text);
				// �������� ����������
				sw.WriteLine("�������� ����������: " + tbUBl.Text);
				// �������� ���������� 1 ���������
				sw.WriteLine("�������� ���������� 1 ���������: " + tbUB1l.Text);
				// �������� ���������� ��������
				sw.WriteLine("�������� ���������� ��������: " + tbUBavl.Text);
				// ���������� ��������� ���������� �� ��������
				sw.WriteLine("���������� ��������� ���������� �� ��������: " + tbdUBl.Text);
				// ���
				sw.WriteLine("���: " + tbIB.Text);
				// ��� 1 ���������
				sw.WriteLine("��� 1 ���������: " + tbIB1.Text);
				// ��� ��������
				sw.WriteLine("��� ��������: " + tbIBav.Text);
				// ���������� ���� �� ��������
				sw.WriteLine("���������� ���� �� ��������: " + tbdIB.Text);
				// ���� ����� ����� � �����������
				sw.WriteLine("���� ����� ����� � �����������: " + tbAIUB.Text);
				// ���� ���������� ������
				sw.WriteLine("���� ���������� ������: " + tbAUPhB.Text);
				// �������� ��������
				sw.WriteLine("�������� �������� P: " + tbPaB.Text);
				// ���������� ��������
				sw.WriteLine("���������� �������� Q: " + tbPrB.Text);
				// ������ ��������
				sw.WriteLine("������ �������� S: " + tbPFullB.Text);
				// ����������� ��������
				sw.WriteLine("����������� �������� Kp: " + tbKPB.Text);

				// ��������� ���� C
				sw.WriteLine("\n\n\n\n���� C\n");
				// ������ ����������
				sw.WriteLine("������ ����������: " + tbUC.Text);
				// ������ ���������� 1 ���������
				sw.WriteLine("������ ���������� 1 ���������: " + tbUC1.Text);
				// ������ ���������� ��������
				sw.WriteLine("������ ���������� ��������: " + tbUCav.Text);
				// ���������� ������� ���������� �� ��������
				sw.WriteLine("���������� ������� ���������� �� ��������: " + tbdUC.Text);
				// �������� ����������
				sw.WriteLine("�������� ����������: " + tbUCl.Text);
				// �������� ���������� 1 ���������
				sw.WriteLine("�������� ���������� 1 ���������: " + tbUC1l.Text);
				// �������� ���������� ��������
				sw.WriteLine("�������� ���������� ��������: " + tbUCavl.Text);
				// ���������� ��������� ���������� �� ��������
				sw.WriteLine("���������� ��������� ���������� �� ��������: " + tbdUCl.Text);
				// ���
				sw.WriteLine("���: " + tbIC.Text);
				// ��� 1 ���������
				sw.WriteLine("��� 1 ���������: " + tbIC1.Text);
				// ��� ��������
				sw.WriteLine("��� ��������: " + tbICav.Text);
				// ���������� ���� �� ��������
				sw.WriteLine("���������� ���� �� ��������: " + tbdIC.Text);
				// ���� ����� ����� � �����������
				sw.WriteLine("���� ����� ����� � �����������: " + tbAIUC.Text);
				// ���� ���������� ������
				sw.WriteLine("���� ���������� ������: " + tbAUPhC.Text);
				// �������� ��������
				sw.WriteLine("�������� �������� P: " + tbPaC.Text);
				// ���������� ��������
				sw.WriteLine("���������� �������� Q: " + tbPrC.Text);
				// ������ ��������
				sw.WriteLine("������ �������� S: " + tbPFullC.Text);
				// ����������� ��������
				sw.WriteLine("����������� �������� Kp: " + tbKPC.Text);

				// ������������ �������� ������� ����������
				sw.WriteLine("\n\n\n������������ �������� ������� ����������: ");
				sw.WriteLine("���� �: " + tbKuPhA.Text);
				sw.WriteLine("���� �: " + tbKuPhB.Text);
				sw.WriteLine("���� �: " + tbKuPhC.Text);
				for (int iRec = 0; iRec < lvHarmonicsPh.Items.Count; ++iRec)
				{
					string s = lvHarmonicsPh.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmonicsPh.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmonicsPh.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

				// ������������ �������� ��������� ����������
				sw.WriteLine("\n\n\n������������ �������� ��������� ����������: ");
				sw.WriteLine("���� �: " + tbKuLinA.Text);
				sw.WriteLine("���� �: " + tbKuLinB.Text);
				sw.WriteLine("���� �: " + tbKuLinC.Text);
				for (int iRec = 0; iRec < lvHarmonicsL.Items.Count; ++iRec)
				{
					string s = lvHarmonicsL.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmonicsL.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmonicsL.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

				// ������������ �������� ����
				sw.WriteLine("\n\n\n������������ �������� ����: ");
				sw.WriteLine("���� �: " + tbKiA.Text);
				sw.WriteLine("���� �: " + tbKiB.Text);
				sw.WriteLine("���� �: " + tbKiC.Text);
				for (int iRec = 0; iRec < lvHarmI.Items.Count; ++iRec)
				{
					string s = lvHarmI.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmI.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmI.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

				// �������� ��������
				sw.WriteLine("\n\n\n�������� ��������: ");
				for (int iRec = 0; iRec < lvPHarm.Items.Count; ++iRec)
				{
					string s = lvPHarm.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvPHarm.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvPHarm.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

				// ���� ��������� ��������
				sw.WriteLine("\n\n\n���� ��������� ��������: ");
				for (int iRec = 0; iRec < lvAPHarm.Items.Count; ++iRec)
				{
					string s = lvAPHarm.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvAPHarm.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvAPHarm.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

				// flikker
				sw.WriteLine("\n\n\n������: ");
				sw.WriteLine("���� �: " + tbFlikA.Text);
				sw.WriteLine("���� �: " + tbFlikB.Text);
				sw.WriteLine("���� �: " + tbFlikC.Text);

				MessageBoxes.ExportSuccess(this);

				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Exception in ExportData():  " + ex.Message);
				MessageBoxes.ErrorExport(this);
				return false;
			}
			finally
			{
				if (sw != null) sw.Close();
			}
		}

		private void btnExport_Click(object sender, EventArgs e)
		{
			ExportData();
		}
	}

	class DataReceiveThread
	{
		public delegate void DataReceivedHandler();
		public event DataReceivedHandler OnDataReceived;
		public delegate void ConnectEndHandler();
		public event ConnectEndHandler OnConnectEnd;
		EmDataSaver.Settings settings_;
		EmDevice device_;
		byte typeAVG_ = 1;
		Form sender_;

		public DataReceiveThread(EmDataSaver.Settings s, byte type, Form sender)
		{
			settings_ = s;
			typeAVG_ = type;
			sender_ = sender;
		}

		public void ThreadEntry()
		{
			try
			{
				object[] port_params = null;

				if (settings_.IOInterface == EmPortType.USB)
				{
					MessageBoxes.InvalidInterface(sender_, settings_.IOInterface,
										settings_.CurDeviceType.ToString());
					return;
				}

				if (settings_.IOInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				if (settings_.IOInterface == EmPortType.Modem)
				{
					port_params = new object[5];
					port_params[0] = settings_.SerialPortNameModem;
					port_params[1] = settings_.SerialSpeedModem;
					port_params[2] = settings_.CurPhoneNumber;
					port_params[3] = settings_.AttemptNumber;
					port_params[4] = settings_.CurDeviceAddress;
				}
				if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				if (settings_.IOInterface == EmPortType.GPRS)
				{
					port_params = new object[3];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
					port_params[2] = settings_.CurDeviceAddress;
				}
				if (settings_.IOInterface == EmPortType.Rs485)
				{
					port_params = new object[3];
					port_params[0] = settings_.SerialPortName485;
					port_params[1] = settings_.SerialSpeed485;
					port_params[2] = settings_.CurDeviceAddress;
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				if (settings_.CurDeviceType == EmDeviceType.EM32)
				{
					device_ = new Em32Device(settings_.IOInterface, settings_.CurDeviceAddress,
											false, port_params, (sender_ as Form).Handle);
				}
				else if (settings_.CurDeviceType == EmDeviceType.ETPQP)
				{
					device_ = new EtPQPDevice(settings_.IOInterface, settings_.CurDeviceAddress,
											false, port_params, (sender_ as Form).Handle);
				}
				else return;

				Int64 serial = device_.OpenDevice();
				if (serial == -1)
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
						settings_.IOParameters);
					return;
				}

				if (!settings_.Licences.IsLicenced(serial))
				{
					MessageBoxes.DeviceIsNotLicenced(this);
					return;
				}

				ExchangeResult errCode = ExchangeResult.Other_Error;
				lock (frmMomentData.devInfo_)
				{
					frmMomentData.devInfo_.SerialNumber = serial;
					errCode = (device_ as Em32Device).ReadMomentData(ref frmMomentData.buffer_, typeAVG_,
															ref frmMomentData.devInfo_);
				}

				// if reading was not successfull
				if (errCode != ExchangeResult.OK)
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
					return;
				}

				// ��� ������
				ushort type2 = Conversions.bytes_2_ushort(ref frmMomentData.buffer_, 0);
				if (type2 == 0)
				{
					MessageBoxes.ErrorFromDevice(this);
					EmService.WriteToLogFailed("Error:  moment data type = 0!");
					return;
				}
				//if (type2 != EmService.typeAVG)
				//{
				//    DeviceIO.EM32.CloseConnection(settings.IOInterface, settings.IOParameters);
				//    MessageBoxes.DeviceReadError(this);
				//    EmService.WriteToLogFailed("incorrect moment data type!  " +
				//        type2 + "  " + EmService.typeAVG);
				//    //return;	
				//}

				//ProcessData(ref buffer, ref devInfo);
				System.Diagnostics.Debug.WriteLine("success");
				if (OnDataReceived != null) OnDataReceived();
			}
			catch (EmDeviceEmptyException)
			{
				MessageBoxes.DeviceHasNoData(this);
				return;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in frmMomentData::ThreadEntry(): " + ex.Message);
				return;
			}
			finally
			{
				if (device_ != null) device_.Close();

				if (OnConnectEnd != null) OnConnectEnd();
				
				//if (OnProgress != null) OnProgress(false);
				//ConnectionThreadAVG = null;
			}
			Thread.Sleep(10000);
		}
	}
}