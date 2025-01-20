namespace Logistics_service.Models.MapModels
{
    public class ManagerMapModel
    {
        public Point[] Points { get; set; }

        public Route[] WaitingOrders { get; set; }

        public Route[] CurrentOrders { get; set; }

        public Point[]? Route { get; set; }

        public double? Distance { get; set; }

        public Point?[] CurrentOrdersPoints { get; set; }

        public Vehicle[] Vehicles { get; set; }

        /// <summary>
        /// Конструктор для инициализации модели карты менеджера.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public ManagerMapModel(Point[] points, Route[] waitingOrders,
            Route[] currentOrders, Point[]? routes, double? distance,
            Point?[] currentOrdersPoints, Vehicle[] vehicles)
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