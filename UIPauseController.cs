using Godot;
using System;

public partial class UIPauseController : Control {
	private bool paused = false;
	
	public override void _Ready () {
		Hide();
	}
	
	public void TogglePause () {
		paused = !paused;
		GetTree().Paused = paused;
		if ( paused )
			Show();
		else
			Hide();
	}
	
	public void OnExitBtnPressed () {
		GetTree().Quit();
	}
}
