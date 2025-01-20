namespace Logistics_service.Models.MapModels
{
    public class CustomerMapModel
    {
        public Point[] Points { get; set; }

        public Route[] WaitingOrders { get; set; }

        public Route[] CurrentOrders { get; set; }

        public Vehicle[] Vehicles { get; set; }

        public Point?[] CurrentOrdersPoints { get; set; }

        /// <summary>
        /// Конструктор для инициализации модели карты клиента.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public CustomerMapModel(Point[] points, Route[] waitingOrders,
            Route[] currentOrders, Vehicle[] vehicles, Point?[] currentOrdersPoints)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            WaitingOrders = waitingOrders ?? throw new ArgumentNullException(nameof(waitingOrders));
            CurrentOrders = currentOrders ?? throw new ArgumentNullException(nameof(currentOrders));
            Vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
            CurrentOrdersPoints = currentOrdersPoints ?? throw new ArgumentNullException(nameof(currentOrdersPoints));
        }
    }
}