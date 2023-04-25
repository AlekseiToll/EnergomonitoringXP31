using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EnergomonitoringXP
{
    public partial class frmAboutBox : Form
    {
        public frmAboutBox()
        {
            InitializeComponent();

			labelVersion.Text = string.Format(labelVersion.Text, this.ProductVersion);
        }
    }
}