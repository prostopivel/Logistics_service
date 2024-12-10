using Logistics_service.Data;
using Logistics_service.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Logistics_service.Controllers.Dashboard
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
        [HttpGet("addUser")]
        public IActionResult AddUser()
        {
            ViewBag.Realm = _configuration["Realm"];
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addUser")]
        public async Task<ActionResult> AddUser(IFormCollection formData)
        {
            var name = formData["Name"];
            var role = formData["Role"];
            var email = formData["Email"];
            var passwordHash = formData["PasswordHash"];

            User user;
            switch (role)
            {
                default:
                    user = new User();
                    break;
                case nameof(UserRole.Customer):
                    user = new Customer();
                    break;
                case nameof(UserRole.Manager):
                    user = new Manager();
                    break;
                case nameof(UserRole.Administrator):
                    user = new Administrator();
                    break;
            }
            user.Name = name;
            user.Role = Enum.Parse<UserRole>(role);
            user.Email = email;
            user.PasswordHash = passwordHash;

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Dashboard", new { role = UserRole.Administrator });
            }

            return View(user);
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

        [HttpGet("adminGet")]
        public ViewResult AdminGet(string returnUrl)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return View("UnauthorizedComletely");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"{returnUrl}\", role = \"Administrator\"";
            return View("Unauthorized");
        }

        [HttpPost("adminPost")]
        public ViewResult AdminPost(IFormCollection formData)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return View("UnauthorizedComletely");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            string returnUrl = formData["returnUrl"];
            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"{returnUrl}\" formData=\"{formData}\", role = \"Administrator\"";
            return View("UnauthorizedPost");
        }
    }
}
