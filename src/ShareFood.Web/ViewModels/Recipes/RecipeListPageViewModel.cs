using Microsoft.AspNetCore.Mvc.Rendering;
using ShareFood.Web.ViewModels.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeListPageViewModel
{
    [Display(Name = "Từ khóa")]
    public string? Keyword { get; set; }

    [Display(Name = "Danh mục")]
    public int? CategoryId { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();

    public PagedResult<RecipeCardViewModel> Recipes { get; set; } = new();
}
