using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Logistics_service.Models
{
    public class Point
    {
        [Key]
        public int Id { get; set; }

        public int Index { get; set; }

        public string? Name { get; set; }

        public int PosX { get; set; }

        public int PosY { get; set; }

        [JsonPropertyName("ConnectedPointsIndexes")]
        public int[] ConnectedPointsIndexes { get; set; }

        public double[] Distances { get; set; }

        public Point() { }

        public Point(string? name, int posX, int posY)
        {
            Name = name;
            PosX = posX;
            PosY = posY;
        }

        public void AddPoint(Point[] points, params int[] indexes)
        {
            ConnectedPointsIndexes = indexes;

            Distances = new double[indexes.Length];

            for (int i = 0; i < indexes.Length; i++)
            {
                var deltaX = (points[indexes[i]].PosX - PosX) * 0.01 * 4040;
                var deltaY = (points[indexes[i]].PosY - PosY) * 0.01 * 1906.7;

                var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                Distances[i] = length;
            }
        }

        public void UpdateLocation(string newLocation)
        {
            // Логика обновления местоположения точки
        }
    }
}
