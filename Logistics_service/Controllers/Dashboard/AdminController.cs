using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.MapModels;
using Logistics_service.Models.Orders;
using Logistics_service.Models.Users;
using Logistics_service.Services;
using Logistics_service.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Logistics_service.Controllers.Dashboard
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly WaitingOrderService _waitingOrder;
        private readonly VehicleService _vehicleService;

        public AdminController(IConfiguration configuration, ApplicationDbContext context,
            WaitingOrderService waitingOrder, VehicleService vehicleService)
        {
            _configuration = configuration;
            _context = context;
            _waitingOrder = waitingOrder;
            _vehicleService = vehicleService;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllCustomers")]
        public async Task<ActionResult> ViewAllCustomers()
        {
            ViewBag.Error = TempData["Error"] as string;
            var customers = await _context.Customers.AsNoTracking().ToListAsync();
            return View(customers);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllManagers")]
        public async Task<ActionResult> ViewAllManagers()
        {
            ViewBag.Error = TempData["Error"] as string;
            var managers = await _context.Managers.AsNoTracking().ToListAsync();
            var administrators = await _context.Administrators.AsNoTracking().ToListAsync();
            return View(new Tuple<List<Manager>, List<Administrator>>(managers, administrators));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (await _context.Users.FirstOrDefaultAsync(c => c.Id == id) is Customer)
            {
                await DeleteUserAsync(id);
            }
            return await ViewAllCustomers();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteManager/{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            await DeleteUserAsync(id);
            return await ViewAllManagers();
        }

        private async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == id);
            if (user is not null && user.Role != UserRole.Administrator)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Deleted user: {user.Email}");
            }
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("addUser")]
        public IActionResult AddUser()
        {
            ViewBag.Error = TempData["Error"] as string;
            ViewBag.RealmHeader = _configuration["Realm"];
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addUser")]
        public async Task<ActionResult> AddUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("addUser");
            }

            if (!await AddUserAsync(user))
            {
                TempData["Error"] = "Пользователь с данной почтой уже существует!";
                return RedirectToAction("addUser");
            }

            return RedirectToAction("administrator", "Dashboard");
        }

        private async Task<bool> AddUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(c => c.Email == user.Email))
            {
                return false;
            }

            try
            {
                switch (user.Role)
                {
                    case UserRole.Customer:
                        _context.Customers.Add(new Customer(user));
                        break;
                    case UserRole.Manager:
                        _context.Managers.Add(new Manager(user));
                        break;
                    case UserRole.Administrator:
                        _context.Administrators.Add(new Administrator(user));
                        break;
                    default:
                        return false;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Added user: {user.Email}");
                return true;
            }
            catch
            {
                return false;
            }
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
            return View("viewAllOrders", new Tuple<ReadyOrder[], ReadyOrder[], ReadyOrder[], AdminMapModel>(
                managerOrders,
                waitingOrders,
                currentOrders,
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

                if (order.ArrivalTime.Date == DateTime.Now.Date && order.Id.HasValue)
                {
                    _waitingOrder.Orders.TryAdd(order.Route.DepartureTime.Value, order.Id.Value);
                }
            }

            return await ViewAllOrders(DateTime.Now.Date);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("fixOrder/{id}")]
        public async Task<IActionResult> FixOrder([FromRoute] int id)
        {
            ViewBag.Error = TempData["Error"] as string;
            var order = await _context.GetOrders()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                order.Route = new Models.Route(order.Route);
            }

            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());
            return View(new Tuple<Vehicle[], ReadyOrder?>(vehicles, order));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("fixOrder")]
        public async Task<IActionResult> FixOrder([FromBody] ManagerOrder order)
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

            if (dbOrder.ArrivalTime.Date == DateTime.Now.Date && dbOrder.Id.HasValue)
            {
                _waitingOrder.Orders
                    .Where(kvp => kvp.Value == dbOrder.Id.Value)
                    .ToList() 
                    .ForEach(kvp => _waitingOrder.Orders.TryRemove(kvp.Key, out _));

                _waitingOrder.Orders.TryAdd(dbOrder.Route.DepartureTime.Value, dbOrder.Id.Value);
            }

            return RedirectToAction("viewAllOrders");
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("manageTransport")]
        public async Task<IActionResult> ManageTransport()
        {
            ViewBag.Error = TempData["Error"] as string;
            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());
            return View(vehicles);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("addTransport")]
        public IActionResult AddTransport()
        {
            ViewBag.Error = TempData["Error"] as string;
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addTransport")]
        public async Task<IActionResult> AddTransport([FromBody] dynamic vehicle)
        {
            using JsonDocument doc = JsonDocument.Parse(vehicle.ToString());
            JsonElement root = doc.RootElement;

            if (!int.TryParse(root.GetProperty("Speed").GetString(), out int speed) ||
                !int.TryParse(root.GetProperty("GarageId").GetString(), out int garageId))
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction("addTransport");
            }

            var point = await _context.Points.FirstOrDefaultAsync(p => p.Index == garageId);
            if (point == null)
            {
                TempData["Error"] = "Точка не найдена!";
                return RedirectToAction("addTransport");
            }

            _context.Vehicles.Add(new Vehicle(point, speed));
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

            var waitingOrders = _context.GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values.ToArray()
                : Array.Empty<ReadyOrder>();

            var vehicles = _vehicleService.Vehicles.ToArray();

            return new AdminMapModel(
                points,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                vehicles,
                currentOrders.Select(order => order.Vehicle.CurrentPoint).ToArray());
        }
    }
}