// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DataGridColumnStyles
{
	/// <summary>
	/// Interface of column style with one-color cell
	/// </summary>
	public interface IColoredColumnStyle
	{
		/// <summary>
		/// Color of the column's background
		/// </summary>
		Color BackgroungColor
		{
			get;
			set;
		}

		/// <summary>
		/// Color of the column's text
		/// </summary>
		Color ForegroundColor
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Interface of column style with any-color cell
	/// </summary>
	public interface IAnyColoredColumnStyle
	{
		/// <summary>
		/// Event CellPaint
		/// </summary>
		event DataGridEventHandler CellPaint;
	}

	/// <summary>
	/// Interface of column style with caption group
	/// </summary>
	public interface ICaptionGroupColumnStyle
	{
		DataGridGroupCaption GroupCaption
		{
			get;
			set;
		}

		int GroupIndex
		{
			get;
			set;
		}

		int ColumnsInGroup
		{
			get;
		}
	}

	/// <summary>
	/// Interface of column style with formula in header
	/// </summary>
	public interface IHeaderFormulaColumnStyle
	{
		string HeaderFormula
		{
			get;
			set;
		}

		bool HeaderIsFormula
		{
			get;
		}

		Font HeaderRegularFont
		{
			get;
			set;
		}

		Font HeaderSubscriptFont
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Interface of column style with formula in all cells of column
	/// </summary>
	public interface ICellFormulaColumnStyle
	{
		Font CellRegularFont
		{
			get;
			set;
		}

		Font CellSubscriptFont
		{
			get;
			set;
		}
	}

	/// <summary>
	/// DataGridColorEventArgs class
	/// </summary>
	public class DataGridEventArgs : System.EventArgs
	{
		private int _column;
		/// <summary>
		/// Gets number of the cell's column
		/// </summary>
		public int Column
		{
			get
			{
				return _column;
			}
		}

		private int _row;
		/// <summary>
		/// Gets number of the cell's row
		/// </summary>
		public int Row
		{
			get
			{
				return _row;
			}
		}

		public object _cellValue;
		/// <summary>
		/// Gets current cell value
		/// </summary>
		public object CellValue
		{
			get
			{
				return _cellValue;
			}
		}

		private Font _textFont;		
		/// <summary>
		/// Gets or sets text font
		/// </summary>
		public Font TextFont
		{
			get
			{
				return _textFont;
			}
			set
			{
				_textFont = value;
			}
		}

		private Brush _backBrush;
		/// <summary>
		/// Gets or sets background brush
		/// </summary>
		public Brush BackBrush
		{
			get
			{
				return _backBrush;
			}
			set
			{
				_backBrush = value;
			}
		}

		private Brush _foreBrush;
		/// <summary>
		/// Gets or sets foreground color
		/// </summary>
		public Brush ForeBrush
		{
			get
			{
				return _foreBrush;
			}
			set
			{
				_foreBrush = value;
			}
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Column">Column number</param>
		/// <param name="Row">Row number</param>
		/// <param name="CellValue">Cell value</param>
		public DataGridEventArgs(int Column, int Row, object CellValue)
		{
			_column = Column;
			_row = Row;
			_cellValue = CellValue;
		}
	}

	/// <summary>
	/// Delegate for OnCellPaint method
	/// </summary>
	public delegate void DataGridEventHandler(object sender, DataGridEventArgs e);
}
