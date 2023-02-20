# Jil

Zero encoding extreme performance binary serializer for C# and Unity.

### Install


```bash
Install-Package StackExchange.Redis.Extensions.MemoryPack
```

```bash
dotnet add package StackExchange.Redis.Extensions.MemoryPack
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.MemoryPack" Version="8.0.5" />
```

```bash
paket add StackExchange.Redis.Extensions.MemoryPack****
```

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton<ISerializer, MemoryPackSerializer>();
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<MemoryPackSerializer>()
				.LifestyleSingleton());
```
