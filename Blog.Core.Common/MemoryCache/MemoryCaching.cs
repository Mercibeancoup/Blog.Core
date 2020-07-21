using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Common.MemoryCache
{
    /// <summary>
    /// 实例化缓存接口ICaching
    /// </summary>
    public class MemoryCaching : ICaching
    {
        //using Microsoft.Extensions.Caching.Memory;
        private IMemoryCache _memeryCache;

        public MemoryCaching(IMemoryCache memeryCache)
        {
            _memeryCache = memeryCache;
        }
        public object Get(string cacheKey)
        {
            return _memeryCache.Get(cacheKey);
        }

        public void Set(string cacheKey, object cacheValue)
        {
            _memeryCache.Set(cacheKey, cacheValue, TimeSpan.FromSeconds(7200));
   
        }
    }
}
