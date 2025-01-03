using Logistics_service.Data;
using Logistics_service.Models.Orders;

namespace Logistics_service.Services
{
    public class WaitingOrderService : BackgroundService
    {
        private readonly VehicleService _vehicleService;
        private readonly SortedDictionary<DateTime, ReadyOrder> _orders;
        private readonly ILogger<WaitingOrderService> _logger;
        private readonly SemaphoreSlim _semaphore;

        public SortedDictionary<DateTime, ReadyOrder> Orders => new SortedDictionary<DateTime, ReadyOrder>(_orders);

        public WaitingOrderService(ILogger<WaitingOrderService> logger, IServiceProvider serviceProvider, VehicleService vehicleService)
        {
            _orders = new SortedDictionary<DateTime, ReadyOrder>();
            _semaphore = new SemaphoreSlim(1, 1);
            _logger = logger;
            _vehicleService = vehicleService;
        }

        public async Task AddOrder(DateTime time, ReadyOrder order, ApplicationDbContext context)
        {
            await _semaphore.WaitAsync();

            try
            {
                _vehicleService.FreeVehicles.Add(order.Vehicle);
                _orders.Add(time, order);
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
                    if (Orders.Count > 0)
                    {
                        var firstTask = Orders.First();

                        if (firstTask.Key <= now)
                        {
                            await _vehicleService.AddVehicle(firstTask.Value.Vehicle);

                            _orders.Remove(firstTask.Key);
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
                        _vehicleService.Vehicles[i].UpdateLocation(1);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}