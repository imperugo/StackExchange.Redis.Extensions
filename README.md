# StackExchange.Redis.Extensions
StackExchange.Redis.Extensions is a library that extends [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) allowing you a set of functionality needed by common applications.

## What can it be used for?
Caching of course. Instead of use directly [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) could be easier use ``ÌCacheClient```

For example:

- **Add an object to Redis**;
- **Remove an object from Redis**;
- **Search Keys into Redis**;
- **Retrieve multiple object with a single roundtrop**;
- **Store multiple object with a single roundtrop**;
- **Async methods**;
- **Much more**;

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
That's a cool feature that is implemented into ICacheClient implementation:

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

## Can I use a Redis method directly from ICacheClient?
```ICacheClient``` exposes a readonly property named Database that is the implementation of IDatabase by [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)

```csharp
myCacheClient.Database.SetAdd("mykey","another key");
```
