using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace StackExchange.Redis.Samples.Web.Mvc;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        var conf = new RedisConfiguration
        {
            AbortOnConnectFail = true,
            KeyPrefix = "MyPrefix__",
            Hosts = new[] { new RedisHost { Host = "localhost", Port = 6379 } },
            AllowAdmin = true,
            ConnectTimeout = 5000,
            Database = 0,
            PoolSize = 50,
            ServerEnumerationStrategy = new()
            {
                Mode = ServerEnumerationStrategy.ModeOptions.All,
                TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
            }
        };

        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(conf);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

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

        // var redisDb = app.ApplicationServices.GetRequiredService<IRedisDatabase>();

        // redisDb.SubscribeAsync<string>("MyEventName", x =>
        //     {
        //         logger.LogInformation("Just got this message {0}", x);

        //         return Task.CompletedTask;
        //     })
        //     .GetAwaiter()
        //     .GetResult();
    }
}
