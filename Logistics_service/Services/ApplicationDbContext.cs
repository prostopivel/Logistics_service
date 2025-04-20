using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Models.Statistic;
using Logistics_service.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Logistics_service.Services
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

        public DbSet<UserMark> UserMarkes { get; set; }

        public DbSet<UserOrderStatistic> UserOrdersStatistic { get; set; }

        public DbSet<EntryInfo> UserEntrysStatistic { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureVehicle(modelBuilder);
            ConfigureRouteAndPoint(modelBuilder);
            ConfigurePoint(modelBuilder);
            ConfigureReadyOrder(modelBuilder);
            ConfigureCustomerOrder(modelBuilder);
        }

        private void ConfigureVehicle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Status)
                .HasDefaultValue(VehicleStatus.Free);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Garage)
                .WithMany()
                .HasForeignKey(v => v.GarageId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureRouteAndPoint(ModelBuilder modelBuilder)
        {
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
        }

        private void ConfigurePoint(ModelBuilder modelBuilder)
        {
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
        }

        private void ConfigureReadyOrder(ModelBuilder modelBuilder)
        {
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
        }

        private void ConfigureCustomerOrder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.ArrivalTime)
                      .IsRequired();
            });
        }

        public async Task<Vehicle[]> GetVehiclesAsync()
        {
            var vehicles = await Vehicles
                .Include(v => v.Garage)
                .AsNoTracking()
                .ToArrayAsync();

            return vehicles.Select(v => new Vehicle(v)).ToArray();
        }

        public async Task<Vehicle?> GetVehicleAsync(int id)
        {
            var vehicle = await Vehicles
                .Include(v => v.Garage)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            return vehicle != null ? new Vehicle(vehicle) : null;
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
            return GetOrders()
                .Where(o => o.ArrivalTime.Date == date.Date)
                .AsNoTracking()
                .ToArray();
        }
    }
}