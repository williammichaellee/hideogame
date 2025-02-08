using Godot;
using System;
using System.Collections.Generic;

public partial class UnitOverlay : TileMapLayer
{
    /// <summary>
    /// Fills the tilemap with the given cells, visually representing where a unit can walk.
    /// </summary>
    /// <param name="cells">A list of walkable cell positions.</param>
    public void Draw(IEnumerable<Vector2I> cells)
    {
        Clear();
        foreach (Vector2I cell in cells)
        {
            SetCell(cell, 0, new Vector2I(0, 0), 0);
            GD.Print("e ", cell);
        }
    }
}
