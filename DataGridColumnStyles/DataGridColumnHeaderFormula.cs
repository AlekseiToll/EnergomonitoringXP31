using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DataGridColumnStyles
{
	public class DataGridColumnHeaderFormula : DataGridColumnOneColor, IHeaderFormulaColumnStyle
	{		
		private Font _headerRegularFont = new Font(FontFamily.GenericSansSerif, (float)7.75);
		private Font _headerSubscriptFont = new Font(FontFamily.GenericSansSerif, 7);
		private string _headerFormula = string.Empty;
		private string[] _strArrColumnHeaderFormula = null;

		#region IHeaderFormulaColumnStyle Members

		public string HeaderFormula
		{
			get
			{
				return _headerFormula;
			}
			set
			{
				_headerFormula = value;
				_strArrColumnHeaderFormula = value.Split(new char[1] { '_' });
			}
		}

		public System.Drawing.Font HeaderRegularFont
		{
			get
			{
				return _headerRegularFont;
			}
			set
			{
				_headerRegularFont = value;
			}
		}

		public System.Drawing.Font HeaderSubscriptFont
		{
			get
			{
				return _headerSubscriptFont;
			}
			set
			{
				_headerSubscriptFont = value;
			}
		}

		public bool HeaderIsFormula
		{
			get
			{
				return _strArrColumnHeaderFormula != null;
			}
		}

		#endregion

		public DataGridColumnHeaderFormula(){}

		public DataGridColumnHeaderFormula(Color BgColor)
		{
			this.BackgroungColor = BgColor;
		}
		
		public DataGridColumnHeaderFormula(Color BgColor, string HeaderFormula)
		{
			this.BackgroungColor = BgColor;
			this.HeaderFormula = HeaderFormula;
		}

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			try
			{
				// check for one per column drawing
				// trying to hit left point...
				int FirstVisibleRowNum = this.DataGridTableStyle.DataGrid.HitTest(bounds.Left, 3 * bounds.Height).Row;
				// if left point not in visible rectangle... trying to hit right point
				if (FirstVisibleRowNum == -1)
					FirstVisibleRowNum = this.DataGridTableStyle.DataGrid.HitTest(bounds.Left + bounds.Width, 3 * bounds.Height).Row;

				// drawing column header
				if (_strArrColumnHeaderFormula != null && rowNum == FirstVisibleRowNum)
				{
					Point pointDrawingZero = new Point(bounds.Left + 3, bounds.Height + 7);

					for (int i = 0; i < _strArrColumnHeaderFormula.Length; i++)
					{
						SizeF size = new SizeF(g.MeasureString(_strArrColumnHeaderFormula[i], _headerRegularFont));
						// check for the string width
						if (pointDrawingZero.X + size.Width > bounds.Right)
							break;

						g.DrawString(_strArrColumnHeaderFormula[i], _headerRegularFont, foreBrush, pointDrawingZero);
						pointDrawingZero.X += (int)size.Width - 2;

						if (_strArrColumnHeaderFormula.Length > ++i)
						{
							size = new SizeF(g.MeasureString(_strArrColumnHeaderFormula[i], _headerSubscriptFont));
							// check for the string width
							if (pointDrawingZero.X + size.Width > bounds.Right)
								break;
							g.DrawString(_strArrColumnHeaderFormula[i], _headerSubscriptFont, foreBrush, pointDrawingZero.X, pointDrawingZero.Y + size.Height - 8);
							pointDrawingZero.X += (int)size.Width - 2;
						}
					}
				}
			}
			finally
			{
				base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
			}
		}

	}
}
