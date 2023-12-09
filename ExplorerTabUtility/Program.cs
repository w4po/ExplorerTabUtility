using System.Threading;
using System.Windows.Forms;
using ExplorerTabUtility.Forms;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility;

internal class Program
{
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var mutex = new Mutex(false, Constants.MutexId);
        if (!mutex.WaitOne(0, false))
        {
            MessageBox.Show("Another instance is already running.\nCheck in System Tray Icons.", Constants.AppName);
            return;
        }

        Application.Run(new TrayIcon());
    }
}