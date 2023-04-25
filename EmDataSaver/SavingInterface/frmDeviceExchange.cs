using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Resources;
using System.IO;
using System.Threading;

using EmServiceLib;
using EmDataSaver;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmArchiveTree;
using DeviceIO;

namespace EmDataSaver.SavingInterface
{
	/// <summary>
	/// Summary description for frmDeviceExchange.
	/// </summary>
	public class frmDeviceExchange : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ImageList ilCheckbox;
		private System.ComponentModel.IContainer components;

		private string DEBUG_MODE_CODE = "debug";
		private string strFormInputText = string.Empty;
		public bool DEBUG_MODE_FLAG = false;
		private string pgSrvConnectStr32_;
		private Em32Device device_;
		private DeviceCommonInfoEm33 devInfoEm33_;
		private DeviceCommonInfoEtPQP devInfoEtPQP_;
		private DeviceCommonInfoEtPQP_A devInfoEtPQP_A_;

		private EmDeviceType curDevType_;

		public bool CREATE_IMAGE_ONLY_FLAG = false;
		public string ImageFileName
		{
			get { return txtImagePath.Text; }
		}

		/// <summary>
		/// Settings objects
		/// </summary>
		private ToolTip myToolTip;
		private ContextMenuStrip cmenuForTree;
		private ToolStripMenuItem cmiEditTime;
		private SplitContainer scMain;
		private GroupBox grbArchivesTreeView;
		private CheckBox chbShowExisting;
		public EmDataSaver.SavingInterface.CheckTreeView.DeviceTreeView tvDeviceData;
		private Label lblDebug;
		private GroupBox grbCreateImageOnly;
		private TextBox txtImagePath;
		private Label lblImageFilename;
		private Button btnBrowse;
		private CheckBox chkCreateImageOnly;
		private Button btnOk;
		private Button btnCancel;
		private CheckBox chbSplit;
		private ToolStripMenuItem cmiParameters;

        private bool bStopClose_ = false;

		//public frmDeviceExchange() {}

		/// <summary>Simple constructor</summary>
		/// <param name="portType">Port type</param>
		/// <param name="portSettings">Port settings</param>
		/// <param name="devInfo">DeviceCommonInfo object</param>
		public frmDeviceExchange(ref DeviceIO.DeviceCommonInfoEm33 devInfo)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			chbShowExisting.Visible = false;
			chbSplit.Visible = false;

