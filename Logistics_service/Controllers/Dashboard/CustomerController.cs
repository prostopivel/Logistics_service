using AutoMapper;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Models.Users;
using Logistics_service.Services;
using Logistics_service.Static;
using Logistics_service.ViewModels;
using Logistics_service.ViewModels.MapModels;
using Logistics_service.ViewModels.OrderModels;
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
        private readonly IMapper _mapper;
        public CustomerController(IConfiguration configuration, ApplicationDbContext context,
            WaitingOrderService waitingOrder, VehicleService vehicleService, IMapper mapper)
        {
            _configuration = configuration;
            _context = context;
            _waitingOrder = waitingOrder;
            _vehicleService = vehicleService;
            _mapper = mapper;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager, UserRole.Customer)]
        [HttpGet("viewOrders")]
        public async Task<IActionResult> ViewOrders()
        {
            return await ViewOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager, UserRole.Customer)]
        [HttpGet("viewOrders/{date}")]
        public async Task<IActionResult> ViewOrders([FromRoute] DateTime date)
        {
            ViewData["Title"] = "viewOrders";

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            var rejectedOrders = await _context.CustomerOrders
                .Where(r => r.Email == email && r.Status == OrderStatus.Reject)
                .Select(r => new CustomerOrder
                {
                    Email = r.Email,
                    ArrivalTime = r.ArrivalTime,
                    BeginningAddress = r.BeginningAddress,
                    DestinationAddress = r.DestinationAddress,
                    Reason = r.Reason.Replace('_', ' ')
                })
                .AsNoTracking()
                .ToArrayAsync();

            var waitingOrders = await _context.CustomerOrders
                .Where(w => w.Email == email && (w.Status == OrderStatus.Created || w.Status == OrderStatus.ManagerAccepted))
                .AsNoTracking()
                .ToArrayAsync();

            var readyOrders = await _context.GetOrders()
                .Where(r => r.CustomerEmail == email && r.ArrivalTime.Date == date.Date)
                .AsNoTracking()
                .ToArrayAsync();

            var mapModel = await ViewMap(date);
            return View(new Tuple<CustomerOrderOutputModel[],
                CustomerOrderOutputModel[], ReadyOrderOutputModel[], CustomerMapModel>(
                _mapper.Map<CustomerOrderOutputModel[]>(rejectedOrders),
                _mapper.Map<CustomerOrderOutputModel[]>(waitingOrders),
                _mapper.Map<ReadyOrderOutputModel[]>(readyOrders),
                mapModel));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager, UserRole.Customer)]
        [HttpGet("createRequest")]
        public async Task<IActionResult> CreateRequest()
        {
            ViewData["Title"] = "createRequest";
            ViewBag.Error = TempData["Error"];

            var points = await _context.Points.AsNoTracking().ToArrayAsync();
            return View(new Tuple<CustomerOrderInputModel, PointOutputModel[]>(
                new CustomerOrderInputModel(),
                _mapper.Map<PointOutputModel[]>(points)));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager, UserRole.Customer)]
        [HttpPost("createRequest")]
        public async Task<IActionResult> CreateRequest([FromBody] CustomerOrderInputModel order)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction("createRequest");
            }

            var customerOrder = _mapper.Map<CustomerOrder>(order);

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            customerOrder.Email = email;
            customerOrder.CreatedAt = DateTime.Now;

            if (customerOrder.ArrivalTime.Hour < 8 || customerOrder.ArrivalTime.Hour > 17)
            {
                TempData["Error"] = "Указано неверное время! Время работы сервиса с 8 до 17.";
                return RedirectToAction("createRequest");
            }

            customerOrder.Status = OrderStatus.Created;
            _context.CustomerOrders.Add(customerOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction("customer", "Dashboard");
        }

        private async Task<CustomerMapModel> ViewMap()
        {
            return await ViewMap(DateTime.Now.Date);
        }

        private async Task<CustomerMapModel> ViewMap(DateTime date)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            var points = await _context.Points.AsNoTracking().ToArrayAsync();

            var waitingOrdersRoutes = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted && o.CustomerEmail == email)
                .Select(order => order.Route)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                    .Where(o => o.CustomerEmail == email)
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            var currentOrdersRoutes = currentOrders
                .Select(order => order.Route)
                .ToArray();

            var vehiclesPoints = currentOrders
                .Select(order => order.Vehicle.CurrentPoint)
                .ToArray();

            var vehicles = _vehicleService.Vehicles
                .Where(v => v.CurrentRoute?.CustomerEmail == email)
                .ToArray();

            return new CustomerMapModel(
                _mapper.Map<PointOutputModel[]>(points),
                _mapper.Map<RouteOutputModel[]>(waitingOrdersRoutes),
                _mapper.Map<RouteOutputModel[]>(currentOrdersRoutes),
                _mapper.Map<VehicleOutputModel[]>(vehicles),
                _mapper.Map<PointOutputModel[]>(vehiclesPoints));
        }
    }
}