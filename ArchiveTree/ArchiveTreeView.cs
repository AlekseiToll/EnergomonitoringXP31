using System;
using System.Windows.Forms;
using System.Resources;
using System.Data;
using System.Collections.Generic;

using EmDataSaver.SqlImage;
using EmServiceLib;
using EmArchiveTree;
using DbServiceLib;

namespace EnergomonitoringXP.ArchiveTreeView
{
	/// <summary>
	/// Spetial TreeView that makes easy adding database to the standard TreeView
	/// </summary>
	public class ArchiveTreeViewContainer : TreeView
	{
		#region Fields

		/// <summary>Gets or sets node, which was clicked by right mouse button</summary>
		private EmTreeNodeBase contextNode_;

		/// <summary>Gets or sets node which must be renamed</summary>
		public EmTreeNodeBase RenamingNode;

		/// <summary>Gets or sets active (opened) Archive node</summary>
		public EmArchNodeBase ActiveArchiveNode;
		/// <summary>Gets or sets active (opened) node PQP</summary>
		public EmArchNodeBase ActiveNodePQP;
		/// <summary>Gets or sets active (opened) node AVG</summary>
		public EmArchNodeBase ActiveNodeAVG;
		/// <summary>Gets or sets active (opened) node DNS</summary>
		public EmArchNodeBase ActiveNodeDNS;

		/// <summary>Settings object</summary>
		protected EmDataSaver.Settings settings_;

		/// <summary>Gets or sets node, which was placed in the "buffer"</summary>
		private EmArchNodeBase bufferNode_;

		/// <summary>Gets or sets cut flag</summary>
		private bool bCutFLAG_;

		private bool DBwasUpdated_ = false;

		private ArchiveTreeViewEM32 treeEm32_;
		private ArchiveTreeViewEM33T treeEm33t_;
		private ArchiveTreeViewETPQP treeEtPqp_;
		private ArchiveTreeViewETPQP_A treeEtPqp_A_;

		//private ArchiveTreeViewBase contextTree_;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public ArchiveTreeViewContainer(/*frmMain MainWindow,*/ EmDataSaver.Settings settings)
		{
			this.settings_ = settings;
			//this.mainWindow_ = MainWindow;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Syncronize inner settings with <paramref name="NewSettings"/>
		/// </summary>
		/// <param name="NewSettings">New settings</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			settings_ = NewSettings.Clone();
		}

