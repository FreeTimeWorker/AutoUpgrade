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
        /// ���� 1��Ӧ�ó������ڳ���exe���ƣ�2,currentversion��3,��Ŀ���ƣ�4����Ŀ����,5,������վ��ַ
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                //��ȡ��ǰĿ¼�£�������û�������ļ� init.json
                if (File.Exists("upgrade.init"))
                {
                    string config = File.ReadAllText("upgrade.init");
                    args = config.Split(',');
                    if (args.Length != 5)
                    {
                        MessageBox.Show("upgrade.init�ļ��ĸ�ʽΪ: Ӧ�ó������ڳ���exe����,currentversion,��Ŀ����,��Ŀ����,������վ��ַ");
                        Application.Exit();
                    }
                    else
                    {
                        StartApplication(args);
                    }
                }
                else
                {
                    MessageBox.Show($"���³����޷���������,�����ͨ���������س����ļ�,�����upgrade.init�ļ������Ұ��չ���:\r\n  xxx.exe(Ӧ�ó�������),1.0.1(Ӧ�ó���汾��),xx��Ŀ(����վ��������п����ҵ�),xx��Ŀ����(����վ��������п����ҵ�),www.upgrade.com(����վ���ַ) \r\n,�༭upgrade.init�ļ�");
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
                MessageBox.Show("���·������쳣,���ȷ������");
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
                MessageBox.Show("���·������쳣,���ȷ������");
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
            info.FileName = StartExeName;//Ҫ�����ĳ����ⲿ����   
            info.Arguments = "false";
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start(info);
            Application.Exit();
        }
    }
}
