using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace EmDataSaver.SavingInterface
{
	// This class helps to show SaveFileDialog and OpenFileDialog objects from
	// a BackgroundWorker thread. Explicit using of those dialogs causes such
	// exception: "Current thread must be set to single thread apartment (STA) 
	// mode before OLE calls can be made."
	internal class SafeShowDialog
	{
		public enum DlgOperation
		{
			OPEN = 0,
			SAVE = 1
		}

		private string fileName_ = "";
		private string initFileName_;
		private string initDirectory_;
		private string defaultExt_;
		private string filter_;
		private bool res_ = false;
		private DlgOperation curOperation_;

		public SafeShowDialog(DlgOperation curOp, string initFileName, string initDir,
			string defExt, string filter)
		{
			curOperation_ = curOp;
			initFileName_ = initFileName;
			initDirectory_ = initDir;
			defaultExt_ = defExt;
			filter_ = filter;
		}

		public bool Run(out string fileName)
		{
			ShowFileDialog();
			fileName = fileName_;
			return res_;
		}

		private void ShowFileDialog()
		{
			Thread t = new Thread(new ThreadStart(ShowFileDialogInit));
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
			t.Join();
		}

		private void ShowFileDialogInit()
		{
			if (curOperation_ == DlgOperation.SAVE)
			{
				SaveFileDialog fd = new SaveFileDialog();
				fd.AddExtension = true;
				fd.FileName = initFileName_;
				fd.DefaultExt = defaultExt_;
				fd.Filter = filter_;

				if (initDirectory_ != string.Empty)
				{
					fd.InitialDirectory = initDirectory_;
					if (!Directory.Exists(fd.InitialDirectory))
					{
						Directory.CreateDirectory(fd.InitialDirectory);
					}
				}

				if (fd.ShowDialog() == DialogResult.OK)
				{
					fileName_ = fd.FileName;
					res_ = true;
				}
			}
			else
			{
				OpenFileDialog fd = new OpenFileDialog();
				if (fd.ShowDialog() == DialogResult.OK)
				{
					fileName_ = fd.FileName;
					res_ = true;
				}
			}
		}
	}
}
