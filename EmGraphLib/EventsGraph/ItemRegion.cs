using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EmGraphLib.DNS
{

	public class ItemRegion
	{
		#region Fields

		private Color foreColor;
		private Color backColor;

		private Pen pen;
		private Brush brush;

		#endregion

		#region Properties

		/// <summary>Gets or sets region back color</summary>
		public Color BackColor
		{
			get { return backColor; }
			set
			{
				backColor = value;
				brush = new SolidBrush(backColor);
			}
		}

		/// <summary>Gets or sets region border color</summary>
		public Color ForeColor
		{
			get { return foreColor; }
			set
			{
				foreColor = value;
				pen = new Pen(foreColor);
			}
		}

		/// <summary>Gets region border Pen</summary>
		internal Pen Pen
		{
			get { return pen; }
		}

		/// <summary>Gets region Brush</summary>

		internal Brush Brush
		{
			get { return brush; }
		}

		#endregion

		#region Constructors

		/// <summary>Constructor</summary>
		public ItemRegion()
		{
			this.ForeColor = Color.Gray;
			this.BackColor = Color.FromArgb(0x80, Color.Silver);
		}

		#endregion
	}
}
