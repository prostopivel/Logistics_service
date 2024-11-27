using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private readonly IConfiguration _configuration;

        public CustomerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewOrders")]
        public IActionResult ViewOrders()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("createRequest")]
        public IActionResult CreateRequest()
        {
            return View();
        }

        [ServiceFilter(typeof(DigestAuthFilter))]
        [HttpGet("viewMap")]
        public IActionResult ViewMap()
        {
            return View();
        }

        [HttpGet("customer")]
        public IActionResult Customer(string returnUrl)
        {
            string realm = _configuration["Realm"];
            string qop = _configuration["Qop"];

            var opaque = HttpContext.Session.GetString("Opaque");
            if (opaque == null)
                return View("Unauthorized");

            string nonce = GenerateDigest.GenerateRandom();
            HttpContext.Session.SetString(opaque, nonce);

            ViewBag.WWWAuthenticateHeader = $"Digest realm=\"{realm}\", qop=\"{qop}\", nonce=\"{nonce}\", opaque=\"{opaque}\", returnUrl=\"{returnUrl}\", role = \"Customer\"";

            return View("Unauthorized");
        }
    }
}
