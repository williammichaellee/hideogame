using Godot;
using System;

[Tool]
public partial class Grass : Resource
{
    // The grid's size in rows and columns.
    [Export]
    public Vector2 Size = new Vector2(20, 20);

    // The size of a cell in pixels.
    [Export]
    public Vector2 CellSize = new Vector2(32, 32);

    // Half of `CellSize`.
    // Used to calculate the center of a grid cell in pixels on the screen.
    private Vector2 HalfCellSize => CellSize / 2;

    // Returns the position of a cell's center in pixels.
    public Vector2 CalculateMapPosition(Vector2 gridPosition)
    {
        return gridPosition * CellSize + HalfCellSize;
    }

    // Returns the coordinates of the cell on the grid given a position on the map.
    public Vector2I CalculateGridCoordinates(Vector2 mapPosition)
    {
        return (Vector2I)((mapPosition / CellSize).Floor());
    }

    // Returns true if the `cellCoordinates` are within the grid.
    public bool IsWithinBounds(Vector2 cellCoordinates)
    {
        bool isInBoundsX = cellCoordinates.X >= 0 && cellCoordinates.X < Size.X;
        bool isInBoundsY = cellCoordinates.Y >= 0 && cellCoordinates.Y < Size.Y;
        return isInBoundsX && isInBoundsY;
    }

    // Makes the `gridPosition` fit within the grid's bounds.
    public Vector2 Clamp(Vector2 gridPosition)
    {
        return new Vector2(
            Mathf.Clamp(gridPosition.X, 0, Size.X - 1),
            Mathf.Clamp(gridPosition.Y, 0, Size.Y - 1)
        );
    }

    // Given Vector2 coordinates, calculates and returns the corresponding integer index.
    public int AsIndex(Vector2 cell)
    {
        return (int)(cell.X + Size.X * cell.Y);
    }
}