using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
// Nguyen lieu  cong thuc
public class RecipeIngredient : BaseEntity
{
    public int RecipeId { get; set; }

    public int IngredientId { get; set; }

    public int UnitId { get; set; }

    [Range(typeof(decimal), "0.1", "99999")]
    public decimal Quantity { get; set; }

    [StringLength(250)]
    public string? Note { get; set; }

    public Recipe? Recipe { get; set; }

    public Ingredient? Ingredient { get; set; }

    public Unit? Unit { get; set; }
}
