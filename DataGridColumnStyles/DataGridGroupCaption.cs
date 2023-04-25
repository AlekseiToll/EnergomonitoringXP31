/// (c) Mars-Energo Ltd.
/// author : Andrew A. Golyakov 
/// 
/// DataGridGroupCaption Class describes visible headers goups

using System;
using System.Drawing;

namespace DataGridColumnStyles
{
	/// <summary>
	/// Group caption class
	/// </summary>			
	public class DataGridGroupCaption
	{
		/// <summary>
		/// Text to be displayed in the group header
		/// </summary>
		public string Text;

		/// <summary>
		/// Number of columns in the group
		/// </summary>
		public int Colspan;
		
		/// <summary>
		/// Text font
		/// </summary>
		public Font TextFont;
		
		/// <summary>
		/// Border <c>Pen</c> object
		/// </summary>
		public readonly Pen penBorder;

		/// <summary>
		/// Group caption background brush
		/// </summary>
		public readonly SolidBrush backBrush;

		/// <summary>
		/// Constructor with only <c>Colspan</c> parameter
		/// </summary>
		/// <param name="Colspan">Number of columns in the group</param>
		public DataGridGroupCaption(int Colspan)
		{
			this.Text = String.Empty;
			this.Colspan = Colspan;
			
			this.TextFont = new Font(FontFamily.GenericSansSerif, (float)8.25);
			this.penBorder = new Pen(Color.FromKnownColor(KnownColor.ControlDarkDark), 1);
			this.backBrush = new SolidBrush(Color.Silver);
		}		
      
		/// <summary>
		/// Constructor with <c>Text</c> and <c>Colspan</c> parameters
		/// </summary>
		/// <param name="Text">Text to be displayed in the group header</param>
		/// <param name="Colspan">Number of columns in the group</param>
		public DataGridGroupCaption(string Text, int Colspan)
		{
			this.Text = Text;
			this.Colspan = Colspan;

			this.TextFont = new Font(FontFamily.GenericSansSerif, (float)8.25);
			this.penBorder = new Pen(Color.FromKnownColor(KnownColor.ControlDarkDark), 1);
			this.backBrush = new SolidBrush(Color.Silver);
		}

		/// <summary>
		/// Constructor with <c>Text</c>, <c>Colspan</c> and <c>BackColor</c> parameters
		/// </summary>
		/// <param name="Text">Text to be displayed in the group header</param>
		/// <param name="Colspan">Number of columns in the group</param>
		/// <param name="BackColor">Group caption background color</param>
		public DataGridGroupCaption(string Text, int Colspan, Color BackColor)
		{
			this.Text = Text;			
			this.Colspan = Colspan;
			
			this.TextFont = new Font(FontFamily.GenericSansSerif, (float)8.25);
			this.penBorder = new Pen(Color.FromKnownColor(KnownColor.ControlDarkDark), 1);			
			backBrush = new SolidBrush(BackColor);
		}

		/// <summary>
		/// Gets or sets group caption background color
		/// </summary>
		public Color BackColor
		{
			get
			{
				return backBrush.Color;
			}
			set
			{
				backBrush.Color = value;
			}
		}
	}
}
