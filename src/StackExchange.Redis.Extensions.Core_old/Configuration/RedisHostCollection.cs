using System.Configuration;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
	/// <summary>
	/// Configuration Element Collection for <see cref="RedisHost"/>
	/// </summary>
	public class RedisHostCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Gets or sets the <see cref="RedisHost"/> at the specified index.
		/// </summary>
		/// <value>
		/// The <see cref="RedisHost"/>.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public RedisHost this[int index]
		{
			get
			{
				return BaseGet(index) as RedisHost;
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}

				BaseAdd(index, value);
			}
		}

		/// <summary>
		/// Creates the new element.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new RedisHost();
		}

		/// <summary>
		/// Gets the element key.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element) 
            => $"{((RedisHost) element).Host}:{((RedisHost) element).CachePort}";
	}
}