// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task GeoAdd_SingleMember_ShouldAddAndRetrievePosition_Async()
    {
        var key = Guid.NewGuid().ToString();

        var added = await Sut.GetDefaultDatabase().GeoAddAsync(key, 13.361389, 38.115556, "Palermo");

        Assert.True(added);

        var position = await Sut.GetDefaultDatabase().GeoPositionAsync(key, "Palermo");

        Assert.NotNull(position);
        Assert.InRange(position.Value.Longitude, 13.36, 13.37);
        Assert.InRange(position.Value.Latitude, 38.11, 38.12);
    }

    [Fact]
    public async Task GeoAdd_MultipleMembers_ShouldAddAll_Async()
    {
        var key = Guid.NewGuid().ToString();

        var entries = new[]
        {
            new GeoEntry(13.361389, 38.115556, "Palermo"),
            new GeoEntry(15.087269, 37.502669, "Catania"),
            new GeoEntry(12.496366, 41.902782, "Roma"),
        };

        var count = await Sut.GetDefaultDatabase().GeoAddAsync(key, entries);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task GeoRemove_ExistingMember_ShouldRemove_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, 13.361389, 38.115556, "Palermo");

        var removed = await Sut.GetDefaultDatabase().GeoRemoveAsync(key, "Palermo");

        Assert.True(removed);

        var position = await Sut.GetDefaultDatabase().GeoPositionAsync(key, "Palermo");

        Assert.Null(position);
    }

    [Fact]
    public async Task GeoDistance_BetweenTwoMembers_ShouldReturnCorrectDistance_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, new[]
        {
            new GeoEntry(13.361389, 38.115556, "Palermo"),
            new GeoEntry(15.087269, 37.502669, "Catania"),
        });

        var distance = await Sut.GetDefaultDatabase().GeoDistanceAsync(key, "Palermo", "Catania", GeoUnit.Kilometers);

        Assert.NotNull(distance);
        Assert.InRange(distance.Value, 160, 170);
    }

    [Fact]
    public async Task GeoHash_SingleMember_ShouldReturnHash_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, 13.361389, 38.115556, "Palermo");

        var hash = await Sut.GetDefaultDatabase().GeoHashAsync(key, "Palermo");

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public async Task GeoHash_MultipleMembers_ShouldReturnHashes_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, new[]
        {
            new GeoEntry(13.361389, 38.115556, "Palermo"),
            new GeoEntry(15.087269, 37.502669, "Catania"),
        });

        var hashes = await Sut.GetDefaultDatabase().GeoHashAsync(key, new[] { "Palermo", "Catania" });

        Assert.Equal(2, hashes.Length);
        Assert.All(hashes, h => Assert.NotNull(h));
    }

    [Fact]
    public async Task GeoPosition_MultipleMembers_ShouldReturnPositions_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, new[]
        {
            new GeoEntry(13.361389, 38.115556, "Palermo"),
            new GeoEntry(15.087269, 37.502669, "Catania"),
        });

        var positions = await Sut.GetDefaultDatabase().GeoPositionAsync(key, new[] { "Palermo", "Catania" });

        Assert.Equal(2, positions.Length);
        Assert.All(positions, p => Assert.NotNull(p));
    }

    [Fact]
    public async Task GeoRadius_ByCoordinates_ShouldReturnNearbyMembers_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, new[]
        {
            new GeoEntry(13.361389, 38.115556, "Palermo"),
            new GeoEntry(15.087269, 37.502669, "Catania"),
            new GeoEntry(12.496366, 41.902782, "Roma"),
        });

        var results = await Sut.GetDefaultDatabase().GeoRadiusAsync(key, 13.361389, 38.115556, 200, GeoUnit.Kilometers);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Member == "Palermo");
        Assert.Contains(results, r => r.Member == "Catania");
        Assert.DoesNotContain(results, r => r.Member == "Roma");
    }

    [Fact]
    public async Task GeoSearch_ByMember_WithCircle_ShouldReturnResults_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().GeoAddAsync(key, new[]
        {
            new GeoEntry(13.361389, 38.115556, "Palermo"),
            new GeoEntry(15.087269, 37.502669, "Catania"),
            new GeoEntry(12.496366, 41.902782, "Roma"),
        });

        var results = await Sut.GetDefaultDatabase().GeoSearchAsync(
            key,
            "Palermo",
            new GeoSearchCircle(200, GeoUnit.Kilometers),
            count: 10,
            order: Order.Ascending);

        Assert.NotEmpty(results);
        Assert.Equal("Palermo", results[0].Member.ToString());
    }
}
