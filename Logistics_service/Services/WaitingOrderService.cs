using Logistics_service.Data;
using Logistics_service.Models.Orders;
using System.Collections.Concurrent;

namespace Logistics_service.Services
{
    public class WaitingOrderService : BackgroundService
    {
        private readonly VehicleService _vehicleService;
        private readonly ConcurrentDictionary<DateTime, ReadyOrder> _orders;
        private readonly ConcurrentDictionary<DateTime, ReadyOrder> _currentOrders;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private SortedDictionary<DateTime, ReadyOrder> _sortedOrders;

        public SortedDictionary<DateTime, ReadyOrder> Orders => new SortedDictionary<DateTime, ReadyOrder>(_sortedOrders);
        public Dictionary<DateTime, ReadyOrder> CurrentOrders => new Dictionary<DateTime, ReadyOrder>(_currentOrders);

        public WaitingOrderService(ILogger<WaitingOrderService> logger, VehicleService vehicleService)
        {
            _orders = new ConcurrentDictionary<DateTime, ReadyOrder>();
            _currentOrders = new ConcurrentDictionary<DateTime, ReadyOrder>();
            _sortedOrders = new SortedDictionary<DateTime, ReadyOrder>();
            _vehicleService = vehicleService;
        }

        public async Task AddOrder(DateTime time, ReadyOrder order, ApplicationDbContext context)
        {
            _vehicleService.FreeVehicles.Add(order.Vehicle);

            _orders.TryAdd(time, order);

            await _semaphore.WaitAsync();
            try
            {
                _sortedOrders.TryAdd(time, order);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                await _semaphore.WaitAsync();
                try
                {
                    if (_sortedOrders.Count > 0)
                    {
                        var firstTask = _sortedOrders.First();

                        if (firstTask.Key <= now)
                        {
                            await Console.Out.WriteLineAsync($"Заказ с Id {firstTask.Value.Id} начался в {firstTask.Key}");
                            await _vehicleService.AddVehicle(firstTask.Value.Vehicle);
                            _orders.TryRemove(firstTask.Key, out var order);
                            _sortedOrders.Remove(firstTask.Key);
                            if (order is not null)
                            {
                                _currentOrders.TryAdd(firstTask.Key, order);
                            }
                        }
                    }
                }
                finally
                {
                    _semaphore.Release();
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
                        }
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}