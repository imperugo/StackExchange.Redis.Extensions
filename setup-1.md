# Setup

In order to use StackExchange.Redis.Extensions you need to use two packages:

* The `core` package;
* The `serialier` package;

### The Core Package

The core package is the library includes all the method you need into your code, like `AddAsync`, `ExistsAsync` and so on. It carries `StackExchange.Redis` and usually you can access to it usi the interface `IRedisCacheClient` or `IRedisDatabase.`

### The Serializer package

This package includes the the class that will be in charge to serialize the object you want to add into Redis.   
In order to prevent conflict between **StackExchange.Redis** serializer and what you are using into your application, the are different options:

* Newtonsoft Json.Net
* MsgPack
* Protobuf-net
* UTF8Json
* Binary
* System.Text.Json

