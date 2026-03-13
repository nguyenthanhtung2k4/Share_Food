using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.ViewComponents;

public class FeaturedCategoriesViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _dbContext;

    public FeaturedCategoriesViewComponent(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 6)
    {
        var items = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryMenuItemViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                RecipeCount = category.Recipes.Count(recipe => recipe.IsVisible)
            })
            .Take(count)
            .ToListAsync();

        return View(items);
    }
}
