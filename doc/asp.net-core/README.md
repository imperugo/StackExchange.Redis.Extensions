# ASP.NET Core

If you are running an ASP.NET Core application, there is a specific package that makes StackExchange.Redis.Extensions configuration easier.

### Install

```bash
dotnet add package StackExchange.Redis.Extensions.AspNetCore
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.AspNetCore" Version="12.*" />
```

### Program.cs

Register the library in your `Program.cs` using the minimal hosting model:

```csharp
var builder = WebApplication.CreateBuilder(args);

var redisConfiguration = builder.Configuration
    .GetSection("Redis")
    .Get<RedisConfiguration>();

builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfiguration);

var app = builder.Build();

app.UseRedisInformation();

app.Run();
```

{% hint style="info" %}
Remember to install your preferred serializer package as well (e.g., `StackExchange.Redis.Extensions.System.Text.Json`).
{% endhint %}

### Adding Compression (optional)

You can optionally enable transparent compression by calling `AddRedisCompression` right after `AddStackExchangeRedisExtensions`:

```csharp
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfiguration);
builder.Services.AddRedisCompression<LZ4Compressor>();
```

See the [Compression](../compressors.md) page for all available compressors and configuration options.

### Adding Health Check (optional)

Monitor the Redis connection pool status through ASP.NET Core health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddRedisExtensionsHealthCheck();
```

Returns Healthy / Degraded / Unhealthy based on pool state and PING result. See [Health Check](../health-check.md) for details.

### Adding IDistributedCache (optional)

Use the standard `IDistributedCache` abstraction backed by the same Redis connection:

```csharp
builder.Services.AddRedisDistributedCache();
```

See [IDistributedCache Adapter](../distributed-cache.md) for sliding expiration behavior and storage format.

### Injecting IRedisDatabase

Once registered, you can inject `IRedisDatabase` directly into your controllers, services, or minimal API handlers:

```csharp
app.MapGet("/users/{key}", async (string key, IRedisDatabase redis) =>
{
    var user = await redis.GetAsync<User>(key);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});
```
