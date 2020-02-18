using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Tests.Extensions;
using StackExchange.Redis.Extensions.Tests.Helpers;
using Xunit;

using static System.Linq.Enumerable;

namespace StackExchange.Redis.Extensions.Tests
{
	[Collection("Redis")]
	public abstract class CacheClientTestBase : IDisposable
	{
		protected readonly IDatabase Db;
		protected readonly IRedisCacheClient Sut;
		protected ISerializer Serializer;
		protected RedisConfiguration redisConfiguration;
		protected IRedisCacheConnectionPoolManager ConnectionPoolManager;

		protected CacheClientTestBase(ISerializer serializer)
		{
			redisConfiguration = new RedisConfiguration()
			{
				AbortOnConnectFail = true,
				KeyPrefix = "MyPrefix__",
				Hosts = new RedisHost[]
				{
					new RedisHost(){Host = "localhost", Port = 6379}
				},
				AllowAdmin = true,
				ConnectTimeout = 3000,
				Database = 0,
				ServerEnumerationStrategy = new ServerEnumerationStrategy()
				{
					Mode = ServerEnumerationStrategy.ModeOptions.All,
					TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
					UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
				}
			};

			Serializer = serializer;
			ConnectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);
			Sut = new RedisCacheClient(ConnectionPoolManager, Serializer, redisConfiguration);
			Db = Sut.GetDbFromConfiguration().Database;
		}

		public void Dispose()
		{
			Db.FlushDatabase();
			Db.Multiplexer.GetSubscriber().UnsubscribeAll();
			ConnectionPoolManager.Dispose();
		}

		[Fact]
		public async Task Info_Should_Return_Valid_Information()
		{
			var response = await Sut.GetDbFromConfiguration().GetInfoAsync();

			Assert.NotNull(response);
			Assert.True(response.Any());
			Assert.Equal("6379", response["tcp_port"]);
		}

        [Fact]
        public async Task Info_Category_Should_Return_Valid_Information()
        {
            var response = await Sut.GetDbFromConfiguration().GetInfoCategorizedAsync();

            Assert.NotNull(response);
            Assert.True(response.Any());
            Assert.Equal("6379", response.SingleOrDefault(x=> x.Key == "tcp_port").InfoValue);
        }

        [Fact]
		public async Task Add_Item_To_Redis_Database()
		{
			var added = await Sut.GetDbFromConfiguration().AddAsync("my Key", "my value");
            var redisValue = await Db.KeyExistsAsync("my Key");

            Assert.True(added);
			Assert.True(redisValue);
		}

		[Fact]
		public async Task Add_Complex_Item_To_Redis_Database()
		{
			var testobject = new TestClass<DateTime>();

			var added = await Sut.GetDbFromConfiguration().AddAsync("my Key", testobject);
			var redisValue = await Db.StringGetAsync("my Key");

			Assert.True(added);

			var obj = Serializer.Deserialize<TestClass<DateTime>>(redisValue);

			Assert.True(Db.KeyExists("my Key"));
			Assert.NotNull(obj);
			Assert.Equal(testobject.Key, obj.Key);
			Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
		}

		[Fact]
		public async Task Add_Multiple_Object_With_A_Single_Roundtrip_To_Redis_Must_Store_Data_Correctly_Into_Database()
		{
			var values = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("key1", "value1"),
				new Tuple<string, string>("key2", "value2"),
				new Tuple<string, string>("key3", "value3")
			};

			var added = await Sut.GetDbFromConfiguration().AddAllAsync(values);

			Assert.True(added);

			Assert.True(await Db.KeyExistsAsync("key1"));
			Assert.True(await Db.KeyExistsAsync("key2"));
			Assert.True(await Db.KeyExistsAsync("key3"));

