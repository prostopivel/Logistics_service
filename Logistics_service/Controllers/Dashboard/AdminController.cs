using Logistics_service.Data;
using Logistics_service.Models.Service;
using Logistics_service.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


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
            ViewBag.RealmHeader = _configuration["Realm"];
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addUser")]
        public async Task<ActionResult> AddUser([FromBody] JsonElement data)
        {
            if (data.ValueKind == JsonValueKind.Null)
            {
                ViewBag.Error = "Отсутствуют данные!";
                return View("addUser");
            }

            string? name, email, passwordHash;
            int role;

            if (data.TryGetProperty("Name", out var nameProperty)
                && data.TryGetProperty("Role", out var roleProperty)
                && data.TryGetProperty("Email", out var emailProperty)
                && data.TryGetProperty("PasswordHash", out var passwordHashProperty))
            {
                name = nameProperty.GetString();
                string? roleNull = roleProperty.GetString();
                email = emailProperty.GetString();
                passwordHash = passwordHashProperty.GetString();

                if (!int.TryParse(roleNull, out role) || role < 0 || role > 2
                    || name == null || email == null || passwordHash == null)
                {
                    ViewBag.Error = "Неверно переданные данные!";
                    return View("addUser");
                }
            }
            else
            {
                ViewBag.Error = "Неверно переданные данные!";
                return View("addUser");
            }

            if (ModelState.IsValid)
            {
                User user = new User
                {
                    Name = name,
                    Role = (UserRole)role,
                    Email = email,
                    PasswordHash = passwordHash
                };

                if (_context.Users.FirstOrDefault(c => c.Email == user.Email) == null)
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ViewBag.Error = "Пользователь с данной почтой уже существует!";
                    return View("addUser");
                }

                await Console.Out.WriteLineAsync("Add: " + user.Email);

                return RedirectToAction("administrator", "Dashboard");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage)
                                  .ToList();

            ViewBag.Error = string.Join("<br>", errors); 
            return View("addUser");
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
                return View("UnauthorizedCompletely");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"{returnUrl}\", role = \"Administrator\"";
            return View("Unauthorized");
        }

        [HttpPost("adminPost")]
        public ViewResult AdminPost(AddUser formData)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return View("UnauthorizedComletely");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            formData.User.PasswordHash = GenerateDigest.ComputeMD5($"{formData.User.Email}:{realm}:{formData.User.PasswordHash}");

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"{formData.ReturnUrl}\" formData=\"{formData.User.AuthParams()}\", role = \"Administrator\"";
            return View("UnauthorizedPost");
        }
    }
}
