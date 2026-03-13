using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
//  Commnet
public class Comment : BaseEntity
{
    public int RecipeId { get; set; }

    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 2)]
    public string Content { get; set; } = string.Empty;

    public Recipe? Recipe { get; set; }

    public ApplicationUser? User { get; set; }
}
