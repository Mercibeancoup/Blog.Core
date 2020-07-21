using Blog.Core.Common.Helper;

using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.AuthHelper.OverWrite
{
    public class JwtHelper
    {
        /// <summary>
        /// 颁发JWT字符串
        /// </summary>
        /// <param name="tokenModelJwt"></param>
        /// <returns></returns>
        public static string IssueJwt(TokenModelJwt tokenModelJwt)
        {
            string iss = Appsettings.app(new string[] { "Audience", "Issuer" });
            string aud = Appsettings.app(new string[] { "Audience", "Audience" });
            string secret = Appsettings.app(new string[] { "Audience", "Secret" });


            var claims = new List<Claim>
            {
                /*
                 * 特别重要：
                 * 1.这里将用户的部分信息，比如uid存入Claim中，如果你想知道如何在其他地方将这个uid从Token中取出，请看下面的SerializeJwt()方法，或者在整个解决方案中，搜索整个方法，看哪里又在使用
                 * 2.你可以研究下HttpContext.User.Claims,具体的请看Policy/PermissionHandler.cs类中是如何使用的
                 */

                new Claim(JwtRegisteredClaimNames.Jti,tokenModelJwt.Uid.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                //这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
                new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(1000)).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Iss,iss),
                new Claim(JwtRegisteredClaimNames.Aud,aud),
                 //new Claim(ClaimTypes.Role,tokenModel.Role),//为了解决一个用户多个角色(比如：Admin,System)，用下边的方法
            };

            //可以将一个用户的角色全部赋予
            claims.AddRange(tokenModelJwt.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

            //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度太短会报出异常)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: iss,
                audience: aud,//需要与添加服务的audience一样
                signingCredentials: creds,

                expires:DateTime.Now.AddHours(1),//1小时后过去
                claims: claims
                
                );


            var jwtHandler = new JwtSecurityTokenHandler();

            var encodeJwt = jwtHandler.WriteToken(jwt);

            return encodeJwt;

        }

        /// <summary>
        /// 解析令牌
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static TokenModelJwt SerializeJwt(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);

            object role;
            try
            {
                jwtToken.Payload.TryGetValue(ClaimTypes.Actor, out role);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var tm = new TokenModelJwt
            {
                Uid = (jwtToken.Id).ObjToInt(),
                Role = role != null ? role.ObjToString() : ""
            };

            return tm;

        }
    }

    /// <summary>
    /// 令牌
    /// </summary>
    public class TokenModelJwt
    {
        /// <summary>
        /// id
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 职能
        /// </summary>
        public string Work { get; set; }
    }


}
