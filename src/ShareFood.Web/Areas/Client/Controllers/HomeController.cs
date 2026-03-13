using Microsoft.AspNetCore.Mvc;
using ShareFood.Web.Services;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
public class HomeController : Controller
{
    private readonly IRecipeQueryService _recipeQueryService;

    public HomeController(IRecipeQueryService recipeQueryService)
    {
        _recipeQueryService = recipeQueryService;
    }

    [HttpGet("/trang-chu")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = new HomePageViewModel
        {
            FeaturedRecipes = await _recipeQueryService.GetFeaturedRecipesAsync(6, cancellationToken),
            LatestRecipes = await _recipeQueryService.GetLatestRecipesAsync(8, cancellationToken)
        };

        return View(viewModel);
    }
}
