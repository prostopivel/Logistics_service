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
            return View(await _context.Customers.ToListAsync());
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllManagers")]
        public async Task<ActionResult> ViewAllManagers()
        {
            return View(new Tuple<List<Manager>, List<Administrator>>(
                await _context.Managers.ToListAsync(),
                await _context.Administrators.ToListAsync()));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            await DeleteUserAsync(id);

            return View("viewAllCustomers", _context.Customers.ToListAsync());
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteManager/{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            await DeleteUserAsync(id);

            return View("viewAllManagers", _context.Managers.ToListAsync());
        }

        private async Task DeleteUserAsync(int id)
        {
            var user = _context.Users.FirstOrDefault(c => c.Id == id);
            if (user is not null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                Console.WriteLine("Delete: " + user.Email);
            }
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("addUser")]
        public IActionResult AddUser()
        {
            ViewBag.RealmHeader = _configuration["Realm"];
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addUser")]
        public async Task<ActionResult> AddUser([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                if (!await AddUserAsync(user))
                {
                    ViewBag.Error = "Пользователь с данной почтой уже существует!";
                    return RedirectToAction("addUser");
                }

                return RedirectToAction("administrator", "Dashboard");
            }

            return RedirectToAction("addUser");
        }

        private async Task<bool> AddUserAsync(User user)
        {
            if (_context.Users.FirstOrDefault(c => c.Email == user.Email) is null)
            {
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
                }
                catch
                {
                    return false;
                }

                await _context.SaveChangesAsync();

                Console.WriteLine("Add: " + user.Email);
                return true;
            }
            return false;
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
            var managerOrders = await _context
                .GetOrders()
                .Where(o => o.Status == ReadyOrderStatus.Created)
                .ToArrayAsync();

            var waitingOrders = _context
                .GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            return View("viewAllOrders", new Tuple<ReadyOrder[], ReadyOrder[], ReadyOrder[], AdminMapModel>(
                managerOrders,
                waitingOrders,
                currentOrders,
                await ViewMap(date)));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("assignOrder")]
        public async Task<IActionResult> AssignOrder([FromBody] int Id)
        {
            if (_context.ReadyOrders.Any(o => o.Id == Id && o.Route != null
            && o.Route.DepartureTime != null))
            {
                var order = _context
                    .GetOrders()
                    .First(o => o.Id == Id);
                order.Status = ReadyOrderStatus.Accepted;

                await _context.SaveChangesAsync();

                if (order.ArrivalTime.Date == DateTime.Now.Date && order.Id is not null)
                {
                    _waitingOrder.Orders.TryAdd((DateTime)order.Route.DepartureTime!, (int)order.Id);
                }
            }

            return await ViewAllOrders(DateTime.Now);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("fixOrder/{id}")]
        public async Task<IActionResult> FixOrder([FromRoute] int Id)
        {
            var order = _context
                .GetOrders()
                .FirstOrDefault(o => o.Id == Id);

            if (order is not null)
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
            if (_context.ReadyOrders.Any(o => o.Id == order.Id))
            {
                var dbOrder = _context
                    .GetOrders()
                    .First(o => o.Id == order.Id);

                var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

                if (ModelState.IsValid)
                {
                    var points = await _context.Points.ToArrayAsync();

                    if (order.StartPointId < 0 || order.StartPointId >= points.Length
                        || order.EndPointId < 0 || order.EndPointId >= points.Length)
                    {
                        ViewBag.Error = "Неверные индексы точек!";
                        return View("fixOrder", new Tuple<Vehicle[], ReadyOrder?>(vehicles, dbOrder));
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
                        return View("fixOrder", new Tuple<Vehicle[], ReadyOrder?>(vehicles, dbOrder));
                    }

                    var readyOrder = new ReadyOrder(route, vehicle, order.ArrivalTime, dbOrder.CustomerEmail);
                    if (readyOrder.Route is null || readyOrder.Route.DepartureTime is null)
                    {
                        ViewBag.Error = "Время выезда не указано!";
                        return View("fixOrder", new Tuple<Vehicle[], ReadyOrder?>(vehicles, dbOrder));
                    }

                    if (_context.ReadyOrders.Any(r => r.Vehicle.Id == readyOrder.Vehicle.Id
                    && (r.Status == ReadyOrderStatus.Accepted || r.Status == ReadyOrderStatus.Created || r.Status == ReadyOrderStatus.Running)
                    && (r.Route.DepartureTime >= readyOrder.Route.DepartureTime && r.Route.DepartureTime <= readyOrder.ArrivalTime
                    || r.ArrivalTime >= readyOrder.Route.DepartureTime && r.ArrivalTime <= readyOrder.ArrivalTime)
                    && r.Id != dbOrder.Id))
                    {
                        ViewBag.Error = "Данное время уже занято!";
                        return View("fixOrder", new Tuple<Vehicle[], ReadyOrder?>(vehicles, dbOrder));
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
                }

                if (dbOrder.ArrivalTime.Date == DateTime.Now.Date && dbOrder.Id is not null)
                {
                    _waitingOrder.Orders.TryAdd((DateTime)dbOrder.Route.DepartureTime!, (int)dbOrder.Id);
                }
            }

            return await ViewAllOrders(DateTime.Now);
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("manageTransport")]
        public async Task<IActionResult> ManageTransport()
        {
            var vehicles = _vehicleService.GetAllVehicles(await _context.GetVehiclesAsync());

            return View(vehicles);
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("addTransport")]
        public IActionResult AddTransport()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addTransport")]
        public async Task<IActionResult> AddTransport([FromBody] dynamic vehicle)
        {
            using JsonDocument doc = JsonDocument.Parse(vehicle.ToString());
            JsonElement root = doc.RootElement;

            string? speedString = root.GetProperty("Speed").GetString();
            string? garageIdString = root.GetProperty("GarageId").GetString();

            if (!int.TryParse(garageIdString, out int garageId)
                || !int.TryParse(speedString, out int speed))
            {
                @ViewBag.Error = "Не число!";
            }
            else
            {
                var point = await _context.Points.FirstOrDefaultAsync(p => p.Index == garageId);

                if (point is null)
                {
                    @ViewBag.Error = "Точка не найдена!";
                }
                else
                {
                    _context.Vehicles.Add(new Vehicle(point, speed));
                    await _context.SaveChangesAsync();

                    return RedirectToAction("administrator", "Dashboard");
                }
            }

            return View();
        }


        public async Task<AdminMapModel> ViewMap()
        {
            return await ViewMap(DateTime.Now.Date);
        }

        public async Task<AdminMapModel> ViewMap(DateTime date)
        {
            //GenerateMap.SaveMap(_context);

            var points = await _context.Points.ToArrayAsync();

            var waitingOrders = _context
                .GetWaitingOrders(date)
                .Where(o => o.Status == ReadyOrderStatus.Accepted)
                .ToArray();

            var currentOrders = date == DateTime.Now.Date || date == default
                ? _waitingOrder.GetCurrentOrders().Values
                    .ToArray()
                : Array.Empty<ReadyOrder>();

            var vehicles = _vehicleService.Vehicles
                .ToArray();

            return new AdminMapModel(
                points,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                vehicles,
                currentOrders.Select(order => order.Vehicle.CurrentPoint).ToArray());
        }
    }
}
