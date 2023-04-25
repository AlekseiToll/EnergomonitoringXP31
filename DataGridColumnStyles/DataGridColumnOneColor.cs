using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DataGridColumnStyles
{
	public class DataGridColumnOneColor : DataGridColumnAnyColor, IColoredColumnStyle
	{
		private Color _backgroungColor = Color.White;
		private Color _foregroundColor = Color.Black;		

		#region IColoredColumnStyle Members

		public Color BackgroungColor
		{
			get
			{
				return _backgroungColor;
			}
			set
			{
				_backgroungColor = value;
			}
		}

		public Color ForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				value = _foregroundColor;
			}
		}

		#endregion

		public DataGridColumnOneColor() { }
		public DataGridColumnOneColor(Color BgColor)
		{
			_backgroungColor = BgColor;
		}

		protected override void  Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			try
			{
				if (_backgroungColor.IsEmpty == false)
				{
					backBrush = new SolidBrush(_backgroungColor);
				}
				if (_foregroundColor.IsEmpty == false)
				{
					foreBrush = new SolidBrush(_foregroundColor);
				}
			}
			finally
			{
				base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
			}
		}
		
		/// <summary>
		/// To deny making cell active (and editing of it)
		/// </summary>
		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible) { }
	}	
}
