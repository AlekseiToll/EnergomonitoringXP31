using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace EmGraphLib.Radial
{
	public class LegendList
	{
		#region Fields

		private List<Legend> legends = null;
		private FlowLayoutPanel panel = null;
		private Color color;

		#endregion

		#region Events
		/// <summary>
		/// NeedRedraw	event handler
		/// </summary>
		internal delegate void NeedRedrawHandler();
		/// <summary>
		/// Occures when one of properties have effect on drawing was changed
		/// </summary>
		internal static event NeedRedrawHandler NeedRedraw;
		#endregion

		#region Properties

		public Color Color
		{
			get { return color; }
			set 
			{ 
				color = value;
				for (int i = 0; i < legends.Count; i++)
				{
					legends[i].Label.BackColor = color;
					// NeedRedraw
				}
			}
		}

		public Legend this[int index]
		{
			get { return legends[index]; }
		}

		public int Count
		{
			get { return legends.Count; }
		}

		#endregion

		#region Constructors

		public LegendList(FlowLayoutPanel panel)
		{
			this.legends = new List<Legend>();
			this.panel = panel;
		}

		#endregion

		#region Internal methods

		internal void Add(Legend item)
		{
			legends.Add(item);
			panel.Controls.Add(item.Label);

			if (NeedRedraw != null) NeedRedraw();
		}

		internal void Clear()
		{
			for (int i = 0; i < legends.Count; i++)
			{
				this.legends[i].Label.Dispose();
			}
			legends.Clear();
			
			if (NeedRedraw != null) NeedRedraw();
		}

		internal bool Remove(int index)
		{	
			try
			{
				this.legends[index].Label.Dispose();
				legends.RemoveAt(index);
				if (NeedRedraw != null) NeedRedraw();
				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion
	}
}
