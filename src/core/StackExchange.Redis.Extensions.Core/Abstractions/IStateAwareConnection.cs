using System;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// Interface in order to wrap <see cref="IConnectionMultiplexer"/> for pooling
/// </summary>
public interface IStateAwareConnection : IDisposable
{
    /// <summary>
    /// Gets wrapped <see cref="IConnectionMultiplexer"/>
    /// </summary>
    IConnectionMultiplexer Connection { get; }

    /// <summary>
    /// Indicates whether connection is established
    /// </summary>
    bool IsConnected();

    /// <summary>
    /// Indicates the total number of outstanding items against this server
    /// </summary>
    long TotalOutstanding();
}
