using System;
using System.Windows.Forms;
using pgcal.Core;

namespace pgcal.Gui {
	public class Ppal : Form {
		
		public static void Main()
		{
			Form mainWindow = new MainWindow();
			
			try {
				Application.Run( mainWindow );
			}
			catch(Exception exc)
			{
				Form window = mainWindow;

				if ( window.IsDisposed ) {
					window = null; 
				}

				MessageBox.Show(
					window,
					"Critical ERROR: " + exc.Message,
					AppInfo.Name,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
		}
	}
}

