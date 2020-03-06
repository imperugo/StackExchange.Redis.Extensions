using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Tests.Extensions;
using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests
{
    public class SimpleTestClass : IDisposable
    {
        private readonly IRedisCacheClient sut;
        private readonly ISerializer serializer;
        private readonly IDatabase db;

        private readonly RedisConfiguration redisConfiguration;
        private readonly IRedisCacheConnectionPoolManager connectionPoolManager;

        public SimpleTestClass()
        {
            redisConfiguration = new RedisConfiguration()
            {
                AbortOnConnectFail = true,
                KeyPrefix = "MyPrefix__",
                Hosts = new RedisHost[]
                {
                    new RedisHost { Host = "localhost", Port = 6379 }
                },
                AllowAdmin = true,
                ConnectTimeout = 3000,
                Database = 0,
                PoolSize = 1,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                }
            };

            this.serializer = new NewtonsoftSerializer();
            connectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);
            sut = new RedisCacheClient(connectionPoolManager, this.serializer, redisConfiguration);
            db = sut.GetDbFromConfiguration().Database;
        }

        protected IRedisCacheClient Sut => sut;

        public void Dispose()
        {
            db.FlushDatabase();
            db.Multiplexer.GetSubscriber().UnsubscribeAll();
            connectionPoolManager.Dispose();
        }

        [Fact]
        public async Task Test()
        {
            var response = await Sut.GetDbFromConfiguration().GetInfoAsync();

            Assert.NotNull(response);
            Assert.True(response.Any());
            Assert.Equal("6379", response["tcp_port"]);
        }
    }
}
