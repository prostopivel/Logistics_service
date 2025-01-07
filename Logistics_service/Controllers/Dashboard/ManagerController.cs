using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Services;
using Logistics_service.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class ManagerController : Controller
    {
        private readonly VehicleService _vehicleService;
        private readonly WaitingOrderService _waitingOrder;
        private readonly ApplicationDbContext _context;
        private readonly OrderQueueService<CustomerOrder> _queueService;
        private readonly OrderQueueService<ReadyOrder> _readyQueueService;
        private readonly IMemoryCache _cache;

        public ManagerController(ApplicationDbContext context, WaitingOrderService waitingOrder,
            OrderQueueService<CustomerOrder> queueService, IMemoryCache cache, 
            VehicleService vehicleService, OrderQueueService<ReadyOrder> readyQueueService)
        {
            _context = context;
            _queueService = queueService;
            _cache = cache;
            _vehicleService = vehicleService;
            _readyQueueService = readyQueueService;
            _waitingOrder = waitingOrder;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("getOrder")]
        public IActionResult GetOrder()
        {
            return View(_queueService.Orders);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("getOrder")]
        public IActionResult GetOrder([FromBody] CustomerOrder order)
        {
            if (!_cache.TryGetValue("CurrentOrder", out _) && _queueService.TryDeleteOrder(order))
            {
                _cache.Set("CurrentOrder", order, TimeSpan.FromMinutes(30));
            }
            else if (_cache.TryGetValue("CurrentOrder", out _))
            {
                ViewBag.Error = "Вы уже приняли заказ!";
            }
            else
            {
                ViewBag.Error = $"Не удалось найти заказ с ID: {order.Id}!";
            }

            return View(_queueService.Orders);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("assignOrder")]
        public async Task<IActionResult> AssignOrder()
        {
            var order = _cache.Get<CustomerOrder?>("CurrentOrder");
            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, order));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("assignOrder")]
        public async Task<IActionResult> AssignOrder([FromBody] ManagerOrder order)
        {
            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            if (!_cache.TryGetValue("CurrentOrder", out var customerOrder))
            {
                ViewBag.Error = "Заказ еще не принят!";
                return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, null));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var points = await _context.Points.ToArrayAsync();

                    if (order.StartPointId < 0 || order.StartPointId >= points.Length
                        || order.EndPointId < 0 || order.EndPointId >= points.Length)
                    {
                        ViewBag.Error = "Неверные индексы точек!";
                        return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, (CustomerOrder?)customerOrder));
                    }
                    var tuple = DijkstraAlgorithm.FindShortestPath(points,
                        points[order.StartPointId], points[order.EndPointId]);

                    var route = new Models.Route(tuple.Item1, tuple.Item2);

                    var vehicle = _vehicleService.GetVehicleById(order.VehicleId);

                    if (vehicle is null)
                    {
                        vehicle = await _context.GetVehicleAsync(order.VehicleId);
                        if (vehicle is null)
                        {
                            ViewBag.Error = "Неверный индекс транспорта!";
                            return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, (CustomerOrder?)customerOrder));
                        }
                    }

                    if (order.CustomerEmail is null)
                    {
                        ViewBag.Error = "Email заказчика не указан!";
                        return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, (CustomerOrder?)customerOrder));
                    }

                    var readyOrder = new ReadyOrder(route, vehicle, order.ArrivalTime, order.CustomerEmail);
                    if (readyOrder.Route is null || readyOrder.Route.DepartureTime is null)
                    {
                        ViewBag.Error = "Время выезда не указано!";
                        return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, (CustomerOrder?)customerOrder));
                    }

                    var authHeader = HttpContext.Request.Headers.Authorization.ToString();
                    var email = GenerateDigest.ParseAuthorizationHeader(authHeader["Digest ".Length..])["username"];

                    readyOrder.Email = email;
                    readyOrder.CreatedAt = DateTime.Now;

                    _cache.Remove("CurrentOrder");
                    _readyQueueService.AddOrder(readyOrder);

                    return RedirectToAction("manager", "Dashboard");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
            }

            return View(new Tuple<Vehicle[], CustomerOrder?>(vehicles, (CustomerOrder?)customerOrder));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("rejectOrder")]
        public async Task<IActionResult> RejectOrder([FromBody] CustomerOrder order)
        {
            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            if (!_cache.TryGetValue("CurrentOrder", out _))
            {
                ViewBag.Error = "Заказ еще не принят!";
                return View("assignOrder", new Tuple<Vehicle[], CustomerOrder?>(vehicles, order));
            }

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader["Digest ".Length..])["username"];
            Console.WriteLine($"Заказ от {order.Email} отклонен в {DateTime.Now} менеджером {email}!");
            _context.CustomerOrders.Add(order);
            await _context.SaveChangesAsync();
            _cache.Remove("CurrentOrder");
            return View("assignOrder", new Tuple<Vehicle[], CustomerOrder?>(vehicles, order));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public async Task<IActionResult> ViewMap()
        {
            _cache.TryGetValue("CurrentOrder", out CustomerOrder? currentOrder);
            var points = await _context.Points.ToArrayAsync();
            var waitingOrders = _waitingOrder.Orders.Values.ToArray();
            var currentOrders = _waitingOrder.CurrentOrders.Values.ToArray();

            return View(new Tuple<Point[], CustomerOrder?, Models.Route[], Models.Route[], Point[]?, double?>(
                points,
                currentOrder,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                null, null));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addLine")]
        public async Task<IActionResult> AddLine([FromBody] dynamic dataSE)
        {
            var points = await _context.Points.ToArrayAsync();
            var waitingOrders = _waitingOrder.Orders.Values.ToArray();
            var currentOrders = _waitingOrder.CurrentOrders.Values.ToArray();
            _cache.TryGetValue("CurrentOrder", out CustomerOrder? currentOrder);

            using JsonDocument doc = JsonDocument.Parse(dataSE.ToString());
            JsonElement root = doc.RootElement;

            string? startString = root.GetProperty("Start").GetString();
            string? endString = root.GetProperty("End").GetString();

            if (!int.TryParse(startString, out int start) || !int.TryParse(endString, out int end))
            {
                @ViewBag.Error = "Не число!";
            }
            else if (start < 0 || start >= points.Length || end < 0 || end >= points.Length)
            {
                @ViewBag.Error = $"Не входят в диапазон от 0 до {points.Length}!";
            }
            else
            {
                var startPoint = points[start];
                var endPoint = points[end];

                var route = DijkstraAlgorithm.FindShortestPath(points, startPoint, endPoint);

                return View("viewMap", new Tuple<Point[], CustomerOrder?, Models.Route[], Models.Route[], Point[]?, double?>(
                    points,
                    currentOrder,
                    waitingOrders.Select(order => order.Route).ToArray(),
                    currentOrders.Select(order => order.Route).ToArray(),
                    route.Item1,
                    route.Item2));
            }

            return View("viewMap", new Tuple<Point[], CustomerOrder?, Models.Route[], Models.Route[], Point[]?, double?>(
                points,
                currentOrder,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                null, null));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAssignOrders")]
        public IActionResult ViewAssignOrders()
        {
            var waitingOrders = _waitingOrder.Orders.Values.ToArray();
            var currentOrders = _waitingOrder.CurrentOrders.Values.ToArray();
            return View(new Tuple<ReadyOrder[], ReadyOrder[]>(currentOrders, waitingOrders));
        }
    }
}