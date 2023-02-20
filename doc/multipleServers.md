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

Here the important things are the properties `IsDefault` and `Name`

When you use `IRedisDatabase` you get the default connection but, if you want to connect to another one you can retrieve it using its name:

```csharp
public class MyClass
{
    private readonly IRedisClientFactory clientFactory;
    
    public MyClass(IRedisClientFactory clientFactory)
    {
        this.clientFactory = clientFactory;
    }
    
    public Task MyMethod()
    {
        var redisClient = clientFactory.GetRedisClient("Secndary Instance");
        
        // do your stuff here
    }
}
```
