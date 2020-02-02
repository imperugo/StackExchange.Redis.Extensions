using StackExchange.Redis.Extensions.Binary;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers
{
    public class BinarySerializeTest : CacheClientTestBase
    {
        public BinarySerializeTest()
            : base(new BinarySerializer())
        {
        }
    }
}
