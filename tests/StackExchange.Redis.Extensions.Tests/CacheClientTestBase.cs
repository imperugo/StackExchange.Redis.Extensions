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
					new RedisHost(){Host = "redishost", Port = 6379}
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
			//redisCacheConnectionPoolManager = new SinglePool(redisConfiguration);
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
		public void Info_Should_Return_Valid_Information()
		{
			var response = Sut.GetDbFromConfiguration().GetInfo();

			Assert.NotNull(response);
			Assert.True(response.Any());
			Assert.Equal("6379", response["tcp_port"]);
		}

		[Fact]
		public void Add_Item_To_Redis_Database()
		{
			var added = Sut.GetDbFromConfiguration().Add("my Key", "my value");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
		}

		[Fact]
		public void Add_Complex_Item_To_Redis_Database()
		{
			var testobject = new TestClass<DateTime>();

			var added = Sut.GetDbFromConfiguration().Add("my Key", testobject);

			var result = Db.StringGet("my Key");

			Assert.True(added);

			var obj = Serializer.Deserialize<TestClass<DateTime>>(result);

			Assert.True(Db.KeyExists("my Key"));
			Assert.NotNull(obj);
			Assert.Equal(testobject.Key, obj.Key);
			Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
		}

		[Fact]
		public void Add_Multiple_Object_With_A_Single_Roundtrip_To_Redis_Must_Store_Data_Correctly_Into_Database()
		{
			var values = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("key1", "value1"),
				new Tuple<string, string>("key2", "value2"),
				new Tuple<string, string>("key3", "value3")
			};

			var added = Sut.GetDbFromConfiguration().AddAll(values);

			Assert.True(added);

			Assert.True(Db.KeyExists("key1"));
			Assert.True(Db.KeyExists("key2"));
			Assert.True(Db.KeyExists("key3"));

			Assert.Equal("value1", Serializer.Deserialize<string>(Db.StringGet("key1")));
			Assert.Equal("value2", Serializer.Deserialize<string>(Db.StringGet("key2")));
			Assert.Equal("value3", Serializer.Deserialize<string>(Db.StringGet("key3")));
		}

		[Fact]
		public void Get_All_Should_Return_All_Database_Keys()
		{
			var values = Range(0, 5)
				.Select(i => new TestClass<string>($"Key{i}", Guid.NewGuid().ToString()))
				.ToArray();

			values.ForEach(x => Db.StringSet(x.Key, Serializer.Serialize(x.Value)));

			var result = Sut.GetDbFromConfiguration().GetAll<string>(new[] { values[0].Key, values[1].Key, values[2].Key, "notexistingkey" });

			Assert.True(result.Count() == 4);
			Assert.Equal(result[values[0].Key], values[0].Value);
			Assert.Equal(result[values[1].Key], values[1].Value);
			Assert.Equal(result[values[2].Key], values[2].Value);
			Assert.Null(result["notexistingkey"]);
		}

		[Fact]
		public void Get_With_Complex_Item_Should_Return_Correct_Value()
		{
			var value = Range(0, 1)
					.Select(i => new ComplexClassForTest<string, Guid>($"Key{i}", Guid.NewGuid()))
					.First();

			Db.StringSet(value.Item1, Serializer.Serialize(value));

			var cachedObject = Sut.GetDbFromConfiguration().Get<ComplexClassForTest<string, Guid>>(value.Item1);

			Assert.NotNull(cachedObject);
			Assert.Equal(value.Item1, cachedObject.Item1);
			Assert.Equal(value.Item2, cachedObject.Item2);
		}

		[Fact]
		public void Remove_All_Should_Remove_All_Specified_Keys()
		{
			var values = Range(1, 5)
					.Select(i => new TestClass<string>($"Key{i}", Guid.NewGuid().ToString()))
					.ToArray();
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			Sut.GetDbFromConfiguration().RemoveAll(values.Select(x => x.Key));

			foreach (var value in values)
			{
				Assert.False(Db.KeyExists(value.Key));
			}
		}

		[Fact]
		public void Search_With_Valid_Start_With_Pattern_Should_Return_Correct_Keys()
		{
			var values = Range(1, 20)
					.Select(i => new TestClass<string>($"Key{i}", Guid.NewGuid().ToString()))
					.ToArray();

			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			var key = Sut.GetDbFromConfiguration().SearchKeys("Key1*").ToList();

			Assert.True(key.Count == 11);
		}

		[Fact]
		public void SearchKeys_With_Key_Prefix_Should_Return_All_Database_Keys()
		{
			Sut.GetDbFromConfiguration().Add("mykey1", "Foo");
			Sut.GetDbFromConfiguration().Add("mykey2", "Bar");
			Sut.GetDbFromConfiguration().Add("key3", "Bar");

			var keys = Sut.GetDbFromConfiguration().SearchKeys("*mykey*");

			Assert.True(keys.Count() == 2);
		}

	    [Fact]
	    public void SearchKeys_With_Key_Prefix_Should_Return_Keys_Without_Prefix()
	    {
	        var values = Range(0, 10)
	            .Select(i => new TestClass<string>($"mykey{i}", Guid.NewGuid().ToString()))
	            .ToArray();

            values.ForEach(x => Sut.GetDbFromConfiguration().Add(x.Key, x.Value));

            var result = Sut.GetDbFromConfiguration().SearchKeys("*mykey*").OrderBy(k => k).ToList();

	        Assert.True(result.Count == 10);

            for (int i = 0; i < result.Count; i++)
	        {
	            Assert.Equal(result[i], values[i].Key);
            }
	    }

        [Fact]
		public void Exist_With_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Range(0, 2)
					.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
					.ToArray();
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			Assert.True(Sut.GetDbFromConfiguration().Exists(values[0].Key));
		}

		[Fact]
		public void Exist_With_Not_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Range(0, 2)
					.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			Assert.False(Sut.GetDbFromConfiguration().Exists("this key doesn not exist into redi"));
		}

		[Fact]
		public void SetAdd_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5)
				.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
				.ToArray();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				Sut.GetDbFromConfiguration().SetAdd<string>("MySet", x.Key);
			});

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Length);
		}

		[Fact]
		public void SetMembers_With_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

			values.ForEach(x =>
			{
				Db.SetAdd("MySet", Serializer.Serialize(x));
			});

			var keys = (Sut.GetDbFromConfiguration().SetMembers<TestClass<string>>("MySet")).ToArray();

			Assert.Equal(keys.Length, values.Length);

			foreach (var key in keys)
			{
				Assert.Contains(values, x => x.Key == key.Key && x.Value == key.Value);
			}
		}

		[Fact]
		public void SetMember_With_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				Db.SetAdd("MySet", x.Key);
			});

			var keys = Sut.GetDbFromConfiguration().SetMember("MySet");

			Assert.Equal(keys.Length, values.Length);
		}

		[Fact]
		public async void SetMembersAsync_With_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

			values.ForEach(x =>
			{
				Db.SetAdd("MySet", Serializer.Serialize(x));
			});

			var keys = (await Sut.GetDbFromConfiguration().SetMembersAsync<TestClass<string>>("MySet")).ToArray();

			Assert.Equal(keys.Length, values.Length);
		}

		[Fact]
		public void Massive_Add_Should_Not_Throw_Exception_And_Work_Correctly()
		{
			const int size = 3000;
			var values = Range(0, size).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray();

			var tupleValues = values.Select(x => new Tuple<string, TestClass<string>>(x.Key, x)).ToList();
			var result = Sut.GetDbFromConfiguration().AddAll(tupleValues);
			var cached = Sut.GetDbFromConfiguration().GetAll<TestClass<string>>(values.Select(x => x.Key));

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
		public void Adding_Value_Type_Should_Return_Correct_Value()
		{
			var d = 1;
			var added = Sut.GetDbFromConfiguration().Add("my Key", d);
			var dbValue = Sut.GetDbFromConfiguration().Get<int>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue, d);
		}

		[Fact]
		public void Adding_Collection_To_Redis_Should_Work_Correctly()
		{
			var items = Range(1, 3).Select(i => new TestClass<string> { Key = $"key{i}", Value = "value{i}" }).ToArray();
			var added = Sut.GetDbFromConfiguration().Add("my Key", items);
			var dbValue = Sut.GetDbFromConfiguration().Get<TestClass<string>[]>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue.Length, items.Length);
			for (var i = 0; i < items.Length; i++)
			{
				Assert.Equal(dbValue[i].Value, items[i].Value);
				Assert.Equal(dbValue[i].Key, items[i].Key);
			}
		}

		[Fact]
		public void Adding_Collection_To_Redis_Should_Expire()
		{
			var expiresIn = new TimeSpan(0, 0, 1);
			var items = Range(1, 3).Select(i => new Tuple<string, string>($"key{i}", "value{i}")).ToArray();
			var added = Sut.GetDbFromConfiguration().AddAll(items, expiresIn);

			Thread.Sleep(expiresIn.Add(new TimeSpan(0, 0, 1)));
			var hasExpired = items.All(x => !Db.KeyExists(x.Item1));

			Assert.True(added);
			Assert.True(hasExpired);
		}

		//[Fact(Skip = "AppVeyor, see here http://help.appveyor.com/discussions/problems/3760-vs-runner-hangs-on-run-all")]
		[Fact]
		public void Pub_Sub()
		{
			var message = Range(0, 10).ToArray();
			var channel = new RedisChannel(Encoding.UTF8.GetBytes("unit_test"), RedisChannel.PatternMode.Auto);
			var subscriberNotified = false;
			IEnumerable<int> subscriberValue = null;

			Action<IEnumerable<int>> action = value =>
			{
				subscriberNotified = true;
				subscriberValue = value;
			};

			Sut.GetDbFromConfiguration().Subscribe(channel, action);

			var result = Sut.GetDbFromConfiguration().Publish(channel, message);

			while (!subscriberNotified)
			{
				Thread.Sleep(100);
			}

			Assert.Equal(1, result);
			Assert.True(subscriberNotified);
			Assert.Equal(message, subscriberValue);
		}

		[Fact]
		public void SetAddGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			Assert.Throws<ArgumentException>(() => Sut.GetDbFromConfiguration().SetAdd<string>(string.Empty, string.Empty));
		}

		[Fact]
		public void SetAddGenericShouldThrowExceptionWhenItemIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Sut.GetDbFromConfiguration().SetAdd<string>("MySet", null));
		}

		[Fact]
		public void SetAddGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				Sut.GetDbFromConfiguration().SetAdd("MySet", x);
			});

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public async Task SetAddAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			try
			{
				await Sut.GetDbFromConfiguration().SetAddAsync<string>(string.Empty, string.Empty);
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentException>(ex);
			}
		}

		[Fact]
		public async Task SetAddAsyncGenericShouldThrowExceptionWhenItemIsNull()
		{
			try
			{
				await Sut.GetDbFromConfiguration().SetAddAsync<string>("MySet", null);
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentNullException>(ex);
			}
		}

		[Fact]
		public async Task SetAddAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

			var key = "MySet";

			foreach (var value in values)
			{
				Db.StringSet(value.Key, Serializer.Serialize(value.Value));
				var result = await Sut.GetDbFromConfiguration().SetAddAsync(key, value);
				Assert.True(result, $"SetAddAsync {key}:{value} failed");
			}

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void SetAddAllGenericShouldReturnValidData()
		{
			var items = new[] { "val1", "val2", "val3" };
			long result = Sut.GetDbFromConfiguration().SetAddAll<string>("MySet", CommandFlags.None, items);
			Assert.Equal(result, items.Length);
		}

		[Fact]
		public void SetAddAllGenericShouldThrowExceptionWhenItemsIsNull()
		{
			try
			{
				long result = Sut.GetDbFromConfiguration().SetAddAll<string>("MySet", CommandFlags.None, (string[])null);
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentNullException>(ex);
			}
		}

		[Fact]
		public void SetAddAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
			try
			{
				long result = Sut.GetDbFromConfiguration().SetAddAll<string>("MySet", CommandFlags.None, "value", null, "value2");
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentException>(ex);
			}
		}

		[Fact]
		public async Task SetAddAllAsyncGenericShouldReturnValidData()
		{
			var items = new[] { "val1", "val2", "val3" };
			long result = await Sut.GetDbFromConfiguration().SetAddAllAsync<string>("MySet", CommandFlags.None, items);
			Assert.Equal(result, items.Length);
		}

		[Fact]
		public async Task SetAddAllAsyncGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
			try
			{
				var items = new string[] { "value", null, "value2" };
				long result = await Sut.GetDbFromConfiguration().SetAddAllAsync<string>("MySet", CommandFlags.None, items);
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentException>(ex);
			}
		}

		[Fact]
		public void SetRemoveGenericWithAnExistingItemShouldReturnTrue()
		{
			const string key = "MySet", item = "MyItem";

			Sut.GetDbFromConfiguration().SetAdd<string>(key, item);

			var result = Sut.GetDbFromConfiguration().SetRemove(key, item);
			Assert.True(result);
		}

		[Fact]
		public void SetRemoveGenericWithAnUnexistingItemShouldReturnFalse()
		{
			const string key = "MySet";

			Sut.GetDbFromConfiguration().SetAdd<string>(key, "ExistingItem");

			var result = Sut.GetDbFromConfiguration().SetRemove(key, "UnexistingItem");
			Assert.False(result);
		}

		[Fact]
		public async Task SetRemoveAsyncGenericWithAnExistingItemShouldReturnTrue()
		{
			const string key = "MySet", item = "MyItem";

			Sut.GetDbFromConfiguration().SetAdd<string>(key, item);

			var result = await Sut.GetDbFromConfiguration().SetRemoveAsync(key, item);
			Assert.True(result);
		}

		[Fact]
		public void SetRemoveAllGenericWithAnExistingItemShouldReturnValidData()
		{
			const string key = "MySet";
			var items = new[] { "MyItem1", "MyItem2" };

			Sut.GetDbFromConfiguration().SetAddAll<string>(key, CommandFlags.None, items);

			var result = Sut.GetDbFromConfiguration().SetRemoveAll(key, CommandFlags.None, items);
			Assert.Equal(items.Length, result);
		}

		[Fact]
		public void SetRemoveAllGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
			try
			{
				long result = Sut.GetDbFromConfiguration().SetRemoveAll<string>("MySet",CommandFlags.None,  "value", null, "value2");
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentException>(ex);
			}
		}

		[Fact]
		public async Task SetRemoveAllAsyncGenericWithAnExistingItemShouldReturnValidData()
		{
			const string key = "MySet";
			var items = new[] { "MyItem1", "MyItem2" };

			Sut.GetDbFromConfiguration().SetAddAll<string>(key, CommandFlags.None, items);

			var result = await Sut.GetDbFromConfiguration().SetRemoveAllAsync(key,CommandFlags.None, items);
			Assert.Equal(items.Length, result);
		}

		[Fact]
		public async Task SetRemoveAllAsyncGenericShouldThrowExceptionWhenItemsContainsOneNullItem()
		{
			try
			{
				long result = await Sut.GetDbFromConfiguration().SetRemoveAllAsync<string>("MySet", CommandFlags.None, "value", null, "value2");
			}
			catch (Exception ex)
			{
				Assert.IsType<ArgumentException>(ex);
			}
		}

		[Fact]
		public void ListAddToLeftGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			Assert.Throws<ArgumentException>(() => Sut.GetDbFromConfiguration().ListAddToLeft(string.Empty, string.Empty));
		}

		[Fact]
		public void ListAddToLeftGenericShouldThrowExceptionWhenItemIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Sut.GetDbFromConfiguration().ListAddToLeft<string>("MyList", null));
		}

		[Fact]
		public void ListAddToLeftGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 5).Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToList();

			const string key = "MyList";

			values.ForEach(x => Sut.GetDbFromConfiguration().ListAddToLeft(key, Serializer.Serialize(x)));

			var keys = Db.ListRange(key);

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			await Assert.ThrowsAsync<ArgumentException>(
				async () => await Sut.GetDbFromConfiguration().ListAddToLeftAsync(string.Empty, string.Empty));
		}

		[Fact]
		public async Task ListAddToLeftAsyncGenericShouldThrowExceptionWhenItemIsNull()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(
				async () => await Sut.GetDbFromConfiguration().ListAddToLeftAsync<string>("MyList", null));
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
			var keys = Db.ListRange(key);

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void ListGetFromRightGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			Assert.Throws<ArgumentException>(() => Sut.GetDbFromConfiguration().ListGetFromRight<string>(string.Empty));
		}

		[Fact]
		public void ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 1)
				.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
				.ToArray();

			var key = "MyList";

			values.ForEach(x => { Db.ListLeftPush(key, Serializer.Serialize(x)); });

			var item = Sut.GetDbFromConfiguration().ListGetFromRight<TestClass<string>>(key);

			Assert.Equal(item.Key, values[0].Key);
			Assert.Equal(item.Value, values[0].Value);
		}

		[Fact]
		public async Task ListGetFromRightAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			await Assert.ThrowsAsync<ArgumentException>(
			   async () => await Sut.GetDbFromConfiguration().ListGetFromRightAsync<string>(string.Empty));
		}

		[Fact]
		public async Task ListGetFromRightAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Range(0, 1)
				.Select(_ => new TestClass<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
				.ToArray();

			var key = "MyList";

			values.ForEach(x => { Db.ListLeftPush(key, Serializer.Serialize(x)); });

			var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key);

			Assert.Equal(item.Key, values[0].Key);
			Assert.Equal(item.Value, values[0].Value);
		}

		[Fact]
		public async Task ListGetFromRightAsyncGeneric_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty()
		{
			var key = "MyList";

			var item = await Sut.GetDbFromConfiguration().ListGetFromRightAsync<TestClass<string>>(key);

			Assert.Null(item);
		}

		[Fact]
		public void ListGetFromRightAsync_With_An_Existing_Key_Should_Return_Null_If_List_Is_Empty()
		{
			var key = "MyList";

			var item = Sut.GetDbFromConfiguration().ListGetFromRight<TestClass<string>>(key);

			Assert.Null(item);
		}

		[Fact]
		public void Get_Value_With_Expiry_Updates_ExpiryAt()
		{
			var key = "TestKey";
			var value = "TestValue";
			var originalTime = DateTime.UtcNow.AddSeconds(5);
			var testTime = DateTime.UtcNow.AddSeconds(20);
			var resultTimeSpan = originalTime.Subtract(DateTime.UtcNow);

			Sut.GetDbFromConfiguration().Add(key, value, originalTime);
			Sut.GetDbFromConfiguration().Get<string>(key, testTime);
			var resultValue = Db.StringGetWithExpiry(key);

			Assert.True(resultTimeSpan < resultValue.Expiry.Value);
		}

		[Fact]
		public void Get_Value_With_Expiry_Updates_ExpiryIn()
		{
			var key = "TestKey";
			var value = "TestValue";
			var originalTime = new TimeSpan(0, 0, 5);
			var testTime = new TimeSpan(0, 0, 20);
			var resultTimeSpan = originalTime;

			Sut.GetDbFromConfiguration().Add(key, value, originalTime);
			Sut.GetDbFromConfiguration().Get<string>(key, testTime);
			var resultValue = Db.StringGetWithExpiry(key);

			Assert.True(resultTimeSpan < resultValue.Expiry.Value);
		}

		[Fact]
		public async void Get_Value_With_Expiry_Updates_ExpiryAt_Async()
		{
			var key = "TestKey";
			var value = "TestValue";
			var originalTime = DateTime.UtcNow.AddSeconds(5);
			var testTime = DateTime.UtcNow.AddSeconds(20);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().GetAsync<string>(key, testTime);
			var resultValue = Db.StringGetWithExpiry(key);

			Assert.True(originalTime.Subtract(DateTime.UtcNow) < resultValue.Expiry.Value);
		}

		[Fact]
		public async void Get_Value_With_Expiry_Updates_ExpiryIn_Async()
		{
			var key = "TestKey";
			var value = "TestValue";
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().GetAsync<string>(key, testTime);
			var resultValue = Db.StringGetWithExpiry(key);

			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public void Get_All_Value_With_Expiry_Updates_Expiry()
		{
			var key = "TestKey";
			var value = new TestClass<string> { Key = key, Value = "Hello World!" };
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			var values = new List<Tuple<string, TestClass<string>>>() { new Tuple<string, TestClass<string>>(key, value) };
			var keys = new List<string> { key };

			Sut.GetDbFromConfiguration().AddAll(values, originalTime);
			Sut.GetDbFromConfiguration().GetAll<TestClass<string>>(keys, testTime);
			var resultValue = Db.StringGetWithExpiry(key);

			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public async void Get_All_Value_With_Expiry_Updates_Expiry_Async()
		{
			var key = "TestKey";
			var value = new TestClass<string> { Key = key, Value = "Hello World!" };
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			var values = new List<Tuple<string, TestClass<string>>>() { new Tuple<string, TestClass<string>>(key, value) };
			var keys = new List<string> { key };

			await Sut.GetDbFromConfiguration().AddAllAsync(values, originalTime);
			await Sut.GetDbFromConfiguration().GetAllAsync<TestClass<string>>(keys, testTime);
			var resultValue = Db.StringGetWithExpiry(key);

			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public void Update_Expiry_ExpiresIn()
		{
			var key = "TestKey";
			var value = "Test Value";
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			Sut.GetDbFromConfiguration().Add(key, value, originalTime);
			Sut.GetDbFromConfiguration().UpdateExpiry(key, testTime);

			var resultValue = Db.StringGetWithExpiry(key);
			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public async void Update_Expiry_ExpiresIn_Async()
		{
			var key = "TestKey";
			var value = "Test Value";
			var originalTime = DateTime.UtcNow.AddSeconds(5).Subtract(DateTime.UtcNow);
			var testTime = DateTime.UtcNow.AddSeconds(20).Subtract(DateTime.UtcNow);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().UpdateExpiryAsync(key, testTime);

			var resultValue = Db.StringGetWithExpiry(key);
			Assert.True(originalTime < resultValue.Expiry.Value);
		}

		[Fact]
		public void Update_Expiry_ExpiresAt()
		{
			var key = "TestKey";
			var value = "Test Value";
			var originalTime = DateTime.UtcNow.AddSeconds(5);
			var testTime = DateTime.UtcNow.AddSeconds(20);

			Sut.GetDbFromConfiguration().Add(key, value, originalTime);
			Sut.GetDbFromConfiguration().UpdateExpiry(key, testTime);

			var resultValue = Db.StringGetWithExpiry(key);
			Assert.True(originalTime.Subtract(DateTime.UtcNow) < resultValue.Expiry.Value);
		}

		[Fact]
		public async void Update_Expiry_ExpiresAt_Async()
		{
			var key = "TestKey";
			var value = "Test Value";
			var originalTime = DateTime.UtcNow.AddSeconds(5);
			var testTime = DateTime.UtcNow.AddSeconds(20);

			await Sut.GetDbFromConfiguration().AddAsync(key, value, originalTime);
			await Sut.GetDbFromConfiguration().UpdateExpiryAsync(key, testTime);

			var resultValue = Db.StringGetWithExpiry(key);
			Assert.True(originalTime.Subtract(DateTime.UtcNow) < resultValue.Expiry.Value);
		}

		#region Hash tests

		[Fact]
		public void HashSetSingleValueNX_ValueDoesntExists_ShouldInsertAndRetrieveValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>("test", DateTime.UtcNow);

			// act
			var res = Sut.GetDbFromConfiguration().HashSet(hashKey, entryKey, entryValue, nx: true);

			// assert
			Assert.True(res);
			var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
			Assert.Equal(entryValue, data);
		}

		[Fact]
		public void HashSetSingleValueNX_ValueExists_ShouldNotInsertOriginalValueNotChanged()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
			var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
			var initRes = Sut.GetDbFromConfiguration().HashSet(hashKey, entryKey, initialValue);

			// act
			var res = Sut.GetDbFromConfiguration().HashSet(hashKey, entryKey, entryValue, nx: true);

			// assert
			Assert.True(initRes);
			Assert.False(res);
			var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
			Assert.Equal(initialValue, data);
		}

		[Fact]
		public void HashSetSingleValue_ValueExists_ShouldUpdateValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
			var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
			var initRes = Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, Serializer.Serialize(initialValue));

			// act
			var res = Sut.GetDbFromConfiguration().HashSet(hashKey, entryKey, entryValue, nx: false);

			// assert
			Assert.True(initRes, "Initial value was not set");
			Assert.False(res); // NOTE: HSET returns: 1 if new field was created and value set, or 0 if field existed and value set. reference: http://redis.io/commands/HSET
			var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
			Assert.Equal(entryValue, data);
		}

		[Fact]
		public void HashSetMultipleValues_HashGetMultipleValues_ShouldInsert()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values = Range(0, 100).Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow));
			var map = values.ToDictionary(val => Guid.NewGuid().ToString());

			// act
			Sut.GetDbFromConfiguration().HashSet(hashKey, map);
			Thread.Sleep(500);
			// assert
			var data = Sut.GetDbFromConfiguration().Database
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
		public void HashDelete_KeyExists_ShouldDelete()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
			Assert.True(Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");
			// act

			var result = Sut.GetDbFromConfiguration().HashDelete(hashKey, entryKey);

			// assert
			Assert.True(result);
			Assert.True(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey).IsNull);
		}

		[Fact]
		public void HashDelete_KeyDoesntExist_ShouldReturnFalse()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			// act

			var result = Sut.GetDbFromConfiguration().HashDelete(hashKey, entryKey);

			// assert
			Assert.False(result);
			Assert.True(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey).IsNull);
		}

		[Fact]
		public void HashDeleteMultiple_AllKeysExist_ShouldDeleteAll()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act

			var result = Sut.GetDbFromConfiguration().HashDelete(hashKey, values.Keys);

			// assert
			Assert.Equal(values.Count, result);
			var dbValues = Sut.GetDbFromConfiguration().Database.HashGet(hashKey, values.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbValues);
			Assert.DoesNotContain(dbValues, x => !x.IsNull);
			Assert.Equal(0, Sut.GetDbFromConfiguration().Database.HashLength(hashKey));
		}

		[Fact]
		public void HashDeleteMultiple_NotAllKeysExist_ShouldDeleteAllOnlyRequested()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var valuesDelete =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);
			var valuesKeep =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				valuesDelete.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
			   valuesKeep.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act

			var result = Sut.GetDbFromConfiguration().HashDelete(hashKey, valuesDelete.Keys);

			// assert
			Assert.Equal(valuesDelete.Count, result);
			var dbDeletedValues = Sut.GetDbFromConfiguration().Database.HashGet(hashKey, valuesDelete.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbDeletedValues);
			Assert.DoesNotContain(dbDeletedValues, x => !x.IsNull);
			var dbValues = Sut.GetDbFromConfiguration().Database.HashGet(hashKey, valuesKeep.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbValues);
			Assert.DoesNotContain(dbValues, x => x.IsNull);
			Assert.Equal(1000, Sut.GetDbFromConfiguration().Database.HashLength(hashKey));
			Assert.Equal(1000, dbValues.Length);
			Assert.All(dbValues, x => Assert.True(valuesKeep.ContainsKey(Sut.GetDbFromConfiguration().Serializer.Deserialize<TestClass<int>>(x).Key)));
		}

		[Fact]
		public void HashExists_KeyExists_ReturnTrue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
			Assert.True(Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");
			// act
			var result = Sut.GetDbFromConfiguration().HashExists(hashKey, entryKey);

			// assert
			Assert.True(result, "Entry doesn't exist in hash, but it should");
		}

		[Fact]
		public void HashExists_KeyDoesntExists_ReturnFalse()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			// act
			var result = Sut.GetDbFromConfiguration().HashExists(hashKey, entryKey);
			// assert
			Assert.False(result, "Entry doesn't exist in hash, but call returned true");
		}

		[Fact]
		public void HashKeys_HashEmpty_ReturnEmptyCollection()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			// act
			var result = Sut.GetDbFromConfiguration().HashKeys(hashKey);
			// assert
			Assert.NotNull(result);
			Assert.Empty(result);
		}

		[Fact]
		public void HashKeys_HashNotEmpty_ReturnKeysCollection()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			// act
			var result = Sut.GetDbFromConfiguration().HashKeys(hashKey);
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
		public void HashValues_HashEmpty_ReturnEmptyCollection()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			// act
			var result = Sut.GetDbFromConfiguration().HashValues<string>(hashKey);
			// assert
			Assert.NotNull(result);
			Assert.Empty(result);
		}

		[Fact]
		public void HashValues_HashNotEmpty_ReturnAllValues()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			// act
			var result = Sut.GetDbFromConfiguration().HashValues<TestClass<DateTime>>(hashKey);
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
		public void HashLength_HashEmpty_ReturnZero()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();

			// act
			var result = Sut.GetDbFromConfiguration().HashLength(hashKey);

			// assert
			Assert.Equal(0, result);
		}

		[Fact]
		public void HashLength_HashNotEmpty_ReturnCorrectCount()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			// act
			var result = Sut.GetDbFromConfiguration().HashLength(hashKey);

			// assert
			Assert.Equal(1000, result);
		}

		[Fact]
		public void HashIncerementByLong_ValueDoesntExist_EntryCreatedWithValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var incBy = 1;
			// act
			Assert.False(Sut.GetDbFromConfiguration().Database.HashExists(hashKey, entryKey));
			var result = Sut.GetDbFromConfiguration().HashIncerementBy(hashKey, entryKey, incBy);
			// assert
			Assert.Equal(incBy, result);
			Assert.True(Sut.GetDbFromConfiguration().HashExists(hashKey, entryKey));
			Assert.Equal(incBy, Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
		}

		[Fact]
		public void HashIncerementByLong_ValueExist_EntryIncrementedCorrectValueReturned()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = 15;
			var incBy = 1;
			Assert.True(Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, entryValue));

			// act
			var result = Sut.GetDbFromConfiguration().HashIncerementBy(hashKey, entryKey, incBy);

			// assert
			var expected = entryValue + incBy;
			Assert.Equal(expected, result);
			Assert.Equal(expected, Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
		}

		[Fact]
		public void HashIncerementByDouble_ValueDoesntExist_EntryCreatedWithValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var incBy = 0.9;
			// act
			Assert.False(Sut.GetDbFromConfiguration().Database.HashExists(hashKey, entryKey));
			var result = Sut.GetDbFromConfiguration().HashIncerementBy(hashKey, entryKey, incBy);
			// assert
			Assert.Equal(incBy, result);
			Assert.True(Sut.GetDbFromConfiguration().HashExists(hashKey, entryKey));
			Assert.Equal(incBy, (double)Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey), 6); // have to provide epsilon due to double error
		}

		[Fact]
		public void HashIncerementByDouble_ValueExist_EntryIncrementedCorrectValueReturned()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = 14.3;
			var incBy = 9.7;
			Assert.True(Sut.GetDbFromConfiguration().Database.HashSet(hashKey, entryKey, entryValue));

			// act
			var result = Sut.GetDbFromConfiguration().HashIncerementBy(hashKey, entryKey, incBy);

			// assert
			var expected = entryValue + incBy;
			Assert.Equal(expected, result);
			Assert.Equal(expected, Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
		}

		[Fact]
		public void HashScan_EmptyHash_ReturnEmptyCursor()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			Assert.True(Sut.GetDbFromConfiguration().Database.HashLength(hashKey) == 0);
			// act
			var result = Sut.GetDbFromConfiguration().HashScan<string>(hashKey, "*");
			// assert
			Assert.Empty(result);
		}

		[Fact]
		public void HashScan_EntriesExistUseAstrisk_ReturnCursorToAllEntries()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

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
		public void HashScan_EntriesExistUseAstrisk_ReturnCursorToAllEntriesBeginningWithTwo()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
					.ToDictionary(x => x.Key);

			Sut.GetDbFromConfiguration().Database.HashSet(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

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

		#endregion // Hash tests

		#region Hash async tests

		[Fact]
		public async Task HashSetSingleValueNXAsync_ValueDoesntExists_ShouldInsertAndRetrieveValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>("test", DateTime.UtcNow);

			// act
			var res = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, entryValue, nx: true);

			// assert
			Assert.True(res);
			var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
			Assert.Equal(entryValue, data);
		}

		[Fact]
		public async Task HashSetSingleValueNXAsync_ValueExists_ShouldNotInsertOriginalValueNotChanged()
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
			var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.GetDbFromConfiguration().Database.HashGet(hashKey, entryKey));
			Assert.Equal(initialValue, data);
		}

		[Fact]
		public async Task HashSetSingleValueAsync_ValueExists_ShouldUpdateValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
			var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
			var initRes = await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey, entryKey, Serializer.Serialize(initialValue));

			// act
			var res = await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, entryKey, entryValue, nx: false);

			// assert
			Assert.True(initRes);
			Assert.False(res); // NOTE: HSET returns: 1 if new field was created and value set, or 0 if field existed and value set. reference: http://redis.io/commands/HSET
			var data = Serializer.Deserialize<TestClass<DateTime>>(await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey));
			Assert.Equal(entryValue, data);
		}

		[Fact]
		public async Task HashSetMultipleValuesAsync_HashGetMultipleValues_ShouldInsert()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values = Range(0, 100).Select(_ => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow));
			var map = values.ToDictionary(val => Guid.NewGuid().ToString());

			// act
			await Sut.GetDbFromConfiguration().HashSetAsync(hashKey, map);

			// assert
			var data = (await Sut.GetDbFromConfiguration().Database
						.HashGetAsync(hashKey, map.Keys.Select(x => (RedisValue)x).ToArray())).ToList()
						.Select(x => Serializer.Deserialize<TestClass<DateTime>>(x))
						.ToList();

			Assert.Equal(map.Count, data.Count());
			foreach (var val in data)
			{
				Assert.True(map.ContainsValue(val), $"result map doesn't contain value: {val.Key}:{val.Value}");
			}
		}

		[Fact]
		public async Task HashDeleteAsync_KeyExists_ShouldDelete()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
			Assert.True(await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");
			// act

			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, entryKey);

			// assert
			Assert.True(result);
			Assert.True((await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey)).IsNull);
		}

		[Fact]
		public async Task HashDeleteAsync_KeyDoesntExist_ShouldReturnFalse()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			// act

			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, entryKey);

			// assert
			Assert.False(result);
			Assert.True((await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey)).IsNull);
		}

		[Fact]
		public async Task HashDeleteMultipleAsync_AllKeysExist_ShouldDeleteAll()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act

			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, values.Keys);

			// assert
			Assert.Equal(values.Count, result);
			var dbValues = await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, values.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbValues);
			Assert.DoesNotContain(dbValues, x => !x.IsNull);
			Assert.Equal(0, await Sut.GetDbFromConfiguration().Database.HashLengthAsync(hashKey));
		}

		[Fact]
		public async Task HashDeleteMultipleAsync_NotAllKeysExist_ShouldDeleteAllOnlyRequested()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var valuesDelete =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);
			var valuesKeep =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				valuesDelete.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
			   valuesKeep.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

			// act

			var result = await Sut.GetDbFromConfiguration().HashDeleteAsync(hashKey, valuesDelete.Keys);

			// assert
			Assert.Equal(valuesDelete.Count, result);
			var dbDeletedValues = await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, valuesDelete.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbDeletedValues);
			Assert.DoesNotContain(dbDeletedValues, x => !x.IsNull);
			var dbValues = await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, valuesKeep.Select(x => (RedisValue)x.Key).ToArray());
			Assert.NotNull(dbValues);
			Assert.DoesNotContain(dbValues, x => x.IsNull);
			Assert.Equal(1000, await Sut.GetDbFromConfiguration().Database.HashLengthAsync(hashKey));
			Assert.Equal(1000, dbValues.Length);
			Assert.All(dbValues, x => Assert.True(valuesKeep.ContainsKey(Sut.GetDbFromConfiguration().Serializer.Deserialize<TestClass<int>>(x).Key)));
		}

		[Fact]
		public async Task HashExistsAsync_KeyExists_ReturnTrue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
			Assert.True(await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey, entryKey, Sut.GetDbFromConfiguration().Serializer.Serialize(entryValue)), "Failed setting test value into redis");
			// act
			var result = await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey);

			// assert
			Assert.True(result, "Entry doesn't exist in hash, but it should");
		}

		[Fact]
		public async Task HashExistsAsync_KeyDoesntExists_ReturnFalse()
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
		public async Task HashKeysAsync_HashEmpty_ReturnEmptyCollection()
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
		public async Task HashKeysAsync_HashNotEmpty_ReturnKeysCollection()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
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
		public async Task HashValuesAsync_HashEmpty_ReturnEmptyCollection()
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
		public async Task HashValuesAsync_HashNotEmpty_ReturnAllValues()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
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
		public async Task HashLengthAsync_HashEmpty_ReturnZero()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();

			// act
			var result = await Sut.GetDbFromConfiguration().HashLengthAsync(hashKey);

			// assert
			Assert.Equal(0, result);
		}

		[Fact]
		public async Task HashLengthAsync_HashNotEmpty_ReturnCorrectCount()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());
			// act
			var result = await Sut.GetDbFromConfiguration().HashLengthAsync(hashKey);

			// assert
			Assert.Equal(1000, result);
		}

		[Fact]
		public async Task HashIncerementByLongAsync_ValueDoesntExist_EntryCreatedWithValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var incBy = 1;
			// act
			Assert.False(await Sut.GetDbFromConfiguration().Database.HashExistsAsync(hashKey, entryKey));
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);
			// assert
			Assert.Equal(incBy, result);
			Assert.True(await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey));
			Assert.Equal(incBy, await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey));
		}

		[Fact]
		public async Task HashIncerementByLongAsync_ValueExist_EntryIncrementedCorrectValueReturned()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = 15;
			var incBy = 1;
			Assert.True(await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey, entryKey, entryValue));

			// act
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

			// assert
			var expected = entryValue + incBy;
			Assert.Equal(expected, result);
			Assert.Equal(expected, await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey));
		}

		[Fact]
		public async Task HashIncerementByDoubleAsync_ValueDoesntExist_EntryCreatedWithValue()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var incBy = 0.9;
			// act
			Assert.False(await Sut.GetDbFromConfiguration().Database.HashExistsAsync(hashKey, entryKey));
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);
			// assert
			Assert.Equal(incBy, result);
			Assert.True(await Sut.GetDbFromConfiguration().HashExistsAsync(hashKey, entryKey));
			Assert.Equal(incBy, (double)await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey), 6); // have to provide epsilon due to double error
		}

		[Fact]
		public async Task HashIncerementByDoubleAsync_ValueExist_EntryIncrementedCorrectValueReturned()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var entryKey = Guid.NewGuid().ToString();
			var entryValue = 14.3;
			var incBy = 9.7;
			Assert.True(await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey, entryKey, entryValue));

			// act
			var result = await Sut.GetDbFromConfiguration().HashIncerementByAsync(hashKey, entryKey, incBy);

			// assert
			var expected = entryValue + incBy;
			Assert.Equal(expected, result);
			Assert.Equal(expected, await Sut.GetDbFromConfiguration().Database.HashGetAsync(hashKey, entryKey));
		}

		[Fact]
		public async Task HashScanAsync_EmptyHash_ReturnEmptyCursor()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			Assert.True(await Sut.GetDbFromConfiguration().Database.HashLengthAsync(hashKey) == 0);
			// act
			var result = await Sut.GetDbFromConfiguration().HashScanAsync<string>(hashKey, "*");
			// assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task HashScanAsync_EntriesExistUseAstrisk_ReturnCursorToAllEntries()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

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
		public async Task HashScanAsync_EntriesExistUseAstrisk_ReturnCursorToAllEntriesBeginningWithTwo()
		{
			// arrange
			var hashKey = Guid.NewGuid().ToString();
			var values =
				Enumerable.Range(0, 1000)
					.Select(x => new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow))
					.ToDictionary(x => x.Key);

			await Sut.GetDbFromConfiguration().Database.HashSetAsync(hashKey,
				values.Select(x => new HashEntry(x.Key, Sut.GetDbFromConfiguration().Serializer.Serialize(x.Value))).ToArray());

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

		#endregion // Hash async tests
		
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
	
		#endregion // Sorted tests
	}
}