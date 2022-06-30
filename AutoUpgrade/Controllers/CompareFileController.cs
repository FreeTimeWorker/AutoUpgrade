using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpgrade.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CompareFileController : ControllerBase
    {
        /// <summary>
        /// 比对文件确定需要下载的文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public FileDiff Compare(List<FileHashCode> fileHasCodes)
        {
            var result = new FileDiff();
            List<string> filenames = fileHasCodes.Select(o => o.FileName).ToList();
            var projectName = HttpContext.Items["ProjectName"].ToString();
            result.Changes.AddRange(GetNewFiles(projectName, filenames));//新增的文件
            var diff = CompareFiles(projectName, fileHasCodes);
            result.Deletedes = diff.Deletedes;
            result.Changes.AddRange(diff.Changes);//有改动的文件
            var ignoreFiles = GetignoreFile(projectName);
            result.Changes= result.Changes.Except(ignoreFiles).ToList();
            return result;
        }
        /// <summary>
        /// 获取忽略的文件
        /// </summary>
        /// <returns></returns>
        public List<string> GetignoreFile(string projectName)
        {
            List<string> ignore = new List<string>();
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", projectName);
            using (FileStream fs = new FileStream(Path.Combine(baseDir, ".ignore"), FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var item = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(item)&&!item.StartsWith("#"))
                        {
                            ignore.Add(item);
                        }
                    }
                }
            }
            ignore.Add(".ignore");
            return ignore;
        }

        /// <summary>
        /// 比较文件
        /// </summary>
        /// <param name="fileHasCodes"></param>
        /// <returns></returns>
        public FileDiff CompareFiles(string projectName,List<FileHashCode> fileHasCodes)
        {
            var result = new FileDiff();
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", projectName);
            foreach (var item in fileHasCodes)
            {
                string filename = Path.Combine(baseDir, item.FileName);
                if (System.IO.File.Exists(filename))
                {
                    var bytes = System.IO.File.ReadAllBytes(filename);
                    var hashcode = ComputerHash(bytes);
                    if (item.HashCode != hashcode)
                    {
                        result.Changes.Add(item.FileName);
                    }
                }
                else
                {
                    result.Deletedes.Add(item.FileName);
                }
            }
            return result;
        }

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
        private List<string> GetNewFiles(string projectName, List<string> filenames)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", projectName);
            List<string>  files = GetFileSystemEntries(baseDir);
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = files[i].Replace(baseDir, "").TrimStart('\\');
            }
            return files.Except(filenames).Where(o=>!o.StartsWith("Upgrade")).ToList();
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
                    lst.Add(item);
                }
            }
            return lst;
        }
    }
}
