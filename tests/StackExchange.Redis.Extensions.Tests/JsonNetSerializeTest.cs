namespace StackExchange.Redis.Extensions.Tests
{
    public class JsonNetSerializeTest : CacheClientTestBase
    {
        public override void OnInitialize()
        {
            Serializer = new Newtonsoft.JsonSerializer();
        }
    }
}