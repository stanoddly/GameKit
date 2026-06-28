using BenchmarkDotNet.Attributes;
using GameKit.AStar;

namespace GameKit.AStar.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class PathFinderBenchmarks
{
    private static readonly GridPoint Center = new GridPoint(128, 128);
    private readonly PathFinder<GridPoint> _pathFinder =
        new PathFinder<GridPoint>(new RepresentativeTerrainMap(256, 256), new OctileHeuristic());
    private readonly List<(GridPoint Position, float Cost)> _path =
        new List<(GridPoint Position, float Cost)>();

    [Benchmark]
    public AreaResult<GridPoint> NormalTurnRange()
    {
        return _pathFinder.ExpandArea(Center, 60);
    }

    [Benchmark]
    public PathResult LongPath()
    {
        _path.Clear();
        return _pathFinder.FindPath(new GridPoint(8, 8), new GridPoint(247, 247), _path);
    }

    [Benchmark]
    public AreaResult<GridPoint> BroadAreaExpansion()
    {
        return _pathFinder.ExpandArea(Center, 1_000);
    }
}

public readonly record struct GridPoint(int X, int Y);

internal sealed class RepresentativeTerrainMap : IPathFinderMap<GridPoint>
{
    private static readonly (int Dx, int Dy, float Cost)[] Neighbors =
    [
        (-1, -1, 14), (0, -1, 10), (1, -1, 14),
        (-1, 0, 10),                   (1, 0, 10),
        (-1, 1, 14),  (0, 1, 10),    (1, 1, 14)
    ];

    private readonly int _height;
    private readonly int _width;

    public RepresentativeTerrainMap(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void ExpandPosition(
        GridPoint origin,
        ICollection<(GridPoint Position, float Cost)> neighbors)
    {
        for (int i = 0; i < Neighbors.Length; i++)
        {
            (int dx, int dy, float cost) = Neighbors[i];
            int x = origin.X + dx;
            int y = origin.Y + dy;
            if (x < 0 || y < 0 || x >= _width || y >= _height || IsBlocked(x, y))
            {
                continue;
            }

            float terrainMultiplier = y % 32 == 8 ? 0.5f : 1f;
            neighbors.Add((new GridPoint(x, y), cost * terrainMultiplier));
        }
    }

    private static bool IsBlocked(int x, int y)
    {
        return x % 32 == 16 && y % 32 != 8;
    }
}

internal sealed class OctileHeuristic : IDistanceHeuristicProvider<GridPoint>
{
    public float GetCost(GridPoint start, GridPoint destination)
    {
        int dx = Math.Abs(destination.X - start.X);
        int dy = Math.Abs(destination.Y - start.Y);
        int diagonal = Math.Min(dx, dy);
        int cardinal = Math.Max(dx, dy) - diagonal;
        return (diagonal * 14 + cardinal * 10) * 0.5f;
    }
}
