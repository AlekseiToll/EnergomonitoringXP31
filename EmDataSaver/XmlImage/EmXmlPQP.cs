using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EmDataSaver.XmlImage
{
	[Serializable]
	public class EmXmlPQP : EmXmlArchivePart
	{
		#region Fields

		private int standardSettingsType;
		private int pqpZone;
		private int unfPagesLength;
		// fliker
		private short t_fliker = 10;

		#endregion

		#region Properties

		[XmlAttribute]
		public int PqpZone
		{
			get { return pqpZone; }
			set { pqpZone = value; }
		}

		[XmlAttribute]
		public int StandardSettingsType
		{
			get { return standardSettingsType; }
			set { standardSettingsType = value; }
		}
		
		[XmlAttribute]
		public int UnfPagesLength
		{
			get { return unfPagesLength; }
			set { unfPagesLength = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker; }
			set { t_fliker = value; }
		}

		#endregion
	}

	[Serializable]
	public class EmXmlPQP_PQP_A : EmXmlArchivePart
	{
		#region Fields

		private int standardSettingsType;
		// fliker
		private short t_fliker = 10;

		#endregion

		#region Properties

		[XmlAttribute]
		public int StandardSettingsType
		{
			get { return standardSettingsType; }
			set { standardSettingsType = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker; }
			set { t_fliker = value; }
		}

		#endregion
	}
}
