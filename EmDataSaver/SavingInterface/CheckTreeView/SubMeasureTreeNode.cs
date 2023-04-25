// (c) Mars-Energo Ltd.
//
// SubMeasureTreeNode class.
// Now it's for AVG values ONLY
//
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.0
// Last revision	:	19.04.2006 13:15

using System;
using System.Windows.Forms;
using EmDataSaver.SavingInterface;
using System.Resources;

using EmArchiveTree;

namespace EmDataSaver.SavingInterface.CheckTreeView
{
	/// <summary>
	/// SubMeasureTreeNode class.
	/// Now it's for AVG values ONLY
	/// </summary>
	public class SubMeasureTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		/// Submeasure type
		/// </summary>
		public SubMeasureType SubMeasureType;

		/// <summary>
		/// Inner measure index
		/// </summary>
		public int MeasureIndex;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="SubMeasureType">Submeasure type</param>
		public SubMeasureTreeNode(SubMeasureType SubMeasureType)
		{
			this.SubMeasureType = SubMeasureType;

			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);

			switch (this.SubMeasureType)
			{
				case SubMeasureType.AVGMain:
					this.Text = rm.GetString("name_submeasure_avg_main");
					break;
				case SubMeasureType.AVGHarmonics:
					this.Text = rm.GetString("name_submeasure_avg_harmonics");
					break;
				case SubMeasureType.AVGAngles:
					this.Text = rm.GetString("name_submeasure_avg_angles");
					break;
			}
			this.Tag = "SubMeasure";
		}
		#endregion
	}
}