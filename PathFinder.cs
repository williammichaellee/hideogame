using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// Finds the path between two points among walkable cells using the AStar pathfinding algorithm
[Tool]
public partial class PathFinder : Resource
{
    // Directions in which a unit can move: up, down, left, right
    private static readonly Vector2[] DIRECTIONS = { Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down };

    // Reference to the grid resource
    private Grass _grid;

    // AStarGrid2D instance for pathfinding
    private AStarGrid2D _astar = new AStarGrid2D();

    // Initializes the AStarGrid2D object upon creation
    // 'grid' resource contains size and cell size information
    // walkableCells is an array of walkable cells represented as Vector2 coordinates
    public PathFinder(Grass grid, ICollection<Vector2> walkableCells)
    {
        _grid = grid;
        _astar.Region = new Rect2I(Vector2I.Zero, new Vector2I((int)_grid.Size.X, (int)_grid.Size.Y));
        _astar.CellSize = _grid.CellSize;
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
        _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan;
        _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;

        // Update the AStarGrid2D configuration
        _astar.Update();

        // Iterate over all points on the grid and disable any that are not in array of walkable cells
        for (int y = 0; y < _grid.Size.Y; y++)
        {
            for (int x = 0; x < _grid.Size.X; x++)
            {
                Vector2I point = new Vector2I(x, y);
                if (!walkableCells.Contains(point))
                {
                    _astar.SetPointSolid(point, true);
                }
            }
        }
    }

    // Returns the path found between start and end as an array of Vector2 coordinates
    public Array<Vector2I> CalculatePointPath(Vector2I start, Vector2I end)
    {
        // Use the AStarGrid2D's GetIdPath method to compute the path
        // We only need to call GetIdPath to return the array
        return _astar.GetIdPath(start, end);
    }
}
