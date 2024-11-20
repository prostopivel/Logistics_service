using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    [Route("api/[controller]")]
    public class ManagerController : Controller
    {
        public IActionResult Index(string digest)
        {
            return View();
        }

        public IActionResult AssignCar()
        {
            return View();
        }

        public IActionResult UpdateRoute()
        {
            return View();
        }

        public IActionResult InformClient()
        {
            return View();
        }
    }
}
