# Multiple Servers

Sometimes there are scenarios where you need to configure to different Redis servers and I don't mean Master or Slave, but different instances with different connection strings and different configuration.

This is possible starting from version 8 and you can do it in this way:

```csharp
var configurations = new[]
        {
            new RedisConfiguration
            {
                AbortOnConnectFail = true,
                KeyPrefix = "MyPrefix__",
                Hosts = new[] { new RedisHost { Host = "localhost", Port = 6379 } },
                AllowAdmin = true,
                ConnectTimeout = 5000,
                Database = 0,
                PoolSize = 5,
                IsDefault = true
            },
            new RedisConfiguration
            {
                AbortOnConnectFail = true,
                KeyPrefix = "MyPrefix__",
                Hosts = new[] { new RedisHost { Host = "localhost", Port = 6389 } },
                AllowAdmin = true,
                ConnectTimeout = 5000,
                Database = 0,
                PoolSize = 2,
                Name = "Secndary Instance"
            }
        };

        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(configurations);
```

Here the important things are the properties `IsDefault` and `Name`.

Each named configuration gets its own isolated connection pool — there is no cross-contamination between instances.

## Resolving named instances

### Option 1: Keyed DI Services (.NET 8+)

When you register configurations with a `Name`, the library automatically registers keyed singletons for `IRedisClient` and `IRedisDatabase`. You can inject them directly using `[FromKeyedServices]`:

```csharp
public class MyService(
    [FromKeyedServices("cache")] IRedisDatabase cacheDb,
    [FromKeyedServices("session")] IRedisDatabase sessionDb)
{
    public async Task Example()
    {
        await cacheDb.AddAsync("key", "value");
        await sessionDb.AddAsync("session:abc", sessionData);
    }
}
```

This also works with minimal APIs:

```csharp
app.MapGet("/data", async ([FromKeyedServices("cache")] IRedisDatabase redis) =>
{
    return await redis.GetAsync<MyData>("key");
});
```

> **Note:** Keyed services are only available when using the overloads that receive `RedisConfiguration` directly (not the `Func<IServiceProvider, ...>` overload), because the configuration names must be known at registration time.

### Option 2: IRedisClientFactory

If you're on .NET 8+ but prefer explicit resolution, or if you're on an older TFM, use `IRedisClientFactory`:

```csharp
public class MyClass(IRedisClientFactory clientFactory)
{
    public Task MyMethod()
    {
        var redisClient = clientFactory.GetRedisClient("session");
        var db = redisClient.GetDefaultDatabase();
        
        // do your stuff here
    }
}
```

Both approaches resolve to the same underlying `IRedisClient` instances.
