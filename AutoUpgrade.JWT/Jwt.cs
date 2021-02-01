using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AutoUpgrade.JWT
{
    public class Jwt : IJwt
    {
        public static List<string> InvalidateTokens = new List<string>();
        private readonly IConfiguration _configuration;
        private string _base64Secret;
        private readonly JwtConfig _jwtConfig = new JwtConfig();
        public Jwt(IConfiguration configration)
        {
            _configuration = configration;
            configration.GetSection("Jwt").Bind(_jwtConfig);
            GetSecret();
        }
        /// <summary>
        /// 获取到加密串
        /// </summary>
        private void GetSecret()
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(_jwtConfig.SecretKey);
            byte[] messageBytes = encoding.GetBytes(_jwtConfig.SecretKey);
            using (HMACSHA512 hmacsha512 = new HMACSHA512(keyByte))
            {
                byte[] hashmessage = hmacsha512.ComputeHash(messageBytes);
                _base64Secret = Convert.ToBase64String(hashmessage);
            }
        }
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="Claims"></param>
        /// <returns></returns>
        public string GetToken(IDictionary<string, string> Claims, string OldToken = null)
        {
            List<Claim> claimsAll = new List<Claim>();
            foreach (KeyValuePair<string, string> item in Claims)
            {
                claimsAll.Add(new Claim(item.Key, item.Value ?? ""));
            }
            byte[] symmetricKey = Convert.FromBase64String(_base64Secret);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                Subject = new ClaimsIdentity(claimsAll),
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.AddMinutes(_jwtConfig.Lifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey),
                                           SecurityAlgorithms.HmacSha512Signature)
            };
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            if (!string.IsNullOrEmpty(OldToken))//执行旧Token过期
            {
                if (!InvalidateTokens.Contains(OldToken))
                {
                    InvalidateTokens.Add(OldToken);
                }
            }
            return tokenHandler.WriteToken(securityToken);
        }
        public bool ValidateToken(string Token, out Dictionary<string, string> Clims)
        {
            Clims = new Dictionary<string, string>();
            if (InvalidateTokens.Contains(Token))
            {
                return false;
            }
            ClaimsPrincipal principal = null;
            if (string.IsNullOrWhiteSpace(Token))
            {
                return false;
            }
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                JwtSecurityToken jwt = handler.ReadJwtToken(Token);
                if (jwt == null)
                {
                    return false;
                }
                byte[] secretBytes = Convert.FromBase64String(_base64Secret);
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,//是否验证Issuer
                    ValidateAudience = true,//是否验证Audience
                    ValidateLifetime = _jwtConfig.ValidateLifetime,//是否验证失效时间
                    ValidateIssuerSigningKey = true,//是否验证SecurityKey
                    ValidAudience = _jwtConfig.Audience,
                    ValidIssuer = _jwtConfig.Issuer
                };
                principal = handler.ValidateToken(Token, validationParameters, out SecurityToken securityToken);
                foreach (Claim item in principal.Claims)
                {
                    Clims.Add(item.Type, item.Value);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 设置token失效
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public bool InvalidateToken(string Token)
        {
            if (!InvalidateTokens.Contains(Token))
            {
                InvalidateTokens.Add(Token);
            }
            return true;
        }
    }
}
