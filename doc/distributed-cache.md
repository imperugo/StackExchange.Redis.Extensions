# IDistributedCache Adapter

StackExchange.Redis.Extensions includes an `IDistributedCache` implementation that uses the same Redis connection pool managed by the library. This lets you use the standard Microsoft caching abstraction without adding a separate Redis connection.

## Setup

Register the adapter **after** `AddStackExchangeRedisExtensions`:

```csharp
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfig);
builder.Services.AddRedisDistributedCache();
```

This registers `IDistributedCache` as a singleton backed by the default `IRedisDatabase`.

## Usage

Inject `IDistributedCache` anywhere:

```csharp
public class SessionService(IDistributedCache cache)
{
    public async Task<byte[]?> GetSessionAsync(string sessionId)
    {
        return await cache.GetAsync($"session:{sessionId}");
    }

    public async Task SetSessionAsync(string sessionId, byte[] data)
    {
        await cache.SetAsync($"session:{sessionId}", data, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4),
        });
    }
}
```

## Expiration behavior

| Option | Behavior |
|--------|----------|
| `SlidingExpiration` only | TTL resets on every `Get` or `Refresh` |
| `AbsoluteExpiration` only | Fixed TTL, no refresh |
| Both | TTL resets on access, capped by the absolute expiration |
| Neither | Key persists until explicitly removed |

## Storage format

Data is stored as a Redis Hash with three fields:

| Field | Type | Description |
|-------|------|-------------|
| `data` | `byte[]` | The cached value |
| `absexp` | `long` | Absolute expiration as UTC ticks (-1 if not set) |
| `sldexp` | `long` | Sliding expiration as ticks (-1 if not set) |

This schema is **compatible with `Microsoft.Extensions.Caching.StackExchangeRedis`**, enabling zero-downtime migration between providers.

## Key prefix

The adapter accesses Redis through `IRedisDatabase.Database`, which applies the `KeyPrefix` configured in `RedisConfiguration`. All distributed cache keys will be prefixed automatically.

## Sync methods

The `Get`, `Set`, `Remove`, and `Refresh` sync methods use the synchronous `IDatabase` methods directly (not `.GetAwaiter().GetResult()`), avoiding thread pool starvation.
