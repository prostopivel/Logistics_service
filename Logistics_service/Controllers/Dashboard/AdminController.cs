using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Models.Users;
using Logistics_service.Services;
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

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllCustomers")]
        public async Task<ActionResult> ViewAllCustomers()
        {
            return View(await _context.Customers.ToListAsync());
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllManagers")]
        public async Task<ActionResult> ViewAllManagers()
        {
            return View(new Tuple<List<Manager>, List<Administrator>>(
                await _context.Managers.ToListAsync(),
                await _context.Administrators.ToListAsync()));
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            await DeleteUserAsync(id);

            return View("viewAllCustomers", _context.Customers.ToListAsync());
        }

        [ResponseCache(NoStore = true, Duration = 0)]
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

                await Console.Out.WriteLineAsync("Delete: " + user.Email);
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
                    return View("addUser");
                }

                return RedirectToAction("administrator", "Dashboard");
            }

            return View("addUser");
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

                await Console.Out.WriteLineAsync("Add: " + user.Email);
                return true;
            }
            return false;
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllOrders")]
        public async Task<IActionResult> ViewAllOrders()
        {
            var managerOrders = await _context
                .GetOrders()
                .Where(o => o.Status == ReadyOrderStatus.Created)
                .ToArrayAsync();
            var waitingNotTodayOrders = await _context
                .GetOrders()
                .Where(o => o.Status == ReadyOrderStatus.Accepted && o.ArrivalTime.Date != DateTime.Today)
                .ToArrayAsync();
            var waitingOrders = (_waitingOrder.GetOrders())
                .Values.ToArray();
            var currentOrders = (_waitingOrder.GetCurrentOrders())
                .Values.ToArray();
            return View("viewAllOrders", new Tuple<ReadyOrder[], ReadyOrder[], ReadyOrder[], ReadyOrder[]>(
                managerOrders, waitingOrders, waitingNotTodayOrders, currentOrders));
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

                var temp = (ReadyOrder)order.Clone();

                if (temp.ArrivalTime.Date == DateTime.Now.Date)
                {
                    if (temp.VehicleId is null)
                    {
                        ViewBag.Error = "Id транспорта не указан!";
                        return await ViewAllOrders();
                    }

                    var vehicle = _vehicleService.GetVehicleById((int)temp.VehicleId);

                    if (vehicle is null)
                    {
                        vehicle = await _context.GetVehicleAsync((int)temp.VehicleId);
                        if (vehicle is null)
                        {
                            ViewBag.Error = "Неверный индекс транспорта!";
                            return await ViewAllOrders();
                        }
                    }

                    temp.Vehicle = vehicle;

                    _waitingOrder.AddOrder((DateTime)temp.Route.DepartureTime!, temp);
                }
            }

            return await ViewAllOrders();
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

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public async Task<IActionResult> ViewMap()
        {
            //GenerateMap.SaveMap(_context);

            var points = await _context.Points.ToArrayAsync();
            var waitingOrders = _waitingOrder.GetOrders()
                .Values.ToArray();
            var currentOrders = _waitingOrder.GetCurrentOrders()
                .Values.ToArray();
            var vehicles = _vehicleService.Vehicles
                .Select(v => new VehicleDto(v))
                .ToArray();
            return View(new Tuple<Point[], Models.Route[], Models.Route[], VehicleDto[], Point?[]>(
                points,
                waitingOrders.Select(order => order.Route).ToArray(),
                currentOrders.Select(order => order.Route).ToArray(),
                vehicles,
                currentOrders.Select(order => order.Vehicle.CurrentPoint).ToArray()));
        }
    }
}
