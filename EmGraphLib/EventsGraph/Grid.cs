using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EmGraphLib.DNS
{
	/// <summary>
	/// Dips and overs grid class
	/// </summary>
	public class Grid
	{
		#region Fields
		
		/// <summary>Inner nominal line</summary>
		private GridLine nominalLine = new GridLine();

		/// <summary>Inner npl line</summary>
		private GridLine limitLine = new GridLine();

		/// <summary>Inner time line</summary>
		private GridTimeline timeLine = new GridTimeline();

		/// <summary>Inner visible flag</summary>
		private bool visible = true;

		/// <summary>Inner tag</summary>
		private object tag = null;

		/// <summary>Inner nominal value</summary>
		private float nominal = 0F;

		/// <summary>Inner normal permissible limit</summary>
		private float limit = 0F;

		#endregion

		#region Properties

		/// <summary>Gets or sets nominal grid line</summary>
		public GridLine NominalLine
		{
			get { return nominalLine; }
			set { nominalLine = value;  }
		}

		/// <summary>Gets or sets limit grid line</summary>
		public GridLine LimitLine
		{
			get { return limitLine; }
			set { limitLine = value; }
		}

		/// <summary>Gets or sets grid time line</summary>
		public GridTimeline TimeLine
		{
			get { return timeLine; }
			set { timeLine = value; }
		}

		/// <summary>Gets or sets is grid visible or not</summary>
		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}

		/// <summary>Gets or sets reserved for user object</summary>
		public object Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		/// <summary>Gets or sets nominal value</summary>
		public float NominalValue
		{
			get { return nominal; }
			set { nominal = value; }
		}

		/// <summary>Gets or sets normal permissible limit</summary>
		public float LimitValue
		{
			get { return limit; }
			set { limit = value; }
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Draw grid lines
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <param name="rect">Destination rectangle</param>
		/// <param name="minVal">Minimun value</param>
		/// <param name="maxVal">Maximum value</param>
		/// <param name="spasing">Spacing value</param>
		/// <param name="leftFieldWidth">Left field width</param>
		/// <param name="visibleTickStart">Start datetime ticks(for timeline)</param>
		/// <param name="visibleTickEnd">End datetime ticks(for timeline)</param>
		internal void Draw(Graphics g, Rectangle rect, float minVal, float maxVal, int spacing, int leftFieldWidth, double visibleTickStart, double visibleTickEnd)
		{
			#region Drawing timeline
			
			if (timeLine.Visible)
			{
				Rectangle timelineRect = DNOGraph.SubtractRect(rect, leftFieldWidth, 0, 0, 0);

				float dHeight = 0;
				timeLine.Draw(g,
					timelineRect,
					new DateTime((long)visibleTickStart),
					new DateTime((long)visibleTickEnd), 
					spacing, out dHeight);

				// changing rectangle's height value
				rect.Height -= (int)dHeight;
			}

			#endregion

			#region Drawing nominal and limits lines

			// calculating helpfull values
			float ratio = (maxVal == minVal) ? 0.5F : (maxVal / (maxVal - minVal));
			float yNom = rect.Top + ratio * rect.Height;
			float dLimit = this.limit / (maxVal - minVal) * rect.Height;
			
			if (nominalLine.Visible)
			{				
				// drawing nominal line	
				nominalLine.Draw(g, rect.Left, rect.Left + leftFieldWidth, yNom, rect.Width, nominal, true);
			}
			
			if (limitLine.Visible)
			{
				// drawing limit lines
				limitLine.Draw(g, rect.Left, rect.Left + leftFieldWidth, yNom - dLimit, rect.Width, 1 + limit, false);
				limitLine.Draw(g, rect.Left, rect.Left + leftFieldWidth, yNom + dLimit, rect.Width, 1 - limit, false);
			}

			#endregion
		}

		#endregion

		#region Private methods
		
	

		#endregion
	}
}
