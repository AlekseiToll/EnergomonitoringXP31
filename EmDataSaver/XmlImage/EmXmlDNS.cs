using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EmDataSaver.XmlImage
{
	public class EmXmlDNS : EmXmlArchivePart
	{
		#region Fields

		private uint dnsNum;
		private uint dnsTimer;
		private byte[] currentDNSBuffer;

		#endregion

		#region Properties

		[XmlAttribute(AttributeName="NumberOfDnsRecords")]
		public uint DnsNum
		{
			get { return dnsNum; }
			set { dnsNum = value; }
		}

		[XmlAttribute(AttributeName = "NumberOfTimerTicks")]
		public uint DnsTimer
		{
			get { return dnsTimer; }
			set { dnsTimer = value; }
		}

		[XmlElement(DataType = "hexBinary")]
		public byte[] CurrentDNSBuffer
		{
			get { return currentDNSBuffer; }
			set { currentDNSBuffer = value; }
		}

		#endregion
	}
}
