# Expose Redis Information

Sometimes it is helpful to expose Redis information outside of your application as a JSON endpoint, in order to monitor the connection, the status of the server, and so on.

For this we have created a specific middleware that you can use like this:

```csharp
var app = builder.Build();

app.UseRedisInformation();

app.Run();
```

From now on, you have two endpoints available:

**Connection information "/redis/connectionInfo"**

```json
{
  "RequiredPoolSize": 5,
  "ActiveConnections": 1,
  "InvalidConnections": 0,
  "ReadyNotUsedYet": 4
}
```

**Redis information "/redis/info"**

```json
{
  "redis_version": "7.2.4",
  "redis_mode": "standalone",
  "os": "Linux 6.1.0 x86_64",
  "arch_bits": "64",
  "tcp_port": "6379",
  "uptime_in_seconds": "34131",
  "uptime_in_days": "0",
  "connected_clients": "2",
  "used_memory_human": "1.65M",
  "used_memory_peak_human": "1.75M",
  "maxmemory_policy": "noeviction",
  "role": "master",
  "connected_slaves": "0",
  "cluster_enabled": "0"
}
```

Of course these responses could contain sensitive data, so you can restrict access to a specific set of IP addresses or apply your own custom authorization logic:

```csharp
var app = builder.Build();

app.UseRedisInformation(options =>
{
    options.AllowedIPs = Array.Empty<IPAddress>();
    // AllowFunction has higher priority than AllowedIPs if not null.
    options.AllowFunction = (HttpContext ctx) =>
    {
        // Your custom authorization logic.
        return true;
    };
});

app.Run();
```

**Warnings**:

Since 11.0, the `RedisInformationMiddleware` has some breaking changes. See [issue #430](https://github.com/imperugo/StackExchange.Redis.Extensions/issues/430) for details.
