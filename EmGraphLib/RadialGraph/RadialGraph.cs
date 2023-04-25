using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EmGraphLib.Radial
{
	public partial class RadialGraph : UserControl
	{
		#region Constants
		
		/// <summary>
		/// Toolbox group name
		/// </summary>
		const string __CAT_GRAPH_DESIGN__ = "Graph disign";

		#endregion

		#region Fields
		/// <summary>
		/// inner title
		/// </summary>
		private string title = string.Empty;
		/// <summary>
		/// inner list of the grids
		/// </summary>
		private RadialGridList radialGridList;
		/// <summary>
		/// inner list of the curves
		/// </summary>
		private VectorCurveList curveList;
		/// <summary>
		/// inner flag signed if vector module takes place
		/// </summary>
		private bool realValues = false;

		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets graph title
		/// </summary>
		[Browsable(true), Category(__CAT_GRAPH_DESIGN__), Description("Graph title")]
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				title = value;
			}
		}

		/// <summary>
		/// Gets or sets is real values will be drawn (or all as nominals)
		/// </summary>
		[Browsable(true), Category(__CAT_GRAPH_DESIGN__), Description("Is covers will be drawn as real values (or as nominal values)")]
		public bool RealValues
		{
			get
			{
				return realValues;
			}
			set
			{
				if (realValues != value)
				{
					realValues = value;
					OnNeedRedraw();
				}
			}
		}

		/// <summary>
		/// Gets radial grid
		/// </summary>
		[Browsable(false)]
		public RadialGridList RadialGridList
		{
			get
			{
				return radialGridList;
			}			
		}

		/// <summary>Gets list of curves</summary>
		[Browsable(false)]
		public VectorCurveList CurveList
		{
			get
			{
				return curveList;
			}
		}

		/// <summary>Gets graph as bitmap</summary>
		public Bitmap Image
		{
			get
			{
				Bitmap bitmap = new Bitmap(this.Width, this.Height);
				Graphics bitmapGraphics = Graphics.FromImage(bitmap);
				fillGraphics(bitmapGraphics, this.Width, this.Height);
				bitmapGraphics.Dispose();

				// screenshot
				/*Rectangle rect = this.RectangleToScreen(this.Bounds);
				Bitmap bitmap = new Bitmap(rect.Width, rect.Height);

				Graphics bmpGraphics = Graphics.FromImage(bitmap);
				bmpGraphics.CopyFromScreen(rect.X, rect.Y, 0, 0,
							new Size(rect.Width, rect.Height));*/

				return bitmap;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public RadialGraph()
		{
			InitializeComponent();
			
			radialGridList = new RadialGridList();
			RadialGridList.NeedRedraw += new RadialGridList.NeedRedrawHandler(OnNeedRedraw);
			curveList = new VectorCurveList(myFowPanel);
			VectorCurveList.NeedRedraw += new VectorCurveList.NeedRedrawHandler(OnNeedRedraw);
		}
		#endregion

		#region Event handlers

		/// <summary>
		/// Inner OnNeedRedraw method	
		/// </summary>
		void OnNeedRedraw()
		{
			this.Invalidate();
		}

		#endregion

		#region Overriden base methods
		
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			// grids ratios
			double[] gridRatios = null;
			// grid nominals
			double[] gridNominals = null;

			VectorStyle[] vectorStyles = null;

			// grid rectangle
			Rectangle gridRect = _getGridBounds(new Rectangle(0, 0, this.Bounds.Width, this.Bounds.Height));

			System.Diagnostics.Debug.WriteLine(gridRect.ToString());

			if (radialGridList != null)
			{
				radialGridList.Draw(e.Graphics, gridRect, this.curveList, this.realValues, out gridRatios, out gridNominals, out vectorStyles);
			}

			if (curveList != null)
			{
				curveList.DrawCurves(e.Graphics, gridRect, this.radialGridList.ZeroAngle, gridRatios, gridNominals, vectorStyles, this.realValues);
			}			
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.Invalidate();
		}

		#endregion
		
		#region Private methods
		/// <summary>
		/// Scales source <paramref name="rect"/>
		/// </summary>
		/// <param name="rect">Source rectangle</param>
		/// <param name="scale">Scale value (from 0.01 upto 1.0)</param>
		/// <returns>Scaled <c>Rectangle</c> if all correct or <c>Rectangle.Empty</c></returns>
		internal static Rectangle _scaleRect(Rectangle rect, double scale)
		{
			if (rect.IsEmpty) return Rectangle.Empty;
			if (scale < 0.01 || scale > 1.0) return Rectangle.Empty;
			if (scale == 1.0) return rect;
			int newWidth = (int)(scale * rect.Width);
			int newHeight = (int)(scale * rect.Height);
			int dTop = (int)((rect.Height - newHeight) / 2);
			int dLeft = (int)((rect.Width - newWidth) / 2);
			return new Rectangle(rect.Left + dLeft, rect.Top + dTop, newWidth, newHeight);
		}

		/// <summary>
		/// Gets grid rectangle on the whole control place
		/// </summary>
		/// <param name="ClipRectangle">Rectangle of whole control</param>
		/// <returns>Rectangle of grid</returns>
		private Rectangle _getGridBounds(Rectangle ClipRectangle)
		{
			if (myFowPanel.Dock == DockStyle.Top)
			{
				ClipRectangle.Y += myFowPanel.Height + 2;
				ClipRectangle.Height -= myFowPanel.Height + 2;
				double top, left, side;
				top = ClipRectangle.Top;
				left = ClipRectangle.Left;
				side = (ClipRectangle.Height < ClipRectangle.Width) ? ClipRectangle.Height - 4 : ClipRectangle.Width - 4;
				return Rectangle.Round(new RectangleF((float)left, (float)top, (float)side, (float)side));
			}
			else
			{				
				ClipRectangle.Width -= myFowPanel.Width - 10;
				double top, left, side;
				top = ClipRectangle.Top;
				left = ClipRectangle.Left;
				side = (ClipRectangle.Height < ClipRectangle.Width) ? ClipRectangle.Height - 4 : ClipRectangle.Width - 4;
				return Rectangle.Round(new RectangleF((float)left, (float)top, (float)side, (float)side));
			}
		}

		private void myFowPanel_Resize(object sender, EventArgs e)
		{
			OnNeedRedraw();
		}

		private void RadialGraph_Resize(object sender, EventArgs e)
		{
			// vertical align
			if (this.Size.Width >= this.Size.Height * 1.75 && myFowPanel.Dock != DockStyle.Right)
			{
				myFowPanel.Dock = DockStyle.Right;
				myFowPanel.FlowDirection = FlowDirection.TopDown;
			}
			if (this.Size.Width < this.Size.Height * 1.75 && myFowPanel.Dock != DockStyle.Top)
			{
				myFowPanel.Dock = DockStyle.Top;
				myFowPanel.FlowDirection = FlowDirection.LeftToRight;
			}
		}

		#endregion

		#region Public methods

		public void fillGraphics(Graphics graph, int w, int h)
		{
			Brush brush = new SolidBrush(Color.White);
			graph.FillRectangle(brush, 0, 0, w, h);

			// grids ratios
			double[] gridRatios = null;
			// grid nominals
			double[] gridNominals = null;

			VectorStyle[] vectorStyles = null;

			// grid rectangle
			Rectangle gridRect = _getGridBounds(new Rectangle(0, 0, w, h));

			if (radialGridList != null)
			{
				radialGridList.Draw(graph, gridRect, this.curveList, this.realValues, out gridRatios, out gridNominals, out vectorStyles);
			}

			if (curveList != null)
			{
				curveList.DrawCurves(graph, gridRect, this.radialGridList.ZeroAngle, gridRatios, gridNominals, vectorStyles, this.realValues);
			}

			if (curveList != null)
			{
				for (int i = 0; i < curveList.Count; ++i)
				{
					for (int j = 0; j < curveList[i].LegendsCount; ++j)
					{
						//string text = curveList[i].GetLegend(j).TextPattern;
						string text = String.Format(curveList[i].GetLegend(j).TextPattern,
								Math.Round(curveList[i].VectorPairList[j].Module, 5),
								Math.Round(curveList[i].VectorPairList[j].Angle - 
										curveList[i].GetLegend(j).ZeroAngle, 4));
						Rectangle rect = curveList[i].GetLegend(j).Label.Bounds;
						rect.X += myFowPanel.Bounds.X;
						rect.Y += myFowPanel.Bounds.Y;
						Brush brushTxt = new SolidBrush(Color.Black);
						graph.DrawString(text, SystemFonts.DefaultFont,
												brushTxt, rect);

						Pen pen = new Pen(curveList[i].GetLegend(j).Color,
										curveList[i].GetLegend(j).Thickness);
						pen.DashStyle = curveList[i].GetLegend(j).Style;
						PointF p1 = new PointF(0 + rect.X, 6 + rect.Y);
						PointF p2 = new PointF(10 + rect.X, 6 + rect.Y);
						graph.DrawLine(pen, p1, p2);
						pen.Dispose();
					}
				}
			}
		}

		#endregion
	}
}