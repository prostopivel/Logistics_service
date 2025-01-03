using Logistics_service.Models;
using Logistics_service.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Logistics_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Point> Points { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Status)
                .HasDefaultValue(VehicleStatus.Free);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Garage)
                .WithMany()
                .HasForeignKey(v => v.GarageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Point>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Point>()
                .Property(p => p.ConnectedPointsIndexes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray()
                );

            modelBuilder.Entity<Point>()
                .Property(p => p.Distances)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray()
                );
        }

        public async Task<Vehicle[]> GetVehiclesAsync()
        {
            var vehicles = await Vehicles.ToArrayAsync();

            for (int i = 0; i < vehicles.Length; i++)
            {
                vehicles[i].Garage = Points.FirstOrDefault(p => p.Id == vehicles[i].GarageId);
            }

            return vehicles;
        }
    }
}