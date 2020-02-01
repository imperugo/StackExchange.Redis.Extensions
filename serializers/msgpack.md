# MsgPack

> MessagePack is an efficient binary serialization format. It lets you exchange data among multiple languages like JSON. But it's faster and smaller. Small integers are encoded into a single byte, and typical short strings require only one extra byte in addition to the strings themselves.

### Install

PackageManager:

{% tabs %}
{% tab title="PackageManager" %}
```bash
Install-Package StackExchange.Redis.Extensions.MsgPack
```
{% endtab %}

{% tab title=".NET Cli" %}
```text
dotnet add package StackExchange.Redis.Extensions.MsgPack
```
{% endtab %}

{% tab title="Package Reference" %}
```
<PackageReference Include="StackExchange.Redis.Extensions.MsgPack" Version="5.5.0" />
```
{% endtab %}

{% tab title="Paket cli" %}
```
paket add StackExchange.Redis.Extensions.MsgPack
```
{% endtab %}
{% endtabs %}

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```aspnet
services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
```

Example using **Castle.Windsor:**

```aspnet
container.Register(Component.For<ISerializer>()
				.ImplementedBy<MsgPackObjectSerializer>()
				.LifestyleSingleton());
```

{% hint style="info" %}
The library used is MsgPack.Cli version 1.0.1. For more information about it, please take a look here [https://github.com/msgpack/msgpack-cli](https://github.com/msgpack/msgpack-cli)
{% endhint %}

