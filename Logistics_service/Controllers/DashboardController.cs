using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Logistics_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("admin")]
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminDashboard()
        {
            return Ok("Admin Dashboard");
        }

        [HttpGet("manager")]
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerDashboard()
        {
            return Ok("Manager Dashboard");
        }

        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        public IActionResult CustomerDashboard()
        {
            return Ok("Customer Dashboard");
        }
    }
}