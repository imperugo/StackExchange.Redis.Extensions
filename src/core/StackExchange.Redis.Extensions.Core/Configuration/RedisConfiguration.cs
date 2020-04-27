using System;
using System.Collections.Generic;
using System.Net.Security;
using StackExchange.Redis.Profiling;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
    /// <summary>
    /// The redis configuration
    /// </summary>
    public class RedisConfiguration
    {
        private ConnectionMultiplexer connection;
        private ConfigurationOptions options;
        private string keyPrefix;
        private string password;
        private bool allowAdmin;
        private bool ssl;
        private int connectTimeout = 5000;
        private int syncTimeout = 1000;
        private bool abortOnConnectFail;
        private int database = 0;
        private RedisHost[] hosts;
        private ServerEnumerationStrategy serverEnumerationStrategy;
        private uint maxValueLength;
        private int poolSize = 5;
        private string[] excludeCommands;
        private string configurationChannel = null;
        private string connectionString = null;
        private Func<ProfilingSession> profilingSessionProvider;

        /// <summary>
        /// A RemoteCertificateValidationCallback delegate responsible for validating the certificate supplied by the remote party; note
        /// that this cannot be specified in the configuration-string.
        /// </summary>
        public event RemoteCertificateValidationCallback CertificateValidation;

        /// <summary>
        /// Gets or sets the connection string. In wins over property configuration.
        /// </summary>
        public string ConnectionString
        {
            get => connectionString;
            set
            {
                connectionString = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the channel to use for broadcasting and listening for configuration change notification.
        /// </summary>
        public string ConfigurationChannel
        {
            get => configurationChannel;
            set
            {
                configurationChannel = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the key separation prefix used for all cache entries.
        /// </summary>
        public string KeyPrefix
        {
            get => keyPrefix;
            set
            {
                keyPrefix = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the redis password.
        /// </summary>
        public string Password
        {
            get => password;
            set
            {
                password = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether admin operations should be allowed.
        /// </summary>
        public bool AllowAdmin
        {
            get => allowAdmin;
            set
            {
                allowAdmin = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether specify if whether the connection should be encrypted.
        /// </summary>
        public bool Ssl
        {
            get => ssl;
            set
            {
                ssl = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the time in milliseconds that should be allowed for connection (defaults to 5 seconds unless SyncTimeout is higher).
        /// </summary>
        public int ConnectTimeout
        {
            get => connectTimeout;
            set
            {
                connectTimeout = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the time in milliseconds that the system should allow for synchronous operations (defaults to 5 seconds).
        /// </summary>
        public int SyncTimeout
        {
            get => syncTimeout;
            set
            {
                syncTimeout = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether connect/configuration timeouts should be explicitly notified via a TimeoutException.
        /// </summary>
        public bool AbortOnConnectFail
        {
            get => abortOnConnectFail;
            set
            {
                abortOnConnectFail = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets database Id.
        /// </summary>
        public int Database
        {
            get => database;
            set
            {
                database = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the host of Redis Servers (The ips or names).
        /// </summary>
        public RedisHost[] Hosts
        {
            get => hosts;
            set
            {
                hosts = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets the strategy to use when executing server wide commands.
        /// </summary>
        public ServerEnumerationStrategy ServerEnumerationStrategy
        {
            get => serverEnumerationStrategy;
            set
            {
                serverEnumerationStrategy = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets maximal value length which can be set in database.
        /// </summary>
        public uint MaxValueLength
        {
            get => maxValueLength;
            set
            {
                maxValueLength = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets redis connections pool size.
        /// </summary>
        public int PoolSize
        {
            get => poolSize;
            set
            {
                poolSize = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets exclude commands.
        /// </summary>
        public string[] ExcludeCommands
        {
            get => excludeCommands;
            set
            {
                excludeCommands = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets or sets redis Profiler to attach to ConnectionMultiplexer.
        /// </summary>
        public Func<ProfilingSession> ProfilingSessionProvider
        {
            get => profilingSessionProvider;
            set
            {
                profilingSessionProvider = value;
                ResetConfigurationOptions();
            }
        }

        /// <summary>
        /// Gets the Redis configuration options
        /// </summary>
        /// <value>An instanfe of <see cref="ConfigurationOptions" />.</value>
        public ConfigurationOptions ConfigurationOptions
        {
            get
            {
                if (options == null)
                {
                    if (!string.IsNullOrEmpty(ConnectionString))
                    {
                        options = ConfigurationOptions.Parse(ConnectionString);
                    }
                    else
                    {
                        options = new ConfigurationOptions
                        {
                            Ssl = Ssl,
                            AllowAdmin = AllowAdmin,
                            Password = Password,
                            ConnectTimeout = ConnectTimeout,
                            SyncTimeout = SyncTimeout,
                            AbortOnConnectFail = AbortOnConnectFail,
                            ConfigurationChannel = ConfigurationChannel,
                            ChannelPrefix = KeyPrefix
                        };

                        foreach (var redisHost in Hosts)
                            options.EndPoints.Add(redisHost.Host, redisHost.Port);
                    }

                    if (ExcludeCommands != null)
                    {
                        options.CommandMap = CommandMap.Create(
                            new HashSet<string>(ExcludeCommands),
                            available: false);
                    }

                    options.CertificateValidation += CertificateValidation;
                }

                return options;
            }
        }

        /// <summary>
        /// Gets the connection multiplex
        /// </summary>
        /// <value>An instanfe of <see cref="ConnectionMultiplexer" />.</value>
        public ConnectionMultiplexer Connection
        {
            get
            {
                if (connection == null)
                    connection = ConnectionMultiplexer.Connect(ConfigurationOptions);

                return connection;
            }
        }

        private void ResetConfigurationOptions()
        {
            // this is needed in order to cover this scenario
            // https://github.com/imperugo/StackExchange.Redis.Extensions/issues/165
            options = null;
        }
    }
}
