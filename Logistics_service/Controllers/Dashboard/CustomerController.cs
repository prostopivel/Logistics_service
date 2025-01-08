using Logistics_service.Services;
using Microsoft.AspNetCore.Mvc;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Static;
using Logistics_service.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics_service.Controllers.Dashboard
{
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly OrderQueueService<CustomerOrder> _queueService;
        private readonly WaitingOrderService _waitingOrder;
        private readonly VehicleService _vehicleService;

        public CustomerController(IConfiguration configuration, 
            OrderQueueService<CustomerOrder> queueService, ApplicationDbContext context,
            WaitingOrderService waitingOrder, VehicleService vehicleService)
        {
            _configuration = configuration;
            _queueService = queueService;
            _context = context;
            _waitingOrder = waitingOrder;
            _vehicleService = vehicleService;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewOrders")]
        public IActionResult ViewOrders()
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader["Digest ".Length..])["username"];
            
            var reqect = _context.CustomerOrders
                .Where(r => r.Email == email)
                .Select(r => new CustomerOrder()
                {
                    Email = r.Email,
                    ArrivalTime = r.ArrivalTime,
                    BeginningAddress = r.BeginningAddress,
                    DestinationAddress = r.DestinationAddress,
                    Reason = r.Reason.Replace('_', ' ')
                })
                .ToArray();

            var ready = _context.ReadyOrders
                .Where(r => r.CustomerEmail == email)
                .Include(r => r.Route) 
                .ThenInclude(route => route.DbPoints)
                .Select(r => new ReadyOrder()
                {
                    ArrivalTime = r.ArrivalTime,
                    Email = r.Email,
                    CustomerEmail = r.CustomerEmail,
                    Route = r.Route,
                    Vehicle = r.Vehicle,
                })
                .ToArray(); 

            return View(new Tuple<CustomerOrder[], ReadyOrder[]>(reqect, ready));
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

                _queueService.AddOrder(order);

                return RedirectToAction("customer", "Dashboard");
            }
            return View("createRequest");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public async Task<IActionResult> ViewMap()
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader["Digest ".Length..])["username"];

            var points = await _context.Points.ToArrayAsync();

            var waitingOrders = _waitingOrder.Orders.Values
                .Where(o => o.CustomerEmail == email)
                .ToArray();

            var currentOrders = _waitingOrder.CurrentOrders.Values
                .Where(o => o.CustomerEmail == email)
                .ToArray();

            var vehicles = _vehicleService.Vehicles
                .Where(v => v.CurrentRoute?.CustomerEmail == email)
                .ToArray();

            return View(new Tuple<Point[], Models.Route[], Models.Route[], Vehicle[]>(
                points,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                vehicles));
        }
    }
}
