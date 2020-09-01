using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.AspNetCore.Midllewares
{
    internal class RedisInformationMiddleware
    {
        private readonly ILogger<RedisInformationMiddleware> logger;
        private readonly RequestDelegate next;
        private readonly IRedisCacheConnectionPoolManager connectionPoolManager;
        private readonly IRedisDatabase redisDatabase;
        private readonly RedisMiddlewareAccessOptions options;

        public RedisInformationMiddleware(
            RequestDelegate next,
            RedisMiddlewareAccessOptions options,
            ILogger<RedisInformationMiddleware> logger,
            IRedisCacheConnectionPoolManager connectionPoolManager,
            IRedisDatabase redisDatabase)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
            this.connectionPoolManager = connectionPoolManager;
            this.redisDatabase = redisDatabase;
        }

        public async Task Invoke(HttpContext context)
        {
            logger.LogTrace("{0} --> Handling request: {1}", nameof(RedisInformationMiddleware), context.Request.Path);

            if (context.Request.Method == "GET" && context.Request.Path == "/redis/connectionInfo")
            {
                if (!IsClientAllowed(context))
                {
                    await next.Invoke(context);
                    return;
                }

                var data = connectionPoolManager.GetConnectionInformations();

                await JsonSerializer.SerializeAsync(context.Response.Body, data);

                return;
            }

            if (context.Request.Method == "GET" && context.Request.Path == "/redis/info")
            {
                if (!IsClientAllowed(context))
                {
                    await next.Invoke(context);
                    return;
                }

                var data = await redisDatabase.GetInfoAsync();

                await JsonSerializer.SerializeAsync(context.Response.Body, data);

                return;
            }

            await next.Invoke(context);
        }

        private bool IsClientAllowed(HttpContext context)
        {
            if (options.AllowedIPs == null)
                return true;

            if (options.AllowedIPs.Any(x => x.ToString() == context.Connection.RemoteIpAddress.ToString()))
                return true;

            if (options.AllowFunction != null)
                return options.AllowFunction(context);

            return false;
        }
    }
}
