using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            const string key = "my Key";
            var added = await Sut.GetDbFromConfiguration().AddAsync(key, "my value", tags: tags);
            var redisValue = await db.KeyExistsAsync(key);

            Assert.True(added);
            Assert.True(redisValue);

            foreach (var tag in tags)
            {
                var tagExists = await Sut.GetDbFromConfiguration().ExistsAsync(CreateTagKey(tag));
                Assert.True(tagExists);

                var taggedValues = await Sut.GetDbFromConfiguration().SetMembersAsync<string>(CreateTagKey(tag));
                Assert.Contains(key, taggedValues);
            }

            string CreateTagKey(string t) => $"tag:{t}";
        }
    }
}
