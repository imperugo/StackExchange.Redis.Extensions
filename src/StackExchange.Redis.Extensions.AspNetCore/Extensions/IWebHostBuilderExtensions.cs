using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using System.IO;

namespace Microsoft.AspNetCore.Hosting
{
	/// <summary>
	/// Extensions for adding an <see cref="ISerializer"/> implementation to an AspNetCore application.
	/// </summary>
	public static class IWebHostBuilderExtensions
	{
		/// <summary>
		/// Add <typeparamref name="T"/> serializer to an AspNetCore application.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ISerializer"/> to add.</typeparam>
		/// <param name="hostBuilder">Application to configure.</param>
		/// <param name="jsonBasePath">Relative path to "redis.json" and redis.{environment.EnvironmentName}.json" file.</param>
		/// <returns>Application builder.</returns>
		public static IWebHostBuilder ConfigureStackExchangeRedisExtensions<T>(this IWebHostBuilder hostBuilder, string jsonBasePath = "Configurations") where T : class, ISerializer, new()
		{
			hostBuilder.ConfigureServices(services =>
			   {
				   var srvs = services.BuildServiceProvider();

				   var environment = srvs.GetRequiredService<IHostingEnvironment>();

				   var path = Path.Combine(environment.ContentRootPath, jsonBasePath, "redis.json");

				   var exists = File.Exists(path);

				   if (!exists) throw new FileNotFoundException("redis.json configure file must be included");

				   var builder = new ConfigurationBuilder();

				   IConfigurationRoot cfg = builder
						.SetBasePath(environment.ContentRootPath)
						.AddJsonFile($"{jsonBasePath}/redis.json", false, true)
						.AddJsonFile($"{jsonBasePath}/redis.{environment.EnvironmentName}.json", true)
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
