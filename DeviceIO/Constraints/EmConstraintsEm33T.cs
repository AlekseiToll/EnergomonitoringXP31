using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DeviceIO.Memory;
using EmServiceLib;

namespace DeviceIO.Constraints
{
    public class EM33TConstraints : EMConstraintsBase
    {
        #region Fields

        // кол-во уставок в поднаборе
        public const int CntConstraints = 96;

        // массив уставок: 6 наборов, в каждом из них 2 поднабора,
        // в каждом поднаборе 100 уставок (100 слов)
        private float[, ,] constraints_ = new float[CntConstraintsSets, CntSubsets, CntConstraints];

        #endregion

        #region Properties

        /// <summary>Gets address of the memory</summary>
        /// <remarks>
        /// (ushort)Address[0] - FRAM Address
        /// (ushort)Address[1] - RAM Page 
        /// (ushort)Address[2] - RAM Shift
        /// </remarks>
        public AddressMemory Address
        {
            get
            {
                AddressMemory addr = new AddressMemory();
                addr.FRAM.Address = 0x0680;
                addr.FRAM.Exists = true;
                addr.RAM.Page = 0x0e;
                addr.RAM.Shift = 0x740;
                addr.RAM.Exists = true;
                return addr;
            }
        }

        public float[, ,] Constraints
        {
            get { return constraints_; }
        }

        /// <summary>Gets size of this memory region in bytes</summary>
        public ushort Size
        {
            get
            {
                // размер одного набора
                int size1set = (4 + 52 +		// резерв
                    4 * 4 +						// частота
                    92 * 2);					// остальное
                return (ushort)(size1set * CntSubsets * CntConstraintsSets);
            }
        }

		public float[, ,] EmptyConstraintsForTable
		{
			get
			{
				return new float[CntConstraintsSets, CntConstraints / 2, CntSubsets * 2];
			}
		}

        // уставки в формате таблицы
        public float[, ,] ConstraintsForTable
        {
            get
            {
                float[, ,] vals = new float[CntConstraintsSets, CntConstraints / 2, CntSubsets * 2];

                for (int iSet = 0; iSet < CntConstraintsSets; ++iSet)
                {
                    // ∆F+
                    vals[iSet, 0, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 1];
                    vals[iSet, 0, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 3];
                    vals[iSet, 0, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 1];
                    vals[iSet, 0, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 3];

                    // ∆F-
                    vals[iSet, 1, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 0];
                    vals[iSet, 1, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 2];
                    vals[iSet, 1, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 0];
                    vals[iSet, 1, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 2];

                    // δU+''
                    vals[iSet, 2, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 5];
                    vals[iSet, 2, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 7];
                    vals[iSet, 2, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 5];
                    vals[iSet, 2, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 7];

                    // δU-''
                    vals[iSet, 3, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 4];
                    vals[iSet, 3, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 6];
                    vals[iSet, 3, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 4];
                    vals[iSet, 3, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 6];

                    // δU+'
                    vals[iSet, 4, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 9];
                    vals[iSet, 4, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 11];
                    vals[iSet, 4, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 9];
                    vals[iSet, 4, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 11];

                    // δU-'
                    vals[iSet, 5, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 8];
                    vals[iSet, 5, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 10];
                    vals[iSet, 5, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 8];
                    vals[iSet, 5, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 10];

                    // δU+
                    //vals[iSet, 6, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 13];
                    //vals[iSet, 6, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 15];
                    //vals[iSet, 6, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 13];
                    //vals[iSet, 6, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 15];

                    //// δU-
                    //vals[iSet, 7, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 12];
                    //vals[iSet, 7, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 14];
                    //vals[iSet, 7, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 12];
                    //vals[iSet, 7, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 14];

                    // K2u
                    vals[iSet, 6, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 12];
                    vals[iSet, 6, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 13];
                    vals[iSet, 6, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 12];
                    vals[iSet, 6, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 13];

                    // K0u
                    vals[iSet, 7, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 14];
                    vals[iSet, 7, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 15];
                    vals[iSet, 7, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 14];
                    vals[iSet, 7, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 15];

                    // Ku2 - Ku40
                    int start = 16;
                    for (int iRow = 8; iRow < 47; ++iRow)
                    {
                        vals[iSet, iRow, 0] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, start];
                        vals[iSet, iRow, 1] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, start + 1];
                        vals[iSet, iRow, 2] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, start];
                        vals[iSet, iRow, 3] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, start + 1];

                        start += 2;
                    }

