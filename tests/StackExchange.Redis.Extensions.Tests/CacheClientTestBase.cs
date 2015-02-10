using System;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Tests.Extensions;

namespace StackExchange.Redis.Extensions.Tests
{
    public class CacheClientTestBase : IDisposable
    {
        protected readonly StackExchangeRedisCacheClient Sut;
        protected readonly IDatabase Db;
        protected ISerializer Serializer;

        public virtual void OnInitialize() { }
        public CacheClientTestBase(ISerializer serializer)
        {
            //Hostname = "bmwremarketingqadev.redis.cache.windows.net";
            //Password = "hGRkEYhVGv2TMfA2aVoHdjn6y+7qslGFnV4vbuoJvJM=";
            //Port = 6380;

            //var connectionString = string.Format("{0}:{1},ssl=true,password={2}", "localhost", 6380, "password");
            var connectionString = string.Format("{0}:{1}", "192.168.59.103", 6379);
            var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            Db = connectionMultiplexer.GetDatabase();
            Serializer = serializer;
            Sut = new StackExchangeRedisCacheClient(connectionMultiplexer, Serializer);
        }
        public void Dispose()
        {
            Db.FlushDatabase();
            Db.Multiplexer.Dispose();
            Sut.Dispose();
        }
    }
}
