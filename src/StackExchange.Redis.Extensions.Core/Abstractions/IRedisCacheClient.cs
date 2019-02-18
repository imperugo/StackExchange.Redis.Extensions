namespace StackExchange.Redis.Extensions.Core.Abstractions
{
	public interface IRedisCacheClient
    {
        IRedisDatabase Db0 { get; }

        IRedisDatabase Db1 { get; }

        IRedisDatabase Db2 { get; }

        IRedisDatabase Db3 { get; }

        IRedisDatabase Db4 { get; }

        IRedisDatabase Db5 { get; }

        IRedisDatabase Db6 { get; }

        IRedisDatabase Db7 { get; }

        IRedisDatabase Db8 { get; }

        IRedisDatabase Db9 { get; }

        IRedisDatabase Db10 { get; }

        IRedisDatabase Db11 { get; }

        IRedisDatabase Db12 { get; }

        IRedisDatabase Db13 { get; }

        IRedisDatabase Db14 { get; }

        IRedisDatabase Db15 { get; }

        IRedisDatabase Db16 { get; }

        ISerializer Serializer { get; }

        IRedisDatabase GetDb(int dbNumber);

		IRedisDatabase GetDbFromConfiguration();
	}
}

