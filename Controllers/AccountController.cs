using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ContentCraft_studio.Models;
using ContentCraft_studio.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace GeminiAspNetDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMongoDbService _mongoDbService;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration, IMongoDbService mongoDbService)
        {
            _logger = logger;
            _configuration = configuration;
            _mongoDbService = mongoDbService;
        }

        // Sign In Action
        public IActionResult SignIn(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, Auth0Constants.AuthenticationScheme);
        }

        // Login Action
        public async Task Login(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        // Logout Action
        [Authorize]
        public async Task Logout()
        {
            try
            {
                // Clear the local authentication cookie
                await HttpContext.SignOutAsync("Cookies");

                // Build Auth0 logout properties
                var redirectUri = Url.Action("Index", "Home");
                var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                    .WithRedirectUri(redirectUri ?? "/")
                    .Build();

                // Sign out from Auth0
                await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout process");
            }
        }

        // Sign Up Action
        public IActionResult SignUp()
        {
            return Redirect($"https://{_configuration["Auth0:Domain"]}/account/signup?client_id={_configuration["Auth0:ClientId"]}&redirect_uri={_configuration["Auth0:CallbackUrl"]}");
        }

        // Profile Action
        [Authorize]
        public IActionResult Profile()
        {
            var userClaims = User.Claims;
            
            var viewModel = new UserProfileViewModel
            {
                Username = User.Identity?.Name,
                Email = userClaims.FirstOrDefault(c => c.Type == "email")?.Value,
                AvatarUrl = userClaims.FirstOrDefault(c => c.Type == "picture")?.Value,
                Bio = "Welcome to ContentCraft Studio!", // You can modify this default value
                JoinDate = DateTime.UtcNow // You might want to store this in your user data
            };

            return View(viewModel);
        }

        // Update Profile Action
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UserModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = new UserModel
                {
                    Id = userId,
                    Nickname = model.Nickname,
                    Email = model.Email
                };

                await Task.Yield();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return Json(new { success = false, message = "Failed to update profile" });
            }
        }

    }
}
