// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis.Extensions.AspNetCore.Caching;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.Tests.Helpers;
using StackExchange.Redis.Extensions.System.Text.Json;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

[Collection("Redis")]
public sealed class RedisDistributedCacheTests : IDisposable
{
    private readonly RedisConnectionPoolManager connectionPoolManager;
    private readonly RedisDistributedCache sut;
    private readonly IDatabase prefixedDb;
    private readonly IDatabase rawDb;

    public RedisDistributedCacheTests()
    {
        var config = RedisConfigurationForTest.CreateBasicConfig();
        connectionPoolManager = new RedisConnectionPoolManager(config);
        var serializer = new SystemTextJsonSerializer(new JsonSerializerOptions());
        var redisClient = new RedisClient(connectionPoolManager, serializer, config);
        var redisDatabase = redisClient.GetDefaultDatabase();
        prefixedDb = redisDatabase.Database;
        rawDb = connectionPoolManager.GetConnection().GetDatabase(config.Database);
        sut = new RedisDistributedCache(redisDatabase);
    }

    public void Dispose()
    {
        rawDb.Execute("FLUSHDB");
        connectionPoolManager.Dispose();
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_Should_Roundtrip_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("hello world");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions());
        var result = await sut.GetAsync(key);

        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_NonExistent_Key_Should_Return_Null_Async()
    {
        var key = Guid.NewGuid().ToString();
        var result = await sut.GetAsync(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_Should_Delete_Key_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions());
        await sut.RemoveAsync(key);

        var result = await sut.GetAsync(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_With_AbsoluteExpiration_Should_Set_TTL_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
        });

        var ttl = await prefixedDb.KeyTimeToLiveAsync(key);
        Assert.NotNull(ttl);
        Assert.True(ttl!.Value.TotalSeconds > 0 && ttl.Value.TotalSeconds <= 30);
    }

    [Fact]
    public async Task SetAsync_With_SlidingExpiration_Should_Set_TTL_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromSeconds(30),
        });

        var ttl = await prefixedDb.KeyTimeToLiveAsync(key);
        Assert.NotNull(ttl);
        Assert.True(ttl!.Value.TotalSeconds > 0 && ttl.Value.TotalSeconds <= 30);
    }

    [Fact]
    public async Task GetAsync_With_SlidingExpiration_Should_Refresh_TTL_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromSeconds(30),
        });

        await Task.Delay(2000);
        await sut.GetAsync(key);

        var ttl = await prefixedDb.KeyTimeToLiveAsync(key);
        Assert.NotNull(ttl);
        Assert.True(ttl!.Value.TotalSeconds > 28);
    }

    [Fact]
    public async Task RefreshAsync_NonExistent_Key_Should_Not_Throw_Async()
    {
        var key = Guid.NewGuid().ToString();
        await sut.RefreshAsync(key);
    }

    [Fact]
    public void Set_And_Get_Should_Roundtrip()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("sync test");

        sut.Set(key, value, new DistributedCacheEntryOptions());
        var result = sut.Get(key);

        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_With_Both_Absolute_And_Sliding_Should_Use_Shorter_TTL_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60),
            SlidingExpiration = TimeSpan.FromSeconds(10),
        });

        var ttl = await prefixedDb.KeyTimeToLiveAsync(key);
        Assert.NotNull(ttl);
        Assert.True(ttl!.Value.TotalSeconds <= 11, $"TTL should be ~10s (sliding wins), was {ttl.Value.TotalSeconds}s");
        Assert.True(ttl.Value.TotalSeconds > 0);
    }

    [Fact]
    public async Task SetAsync_With_Absolute_Shorter_Than_Sliding_Should_Use_Absolute_TTL_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5),
            SlidingExpiration = TimeSpan.FromSeconds(30),
        });

        var ttl = await prefixedDb.KeyTimeToLiveAsync(key);
        Assert.NotNull(ttl);
        Assert.True(ttl!.Value.TotalSeconds <= 6, $"TTL should be ~5s (absolute wins), was {ttl.Value.TotalSeconds}s");
        Assert.True(ttl.Value.TotalSeconds > 0);
    }

    [Fact]
    public async Task SetAsync_With_AbsoluteExpiration_DateTimeOffset_Should_Set_TTL_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        await sut.SetAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(30),
        });

        var ttl = await prefixedDb.KeyTimeToLiveAsync(key);
        Assert.NotNull(ttl);
        Assert.True(ttl!.Value.TotalSeconds > 0 && ttl.Value.TotalSeconds <= 30);
    }

    [Fact]
    public void Set_With_Past_AbsoluteExpiration_Should_Throw()
    {
        var key = Guid.NewGuid().ToString();
        var value = Encoding.UTF8.GetBytes("test");

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Set(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(-10),
        }));
    }
}
