using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using StackExchange.Redis.Extensions;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Jil;
using System.Configuration;

namespace Web
{
    public class RedisDb
    {
        //Redis Connection
        public static ConnectionMultiplexer Connection 
        {
            get { return LazyConnection.Value; } 
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection =
            new Lazy<ConnectionMultiplexer>(
                () =>
                {
                    return
                        ConnectionMultiplexer.Connect(GetRedisAddress());
                });



        //RedisCacheClient
        public static StackExchangeRedisCacheClient RedisCache
        {
            get { return LazyRedisCache.Value; }
        }

        private static readonly Lazy<StackExchangeRedisCacheClient> LazyRedisCache =
            new Lazy<StackExchangeRedisCacheClient>(
                () =>
                {
                    return new StackExchangeRedisCacheClient(new JilSerializer(), GetRedisAddress());                        
                });
        

        public static string GetRedisAddress()
        {
            return ConfigurationManager.AppSettings.Get("RedisAddress");
        }
    }
}