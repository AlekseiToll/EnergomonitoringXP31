using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EmServiceLib;

namespace EmDataSaver.SqlImage
{
	public class EmSqlDeviceImage
	{
		#region Fields

		private long serialNumber_;
		private ushort internalType_;
		private EmDeviceType devType_;
		private string name_;
		private string version_;
		private string sql_;
		private short t_fliker_ = -1;

		private EmSqlArchive[] arhives_;

		#endregion

		#region Properties

		[XmlAttribute]
		public long SerialNumber
		{
			get { return serialNumber_; }
			set { serialNumber_ = value; }
		}

		[XmlAttribute]
		public string Name
		{
			get { return name_; }
			set { name_ = value; }
		}

		[XmlAttribute]
		public string Version
		{
			get { return version_; }
			set { version_ = value; }
		}

		[XmlAttribute]
		public ushort InternalType
		{
			get { return internalType_; }
			set { internalType_ = value; }
		}

		[XmlAttribute]
		public EmDeviceType DeviceType
		{
			get { return devType_; }
			set { devType_ = value; }
		}

		[XmlElement]
		public string Sql
		{
			get { return sql_; }
			set { sql_ = value; }
		}

		[XmlArray]
		public EmSqlArchive[] Archives
		{
			get { return arhives_; }
			set { arhives_ = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		#endregion
	}

	[Serializable]
	public class EtPQPSqlDeviceImage
	{
		#region Fields

		private Int64 deviceId_;
		private long serialNumber_;
		private string name_;
		private string deviceInfo_;
		private string version_;
		private string sql_;
		private short t_fliker_ = 10;

		private EmSqlObject[] objects_;

		#endregion

		#region Properties

		[XmlAttribute]
		public Int64 DeviceId
		{
			get { return deviceId_; }
			set { deviceId_ = value; }
		}

		[XmlAttribute]
		public long SerialNumber
		{
			get { return serialNumber_; }
			set { serialNumber_ = value; }
		}

		[XmlElement]
		public string DeviceInfo
		{
			get { return deviceInfo_; }
			set { deviceInfo_ = value; }
		}

		[XmlElement]
		public string Name
		{
			get { return name_; }
			set { name_ = value; }
		}

		[XmlElement]
		public string Version
		{
			get { return version_; }
			set { version_ = value; }
		}

		[XmlElement]
		public string Sql
		{
			get { return sql_; }
			set { sql_ = value; }
		}

		[XmlArray]
		public EmSqlObject[] Objects
		{
			get { return objects_; }
			set { objects_ = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		#endregion
	}

	[Serializable]
	public class EtPQP_A_SqlDeviceImage
	{
		#region Fields

		private Int64 deviceId_;
		private long serialNumber_;
		private string name_;
		private string deviceInfo_;
		private string version_;
		private string sql_;
		private short t_fliker_ = 10;

		private EmSqlRegistration[] registrations_;

		#endregion

		#region Properties

		[XmlAttribute]
		public Int64 DeviceId
		{
			get { return deviceId_; }
			set { deviceId_ = value; }
		}

		[XmlAttribute]
		public long SerialNumber
		{
			get { return serialNumber_; }
			set { serialNumber_ = value; }
		}

		[XmlElement]
		public string DeviceInfo
		{
			get { return deviceInfo_; }
			set { deviceInfo_ = value; }
		}

		[XmlElement]
		public string Name
		{
			get { return name_; }
			set { name_ = value; }
		}

		[XmlElement]
		public string Version
		{
			get { return version_; }
			set { version_ = value; }
		}

		[XmlElement]
		public string Sql
		{
			get { return sql_; }
			set { sql_ = value; }
		}

		[XmlArray]
		public EmSqlRegistration[] Registrations
		{
			get { return registrations_; }
			set { registrations_ = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		#endregion
	}
}
