// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> SortedSetAddAsync<T>(
        string key,
        T value,
        double score,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(value);

        return Database.SortedSetAddAsync(key, entryBytes, score, flag);
    }

    /// <inheritdoc/>
    public Task<bool> SortedSetRemoveAsync<T>(
        string key,
        T value,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(value);

        return Database.SortedSetRemoveAsync(key, entryBytes, flag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> SortedSetRangeByScoreAsync<T>(
        string key,
        double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity,
        Exclude exclude = Exclude.None,
        Order order = Order.Ascending,
        long skip = 0L,
        long take = -1L,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await Database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, flag).ConfigureAwait(false);

        return result.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m!));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ScoreRankResult<T>>> SortedSetRangeByRankWithScoresAsync<T>(
        string key,
        long start = 0L,
        long stop = -1L,
        Order order = Order.Ascending,
        CommandFlags commandFlags = CommandFlags.None)
    {
        var result = await Database.SortedSetRangeByRankWithScoresAsync(key, start, stop, order, commandFlags).ConfigureAwait(false);

        var list = new ScoreRankResult<T>[result.Length];

        for (var i = 0; i < result.Length; i++)
        {
            var x = result[i];
            list[i] = new ScoreRankResult<T>(Serializer.Deserialize<T>(x.Element!), x.Score);
        }

        return list;
    }
}
