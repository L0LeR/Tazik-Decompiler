using System;
using System.Windows.Forms;

namespace TazikDecompiler
{
    internal static class Program
    {

        // Launcher and this is all ¯\_(ツ)_/¯
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
