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
using NativeWifi;

namespace EnergomonitoringXP
{
	public partial class frmMomentDataEtPQP_A : Form
	{
		EmDataSaver.Settings settings_;

		// тип мгновенных значений: 1 - 3 sec, 2 - 1 min, 3 - 30 min
		private AvgTypes_PQP_A typeAVG_ = AvgTypes_PQP_A.ThreeSec;
		private object sender_;

		protected Thread ConnectionThreadAVG = null;
		DataReceiveThreadEtPQP_A dataThread_;
		public static byte[] buffer_ = null;
		public static BaseDeviceCommonInfo devInfo_ = new BaseDeviceCommonInfo();

		public delegate void EnableProgressHandler(bool enable);
		public event EnableProgressHandler OnProgress;

		public frmMomentDataEtPQP_A(EmDataSaver.Settings s, object sender)
		{
			settings_ = s;
			sender_ = sender;
			InitializeComponent();
		}

		void frmMomentData_OnDataReceived()
		{
			if (this.InvokeRequired == false) // thread checking
			{
				lock (frmMomentDataEtPQP_A.buffer_)
				{
					ProcessData(ref buffer_);
				}
			}
			else
			{
				DataReceiveThreadEtPQP_A.DataReceivedHandler received =
					new DataReceiveThreadEtPQP_A.DataReceivedHandler(frmMomentData_OnDataReceived);
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
				DataReceiveThreadEtPQP_A.ConnectEndHandler end =
					new DataReceiveThreadEtPQP_A.ConnectEndHandler(frmMomentData_OnConnectEnd);
				this.Invoke(end);
			}
		}

		private void ProcessData(ref byte[] buffer)
		{
			if (buffer == null)
			{
				EmService.WriteToLogFailed("frmMomentDataEtPQP_A.ProcessData(): buffer == null");
				return;
			}

			devInfo_.U_Limit = Conversions.bytes_2_ushort(ref buffer, 72);
			devInfo_.I_Limit = Conversions.bytes_2_ushort(ref buffer, 76);
			devInfo_.F_Limit = Conversions.bytes_2_ushort(ref buffer, 78);

			devInfo_.I_transformer_usage = Conversions.bytes_2_short(ref buffer, 88);
			devInfo_.I_transformer_primary = Conversions.bytes_2_short(ref buffer, 90);
			if (devInfo_.I_transformer_usage == 1) devInfo_.I_transformer_secondary = 1;
			else if (devInfo_.I_transformer_usage == 2) devInfo_.I_transformer_secondary = 5;

			// object name
			devInfo_.ObjectName = Conversions.bytes_2_string(ref buffer, 96, 16);
			if (devInfo_.ObjectName == "")
				devInfo_.ObjectName = "default object";

			////////////////////// show values in window ////////////////////////////
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

			float u_multiplier = 1;
			if (devInfo_.U_transformer_enable)
				u_multiplier = EtPQP_A_Device.GetUTransformerMultiplier(devInfo_.U_transformer_type);
			u_multiplier /= 1000000f;

			float i_multiplier = devInfo_.I_Limit;
			if (devInfo_.I_transformer_usage == 1 || devInfo_.I_transformer_usage == 2)
			{
				i_multiplier *= devInfo_.I_transformer_primary;
				i_multiplier /= devInfo_.I_transformer_secondary;
			}
			i_multiplier /= 1000000f;

			float p_multiplier = devInfo_.I_Limit;
			if (devInfo_.U_transformer_enable)
				p_multiplier *= EtPQP_A_Device.GetUTransformerMultiplier(devInfo_.U_transformer_type);

			if (devInfo_.I_transformer_usage == 1 || devInfo_.I_transformer_usage == 2)
			{
				p_multiplier *= devInfo_.I_transformer_primary;
				p_multiplier /= devInfo_.I_transformer_secondary;
			}
			p_multiplier /= 1000000f;

			float a_multiplier = 1f / 1000f;

			int shift = 0;

			#region F

			// частота
			float f_a = Conversions.bytes_2_uint_new(ref buffer, shift + 256) / 1000f;
			float f_b = Conversions.bytes_2_uint_new(ref buffer, shift + 260) / 1000f;
			float f_c = Conversions.bytes_2_uint_new(ref buffer, shift + 264) / 1000f;
			float f_ab = Conversions.bytes_2_uint_new(ref buffer, shift + 268) / 1000f;
			float f_bc = Conversions.bytes_2_uint_new(ref buffer, shift + 272) / 1000f;
			float f_ca = Conversions.bytes_2_uint_new(ref buffer, shift + 276) / 1000f;

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
			    devInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
			{
				SetValueToTextbox(f_a, ref tbFa);

				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					SetValueToTextbox(f_b, ref tbFb);
					SetValueToTextbox(f_c, ref tbFc);
				}
			}
			else
			{
				SetValueToTextbox(f_ab, ref tbFa);
				SetValueToTextbox(f_bc, ref tbFb);
				SetValueToTextbox(f_ca, ref tbFc);
			}

			#endregion

			#region U

			// напряжение - действующие значения
			float u_a = Conversions.bytes_2_uint_new(ref buffer, shift + 280) * u_multiplier;
			SetValueToTextbox(u_a, ref tbUA);
			
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float u_b = Conversions.bytes_2_uint_new(ref buffer, shift + 284) * u_multiplier;
				SetValueToTextbox(u_b, ref tbUB);

				float u_c = Conversions.bytes_2_uint_new(ref buffer, shift + 288) * u_multiplier;
				SetValueToTextbox(u_c, ref tbUC);
			}
			float u_ab = Conversions.bytes_2_uint_new(ref buffer, shift + 292) * u_multiplier;
			SetValueToTextbox(u_ab, ref tbUAl);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float u_bc = Conversions.bytes_2_uint_new(ref buffer, shift + 296) * u_multiplier;
				SetValueToTextbox(u_bc, ref tbUBl);

