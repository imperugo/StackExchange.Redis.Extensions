# StackExchange.Redis.Extensions

StackExchange.Redis.Extensions is a library that extends [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) allowing you a set of functionality needed by common applications.
The library is signed and completely compatible with the **.Net Standard 2.0**


## What can it be used for?
Caching of course. Instead of use directly [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) could be easier use `ICacheClient`

For example:

- **Add an object to Redis**;
- **Custom Serialized (need to implement your own serializer inherit from ISerializer)**;
- **Changed Flush method**;
- **Remove an object from Redis**;
- **Search Keys into Redis**;
- **Retrieve multiple object with a single roundtrip**;
- **Store multiple object with a single roundtrip**;
- **Get Redis Server information**;
- **Set Add**;
- **Set AddAdd**;
- **SetRemove**;
- **SetRemoveAll**;
- **Set Member**;
- **Pub/Sub events**;
- **Save**;
- **Async methods**;
- **Hash methods**;
- **Support for Keyspace isolation**;
- **Support for multiple database**:
- **Much more**;

Channel  | Status | 
-------- | :------------: | 
Nuget (Core) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Core.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Core/)
Nuget (Json.NET) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Newtonsoft.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Newtonsoft/)
Nuget (MsgPack) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.MsgPack.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.MsgPack/)
Nuget (Protobuf) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Protobuf.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Protobuf/)
Nuget (UTF8Json) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Utf8Json.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Utf8Json/)
Nuget (Binary) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Binary.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Binary/)
Nuget (System.Text.Json) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.System.Text.Json.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.System.Text.Json/)


## How to install it
StackExchange.Redis.Extensions is composed by two libraries, the Core and the Serializer implementation.
Because there are several good serializer and we don't want add another dependency in your project you can choose your favorite or create a new one.

## Install Core 

```
PM> Install-Package StackExchange.Redis.Extensions.Core
```

## Install Json.NET implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.Newtonsoft
```

## Install Jil implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.Jil
```

## Install Message Pack CLI implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.MsgPack
```

## Install Protocol Buffers implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.Protobuf 
```

## Install Binary Formatter implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.Binary 
```

## Install UTF8Json Formatter implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.Utf8Json 
```

## Install System.Text.Json Formatter implementation 

```
PM> Install-Package StackExchange.Redis.Extensions.System.Text.Json
```

## How to configure it
You can use it registering the instance with your favorite Container. Here an example using [Castle Windsor](https://github.com/castleproject/Windsor):

About the configuration is enough to create an instance of `RedisConfiguration`

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
	}
};
```

of course some of them are options (take a look [here](https://github.com/imperugo/StackExchange.Redis.Extensions/blob/master/src/StackExchange.Redis.Extensions.Core/Configuration/RedisConfiguration.cs))

if you are running this library on ASP.NET Core, you can use the following code:

```csharp
config.SetBasePath(env.ContentRootPath)
.AddJsonFile("./Configuration/appSettings.json", optional: false, reloadOnChange: true)
.AddJsonFile($"./Configuration/appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
.AddEnvironmentVariables();

IConfigurationRoot cfg = config.Build();

var redisConfiguration = cfg.GetSection("Redis").Get<RedisConfiguration>();
```

here the json file

```json
{
	"Redis": {
		"Password": "my_super_secret_password",
		"AllowAdmin": true,
		"Ssl": false,
		"ConnectTimeout": 6000,
		"ConnectRetry": 2,
		"Database": 0,
		"Hosts": [
		{
			"Host": "192.168.0.10",
			"Port": "6379"
		},
		{
			"Host": "192.168.0.11",
			"Port": "6381"
		}]
	}
}
```

In case of `App.Config` or `Web.Config` you have to use a specific package that reads the configuration from the "old" configuration file.

```
PM> Install-Package StackExchange.Redis.Extensions.LegacyConfiguration
```

Here the example of an `App.config` file

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="redisCacheClient" type="StackExchange.Redis.Extensions.LegacyConfiguration.RedisCachingSectionHandler, StackExchange.Redis.Extensions.LegacyConfiguration" />
	</configSections>

	<redisCacheClient allowAdmin="true" ssl="false" connectTimeout="3000" database="24">
		<serverEnumerationStrategy mode="Single" targetRole="PreferSlave" unreachableServerAction="IgnoreIfOtherAvailable" /> 
		<hosts>
			<add host="127.0.0.1" cachePort="6379" />
		</hosts>
	</redisCacheClient>

</configuration>
```

after that, to have the configuration, is enough to run this code

```chsarp
var redisConfiguration = RedisCachingSectionHandler.GetConfig();
```

With dependency Injection you can do something like this using Castle Windsor

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<NewtonsoftSerializer>()
				.LifestyleSingleton());

