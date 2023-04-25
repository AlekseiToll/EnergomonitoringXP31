using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EmDataSaver.SavingInterface
{
	public partial class frmEditTimeInterval : Form
	{
		public frmEditTimeInterval()
		{
			InitializeComponent();
		}

		#region Properties

		public DateTime DateStart
		{
			get { return dtpStart.Value; }
			set { dtpStart.Value = value; }
		}

		public DateTime DateEnd
		{
			get { return dtpEnd.Value; }
			set { dtpEnd.Value = value; }
		}

		public DateTime MinDateStart
		{
			set { dtpStart.MinDate = value; }
		}

		public DateTime MaxDateEnd
		{
			set { dtpEnd.MaxDate = value; }
		}

		#endregion
	}
}