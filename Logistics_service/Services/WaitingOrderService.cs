using Logistics_service.Data;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using static System.Formats.Asn1.AsnWriter;

namespace Logistics_service.Services
{
    public class WaitingOrderService : BackgroundService
    {
        private DateTime UpdateOrdersTime;

        private ConcurrentDictionary<DateTime, ReadyOrder> _orders;
        private readonly IServiceProvider _serviceProvider;
        private readonly VehicleService _vehicleService;
        private readonly ConcurrentDictionary<DateTime, ReadyOrder> _currentOrders;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Dictionary<DateTime, ReadyOrder> GetOrders() =>
            new Dictionary<DateTime, ReadyOrder>(_orders);
        public Dictionary<DateTime, ReadyOrder> GetCurrentOrders() =>
            new Dictionary<DateTime, ReadyOrder>(_currentOrders);


        public WaitingOrderService(ILogger<WaitingOrderService> logger,
            VehicleService vehicleService, IServiceProvider serviceProvider)
        {
            _orders = new ConcurrentDictionary<DateTime, ReadyOrder>();
            _currentOrders = new ConcurrentDictionary<DateTime, ReadyOrder>();
            _vehicleService = vehicleService;
            _serviceProvider = serviceProvider;
            var now = DateTime.Now;
            UpdateOrdersTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
        }

        public void AddOrder(DateTime time, ReadyOrder order)
        {
            order.Route = new Models.Route(order.Route)
            {
                CustomerEmail = order.CustomerEmail,
            };
            order.Vehicle = new Vehicle(order.Vehicle);

            if (!order.Vehicle.SetOrder(order))
            {
                return;
            }

            _vehicleService.FreeVehicles.Add(order.Vehicle);

            _orders.TryAdd(time, order);
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
                        if (order.Status == ReadyOrderStatus.Accepted)
                        {
                            order.Status = ReadyOrderStatus.Running;

                            var temp = (ReadyOrder)order.Clone();

                            AddOrder((DateTime)temp.Route.DepartureTime!, temp);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    UpdateOrdersTime = new DateTime(now.Year, now.Month, now.Day + 1, 8, 0, 0);
                }

                if (!_orders.IsEmpty)
                {
                    var firstTask = _orders.OrderBy(pair => pair.Key).FirstOrDefault();

                    if (firstTask.Key <= now)
                    {
                        await Console.Out.WriteLineAsync($"Заказ с Id {firstTask.Value.Id} начался в {firstTask.Key}");

                        if (_orders.TryRemove(firstTask.Key, out var order))
                        {
                            await _vehicleService.AddVehicle(order.Vehicle);
                            _currentOrders.TryAdd(firstTask.Key, order);
                        }
                    }
                }

                if (_vehicleService.Vehicles.Count > 0)
                {
                    for (int i = 0; i < _vehicleService.Vehicles.Count; i++)
                    {
                        if (!_vehicleService.Vehicles[i].UpdateLocation(2))
                        {
                            var ord = _currentOrders.FirstOrDefault(o => o.Value.VehicleId == _vehicleService.Vehicles[i].Id);
                            _currentOrders.TryRemove(ord);
                            await _vehicleService.DeleteVehicle(_vehicleService.Vehicles[i]);
                            
                            using var scope = _serviceProvider.CreateScope();
                            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            if (context.ReadyOrders.Any(o => o.Id == ord.Value.Id))
                            {
                                context.ReadyOrders.First(o => o.Id == ord.Value.Id).Status = ReadyOrderStatus.Completed;
                                await context.SaveChangesAsync();
                            }
                            await Console.Out.WriteLineAsync($"Заказ с Id {ord.Value.Id} закончился в {ord.Key}");
                        }
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}