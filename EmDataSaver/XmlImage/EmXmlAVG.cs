using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using DeviceIO;
using EmServiceLib;

namespace EmDataSaver.XmlImage
{
	public enum AvgSubTypes { Main = 0, Harmonics = 1, HarmonicPowersAndAngles = 2 }

	//public enum AvgTypes { Bad = 0, ThreeSec = 1, OneMin = 2, ThirtyMin = 3 }
	
	public class EmXmlAVG : EmXmlArchivePart
	{
		#region Fields
				
		//private int avgTime_;
		private uint avgNum_;
		private AvgSubTypes[] avgSub_ = null;
		private AvgTypes avgType_;
		private SavingInterface.frmAvgParams.MaskAvgArray masksAvg_;
		
		#endregion

		#region Properties

		[XmlAttribute(AttributeName="TimeIndex")]
		public int AvgTime
		{
			//get { return avgTime_; }
			set {
				switch (value)
				{
					case 0: avgType_ = AvgTypes.ThreeSec; break;
					case 1: avgType_ = AvgTypes.OneMin; break;
					case 2: avgType_ = AvgTypes.ThirtyMin; break;
					default: avgType_ = AvgTypes.Bad; break;
				}
			}
		}

		[XmlAttribute(AttributeName="NumberOfRecords")]
		public uint AvgNum
		{
			get { return avgNum_; }
			set { avgNum_ = value; }
		}

		[XmlAttribute(AttributeName="SubTypes")]
		public AvgSubTypes[] AvgSub
		{
			get { return avgSub_; }
			set { avgSub_ = value; }
		}

		[XmlAttribute(AttributeName = "AvgTypes")]
		public AvgTypes AvgType
		{
			get { return avgType_; }
			set { avgType_ = value; }
		}

		[XmlElement]
		public SavingInterface.frmAvgParams.MaskAvgArray MasksAvg
		{
			get { return masksAvg_; }
			set { masksAvg_ = value; }
		}

		#endregion
	}

    public class EmXmlAVG_PQP_A : EmXmlArchivePart
    {
        #region Fields

        //private int avgTime_;
        private uint avgNum_;
        private AvgSubTypes[] avgSub_ = null;
        private AvgTypes_PQP_A avgType_;

        #endregion

        #region Properties

        [XmlAttribute(AttributeName = "TimeIndex")]
        public int AvgTime
        {
            //get { return avgTime_; }
            set
            {
                switch (value)
                {
                    case 0: avgType_ = AvgTypes_PQP_A.ThreeSec; break;
                    case 1: avgType_ = AvgTypes_PQP_A.TenMin; break;
                    case 2: avgType_ = AvgTypes_PQP_A.TwoHours; break;
                    default: avgType_ = AvgTypes_PQP_A.Bad; break;
                }
            }
        }

        [XmlAttribute(AttributeName = "NumberOfRecords")]
        public uint AvgNum
        {
            get { return avgNum_; }
            set { avgNum_ = value; }
        }

        [XmlAttribute(AttributeName = "SubTypes")]
        public AvgSubTypes[] AvgSub
        {
            get { return avgSub_; }
            set { avgSub_ = value; }
        }

        [XmlAttribute(AttributeName = "AvgTypes_PQP_A")]
        public AvgTypes_PQP_A AvgType
        {
            get { return avgType_; }
            set { avgType_ = value; }
        }

        #endregion
    }
}
