using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models
{
    public class Route
    {
        [Key]
        public int Id { get; set; }

        [NotMapped]
        private Queue<Point> _points { get; set; } = new Queue<Point>();

        public double Distance { get; set; }

        public DateTime DepartureTime { get; set; }

        [NotMapped]
        public string CustomerEmail { get; set; }

        [NotMapped]
        public List<Point> Points => _points.ToList();

        public ICollection<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Route() { }

        /// <summary>
        /// Конструктор для инициализации маршрута с массивом точек и расстоянием.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Route(Point[] points, double distance)
        {
            if (points is null)
            {
                throw new ArgumentNullException(nameof(points), "Массив точек не может быть null.");
            }

            RoutePoints = new List<RoutePoint>();
            for (int i = 0; i < points.Length; i++)
            {
                RoutePoints.Add(new RoutePoint(points[i].Id, i));
            }

            Distance = distance;
        }

        /// <summary>
        /// Конструктор для копирования маршрута.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Route(Route route)
        {
            if (route is null)
            {
                throw new ArgumentNullException(nameof(route), "Маршрут не может быть null.");
            }

            _points = new Queue<Point>(route.RoutePoints
                .OrderBy(r => r.OrderIndex)
                .Select(r => r.Point));

            Distance = route.Distance;
            DepartureTime = route.DepartureTime;
            CustomerEmail = route.CustomerEmail;
            Id = route.Id;
            RoutePoints = route.RoutePoints;
        }

        /// <summary>
        /// Добавляет точку в очередь маршрута.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void EnqueuePoint(Point point)
        {
            if (point is null)
            {
                throw new ArgumentNullException(nameof(point), "Точка не может быть null.");
            }

            _points.Enqueue(point);
        }

        /// <summary>
        /// Удаляет и возвращает точку из очереди маршрута.
        /// </summary>
        /// <returns>Точка маршрута или null, если очередь пуста.</returns>
        public Point? DequeuePoint()
        {
            return _points.Count > 0 ? _points.Dequeue() : null;
        }

        /// <summary>
        /// Добавляет массив точек в начало очереди маршрута.
        /// </summary>
        /// <param name="points">Массив точек для добавления.</param>
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