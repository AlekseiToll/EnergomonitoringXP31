using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace EmGraphLib.DNS
{
	/// <summary>
	/// Grid line class
	/// </summary>
	public class GridLine
	{
		#region Fields

		private Color color = Color.Black;
		private DashStyle style = DashStyle.Solid;
		private float thickness = 1F;
		private Pen pen = new Pen(Color.Black, 1);
		private bool visible = true;
		private GridLineLabel label = new GridLineLabel();

		#endregion

		#region Properties

		/// <summary>Gets or sets is line is visible or not</summary>
		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}

		/// <summary>Gets or sets line color</summary>
		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				pen.Color = value;
			}
		}

		/// <summary>Gets or sets line dash style</summary>
		public DashStyle Style
		{
			get { return style; }
			set
			{
				style = value;
				pen.DashStyle = value;
			}
		}

		/// <summary>Gets or sets line thickness</summary>
		public float Thickness
		{
			get { return thickness; }
			set
			{
				thickness = value;
				pen.Width = value;
			}
		}

		/// <summary>Gets line label</summary>
		public GridLineLabel Label
		{
			get { return label; }
		}
				
		#endregion			

		#region Constructors

		/// <summary>Default constructor</summary>
		public GridLine() { }

		/// <summary>Constructor</summary>
		/// <param name="color">Line color</param>
		/// <param name="style">Line dash style</param>
		/// <param name="thickness">Line thickness</param>
		public GridLine(Color color, float thickness, DashStyle style)
		{
			this.color = color;
			this.style = style;
			this.thickness = thickness;

			this.pen = new Pen(this.color, this.thickness);
			this.pen.DashStyle = this.style;
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <param name="x">Line start x coordinate</param>
		/// <param name="y">Line start y coordinate</param>
		/// <param name="width">Line's width</param>
		/// <param name="value">Value</param>
		/// <param name="isNominal">Is line is nominal (true) or limit (false)</param>
		internal void Draw(Graphics g, float x0, float x, float y, float width, float value, bool isNominal)
		{
			g.DrawLine(this.pen, x, y, x + width, y);

			if (this.Label.Visible == true)
			{
				label.Draw(g, x0, y - this.label.Size(value).Height / 2, value);
			}
		}

		#endregion
	}
}
