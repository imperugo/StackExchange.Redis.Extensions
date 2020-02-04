using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Tests.Helpers;
using Xunit;
using static System.Linq.Enumerable;

namespace StackExchange.Redis.Extensions.Core.Tests
{
    public abstract partial class CacheClientTestBase
    {
        [Fact]
        public async Task HashSetSingleValueNX_ValueDoesntExists_ShouldInsertAndRetrieveValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>("test", DateTime.UtcNow);

            // act
            var res = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, entryValue, nx: true);

            // assert
            Assert.True(res);

            var redisValue = await db.HashGetAsync(hashKey, entryKey);
            var data = serializer.Deserialize<TestClass<DateTime>>(redisValue);

            Assert.Equal(entryValue, data);
        }

        [Fact]
        public async Task HashSetSingleValueNX_ValueExists_ShouldNotInsertOriginalValueNotChanged()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
            var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
            var initRes = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, initialValue);

            // act
            var res = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, entryValue, nx: true);

            // assert
            Assert.True(initRes);
            Assert.False(res);
            var redisvalue = await db.HashGetAsync(hashKey, entryKey);
            var data = serializer.Deserialize<TestClass<DateTime>>(redisvalue);
            Assert.Equal(initialValue, data);
        }

        [Fact]
        public async Task HashSetSingleValue_ValueExists_ShouldUpdateValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
            var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
            var initRes = Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, serializer.Serialize(initialValue));

            // act
            var res = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, entryValue, nx: false);

            // assert
            Assert.True(initRes, "Initial value was not set");
            Assert.False(res); // NOTE: HSET returns: 1 if new field was created and value set, or 0 if field existed and value set. reference: http://redis.io/commands/HSET
            var data = serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
            Assert.Equal(entryValue, data);
        }

        [Fact]
        public async Task HashSetMultipleValues_HashGetMultipleValues_ShouldInsert()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values = Range(0, 100).Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow));
            var map = values.ToDictionary(val => Guid.NewGuid().ToString());

            // act
            await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, map);
            await Task.Delay(500);

            // assert
            var data = db
                        .HashGet(hashKey, map.Keys.Select(x => (RedisValue)x).ToArray()).ToList()
                        .Select(x => serializer.Deserialize<TestClass<DateTime>>(x))
                        .ToList();

            Assert.Equal(map.Count, data.Count());

            foreach (var val in data)
            {
                Assert.True(map.ContainsValue(val), $"result map doesn't contain value: {val}");
            }
        }

        [Fact]
        public async Task HashDelete_KeyExists_ShouldDelete()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);

            Assert.True(db.HashSet(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");

            // act
            var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, entryKey);

            // assert
            Assert.True(result);
            Assert.True((await db.HashGetAsync(hashKey, entryKey)).IsNull);
        }

        [Fact]
        public async Task HashDelete_KeyDoesntExist_ShouldReturnFalse()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();

            // act
            var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, entryKey);

            // assert
            Assert.False(result);
            Assert.True((await db.HashGetAsync(hashKey, entryKey)).IsNull);
        }

        [Fact]
        public async Task HashDeleteMultiple_AllKeysExist_ShouldDeleteAll()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values =
                Range(0, 1000)
                    .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, values.Keys);

            // assert
            Assert.Equal(values.Count, result);
            var dbValues = await db.HashGetAsync(hashKey, values.Select(x => (RedisValue)x.Key).ToArray());
            Assert.NotNull(dbValues);
            Assert.DoesNotContain(dbValues, x => !x.IsNull);
            Assert.Equal(0, await db.HashLengthAsync(hashKey));
        }

        [Fact]
        public async Task HashDeleteMultiple_NotAllKeysExist_ShouldDeleteAllOnlyRequested()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();

            var valuesDelete =
                Range(0, 1000)
                    .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                    .ToDictionary(x => x.Key);

            var valuesKeep =
                Range(0, 1000)
                    .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, valuesDelete.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
            await db.HashSetAsync(hashKey, valuesKeep.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, valuesDelete.Keys);

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
            Assert.All(dbValues, x => Assert.True(valuesKeep.ContainsKey(Sut.GetDbFromConfiguration().Serializer.Deserialize<TestClass<int>>(x).Key)));
        }

        [Fact]
        public async Task HashExists_KeyExists_ReturnTrue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
            Assert.True(await db.HashSetAsync(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");

            // act
            var result = await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey);

            // assert
            Assert.True(result, "Entry doesn't exist in hash, but it should");
        }

        [Fact]
        public async Task HashExists_KeyDoesntExists_ReturnFalse()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();

            // act
            var result = await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey);

            // assert
            Assert.False(result, "Entry doesn't exist in hash, but call returned true");
        }

        [Fact]
        public async Task HashKeys_HashEmpty_ReturnEmptyCollection()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();

            // act
            var result = await Sut.GetDbFromConfiguration().HashKeysAsync(hashKey);

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task HashKeys_HashNotEmpty_ReturnKeysCollection()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values =
                Range(0, 1000)
                    .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.GetDbFromConfiguration().HashKeysAsync(hashKey);

            // assert
            Assert.NotNull(result);
            var collection = result as IList<string> ?? result.ToList();
            Assert.NotEmpty(collection);
            Assert.Equal(values.Count, collection.Count());

            foreach (var key in collection)
            {
                Assert.True(values.ContainsKey(key));
            }
        }

        [Fact]
        public async Task HashValues_HashEmpty_ReturnEmptyCollection()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();

            // act
            var result = await Sut.GetDbFromConfiguration().HashValuesAsync<string>(hashKey);

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task HashValues_HashNotEmpty_ReturnAllValues()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values =
                Range(0, 1000)
                    .Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.GetDbFromConfiguration().HashValuesAsync<TestClass<DateTime>>(hashKey);

            // assert
            Assert.NotNull(result);
            var collection = result as IList<TestClass<DateTime>> ?? result.ToList();
            Assert.NotEmpty(collection);
            Assert.Equal(values.Count, collection.Count());

            foreach (var key in collection)
                Assert.Contains(key, values.Values);
        }

        [Fact]
        public async Task HashLength_HashEmpty_ReturnZero()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();

            // act
            var result = await Sut.GetDbFromConfiguration().HashLengthAsync(hashKey);

            // assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task HashLength_HashNotEmpty_ReturnCorrectCount()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values =
                Range(0, 1000)
                    .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.GetDbFromConfiguration().HashLengthAsync(hashKey);

            // assert
            Assert.Equal(1000, result);
        }

        [Fact]
        public async Task HashIncerementByLong_ValueDoesntExist_EntryCreatedWithValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var incBy = 1;

            // act
            Assert.False(db.HashExists(hashKey, entryKey));
            var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

            // assert
            Assert.Equal(incBy, result);
            Assert.True(await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey));
            Assert.Equal(incBy, db.HashGet(hashKey, entryKey));
        }

        [Fact]
        public async Task HashIncerementByLong_ValueExist_EntryIncrementedCorrectValueReturned()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = 15;
            var incBy = 1;

            Assert.True(db.HashSet(hashKey, entryKey, entryValue));

            // act
            var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

            // assert
            var expected = entryValue + incBy;
            Assert.Equal(expected, result);
            Assert.Equal(expected, await db.HashGetAsync(hashKey, entryKey));
        }

        [Fact]
        public async Task HashIncerementByDouble_ValueDoesntExist_EntryCreatedWithValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var incBy = 0.9;

            // act
            Assert.False(db.HashExists(hashKey, entryKey));
            var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

            // assert
            Assert.Equal(incBy, result);
            Assert.True(await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey));
            Assert.Equal(incBy, (double)await db.HashGetAsync(hashKey, entryKey), 6); // have to provide epsilon due to double error
        }

        [Fact]
        public async Task HashIncerementByDouble_ValueExist_EntryIncrementedCorrectValueReturned()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = 14.3;
            var incBy = 9.7;

            Assert.True(db.HashSet(hashKey, entryKey, entryValue));

            // act
            var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

            // assert
            var expected = entryValue + incBy;
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
            var result = Sut.GetDbFromConfiguration().HashScan<string>(hashKey, "*");

            // assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task HashScan_EntriesExistUseAstrisk_ReturnCursorToAllEntries()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values =
                Range(0, 1000)
                    .Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = Sut.GetDbFromConfiguration().HashScan<TestClass<DateTime>>(hashKey, "*");

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
        public async Task HashScan_EntriesExistUseAstrisk_ReturnCursorToAllEntriesBeginningWithTwo()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var values =
                Range(0, 1000)
                    .Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
                    .ToDictionary(x => x.Key);

            await db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = Sut.GetDbFromConfiguration().HashScan<TestClass<DateTime>>(hashKey, "2*");

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
}
