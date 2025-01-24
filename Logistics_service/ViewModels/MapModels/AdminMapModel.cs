using Logistics_service.Models;

namespace Logistics_service.ViewModels.MapModels
{
    public class AdminMapModel
    {
        public PointOutputModel[] Points { get; set; }

        public RouteOutputModel[] WaitingOrders { get; set; }

        public RouteOutputModel[] CurrentOrders { get; set; }

        public VehicleOutputModel[] Vehicles { get; set; }

        public PointOutputModel?[] CurrentOrdersPoints { get; set; }

        /// <summary>
        /// Конструктор для инициализации модели карты администратора.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public AdminMapModel(PointOutputModel[] points, RouteOutputModel[] waitingOrders, 
            RouteOutputModel[] currentOrders, VehicleOutputModel[] vehicles, PointOutputModel?[] currentOrdersPoints)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            WaitingOrders = waitingOrders ?? throw new ArgumentNullException(nameof(waitingOrders));
            CurrentOrders = currentOrders ?? throw new ArgumentNullException(nameof(currentOrders));
            Vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
            CurrentOrdersPoints = currentOrdersPoints ?? throw new ArgumentNullException(nameof(currentOrdersPoints));
        }
    }
}