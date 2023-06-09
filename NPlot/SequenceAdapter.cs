/*
NPlot - A charting library for .NET

SequenceAdapter.cs
Copyright (C) 2003-2004
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
using System.Collections;
using System.Data;

namespace NPlot
{

	/// <summary>
	/// This class is responsible for interpreting the various ways you can 
	/// specify data to plot objects using the DataSource, DataMember, ordinateData
	/// and AbscissaData properties. It is a bridge that provides access to this
	/// data via a single interface.
	/// </summary>
	public class SequenceAdapter
	{
		private AdapterUtils.IAxisSuggester XAxisSuggester_;
		private AdapterUtils.IAxisSuggester YAxisSuggester_;
        private AdapterUtils.ICounter counter_;
        private AdapterUtils.IDataGetter xDataGetter_;
        private AdapterUtils.IDataGetter yDataGetter_;

        /// <summary>
		/// Constructor. The data source specifiers must be specified here.
		/// </summary>
		/// <param name="dataSource">The source containing a list of values to plot.</param>
		/// <param name="dataMember">The specific data member in a multimember data source to get data from.</param>
		/// <param name="ordinateData">The source containing a list of values to plot on the ordinate axis, or a the name of the column to use for this data.</param>
		/// <param name="abscissaData">The source containing a list of values to plot on the abscissa axis, or a the name of the column to use for this data.</param>
		public SequenceAdapter( object dataSource, string dataMember, object ordinateData, object abscissaData )
		{

			if (dataSource == null && dataMember == null)
			{
				if (ordinateData is IList) 
				{

                    this.YAxisSuggester_ = new AdapterUtils.AxisSuggester_IList((IList)ordinateData);
                    this.yDataGetter_ = new AdapterUtils.DataGetter_IList((IList)ordinateData);
                    this.counter_ = new AdapterUtils.Counter_IList((IList)ordinateData);

                    if (abscissaData is IList)
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_IList((IList)abscissaData);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_IList((IList)abscissaData);
                        return;
					}

					else if (abscissaData is StartStep)
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_StartStep((StartStep)abscissaData, (IList)ordinateData);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_StartStep((StartStep)abscissaData);
                        return;
					}

					else if (abscissaData == null)
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_Auto((IList)ordinateData);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_Count();
                        return;
					}
				}
				

				else if (ordinateData == null)
				{
					if (abscissaData == null)
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_Null();
                        this.YAxisSuggester_ = new AdapterUtils.AxisSuggester_Null();
                        this.counter_ = new AdapterUtils.Counter_Null();
                        this.xDataGetter_ = new AdapterUtils.DataGetter_Null();
                        this.yDataGetter_ = new AdapterUtils.DataGetter_Null();
                        return;
					}
					else if (abscissaData is IList)
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_IList((IList)abscissaData);
                        this.YAxisSuggester_ = new AdapterUtils.AxisSuggester_Auto((IList)abscissaData);
                        this.counter_ = new AdapterUtils.Counter_IList((IList)abscissaData);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_IList((IList)abscissaData);
                        this.yDataGetter_ = new AdapterUtils.DataGetter_Count();
                        return;
					}
					
					else
					{
						// unknown.
					}

				}
				else
				{
					// unknown
				}

			}

			else if (dataSource is IList && dataMember == null)
			{
				if (dataSource is DataView)
				{
					DataView data = (DataView)dataSource;

                    this.counter_ = new AdapterUtils.Counter_DataView(data);
                    this.xDataGetter_ = new AdapterUtils.DataGetter_DataView(data, (string)abscissaData);
                    this.yDataGetter_ = new AdapterUtils.DataGetter_DataView(data, (string)ordinateData);
                    this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_DataView(data, (string)abscissaData);
                    this.YAxisSuggester_ = new AdapterUtils.AxisSuggester_DataView(data, (string)ordinateData);
                    return;
				}

				else
				{
                    this.YAxisSuggester_ = new AdapterUtils.AxisSuggester_IList((IList)dataSource);
                    this.counter_ = new AdapterUtils.Counter_IList((IList)dataSource);
                    this.yDataGetter_ = new AdapterUtils.DataGetter_IList((IList)dataSource);

                    if ((ordinateData == null) && (abscissaData == null))
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_Auto((IList)dataSource);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_Count();
                        return;
					}

					else if ((ordinateData == null) && (abscissaData is StartStep))
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_StartStep((StartStep)abscissaData, (IList)ordinateData);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_StartStep((StartStep)abscissaData);
                        return;
					}

					else if ((ordinateData == null) && (abscissaData is IList))
					{
                        this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_IList((IList)abscissaData);
                        this.xDataGetter_ = new AdapterUtils.DataGetter_IList((IList)abscissaData);
                        return;
					}

					else
					{
						// unknown.
					}
				}
			}

			else if ( ((dataSource is DataTable) && (dataMember == null)) || (dataSource is DataSet) )
			{
				DataRowCollection rows = null;

				if (dataSource is DataSet)
				{
					if (dataMember != null)
					{
						rows = ((DataTable)((DataSet)dataSource).Tables[dataMember]).Rows;
					}
					else
					{
						rows = ((DataTable)((DataSet)dataSource).Tables[0]).Rows;
					}
				}
				else
				{
					rows = ((DataTable)dataSource).Rows;
				}

                this.yDataGetter_ = new AdapterUtils.DataGetter_Rows(rows, (string)ordinateData);
                this.YAxisSuggester_ = new AdapterUtils.AxisSuggester_Rows(rows, (string)ordinateData);
                this.counter_ = new AdapterUtils.Counter_Rows(rows);

                if ((abscissaData is string) && (ordinateData is string))
				{
                    this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_Rows(rows, (string)abscissaData);
                    this.xDataGetter_ = new AdapterUtils.DataGetter_Rows(rows, (string)abscissaData);
                    return;
				}
				else if ((abscissaData == null) && (ordinateData is string))
				{
                    this.XAxisSuggester_ = new AdapterUtils.AxisSuggester_RowAuto(rows);
                    this.xDataGetter_ = new AdapterUtils.DataGetter_Count();
                    return;
				}
				else
				{
					// unknown.
				}
			}

			else
			{
				// unknown.
			}

			throw new NPlotException( "Do not know how to interpret data provided to chart." );

		}


		/// <summary>
		/// Returns the number of points.
		/// </summary>
		public int Count
		{
			get
			{
				return counter_.Count;
			}
		}


		/// <summary>
		/// Returns the ith point.
		/// </summary>
		public PointD this[int i] 
		{
			get
			{
				return new PointD( this.xDataGetter_.Get(i), this.yDataGetter_.Get(i) );
			}
		}


		/// <summary>
		/// Returns an x-axis that is suitable for drawing the data.
		/// </summary>
		/// <returns>A suitable x-axis.</returns>
		public Axis SuggestXAxis()
		{
			return this.XAxisSuggester_.Get();
		}


		/// <summary>
		/// Returns a y-axis that is suitable for drawing the data.
		/// </summary>
		/// <returns>A suitable y-axis.</returns>
		public Axis SuggestYAxis()
		{
			Axis a = this.YAxisSuggester_.Get();
			// TODO make 0.08 a parameter.
			a.IncreaseRange( 0.08 );
			return a;
		}


		/// <summary>
		/// Writes data out as text. 
		/// </summary>
		/// <param name="sb">StringBuilder to write to.</param>
		/// <param name="region">Only write out data in this region if onlyInRegion is true.</param>
		/// <param name="onlyInRegion">If true, only data in region is written, else all data is written.</param>
		public void WriteData( System.Text.StringBuilder sb, RectangleD region, bool onlyInRegion )
		{
			for (int i=0; i<this.Count;	++i)
			{
				if ( !(onlyInRegion && 
					   (this[i].X >= region.X && this[i].X <= region.X+region.Width) &&
					   (this[i].Y >= region.Y && this[i].Y <= region.Y+region.Height)) )
					continue;

				sb.Append( this[i].ToString() );
				sb.Append( "\r\n" );
			}
		}


	}
}
