/*
NPlot - A charting library for .NET

BarPlot.cs
Copyright (C) 2003
Matt Howlett

Redistribution and use of NPlot or parts there-of in source and
binary forms, with or without modification, are permitted provided
that the following conditions are met:

1. Re-distributions in source form must retain at the head of each
   source file the above copyright notice, this list of conditions
   and the following disclaimer.

2. Any product ("the product") that makes use NPlot or parts 
   there-of must either:
  
    (a) allow any user of the product to obtain a complete machine-
        readable copy of the corresponding source code for the 
        product and the version of NPlot used for a charge no more
        than your cost of physically performing source distribution,
	    on a medium customarily used for software interchange, or:

    (b) reproduce the following text in the documentation, about 
        box or other materials intended to be read by human users
        of the product that is provided to every human user of the
        product: 
   
              "This product includes software developed as 
              part of the NPlot library project available 
              from: http://www.nplot.com/" 

        The words "This product" may optionally be replace with 
        the actual name of the product.

------------------------------------------------------------------------

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Drawing;

namespace NPlot
{

	/// <summary>
	/// Draws 
	/// </summary>
	public class BarPlot : BasePlot, IPlot, IDrawable
	{

		/// <summary>
		/// Default Constructor
		/// </summary>
		public BarPlot()
		{
		}


		/// <summary>
		/// Gets or sets the data, or column name for the ordinate [y] axis.
		/// </summary>
		public object OrdinateDataTop
		{
			get
			{
				return this.ordinateDataTop_;
			}
			set
			{
				this.ordinateDataTop_ = value;
			}
		}
		private object ordinateDataTop_ = null;

		
		/// <summary>
		/// Gets or sets the data, or column name for the ordinate [y] axis.
		/// </summary>
		public object OrdinateDataBottom
		{
			get
			{
				return this.ordinateDataBottom_;
			}
			set
			{
				this.ordinateDataBottom_ = value;
			}
		}
		private object ordinateDataBottom_ = null;


		/// <summary>
		/// Gets or sets the data, or column name for the abscissa [x] axis.
		/// </summary>
		public object AbscissaData
		{
			get
			{
				return this.abscissaData_;
			}
			set
			{
				this.abscissaData_ = value;
			}
		}
		private object abscissaData_ = null;


		/// <summary>
		/// Draws the line plot on a GDI+ surface against the provided x and y axes.
		/// </summary>
		/// <param name="g">The GDI+ surface on which to draw.</param>
		/// <param name="xAxis">The X-Axis to draw against.</param>
		/// <param name="yAxis">The Y-Axis to draw against.</param>
		public void Draw( Graphics g, PhysicalAxis xAxis, PhysicalAxis yAxis )
		{
			SequenceAdapter dataTop = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateDataTop, this.AbscissaData );

			SequenceAdapter dataBottom = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateDataBottom, this.AbscissaData );

			ITransform2D t = Transform2D.GetTransformer( xAxis, yAxis );
			
			for (int i=0; i<dataTop.Count; ++i)
			{
				PointF physicalBottom = t.Transform( dataBottom[i] );
				PointF physicalTop = t.Transform( dataTop[i] );

				Rectangle r = new Rectangle( (int)physicalBottom.X - 4, (int)physicalTop.Y,
					8, (int)(physicalBottom.Y - physicalTop.Y) );

				g.FillRectangle( this.rectangleBrush_.Get(r), r );
				g.DrawRectangle( borderPen_, r );

			}

		}

		/// <summary>
		/// Returns an x-axis that is suitable for drawing this plot.
		/// </summary>
		/// <returns>A suitable x-axis.</returns>
		public Axis SuggestXAxis()
		{
			SequenceAdapter dataBottom_ = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateDataBottom, this.AbscissaData );

			SequenceAdapter dataTop_ = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateDataTop, this.AbscissaData );

			return dataBottom_.SuggestXAxis();
		}


		/// <summary>
		/// Returns a y-axis that is suitable for drawing this plot.
		/// </summary>
		/// <returns>A suitable y-axis.</returns>
		public Axis SuggestYAxis()
		{
			SequenceAdapter dataBottom_ = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateDataBottom, this.AbscissaData );

			SequenceAdapter dataTop_ = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateDataTop, this.AbscissaData );

			return dataTop_.SuggestYAxis();
		}


		/// <summary>
		/// Draws a representation of this plot in the legend.
		/// </summary>
		/// <param name="g">The graphics surface on which to draw.</param>
		/// <param name="startEnd">A rectangle specifying the bounds of the area in the legend set aside for drawing.</param>
		public virtual void DrawInLegend(Graphics g, Rectangle startEnd)
		{
			int smallerHeight = (int)(startEnd.Height * 0.5f);
			int heightToRemove = (int)(startEnd.Height * 0.5f);
			Rectangle newRectangle = new Rectangle( startEnd.Left, startEnd.Top + smallerHeight / 2, startEnd.Width, smallerHeight );
			g.FillRectangle( rectangleBrush_.Get( newRectangle ), newRectangle );
			g.DrawRectangle( borderPen_, newRectangle );
		}

		/// <summary>
		/// The pen used to draw the plot
		/// </summary>
		public System.Drawing.Pen BorderPen
		{
			get
			{
				return borderPen_;
			}
			set
			{
				borderPen_ = value;
			}
		}
		private System.Drawing.Pen borderPen_ = new Pen(Color.Black);


		/// <summary>
		/// The color of the pen used to draw lines in this plot.
		/// </summary>
		public System.Drawing.Color BorderColor
		{
			set
			{
				if (borderPen_ != null)
				{
					borderPen_.Color = value;
				}
				else
				{
					borderPen_ = new Pen(value);
				}
			}
			get
			{
				return borderPen_.Color;
			}
		}


		/// <summary>
		/// Set/Get the fill brush
		/// </summary>
		public IRectangleBrush FillBrush
		{
			get
			{
				return rectangleBrush_;
			}
			set
			{
				rectangleBrush_ = value;
			}

		}
		private IRectangleBrush rectangleBrush_ = new RectangleBrushes.Solid( Color.LightGray );



		/// <summary>
		/// Write data associated with the plot as text.
		/// </summary>
		/// <param name="sb">the string builder to write to.</param>
		/// <param name="region">Only write out data in this region if onlyInRegion is true.</param>
		/// <param name="onlyInRegion">If true, only data in region is written, else all data is written.</param>
		/// <remarks>TODO: not implemented.</remarks>
		public void WriteData( System.Text.StringBuilder sb, RectangleD region, bool onlyInRegion )
		{
			sb.Append( "Write data not implemented yet for BarPlot\r\n" );
		}


	}

}
