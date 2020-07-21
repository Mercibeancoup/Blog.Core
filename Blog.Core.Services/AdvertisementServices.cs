using Blog.Core.IRepository;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Blog.Core.Services.BASE;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Blog.Core.Services
{
    public class AdvertisementServices : BaseServices<Advertisement>,IAdvertisementServices
    {

        private IAdvertisementRepository _dal;
        public AdvertisementServices(IAdvertisementRepository dal)
        {
            _dal = dal;
            base.baseDal = dal;
        }
        //继承BaseServices，无需实现任何功能
        //public readonly IAdvertisementRepository _advertisementRepository;

        //public AdvertisementServices(IAdvertisementRepository advertisementRepository)
        //{
        //    _advertisementRepository = advertisementRepository;
        //}

        //public IAdvertisementRepository dal = new AdvertisementRepository();

        //public int Add(Advertisement model)
        //{
        //    return dal.Add(model);
        //}

        //public bool Delete(Advertisement model)
        //{
        //    return dal.Delete(model);

        //}

        //public List<Advertisement> Query(Expression<Func<Advertisement, bool>> whereExpression)
        //{
        //    return dal.Query(whereExpression);
        //}


        ////public int Sum()
        ////{
        ////    return _advertisementRepository.Sum();
        ////}

        //public int Sum()
        //{
        //    return dal.Sum();
        //}

        //public bool Update(Advertisement model)
        //{
        //    return dal.Update(model);

        //}
    }
}
