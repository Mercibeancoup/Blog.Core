
using Blog.Core.Model.Models;
using Blog.Core.Services.BASE;
using Blog.Core.IServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Blog.Core.IRepository;

namespace Blog.Core.Services
{
    public class BlogArticleServices : BaseServices<BlogArticle>, IBlogArticleServices
    {
        private IBlogArticleRepository _dal;
        public BlogArticleServices(IBlogArticleRepository dal)
        {
            _dal = dal;
            base.baseDal = dal;
        }
        /// <summary>
        /// 获取blog列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<BlogArticle>> GetBlogs()
        {
            var blogList = await _dal.Query(a => a.bID > 0, a =>a.bID);

            return blogList;

        }
    }
}
