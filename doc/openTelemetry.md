# OpenTelemetry

Because this libraries use a connection pool in order to be faster and prevent timeouts, you have to use a "special" way to integrate the library with OpenTelemetry because we have more than a single connection.

The first to do is to reference all the OpenTelemetry libraries included `OpenTelemetry.Instrumentation.StackExchangeRedis`
Than instruments all the connections.

```csharp
services
    .AddRedisInstrumentation()
    .ConfigureRedisInstrumentation((sp, instrumentation) =>
    {
        var redisClient = sp.GetRequiredService<IRedisClient>();

        foreach (var connection in redisClient.ConnectionPoolManager.GetConnections())
            instrumentation.AddConnection(connection);
    });
```
