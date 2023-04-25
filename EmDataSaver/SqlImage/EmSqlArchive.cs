using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EmServiceLib;
using DeviceIO.Constraints;
using DeviceIO;

namespace EmDataSaver.SqlImage
{
	public class EmSqlArchive
	{
		#region Fields

		private string objectName;
		private ConnectScheme connectionScheme;
		private float f_Nominal;
		private float u_NominalLinear;
		private float u_NominalPhase;
		private ushort currentTransducerIndex;
		private float u_Limit;
		private float i_Limit;
		private DateTime commonBegin;
		private DateTime commonEnd;

		private EmSqlDataNode[] data = null;

		private string sql;

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
		public string Sql
		{
			get { return sql; }
			set { sql = value; }
		}

		[XmlElement]
		public EmSqlDataNode[] Data
		{
			get { return data; }
			set { data = value; }
		}

		#endregion
	}

	public class EmSqlObject
	{
		#region Fields

		private Int64 objectId_;
		private Int64 globalObjectId_;
		private Int64 deviceId_;
		private string objectName_;
		private string objectInfo_;
		private string devVersion_;
		private ConnectScheme connectionScheme_;
		private ushort t_fliker_;
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
		private TimeSpan ml_start_time1_;
		private TimeSpan ml_end_time1_;
		private TimeSpan ml_start_time2_;
		private TimeSpan ml_end_time2_;

		private EmSqlDataNode[] dataPQP_ = null;
		private EmSqlDataNode[] dataAVG_ = null;
		private EmSqlDataNode[] dataDNS_ = null;

		private string sql_;

		#endregion

		#region Properties

		[XmlAttribute]
		public Int64 ObjectId
		{
			get { return objectId_; }
			set { objectId_ = value; }
		}

		[XmlAttribute]
		public Int64 GlobalObjectId
		{
			get { return globalObjectId_; }
			set { globalObjectId_ = value; }
		}

		[XmlAttribute]
		public Int64 DeviceId
		{
			get { return deviceId_; }
			set { deviceId_ = value; }
		}

		[XmlElement]
		public string ObjectName
		{
			get { return objectName_; }
			set { objectName_ = value; }
		}

		[XmlElement]
		public string ObjectInfo
		{
			get { return objectInfo_; }
			set { objectInfo_ = value; }
		}

		[XmlElement]
		public string DeviceVersion
		{
			get { return devVersion_; }
			set { devVersion_ = value; }
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
		public ushort T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
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
		public string Sql
		{
			get { return sql_; }
			set { sql_ = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime1
		{
			get { return ml_start_time1_; }
			set { ml_start_time1_ = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime1
		{
			get { return ml_end_time1_; }
			set { ml_end_time1_ = value; }
		}

		[XmlElement]
		public TimeSpan MlStartTime2
		{
			get { return ml_start_time2_; }
			set { ml_start_time2_ = value; }
		}

		[XmlElement]
		public TimeSpan MlEndTime2
		{
			get { return ml_end_time2_; }
			set { ml_end_time2_ = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataPQP
		{
			get { return dataPQP_; }
			set { dataPQP_ = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataAVG
		{
			get { return dataAVG_; }
			set { dataAVG_ = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataDNS
		{
			get { return dataDNS_; }
			set { dataDNS_ = value; }
		}

		#endregion
	}

	public class EmSqlRegistration
	{
		#region Fields

		private Int64 reg_Id_;			// unique for DB
		private Int64 registrationId_;	// unique for the device
		private Int64 deviceId_;
		private string objectName_;
		private string objectInfo_;
		private string devVersion_;
		private DateTime devVersionDate_;
		private ConnectScheme connectionScheme_;
		private short constraintType_;

		private SystemInfoEtPQP_A sysInfo_;

		private DateTime commonBegin_;
		private DateTime commonEnd_;

		private EmSqlDataNode[] dataPQP_ = null;
		private EmSqlDataNode[] dataAVG_ = null;
		private EmSqlDataNode[] dataDNS_ = null;

		private string sql_;

        private EtPQP_A_ConstraintsDetailed constraints_; //= new EtPQP_A_ConstraintsDetailed();

		#endregion

		#region Properties

		[XmlElement]
		public SystemInfoEtPQP_A SysInfo
		{
			get { return sysInfo_; }
			set { sysInfo_ = value; }
		}

        [XmlElement]
        public EtPQP_A_ConstraintsDetailed Constraints
        {
            get { return constraints_; }
            set { constraints_ = value; }
        }

		[XmlAttribute]
		public Int64 RegId
		{
			get { return reg_Id_; }
			set { reg_Id_ = value; }
		}

		[XmlAttribute]
		public Int64 RegistrationId
		{
			get { return registrationId_; }
			set { registrationId_ = value; }
		}

		[XmlAttribute]
		public Int64 DeviceId
		{
			get { return deviceId_; }
			set { deviceId_ = value; }
		}

		[XmlElement]
		public string ObjectName
		{
			get { return objectName_; }
			set { objectName_ = value; }
		}

		[XmlElement]
		public string ObjectInfo
		{
			get { return objectInfo_; }
			set { objectInfo_ = value; }
		}

		[XmlElement]
		public string DeviceVersion
		{
			get { return devVersion_; }
			set { devVersion_ = value; }
		}

		public DateTime DevVersionDate
		{
			get { return devVersionDate_; }
			set { devVersionDate_ = value; }
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

		[XmlElement]
		public string Sql
		{
			get { return sql_; }
			set { sql_ = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataPQP
		{
			get { return dataPQP_; }
			set { dataPQP_ = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataAVG
		{
			get { return dataAVG_; }
			set { dataAVG_ = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataDNS
		{
			get { return dataDNS_; }
			set { dataDNS_ = value; }
		}

		#endregion
	}

	public class EmSqlEm32Device
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

		private EmSqlDataNode[] dataPQP = null;
		private EmSqlDataNode[] dataAVG = null;
		private EmSqlDataNode[] dataDNO = null;

		private string sql;

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
		public string Sql
		{
			get { return sql; }
			set { sql = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataPQP
		{
			get { return dataPQP; }
			set { dataPQP = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataAVG
		{
			get { return dataAVG; }
			set { dataAVG = value; }
		}

		[XmlArray]
		public EmSqlDataNode[] DataDNO
		{
			get { return dataDNO; }
			set { dataDNO = value; }
		}

		#endregion
	}
}
