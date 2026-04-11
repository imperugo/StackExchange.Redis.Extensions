# Replace an object

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

### Replace the object

Replacing an object is straightforward:

```csharp
bool replaced = await redis.ReplaceAsync("my-cache-key", user, TimeSpan.FromMinutes(10));
```
