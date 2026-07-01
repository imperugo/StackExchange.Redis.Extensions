// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using NSubstitute;

using StackExchange.Redis.Extensions.AspNetCore.HealthChecks;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

[Collection("Redis")]
public class RedisHealthCheckTests
{
    [Fact]
    public async Task Should_Return_Healthy_When_All_Connections_Active_Async()
    {
        var poolManager = Substitute.For<IRedisConnectionPoolManager>();
        poolManager.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 5,
            InvalidConnections = 0,
            RequiredPoolSize = 5,
        });

        var database = Substitute.For<IRedisDatabase>();
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        var redisDb = Substitute.For<IDatabase>();
        redisDb.PingAsync(Arg.Any<CommandFlags>()).Returns(global::System.TimeSpan.FromMilliseconds(1));
        database.Database.Returns(redisDb);

        var client = Substitute.For<IRedisClient>();
        client.Name.Returns("test");
        client.ConnectionPoolManager.Returns(poolManager);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client]);
        factory.GetDefaultRedisDatabase().Returns(database);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Should_Return_Degraded_When_Some_Connections_Invalid_Async()
    {
        var poolManager = Substitute.For<IRedisConnectionPoolManager>();
        poolManager.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 3,
            InvalidConnections = 2,
            RequiredPoolSize = 5,
        });

        var database = Substitute.For<IRedisDatabase>();
        var redisDb = Substitute.For<IDatabase>();
        redisDb.PingAsync(Arg.Any<CommandFlags>()).Returns(global::System.TimeSpan.FromMilliseconds(1));
        database.Database.Returns(redisDb);

        var client = Substitute.For<IRedisClient>();
        client.Name.Returns("test");
        client.ConnectionPoolManager.Returns(poolManager);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client]);
        factory.GetDefaultRedisDatabase().Returns(database);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Degraded, result.Status);
    }

    [Fact]
    public async Task Should_Return_Unhealthy_When_All_Connections_Invalid_Async()
    {
        var poolManager = Substitute.For<IRedisConnectionPoolManager>();
        poolManager.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 0,
            InvalidConnections = 5,
            RequiredPoolSize = 5,
        });

        var client = Substitute.For<IRedisClient>();
        client.Name.Returns("test");
        client.ConnectionPoolManager.Returns(poolManager);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client]);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
