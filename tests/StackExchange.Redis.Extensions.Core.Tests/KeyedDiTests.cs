// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Tests.Helpers;
using StackExchange.Redis.Extensions.System.Text.Json;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

[Collection("Redis")]
public class KeyedDiTests
{
    [Fact]
    public void Should_Resolve_Keyed_RedisClient_By_Name()
    {
        var config = RedisConfigurationForTest.CreateBasicConfig();
        config.Name = "test-redis";
        config.IsDefault = true;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(config);

        using var sp = services.BuildServiceProvider();
        var client = sp.GetKeyedService<IRedisClient>("test-redis");

        Assert.NotNull(client);
    }

    [Fact]
    public void Should_Resolve_Keyed_RedisDatabase_By_Name()
    {
        var config = RedisConfigurationForTest.CreateBasicConfig();
        config.Name = "test-redis";
        config.IsDefault = true;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(config);

        using var sp = services.BuildServiceProvider();
        var database = sp.GetKeyedService<IRedisDatabase>("test-redis");

        Assert.NotNull(database);
    }

    [Fact]
    public void Should_Resolve_Multiple_Keyed_Clients()
    {
        var config1 = RedisConfigurationForTest.CreateBasicConfig();
        config1.Name = "cache";
        config1.IsDefault = true;

        var config2 = RedisConfigurationForTest.CreateBasicConfig();
        config2.Name = "session";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new[] { config1, config2 });

        using var sp = services.BuildServiceProvider();
        var cacheClient = sp.GetKeyedService<IRedisClient>("cache");
        var sessionClient = sp.GetKeyedService<IRedisClient>("session");

        Assert.NotNull(cacheClient);
        Assert.NotNull(sessionClient);
    }

    [Fact]
    public void Should_Not_Resolve_Keyed_Client_With_Unknown_Name()
    {
        var config = RedisConfigurationForTest.CreateBasicConfig();
        config.Name = "test-redis";
        config.IsDefault = true;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(config);

        using var sp = services.BuildServiceProvider();
        var client = sp.GetKeyedService<IRedisClient>("unknown");

        Assert.Null(client);
    }
}
