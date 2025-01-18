using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.MapModels;
using Logistics_service.Models.Orders;
using Logistics_service.Services;
using Logistics_service.Static;
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

        public ManagerController(ApplicationDbContext context, IMemoryCache cache,
            WaitingOrderService waitingOrder, VehicleService vehicleService)
        {
            _context = context;
            _cache = cache;
            _vehicleService = vehicleService;
            _waitingOrder = waitingOrder;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("getOrder")]
        public async Task<IActionResult> GetOrder()
        {
            ViewBag.Error = TempData["Error"] as string;

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (_cache.TryGetValue($"CurrentOrder_{email}", out _))
            {
                return RedirectToAction("assignOrder", routeValues: null);
            }

            var orders = await _context.CustomerOrders
                .Where(o => o.Status == OrderStatus.Created
                || o.Status == OrderStatus.ManagerAccepted)
                .ToArrayAsync();

            return View(orders);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("getOrder")]
        public async Task<IActionResult> GetOrder([FromBody] CustomerOrder order)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (!_cache.TryGetValue($"CurrentOrder_{email}", out _)
                && _context.CustomerOrders.Any(o => o.Id == order.Id))
            {
                _context.CustomerOrders.First(o => o.Id == order.Id).Status = OrderStatus.ManagerAccepted;
                await _context.SaveChangesAsync();

                order.Status = OrderStatus.ManagerAccepted;

                _cache.Set($"CurrentOrder_{email}", order, TimeSpan.FromMinutes(30));
                return RedirectToAction("assignOrder");
            }
            else if (_cache.TryGetValue($"CurrentOrder_{email}", out _))
            {
                TempData["Error"] = "Вы уже приняли заказ!";
            }
            else
            {
                TempData["Error"] = $"Не удалось найти заказ с ID: {order.Id}!";
            }

            return RedirectToAction("getOrder");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("assignOrder")]
        public async Task<IActionResult> AssignOrder(int start = 0, int end = 0)
        {
            return await AssignOrder(DateTime.Now.Date, start, end);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("assignOrder/{date}")]
        public async Task<IActionResult> AssignOrder(DateTime date, int start = 0, int end = 0)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            var order = _cache.Get<CustomerOrder?>($"CurrentOrder_{email}");

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            var mapModel = start == end && start == 0
                ? await ViewMap(date)
                : await AddMapLine(date, start, end);

            return View(new Tuple<CustomerOrder?, Vehicle[], ReadyOrder[], ReadyOrder[], ManagerMapModel?>(
                order,
                vehicles,
                waitingOrders,
                currentOrders,
                mapModel));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("viewAssignOrder")]
        public IActionResult ViewAssignOrder([FromBody] LineMapModel? line = null)
        {
            return RedirectToAction("assignOrder", new { date = DateTime.Now.Date, start = line?.Start, end = line?.End });
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("viewAssignOrder/{date}")]
        public IActionResult ViewAssignOrder([FromRoute] DateTime date, [FromBody] LineMapModel? line = null)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            return RedirectToAction("assignOrder", new { date = formattedDate, start = line?.Start, end = line?.End });
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("assignOrder")]
        public async Task<IActionResult> AssignOrder([FromBody] ManagerOrder order)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (!_cache.TryGetValue($"CurrentOrder_{email}", out var customerOrder))
            {
                ViewBag.Error = "Заказ еще не принят!";
                return RedirectToAction("assignOrder");
            }

            if (ModelState.IsValid)
            {
                var points = await _context.Points.ToArrayAsync();

                if (order.StartPointId < 0 || order.StartPointId >= points.Length
                    || order.EndPointId < 0 || order.EndPointId >= points.Length)
                {
                    ViewBag.Error = "Неверные индексы точек!";
                    return RedirectToAction("assignOrder");
                }
                var tuple = DijkstraAlgorithm.FindShortestPath(points,
                    points[order.StartPointId], points[order.EndPointId]);

                var route = new Models.Route(tuple.Item1, tuple.Item2)
                {
                    CustomerEmail = order.CustomerEmail
                };

                var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == order.VehicleId);
                if (vehicle is null)
                {
                    ViewBag.Error = "Неверный индекс транспорта!";
                    return RedirectToAction("assignOrder");
                }

                if (order.CustomerEmail is null)
                {
                    ViewBag.Error = "Email заказчика не указан!";
                    return RedirectToAction("assignOrder");
                }

                var readyOrder = new ReadyOrder(route, vehicle, order.ArrivalTime, order.CustomerEmail);
                if (readyOrder.Route is null || readyOrder.Route.DepartureTime is null)
                {
                    ViewBag.Error = "Время выезда не указано!";
                    return RedirectToAction("assignOrder");
                }

                if (_context.ReadyOrders.Any(r => r.Vehicle.Id == readyOrder.Vehicle.Id
                && (r.Status == ReadyOrderStatus.Accepted || r.Status == ReadyOrderStatus.Created || r.Status == ReadyOrderStatus.Running)
                && (r.Route.DepartureTime >= readyOrder.Route.DepartureTime && r.Route.DepartureTime <= readyOrder.ArrivalTime
                || r.ArrivalTime >= readyOrder.Route.DepartureTime && r.ArrivalTime <= readyOrder.ArrivalTime)))
                {
                    ViewBag.Error = "Данное время уже занято!";
                    return RedirectToAction("assignOrder");
                }

                readyOrder.Email = email;
                readyOrder.CreatedAt = DateTime.Now;

                _cache.Remove($"CurrentOrder_{email}");

                if (customerOrder is not null && _context.CustomerOrders.Any(
                    o => o.Id == ((CustomerOrder)customerOrder).Id))
                {
                    _context.CustomerOrders.First(o => o.Id ==
                    ((CustomerOrder)customerOrder).Id).Status = OrderStatus.AdminAccepted;

                    _context.ReadyOrders.Add(readyOrder);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    ViewBag.Error = "Заказ не найден!";
                    return RedirectToAction("assignOrder");
                }

                return RedirectToAction("manager", "Dashboard");
            }

            return RedirectToAction("assignOrder");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("rejectOrder")]
        public async Task<IActionResult> RejectOrder([FromBody] CustomerOrder order)
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            if (!_cache.TryGetValue($"CurrentOrder_{email}", out _))
            {
                ViewBag.Error = "Заказ еще не принят!";
                return View("assignOrder");
            }

            Console.WriteLine($"Заказ от {order.Email} отклонен в {DateTime.Now} менеджером {email}!");

            var rejectOrder = _context.CustomerOrders.FirstOrDefault(o => o.Id == order.Id);

            if (rejectOrder is null)
            {
                ViewBag.Error = "Заказ отсутствует!";
                return View("assignOrder");
            }

            rejectOrder.Status = OrderStatus.Reject;
            rejectOrder.Reason = order.Reason;

            await _context.SaveChangesAsync();

            _cache.Remove($"CurrentOrder_{email}");

            return View("getOrder");
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAssignOrders")]
        public async Task<IActionResult> ViewAssignOrders()
        {
            return await ViewAssignOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAssignOrders/{date}")]
        public async Task<IActionResult> ViewAssignOrders(DateTime date)
        {
            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date
                ? _waitingOrder.GetCurrentOrders().Values
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            var mapModel = await ViewMap(date);

            return View(new Tuple<ReadyOrder[], ReadyOrder[], Vehicle[], ManagerMapModel?>(
                waitingOrders,
                currentOrders,
                vehicles,
                mapModel));
        }

        public async Task<ManagerMapModel> ViewMap()
        {
            return await ViewMap(DateTime.Now.Date);
        }

        public async Task<ManagerMapModel> ViewMap(DateTime date)
        {
            var points = await _context.Points.ToArrayAsync();

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            var vehicles = _vehicleService.Vehicles
                .ToArray();

            return new ManagerMapModel(
                points,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                null,
                null,
                currentOrders.Select(order => order.Vehicle.CurrentPoint).ToArray(),
                vehicles);
        }

        public async Task<ManagerMapModel> AddMapLine(DateTime date, int start, int end)
        {
            var mapModel = await ViewMap(date);

            if (start < 0 || start >= mapModel.Points.Length || end < 0 || end >= mapModel.Points.Length)
            {
                @ViewBag.Error = $"Не входят в диапазон от 0 до {mapModel.Points.Length}!";
                return mapModel;
            }

            var startPoint = mapModel.Points[start];
            var endPoint = mapModel.Points[end];

            var route = DijkstraAlgorithm.FindShortestPath(mapModel.Points, startPoint, endPoint);

            mapModel.Route = route.Item1;
            mapModel.Distanse = route.Item2;

            return mapModel;
        }
    }
}