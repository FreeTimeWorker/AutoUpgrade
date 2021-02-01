using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoUpgrade.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UpgradeController : ControllerBase
    {
        [HttpGet]
        public VirtualFileResult GetFile(string FileName)
        {
            var projectname = HttpContext.Items["ProjectName"].ToString();
            FileName = Path.Combine(projectname, FileName);
            return File(FileName, "application/octet-stream",Path.GetFileName(FileName));
        }
        /// <summary>
        /// 提交版本号获取是否需要更新
        /// </summary>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        [HttpGet]
        public bool HasUpgrade(string currentVersion)
        {
            var projectname = HttpContext.Items["ProjectName"].ToString();
            var versionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"wwwroot",projectname,"version.txt");
            using (FileStream fs = new FileStream(versionFile, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var topversion = sr.ReadToEnd().Replace("\r","").Replace("\n","").Trim();
                    return topversion != currentVersion;
                }
            }
        }
    }
}
