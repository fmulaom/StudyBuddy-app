using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Authorize]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "StudyTasks");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogError("Error view returned. RequestId {RequestId}", requestId);

        return View(new ErrorViewModel { RequestId = requestId });
    }
}
