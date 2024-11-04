using Microsoft.AspNetCore.Mvc;

namespace Taxi_App.Controllers
{
    public class Admin : Controller
    {
        public IActionResult ViewAllCustomers()
        {
            return View();
        }

        public IActionResult ViewAllManagers()
        {
            return View();
        }

        public IActionResult AddManager()
        {
            return View();
        }

        public IActionResult ViewAllOrders()
        {
            return View();
        }

        public IActionResult ManageTransport()
        {
            return View();
        }

        public IActionResult ViewMap()
        {
            return View();
        }
    }
}
