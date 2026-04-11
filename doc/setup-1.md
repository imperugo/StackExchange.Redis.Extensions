# Setup

In order to use StackExchange.Redis.Extensions you need to use two packages:

* The `core` package;
* The `serializer` package;

Optionally you can also add:

* A `compression` package (to reduce memory and bandwidth usage);

### The Core Package

The core package is the library that includes all the methods you need in your code, like `AddAsync`, `ExistsAsync` and so on. It carries `StackExchange.Redis` and you can access it using the `IRedisDatabase` interface injected via dependency injection.

### The Serializer Package

This package includes the class responsible for serializing the objects you want to store in Redis.
In order to prevent conflicts between **StackExchange.Redis** serialization and what you are using in your application, there are different options:

* Newtonsoft Json.NET
* System.Text.Json
* MsgPack
* Protobuf-net
* UTF8Json
* MemoryPack
* ServiceStack

### The Compression Packages (optional)

You can optionally add a compression package to transparently compress data before storing it in Redis. Available compressors:

* GZip (no external dependencies)
* Brotli (no external dependencies)
* LZ4
* Snappier
* ZstdSharp

See the [Compression](compressors.md) page for setup instructions and recommendations.
