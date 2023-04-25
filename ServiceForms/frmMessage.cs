using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Resources;

namespace EnergomonitoringXP
{
	// по логике программы удобнее, чтобы этот диалог появлялся один раз, поэтому
	// здесь используется паттерн singleton
	public partial class frmMessage : System.Windows.Forms.Form
	{
		private static frmMessage instance_;

		private frmMessage(string mess, float fontSize)
		{
			InitializeComponent();
			if (mess.Length <= 40)
			{
				tbMessage.Visible = false;
				lbMessage.Visible = true;
				//tbMessage.Location = new Point(82, tbMessage.Location.Y);
				//lbMessage.Size = new Size(280, 24);
				lbMessage.Text = mess;
				lbMessage.Font = new Font(this.Font.FontFamily, fontSize);
			}
			else
			{
				tbMessage.Visible = true;
				lbMessage.Visible = false;
				//tbMessage.Location = new Point(5, tbMessage.Location.Y);
				//tbMessage.Size = new Size(427, 52);
				tbMessage.Text = mess;
				tbMessage.Font = new Font(this.Font.FontFamily, fontSize);
			}
		}

		public static frmMessage Instance(string mess, bool okBtn, float fontSize)
		{
			if (instance_ == null)
			{
				instance_ = new frmMessage(mess, fontSize);
				if (!okBtn) instance_.btnOk.Visible = false;
			}
			return instance_;
		}

		public void DeleteInstance()
		{
			instance_ = null;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
			instance_ = null;
		}

		private void frmMessage_FormClosing(object sender, FormClosingEventArgs e)
		{
			instance_ = null;
		}
	}
}