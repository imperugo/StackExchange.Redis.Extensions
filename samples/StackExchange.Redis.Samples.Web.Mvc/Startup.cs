using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Samples.Web.Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // var conf = new RedisConfiguration()
            // {
            //     AbortOnConnectFail = true,
            //     KeyPrefix = "MyPrefix__",
            //     Hosts = new RedisHost[]
            //     {
            //         new RedisHost { Host = "localhost", Port = 6379 }
            //     },
            //     AllowAdmin = true,
            //     ConnectTimeout = 3000,
            //     Database = 0,
            //     ServerEnumerationStrategy = new ServerEnumerationStrategy()
            //     {
            //         Mode = ServerEnumerationStrategy.ModeOptions.All,
            //         TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
            //         UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
            //     }
            // };
            var conf = new RedisConfiguration();
            conf.ConnectionString = "localhost:6379,ConnectTimeout=5000,allowAdmin=true";

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(conf);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UserRedisInformation();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
