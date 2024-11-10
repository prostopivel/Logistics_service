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

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        //auth/login
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl ?? Url.Content("~/");
            return View();
        }

        //auth/login
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

            string digest = GenerateDigest.Generate(user.PasswordHash);

            HttpContext.Session.SetString(digest, user.Role.ToString());

            if (returnUrl == null)
                return RedirectToAction("ViewIndex", "Home", new { digest });
                //return Redirect($"/home/index?digest={digest}");
            else
                return Redirect($"{returnUrl}?digest={digest}");
        }

        private bool VerifyPassword(User user, string password)
        {
            // Здесь должна быть логика проверки пароля
            return user.PasswordHash == password;
        }
    }
}