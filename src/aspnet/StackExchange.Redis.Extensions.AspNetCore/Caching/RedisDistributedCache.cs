// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.AspNetCore.Caching;

/// <summary>
/// An <see cref="IDistributedCache"/> implementation backed by StackExchange.Redis.Extensions.
/// Uses a Hash-based storage format compatible with Microsoft.Extensions.Caching.StackExchangeRedis.
/// </summary>
internal sealed class RedisDistributedCache : IDistributedCache
{
    private const long NotPresent = -1;

    private static readonly RedisValue DataField = "data";
    private static readonly RedisValue AbsoluteExpirationField = "absexp";
    private static readonly RedisValue SlidingExpirationField = "sldexp";

    private readonly IDatabase db;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDistributedCache"/> class.
    /// </summary>
    /// <param name="redisDatabase">The Redis database to use for caching.</param>
    public RedisDistributedCache(IRedisDatabase redisDatabase)
    {
        if (redisDatabase is null)
            throw new ArgumentNullException(nameof(redisDatabase));

        db = redisDatabase.Database;
    }

    /// <inheritdoc/>
    public byte[]? Get(string key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        return GetAndRefresh(key, getData: true);
    }

    /// <inheritdoc/>
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        token.ThrowIfCancellationRequested();

        return GetAndRefreshAsync(key, getData: true);
    }

    /// <inheritdoc/>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var absoluteExpiration = GetAbsoluteExpiration(options);
        var slidingTicks = options.SlidingExpiration?.Ticks ?? NotPresent;

        var fields = new HashEntry[]
        {
            new(DataField, value),
            new(AbsoluteExpirationField, absoluteExpiration?.Ticks ?? NotPresent),
            new(SlidingExpirationField, slidingTicks),
        };

        db.HashSet(key, fields);

        var expiry = GetExpirationTimeout(absoluteExpiration, options.SlidingExpiration);

        if (expiry.HasValue)
            db.KeyExpire(key, expiry.Value);
    }

    /// <inheritdoc/>
    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        token.ThrowIfCancellationRequested();

        var absoluteExpiration = GetAbsoluteExpiration(options);
        var slidingTicks = options.SlidingExpiration?.Ticks ?? NotPresent;

        var fields = new HashEntry[]
        {
            new(DataField, value),
            new(AbsoluteExpirationField, absoluteExpiration?.Ticks ?? NotPresent),
            new(SlidingExpirationField, slidingTicks),
        };

        await db.HashSetAsync(key, fields).ConfigureAwait(false);

        var expiry = GetExpirationTimeout(absoluteExpiration, options.SlidingExpiration);

        if (expiry.HasValue)
            await db.KeyExpireAsync(key, expiry.Value).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Remove(string key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        db.KeyDelete(key);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        token.ThrowIfCancellationRequested();

        await db.KeyDeleteAsync(key).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Refresh(string key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        GetAndRefresh(key, getData: false);
    }

    /// <inheritdoc/>
    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        token.ThrowIfCancellationRequested();

        await GetAndRefreshAsync(key, getData: false).ConfigureAwait(false);
    }

    private byte[]? GetAndRefresh(string key, bool getData)
    {
        RedisValue[] results;

        if (getData)
            results = db.HashGet(key, [DataField, AbsoluteExpirationField, SlidingExpirationField]);
        else
            results = db.HashGet(key, [AbsoluteExpirationField, SlidingExpirationField]);

        if (results[0].IsNull)
            return null;

        MapExpirationFields(results, getData, out var absExpTicks, out var sldExpTicks);
        RefreshExpiration(key, absExpTicks, sldExpTicks);

        return getData ? (byte[]?)results[0] : null;
    }

    private async Task<byte[]?> GetAndRefreshAsync(string key, bool getData)
    {
        RedisValue[] results;

        if (getData)
            results = await db.HashGetAsync(key, [DataField, AbsoluteExpirationField, SlidingExpirationField]).ConfigureAwait(false);
        else
            results = await db.HashGetAsync(key, [AbsoluteExpirationField, SlidingExpirationField]).ConfigureAwait(false);

        if (results[0].IsNull)
            return null;

        MapExpirationFields(results, getData, out var absExpTicks, out var sldExpTicks);
        await RefreshExpirationAsync(key, absExpTicks, sldExpTicks).ConfigureAwait(false);

        return getData ? (byte[]?)results[0] : null;
    }

    private static void MapExpirationFields(RedisValue[] results, bool getData, out long absExpTicks, out long sldExpTicks)
    {
        var offset = getData ? 1 : 0;
        absExpTicks = (long)results[offset];
        sldExpTicks = (long)results[offset + 1];
    }

    private void RefreshExpiration(string key, long absExpTicks, long sldExpTicks)
    {
        if (sldExpTicks == NotPresent)
            return;

        var expiry = CalculateNewExpiry(absExpTicks, sldExpTicks);

        if (expiry.HasValue)
            db.KeyExpire(key, expiry.Value);
    }

    private async Task RefreshExpirationAsync(string key, long absExpTicks, long sldExpTicks)
    {
        if (sldExpTicks == NotPresent)
            return;

        var expiry = CalculateNewExpiry(absExpTicks, sldExpTicks);

        if (expiry.HasValue)
            await db.KeyExpireAsync(key, expiry.Value).ConfigureAwait(false);
    }

    private static TimeSpan? CalculateNewExpiry(long absExpTicks, long sldExpTicks)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var sliding = TimeSpan.FromTicks(sldExpTicks);
        var newExpiration = utcNow.Add(sliding);

        if (absExpTicks != NotPresent)
        {
            var absoluteExpiration = new DateTimeOffset(absExpTicks, TimeSpan.Zero);

            if (absoluteExpiration <= utcNow)
                return null;

            if (newExpiration > absoluteExpiration)
                newExpiration = absoluteExpiration;
        }

        return newExpiration - utcNow;
    }

    private static DateTimeOffset? GetAbsoluteExpiration(DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            if (options.AbsoluteExpiration.Value <= DateTimeOffset.UtcNow)
                throw new ArgumentOutOfRangeException(nameof(options), options.AbsoluteExpiration.Value, "The absolute expiration value must be in the future.");

            return options.AbsoluteExpiration;
        }

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            return DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);

        return null;
    }

    private static TimeSpan? GetExpirationTimeout(DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
    {
        if (absoluteExpiration.HasValue && slidingExpiration.HasValue)
        {
            var remaining = absoluteExpiration.Value - DateTimeOffset.UtcNow;
            return remaining < slidingExpiration.Value ? remaining : slidingExpiration.Value;
        }

        if (absoluteExpiration.HasValue)
            return absoluteExpiration.Value - DateTimeOffset.UtcNow;

        if (slidingExpiration.HasValue)
            return slidingExpiration.Value;

        return null;
    }
}
