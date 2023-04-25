using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.IO;

using EmDataSaver.SqlImage;
using EmServiceLib;

namespace EmDataSaver.SavingInterface
{
	/// <summary>Export details window form</summary>
	public partial class frmExport : System.Windows.Forms.Form
	{
		#region Fields

		private EmSqlDataNodeType[] input_parts = null;
		private bool[] output_parts = null;
		private EmDeviceType curDevType_;
		private string imageFileName_;

		#endregion

		#region Properties
		public EmSqlDataNodeType[] InputArchiveParts
		{
			get { return input_parts; }
			set
			{
				input_parts = value;
				if (input_parts != null)
					if (input_parts.Length != 0)
					{
						output_parts = new bool[input_parts.Length];
						for (int i = 0; i < output_parts.Length; i++)
						{
							output_parts[i] = true;
						}
					}
			}
		}

		public EmSqlDataNodeType[] OutputArchiveParts
		{
			get 
			{
				if (output_parts == null) return null;
				if (output_parts.Length == 0) return null;
				List<EmSqlDataNodeType> temp_list = new List<EmSqlDataNodeType>();
				for (int i = 0; i < output_parts.Length; i++)
					if (output_parts[i]) temp_list.Add(input_parts[i]);
				if (temp_list.Count == 0) return null;
				EmSqlDataNodeType[] temp = new EmSqlDataNodeType[temp_list.Count];
				temp_list.CopyTo(temp);
				return temp;
			}
		}

		public string FileName
		{
			get { return txtImagePath.Text; }
		}

		#endregion

		#region Constructor
		
		public frmExport(EmDeviceType devType, string imageFileName)
		{
			InitializeComponent();
			curDevType_ = devType;
			imageFileName_ = imageFileName;
			imageFileName_ += "." + curDevType_.ToString().ToLower() + ".xml";
		}

		#endregion

		#region Methods

		public new DialogResult ShowDialog()
		{
			DialogResult res = base.ShowDialog();
			Environment.CurrentDirectory = EmService.AppDirectory;
			return res;
		}

		private void frmToolboxExportDetails_Load(object sender, EventArgs e)
		{
			try
			{
				if (input_parts == null) return;
				if (input_parts.Length == 0) return;

				ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);

				for (int i = 0; i < input_parts.Length; i++)
				{
					string item = string.Empty;
					if (input_parts[i] == EmSqlDataNodeType.PQP)
						item = rm.GetString("name_measure_type_pke_full");
					else if (input_parts[i] == EmSqlDataNodeType.AVG)
						item = rm.GetString("name_measure_type_avg_full");
					else if (input_parts[i] == EmSqlDataNodeType.Events)
						item = rm.GetString("name_measure_type_dns_full");
					int index = chklbParts.Items.Add(item, true);
				}

				txtImagePath.Text = EmService.TEMP_IMAGE_DIR;
				if (!Directory.Exists(txtImagePath.Text))
				{
					Directory.CreateDirectory(txtImagePath.Text);
				}
				//txtImagePath.Text += ("export." + curDevType_.ToString().ToLower() + ".xml");
				txtImagePath.Text += imageFileName_;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in frmToolboxExportDetails_Load(): ");
				throw;
			}
		}

		private void chklbParts_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			try
			{
				output_parts[e.Index] = e.NewValue == CheckState.Checked ? true : false;

				bool something_checked = false;
				for (int i = 0; i < output_parts.Length; i++)
				{
					if (output_parts[i])
					{
						something_checked = true;
						break;
					}
				}
				btnOk.Enabled = something_checked;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in frmExport::chklbParts_ItemCheck(): ");
				throw;
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			try
			{
				string filter;
				string defaultExt;

				switch (curDevType_)
				{
					case EmDeviceType.EM33T:
						defaultExt = EmSqlImageCreator33.ImageFileExtention;
						filter = EmSqlImageCreator33.ImageFilter; break;
					case EmDeviceType.EM31K:
						defaultExt = EmSqlImageCreator33.ImageFileExtention31K;
						filter = EmSqlImageCreator33.ImageFilter31K; break;
					case EmDeviceType.EM33T1:
						defaultExt = EmSqlImageCreator33.ImageFileExtention33T1;
						filter = EmSqlImageCreator33.ImageFilter33T1; break;
					case EmDeviceType.EM32:
						defaultExt = EmSqlImageCreator32.ImageFileExtention;
						filter = EmSqlImageCreator32.ImageFilter; break;
					case EmDeviceType.ETPQP:
						defaultExt = EtSqlImageCreatorPQP.ImageFileExtention;
						filter = EtSqlImageCreatorPQP.ImageFilter; break;
					case EmDeviceType.ETPQP_A:
						defaultExt = EtSqlImageCreatorPQP_A.ImageFileExtention;
						filter = EtSqlImageCreatorPQP_A.ImageFilter; break;
					default:
						throw new EmException("frmExport: Unknown device type!");
				}

				string fileName;
				SafeShowDialog safeDlg = new SafeShowDialog(SafeShowDialog.DlgOperation.SAVE,
					imageFileName_, EmService.TEMP_IMAGE_DIR, defaultExt, filter);
				if (!safeDlg.Run(out fileName))
					return;

				if (!fileName.Contains(defaultExt))
				{
					int dot = fileName.IndexOf('.');
					fileName = fileName.Substring(0, dot);
					fileName += ("." + defaultExt);
				}

				txtImagePath.Text = fileName;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in frmExport::btnBrowse_Click(): ");
				throw;
			}
		}

		#endregion
	}
}