using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace Logistics_service
{
    public static class DigestAlg
    {
        private static Dictionary<string, string> nonceStore = new Dictionary<string, string>();

        public static string GenerateNonce()
        {
            var nonceBytes = RandomNumberGenerator.GetBytes(32);
            var nonce = Convert.ToBase64String(nonceBytes);
            nonceStore[nonce] = nonce;
            return nonce;
        }

        public static bool ValidateNonce(string nonce)
        {
            return nonceStore.ContainsKey(nonce);
        }

        public static string GenerateDigestResponse(string username, string nonce)
        {
            string realm = "Logistic";
            string qop = "auth";
            string cnonce = Guid.NewGuid().ToString("N");
            int nc = 1;

            string ha1 = CalculateMD5($"{username}:{realm}");
            string ha2 = CalculateMD5($"POST:/api/auth/login");

            string response = CalculateMD5($"{ha1}:{nonce}:{nc:x8}:{cnonce}:{qop}:{ha2}");

            Console.WriteLine($"username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"/api/auth/login\", qop={qop}, nc={nc:x8}, cnonce=\"{cnonce}\", response=\"{response}\"");

            return $"username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"/api/auth/login\", qop={qop}, nc={nc:x8}, cnonce=\"{cnonce}\", response=\"{response}\"";
        }

        private static string CalculateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static Dictionary<string, string> ParseAuthenticateHeader(string header)
        {
            var parameters = new Dictionary<string, string>();
            string[] parts = header.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                string[] keyValue = part.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim().Trim('"');
                    parameters[key] = value;
                }
            }

            return parameters;
        }

        public static bool ValidateDigest(string digest, string username, string nonce)
        {
            return GenerateDigestResponse(username, nonce) == digest;
        }

        public static void RemoveNonce(string nonce)
        {
            nonceStore.Remove(nonce);
        }
    }
}