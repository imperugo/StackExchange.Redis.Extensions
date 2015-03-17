# StackExchange.Redis.Extensions

StackExchange.Redis.Extensions is a library that extends [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) allowing you a set of functionality needed by common applications.


## What can it be used for?
Caching of course. Instead of use directly [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) could be easier use ``ÌCacheClient```

For example:

- **Add an object to Redis**;
- **Remove an object from Redis**;
- **Search Keys into Redis**;
- **Retrieve multiple object with a single roundtrip**;
- **Store multiple object with a single roundtrip**;
- **Async methods**;
- **Retrieve Redis Server status**;
- **Much more**;

[![Issue Stats](http://www.issuestats.com/github/imperugo/StackExchange.Redis.Extensions/badge/issue)](http://www.issuestats.com/github/imperugo/StackExchange.Redis.Extensions)

##How to install it
StackExchange.Redis.Extensions is composed by two libraries, the Core and the Serializer implementation.
Because there are several good serializer and we don't want add another dependency in your project you can choose your favorite or create a new one.

##Install Core [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Core.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Core/)

```
PM> Install-Package StackExchange.Redis.Extensions.Core
```

## Install Json.NET implementation [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Newtonsoft.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Newtonsoft/)

```
PM> Install-Package StackExchange.Redis.Extensions.Newtonsoft
```

##Install Jil implementation [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Jil.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Jil/)

```
PM> Install-Package StackExchange.Redis.Extensions.Jil
```

##Install Message Pack CLI implementation [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.MsgPack.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.MsgPack/)

```
PM> Install-Package StackExchange.Redis.Extensions.MsgPack
```

## How to configure it
You can use it registering the instance with your favorite Container. Here an example using Castle:

```csharp

container.Register(Component.For<ICacheClient>()				.ImplementedBy<StackExchangeRedisCacheClient>()				.LifestyleSingleton());

```

of you can create your own instance

```csharp

var cacheClient = new StackExchangeRedisCacheClient();

```

To specify the connection string it's enough to add it into AppSetting in your config files (use **RedisConnectionString** as key) or specify your ``ConnectionMultiplexer``` instance into the constructor.


## Serialization
To offer the opportunity to store a class into Redis, that class must be serializable; right now there are three serialization options:

- [**BinarySerialization**](http://msdn.microsoft.com/en-us/library/72hyey7b%28v=vs.110%29.aspx) (Requires ```SerializableAttribute``` on top of the class to store into Redis)
- [**NewtonSoft**](https://github.com/JamesNK/Newtonsoft.Json) (Uses JSon.Net to serialize a class without ```SerializableAttribute```)
- [**Jil**](https://github.com/kevin-montrose/Jil) (Use super fast json serializer)
- [**MessagePack CLI**](https://github.com/msgpack/msgpack-cli) (serialization/deserialization for CLI)


## How can I store an object into Redis?
There are several methods in ```ICacheClient``` that can solve this request.

```csharp

var user = new User()
{
	Firstname = "Ugo",
	Lastname = "Lattanzi",
	Twitter = "@imperugo"
	Blog = "http://tostring.it"
}

bool added = myCacheClient.Add("my cache key", user, DateTimeOffset.Now.AddMinutes(10));

```

## How can I retrieve an object into Redis?
Easy:

```csharp
var cachedUser = myCacheClient.Get<User>("my cache key");
```

## How can I retrieve multiple object with single roundtrip?
That's a cool feature that is implemented into ```ICacheClient``` implementation:

```csharp
var cachedUsers = myCacheClient.GetAll<User>(new {"key1","key2","key3"});
```

## How can I add multiple object with single roundtrip?
That's a cool feature that is implemented into ICacheClient implementation:

```csharp
IList<Tuple<string, string>> values = new List<Tuple<string, string>>();values.Add(new Tuple<string, string>("key1","value1"));values.Add(new Tuple<string, string>("key2","value2"));values.Add(new Tuple<string, string>("key3","value3"));bool added = sut.AddAll(values);
```

## Can I search keys into Redis?
Yes that's possible using a specific pattern.
If you want to search all keys that start with ```myCacheKey```:

```csharp
var keys = myCacheClient.SearchKeys("myCacheKey*");
```

If you want to search all keys that contain with ```myCacheKey```:

```csharp
var keys = myCacheClient.SearchKeys("*myCacheKey*");
```

If you want to search all keys that end with ```myCacheKey```:

```csharp
var keys = myCacheClient.SearchKeys("*myCacheKey");
```

## Can I use a Redis method directly from ICacheClient without add another dependency to my class?

Of course you can. ```ICacheClient``` exposes a readonly property named ```Database``` that is the implementation of IDatabase by [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)

```csharp
myCacheClient.Database.SetAdd("mykey","another key");
```

## How can I get server information?
```ICacheClient``` has a method ```GetInfo``` and ```GetInfoAsync``` for that:

```csharp
var info = myCacheClient.GetInfo();
```

For more info about the values returned, take a look [here](http://redis.io/commands/INFO)

## Contributing
**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)
 * [Open an issue](https://github.com/imperugo/StackExchange.Redis.Extensions/issues) if you encounter a bug or have a suggestion for improvements/features


Once you're familiar with Git and GitHub, clone the repository and start contributing.
