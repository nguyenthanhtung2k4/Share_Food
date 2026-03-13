using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.Services;
using ShareFood.Web.Settings;

namespace ShareFood.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu cấu hình kết nối cơ sở dữ liệu.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection(EmailSettings.SectionName));

        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<VietnameseIdentityErrorDescriber>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/tai-khoan/dang-nhap";
            options.LogoutPath = "/tai-khoan/dang-xuat";
            options.AccessDeniedPath = "/truy-cap-bi-tu-choi";
            options.Cookie.Name = "ShareFood.Auth";
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleNames.Admin));
        });

        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();
        builder.Services.AddScoped<IRecipeQueryService, RecipeQueryService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            await DbInitializer.InitializeAsync(scope.ServiceProvider);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}
