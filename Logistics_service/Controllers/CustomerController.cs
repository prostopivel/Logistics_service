﻿using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        public IActionResult Index(string digest)
        {
            return View();
        }

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
