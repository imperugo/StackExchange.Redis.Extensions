# Logging

StackExchange.Redis.Extensions uses [source-generated logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/source-generation) via `[LoggerMessage]` attributes for zero-allocation, high-performance log output. All logs are emitted through `Microsoft.Extensions.Logging.ILogger`.

## Setup

Logging is automatic when using ASP.NET Core DI — just ensure your `RedisConfiguration.LoggerFactory` is set (done automatically by `AddStackExchangeRedisExtensions`).

For manual setup:

```csharp
var config = new RedisConfiguration
{
    LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole()),
    // ... other config
};
```

## Log Events Reference

All log messages have a stable `EventId` for filtering and alerting.

### Connection Pool (1xxx)

| EventId | Level | Message | When |
|---------|-------|---------|------|
| 1001 | `Information` | Redis connection pool initialized: {PoolSize} connections to {Endpoint} in {ElapsedMs}ms | Pool created successfully at startup |
| 1002 | `Debug` | Using connection {ConnectionHash} with {Outstanding} outstanding commands | Every `GetConnection()` call |
| 1003 | `Warning` | All Redis connections are disconnected. Using connection {ConnectionHash} in degraded mode | All pool connections are down |
| 1004 | `Debug` | Disposing Redis connection pool ({PoolSize} connections) | Pool is being disposed |
| 1005 | `Information` | Using async ConfigurationOptions handler for pool initialization | Azure Managed Identity or custom handler active |
| 1006 | `Error` | Redis pool initialization failed at connection {Index} of {PoolSize}. Cleaning up {CreatedConnections} previously created connections | Connection fails during pool creation |

### Connection State (2xxx)

| EventId | Level | Message | When |
|---------|-------|---------|------|
| 2001 | `Error` | Redis connection failed: {FailureType} | StackExchange.Redis `ConnectionFailed` event |
| 2002 | `Warning` | Redis connection restored to {Endpoint} | StackExchange.Redis `ConnectionRestored` event |
| 2003 | `Error` | Redis internal error from {Origin} | StackExchange.Redis `InternalError` event |
| 2004 | `Error` | Redis server error: {ErrorMessage} | StackExchange.Redis `ErrorMessage` event |

### Client Factory (3xxx)

| EventId | Level | Message | When |
|---------|-------|---------|------|
| 3001 | `Information` | Redis client created: {ClientName} (database: {Database}) | Each named client registered |

### Pub/Sub (4xxx)

| EventId | Level | Message | When |
|---------|-------|---------|------|
| 4001 | `Error` | Error processing subscription message on channel {Channel} | Subscription handler throws an exception |

## Recommended Log Levels

| Environment | Minimum Level | What you see |
|-------------|--------------|-------------|
| **Production** | `Warning` | Disconnections, degraded mode, handler errors |
| **Staging** | `Information` | Pool init, client creation, Azure handler usage |
| **Development** | `Debug` | Every connection selection, pool disposal |

## Filtering

Use the `StackExchange.Redis.Extensions.Core` category to control log output:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "StackExchange.Redis.Extensions.Core": "Information"
    }
  }
}
```

## Interpreting Common Log Patterns

### Healthy startup

```
info: StackExchange.Redis.Extensions.Core[1001] Redis connection pool initialized: 5 connections to localhost:6379 in 234ms
info: StackExchange.Redis.Extensions.Core[3001] Redis client created: Default (database: 0)
```

### Connection flap (reconnecting)

```
fail: StackExchange.Redis.Extensions.Core[2001] Redis connection failed: SocketFailure
warn: StackExchange.Redis.Extensions.Core[2002] Redis connection restored to localhost:6379
```

### All connections down

```
warn: StackExchange.Redis.Extensions.Core[1003] All Redis connections are disconnected. Using connection 12345678 in degraded mode
```

This means SE.Redis is attempting reconnection. Operations will fail until at least one connection is restored. No action is needed — the pool automatically recovers.

### Subscription handler failure

```
fail: StackExchange.Redis.Extensions.Core[4001] Error processing subscription message on channel orders:new
      System.InvalidOperationException: Failed to process order
```

The subscription is still active. The failed message was logged but the handler continues processing subsequent messages.

### Pool initialization failure (e.g., Azure token expired)

```
fail: StackExchange.Redis.Extensions.Core[1006] Redis pool initialization failed at connection 2 of 5. Cleaning up 2 previously created connections
```

The 2 successfully created connections are properly disposed. The original exception will propagate to DI, failing the application startup.

## Performance

All log messages use compile-time source generation (`[LoggerMessage]` attribute), which means:
- **Zero boxing** of value-type parameters (int, long, double)
- **Zero string interpolation** when the log level is disabled
- **No temporary allocations** for message formatting
- The `IsEnabled()` check is built into the generated code
