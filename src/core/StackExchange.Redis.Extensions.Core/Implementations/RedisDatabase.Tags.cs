using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    public partial class RedisDatabase : IRedisDatabase
    {
        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetByTag<T>(string tag)
        {
            var tagKey = GetTagKey(tag, typeof(T));

            var memberKeys = await SetMembersAsync<string>(tagKey);

            return (await GetAllAsync<T>(memberKeys)).Values;
        }
    }
}
