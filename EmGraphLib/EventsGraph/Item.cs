using System;
using System.Collections.Generic;
using System.Text;

namespace EmGraphLib.DNS
{
	/// <summary>
	///  Dip or Over item class
	/// </summary>
	public class Item
	{
		#region Fields

		/// <summary>значение</summary>
		private float value = 0F;

		/// <summary>время начала</summary>
		private DateTime start;
		/// <summary>длительность</summary>
		private TimeSpan duration;
		/// <summary>фаза</summary>
		private Phase phase;
				
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="start">Deviation start datetime</param>
		/// <param name="duration">Duration of deviation</param>
		/// <param name="value">Deviation value</param>
		/// <param name="phase">Phase of event</param>
		public Item(DateTime start, TimeSpan duration, float value, Phase phase)
		{
			this.start = start;
			this.duration = duration;
			this.value = value;
			this.phase = phase;
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets value</summary>
		public float Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>Phase</summary>
		public Phase Phase
		{
			get { return phase; }
			set { phase = value; }
		}

		/// <summary>Gets or sets start datetime</summary>
		public DateTime Start
		{
			get { return this.start; }
			set { this.start = value; }
		}

		/// <summary>Gets end datetime</summary>
		public DateTime End
		{
			get 
			{
				DateTime end = new DateTime();
				try
				{
					end = this.start + this.duration;
				}
				catch { }
				return end; 
			}			
		}

		/// <summary>Gets or sets the duration of deviation</summary>
		public TimeSpan Duration
		{
			get { return duration; }
			set { duration = value; }
		}

		#endregion
	}
}
