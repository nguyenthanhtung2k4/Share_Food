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
[Route("quan-tri/don-vi-do")]
public class UnitsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public UnitsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.Units.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(unit => unit.Name.Contains(keyword) || unit.Symbol.Contains(keyword));
        }

        var items = await query
            .OrderBy(unit => unit.Name)
            .ToListAsync(cancellationToken);

        ViewData["Keyword"] = keyword;
        return View(items);
    }

    [HttpGet("them-moi")]
    public IActionResult Create()
    {
        return View(new UnitFormViewModel());
    }

    [HttpPost("them-moi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _dbContext.Units.AnyAsync(unit => unit.Name == model.Name || unit.Symbol == model.Symbol, cancellationToken);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Đơn vị đo này đã tồn tại.");
            return View(model);
        }

        _dbContext.Units.Add(new Unit
        {
            Name = model.Name.Trim(),
            Symbol = model.Symbol.Trim()
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Thêm thành công", "Đơn vị đo mới đã được tạo.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("chinh-sua/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var unit = await _dbContext.Units.FindAsync([id], cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        return View(new UnitFormViewModel
        {
            Id = unit.Id,
            Name = unit.Name,
            Symbol = unit.Symbol
        });
    }

    [HttpPost("chinh-sua/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UnitFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var unit = await _dbContext.Units.FindAsync([id], cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        var exists = await _dbContext.Units.AnyAsync(item => item.Id != id && (item.Name == model.Name || item.Symbol == model.Symbol), cancellationToken);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Đơn vị đo này đã tồn tại.");
            return View(model);
        }

        unit.Name = model.Name.Trim();
        unit.Symbol = model.Symbol.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Cập nhật thành công", "Đơn vị đo đã được cập nhật.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var unit = await _dbContext.Units
            .Include(item => item.RecipeIngredients)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (unit is null)
        {
            TempData.SetError("Không tìm thấy", "Đơn vị đo cần xóa không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        if (unit.RecipeIngredients.Count > 0)
        {
            TempData.SetError("Không thể xóa", "Đơn vị đo đang được sử dụng trong công thức.");
            return RedirectToAction(nameof(Index));
        }

        _dbContext.Units.Remove(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Xóa thành công", "Đơn vị đo đã được xóa.");
        return RedirectToAction(nameof(Index));
    }
}
