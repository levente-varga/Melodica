using Godot;
using System;

public partial class PlayMenu : Node2D
{
	Button bPlay;

	public override void _Ready()
	{
		GetNodes();
	}

	private void GetNodes() {
		bPlay = GetNode<Button>("UI/bQuickPlay");
	}

	public override void _Process(double delta)
	{
	}

	void OnPlayPressed() {
		GetTree().ChangeSceneToFile("res://Scenes/Game");
	}
}
