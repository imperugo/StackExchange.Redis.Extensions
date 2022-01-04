using StackExchange.Redis.Extensions.MsgPack;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers;

public class MessagePackSerializerTest : CacheClientTestBase
{
    public MessagePackSerializerTest()
        : base(new MsgPackObjectSerializer())
    {
    }
}
