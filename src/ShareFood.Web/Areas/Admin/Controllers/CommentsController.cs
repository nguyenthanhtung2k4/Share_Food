using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.ViewModels.Admin;

namespace ShareFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("quan-tri/binh-luan")]
public class CommentsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public CommentsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.Comments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(item => item.Content.Contains(keyword) || item.Recipe!.Title.Contains(keyword));
        }

        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new AdminCommentListItemViewModel
            {
                Id = item.Id,
                RecipeId = item.RecipeId,
                RecipeTitle = item.Recipe!.Title,
                RecipeSlug = item.Recipe.Slug,
                UserName = item.User!.FullName,
                Content = item.Content,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        ViewData["Keyword"] = keyword;
        return View(items);
    }

    [HttpPost("xoa/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var comment = await _dbContext.Comments.FindAsync([id], cancellationToken);
        if (comment is null)
        {
            TempData.SetError("Không tìm thấy", "Bình luận cần xóa không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Xóa thành công", "Bình luận đã được xóa.");
        return RedirectToAction(nameof(Index));
    }
}
