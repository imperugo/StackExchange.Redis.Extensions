---
name: redis-diagnose
description: Troubleshoot common StackExchange.Redis.Extensions issues — timeouts, connection failures, serialization problems, pool exhaustion
---

# Redis Diagnose

Diagnose and fix common issues with StackExchange.Redis.Extensions.

## When to use

When the user reports:
- Timeout exceptions
- Connection failures
- Serialization errors
- Pool exhaustion
- Pub/Sub messages not being received
- Performance issues

## Diagnostic Tree

### RedisTimeoutException
**Symptoms:** `StackExchange.Redis.RedisTimeoutException`

**Check in order:**
1. **SyncTimeout too low** — default is 5000ms, increase for slow networks
   ```csharp
   config.SyncTimeout = 10000; // 10 seconds
   ```
2. **Pool size too small** — default 5, increase for high-throughput
   ```csharp
   config.PoolSize = 10;
   ```
3. **Connection strategy** — switch to LeastLoaded if using RoundRobin
   ```csharp
   config.ConnectionSelectionStrategy = ConnectionSelectionStrategy.LeastLoaded;
   ```
4. **Large values** — enable compression to reduce payload size
   ```csharp
   services.AddRedisCompression<LZ4Compressor>();
   ```
5. **ThreadPool starvation** — check with `ThreadPool.GetAvailableThreads()`, increase min threads

### RedisConnectionException
**Symptoms:** `SocketClosed`, `ConnectionFailed`

**Check:**
1. **Redis server reachable?** — `redis-cli -h <host> -p <port> ping`
2. **Firewall/NSG rules** — port 6379 (or 6380 for TLS) open?
3. **TLS misconfiguration** — if using Ssl=true, check certificate callbacks
4. **Sentinel misconfiguration** — ServiceName must match Redis master name exactly
5. **Azure Cache for Redis** — ensure Managed Identity is configured if not using password

### Serialization Errors
**Symptoms:** `JsonException`, `InvalidOperationException`, corrupt data

**Check:**
1. **Type mismatch** — GetAsync<T> must use same T as AddAsync<T>
2. **String values are JSON-encoded** — "hello" is stored as "\"hello\"", this is by design
3. **Compression migration** — enabling compression makes old (uncompressed) data unreadable
   - Error: `InvalidOperationException: Failed to decompress data from Redis`
   - Fix: flush the database or read old data without compression first
4. **Value type quirk** — `GetAsync<int>()` returns 0 (not null) for missing keys because `default(int)` is `0`. Use `GetAsync<int?>()` to distinguish missing keys from actual zero values.

### Pub/Sub Not Receiving Messages
**Check:**
1. **KeyPrefix** — channels are automatically prefixed. Don't add prefix manually.
2. **Serializer mismatch** — publisher and subscriber must use the same serializer
3. **Handler exceptions** — check logs for EventId 4001 errors. Handlers that throw don't crash but the message is lost.
4. **Different connection pools** — ensure pub and sub use the same IRedisDatabase instance

### Pool Exhaustion / All Connections Down
**Symptoms:** All operations fail, logs show EventId 1003

**Check:**
1. **Pool health** — inject `IRedisClient` and call `client.ConnectionPoolManager.GetConnectionInformation()`
2. **Redis server overloaded** — check `INFO clients` on Redis
3. **Network partition** — the pool skips disconnected connections automatically and logs warnings
4. **Dispose pattern** — ensure IRedisConnectionPoolManager is not disposed prematurely

### Performance Issues
**Check:**
1. **Enable logging** — set log level to Debug to see connection selection
   ```json
   { "Logging": { "LogLevel": { "StackExchange.Redis.Extensions.Core": "Debug" } } }
   ```
2. **Check outstanding commands** — pool info shows outstanding count per connection
3. **Use compression** for large objects — LZ4 adds ~1ms latency but reduces network 5-10x
4. **Use AddAllAsync** for bulk writes instead of loop of AddAsync
5. **Use GetAllAsync** for bulk reads (note: requires `HashSet<string>` for keys, not arrays)

## Logging Reference

| EventId | Level | Meaning |
|---------|-------|---------|
| 1001 | Info | Pool initialized successfully |
| 1003 | Warning | All connections disconnected — degraded mode |
| 1006 | Error | Pool initialization failed |
| 2001 | Error | Connection failed |
| 2002 | Warning | Connection restored |
| 4001 | Error | Pub/Sub handler threw exception |

Enable with:
```json
{ "Logging": { "LogLevel": { "StackExchange.Redis.Extensions.Core": "Information" } } }
```
