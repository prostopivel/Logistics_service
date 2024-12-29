using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Logistics_service.Models.Orders;
using Logistics_service.Services;
using Microsoft.Extensions.Caching.Memory;
using Logistics_service.Static;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderQueueService<CustomerOrder> _queueService;
        private readonly IMemoryCache _cache;

        public ManagerController(ApplicationDbContext context, OrderQueueService<CustomerOrder> queueService, IMemoryCache cache)
        {
            _context = context;
            _queueService = queueService;
            _cache = cache;
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
            if (!_cache.TryGetValue("CurrentOrder", out CustomerOrder? currentOrder) && _queueService.TryDequeueOrder(order))
            {
                _cache.Set("CurrentOrder", order, TimeSpan.FromMinutes(30));
            }
            else if (_cache.TryGetValue("CurrentOrder", out currentOrder))
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
        public IActionResult AssignOrder()
        {
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
        [HttpGet("viewCars")]
        public IActionResult ViewCars()
        {
            return View();
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

            return View(new Tuple<Point[], CustomerOrder?, Point[]?, double?>(points, currentOrder, null, null));
        }
    }
}