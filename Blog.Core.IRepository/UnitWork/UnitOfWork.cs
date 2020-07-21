using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.IRepository.UnitWork
{
    public interface IUnitOfWork
    {
        SqlSugarClient GetDbClient();


        void BeganTran();


        void CommitTran();


        void RollBackTran();

    }
}
