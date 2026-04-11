# Usage

The main entry point for interacting with Redis is the `IRedisDatabase` interface, which you inject via dependency injection. Here is a list of the features the library offers:

* [Add, retrieve and remove complex objects](add-and-retrieve-complex-object-to-redis.md)
* [Replace an object](replace-an-object.md)
* [Work with multiple items](work-with-multiple-items.md)
* [Custom Serialization](custom-serializer.md)
* [Compression](../compressors.md)
* [Pub/Sub events](../pubsub.md)
* **Search Keys into Redis**
* **Store multiple objects with a single roundtrip**
* **Get Redis Server information**
* **Set Add / Set Remove / Set Member**
* **Geo commands**
* **Stream commands**
* **Hash commands (including Hash Field Expiry)**
* **Tag-based operations**
* **Async methods**
* **Support for keyspace isolation**
* **Support for multiple databases**

Note that all the examples are based on a C# object like this:

```csharp
public class User
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CultureInfo? Culture { get; set; }
    public string? TimeZoneId { get; set; }
    public bool EmailConfirmed { get; set; }
    public Company Company { get; set; } = new();
}

public class Company
{
    public string Name { get; set; } = string.Empty;
    public string Vat { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Zipcode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Fax { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
```

{% hint style="info" %}
Your class can be anything that is serializable by your chosen serializer.
{% endhint %}
