using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Logistics_service.Models
{
    public class Point : ICloneable
    {
        /// <summary>
        /// Коэффициент преобразования для координаты X.
        /// </summary>
        public const double ConvertX = 40.40;

        /// <summary>
        /// Коэффициент преобразования для координаты Y.
        /// </summary>
        public const double ConvertY = 19.067;

        [Key]
        public int Id { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public int PosX { get; set; }

        public int PosY { get; set; }

        [JsonPropertyName("ConnectedPointsIndexes")]
        public int[] ConnectedPointsIndexes { get; set; } = Array.Empty<int>();

        public double[] Distances { get; set; } = Array.Empty<double>();

        public virtual ICollection<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Point() { }

        /// <summary>
        /// Конструктор для инициализации точки с именем и координатами.
        /// </summary>
        public Point(string name, int posX, int posY)
        {
            Name = name;
            PosX = posX;
            PosY = posY;
        }

        /// <summary>
        /// Добавляет связанные точки и вычисляет расстояния до них.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddPoint(Point[] points, params int[] indexes)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points), "Массив точек не может быть null.");
            }

            ConnectedPointsIndexes = indexes;
            Distances = new double[indexes.Length];

            for (int i = 0; i < indexes.Length; i++)
            {
                var deltaX = (points[indexes[i]].PosX - PosX) * ConvertX;
                var deltaY = (points[indexes[i]].PosY - PosY) * ConvertY;

                var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                Distances[i] = length;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Point point && Id == point.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Index, Name);
        }

        public object Clone()
        {
            return new Point
            {
                Id = Id,
                Index = Index,
                Name = Name,
                PosX = PosX,
                PosY = PosY,
                ConnectedPointsIndexes = (int[])ConnectedPointsIndexes.Clone(),
                Distances = (double[])Distances.Clone()
            };
        }
    }
}