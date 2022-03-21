# Newtonsoft Json.Net

Json.NET is a popular high-performance JSON framework for .NET. If you are familiar with it, probably this package is what you need.

### Install

{% tabs %}
{% tab title="PackageManager" %}
```bash
Install-Package StackExchange.Redis.Extensions.Newtonsoft
```
{% endtab %}

{% tab title=".NET Cli" %}
```
dotnet add package StackExchange.Redis.Extensions.Newtonsoft
```
{% endtab %}

{% tab title="Package Reference" %}
```
<PackageReference Include="StackExchange.Redis.Extensions.Newtonsoft" Version="5.5.0" />
```
{% endtab %}

{% tab title="Paket cli" %}
```
paket add StackExchange.Redis.Extensions.Newtonsoft
```
{% endtab %}
{% endtabs %}

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```aspnet
services.AddSingleton<ISerializer, NewtonsoftSerializer>();
```

Example using **Castle.Windsor:**

```aspnet
container.Register(Component.For<ISerializer>()
				.ImplementedBy<NewtonsoftSerializer>()
				.LifestyleSingleton());
```

{% hint style="info" %}
The library used is Newtonsoft.Json version 12.0.3. For more information about it, please take a look here [https://www.newtonsoft.com/json](https://www.newtonsoft.com/json)
{% endhint %}
