using Logistics_service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Logistics_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession(); // Добавляем middleware для сессий

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithViews();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole", policy =>
                    policy.RequireRole("Administrator"));
                options.AddPolicy("RequireManagerRole", policy =>
                    policy.RequireRole("Manager"));
                options.AddPolicy("RequireCustomerRole", policy =>
                    policy.RequireRole("Customer"));
            });

            // Настройка сессий
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Настройка аутентификации
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Auth/Login");
                    options.AccessDeniedPath = new PathString("/Auth/AccessDenied");
                });
        }
    }
}