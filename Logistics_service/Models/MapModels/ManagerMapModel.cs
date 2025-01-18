using Logistics_service.Models.Orders;

namespace Logistics_service.Models.MapModels
{
    public class ManagerMapModel
    {
        public Point[] Points { get; set; }

        public Route[] WaitingOrders { get; set; }

        public Route[] CurrentOrders { get; set; }

        public Point[]? Route { get; set; }

        public double? Distanse { get; set; }

        public Point?[] CurrentOrdersPoints { get; set; }

        public Vehicle[] Vehicles { get; set; }

        public ManagerMapModel(Point[] points,Route[] waitingOrders, 
            Route[] currentOrders, Point[]? routes, double? distanse, 
            Point?[] currentOrdersPoints, Vehicle[] vehicles)
        {
            Points = points;
            WaitingOrders = waitingOrders;
            CurrentOrders = currentOrders;
            Route = routes;
            Distanse = distanse;
            CurrentOrdersPoints = currentOrdersPoints;
            Vehicles = vehicles;
        }
    }
}
