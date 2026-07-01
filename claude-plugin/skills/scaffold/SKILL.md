---
name: redis-scaffold
description: Generate code patterns for StackExchange.Redis.Extensions — Streams, Geo, VectorSet, Hash, Pub/Sub, Sets, Compression
---

# Redis Scaffold

Generate production-ready code patterns using StackExchange.Redis.Extensions.

## When to use

When the user asks to implement:
- Redis Streams consumer group workflow
- GeoSpatial search (nearby locations)
- VectorSet similarity search (RAG, recommendations)
- Hash operations with per-field TTL
- Pub/Sub messaging
- Caching patterns (cache-aside, write-through, IDistributedCache)
- Atomic counters (increment/decrement)
- Set operations (union, intersect, difference)
- Key management (rename, type, dump/restore)
- Bulk operations

## Patterns

### Cache-Aside Pattern
```csharp
public class ProductService(IRedisDatabase redis, IProductRepository repo)
{
    public async Task<Product?> GetProductAsync(int id)
    {
        var key = $"product:{id}";
        var cached = await redis.GetAsync<Product>(key);

        if (cached is not null)
            return cached;

        var product = await repo.GetByIdAsync(id);

        if (product is not null)
            await redis.AddAsync(key, product, TimeSpan.FromMinutes(30));

        return product;
    }
}
```

### Consumer Group (Streams)
```csharp
public class OrderProcessor(IRedisDatabase redis)
{
    public async Task ProcessAsync(CancellationToken ct)
    {
        await redis.StreamCreateConsumerGroupAsync("orders", "processors", "0-0", createStream: true);

        while (!ct.IsCancellationRequested)
        {
            var entries = await redis.StreamReadGroupAsync("orders", "processors", "worker-1", ">", count: 10); // ">" = read only new messages

            foreach (var entry in entries)
            {
                try
                {
                    // Process message
                    await redis.StreamAcknowledgeAsync("orders", "processors", entry.Id.ToString());
                }
                catch
                {
                    // Message stays in PEL for retry
                }
            }

            if (entries.Length == 0)
                await Task.Delay(1000, ct);
        }
    }
}
```

### GeoSpatial Search
```csharp
public class StoreLocator(IRedisDatabase redis)
{
    public async Task<GeoRadiusResult[]> FindNearbyAsync(double lat, double lon, double radiusKm)
    {
        return await redis.GeoSearchAsync("stores", lon, lat,
            new GeoSearchCircle(radiusKm, GeoUnit.Kilometers),
            count: 20, order: Order.Ascending);
    }

    public async Task AddStoreAsync(string id, double lat, double lon)
    {
        await redis.GeoAddAsync("stores", lon, lat, id);
    }
}
```

### VectorSet Similarity Search (RAG)
```csharp
public class DocumentSearch(IRedisDatabase redis)
{
    public async Task IndexAsync(string docId, float[] embedding, string title)
    {
        await redis.VectorSetAddAsync("docs",
            VectorSetAddRequest.Member(docId, embedding,
                attributes: $"""{{ "title": "{title}" }}"""));
    }

    public async Task<List<(string Id, double Score)>> SearchAsync(float[] queryVector, int topK = 5)
    {
        using var results = await redis.VectorSetSimilaritySearchAsync("docs",
            VectorSetSimilaritySearchRequest.ByVector(queryVector) with { Count = topK });

        var items = new List<(string, double)>();
        if (results is not null)
            foreach (var r in results.Span)
                items.Add((r.Member!, r.Score));

        return items;
    }
}
```

### Hash with Per-Field TTL
```csharp
public class SessionStore(IRedisDatabase redis)
{
    public async Task SetSessionDataAsync(string userId, string token, object profile)
    {
        var hashKey = $"user:{userId}";

        // Permanent profile data
        await redis.HashSetAsync(hashKey, "profile", profile);

        // Session token expires in 30 minutes
        await redis.HashSetWithExpiryAsync(hashKey, "token", token, TimeSpan.FromMinutes(30));
    }
}
```

### Pub/Sub with Typed Messages
```csharp
public class EventBus(IRedisDatabase redis)
{
    public async Task PublishAsync<T>(string channel, T message)
    {
        await redis.PublishAsync(new RedisChannel(channel, RedisChannel.PatternMode.Literal), message);
    }

    public async Task SubscribeAsync<T>(string channel, Func<T?, Task> handler)
    {
        await redis.SubscribeAsync<T>(new RedisChannel(channel, RedisChannel.PatternMode.Literal), handler);
    }
}
```

### Bulk Operations with Expiry
```csharp
public async Task CacheBulkAsync(IRedisDatabase redis, Dictionary<string, Product> products)
{
    var items = products.Select(p => Tuple.Create($"product:{p.Key}", p.Value)).ToArray();
    await redis.AddAllAsync(items, TimeSpan.FromHours(1));
}
```

### Atomic Counters
```csharp
public class RateLimiter(IRedisDatabase redis)
{
    public async Task<bool> AllowRequestAsync(string clientId, int maxRequests, TimeSpan window)
    {
        var key = $"ratelimit:{clientId}";
        var count = await redis.StringIncrementAsync(key);

        if (count == 1)
            await redis.UpdateExpiryAsync(key, window);

        return count <= maxRequests;
    }
}
```

### Set Combine (Union, Intersect, Difference)
```csharp
public class TagService(IRedisDatabase redis)
{
    public async Task<string[]> GetCommonTagsAsync(string user1, string user2)
    {
        return await redis.SetCombineAsync<string>(
            SetOperation.Intersect, $"user:{user1}:tags", $"user:{user2}:tags");
    }

    public async Task<long> MergeTagsAsync(string destination, params string[] sources)
    {
        return await redis.SetCombineAndStoreAsync(
            SetOperation.Union, destination, sources);
    }
}
```

### Key Management
```csharp
// Rename with condition
await redis.KeyRenameAsync("temp:data", "final:data", When.NotExists);

// Check type before operations
var type = await redis.KeyTypeAsync("my-key"); // RedisType.String, Set, Hash, ...

// Dump and restore (migrate between databases)
var dump = await redis.KeyDumpAsync("source-key");
await redis.KeyRestoreAsync("dest-key", dump, TimeSpan.FromHours(24));
```

### IDistributedCache
```csharp
// Registered via: builder.Services.AddRedisDistributedCache()
public class SessionService(IDistributedCache cache)
{
    public async Task SetAsync(string sessionId, byte[] data)
    {
        await cache.SetAsync($"session:{sessionId}", data, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4),
        });
    }
}
```

## Important Notes

- All values go through ISerializer — strings are JSON-encoded ("hello" → "\"hello\"")
- For raw Redis operations, use `redis.Database` directly
- KeyPrefix applies to both keys AND Pub/Sub channels
- VectorSet requires Redis 8.0+
- Hash field expiry requires Redis 7.4+
- Compression wraps ISerializer transparently — all operations benefit automatically
- Lease<T> return types (VectorSet search) must be disposed after use
