# Jil

Definitely Fastest and Zero Allocation JSON Serializer for C#(.NET, .NET Core, Unity and Xamarin), this serializer write/read directly to UTF8 binary so boostup performance. And I adopt the same architecture as the fastest binary serializer, MessagePack for C# that I've developed.

### Install


```bash
Install-Package StackExchange.Redis.Extensions.Utf8
```

```bash
dotnet add package StackExchange.Redis.Extensions.Utf8
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.Utf8" Version="8.0.5" />
```

```bash
paket add StackExchange.Redis.Extensions.Utf8****
```

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton<ISerializer, Utf8Serializer>();
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<Utf8Serializer>()
				.LifestyleSingleton());
```
