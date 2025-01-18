using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.MapModels;
using Logistics_service.Models.Orders;
using Logistics_service.Services;
using Logistics_service.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics_service.Controllers.Dashboard
{
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly WaitingOrderService _waitingOrder;
        private readonly VehicleService _vehicleService;

        public CustomerController(IConfiguration configuration, ApplicationDbContext context,
            WaitingOrderService waitingOrder, VehicleService vehicleService)
        {
            _configuration = configuration;
            _context = context;
            _waitingOrder = waitingOrder;
            _vehicleService = vehicleService;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewOrders")]
        public async Task<IActionResult> ViewOrders()
        {
            return await ViewOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewOrders/{date}")]
        public async Task<IActionResult> ViewOrders([FromRoute] DateTime date)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            var reqect = _context.CustomerOrders
                .Where(r => r.Email == email && r.Status == OrderStatus.Reject)
                .Select(r => new CustomerOrder()
                {
                    Email = r.Email,
                    ArrivalTime = r.ArrivalTime,
                    BeginningAddress = r.BeginningAddress,
                    DestinationAddress = r.DestinationAddress,
                    Reason = r.Reason.Replace('_', ' ')
                })
                .ToArray();

            var wait = _context.CustomerOrders
                .Where(w => w.Email == email && w.Status == OrderStatus.Created
                || w.Status == OrderStatus.ManagerAccepted)
                .Select(w => new CustomerOrder()
                {
                    Email = w.Email,
                    ArrivalTime = w.ArrivalTime,
                    BeginningAddress = w.BeginningAddress,
                    DestinationAddress = w.DestinationAddress,
                    Status = w.Status
                })
                .ToArray();

            var ready = _context
                .GetOrders()
                .Where(r => r.CustomerEmail == email && r.ArrivalTime.Date == date)
                .Select(r => new ReadyOrder()
                {
                    ArrivalTime = r.ArrivalTime,
                    Email = r.Email,
                    CustomerEmail = r.CustomerEmail,
                    Route = r.Route,
                    Vehicle = r.Vehicle,
                    Status = r.Status
                })
                .ToArray();

            return View(new Tuple<CustomerOrder[], CustomerOrder[], ReadyOrder[], CustomerMapModel>(
                reqect,
                wait,
                ready,
                await ViewMap(date)));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("createRequest")]
        public async Task<IActionResult> CreateRequest()
        {
            ViewBag.Error = TempData["Error"];
            return View(new Tuple<CustomerOrder, Point[]>(
                new CustomerOrder(),
                await _context.Points.ToArrayAsync()));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("createRequest")]
        public async Task<IActionResult> CreateRequest([FromBody] CustomerOrder order)
        {
            if (ModelState.IsValid)
            {
                var authHeader = HttpContext.Request.Headers.Authorization.ToString();
                var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

                order.Email = email;
                order.CreatedAt = DateTime.Now;

                if (order.ArrivalTime.Hour < 8 || order.ArrivalTime.Hour > 17)
                {
                    TempData["Error"] = "Указано неверное время! Время работы сервиса с 8 до 17.";
                    return RedirectToAction("createRequest");
                }

                order.Status = OrderStatus.Created;
                _context.CustomerOrders.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction("customer", "Dashboard");
            }

            return RedirectToAction("createRequest");
        }

        private async Task<CustomerMapModel> ViewMap()
        {
            return await ViewMap(DateTime.Now.Date);
        }
        private async Task<CustomerMapModel> ViewMap(DateTime date)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];
            var points = await _context.Points.ToArrayAsync();

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date
                ? _waitingOrder.GetCurrentOrders().Values
                    .Where(o => o.CustomerEmail == email)
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            var vehicles = _vehicleService.Vehicles
                .Where(v => v.CurrentRoute?.CustomerEmail == email)
                .ToArray();

            return new CustomerMapModel(
                points,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                vehicles,
                currentOrders.Select(order => order.Vehicle.CurrentPoint).ToArray());
        }
    }
}
