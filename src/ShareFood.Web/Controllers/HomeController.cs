using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShareFood.Web.Models;

namespace ShareFood.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Home", new { area = "Client" });
    }

    [HttpGet("/truy-cap-bi-tu-choi")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
