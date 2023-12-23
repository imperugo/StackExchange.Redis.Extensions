# Connection Pooling

Our Redis client wrapper includes an additional feature for connection pooling. Connection pooling can greatly improve the performance of your application by 
distribuiting the load across multiple connections.

To do this, the client undertakes to create a series of active connections to the server so that they are ready when their use is needed.

## Benefits of Connection Pooling

- **Improved performance**: Connection pooling reduces the time spent in opening and closing connections, thereby improving the performance of command execution.
- **Optimized use of resources**: By reusing existing connections, connection pooling helps to save system resources. It limits the number of simultaneous connections, reducing the memory usage and load on your database.

## Disadvantages

Depending on the size of the pool, the client, on startup, opens a connection to the server.

This means that the higher the pool size the longer the application startup time and the number of connections used.

## Connection Selection Strategies

Our Redis client wrapper supports two strategies for selecting connections from the pool that you can select into the RedisConfiguration:

```csharp
var redisConfiguration = new RedisConfiguration()
{
    AbortOnConnectFail = false,
    KeyPrefix = "MyPrefix__",
    Hosts = [
        new RedisHost
        {
            Host = "localhost",
            Port = 6379
        }
    ],
    AllowAdmin = true,
    ConnectTimeout = 3000,
    Database = 0,
    PoolSize = 2,
    ConnectionSelectionStrategy = ConnectionSelectionStrategy.LeastLoaded,
    ServerEnumerationStrategy = new()
    {
        Mode = ServerEnumerationStrategy.ModeOptions.All,
        TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
        UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
    }
};
```

> The important part is the `ConnectionSelectionStrategy` property.

### RoundRobin

With this strategy, each call returns the next connection in the pool in a round-robin manner. This strategy is useful for distributing the load evenly across all the connections. (more information about round robine is available [here](https://en.wikipedia.org/wiki/Round-robin_scheduling))

### LeastLoaded

This strategy returns the least loaded connection for each call. The load of a connection is defined by its `ServerCounters.TotalOutstanding`. This strategy is effective in scenarios where the load is unevenly distributed among connections.

> The default strategy is `LeastLoaded`.

## When to Use Connection Pooling

Connection pooling is particularly beneficial for applications with high traffic, where opening and closing connections frequently can significantly impact performance and there are lot of concurrent operation from the client to the server.

For more information on how to configure the connection pool and set up the selection strategy, refer to the 'Configuration' section of our application.
