using Blog.Core.IRepository.UnitWork;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Repository.UnitWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISqlSugarClient _sqlSugarClient;

        public UnitOfWork(ISqlSugarClient sqlSugarClient)
        {
            _sqlSugarClient = sqlSugarClient;
        }


        /// <summary>
        /// 获取DB，保证唯一性
        /// </summary>
        /// <returns></returns>
        public SqlSugarClient GetDbClient()
        {
            // 必须要as，后边会用到切换数据库操作
            return _sqlSugarClient as SqlSugarClient;
        }

        public void BeganTran()
        {
            GetDbClient().BeginTran();
        }

        public void CommitTran()
        {
            GetDbClient().BeginTran();
        }

        public void RollBackTran()
        {
            GetDbClient().BeginTran();
        }
    }
}
