using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GeminiAspNetDemo.Models;
using GeminiAspNetDemo.Services;

namespace GeminiAspNetDemo.Controllers;

public class HomeController : Controller
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IGeminiService geminiService, ILogger<HomeController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Generate(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            return BadRequest("Prompt is required");
        }

        try
        {
            var response = await _geminiService.GenerateContentAsync(prompt);
            return Json(new { success = true, content = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content");
            return Json(new { success = false, error = "Error generating content" });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
