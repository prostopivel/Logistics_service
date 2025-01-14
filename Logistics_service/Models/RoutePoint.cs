using System.Text.Json.Serialization;

namespace Logistics_service.Models
{
    public class RoutePoint
    {
        public int RouteId { get; set; }
        [JsonIgnore]
        public Route Route { get; set; }

        public int PointId { get; set; }
        [JsonIgnore]
        public Point Point { get; set; }

        public int OrderIndex { get; set; }

        public RoutePoint() { }
        public RoutePoint(int PointId, int OrderIndex)
        {
            this.OrderIndex = OrderIndex;
            this.PointId = PointId;
        }
    }
}
