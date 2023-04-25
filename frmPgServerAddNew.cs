using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;

using DbServiceLib;
using EnergomonitoringXP.ArchiveTreeView;
using EmServiceLib;

namespace EnergomonitoringXP
{
	public partial class frmPgServerAddNew : Form
	{
		int _Port = -1;

		#region Properties

		/// <summary>
		/// Gets or sets PostgreSQL Server Name
		/// </summary>
		public string ServerName
		{
			get { return txtName.Text; }
			set { txtName.Text = value; }
		}

		/// <summary>
		/// Gets or sets PostgreSQL Server Host
		/// </summary>
		public string Host
		{
			get { return txtHost.Text; }
			set { txtHost.Text = value; }
		}

		/// <summary>
		/// Gets or sets PostgreSQL Server Port
		/// </summary>
		public int Port
		{
			get { return _Port; }
			set { _Port = value; txtPort.Text = value.ToString(); }
		}

		#endregion

		public frmPgServerAddNew()
		{
			InitializeComponent();
		}

		public frmPgServerAddNew(bool bOptions)
		{
			InitializeComponent();

			ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);

			string msg = string.Empty;
			if (bOptions) this.Text = rm.GetString("name_frmPgServerAddNew_header_options");
			else this.Text = rm.GetString("name_frmPgServerAddNew_header_add");
		}

		private void btnTestConnection_Click(object sender, EventArgs e)
		{
			// здесь для проверки связи с сервером достаточно сторки соединения с БД em_db, т.к. она
			// в любом случае точно существует
			string connectStr = String.Format("SERVER={0};Port={1};DATABASE=em_db;USER ID=energomonitor;PASSWORD=4emworknet4;Encoding=UNICODE", txtHost.Text, txtPort.Text);
			DbService dbService = new DbService(connectStr);
			try
			{
				dbService.Open();
				MessageBoxes.DbConnectionOk(this);
			}
			catch
			{
				MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
			}
			finally
			{
				if (dbService != null) dbService.CloseConnect();
			}
		}

		private void txtAny_Changed(object sender, EventArgs e)
		{
			btnOK.Enabled =
				txtName.Text != string.Empty &
				txtHost.Text != string.Empty &
				txtPort.Text != string.Empty;

			btnTestConnection.Enabled =
				txtHost.Text != string.Empty &
				txtPort.Text != string.Empty &
				Int32.TryParse(txtPort.Text, out _Port);
		}
	}
}