		public bool ConnectServerAndLoadData(int pgServerIndex)
		{
			if (settings_.PgServers == null)
			{
				EmService.WriteToLogGeneral("ATVC::PgServers = null");
				return false;
			}
			EmService.WriteToLogGeneral("ATVC::ConnectServerAndLoadData index = " + pgServerIndex.ToString());
			//if (this.ContextNode != null)
			//    if (this.ContextNodeType != EmTreeNodeType.PgServer)
			//        return false;
			if (settings_.CurServerIndex < 0) settings_.CurServerIndex = pgServerIndex;

			// проверяем была ли база инициализирована. если нет, выдаем сообщение и выходим
			if (!DBwasUpdated_)
			{
				string connectString = settings_.PgServers[pgServerIndex].PgConnectionStringSystem;
				DbService dbService = new DbService(connectString);
				try 
				{
					//EmService.WriteToLogGeneral("ATVC::before DB open");
					dbService.Open();
					//EmService.WriteToLogGeneral("ATVC::after DB open");
					string commandText = "SELECT datname FROM pg_database;";
					dbService.ExecuteReader(commandText);
					List<string> databases = new List<string>();
					while (dbService.DataReaderRead())
					{
						//EmService.WriteToLogGeneral("ATVC::while DataReaderRead");
						databases.Add(dbService.DataReaderData(0) as string);
					}
					if (!databases.Contains("em_db") || !databases.Contains("em32_db") ||
						!databases.Contains("et33_db") || !databases.Contains("etpqp_a_db"))
					{
						MessageBoxes.MsgDBnotInitialized(this);
						return false;
					}
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Exception in ConnectServerAndLoadData(): DB not init!");
					MessageBoxes.MsgDBnotInitialized(this);
					return false;
				}
				finally 
				{ 
					if (dbService != null) dbService.CloseConnect(); 
				}
			}

			treeEm32_ = new ArchiveTreeViewEM32(pgServerIndex,
								(EmTreeNodeBase)this.Nodes[pgServerIndex],
								GetPgConnectionString(EmDeviceType.EM32, pgServerIndex));
			treeEm33t_ = new ArchiveTreeViewEM33T(pgServerIndex,
								(EmTreeNodeBase)this.Nodes[pgServerIndex],
								GetPgConnectionString(EmDeviceType.EM33T, pgServerIndex));
			treeEtPqp_ = new ArchiveTreeViewETPQP(pgServerIndex,
								(EmTreeNodeBase)this.Nodes[pgServerIndex],
								GetPgConnectionString(EmDeviceType.ETPQP, pgServerIndex));
			treeEtPqp_A_ = new ArchiveTreeViewETPQP_A(pgServerIndex,
								(EmTreeNodeBase)this.Nodes[pgServerIndex],
								GetPgConnectionString(EmDeviceType.ETPQP_A, pgServerIndex));

			this.Nodes[pgServerIndex].Nodes.Clear();

			// load data from Em33 and Em31
			if (!treeEm33t_.ConnectServerAndLoadData(DBwasUpdated_))
			{
				EmService.WriteToLogFailed("Unable load archives info for Em33 or Em31!");
				//return false;
			}
			// load data from Em32
			try
			{
				if (!treeEm32_.ConnectServerAndLoadData(DBwasUpdated_))
				{
					EmService.WriteToLogFailed("Unable load archives info for Em32!");
					//return false;
				}
			}
			catch (EmNoDatabaseException)
			{
				treeEm32_ = null;
			}
			// load data from EtPQP
			try
			{
				if (!treeEtPqp_.ConnectServerAndLoadData(DBwasUpdated_))
				{
					EmService.WriteToLogFailed("Unable load archives info for EtPQP!");
					//return false;
				}
			}
			catch (EmNoDatabaseException)
			{
				treeEtPqp_ = null;
			}
			// load data from EtPQP-A
			try
			{
				if (!treeEtPqp_A_.ConnectServerAndLoadData(DBwasUpdated_))
				{
					EmService.WriteToLogFailed("Unable load archives info for EtPQP-A!");
					//return false;
				}
			}
			catch (EmNoDatabaseException)
			{
				treeEtPqp_A_ = null;
			}

			DBwasUpdated_ = true;

			return true;
		}

        /// <summary>Delete database/folder</summary>
        public bool DeleteFolder()
        {
			if (this.contextNode_.NodeType == EmTreeNodeType.PgServer)
			{
				List<EmDataSaver.PgServerItem> tempList = new List<EmDataSaver.PgServerItem>();
				for(int iServ = 0; iServ < settings_.PgServers.Length; ++iServ)
				{
					if(iServ != contextNode_.PgServerIndex)
						tempList.Add(settings_.PgServers[iServ]);
				}
				settings_.PgServers = tempList.ToArray();
				settings_.SaveSettings();
				this.contextNode_.Remove();
				return true;
			}

			return GetContextTree().DeleteFolder(ref this.contextNode_);
        }

        /// <summary>Expand all inner nodes</summary>
		public bool ExportArchive(out EmSqlDataNodeType[] parts, out EmDeviceType devType)
        {
			devType = EmDeviceType.NONE;
			parts = null;
			try
			{
				if (!(contextNode_ is EmArchNodeBase)) return false;

				devType = (contextNode_ as EmArchNodeBase).DeviceType;
				return GetContextTree().ExportArchive(out parts, ref this.contextNode_);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ExportArchive(): " + ex.Message);
				throw;
			}
        }

		/// <summary>
		/// Отобразить свойства узла дерева
		/// </summary>
		public bool ShowArchOptions()
		{
			return GetContextTree().ShowArchOptions(ref contextNode_);
		}

		/// <summary>Cut node to the buffer</summary>
		public void CutFolder()
		{
			if (!(contextNode_ is EmArchNodeBase))
			{
				bufferNode_ = null;
				return;
			}

			this.bufferNode_ = (EmArchNodeBase)this.contextNode_;
			this.bCutFLAG_ = true;
		}

