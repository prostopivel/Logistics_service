using Azure.Core;
using Logistics_service.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Logistics_service.Models.Users;

namespace Logistics_service
{
    public static class GenerateDigest
    {
        public static string errorMessage = "Неизвестная ошибка!";
        public static IConfiguration _configuration;
        public static ApplicationDbContext _context;

        public static async Task<bool> Auth(string authHeader,
            string expectedNonce, IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Digest "))
            {
                errorMessage = "Неправильный дайджест!";
                return false;
            }

            var authParams = ParseAuthorizationHeader(authHeader.Substring("Digest ".Length));

            var expectedRole = await ValidateDigestToken(authParams, expectedNonce);

            if (expectedRole == null)
            {
                return false;
            }

            if (!Enum.TryParse(typeof(UserRole), authParams["role"], true, out var role))
            {
                errorMessage = "Неправильная роль!";
                return false;
            }
            return expectedRole == (UserRole)role;
        }

        private static async Task<UserRole?> ValidateDigestToken(Dictionary<string, string> authParams, string expectedNonce)
        {
            var username = authParams["username"];
            var nonce = authParams["nonce"];
            var uri = authParams["uri"];
            var nc = authParams["nc"];
            var cnonce = authParams["cnonce"];
            var response = authParams["response"];

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

        private static async Task<Tuple<string, UserRole>?> ComputeDigestResponse(string username, string nonce, string uri, string nc, string cnonce)
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

        private static string ComputeMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static Dictionary<string, string> ParseAuthorizationHeader(string header)
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