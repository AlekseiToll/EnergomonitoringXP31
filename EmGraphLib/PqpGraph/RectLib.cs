using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EmGraphLib.PqpGraph
{
	class RectLib
	{
		/// <summary>
		/// Splits Rectangle horizontaly to the array of Rectangles
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="number">Number of destination rectangles</param>
		/// <returns>Array of rectangles</returns>
		public static Rectangle[] SplitHorRect(Rectangle rect, int number, int spacing)
		{
			if (rect.IsEmpty) return null;
			if (number < 1) return null;
			
			int spsum = spacing * (number - 1);			
			int dx = (rect.Width - spsum) / number;
			Rectangle[] rects = new Rectangle[number];
			for (int i = 0; i < number; i++)
			{
				rects[i] = new Rectangle(rect.X + i * (dx + spacing), rect.Y, dx, rect.Height);
			}
			return rects;
		}

		/// <summary>
		/// Cuts spacing value from each side of the rectangle
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="spacing">Spacing value in pixels to cut</param>
		/// <returns>New rectangle</returns>
		public static Rectangle CutSpacing(Rectangle rect, int spacing)
		{
			if (rect.IsEmpty) return Rectangle.Empty;

			return new Rectangle(
				rect.X + spacing,
				rect.Y + spacing,
				rect.Width - 2 * spacing,
				rect.Height - 2 * spacing);
		}

		/// <summary>
		/// Cuts spacing value from each side of the rectangle
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="spacing">Spacing value in pixels to cut</param>
		/// <param name="bTop">Need to cut top side</param>
		/// <param name="bBottom">Need to cut bottom side</param>
		/// <param name="bLeft">Need to cut left side</param>
		/// <param name="bRight">Need to cut right side</param>
		/// <returns>New rectangle</returns>
		public static Rectangle CutSpacing(Rectangle rect, int spacing, 
			bool bTop, bool bBottom, bool bLeft, bool bRight)
		{
			if (rect.IsEmpty) return Rectangle.Empty;

			int width = rect.Width;
			int height = rect.Height;
			int left = rect.Left;
			int top = rect.Top;

			if (bTop)
			{
				height -= spacing;
				top += spacing;
			}
			if (bBottom) height -= spacing;

			if (bLeft)
			{
				width -= spacing;
				left += spacing;
			}
			if (bRight) width -= spacing;

			return new Rectangle(left, top, width, height);
		}

		/// <summary>
		/// Centers size in the source rectangle
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="size">Destination size</param>
		/// <returns>Rectangle with size of destination size</returns>
		public static Rectangle CenterSize(Rectangle rect, Size size)
		{
			if (rect.IsEmpty) return rect;
			size.Width = size.Width < rect.Width ? size.Width : rect.Width;
			size.Height = size.Height < rect.Height ? size.Height : rect.Height;

			int x = rect.Left + (rect.Width - size.Width) / 2;
			int y = rect.Top + (rect.Height - size.Height) / 2;

			return new Rectangle(x, y, size.Width, size.Height);
		}

		/// <summary>
		/// Clips the source rectangle velrticaly
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="gridWldTop">Grid's top value (in world coordinates)</param>
		/// <param name="gridWldBottom">Grid's bottom value (in world coordinates)</param>
		/// <param name="barWldTop">Bar's top value (in world coordinates)</param>
		/// <param name="barWldBottom">Bar's bottom value (in world coordinates)</param>
		/// <returns>Cliped rectangle</returns>
		public static Rectangle Clip(Rectangle rect, 
			float gridWrdTop, float gridWldBottom, 
			float barWldTop, float barWldBottom)
		{
			if (rect.IsEmpty) return rect;
			if (gridWrdTop == gridWldBottom) return rect;
			
			int gridPxlTop = rect.Top;
			int gridPxlHeight = rect.Height;

			float r = (float)gridPxlHeight / (gridWrdTop - gridWldBottom);

			int barPxlTop = (int)Math.Ceiling(gridPxlTop + r * (gridWrdTop - barWldTop));
			int barPxlHeight = (int)Math.Ceiling(r * (barWldTop - barWldBottom));

			rect = new Rectangle(rect.X, barPxlTop, rect.Width, barPxlHeight);

			return rect;
		}
	}
}

