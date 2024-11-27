using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public DashboardController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("administrator")]
        public IActionResult Administrator()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("manager")]
        public IActionResult Manager()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("customer")]
        public IActionResult Customer()
        {
            return View();
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard(UserRole role)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return View("Unauthorized");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"/dashboard/{role}\", role = \"{role}\"";

            Console.WriteLine("dashboard:Unauthorized");
            return View("Unauthorized");
        }
    }
}