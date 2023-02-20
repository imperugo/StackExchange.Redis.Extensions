// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Net.Security;
using System.Security.Authentication;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.Models;
using StackExchange.Redis.Profiling;

namespace StackExchange.Redis.Extensions.Core.Configuration;

/// <summary>
/// The redis configuration
/// </summary>
public class RedisConfiguration
{
    private ConfigurationOptions? options;
    private string keyPrefix = string.Empty;
    private string? password;
    private bool allowAdmin;
    private bool ssl;
    private int connectTimeout = 5000;
    private int syncTimeout = 1000;
    private bool abortOnConnectFail;
    private int database;
    private RedisHost[] hosts = Array.Empty<RedisHost>();
    private ServerEnumerationStrategy serverEnumerationStrategy = new();
    private uint maxValueLength;
    private int poolSize = 5;
    private string[]? excludeCommands;
    private string? configurationChannel;
    private string? connectionString;
    private string? serviceName;
    private SslProtocols? sslProtocols;
    private Func<ProfilingSession>? profilingSessionProvider;
    private int workCount = Environment.ProcessorCount * 2;
    private ConnectionSelectionStrategy connectionSelectionStrategy = ConnectionSelectionStrategy.LeastLoaded;

    /// <summary>
    /// A RemoteCertificateValidationCallback delegate responsible for validating the certificate supplied by the remote party; note
    /// that this cannot be specified in the configuration-string.
    /// </summary>
    public event RemoteCertificateValidationCallback? CertificateValidation;

    /// <summary>
    /// Indicate if the current configuration is the default;
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// The identifier name for the connection
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the every ConnectionSelectionStrategy to use during connection selection.
    /// </summary>
    public ConnectionSelectionStrategy ConnectionSelectionStrategy
    {
        get => connectionSelectionStrategy;

        set
        {
            connectionSelectionStrategy = value;
            ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the every ConnectionMultiplexer SocketManager WorkCount
    /// </summary>
    public int WorkCount
    {
        get => workCount;

        set
        {
            workCount = value;
            ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the servicename used in case of Sentinel.
    /// </summary>
    public string? ServiceName
    {
        get => serviceName;

        set
        {
            serviceName = value;
            ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets a value indicating whether get a boolean value that indicates if the cluster is configured for sentinel or not
    /// </summary>
    public bool IsSentinelCluster => !string.IsNullOrEmpty(ServiceName);

    /// <summary>
    /// Gets or sets the connection string. In wins over property configuration.
    /// </summary>
    public SslProtocols? SslProtocols
    {
        get => sslProtocols;

        set
        {
            sslProtocols = value;
            ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the connection string. In wins over property configuration.
    /// </summary>
    public string? ConnectionString
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
    public string? ConfigurationChannel
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
    public string? Password
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
    public string[]? ExcludeCommands
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
    public Func<ProfilingSession>? ProfilingSessionProvider
    {
        get => profilingSessionProvider;

        set
        {
            profilingSessionProvider = value;
            ResetConfigurationOptions();
        }
    }

    /// <summary>
    /// Gets or sets the factory for <see cref="IStateAwareConnection"/> creation
    /// </summary>
    /// <returns>>If property is not set, default <see cref="IStateAwareConnection"/> will be resolved</returns>
    /// <remarks>
    ///     Proprerty is optional.
    ///     Property should be assined by invocation code only once. (We are not doing additional checks in the property itself in order to prevent any possible issues during serialization)
    /// </remarks>
    public StateAwareConnectionResolver StateAwareConnectionFactory { get; set; } = (cm, logger) => new RedisConnectionPoolManager.StateAwareConnection(cm, logger);

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
                ConfigurationOptions newOptions;

                if (ConnectionString?.Length > 0)
                {
                    newOptions = ConfigurationOptions.Parse(ConnectionString);
                }
                else
                {
                    newOptions = new()
                    {
                        Ssl = Ssl,
                        AllowAdmin = AllowAdmin,
                        Password = Password,
                        ConnectTimeout = ConnectTimeout,
                        SyncTimeout = SyncTimeout,
                        AbortOnConnectFail = AbortOnConnectFail,
                        ConfigurationChannel = ConfigurationChannel!,
                        SslProtocols = sslProtocols,
                        ChannelPrefix = KeyPrefix
                    };

                    if (IsSentinelCluster)
                    {
                        newOptions.ServiceName = ServiceName;
                        newOptions.CommandMap = CommandMap.Sentinel;
                    }

                    foreach (var redisHost in Hosts)
                        newOptions.EndPoints.Add(redisHost.Host, redisHost.Port);
                }

                if (ExcludeCommands != null)
                {
                    newOptions.CommandMap = CommandMap.Create(
                        new(ExcludeCommands),
                        false);
                }

                if (WorkCount > 0)
                    newOptions.SocketManager = new(GetType().Name, WorkCount);

                newOptions.CertificateValidation += CertificateValidation;
                options = newOptions;
            }

            return options;
        }
    }

    private void ResetConfigurationOptions()
    {
        // this is needed in order to cover this scenario
        // https://github.com/imperugo/StackExchange.Redis.Extensions/issues/165
        options = null;
    }
}
