// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> GeoAddAsync(string key, double longitude, double latitude, string member, CommandFlags flag = CommandFlags.None) =>
        Database.GeoAddAsync(key, longitude, latitude, member, flag);

    /// <inheritdoc/>
    public Task<bool> GeoAddAsync(string key, GeoEntry value, CommandFlags flag = CommandFlags.None) =>
        Database.GeoAddAsync(key, value, flag);

    /// <inheritdoc/>
    public Task<long> GeoAddAsync(string key, GeoEntry[] values, CommandFlags flag = CommandFlags.None) =>
        Database.GeoAddAsync(key, values, flag);

    /// <inheritdoc/>
    public Task<bool> GeoRemoveAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.GeoRemoveAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<double?> GeoDistanceAsync(string key, string member1, string member2, GeoUnit unit = GeoUnit.Meters, CommandFlags flag = CommandFlags.None) =>
        Database.GeoDistanceAsync(key, member1, member2, unit, flag);

    /// <inheritdoc/>
    public Task<string?> GeoHashAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.GeoHashAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<string?[]> GeoHashAsync(string key, string[] members, CommandFlags flag = CommandFlags.None) =>
        Database.GeoHashAsync(key, members.ToFastArray(m => (RedisValue)m), flag);

    /// <inheritdoc/>
    public Task<GeoPosition?> GeoPositionAsync(string key, string member, CommandFlags flag = CommandFlags.None) =>
        Database.GeoPositionAsync(key, member, flag);

    /// <inheritdoc/>
    public Task<GeoPosition?[]> GeoPositionAsync(string key, string[] members, CommandFlags flag = CommandFlags.None) =>
        Database.GeoPositionAsync(key, members.ToFastArray(m => (RedisValue)m), flag);

    /// <inheritdoc/>
    public Task<GeoRadiusResult[]> GeoRadiusAsync(string key, string member, double radius, GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None) =>
        Database.GeoRadiusAsync(key, member, radius, unit, count, order, options, flag);

    /// <inheritdoc/>
    public Task<GeoRadiusResult[]> GeoRadiusAsync(string key, double longitude, double latitude, double radius, GeoUnit unit = GeoUnit.Meters, int count = -1, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None) =>
        Database.GeoRadiusAsync(key, longitude, latitude, radius, unit, count, order, options, flag);

    /// <inheritdoc/>
    public Task<GeoRadiusResult[]> GeoSearchAsync(string key, string member, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None) =>
        Database.GeoSearchAsync(key, member, shape, count, demandClosest, order, options, flag);

    /// <inheritdoc/>
    public Task<GeoRadiusResult[]> GeoSearchAsync(string key, double longitude, double latitude, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, GeoRadiusOptions options = GeoRadiusOptions.Default, CommandFlags flag = CommandFlags.None) =>
        Database.GeoSearchAsync(key, longitude, latitude, shape, count, demandClosest, order, options, flag);

    /// <inheritdoc/>
    public Task<long> GeoSearchAndStoreAsync(string sourceKey, string destinationKey, string member, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, bool storeDistances = false, CommandFlags flag = CommandFlags.None) =>
        Database.GeoSearchAndStoreAsync(sourceKey, destinationKey, member, shape, count, demandClosest, order, storeDistances, flag);

    /// <inheritdoc/>
    public Task<long> GeoSearchAndStoreAsync(string sourceKey, string destinationKey, double longitude, double latitude, GeoSearchShape shape, int count = -1, bool demandClosest = true, Order? order = null, bool storeDistances = false, CommandFlags flag = CommandFlags.None) =>
        Database.GeoSearchAndStoreAsync(sourceKey, destinationKey, longitude, latitude, shape, count, demandClosest, order, storeDistances, flag);
}
