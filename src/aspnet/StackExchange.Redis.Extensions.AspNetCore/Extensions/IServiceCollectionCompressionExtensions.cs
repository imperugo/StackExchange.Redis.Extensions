// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using StackExchange.Redis.Extensions.Core;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to add compression support to the Redis serialization pipeline.
/// </summary>
public static class IServiceCollectionCompressionExtensions
{
    /// <summary>
    /// Adds compression to the Redis serialization pipeline using the specified <see cref="ICompressor"/> implementation.
    /// Wraps the registered <see cref="ISerializer"/> with a <see cref="CompressedSerializer"/>.
    /// Must be called after <c>AddStackExchangeRedisExtensions</c>.
    /// </summary>
    /// <typeparam name="TCompressor">The compressor implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddStackExchangeRedisExtensions&lt;SystemTextJsonSerializer&gt;(config);
    /// services.AddRedisCompression&lt;GZipCompressor&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddRedisCompression<TCompressor>(this IServiceCollection services)
        where TCompressor : class, ICompressor, new()
    {
        var compressor = new TCompressor();

        return services.AddRedisCompression(compressor);
    }

    /// <summary>
    /// Adds compression to the Redis serialization pipeline using the specified <see cref="ICompressor"/> instance.
    /// Wraps the registered <see cref="ISerializer"/> with a <see cref="CompressedSerializer"/>.
    /// Must be called after <c>AddStackExchangeRedisExtensions</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="compressor">The compressor instance to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisCompression(this IServiceCollection services, ICompressor compressor)
    {
        ArgumentNullException.ThrowIfNull(compressor);

        services.AddSingleton(compressor);

        // Find the existing ISerializer registration and wrap it
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var descriptor = services[i];

            if (descriptor.ServiceType != typeof(ISerializer))
                continue;

            var previousFactory = descriptor.ImplementationFactory;
            var previousInstance = descriptor.ImplementationInstance;
            var previousType = descriptor.ImplementationType;

            services[i] = ServiceDescriptor.Singleton<ISerializer>(sp =>
            {
                ISerializer inner;

                if (previousInstance != null)
                    inner = (ISerializer)previousInstance;
                else if (previousFactory != null)
                    inner = (ISerializer)previousFactory(sp);
                else
                    inner = (ISerializer)ActivatorUtilities.CreateInstance(sp, previousType!);

                return new CompressedSerializer(inner, compressor);
            });

            return services;
        }

        throw new InvalidOperationException(
            $"No registration for {nameof(ISerializer)} found. Call AddStackExchangeRedisExtensions before AddRedisCompression.");
    }
}
