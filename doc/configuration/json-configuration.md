# Json Configuration

If you work with .NET Core the Json format is absolutely the best way because .NET Core offers a package that makes easy to bind the json file into a C# class but let's proceed in order, the json first:

```javascript
{
	"Redis": {
		"Password": "my_super_secret_password",
		"AllowAdmin": true,
		"Ssl": false,
		"ConnectTimeout": 6000,
		"ConnectRetry": 2,
		"Database": 0,
		"ServiceName" : "my-sentinel", // In case you are using Sentinel
		"Hosts": [
		{
			"Host": "192.168.0.10",
			"Port": "6379"
		},
		{
			"Host": "192.168.0.11",
			"Port": "6381"
		}]
	},
	"MaxValueLength" = 1024,
	"PoolSize" = 5,
	"KeyPrefix" = "_my_key_prefix_",
}
```

Now:

```javascript
config.SetBasePath(env.ContentRootPath)
.AddJsonFile("./Configuration/redis.json", optional: false, reloadOnChange: true)
.AddJsonFile($"./Configuration/redis.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
.AddEnvironmentVariables();

IConfigurationRoot cfg = config.Build();

var redisConfiguration = cfg.GetSection("Redis").Get<RedisConfiguration>();
```

Finally the dependency injection:

Example using **Microsoft.Extensions.DependencyInjection:**

```aspnet
services.AddSingleton(redisConfiguration);
```

Example using **Castle.Windsor:**

```aspnet
container.Register(Component.For<RedisConfiguration>().Instance(redisConfiguration));
```

{% hint style="info" %}
If you prefer to use the `appsettings.json` file, is enough to invoke the `GetSection("Redis")` line of the code
{% endhint %}
