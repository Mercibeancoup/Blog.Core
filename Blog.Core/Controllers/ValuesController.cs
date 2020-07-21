using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Core.Controllers
{
    /// <summary>
    /// 自动生成API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
        // GET api/values
        /// <summary>
        /// 获取分页的数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Policy = "SystemAndAdmin")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        /// <summary>
        /// 获取{id}的对象信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles  = "Admin")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }


        /// <summary>
        /// 这是要隐藏的接口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [ApiExplorerSettings(IgnoreApi =true)]
        public string Ignore(int id)
        {
            return "value";
        }

        ///// <summary>
        ///// 这是Love
        ///// </summary>
        ///// <param name="love">model实体类参数</param>
        //[HttpPut("id")]       
        //public IActionResult PostLove([FromBody]Love love)
        //{
        //    return CreatedAtRoute("GetProduct", new { id = 1 });
        //}

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            return CreatedAtRoute("GetProduct", new { id = 1 });
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


    }
}
