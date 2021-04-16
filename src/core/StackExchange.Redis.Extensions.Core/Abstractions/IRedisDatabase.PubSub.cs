using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    /// <summary>
    /// The Redis Database
    /// </summary>
    public partial interface IRedisDatabase
    {
        /// <summary>
        ///     Publishes a message to a channel.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="channel">The pub/sub channel name</param>
        /// <param name="message">The messange to send.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Registers a callback handler to process messages published to a channel.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="channel">The pub/sub channel name</param>
        /// <param name="handler">The function to run when a message has received.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Unregisters a callback handler to process messages published to a channel.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="channel">The pub/sub channel name</param>
        /// <param name="handler">The function to run when a message has received.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Unregisters all callback handlers on a channel.
        /// </summary>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task UnsubscribeAllAsync(CommandFlags flag = CommandFlags.None);
    }
}
