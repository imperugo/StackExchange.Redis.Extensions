# MsgPack

> MessagePack is an efficient binary serialization format. It lets you exchange data among multiple languages like JSON. But it's faster and smaller. Small integers are encoded into a single byte, and typical short strings require only one extra byte in addition to the strings themselves.

### Install

PackageManager:

```bash
Install-Package StackExchange.Redis.Extensions.MsgPack
```

```bash
dotnet add package StackExchange.Redis.Extensions.MsgPack
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.MsgPack" Version="5.5.0" />
```

```bash
paket add StackExchange.Redis.Extensions.MsgPack
```

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<MsgPackObjectSerializer>()
				.LifestyleSingleton());
```
