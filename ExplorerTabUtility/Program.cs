using System;
using System.Threading;
using System.Windows.Forms;
using ExplorerTabUtility.Forms;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility;

internal class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        using var mutex = new Mutex(false, Constants.MutexId);
        if (!mutex.WaitOne(0, false))
        {
            MessageBox.Show("""
                            Another instance is already running.
                            Check in System Tray Icons.
                            """, Constants.AppName);
            return;
        }

        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception e)
        {
            MessageBox.Show($"""
                             Error: {e.Message}
                             
                             {e.StackTrace}
                             """, Constants.AppName);
        }
    }
}