using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace StackExchange.Redis.Samples.Web.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return Ok(new { Message = "Ciao" });
        }
    }
}
