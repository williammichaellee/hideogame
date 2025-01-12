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
    [Export] public Grass Grid { get; set; } = GD.Load<Grass>("res://Grass.tres");
    // Texture representing the unit, can be reassigned instantly in inspector
    [Export] public Texture Skin { get => _skin; set => SetSkin(value); }
    // Distance to which the unit can walk to in cells
    [Export] public int MoveRange { get; set; } = 6;
    // Offsets skin so that its sprite aligns with its shadow
    [Export] public Vector2 SkinOffset { get => _skinOffset; set => SetSkinOffset(value); }
    // Unit's move speed in pixels
    [Export] public float MoveSpeed { get; set; } = 600.0f;

    // Coordinates of the grid's cell the unit is on.
    private Vector2 _cell = Vector2.Zero;
    public Vector2 Cell
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

    private Sprite2D _sprite;
    private AnimationPlayer _animPlayer;
    private PathFollow2D _pathFollow;

    public override void _Ready()
    // Use the 'process' callback to move the unit along a path
    // Unless it has a path to walk, don't update it every frame
    // See walk_along
    {
        // Stops Node from processing
        SetProcess(false);

        _sprite = GetNode<Sprite2D>("PathFollow2D/Sprite");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _pathFollow = GetNode<PathFollow2D>("PathFollow2D");

        // Initializes the 'cell' property and snap the unit to the cell's center on the map
        Cell = Grid.CalculateGridCoordinates(Position);
        Position = Grid.CalculateMapPosition(Cell);

        if (!Engine.IsEditorHint())
        {
            // Creates curve resource
            // Creating it in editor prevents us from moving unit
            Curve = new Curve2D();
        }

        Vector2[] _points = {
		new Vector2(2, 2),
		new Vector2(2, 5),
		new Vector2(8, 5),
		new Vector2(8, 7)
        };
        WalkAlong(_points);
    }

    public override void _Process(double delta)
    {
        _pathFollow.Progress += MoveSpeed * (float)delta;

        if (_pathFollow.ProgressRatio >= 1.0f)
        {
            IsWalking = false;
            _pathFollow.Progress = 0.0f;
            Position = ((dynamic)Grid).CalculateMapPosition(Cell);
            Curve.ClearPoints();
            EmitSignal(nameof(WalkFinished));
        }
    }

    public void WalkAlong(Vector2[] path)
    {
        if (path.Length == 0)
        {
            return;
        }

        Curve.AddPoint(Vector2.Zero);
        foreach (Vector2 point in path)
        {
            Curve.AddPoint(((dynamic)Grid).CalculateMapPosition(point) - Position);
        }
        Cell = path[path.Length - 1];
        IsWalking = true;
    }

    private void SetCell(Vector2 value)
    {
        _cell = ((dynamic)Grid).Clamp(value);
    }

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

    private void SetSkin(Texture value)
    {
        _skin = value;
        if (_sprite == null)
        {
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