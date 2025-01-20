using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics_service.Services
{
    public class VehicleService
    {
        private readonly IServiceProvider _serviceProvider;
        public List<Vehicle> Vehicles { get; init; }

        public VehicleService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Vehicles = new List<Vehicle>();
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            if (vehicle?.Id == null)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingVehicle = await context.GetVehicleAsync((int)vehicle.Id);
            if (existingVehicle == null)
            {
                return;
            }

            existingVehicle.Status = VehicleStatus.InUse;
            await context.SaveChangesAsync();

            vehicle.Status = VehicleStatus.InUse;
            vehicle.SetRoute(await context.Points.ToArrayAsync());

            Vehicles.Add(vehicle);
        }

        public async Task DeleteVehicleAsync(Vehicle vehicle)
        {
            if (vehicle?.Id == null || vehicle.Id <= 0)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingVehicle = await context.GetVehicleAsync((int)vehicle.Id);
            if (existingVehicle == null)
            {
                return;
            }

            existingVehicle.Status = VehicleStatus.Free;
            await context.SaveChangesAsync();

            Vehicles.Remove(vehicle);
        }

        public Vehicle[] GetAllVehicles(Vehicle[] vehicles)
        {
            for (int i = 0; i < vehicles.Length; i++)
            {
                var vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicles[i].Id);
                if (vehicle != null)
                {
                    vehicles[i] = vehicle;
                }
            }

            return vehicles;
        }

        public Vehicle? GetVehicleById(int id)
        {
            return Vehicles.FirstOrDefault(v => v.Id == id);
        }
    }
}