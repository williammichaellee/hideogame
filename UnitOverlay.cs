using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public partial class UnitOverlay : TileMapLayer
{
    /// <summary>
    /// Fills the tilemap with the given cells, visually representing where a unit can walk.
    /// </summary>
    /// <param name="cells">A list of walkable cell positions.</param>
    public void Draw(IEnumerable<Vector2I> cells, IEnumerable<Vector2I> allies)
    {
        Clear();
        var cellsEnum = cells.GetEnumerator();
        var alliesEnum = allies.GetEnumerator();
        // Represents where a unit can walk
        while (cellsEnum.MoveNext())
        {
            SetCell(cellsEnum.Current, 0, new Vector2I(0, 0));
        }
        // Represents tiles occupied by allied units
        while (alliesEnum.MoveNext())
        {
            if (cells.Contains(alliesEnum.Current))
            {
                SetCell(alliesEnum.Current, 1, new Vector2I(0, 0));
            }
        }
    }
}
