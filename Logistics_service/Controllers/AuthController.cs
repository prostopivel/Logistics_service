using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Authorization;
using Logistics_service.JWT;

namespace Logistics_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _secretKey = configuration["Jwt:SecretKey"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
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

            var token = JwtTokenGenerator.GenerateToken(user, _secretKey, _issuer, _audience);

            if (returnUrl == null)
                return Redirect($"/home/index?token={token}&role={user.Role}");
            else
                return Redirect($"{returnUrl}?token={token}&role={user.Role}");
        }

        private bool VerifyPassword(User user, string password)
        {
            // Здесь должна быть логика проверки пароля
            return user.PasswordHash == password;
        }
    }
}