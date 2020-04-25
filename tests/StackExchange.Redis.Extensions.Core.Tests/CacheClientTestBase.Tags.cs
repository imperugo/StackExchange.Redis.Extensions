using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Implementations;
using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests
{
    public abstract partial class CacheClientTestBase
    {
        [Fact]
        [Trait("Category", "tags")]
        [Trait("Category", "simple")]
        public async Task Add_Tagged_Item_To_Redis_Database()
        {
            var tags = new HashSet<string> { $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}" };
            var key = Guid.NewGuid().ToString("N");

            var added = await Sut.GetDbFromConfiguration().AddAsync(key, "my value", tags: tags);
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
    }
}
