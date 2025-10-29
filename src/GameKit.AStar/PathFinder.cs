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
    List<(TPoint position, float cost)> ExpandPosition(TPoint origin);
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
    //bool TryEvaluatePathCost(IEnumerable<TPoint> path, out float cost);
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
        //_distanceHeuristicProvider = new ChebychevDistanceHeuristicProvider();
        _expansionLimit = int.MaxValue;
    }

    public AreaResult<TPoint> ExpandArea(TPoint start, float maxCost)
    {
        //List<AreaEdge> edges = new();
        HashSet<TPoint> outside = new();
        Dictionary<TPoint, float> costs = new();
        HashSet<TPoint> open = new();
        Dictionary<TPoint, TPoint> cameFrom = new();

        costs[start] = 0;

        open.Add(start);

        while (open.Count > 0)
        {
            var evaluatedLocation = open.First();
            open.Remove(evaluatedLocation);
            var evaluatedLocationCost = costs[evaluatedLocation];

            if (evaluatedLocationCost > maxCost)
            {
                outside.Add(evaluatedLocation);
                continue;
            }

            /*bool wouldNeigborsBeExpensiveAnyway = (evaluatedLocationCost + _map.MinimalCost) > maxCost;
            if (wouldNeigborsBeExpensiveAnyway)
            {
                continue;
            }*/

            List<(TPoint Location, float Cost)> neighbors = _map.ExpandPosition(evaluatedLocation);
            if (neighbors.Count == 0) continue;

            foreach ((var neighborLocation, var neighborCost) in neighbors)
            {
                var neighborFinalCost = evaluatedLocationCost + neighborCost;
                if (!costs.TryGetValue(neighborLocation, out var existingLocationCost))
                    existingLocationCost = float.PositiveInfinity;

                var isTooExpensive = neighborFinalCost > maxCost;
                if (isTooExpensive)
                {
                    outside.Add(neighborLocation);
                    continue;
                }

                var isMoreExpensiveThanPreviousFrom = existingLocationCost <= neighborFinalCost;
                if (isMoreExpensiveThanPreviousFrom) continue;

                costs[neighborLocation] = neighborFinalCost;
                cameFrom[neighborLocation] = evaluatedLocation;
                open.Add(neighborLocation);
                outside.Remove(neighborLocation);
            }
        }

        List<AreaEdge<TPoint>> edges = new();
        foreach (var outsidePosition in outside)
        {
            if (cameFrom.ContainsKey(outsidePosition) || outsidePosition.Equals(start)) continue;

            List<(TPoint Location, float Cost)> neighbors = _map.ExpandPosition(outsidePosition);
            if (neighbors.Count != 0)
                foreach ((var neighborLocation, var _) in neighbors)
                    if (cameFrom.ContainsKey(neighborLocation))
                        edges.Add(new AreaEdge<TPoint> { Inside = neighborLocation, Outside = outsidePosition });
        }

        /*
        if (edges.Count > 0)
        {
            for (int i = edges.Count - 1; i >= 0; i--)
            {
                if (cameFrom.ContainsKey(edges[i].Outside))
                {
                    edges[i] = edges[^1];
                    edges.RemoveAt(edges.Count - 1);
                }
            }
        }
        */

        return new AreaResult<TPoint>(cameFrom, costs, edges);
    }

    private static (TPoint, float) FindPointWithLowCost(Dictionary<TPoint, float> potentialCosts)
    {
        float? bestCost = null;
        var bestPoint = default(TPoint);

        foreach ((var evaluatedPoint, var evaluatedCost) in potentialCosts)
            if (!bestCost.HasValue || evaluatedCost < bestCost.Value)
            {
                bestCost = evaluatedCost;
                bestPoint = evaluatedPoint;
            }

        // it can't be null because potentialCosts always contains a value
        return (bestPoint, bestCost!.Value);
    }

    private void Reconstruct(IDictionary<TPoint, TPoint> cameFrom, Dictionary<TPoint, float> costs, TPoint current,
        List<(TPoint, float)> result)
    {
        // skip the last one
        //if (_loose == true)
        //{
        //    current = cameFrom[current];
        //}

        while (cameFrom.ContainsKey(current))
        {
            var cost = costs[current];
            result.Add((current, cost));
            current = cameFrom[current];
        }

        // TODO: improve
        result.Reverse();
    }

    public PathResult FindPath(TPoint start, TPoint destination, List<(TPoint, float)> result)
    {
        var expansionsCount = 0;
        var neighbors = new List<(TPoint, float)>();
        // TODO: perhaps open and potentialCosts could be merged
        var open = new HashSet<TPoint>();
        var potentialCosts = new Dictionary<TPoint, float>();
        var closed = new HashSet<TPoint>();
        var cameFrom = new Dictionary<TPoint, TPoint>();
        var costs = new Dictionary<TPoint, float>();

        costs[start] = 0;

        var cost = _distanceHeuristicProvider.GetCost(start, destination);
        open.Add(start);
        potentialCosts.Add(start, cost);

        while (open.Count > 0)
        {
            expansionsCount += 1;
            if (expansionsCount >= _expansionLimit) return PathResult.ExpansionLimitExceeded;
            //TODO: use something more suitable to get rid of O(n) complexity
            (var current, var potentialCost) = FindPointWithLowCost(potentialCosts);
            open.Remove(current);
            potentialCosts.Remove(current);

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
            neighbors = _map.ExpandPosition(current);
            if (neighbors.Count == 0) continue;

            foreach ((var neighborLocation, var neighborCost) in neighbors)
            {
                if (closed.Contains(neighborLocation))
                    continue;

                cost = costs[current] + neighborCost;

                if (!costs.ContainsKey(neighborLocation) || cost < costs[neighborLocation])
                {
                    cameFrom[neighborLocation] = current;
                    costs[neighborLocation] = cost;

                    // let's calculate potential cost
                    cost = cost + _distanceHeuristicProvider.GetCost(neighborLocation, destination);
                    if (!open.Contains(neighborLocation)) open.Add(neighborLocation);
                    potentialCosts[neighborLocation] = cost;
                }
            }
        }

        return PathResult.NotFound;
    }
}