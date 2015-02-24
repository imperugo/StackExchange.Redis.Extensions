using System.Configuration;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
	public class RedisHostCollection : ConfigurationElementCollection
	{
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

		protected override ConfigurationElement CreateNewElement()
		{
			return new RedisHost();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((RedisHost)element).Host;
		}
	}
}