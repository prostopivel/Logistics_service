using Logistics_service.Models;
using Logistics_service.Models.Users;
using Logistics_service.Services;
using Logistics_service.Static;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;

namespace Logistics_service.Controllers.Dashboard
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
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

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            var opaque = HttpContext.Session.GetString(ipAddress);
            if (opaque == null)
                return View("Unauthorized");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"/dashboard/{role}\", role = \"{role}\"";

            return View("Authenticate");
        }
    }
}