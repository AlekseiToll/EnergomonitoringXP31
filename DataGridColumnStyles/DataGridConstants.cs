// (c) Mars-Energo Ltd.
// 
// Average values float-window form
//
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.2
// Last revision	:	07.02.2006 16:33

using System;
using System.Drawing;

namespace DataGridColumnStyles
{
    /// <summary>
    /// Define color constants of data grid columns
    /// </summary>
    public class DataGridColors
    {
		/// <summary>Gets color for all common data independently of phase</summary>
		public static Color ColorCommon
		{
			get { return Color.AliceBlue; }
		}

        /// <summary>Gets color for datetime column</summary>
		public static Color ColorAvgTime
		{
			get { return Color.White; }
		}

        /// <summary>Gets color for columns for parameter of phase A</summary>
		public static Color ColorAvgPhaseA
		{
			get { return Color.FromArgb(0xFF, 0xFF, 0xE1); }
		}

        /// <summary>Gets color for columns for parameter of phase B</summary>
		public static Color ColorAvgPhaseB
		{
			get { return Color.FromArgb(0xF0, 0xFF, 0xF0); }
		}

        /// <summary>Gets color for columns for parameter of phase C</summary>
		public static Color ColorAvgPhaseC
		{
			get { return Color.FromArgb(0xFF, 0xF0, 0xF0); }
		}

		/// <summary>Gets color for result PQP data independently of phase</summary>
		public static Color ColorPkeResult
		{
			get { return Color.Ivory; }
		}

		/// <summary>Gets color for all standsrd PQP data independently of phase</summary>
		public static Color ColorPkeStandard
		{
			get { return Color.LavenderBlush; }
		}
		
		/// <summary>Gets color for datetime column</summary>
		public static Color ColorPqpParam
		{
			get { return Color.White; }
		}
    }

    /// <summary>
    /// Define width constants of data grid columns
    /// </summary>
    public class DataColumnsWidth
    {
        /// <summary>
        /// Gets width for datetime columns
        /// </summary>
        public static int TimeWidth
        {
            get
            {
                return 120;
            }
        }

        /// <summary>
        /// Gets default width for all columns
        /// </summary>
        public static int CommonWidth
        {
            get
            {
                return 90;
            }
        }

		/// <summary>
		/// Gets default width for all columns
		/// </summary>
		public static int SmallWidth
		{
			get
			{
				return 50;
			}
		}
    }

    /// <summary>
    /// Define format constants of data grid columns
    /// </summary>
    public class DataColumnsFormat
    {
        /// <summary>
        /// Gets format for columns with <c>float</c> data type
        /// </summary>
        public static string FloatFormat
        {
            get
            {
                return "0.0000";
            }
        }

		public static string FloatShortFormat
		{
			get
			{
				return "##0.##";
			}
		}

		//public static string PercentFormat
		//{
		//    get
		//    {
		//        return "0.####";
		//    }
		//}

		public static string GetPercentFormat(int floatSigns)
		{
			if(floatSigns < 1) return "0.##";	// by default

			string res = "0.";
			for (int i = 0; i < floatSigns; ++i)
				res += "#";
			return res;
		}
    }      
}
