using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeCreateEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên công thức.")]
    [Display(Name = "Tên công thức")]
    [StringLength(160, MinimumLength = 5, ErrorMessage = "Tên công thức phải từ 5 đến 160 ký tự.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn.")]
    [Display(Name = "Mô tả ngắn")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "Mô tả ngắn phải từ 20 đến 1000 ký tự.")]
    public string Summary { get; set; } = string.Empty;

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? Thumbnail { get; set; }

    public string? ExistingThumbnailPath { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
    [Display(Name = "Danh mục")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số khẩu phần.")]
    [Display(Name = "Khẩu phần")]
    [Range(1, 50, ErrorMessage = "Số khẩu phần phải từ 1 đến 50.")]
    public int Servings { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thời gian nấu.")]
    [Display(Name = "Thời gian nấu (phút)")]
    [Range(1, 600, ErrorMessage = "Thời gian nấu phải từ 1 đến 600 phút.")]
    public int CookTimeMinutes { get; set; }

    public List<RecipeIngredientInputViewModel> Ingredients { get; set; } = new();

    public List<RecipeStepInputViewModel> Steps { get; set; } = new();

    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> AvailableIngredients { get; set; } = Array.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Units { get; set; } = Array.Empty<SelectListItem>();
}
