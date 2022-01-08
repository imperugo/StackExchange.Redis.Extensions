// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace StackExchange.Redis.Samples.Web.Mvc;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        var configurations = new[]
        {
            new RedisConfiguration
            {
                AbortOnConnectFail = true,
                KeyPrefix = "MyPrefix__",
                Hosts = new[] { new RedisHost { Host = "localhost", Port = 6379 } },
                AllowAdmin = true,
                ConnectTimeout = 5000,
                Database = 0,
                PoolSize = 5,
                IsDefault = true
            },
            new RedisConfiguration
            {
                AbortOnConnectFail = true,
                KeyPrefix = "MyPrefix__",
                Hosts = new[] { new RedisHost { Host = "localhost", Port = 6389 } },
                AllowAdmin = true,
                ConnectTimeout = 5000,
                Database = 0,
                PoolSize = 2,
                Name = "Secndary Instance"
            }
        };

        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(configurations);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        // app.UseRedisInformation(opt =>
        // {
        //     opt.AllowedIPs = Array.Empty<IPAddress>();
        //     // opt.AllowedIPs = = new[] { IPAddress.Parse("127.0.0.1"), IPAddress.Parse("::1") };
        //     opt.AllowFunction = (HttpContext x) =>
        //     {
        //         return false;
        //     };
        // });

        app.UseRedisInformation();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });

        var redisDatabase = app.ApplicationServices.GetRequiredService<IRedisDatabase>();

        var obj = new ValueTypeRedisItem<bool>(true);

        redisDatabase.AddAsync("myCacheKey", obj)
            .GetAwaiter()
            .GetResult();

        var result = redisDatabase.GetAsync<ValueTypeRedisItem<bool>>("myCacheKey")
            .GetAwaiter()
            .GetResult();
    }
}

public class CacheObject
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime Birthdate { get; set; }
}

/// <summary>
/// The container class for value types
/// </summary>
/// <typeparam name="T"></typeparam>
public class ValueTypeRedisItem<T> where T : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueTypeRedisItem{T}"/> class.
    /// </summary>
    /// <param name="value">The Value</param>
    public ValueTypeRedisItem(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Return the specified value
    /// </summary>
    public T Value { get; }
}
