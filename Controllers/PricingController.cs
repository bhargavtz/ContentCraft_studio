using Microsoft.AspNetCore.Mvc;
using ContentCraft_studio.Models;
using ContentCraft_studio.Services;

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
            var plans = new[]
            {
                new {
                    Name = "Free",
                    Price = 0,
                    Description = "Perfect for getting started",
                    Features = new[] { "5 AI generations/month", "Basic templates" },
                    Popular = false
                },
                new {
                    Name = "Pro",
                    Price = 2499,
                    Description = "Best for professionals",
                    Features = new[] { "Unlimited AI generations", "Premium templates", "Priority support" },
                    Popular = true
                },
                new {
                    Name = "Enterprise",
                    Price = 7999,
                    Description = "For large teams",
                    Features = new[] { "Everything in Pro", "Custom integrations", "24/7 support" },
                    Popular = false
                }
            };
            var testimonials = new[]
            {
                new { Name = "Amit S.", Text = "ContentCraft Studio transformed my workflow! Highly recommended." },
                new { Name = "Priya K.", Text = "The AI tools are top-notch and easy to use." },
                new { Name = "Rahul D.", Text = "Our team productivity doubled after switching to the Pro plan." }
            };
            ViewData["Plans"] = plans;
            ViewData["Testimonials"] = testimonials;
            return View();
        }

        private readonly IMongoDbService _mongoDbService;

        public PricingController(IMongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        [HttpPost]
        [Route("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment([FromForm] PaymentModel payment)
        {
            if (string.IsNullOrEmpty(payment.PlanName))
            {
                return BadRequest("Plan is required");
            }

            try
            {
                // Set payment details
                payment.UserId = User.Identity.Name; // Get current user's ID
                payment.PaymentDate = DateTime.UtcNow;
                payment.Status = "success";
                payment.TransactionId = Guid.NewGuid().ToString();

                // Save payment to database
                await _mongoDbService.SavePaymentAsync(payment);

                TempData["Plan"] = payment.PlanName;
                TempData["TransactionId"] = payment.TransactionId;
                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                return BadRequest($"Payment failed: {ex.Message}");
            }
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
