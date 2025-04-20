using System.Text.Json.Serialization;

namespace Logistics_service.Models.Statistic
{
    public class UserOrderStatistic
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int PointId { get; set; }

        [JsonIgnore]
        public Point Point { get; set; }

        public UserOrderStatistic() { }

        public UserOrderStatistic(DateTime date, Point? point)
        {
            if (point == null)
                return;

            Date = date;
            PointId = point.Id;
            Point = point;
        }
    }
}
