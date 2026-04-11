# JSON Configuration

If you work with .NET 8+, the JSON format is the best approach because the framework offers built-in support for binding JSON files to C# classes. Here is an example configuration:

```json
{
  "Redis": {
    "Password": "my_super_secret_password",
    "AllowAdmin": true,
    "Ssl": false,
    "ConnectTimeout": 6000,
    "ConnectRetry": 2,
    "Database": 0,
    "ServiceName": "my-sentinel",
    "Hosts": [
      {
        "Host": "192.168.0.10",
        "Port": "6379"
      },
      {
        "Host": "192.168.0.11",
        "Port": "6381"
      }
    ],
    "MaxValueLength": 1024,
    "PoolSize": 5,
    "KeyPrefix": "_my_key_prefix_"
  }
}
```

Now bind it to a `RedisConfiguration` object:

```csharp
var redisConfiguration = builder.Configuration
    .GetSection("Redis")
    .Get<RedisConfiguration>();
```

Finally, register the services:

```csharp
builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfiguration);
```

{% hint style="info" %}
If you prefer to use the `appsettings.json` file, just call `GetSection("Redis")` on the configuration object. No separate JSON file is required.
{% endhint %}

### Using a Separate Configuration File

If you prefer to keep the Redis configuration in a dedicated file (e.g., `redis.json`), you can add it to the configuration builder:

```csharp
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("Configuration/redis.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Configuration/redis.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var redisConfiguration = builder.Configuration
    .GetSection("Redis")
    .Get<RedisConfiguration>();
```
