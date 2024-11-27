using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

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
                context.Result = new RedirectToActionResult("Unauthorized", "Error", null);
                return;
            }

            var expectedNonce = context.HttpContext.Session.GetString(opaque);
            if (expectedNonce == null)
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Error", null);
                return;
            }

            if (!await GenerateDigest.Auth(authHeader, expectedNonce, 
                _configuration, _context))
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Error", null);
            }
        }
    }
}
