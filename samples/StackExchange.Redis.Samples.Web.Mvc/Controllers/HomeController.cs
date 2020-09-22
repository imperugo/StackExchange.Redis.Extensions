using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Samples.Web.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRedisDatabase redisDatabase;

        public HomeController(IRedisDatabase redisDatabase)
        {
            this.redisDatabase = redisDatabase;
        }

        public async Task<IActionResult> Index()
        {
            await redisDatabase.PublishAsync("MyEventName", "ping");

            return Ok(new { Message = "Ciao" });
        }
    }
}