			Assert.Equal("value1", Serializer.Deserialize<string>(await Db.StringGetAsync("key1")));
			Assert.Equal("value2", Serializer.Deserialize<string>(await Db.StringGetAsync("key2")));
			Assert.Equal("value3", Serializer.Deserialize<string>(await Db.StringGetAsync("key3")));
		}

		[Fact]
		public async Task Get_All_Should_Return_All_Database_Keys()
		{
			var values = Range(0, 5)
				.Select(i => new TestClass<string>($"Key{i}", Guid.NewGuid().ToString()))
				.ToArray();

            foreach(var x in values)
            {
                await Db.StringSetAsync(x.Key, Serializer.Serialize(x.Value));
            }

            var keys = new[] { values[0].Key, values[1].Key, values[2].Key, "notexistingkey" };

            var result = await Sut.GetDbFromConfiguration().GetAllAsync<string>(keys);

			Assert.True(result.Count() == 4);
			Assert.Equal(result[values[0].Key], values[0].Value);
			Assert.Equal(result[values[1].Key], values[1].Value);
			Assert.Equal(result[values[2].Key], values[2].Value);
			Assert.Null(result["notexistingkey"]);
		}

		[Fact]
		public async Task Get_With_Complex_Item_Should_Return_Correct_Value()
		{
			var value = Range(0, 1)
					.Select(i => new ComplexClassForTest<string, Guid>($"Key{i}", Guid.NewGuid()))
					.First();

			await Db.StringSetAsync(value.Item1, Serializer.Serialize(value));

			var cachedObject = await Sut.GetDbFromConfiguration().GetAsync<ComplexClassForTest<string, Guid>>(value.Item1);

			Assert.NotNull(cachedObject);
			Assert.Equal(value.Item1, cachedObject.Item1);
			Assert.Equal(value.Item2, cachedObject.Item2);
		}

		[Fact]
		public async Task Remove_All_Should_Remove_All_Specified_Keys()
		{
			var values = Range(1, 5)
					.Select(i => new TestClass<string>($"Key{i}", Guid.NewGuid().ToString()))
					.ToArray();

            foreach(var x in values)
            {
                await Db.StringSetAsync(x.Key, x.Value);
            }

			await Sut.GetDbFromConfiguration().RemoveAllAsync(values.Select(x => x.Key));

			foreach (var value in values)
			{
				Assert.False(Db.KeyExists(value.Key));
			}
		}

		[Fact]
		public async Task Search_With_Valid_Start_With_Pattern_Should_Return_Correct_Keys()
		{
			var values = Range(1, 20)
					.Select(i => new TestClass<string>($"Key{i}", Guid.NewGuid().ToString()))
					.ToArray();

            foreach (var x in values)
            {
                await Db.StringSetAsync(x.Key, x.Value);
            }

			var key = (await Sut.GetDbFromConfiguration().SearchKeysAsync("Key1*")).ToList();

			Assert.True(key.Count == 11);
		}

		[Fact]
		public async Task SearchKeys_With_Key_Prefix_Should_Return_All_Database_Keys()
		{
			var tsk1 = Sut.GetDbFromConfiguration().AddAsync("mykey1", "Foo");
            var tsk2 = Sut.GetDbFromConfiguration().AddAsync("mykey2", "Bar");
            var tsk3 = Sut.GetDbFromConfiguration().AddAsync("key3", "Bar");

            await Task.WhenAll(tsk1, tsk2, tsk3);

			var keys = await Sut.GetDbFromConfiguration().SearchKeysAsync("*mykey*");

			Assert.True(keys.Count() == 2);
		}

        [Fact]
        public async Task SearchKeys_With_Start_Should_Return_All_Keys()
        {
            var values = Range(0, 10)
                .Select(i => new TestClass<string>($"mykey{i}", Guid.NewGuid().ToString()))
                .ToArray();

            foreach (var x in values)
            {
                await Db.StringSetAsync(x.Key, x.Value);
            }

            var result = (await Sut.GetDbFromConfiguration().SearchKeysAsync("*")).OrderBy(k => k).ToList();

            Assert.True(result.Count == 10);
        }


        [Fact]
	    public async Task SearchKeys_With_Key_Prefix_Should_Return_Keys_Without_Prefix()
	    {
	        var values = Range(0, 10)
	            .Select(i => new TestClass<string>($"mykey{i}", Guid.NewGuid().ToString()))
	            .ToArray();

            foreach (var x in values)
            {
                await Db.StringSetAsync(x.Key, x.Value);
            }

            var result = (await Sut.GetDbFromConfiguration().SearchKeysAsync("*mykey*")).OrderBy(k => k).ToList();

	        Assert.True(result.Count == 10);

            for (int i = 0; i < result.Count; i++)
	        {
	            Assert.Equal(result[i], values[i].Key);
            }
	    }

        [Fact]
		public async Task Exist_With_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Range(0, 2)
					.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
					.ToArray();

            foreach (var x in values)
            {
                await Db.StringSetAsync(x.Key, x.Value);
            }

            Assert.True(await Sut.GetDbFromConfiguration().ExistsAsync(values[0].Key));
		}

		[Fact]
		public async Task Exist_With_Not_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Range(0, 2)
					.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            foreach (var x in values)
            {
                await Db.StringSetAsync(x.Key, x.Value);
            }

            Assert.False(await Sut.GetDbFromConfiguration().ExistsAsync("this key doesn not exist into redi"));
		}

		[Fact]
		public async Task SetAdd_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5)
				.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
				.ToArray();

            foreach(var x in values)
            {
                await Db.StringSetAsync(x.Key, Serializer.Serialize(x.Value));
                await Sut.GetDbFromConfiguration().SetAddAsync("MySet", x.Key);
            };

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Length);
		}

		[Fact]
		public async Task SetMembers_With_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

            foreach (var x in values)
            {
                await Db.SetAddAsync("MySet", Serializer.Serialize(x));
            };

			var keys = (await Sut.GetDbFromConfiguration().SetMembersAsync<TestClass<string>>("MySet")).ToArray();

			Assert.Equal(keys.Length, values.Length);

			foreach (var key in keys)
			{
				Assert.Contains(values, x => x.Key == key.Key && x.Value == key.Value);
			}
		}

		[Fact]
		public async Task SetMember_With_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

            foreach (var x in values)
            {
                await Db.StringSetAsync(x.Key, Serializer.Serialize(x.Value));
                await Db.SetAddAsync("MySet", x.Key);
            };

			var keys = await Sut.GetDbFromConfiguration().SetMemberAsync("MySet");

			Assert.Equal(keys.Length, values.Length);
		}

		[Fact]
		public async Task SetMembers_With_Complex_Object_And_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

            foreach (var x in values)
            {
                await Db.SetAddAsync("MySet", Serializer.Serialize(x));
            };

			var keys = (await Sut.GetDbFromConfiguration().SetMembersAsync<TestClass<string>>("MySet")).ToArray();

			Assert.Equal(keys.Length, values.Length);
		}

		[Fact]
		public async Task Massive_Add_Should_Not_Throw_Exception_And_Work_Correctly()
		{
			const int size = 3000;
			var values = Range(0, size).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

			var tupleValues = values.Select(x => new Tuple<string, TestClass<string>>(x.Key, x)).ToList();
			var result = await Sut.GetDbFromConfiguration().AddAllAsync(tupleValues);
			var cached = await Sut.GetDbFromConfiguration().GetAllAsync<TestClass<string>>(values.Select(x => x.Key));

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
		public async Task Adding_Value_Type_Should_Return_Correct_Value()
		{
			var d = 1;
			var added = await Sut.GetDbFromConfiguration().AddAsync("my Key", d);
			var dbValue = await Sut.GetDbFromConfiguration().GetAsync<int>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue, d);
		}

		[Fact]
		public async Task Adding_Collection_To_Redis_Should_Work_Correctly()
		{
			var items = Range(1, 3).Select(i => new TestClass<string> { Key = $"key{i}", Value = "value{i}" }).ToArray();
			var added = await Sut.GetDbFromConfiguration().AddAsync("my Key", items);
			var dbValue = await Sut.GetDbFromConfiguration().GetAsync<TestClass<string>[]>("my Key");

			Assert.True(added);
			Assert.True(await Db.KeyExistsAsync("my Key"));
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
			var items = Range(1, 3).Select(i => new Tuple<string, string>($"key{i}", "value{i}")).ToArray();
			var added = await Sut.GetDbFromConfiguration().AddAllAsync(items, expiresIn);

            await Task.Delay(expiresIn.Add(new TimeSpan(0, 0, 1)));
			var hasExpired = items.All(x => !Db.KeyExists(x.Item1));

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

			Func<IEnumerable<int>, Task> action = value => {
                {
				    subscriberNotified = true;
				    subscriberValue = value;
                }

                return Task.CompletedTask;
            };

			await Sut.GetDbFromConfiguration().SubscribeAsync(channel, action);

			var result = await Sut.GetDbFromConfiguration().PublishAsync(channel, message);

			while (!subscriberNotified)
			{
                await Task.Delay(100);
			}

			Assert.Equal(1, result);
			Assert.True(subscriberNotified);
			Assert.Equal(message, subscriberValue);
		}

		[Fact]
		public async Task SetAddGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAddAsync(string.Empty, string.Empty));
		}

		[Fact]
		public async Task SetAddGenericShouldThrowExceptionWhenItemIsNull()
		{
            await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAddAsync<string>("MySet", null));
		}

		[Fact]
		public async Task SetAddGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

            foreach(var x in values)
            {
                await Db.StringSetAsync(x.Key, Serializer.Serialize(x.Value));
                await Sut.GetDbFromConfiguration().SetAddAsync("MySet", x);
            }

			var keys = await Db.SetMembersAsync("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public async Task SetAddAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
            await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAddAsync(string.Empty, string.Empty) );
		}

		[Fact]
		public async Task SetAddAsyncGenericShouldThrowExceptionWhenItemIsNull()
		{
            await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAddAsync<string>("MySet", null));
        }

		[Fact]
		public async Task SetAddAllGenericShouldThrowExceptionWhenItemsIsNull()
		{
            await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAddAllAsync("MySet", CommandFlags.None, (string[])null));
		}

		[Fact]
		public async Task SetAddAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
            await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAddAllAsync("MySet", CommandFlags.None, "value", null, "value2"));
		}

		[Fact]
        public async Task SetRemoveGenericWithAnExistingItemShouldReturnTrue()
		{
			const string key = "MySet", item = "MyItem";

			await Sut.GetDbFromConfiguration().SetAddAsync(key, item);

			var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, item);
			Assert.True(result);
		}

		[Fact]
		public async Task SetRemoveGenericWithAnUnexistingItemShouldReturnFalse()
		{
			const string key = "MySet";

			await Sut.GetDbFromConfiguration().SetAddAsync(key, "ExistingItem");

			var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, "UnexistingItem");
			Assert.False(result);
		}

		[Fact]
		public async Task SetRemoveAsyncGenericWithAnExistingItemShouldReturnTrue()
		{
			const string key = "MySet", item = "MyItem";

			await Sut.GetDbFromConfiguration().SetAddAsync(key, item);

			var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, item);
			Assert.True(result);
		}

		[Fact]
		public async Task SetRemoveAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
            await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetRemoveAllAsync("MySet", CommandFlags.None, "value", null, "value2"));
		}

		[Fact]
		public async Task SetRemoveAllGenericWithAnExistingItemShouldReturnValidData()
		{
			const string key = "MySet";
			var items = new[] { "MyItem1", "MyItem2" };

			await Sut.GetDbFromConfiguration().SetAddAllAsync(key, CommandFlags.None, items);

			var result = await Sut.GetDbFromConfiguration().SetRemoveAllAsync(key,CommandFlags.None, items);
			Assert.Equal(items.Length, result);
		}

		[Fact]
		public async Task SetRemoveAllAsyncGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
            await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().SetRemoveAllAsync<string>("MySet", CommandFlags.None, "value", null, "value2"));
		}

		[Fact]
		public async Task ListAddToLeftGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, string.Empty));
		}

		[Fact]
		public async Task ListAddToLeftGenericShouldThrowExceptionWhenItemIsNull()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", null));
		}

		[Fact]
		public async Task ListAddToLeftGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

			const string key = "MyList";

            foreach(var x in values)
            {
                await Sut.GetDbFromConfiguration().ListAddToLeftAsync(key, Serializer.Serialize(x));
            }

			var keys = await Db.ListRangeAsync(key);

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			await Assert.ThrowsAsync<ArgumentException>(
				() => Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, string.Empty));
		}

		[Fact]
		public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenItemIsNull()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(
				() => Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", null));
		}

		[Fact]
		public async Task ListAddToLeftAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

			const string key = "MyListAsync";

			foreach (var value in values)
			{
				// TODO: why no assertion on the result?
				var result = await Sut.GetDbFromConfiguration().ListAddToLeftAsync(key, Serializer.Serialize(value));
			}
			var keys = await Db.ListRangeAsync(key);

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public async Task ListGetFromRightGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			await Assert.ThrowsAsync<ArgumentException>(() => Sut.GetDbFromConfiguration().ListGetFromRightAsync<string>(string.Empty));
		}

		[Fact]
		public async Task ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 1)
				.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
				.ToArray();

			var key = "MyList";

            foreach(var x in values)
            {
                await Db.ListLeftPushAsync(key, Serializer.Serialize(x));
            }
            
            var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key);

			Assert.Equal(item.Key, values[0].Key);
			Assert.Equal(item.Value, values[0].Value);
		}

		[Fact]
		public async Task ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty()
		{
			var key = "MyList";

			var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key);

			Assert.Null(item);
		}

		[Fact]
		public async Task ListGetFromRight_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty()
		{
			var key = "MyList";

			var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key);

			Assert.Null(item);
		}

		[Fact]
		public async Task Get_Value_With_Expiry_Updates_ExpiryAt()
		{
			var key = "TestKey";
			var value = "TestValue";
			var originalTime = DateTime.UtcNow.AddSeconds(5);
			var testTime = DateTime.UtcNow.AddSeconds(20);
			var resultTimeSpan = originalTime.Subtract(DateTime.UtcNow);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().GetAsync<string>(key, testTime);
			var resultValue = await Db.StringGetWithExpiryAsync(key);

			Assert.True(resultTimeSpan < resultValue.Expiry.Value);
		}

		[Fact]
		public async Task Get_Value_With_Expiry_Updates_ExpiryIn()
		{
			var key = "TestKey";
			var value = "TestValue";
			var originalTime = new TimeSpan(0, 0, 5);
			var testTime = new TimeSpan(0, 0, 20);
			var resultTimeSpan = originalTime;

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().GetAsync<string>(key, testTime);
			var resultValue = await Db.StringGetWithExpiryAsync(key);

			Assert.True(resultTimeSpan < resultValue.Expiry.Value);
		}

		[Fact]
		public async Task Get_All_Value_With_Expiry_Updates_Expiry()
		{
			var key = "TestKey";
			var value = new TestClass<string> { Key = key, Value = "Hello World!" };
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			var values = new List<Tuple<string, TestClass<string>>>() { new Tuple<string, TestClass<string>>(key, value) };
			var keys = new List<string> { key };

			await Sut.GetDbFromConfiguration().AddAllAsync(values, originalTime);
			await Sut.GetDbFromConfiguration().GetAllAsync<TestClass<string>>(keys, testTime);
			var resultValue = await Db.StringGetWithExpiryAsync(key);

			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public async Task Update_Expiry_ExpiresIn()
		{
			var key = "TestKey";
			var value = "Test Value";
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().UpdateExpiryAsync(key, testTime);

			var resultValue = await Db.StringGetWithExpiryAsync(key);
			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public async Task Update_Expiry_ExpiresAt_Async()
		{
			var key = "TestKey";
			var value = "Test Value";
			var originalTime = DateTime.UtcNow.AddSeconds(5);
			var testTime = DateTime.UtcNow.AddSeconds(20);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().UpdateExpiryAsync(key, testTime);

			var resultValue = await Db.StringGetWithExpiryAsync(key);
			Assert.True(originalTime.Subtract(DateTime.UtcNow) < resultValue.Expiry.Value);
		}

		#region Hash tests

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

            var redisValue = await Db.HashGetAsync(hashKey, entryKey);
            var data = Serializer.Deserialize<TestClass<DateTime>>(redisValue);

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
            var redisvalue = await Db.HashGetAsync(hashKey, entryKey);
            var data = Serializer.Deserialize<TestClass<DateTime>>(redisvalue);
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
			var initRes = Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, Serializer.Serialize(initialValue));

			// act
			var res = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, entryValue, nx: false);

			// assert
			Assert.True(initRes, "Initial value was not set");
			Assert.False(res); // NOTE: HSET returns: 1 if new field was created and value set, or 0 if field existed and value set. reference: http://redis.io/commands/HSET
			var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
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
			var data = Db
						.HashGet(hashKey, map.Keys.Select(x => (RedisValue)x).ToArray()).ToList()
						.Select(x => Serializer.Deserialize<TestClass<DateTime>>(x))
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

			Assert.True(Db.HashSet(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");
			// act

			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, entryKey);

			// assert
			Assert.True(result);
			Assert.True((await Db.HashGetAsync(hashKey, entryKey)).IsNull);
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
			Assert.True((await Db.HashGetAsync(hashKey, entryKey)).IsNull);
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

			await Db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act
			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, values.Keys);

			// assert
			Assert.Equal(values.Count, result);
			var dbValues = await Db.HashGetAsync(hashKey, values.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbValues);
			Assert.DoesNotContain(dbValues, x => !x.IsNull);
			Assert.Equal(0, await Db.HashLengthAsync(hashKey));
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

			await Db.HashSetAsync(hashKey,
				valuesDelete.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			await Db.HashSetAsync(hashKey,
			   valuesKeep.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act
			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, valuesDelete.Keys);

			// assert
			Assert.Equal(valuesDelete.Count, result);
			var dbDeletedValues = await Db.HashGetAsync(hashKey, valuesDelete.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbDeletedValues);
			Assert.DoesNotContain(dbDeletedValues, x => !x.IsNull);
			var dbValues = await Db.HashGetAsync(hashKey, valuesKeep.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbValues);
			Assert.DoesNotContain(dbValues, x => x.IsNull);
			Assert.Equal(1000, await Db.HashLengthAsync(hashKey));
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
			Assert.True(await Db.HashSetAsync(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");
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

			await Db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

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

			await Db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act
			var result = await Sut.GetDbFromConfiguration().HashValuesAsync<TestClass<DateTime>>(hashKey);

			// assert
			Assert.NotNull(result);
			var collection = result as IList<TestClass<DateTime>> ?? result.ToList();
			Assert.NotEmpty(collection);
			Assert.Equal(values.Count, collection.Count());

			foreach (var key in collection)
			{
				Assert.Contains(key, values.Values);
			}
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

			await Db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

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
			Assert.False(Db.HashExists(hashKey, entryKey));
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);
			
            // assert
			Assert.Equal(incBy, result);
			Assert.True(await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey));
			Assert.Equal(incBy, Db.HashGet(hashKey, entryKey));
		}

		[Fact]
		public async Task HashIncerementByLong_ValueExist_EntryIncrementedCorrectValueReturned()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = 15;
			var incBy = 1;

			Assert.True(Db.HashSet(hashKey, entryKey, entryValue));

			// act
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

			// assert
			var expected = entryValue + incBy;
			Assert.Equal(expected, result);
			Assert.Equal(expected, await Db.HashGetAsync(hashKey, entryKey));
		}

		[Fact]
		public async Task HashIncerementByDouble_ValueDoesntExist_EntryCreatedWithValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var incBy = 0.9;

			// act
			Assert.False(Db.HashExists(hashKey, entryKey));
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

			// assert
			Assert.Equal(incBy, result);
			Assert.True(await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey));
			Assert.Equal(incBy, (double)await Db.HashGetAsync(hashKey, entryKey), 6); // have to provide epsilon due to double error
		}

		[Fact]
		public async Task HashIncerementByDouble_ValueExist_EntryIncrementedCorrectValueReturned()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = 14.3;
			var incBy = 9.7;

			Assert.True(Db.HashSet(hashKey, entryKey, entryValue));

			// act
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

			// assert
			var expected = entryValue + incBy;
			Assert.Equal(expected, result);
			Assert.Equal(expected, Db.HashGet(hashKey, entryKey));
		}

		[Fact]
		public async Task HashScan_EmptyHash_ReturnEmptyCursor()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			Assert.True(Db.HashLength(hashKey) == 0);

			// act
			var result = await Sut.GetDbFromConfiguration().HashScanAsync<string>(hashKey, "*");
			
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

			await Db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act
			var result = await Sut.GetDbFromConfiguration().HashScanAsync<TestClass<DateTime>>(hashKey, "*");

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

			await Db.HashSetAsync(hashKey, values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act
			var result = await Sut.GetDbFromConfiguration().HashScanAsync<TestClass<DateTime>>(hashKey, "2*");

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

		#endregion // Hash tests
		
		#region Sorted Sets
		
		[Fact]
		public async Task Add_Item_To_Sorted_Set()
		{
			var testobject = new TestClass<DateTime>();
	
			var added = await Sut.GetDbFromConfiguration().SortedSetAddAsync("my Key", testobject, 0);
	  
			var result = Db.SortedSetScan("my Key").First();
	  
			Assert.True(added);
	  
			var obj = Serializer.Deserialize<TestClass<DateTime>>(result.Element);
	  
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
	  
			await Sut.GetDbFromConfiguration().SortedSetAddAsync("my Key", entryValueFirst, 1);
			await Sut.GetDbFromConfiguration().SortedSetAddAsync("my Key", entryValueLast, 2);
	  
			var result = Db.SortedSetScan("my Key").ToList();
	  
			Assert.NotNull(result);
	  
			var dataFirst = Serializer.Deserialize<TestClass<DateTime>>(result[0].Element);
			var dataLast = Serializer.Deserialize<TestClass<DateTime>>(result[1].Element);
	  
			Assert.Equal(entryValueFirst.Value, dataFirst.Value);
			Assert.Equal(entryValueLast.Value, dataLast.Value);
		}
	
		[Fact]
		public async Task Remove_Item_From_Sorted_Set()
		{
			var testobject = new TestClass<DateTime>();
	  
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject), 0);
	  
			var removed = await Sut.GetDbFromConfiguration().SortedSetRemoveAsync("my Key", testobject);
	  
			Assert.True(removed);
	  
	  
			Assert.Empty(Db.SortedSetScan("my Key"));
		}
	
		[Fact]
		public async Task Return_items_ordered()
		{
			var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
			var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
			var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
	  
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject1), 1);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject2), 2);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject3), 3);
	  
			var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key")).ToList();
	  
			Assert.Equal(descendingList[0], testobject1);
			Assert.Equal(descendingList[1], testobject2);
			Assert.Equal(descendingList[2], testobject3);
			Assert.Equal(3, descendingList.Count);
		}
	
		[Fact]
		public async Task Return_items_ordered_ascended()
		{
			var testobject1 = new TestClass<DateTime>("test_1", DateTime.UtcNow);
			var testobject2 = new TestClass<DateTime>("test_2", DateTime.UtcNow);
			var testobject3 = new TestClass<DateTime>("test_3", DateTime.UtcNow);
	  
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject1), 3);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject2), 2);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject3), 1);
	  
			var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", order: Order.Ascending)).ToList();
	  
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
	  
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject1), 1);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject2), 2);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject3), 3);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject4), 4);
	  
			var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", start: 1, stop: 2)).ToList();
	  
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
	  
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject1), 1);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject2), 2);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject3), 3);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject4), 4);
	  
			var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", skip: 1, take: 2)).ToList();
	  
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
	  
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject1), 1);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject2), 2);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject3), 3);
			await Db.SortedSetAddAsync("my Key", Serializer.Serialize(testobject4), 4);
	  
			var descendingList = (await Sut.GetDbFromConfiguration().SortedSetRangeByScoreAsync<TestClass<DateTime>>("my Key", start: 1, stop: 4, exclude: Exclude.Both)).ToList();
	  
			Assert.Equal(descendingList[0], testobject2);
			Assert.Equal(descendingList[1], testobject3);
			Assert.Equal(2, descendingList.Count);
		}

        [Fact]
        public async Task Add_IncrementItemt_To_Sorted_Set()
        {
            var testobject = new TestClass<DateTime>();
            var defaultscore = 1;
            var nextscore = 2;
            var added =  await Sut.GetDbFromConfiguration().SortedSetAddIncrementAsync("my Key", testobject, defaultscore);
            var added2 = await Sut.GetDbFromConfiguration().SortedSetAddIncrementAsync("my Key", testobject, nextscore);
            var result = Db.SortedSetScan("my Key").First();
         
            Assert.Equal(defaultscore, added);
            Assert.Equal(defaultscore+ nextscore, result.Score);
            var obj = Serializer.Deserialize<TestClass<DateTime>>(result.Element);

            Assert.NotNull(obj);
            Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
        }

        #endregion 
    }
}