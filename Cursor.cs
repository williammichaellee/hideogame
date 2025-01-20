using Godot;
using System;

// Player-controlled cursor, allows them to navigate the game grid, select units, and move them
// Supports both keyboard and mouse (or touch) input
// The `tool` mode allows us to preview the drawing code below in the editor
[Tool]
public partial class Cursor : Node2D
{
    // Uses signals to keep the cursor decoupled from other nodes
    // Emits a signal when the player moves the cursor or wants to interacts with a cell
    // And let another node handle the interaction

    // Signal emitted when the user interacts with the currently hovered cell
    [Signal]
    public delegate void AcceptPressedEventHandler(Vector2 cell);
    // Emitted when the cursor moved to a new cell
    [Signal]
    public delegate void MovedEventHandler(Vector2 newCell);

    // Grid resource, giving the node access to the grid size, and more
    [Export] public Resource Grid { get; set; } = GD.Load<Resource>("res://Grass.tres");
    // Time before the cursor can move again in seconds
    [Export] public float UiCooldown { get; set; } = 0.1f;
    // Coordinates of the current cell the user is hovering
    private Vector2 _cell = Vector2.Zero;
    public Vector2 Cell
    {
        get => _cell;
        set => SetCell(value);
    }

    // Timer for cursor movement cooldown
    private Timer _timer;

    // When the cursor enters the scene tree, we snap its position to the center of the cell
    // And initialize the timer with UiCooldown
    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = UiCooldown;
        Position = ((dynamic)Grid).CalculateMapPosition(Cell);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // If the user moves the mouse, we capture that input and update the node's cell in priority
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            Cell = ((dynamic)Grid).CalculateGridCoordinates(mouseMotionEvent.Position);
        }
        // If we are already hovering the cell and click on it, or we press the enter key
        // Then the player wants to interact with that cell
        else if (@event.IsActionPressed("click") || @event.IsActionPressed("ui_accept"))
        {
            // In that case, we emit a signal to let another node handle that input
            // The game board will have the responsibility of looking at the cell's content
            EmitSignal(nameof(AcceptPressed), Cell);
            GetViewport().SetInputAsHandled();
        }

        // The code below is for the cursor's movement
        // The following lines make some preliminary checks to see whether the cursor should move
        // or not if the user presses an arrow key
        bool shouldMove = @event.IsPressed();
        // If the player is pressing the key in this frame, we allow the cursor to move
        // If they keep the key down, we only want to move after the cooldown timer stops
        if (@event.IsEcho())
        {
            shouldMove = shouldMove && _timer.IsStopped();
        }
        // And if the cursor shouldn't move, we prevent it from doing so
        if (!shouldMove)
        {
            return;
        }

        // We update the cursor's current cell based on the input direction
        // See SetCell function below to see what changes that triggers
        if (@event.IsAction("ui_right"))
        {
            Cell += Vector2.Right;
        }
        else if (@event.IsAction("ui_up"))
        {
            Cell += Vector2.Up;
        }
        else if (@event.IsAction("ui_left"))
        {
            Cell += Vector2.Left;
        }
        else if (@event.IsAction("ui_down"))
        {
            Cell += Vector2.Down;
        }
    }

    // We use the draw callback to a rectangular outline the size of a grid cell, width of 2 pixels
    public override void _Draw()
    {
        Rect2 rect = new Rect2(-((dynamic)Grid).CellSize / 2, ((dynamic)Grid).CellSize);
        DrawRect(rect, Colors.AliceBlue, false, 2.0f);
    }

    // This function controls the cursor's current position
    private void SetCell(Vector2 value)
    {
        // We first clamp the cell coordinates and ensure that we weren't trying to move outside grid boundaries
        Vector2 newCell = ((dynamic)Grid).Clamp(value);
        if (newCell.IsEqualApprox(Cell))
        {
            return;
        }

        _cell = newCell;
        // If we move to a new cell, we update the cursor's position, emit a signal, and start the cooldown timer
        // This will limit the rate at which the cursor moves when we keep the direction key down
        Position = ((dynamic)Grid).CalculateMapPosition(Cell);
        EmitSignal(nameof(Moved), Cell);
        _timer.Start();
    }
}