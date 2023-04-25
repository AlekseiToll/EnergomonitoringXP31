using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceIO.Constraints
{
    public abstract class EMConstraintsBase
    {
        public enum ConstraintsType
        {
            GOST13109_0_38 = 0,			// ГОСТ 13109 0-38кВ
            GOST13109_6_20 = 1,			// ГОСТ 13109 6-20кВ
            GOST13109_35 = 2,			// ГОСТ 13109 35кВ
            GOST13109_110_330 = 3,			// ГОСТ 13109 110-330кВ
            USER1 = 4,			// Пользовательский 1
            USER2 = 5			// Пользовательский 2
        }

        public enum ConstraintsSubType
        {
            TYPE_3PH4W = 0,
            TYPE_3PH3W = 1
        }

        // кол-во наборов уставок
        public const int CntConstraintsSets = 6;
        // кол-во поднаборов в одном наборе
        public const int CntSubsets = 2;
    }
}
