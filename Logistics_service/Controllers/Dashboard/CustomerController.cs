using Logistics_service.Services;
using Microsoft.AspNetCore.Mvc;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Static;

namespace Logistics_service.Controllers.Dashboard
{
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly OrderQueueService<CustomerOrder> _queueService;

        public CustomerController(IConfiguration configuration, OrderQueueService<CustomerOrder> queueService)
        {
            _configuration = configuration;
            _queueService = queueService;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewOrders")]
        public IActionResult ViewOrders()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("createRequest")]
        public IActionResult CreateRequest()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("createRequest")]
        public ActionResult CreateRequest([FromBody] CustomerOrder order)
        {
            if (ModelState.IsValid)
            {
                var authHeader = HttpContext.Request.Headers.Authorization.ToString();
                var email = GenerateDigest.ParseAuthorizationHeader(authHeader["Digest ".Length..])["username"];

                order.Email = email;
                order.CreatedAt = DateTime.Now;

                if (order.ArrivalTime.Hour < 8 || order.ArrivalTime.Hour > 17)
                {
                    ViewBag.Error = "Указано неверное время! Время работы сервиса с 8 до 17.";
                    return View("createRequest");
                }

                _queueService.EnqueueOrder(order);

                return RedirectToAction("customer", "Dashboard");
            }
            return View("createRequest");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public IActionResult ViewMap()
        {
            return View();
        }
    }
}