container.Register(Component.For<IRedisCacheClient>()
				.ImplementedBy<RedisCacheClient>()
				.LifestyleSingleton());

container.Register(Component.For<IRedisCacheConnectionPoolManager>()
				.ImplementedBy<RedisCacheConnectionPoolManager>()
				.LifestyleSingleton());

container.Register(Component.For<IRedisDefaultCacheClient>()
				.ImplementedBy<RedisDefaultCacheClient>()
				.LifestyleSingleton());

```

or using ASP.NET Core integrated DI:

```csharp
services.AddSingleton(redisConfiguration);
services.AddSingleton<IRedisCacheClient,RedisCacheClient>();
services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
services.AddSingleton<ISerializer,NewtonsoftSerializer>();
```

or you can create your own instance

```csharp
var serializer = new NewtonsoftSerializer();
var cacheClient = new StackExchangeRedisCacheClient(serializer, redisConfiguration);
```

or install the specific package

```
PM> StackExchange.Redis.Extensions.AspNetCore
```
and then 

```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(myRedisConfigurationInstance);
}
```


## Serialization
In order to store a class into Redis, that class must be serializable. Below is the list of serialization options:

- [**BinaryFormatter**](http://msdn.microsoft.com/en-us/library/72hyey7b%28v=vs.110%29.aspx) - Requires `SerializableAttribute` on top of the class to store into Redis.
- [**Jil**](https://github.com/kevin-montrose/Jil) - Fastest JSON serializer.
- [**MessagePack CLI**](https://github.com/msgpack/msgpack-cli) - serialization/deserialization for CLI.
- [**Newtonsoft**](https://github.com/JamesNK/Newtonsoft.Json) - Uses Json.Net to serialize a class without `SerializableAttribute`.
- [**Protocol Buffers**](https://developers.google.com/protocol-buffers/) - Fastest overall serializer which also happens to produce the smallest output. Developed by Google. Using [protobuf-net](https://github.com/mgravell/protobuf-net) implementation.
- [**Utf8Json**](https://github.com/neuecc/Utf8Json) - Definitely Fastest and Zero Allocation JSON Serializer for C#(.NET, .NET Core, Unity and Xamarin), this serializer write/read directly to UTF8 binary so boostup performance


## How can I store an object into Redis?
There are several methods in `IRedisCacheClient` that can solve this request.

```csharp

var user = new User()
{
	Firstname = "Ugo",
	Lastname = "Lattanzi",
	Twitter = "@imperugo"
	Blog = "http://tostring.it"
}

bool added = cacheClient.Db0.Add("my cache key", user, DateTimeOffset.Now.AddMinutes(10));

```

## How can I retrieve an object into Redis?
Easy:

```csharp
var cachedUser = cacheClient.Db0.Get<User>("my cache key");
```

## How can I retrieve multiple object with single roundtrip?
That's a cool feature that is implemented into `ICacheClient` implementation:

```csharp
var cachedUsers = cacheClient.Db0.GetAll<User>(new {"key1","key2","key3"});
```

## How can I add multiple object with single roundtrip?
That's a cool feature that is implemented into `IRedisCacheClient` implementation:

```csharp
IList<Tuple<string, string>> values = new List<Tuple<string, string>>();

values.Add(new Tuple<string, string>("key1","value1"));
values.Add(new Tuple<string, string>("key2","value2"));
values.Add(new Tuple<string, string>("key3","value3"));

bool added = cacheClient.Db0.AddAll(values);
```

## Can I search keys into Redis?
Yes that's possible using a specific pattern.
If you want to search all keys that start with `myCacheKey`:

```csharp
var keys = cacheClient.Db0.SearchKeys("myCacheKey*");
```

If you want to search all keys that contain with `myCacheKey`:

```csharp
var keys = cacheClient.Db0.SearchKeys("*myCacheKey*");
```

If you want to search all keys that end with ```myCacheKey```:

```csharp
var keys = cacheClient.Db0.SearchKeys("*myCacheKey");
```

## Can I use a Redis method directly from ICacheClient without add another dependency to my class?

Of course you can. `IRedisCacheClient` exposes a readonly property named `Database` that is the implementation of IDatabase by [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)

```csharp
cacheClient.Db0.Database.SetAdd("mykey","another key");
```

## How can I get server information?
`ICacheClient` has a method `GetInfo` and `GetInfoAsync` for that:

```csharp
var info = cacheClient.Db0.GetInfo();
```

>If you don't want to specify every time you can use the client the database, is enought to use `IRedisDefaultCacheClient` instead of `IRedisCacheClient`


For more info about the values returned, take a look [here](http://redis.io/commands/INFO)

## Contributing
**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)
 * [Open an issue](https://github.com/imperugo/StackExchange.Redis.Extensions/issues) if you encounter a bug or have a suggestion for improvements/features


Once you're familiar with Git and GitHub, clone the repository and start contributing.
