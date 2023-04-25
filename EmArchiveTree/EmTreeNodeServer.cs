using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmArchiveTree
{
	public class EmTreeNodeServer : EmTreeNodeBase
	{
		#region Fields

		protected int port_ = 0;
		protected string host_ = string.Empty;
		protected string serverName_ = string.Empty;
		protected bool connected_ = false;

		#endregion

		#region Constructors

		public EmTreeNodeServer() : base(EmTreeNodeType.PgServer) 
		{
			this.ImageIndex = this.SelectedImageIndex = 4;
		}

		public EmTreeNodeServer(string host, int port, string serverName)
			: base(EmTreeNodeType.PgServer)
		{
			this.host_ = host;
			this.port_ = port;
			this.serverName_ = serverName;
			this.Text = this.PgVisibleServerName;
			this.ImageIndex = this.SelectedImageIndex = 4;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets connection port to PostgreSQL server
		/// </summary>
		public int PgPort
		{
			get { return this.port_; }
			set
			{
				if (value > 0 && value < 65536 && value != this.port_)
				{
					this.port_ = value;
					this.Name = this.PgVisibleServerName;
				}
			}
		}

		/// <summary>
		/// Gets or sets PostgreSQL server host
		/// </summary>
		public string PgHost
		{
			get { return this.host_; }
			set
			{
				if (value != this.host_)
				{
					this.host_ = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets PostgreSQL server name
		/// </summary>
		public string PgServerName
		{
			get { return serverName_; }
			set { serverName_ = value; }
		}

		/// <summary>
		/// Is connection with the PostgreSQL server established or not
		/// </summary>
		public bool Connected
		{
			get { return connected_; }
			set
			{
				if (value != connected_)
				{
					connected_ = value;
					if (connected_) this.ImageIndex = this.SelectedImageIndex = 3;
					else this.ImageIndex = this.SelectedImageIndex = 4; ;
				}
			}
		}

		/// <summary>
		/// Gets string of the Node to show in the tree view
		/// </summary>
		public string PgVisibleServerName
		{
			get { return string.Format("{0} ({1}:{2})", this.serverName_, this.host_, this.port_); }
		}

		#endregion

		#region Public Methods

		public void Connect()
		{
			this.Connected = true;
		}

		public void Disconnect()
		{
			this.Connected = false;
		}

		#endregion

		#region Overriden methods

		/// <summary>Overriden method Clone()</summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeServer)node).host_ = this.host_;
			((EmTreeNodeServer)node).port_ = this.port_;
			((EmTreeNodeServer)node).serverName_ = this.serverName_;
			return node;
		}

		#endregion
	}
}
