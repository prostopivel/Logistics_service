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
        public List<Point> Points => new List<Point>(_points);

        public List<Point> DbPoints { get; set; }

        public Route() { }

        public Route(Point[] points, double distance)
        {
            _points = new Queue<Point>(points);
            Distance = distance;
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
