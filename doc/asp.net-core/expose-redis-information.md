# Expose redis information

Sometimes is helpfull to expose the redis information outside of you application, like a json endpoint, in order to monitor the connection, the status of the server an so on.

For this we have created a specific middleware that you can use like this:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UserRedisInformation();
}
```

From now, you have two endpoint availables:

**edis information "/redis/connectionInfo"**

```csharp
{
  "RequiredPoolSize": 5,
  "ActiveConnections": 1,
  "InvalidConnections": 0,
  "ReadyNotUsedYet": 4
}
```

**Redis information "/redis/info"**

```csharp
{
  "redis_version": "5.0.7",
  "redis_git_sha1": "00000000",
  "redis_git_dirty": "0",
  "redis_build_id": "5f6bfe2b13cc4617",
  "redis_mode": "standalone",
  "os": "Linux 4.19.76-linuxkit x86_64",
  "arch_bits": "64",
  "multiplexing_api": "epoll",
  "atomicvar_api": "atomic-builtin",
  "gcc_version": "8.3.0",
  "process_id": "1",
  "run_id": "ed1b72b1d1744c414f3fc2495f298ac033f787c6",
  "tcp_port": "6379",
  "uptime_in_seconds": "34131",
  "uptime_in_days": "0",
  "hz": "10",
  "configured_hz": "10",
  "lru_clock": "10956504",
  "executable": "/data/redis-server",
  "config_file": "",
  "connected_clients": "2",
  "client_recent_max_input_buffer": "2",
  "client_recent_max_output_buffer": "0",
  "blocked_clients": "0",
  "used_memory": "1732288",
  "used_memory_human": "1.65M",
  "used_memory_rss": "5775360",
  "used_memory_rss_human": "5.51M",
  "used_memory_peak": "1834480",
  "used_memory_peak_human": "1.75M",
  "used_memory_peak_perc": "94.43%",
  "used_memory_overhead": "858352",
  "used_memory_startup": "791264",
  "used_memory_dataset": "873936",
  "used_memory_dataset_perc": "92.87%",
  "allocator_allocated": "1804256",
  "allocator_active": "2080768",
  "allocator_resident": "4820992",
  "total_system_memory": "2086522880",
  "total_system_memory_human": "1.94G",
  "used_memory_lua": "34816",
  "used_memory_lua_human": "34.00K",
  "used_memory_scripts": "136",
  "used_memory_scripts_human": "136B",
  "number_of_cached_scripts": "1",
  "maxmemory": "0",
  "maxmemory_human": "0B",
  "maxmemory_policy": "noeviction",
  "allocator_frag_ratio": "1.15",
  "allocator_frag_bytes": "276512",
  "allocator_rss_ratio": "2.32",
  "allocator_rss_bytes": "2740224",
  "rss_overhead_ratio": "1.20",
  "rss_overhead_bytes": "954368",
  "mem_fragmentation_ratio": "3.42",
  "mem_fragmentation_bytes": "4085176",
  "mem_not_counted_for_evict": "0",
  "mem_replication_backlog": "0",
  "mem_clients_slaves": "0",
  "mem_clients_normal": "66616",
  "mem_aof_buffer": "0",
  "mem_allocator": "jemalloc-5.1.0",
  "active_defrag_running": "0",
  "lazyfree_pending_objects": "0",
  "loading": "0",
  "rdb_changes_since_last_save": "0",
  "rdb_bgsave_in_progress": "0",
  "rdb_last_save_time": "1587980677",
  "rdb_last_bgsave_status": "ok",
  "rdb_last_bgsave_time_sec": "-1",
  "rdb_current_bgsave_time_sec": "-1",
  "rdb_last_cow_size": "0",
  "aof_enabled": "0",
  "aof_rewrite_in_progress": "0",
  "aof_rewrite_scheduled": "0",
  "aof_last_rewrite_time_sec": "-1",
  "aof_current_rewrite_time_sec": "-1",
  "aof_last_bgrewrite_status": "ok",
  "aof_last_write_status": "ok",
  "aof_last_cow_size": "0",
  "total_connections_received": "10",
  "total_commands_processed": "86",
  "instantaneous_ops_per_sec": "0",
  "total_net_input_bytes": "3307",
  "total_net_output_bytes": "27046",
  "instantaneous_input_kbps": "0.00",
  "instantaneous_output_kbps": "0.00",
  "rejected_connections": "0",
  "sync_full": "0",
  "sync_partial_ok": "0",
  "sync_partial_err": "0",
  "expired_keys": "0",
  "expired_stale_perc": "0.00",
  "expired_time_cap_reached_count": "0",
  "evicted_keys": "0",
  "keyspace_hits": "0",
  "keyspace_misses": "10",
  "pubsub_channels": "1",
  "pubsub_patterns": "0",
  "latest_fork_usec": "0",
  "migrate_cached_sockets": "0",
  "slave_expires_tracked_keys": "0",
  "active_defrag_hits": "0",
  "active_defrag_misses": "0",
  "active_defrag_key_hits": "0",
  "active_defrag_key_misses": "0",
  "role": "master",
  "connected_slaves": "0",
  "master_replid": "c5395946e2c8303b9e256aad1ae5919c0721132f",
  "master_replid2": "0000000000000000000000000000000000000000",
  "master_repl_offset": "0",
  "second_repl_offset": "-1",
  "repl_backlog_active": "0",
  "repl_backlog_size": "1048576",
  "repl_backlog_first_byte_offset": "0",
  "repl_backlog_histlen": "0",
  "used_cpu_sys": "31.233827",
  "used_cpu_user": "21.267245",
  "used_cpu_sys_children": "0.008739",
  "used_cpu_user_children": "0.003207",
  "cluster_enabled": "0",
  "db6": "keys=6,expires=0,avg_ttl=0"
}
```

Of course these responses could contains sensilbe data, for this reason you can allow access only to a specific set of IP Addresses of you custom login.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UserRedisInformation(x =>
    {
        x.AllowedIPs = Array.Empty<IPAddress>();
        x.AllowFunction = (HttpContext ctx) =>
        {
            // My custom logic
            return true;
        };
    });
}
```
