using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EmDataSaver.XmlImage
{
//	public enum ArchivePartType { PQP, AVG, DNS }

	[Serializable]
	public class EmXmlArchivePart
	{
		#region Fields

		private DateTime start_;
		private DateTime end_;

		private byte[] dataPages_ = null;
		private byte[] dataPagesExtra_ = null;

		#endregion

		#region Properties

		[XmlAttribute]
		public DateTime Start
		{
			get { return start_; }
			set { start_ = value; }
		}

		[XmlAttribute]
		public DateTime End
		{
			get { return end_; }
			set { end_ = value; }
		}

		[XmlElement(DataType="hexBinary")]
		public byte[] DataPages
		{
			get { return dataPages_; }
			set { dataPages_ = value; }
		}

		[XmlElement(DataType = "hexBinary")]
		public byte[] DataPagesExtra
		{
			get { return dataPagesExtra_; }
			set { dataPagesExtra_ = value; }
		}

		#endregion
	}
}
