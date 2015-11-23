using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Tests.Extensions;
using StackExchange.Redis.Extensions.Tests.Helpers;
using Xunit;

namespace StackExchange.Redis.Extensions.Tests
{
	[Collection("Redis")]
	public abstract class CacheClientTestBase : IDisposable
	{
		protected readonly StackExchangeRedisCacheClient Sut;
		protected readonly IDatabase Db;
		protected ISerializer Serializer;

		protected CacheClientTestBase(ISerializer serializer)
		{
			Serializer = serializer;
			Sut = new StackExchangeRedisCacheClient(Serializer);
			Db = Sut.Database;
		}

		[Fact]
		public void Info_Should_Return_Valid_Informatino()
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
			TestClass<DateTime> testobject = new TestClass<DateTime>();

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

			bool added = Sut.AddAll(values);

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

			IDictionary<string, string> result = Sut.GetAll<string>(new[] { values[0].Key, values[1].Key, values[2].Key, "notexistingkey" });

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
				Sut.SetAdd("MySet", x.Key);
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

			for (int i = 0; i < values.Count; i++)
			{
				TestClass<string> value = values[i];
				Assert.Equal(value.Key,cached[value.Key].Key);
				Assert.Equal(value.Value,cached[value.Key].Value);
			}
		}

		[Fact]
		public void Adding_Value_Type_Should_Return_Correct_Value()
		{
			int d = 1;
			bool added = Sut.Add("my Key", d);
			int dbValue = Sut.Get<int>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue,d);
		}

		[Fact]
		public void Adding_Collection_To_Redis_Should_Work_Correctly()
		{
			Collection<TestClass<string>> items = new Collection<TestClass<string>>();
            items.Add(new TestClass<string>() {Key = "key1",Value = "key1"});
            items.Add(new TestClass<string>() {Key = "key2",Value = "key2"});
            items.Add(new TestClass<string>() {Key = "key3",Value = "key3"});
			
			bool added = Sut.Add("my Key", items);
			var dbValue = Sut.Get<Collection<TestClass<string>>>("my Key");

			Assert.True(added);
			Assert.True(Db.KeyExists("my Key"));
			Assert.Equal(dbValue.Count, items.Count);
			for (int i = 0; i < items.Count; i++)
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

		public void Dispose()
		{
			Db.FlushDatabase();
			Db.Multiplexer.GetSubscriber().UnsubscribeAll();
            Db.Multiplexer.Dispose();
			Sut.Dispose();
		}
	}
}
