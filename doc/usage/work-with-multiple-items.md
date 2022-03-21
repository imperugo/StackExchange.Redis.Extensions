# Work with multiple items

If you need to add, remove or retrieve more than one item you don't have to send multiple requests into Redis. This library offers few methods that allow you to use a single command in order to do that increasing the performances because there is only one roundtrip from your app to the Redis server.

You instance:

```csharp
var user1 = new User()
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

var user2 = new User()
{
	Username = "mario.rossi",
	Firstname = "Mario",
	Lastname = "Rossi",
	Twitter = "@mariorossi"
	Blog = "http://imperugo.tostring.it/",
	Company = new Company 
	{
		Name = "My Super Company",
		Vat = "IT12345678911",
		Address = "somewhere road 12"
	}
}
```

Add multiple items:

```csharp
var items = new List<Tuple<string, User>>();
items.Add(new Tuple<string, User>("key1", user1));
items.Add(new Tuple<string, User>("key2", user2));

bool added = await cacheClient
                .Db0
                .AddAllAsync(items, DateTimeOffset.Now.AddMinutes(10));
```

Remove multiple items:

```csharp
var numberOfItemRemoved = await cacheClient
                                .Db0
                                .RemoveAllAsync(new []{"key1","key2"});
```

Retrieve multiple items:

```csharp
var users = await cacheClient
                                .Db0
                                .GetAllAsync<User>(new []{"key1","key2"});
```

{% hint style="info" %}
Note that is possible to change the expires time of the items also when you are retrieving them. In fact the method GetAllAsync offers two overloads that accept DateTimeOffset or TimeSpan in order to change the expiration time of the items without submit them again.
{% endhint %}
