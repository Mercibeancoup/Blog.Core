using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Common.Helper
{
    /// <summary>
    /// 对象object的扩展类
    /// </summary>
    public static class UtilConvet
    {
        #region 对象转换成int数据
        /// <summary>
        /// 对象转换成int数据
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue)
        {
            int reval = 0;
            if (thisValue == null)
            {
                return 0;
            }
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return reval;
        }

        /// <summary>
        /// 对象转换成int数据
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <param name="errorValue">转换失败时的错误数据</param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue, int errorValue)
        {
            int reval = 0;
            if (thisValue != null & thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }

        /// <summary>
        /// 对象转换成int数据(正数)
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static double ObjToMoney(this object thisValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }

        /// <summary>
        /// 对象转换成int数据(正数)
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <param name="errorValue">转换失败时的错误数据</param>
        /// <returns></returns>
        public static double ObjToMoney(this object thisValue, double errorValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        #endregion

        #region 对象转换成字符串
        /// <summary>
        /// 对象转换成字符串
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue)
        {
            return thisValue == null ? "" : thisValue.ToString().Trim();
        }
        /// <summary>
        /// 对象转换成字符串
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <param name="errorValue">转换失败时的错误数据</param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue, string errorValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return errorValue;
        }
        #endregion

        #region 对象转换成十进制数据
        /// <summary>
        /// 对象转换成十进制数据
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <returns></returns>
        public static Decimal ObjToDecimal(this object thisValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }

        /// <summary>
        /// 对象转换成十进制数据
        /// </summary>
        /// <param name="thisValue">需要转换的对象</param>
        /// <param name="errorValue">转换失败时的错误数据</param>
        /// <returns></returns>
        public static Decimal ObjToDecimal(this object thisValue, decimal errorValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        #endregion

        #region 对象转换成时间类型数据
        /// <summary>
        /// 对象转换成时间类型数据
        /// </summary>
        /// <param name="thisValue">需要转换的数据</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }
            return reval;
        }
        /// <summary>
        /// 对象转换成时间类型数据
        /// </summary>
        /// <param name="thisValue">需要转换的数据</param>
        /// <param name="errorValue">转换失败的时的数据</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }
        #endregion

        #region 对象转换成bool变量
        /// <summary>
        /// 对象转换成bool变量
        /// </summary>
        /// <param name="thisValue">需要转换的数据</param>
        /// <returns></returns>
        public static bool ObjToBool(this object thisValue)
        {
            bool reval = false;
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return reval;
        }

        #endregion

    }
}
