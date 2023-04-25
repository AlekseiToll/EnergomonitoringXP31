//-------------------------------------------------------------------------
// CalendarCell.cs, from the MSDN topic 
//              "How to: Host Controls in Windows Forms DataGridView Cells
//-------------------------------------------------------------------------
using System;
using System.Windows.Forms;

namespace EnergomonitoringXP
{

	public class CalendarCell : DataGridViewTextBoxCell
	{
		CalendarEditingControl ctl_ = null;
		DateTime oldValue_;

		public CalendarCell() : base()
		{
			// Use the short date format.
			//this.Style.Format = "d";

			this.Style.Format = "HH:mm";

			//this.Value = DateTime.Now;
		}

		public override void InitializeEditingControl(int rowIndex, object
			initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			// Set the value of the editing control to the current cell value.
			base.InitializeEditingControl(rowIndex, initialFormattedValue,
				dataGridViewCellStyle);

			ctl_ = DataGridView.EditingControl as CalendarEditingControl;
			ctl_.Value = (DateTime)this.Value;

			ctl_.ValueChanged += new System.EventHandler(this.ctl_ValueChanged);
			oldValue_ = ctl_.Value;
		}

		private void ctl_ValueChanged(object sender, EventArgs e)
		{
			TimeSpan diff = new TimeSpan(0, 1, 0);

			if ((ctl_.Value.Minute - oldValue_.Minute) == 1)
			{
				ctl_.Value = ctl_.Value.AddMinutes(9);
			}
			if ((oldValue_.Minute - ctl_.Value.Minute) == 1)
			{
				ctl_.Value = ctl_.Value.AddMinutes(-9);
			}
			if ((ctl_.Value.Minute - oldValue_.Minute) == 59)
			{
				ctl_.Value = ctl_.Value.AddMinutes(-9);
				ctl_.Value = ctl_.Value.AddHours(-1);
			}
			if ((oldValue_.Minute - ctl_.Value.Minute) == 59)
			{
				ctl_.Value = ctl_.Value.AddMinutes(9);
				ctl_.Value = ctl_.Value.AddHours(1);
			}

			TimeSpan diff2 = new TimeSpan(0, 59, 0);
			if ((ctl_.Value - oldValue_) == diff)
			{
				ctl_.Value = ctl_.Value.AddMinutes(9);
			}
			if ((oldValue_ - ctl_.Value) == diff)
			{
				ctl_.Value = ctl_.Value.AddMinutes(-9);
			}
			if ((ctl_.Value - oldValue_) == diff2)
			{
				ctl_.Value = ctl_.Value.AddMinutes(9);
			}
			if ((oldValue_ - ctl_.Value) == diff2)
			{
				ctl_.Value = ctl_.Value.AddMinutes(-9);
			}

			oldValue_ = ctl_.Value;
		}

		#region Properties

		public override Type EditType
		{
			get
			{
				// Return the type of the editing contol that CalendarCell uses.
				return typeof(CalendarEditingControl);
			}
		}

		public override Type ValueType
		{
			get
			{
				// Return the type of the value that CalendarCell contains.
				return typeof(DateTime);
			}
		}

		public override object DefaultNewRowValue
		{
			get
			{
				// Use the current date and time as the default value.
				return DateTime.Now;
			}
		}

		//public override DateTime CellValue
		//{
		//    get
		//    {
		//        return (DateTime)this.Value;
		//    }
		//}

		#endregion
	}
}
