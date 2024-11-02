using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Logistics_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private IConfiguration _configuration;
        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("user")]
        public IActionResult UserDashboard()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            Console.WriteLine("Token: " + token);

            return Ok("User Dashboard");
        }

        [HttpGet("admin")]
        public IActionResult AdminDashboard(string token = null, string role = null)
        {
            var jwtSettings = _configuration.GetSection("Jwt").Get<JwtSettings>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
            {
                return View("Unauthorized");
            }

            var validator = new JwtValidator(jwtSettings.SecretKey, jwtSettings.Issuer, jwtSettings.Audience);
            if (validator.ValidateToken(token, "Administrator"))
            {
                return View();
            }
            else
            {
                return View("Unauthorized");
            }
        }

        [HttpGet("manager")]
        public IActionResult ManagerDashboard(string token = null, string role = null)
        {
            var jwtSettings = _configuration.GetSection("Jwt").Get<JwtSettings>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
            {
                return View("Unauthorized");
            }

            var validator = new JwtValidator(jwtSettings.SecretKey, jwtSettings.Issuer, jwtSettings.Audience);
            if (validator.ValidateToken(token, "Manager")
                || validator.ValidateToken(token, "Administrator"))
            {
                return View();
            }
            else
            {
                return View("Unauthorized");
            }
        }

        [HttpGet("customer")]
        public IActionResult CustomerDashboard(string token = null, string role = null)
        {
            var jwtSettings = _configuration.GetSection("Jwt").Get<JwtSettings>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
            {
                return View("Unauthorized");
            }

            var validator = new JwtValidator(jwtSettings.SecretKey, jwtSettings.Issuer, jwtSettings.Audience);
            if (validator.ValidateToken(token, "Customer")
                || validator.ValidateToken(token, "Manager")
                || validator.ValidateToken(token, "Administrator"))
            {
                return View();
            }
            else
            {
                return View("Unauthorized");
            }
        }
    }
}