using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System;
using Microsoft.AspNetCore.Http;
using Azure;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Azure.Core;

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
            string nonce = GenerateRandom();
            string opaque = GenerateRandom();

            HttpContext.Session.SetString(nonce, opaque);

            // Передача данных в представление через ViewBag
            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\"";

            return View();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Digest "))
            {
                return Unauthorized(new { message = "Authorization header is missing or invalid." });
            }

            var authParams = ParseAuthorizationHeader(authHeader.Substring("Digest ".Length));

            try
            {
                if (!await ValidateDigestToken(authParams))
                {
                    return Unauthorized(new { message = "Invalid digest token." });
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }

            // Выполнение других действий (например, создание сессии, авторизация пользователя)
            // ...

            if (returnUrl == null)
            {
                return RedirectToAction("ViewIndex", "Home");
            }
            else
            {
                return Redirect($"{returnUrl}");
            }
        }

        private async Task<bool> ValidateDigestToken(Dictionary<string, string> authParams)
        {
            var username = authParams["username"];
            var nonce = authParams["nonce"];
            var uri = authParams["uri"];
            var nc = authParams["nc"];
            var cnonce = authParams["cnonce"];
            var response = authParams["response"];
            var opaque = authParams["opaque"];

            var expectedOpaque = HttpContext.Session.GetString(nonce);
            if (opaque != expectedOpaque)
            {
                return false;
            }


            var expectedResponse = await ComputeDigestResponse(username, nonce, uri, nc, cnonce);

            // Сравнение вычисленного значения с полученным
            return response == expectedResponse;
        }

        private async Task<string> ComputeDigestResponse(string username, string nonce, string uri, string nc, string cnonce)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
            var password = string.Empty;
            if (user == null)
                throw new Exception("User not found!");
            else
                password = user.PasswordHash;

            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var A1 = $"{username}:{realm}:{password}";
            var A2 = $"POST:{uri}";

            var HA1 = ComputeMD5(A1);
            var HA2 = ComputeMD5(A2);

            var response = ComputeMD5($"{HA1}:{nonce}:{nc}:{cnonce}:{qop}:{HA2}");
            return response;
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

        private string GenerateRandom()
        {
            byte[] randomBytes = new byte[16];
            RandomNumberGenerator.Fill(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }
    }
}