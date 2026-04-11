# GeoSpatial

StackExchange.Redis.Extensions wraps Redis's [GeoSpatial commands](https://redis.io/docs/data-types/geospatial/) with a clean, string-based API.

## Adding Locations

```csharp
// Single location
await redis.GeoAddAsync("stores", -73.935242, 40.730610, "NYC Store");

// Multiple locations in one call
await redis.GeoAddAsync("stores", new[]
{
    new GeoEntry(-73.935242, 40.730610, "NYC Store"),
    new GeoEntry(-118.243685, 34.052234, "LA Store"),
    new GeoEntry(-87.629799, 41.878113, "Chicago Store"),
});
```

## Distance Between Members

```csharp
var km = await redis.GeoDistanceAsync("stores", "NYC Store", "LA Store", GeoUnit.Kilometers);
// ~3944 km
```

## Get Coordinates

```csharp
// Single member
var pos = await redis.GeoPositionAsync("stores", "NYC Store");
Console.WriteLine($"Lat: {pos?.Latitude}, Lon: {pos?.Longitude}");

// Multiple members
var positions = await redis.GeoPositionAsync("stores", new[] { "NYC Store", "LA Store" });
```

## Search by Radius

```csharp
// Search within 1000km of coordinates
var nearby = await redis.GeoRadiusAsync("stores",
    -73.935242, 40.730610,
    1000, GeoUnit.Kilometers,
    count: 5, order: Order.Ascending);

foreach (var result in nearby)
    Console.WriteLine($"{result.Member}: {result.Distance}km");
```

## Search by Shape (Redis 6.2+)

```csharp
// Circle search centered on a member
var circle = await redis.GeoSearchAsync("stores", "NYC Store",
    new GeoSearchCircle(500, GeoUnit.Miles));

// Box search centered on coordinates
var box = await redis.GeoSearchAsync("stores", -73.935, 40.730,
    new GeoSearchBox(1000, 500, GeoUnit.Kilometers));
```

## Search and Store Results

```csharp
// Store search results in a new key
var count = await redis.GeoSearchAndStoreAsync(
    "stores", "nearby-nyc", "NYC Store",
    new GeoSearchCircle(100, GeoUnit.Miles));
```

## Remove a Member

```csharp
await redis.GeoRemoveAsync("stores", "Chicago Store");
```

## Get Geohash

```csharp
var hash = await redis.GeoHashAsync("stores", "NYC Store");
// e.g., "dr5regw3pp0"
```

## Architecture

```mermaid
graph LR
    A[GeoAddAsync] -->|GEOADD| R[(Redis Sorted Set)]
    B[GeoSearchAsync] -->|GEOSEARCH| R
    C[GeoDistanceAsync] -->|GEODIST| R
    D[GeoPositionAsync] -->|GEOPOS| R
```

> **Note:** Geo members are string identifiers (e.g., store names, IDs). Coordinates are stored internally by Redis as sorted set scores. If you need to associate complex objects with a location, store them under a key derived from the member name.

## API Reference

| Method | Redis Command | Parameters | Returns |
|--------|--------------|------------|---------|
| `GeoAddAsync(key, longitude, latitude, member)` | GEOADD | `string key, double lon, double lat, string member` | `Task<bool>` |
| `GeoAddAsync(key, value)` | GEOADD | `string key, GeoEntry value` | `Task<bool>` |
| `GeoAddAsync(key, values)` | GEOADD | `string key, GeoEntry[] values` | `Task<long>` |
| `GeoRemoveAsync(key, member)` | ZREM | `string key, string member` | `Task<bool>` |
| `GeoDistanceAsync(key, member1, member2, unit)` | GEODIST | `string key, string m1, string m2, GeoUnit unit` | `Task<double?>` |
| `GeoHashAsync(key, member)` | GEOHASH | `string key, string member` | `Task<string?>` |
| `GeoHashAsync(key, members)` | GEOHASH | `string key, string[] members` | `Task<string?[]>` |
| `GeoPositionAsync(key, member)` | GEOPOS | `string key, string member` | `Task<GeoPosition?>` |
| `GeoPositionAsync(key, members)` | GEOPOS | `string key, string[] members` | `Task<GeoPosition?[]>` |
| `GeoRadiusAsync(key, member, radius, ...)` | GEORADIUS | by member | `Task<GeoRadiusResult[]>` |
| `GeoRadiusAsync(key, lon, lat, radius, ...)` | GEORADIUS | by coordinates | `Task<GeoRadiusResult[]>` |
| `GeoSearchAsync(key, member, shape, ...)` | GEOSEARCH | by member + shape | `Task<GeoRadiusResult[]>` |
| `GeoSearchAsync(key, lon, lat, shape, ...)` | GEOSEARCH | by coordinates + shape | `Task<GeoRadiusResult[]>` |
| `GeoSearchAndStoreAsync(src, dst, member, shape, ...)` | GEOSEARCHSTORE | by member | `Task<long>` |
| `GeoSearchAndStoreAsync(src, dst, lon, lat, shape, ...)` | GEOSEARCHSTORE | by coordinates | `Task<long>` |
