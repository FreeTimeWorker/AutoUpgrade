using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Upgrade
{
    static class Program
    {
        public static HttpClient client = new HttpClient();
        public static string StartExeName = "";
        /// <summary>
        /// 参数 1，应用程序的入口程序exe名称，2,currentversion，3,项目名称，4，项目密码,5,更新网站地址
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                //读取当前目录下，看看有没有配置文件 init.json
                if (File.Exists("upgrade.init"))
                {
                    string config = File.ReadAllText("upgrade.init");
                    args = config.Split(',');
                    if (args.Length != 5)
                    {
                        MessageBox.Show("upgrade.init文件的格式为: 应用程序的入口程序exe名称,currentversion,项目名称,项目密码,更新网站地址");
                        Application.Exit();
                    }
                    else
                    {
                        StartApplication(args);
                    }
                }
                else
                {
                    MessageBox.Show($"更新程序无法单独启动,如果想通过程序下载程序文件,请添加upgrade.init文件，并且按照规则:\r\n  xxx.exe(应用程序名称),1.0.1(应用程序版本号),xx项目(更新站点的配置中可以找到),xx项目密码(更新站点的配置中可以找到),www.upgrade.com(更新站点地址) \r\n,编辑upgrade.init文件");
                    Application.Exit();
                }
            }
            else
            {
                StartApplication(args);
            }
        }
        static void StartApplication(string [] args)
        {
            StartExeName = args[0].Trim();
            client.BaseAddress = new Uri(args[4].Trim());
            string currentVersion = args[1];
            string projectName = args[2];
            string projectpassword = args[3];
            var projectinfo = new ProjectInfo() { Name = projectName, PassWord = projectpassword };
            var tokenres = client.PostAsync("/Auth/GetToken", new StringContent(JsonConvert.SerializeObject(projectinfo), System.Text.Encoding.UTF8, "application/json")).Result;
            if (tokenres.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("更新服务器异常,点击确定继续");
                StartProcess();
            }
            else
            {
                var tokenstr = tokenres.Content.ReadAsStringAsync().Result;
                client.DefaultRequestHeaders.Add("Token", string.Concat("autoupgrade ", tokenstr));
            };
            var contiuerun = false;
            var versionres = client.GetAsync(string.Concat("/Upgrade/HasUpgrade?currentVersion=", currentVersion)).Result;

            if (versionres.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("更新服务器异常,点击确定继续");
                StartProcess();
            }
            else
            {
                var r = versionres.Content.ReadAsStringAsync().Result;
                if (r == "true")
                {
                    contiuerun = true;
                }
            }
            if (contiuerun)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
            else
            {
                StartProcess();
            }
        }
        static void StartProcess()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = StartExeName;//要启动的程序外部名称   
            info.Arguments = "false";
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start(info);
            Application.Exit();
        }
    }
}
