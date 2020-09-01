using System;
using System.Net;

using Microsoft.AspNetCore.Http;

namespace StackExchange.Redis.Extensions.AspNetCore.Midllewares
{
    /// <summary>
    /// All the options needed to allow the client to redis information middleware
    /// </summary>
    public class RedisMiddlewareAccessOptions
    {
        /// <summary>
        /// Gets or sets the function that allows you to customize who can access to redis information.
        /// </summary>
        /// <value>The funcion.</value>
        public Func<HttpContext, bool> AllowFunction { get; set; }

        /// <summary>
        /// Gets or sets the allowed IPs to show the redis servers information
        /// </summary>
        /// <value>An an array with the allowed ips</value>
        public IPAddress[] AllowedIPs { get; set; }
    }
}
