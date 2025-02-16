using Godot;
using Godot.NativeInterop;
using System;
using System.IO;
using System.Runtime.CompilerServices;

// Represents a unit on the game board.
// The board manages the Unit's position inside the game grid.
// The unit itself is only a visual representation that moves smoothly in the game world.
// We use the tool mode so the `skin` and `skin_offset` below update in the editor.
[Tool]
public partial class Unit : Path2D
{
    // Emitted when the unit reached the end of a path along which it was walking.
    // Used to notify the game board that a unit reached its destination
    [Signal]
    public delegate void WalkFinishedEventHandler();

    // Preloads 'grass.tres' resource
    [Export] public Resource Grid { get; set; } = GD.Load<Resource>("res://Grass.tres");
    // Texture representing the unit, can be reassigned instantly in inspector
    [Export] public Texture Skin { get => _skin; set => SetSkin(value); }
    // Distance to which the unit can walk to in cells
    [Export] public int MoveRange { get; set; } = 2;
    // Offsets skin so that its sprite aligns with its shadow
    [Export] public Vector2 SkinOffset { get => _skinOffset; set => SetSkinOffset(value); }
    // Unit's move speed in pixels
    [Export] public float MoveSpeed { get; set; } = 600.0f;
    [Export] private Sprite2D _sprite;
    [Export] private AnimationPlayer _animPlayer;
    [Export] private PathFollow2D _pathFollow;

    // Coordinates of the grid's cell the unit is on.
    private Vector2I _cell = Vector2I.Zero;
    public Vector2I Cell
    {
        get => _cell;
        set => SetCell(value);
    }

    // Toggles the selected animation on the unit
    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetIsSelected(value);
    }

    // Toggles processing for this unit through its setter function
    // See 'SetIsWalking'
    private bool _isWalking = false;
    private bool IsWalking
    {
        get => _isWalking;
        set => SetIsWalking(value);
    }

    private Texture _skin;
    private Vector2 _skinOffset = Vector2.Zero;

    public override void _Ready()
    // Use the 'process' callback to move the unit along a path
    // Unless it has a path to walk, don't update it every frame
    // See WalkAlong
    {
        // Stops Node from processing
        SetProcess(false);

        // Initializes the 'cell' property and snap the unit to the cell's center on the map
        Cell = ((dynamic)Grid).CalculateGridCoordinates(Position);
        Position = ((dynamic)Grid).CalculateMapPosition(Cell);

        if (!Engine.IsEditorHint())
        {
            // Creates curve resource
            // Creating it in editor prevents us from moving unit
            Curve = new Curve2D();
        }
        // Temp instructions for moving the unit
        // Vector2I[] _points = {
		// new(2, 2),
		// new(2, 5),
		// new(8, 5),
		// new(8, 7)
        // };
        // WalkAlong(_points);
    }

    // When 'active,' moves the unit along its 'curve' with the help of the PathFollow2D node
    public override void _Process(double delta)
    {
        // Every frame, the 'PathFollow2D.offset' property moves the sprites along the 'curve'
        // It moves an exact number of pixels, taking turns into account
        _pathFollow.Progress += MoveSpeed * (float)delta;

        // When we increase the 'offset' above, the 'ProgressRatio' also updates
        // This represents how far you are along the curve in percent
        // Unit stops moving when it reaches end (ProgressRatio equals '1.0')
        if (_pathFollow.ProgressRatio >= 1.0f)
        {
            // Setting IsWalking to false also turns off processing
            IsWalking = false;
            // Reset Progress to 0.0 which snaps the sprites back to the Unit node's position
            // Position the node to the center of the target grid cell and clear the curve
            // In the process loop, we only move the sprite but not the unit
            // The following lines move the unit in a way that's transparent to the player
            _pathFollow.Progress = 0.0f;
            Position = ((dynamic)Grid).CalculateMapPosition(Cell);
            Curve.ClearPoints();
            // Emit a signal used by the game board
            EmitSignal(nameof(WalkFinished));
        }
    }

    // Starts walking along the 'path'
    // 'path' is an array of grid coordinates that the function converts to map coordinates
    public void WalkAlong(Vector2I[] path)
    {
        if (path.Length == 0)
        {
            return;
        }

        // Converts the 'path' to points on the 'curve'
        // Property comes from the Path2D class the Unit extends
        Curve.AddPoint(Vector2I.Zero);
        foreach (Vector2I point in path)
        {
            Curve.AddPoint(((dynamic)Grid).CalculateMapPosition(point) - Position);
        }
        // Instantly change the unit's cell to the target position
        // We have the coordinates provided by the 'path' argument
        // The cell itself represents the grid coordinates the unit will stand on
        Cell = path[path.Length - 1];
        // The 'IsWalking' property triggers the move animation and turns on '_Process;'
        // See SetIsWalking below
        IsWalking = true;
    }

    // When changing the cell's value, we don't want coords out of the grid, so we clamp them
    private void SetCell(Vector2I value)
    {
        Vector2 vec = new Vector2(value.X, value.Y);
        Vector2 clampedvec = ((dynamic)Grid).Clamp(value);
        _cell = new Vector2I(
            Mathf.RoundToInt(clampedvec.X), 
            Mathf.RoundToInt(clampedvec.Y)
        );
    }

    // The _isSelected property toggles playback of the "selected" animation
    private void SetIsSelected(bool value)
    {
        _isSelected = value;
        if (_isSelected)
        {
            _animPlayer.Play("selected");
        }
        else
        {
            _animPlayer.Play("idle");
        }
    }

    // Both setters below manipulate the unit's Sprite node
    // Here, we update the sprite's texture:
    private void SetSkin(Texture value)
    {
        _skin = value;
        // Setter functions are called during the node's _Init() callback before they entered the tree
        // At that time, the _sprite variable is null
        // If so, we have to wait to update the sprite's properties
        if (_sprite == null)
        {
            // Allows us to wait until the node's _rReady() callback ended
            CallDeferred(nameof(UpdateSpriteTexture));
        }
        else
        {
            _sprite.Texture = (Texture2D)value;
        }
    }

    private void SetSkinOffset(Vector2 value)
    {
        _skinOffset = value;
        if (_sprite == null)
        {
            CallDeferred(nameof(UpdateSpritePosition));
        }
        else
        {
            _sprite.Position = value;
        }
    }

    private void SetIsWalking(bool value)
    {
        _isWalking = value;
        SetProcess(_isWalking);
    }

    private void UpdateSpriteTexture()
    {
        if (_sprite != null)
        {
            _sprite.Texture = (Texture2D)_skin;
        }
    }

    private void UpdateSpritePosition()
    {
        if (_sprite != null)
        {
            _sprite.Position = _skinOffset;
        }
    }
}