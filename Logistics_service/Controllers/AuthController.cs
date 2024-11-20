using Logistics_service.Controllers;
using Logistics_service.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //auth/auth
        [HttpGet("auth")]
        [AllowAnonymous]
        public IActionResult Auth(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl ?? Url.Content("~/");
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];
            string nonce = GenerateDigestController.GenerateRandom();
            string opaque = GenerateDigestController.GenerateRandom();

            HttpContext.Session.SetString("Opaque", opaque);
            HttpContext.Session.SetString(opaque, nonce);

            // Передача данных в представление через ViewBag
            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\"";

            return View();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login()
        {
            return RedirectToAction("Auth", "GenerateDigest", new { returnUrl = "/Dashboard/Dashboard" });
        }
    }
}