# StackExchange.Redis.Extensions Documentation

## Getting Started

* [Setup & Installation](setup-1.md)
* [Dependency Injection](dependency-injection.md)

## Configuration

* [Configuration Overview](configuration/README.md)
  * [JSON Configuration](configuration/json-configuration.md)
  * [C# Configuration](configuration/c-configuration.md)
  * [Connection String](configuration/connectionstring-configuration.md)
  * [Connection Pool](configuration/connection-pool.md)

## Serializers

* [Serializers Overview](serializers/README.md)
  * [System.Text.Json](serializers/system.text.json.md) (recommended)
  * [Newtonsoft Json.NET](serializers/newtonsoft-json.net.md)
  * [MemoryPack](serializers/memoryPack.md)
  * [MsgPack](serializers/msgpack.md)
  * [Protobuf](serializers/protobuf.md)
  * [Utf8Json](serializers/utf8.md)

## Compression

* [Compression Overview](compressors.md) — GZip, Brotli, LZ4, Snappy, Zstandard

## Features

* [Add, Retrieve and Remove Objects](usage/add-and-retrieve-complex-object-to-redis.md)
* [Work with Multiple Items](usage/work-with-multiple-items.md)
* [Replace an Object](usage/replace-an-object.md)
* [Hash Operations](usage/README.md)
* [Hash Field Expiry](hash-field-expiry.md) (Redis 7.4+)
* [GeoSpatial Indexes](geospatial.md)
* [Redis Streams](streams.md)
* [Pub/Sub Messaging](pubsub.md)
* [Custom Serializer](usage/custom-serializer.md)

## Advanced

* [Logging & Diagnostics](logging.md)
* [Multiple Redis Servers](multipleServers.md)
* [Azure Managed Identity](azure-managed-identity.md)
* [OpenTelemetry](openTelemetry.md)
* [ASP.NET Core Middleware](asp.net-core/README.md)
  * [Expose Redis Information](asp.net-core/expose-redis-information.md)

## Development

* [Packages](packages.md)
* [Unit Tests](work-with-the-code/unit-tests.md)
* [License](license.md)
* [Helpful Links](helpful-link.md)
