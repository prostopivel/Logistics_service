using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    [Route("[controller]")]
    public class ErrorController : Controller
    {
        [Route("{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            switch (statusCode)
            {
                case 401:
                    return View("Unauthorized");
                case 404:
                    return View("NotFound");
                case 500:
                    return View("Error");
                default:
                    return View("Error");
            }
        }

        [Route("Unauthorized")]
        public new IActionResult Unauthorized()
        {
            return View();
        }
    }
}