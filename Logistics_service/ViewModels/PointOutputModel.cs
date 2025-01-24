using System.Text.Json.Serialization;

namespace Logistics_service.ViewModels
{
    public class PointOutputModel
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public int PosX { get; set; }

        public int PosY { get; set; }

        [JsonPropertyName("ConnectedPointsIndexes")]
        public int[] ConnectedPointsIndexes { get; set; }

        public double[] Distances { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is PointOutputModel point && Index == point.Index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PosX, Index, PosY);
        }
    }
}
