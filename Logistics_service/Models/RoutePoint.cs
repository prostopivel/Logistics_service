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

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public RoutePoint() { }

        /// <summary>
        /// Конструктор для инициализации точки маршрута.
        /// </summary>
        public RoutePoint(int pointId, int orderIndex)
        {
            PointId = pointId;
            OrderIndex = orderIndex;
        }
    }
}