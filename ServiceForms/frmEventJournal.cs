using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
using System.Globalization;

using EmServiceLib;
using DbServiceLib;
using DeviceIO;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Form to show event journal
	/// </summary>
	public partial class frmEventJournal : Form
	{
		string connectionStringEm32_;
		Int64 devId_;

		/// <summary>Constructor</summary>
		public frmEventJournal(string connectionStringEm32, Int64 devId)
		{
			connectionStringEm32_ = connectionStringEm32;
			devId_ = devId;
			InitializeComponent();
		}

		private void frmEventJournal_Load(object sender, EventArgs e)
		{
			//this.OnProgress += new EnableProgressHandler(frmEventJournal_OnProgress);

			List<EmDataSaver.JournalEntry> listEvents = new List<EmDataSaver.JournalEntry>();
			DbService dbService = new DbService(connectionStringEm32_);

			try
			{
				dbService.Open();
				string commandText = 
					String.Format("SELECT * FROM event_journal WHERE device_id = {0};", devId_);
				dbService.ExecuteReader(commandText);

				while (dbService.DataReaderRead())
				{
					Int64 entryNum = (Int64)dbService.DataReaderData("event_number");
					DateTime dt = (DateTime)dbService.DataReaderData("event_date");
					short type = (short)dbService.DataReaderData("event_type");
					listEvents.Add(new EmDataSaver.JournalEntry(entryNum, dt, type));
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmEventJournal_Load() 1: ");
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}

			try
			{
				for (int i = 0; i < listEvents.Count; ++i)
				{
					ListViewItem item =
						new ListViewItem(listEvents[i].Date.ToString(new CultureInfo("ru-RU")), 0);
					item.SubItems.Add(listEvents[i].GetEventText());
					lvEvents.Items.Add(item);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmEventJournal_Load() 2: ");
			}
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}