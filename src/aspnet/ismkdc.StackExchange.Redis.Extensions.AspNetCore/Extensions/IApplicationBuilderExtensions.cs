using System;

using Microsoft.AspNetCore.Builder;

using StackExchange.Redis.Extensions.AspNetCore.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// A set of extension methods for redis.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Enable Redis configuration middleware
        /// </summary>
        /// <param name="application">The application builder.</param>
        /// <param name="options">The redis information options.</param>
        /// <returns>An instance of IApplicationBuilder.</returns>
        public static IApplicationBuilder UseRedisInformation(this IApplicationBuilder application, Action<RedisMiddlewareAccessOptions> options = null)
        {
            var opt = new RedisMiddlewareAccessOptions();
            options?.Invoke(opt);

            application.UseMiddleware<RedisInformationMiddleware>(opt);
            return application;
        }
    }
}
