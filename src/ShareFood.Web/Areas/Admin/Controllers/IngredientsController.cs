using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.ViewModels.Admin;

namespace ShareFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("quan-tri/nguyen-lieu")]
public class IngredientsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public IngredientsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.Ingredients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(ingredient => ingredient.Name.Contains(keyword));
        }

        var items = await query
            .OrderBy(ingredient => ingredient.Name)
            .ToListAsync(cancellationToken);

        ViewData["Keyword"] = keyword;
        return View(items);
    }

    [HttpGet("them-moi")]
    public IActionResult Create()
    {
        return View(new IngredientFormViewModel());
    }

    [HttpPost("them-moi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IngredientFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _dbContext.Ingredients.AnyAsync(ingredient => ingredient.Name == model.Name, cancellationToken);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Nguyên liệu này đã tồn tại.");
            return View(model);
        }

        _dbContext.Ingredients.Add(new Ingredient
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim()
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Thêm thành công", "Nguyên liệu mới đã được tạo.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("chinh-sua/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync([id], cancellationToken);
        if (ingredient is null)
        {
            return NotFound();
        }

        return View(new IngredientFormViewModel
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Description = ingredient.Description
        });
    }

    [HttpPost("chinh-sua/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IngredientFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ingredient = await _dbContext.Ingredients.FindAsync([id], cancellationToken);
        if (ingredient is null)
        {
            return NotFound();
        }

        var exists = await _dbContext.Ingredients.AnyAsync(item => item.Id != id && item.Name == model.Name, cancellationToken);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Nguyên liệu này đã tồn tại.");
            return View(model);
        }

        ingredient.Name = model.Name.Trim();
        ingredient.Description = model.Description?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Cập nhật thành công", "Nguyên liệu đã được cập nhật.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ingredient = await _dbContext.Ingredients
            .Include(item => item.RecipeIngredients)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (ingredient is null)
        {
            TempData.SetError("Không tìm thấy", "Nguyên liệu cần xóa không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        if (ingredient.RecipeIngredients.Count > 0)
        {
            TempData.SetError("Không thể xóa", "Nguyên liệu đang được sử dụng trong công thức.");
            return RedirectToAction(nameof(Index));
        }

        _dbContext.Ingredients.Remove(ingredient);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Xóa thành công", "Nguyên liệu đã được xóa.");
        return RedirectToAction(nameof(Index));
    }
}