			curDevType_ = devInfo.DeviceType;

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			CheckTreeView.DeviceTreeView.NothingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_NothingChecked);
			CheckTreeView.DeviceTreeView.SomethingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_SomethingChecked);

			devInfoEm33_ = devInfo;

			tvDeviceData.ImportDataFromContents(ref devInfo);

			txtImagePath.Text = EmService.GetSqlImageFilePathAndName(curDevType_);
			txtImagePath.ReadOnly = true;
			btnBrowse.Enabled = false;
		}

		public frmDeviceExchange(ref DeviceIO.DeviceCommonInfoEtPQP devInfo)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			chbShowExisting.Visible = false;
			chbSplit.Visible = true;

			curDevType_ = EmDeviceType.ETPQP;

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			CheckTreeView.DeviceTreeView.NothingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_NothingChecked);
			CheckTreeView.DeviceTreeView.SomethingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_SomethingChecked);

			devInfoEtPQP_ = devInfo;

			tvDeviceData.ImportDataFromContents(ref devInfo, chbSplit.Checked);

			txtImagePath.Text = EmService.GetSqlImageFilePathAndName(curDevType_);
			txtImagePath.ReadOnly = true;
			btnBrowse.Enabled = false;
		}

		public frmDeviceExchange(ref DeviceIO.DeviceCommonInfoEtPQP_A devInfo)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			chbShowExisting.Visible = false;
			chbSplit.Visible = true;

			curDevType_ = EmDeviceType.ETPQP_A;

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			CheckTreeView.DeviceTreeView.NothingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_NothingChecked);
			CheckTreeView.DeviceTreeView.SomethingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_SomethingChecked);

			devInfoEtPQP_A_ = devInfo;

			tvDeviceData.ImportDataFromContents(ref devInfo, chbSplit.Checked);

			txtImagePath.Text = EmService.GetSqlImageFilePathAndName(curDevType_);
			txtImagePath.ReadOnly = true;
			btnBrowse.Enabled = false;
		}

		/// <summary>Simple constructor for Em32</summary>
		public frmDeviceExchange(ref DeviceIO.Em32Device device, string pgSrvConnectStr)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			chbShowExisting.Visible = true;
			chbSplit.Visible = true;

			curDevType_ = EmDeviceType.EM32;

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			CheckTreeView.DeviceTreeView.NothingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_NothingChecked);
			CheckTreeView.DeviceTreeView.SomethingChecked +=
				new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_SomethingChecked);

			this.pgSrvConnectStr32_ = pgSrvConnectStr;
			this.device_ = device;

			tvDeviceData.ImportDataFromContents(ref device_, chbShowExisting.Checked,
												pgSrvConnectStr, chbSplit.Checked);

			txtImagePath.Text = EmService.GetSqlImageFilePathAndName(curDevType_);
			txtImagePath.ReadOnly = true;
			btnBrowse.Enabled = false;
		}

		public frmDeviceExchange(SqlImage.EmSqlDeviceImage sqlImg)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			chbShowExisting.Visible = false;
			chbSplit.Visible = false;

			this.txtImagePath.Visible = false;
			this.grbCreateImageOnly.Visible = false;
			this.btnBrowse.Visible = false;

			this.tvDeviceData.ImportDataFromSqlImage(sqlImg);
			this.tvDeviceData.EnableMouseChecks = false;

			//int grbNewHeight = -this.grbArchivesTreeView.Top + grbCreateImageOnly.Bottom;
			//this.tvDeviceData.Height += grbNewHeight - this.grbArchivesTreeView.Height;
			//this.grbArchivesTreeView.Height = grbNewHeight;

			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
			this.Text = rm.GetString("name_exchange_window_caption");
			this.grbArchivesTreeView.Text = rm.GetString("name_exchange_window_tree_caption");

			btnOk.Enabled = true;
		}

		public frmDeviceExchange(SqlImage.EtPQPSqlDeviceImage sqlImg)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			chbShowExisting.Visible = false;
			chbSplit.Visible = false;

			this.txtImagePath.Visible = false;
			this.grbCreateImageOnly.Visible = false;
			this.btnBrowse.Visible = false;

			this.tvDeviceData.ImportDataFromSqlImage(sqlImg);
			this.tvDeviceData.EnableMouseChecks = false;

			//int grbNewHeight = -this.grbArchivesTreeView.Top + grbCreateImageOnly.Bottom;
			//this.tvDeviceData.Height += grbNewHeight - this.grbArchivesTreeView.Height;
			//this.grbArchivesTreeView.Height = grbNewHeight;

			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
			this.Text = rm.GetString("name_exchange_window_caption");
			this.grbArchivesTreeView.Text = rm.GetString("name_exchange_window_tree_caption");

			btnOk.Enabled = true;
		}

		public frmDeviceExchange(SqlImage.EtPQP_A_SqlDeviceImage sqlImg)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			chbShowExisting.Visible = false;
			chbSplit.Visible = false;

			this.txtImagePath.Visible = false;
			this.grbCreateImageOnly.Visible = false;
			this.btnBrowse.Visible = false;

			this.tvDeviceData.ImportDataFromSqlImage(sqlImg);
			this.tvDeviceData.EnableMouseChecks = false;

			//int grbNewHeight = -this.grbArchivesTreeView.Top + grbCreateImageOnly.Bottom;
			//this.tvDeviceData.Height += grbNewHeight - this.grbArchivesTreeView.Height;
			//this.grbArchivesTreeView.Height = grbNewHeight;

			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
			this.Text = rm.GetString("name_exchange_window_caption");
			this.grbArchivesTreeView.Text = rm.GetString("name_exchange_window_tree_caption");

			btnOk.Enabled = true;
		}

		public frmDeviceExchange(SqlImage.EmSqlEm32Device sqlImg)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			CheckForIllegalCrossThreadCalls = false;

			chbShowExisting.Visible = false;
			chbSplit.Visible = false;

			this.txtImagePath.Visible = false;
			this.grbCreateImageOnly.Visible = false;
			this.btnBrowse.Visible = false;

			this.tvDeviceData.ImportDataFromSqlImage(sqlImg);
			this.tvDeviceData.EnableMouseChecks = false;

			//int grbNewHeight = -this.grbArchivesTreeView.Top + grbCreateImageOnly.Bottom;
			//this.tvDeviceData.Height += grbNewHeight - this.grbArchivesTreeView.Height;
			//this.grbArchivesTreeView.Height = grbNewHeight;

			ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
			this.Text = rm.GetString("name_exchange_window_caption");
			this.grbArchivesTreeView.Text = rm.GetString("name_exchange_window_tree_caption");

			btnOk.Enabled = true;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDeviceExchange));
			this.scMain = new System.Windows.Forms.SplitContainer();
			this.grbArchivesTreeView = new System.Windows.Forms.GroupBox();
			this.chbSplit = new System.Windows.Forms.CheckBox();
			this.chbShowExisting = new System.Windows.Forms.CheckBox();
			this.tvDeviceData = new EmDataSaver.SavingInterface.CheckTreeView.DeviceTreeView();
			this.ilCheckbox = new System.Windows.Forms.ImageList(this.components);
			this.lblDebug = new System.Windows.Forms.Label();
			this.grbCreateImageOnly = new System.Windows.Forms.GroupBox();
			this.txtImagePath = new System.Windows.Forms.TextBox();
			this.lblImageFilename = new System.Windows.Forms.Label();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.chkCreateImageOnly = new System.Windows.Forms.CheckBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.myToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.cmenuForTree = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cmiEditTime = new System.Windows.Forms.ToolStripMenuItem();
			this.cmiParameters = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
			this.scMain.Panel1.SuspendLayout();
			this.scMain.Panel2.SuspendLayout();
			this.scMain.SuspendLayout();
			this.grbArchivesTreeView.SuspendLayout();
			this.grbCreateImageOnly.SuspendLayout();
			this.cmenuForTree.SuspendLayout();
			this.SuspendLayout();
			// 
			// scMain
			// 
			resources.ApplyResources(this.scMain, "scMain");
			this.scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.scMain.Name = "scMain";
			// 
			// scMain.Panel1
			// 
			resources.ApplyResources(this.scMain.Panel1, "scMain.Panel1");
			this.scMain.Panel1.Controls.Add(this.grbArchivesTreeView);
			this.myToolTip.SetToolTip(this.scMain.Panel1, resources.GetString("scMain.Panel1.ToolTip"));
			// 
			// scMain.Panel2
			// 
			resources.ApplyResources(this.scMain.Panel2, "scMain.Panel2");
			this.scMain.Panel2.Controls.Add(this.lblDebug);
			this.scMain.Panel2.Controls.Add(this.grbCreateImageOnly);
			this.scMain.Panel2.Controls.Add(this.btnOk);
			this.scMain.Panel2.Controls.Add(this.btnCancel);
			this.myToolTip.SetToolTip(this.scMain.Panel2, resources.GetString("scMain.Panel2.ToolTip"));
			this.myToolTip.SetToolTip(this.scMain, resources.GetString("scMain.ToolTip"));
			// 
			// grbArchivesTreeView
			// 
			resources.ApplyResources(this.grbArchivesTreeView, "grbArchivesTreeView");
			this.grbArchivesTreeView.Controls.Add(this.chbSplit);
			this.grbArchivesTreeView.Controls.Add(this.chbShowExisting);
			this.grbArchivesTreeView.Controls.Add(this.tvDeviceData);
			this.grbArchivesTreeView.Name = "grbArchivesTreeView";
			this.grbArchivesTreeView.TabStop = false;
			this.myToolTip.SetToolTip(this.grbArchivesTreeView, resources.GetString("grbArchivesTreeView.ToolTip"));
			// 
			// chbSplit
			// 
			resources.ApplyResources(this.chbSplit, "chbSplit");
			this.chbSplit.Name = "chbSplit";
			this.myToolTip.SetToolTip(this.chbSplit, resources.GetString("chbSplit.ToolTip"));
			this.chbSplit.UseVisualStyleBackColor = true;
			this.chbSplit.Click += new System.EventHandler(this.chbSplit_Click);
			// 
			// chbShowExisting
			// 
			resources.ApplyResources(this.chbShowExisting, "chbShowExisting");
			this.chbShowExisting.Name = "chbShowExisting";
			this.myToolTip.SetToolTip(this.chbShowExisting, resources.GetString("chbShowExisting.ToolTip"));
			this.chbShowExisting.UseVisualStyleBackColor = true;
			this.chbShowExisting.Click += new System.EventHandler(this.chbShowExisting_Click);
			// 
			// tvDeviceData
			// 
			resources.ApplyResources(this.tvDeviceData, "tvDeviceData");
			this.tvDeviceData.HideSelection = false;
			this.tvDeviceData.ImageList = this.ilCheckbox;
			this.tvDeviceData.Name = "tvDeviceData";
			this.myToolTip.SetToolTip(this.tvDeviceData, resources.GetString("tvDeviceData.ToolTip"));
			this.tvDeviceData.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tvDeviceData_MouseClick);
			// 
			// ilCheckbox
			// 
			this.ilCheckbox.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilCheckbox.ImageStream")));
			this.ilCheckbox.TransparentColor = System.Drawing.Color.Transparent;
			this.ilCheckbox.Images.SetKeyName(0, "");
			this.ilCheckbox.Images.SetKeyName(1, "");
			this.ilCheckbox.Images.SetKeyName(2, "");
			// 
			// lblDebug
			// 
			resources.ApplyResources(this.lblDebug, "lblDebug");
			this.lblDebug.ForeColor = System.Drawing.Color.Red;
			this.lblDebug.Name = "lblDebug";
			this.myToolTip.SetToolTip(this.lblDebug, resources.GetString("lblDebug.ToolTip"));
			// 
			// grbCreateImageOnly
			// 
			resources.ApplyResources(this.grbCreateImageOnly, "grbCreateImageOnly");
			this.grbCreateImageOnly.Controls.Add(this.txtImagePath);
			this.grbCreateImageOnly.Controls.Add(this.lblImageFilename);
			this.grbCreateImageOnly.Controls.Add(this.btnBrowse);
			this.grbCreateImageOnly.Controls.Add(this.chkCreateImageOnly);
			this.grbCreateImageOnly.Name = "grbCreateImageOnly";
			this.grbCreateImageOnly.TabStop = false;
			this.myToolTip.SetToolTip(this.grbCreateImageOnly, resources.GetString("grbCreateImageOnly.ToolTip"));
			// 
			// txtImagePath
			// 
			resources.ApplyResources(this.txtImagePath, "txtImagePath");
			this.txtImagePath.Name = "txtImagePath";
			this.myToolTip.SetToolTip(this.txtImagePath, resources.GetString("txtImagePath.ToolTip"));
			// 
			// lblImageFilename
			// 
			resources.ApplyResources(this.lblImageFilename, "lblImageFilename");
			this.lblImageFilename.Name = "lblImageFilename";
			this.myToolTip.SetToolTip(this.lblImageFilename, resources.GetString("lblImageFilename.ToolTip"));
			// 
			// btnBrowse
			// 
			resources.ApplyResources(this.btnBrowse, "btnBrowse");
			this.btnBrowse.Name = "btnBrowse";
			this.myToolTip.SetToolTip(this.btnBrowse, resources.GetString("btnBrowse.ToolTip"));
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// chkCreateImageOnly
			// 
			resources.ApplyResources(this.chkCreateImageOnly, "chkCreateImageOnly");
			this.chkCreateImageOnly.Name = "chkCreateImageOnly";
			this.myToolTip.SetToolTip(this.chkCreateImageOnly, resources.GetString("chkCreateImageOnly.ToolTip"));
			this.chkCreateImageOnly.UseVisualStyleBackColor = true;
			this.chkCreateImageOnly.CheckedChanged += new System.EventHandler(this.chkCreateImageOnly_CheckedChanged);
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.myToolTip.SetToolTip(this.btnOk, resources.GetString("btnOk.ToolTip"));
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.myToolTip.SetToolTip(this.btnCancel, resources.GetString("btnCancel.ToolTip"));
			// 
			// myToolTip
			// 
			this.myToolTip.AutoPopDelay = 8000;
			this.myToolTip.InitialDelay = 1000;
			this.myToolTip.IsBalloon = true;
			this.myToolTip.ReshowDelay = 100;
			this.myToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			// 
			// cmenuForTree
			// 
			resources.ApplyResources(this.cmenuForTree, "cmenuForTree");
			this.cmenuForTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiEditTime,
            this.cmiParameters});
			this.cmenuForTree.Name = "cmenuForTree";
			this.myToolTip.SetToolTip(this.cmenuForTree, resources.GetString("cmenuForTree.ToolTip"));
			// 
			// cmiEditTime
			// 
			resources.ApplyResources(this.cmiEditTime, "cmiEditTime");
			this.cmiEditTime.Name = "cmiEditTime";
			this.cmiEditTime.Click += new System.EventHandler(this.cmiEditTime_Click);
			// 
			// cmiParameters
			// 
			resources.ApplyResources(this.cmiParameters, "cmiParameters");
			this.cmiParameters.Name = "cmiParameters";
			this.cmiParameters.Click += new System.EventHandler(this.cmiParameters_Click);
			// 
			// frmDeviceExchange
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ControlBox = false;
			this.Controls.Add(this.scMain);
			this.KeyPreview = true;
			this.Name = "frmDeviceExchange";
			this.myToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDeviceExchange_FormClosing);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmDeviceExchange_KeyPress);
			this.scMain.Panel1.ResumeLayout(false);
			this.scMain.Panel2.ResumeLayout(false);
			this.scMain.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
			this.scMain.ResumeLayout(false);
			this.grbArchivesTreeView.ResumeLayout(false);
			this.grbArchivesTreeView.PerformLayout();
			this.grbCreateImageOnly.ResumeLayout(false);
			this.grbCreateImageOnly.PerformLayout();
			this.cmenuForTree.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public new DialogResult ShowDialog()
		{
			DialogResult res = base.ShowDialog();
			Environment.CurrentDirectory = EmService.AppDirectory;
			return res;
		}

		private void DeviceTreeView_NothingChecked()
		{
			btnOk.Enabled = false;
		}

		private void DeviceTreeView_SomethingChecked()
		{
			btnOk.Enabled = true;
		}

		private void chkCreateImageOnly_CheckedChanged(object sender, EventArgs e)
		{
			CREATE_IMAGE_ONLY_FLAG = chkCreateImageOnly.Checked;

			txtImagePath.ReadOnly = !CREATE_IMAGE_ONLY_FLAG;
			btnBrowse.Enabled = CREATE_IMAGE_ONLY_FLAG;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
				string filter;
				string defaultExt;
				switch (curDevType_)
				{
					case EmDeviceType.EM32:
						defaultExt = EmSqlImageCreator32.ImageFileExtention;
						filter = string.Format(rm.GetString("dialog_filter"),
								EmSqlImageCreator32.ImageFileExtention); break;
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
					case EmDeviceType.EM31K:
						defaultExt = EmSqlImageCreator33.ImageFileExtention;
						filter = string.Format(rm.GetString("dialog_filter"),
								EmSqlImageCreator33.ImageFileExtention); break;
					case EmDeviceType.ETPQP:
						defaultExt = EtSqlImageCreatorPQP.ImageFileExtention;
						filter = string.Format(rm.GetString("dialog_filter"),
								EtSqlImageCreatorPQP.ImageFileExtention); break;
                    case EmDeviceType.ETPQP_A:
                        defaultExt = EtSqlImageCreatorPQP_A.ImageFileExtention;
                        filter = string.Format(rm.GetString("dialog_filter"),
                                EtSqlImageCreatorPQP_A.ImageFileExtention); break;
					default: throw new EmException("btnBrowse_Click: Unknown device type!");
				}

				string fileName;
				SafeShowDialog safeDlg = new SafeShowDialog(SafeShowDialog.DlgOperation.SAVE,
					EmService.GetSqlImageFileName(curDevType_), 
					EmService.TEMP_IMAGE_DIR, defaultExt, filter);
				if (!safeDlg.Run(out fileName))
					return;

				if (!fileName.Contains(defaultExt.ToLower()))
				{
					int dot = fileName.IndexOf('.');
					fileName = fileName.Substring(0, dot);
					fileName += ("." + defaultExt);
				}

				Environment.CurrentDirectory = EmService.AppDirectory;

				txtImagePath.Text = fileName;
			}
            catch (EmException ex)
            {
                EmService.DumpException(ex, "Error in frmDeviceExchange::btnBrowse_Click ");
            }
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDeviceExchange::btnBrowse_Click ");
				throw;
			}
		}

		private void frmDeviceExchange_KeyPress(object sender, KeyPressEventArgs e)
		{
			strFormInputText += e.KeyChar;
			if (strFormInputText.ToLower().Contains(DEBUG_MODE_CODE))
			{
				DEBUG_MODE_FLAG = true;
				lblDebug.Visible = true;
			}
		}

		private void chbShowExisting_Click(object sender, EventArgs e)
		{
			if (device_ == null) return;

			if (device_.DeviceType == EmDeviceType.EM32)
				tvDeviceData.ImportDataFromContents(ref device_, chbShowExisting.Checked,
					pgSrvConnectStr32_, chbSplit.Checked);
		}

		private void tvDeviceData_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Right)
				{
					// change selected item
					TreeNode curNode = tvDeviceData.GetNodeAt(e.X, e.Y);
					tvDeviceData.SelectedNode = curNode;

					Thread.Sleep(100);

					// show context menu
					if (tvDeviceData.SelectedNode is CheckTreeView.MeasureTreeNode)
					{
						CheckTreeView.MeasureTreeNode contextNode =
							tvDeviceData.SelectedNode as CheckTreeView.MeasureTreeNode;

						if (contextNode.DeviceType == EmDeviceType.EM32 ||
							contextNode.DeviceType == EmDeviceType.ETPQP ||
							contextNode.DeviceType == EmDeviceType.ETPQP_A)
						{
							if (contextNode.MeasureType == MeasureType.PQP)
								return;
							if (contextNode.MeasureType == MeasureType.DNS &&
								contextNode.DeviceType == EmDeviceType.ETPQP_A)
								return;

							if (contextNode.MeasureType == MeasureType.AVG && 
								contextNode.DeviceType != EmDeviceType.ETPQP_A)
								cmiParameters.Visible = true;
							else
								cmiParameters.Visible = false;
							int y = e.Y;//(sender as Control).Location.Y + e.Y;
							y += this.Size.Height - this.ClientRectangle.Height;
							cmenuForTree.Show(this, new Point(e.X, y));
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDeviceExchange::tvDeviceData_MouseClick ");
				//throw;
			}
		}

		private void cmiEditTime_Click(object sender, EventArgs e)
		{
			try
			{
				if (!(tvDeviceData.SelectedNode is CheckTreeView.MeasureTreeNode)) return;

				CheckTreeView.MeasureTreeNode contextNode =
							tvDeviceData.SelectedNode as CheckTreeView.MeasureTreeNode;

				frmEditTimeInterval frmEdit = new frmEditTimeInterval();
				//frmEdit.Location = new Point(cmenuForTree.Left + this.Location.X, 
				//							cmenuForTree.Top + this.Location.Y);
				frmEdit.Location = Cursor.Position;
				frmEdit.DateStart = contextNode.StartDateTime;
				frmEdit.DateEnd = contextNode.EndDateTime;
				frmEdit.MinDateStart = contextNode.OriginDateStart;
				frmEdit.MaxDateEnd = contextNode.OriginDateEnd;

				if (frmEdit.ShowDialog() == DialogResult.OK)
				{
					if (frmEdit.DateStart > frmEdit.DateEnd) return;

					string addText = string.Empty;
                    if (contextNode.Text.Contains("3 sec")) addText = "3 sec";
                    else if (contextNode.Text.Contains("1 min")) addText = "1 min";
                    else if (contextNode.Text.Contains("30 min")) addText = "30 min";
                    else if (contextNode.Text.Contains("10 min")) addText = "10 min";
                    else if (contextNode.Text.Contains("2 hours")) addText = "2 hours";

					contextNode.StartDateTime = frmEdit.DateStart;
					contextNode.EndDateTime = frmEdit.DateEnd;
					contextNode.Text = frmEdit.DateStart.ToString() + " - " +
												frmEdit.DateEnd.ToString();
					if (addText != string.Empty)
						contextNode.Text += ("  " + addText);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDeviceExchange::cmiEditTime_Click ");
				//throw;
			}
		}

		private void cmiParameters_Click(object sender, EventArgs e)
		{
			try
			{
				if (!(tvDeviceData.SelectedNode is CheckTreeView.MeasureTreeNode)) return;

				CheckTreeView.MeasureTreeNode contextNode =
							tvDeviceData.SelectedNode as CheckTreeView.MeasureTreeNode;

				CheckTreeView.ObjectTreeNode parentObj =
					tvDeviceData.GetParentObject(tvDeviceData.SelectedNode);
				frmAvgParams wndAvgParams = new frmAvgParams(parentObj.ConnectionScheme,
						contextNode.DeviceType,
						!contextNode.MasksAvgWasSet);
				if (contextNode.MasksAvgWasSet)
					wndAvgParams.LoadFormState(contextNode.MasksAvg, parentObj.ConnectionScheme);

				if (wndAvgParams.ShowDialog() == DialogResult.OK)
				{
					contextNode.MasksAvg = wndAvgParams.GetMask();
					contextNode.MasksAvgWasSet = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDeviceExchange::cmiParameters_Click ");
				//throw;
			}
		}

		private void chbSplit_Click(object sender, EventArgs e)
		{
			if (curDevType_ == EmDeviceType.EM32 && device_ != null)
				tvDeviceData.ImportDataFromContents(ref device_, chbShowExisting.Checked,
					pgSrvConnectStr32_, chbSplit.Checked);
			else if (curDevType_ == EmDeviceType.ETPQP)
			{
				tvDeviceData.ImportDataFromContents(ref devInfoEtPQP_, chbSplit.Checked);
			}
			else if (curDevType_ == EmDeviceType.ETPQP_A)
			{
				tvDeviceData.ImportDataFromContents(ref devInfoEtPQP_A_, chbSplit.Checked);
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
                DeviceTreeView devTree = tvDeviceData;
                if (devTree.Nodes.Count < 1) return;

				int limitAVGHours = 8;
				if (curDevType_ == EmDeviceType.ETPQP_A) limitAVGHours = 2;

                //????????? проверить для эм32!
                for (int iObj = 0; iObj < devTree.Nodes[0].Nodes.Count; iObj++)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[iObj]).CheckState !=
								CheckState.Unchecked)
					{
						ObjectTreeNode objNodeTmp = (ObjectTreeNode)devTree.Nodes[0].Nodes[iObj];
						foreach (MeasureTypeTreeNode typeNode in objNodeTmp.Nodes)
						{
                            if (typeNode.MeasureType == MeasureType.AVG)
                            {
                                for (int iAvg = 0; iAvg < typeNode.Nodes.Count; iAvg++)
                                {
                                    MeasureTreeNode curMeasureNode =
                                        (typeNode.Nodes[iAvg] as MeasureTreeNode);
                                    if (curMeasureNode.Text.Contains("3 sec") && 
										curMeasureNode.CheckState != CheckState.Unchecked)
                                    {
										TimeSpan ts = curMeasureNode.EndDateTime -
                                            curMeasureNode.StartDateTime;
										if (ts > new TimeSpan(limitAVGHours, 0, 0))
                                        {
                                            if (MessageBoxes.MsgArchiveMoreThanLimit(this, this, limitAVGHours) ==
                                                DialogResult.No)
                                            {
                                                bStopClose_ = true;
                                            }
                                            //реакцию юзера рассматриваем как действительную для
                                            //всех длинных архивов, поэтому выходим
                                            return;
                                        }
                                    }
                                }
                            }
						}
					}
				}		
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in frmDeviceExchange::btnOk_Click ");
				throw;
			}
		}

        private void frmDeviceExchange_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bStopClose_)
            {
                e.Cancel = true;
                bStopClose_ = false;
            }
		}

		#region Properties

		public bool SplitByDays { get { return chbSplit.Checked; } }

		#endregion
	}
}
