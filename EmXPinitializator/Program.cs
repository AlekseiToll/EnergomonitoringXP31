using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;

namespace EmXPinitializer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main( string[] args )
		{
			Application.EnableVisualStyles();
            bool doAutomatic = false;
            if (args.Length > 0 && args[0] == "-auto")
                doAutomatic = true;
			Application.Run(new frmMain( doAutomatic ));
		}
	}
}