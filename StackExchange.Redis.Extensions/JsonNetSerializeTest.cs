using StackExchange.Redis.Extensions.Newtonsoft;

namespace StackExchange.Redis.Extensions.Tests
{
	public class JsonNetSerializeTest : CacheClientTestBase
	{
		public JsonNetSerializeTest()
			: base(new NewtonsoftSerializer())
		{

		}
	}
}