                    // Ku
                    vals[iSet, 47, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 94];
                    vals[iSet, 47, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 95];
                    vals[iSet, 47, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 94];
                    vals[iSet, 47, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 95];
                }

                return vals;
            }

            set
            {
                float[, ,] vals = value;
                if (vals.Length != (CntConstraintsSets * (CntConstraints / 2) * (CntSubsets * 2)))
                    throw new EmException("ConstraintsForTable: invalid value length!");
                //new float[CntConstraintsSets, CntConstraints / 2, CntSubsets * 2];

                for (int iSet = 0; iSet < CntConstraintsSets; ++iSet)
                {
                    // ∆F+
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 1] = vals[iSet, 0, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 3] = vals[iSet, 0, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 1] = vals[iSet, 0, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 3] = vals[iSet, 0, 3];

                    // ∆F-
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 0] = vals[iSet, 1, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 2] = vals[iSet, 1, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 0] = vals[iSet, 1, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 2] = vals[iSet, 1, 3];

                    // δU+''
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 5] = vals[iSet, 2, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 7] = vals[iSet, 2, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 5] = vals[iSet, 2, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 7] = vals[iSet, 2, 3];

                    // δU-''
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 4] = vals[iSet, 3, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 6] = vals[iSet, 3, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 4] = vals[iSet, 3, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 6] = vals[iSet, 3, 3];

                    // δU+'
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 9] = vals[iSet, 4, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 11] = vals[iSet, 4, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 9] = vals[iSet, 4, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 11] = vals[iSet, 4, 3];

                    // δU-'
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 8] = vals[iSet, 5, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 10] = vals[iSet, 5, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 8] = vals[iSet, 5, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 10] = vals[iSet, 5, 3];

                    // δU+
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 13] = vals[iSet, 6, 0];
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 15] = vals[iSet, 6, 1];
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 13] = vals[iSet, 6, 2];
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 15] = vals[iSet, 6, 3];

                    // δU-
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 12] = vals[iSet, 7, 0];
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 14] = vals[iSet, 7, 1];
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 12] = vals[iSet, 7, 2];
                    //constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 14] = vals[iSet, 7, 3];

                    // K2u
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 12] = vals[iSet, 6, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 13] = vals[iSet, 6, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 12] = vals[iSet, 6, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 13] = vals[iSet, 6, 3];

                    // K0u
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 14] = vals[iSet, 7, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 15] = vals[iSet, 7, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 14] = vals[iSet, 7, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 15] = vals[iSet, 7, 3];

                    // Ku2 - Ku40
                    int start = 16;
                    for (int iRow = 8; iRow < 47; ++iRow)
                    {
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, start] = vals[iSet, iRow, 0];
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, start + 1] = vals[iSet, iRow, 1];
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, start] = vals[iSet, iRow, 2];
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, start + 1] = vals[iSet, iRow, 3];

                        start += 2;
                    }

                    // Ku
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 94] = vals[iSet, 47, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 95] = vals[iSet, 47, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 94] = vals[iSet, 47, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 95] = vals[iSet, 47, 3];
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

            int size = 256;

            try
            {
                constraints_ = new float[CntConstraintsSets, CntSubsets, CntConstraints];
                int shift = 4;  // первые 4 байта - резерв

                for (int iSet = 0; iSet < CntConstraintsSets; iSet++)
                {
                    for (int iSubSet = 0; iSubSet < CntSubsets; iSubSet++)
                    {
                        shift = (iSubSet + 2 * iSet) * size;
                        shift += 4;

                        for (int iConst = 0; iConst < CntConstraints; iConst++)
                        {
                            // первые 4 уставки - частота, для нее другой формат
                            if (iConst < 4)
                            {
                                constraints_[iSet, iSubSet, iConst] =
                                    Conversions.bytes_2_signed_float2w65536(ref array, shift);
                                shift += 4; // 2 words
                            }
                            else if (iConst < 12)		// следующие 8 уставок
                            {
                                constraints_[iSet, iSubSet, iConst] =
                                    Conversions.bytes_2_signed_float1w65536_percent(ref array, shift);
                                shift += 2; // 1 word
                            }
                            else // остальные уставки
                            {
                                constraints_[iSet, iSubSet, iConst] =
                                    Conversions.bytes_2_float1w65536_percent(ref array, shift);
                                shift += 2; // 1 word
                            }
                        }
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
        public byte[] Pack()
        {
            int size = 256;

            try
            {
                if (constraints_ == null) return null;

                byte[] array = new byte[this.Size];
                int shift = 4;	// первые 4 байта - резерв

                for (int iSet = 0; iSet < CntConstraintsSets; iSet++) // constraints types
                {
                    for (int iSubSet = 0; iSubSet < CntSubsets; iSubSet++)
                    {
                        shift = (iSubSet + 2 * iSet) * size;
                        shift += 4;

                        for (int iConst = 0; iConst < CntConstraints; iConst++)
                        {
                            // первые 4 уставки - частота, для нее другой формат
                            if (iConst < 4)
                            {
                                Conversions.signed_float2w65536_2_bytes(
                                    constraints_[iSet, iSubSet, iConst],
                                    ref array, shift);
                                shift += 4; // 2 words
                            }
                            else if (iConst < 12)		// следующие 8 уставок
                            {
                                Conversions.signed_float_percent_1w65536_2_bytes(
                                    constraints_[iSet, iSubSet, iConst],
                                    ref array, shift);
                                shift += 2;
                            }
                            else // остальные уставки
                            {
                                Conversions.float_1w65536_percent_2_bytes(
                                    constraints_[iSet, iSubSet, iConst],
                                    ref array, shift);
                                shift += 2;
                            }
                        }
                    }
                }

                return array;
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in EMSLIPConstraints::Pack(): ");
                return null;
            }
        }

        #endregion
    }
}
