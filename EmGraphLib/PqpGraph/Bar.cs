using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace EmGraphLib.PqpGraph
{
	/// <summary>
	/// One Bar on the PQP diagram
	/// </summary>
	[Localizable(true),
   System.Drawing.ToolboxBitmap(@"D:\test\EnergomonitoringXP.4.0.6b\EnergomonitoringXP v.4.0.6.0\EmGraphLib\PqpGraph\PqpGraph.png")]
	public class Bar: Component
	{
		private Brush barBrush;
		private Pen barPen;

		private float worldTop;
		private float worldBottom;

		private String caption;
		private Font captionFont;
		private string percentText;

		private Font percentFont;
		private string worldValFormat;

		private Font worldValFont;
		private Brush captionBrush;
		private Brush percentBrush;
		private Brush worldValBrush;

		/// <summary>
		/// Constructor with a parameters
		/// </summary>
		/// <param name="Caption">Caption text of the bar</param>
		/// <param name="WorldTop">World top coordinate</param>
		/// <param name="WorldBottom">World bottom coordinate</param>
		/// <param name="PercentText">Percent value text</param>
		/// <param name="BarColor">Bar fill color</param>
		/// <param name="BarBoundColor">Bar bound color</param>
		/// <param name="PercentColor">Percent avlue color</param>
		public Bar(string Caption, float WorldTop, float WorldBottom, string PercentText, Color BarColor, Color BarBoundColor, Color PercentColor)
		{
			this.caption = Caption;
			this.captionFont = new Font(FontFamily.GenericSansSerif, 10F);
			this.captionBrush = new SolidBrush(Color.DarkGray);

			this.barBrush = new SolidBrush(BarColor);
			this.barPen = new Pen(BarBoundColor);

			this.worldTop = WorldTop;
			this.worldBottom = WorldBottom;
			this.worldValFont = new Font(FontFamily.GenericSansSerif, 8F);
			this.worldValFormat = "0.0##";
			this.worldValBrush = new SolidBrush(Color.SteelBlue);
						
			this.percentBrush = new SolidBrush(PercentColor);			
			this.percentFont = new Font(FontFamily.GenericSansSerif, 10F);
			this.percentText = PercentText;
		}

		/// <summary>
		/// Constructor without any parameters
		/// </summary>
		public Bar()
		{
			this.caption = "Fill Caption";
			this.captionFont = new Font(FontFamily.GenericSansSerif, 10F);
			this.captionBrush = new SolidBrush(Color.DarkGray);

			this.barBrush = new SolidBrush(Color.White);
			this.barPen = new Pen(Color.SteelBlue);

			this.worldTop = 1.0F;
			this.worldBottom = 0.0F;
			this.worldValFont = new Font(FontFamily.GenericSansSerif, 8.0F);
			this.worldValFormat = "0.0##";
			this.worldValBrush = new SolidBrush(Color.SteelBlue);

			this.percentText = string.Empty;
			this.percentBrush = new SolidBrush(Color.SteelBlue);
			this.percentFont = new Font(FontFamily.GenericSansSerif, 10F);
		}

		/// <summary>
		/// Gets or sets caption text of the bar
		/// </summary>
		[Localizable(true),
		Browsable(true), 
		Category("Caption"), 
		Description("Caption text")]
		public string Caption
		{
			get
			{
				return caption;
			}
			set
			{
				caption = value;
			}
		}

		/// <summary>
		/// Gets or sets caption font
		/// </summary>
		[Browsable(true),
		Category("Caption"),
		Description("Caption font")]
		public Font CaptionFont
		{
			get
			{
				return captionFont;
			}
			set
			{
				captionFont = value;
			}
		}

		/// <summary>
		/// Gets or sets caption color
		/// </summary>
		[Browsable(true),
		Category("Caption"),
		Description("Caption foregroung color"),
		RefreshProperties(RefreshProperties.All)]
		public Color CaptionColor
		{
			get
			{
				return (captionBrush as SolidBrush).Color;
			}
			set
			{
				if (value != null) captionBrush = new SolidBrush(value);
			}
		}

		/// <summary>
		/// Gets bar caption Brush object
		/// </summary>
		[Browsable(false)]
		internal Brush CaptionBrush
		{
			get
			{
				return captionBrush;
			}
		}		

		/// <summary>
		/// Gets or sets world bottom coordinate
		/// </summary>
		[Browsable(true), 
		Category("World coordinates"), 
		Description("Bottom world coordinate value"),
		DefaultValue(float.NaN),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public float WorldBottom
		{
			get
			{
				return worldBottom;
			}
			set
			{
				if (value != worldBottom)
				{
					worldBottom = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets world top coordinate
		/// </summary>
		[Browsable(true), 
		Category("World coordinates"), 
		Description("Top world coordinate value"), 
		DefaultValue(float.NaN),
	    DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public float WorldTop
		{
			get
			{
				return worldTop;
			}
			set
			{
				worldTop = value;
			}
		}

		/// <summary>
		/// Gets or sets coordinate's format text
		/// </summary>
		[Browsable(true),
		Category("World coordinates"),
		Description("Format string of world coordinates value"),
		DefaultValue("0.0##"),
	    DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string WorldValFormat
		{
			get
			{
				return worldValFormat;
			}
			set
			{
				worldValFormat = value;
			}
		}

		/// <summary>
		/// Gets or sets caption font
		/// </summary>
		[Browsable(true),
		Category("World coordinates"),
		Description("World coordinates font")]
		public Font WorldValFont
		{
			get
			{
				return worldValFont;
			}
			set
			{
				worldValFont = value;
			}
		}

		/// <summary>
		/// Gets or sets world coordinates text color
		/// </summary>
		[Browsable(true),
		Category("World coordinates"),
		Description("World coordinates font color")]
		public Color WorldValColor
		{
			get
			{
				return (worldValBrush as SolidBrush).Color;
			}
			set
			{
				if ((worldValBrush as SolidBrush).Color != value)
					worldValBrush = new SolidBrush(value);
			}
		}

		/// <summary>
		/// Gets world coordinates text brush
		/// </summary>
		[Browsable(false)]
		public Brush WorldValBrush
		{
			get
			{
				return worldValBrush;
			}
		}

		/// <summary>
		/// Gets or sets bar's color
		/// </summary>
		[Browsable(true),
		Category("Appearance"),
		Description("Bar's fill color")]
		public Color BarColor
		{
			get
			{
				return (barBrush as SolidBrush).Color;
			}
			set
			{
				if ((barBrush as SolidBrush).Color != value)
					barBrush = new SolidBrush(value);
			}
		}

		/// <summary>
		/// Gets or sets bar's border color
		/// </summary>
		[Browsable(true),
		Category("Appearance"),
		Description("Bar's border color")]
		public Color BarBorderColor
		{
			get
			{
				return barPen.Color;
			}
			set
			{
				if (barPen.Color != value)
					barPen = new Pen(value);
			}
		}

		/// <summary>
		/// Gets bar bound's pen
		/// </summary>
		[Browsable(false)]
		internal Pen BarPen
		{
			get
			{
				return barPen;
			}
		}

		/// <summary>
		/// Gets bar Brush object
		/// </summary>
		[Browsable(false)]
		public Brush BarBrush
		{
			get
			{
				return barBrush;
			}
		}

		/// <summary>
		/// Gets or sets percent value text
		/// </summary>
		[Browsable(false)]
		public string PercentText
		{
			get
			{
				return percentText;
			}
			set
			{
				percentText = value;
			}
		}

		/// <summary>
		/// Gets or sets percent font
		/// </summary>
		[Browsable(true),
		Category("Appearance"),
		Description("Bar's text font")]
		public Font PercentFont
		{
			get
			{
				return percentFont;
			}
			set
			{
				percentFont = value;
			}
		}
		
		/// <summary>
		/// Gets or sets percent value text color
		/// </summary>
		[Browsable(true),
		Category("Appearance"),
		Description("Bar's text color")]
		public Color PercentColor
		{
			get
			{
				return (percentBrush as SolidBrush).Color;
			}
			set
			{
				if ((percentBrush as SolidBrush).Color != value)
				{
					percentBrush = new SolidBrush(value);
				}
			}
		}

		/// <summary>
		/// Gets percent value text brush
		/// </summary>
		[Browsable(false)]
		internal Brush PercentBrush
		{
			get
			{
				return percentBrush;
			}
		}
		

		/// <summary>
		/// Gets caption size
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <returns>Caption size</returns>
		public Size GetCaptionSize(Graphics g)
		{
			return Size.Ceiling(g.MeasureString(caption, captionFont));
		}

		/// <summary>
		/// Gets percent text size
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <returns>Percent text size</returns>
		public Size GetPercentTextSize(System.Drawing.Graphics g)
		{
			return Size.Ceiling(g.MeasureString(percentText, percentFont));
		}

		/// <summary>
		/// Gets world text size
		/// </summary>
		/// <returns>World text size</returns>
		public Size GetWorldTextSize(System.Drawing.Graphics g)
		{
			Size wts = Size.Ceiling(g.MeasureString(worldTop.ToString(worldValFormat), worldValFont));
			Size bts = Size.Ceiling(g.MeasureString(worldBottom.ToString(worldValFormat), worldValFont));

			return new Size(
				(wts.Width > bts.Width ? wts.Width : bts.Width) + 4,
				wts.Height > bts.Height ? wts.Height : bts.Height);
		}
	}
}
