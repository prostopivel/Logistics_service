using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Admin(string digest)
        {
            if (AuthenticateAndAuthorize("AdminDashboard", digest))
                return View("AdminDashboard", digest);
            else
                return View("Unauthorized");
        }

        public IActionResult Manager(string digest)
        {
            if (AuthenticateAndAuthorize("ManagerDashboard", digest))
                return View("ManagerDashboard", digest);
            else
                return View("Unauthorized");
        }

        public IActionResult Customer(string digest)
        {
            if (AuthenticateAndAuthorize("CustomerDashboard", digest))
                return View("CustomerDashboard", digest);
            else
                return View("Unauthorized");
        }

        private bool AuthenticateAndAuthorize(string viewName, string digest)
        {
            string? userRole = HttpContext.Session.GetString(digest);

            if (userRole == null || !Enum.TryParse(typeof(UserRole), userRole, out var result))
                return false;
            switch (viewName)
            {
                case "AdminDashboard":
                    if ((UserRole)result == UserRole.Administrator)
                        return true;
                    else
                        return false;
                case "ManagerDashboard":
                    if ((UserRole)result == UserRole.Administrator
                        || (UserRole)result == UserRole.Manager)
                        return true;
                    else
                        return false;
                case "CustomerDashboard":
                    if ((UserRole)result == UserRole.Administrator
                        || (UserRole)result == UserRole.Manager
                        || (UserRole)result == UserRole.Customer)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];


            var opaque = HttpContext.Session.GetString("Opaque");
            string nonce = GenerateDigestController.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            // Передача данных в представление через ViewBag
            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\"";

            return View();
        }
    }
}