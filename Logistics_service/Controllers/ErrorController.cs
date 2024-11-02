using Microsoft.AspNetCore.Mvc;

namespace Logistics_service.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
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
    }
}