using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    /// <summary>
    /// The Redis Database
    /// </summary>
    public partial interface IRedisDatabase
    {
        /// <summary>
        ///     Lists the add to left asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None) where T : class;

        /// <summary>
        ///     Removes and returns the last element of the list stored at key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns></returns>
        /// <remarks>
        ///     http://redis.io/commands/rpop
        /// </remarks>
        Task<T> ListGetFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None) where T : class;

        /// <summary>
        ///     Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
    }
}
