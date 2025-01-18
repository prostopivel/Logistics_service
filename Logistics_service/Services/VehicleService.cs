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

        public async Task AddVehicle(Vehicle vehicle)
        {
            if (vehicle is not null && vehicle.Id is not null)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var existingVehicle = await context.GetVehicleAsync((int)vehicle.Id);

                if (existingVehicle is not null)
                {
                    existingVehicle.Status = VehicleStatus.InUse;
                    await context.SaveChangesAsync();

                    vehicle.Status = VehicleStatus.InUse;
                    vehicle.SetRoute(await context.Points.ToArrayAsync());

                    Vehicles.Add(vehicle);
                }
            }
        }

        public async Task DeleteVehicle(Vehicle vehicle)
        {
            if (vehicle.Id is not null && vehicle.Id > 0)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var existingVehicle = await context.GetVehicleAsync((int)vehicle.Id);

                    if (existingVehicle is not null)
                    {
                        existingVehicle.Status = VehicleStatus.Free;
                        await context.SaveChangesAsync();

                        Vehicles.Remove(vehicle);
                    }
                }
            }
        }

        public Vehicle[] GetAllVehicles(Vehicle[] vehicles)
        {
            int i = 0;
            foreach (var item in vehicles)
            {
                var vehicle = Vehicles.FirstOrDefault(v => v.Id == item.Id);

                if (vehicle is not null)
                {
                    vehicles[i] = vehicle;
                }

                i++;
            }

            return vehicles;
        }

        public Vehicle? GetVehicleById(int id)
        {
            var vehicle = Vehicles.FirstOrDefault(v => v.Id == id);

            if (vehicle is not null)
            {
                return vehicle;
            }
            else
            {
                return null;
            }
        }
    }
}