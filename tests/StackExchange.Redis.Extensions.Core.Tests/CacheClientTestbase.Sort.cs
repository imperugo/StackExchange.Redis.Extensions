// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task Add_Item_To_Sorted_Set_Async()
    {
        var testClass = new Helpers.TestClass<DateTime>() { Value = DateTime.Now };

        var added = await Sut.GetDefaultDatabase().SortedSetAddAsync("my Key", testClass, 0);

        SortedSetEntry? result = null;

        await foreach (var r in db.SortedSetScanAsync("my Key"))
        {
            result = r;
            break;
        }

        Assert.True(added);

        var obj = serializer.Deserialize<Helpers.TestClass<DateTime>>(result!.Value.Element!);

        Assert.NotNull(obj);
        Assert.Equal(testClass.Key, obj.Key);
        Assert.Equal(testClass.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
    }

    [Fact]
    public async Task Add_More_Items_To_Sorted_Set_In_Order_Async()
    {
        var utcNow = DateTime.UtcNow;
        var entryValueFirst = new Helpers.TestClass<DateTime>("test_first", utcNow);
        var entryValueLast = new Helpers.TestClass<DateTime>("test_last", utcNow);

        await Sut.GetDefaultDatabase().SortedSetAddAsync("my Key", entryValueFirst, 1);
        await Sut.GetDefaultDatabase().SortedSetAddAsync("my Key", entryValueLast, 2);

        List<SortedSetEntry> results = new();

        await foreach (var r in db.SortedSetScanAsync("my Key"))
            results.Add(r);

        Assert.NotEmpty(results);

        var dataFirst = serializer.Deserialize<Helpers.TestClass<DateTime>>(results[0].Element!);
        var dataLast = serializer.Deserialize<Helpers.TestClass<DateTime>>(results[1].Element!);

        Assert.Equal(entryValueFirst.Value, dataFirst!.Value);
        Assert.Equal(entryValueLast.Value, dataLast!.Value);
    }

    [Fact]
    public async Task Remove_Item_From_Sorted_Set_Async()
    {
        var testClass = new Helpers.TestClass<DateTime>();

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass), 0);

        var removed = await Sut.GetDefaultDatabase().SortedSetRemoveAsync("my Key", testClass);

        Assert.True(removed);

        List<SortedSetEntry> results = new();

        await foreach (var r in db.SortedSetScanAsync("my Key"))
            results.Add(r);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Return_items_ordered_by_rank_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testClass4 = new Helpers.TestClass<DateTime>("test_4", DateTime.UtcNow);
        var testClass5 = new Helpers.TestClass<DateTime>("test_5", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 1);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 3);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass4), 4);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass5), 5);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByRankWithScoresAsync<Helpers.TestClass<DateTime>>("my Key", 0, 2)).ToList();

        Assert.Equal(descendingList[0].Element, testClass1);
        Assert.Equal(descendingList[1].Element, testClass2);
        Assert.Equal(descendingList[2].Element, testClass3);

        Assert.Equal(1, descendingList[0].Score);
        Assert.Equal(2, descendingList[1].Score);
        Assert.Equal(3, descendingList[2].Score);

        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_by_score_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 1);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 3);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByScoreAsync<Helpers.TestClass<DateTime>>("my Key")).ToList();

        Assert.Equal(descendingList[0], testClass1);
        Assert.Equal(descendingList[1], testClass2);
        Assert.Equal(descendingList[2], testClass3);
        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_by_rank_descent_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testClass4 = new Helpers.TestClass<DateTime>("test_4", DateTime.UtcNow);
        var testClass5 = new Helpers.TestClass<DateTime>("test_5", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 1);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 3);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass4), 4);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass5), 5);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByRankWithScoresAsync<Helpers.TestClass<DateTime>>("my Key", 0, 2, Order.Descending)).ToList();

        Assert.Equal(descendingList[0].Element, testClass5);
        Assert.Equal(descendingList[1].Element, testClass4);
        Assert.Equal(descendingList[2].Element, testClass3);

        Assert.Equal(5, descendingList[0].Score);
        Assert.Equal(4, descendingList[1].Score);
        Assert.Equal(3, descendingList[2].Score);

        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_ascended_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 3);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 1);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByScoreAsync<Helpers.TestClass<DateTime>>("my Key", order: Order.Ascending)).ToList();

        Assert.Equal(descendingList[0], testClass3);
        Assert.Equal(descendingList[1], testClass2);
        Assert.Equal(descendingList[2], testClass1);
        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_by_specific_score_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testClass4 = new Helpers.TestClass<DateTime>("test_4", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 1);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 3);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass4), 4);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByScoreAsync<Helpers.TestClass<DateTime>>("my Key", 1, 2)).ToList();

        Assert.Equal(descendingList[0], testClass1);
        Assert.Equal(descendingList[1], testClass2);
        Assert.Equal(2, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_and_skipping_and_taking_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testClass4 = new Helpers.TestClass<DateTime>("test_4", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 1);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 3);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass4), 4);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByScoreAsync<Helpers.TestClass<DateTime>>("my Key", skip: 1, take: 2)).ToList();

        Assert.Equal(descendingList[0], testClass2);
        Assert.Equal(descendingList[1], testClass3);
        Assert.Equal(2, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_with_exclude_Async()
    {
        var testClass1 = new Helpers.TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testClass2 = new Helpers.TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testClass3 = new Helpers.TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testClass4 = new Helpers.TestClass<DateTime>("test_4", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass1), 1);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass2), 2);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass3), 3);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testClass4), 4);

        var descendingList = (await Sut.GetDefaultDatabase().SortedSetRangeByScoreAsync<Helpers.TestClass<DateTime>>("my Key", 1, 4, Exclude.Both)).ToList();

        Assert.Equal(descendingList[0], testClass2);
        Assert.Equal(descendingList[1], testClass3);
        Assert.Equal(2, descendingList.Count);
    }

    [Fact]
    public async Task Add_IncrementItem_To_Sorted_Set_Async()
    {
        var testClass = new Helpers.TestClass<DateTime>();

        const int defaultScore = 1;
        const int nextScore = 2;
        var added = await Sut.GetDefaultDatabase().SortedSetAddIncrementAsync("my Key", testClass, defaultScore);
        await Sut.GetDefaultDatabase().SortedSetAddIncrementAsync("my Key", testClass, nextScore);
        SortedSetEntry? result = null;

        await foreach (var r in db.SortedSetScanAsync("my Key"))
        {
            result = r;
            break;
        }

        Assert.Equal(defaultScore, added);
        Assert.Equal(defaultScore + nextScore, result!.Value.Score);
        var obj = serializer.Deserialize<Helpers.TestClass<DateTime>>(result.Value.Element!);

        Assert.NotNull(obj);
        Assert.Equal(testClass.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
    }
}
