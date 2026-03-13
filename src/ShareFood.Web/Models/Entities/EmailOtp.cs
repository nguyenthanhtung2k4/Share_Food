using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;

public class EmailOtp : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(6)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public ApplicationUser? User { get; set; }
}
