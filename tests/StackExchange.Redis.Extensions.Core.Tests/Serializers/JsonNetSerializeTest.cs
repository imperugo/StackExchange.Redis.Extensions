using StackExchange.Redis.Extensions.Newtonsoft;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers
{
    public class JsonNetSerializeTest : CacheClientTestBase
    {
        public JsonNetSerializeTest()
            : base(new NewtonsoftSerializer())
        {
        }
    }
}
