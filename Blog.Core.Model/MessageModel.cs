using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Model
{
    /// <summary>
    /// 通用返回信息类
    /// </summary>
    class MessageModel<T>
    {
        /// <summary>
        /// 操作是否相同
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回数据类型
        /// </summary>
        public  List<T> Data { get; set; }
    }
}
