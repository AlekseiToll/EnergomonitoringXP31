using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EmDataSaver.SavingInterface
{
	public partial class frmNewFirmware : Form
	{
		//private string marsLink_ = "ftp://ftp_marsen_1:4kjfZH3N4YG@mars-energo.ru";
		private string marsLink_ = "http://www.mars-energo.ru/index.php?option=com_catalog&task=product&id=58&Itemid=10";

		public frmNewFirmware()
		{
			InitializeComponent();
		}

		private void frmNewFirmware_Load(object sender, EventArgs e)
		{
			linkFtp.Links.Clear();
			linkFtp.Links.Add(0, marsLink_.Length, marsLink_);
		}

		private void linkFtp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(e.Link.LinkData.ToString());
			System.Diagnostics.Process.Start(sInfo);
		}

		public bool ShowThisMessage
		{
			get { return !(chbShowMessage.Checked); }
		}
	}
}
