using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContentCraft_studio.Models;
using ContentCraft_studio.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ContentCraft_studio.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IMongoDbService mongoDbService, ILogger<DashboardController> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
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


                // Get all collections for the dashboard
                var viewModel = new DashboardViewModel
                {
                    User = userModel,
                    BlogPosts = await _mongoDbService.GetAllBlogPostsAsync(),
                    Stories = await _mongoDbService.GetAllStoriesAsync(),
                    InstagramCaptions = await _mongoDbService.GetAllCaptionsAsync(),
                    ImageDescriptions = await _mongoDbService.GetAllImageDescriptionsAsync(),
                    BusinessNames = await _mongoDbService.GetAllBusinessNamesAsync(),
                    UserActivities = await _mongoDbService.GetUserActivitiesAsync(userId),
                    RecentActivities = await _mongoDbService.GetRecentActivitiesAsync(userId)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DashboardController.Index");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteActivity(string id)
        {
            await _mongoDbService.DeleteUserActivityAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateCaption([FromBody] UpdateCaptionRequest request)
        {
            try
            {
                await _mongoDbService.UpdateCaptionAsync(request.Id, request.Text);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteCaption([FromBody] string id)
        {
            try
            {
                await _mongoDbService.DeleteCaptionAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStory([FromBody] UpdateStoryRequest request)
        {
            try
            {
                await _mongoDbService.UpdateStoryAsync(request.Id, request.Content);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteStory([FromBody] string id)
        {
            try
            {
                await _mongoDbService.DeleteStoryAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
    
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateBlogPost([FromBody] UpdateBlogPostRequest request)
        {
            try
            {
                await _mongoDbService.UpdateBlogPostAsync(request.Id, request.Title, request.Content);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteBlogPost([FromBody] string id)
        {
            try
            {
                await _mongoDbService.DeleteBlogPostAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteImageDescription([FromBody] string id)
        {
            try
            {
                await _mongoDbService.DeleteImageDescriptionAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteBusinessName([FromBody] string id)
        {
            try
            {
                await _mongoDbService.DeleteBusinessNameAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditUserData([FromBody] UserModel updatedUserData)
        {
            try
            {
                // Update user data in all relevant collections
                await _mongoDbService.UpdateUserAsync(updatedUserData);
                await _mongoDbService.UpdateUserActivitiesAsync(updatedUserData.Id, updatedUserData);
                await _mongoDbService.UpdateUserStoriesAsync(updatedUserData.Id, updatedUserData);
                await _mongoDbService.UpdateUserBlogPostsAsync(updatedUserData.Id, updatedUserData);
                await _mongoDbService.UpdateUserImageDescriptionsAsync(updatedUserData.Id, updatedUserData);
                await _mongoDbService.UpdateUserBusinessNamesAsync(updatedUserData.Id, updatedUserData);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateImageDescription([FromBody] UpdateImageDescriptionRequest request)
        {
            try
            {
                await _mongoDbService.UpdateImageDescriptionAsync(request.Id, request.Description);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("/Dashboard/BlogPosts")]
        public async Task<IActionResult> GetBlogPosts()
        {
            var blogPosts = await _mongoDbService.GetAllBlogPostsAsync();
            return Ok(blogPosts);
        }
    }
}
