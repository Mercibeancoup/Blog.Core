using Blog.Core.IRepository.BASE;
using Blog.Core.IRepository.UnitWork;

using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using System.Linq;

using Blog.Core.Model;
using Blog.Core.Common.Helper;
using Blog.Core.Common.DB;
using StackExchange.Profiling;

namespace Blog.Core.Repository.BASE
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {
        #region (过时)基本字段,属性,构造方法
        //为了松耦合及多数据库操作，移动至common
        //private DbContext context;
        //private SqlSugarClient _db;
        //private SimpleClient<TEntity> entityDB;

        //internal SqlSugarClient Db
        //{
        //    get { return _db; }
        //    private set { _db = value; }
        //}
        //public DbContext Context
        //{
        //    get { return context; }
        //    set { context = value; }
        //}

        //internal SimpleClient<TEntity> EntityDB
        //{
        //    get { return entityDB; }
        //    private set { entityDB = value; }
        //}

        //public BaseRepository()
        //{
        //    DbContext.Init(BaseDBConfig.ConnectionString, DbContext.DbType);
        //    context = DbContext.GetDbContext();
        //    _db = context.Db;
        //    entityDB = context.GetEntityDB<TEntity>(db);
        //}



        #endregion

        #region 基本字段，属性，构造方法
        private readonly IUnitOfWork _unitOfWork;

        private SqlSugarClient _dbBase;

        private ISqlSugarClient _db
        {
            get
            {
                /* 如果要开启多库支持，
                * 1、在appsettings.json 中开启MutiDBEnabled节点为true，必填
                * 2、设置一个主连接的数据库ID，节点MainDB，对应的连接字符串的Enabled也必须true，必填
                */
                if (Appsettings.app(new string[] { "MutiDBEnabled" }).ObjToBool())
                {
                    var objs = typeof(TEntity).GetTypeInfo().GetCustomAttributes(typeof(SugarTable), true);
                    if (objs.FirstOrDefault(x => x.GetType() == typeof(SugarTable)) is SugarTable sugarTable)
                    {
                        _dbBase.ChangeDatabase(sugarTable.TableDescription.ToLower());
                    }
                    else
                    {
                        _dbBase.ChangeDatabase(MainDb.CurrentDbConnId.ToLower());
                    }
                }

                if(Appsettings.app(new string[] { "AppSettings", "SqlAOP", "Enabled" }).ObjToBool())
                {
                    _dbBase.Aop.OnLogExecuting = (sql, pars) =>//SQL执行事件
                    {
                        Parallel.For(0, 1, e =>
                        {
                            //用于分析程应用
                            MiniProfiler.Current.CustomTiming("SQL", GetParas(pars) + "【SQL参数】：" + sql);
                            //LogLock.OutSql2Log("SqlLog", new string[] { GetParas(pars), "【SQL语句】：" + sql });
                        });
                    };

                }
                return _dbBase;
            }

        }



        internal ISqlSugarClient Db
        {
            get { return _db; }
        }

        public BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dbBase = unitOfWork.GetDbClient();
        }
        private string GetParas(SugarParameter[] pars)
        {
            string key = "【SQL参数】：";
            foreach (var param in pars)
            {
                key += $"{param.ParameterName}:{param.Value}\n";
            }

            return key;
        }
        #endregion

        #region 查询数据
        /// <summary>
        /// 功能描述:根据ID查询一条数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="objId">id（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <returns></returns>
        public async Task<TEntity> QueryByID(object objId)
        {
            return await Task.Run(() => _db.Queryable<TEntity>().InSingle(objId));
        }

        /// <summary>
        /// 功能描述:根据ID查询一条数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="objId">id（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <param name="blnUseCache">是否使用缓存</param>
        /// <returns>数据实体</returns>
        public async Task<TEntity> QueryByID(object objId, bool blnUseCache = false)
        {
            return await Task.Run(() => _db.Queryable<TEntity>().WithCacheIF(blnUseCache).InSingle(objId));
        }

        /// <summary>
        /// 功能描述:根据ID查询数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="lstIds">id列表（必须指定主键特性 [SugarColumn(IsPrimaryKey=true)]），如果是联合主键，请使用Where条件</param>
        /// <returns>数据实体列表</returns>
        public async Task<List<TEntity>> QueryByIDs(object[] lstIds)
        {
            return await Task.Run(() => _db.Queryable<TEntity>().In(lstIds).ToList());
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 写入实体数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns></returns>
        public async Task<int> Add(TEntity entity)
        {
            var i = await Task.Run(() => _db.Insertable<TEntity>(entity).ExecuteReturnBigIdentity());
            return (int)i;
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity entity)
        {
            //这种方式会以主键为条件
            return await Task.Run(() => _db.Updateable(entity).ExecuteCommand() > 0);

        }

        /// <summary>
        /// 使用条件进行更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity entity, string strWhere)
        {
            return await Task.Run(() => _db.Updateable(entity).Where(strWhere).ExecuteCommand() > 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<bool> Update(string strSql, SugarParameter[] parameters = null)
        {
            return await Task.Run(() => _db.Ado.ExecuteCommand(strSql, parameters) > 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="lstColumns">需要更新的栏位</param>
        /// <param name="lstIgnoreColumns">不更新的栏位</param>
        /// <param name="strWhere">条件语句</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity entity, List<string> lstColumns = null, List<string> lstIgnoreColumns = null, string strWhere = "")
        {
            IUpdateable<TEntity> up = await Task.Run(() => _db.Updateable(entity));
            if (lstIgnoreColumns != null & lstIgnoreColumns.Count > 0)
            {
                up = await Task.Run(
                    () => up.IgnoreColumns(lstColumns.ToArray())
                    );
            }
            if (lstColumns != null && lstColumns.Count > 0)
            {
                up = await Task.Run(
                    () => up.UpdateColumns(lstColumns.ToArray())
                );
            }
            if (!string.IsNullOrEmpty(strWhere))
            {
                up = await Task.Run(() => up.Where(strWhere));
            }

            return await Task.Run(() => up.ExecuteCommand()) > 0;

        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除指定entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Delete(TEntity entity)
        {
            return await Task.Run(
                () => _db.Deleteable<TEntity>(entity).ExecuteCommand()
                > 0);
        }


        /// <summary>
        /// 根据实体删除一条数据
        /// </summary>
        /// <param name="id">实体类</param>
        /// <returns></returns>
        public async Task<bool> DeleteById(object id)
        {
            return await Task.Run(
                () => _db.Deleteable<TEntity>(id).ExecuteCommand()
                > 0);
        }


        /// <summary>
        /// 删除指定ID集合的数据(批量删除)
        /// </summary>
        /// <param name="model">主键ID集合</param>
        /// <returns></returns>
        public async Task<bool> DeleteByIds(object[] ids)
        {
            return await Task.Run(() => _db.Deleteable<TEntity>().In(ids).ExecuteCommand() > 0);
        }

        //public async Task<bool> Delete(TEntity entity)
        //{

        //    return await Task.Run(
        //        () => _db.Deleteable<TEntity>(entity).ExecuteCommand()
        //        > 0);
        //}
        #endregion

        #region 查询所有数据

        /// <summary>
        /// 功能描述:查询所有数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query()
        {
            return await Task.Run(() => _db.Queryable<TEntity>().ToList());
        }

        /// <summary>
        /// 功能描述:查询数据列表
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string strWhere)
        {
            return await Task.Run(
                () => _db.Queryable<TEntity>().WhereIF(!string.IsNullOrEmpty(strWhere), strWhere).ToList()
                );
        }

        /// <summary>
        /// 功能描述:查询数据列表
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="whereExpression">whereExpression</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await Task.Run(
                () => _db.Queryable<TEntity>().WhereIF(whereExpression!=null,whereExpression).ToList()
            );
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
            return await Task.Run(
                () => _db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                .WhereIF(whereExpression != null, whereExpression)
                .ToList());
        }

        /// <summary>
        /// 功能描述:查询一个列表
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="orderByExpression">排序表达式</param>
        /// <param name="isAsc">是否正序排序</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression, bool isAsc = true)
        {
            return await Task.Run(
                () => _db.Queryable<TEntity>()
                .OrderByIF(orderByExpression != null, orderByExpression, isAsc ? OrderByType.Asc : OrderByType.Desc)
                .WhereIF(whereExpression != null, whereExpression)
                .ToList());
        }

        /// <summary>
        /// 功能描述:查询一个列表
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string strWhere, string strOrderByFileds)
        {
            return await Task.Run(
                () => _db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                .WhereIF(!string.IsNullOrEmpty(strWhere), strWhere)
                .ToList());
        }




        #region 分页查询

        /// <summary>
        /// 功能描述:查询前N条数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="intTop">前N条</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int intTop, string strOrderByFileds)
        {
            return await Task.Run(
                () => _db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                .WhereIF(whereExpression != null, whereExpression)
                .Take(intTop)
                .ToList()
                );
        }

        /// <summary>
        /// 功能描述:查询前N条数据
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="intTop">前N条</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(string strWhere, int intTop, string strOrderByFileds)
        {
            return await Task.Run(() => _db.Queryable<TEntity>()
           .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
           .WhereIF(!string.IsNullOrEmpty(strWhere), strWhere)
           .Take(intTop)
           .ToList());
        }

        /// <summary>
        /// 功能描述:分页查询
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="intPageIndex">页码（下标0）</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public Task<List<TEntity>> Query(string strWhere, int intPageIndex, int intPageSize, string strOrderByFileds)
        {
            return Task.Run(
                () => _db.Queryable<TEntity>()
                .WhereIF(!string.IsNullOrEmpty(strWhere), strWhere)
                .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                .ToPageList(intPageIndex, intPageSize));
        }

        /// <summary>
        /// 功能描述:分页查询
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="intPageIndex">页码（下标0）</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, int intPageIndex, int intPageSize, string strOrderByFileds)
        {
            return await Task.Run(
                () => _db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                .WhereIF(whereExpression != null, whereExpression)
                .ToPageList(intPageIndex, intPageSize)
                );
        }

        /// <summary>
        /// 功能描述:分页查询
        /// 作　　者:Blog.Core
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="intPageIndex">页码（下标0）</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns></returns>
        public async Task<List<TEntity>> QueryPage(Expression<Func<TEntity, bool>> whereExpression, int intPageIndex = 0, int intPageSize = 20, string strOrderByFileds = null)
        {
            return await Task.Run(() =>
            _db.Queryable<TEntity>()
            .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
            .WhereIF(whereExpression != null, whereExpression)
            .ToPageList(intPageIndex, intPageSize));
        }




        #endregion

        #endregion

    }
}
