using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EmServiceLib;

namespace DeviceIO.Constraints
{
    public class EMSLIPConstraints : EMConstraintsBase
    {
        #region Fields

        // кол-во уставок в поднаборе
        public const int CntConstraints = 100;

        // массив уставок: 6 наборов, в каждом из них 2 поднабора,
        // в каждом поднаборе 100 уставок (100 слов)
        private float[, ,] constraints_ = new float[CntConstraintsSets, CntSubsets, CntConstraints];

        #endregion

        #region Properties

        public float[, ,] Constraints
        {
            get { return constraints_; }
        }

        /// <summary>Gets size of this memory region in bytes</summary>
        public ushort Size
        {
            get
            {
                return (ushort)(CntConstraintsSets * CntSubsets * CntConstraints * 2);
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
                    vals[iSet, 4, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 5];
                    vals[iSet, 4, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 7];
                    vals[iSet, 4, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 5];
                    vals[iSet, 4, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 7];

                    // δU-''
                    vals[iSet, 5, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 4];
                    vals[iSet, 5, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 6];
                    vals[iSet, 5, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 4];
                    vals[iSet, 5, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 6];

                    // δU+'
                    vals[iSet, 2, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 9];
                    vals[iSet, 2, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 11];
                    vals[iSet, 2, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 9];
                    vals[iSet, 2, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 11];

                    // δU-'
                    vals[iSet, 3, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 8];
                    vals[iSet, 3, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 10];
                    vals[iSet, 3, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 8];
                    vals[iSet, 3, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 10];

                    // δU+
                    vals[iSet, 6, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 13];
                    vals[iSet, 6, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 15];
                    vals[iSet, 6, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 13];
                    vals[iSet, 6, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 15];

                    // δU-
                    vals[iSet, 7, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 12];
                    vals[iSet, 7, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 14];
                    vals[iSet, 7, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 12];
                    vals[iSet, 7, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 14];

                    // K2u
                    vals[iSet, 8, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 16];
                    vals[iSet, 8, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 17];
                    vals[iSet, 8, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 16];
                    vals[iSet, 8, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 17];

                    // K0u
                    vals[iSet, 9, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 18];
                    vals[iSet, 9, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 19];
                    vals[iSet, 9, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 18];
                    vals[iSet, 9, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 19];

                    // Ku2 - Ku40
                    int startNDZ = 22, startPDZ = 61;
                    for (int iRow = 10; iRow < 49; ++iRow)
                    {
                        vals[iSet, iRow, 0] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, startNDZ];
                        vals[iSet, iRow, 1] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, startPDZ];
                        vals[iSet, iRow, 2] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, startNDZ];
                        vals[iSet, iRow, 3] =
                            constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, startPDZ];

                        startNDZ++;
                        startPDZ++;
                    }

                    // Ku
                    vals[iSet, 49, 0] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 20];
                    vals[iSet, 49, 1] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 21];
                    vals[iSet, 49, 2] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 20];
                    vals[iSet, 49, 3] = constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 21];
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
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 5] = vals[iSet, 4, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 7] = vals[iSet, 4, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 5] = vals[iSet, 4, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 7] = vals[iSet, 4, 3];

                    // δU-''
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 4] = vals[iSet, 5, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 6] = vals[iSet, 5, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 4] = vals[iSet, 5, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 6] = vals[iSet, 5, 3];

                    // δU+'
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 9] = vals[iSet, 2, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 11] = vals[iSet, 2, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 9] = vals[iSet, 2, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 11] = vals[iSet, 2, 3];

                    // δU-'
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 8] = vals[iSet, 3, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 10] = vals[iSet, 3, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 8] = vals[iSet, 3, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 10] = vals[iSet, 3, 3];

                    // δU+
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 13] = vals[iSet, 6, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 15] = vals[iSet, 6, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 13] = vals[iSet, 6, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 15] = vals[iSet, 6, 3];

                    // δU-
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 12] = vals[iSet, 7, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 14] = vals[iSet, 7, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 12] = vals[iSet, 7, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 14] = vals[iSet, 7, 3];

                    // K2u
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 16] = vals[iSet, 8, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 17] = vals[iSet, 8, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 16] = vals[iSet, 8, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 17] = vals[iSet, 8, 3];

                    // K0u
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 18] = vals[iSet, 9, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 19] = vals[iSet, 9, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 18] = vals[iSet, 9, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 19] = vals[iSet, 9, 3];

                    // Ku2 - Ku40
                    int startNDZ = 22, startPDZ = 61;
                    for (int iRow = 10; iRow < 49; ++iRow)
                    {
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, startNDZ] = vals[iSet, iRow, 0];
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, startPDZ] = vals[iSet, iRow, 1];
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, startNDZ] = vals[iSet, iRow, 2];
                        constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, startPDZ] = vals[iSet, iRow, 3];

                        startNDZ++;
                        startPDZ++;
                    }

                    // Ku
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 20] = vals[iSet, 49, 0];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH4W, 21] = vals[iSet, 49, 1];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 20] = vals[iSet, 49, 2];
                    constraints_[iSet, (int)ConstraintsSubType.TYPE_3PH3W, 21] = vals[iSet, 49, 3];
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
                constraints_ = new float[CntConstraintsSets /*6*/, CntSubsets /*2*/,
                                        CntConstraints /*100*/];
                int shift = 0;

                for (int iSet = 0; iSet < CntConstraintsSets; iSet++)
                {
                    for (int iSubSet = 0; iSubSet < CntSubsets; iSubSet++)
                    {
                        for (int iConst = 0; iConst < CntConstraints; iConst++)
                        {
                            // первые 4 уставки - частота, для нее другой формат
                            if (iConst < 4)
                                constraints_[iSet, iSubSet, iConst] =
                                    Conversions.bytes_2_signed_float8192(ref array, shift);
                            else
                                constraints_[iSet, iSubSet, iConst] =
                                    Conversions.bytes_2_signed_float1024(ref array, shift);

                            shift += 2; // 1 word
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
            try
            {
                if (constraints_ == null) return null;

                byte[] array = new byte[this.Size];
                int shift = 0;

                for (int iSet = 0; iSet < CntConstraintsSets; iSet++) // constraints types
                {
                    for (int iSubSet = 0; iSubSet < CntSubsets; iSubSet++)
                    {
                        for (int iConst = 0; iConst < CntConstraints; iConst++)
                        {
                            // первые 4 уставки - частота, для нее другой формат
                            if (iConst < 4)
                                Conversions.signed_float8192_2_bytes_new(constraints_[iSet, iSubSet, iConst],
                                ref array, shift);
                            else
                                Conversions.signed_float1024_2_bytes(constraints_[iSet, iSubSet, iConst],
                                ref array, shift);

                            shift += 2;
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
