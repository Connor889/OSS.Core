﻿using System;
using Newtonsoft.Json;
using OSS.Common.Plugs.CachePlug;
using StackExchange.Redis;

namespace OSS.Plugs.RedisCache
{
    /// <summary>
    /// redis缓存实现类
    /// </summary>
    public class StackRedisPlug : ICachePlug
    {
        //redis数据库连接字符串
        private readonly string ConnectionStr = null;
        // "mycache.redis.cache.windows.net,abortConnect=false, ssl=true,password=..."


        private readonly int _db = 0;
        //  静态变量  保证  sns_center模块  和 其他模块使用的是不同实例的相同链接
        private static ConnectionMultiplexer connection;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="connectStr"></param>
        public StackRedisPlug(int db, string connectStr)
        {
            ConnectionStr = connectStr;
            this._db = db;
        }

        /// <summary>
        /// 缓存数据库
        /// </summary>
        public ConnectionMultiplexer CacheConnection
        {
            get
            {
                if (connection == null || !connection.IsConnected)
                {
                    connection = new Lazy<ConnectionMultiplexer>(()=>ConnectionMultiplexer.Connect(ConnectionStr)).Value; 
                }
                return connection;
            }
        }

        /// <summary>
        /// 缓存数据库
        /// </summary>
        public IDatabase CacheRedis => CacheConnection.GetDatabase(_db);

        /// <summary>
        /// 添加缓存，已存在不更新
        /// </summary>
        /// <typeparam name="T">添加缓存对象类型</typeparam>
        /// <param name="key">添加对象的key</param>
        /// <param name="obj">值</param>
        /// <param name="slidingExpiration">缓存时间 （redis目前都用绝对的）</param>
        /// <param name="absoluteExpiration"> 绝对过期时间（此字段无用 redis目前都用绝对的） </param>
        /// <returns>是否添加成功</returns>
        private bool Add<T>(string key, T obj, TimeSpan slidingExpiration, DateTime? absoluteExpiration)
        {
            if (slidingExpiration == TimeSpan.Zero && absoluteExpiration == null)
                throw new ArgumentNullException(nameof(slidingExpiration), "缓存过期时间不正确,需要设置固定过期时间或者相对过期时间");

            if (obj == null)
                return false;

            var jsonStr = JsonConvert.SerializeObject(obj);

            if (slidingExpiration == TimeSpan.Zero)
            {
                slidingExpiration = new TimeSpan(Convert.ToDateTime(absoluteExpiration).Ticks) - new TimeSpan(DateTime.Now.Ticks);
            }
            return CacheRedis.StringSet(key, jsonStr, slidingExpiration);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="obj"></param>
        ///// <param name="slidingExpiration"></param>
        ///// <param name="absoluteExpiration"></param>
        ///// <returns></returns>
        //public bool AddOrUpdate<T>(string key, T obj, TimeSpan slidingExpiration, DateTime? absoluteExpiration = null)
        //{
        //    return Add(key, obj, slidingExpiration, absoluteExpiration);
        //}

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            string value = CacheRedis.StringGet(key);
            if (string.IsNullOrEmpty(value))
                return default(T);

            T result = JsonConvert.DeserializeObject<T>(value);
            return result;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return CacheRedis.KeyDelete(key);
        }

        public bool Set<T>(string key, T obj, TimeSpan slidingExpiration)
        {
            return Add(key, obj, slidingExpiration, null);
        }


        [Obsolete("Redis 中没有提供此缓存过期方式！")]
        public bool Set<T>(string key, T obj, DateTime absoluteExpiration)
        {
            throw new Exception("Redis 暂不支持此方法！");
        }


        [Obsolete("不再建议使用此方法！")]
        public bool AddOrUpdate<T>(string key, T obj, TimeSpan slidingExpiration, DateTime? absoluteExpiration = null)
        {
            return Add(key, obj, slidingExpiration, absoluteExpiration);
        }
    }
}
