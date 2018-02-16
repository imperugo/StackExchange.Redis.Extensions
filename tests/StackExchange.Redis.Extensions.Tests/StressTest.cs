using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Tests.Extensions;
using StackExchange.Redis.Extensions.Tests.Helpers;
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
			    EndPoints = {{ "localhost" , 6379 }},
			    AllowAdmin = true
		    });

		    sut = new StackExchangeRedisCacheClient(mux, new NewtonsoftSerializer());
	    }

	    public void Dispose()
	    {
		    sut.Database.FlushDatabase();
		    mux.GetSubscriber().UnsubscribeAll();
		    sut.Database.Multiplexer.Dispose();
		    sut.Dispose();
	    }

	    private void PopulateDb(string prefix, int numberOfItems)
	    {
		    for (int i = 0; i < numberOfItems; i++)
		    {
			    var key = $"{prefix}{i}";
			    var fakeKey = $"mypersonakey_{i}";

			    sut.Database.StringSet(key, $"key: {key} - value: {i}");
			    sut.Database.StringSet(fakeKey, $"key: {fakeKey} - value: {i}");
		    }
	    }

	    [Fact(Skip = "Is a performance test, must be run manually")]
	    public void StressTest_On_SearchKeysMethod()
	    {
			//Note: https://github.com/imperugo/StackExchange.Redis.Extensions/issues/115

		    var filter = "prefix_";
		    var totalItemsToAdd = 1000;
		    PopulateDb(filter, totalItemsToAdd);
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
    }
}
