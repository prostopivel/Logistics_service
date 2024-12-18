using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using Taxi_App;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private string errorMessage = "Неизвестная ошибка!";

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            GenerateDigest._configuration = configuration;
            GenerateDigest._context = context;
        }

        //auth/auth
        [HttpGet("auth")]
        [AllowAnonymous]
        public IActionResult Auth(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl ?? Url.Content("~/");
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];
            string nonce = GenerateDigest.GenerateRandom();
            string opaque = GenerateDigest.GenerateRandom();

            HttpContext.Session.SetString("Opaque", opaque);
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\"";

            return View();
        }

        //auth/autor
        [HttpGet("autor")]
        [AllowAnonymous]
        public IActionResult Autor(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl ?? Url.Content("~/");
            ViewBag.RealmHeader = _configuration["Realm"];

            return View();
        }

        //auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            var role = await Auth();
            if (role != null)
            {
                ViewBag.role = role;
                return Json(new { redirectUrl = Url.Action("dashboard", "Dashboard", new { role }) });
            }
            else
            {
                return Json(new { redirectUrl = Url.Action("UnauthorizedCompletely", "Error", new { errorMessage }) });
            }
        }

        //auth/autLog
        [HttpPost("autLog")]
        [AllowAnonymous]
        public IActionResult AutLog(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.Role = UserRole.Customer;
                if (_context.Users.FirstOrDefault(c => c.Email == customer.Email) == null)
                {
                    _context.Users.Add(customer);
                    _context.SaveChangesAsync();
                }
                else
                    return Json(new { redirectUrl = Url.Action("UnauthorizedCompletely", "Error", new { errorMessage = "Данный пользователь уже существует!" }) });
                ViewBag.role = UserRole.Customer;
                return Json(new { redirectUrl = Url.Action("Index", "Home") });
            }
            else
            {
                return Json(new { redirectUrl = Url.Action("autor", "auth") });
            }
        }

        public async Task<UserRole?> Auth()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Digest "))
            {
                errorMessage = "Неправильный дайджест!";
                return null;
            }

            var authParams = ParseAuthorizationHeader(authHeader.Substring("Digest ".Length));

            return await ValidateDigestToken(authParams);
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
                errorMessage = "Неправильный nonce!";
                return null;
            }

            var expectedResponse = await ComputeDigestResponse(username, nonce, uri, nc, cnonce);


            if (response != expectedResponse?.Item1)
            {
                errorMessage = "Неправильный response!";
                return null;
            }
            else
                return expectedResponse.Item2;
        }

        private async Task<Tuple<string, UserRole>?> ComputeDigestResponse(string username, string nonce, string uri, string nc, string cnonce)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
            if (user == null)
            {
                errorMessage = "Пользователь не найден!";
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

        private string ComputeMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
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
    }
}