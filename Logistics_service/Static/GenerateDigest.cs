using Logistics_service.Models.Users;
using Logistics_service.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Logistics_service.Static
{
    public static class GenerateDigest
    {
        public static string ErrorMessage { get; private set; } = "Неизвестная ошибка!";
        public static IConfiguration Configuration { get; set; }
        public static ApplicationDbContext Context { get; set; }

        public static async Task<bool> Auth(string authHeader, string expectedNonce, IConfiguration configuration, ApplicationDbContext context)
        {
            Configuration = configuration;
            Context = context;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Digest "))
            {
                ErrorMessage = "Неправильный дайджест!";
                return false;
            }

            var authParams = ParseAuthorizationHeader(authHeader["Digest ".Length..]);

            var expectedRole = await ValidateDigestToken(authParams, expectedNonce);
            if (expectedRole == null)
            {
                return false;
            }

            if (!Enum.TryParse(authParams["role"], true, out UserRole role))
            {
                ErrorMessage = "Неправильная роль!";
                return false;
            }

            return expectedRole == role;
        }

        private static async Task<UserRole?> ValidateDigestToken(Dictionary<string, string> authParams, string expectedNonce)
        {
            var username = authParams["username"];
            var nonce = authParams["nonce"];
            var response = authParams["response"];

            if (nonce != expectedNonce)
            {
                ErrorMessage = "Неправильный nonce!";
                return null;
            }

            var expectedResponse = await ComputeDigestResponse(username, nonce, authParams["uri"], authParams["nc"], authParams["cnonce"]);
            if (expectedResponse == null)
            {
                return null;
            }

            if (response != expectedResponse.Item1)
            {
                ErrorMessage = "Неправильный response!";
                return null;
            }

            return expectedResponse.Item2;
        }

        private static async Task<Tuple<string, UserRole>?> ComputeDigestResponse(string username, string nonce, string uri, string nc, string cnonce)
        {
            var user = await Context.Users.FirstOrDefaultAsync(u => u.Email == username);
            if (user == null)
            {
                ErrorMessage = "Пользователь не найден!";
                return null;
            }

            string realm = Configuration["Realm"];
            string qop = Configuration["Qop"];

            var A2 = $"POST:{uri}";
            var HA1 = user.PasswordHash;
            var HA2 = ComputeMD5(A2);

            var response = ComputeMD5($"{HA1}:{nonce}:{nc}:{cnonce}:{qop}:{HA2}");
            return new Tuple<string, UserRole>(response, user.Role);
        }

        public static string ComputeMD5(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public static Dictionary<string, string> ParseAuthorizationHeader(string header)
        {
            if (header.StartsWith("Digest "))
            {
                header = header.Substring("Digest ".Length);
            }

            var paramsDict = new Dictionary<string, string>();
            var parts = header.Split(',');

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    paramsDict[kv[0].Trim()] = kv[1].Trim('"');
                }
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