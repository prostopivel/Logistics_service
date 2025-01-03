namespace Logistics_service.Models
{
    public class Route
    {
        private Queue<Point> _points { get; set; }

        public double Distance { get; set; }

        public DateTime? DepartureTime { get; set; }

        public List<Point> Points { get => new List<Point>(_points); }

        public Route(Point[] points, double distance)
        {
            _points = new Queue<Point>(points);
            Distance = distance;
        }


        public void EnqueuePoint(Point point)
        {
            _points.Enqueue(point);
        }

        public Point DequeuePoint()
        {
            return _points.Dequeue();
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
