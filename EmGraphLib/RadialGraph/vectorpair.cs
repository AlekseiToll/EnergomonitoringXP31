using System;
using System.Collections.Generic;
using System.Text;

namespace EmGraphLib.Radial
{
	/// <summary>
	/// Vector item
	/// </summary>
	public class VectorPair
	{
		#region Fields
		/// <summary>
		/// Inner module
		/// </summary>
		private double module;
		/// <summary>
		/// Inner angle
		/// </summary>
		private double angle;
		#endregion 
		
		#region Properties
		/// <summary>
		/// Module value
		/// </summary>
		public double Module
		{
			get
			{
				return module;
			}
			set
			{
				module = value;
				if (NeedRedraw != null) NeedRedraw();
			}
		}

		/// <summary>
		/// Angle in degrees
		/// </summary>
		public double Angle
		{
			get
			{
				return angle;
			}
			set
			{
				angle = value % 360;
				if (NeedRedraw != null) NeedRedraw();
			}
		}
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

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Module">Module of the vector</param>
		/// <param name="Angle">Angle of the vector</param>
		public VectorPair(double Module, double Angle)
		{
			this.module = Module;
			this.angle = Angle;
		}
		#endregion
	}
}
