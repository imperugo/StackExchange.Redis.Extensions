using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Tests.Extensions;
using StackExchange.Redis.Extensions.Tests.Helpers;

using Xunit;

using static System.Linq.Enumerable;

namespace StackExchange.Redis.Extensions.Core.Tests;

[Collection("Redis")]
public abstract partial class CacheClientTestBase : IDisposable
{
    private readonly IRedisCacheClient sut;
    private readonly IDatabase db;
    private readonly ISerializer serializer;
    private readonly IRedisCacheConnectionPoolManager connectionPoolManager;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    internal CacheClientTestBase(ISerializer serializer)
    {
        var redisConfiguration = new RedisConfiguration()
        {
            AbortOnConnectFail = true,
            KeyPrefix = "MyPrefix__",
            Hosts = new[]
            {
                new RedisHost { Host = "localhost", Port = 6379 }
            },
            AllowAdmin = true,
            ConnectTimeout = 3000,
            Database = 0,
            PoolSize = 5,
            ServerEnumerationStrategy = new()
            {
                Mode = ServerEnumerationStrategy.ModeOptions.All,
                TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
            }
        };

        var moqLogger = new Mock<ILogger<RedisCacheConnectionPoolManager>>();

        this.serializer = serializer;
        connectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration, moqLogger.Object);
        sut = new RedisCacheClient(connectionPoolManager, this.serializer, redisConfiguration);
        db = sut.GetDbFromConfiguration().Database;
    }

    protected IRedisCacheClient Sut => sut;

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
    public async Task Info_Should_Return_Valid_Information()
    {
        var response = await Sut
            .GetDbFromConfiguration()
            .GetInfoAsync()
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.True(response.Count > 0);
        Assert.Equal("6379", response["tcp_port"]);
    }

    [Fact]
    public async Task Info_Category_Should_Return_Valid_Information()
    {
        var response = await Sut
            .GetDbFromConfiguration()
            .GetInfoCategorizedAsync()
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.True(response.Count > 0);
        Assert.Equal("6379", response.Single(x => x.Key == "tcp_port").InfoValue);
    }

    [Fact]
    public async Task Add_Item_To_Redis_Database()
    {
        var added = await Sut
            .GetDbFromConfiguration()
            .AddAsync("my Key", "my value")
            .ConfigureAwait(false);

        var redisValue = await db.KeyExistsAsync("my Key").ConfigureAwait(false);

        Assert.True(added);
        Assert.True(redisValue);
    }

    [Fact]
    public async Task Add_Complex_Item_To_Redis_Database()
    {
        var testobject = new TestClass<DateTime>();

        var added = await Sut
            .GetDbFromConfiguration()
            .AddAsync("my Key", testobject)
            .ConfigureAwait(false);

        var redisValue = await db.StringGetAsync("my Key").ConfigureAwait(false);

        Assert.True(added);

        var obj = serializer.Deserialize<TestClass<DateTime>>(redisValue);

        Assert.True(db.KeyExists("my Key"));
        Assert.NotNull(obj);
        Assert.Equal(testobject.Key, obj.Key);
        Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
    }

    [Fact]
    public async Task Add_Multiple_Object_With_A_Single_Roundtrip_To_Redis_Must_Store_Data_Correctly_Into_Database()
    {
        var values = new List<Tuple<string, string>>
        {
            new("key1", "value1"),
            new("key2", "value2"),
            new("key3", "value3")
        };

        var added = await Sut
            .GetDbFromConfiguration()
            .AddAllAsync(values)
            .ConfigureAwait(false);

        Assert.True(added);

        Assert.True(await db.KeyExistsAsync("key1").ConfigureAwait(false));
        Assert.True(await db.KeyExistsAsync("key2").ConfigureAwait(false));
        Assert.True(await db.KeyExistsAsync("key3").ConfigureAwait(false));

        Assert.Equal("value1", serializer.Deserialize<string>(await db.StringGetAsync("key1").ConfigureAwait(false)));
        Assert.Equal("value2", serializer.Deserialize<string>(await db.StringGetAsync("key2").ConfigureAwait(false)));
        Assert.Equal("value3", serializer.Deserialize<string>(await db.StringGetAsync("key3").ConfigureAwait(false)));
    }

    [Fact]
    public async Task Get_All_Should_Return_All_Database_Keys()
    {
        var values = Range(0, 5)
            .Select(i => new TestClass<string>($"Key{i.ToString()}", Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, cancellationToken) => await db.StringSetAsync(x.Key, serializer.Serialize(x.Value)).ConfigureAwait(false)).ConfigureAwait(false);

        var keys = new[]
        {
            values[0].Key,
            values[1].Key,
            values[2].Key,
            "notexistingkey"
        };

        var result = await Sut
            .GetDbFromConfiguration()
            .GetAllAsync<string>(keys)
            .ConfigureAwait(false);

        Assert.True(result.Count == 4);
        Assert.Equal(result[values[0].Key], values[0].Value);
        Assert.Equal(result[values[1].Key], values[1].Value);
        Assert.Equal(result[values[2].Key], values[2].Value);
        Assert.Null(result["notexistingkey"]);
    }

    [Fact]
    public async Task Get_With_Complex_Item_Should_Return_Correct_Value()
    {
        var value = Range(0, 1)
            .Select(i => new ComplexClassForTest<string, Guid>($"Key{i.ToString()}", Guid.NewGuid()))
            .First();

        await db.StringSetAsync(value.Item1, serializer.Serialize(value)).ConfigureAwait(false);

        var cachedObject = await Sut
            .GetDbFromConfiguration()
            .GetAsync<ComplexClassForTest<string, Guid>>(value.Item1)
            .ConfigureAwait(false);

        Assert.NotNull(cachedObject);
        Assert.Equal(value.Item1, cachedObject.Item1);
        Assert.Equal(value.Item2, cachedObject.Item2);
    }

    [Fact]
    public async Task Remove_All_Should_Remove_All_Specified_Keys()
    {
        var values = Range(1, 5)
            .Select(i => new TestClass<string>($"Key{i.ToString()}", Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.StringSetAsync(x.Key, x.Value).ConfigureAwait(false)).ConfigureAwait(false);

        await Sut
            .GetDbFromConfiguration()
            .RemoveAllAsync(values.Select(x => x.Key).ToArray())
            .ConfigureAwait(false);

        foreach (var value in values)
            Assert.False(db.KeyExists(value.Key));
    }

    [Fact]
    public async Task Search_With_Valid_Start_With_Pattern_Should_Return_Correct_Keys()
    {
        var values = Range(1, 20)
            .Select(i => new TestClass<string>($"Key{i.ToString()}", Guid.NewGuid().ToString()));

        await Parallel.ForEachAsync(values, async (x, _) => await db.StringSetAsync(x.Key, x.Value).ConfigureAwait(false)).ConfigureAwait(false);

        var keys = await Sut
            .GetDbFromConfiguration()
            .SearchKeysAsync("Key1*")
            .ConfigureAwait(false);

        Assert.Equal(11, keys.Count());
    }

    [Fact]
    public async Task SearchKeys_With_Key_Prefix_Should_Return_All_Database_Keys()
    {
        var tsk1 = Sut.GetDbFromConfiguration().AddAsync("mykey1", "Foo");
        var tsk2 = Sut.GetDbFromConfiguration().AddAsync("mykey2", "Bar");
        var tsk3 = Sut.GetDbFromConfiguration().AddAsync("key3", "Bar");

        await Task.WhenAll(tsk1, tsk2, tsk3).ConfigureAwait(false);

        var keys = await Sut
            .GetDbFromConfiguration()
            .SearchKeysAsync("*mykey*")
            .ConfigureAwait(false);

        Assert.Equal(2, keys.Count());
    }

    [Fact]
    public async Task SearchKeys_With_Start_Should_Return_All_Keys()
    {
        var values = Range(0, 10)
            .Select(i => new TestClass<string>($"mykey{i.ToString()}", Guid.NewGuid().ToString()));

        await Parallel.ForEachAsync(values, async (x, _) => await db.StringSetAsync(x.Key, x.Value).ConfigureAwait(false)).ConfigureAwait(false);

        var keys = (await Sut
            .GetDbFromConfiguration()
            .SearchKeysAsync("*")
            .ConfigureAwait(false))
            .OrderBy(k => k);

        Assert.Equal(10, keys.Count());
    }

    [Fact]
    public async Task SearchKeys_With_Key_Prefix_Should_Return_Keys_Without_Prefix()
    {
        var values = Range(0, 10)
            .Select(i => new TestClass<string>($"mykey{i.ToString()}", Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.StringSetAsync(x.Key, x.Value).ConfigureAwait(false)).ConfigureAwait(false);

        var keys = (await Sut
            .GetDbFromConfiguration()
            .SearchKeysAsync("*mykey*")
            .ConfigureAwait(false))
            .OrderBy(k => k)
            .ToList();

        Assert.Equal(10, keys.Count);

        for (var i = 0; i < keys.Count; i++)
            Assert.Equal(keys[i], values[i].Key);
    }

    [Fact]
    public async Task Exist_With_Valid_Object_Should_Return_The_Correct_Instance()
    {
        var values = Range(0, 2)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.StringSetAsync(x.Key, x.Value).ConfigureAwait(false)).ConfigureAwait(false);

        Assert.True(await Sut
            .GetDbFromConfiguration()
            .ExistsAsync(values[0].Key)
            .ConfigureAwait(false));
    }

    [Fact]
    public async Task Exist_With_Not_Valid_Object_Should_Return_The_Correct_Instance()
    {
        var values = Range(0, 2)
            .Select(
                _ => new TestClass<string>(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()));

        await Parallel.ForEachAsync(values, async (x, _) => await db.StringSetAsync(x.Key, x.Value).ConfigureAwait(false)).ConfigureAwait(false);

        Assert.False(await Sut.GetDbFromConfiguration().ExistsAsync("this key doesn not exist into redi").ConfigureAwait(false));
    }

    [Fact]
    public async Task SetAdd_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, _) =>
            {
                await db.StringSetAsync(x.Key, serializer.Serialize(x.Value)).ConfigureAwait(false);
                await Sut.GetDbFromConfiguration().SetAddAsync("MySet", x.Key).ConfigureAwait(false);
            })
            .ConfigureAwait(false);

        var keys = db.SetMembers("MySet");

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task SetPop_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.SetAddAsync("MySet", serializer.Serialize(x.Value)).ConfigureAwait(false)).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetPopAsync<string>("MySet").ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Contains(values, v => v.Value == result);

        var members = await db.SetMembersAsync("MySet").ConfigureAwait(false);
        var itemsLeft = members.Select(m => serializer.Deserialize<string>(m)).ToArray();

        Assert.True(itemsLeft.Length == 4);
        Assert.DoesNotContain(itemsLeft, l => l == result);
    }

    [Fact]
    public async Task SetPop_With_A_Non_Existing_Key_Should_Return_Null()
    {
        Assert.Null(await Sut.GetDbFromConfiguration().SetPopAsync<string>("MySet").ConfigureAwait(false));
    }

    [Fact]
    public async Task SetPop_With_An_Empty_Key_Should_Throw_Exception()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetPopAsync<string>(string.Empty)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetPop_Count_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.SetAddAsync("MySet", serializer.Serialize(x.Value)).ConfigureAwait(false)).ConfigureAwait(false);

        var result = (await Sut
            .GetDbFromConfiguration()
            .SetPopAsync<string>("MySet", 3)
            .ConfigureAwait(false))
            .ToList();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        foreach (var r in result)
            Assert.Contains(values, v => v.Value == r);

        var members = await db
            .SetMembersAsync("MySet")
            .ConfigureAwait(false);

        var itemsLeft = members.Select(m => serializer.Deserialize<string>(m)).ToArray();

        Assert.True(itemsLeft.Length == 2);

        foreach (var r in result)
            Assert.DoesNotContain(itemsLeft, l => l == r);
    }

    [Fact]
    public async Task SetPop_Count_With_A_Non_Existing_Key_Should_Return_Null()
    {
        var result = await Sut.GetDbFromConfiguration().SetPopAsync<string>("MySet", 0).ConfigureAwait(false);
        Assert.Null(result);
    }

    [Fact]
    public async Task SetPop_Count_With_An_Empty_Key_Should_Throw_Exception()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetPopAsync<string>(string.Empty, 0)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetMembers_With_Valid_Data_Should_Return_Correct_Keys()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.SetAddAsync("MySet", serializer.Serialize(x)).ConfigureAwait(false)).ConfigureAwait(false);

        var keys = (await Sut
            .GetDbFromConfiguration()
            .SetMembersAsync<TestClass<string>>("MySet")
            .ConfigureAwait(false))
            .ToArray();

        Assert.Equal(keys.Length, values.Length);

        foreach (var key in keys)
            Assert.Contains(values, x => x.Key == key.Key && x.Value == key.Value);
    }

    [Fact]
    public async Task SetMember_With_Valid_Data_Should_Return_Correct_Keys()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        await Parallel.ForEachAsync(values, async (x, _) =>
            {
                await db.StringSetAsync(x.Key, serializer.Serialize(x.Value)).ConfigureAwait(false);
                await db.SetAddAsync("MySet", x.Key).ConfigureAwait(false);
            })
            .ConfigureAwait(false);

        var keys = await Sut
            .GetDbFromConfiguration()
            .SetMemberAsync("MySet")
            .ConfigureAwait(false);

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task SetMembers_With_Complex_Object_And_Valid_Data_Should_Return_Correct_Keys()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        await Parallel.ForEachAsync(values, async (x, _) => await db.SetAddAsync("MySet", serializer.Serialize(x)).ConfigureAwait(false)).ConfigureAwait(false);

        var keys = (await Sut
            .GetDbFromConfiguration()
            .SetMembersAsync<TestClass<string>>("MySet")
            .ConfigureAwait(false))
            .ToArray();

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task Massive_Add_Should_Not_Throw_Exception_And_Work_Correctly()
    {
        const int size = 3000;
        var values = Range(0, size).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        var tupleValues = values.Select(x => new Tuple<string, TestClass<string>>(x.Key, x)).ToList();

        var result = await Sut.GetDbFromConfiguration().AddAllAsync(tupleValues).ConfigureAwait(false);
        var cached = await Sut.GetDbFromConfiguration().GetAllAsync<TestClass<string>>(values.Select(x => x.Key).ToArray()).ConfigureAwait(false);

        Assert.True(result);
        Assert.NotNull(cached);
        Assert.Equal(size, cached.Count);

        foreach (var value in values)
        {
            Assert.Equal(value.Key, cached[value.Key].Key);
            Assert.Equal(value.Value, cached[value.Key].Value);
        }
    }

    [Fact]
    public async Task Massive_Add_With_Expiring_Should_Delete_Expired_Keys()
    {
        var values = new List<Tuple<string, string>>
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
            .GetDbFromConfiguration()
            .AddAllAsync(values, TimeSpan.FromMilliseconds(1))
            .ConfigureAwait(false);

        await Task.Delay(TimeSpan.FromMilliseconds(2)).ConfigureAwait(false);

        foreach (var value in values)
        {
            var exists = await Sut
                .GetDbFromConfiguration()
                .ExistsAsync(value.Item1)
                .ConfigureAwait(false);

            Assert.False(exists, value.Item1);
        }
    }

    [Fact]
    public async Task Massive_Add_With_Expiring_And_Add_List_Again_Should_Work()
    {
        // Issue 228
        // https://github.com/imperugo/StackExchange.Redis.Extensions/issues/288
        var valuesOneList = new List<Tuple<string, string>>
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

        await Sut.GetDbFromConfiguration().AddAllAsync(valuesOneList, TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);

        await Task.Delay(TimeSpan.FromMilliseconds(2)).ConfigureAwait(false);

        foreach (var value in valuesOneList)
        {
            var exists = await Sut
                .GetDbFromConfiguration()
                .ExistsAsync(value.Item1)
                .ConfigureAwait(false);

            Assert.False(exists, value.Item1);
        }

        var valuesTwoLis = new List<Tuple<string, string>>
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

        await Sut.GetDbFromConfiguration()
            .AddAllAsync(valuesTwoLis, TimeSpan.FromMilliseconds(1))
            .ConfigureAwait(false);

        await Task.Delay(TimeSpan.FromMilliseconds(2)).ConfigureAwait(false);

        foreach (var value in valuesTwoLis)
        {
            var exists = await Sut.GetDbFromConfiguration().ExistsAsync(value.Item1).ConfigureAwait(false);
            Assert.False(exists, value.Item1);
        }
    }

    [Fact]
    public async Task Adding_Value_Type_Should_Return_Correct_Value()
    {
        const int d = 1;
        var added = await Sut.GetDbFromConfiguration().AddAsync("my Key", d).ConfigureAwait(false);
        var dbValue = await Sut.GetDbFromConfiguration().GetAsync<int>("my Key").ConfigureAwait(false);

        Assert.True(added);
        Assert.True(db.KeyExists("my Key"));
        Assert.Equal(dbValue, d);
    }

    [Fact]
    public async Task Adding_Collection_To_Redis_Should_Work_Correctly()
    {
        var items = Range(1, 3).Select(i => new TestClass<string> { Key = $"key{i.ToString()}", Value = $"value{i.ToString()}" }).ToArray();
        var added = await Sut.GetDbFromConfiguration().AddAsync("my Key", items).ConfigureAwait(false);
        var dbValue = await Sut.GetDbFromConfiguration().GetAsync<TestClass<string>[]>("my Key").ConfigureAwait(false);

        Assert.True(added);
        Assert.True(await db.KeyExistsAsync("my Key").ConfigureAwait(false));
        Assert.Equal(dbValue.Length, items.Length);

        for (var i = 0; i < items.Length; i++)
        {
            Assert.Equal(dbValue[i].Value, items[i].Value);
            Assert.Equal(dbValue[i].Key, items[i].Key);
        }
    }

    [Fact]
    public async Task Adding_Collection_To_Redis_Should_Expire()
    {
        var expiresIn = new TimeSpan(0, 0, 1);
        var items = Range(1, 3).Select(i => new Tuple<string, string>($"key{i.ToString()}", "value{i}")).ToArray();
        var added = await Sut.GetDbFromConfiguration().AddAllAsync(items, expiresIn).ConfigureAwait(false);

        await Task.Delay(expiresIn.Add(new(0, 0, 1))).ConfigureAwait(false);
        var hasExpired = items.All(x => !db.KeyExists(x.Item1));

        Assert.True(added);
        Assert.True(hasExpired);
    }

    [Fact]
    public async Task Pub_Sub()
    {
        var message = Range(0, 10).ToArray();
        var channel = new RedisChannel(Encoding.UTF8.GetBytes("unit_test"), RedisChannel.PatternMode.Auto);
        var subscriberNotified = false;
        IEnumerable<int> subscriberValue = null;

        Task action(IEnumerable<int> value)
        {
            {
                subscriberNotified = true;
                subscriberValue = value;
            }

            return Task.CompletedTask;
        }

        await Sut.GetDbFromConfiguration().SubscribeAsync(channel, (Func<IEnumerable<int>, Task>)action).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().PublishAsync(channel, message).ConfigureAwait(false);

        while (!subscriberNotified)
            await Task.Delay(100).ConfigureAwait(false);

        Assert.Equal(1, result);
        Assert.True(subscriberNotified);
        Assert.Equal(message, subscriberValue);
    }

    [Fact]
    public async Task SetAddGenericShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAddAsync(string.Empty, string.Empty)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetAddGenericShouldThrowExceptionWhenItemIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAddAsync<string>("MySet", null)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetAddGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

        await Parallel.ForEachAsync(values, async (x, _) =>
            {
                await db.StringSetAsync(x.Key, serializer.Serialize(x.Value)).ConfigureAwait(false);
                await Sut.GetDbFromConfiguration().SetAddAsync("MySet", x).ConfigureAwait(false);
            })
            .ConfigureAwait(false);

        var keys = await db
            .SetMembersAsync("MySet")
            .ConfigureAwait(false);

        Assert.Equal(keys.Length, values.Count);
    }

    [Fact]
    public async Task SetAddAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAddAsync(string.Empty, string.Empty)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetAddAsyncGenericShouldThrowExceptionWhenItemIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAddAsync<string>("MySet", null)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetContainsAsyncShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetContainsAsync(string.Empty, string.Empty)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetContainsAsyncShouldReturnTrue()
    {
        const string key = "MySet";
        const string item = "MyItem";

        await Sut.GetDbFromConfiguration().SetAddAsync(key, item).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetContainsAsync(key, item).ConfigureAwait(false);

        Assert.True(result);
    }

    [Fact]
    public async Task SetContainsAsyncShouldReturnFalseWhenItemIsWrong()
    {
        const string key = "MySet";
        const string item = "MyItem";
        const string unknownItem = "MyUnknownItem";

        await Sut.GetDbFromConfiguration().SetAddAsync(key, item).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetContainsAsync(key, unknownItem).ConfigureAwait(false);

        Assert.False(result);
    }

    [Fact]
    public async Task SetContainsAsyncShouldReturnFalseWhenKeyIsWrong()
    {
        const string key = "MySet";
        const string item = "MyItem";
        const string unknownKey = "MyUnknownKey";

        await Sut.GetDbFromConfiguration().SetAddAsync(key, item).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetContainsAsync(unknownKey, item).ConfigureAwait(false);

        Assert.False(result);
    }

    [Fact]
    public async Task SetContainsAsyncShouldThrowExceptionWhenItemIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetContainsAsync<string>("MySet", null)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetAddAllGenericShouldThrowExceptionWhenItemsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAddAllAsync("MySet", CommandFlags.None, (string[])null)).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetAddAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAddAllAsync("MySet", CommandFlags.None, "value", null, "value2")).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetRemoveGenericWithAnExistingItemShouldReturnTrue()
    {
        const string key = "MySet", item = "MyItem";

        await Sut.GetDbFromConfiguration().SetAddAsync(key, item).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, item).ConfigureAwait(false);
        Assert.True(result);
    }

    [Fact]
    public async Task SetRemoveGenericWithAnUnexistingItemShouldReturnFalse()
    {
        const string key = "MySet";

        await Sut.GetDbFromConfiguration().SetAddAsync(key, "ExistingItem").ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, "UnexistingItem").ConfigureAwait(false);
        Assert.False(result);
    }

    [Fact]
    public async Task SetRemoveAsyncGenericWithAnExistingItemShouldReturnTrue()
    {
        const string key = "MySet", item = "MyItem";

        await Sut.GetDbFromConfiguration().SetAddAsync(key, item).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, item).ConfigureAwait(false);
        Assert.True(result);
    }

    [Fact]
    public async Task SetRemoveAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetRemoveAllAsync("MySet", CommandFlags.None, "value", null, "value2")).ConfigureAwait(false);
    }

    [Fact]
    public async Task SetRemoveAllGenericWithAnExistingItemShouldReturnValidData()
    {
        const string key = "MySet";
        var items = new[] { "MyItem1", "MyItem2" };

        await Sut.GetDbFromConfiguration().SetAddAllAsync(key, CommandFlags.None, items).ConfigureAwait(false);

        var result = await Sut.GetDbFromConfiguration().SetRemoveAllAsync(key, CommandFlags.None, items).ConfigureAwait(false);
        Assert.Equal(items.Length, result);
    }

    [Fact]
    public async Task SetRemoveAllAsyncGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetRemoveAllAsync<string>("MySet", CommandFlags.None, "value", null, "value2")).ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftGenericShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, string.Empty)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftArrayShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, items: Array.Empty<TestClass<string>>())).ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftGenericShouldThrowExceptionWhenItemIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", item: null)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftGenericShouldThrowExceptionWhenItemsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", items: null)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

        const string key = "MyList";

        foreach (var x in values)
            await Sut.GetDbFromConfiguration().ListAddToLeftAsync(key, serializer.Serialize(x)).ConfigureAwait(false);

        var keys = await db.ListRangeAsync(key).ConfigureAwait(false);

        Assert.Equal(keys.Length, values.Count);
    }

    [Fact]
    public async Task ListAddToLeftArray_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5000).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        const string key = "MyList";

        await Sut.GetDbFromConfiguration().ListAddToLeftAsync(key, items: values).ConfigureAwait(false);

        var keys = await db.ListRangeAsync(key).ConfigureAwait(false);

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, string.Empty))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftAsyncArrayShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, items: Array.Empty<TestClass<string>>()))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenItemIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", item: null))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenItemsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", items: null))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task ListAddToLeftAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

        const string key = "MyListAsync";

        foreach (var value in values)
        {
            // TODO: why no assertion on the result?
            var result = await Sut.GetDbFromConfiguration().ListAddToLeftAsync(key, serializer.Serialize(value)).ConfigureAwait(false);
        }

        var keys = await db.ListRangeAsync(key).ConfigureAwait(false);

        Assert.Equal(keys.Length, values.Count);
    }

    [Fact]
    public async Task ListAddToLeftAsyncArray_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 5000).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

        const string key = "MyListAsync";

        await Sut.GetDbFromConfiguration().ListAddToLeftAsync(key, items: values).ConfigureAwait(false);

        var keys = await db.ListRangeAsync(key).ConfigureAwait(false);

        Assert.Equal(keys.Length, values.Length);
    }

    [Fact]
    public async Task ListGetFromRightGenericShouldThrowExceptionWhenKeyIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().ListGetFromRightAsync<string>(string.Empty)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
    {
        var values = Range(0, 1)
            .Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
            .ToArray();

        const string key = "MyList";

        await Parallel.ForEachAsync(values, async (x, _) => await db.ListLeftPushAsync(key, serializer.Serialize(x)).ConfigureAwait(false)).ConfigureAwait(false);

        var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key).ConfigureAwait(false);

        Assert.Equal(item.Key, values[0].Key);
        Assert.Equal(item.Value, values[0].Value);
    }

    [Fact]
    public async Task ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty()
    {
        const string key = "MyList";

        var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key).ConfigureAwait(false);

        Assert.Null(item);
    }

    [Fact]
    public async Task ListGetFromRight_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty()
    {
        const string key = "MyList";

        var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key).ConfigureAwait(false);

        Assert.Null(item);
    }

    [Fact]
    public async Task Get_Value_With_Expiry_Updates_ExpiryAt()
    {
        const string key = "TestKey";
        const string value = "TestValue";
        var originalTime = DateTime.UtcNow.AddSeconds(5);
        var testTime = DateTime.UtcNow.AddSeconds(20);
        var resultTimeSpan = originalTime.Subtract(DateTime.UtcNow);

        await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().GetAsync<string>(key, testTime).ConfigureAwait(false);

        var resultValue = await db.StringGetWithExpiryAsync(key).ConfigureAwait(false);

        Assert.True(resultTimeSpan < resultValue.Expiry.Value);
    }

    [Fact]
    public async Task Get_Value_With_Expiry_Updates_ExpiryIn()
    {
        const string key = "TestKey";
        const string value = "TestValue";
        var originalTime = new TimeSpan(0, 0, 5);
        var testTime = new TimeSpan(0, 0, 20);
        var resultTimeSpan = originalTime;

        await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().GetAsync<string>(key, testTime).ConfigureAwait(false);
        var resultValue = await db.StringGetWithExpiryAsync(key).ConfigureAwait(false);

        Assert.True(resultTimeSpan < resultValue.Expiry.Value);
    }

    [Fact]
    public async Task Get_All_Value_With_Expiry_Updates_Expiry()
    {
        const string key = "TestKey";
        var value = new TestClass<string> { Key = key, Value = "Hello World!" };
        var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
        var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

        var values = new List<Tuple<string, TestClass<string>>> { new(key, value) };
        var keys = new List<string> { key };

        await Sut.GetDbFromConfiguration().AddAllAsync(values, originalTime).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().GetAllAsync<TestClass<string>>(keys.ToArray(), testTime).ConfigureAwait(false);
        var resultValue = await db.StringGetWithExpiryAsync(key).ConfigureAwait(false);

        Assert.True(originalTime < resultValue.Expiry.Value);
    }

    [Fact]
    public async Task Update_Expiry_ExpiresIn()
    {
        const string key = "TestKey";
        const string value = "Test Value";
        var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
        var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

        await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().UpdateExpiryAsync(key, testTime).ConfigureAwait(false);

        var resultValue = await db.StringGetWithExpiryAsync(key).ConfigureAwait(false);
        Assert.True(originalTime < resultValue.Expiry.Value);
    }

    [Fact]
    public async Task Update_Expiry_ExpiresAt_Async()
    {
        const string key = "TestKey";
        const string value = "Test Value";
        var originalTime = DateTime.UtcNow.AddSeconds(5);
        var testTime = DateTime.UtcNow.AddSeconds(20);

        await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime).ConfigureAwait(false);
        await Sut.GetDbFromConfiguration().UpdateExpiryAsync(key, testTime).ConfigureAwait(false);

        var resultValue = await db.StringGetWithExpiryAsync(key).ConfigureAwait(false);
        Assert.True(originalTime.Subtract(DateTime.UtcNow) < resultValue.Expiry.Value);
    }
}
