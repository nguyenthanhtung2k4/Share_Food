using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.Services;
using ShareFood.Web.ViewModels.Common;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
[Route("cong-thuc")]
public class RecipesController : Controller
{
    private const int PageSize = 9;

    private readonly ApplicationDbContext _dbContext;
    private readonly IRecipeQueryService _recipeQueryService;
    private readonly IFileStorageService _fileStorageService;

    public RecipesController(
        ApplicationDbContext dbContext,
        IRecipeQueryService recipeQueryService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _recipeQueryService = recipeQueryService;
        _fileStorageService = fileStorageService;
    }

    [AllowAnonymous]
    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, int? categoryId, int page = 1, CancellationToken cancellationToken = default)
    {
        var categories = await GetCategoryOptionsAsync(categoryId, cancellationToken);
        var recipes = await _recipeQueryService.SearchVisibleRecipesAsync(keyword, categoryId, Math.Max(page, 1), PageSize, cancellationToken);

        return View(new RecipeListPageViewModel
        {
            Keyword = keyword,
            CategoryId = categoryId,
            Categories = categories,
            Recipes = recipes
        });
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/{slug?}")]
    public async Task<IActionResult> Details(int id, string? slug, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(item => item.Category)
            .Include(item => item.Author)
            .Include(item => item.Ingredients)
                .ThenInclude(item => item.Ingredient)
            .Include(item => item.Ingredients)
                .ThenInclude(item => item.Unit)
            .Include(item => item.Steps)
            .Include(item => item.Comments)
                .ThenInclude(item => item.User)
            .Include(item => item.Favorites)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        var currentUserId = GetCurrentUserId();
        var canViewHiddenRecipe = recipe.AuthorId == currentUserId || User.IsInRole(RoleNames.Admin);
        if (!recipe.IsVisible && !canViewHiddenRecipe)
        {
            return NotFound();
        }

        if (!string.Equals(recipe.Slug, slug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Details), new { id = recipe.Id, slug = recipe.Slug });
        }

        var detail = new RecipeDetailViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Summary = recipe.Summary,
            ThumbnailPath = recipe.ThumbnailPath,
            Servings = recipe.Servings,
            CookTimeMinutes = recipe.CookTimeMinutes,
            CategoryName = recipe.Category?.Name ?? string.Empty,
            AuthorId = recipe.AuthorId,
            AuthorName = recipe.Author?.FullName ?? string.Empty,
            IsVisible = recipe.IsVisible,
            IsOwner = recipe.AuthorId == currentUserId,
            IsFavorited = !string.IsNullOrWhiteSpace(currentUserId) && recipe.Favorites.Any(item => item.UserId == currentUserId),
            FavoriteCount = recipe.Favorites.Count,
            Ingredients = recipe.Ingredients
                .OrderBy(item => item.Id)
                .Select(item => new RecipeIngredientDetailViewModel
                {
                    IngredientName = item.Ingredient?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitName = item.Unit?.Name ?? string.Empty,
                    UnitSymbol = item.Unit?.Symbol ?? string.Empty,
                    Note = item.Note
                })
                .ToList(),
            Steps = recipe.Steps
                .OrderBy(item => item.StepNumber)
                .Select(item => new RecipeStepDetailViewModel
                {
                    StepNumber = item.StepNumber,
                    Instruction = item.Instruction
                })
                .ToList(),
            Comments = recipe.Comments
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => new RecipeCommentViewModel
                {
                    UserName = item.User?.FullName ?? "Thành viên",
                    Content = item.Content,
                    CreatedAt = item.CreatedAt
                })
                .ToList()
        };

        return View(detail);
    }

    [Authorize]
    [HttpGet("dang-moi")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await BuildRecipeFormAsync(new RecipeCreateEditViewModel(), cancellationToken);
        return View(model);
    }

    [Authorize]
    [HttpPost("dang-moi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecipeCreateEditViewModel model, CancellationToken cancellationToken)
    {
        NormalizeRecipeForm(model);
        ValidateRecipeForm(model);

        if (!ModelState.IsValid)
        {
            await PopulateRecipeSelectionsAsync(model, cancellationToken);
            return View(model);
        }

        string? imagePath;
        try
        {
            imagePath = await _fileStorageService.SaveRecipeImageAsync(model.Thumbnail, cancellationToken: cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Thumbnail), exception.Message);
            await PopulateRecipeSelectionsAsync(model, cancellationToken);
            return View(model);
        }

        var recipe = new Recipe
        {
            Title = model.Title.Trim(),
            Slug = await GenerateUniqueRecipeSlugAsync(model.Title, cancellationToken: cancellationToken),
            Summary = model.Summary.Trim(),
            ThumbnailPath = imagePath,
            CategoryId = model.CategoryId,
            Servings = model.Servings,
            CookTimeMinutes = model.CookTimeMinutes,
            AuthorId = GetCurrentUserId()!,
            IsVisible = true
        };

        foreach (var ingredient in model.Ingredients)
        {
            recipe.Ingredients.Add(new RecipeIngredient
            {
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                UnitId = ingredient.UnitId,
                Note = ingredient.Note?.Trim()
            });
        }

        foreach (var step in model.Steps)
        {
            recipe.Steps.Add(new RecipeStep
            {
                StepNumber = step.StepNumber,
                Instruction = step.Instruction.Trim()
            });
        }

        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData.SetSuccess("Đăng công thức thành công", "Công thức của bạn đã được chia sẻ lên hệ thống.");
        return RedirectToAction(nameof(Details), new { id = recipe.Id, slug = recipe.Slug });
    }

    [Authorize]
    [HttpGet("chinh-sua/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .Include(item => item.Ingredients)
            .Include(item => item.Steps)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        if (!CanManageRecipe(recipe))
        {
            return Forbid();
        }

        var model = new RecipeCreateEditViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Summary = recipe.Summary,
            ExistingThumbnailPath = recipe.ThumbnailPath,
            CategoryId = recipe.CategoryId,
            Servings = recipe.Servings,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Ingredients = recipe.Ingredients
                .OrderBy(item => item.Id)
                .Select(item => new RecipeIngredientInputViewModel
                {
                    IngredientId = item.IngredientId,
                    Quantity = item.Quantity,
                    UnitId = item.UnitId,
                    Note = item.Note
                })
                .ToList(),
            Steps = recipe.Steps
                .OrderBy(item => item.StepNumber)
                .Select(item => new RecipeStepInputViewModel
                {
                    StepNumber = item.StepNumber,
                    Instruction = item.Instruction
                })
                .ToList()
        };

        await PopulateRecipeSelectionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize]
    [HttpPost("chinh-sua/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RecipeCreateEditViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        NormalizeRecipeForm(model);
        ValidateRecipeForm(model);

        var recipe = await _dbContext.Recipes
            .Include(item => item.Ingredients)
            .Include(item => item.Steps)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        if (!CanManageRecipe(recipe))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.ExistingThumbnailPath = recipe.ThumbnailPath;
            await PopulateRecipeSelectionsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            recipe.ThumbnailPath = await _fileStorageService.SaveRecipeImageAsync(model.Thumbnail, recipe.ThumbnailPath, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Thumbnail), exception.Message);
            model.ExistingThumbnailPath = recipe.ThumbnailPath;
            await PopulateRecipeSelectionsAsync(model, cancellationToken);
            return View(model);
        }

        recipe.Title = model.Title.Trim();
        recipe.Slug = await GenerateUniqueRecipeSlugAsync(model.Title, recipe.Id, cancellationToken);
        recipe.Summary = model.Summary.Trim();
        recipe.CategoryId = model.CategoryId;
        recipe.Servings = model.Servings;
        recipe.CookTimeMinutes = model.CookTimeMinutes;

        _dbContext.RecipeIngredients.RemoveRange(recipe.Ingredients);
        _dbContext.RecipeSteps.RemoveRange(recipe.Steps);

        recipe.Ingredients = model.Ingredients.Select(item => new RecipeIngredient
        {
            IngredientId = item.IngredientId,
            Quantity = item.Quantity,
            UnitId = item.UnitId,
            Note = item.Note?.Trim()
        }).ToList();

        recipe.Steps = model.Steps.Select(item => new RecipeStep
        {
            StepNumber = item.StepNumber,
            Instruction = item.Instruction.Trim()
        }).ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Cập nhật thành công", "Công thức đã được cập nhật.");
        return RedirectToAction(nameof(Details), new { id = recipe.Id, slug = recipe.Slug });
    }

    [Authorize]
    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (recipe is null)
        {
            TempData.SetError("Không tìm thấy", "Công thức cần xóa không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        if (!CanManageRecipe(recipe))
        {
            return Forbid();
        }

        _fileStorageService.DeleteFile(recipe.ThumbnailPath);
        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData.SetSuccess("Xóa thành công", "Công thức đã được xóa.");
        return RedirectToAction("Index", "Profile");
    }

    [Authorize]
    [HttpPost("{id:int}/binh-luan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, [Bind(Prefix = "NewComment")] CommentInputViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData.SetError("Bình luận chưa hợp lệ", "Vui lòng nhập nội dung bình luận hợp lệ.");
            return RedirectToAction(nameof(Details), new { id });
        }

        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.IsVisible, cancellationToken);

        if (recipe is null)
        {
            return NotFound();
        }

        _dbContext.Comments.Add(new Comment
        {
            RecipeId = id,
            UserId = GetCurrentUserId()!,
            Content = model.Content.Trim()
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Bình luận thành công", "Cảm ơn bạn đã chia sẻ cảm nhận về món ăn này.");
        return RedirectToAction(nameof(Details), new { id, slug = recipe.Slug });
    }

    private bool CanManageRecipe(Recipe recipe)
    {
        var currentUserId = GetCurrentUserId();
        return recipe.AuthorId == currentUserId || User.IsInRole(RoleNames.Admin);
    }

    private async Task<RecipeCreateEditViewModel> BuildRecipeFormAsync(RecipeCreateEditViewModel model, CancellationToken cancellationToken)
    {
        if (model.Ingredients.Count == 0)
        {
            model.Ingredients.Add(new RecipeIngredientInputViewModel());
        }

        if (model.Steps.Count == 0)
        {
            model.Steps.Add(new RecipeStepInputViewModel { StepNumber = 1 });
        }

        await PopulateRecipeSelectionsAsync(model, cancellationToken);
        return model;
    }

    private async Task PopulateRecipeSelectionsAsync(RecipeCreateEditViewModel model, CancellationToken cancellationToken)
    {
        model.Categories = await GetCategoryOptionsAsync(model.CategoryId, cancellationToken);
        model.AvailableIngredients = await _dbContext.Ingredients
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new SelectListItem(item.Name, item.Id.ToString()))
            .ToListAsync(cancellationToken);

        model.Units = await _dbContext.Units
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new SelectListItem($"{item.Name} ({item.Symbol})", item.Id.ToString()))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SelectListItem>> GetCategoryOptionsAsync(int? selectedValue, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new SelectListItem(item.Name, item.Id.ToString(), selectedValue == item.Id))
            .ToListAsync(cancellationToken);
    }

    private void NormalizeRecipeForm(RecipeCreateEditViewModel model)
    {
        model.Ingredients = model.Ingredients
            .Where(item => item.IngredientId > 0 && item.UnitId > 0 && item.Quantity > 0)
            .Select(item => new RecipeIngredientInputViewModel
            {
                IngredientId = item.IngredientId,
                Quantity = item.Quantity,
                UnitId = item.UnitId,
                Note = item.Note?.Trim()
            })
            .ToList();

        model.Steps = model.Steps
            .Where(item => !string.IsNullOrWhiteSpace(item.Instruction))
            .Select((item, index) => new RecipeStepInputViewModel
            {
                StepNumber = index + 1,
                Instruction = item.Instruction.Trim()
            })
            .ToList();
    }

    private void ValidateRecipeForm(RecipeCreateEditViewModel model)
    {
        if (model.Ingredients.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng thêm ít nhất một nguyên liệu.");
        }

        if (model.Steps.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng thêm ít nhất một bước nấu.");
        }

        var hasDuplicateIngredient = model.Ingredients
            .GroupBy(item => item.IngredientId)
            .Any(group => group.Count() > 1);

        if (hasDuplicateIngredient)
        {
            ModelState.AddModelError(string.Empty, "Mỗi nguyên liệu chỉ nên xuất hiện một lần trong một công thức.");
        }
    }

    private async Task<string> GenerateUniqueRecipeSlugAsync(string title, int? currentRecipeId = null, CancellationToken cancellationToken = default)
    {
        var baseSlug = SlugHelper.Generate(title);
        var slug = baseSlug;
        var index = 1;

        while (await _dbContext.Recipes.AnyAsync(
                   item => item.Slug == slug && (!currentRecipeId.HasValue || item.Id != currentRecipeId.Value),
                   cancellationToken))
        {
            slug = $"{baseSlug}-{index++}";
        }

        return slug;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
