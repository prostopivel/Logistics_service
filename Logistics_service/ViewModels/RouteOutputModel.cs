using Logistics_service.Models;

namespace Logistics_service.ViewModels
{
    public class RouteOutputModel
    {
        public double Distance { get; set; }

        public DateTime DepartureTime { get; set; }

        public PointOutputModel[] Points { get; set; }

        public PointOutputModel[] RoutePoints { get; set; }
    }
}