// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Tests.Helpers;

using Xunit;

using static System.Linq.Enumerable;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task HashSetSingleValueNX_ValueDoesntExists_ShouldInsertAndRetrieveValue_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        var entryValue = new TestClass<DateTime>("test", DateTime.UtcNow);

        // act
        var res = await Sut.GetDefaultDatabase().HashSetAsync(hashKey, entryKey, entryValue, true);

        // assert
        Assert.True(res);

        var redisValue = await db.HashGetAsync(hashKey, entryKey);
        var data = serializer.Deserialize<TestClass<DateTime>>(redisValue);

        Assert.Equal(entryValue, data);
    }

    [Fact]
    public async Task HashSetSingleValueNX_ValueExists_ShouldNotInsertOriginalValueNotChanged_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
        var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
        var initRes = await Sut.GetDefaultDatabase().HashSetAsync(hashKey, entryKey, initialValue);

        // act
        var res = await Sut.GetDefaultDatabase().HashSetAsync(hashKey, entryKey, entryValue, true);

        // assert
        Assert.True(initRes);
        Assert.False(res);
        var redisValue = await db.HashGetAsync(hashKey, entryKey);
        var data = serializer.Deserialize<TestClass<DateTime>>(redisValue);
        Assert.Equal(initialValue, data);
    }

    [Fact]
    public async Task HashSetSingleValue_ValueExists_ShouldUpdateValue_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
        var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
        var initRes = Sut.GetDefaultDatabase().Database.HashSet(hashKey, entryKey, serializer.Serialize(initialValue));

        // act
        var res = await Sut.GetDefaultDatabase().HashSetAsync(hashKey, entryKey, entryValue);

        // assert
        Assert.True(initRes, "Initial value was not set");
        Assert.False(res); // NOTE: HSET returns: 1 if new field was created and value set, or 0 if field existed and value set. reference: http://redis.io/commands/HSET
        var data = serializer.Deserialize<TestClass<DateTime>>(Sut.GetDefaultDatabase().Database.HashGet(hashKey, entryKey));
        Assert.Equal(entryValue, data);
    }

    [Fact]
    public async Task HashSetMultipleValues_HashGetMultipleValues_ShouldInsert_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values = Range(0, 100).Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow));
        var map = values.ToDictionary(_ => Guid.NewGuid().ToString());

        // act
        await Sut.GetDefaultDatabase().HashSetAsync(hashKey, map);
        await Task.Delay(500);

        // assert
        var data = db
            .HashGet(hashKey, map.Keys.Select(x => (RedisValue)x).ToArray())
            .ToList()
            .ConvertAll(x => serializer.Deserialize<TestClass<DateTime>>(x));

        Assert.Equal(map.Count, data.Count);

        foreach (var val in data)
            Assert.True(map.ContainsValue(val), $"result map doesn't contain value: {val}");
    }

    [Fact]
    public async Task HashDelete_KeyExists_ShouldDelete_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);

        Assert.True(db.HashSet(hashKey, entryKey, Sut.GetDefaultDatabase().Serializer.Serialize(entryValue)), "Failed setting test value into redis");

        // act
        var result = await Sut.GetDefaultDatabase().HashDeleteAsync(hashKey, entryKey);

        // assert
        Assert.True(result);
        Assert.True((await db.HashGetAsync(hashKey, entryKey)).IsNull);
    }

    [Fact]
    public async Task HashDelete_KeyDoesntExist_ShouldReturnFalse_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();

        // act
        var result = await Sut.GetDefaultDatabase().HashDeleteAsync(hashKey, entryKey);

        // assert
        Assert.False(result);
        Assert.True((await db.HashGetAsync(hashKey, entryKey)).IsNull);
    }

    [Fact]
    public async Task HashDeleteMultiple_AllKeysExist_ShouldDeleteAll_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values
            = Range(0, 1000)
                .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = await Sut.GetDefaultDatabase().HashDeleteAsync(hashKey, values.Keys.ToArray());

        // assert
        Assert.Equal(values.Count, result);
        var dbValues = await db.HashGetAsync(hashKey, values.Select(x => (RedisValue)x.Key).ToArray());
        Assert.NotNull(dbValues);
        Assert.DoesNotContain(dbValues, x => !x.IsNull);
        Assert.Equal(0, await db.HashLengthAsync(hashKey));
    }

    [Fact]
    public async Task HashDeleteMultiple_NotAllKeysExist_ShouldDeleteAllOnlyRequested_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();

        var valuesDelete
            = Range(0, 1000)
                .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                .ToDictionary(x => x.Key);

        var valuesKeep
            = Range(0, 1000)
                .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, valuesDelete.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());
        await db.HashSetAsync(hashKey, valuesKeep.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = await Sut.GetDefaultDatabase().HashDeleteAsync(hashKey, valuesDelete.Keys.ToArray());

        // assert
        Assert.Equal(valuesDelete.Count, result);
        var dbDeletedValues = await db.HashGetAsync(hashKey, valuesDelete.Select(x => (RedisValue)x.Key).ToArray());
        Assert.NotNull(dbDeletedValues);
        Assert.DoesNotContain(dbDeletedValues, x => !x.IsNull);
        var dbValues = await db.HashGetAsync(hashKey, valuesKeep.Select(x => (RedisValue)x.Key).ToArray());
        Assert.NotNull(dbValues);
        Assert.DoesNotContain(dbValues, x => x.IsNull);
        Assert.Equal(1000, await db.HashLengthAsync(hashKey));
        Assert.Equal(1000, dbValues.Length);
        Assert.All(dbValues, x => Assert.True(valuesKeep.ContainsKey(Sut.GetDefaultDatabase().Serializer.Deserialize<TestClass<int>>(x).Key)));
    }

    [Fact]
    public async Task HashExists_KeyExists_ReturnTrue_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
        Assert.True(await db.HashSetAsync(hashKey, entryKey, Sut.GetDefaultDatabase().Serializer.Serialize(entryValue)), "Failed setting test value into redis");

        // act
        var result = await Sut.GetDefaultDatabase().HashExistsAsync(hashKey, entryKey);

        // assert
        Assert.True(result, "Entry doesn't exist in hash, but it should");
    }

    [Fact]
    public async Task HashExists_KeyDoesntExists_ReturnFalse_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();

        // act
        var result = await Sut.GetDefaultDatabase().HashExistsAsync(hashKey, entryKey);

        // assert
        Assert.False(result, "Entry doesn't exist in hash, but call returned true");
    }

    [Fact]
    public async Task HashKeys_HashEmpty_ReturnEmptyCollection_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();

        // act
        var result = await Sut.GetDefaultDatabase().HashKeysAsync(hashKey);

        // assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HashKeys_HashNotEmpty_ReturnKeysCollection_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values
            = Range(0, 1000)
                .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = await Sut.GetDefaultDatabase().HashKeysAsync(hashKey);

        // assert
        Assert.NotNull(result);
        var collection = result as IList<string> ?? result.ToList();
        Assert.NotEmpty(collection);
        Assert.Equal(values.Count, collection.Count);

        foreach (var key in collection)
            Assert.True(values.ContainsKey(key));
    }

    [Fact]
    public async Task HashValues_HashEmpty_ReturnEmptyCollection_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();

        // act
        var result = await Sut.GetDefaultDatabase().HashValuesAsync<string>(hashKey);

        // assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HashValues_HashNotEmpty_ReturnAllValues_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values
            = Range(0, 1000)
                .Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = await Sut.GetDefaultDatabase().HashValuesAsync<TestClass<DateTime>>(hashKey);

        // assert
        Assert.NotNull(result);
        var collection = result as IList<TestClass<DateTime>> ?? result.ToList();
        Assert.NotEmpty(collection);
        Assert.Equal(values.Count, collection.Count);

        foreach (var key in collection)
            Assert.Contains(key, values.Values);
    }

    [Fact]
    public async Task HashLength_HashEmpty_ReturnZero_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();

        // act
        var result = await Sut.GetDefaultDatabase().HashLengthAsync(hashKey);

        // assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task HashLength_HashNotEmpty_ReturnCorrectCount_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values
            = Range(0, 1000)
                .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = await Sut.GetDefaultDatabase().HashLengthAsync(hashKey);

        // assert
        Assert.Equal(1000, result);
    }

    [Fact]
    public async Task HashIncrementByLong_ValueDoesntExist_EntryCreatedWithValue_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        const int incBy = 1;

        // act
        Assert.False(db.HashExists(hashKey, entryKey));
        var result = await Sut.GetDefaultDatabase().HashIncerementByAsync(hashKey, entryKey, incBy);

        // assert
        Assert.Equal(incBy, result);
        Assert.True(await Sut.GetDefaultDatabase().HashExistsAsync(hashKey, entryKey));
        Assert.Equal(incBy, db.HashGet(hashKey, entryKey));
    }

    [Fact]
    public async Task HashIncrementByLong_ValueExist_EntryIncrementedCorrectValueReturned_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        const int entryValue = 15;
        const int incBy = 1;

        Assert.True(db.HashSet(hashKey, entryKey, entryValue));

        // act
        var result = await Sut.GetDefaultDatabase().HashIncerementByAsync(hashKey, entryKey, incBy);

        // assert
        const int expected = entryValue + incBy;
        Assert.Equal(expected, result);
        Assert.Equal(expected, await db.HashGetAsync(hashKey, entryKey));
    }

    [Fact]
    public async Task HashIncrementByDouble_ValueDoesntExist_EntryCreatedWithValue_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        const double incBy = 0.9;

        // act
        Assert.False(db.HashExists(hashKey, entryKey));
        var result = await Sut.GetDefaultDatabase().HashIncerementByAsync(hashKey, entryKey, incBy);

        // assert
        Assert.Equal(incBy, result);
        Assert.True(await Sut.GetDefaultDatabase().HashExistsAsync(hashKey, entryKey));
        Assert.Equal(incBy, (double)await db.HashGetAsync(hashKey, entryKey), 6); // have to provide epsilon due to double error
    }

    [Fact]
    public async Task HashIncrementByDouble_ValueExist_EntryIncrementedCorrectValueReturned_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var entryKey = Guid.NewGuid().ToString();
        const double entryValue = 14.3;
        const double incBy = 9.7;

        Assert.True(db.HashSet(hashKey, entryKey, entryValue));

        // act
        var result = await Sut.GetDefaultDatabase().HashIncerementByAsync(hashKey, entryKey, incBy);

        // assert
        const double expected = entryValue + incBy;
        Assert.Equal(expected, result);
        Assert.Equal(expected, db.HashGet(hashKey, entryKey));
    }

    [Fact]
    public void HashScan_EmptyHash_ReturnEmptyCursor()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        Assert.True(db.HashLength(hashKey) == 0);

        // act
        var result = Sut.GetDefaultDatabase().HashScan<string>(hashKey, "*");

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HashScan_EntriesExistUseAsterisk_ReturnCursorToAllEntries_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values
            = Range(0, 1000)
                .Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = Sut.GetDefaultDatabase().HashScan<TestClass<DateTime>>(hashKey, "*");

        // assert
        Assert.NotNull(result);
        var resultEnum = result.ToDictionary(x => x.Key, x => x.Value);
        Assert.Equal(1000, resultEnum.Count);

        foreach (var key in values.Keys)
        {
            Assert.True(resultEnum.ContainsKey(key));
            Assert.Equal(values[key], resultEnum[key]);
        }
    }

    [Fact]
    public async Task HashScan_EntriesExistUseAsterisk_ReturnCursorToAllEntriesBeginningWithTwo_Async()
    {
        // arrange
        var hashKey = Guid.NewGuid().ToString();
        var values
            = Range(0, 1000)
                .Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
                .ToDictionary(x => x.Key);

        await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDefaultDatabase().Serializer.Serialize(x.Value))).ToArray());

        // act
        var result = Sut.GetDefaultDatabase().HashScan<TestClass<DateTime>>(hashKey, "2*");

        // assert
        Assert.NotNull(result);
        var resultEnum = result.ToDictionary(x => x.Key, x => x.Value);
        Assert.Equal(values.Keys.Count(x => x.StartsWith("2", StringComparison.Ordinal)), resultEnum.Count);

        foreach (var key in values.Keys.Where(x => x.StartsWith("2", StringComparison.Ordinal)))
        {
            Assert.True(resultEnum.ContainsKey(key));
            Assert.Equal(values[key], resultEnum[key]);
        }
    }
}
