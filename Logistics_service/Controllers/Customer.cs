using Microsoft.AspNetCore.Mvc;

namespace Taxi_App.Controllers
{
    public class Customer : Controller
    {
        public IActionResult ViewOrders()
        {
            return View();
        }

        public IActionResult CreateRequest()
        {
            return View();
        }

        public IActionResult ViewMap()
        {
            return View();
        }
    }
}
