using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using System.IO;

namespace Microsoft.AspNetCore.Hosting
{
	public static class IWebHostBuilderExtensions
	{
		public static IWebHostBuilder ConfigureStackExchangeRedisExtensions<T>(this IWebHostBuilder hostBuilder, string jsonBasePath = "Configurations") where T : class, ISerializer, new()
		{
			hostBuilder.ConfigureServices(services =>
			   {
				   var srvs = services.BuildServiceProvider();

				   var environment = srvs.GetRequiredService<IHostingEnvironment>();

				   var path = Path.Combine(environment.ContentRootPath, jsonBasePath, "redis.json");

				   var exists = File.Exists(path);


				   var builder = new ConfigurationBuilder();

				   IConfigurationRoot cfg = builder
						.SetBasePath(environment.ContentRootPath)
						.AddJsonFile("Configurations/redis.json", false, true)
						.AddJsonFile($"Configurations/redis.{environment.EnvironmentName}.json", true)
						.AddEnvironmentVariables()
						.Build();


				   var redisConfiguration = cfg.GetSection("redisConfiguration").Get<RedisConfiguration>();

				   services.AddSingleton(redisConfiguration);

				   services.AddStackExchangeRedisExtensions<T>(redisConfiguration);
			   });

			return hostBuilder;
		}
	}
}
