// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace StackExchange.Redis.Extensions.Core.Configuration;

/// <summary>
/// This class represent the redis host configuration section
/// </summary>
public class RedisHost
{
    /// <summary>
    /// Gets or sets the IP or host name of the redis server.
    /// </summary>
    /// <value>The IP or host name of the redis server.</value>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port of the redis server.
    /// </summary>
    /// <value>The port of the redis server.</value>
    public int Port { get; set; } = 6379;
}
