using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;

public class Notification : BaseEntity
{
    public string RecipientUserId { get; set; } = string.Empty;

    public int RecipeId { get; set; }

    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public ApplicationUser? RecipientUser { get; set; }

    public Recipe? Recipe { get; set; }
}
