using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace StackExchange.Redis.Extensions.AspNetCore.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
				.ConfigureStackExchangeRedisExtensions<NewtonsoftSerializer>()
                .UseStartup<Startup>();
    }
}
