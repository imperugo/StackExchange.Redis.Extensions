# Custom serializer

If you want to use a serializer that is not available with this library you have two options:

* Send a PR adding your serializer (this is absolutely the best option)
* Create a class that implement `ISerializer` and use it

The ISerializer interface looks like this:

```csharp
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core
{
	/// <summary>
	/// Contract for Serializer implementation
	/// </summary>
	public interface ISerializer
	{
		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		byte[] Serialize(object item);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		object Deserialize(byte[] serializedObject);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		T Deserialize<T>(byte[] serializedObject);
	}
```

and it is available on the main package StackExchange.Redis.Extensions.Core.

After that you can register you implementation directly in your favorite dependency injection framewor (see [here](../dependency-injection.md))
