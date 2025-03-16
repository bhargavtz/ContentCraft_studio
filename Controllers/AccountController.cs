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
        public async Task Login(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [Authorize]
        public async Task<IActionResult> Logout(bool switchAccount = false)
        {
            try
            {
                var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                    .WithRedirectUri($"{Request.Scheme}://{Request.Host}/Account/Login")
                    .Build();

                await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
                
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                HttpContext.Session.Clear();
                
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                Response.Headers.Remove("Authorization");

                if (switchAccount)
                {
                    TempData["Message"] = "Please sign in with a different account.";
                    var auth0Settings = HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Settings>>().Value;
                return Redirect($"https://{auth0Settings.Domain}/v2/logout?client_id={auth0Settings.ClientId}&returnTo={Request.Scheme}://{Request.Host}/Account/Login");
                }

                TempData["Message"] = "Successfully logged out. Please sign in with your email address to continue.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred during logout. Please try clearing your browser cache and try again.";
                return RedirectToAction("Login");
            }
        }

        public IActionResult SignIn()
        {
            return RedirectToAction(nameof(Login));
        }

        public IActionResult SignUp()
        {
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public IActionResult Profile()
        {
            var user = HttpContext.User;
            var profile = new
            {
                Name = user.Identity.Name,
                Email = user.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                Picture = user.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            };
            return View(profile);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }
    }
}
