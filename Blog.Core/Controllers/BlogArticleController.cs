using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogArticleController : ControllerBase
    {
        private readonly IBlogArticleServices _dal;
        public BlogArticleController(IBlogArticleServices blogArticleServices)
        {
            _dal = blogArticleServices;
        }

        /// <summary>
        /// 根据id获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<BlogArticle>> GetBlogs()
        {
            return await     _dal.GetBlogs();
        }
    }
}