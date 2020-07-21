using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Blog.Core.IRepository;
using Blog.Core.IRepository.UnitWork;
using Blog.Core.Model.Models;
using Blog.Core.Repository.BASE;
using Blog.Core.Repository.sugar;
using SqlSugar;

namespace Blog.Core.Repository
{
    public class AdvertisementRepository : BaseRepository<Advertisement>, IAdvertisementRepository
    {
        public AdvertisementRepository(IUnitOfWork unitOfWork):base(unitOfWork)
        {

        }
        //因为持久层的基类已经实现了对应的增删改查，所以其他方法以下方法和构造全部不用定义实现
        //private DbContext context;
        //private SqlSugarClient db;
        //private SimpleClient<Advertisement> entityDB;

        //internal SqlSugarClient Db
        //{
        //    get { return db; }
        //    private set { db = value; }
        //}
        //public   DbContext Context
        //{
        //    get { return context; }
        //    set { context = value; }
        //}
        //public AdvertisementRepository()
        //{
        //    DbContext.Init(BaseDBConfig.ConnectionString,DbContext.DbType);
        //    context = DbContext.GetDbContext();
        //    db = context.Db;
        //    entityDB = context.GetEntityDB<Advertisement>(db);
        //}


        //public int Sum()
        //{
        //    return 0;
        //}

        //public int Add(Advertisement model)
        //{
        //    var i = db.Insertable(model).ExecuteReturnBigIdentity();
        //    return i.ObjToInt();
        //}

        //public bool Delete(Advertisement model)
        //{
        //    var i = db.Deleteable(model).ExecuteCommand();
        //    return i>0;
        //}

        //public List<Advertisement> Query(Expression<Func<Advertisement, bool>> whereExpression)
        //{
        //    return entityDB.GetList(whereExpression);
        //    throw new NotImplementedException();
        //}

        //public bool Update(Advertisement model)
        //{
        //    //这种方式会以主键为条件
        //    var i = db.Updateable(model).ExecuteCommand();
        //    return i>0;
        //}
    }
}
