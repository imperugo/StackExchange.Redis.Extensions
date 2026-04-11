# Protobuf

protobuf-net is a contract based serializer for .NET code, that happens to write data in the "protocol buffers" serialization format engineered by Google. The API, however, is very different to Google's, and follows typical .NET patterns (it is broadly comparable, in usage, to XmlSerializer, DataContractSerializer, etc). It should work for most .NET languages that write standard types and can use attributes.

### Install

```bash
dotnet add package StackExchange.Redis.Extensions.Protobuf
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.Protobuf" Version="12.*" />
```

### Setup

If you are using the `StackExchange.Redis.Extensions.AspNetCore` package, register it via the built-in extension method in your `Program.cs`:

```csharp
builder.Services.AddStackExchangeRedisExtensions<ProtobufSerializer>(redisConfiguration);
```

Otherwise, register the serializer manually:

```csharp
services.AddSingleton<ISerializer, ProtobufSerializer>();
```
