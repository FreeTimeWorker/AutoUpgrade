using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoUpgrade.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CompareFileController : ControllerBase
    {
        private readonly ILogger<CompareFileController> _logger;

        public CompareFileController(ILogger<CompareFileController> logger)
        {
            _logger = logger;
        }


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
            var diff = CompareFiles(projectName, fileHasCodes);
            result.Deletedes = diff.Deletedes;
            result.Changes.AddRange(diff.Changes);//有改动的文件
            result.Changes.AddRange(GetNewFiles(projectName, filenames));//新增的文件
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
            var ignoreFiles = GetignoreFile(projectName);
            foreach (var item in fileHasCodes)
            {
                string filename = Path.Combine(baseDir, item.FileName.Replace("\\","/"));
                if (System.IO.File.Exists(filename))
                {
                    var bytes = System.IO.File.ReadAllBytes(filename);
                    var hashcode = ComputerHash(bytes);
                    if (item.HashCode != hashcode)
                    {
                        result.Changes.Add(item.FileName);
                    }

                    _logger.LogInformation($"比较文件hash值,文件路径{filename},计算结果{hashcode},window计算结果{item.HashCode},一致:{item.HashCode == hashcode}");
                }
                else
                {
                    if (ignoreFiles.Contains(item.FileName))
                    {
                        continue;
                    }
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
                files[i] = files[i].Replace(baseDir,"").TrimStart('\\').TrimStart('/').Replace("\\","/");
            }
            var postFiles = filenames.Select(s => s.Replace("\\", "/")).ToList();
            var res = files.Except(postFiles).Where(o=>!o.StartsWith("Upgrade")).ToList();
            return res;
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
