using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class ManagerController : Controller
    {
        private readonly IConfiguration _configuration;

        public ManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("assignCar")]
        public IActionResult AssignCar()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("updateRoute")]
        public IActionResult UpdateRoute()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("informClient")]
        public IActionResult InformClient()
        {
            return View();
        }
    }
}
