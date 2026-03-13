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
[Route("quan-tri/danh-muc")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public CategoriesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(category => category.Name.Contains(keyword));
        }

        var items = await query
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

        ViewData["Keyword"] = keyword;
        return View(items);
    }

    [HttpGet("them-moi")]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost("them-moi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var slug = SlugHelper.Generate(model.Name);
        var exists = await _dbContext.Categories.AnyAsync(category => category.Name == model.Name || category.Slug == slug, cancellationToken);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Danh mục này đã tồn tại.");
            return View(model);
        }

        _dbContext.Categories.Add(new Category
        {
            Name = model.Name.Trim(),
            Slug = slug,
            Description = model.Description?.Trim()
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Thêm thành công", "Danh mục mới đã được tạo.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("chinh-sua/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        });
    }

    [HttpPost("chinh-sua/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = await _dbContext.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        var slug = SlugHelper.Generate(model.Name);
        var exists = await _dbContext.Categories
            .AnyAsync(item => item.Id != id && (item.Name == model.Name || item.Slug == slug), cancellationToken);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Danh mục này đã tồn tại.");
            return View(model);
        }

        category.Name = model.Name.Trim();
        category.Description = model.Description?.Trim();
        category.Slug = slug;

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Cập nhật thành công", "Danh mục đã được cập nhật.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .Include(item => item.Recipes)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (category is null)
        {
            TempData.SetError("Không tìm thấy", "Danh mục cần xóa không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        if (category.Recipes.Count > 0)
        {
            TempData.SetError("Không thể xóa", "Danh mục đang được sử dụng trong công thức.");
            return RedirectToAction(nameof(Index));
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Xóa thành công", "Danh mục đã được xóa.");
        return RedirectToAction(nameof(Index));
    }
}
