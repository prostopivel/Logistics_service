using Logistics_service.Data;
using Logistics_service.Models.Users;
using Logistics_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Logistics_service.Controllers.Dashboard
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AdminController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
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
        [HttpDelete("deleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await DeleteUserAsync(id);

            return View("viewAllCustomers", await _context.Customers.ToListAsync());
        }

        private async Task DeleteUserAsync(int id)
        {
            var user = _context.Users.FirstOrDefault(c => c.Id == id);
            if (user != null)
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
            if (_context.Users.FirstOrDefault(c => c.Email == user.Email) == null)
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
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("manageTransport")]
        public IActionResult ManageTransport()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public IActionResult ViewMap()
        {
            return View(GenerateMap.Points);
        }
    }
}
