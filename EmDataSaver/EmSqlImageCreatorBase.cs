using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.ComponentModel;

using DeviceIO;
using EmDataSaver.SqlImage;
using EmServiceLib;

namespace EmDataSaver
{
	public abstract class EmSqlImageCreatorBase
	{
		#region Fields

		protected object sender_;
		protected Settings settings_;
		protected DoWorkEventArgs e_;

		protected EmSqlDataNodeType[] parts_;
		//protected long archive_id_;
		//protected long device_id_;

		// число шагов при формировании образа (инфа для ProgressBar)
		protected double cnt_steps_progress_ = 0.0;		// 100%
		protected double cur_percent_progress_ = 0.0;	// current percent

		#endregion

		#region Main methods

		/// <summary>
		/// Main saving function
		/// Start saving process
		/// </summary>
		public void Run(ref DoWorkEventArgs e)
		{
			try
			{
				this.e_ = e;
				e.Result = CreateSqlImage();
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in ExSqlImageCreatorXX::Run(): " + emx.Message);
				e.Result = false;
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExSqlImageCreatorXX::Run():");
				e.Result = false;
				throw;
			}
		}

		public abstract bool CreateSqlImage();

		#endregion

		#region Protected Methods

		// данные для t2
		protected void get_data_for_t2(ref byte[] buffer, int param_id,
			ref float fNDP_d, ref float fNDP_u, ref float fPDP_d, ref float fPDP_u,
			ref float f_max, ref float f_min, ref float f_95_low, ref float f_95_up,
			ref ushort num_out_max_rng, ref ushort num_nrm_rng, ref ushort num_all, 
			ref ushort num_max_rng,
			ConnectScheme con_scheme)
		{
			try
			{
				// ГОСТы из уставок
				if (param_id == 1001)
				{
					fNDP_u = Conversions.bytes_2_signed_float8192(ref buffer, 26);
					fNDP_d = Conversions.bytes_2_signed_float8192(ref buffer, 24);
					fPDP_u = Conversions.bytes_2_signed_float8192(ref buffer, 30);
					fPDP_d = Conversions.bytes_2_signed_float8192(ref buffer, 28);
				}
				else
				{
					switch (param_id)
					{
						case 1002:
						case 1003:
						case 1004:
						case 1005:
						case 1014:
						case 1015:
						case 1016:
							fNDP_d = Conversions.bytes_2_signed_float1024(ref buffer, 48);
							fNDP_u = Conversions.bytes_2_signed_float1024(ref buffer, 50);
							fPDP_d = Conversions.bytes_2_signed_float1024(ref buffer, 52);
							fPDP_u = Conversions.bytes_2_signed_float1024(ref buffer, 54);
							break;
						case 1006:
						case 1007:
						case 1008:
						case 1009:
						case 1017:
						case 1018:
						case 1019:
							fNDP_d = Conversions.bytes_2_signed_float1024(ref buffer, 32);
							fNDP_u = Conversions.bytes_2_signed_float1024(ref buffer, 34);
							fPDP_d = Conversions.bytes_2_signed_float1024(ref buffer, 36);
							fPDP_u = Conversions.bytes_2_signed_float1024(ref buffer, 38);
							break;
						case 1010:
						case 1011:
						case 1012:
						case 1013:
						case 1020:
						case 1021:
						case 1022:
							fNDP_d = Conversions.bytes_2_signed_float1024(ref buffer, 40);
							fNDP_u = Conversions.bytes_2_signed_float1024(ref buffer, 42);
							fPDP_d = Conversions.bytes_2_signed_float1024(ref buffer, 44);
							fPDP_u = Conversions.bytes_2_signed_float1024(ref buffer, 46);
							break;
					}
				}

				switch (param_id)
				{
					case 1001:
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 450);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 448);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 454);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 452));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(Conversions.bytes_2_signed_float_Q_6_9(ref buffer, 460), 3);
						f_min = (float)Math.Round(Conversions.bytes_2_signed_float_Q_6_9(ref buffer, 462), 3);

						// 95 %
						f_95_low = (float)Math.Round(Conversions.bytes_2_signed_float_Q_6_9(ref buffer, 458), 3);
						f_95_up = (float)Math.Round(Conversions.bytes_2_signed_float_Q_6_9(ref buffer, 456), 3);
						break;

					case 1002:		// dU_y
						if (con_scheme != ConnectScheme.Ph1W2)
						{
							num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 466);	// в НДП
							num_all = Conversions.bytes_2_ushort(ref buffer, 464);	// общее кол-во
							num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 470);	// за ПДП
							// отсчетов между ПДП и НДП
							//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 468));
							num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

							// наибольшее и наименьшее значения
							f_max = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 492), 4) * 100;
							f_min = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 494), 4) * 100;

							// 95 %
							f_95_low = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 490), 4) * 100;
							f_95_up = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 488), 4) * 100;
						}
						else  // если 1ф2пр, то берем инфу для фазы А
						{
							num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 514);	// в НДП
							num_all = Conversions.bytes_2_ushort(ref buffer, 512);	// общее кол-во
							num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 518);	// за ПДП
							// отсчетов между ПДП и НДП
							//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 500));
							num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

							// наибольшее и наименьшее значения
							f_max = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 540), 4) * 100;
							f_min = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 542), 4) * 100;

							// 95 %
							f_95_low = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 538), 4) * 100;
							f_95_up = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 536), 4) * 100;
						}
						break;

					//case 1006:		// dU_y_'
					//    if (con_scheme != ConnectScheme.Ph1W2)
					//    {
					//        num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 486);	// за ПДП
					//        num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 482);	// в НДП
					//        num_all = Conversions.bytes_2_ushort(ref buffer, 480);	// общее кол-во
					//        // отсчетов между ПДП и НДП
					//        //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 484));
					//        num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//        // наибольшее и наименьшее значения
					//        f_max = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 508), 4) * 100;
					//        f_min = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 510), 4) * 100;

					//        // 95 %
					//        f_95_low = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 506), 4) * 100;
					//        f_95_up = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 504), 4) * 100;
					//    }
					//    else  // если 1ф2пр, то берем инфу для фазы А
					//    {
					//        num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 530);	// в НДП
					//        num_all = Conversions.bytes_2_ushort(ref buffer, 528);	// общее кол-во
					//        num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 534);	// за ПДП
					//        // отсчетов между ПДП и НДП
					//        //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 532));
					//        num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//        // наибольшее и наименьшее значения
					//        f_max = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 556), 4) * 100;
					//        f_min = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 558), 4) * 100;

					//        // 95 %
					//        f_95_low = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 554), 4) * 100;
					//        f_95_up = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 552), 4) * 100;
					//    }
					//    break;

					//case 1010:		// dU_y_"
					//    if (con_scheme != ConnectScheme.Ph1W2)
					//    {
					//        num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 474);	// в НДП
					//        num_all = Conversions.bytes_2_ushort(ref buffer, 472);	// общее кол-во
					//        num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 478);	// за ПДП
					//        // отсчетов между ПДП и НДП
					//        //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 476));
					//        num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//        // наибольшее и наименьшее значения
					//        f_max = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 500), 4) * 100;
					//        f_min = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 502), 4) * 100;

					//        // 95 %
					//        f_95_low = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 498), 4) * 100;
					//        f_95_up = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 496), 4) * 100;
					//    }
					//    else  // если 1ф2пр, то берем инфу для фазы А
					//    {
					//        num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 522);	// в НДП
					//        num_all = Conversions.bytes_2_ushort(ref buffer, 520);	// общее кол-во
					//        num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 526);	// за ПДП
					//        // отсчетов между ПДП и НДП
					//        //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 524));
					//        num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//        // наибольшее и наименьшее значения
					//        f_max = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 548), 4) * 100;
					//        f_min = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 550), 4) * 100;

					//        // 95 %
					//        f_95_low = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 546), 4) * 100;
					//        f_95_up = (float)Math.Round(
					//            Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 544), 4) * 100;
					//        break;
					//    }
					//    break;

					case 1006:		// dU_y_'
						if (con_scheme != ConnectScheme.Ph1W2)
						{
							num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 478);	// за ПДП
							num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 474);	// в НДП
							num_all = Conversions.bytes_2_ushort(ref buffer, 472);	// общее кол-во
							// отсчетов между ПДП и НДП
							//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 476));
							num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

							// наибольшее и наименьшее значения
							f_max = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 500), 4) * 100;
							f_min = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 502), 4) * 100;

							// 95 %
							f_95_low = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 498), 4) * 100;
							f_95_up = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 496), 4) * 100;
						}
						else  // если 1ф2пр, то берем инфу для фазы А
						{
							num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 522);	// в НДП
							num_all = Conversions.bytes_2_ushort(ref buffer, 520);	// общее кол-во
							num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 526);	// за ПДП
							// отсчетов между ПДП и НДП
							//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 508));
							num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

							// наибольшее и наименьшее значения
							f_max = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 548), 4) * 100;
							f_min = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 550), 4) * 100;

							// 95 %
							f_95_low = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 546), 4) * 100;
							f_95_up = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 544), 4) * 100;
						}
						break;

					case 1010:		// dU_y_"
						if (con_scheme != ConnectScheme.Ph1W2)
						{
							num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 482);	// в НДП
							num_all = Conversions.bytes_2_ushort(ref buffer, 480);	// общее кол-во
							num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 486);	// за ПДП
							// отсчетов между ПДП и НДП
							//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 484));
							num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

							// наибольшее и наименьшее значения
							f_max = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 508), 4) * 100;
							f_min = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 510), 4) * 100;

							// 95 %
							f_95_low = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 506), 4) * 100;
							f_95_up = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 504), 4) * 100;
						}
						else  // если 1ф2пр, то берем инфу для фазы А
						{
							num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 530);	// в НДП
							num_all = Conversions.bytes_2_ushort(ref buffer, 528);	// общее кол-во
							num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 534);	// за ПДП
							// отсчетов между ПДП и НДП
							//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 516));
							num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

							// наибольшее и наименьшее значения
							f_max = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 556), 4) * 100;
							f_min = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 558), 4) * 100;

							// 95 %
							f_95_low = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 554), 4) * 100;
							f_95_up = (float)Math.Round(
								Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 552), 4) * 100;
							break;
						}
						break;

					case 1003:		// dU_A
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 514);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 512);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 518);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 500));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 540), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 542), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 538), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 536), 4) * 100;
						break;

					//case 1007:		// dU_A_'
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 530);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 528);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 534);	// за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 532));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 556), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 558), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 554), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 552), 4) * 100;
					//    break;

					//case 1011:		// dU_A_"
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 522);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 520);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 526);	// за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 524));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 548), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 550), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 546), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 544), 4) * 100;
					//    break;

					case 1007:		// dU_A_'
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 522);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 520);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 526);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 508));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 548), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 550), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 546), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 544), 4) * 100;
						break;

					case 1011:		// dU_A_"
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 530);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 528);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 534);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 516));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 556), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 558), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 554), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 552), 4) * 100;
						break;

					case 1014:		// dU_AB
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 562);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 560);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 566);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 532));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 588), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 590), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 586), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 584), 4) * 100;
						break;

					//case 1017:		// dU_AB_'
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 578);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 576);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 582);	// за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 580));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 604), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 606), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 602), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 600), 4) * 100;
					//    break;

					//case 1020:		// dU_AB_"
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 570);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 568);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 574);	// за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 572));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 596), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 598), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 594), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 592), 4) * 100;
					//    break;

					case 1017:		// dU_AB_'
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 570);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 568);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 574);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 540));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 596), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 598), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 594), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 592), 4) * 100;
						break;

					case 1020:		// dU_AB_"
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 578);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 576);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 582);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 548));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 604), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 606), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 602), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 600), 4) * 100;
						break;

					case 1004:		// dU_B
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 610);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 608);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 614);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 564));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 636), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 638), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 634), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 632), 4) * 100;
						break;

					//case 1008:		// dU_B_'
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 626);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 624);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 630);	// за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 628));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 652), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 654), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 650), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 648), 4) * 100;
					//    break;

					//case 1012:		// dU_B_"
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 618);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 616);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 622);	// за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 620));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 644), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 646), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 642), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 640), 4) * 100;
					//    break;

					case 1008:		// dU_B_'
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 618);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 616);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 622);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 572));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 644), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 646), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 642), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 640), 4) * 100;
						break;

					case 1012:		// dU_B_"
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 626);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 624);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 630);	// за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 580));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 652), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 654), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 650), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 648), 4) * 100;
						break;

					case 1015:		// dU_BC
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 658);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 656);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 662);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 596));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 684), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 686), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 682), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 680), 4) * 100;
						break;

					//case 1018:		// dU_BC_'
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 674);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 672);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 678);	//за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 676));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 700), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 702), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 698), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 696), 4) * 100;
					//    break;

					//case 1021:		// δU_BC_"
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 666);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 664);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 670);	//за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 668));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 692), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 694), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 690), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 688), 4) * 100;
					//    break;

					case 1018:		// dU_BC_'
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 666);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 664);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 670);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 604));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 692), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 694), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 690), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 688), 4) * 100;
						break;

					case 1021:		// δU_BC_"
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 674);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 672);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 678);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 612));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 700), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 702), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 698), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 696), 4) * 100;
						break;

					case 1005:		// dU_C
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 706);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 704);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 710);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 628));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 732), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 734), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 730), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 728), 4) * 100;
						break;

					//case 1009:		// dU_C_'
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 722);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 720);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 726);	//за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 724));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 748), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 750), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 746), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 744), 4) * 100;
					//    break;

					//case 1013:		// dU_C_"
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 714);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 712);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 718);	//за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 716));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 740), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 742), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 738), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 736), 4) * 100;
					//    break;

					case 1009:		// dU_C_'
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 714);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 712);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 718);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 636));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 740), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 742), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 738), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 736), 4) * 100;
						break;

					case 1013:		// dU_C_"
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 722);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 720);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 726);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 644));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 748), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 750), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 746), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 744), 4) * 100;
						break;

					case 1016:		// dU_CA
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 754);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 752);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 758);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 660));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 780), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 782), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 778), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 776), 4) * 100;
						break;

					//case 1019:		// dU_CA_'
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 770);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 768);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 774);	//за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 772));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 796), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 798), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 794), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 792), 4) * 100;
					//    break;

					//case 1022:		// dU_CA_"
					//    num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 762);	// в НДП
					//    num_all = Conversions.bytes_2_ushort(ref buffer, 760);	// общее кол-во
					//    num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 766);	//за ПДП
					//    // отсчетов между ПДП и НДП
					//    //num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 764));
					//    num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

					//    // наибольшее и наименьшее значения
					//    f_max = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 788), 4) * 100;
					//    f_min = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 790), 4) * 100;

					//    // 95 %
					//    f_95_low = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 786), 4) * 100;
					//    f_95_up = (float)Math.Round(
					//        Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 784), 4) * 100;
					//    break;

					case 1019:		// dU_CA_'
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 762);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 760);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 766);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 668));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 788), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 790), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 786), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 784), 4) * 100;
						break;

					case 1022:		// dU_CA_"
						num_nrm_rng = Conversions.bytes_2_ushort(ref buffer, 770);	// в НДП
						num_all = Conversions.bytes_2_ushort(ref buffer, 768);	// общее кол-во
						num_out_max_rng = Conversions.bytes_2_ushort(ref buffer, 774);	//за ПДП
						// отсчетов между ПДП и НДП
						//num_max_rng = (ushort)(num_nrm_rng - Conversions.bytes_2_ushort(ref buffer, 676));
						num_max_rng = (ushort)(num_all - num_nrm_rng - num_out_max_rng);

						// наибольшее и наименьшее значения
						f_max = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 796), 4) * 100;
						f_min = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 798), 4) * 100;

						// 95 %
						f_95_low = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 794), 4) * 100;
						f_95_up = (float)Math.Round(
							Conversions.bytes_2_signed_float_Q_1_15(ref buffer, 792), 4) * 100;
						break;
				}
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in get_data_for_t2():  " + e.Message);
				throw e;
			}
		}

		#endregion
	}
}
