using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
//  Công  thức nấu ăn
public class Recipe : BaseEntity
{
    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(260)]
    public string? ThumbnailPath { get; set; }

    [Range(1, 50)]
    public int Servings { get; set; }

    [Range(1, 600)]
    public int CookTimeMinutes { get; set; }

    public bool IsVisible { get; set; } = true;

    public int CategoryId { get; set; }

    public string AuthorId { get; set; } = string.Empty;

    public Category? Category { get; set; }

    public ApplicationUser? Author { get; set; }

    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
