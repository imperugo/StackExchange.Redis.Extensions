// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A set of extension methods that help you to confire StackExchangeRedisExtensions into your dependency injection
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Add StackExchange.Redis with its serialization provider.
    /// When the configuration has a non-empty <see cref="RedisConfiguration.Name"/>, keyed services for
    /// <see cref="IRedisClient"/> and <see cref="IRedisDatabase"/> are registered automatically.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfiguration">The redis configration.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        RedisConfiguration redisConfiguration)
        where T : class, ISerializer
    {
        return services.AddStackExchangeRedisExtensions<T>([redisConfiguration]);
    }

    /// <summary>
    /// Add StackExchange.Redis with its serialization provider.
    /// When configurations have a non-empty <see cref="RedisConfiguration.Name"/>, keyed services for
    /// <see cref="IRedisClient"/> and <see cref="IRedisDatabase"/> are registered automatically.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfigurations">The redis configrations.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        IEnumerable<RedisConfiguration> redisConfigurations)
        where T : class, ISerializer
    {
        services.AddStackExchangeRedisExtensions<T>(sp => redisConfigurations);

        foreach (var config in redisConfigurations.Where(c => !string.IsNullOrEmpty(c.Name)))
        {
            var name = config.Name;

            services.AddKeyedSingleton<IRedisClient>(name,
                (sp, _) => sp.GetRequiredService<IRedisClientFactory>().GetRedisClient(name));

            services.AddKeyedSingleton<IRedisDatabase>(name,
                (sp, _) => sp.GetRequiredService<IRedisClientFactory>().GetRedisDatabase(name));
        }

        return services;
    }

    /// <summary>
    /// Add StackExchange.Redis with its serialization provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfigurationFactory">The redis configration factory.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    /// <remarks>
    /// Keyed DI services are not registered when using this overload because configuration names are
    /// not available at registration time. Use the overloads accepting <see cref="RedisConfiguration"/>
    /// or <see cref="IEnumerable{RedisConfiguration}"/> to get keyed service support.
    /// </remarks>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        Func<IServiceProvider, IEnumerable<RedisConfiguration>> redisConfigurationFactory)
        where T : class, ISerializer
    {
        services.AddSingleton<IRedisClientFactory, RedisClientFactory>();
        services.AddSingleton<ISerializer, T>();

        services.AddSingleton((provider) => provider
            .GetRequiredService<IRedisClientFactory>()
            .GetDefaultRedisClient());

        services.AddSingleton((provider) => provider
            .GetRequiredService<IRedisClientFactory>()
            .GetDefaultRedisClient()
            .GetDefaultDatabase());

        services.AddSingleton(redisConfigurationFactory);

        return services;
    }
}
