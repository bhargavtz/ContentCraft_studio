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

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

        // Sign Up Action
        public IActionResult SignUp()
        {
            return Redirect($"https://{_configuration["Auth0:Domain"]}/account/signup?client_id={_configuration["Auth0:ClientId"]}&redirect_uri={_configuration["Auth0:CallbackUrl"]}");
        }

        // Profile Action
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userNickname = User.FindFirstValue("nickname");
            var picture = User.FindFirstValue("picture");
            var emailVerified = User.FindFirstValue("email_verified");
            var locale = User.FindFirstValue("locale");
            var updatedAt = User.FindFirstValue("updated_at");
            var authTime = User.FindFirstValue("auth_time");

            var user = new UserModel
            {
                Id = userId,
                Name = userName,
                Email = userEmail,
                Nickname = userNickname,
                Picture = picture,
                EmailVerified = emailVerified != null ? bool.Parse(emailVerified) : false,
                Locale = locale,
                UpdatedAt = updatedAt != null ? DateTime.Parse(updatedAt) : DateTime.UtcNow,
                AuthTime = authTime != null ? Convert.ToInt64(authTime) : 0
            };

            return View(user);
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