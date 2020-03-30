using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Configuration;
using Xunit;

namespace StackExchange.Redis.Extensions.LegacyConfiguration.Tests
{
    public class RedisCachingSectionHandlerTests : IDisposable
    {
        private RedisCachingSectionHandler sut;

        public RedisCachingSectionHandlerTests()
        {
            sut = new RedisCachingSectionHandler();
        }

        public void Dispose()
        {
            (sut as IDisposable)?.Dispose();
            sut = null;
        }

        [Fact]
        public void ReadingFromConfigurationFile_ShouldReturnValidValues()
        {
            //NOTE: This isn't a good test, need to find a way to inject a config file in memory (no idea how to do that)
            var cfg = RedisCachingSectionHandler.GetConfig();

            Assert.NotNull(cfg);
            Assert.NotNull(cfg.ServerEnumerationStrategy);
            Assert.NotNull(cfg.Hosts);
            Assert.True(cfg.AllowAdmin);
            Assert.False(cfg.Ssl);
            Assert.Equal(3000, cfg.ConnectTimeout);
            Assert.Equal(1000, cfg.SyncTimeout);
            Assert.Equal(24, cfg.Database);
            Assert.Equal(10, cfg.PoolSize);

            Assert.Equal(ServerEnumerationStrategy.ModeOptions.Single, cfg.ServerEnumerationStrategy.Mode);
            Assert.Equal(ServerEnumerationStrategy.TargetRoleOptions.PreferSlave, cfg.ServerEnumerationStrategy.TargetRole);
            Assert.Equal(ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable, cfg.ServerEnumerationStrategy.UnreachableServerAction);

            Assert.Single(cfg.Hosts);
            Assert.Equal("127.0.0.1", cfg.Hosts.First().Host);
            Assert.Equal(6379, cfg.Hosts.First().Port);
        }
    }
}
