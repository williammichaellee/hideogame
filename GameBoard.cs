using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameBoard : Node2D
{
	 private static readonly Vector2I[] DIRECTIONS = { Vector2I.Left, Vector2I.Right, Vector2I.Up, Vector2I.Down };

	// Resource of type Grid
	[Export] public Grass Grid { get; set; }

	// Mapping of cell coordinates to the units they contain
	private Dictionary<Vector2I, Unit> _units = new();
	private Unit _activeUnit;
	private List<Vector2I> _walkableCells = new();

	private UnitOverlay _unitOverlay;
	private UnitPath _unitPath;
	private TileMapLayer _baseTileMapLayer;

	public override void _Ready()
	{
		_unitOverlay = GetNode<UnitOverlay>("UnitOverlay");
		_unitPath = GetNode<UnitPath>("UnitPath");
		_baseTileMapLayer = GetNode<TileMapLayer>("BaseTileMap");
		Reinitialize();
		var asString = string.Join(System.Environment.NewLine, _units);
		foreach (var unit in _units)
			_unitOverlay.Draw(GetWalkableCells(unit.Value));
		
		var cursor = GetNode<Cursor>("Cursor");
		cursor.AcceptPressed += OnCursorAcceptPressed;
		cursor.Moved += OnCursorMoved;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_activeUnit != null && @event.IsActionPressed("ui_cancel"))
		{
			DeselectActiveUnit();
			ClearActiveUnit();
		}
	}

	public string GetConfigurationWarning()
	{
		if (Grid == null)
		{
			return "You need a Grid resource for this node to work.";
		}
		return string.Empty;
	}

	// Returns true if the cell is occupied by a unit
	public bool IsOccupied(Vector2I cell)
	{
		return _units.ContainsKey(cell);
	}

	// Returns an array of cells a given unit can walk using the flood fill algorithm
	public List<Vector2I> GetWalkableCells(Unit unit)
	{
		return FloodFillWalking(unit.Cell, unit.MoveRange);
	}

	// Clears and refills the _units dictionary with game objects on the board
	private void Reinitialize()
	{
		_units.Clear();
		foreach (Node child in GetChildren())
		{
			if (child is Unit unit)
			{
				_units[unit.Cell] = unit;
			}
		}
	}

	// Returns a list of all walkable cell coordinates based on max distance and tiles marked "walkable"
	private List<Vector2I> FloodFillWalking(Vector2I cell, int maxDistance)
	{
		var result = new List<Vector2I>();
		var cellQueue = new Queue<Vector2I>();
		var distQueue = new Queue<int>();
		cellQueue.Enqueue(cell);
		distQueue.Enqueue(0);

		while (cellQueue.Count != 0)
		{
			Vector2I coords = cellQueue.Dequeue();
			int dist = distQueue.Dequeue();

			if (!Grid.IsWithinBounds(coords)) continue;
			if (result.Contains(coords)) continue;
			if (dist > maxDistance) continue;

			var tileMapCell = _baseTileMapLayer.GetCellTileData(coords);
			if (tileMapCell == null) continue;
			if (!(bool)tileMapCell.GetCustomData("walkable")) continue;

			result.Add(coords);

			foreach (var direction in DIRECTIONS)
			{
				var neighbor = coords + direction;

				if (result.Contains(neighbor) || cellQueue.Contains(neighbor))
					continue;

				cellQueue.Enqueue(neighbor);
				distQueue.Enqueue(dist + 1);
			}
		}

		return result;
	}

	// Moves the active unit to the new cell if it's walkable and not occupied
	private async void MoveActiveUnit(Vector2I newCell)
	{
		if (IsOccupied(newCell) || !_walkableCells.Contains(newCell)) return;

		_units.Remove(_activeUnit.Cell);
		_units[newCell] = _activeUnit;

		DeselectActiveUnit();
		_activeUnit.WalkAlong(_unitPath.CurrentPath.Select(v => new Vector2I(v.X, v.Y)).ToArray());
		await ToSignal(_activeUnit, "WalkFinished");
		ClearActiveUnit();
	}

	// Selects the unit in the given cell, sets it as the active unit, and draws its walkable cells
	private void SelectUnit(Vector2I cell)
	{
		if (!_units.ContainsKey(cell)) return;

		_activeUnit = _units[cell];
		_activeUnit.IsSelected = true;
		_walkableCells = GetWalkableCells(_activeUnit);

		_unitOverlay.Draw(_walkableCells.Select(v => new Vector2I((int)v.X, (int)v.Y)).ToList());
		_unitPath.Initialize(_walkableCells);
	}

	// Deselects the active unit and clears overlays and path drawings
	private void DeselectActiveUnit()
	{
		_activeUnit.IsSelected = false;
		_unitOverlay.Clear();
		_unitPath.Stop();
	}

	// Clears the reference to the active unit and its walkable cells
	private void ClearActiveUnit()
	{
		_activeUnit = null;
		_walkableCells.Clear();
	}



	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left) {

		}
	}

	// Handles cursor press: either selects or moves a unit
	private void OnCursorAcceptPressed(Vector2I cell)
	{
		// GD.Print($"OnCursorAcceptPressed {cell}");
		if (_activeUnit == null)
		{
			SelectUnit(cell);
		}
		// If you click on the cell again, go through the deselect process the same as if "ui_cancel" is pressed
		// GetValueOrDefault is there to prevent key errors when cell does not have a unit on it
		// (and therefore does not have a corresponding value in the _units dictionary)
		else if (_units.GetValueOrDefault(cell) == _activeUnit) {
			DeselectActiveUnit();
			ClearActiveUnit();
		}
		else if (_activeUnit.IsSelected)
		{
			MoveActiveUnit(cell);
		}
	}

	// Updates the interactive path drawing if there's an active and selected unit
	private void OnCursorMoved(Vector2I newCell)
	{
		if (_activeUnit != null && _activeUnit.IsSelected)
		{
			_unitPath.Draw((Vector2I)_activeUnit.Cell, (Vector2I)newCell);
		}
	}
}
