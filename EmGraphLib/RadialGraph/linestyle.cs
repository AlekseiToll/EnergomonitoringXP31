using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;

namespace EmGraphLib.Radial
{
	/// <summary>
	/// Contains vector style information depending of grid line style
	/// </summary>
	internal struct VectorStyle
	{
		/// <summary>
		/// Line style 
		/// </summary>
		public DashStyle Style;

		/// <summary>
		/// Thickness of line
		/// </summary>
		public float Thickness;
	}
}
