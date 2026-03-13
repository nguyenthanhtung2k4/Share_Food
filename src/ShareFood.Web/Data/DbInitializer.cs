using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ShareFood.Web.Models.Entities;

namespace ShareFood.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { RoleNames.Admin, RoleNames.User })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        const string adminEmail = "admin@sharefood.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Quản trị viên Share Food",
                EmailConfirmed = true,
                JoinedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin1234");
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Không thể tạo tài khoản quản trị mặc định: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
        }

        const string memberEmail = "thanhvien@sharefood.local";
        var memberUser = await userManager.FindByEmailAsync(memberEmail);
        if (memberUser is null)
        {
            memberUser = new ApplicationUser
            {
                UserName = memberEmail,
                Email = memberEmail,
                FullName = "Thành viên dùng thử",
                EmailConfirmed = true,
                JoinedAt = DateTime.UtcNow
            };

            var memberResult = await userManager.CreateAsync(memberUser, "Thanhvien123");
            if (!memberResult.Succeeded)
            {
                var errors = string.Join("; ", memberResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Không thể tạo tài khoản dùng thử: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(memberUser, RoleNames.User))
        {
            await userManager.AddToRoleAsync(memberUser, RoleNames.User);
        }

        if (!await dbContext.Recipes.AnyAsync())
        {
            var recipe = new Recipe
            {
                Title = "Gà hầm rau củ",
                Slug = "ga-ham-rau-cu",
                Summary = "Món gà hầm mềm thơm, nước dùng ngọt tự nhiên từ rau củ, rất hợp cho bữa cơm gia đình.",
                CategoryId = 1,
                AuthorId = memberUser.Id,
                Servings = 4,
                CookTimeMinutes = 50,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Rửa sạch gà, chặt miếng vừa ăn và ướp nhẹ với muối." },
                    new RecipeStep { StepNumber = 2, Instruction = "Phi thơm hành, cho gà vào đảo săn rồi thêm nước vừa ngập mặt." },
                    new RecipeStep { StepNumber = 3, Instruction = "Cho cà rốt và khoai tây vào nấu lửa nhỏ đến khi mềm, nêm nếm vừa ăn." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = 1, Quantity = 700, UnitId = 1, Note = "Chặt miếng vừa ăn" },
                    new RecipeIngredient { IngredientId = 2, Quantity = 2, UnitId = 5, Note = "Cắt khúc" },
                    new RecipeIngredient { IngredientId = 3, Quantity = 2, UnitId = 5, Note = "Cắt miếng lớn" }
                ]
            };

            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();

            dbContext.Comments.Add(new Comment
            {
                RecipeId = recipe.Id,
                UserId = adminUser.Id,
                Content = "Món này rất hợp cho bữa tối, nước dùng ngọt và dễ ăn."
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
