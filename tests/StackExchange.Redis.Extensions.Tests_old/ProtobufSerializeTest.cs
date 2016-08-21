using StackExchange.Redis.Extensions.Protobuf;

namespace StackExchange.Redis.Extensions.Tests
{
	public class ProtobufSerializeTest : CacheClientTestBase
	{
		public ProtobufSerializeTest()
			: base(new ProtobufSerializer())
		{

		}
	}
}