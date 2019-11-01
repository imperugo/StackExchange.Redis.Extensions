using System;
using System.Collections.Generic;
using System.Net.Security;
using StackExchange.Redis.Profiling;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
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
        private int poolSize = 5;
	    private string[] excludeCommands;
        private string configurationChannel = null;
        private Func<ProfilingSession> profilingSessionProvider;

        /// <summary>
        /// The key separation prefix used for all cache entries
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
        /// The key separation prefix used for all cache entries
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
        /// The password or access key
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
        /// Specify if the connection can use Admin commands like flush database
        /// </summary>
        /// <value>
        ///   <c>true</c> if can use admin commands; otherwise, <c>false</c>.
        /// </value>
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
        /// Specify if the connection is a secure connection or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is secure; otherwise, <c>false</c>.
        /// </value>
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
        /// The connection timeout
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
        /// Time (ms) to allow for synchronous operations
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
        /// If true, Connect will not create a connection while no servers are available
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
        /// Database Id
        /// </summary>
        /// <value>
        /// The database id, the default value is 0
        /// </value>
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
        /// The host of Redis Servers
        /// </summary>
        /// <value>
        /// The ips or names
        /// </value>
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
        /// The strategy to use when executing server wide commands
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
        /// Redis connections pool size
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
        /// Exclude commands
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
        /// Redis Profiler to attach to ConnectionMultiplexer
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
        /// A RemoteCertificateValidationCallback delegate responsible for validating the certificate supplied by the remote party; note
        /// that this cannot be specified in the configuration-string.
        /// </summary>
        public event RemoteCertificateValidationCallback CertificateValidation;

		public ConfigurationOptions ConfigurationOptions
		{
			get
			{
				if (options == null)
				{
					options = new ConfigurationOptions
					{
						Ssl = Ssl,
						AllowAdmin = AllowAdmin,
						Password = Password,
						ConnectTimeout = ConnectTimeout,
						SyncTimeout = SyncTimeout,
						AbortOnConnectFail = AbortOnConnectFail,
                        ConfigurationChannel = ConfigurationChannel
					};
					
					if (ExcludeCommands != null)
					{
						options.CommandMap = CommandMap.Create(
							new HashSet<string>(ExcludeCommands),
							available: false
						);
					}

					foreach (var redisHost in Hosts)
						options.EndPoints.Add(redisHost.Host, redisHost.Port);

					options.CertificateValidation += CertificateValidation;
				}

				return options;
			}
		}

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
