using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Implementations;
using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests
{
    public abstract partial class CacheClientTestBase
    {
        [Theory]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Add_Tagged_Item_To_Redis_Database(bool keyExists)
        {
            var tags = new HashSet<string> { $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}" };
            var key = Guid.NewGuid().ToString("N");
            var testValue = Guid.NewGuid().ToString("N");

            if (keyExists)
            {
                await Sut.GetDbFromConfiguration().AddAsync(key, "initial vlaue", tags: tags);
            }

            var added = await Sut.GetDbFromConfiguration().AddAsync(key, testValue, tags: tags);
            var redisValue = await db.KeyExistsAsync(key);

            Assert.True(added);
            Assert.True(redisValue);

            foreach (var tag in tags)
            {
                var values = await Sut.GetDbFromConfiguration().GetByTag<string>(tag);

                Assert.Contains(testValue, values);
            }
        }

        [Theory]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [InlineData(When.Exists, true)]
        [InlineData(When.Exists, false)]
        public async Task Add_Tagged_Item_With_Condition_Exists_To_Redis_Database(When condition, bool valueExists)
        {
            var tags = Enumerable.Range(0, 3)
                .Select(_ => GetRandomString())
                .ToHashSet();
            var key = GetRandomString();
            var initialValue = GetRandomString();

            if (valueExists)
            {
                Assert.True(await Sut.GetDbFromConfiguration().AddAsync(key, initialValue, When.Always), "Test precondition to add key");
            }

            var added = await Sut.GetDbFromConfiguration()
                .AddAsync(key, "my value", condition, tags: tags);

            var keyExists = await db.KeyExistsAsync(key);

            Assert.True(valueExists ? added : !added);
            Assert.True(valueExists ? keyExists : !keyExists);

            foreach (var tag in tags)
            {
                var resualt = await Sut.GetDbFromConfiguration().GetByTag<string>(tag);

                Assert.True(added ? resualt.Any() : !resualt.Any());
            }
        }

        [Theory]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [InlineData(When.NotExists, true)]
        [InlineData(When.NotExists, false)]
        public async Task Add_Tagged_Item_With_Condition_NotExists_To_Redis_Database(When condition, bool valueExists)
        {
            var tags = Enumerable.Range(0, 3)
                .Select(_ => GetRandomString())
                .ToHashSet();
            var key = GetRandomString();
            var initialValue = GetRandomString();

            var client = Sut.GetDbFromConfiguration();

            if (valueExists)
            {
                Assert.True(await client.AddAsync(key, initialValue, When.Always), "Test precondition to add key");
            }

            var added = await client.AddAsync(key, "my value", condition, tags: tags);

            var keyExists = await db.KeyExistsAsync(key);

            Assert.True(valueExists ? !added : added);
            Assert.True(keyExists);

            foreach (var tag in tags)
            {
                var result = await Sut.GetDbFromConfiguration().GetByTag<string>(tag);
                Assert.True(added ? result.Any() : !result.Any());
            }
        }

        [Fact]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [Trait("Category", "get by tag")]
        public async Task Add_Multiple_Tagged_Items_To_Redis_Database_Should_GetByTag()
        {
            var tags = new HashSet<string> { GetRandomString() };
            var testValue = GetRandomString();

            var expectedItems = Enumerable.Range(0, 10).Select(x => new TestClass
            {
                IntValue = x,
                StringValue = GetRandomString(),
                BoolValue = x % 2 == 0
            }).ToArray();

            foreach (var expectedItem in expectedItems)
            {
                var key = GetRandomString();
                var added = await Sut.GetDbFromConfiguration().AddAsync(key, expectedItem, tags: tags);
                var redisValue = await db.KeyExistsAsync(key);
                Assert.True(added);
                Assert.True(redisValue);
            }

            var actualItems = (await Sut.GetDbFromConfiguration().GetByTag<TestClass>(tags.Single())).ToArray();
            Assert.Equal(expectedItems.Count(), actualItems.Count());

            // TODO: why xunit and not nunit?
            foreach (var expectedItem in expectedItems)
            {
                Assert.Contains(expectedItem, actualItems);
            }
        }

        private static string GetRandomString()
            => $"{Guid.NewGuid():N}";

        [Serializable]
        public class TestClass
        {
            public TestClass()
            {
            }

            public TestClass(int intValue, string stringValue, bool boolValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                BoolValue = boolValue;
            }

            [DataMember(Name = "intValue", EmitDefaultValue = false)]
            public int IntValue { get; set; }

            [DataMember(Name = "stringValue", EmitDefaultValue = false)]
#pragma warning disable CA2235 // Mark all non-serializable fields: string is serializeable by attribute
            public string StringValue { get; set; }
#pragma warning restore CA2235 // Mark all non-serializable fields

            [DataMember(Name = "boolValue", EmitDefaultValue = false)]
            public bool BoolValue { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as TestClass);
            }

            public bool Equals(TestClass other)
            {
                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                return other.BoolValue == this.BoolValue
                    && other.IntValue == this.IntValue
                    && other.StringValue == this.StringValue;
            }

            public override int GetHashCode() => base.GetHashCode()
                    ^ this.BoolValue.GetHashCode()
                    ^ this.IntValue.GetHashCode()
                    ^ this.StringValue.GetHashCode();
        }
    }
}
