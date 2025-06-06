﻿using Logistics_service.Models.Users;
using Logistics_service.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Logistics_service.Services
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
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            var opaque = context.HttpContext.Session.GetString("Opaque");

            if (opaque == null)
            {
                ReturnUnauthorizedResult(context);
                return;
            }

            var expectedNonce = context.HttpContext.Session.GetString(opaque);

            if (expectedNonce == null)
            {
                ReturnUnauthorizedResult(context);
                return;
            }

            if (!await GenerateDigest.Auth(authHeader, expectedNonce, _configuration, _context))
            {
                ReturnUnauthorizedResult(context);
                return;
            }

            var authorizeRoleAttributes = context.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeRoleAttribute>()
                .ToList();

            if (authorizeRoleAttributes.Any())
            {
                var userRoleHeader = GenerateDigest.ParseAuthorizationHeader(authHeader)["role"];
                if (string.IsNullOrEmpty(userRoleHeader) || !Enum.TryParse(userRoleHeader, out UserRole userRole))
                {
                    ReturnUnauthorizedResult(context);
                    return;
                }

                if (!authorizeRoleAttributes.Any(attr => attr.AllowedRoles.Contains(userRole)))
                {
                    ReturnUnauthorizedResult(context);
                    return;
                }
            }
        }

        private void ReturnUnauthorizedResult(AuthorizationFilterContext context)
        {
            var jsonResult = GenerateJsonResult(context);
            context.Result = jsonResult ?? new RedirectToActionResult("Unauthorized", "Error", new { errorMessage = "Авторизация не пройдена!" });
        }

        private IActionResult? GenerateJsonResult(AuthorizationFilterContext context)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = context.HttpContext.Session.GetString("Opaque");
            if (opaque == null)
            {
                return null;
            }

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