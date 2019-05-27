using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Tests.Extensions;
using Xunit;
using Xunit.Abstractions;
namespace StackExchange.Redis.Extensions.Tests
{
	public class StressTest : IDisposable
	{
		private readonly ICacheClient sut;
		private readonly ITestOutputHelper output;
		private readonly IConnectionMultiplexer mux;

		public StressTest(ITestOutputHelper output)
		{
			this.output = output;
			mux = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				DefaultVersion = new Version(3, 0, 500),
				EndPoints = { { "localhost", 6379 } },
				AllowAdmin = true,
				ConnectTimeout = 5000
			});
			sut = new StackExchangeRedisCacheClient(mux, new NewtonsoftSerializer(),11);
		}

		public void Dispose()
		{
            if(sut != null)
            {
                sut.Database.FlushDatabase();
			    sut.Database.Multiplexer.GetSubscriber().UnsubscribeAll();
			    sut.Database.Multiplexer.Dispose();
			    sut.Dispose();
            }
		}

		private async Task PopulateDbAsync(string prefix, int numberOfItems)
		{
			var tasks = new List<Task>();

			for (int i = 0; i < numberOfItems; i++)
			{
				var key = $"{prefix}{i}";
				var tsk1 = sut.Database.StringSetAsync(key, $"key: {key} - value: {i}");
				
				tasks.Add(tsk1);
			}

			await Task.WhenAll(tasks);
		}

		[Fact(Skip = "Is a performance test, must be run manually")]
		public async Task StressTest_On_SearchKeysMethod()
																																																										{
			//Note: https://github.com/imperugo/StackExchange.Redis.Extensions/issues/115
			var filter = "prefix_";
			var totalItemsToAdd = 10000;
			await PopulateDbAsync(filter, totalItemsToAdd);
			List<string> schemas = new List<string>();

			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

			#region With SCAN and MATCH
			sw.Start();
			int nextCursor = 0;
			do
			{
				RedisResult redisResult = sut.Database.Execute("SCAN", nextCursor.ToString(), "MATCH", $"{filter}*", "COUNT", "5000");
				var innerResult = (RedisResult[])redisResult;
				nextCursor = int.Parse((string)innerResult[0]);
				List<string> resultLines = ((string[])innerResult[1]).ToList();
				schemas.AddRange(resultLines);
			}
			while (nextCursor != 0);
			output.WriteLine($"SCAN and MATCH: Got {schemas.Count} elements of {filter} in {sw.ElapsedMilliseconds}msec.", schemas.Count, sw.ElapsedMilliseconds);
			#endregion With SCAN and MATCH
			#region With method SearchKeys
			sw.Restart();
			IEnumerable<string> schemas1 = sut.SearchKeys($"{filter}*");
			output.WriteLine($"SearchKeys: Got {schemas1.Count()} elements of {filter} in {sw.ElapsedMilliseconds}msec.");
			#endregion With method SearchKeys

			Assert.Equal(schemas.Count, totalItemsToAdd);
			Assert.Equal(schemas1.Count(), totalItemsToAdd);
		}

        //[Fact(Skip = "Is a performance test, must be run manually")]
        [Fact]
        public async Task Open_Tons_of_concurrent_connections()
        {
            var sut = new RedisCacheConnectionPoolManager(new RedisConfiguration()
			{
				AbortOnConnectFail = false,
				Hosts = new RedisHost[]
				{
					new RedisHost() { Host = "localhost", Port = 6379 }
				},
				AllowAdmin = true,
				ConnectTimeout = 5000,
				Database = 8,
                PoolSize = 10,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
				{
					Mode = ServerEnumerationStrategy.ModeOptions.All,
					TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
					UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
				}
			});

            await sut.GetConnection().GetDatabase().StringSetAsync("key","value");

            var conn = sut.GetConnection();

            Parallel.For(0,1000, x => 
            {
                try
                {
                    sut.GetConnection().GetDatabase().StringGet("key"); 
                } 
                catch (Exception exc)
                { 
                    output.WriteLine($"Index: {x} - Exception {exc.ToString()}");
                    throw;
                }
            });
        }
	}
}
