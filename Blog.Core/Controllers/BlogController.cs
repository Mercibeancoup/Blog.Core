using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Blog.Core.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    //[Authorize(Policy = "Admin")]
    public class BlogController : Controller
    {
                       
        private readonly IAdvertisementServices _advertisementServices;

        /// <summary>
        /// 采用构造函数注入services
        /// 其他还有通过setter方法，接口方法注入
        /// </summary>
        public BlogController(IAdvertisementServices advertisementServices)
        {
            _advertisementServices = advertisementServices;
        }



        /// <summary>
        /// 根据id获取数据
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpGet("{id}",Name="Get")]
        public async Task<List<Advertisement>> Get(int id)
        {
            return await _advertisementServices.Query(d => d.Id == id);
        }

    }
}
