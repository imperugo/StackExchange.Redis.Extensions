using StackExchange.Redis.Extensions.Jil;

namespace StackExchange.Redis.Extensions.Tests
{
    public class JilSerializerTest : CacheClientTestBase
	{
        public JilSerializerTest()
            : base(new JilSerializer())
		{
		}
	}
}
