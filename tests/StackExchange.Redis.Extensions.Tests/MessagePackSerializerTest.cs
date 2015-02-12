using StackExchange.Redis.Extensions.MsgPack;

namespace StackExchange.Redis.Extensions.Tests
{
    public class MessagePackSerializerTest : CacheClientTestBase
    {
        public MessagePackSerializerTest()
            : base(new MsgPackObjectSerializer())
        {

        }
    }
}