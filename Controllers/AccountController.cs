using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using GeminiAspNetDemo.Models;

namespace ContentCraft_studio.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOptions<Auth0Settings> _auth0Settings;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IOptions<Auth0Settings> auth0Settings, ILogger<AccountController> logger)
        {
            _auth0Settings = auth0Settings;
            _logger = logger;
        }

        public async Task<IActionResult> Login(string returnUrl = "/", string message = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrEmpty(message))
            {
                TempData["Message"] = message;
            }

            try
            {
                var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                    .WithRedirectUri(returnUrl)
                    .WithParameter("prompt", "select_account")
                    .WithParameter("response_mode", "form_post")
                    .Build();

                await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Login error: {ex.Message}";
                _logger.LogError(errorMessage);
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = "An error occurred during login. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public IActionResult Profile()
        {
            var user = HttpContext.User;
            var profile = new
            {
                Name = user.Identity?.Name,
                Email = user.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                Picture = user.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            };
            return View(profile);
        }

        public async Task<IActionResult> SignIn()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return await Login("/", "Please sign in with your email address.");
        }

        public async Task<IActionResult> SignUp()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var auth0Settings = _auth0Settings.Value;
            var callbackUrl = auth0Settings.CallbackUrl ?? $"{Request.Scheme}://{Request.Host}/callback";
            var state = Guid.NewGuid().ToString();
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(callbackUrl)
                .WithParameter("screen_hint", "signup")
                .WithParameter("state", state)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            return new EmptyResult();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout(bool switchAccount = false)
        {
            try
            {
                var auth0Settings = HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Settings>>().Value;
                var returnUrl = Url.Action("Index", "Home", null, Request.Scheme) ?? "/";
               
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme);
                
                HttpContext.Session.Clear();
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                var logoutUrl = $"https://{auth0Settings.Domain}/v2/logout?client_id={auth0Settings.ClientId}&returnTo={Uri.EscapeDataString(returnUrl)}&state={Guid.NewGuid()}";
                return Redirect(logoutUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}