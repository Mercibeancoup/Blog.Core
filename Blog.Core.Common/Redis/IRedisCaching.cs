using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Common
{
    /// <summary>
    /// redis缓存接口
    /// </summary>
    public interface IRedisCaching
    {

        /// <summary>
        ///获取 Reids 缓存值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetValue(string key);

        /// <summary>
        ///  获取值，并序列化
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        TEntity Get<TEntity>(string key);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="timeSpan">过期时间</param>
        void Set(string key, object value, TimeSpan timeSpan);

        //判断是否存在
        bool Get(string key);

        //移除某一个缓存值
        void Remove(string key);

        //全部清除
        void Clear();

        /// <summary>
        /// 增加/修改
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetValue(string key, byte[] value);
    }
}
