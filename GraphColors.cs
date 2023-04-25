	using System;
using System.Drawing;

namespace EnergomonitoringXP.Graph
{
	/// <summary>
	/// Class to manage colors array for zedGraph
	/// </summary>
	public class GraphColors
	{
		private const int _number = 8;

		private Color[] _colors = new Color[_number] {
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.OrangeRed,
			Color.BlueViolet, 
			Color.DeepPink,
			Color.Brown,
			Color.DarkCyan
		};
		private bool[] _colorsBinded = new bool[_number];		
		private int _bindedCount = 0;

		/// <summary>
		/// Constructor
		/// </summary>
		public GraphColors(){}

		/// <summary>
		/// Binds first free color in inner array and returns it
		/// </summary>
		/// <returns>Free color or Color.Empty</returns>
		public Color BindColor()
		{
			if (_bindedCount < _number)
			{
				for (int i = 0; i < _colorsBinded.Length; i++)
				{
					if (_colorsBinded[i] == false)
					{
						_colorsBinded[i] = true;
						_bindedCount++;
						return _colors[i];
					}
				}
			}
			return Color.Empty;
		}
		
		/// <summary>
		/// Release color in inner array.
		/// </summary>
		/// <param name="FreeColor">Color to be released</param>
		/// <returns>True if color has been released or False</returns>
		public bool ReleaseColor(Color FreeColor)
		{
			for (int i = 0; i < _number; i++)
			{
				if (_colors[i] == FreeColor)
				{
					_colorsBinded[i] = false;
					_bindedCount--;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Release all colors
		/// </summary>
		public void ReleaseColors()
		{
			for (int i = 0; i < _number; i++)
			{
				_colorsBinded[i] = false;
			}
			_bindedCount = 0;
		}

		/// <summary>
		/// Gets number of binded colors
		/// </summary>
		public int BindedColorsCount
		{
			get
			{
				return _bindedCount;
			}
		}

		/// <summary>
		/// Gets numbers of all colors
		/// </summary>
		public int AllColorsCount
		{
			get
			{
				return _number;
			}
		}
	}
}