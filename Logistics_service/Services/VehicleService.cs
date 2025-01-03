﻿using Logistics_service.Data;
using Logistics_service.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics_service.Services
{
    public class VehicleService
    {
        private readonly IServiceProvider _serviceProvider;
        public List<Vehicle> Vehicles { get; init; }
        public List<Vehicle> FreeVehicles { get; init; }

        public VehicleService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Vehicles = new List<Vehicle>();
            FreeVehicles = new List<Vehicle>();
        }

        public async Task AddVehicle(Vehicle vehicle)
        {
            if (vehicle.Id is not null && vehicle.Id > 0)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var existingVehicle = await context.Vehicles.FindAsync((int)vehicle.Id);

                    if (existingVehicle is not null)
                    {
                        existingVehicle.Status = VehicleStatus.InUse;
                        vehicle.SetRoute(await context.Points.ToArrayAsync());
                        Vehicles.Add(vehicle);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public Vehicle[] GetAllVehicles(Vehicle[] vehicles)
        {
            int i = 0;
            foreach (var item in vehicles)
            {
                var freeVehicle = FreeVehicles.FirstOrDefault(v => v.Id == item.Id);
                var vehicle = Vehicles.FirstOrDefault(v => v.Id == item.Id);

                if (freeVehicle is not null)
                {
                    vehicles[i] = freeVehicle;
                }
                else if (vehicle is not null)
                {
                    vehicles[i] = vehicle;
                }

                i++;
            }

            return vehicles;
        }

        public Vehicle? GetVehicleById(int id)
        {
            var freeVehicle = FreeVehicles.FirstOrDefault(v => v.Id == id);
            var vehicle = Vehicles.FirstOrDefault(v => v.Id == id);

            if (freeVehicle is not null)
            {
                return freeVehicle;
            }
            else if (vehicle is not null)
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