				float u_ca = Conversions.bytes_2_uint_new(ref buffer, shift + 300) * u_multiplier;
				SetValueToTextbox(u_ca, ref tbUCl);
			}

			// напряжение - средневыпрямленное значение
			float u_a_avdirect = Conversions.bytes_2_int(ref buffer, shift + 360) * u_multiplier;
			SetValueToTextbox(u_a_avdirect, ref tbUAav);
			
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float u_b_avdirect = Conversions.bytes_2_int(ref buffer, shift + 364) * u_multiplier;
				SetValueToTextbox(u_b_avdirect, ref tbUBav);

				float u_c_avdirect = Conversions.bytes_2_int(ref buffer, shift + 368) * u_multiplier;
				SetValueToTextbox(u_c_avdirect, ref tbUCav);
			}

			float u_ab_avdirect = Conversions.bytes_2_int(ref buffer, shift + 372) * u_multiplier;
			SetValueToTextbox(u_ab_avdirect, ref tbUAavl);
			
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float u_bc_avdirect = Conversions.bytes_2_int(ref buffer, shift + 376) * u_multiplier;
				SetValueToTextbox(u_bc_avdirect, ref tbUBavl);

				float u_ca_avdirect = Conversions.bytes_2_int(ref buffer, shift + 380) * u_multiplier;
				SetValueToTextbox(u_ca_avdirect, ref tbUCavl);
			}

			// напряжение - постоянная составляющая
			//float u_a_const = Conversions.bytes_2_int(ref buffer, shift + 320) * u_multiplier;
			//float u_b_const = Conversions.bytes_2_int(ref buffer, shift + 324) * u_multiplier;
			//float u_c_const = Conversions.bytes_2_int(ref buffer, shift + 328) * u_multiplier;
			//float u_ab_const = Conversions.bytes_2_int(ref buffer, shift + 332) * u_multiplier;
			//float u_bc_const = Conversions.bytes_2_int(ref buffer, shift + 336) * u_multiplier;
			//float u_ca_const = Conversions.bytes_2_int(ref buffer, shift + 340) * u_multiplier;

			// напряжение - 1-ая гармоника
			float u_a_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 400) * u_multiplier;
			SetValueToTextbox(u_a_1harm, ref tbUA1);
			tbUA1diag.Text = tbUA1.Text;
			float u_b_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 404) * u_multiplier;
			float u_c_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 408) * u_multiplier;

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				SetValueToTextbox(u_b_1harm, ref tbUB1);
				tbUB1diag.Text = tbUB1.Text;

				SetValueToTextbox(u_c_1harm, ref tbUC1);
				tbUC1diag.Text = tbUC1.Text;

				float u_ab_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 412) * u_multiplier;
				SetValueToTextbox(u_ab_1harm, ref tbUA1l);
				//tbUA1diag.Text = tbUA1l.Text;

				float u_bc_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 416) * u_multiplier;
				SetValueToTextbox(u_bc_1harm, ref tbUB1l);
				//tbUB1diag.Text = tbUB1l.Text;

				float u_ca_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 420) * u_multiplier;
				SetValueToTextbox(u_ca_1harm, ref tbUC1l);
				//tbUC1diag.Text = tbUC1l.Text;
			}

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				// Напряжение прямой последовательности 
				float u_1 = Conversions.bytes_2_int(ref buffer, shift + 544) * u_multiplier;
				SetValueToTextbox(u_1, ref tbU1);
				// Напряжение обратной последовательности
				float u_2 = Conversions.bytes_2_int(ref buffer, shift + 548) * u_multiplier;
				SetValueToTextbox(u_2, ref tbU2);
				// Напряжение нулевой последовательности
				float u_0 = Conversions.bytes_2_int(ref buffer, shift + 552) * u_multiplier;
				SetValueToTextbox(u_0, ref tbU0);
			}

			#endregion

			#region I

			// ток - действующие значения
			float i_a = Conversions.bytes_2_uint_new(ref buffer, shift + 304) * i_multiplier;
			SetValueToTextbox(i_a, ref tbIA);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float i_b = Conversions.bytes_2_uint_new(ref buffer, shift + 308) * i_multiplier;
				SetValueToTextbox(i_b, ref tbIB);

				float i_c = Conversions.bytes_2_uint_new(ref buffer, shift + 312) * i_multiplier;
				SetValueToTextbox(i_c, ref tbIC);

				//float i_n = Conversions.bytes_2_uint_new(ref buffer, shift + 316) * i_multiplier;
			}

			// ток - средневыпрямленное значение
			float i_a_avdirect = Conversions.bytes_2_int(ref buffer, shift + 384) * i_multiplier;
			SetValueToTextbox(i_a_avdirect, ref tbIAav);
			
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float i_b_avdirect = Conversions.bytes_2_int(ref buffer, shift + 388) * i_multiplier;
				SetValueToTextbox(i_b_avdirect, ref tbIBav);

				float i_c_avdirect = Conversions.bytes_2_int(ref buffer, shift + 392) * i_multiplier;
				SetValueToTextbox(i_c_avdirect, ref tbICav);

				//float i_n_avdirect = Conversions.bytes_2_int(ref buffer, shift + 396) * i_multiplier;
			}

			// ток - постоянная составляющая
			//float i_a_const = Conversions.bytes_2_int(ref buffer, shift + 344) * i_multiplier;
			//float i_b_const = Conversions.bytes_2_int(ref buffer, shift + 348) * i_multiplier;
			//float i_c_const = Conversions.bytes_2_int(ref buffer, shift + 352) * i_multiplier;
			//float i_n_const = Conversions.bytes_2_int(ref buffer, shift + 356) * i_multiplier;

			// ток - 1-ая гармоника
			float i_a_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 424) * i_multiplier;
			SetValueToTextbox(i_a_1harm, ref tbIA1);
			tbIA1diag.Text = tbIA1.Text;
			float i_b_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 428) * i_multiplier;
			float i_c_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 432) * i_multiplier;

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				SetValueToTextbox(i_b_1harm, ref tbIB1);
				tbIB1diag.Text = tbIB1.Text;

				SetValueToTextbox(i_c_1harm, ref tbIC1);
				tbIC1diag.Text = tbIC1.Text;

				//float i_n_1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 436) * i_multiplier;
			}

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				// Ток прямой последовательности
				float i_1 = Conversions.bytes_2_int(ref buffer, shift + 564) * i_multiplier;
				SetValueToTextbox(i_1, ref tbI1);
				// Ток обратной последовательности
				float i_2 = Conversions.bytes_2_int(ref buffer, shift + 568) * i_multiplier;
				SetValueToTextbox(i_2, ref tbI2);
				// Ток нулевой последовательности
				float i_0 = Conversions.bytes_2_int(ref buffer, shift + 572) * i_multiplier;
				SetValueToTextbox(i_0, ref tbI0);
			}

			#endregion

			#region Other

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				// Коэффициент обратной последовательности
				float k2u = Conversions.bytes_2_int(ref buffer, shift + 556) / 1342177.28f / 100;
				SetValueToTextbox(k2u, ref tbKU2);
				// Коэффициент нулевой последовательности
				float k0u = Conversions.bytes_2_int(ref buffer, shift + 560) / 1342177.28f / 100;
				SetValueToTextbox(k0u, ref tbKU0);
			}

			#endregion

			// Угол мощности прямой последовательности
			//float angle_p_1 = Conversions.bytes_2_int(ref buffer, shift + 588) / 1000f;
			// Угол мощности обратной последовательности
			//float angle_p_2 = Conversions.bytes_2_int(ref buffer, shift + 592) / 1000f;
			// Угол мощности нулевой последовательности
			//float angle_p_0 = Conversions.bytes_2_int(ref buffer, shift + 596) / 1000f;

			#region dU

			// Отклонение установившегося напряжения [относительное]
			float rd_u = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 600) * 100;
			SetValueToTextbox(rd_u, ref tbdUPer);
			// Отклонение 1 гармоники от номинала – фаза A [относительное]
			//float rd_u_1harm_A = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 604) * 100;
			//float rd_u_1harm_B = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 608) * 100;
			//float rd_u_1harm_C = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 612) * 100;
			//float rd_u_1harm_AB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 616) * 100;
			//float rd_u_1harm_BC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 620) * 100;
			//float rd_u_1harm_CA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 624) * 100;

			// Положительное отклонение напряжения [абсолютное]
			float d_u_pos_A = Conversions.bytes_2_int(ref buffer, shift + 628) * u_multiplier;
			SetValueToTextbox(d_u_pos_A, ref tbdUApos);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float d_u_pos_B = Conversions.bytes_2_int(ref buffer, shift + 632) * u_multiplier;
				SetValueToTextbox(d_u_pos_B, ref tbdUBpos);

				float d_u_pos_C = Conversions.bytes_2_int(ref buffer, shift + 636) * u_multiplier;
				SetValueToTextbox(d_u_pos_C, ref tbdUCpos);
			}
			// Отрицательное отклонение напряжения [абсолютное]
			float d_u_neg_A = Conversions.bytes_2_int(ref buffer, shift + 652) * u_multiplier;
			SetValueToTextbox(d_u_neg_A, ref tbdUAneg);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float d_u_neg_B = Conversions.bytes_2_int(ref buffer, shift + 656) * u_multiplier;
				SetValueToTextbox(d_u_neg_B, ref tbdUBneg);

				float d_u_neg_C = Conversions.bytes_2_int(ref buffer, shift + 660) * u_multiplier;
				SetValueToTextbox(d_u_neg_C, ref tbdUCneg);
			}
			// Положительное отклонение напряжения [относительное]
			float rd_u_pos_A = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 676) * 100;
			SetValueToTextbox(rd_u_pos_A, ref tbdUrelApos);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float rd_u_pos_B = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 680) * 100;
				SetValueToTextbox(rd_u_pos_B, ref tbdUrelBpos);
				
				float rd_u_pos_C = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 684) * 100;
				SetValueToTextbox(rd_u_pos_C, ref tbdUrelCpos);
			}
			// Отрицательное отклонение напряжения [относительное]
			float rd_u_neg_A = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 700) * 100;
			SetValueToTextbox(rd_u_neg_A, ref tbdUrelAneg);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float rd_u_neg_B = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 704) * 100;
				SetValueToTextbox(rd_u_neg_B, ref tbdUrelBneg);

				float rd_u_neg_C = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 708) * 100;
				SetValueToTextbox(rd_u_neg_C, ref tbdUrelCneg);
			}

			// Линейные отклонения напряжения
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float d_u_pos_AB = Conversions.bytes_2_int(ref buffer, shift + 640) * u_multiplier;
				SetValueToTextbox(d_u_pos_AB, ref tbdUrelABpos);

				float d_u_pos_BC = Conversions.bytes_2_int(ref buffer, shift + 644) * u_multiplier;
				SetValueToTextbox(d_u_pos_BC, ref tbdUrelBCpos);

				float d_u_pos_CA = Conversions.bytes_2_int(ref buffer, shift + 648) * u_multiplier;
				SetValueToTextbox(d_u_pos_CA, ref tbdUrelCApos);

				float d_u_neg_AB = Conversions.bytes_2_int(ref buffer, shift + 664) * u_multiplier;
				SetValueToTextbox(d_u_neg_AB, ref tbdUrelABneg);

				float d_u_neg_BC = Conversions.bytes_2_int(ref buffer, shift + 668) * u_multiplier;
				SetValueToTextbox(d_u_neg_BC, ref tbdUrelBCneg);

				float d_u_neg_CA = Conversions.bytes_2_int(ref buffer, shift + 672) * u_multiplier;
				SetValueToTextbox(d_u_neg_CA, ref tbdUrelCAneg);

				float rd_u_pos_AB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 688) * 100;
				SetValueToTextbox(rd_u_pos_AB, ref tbdUABpos);

				float rd_u_pos_BC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 692) * 100;
				SetValueToTextbox(rd_u_pos_BC, ref tbdUBCpos);

				float rd_u_pos_CA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 696) * 100;
				SetValueToTextbox(rd_u_pos_CA, ref tbdUCApos);

				float rd_u_neg_AB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 712) * 100;
				SetValueToTextbox(rd_u_neg_AB, ref tbdUABneg);
				
				float rd_u_neg_BC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 716) * 100;
				SetValueToTextbox(rd_u_neg_BC, ref tbdUBCneg);

				float rd_u_neg_CA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 720) * 100;
				SetValueToTextbox(rd_u_neg_CA, ref tbdUCAneg);
			}

			#endregion

			#region P

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				// Мощность прямой последовательности
				float p_1 = Conversions.bytes_2_int(ref buffer, shift + 576) * p_multiplier;
				SetValueToTextbox(p_1, ref tbP1);
				// Мощность обратной последовательности
				float p_2 = Conversions.bytes_2_int(ref buffer, shift + 580) * p_multiplier;
				SetValueToTextbox(p_2, ref tbP2);
				// Мощность нулевой последовательности
				float p_0 = Conversions.bytes_2_int(ref buffer, shift + 584) * p_multiplier;
				SetValueToTextbox(p_0, ref tbP0);
			}

			// Мощность активная
			float p_a = Conversions.bytes_2_int(ref buffer, shift + 440) * p_multiplier;
			SetValueToTextbox(p_a, ref tbPa);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float p_b = Conversions.bytes_2_int(ref buffer, shift + 444) * p_multiplier;
				float p_c = Conversions.bytes_2_int(ref buffer, shift + 448) * p_multiplier;
				float p_sum = Conversions.bytes_2_int(ref buffer, shift + 452) * p_multiplier;
				if (devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
				    devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					p_sum = Conversions.bytes_2_int(ref buffer, shift + 464) * p_multiplier;
					p_a = Conversions.bytes_2_int(ref buffer, shift + 456) * p_multiplier;
					SetValueToTextbox(p_a, ref tbPa);
					p_b = Conversions.bytes_2_int(ref buffer, shift + 460) * p_multiplier;
					labelPA.Text = "1";
					labelPB.Text = "2";
					labelPC.Text = "";
				}
				else
				{
					labelPA.Text = "A";
					labelPB.Text = "B";
					labelPC.Text = "C";

					SetValueToTextbox(p_c, ref tbPc);
				}

				SetValueToTextbox(p_b, ref tbPb);
				SetValueToTextbox(p_sum, ref tbPsum);

				//float p_1 = Conversions.bytes_2_int(ref buffer, shift + 456) * p_multiplier;
				//float p_2 = Conversions.bytes_2_int(ref buffer, shift + 460) * p_multiplier;
			}

			// Мощность полная
			float s_a = Conversions.bytes_2_int(ref buffer, shift + 468) * p_multiplier;
			SetValueToTextbox(s_a, ref tbSa);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float s_b = Conversions.bytes_2_int(ref buffer, shift + 472) * p_multiplier;
				float s_c = Conversions.bytes_2_int(ref buffer, shift + 476) * p_multiplier;
				float s_sum = Conversions.bytes_2_int(ref buffer, shift + 480) * p_multiplier;
				if (devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
				    devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					s_sum = Conversions.bytes_2_int(ref buffer, shift + 492) * p_multiplier;
					s_a = Conversions.bytes_2_int(ref buffer, shift + 484) * p_multiplier;
					SetValueToTextbox(s_a, ref tbSa);
					s_b = Conversions.bytes_2_int(ref buffer, shift + 488) * p_multiplier;
				}
				else
				{
					SetValueToTextbox(s_c, ref tbSc);
				}

				SetValueToTextbox(s_b, ref tbSb);
				SetValueToTextbox(s_sum, ref tbSsum);

				//float s_1 = Conversions.bytes_2_int(ref buffer, shift + 484) * p_multiplier;
				//float s_2 = Conversions.bytes_2_int(ref buffer, shift + 488) * p_multiplier;
			}

			// Мощность реактивная (по первой гармонике)
			float q_a = Conversions.bytes_2_int(ref buffer, shift + 496) * p_multiplier;
			SetValueToTextbox(q_a, ref tbQa);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float q_b = Conversions.bytes_2_int(ref buffer, shift + 500) * p_multiplier;
				float q_c = Conversions.bytes_2_int(ref buffer, shift + 504) * p_multiplier;
				float q_sum = Conversions.bytes_2_int(ref buffer, shift + 508) * p_multiplier;

				if (devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
				    devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					q_sum = Conversions.bytes_2_int(ref buffer, shift + 520) * p_multiplier;
					q_a = Conversions.bytes_2_int(ref buffer, shift + 512) * p_multiplier;
					SetValueToTextbox(q_a, ref tbQa);
					q_b = Conversions.bytes_2_int(ref buffer, shift + 516) * p_multiplier;
				}
				else
				{
					SetValueToTextbox(q_c, ref tbQc);
				}

				SetValueToTextbox(q_b, ref tbQb);
				SetValueToTextbox(q_sum, ref tbQsum);
			}

			//float q_1 = Conversions.bytes_2_int(ref buffer, shift + 512) * p_multiplier;
			//float q_2 = Conversions.bytes_2_int(ref buffer, shift + 516) * p_multiplier;

			// Коэффициент мощности Kp
			float kp_a = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 524);
			SetValueToTextbox(kp_a, ref tbKPA);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float kp_b = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 528);
				SetValueToTextbox(kp_b, ref tbKPB);

				float kp_c = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 532);
				SetValueToTextbox(kp_c, ref tbKPC);

				float kp_sum = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 536);
				if (devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
				    devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					kp_sum = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 540);
				}
				SetValueToTextbox(kp_sum, ref tbKpSum);
			}

			#endregion

			#region Angles

			// Между фазным напряжением UA и током IA
			float angle_ua_ia = Conversions.bytes_2_int(ref buffer, shift + 736) * a_multiplier;
			if (angle_ua_ia > 360 || angle_ua_ia < -360)
			{
				tbAngleIUA.Text = "-";
				angle_ua_ia = 0;
			}
			else SetValueToTextbox(angle_ua_ia, ref tbAngleIUA);

			float angle_ub_ib = Conversions.bytes_2_int(ref buffer, shift + 740) * a_multiplier;
			float angle_uc_ic = Conversions.bytes_2_int(ref buffer, shift + 744) * a_multiplier;
			float angle_ua_ub = Conversions.bytes_2_int(ref buffer, shift + 724) * a_multiplier;
			float angle_ub_uc = Conversions.bytes_2_int(ref buffer, shift + 728) * a_multiplier;
			float angle_uc_ua = Conversions.bytes_2_int(ref buffer, shift + 732) * a_multiplier;

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				if (angle_ub_ib > 360 || angle_ub_ib < -360)
				{
					tbAngleIUB.Text = "-";
					angle_ub_ib = 0;
				}
				else SetValueToTextbox(angle_ub_ib, ref tbAngleIUB);
				if (angle_uc_ic > 360 || angle_uc_ic < -360)
				{
					tbAngleIUC.Text = "-";
					angle_uc_ic = 0;
				}
				else SetValueToTextbox(angle_uc_ic, ref tbAngleIUC);

				// Между фазными напряжениями UA UB
				if (angle_ua_ub > 360 || angle_ua_ub < -360)
				{
					tbAngleUaUb.Text = "-";
					angle_ua_ub = 0;
				}
				else SetValueToTextbox(angle_ua_ub, ref tbAngleUaUb);
				if (angle_ub_uc > 360 || angle_ub_uc < -360)
				{
					tbAngleUbUc.Text = "-";
					angle_ub_uc = 0;
				}
				else SetValueToTextbox(angle_ub_uc, ref tbAngleUbUc);
				if (angle_uc_ua > 360 || angle_uc_ua < -360)
				{
					tbAngleUcUa.Text = "-";
					angle_uc_ua = 0;
				}
				else SetValueToTextbox(angle_uc_ua, ref tbAngleUcUa);
			}

			#endregion

			#region U phase harmonics

			lvHarmonicsPh.Items.Clear();
			lvHarmonicsPhOrders.Items.Clear();

			shift = 0;
			// Коэффициенты гармоник фазного напряжения ///////////////////////////////////////////////
			System.Globalization.CultureInfo cultureRu = new System.Globalization.CultureInfo("ru-RU");
			// Суммарное значение для гармонических подгрупп порядка > 1
			float summ_for_order_more_1_a = Conversions.bytes_2_int(ref buffer, shift + 880) * u_multiplier;
			SetValueToTextbox(summ_for_order_more_1_a, ref tbGrgSumA);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float summ_for_order_more_1_b = Conversions.bytes_2_int(ref buffer, shift + 1284) * u_multiplier;
				SetValueToTextbox(summ_for_order_more_1_b, ref tbGrgSumB);
				float summ_for_order_more_1_c = Conversions.bytes_2_int(ref buffer, shift + 1688) * u_multiplier;
				SetValueToTextbox(summ_for_order_more_1_c, ref tbGrgSumC);
			}
			// Значение для порядка = 1, Значения для порядков 2…50
			for (int iOrder = 0; iOrder < 50; ++iOrder)
			{
				float order_value_a =
					Conversions.bytes_2_int(ref buffer, shift + 884 + iOrder * 4) * u_multiplier;
				order_value_a = (float)Math.Round((double)order_value_a, 6);
				float order_value_b =
						Conversions.bytes_2_int(ref buffer, shift + 1288 + iOrder * 4) * u_multiplier;
				order_value_b = (float)Math.Round((double)order_value_b, 6);
				float order_value_c =
						Conversions.bytes_2_int(ref buffer, shift + 1692 + iOrder * 4) * u_multiplier;
				order_value_c = (float)Math.Round((double)order_value_c, 6);

				ListViewItem item1 = new ListViewItem((iOrder + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(order_value_a.ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(order_value_b.ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(order_value_c.ToString("F6", cultureRu), 6));
				}
				lvHarmonicsPh.Items.Add(item1);
			}

			// Суммарный коэффициент, Коэффициенты для порядков 2…50
			float thds_a = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1084) * 100;
			SetValueToTextbox(thds_a, ref tbThdsA);
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float thds_b = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1488) * 100;
				SetValueToTextbox(thds_b, ref tbThdsB);
				float thds_c = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1892) * 100;
				SetValueToTextbox(thds_c, ref tbThdsC);
			}

			for (int iOrder = 1; iOrder < 50; ++iOrder)
			{
				float order_coeff_a = 
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1084 + iOrder * 4) * 100;
				order_coeff_a = (float)Math.Round((double)order_coeff_a, 6);
				float order_coeff_b =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1488 + iOrder * 4) * 100;
				order_coeff_b = (float)Math.Round((double)order_coeff_b, 6);
				float order_coeff_c =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 1892 + iOrder * 4) * 100;
				order_coeff_c = (float)Math.Round((double)order_coeff_c, 6);

				ListViewItem item1 = new ListViewItem((iOrder + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(order_coeff_a.ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(order_coeff_b.ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(order_coeff_c.ToString("F6", cultureRu), 6));
				}
				lvHarmonicsPhOrders.Items.Add(item1);
			}

			#endregion

			#region U lin harmonics

			lvHarmonicsLin.Items.Clear();
			lvHarmonicsLinOrders.Items.Clear();

			// Коэффициенты гармоник линейного напряжения //////////////////////////////////
			// Суммарное значение для гармонических подгрупп порядка > 1
			summ_for_order_more_1_a = Conversions.bytes_2_int(ref buffer, shift + 2092) * u_multiplier;
			SetValueToTextbox(summ_for_order_more_1_a, ref tbGrgSumAB);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float summ_for_order_more_1_b = Conversions.bytes_2_int(ref buffer, shift + 2496) * u_multiplier;
				SetValueToTextbox(summ_for_order_more_1_b, ref tbGrgSumBC);
				float summ_for_order_more_1_c = Conversions.bytes_2_int(ref buffer, shift + 2900) * u_multiplier;
				SetValueToTextbox(summ_for_order_more_1_c, ref tbGrgSumCA);
			}
			// Значение для порядка = 1, Значения для порядков 2…50
			for (int iOrder = 0; iOrder < 50; ++iOrder)
			{
				float order_value_a =
					Conversions.bytes_2_int(ref buffer, shift + 2096 + iOrder * 4) * u_multiplier;
				order_value_a = (float)Math.Round((double)order_value_a, 6);
				float order_value_b =
						Conversions.bytes_2_int(ref buffer, shift + 2500 + iOrder * 4) * u_multiplier;
				order_value_b = (float)Math.Round((double)order_value_b, 6);
				float order_value_c =
						Conversions.bytes_2_int(ref buffer, shift + 2904 + iOrder * 4) * u_multiplier;
				order_value_c = (float)Math.Round((double)order_value_c, 6);

				ListViewItem item1 = new ListViewItem((iOrder + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(order_value_a.ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(order_value_b.ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(order_value_c.ToString("F6", cultureRu), 6));
				}
				lvHarmonicsLin.Items.Add(item1);
			}

			// Суммарный коэффициент, Коэффициенты для порядков 2…50
			thds_a = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 2296) * 100;
			SetValueToTextbox(thds_a, ref tbThdsAB);
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float thds_b = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 2700) * 100;
				SetValueToTextbox(thds_b, ref tbThdsBC);
				float thds_c = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3104) * 100;
				SetValueToTextbox(thds_c, ref tbThdsCA);
			}

			for (int iOrder = 1; iOrder < 50; ++iOrder)
			{
				float order_coeff_a =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 2296 + iOrder * 4) * 100;
				order_coeff_a = (float)Math.Round((double)order_coeff_a, 6);
				float order_coeff_b =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 2700 + iOrder * 4) * 100;
				order_coeff_b = (float)Math.Round((double)order_coeff_b, 6);
				float order_coeff_c =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3104 + iOrder * 4) * 100;
				order_coeff_c = (float)Math.Round((double)order_coeff_c, 6);

				ListViewItem item1 = new ListViewItem((iOrder + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(order_coeff_a.ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(order_coeff_b.ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(order_coeff_c.ToString("F6", cultureRu), 6));
				}
				lvHarmonicsLinOrders.Items.Add(item1);
			}

			#endregion

			#region I harmonics

			lvHarmonicsI.Items.Clear();
			lvHarmonicsIOrders.Items.Clear();

			// Гармоники тока //////////////////////////////////////////////////////
			// Суммарное значение для гармонических подгрупп порядка > 1
			summ_for_order_more_1_a = Conversions.bytes_2_int(ref buffer, shift + 3304) * u_multiplier;
			SetValueToTextbox(summ_for_order_more_1_a, ref tbGrgSumIA);

			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float summ_for_order_more_1_b = Conversions.bytes_2_int(ref buffer, shift + 3708) * u_multiplier;
				SetValueToTextbox(summ_for_order_more_1_b, ref tbGrgSumIB);
				float summ_for_order_more_1_c = Conversions.bytes_2_int(ref buffer, shift + 4112) * u_multiplier;
				SetValueToTextbox(summ_for_order_more_1_c, ref tbGrgSumIC);
			}
			// Значение для порядка = 1, Значения для порядков 2…50
			for (int iOrder = 0; iOrder < 50; ++iOrder)
			{
				float order_value_a =
					Conversions.bytes_2_int(ref buffer, shift + 3308 + iOrder * 4) * u_multiplier;
				order_value_a = (float)Math.Round((double)order_value_a, 6);
				float order_value_b =
						Conversions.bytes_2_int(ref buffer, shift + 3712 + iOrder * 4) * u_multiplier;
				order_value_b = (float)Math.Round((double)order_value_b, 6);
				float order_value_c =
						Conversions.bytes_2_int(ref buffer, shift + 4116 + iOrder * 4) * u_multiplier;
				order_value_c = (float)Math.Round((double)order_value_c, 6);

				ListViewItem item1 = new ListViewItem((iOrder + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(order_value_a.ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(order_value_b.ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(order_value_c.ToString("F6", cultureRu), 6));
				}
				lvHarmonicsI.Items.Add(item1);
			}

			// Суммарный коэффициент, Коэффициенты для порядков 2…50
			thds_a = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3508) * 100;
			SetValueToTextbox(thds_a, ref tbThdsIA);
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				float thds_b = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3912) * 100;
				SetValueToTextbox(thds_b, ref tbThdsIB);
				float thds_c = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 4316) * 100;
				SetValueToTextbox(thds_c, ref tbThdsIC);
			}

			for (int iOrder = 1; iOrder < 50; ++iOrder)
			{
				float order_coeff_a =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3508 + iOrder * 4) * 100;
				order_coeff_a = (float)Math.Round((double)order_coeff_a, 6);
				float order_coeff_b =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 3912 + iOrder * 4) * 100;
				order_coeff_b = (float)Math.Round((double)order_coeff_b, 6);
				float order_coeff_c =
					Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 4316 + iOrder * 4) * 100;
				order_coeff_c = (float)Math.Round((double)order_coeff_c, 6);

				ListViewItem item1 = new ListViewItem((iOrder + 1).ToString(), 0);
				item1.SubItems.Add(formatNumber(order_coeff_a.ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(order_coeff_b.ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(order_coeff_c.ToString("F6", cultureRu), 6));
				}
				lvHarmonicsIOrders.Items.Add(item1);
			}

			#endregion

			#region U phase interharmonics

			lvInterharmUph.Items.Clear();

			// Инергармоники напряжения ///////////////////////////////////////////
			// Среднеквадратическое значение интергармонических групп порядков 0…50
			float[] avg_square_order_a = new float[51];
			float[] avg_square_order_b = new float[51];
			float[] avg_square_order_c = new float[51];
			for (int iOrder = 0; iOrder < 51; ++iOrder)
			{
				avg_square_order_a[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 4920 + iOrder * 4) * u_multiplier;
		
				avg_square_order_b[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 5124 + iOrder * 4) * u_multiplier;
			
				avg_square_order_c[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 5328 + iOrder * 4) * u_multiplier;

				ListViewItem item1 = new ListViewItem(iOrder.ToString(), 0);
				item1.SubItems.Add(formatNumber(avg_square_order_a[iOrder].ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(avg_square_order_b[iOrder].ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(avg_square_order_c[iOrder].ToString("F6", cultureRu), 6));
				}
				lvInterharmUph.Items.Add(item1);
			}

			#endregion

			#region U lin interharmonics

			lvInterharmUlin.Items.Clear();

			// Инергармоники напряжения ///////////////////////////////////////////
			// Среднеквадратическое значение интергармонических групп порядков 0…50
			for (int iOrder = 0; iOrder < 51; ++iOrder)
			{
				avg_square_order_a[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 5532 + iOrder * 4) * u_multiplier;

				avg_square_order_b[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 5736 + iOrder * 4) * u_multiplier;

				avg_square_order_c[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 5940 + iOrder * 4) * u_multiplier;

				ListViewItem item1 = new ListViewItem(iOrder.ToString(), 0);
				item1.SubItems.Add(formatNumber(avg_square_order_a[iOrder].ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(avg_square_order_b[iOrder].ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(avg_square_order_c[iOrder].ToString("F6", cultureRu), 6));
				}
				lvInterharmUlin.Items.Add(item1);
			}

			#endregion

			#region I interharmonics

			lvInterharmI.Items.Clear();

			// Инергармоники напряжения ///////////////////////////////////////////
			// Среднеквадратическое значение интергармонических групп порядков 0…50
			for (int iOrder = 0; iOrder < 51; ++iOrder)
			{
				avg_square_order_a[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 6144 + iOrder * 4) * u_multiplier;

				avg_square_order_b[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 6348 + iOrder * 4) * u_multiplier;

				avg_square_order_c[iOrder] =
					Conversions.bytes_2_int(ref buffer, shift + 6552 + iOrder * 4) * u_multiplier;

				ListViewItem item1 = new ListViewItem(iOrder.ToString(), 0);
				item1.SubItems.Add(formatNumber(avg_square_order_a[iOrder].ToString("F6", cultureRu), 6));
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					item1.SubItems.Add(formatNumber(avg_square_order_b[iOrder].ToString("F6", cultureRu), 6));
					item1.SubItems.Add(formatNumber(avg_square_order_c[iOrder].ToString("F6", cultureRu), 6));
				}
				lvInterharmI.Items.Add(item1);
			}

			#endregion

			#region Not used

			// Мощности гармоник
			//float[] pA = new float[40];
			//float[] pB = new float[40];
			//float[] pC = new float[40];

			//lvPHarm.Items.Clear();
			//for (int i = 0; i < 40; ++i)
			//{
			//    pA[i] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 756 + i * 4, true);
			//    pA[i] = (float)Math.Round((double)pA[i], 8);
			//    pB[i] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 1780 + i * 4, true);
			//    pB[i] = (float)Math.Round((double)pB[i], 8);
			//    pC[i] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 2804 + i * 4, true);
			//    pC[i] = (float)Math.Round((double)pC[i], 8);
			//    ListViewItem item1 = new ListViewItem((i + 1).ToString(), 0);
			//    item1.SubItems.Add(formatNumber(pA[i].ToString("F6", cultureRu), 6));
			//    item1.SubItems.Add(formatNumber(pB[i].ToString("F6", cultureRu), 6));
			//    item1.SubItems.Add(formatNumber(pC[i].ToString("F6", cultureRu), 6));
			//    lvPHarm.Items.Add(item1);
			//}
			// Углы мощностей гармоник
			//float[] aphA = new float[40];
			//float[] aphB = new float[40];
			//float[] aphC = new float[40];

			//lvAPHarm.Items.Clear();
			//for (int i = 0; i < 40; ++i)
			//{
			//    aphA[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 916 + i * 4);
			//    aphA[i] = (float)Math.Round((double)aphA[i], 8);
			//    aphB[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 1940 + i * 4);
			//    aphB[i] = (float)Math.Round((double)aphB[i], 8);
			//    aphC[i] = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 2964 + i * 4);
			//    aphC[i] = (float)Math.Round((double)aphC[i], 8);
			//    ListViewItem item1 = new ListViewItem((i + 1).ToString(), 0);
			//    item1.SubItems.Add(formatNumber(aphA[i].ToString("F6", cultureRu), 6));
			//    item1.SubItems.Add(formatNumber(aphB[i].ToString("F6", cultureRu), 6));
			//    item1.SubItems.Add(formatNumber(aphC[i].ToString("F6", cultureRu), 6));
			//    lvAPHarm.Items.Add(item1);
			//}

			// flikker
			// A
			//float flikA = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 3264, true);
			//flikA = (float)Math.Round((double)flikA, 8);
			//tbFlikA.Text = flikA.ToString("F6", cultureRu);
			//tbFlikA.Text = formatNumber(tbFlikA.Text, 8);
			//// B
			//float flikB = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 3268, true);
			//flikB = (float)Math.Round((double)flikB, 8);
			//tbFlikB.Text = flikB.ToString("F6", cultureRu);
			//tbFlikB.Text = formatNumber(tbFlikB.Text, 8);
			//// C
			//float flikC = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 3272, true);
			//flikC = (float)Math.Round((double)flikC, 8);
			//tbFlikC.Text = flikC.ToString("F6", cultureRu);
			//tbFlikC.Text = formatNumber(tbFlikC.Text, 8);

			//temperature
			//short iTemper = Conversions.bytes_2_short(ref buffer, 3276);
			//float fTemper = iTemper / 16;
			//tbTemperature.Text = fTemper.ToString("F6", cultureRu);

			// отклонение частоты
			//float df = Conversions.bytes_2_signed_float_Q_7_24(ref buffer, 68);
			//df = (float)Math.Round((double)df, 8);
			//tbdF.Text = df.ToString("F6", cultureRu);
			//tbdF.Text = formatNumber(tbdF.Text, 8);
			// Угол между током и напряжением прямой последовательности UI1
			//float ui1 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 120);
			//ui1 = (float)Math.Round((double)ui1, 8);
			//tbUI1new.Text = ui1.ToString("F6", cultureRu);
			//tbUI1new.Text = formatNumber(tbUI1new.Text, 8);
			//// Угол между током и напряжением обратной последовательности UI2
			//float ui2 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 124);
			//ui2 = (float)Math.Round((double)ui2, 8);
			//tbUI2new.Text = ui2.ToString("F6", cultureRu);
			//tbUI2new.Text = formatNumber(tbUI2new.Text, 8);
			//// Угол между током и напряжением нулевой последовательности UI0
			//float ui0 = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 128);
			//ui0 = (float)Math.Round((double)ui0, 8);
			//tbUI0new.Text = ui0.ToString("F6", cultureRu);
			//tbUI0new.Text = formatNumber(tbUI0new.Text, 8);

			// Параметры фазы A
			// Отклонение тока от номинала
			//float diA = Conversions.bytes_2_signed_float_Q_0_31(ref buffer, 248) * 100;
			//diA = (float)Math.Round((double)diA, 8);
			//tbdIA.Text = diA.ToString("F6", cultureRu);
			//tbdIA.Text = formatNumber(tbdIA.Text, 8);
			// Угол напряжения фазный
			//float aPhA = Conversions.bytes_2_signed_float_Q_8_23(ref buffer, 256);
			//aPhA = (float)Math.Round((double)aPhA, 8);
			//tbAUPhA.Text = aPhA.ToString("F6", cultureRu);
			//tbAUPhA.Text = formatNumber(tbAUPhA.Text, 8);

			/*if (p_sum > 0)
			{
				if (q_sum > 0)
				{
					tbKpSum.Text += "L";
					tbKPA.Text += "L";
					tbKPB.Text += "L";
					tbKPC.Text += "L";
				}
				else
				{
					tbKpSum.Text += "C";
					tbKPA.Text += "C";
					tbKPB.Text += "C";
					tbKPC.Text += "C";
				}
			}
			else
			{
				if (q_sum > 0)
				{
					tbKpSum.Text += "C";
					tbKPA.Text += "C";
					tbKPB.Text += "C";
					tbKPC.Text += "C";
				}
				else
				{
					tbKpSum.Text += "L";
					tbKPA.Text += "L";
					tbKPB.Text += "L";
					tbKPC.Text += "L";
				}
			}*/

			#endregion

			// paint diagram
			radialGraph.RadialGridList.ZeroAngle = 0;
			//existsU1 = true;

			//radialGraph.RadialGridList.ZeroAngle = 120;
			if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				double[] modulesA = new double[2];
				double[] anglesA = new double[2];
				double[] modulesB = new double[2];
				double[] anglesB = new double[2];
				double[] modulesC = new double[2];
				double[] anglesC = new double[2];

				modulesA[0] = u_a_1harm;	// U1_A
				modulesA[1] = i_a_1harm;	// I1_A					15
				anglesA[0] = angle_ua_ub;	// <U1aU1b
				anglesA[1] = angle_ua_ia;	// <U1aI1a				-90

				modulesB[0] = u_b_1harm;
				modulesB[1] = i_b_1harm;
				anglesB[0] = angle_ub_uc;
				anglesB[1] = angle_ub_ib;

				modulesC[0] = u_c_1harm;
				modulesC[1] = i_c_1harm;
				anglesC[0] = angle_uc_ua;	
				anglesC[1] = angle_uc_ic;

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
						radialGraph.CurveList[0].GetLegend(0).TextPattern = "Ua(1) {0}(" + v_suffix + ") ";
						radialGraph.CurveList[0].GetLegend(1).TextPattern = 
							"<UaIa " + anglesA[1].ToString("0.000") + "В°; Ia(1) {0}(" + c_suffix + ")";
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " + 
							anglesA[0].ToString("0.000") + "В°; Uab(1) {0}(" + v_suffix + ") ";
						radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " + 
							anglesB[0].ToString("0.000") + "В°; Ubc(1) {0}(" + v_suffix + ") ";
						radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " +
							anglesC[0].ToString("0.000") + "В°; Uca(1) {0}(" + v_suffix + ") ";

						radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
							anglesA[1].ToString("0.000") + "В°; Ia(1) {0}(" + c_suffix + ")";
						radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " +
							anglesB[1].ToString("0.000") + "В°; Ib(1) {0}(" + c_suffix + ")";
						radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " +
							anglesC[1].ToString("0.000") + "В°; Ic(1) {0}(" + c_suffix + ")";
						break;
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						radialGraph.CurveList[0].GetLegend(0).TextPattern = "<UaUb " +
							anglesA[0].ToString("0.000") + "В°; Ua(1) {0}(" + v_suffix + ") ";
						radialGraph.CurveList[1].GetLegend(0).TextPattern = "<UbUc " +
							anglesB[0].ToString("0.000") + "В°; Ub(1) {0}(" + v_suffix + ") ";
						radialGraph.CurveList[2].GetLegend(0).TextPattern = "<UcUa " +
							anglesC[0].ToString("0.000") + "В°; Uc(1) {0}(" + v_suffix + ") ";

						radialGraph.CurveList[0].GetLegend(1).TextPattern = "<UaIa " +
							anglesA[1].ToString("0.000") + "В°; Ia(1) {0}(" + c_suffix + ")";
						radialGraph.CurveList[1].GetLegend(1).TextPattern = "<UbIb " +
							anglesB[1].ToString("0.000") + "В°; Ib(1) {0}(" + c_suffix + ")";
						radialGraph.CurveList[2].GetLegend(1).TextPattern = "<UcIc " +
							anglesC[1].ToString("0.000") + "В°; Ic(1) {0}(" + c_suffix + ")";
						break;
				}

				radialGraph.Invalidate();
			}
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
				dataThread_ = new DataReceiveThreadEtPQP_A(settings_, typeAVG_, this as Form);
				dataThread_.OnDataReceived += new DataReceiveThreadEtPQP_A.DataReceivedHandler(frmMomentData_OnDataReceived);
				dataThread_.OnConnectEnd += new DataReceiveThreadEtPQP_A.ConnectEndHandler(frmMomentData_OnConnectEnd);

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

			// число целое
			if (point == -1)
			{
				while (num.Length < lenWhole)
				{
					num = num.Insert(0, " ");
				}
			}
			else   // число дробное
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
			typeAVG_ = AvgTypes_PQP_A.ThreeSec;
		}

		private void rb10min_Click(object sender, EventArgs e)
		{
			typeAVG_ = AvgTypes_PQP_A.TenMin;
		}

		private void rb2hour_Click(object sender, EventArgs e)
		{
			typeAVG_ = AvgTypes_PQP_A.TwoHours;
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

				sw.WriteLine("Серийный номер устройства: " + labelSerial2.Text);
				sw.WriteLine("Имя объекта: " + labelName2.Text);
				sw.WriteLine("Схема подключения: " + labelScheme2.Text);

				if (rb3sec.Checked) sw.WriteLine("Тип усреднения: 3 секунды");
				else if (rb10min.Checked) sw.WriteLine("Тип усреднения: 10 минут");
				else if (rb2hour.Checked) sw.WriteLine("Тип усреднения: 2 часа");

				//sw.WriteLine("\n\nНоминальная частота: " + tbFNom.Text);
				sw.WriteLine("Номинальное линейное напряжение: " + tbUNomLin.Text);
				sw.WriteLine("Номинальное фазное напряжение: " + tbUNomPh.Text);
				//sw.WriteLine("Номинальный ток: " + tbINomPh.Text);

				// частота
				sw.WriteLine("\n\nЧастота по фазе A: " + tbFa.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Частота по фазе B: " + tbFb.Text);
					sw.WriteLine("Частота по фазе C: " + tbFc.Text);
				}
				sw.WriteLine("");

				// Напряжение
				// Фазное напряжение
				sw.WriteLine("Фазное напряжение (фаза А): " + tbUA.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Фазное напряжение (фаза B): " + tbUB.Text);
					sw.WriteLine("Фазное напряжение (фаза C): " + tbUC.Text);
					// Линейное напряжение
					sw.WriteLine("Линейное напряжение (линия AB): " + tbUAl.Text);
					sw.WriteLine("Линейное напряжение (линия BC): " + tbUBl.Text);
					sw.WriteLine("Линейное напряжение (линия CA): " + tbUCl.Text);
				}
				// средневыпрямленное значение
				sw.WriteLine("Напряжение, средневыпрямленное значение (фаза А): " + tbUAav.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Напряжение, средневыпрямленное значение (фаза B): " + tbUBav.Text);
					sw.WriteLine("Напряжение, средневыпрямленное значение (фаза C): " + tbUCav.Text);
					sw.WriteLine("Напряжение, средневыпрямленное значение (линия AB): " + tbUAavl.Text);
					sw.WriteLine("Напряжение, средневыпрямленное значение (линия BC): " + tbUBavl.Text);
					sw.WriteLine("Напряжение, средневыпрямленное значение (линия CA): " + tbUCavl.Text);
				}
				// Фазное напряжение 1 гармоники
				sw.WriteLine("Фазное напряжение 1 гармоники (фаза А): " + tbUA1.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Фазное напряжение 1 гармоники (фаза B): " + tbUB1.Text);
					sw.WriteLine("Фазное напряжение 1 гармоники (фаза C): " + tbUC1.Text);
					// Линейное напряжение 1 гармоники
					sw.WriteLine("Линейное напряжение 1 гармоники (линия AB): " + tbUA1l.Text);
					sw.WriteLine("Линейное напряжение 1 гармоники (линия BC): " + tbUB1l.Text);
					sw.WriteLine("Линейное напряжение 1 гармоники (линия CA): " + tbUC1l.Text);

					// напряжение прямой последовательности U1
					sw.WriteLine("Напряжение прямой последовательности U1: " + tbU1.Text);
					// Напряжение обратной последовательности U2
					sw.WriteLine("Напряжение обратной последовательности U2: " + tbU2.Text);
					// Напряжение нулевой последовательности U0
					sw.WriteLine("Напряжение нулевой последовательности U0: " + tbU0.Text);
					// Коэффициент обратной последовательности KU2
					sw.WriteLine("Коэффициент обратной последовательности KU2: " + tbKU2.Text);
					// Коэффициент нулевой последовательности KU0
					sw.WriteLine("Коэффициент нулевой последовательности KU0: " + tbKU0.Text);
				}
				// Отклонение установившегося напряжения [относительное]
				sw.WriteLine("Отклонение установившегося напряжения [относительное]: " + tbdUPer.Text);
				// Положительное отклонение напряжения
				sw.WriteLine("Положительное отклонение напряжения (фаза A): " + tbdUApos.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Положительное отклонение напряжения (фаза B): " + tbdUBpos.Text);
					sw.WriteLine("Положительное отклонение напряжения (фаза C): " + tbdUCpos.Text);
					sw.WriteLine("Положительное отклонение напряжения (линия AB): " + tbdUABpos.Text);
					sw.WriteLine("Положительное отклонение напряжения (линия BC): " + tbdUBCpos.Text);
					sw.WriteLine("Положительное отклонение напряжения (линия CA): " + tbdUCApos.Text);
				}
				// Отрицательное отклонение напряжения
				sw.WriteLine("Отрицательное отклонение напряжения (фаза A): " + tbdUAneg.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Отрицательное отклонение напряжения (фаза B): " + tbdUBneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения (фаза C): " + tbdUCneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения (линия AB): " + tbdUABneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения (линия BC): " + tbdUBCneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения (линия CA): " + tbdUCAneg.Text);
				}
				// Положительное отклонение напряжения [относительное]
				sw.WriteLine("Положительное отклонение напряжения [относительное] (фаза A): " + tbdUrelApos.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Положительное отклонение напряжения [относительное] (фаза B): " + tbdUrelBpos.Text);
					sw.WriteLine("Положительное отклонение напряжения [относительное] (фаза C): " + tbdUrelCpos.Text);
					sw.WriteLine("Положительное отклонение напряжения [относительное] (линия AB): " + tbdUrelABpos.Text);
					sw.WriteLine("Положительное отклонение напряжения [относительное] (линия BC): " + tbdUrelBCpos.Text);
					sw.WriteLine("Положительное отклонение напряжения [относительное] (линия CA): " + tbdUrelCApos.Text);
				}
				// Отрицательное отклонение напряжения [относительное]
				sw.WriteLine("Отрицательное отклонение напряжения [относительное] (фаза A): " + tbdUrelAneg.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Отрицательное отклонение напряжения [относительное] (фаза B): " + tbdUrelBneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения [относительное] (фаза C): " + tbdUrelCneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения [относительное] (линия AB): " + tbdUrelABneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения [относительное] (линия BC): " + tbdUrelBCneg.Text);
					sw.WriteLine("Отрицательное отклонение напряжения [относительное] (линия CA): " + tbdUrelCAneg.Text);
				}
				sw.WriteLine("");

				// Ток
				sw.WriteLine("Ток (фаза А): " + tbIA.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Ток (фаза B): " + tbIB.Text);
					sw.WriteLine("Ток (фаза C): " + tbIC.Text);
				}
				// Ток 1 гармоники
				sw.WriteLine("Ток 1 гармоники (фаза А): " + tbIA1.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Ток 1 гармоники (фаза B): " + tbIB1.Text);
					sw.WriteLine("Ток 1 гармоники (фаза C): " + tbIC1.Text);
				}
				// Ток средневыпрямленное значение
				sw.WriteLine("Ток, средневыпрямленное значение (фаза А): " + tbIAav.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Ток, средневыпрямленное значение (фаза B): " + tbIBav.Text);
					sw.WriteLine("Ток, средневыпрямленное значение (фаза C): " + tbICav.Text);

					// Ток прямой последовательности I1
					sw.WriteLine("Ток прямой последовательности I1: " + tbI1.Text);
					// Ток обратной последовательности I2
					sw.WriteLine("Ток обратной последовательности I2: " + tbI2.Text);
					// Ток нулевой последовательности I0
					sw.WriteLine("Ток нулевой последовательности I0: " + tbI0.Text);
				}
				sw.WriteLine("");

				// Мощность
				// Суммарная активная мощность P
				sw.WriteLine("Суммарная активная мощность P: " + tbPsum.Text);
				// Активная мощность
				sw.WriteLine("Активная мощность P (фаза А): " + tbPa.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Активная мощность P (фаза B): " + tbPb.Text);
					sw.WriteLine("Активная мощность P (фаза C): " + tbPc.Text);
				}
				// Суммарная реактивная мощность Q
				sw.WriteLine("Суммарная реактивная мощность Q: " + tbQsum.Text);
				// Реактивная мощность
				sw.WriteLine("Реактивная мощность Q (фаза А): " + tbQa.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Реактивная мощность Q (фаза B): " + tbQb.Text);
					sw.WriteLine("Реактивная мощность Q (фаза C): " + tbQc.Text);
				}
				// Суммарная полная мощность S
				sw.WriteLine("Суммарная полная мощность S: " + tbSsum.Text);
				// Полная мощность
				sw.WriteLine("Полная мощность S (фаза А): " + tbSa.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Полная мощность S (фаза B): " + tbSb.Text);
					sw.WriteLine("Полная мощность S (фаза C): " + tbSc.Text);
				}
				// Суммарный коэффициент мощности Kp
				sw.WriteLine("Суммарный коэффициент мощности Kp: " + tbKpSum.Text);
				// Коэффициент мощности
				sw.WriteLine("Коэффициент мощности Kp (фаза А): " + tbKPA.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Коэффициент мощности Kp (фаза B): " + tbKPB.Text);
					sw.WriteLine("Коэффициент мощности Kp (фаза C): " + tbKPC.Text);

					// Мощность прямой последовательности P1
					sw.WriteLine("Мощность прямой последовательности P1: " + tbP1.Text);
					// Мощность обратной последовательности P2
					sw.WriteLine("Мощность обратной последовательности P2: " + tbP2.Text);
					// Мощность нулевой оследовательности P0
					sw.WriteLine("Мощность нулевой оследовательности P0: " + tbP0.Text);
				}
				sw.WriteLine("");

				// Углы
				// Угол между током и напряжением
				sw.WriteLine("Угол между током и напряжением (фаза А): " + tbAngleIUA.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Угол между током и напряжением (фаза B): " + tbAngleIUB.Text);
					sw.WriteLine("Угол между током и напряжением (фаза C): " + tbAngleIUC.Text);
					// Угол напряжения фазный
					sw.WriteLine("Угол напряжения фазный (Ua Ub): " + tbAngleUaUb.Text);
					sw.WriteLine("Угол напряжения фазный (Ub Uc): " + tbAngleUbUc.Text);
					sw.WriteLine("Угол напряжения фазный (Uc Ua): " + tbAngleUcUa.Text);
				}
				sw.WriteLine("");

				// Гармонические подгруппы
				sw.WriteLine("\n\nГармонические подгруппы: ");
				sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (Фаза А): " + tbGrgSumA.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (Фаза B): " +
						tbGrgSumB.Text);
					sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (Фаза C): " +
						tbGrgSumC.Text);
				}
				sw.WriteLine("\nЗначения для порядков 1...50: ");
				for (int iRec = 0; iRec < lvHarmonicsPh.Items.Count; ++iRec)
				{
					string s = lvHarmonicsPh.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmonicsPh.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmonicsPh.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}
				sw.WriteLine("\nСуммарный коэффициент (Фаза А): " + tbThdsA.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Суммарный коэффициент (Фаза B): " + tbThdsB.Text);
					sw.WriteLine("Суммарный коэффициент (Фаза C): " + tbThdsC.Text);
				}
				sw.WriteLine("\nКоэффициенты для порядков 2...50: ");
				for (int iRec = 0; iRec < lvHarmonicsPhOrders.Items.Count; ++iRec)
				{
					string s = lvHarmonicsPhOrders.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmonicsPhOrders.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmonicsPhOrders.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("\nСуммарное значение для гармонических подгрупп порядка > 1 (линия AB): " +
					             tbGrgSumAB.Text);
					sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (линия BC): " +
					             tbGrgSumBC.Text);
					sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (линия CA): " +
					             tbGrgSumCA.Text);
					sw.WriteLine("\nЗначения для порядков 1...50: ");
					for (int iRec = 0; iRec < lvHarmonicsLin.Items.Count; ++iRec)
					{
						string s = lvHarmonicsLin.Items[iRec].Text + ":  ";
						for (int iItem = 1; iItem < lvHarmonicsLin.Items[iRec].SubItems.Count; ++iItem)
						{
							s += lvHarmonicsLin.Items[iRec].SubItems[iItem].Text + "  ";
						}
						sw.WriteLine(s);
					}
					sw.WriteLine("\nСуммарный коэффициент (линия AB): " + tbThdsAB.Text);
					sw.WriteLine("Суммарный коэффициент (линия BC): " + tbThdsBC.Text);
					sw.WriteLine("Суммарный коэффициент (линия CA): " + tbThdsCA.Text);
					sw.WriteLine("\nКоэффициенты для порядков 2...50: ");
					for (int iRec = 0; iRec < lvHarmonicsLinOrders.Items.Count; ++iRec)
					{
						string s = lvHarmonicsLinOrders.Items[iRec].Text + ":  ";
						for (int iItem = 1; iItem < lvHarmonicsLinOrders.Items[iRec].SubItems.Count; ++iItem)
						{
							s += lvHarmonicsLinOrders.Items[iRec].SubItems[iItem].Text + "  ";
						}
						sw.WriteLine(s);
					}
				}

				sw.WriteLine("\nСуммарное значение для гармонических подгрупп порядка > 1 (Ток IA): " +
					tbGrgSumAB.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (Ток IB): " +
					             tbGrgSumBC.Text);
					sw.WriteLine("Суммарное значение для гармонических подгрупп порядка > 1 (Ток IC): " +
					             tbGrgSumCA.Text);
				}
				sw.WriteLine("\nЗначения для порядков 1...50: ");
				for (int iRec = 0; iRec < lvHarmonicsI.Items.Count; ++iRec)
				{
					string s = lvHarmonicsI.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmonicsI.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmonicsI.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}
				sw.WriteLine("\nСуммарный коэффициент (Ток IA): " + tbThdsAB.Text);
				if (devInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					sw.WriteLine("Суммарный коэффициент (Ток IB): " + tbThdsBC.Text);
					sw.WriteLine("Суммарный коэффициент (Ток IC): " + tbThdsCA.Text);
				}
				sw.WriteLine("\nКоэффициенты для порядков 2...50: ");
				for (int iRec = 0; iRec < lvHarmonicsIOrders.Items.Count; ++iRec)
				{
					string s = lvHarmonicsIOrders.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvHarmonicsIOrders.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvHarmonicsIOrders.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}
				
				// Интергармоники
				// Среднеквадратическое значение интергармонических групп
				sw.WriteLine("\nСреднеквадратическое значение интергармонических групп (фазное напряжение): ");
				for (int iRec = 0; iRec < lvInterharmUph.Items.Count; ++iRec)
				{
					string s = lvInterharmUph.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvInterharmUph.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvInterharmUph.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}
				sw.WriteLine("\nСреднеквадратическое значение интергармонических групп (линейное напряжение): ");
				for (int iRec = 0; iRec < lvInterharmUlin.Items.Count; ++iRec)
				{
					string s = lvInterharmUlin.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvInterharmUlin.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvInterharmUlin.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}
				sw.WriteLine("\nСреднеквадратическое значение интергармонических групп (ток): ");
				for (int iRec = 0; iRec < lvInterharmI.Items.Count; ++iRec)
				{
					string s = lvInterharmI.Items[iRec].Text + ":  ";
					for (int iItem = 1; iItem < lvInterharmI.Items[iRec].SubItems.Count; ++iItem)
					{
						s += lvInterharmI.Items[iRec].SubItems[iItem].Text + "  ";
					}
					sw.WriteLine(s);
				}

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

		private void SetValueToTextbox(float val, ref TextBox tb)
		{
			System.Globalization.CultureInfo cultureRu = new System.Globalization.CultureInfo("ru-RU");
			val = (float)Math.Round((double)val, 6);
			tb.Text = val.ToString("F6", cultureRu);
			tb.Text = formatNumber(tb.Text, 6);
		}
	}

	class DataReceiveThreadEtPQP_A
	{
		public delegate void DataReceivedHandler();
		public event DataReceivedHandler OnDataReceived;
		public delegate void ConnectEndHandler();
		public event ConnectEndHandler OnConnectEnd;
		EmDataSaver.Settings settings_;
		EmDevice device_;
		AvgTypes_PQP_A typeAVG_ = AvgTypes_PQP_A.ThreeSec;
		Form sender_;

		public DataReceiveThreadEtPQP_A(EmDataSaver.Settings s, AvgTypes_PQP_A type, Form sender)
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

				}
				else if (settings_.IOInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				else if (settings_.IOInterface == EmPortType.WI_FI)
				{
					try
					{
						if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
						{
							WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
							                                                            settings_.WifiPassword);

							if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
							{
								EmService.WriteToLogFailed("frmObjectNames: Wi-fi not connected!");
								MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
								                                   settings_.IOParameters);
								return;
							}
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "frmObjectNames: Exception in ConnectToWifi() WI-FI:");
						MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
						                                   settings_.IOParameters);
						return;
					}

					port_params = new object[2];
					port_params[0] = EmService.GetCurrentDhcpIpAddress();
					port_params[1] = settings_.CurrentPort;
				}
				else
				{
					throw new EmInvalidInterfaceException();
				}

				// if not RS-485 then we have to set the broadcasting address
				if (settings_.IOInterface != EmPortType.Rs485 && settings_.IOInterface != EmPortType.Modem &&
					settings_.IOInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
				}

				if (settings_.CurDeviceType == EmDeviceType.ETPQP_A)
				{
					device_ = new EtPQP_A_Device(settings_.IOInterface, settings_.CurDeviceAddress,
											false, port_params,
											settings_.CurWifiProfileName, settings_.WifiPassword,
											(sender_ as Form).Handle);
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
				lock (frmMomentDataEtPQP_A.devInfo_)
				{
					frmMomentDataEtPQP_A.devInfo_.SerialNumber = serial;
					errCode = (device_ as EtPQP_A_Device).ReadMomentData(ref frmMomentDataEtPQP_A.buffer_, typeAVG_,
															ref frmMomentDataEtPQP_A.devInfo_);
				}

				// if reading was not successfull
				if (errCode != ExchangeResult.OK)
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
							settings_.IOParameters);
					return;
				}

				// тип записи
				//ushort type2 = Conversions.bytes_2_ushort(ref frmMomentDataEtPQP_A.buffer_, 0);
				//if (type2 == 0)
				//{
				//    MessageBoxes.ErrorFromDevice(this);
				//    EmService.WriteToLogFailed("Error:  moment data type = 0!");
				//    return;
				//}
				//if (type2 != EmService.typeAVG)
				//{
				//    DeviceIO.EM32.CloseConnection(settings.IOInterface, settings.IOParameters);
				//    MessageBoxes.DeviceReadError(this);
				//    EmService.WriteToLogFailed("incorrect moment data type!  " +
				//        type2 + "  " + EmService.typeAVG);
				//    //return;	
				//}

				//ProcessData(ref buffer, ref devInfo);
				if (OnDataReceived != null) OnDataReceived();
			}
			catch (EmDeviceEmptyException)
			{
				if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
					MessageBox.Show("Невозможно получить данные");
				else MessageBox.Show("Unable to get data");
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmMomentDataEtPQP_A::ThreadEntry():");
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