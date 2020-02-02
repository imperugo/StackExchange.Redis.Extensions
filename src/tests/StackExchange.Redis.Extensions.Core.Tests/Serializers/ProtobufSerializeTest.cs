using StackExchange.Redis.Extensions.Protobuf;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers
{
    public class ProtobufSerializeTest : CacheClientTestBase
    {
        public ProtobufSerializeTest()
            : base(new ProtobufSerializer())
        {
        }
    }
}
