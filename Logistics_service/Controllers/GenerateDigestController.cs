using Azure.Core;
using Logistics_service.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class GenerateDigestController : ControllerBase
    {
        private IConfiguration _configuration;
        private ApplicationDbContext _context;
        private string uri;

        public GenerateDigestController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("auth")]
        public async Task<ActionResult> Auth()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Digest "))
            {
                return StatusCode(401, "Authorization header is missing or invalid.");
            }

            var authParams = ParseAuthorizationHeader(authHeader.Substring("Digest ".Length));

            try
            {
                if (!await ValidateDigestToken(authParams))
                {
                    return StatusCode(401, "Invalid digest token.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }

            return Redirect($"{uri}");
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

            this.uri = uri;

            var expectedNonce = HttpContext.Session.GetString(opaque);
            if (nonce != expectedNonce)
            {
                return false;
            }

            var expectedResponse = await ComputeDigestResponse(username, nonce, uri, nc, cnonce);

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

        public static string GenerateRandom()
        {
            byte[] randomBytes = new byte[16];
            RandomNumberGenerator.Fill(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }
    }
}