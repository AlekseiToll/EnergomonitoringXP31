using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using DbServiceLib;
using EmServiceLib;

namespace DeviceIO.Constraints
{
    public class EtPQPAConstraints : EMConstraintsBase
    {
        #region Fields

        // кол-во уставок в поднаборе
        public const int CntConstraints = 100;

        // массив уставок: 6 наборов, в каждом из них 2 поднабора,
        // в каждом поднаборе 100 уставок (100 слов)
        private float[,] constraints_ = new float[CntConstraintsSets, CntConstraints];

        #endregion

        #region Properties

        public float[,] Constraints
        {
            get { return constraints_; }
        }

        /// <summary>Gets size of this memory region in bytes</summary>
        public ushort Size
        {
            get
            {
                return (ushort)(CntConstraintsSets * CntConstraints * 4);
            }
        }

		public float[, ,] EmptyConstraintsForTable
		{
			get
			{
				return new float[CntConstraintsSets, CntConstraints / 2, 2 /*2 columns*/];
			}
		}

        // уставки в формате таблицы
        public float[, ,] ConstraintsForTable
        {
            get
            {
                float[, ,] vals = new float[CntConstraintsSets, CntConstraints / 2, 2 /*2 columns*/];

                for (int iSet = 0; iSet < CntConstraintsSets; ++iSet)
                {
                    // ∆F+ synch
                    vals[iSet, 0, 0] = constraints_[iSet, 2];
                    vals[iSet, 0, 1] = constraints_[iSet, 3];

                    // ∆F- synch
                    vals[iSet, 1, 0] = constraints_[iSet, 0];
                    vals[iSet, 1, 1] = constraints_[iSet, 1];

                    // ∆F+ iso
                    vals[iSet, 2, 0] = constraints_[iSet, 6];
                    vals[iSet, 2, 1] = constraints_[iSet, 7];

                    // ∆F- iso
                    vals[iSet, 3, 0] = constraints_[iSet, 4];
                    vals[iSet, 3, 1] = constraints_[iSet, 5];

                    // δU+
                    vals[iSet, 4, 0] = constraints_[iSet, 8];
                    vals[iSet, 4, 1] = constraints_[iSet, 9];

					// δU-
					vals[iSet, 5, 0] = constraints_[iSet, 10];
					vals[iSet, 5, 1] = constraints_[iSet, 11];

                    // flik short
                    vals[iSet, 6, 0] = constraints_[iSet, 12];
                    vals[iSet, 6, 1] = constraints_[iSet, 13];

                    // flik long
                    vals[iSet, 7, 0] = constraints_[iSet, 14];
                    vals[iSet, 7, 1] = constraints_[iSet, 15];

                    // K harm 2 - K harm 40
                    int startNDZ = 16, startPDZ = 55;
                    for (int iRow = 8; iRow < 47; ++iRow)
                    {
                        vals[iSet, iRow, 0] = constraints_[iSet, startNDZ++];
                        vals[iSet, iRow, 1] = constraints_[iSet, startPDZ++];
                    }

                    // K harm total
                    vals[iSet, 47, 0] = constraints_[iSet, 94];
                    vals[iSet, 47, 1] = constraints_[iSet, 95];

                    // K2U
                    vals[iSet, 48, 0] = constraints_[iSet, 96];
                    vals[iSet, 48, 1] = constraints_[iSet, 97];

                    // K0U
                    vals[iSet, 49, 0] = constraints_[iSet, 98];
                    vals[iSet, 49, 1] = constraints_[iSet, 99];
                }

                return vals;
            }

            set
            {
                float[, ,] vals = value;
                if (vals.Length != (CntConstraintsSets * (CntConstraints / 2) * 2))
                    throw new EmException("ConstraintsForTable: invalid value length!");

                for (int iSet = 0; iSet < CntConstraintsSets; ++iSet)
                {
                    // ∆F+ synch
                    constraints_[iSet, 2] = vals[iSet, 0, 0];
                    constraints_[iSet, 3] = vals[iSet, 0, 1];

                    // ∆F- synch
                    constraints_[iSet, 0] = vals[iSet, 1, 0];
                    constraints_[iSet, 1] = vals[iSet, 1, 1];

                    // ∆F+ iso
                    constraints_[iSet, 6] = vals[iSet, 2, 0];
                    constraints_[iSet, 7] = vals[iSet, 2, 1];

                    // ∆F- iso
                    constraints_[iSet, 4] = vals[iSet, 3, 0];
                    constraints_[iSet, 5] = vals[iSet, 3, 1];

                    // δU-
                    constraints_[iSet, 8] = vals[iSet, 4, 0];
                    constraints_[iSet, 9] = vals[iSet, 4, 1];

					// δU+
					constraints_[iSet, 10] = vals[iSet, 5, 0];
					constraints_[iSet, 11] = vals[iSet, 5, 1];

                    // flik short
                    constraints_[iSet, 12] = vals[iSet, 6, 0];
                    constraints_[iSet, 13] = vals[iSet, 6, 1];

                    // flik long
                    constraints_[iSet, 14] = vals[iSet, 7, 0];
                    constraints_[iSet, 15] = vals[iSet, 7, 1];

                    // K harm 2 - K harm 40
                    int startNDZ = 16, startPDZ = 55;
                    for (int iRow = 8; iRow < 47; ++iRow)
                    {
                        constraints_[iSet, startNDZ++] = vals[iSet, iRow, 0];
                        constraints_[iSet, startPDZ++] = vals[iSet, iRow, 1];
                    }

                    // K harm total
                    constraints_[iSet, 94] = vals[iSet, 47, 0];
                    constraints_[iSet, 95] = vals[iSet, 47, 1];

                    // K2U
                    constraints_[iSet, 96] = vals[iSet, 48, 0];
                    constraints_[iSet, 97] = vals[iSet, 48, 1];

                    // K0U
                    constraints_[iSet, 98] = vals[iSet, 49, 0];
                    constraints_[iSet, 99] = vals[iSet, 49, 1];
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>Parses array and fills inner object list</summary>
        /// <param name="array">byte array to parse</param>
        /// <returns>True if all OK or False</returns>
        public bool Parse(ref byte[] array)
        {
            if (array == null) return false;
            if (array.Length < this.Size) return false;

            try
            {
                constraints_ = new float[CntConstraintsSets /*6*/, CntConstraints /*98*/];
                int shift = 0;

                for (int iSet = 0; iSet < CntConstraintsSets; iSet++)
                {
                    for (int iConst = 0; iConst < CntConstraints; iConst++)
                    {
                        // первые 4 уставки - частота, для нее другой формат
                        //if (iConst < 4)
                        //    constraints_[iSet, iConst] =
                        //        Conversions.bytes_2_signed_float65536(ref array, shift);
                        //else
                        constraints_[iSet, iConst] =
                            Conversions.bytes_2_signed_float_Q_15_16_new(ref array, shift);

                        shift += 4; // 2 words
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in EMSLIPConstraints::Parse(): ");
                constraints_ = null;
                return false;
            }
        }

        /// <summary>Packs all inner data into array</summary>
        public bool Pack(ref byte[] buffer, out Int32 checkSum1, out Int32 checkSum2)
        {
            try
            {
                checkSum1 = checkSum2 = 0;
                if (constraints_ == null) return false;

                buffer = new byte[this.Size];
                int shift = 0;
                Int32 tmp = 0;

                for (int iSet = 0; iSet < CntConstraintsSets; iSet++) // constraints types
                {
                    for (int iConst = 0; iConst < CntConstraints; iConst++)
                    {
                        Conversions.signed_float2w_Q_15_16_2_bytes(constraints_[iSet, iConst],
                                ref buffer, shift);

                        if (iSet == 4)
                        {
                            tmp = Conversions.bytes_2_int(ref buffer, shift);
                            //System.Diagnostics.Debug.WriteLine(tmp.ToString());
                            checkSum1 += tmp;	// user1
                        }
                        if (iSet == 5)
                        {
                            tmp = Conversions.bytes_2_int(ref buffer, shift);
                            checkSum2 += tmp;	// user2
                        }

                        shift += 4;
                    }
                }
                checkSum1 += 0x23456789;
                checkSum2 += 0x23456789;

                return true;
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in EMSLIPConstraints::Pack(): ");
                checkSum1 = checkSum2 = 0;
                return false;
            }
        }

        #endregion
    }

    public class EtPQP_A_ConstraintsDetailed
    {
        [XmlElement]
        public float Sets_f_synchro_down95;
        [XmlElement]
        public float Sets_f_synchro_down100;
        [XmlElement]
        public float Sets_f_synchro_up95;
        [XmlElement]
        public float Sets_f_synchro_up100;
        [XmlElement]
        public float Sets_f_isolate_down95;
        [XmlElement]
        public float Sets_f_isolate_down100;
        [XmlElement]
        public float Sets_f_isolate_up95;
        [XmlElement]
        public float Sets_f_isolate_up100;
		[XmlElement]
		public float Sets_u_deviation_down95;
		[XmlElement]
		public float Sets_u_deviation_down100;
        [XmlElement]
        public float Sets_u_deviation_up95;
        [XmlElement]
        public float Sets_u_deviation_up100;
        [XmlElement]
        public float Sets_flick_short_down95;
        [XmlElement]
        public float Sets_flick_short_down100;
        [XmlElement]
        public float Sets_flick_long_up95;
        [XmlElement]
        public float Sets_flick_long_up100;

        [XmlArray]
        public float[] Sets_k_harm_95 = new float[39];
        [XmlArray]
        public float[] Sets_k_harm_100 = new float[39];
        [XmlElement]
        public float Sets_k_harm_total_95;
        [XmlElement]
        public float Sets_k_harm_total_100;

        [XmlElement]
        public float Sets_k2u_95;
        [XmlElement]
        public float Sets_k2u_100;
        [XmlElement]
        public float Sets_k0u_95;
        [XmlElement]
        public float Sets_k0u_100;

        public EtPQP_A_ConstraintsDetailed()
        { }

		public EtPQP_A_ConstraintsDetailed(ref DbService dbService)
        {
            try
            {
                Sets_f_synchro_down95 = (int)dbService.DataReaderData("sets_f_synchro_down95");
				Sets_f_synchro_down100 = (int)dbService.DataReaderData("sets_f_synchro_down100");
				Sets_f_synchro_up95 = (int)dbService.DataReaderData("sets_f_synchro_up95");
				Sets_f_synchro_up100 = (int)dbService.DataReaderData("sets_f_synchro_up100");
				Sets_f_isolate_down95 = (int)dbService.DataReaderData("sets_f_isolate_down95");
				Sets_f_isolate_down100 = (int)dbService.DataReaderData("sets_f_isolate_down100");
				Sets_f_isolate_up95 = (int)dbService.DataReaderData("sets_f_isolate_up95");
				Sets_f_isolate_up100 = (int)dbService.DataReaderData("sets_f_isolate_up100");

				Sets_u_deviation_down95 = (int)dbService.DataReaderData("sets_u_deviation_down95");
				Sets_u_deviation_down100 = (int)dbService.DataReaderData("sets_u_deviation_down100");
				Sets_u_deviation_up95 = (int)dbService.DataReaderData("sets_u_deviation_up95");
				Sets_u_deviation_up100 = (int)dbService.DataReaderData("sets_u_deviation_up100");

				Sets_flick_short_down95 = (int)dbService.DataReaderData("sets_flick_short_down95");
				Sets_flick_short_down100 = (int)dbService.DataReaderData("sets_flick_short_down100");
				Sets_flick_long_up95 = (int)dbService.DataReaderData("sets_flick_long_up95");
				Sets_flick_long_up100 = (int)dbService.DataReaderData("sets_flick_long_up100");

                for (int iSet = 0; iSet < 39; ++iSet)
                {
                    string param95 = "sets_k_harm_95_" + (iSet + 2).ToString();
                    string param100 = "sets_k_harm_100_" + (iSet + 2).ToString();
					Sets_k_harm_95[iSet] = (int)dbService.DataReaderData(param95);
					Sets_k_harm_100[iSet] = (int)dbService.DataReaderData(param100);
                }
				Sets_k_harm_total_95 = (int)dbService.DataReaderData("sets_k_harm_total_95");
				Sets_k_harm_total_100 = (int)dbService.DataReaderData("sets_k_harm_total_100");

				Sets_k2u_95 = (int)dbService.DataReaderData("sets_k2u_95");
				Sets_k2u_100 = (int)dbService.DataReaderData("sets_k2u_100");
				Sets_k0u_95 = (int)dbService.DataReaderData("sets_k0u_95");
				Sets_k0u_100 = (int)dbService.DataReaderData("sets_k0u_100");
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in EtPQP_A_ConstraintsDetailed():");
                throw;
            }
        }

		public EtPQP_A_ConstraintsDetailed(float[] constr)
		{
			if (constr.Length != EtPQPAConstraints.CntConstraints)
			{
				throw new EmException("EtPQP_A_ConstraintsDetailed: Invalid length! " + constr.Length);
			}

			Sets_f_synchro_down95 = constr[0];
			Sets_f_synchro_down100 = constr[1];
			Sets_f_synchro_up95 = constr[2];
			Sets_f_synchro_up100 = constr[3];
			Sets_f_isolate_down95 = constr[4];
			Sets_f_isolate_down100 = constr[5];
			Sets_f_isolate_up95 = constr[6];
			Sets_f_isolate_up100 = constr[7];
			Sets_u_deviation_down95 = constr[8];
			Sets_u_deviation_down100 = constr[9];
			Sets_u_deviation_up95 = constr[10];
			Sets_u_deviation_up100 = constr[11];
			Sets_flick_short_down95 = constr[12];
			Sets_flick_short_down100 = constr[13];
			Sets_flick_long_up95 = constr[14];
			Sets_flick_long_up100 = constr[15];

			for (int iSet = 0; iSet < 39; ++iSet)
			{
				Sets_k_harm_95[iSet] = constr[iSet + 16];
				Sets_k_harm_100[iSet] = constr[iSet + 55];
			}
			Sets_k_harm_total_95 = constr[94];
			Sets_k_harm_total_100 = constr[95];

			Sets_k2u_95 = constr[96];
			Sets_k2u_100 = constr[97];
			Sets_k0u_95 = constr[98];
			Sets_k0u_100 = constr[99];
		}

        public float[] ConstraintsArray
        {
            get
            {
                float[] constr = new float[EtPQPAConstraints.CntConstraints];

                constr[0] = Sets_f_synchro_down95;
                constr[1] = Sets_f_synchro_down100;
                constr[2] = Sets_f_synchro_up95;
                constr[3] = Sets_f_synchro_up100;
                constr[4] = Sets_f_isolate_down95;
                constr[5] = Sets_f_isolate_down100;
                constr[6] = Sets_f_isolate_up95;
                constr[7] = Sets_f_isolate_up100;
				constr[8] = Sets_u_deviation_down95;
				constr[9] = Sets_u_deviation_down100;
                constr[10] = Sets_u_deviation_up95;
                constr[11] = Sets_u_deviation_up100;
                constr[12] = Sets_flick_short_down95;
                constr[13] = Sets_flick_short_down100;
                constr[14] = Sets_flick_long_up95;
                constr[15] = Sets_flick_long_up100;

                for (int iSet = 0; iSet < 39; ++iSet)
                {
                    constr[iSet + 16] = Sets_k_harm_95[iSet];
                    constr[iSet + 55] = Sets_k_harm_100[iSet];
                }
                constr[94] = Sets_k_harm_total_95;
                constr[95] = Sets_k_harm_total_100;

                constr[96] = Sets_k2u_95;
                constr[97] = Sets_k2u_100;
                constr[98] = Sets_k0u_95;
                constr[99] = Sets_k0u_100;

                return constr;
            }
        }
    }
}
