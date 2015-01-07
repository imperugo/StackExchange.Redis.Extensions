using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using StackExchange.Redis.Extensions.Tests.Extensions;
using StackExchange.Redis.Extensions.Tests.Helpers;
using Xunit;

namespace StackExchange.Redis.Extensions.Tests
{
	public class StackExchangeRedisCacheClientTest : IDisposable
	{
		private readonly StackExchangeRedisCacheClient sut;
		private readonly IDatabase db;
		private readonly ISerializer serializer;

		public StackExchangeRedisCacheClientTest()
		{
			var connectionString = string.Format("{0}:{1},ssl=true,password={2}", "localhost", 6380, "mypsw");
			var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
			db = connectionMultiplexer.GetDatabase();
			serializer = new TestItemSerializer();
			sut = new StackExchangeRedisCacheClient(connectionMultiplexer, serializer);
		}

		public void Dispose()
		{
			db.FlushDatabase();
			db.Multiplexer.Dispose();
			sut.Dispose();
		}

		[Fact]
		public void Add_Item_To_Redis_Database()
		{
			var added = sut.Add("my Key", "my value");

			Assert.True(added);
			Assert.True(db.KeyExists("my Key"));
		}

		[Fact]
		public void Add_Complex_Item_To_Redis_Database()
		{
			TestClass<DateTime> testobject = new TestClass<DateTime>();

			var added = sut.Add("my Key", testobject);

			var result = db.StringGet("my Key");

			Assert.True(added);
			Assert.NotNull(result);

			var obj = serializer.Deserialize<TestClass<DateTime>>(result);

			Assert.True(db.KeyExists("my Key"));
			Assert.NotNull(obj);
			Assert.Equal(testobject.Key, obj.Key);
			Assert.Equal(testobject.Value, obj.Value);
		}

		[Fact]
		public void Get_All_Should_Return_All_Database_Keys()
		{
			var values = Builder<TestClass<string>>
						.CreateListOfSize(5)
						.All()
						.Build();
			values.ForEach(x => db.StringSet(x.Key, serializer.Serialize(x.Value)));

			IDictionary<string, string> result = sut.GetAll<string>(new[] { values[0].Key, values[1].Key, values[2].Key, "notexistingkey" });

			Assert.True(result.Count() == 4);
			Assert.Equal(result[ values[0].Key], values[0].Value);
			Assert.Equal(result[ values[1].Key], values[1].Value);
			Assert.Equal(result[ values[2].Key], values[2].Value);
			Assert.Null(result["notexistingkey"]);
		}

		[Fact]
		public void Remove_All_Should_Remove_All_Specified_Keys()
		{
			var values = Builder<TestClass<string>>
						.CreateListOfSize(5)
						.All()
						.Build();
			values.ForEach(x => db.StringSet(x.Key,x.Value));

			sut.RemoveAll(values.Select(x => x.Key));

			foreach (var value in values)
			{
				Assert.False(db.KeyExists(value.Key));
			}
		}

		[Fact]
		public void Search_With_Valid_Start_With_Pattern_Should_Return_Correct_Keys()
		{
			var values = Builder<TestClass<string>>
						.CreateListOfSize(20)
						.Build();
			values.ForEach(x => db.StringSet(x.Key, x.Value));

			var key = sut.SearchKeys("Key1*").ToList();

			Assert.True(key.Count == 11);
		}

		[Fact]
		public void Exist_With_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Builder<TestClass<string>>
					.CreateListOfSize(2)
					.Build();
			values.ForEach(x => db.StringSet(x.Key, x.Value));

			Assert.True(sut.Exists(values[0].Key));
		}

		[Fact]
		public void Exist_With_Not_Valid_Object_Should_Return_The_Correct_Instance()
		{
			var values = Builder<TestClass<string>>
					.CreateListOfSize(2)
					.Build();
			values.ForEach(x => db.StringSet(x.Key, x.Value));

			Assert.False(sut.Exists("this key doesn not exist into redi"));
		}
	}
}
