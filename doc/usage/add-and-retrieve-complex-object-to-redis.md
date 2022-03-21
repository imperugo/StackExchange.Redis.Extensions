# Add, retrieve and remove complex object

Create your instance:

```csharp
var user = new User()
{
	Username = "imperugo",
	Firstname = "Ugo",
	Lastname = "Lattanzi",
	Twitter = "@imperugo"
	Blog = "http://tostring.it",
	Company = new Company 
	{
		Name = "My Super Company",
		Vat = "IT12345678911",
		Address = "somewhere road 12"
	}
}
```

Add object to Redis

```csharp
bool added = await cacheClient.Db0.AddAsync("my cache key", user, DateTimeOffset.Now.AddMinutes(10));
```

Retrieve the object:

```csharp
var userFromCache = await cacheClient.Db0.GetAsync<User>("my cache key");
```

{% hint style="info" %}
Note that is possible to change the expires time of the item also when you are retrieving it. In fact the method GetAsync offers two overloads that accept DateTimeOffset or TimeSpan in order to change the expiration time of the item without submit it again.
{% endhint %}

Remove and object

```csharp
bool removed = await cacheClient.Db0.RemoveAsync<User>("my cache key");
```

