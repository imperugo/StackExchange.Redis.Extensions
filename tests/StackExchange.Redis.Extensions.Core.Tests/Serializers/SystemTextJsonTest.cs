using StackExchange.Redis.Extensions.System.Text.Json;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers;

public class SystemTextJsonTest : CacheClientTestBase
{
    public SystemTextJsonTest()
        : base(new SystemTextJsonSerializer())
    {
    }
}
