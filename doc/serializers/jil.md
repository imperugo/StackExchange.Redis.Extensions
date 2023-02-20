# Jil

A fast JSON (de)serializer, built on [Sigil](https://github.com/kevin-montrose/Sigil) with a number of somewhat crazy optimization tricks.

### Install


```bash
Install-Package StackExchange.Redis.Extensions.Jil
```

```bash
dotnet add package StackExchange.Redis.Extensions.Jil
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.Jil" Version="8.0.5" />
```

```bash
paket add StackExchange.Redis.Extensions.Jil****
```

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton<ISerializer, JilSerializer>();
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<JilSerializer>()
				.LifestyleSingleton());
```
