using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    public partial interface IRedisDatabase
    {
        Task<IEnumerable<T>> GetByTagAsync<T>(string tag, CommandFlags commandFlags = CommandFlags.None);

        Task<long> RemoveByTagAsync<T>(string tag, CommandFlags flags = CommandFlags.None);
    }
}
