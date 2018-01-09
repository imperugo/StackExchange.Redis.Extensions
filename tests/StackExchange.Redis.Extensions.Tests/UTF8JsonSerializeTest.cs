using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Utf8Json;

namespace StackExchange.Redis.Extensions.Tests
{
	public class UTF8JsonSerializeTest : CacheClientTestBase
	{
		public UTF8JsonSerializeTest()
			: base(new Utf8JsonSerializer())
		{

		}
	}
}