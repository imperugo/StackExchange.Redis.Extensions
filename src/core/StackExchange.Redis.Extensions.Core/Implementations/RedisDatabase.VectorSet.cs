// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> VectorSetAddAsync(string key, VectorSetAddRequest request, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetAddAsync(key, request, flag);

    /// <inheritdoc/>
    public Task<Lease<VectorSetSimilaritySearchResult>?> VectorSetSimilaritySearchAsync(string key, VectorSetSimilaritySearchRequest query, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetSimilaritySearchAsync(key, query, flag);

    /// <inheritdoc/>
    public Task<bool> VectorSetRemoveAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetRemoveAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<bool> VectorSetContainsAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetContainsAsync(key, member, flag);

    /// <inheritdoc/>
#pragma warning disable RCS1174 // async/await required for cross-TFM int→long cast
    public async Task<long> VectorSetLengthAsync(string key, CommandFlags flag = CommandFlags.None) =>
        await Database.VectorSetLengthAsync(key, flag).ConfigureAwait(false);
#pragma warning restore RCS1174

    /// <inheritdoc/>
#pragma warning disable RCS1174
    public async Task<long> VectorSetDimensionAsync(string key, CommandFlags flag = CommandFlags.None) =>
        await Database.VectorSetDimensionAsync(key, flag).ConfigureAwait(false);
#pragma warning restore RCS1174

    /// <inheritdoc/>
    public Task<string?> VectorSetGetAttributesJsonAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetGetAttributesJsonAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<bool> VectorSetSetAttributesJsonAsync(string key, string member, string attributesJson, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetSetAttributesJsonAsync(key, member, attributesJson, flag);

    /// <inheritdoc/>
    public Task<VectorSetInfo?> VectorSetInfoAsync(string key, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetInfoAsync(key, flag);

    /// <inheritdoc/>
    public Task<RedisValue> VectorSetRandomMemberAsync(string key, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetRandomMemberAsync(key, flag);

    /// <inheritdoc/>
    public Task<RedisValue[]> VectorSetRandomMembersAsync(string key, long count, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetRandomMembersAsync(key, count, flag);

    /// <inheritdoc/>
    public Task<Lease<float>?> VectorSetGetApproximateVectorAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetGetApproximateVectorAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<Lease<RedisValue>?> VectorSetGetLinksAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetGetLinksAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<Lease<VectorSetLink>?> VectorSetGetLinksWithScoresAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.VectorSetGetLinksWithScoresAsync(key, member, flag);
}
