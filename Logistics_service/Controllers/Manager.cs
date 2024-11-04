using Microsoft.AspNetCore.Mvc;

namespace Taxi_App.Controllers
{
    public class Manager : Controller
    {
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
