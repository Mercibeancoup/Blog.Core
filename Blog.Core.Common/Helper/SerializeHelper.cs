using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Common.Helper
{
    /// <summary>
    /// 序列化
    /// </summary>
    public class SerializeHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item);

            return Encoding.UTF8.GetBytes(jsonString);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public  static TEntity Deserialzie<TEntity>(byte[] value)
        {
            if(value==null)
            {
                return default(TEntity);
            }
            else
            {
                var jsonString = Encoding.UTF8.GetString(value);
                return JsonConvert.DeserializeObject<TEntity>(jsonString);
            }
        }

    }
}
