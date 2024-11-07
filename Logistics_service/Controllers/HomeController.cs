using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            var digestResponse = HttpContext.Session.GetString("DigestResponse");
            var role = HttpContext.Session.GetString("Role");

            ViewBag.Role = role;
            ViewBag.Digest = digestResponse;

            return View();
        }
    }
}