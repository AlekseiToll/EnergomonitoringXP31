using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DataGridColumnStyles
{
	public class DataGridColumnGroupCaption : DataGridColumnHeaderFormula, ICaptionGroupColumnStyle
	{
		private DataGridGroupCaption _caption = null;
		private int _index = 0;

		#region ICaptionGroupColumnStyle Members

		public DataGridGroupCaption GroupCaption
		{
			get
			{
				return _caption;
			}
			set
			{
				_caption = value;
			}
		}

		public int GroupIndex
		{
			get
			{
				return _index;
			}
			set
			{
				_index = value;
			}
		}

		public int ColumnsInGroup
		{
			get 
			{
				if (_caption != null)
					return _caption.Colspan;
				else
					return 0;
			}
		}

		#endregion

		public DataGridColumnGroupCaption() { }
		public DataGridColumnGroupCaption(DataGridGroupCaption Caption, int Index )
		{
			_caption = Caption;
			_index = Index;
		}

		public DataGridColumnGroupCaption(DataGridGroupCaption Caption, int Index, string HeaderFormula)
		{
			_caption = Caption;
			_index = Index;
			this.HeaderFormula = HeaderFormula;
		}

		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			try
			{
				/// check for one per column drawing
				// trying to hit left point...
				int FirstVisibleRowNum = this.DataGridTableStyle.DataGrid.HitTest(bounds.Left, 3 * bounds.Height).Row;
				// if left point not in visible rectangle... trying to hit right point
				if (FirstVisibleRowNum == -1)
					FirstVisibleRowNum = this.DataGridTableStyle.DataGrid.HitTest(bounds.Left + bounds.Width, 3 * bounds.Height).Row;

				/// new (for groups)
				// thisIndex
				int thisIndex = DataGridTableStyle.GridColumnStyles.IndexOf(this);

				// check for one per group _caption drawing
				bool IsFirstVisibleInGroup = false;
				if (this._index == 0 || thisIndex - 1 < this.DataGridTableStyle.DataGrid.FirstVisibleColumn || this.DataGridTableStyle.GridColumnStyles[thisIndex - 1].Width == 0)
				{
					IsFirstVisibleInGroup = true;
				}

				if (_caption != null && rowNum == FirstVisibleRowNum && IsFirstVisibleInGroup == true)
				{
					// defining drawing region
					Graphics dgG = Graphics.FromHwnd(this.DataGridTableStyle.DataGrid.Handle);
					dgG.Clip = new Region(new Rectangle(this.DataGridTableStyle.DataGrid.RowHeaderWidth + 1, 2, this.DataGridTableStyle.DataGrid.Width - DataGridTableStyle.DataGrid.RowHeaderWidth - 3, 19));

					// width
					int width = 0;
					for (int i = thisIndex - this._index; i < thisIndex - this._index + _caption.Colspan; i++)
					{
						width += this.DataGridTableStyle.GridColumnStyles[i].Width;
					}

					// height
					int height = 18;

					// left
					int left = bounds.Left;
					for (int i = thisIndex - this._index; i < thisIndex; i++)
					{
						left -= this.DataGridTableStyle.GridColumnStyles[i].Width;
					}
					//top
					int top = 2;

					dgG.FillRectangle(_caption.backBrush, left, top, width, height + 1);
					// left  line
					dgG.DrawLine(_caption.penBorder, left - 1, top, left - 1, top + height);
					// right line
					dgG.DrawLine(_caption.penBorder, left + width - 1, top, left + width - 1, top + height);
					if (_caption.Text != String.Empty)
					{
						dgG.DrawString(_caption.Text, _caption.TextFont, foreBrush, new Rectangle(left + 4, top + 3, width, height));
					}

					if (this.DataGridTableStyle.DataGrid.FirstVisibleColumn == thisIndex)
					{
						dgG.DrawLine(_caption.penBorder, this.DataGridTableStyle.DataGrid.RowHeaderWidth + 1, 2, this.DataGridTableStyle.DataGrid.RowHeaderWidth + 1, top + height);
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
