# Health Check

StackExchange.Redis.Extensions provides an ASP.NET Core health check that monitors the status of all Redis connection pools and verifies connectivity with a PING command.

## Setup

```csharp
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfig);

builder.Services.AddHealthChecks()
    .AddRedisExtensionsHealthCheck();
```

## How it works

The health check iterates all Redis clients registered via `IRedisClientFactory` and reports:

| Status | Condition |
|--------|-----------|
| **Healthy** | All pool connections active, PING succeeds |
| **Degraded** | Some pool connections invalid, but at least one active and PING succeeds |
| **Unhealthy** | All connections invalid, or PING fails |

Diagnostic data includes per-client pool details:

```json
{
  "cache:active": 5,
  "cache:invalid": 0,
  "cache:required": 5,
  "session:active": 3,
  "session:invalid": 2,
  "session:required": 5
}
```

## Configuration

The extension method accepts standard ASP.NET Core health check parameters:

```csharp
builder.Services.AddHealthChecks()
    .AddRedisExtensionsHealthCheck(
        name: "redis",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "cache", "ready" },
        timeout: TimeSpan.FromSeconds(5));
```

## Multiple Redis instances

When multiple `RedisConfiguration` objects are registered (each with a unique `Name`), the health check inspects **all** of them. If any instance has invalid connections, the overall status reflects the worst case.
