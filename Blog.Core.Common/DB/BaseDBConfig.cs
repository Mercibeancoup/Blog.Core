using Blog.Core.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Blog.Core.Common.DB
{
    /// <summary>
    /// DB配置 多数据库
    /// </summary>
    public class BaseDBConfig
    {

        public static (List<MultiDBOperate>, List<MultiDBOperate>) MutiConnectionString => MultiInitConn();

        private static string DifDBConnOfSecurity(params string[] conn)
        {
            foreach (var item in conn)
            {
                try
                {
                    if (File.Exists(item))
                    {
                        return File.ReadAllText(item).Trim();
                    }
                }
                catch (System.Exception) { }
            }

            return conn[conn.Length - 1];
        }

        public static (List<MultiDBOperate>, List<MultiDBOperate>) MultiInitConn()
        {
            List<MultiDBOperate> listdatabase = new List<MultiDBOperate>();
            List<MultiDBOperate> listdatabaseSimpleDB = new List<MultiDBOperate>();//单库
            List<MultiDBOperate> listdatabaseSlaveDB = new List<MultiDBOperate>();//从库

            string Path = "appsettings.json";
            using (var file = new StreamReader(Path))
            using (var reader = new JsonTextReader(file))
            {
                var jObj = (JObject)JToken.ReadFrom(reader);
                if (!string.IsNullOrWhiteSpace("DBS"))
                {
                    var secJt = jObj["DBS"];
                    if (secJt != null)
                    {
                        for (int i = 0; i < secJt.Count(); i++)
                        {
                            if (secJt[i]["Enabled"].ObjToBool())
                            {
                                listdatabase.Add(SpecialDbString(new MultiDBOperate()
                                {
                                    ConnId = secJt[i]["ConnId"].ObjToString(),
                                    Conn = secJt[i]["Connection"].ObjToString(),
                                    DbType = (DataBaseType)(secJt[i]["DBType"].ObjToInt()),
                                    HitRate = secJt[i]["HitRate"].ObjToInt(),
                                }));
                            }
                        }
                    }
                }

                // 单库，且不开启读写分离，只保留一个
                if (!Appsettings.app(new string[] { "CQRSEnabled" }).ObjToBool() && !Appsettings.app(new string[] { "MutiDBEnabled" }).ObjToBool())
                {
                    if (listdatabase.Count == 1)
                    {
                        return (listdatabase, listdatabaseSlaveDB);
                    }
                    else
                    {
                        var dbFirst = listdatabase.FirstOrDefault(d => d.ConnId == Appsettings.app(new string[] { "MainDB" }).ObjToString());
                        if (dbFirst == null)
                        {
                            dbFirst = listdatabase.FirstOrDefault();
                        }
                        listdatabaseSimpleDB.Add(dbFirst);
                        return (listdatabaseSimpleDB, listdatabaseSlaveDB);
                    }
                }


                // 读写分离，且必须是单库模式，获取从库

                if (Appsettings.app(new string[] { "CQRSEnabled" }).ObjToBool() && !Appsettings.app(new string[] { "MutiDBEnabled" }).ObjToBool())
                {
                    if (listdatabase.Count > 1)
                    {
                        listdatabaseSlaveDB = listdatabase.Where(d => d.ConnId != Appsettings.app(new string[] { "MainDB" }).ObjToString()).ToList();
                    }
                }



                return (listdatabase, listdatabaseSlaveDB);
            }

        }
       
        private static MultiDBOperate SpecialDbString(MultiDBOperate MultiDBOperate)
        {
            if (MultiDBOperate.DbType == DataBaseType.Sqlite)
            {
                MultiDBOperate.Conn = $"DataSource=" + Path.Combine(Environment.CurrentDirectory, MultiDBOperate.Conn);
            }
            //else if (MultiDBOperate.DbType == DataBaseType.SqlServer)
            //{
            //    MultiDBOperate.Conn = DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1.txt", @"c:\my-file\dbCountPsw1.txt", MultiDBOperate.Conn);
            //}
            else if (MultiDBOperate.DbType == DataBaseType.MySql)
            {
                MultiDBOperate.Conn = DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1_MySqlConn.txt", @"c:\my-file\dbCountPsw1_MySqlConn.txt", MultiDBOperate.Conn);
            }
            else if (MultiDBOperate.DbType == DataBaseType.Oracle)
            {
                MultiDBOperate.Conn = DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1_OracleConn.txt", @"c:\my-file\dbCountPsw1_OracleConn.txt", MultiDBOperate.Conn);
            }

            return MultiDBOperate;
        }
    }


    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DataBaseType
    {
        MySql = 0,
        SqlServer = 1,
        Sqlite = 2,
        Oracle = 3,
        PostgreSQL = 4
    }
    public class MultiDBOperate
    {
        /// <summary>
        /// 连接ID
        /// </summary>
        public string ConnId { get; set; }

        /// <summary>
        /// 从库执行级别，越大越先执行
        /// </summary>
        public int HitRate { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string Conn { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DataBaseType DbType { get; set; }
    }
}