		/// <summary>Paste node from the buffer</summary>
		public void PasteFolder()
		{
			try
			{
				if (bufferNode_ == null)
				{
					EmService.WriteToLogFailed("Paste was clicked, but buffer node is null");
					return;
				}

				if (bCutFLAG_ != true) return;

				if (!(this.contextNode_ is EmArchNodeBase))
					return;

				EmArchNodeBase contNodeArchNode = (this.contextNode_ as EmArchNodeBase);
				InsertFolder(ref bufferNode_, ref contNodeArchNode);

				bufferNode_ = null;
				bCutFLAG_ = false;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in PasteFolder(): " + ex.Message);
				throw;
			}
		}

		/// <summary>Paste node from the buffer</summary>
		public bool InsertFolder(ref EmArchNodeBase nodeToPaste, ref EmArchNodeBase nodeDestination)
		{
			return GetContextTree().InsertFolder(ref nodeToPaste, ref nodeDestination);
		}

		public bool CreateNewFolder()
		{
			try
			{
                TreeNode selNode = null;
                ArchiveTreeViewBase contextTree = GetContextTree();
                if (contextTree == null) return false;

                bool res = contextTree.CreateNewFolder(ref this.contextNode_, ref selNode);
                if (selNode != null) this.SelectedNode = selNode;
                return res;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in CreateNewFolder(): " + ex.Message);
				throw;
			}
		}
		
		#endregion

		#region Properties

		public EmTreeNodeType ContextNodeType
		{
			get { return this.contextNode_.NodeType; }
		}

		public EmTreeNodeBase ContextNode
		{
			get { return this.contextNode_; }
			set { this.contextNode_ = value; }
		}

		public EmTreeNodeBase BufferNode
		{
			get { return this.bufferNode_; }
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriding OnMouseUp event to save info about which element's context menu must be shown.
		/// </summary>
		/// <param name="e">Standard MouseEventArgs</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				SelectedNode = this.GetNodeAt(new System.Drawing.Point(e.X, e.Y));
				ContextNode = (EmTreeNodeBase)SelectedNode;
			}

			base.OnMouseUp(e);
		}											 

		/// <summary>
		/// Overriding OnKeyUp.
		/// </summary>
		/// <param name="e">Standard KeyEventArgs</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Right || 
				e.KeyCode == Keys.Left || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown) 
			{
				ContextNode = (EmTreeNodeBase)SelectedNode;
			}
			base.OnKeyUp(e);
		}

		#endregion

		#region Private Methods

		private string GetPgConnectionString(EmDeviceType devType, int PgServerIndex)
		{
			try
			{
				string ConnectString = "";
				settings_.LoadSettings();
				switch (devType)
				{
					case EmDeviceType.EM32:
						ConnectString = settings_.PgServers[PgServerIndex].PgConnectionStringEm32; break;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T1:
						ConnectString = settings_.PgServers[PgServerIndex].PgConnectionStringEm33; break;
					case EmDeviceType.ETPQP:
						ConnectString = settings_.PgServers[PgServerIndex].PgConnectionStringEtPQP; break;
					case EmDeviceType.ETPQP_A:
						ConnectString = settings_.PgServers[PgServerIndex].PgConnectionStringEtPQP_A; break;
				}
				return ConnectString;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in GetPgConnectionString(): " + ex.Message);
				throw;
			}
		}

		private ArchiveTreeViewBase GetContextTree()
		{
			try
			{
				if (ContextNodeType == EmTreeNodeType.PgServer) return null;
				if (!(contextNode_ is EmArchNodeBase)) return null;

				if (treeEm32_ == null && treeEtPqp_ == null && treeEtPqp_A_ == null) return treeEm33t_;

				EmArchNodeBase contextEmArchNode = (EmArchNodeBase)contextNode_;
				switch (contextEmArchNode.DeviceType)
				{
					case EmDeviceType.EM31K:
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
						return treeEm33t_;
					case EmDeviceType.EM32: return treeEm32_;
					case EmDeviceType.ETPQP: return treeEtPqp_;
					case EmDeviceType.ETPQP_A: return treeEtPqp_A_;
					default: return null;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in GetContextTree(): " + ex.Message);
				throw;
			}
		}

		#endregion
	}
}
