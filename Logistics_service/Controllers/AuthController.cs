using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Authorization;

namespace Logistics_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(user, request.Password))
            {
                return Unauthorized();
            }

            var token = JwtTokenGenerator.GenerateToken(user, _secretKey, _issuer, _audience);

            switch (user.Role)
            {
                case UserRole.Administrator:
                    return RedirectToAction("AdminDashboard", "Dashboard");
                case UserRole.Manager:
                    return RedirectToAction("ManagerDashboard", "Dashboard");
                case UserRole.Customer:
                    return RedirectToAction("CustomerDashboard", "Dashboard");
                default:
                    return Unauthorized();
            }
        }

        private bool VerifyPassword(User user, string password)
        {
            // Здесь должна быть логика проверки пароля
            return user.PasswordHash == password;
        }
    }
}