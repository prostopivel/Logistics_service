using Logistics_service.Models.Users;
using System.Text.Json.Serialization;

namespace Logistics_service.Models
{
    public class Marker
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public int PointId { get; set; }

        [JsonIgnore]
        public Point Point { get; set; }

        public Marker() { }

        public Marker(User? user, Point? point)
        {
            if (user == null || point == null)
                return;

            UserId = user.Id;
            User = user;
            PointId = point.Id;
            Point = point;
        }
    }
}
