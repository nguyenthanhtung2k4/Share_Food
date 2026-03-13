using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
//  Công thức từng bước nấu  ăn
public class RecipeStep : BaseEntity
{
    public int RecipeId { get; set; }

    [Range(1, 50)]
    public int StepNumber { get; set; }

    [Required]
    [StringLength(2000)]
    public string Instruction { get; set; } = string.Empty;

    public Recipe? Recipe { get; set; }
}
