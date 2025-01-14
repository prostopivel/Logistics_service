using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models
{
    public class Route
    {
        [Key]
        public int? Id { get; set; }

        [NotMapped]
        private Queue<Point> _points { get; set; }

        public double Distance { get; set; }

        public DateTime? DepartureTime { get; set; }

        [NotMapped]
        public string? CustomerEmail { get; set; }

        [NotMapped]
        public List<Point> Points => new List<Point>(_points ?? Enumerable.Empty<Point>());

        public ICollection<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();

        public Route() { }

        public Route(Point[] points, double distance)
        {
            RoutePoints = new List<RoutePoint>();

            int i = 0;
            foreach (var item in points)
            {
                RoutePoints.Add(new RoutePoint(item.Id, i++));
            }

            Distance = distance;
        }

        public Route(Route route)
        {
            _points = new Queue<Point>();

            foreach (var item in route.RoutePoints.OrderBy(r => r.OrderIndex))
            {
                _points.Enqueue(item.Point);
            }

            Distance = route.Distance;
            DepartureTime = route.DepartureTime;
            CustomerEmail = route.CustomerEmail;
            Id = route.Id;
            RoutePoints = route.RoutePoints;
        }

        public void EnqueuePoint(Point point)
        {
            _points.Enqueue(point);
        }

        public Point? DequeuePoint()
        {
            if (_points.Count > 0)
            {
                return _points.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void AddPoints(Point[] points)
        {
            if (points is null || points.Length == 0)
            {
                return;
            }

            var newQueue = new Queue<Point>(points);
            foreach (var point in _points)
            {
                newQueue.Enqueue(point);
            }

            _points = newQueue;
        }
    }
}
