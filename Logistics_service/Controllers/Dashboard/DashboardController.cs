using Logistics_service.Models;
using Logistics_service.Models.Users;
using Logistics_service.Services;
using Logistics_service.Static;
using Microsoft.AspNetCore.Mvc;
using System.Data;

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
        [AuthorizeRole(UserRole.Administrator)]
        [HttpGet("administrator")]
        public IActionResult Administrator()
        {
            ViewData["Title"] = "administrator";
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpGet("manager")]
        public IActionResult Manager()
        {
            ViewData["Title"] = "manager";
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager, UserRole.Customer)]
        [HttpGet("customer")]
        public IActionResult Customer()
        {
            ViewData["Title"] = "customer";
            return View();
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard(UserRole role)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
            {
                return View("Unauthorized");
            }

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"/dashboard/{role}\", role=\"{role}\"";

            return View("Authenticate");
        }
    }
}