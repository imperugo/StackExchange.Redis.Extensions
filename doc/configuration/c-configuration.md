# C# Configuration

If you want to embed your configuration directly into the c# the class more or less looks like this:

```csharp
var redisConfiguration = new RedisConfiguration()
{
	AbortOnConnectFail = true,
	KeyPrefix = "_my_key_prefix_",
	Hosts = new RedisHost[]
	{
		new RedisHost(){Host = "192.168.0.10", Port = 6379},
		new RedisHost(){Host = "192.168.0.11",  Port =6379},
		new RedisHost(){Host = "192.168.0.12",  Port =6379}
	},
	ServiceName = "my-sentinel", // In case you are using Sentinel
	AllowAdmin = true,
	ConnectTimeout = 3000,
	Database = 0,
	Ssl = true,
	Password = "my_super_secret_password",
	ServerEnumerationStrategy = new ServerEnumerationStrategy()
	{
		Mode = ServerEnumerationStrategy.ModeOptions.All,
		TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
		UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
	},
	MaxValueLength = 1024,
	PoolSize = 5
};
```

Not is enough to register it into your DI container:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton(redisConfiguration);
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<RedisConfiguration>().Instance(redisConfiguration));
```

