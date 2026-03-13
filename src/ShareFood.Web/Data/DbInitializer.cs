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

        if (!adminUser.ReceiveNewsEmails)
        {
            adminUser.ReceiveNewsEmails = true;
            await userManager.UpdateAsync(adminUser);
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

        if (!memberUser.ReceiveNewsEmails)
        {
            memberUser.ReceiveNewsEmails = true;
            await userManager.UpdateAsync(memberUser);
        }

        var bepNhaUser = await EnsureUserAsync(userManager, "bepnha@sharefood.local", "Bếp Nhà Cuối Tuần", "Bepnha123");
        var monNgonUser = await EnsureUserAsync(userManager, "monngon@sharefood.local", "Món Ngon Mỗi Ngày", "Monngon123");
        var anNhienUser = await EnsureUserAsync(userManager, "annhien@sharefood.local", "An Nhiên Vào Bếp", "Annhiem123");

        await EnsureIngredientsAsync(dbContext);
        await EnsureRecipesAsync(dbContext, adminUser, memberUser, bepNhaUser, monNgonUser, anNhienUser);
        await EnsureInteractionsAsync(dbContext, adminUser, memberUser, bepNhaUser, monNgonUser, anNhienUser);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                JoinedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Không thể tạo tài khoản mẫu {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, RoleNames.User))
        {
            await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        if (!user.ReceiveNewsEmails)
        {
            user.ReceiveNewsEmails = true;
            await userManager.UpdateAsync(user);
        }

        return user;
    }

    private static async Task EnsureIngredientsAsync(ApplicationDbContext dbContext)
    {
        var ingredientCatalog = new Dictionary<string, string>
        {
            ["Thịt bò"] = "Thích hợp cho các món nước nóng và đậm vị.",
            ["Bánh phở"] = "Sợi phở mềm, dùng cho các món nước hoặc xào.",
            ["Nước dùng"] = "Phần nước ninh xương hoặc rau củ làm nền hương vị.",
            ["Rau thơm"] = "Nhóm rau ăn kèm giúp món ăn tươi mát hơn.",
            ["Bột gạo"] = "Nguyên liệu nền cho các loại bánh và vỏ giòn.",
            ["Tôm"] = "Nguyên liệu hải sản dễ kết hợp với rau và bánh.",
            ["Thịt ba chỉ"] = "Tạo độ béo thơm cho món cuốn và món xào.",
            ["Xà lách"] = "Rau ăn kèm giúp cân bằng hương vị.",
            ["Bún tươi"] = "Sợi bún mềm dùng cho món cuốn hoặc món trộn.",
            ["Cà phê xay"] = "Phần cà phê đậm vị cho đồ uống truyền thống.",
            ["Sữa đặc"] = "Tăng độ béo và vị ngọt cho đồ uống, món tráng miệng.",
            ["Chanh dây"] = "Loại quả có vị chua thơm dùng làm sốt hoặc nước uống.",
            ["Dâu tây"] = "Loại quả thích hợp cho sinh tố và món tráng miệng.",
            ["Sữa chua"] = "Tạo độ chua nhẹ và vị dịu cho món lạnh.",
            ["Nấm"] = "Nguyên liệu thanh vị, hợp với món chay và món súp.",
            ["Kem tươi"] = "Tăng độ béo mượt cho các món súp hoặc món ngọt.",
            ["Trứng gà"] = "Nguyên liệu cơ bản cho nhiều món bánh.",
            ["Bơ lạt"] = "Giúp món bánh thơm và mềm hơn.",
            ["Mật ong"] = "Tạo vị ngọt thơm tự nhiên.",
            ["Táo"] = "Trái cây giòn ngọt dùng cho salad và món lạnh.",
            ["Chuối"] = "Nguyên liệu mềm ngọt hợp với sinh tố hoặc bánh.",
            ["Dưa leo"] = "Rau củ thanh mát cho món cuốn và salad."
        };

        var existingNames = await dbContext.Ingredients
            .AsNoTracking()
            .Select(item => item.Name)
            .ToHashSetAsync();

        var missingIngredients = ingredientCatalog
            .Where(item => !existingNames.Contains(item.Key))
            .Select(item => new Ingredient
            {
                Name = item.Key,
                Description = item.Value
            })
            .ToList();

        if (missingIngredients.Count == 0)
        {
            return;
        }

        dbContext.Ingredients.AddRange(missingIngredients);
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureRecipesAsync(
        ApplicationDbContext dbContext,
        ApplicationUser adminUser,
        ApplicationUser memberUser,
        ApplicationUser bepNhaUser,
        ApplicationUser monNgonUser,
        ApplicationUser anNhienUser)
    {
        var ingredientIds = await dbContext.Ingredients
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Name, item => item.Id);

        var existingSlugs = await dbContext.Recipes
            .AsNoTracking()
            .Select(item => item.Slug)
            .ToHashSetAsync();

        var recipesToAdd = new List<Recipe>();

        if (!existingSlugs.Contains("ga-ham-rau-cu"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Gà hầm rau củ",
                Slug = "ga-ham-rau-cu",
                Summary = "Món gà hầm mềm thơm, nước dùng ngọt tự nhiên từ rau củ, rất hợp cho bữa cơm gia đình.",
                ThumbnailPath = "/uploads/recipes/db28e8c96400419f8770f897f55db579.jpg",
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
                    new RecipeIngredient { IngredientId = ingredientIds["Thịt gà"], Quantity = 700, UnitId = 1, Note = "Chặt miếng vừa ăn" },
                    new RecipeIngredient { IngredientId = ingredientIds["Cà rốt"], Quantity = 2, UnitId = 5, Note = "Cắt khúc" },
                    new RecipeIngredient { IngredientId = ingredientIds["Khoai tây"], Quantity = 2, UnitId = 5, Note = "Cắt miếng lớn" }
                ]
            });
        }

        if (!existingSlugs.Contains("pho-bo-tai-nha"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Phở bò tại nhà",
                Slug = "pho-bo-tai-nha",
                Summary = "Bát phở nóng với nước dùng trong, vị thơm dịu và topping bò mềm rất hợp cho bữa sáng cuối tuần.",
                ThumbnailPath = "/uploads/seed/pho-bo.jpg",
                CategoryId = 1,
                AuthorId = bepNhaUser.Id,
                Servings = 4,
                CookTimeMinutes = 60,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Đun nóng nước dùng, nêm thêm chút muối và đường để vị hài hòa." },
                    new RecipeStep { StepNumber = 2, Instruction = "Trụng bánh phở nhanh trong nước nóng rồi cho vào tô cùng thịt bò thái mỏng." },
                    new RecipeStep { StepNumber = 3, Instruction = "Chan nước dùng, thêm hành tây và rau thơm rồi thưởng thức khi còn nóng." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Thịt bò"], Quantity = 400, UnitId = 1, Note = "Thái lát mỏng" },
                    new RecipeIngredient { IngredientId = ingredientIds["Bánh phở"], Quantity = 350, UnitId = 1, Note = "Trụng nhanh trước khi dùng" },
                    new RecipeIngredient { IngredientId = ingredientIds["Nước dùng"], Quantity = 2, UnitId = 4, Note = "Giữ nóng liên tục" },
                    new RecipeIngredient { IngredientId = ingredientIds["Rau thơm"], Quantity = 120, UnitId = 1, Note = "Dùng ăn kèm" }
                ]
            });
        }

        if (!existingSlugs.Contains("goi-cuon-tom-thit"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Gỏi cuốn tôm thịt",
                Slug = "goi-cuon-tom-thit",
                Summary = "Món cuốn thanh mát với tôm, thịt và nhiều rau xanh, phù hợp cho bữa nhẹ hoặc khai vị.",
                ThumbnailPath = "/uploads/seed/goi-cuon.jpg",
                CategoryId = 1,
                AuthorId = monNgonUser.Id,
                Servings = 3,
                CookTimeMinutes = 25,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Luộc tôm và thịt ba chỉ đến khi chín, để nguội rồi cắt lát vừa ăn." },
                    new RecipeStep { StepNumber = 2, Instruction = "Chuẩn bị bún tươi, xà lách và rau thơm thành từng phần nhỏ dễ cuốn." },
                    new RecipeStep { StepNumber = 3, Instruction = "Làm mềm bánh tráng, xếp nguyên liệu vào rồi cuốn chắc tay trước khi dùng." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Tôm"], Quantity = 250, UnitId = 1, Note = "Luộc chín" },
                    new RecipeIngredient { IngredientId = ingredientIds["Thịt ba chỉ"], Quantity = 200, UnitId = 1, Note = "Luộc chín và cắt lát" },
                    new RecipeIngredient { IngredientId = ingredientIds["Xà lách"], Quantity = 1, UnitId = 5, Note = "Tách lá vừa cuốn" },
                    new RecipeIngredient { IngredientId = ingredientIds["Bún tươi"], Quantity = 250, UnitId = 1, Note = "Chia thành phần nhỏ" },
                    new RecipeIngredient { IngredientId = ingredientIds["Rau thơm"], Quantity = 80, UnitId = 1, Note = "Ăn kèm" }
                ]
            });
        }

        if (!existingSlugs.Contains("ca-phe-sua-da-mat-lanh"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Cà phê sữa đá mát lạnh",
                Slug = "ca-phe-sua-da-mat-lanh",
                Summary = "Ly cà phê sữa đá đậm đà, ngọt béo vừa phải, rất hợp cho buổi sáng tỉnh táo.",
                ThumbnailPath = "/uploads/seed/ca-phe-sua-da.jpg",
                CategoryId = 4,
                AuthorId = adminUser.Id,
                Servings = 1,
                CookTimeMinutes = 10,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Cho cà phê xay vào phin và châm nước sôi vừa đủ để chiết xuất." },
                    new RecipeStep { StepNumber = 2, Instruction = "Rót sữa đặc vào ly, thêm cà phê vừa pha rồi khuấy đều." },
                    new RecipeStep { StepNumber = 3, Instruction = "Thêm đá viên và phục vụ ngay khi cà phê còn thơm." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Cà phê xay"], Quantity = 25, UnitId = 1, Note = "Chọn loại rang vừa" },
                    new RecipeIngredient { IngredientId = ingredientIds["Sữa đặc"], Quantity = 45, UnitId = 3, Note = "Gia giảm theo khẩu vị" }
                ]
            });
        }

        if (!existingSlugs.Contains("salad-rau-cu-sot-chanh-day"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Salad rau củ sốt chanh dây",
                Slug = "salad-rau-cu-sot-chanh-day",
                Summary = "Đĩa salad tươi mát với sốt chanh dây chua dịu, phù hợp cho bữa nhẹ hoặc món khai vị.",
                ThumbnailPath = "/uploads/seed/salad-rau-cu.jpg",
                CategoryId = 2,
                AuthorId = anNhienUser.Id,
                Servings = 2,
                CookTimeMinutes = 15,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Rửa sạch xà lách, dưa leo và cà rốt rồi để ráo hoàn toàn." },
                    new RecipeStep { StepNumber = 2, Instruction = "Trộn chanh dây với chút đường và muối để tạo thành phần sốt chua ngọt." },
                    new RecipeStep { StepNumber = 3, Instruction = "Xếp rau củ ra đĩa, rưới sốt lên mặt và dùng ngay để giữ độ giòn." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Xà lách"], Quantity = 1, UnitId = 5, Note = "Xé miếng vừa ăn" },
                    new RecipeIngredient { IngredientId = ingredientIds["Dưa leo"], Quantity = 1, UnitId = 5, Note = "Thái lát mỏng" },
                    new RecipeIngredient { IngredientId = ingredientIds["Cà rốt"], Quantity = 1, UnitId = 5, Note = "Bào sợi" },
                    new RecipeIngredient { IngredientId = ingredientIds["Chanh dây"], Quantity = 2, UnitId = 5, Note = "Lọc lấy ruột" }
                ]
            });
        }

        if (!existingSlugs.Contains("pancake-mat-ong-trai-cay"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Pancake mật ong trái cây",
                Slug = "pancake-mat-ong-trai-cay",
                Summary = "Bánh pancake mềm xốp dùng cùng mật ong và trái cây tươi, hợp cho bữa sáng thư giãn.",
                ThumbnailPath = "/uploads/seed/pancake-mat-ong.jpg",
                CategoryId = 3,
                AuthorId = bepNhaUser.Id,
                Servings = 3,
                CookTimeMinutes = 20,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Trộn bột mì, trứng gà và sữa tươi đến khi hỗn hợp mịn đều." },
                    new RecipeStep { StepNumber = 2, Instruction = "Làm nóng chảo với chút bơ lạt rồi đổ từng vá bột nhỏ để áp chảo." },
                    new RecipeStep { StepNumber = 3, Instruction = "Xếp bánh ra đĩa, rưới mật ong và thêm trái cây tươi trước khi dùng." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Bột mì"], Quantity = 220, UnitId = 1, Note = "Rây mịn trước khi trộn" },
                    new RecipeIngredient { IngredientId = ingredientIds["Trứng gà"], Quantity = 2, UnitId = 5, Note = "Đánh tan" },
                    new RecipeIngredient { IngredientId = ingredientIds["Sữa tươi"], Quantity = 180, UnitId = 3, Note = "Để ở nhiệt độ phòng" },
                    new RecipeIngredient { IngredientId = ingredientIds["Bơ lạt"], Quantity = 30, UnitId = 1, Note = "Dùng để chống dính" },
                    new RecipeIngredient { IngredientId = ingredientIds["Mật ong"], Quantity = 3, UnitId = 6, Note = "Rưới lúc ăn" }
                ]
            });
        }

        if (!existingSlugs.Contains("sinh-to-dau-sua-chua"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Sinh tố dâu sữa chua",
                Slug = "sinh-to-dau-sua-chua",
                Summary = "Ly sinh tố mát lạnh với vị chua ngọt dễ uống, rất phù hợp cho những ngày nắng nóng.",
                ThumbnailPath = "/uploads/seed/sinh-to-dau.jpg",
                CategoryId = 4,
                AuthorId = monNgonUser.Id,
                Servings = 2,
                CookTimeMinutes = 10,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Rửa sạch dâu tây, cắt bỏ cuống rồi để ráo nước." },
                    new RecipeStep { StepNumber = 2, Instruction = "Cho dâu, sữa chua và chút sữa tươi vào máy xay đến khi mịn." },
                    new RecipeStep { StepNumber = 3, Instruction = "Rót ra ly, thêm đá nếu thích lạnh sâu và dùng ngay." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Dâu tây"], Quantity = 220, UnitId = 1, Note = "Chọn quả chín đỏ" },
                    new RecipeIngredient { IngredientId = ingredientIds["Sữa chua"], Quantity = 2, UnitId = 5, Note = "Loại không đường" },
                    new RecipeIngredient { IngredientId = ingredientIds["Sữa tươi"], Quantity = 120, UnitId = 3, Note = "Điều chỉnh độ sánh" }
                ]
            });
        }

        if (!existingSlugs.Contains("sup-nam-kem-beo"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Súp nấm kem béo",
                Slug = "sup-nam-kem-beo",
                Summary = "Món súp nóng nhẹ với nấm và kem tươi, hương vị dịu và phù hợp cho bữa tối thanh đạm.",
                ThumbnailPath = "/uploads/seed/sup-nam.jpg",
                CategoryId = 2,
                AuthorId = anNhienUser.Id,
                Servings = 3,
                CookTimeMinutes = 30,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Áp chảo nấm với chút bơ đến khi dậy mùi thơm." },
                    new RecipeStep { StepNumber = 2, Instruction = "Thêm nước dùng vào nấu sôi nhẹ rồi xay sơ cho hỗn hợp mượt hơn." },
                    new RecipeStep { StepNumber = 3, Instruction = "Cho kem tươi vào sau cùng, khuấy đều và nêm nếm trước khi dùng." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Nấm"], Quantity = 320, UnitId = 1, Note = "Cắt lát vừa" },
                    new RecipeIngredient { IngredientId = ingredientIds["Kem tươi"], Quantity = 150, UnitId = 3, Note = "Cho vào cuối cùng" },
                    new RecipeIngredient { IngredientId = ingredientIds["Nước dùng"], Quantity = 800, UnitId = 3, Note = "Dùng nước rau củ" },
                    new RecipeIngredient { IngredientId = ingredientIds["Bơ lạt"], Quantity = 20, UnitId = 1, Note = "Làm thơm nấm" }
                ]
            });
        }

        if (!existingSlugs.Contains("salad-trai-cay-sua-chua"))
        {
            recipesToAdd.Add(new Recipe
            {
                Title = "Salad trái cây sữa chua",
                Slug = "salad-trai-cay-sua-chua",
                Summary = "Món tráng miệng thanh mát kết hợp trái cây tươi và sữa chua, phù hợp cho cả người lớn lẫn trẻ nhỏ.",
                ThumbnailPath = "/uploads/seed/salad-trai-cay.jpg",
                CategoryId = 3,
                AuthorId = memberUser.Id,
                Servings = 2,
                CookTimeMinutes = 12,
                IsVisible = true,
                Steps =
                [
                    new RecipeStep { StepNumber = 1, Instruction = "Cắt táo, chuối và dâu thành miếng vừa ăn để riêng." },
                    new RecipeStep { StepNumber = 2, Instruction = "Trộn nhẹ các loại trái cây với sữa chua để áo đều bề mặt." },
                    new RecipeStep { StepNumber = 3, Instruction = "Làm lạnh vài phút trước khi dùng để món ăn tươi mát hơn." }
                ],
                Ingredients =
                [
                    new RecipeIngredient { IngredientId = ingredientIds["Táo"], Quantity = 1, UnitId = 5, Note = "Cắt hạt lựu" },
                    new RecipeIngredient { IngredientId = ingredientIds["Chuối"], Quantity = 1, UnitId = 5, Note = "Thái khoanh" },
                    new RecipeIngredient { IngredientId = ingredientIds["Dâu tây"], Quantity = 120, UnitId = 1, Note = "Cắt đôi" },
                    new RecipeIngredient { IngredientId = ingredientIds["Sữa chua"], Quantity = 2, UnitId = 5, Note = "Trộn đều khi dùng" }
                ]
            });
        }

        if (recipesToAdd.Count == 0)
        {
            return;
        }

        dbContext.Recipes.AddRange(recipesToAdd);
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureInteractionsAsync(
        ApplicationDbContext dbContext,
        ApplicationUser adminUser,
        ApplicationUser memberUser,
        ApplicationUser bepNhaUser,
        ApplicationUser monNgonUser,
        ApplicationUser anNhienUser)
    {
        var recipes = await dbContext.Recipes
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Slug, item => item);

        await AddCommentIfMissingAsync(dbContext, recipes["ga-ham-rau-cu"].Id, adminUser.Id, "Món này rất hợp cho bữa tối, nước dùng ngọt và dễ ăn.");
        await AddCommentIfMissingAsync(dbContext, recipes["pho-bo-tai-nha"].Id, memberUser.Id, "Nước dùng thơm nhẹ, rất dễ làm để ăn sáng cuối tuần.");
        await AddCommentIfMissingAsync(dbContext, recipes["goi-cuon-tom-thit"].Id, anNhienUser.Id, "Cuốn chắc tay và chấm cùng nước mắm chua ngọt là chuẩn bài.");
        await AddCommentIfMissingAsync(dbContext, recipes["pancake-mat-ong-trai-cay"].Id, adminUser.Id, "Bánh mềm, lên màu đẹp và ăn cùng trái cây rất cân vị.");
        await AddCommentIfMissingAsync(dbContext, recipes["sup-nam-kem-beo"].Id, monNgonUser.Id, "Món súp này rất hợp cho ngày mưa hoặc bữa tối nhẹ.");

        await AddFavoriteIfMissingAsync(dbContext, recipes["pho-bo-tai-nha"].Id, adminUser.Id);
        await AddFavoriteIfMissingAsync(dbContext, recipes["goi-cuon-tom-thit"].Id, memberUser.Id);
        await AddFavoriteIfMissingAsync(dbContext, recipes["salad-rau-cu-sot-chanh-day"].Id, bepNhaUser.Id);
        await AddFavoriteIfMissingAsync(dbContext, recipes["pancake-mat-ong-trai-cay"].Id, anNhienUser.Id);
        await AddFavoriteIfMissingAsync(dbContext, recipes["sinh-to-dau-sua-chua"].Id, adminUser.Id);
    }

    private static async Task AddCommentIfMissingAsync(ApplicationDbContext dbContext, int recipeId, string userId, string content)
    {
        var exists = await dbContext.Comments
            .AnyAsync(item => item.RecipeId == recipeId && item.UserId == userId);

        if (exists)
        {
            return;
        }

        dbContext.Comments.Add(new Comment
        {
            RecipeId = recipeId,
            UserId = userId,
            Content = content
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task AddFavoriteIfMissingAsync(ApplicationDbContext dbContext, int recipeId, string userId)
    {
        var exists = await dbContext.Favorites
            .AnyAsync(item => item.RecipeId == recipeId && item.UserId == userId);

        if (exists)
        {
            return;
        }

        dbContext.Favorites.Add(new Favorite
        {
            RecipeId = recipeId,
            UserId = userId
        });

        await dbContext.SaveChangesAsync();
    }
}
