using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;

namespace EnergomonitoringXP
{
	public partial class frmPgServers : Form
	{
		public int PgServerIndex
		{
			get
			{
				if (lvServers.CheckedItems == null) return -1;
				return lvServers.CheckedItems[0].Index;
			}
		}
		public frmPgServers()
		{
			InitializeComponent();
		}

		private void frmPgServers_Load(object sender, EventArgs e)
		{

		}

		private void lvServers_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			//lvServers.CheckedItems.IndexOf(e.Item);
			foreach(ListViewItem lvi in lvServers.Items)
			{
				if (e.Item != lvi) lvi.Checked = false;
			}
			e.Item.Checked = true;
			if (!btnOK.Enabled) btnOK.Enabled = true;
		}
	}
}