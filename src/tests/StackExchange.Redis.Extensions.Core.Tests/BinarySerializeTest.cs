using StackExchange.Redis.Extensions.Binary;

namespace StackExchange.Redis.Extensions.Tests
{
    public class BinarySerializeTest : CacheClientTestBase
    {
        public BinarySerializeTest()
            : base(new BinarySerializer())
        {
        }
    }
}
