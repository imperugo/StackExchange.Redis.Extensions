# OpenTelemetry

Because this libraries use a connection pool in order to be faster and prevent timeouts, you have to use a "special" way to integrate the library with OpenTelemetry because we have more than a single connection.

The first to do is to reference all the OpenTelemetry libraries included `OpenTelemetry.Instrumentation.StackExchangeRedis`
Than instruments all the connections.

```csharp
services
    .AddOpenTelemetry()
    .WithTracing(b =>
    {
        // Do your stuff here
    
        b.AddInstrumentation((sp, traceProvider) =>
        {
            // Iterate all connection and add the instrumentation
            foreach (var connection in sp.GetRequiredService<IRedisClient>().ConnectionPoolManager.GetConnections())
                b.AddRedisInstrumentation(connection, opt => opt.SetVerboseDatabaseStatements = true);
    
            return traceProvider;
        });
    });
```
