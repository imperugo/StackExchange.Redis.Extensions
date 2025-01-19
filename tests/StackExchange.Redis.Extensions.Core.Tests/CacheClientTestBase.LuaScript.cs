// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.Tests.Helpers;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Tests.Extensions;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public class CacheClientTestBase_WithoutKeyPrefixForLuaScript : IDisposable
{
#pragma warning disable CA1859
    private readonly IDatabase db;
    private readonly ISerializer serializer;
    private readonly IRedisConnectionPoolManager connectionPoolManager;
#pragma warning restore CA1859
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public CacheClientTestBase_WithoutKeyPrefixForLuaScript()
    {
        var redisConfiguration = RedisConfigurationForTest.CreateBasicConfig();
        redisConfiguration.KeyPrefix = string.Empty;

        var moqLogger = new Mock<ILogger<RedisConnectionPoolManager>>();

        serializer = new NewtonsoftSerializer();
        connectionPoolManager = new RedisConnectionPoolManager(redisConfiguration, moqLogger.Object);
        Sut = new RedisClient(connectionPoolManager, serializer, redisConfiguration);
        db = Sut.GetDefaultDatabase().Database;
    }

    protected IRedisClient Sut { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            // free managed resources
            db.FlushDatabase();
            db.Multiplexer.GetSubscriber().UnsubscribeAll();
            connectionPoolManager.Dispose();
        }

        // free native resources if there are any.
        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }

    [Fact]
    public async Task HashGetAllAsyncAtOneTimeAsync_ValueExists_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey1 = Guid.NewGuid().ToString();
        var entryKey2 = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().HashSetAsync(hashKey, entryKey1, "testvalue1");
        await Sut.GetDefaultDatabase().HashSetAsync(hashKey, entryKey2, "testvalue2");

        // act
        Assert.True(await db.HashExistsAsync(hashKey, entryKey1));
        Assert.True(await db.HashExistsAsync(hashKey, entryKey2));
        var result = await Sut.GetDefaultDatabase().HashGetAllAsyncAtOneTimeAsync<string>(hashKey, [entryKey1, entryKey2]);

        // assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
}
