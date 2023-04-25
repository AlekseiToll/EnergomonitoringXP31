// (c) Mars-Energo Ltd.
//
// CheckTreeNode class. 
// All other types of nodes are inherited from thus class
// 
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.0
// Last revision	:	19.04.2006 13:15

using System;
using System.Windows.Forms;

namespace EmDataSaver.SavingInterface.CheckTreeView
{
	/// <summary>
	/// CheckTreeNode class.
	/// All other types of nodes are inherited from thus class
	/// </summary>
	public class CheckTreeNode : TreeNode
	{
		#region Fields

		/// <summary>
		/// Checked State
		/// </summary>
		private CheckState _checkState;

		#endregion

		#region Properties

		/// <summary>
		/// Gets checked state
		/// </summary>
		public CheckState CheckState
		{
			get
			{
				return _checkState;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public CheckTreeNode()
		{
			_setState(this, CheckState.Unchecked);
		}

		#endregion

		#region Private methods

		private void _setState(CheckTreeNode Node, CheckState CheckState)
		{
			Node._checkState = CheckState;
			// Images
			if (CheckState == System.Windows.Forms.CheckState.Unchecked)
			{
				Node.ImageIndex = 0;
				Node.SelectedImageIndex = 0;
			}
			else if (CheckState == System.Windows.Forms.CheckState.Checked)
			{
				Node.ImageIndex = 1;
				Node.SelectedImageIndex = 1;
			}
			else
			{
				Node.ImageIndex = 2;
				Node.SelectedImageIndex = 2;
			}
		}

		private CheckState _analyseState(CheckTreeNode Node)
		{
			int iChecked = 0;
			int iUnchecked = 0;
			int iNumOfCheckTreeNode = Node.Nodes.Count;

			for (int i = 0; i < Node.Nodes.Count; i++)
			{
				// to skip if child is not CheckTreeNode
				if (!(Node.Nodes[i] is CheckTreeNode))
				{
					iNumOfCheckTreeNode--;
					continue;
				}

				if ((Node.Nodes[i] as CheckTreeNode).CheckState == CheckState.Checked)
				{
					iChecked++;
				}
				else if ((Node.Nodes[i] as CheckTreeNode).CheckState == CheckState.Unchecked)
				{
					iUnchecked++;
				}
			}

			if (iChecked == iNumOfCheckTreeNode) return CheckState.Checked;
			if (iUnchecked == iNumOfCheckTreeNode) return CheckState.Unchecked;
			return CheckState.Indeterminate;
		}

		private void _updateParent(CheckTreeNode Node)
		{
			if (Node.Parent == null) return;
			//if ( !(Node.Parent is CheckTreeNode) ) return;

			_setState((Node.Parent as CheckTreeNode), _analyseState((Node.Parent as CheckTreeNode)));
			_updateParent((Node.Parent as CheckTreeNode));
		}

		private void _updateChilds(CheckTreeNode Node, CheckState CheckState)
		{
			for (int i = 0; i < Node.Nodes.Count; i++)
			{
				if (!(Node.Nodes[i] is CheckTreeNode)) continue;

				_setState((Node.Nodes[i] as CheckTreeNode), CheckState);
				_updateChilds((Node.Nodes[i] as CheckTreeNode), CheckState);
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Check node (and all child nodes recursively)
		/// </summary>
		public void Check()
		{
			// this
			_setState(this, CheckState.Checked);

			// child
			_updateChilds(this, CheckState.Checked);

			// parent
			_updateParent(this);
		}

		/// <summary>
		/// Uncheck node (and all child nodes recursively)
		/// </summary>
		public void Uncheck()
		{
			// this
			_setState(this, CheckState.Unchecked);

			// child
			_updateChilds(this, CheckState.Unchecked);

			// parent
			_updateParent(this);
		}

		#endregion
	}

}