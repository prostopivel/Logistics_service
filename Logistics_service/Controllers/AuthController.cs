using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System;
using Microsoft.AspNetCore.Http;

namespace Logistics_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        //api/auth/login
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl ?? Url.Content("~/");
            return View();
        }

        //api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginRequest request, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", request);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(user, request.Password))
            {
                return View("Unauthorized");
            }

            string nonce = DigestAlg.GenerateNonce();

            string digestResponse = DigestAlg.GenerateDigestResponse(user.Email, nonce);

            // Сохранение информации о пользователе в сессии
            HttpContext.Session.SetString("Username", user.Email);
            HttpContext.Session.SetString("DigestResponse", digestResponse);
            HttpContext.Session.SetString("Nonce", nonce);
            HttpContext.Session.SetString("Role", user.Role.ToString());

            Response.Cookies.Append("Digest", digestResponse, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });


            if (returnUrl == null)
                return Redirect($"/home/index");
            else
                return Redirect(returnUrl);
        }

        private bool VerifyPassword(User user, string password)
        {
            // Здесь должна быть логика проверки пароля
            return user.PasswordHash == password;
        }
    }
}