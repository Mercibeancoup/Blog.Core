using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Linq;
using Blog.Core.Common.Helper;
using Newtonsoft.Json;

namespace Blog.Core.Common.LogHelper
{
    public class LogLock
    {
        //采用ReaderWriterLockSlim 以允许多读单写模式
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        static int WriteCount = 0;
        static int FailCount = 0;
        static string _contentRoot = string.Empty;

        public LogLock(string contentPath)
        {
            _contentRoot = contentPath;
        }

        /// <summary>
        /// 将数据写入文件中
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dataParse"></param>
        /// <param name="IsHeader"></param>
        public static void OutSql2Log(string fileName, string[] dataParse, bool IsHeader = true)
        {
            try
            {
                //设置读写锁为写入模式独占资源，其他写入需要等待本次写入结束之后才能继续写入
                //注意：长时间持有读线程锁或写线程锁会使其他线程发生饥饿（starve），为了得到更好的性能，需要考虑重新构造应用程序以将写访问的持续时间减少到最少
                //从性能考虑，请求进入写入模式应该紧跟文件操作之前，在此进入写入模式仅是为了降低代码复杂度
                //因进入与推出写入模式应在同一个try catch语句块内，所以在请求进入写入模式之前不能触发异常，否则释放次数大于请求次数将会触发异常
                LogWriteLock.EnterWriteLock();

                var path = Path.Combine(_contentRoot, "Log");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var logFilePath = Path.Combine(path, $@"{fileName}.log");

                var now = DateTime.Now;

                var logContent = string.Join("\r\n", dataParse);

                if (IsHeader)
                {
                    logContent = ("--------------------------------\r\n" +
                        DateTime.Now + "\r\n" +
                        string.Join("\r\n", dataParse) + "\r\n"
                        );
                }

                File.AppendAllText(logFilePath, logContent);
                WriteCount++;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                FailCount++;
            }
            finally
            {
                //退出写入模式，释放资源占用
                //注意：一次请求对应一次释放
                //    若释放请求次数>请求次数将会触发异常[写入锁定未经保持即被释放]
                //    若请求处理完成后未释放将会触发异常[此模式下不允许以递归方式获取写入锁定]
                LogWriteLock.ExitWriteLock();
            }
        }

        public static string ReadLog(string Path, Encoding encode)
        {
            string s = "";
            try
            {
                LogWriteLock.EnterReadLock();
                if (!File.Exists(Path))
                {
                    s = null;
                }
                else
                {
                    using (var sr = new StreamReader(Path))
                    {
                        s = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {

                FailCount++;
            }
            finally
            {
                LogWriteLock.ExitReadLock();
            }
            return s;
        }

        public static List<LogInfo> GetLogData()
        {
            var aopLogs = new List<LogInfo>();
            var excLogs = new List<LogInfo>();
            var sqlLogs = new List<LogInfo>();
            var requestsLogs = new List<LogInfo>();

            try
            {
                var aoplogContent = ReadLog(Path.Combine(_contentRoot, "Log", "AOPLog.log"), Encoding.UTF8);
                if (!string.IsNullOrEmpty(aoplogContent))
                {
                    aopLogs = aoplogContent.Split("--------------------------------")
                        .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
                        .Select(d => new LogInfo
                        {
                            Datetime = d.Split("|")[0].ObjToDate(),
                            Content = d.Split("|")[1]?.Replace("\r\n", "<br>"),
                            LogColor = "AOP"
                        }).ToList();


                }
            }
            catch (Exception)
            {

                throw;
            }

            try
            {
                var excLogContent = ReadLog(
                    Path.Combine(_contentRoot, "Log", $"GlobalExcepLogs_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log"),
                    Encoding.UTF8);

                if (!string.IsNullOrEmpty(excLogContent))
                {
                    excLogs = excLogContent.Split("--------------------------------")
                        .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
                        .Select(d => new LogInfo
                        {
                            Datetime = (d.Split("|")[0]).Split(',')[0].ObjToDate(),
                            Content = d.Split("|")[1]?.Replace("\r\n", "<br>"),
                            LogColor = "EXC",
                            Import = 9,
                        }).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }

            try
            {
                var sqlLogContent = ReadLog(Path.Combine(_contentRoot, "Log", "SqlLog"), Encoding.UTF8);

                if (!string.IsNullOrEmpty(sqlLogContent))
                {
                    sqlLogs = sqlLogContent.Split("--------------------------------")
                        .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
                        .Select(d => new LogInfo()
                        {
                            Datetime = d.Split("|")[0].ObjToDate(),
                            Content = d.Split("|")[1].Replace("\r\n", "</br>"),
                            LogColor = "SQL"
                        }).ToList();
                }

            }
            catch (Exception)
            {

                throw;
            }

            try
            {
                var Logs = JsonConvert.DeserializeObject<List<RequestInfo>>("[" + ReadLog(Path.Combine(_contentRoot, "Log", "RequestIpInfoLog.log"), Encoding.UTF8) + "]");

                Logs = Logs.Where(d => d.Datetime.ObjToDate() >= DateTime.Today).ToList();

                requestsLogs = Logs.Select(d => new LogInfo
                {
                    Datetime = d.Datetime.ObjToDate(),
                    Content = $"IP:{d.Ip}<br>{d.Url}",
                    LogColor = "ReqRes",
                }).ToList();
            }
            catch (Exception)
            {
            }

            if (excLogs != null)
            {
                aopLogs.AddRange(excLogs);
            }
            if (sqlLogs != null)
            {
                aopLogs.AddRange(sqlLogs);
            }
            if (requestsLogs != null)
            {
                aopLogs.AddRange(requestsLogs);
            }

            aopLogs = aopLogs.OrderByDescending(s => s.Import)
                .ThenByDescending(s => s.Datetime)
                .Take(100)
                .ToList();

            return aopLogs;
        }
    }
}
