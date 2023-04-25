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
	public class GridTimeline
	{
		#region Fields

		private Color color = Color.Black;
		private DashStyle style = DashStyle.Solid;
		private float thickness = 1F;
		private Pen pen = new Pen(Color.Black, 1);
		private bool visible = true;
		private GridTimelineLabel label = new GridTimelineLabel();

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
		public GridTimelineLabel Label
		{
			get { return label; }
		}
				
		#endregion			

		#region Constructors

		/// <summary>Default constructor</summary>
		public GridTimeline() { }

		/// <summary>Constructor</summary>
		/// <param name="color">Line color</param>
		/// <param name="style">Line dash style</param>
		/// <param name="thickness">Line thickness</param>
		public GridTimeline(Color color, float thickness, DashStyle style)
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
		/// <param name="rect">Grid rectangle</param>
		/// <param name="start">Datetime of start</param>
		/// <param name="end">Datetime of end</param>
		/// <param name="dHeight">Height decrement value</param>
		/// <param name="spacing">Spacing value</param>
		internal void Draw(Graphics g, Rectangle rect, DateTime start, DateTime end, int spacing, out float dHeight)
		{
			dHeight = spacing;

			if (this.label.Visible)
			{
				dHeight += label.Font.Height;
			}

			// main horizontal line
			int lineY = (int)(rect.Bottom + spacing * 0.5 - dHeight);
			//g.DrawLine(this.pen, rect.Left, lineY, rect.Right, lineY);

			g.DrawLine(this.pen, rect.Left, rect.Top, rect.Left, rect.Bottom);
			g.DrawLine(this.pen, rect.Right, rect.Top, rect.Right, rect.Bottom);

			
			// vertical lines
			
			// start and end
			if (this.Label.Visible == true)
			{
				label.Draw(g, rect.Left, lineY + spacing * 0.5F, start, "HH:mm:ss.fff", GridTimelineLabel.AlignType.Left);
				label.Draw(g, rect.Right, lineY + spacing * 0.5F, end, "HH:mm:ss.fff", GridTimelineLabel.AlignType.Right);
			}
		}

		#endregion
	}
}
