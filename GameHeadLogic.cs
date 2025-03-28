using Godot;
using System;

public partial class GameHeadLogic : Node {
	private UIPauseController pauseMenu;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready () {
		pauseMenu = GetNode<UIPauseController>( "Main/UI/Pause" );
	}

	public override void _Input ( InputEvent @event ) {
		if ( @event.IsActionPressed( "ui_cancel" ) ) {
			pauseMenu.TogglePause();
		}
	}
}
