# MemoryPack

Zero encoding extreme performance binary serializer for C# and Unity.

### Install

```bash
dotnet add package StackExchange.Redis.Extensions.MemoryPack
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.MemoryPack" Version="12.*" />
```

### Setup

If you are using the `StackExchange.Redis.Extensions.AspNetCore` package, register it via the built-in extension method in your `Program.cs`:

```csharp
builder.Services.AddStackExchangeRedisExtensions<MemoryPackSerializer>(redisConfiguration);
```

Otherwise, register the serializer manually:

```csharp
services.AddSingleton<ISerializer, MemoryPackSerializer>();
```
