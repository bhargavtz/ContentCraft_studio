using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;

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
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
    }
}
