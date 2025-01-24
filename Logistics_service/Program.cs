using Logistics_service.Services;
using Logistics_service.Static;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json.Serialization;

namespace Logistics_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsProduction())
            {
                builder.WebHost.ConfigureKestrel((context, serverOptions) =>
                {
                    var kestrelSection = context.Configuration.GetSection("Kestrel");
                    serverOptions.Configure(kestrelSection);
                });
            }

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            PrintLocalIpAddresses();
            ConfigureMiddleware(app);
            ConfigureDatabase(app);

            if (!app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddScoped<DigestAuthFilter>();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddAutoMapper(typeof(MapperService).Assembly);

            services.AddSingleton<VehicleService>();
            services.AddMemoryCache();

            services.AddSingleton<WaitingOrderService>();
            services.AddHostedService(provider => provider.GetRequiredService<WaitingOrderService>());
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
        }

        private static void ConfigureDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            GenerateDigest.Configuration = services.GetRequiredService<IConfiguration>();
            GenerateDigest.Context = services.GetRequiredService<ApplicationDbContext>();

            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
        }

        private static void PrintLocalIpAddresses()
        {
            try
            {
                var hostName = Dns.GetHostName();
                Console.WriteLine($"Host Name: {hostName}");

                var ipAddresses = Dns.GetHostAddresses(hostName)
                    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToList();

                if (ipAddresses.Count != 0)
                {
                    Console.WriteLine("Local IP Addresses:");
                    foreach (var ip in ipAddresses)
                    {
                        Console.WriteLine($"- {ip}");
                    }
                }
                else
                {
                    Console.WriteLine("No IPv4 addresses found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving IP addresses: {ex.Message}");
            }
        }
    }
}