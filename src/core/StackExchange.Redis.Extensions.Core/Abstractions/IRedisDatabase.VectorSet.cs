// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database VectorSet extensions for AI/ML similarity search.
/// Requires Redis 8.0+.
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Adds a vector to the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="request">The add request containing the member, vector, and optional attributes.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the member was added, false if it already existed and was updated.</returns>
    Task<bool> VectorSetAddAsync(string key, VectorSetAddRequest request, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Performs a similarity search against the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="query">The search request containing the query vector and parameters.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The matching results with scores. The returned Lease must be disposed after use.</returns>
    Task<Lease<VectorSetSimilaritySearchResult>?> VectorSetSimilaritySearchAsync(string key, VectorSetSimilaritySearchRequest query, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes a member from the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="member">The member to remove.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the member was removed, false if it did not exist.</returns>
    Task<bool> VectorSetRemoveAsync(string key, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Checks if a member exists in the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="member">The member to check.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the member exists.</returns>
    Task<bool> VectorSetContainsAsync(string key, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the number of members in the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The cardinality of the VectorSet, or 0 if the key does not exist.</returns>
    Task<long> VectorSetLengthAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the number of dimensions of vectors in the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of dimensions.</returns>
    Task<long> VectorSetDimensionAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Gets the JSON attributes associated with a member in the VectorSet.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="member">The member to retrieve attributes for.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The JSON attributes string, or null if the member has no attributes.</returns>
    Task<string?> VectorSetGetAttributesJsonAsync(string key, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Sets JSON attributes on a member in the VectorSet.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="member">The member to set attributes on.</param>
    /// <param name="attributesJson">The JSON attributes string.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the attributes were set.</returns>
    Task<bool> VectorSetSetAttributesJsonAsync(string key, string member, string attributesJson, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns information about the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>Information about the VectorSet.</returns>
    Task<VectorSetInfo?> VectorSetInfoAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns a random member from the VectorSet stored at key.
    /// </summary>
    /// <param name="key">The key of the VectorSet.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>A random member, or null if the VectorSet is empty.</returns>
    Task<RedisValue> VectorSetRandomMemberAsync(string key, CommandFlags flag = CommandFlags.None);
}
