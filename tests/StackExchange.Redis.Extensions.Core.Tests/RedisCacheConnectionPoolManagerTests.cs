using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Tests.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Tests;

public class RedisCacheConnectionPoolManagerTests : IDisposable
{
    private bool isDisposed;
    private readonly RedisCacheConnectionPoolManager sut;
    private readonly ITestOutputHelper output;

    public RedisCacheConnectionPoolManagerTests(ITestOutputHelper output)
    {
        // See more info here: https://gist.github.com/JonCole/e65411214030f0d823cb#file-threadpool-md
        // Everything started from here https://gist.github.com/JonCole/925630df72be1351b21440625ff2671f#file-redis-bestpractices-stackexchange-redis-md
        ThreadPool.GetMaxThreads(out var maxThread, out var maxIoThread);
        ThreadPool.SetMinThreads(maxThread / 2, maxIoThread);

        this.output = output;

        var configuration = new RedisConfiguration
        {
            AbortOnConnectFail = true,
            KeyPrefix = "MyPrefix__",
            Hosts = new RedisHost[]
            {
                new() { Host = "localhost", Port = 6379 }
            },
            AllowAdmin = true,
            ConnectTimeout = 3000,
            Database = 0,
            PoolSize = 5,
            ServerEnumerationStrategy = new()
            {
                Mode = ServerEnumerationStrategy.ModeOptions.All,
                TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
            }
        };

        var logger = output.BuildLoggerFor<RedisCacheConnectionPoolManager>();

        sut = new(configuration, logger);
        this.output = output;
    }

    [Fact]
    public async Task Equeue_Parallels_Calls_Should_Use_All_The_Pool()
    {
        const string cacheKey = "my cache key";
        const int maxNumberOfIterations = 10000;

        var errors = new bool[maxNumberOfIterations];

        await sut
            .GetConnection()
            .GetDatabase(0)
            .StringSetAsync(cacheKey, "my cache value")
            .ConfigureAwait(false);

        Parallel.For(0, maxNumberOfIterations, (i, state) =>
        {
            try
            {
                sut
                    .GetConnection()
                    .GetDatabase(0)
                    .StringGetAsync(cacheKey);

                errors[i] = false;
            }
            catch (RedisTimeoutException exc)
            {
                errors[i] = true;

                output.WriteLine(exc.Message);
            }
        });

        Assert.Equal(0, errors.Count(x => x));
        Assert.Equal(maxNumberOfIterations, errors.Count(x => !x));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            // free managed resources
            sut.GetConnection().GetDatabase().FlushDatabase();
            sut.GetConnection().GetSubscriber().UnsubscribeAll();
            sut.Dispose();
        }

        isDisposed = true;
    }
}
