using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EmGraphLib.DNS
{
	public enum Phase
	{
		A = 0,
		B = 1,
		C = 2,
		AB = 3,
		BC = 4,
		CA = 5,
		ABCN = 6,
		ABC = 7
	}
	
	/// <summary>
	/// Dips and overs graph control
	/// </summary>
	public partial class DNOGraph : UserControl
	{
		#region Fields

		/// <summary>Inner spacing</summary>
		private int spacing = 5;

		/// <summary>Inner grid object</summary>
		private Grid grid = new Grid();

		/// <summary>Inner items list</summary>
		private ItemsList items = new ItemsList();

        private bool isRedrawHighlightOnly = false;

		private Phase[] phasesToDraw = null;

		// Для выделения
		private Rectangle itemsRect = new Rectangle(0, 0, 0, 0);

		private bool isSelectionGoes = false;
		private Point startPoint = new Point(0, 0);
		private Rectangle selRect = new Rectangle(0, 0, 0, 0);
		private Rectangle drawableRect = new Rectangle(0, 0, 0, 0);

		// для масштабирования
		private double tickMin;	// начальный отсчет для всего графика (т.е. с дефолтным масштабом)
		private double tickMax;	// конечный отсчет для всего графика (т.е. с дефолтным масштабом)

		private double visibleTickStart;	// с какого отсчета начинаем отображение
		private double visibleTickEnd;		// на каком отсчете заканчиваем отображение

		#endregion

		#region Properties

		/// <summary>Gets grid object</summary>
		public Grid Grid
		{
			get { return grid; }
		}

		/// <summary>Gets items list</summary>
		public ItemsList Items
		{
			get { return items; }
		}

		/// <summary>Gets or sets spacing value</summary>
		public int Spacing
		{
			get { return spacing; }
			set { if (value >= 0) spacing = value; }
		}
		
		/// <summary>Gets or sets phases to draw</summary>
		public Phase[] PhasesToDraw
		{
			get { return phasesToDraw; }
			set { phasesToDraw = value; }
		}

		public double TickMax
		{
			get { return tickMax; }
			set
			{
				tickMax = visibleTickEnd = value;
				items.End = new DateTime((long)tickMax);
			}
		}

		public double TickMin
		{
			get { return tickMin; }
			set
			{
				tickMin = visibleTickStart = value;
				items.Start = new DateTime((long)tickMin);				
			}
		}

		public double VisibleTickStart
		{
			get { return visibleTickStart; }
			set { visibleTickStart = value; }
		}

		public double VisibleTickEnd
		{
			get { return visibleTickEnd; }
			set { visibleTickEnd = value; }
		}

		public Bitmap Image
		{
			get 
			{
				Bitmap bitmap = new Bitmap(this.Width, this.Height);
				Graphics bitmapGraphics = Graphics.FromImage(bitmap);
				fillGraphics(bitmapGraphics, this.Width, this.Height);
				bitmapGraphics.Dispose();
				return bitmap; 
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Dips and swells
		/// </summary>
		public DNOGraph()
		{
			InitializeComponent();
		}

		#endregion

		#region Overriden base methods

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (this.visibleTickStart == 0 && this.visibleTickEnd == 0) return;
			if (this.phasesToDraw == null)
				return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle gridRect = new Rectangle();
            float minVal = 0;
            float maxVal = 0;
            int LeftFieldWidth = 0;
			
            GetRects(out gridRect, out itemsRect, out LeftFieldWidth);
			items.GetMinMaxValues(out minVal, out maxVal, grid.LimitValue, this.phasesToDraw, 
						new DateTime((long)visibleTickStart), new DateTime((long)visibleTickEnd));
			
            if (!isRedrawHighlightOnly)
            {
				// drawing items
				items.Draw(e.Graphics, itemsRect, minVal, maxVal, grid.LimitValue, false, 
						this.phasesToDraw, new DateTime((long)visibleTickStart), 
						new DateTime((long)visibleTickEnd));
                // drawing grid
				grid.Draw(e.Graphics, gridRect, minVal, maxVal, spacing, LeftFieldWidth, 
					visibleTickStart, visibleTickEnd);
			}
            else
            {
                Brush br = new SolidBrush(this.BackColor);
                e.Graphics.FillRectangle(br, LeftFieldWidth, 0, 
						gridRect.Width - LeftFieldWidth, 
						this.Height - gridRect.Height - spacing - 1);
                br.Dispose();
                // drawing items
				items.Draw(e.Graphics, itemsRect, minVal, maxVal, grid.LimitValue, true, 
						this.phasesToDraw, new DateTime((long)visibleTickStart), 
						new DateTime((long)visibleTickEnd));
                isRedrawHighlightOnly = false;
            }
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (this.itemsRect.Contains(e.X, e.Y))
				{
					this.isSelectionGoes = true;
					this.selRect = new Rectangle(0, 0, 0, 0);
					this.drawableRect = new Rectangle(0, 0, 0, 0);
					this.startPoint = new Point(e.X, itemsRect.Y);
				}
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (isSelectionGoes)
			{
				// сколько отсчетов в пикселе
				double ratio = (visibleTickEnd - visibleTickStart) / itemsRect.Width; 

				isSelectionGoes = false;

				if (Math.Abs(selRect.Width) > 3)
				{
					visibleTickStart = visibleTickStart + (selRect.Left - itemsRect.Left) * ratio;
					visibleTickEnd = visibleTickStart + (selRect.Right - selRect.Left) * ratio;
					Refresh();
				}
				//ControlPaint.DrawReversibleFrame(selRect, this.BackColor, FrameStyle.Dashed);
			}
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (itemsRect.Contains(e.X, e.Y))
			{
				if (this.Cursor != Cursors.Cross)
					this.Cursor = Cursors.Cross;
			}
			else this.Cursor = Cursors.Default;

			if (isSelectionGoes)
			{
				ControlPaint.DrawReversibleFrame(drawableRect, this.BackColor, FrameStyle.Dashed);

				//Point endPoint = this.PointToScreen(new Point(e.X, itemsRect.Y));
				
				if (itemsRect.Contains(e.X, e.Y))
				{
					selRect.X = startPoint.X < e.X ? startPoint.X : e.X;
					selRect.Y = itemsRect.Y;
					selRect.Width = Math.Abs(e.X - startPoint.X);
					selRect.Height = itemsRect.Height;

					drawableRect = RectangleToScreen(selRect);
				}

				// Draw the new rectangle by calling DrawReversibleFrame again.  
				ControlPaint.DrawReversibleFrame(drawableRect,
					this.BackColor, FrameStyle.Dashed);
			}
			
			base.OnMouseMove(e);
		}

		//protected override On

		#endregion	

		#region Private methods

        private void GetRects(out Rectangle gridRect, out Rectangle itemsRect, /*out float minVal, out float maxVal,*/ out int LeftFieldWidth)
        {
            //getMinMax(out minVal, out maxVal);

			Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
			rect = SubtractRect(rect, this.spacing);

			// creating grid rect
			gridRect = new Rectangle(rect.Location, rect.Size);
			// creating items rect and saving left field width

            // correcting grid rect for highlight if needed
            if (items.Highlight.Visible)
            {
                int spacingHighlight = 5;
                gridRect = SubtractRect(gridRect, 0, items.Highlight.Size.Height + spacingHighlight, 0, 0);
            }            
            itemsRect = getItemsRect(gridRect, this.spacing, out LeftFieldWidth);

        }

		/// <summary>
		/// Gets rectangle for items
		/// </summary>
		/// <param name="gridRect"></param>
		/// <param name="spacing"></param>
		/// <returns></returns>
		private Rectangle getItemsRect(Rectangle gridRect, int spacing, out int LeftFieldWidth)
		{
			int dWidth = 0;
			int dHeight = spacing;

			// finding maximum width of the left labels
			int sizeLimitP = this.grid.LimitLine.Label.Size(this.grid.LimitValue).Width;
			int sizeLimitM = this.grid.LimitLine.Label.Size(-this.grid.LimitValue).Width;
			int sizeNom = this.grid.NominalLine.Label.Size(this.grid.NominalValue).Width;
			int maxSize = sizeLimitM > sizeLimitP ? sizeLimitM : sizeLimitP;
			if (dWidth > maxSize) maxSize = sizeNom;
			dWidth += maxSize + 10;
			LeftFieldWidth = dWidth;

			// for timeline
			if (this.grid.TimeLine.Label.Visible)
			{
				dHeight += this.grid.TimeLine.Label.Font.Height;
			}

			// customizing outRect rectangle
			return SubtractRect(gridRect, dWidth, 0, 0, dHeight);
		}

		/// <summary>
		/// Calculates and returns grid <c>Rectangle</c>
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="spacing">Spacing value</param>
		/// <returns>New subtract region</returns>
		internal static Rectangle SubtractRect(Rectangle rect, int spacing)
		{
			return new Rectangle(
				rect.Top + spacing, 
				rect.Left + spacing, 
				rect.Width - 2 * spacing,
				rect.Height - 2 * spacing);
		}

		/// <summary>
		/// Calculates and returns grid <c>Rectangle</c>
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="dLeft">Left dx</param>
		/// <param name="dTop">Top dy</param>
		/// <param name="dRight">Right dx</param>
		/// <param name="dBottom">Bottom dy</param>
		/// <returns>New subtract region</returns>
		internal static Rectangle SubtractRect(Rectangle rect, int dLeft, int dTop, int dRight, int dBottom)
		{
			return new Rectangle(
				rect.Left + dLeft,
				rect.Top + dTop,				
				rect.Width - dLeft - dRight,
				rect.Height - dTop - dBottom);
		}

		// <summary>
		// Gets minimum and maximum values in the list
		// </summary>
		// <param name="minVal">minimum diviation value</param>
		// <param name="maxVal">maximum diviation value</param>
		/*private void getMinMax(out float minVal, out float maxVal)
		{
			if (items.Count == 0)
			{
				minVal = 0F;
				maxVal = 0F;				
			}
			else 
			{
				minVal = this.items[0].Value;
				maxVal = this.items[0].Value;
				
				for (int i = 1; i < items.Count; i++)
				{
					if (items[i].Value > maxVal) maxVal = items[i].Value;
					if (items[i].Value < minVal) minVal = items[i].Value;
				}
			}

			if (minVal > -grid.LimitValue) minVal = -grid.LimitValue;
			if (maxVal < grid.LimitValue) maxVal = grid.LimitValue;
		}*/

		#endregion

		#region Public methods

		/// <summary>
		/// Exports graph image to the file
		/// </summary>
		/// <param name="filename">File name to export</param>
		/// <returns>True if all ok or false</returns>
		public bool SaveImage(String filename)
		{
			Graphics g = Graphics.FromHwnd(this.Handle);
			

			return false;
		}

        public void RedrawHighlight()
        {
            Rectangle rect = new Rectangle();            
            PaintEventArgs e = new PaintEventArgs(Graphics.FromHwnd(this.Handle), rect);
            isRedrawHighlightOnly = true;
            this.OnPaint(e);
        }

		public void fillGraphics(Graphics graph, int w, int h)
		{
			Brush brush = new SolidBrush(Color.White);
			graph.FillRectangle(brush, 0, 0, w, h);

			if ((this.visibleTickStart != 0 || this.visibleTickEnd != 0) &&
				this.phasesToDraw != null)
			{
				graph.SmoothingMode = SmoothingMode.AntiAlias;

				Rectangle gridRect = new Rectangle();
				float minVal = 0;
				float maxVal = 0;
				int LeftFieldWidth = 0;

				GetRects(out gridRect, out itemsRect, out LeftFieldWidth);
				items.GetMinMaxValues(out minVal, out maxVal, grid.LimitValue,
							this.phasesToDraw,
							new DateTime((long)visibleTickStart),
							new DateTime((long)visibleTickEnd));

				// drawing items
				items.Draw(graph, itemsRect, minVal, maxVal,
						grid.LimitValue, false,
						this.phasesToDraw, new DateTime((long)visibleTickStart),
						new DateTime((long)visibleTickEnd));
				// drawing grid
				grid.Draw(graph, gridRect, minVal, maxVal, spacing,
						LeftFieldWidth,
						visibleTickStart, visibleTickEnd);
			}
		}

		/// <summary>
		/// Don not forget to refresh after reset
		/// </summary>
		public void ResetZoom()
		{
			visibleTickStart = tickMin;
			visibleTickEnd = tickMax;
		}

		#endregion
	}
}
