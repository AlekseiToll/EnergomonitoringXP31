using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EmServiceLib;
using DeviceIO;

namespace EmDataSaver.XmlImage
{
	[Serializable]
	public class EmXmlDeviceImage
	{
		#region Fields

		private long serialNumber_;
		private ushort internalType_;
		private EmDeviceType devType_;
		private string name_;
		private string version_;

		private EmXmlArchive[] archiveList_ = null;

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

		[XmlArray]
		public EmXmlArchive[] ArchiveList
		{
			get { return archiveList_; }
			set { archiveList_ = value; }
		}

		#endregion
	}

	[Serializable]
	public class EtPQPXmlDeviceImage
	{
		#region Fields

		private long serialNumber_;
		private string name_;
		private string version_;

		private EtPQPXmlArchive[] archiveList_ = null;

		private string SD_CurrentRangeName_1_;
		private string SD_CurrentRangeName_2_;
		private string SD_CurrentRangeName_3_;
		private string SD_CurrentRangeName_4_;
		private string SD_CurrentRangeName_5_;
		private string SD_CurrentRangeName_6_;

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
		public string SD_CurrentRangeName_1
		{
			get { return SD_CurrentRangeName_1_; }
			set { SD_CurrentRangeName_1_ = value; }
		}

		[XmlAttribute]
		public string SD_CurrentRangeName_2
		{
			get { return SD_CurrentRangeName_2_; }
			set { SD_CurrentRangeName_2_ = value; }
		}

		[XmlAttribute]
		public string SD_CurrentRangeName_3
		{
			get { return SD_CurrentRangeName_3_; }
			set { SD_CurrentRangeName_3_ = value; }
		}

		[XmlAttribute]
		public string SD_CurrentRangeName_4
		{
			get { return SD_CurrentRangeName_4_; }
			set { SD_CurrentRangeName_4_ = value; }
		}

		[XmlAttribute]
		public string SD_CurrentRangeName_5
		{
			get { return SD_CurrentRangeName_5_; }
			set { SD_CurrentRangeName_5_ = value; }
		}

		[XmlAttribute]
		public string SD_CurrentRangeName_6
		{
			get { return SD_CurrentRangeName_6_; }
			set { SD_CurrentRangeName_6_ = value; }
		}

		[XmlAttribute]
		public string Version
		{
			get { return version_; }
			set { version_ = value; }
		}

		[XmlArray]
		public EtPQPXmlArchive[] ArchiveList
		{
			get { return archiveList_; }
			set { archiveList_ = value; }
		}

		#endregion
	}

	[Serializable]
	public class EtPQP_A_XmlDeviceImage
	{
		#region Fields

		private long serialNumber_;
		private string name_;
		private string version_;

		private EtPQP_A_XmlArchive[] archiveList_ = null;

		//private string SD_CurrentRangeName_1_;
		//private string SD_CurrentRangeName_2_;
		//private string SD_CurrentRangeName_3_;
		//private string SD_CurrentRangeName_4_;
		//private string SD_CurrentRangeName_5_;
		//private string SD_CurrentRangeName_6_;

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

		//[XmlAttribute]
		//public string SD_CurrentRangeName_1
		//{
		//    get { return SD_CurrentRangeName_1_; }
		//    set { SD_CurrentRangeName_1_ = value; }
		//}

		//[XmlAttribute]
		//public string SD_CurrentRangeName_2
		//{
		//    get { return SD_CurrentRangeName_2_; }
		//    set { SD_CurrentRangeName_2_ = value; }
		//}

		//[XmlAttribute]
		//public string SD_CurrentRangeName_3
		//{
		//    get { return SD_CurrentRangeName_3_; }
		//    set { SD_CurrentRangeName_3_ = value; }
		//}

		//[XmlAttribute]
		//public string SD_CurrentRangeName_4
		//{
		//    get { return SD_CurrentRangeName_4_; }
		//    set { SD_CurrentRangeName_4_ = value; }
		//}

		//[XmlAttribute]
		//public string SD_CurrentRangeName_5
		//{
		//    get { return SD_CurrentRangeName_5_; }
		//    set { SD_CurrentRangeName_5_ = value; }
		//}

		//[XmlAttribute]
		//public string SD_CurrentRangeName_6
		//{
		//    get { return SD_CurrentRangeName_6_; }
		//    set { SD_CurrentRangeName_6_ = value; }
		//}

		[XmlAttribute]
		public string Version
		{
			get { return version_; }
			set { version_ = value; }
		}

		[XmlArray]
		public EtPQP_A_XmlArchive[] ArchiveList
		{
			get { return archiveList_; }
			set { archiveList_ = value; }
		}

		#endregion
	}
}
