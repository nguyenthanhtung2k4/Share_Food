using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
//  user  người dung
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    public bool ReceiveNewsEmails { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<EmailOtp> EmailOtps { get; set; } = new List<EmailOtp>();
}
