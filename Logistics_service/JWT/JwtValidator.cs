using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Logistics_service.JWT
{
    public class JwtValidator
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtValidator(string secretKey, string issuer, string audience)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
        }

        public bool ValidateToken(string token, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Проверка роли
                if (!principal.IsInRole(role))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}