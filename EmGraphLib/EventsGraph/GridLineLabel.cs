using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace EmGraphLib.DNS
{
	public class GridLineLabel
	{
		#region Fields

		private Font font = new Font("Verdana", 8, FontStyle.Regular);
		private Color color = Color.Black;
		private string format = "{0}";
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
		/// Gets or sets label's value format
		/// </summary>
		public string Format
		{
			get { return format; }
			set { format = value; }
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
		public GridLineLabel() { }

		/// <summary>Constructor</summary>
		/// <param name="text">Label's text</param>
		public GridLineLabel(string text)
		{
			this.format = text;
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Gets label's text size
		/// </summary>
		internal Size Size(float value)
		{
			System.Drawing.Graphics g = Graphics.FromHwnd((new Panel()).Handle);
			if (format == null || font == null) return System.Drawing.Size.Empty;
			return g.MeasureString(string.Format(format, value), font).ToSize();
		}

		internal void Draw(Graphics g, float x, float y, float value)
		{
			g.DrawString(string.Format(this.format, value),	this.font, this.brush, x, y);
		}

		#endregion
	}
}
