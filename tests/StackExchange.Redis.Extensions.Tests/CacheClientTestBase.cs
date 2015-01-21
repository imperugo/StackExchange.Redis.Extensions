using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Tests.Extensions;
using StackExchange.Redis.Extensions.Tests.Helpers;
using Xunit;

namespace StackExchange.Redis.Extensions.Tests
{
    public class CacheClientTestBase : IDisposable
    {
        private readonly StackExchangeRedisCacheClient sut;
        private readonly IDatabase db;
        protected ISerializer Serializer;

        public virtual void OnInitialize() { }
        public CacheClientTestBase()
        {
            //Hostname = "bmwremarketingqadev.redis.cache.windows.net";
            //Password = "hGRkEYhVGv2TMfA2aVoHdjn6y+7qslGFnV4vbuoJvJM=";
            //Port = 6380;

            //var connectionString = string.Format("{0}:{1},ssl=true,password={2}", "localhost", 6380, "password");
            var connectionString = string.Format("{0}:{1}", "192.168.59.103", 6379);
            var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            db = connectionMultiplexer.GetDatabase();

            OnInitialize();

            sut = new StackExchangeRedisCacheClient(connectionMultiplexer, Serializer);
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

            var obj = Serializer.Deserialize<TestClass<DateTime>>(result);

            Assert.True(db.KeyExists("my Key"));
            Assert.NotNull(obj);
            Assert.Equal(testobject.Key, obj.Key);
            Assert.Equal(testobject.Value, obj.Value);
        }

        [Fact]
        public void Add_Multiple_Object_With_A_Single_Roundtrip_To_Redis_Must_Store_Data_Correctly_Into_Database()
        {
            IList<Tuple<string, string>> values = new List<Tuple<string, string>>();
            values.Add(new Tuple<string, string>("key1", "value1"));
            values.Add(new Tuple<string, string>("key2", "value2"));
            values.Add(new Tuple<string, string>("key3", "value3"));

            bool added = sut.AddAll(values);

            Assert.True(added);

            Assert.True(db.KeyExists("key1"));
            Assert.True(db.KeyExists("key2"));
            Assert.True(db.KeyExists("key3"));

            Assert.Equal(Serializer.Deserialize<string>(db.StringGet("key1")), "value1");
            Assert.Equal(Serializer.Deserialize<string>(db.StringGet("key2")), "value2");
            Assert.Equal(Serializer.Deserialize<string>(db.StringGet("key3")), "value3");
        }

        [Fact]
        public void Get_All_Should_Return_All_Database_Keys()
        {
            var values = Builder<TestClass<string>>
                        .CreateListOfSize(5)
                        .All()
                        .Build();
            values.ForEach(x => db.StringSet(x.Key, Serializer.Serialize(x.Value)));

            IDictionary<string, string> result = sut.GetAll<string>(new[] { values[0].Key, values[1].Key, values[2].Key, "notexistingkey" });

            Assert.True(result.Count() == 4);
            Assert.Equal(result[values[0].Key], values[0].Value);
            Assert.Equal(result[values[1].Key], values[1].Value);
            Assert.Equal(result[values[2].Key], values[2].Value);
            Assert.Null(result["notexistingkey"]);
        }

        [Fact]
        public void Remove_All_Should_Remove_All_Specified_Keys()
        {
            var values = Builder<TestClass<string>>
                        .CreateListOfSize(5)
                        .All()
                        .Build();
            values.ForEach(x => db.StringSet(x.Key, x.Value));

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
