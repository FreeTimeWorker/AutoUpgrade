using AutoUpgrade.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoUpgrade.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwt _jwt;
        IConfiguration _configration;
        private List<ProjectInfo> projectInfos;
        public AuthController(IJwt jwt, IConfiguration configration)
        {
            _jwt = jwt;
            _configration = configration;
            projectInfos= _configration.GetSection("ProjectInfo").Get<List<ProjectInfo>>();
        }
        
        [HttpPost]
        public string GetToken(JsonDocument tokenModel)
        {
            if (!tokenModel.RootElement.TryGetProperty("Name", out JsonElement Name) || !tokenModel.RootElement.TryGetProperty("PassWord", out JsonElement PassWord))
            {
                return "token获取失败";
            }
            else
            {
                var name = Name.GetString();
                var password = PassWord.GetString();
                var model = projectInfos.FirstOrDefault(o => o.Name == name && o.PassWord == password);
                if (model != null)
                {
                    Dictionary<string, string> clims = new Dictionary<string, string>
                    {
                        {"ProjectName", name }
                    };
                    return _jwt.GetToken(clims);
                }
                else
                {
                    return "token获取失败";
                }
            }
        }
    }
}
