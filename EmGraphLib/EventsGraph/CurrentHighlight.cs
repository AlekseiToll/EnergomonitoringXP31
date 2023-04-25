using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EmGraphLib.DNS
{
	/// <summary>
	/// Current deviation highlighting triangle object
	/// </summary>
	public class CurrentHighlight
	{
		#region Fields

		private bool visible = true;
		private object tag = null;

		private Color foreColor = Color.Black;
		private Color backColor = Color.Gray;
		private Pen pen = new Pen(Color.Black);
		private Brush brush = new SolidBrush(Color.Gray);

		private Size size = new Size(10, 5);

		#endregion

		#region Properties

		/// <summary>Gets or sets is grid visible or not</summary>
		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}

		/// <summary>Gets or sets reserved for user object</summary>
		public object Tag
		{
			get { return tag; }
			set { tag = value; }
		}

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

		/// <summary>Gets or sets thiangle size</summary>
		public Size Size
		{
			get { return size; }
			set { size = value; }
		}

		#endregion

		#region Constructors

		public CurrentHighlight() { }

		#endregion

		#region Internal methods

		/// <summary>
		/// Draw grid lines
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <param name="x">Center x coordinate</param>
		/// <param name="y">Top y coodrinate</param>
		internal void Draw(Graphics g, float x, float y)
		{
			Point[] points = new Point[3];
			points[0].Offset((int)(x - this.size.Width / 2), (int)y);
			points[1].Offset((int)(x + this.size.Width / 2), (int)y);
			points[2].Offset((int)x, (int)(y + this.size.Height));

			g.DrawPolygon(this.pen, points);
			g.FillPolygon(this.brush, points);
		}

		#endregion
	}
}
