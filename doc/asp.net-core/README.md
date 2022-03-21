# ASP.NET Core

If you are running into ASP.NET Core application, there is a specific package that you can use in order to make StackExchange.Redis.Extensions configuration easier.

{% tabs %}
{% tab title="PackageManager" %}
```bash
Install-Package StackExchange.Redis.Extensions.AspNetCore
```
{% endtab %}

{% tab title=".NET Cli" %}
```bash
dotnet add package StackExchange.Redis.Extensions.AspNetCore
```
{% endtab %}

{% tab title="Package Reference" %}
```xml
<PackageReference Include="StackExchange.Redis.Extensions.AspNetCore" Version="6.1.0" />
```
{% endtab %}

{% tab title="Paket cli" %}
```bash
paket add StackExchange.Redis.Extensions.AspNetCore
```
{% endtab %}
{% endtabs %}

### Startup.cs

Into your startup class,&#x20;

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Retrieve the configuration fro your json/xml file

    services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(conf);
}I
```

{% hint style="info" %}
Remember to install also you favorite serializer.
{% endhint %}

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UserRedisInformation();
}
```

