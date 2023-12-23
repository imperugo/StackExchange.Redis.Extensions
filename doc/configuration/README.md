# Configuration

### Prevent big values

**Redis works best with smaller values**, so consider chopping up bigger data into multiple keys. In [this Redis discussion](https://stackoverflow.com/questions/55517224/what-is-the-ideal-value-size-range-for-redis-is-100kb-too-large/), some considerations are listed that you should consider carefully. Read [this article](https://docs.microsoft.com/en-gb/azure/azure-cache-for-redis/cache-troubleshoot-client#large-request-or-response-size) for an example problem that can be caused by large values.

With StackExchange.Redis you can set the max value size via configuration.
