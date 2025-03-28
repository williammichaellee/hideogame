using Godot;
using System;

public partial class UIPauseController : Control {
	private Control pauseMenu;
	
	private void Ready () {
		pauseMenu = GetNode<Control>( "Pause" );
		pauseMenu.visible = false;
	}
	
	private void Pause () {
		// this pauses the game
		GetTree().Paused = true;
		pauseMenu.visible = true;
	}
	
	private void Quit () {
		
	}
}
