using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Extensions;
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
		protected readonly StackExchangeRedisCacheClient Sut;
		protected ISerializer Serializer;

		protected CacheClientTestBase(ISerializer serializer)
		{
			Serializer = serializer;
		    var mux = ConnectionMultiplexer.Connect(new ConfigurationOptions
		    {
		        DefaultVersion = new Version(3, 0, 500),
		        EndPoints = {{"localhost", 6379}},
		        AllowAdmin = true
		    });

            Sut = new StackExchangeRedisCacheClient(mux, Serializer);
			Db = Sut.Database;
		}

		public void Dispose()
		{
			Db.FlushDatabase();
			Db.Multiplexer.GetSubscriber().UnsubscribeAll();
			Db.Multiplexer.Dispose();
			Sut.Dispose();
		}

		[Fact]
		public void Info_Should_Return_Valid_Information()
		{
			var response = Sut.GetInfo();

			Assert.NotNull(response);
			Assert.True(response.Any());
			Assert.Equal(response["os"], "Windows");
			Assert.Equal(response["tcp_port"], "6379");
		}

		[Fact]
		public void Add_Item_To_Redis_Database()
		{
			var added = Sut.Add("my Key", "my value");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
		}

		[Fact]
		public void Add_Complex_Item_To_Redis_Database()
		{
			var testobject = new TestClass<DateTime>();

			var added = Sut.Add("my Key", testobject);

			var result = Db.StringGet("my Key");

			Assert.True(added);
			Assert.NotNull(result);

			var obj = Serializer.Deserialize<TestClass<DateTime>>(result);

			Assert.True(Db.KeyExists("my Key"));
			Assert.NotNull(obj);
			Assert.Equal(testobject.Key, obj.Key);
			Assert.Equal(testobject.Value.ToUniversalTime(), obj.Value.ToUniversalTime());
		}

		[Fact]
		public void Add_Multiple_Object_With_A_Single_Roundtrip_To_Redis_Must_Store_Data_Correctly_Into_Database()
		{
			IList<Tuple<string, string>> values = new List<Tuple<string, string>>();
			values.Add(new Tuple<string, string>("key1", "value1"));
			values.Add(new Tuple<string, string>("key2", "value2"));
			values.Add(new Tuple<string, string>("key3", "value3"));

			var added = Sut.AddAll(values);

			Assert.True(added);

			Assert.True(Db.KeyExists("key1"));
			Assert.True(Db.KeyExists("key2"));
			Assert.True(Db.KeyExists("key3"));

			Assert.Equal(Serializer.Deserialize<string>(Db.StringGet("key1")), "value1");
			Assert.Equal(Serializer.Deserialize<string>(Db.StringGet("key2")), "value2");
			Assert.Equal(Serializer.Deserialize<string>(Db.StringGet("key3")), "value3");
		}

		[Fact]
		public void Get_All_Should_Return_All_Database_Keys()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();
			values.ForEach(x => Db.StringSet(x.Key, Serializer.Serialize(x.Value)));

			var result = Sut.GetAll<string>(new[] {values[0].Key, values[1].Key, values[2].Key, "notexistingkey"});

			Assert.True(result.Count() == 4);
			Assert.Equal(result[values[0].Key], values[0].Value);
			Assert.Equal(result[values[1].Key], values[1].Value);
			Assert.Equal(result[values[2].Key], values[2].Value);
			Assert.Null(result["notexistingkey"]);
		}

		[Fact]
		public void Get_With_Complex_Item_Should_Return_Correct_Value()
		{
			var value = Builder<ComplexClassForTest<string, string>>
				.CreateListOfSize(1)
				.All()
				.Build().First();

			Db.StringSet(value.Item1, Serializer.Serialize(value));

			var cachedObject = Sut.Get<ComplexClassForTest<string, string>>(value.Item1);

			Assert.NotNull(cachedObject);
			Assert.Equal(value.Item1, cachedObject.Item1);
			Assert.Equal(value.Item2, cachedObject.Item2);
		}

		[Fact]
		public void Remove_All_Should_Remove_All_Specified_Keys()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			Sut.RemoveAll(values.Select(x => x.Key));

			foreach (var value in values)
			{
				Assert.False(Db.KeyExists(value.Key));
			}
		}

		[Fact]
		public void Search_With_Valid_Start_With_Pattern_Should_Return_Correct_Keys()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(20)
				.Build();
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			var key = Sut.SearchKeys("Key1*").ToList();

			Assert.True(key.Count == 11);
		}

		[Fact]
		public void Exist_With_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(2)
				.Build();
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			Assert.True(Sut.Exists(values[0].Key));
		}

		[Fact]
		public void Exist_With_Not_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(2)
				.Build();
			values.ForEach(x => Db.StringSet(x.Key, x.Value));

			Assert.False(Sut.Exists("this key doesn not exist into redi"));
		}

		[Fact]
		public void SetAdd_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				Sut.SetAdd<string>("MySet", x.Key);
			});

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void SetMember_With_Valid_Data_Should_Return_Correct_Keys()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				Db.SetAdd("MySet", x.Key);
			});

			var keys = Sut.SetMember("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void Massive_Add_Should_Not_Throw_Exception_And_Work_Correctly()
		{
			const int size = 3000;
			var values = Builder<TestClass<string>>
				.CreateListOfSize(size)
				.All()
				.Build();

			var tupleValues = values.Select(x => new Tuple<string, TestClass<string>>(x.Key, x)).ToList();
			var result = Sut.AddAll(tupleValues);
			var cached = Sut.GetAll<TestClass<string>>(values.Select(x => x.Key));

			Assert.True(result);
			Assert.NotNull(cached);
			Assert.Equal(size, cached.Count);

			for (var i = 0; i < values.Count; i++)
			{
				var value = values[i];
				Assert.Equal(value.Key, cached[value.Key].Key);
				Assert.Equal(value.Value, cached[value.Key].Value);
			}
		}

		[Fact]
		public void Adding_Value_Type_Should_Return_Correct_Value()
		{
			var d = 1;
			var added = Sut.Add("my Key", d);
			var dbValue = Sut.Get<int>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue, d);
		}

		[Fact]
		public void Adding_Collection_To_Redis_Should_Work_Correctly()
		{
			var items = new Collection<TestClass<string>>();
			items.Add(new TestClass<string> {Key = "key1", Value = "key1"});
			items.Add(new TestClass<string> {Key = "key2", Value = "key2"});
			items.Add(new TestClass<string> {Key = "key3", Value = "key3"});

			var added = Sut.Add("my Key", items);
			var dbValue = Sut.Get<Collection<TestClass<string>>>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue.Count, items.Count);
			for (var i = 0; i < items.Count; i++)
			{
				Assert.Equal(dbValue[i].Value, items[i].Value);
				Assert.Equal(dbValue[i].Key, items[i].Key);
			}
		}

		[Fact]
		public async Task Pub_Sub()
		{
			var message = Enumerable.Range(0, 10).ToArray();
			const string channel = "unit_test";
			var subscriberNotified = false;
			IEnumerable<int> subscriberValue = null;

			var action = new Action<IEnumerable<int>>(value =>
			{
				subscriberNotified = true;
				subscriberValue = value;
			});

			Sut.Subscribe(channel, action);

			var result = Sut.Publish("unit_test", message);

			await Task.Run(() =>
			{
				while (!subscriberNotified)
				{
					Thread.Sleep(100);
				}
			});

			//TODO:need to understand why return 2 instead of 1
			//Assert.Equal(1, result);
			Assert.True(subscriberNotified);
			Assert.Equal(message, subscriberValue);
		}

		[Fact]
		public void SetAddGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			Assert.Throws<ArgumentException>(() => Sut.SetAdd<string>(string.Empty, string.Empty));
		}

		[Fact]
		public void SetAddGenericShouldThrowExceptionWhenItemIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Sut.SetAdd<string>("MySet", null));
		}

		[Fact]
		public void SetAddGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				Sut.SetAdd("MySet", x);
			});

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void SetAddAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			var exceptions = Sut.SetAddAsync<string>(string.Empty, string.Empty).Exception;
			Assert.IsType<ArgumentException>(exceptions.Flatten().GetBaseException());
		}

		[Fact]
		public void SetAddAsyncGenericShouldThrowExceptionWhenItemIsNull()
		{
			var exceptions = Sut.SetAddAsync<string>("MySet", null).Exception;
			Assert.IsType<ArgumentNullException>(exceptions.Flatten().GetBaseException());
		}

		[Fact]
		public void SetAddAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();

			values.ForEach(x =>
			{
				Db.StringSet(x.Key, Serializer.Serialize(x.Value));
				var result = Sut.SetAddAsync("MySet", x).Result;
			});

			var keys = Db.SetMembers("MySet");

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void ListAddToLeftGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			Assert.Throws<ArgumentException>(() => Sut.ListAddToLeft(string.Empty, string.Empty));
		}

		[Fact]
		public void ListAddToLeftGenericShouldThrowExceptionWhenItemIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Sut.ListAddToLeft<string>("MyList", null));
		}

		[Fact]
		public void ListAddToLeftGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();

			var key = "MyList";

			values.ForEach(x => { Sut.ListAddToLeft(key, Serializer.Serialize(x)); });

			var keys = Db.ListRange(key);

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void ListAddToLeftAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			var exceptions = Sut.ListAddToLeftAsync(string.Empty, string.Empty).Exception;
			Assert.IsType<ArgumentException>(exceptions.Flatten().GetBaseException());
		}

		[Fact]
		public void ListAddToLeftAsyncGenericShouldThrowExceptionWhenItemIsNull()
		{
			var exceptions = Sut.ListAddToLeftAsync<string>("MyList", null).Exception;
			Assert.IsType<ArgumentNullException>(exceptions.Flatten().GetBaseException());
		}

		[Fact]
		public void ListAddToLeftAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(5)
				.All()
				.Build();

			var key = "MyListAsync";

			values.ForEach(x => { var result = Sut.ListAddToLeftAsync(key, Serializer.Serialize(x)).Result; });

			var keys = Db.ListRange(key);

			Assert.Equal(keys.Length, values.Count);
		}

		[Fact]
		public void ListGetFromRightGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			Assert.Throws<ArgumentException>(() => Sut.ListGetFromRight<string>(string.Empty));
		}

		[Fact]
		public void ListGetFromRightGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(1)
				.All()
				.Build();

			var key = "MyList";

			values.ForEach(x => { Db.ListLeftPush(key, Serializer.Serialize(x)); });

			var item = Sut.ListGetFromRight<TestClass<string>>(key);

			Assert.Equal(item.Key, values[0].Key);
			Assert.Equal(item.Value, values[0].Value);
		}

		[Fact]
		public void ListGetFromRightAsyncGenericShouldThrowExceptionWhenKeyIsEmpty()
		{
			var exceptions = Sut.ListGetFromRightAsync<string>(string.Empty).Exception;
			Assert.IsType<ArgumentException>(exceptions.Flatten().GetBaseException());
		}

		[Fact]
		public void ListGetFromRightAsyncGeneric_With_An_Existing_Key_Should_Return_Valid_Data()
		{
			var values = Builder<TestClass<string>>
				.CreateListOfSize(1)
				.All()
				.Build();

			var key = "MyList";

			values.ForEach(x => { Db.ListLeftPush(key, Serializer.Serialize(x)); });

			var item = Sut.ListGetFromRightAsync<TestClass<string>>(key).Result;

			Assert.Equal(item.Key, values[0].Key);
			Assert.Equal(item.Value, values[0].Value);
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
            var res = Sut.HashSet(hashKey, entryKey, entryValue, nx: true);

            // assert
            Assert.True(res);
            var data = Serializer.Deserialize<TestClass <DateTime>>(Sut.Database.HashGet(hashKey, entryKey));
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
            var initRes = Sut.HashSet(hashKey, entryKey, initialValue);

            // act
            var res = Sut.HashSet(hashKey, entryKey, entryValue, nx: true);

            // assert
            Assert.True(initRes);
            Assert.False(res);
            var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.Database.HashGet(hashKey, entryKey));
            Assert.Equal(initialValue, data);
        }

        [Fact] // TODO: NX doesn't work for some reason
        public void HashSetSingleValue_ValueExists_ShouldUpdateValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
            var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
            var initRes = Sut.Database.HashSet(hashKey, entryKey, Serializer.Serialize(initialValue));

            // act
            var res = Sut.HashSet(hashKey, entryKey, entryValue, true);

            // assert
            Assert.True(initRes);
            Assert.True(res);
            var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.Database.HashGet(hashKey, entryKey));
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
            Sut.HashSet(hashKey, map);
            Thread.Sleep(500);
            // assert
            var data = Sut.Database
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
            Assert.True(Sut.Database.HashSet(hashKey, entryKey, Sut.Serializer.Serialize(entryValue)), "Failed setting test value into redis");
            // act

            var result = Sut.HashDelete(hashKey, entryKey);

            // assert
            Assert.True(result);
            Assert.True(Sut.Database.HashGet(hashKey,entryKey).IsNull);
        }

        [Fact]
        public void HashDelete_KeyDoesntExist_ShouldReturnFalse()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            // act

            var result = Sut.HashDelete(hashKey, entryKey);

            // assert
            Assert.False(result);
            Assert.True(Sut.Database.HashGet(hashKey, entryKey).IsNull);
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

            Sut.Database.HashSet(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act

            var result = Sut.HashDelete(hashKey, values.Keys);

            // assert
            Assert.Equal(values.Count, result);
            var dbValues = Sut.Database.HashGet(hashKey, values.Select(x => (RedisValue) x.Key).ToArray());
            Assert.NotNull(dbValues);
            Assert.False(dbValues.Any(x => !x.IsNull));
            Assert.Equal(0, Sut.Database.HashLength(hashKey));
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

            Sut.Database.HashSet(hashKey,
                valuesDelete.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            Sut.Database.HashSet(hashKey,
               valuesKeep.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act

            var result = Sut.HashDelete(hashKey, valuesDelete.Keys);

            // assert
            Assert.Equal(valuesDelete.Count, result);
            var dbDeletedValues = Sut.Database.HashGet(hashKey, valuesDelete.Select(x => (RedisValue)x.Key).ToArray());
            Assert.NotNull(dbDeletedValues);
            Assert.False(dbDeletedValues.Any(x => !x.IsNull));
            var dbValues = Sut.Database.HashGet(hashKey, valuesKeep.Select(x => (RedisValue)x.Key).ToArray());
            Assert.NotNull(dbValues);
            Assert.False(dbValues.Any(x => x.IsNull));
            Assert.Equal(1000, Sut.Database.HashLength(hashKey));
            Assert.Equal(1000, dbValues.Length);
            Assert.All(dbValues, x => Assert.True(valuesKeep.ContainsKey(Sut.Serializer.Deserialize<TestClass<int>>(x).Key)));
        }

        [Fact]
        public void HashExists_KeyExists_ReturnTrue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
            Assert.True(Sut.Database.HashSet(hashKey, entryKey, Sut.Serializer.Serialize(entryValue)), "Failed setting test value into redis");
            // act
            var result = Sut.HashExists(hashKey, entryKey);

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
            var result = Sut.HashExists(hashKey, entryKey);
            // assert
            Assert.False(result, "Entry doesn't exist in hash, but call returned true");
        }

        [Fact]
        public void HashKeys_HashEmpty_ReturnEmptyCollection()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            // act
            var result = Sut.HashKeys(hashKey);
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

            Sut.Database.HashSet(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            // act
            var result = Sut.HashKeys(hashKey);
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
            var result = Sut.HashValues<string>(hashKey);
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

            Sut.Database.HashSet(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            // act
            var result = Sut.HashValues<TestClass<DateTime>>(hashKey);
            // assert
            Assert.NotNull(result);
            var collection = result as IList<TestClass<DateTime>> ?? result.ToList();
            Assert.NotEmpty(collection);
            Assert.Equal(values.Count, collection.Count());
            foreach (var key in collection)
            {
                Assert.True(values.Values.Contains(key));
            }
        }

        [Fact]
        public void HashLength_HashEmpty_ReturnZero()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();

            // act
            var result = Sut.HashLength(hashKey);

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

            Sut.Database.HashSet(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            // act
            var result = Sut.HashLength(hashKey);

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
            Assert.False(Sut.Database.HashExists(hashKey,entryKey));
            var result = Sut.HashIncerementBy(hashKey, entryKey, incBy);
            // assert
            Assert.Equal(incBy, result);
            Assert.True(Sut.HashExists(hashKey,entryKey));
            Assert.Equal(incBy, Sut.Database.HashGet(hashKey,entryKey));
        }

        [Fact]
        public void HashIncerementByLong_ValueExist_EntryIncrementedCorrectValueReturned()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = 15;
            var incBy = 1;
            Assert.True(Sut.Database.HashSet(hashKey, entryKey, entryValue));
            
            // act
            var result = Sut.HashIncerementBy(hashKey, entryKey, incBy);
            
            // assert
            var expected = entryValue + incBy;
            Assert.Equal(expected, result);
            Assert.Equal(expected, Sut.Database.HashGet(hashKey, entryKey));
        }

        [Fact]
        public void HashIncerementByDouble_ValueDoesntExist_EntryCreatedWithValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var incBy = 0.9;
            // act
            Assert.False(Sut.Database.HashExists(hashKey, entryKey));
            var result = Sut.HashIncerementBy(hashKey, entryKey, incBy);
            // assert
            Assert.Equal(incBy, result);
            Assert.True(Sut.HashExists(hashKey, entryKey));
            Assert.Equal(incBy, (double)Sut.Database.HashGet(hashKey, entryKey), 6); // have to provide epsilon due to double error
        }

        [Fact]
        public void HashIncerementByDouble_ValueExist_EntryIncrementedCorrectValueReturned()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = 14.3;
            var incBy = 9.7;
            Assert.True(Sut.Database.HashSet(hashKey, entryKey, entryValue));

            // act
            var result = Sut.HashIncerementBy(hashKey, entryKey, incBy);

            // assert
            var expected = entryValue + incBy;
            Assert.Equal(expected, result);
            Assert.Equal(expected, Sut.Database.HashGet(hashKey, entryKey));
        }

        [Fact]
        public void HashScan_EmptyHash_ReturnEmptyCursor()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            Assert.True(Sut.Database.HashLength(hashKey) == 0);
            // act
            var result = Sut.HashScan<string>(hashKey, "*");
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

            Sut.Database.HashSet(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            
            // act
            var result = Sut.HashScan<TestClass<DateTime>>(hashKey, "*");
            
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

            Sut.Database.HashSet(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = Sut.HashScan<TestClass<DateTime>>(hashKey, "2*");

            // assert
            Assert.NotNull(result);
            var resultEnum = result.ToDictionary(x => x.Key, x => x.Value);
            Assert.Equal(values.Keys.Count(x => x.StartsWith("2")), resultEnum.Count);
            foreach (var key in values.Keys.Where(x => x.StartsWith("2")))
            {
                Assert.True(resultEnum.ContainsKey(key));
                Assert.Equal(values[key], resultEnum[key]);
            }
        }

        // async variants

        /*
	    [Fact]
	    public void Method_State_ExpectedResult()
	    {
            // arrange
            // act
            // assert
        }
        */
        #endregion // Hash tests

        #region Hash async tests

        [Fact]
        public async Task HashSetSingleValueNXASync_ValueDoesntExists_ShouldInsertAndRetrieveValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>("test", DateTime.UtcNow);

            // act
            var res = await Sut.HashSetAsync(hashKey, entryKey, entryValue, nx: true);

            // assert
            Assert.True(res);
            var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.Database.HashGet(hashKey, entryKey));
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
            var initRes = await Sut.HashSetAsync(hashKey, entryKey, initialValue);

            // act
            var res = await Sut.HashSetAsync(hashKey, entryKey, entryValue, nx: true);

            // assert
            Assert.True(initRes);
            Assert.False(res);
            var data = Serializer.Deserialize<TestClass<DateTime>>(Sut.Database.HashGet(hashKey, entryKey));
            Assert.Equal(initialValue, data);
        }

        [Fact] // TODO: NX doesn't work for some reason
        public async Task HashSetSingleValueAsync_ValueExists_ShouldUpdateValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>("test1", DateTime.UtcNow);
            var initialValue = new TestClass<DateTime>("test2", DateTime.UtcNow);
            var initRes = await Sut.Database.HashSetAsync(hashKey, entryKey, Serializer.Serialize(initialValue));

            // act
            var res = await Sut.HashSetAsync(hashKey, entryKey, entryValue, true);

            // assert
            Assert.True(initRes);
            Assert.True(res);
            var data = Serializer.Deserialize<TestClass<DateTime>>(await Sut.Database.HashGetAsync(hashKey, entryKey));
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
            await Sut.HashSetAsync(hashKey, map);

            // assert
            var data = (await Sut.Database
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
            Assert.True(await Sut.Database.HashSetAsync(hashKey, entryKey, Sut.Serializer.Serialize(entryValue)), "Failed setting test value into redis");
            // act

            var result = await Sut.HashDeleteAsync (hashKey, entryKey);

            // assert
            Assert.True(result);
            Assert.True((await Sut.Database.HashGetAsync(hashKey, entryKey)).IsNull);
        }

        [Fact]
        public async Task HashDeleteAsync_KeyDoesntExist_ShouldReturnFalse()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            // act

            var result = await Sut.HashDeleteAsync(hashKey, entryKey);

            // assert
            Assert.False(result);
            Assert.True((await Sut.Database.HashGetAsync(hashKey, entryKey)).IsNull);
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

            await Sut.Database.HashSetAsync(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act

            var result = await Sut.HashDeleteAsync(hashKey, values.Keys);

            // assert
            Assert.Equal(values.Count, result);
            var dbValues = await Sut.Database.HashGetAsync(hashKey, values.Select(x => (RedisValue)x.Key).ToArray());
            Assert.NotNull(dbValues);
            Assert.False(dbValues.Any(x => !x.IsNull));
            Assert.Equal(0, await Sut.Database.HashLengthAsync(hashKey));
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

            await Sut.Database.HashSetAsync(hashKey,
                valuesDelete.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            await Sut.Database.HashSetAsync(hashKey,
               valuesKeep.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act

            var result = await Sut.HashDeleteAsync(hashKey, valuesDelete.Keys);

            // assert
            Assert.Equal(valuesDelete.Count, result);
            var dbDeletedValues = await Sut.Database.HashGetAsync(hashKey, valuesDelete.Select(x => (RedisValue)x.Key).ToArray());
            Assert.NotNull(dbDeletedValues);
            Assert.False(dbDeletedValues.Any(x => !x.IsNull));
            var dbValues = await Sut.Database.HashGetAsync(hashKey, valuesKeep.Select(x => (RedisValue)x.Key).ToArray());
            Assert.NotNull(dbValues);
            Assert.False(dbValues.Any(x => x.IsNull));
            Assert.Equal(1000, await Sut.Database.HashLengthAsync(hashKey));
            Assert.Equal(1000, dbValues.Length);
            Assert.All(dbValues, x => Assert.True(valuesKeep.ContainsKey(Sut.Serializer.Deserialize<TestClass<int>>(x).Key)));
        }

        [Fact]
        public async Task HashExistsAsync_KeyExists_ReturnTrue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = new TestClass<DateTime>(Guid.NewGuid().ToString(), DateTime.UtcNow);
            Assert.True(await Sut.Database.HashSetAsync(hashKey, entryKey, Sut.Serializer.Serialize(entryValue)), "Failed setting test value into redis");
            // act
            var result = await Sut.HashExistsAsync(hashKey, entryKey);

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
            var result = await Sut.HashExistsAsync(hashKey, entryKey);
            // assert
            Assert.False(result, "Entry doesn't exist in hash, but call returned true");
        }

        [Fact]
        public async Task HashKeysAsync_HashEmpty_ReturnEmptyCollection()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            // act
            var result = await Sut.HashKeysAsync(hashKey);
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
                Enumerable.Range(0, 1000)
                    .Select(x => new TestClass<int>(Guid.NewGuid().ToString(), x))
                    .ToDictionary(x => x.Key);

            await Sut.Database.HashSetAsync(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            // act
            var result = await Sut.HashKeysAsync(hashKey);
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
            var result = await Sut.HashValuesAsync<string>(hashKey);
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

            await Sut.Database.HashSetAsync(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            // act
            var result = await Sut.HashValuesAsync<TestClass<DateTime>>(hashKey);
            // assert
            Assert.NotNull(result);
            var collection = result as IList<TestClass<DateTime>> ?? result.ToList();
            Assert.NotEmpty(collection);
            Assert.Equal(values.Count, collection.Count());
            foreach (var key in collection)
            {
                Assert.True(values.Values.Contains(key));
            }
        }

        [Fact]
        public async Task HashLengthAsync_HashEmpty_ReturnZero()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();

            // act
            var result = await Sut.HashLengthAsync(hashKey);

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

            await Sut.Database.HashSetAsync(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());
            // act
            var result = await Sut.HashLengthAsync(hashKey);

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
            Assert.False(await Sut.Database.HashExistsAsync(hashKey, entryKey));
            var result = await Sut.HashIncerementByAsync(hashKey, entryKey, incBy);
            // assert
            Assert.Equal(incBy, result);
            Assert.True(await Sut.HashExistsAsync(hashKey, entryKey));
            Assert.Equal(incBy, await Sut.Database.HashGetAsync(hashKey, entryKey));
        }

        [Fact]
        public async Task HashIncerementByLongAsync_ValueExist_EntryIncrementedCorrectValueReturned()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = 15;
            var incBy = 1;
            Assert.True(await Sut.Database.HashSetAsync(hashKey, entryKey, entryValue));

            // act
            var result = await Sut.HashIncerementByAsync(hashKey, entryKey, incBy);

            // assert
            var expected = entryValue + incBy;
            Assert.Equal(expected, result);
            Assert.Equal(expected, await Sut.Database.HashGetAsync(hashKey, entryKey));
        }

        [Fact]
        public async Task HashIncerementByDoubleAsync_ValueDoesntExist_EntryCreatedWithValue()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var incBy = 0.9;
            // act
            Assert.False(await Sut.Database.HashExistsAsync(hashKey, entryKey));
            var result = await Sut.HashIncerementByAsync(hashKey, entryKey, incBy);
            // assert
            Assert.Equal(incBy, result);
            Assert.True(await Sut.HashExistsAsync(hashKey, entryKey));
            Assert.Equal(incBy, (double)await Sut.Database.HashGetAsync(hashKey, entryKey), 6); // have to provide epsilon due to double error
        }

        [Fact]
        public async Task HashIncerementByDoubleAsync_ValueExist_EntryIncrementedCorrectValueReturned()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            var entryKey = Guid.NewGuid().ToString();
            var entryValue = 14.3;
            var incBy = 9.7;
            Assert.True(await Sut.Database.HashSetAsync(hashKey, entryKey, entryValue));

            // act
            var result = await Sut.HashIncerementByAsync(hashKey, entryKey, incBy);

            // assert
            var expected = entryValue + incBy;
            Assert.Equal(expected, result);
            Assert.Equal(expected, await Sut.Database.HashGetAsync(hashKey, entryKey));
        }

        [Fact]
        public async Task HashScanAsync_EmptyHash_ReturnEmptyCursor()
        {
            // arrange
            var hashKey = Guid.NewGuid().ToString();
            Assert.True(await Sut.Database.HashLengthAsync(hashKey) == 0);
            // act
            var result = await Sut.HashScanAsync<string>(hashKey, "*");
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

            await Sut.Database.HashSetAsync(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.HashScanAsync<TestClass<DateTime>>(hashKey, "*");

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

            await Sut.Database.HashSetAsync(hashKey,
                values.Select(x => new HashEntry(x.Key, Sut.Serializer.Serialize(x.Value))).ToArray());

            // act
            var result = await Sut.HashScanAsync<TestClass<DateTime>>(hashKey, "2*");

            // assert
            Assert.NotNull(result);
            var resultEnum = result.ToDictionary(x => x.Key, x => x.Value);
            Assert.Equal(values.Keys.Count(x => x.StartsWith("2")), resultEnum.Count);
            foreach (var key in values.Keys.Where(x => x.StartsWith("2")))
            {
                Assert.True(resultEnum.ContainsKey(key));
                Assert.Equal(values[key], resultEnum[key]);
            }
        }

        #endregion // Hash async tests
    }
}