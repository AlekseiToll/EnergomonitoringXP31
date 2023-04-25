using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace EmGraphLib.DNS
{
	public class GridTimelineLabel
	{
		internal enum AlignType
		{
			Left,
			Right,
			Center
		}

		#region Fields

		private Font font = new Font("Verdana", 8, FontStyle.Regular);
		private Color color = Color.Black;
		private bool visible = true;
		private Brush brush = new SolidBrush(Color.Black);

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets label's font
		/// </summary>
		public Font Font
		{
			get { return font; }
			set { font = value; }
		}

		/// <summary>
		/// Gets or sets label's font color
		/// </summary>
		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				brush = new SolidBrush(color);
			}
		}

		/// <summary>
		/// Gets or sets is grid line visible or not
		/// </summary>
		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}

		#endregion

		#region Constructors

		/// <summary>Constructor</summary>
		public GridTimelineLabel() { }

		#endregion

		#region Internal methods

		/// <summary>
		/// Draw time label
		/// </summary>
		/// <param name="g">Graphics</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="time">Datetime value to draw</param>
		/// <param name="format">Datetime format</param>
		/// <param name="align">Align type</param>
		internal void Draw(Graphics g, float x, float y, DateTime time, string format, AlignType align)
		{

			if (align != AlignType.Left)
			{
				int width = this.Size(time, format).Width;
				switch (align)
				{
					case AlignType.Right:
						x -= width;
						break;
					case AlignType.Center:
						x -= width / 2;
						break;
				}
			}

			g.DrawString(time.ToString(format, Thread.CurrentThread.CurrentCulture), font, brush, x, y);

		}
		
		#endregion

		/// <summary>
		/// Gets label's text size
		/// </summary>
		private Size Size(DateTime time, string format)
		{
			System.Drawing.Graphics g = Graphics.FromHwnd((new Panel()).Handle);
			if (format == null || font == null) return System.Drawing.Size.Empty;
			return g.MeasureString(time.ToString(format), font).ToSize();
		}
	}
}
