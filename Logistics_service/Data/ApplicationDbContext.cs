using Microsoft.EntityFrameworkCore;
using Logistics_service.Models;

namespace Logistics_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Models.Route> Routes { get; set; }
        public DbSet<Point> Points { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Map> Maps { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей между таблицами
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Orders)
                .WithOne(o => o.Vehicle)
                .HasForeignKey(o => o.VehicleId);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Route)
                .WithOne(r => r.Vehicle)
                .HasForeignKey<Models.Route>(r => r.VehicleId);

            modelBuilder.Entity<Models.Route>()
                .HasMany(r => r.Points)
                .WithOne(p => p.Route)
                .HasForeignKey(p => p.RouteId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Points)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.Recipient)
                .HasForeignKey(n => n.RecipientId);
        }
    }
}
