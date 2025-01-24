using Logistics_service.Models;

namespace Logistics_service.ViewModels
{
    public class VehicleOutputModel
    {
        public int Id { get; set; }

        public int Speed { get; set; }

        public PointOutputModel Garage { get; set; }

        public VehicleStatus Status { get; set; }

        public double PosX { get; set; }

        public double PosY { get; set; }

        public RouteOutputModel CurrentRoute { get; private set; }

        public SortedDictionary<DateTime, RouteOutputModel> Routes { get; set; }
    }
}
