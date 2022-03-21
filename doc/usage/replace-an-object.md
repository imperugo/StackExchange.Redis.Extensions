# Replace an object

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

Replacing an object is pretty easy.

```csharp
bool added = await cacheClient.Db0.ReplaceAsync("my cache key", user, DateTimeOffset.Now.AddMinutes(10));
```

