# Configuration

**StackExchange.Redis.Extensions** is based on top of  **StackExchange.Redis** and of course, for the connection part, offers all the main library has but with a bounce of more features like ConnectionPool and MaxMessageSize.

### The importance of Connection pool.

The connection pool is one of the most helpful features that this library offers because is some case can save you from the annoying timeout. [Here](https://github.com/StackExchange/StackExchange.Redis/blob/master/docs/Timeouts.md)  the complete documentation of the most common reason why you can get a timeout using Redis, but below you can find a summary:

> because StackExchange.Redis uses a single TCP connection and can only read one response at a time. Even though the first operation timed out, it does not stop the data being sent to/from the server, and other requests are blocked until this is finished. Thereby, causing timeouts. One solution is to minimize the chance of timeouts by ensuring that your redis-server cache is large enough for your workload and splitting large values into smaller chunks. **Another possible solution is to use a pool of ConnectionMultiplexer objects in your client, and choose the "least loaded" ConnectionMultiplexer when sending a new request**. This should prevent a single timeout from causing other requests to also timeout.

With StackExchange.Redis this comes out of the box and you don't have to do anything except increase the size of the pool if you need it.

### Prevent big values

**Redis works best with smaller values**, so consider chopping up bigger data into multiple keys. In [this Redis discussion](https://stackoverflow.com/questions/55517224/what-is-the-ideal-value-size-range-for-redis-is-100kb-too-large/), some considerations are listed that you should consider carefully. Read [this article](https://docs.microsoft.com/en-gb/azure/azure-cache-for-redis/cache-troubleshoot-client#large-request-or-response-size) for an example problem that can be caused by large values.

With StackExchange.Redis you can set the max value size via configuration.

