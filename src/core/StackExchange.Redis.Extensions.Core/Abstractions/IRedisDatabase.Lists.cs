using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Lists the add to left asynchronous.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
        where T : class;

    /// <summary>
    ///     Lists the add to left asynchronous.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="items">The items.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    Task<long> ListAddToLeftAsync<T>(string key, T[] items, CommandFlags flag = CommandFlags.None)
        where T : class;

    /// <summary>
    ///     Removes and returns the last element of the list stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <remarks>
    ///     http://redis.io/commands/rpop
    /// </remarks>
    Task<T> ListGetFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        where T : class;
}
