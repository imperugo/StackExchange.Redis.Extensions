**StackExchange.Redis.Extensions** is a library that extends [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) allowing you a set of functionality needed by common applications. The library is signed and completely compatible with the **.NET Standard 2.0, .NET Framework 4.6.2+, .NET 8.0+** 



The idea of this library is to make easier your live when you need to send / receive objects into Redis, in fact it wraps what the main library [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) offers but serializing and deserializing your objects.

- Here the list of the most asked features:
- Add and retrieve complex object from and to Redis;
- Store and retrieve multiple object with a single request;
- Pub/Sub events;
- Support to Hash methods (https://redis.io/commands/hset);
- Sort methods (https://redis.io/commands/sort);
- List methods (https://redis.io/commands/lpush);
- Support for multiple database;
- Search Keys into Redis;
- Retrieve Redis Server information;
- Async methods;
- Connection pooling;
- Auto purge connections;
- SetPop;
- Profiling Session Provider;



[Documentation](doc/README.md) is composed of articles and guidance detailing how to get the most out of StackExchange.Redis.Extensions



### Release

![.NET Core](https://github.com/imperugo/StackExchange.Redis.Extensions/actions/workflows/dotnetcore.yml/badge.svg)

Latest release is available on NuGet.

| Channel                  | Status                                                       |
| ------------------------ | ------------------------------------------------------------ |
| Nuget (Core)             | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Core.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Core/) |
| Nuget (Json.NET)         | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Newtonsoft.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Newtonsoft/) |
| Nuget (MsgPack)          | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.MsgPack.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.MsgPack/) |
| Nuget (Protobuf)         | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Protobuf.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Protobuf/) |
| Nuget (UTF8Json)         | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.Utf8Json.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.Utf8Json/) |
| Nuget (System.Text.Json) | [![NuGet Status](http://img.shields.io/nuget/v/StackExchange.Redis.Extensions.System.Text.Json.svg?style=flat)](https://www.nuget.org/packages/StackExchange.Redis.Extensions.System.Text.Json/) |



### Q&A

For questions or issues do not hesitate to [raise an issue](https://github.com/imperugo/StackExchange.Redis.Extensions/issues/new/choose) or [get in touch](https://twitter.com/imperugo).

### Contributing

Thanks to all the people who already contributed!

<a href="https://github.com/imperugo/StackExchange.Redis.Extensions/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=imperugo/StackExchange.Redis.Extensions" />
</a>

### License
StackExchange.Redis is Copyright ©  [Ugo Lattanzi](https://www.linkedin.com/in/imperugo/) and other contributors under the MIT license.
