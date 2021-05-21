using Jil;

using StackExchange.Redis.Extensions.Jil;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers
{
    public class JilSerializerTest : CacheClientTestBase
    {
        public JilSerializerTest()
            : base(new JilSerializer(new Options(
                prettyPrint: false,
                excludeNulls: true,
                jsonp: false,
                dateFormat: DateTimeFormat.ISO8601,
                includeInherited: true,
                unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsLocal)))
        {
        }
    }
}
