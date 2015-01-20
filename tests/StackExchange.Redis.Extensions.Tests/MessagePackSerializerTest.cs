using StackExchange.Redis.Extensions.MsgPack;

namespace StackExchange.Redis.Extensions.Tests
{
    public class MessagePackSerializerTest : CacheClientTestBase
    {
        public override void OnInitialize()
        {
            Serializer = new MsgPackObjectSerializer();
        }
    }
}