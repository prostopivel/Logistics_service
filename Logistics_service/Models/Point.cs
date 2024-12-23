using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Point
    {
        public int Index { get; set; }

        public string? Name { get; set; }

        public int PosX { get; set; }

        public int PosY { get; set; }

        public int[]? ConnectedPointsInsexes { get; set; }

        public Point() { }

        public Point(string? name, int posX, int posY)
        {
            Name = name;
            PosX = posX;
            PosY = posY;
        }

        public void AddPoint(params int[] indexes)
        {
            ConnectedPointsInsexes = indexes;
        }

        public void UpdateLocation(string newLocation)
        {
            // Логика обновления местоположения точки
        }
    }
}
