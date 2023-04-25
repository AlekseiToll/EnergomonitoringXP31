using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EmGraphLib.Radial
{
	/// <summary>
	/// Implements legend
	/// </summary>
	public class Legend
	{
		#region Fields

		private Label label;
		private string textPattern = string.Empty;

		private Color color;
		private DashStyle style;
		private float thickness;
		private double zeroAngle;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public Legend()
		{
			label = new Label();
			Label.Paint += new PaintEventHandler(Label_Paint);
			label.AutoSize = true;
		}

		void Label_Paint(object sender, PaintEventArgs e)
		{
			if ((sender as Label) == label)
			{
				Pen pen = new Pen(color, thickness);
				pen.DashStyle = style;
				PointF p1 = new PointF(0, 6);
				PointF p2 = new PointF(10, 6);
				e.Graphics.DrawLine(pen, p1, p2);
				pen.Dispose();
			}
		}
		
		#endregion

		#region Public properties

			/// <summary>
		/// Gets or sets text pattern for legend
		/// </summary>
		public string TextPattern
		{
			get
			{
				return textPattern;
			}
			set
			{
				textPattern = "    " + value;
			}
		}

		public double ZeroAngle
		{
			get
			{
				return zeroAngle;
			}
			set
			{
				zeroAngle = value;
			}
		}
		
		
		#endregion

		#region Internal properties

		internal Label Label
		{
			get
			{
				return label;
			}
		}
		
		internal Color Color
		{
			get { return color; }
			set { color = value; }
		}
		internal DashStyle Style
		{
			get { return style; }
			set { style = value; }
		}
		internal float Thickness
		{
			get { return thickness; }
			set { thickness = value; }
		}

		#endregion
	}
}
