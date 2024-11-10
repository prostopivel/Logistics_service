using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Logistics_service.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Admin(string digest)
        {
            if (AuthenticateAndAuthorize("AdminDashboard", digest))
                return View("AdminDashboard", digest);
            else
                return View("Unauthorized");
        }

        public IActionResult Manager(string digest)
        {
            if (AuthenticateAndAuthorize("ManagerDashboard", digest))
                return View("ManagerDashboard", digest);
            else
                return View("Unauthorized");
        }

        public IActionResult Customer(string digest)
        {
            if (AuthenticateAndAuthorize("CustomerDashboard", digest))
                return View("CustomerDashboard", digest);
            else
                return View("Unauthorized"); 
        }

        private bool AuthenticateAndAuthorize(string viewName, string digest)
        {
            string? userRole = HttpContext.Session.GetString(digest);

            if (userRole == null || !Enum.TryParse(typeof(UserRole), userRole, out var result))
                return false;
            switch (viewName)
            {
                case "AdminDashboard":
                    if ((UserRole)result == UserRole.Administrator)
                        return true;
                    else
                        return false;
                case "ManagerDashboard":
                    if ((UserRole)result == UserRole.Administrator 
                        || (UserRole)result == UserRole.Manager)
                        return true;
                    else
                        return false;
                case "CustomerDashboard":
                    if ((UserRole)result == UserRole.Administrator
                        || (UserRole)result == UserRole.Manager
                        || (UserRole)result == UserRole.Customer)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }
    }
}