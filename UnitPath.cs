using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

public partial class UnitPath : TileMapLayer
{
    // The grid resource, which defines the grid size and cell properties.
    [Export]
    public Grass Grid { get; set; }

    // PathFinder instance for finding paths.
    private PathFinder _pathfinder;

    // The current path being drawn as a packed array of Vector2 coordinates.
    private Array<Vector2I> _currentPath = new Array<Vector2I>();

    public override void _Ready() {
        // These two points define the start and end of a rectangle of cells.
        Vector2I rectStart = new Vector2I(4, 4);
        Vector2I rectEnd = new Vector2I(10, 8);

        // The following lines generate a list of points filling the rectangle 
        // from rectStart to rectEnd.
        var points = new List<Vector2>();

        // In a for loop, range-based iteration can be done with a standard range loop.
        for (int x = 0; x <= rectEnd.X - rectStart.X; x++)
        {
            for (int y = 0; y <= rectEnd.Y - rectStart.Y; y++)
            {
                points.Add(rectStart + new Vector2(x, y));
            }
        }

        // Use the points to initialize the PathFinder and draw a path.
        Initialize(points);
        Draw(rectStart, new Vector2I(8, 7));
    }

    /// <summary>
    /// Creates a new PathFinder that uses the AStar algorithm to find a path 
    /// between two cells among the `walkableCells`.
    /// </summary>
    /// <param name="walkableCells">The list of walkable cells on the grid.</param>
    public void Initialize(ICollection<Vector2> walkableCells)
    {
        _pathfinder = new PathFinder(Grid, walkableCells);
    }

    /// <summary>
    /// Finds and draws the path between `cellStart` and `cellEnd`.
    /// </summary>
    /// <param name="cellStart">The starting cell.</param>
    /// <param name="cellEnd">The ending cell.</param>
    public new void Draw(Vector2I cellStart, Vector2I cellEnd)
    {
        base._Draw();
        Clear(); // Clears the current tiles.
        _currentPath = _pathfinder.CalculatePointPath(cellStart, cellEnd);
        SetCellsTerrainConnect(_currentPath, 0, 0, false);
    }

    /// <summary>
    /// Stops drawing by clearing the drawn path and resetting the PathFinder.
    /// </summary>
    public void Stop()
    {
        _pathfinder = null; // Clear the PathFinder instance.
        Clear(); // Clears the current tiles.
    }
}
