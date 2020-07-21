using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Blog.Core.IRepository;
using Blog.Core.IRepository.UnitWork;
using Blog.Core.Model.Models;
using Blog.Core.Repository.BASE;
using Blog.Core.Repository.sugar;
using SqlSugar;

namespace Blog.Core.Repository
{
    public class BlogArticleRepository : BaseRepository<BlogArticle>, IBlogArticleRepository
    {
        public BlogArticleRepository(IUnitOfWork unitOfWork):base(unitOfWork)
        {

        }
        //因为持久层的基类已经实现了对应的增删改查，所以其他方法以下方法和构造全部不用定义实现

    }
}
