using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Logistics_service.Services
{
    public class WaitingOrderService : BackgroundService
    {
        private DateTime _updateOrdersTime;
        private readonly IServiceProvider _serviceProvider;
        private readonly VehicleService _vehicleService;
        private readonly ConcurrentDictionary<int, ReadyOrder> _currentOrders;

        public ConcurrentDictionary<DateTime, int> Orders { get; init; }

        public WaitingOrderService(ILogger<WaitingOrderService> logger,
            VehicleService vehicleService, IServiceProvider serviceProvider)
        {
            Orders = new ConcurrentDictionary<DateTime, int>();
            _currentOrders = new ConcurrentDictionary<int, ReadyOrder>();
            _vehicleService = vehicleService;
            _serviceProvider = serviceProvider;
            var now = DateTime.Now;
            _updateOrdersTime = new DateTime(now.Year, now.Month, now.Day, 7, 55, 0);
        }

        public ConcurrentDictionary<int, ReadyOrder> GetCurrentOrders() =>
            new ConcurrentDictionary<int, ReadyOrder>(_currentOrders);

        public async Task AddOrderAsync(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var order = await context.GetOrders()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return;
            }

            order.Route = new Models.Route(order.Route)
            {
                CustomerEmail = order.CustomerEmail,
            };
            order.Vehicle = new Vehicle(order.Vehicle);

            if (!order.Vehicle.SetOrder(order))
            {
                return;
            }

            await _vehicleService.AddVehicleAsync(order.Vehicle);
            _currentOrders.TryAdd(order.Id ?? 0, order);

            Console.WriteLine($"Заказ с Id {order.Id} начался в {DateTime.Now}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now >= _updateOrdersTime)
                {
                    await UpdateOrdersAsync(stoppingToken);
                    _updateOrdersTime = new DateTime(now.Year, now.Month, now.Day + 1, 8, 0, 0);
                }

                await ProcessCurrentOrdersAsync(stoppingToken);
                await UpdateVehicleLocationsAsync(stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task UpdateOrdersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var orders = await context.GetOrders()
                .Where(o => o.ArrivalTime.Date <= _updateOrdersTime.Date)
                .ToArrayAsync(stoppingToken);

            foreach (var order in orders)
            {
                if (order.Status == ReadyOrderStatus.Accepted || order.Status == ReadyOrderStatus.Running)
                {
                    order.Status = ReadyOrderStatus.Running;
                    if (order.Route != null && order.Route.DepartureTime != null && order.Id != null)
                    {
                        Orders.TryAdd((DateTime)order.Route.DepartureTime, (int)order.Id);
                    }
                }
            }

            await context.SaveChangesAsync(stoppingToken);
        }

        private async Task ProcessCurrentOrdersAsync(CancellationToken stoppingToken)
        {
            var currentOrder = Orders.FirstOrDefault(o => o.Key <= DateTime.Now);

            if (!currentOrder.Equals(default(KeyValuePair<DateTime, int>)))
            {
                await AddOrderAsync(currentOrder.Value);
                Orders.TryRemove(currentOrder.Key, out _);
            }
        }

        private async Task UpdateVehicleLocationsAsync(CancellationToken stoppingToken)
        {
            for (int i = 0; i < _vehicleService.Vehicles.Count; i++)
            {
                var vehicle = _vehicleService.Vehicles[i];
                if (!vehicle.UpdateLocation(2))
                {
                    var order = _currentOrders.FirstOrDefault(o => o.Value.VehicleId == vehicle.Id);

                    if (order.Value != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var dbOrder = await context.ReadyOrders.FirstOrDefaultAsync(o => o.Id == order.Value.Id, stoppingToken);

                        if (dbOrder != null)
                        {
                            dbOrder.Status = ReadyOrderStatus.Completed;
                            await context.SaveChangesAsync(stoppingToken);
                        }

                        _currentOrders.TryRemove(order);
                        await _vehicleService.DeleteVehicleAsync(vehicle);

                        Console.WriteLine($"Заказ с Id {order.Value.Id} закончился в {DateTime.Now}");
                    }
                }
            }
        }
    }
}