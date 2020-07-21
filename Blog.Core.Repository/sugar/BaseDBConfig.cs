using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blog.Core.Repository.sugar
{
    [Obsolete("this function is obsolete,please try to attch the same function in Blog.Core.Common")]
    public class BaseDBConfig
    {
        //public static string ConnectionString = File.ReadAllText(@".\dbCountPsw1.txt").Trim();

        public static string ConnectionString
        {
            get;
            set;
        } //= File.ReadAllText(@".\dbCountPsw1.txt").Trim();
    }
}
