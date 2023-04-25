using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace EmServiceLib.SavingInterface
{
	public partial class frmSentLogs : Form
	{
		public frmSentLogs()
		{
			InitializeComponent();
		}

		private void btnSent_Click(object sender, EventArgs e)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmService.emstrings", System.Reflection.Assembly.GetExecutingAssembly());
				if (tbSender.Text.Length < 1)
				{
					string msg = rm.GetString("str_no_sender");
					string cap = rm.GetString("unfortunately_caption");
					MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (tbEmail.Text.Length < 1)
				{
					string msg = rm.GetString("str_no_email");
					string cap = rm.GetString("unfortunately_caption");
					MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (!tbEmail.Text.Contains('@'))
				{
					string msg = rm.GetString("str_invalid_email");
					string cap = rm.GetString("unfortunately_caption");
					MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// sending message////////////////////////////
				// message info
				MailMessage mail_msg = new MailMessage();
				mail_msg.From = new MailAddress("mars.energo.service@gmail.com");
				mail_msg.To.Add(new MailAddress("support@mars-energo.ru"));
				mail_msg.CC.Add(new MailAddress("alextr437@mail.ru"));
				mail_msg.CC.Add(new MailAddress("alextr438@gmail.com"));
				//mail_msg.To.Add(new MailAddress("alextr437@mail.ru"));
				//mail_msg.CC.Add(new MailAddress("alextr437@yandex.ru"));
				mail_msg.Subject = "Logs from client";
				mail_msg.Body = "Letter from address: " + tbEmail.Text + "   Sender: " + tbSender.Text + '\n';
				if (tbComment.Text.Length > 0) mail_msg.Body += tbComment.Text;

				// find logs
				string[] fileNames = Directory.GetFiles(EmService.AppDirectory, "*.txt");
				if (fileNames.Length < 1)
				{
					mail_msg.Body += "No logs were found!\n";
					mail_msg.Body += Directory.GetFiles(EmService.AppDirectory);
				}
				else
				{
					// add logs to message
					for (int iFile = 0; iFile < fileNames.Length; iFile++)
					{
						mail_msg.Attachments.Add(new Attachment(fileNames[iFile]));
					}
				}

				SmtpClient smtp = new SmtpClient();
				smtp.Host = "smtp.gmail.com";
				smtp.Port = 587;
				smtp.EnableSsl = true;
				smtp.Credentials = new NetworkCredential(/*tbEmail.Text.Split('@')[0]*/"mars.energo.service", "postgres" /*password*/);
				smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtp.Timeout = 30000;
				//smtp.UseDefaultCredentials = true;
				smtp.Send(mail_msg);
				mail_msg.Dispose();

				string strMsg = rm.GetString("msg_logs_were_sent");
				string strCap = rm.GetString("ok_caption");
				MessageBox.Show(strMsg, strCap, MessageBoxButtons.OK, MessageBoxIcon.Information);

				this.Close();
			}
			catch (SmtpException ex)
			{
				//! Если будут ошибки при отправке, сделать перезапуск IIS

				EmService.DumpException(ex, "Exception in frmSentLogs::btnSent_Click():");

				ResourceManager rm = new ResourceManager("EmService.emstrings", System.Reflection.Assembly.GetExecutingAssembly());
				string strMsg = rm.GetString("msg_error_send_logs");
				string strCap = rm.GetString("unfortunately_caption");
				MessageBox.Show(strMsg, strCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in frmSentLogs::btnSent_Click():");

				ResourceManager rm = new ResourceManager("EmService.emstrings", System.Reflection.Assembly.GetExecutingAssembly());
				string strMsg = rm.GetString("msg_error2_send_logs");
				string strCap = rm.GetString("unfortunately_caption");
				MessageBox.Show(strMsg, strCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
