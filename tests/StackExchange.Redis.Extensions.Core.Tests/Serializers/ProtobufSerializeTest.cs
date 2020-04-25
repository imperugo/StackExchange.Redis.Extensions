using ProtoBuf.Meta;
using StackExchange.Redis.Extensions.Protobuf;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers
{
    public class ProtobufSerializeTest : CacheClientTestBase
    {
        public ProtobufSerializeTest()
            : base(new ProtobufSerializer())
        {
            if (!RuntimeTypeModel.Default.IsDefined(typeof(TestClass)))
            {
                RuntimeTypeModel.Default
                    .Add(typeof(TestClass), false)
                        .Add("IntValue")
                        .Add("StringValue")
                        .Add("BoolValue")
                        .UseConstructor = true;
            }
        }
    }
}
