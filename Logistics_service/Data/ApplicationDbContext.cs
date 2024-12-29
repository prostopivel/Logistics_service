using Logistics_service.Models.Users;
using Logistics_service.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Logistics_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Point> Points { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
    }
}