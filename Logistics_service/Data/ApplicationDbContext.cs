using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        public DbSet<ReadyOrder> ReadyOrders { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<Models.Route> Routes { get; set; }
        public DbSet<RoutePoint> RoutePoints { get; set; } // Промежуточная таблица

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация для Vehicle
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Status)
                .HasDefaultValue(VehicleStatus.Free);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Garage)
                .WithMany()
                .HasForeignKey(v => v.GarageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация для Route и Point (связь многие-ко-многим через RoutePoint)
            modelBuilder.Entity<RoutePoint>()
                .HasKey(rp => new { rp.RouteId, rp.PointId });

            modelBuilder.Entity<RoutePoint>()
                .HasOne(rp => rp.Route)
                .WithMany(r => r.RoutePoints)
                .HasForeignKey(rp => rp.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoutePoint>()
                .HasOne(rp => rp.Point)
                .WithMany(p => p.RoutePoints)
                .HasForeignKey(rp => rp.PointId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoutePoint>()
                .Property(rp => rp.OrderIndex)
                .IsRequired();

            // Конфигурация для Point
            modelBuilder.Entity<Point>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Point>()
                .Property(p => p.ConnectedPointsIndexes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray()
                )
                .Metadata.SetValueComparer(new ValueComparer<int[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToArray()));

            modelBuilder.Entity<Point>()
                .Property(p => p.Distances)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray()
                )
                .Metadata.SetValueComparer(new ValueComparer<double[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToArray()));

            // Конфигурация для ReadyOrder
            modelBuilder.Entity<ReadyOrder>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.Vehicle)
                      .WithMany()
                      .HasForeignKey(r => r.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Route)
                      .WithMany()
                      .HasForeignKey(r => r.RouteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.ArrivalTime)
                      .IsRequired();

                entity.Property(r => r.CustomerEmail)
                      .IsRequired()
                      .HasMaxLength(255);
            });

            // Конфигурация для CustomerOrder
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.ArrivalTime)
                      .IsRequired();
            });
        }

        public async Task<Vehicle[]> GetVehiclesAsync()
        {
            var vehicles = await Vehicles.ToArrayAsync();

            for (int i = 0; i < vehicles.Length; i++)
            {
                vehicles[i].Garage = Points.FirstOrDefault(p => p.Id == vehicles[i].GarageId);
                vehicles[i] = new Vehicle(vehicles[i]);
            }

            return vehicles;
        }

        public async Task<Vehicle?> GetVehicleAsync(int id)
        {
            var vehicle = await Vehicles
                .Include(v => v.Garage)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle is null)
            {
                return null;
            }

            var resVehicle = new Vehicle(vehicle);

            return resVehicle;
        }

        public IQueryable<ReadyOrder> GetOrders()
        {
            return ReadyOrders
                    .Include(o => o.Vehicle)
                        .ThenInclude(v => v.Garage)
                    .Include(o => o.Route)
                        .ThenInclude(r => r.RoutePoints)
                            .ThenInclude(rp => rp.Point);
        }

        public ReadyOrder[] GetWaitingOrders(DateTime date)
        {
            var ord = GetOrders();
            return ord
                .Where(o => o.ArrivalTime.Date == date.Date)
                .ToArray();
        }
    }
}