using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace EmGraphLib.Radial
{
	/// <summary>
	/// Group of vectors vector in one grid
	/// </summary>
	public class VectorCurve
	{
		#region Fields

		/// <summary>
		/// Inner pair list
		/// </summary>
		private List<VectorPair> pairs = null;
		/// <summary>
		/// Inner color
		/// </summary>
		private Color color = Color.Empty;
		/// <summary>
		/// Inner legend list object
		/// </summary>
		private LegendList legends;
        ///// <summary>
        ///// Inner pointer to the panel
        ///// </summary>
        //private FlowLayoutPanel panel;

		#endregion

		#region Properties

		/// <summary>
		/// Gets vector pair list
		/// </summary>
		public List<VectorPair> VectorPairList
		{
			get { return pairs;	}
		}

		/// <summary>
		/// Gets or sets color of the curve
		/// </summary>
		public Color Color
		{
			get { return this.color; }
			set	
			{ 
				this.color = value;
				if (NeedRedraw != null) NeedRedraw();
			}
		}

		/// <summary>
		/// Gets legends count
		/// </summary>
		public int LegendsCount
		{
			get { return legends.Count; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="color">Color of the curve</param>
		/// <param name="panel"><c>FlowLayoutPanel</c> object</param>
		internal VectorCurve(FlowLayoutPanel panel, Color color)
		{
			this.color = color;
			pairs = new List<VectorPair>();
			VectorPair.NeedRedraw += new VectorPair.NeedRedrawHandler(OnNeedRedraw);
			legends = new LegendList(panel);
			LegendList.NeedRedraw += new LegendList.NeedRedrawHandler(OnNeedRedraw);
		}
		#endregion

		#region Public methods

		public void AddLegend(string TextPattern)
		{
			Legend newLegend = new Legend();
			newLegend.TextPattern = TextPattern;
			legends.Add(newLegend);
		}

		public void RemoveLegend(int index)
		{
			legends.Remove(index);
		}

		public Legend GetLegend(int index)
		{
			if (index < legends.Count)
				return legends[index];
			else
				return null;
		}
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

		void OnNeedRedraw()
		{
			if (NeedRedraw != null) NeedRedraw();
		}
		#endregion
	}
}
