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

        [HttpPost]
        [Route("ProcessPayment")]
        public IActionResult ProcessPayment([FromForm] string plan, [FromForm] string cardNumber, [FromForm] string expiry, [FromForm] string cvc)
        {
            if (string.IsNullOrEmpty(plan))
            {
                return BadRequest("Plan is required");
            }

            // TODO: Add real payment processing logic here
            // Stripe payment integration (demo purpose, replace with real tokenization in production)
            try
            {
                // Here you would use Stripe API to create a payment intent and confirm payment
                // For demo, we assume payment is successful
                // You can add Stripe logic here if you have API keys
            }
            catch (Exception ex)
            {
                return BadRequest($"Payment failed: {ex.Message}");
            }

            TempData["Plan"] = plan;
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
