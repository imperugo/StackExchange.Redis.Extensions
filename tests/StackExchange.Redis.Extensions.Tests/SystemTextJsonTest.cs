using StackExchange.Redis.Extensions.System.Text.Json;

namespace StackExchange.Redis.Extensions.Tests
{
    public class SystemTextJsonTest : CacheClientTestBase
	{
		public SystemTextJsonTest()
			: base(new SystemTextJsonSerializer())
		{

		}
	}
}