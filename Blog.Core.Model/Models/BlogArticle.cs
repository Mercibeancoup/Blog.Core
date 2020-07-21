
using System;
using SqlSugar;

namespace Blog.Core.Model.Models
{
    /// <summary>
    /// 博客文章
    /// </summary>
    public class BlogArticle
    {
        /// <summary>
        /// 主键
        /// </summary>
        /// 这里之所以没用RootEntity，是想保持和之前的数据库一致，主键是bID，不是Id
         [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int bID { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string bsubmitter { get; set; }

        /// <summary>
        /// 标题blog
        /// </summary>
        public string btitle { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public string bcategory { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string bcontent { get; set; }

        /// <summary>
        /// 访问量
        /// </summary>
        public int btraffic { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        public int bcommentNum { get; set; }

        /// <summary> 
        /// 修改时间
        /// </summary>
        public DateTime bUpdateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime bCreateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string bRemark { get; set; }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public bool? IsDeleted { get; set; }

    }
}
