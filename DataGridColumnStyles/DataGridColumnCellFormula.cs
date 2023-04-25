using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace DataGridColumnStyles
{
	public class DataGridColumnCellFormula : DataGridColumnGroupCaption, ICellFormulaColumnStyle
	{
		#region ICellFormulaColumnStyle Members

		private Font _cellRegularFont = new Font(FontFamily.GenericSansSerif, (float)7.75);
		private Font _cellSubscriptFont = new Font(FontFamily.GenericSansSerif, 7);

		public System.Drawing.Font CellRegularFont
		{
			get
			{
				return _cellRegularFont;
			}
			set
			{
				_cellRegularFont = value;
			}
		}

		public System.Drawing.Font CellSubscriptFont
		{
			get
			{
				return _cellSubscriptFont;
			}
			set
			{
				_cellSubscriptFont = value;
			}
		}

		#endregion

		public DataGridColumnCellFormula() { }
		public DataGridColumnCellFormula(DataGridGroupCaption Caption, int Index) 
		{
			this.GroupCaption = Caption;
			this.GroupIndex = Index;
		}

		

		protected override void Paint(Graphics g, Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			
			try
			{				
				base.Paint (g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);

				/// drawing cell
				g.FillRectangle(backBrush, bounds);

				string CellFormula = this.GetColumnValueAtRow(source, rowNum).ToString();


				if (CellFormula.Trim().Equals("∆f")) CellFormula = "∆F";
				if (CellFormula.Trim().Equals("K_U") && Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("iv") ) CellFormula = "THD_U";
				if (CellFormula.Trim().Equals("δU_y") && Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("iv")) CellFormula = "δU_s";
				if (CellFormula.Trim().Equals("δU_y_'") && Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("iv")) CellFormula = "δU_s_'";
				if (CellFormula.Trim().Equals("δU_y_\"") && Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("iv")) CellFormula = "δU_s_\"";

				
				
				if (CellFormula != null)
				{
					char[] chSplitter = new char[1];
					chSplitter[0] = '_';
					string[] strArr = CellFormula.Split(chSplitter);

					Point p = new Point(bounds.Left, bounds.Top + 2);

					for (int i = 0; i < strArr.Length; i++)
					{
						g.DrawString(strArr[i], _cellRegularFont, foreBrush, p);
						SizeF size = new SizeF(g.MeasureString(strArr[i], _cellRegularFont));

						p.X += (int)size.Width - 2;

						if (strArr.Length > ++i)
						{
							g.DrawString(strArr[i], _cellSubscriptFont, foreBrush, p.X, p.Y + size.Height - 10);
							size = new SizeF(g.MeasureString(strArr[i], _cellSubscriptFont));
							p.X += (int)size.Width - 4;
						}
					}
				}

			}
			catch { }
		}
	}
}
