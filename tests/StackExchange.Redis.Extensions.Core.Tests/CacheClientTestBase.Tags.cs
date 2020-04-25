using System;
using System.Collections.Generic;
using System.Linq;
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
                var tagKey = RedisDatabase.GetTagKey(tag, typeof(string));
                var tagExists = await Sut.GetDbFromConfiguration().ExistsAsync(tagKey);
                Assert.True(tagExists);

                var taggedValues = await Sut.GetDbFromConfiguration().SetMembersAsync<string>(tagKey);
                Assert.Contains(key, taggedValues);
            }
        }

        [Theory]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [InlineData(When.Exists, true)]
        [InlineData(When.Exists, false)]
        public async Task Add_Tagged_Item_With_Condition_Exists_To_Redis_Database(When condition, bool valueExists)
        {
            var tags = new HashSet<string> { $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}" };
            var key = Guid.NewGuid().ToString("N");
            var initialValue = Guid.NewGuid().ToString("N");

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
                var tagKey = RedisDatabase.GetTagKey(tag, typeof(string));
                var tagExists = await db.KeyExistsAsync(tagKey);
                Assert.True(added ? tagExists : !tagExists);
            }
        }

        [Theory]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [InlineData(When.NotExists, true)]
        [InlineData(When.NotExists, false)]
        public async Task Add_Tagged_Item_With_Condition_NotExists_To_Redis_Database(When condition, bool valueExists)
        {
            var tags = new HashSet<string> { $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}" };
            var key = Guid.NewGuid().ToString("N");
            var initialValue = Guid.NewGuid().ToString("N");

            if (valueExists)
            {
                Assert.True(await Sut.GetDbFromConfiguration().AddAsync(key, initialValue, When.Always), "Test precondition to add key");
            }

            var added = await Sut.GetDbFromConfiguration()
                .AddAsync(key, "my value", condition, tags: tags);

            var redisValue = await db.KeyExistsAsync(key);

            Assert.True(valueExists ? !added : added);
            Assert.True(redisValue);

            foreach (var tag in tags)
            {
                var tagKey = RedisDatabase.GetTagKey(tag, typeof(string));
                var tagExists = await db.KeyExistsAsync(tagKey);
                Assert.True(added ? tagExists : !tagExists);
            }
        }

        [Fact]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        [Trait("Category", "get by tag")]
        public async Task Add_Multiple_Tagged_Items_To_Redis_Database_Should_GetByTag()
        {
            var tags = new HashSet<string> { $"{Guid.NewGuid():N}" };
            var testValue = $"{Guid.NewGuid():N}";

            var expectedItems = Enumerable.Range(0, 10).Select(x => new TestClass
            {
                IntValue = x,
                StringValue = $"{Guid.NewGuid():N}",
                BoolValue = x % 2 == 0
            }).ToArray();

            foreach (var expectedItem in expectedItems)
            {
                var key = $"{Guid.NewGuid():N}";
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

            public int IntValue { get; set; }

            public string StringValue { get; set; }

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
