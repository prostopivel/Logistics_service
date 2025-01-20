using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Users;
using Logistics_service.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private string _errorMessage = "Неизвестная ошибка!";

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            GenerateDigest.Configuration = configuration;
            GenerateDigest.Context = context;
        }

        [HttpGet("logIn")]
        [AllowAnonymous]
        public IActionResult LogIn()
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];
            string nonce = GenerateDigest.GenerateRandom();
            string opaque = GenerateDigest.GenerateRandom();

            HttpContext.Session.SetString("Opaque", opaque);
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\"";

            return View();
        }

        [HttpGet("registration")]
        [AllowAnonymous]
        public IActionResult Registration()
        {
            ViewBag.RealmHeader = _configuration["Realm"];
            return View();
        }

        [HttpPost("logIn")]
        [AllowAnonymous]
        public async Task<IActionResult> LogInPost()
        {
            var role = await Auth();
            if (role != null)
            {
                ViewBag.role = role;
                return JsonReturn("dashboard", "Dashboard", new { role });
            }
            else
            {
                return JsonReturn("Unauthorized", "Error", new { _errorMessage });
            }
        }

        [HttpPost("registration")]
        [AllowAnonymous]
        public async Task<IActionResult> Registration(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.Role = UserRole.Customer;

                if (!await AddCustomerAsync(customer))
                {
                    return JsonReturn("Unauthorized", "Error", new { errorMessage = "Данный пользователь уже существует!" });
                }

                ViewBag.role = UserRole.Customer;
                return JsonReturn("Index", "Home");
            }
            else
            {
                return JsonReturn("autor", "auth");
            }
        }

        private async Task<bool> AddCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Users.FirstOrDefaultAsync(c => c.Email == customer.Email);

            if (_context.Users.IsNullOrEmpty())
            {
                Administrator administrator = new Administrator()
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    PasswordHash = customer.PasswordHash,
                    Role = UserRole.Administrator
                };

                _context.Administrators.Add(administrator);
                await _context.SaveChangesAsync();
                return true;
            }

            if (existingCustomer == null)
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private async Task<UserRole?> Auth()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Digest "))
            {
                _errorMessage = "Неправильный дайджест!";
                return null;
            }

            try
            {
                var authParams = ParseAuthorizationHeader(authHeader["Digest ".Length..]);
                return await ValidateDigestToken(authParams);
            }
            catch
            {
                return null;
            }
        }

        private async Task<UserRole?> ValidateDigestToken(Dictionary<string, string> authParams)
        {
            var username = authParams["username"];
            var nonce = authParams["nonce"];
            var uri = authParams["uri"];
            var nc = authParams["nc"];
            var cnonce = authParams["cnonce"];
            var response = authParams["response"];
            var opaque = authParams["opaque"];

            var expectedNonce = HttpContext.Session.GetString(opaque);
            if (nonce != expectedNonce)
            {
                _errorMessage = "Неправильный nonce!";
                return null;
            }

            var expectedResponse = await ComputeDigestResponse(username, nonce, uri, nc, cnonce);

            if (expectedResponse == null)
            {
                return null;
            }
            else if (response != expectedResponse.Item1)
            {
                _errorMessage = "Неправильный response!";
                return null;
            }
            else
            {
                return expectedResponse.Item2;
            }
        }

        private async Task<Tuple<string, UserRole>?> ComputeDigestResponse(string username, string nonce, string uri, string nc, string cnonce)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
            if (user == null)
            {
                _errorMessage = "Пользователь не найден!";
                return null;
            }

            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var A2 = $"POST:{uri}";
            var HA1 = user.PasswordHash;
            var HA2 = ComputeMD5(A2);

            var response = ComputeMD5($"{HA1}:{nonce}:{nc}:{cnonce}:{qop}:{HA2}");
            return new Tuple<string, UserRole>(response, user.Role);
        }

        private static string ComputeMD5(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private Dictionary<string, string> ParseAuthorizationHeader(string header)
        {
            var paramsDict = new Dictionary<string, string>();
            var parts = header.Split(',');
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                paramsDict[kv[0].Trim()] = kv[1].Trim('"');
            }
            return paramsDict;
        }

        public static string GenerateRandom()
        {
            byte[] randomBytes = new byte[16];
            RandomNumberGenerator.Fill(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }

        private IActionResult JsonReturn(string action, string controller, object? data = null)
        {
            return Json(new
            {
                redirectUrl = Url.Action(action, controller, data)
            });
        }
    }
}