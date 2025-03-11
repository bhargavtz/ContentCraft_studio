using Microsoft.AspNetCore.Mvc;

namespace ContentCraft_studio.Controllers
{
    [Route("[controller]")]
    public class PricingController : Controller
    {
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("ProcessPayment")]
        public IActionResult ProcessPayment([FromForm] string plan, [FromForm] string cardNumber, [FromForm] string expiry, [FromForm] string cvc)
        {
            if (string.IsNullOrEmpty(plan))
            {
                return BadRequest("Plan is required");
            }

            // Store plan info in TempData
            TempData["Plan"] = plan;
            
            // TODO: Add real payment processing logic here
            
            return RedirectToAction("Success");
        }

        [HttpGet]
        [Route("Success")]
        public IActionResult Success()
        {
            var plan = TempData["Plan"] as string;
            if (string.IsNullOrEmpty(plan))
            {
                return RedirectToAction("Index");
            }
            
            ViewData["Plan"] = plan;
            return View();
        }
    }
}
