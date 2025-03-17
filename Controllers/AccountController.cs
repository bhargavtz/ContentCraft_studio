using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;

namespace GeminiAspNetDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration Configuration;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult SignIn(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, Auth0Constants.AuthenticationScheme);
        }

        public async Task Login(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [Authorize]
        public async Task Logout()
        {            
            try
            {
                // Clear the local authentication cookie
                await HttpContext.SignOutAsync("Cookies");

                // Build Auth0 logout properties
                var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                    .WithRedirectUri(Url.Action("Index", "Home"))
                    .Build();

                // Sign out from Auth0
                await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout process");
            }
        }

        public IActionResult SignUp()
        {
            return Redirect($"https://{Configuration["Auth0:Domain"]}/account/signup?client_id={Configuration["Auth0:ClientId"]}&redirect_uri={Configuration["Auth0:CallbackUrl"]}");
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }
    }
}