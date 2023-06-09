/*
NPlot - A charting library for .NET

StepPlot.cs
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
	/// Encapsulates functionality for plotting data as a stepped line.
	/// </summary>
	public class StepPlot : BaseSequencePlot, IPlot, ISequencePlot
	{

		/// <summary>
		/// Constructor.
		/// </summary>
		public StepPlot()
		{
			this.Center = false;
		}


		/// <summary>
		/// Draws the step plot on a GDI+ surface against the provided x and y axes.
		/// </summary>
		/// <param name="g">The GDI+ surface on which to draw.</param>
		/// <param name="xAxis">The X-Axis to draw against.</param>
		/// <param name="yAxis">The Y-Axis to draw against.</param>
		public virtual void Draw( Graphics g, PhysicalAxis xAxis, PhysicalAxis yAxis )
		{

			SequenceAdapter data = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateData, this.AbscissaData );

			for (int i=0; i<data.Count; ++i)
			{
				PointD p1 = data[i];
				if (Double.IsNaN(p1.X) || Double.IsNaN(p1.Y))
				{
					continue;
				}

				PointD p2;
				PointD p3;
				if (i+1 != data.Count)
				{
					p2 = data[i+1];
					if (Double.IsNaN(p2.X) || Double.IsNaN(p2.Y))
					{
						continue;
					}
					p2.Y = p1.Y;
					p3 = data[i+1];
				}
				else
				{
					p2 = data[i-1];
					double offset = p1.X - p2.X;
					p2.X = p1.X + offset;
					p2.Y = p1.Y;
					p3 = p2; 
				}

				if ( this.center_ )
				{
					double offset = ( p2.X - p1.X ) / 2.0f;
					p1.X -= offset;
					p2.X -= offset;
					p3.X -= offset;
				}

				PointF xPos1 = xAxis.WorldToPhysical( p1.X, false );
				PointF yPos1 = yAxis.WorldToPhysical( p1.Y, false );
				PointF xPos2 = xAxis.WorldToPhysical( p2.X, false );
				PointF yPos2 = yAxis.WorldToPhysical( p2.Y, false );
				PointF xPos3 = xAxis.WorldToPhysical( p3.X, false );
				PointF yPos3 = yAxis.WorldToPhysical( p3.Y, false );

				if (!this.hideHorizontalSegments_)
				{
					if (scale_ != 1.0f)
					{
						float middle = (xPos2.X + xPos1.X) / 2.0f;
						float width = xPos2.X - xPos1.X;
						width *= this.scale_;
						g.DrawLine( Pen, (int)(middle-width/2.0f), yPos1.Y, (int)(middle+width/2.0f), yPos2.Y );
					}
					else
					{
						g.DrawLine( Pen, xPos1.X, yPos1.Y, xPos2.X, yPos2.Y );
					}
				}
				
				if (!this.hideVerticalSegments_)
				{
					g.DrawLine( Pen, xPos2.X, yPos2.Y, xPos3.X, yPos3.Y );
				}

			}

		}


		/// <summary>
		/// Returns an X-axis suitable for use by this plot. The axis will be one that is just long
		/// enough to show all data.
		/// </summary>
		/// <returns>X-axis suitable for use by this plot.</returns>
		public Axis SuggestXAxis()
		{
			SequenceAdapter data = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateData, this.AbscissaData );

			if (data.Count < 2)
			{
				return data.SuggestXAxis();
			}

			// else

			Axis a = data.SuggestXAxis();

			PointD p1 = data[0];
			PointD p2 = data[1];
			PointD p3 = data[data.Count-2];
			PointD p4 = data[data.Count-1];

			double offset1;
			double offset2;

			if (!center_)
			{
				offset1 = 0.0f;
				offset2 = p4.X - p3.X;
			}
			else
			{
				offset1 = (p2.X - p1.X)/2.0f;
				offset2 = (p4.X - p3.X)/2.0f;
			}

			a.WorldMin -= offset1;
			a.WorldMax += offset2;

			return a;
		}


		/// <summary>
		/// Returns an Y-axis suitable for use by this plot. The axis will be one that is just long
		/// enough to show all data.
		/// </summary>
		/// <returns>Y-axis suitable for use by this plot.</returns>
		public Axis SuggestYAxis()
		{
			SequenceAdapter data = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateData, this.AbscissaData );

			return data.SuggestYAxis();
		}


		/// <summary>
		/// Gets or sets whether or not steps should be centered. If true, steps will be centered on the
		/// X abscissa values. If false, the step corresponding to a given x-value will be drawn between 
		/// this x-value and the next x-value at the current y-height.
		/// </summary>
		public bool Center
		{
			set
			{
				center_ = value;
			}
			get
			{
				return center_;
			}
		}
		private bool center_;


        /// <summary>
        /// Draws a representation of this plot in the legend.
        /// </summary>
        /// <param name="g">The graphics surface on which to draw.</param>
        /// <param name="startEnd">A rectangle specifying the bounds of the area in the legend set aside for drawing.</param>
        public virtual void DrawInLegend(Graphics g, Rectangle startEnd)
        {
            g.DrawLine(pen_, startEnd.Left, (startEnd.Top + startEnd.Bottom) / 2,
                startEnd.Right, (startEnd.Top + startEnd.Bottom) / 2);
        }


        /// <summary>
        /// The pen used to draw the plot
        /// </summary>
        public System.Drawing.Pen Pen
        {
            get
            {
                return pen_;
            }
            set
            {
                pen_ = value;
            }
        }
        private System.Drawing.Pen pen_ = new Pen(Color.Black);


        /// <summary>
        /// The color of the pen used to draw lines in this plot.
        /// </summary>
        public System.Drawing.Color Color
        {
            set
            {
                if (pen_ != null)
                {
                    pen_.Color = value;
                }
                else
                {
                    pen_ = new Pen(value);
                }
            }
            get
            {
                return pen_.Color;
            }
        }


		/// <summary>
		/// If true, then vertical lines are hidden.
		/// </summary>
		public bool HideVerticalSegments
		{
			get
			{
				return hideVerticalSegments_;
			}
			set
			{
				hideVerticalSegments_ = value;
			}
		}
		bool hideVerticalSegments_ = false;


		/// <summary>
		/// If true, then vertical lines are hidden.
		/// </summary>
		public bool HideHorizontalSegments
		{
			get
			{
				return hideHorizontalSegments_;
			}
			set
			{
				hideHorizontalSegments_ = value;
			}
		}
		bool hideHorizontalSegments_ = false;


		/// <summary>
		/// The horizontal line length is multiplied by this amount. Default
		/// corresponds to a value of 1.0.
		/// </summary>
		public float WidthScale
		{
			get
			{
				return scale_;
			}
			set
			{
				scale_ = value;
			}
		}
		private float scale_ = 1.0f;

    }
}
