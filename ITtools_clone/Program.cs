using Microsoft.EntityFrameworkCore;
using ITtools_clone.Repositories;
using ITtools_clone.Services;
using ITtools_clone.Models;
using ITtools_clone.Middlewares;
using DotNetEnv;

namespace ITtools_clone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
                        // Lấy chuỗi kết nối từ appsettings.json
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

            // Đăng ký Entity Framework Core với MySQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Cấu hình Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddHttpContextAccessor();

            // Đăng ký Logging
            builder.Services.AddLogging();

            // Register repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IToolRepository, ToolRepository>();
            builder.Services.AddScoped<IFavouriteRepository, FavouriteRepository>();

            // Register services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IToolService, ToolService>();
            builder.Services.AddScoped<IFavouriteService, FavouriteService>();

            builder.Services.AddDistributedMemoryCache(); // Lưu trữ session trong bộ nhớ

            var app = builder.Build();

            // Kiểm tra kết nối MySQL TRƯỚC KHI CHẠY ỨNG DỤNG
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                try
                {
                    if (dbContext.Database.CanConnect())
                    {
                        logger.LogInformation("✅ Kết nối MySQL thành công!");
                    }
                    else
                    {
                        logger.LogWarning("⚠ Không thể kết nối đến MySQL.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Lỗi kết nối MySQL");
                }
            }

            // 📌 Cấu hình pipeline HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSession(); // 🔥 Đặt trước middleware để tránh lỗi Session
            app.UseMiddleware<PremiumToolAccessMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
