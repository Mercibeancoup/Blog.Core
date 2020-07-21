using Blog.Core.Common;
using Blog.Core.Common.DB;
using Blog.Core.Common.Helper;
//using Blog.Core.Common.Helper;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core.Extensions
{
    /// <summary>
    /// SqlSugar 启动服务
    /// </summary>
    public static  class SqlsugarSetup
    {
        /// <summary>
        /// services的扩展方法，用于实现Sqlsugar的依赖注入
        /// </summary>
        /// <param name="services"></param>
        public static void AddSqlsugarSetup(this IServiceCollection services)
        {
            if(services==null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            // 默认添加主数据库连接
            MainDb.CurrentDbConnId = Appsettings.app(new string[] { "MainDB" });

            //把多个连接对象注入服务，这里必须采用scope，因为有事务操作
            services.AddScoped<ISqlSugarClient>(o => 
            {
                //连接字符串
                var listConfig = new List<ConnectionConfig>();
                //配置从库
                var listConfig_Slave = new List<SlaveConnectionConfig>();
                BaseDBConfig.MutiConnectionString.Item2.ForEach(s=> 
                {
                    listConfig_Slave.Add(new SlaveConnectionConfig()
                    {
                        HitRate = s.HitRate,
                        ConnectionString = s.Conn
                    });
                });

                BaseDBConfig.MutiConnectionString.Item1.ForEach(m =>
                {
                    listConfig.Add(new ConnectionConfig()
                    {
                        ConfigId = m.ConnId.ObjToString().ToLower(),
                        ConnectionString = m.Conn,
                        DbType = (DbType)m.DbType,
                        IsAutoCloseConnection = true,
                        IsShardSameThread = false,
                        AopEvents = new AopEvents
                        {
                            OnLogExecuting = (sql, p) =>
                            {
                                // 多库操作的话，此处记录aop日志无效，在BaseRepository.cs配置有效
                            }
                        },
                        MoreSettings = new ConnMoreSettings()
                        {
                            IsAutoRemoveDataCache = true
                        },
                        // 从库
                        SlaveConnectionConfigs = listConfig_Slave,
                        //InitKeyType = InitKeyType.SystemTable
                    }
                   );
                });
                return new SqlSugarClient(listConfig);
            });

        }
    }
}
