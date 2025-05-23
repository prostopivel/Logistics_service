﻿using AutoMapper;
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
using Microsoft.Extensions.Caching.Memory;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class ManagerController : Controller
    {
        private readonly VehicleService _vehicleService;
        private readonly WaitingOrderService _waitingOrder;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public ManagerController(ApplicationDbContext context, IMemoryCache cache,
            WaitingOrderService waitingOrder, VehicleService vehicleService, IMapper mapper)
        {
            _context = context;
            _cache = cache;
            _vehicleService = vehicleService;
            _waitingOrder = waitingOrder;
            _mapper = mapper;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpGet("getOrder")]
        public async Task<IActionResult> GetOrder()
        {
            ViewData["Title"] = "getOrder";
            ViewBag.Error = TempData["Error"] as string;

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (_cache.TryGetValue($"CurrentOrder_{email}", out _))
            {
                return RedirectToAction("assignOrder");
            }

            var orders = await _context.CustomerOrders
                .Where(o => o.Status == OrderStatus.Created || o.Status == OrderStatus.ManagerAccepted)
                .AsNoTracking()
                .ToArrayAsync();

            return View(_mapper.Map<CustomerOrderOutputModel[]>(orders));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpPost("getOrder")]
        public async Task<IActionResult> GetOrder([FromBody] CustomerOrderOutputModel order)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (!_cache.TryGetValue($"CurrentOrder_{email}", out _) &&
                await _context.CustomerOrders.AnyAsync(o => o.Id == order.Id))
            {
                var customerOrder = await _context.CustomerOrders.FirstAsync(o => o.Id == order.Id);
                customerOrder.Status = OrderStatus.ManagerAccepted;
                await _context.SaveChangesAsync();

                _cache.Set($"CurrentOrder_{email}", customerOrder, TimeSpan.FromMinutes(30));
                return RedirectToAction("assignOrder");
            }

            TempData["Error"] = _cache.TryGetValue($"CurrentOrder_{email}", out _)
                ? "Вы уже приняли заказ!"
                : $"Не удалось найти заказ с ID: {order.Id}!";

            return RedirectToAction("getOrder");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpGet("assignOrder")]
        public async Task<IActionResult> AssignOrder(int start = 0, int end = 0)
        {
            return await AssignOrder(DateTime.Now.Date, start, end);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpGet("assignOrder/{date}")]
        public async Task<IActionResult> AssignOrder(DateTime date, int start = 0, int end = 0)
        {
            ViewData["Title"] = "assignOrder";
            ViewBag.Error = TempData["Error"] as string;
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            var order = _cache.Get<CustomerOrder?>($"CurrentOrder_{email}");
            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values.ToArray()
                : Array.Empty<ReadyOrder>();

            var mapModel = start == end && start == 0
                ? await ViewMap(date)
                : await AddMapLine(date, start, end);

            return View(new Tuple<CustomerOrderOutputModel?, VehicleOutputModel[],
                ReadyOrderOutputModel[], ReadyOrderOutputModel[], ManagerMapModel>(
                _mapper.Map<CustomerOrderOutputModel?>(order),
                _mapper.Map<VehicleOutputModel[]>(vehicles),
                _mapper.Map<ReadyOrderOutputModel[]>(waitingOrders),
                _mapper.Map<ReadyOrderOutputModel[]>(currentOrders),
                mapModel));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpPost("viewAssignOrder")]
        public IActionResult ViewAssignOrder([FromBody] LineMapModel? line = null)
        {
            return RedirectToAction("assignOrder", new { date = DateTime.Now.Date, start = line?.Start, end = line?.End });
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpPost("viewAssignOrder/{date}")]
        public IActionResult ViewAssignOrder([FromRoute] DateTime date, [FromBody] LineMapModel? line = null)
        {
            return RedirectToAction("assignOrder", new { date = date.ToString("yyyy-MM-dd"), start = line?.Start, end = line?.End });
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpPost("assignOrder")]
        public async Task<IActionResult> AssignOrder([FromBody] ManagerOrderInputModel order)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (!_cache.TryGetValue($"CurrentOrder_{email}", out var cachedOrder))
            {
                TempData["Error"] = "Заказ еще не принят!";
                return RedirectToAction("assignOrder");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction("assignOrder");
            }

            var points = await _context.Points.AsNoTracking().ToArrayAsync();

            if (order.StartPointId < 0 || order.StartPointId >= points.Length ||
                order.EndPointId < 0 || order.EndPointId >= points.Length)
            {
                TempData["Error"] = "Неверные индексы точек!";
                return RedirectToAction("assignOrder");
            }

            var (path, distance) = DijkstraAlgorithm.FindShortestPath(points, points[order.StartPointId], points[order.EndPointId]);

            var route = new Models.Route(path, distance)
            {
                CustomerEmail = order.CustomerEmail
            };

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == order.VehicleId);
            if (vehicle == null)
            {
                TempData["Error"] = "Неверный индекс транспорта!";
                return RedirectToAction("assignOrder");
            }

            var readyOrder = new ReadyOrder(route, vehicle, order.ArrivalTime, order.CustomerEmail);
            if (readyOrder.Route?.DepartureTime == null)
            {
                TempData["Error"] = "Время выезда не указано!";
                return RedirectToAction("assignOrder");
            }

            if (_context.ReadyOrders.Any(r => r.Vehicle.Id == readyOrder.Vehicle.Id &&
                (r.Status == ReadyOrderStatus.Accepted || r.Status == ReadyOrderStatus.Created || r.Status == ReadyOrderStatus.Running) &&
                (r.Route.DepartureTime >= readyOrder.Route.DepartureTime && r.Route.DepartureTime <= readyOrder.ArrivalTime ||
                 r.ArrivalTime >= readyOrder.Route.DepartureTime && r.ArrivalTime <= readyOrder.ArrivalTime)))
            {
                TempData["Error"] = "Данное время уже занято!";
                return RedirectToAction("assignOrder");
            }

            readyOrder.Email = email;
            readyOrder.CreatedAt = DateTime.Now;

            _cache.Remove($"CurrentOrder_{email}");

            if (cachedOrder is CustomerOrder customerOrder &&
                await _context.CustomerOrders.AnyAsync(o => o.Id == customerOrder.Id))
            {
                var dbOrder = await _context.CustomerOrders.FirstAsync(o => o.Id == customerOrder.Id);
                dbOrder.Status = OrderStatus.AdminAccepted;

                _context.ReadyOrders.Add(readyOrder);
                await _context.SaveChangesAsync();

                return RedirectToAction("manager", "Dashboard");
            }

            TempData["Error"] = "Заказ не найден!";
            return RedirectToAction("assignOrder");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpDelete("rejectOrder")]
        public async Task<IActionResult> RejectOrder([FromBody] CustomerOrder order)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные!";
                return View("assignOrder");
            }

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (!_cache.TryGetValue($"CurrentOrder_{email}", out _))
            {
                TempData["Error"] = "Заказ еще не принят!";
                return View("assignOrder");
            }

            Console.WriteLine($"Заказ от {order.Email} отклонен в {DateTime.Now} менеджером {email}!");

            var rejectOrder = await _context.CustomerOrders.FirstOrDefaultAsync(o => o.Id == order.Id);
            if (rejectOrder == null)
            {
                TempData["Error"] = "Заказ отсутствует!";
                return View("assignOrder");
            }

            rejectOrder.Status = OrderStatus.Reject;
            rejectOrder.Reason = order.Reason;

            await _context.SaveChangesAsync();
            _cache.Remove($"CurrentOrder_{email}");

            return View("getOrder");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpGet("viewAssignOrders")]
        public async Task<IActionResult> ViewAssignOrders()
        {
            return await ViewAssignOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator, UserRole.Manager)]
        [HttpGet("viewAssignOrders/{date}")]
        public async Task<IActionResult> ViewAssignOrders(DateTime date)
        {
            ViewData["Title"] = "viewAssignOrders";
            ViewBag.Error = TempData["Error"] as string;

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values.ToArray()
                : Array.Empty<ReadyOrder>();

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());
            var mapModel = await ViewMap(date);

            return View(new Tuple<ReadyOrderOutputModel[], ReadyOrderOutputModel[],
                VehicleOutputModel[], ManagerMapModel>(
                _mapper.Map<ReadyOrderOutputModel[]>(waitingOrders),
                _mapper.Map<ReadyOrderOutputModel[]>(currentOrders),
                _mapper.Map<VehicleOutputModel[]>(vehicles),
                mapModel));
        }

        public async Task<ManagerMapModel> ViewMap()
        {
            return await ViewMap(DateTime.Now.Date);
        }

        public async Task<ManagerMapModel> ViewMap(DateTime date)
        {
            var points = await _context.Points.AsNoTracking().ToArrayAsync();

            var waitingOrdersRoutes = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .Select(order => order.Route).ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                : Array.Empty<ReadyOrder>();

            var currentOrdersRoutes = currentOrders
                .Select(order => order.Route)
                .ToArray();

            var vehiclesPoints = currentOrders
                .Select(order => order.Vehicle.CurrentPoint)
                .ToArray();

            var vehicles = _vehicleService.Vehicles.ToArray();

            return new ManagerMapModel(
                _mapper.Map<PointOutputModel[]>(points),
                _mapper.Map<RouteOutputModel[]>(waitingOrdersRoutes),
                _mapper.Map<RouteOutputModel[]>(currentOrdersRoutes),
                null,
                null,
                _mapper.Map<PointOutputModel[]>(vehiclesPoints),
                _mapper.Map<VehicleOutputModel[]>(vehicles));
        }

        public async Task<ManagerMapModel> AddMapLine(DateTime date, int start, int end)
        {
            var points = await _context.Points.AsNoTracking().ToArrayAsync();

            var waitingOrdersRoutes = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .Select(order => order.Route).ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                : Array.Empty<ReadyOrder>();

            var currentOrdersRoutes = currentOrders
                .Select(order => order.Route)
                .ToArray();

            var vehiclesPoints = currentOrders
                .Select(order => order.Vehicle.CurrentPoint)
                .ToArray();

            var vehicles = _vehicleService.Vehicles.ToArray();

            var mapModel = new ManagerMapModel(
                _mapper.Map<PointOutputModel[]>(points),
                _mapper.Map<RouteOutputModel[]>(waitingOrdersRoutes),
                _mapper.Map<RouteOutputModel[]>(currentOrdersRoutes),
                null,
                null,
                _mapper.Map<PointOutputModel[]>(vehiclesPoints),
                _mapper.Map<VehicleOutputModel[]>(vehicles));

            if (start < 0 || start >= points.Length || end < 0 || end >= points.Length)
            {
                TempData["Error"] = $"Не входят в диапазон от 0 до {points.Length}!";
                return mapModel;
            }

            var startPoint = points[start];
            var endPoint = points[end];

            var route = DijkstraAlgorithm.FindShortestPath(points, startPoint, endPoint);

            mapModel.Route = _mapper.Map<PointOutputModel[]>(route.Item1);
            mapModel.Distance = route.Item2;

            return mapModel;
        }
    }
}