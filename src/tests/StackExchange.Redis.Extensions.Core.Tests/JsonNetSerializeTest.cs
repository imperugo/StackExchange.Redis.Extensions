using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis.Extensions.Newtonsoft;
using Xunit;

namespace StackExchange.Redis.Extensions.Tests
{
    public class JsonNetSerializeTest : CacheClientTestBase
    {
        public JsonNetSerializeTest()
            : base(new NewtonsoftSerializer())
        {
        }
    }
}
