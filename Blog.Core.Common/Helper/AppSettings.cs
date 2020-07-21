using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Blog.Core.Common.Helper
{
    /// <summary>
    /// appsettings.json操作类
    /// </summary>
    public class Appsettings
    {

        static IConfiguration Configuration { get; set; }

        //static string contentPath;

        #region 之前的构造函数
        //public AppSettings(string contentPath)
        //{
        //    string path = "appsettings.json";

        //    //如果你把配置文件 是 根据环境变量来分开了，可以这样写
        //    //var path = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";


        //    Configuration = new ConfigurationBuilder()
        //        .SetBasePath(contentPath)
        //        .Add(new JsonConfigurationSource
        //        {
        //            Path = path,
        //            Optional = false,
        //            ReloadOnChange = true//这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
        //        }).Build();
        //}
        #endregion


        static Appsettings()
        {
            string Path = "appsettings.json";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Add(new JsonConfigurationSource
                {
                    Path = Path,
                    Optional = false,
                    ReloadOnChange = true
                })///这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
            .Build();

        }

        /// <summary>
        /// 封装要操作的操作符
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string app(params string[] sections)
        {
            try
            {
                var val = string.Empty;

                for (int i = 0; i < sections.Length; i++)
                {
                    val += sections[i] + ":";
                }

                return Configuration[val.TrimEnd(':')];

            }
            catch (Exception)
            {
                return "";
            }

            //return "";
        }
    }
}
