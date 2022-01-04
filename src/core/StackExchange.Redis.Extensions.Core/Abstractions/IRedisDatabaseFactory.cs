namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database Factory usefull in case of multplie instances of Redis.
/// </summary>
public interface IRedisDatabaseFactory
{
    /// <summary>
    /// Return an instance of <see cref="IRedisDatabase"/>.
    /// </summary>
    IRedisCacheClient GetDefaultRedisClient();

    /// <summary>
    /// Return an instance of <see cref="IRedisDatabase"/>.
    /// </summary>
    /// <param name="name">If not specified returns the default instance</param>
    IRedisCacheClient GetRedisClient(string name = null);
}

