using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Add the entry to a sorted set with a score
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(1)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="score">Score of the entry</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    Task<bool> SortedSetAddAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Remove the entry to a sorted set
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(1)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been removed. Otherwise false
    /// </returns>
    Task<bool> SortedSetRemoveAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None) where T : class;

    /// <summary>
    ///     Get entries from sorted-set ordered
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(log(N)+M) with N being the number of elements in the sorted set and M the number of elements being returned. If M is constant (e.g. always asking for the first 10 elements with LIMIT), you can consider it O(log(N)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="start">Min score</param>
    /// <param name="stop">Max score</param>
    /// <param name="exclude">Exclude start / stop</param>
    /// <param name="order">Order of sorted set</param>
    /// <param name="skip">Skip count</param>
    /// <param name="take">Take count</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been removed. Otherwise false
    /// </returns>
    Task<IEnumerable<T?>> SortedSetRangeByScoreAsync<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0L, long take = -1L, CommandFlags flag = CommandFlags.None) where T : class;

    /// <summary>
    ///     Get entries from sorted-set ordered by rank
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(log(N)+M) with N being the number of elements in the sorted set and M the number of elements being returned. If M is constant (e.g. always asking for the first 10 elements with LIMIT), you can consider it O(log(N)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="start">Min score</param>
    /// <param name="stop">Max score</param>
    /// <param name="order">Order of sorted set</param>
    /// <param name="commandFlags">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been removed. Otherwise false
    /// </returns>
    Task<IEnumerable<ScoreRankResult<T>>> SortedSetRangeByRankWithScoresAsync<T>(string key, long start = 0L, long stop = -1L, Order order = Order.Ascending, CommandFlags commandFlags = CommandFlags.None) where T : class;

    /// <summary>
    ///     Add the entry to a sorted set with  an increment score
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(1)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="score">Score of the entry</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///      if the object has been added return previous score. Otherwise return 0.0 when first add
    /// </returns>
    Task<double> SortedSetAddIncrementAsync<T>(string key, T? value, double score, CommandFlags flag = CommandFlags.None) where T : class;
}
