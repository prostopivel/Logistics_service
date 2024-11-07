using Logistics_service.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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

        [HttpGet("admin")]
        public IActionResult AdminDashboard()
        {
            return AuthenticateAndAuthorize("AdminDashboard");
        }

        [HttpGet("manager")]
        public IActionResult ManagerDashboard()
        {
            return AuthenticateAndAuthorize("ManagerDashboard");
        }

        [HttpGet("customer")]
        public IActionResult CustomerDashboard()
        {
            return AuthenticateAndAuthorize("CustomerDashboard");
        }

        private IActionResult AuthenticateAndAuthorize(string viewName)
        {
            var digestCookie = Request.Cookies["Digest"];
            if (digestCookie != null)
            {
                var authHeader = $"Digest {digestCookie}";
                var authParams = authHeader.Substring("Digest ".Length).Split(',');
                var nonce = authParams.FirstOrDefault(p => p.Trim().StartsWith("nonce="))?.Split('=')[1].Trim('"');
                var username = authParams.FirstOrDefault(p => p.Trim().StartsWith("username="))?.Split('=')[1].Trim('"');
                var response = authParams.FirstOrDefault(p => p.Trim().StartsWith("response="))?.Split('=')[1].Trim('"');

                 if (DigestAlg.ValidateNonce(nonce) && DigestAlg.ValidateDigest(response, username, nonce))
                {
                    // Удаляем nonce после успешной аутентификации
                    DigestAlg.RemoveNonce(nonce);
                    return View(viewName);
                }
            }

            // Генерация нового nonce и отправка заголовка WWW-Authenticate
            string newNonce = DigestAlg.GenerateNonce();
            Response.Headers.Add("WWW-Authenticate", $"Digest realm=\"example\", nonce=\"{newNonce}\", qop=\"auth\", algorithm=MD5");
            return Unauthorized();
        }
    }
}