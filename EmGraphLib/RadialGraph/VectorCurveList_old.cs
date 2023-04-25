using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EmGraphLib.Radial
{
	public class VectorCurveList
	{
		#region Fields

		/// <summary>Inner curves</summary>
		private List<VectorCurve> curves;

		/// <summary>Inner pointer to the panel</summary>
		private FlowLayoutPanel panel;

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
		public VectorCurveList(FlowLayoutPanel panel)
		{
			curves = new List<VectorCurve>();
			VectorCurve.NeedRedraw += new VectorCurve.NeedRedrawHandler(OnNeedRedraw);

			this.panel = panel;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets curve at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index</param>
		/// <returns><c>VectorCurve</c> object</returns>
		public VectorCurve this[int index]
		{
			get
			{
				try
				{
					return curves[index];
				}
				catch
				{
					return null;
				}
			}
			set
			{
				curves[index] = value;
			}
		}

		/// <summary>
		/// Gets the number of elements actually contained in the list 
		/// </summary>
		public int Count
		{
			get { return curves.Count; }
		}		

		#endregion

		#region Public methods

		/// <summary>
		/// Removes the list curve at the specified index.
		/// </summary>
		/// <param name="index">index of curve to remove from the list</param>
		/// <returns>True of all correct or false</returns>
		public bool Remove(int index)
		{
			try
			{
				for (int i=0; i<curves[index].LegendsCount; i++)
				{
					curves[index].RemoveLegend(index);
				}
				curves.RemoveAt(index);
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		/// <summary>
		/// Adds an <c>VectorCurve</c> item to the list
		/// </summary>
		/// <param name="modules">Array of modules values</param>
		/// <param name="angles">Array of angles values</param>
		/// <param name="color">Color of curve</param>
		public void Add(double[] modules, double[] angles, Color color)
		{
			int min = modules.Length < angles.Length ? modules.Length : angles.Length;
			VectorCurve newCurve = new VectorCurve(this.panel, color);
			for (int i = 0; i < min; i++)
			{
				VectorPair pair = new VectorPair(modules[i], angles[i]);
				newCurve.VectorPairList.Add(pair);
			}
			this.curves.Add(newCurve);
		}

		/// <summary>
		/// Removes all items from the list
		/// </summary>
		public void Clear()
		{

			for (int j = 0; j < curves.Count; j++)
			{
				int _legendsCount = curves[j].LegendsCount - 1;
				for (int i = _legendsCount; i >= 0; i--)
				{
					curves[j].RemoveLegend(i);
				}
			}
			curves.Clear();
		}

		/// <summary>
		/// Removes an element from a list
		/// </summary>
		/// <param name="item">The <paramref name="VectorCurve"/> to remove from the list</param>
		/// <returns></returns>
		public bool Remove(VectorCurve item)
		{
			for (int i = 0; i < item.LegendsCount; i++)
			{
				item.RemoveLegend(i);
			}			
			return curves.Remove(item);
		}
		#endregion

		#region Private methods

		/// <summary>Transforms to the points of grid</summary>
		/// <param name="pair"><c>VectorPair</c> object</param>
		/// <param name="zeroAngle">Andge witch we are assume as zero</param>
		/// <param name="ratio">Ratio to transform</param>
		/// <param name="nominal">Nominal value</param>
		/// <param name="p1">Base point 1</param>
		/// <param name="realValues">Real values or nominals flag</param>
		/// <param name="p2">Out point 2</param>
		public void _transform(VectorPair pair, double zeroAngle, double ratio, double nominal, Point p1, bool realValues, out Point p2)
		{
			double normalizedAngle = pair.Angle - zeroAngle;
			double radian_per_degree = Math.PI / 180;
			double pixModule;
			if (ratio == 0 && realValues) pixModule = 0;
			else pixModule = realValues ? pair.Module / ratio : nominal / ratio;
			double x = p1.X;
			double y = p1.Y;

			x += Math.Sin(normalizedAngle * radian_per_degree) * pixModule;
			y += -Math.Cos(normalizedAngle * radian_per_degree) * pixModule;
			p2 = new Point((int)x, (int)y);
		}

		/// <summary>Draw</summary>
		/// <param name="g"><c>Graphics</c> object</param>
		/// <param name="rect"><c>Rectangle</c> of grid</param>
		/// <param name="zeroAngle">Andge witch we are assume as zero</param>
		/// <param name="ratio">Scale ratio</param>
		/// <param name="nominal">Nominals value</param>
		/// <param name="style">Vector style</param>
		/// <param name="pair"><c>VectorPair</c> object</param>
		/// <param name="realValues">Real values or nominals flag</param>
		/// <param name="color">Color or curve</param>
		private void _drawPair(Graphics g, Rectangle rect, double zeroAngle, 
			double ratio, double nominal, VectorStyle style, VectorPair pair,
			bool realValues, Color color, ref List<Point> newCurve)
		{
			//p1 - точка в центре
			Point p1 = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
			Point p2 = Point.Empty;
			_transform(pair, zeroAngle, ratio, nominal, p1, realValues, out p2);
			Pen pen = new Pen(color, style.Thickness);
			pen.DashStyle = style.Style;
			if (p2.X != -2147483648 && p1.Y != -2147483648)
				g.DrawLine(pen, p1, p2);

			if(style.Thickness > 1)
			{
				newCurve.Add(p2);
			}
			pen.Dispose();
		}

		#endregion

		#region Internal methods

		/// <summary>Draw curves</summary>
		/// <param name="g"><c>Graphics</c> object</param>
		/// <param name="rect">Source rectangle</param>
		/// <param name="zeroAngle">Angle wich will assumed as zero</param>
		/// <param name="ratios">Grid scale ratios array</param>
		/// <param name="nominals">Grid nominals array</param>
		/// <param name="styles">Array of vector styles</param>
		/// <param name="realValues">Real values or nominals flag</param>
		internal void DrawCurves(Graphics g, Rectangle rect, double zeroAngle, double[] ratios, double[] nominals, VectorStyle[] styles, bool realValues)
		{
			if (curves.Count == 0) return;

			// в этом списке будем хранить координаты концов длинных прямых
			List<Point> newCurve = new List<Point>();

			foreach (VectorCurve curve in curves)
			{
				if (curve.VectorPairList.Count == 0) continue;

				for (int i = 0; i < curve.VectorPairList.Count; i++)
				{
					if (i < ratios.Length)
					{
						_drawPair(g, rect, zeroAngle, ratios[i], nominals[i], styles[i], curve.VectorPairList[i], realValues, curve.Color, ref newCurve);					
					}
					if (i < curve.LegendsCount && i < styles.Length)
					{
						curve.GetLegend(i).Color = curve.Color;
						curve.GetLegend(i).Style = styles[i].Style;
						curve.GetLegend(i).Thickness = styles[i].Thickness;

						curve.GetLegend(i).Label.Text = String.Format(curve.GetLegend(i).TextPattern, Math.Round(curve.VectorPairList[i].Module, 5), Math.Round(curve.VectorPairList[i].Angle - curve.GetLegend(i).ZeroAngle, 4));
					}
				}
			}
			/////////////
			// соединяем концы длинных прямых синей линией
			if (newCurve.Count > 1)
			{
				Pen pen = new Pen(Color.Blue, 2.0f);
				pen.DashStyle = DashStyle.Solid;
				if(newCurve[0] != newCurve[1])
					g.DrawLine(pen, newCurve[0], newCurve[1]);
				if (newCurve.Count > 2)
				{
					if (newCurve[1] != newCurve[2])
						g.DrawLine(pen, newCurve[1], newCurve[2]);
					if (newCurve[2] != newCurve[0])
						g.DrawLine(pen, newCurve[2], newCurve[0]);
				}
				pen.Dispose();
			}
			////////////
		}

		#endregion
	}
}
