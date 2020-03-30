﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.LegacyConfiguration.Configuration;
using RedisHost = StackExchange.Redis.Extensions.LegacyConfiguration.Configuration.RedisHost;

namespace StackExchange.Redis.Extensions.LegacyConfiguration
{
    /// <summary>
    /// The implementation of <see cref="IRedisCachingConfiguration"/>
    /// </summary>
    public class RedisCachingSectionHandler : ConfigurationSection, IRedisCachingConfiguration
    {
        /// <summary>
        /// The host of Redis Server
        /// </summary>
        /// <value>
        /// The ip or name
        /// </value>
        [ConfigurationProperty("hosts")]
        public RedisHostCollection RedisHosts
            => this["hosts"] as RedisHostCollection;

        /// <summary>
        /// The strategy to use when executing server wide commands
        /// </summary>
        [ConfigurationProperty("serverEnumerationStrategy")]
        public ServerEnumerationStrategyConfiguration ServerEnumerationStrategy
            => this["serverEnumerationStrategy"] as ServerEnumerationStrategyConfiguration;


        /// <summary>
        /// Specify if the connection can use Admin commands like flush database
        /// </summary>
        /// <value>
        ///   <c>true</c> if can use admin commands; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("allowAdmin")]
        public bool AllowAdmin { get => (bool)base["allowAdmin"]; }

        /// <summary>
        /// Specify if the connection is a secure connection or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is secure; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("ssl")]
        public bool Ssl { get => (bool)base["ssl"]; }

        /// <summary>
        /// The connection timeout
        /// </summary>
        [ConfigurationProperty("connectTimeout", DefaultValue = 5000)]
        public int ConnectTimeout { get=> (int)base["connectTimeout"]; }

        /// <summary>
        /// Time (ms) to allow for synchronous operations
        /// </summary>
        [ConfigurationProperty("syncTimeout", DefaultValue = 1000)]
        public int SyncTimeout { get => (int)base["syncTimeout"]; }

        /// <summary>
        /// If true, Connect will not create a connection while no servers are available
        /// </summary>
        [ConfigurationProperty("abortOnConnectFail")]
        public bool AbortOnConnectFail { get => (bool)base["abortOnConnectFail"]; }

        /// <summary>
        /// Database Id
        /// </summary>
        /// <value>
        /// The database id, the default value is 0
        /// </value>
        [ConfigurationProperty("database", DefaultValue = 0)]
        public int Database { get => (int)base["database"]; }

        /// <summary>
        /// The password or access key
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password => this["password"] as string;

        /// <summary>
        /// The key separation prefix used for all cache entries
        /// </summary>
        [ConfigurationProperty("keyprefix", IsRequired = false)]
        public string KeyPrefix => this["keyprefix"] as string;

        /// <summary>
        /// Redis connections pool size
        /// </summary>
        [ConfigurationProperty("poolSize", DefaultValue = 5)]
        public int PoolSize { get => (int)base["poolSize"]; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public static RedisConfiguration GetConfig()
        {
            var cfg = ConfigurationManager.GetSection("redisCacheClient") as RedisCachingSectionHandler;

            if (cfg == null)
            {
                throw new ConfigurationErrorsException("Unable to locate <redisCacheClient> section into your configuration file. Take a look https://github.com/imperugo/StackExchange.Redis.Extensions");
            }

            RedisConfiguration result = new RedisConfiguration();
            result.AbortOnConnectFail = cfg.AbortOnConnectFail;
            result.AllowAdmin = cfg.AllowAdmin;
            result.ConnectTimeout = cfg.ConnectTimeout;
            result.SyncTimeout = cfg.SyncTimeout;
            result.Database = cfg.Database;
            result.KeyPrefix = cfg.KeyPrefix;
            result.Password = cfg.Password;
            result.Ssl = cfg.Ssl;
            result.PoolSize = cfg.PoolSize;

            List<Core.Configuration.RedisHost> hosts = new List<Core.Configuration.RedisHost>();

            foreach (RedisHost host in cfg.RedisHosts)
            {
                hosts.Add(new Core.Configuration.RedisHost()
                {
                    Host = host.Host,
                    Port = host.CachePort
                });
            }

            result.Hosts = hosts.ToArray();
            result.ServerEnumerationStrategy = new ServerEnumerationStrategy()
            {
                UnreachableServerAction = cfg.ServerEnumerationStrategy.UnreachableServerAction,
                TargetRole = cfg.ServerEnumerationStrategy.TargetRole,
                Mode = cfg.ServerEnumerationStrategy.Mode
            };

            return result;
        }
    }
}