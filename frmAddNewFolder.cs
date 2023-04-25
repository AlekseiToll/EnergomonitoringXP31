using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Describes dialog window to enter the name of creating folder
	/// </summary>
	public partial class frmAddNewFolder : Form
	{
		/// <summary>
		/// Name of the folder we are want to create
		/// </summary>
		public string FolderName = String.Empty;
		public string FolderInfo = String.Empty;


		/// <summary>
		/// Default constructor
		/// </summary>
		public frmAddNewFolder()
		{
			InitializeComponent();

			FolderName = txtName.Text;
			FolderInfo = txtInfo.Text;
		}

		private void txtFolderName_TextChanged(object sender, EventArgs e)
		{
			FolderName = txtName.Text;
			btnOK.Enabled = txtName.Text.Length > 0 ? true : false;
		}

		private void txtInfo_TextChanged(object sender, EventArgs e)
		{
			FolderInfo = txtInfo.Text;
		}
	}
}