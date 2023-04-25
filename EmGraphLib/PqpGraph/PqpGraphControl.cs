using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EmGraphLib.PqpGraph
{
	//[System.Drawing.ToolboxBitmap(@"D:\test\EnergomonitoringXP.4.0.6b\EnergomonitoringXP v.4.0.6.0\EmGraphLib\PqpGraph\PqpGraph.bmp")]
	public partial class PqpGraphControl : UserControl
	{
		//private const bool DEBUG_FRAMES_DRAW = false;

		#region Fields
		
		private int spacing;
		private int padding;
		private List<Bar> bars;
		private Pen borderPen;
		private bool drawBorder;

		private string worldNanText;
		#endregion				
		
		#region Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		public PqpGraphControl()
		{
			InitializeComponent();

			spacing = 5;
			padding = 10;
			borderPen = new Pen(Color.SteelBlue);
			bars = new List<Bar>();			
			drawBorder = true;
		}

		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets spacing value
		/// </summary>
		[Browsable(true), Category("Appearance"), Description("Spacing value in pixels"), DefaultValue(5)]
		public int Spacing
		{
			get
			{
				return spacing;
			}
			set
			{
				if (value < 0 && value != spacing) return;
				spacing = value;
				if (this.DesignMode) this.Refresh();
			}
		}


		/// <summary>
		/// Gets or sets padding value beetwen Bars regions
		/// </summary>
		[Browsable(true), Category("Appearance"), Description("Padding value beetwen Bars regions"), DefaultValue(10)]
		public int BarPadding
		{
			get
			{
				return padding;
			}
			set
			{
				if (value != padding && value >= 0)
				{
					padding = value;
					if (this.DesignMode) this.Refresh();
				}
			}
		}

		/// <summary>
		/// Gets List of Bar objects
		/// </summary>
		[Browsable(true), 
		Category("Members"),
		Description("Bars to draw"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Localizable(true),
		RefreshProperties(RefreshProperties.All)]
		public List<Bar> Bars
		{
			get
			{
				return bars;
			}
			set
			{
				bars = value;
			}
		}

		/// <summary>
		/// Gets bar's bound color
		/// </summary>
		[Browsable(true), Category("Appearance"), Description("Color of the border")]
		public Color BorderColor
		{
			get
			{
				return borderPen.Color;
			}
			set
			{
				if (borderPen.Color != value)
				{
					borderPen = new Pen(value);
					if (this.DesignMode) this.Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets is need to draw bound or not
		/// </summary>
		[Browsable(true), Category("Appearance"), Description("Is border need to be drawn or not"), DefaultValue(true)]
		public bool DrawBorder
		{
			get
			{
				return drawBorder;
			}
			set
			{
				if (drawBorder != value)
				{
					drawBorder = value;
					if (this.DesignMode) this.Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets NaN text
		/// </summary>
		[Browsable(true), Category("Appearance"), Description("Text would displayed when bar's world coordinates are equals to NaN"), DefaultValue(""), Localizable(true)]
		public string WorldNanText
		{
			get
			{
				return worldNanText;
			}
			set
			{
				worldNanText = value;
			}
		}

		#endregion

		

		#region Overriden Functions

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			Draw(e.Graphics);			
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.Invalidate();
		}
		
		#endregion

		#region Private Functions

		/// <summary>
		/// Main drawing function
		/// </summary>
		private void Draw(Graphics g)
		{
			// drawing border
			Rectangle borderRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
			if (drawBorder)
				g.DrawRectangle(borderPen, borderRect);

			// calculating grid world
			float grid_WorldTop, grid_WorldBottom;
			CalcWorld(this.bars, out grid_WorldTop, out grid_WorldBottom);
			
			// calculating client rectangle			
			Rectangle clientRect = RectLib.CutSpacing(borderRect, this.spacing + 1);

			// drawing bars
			DrawBars(g, bars, clientRect, grid_WorldTop, grid_WorldBottom);
		}
		
		/// <summary>
		/// Gets world coordinates of the most top and the most bottom points of bar's list
		/// </summary>
		/// <param name="bs">List of Bar's object</param>
		/// <param name="top">Top point. Out</param>
		/// <param name="bottom">Bottom point. Out</param>
		private void CalcWorld(List<Bar> bs, out float top, out float bottom)
		{
			top = float.MinValue;
			bottom = float.MaxValue;

			foreach (Bar bar in bs)
			{
				if (bar.WorldBottom < bottom) bottom = bar.WorldBottom;
				if (bar.WorldTop > top) top = bar.WorldTop;
			}
		}

		private void DrawBars(Graphics g, List<Bar> bs, 
			Rectangle rect, float gridWorldTop, float gridWorldBottom )
		{
			if (bs == null) return;
			if (bs.Count == 0) return;

			Size bcsz = GetBarCaptionsSize(g, bs);
			
			Rectangle rectBars = RectLib.CutSpacing(rect, bcsz.Height, true, false, false, false);
			Rectangle[] rs = RectLib.SplitHorRect(rectBars, bs.Count, padding);

			Rectangle rectCaps = new Rectangle(rect.Left, rect.Top, rect.Width, bcsz.Height);
			Rectangle[] cs = RectLib.SplitHorRect(rectCaps, bs.Count, padding);
			for (int i = 0; i < bs.Count; i++)
			{
				DrawBar(g, bs[i], rs[i], cs[i], gridWorldTop, gridWorldBottom);
			}
		}


		/// <summary>
		/// Draws one bar. 
		/// Uses DrawBarCaption, DrawBarWorldCoords and DrawBarInstance functions
		/// </summary>
		private void DrawBar(Graphics g, Bar bar, 
			Rectangle barRect, Rectangle capRect, float gridWorldTop, float gridWorldBottom)
		{
			DrawBarCaption(g, bar, capRect);

			if (bar.WorldTop.Equals(float.NaN) || bar.WorldBottom.Equals(float.NaN))
			{
				Brush br = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal, (bar.BarBrush as SolidBrush).Color, this.BackColor);
				g.FillRectangle(br, barRect);
				br.Dispose();
				g.DrawRectangle(bar.BarPen, barRect);

				Size szNan = Size.Ceiling(g.MeasureString(this.WorldNanText, bar.PercentFont));
				Rectangle rectNan = RectLib.CenterSize(barRect, szNan);

				g.FillRectangle(new SolidBrush(this.BackColor), rectNan);
				g.DrawString(this.WorldNanText, bar.PercentFont, bar.PercentBrush, rectNan);
								
				return;
			}

			Size pctSize = bar.GetPercentTextSize(g);
			Size wldSize = bar.GetWorldTextSize(g);

			bool F_BNOSPC = false;	// no horizontal space to draw bar flag
			if (pctSize.Width + wldSize.Width > barRect.Width) F_BNOSPC = true;

			Rectangle baseRect;
			if (F_BNOSPC)
			{
				baseRect = RectLib.Clip(barRect,
					gridWorldTop, gridWorldBottom, bar.WorldTop, bar.WorldBottom);
				DrawBarInstance(g, bar, barRect, baseRect, pctSize);
				return;
			}			

			baseRect = RectLib.CutSpacing(barRect, 
				wldSize.Width, false, false, true, false);
			Rectangle insRect = RectLib.Clip(baseRect, 
				gridWorldTop, gridWorldBottom, bar.WorldTop, bar.WorldBottom);
			DrawBarInstance(g, bar, baseRect, insRect, pctSize);

			baseRect = new Rectangle(
				barRect.Left, barRect.Top, wldSize.Width, barRect.Height);
			Rectangle wldRect = RectLib.Clip(baseRect, 
				gridWorldTop, gridWorldBottom, bar.WorldTop, bar.WorldBottom);
			DrawBarWorldCoords(g, bar, baseRect, wldRect, wldSize);
		}

		private void DrawBarWorldCoords(Graphics g, Bar bar, Rectangle baseRect, Rectangle wldRect, Size wldSize)
		{
			if (bar.WorldTop != bar.WorldBottom)
			{
				Rectangle topRect = new Rectangle(
					wldRect.Location, wldSize);
				Rectangle bottomRect = new Rectangle(
					wldRect.Left, wldRect.Bottom - wldSize.Height, wldSize.Width, wldSize.Height);

				if (wldSize.Height > wldRect.Height)
				{
					topRect.Y = wldRect.Top + wldRect.Height / 2 - topRect.Height;
					bottomRect.Y = topRect.Bottom;
				}

				if (bottomRect.Top < topRect.Bottom)
				{
					topRect.Y = wldRect.Top;
					bottomRect.Y = topRect.Bottom;
				}

				if (bottomRect.Bottom - 1 > baseRect.Bottom)
				{
					bottomRect.Y = baseRect.Bottom - bottomRect.Height + 1;
					topRect.Y = bottomRect.Top - topRect.Height;
				}

				if (topRect.Top < baseRect.Top)
				{
					topRect.Y = baseRect.Top;
					bottomRect.Y = topRect.Bottom;
				}

				string topStr = bar.WorldTop.ToString(bar.WorldValFormat);
				string bottomStr = bar.WorldBottom.ToString(bar.WorldValFormat);

				g.DrawString(topStr, bar.WorldValFont, bar.WorldValBrush, topRect);
				g.DrawString(bottomStr, bar.WorldValFont, bar.WorldValBrush, bottomRect);
				// DEBUG REGINFO
                //if (DEBUG_FRAMES_DRAW)
                //{
                //    g.DrawRectangle(bar.BarPen, topRect);
                //    g.DrawRectangle(bar.BarPen, bottomRect);
                //}
			}
			else
			{
				//wldSize.Height /= 2;
				Rectangle topRect = new Rectangle(wldRect.Location, wldSize);
				
				if (wldSize.Height > wldRect.Height)
				{
					topRect.Y = wldRect.Top + wldRect.Height / 2 - topRect.Height / 2;
				}

				if (topRect.Bottom - 1 > baseRect.Bottom)
				{
					topRect.Y = baseRect.Bottom - topRect.Height + 1;
				}

				if (topRect.Top < baseRect.Top)
				{
					topRect.Y = baseRect.Top;
				}

				string topStr = bar.WorldTop.ToString(bar.WorldValFormat);

				g.DrawString(topStr, bar.WorldValFont, bar.WorldValBrush, topRect);
				// DEBUG REGINFO
                //if (DEBUG_FRAMES_DRAW)
                //{
                //    g.DrawRectangle(bar.BarPen, topRect);
                //}
			}
		}

		private void DrawBarInstance(Graphics g, Bar bar, Rectangle baseRect, Rectangle barInstanceRect, Size pctSize)
		{
			if (barInstanceRect.Height > 0)
			{
				g.FillRectangle(bar.BarBrush, barInstanceRect);
				g.DrawRectangle(bar.BarPen, barInstanceRect);
			}
			else
			{
				g.DrawLine(bar.BarPen,
					barInstanceRect.Left,
					barInstanceRect.Top,
					barInstanceRect.Right,
					barInstanceRect.Bottom);
			}
			// trying to draw percent text
			Rectangle pctRect = RectLib.CenterSize(barInstanceRect, pctSize);
			
			Size sz = bar.GetPercentTextSize(g);
			if (sz.Height > pctRect.Height)
			{
				pctRect.Height = sz.Height;
				pctRect.Y = barInstanceRect.Bottom;
				if (pctRect.Bottom > baseRect.Bottom) 
				{
					pctRect.Y = barInstanceRect.Top - sz.Height;
				}
			}
			

			g.DrawString(bar.PercentText, bar.PercentFont, bar.PercentBrush, pctRect);
			// DEBUG REGINFO
            //if (DEBUG_FRAMES_DRAW)
            //{
            //    g.DrawRectangle(bar.BarPen, pctRect);
            //}
		}

		private void DrawBarCaption(Graphics g, Bar bar, Rectangle capRect)
		{
			//capRect = RectLib.CutSpacing(capRect, bar.TextSpacing);
			g.DrawString(bar.Caption, bar.CaptionFont, bar.CaptionBrush, capRect);
			// DEBUG REGINFO
            //if (DEBUG_FRAMES_DRAW)
            //{
            //    g.DrawRectangle(bar.BarPen, capRect);
            //}
		}

		private Size GetBarCaptionsSize(Graphics g, List<Bar> bs)
		{
			Size sz = new Size(0, 0);

			foreach (Bar bar in bs)
			{
				Size s = bar.GetCaptionSize(g);
				if (s.Height > sz.Height) sz.Height = s.Height;
				if (s.Width > sz.Width) sz.Width = s.Width;
			}
			return sz;
		}

		#endregion
	}
}
