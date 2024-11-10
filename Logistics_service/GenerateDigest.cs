using Logistics_service.Data;
using System.Text;
using System.Security.Cryptography;

namespace Logistics_service
{
    public static class GenerateDigest
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string Generate(string password)
        {
            string? salt = _configuration["Salt"];

            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
