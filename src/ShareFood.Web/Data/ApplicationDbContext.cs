using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Models.Entities;

namespace ShareFood.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private static readonly DateTime SeedTimestamp = new(2026, 03, 13, 0, 0, 0, DateTimeKind.Utc);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName).HasMaxLength(120);
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(category => category.Name).IsUnique();
            entity.HasIndex(category => category.Slug).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(100);
            entity.Property(category => category.Slug).HasMaxLength(120);
            entity.Property(category => category.Description).HasMaxLength(500);

            entity.HasData(
                new Category { Id = 1, Name = "Món chính", Slug = "mon-chinh", Description = "Những món ăn chính dùng cho bữa trưa hoặc bữa tối.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Category { Id = 2, Name = "Món chay", Slug = "mon-chay", Description = "Công thức thanh đạm, tốt cho sức khỏe.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Category { Id = 3, Name = "Món tráng miệng", Slug = "mon-trang-mieng", Description = "Các món ngọt nhẹ cho bữa ăn thêm trọn vẹn.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Category { Id = 4, Name = "Đồ uống", Slug = "do-uong", Description = "Thức uống ngon, dễ làm tại nhà.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp });
        });

        builder.Entity<Ingredient>(entity =>
        {
            entity.HasIndex(ingredient => ingredient.Name).IsUnique();
            entity.Property(ingredient => ingredient.Name).HasMaxLength(120);
            entity.Property(ingredient => ingredient.Description).HasMaxLength(300);

            entity.HasData(
                new Ingredient { Id = 1, Name = "Thịt gà", Description = "Nguyên liệu giàu đạm, dễ chế biến.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Ingredient { Id = 2, Name = "Cà rốt", Description = "Tạo vị ngọt tự nhiên và màu sắc đẹp.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Ingredient { Id = 3, Name = "Khoai tây", Description = "Nguyên liệu quen thuộc trong nhiều món hầm.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Ingredient { Id = 4, Name = "Sữa tươi", Description = "Dùng cho món tráng miệng và đồ uống.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Ingredient { Id = 5, Name = "Bột mì", Description = "Nguyên liệu nền cho nhiều món bánh.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Ingredient { Id = 6, Name = "Đường", Description = "Tăng vị ngọt cho món ăn.", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp });
        });

        builder.Entity<Unit>(entity =>
        {
            entity.HasIndex(unit => unit.Name).IsUnique();
            entity.Property(unit => unit.Name).HasMaxLength(80);
            entity.Property(unit => unit.Symbol).HasMaxLength(20);

            entity.HasData(
                new Unit { Id = 1, Name = "Gram", Symbol = "g", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Unit { Id = 2, Name = "Kilogram", Symbol = "kg", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Unit { Id = 3, Name = "Mi-li-lít", Symbol = "ml", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Unit { Id = 4, Name = "Lít", Symbol = "l", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Unit { Id = 5, Name = "Cái", Symbol = "cái", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
                new Unit { Id = 6, Name = "Muỗng", Symbol = "muỗng", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp });
        });

        builder.Entity<Recipe>(entity =>
        {
            entity.HasIndex(recipe => recipe.Slug).IsUnique();
            entity.Property(recipe => recipe.Title).HasMaxLength(160);
            entity.Property(recipe => recipe.Slug).HasMaxLength(180);
            entity.Property(recipe => recipe.Summary).HasMaxLength(1000);
            entity.Property(recipe => recipe.ThumbnailPath).HasMaxLength(260);

            entity.HasOne(recipe => recipe.Category)
                .WithMany(category => category.Recipes)
                .HasForeignKey(recipe => recipe.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(recipe => recipe.Author)
                .WithMany(user => user.Recipes)
                .HasForeignKey(recipe => recipe.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RecipeStep>(entity =>
        {
            entity.HasIndex(step => new { step.RecipeId, step.StepNumber }).IsUnique();
            entity.Property(step => step.Instruction).HasMaxLength(2000);

            entity.HasOne(step => step.Recipe)
                .WithMany(recipe => recipe.Steps)
                .HasForeignKey(step => step.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasIndex(recipeIngredient => new { recipeIngredient.RecipeId, recipeIngredient.IngredientId }).IsUnique();
            entity.Property(recipeIngredient => recipeIngredient.Quantity).HasPrecision(10, 2);
            entity.Property(recipeIngredient => recipeIngredient.Note).HasMaxLength(250);

            entity.HasOne(recipeIngredient => recipeIngredient.Recipe)
                .WithMany(recipe => recipe.Ingredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(recipeIngredient => recipeIngredient.Ingredient)
                .WithMany(ingredient => ingredient.RecipeIngredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(recipeIngredient => recipeIngredient.Unit)
                .WithMany(unit => unit.RecipeIngredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Comment>(entity =>
        {
            entity.Property(comment => comment.Content).HasMaxLength(1000);

            entity.HasOne(comment => comment.Recipe)
                .WithMany(recipe => recipe.Comments)
                .HasForeignKey(comment => comment.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(comment => comment.User)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(favorite => new { favorite.RecipeId, favorite.UserId }).IsUnique();

            entity.HasOne(favorite => favorite.Recipe)
                .WithMany(recipe => recipe.Favorites)
                .HasForeignKey(favorite => favorite.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(favorite => favorite.User)
                .WithMany(user => user.Favorites)
                .HasForeignKey(favorite => favorite.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.HasIndex(notification => new { notification.RecipientUserId, notification.IsRead, notification.CreatedAt });
            entity.Property(notification => notification.Title).HasMaxLength(160);
            entity.Property(notification => notification.Message).HasMaxLength(500);

            entity.HasOne(notification => notification.RecipientUser)
                .WithMany(user => user.Notifications)
                .HasForeignKey(notification => notification.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(notification => notification.Recipe)
                .WithMany(recipe => recipe.Notifications)
                .HasForeignKey(notification => notification.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmailOtp>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.IsUsed, item.ExpiresAt });
            entity.Property(item => item.Code).HasMaxLength(6);

            entity.HasOne(item => item.User)
                .WithMany(user => user.EmailOtps)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ApplyTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
