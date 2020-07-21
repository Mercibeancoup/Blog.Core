using Blog.Core.IRepository.BASE;
using Blog.Core.IServices.BASE;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Services.BASE
{
    public class BaseServices<TEntity> : IBaseServices<TEntity> where TEntity : class, new()
    {
        //public IBaseRepository<TEntity> _baseDal= new BaseRepository<TEntity>();
        //基类不需要通过构造函数进行赋值，在子类中通过构造函数赋值即可
        internal IBaseRepository<TEntity> baseDal;

        #region 添加数据
        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="entity">博文实体类</param>
        /// <returns></returns>
        public async Task<int> Add(TEntity model)
        {
            return await baseDal.Add(model);
        }

        #endregion

        #region 删除数据
        /// <summary>
        /// 根据实体删除一条数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns></returns>
        public async Task<bool> Delete(TEntity model)
        {
            return await baseDal.Delete(model);
        }

        /// <summary>
        /// 根据实体删除一条数据
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public async Task<bool> DeleteById(object id)
        {
            return await baseDal.DeleteById(id);
        }

        public async Task<bool> DeleteByIds(object[] ids)
        {
            return await baseDal.DeleteByIds(ids);
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity model)
        {
            return await baseDal.Update(model);
        }

        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <param name="strWhere">条件</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity entity, string strWhere)
        {
            return await baseDal.Update(entity, strWhere);
        }

        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <param name="lstColumns">需更新栏位</param>
        /// <param name="lstIgnoreColumns">不需更新栏位</param>
        /// <param name="strWhere">条件</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity entity, List<string> lstColumns = null, List<string> lstIgnoreColumns = null, string strWhere = "")
        {
            return await baseDal.Update(entity, lstColumns, lstIgnoreColumns, strWhere);
        }
        #endregion

        #region 查询数据

        /// <summary>
        /// 功能描述:查询所有数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query()
        {
            return await baseDal.Query();
        }

        /// <summary>
        /// 功能描述:查询所有数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(string strWhere)
        {
            return await baseDal.Query(strWhere);
        }

        /// <summary>
        /// 功能描述:查询数据列表
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="whereExpression">whereExpression</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await baseDal.Query(whereExpression);
        }

        /// <summary>
        /// 功能描述:查询一个列表
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, string strOrderByFileds)
        {
            return await baseDal.Query(whereExpression, strOrderByFileds);
        }

        /// <summary>
        /// 功能描述:查询一个列表
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="orderByExpression">排序字段，如name asc,age desc</param>
        /// <param name="isAsc">是否顺序排列</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression, bool isAsc = true)
        {
            return await baseDal.Query(whereExpression, orderByExpression, isAsc);
        }
        /// <summary>
        /// 功能描述:查询一个列表
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(string strWhere, string strOrderByFileds)
        {
            return await baseDal.Query(strWhere, strOrderByFileds);
        }

        #region 查询前N条数据
        /// <summary>
        /// 查询前N条数据
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="intTop"></param>
        /// <param name="strOrderByFileds"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int intTop, string strOrderByFileds)
        {
            return await baseDal.Query(whereExpression, intTop, strOrderByFileds);
        }

        /// <summary>
        /// 查询前N条数据
        /// </summary>
        /// <param name="strWhere"></param>
        /// <param name="intTop"></param>
        /// <param name="strOrderByFileds"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(string strWhere, int intTop, string strOrderByFileds)
        {
            return await baseDal.Query(strWhere, intTop, strOrderByFileds);
        }
        #endregion


        #region 分页查询
        /// <summary>
        /// 功能描述:分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="intPageIndex">页码（下标0）</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int intPageIndex, int intPageSize, string strOrderByFileds)
        {
            return await baseDal.Query(whereExpression, intPageIndex, intPageSize, strOrderByFileds);
        }

        /// <summary>
        /// 功能描述:分页查询
        /// </summary>
        /// <param name="strWhere">条件表达式</param>
        /// <param name="intPageIndex">页码（下标0）</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string strWhere, int intPageIndex, int intPageSize, string strOrderByFileds)
        {
            return await baseDal.Query(strWhere, intPageIndex, intPageSize, strOrderByFileds);
        }

        /// <summary>
        /// 功能描述:分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="intPageIndex">页码（下标0）</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> QueryPage(Expression<Func<TEntity, bool>> whereExpression, int intPageIndex = 0, int intPageSize = 20, string strOrderByFileds = null)
        {
            return await baseDal.QueryPage(whereExpression, intPageIndex, intPageSize, strOrderByFileds);
        }

        #endregion

        /// <summary>
        /// 功能描述:根据ID查询一条数据
        /// 作　　者:AZLinli.Blog.Core
        /// </summary>
        /// <param name="objId">id（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <returns></returns>
        public async Task<TEntity> QueryByID(object objId)
        {
            return await baseDal.QueryByID(objId);
        }
        /// <summary>
        /// 功能描述:根据ID查询一条数据
        /// 作　　者:AZLinli.Blog.Core
        /// </summary>
        /// <param name="objId">id（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <param name="blnUseCache">是否使用缓存</param>
        /// <returns>数据实体</returns>
        public async Task<TEntity> QueryByID(object objId, bool blnUseCache = false)
        {
            return await baseDal.QueryByID(objId, blnUseCache);
        }

        /// <summary>
        /// 功能描述:根据ID列表查询数据
        /// 作　　者:AZLinli.Blog.Core
        /// </summary>
        /// <param name="lstIds">id列表（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <returns>数据实体列表</returns>
        public async Task<List<TEntity>> QueryByIDs(object[] lstIds)
        {
            return await baseDal.QueryByIDs(lstIds);
        }




        #endregion


    }
}
