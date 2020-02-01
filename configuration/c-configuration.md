# C\# Configuration

```javascript
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

