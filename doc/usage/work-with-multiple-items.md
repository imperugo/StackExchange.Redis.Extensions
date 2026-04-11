# Work with multiple items

If you need to add, remove or retrieve more than one item, you do not have to send multiple requests to Redis. This library offers methods that allow you to use a single command, increasing performance because there is only one roundtrip from your application to the Redis server.

All the examples below assume you have `IRedisDatabase` injected via dependency injection:

```csharp
public class MyService(IRedisDatabase redis)
{
    // use redis directly
}
```

### Create your instances

```csharp
var user1 = new User
{
    Username = "imperugo",
    FirstName = "Ugo",
    LastName = "Lattanzi",
    Email = "ugo@example.com",
    Company = new Company
    {
        Name = "My Super Company",
        Vat = "IT12345678911",
        Address = "somewhere road 12"
    }
};

var user2 = new User
{
    Username = "mario.rossi",
    FirstName = "Mario",
    LastName = "Rossi",
    Email = "mario@example.com",
    Company = new Company
    {
        Name = "My Super Company",
        Vat = "IT12345678911",
        Address = "somewhere road 12"
    }
};
```

### Add multiple items

```csharp
var items = new List<Tuple<string, User>>
{
    new("key1", user1),
    new("key2", user2)
};

bool added = await redis.AddAllAsync(items, TimeSpan.FromMinutes(10));
```

### Remove multiple items

```csharp
var numberOfItemsRemoved = await redis.RemoveAllAsync(["key1", "key2"]);
```

### Retrieve multiple items

```csharp
var users = await redis.GetAllAsync<User>(["key1", "key2"]);
```

{% hint style="info" %}
It is possible to update the expiry time of the items when retrieving them. The `GetAllAsync` method offers overloads that accept `DateTimeOffset` or `TimeSpan` to change the expiration without re-submitting the values.
{% endhint %}
