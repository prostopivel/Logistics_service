using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Logistics_service.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            Console.WriteLine($"Подключился {ipAddress}");

            return View();
        }

        public IActionResult ViewIndex(string digest)
        {
            if (digest != null)
            {
                string? userRole = HttpContext.Session.GetString(digest);

                ViewBag.Role = userRole;
                ViewBag.Digest = digest;
            }

            return View();
        }
    }
}