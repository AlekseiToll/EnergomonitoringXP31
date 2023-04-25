// (c) Mars-Energo Ltd.
//
// MeasureTypeTreeNode class.
// Contains global mesasure types such as PQP, AVG etc..
// 
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.0
// Last revision	:	19.04.2006 13:15

using System;
using System.Windows.Forms;

using EmArchiveTree;

namespace EmDataSaver.SavingInterface.CheckTreeView
{
	/// <summary>
	/// MeasureTypeTreeNode class.
	/// Contains global mesasure types such as PQP, AVG etc..
	/// </summary>
	public class MeasureTypeTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		/// Type of measure
		/// </summary>
		public MeasureType MeasureType;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="MeasureType">Type of creating measure</param>
		public MeasureTypeTreeNode(MeasureType MeasureType)
		{
			this.Tag = "MeasureType";
			this.MeasureType = MeasureType;
		}

		#endregion
	}
}