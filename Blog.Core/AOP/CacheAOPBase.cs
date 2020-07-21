using Blog.Core.Common.Helper;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Blog.Core.AOP
{
    public abstract class CacheAOPBase : IInterceptor
    {
        /// <summary>
        /// AOP的拦截方法
        /// </summary>
        /// <param name="invocation"></param>
        public abstract void Intercept(IInvocation invocation);

        /// <summary>
        /// 自定义缓存key
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        protected string CustomCacheKey(IInvocation invocation)
        {
            var typeName = invocation.TargetType.Name;
            var methodName = invocation.Method.Name;
            var methodArguments = invocation.Arguments.Select(GetArgumentValue).Take(3).ToList();

            var key = $"{typeName}:{methodName}";
            foreach (var item in methodArguments)
            {
                key = $"{key}{item}";
            }
            return key.Trim(':');
        }


        /// <summary>
        /// object 转 string
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected static string GetArgumentValue(object arg)
        {
            if (arg is DateTime || arg is DateTime?)
                return ((DateTime)arg).ToString("yyyyMMddHHmmss");

            if (arg is string || arg is ValueType || arg is Nullable)
                return arg.ToString();

            if (arg != null)
            {
                if (arg is Expression)
                {
                    var obj = arg as Expression;
                    var result = Resolve(obj);
                    return MD5Helper.MD5Encrypt16(result);
                }
                else if (arg.GetType().IsClass)
                {
                    return MD5Helper.MD5Encrypt16(Newtonsoft.Json.JsonConvert.SerializeObject(arg));
                }
            }
            return string.Empty;
        }

        private static string Resolve(Expression expression)
        {
            if (expression is LambdaExpression)
            {
                var lambda = expression as LambdaExpression;
                expression = lambda.Body;
                return Resolve(expression);
            }
            if (expression is BinaryExpression)
            {
                var binary = expression as BinaryExpression;
                //解析x=>x.Name=="123" x.Age==123这类
                if (binary.Left is MemberExpression && binary.Right is ConstantExpression)
                {
                    return ResolveFunc(binary.Left, binary.Right, binary.NodeType);
                }
                //解析x=>x.Name.Contains("xxx")==false这类的
                if (binary.Left is MethodCallExpression && binary.Right is ConstantExpression)
                {
                    object value = (binary.Right as ConstantExpression).Value;
                    return ResolveLinqToObject(binary.Left, value, binary.NodeType);
                }
                // 解析x=>x.Date == DateTime.Now这种
                if ((binary.Left is MemberExpression && binary.Right is MemberExpression)
                    || (binary.Left is MemberExpression && binary.Right is UnaryExpression)
                    )
                {
                    LambdaExpression lambda = Expression.Lambda(binary.Right);
                    Delegate fn = lambda.Compile();
                    ConstantExpression value = Expression.Constant(fn.DynamicInvoke(null), binary.Right.Type);
                    return ResolveFunc(binary.Left, value, binary.NodeType);
                }

            }
            if (expression is UnaryExpression)
            {
                UnaryExpression unary = expression as UnaryExpression;
                //解析!x=>x.Name.Contains("xxx")或!array.Contains(x.Name)这类
                if (unary.Operand is MethodCallExpression)
                    return ResolveLinqToObject(unary.Operand, false);
                //解析x=>!x.isDeletion这样的 
                if (unary.Operand is MemberExpression && unary.NodeType == ExpressionType.Not)
                {
                    ConstantExpression constant = Expression.Constant(false);
                    return ResolveFunc(unary.Operand, constant, ExpressionType.Equal);
                }
            }
            //解析x=>x.isDeletion这样的 
            if (expression is MemberExpression && expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression member = expression as MemberExpression;
                ConstantExpression constant = Expression.Constant(true);
                return ResolveFunc(member, constant, ExpressionType.Equal);
            }
            //x=>x.Name.Contains("xxx")或array.Contains(x.Name)这类
            if (expression is MethodCallExpression)
            {
                MethodCallExpression methodcall = expression as MethodCallExpression;
                return ResolveLinqToObject(methodcall, true);
            }
            var body = expression as BinaryExpression;
            //已经修改过代码body应该不会是null值了
            if (body == null)
                return string.Empty;
            var Operator = GetOperator(body.NodeType);
            var Left = Resolve(body.Left);
            var Right = Resolve(body.Right);
            string Result = string.Format("({0} {1} {2})", Left, Operator, Right);
            return Result;
        }

        private static string ResolveLinqToObject(Expression expression, object value, ExpressionType? expressiontype = null)
        {
            var MethodCall = expression as MethodCallExpression;
            var MethodName = MethodCall.Method.Name;
            switch (MethodName)
            {
                case "Contains":
                    if (MethodCall.Object != null)
                        return Like(MethodCall);
                    return In(MethodCall, value);
                case "Count":
                    return Len(MethodCall, value, expressiontype.Value);
                case "LongCount":
                    return Len(MethodCall, value, expressiontype.Value);
                default:
                    throw new Exception(string.Format("不支持{0}方法的查找！", MethodName));
            }
        }

        private static string ResolveFunc(Expression left, Expression right, ExpressionType expressionType)
        {
            var Name = (left as MemberExpression).Member.Name;
            var Value = (right as ConstantExpression).Value;
            var Opeator = GetOperator(expressionType);
            return Name + Opeator + Opeator;
        }

        /// <summary>
        /// 节点类型种类
        /// </summary>
        /// <param name="expressionType">表达式类型</param>
        /// <returns></returns>
        private static string GetOperator(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.And:
                    return "and";
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.Or:
                    return "or";
                case ExpressionType.OrElse:
                    return "or";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                default:
                    throw new Exception(string.Format("不支持{0}此种运算符查找！" + expressionType));
            }
        }

        private static string Like(MethodCallExpression expression)
        {
            var Temp = expression.Arguments[0];
            LambdaExpression lambda = Expression.Lambda(Temp);
            Delegate fn = lambda.Compile();
            var tempValue = Expression.Constant(fn.DynamicInvoke(null), Temp.Type);
            string Value = string.Format("%{0}%", tempValue);
            string Name = (expression.Object as MemberExpression).Member.Name;
            string Result = string.Format("{0} like {1}", Name, Value);
            return Result;
        }
        private static string In(MethodCallExpression expression, object isTrue)
        {
            var Argument1 = (expression.Arguments[0] as MemberExpression).Expression as ConstantExpression;
            var Argument2 = expression.Arguments[1] as MemberExpression;
            var Field_Array = Argument1.Value.GetType().GetFields().First();
            object[] Array = Field_Array.GetValue(Argument1.Value) as object[];
            List<string> SetInPara = new List<string>();
            for (int i = 0; i < Array.Length; i++)
            {
                string Name_para = "InParameter" + i;
                string Value = Array[i].ToString();
                SetInPara.Add(Value);
            }
            string Name = Argument2.Member.Name;
            string Operator = Convert.ToBoolean(isTrue) ? "in" : " not in";
            string CompName = string.Join(",", SetInPara);
            string Result = string.Format("{0} {1} ({2})", Name, Operator, CompName);
            return Result;
        }
        private static string Len(MethodCallExpression expression, object value, ExpressionType expressiontype)
        {
            object Name = (expression.Arguments[0] as MemberExpression).Member.Name;
            string Operator = GetOperator(expressiontype);
            string Result = string.Format("len({0}){1}{2}", Name, Operator, value.ToString());
            return Result;
        }

    }
}
