using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DataGridColumnStyles
{
	public class DataGridColumnAnyColor : DataGridTextBoxColumn, IAnyColoredColumnStyle
	{
		public DataGridColumnAnyColor() { }

		#region IAnyColoredColumnStyle Members

		public event DataGridEventHandler CellPaint;

		#endregion

		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			DataGridEventArgs e = null;
			try
			{				
				int col = DataGridTableStyle.GridColumnStyles.IndexOf(this);
				e = new DataGridEventArgs(col, rowNum, this.GetColumnValueAtRow(source, rowNum));
				e.BackBrush = backBrush;
				e.ForeBrush = foreBrush;
				OnCellPaint(e);
			}
			finally
			{
				base.Paint(g, bounds, source, rowNum, e.BackBrush, e.ForeBrush, alignToRight);
			}
		}

		protected virtual void OnCellPaint(DataGridEventArgs e)
		{
			if (CellPaint != null)
			{
				CellPaint(this, e);
			}
		}
	}
}
