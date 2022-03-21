# Usage

There are multiple api that the library offers, here a list:

* [Add, retrieve and remove complex object](add-and-retrieve-complex-object-to-redis.md);
* [Replace an object](replace-an-object.md);
* ****[Custom Serialization](custom-serializer.md);
* **Search Keys into Redis**;
* [Work with multiple items;](work-with-multiple-items.md)
* **Store multiple object with a single roundtrip**;
* **Get Redis Server information**;
* **Set Add**;
* **Set AddAdd**;
* **SetRemove**;
* **SetRemoveAll**;
* **Set Member**;
* **Pub/Sub events**;
* **Save**;
* **Async methods**;
* **Hash methods**;
* **Support for Keyspace isolation**;
* **Support for multiple database**:

Note that all the example are based on a c# object like this:

```javascript
public class User
{
    public UserDocument()
    {
        Company = new CompanyDocument();
    }

    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public CultureInfo Culture { get; set; }
    public string TimeZoneId { get; set; }
    public bool EmailConfirmed { get; set; }
    public Company Company { get; set; }
}

public class Company
{
    public string Name { get; set; }
    public string Vat { get; set; }
    public string Address { get; set; }
    public string District { get; set; }
    public string Zipcode { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string Country { get; set; }
}
```

{% hint style="info" %}
&#x20;Your class could be everything is serializable.
{% endhint %}
