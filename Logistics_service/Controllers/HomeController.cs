using Logistics_service.Models.Users;
using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Logistics_service.Models.Statistic;
using Logistics_service.Services;

namespace Logistics_service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            Console.WriteLine($"Подключился {ipAddress}");

            var newMark = new EntryInfo(DateTime.Now, ipAddress);
            _context.UserEntrysStatistic.Add(newMark);
            await _context.SaveChangesAsync();

            return View();
        }
    }
}