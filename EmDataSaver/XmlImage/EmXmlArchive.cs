using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using DeviceIO;
using DeviceIO.Constraints;
using EmServiceLib;

namespace EmDataSaver.XmlImage
{
	[Serializable]
	public class EmXmlArchive
	{
		#region Fields

		private Int32 objectId_;
		private string objectName_;
		private ConnectScheme connectionScheme_;
		private float f_Nominal_;
		private float u_NominalLinear_;
		private float u_NominalPhase_;
		private float i_NominalPhase_;
		private ushort currentTransducerIndex_;
		private short constraintType_;
		private float u_Limit_;
		private float i_Limit_;
		private DateTime commonBegin_;
		private DateTime commonEnd_;
		private uint dnoTimer_;
		private AvgTypes avgType_;
		// fliker
		private short t_fliker_;

		private EmXmlPQP[] archivePQP_ = null;
		private EmXmlAVG archiveAVG_ = null;
		private EmXmlDNS archiveDNS_ = null;

		private TimeSpan ml_start_time1;
		private TimeSpan ml_end_time1;
		private TimeSpan ml_start_time2;
		private TimeSpan ml_end_time2; 

		#endregion

		#region Properties

		[XmlAttribute]
		public Int32 ObjectId
		{
			get { return objectId_; }
			set { objectId_ = value; }
		}

		[XmlAttribute]
		public string ObjectName
		{
			get { return objectName_; }
			set { objectName_ = value; }
		}

		[XmlAttribute]
		public DateTime CommonBegin
		{
			get { return commonBegin_; }
			set { commonBegin_ = value; }
		}

		[XmlAttribute]
		public DateTime CommonEnd
		{
			get { return commonEnd_; }
			set { commonEnd_ = value; }
		}

