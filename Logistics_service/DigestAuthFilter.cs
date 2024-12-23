using Logistics_service.Data;
using Logistics_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;

namespace Logistics_service
{
    public class DigestAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public DigestAuthFilter(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

            var opaque = context.HttpContext.Session.GetString("Opaque");

            if (opaque == null)
            {
                ReturnResult();
                return;
            }

            var expectedNonce = context.HttpContext.Session.GetString(opaque);

            if (expectedNonce == null)
            {
                ReturnResult();
                return;
            }

            if (!await GenerateDigest.Auth(authHeader, expectedNonce, _configuration, _context))
            {
                ReturnResult();
                return;
            }

            void ReturnResult()
            {
                var cont = GenerateJson(context);
                if (cont == null)
                    context.Result = new RedirectToActionResult("Unauthorized", "Error", new { errorMessage = "Авторизация не пройдена!" });
                context.Result = cont;
            }
        }

        private IActionResult? GenerateJson(AuthorizationFilterContext context)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = context.HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return null;

            string nonce = GenerateDigest.GenerateRandom();
            context.HttpContext.Session.SetString(opaque, nonce);
            return new JsonResult(new
            {
                realm,
                qop,
                nonce,
                opaque
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

        }
    }
}
