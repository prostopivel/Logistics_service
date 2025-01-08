using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Logistics_service.Models
{
    public class Point : ICloneable
    {
        public const double ConvertX = 40.40;
        public const double ConvertY = 19.067;

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
                var deltaX = (points[indexes[i]].PosX - PosX) * ConvertX;
                var deltaY = (points[indexes[i]].PosY - PosY) * ConvertY;

                var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                Distances[i] = length;
            }
        }

        public void UpdateLocation(string newLocation)
        {
            // Логика обновления местоположения точки
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
