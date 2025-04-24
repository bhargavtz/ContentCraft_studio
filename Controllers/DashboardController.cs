using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContentCraft_studio.Models;
using ContentCraft_studio.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace ContentCraft_studio.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IMongoDbService _mongoDbService;

        public DashboardController(IMongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("SignIn", "Account");
                }

                var userModel = new UserModel
                {
                    Id = userId,
                    Name = User.Identity?.Name ?? string.Empty,
                    Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                    Nickname = User.FindFirstValue("nickname") ?? string.Empty,
                    LastLogin = DateTime.UtcNow
                };

                // Store or update user data in MongoDB
                await _mongoDbService.UpsertUserAsync(userModel);

                var viewModel = new DashboardViewModel
                {
                    User = userModel,
                    TotalUsage = await _mongoDbService.GetUserTotalUsageAsync(userId),
                    RecentActivities = await _mongoDbService.GetUserRecentActivitiesAsync(userId)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error and return to home page
                return RedirectToAction("Index", "Home");
            }
        }
    }
}