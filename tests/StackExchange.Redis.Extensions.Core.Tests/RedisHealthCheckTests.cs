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

    [Fact]
    public async Task Should_Return_Unhealthy_When_Ping_Throws_RedisException_Async()
    {
        var poolManager = Substitute.For<IRedisConnectionPoolManager>();
        poolManager.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 5,
            InvalidConnections = 0,
            RequiredPoolSize = 5,
        });

        var database = Substitute.For<IRedisDatabase>();
        var redisDb = Substitute.For<IDatabase>();
        redisDb.PingAsync(Arg.Any<CommandFlags>()).Returns<global::System.TimeSpan>(_ => throw new RedisException("test error"));
        database.Database.Returns(redisDb);

        var client = Substitute.For<IRedisClient>();
        client.Name.Returns("test");
        client.ConnectionPoolManager.Returns(poolManager);
        client.GetDefaultDatabase().Returns(database);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client]);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("PING failed", result.Description, global::System.StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_Return_Unhealthy_When_Ping_Throws_RedisTimeoutException_Async()
    {
        var poolManager = Substitute.For<IRedisConnectionPoolManager>();
        poolManager.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 5,
            InvalidConnections = 0,
            RequiredPoolSize = 5,
        });

        var database = Substitute.For<IRedisDatabase>();
        var redisDb = Substitute.For<IDatabase>();
        redisDb.PingAsync(Arg.Any<CommandFlags>()).Returns<global::System.TimeSpan>(_ => throw new RedisTimeoutException("timeout", CommandStatus.Unknown));
        database.Database.Returns(redisDb);

        var client = Substitute.For<IRedisClient>();
        client.Name.Returns("test");
        client.ConnectionPoolManager.Returns(poolManager);
        client.GetDefaultDatabase().Returns(database);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client]);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("timed out", result.Description, global::System.StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_Return_Unhealthy_When_Ping_Throws_RedisConnectionException_Async()
    {
        var poolManager = Substitute.For<IRedisConnectionPoolManager>();
        poolManager.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 5,
            InvalidConnections = 0,
            RequiredPoolSize = 5,
        });

        var database = Substitute.For<IRedisDatabase>();
        var redisDb = Substitute.For<IDatabase>();
        redisDb.PingAsync(Arg.Any<CommandFlags>()).Returns<global::System.TimeSpan>(_ => throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "connection lost"));
        database.Database.Returns(redisDb);

        var client = Substitute.For<IRedisClient>();
        client.Name.Returns("test");
        client.ConnectionPoolManager.Returns(poolManager);
        client.GetDefaultDatabase().Returns(database);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client]);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("connection error", result.Description, global::System.StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_Return_Unhealthy_When_No_Clients_Configured_Async()
    {
        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns(new List<IRedisClient>());

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("No Redis clients", result.Description, global::System.StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_Return_Degraded_With_Multiple_Clients_When_One_Has_Invalid_Connections_Async()
    {
        var healthyPool = Substitute.For<IRedisConnectionPoolManager>();
        healthyPool.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 5,
            InvalidConnections = 0,
            RequiredPoolSize = 5,
        });

        var degradedPool = Substitute.For<IRedisConnectionPoolManager>();
        degradedPool.GetConnectionInformation().Returns(new ConnectionPoolInformation
        {
            ActiveConnections = 3,
            InvalidConnections = 2,
            RequiredPoolSize = 5,
        });

        var db1 = Substitute.For<IRedisDatabase>();
        var redisDb1 = Substitute.For<IDatabase>();
        redisDb1.PingAsync(Arg.Any<CommandFlags>()).Returns(global::System.TimeSpan.FromMilliseconds(1));
        db1.Database.Returns(redisDb1);

        var db2 = Substitute.For<IRedisDatabase>();
        var redisDb2 = Substitute.For<IDatabase>();
        redisDb2.PingAsync(Arg.Any<CommandFlags>()).Returns(global::System.TimeSpan.FromMilliseconds(2));
        db2.Database.Returns(redisDb2);

        var client1 = Substitute.For<IRedisClient>();
        client1.Name.Returns("cache");
        client1.ConnectionPoolManager.Returns(healthyPool);
        client1.GetDefaultDatabase().Returns(db1);

        var client2 = Substitute.For<IRedisClient>();
        client2.Name.Returns("session");
        client2.ConnectionPoolManager.Returns(degradedPool);
        client2.GetDefaultDatabase().Returns(db2);

        var factory = Substitute.For<IRedisClientFactory>();
        factory.GetAllClients().Returns([client1, client2]);

        var healthCheck = new RedisHealthCheck(factory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.True(result.Data.ContainsKey("cache:ping_ms"));
        Assert.True(result.Data.ContainsKey("session:ping_ms"));
    }
}
