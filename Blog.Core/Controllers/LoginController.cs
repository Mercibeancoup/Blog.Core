using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.AuthHelper.OverWrite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Core.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        [Authorize(Roles ="Admin")]
        [Authorize(Roles = "System")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取token码
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public async Task<object> GetJwtStr(string name ,string pass)
        {
            // 将用户id和角色名，作为单独的自定义变量封装进 token 字符串中。
            TokenModelJwt tokenModel = new TokenModelJwt() { Uid = 1, Role = "Admin" };
            var jwtStr = JwtHelper.IssueJwt(tokenModel);

            var suc = true;

            return  Ok(new { 
                success=suc,
                token=jwtStr
            });
        }
    }
}