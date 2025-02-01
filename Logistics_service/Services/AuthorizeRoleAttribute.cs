using Logistics_service.Models.Users;

namespace Logistics_service.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeRoleAttribute : Attribute
    {
        public UserRole[] AllowedRoles { get; }

        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }
    }
}