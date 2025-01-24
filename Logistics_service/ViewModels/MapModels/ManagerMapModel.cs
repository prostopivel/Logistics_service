using Logistics_service.Models;

namespace Logistics_service.ViewModels.MapModels
{
    public class ManagerMapModel
    {
        public PointOutputModel[] Points { get; set; }

        public RouteOutputModel[] WaitingOrders { get; set; }

        public RouteOutputModel[] CurrentOrders { get; set; }

        public PointOutputModel[]? Route { get; set; }

        public double? Distance { get; set; }

        public PointOutputModel?[] CurrentOrdersPoints { get; set; }

        public VehicleOutputModel[] Vehicles { get; set; }

        /// <summary>
        /// Конструктор для инициализации модели карты менеджера.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public ManagerMapModel(PointOutputModel[] points, RouteOutputModel[] waitingOrders,
            RouteOutputModel[] currentOrders, PointOutputModel[]? routes, double? distance,
            PointOutputModel?[] currentOrdersPoints, VehicleOutputModel[] vehicles)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            WaitingOrders = waitingOrders ?? throw new ArgumentNullException(nameof(waitingOrders));
            CurrentOrders = currentOrders ?? throw new ArgumentNullException(nameof(currentOrders));
            Route = routes;
            Distance = distance;
            CurrentOrdersPoints = currentOrdersPoints ?? throw new ArgumentNullException(nameof(currentOrdersPoints));
            Vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
        }
    }
}