using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Resources;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;

using WeifenLuo.WinFormsUI;
using EnergomonitoringXP.ArchiveTreeView;
using DbServiceLib;
using EmDataSaver.SqlImage;
using EmDataSaver.SavingInterface;
using EmServiceLib;
using EmArchiveTree;

namespace EnergomonitoringXP
{
	/// <summary>
	/// Summary description for frmToolbox.
	/// </summary>
	public class frmToolbox : DockContent
	{
		// TreeView
		// Node being dragged
		private EmArchNodeBase dragNode_ = null;

		// Temporary drop node for selection
		private EmTreeNodeBase tempDropNode_ = null;
		private ImageList imageListDrag;

		// Timer for scrolling
		private readonly Timer timer = new Timer();
		private ImageList ilArchives;
		private ContextMenuStrip cmsOptions;
		private ToolStripMenuItem msArchOpen;
		private ToolStripSeparator msArchSeparator01;
		private ToolStripMenuItem msArchRename;
		private ToolStripMenuItem msArchDelete;
		private ToolStripSeparator msArchSeparator02;
		private ToolStripMenuItem msArchOptions;
		private ToolStripMenuItem msNewFolder;
		private ToolStripMenuItem msCut;
		private ToolStripMenuItem msPaste;
		private ToolStripSeparator msArchSeparator03;
		private ToolStripMenuItem msExpandAll;

		/// <summary>Дерево архивов</summary>
		private ArchiveTreeViewContainer tvArchives = null;

		private System.ComponentModel.IContainer components;

		/// <summary>Settings object</summary>
		private EmDataSaver.Settings settings_ = null;
		private ToolStripMenuItem msArchExport;
		private ToolStripSeparator msArchSeparator04;
		private ToolStripMenuItem msArchOpenTR;
		private ToolStripMenuItem msArchCreateNewPgServer;
		private ToolStripMenuItem msArchConnect;
		private ToolStripMenuItem msArchDisconnect;

		/// <summary>Pointer to the main application window</summary>
		protected frmMain mainWindow_ = null;

		/// <summary>List of indexes of connected servers</summary>
		private List<int> connectedServerIndexes_ = new List<int>();

		/// <summary>Synchronize settings</summary>
		/// <param name="NewSettings">Object to synchronize with</param>
		public void SyncronizeSettings(EmDataSaver.Settings NewSettings)
		{
			try
			{
				// syncronizing own settings
				settings_ = NewSettings.Clone();
				// syncronizing child settings
				tvArchives.SyncronizeSettings(NewSettings.Clone());
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SyncronizeSettings(): ");
				throw;
			}
		}

		// force - переподключить сервер, когда он уже подключен (нужно если обновилось содержимое сервера)
		public bool ConnectServerAndLoadData(int pgServerIndex, bool force)
		{
			try
			{
				EmService.WriteToLogGeneral("ConnectServerAndLoadData index = " + pgServerIndex.ToString());
				// check if this server is already connected
				if (!connectedServerIndexes_.Contains(pgServerIndex) || force)
				{
					if (tvArchives.ConnectServerAndLoadData(pgServerIndex))
					{
						(tvArchives.Nodes[pgServerIndex] as EmTreeNodeServer).Connect();

						if (!connectedServerIndexes_.Contains(pgServerIndex))
							connectedServerIndexes_.Add(pgServerIndex);

						return true;
					}
				}
				else return true; // already connected

				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ConnectServerAndLoadData():");
				//throw;
				return false;
			}
		}

		public void DeactivateAllActiveNodes()
		{
			if (tvArchives.ActiveArchiveNode != null)
			{ tvArchives.ActiveArchiveNode.Deactivate(); tvArchives.ActiveArchiveNode = null; }
			if (tvArchives.ActiveNodeAVG != null)
			{ tvArchives.ActiveNodeAVG.Deactivate(); tvArchives.ActiveNodeAVG = null; }
			if (tvArchives.ActiveNodePQP != null)
			{ tvArchives.ActiveNodePQP.Deactivate(); tvArchives.ActiveNodePQP = null; }
			if (tvArchives.ActiveNodeDNS != null)
			{ tvArchives.ActiveNodeDNS.Deactivate(); tvArchives.ActiveNodeDNS = null; }
		}

		///// <summary>Default constructor</summary>
		//public frmToolbox() 
		//{
		//    InitializeComponent(); 
		//}

		/// <summary>
		/// Default constructor
		/// <param name="MainWindow">Pointer to the main application window</param>
		/// <param name="settings">settings object</param>
		/// </summary>
		public frmToolbox(frmMain MainWindow, EmDataSaver.Settings settings)
		{
			// Вызываем перед InitializeComponent() 
			this.settings_ = settings;
			this.mainWindow_ = MainWindow;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			imageListDrag = new ImageList();

			timer.Interval = 200;
			timer.Tick += new EventHandler(timer_Tick);

			if (settings.PgServers == null) return;
			if (settings.PgServers.Length == 0) return;

			foreach (EmDataSaver.PgServerItem pgSrv in settings.PgServers)
			{
				EmTreeNodeServer node = new EmTreeNodeServer(
					pgSrv.PgHost,
					pgSrv.PgPort,
					pgSrv.PgServerName);
				tvArchives.Nodes.Add(node);
			}
		}

