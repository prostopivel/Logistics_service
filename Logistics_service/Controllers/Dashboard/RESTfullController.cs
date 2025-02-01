using AutoMapper;
using Logistics_service.Models;
using Logistics_service.Models.Users;
using Logistics_service.Services;
using Logistics_service.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics_service.Controllers.Dashboard
{
    [Route("admin")]
    public class RESTfullController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RESTfullController(IConfiguration configuration, ApplicationDbContext context, IMapper mapper)
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator)]
        [HttpGet("viewAllCustomers")]
        public async Task<ActionResult> ViewAllCustomers()
        {
            ViewData["Title"] = "viewAllCustomers";
            ViewBag.Error = TempData["Error"] as string;

            var customers = await _context.Customers.AsNoTracking().ToArrayAsync();

            return View("viewAllCustomers", _mapper.Map<UserOutputModel[]>(customers));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator)]
        [HttpGet("viewAllManagers")]
        public async Task<ActionResult> ViewAllManagers()
        {
            ViewData["Title"] = "viewAllManagers";
            ViewBag.Error = TempData["Error"] as string;

            var managers = await _context.Managers.AsNoTracking().ToArrayAsync();
            var administrators = await _context.Administrators.AsNoTracking().ToArrayAsync();

            return View("viewAllManagers", new Tuple<UserOutputModel[], UserOutputModel[]>(
                _mapper.Map<UserOutputModel[]>(managers),
                _mapper.Map<UserOutputModel[]>(administrators)));
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator)]
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
        [AuthorizeRole(UserRole.Administrator)]
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
        [AuthorizeRole(UserRole.Administrator)]
        [HttpGet("addUser")]
        public IActionResult AddUser()
        {
            ViewData["Title"] = "addUser";
            ViewBag.Error = TempData["Error"] as string;
            ViewBag.RealmHeader = _configuration["Realm"];

            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [AuthorizeRole(UserRole.Administrator)]
        [HttpPost("addUser")]
        public async Task<ActionResult> AddUser([FromBody] UserInputModel user)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("addUser");
            }

            if (!await AddUserAsync(_mapper.Map<User>(user)))
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
    }
}
