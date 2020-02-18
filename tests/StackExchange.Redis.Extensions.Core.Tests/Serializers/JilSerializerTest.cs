using StackExchange.Redis.Extensions.Jil;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers
{
    public class JilSerializerTest : CacheClientTestBase
    {
        public JilSerializerTest()
            : base(new JilSerializer())
        {
        }
    }
}
