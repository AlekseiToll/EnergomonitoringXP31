using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EmGraphLib.Radial
{
	public class RadialGrid
	{
		#region Fields

		/// <summary>
		/// Inner scale
		/// </summary>
		private double scale;
		/// <summary>
		/// Inner nominal value
		/// </summary>
		private double nominalValue;
		/// <summary>
		/// Inner foregroung (line) color
		/// </summary>
		private Color foreColor = Color.Gray;
		/// <summary>
		/// Inner line thickness
		/// </summary>
		private int thickness = 1;
		/// <summary>
		/// Inner line style
		/// </summary>
		private DashStyle style = DashStyle.Solid;
		/// <summary>
		/// Inner visible flag
		/// </summary>
		private bool visible = true;
		/// <summary>
		/// Inner tag
		/// </summary>
		private object tag = null;
				
		/// <summary>
		/// Gets or sets scale of the grid from (<c>0.01</c> upto <c>1.0</c>)
		/// </summary>
		public double Scale
		{
			get
			{
				return scale;
			}
			set
			{
				if (value >= 0.01 && value <= 1.0)
				{
					scale = value;
					if (NeedRedraw != null) NeedRedraw();
				}
			}
		}

		/// <summary>
		/// Gets or sets nominal value
		/// </summary>
		public double NominalValue
		{
			get
			{
				return nominalValue;
			}
			set
			{
				nominalValue = value;
				if (NeedRedraw != null) NeedRedraw();
			}
		}

		/// <summary>
		/// Gets or sets color of grid lines
		/// </summary>
		public Color ForeColor
		{
			get
			{
				return foreColor;
			}
			set
			{
				foreColor = value;
				if (NeedRedraw != null) NeedRedraw();
			}
		}

		/// <summary>
		/// Gets or sets line thickness
		/// </summary>
		public int Thickness
		{
			get
			{
				return thickness;
			}
			set
			{
				if (value > 0)
					thickness = value;			
			}
		}

		/// <summary>
		/// Gets or sets the line style
		/// </summary>
		public DashStyle Style
		{
			get
			{
				return style;
			}
			set
			{
				style = value;
			}
		}

		/// <summary>
		/// Gets or sets is grid visible or not
		/// </summary>
		public bool Visible
		{
			get
			{
				return visible;
			}
			set
			{
				visible = value;
				if (NeedRedraw != null) NeedRedraw();
			}
		}

		/// <summary>
		/// Gets or sets reserved for user object
		/// </summary>
		public object Tag
		{
			get
			{
				return tag;
			}
			set
			{
				tag = value;
			}
		}

		#endregion		

		#region Events
		/// <summary>
		/// NeedRedraw	event handler
		/// </summary>
		internal delegate void NeedRedrawHandler();
		/// <summary>
		/// Occures when one of properties have effect on drawing was changed
		/// </summary>
		internal static event NeedRedrawHandler NeedRedraw;
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nominalValue">Nominal value</param>
		/// <param name="part">Part of this grid from whole grid space</param>
		public RadialGrid(double nominalValue, double part) 
		{
			this.nominalValue = nominalValue;
			this.scale = part;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nominalValue">Nominal value</param>
		/// <param name="part">Part of this grid from whole grid space</param>
		/// <param name="foreColor">Color of grid lines</param>
		public RadialGrid(double nominalValue, double part, Color foreColor)
		{
			this.nominalValue = nominalValue;
			this.scale = part;
			this.foreColor = foreColor;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nominalValue">Nominal value</param>
		/// <param name="part">Part of this grid from whole grid space</param>
		/// <param name="thickness">Grid line thickness</param>
		/// <param name="style">Grid line style</param>
		public RadialGrid(double nominalValue, double part, int thickness, DashStyle style)
		{
			this.nominalValue = nominalValue;
			this.scale = part;
			this.thickness = thickness;
			this.style = style;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nominalValue">Nominal value</param>
		/// <param name="part">Part of this grid from whole grid space</param>
		/// <param name="thickness">Grid line thickness</param>
		/// <param name="style">Grid line style</param>
		/// <param name="foreColor">Color of grid lines</param>
		public RadialGrid(double nominalValue, double part, int thickness, DashStyle style, Color foreColor)
		{
			this.nominalValue = nominalValue;
			this.scale = part;
			this.foreColor = foreColor;
			this.thickness = thickness;
			this.style = style;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Draw nominal ellipse
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <param name="rect">Rectangle of circle</param>
		internal void DrawNominal(Graphics g, Rectangle rect)
		{
			// creating and customizing Pen object
			Pen _linePen = new Pen(foreColor, (float)this.thickness);
			_linePen.DashStyle = this.style;

			// drawing ellipse
			g.DrawEllipse(_linePen, rect);

			// disposing Pen object
			_linePen.Dispose();
		}

		#endregion
	}
}
