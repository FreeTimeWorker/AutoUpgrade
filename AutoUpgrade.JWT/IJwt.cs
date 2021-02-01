using System.Collections.Generic;

namespace AutoUpgrade.JWT
{
    public interface IJwt
    {
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="Clims">需要传递到Clims中的信息</param>
        /// <param name="OldToken">传递OldToken该方法会让OldToken立即失效</param>
        /// <returns></returns>
        string GetToken(IDictionary<string, string> Clims, string OldToken = null);
        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="Token">待验证的字符串</param>
        /// <param name="Clims">得到传入Clim中的信息</param>
        /// <returns></returns>
        bool ValidateToken(string Token, out Dictionary<string, string> Clims);
        /// <summary>
        /// 让Token立即失效
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        bool InvalidateToken(string Token);
    }
}
