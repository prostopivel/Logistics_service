using Logistics_service.Models;

namespace Logistics_service.Models.MapModels
{
    public class AdminMapModel
    {
        public Point[] Points { get; set; }

        public Route[] WaitingOrders { get; set; }

        public Route[] CurrentOrders { get; set; }

        public Vehicle[] Vehicles { get; set; }

        public Point?[] CurrentOrdersPoints { get; set; }


        public AdminMapModel(Point[] points, Route[] waitingOrders, Route[] currentOrders, Vehicle[] vehicles, Point?[] currentOrdersPoints)
        {
            Points = points;
            WaitingOrders = waitingOrders;
            CurrentOrders = currentOrders;
            Vehicles = vehicles;
            CurrentOrdersPoints = currentOrdersPoints;
        }
    }
}
