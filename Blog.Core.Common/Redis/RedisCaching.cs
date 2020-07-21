using Blog.Core.Common.Helper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Common
{
    /// <summary>
    /// 实例化接口IRedisCaching
    /// 单例模式实现
    /// </summary>
    class RedisCaching : IRedisCaching
    {
        private readonly string _redisConnectionString;

        //初始化redis连接(redis调制器)
        private volatile ConnectionMultiplexer redisConnection;

        private readonly object redisConnectionLock = new object();

        public RedisCaching()
        {
            var redisConfiguration = Appsettings.app(new string[] { "AppSettings", "RedisCachingAOP", "ConnectionString" });

            if(string.IsNullOrWhiteSpace(redisConfiguration))
            {
                throw new ArgumentException("redis config is empty", nameof(redisConfiguration));
            }

            _redisConnectionString = redisConfiguration;
            redisConnection = GetRedisConnection();
        }

        /// <summary>
        /// 核心代码，获取连接实例
        /// 通过双if加lock的方式，实现单例模式
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetRedisConnection()
        {
            if(redisConnection != null&& redisConnection.IsConnected)
            {
                return this.redisConnection;
            }
            //加锁，防止异步编程中，出现单例无效的问题
            lock(redisConnectionLock)
            {
                if(this.redisConnection!=null)
                {
                    //释放redis连接
                    this.redisConnection.Dispose();
                }
                try
                {
                    this.redisConnection = ConnectionMultiplexer.Connect(_redisConnectionString);
                }
                catch
                {
                    //throw new Exception("Redis服务未启用，请开启该服务，并且请注意端口号，本项目使用的的6319，而且我的是没有设置密码。");
                }
            }
            return this.redisConnection;
        }
        
        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            foreach (var endPoint in this.GetRedisConnection().GetEndPoints())
            {
                var server = this.GetRedisConnection().GetServer(endPoint);
                foreach (var key in server.Keys())
                {
                    redisConnection.GetDatabase().KeyDelete(key);
                }
            }
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TEntity Get<TEntity>(string key)
        {
            var value = redisConnection.GetDatabase().StringGet(key);
            if(!value.HasValue)
            {
                return default(TEntity);
            }
            else
            {
                //需要用的反序列化，将Redis存储的Byte[]，进行反序列化
                return SerializeHelper.Deserialzie<TEntity>(value);
            }
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Get(string key)
        {
            return redisConnection.GetDatabase().KeyExists(key);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            return redisConnection.GetDatabase().StringGet(key);
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            redisConnection.GetDatabase().KeyDelete(key);
 
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        public void Set(string key, object value, TimeSpan timeSpan)
        {
            if(value!=null)
            {
                //序列化，将object值生成RedisValue
                redisConnection.GetDatabase().StringSet(key, SerializeHelper.Serialize(value), timeSpan);
            }
        }

        /// <summary>
        /// 增加/修改
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(string key, byte[] value)
        {
            return redisConnection.GetDatabase().StringSet(key, value, TimeSpan.FromSeconds(120));
        }
    }
}
