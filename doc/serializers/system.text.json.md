# System.Text.Json

The System.Text.Json provides high-performance and low-allocating types that serialize objects to JavaScript Object Notation (JSON) text and deserialize JSON text to objects, with UTF-8 support built-in. Also provides types to read and write JSON text encoded as UTF-8, and to create an in-memory document object model (DOM), that is read-only, for random access of the JSON elements within a structured view of the data.

### Install

{% tabs %}
{% tab title="PackageManager" %}
```bash
Install-Package StackExchange.Redis.Extensions.System.Text.Json
```
{% endtab %}

{% tab title=".NET Cli" %}
```bash
dotnet add package StackExchange.Redis.Extensions.System.Text.Json
```
{% endtab %}

{% tab title="Package Reference" %}
```xml
<PackageReference Include="StackExchange.Redis.Extensions.System.Text.Json" Version="5.5.0" />
```
{% endtab %}

{% tab title="Paket cli" %}
```bash
paket add StackExchange.Redis.Extensions.System.Text.Json
```
{% endtab %}
{% endtabs %}

### Setup

Now that you have installed the package, you can register it into your favourite dependency injection framework:

Example using **Microsoft.Extensions.DependencyInjection:**

```csharp
services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
```

Example using **Castle.Windsor:**

```csharp
container.Register(Component.For<ISerializer>()
				.ImplementedBy<SystemTextJsonSerializer>()
				.LifestyleSingleton());
```

{% hint style="info" %}
The library used is System.Text.Json version 4.7.0. For more information about it, please take a look here [https://docs.microsoft.com/en-us/dotnet/api/system.text.json?view=netcore-3.1](https://docs.microsoft.com/en-us/dotnet/api/system.text.json?view=netcore-3.1)
{% endhint %}
