using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winform_App
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            #if DEBUG
            args = new string[] { "false" };//发布时生成release版本的放到服务器，必须要这样
            #endif
            if (args.Length != 0 && args[0] == "false")
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                string currentVersion = "0.0.0.0";
                if (File.Exists("version.txt"))
                {
                    currentVersion = File.ReadAllText("version.txt");
                }
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "Upgrade.exe";//要启动的程序外部名称 
                info.Arguments = "Winform_App.exe " + currentVersion + " SampleProject 5C210AF8-6D4E-4D06-A9B7-F1E04BFE8AC3 http://localhost:56799/";
                info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(info);
                Application.Exit();
            }
        }
    }
}
