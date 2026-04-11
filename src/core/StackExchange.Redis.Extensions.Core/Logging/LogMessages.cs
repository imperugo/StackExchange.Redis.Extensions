// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using Microsoft.Extensions.Logging;

namespace StackExchange.Redis.Extensions.Core.Logging;

/// <summary>
/// High-performance, source-generated log messages for StackExchange.Redis.Extensions.
/// Uses <see cref="LoggerMessageAttribute"/> to eliminate boxing and reduce allocations.
/// </summary>
internal static partial class LogMessages
{
    // ─── Connection Pool (1xxx) ────────────────────────────────────────

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Redis connection pool initialized: {PoolSize} connections to {Endpoint} in {ElapsedMs}ms")]
    public static partial void PoolInitialized(ILogger logger, int poolSize, string endpoint, long elapsedMs);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug,
        Message = "Using connection {ConnectionHash} with {Outstanding} outstanding commands")]
    public static partial void ConnectionSelected(ILogger logger, int connectionHash, long outstanding);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning,
        Message = "All Redis connections are disconnected. Using connection {ConnectionHash} in degraded mode")]
    public static partial void AllConnectionsDisconnected(ILogger logger, int connectionHash);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Debug,
        Message = "Disposing Redis connection pool ({PoolSize} connections)")]
    public static partial void PoolDisposing(ILogger logger, int poolSize);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Information,
        Message = "Using async ConfigurationOptions handler for pool initialization")]
    public static partial void UsingAsyncConfigHandler(ILogger logger);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Error,
        Message = "Redis pool initialization failed at connection {Index} of {PoolSize}. Cleaning up {CreatedConnections} previously created connections")]
    public static partial void PoolInitializationFailed(ILogger logger, int index, int poolSize, int createdConnections);

    // ─── Connection State (2xxx) ───────────────────────────────────────

    [LoggerMessage(EventId = 2001, Level = LogLevel.Error,
        Message = "Redis connection failed: {FailureType}")]
    public static partial void ConnectionFailed(ILogger logger, Exception? exception, string failureType);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "Redis connection restored to {Endpoint}")]
    public static partial void ConnectionRestored(ILogger logger, string endpoint);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Error,
        Message = "Redis internal error from {Origin}")]
    public static partial void InternalError(ILogger logger, Exception? exception, string origin);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Error,
        Message = "Redis server error: {ErrorMessage}")]
    public static partial void ServerError(ILogger logger, string errorMessage);

    // ─── Client Factory (3xxx) ─────────────────────────────────────────

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information,
        Message = "Redis client created: {ClientName} (database: {Database})")]
    public static partial void ClientCreated(ILogger logger, string clientName, int database);

    // ─── Pub/Sub (4xxx) ────────────────────────────────────────────────

    [LoggerMessage(EventId = 4001, Level = LogLevel.Error,
        Message = "Error processing subscription message on channel {Channel}")]
    public static partial void SubscriptionHandlerError(ILogger logger, Exception? exception, string channel);
}
