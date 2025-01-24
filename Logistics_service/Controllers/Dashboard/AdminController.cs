using AutoMapper;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
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
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WaitingOrderService _waitingOrder;
        private readonly VehicleService _vehicleService;
        private readonly IMapper _mapper;

        public AdminController(ApplicationDbContext context, WaitingOrderService waitingOrder, 
            VehicleService vehicleService, IMapper mapper)
        {
            _context = context;
            _waitingOrder = waitingOrder;
            _vehicleService = vehicleService;
            _mapper = mapper;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllOrders")]
        public async Task<IActionResult> ViewAllOrders()
        {
            return await ViewAllOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllOrders/{date}")]
        public async Task<IActionResult> ViewAllOrders([FromRoute] DateTime date)
        {
            ViewData["Title"] = "viewAllOrders";
            ViewBag.Error = TempData["Error"] as string;

            var managerOrders = await _context.GetOrders()
                .Where(o => o.Status == ReadyOrderStatus.Created)
                .AsNoTracking()
                .ToArrayAsync();

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values.ToArray()
                : Array.Empty<ReadyOrder>();

            var mapModel = await ViewMap(date);
            return View("viewAllOrders", new Tuple<ReadyOrderOutputModel[],
                ReadyOrderOutputModel[], ReadyOrderOutputModel[], AdminMapModel>(
                _mapper.Map<ReadyOrderOutputModel[]>(managerOrders),
                _mapper.Map<ReadyOrderOutputModel[]>(waitingOrders),
                _mapper.Map<ReadyOrderOutputModel[]>(currentOrders),
                mapModel));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("assignOrder")]
        public async Task<IActionResult> AssignOrder([FromBody] int id)
        {
            var order = await _context.GetOrders()
                .FirstOrDefaultAsync(o => o.Id == id && o.Route != null && o.Route.DepartureTime != null);

            if (order != null)
            {
                order.Status = ReadyOrderStatus.Accepted;
                await _context.SaveChangesAsync();

                if (order.ArrivalTime.Date == DateTime.Now.Date)
                {
                    _waitingOrder.Orders.TryAdd(order.Route.DepartureTime, order.Id);
                }
            }

            return await ViewAllOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("fixOrder/{id}")]
        public async Task<IActionResult> FixOrder([FromRoute] int id)
        {
            ViewData["Title"] = "fixOrder";
            ViewBag.Error = TempData["Error"] as string;

            var order = await _context.GetOrders()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                order.Route = new Models.Route(order.Route);
            }

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());
            return View(new Tuple<VehicleOutputModel[], ReadyOrderOutputModel?>(
                _mapper.Map<VehicleOutputModel[]>(vehicles),
                _mapper.Map<ReadyOrderOutputModel?>(order)));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("fixOrder")]
        public async Task<IActionResult> FixOrder([FromBody] ManagerOrderInputModel order)
        {
            if (!_context.ReadyOrders.Any(o => o.Id == order.Id))
            {
                return await ViewAllOrders(DateTime.Now.Date);
            }

            var dbOrder = await _context.GetOrders()
                .FirstAsync(o => o.Id == order.Id);

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction("fixOrder", order.Id);
            }

            if (dbOrder.Status == ReadyOrderStatus.Running || dbOrder.Status == ReadyOrderStatus.Completed)
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction("fixOrder", order.Id);
            }

            var points = await _context.Points.AsNoTracking().ToArrayAsync();

            if (order.StartPointId < 0 || order.StartPointId >= points.Length ||
                order.EndPointId < 0 || order.EndPointId >= points.Length)
            {
                TempData["Error"] = "Неверные индексы точек!";
                return RedirectToAction("fixOrder", order.Id);
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
                return RedirectToAction("fixOrder", order.Id);
            }

            var readyOrder = new ReadyOrder(route, vehicle, order.ArrivalTime, dbOrder.CustomerEmail);
            if (readyOrder.Route?.DepartureTime == null)
            {
                TempData["Error"] = "Время выезда не указано!";
                return RedirectToAction("fixOrder", order.Id);
            }

            if (_context.ReadyOrders.Any(
                r => r.Vehicle.Id == readyOrder.Vehicle.Id
                && (r.Status == ReadyOrderStatus.Accepted || r.Status == ReadyOrderStatus.Created || r.Status == ReadyOrderStatus.Running)
                && (r.Route.DepartureTime >= readyOrder.Route.DepartureTime && r.Route.DepartureTime <= readyOrder.ArrivalTime ||
                 r.ArrivalTime >= readyOrder.Route.DepartureTime && r.ArrivalTime <= readyOrder.ArrivalTime)
                 && r.Id != dbOrder.Id))
            {
                TempData["Error"] = "Данное время уже занято!";
                return RedirectToAction("fixOrder", order.Id);
            }

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var email = GenerateDigest.ParseAuthorizationHeader(authHeader)["username"];

            dbOrder.Email = email;
            dbOrder.Status = ReadyOrderStatus.Accepted;
            dbOrder.CreatedAt = DateTime.Now;
            dbOrder.Route = readyOrder.Route;
            dbOrder.RouteId = readyOrder.RouteId;
            dbOrder.Vehicle = readyOrder.Vehicle;
            dbOrder.VehicleId = readyOrder.VehicleId;
            dbOrder.ArrivalTime = readyOrder.ArrivalTime;

            await _context.SaveChangesAsync();

            if (dbOrder.ArrivalTime.Date <= DateTime.Now.Date)
            {
                _waitingOrder.Orders
                    .Where(kvp => kvp.Value == dbOrder.Id)
                    .ToList()
                    .ForEach(kvp => _waitingOrder.Orders.TryRemove(kvp.Key, out _));

                _waitingOrder.Orders.TryAdd(dbOrder.Route.DepartureTime, dbOrder.Id);
            }

            return RedirectToAction("viewAllOrders");
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("manageTransport")]
        public async Task<IActionResult> ManageTransport()
        {
            ViewData["Title"] = "manageTransport";
            ViewBag.Error = TempData["Error"] as string;

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());
            return View(_mapper.Map<VehicleOutputModel[]>(vehicles));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("addTransport")]
        public IActionResult AddTransport()
        {
            ViewData["Title"] = "addTransport";
            ViewBag.Error = TempData["Error"] as string;

            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addTransport")]
        public async Task<IActionResult> AddTransport([FromBody] VehicleInputModel vehicle)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction("addTransport");
            }

            var point = await _context.Points.FirstOrDefaultAsync(p => p.Index == vehicle.GarageId);
            if (point == null)
            {
                TempData["Error"] = "Точка не найдена!";
                return RedirectToAction("addTransport");
            }

            _context.Vehicles.Add(new Vehicle(point, vehicle.Speed));
            await _context.SaveChangesAsync();

            return RedirectToAction("administrator", "Dashboard");
        }

        public async Task<AdminMapModel> ViewMap()
        {
            return await ViewMap(DateTime.Now.Date);
        }

        public async Task<AdminMapModel> ViewMap(DateTime date)
        {
            var points = await _context.Points.AsNoTracking().ToArrayAsync();

            var waitingOrdersRoutes = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .Select(order => order.Route)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values.ToArray()
                : Array.Empty<ReadyOrder>();

            var currentOrdersRoutes = currentOrders
                .Select(order => order.Route)
                .ToArray();

            var vehiclesPoints = currentOrders
                .Select(order => order.Vehicle.CurrentPoint)
                .ToArray();

            var vehicles = _vehicleService.Vehicles.ToArray();

            return new AdminMapModel(
                _mapper.Map<PointOutputModel[]>(points),
                _mapper.Map<RouteOutputModel[]>(waitingOrdersRoutes),
                _mapper.Map<RouteOutputModel[]>(currentOrdersRoutes),
                _mapper.Map<VehicleOutputModel[]>(vehicles),
                _mapper.Map<PointOutputModel[]>(vehiclesPoints));
        }
    }
}