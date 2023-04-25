using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EmGraphLib.Radial
{
	public class RadialGridList
	{
		#region Fields

		/// <summary>
		/// inner zero angle
		/// </summary>
		private double zeroAngle = 0;
		
		/// <summary>
		/// Gets or sets zero angle value in degrees from 12:00.
		/// Possible values from <c>0</c> to <c>359.(9)</c>
		/// </summary>
		public double ZeroAngle
		{
			get
			{
				return zeroAngle;
			}
			set
			{
				if (value >= 0 && value < 360)
					zeroAngle = value;
			}
		}

		/// <summary>
		/// inner grid list
		/// </summary>
		private List<RadialGrid> grids_;



		#endregion

		#region Events
		/// <summary>
		/// NeedRedraw event handler
		/// </summary>
		internal delegate void NeedRedrawHandler();
		/// <summary>
		/// Occures when one of properties have effect on drawing was changed
		/// </summary>
		internal static event NeedRedrawHandler NeedRedraw;

		/// <summary>
		/// Occures when one of grid in inner collection is need to be redrawn
		/// </summary>
		private void OnNeedRedraw()
		{
			if (NeedRedraw != null) NeedRedraw();
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public RadialGridList()
		{
			grids_ = new List<RadialGrid>();
			RadialGrid.NeedRedraw +=new RadialGrid.NeedRedrawHandler(OnNeedRedraw);			
		}

		#endregion

		#region Public methods
				
		public RadialGrid this[int index]
		{
			get
			{
				return grids_[index];
			}
			set
			{
				grids_[index] = value;
			}
		}

		// <summary>
		// Add new grid to the <c>RadialGraph</c> object
		// </summary>
		// <param name="item"><c>RadialGrid</c> object to be added</param>
		/*public void Add(RadialGrid item)
		{
			grids.Add(item);
		}*/

		// <summary>
		// Add new grid to the <c>RadialGraph</c> object
		// </summary>
		// <param name="nominalValue">Nominal value</param>
		// <param name="part">Part of this grid from whole grid space</param>
		/*public void Add(double nominalValue, double part)
		{
			RadialGrid newGrid = new RadialGrid(nominalValue, part);
			grids.Add(newGrid);
		}*/

		// <summary>
		// Add new grid to the <c>RadialGraph</c> object
		// </summary>
		// <param name="nominalValue">Nominal value</param>
		// <param name="part">Part of this grid from whole grid space</param>
		// <param name="foreColor">Color of grid lines</param>
		/*public void Add(double nominalValue, double part, Color foreColor)
		{
			RadialGrid newGrid = new RadialGrid(nominalValue, part, foreColor);
			grids.Add(newGrid);
		}*/

		/// <summary>
		/// Add new grid to the <c>RadialGraph</c> object
		/// </summary>
		/// <param name="nominalValue">Nominal value</param>
		/// <param name="part">Part of this grid from whole grid space</param>
		/// <param name="thickness">Grid line thickness</param>
		/// <param name="style">Grid line style</param>
		public void Add(double nominalValue, double part, int thickness, DashStyle style)
		{
			RadialGrid newGrid = new RadialGrid(nominalValue, part, thickness, style);
			grids_.Add(newGrid);
		}

		// <summary>
		// Add new grid to the <c>RadialGraph</c> object
		// </summary>
		// <param name="nominalValue">Nominal value</param>
		// <param name="part">Part of this grid from whole grid space</param>
		// <param name="thickness">Grid line thickness</param>
		// <param name="style">Grid line style</param>
		// <param name="foreColor">Color of grid lines</param>
		/*public void Add(double nominalValue, double part, int thickness, DashStyle style, Color color)
		{
			RadialGrid newGrid = new RadialGrid(nominalValue, part, thickness, style, color);
			grids.Add(newGrid);
		}*/

		/// <summary>
		/// Remove specified<c>RadialGrid</c> from <c>RadialGraph</c> object
		/// </summary>
		/// <param name="item"><c>RadialGrid</c>object to be removed</param>
		/// <returns><c>True</c> if all correct or <c>False</c></returns>
		public bool Remove(RadialGrid item)
		{
			return grids_.Remove(item);
		}

		/// <summary>
		/// Remove <c>RadialGrid</c> with specified index from <c>RadialGraph</c> object
		/// </summary>
		/// <param name="itemIndex">Index of <c>RadialGrid</c>object to be removed</param>
		/// <returns><c>True</c> if all correct or <c>False</c></returns>
		public bool Remove(int itemIndex)
		{
			try
			{
				grids_.RemoveAt(itemIndex);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Delete all <c>RadialGrid</c> objects from <c>RadialGraph</c> object
		/// </summary>
		public void Clear()
		{
			grids_.Clear();
		}

		/// <summary>
		/// Gets count of <c>RadialGrid</c> objects in <c>RadialGraph</c>
		/// </summary>
		public int Count
		{
			get { return grids_.Count; }
		}
		
		#endregion

		#region Private methods

		/// <summary>
		/// Gets array of <c>Rectangle</c> objects
		/// </summary>
		/// <param name="mainRect">Source (maximum) rectangle</param>
		/// <param name="grids"><typeparamref name="grids"/> object</param>
		/// <param name="curveList">VectorCurveList object</param>
		/// <param name="realValues">Real values or nominals flag</param>
		/// <returns>Array of <c>Rectangle</c> objects</returns>
		private Rectangle[] _getGridNominalRects(Rectangle mainRect, List<RadialGrid> grids, VectorCurveList curveList, bool realValues)
		{
			// creating data array
			double[][] data = _getValueArray(curveList);

			// creating rects array
			Rectangle[] rects = new Rectangle[grids.Count];
			
			// filling rects array
			for (int i = 0; i < grids.Count; i++)
			{
				rects[i] = RadialGraph._scaleRect(mainRect, grids[i].Scale);
				if (realValues)
				{
					rects[i] = _getGridNominalRect(rects[i], data[i], grids[i].NominalValue);
				}
			}
			return rects;
		}

		/// <summary>
		/// Gets nominal circle rectangle
		/// </summary>
		/// <param name="maxRect">Maximum rectangle</param>
		/// <param name="values">Values to calculate scale</param>
		/// <param name="Nominal">Nominal value to calculate scale</param>
		/// <returns>Nominal rectangle</returns>
		private Rectangle _getGridNominalRect(Rectangle maxRect, double[] values, double Nominal)
		{
			// searching for max module value
			double maxModule = 0;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] > maxModule)
				{
					maxModule = values[i];
				}
			}

			if (maxModule == 0 || maxModule <= Nominal) return maxRect;

			// значение изменения (уменьшения) полустороны
			// на самом деле это значение на которую надо укоротить 
			// исходный квадрат с каждой стороны
			float dSide = (float)(maxRect.Width - (Nominal / maxModule) * maxRect.Width) / 2;

			return Rectangle.Round(
				new RectangleF(
					maxRect.X + dSide,
					maxRect.Y + dSide,
					maxRect.Width - dSide * 2,
					maxRect.Height - dSide * 2
				)
			);
		}

		/// <summary>
		/// Convert <paramref name="curveList"/> to the two dimentional array of double 
		/// </summary>
		/// <param name="curveList"><c>VectorCurveList</c> object to be converted</param>
		/// <returns>Two dimentional array of double</returns>
		private double[][] _getValueArray(VectorCurveList curveList)
		{
			// finding max curve capacity
			int maxCurveSize = 0;
			for (int i = 0; i < curveList.Count; i++)
			{
				if (curveList[i].VectorPairList.Count > maxCurveSize)
					maxCurveSize = curveList[i].VectorPairList.Count;
			}

			// creating list of arrays with this capacity
			double[][] data = new double[maxCurveSize][];
			for (int i = 0; i < maxCurveSize; i++)
			{
				data[i] = new double[curveList.Count];
			}

			// filling arrays
			for (int j = 0; j < maxCurveSize; j++)
			{
				for (int i = 0; i < curveList.Count; i++)
				{
					// проверка чтобы не вывалиться за границы массива
					if (j < curveList[i].VectorPairList.Count)
					{
						data[j][i] = curveList[i].VectorPairList[j].Module;
					}
				}
			}
			return data;
		}

		/// <summary>
		/// Gets all grids ratios
		/// </summary>
		/// <param name="grids">List of RadialGrids</param>
		/// <param name="rects">Array of rects</param>
		/// <returns>Array of gids ratios</returns>
		private double[] _getRatios(List<RadialGrid> grids, Rectangle[] rects)
		{
			double[] ratios = new double[grids.Count];
			for (int i = 0; i < grids.Count; i++)
			{
				if (!rects[i].IsEmpty)
				{
					ratios[i] = grids[i].NominalValue / (rects[i].Width / 2);
				}
			}
			return ratios;
		}

		/// <summary>
		/// Gets all grids nominals
		/// </summary>
		/// <param name="grids">List of RadialGrids</param>
		/// <returns>Array of gids nominals</returns>
		private double[] _getNominals(List<RadialGrid> grids)
		{
			double[] nominals = new double[grids.Count];
			for (int i = 0; i < grids.Count; i++)
			{
				nominals[i] = grids[i].NominalValue;
			}
			return nominals;
		}

		/// <summary>
		/// Gets all vector styles (from grid line styles)
		/// </summary>
		/// <param name="grids">List of RadialGrids</param>
		/// <returns>Array of vector styles</returns>
		private VectorStyle[] _getVectorStyles(List<RadialGrid> grids)
		{
			VectorStyle[] styles = new VectorStyle[grids.Count];
			for (int i = 0; i < grids.Count; i++)
			{
				styles[i].Style = grids[i].Style;
				styles[i].Thickness = grids[i].Thickness;
			}
			return styles;
		}

		#endregion

		#region Internal methods

		/// <summary>Draw grid circles</summary>
		/// <param name="g"><c>Graphics</c> object</param>
		/// <param name="rect"><c>RadialGrid</c> rectangle</param>
		/// <param name="curveList">Curve list to calculate scale of grids</param>
		/// <param name="realValues">Real values or nominals flag</param>
		/// <param name="ratios">Array of ratios to scale curves</param>
		/// <param name="nominals">Array of nominals to draw curves in nominals mode</param>		
		/// <param name="styles">Array of styles of each vector in any curve</param>
		internal void Draw(Graphics g, Rectangle rect, VectorCurveList curveList, bool realValues, out double[] ratios, out double[] nominals, out VectorStyle[] styles)
		{
			// calculating all grids rectangles
			Rectangle[] rects = _getGridNominalRects(rect, grids_, curveList, realValues);

			// drawing each grid
			for (int i = 0; i < grids_.Count; i++)
			{
				grids_[i].DrawNominal(g, rects[i]);
			}

			// creating and customizing Pen object
			Pen _linePen = new Pen(Color.Gray);
			_linePen.DashStyle = DashStyle.Dot;

			// drawing vertical and horizontal lines
			g.DrawLine(_linePen, rect.Left + rect.Width / 2, rect.Top, rect.Left + rect.Width / 2, rect.Bottom);
			g.DrawLine(_linePen, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2);

			// disposing Pen object
			_linePen.Dispose();

			// return ratios
			ratios = _getRatios(grids_, rects);
			// return nominals
			nominals = _getNominals(grids_);
			// return styles
			styles = _getVectorStyles(grids_);
		}
		#endregion
	}
}
