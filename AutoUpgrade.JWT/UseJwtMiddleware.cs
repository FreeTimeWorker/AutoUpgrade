using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoUpgrade.JWT
{
    public class UseJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtConfig _jwtConfig = new JwtConfig();
        private readonly IJwt _jwt;
        public UseJwtMiddleware(RequestDelegate next, IConfiguration configration, IJwt jwt)
        {
            _next = next;
            _jwt = jwt;
            configration.GetSection("Jwt").Bind(_jwtConfig);
        }

        private Regex GetIgnoreUrlsReg(List<string> urls)
        {
            List<string> regStrs = new List<string>();
            foreach (var item in urls)
            {
                regStrs.Add(string.Concat("^", item, ".*$"));
            }
            return new Regex(string.Join("|", regStrs));
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (GetIgnoreUrlsReg(_jwtConfig.IgnoreUrls).Match(context.Request.Path).Success)
            {
                return Validate(context, false);
            }
            else
            {
                return Validate(context, true);
            }
        }
        private Task Validate(HttpContext context, bool ValidateToken)
        {
            if (context.Request.Headers.TryGetValue(this._jwtConfig.HeadField, out Microsoft.Extensions.Primitives.StringValues authValue))
            {
                var authstr = authValue.ToString();
                if (this._jwtConfig.Prefix.Length > 0)
                {
                    if (!authstr.Contains(this._jwtConfig.Prefix))
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"Status\":401,\"StatusMsg\":\"权限验证失败\"}");
                    }
                    authstr = authValue.ToString().Substring(this._jwtConfig.Prefix.Length, authValue.ToString().Length - (this._jwtConfig.Prefix.Length));
                }
                if (this._jwt.ValidateToken(authstr, out Dictionary<string, string> Clims) && !Jwt.InvalidateTokens.Contains(authstr))
                {
                    List<string> climsKeys = new List<string>() { "nbf", "exp", "iat", "iss", "aud" };
                    IDictionary<string, string> RenewalDic = new Dictionary<string, string>();
                    foreach (var item in Clims)
                    {
                        if (climsKeys.FirstOrDefault(o => o == item.Key) == null)
                        {
                            context.Items.Add(item.Key, item.Value);
                            RenewalDic.Add(item.Key, item.Value);
                        }
                    }
                    //验证通过的情况下判断续期时间
                    if (Clims.Keys.FirstOrDefault(o => o == "exp") != null)
                    {
                        var start = new DateTime(1970, 1, 1, 0, 0, 0);
                        var timespanStart = long.Parse(Clims["nbf"]);//token有效时间的开始时间点
                        var tartDate = start.AddSeconds(timespanStart).ToLocalTime();
                        var o =  DateTime.Now- tartDate;//当前时间减去开始时间大于续期时间限制
                        if (o.TotalMinutes > _jwtConfig.RenewalTime)
                        {
                            //执行续期
                            var newToken = this._jwt.GetToken(RenewalDic);
                            context.Response.Headers.Add(_jwtConfig.ReTokenHeadField, newToken);
                        }
                    }
                    return this._next(context);
                }
                else
                {
                    if (ValidateToken == true)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"Status\":401,\"StatusMsg\":\"权限验证失败\"}");
                    }
                    else
                    {
                        return this._next(context);
                    }
                }
            }
            else
            {
                if (ValidateToken == true)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("{\"Status\":401,\"StatusMsg\":\"权限验证失败\"}");
                }
                else
                {
                    return this._next(context);
                }
            }
        }
    }
    /// <summary>
    /// 扩展方法，将中间件暴露出去
    /// </summary>
    public static class UseUseJwtMiddlewareExtensions
    {
        /// <summary>
        /// 权限检查
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseJwt(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UseJwtMiddleware>();
        }
    }

}
