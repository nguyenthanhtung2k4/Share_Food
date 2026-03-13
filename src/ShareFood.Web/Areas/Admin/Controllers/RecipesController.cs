using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.Services;
using ShareFood.Web.ViewModels.Admin;

namespace ShareFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("quan-tri/cong-thuc")]
public class RecipesController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public RecipesController(ApplicationDbContext dbContext, IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.Recipes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(item => item.Title.Contains(keyword) || item.Author!.FullName.Contains(keyword));
        }

        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new AdminRecipeListItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                CategoryName = item.Category!.Name,
                AuthorName = item.Author!.FullName,
                IsVisible = item.IsVisible,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        ViewData["Keyword"] = keyword;
        return View(items);
    }

    [HttpPost("an-hien/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVisibility(int id, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes.FindAsync([id], cancellationToken);
        if (recipe is null)
        {
            TempData.SetError("Không tìm thấy", "Công thức cần cập nhật không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        recipe.IsVisible = !recipe.IsVisible;
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData.SetSuccess("Cập nhật thành công", recipe.IsVisible ? "Công thức đã được hiển thị lại." : "Công thức đã được ẩn khỏi danh sách công khai.");
        return RedirectToAction(nameof(Index));
    }

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

        _fileStorageService.DeleteFile(recipe.ThumbnailPath);
        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData.SetSuccess("Xóa thành công", "Công thức đã được xóa khỏi hệ thống.");
        return RedirectToAction(nameof(Index));
    }
}
