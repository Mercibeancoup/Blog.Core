using Blog.Core.Common.Hubs;
using Blog.Core.Common.LogHelper;
using Castle.DynamicProxy;//动态代理
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Blog.Core.AOP
{
    /// <summary>
    /// 拦截器BlogAOP ,继承IInterceptor 
    /// 面向切面的日志使用
    /// </summary>
    public class BlogLogAOP : IInterceptor
    {
        //private readonly IHubContext<ChatHub> _hubContext;
         private readonly IHttpContextAccessor _accessor;

        public BlogLogAOP()
        {
            //_hubContext = hubContext;
            // _accessor = accessor;
        }

        /// <summary>
        /// 实例化IInterceptor唯一方法
        /// </summary>
        /// <param name="invocation">包含被拦截的方法信息</param>
        public void Intercept(IInvocation invocation)
        {
            //var UserName = _accessor.HttpContext.User.Identity.Name;

            //记录被拦截方法信息的日志信息
            var dataIntercept =
                //$"【当前操作用户】:{UserName}\r\n"+
                $"【当前执行方法】:{invocation.Method.Name}" +
                $"【携带的参数有】：{string.Join(",", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n";

            try
            {
                MiniProfiler.Current.Step($"执行services方法：{invocation.Method.Name}()->");
                //在被拦截的方法执行完毕后 继续执行当前方法
                invocation.Proceed();

                // 异步获取异常，先执行
                if (IsAsyncMethod(invocation.Method))
                {

                    //Wait task execution and modify return value
                    if (invocation.Method.ReturnType == typeof(Task))
                    {
                        invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally(
                            (Task)invocation.ReturnValue,
                            async () => await SuccessAction(invocation, dataIntercept),//成功时执行    
                            ex =>
                            {
                                LogEx(ex, dataIntercept);
                            });
                    }
                    else
                    {
                        invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                            invocation.Method.ReturnType.GenericTypeArguments[0],
                            invocation.ReturnValue,
                            async () => await SuccessAction(invocation, dataIntercept),/*成功时执行*/
                            ex =>
                            {
                                LogEx(ex, dataIntercept);
                            });
                    }

                }
                else//Task<TResult>
                {//同步1

                }
            }
            catch (Exception ex)//同步2
            {
                LogEx(ex, dataIntercept);
            }

            //_hubContext.Clients.All.SendAsync("ReceiveUpdate", LogLock.GetLogData()).Wait();

        }

        //public void Intercept(IInvocation invocation)
        //{
        //    //记录被拦截方法信息的日志信息
        //    var dataIntercept = $"{DateTime.Now.ToString("yyyyMMddHHmmss")} " +
        //        $"当前执行方法：{ invocation.Method.Name} " +
        //        $"参数是： {string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n";

        //    //在被拦截的方法执行完毕后 继续执行当前方法
        //    invocation.Proceed();



        //    dataIntercept += ($"被拦截方法执行完毕，返回结果：{invocation.ReturnValue}");

        //    #region 输出到当前项目日志
        //    var path = Directory.GetCurrentDirectory() + @"\Log";
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }

        //    string fileName = path + $@"\InterceptLog-{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";

        //    StreamWriter sw = File.AppendText(fileName);
        //    sw.WriteLine(dataIntercept);
        //    sw.Close();
        //    #endregion
        //}

        private void LogEx(Exception ex, string dataIntercept)
        {
            if (ex != null)
            {
                //执行的service中，收录异常
                MiniProfiler.Current.CustomTiming("Errors", ex.Message);

                //执行的service ，捕获异常
                dataIntercept += ($"【执行完成结果】：方法中出现异常：{ex.Message + ex.InnerException}\r\n");

                //异常日志有详细的堆栈信息
                Parallel.For(0, 1, e =>
                {
                    LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept });
                });
            }
        }

        private async Task SuccessAction(IInvocation invocation, string dataIntercept)
        {
            var type = invocation.Method.ReturnType;
            if (typeof(Task).IsAssignableFrom(type))
            {
                var resultProperty = type.GetProperty("Result");
                dataIntercept += ($"【执行完成结果】:{JsonConvert.SerializeObject(resultProperty.GetValue(invocation.ReturnValue))}");
            }
            else
            {
                dataIntercept += ($"【执行完成结果】：{invocation.ReturnValue}");
            }

            Parallel.For(0, 1, e =>
            {
                LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept });
            });
        }

        /// <summary>
        /// 是否为异步方法
        /// </summary>
        /// <param name="method">MethodInfo</param>
        /// <returns></returns>
        private static bool IsAsyncMethod(MethodInfo method)
        {
            return (method.ReturnType == typeof(Task) ||
                 (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                 );
        }


    }

    internal static class InternalAsyncHelper
    {

        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                await actualReturnValue;
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static async Task<T> AwaitTaskWithPostActionAndFinallyAndGetResult<T>(Task<T> actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                var result = await actualReturnValue;
                await postAction();
                return result;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static object CallAwaitTaskWithPostActionAndFinallyAndGetResult(Type taskReturnType, object actualReturnValue, Func<Task> action, Action<Exception> finalAction)
        {
            var obj = typeof(InternalAsyncHelper)
                .GetMethod("AwaitTaskWithPostActionAndFinallyAndGetResult",
                BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(taskReturnType)
                .Invoke(null, new object[] { actualReturnValue, action, finalAction });
            return obj;
        }
    }
}
