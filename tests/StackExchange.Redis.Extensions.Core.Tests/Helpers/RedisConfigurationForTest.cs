// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.Tests.Helpers
{
    internal static class RedisConfigurationForTest
    {
        private const string DEFAULT_HOST = "localhost"; // Or use LAN IP: 192.168.*.*

        public static RedisConfiguration CreateBasicConfig(string targetHost = default) => new()
        {
            AbortOnConnectFail = false,
            KeyPrefix = "MyPrefix__",
            Hosts = [
                new()
                {
                    Host = string.IsNullOrEmpty(targetHost) ? DEFAULT_HOST : targetHost,
                    Port = 6379
                }
            ],
            // Password = "hello_world",
            AllowAdmin = true,
            ConnectTimeout = 3000,
            Database = 0, // DO NOT change.
            PoolSize = 5,
            ServerEnumerationStrategy = new()
            {
                Mode = ServerEnumerationStrategy.ModeOptions.All,
                TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
            }
        };
    }
}
