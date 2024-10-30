using Microsoft.AspNetCore.Mvc;

namespace Taxi_App.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
