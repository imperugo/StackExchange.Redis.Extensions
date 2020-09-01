using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Newtonsoft;

using Xunit;

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
