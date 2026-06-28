using System.Numerics;

namespace GameKit.AStar;

public enum PathResult
{
    Found,
    NotFound,
    ExpansionLimitExceeded
}

public interface IDistanceHeuristicProvider<TPoint>
{
    float GetCost(TPoint start, TPoint destination);
}

internal class ChebyshevDistanceHeuristicProvider : IDistanceHeuristicProvider<Vector2>
{
    public float GetCost(Vector2 start, Vector2 destination)
    {
        return Math.Max(Math.Abs(destination.X - start.X), Math.Abs(start.Y - destination.Y));
    }
}

public interface IPathFinderMap<TPoint>
{
    void ExpandPosition(TPoint origin, ICollection<(TPoint Position, float Cost)> neighbors);
}

public readonly record struct AreaEdge<TPoint>(TPoint Inside, TPoint Outside)
{
    public void Deconstruct(out TPoint inside, out TPoint outside)
    {
        inside = Inside;
        outside = Outside;
    }
}

public record AreaResult<TPoint>(Dictionary<TPoint, TPoint> CameFrom, Dictionary<TPoint, float> Costs,
    List<AreaEdge<TPoint>> Edges) where TPoint : struct;

public interface IPathFinder<TPoint> where TPoint : struct
{
    AreaResult<TPoint> ExpandArea(TPoint start, float maxCost);
}

public class PathFinder<TPoint> : IPathFinder<TPoint> where TPoint : struct
{
    private readonly IDistanceHeuristicProvider<TPoint> _distanceHeuristicProvider;
    private readonly int _expansionLimit;
    private readonly IPathFinderMap<TPoint> _map;

    public PathFinder(IPathFinderMap<TPoint> map, IDistanceHeuristicProvider<TPoint> distanceHeuristicProvider)
    {
        _map = map;
        _distanceHeuristicProvider = distanceHeuristicProvider;
        _expansionLimit = int.MaxValue;
    }

    public AreaResult<TPoint> ExpandArea(TPoint start, float maxCost)
    {
        HashSet<TPoint> outside = new HashSet<TPoint>();
        Dictionary<TPoint, float> costs = new Dictionary<TPoint, float>();
        PriorityQueue<TPoint, float> open = new PriorityQueue<TPoint, float>();
        Dictionary<TPoint, TPoint> cameFrom = new Dictionary<TPoint, TPoint>();
        List<(TPoint Position, float Cost)> neighbors = new List<(TPoint Position, float Cost)>();

        costs[start] = 0;
        open.Enqueue(start, 0);

        while (open.TryDequeue(out TPoint evaluatedLocation, out float queuedCost))
        {
            float evaluatedLocationCost = costs[evaluatedLocation];
            if (queuedCost > evaluatedLocationCost)
            {
                continue;
            }

            if (evaluatedLocationCost > maxCost)
            {
                outside.Add(evaluatedLocation);
                continue;
            }

            neighbors.Clear();
            _map.ExpandPosition(evaluatedLocation, neighbors);

            foreach ((TPoint neighborLocation, float neighborCost) in neighbors)
            {
                float neighborFinalCost = evaluatedLocationCost + neighborCost;
                if (neighborFinalCost > maxCost)
                {
                    outside.Add(neighborLocation);
                    continue;
                }

                if (costs.TryGetValue(neighborLocation, out float existingLocationCost)
                    && existingLocationCost <= neighborFinalCost)
                {
                    continue;
                }

                costs[neighborLocation] = neighborFinalCost;
                cameFrom[neighborLocation] = evaluatedLocation;
                open.Enqueue(neighborLocation, neighborFinalCost);
                outside.Remove(neighborLocation);
            }
        }

        List<AreaEdge<TPoint>> edges = new List<AreaEdge<TPoint>>();
        foreach (TPoint outsidePosition in outside)
        {
            if (cameFrom.ContainsKey(outsidePosition) || outsidePosition.Equals(start))
            {
                continue;
            }

            neighbors.Clear();
            _map.ExpandPosition(outsidePosition, neighbors);
            foreach ((TPoint neighborLocation, float _) in neighbors)
            {
                if (cameFrom.ContainsKey(neighborLocation))
                {
                    edges.Add(new AreaEdge<TPoint>(neighborLocation, outsidePosition));
                }
            }
        }

        return new AreaResult<TPoint>(cameFrom, costs, edges);
    }

    public PathResult FindPath(TPoint start, TPoint destination, List<(TPoint, float)> result)
    {
        int expansionsCount = 0;
        PriorityQueue<TPoint, float> open = new PriorityQueue<TPoint, float>();
        HashSet<TPoint> closed = new HashSet<TPoint>();
        Dictionary<TPoint, TPoint> cameFrom = new Dictionary<TPoint, TPoint>();
        Dictionary<TPoint, float> costs = new Dictionary<TPoint, float>();
        List<(TPoint Position, float Cost)> neighbors = new List<(TPoint Position, float Cost)>();

        costs[start] = 0;
        open.Enqueue(start, _distanceHeuristicProvider.GetCost(start, destination));

        while (open.TryDequeue(out TPoint current, out float potentialCost))
        {
            if (closed.Contains(current))
            {
                continue;
            }

            expansionsCount++;
            if (expansionsCount >= _expansionLimit)
            {
                return PathResult.ExpansionLimitExceeded;
            }

            if (float.IsInfinity(potentialCost))
            {
                closed.Add(current);
                continue;
            }

            if (current.Equals(destination))
            {
                Reconstruct(cameFrom, costs, current, result);
                return PathResult.Found;
            }

            closed.Add(current);
            neighbors.Clear();
            _map.ExpandPosition(current, neighbors);

            foreach ((TPoint neighborLocation, float neighborCost) in neighbors)
            {
                if (closed.Contains(neighborLocation))
                {
                    continue;
                }

                float cost = costs[current] + neighborCost;
                if (costs.TryGetValue(neighborLocation, out float existingCost) && cost >= existingCost)
                {
                    continue;
                }

                cameFrom[neighborLocation] = current;
                costs[neighborLocation] = cost;
                float potentialNeighborCost =
                    cost + _distanceHeuristicProvider.GetCost(neighborLocation, destination);
                open.Enqueue(neighborLocation, potentialNeighborCost);
            }
        }

        return PathResult.NotFound;
    }

    private static void Reconstruct(
        IDictionary<TPoint, TPoint> cameFrom,
        Dictionary<TPoint, float> costs,
        TPoint current,
        List<(TPoint, float)> result)
    {
        while (cameFrom.ContainsKey(current))
        {
            float cost = costs[current];
            result.Add((current, cost));
            current = cameFrom[current];
        }

        result.Reverse();
    }
}
