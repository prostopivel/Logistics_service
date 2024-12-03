using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AdminController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllCustomers")]
        public async Task<ActionResult> ViewAllCustomers()
        {
            return View(await _context.Customers.ToListAsync());
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllManagers")]
        public async Task<ActionResult> ViewAllManagers()
        {
            return View(await _context.Managers.ToListAsync());
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("addManager")]
        public IActionResult AddManager()
        {
            ViewBag.Realm = _configuration["Realm"];
            return View();
        }

        [HttpPost("addManager")]
        public async Task<ActionResult> AddManager(Manager manager)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(manager);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Dashboard", new { role = UserRole.Administrator });
            }

            return View(manager);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllOrders")]
        public IActionResult ViewAllOrders()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("manageTransport")]
        public IActionResult ManageTransport()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public IActionResult ViewMap()
        {
            return View();
        }

        [HttpGet("admin")]
        public IActionResult Admin(string returnUrl)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return View("Unauthorized");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"{returnUrl}\", role = \"Administrator\"";

            return View("Unauthorized");
        }
    }
}
