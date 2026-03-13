namespace ShareFood.Web.Models.Entities;
//  Yeu thich 
public class Favorite : BaseEntity
{
    public int RecipeId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Recipe? Recipe { get; set; }

    public ApplicationUser? User { get; set; }
}
