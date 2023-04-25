using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EmServiceLib;

namespace EmArchiveTree
{
	/// <summary>
	/// TreeNode that contains folder information
	/// </summary>
	public class EmTreeNodeFolder : EmArchNodeBase
	{
		#region Fields

		/// <summary>
		/// Primary key of this folder (field "folder_id") in "folders" table in DB
		/// </summary>
		protected Int64 folderId_;

		/// <summary>Field "folder_info" in "folders" table in DB</summary>
		public string FolderInfo;

		/// <summary>Field "device_id" in "folders" table in DB</summary>
		public Int64 DeviceId;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public EmTreeNodeFolder() : base()
		{
			this.Text = String.Empty;
			this.FolderId = 0;
			this.FolderInfo = String.Empty;

			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		public EmTreeNodeFolder(EmTreeNodeType nodeType, EmDeviceType devType,
			EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(nodeType, devType, parentDev, parentObj)
		{
			this.Text = String.Empty;
			this.FolderId = 0;
			this.FolderInfo = String.Empty;

			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		/// <summary>
		/// Constructor for use
		/// </summary>
		public EmTreeNodeFolder(Int64 FolderId, string FolderName,
								string FolderInfo, EmTreeNodeType nodeType,
								EmDeviceType devType, EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(nodeType, devType, parentDev, parentObj)
		{
			this.Text = FolderName;
			this.FolderId = FolderId;
			this.FolderInfo = FolderInfo;

			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		public EmTreeNodeFolder(Int64 FolderId, string FolderName,
								string FolderInfo, EmTreeNodeType nodeType,
								EmDeviceType devType, Int64 devId,
								EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(nodeType, devType, parentDev, parentObj)
		{
			this.Text = FolderName;
			this.FolderId = FolderId;
			this.FolderInfo = FolderInfo;
			this.DeviceId = devId;

			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method Clone()
		/// </summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeFolder)node).FolderId = this.FolderId;
			((EmTreeNodeFolder)node).FolderInfo = this.FolderInfo;
			((EmTreeNodeFolder)node).DeviceId = this.DeviceId;
			return node;
		}

		#endregion

		#region Properties

		public Int64 FolderId
		{
			get { return folderId_; }
			set { folderId_ = value; }
		}

		#endregion
	}

	/// <summary>Device folder</summary>
	public class EmTreeNodeDeviceFolder : EmArchNodeBase
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public EmTreeNodeDeviceFolder() : base()
		{
			this.Text = "NONE";
			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		public EmTreeNodeDeviceFolder(EmDeviceType devType)
			: base(EmTreeNodeType.DeviceFolder, devType, null, null)
		{
			this.Text = EmService.GetDeviceTypeAsString(devType);
			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method Clone()
		/// </summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			return node;
		}

		#endregion
	}

	/// <summary>Year folder</summary>
	public class EmTreeNodeYearFolder : EmArchNodeBase
	{
		#region Fields

		/// <summary>
		/// Primary key of this folder (field <c>folder_id</c>) in <c>folders</c> table in PostgreSQL database
		/// </summary>
		private Int64 folderId_;

		/// <summary>
		/// Field <c>folder_info</c>) in <c>folders</c> table in PostgreSQL database
		/// </summary>
		public string FolderInfo;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public EmTreeNodeYearFolder()
		{
			this.folderId_ = 0;
			this.Text = "Unknown";
			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		public EmTreeNodeYearFolder(Int64 folderId, EmDeviceType devType, string text,
									EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.YearFolder, devType, parentDev, parentObj)
		{
			this.folderId_ = folderId;
			this.Text = text;
			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		#endregion

		#region Properties

		public Int64 FolderId
		{
			get { return folderId_; }
			set { folderId_ = value; }
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method Clone()
		/// </summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeYearFolder)node).FolderId = this.FolderId;
			((EmTreeNodeYearFolder)node).FolderInfo = this.FolderInfo;
			return node;
		}

		#endregion
	}

	/// <summary>Month folder</summary>
	public class EmTreeNodeMonthFolder : EmArchNodeBase
	{
		#region Fields

		/// <summary>
		/// Primary key of this folder (field <c>folder_id</c>) in <c>folders</c> table in PostgreSQL database
		/// </summary>
		private Int64 folderId_;

		/// <summary>
		/// Field <c>folder_info</c>) in <c>folders</c> table in PostgreSQL database
		/// </summary>
		public string FolderInfo;

		#endregion

		#region Constructors

		/// <summary>Default constructor</summary>
		public EmTreeNodeMonthFolder()
		{
			this.folderId_ = 0;
			this.Text = "Unknown";
			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		public EmTreeNodeMonthFolder(Int64 folderId, EmDeviceType devType, string text,
									EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.MonthFolder, devType, parentDev, parentObj)
		{
			this.folderId_ = folderId;
			this.Text = text;
			// setting images 
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		#endregion

		#region Properties

		public Int64 FolderId
		{
			get { return folderId_; }
			set { folderId_ = value; }
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method Clone()
		/// </summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeMonthFolder)node).FolderId = this.FolderId;
			((EmTreeNodeMonthFolder)node).FolderInfo = this.FolderInfo;
			return node;
		}

		#endregion
	}
}