		/// <summary>Clean up any resources being used</summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmToolbox));
			this.ilArchives = new System.Windows.Forms.ImageList(this.components);
			this.cmsOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.msArchCreateNewPgServer = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchDisconnect = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchOpenTR = new System.Windows.Forms.ToolStripMenuItem();
			this.msExpandAll = new System.Windows.Forms.ToolStripMenuItem();
			this.msNewFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchSeparator02 = new System.Windows.Forms.ToolStripSeparator();
			this.msCut = new System.Windows.Forms.ToolStripMenuItem();
			this.msPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchSeparator01 = new System.Windows.Forms.ToolStripSeparator();
			this.msArchRename = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchSeparator03 = new System.Windows.Forms.ToolStripSeparator();
			this.msArchExport = new System.Windows.Forms.ToolStripMenuItem();
			this.msArchSeparator04 = new System.Windows.Forms.ToolStripSeparator();
			this.msArchOptions = new System.Windows.Forms.ToolStripMenuItem();
			//this.settings_ = new EmDataSaver.Settings();  //uncomment to view designer
			this.tvArchives = new EnergomonitoringXP.ArchiveTreeView.ArchiveTreeViewContainer(/*this.mainWindow_,*/ this.settings_);
			this.cmsOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// ilArchives
			// 
			this.ilArchives.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilArchives.ImageStream")));
			this.ilArchives.TransparentColor = System.Drawing.Color.Transparent;
			this.ilArchives.Images.SetKeyName(0, "");
			this.ilArchives.Images.SetKeyName(1, "");
			this.ilArchives.Images.SetKeyName(2, "");
			this.ilArchives.Images.SetKeyName(3, "");
			this.ilArchives.Images.SetKeyName(4, "");
			// 
			// cmsOptions
			// 
			this.cmsOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msArchCreateNewPgServer,
            this.msArchConnect,
            this.msArchDisconnect,
            this.msArchOpen,
            this.msArchOpenTR,
            this.msExpandAll,
            this.msNewFolder,
            this.msArchSeparator02,
            this.msCut,
            this.msPaste,
            this.msArchSeparator01,
            this.msArchRename,
            this.msArchDelete,
            this.msArchSeparator03,
            this.msArchExport,
            this.msArchSeparator04,
            this.msArchOptions});
			this.cmsOptions.Name = "cmsOptions";
			resources.ApplyResources(this.cmsOptions, "cmsOptions");
			// 
			// msArchCreateNewPgServer
			// 
			resources.ApplyResources(this.msArchCreateNewPgServer, "msArchCreateNewPgServer");
			this.msArchCreateNewPgServer.Name = "msArchCreateNewPgServer";
			this.msArchCreateNewPgServer.Click += new System.EventHandler(this.msCreateNewPgSrv_Click);
			// 
			// msArchConnect
			// 
			resources.ApplyResources(this.msArchConnect, "msArchConnect");
			this.msArchConnect.Name = "msArchConnect";
			this.msArchConnect.Click += new System.EventHandler(this.msArchConnect_Click);
			// 
			// msArchDisconnect
			// 
			resources.ApplyResources(this.msArchDisconnect, "msArchDisconnect");
			this.msArchDisconnect.Name = "msArchDisconnect";
			this.msArchDisconnect.Click += new System.EventHandler(this.msArchDisconnect_Click);
			// 
			// msArchOpen
			// 
			resources.ApplyResources(this.msArchOpen, "msArchOpen");
			this.msArchOpen.Name = "msArchOpen";
			this.msArchOpen.Click += new System.EventHandler(this.mmArchOpen_Click);
			// 
			// msArchOpenTR
			// 
			resources.ApplyResources(this.msArchOpenTR, "msArchOpenTR");
			this.msArchOpenTR.Name = "msArchOpenTR";
			this.msArchOpenTR.Click += new System.EventHandler(this.msArchOpenTR_Click);
			// 
			// msExpandAll
			// 
			resources.ApplyResources(this.msExpandAll, "msExpandAll");
			this.msExpandAll.Name = "msExpandAll";
			this.msExpandAll.Click += new System.EventHandler(this.msExpandAll_Click);
			// 
			// msNewFolder
			// 
			resources.ApplyResources(this.msNewFolder, "msNewFolder");
			this.msNewFolder.Name = "msNewFolder";
			this.msNewFolder.Click += new System.EventHandler(this.msNewFolder_Click);
			// 
			// msArchSeparator02
			// 
			this.msArchSeparator02.Name = "msArchSeparator02";
			resources.ApplyResources(this.msArchSeparator02, "msArchSeparator02");
			// 
			// msCut
			// 
			resources.ApplyResources(this.msCut, "msCut");
			this.msCut.Name = "msCut";
			this.msCut.Click += new System.EventHandler(this.msCut_Click);
			// 
			// msPaste
			// 
			resources.ApplyResources(this.msPaste, "msPaste");
			this.msPaste.Name = "msPaste";
			this.msPaste.Click += new System.EventHandler(this.msPaste_Click);
			// 
			// msArchSeparator01
			// 
			this.msArchSeparator01.Name = "msArchSeparator01";
			resources.ApplyResources(this.msArchSeparator01, "msArchSeparator01");
			// 
			// msArchRename
			// 
			resources.ApplyResources(this.msArchRename, "msArchRename");
			this.msArchRename.Name = "msArchRename";
			this.msArchRename.Click += new System.EventHandler(this.mmArchRename_Click);
			// 
			// msArchDelete
			// 
			resources.ApplyResources(this.msArchDelete, "msArchDelete");
			this.msArchDelete.Name = "msArchDelete";
			this.msArchDelete.Click += new System.EventHandler(this.mmArchDelete_Click);
			// 
			// msArchSeparator03
			// 
			this.msArchSeparator03.Name = "msArchSeparator03";
			resources.ApplyResources(this.msArchSeparator03, "msArchSeparator03");
			// 
			// msArchExport
			// 
			resources.ApplyResources(this.msArchExport, "msArchExport");
			this.msArchExport.Name = "msArchExport";
			this.msArchExport.Click += new System.EventHandler(this.msArchExport_Click);
			// 
			// msArchSeparator04
			// 
			this.msArchSeparator04.Name = "msArchSeparator04";
			resources.ApplyResources(this.msArchSeparator04, "msArchSeparator04");
			// 
			// msArchOptions
			// 
			resources.ApplyResources(this.msArchOptions, "msArchOptions");
			this.msArchOptions.Name = "msArchOptions";
			this.msArchOptions.Click += new System.EventHandler(this.mmArchOptions_Click);
			// 
			// tvArchives
			// 
			this.tvArchives.AllowDrop = true;
			this.tvArchives.ContextNode = null;
			resources.ApplyResources(this.tvArchives, "tvArchives");
			this.tvArchives.ImageList = this.ilArchives;
			this.tvArchives.Name = "tvArchives";
			this.tvArchives.ShowRootLines = false;
			this.tvArchives.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvArchives_ItemDrag);
			this.tvArchives.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvArchives_AfterSelect);
			this.tvArchives.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvArchives_DragDrop);
			this.tvArchives.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvArchives_DragEnter);
			this.tvArchives.DragOver += new System.Windows.Forms.DragEventHandler(this.tvArchives_DragOver);
			this.tvArchives.DragLeave += new System.EventHandler(this.tvArchives_DragLeave);
			this.tvArchives.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.tvArchives_GiveFeedback);
			this.tvArchives.DoubleClick += new System.EventHandler(this.tvArchives_DoubleClick);
			this.tvArchives.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tvArchives_KeyUp);
			this.tvArchives.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvArchives_MouseUp);
			// 
			// frmToolbox
			// 
			resources.ApplyResources(this, "$this");
			this.CloseButton = false;
			this.Controls.Add(this.tvArchives);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockLeft)));
			this.HideOnClose = true;
			this.Name = "frmToolbox";
			this.ShowHint = WeifenLuo.WinFormsUI.DockState.DockLeftAutoHide;
			this.cmsOptions.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Открытие архива с или без учета коэффициентов трансформации
		/// </summary>
		/// <param name="withTR">Учитывать коэффициенты трансформации (true) или нет (false)</param>
		private void OpenArchive(bool withTR)
		{
			try
			{
				EmService.ShowWndFeedback = true;

				this.mainWindow_.Cursor = Cursors.WaitCursor;

				//bool isAVGShown = false;

				// if ContextNode is EmTreeNodeObject
				if (tvArchives.ContextNode.NodeType == EmTreeNodeType.Object)
				{
					if ((tvArchives.ContextNode as EmArchNodeBase).DeviceType == EmDeviceType.EM33T ||
						(tvArchives.ContextNode as EmArchNodeBase).DeviceType == EmDeviceType.EM31K ||
						(tvArchives.ContextNode as EmArchNodeBase).DeviceType == EmDeviceType.EM33T1)
					{
						// determinating count of measure types
						int iNumOfMeasureTypes = tvArchives.ContextNode.Nodes.Count;
						// and then trying to find EmTreeNodeDBMeasureType with MeasureType equals to PQP
						for (int i = 0; i < iNumOfMeasureTypes; i++)
						{
							switch ((tvArchives.ContextNode.Nodes[i] as EmTreeNodeDBMeasureType).MeasureType)
							{
								case MeasureType.PQP:
									OpenSubArchive(ArchiveType.PQP, tvArchives.ContextNode.Nodes[i].Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
									break;
								case MeasureType.DNS:
									OpenSubArchive(ArchiveType.DNS, tvArchives.ContextNode.Nodes[i].Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
									break;
								case MeasureType.AVG:
									for (int iAvg = 0;
										iAvg < tvArchives.ContextNode.Nodes[i].Nodes.Count; ++iAvg)
									{
										for (int iArch = 0;
											iArch < tvArchives.ContextNode.Nodes[i].Nodes[iAvg].Nodes.Count;
											++iArch)
										{
											OpenSubArchive(ArchiveType.AVG,
												tvArchives.ContextNode.Nodes[i].Nodes[iAvg].Nodes[iArch] as EmTreeNodeDBMeasureBase, withTR);
										}
									}
									//isAVGShown = true;
									break;
							}
						}
					}
					if ((tvArchives.ContextNode as EmArchNodeBase).DeviceType == EmDeviceType.ETPQP)
					{
						// and then trying to find EmTreeNodeDBMeasureType with MeasureType equals to PQP
						if (tvArchives.ContextNode.Nodes.Count > 0)  //year
						{
							if (tvArchives.ContextNode.Nodes[0].Nodes.Count > 0)   // month
							{
								for (int iType = 0;
									iType < tvArchives.ContextNode.Nodes[0].Nodes[0].Nodes.Count; iType++)
								{
									EmTreeNodeDBMeasureType curTypeNode =
										tvArchives.ContextNode.Nodes[0].Nodes[0].Nodes[iType] as
											EmTreeNodeDBMeasureType;
									switch (curTypeNode.MeasureType)
									{
										case MeasureType.PQP:
											OpenSubArchive(ArchiveType.PQP, curTypeNode.Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
											break;
										case MeasureType.DNS:
											OpenSubArchive(ArchiveType.DNS, curTypeNode.Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
											break;
										case MeasureType.AVG:
											if (curTypeNode.Nodes.Count > 0)
											{
												OpenSubArchive(ArchiveType.AVG, curTypeNode.Nodes[0].Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
											}
											//isAVGShown = true;
											break;
									}
								}
							}
						}
					}
				}

				else if (tvArchives.ContextNode.NodeType == EmTreeNodeType.Registration)
				{
					// determinating count of measure types
					int iNumOfMeasureTypes = tvArchives.ContextNode.Nodes.Count;
					// and then trying to find EmTreeNodeDBMeasureType with MeasureType equals to PQP
					for (int i = 0; i < iNumOfMeasureTypes; i++)
					{
						switch ((tvArchives.ContextNode.Nodes[i] as EmTreeNodeDBMeasureType).MeasureType)
						{
							case MeasureType.PQP:
								OpenSubArchive(ArchiveType.PQP, tvArchives.ContextNode.Nodes[i].Nodes[0] as
									EmTreeNodeDBMeasureBase, withTR);
								break;
							case MeasureType.DNS:
								OpenSubArchive(ArchiveType.DNS, tvArchives.ContextNode.Nodes[i].Nodes[0] as
									EmTreeNodeDBMeasureBase, withTR);
								break;
							case MeasureType.AVG:
								for (int iAvg = 0;
									iAvg < tvArchives.ContextNode.Nodes[i].Nodes.Count; ++iAvg)
								{
									for (int iArch = 0;
										iArch < tvArchives.ContextNode.Nodes[i].Nodes[iAvg].Nodes.Count;
										++iArch)
									{
										OpenSubArchive(ArchiveType.AVG,
											tvArchives.ContextNode.Nodes[i].Nodes[iAvg].Nodes[iArch] as
												EmTreeNodeDBMeasureBase, withTR);
									}
								}
								//isAVGShown = true;
								break;
						}
					}
				}

					// if ContextNode is EmTreeNodeDBMeasureType
				else if (tvArchives.ContextNode.NodeType == EmTreeNodeType.MeasureGroup)
				{
					switch ((tvArchives.ContextNode as EmTreeNodeDBMeasureType).MeasureType)
					{
						case MeasureType.PQP:
							OpenSubArchive(ArchiveType.PQP, tvArchives.ContextNode.Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
							break;
						case MeasureType.DNS:
							OpenSubArchive(ArchiveType.DNS, tvArchives.ContextNode.Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
							break;
						case MeasureType.AVG:
							OpenSubArchive(ArchiveType.AVG, tvArchives.ContextNode.Nodes[0].Nodes[0] as EmTreeNodeDBMeasureBase, withTR);
							//isAVGShown = true;
							break;
					}
				}

				// if ContextNode is DВMeasureTreeNode then we are looking to it's parent
				// because DВMeasureTreeNode itself has no any information about kind(type)
				// of measures
				else if (tvArchives.ContextNode.NodeType == EmTreeNodeType.Measure)
				{
					EmTreeNodeDBMeasureType parentMeasureType;
					if (tvArchives.ContextNode.Parent is EmTreeNodeDBMeasureType) // for PQP, DNS
						parentMeasureType = (EmTreeNodeDBMeasureType)tvArchives.ContextNode.Parent;
					else  // for AVG
						parentMeasureType = (EmTreeNodeDBMeasureType)tvArchives.ContextNode.Parent.Parent;

					switch (parentMeasureType.MeasureType)
					{
						case MeasureType.PQP:
							OpenSubArchive(ArchiveType.PQP, tvArchives.ContextNode as EmTreeNodeDBMeasureBase, withTR);
							break;
						case MeasureType.DNS:
							OpenSubArchive(ArchiveType.DNS, tvArchives.ContextNode as EmTreeNodeDBMeasureBase, withTR);
							break;
						case MeasureType.AVG:
							OpenSubArchive(ArchiveType.AVG, tvArchives.ContextNode as EmTreeNodeDBMeasureBase, withTR);
							//isAVGShown = true;
							break;
					}
				}
				this.mainWindow_.Cursor = Cursors.Default;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in _openArchive():");
				throw;
			}
		}

		/// <summary>
		/// Открытие архива на основании данных
		/// о записи открываемого времени Средних
		/// </summary>
		private void OpenSubArchive(ArchiveType archType, EmTreeNodeDBMeasureBase curNode, bool withTR)
		{
			try
			{
				// if object must be opend is already opened - return
				if (this.ActiveNodeAVG == curNode || this.ActiveNodeDNS == curNode || this.ActiveNodePQP == curNode)
				{
					return;
				}

				ArchiveInfo prevArchive = mainWindow_.CurrentArchive;
				// получаем id родительского объекта
				long newArchiveId = GetParentId(curNode);
				// проверяем был ли уже отркыт этот архив
				if (prevArchive != null &&
					(prevArchive.DevType != curNode.DeviceType || prevArchive.ObjectId != newArchiveId))
				{
					// до этого был открыт другой архив, поэтому закрываем его
					mainWindow_.CloseArchive();
				}
				else
				{
					// если мы попали сюда, значит был открыт другой суб-архив того же самого архива. тогда
					// архив закрывать не надо, надо закрыть только предыдущий суб-архив
					if (archType == ArchiveType.AVG) 
						if(tvArchives.ActiveNodeAVG != null) tvArchives.ActiveNodeAVG.Deactivate();
					if (archType == ArchiveType.PQP)
						if (tvArchives.ActiveNodePQP != null) tvArchives.ActiveNodePQP.Deactivate();
					if (archType == ArchiveType.DNS)
						if (tvArchives.ActiveNodeDNS != null) tvArchives.ActiveNodeDNS.Deactivate();
				}

				// передаем главному окну инфу об архиве, если он еще не открыт
				if (mainWindow_.CurrentArchive == null)
				{
					if (curNode.DeviceType == EmDeviceType.EM33T ||
					curNode.DeviceType == EmDeviceType.EM33T1 ||
					curNode.DeviceType == EmDeviceType.EM31K)
					{
						mainWindow_.SetCurrentArchive(
							(curNode.ParentObject as EmTreeNodeObject).ObjectName,  //object name
							(curNode.ParentObject as EmTreeNodeObject).StartDateTime,
							(curNode.ParentObject as EmTreeNodeObject).EndDateTime,
							(curNode.ParentObject as EmTreeNodeObject).ConnectionScheme,
							(curNode.ParentObject as EmTreeNodeObject).NominalLinearVoltage,
							(curNode.ParentObject as EmTreeNodeObject).NominalPhaseVoltage,
							(curNode.ParentObject as EmTreeNodeObject).NominalFrequency,
							(curNode as EmTreeNodeDBMeasureClassic).AvgType,
							(curNode.ParentObject as EmTreeNodeObject).ObjectId,
							withTR,
							curNode.ConstraintType,
							curNode.T_fliker,
							(curNode as EmTreeNodeDBMeasureClassic).MlStartDateTime1,
							(curNode as EmTreeNodeDBMeasureClassic).MlEndDateTime1,
							(curNode as EmTreeNodeDBMeasureClassic).MlStartDateTime2,
							(curNode as EmTreeNodeDBMeasureClassic).MlEndDateTime2,
							curNode.DevVersion,
							curNode.DeviceType,
							curNode.PgServerIndex);
					}
					else if (curNode.DeviceType == EmDeviceType.EM32)
					{
						mainWindow_.SetCurrentArchive(
							(curNode.ParentDevice as EmTreeNodeEm32Device).ObjectName,
							curNode.StartDateTime,
							curNode.EndDateTime,
							(curNode.ParentDevice as EmTreeNodeEm32Device).ConnectionScheme,
							(curNode.ParentDevice as EmTreeNodeEm32Device).NominalLinearVoltage,
							(curNode.ParentDevice as EmTreeNodeEm32Device).NominalPhaseVoltage,
							(curNode.ParentDevice as EmTreeNodeEm32Device).NominalFrequency,
							(curNode as EmTreeNodeDBMeasureClassic).AvgType,
							(curNode.ParentDevice as EmTreeNodeEm32Device).DeviceId,
							withTR,
							curNode.ConstraintType,
							curNode.T_fliker,
							(curNode as EmTreeNodeDBMeasureClassic).MlStartDateTime1,
							(curNode as EmTreeNodeDBMeasureClassic).MlEndDateTime1,
							(curNode as EmTreeNodeDBMeasureClassic).MlStartDateTime2,
							(curNode as EmTreeNodeDBMeasureClassic).MlEndDateTime2,
							curNode.DevVersion,
							curNode.DeviceType,
							curNode.PgServerIndex);
					}
					else if (curNode.DeviceType == EmDeviceType.ETPQP)
					{
						EmTreeNodeObject parentObj = curNode.ParentObject as EmTreeNodeObject;
						mainWindow_.SetCurrentArchive(
							parentObj.ObjectName,
							parentObj.StartDateTime,
							parentObj.EndDateTime,
							parentObj.ConnectionScheme,
							parentObj.NominalLinearVoltage,
							parentObj.NominalPhaseVoltage,
							parentObj.NominalFrequency,
							(curNode as EmTreeNodeDBMeasureClassic).AvgType,
							parentObj.ObjectId,
							withTR,
							curNode.ConstraintType,
							curNode.T_fliker,
							(curNode as EmTreeNodeDBMeasureClassic).MlStartDateTime1,
							(curNode as EmTreeNodeDBMeasureClassic).MlEndDateTime1,
							(curNode as EmTreeNodeDBMeasureClassic).MlStartDateTime2,
							(curNode as EmTreeNodeDBMeasureClassic).MlEndDateTime2,
							curNode.DevVersion,
							curNode.DeviceType,
							curNode.PgServerIndex);
					}
					else if (curNode.DeviceType == EmDeviceType.ETPQP_A)
					{
						EmTreeNodeRegistration parentReg = curNode.ParentObject as EmTreeNodeRegistration;
						mainWindow_.SetCurrentArchive(
							parentReg.ObjectName,
							parentReg.StartDateTime,
							parentReg.EndDateTime,
							parentReg.ConnectionScheme,
							parentReg.NominalLinearVoltage,
							parentReg.NominalPhaseVoltage,
							parentReg.NominalFrequency,
							(curNode as EmTreeNodeDBMeasureEtPQP_A).AvgType_PQP_A,
							parentReg.RegistrationId,
							curNode.ConstraintType,
							curNode.T_fliker,
							curNode.DevVersion,
							curNode.PgServerIndex,
							parentReg.ILimit, parentReg.ULimit);
					}
				}

				// теперь занимаемся суб-архивом
				switch (archType)
				{
					case ArchiveType.AVG: tvArchives.ActiveNodeAVG = curNode; break;
					case ArchiveType.PQP: tvArchives.ActiveNodePQP = curNode; break;
					case ArchiveType.DNS: tvArchives.ActiveNodeDNS = curNode; break;
				}
				curNode.Activate();
				if (curNode.DeviceType != EmDeviceType.EM32)
				{
					curNode.ParentObject.Activate();
					tvArchives.ActiveArchiveNode = curNode.ParentObject;
				}
				else
				{
					curNode.ParentDevice.Activate();
					tvArchives.ActiveArchiveNode = curNode.ParentDevice;
				}

				// enshure visible
				curNode.EnsureVisible();

				// открываем суб-архив
				mainWindow_.SetCurrentSubArchive(archType, curNode.Id, curNode.StartDateTime, curNode.EndDateTime);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in OpenSubArchive(): ");
				throw;
			}
		}

		// ф-я возвращает id родительского объекта (объекта, прибора или регистрации в зависимости от типа устройства)
		private long GetParentId(EmTreeNodeDBMeasureBase curNode)
		{
			try
			{
				if (curNode == null) return -1;

				if (curNode.DeviceType == EmDeviceType.EM32)
					return (curNode.ParentDevice as EmTreeNodeEm32Device).DeviceId;
				else if (curNode.DeviceType == EmDeviceType.ETPQP_A)
					return (curNode.ParentObject as EmTreeNodeRegistration).RegistrationId;
				else return (curNode.ParentObject as EmTreeNodeObject).ObjectId;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetParentId(): ");
				throw;
			}
		}

		#endregion

		#region Node's context menu events and open archive

		private void tvArchives_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenArchive(false);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in tvArchives_DoubleClick(): ");
				throw;
			}
		}

		private void msCreateNewPgSrv_Click(object sender, EventArgs e)
		{
			try
			{
				frmPgServerAddNew wndPgServerAddNew = new frmPgServerAddNew(false);
				if (wndPgServerAddNew.ShowDialog(this) != DialogResult.OK) return;

				EmTreeNodeServer node = new EmTreeNodeServer(
					wndPgServerAddNew.Host,
					wndPgServerAddNew.Port,
					wndPgServerAddNew.ServerName);
				tvArchives.Nodes.Add(node);

				EmDataSaver.PgServerItem newServer = new EmDataSaver.PgServerItem();
				newServer.PgHost = node.PgHost;
				newServer.PgPort = node.PgPort;
				newServer.PgServerName = node.PgServerName;

				List<EmDataSaver.PgServerItem> pgSrvList = new List<EmDataSaver.PgServerItem>();
				if (settings_.PgServers != null && settings_.PgServers.Length > 0) 
					pgSrvList.AddRange(settings_.PgServers);
				pgSrvList.Add(newServer);
				settings_.PgServers = pgSrvList.ToArray();

				settings_.SaveSettings();
				mainWindow_.SyncronizeSettings();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in msCreateNewPgSrv_Click(): ");
				throw;
			}
		}

		private void mmArchOptions_Click(object sender, EventArgs e)
		{
			try
			{
				if (tvArchives.ContextNodeType == EmTreeNodeType.PgServer)
				{
					frmPgServerAddNew wndOptions = new frmPgServerAddNew(true);
					wndOptions.ServerName = settings_.PgServers[tvArchives.ContextNode.Index].PgServerName;
					wndOptions.Host = settings_.PgServers[tvArchives.ContextNode.Index].PgHost;
					wndOptions.Port = settings_.PgServers[tvArchives.ContextNode.Index].PgPort;

					if (wndOptions.ShowDialog() != DialogResult.OK) return;
					(tvArchives.ContextNode as EmTreeNodeServer).PgServerName = wndOptions.ServerName;
					(tvArchives.ContextNode as EmTreeNodeServer).PgHost = wndOptions.Host;
					(tvArchives.ContextNode as EmTreeNodeServer).PgPort = wndOptions.Port;
					tvArchives.ContextNode.Text = (tvArchives.ContextNode as EmTreeNodeServer).PgVisibleServerName;

					settings_.PgServers[tvArchives.ContextNode.Index].PgHost = wndOptions.Host;
					settings_.PgServers[tvArchives.ContextNode.Index].PgPort = wndOptions.Port;
					settings_.PgServers[tvArchives.ContextNode.Index].PgServerName = wndOptions.ServerName;

					settings_.SaveSettings();
					mainWindow_.SyncronizeSettings();
				}
				else
					tvArchives.ShowArchOptions();
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in mmArchOptions_Click(): " + emx.Message);
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in mmArchOptions_Click():");
				throw;
			}
		}

		private void msArchConnect_Click(object sender, EventArgs e)
		{
			try
			{
				if (tvArchives.ContextNodeType == EmTreeNodeType.PgServer)
				{
					ConnectServerAndLoadData((tvArchives.ContextNode as EmTreeNodeServer).Index, false);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in mmArchConnect_Click():");
				throw;
			}
		}

		private void msArchDisconnect_Click(object sender, EventArgs e)
		{
			try
			{
				tvArchives.ContextNode.Nodes.Clear();

				if (tvArchives.ContextNode.NodeType == EmTreeNodeType.PgServer)
				{
					connectedServerIndexes_.Remove((tvArchives.ContextNode as EmTreeNodeServer).Index);
					(tvArchives.ContextNode as EmTreeNodeServer).Disconnect();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in mmArchDisconnect_Click():");
				throw;
			}
		}

		/// <summary>Cut node to the buffer</summary>
		private void msCut_Click(object sender, EventArgs e)
		{
			tvArchives.CutFolder();
		}

		/// <summary>Paste node from the buffer</summary>
		private void msPaste_Click(object sender, EventArgs e)
		{
			tvArchives.PasteFolder();
		}

		/// <summary>Adding new subfolder</summary>
		private void msNewFolder_Click(object sender, EventArgs e)
		{
			try
			{
				tvArchives.CreateNewFolder();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in msNewFolder_Click():");
				throw;
			}
		}

		/// <summary>Expand all inner nodes</summary>
		private void msExpandAll_Click(object sender, EventArgs e)
		{
			tvArchives.ContextNode.ExpandAll();
		}

		/// <summary>Open archive</summary>
		private void mmArchOpen_Click(object sender, EventArgs e)
		{
			OpenArchive(false);
		}

		/// <summary>Open archive with Turn Ratios calculations</summary>
		private void msArchOpenTR_Click(object sender, EventArgs e)
		{
			OpenArchive(true);
		}

		/// <summary>Delete database/folder</summary>
		private void mmArchDelete_Click(object sender, EventArgs e)
		{
			if (MessageBoxes.DeleteConfirmation(this) != DialogResult.OK)
				return;

			try
			{
				if (tvArchives.ContextNodeType == EmTreeNodeType.PgServer)
				{
					List<EmDataSaver.PgServerItem> pgSrvList = new List<EmDataSaver.PgServerItem>();
					if (settings_.PgServers == null) return;
					if (settings_.PgServers.Length == 0) return;

					pgSrvList.AddRange(settings_.PgServers);
					if (tvArchives.ContextNode.PgServerIndex < pgSrvList.Count)
						pgSrvList.RemoveAt(tvArchives.ContextNode.PgServerIndex);
					settings_.PgServers = pgSrvList.ToArray();

					settings_.SaveSettings();
					mainWindow_.SyncronizeSettings();

					//tvArchives.ContextNode.Remove();
					//return;
				}
                tvArchives.DeleteFolder();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in mmArchDelete_Click():");
				throw;
			}
		}
		
		/// <summary>Rename folder</summary>
		private void mmArchRename_Click(object sender, System.EventArgs e)
		{
			try
			{
				if (sender is MenuItem)
					tvArchives.RenamingNode = tvArchives.ContextNode;
				else
					tvArchives.RenamingNode = (EmTreeNodeBase)tvArchives.SelectedNode;

				TextBox t = new TextBox();
				t.Parent = this;
				t.MaxLength = 30;
				t.BorderStyle = BorderStyle.FixedSingle;
				t.Text = tvArchives.RenamingNode.Text;
				t.Top = tvArchives.RenamingNode.Bounds.Top;
				t.Left = tvArchives.RenamingNode.Bounds.Left;
				t.Width = tvArchives.RenamingNode.Bounds.Width + 10;
				t.Height = tvArchives.RenamingNode.Bounds.Height - 6;

				t.Show();
				t.Focus();
				t.KeyDown += new KeyEventHandler(t_KeyDown);
				t.Leave += new EventHandler(t_Leave);
				t.LostFocus += new EventHandler(t_Leave);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in mmArchRename_Click(): " + ex.Message);
				throw;
			}
		}

		/// <summary>Export inner nodes</summary>
		void msArchExport_Click(object sender, EventArgs e)
		{
			try
			{
				EmDeviceType devType;
				EmSqlDataNodeType[] parts;
                tvArchives.ExportArchive(out parts, out devType);

				if (parts != null && parts.Length > 0)
				{
					switch (devType)
					{
						case EmDeviceType.EM31K:
						case EmDeviceType.EM33T:
						case EmDeviceType.EM33T1:
							mainWindow_.StartExportArchive(devType, 
								tvArchives.ContextNode.PgServerIndex,
								(tvArchives.ContextNode as EmTreeNodeObject).ObjectId, 0, parts,
								EmService.GetValidFileName((tvArchives.ContextNode as
									EmTreeNodeObject).Text));
							break;
						case EmDeviceType.EM32:
							mainWindow_.StartExportArchive(EmDeviceType.EM32, 
								tvArchives.ContextNode.PgServerIndex,
								(tvArchives.ContextNode as EmTreeNodeEm32Device).DeviceId, 0, parts,
								EmService.GetValidFileName((tvArchives.ContextNode as
									EmTreeNodeEm32Device).Text));
							break;
						case EmDeviceType.ETPQP:
							EmTreeNodeEtPQPDevice parentDev =
								(tvArchives.ContextNode as EmArchNodeBase).ParentDevice as
									EmTreeNodeEtPQPDevice;
							if (parentDev == null) 
								throw new EmException("Unable to get parent device!");
							mainWindow_.StartExportArchive(EmDeviceType.ETPQP, 
									tvArchives.ContextNode.PgServerIndex,
									parentDev.DeviceId,
									(tvArchives.ContextNode as EmTreeNodeObject).ObjectId,
									parts,
									EmService.GetValidFileName(
										(tvArchives.ContextNode as EmTreeNodeObject).Text));
							break;

						case EmDeviceType.ETPQP_A:
							EmTreeNodeEtPQP_A_Device parentDevA =
								(tvArchives.ContextNode as EmArchNodeBase).ParentDevice as
									EmTreeNodeEtPQP_A_Device;
							if (parentDevA == null)
								throw new EmException("Unable to get parent device!");
							mainWindow_.StartExportArchive(EmDeviceType.ETPQP_A,
									tvArchives.ContextNode.PgServerIndex,
									parentDevA.DeviceId,
									(tvArchives.ContextNode as EmTreeNodeRegistration).RegistrationId,
									parts,
									EmService.GetValidFileName(
										(tvArchives.ContextNode as EmTreeNodeRegistration).Text));
							break;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in mmArchExport_Click(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#region Drag'n'Drop
		/// <summary>
		/// Start dragging node
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.ItemDragEventArgs</param>
		private void tvArchives_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
		{
			try
			{
				if(!(e.Item is EmArchNodeBase))
					return;

				// Get drag node
				dragNode_ = (EmArchNodeBase)e.Item;

				if (dragNode_.NodeType == EmTreeNodeType.MeasureGroup ||
					dragNode_.NodeType == EmTreeNodeType.Measure ||
					dragNode_.NodeType == EmTreeNodeType.PgServer ||
					dragNode_.NodeType == EmTreeNodeType.YearFolder ||
					dragNode_.NodeType == EmTreeNodeType.MonthFolder ||
					dragNode_.NodeType == EmTreeNodeType.DeviceFolder ||
					dragNode_.NodeType == EmTreeNodeType.AvgGroup)
					return;

				// Select drag node
				tvArchives.SelectedNode = dragNode_;

				// Reset image list used for drag image
				this.imageListDrag.Images.Clear();
				int width = dragNode_.Bounds.Width + this.tvArchives.Indent;
				if (width > 256) width = 256;
				int height = dragNode_.Bounds.Height;
				if (height > 256) height = 256;
				this.imageListDrag.ImageSize = new Size(width, height);

				// Create new bitmap
				// This bitmap will contain the tree node image to be dragged
				Bitmap bmp = new Bitmap(dragNode_.Bounds.Width + tvArchives.Indent, 
										dragNode_.Bounds.Height);

				// Get graphics from bitmap
				Graphics gfx = Graphics.FromImage(bmp);

				// Draw node icon into the bitmap
				gfx.DrawImage(this.ilArchives.Images[((TreeNode)e.Item).ImageIndex < 0 ? 0 : ((TreeNode)e.Item).ImageIndex], 0, 0);

				// Draw node label into bitmap
				gfx.DrawString(dragNode_.Text,
					tvArchives.Font,
					new SolidBrush(tvArchives.ForeColor),
					(float)tvArchives.Indent, 1.0f);

				// Add bitmap to imagelist
				this.imageListDrag.Images.Add(bmp);

				// Get mouse position in client coordinates
				Point p = tvArchives.PointToClient(Control.MousePosition);

				// Compute delta between mouse position and node bounds
				int dx = p.X + tvArchives.Indent - dragNode_.Bounds.Left;
				int dy = p.Y - dragNode_.Bounds.Top;

				if (DragHelper.ImageList_BeginDrag(this.imageListDrag.Handle, 0, dx, dy))
				{
					// Begin dragging
					tvArchives.DoDragDrop(bmp, DragDropEffects.Move);
					// End dragging image
					DragHelper.ImageList_EndDrag();
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_ItemDrag(): " + ex.Message);
				throw;
			}
		}


		/// <summary>
		/// Drag Ennter event
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.DragEventArgs</param>
		private void tvArchives_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			try
			{
				DragHelper.ImageList_DragEnter(this.tvArchives.Handle, e.X - this.tvArchives.Left,
					e.Y - this.tvArchives.Top);

				// Enable timer for scrolling dragged item
				this.timer.Enabled = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_DragEnter(): " + ex.Message);
				throw;
			}
		}


		/// <summary>
		/// End dragging node
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.DragEventArgs</param>
		private void tvArchives_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			try
			{
				// Unlock updates
				DragHelper.ImageList_DragLeave(tvArchives.Handle);

				// Get drop node
				EmTreeNodeBase dropNode = 
					(EmTreeNodeBase)tvArchives.GetNodeAt(tvArchives.PointToClient(new Point(e.X, e.Y)));
				if (!(dropNode is EmArchNodeBase))
					return;

				// If drop node isn't equal to drag node, add drag node as child of drop node
				if (dragNode_ != dropNode && dragNode_.Parent != dropNode &&
					dragNode_.DeviceType == ((EmArchNodeBase)dropNode).DeviceType)
				{
					EmArchNodeBase dropNodeArchNode = (dropNode as EmArchNodeBase);
					tvArchives.InsertFolder(ref dragNode_, ref dropNodeArchNode);

					// Set drag node to null
					this.dragNode_ = null;

					// Disable scroll timer
					this.timer.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_DragDrop(): " + ex.Message);
				throw;
			}
		}


		/// <summary>
		/// Drag leave event
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.EventArgs</param>
		private void tvArchives_DragLeave(object sender, System.EventArgs e)
		{
			try
			{
				DragHelper.ImageList_DragLeave(this.tvArchives.Handle);

				// Disable timer for scrolling dragged item
				this.timer.Enabled = false;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_DragLeave(): " + ex.Message);
				throw;
			}
		}


		/// <summary>
		/// Here we can choose, can drop node to take dragged node or can't
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.DragEventArgs</param>
		private void tvArchives_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			try
			{
				// Compute drag position and move image
				Point formP = this.PointToClient(new Point(e.X, e.Y));
				DragHelper.ImageList_DragMove(formP.X - this.tvArchives.Left, formP.Y - this.tvArchives.Top);

				// Get actual drop node
				EmTreeNodeBase dropNode = (EmTreeNodeBase)this.tvArchives.GetNodeAt(this.tvArchives.PointToClient(new Point(e.X, e.Y)));
				if (dropNode == null)
				{
					e.Effect = DragDropEffects.None;
					return;
				}

				// if drop folder is a folder expand it
				if (dropNode.NodeType == EmTreeNodeType.Folder) dropNode.Expand();

				e.Effect = DragDropEffects.Move;

				// if mouse is on a new node select it
				if (this.tempDropNode_ != dropNode)
				{
					DragHelper.ImageList_DragShowNolock(false);
					this.tvArchives.SelectedNode = dropNode;
					DragHelper.ImageList_DragShowNolock(true);
					tempDropNode_ = dropNode;
				}

				// Avoid that drop node is child of drag node 
				TreeNode tmpNode = dropNode;
				while (tmpNode.Parent != null)
				{
					if (tmpNode.Parent == dragNode_) e.Effect = DragDropEffects.None;
					tmpNode = tmpNode.Parent;
				}

				// ========== START CHECK ==========
				//

				// If source node and destinaton node are from different PostgreSQL servers
				if (dropNode.PgServerIndex != dragNode_.PgServerIndex)
				{
					e.Effect = DragDropEffects.None;
				}

				if (!(dropNode is EmArchNodeBase))
				{
					e.Effect = DragDropEffects.None;
				}
				else
				{
					if (((EmArchNodeBase)dropNode).DeviceType != dragNode_.DeviceType)
						e.Effect = DragDropEffects.None;
				}

				if(dragNode_.Parent == dropNode)
					e.Effect = DragDropEffects.None;

				// место назначения - папка (папка, внешняя для устройства!)
				if (dropNode.NodeType == EmTreeNodeType.Folder)
				{
					if (dragNode_.DeviceType == EmDeviceType.EM33T ||
						dragNode_.DeviceType == EmDeviceType.EM31K ||
						dragNode_.DeviceType == EmDeviceType.EM33T1)
					{
						if (dragNode_.NodeType != EmTreeNodeType.Folder &&
							dragNode_.NodeType != EmTreeNodeType.Object)
						{
							e.Effect = DragDropEffects.None;
						}
					}
					if (dragNode_.DeviceType == EmDeviceType.EM32)
					{
						if(dragNode_.NodeType != EmTreeNodeType.Folder &&
							dragNode_.NodeType != EmTreeNodeType.EM32Device)
						{
							e.Effect = DragDropEffects.None;
						}
					}
					if (dragNode_.DeviceType == EmDeviceType.ETPQP)
					{
						if (dragNode_.NodeType != EmTreeNodeType.Folder &&
							dragNode_.NodeType != EmTreeNodeType.ETPQPDevice)
						{
							e.Effect = DragDropEffects.None;
						}
					}
					if (dragNode_.DeviceType == EmDeviceType.ETPQP_A)
					{
						if (dragNode_.NodeType != EmTreeNodeType.Folder &&
							dragNode_.NodeType != EmTreeNodeType.Registration)
						{
							e.Effect = DragDropEffects.None;
						}
					}
				}

				// место назначения - папка (папка, внутренняя для устройства!)
				if (dropNode.NodeType == EmTreeNodeType.FolderInDevice)
				{
					if (dragNode_.DeviceType == EmDeviceType.ETPQP)
					{
						if (dragNode_.NodeType != EmTreeNodeType.FolderInDevice &&
							dragNode_.NodeType != EmTreeNodeType.Object)
						{
							e.Effect = DragDropEffects.None;
						}
					}
					if (dragNode_.DeviceType == EmDeviceType.ETPQP_A)
					{
						if (dragNode_.NodeType != EmTreeNodeType.FolderInDevice &&
							dragNode_.NodeType != EmTreeNodeType.Registration)
						{
							e.Effect = DragDropEffects.None;
						}
					}
				}

				if (dropNode.NodeType == EmTreeNodeType.ETPQPDevice)
				{
					if (dragNode_.DeviceType == EmDeviceType.ETPQP)
					{
						if (dragNode_.NodeType != EmTreeNodeType.FolderInDevice &&
							dragNode_.NodeType != EmTreeNodeType.Object)
						{
							e.Effect = DragDropEffects.None;
						}
					}
				}

				if (dropNode.NodeType == EmTreeNodeType.ETPQP_A_Device)
				{
					if (dragNode_.DeviceType == EmDeviceType.ETPQP_A)
					{
						if (dragNode_.NodeType != EmTreeNodeType.FolderInDevice &&
							dragNode_.NodeType != EmTreeNodeType.Registration)
						{
							e.Effect = DragDropEffects.None;
						}
					}
				}

				if (dropNode.NodeType == EmTreeNodeType.DeviceFolder)
				{
					if (dragNode_.NodeType != EmTreeNodeType.Folder &&
						dragNode_.NodeType != EmTreeNodeType.EM32Device &&
						dragNode_.NodeType != EmTreeNodeType.ETPQPDevice)
					{
						e.Effect = DragDropEffects.None;
					}
				}

				if (dropNode.NodeType == EmTreeNodeType.Object ||
					dropNode.NodeType == EmTreeNodeType.Registration ||
					dropNode.NodeType == EmTreeNodeType.MeasureGroup ||
					dropNode.NodeType == EmTreeNodeType.Measure ||
					dropNode.NodeType == EmTreeNodeType.EM32Device ||
					dropNode.NodeType == EmTreeNodeType.YearFolder ||
					dropNode.NodeType == EmTreeNodeType.MonthFolder)
				{
					e.Effect = DragDropEffects.None;
				}
				//
				// ========== END CHECK ==========
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_DragOver(): " + ex.Message);
				throw;
			}
		}


		/// <summary>
		/// I'm really don't know, why it need here...
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.GiveFeedbackEventArgs</param>
		private void tvArchives_GiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
		{
			try
			{
				if (e.Effect == DragDropEffects.Move)
				{
					// Show pointer cursor while dragging
					e.UseDefaultCursors = false;
					this.tvArchives.Cursor = Cursors.Default;
				}
				else e.UseDefaultCursors = true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_GiveFeedback(): " + ex.Message);
				throw;
			}
		}


		/// <summary>
		/// This is for automatic scrolling of nodes in the TreeView
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="e">EventArgs</param>
		private void timer_Tick(object sender, EventArgs e)
		{
			try
			{
				// get node at mouse position
				Point pt = PointToClient(Control.MousePosition);
				TreeNode node = this.tvArchives.GetNodeAt(pt);

				if (node == null) return;

				// if mouse is near to the top, scroll up
				if (pt.Y < 30)
				{
					// set actual node to the upper one
					if (node.PrevVisibleNode != null)
					{
						node = node.PrevVisibleNode;

						// hide drag image
						DragHelper.ImageList_DragShowNolock(false);
						// scroll and refresh
						node.EnsureVisible();
						this.tvArchives.Refresh();
						// show drag image
						DragHelper.ImageList_DragShowNolock(true);

					}
				}
				// if mouse is near to the bottom, scroll down
				else if (pt.Y > this.tvArchives.Size.Height - 30)
				{
					if (node.NextVisibleNode != null)
					{
						node = node.NextVisibleNode;

						DragHelper.ImageList_DragShowNolock(false);
						node.EnsureVisible();
						this.tvArchives.Refresh();
						DragHelper.ImageList_DragShowNolock(true);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in timer_Tick(): " + ex.Message);
				throw;
			}
		}
		#endregion

		#region Node's Context Menu Drawing
		/// <summary>Context menu by mouse</summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.MouseEventArgs</param>
		private void tvArchives_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Right)
				{
					showContextMenu_(new Point(e.X, e.Y));
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_MouseUp(): " + ex.Message);
				throw;
			}
		}

		/// <summary>Showing context menu</summary>
		private void showContextMenu_(Point p)
		{
			try
			{
				if (tvArchives.ContextNode == null)
				{
					cmsOptions.Items[0].Visible = true;		// create new PgServer
					cmsOptions.Items[1].Visible = false;	// connect to PgServer
					cmsOptions.Items[2].Visible = false;	// disconnect from PgServer
					cmsOptions.Items[3].Visible = false;	// open
					cmsOptions.Items[4].Visible = false;	// open w
					cmsOptions.Items[5].Visible = false;	// expand
					cmsOptions.Items[6].Visible = false;	// add folder
					cmsOptions.Items[7].Visible = false;	// -
					cmsOptions.Items[8].Visible = false;	// cut
					cmsOptions.Items[9].Visible = false;	// paste
					cmsOptions.Items[10].Visible = false;	// -
					cmsOptions.Items[11].Visible = false;	// rename
					cmsOptions.Items[12].Visible = false;	// delete
					cmsOptions.Items[13].Visible = false;	// -
					cmsOptions.Items[14].Visible = false;	// export
					cmsOptions.Items[15].Visible = false;	// -
					cmsOptions.Items[16].Visible = false;	// options

					cmsOptions.Show(tvArchives, p);
					return;
				}

				if (tvArchives.ContextNodeType == EmTreeNodeType.PgServer)
				{
					bool bConnected = (tvArchives.ContextNode as EmTreeNodeServer).Connected;
					cmsOptions.Items[0].Visible = false;			// create new PgServer
					cmsOptions.Items[1].Visible = !bConnected;		// connect to PgServer
					cmsOptions.Items[2].Visible = bConnected;		// disconnect from PgServer
					cmsOptions.Items[3].Visible = false;			// open
					cmsOptions.Items[4].Visible = false;			// open with TR
					cmsOptions.Items[5].Visible = bConnected;		// expand all
					cmsOptions.Items[6].Visible = false;			// add folder
					cmsOptions.Items[7].Visible = false;			// -
					cmsOptions.Items[8].Visible = false;			// cut
					cmsOptions.Items[9].Visible = false;			// paste
					cmsOptions.Items[10].Visible = true;			// -
					cmsOptions.Items[11].Visible = false;			// rename
					cmsOptions.Items[12].Visible = true;			// delete
					cmsOptions.Items[13].Visible = false;			// -
					cmsOptions.Items[14].Visible = false;			// export
					cmsOptions.Items[15].Visible = true;			// -
					cmsOptions.Items[16].Visible = true;			// options

					cmsOptions.Show(tvArchives, p);
					return;
				}

				EmDeviceType curDevType = (tvArchives.ContextNode as EmArchNodeBase).DeviceType;

				bool bCanPaste = false;
				if (tvArchives.BufferNode != null)
				{
					if (curDevType == EmDeviceType.EM33T ||
						curDevType == EmDeviceType.EM31K ||
						curDevType == EmDeviceType.EM33T1)
					{
						if (tvArchives.ContextNodeType == EmTreeNodeType.Folder)
						{
							bCanPaste =
							(tvArchives.BufferNode.NodeType == EmTreeNodeType.Object &&
							(tvArchives.ContextNodeType == EmTreeNodeType.Folder ||
							tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder))

							|| (tvArchives.BufferNode.NodeType == EmTreeNodeType.Folder &&
							(tvArchives.ContextNodeType == EmTreeNodeType.Folder ||
							tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder));
						}
					}
					else if (curDevType == EmDeviceType.ETPQP_A)
					{
						bCanPaste =
						(tvArchives.BufferNode.NodeType == EmTreeNodeType.Registration &&
						(tvArchives.ContextNodeType == EmTreeNodeType.FolderInDevice ||
						tvArchives.ContextNodeType == EmTreeNodeType.ETPQPDevice))

						|| (tvArchives.BufferNode.NodeType == EmTreeNodeType.Folder &&
						(tvArchives.ContextNodeType == EmTreeNodeType.Folder ||
						tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder))

						|| (tvArchives.BufferNode.NodeType == EmTreeNodeType.ETPQP_A_Device &&
						(tvArchives.ContextNodeType == EmTreeNodeType.Folder ||
						tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder));
					}
					else if (curDevType == EmDeviceType.EM32)
					{
						bCanPaste =
							(tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder ||
							tvArchives.ContextNodeType == EmTreeNodeType.Folder) &&
							(tvArchives.BufferNode.NodeType == EmTreeNodeType.Folder ||
							tvArchives.BufferNode.NodeType == EmTreeNodeType.EM32Device);
					}
					else if (curDevType == EmDeviceType.ETPQP)
					{
						bCanPaste =
							(tvArchives.BufferNode.NodeType == EmTreeNodeType.Object &&
							(tvArchives.ContextNodeType == EmTreeNodeType.FolderInDevice ||
							tvArchives.ContextNodeType == EmTreeNodeType.ETPQPDevice))

							|| (tvArchives.BufferNode.NodeType == EmTreeNodeType.Folder &&
							(tvArchives.ContextNodeType == EmTreeNodeType.Folder ||
							tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder))

							|| (tvArchives.BufferNode.NodeType == EmTreeNodeType.FolderInDevice &&
							(tvArchives.ContextNodeType == EmTreeNodeType.FolderInDevice ||
							tvArchives.ContextNodeType == EmTreeNodeType.ETPQPDevice))

							|| (tvArchives.BufferNode.NodeType == EmTreeNodeType.ETPQPDevice &&
							(tvArchives.ContextNodeType == EmTreeNodeType.Folder ||
							tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder));
					}

					if (((EmArchNodeBase)tvArchives.BufferNode).DeviceType !=
							((EmArchNodeBase)tvArchives.ContextNode).DeviceType)
						bCanPaste = false;

					if (tvArchives.BufferNode == tvArchives.ContextNode ||
						tvArchives.BufferNode.Parent == tvArchives.ContextNode)
						bCanPaste = false;

					// Avoid that drop node is child of drag node 
					TreeNode tmpNode = tvArchives.ContextNode;
					while (tmpNode.Parent != null)
					{
						if (tmpNode.Parent == tvArchives.BufferNode) bCanPaste = false;
						tmpNode = tmpNode.Parent;
					}
				}

				bool bCanAdd = false;
				if (curDevType == EmDeviceType.EM33T ||
					curDevType == EmDeviceType.EM31K ||
					curDevType == EmDeviceType.EM33T1)
				{
					bCanAdd = (tvArchives.ContextNodeType == EmTreeNodeType.Folder) ||
						(tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder);
				}
				else if (curDevType == EmDeviceType.EM32)
				{
					bCanAdd = (tvArchives.ContextNodeType == EmTreeNodeType.Folder) 
						|| (tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder);
				}
				else if (curDevType == EmDeviceType.ETPQP || curDevType == EmDeviceType.ETPQP_A)
				{
					bCanAdd = (tvArchives.ContextNodeType == EmTreeNodeType.Folder) ||
						(tvArchives.ContextNodeType == EmTreeNodeType.DeviceFolder) ||
						(tvArchives.ContextNodeType == EmTreeNodeType.ETPQPDevice) ||
						(tvArchives.ContextNodeType == EmTreeNodeType.FolderInDevice);
				}

				switch (tvArchives.ContextNodeType)
				{
					case EmTreeNodeType.Folder:
					case EmTreeNodeType.FolderInDevice:
						cmsOptions.Items[0].Visible = false;						// create new PgServer
						cmsOptions.Items[1].Visible = false;						// connect to PgServer
						cmsOptions.Items[2].Visible = false;						// disconnect from PgServer
						cmsOptions.Items[3].Visible = false;						// open
						cmsOptions.Items[4].Visible = false;						// open with TR
						cmsOptions.Items[5].Visible = true;							// expand all
						cmsOptions.Items[6].Visible = bCanAdd;						// add folder
						cmsOptions.Items[7].Visible = true;							// -
						cmsOptions.Items[8].Visible = true;							// cut
						cmsOptions.Items[9].Visible = bCanPaste;					// paste
						cmsOptions.Items[10].Visible = true;						// -
						cmsOptions.Items[11].Visible = false;						// rename
						cmsOptions.Items[12].Visible = true;						// delete
						cmsOptions.Items[13].Visible = false;						// -
						cmsOptions.Items[14].Visible = false;						// export
						cmsOptions.Items[15].Visible = true;						// -
						cmsOptions.Items[16].Visible = true;						// options
						break;

					case EmTreeNodeType.YearFolder:
					case EmTreeNodeType.MonthFolder:
						for (int i = 0; i < cmsOptions.Items.Count; i++)
						{
							cmsOptions.Items[i].Visible = false;
						}
						cmsOptions.Items[5].Visible = true;							// expand all
						cmsOptions.Items[10].Visible = true;						// -
						cmsOptions.Items[12].Visible = true;						// delete
						cmsOptions.Items[15].Visible = true;						// -
						cmsOptions.Items[16].Visible = true;						// options
						break;

					case EmTreeNodeType.DeviceFolder:
						for (int i = 0; i < cmsOptions.Items.Count; i++)
						{
							cmsOptions.Items[i].Visible = false;
						}
						cmsOptions.Items[5].Visible = true;				// expand all
						cmsOptions.Items[6].Visible = true;				// add folder
						cmsOptions.Items[7].Visible = bCanPaste;		// -
						cmsOptions.Items[9].Visible = bCanPaste;		// paste
						break;

					case EmTreeNodeType.EM32Device:
						for (int i = 0; i < cmsOptions.Items.Count; i++)
						{
							cmsOptions.Items[i].Visible = false;
						}
						cmsOptions.Items[5].Visible = true;		// expand all
						cmsOptions.Items[7].Visible = true;		// -
						cmsOptions.Items[8].Visible = true;     // cut
						cmsOptions.Items[10].Visible = true;    // -
						cmsOptions.Items[12].Visible = true;	// delete
						cmsOptions.Items[13].Visible = true;	// -
						cmsOptions.Items[14].Visible = true;	// export
						cmsOptions.Items[15].Visible = true;	// -
						cmsOptions.Items[16].Visible = true;	// options
						break;

					case EmTreeNodeType.Object:
					case EmTreeNodeType.Registration:
						cmsOptions.Items[0].Visible = false;    // create new PgServer
						cmsOptions.Items[1].Visible = false;	// connect to PgServer
						cmsOptions.Items[2].Visible = false;	// disconnect from PgServer
						cmsOptions.Items[3].Visible = true;     // open
						cmsOptions.Items[4].Visible = 
							curDevType != EmDeviceType.ETPQP_A;		// open with TR
						cmsOptions.Items[5].Visible = true;    // expand all
						cmsOptions.Items[6].Visible = false;    // add folder
						cmsOptions.Items[7].Visible = true;     // -
						cmsOptions.Items[8].Visible = false;     // cut
						cmsOptions.Items[9].Visible = false;    // paste
						cmsOptions.Items[10].Visible = false;     // -
						cmsOptions.Items[11].Visible = false;    // rename
						cmsOptions.Items[12].Visible = true;	 // delete
						cmsOptions.Items[13].Visible = true;    // -
						// comment export
						cmsOptions.Items[14].Visible = true;	// export
						//cmsOptions.Items[14].Visible = tvArchives.ContextNodeType != EmTreeNodeType.Registration;
						//cmsOptions.Items[15].Visible = true;    // -
						cmsOptions.Items[15].Visible = tvArchives.ContextNodeType != EmTreeNodeType.Registration;
						cmsOptions.Items[16].Visible = true;	// options
						break;

					case EmTreeNodeType.ETPQPDevice:
					case EmTreeNodeType.ETPQP_A_Device:
						cmsOptions.Items[0].Visible = false;			// create new PgServer
						cmsOptions.Items[1].Visible = false;			// connect to PgServer
						cmsOptions.Items[2].Visible = false;			// disconnect from PgServer
						cmsOptions.Items[3].Visible = false;			// open
						cmsOptions.Items[4].Visible = false;			// open with TR
						cmsOptions.Items[5].Visible = true;				// expand all
						cmsOptions.Items[6].Visible = true;				// add folder
						cmsOptions.Items[7].Visible = false;			// -
						cmsOptions.Items[8].Visible = true;				// cut
						cmsOptions.Items[9].Visible = true;			// paste
						cmsOptions.Items[10].Visible = true;			// -
						cmsOptions.Items[11].Visible = false;			// rename
						cmsOptions.Items[12].Visible = true;			// delete
						cmsOptions.Items[13].Visible = false;			// -
						cmsOptions.Items[14].Visible = false;			// export
						cmsOptions.Items[15].Visible = true;			// -
						cmsOptions.Items[16].Visible = true;			// options	
						break;

					case EmTreeNodeType.MeasureGroup:
						cmsOptions.Items[0].Visible = false;    // create new PgServer
						cmsOptions.Items[1].Visible = false;	// connect to PgServer
						cmsOptions.Items[2].Visible = false;	// disconnect from PgServer
						cmsOptions.Items[3].Visible = true;     // open
						cmsOptions.Items[4].Visible =			// open with TR
							((tvArchives.ContextNode as EmTreeNodeDBMeasureType).MeasureType == MeasureType.AVG) &&
							(curDevType != EmDeviceType.ETPQP_A);
						cmsOptions.Items[5].Visible = false;    // expand all
						cmsOptions.Items[6].Visible = false;    // add folder
						cmsOptions.Items[7].Visible = false;    // -
						cmsOptions.Items[8].Visible = false;    // cut
						cmsOptions.Items[9].Visible = false;    // paste
						cmsOptions.Items[10].Visible = true;     // -
						cmsOptions.Items[11].Visible = false;    // rename
						cmsOptions.Items[12].Visible = true;	 // delete
						cmsOptions.Items[13].Visible = false;   // -
						cmsOptions.Items[14].Visible = false;	// export
						cmsOptions.Items[15].Visible = false;   // -
						cmsOptions.Items[16].Visible = false;	// options					
						break;

					case EmTreeNodeType.Measure:
						cmsOptions.Items[0].Visible = false;    // create new PgServer
						cmsOptions.Items[1].Visible = false;	// connect to PgServer
						cmsOptions.Items[2].Visible = false;	// disconnect from PgServer
						cmsOptions.Items[3].Visible = true;     // open					
						cmsOptions.Items[4].Visible =			// open with TR
							(tvArchives.ContextNode.Parent is EmTreeNodeAvgTypeClassic); // to except EtPQP-A
						cmsOptions.Items[5].Visible = false;    // expand all
						cmsOptions.Items[6].Visible = false;    // add folder
						cmsOptions.Items[7].Visible = false;    // -
						cmsOptions.Items[8].Visible = false;    // cut
						cmsOptions.Items[9].Visible = false;    // paste
						cmsOptions.Items[10].Visible = true;     // -
						cmsOptions.Items[11].Visible = false;    // rename
						cmsOptions.Items[12].Visible = true;	    // delete
						cmsOptions.Items[13].Visible = false;   // -
						cmsOptions.Items[14].Visible = false;	// export
						cmsOptions.Items[15].Visible = false;   // -
						cmsOptions.Items[16].Visible = false;	// options					
						break;

					default:
						for (int i = 0; i < cmsOptions.Items.Count; i++)
						{
							cmsOptions.Items[i].Visible = false;
						}
						break;
				}
				cmsOptions.Show(tvArchives, p);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in showContextMenu_(): ");
				throw;
			}
		}

		/// <summary>Context menu by keyboard</summary>
		/// <param name="sender">object</param>
		/// <param name="e">System.Windows.Forms.KeyEventArgs</param>
		private void tvArchives_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == Keys.Apps && tvArchives.SelectedNode != null)
				{
					tvArchives.ContextNode = (EmTreeNodeBase)tvArchives.SelectedNode;
					showContextMenu_(new Point(tvArchives.ContextNode.Bounds.Left, 
										tvArchives.ContextNode.Bounds.Bottom));

				}
				else if (e.KeyData == Keys.F2 && tvArchives.SelectedNode != null)
				{
					if (((EmTreeNodeBase)tvArchives.SelectedNode).NodeType == EmTreeNodeType.Folder)
					{
						tvArchives.RenamingNode = (EmTreeNodeBase)tvArchives.SelectedNode;
						tvArchives.ContextNode = (EmTreeNodeBase)tvArchives.SelectedNode;
						mmArchRename_Click(sender, EventArgs.Empty);
					}
				}
				else if (e.KeyData == Keys.F7 && tvArchives.SelectedNode != null)
				{
					if (((EmTreeNodeBase)tvArchives.SelectedNode).NodeType == EmTreeNodeType.Folder ||
						((EmTreeNodeBase)tvArchives.SelectedNode).NodeType == EmTreeNodeType.PgServer ||
						((EmTreeNodeBase)tvArchives.SelectedNode).NodeType == EmTreeNodeType.DeviceFolder)
					{
						tvArchives.RenamingNode = (EmTreeNodeBase)tvArchives.SelectedNode;
						tvArchives.ContextNode = (EmTreeNodeBase)tvArchives.SelectedNode;
						msNewFolder_Click(sender, EventArgs.Empty);
					}
				}
				else if (e.KeyData == Keys.Delete && tvArchives.SelectedNode != null)
				{
					tvArchives.ContextNode = (EmTreeNodeBase)tvArchives.SelectedNode;
					if(tvArchives.ContextNode.NodeType != EmTreeNodeType.DeviceFolder)
						mmArchDelete_Click(sender, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_KeyUp(): " + ex.Message);
				throw;
			}
		}

		private void tvArchives_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				tvArchives.ContextNode = (EmTreeNodeBase)tvArchives.SelectedNode;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in tvArchives_AfterSelect(): " + ex.Message);
				throw;
			}

		}

		private void t_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				if(!(tvArchives.RenamingNode is EmArchNodeBase)) return;

				DbService dbService = new DbService(
					//settings_.PgServers[tvArchives.RenamingNode.PgServerIndex].PgConnectionStringEm33);
					GetPgConnectionString((tvArchives.RenamingNode as EmArchNodeBase).DeviceType, 
												tvArchives.RenamingNode.PgServerIndex));
				try 
				{
					dbService.Open();

					string commandText = String.Format(
						"UPDATE folders SET name = '{0}' WHERE folder_id = {1};",
						(sender as TextBox).Text,
						(tvArchives.RenamingNode as EmTreeNodeFolder).FolderId
						);
					int res = dbService.ExecuteNonQuery(commandText, true);
					if (res > 0) this.tvArchives.RenamingNode.Text = (sender as TextBox).Text;
				}
				catch
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return;
				}
				finally
				{
					if (dbService != null) dbService.CloseConnect();
					(sender as TextBox).Dispose();
				}
			}
			else if (e.KeyData == Keys.Escape)
			{
				(sender as TextBox).Dispose();
			}
		}

		private void t_Leave(object sender, EventArgs e)
		{
			(sender as TextBox).Dispose();
		}

		private string GetPgConnectionString(EmDeviceType devType, int PgServerIndex)
		{
			string ConnectString = "";
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
			}
			return ConnectString;
		}

		#endregion

		#region Properties

		public EmArchNodeBase ActiveArchiveNode
		{
			get { return tvArchives.ActiveArchiveNode; }
		}

		public EmArchNodeBase ActiveNodeAVG
		{
			get { return tvArchives.ActiveNodeAVG; }
		}

		public EmArchNodeBase ActiveNodePQP
		{
			get { return tvArchives.ActiveNodePQP; }
		}

		public EmArchNodeBase ActiveNodeDNS
		{
			get { return tvArchives.ActiveNodeDNS; }
		}

		#endregion
	}
}
