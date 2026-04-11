# Add, retrieve and remove complex object

All the examples below assume you have `IRedisDatabase` injected via dependency injection:

```csharp
public class MyService(IRedisDatabase redis)
{
    // use redis directly
}
```

### Create your instance

```csharp
var user = new User
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
```

### Add object to Redis

```csharp
bool added = await redis.AddAsync("my-cache-key", user, TimeSpan.FromMinutes(10));
```

### Retrieve the object

```csharp
var userFromCache = await redis.GetAsync<User>("my-cache-key");
```

{% hint style="info" %}
It is possible to update the expiry time of the item when retrieving it. The `GetAsync` method offers overloads that accept `DateTimeOffset` or `TimeSpan` to change the expiration without re-submitting the value.
{% endhint %}

### Remove the object

```csharp
bool removed = await redis.RemoveAsync("my-cache-key");
```
