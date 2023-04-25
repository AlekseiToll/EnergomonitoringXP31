using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Windows.Forms;

using EmServiceLib;

namespace ExportToExcel
{
    class MessageBoxes
    {
        /// <summary>Error openening report template</summary>
        public static void PqpReportTmplateError(object sender, string fname)
        {
            ResourceManager rm = new ResourceManager("ExportToExcel.emstrings", sender.GetType().Assembly);
            string msg = string.Format(rm.GetString("msg_pqp_report_template_error_text"), fname);
            string cap = rm.GetString("unfortunately_caption");
            MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void PqpReportWriteError(object sender, string fname)
        {
            ResourceManager rm = new ResourceManager("ExportToExcel.emstrings", sender.GetType().Assembly);
            string msg = string.Format(rm.GetString("msg_pqp_report_write_error_text"), fname);
            string cap = rm.GetString("unfortunately_caption");
            MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void PqpReportSavedSuccess(object sender, string fname)
        {
            ResourceManager rm = new ResourceManager("ExportToExcel.emstrings", sender.GetType().Assembly);
            string msg = string.Format(rm.GetString("msg_pqp_report_success_text"), fname);
            string cap = rm.GetString("ok_caption");
            MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
