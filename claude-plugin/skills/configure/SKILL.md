---
name: redis-configure
description: Generate StackExchange.Redis.Extensions configuration and DI registration from natural language requirements
---

# Redis Configure

Generate complete `RedisConfiguration` and ASP.NET Core DI setup for StackExchange.Redis.Extensions based on the user's requirements.

## When to use

When the user asks to:
- Set up Redis in their .NET project
- Configure connection pooling, Sentinel, TLS, Azure Managed Identity
- Choose a serializer or compressor
- Set up multiple Redis instances

## How to respond

1. Ask clarifying questions if needed (single vs. multi-instance, cloud vs. local, auth method)
2. Generate the complete configuration

## Configuration Reference

### NuGet packages required
- `StackExchange.Redis.Extensions.Core` — always required
- `StackExchange.Redis.Extensions.AspNetCore` — for DI registration
- One serializer: `System.Text.Json` (recommended), `Newtonsoft`, `MemoryPack`, `MsgPack`, `Protobuf`, `ServiceStack`, `Utf8Json`
- Optional compressor: `Compression.LZ4` (fastest), `Compression.Snappier`, `Compression.ZstdSharp` (best ratio), `Compression.GZip` (no deps), `Compression.Brotli` (best ratio for text)

### appsettings.json structure
```json
{
  "Redis": {
    "Password": "",
    "AllowAdmin": true,
    "Ssl": false,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "Database": 0,
    "Hosts": [{ "Host": "localhost", "Port": 6379 }],
    "PoolSize": 5,
    "IsDefault": true
  }
}
```

### Program.cs registration
```csharp
var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfig);
// Optional compression:
builder.Services.AddRedisCompression<LZ4Compressor>();
```

### Azure Managed Identity
```csharp
redisConfig.ConfigurationOptionsAsyncHandler = async opts =>
{
    await opts.ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
    return opts;
};
```

### Sentinel configuration
```csharp
var config = new RedisConfiguration
{
    ServiceName = "mymaster",
    Hosts = new[] {
        new RedisHost { Host = "sentinel1", Port = 26379 },
        new RedisHost { Host = "sentinel2", Port = 26379 },
    },
    IsDefault = true,
};
```

### Multiple instances with Keyed DI (.NET 8+)
```csharp
var configs = new[]
{
    new RedisConfiguration { Name = "Cache", IsDefault = true, /* ... */ },
    new RedisConfiguration { Name = "Session", /* ... */ },
};
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(configs);

// Option 1: Keyed DI Services (.NET 8+)
public class MyService(
    [FromKeyedServices("Cache")] IRedisDatabase cacheDb,
    [FromKeyedServices("Session")] IRedisDatabase sessionDb) { }

// Option 2: Factory pattern (all TFMs)
// inject IRedisClientFactory, call GetRedisClient("Session")
```

### Health Check
```csharp
builder.Services.AddHealthChecks()
    .AddRedisExtensionsHealthCheck(
        name: "redis",
        tags: new[] { "db", "cache", "ready" });
```

### IDistributedCache adapter
```csharp
// Call after AddStackExchangeRedisExtensions
builder.Services.AddRedisDistributedCache();

// Uses Hash-based storage compatible with Microsoft.Extensions.Caching.StackExchangeRedis
// Supports sliding expiration, absolute expiration, and refresh
```

### Key properties
| Property | Default | Description |
|----------|---------|-------------|
| PoolSize | 5 | Connection pool size |
| ConnectionSelectionStrategy | LeastLoaded | LeastLoaded or RoundRobin |
| SyncTimeout | 5000 | Sync timeout (ms) |
| ConnectTimeout | 5000 | Connect timeout (ms) |
| KeyPrefix | "" | Prefix for all keys and channels |
| KeepAlive | -1 | Heartbeat interval (seconds) |
| ClientName | null | Connection client name |
| MaxValueLength | 0 | Max serialized value size (0 = unlimited) |

### Serializer selection guide
| Scenario | Recommended |
|----------|-------------|
| General purpose | System.Text.Json |
| Legacy JSON.NET compatibility | Newtonsoft |
| Maximum performance (binary) | MemoryPack (net8.0+) |
| Cross-language compatibility | Protobuf or MsgPack |

### Compressor selection guide
| Scenario | Recommended |
|----------|-------------|
| Lowest latency (caching) | LZ4 |
| Best compression ratio | ZstdSharp or Brotli |
| No external dependencies | GZip |
