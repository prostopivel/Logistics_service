using Logistics_service.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logistics_service.Static
{
    public static class DijkstraAlgorithm
    {
        public static Tuple<Point[], double> FindShortestPath(Point[] points, Point startPoint, Point endPoint)
        {
            var distances = new Dictionary<Point, double>();
            var previous = new Dictionary<Point, Point>();
            var priorityQueue = new SortedSet<(double Distance, Point Point)>(new DistancePointComparer());

            foreach (var point in points)
            {
                distances[point] = double.PositiveInfinity;
                previous[point] = null;
            }
            distances[startPoint] = 0;
            priorityQueue.Add((0, startPoint));

            while (priorityQueue.Count > 0)
            {
                var (currentDistance, currentPoint) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (currentPoint == endPoint)
                    break;

                for (int i = 0; i < currentPoint.ConnectedPointsIndexes.Length; i++)
                {
                    int neighborIndex = currentPoint.ConnectedPointsIndexes[i];
                    Point neighbor = points.First(p => p.Index == neighborIndex);
                    double edgeDistance = currentPoint.Distances[i];
                    double newDistance = currentDistance + edgeDistance;

                    if (newDistance < distances[neighbor])
                    {
                        priorityQueue.Remove((distances[neighbor], neighbor));
                        distances[neighbor] = newDistance;
                        previous[neighbor] = currentPoint;
                        priorityQueue.Add((newDistance, neighbor));
                    }
                }
            }

            var path = new List<Point>();
            for (var point = endPoint; point != null; point = previous[point])
            {
                path.Add(point);
            }
            path.Reverse();

            return new Tuple<Point[], double>(path.ToArray(), distances[endPoint]);
        }
    }

    public class DistancePointComparer : IComparer<(double Distance, Point Point)>
    {
        public int Compare((double Distance, Point Point) x, (double Distance, Point Point) y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }
}