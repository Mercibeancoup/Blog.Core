
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Model
{
    /// <summary>
    /// 通用分页信息类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageModel<T>
    {
        /// <summary>
        /// 当前页标
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int pageCount { get; set; }

        /// <summary>
        /// 数据总数
        /// </summary>
        public int dataCount { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public List<T> data { get; set; }
    }
}
