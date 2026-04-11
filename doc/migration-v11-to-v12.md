# Migration Guide: v11 to v12

This guide covers everything you need to know to upgrade from StackExchange.Redis.Extensions v11 to v12.

## Quick Summary

v12 is a major release with **no breaking changes to the public API**. All existing code continues to work. The upgrade brings new features, bug fixes, performance improvements, and 5 new NuGet packages for compression.

## Step 1: Update NuGet Packages

```bash
# Update core packages
dotnet add package StackExchange.Redis.Extensions.Core --version 12.1.0
dotnet add package StackExchange.Redis.Extensions.AspNetCore --version 12.1.0

# Update your serializer (pick the one you use)
dotnet add package StackExchange.Redis.Extensions.System.Text.Json --version 12.1.0
# or: Newtonsoft, MsgPack, Protobuf, MemoryPack, ServiceStack, Utf8Json

# Optional: add compression (new in v12)
dotnet add package StackExchange.Redis.Extensions.Compression.LZ4 --version 12.1.0
```

## Step 2: Verify Your Configuration

### SyncTimeout Default Changed

The default `SyncTimeout` changed from **1000ms to 5000ms** to match StackExchange.Redis defaults and the XML documentation.

**If you were relying on the old 1000ms default** and want to keep it:
```csharp
config.SyncTimeout = 1000;
```

**If you had no explicit SyncTimeout**, your app now has a more generous timeout, which should reduce `RedisTimeoutException` occurrences.

### Sentinel Users

`CommandMap.Sentinel` is **no longer applied** to the master connection. In v11, Sentinel configuration incorrectly disabled data commands (GET, SET, EVAL, SCAN) on the resolved master. This is now fixed.

**Action required:** If you had a workaround for this (e.g., `ExcludeCommands = null` or connection string overrides), you can remove it.

### StackExchange.Redis Upgraded

The SE.Redis dependency changed from `[2.8.*,3.0)` to `2.12.14`. This is a **pinned version** (no range).

**Possible impact:** If you were using SE.Redis features that changed between 2.8 and 2.12, check the [SE.Redis release notes](https://github.com/StackExchange/StackExchange.Redis/releases).

## Step 3: Take Advantage of New Features

### .NET 10 Support

v12 targets `netstandard2.1`, `net8.0`, `net9.0`, and `net10.0`. No action needed — the correct TFM is selected automatically.

### Connection Pool Resilience

`GetConnection()` now **skips disconnected connections** automatically. If all connections are down, the pool falls back to any connection (letting SE.Redis's internal reconnection handle recovery). A warning is logged (EventId 1003).

**Action:** No code changes needed. This just works.

### Pub/Sub Error Handling

Subscription handler exceptions are now **logged** (EventId 4001) instead of being silently swallowed. To see these logs, ensure `RedisConfiguration.LoggerFactory` is set (automatic with `AddStackExchangeRedisExtensions`).

**Action:** Check your logs for subscription handler errors that were previously hidden.

### AddAllAsync with Expiry — Fixed

In v11, `AddAllAsync` with expiry used a two-phase approach (MSET + separate EXPIREAT commands) that had a race condition. In v12, each key is set atomically with its expiry via `SET key value PX <ms>` in a batch.

**Action:** No code changes needed. Your data is now more reliable.

### New: GeoSpatial API

```csharp
await redis.GeoAddAsync("stores", 13.361389, 38.115556, "Palermo");
var nearby = await redis.GeoSearchAsync("stores", 13.361389, 38.115556,
    new GeoSearchCircle(200, GeoUnit.Kilometers));
```

[Full documentation](geospatial.md)

### New: Redis Streams

```csharp
await redis.StreamAddAsync("orders", "payload", orderData);
var entries = await redis.StreamReadGroupAsync("orders", "processors", "worker-1", ">");
await redis.StreamAcknowledgeAsync("orders", "processors", entries[0].Id.ToString());
```

[Full documentation](streams.md)

### New: Hash Field Expiry (Redis 7.4+)

```csharp
await redis.HashSetWithExpiryAsync("user:1", "session", data, TimeSpan.FromMinutes(30));
```

[Full documentation](hash-field-expiry.md)

### New: VectorSet for AI/ML (Redis 8.0+)

```csharp
await redis.VectorSetAddAsync("docs", VectorSetAddRequest.Member("doc-1", embedding));
using var results = await redis.VectorSetSimilaritySearchAsync("docs",
    VectorSetSimilaritySearchRequest.ByVector(queryEmb) with { Count = 5 });
```

[Full documentation](vectorset.md)

### New: Transparent Compression

```csharp
services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(config);
services.AddRedisCompression<LZ4Compressor>(); // one line!
```

**Warning:** Enabling compression on existing data makes old (uncompressed) values unreadable. Plan a migration strategy.

[Full documentation](compressors.md)

### New: Azure Managed Identity

```csharp
config.ConfigurationOptionsAsyncHandler = async opts =>
{
    await opts.ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
    return opts;
};
```

[Full documentation](azure-managed-identity.md)

### New: Configuration Properties

| Property | Description |
|----------|-------------|
| `ClientName` | Set Redis connection client name |
| `KeepAlive` | Heartbeat interval (seconds). -1 = default, 0 = disabled |
| `CertificateSelection` | TLS client certificate selection callback |
| `ConfigurationOptionsAsyncHandler` | Async callback for custom ConfigurationOptions (e.g., Azure) |

### New: Source-Generated Logging

All logging now uses `[LoggerMessage]` attributes for zero-allocation performance. [See logging reference](logging.md) for EventId table.

## Step 4: Test

```bash
# Run tests against your codebase
dotnet test

# If using Moq — consider switching to NSubstitute
# v12 replaced Moq internally due to the SponsorLink data collection incident
```

## Dependency Changes

| Package | v11 | v12 |
|---------|-----|-----|
| StackExchange.Redis | [2.8.*,3.0) | 2.12.14 |
| Target Frameworks | netstandard2.1, net8.0, net9.0 | + net10.0 |
| Test Framework | Moq | NSubstitute |
| Analyzers | Roslynator 4.12, CodeAnalysis 3.11 | Roslynator 4.15, CodeAnalysis 5.3 |

## New NuGet Packages (v12)

| Package | Description |
|---------|-------------|
| `StackExchange.Redis.Extensions.Compression.LZ4` | LZ4 compression (fastest) |
| `StackExchange.Redis.Extensions.Compression.Snappier` | Snappy compression |
| `StackExchange.Redis.Extensions.Compression.ZstdSharp` | Zstandard compression |
| `StackExchange.Redis.Extensions.Compression.GZip` | GZip compression (no deps) |
| `StackExchange.Redis.Extensions.Compression.Brotli` | Brotli compression (no deps) |

## FAQ

**Q: Is v12 backward compatible with v11?**
A: Yes. No public API was removed or changed. All new features are additive.

**Q: Do I need to flush Redis when upgrading?**
A: No. Existing data is fully compatible. Only enable compression if you understand the migration implications.

**Q: Does v12 support .NET 6 or .NET 7?**
A: Not as explicit TFMs, but `netstandard2.1` covers .NET Core 3.0+ and .NET 5+.

**Q: I was using the `nuget` branch to trigger publish. Is that still supported?**
A: No. Publishing is now manual via GitHub Actions workflow dispatch. The `nuget` branch has been deleted.
