using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.ComponentModel;

using DeviceIO;
using EmDataSaver.SqlImage;

namespace EmDataSaver
{
	public abstract class EmDataSaverBase
	{
		#region Fields

		protected object sender_;
		protected Settings settings_;
		protected string pgSrvConnectStr_;
		protected string pgHost_;

		protected DoWorkEventArgs e_;

		#endregion
	}

	/// <summary>Journal entry</summary>
	public class JournalEntry
	{
		public Int64 EntryNumber;	// номер события читается из прибора
		public DateTime Date;
		public short EventType;
		//public Int64 ExtraData;

		public JournalEntry(Int64 num, DateTime dt, short type)
		{
			EntryNumber = num;
			Date = dt;
			EventType = type;
			//ExtraData = extra;
		}

		public string GetEventText()
		{
			string strEvent = "";

			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings",
				this.GetType().Assembly);
			switch (EventType)
			{
				case 0:
					strEvent = rm.GetString("event_journal_0_text");
					break;
				case 1:
					strEvent = rm.GetString("event_journal_1_text");
					break;
				case 2:
					strEvent = rm.GetString("event_journal_2_text");
					break;
				case 3:
					strEvent = rm.GetString("event_journal_3_text");
					break;
				case 4:
					strEvent = rm.GetString("event_journal_4_text");
					break;
				default: strEvent = string.Format("Unknown event type {0}", EventType);
					break;
			}

			return strEvent;
		}
	}
}
