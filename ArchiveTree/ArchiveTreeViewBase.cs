using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using EmDataSaver.SqlImage;
using EmServiceLib;
using EmArchiveTree;

namespace EnergomonitoringXP.ArchiveTreeView
{
	abstract class ArchiveTreeViewBase : TreeView
	{
		#region Fields

		protected int pgServerIndex_;

		protected EmTreeNodeBase pgServerNode_;

		protected string connectString_;

		#endregion

		#region Constructors

		internal ArchiveTreeViewBase(int pgServerIndex, EmTreeNodeBase pgServerNode, string connectString)
		{
			pgServerIndex_ = pgServerIndex;
			pgServerNode_ = pgServerNode;
			connectString_ = connectString;
		}

		#endregion

		#region Internal Methods

		internal abstract bool ConnectServerAndLoadData(bool DBwasUpdated);

		internal abstract bool DeleteFolder(ref EmTreeNodeBase contextNode);

		internal abstract bool CreateNewFolder(ref EmTreeNodeBase contextNode, ref TreeNode selectedNode);

		internal abstract bool ExportArchive(out EmSqlDataNodeType[] parts, ref EmTreeNodeBase contextNode);

		/// <summary>
		/// Отобразить свойства узла дерева
		/// </summary>
		/// <param name="contextNode">Текущий узел дерева</param>
		internal abstract bool ShowArchOptions(ref EmTreeNodeBase contextNode);

		internal abstract bool InsertFolder(ref EmArchNodeBase nodeToPaste, ref EmArchNodeBase nodeDestination);

		#endregion
	}
}
