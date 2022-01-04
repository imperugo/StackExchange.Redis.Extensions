using System;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Tests.Helpers;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task Add_Item_To_Sorted_Set()
    {
        var testobject = new TestClass<DateTime>();

        var added = await Sut.GetDbFromConfiguration().SortedSetAddAsync("my Key", testobject, 0).ConfigureAwait(false);

        var result = db.SortedSetScan("my Key").First();

        Assert.True(added);

        var obj = serializer.Deserialize<TestClass<DateTime>>(result.Element);

        Assert.NotNull(obj);
        Assert.Equal(testobject.Key, obj.Key);
        Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
    }

    [Fact]
    public async Task Add_More_Items_To_Sorted_Set_In_Order()
    {
        var utcNow = DateTime.UtcNow;
        var entryValueFirst = new TestClass<DateTime>("test_first", utcNow);
        var entryValueLast = new TestClass<DateTime>("test_last", utcNow);

        await Sut.GetDbFromConfiguration().SortedSetAddAsync("my Key", entryValueFirst, 1).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().SortedSetAddAsync("my Key", entryValueLast, 2).ConfigureAwait(false);

        var result = db.SortedSetScan("my Key").ToList();

        Assert.NotNull(result);

        var dataFirst = serializer.Deserialize<TestClass<DateTime>>(result[0].Element);
        var dataLast = serializer.Deserialize<TestClass<DateTime>>(result[1].Element);

        Assert.Equal(entryValueFirst.Value, dataFirst.Value);
        Assert.Equal(entryValueLast.Value, dataLast.Value);
    }

    [Fact]
    public async Task Remove_Item_From_Sorted_Set()
    {
        var testobject = new TestClass<DateTime>();

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject), 0).ConfigureAwait(false);

        var removed = await Sut.GetDbFromConfiguration().SortedSetRemoveAsync("my Key", testobject).ConfigureAwait(false);

        Assert.True(removed);

        Assert.Empty(db.SortedSetScan("my Key"));
    }

    [Fact]
    public async Task Return_items_ordered_by_rank()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testobject4 = new TestClass<DateTime>("test_4", DateTime.UtcNow);
        var testobject5 = new TestClass<DateTime>("test_5", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 1).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 3).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject4), 4).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject5), 5).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByRankWithScoresAsync<TestClass<DateTime>>("my Key", 0, 2).ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0].Element, testobject1);
        Assert.Equal(descendingList[1].Element, testobject2);
        Assert.Equal(descendingList[2].Element, testobject3);

        Assert.Equal(1, descendingList[0].Score);
        Assert.Equal(2, descendingList[1].Score);
        Assert.Equal(3, descendingList[2].Score);

        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_by_score()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 1).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 3).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key").ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0], testobject1);
        Assert.Equal(descendingList[1], testobject2);
        Assert.Equal(descendingList[2], testobject3);
        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_by_rank_descent()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testobject4 = new TestClass<DateTime>("test_4", DateTime.UtcNow);
        var testobject5 = new TestClass<DateTime>("test_5", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 1).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 3).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject4), 4).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject5), 5).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByRankWithScoresAsync<TestClass<DateTime>>("my Key", 0, 2, Order.Descending).ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0].Element, testobject5);
        Assert.Equal(descendingList[1].Element, testobject4);
        Assert.Equal(descendingList[2].Element, testobject3);

        Assert.Equal(5, descendingList[0].Score);
        Assert.Equal(4, descendingList[1].Score);
        Assert.Equal(3, descendingList[2].Score);

        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_ascended()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 3).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 1).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", order: Order.Ascending).ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0], testobject3);
        Assert.Equal(descendingList[1], testobject2);
        Assert.Equal(descendingList[2], testobject1);
        Assert.Equal(3, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_by_specific_score()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testobject4 = new TestClass<DateTime>("test_4", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 1).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 3).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject4), 4).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", 1, 2).ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0], testobject1);
        Assert.Equal(descendingList[1], testobject2);
        Assert.Equal(2, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_and_skipping_and_taking()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testobject4 = new TestClass<DateTime>("test_4", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 1).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 3).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject4), 4).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", skip: 1, take: 2).ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0], testobject2);
        Assert.Equal(descendingList[1], testobject3);
        Assert.Equal(2, descendingList.Count);
    }

    [Fact]
    public async Task Return_items_ordered_with_exclude()
    {
        var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
        var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
        var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
        var testobject4 = new TestClass<DateTime>("test_4", DateTime.UtcNow);

        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject1), 1).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject2), 2).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject3), 3).ConfigureAwait(false);
        await db.SortedSetAddAsync("my Key", serializer.Serialize(testobject4), 4).ConfigureAwait(false);

        var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", 1, 4, Exclude.Both).ConfigureAwait(false)).ToList();

        Assert.Equal(descendingList[0], testobject2);
        Assert.Equal(descendingList[1], testobject3);
        Assert.Equal(2, descendingList.Count);
    }

    [Fact]
    public async Task Add_IncrementItemt_To_Sorted_Set()
    {
        var testobject = new TestClass<DateTime>();
        const int defaultscore = 1;
        const int nextscore = 2;
        var added = await Sut.GetDbFromConfiguration().SortedSetAddIncrementAsync("my Key", testobject, defaultscore).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().SortedSetAddIncrementAsync("my Key", testobject, nextscore).ConfigureAwait(false);
        var result = db.SortedSetScan("my Key").First();

        Assert.Equal(defaultscore, added);
        Assert.Equal(defaultscore + nextscore, result.Score);
        var obj = serializer.Deserialize<TestClass<DateTime>>(result.Element);

        Assert.NotNull(obj);
        Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
    }
}
