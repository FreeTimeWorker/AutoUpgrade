using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Upgrade
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            this.btnStartExe.Enabled = false;
        }
        private List<string> willDownLoadFiles;
        private List<string> willDelFiles;
        private void Main_Load(object sender, EventArgs e)
        {
            //去找看看有那些需要下载的文件
            Task.Factory.StartNew(() =>
            {
                //获取需要本地文件夹中程序目录的文件
                var dir = AppDomain.CurrentDomain.BaseDirectory;
                var files = GetFileSystemEntries(dir);
                List<FileHashCode> fileHashCodes = new List<FileHashCode>();
                foreach (var item in files)
                {
                    string filename = item.Replace(dir, "");
                    var filebytes = File.ReadAllBytes(item);
                    var filehash = ComputerHash(filebytes);
                    fileHashCodes.Add(new FileHashCode()
                    {
                        FileName = filename,
                        HashCode = filehash
                    });
                }
                notifyForm("正在比对文件",0, 100);
                //确认了需要下载的文件
                Program.client.PostAsync("/CompareFile/Compare", JsonContent.Create<List<FileHashCode>>(fileHashCodes, options: new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNamingPolicy = null//属性名称序列化为其他形式，null为不转换原样输出
                }))
                .ContinueWith(result => {
                    notifyForm("比对文件完成", 100, 100);
                    if (result.Result.Content.ReadAsStringAsync().Result == "")
                    {
                        willDownLoadFiles = new List<string>();
                        willDelFiles = new List<string>();
                    }
                    else
                    {
                        var res = result.Result.Content.ReadFromJsonAsync<FileDiff>().Result;
                        willDownLoadFiles = res.Changes;
                        willDelFiles = res.Deletedes;
                    }
                    notifyForm($"需要下载的文件数:{willDownLoadFiles.Count} 需要删除的文件为", 0, 100);
                }).Wait();
                var current = 0;
                if (willDelFiles != null && willDelFiles.Count > 0)
                {
                    notifyForm($"清理文件:{willDelFiles.Count} 需要删除的文件为", 0, willDelFiles.Count);
                    foreach (var item in willDelFiles)
                    {
                        File.Delete(item);
                        notifyForm($"清理文件:{willDelFiles.Count} 需要删除的文件为", current++, willDelFiles.Count);
                    }
                }
                current = 0;
                foreach (var item in willDownLoadFiles.OrderBy(item=>item!= "version.txt"))
                {
                    current++;
                    var retryTime = 3;
                    var downSuccess = false;
                    while (retryTime>0&&!downSuccess)
                    {
                        try
                        {
                            notifyForm($"【{current}/{willDownLoadFiles.Count}】   正在下载文件{item}", current, willDownLoadFiles.Count);
                            var itempath = item.TrimStart('\\');
                            Program.client.GetAsync(string.Concat("/Upgrade/GetFile?FileName=" + itempath))
                            .ContinueWith(result => {
                                result.Result.Content.ReadAsStreamAsync().ContinueWith(o => {
                                    if (File.Exists(Path.Combine(dir, itempath)))
                                    {
                                        File.Delete(Path.Combine(dir, itempath));
                                    }
                                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(dir, itempath)));
                                    byte[] body = new byte[o.Result.Length];
                                    o.Result.Read(body, 0, (int)o.Result.Length);
                                    using (FileStream fs = new FileStream(Path.Combine(dir, itempath), FileMode.Create))
                                    {
                                        fs.Write(body);
                                        fs.Flush();
                                        fs.Close();
                                    }
                                }).Wait();
                            }).Wait();
                            downSuccess = true;
                           
                        }
                        catch
                        {
                            notifyForm($"【{current}/{willDownLoadFiles.Count}】    下载文件{item}失败,重试下载", current, willDownLoadFiles.Count);
                            retryTime--;
                        }
                    }
                    
                }
                this.btnStartExe.Invoke(new Action(delegate ()
                {
                    notifyForm("更新完成，点击启动按钮继续", 100, 100);
                    this.btnStartExe.Enabled = true;
                }));
            });
        }
        private List<string> GetFileSystemEntries(string dir)
        {
            List<string> lst = new List<string>();
            foreach (string item in Directory.GetFileSystemEntries(dir))
            {
                if (System.IO.File.GetAttributes(item) == FileAttributes.Directory)
                {
                    lst.AddRange(GetFileSystemEntries(item));
                }
                else
                {
                    if (!Path.GetFileName(item).StartsWith("Upgrade"))
                    {
                        lst.Add(item);
                    }
                }
            }
            return lst;
        }

        private void notifyForm(string message,int current, int count)
        {
            this.btnStartExe.Invoke(new Action(delegate ()
            {
                label1.Text = message;
                progressBar.Maximum = count;
                progressBar.Value = current;
            }));
        }
        /// <summary>
        /// 计算hash
        /// </summary>
        /// <param name="bts"></param>
        /// <returns></returns>
        private string ComputerHash(byte[] bts)
        {
            System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
            Byte[] buffer = calculator.ComputeHash(bts);
            calculator.Clear();
            //将字节数组转换成十六进制的字符串形式
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {

                stringBuilder.Append(buffer[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
        private void btnStartExe_Click(object sender, EventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Program.StartExeName;//要启动的程序外部名称 
            info.Arguments = "false";
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start(info);
            Application.Exit();
        }
    }
}
