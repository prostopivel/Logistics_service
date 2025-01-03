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
        private readonly ApplicationDbContext _context;
        private readonly OrderQueueService<CustomerOrder> _queueService;
        private readonly OrderQueueService<ReadyOrder> _readyQueueService;
        private readonly IMemoryCache _cache;

        public ManagerController(ApplicationDbContext context, 
            OrderQueueService<CustomerOrder> queueService, IMemoryCache cache, 
            VehicleService vehicleService, OrderQueueService<ReadyOrder> readyQueueService)
        {
            _context = context;
            _queueService = queueService;
            _cache = cache;
            _vehicleService = vehicleService;
            _readyQueueService = readyQueueService;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("getOrder")]
        public IActionResult GetOrder()
        {
            return View(_queueService.PeekAll());
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("getOrder")]
        public IActionResult GetOrder([FromBody] CustomerOrder order)
        {
            if (!_cache.TryGetValue("CurrentOrder", out _) && _queueService.TryDequeueOrder(order))
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

            return View(_queueService.PeekAll());
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
            if (ModelState.IsValid)
            {
                try
                {
                    var points = await _context.Points.ToArrayAsync();

                    if (order.StartPointId < 0 || order.StartPointId >= points.Length
                        || order.EndPointId < 0 || order.EndPointId >= points.Length)
                    {
                        ViewBag.Error = "Неверные индексы точек!";
                        return View();
                    }
                    var tuple = DijkstraAlgorithm.FindShortestPath(points,
                        points[order.StartPointId], points[order.EndPointId]);

                    var route = new Models.Route(tuple.Item1, tuple.Item2);

                    var vehicle = _vehicleService.GetVehicleById(order.VehicleId);

                    if (vehicle is null)
                    {
                        vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == order.VehicleId);
                        if (vehicle is null)
                        {
                            ViewBag.Error = "Неверный индекс транспорта!";
                            return View();
                        }
                    }

                    if (order.CustomerEmail is null)
                    {
                        ViewBag.Error = "Email заказчика не указан!";
                        return View();
                    }

                    var readyOrder = new ReadyOrder(route, vehicle, order.ArrivalTime, order.CustomerEmail);
                    if (readyOrder.Route is null || readyOrder.Route.DepartureTime is null)
                    {
                        ViewBag.Error = "Время выезда не указано!";
                        return View();
                    }

                    var authHeader = HttpContext.Request.Headers.Authorization.ToString();
                    var email = GenerateDigest.ParseAuthorizationHeader(authHeader["Digest ".Length..])["username"];

                    readyOrder.Email = email;
                    readyOrder.CreatedAt = DateTime.Now;

                    _readyQueueService.EnqueueOrder(readyOrder);
                    //await _waitingOrder.AddOrder((DateTime)readyOrder.Route.DepartureTime, readyOrder, _context);

                    return RedirectToAction("manager", "Dashboard");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
            }

            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public async Task<IActionResult> ViewMap()
        {
            _cache.TryGetValue("CurrentOrder", out CustomerOrder? currentOrder);
            var points = await _context.Points.ToArrayAsync();

            return View(new Tuple<Point[], CustomerOrder?, Point[]?, double?>(points, currentOrder, null, null));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addLine")]
        public async Task<IActionResult> AddLine([FromBody] dynamic dataSE)
        {
            var points = await _context.Points.ToArrayAsync();
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

                return View("viewMap", new Tuple<Point[], CustomerOrder?, Point[]?, double?>(points, currentOrder, route.Item1, route.Item2));
            }

            return View("viewMap", new Tuple<Point[], CustomerOrder?, Point[]?, double?>(points, currentOrder, null, null));
        }
    }
}