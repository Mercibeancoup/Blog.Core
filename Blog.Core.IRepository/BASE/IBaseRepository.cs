using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Blog.Core.Model;
using SqlSugar;

namespace Blog.Core.IRepository.BASE
{
    public interface IBaseRepository<TEntity> where TEntity :class
    {
        Task<TEntity> QueryByID(object objId);
        Task<TEntity> QueryByID(object objId, bool blnUseCache = false);
        Task<List<TEntity>> QueryByIDs(object[] lstIds);

        Task<int> Add(TEntity entity);

        Task<bool> DeleteById(object id);

        //[Obsolete("this function is obsolete ,please use the fuction Delete(object id) instead")]
        Task<bool> Delete(TEntity entity);

        Task<bool> DeleteByIds(object[] ids);

        Task<bool> Update(TEntity entity);
        Task<bool> Update(TEntity entity, string strWhere);
        Task<bool> Update(string strSql, SugarParameter[] parameters = null);
        Task<bool> Update(TEntity entity, List<string> lstColumns = null, List<string> lstIgnoreColumns = null, string strWhere = "");


        Task<List<TEntity>> Query();
        Task<List<TEntity>> Query(string strWhere);
        Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression);
        Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, string strOrderByFileds);
        Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression, bool isAsc = true);
        Task<List<TEntity>> Query(string strWhere, string strOrderByFileds);
        Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int intTop, string strOrderByFileds);
        Task<List<TEntity>> Query(string strWhere, int intTop, string strOrderByFileds);
        Task<List<TEntity>> Query(
            Expression<Func<TEntity, bool>> whereExpression, int intPageIndex, int intPageSize, string strOrderByFileds);
        Task<List<TEntity>> Query(string strWhere, int intPageIndex, int intPageSize, string strOrderByFileds);
        Task<List<TEntity>> QueryPage(Expression<Func<TEntity, bool>> whereExpression, int intPageIndex = 0, int intPageSize = 20, string strOrderByFileds = null);
    }
}