		[XmlAttribute]
		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme_; }
			set { connectionScheme_ = value; }
		}

		[XmlAttribute]
		public float F_Nominal
		{
			get { return f_Nominal_; }
			set { f_Nominal_ = value; }
		}

		[XmlAttribute]
		public float U_NominalLinear
		{
			get { return u_NominalLinear_; }
			set { u_NominalLinear_ = value; }
		}

		[XmlAttribute]
		public float U_NominalPhase
		{
			get { return u_NominalPhase_; }
			set { u_NominalPhase_ = value; }
		}

		[XmlAttribute]
		public float I_NominalPhase
		{
			get { return i_NominalPhase_; }
			set { i_NominalPhase_ = value; }
		}

		[XmlAttribute]
		public float U_Limit
		{
			get { return u_Limit_; }
			set { u_Limit_ = value; }
		}

		[XmlAttribute]
		public float I_Limit
		{
			get { return i_Limit_; }
			set { i_Limit_ = value; }
		}

		[XmlAttribute]
		public ushort CurrentTransducerIndex
		{
			get { return currentTransducerIndex_; }
			set { currentTransducerIndex_ = value; }
		}

		[XmlAttribute]
		public short ConstraintType
		{
			get { return constraintType_; }
			set { constraintType_ = value; }
		}

		[XmlElement]
		public EmXmlPQP[] ArchivePQP
		{
			get { return archivePQP_; }
			set { archivePQP_ = value; }
		}

		[XmlElement]
		public EmXmlDNS ArchiveDNS
		{
			get { return archiveDNS_; }
			set { archiveDNS_ = value; }
		}

		[XmlElement]
		public EmXmlAVG ArchiveAVG
		{
			get { return archiveAVG_; }
			set { archiveAVG_ = value; }
		}

		[XmlAttribute]
		public AvgTypes AvgType
		{
			get { return avgType_; }
			set { avgType_ = value; }
		}

		[XmlAttribute]
		public ushort AvgTime
		{
			set
			{
				switch (value)
				{
					case 0: avgType_ = AvgTypes.ThreeSec; break;
					case 1: avgType_ = AvgTypes.OneMin; break;
					case 2: avgType_ = AvgTypes.ThirtyMin; break;
					default: avgType_ = AvgTypes.Bad; break;
				}
			}
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		[XmlAttribute]
		public uint DnsTimer
		{
			get { return dnoTimer_; }
			set { dnoTimer_ = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime1
		{
			get { return ml_start_time1; }
			set { ml_start_time1 = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime1
		{
			get { return ml_end_time1; }
			set { ml_end_time1 = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime2
		{
			get { return ml_start_time2; }
			set { ml_start_time2 = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime2
		{
			get { return ml_end_time2; }
			set { ml_end_time2 = value; }
		}
				
		#endregion
	}

	[Serializable]
	public class EtPQP_A_XmlArchive
	{
		#region Fields

		private UInt32 registrationId_;
		private string objectName_;
		private ConnectScheme connectionScheme_;
		private short constraintType_;
		private float[] constraints_ = new float[EtPQPAConstraints.CntConstraints];
		private DateTime commonBegin_;
		private DateTime commonEnd_;

		private EmXmlPQP_PQP_A[] archivePQP_ = null;
		private EmXmlAVG_PQP_A[] archiveAVG_ = null;
		private EmXmlDNS[] archiveDNS_ = null;

		private SystemInfoEtPQP_A sysInfo_;

		private string devVersion_;
		private DateTime devVersionDate_;

		#endregion

		#region Properties

		[XmlElement]
		public SystemInfoEtPQP_A SysInfo
		{
			get { return sysInfo_; }
			set { sysInfo_ = value; }
		}

		[XmlAttribute]
		public UInt32 RegistrationId
		{
			get { return registrationId_; }
			set { registrationId_ = value; }
		}

		[XmlAttribute]
		public string ObjectName
		{
			get { return objectName_; }
			set { objectName_ = value; }
		}

		[XmlAttribute]
		public DateTime CommonBegin
		{
			get { return commonBegin_; }
			set { commonBegin_ = value; }
		}

		[XmlAttribute]
		public DateTime CommonEnd
		{
			get { return commonEnd_; }
			set { commonEnd_ = value; }
		}

		[XmlAttribute]
		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme_; }
			set { connectionScheme_ = value; }
		}

		[XmlAttribute]
		public short ConstraintType
		{
			get { return constraintType_; }
			set { constraintType_ = value; }
		}

		[XmlAttribute]
		public float[] Constraints
		{
			get { return constraints_; }
			set { constraints_ = value; }
		}

		[XmlElement]
		public EmXmlPQP_PQP_A[] ArchivePQP
		{
			get { return archivePQP_; }
			set { archivePQP_ = value; }
		}

		[XmlElement]
		public EmXmlDNS[] ArchiveDNS
		{
			get { return archiveDNS_; }
			set { archiveDNS_ = value; }
		}

		[XmlElement]
        public EmXmlAVG_PQP_A[] ArchiveAVG
		{
			get { return archiveAVG_; }
			set { archiveAVG_ = value; }
		}

		public string DevVersion
		{
			get { return devVersion_; }
			set { devVersion_ = value; }
		}

		public DateTime DevVersionDate
		{
			get { return devVersionDate_; }
			set { devVersionDate_ = value; }
		}

		#endregion
	}

	[Serializable]
	public class EtPQPXmlArchive
	{
		#region Fields

		private Int32 objectId_;
		private Int32 globalObjectId_;
		private string objectName_;
		private ConnectScheme connectionScheme_;
		private float f_Nominal_;
		private float u_NominalLinear_;
		private float u_NominalPhase_;
		private float i_NominalPhase_;
		private ushort currentTransducerIndex_;
		private short constraintType_;
		private float u_Limit_;
		private float i_Limit_;
		private DateTime commonBegin_;
		private DateTime commonEnd_;
		private short t_fliker_;

		private EmXmlPQP[] archivePQP_ = null;
		private EmXmlAVG[] archiveAVG_ = null;
		private EmXmlDNS[] archiveDNS_ = null;

		private TimeSpan ml_start_time1;
		private TimeSpan ml_end_time1;
		private TimeSpan ml_start_time2;
		private TimeSpan ml_end_time2;

		#endregion

		#region Properties

		[XmlAttribute]
		public Int32 ObjectId
		{
			get { return objectId_; }
			set { objectId_ = value; }
		}

		[XmlAttribute]
		public Int32 GlobalObjectId
		{
			get { return globalObjectId_; }
			set { globalObjectId_ = value; }
		}

		[XmlAttribute]
		public string ObjectName
		{
			get { return objectName_; }
			set { objectName_ = value; }
		}

		[XmlAttribute]
		public DateTime CommonBegin
		{
			get { return commonBegin_; }
			set { commonBegin_ = value; }
		}

		[XmlAttribute]
		public DateTime CommonEnd
		{
			get { return commonEnd_; }
			set { commonEnd_ = value; }
		}

		[XmlAttribute]	
		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme_; }
			set { connectionScheme_ = value; }
		}

		[XmlAttribute]
		public float F_Nominal
		{
			get { return f_Nominal_; }
			set { f_Nominal_ = value; }
		}

		[XmlAttribute]
		public float U_NominalLinear
		{
			get { return u_NominalLinear_; }
			set { u_NominalLinear_ = value; }
		}

		[XmlAttribute]
		public float U_NominalPhase
		{
			get { return u_NominalPhase_; }
			set { u_NominalPhase_ = value; }
		}

		[XmlAttribute]
		public float I_NominalPhase
		{
			get { return i_NominalPhase_; }
			set { i_NominalPhase_ = value; }
		}

		[XmlAttribute]
		public float U_Limit
		{
			get { return u_Limit_; }
			set { u_Limit_ = value; }
		}

		[XmlAttribute]
		public float I_Limit
		{
			get { return i_Limit_; }
			set { i_Limit_ = value; }
		}

		[XmlAttribute]
		public ushort CurrentTransducerIndex
		{
			get { return currentTransducerIndex_; }
			set { currentTransducerIndex_ = value; }
		}

		[XmlAttribute]
		public short ConstraintType
		{
			get { return constraintType_; }
			set { constraintType_ = value; }
		}

		[XmlElement]
		public EmXmlPQP[] ArchivePQP
		{
			get { return archivePQP_; }
			set { archivePQP_ = value; }
		}

		[XmlElement]
		public EmXmlDNS[] ArchiveDNS
		{
			get { return archiveDNS_; }
			set { archiveDNS_ = value; }
		}

		[XmlElement]
		public EmXmlAVG[] ArchiveAVG
		{
			get { return archiveAVG_; }
			set { archiveAVG_ = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime1
		{
			get { return ml_start_time1; }
			set { ml_start_time1 = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime1
		{
			get { return ml_end_time1; }
			set { ml_end_time1 = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime2
		{
			get { return ml_start_time2; }
			set { ml_start_time2 = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime2
		{
			get { return ml_end_time2; }
			set { ml_end_time2 = value; }
		}

		#endregion
	}

	[Serializable]
	public class EmXmlArchiveEm32
	{
		#region Fields

		private string objectName;
		private ConnectScheme connectionScheme;
		private float f_Nominal;
		private float u_NominalLinear;
		private float u_NominalPhase;
		private float i_NominalPhase;
		private ushort currentTransducerIndex;
		private float u_Limit;
		private float i_Limit;
		private DateTime commonBegin;
		private DateTime commonEnd;
		private uint dnoTimer;
		// fliker
		private short t_fliker;

		private EmXmlPQP[] archivePQP = null;
		private EmXmlAVG[] archiveAVG = null;
		private EmXmlDNS[] archiveDNS = null;

		#endregion

		#region Properties

		[XmlAttribute]
		public string ObjectName
		{
			get { return objectName; }
			set { objectName = value; }
		}

		[XmlAttribute]
		public DateTime CommonBegin
		{
			get { return commonBegin; }
			set { commonBegin = value; }
		}

		[XmlAttribute]
		public DateTime CommonEnd
		{
			get { return commonEnd; }
			set { commonEnd = value; }
		}

		[XmlAttribute]
		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme; }
			set { connectionScheme = value; }
		}

		[XmlAttribute]
		public float F_Nominal
		{
			get { return f_Nominal; }
			set { f_Nominal = value; }
		}

		[XmlAttribute]
		public float U_NominalLinear
		{
			get { return u_NominalLinear; }
			set { u_NominalLinear = value; }
		}

		[XmlAttribute]
		public float U_NominalPhase
		{
			get { return u_NominalPhase; }
			set { u_NominalPhase = value; }
		}

		[XmlAttribute]
		public float I_NominalPhase
		{
			get { return i_NominalPhase; }
			set { i_NominalPhase = value; }
		}

		[XmlAttribute]
		public float U_Limit
		{
			get { return u_Limit; }
			set { u_Limit = value; }
		}

		[XmlAttribute]
		public float I_Limit
		{
			get { return i_Limit; }
			set { i_Limit = value; }
		}

		[XmlAttribute]
		public ushort CurrentTransducerIndex
		{
			get { return currentTransducerIndex; }
			set { currentTransducerIndex = value; }
		}

		[XmlElement]
		public EmXmlPQP[] ArchivePQP
		{
			get { return archivePQP; }
			set { archivePQP = value; }
		}

		[XmlElement]
		public EmXmlDNS[] ArchiveDNS
		{
			get { return archiveDNS; }
			set { archiveDNS = value; }
		}

		[XmlElement]
		public EmXmlAVG[] ArchiveAVG
		{
			get { return archiveAVG; }
			set { archiveAVG = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker; }
			set { t_fliker = value; }
		}

		[XmlAttribute]
		public uint DnsTimer
		{
			get { return dnoTimer; }
			set { dnoTimer = value; }
		}

		#endregion
	}


	[Serializable]
	public class Em32XmlDeviceImage
	{
		#region Fields

		private long ser_number;
		private string objectName;
		private ConnectScheme connectionScheme;
		private float f_Nominal;
		private float u_NominalLinear;
		private float u_NominalPhase;
		private float i_NominalPhase;
		private ushort currentTransducerIndex;
		private float u_Limit;
		private float i_Limit;
		private string dev_version;
		private short constraintType;
		private short t_fliker;
		private TimeSpan ml_start_time1;
		private TimeSpan ml_end_time1;
		private TimeSpan ml_start_time2;
		private TimeSpan ml_end_time2;

		private EmXmlPQP[] archivePQP = null;
		private EmXmlAVG[] archiveAVG = null;
		private EmXmlDNS[] archiveDNS = null;

		#endregion

		#region Properties

		[XmlAttribute]
		public long SerialNumber
		{
			get { return ser_number; }
			set { ser_number = value; }
		}

		[XmlAttribute]
		public string ObjectName
		{
			get { return objectName; }
			set { objectName = value; }
		}

		[XmlAttribute]
		public string DevVersion
		{
			get { return dev_version; }
			set { dev_version = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime1
		{
			get { return ml_start_time1; }
			set { ml_start_time1 = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime1
		{
			get { return ml_end_time1; }
			set { ml_end_time1 = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime2
		{
			get { return ml_start_time2; }
			set { ml_start_time2 = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime2
		{
			get { return ml_end_time2; }
			set { ml_end_time2 = value; }
		}

		[XmlAttribute]
		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme; }
			set { connectionScheme = value; }
		}

		[XmlAttribute]
		public float F_Nominal
		{
			get { return f_Nominal; }
			set { f_Nominal = value; }
		}

		[XmlAttribute]
		public float U_NominalLinear
		{
			get { return u_NominalLinear; }
			set { u_NominalLinear = value; }
		}

		[XmlAttribute]
		public float U_NominalPhase
		{
			get { return u_NominalPhase; }
			set { u_NominalPhase = value; }
		}

		[XmlAttribute]
		public float I_NominalPhase
		{
			get { return i_NominalPhase; }
			set { i_NominalPhase = value; }
		}

		[XmlAttribute]
		public float U_Limit
		{
			get { return u_Limit; }
			set { u_Limit = value; }
		}

		[XmlAttribute]
		public float I_Limit
		{
			get { return i_Limit; }
			set { i_Limit = value; }
		}

		[XmlAttribute]
		public ushort CurrentTransducerIndex
		{
			get { return currentTransducerIndex; }
			set { currentTransducerIndex = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker; }
			set { t_fliker = value; }
		}

		[XmlAttribute]
		public short ConstraintType
		{
			get { return constraintType; }
			set { constraintType = value; }
		}

		[XmlElement]
		public EmXmlPQP[] ArchivePQP
		{
			get { return archivePQP; }
			set { archivePQP = value; }
		}

		[XmlElement]
		public EmXmlDNS[] ArchiveDNS
		{
			get { return archiveDNS; }
			set { archiveDNS = value; }
		}

		[XmlElement]
		public EmXmlAVG[] ArchiveAVG
		{
			get { return archiveAVG; }
			set { archiveAVG = value; }
		}

		#endregion
	}
}
