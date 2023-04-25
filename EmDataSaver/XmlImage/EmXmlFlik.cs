using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EmDataSaver.XmlImage
{
	[Serializable]
	public class EmXmlFlik : EmXmlArchivePart
	{
		#region Fields

		//private int standardSettingsType;
		private int unfPagesLength;

		#endregion

		#region Properties

		/*[XmlAttribute]
		public int StandardSettingsType
		{
			get { return standardSettingsType; }
			set { standardSettingsType = value; }
		}*/
		
		[XmlAttribute]
		public int UnfPagesLength
		{
			get { return unfPagesLength; }
			set { unfPagesLength = value; }
		}

		#endregion
	}
}
