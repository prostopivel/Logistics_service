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
        private readonly OrderQueueService<ReadyOrder> _readyQueueService;

        public AdminController(IConfiguration configuration, ApplicationDbContext context, 
            WaitingOrderService waitingOrder, VehicleService vehicleService, 
            OrderQueueService<ReadyOrder> readyQueueService)
        {
            _configuration = configuration;
            _context = context;
            _waitingOrder = waitingOrder;
            _vehicleService = vehicleService;
            _readyQueueService = readyQueueService;
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
            return View(await _context.Managers.ToListAsync());
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            await DeleteUserAsync(id);

            return View("viewAllCustomers", await _context.Customers.ToListAsync());
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpDelete("deleteManager/{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            await DeleteUserAsync(id);

            return View("viewAllManagers", await _context.Managers.ToListAsync());
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

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewAllOrders")]
        public IActionResult ViewAllOrders()
        {
            return View(_readyQueueService.PeekAll());
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpPost("addOrder")]
        public async Task<ActionResult> AddOrder([FromBody] ReadyOrder order)
        {
            if (ModelState.IsValid && order.Route.DepartureTime is not null)
            {
                await _waitingOrder.AddOrder((DateTime)order.Route.DepartureTime, order, _context);
                return View("viewAllOrders");
            }

            return View("addOrder");
        }

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

            return View(await _context.Points.ToArrayAsync());
        }
    }
}
