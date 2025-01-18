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
    }
}