# Jil

protobuf-net is a contract based serializer for .NET code, that happens to write data in the "protocol buffers" serialization format engineered by Google. The API, however, is very different to Google's, and follows typical .NET patterns (it is broadly comparable, in usage, to XmlSerializer, DataContractSerializer, etc). It should work for most .NET languages that write standard types and can use attributes.

### Install


```bash
Install-Package StackExchange.Redis.Extensions.Protobuf
```

```bash
dotnet add package StackExchange.Redis.Extensions.Protobuf
```

```xml
<PackageReference Include="StackExchange.Redis.Extensions.Protobuf" Version="8.0.5" />
```

```bash
paket add StackExchange.Redis.Extensions.Protobuf****
```

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton<ISerializer, ProtobufSerializer>();
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<ProtobufSerializer>()
				.LifestyleSingleton());
```
