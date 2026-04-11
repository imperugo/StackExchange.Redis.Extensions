// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database Geo extensions
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Add the specified member to the geospatial index stored at key.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="longitude">The longitude of the geo entry.</param>
    /// <param name="latitude">The latitude of the geo entry.</param>
    /// <param name="member">The name to assign to this entry.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the member was added, false if it already existed and was updated.</returns>
    Task<bool> GeoAddAsync(string key, double longitude, double latitude, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Add one geo entry to the geospatial index stored at key.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="value">The geo entry to store.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the member was added, false if it already existed and was updated.</returns>
    Task<bool> GeoAddAsync(string key, GeoEntry value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Add multiple geo entries to the geospatial index stored at key.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="values">The geo entries to add.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of elements added (not including updates to existing members).</returns>
    Task<long> GeoAddAsync(string key, GeoEntry[] values, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes the specified member from the geospatial index stored at key.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="member">The member to remove.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the member existed and was removed, otherwise false.</returns>
    Task<bool> GeoRemoveAsync(string key, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the distance between two members in the geospatial index.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="member1">The first member.</param>
    /// <param name="member2">The second member.</param>
    /// <param name="unit">The unit of distance to return.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The distance in the specified unit, or null if either member is missing.</returns>
    Task<double?> GeoDistanceAsync(string key, string member1, string member2, GeoUnit unit = GeoUnit.Meters, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the geohash string of a single member in the geospatial index.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="member">The member to retrieve.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The geohash string, or null if the member does not exist.</returns>
    Task<string?> GeoHashAsync(string key, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the geohash strings for multiple members in the geospatial index.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="members">The members to retrieve.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>An array of geohash strings, with null entries for missing members.</returns>
    Task<string?[]> GeoHashAsync(string key, string[] members, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the longitude and latitude of a single member in the geospatial index.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="member">The member to retrieve.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The position, or null if the member does not exist.</returns>
    Task<GeoPosition?> GeoPositionAsync(string key, string member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the longitude and latitude of multiple members in the geospatial index.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="members">The members to retrieve.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>An array of positions, with null entries for missing members.</returns>
    Task<GeoPosition?[]> GeoPositionAsync(string key, string[] members, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns members within the specified radius of a given member.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="member">The member to use as the center point.</param>
    /// <param name="radius">The radius to search within.</param>
    /// <param name="unit">The unit of the radius.</param>
    /// <param name="count">The maximum number of results, -1 for unlimited.</param>
    /// <param name="order">The order in which to return results.</param>
    /// <param name="options">Options controlling which data to include in results.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The matching results.</returns>
    Task<GeoRadiusResult[]> GeoRadiusAsync(string key, string member, double radius, GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns members within the specified radius of a given longitude/latitude coordinate.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="longitude">The longitude of the center point.</param>
    /// <param name="latitude">The latitude of the center point.</param>
    /// <param name="radius">The radius to search within.</param>
    /// <param name="unit">The unit of the radius.</param>
    /// <param name="count">The maximum number of results, -1 for unlimited.</param>
    /// <param name="order">The order in which to return results.</param>
    /// <param name="options">Options controlling which data to include in results.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The matching results.</returns>
    Task<GeoRadiusResult[]> GeoRadiusAsync(string key, double longitude, double latitude, double radius, GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns members of the geospatial index bounded by the provided shape, centered on a given member.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="member">The set member to use as the center of the shape.</param>
    /// <param name="shape">The shape bounding the search (use GeoSearchCircle or GeoSearchBox).</param>
    /// <param name="count">The maximum number of results, -1 for unlimited.</param>
    /// <param name="demandClosest">When true, terminates early once count results are found.</param>
    /// <param name="order">The order in which to return results.</param>
    /// <param name="options">Options controlling which data to include in results.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The matching results.</returns>
    Task<GeoRadiusResult[]> GeoSearchAsync(string key, string member, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns members of the geospatial index bounded by the provided shape, centered on a given coordinate.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="longitude">The longitude of the center point.</param>
    /// <param name="latitude">The latitude of the center point.</param>
    /// <param name="shape">The shape bounding the search (use GeoSearchCircle or GeoSearchBox).</param>
    /// <param name="count">The maximum number of results, -1 for unlimited.</param>
    /// <param name="demandClosest">When true, terminates early once count results are found.</param>
    /// <param name="order">The order in which to return results.</param>
    /// <param name="options">Options controlling which data to include in results.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The matching results.</returns>
    Task<GeoRadiusResult[]> GeoSearchAsync(string key, double longitude, double latitude, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Searches the geospatial index and stores the results in a destination key, centered on a given member.
    /// </summary>
    /// <param name="sourceKey">The key of the source set.</param>
    /// <param name="destinationKey">The key to store the results at.</param>
    /// <param name="member">The set member to use as the center of the shape.</param>
    /// <param name="shape">The shape bounding the search.</param>
    /// <param name="count">The maximum number of results, -1 for unlimited.</param>
    /// <param name="demandClosest">When true, terminates early once count results are found.</param>
    /// <param name="order">The order in which to store results.</param>
    /// <param name="storeDistances">When true, the destination is a plain sorted set of distances rather than a geo-encoded set.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of elements stored in the destination key.</returns>
    Task<long> GeoSearchAndStoreAsync(string sourceKey, string destinationKey, string member, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, bool storeDistances = false, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Searches the geospatial index and stores the results in a destination key, centered on a given coordinate.
    /// </summary>
    /// <param name="sourceKey">The key of the source set.</param>
    /// <param name="destinationKey">The key to store the results at.</param>
    /// <param name="longitude">The longitude of the center point.</param>
    /// <param name="latitude">The latitude of the center point.</param>
    /// <param name="shape">The shape bounding the search.</param>
    /// <param name="count">The maximum number of results, -1 for unlimited.</param>
    /// <param name="demandClosest">When true, terminates early once count results are found.</param>
    /// <param name="order">The order in which to store results.</param>
    /// <param name="storeDistances">When true, the destination is a plain sorted set of distances rather than a geo-encoded set.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of elements stored in the destination key.</returns>
    Task<long> GeoSearchAndStoreAsync(string sourceKey, string destinationKey, double longitude, double latitude, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, bool storeDistances = false, CommandFlags flag = CommandFlags.None);
}
