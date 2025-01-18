using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using System.Collections.Concurrent;

namespace Logistics_service.Services
{
    public class WaitingOrderService : BackgroundService
    {
        private DateTime UpdateOrdersTime;

        private readonly IServiceProvider _serviceProvider;
        private readonly VehicleService _vehicleService;
        private readonly ConcurrentDictionary<int, ReadyOrder> _currentorders;

        public ConcurrentDictionary<DateTime, int> Orders { get; init; }

        public ConcurrentDictionary<int, ReadyOrder> GetCurrentOrders() =>
            new ConcurrentDictionary<int, ReadyOrder>(_currentorders);

        public WaitingOrderService(ILogger<WaitingOrderService> logger,
            VehicleService vehicleService, IServiceProvider serviceProvider)
        {
            Orders = new ConcurrentDictionary<DateTime, int>();
            _currentorders = new ConcurrentDictionary<int, ReadyOrder>();
            _vehicleService = vehicleService;
            _serviceProvider = serviceProvider;
            var now = DateTime.Now;
            UpdateOrdersTime = new DateTime(now.Year, now.Month, now.Day, 7, 55, 0);
        }

        public async Task AddOrder(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var order = context
                .GetOrders()
                .FirstOrDefault(o => o.Id == id);

            if (order is null)
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

            await _vehicleService.AddVehicle(order.Vehicle);
            _currentorders.TryAdd(order.Id ?? 0, order);

            Console.WriteLine($"Заказ с Id {order.Id} начался в {DateTime.Now}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now >= UpdateOrdersTime)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var orders = context
                        .GetOrders()
                        .Where(o => o.ArrivalTime.Date <= UpdateOrdersTime.Date)
                        .ToArray();

                    foreach (var order in orders)
                    {
                        if (order.Status == ReadyOrderStatus.Accepted || order.Status == ReadyOrderStatus.Running)
                        {
                            order.Status = ReadyOrderStatus.Running;
                            if (order.Route is not null && order.Route.DepartureTime is not null && order.Id is not null)
                                Orders.TryAdd((DateTime)order.Route.DepartureTime, (int)order.Id);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    UpdateOrdersTime = new DateTime(now.Year, now.Month, now.Day + 1, 8, 0, 0);
                }

                var currentorder = Orders.FirstOrDefault(o => o.Key <= now);

                if (!currentorder.Equals(default(KeyValuePair<DateTime, int>)))
                {
                    await AddOrder(currentorder.Value);

                    Orders.Remove(currentorder.Key, out var order);
                }

                for (int i = 0; i < _vehicleService.Vehicles.Count; i++)
                {
                    if (!_vehicleService.Vehicles[i].UpdateLocation(2))
                    {
                        var ord = _currentorders.FirstOrDefault(o => o.Value.VehicleId == _vehicleService.Vehicles[i].Id);

                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        if (context.ReadyOrders.Any(o => o.Id == ord.Value.Id))
                        {
                            context.ReadyOrders.First(o => o.Id == ord.Value.Id).Status = ReadyOrderStatus.Completed;
                            await context.SaveChangesAsync(stoppingToken);
                        }

                        _currentorders.TryRemove(ord);
                        await _vehicleService.DeleteVehicle(_vehicleService.Vehicles[i]);

                        await Console.Out.WriteLineAsync($"Заказ с Id {ord.Value.Id} закончился в {DateTime.Now}");
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}