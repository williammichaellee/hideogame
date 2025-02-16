using Godot;
using System;

public partial class TestTileMapLayer : Node2D
{

	// Hardcoded for now, there may or may not be a way to automatically derive this from the TileMapLayer props
	private static int tileSideLength = 32;
	private Sprite2D cursorSprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cursorSprite = GetNode<Sprite2D>("CursorSprite");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private Vector2I MousePositionToTilePosition(InputEventMouse inputEvent)
	{
		Vector2I rawCoords = (Vector2I)inputEvent.Position;
		Vector2I tileCoords = rawCoords / tileSideLength;
		return tileCoords;
	}

	// Gets *center* of tile, in pixel coordinates
	private Vector2 TilePositionToPixelPosition(Vector2I tilePosition)
	{
		Vector2 pixelCoords = tilePosition * tileSideLength;
		Vector2 pixelOffset = new Vector2(tileSideLength/2, tileSideLength/2);
		pixelCoords += pixelOffset;
		return pixelCoords;
	}

	// public override void _Input(InputEvent @event)
	// {
	// 	// Mouse in viewport coordinates.
	// 	if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left) {
	// 		GD.Print("Mouse Click at: ", eventMouseButton.Position);
	// 		var tileCoords = MousePositionToTilePosition(eventMouseButton);
	// 		GD.Print("Tile coordinate is: ", tileCoords);
	// 		cursorSprite.Position = TilePositionToPixelPosition(tileCoords);
	// 	}
	// }
}
