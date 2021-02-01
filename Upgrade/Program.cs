using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
            //args = new string[] { "Winform_App.exe", "1.0.0", "SampleProject", "5C210AF8-6D4E-4D06-A9B7-F1E04BFE8AC3", "http://localhost:56799/" };
            if (args.Length != 5)
            {
                MessageBox.Show("�������󣬲��� 1��Ӧ�ó������ڳ���exe���ƣ�2,currentversion��3,��Ŀ���ƣ�4����Ŀ����");
                Application.Exit();
            }
            else
            {
                StartExeName = args[0].Trim();
                client.BaseAddress = new Uri(args[4].Trim());
                string currentVersion = args[1];
                string projectName = args[2];
                string projectpassword = args[3];
                var projectinfo = new ProjectInfo() { Name = projectName, PassWord = projectpassword };
                client.PostAsync("/Auth/GetToken", JsonContent.Create<ProjectInfo>(projectinfo,options:new System.Text.Json.JsonSerializerOptions() {
                    PropertyNamingPolicy = null//�����������л�Ϊ������ʽ��nullΪ��ת��ԭ�����
                }))
                .ContinueWith(result=> {
                    var tokenstr = result.Result.Content.ReadAsStringAsync().Result;
                    client.DefaultRequestHeaders.Add("Token", string.Concat("autoupgrade ", tokenstr));
                }).Wait();
                var contiuerun = false;
                client.GetAsync(string.Concat("/Upgrade/HasUpgrade?currentVersion=", currentVersion))
                    .ContinueWith(result =>
                    {
                        var r = result.Result.Content.ReadAsStringAsync().Result;
                        if (r == "true")
                        {
                            contiuerun = true;
                        }
                    }).Wait();
                if (contiuerun)
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Main());
                }
                else
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
    }
}
