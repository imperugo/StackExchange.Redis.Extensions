// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Helpers;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.Tests.Helpers;
using StackExchange.Redis.Extensions.Tests.Extensions;

using Xunit;

using static System.Linq.Enumerable;

namespace StackExchange.Redis.Extensions.Core.Tests;

[Collection("Redis")]
public abstract partial class CacheClientTestBase : IDisposable
{
    private readonly RedisClient sut;
    private readonly IDatabase db;
    private readonly ISerializer serializer;
    private readonly RedisConnectionPoolManager connectionPoolManager;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    internal CacheClientTestBase(ISerializer serializer)
    {
        var redisConfiguration = RedisConfigurationForTest.CreateBasicConfig();
        redisConfiguration.ConnectionSelectionStrategy = ConnectionSelectionStrategy.LeastLoaded;

        var moqLogger = new Mock<ILogger<RedisConnectionPoolManager>>();

        this.serializer = serializer;
        connectionPoolManager = new RedisConnectionPoolManager(redisConfiguration, moqLogger.Object);
        sut = new RedisClient(connectionPoolManager, this.serializer, redisConfiguration);
        db = sut.GetDefaultDatabase().Database;
    }

    protected IRedisClient Sut => sut;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            // free managed resources
            db.FlushDatabase();
            db.Multiplexer.GetSubscriber().UnsubscribeAll();
            connectionPoolManager.Dispose();
        }

        // free native resources if there are any.
        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }

    [Fact]
    public async Task Info_Should_Return_Valid_Information_Async()
    {
        var response = await Sut
            .GetDefaultDatabase()
            .GetInfoAsync();

        Assert.NotNull(response);
        Assert.True(response.Count > 0);
        Assert.Equal("6379", response["tcp_port"]);
    }

    [Fact]
    public async Task Info_Category_Should_Return_Valid_Information_Async()
    {
        var response = await Sut
            .GetDefaultDatabase()
            .GetInfoCategorizedAsync();

        Assert.NotNull(response);
        Assert.NotEmpty(response);
        Assert.Equal("6379", response.Single(x => x.Key == "tcp_port").InfoValue);
    }

    [Fact]
    public async Task Add_Item_To_Redis_Database_Async()
    {
        var added = await Sut
            .GetDefaultDatabase()
            .AddAsync("my Key", "my value");

        var redisValue = await db.KeyExistsAsync("my Key");

        Assert.True(added);
        Assert.True(redisValue);
    }

    [Fact]
    public async Task Add_Value_Type_To_Redis_Database_Async()
    {
        var added = await Sut
            .GetDefaultDatabase()
            .AddAsync("my Key", true);

        var redisValue = await db.KeyExistsAsync("my Key");

        Assert.True(added);
        Assert.True(redisValue);
    }

    [Fact]
    public async Task Add_Complex_Item_To_Redis_Database_Async()
    {
        var testClass = new TestClass<DateTime>();

        var added = await Sut
            .GetDefaultDatabase()
            .AddAsync("my Key", testClass);

        var redisValue = await db.StringGetAsync("my Key");

        Assert.True(added);

        var obj = serializer.Deserialize<TestClass<DateTime>>(redisValue);

        Assert.True(await db.KeyExistsAsync("my Key"));
        Assert.NotNull(obj);
        Assert.Equal(testClass.Key, obj.Key);
        Assert.Equal(testClass.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
    }

    [Fact]
    public async Task Add_Multiple_Object_With_A_Single_Roundtrip_To_Redis_Must_Store_Data_Correctly_Into_Database_Async()
    {
        var values = new Tuple<string, string>[]
        {
            new("key1", "value1"),
            new("key2", "value2"),
            new("key3", "value3")
        };

        var added = await Sut
            .GetDefaultDatabase()
            .AddAllAsync(values);

        Assert.True(added);

        Assert.True(await db.KeyExistsAsync("key1"));
        Assert.True(await db.KeyExistsAsync("key2"));
        Assert.True(await db.KeyExistsAsync("key3"));

        Assert.Equal("value1", serializer.Deserialize<string>((await db.StringGetAsync("key1"))));
        Assert.Equal("value2", serializer.Deserialize<string>((await db.StringGetAsync("key2"))));
        Assert.Equal("value3", serializer.Deserialize<string>((await db.StringGetAsync("key3"))));
    }

    [Fact]
    public async Task Get_All_Should_Return_All_Database_Keys_Async()
    {
        var values = Range(0, 5)
            .Select(i => new TestClass<string>($"Key{i.ToString(CultureInfo.InvariantCulture)}", Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        const string notExistingKey = "notexistingkey";

        var keys = new HashSet<string>
        {
            values[0].Key!,
            values[1].Key!,
            values[2].Key!,
            notExistingKey
        };

        var result = await Sut
            .GetDefaultDatabase()
            .GetAllAsync<string>(keys);

        Assert.Equal(4, result.Count);
        Assert.Equal(result[values[0].Key!], values[0].Value);
        Assert.Equal(result[values[1].Key!], values[1].Value);
        Assert.Equal(result[values[2].Key!], values[2].Value);
        Assert.Null(result[notExistingKey]);
    }

    [Fact]
    public async Task Get_With_Value_Type_Should_Return_Correct_Value_Async()
    {
        var now = DateTime.UtcNow;
        await db.StringSetAsync("my key", serializer.Serialize(true));
        await db.StringSetAsync("my key2", serializer.Serialize(now));

        var cachedObject = await Sut
            .GetDefaultDatabase()
            .GetAsync<bool>("my key");

        var cachedObject2 = await Sut
            .GetDefaultDatabase()
            .GetAsync<DateTime>("my key2");

        Assert.True(cachedObject);
        Assert.Equal(now, cachedObject2);
    }

    [Fact]
    public async Task Get_With_Complex_Item_Should_Return_Correct_Value_Async()
    {
        var value = Range(0, 1)
            .Select(i => new ComplexClassForTest<string, Guid>($"Key{i.ToString(CultureInfo.InvariantCulture)}", Guid.NewGuid()))
            .First();

        await db.StringSetAsync(value.Item1, serializer.Serialize(value));

        var cachedObject = await Sut
            .GetDefaultDatabase()
            .GetAsync<ComplexClassForTest<string, Guid>>(value.Item1!);

        Assert.NotNull(cachedObject);
        Assert.Equal(value.Item1, cachedObject.Item1);
        Assert.Equal(value.Item2, cachedObject.Item2);
    }

    [Fact]
    public async Task Remove_All_Should_Remove_All_Specified_Keys_Async()
    {
        var values = Range(1, 5)
            .Select(i => new TestClass<string>($"Key{i.ToString(CultureInfo.InvariantCulture)}", Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        await Sut
            .GetDefaultDatabase()
            .RemoveAllAsync(values.Select(x => x.Key).ToArray());

        foreach (var value in values)
            Assert.False(await db.KeyExistsAsync(value.Key));
    }

    [Fact]
    public async Task Search_With_Valid_Start_With_Pattern_Should_Return_Correct_Keys_Async()
    {
        var values = Range(1, 20)
            .Select(i => new TestClass<string>($"Key{i.ToString(CultureInfo.InvariantCulture)}", Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        var keys = await Sut
            .GetDefaultDatabase()
            .SearchKeysAsync("Key1*");

        Assert.Equal(11, keys.Count());
    }

    [Fact]
    public async Task SearchKeys_With_Key_Prefix_Should_Return_All_Database_Keys_Async()
    {
        var tsk1 = Sut.GetDefaultDatabase().AddAsync("myKey1", "Foo");
        var tsk2 = Sut.GetDefaultDatabase().AddAsync("myKey2", "Bar");
        var tsk3 = Sut.GetDefaultDatabase().AddAsync("key3", "Bar");

        await Task.WhenAll(tsk1, tsk2, tsk3);

        var keys = await Sut
            .GetDefaultDatabase()
            .SearchKeysAsync("*myKey*");

        Assert.Equal(2, keys.Count());
    }

    [Fact]
    public async Task SearchKeys_With_Start_Should_Return_All_Keys_Async()
    {
        var values = Range(0, 10)
            .Select(i => new TestClass<string>($"myKey{i.ToString(CultureInfo.InvariantCulture)}", Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        var keys = (await Sut
            .GetDefaultDatabase()
            .SearchKeysAsync("*"))
            .Order();

        Assert.Equal(10, keys.Count());
    }

    [Fact]
    public async Task SearchKeys_With_Key_Prefix_Should_Return_Keys_Without_Prefix_Async()
    {
        var values = Range(0, 10)
            .Select(i => new TestClass<string>($"myKey{i.ToString(CultureInfo.InvariantCulture)}", Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        var keys = (await Sut
            .GetDefaultDatabase()
            .SearchKeysAsync("*myKey*"))
            .Order()
            .ToList();

        Assert.Equal(10, keys.Count);

        for (var i = 0; i < keys.Count; i++)
            Assert.Equal(keys[i], values[i].Key);
    }

    [Fact]
    public async Task Exist_With_Valid_Object_Should_Return_The_Correct_Instance_Async()
    {
        var values = Range(0, 2)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        Assert.True(await Sut
            .GetDefaultDatabase()
            .ExistsAsync(values[0].Key!));
    }

    [Fact]
    public async Task Exist_With_Not_Valid_Object_Should_Return_The_Correct_Instance_Async()
    {
        var values = Range(0, 2)
            .Select(
                _ => new TestClass<string>(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.StringSetAsync(x.Key, serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        Assert.False(await Sut.GetDefaultDatabase().ExistsAsync("this key doesn not exist into redi"));
    }

    [Fact]
    public async Task SetAdd_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        foreach (var x in values)
        {
            await db.StringSetAsync(x.Key, serializer.Serialize(x.Value));
            await Sut.GetDefaultDatabase().SetAddAsync("MySet", x.Key);
        }

        var keys = await db.SetMembersAsync("MySet");

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task SetPop_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.SetAddAsync("MySet", serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        var result = await Sut.GetDefaultDatabase().SetPopAsync<string>("MySet");

        Assert.NotNull(result);
        Assert.Contains(values, v => v.Value == result);

        var members = await db.SetMembersAsync("MySet");
        var itemsLeft = members.Select(m => serializer.Deserialize<string>(m)).ToArray();

        Assert.Equal(4, itemsLeft.Length);
        Assert.DoesNotContain(itemsLeft, l => l == result);
    }

    [Fact]
    public async Task SetPop_With_A_Non_Existing_Key_Should_Return_Null_Async()
    {
        Assert.Null(await Sut.GetDefaultDatabase().SetPopAsync<string>("MySet"));
    }

    [Fact]
    public async Task SetPop_With_An_Empty_Key_Should_Throw_Exception_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetPopAsync<string>(string.Empty));
    }

    [Fact]
    public async Task SetPop_Count_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        var tasks = values.ToFastArray(x => db.SetAddAsync("MySet", serializer.Serialize(x.Value)));

        await Task.WhenAll(tasks);

        var result = (await Sut
            .GetDefaultDatabase()
            .SetPopAsync<string>("MySet", 3))
            .ToList();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        foreach (var r in result)
            Assert.Contains(values, v => v.Value == r);

        var members = await db.SetMembersAsync("MySet");

        var itemsLeft = members.ToFastArray(m => serializer.Deserialize<string>(m));

        Assert.Equal(2, itemsLeft.Length);

        foreach (var r in result)
            Assert.DoesNotContain(itemsLeft, l => l == r);
    }

    [Fact]
    public async Task SetPop_Count_With_A_Non_Existing_Key_Should_Return_Null_Async()
    {
        var result = await Sut.GetDefaultDatabase().SetPopAsync<string>("MySet");
        Assert.Null(result);
    }

    [Fact]
    public async Task SetPop_Count_With_An_Empty_Key_Should_Throw_Exception_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetPopAsync<string>(string.Empty));
    }

    [Fact]
    public async Task SetMembers_With_Valid_Data_Should_Return_Correct_Keys_Async()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        var tasks = values.ToFastArray(x => db.SetAddAsync("MySet", serializer.Serialize(x)));

        await Task.WhenAll(tasks);

        var keys = (await Sut
            .GetDefaultDatabase()
            .SetMembersAsync<TestClass<string>>("MySet"))
            .ToArray();

        Assert.Equal(keys.Length, values.Length);

        foreach (var key in keys)
            Assert.Contains(values, x => x.Key == key.Key && x.Value == key.Value);
    }

    [Fact]
    public async Task SetMember_With_Valid_Data_Should_Return_Correct_Keys_Async()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        foreach (var x in values)
        {
            await db.StringSetAsync(x.Key, serializer.Serialize(x.Value));
            await db.SetAddAsync("MySet", x.Key);
        }

        var keys = await Sut
            .GetDefaultDatabase()
            .SetMemberAsync("MySet");

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task SetMembers_With_Complex_Object_And_Valid_Data_Should_Return_Correct_Keys_Async()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        var tasks = values.ToFastArray(x => db.SetAddAsync("MySet", serializer.Serialize(x)));

        await Task.WhenAll(tasks);

        var keys = (await Sut
            .GetDefaultDatabase()
            .SetMembersAsync<TestClass<string>>("MySet"))
            .ToArray();

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task Massive_Add_Should_Not_Throw_Exception_And_Work_Correctly_Async()
    {
        const int size = 3000;
        var values = Range(0, size).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        var tupleValues = values.Select(x => new Tuple<string, TestClass<string>>(x.Key!, x)).ToArray();

        var result = await Sut.GetDefaultDatabase().AddAllAsync(tupleValues);
        var cached = await Sut.GetDefaultDatabase().GetAllAsync<TestClass<string>>(values.Select(x => x.Key!).ToHashSet());

        Assert.True(result);
        Assert.NotNull(cached);
        Assert.Equal(size, cached.Count);

        foreach (var value in values)
        {
            Assert.NotNull(value);
            Assert.NotNull(value.Key);
            Assert.Equal(value.Key, cached[value.Key]!.Key);
            Assert.Equal(value.Value, cached[value.Key]!.Value);
        }
    }

    [Fact]
    public async Task Massive_Add_With_Expiring_Should_Delete_Expired_Keys_Async()
    {
        var values = new Tuple<string, string>[]
        {
            new("ProductOneList1", "1"),
            new("ProductOneList2", "2"),
            new("ProductOneList3", "3"),
            new("ProductOneList4", "4"),
            new("ProductOneList5", "5"),
            new("ProductOneList6", "6"),
            new("ProductOneList7", "7"),
            new("ProductOneList8", "8"),
            new("ProductOneList9", "9")
        };

        await Sut
            .GetDefaultDatabase()
            .AddAllAsync(values, TimeSpan.FromMilliseconds(1));

        await Task.Delay(TimeSpan.FromMilliseconds(2));

        foreach (var value in values)
        {
            var exists = await Sut
                .GetDefaultDatabase()
                .ExistsAsync(value.Item1);

            Assert.False(exists, value.Item1);
        }
    }

    [Fact]
    public async Task Massive_Add_With_Expiring_And_Add_List_Again_Should_Work_Async()
    {
        // Issue 228
        // https://github.com/imperugo/StackExchange.Redis.Extensions/issues/288
        var valuesOneList = new Tuple<string, string>[]
        {
            new("ProductManyList1", "1"),
            new("ProductManyList2", "2"),
            new("ProductManyList3", "3"),
            new("ProductManyList4", "4"),
            new("ProductManyList5", "5"),
            new("ProductManyList6", "6"),
            new("ProductManyList7", "7"),
            new("ProductManyList8", "8"),
            new("ProductManyList9", "9")
        };

        await Sut.GetDefaultDatabase().AddAllAsync(valuesOneList, TimeSpan.FromMilliseconds(1));

        await Task.Delay(TimeSpan.FromMilliseconds(10));

        foreach (var value in valuesOneList)
        {
            var exists = await Sut
                .GetDefaultDatabase()
                .ExistsAsync(value.Item1);

            Assert.False(exists, value.Item1);
        }

        var valuesTwoLis = new Tuple<string, string>[]
        {
            new("ProductManyList10", "1"),
            new("ProductManyList11", "2"),
            new("ProductManyList12", "3"),
            new("ProductManyList13", "4"),
            new("ProductManyList14", "5"),
            new("ProductManyList15", "6"),
            new("ProductManyList16", "7"),
            new("ProductManyList17", "8"),
            new("ProductManyList18", "9")
        };

        await Sut.GetDefaultDatabase().AddAllAsync(valuesTwoLis, TimeSpan.FromMilliseconds(1));

        await Task.Delay(TimeSpan.FromMilliseconds(10));

        foreach (var value in valuesTwoLis)
        {
            var exists = await Sut.GetDefaultDatabase().ExistsAsync(value.Item1);
            Assert.False(exists, value.Item1);
        }
    }

    [Fact]
    public async Task Adding_Collection_To_Redis_Should_Work_Correctly_Async()
    {
        var items = Range(1, 3).Select(i => new TestClass<string> { Key = $"key{i.ToString(CultureInfo.InvariantCulture)}", Value = $"value{i.ToString(CultureInfo.InvariantCulture)}" }).ToArray();
        var added = await Sut.GetDefaultDatabase().AddAsync("my Key", items);
        var dbValue = await Sut.GetDefaultDatabase().GetAsync<TestClass<string>[]>("my Key");

        Assert.True(added);
        Assert.True(await db.KeyExistsAsync("my Key"));
        Assert.NotNull(dbValue);
        Assert.Equal(dbValue.Length, items.Length);

        for (var i = 0; i < items.Length; i++)
        {
            Assert.Equal(dbValue[i].Value, items[i].Value);
            Assert.Equal(dbValue[i].Key, items[i].Key);
        }
    }

    [Fact]
    public async Task Adding_Collection_To_Redis_Should_Expire_Async()
    {
        var expiresIn = new TimeSpan(0, 0, 1);
        var items = Range(1, 3).Select(i => new Tuple<string, string>($"key{i.ToString(CultureInfo.InvariantCulture)}", "value{i}")).ToArray();
        var added = await Sut.GetDefaultDatabase().AddAllAsync(items, expiresIn);

        await Task.Delay(expiresIn.Add(new(0, 0, 1)));
        var hasExpired = items.All(x => !db.KeyExists(x.Item1));

        Assert.True(added);
        Assert.True(hasExpired);
    }

    [Fact]
    public async Task Pub_Sub_Async()
    {
        var message = Range(0, 10).ToArray();
        var channel = new RedisChannel(Encoding.UTF8.GetBytes("unit_test"), RedisChannel.PatternMode.Auto);
        var subscriberNotified = false;
        IEnumerable<int>? subscriberValue = null;

        await Sut.GetDefaultDatabase().SubscribeAsync(channel, ((Func<IEnumerable<int>, Task>)ActionAsync)!);

        var result = await Sut.GetDefaultDatabase().PublishAsync(channel, message);

        while (!subscriberNotified)
            await Task.Delay(100);

        Assert.Equal(1, result);
        Assert.True(subscriberNotified);
        Assert.Equal(message, subscriberValue);

        return;

        Task ActionAsync(IEnumerable<int> value)
        {
            {
                subscriberNotified = true;
                subscriberValue = value;
            }

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SetAddGenericShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetAddAsync(string.Empty, string.Empty));
    }

    [Fact]
    public async Task SetAddGeneric_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

        foreach (var x in values)
        {
            await db.StringSetAsync(x.Key, serializer.Serialize(x.Value));
            await Sut.GetDefaultDatabase().SetAddAsync("MySet", x);
        }

        var keys = await db.SetMembersAsync("MySet");

        Assert.Equal(keys.Length, values.Count);
    }

    [Fact]
    public async Task SetAddAsyncGenericShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetAddAsync(string.Empty, string.Empty));
    }

    [Fact]
    public async Task SetContainsAsyncShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetContainsAsync(string.Empty, string.Empty));
    }

    [Fact]
    public async Task SetContainsAsyncShouldReturnTrue_Async()
    {
        const string key = "MySet";
        const string item = "MyItem";

        await Sut.GetDefaultDatabase().SetAddAsync(key, item);

        var result = await Sut.GetDefaultDatabase().SetContainsAsync(key, item);

        Assert.True(result);
    }

    [Fact]
    public async Task SetContainsAsyncShouldReturnFalseWhenItemIsWrong_Async()
    {
        const string key = "MySet";
        const string item = "MyItem";
        const string unknownItem = "MyUnknownItem";

        await Sut.GetDefaultDatabase().SetAddAsync(key, item);

        var result = await Sut.GetDefaultDatabase().SetContainsAsync(key, unknownItem);

        Assert.False(result);
    }

    [Fact]
    public async Task SetContainsAsyncShouldReturnFalseWhenKeyIsWrong_Async()
    {
        const string key = "MySet";
        const string item = "MyItem";
        const string unknownKey = "MyUnknownKey";

        await Sut.GetDefaultDatabase().SetAddAsync(key, item);

        var result = await Sut.GetDefaultDatabase().SetContainsAsync(unknownKey, item);

        Assert.False(result);
    }

    [Fact]
    public async Task SetAddAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetAddAllAsync("MySet", CommandFlags.None, "value", null, "value2"));
    }

    [Fact]
    public async Task SetRemoveGenericWithAnExistingItemShouldReturnTrue_Async()
    {
        const string key = "MySet", item = "MyItem";

        await Sut.GetDefaultDatabase().SetAddAsync(key, item);

        var result = await Sut.GetDefaultDatabase().SetRemoveAsync(key, item);
        Assert.True(result);
    }

    [Fact]
    public async Task SetRemoveGenericWithAnUnexistingItemShouldReturnFalse_Async()
    {
        const string key = "MySet";

        await Sut.GetDefaultDatabase().SetAddAsync(key, "ExistingItem");

        var result = await Sut.GetDefaultDatabase().SetRemoveAsync(key, "UnexistingItem");
        Assert.False(result);
    }

    [Fact]
    public async Task SetRemoveAsyncGenericWithAnExistingItemShouldReturnTrue_Async()
    {
        const string key = "MySet", item = "MyItem";

        await Sut.GetDefaultDatabase().SetAddAsync(key, item);

        var result = await Sut.GetDefaultDatabase().SetRemoveAsync(key, item);
        Assert.True(result);
    }

    [Fact]
    public async Task SetRemoveAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().SetRemoveAllAsync("MySet", CommandFlags.None, "value", null, "value2"));
    }

    [Fact]
    public async Task SetRemoveAllGenericWithAnExistingItemShouldReturnValidData_Async()
    {
        const string key = "MySet";
        var items = new[] { "MyItem1", "MyItem2" };

        await Sut.GetDefaultDatabase().SetAddAllAsync(key, CommandFlags.None, items);

        var result = await Sut.GetDefaultDatabase().SetRemoveAllAsync(key, CommandFlags.None, items);
        Assert.Equal(items.Length, result);
    }

    [Fact]
    public async Task ListAddToLeftGenericShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().ListAddToLeftAsync(string.Empty, string.Empty));
    }

    [Fact]
    public async Task ListAddToLeftArrayShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().ListAddToLeftAsync(string.Empty, items: Array.Empty<TestClass<string>>()));
    }

    [Fact]
    public async Task ListAddToLeftGeneric_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

        const string key = "MyList";

        foreach (var x in values)
            await Sut.GetDefaultDatabase().ListAddToLeftAsync(key, serializer.Serialize(x), When.Always);

        var keys = await db.ListRangeAsync(key);

        Assert.Equal(keys.Length, values.Count);
    }

    [Fact]
    public async Task ListAddToLeftArray_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5000).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        const string key = "MyList";

        await Sut.GetDefaultDatabase().ListAddToLeftAsync(key, items: values);

        var keys = await db.ListRangeAsync(key);

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().ListAddToLeftAsync(string.Empty, string.Empty));
    }

    [Fact]
    public async Task ListAddToLeftAsyncArrayShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().ListAddToLeftAsync(string.Empty, items: Array.Empty<TestClass<string>>()));
    }

    [Fact]
    public async Task ListAddToLeftAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

        const string key = "MyListAsync";

        foreach (var value in values)
        {
            // TODO: why no assertion on the result?
            var result = await Sut.GetDefaultDatabase().ListAddToLeftAsync(key, serializer.Serialize(value), When.Always);
        }

        var keys = await db.ListRangeAsync(key);

        Assert.Equal(keys.Length, values.Count);
    }

    [Fact]
    public async Task ListAddToLeftAsyncArray_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 5000).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        const string key = "MyListAsync";

        await Sut.GetDefaultDatabase().ListAddToLeftAsync(key, items: values);

        var keys = await db.ListRangeAsync(key);

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task ListGetFromRightGenericShouldThrowExceptionWhenKeyIsEmpty_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDefaultDatabase().ListGetFromRightAsync<string>(string.Empty));
    }

    [Fact]
    public async Task ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Valid_Data_Async()
    {
        var values = Range(0, 1)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        const string key = "MyList";

        var tasks = values.ToFastArray(x => db.ListLeftPushAsync(key, serializer.Serialize(x)));

        await Task.WhenAll(tasks);

        var item = await Sut.GetDefaultDatabase().ListGetFromRightAsync<TestClass<string>>(key);

        Assert.NotNull(item);
        Assert.Equal(item.Key, values[0].Key);
        Assert.Equal(item.Value, values[0].Value);
    }

    [Fact]
    public async Task ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty_Async()
    {
        const string key = "MyList";

        var item = await Sut.GetDefaultDatabase().ListGetFromRightAsync<TestClass<string>>(key);

        Assert.Null(item);
    }

    [Fact]
    public async Task ListGetFromRight_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty_Async()
    {
        const string key = "MyList";

        var item = await Sut.GetDefaultDatabase().ListGetFromRightAsync<TestClass<string>>(key);

        Assert.Null(item);
    }

    [Fact]
    public async Task Get_Value_With_Expiry_Updates_ExpiryAt_Async()
    {
        const string key = "TestKey";
        const string value = "TestValue";
        var originalTime = DateTime.UtcNow.AddSeconds(5);
        var testTime = DateTime.UtcNow.AddSeconds(20);
        var resultTimeSpan = originalTime.Subtract(DateTime.UtcNow);

        await Sut.GetDefaultDatabase().AddAsync(key, value, originalTime);
        await Sut.GetDefaultDatabase().GetAsync<string>(key, testTime);

        var resultValue = await db.StringGetWithExpiryAsync(key);

        Assert.True(resultTimeSpan < resultValue.Expiry!.Value);
    }

    [Fact]
    public async Task Get_Value_With_Expiry_Updates_ExpiryIn_Async()
    {
        const string key = "TestKey";
        const string value = "TestValue";
        var originalTime = new TimeSpan(0, 0, 5);
        var testTime = new TimeSpan(0, 0, 20);
        var resultTimeSpan = originalTime;

        await Sut.GetDefaultDatabase().AddAsync(key, value, originalTime);
        await Sut.GetDefaultDatabase().GetAsync<string>(key, testTime);
        var resultValue = await db.StringGetWithExpiryAsync(key);

        Assert.True(resultTimeSpan < resultValue.Expiry!.Value);
    }

    [Fact]
    public async Task Get_All_Value_With_Expiry_Updates_Expiry_Async()
    {
        const string key = "TestKey";
        var value = new TestClass<string> { Key = key, Value = "Hello World!" };
        var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
        var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

        var values = new Tuple<string, TestClass<string>>[] { new(key, value) };
        var keys = new List<string> { key };

        await Sut.GetDefaultDatabase().AddAllAsync(values, originalTime);
        await Sut.GetDefaultDatabase().GetAllAsync<TestClass<string>>([.. keys], testTime);
        var resultValue = await db.StringGetWithExpiryAsync(key);

        Assert.True(originalTime < resultValue.Expiry!.Value);
    }

    [Fact]
    public async Task Update_Expiry_ExpiresIn_Async()
    {
        const string key = "TestKey";
        const string value = "Test Value";
        var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
        var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

        await Sut.GetDefaultDatabase().AddAsync(key, value, originalTime);
        await Sut.GetDefaultDatabase().UpdateExpiryAsync(key, testTime);

        var resultValue = await db.StringGetWithExpiryAsync(key);
        Assert.True(originalTime < resultValue.Expiry!.Value);
    }

    [Fact]
    public async Task Update_Expiry_ExpiresAt_Async()
    {
        const string key = "TestKey";
        const string value = "Test Value";
        var originalTime = DateTime.UtcNow.AddSeconds(5);
        var testTime = DateTime.UtcNow.AddSeconds(20);

        await Sut.GetDefaultDatabase().AddAsync(key, value, originalTime);
        await Sut.GetDefaultDatabase().UpdateExpiryAsync(key, testTime);

        var resultValue = await db.StringGetWithExpiryAsync(key);
        Assert.True(originalTime.Subtract(DateTime.UtcNow) < resultValue.Expiry!.Value);
    }
}
